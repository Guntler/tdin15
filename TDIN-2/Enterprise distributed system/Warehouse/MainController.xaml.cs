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
            InitializeComponent();
        }

        private void CheckBoxZone_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkedItem = (CheckBox)e.OriginalSource;
            if (checkedItem.Content != null)
            {
                foreach (var msg in MessageList.Where(msg => msg.Book.Title.Equals(checkedItem.Content)))
                {
                    _selectedMessage = msg;
                    break;
                }

            }
        }

        private void btnShowSelectedItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(_selectedMessage.ToString());
        }

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            _allSelected = true;
        }

        private void BtnRestock_OnClick(object sender, RoutedEventArgs e)
        {
            if (_allSelected)
            {
                foreach (var msg in MessageList)
                {
                    handleShipment(msg);
                }
            }
            else
            {
                handleShipment(_selectedMessage);   
            }
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
            }
            else
            {
                Debug.WriteLine("Cant handle a null msg");
            }
        }
    }
}
