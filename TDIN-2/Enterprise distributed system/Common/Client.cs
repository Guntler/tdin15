﻿using System;
using System.Runtime.Serialization;
using MongoDB.Bson;

namespace Common
{
    [DataContract]
    public class Client
    {
        [DataMember(Name = "id")]
        public string Id;
        [DataMember(Name = "Email")]
        public string Email { get; set; }
        [DataMember(Name = "Username")]
        public string Username { get; set; }
        [DataMember(Name = "Address")]
        public string Address { get; set; }
        [DataMember(Name = "Password")]
        public string Password { get; set; }

        public Client(string username, string password, string address="", string email="")
        {
            var random = new Random();
            var timestamp = DateTime.UtcNow;
            var machine = random.Next(0, 16777215);
            var pid = (short)random.Next(0, 32768);
            var increment = random.Next(0, 16777215);
            Id = new ObjectId(timestamp, machine, pid, increment).ToString();
            Username = username;
            Password = password;
            Address = address;
            Email = email;
        }

        public string IsComplete()
        {
            if (Username.Equals(""))
                return "username";
            if (Password.Equals(""))
                return "password";
            if (Address.Equals(""))
                return "address";
            if (Email.Equals(""))
                return "email";
            return "true";
        }
    }
}
