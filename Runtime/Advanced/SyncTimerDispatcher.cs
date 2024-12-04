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
#pragma warning disable  CS1591

namespace Nistec.Runtime.Advanced
{
    
    public class SyncTimerDispatcher<T> : IDisposable //where T : ISyncItem
    {
        private ThreadTimer SettingTimer;
        private ConcurrentDictionary<T, DateTime> m_Timer;

        public static SyncTimerDispatcher<T> Global = new SyncTimerDispatcher<T>();

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

        public DateTime LastSyncTime
        {
            get;
            private set;
        }
        public SyncTimerState SyncState
        {
            get;
            private set;
        }

        public bool IsRemote
        {
            get;
            internal set;
        }

        public DateTime NextSyncTime
        {
            get
            {
                return this.LastSyncTime.AddSeconds((double)this.IntervalSeconds);
            }
        }
        #endregion

        #region ctor

        public SyncTimerDispatcher()
            : this(60, 100, true)
        {

        }

        public SyncTimerDispatcher(int intervalSeconds, int initialCapacity, bool isRemote)
        {

            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * 2;
            if (initialCapacity < 100)
                initialCapacity = 101;

            m_Timer = new ConcurrentDictionary<T, DateTime>(concurrencyLevel, initialCapacity);

            this.IntervalSeconds = intervalSeconds;
            this.IsRemote = isRemote;
            this.Initialized = false;
            this.SyncState = SyncTimerState.Idle;
            this.LastSyncTime = DateTime.Now;
            this.LogAction(false, "Initialized TimeoutDispatcher");
        }

        public void Dispose()
        {
            if (SettingTimer != null)
            {
                DisposeTimer();
            }
        }
        #endregion

        #region events

        public event EventHandler SyncStarted;

        public event SyncItemEventHandler<T> SyncItemCompleted;

        protected virtual void OnSyncStarted(EventArgs e)
        {
            this.OnSyncTimer();

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
            finally
            {
                Remove(item);
            }
            

        }

        #endregion

        #region item timeout

        public void Add(T item, DateTime expiration)
        {
            if (item.Equals(default(T)))
            {
                throw new ArgumentNullException("TimerDispatcher.Add.item");
            }
            m_Timer[item] = expiration;
        }

        public void Add(T item, int expirationMinutes)
        {
            if (item.Equals(default(T)))
            {
                throw new ArgumentNullException("TimerDispatcher.Add.item");
            }

            DateTime time = DateTime.Now.AddMinutes(expirationMinutes);
            m_Timer[item] = time;
        }

        public bool Remove(T item)
        {
            if (item.Equals(default(T)))
            {
                return false;
            }
            DateTime time;
            return m_Timer.TryRemove(item, out time);

        }

        public DateTime Get(T item)
        {
            if (item.Equals(default(T)))
            {
                throw new ArgumentNullException("TimerDispatcher.Remove.item");
            }
            DateTime res = DateTime.MinValue;

            m_Timer.TryGetValue(item, out res);
            return res;
        }

        public bool TryGetValue(T item, out DateTime res)
        {
            if (item.Equals(default(T)))
            {
                res = DateTime.MinValue;
                return false;
            }

            return m_Timer.TryGetValue(item, out res);
        }

        public void Update(T item)
        {
            if (item.Equals(default(T)))
                return;

            m_Timer[item] = DateTime.Now;
        }

        public Dictionary<T, DateTime> Copy()
        {
            Dictionary<T, DateTime> copy = null;
            copy = new Dictionary<T, DateTime>(m_Timer);
            return copy;
        }

        public Dictionary<T, DateTime> CopyAndClear()
        {
            Dictionary<T, DateTime> copy = null;
            copy = new Dictionary<T, DateTime>(m_Timer);
            m_Timer.Clear();
            return copy;
        }

        public void Clear()
        {
                m_Timer.Clear();
        }

        public T[] GetTimedoutItems()
        {
            List<T> list = new List<T>();

            TimeSpan ts = TimeSpan.FromMinutes(1);
            KeyValuePair<T, DateTime>[] items = m_Timer.Where(dic => ts < DateTime.Now.Subtract(dic.Value)).ToArray();


            foreach (var item in items)
            {
                list.Add(item.Key);
            }
            return list.ToArray();
        }


        #endregion

        #region Timer Sync

        internal void SetCacheSyncState(SyncTimerState state)
        {
            this.SyncState = state;
        }

        private void SettingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.Initialized && (this.SyncState == SyncTimerState.Idle))
            {
                this.LogAction(false, "Synchronize Start");
                this.LastSyncTime = DateTime.Now;
                this.OnSyncStarted(EventArgs.Empty);
                DateTime time = this.LastSyncTime.AddSeconds((double)this.IntervalSeconds);
                this.LogAction(false, "Synchronize End, Next Sync:{0}", new string[] { time.ToString() });
           }
        }

        public void Start()
        {

            if (!this.Initialized)
            {
                this.SyncState = SyncTimerState.Idle;
                this.Initialized = true;
                this.InitializeTimer();
            }
        }

        public void Stop()
        {
            if (this.Initialized)
            {
                this.Initialized = false;
                this.SyncState = SyncTimerState.Idle;
                this.DisposeTimer();
            }
        }

        private void DisposeTimer()
        {
            this.SettingTimer.Stop();
            this.SettingTimer.Enabled = false;
            this.SettingTimer.Elapsed -= new System.Timers.ElapsedEventHandler(this.SettingTimer_Elapsed);
            this.SettingTimer = null;
            this.LogAction(false, "Dispose Timer");
        }

        private void InitializeTimer()
        {
            this.SettingTimer = new ThreadTimer((long)(this.IntervalSeconds * 1000));
            this.SettingTimer.AutoReset = true;
            this.SettingTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.SettingTimer_Elapsed);
            this.SettingTimer.Enabled = true;
            this.SettingTimer.Start();
            this.LogAction(false, "Initialized Timer Interval:{0}", new string[] { this.SettingTimer.Interval.ToString() });
        }

        public void DoSync()
        {
            OnSyncTimer();
        }

        protected virtual void OnSyncTimer()
        {
            try
            {
                this.LogAction(false, "OnSyncTimer Start");

                T[] list = GetTimedoutItems();
                if (list != null && list.Length > 0)
                {
                    foreach (T item in list)
                    {
                        OnSyncItemCompleted(item);
                    }
                    this.LogAction(false, "OnSync End, items removed:{0}", new string[] { list.Length.ToString() });
                }
            }
            catch (Exception ex)
            {
                this.LogAction(true, "OnSync End error :" + ex.Message);

            }
        }

        #endregion

        #region LogAction
        protected virtual void LogAction(bool isEror, string text)
        {
           
        }

        protected virtual void LogAction(bool isEror, string text, params string[] args)
        {
            
        }
        #endregion

    }
}
