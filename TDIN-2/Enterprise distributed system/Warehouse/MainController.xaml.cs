using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Common;
using MongoDB.Driver;

namespace Warehouse
{
    /// <summary>
    /// Interaction logic for MainController.xaml
    /// </summary>
    public partial class MainView
    {
        private GUI _parent;
        public static ObservableCollection<Message> MessageList = new ObservableCollection<Message>();
        private Message _selectedMessage;
        private bool _allSelected;
        public MainView(GUI parent)
        {
            _parent = parent;
            _allSelected = false;
            loadMessages();
            InitializeComponent();
        }

        private void loadMessages()
        {
            DatabaseConnector client = new DatabaseConnector("mongodb://tdin:tdin@ds031812.mongolab.com:31812/", "warehouse");
            var collection = client.Database.GetCollection<Message>("requests");
            var list = collection.Find(o => o.Action.Equals("restock")).ToListAsync();
            list.Wait();
            foreach (var msg in list.Result)
            {
                MessageList.Add(msg);

            }
        }

        private void CheckBoxZone_Checked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("CheckBoxZone!");
            CheckBox checkedItem = (CheckBox)e.OriginalSource;
            if (checkedItem.Content != null)
            {
                Debug.WriteLine(checkedItem);

                foreach (var msg in MessageList.Where(msg => msg.Id.Equals(checkedItem.Content)))
                {
                    Debug.WriteLine("Found the message");
                    _selectedMessage = msg;
                    break;
                }
            }
            else
            {
                Debug.WriteLine("ITS NULL");
                Debug.WriteLine(checkedItem);
            }
        }

        private void btnShowSelectedItem_Click(object sender, RoutedEventArgs e)
        {
            string text = String.Format("Message Received:\n\t Id: {0}\nBook information\n\tTitle: {1}\n\tAuthor: {2}\n\tQuantity available: {3}\nAmount to restock: {4}", _selectedMessage.Id,_selectedMessage.Book.Title, _selectedMessage.Book.Author, _selectedMessage.Book.Quantity, _selectedMessage.Amount);
            MessageBox.Show(text);
        }

        private void BtnRestock_OnClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(_selectedMessage.ToString());
            handleShipment(_selectedMessage);
        }

        private void handleShipment(Message msg)
        {
            if (msg != null)
            {
                var httpWebRequest =
                    (HttpWebRequest) WebRequest.Create("http://localhost:8700/Store/api/shipment");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof (Message));
                    MemoryStream ms = new MemoryStream();
                    var shipment = new Message("new stock", msg.Amount, msg.Book);
                    ser.WriteObject(ms, shipment);
                    string jsonString = Encoding.UTF8.GetString(ms.ToArray());
                    ms.Close();
                    streamWriter.Write(jsonString);
                }

                var httpResponse = (HttpWebResponse) httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    Debug.WriteLine("result of shipment request: ");
                    Debug.WriteLine(result);
                }

                if (httpResponse.StatusCode == HttpStatusCode.Accepted)
                {
                    DatabaseConnector client = new DatabaseConnector(
                        "mongodb://tdin:tdin@ds031812.mongolab.com:31812/", "warehouse");
                    var collection = client.Database.GetCollection<Message>("requests");
                    var list = collection.DeleteOneAsync(o => o.Id.Equals(msg.Id));
                    list.Wait();
                    MessageList.Remove(msg);
                }
                else
                {
                    Debug.WriteLine("Store did not accept shipment");
                    Console.WriteLine(httpResponse.StatusCode);
                }

            }
            else
            {
                Debug.WriteLine("Cant handle a null msg");
            }
        }
    }
}
