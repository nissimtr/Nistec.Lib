//licHeader
//===============================================================================================================
// System  : Nistec.Lib - Nistec.Lib Class Library
// Author  : Nissim Trujman  (nissim@nistec.net)
// Updated : 01/07/2015
// Note    : Copyright 2007-2015, Nissim Trujman, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is part of nistec library.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: http://nistec.net/license/nistec.cache-license.txt.  
// This notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who      Comments
// ==============================================================================================================
// 10/01/2006  Nissim   Created the code
//===============================================================================================================
//licHeader|
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Threading;
using System.Messaging;

using System.Runtime.InteropServices;
using System.ComponentModel;
#pragma warning disable CS1591
namespace Nistec.Threading
{
    /// <summary>
    /// ThreadTimer
    /// </summary>
    public class ThreadTimer : System.Timers.Timer
    {
 
        private DateTime timeStart;
        private bool stop;
        private TimeSpan timeSpan;
        private string currentTime;
        
        private DateTime signalTime;
        public Action<DateTime> Function { get; set; }
          
        /// <summary>
        /// ThreadTimer
        /// </summary>
        public ThreadTimer():this(100)
        {

        }

        /// <summary>
        /// ThreadTimer ctor
        /// </summary>
        /// <param name="interval"></param>
        public ThreadTimer(long interval)
            : base((double)interval)
        {
            stop = false;
            this.Elapsed += new System.Timers.ElapsedEventHandler(ThreadTimer_Elapsed);
            //// Keep the timer alive until the end of Main.
            //GC.KeepAlive(this);

        }

        void ThreadTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            signalTime = e.SignalTime;
            OnElapsed(e);
        }
  
        /// <summary>
        /// OnElapsed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnElapsed(System.Timers.ElapsedEventArgs e)
        {
            if (Function != null)
                Function(e.SignalTime);
        }

        private void SetTimeElapsed()
        {
            DateTime dtn = DateTime.Now;
            timeSpan = dtn.Subtract(timeStart);
            currentTime = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        }

        /// <summary>
        /// Wait
        /// </summary>
        public void Wait(long interval)
        {
            Start();
            TimeSpan ts = DateTime.Now.Subtract(timeStart);
            while (!stop &&  (long)ts.TotalMilliseconds < interval)//(tickCount == 0)
            {
                ts = DateTime.Now.Subtract(timeStart);
                Thread.Sleep(10);
            }
            Stop();
        }

        /// <summary>
        /// RestartTimer
        /// </summary>
        public void RestartTimer()
        {
            Stop();
            Start();
        }

        /// <summary>
        /// Start
        /// </summary>
        public new void Start()
        {
            base.Start();
            signalTime= timeStart=DateTime.Now;
        }


       /// <summary>
        /// CurrentTimeSpan
       /// </summary>
        public TimeSpan CurrentTimeSpan
        {
            get { return timeSpan; }
        }
        /// <summary>
        /// SignalTime
        /// </summary>
        public DateTime SignalTime
        {
            get { return signalTime; }
        }
        /// <summary>
        /// CurrentDisplayTime
        /// </summary>
        public string CurrentDisplayTime
        {
            get 
            {
                SetTimeElapsed();
                return currentTime; 
            }
        }
    }
}
