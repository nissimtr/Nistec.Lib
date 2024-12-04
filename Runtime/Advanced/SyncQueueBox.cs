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
using System.Linq;
using System.Text;
using System.Collections;
using Nistec.Threading;
using System.Collections.Concurrent;
using System.Threading;
using Nistec.Generic;
#pragma warning disable  CS1591

namespace Nistec.Runtime.Advanced
{
   
  
    public class SyncQueueBox<T> : IDisposable where T : ISyncItem
    {
        private int synchronized;
        private bool KeepAlive = false;
        private ConcurrentQueue<T> m_SynBox;
        public static SyncQueueBox<T> Global = new SyncQueueBox<T>(true);

        #region properties

        public int IntervalSeconds
        {
            get;
            internal set;
        }

        public bool Initialized
        {
            get;
            private set;
        }

        public bool IsRemote
        {
            get;
            private set;
        }

        #endregion

        #region ctor

        public SyncQueueBox(bool isRemote)
        {
            IntervalSeconds = 1000;
            m_SynBox = new ConcurrentQueue<T>();
            IsRemote = isRemote;
            this.Initialized = true;
            //Netlog.Debug("Initialized SyncQueueBox");

            if (isRemote)
            {
                Start();
            }
        }

        
        public void Dispose()
        {
            
        }
        #endregion

        #region events

        public event EventHandler SyncStarted;

        public event SyncItemEventHandler<T> SyncItemCompleted;

        protected virtual void OnSyncStarted(EventArgs e)
        {
            if (this.SyncStarted != null)
            {
                this.SyncStarted(this, e);
            }
        }

        protected virtual void OnSyncItemCompleted(SyncItemEventArgs<T> e)
        {
            if (this.SyncItemCompleted != null)
            {
                this.SyncItemCompleted(this, e);
            }
        }

        private void OnSyncItemCompleted(T item)
        {
            try
            {
                OnSyncItemCompleted(new SyncItemEventArgs<T>(item));

            }
            catch (Exception ex)
            {
                LogAction(true, ex.Message);
            }
        }

        #endregion

        #region Timer Sync

       
        public void Start()
        {

            if (!this.Initialized)
            {
                throw new Exception("The SyncQueueBox not initialized!");
            }

            if (KeepAlive)
                return;
            //Netlog.Debug("SyncQueueBox Started...");
            IsRemote = true;
            KeepAlive = true;
            Thread.Sleep(1000);
            Thread th = new Thread(new ThreadStart(InternalStart));
            th.IsBackground = true;
            th.Start();
        }

        public void Stop()
        {
            KeepAlive = false;
            this.Initialized = false;
            //Netlog.Debug("SyncQueueBox Stoped");
        }


        private void InternalStart()
        {
            OnSyncStarted(EventArgs.Empty);

            while (KeepAlive)
            {
                DoSync();
                Thread.Sleep(IntervalSeconds * 1024);
            }
            //Netlog.Warn("Initialized SyncQueueBox Not keep alive");
        }


        public void Add(T item)
        {
            m_SynBox.Enqueue(item);
        }
        
        public bool TryGet(out T item)
        {
            return m_SynBox.TryDequeue(out item);
        }

        public void DoSync()
        {
            OnSyncTask();
        }

        protected void OnSyncTask()
        {
            try
            {
                //0 indicates that the method is not in use.
                if (0 == Interlocked.Exchange(ref synchronized, 1))
                {
                    T syncTask = default(T);
                    if (m_SynBox.TryDequeue(out syncTask))
                    {
                        OnSyncItemCompleted(syncTask);
                    }
                }
            }
            catch (Exception ex)
            {
                //Netlog.Exception("c" , ex);
                LogAction(true, "SyncQueueBox OnSyncTask End error : {0}", ex.Message);
            }
            finally
            {
                //Release the lock
                Interlocked.Exchange(ref synchronized, 0);
            }
        }

        #endregion

        #region LogAction
        //protected virtual void LogAction(bool isEror, string text)
        //{
           
        //}

        protected virtual void LogAction(bool isEror, string text, params string[] args)
        {
            
        }
        #endregion

    }
}
