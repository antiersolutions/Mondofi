using AIS.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AIS.Hubs.FloorPlan
{
    [Authorize]
    [HubName("FloorPlanHub")]
    public class FloorPlanHub : Hub
    {
        private readonly AutoUpdateEndingReservation _autoupdateendingreservation;

        public FloorPlanHub() : this(AutoUpdateEndingReservation.Instance) { }

        public FloorPlanHub(AutoUpdateEndingReservation AutoUpdateEndingReservation)
        {
            _autoupdateendingreservation = AutoUpdateEndingReservation;
        }

        public override Task OnConnected()
        {
            var dataBaseName = Context.User.Identity.GetDatabaseName();
            Groups.Add(Context.ConnectionId, dataBaseName);

            return base.OnConnected();
        }
    }
}