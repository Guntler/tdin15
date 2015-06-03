using System.ServiceModel;
using Common;

namespace Warehouse
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService" in both code and config file together.
    [ServiceContract]
    public interface IWarehouseService
    {
        [OperationContract(IsOneWay = true)]
        [TransactionFlow(TransactionFlowOption.Allowed)]
        void SendToWarehouse(Message msg);

        [OperationContract(IsOneWay = true)]
        void ShowMessage(string msg);
    }
}
