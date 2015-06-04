using System;
﻿using System.Windows;
﻿using Common;
﻿using MongoDB.Driver;
﻿using Store;

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
                var amount = Convert.ToInt32(AmountBox.Text);
                if (book.Quantity <= 0 || book.Quantity < amount)
                {
                    BookNoStockDialog dialog = new BookNoStockDialog(book.Title)
                    {
                        Title = "No stock available!",
                        ShowInTaskbar = false,
                        ResizeMode = ResizeMode.NoResize,
                        Owner = this
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        if (dialog.WillSend)
                        {
                            new FrontEndService().AddOrder(new Order(book.Title, amount, dialog.User.Id), _token.ToString());
                        }
                    }
                }
                else
                {
                    BookPickClient dialog = new BookPickClient()
                    {
                        Title = "Input Client Name",
                        ShowInTaskbar = false,
                        ResizeMode = ResizeMode.NoResize,
                        Owner = this
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        if (dialog.WillSend)
                        {
                            new FrontEndService().printReceipt(new Order(book.Title, amount, new Client(dialog.user,"asdfgh","","").Id));
                        }
                    }
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
