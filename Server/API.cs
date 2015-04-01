using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.SQLite;
using System.Threading;

/// <summary>
/// Class used to store all persistant information
/// Logs all transactions done so far -> possibly in a text file
/// Stores for later use active transactions -> (bonus point?) store in binary file with easy read/write 
///     and all clients check if they have it to transfer to server (useful for distributed systems using diferent computers)
/// </summary>
public class API : MarshalByRefObject, IAPI
{
    public event AlterDelegate alterEvent;
    List<Diginote> RegisteredNotes;
    Dictionary<string, User> RegisteredUsers;
    private List<DOrder> RegisteredOrders;
    private List<DTransaction> RegisteredTransactions;
    string filePath = "Transactions.txt";
    SQLiteConnection m_dbConnection;
    public List<DOrder> ActiveOrders { get; private set; }

    public double ExchangeValue { get; set; }

    public API()
    {
        RegisteredOrders = new List<DOrder>();
        RegisteredNotes = new List<Diginote>();
        RegisteredTransactions = new List<DTransaction>();
        RegisteredUsers = new Dictionary<string, User>();
        ExchangeValue = 1.00;
        m_dbConnection = new SQLiteConnection("Data Source=../../../database.db;Version=3;");
        m_dbConnection.Open();
        this.ActiveOrders = this.GetActiveOrders(); ;
        //maybe ask clients for their database files
    }

    ~API()
    {
        m_dbConnection.Close();
    }

    void NotifyClients(Operation op, DOrder order)
    {
        if (alterEvent != null)
        {
            Delegate[] invkList = alterEvent.GetInvocationList();

            foreach (AlterDelegate handler in invkList)
            {
                new Thread(() =>
                {
                    try
                    {
                        handler(op, order);
                        Console.WriteLine("Invoking event handler");
                    }
                    catch (Exception)
                    {
                        alterEvent -= handler;
                        Console.WriteLine("Exception: Removed an event handler");
                    }
                }).Start();
            }
        }
    }

    public override object InitializeLifetimeService()
    {
        return null;
    }

    #region Interface functions

    #region User

    public User ValidateUser(string username, string pass)
    {
        var user = GetUserByName(username) ?? new User();
        return user.password.Equals(pass) ? user : null;

        /*if (!RegisteredUsers.ContainsKey(username)) //user does not exist
            return false;
        else if (RegisteredUsers[username].password != pass) //credentials dont match
            return false;
        else return true;*/
    }

    public int RegisterUser(ref User us)
    {

        if (GetUserByName(us.Nickname)!=null)
            return 1; //username already exists
        else
        {
            string sql = "Insert into User (name, Nickname, password) values ('" + us.name + "','" + us.Nickname + "','" + us.password + "')";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
            
            sql = @"select last_insert_rowid()";
            command = new SQLiteCommand(sql, m_dbConnection);
            us.Id = (long)command.ExecuteScalar();
            Console.WriteLine(@"New id: " + us.Id);

            RegisteredUsers.Add(us.Nickname, us);

            sql = "select * from User";
            command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            /*while (reader.Read())
                Console.WriteLine("Name: " + reader["name"] + "\tnickname: " + reader["Nickname"] + "\tpassword: " + reader["password"]);*/

           //Console.WriteLine("User registered:\n" + us);

            for (int i = 0; i < 100; i++)
                RegisterDiginote(us);

            return 0;
        }
    }

    public void RemoveUserByName(string nickname)
    {
        RegisteredUsers.Remove(nickname);
        string sql = "delete from User where Nickname = '" + nickname + "'";
        SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
        command.ExecuteNonQuery();

        sql = "select * from User where Nickname = '" + nickname + "'";
        command = new SQLiteCommand(sql, m_dbConnection);
        SQLiteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            Console.WriteLine("Could not remove User " + nickname);
        }

