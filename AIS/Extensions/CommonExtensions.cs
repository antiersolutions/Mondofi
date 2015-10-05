using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AIS.Extensions
{
    public static class CommonExtensions
    {
        private static Random random = new Random();

        public static int GetRandomNumber(int min, int max)
        {
            return random.Next(min, max);
        }
    }
}