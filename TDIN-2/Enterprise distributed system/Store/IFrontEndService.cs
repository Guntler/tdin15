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
        Stream Login(Client cliente);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "api/register")]
        Stream Register(Client cliente);

        [OperationContract]
        [WebInvoke(Method = "GET",
            UriTemplate = "api/books/{token}", 
            ResponseFormat=WebMessageFormat.Json)]
        [Description("Gets all books available.")]
        Stream GetBooks(string token);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "api/book")]
        Stream AddBook(Book book);
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

        //List all books
        [OperationContract]
        Collection<Book> GetBooks();

        [OperationContract]
        Book GetBookByTitle(string title);
         * */
    }
}
