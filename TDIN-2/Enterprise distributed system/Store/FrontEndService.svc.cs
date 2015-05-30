using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public static readonly Dictionary<Guid, Client> LoggedinUsers = new Dictionary<Guid, Client>(); 
        private Dictionary<string, object> _result;
        private JavaScriptSerializer s = new JavaScriptSerializer();

        private Client validateClient(Guid token)
        {
            if (LoggedinUsers.ContainsKey(token))
            {
                return LoggedinUsers[token];
            }
            else
            {
                throw new Exception("Invalid token");
            }
        }

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
                        if (!LoggedinUsers.ContainsValue(cliente))
                        {
                            var newToken = Guid.NewGuid();
                            LoggedinUsers.Add(newToken, list.Result[0]);
                            _result.Add("Client", list.Result[0]);
                            _result.Add("Token", newToken);
                        }
                        else
                        {
                            throw new Exception("User already logged in!");
                        }
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

        public Stream GetBooks(string guid)
        {
            _result = new Dictionary<string, object>();
            try
            {
                Client user = validateClient(Guid.Parse(guid));
                DatabaseConnector client = new DatabaseConnector("tdin", "tdin", "store");
                var collection = client.Database.GetCollection<Book>("books");
                var list = collection.Find("").ToListAsync();
                list.Wait();
                _result.Add("Books",list.Result);
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

        public Stream AddBook(Book book)
        {
            _result = new Dictionary<string, object>();
            try
            {
                DatabaseConnector client = new DatabaseConnector("tdin", "tdin", "store");
                var collection = client.Database.GetCollection<Book>("books");
                var query = collection.InsertOneAsync(book);
                query.Wait();
                if (query.IsCompleted)
                {
                    _result.Add("Text", "Book add sucessfully");
                    _result.Add("Data", book);
                }
                else
                {
                    throw new Exception(string.Format("Failed to register book: \n{0}", query));
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
