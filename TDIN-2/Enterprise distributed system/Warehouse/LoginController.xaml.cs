using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
            this._parent = parent;
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
