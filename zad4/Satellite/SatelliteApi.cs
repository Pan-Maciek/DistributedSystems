using System;
using System.Threading;

namespace zad4.Satellite {
    public enum Status {
        Ok, BatteryLow, 
        PropulsionError, NavigationError
    }
    
    public static class SatelliteApi {

        public static Status GetStatus(int satelliteIndex){
            var rand = new Random();

            Thread.Sleep(100 + rand.Next(400));

            var p = rand.NextDouble();
            return p switch {
                < 0.8 => Status.Ok,
                < 0.9 => Status.BatteryLow,
                < 0.95 => Status.NavigationError,
                _ => Status.PropulsionError
            };
        }

    }
}