using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Timers;

namespace RTD.Server {

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class RTDServer : IRTDServer {

        List<IRTDClient> clients;
        Timer timer;
        Random rnd;

        public RTDServer() {
            clients = new List<IRTDClient>();
            rnd = new Random();

            // send random value to clients every 100ms
            timer = new Timer(100);
            timer.Elapsed += SendData;
            timer.Start();
        }

void SendData(object sender, ElapsedEventArgs e) {
            
    // create random number
    var next = rnd.NextDouble();
    Console.WriteLine("Sending " + next);
            
    // send number to clients
    int ix = 0;
    while (ix < clients.Count) {// can't do foreach because we want to remove dead ones
        var c = clients[ix];
        try { 
            c.SendValue(next);
            ix++;
        }
        catch (Exception e2) { Unregister(c); }
    } 
}
        
// for clients to make themselves known
public void Register() {
    Console.WriteLine("We have a guest...");
    var c = OperationContext.Current.GetCallbackChannel<IRTDClient>();
    if (!clients.Contains(c)) 
        clients.Add(c);
}

        // to grafefully close the connection from the client
        public void UnRegister() {            
            var c = OperationContext.Current.GetCallbackChannel<IRTDClient>();
            Unregister(c);
        }

        private void Unregister(IRTDClient c) {
            Console.WriteLine("A guest just left us...");
            if (clients.Contains(c))
                clients.Remove(c);
        }       
    }
}
