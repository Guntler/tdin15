using System;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class Message
    {
        [DataMember(Name = "action")]
        public string Action { get; set; }

        [DataMember(Name = "book")]
        public Book Book { get; set; }

        [DataMember(Name = "Amount")]
        public int Amount { get; set; }

        public Message(string action, int quantity, Book book)
        {
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
