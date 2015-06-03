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

namespace StoreApp
{
    struct User
    {
        public string Username;

        public User(string username)
        {
            Username = username;
        }
    }
    /// <summary>
    /// Interaction logic for Landing.xaml
    /// </summary>
    public partial class Landing : Window
    {
        private readonly User _user;

        public Landing(string username)
        {
            InitializeComponent();
            _user = new User(username);
            WelcomeBox.Text = "Welcome, " + _user.Username;
        }

        private void Button_Click_Logout(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_Purchase(object sender, RoutedEventArgs e)
        {
            Purchase dialog = new Purchase()
            {
                Title = "Purchase",
                ShowInTaskbar = false,
                ResizeMode = ResizeMode.NoResize,
                Topmost = true,
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
            }
        }

        private void Button_Click_Notifications(object sender, RoutedEventArgs e)
        {
            Shippings dialog = new Shippings()
            {
                Title = "Notifications",
                ShowInTaskbar = false,
                ResizeMode = ResizeMode.NoResize,
                Topmost = true,
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
            }

        }
    }
}
