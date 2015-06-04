using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
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
using Store;

namespace StoreApp
{
    /// <summary>
    /// Interaction logic for Purchase.xaml
    /// </summary>
    public partial class Purchase : Window
    {
        private Book book;
        private object _token;

        public Purchase(object o)
        {
            InitializeComponent();
            _token = o;
        }

        private void Button_Click_Purchase(object sender, RoutedEventArgs e)
        {
            if (book != null)
            {
                if (book.Quantity <= 0)
                {
                    BookNoStockDialog dialog = new BookNoStockDialog(book.Title)
                    {
                        Title = "No stock available!",
                        ShowInTaskbar = false,
                        ResizeMode = ResizeMode.NoResize,
                        Topmost = true,
                        Owner = this
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        MessageBox.Show("You said: " + dialog.WillSend.ToString());
                        if (dialog.WillSend)
                        {
                              new FrontEndService().AddOrder(new Order(book.Title, 1, dialog.User.Id), _token.ToString());
                        }
                    }
                }
                else
                {
                    //print receipt
                }
            }
            else
            {
                WarningBox.Text = "No book selected!";
            }
        }

        private void Button_Click_Search(object sender, RoutedEventArgs e)
        {
            var bookName = BookBox.Text;
            DatabaseConnector client = new DatabaseConnector("mongodb://tdin:tdin@ds031942.mongolab.com:31942/", "store");
            var collection = client.Database.GetCollection<Book>("books");
            var list = collection.Find(x => x.Title.Equals(bookName)).ToListAsync();
            list.Wait();
            if (list.Result.Count == 1) {
                book = list.Result[0];
                WarningBox.Text = "";
                TitleBox.Text = "Title: " + book.Title;
                AuthorBox.Text = "Author: " + book.Author;
                PriceBox.Text = "Price: " + book.Price;
                QuantityBox.Text = "Stock: " + book.Quantity;
            }
            if (list.Result.Count < 1)
            {
                WarningBox.Text = "No record of book with title: " + bookName;
                book = null;
                TitleBox.Text = "";
                AuthorBox.Text = "";
                PriceBox.Text = "";
                QuantityBox.Text = "";
            }
        }
    }
}
