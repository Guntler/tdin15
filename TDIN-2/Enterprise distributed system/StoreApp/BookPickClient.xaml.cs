using System.Windows;
using Common;
using MongoDB.Driver;

namespace StoreApp
{
    /// <summary>
    /// Interaction logic for BookNoStockDialog.xaml
    /// </summary>
    public partial class BookPickClient : Window
    {
        public bool WillSend { get; set; }
        public string user = "";

        public BookPickClient()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            user = UserBox.Text;
            if (!user.Equals(""))
            {
                DialogResult = true;
            }
            else
                WarningBox.Text = "Please input a user name.";
        }
    }
}