        Console.WriteLine("Successfuly removed User " + nickname);
    }

    public User GetUserByName(string nickname)
    {
        string sql = "select * from User where Nickname = '" + nickname+"'";
        SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
        SQLiteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            User user = new User(reader["name"].ToString(), reader["Nickname"].ToString(), reader["password"].ToString());
            Console.WriteLine("Found: " + "Name: " + reader["name"] + "\tnickname: " + reader["Nickname"] + "\tpassword: " + reader["password"]);
            return user;
        }

        Console.WriteLine("No users matched the query " + nickname);
        return null;
    }

    /// <summary>
    /// Updates User entry in dictionary, username must be the same, must keep oldpassword until operation is concluded
    /// </summary>
    public int UpdateUser(string username, string oldPassword, User updated)
    {
        if (!RegisteredUsers.ContainsKey(username)) //username does not exists
            return 1;
        else
        {
            User entry = RegisteredUsers[username];
            if (entry.password != oldPassword) //incorrect credentials
            {
                return 2;
            }
            else
            {
                RegisteredUsers[username] = updated;
                return 0;
            }
        }
    }

    #endregion User

    #region Diginote

    /**
     * TODO Add Error Checking
     */
    public void RegisterDiginote(User owner)
    {
        if (GetUserByName(owner.Nickname) == null)
        {
            Console.WriteLine(@"The user appointed as owner of this Diginote does not exist.");
            return;
        }

        Diginote note = new Diginote(owner);

        string sql = "Insert into Diginote (owner) values ('" + owner.Nickname + "')";

        SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
        command.ExecuteNonQuery();

        sql = @"select last_insert_rowid()";
        command = new SQLiteCommand(sql, m_dbConnection);
        note.Id = (long)command.ExecuteScalar();
        Console.WriteLine(@"New note id: " + note.Id);
        
        RegisteredNotes.Add(note);
    }

    public List<Diginote> GetDiginotesByUser(User us)
    {
        List<Diginote> notes = new List<Diginote>();

        string sql = "select * from Diginote where owner = '" + us.Nickname + "'";
        SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
        SQLiteDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            Diginote note = null;
            note = new Diginote(us);
            notes.Add(note);
        }

        return notes;
    }

    public Diginote GetDiginote(long id)
    {
        string sql = "select * from Diginote where id = '" + id + "'";
        SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
        SQLiteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            Diginote note = null;
            User owner = GetUserByName(reader["id"].ToString());
            if(owner!=null)
                note = new Diginote(owner);
            else
            {
                Console.WriteLine(@"The user specified as the owner no longer exists.");
                return null;
            }
            Console.WriteLine(@"Found: " + note.ToString());
            return note;
        }

        Console.WriteLine(@"No notes matched the query " + id);
        return null;
    }

    public void DeleteDiginote(long id)
    {
        RegisteredNotes.RemoveAll(c => c.Id == id);
        string sql = "Delete from Diginote where id = '" + id + "'";
        SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
        command.ExecuteNonQuery();

        sql = "Select * from Diginote where id = '" + id + "'";
        command = new SQLiteCommand(sql, m_dbConnection);
        SQLiteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            Console.WriteLine("Could not remove Diginote " + id);
        }

        Console.WriteLine("Successfuly removed Diginote " + id);
    }

    #endregion Diginote

    #region Order

    public void RegisterOrder(ref DOrder order)
    {
        string sql = "Insert into DOrder (type,status,source,value,amount) values ('"
                        + ((int)order.Type)+1 + "','" + ((int)order.Status)+1 + "','" + order.Source.Nickname + "','" + order.Value + "','"
                        + order.Amount + "')";

        SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
        if(command.ExecuteNonQuery()<0)
            Console.WriteLine("Error registering an Order.");
        else
            Console.WriteLine("Successfully registered an Order.");

        sql = @"select last_insert_rowid()";
        command = new SQLiteCommand(sql, m_dbConnection);
        order.Id = (long)command.ExecuteScalar();
        Console.WriteLine(@"New order id: " + order.Id);

        RegisteredOrders.Add(order);
        
        NotifyClients(Operation.New, order);
    }

    public DOrder GetOrder(long id)
    {
        string sql = "select * from DOrder where id = '" + id + "'";
        SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
        SQLiteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            Console.WriteLine("In read order");
            DOrder order = null;
            User source = GetUserByName(reader["source"].ToString());
            OrderType type;
            if (!OrderType.TryParse(reader["type"].ToString(), out type))
            {
                Console.WriteLine(@"Type of order was in an incorrect format.");
                return null;
            }

            OrderStatus status;
            if (!OrderStatus.TryParse(reader["status"].ToString(), out status))
            {
                Console.WriteLine(@"Type of order was in an incorrect format.");
                return null;
            }

            //NOTE: Double.tryparse may cause problems - refer to here if anything happens
            if (source == null)
            {
                Console.WriteLine(@"The user specified as the owner no longer exists.");
                return null;
            }

            int amount = Convert.ToInt32(reader["amount"].ToString());
            double value;
            if (!Double.TryParse(reader["value"].ToString(), out value))
            {
                Console.WriteLine(@"Could not parse value of Order.");
                return null;
            }

            long orderId = Convert.ToInt64(reader["id"].ToString());
            order = new DOrder(source, amount, value, type) { Id = orderId, Status = status };

            Console.WriteLine(@"Found: " + order.ToString());
            return order;
        }

        Console.WriteLine(@"No orders matched the query " + id);
        return null;
    }

    private List<DOrder> GetActiveOrders()
    {
        string sql = "select * from DOrder where status = 1 ORDER BY id";
        SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
        SQLiteDataReader reader = command.ExecuteReader();
        List<DOrder> orders = new List<DOrder>();
        while (reader.Read())
        {
            DOrder order = null;
            User source = GetUserByName(reader["source"].ToString());
            OrderType type;
            if (!OrderType.TryParse(reader["type"].ToString(), out type))
            {
                Console.WriteLine(@"Type of order was in an incorrect format.");
                return null;
            }

            OrderStatus status;
            if (!OrderStatus.TryParse(reader["status"].ToString(), out status))
            {
                Console.WriteLine(@"Type of order was in an incorrect format.");
                return null;
            }

            //NOTE: Double.tryparse may cause problems - refer to here if anything happens
            if (source == null)
            {
                Console.WriteLine(@"The user specified as the owner no longer exists.");
                return null;
            }

            int amount = Convert.ToInt32(reader["amount"].ToString());
            double value;
            if (!Double.TryParse(reader["value"].ToString(), out value))
            {
                Console.WriteLine(@"Could not parse value of Order.");
                return null;
            }

            long orderId = Convert.ToInt64(reader["id"].ToString());
            order = new DOrder(source, amount, value, type) { Id = orderId, Status = status };

            orders.Add(order);
        }

        return orders;
    }

    public void EditOrder(DOrder order)
    {
        string sql = "Update DOrder SET amount = '" + order.Amount + "' where id = '" + order.Id + "'";

        SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
        command.ExecuteNonQuery();
        
        NotifyClients(Operation.Change, order);
    }

    public void DeleteOrder(DOrder order)
    {
        RegisteredOrders.RemoveAll(c => c.Id == order.Id);
        string sql = "Delete from DOrder where id = '" + order.Id + "'";
        SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
        command.ExecuteNonQuery();

        sql = "select * from DOrder where id = '" + order.Id + "'";
        command = new SQLiteCommand(sql, m_dbConnection);
        SQLiteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            Console.WriteLine("Could not remove Order " + order.Id);
        }

        Console.WriteLine("Successfuly removed Order " + order.Id);
    }

    #endregion Order

    #region Transaction

    public void RegisterTransaction(DTransaction transaction)
    {
        string sql = "Insert into DTransaction (id,destination,value) values ('"
                        + transaction.Order.Id + "','" + transaction.Destination.Nickname + "','" + transaction.Value + "')";

        SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
        if (command.ExecuteNonQuery() < 0)
            Console.WriteLine("Error registering a Transaction.");
        else
            Console.WriteLine("Successfully registered a Transaction.");

        RegisteredTransactions.Add(transaction);
    }

    public DTransaction GetTransaction(long id)
    {
        string sql = "select * from DTransaction where id = '" + id + "'";
        SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
        SQLiteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            DTransaction trans = null;
            User dest = GetUserByName(reader["destination"].ToString());

            DOrder order = GetOrder(Convert.ToInt64(reader["id"].ToString()));
            if (order == null)
            {
                Console.WriteLine(@"The Order of this Transaction could not be found.");
                return null;
            }

            if (dest == null)
            {
                Console.WriteLine(@"The user specified as the Destination no longer exists.");
                return null;
            }

            trans = new DTransaction(dest, Double.Parse(reader["value"].ToString()), order);

            Console.WriteLine(@"Found: " + trans.ToString());
            return trans;
        }

        Console.WriteLine(@"No Transactions matched the query " + id);
        return null;
    }

    public void DeleteTransaction(DTransaction transaction)
    {
        RegisteredTransactions.RemoveAll(c => c.Order.Id == transaction.Order.Id);

        string sql = "Delete from DTransaction where id = '" + transaction.Order.Id + "'";
        SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
        command.ExecuteNonQuery();

        sql = "select * from DTransaction where id = '" + transaction.Order.Id + "'";
        command = new SQLiteCommand(sql, m_dbConnection);
        SQLiteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            Console.WriteLine("Could not remove Transaction pertaining to Order " + transaction.Order.Id);
        }

        Console.WriteLine("Successfuly removed Transaction pertaining to Order " + transaction.Order.Id);
    }

    #endregion Transaction

    #region Actions
    public bool SellOrder(int quantity)
    {
        return false;
    }

    public bool BuyOrder(int quantity)
    {
        return false;
    }

    public bool ChangeExchangeValue(double value)
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
