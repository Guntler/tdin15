using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
/// <summary>
/// The object class to be stored/transfered through the system
/// </summary>
public class Diginote
{
    private static int idCounter = 0;
    public long Id { get; set; }
    public int value { get; set; }
    public User Owner { get; set; }
    public Diginote(User owner) { 
        Id = ++idCounter; this.value = 1;
        Owner = owner;
    }
}

public enum OrderStatus { Active,Fulfilled,Cancelled };

public enum OrderType { Buy,Sell };

[Serializable]
/// <summary>
/// The object class referring to a purchase/sale order 
/// </summary>
public class DOrder
{
    public long Id { get; set; }
    public double Value { get; set; }
    public OrderType Type { get; set; }
    public OrderStatus Status { get; set; }
    public int Amount { get; set; }
    public User Source { get; set; }
    public DateTime Date { get; set; }

    public DOrder(User source, int amount,double value,OrderType type,DateTime date)
    {
        Source = source;
        Value = value;
        Amount = amount;
        Status = OrderStatus.Active;
        Type = type;
        Date = date;
    }

    public override string ToString()
    {
        return "id: " + this.Id + "\nValue: " + this.Value + "\nType: " + this.Type.ToString() + "\nStatus: "+this.Status.ToString() + "\nAmount: " + this.Amount + "\nSource: " + this.Source.Nickname + "\nDate: " + this.Date.ToString();
    }

}

[Serializable]
/// <summary>
/// The object class referring to a concluded or cancelled Order
/// </summary>
public class DTransaction
{
    public DOrder Order { get; set; }
    public double Value { get; set; }
    public User Destination { get; set; }
    public DateTime Date { get; set; }

    public DTransaction(User destination, double value, DOrder order)
    {
        Destination = destination;
        Value = value;
        Order = order;
        Date = DateTime.Today;
    }

}

[Serializable]
/// <summary>
/// name refers to the complete user name
/// Nickname is the data valued used to check User credentials (username)
/// use password to confirm data information
/// </summary>
public class User
{
    public long Id { get; set; }
    public string name { get; set; }

    public string Nickname { get; set; }

    public string password { get; set; }

    public  List<Diginote> wallet;

    public User(string name, string username, string password) {
        if (name == "" || username == "" || password == ""){
            throw new InvalidObject("Invalid user parameter values.");
        } else {
            this.name = name;
            this.Nickname = username;
            this.password = password;
            this.wallet = new List<Diginote>();
        }
    }

    public User(){
        this.name = "";
        this.Nickname = "";
        this.password = "";
    }

    public override string ToString()
    {
        return "Name: " + this.name + "\nNickname: " + this.Nickname + "\nPassword: " + this.password + "\n";
    }

    public override int GetHashCode() { return Id.GetHashCode(); }
    public override bool Equals(object obj) { return this.Nickname.Equals(((User)obj).Nickname); }
}

public enum Operation { New, Change, Remove, Notify};

public delegate void AlterDelegate(Operation op, DOrder order);

public class AlterEventRepeater : MarshalByRefObject
{
    public event AlterDelegate alterEvent;

    public override object InitializeLifetimeService()
    {
        return null;
    }

    public void Repeater(Operation op, DOrder order)
    {
        if (alterEvent != null)
            alterEvent(op, order);
    }
}

public class InvalidObject: Exception
{
    public InvalidObject()
    {
    }

    public InvalidObject(string message)
        : base(message)
    {
    }

    public InvalidObject(string message, Exception inner)
        : base(message, inner)
    {
    }
}

public interface IAPI
{
    event AlterDelegate alterEvent;
    List<DOrder> ActiveOrders { get; }
    double ExchangeValue { get; }

    #region User

    User ValidateUser(string username, string pass);

    int RegisterUser(ref User us);

    User GetUserByName(string name);

    void RemoveUserByName(string name);

    int UpdateUser(string username, string oldPassword, User updated);

    void logout(ref User us);

    #endregion User

    #region Diginote

    void RegisterDiginote(User owner);

    List<Diginote> GetDiginotesByUser(User us);

    Diginote GetDiginote(long id);

    void PurchaseDiginotes(User owner, int amt, User buyer);

    void DeleteDiginote(long id);

    #endregion Diginote

    #region Order

    void RegisterOrder(ref DOrder order);

    DOrder GetOrder(long id);

    void ChangeAllUserOrders(User user, float newValue);

    void EditOrder(DOrder order);
    
    void DeleteOrder(DOrder order);

    void DeleteAllUserOrders(User user);

    void CancelOrder(DOrder order);

    void FulfillOrder(User buyer, DOrder order);

    void MatchOrder(DOrder order);

    DOrder FindOldestOrder(OrderType type, String source);
    #endregion Order

    #region Transaction

    void RegisterTransaction(DTransaction transaction);

    DTransaction GetTransaction(long id);

    void DeleteTransaction(DTransaction transaction);

    #endregion Transaction

    #region Actions

    bool SellOrder(int quantity);

    bool BuyOrder(int quantity);

    bool ChangeExchangeValue(double value);

    #endregion Actions

}
