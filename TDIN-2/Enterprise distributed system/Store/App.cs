using System;
using System.ServiceModel.Web;

namespace Store
{
    public class App
    {
        private static void Main(string[] args)
        {
            WebServiceHost host = new WebServiceHost(typeof(FrontEndService));
            host.Open();
            Console.WriteLine("Rest service running");
            Console.WriteLine("Press ENTER to stop the service");
            Console.ReadLine();
            host.Close();

        }
    }
}