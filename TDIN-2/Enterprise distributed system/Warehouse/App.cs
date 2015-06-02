using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Warehouse
{
    public class App : System.Windows.Application
    {
        [STAThread]
        public static void Main()
        {
            App app = new App();
            app.StartupUri = new System.Uri("MainWindow.xaml", System.UriKind.Relative);
            ServiceHost host = new ServiceHost(typeof(Warehouse.WarehouseService));
            host.Open();
            app.Run();

        }

    }
}
