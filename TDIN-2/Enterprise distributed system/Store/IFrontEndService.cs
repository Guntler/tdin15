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
        [WebInvoke(Method = "GET",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "api/logout/{token}")]
        [Description("Logout of user, identified by the pair token Client given.")]
        Stream Logout(string token);

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
        [WebInvoke(Method = "GET",
            UriTemplate = "api/orders/{token}",
            ResponseFormat = WebMessageFormat.Json)]
        [Description("Lists all orders made by the user identified in the token")]
        Stream GetOrdersForClient(string token);

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "api/orders/{id}/{token}",
            ResponseFormat = WebMessageFormat.Json)]
        [Description("Returns the order made by the user identified in the token and id")]
        Stream GetOrderById(string token, string id);

        [OperationContract]
        [WebInvoke(Method = "DELETE",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "api/order/{token}")]
        Stream CancelOrder(string id, string token);

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
