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
    private int id;
    public int value { get; set; }
    Diginote() { 
        this.id = ++idCounter; this.value = 1; 
    }

    public int getId() { 
        return this.id; 
    }
}

[Serializable]
/// <summary>
/// name refers to the complete user name
/// nickname is the data valued used to check User credentials (username)
/// use password to confirm data information
/// </summary>
public class User
{
    public string name { get; set; }

    public string nickname { get; set; }

    public string password { get; set; }

    private List<Diginote> wallet;

    public User(string name, string username, string password) {
        this.name = name;
        this.nickname = username;
        this.password = password;
        this.wallet = new List<Diginote>();
    }

    public override string ToString()
    {
        return "Name: " + this.name + "\nNickname: " + this.nickname + "\nPassword: " + this.password + "\n";
    }

}

public interface IAPI
{

    #region User

    bool validateUser(string username, string pass);

    int registerUser(User us);

    int updateUser(string Username, string OldPassword, User Updated);

    #endregion User

    #region Actions
    bool sellOrder(int quantity);

    bool buyOrder(int quantity);

    bool changeExchangeValue(double value);

    #endregion Actions
}
