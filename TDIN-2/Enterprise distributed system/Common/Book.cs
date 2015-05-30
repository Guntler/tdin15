﻿using System;
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
        public Book(string title, string author, int price)
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
        }
    }
}