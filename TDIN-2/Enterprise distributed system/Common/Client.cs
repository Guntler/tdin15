using System;
using System.Runtime.Serialization;
using MongoDB.Bson;

namespace Common
{
    [DataContract]
    public class Client : IComparable<Client>
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

        public Client(string username, string password, string address = "", string email = "")
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

        public Field IsComplete()
        {
            if (Username.Equals(""))
                return Field.Username;
            if (Password.Equals(""))
                return Field.Password;
            if (Address.Equals(""))
                return Field.Address;
            if (Email.Equals(""))
                return Field.Email;
            return Field.OK;
        }

        public override string ToString()
        {
            return String.Format("Username: {0}, Password:{1}, Email:{2}, Address:{3}", Username, Password, Email, Address);
        }

        public int CompareTo(Client other)
        {
            return String.Compare(Username, other.Username, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            if (Username == null) return 0;
            return Username.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            Client other = (Client)obj;
            Console.WriteLine("Equals: " + ToString() + " " + other);
            return other.Username == Username;
        }
    }
    public enum Field
    {
        Username, Password, Email, Address, OK
    }


}
