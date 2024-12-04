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
using System.Security;
using Nistec.Generic;
using System.Collections.Concurrent;
#pragma warning disable  CS1591

namespace Nistec.Threading
{
    /// <summary>
    /// Provides an thread pool that can resume or suspend threads in pool.
    /// </summary>
    public class GenericThreadPool : IDisposable 
    {

        public enum GenericThradState
        {
            Idle = 0,
            WorkBegin = 1,
            WorkEnd = 2
        }

        class GTState
        {
            public DateTime LastActivated;
            public GenericThradState State;
            public bool Started;

            public GTState()
            {
                LastActivated = DateTime.Now;
                State = GenericThradState.Idle;
                Started = false;

            }

            public void Suspend()
            {
                Started = false;
                State = GenericThradState.Idle;
            }
            public long GetIdlTime()
            {
                try
                {
                    return Convert.ToInt64(DateTime.Now.Subtract(LastActivated).TotalSeconds);
                }
                catch
                {
                    return 0;
                }
            }
            public bool CanResume()
            {
                return !Started && State == GenericThradState.Idle;
            }
            public bool CanSuspend(int idlTimeout)
            {
                long idleTime = GetIdlTime();
                return Started && State == GenericThradState.Idle && idleTime > idlTimeout;
            }
        }

        #region Members

        private bool initilized = false;
        /// <summary>
        /// Hashtable of all the threads in the thread pool.
        /// </summary>
        private Hashtable workerThreads = Hashtable.Synchronized(new Hashtable());
        //private ConcurrentDictionary<string, Thread> workerThreads = new ConcurrentDictionary<string, Thread>();

        /// <summary>
        /// Hashtable of all the threads in the thread pool.
        /// </summary>
        private Dictionary<string, GTState> workItems = new Dictionary<string, GTState>();

        private int m_MaxThread = 1;
        private int m_MinThread = 1;
        private bool m_FixedSize = false;
        private int m_Timeout = 1000;
        private bool m_IsAutoSettings = false;

        /// <summary>
        /// Signaled when the thread pool is idle, i.e. no thread is busy
        /// and the work items queue is empty
        /// </summary>
        private ManualResetEvent m_IdleWaitHandle = new ManualResetEvent(true);

        /// <summary>
        /// An event to signal all the threads to quit immediately.
        /// </summary>
        private ManualResetEvent m_shuttingDownEvent = new ManualResetEvent(false);

        /// <summary>
        /// Indicate that the NetThreadPool has been disposed
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// ErrorOcurred
        /// </summary>
        public event GenericEventHandler<string> ErrorOcurred;


        #endregion

        #region Thread settings

        Thread thSettings;

        private void StartSettings()
        {
            IncrementAvgFactor = TimeSpan.FromMilliseconds(100).Ticks;
            DecrementAvgFactor = TimeSpan.FromMilliseconds(200).Ticks;
            IncrementFactor = 10;
            DecrementFactor = -10;

            thSettings = new Thread(new ThreadStart(SettingsWorker));
            thSettings.IsBackground = true;
            thSettings.Start();
        }

        private void StopSettings()
        {
            if (thSettings != null)
            {
                //thSettings.Abort();
            }
        }

        private void SettingsWorker()
        {
            if (m_FixedSize)
                return;

            try
            {
                int workItems = Thread.VolatileRead(ref m_CurrentWorkItems);

                long sumIdlTicks = Interlocked.Read(ref m_SumIdleTicks);
                long workedEndCounter = Interlocked.Read(ref m_WorkedEndCounter);

                if (workedEndCounter == 0 || workedEndCounter < workItems)
                {
                    Interlocked.Exchange(ref m_WorkedEndCounter, 0);
                    Interlocked.Exchange(ref m_SumIdleTicks, 0);
                    return;
                }

                double avg = sumIdlTicks / workedEndCounter;

                if (avg < IncrementAvgFactor)
                    Interlocked.Increment(ref m_Factor);
                else if (avg > DecrementAvgFactor)
                    Interlocked.Decrement(ref m_Factor);

                long factor = Interlocked.Read(ref m_Factor);

                if (factor > IncrementFactor)
                {
                    if (workItems < MaxThread)
                    {
                        AddThreadWorker();
                        Interlocked.Exchange(ref m_Factor, 0);
                    }
                }
                else if (factor < DecrementFactor)
                {
                    if (workItems > MinThread)
                    {
                        RemoveThreadWorker();
                        Interlocked.Exchange(ref m_Factor, 0);
                    }
                }
                Interlocked.Exchange(ref m_WorkedEndCounter, 0);
                Interlocked.Exchange(ref m_SumIdleTicks, 0);
            }
            catch
            {

            }
            Thread.Sleep(10000);
        }

