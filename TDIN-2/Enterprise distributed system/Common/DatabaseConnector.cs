using System;
using MongoDB.Driver;

namespace Common
{
    public class DatabaseConnector
    {
        public IMongoDatabase Database { get; set; }
        public DatabaseConnector(string username, string password, string databaseName)
        {
            string connectionUrl = String.Format("mongodb://{0}:{1}@ds031942.mongolab.com:31942/{2}", username, password, databaseName);
            var client = new MongoClient(connectionUrl);
            Database = client.GetDatabase(databaseName);
        }
    }
}
