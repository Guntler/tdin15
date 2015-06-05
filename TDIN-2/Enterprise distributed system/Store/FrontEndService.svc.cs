using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.ServiceModel.Web;
using System.Text;
using System.Web.Script.Serialization;
using Common;
using MongoDB.Bson;
using MongoDB.Driver;
using Store.WarehouseService;
using Mandrill;
using Mandrill.Models;
using Mandrill.Requests.Messages;

namespace Store
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "FrontEndService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select FrontEndService.svc or FrontEndService.svc.cs at the Solution Explorer and start debugging.
    public class FrontEndService : IFrontEndService
    {
        public static readonly Dictionary<Guid, Client> LoggedinUsers = new Dictionary<Guid, Client>();
        private Dictionary<string, object> _result;
        private JavaScriptSerializer s = new JavaScriptSerializer();
        public static List<Message> ReceivedMessages = new List<Message>();
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

        public static Book GetBook(string title)
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

                Client aux = new Client(cliente.Username, cliente.Password, cliente.Address, cliente.Email);
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

        public Stream RequestUpdateBook(Book newBook)
        {
            _result = new Dictionary<string, object>();
            try
            {
                Console.WriteLine("In updateBook: " + newBook);
                var updateresult = UpdateBook(newBook);
                _result.Add("updated book", updateresult);
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
                var book = GetBook(order.Title);
                if (book.Quantity >= order.Quantity)
                {
                    var random = new Random();
                    var timestamp = DateTime.UtcNow;
                    var machine = random.Next(0, 16777215);
                    var pid = (short)random.Next(0, 32768);
                    var increment = random.Next(0, 16777215);
                    order.Id = new ObjectId(timestamp, machine, pid, increment).ToString();
                    order.State = new StateEnum { CurrentState = StateEnum.State.Dispatched };
                    order.State.Date = DateTime.Now.AddDays(1);
                    var query = collection.InsertOneAsync(order);
                    query.Wait();
                    if (query.IsCompleted)
                    {
                        _result.Add("Text", "Order add sucessfully");
                        _result.Add("Data", order);
                        book.Quantity -= order.Quantity;
                        UpdateBook(book);
                        NotifyClient(order);
                    }
                    else
                    {
                        throw new Exception(string.Format("Failed to register order: \n{0}", query));
                    }
                }
                else //send to mq
                {
                    var msg = new Message("restock", order.Quantity * 10, book);
                    Console.WriteLine(msg.ToString());
                    try
                    {
                        WarehouseServiceClient warehouse = new WarehouseServiceClient();
                        warehouse.SendToWarehouseAsync(msg);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                        Debug.WriteLine(e.StackTrace);
                        throw;
                    }

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
                        _result.Add("Text", "Order could not be fulfilled, sent message to warehouse to restock");
                        _result.Add("Data", order);
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

        public Stream ReceiveFromWarehouse(Message order)
        {
            AddWarehouseMesage(order);
            Console.WriteLine(order.ToString());
            Console.WriteLine("ReceivedMessages: " + GetMessages().Count);
            _result = new Dictionary<string, object> { { "Success", "" } };
            string result = s.Serialize(_result);

            if (WebOperationContext.Current != null)
                WebOperationContext.Current.OutgoingResponse.ContentType = "application/json; charset=utf-8";
            if (WebOperationContext.Current != null)
            {
                OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
                response.StatusCode = HttpStatusCode.Accepted;
            }
            return new MemoryStream(Encoding.UTF8.GetBytes(result));
        }

        public static List<Message> GetMessages()
        {
            DatabaseConnector client = new DatabaseConnector("mongodb://tdin:tdin@ds031942.mongolab.com:31942/", "store");
            var collection = client.Database.GetCollection<Message>("messages");
            var list = collection.Find(m => m.Id != "").ToListAsync();
            list.Wait();
            return list.Result;
        }

        public static void AddWarehouseMesage(Message order)
        {
            DatabaseConnector client = new DatabaseConnector("mongodb://tdin:tdin@ds031942.mongolab.com:31942/", "store");
            var collection = client.Database.GetCollection<Message>("messages");
            var task = collection.InsertOneAsync(order);
            task.Wait();
        }

        public static void DeleteWarehouseMessage(string messageId)
        {
            DatabaseConnector client = new DatabaseConnector("mongodb://tdin:tdin@ds031942.mongolab.com:31942/", "store");
            var collection = client.Database.GetCollection<Message>("messages");
            var task = collection.DeleteOneAsync(o => o.Id.Equals(messageId));
            task.Wait();
        }

        public static List<Order> FindIncompleteOrdersByBook(string title)
        {
            DatabaseConnector client = new DatabaseConnector("mongodb://tdin:tdin@ds031942.mongolab.com:31942/", "store");
            var collection = client.Database.GetCollection<Order>("orders");
            var list = collection.Find(m => m.Title == title && m.State.CurrentState == StateEnum.State.WaitingDispatch).ToListAsync();
            list.Wait();
            return list.Result;
        }

        public static void UpdateOrder(Order order)
        {
            var book = GetBook(order.Title);
            DatabaseConnector client = new DatabaseConnector("mongodb://tdin:tdin@ds031942.mongolab.com:31942/", "store");
            var collection = client.Database.GetCollection<Order>("orders");
            if (book.Quantity >= order.Quantity)
            {
                order.State.CurrentState = StateEnum.State.Dispatched;
                order.State.Date = DateTime.Today;
                var task = collection.ReplaceOneAsync(o => o.Id.Equals(order.Id), order);
                task.Wait();
                book.Quantity -= order.Quantity;
                UpdateBook(book);
                Debug.WriteLine(order.ToString());
                NotifyClient(order);
            }
        }

        private static async void NotifyClient(Order ord)
        {
            var cliente = getClient(ord.ClientId);
            DatabaseConnector client = new DatabaseConnector("mongodb://tdin:tdin@ds031942.mongolab.com:31942/", "store");
            var collection = client.Database.GetCollection<Client>("clients");
            var list = collection.Find(x => x.Username.Equals(cliente.Username)).ToListAsync();
            list.Wait();
            if (list.Result.Count > 0)
            {
                MandrillApi api = new MandrillApi("9jetNeue5BSpjgF1DKotSQ");

                var mandrillRecipients = new List<EmailAddress>();
                mandrillRecipients.Add(new EmailAddress(cliente.Email));
                // add recipents to the mandrillRecipientsList
                var email = new EmailMessage
                {
                    To = mandrillRecipients,
                    FromEmail = "bookStore@tdin.com",
                    FromName = "Book Store Support",
                    Subject = "Estado de encomenda",
                    Text = "A sua encomenda foi atualizada\n" + ord.ToString(),
                    RawMessage = "A sua encomenda foi atualizada\n" + ord.ToString()
                };

                await api.SendMessage(new SendMessageRequest(email));
            }
            else
            {
                Debug.WriteLine("Could not find user: " + cliente.Username);
            }
        }

        public static Book UpdateBook(Book newBook)
        {
            DatabaseConnector client = new DatabaseConnector("mongodb://tdin:tdin@ds031942.mongolab.com:31942/", "store");
            var collection = client.Database.GetCollection<Book>("books");
            var task = collection.ReplaceOneAsync(o => o.Title == newBook.Title, newBook);
            task.Wait();
            var list = collection.Find(x => x.Title == newBook.Title).ToListAsync();
            list.Wait();
            return list.Result[0];

        }

        public static void PrintReceipt(Order ord)
        {
            var orderClient = getClient(ord.ClientId);
            var orderBook = GetBook(ord.Title);
            Debug.WriteLine("New receipt for order\n");
            Debug.WriteLine("\tId: " + ord.Id + "\n");
            Debug.WriteLine("\tState: " + ord.State.CurrentState + "\n");
            Debug.WriteLine("\tDate: " + ord.State.Date + "\n");
            Debug.WriteLine("\tFor client: \n");
            Debug.WriteLine("\t\tId:" + orderClient.Id + "\n");
            Debug.WriteLine("\t\tUsername:" + orderClient.Username + "\n");
            Debug.WriteLine("\t\tEmail:" + orderClient.Email + "\n");
            Debug.WriteLine("\t\tAddress:" + orderClient.Address + "\n");
            Debug.WriteLine("\tBook information: \n");
            Debug.WriteLine("\t\tTitle: " + orderBook.Title + "\n");
            Debug.WriteLine("\t\tAuthor: " + orderBook.Author + "\n");
            Debug.WriteLine("\t\tPrice per unit: " + orderBook.Price + "\n");
            Debug.WriteLine("\tQuantity: " + ord.Quantity + "\n");
            Debug.WriteLine("\tTotal price: " + ord.Quantity * orderBook.Price + "\n");
            Debug.WriteLine("\n\n");
        }

        private static Client getClient(string id)
        {
            DatabaseConnector client = new DatabaseConnector("mongodb://tdin:tdin@ds031942.mongolab.com:31942/", "store");
            var collection = client.Database.GetCollection<Client>("clients");
            Console.WriteLine("trying to find user of id:"+id);
            var task = collection.Find(cli => cli.Id.Equals(id)).ToListAsync();
            task.Wait();
            if (task.IsCompleted)
            {
                if (task.Result.Count == 1)
                {
                    return task.Result[0];
                }
                Debug.WriteLine("couldnt find unique user");
                return task.Result[0];
            }
            return null;
        }

    }
}