        #endregion

        #region pool settings

        private long m_LastWorkedBegin;
        private long m_LastWorkedEnd;
        private long m_Factor;
        private long m_WorkedEndCounter;
        private long m_SumIdleTicks;


        //There are 10,000 ticks in a millisecond. 
        const long tickMillisecond = 10000;
        private long IncrementAvgFactor = 100;
        private long DecrementAvgFactor = 200;
        private long IncrementFactor = 10;
        private long DecrementFactor = 10;



        private void UpdatePoolState(GenericThradState initialState)
        {
            if (m_FixedSize)
                return;

            switch (initialState)
            {
                case GenericThradState.WorkEnd:
                    Interlocked.Exchange(ref m_LastWorkedEnd, DateTime.Now.Ticks);
                    Interlocked.Increment(ref m_WorkedEndCounter);
                    break;
                case GenericThradState.WorkBegin:
                    Interlocked.Exchange(ref m_SumIdleTicks, m_SumIdleTicks + (m_LastWorkedEnd - m_LastWorkedBegin));
                    Interlocked.Exchange(ref m_LastWorkedBegin, DateTime.Now.Ticks);
                    break;
            }
        }

        #endregion

        #region Constructor
        /// <summary>
        /// Ctor GenericThreadPool with fixed size
        /// </summary>
        /// <param name="maxThread"></param>
        public GenericThreadPool(int maxThread)
            : this(maxThread, maxThread, true)
        {
        }

        /// <summary>
        /// Ctor GenericThreadPool with dynamic size, using ManualReset and IdleTime
        /// </summary>
        /// <param name="minThread"></param>
        /// <param name="maxThread"></param>
        public GenericThreadPool(int minThread, int maxThread)
            : this(minThread, maxThread, false)
        {
        }

        /// <summary>
        /// Ctor GenericThreadPool
        /// </summary>
        /// <param name="minThread"></param>
        /// <param name="maxThread"></param>
        /// <param name="fixedSize"></param>
        private GenericThreadPool(int minThread, int maxThread, bool fixedSize)
        {
            Console.WriteLine("Init GenericThreadPool ");
            if (maxThread <= 0)
            {
                throw new ArgumentException("maxThread required number > 0");
            }
            if (minThread <= 0 || minThread > maxThread)
            {
                throw new ArgumentException("minThread required number > 0 and less equal to max thread");
            }
            m_MaxThread = maxThread;
            m_MinThread = minThread;

            //set size settings
            m_FixedSize = fixedSize;
            if (m_MinThread == m_MaxThread)
            {
                m_FixedSize = true;
            }
        }

