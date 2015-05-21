using System;
using System.Runtime.Serialization;
using MongoDB.Bson;

namespace Common
{
    [DataContract]
    class Order
    {
        [DataMember]
        public ObjectId Id { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public int Quantity { get; set; }

        [DataMember]
        public ObjectId ClientId { get; set; }

        [DataMember]
        public StateEnum State { get; set; }

        public Order(string title, int quantity, ObjectId clientId)
        {
            var random = new Random();
            var timestamp = DateTime.UtcNow;
            var machine = random.Next(0, 16777215);
            var pid = (short) random.Next(0, 32768);
            var increment = random.Next(0, 16777215);
            Id = new ObjectId(timestamp, machine, pid, increment);
            Title = title;
            Quantity = quantity;
            ClientId = clientId;
            State = new StateEnum();
        }
    }

    [DataContract(Name = "State")]
    class StateEnum
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

    }

}
