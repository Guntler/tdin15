using System;
using System.Windows;
using System.Windows.Threading;
using Common;

namespace Warehouse
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class GUI : Window
    {
        private LoginView login;
        private MainView main;
        public GUI()
        {
            InitializeComponent();
            login = new LoginView(this);
            main = new MainView(this);
            ShowLogin();
        }

        public void ShowLogin()
        {
            ContentHolder.Content = login;
        }

        public void ShowMain()
        {
            ContentHolder.Content = main;
        }

        public static void AddMsgToList(Message msg)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                (Action)delegate {
                    MainView.MessageList.Add(msg);
                });
        }
    }
}