        ~GenericThreadPool()
        {
            Dispose();
        }
        /// <summary>
        /// Dispose
        /// </summary>
        public virtual void Dispose()
        {
            if (!disposed)
            {
                if (initilized)
                {
                    Abort();
                }

                if (null != m_shuttingDownEvent)
                {

                    m_shuttingDownEvent.Close();
                    m_shuttingDownEvent = null;
                }
                workerThreads.Clear();
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }
        private void ValidateNotDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().ToString(), "The NetThreadPool has been shutdown");
            }
        }

        #endregion

        #region property

        /// <summary>
        /// Get current thread name
        /// </summary>
        public string CurrentThreadName
        {
            get { return Thread.CurrentThread.Name; }
        }

        /// <summary>
        /// Get MaxThread
        /// </summary>
        public int MaxThread
        {
            get { return m_MaxThread; }
        }
        /// <summary>
        /// Get MinThread
        /// </summary>
        public int MinThread
        {
            get { return m_MinThread; }
        }

        /// <summary>
        /// Get CurrentWorkItems
        /// </summary>
        public int CurrentWorkItems
        {
            get { return Thread.VolatileRead(ref m_CurrentWorkItems); }
        }
        /// <summary>
        /// Get if is a FixedSize
        /// </summary>
        public bool FixedSize
        {
            get { return m_FixedSize; }
        }

        /// <summary>
        /// Get if is an auto threads settings
        /// </summary>
        public bool IsAutoSettings
        {
            get { return m_IsAutoSettings; }
        }

        
        /// <summary>
        /// Get or set SuspendTimeout in miliseconds
        /// </summary>
        public int SuspendTimeout
        {
            get { return m_Timeout; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("required value bigger then zero");
                }
                m_Timeout = value;
            }
        }
        /// <summary>
        /// Get or set Idle Time in seconds
        /// </summary>
        public int IdleSecondTime
        {
            get { return m_IdleSecondTime; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("required value bigger then zero");
                }
                m_IdleSecondTime = value;
            }
        }
   
        #endregion

        #region MainThread

        ParameterizedThreadStart m_threadStart;
        
        /// <summary>
        /// Start Thread Pool
        /// </summary>
        /// <param name="start"></param>
        public void StartThreadPool(ParameterizedThreadStart start)
        {
            Console.WriteLine("Create GenericThreadPool GTP");

            try
            {
                if (initilized)
                {
                    return;
                }


                m_threadStart = start;
                workItems.Clear();
               
                for (int i = 0; i < MaxThread; i++)
                {
                    Thread workerThread = new Thread(start);
                    int num = i + 1;
                    string name = "GTP#" + num.ToString();
                    workerThread.Name = name;
                    workerThread.IsBackground = true;
                    workerThreads.Add(name, workerThread);
                    workItems[name] = new GTState();
                }
                //initilized = true;
                int counter = 0;
                foreach (DictionaryEntry o in workerThreads)
                {
                    if (m_FixedSize || counter < MinThread)
                    {
                        Thread t = (Thread)o.Value;
                        ResumeThreadStart(t);
                        counter++;
                    }
                }

                if (m_IsAutoSettings)
                {
                    //TODO: START AUTO THREAD
                }

                initilized = true;

            }
            catch (Exception ex)
            {
                Console.WriteLine("GTP Error:" + ex.Message);

            }
        }

         private void ResumeThreadStart(Thread t)
        {
            try
            {
                t.Start();
                workItems[t.Name].Started = true;
                Interlocked.Increment(ref m_CurrentWorkItems);
            }
            catch (Exception ex)
            {
                Console.WriteLine("GenericThreadPool ResumeThreadStart Error: " + ex.Message);
            }
        }
        private void ResumeThreadStart(Thread t, string name)
        {
            try
            {
                t = new Thread(m_threadStart);
                t.Name = name;
                t.IsBackground = true;
                t.Start();
                workItems[t.Name].Started = true;
                Interlocked.Increment(ref m_CurrentWorkItems);
 
            }
            catch (Exception ex)
            {
                Console.WriteLine("GenericThreadPool ResumeThreadStart Error: " + ex.Message);
            }
        }
        private void ResumeThreadStart(string name)
        {
            try
            {
                Thread t = (Thread)workerThreads[name];
                if (t == null)
                {
                    throw new Exception("ResumeThreadStart error: thread not exists in thread pool");
                }
                t = new Thread(m_threadStart);
                t.Name = name;
                t.IsBackground = true;
                t.Start();
                workItems[t.Name].Started = true;
                Interlocked.Increment(ref m_CurrentWorkItems);

            }
            catch (Exception ex)
            {
                Console.WriteLine("GenericThreadPool ResumeThreadStart Error: " + ex.Message);
            }
        }

        private void SuspendThread(Thread t)
        {
            try
            {

                if ((t.ThreadState & ThreadState.Unstarted) == 0 &&
                       (t.ThreadState & ThreadState.Background) != 0)
                {

                    t.Abort(t.Name);
                    t.Join(SuspendTimeout);
                }
                workItems[t.Name].Started = false;

                Interlocked.Decrement(ref m_CurrentWorkItems);

            }
            catch (Exception ex)
            {
                Console.WriteLine("GenericThreadPool SuspendThread Error: " + ex.Message);
            }
        }

        /// <summary>
        /// StopThreadPool
        /// </summary>
        public virtual void StopThreadPool()
        {
            Console.WriteLine("Stop GenericThreadPool GTP");
            initilized = false;
            Abort();

        }


        /// <summary>
        /// Wait for the thread pool to be idle
        /// </summary>
        public void WaitForIdle()
        {
            WaitForIdle(Timeout.Infinite);
        }

        /// <summary>
        /// Wait for the thread pool to be idle
        /// </summary>
        public bool WaitForIdle(TimeSpan timeout)
        {
            return WaitForIdle((int)timeout.TotalMilliseconds);
        }

        /// <summary>
        /// Wait for the thread pool to be idle
        /// </summary>
        public bool WaitForIdle(int millisecondsTimeout)
        {
            return m_IdleWaitHandle.WaitOne(millisecondsTimeout, false);
        }

        /// <summary>
        /// Force the NetThreadPool to shutdown
        /// </summary>
        public void Abort()
        {
            Abort(true, 0);
        }

        public void Abort(bool forceAbort, TimeSpan timeout)
        {
            Abort(forceAbort, (int)timeout.TotalMilliseconds);
        }

        /// <summary>
        /// Empties the queue of work items and abort the threads in the pool.
        /// </summary>
        public void Abort(bool forceAbort, int millisecondsTimeout)
        {
            ValidateNotDisposed();
            Thread[] threads = null;
            lock (this.SyncRoot)
            {
                // Shutdown the work items queue
                //_workItemsQueue.Dispose();

                // Signal the threads to exit
                initilized = false;// _shutdown = true;
                m_shuttingDownEvent.Set();

                // Make a copy of the threads' references in the pool
                threads = new Thread[workerThreads.Count];
                workerThreads.Values.CopyTo(threads, 0);
            }

            int millisecondsLeft = millisecondsTimeout;
            DateTime start = DateTime.Now;
            bool waitInfinitely = (Timeout.Infinite == millisecondsTimeout);
            bool timeout = false;

            // Each iteration we update the time left for the timeout.
            foreach (Thread thread in threads)
            {
                // Join don't work with negative numbers
                if (!waitInfinitely && (millisecondsLeft < 0))
                {
                    timeout = true;
                    break;
                }
                bool success = false;

                if (!((thread.ThreadState & ThreadState.Stopped) != 0 ||
                        (thread.ThreadState & ThreadState.Unstarted) != 0))
                    success = thread.Join(millisecondsLeft);
                if (!success)
                {
                    timeout = true;
                    break;
                }

                if (!waitInfinitely)
                {
                    // Update the time left to wait
                    TimeSpan ts = DateTime.Now - start;
                    millisecondsLeft = millisecondsTimeout - (int)ts.TotalMilliseconds;
                }
            }

            if (timeout && forceAbort)
            {
                // Abort the threads in the pool
                foreach (Thread thread in threads)
                {
                    if ((thread != null) && thread.IsAlive)
                    {
                        try
                        {
                            thread.Abort("Shutdown");
                        }
                        catch (SecurityException)
                        {
                            //e = e;
                        }
                        catch (ThreadStateException)
                        {
                            //ex = ex;
                            // In case the thread has been terminated 
                            // after the check if it is alive.
                        }
                    }
                }
            }
        }

        #endregion

        #region ICollection

        public bool IsSynchronized
        {
            get
            {
                if (workerThreads != null)
                {
                    return ((ICollection)this.workerThreads).IsSynchronized;
                }
                return false;
            }
        }

        public object SyncRoot
        {
            get
            {
                if (workerThreads != null)
                {
                    return ((ICollection)this.workerThreads).SyncRoot;
                }
                return null;
            }
        }

 
        #endregion

        #region managed threadPool

        private int m_CurrentWorkItems = 0;
        private DateTime m_LastFull;
        private DateTime m_LastWorked;
        private int m_IsFull;
        private int m_IdleSecondTime = 60;
        private object m_lock = new object();

        private int SetPoolState(GenericThradState initialState)
        {
            if (m_FixedSize)
                return 0;

            if (!m_IsAutoSettings)
                return 0;

          
            int currentWorkItems = CurrentWorkItems;

            if (initialState != GenericThradState.Idle)
            {
                //chek if we have available thread to resume.
                if (currentWorkItems < m_MaxThread)
                {

                    if (m_IsFull > 0)
                    {
                        if (DateTime.Now.Subtract(m_LastFull).TotalSeconds > m_IdleSecondTime)
                        {
                            Interlocked.Exchange(ref m_IsFull, 0);
                            return 1;
                        }
                    }
                    else
                    {
                        m_LastFull = DateTime.Now;
                        Interlocked.Exchange(ref m_IsFull, 1);
                    }
                }
                m_LastWorked = DateTime.Now;
            }
            else //if (initialState == GenericThradState.Idle)
            {
                if (currentWorkItems > m_MinThread)
                {

                    if (DateTime.Now.Subtract(m_LastWorked).TotalSeconds > m_IdleSecondTime)
                    {
                        Interlocked.Exchange(ref m_IsFull, 0);
                        return -1;
                    }
                }
            }

            return 0;
        }


       

        private int GetThreadWorkerCount()
        {
            int count = 0;
            lock (workerThreads.SyncRoot)
            {
                foreach (object o in workerThreads.Values)
                {
                    Thread t = (Thread)o;
                    if ((t.ThreadState & ThreadState.Stopped) != 0 ||
                        (t.ThreadState & ThreadState.Unstarted) != 0)
                        count++;
                }
            }
            return MaxThread - count;
        }

        private bool CanIncrement()
        {
            if (!initilized)
                return false;
            return CurrentWorkItems < m_MaxThread;
        }

        private bool CanDecrement()
        {
            if (!initilized)
                return false;
            return CurrentWorkItems > m_MinThread;
        }

        public bool IncrementAvailableThreads()
        {
            return AddThreadWorker();
        }

        public bool DecrementAvailableThreads()
        {
            return RemoveThreadWorker();
        }

        private bool AddThreadWorker()
        {

            bool found = false;
            //we try to find stopped thread to resycle

            lock (((ICollection)workItems).SyncRoot)
            {
                foreach (KeyValuePair<string, GTState> gt in workItems)
                {

                    if (gt.Value.CanResume())
                    {
                        Thread t = (Thread)workerThreads[gt.Key];
                        ResumeThreadStart(t, gt.Key);
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
            {
                lock (workerThreads.SyncRoot)
                {
                    foreach (DictionaryEntry o in workerThreads)
                    {
                        Thread t = (Thread)o.Value;
                        if ((t.ThreadState & ThreadState.Unstarted) != 0)
                        {
                            ResumeThreadStart(t);
                            found = true;
                            break;
                        }
                    }
                }
            }
            return found;

        }

        private bool RemoveThreadWorker()
        {
            bool found = false;

            lock (((ICollection)workItems).SyncRoot)
            {
                foreach (KeyValuePair<string, GTState> gt in workItems)
                {

                    if (gt.Value.CanSuspend(m_IdleSecondTime))
                    {
                        Thread t = (Thread)workerThreads[gt.Key];
                        SuspendThread(t);
                        found = true;
                        break;
                    }
                }
            }

            return found;

           
        }


        private void CheckTreadPoolState(GenericThradState initialState)
        {
            if (!initilized)
                return;

            int state = SetPoolState(initialState);

            if (state < 0)
            {
                //RemoveThreads(1);
                if (m_CurrentWorkItems > MinThread)//TryDecrementAvailableThreads())
                {
                    RemoveThreadWorker();
                }
            }
            else if (state > 0)
            {
                //AddThreads(1);
                if (m_CurrentWorkItems < MaxThread)//TryIncrementAvailableThreads())
                {
                    AddThreadWorker();
                }
            }
        }

  
        /// <summary>
        /// Set ManualReset
        /// </summary>
        /// <param name="initialState">true= WorkBegin, false=WorkEnd </param>
        /// <param name="timeToSleep">time to sleep in milisconds</param>
        public void ManualReset(bool initialState, int timeToSleep)
        {
            ManualReset(initialState ? GenericThradState.WorkBegin : GenericThradState.WorkEnd);
            if (timeToSleep > 0)
            {
                Thread.Sleep(timeToSleep);
            }
        }
        /// <summary>
        /// Set ManualReset
        /// </summary>
        /// <param name="initialState"></param>
        public void ManualReset(bool initialState)
        {
            ManualReset(initialState ? GenericThradState.WorkBegin : GenericThradState.WorkEnd);
        }
        /// <summary>
        /// Set ManualIdle
        /// </summary>
        public void ManualIdle(int timeToSleep)
        {
            ManualReset(GenericThradState.Idle);
            if (timeToSleep > 0)
            {
                Thread.Sleep(timeToSleep);
            }
        }
        /// <summary>
        /// Set ManualIdle
        /// </summary>
        /// <param name="initialState"></param>
        public void ManualReset(GenericThradState initialState)
        {
            if (!m_FixedSize)
            {
                if (m_IsAutoSettings)
                {
                    CheckTreadPoolState(initialState);
                }
            }
        }

        #endregion

        #region override

        /// <summary>
        /// OnErrorOcurred
        /// </summary>
        /// <param name="msg"></param>
        private void OnErrorOcurred(string msg)
        {
            OnErrorOcurred(new GenericEventArgs<string>(msg));
        }
        /// <summary>
        /// OnErrorOcurred
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnErrorOcurred(GenericEventArgs<string> e)
        {
            if (ErrorOcurred != null)
                ErrorOcurred(this, e);
        }

        #endregion

    }
}



    

