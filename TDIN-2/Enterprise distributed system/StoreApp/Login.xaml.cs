using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Common;
using MongoDB.Driver;
using Newtonsoft.Json;
using Store;

namespace StoreApp
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private FrontEndService service;
        public Login()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string user = UsernameBox.Text;
            string pass = PasswordBox.Password;
            Debug.WriteLine("User: " + user + ", pass: " + pass);
            DatabaseConnector client = new DatabaseConnector("mongodb://tdin:tdin@ds031942.mongolab.com:31942/", "store");
            var collection = client.Database.GetCollection<Client>("clients");
            var list = collection.Find(x => x.Username.Equals(user)).ToListAsync();
            list.Wait();
            if (list.Result.Count == 1)
            {
                var loginResult = new FrontEndService().Login(list.Result[0]);
                string json = new StreamReader(loginResult).ReadToEnd();
                Dictionary<string, object> output = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (list.Result[0].Password.Equals(pass))
                {
                    Landing obj = new Landing(list.Result[0], output["Token"]);

                    obj.Show();
                    Hide();
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
