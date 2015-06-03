using System;
using System.Collections.Generic;
using System.IO;
using System.Messaging;
using System.Net;
using System.ServiceModel.Web;
using System.Text;
using System.Web.Script.Serialization;
using Common;
using MongoDB.Bson;
using MongoDB.Driver;
using Store.WarehouseService;
using Message = Common.Message;

namespace Store
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "FrontEndService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select FrontEndService.svc or FrontEndService.svc.cs at the Solution Explorer and start debugging.
    public class FrontEndService : IFrontEndService
    {
        public static readonly Dictionary<Guid, Client> LoggedinUsers = new Dictionary<Guid, Client>();
        private Dictionary<string, object> _result;
        private JavaScriptSerializer s = new JavaScriptSerializer();

        public Stream StaticContent(string content)
        {
            if (WebOperationContext.Current != null)
            {
                OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                string path = "public/" + (string.IsNullOrEmpty(content) ? "index.html" : content);
                string extension = Path.GetExtension(path);
                string contentType = string.Empty;

                switch (extension)
                {
                    case ".htm":
                    case ".html":
                        contentType = "text/html";
                        break;
                    case ".jpg":
                        contentType = "image/jpeg";
                        break;
                    case ".png":
                        contentType = "image/png";
                        break;
                    case ".js":
                        contentType = "application/javascript";
                        break;
                    case ".css":
                        contentType = "text/css";
                        break;
                }

                if (File.Exists(path) && !string.IsNullOrEmpty(contentType))
                {
                    response.ContentType = contentType;
                    response.StatusCode = HttpStatusCode.OK;
                    return File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                }
                response.StatusCode = HttpStatusCode.NotFound;
                return null;
            }
            return null;
        }

        private Client validateClient(Guid token)
        {
            if (LoggedinUsers.ContainsKey(token))
            {
                return LoggedinUsers[token];
            }
            throw new Exception("Invalid token");
        }

        private Book getBook(string title)
        {
            DatabaseConnector client = new DatabaseConnector("mongodb://tdin:tdin@ds031942.mongolab.com:31942/", "store");
            var collection = client.Database.GetCollection<Book>("books");
            var list = collection.Find(x => x.Title.Equals(title)).ToListAsync();
            list.Wait();
            if (list.Result.Count == 1)
                return list.Result[0];
            if (list.Result.Count < 1)
            {
                throw new Exception("No record of book with title: " + title);
            }
            throw new Exception("something went wrong with the book registration: " + list.Result.ToArray());
        }

        public Stream Login(Client cliente)
        {
            _result = new Dictionary<string, object>();
            try
            {
                DatabaseConnector client = new DatabaseConnector("mongodb://tdin:tdin@ds031942.mongolab.com:31942/", "store");
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
                }
                else
                {
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

        public Stream Logout(string token)
        {
            _result = new Dictionary<string, object>();
            try
            {
                Client user = validateClient(Guid.Parse(token));
                DatabaseConnector client = new DatabaseConnector("mongodb://tdin:tdin@ds031942.mongolab.com:31942/", "store");
                var collection = client.Database.GetCollection<Client>("clients");
                var list = collection.Find(x => x.Username == user.Username).ToListAsync();
                list.Wait();
                if (list.Result.Count == 1)
                {
                    if (list.Result[0].Password.Equals(user.Password))
                    {
                        if (!LoggedinUsers.ContainsValue(user))
                        {
                            throw new Exception("User not logged in!");
                        }
                        LoggedinUsers.Remove(Guid.Parse(token));
                        _result.Add("success", "user: " + user.Username + " logged off");
                    }
                    else
                    {
                        throw new Exception("Invalid credentials");
                    }
                }
                else
                {
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
                DatabaseConnector client = new DatabaseConnector("mongodb://tdin:tdin@ds031942.mongolab.com:31942/", "store");
                var collection = client.Database.GetCollection<Client>("clients");
                var list = collection.Find(x => x.Username == cliente.Username).ToListAsync();
                list.Wait();
                if (list.Result.Count > 0)
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

        public Stream GetBooks(string token)
        {
            _result = new Dictionary<string, object>();
            try
            {
                Client user = validateClient(Guid.Parse(token));
                DatabaseConnector client = new DatabaseConnector("mongodb://tdin:tdin@ds031942.mongolab.com:31942/", "store");
                var collection = client.Database.GetCollection<Book>("books");
                var list = collection.Find(x => x.Title != "").ToListAsync();
                list.Wait();
                _result.Add("Books", list.Result);
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

        public Stream GetBookByTitle(string title, string token)
        {
            _result = new Dictionary<string, object>();
            try
            {
                Client user = validateClient(Guid.Parse(token));
                DatabaseConnector client = new DatabaseConnector("mongodb://tdin:tdin@ds031942.mongolab.com:31942/", "store");
                var collection = client.Database.GetCollection<Book>("books");
                var list = collection.Find(x => x.Title == title).ToListAsync();
                list.Wait();
                _result.Add("Book", list.Result);
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
                DatabaseConnector client = new DatabaseConnector("mongodb://tdin:tdin@ds031942.mongolab.com:31942/", "store");
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
      
        public Stream UpdateBook(Book newBook)
        {
            _result = new Dictionary<string, object>();
            try
            {
                DatabaseConnector client = new DatabaseConnector("mongodb://tdin:tdin@ds031942.mongolab.com:31942/", "store");
                var collection = client.Database.GetCollection<Book>("books");
                var list = collection.Find(x => x.Title == newBook.Title).ToListAsync();
                list.Wait();
                _result.Add("updated book", list.Result);
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

        public Stream AddOrder(Order order, string token)
        {
            _result = new Dictionary<string, object>();
            try
            {
                Client user = validateClient(Guid.Parse(token));
                DatabaseConnector client = new DatabaseConnector("mongodb://tdin:tdin@ds031942.mongolab.com:31942/", "store");
                var collection = client.Database.GetCollection<Order>("orders");
                var book = getBook(order.Title);
                if (book.Quantity >= order.Quantity)
                {
                    var random = new Random();
                    var timestamp = DateTime.UtcNow;
                    var machine = random.Next(0, 16777215);
                    var pid = (short) random.Next(0, 32768);
                    var increment = random.Next(0, 16777215);
                    order.Id = new ObjectId(timestamp, machine, pid, increment).ToString();
                    order.State = new StateEnum();
                    order.State.CurrentState = StateEnum.State.Dispatched;
                    order.State.Date = DateTime.Now.AddDays(1);
                    var query = collection.InsertOneAsync(order);
                    query.Wait();
                    if (query.IsCompleted)
                    {
                        _result.Add("Text", "Order add sucessfully");
                        _result.Add("Data", order);
                        book.Quantity -= order.Quantity;
                        UpdateBook(book);
                    }
                    else
                    {
                        throw new Exception(string.Format("Failed to register order: \n{0}", query));
                    }
                }
                else //send to mq
                {
                    WarehouseServiceClient warehouse = new WarehouseServiceClient();
                    warehouse.SendToWarehouseAsync(new Message("restock", order.Quantity*10, book));
                    var random = new Random();
                    var timestamp = DateTime.UtcNow;
                    var machine = random.Next(0, 16777215);
                    var pid = (short)random.Next(0, 32768);
                    var increment = random.Next(0, 16777215);
                    order.Id = new ObjectId(timestamp, machine, pid, increment).ToString();
                    order.State = new StateEnum();
                    order.State.CurrentState = StateEnum.State.WaitingDispatch;
                    var query = collection.InsertOneAsync(order);
                    query.Wait();
                    if (query.IsCompleted)
                    {
                        _result.Add("Text", "Order add sucessfully");
                        _result.Add("Data", order);
                        book.Quantity -= order.Quantity;
                        UpdateBook(book);
                    }
                    else
                    {
                        throw new Exception(string.Format("Failed to register order: \n{0}", query));
                    }
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

        public Stream GetOrdersForClient(string token)
        {
            _result = new Dictionary<string, object>();
            try
            {
                Client user = validateClient(Guid.Parse(token));
                DatabaseConnector client = new DatabaseConnector("mongodb://tdin:tdin@ds031942.mongolab.com:31942/", "store");
                var collection = client.Database.GetCollection<Order>("orders");
                var list = collection.Find(x => x.ClientId.Equals(user.Id)).ToListAsync();
                list.Wait();
                _result.Add("Orders", list.Result);
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

        public Stream GetOrderById(string token, string id)
        {
            _result = new Dictionary<string, object>();
            try
            {
                Client user = validateClient(Guid.Parse(token));
                DatabaseConnector client = new DatabaseConnector("mongodb://tdin:tdin@ds031942.mongolab.com:31942/", "store");
                var collection = client.Database.GetCollection<Order>("orders");
                var list = collection.Find(x => x.Id.Equals(id)).ToListAsync();
                list.Wait();
                _result.Add("result", list.Result);
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

        public Stream CancelOrder(string id, string token)
        {
            _result = new Dictionary<string, object>();
            try
            {
                Client user = validateClient(Guid.Parse(token));
                DatabaseConnector client = new DatabaseConnector("mongodb://tdin:tdin@ds031942.mongolab.com:31942/", "store");
                var collection = client.Database.GetCollection<Order>("orders");
                var query = collection.FindOneAndDeleteAsync(x => x.Id == id);
                query.Wait();
                if (query.IsCompleted)
                {
                    _result.Add("Text", "Order canceled sucessfully");
                    _result.Add("Data", query.Result);
                }
                else
                {
                    throw new Exception(string.Format("Failed to cancel order: \n{0}", query));
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

    }
}
