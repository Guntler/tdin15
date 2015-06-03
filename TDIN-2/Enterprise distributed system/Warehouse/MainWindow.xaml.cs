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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string user = UsernameBox.Text;
            string pass = PasswordBox.Password;
            Debug.WriteLine("User: "+user+", pass: "+pass);
            DatabaseConnector client = new DatabaseConnector("tdin", "tdin", "warehouse");
            var collection = client.Database.GetCollection<WarehouseWorker>("users");
            var list = collection.Find(x => x.Username.Equals(user)).ToListAsync();
            list.Wait();
            if (list.Result.Count == 1)
            {
                if (list.Result[0].Password.Equals(pass))
                {
                    Debug.WriteLine("YOU ARE OK TO GO!");
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
