﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Windows;
using Common;

namespace Warehouse
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service" in both code and config file together.
    public class WarehouseService : IWarehouseService
    {
        public void SendToWarehouse(Message msg)
        {
            GUI.AddMsgToList(msg);
            this.addToDatabase(msg);
        }

        private void addToDatabase(Message msg)
        {
            DatabaseConnector client = new DatabaseConnector("mongodb://tdin:tdin@ds031812.mongolab.com:31812/", "warehouse");
            var collection = client.Database.GetCollection<Message>("requests");
            var task = collection.InsertOneAsync(msg);
        }
    }
}
