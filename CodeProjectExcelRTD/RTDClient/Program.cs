using RTD.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace RTD.Client {

    class Program {
                
        static void Main(string[] args) {
            
            var client = new RTDClient("net.tcp://localhost:8080/hello");
            Console.WriteLine("Client is up, press <Return> to close");
            Console.ReadLine();

            client.Close();
        }
    }

    public class RTDClient : IRTDClient {

        IRTDServer _server;

        public RTDClient(string address) {
            var a = new EndpointAddress(address);
            var b = new NetTcpBinding();
            var f = new DuplexChannelFactory<IRTDServer>(this, b, a);
            _server = f.CreateChannel();
            _server.Register();
        }
        
        public void SendValue(double x) {
            Console.WriteLine("We received the number " + x.ToString());
        }

        public void Close() {
            // remove this client from the server's client list
            _server.UnRegister();
        }
    }
}
