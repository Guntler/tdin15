﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Class used to store all persistant information
/// Logs all transactions done so far -> possibly in a text file
/// Stores for later use active transactions -> (bonus point?) store in binary file with easy read/write 
///     and all clients check if they have it to transfer to server (useful for distributed systems using diferent computers)
/// </summary>
public class Database
{
    List<Diginote> registared_Coins;

    public Database() {
        this.registared_Coins = new List<Diginote>();
        //maybe ask clients for their database files
    }

    public bool validateUser(User us) {
        return false;
    }

    public void registerUser(User us) { 
    
    }

    public void saveData() {

    }
}
