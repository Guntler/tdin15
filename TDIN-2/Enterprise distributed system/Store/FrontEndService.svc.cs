using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel.Web;
using System.Text;
using System.Web.Script.Serialization;
using Common;
using MongoDB.Driver;

namespace Store
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "FrontEndService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select FrontEndService.svc or FrontEndService.svc.cs at the Solution Explorer and start debugging.
    public class FrontEndService : IFrontEndService
    {

        private Dictionary<string, object> _result;
        private JavaScriptSerializer s = new JavaScriptSerializer();

        public Stream Login(Client cliente)
        {
            _result = new Dictionary<string, object>();
            try
            {
                DatabaseConnector client = new DatabaseConnector("tdin", "tdin", "store");
                var collection = client.Database.GetCollection<Client>("clients");
                var list = collection.Find(x => x.Username == cliente.Username).ToListAsync();
                list.Wait();
                if (list.Result.Count == 1)
                {
                    if (list.Result[0].Password.Equals(cliente.Password))
                    {
                        _result.Add("Client",list.Result[0]);
                        _result.Add("Token", 0009);
                    }
                    else
                    {
                        throw new Exception("Invalid credentials");
                    }
                }else{
                    throw new Exception("No match found");
                }
            }
            catch (Exception e)
            {
                if (WebOperationContext.Current != null)
                {
                    OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                    response.StatusCode = HttpStatusCode.NotAcceptable;
                }
                _result.Add("Error", true);
                _result.Add("Reason", e.Message);
            }
            string result = s.Serialize(_result);
            if (WebOperationContext.Current != null)
                WebOperationContext.Current.OutgoingResponse.ContentType = "application/json; charset=utf-8";
            return new MemoryStream(Encoding.UTF8.GetBytes(result));
        }

        public Stream Register(Client cliente)
        {
            _result = new Dictionary<string, object>();
            try
            {
                if (cliente.Username.Equals("") || cliente.Password.Equals(""))
                    throw new Exception(string.Format("Invalid user-password credentials, provided user='{0}', password='{1}'.", cliente.Username, cliente.Password));

                DatabaseConnector client = new DatabaseConnector("tdin", "tdin", "store");
                var collection = client.Database.GetCollection<Client>("clients");
                var list = collection.Find(x => x.Username == cliente.Username).ToListAsync();
                list.Wait();
                if(list.Result.Count>0)
                    throw new Exception("User already exists");

                Client aux = new Client(cliente.Username, cliente.Password);
                var queryResult = collection.InsertOneAsync(aux);
                queryResult.Wait();
                if (queryResult.IsCompleted)
                {
                    _result.Add("Text", "User registered sucessfully");
                    _result.Add("Data", aux);
                }
                else
                {
                    throw new Exception(string.Format("Failed to register user: \n{0}", queryResult));    
                }
            }
            catch (Exception e)
            {
                if (WebOperationContext.Current != null)
                {
                    OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                    response.StatusCode = HttpStatusCode.NotAcceptable;
                }
                _result.Add("Error", true);
                _result.Add("Reason", e.Message);
            }
            string result = s.Serialize(_result);
            if (WebOperationContext.Current != null)
                WebOperationContext.Current.OutgoingResponse.ContentType = "application/json; charset=utf-8";
            return new MemoryStream(Encoding.UTF8.GetBytes(result));


        }
        /*
        public Collection<Order> GetOrderByClient(string clientId)
        {
            throw new NotImplementedException();
        }

        public Order AddOrder(string title, int quantity, string clientId)
        {
            throw new NotImplementedException();
        }

        public bool CancelOrder(string id)
        {
            throw new NotImplementedException();
        }

        public Order ChangeOrder(Order newOrder, string id)
        {
            throw new NotImplementedException();
        }

        public Collection<Book> GetBooks()
        {
            throw new NotImplementedException();
        }

        public Book GetBookByTitle(string title)
        {
            throw new NotImplementedException();
        }
         */
    }
}
