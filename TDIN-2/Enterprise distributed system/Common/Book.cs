using System;
using System.Runtime.Serialization;
using MongoDB.Bson;

namespace Common
{
    [DataContract]
    class Book
    {
        [DataMember] public ObjectId Id { get; set; }
        [DataMember] public string Title { get; set; }
        [DataMember] public string Author { get; set; }
        [DataMember] public int Price { get; set; }
        public Book(string title, string author, int price)
        {
            var random = new Random();
            var timestamp = DateTime.UtcNow;
            var machine = random.Next(0, 16777215);
            var pid = (short)random.Next(0, 32768);
            var increment = random.Next(0, 16777215);
            Id = new ObjectId(timestamp, machine, pid, increment);
            Title = title;
            Author = author;
            Price = price;
        }
    }
}
