using System;
using System.Runtime.Serialization;
using MongoDB.Bson;

namespace Common
{
    [DataContract]
    public class Message
    {

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "action")]
        public string Action { get; set; }

        [DataMember(Name = "book")]
        public Book Book { get; set; }

        [DataMember(Name = "Amount")]
        public int Amount { get; set; }

        public Message(string action, int quantity, Book book)
        {
            var random = new Random();
            var timestamp = DateTime.UtcNow;
            var machine = random.Next(0, 16777215);
            var pid = (short)random.Next(0, 32768);
            var increment = random.Next(0, 16777215);
            Id = new ObjectId(timestamp, machine, pid, increment).ToString();
            Action = action;
            Book = book;
            Amount = quantity;
        }

        public override string ToString()
        {
            return String.Format("Message Action: {0}, Restock {1}, by {2} amount", Action,Book, Amount);
        }
    }
}
