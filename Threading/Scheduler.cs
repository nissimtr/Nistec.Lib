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
using System.Text;
using Nistec.Collections;
#pragma warning disable  CS1591

namespace Nistec.Threading
{

    #region SyncDataEventsArgs

    public delegate void SchedulerEventHandler(object sender, SchedulerEventArgs e);

    public class SchedulerEventArgs : EventArgs
    {
        private string itemName;
        private IScheduler Sender;

        public SchedulerEventArgs(IScheduler sender, string name)
        {
            this.itemName = name;
            this.Sender = sender;
        }

        #region Properties Implementation

        /// <summary>
        /// Get Table
        /// </summary>
        public string ScheduleName
        {
            get { return this.itemName; }
        }
        public void Commit()
        {
            Sender.Commit(ScheduleName);
        }

        #endregion

    }

    #endregion


    public enum ScheduleMode
    {
        Interval=0,
        Daily=1,
        Weekly=2,
        Monthly=3,
        Once=4
    }

    public class Schedule
    {
        public readonly string ScheduleName;
        public readonly ScheduleMode Mode;
        public readonly TimeSpan Time;
        public readonly DayOfWeek WeekDay;
        
        public Schedule(string name, ScheduleMode mode, TimeSpan time)
        {
            this.ScheduleName = name;
            this.Mode = mode;
            this.Time = time;
            CalcNextTime();
        }

        public Schedule(string name, ScheduleMode mode, TimeSpan time, DayOfWeek weekDay)
        {
            this.ScheduleName = name;
            this.Mode = mode;
            this.Time = time;
            this.WeekDay = weekDay;
            CalcNextTime();
        }

        public Schedule(string name, DateTime time)
        {
            this.ScheduleName = name;
            this.Mode = ScheduleMode.Once;
            this._NextTime = time;
            this.ExpirationTime = time.AddMinutes(1);
            AllowExpired = true;
            //CalcNextTime();
        }
 
        public bool isExpired = false;

        public bool IsExpired
        {
            get
            {
                if(!AllowExpired)
                    return false;
                return DateTime.Now > ExpirationTime;
            }
        }
        public DateTime _LastTime;
        public DateTime LastTime
        {
            get {return _LastTime; }
        }
        public DateTime _NextTime;
        public DateTime NextTime
        {
            get {return _NextTime; }
        }
        public DateTime _ExpirationTime;
        public DateTime ExpirationTime
        {
            get {return _ExpirationTime; }
            set { _ExpirationTime=value; }
        }
        public bool _Enabled = true;
        public bool Enabled
        {
            get { return _Enabled; }
            set { _Enabled = value; }
        }
        public bool _AllowExpired = false;
        public bool AllowExpired
        {
            get { return _AllowExpired; }
            set { _AllowExpired = value; }
        }
        public int _Count;
        public int Count
        {
            get { return _Count; }
        }


        /// <summary>
        /// CalcNextTime
        /// </summary>
        /// <returns></returns>
        internal void CalcNextTime()
        {
            DateTime curTime = NextTime;
            switch (Mode)
            {
                case ScheduleMode.Interval:
                    _NextTime = LastTime.AddDays(Time.Days).AddHours(Time.Hours).AddMinutes(Time.Minutes);
                    break;
                case ScheduleMode.Daily:
                    _NextTime = LastTime.AddDays(1);
                    break;
                case ScheduleMode.Weekly:
                    _NextTime = LastTime.AddDays(7);
                    break;
                case ScheduleMode.Monthly:
                    _NextTime = LastTime.AddMonths(1);
                    break;
                case ScheduleMode.Once:

                    break;
            }
            _LastTime = curTime;
            _Count++;
        }
    }


    public interface IScheduler
    {
        void Commit(string name);
    }

    /// <summary>
    /// Sync time struct
    /// </summary>
    [Serializable]
    public class Scheduler:IScheduler
    {
  
        const int DefaultInterval = 60000;
        private Dictionary<string, Schedule> scheduleList;
        private Dictionary<string, DateTime> actionList;
        private Schedule curSchedule;

        public event SchedulerEventHandler ScheduleElapsed;

  
 
        /// <summary>
        /// SyncTimer constructor
        /// </summary>
        public Scheduler()
        {

            scheduleList = new Dictionary<string, Schedule>();
            actionList = new Dictionary<string, DateTime>();
            //curIndex = -1;
            //_nextRun = DateTime.Now;
            //_lastRun = DateTime.Now;
            aTimer = new System.Timers.Timer();
            aTimer.AutoReset = true;
            aTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = (double)DefaultInterval;
            Start();
        }

        #region auto sync

        System.Timers.Timer aTimer;

        /// <summary>
        /// Get indicator if sync are enabled 
        /// </summary>
        public bool Enabled
        {
            get
            {
                if (aTimer == null)
                    return false;
                return aTimer.Enabled;
            }
        }

    

        /// <summary>
        /// Start Async config Background multi thread Listner 
        /// </summary>
        protected virtual void Start()
        {
            if (Enabled)
                return;
            //this.interval = interval.TotalMilliseconds;

            try
            {

                aTimer.Enabled = true;
                //initilized = true;
                // Keep the timer alive until the end of Main.
                GC.KeepAlive(aTimer);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:" + ex.Message);
            }
        }


        /// <summary>
        /// Stop AsyncQueue Background multi thread Listner 
        /// </summary>
        protected virtual void Stop()
        {
            Console.WriteLine("Stop Scheduler ");
            if (aTimer != null)
            {
                aTimer.Stop();
            }
        }


        private void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine("Start Scheduler ...");
            if (scheduleList.Count > 0)
            {
                OnSchedule();

            }
        }

        private void OnSchedule()
        {

            DateTime time = DateTime.Now;
            foreach (Schedule item in scheduleList.Values)
            {
                if (item.Enabled && time >= item.NextTime)
                {
                    curSchedule = item;
                    item.CalcNextTime();
                    if (!actionList.ContainsKey(item.ScheduleName))
                    {
                        actionList[item.ScheduleName] = DateTime.Now;
                        OnScheduleElapsed(new SchedulerEventArgs(this,item.ScheduleName));
                    }
                }
            }

        }

        public void Commit(string scheduleName)
        {
            actionList.Remove(scheduleName);
        }


        /// <summary>
        /// OnTimeElapsed
        /// </summary>
        /// <param name="e"></param>
        private void OnScheduleElapsed(SchedulerEventArgs e)
        {
            if (ScheduleElapsed != null)
                ScheduleElapsed(this, e);
        }

 
        #endregion

        #region Scheduler methods

        public void AddItem(Schedule item)
        {
            if (scheduleList.ContainsKey(item.ScheduleName))
            {
                throw new ArgumentException("item with the same name allready exists");

            }
            scheduleList[item.ScheduleName] = item;
        }

        public bool RemoveItem(string name)
        {
            actionList.Remove(name);
            return scheduleList.Remove(name);

        }
        #endregion

        /// <summary>
        /// Get The last Schedule was running
        /// </summary>
        /// <returns></returns>
        public Schedule Current()
        {
            return curSchedule;
        }
 
    }
}
