using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace AIS.Hubs.FloorPlan
{
    public class TestSignalRTimer
    {
        private readonly static Lazy<TestSignalRTimer> _instance = new Lazy<TestSignalRTimer>(() => new TestSignalRTimer());
        private readonly TimeSpan BroadcastInterval = TimeSpan.FromMinutes(1);
        private readonly IHubContext _hubContext;
        private Timer _broadcastLoop;

        public TestSignalRTimer()
        {
            // to send to its connected clients
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<FloorPlanHub>();

            // Start the broadcast loop
            _broadcastLoop = new Timer(
                BroadcastUpdate,
                null,
                BroadcastInterval - TimeSpan.FromSeconds(DateTime.UtcNow.Second),
                BroadcastInterval);
        }

        public void BroadcastUpdate(object state)
        {
            decimal progress = 0;
            _hubContext.Clients.All.startProgress();

            for (decimal i = 0; i < 10; i++)
            {
                _hubContext.Clients.All.updateEndingReservation(DateTime.UtcNow.ToLongDateString());

                progress = Convert.ToInt32(Math.Floor((i / 10) * 100));
                _hubContext.Clients.All.updateProgress(progress);
                Thread.Sleep(200);
            }

            _hubContext.Clients.All.updateProgress(100);
        }

        public static TestSignalRTimer Instance
        {
            get
            {
                return _instance.Value;
            }
        }
    }
}