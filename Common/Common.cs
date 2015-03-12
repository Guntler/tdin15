using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// The object class to be stored/transfered through the system
    /// </summary>
    public class Diginote
    {
        private static int idCounter=0;
        private int id;
        public int value { get; set; }
        Diginote() { this.id = ++idCounter; this.value = 1; }

        public int getId() { return this.id; }
    }

    public class User {
        public string username { get; set; }

        public string password { get; set; }

        private List<Diginote> wallet;
    }

    public class Transaction { 
    
    }
}
