using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Excel;
using System.Windows.Forms;

namespace Eagle7
{
    [
        Guid("D4E389BB-E1F0-423C-909D-95A4F877D13E"),
        ProgId("E7.Dev.RtdServer"),
        ComVisible(true)
    ]
    public class E7RtdServer : IRtdServer
    {
        private IRTDUpdateEvent m_callback;
        private Timer m_timer;
        private int m_topicId;

        /* ServerStart is the first method called by Excel and is where we prepare the RTD server. In particular we set
         * the callback member variable and prepare the timer. Notice that the timer is not yet enabled.
         * Returning 1 indicates that everything is fine. */
        public int ServerStart(IRTDUpdateEvent callback)
        {
            m_callback = callback;
            m_timer = new Timer();
            m_timer.Tick += timer_Tick;
            m_timer.Interval = 2000;
            return 1;
        }

        /* ServerTerminate is called when Excel is ready to unload the RTD server.
         * Here we simply release the timer. */
        public void ServerTerminate()
        {
            if (null != m_timer)
            {
                m_timer.Dispose();
                m_timer = null;
            }
        }

        /* ConnectData is called for each “topic” that Excel wishes to “subscribe” to. It is called once for every unique
         * subscription. As should be obvious, this implementation assumes there will only be a single topic. In a future
         * post I’ll talk about handling multiple topics. ConnectData also starts the timer and returns an initial value
         * that Excel can display.*/
        public object ConnectData(int topicId, ref Array strings, ref bool newValues)
        {
            m_topicId = topicId;
            m_timer.Start();
            return GetTime();
        }

        /* DisconnectData is called to tell the RTD server that Excel is no longer interested in data for the particular topic.
         * In this case, we simply stop the timer to prevent the RTD server from notifying Excel of any further updates. */
        public void DisconnectData(int topicId)
        {
            m_timer.Stop();
        }

        /* TimerEventHandler is the private method that is called when the timer Tick event is raised. It stops the timer and
         * uses the callback interface to let Excel know that updates are available. Stopping the timer is important since we
         * don’t want to call UpdateNotify repeatedly. */
        void timer_Tick(object sender, EventArgs e)
        {
            m_timer.Stop();
            m_callback.UpdateNotify();
        }

        /* RefreshData is called when Excel is ready to retrieve any updated data for the topics that it has previously subscribed
         * to via ConnectData. The implementation looks a bit strange. That’s mainly because Excel is expecting the data as a
         * COM SAFEARRAY. Although it isn’t pretty, The CLR’s COM infrastructure does a commendable job of marshalling the data
         * for you. All you need to do is populate the two-dimensional array with the topic Ids and values and set the topicCount
         * parameter to the number of topics that are included in the update.
         * Finally, the timer is restarted before returning the data. */
        public Array RefreshData(ref int topicCount)
        {
            object[,] data = new object[2, 1];
            data[0, 0] = m_topicId;
            data[1, 0] = GetTime();

            topicCount = 1;

            m_timer.Start();
            return data;
        }

        /* Heartbeat is called by Excel if it hasn’t received any updates recently in an attempt to determine whether your
         * RTD server is still OK. Returning 1 indicates that everything is fine. */
        public int Heartbeat()
        {
            return 1;
        }

        /* GetTime is a private method used to get a formatted time string that represents the data to display in Excel. As you
         * can imagine, this RTD server simply updates the time in the cell roughly every two seconds. */
        private string GetTime()
        {
            return DateTime.Now.ToString("hh:mm:ss:ff");
        }


    } // END OF CLASS
} // END OF NAMESPACE
