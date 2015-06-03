using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Common;

namespace Warehouse
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service" in both code and config file together.
    public class WarehouseService : IWarehouseService
    {
        public void SendToWarehouse(Message msg)
        {
            Debug.WriteLine("SENDTOWAREHOUSE DATA RECEIVED");
            Debug.WriteLine(msg.ToString());
        }

        public void ShowMessage(string msg)
        {
            Debug.WriteLine(msg + " Received at: " + System.DateTime.Now.ToString());
        }
    }
}
