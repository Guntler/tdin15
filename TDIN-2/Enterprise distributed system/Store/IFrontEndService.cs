using System;
using System.Collections.Generic;
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
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/login")]
        string Login(Client cliente);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "/register")]
        AjaxDictionary<string, object> Register(Client cliente);
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
