using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using zad4.Satellite;
using zad4.Station;
using Status = zad4.Satellite.Status;

namespace zad4.Dispatcher {
    public class Dispatcher : ReceiveActor {

        private readonly Dictionary<int, IActorRef> _satellites;
        public Dispatcher(Dictionary<int, IActorRef> satellites) {
            _satellites = satellites;
            Receive<QueryRequest>(HandleQueryRequest);
        }

        public static Props Props(Dictionary<int, IActorRef> satellites) =>
            Akka.Actor.Props.Create<Dispatcher>(satellites);

        private static StatusRequest StatusRequest = new();
        private void HandleQueryRequest(QueryRequest request) {
            var responses = new Task<StatusResponse>[request.Range];
            for (var i = 0; i < request.Range; i++) {
                var cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(request.Timeout);
                responses[i] = _satellites[request.FirstSatelliteId + i].Ask<StatusResponse>(StatusRequest, cancellationTokenSource.Token);
            }

            var target = Sender;
            var oldSelf = Self;
            Utils.WhenAllFinished(responses).ContinueWith(status => {
                var errors = new Dictionary<int, Status>();
                
                var finished = (from x in status.Result where x.Finished select x.Value).ToArray();
                                   
                foreach (var response in finished) {
                    if (response.Status != Status.Ok)
                        errors[response.SatelliteId] = response.Status;
                }

                target.Tell(new QueryResponse {
                    QueryId = request.QueryId,
                    Errors = errors,
                    PercentFinished = (decimal) finished.Length / request.Range
                }, oldSelf);
            });
        }

    }
}