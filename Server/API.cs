using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.SQLite;

/// <summary>
/// Class used to store all persistant information
/// Logs all transactions done so far -> possibly in a text file
/// Stores for later use active transactions -> (bonus point?) store in binary file with easy read/write 
///     and all clients check if they have it to transfer to server (useful for distributed systems using diferent computers)
/// </summary>
public class API : MarshalByRefObject, IAPI
{
    List<Diginote> registared_Coins;
    Dictionary<string, User> UserTable;
    string filePath = "Transactions.txt";
    SQLiteConnection m_dbConnection;

    private double exchangeValue { get; set; }

    public API()
    {
        this.registared_Coins = new List<Diginote>();
        this.UserTable = new Dictionary<string, User>();
        this.exchangeValue = 1.00;
        m_dbConnection = new SQLiteConnection("Data Source=../../../database.db;Version=3;");
        m_dbConnection.Open();

        //maybe ask clients for their database files
    }

    ~API()
    {
        m_dbConnection.Close();
    }

    public override object InitializeLifetimeService()
    {
        return null;
    }

    #region Interface functions

    #region User

    public bool validateUser(string username, string pass)
    {
        if (!UserTable.ContainsKey(username)) //user does not exist
            return false;
        else if (UserTable[username].password != pass) //credentials dont match
            return false;
        else return true;
    }

    public int registerUser(User us)
    {

        if (UserTable.ContainsKey(us.nickname))
            return 1; //username already exists
        else
        {
            UserTable.Add(us.nickname, us);
            string sql = "Insert into User (name, nickname, password) values ('" + us.name + "','" + us.nickname + "','" + us.password + "')";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();

            sql = "select * from User";
            command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
                Console.WriteLine("Name: " + reader["name"] + "\tnickname: " + reader["nickname"] + "\tpassword: " + reader["password"]);

           //Console.WriteLine("User registered:\n" + us);
            return 0;
        }
    }

    /// <summary>
    /// Updates User entry in dictionary, username must be the same, must keep oldpassword until operation is concluded
    /// </summary>
    public int updateUser(string Username, string OldPassword, User Updated)
    {
        if (!UserTable.ContainsKey(Username)) //username does not exists
            return 1;
        else
        {
            User entry = UserTable[Username];
            if (entry.password != OldPassword) //incorrect credentials
            {
                return 2;
            }
            else
            {
                UserTable[Username] = Updated;
                return 0;
            }
        }
    }

    #endregion User

    #region Actions
    public bool sellOrder(int quantity)
    {
        return false;
    }

    public bool buyOrder(int quantity)
    {
        return false;
    }

    public bool changeExchangeValue(double value)
    {
        return false;
    }

    #endregion Actions

    #endregion Interface functions

    public void saveData()
    {
        string lines = ""; //generate text to save
        using (StreamWriter file = File.AppendText(filePath)) //open in append, if not exist creates
        {
            file.WriteLine(lines);
            file.Close();
        }
        return;
    }
}
