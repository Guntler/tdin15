using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Common;
using MongoDB.Driver;

namespace Warehouse
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        private readonly GUI _parent;
        public LoginView(GUI parent)
        {
            _parent = parent;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string user = UsernameBox.Text;
            string pass = PasswordBox.Password;
            Debug.WriteLine("User: " + user + ", pass: " + pass);
            DatabaseConnector client = new DatabaseConnector("mongodb://tdin:tdin@ds031812.mongolab.com:31812/", "warehouse");
            var collection = client.Database.GetCollection<WarehouseWorker>("users");
            var list = collection.Find(x => x.Username.Equals(user)).ToListAsync();
            list.Wait();
            if (list.Result.Count == 1)
            {
                if (list.Result[0].Password.Equals(pass))
                {
                    _parent.ShowMain();
                }
                else
                {
                    throw new Exception("Invalid credentials");
                }
            }
            else
            {
                throw new Exception("No match found");
            }

        }
    }
}
