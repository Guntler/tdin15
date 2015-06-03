using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using Common;
using MongoDB.Driver;

namespace StoreApp
{
    /// <summary>
    /// Interaction logic for BookNoStockDialog.xaml
    /// </summary>
    public partial class BookNoStockDialog : Window
    {
        public bool WillSend { get; set; }
        public Client User { get; set; }

        public BookNoStockDialog(string title)
        {
            InitializeComponent();
            WillSend = false;
            User = null;
            NoStockMessage.Text = "There are no remaining copies of " + title +
                ". Should you wish to notify the warehouse, please search for a user to append to this order.";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (User != null)
            {
                WillSend = true;
                DialogResult = true;
            }
            else
                SearchResultBox.Text = "Please search for a User before sending the message.";
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            
        }

        private void Button_Click_Search(object sender, RoutedEventArgs e)
        {
            DatabaseConnector client = new DatabaseConnector("mongodb://tdin:tdin@ds031942.mongolab.com:31942/", "store");
            var collection = client.Database.GetCollection<Client>("clients");
            var list = collection.Find(x => x.Username.Equals(UserBox.Text)).ToListAsync();
            list.Wait();
            if (list.Result.Count == 1)
            {
                User = list.Result[0];
                SearchResultBox.Text = "Found a user " + User.Username + " with email address " + User.Email;
            }
            else
            {
                SearchResultBox.Text = "User could not be found.";
            }
        }
    }
}
