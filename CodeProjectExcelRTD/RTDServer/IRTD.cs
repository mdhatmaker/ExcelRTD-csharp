using System.ServiceModel;

namespace RTD.Server {

    [ServiceContract(CallbackContract = typeof(IRTDClient))]
    public interface IRTDServer {

        [OperationContract(IsOneWay = true)]
        void Register();

        [OperationContract(IsOneWay = true)]
        void UnRegister();

    }

    [ServiceContract]
    public interface IRTDClient {

        [OperationContract(IsOneWay = true)]
        void SendValue(double x);

    }
}
