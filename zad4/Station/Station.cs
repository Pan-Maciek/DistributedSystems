using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Event;
using Status = zad4.Satellite.Status;

namespace zad4.Station {
    
    public record QueryRequest {
        public Guid QueryId { get; } = Guid.NewGuid();
        public int FirstSatelliteId { get; init; }
        public int Range { get; init; }
        public TimeSpan Timeout { get; init; } = TimeSpan.FromMilliseconds(300);
    }
    
    public class QueryResponse {
        public Guid QueryId { get; init; }
        public Dictionary<int, Status> Errors { get; init; }
        public decimal PercentFinished { get; init; }
    }
    
    public class Station : ReceiveActor {
        
        public string Name { get; }
        private readonly IActorRef _dispatcher;
        private readonly ILoggingAdapter _logger;

        public Station(string name, IActorRef dispatcher) {
            Name = name;
            _dispatcher = dispatcher;
            _logger = Context.GetLogger();
            Receive<QueryRequest>(HandleQueryRequest);
        }

        private void HandleQueryRequest(QueryRequest request) {
            var time1 = DateTime.Now;
            var response = _dispatcher.Ask<QueryResponse>(request);
            response.Wait();
            var responseTime = DateTime.Now - time1;
            _logger.Info($"{Name} Time: {responseTime.TotalMilliseconds}, Errors: {response.Result.Errors.Count} Responded: {response.Result.PercentFinished*100}%");
            foreach (var (id, status) in response.Result.Errors) 
                _logger.Error($"{id}, {status}");
        }

        public static Props Props(string name, IActorRef dispatcher) =>
            Akka.Actor.Props.Create<Station>(name, dispatcher);
    }
}