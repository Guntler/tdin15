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
    string logPath;
    SQLiteConnection m_dbConnection;
    HashSet<User> loggedInUsers;
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
        this.ActiveOrders = this.GetActiveOrders();
        loggedInUsers = new HashSet<User>();
        logPath = "Server Log:" + getTime();
        Console.WriteLine("File path:"+logPath);
        //maybe ask clients for their database files
    }

    ~API()
    {
        m_dbConnection.Close();
    }

    static string getTime()
    {
        return DateTime.UtcNow.ToString("yyyy/MM/dd-HH':'mm':'ss");
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

        if (user.password.Equals(pass) && loggedInUsers.Add(user))
        {
            user.wallet = GetDiginotesByUser(user);
            return user;
        }
        return null;

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
            //Console.WriteLine("Found: " + "Name: " + reader["name"] + "\tnickname: " + reader["Nickname"] + "\tpassword: " + reader["password"]);
            return user;
        }

        //Console.WriteLine("No users matched the query " + nickname);
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

    public void logout(ref User us) {
        if (loggedInUsers.Contains(us))
        {
            loggedInUsers.Remove(us);
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
        //Console.WriteLine(@"New note id: " + note.Id);
        
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

    public void PurchaseDiginotes(User owner, int amt, User buyer)
    {
        for (var i = 0; i < amt; i++)
        {
            string sql = "Update Diginote SET owner = '" + buyer.Nickname + "' where id = '" + owner.wallet[0].Id + "'";
            owner.wallet.RemoveAt(0);

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
        }   
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
        if ((order.Type.Equals(OrderType.Buy) && order.Value > ExchangeValue) ||
            (order.Type.Equals(OrderType.Sell) && order.Value < ExchangeValue))
        {
            //Ask for a new value
        }

        var sql = "";
        SQLiteCommand command;

        try
        {
            sql = "Insert into DOrder (type,status,date,source,value,amount) values ('"
                        + (((int)order.Type) + 1) + "','" + (((int)order.Status) + 1) + "','" + order.Date.ToString("yyyyMMddHHmmss") + "','" + order.Source.Nickname + "','" + order.Value + "','"
                        + order.Amount + "')";

            command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
        }
        catch (SQLiteException sqle)
        {
            Console.WriteLine(sqle.ToString());
        }
        finally {
            this.ActiveOrders.Add(order);
            sql = @"select last_insert_rowid()";
            command = new SQLiteCommand(sql, m_dbConnection);
            order.Id = (long)command.ExecuteScalar();
            Console.WriteLine(@"New order id: " + order.Id);

            RegisteredOrders.Add(order);

            NotifyClients(Operation.New, order);

            //UNCOMMENT WHEN READY TO TEST
            //MatchOrder(order);
        }
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
            if (!Enum.IsDefined(typeof(OrderType), (Convert.ToInt32(reader["type"].ToString())) - 1))
            {
                Console.WriteLine(@"Type of order was in an incorrect format.");
                continue;
            }
            type = ((OrderType)(Convert.ToInt32(reader["type"].ToString())) - 1);

            OrderStatus status;
            if (!Enum.IsDefined(typeof(OrderStatus), (Convert.ToInt32(reader["status"].ToString())) - 1))
            {
                Console.WriteLine(@"Status of order was in an incorrect format.");
                continue;
            }
            status = ((OrderStatus)(Convert.ToInt32(reader["status"].ToString())) - 1);

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

            string date = reader["date"].ToString();
            Console.WriteLine(type.ToString());
            long orderId = Convert.ToInt64(reader["id"].ToString());
            order = new DOrder(source, amount, value, type,DateTime.Parse(date)) { Id = orderId, Status = status };
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
            if (!Enum.IsDefined(typeof(OrderType), (Convert.ToInt32(reader["type"].ToString()))-1)){
                Console.WriteLine(@"Type of order was in an incorrect format.");
                continue;
            }
            type = ((OrderType) (Convert.ToInt32(reader["type"].ToString()))-1);

            OrderStatus status;
            if (!Enum.IsDefined(typeof(OrderStatus), (Convert.ToInt32(reader["status"].ToString())) - 1))
            {
                Console.WriteLine(@"Status of order was in an incorrect format.");
                continue;
            }
            status = ((OrderStatus)(Convert.ToInt32(reader["status"].ToString())) - 1);

            //NOTE: Double.tryparse may cause problems - refer to here if anything happens
            if (source == null)
            {
                Console.WriteLine(@"The user specified as the owner no longer exists.");
                continue;
            }

            int amount = Convert.ToInt32(reader["amount"].ToString());
            double value;
            if (!Double.TryParse(reader["value"].ToString(), out value))
            {
                Console.WriteLine(@"Could not parse value of Order.");
                continue;
            }

            string date = reader["date"].ToString();

            long orderId = Convert.ToInt64(reader["id"].ToString());
            order = new DOrder(source, amount, value, type, DateTime.Parse(date)) { Id = orderId, Status = status };

            orders.Add(order);
        }

        return orders;
    }

    public void ChangeAllUserOrders(User user, double newValue)
    {
        foreach (var o in ActiveOrders.Where(o => o.Source.Nickname.Equals(user.Nickname)))
        {
            o.Value=newValue;
            EditOrder(o);
        }
    }

    public void DeleteAllUserOrders(User user)
    {
        foreach (var o in ActiveOrders.Where(o => o.Source.Nickname.Equals(user.Nickname)))
        {
            CancelOrder(o);
        }
    }

    public void EditOrder(DOrder order)
    {
        string sql = "Update DOrder SET value = '" + order.Value + "', amount = '"+ order.Amount + "' where id = '" + order.Id + "'";

        SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
        command.ExecuteNonQuery();

        var i = ActiveOrders.FindIndex(o => order.Id == o.Id);

        if (order.Status.Equals(OrderStatus.Active))
        {
            if (i > -1) ActiveOrders[i] = order;
        }

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

    public void CancelOrder(DOrder order)
    {
        order.Status=OrderStatus.Cancelled;

        string sql = "Update DOrder SET status = '" + (((int)order.Status) + 1) + "' where id = '" + order.Id + "'";

        SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
        command.ExecuteNonQuery();

        ActiveOrders.Remove(order);
        NotifyClients(Operation.Remove,order);
    }

    public void FulfillOrder(User buyer, DOrder order)
    {
        if (!buyer.Nickname.Equals(order.Source.Nickname))
        {
            PurchaseDiginotes(order.Source, order.Amount, buyer);
            order.Status = OrderStatus.Fulfilled;

            string sql = "Update DOrder SET status = '" + (((int)order.Status) + 1) + "' where id = '" + order.Id + "'";

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();

            ActiveOrders.Remove(order);
            RegisteredTransactions.Add(new DTransaction(buyer,order.Value,order));

            NotifyClients(Operation.Remove, order);
        }
        else
        {
            //Display Error message because you can't buy your own order
        }
    }

    public void MatchOrder(DOrder order)
    {
        //As the buyer
        if (order.Type.Equals(OrderType.Buy))
        {
            DOrder oldestOrder = FindOldestOrder(OrderType.Sell, order.Source.Nickname);

            while (oldestOrder != null)
            {
                DOrder dummyOrder = order;
                oldestOrder.Amount -= order.Amount;
                order.Amount -= oldestOrder.Amount;
                if (oldestOrder.Amount < 0) oldestOrder.Amount = 0;
                if (order.Amount < 0) order.Amount = 0;
                EditOrder(order);
                EditOrder(oldestOrder);

                NotifyClients(Operation.Notify,dummyOrder);

                if (oldestOrder.Amount <= 0)
                {
                    FulfillOrder(order.Source,oldestOrder);     //we fully bought someone's sell order
                }

                if (order.Amount <= 0)
                {
                    FulfillOrder(oldestOrder.Source, order);     //someone fully sold to our buy order
                    break;
                }
                oldestOrder = FindOldestOrder(OrderType.Sell, order.Source.Nickname);
            }

        } //As the seller
        else
        {
            DOrder oldestOrder = FindOldestOrder(OrderType.Buy, order.Source.Nickname);

            while (oldestOrder != null)
            {
                DOrder dummyOrder = order;
                oldestOrder.Amount -= order.Amount;
                order.Amount -= oldestOrder.Amount;
                if (oldestOrder.Amount < 0) oldestOrder.Amount = 0;
                if (order.Amount < 0) order.Amount = 0;
                EditOrder(order);
                EditOrder(oldestOrder);

                NotifyClients(Operation.Notify, dummyOrder);

                if (oldestOrder.Amount <= 0)
                {
                    FulfillOrder(order.Source, oldestOrder);     //we fully sold to someone's buy order
                }

                if (order.Amount <= 0)
                {
                    FulfillOrder(oldestOrder.Source, order);     //someone fully bought to our sell order
                    break;
                }
                oldestOrder = FindOldestOrder(OrderType.Buy, order.Source.Nickname);
            }
        }
    }

    public DOrder FindOldestOrder(OrderType type, String source)
    {
        DOrder oldestOrder = null;
        foreach (var o in ActiveOrders)
        {
            if ((oldestOrder == null || (o.Date < oldestOrder.Date))
                    && o.Type.Equals(type)&&!o.Source.Nickname.Equals(source))
            {
                oldestOrder = o;
            }
        }

        return oldestOrder;
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

    public void logEntry(string who, string when, string what, string result)
    {
        using (StreamWriter file = File.AppendText(logPath)) //open in append, if not exist creates
        {
            file.WriteLine(String.Format("{0} [{1}] {2} {3} ", who, when, what, result));
            //file.WriteLine(line);
            file.Close();
        }
        return;
    }
}
