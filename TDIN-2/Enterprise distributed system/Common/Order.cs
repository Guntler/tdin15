using System;
using System.Runtime.Serialization;
using MongoDB.Bson;

namespace Common
{
    [DataContract]
    public class Order
    {
        [DataMember (Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "Title")]
        public string Title { get; set; }

        [DataMember(Name = "Quantity")]
        public int Quantity { get; set; }

        [DataMember(Name = "ClientId")]
        public string ClientId { get; set; }

        [DataMember(Name = "State")]
        public StateEnum State { get; set; }

        public Order(string title, int quantity, string clientId)
        {
            var random = new Random();
            var timestamp = DateTime.UtcNow;
            var machine = random.Next(0, 16777215);
            var pid = (short) random.Next(0, 32768);
            var increment = random.Next(0, 16777215);
            Id = new ObjectId(timestamp, machine, pid, increment).ToString();
            Title = title;
            Quantity = quantity;
            ClientId = clientId;
            State = new StateEnum();
        }

        public override string ToString()
        {
            return String.Format("Order Id: {0}, Title:{1}, Quantity:{2}, ClientId: {3}, {4}", Id, Title, Quantity, ClientId, State.ToString());
        }
    }

    [DataContract(Name = "State")]
    public class StateEnum
    {
        public StateEnum()
        {
            CurrentState = State.WaitingDispatch;
            Date = DateTime.Now;
        }
        public enum State
        {
            WaitingDispatch, Waiting, Dispatched
        }

        [DataMember(Name = "CurrentState")]
        public State CurrentState { get; set; }
        [DataMember(Name = "dateTime")]
        public DateTime Date { get; set; }

        public override string ToString()
        {
            return String.Format("State Current: {0}, Date:{1}", CurrentState, Date);
        }
    }

}
