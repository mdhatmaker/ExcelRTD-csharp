using ExcelDna.Integration;
using RTD.Server;
using System.ServiceModel;
using ExcelDna.Integration.RxExcel;
using System.Reactive.Linq;
using System;
using System.Reactive.Disposables;
using System.Reflection;

namespace RTD.Excel {

    public class RTDAddIn : IExcelAddIn, IRTDClient {

        // connection to server
        static IRTDServer _server;

        public void AutoOpen() {

            // setup support for RTD functions
            ExcelAsyncUtil.Initialize();

            // setup error handler
            ExcelIntegration.RegisterUnhandledExceptionHandler(ex => "!!! EXCEPTION: " + ex.ToString());

            // setup connection to server
            var a = new EndpointAddress("net.tcp://localhost:8080/hello");
            var b = new NetTcpBinding();
            var f = new DuplexChannelFactory<IRTDServer>(this, b, a);
            _server = f.CreateChannel();
            _server.Register(); // this makes the server get a callback channel to us so it can call SendValue

            // increase RTD refresh rate since the 2 seconds default is too slow (move as setting to ribbon later)
            object app;
            object rtd;
            app = ExcelDnaUtil.Application;
            rtd = app.GetType().InvokeMember("RTD", BindingFlags.GetProperty, null, app, null);
            rtd.GetType().InvokeMember("ThrottleInterval", BindingFlags.SetProperty, null, rtd, new object[] { 100 });

        }

        public void AutoClose() {
            _server.UnRegister();
            ExcelAsyncUtil.Uninitialize();
        }

// event boilerplate stuff
public delegate void ValueSentHandler(ValueSentEventArgs args);
public static event ValueSentHandler OnValueSent;
public class ValueSentEventArgs : EventArgs {
    public double Value { get; private set; }
    public ValueSentEventArgs(double Value) {
        this.Value = Value;
    }
}

// this gets called by the server if there is a new value
public void SendValue(double x) {

    // invert method call from WCF into event for Rx
    if (OnValueSent != null)
        OnValueSent(new ValueSentEventArgs(x));
}

        // this function accepts an observer and calls OnNext on it every time the OnValueSent event is raised
        // this is the part that chains individual events into streams
        // it returns an IDisposable that can be called after the stream is complete 
static Func<IObserver<double>, IDisposable> Event2Observable = observer => {
    OnValueSent += d => observer.OnNext(d.Value);   // when a new value was sent, call OnNext on the Observers.
    return Disposable.Empty;                        // we'll keep listening until Excel is closed, there's no end to this stream
};
        
// this calls the wrapper to actually put it all together
[ExcelFunction("Gets realtime values from server")]
public static object GetValues() {
            
    // a delegate that creates an observable over Event2Observable
    Func<IObservable<double>> f2 = () => Observable.Create<double>(Event2Observable);

    //  pass that to Excel wrapper   
    return RxExcel.Observe("GetValues", null, f2);          
}
    }
}
