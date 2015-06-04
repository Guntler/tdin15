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
﻿using Common;
﻿using Store;


namespace StoreApp
{
    /// <summary>
    /// Interaction logic for Shippings.xaml
    /// </summary>
    public partial class Shippings : Window
    {
        List<MessageItem> messages = new List<MessageItem>(); 
        public Shippings()
        {
            InitializeComponent();
            this.DataContext = this;
            
            foreach(var m in FrontEndService.ReceivedMessages)
            {
                messages.Add(new MessageItem() { Book=m.Book.Title,Amount=m.Amount });
            }
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
            Book aux = new FrontEndService().GetBook(button.Tag.ToString());
            aux.Quantity = messages.Find(c => c.Book == button.Tag.ToString()).Amount;
            new FrontEndService().UpdateBook(aux);
            var m = FrontEndService.ReceivedMessages.Find(c => c.Book.Title == button.Tag.ToString());
            FrontEndService.ReceivedMessages.Remove(m);
            var d = messages.Find(c => c.Book == button.Tag.ToString());
            messages.Remove(d);
            DialogResult = true;
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
