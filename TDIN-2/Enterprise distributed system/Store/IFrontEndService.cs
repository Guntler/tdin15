using System;
using System.ComponentModel;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;
using Common;

namespace Store
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IFrontEndService" in both code and config file together.
    [ServiceContract]
    public interface IFrontEndService
    {
        [OperationContract]
        [WebInvoke(Method = "GET",
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/{*content}")]
        Stream StaticContent(string content);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "api/login")]
        [Description("Validates login for a given user and returns a session token used for authentication of the api")]
        Stream Login(Client cliente);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "api/register")]
        [Description("Registers a client, all fields (username, password, email, address) must be filled")]
        Stream Register(Client cliente);

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "api/books/{token}", 
            ResponseFormat=WebMessageFormat.Json)]
        [Description("Lists all books available.")]
        Stream GetBooks(string token);

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "api/books/{title}/{token}",
            ResponseFormat = WebMessageFormat.Json)]
        [Description("Returns a book with the specified title.")]
        Stream GetBookByTitle(string title, string token);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "api/book")]
        Stream AddBook(Book book);

        [OperationContract]
        [WebInvoke(Method = "POST",
           RequestFormat = WebMessageFormat.Json,
           ResponseFormat = WebMessageFormat.Json,
           BodyStyle = WebMessageBodyStyle.Bare,
           UriTemplate = "api/order/{token}")]
        [Description("")]
        Stream AddOrder(Order order, string token);

        [OperationContract]
        Stream GetOrderByClient(string clientId);

        /*
        //CRUD OPs Order
        [OperationContract]
        Collection<Order> GetOrderByClient(string clientId);

        [OperationContract]
        Order AddOrder(string title, int quantity, string clientId);

        [OperationContract]
        Boolean CancelOrder(string id);
        
        [OperationContract]
        Order ChangeOrder(Order newOrder, string id);
        
         * */
    }
}
