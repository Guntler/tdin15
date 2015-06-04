using System;
using System.Runtime.Serialization;
using MongoDB.Bson;

namespace Common
{
    [DataContract]

    public class WarehouseWorker
    {
        [DataMember(Name = "id")] public string Id;

        [DataMember(Name = "Username")]
        public string Username { get; set; }

        [DataMember(Name = "Password")]
        public string Password { get; set; }

        public WarehouseWorker(string username, string password)
        {
            var random = new Random();
            var timestamp = DateTime.UtcNow;
            var machine = random.Next(0, 16777215);
            var pid = (short) random.Next(0, 32768);
            var increment = random.Next(0, 16777215);
            Id = new ObjectId(timestamp, machine, pid, increment).ToString();
            Username = username;
            Password = password;
        }
    }
}
