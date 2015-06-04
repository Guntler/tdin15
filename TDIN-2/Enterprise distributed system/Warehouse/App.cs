using System;
using System.ServiceModel;
using System.Windows;

namespace Warehouse
{
    public class App : Application
    {
        [STAThread]
        public static void Main()
        {
            App app = new App();
            app.StartupUri = new Uri("GUI.xaml", UriKind.Relative);
            ServiceHost host = new ServiceHost(typeof(WarehouseService));
            host.Open();
            app.Run();

        }

    }
}
