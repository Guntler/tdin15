using System;
using MongoDB.Driver;

namespace Common
{
    public class DatabaseConnector
    {
        public IMongoDatabase Database { get; set; }
        public DatabaseConnector(string path, string databaseName)
        {
            string connectionUrl = String.Format("{0}{1}", path, databaseName);
            var client = new MongoClient(connectionUrl);
            Database = client.GetDatabase(databaseName);
        }
    }
}
