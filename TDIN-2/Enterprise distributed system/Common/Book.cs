using System;
using System.Runtime.Serialization;
using MongoDB.Bson;

namespace Common
{
    [DataContract]
    public class Book
    {
        [DataMember(Name = "id")] public string Id { get; set; }
        [DataMember(Name = "Title")]public string Title { get; set; }
        [DataMember(Name = "Author")]public string Author { get; set; }
        [DataMember(Name = "Price")]public double Price { get; set; }
        [DataMember(Name = "Quantity")]
        public double Quantity { get; set; }
        public Book(string title, string author, int price, int quantity)
        {
            var random = new Random();
            var timestamp = DateTime.UtcNow;
            var machine = random.Next(0, 16777215);
            var pid = (short)random.Next(0, 32768);
            var increment = random.Next(0, 16777215);
            Id = new ObjectId(timestamp, machine, pid, increment).ToString();
            Title = title;
            Author = author;
            Price = price;
            Quantity = quantity;
        }
        public override string ToString()
        {
            return String.Format("Book Id: {0}, Title:{1}, Author:{2}, Price:{3}, Quantity:{4}", Id, Title, Author, Price, Quantity);
        }
    }
}
