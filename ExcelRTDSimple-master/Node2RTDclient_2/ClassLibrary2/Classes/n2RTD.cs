/*
Project             :   alignmentsystems.node2rtd 
Design              :   An Excel RTD client side server component that connects to a node.js example of a simplified financial markets real-time data server 
Typical use case    :   Allow clients of an financial services firm to see the current bid and ask prices of stocks in their portfolio in real time
Author              :   John Greenan
Email               :   enquire@alignment-systems.com
Date                :   26th October 2014
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using XL=Microsoft.Office.Interop.Excel;

namespace alignmentsystems.n2excel
{

    [ComVisibleAttribute(true)]
    [
    Guid("B856EB46-BCA3-4A44-9DC9-8995C37680CB"),
    ProgId("alignmentsystems.node2rtd2"),
    ]
    public class n2RTD: XL.IRtdServer
    {
        private readonly Dictionary<int, IncrementUpwards> _topics = new Dictionary<int, IncrementUpwards>();
        private Timer _timer;

        public dynamic ConnectData(int TopicID, ref Array Strings, ref bool GetNewValues)
        {
            var start = Convert.ToInt32(Strings.GetValue(0).ToString());
            GetNewValues = true;
            _topics[TopicID] = new IncrementUpwards { CurrentValue = start };
            return start;
        }

        public void DisconnectData(int TopicID)
        {
            _topics.Remove(TopicID);
        }

        public int Heartbeat()
        {
            return 1;
        }

        public Array RefreshData(ref int TopicCount)
        {
            var data = new object[2, _topics.Count];
            var index = 0;

            foreach(var entry in _topics)
            {
                ++entry.Value.CurrentValue;
                data[0, index] = entry.Key;
                data[1, index] = entry.Value.CurrentValue;
                ++index;
            }
            TopicCount = _topics.Count;
            return data;
        }

        public int ServerStart(XL.IRTDUpdateEvent CallbackObject)
        {
            _timer = new Timer(delegate { CallbackObject.UpdateNotify(); }, null, TimeSpan.Zero, TimeSpan.FromSeconds(3));
            return 1;
        }

        public void ServerTerminate()
        {
            _timer.Dispose();
        }
    }
}
