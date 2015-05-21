using System;
using System.Runtime.Serialization;
using MongoDB.Bson;

namespace Common
{
    [DataContract]
    class Client
    {
        [DataMember] public ObjectId Id { get; set; }

        [DataMember] public string Username { get; set; }

        [DataMember] public string Password { get; set; }

        [DataMember] public string Address { get; set; }

        [DataMember] public string Email { get; set; }

        public Client(string username, string password, string address, string email)
        {
            var random = new Random();
            var timestamp = DateTime.UtcNow;
            var machine = random.Next(0, 16777215);
            var pid = (short)random.Next(0, 32768);
            var increment = random.Next(0, 16777215);
            Id = new ObjectId(timestamp, machine, pid, increment);
            Username = username;
            Password = password;
            Address = address;
            Email = email;
        }
    }
}
