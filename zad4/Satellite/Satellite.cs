using System.Threading;
using Akka.Actor;

namespace zad4.Satellite {
    public record StatusRequest { }
    
    public record StatusResponse {
        public Status Status { get; }
        public int SatelliteId { get; }

        public StatusResponse(int satelliteId, Status status) {
            SatelliteId = satelliteId;
            Status = status;
        }
    }
    
    public class Satellite : ReceiveActor {

        public int Id { get; }
        public Satellite(int id) {
            Id = id;
            Receive<StatusRequest>(HandleStatusRequest);
        }

        public static Props Props(int id) =>
            Akka.Actor.Props.Create<Satellite>(id);
        
        private void HandleStatusRequest(StatusRequest request) {
            var status = SatelliteApi.GetStatus(Id);
            Sender.Tell(new StatusResponse(Id, status), Self);
        }
    }
}
