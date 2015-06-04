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
using System.Windows.Shapes;
using Store;


namespace StoreApp
{
    /// <summary>
    /// Interaction logic for Shippings.xaml
    /// </summary>
    public partial class Shippings : Window
    {
        public Shippings()
        {
            InitializeComponent();
            this.DataContext = this;
            var messages = new List<MessageItem>();
            foreach(var m in FrontEndService.ReceivedMessages)
            {
                messages.Add(new MessageItem() { Book=m.Book.Title,Amount=m.Amount });
            }
            messages.Add(new MessageItem() { Book = "Novo Titulo", Amount = 5 });
            messages.Add(new MessageItem() { Book = "Novossssso", Amount = 10 });
            ListContainer.ItemsSource = messages;
            Debug.WriteLine(messages.Count);
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void AddButton(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Debug.WriteLine(button.Tag);
        }

        private void ListContainer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

    public class MessageItem
    {
        public string Book { get; set; }
        public int Amount { get; set; }
    }
}
