using System.ServiceModel;

namespace Dev2 {
    [ServiceContract(CallbackContract=typeof(IFrameworkDuplexCallbackChannel))]
    public interface IFrameworkDuplexDataChannel {
        [OperationContract(IsOneWay=true)]
        void Register(string userName);

        [OperationContract(IsOneWay=true)]
        void Unregister(string userName);

        [OperationContract]
        void ShowUsers(string userName);

        [OperationContract(IsOneWay=true)]
        void SendMessage(string userName, string message);

        [OperationContract(IsOneWay=true)]
        void SendPrivateMessage(string userName, string targetUserName, string message);

        [OperationContract(IsOneWay = true)]
        void SetDebug(string userName, string serviceName, bool debugOn);

        [OperationContract]
        void Rollback(string userName, string serviceName, int versionNo);

        [OperationContract(IsOneWay = true)]
        void Rename(string userName, string resourceType, string resourceName, string newResourceName);

        [OperationContract]
        void Reload();

        [OperationContract]
        void ReloadSpecific(string userName, string resourceName);

    }
}
