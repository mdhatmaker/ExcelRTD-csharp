using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Timers;

namespace RTD.Server {

    class Program {
        static void Main(string[] args) {

            Uri baseAddress = new Uri("net.tcp://localhost:8080/hello");
            var server = new RTDServer();
            var host = new ServiceHost(server, baseAddress);
            host.Open();

            Console.WriteLine("The service is ready at {0}", baseAddress);
            Console.WriteLine("Press <Enter> to stop the service.");
            Console.ReadLine();

            host.Close();
        }

    }
}
