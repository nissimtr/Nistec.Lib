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
using System.ComponentModel;
using System.Threading;
using System.Collections;
using Nistec.Collections;
using Nistec.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
#pragma warning disable  CS1591

namespace Nistec.Threading
{

    public enum TaskState
    {
        Waite,
        Running,
        Abort,
        Timeout,
        Completed,
        Quit,
        Error
    }

    public static class TaskPool
    {
        static AsyncTasker _Pool;
        static AsyncTasker Pool
        {
            get
            {
                if (_Pool == null)
                {
                    _Pool = new AsyncTasker(true, true);
                }
                return _Pool;
            }
        }

        public static ITaskerEvents Events
        {
            get { return Pool; }
        }


        public static void AddTask(TaskItem item)
        {
            Pool.Add(item);
        }

        public static Guid AddTask(Action<object> fanction, object args)
        {
            return Pool.Add(fanction, args); 
        }

        public static Guid AddTask(Action<object> fanction, object args, TimeSpan timeout, DateTime execTime)
        {
            return Pool.Add(fanction, args, timeout, execTime); 
        }

        //public static TaskItem Peek(Guid key)
        //{
        //    return Pool.Peek(key); 
        //}

        //public static bool Remove(Guid key)
        //{
        //    return Pool.Remove(key); 
        //}

        public static bool RemoveProcess(Guid key)
        {
            return Pool.RemoveProcess(key);
        }

        public static int Count
        {
            get
            {
                return Pool.Count; 
            }
        }

        public static int InProcess
        {
            get
            {
                return Pool.InProcess; 
            }
        }
    }

    public class TaskItem : IDisposable
    {

        #region members

        internal Action FunctionOneWay;
        internal Action<object> Function;
        internal object Args;
        internal delegate void TaskItemCallback(object args);
        internal IAsyncTasker Owner;
        public bool IsOneWay { get; private set; }

        public Guid Key { get; private set; }
        public TimeSpan Timeout { get; set; }
        public DateTime ExecTime { get; set; }
        public TaskState State { get; internal set; }

        internal Action<GenericEventArgs<TaskItem>> FunctionCompleted;

        public DateTime StartExecTime { get; internal set; }
        public int Retry { get; internal set; }
        internal bool IsExecuteTimedout
        {
            get { return DateTime.Now.Subtract(StartExecTime) > Timeout; }
        }
        public bool IsEmpty
        {
            get { return Function == null && FunctionOneWay==null; }
        }

        #endregion

        #region ctor

        public TaskItem(Action<object> fanction, object args)
        {
            IsOneWay = false;
            ExecTime = DateTime.Now;
            Timeout = TimeSpan.FromSeconds(120);
            State = TaskState.Waite;
            this.Function = fanction;
            this.Key = Guid.NewGuid();
            Args = args;
            StartExecTime = ExecTime.AddSeconds(10);
        }

        public TaskItem(Action fanction, int timeoutSecond)
        {
            IsOneWay = true;
            ExecTime = DateTime.Now;
            Timeout = TimeSpan.FromSeconds(timeoutSecond);
            State = TaskState.Waite;
            this.FunctionOneWay = fanction;
            this.Key = Guid.NewGuid();
            Args = null;
            StartExecTime = ExecTime.AddSeconds(10);
        }
        public TaskItem(Action fanction, object args, int timeoutSecond, Action<GenericEventArgs<TaskItem>> onCompleted)
            : this(fanction, timeoutSecond)
        {
            FunctionCompleted = onCompleted;
            Args = args;
        }
        public TaskItem(Action<object> fanction, object args, Action<GenericEventArgs<TaskItem>> onCompleted)
            : this(fanction, args)
        {
            FunctionCompleted = onCompleted;
        }

        public TaskItem(Action<object> fanction, object args, TimeSpan timeout, DateTime execTime)
        {
            IsOneWay = false;
            ExecTime = execTime;
            Timeout = timeout;
            State = TaskState.Waite;
            this.Function = fanction;
            this.Key = Guid.NewGuid();
            Args = args;
            StartExecTime = execTime.AddSeconds(10);
        }

        ~TaskItem()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed = false;
        protected void Dispose(bool disposing)
        {
            if (!disposed)
            {
                OnProcessCompleted();
                Function = null;
                FunctionOneWay = null;
                Args = null;
                Owner = null;
            }
        }

        #endregion

        #region events

        public event EventHandler<GenericEventArgs<TaskItem>> TaskCompleted;
        /// <summary>
        /// OnTaskCompleted
        /// </summary>
        /// <param name="e"></param>
        protected void OnTaskCompleted(GenericEventArgs<TaskItem> e)
        {

            if (TaskCompleted != null)
                TaskCompleted(this, e);

            if (FunctionCompleted != null)
                FunctionCompleted(e);

            Owner.OnTaskItemCompleted(e);

            OnProcessCompleted();

        }

        public event EventHandler<GenericEventArgs<string,Exception>> TaskError;
        /// <summary>
        /// OnTaskCompleted
        /// </summary>
        /// <param name="e"></param>
        protected void OnError(GenericEventArgs<string, Exception> e)
        {

            if (TaskError != null)
                TaskError(this, e);

            if (FunctionCompleted != null)
                FunctionCompleted(new GenericEventArgs<TaskItem>(this));


            Owner.OnTaskError(e);

            OnProcessCompleted();

        }

        #endregion

        #region Execute

        WorkItem _WorkItem;

        public void CancelExecuting(bool allowAbort = true)
        {
            try
            {
                if (_WorkItem != null)
                    ThreadPoolEx.Cancel(_WorkItem, allowAbort);
                State = TaskState.Abort;
                OnTaskCompleted(new GenericEventArgs<TaskItem>(this));

            }
            catch (Exception ex)
            {
                OnError(new GenericEventArgs<string,Exception>(Key.ToString(),ex));
            }
        }

        internal void ExecuteWorkItem()
        {
            Thread watchedThread = null;
            StartExecTime = DateTime.Now;
            Retry++;

            if (IsOneWay)
            {
                Action function = FunctionOneWay;

                if (function == null) throw new ArgumentNullException("function");
                _WorkItem = null;
                
                using (ManualResetEvent handle = new ManualResetEvent(false))
                {

                    WaitCallback callBack = obj =>
                    {
                        watchedThread = obj as Thread;
                        function();
                        handle.Set();
                    };

                    _WorkItem = ThreadPoolEx.QueueUserWorkItem(callBack, Thread.CurrentThread);
                    if (handle.WaitOne(Timeout))
                    {
                        //handle.Set();
                        //TResult result =(TResult) wi.State;
                        State = TaskState.Completed;
                        OnTaskCompleted(new GenericEventArgs<TaskItem>(this));
                    }
                    else
                    {
                        handle.Set();
                        if (_WorkItem != null)
                            ThreadPoolEx.Cancel(_WorkItem, true);

                        OnError(new GenericEventArgs<string, Exception>(Key.ToString(), new TimeoutException("Execute task operation has timed out")));
                    }
                }
            }
            else
            {
                Action<object> function = Function;

                if (function == null) throw new ArgumentNullException("function");
                _WorkItem = null;
               
                using (ManualResetEvent handle = new ManualResetEvent(false))
                {

                    WaitCallback callBack = obj =>
                    {
                        watchedThread = obj as Thread;
                        function(Args);
                        handle.Set();
                    };

                    _WorkItem = ThreadPoolEx.QueueUserWorkItem(callBack, Thread.CurrentThread);
                    if (handle.WaitOne(Timeout))
                    {
                        
                        State = TaskState.Completed;
                        OnTaskCompleted(new GenericEventArgs<TaskItem>(this));
                    }
                    else
                    {
                        handle.Set();
                        if (_WorkItem != null)
                            ThreadPoolEx.Cancel(_WorkItem, true);

                        OnError(new GenericEventArgs<string, Exception>(Key.ToString(), new TimeoutException("Execute task operation has timed out")));
                    }
                }
            }
        }

        internal void Execute(object args)
        {
            Thread threadToKill = null;
            StartExecTime = DateTime.Now;
            Retry++;
            AsyncCallback callBack = CreateAsyncCallBack();

            if (IsOneWay)
            {
                Action function = FunctionOneWay;
                //Owner = this;
                if (function == null) throw new ArgumentNullException("function");
                
                Action wrappedAction = () =>
                {
                    threadToKill = Thread.CurrentThread;
                    function();
                };
                IAsyncResult result = wrappedAction.BeginInvoke(callBack, args);
                if (result.AsyncWaitHandle.WaitOne(Timeout))
                {
                    wrappedAction.EndInvoke(result);
                }
                else
                {
                    threadToKill.Abort();
                    
                    OnError(new GenericEventArgs<string, Exception>(Key.ToString(), new TimeoutException("Execute task operation has timed out")));

                }
            }
            else
            {
                Action<object> function = Function;
               
                if (function == null) throw new ArgumentNullException("function");
                Action wrappedAction = () =>
                {
                    threadToKill = Thread.CurrentThread;
                    function(args);
                };
                IAsyncResult result = wrappedAction.BeginInvoke(callBack, args);
                if (result.AsyncWaitHandle.WaitOne(Timeout))
                {
                    wrappedAction.EndInvoke(result);
                }
                else
                {
                    threadToKill.Abort();
                    
                    OnError(new GenericEventArgs<string, Exception>(Key.ToString(), new TimeoutException("Execute task operation has timed out")));

                }
            }
        }


        internal void ExecuteTask()
        {
            try
            {

                IAsyncResult result = BeginTask(null);

                EndTask(result);
            }
            catch (Exception ex)
            {
                OnError(new GenericEventArgs<string, Exception>(Key.ToString(), ex));

            }
        }

        #endregion

        #region Async task operation

        internal IAsyncResult BeginTask(AsyncCallback callback)
        {
            StartExecTime = DateTime.Now;
            Retry++;
            TaskItemCallback caller = CreateTaskCacllBack();

            if (callback == null)
            {
                callback = CreateAsyncCallBack();
            }

            // Initiate the asychronous call.  Include an AsyncCallback
            // delegate representing the callback method, and the data
            // needed to call EndInvoke.
            IAsyncResult result = caller.BeginInvoke(Args, callback, caller);

            result.AsyncWaitHandle.WaitOne(Timeout);

            return result;
        }

        internal IAsyncResult BeginTask(TaskItem task, AsyncCallback callback)
        {
            StartExecTime = DateTime.Now;
            Retry++;
            TaskItemCallback caller = CreateTaskCacllBack();

            if (callback == null)
            {
                callback = CreateAsyncCallBack();
            }

            // Initiate the asychronous call.  Include an AsyncCallback
            // delegate representing the callback method, and the data
            // needed to call EndInvoke.
            IAsyncResult result = caller.BeginInvoke(task.Args, callback, caller);

            result.AsyncWaitHandle.WaitOne(task.Timeout);

            //this.resetEvent.Set();
            return result;
        }

        /// <summary>Completes the specified asynchronous receive operation.</summary>
        /// <param name="asyncResult"></param>
        /// <returns></returns>
        internal void EndTask(IAsyncResult asyncResult)
        {
            // Retrieve the delegate.
            TaskItemCallback caller = (TaskItemCallback)asyncResult.AsyncState;

            // Call EndInvoke to retrieve the results.
            caller.EndInvoke(asyncResult);

            //return Result;
        }

        #endregion

        #region Call backs
        private AsyncCallback onRequestCompleted;
        private AsyncCallback CreateAsyncCallBack()
        {
            if (this.onRequestCompleted == null)
            {
                this.onRequestCompleted = new AsyncCallback(OnRequestCompleted);
            }
            return this.onRequestCompleted;
        }

        internal void TaskItemWorker(object args)
        {
            State = TaskState.Running;
            try
            {
                if (IsOneWay)
                    this.FunctionOneWay();
                else
                    this.Function(args);
            }
            catch
            {
                State = TaskState.Error;
            }
        }

        internal TaskItemCallback CreateTaskCacllBack()
        {
            TaskItemCallback caller = new TaskItemCallback(TaskItemWorker);
            return caller;
        }

        

        internal void OnRequestCompleted(IAsyncResult asyncResult)
        {
            State = TaskState.Completed;
            OnTaskCompleted(new GenericEventArgs<TaskItem>(this));
        }

        internal void OnProcessCompleted()
        {
            if (Owner != null && State != TaskState.Quit)
            {
               bool removed = Owner.RemoveTaskProcces(this);//.TasksProcess.Remove(this);
               if (removed)
               {
                   State = TaskState.Quit;
               }
            }
        }
      

        #endregion
    }

    public interface ITaskerEvents
    {

        event EventHandler<GenericEventArgs<TaskItem>> TaskCompleted;

        event EventHandler<GenericEventArgs<string, Exception>> TaskError;

    }

    public interface IAsyncTasker : ITaskerEvents
    {

        void Add(TaskItem item);


        Guid Add(Action<object> fanction, object args);


        Guid Add(Action<object> fanction, object args, TimeSpan timeout, DateTime execTime);

        int Count { get; }

        int InProcess { get; }

        void OnTaskItemCompleted(GenericEventArgs<TaskItem> e);

        void OnTaskError(GenericEventArgs<string, Exception> e);

        bool RemoveTaskProcces(TaskItem item);
    }

    public class AsyncTasker : IAsyncTasker
    {
        /// <summary>
        /// Default Instance
        /// </summary>
        public static readonly AsyncTasker Instance = new AsyncTasker(true,true);

       
        #region ctor

        public AsyncTasker()
        {
            Init(false,true,100,3000);
        }

        public AsyncTasker(bool start, bool enableTaskInProcess)
        {
            Init(start, enableTaskInProcess, 100, 3000);
        }

        public AsyncTasker(bool start, bool enableTaskInProcess, int minDelay, int maxDelay)
        {
            Init(start, enableTaskInProcess, minDelay, maxDelay);
        }

        void Init(bool start, bool enableTaskInProcess,int minDelay, int maxDelay)
        {
            Tasks = new BlockingCollection<TaskItem>();
            TasksProcess = new ConcurrentDictionary<Guid, TaskItem>();
            EnableTaskInProcess = enableTaskInProcess;
            listenerDelay = new  ListenerDelay(minDelay, maxDelay);
            //if (enableTaskInProcess)
            //{
            //    TasksProcess = new ConcurrentDictionary<Guid, TaskItem>();
            //}
            if (start)
            {
                Start();
            }
        }

        #endregion

        #region events

        public event EventHandler<GenericEventArgs<TaskItem>> TaskCompleted;
        /// <summary>
        /// OnTaskCompleted
        /// </summary>
        /// <param name="e"></param>
        protected void OnTaskCompleted(GenericEventArgs<TaskItem> e)
        {

            if (TaskCompleted != null)
                TaskCompleted(this, e);
        }

        public void OnTaskItemCompleted(GenericEventArgs<TaskItem> e)
        {
            //RemoveTaskProcces(e.Args);
            OnTaskCompleted(e);
        }


        public event EventHandler<GenericEventArgs<string, Exception>> TaskError;
        /// <summary>
        /// OnTaskCompleted
        /// </summary>
        /// <param name="e"></param>
        protected void OnError(GenericEventArgs<string, Exception> e)
        {
            if (TaskError != null)
                TaskError(this, e);
        }

        void OnError(Guid key, Exception ex)
        {
            OnError(new GenericEventArgs<string, Exception>(key.ToString(), ex));
        }
        void OnError(string message, Exception ex)
        {
            OnError(new GenericEventArgs<string, Exception>(message, ex));
        }
        public void OnTaskError(GenericEventArgs<string, Exception> e)
        {
            OnError(e);
        }

        public bool RemoveTaskProcces(TaskItem item)
        {
            TaskItem task;
            return TasksProcess.TryRemove(item.Key, out task);
            //if (removed && task != null)
            //{
            //    task.Dispose();
            //}
            //return removed;
        }

        #endregion

        #region Task queue

        BlockingCollection<TaskItem> Tasks;

        ConcurrentDictionary<Guid, TaskItem> TasksProcess;
        public bool EnableTaskInProcess { get; private set; }

        public void Add(TaskItem item)
        {
            item.Owner = this;
            Tasks.Add(item);
        }

        public Guid Add(Action<object> fanction, object args)
        {
            TaskItem item = new TaskItem(fanction, args);
            Add(item);
            return item.Key;
        }

        public Guid Add(Action<object> fanction, object args, TimeSpan timeout, DateTime execTime)
        {
            TaskItem item = new TaskItem(fanction, args, timeout, execTime);
            Add(item);
            return item.Key;
        }
        public bool RemoveProcess(TaskItem item)
        {
            TaskItem task = null;

            return TasksProcess.TryRemove(item.Key, out task);
        }

        public bool RemoveProcess(Guid key)
        {
            TaskItem task = null;

            return TasksProcess.TryRemove(key, out task);
        }

        internal void ClearProcessTimedout()
        {
            if (TasksProcess.Count > 0)
            {
                var tasks = TasksProcess.Values.Where(item => item.IsExecuteTimedout);
                if (tasks != null)
                {
                    foreach (var t in tasks)
                    {
                        if (t.Retry < 3)
                        {
                            Add(t);
                        }
                        TaskItem tsk;
                        TasksProcess.TryRemove(t.Key, out tsk);
                    }
                }
            }
        }

        public int Count
        {
            get
            {
                return Tasks.Count;
            }
        }

        public int InProcess
        {
            get
            {
                return TasksProcess.Count;
            }
        }


        #endregion

        #region Timer Sync

        int synchronized;
        Thread tt;
        private volatile bool alive;
        ListenerDelay listenerDelay;

        public bool Initialized
        {
            get { return alive; }
            //private set;
        }
       
        public void Start()
        {

            if (!this.Initialized)
            {
                alive = true;
                tt = new Thread(new ThreadStart(DequeueWorker));
                tt.IsBackground = true;
                tt.Start();
            }
        }

        public void Stop(int timeout = 0)
        {
            if (this.Initialized)
            {
                //this.Initialized = false;

                try
                {
                    alive = false;
                    // stop the thread
                    tt.Interrupt();

                    // Optionally block the caller
                    // by wait until the thread exits.
                    // If they leave the default timeout, 
                    // then they will not wait at all
                    tt.Join(timeout);
                }
                catch (ThreadInterruptedException tiex)
                {
                    /* Clean up. */
                    OnError("Stop ThreadInterruptedException error ", tiex);
                }
                catch (Exception ex)
                {
                    OnError("Stop error ", ex);
                }
            }
        }

     
        private void DequeueWorker()
        {
            Console.WriteLine("Debuger-AsyncTasker.DequeueWorker...");

            while (alive)
            {
                TaskItem item = null;
                try
                {
                    if (0 == Interlocked.Exchange(ref synchronized, 1))
                    {
                        //while (!Tasks.IsCompleted)
                        //{
                            if (Tasks.TryTake(out item))
                            {
                                if (item != null && !item.IsEmpty)
                                {
                                    if (EnableTaskInProcess)
                                    {
                                        TasksProcess[item.Key] = item;
                                    }
                                    item.ExecuteWorkItem();
                                }
                                listenerDelay.Delay(true);
                                //Thread.Sleep(500);
                            }
                            else
                            listenerDelay.Delay(false);
                            //Thread.Sleep(6000);
                        //}
                    }
                }
                catch (ThreadInterruptedException tiex)
                {
                    OnError(item == null ? Guid.Empty : item.Key, tiex);
                }
                catch (Exception ex)
                {
                    OnError(item == null ? Guid.Empty : item.Key, ex);
                }
                finally
                {
                    Interlocked.Exchange(ref synchronized, 0);
                }
            }

        }
        #endregion

        #region Delay Performance
#if(false)

        DelayPerformance delayProcess;// = new DelayPerformance(10, 10000, 50.0f);
        //delayProcess.Delay(true); or delayProcess.Delay(false);
        class DelayPerformance
        {
            const int MIN_DELAY = 10;
            const int MAX_DELAY = 60000;
            const float MAX_CPU = 50.0f;

            private PerformanceCounter CPUCounter;
            public DelayPerformance(int minDelay, int maxDelay, float maxCpu= MAX_CPU)
            {
                if (minDelay < MIN_DELAY)
                {
                    throw new ArgumentOutOfRangeException("minDelay should be more them 10 ms");
                }
                if (maxDelay > MAX_DELAY)
                {
                    throw new ArgumentOutOfRangeException("maxDelay should be maximum 60000 ms");
                }
                if (maxDelay <= minDelay)
                {
                    throw new ArgumentException("maxDelay should be greater then minDelay");
                }
                if (maxCpu <= 0 || maxCpu > MAX_CPU)
                {
                    maxCpu = MAX_CPU;
                }

                this.maxCpu = maxCpu;
                this.minDelay = minDelay;
                this.maxDelay = maxDelay;
                this.current = maxDelay / 2;

                this.highStPower = minDelay;
                this.midStPower = Math.Min(minDelay * 10, 100);
                this.lowStPower = Math.Max(midStPower * 2, Math.Min(maxDelay / 10, 1000));

                this.CPUCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

                Console.WriteLine("DelayPerformance start - Thread:{0}, minDelay:{1}, maxDelay:{2}, highStPower:{3}, midStPower:{4},lowStPower:{5}", Thread.CurrentThread.ManagedThreadId, minDelay, maxDelay, highStPower, midStPower, lowStPower);

            }

            const int maxSycle = 10;
            const int cpuStep = 100;

            int highStPower;// = 10;
            int midStPower;// = 100;
            int lowStPower;// = 1000;

            int maxDelay;// = 60000;
            int minDelay;// = 10;
            float maxCpu;// = MAX_CPU;

            const float lowFactor = 0.2f;
            const float midFactor = 0.5f;
            const float highFactor = 0.8f;

            long counter = 0;
            long activeCounter = 0;
            long collector = 0;
            long current = 0;
            long cpuCollector;

            public void Delay(bool isActive)
            {

                long icounter = Interlocked.Read(ref counter);
                float cpuCounter = CPUCounter.NextValue();
                long icurrent = Interlocked.Read(ref current);

                if (icounter >= maxSycle)
                {
                    long iactiveCounter = Interlocked.Read(ref activeCounter);
                    long icpuCollector = Interlocked.Read(ref cpuCollector);
                    long icollector = Interlocked.Read(ref collector);
                    long sumDelay = icurrent * maxSycle;

                    int avgDelay = iactiveCounter > 0 ? ((int)(icollector / iactiveCounter)) : 0;

                    float fcpuCollector = (float)(icpuCollector / (float)100);
                    float cpuAvg = icounter > 0 ? fcpuCollector / icounter : 0;

                    //long midHighFactor = (maxDelay - minDelay) / 2;
                    //long midLowFactor = (maxDelay - minDelay) / 3;

                    int step = lowStPower;

                    if (icurrent > ((maxDelay - minDelay) / 2))
                        step = lowStPower;
                    else if (icurrent <= midStPower * 2)
                        step = highStPower;
                    else
                        step = midStPower;

                    Console.WriteLine("sum- Thread:{0}, current:{1}, power:{2}, cpuAvg:{3}, step:{4}, avgDelay:{5} ", Thread.CurrentThread.ManagedThreadId, icurrent, (float)(icollector/sumDelay), cpuAvg, step, avgDelay);

                    if (cpuAvg > MAX_CPU)//need more power
                    {
                        if (icurrent + step < maxDelay)
                        {
                            Interlocked.Add(ref current, step); // realease power by cpu
                            //Console.WriteLine("realease power by cpu");
                        }
                    }
                    else if (icollector >= (sumDelay * highFactor))//need more power
                    {
                        if (avgDelay > minDelay)//allow get more power
                        {
                            if (avgDelay <= cpuStep)//high step will take more power
                            {
                                if (cpuAvg < maxCpu && icurrent - step >= minDelay)//if current avg cpu is allow more power
                                {
                                    Interlocked.Add(ref current, step * -1);// add power
                                    //Console.WriteLine("add power cpuStep");
                                }
                            }
                            else if (icurrent - step >= minDelay)
                            {
                                Interlocked.Add(ref current, step * -1); // add power
                                //Console.WriteLine("add power no cpuStep");
                            }
                        }
                    }
                    else if (icollector < sumDelay)//release power or stay same using
                    {
                        if ((icurrent + step) >= maxDelay)//can release power
                        {
                            Interlocked.Exchange(ref current, maxDelay); //release max power
                            //Console.WriteLine("release max power");
                        }
                        //else if (icollector > (sumDelay * highFactor))//if using more then mid power
                        //{
                        //    if (avgDelay <= cpuStep)//fast step will take more power
                        //    {
                        //        if (cpuAvg < maxCpu && icurrent - step >= minDelay)//if current avg cpu is allow more power
                        //        {
                        //            Interlocked.Add(ref current, step * -1);// add power
                        //            Console.WriteLine("add power cpuStep highFactor");
                        //        }
                        //    }
                        //    else if (icurrent - step >= minDelay)
                        //    {
                        //        Interlocked.Add(ref current, step * -1); // add power
                        //        Console.WriteLine("add power no cpuStep highFactor");
                        //    }
                        //}
                        else if (icollector > (sumDelay * midFactor))//if using more then mid power
                        {
                            //stay current
                            //Console.WriteLine("stay current");
                        }
                        else if ((icurrent + step) < maxDelay)//can release power
                        {
                            Interlocked.Add(ref current, step); //release power
                            //Console.WriteLine("release power");
                        }
                    }

                    Interlocked.Exchange(ref activeCounter, 0);
                    Interlocked.Exchange(ref counter, 0);
                    Interlocked.Exchange(ref collector, 0);
                    Interlocked.Exchange(ref cpuCollector, 0);
                }
                else if (isActive)
                {
                    Interlocked.Add(ref collector, icurrent);
                    Interlocked.Increment(ref counter);
                    Interlocked.Increment(ref activeCounter);
                    Interlocked.Add(ref cpuCollector, (long)cpuCounter * 100);
                }
                else
                {
                    Interlocked.Increment(ref counter);
                    Interlocked.Add(ref cpuCollector, (long)cpuCounter * 100);
                }

                int curDelay = (int)Interlocked.Read(ref current);

                Thread.Sleep(curDelay);

                //Console.WriteLine("thread:{0},current:{1}, counter:{2}, delay:{3}, cpuCounter:{4}", Thread.CurrentThread.ManagedThreadId, icurrent, icounter, curDelay, cpuCounter);

            }

        }
#endif
        #endregion
    }

#if (false)
    public interface IAsyncTasker : ITaskerEvents
    {

        void Add(TaskItem item);


        Guid Add(Action<object> fanction, object args);


        Guid Add(Action<object> fanction, object args, TimeSpan timeout, DateTime execTime);


        TaskItem Peek(Guid key);


        bool Remove(Guid key);


        int Count { get; }


        int InProcess { get; }

        void OnTaskItemCompleted(GenericEventArgs<TaskItem> e);

        void OnTaskError(GenericEventArgs<Guid, Exception> e);

        bool RemoveTaskProcces(TaskItem item);
    }

    public class AsyncTasker : IAsyncTasker
    {
        /// <summary>
        /// Default Instance
        /// </summary>
        public static readonly AsyncTasker Instance = new AsyncTasker(true);

    #region  memebers
                
        /// <summary>
        /// DefaultMaxTimeout
        /// </summary>
        public static readonly TimeSpan DefaultTimeOut = TimeSpan.FromMilliseconds(4294967295);

     
        private ManualResetEvent resetEvent;

        //public bool Initialized
        //{
        //    get { return keepAlive; }
        //}

        public int Capacity
        {
            get;
            private set;
        }

        internal int Interval
        {
            get;
            private set;
        }

    #endregion

    #region ctor

        public AsyncTasker(int capacity=10,int interval=1000)
        {
            Init(capacity, interval,false);
        }

        internal AsyncTasker(bool start, int capacity = 10, int interval = 1000)
        {
            Init(capacity, interval,start);
        }

        void Init(int capacity, int interval, bool start)
        {
            Capacity = (capacity < 0 || capacity > 1000) ? 10 : capacity;
            Interval = interval < 1000 ? 1000 : interval;
            resetEvent = new ManualResetEvent(false);
            if (start)
            {
                Start();
            }
        }

    #endregion

    #region events

        public event EventHandler<GenericEventArgs<TaskItem>> TaskCompleted;
        /// <summary>
        /// OnTaskCompleted
        /// </summary>
        /// <param name="e"></param>
        protected void OnTaskCompleted(GenericEventArgs<TaskItem> e)
        {

            if (TaskCompleted != null)
                TaskCompleted(this, e);
        }

        public void OnTaskItemCompleted(GenericEventArgs<TaskItem> e)
        {
            OnTaskCompleted(e);
        }

       
        public event EventHandler<GenericEventArgs<Guid,Exception>> TaskError;
        /// <summary>
        /// OnTaskCompleted
        /// </summary>
        /// <param name="e"></param>
        protected void OnError(GenericEventArgs<Guid, Exception> e)
        {
             if (TaskError != null)
                TaskError(this, e);
        }

        void OnError(Guid key, Exception ex)
        {
            OnError(new GenericEventArgs<Guid, Exception>(key, ex));
        }

        public void OnTaskError(GenericEventArgs<Guid, Exception> e)
        {
            OnError(e);
        }

        public bool RemoveTaskProcces(TaskItem item)
        {
            TaskItem task;
            return TasksProcess.TryRemove(item.Key, out task);
        }

    #endregion

    #region Task queue

        ConcurrentDictionary<Guid, TaskItem> taskItems;
        internal ConcurrentDictionary<Guid,TaskItem> Tasks
        {
            get
            {
                if (this.taskItems == null)
                {
                    taskItems = new ConcurrentDictionary<Guid, TaskItem>();
                    //taskItems.Capacity = 100;
                }
                return this.taskItems;
            }
        }

        ConcurrentDictionary<Guid, TaskItem> taskProcess;

        internal ConcurrentDictionary<Guid, TaskItem> TasksProcess
        {
            get
            {
                if (this.taskProcess == null)
                {
                    taskProcess = new ConcurrentDictionary<Guid, TaskItem>();
                    //taskProcess.Capacity = 100;
                }
                return this.taskProcess;
            }
        }

        //List<TaskItem> taskItems;

        //internal List<TaskItem> Tasks
        //{
        //    get
        //    {
        //        if (this.taskItems == null)
        //        {
        //            taskItems = new List<TaskItem>();
        //            taskItems.Capacity = 100;
        //        }
        //        return this.taskItems;
        //    }
        //}

        //List<TaskItem> taskProcess;

        //internal List<TaskItem> TasksProcess
        //{
        //    get
        //    {
        //        if (this.taskProcess == null)
        //        {
        //            taskProcess = new List<TaskItem>();
        //            taskProcess.Capacity = 100;
        //        }
        //        return this.taskProcess;
        //    }
        //}

        internal TaskItem Dequeue()
        {

            TaskItem task = null;
            if (Tasks.Count > 0)
            {
                task = Tasks.Values.Where(item => item.ExecTime < DateTime.Now).OrderBy(item => item.ExecTime).FirstOrDefault();
                if (task != null)
                {
                    //Tasks.Remove(task);
                    if (Tasks.TryRemove(task.Key,out task))
                    {
                        //TasksProcess.Add(task);
                        TasksProcess[task.Key] = task;
                    }
                    else
                    {
                        task = null;
                    }
                }
            }
            return task;

        }

        public void Add(TaskItem item)
        {
            //lock (mlock)
            //{
                item.Owner = this;
                //Tasks.Add(item);
                Tasks[item.Key] = item;
            //}
        }

        public Guid Add(Action<object> fanction, object args)
        {
            TaskItem item = new TaskItem(fanction, args);
            Add(item);
            return item.Key;
        }

        public Guid Add(Action<object> fanction, object args, TimeSpan timeout, DateTime execTime)
        {
            TaskItem item = new TaskItem(fanction, args, timeout, execTime);
            Add(item);
            return item.Key;
        }

        public TaskItem Peek(Guid key)
        {
            TaskItem task = null;

            Tasks.TryGetValue(key, out task);

            //lock (mlock)
            //{
            //if (Tasks.Count > 0)
            //{
            //    task = Tasks.Where(item => item.Key == key).FirstOrDefault();
            //}
            //}
            return task;
        }

        public bool Remove(Guid key)
        {

            TaskItem task = null;

            return Tasks.TryRemove(key, out task);

            //lock (mlock)
            //{
            //if (Tasks.Count > 0)
            //{
            //    TaskItem task = Tasks.Where(item => item.Key == key).FirstOrDefault();
            //    if (task != null)
            //    {
            //        return Tasks.Remove(task);
            //    }
            //}
            //}
            //return false;
        }

        internal bool RemoveProcess(TaskItem item)
        {
            TaskItem task = null;

            return TasksProcess.TryRemove(item.Key, out task);

            //lock (mlock)
            //{
            //if (!TasksProcess.IsEmpty)//(TasksProcess.Count > 0)
            //{
            //    if (item != null)
            //    {
            //        return TasksProcess.Remove(item);
            //    }
            //}
            //}
            //return false;
        }

        internal bool RemoveProcess(Guid key)
        {
            TaskItem task = null;

            return TasksProcess.TryRemove(key, out task);

            //lock (mlock)
            //{
            //if (!TasksProcess.IsEmpty)//(TasksProcess.Count > 0)
            //{
            //    TaskItem task = TasksProcess.Where(item => item.Key == key).FirstOrDefault();
            //    if (task != null)
            //    {
            //        return TasksProcess.Remove(task);
            //    }
            //}
            //}
            //return false;

        }

        internal void ClearProcessTimedout()
        {
            if (TasksProcess.Count > 0)
            {
                var tasks = TasksProcess.Values.Where(item => item.IsExecuteTimedout);
                if (tasks != null)
                {
                    foreach (var t in tasks)
                    {
                        if (t.Retry < 3)
                        {
                            Add(t);
                        }
                        TaskItem tsk;
                        TasksProcess.TryRemove(t.Key, out tsk);
                    }
                }
            }
        }

        public int Count
        {
            get
            {
                //lock (mlock)
                //{
                    return Tasks.Count;
                //}
            }
        }

        public int InProcess
        {
            get
            {
                //lock (mlock)
                //{
                    return TasksProcess.Count;
                //}
            }
        }


    #endregion

    #region timer

       
        /*
        //static object mlock = new object();
        Thread thTask;
        private bool keepAlive;

        //public static object TaskLock
        //{
        //    get { return mlock; }
        //}

        public void Start()
        {
            if (keepAlive)
                return;
            thTask = new Thread(new ThreadStart(TaskWorker));
            thTask.IsBackground = true;
            keepAlive = true;
            thTask.Start();
        }

        public void Stop()
        {
            keepAlive = false;
        }

        private void TaskWorker()
        {
            while (keepAlive)
            {
                TaskItem item = null;
                try
                {
                    //lock (mlock)
                    //{
                        item = Dequeue();

                        if (item != null && !item.IsEmpty)
                        {
                            item.ExecuteWorkItem();
                        }
                    //}
                }
                catch (Exception ex)
                {
                    OnError(item == null ? Guid.Empty : item.Key, ex);
                }

                Thread.Sleep(Interval);
            }
        }
        */

    #endregion

    #region Timer Sync

        int synchronized;
        
        //private ThreadTimer SettingTimer;
        System.Timers.Timer SettingTimer;
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
 
        public DateTime NextSyncTime
        {
            get
            {
                return this.LastSyncTime.AddMilliseconds((double)this.Interval);
            }
        }

        private void SettingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.Initialized)
            {
                this.LastSyncTime = DateTime.Now;
                this.DequeueWorker();
                DateTime time = this.LastSyncTime.AddMilliseconds((double)this.Interval);
                //this.NextSyncTime = time;
            }
        }

        public void Start()
        {

            if (!this.Initialized)
            {
                //this.SyncState = CacheSyncState.Idle;
                this.Initialized = true;
                this.InitializeTimer();
            }
        }

        public void Stop()
        {
            if (this.Initialized)
            {
                this.Initialized = false;
                //this.SyncState = CacheSyncState.Idle;
                this.DisposeTimer();
            }
        }

        private void DisposeTimer()
        {
            this.SettingTimer.Stop();
            this.SettingTimer.Enabled = false;
            this.SettingTimer.Elapsed -= new System.Timers.ElapsedEventHandler(this.SettingTimer_Elapsed);
            this.SettingTimer = null;
        }

        private void InitializeTimer()
        {
            this.SettingTimer = new ThreadTimer((long)(this.Interval));
            this.SettingTimer.AutoReset = true;
            this.SettingTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.SettingTimer_Elapsed);
            this.SettingTimer.Enabled = true;
            this.SettingTimer.Start();
        }

        private void DequeueWorker()
        {
            Console.WriteLine("Debuger-AsyncTasker.DequeueWorker...");
            if (0 == Interlocked.Exchange(ref synchronized, 1))
            {
                TaskItem item = null;
                do
                {
                    try
                    {
                        item = Dequeue();

                        if (item != null && !item.IsEmpty)
                        {
                            item.ExecuteWorkItem();
                            Thread.Sleep(100);
                        }
                    }
                    catch (Exception ex)
                    {
                        OnError(item == null ? Guid.Empty : item.Key, ex);
                    }
                } while (item != null);
            }
            Interlocked.Exchange(ref synchronized, 0);
        }
    #endregion

    }

    public class TaskerQueue : IAsyncTasker
    {
    #region  memebers

        /// <summary>
        /// DefaultMaxTimeout
        /// </summary>
        public static readonly TimeSpan DefaultTimeOut = TimeSpan.FromMilliseconds(4294967295);


        //private AsyncCallback onRequestCompleted;
        private ManualResetEvent resetEvent;

        public bool Initialized
        {
            get { return keepAlive; }
        }

        public int Capacity
        {
            get;
            private set;
        }

        internal int Interval
        {
            get;
            private set;
        }

    #endregion

    #region ctor

        public TaskerQueue(int capacity = 10, int interval = 100)
        {
            Init(capacity, interval, false);
        }

        internal TaskerQueue(bool start, int capacity = 10, int interval = 100)
        {
            Init(capacity, interval, start);
        }

        void Init(int capacity, int interval, bool start)
        {
            Capacity = (capacity < 0 || capacity > 1000) ? 10 : capacity;
            Interval = interval < 10 ? 100 : interval;
            resetEvent = new ManualResetEvent(false);
            if (start)
            {
                Start();
            }
        }

    #endregion

    #region events

        public event EventHandler<GenericEventArgs<TaskItem>> TaskCompleted;
        /// <summary>
        /// OnTaskCompleted
        /// </summary>
        /// <param name="e"></param>
        protected void OnTaskCompleted(GenericEventArgs<TaskItem> e)
        {

            if (TaskCompleted != null)
                TaskCompleted(this, e);
        }

        public void OnTaskItemCompleted(GenericEventArgs<TaskItem> e)
        {
            OnTaskCompleted(e);
        }

       
        public event EventHandler<GenericEventArgs<string, Exception>> TaskError;
        /// <summary>
        /// OnTaskCompleted
        /// </summary>
        /// <param name="e"></param>
        public void OnError(GenericEventArgs<string, Exception> e)
        {
            if (TaskError != null)
                TaskError(this, e);
        }

        void OnError(string key, Exception ex)
        {
            OnError(new GenericEventArgs<string, Exception>(key, ex));
        }

        public void OnTaskError(GenericEventArgs<string, Exception> e)
        {
            OnError(e);
        }

        public bool RemoveTaskProcces(TaskItem item)
        {
            return Remove(item.Key);
        }

    #endregion

    #region Task queue

        ConcurrentQueue<TaskItem> taskItems;

        internal ConcurrentQueue<TaskItem> Tasks
        {
            get
            {
                if (this.taskItems == null)
                {
                    taskItems = new ConcurrentQueue<TaskItem>();
                    //taskItems.Capacity = 100;
                }
                return this.taskItems;
            }
        }


        internal TaskItem Dequeue()
        {

            TaskItem task = null;
            if (Tasks.Count > 0)
            {
                Tasks.TryDequeue(out task);
            }
            return task;

        }

        public void Add(TaskItem item)
        {
            //lock (mlock)
            //{
                item.Owner = this;
                Tasks.Enqueue(item);
            //}
        }

        public Guid Add(Action<object> fanction, object args)
        {
            TaskItem item = new TaskItem(fanction, args);
            Add(item);
            return item.Key;
        }

        public Guid Add(Action<object> fanction, object args, TimeSpan timeout, DateTime execTime)
        {
            TaskItem item = new TaskItem(fanction, args, timeout, execTime);
            Add(item);
            return item.Key;
        }

        public TaskItem Peek(Guid key)
        {
            TaskItem task = null;
            //lock (mlock)
            //{
            //    if (Tasks.Count > 0)
            //    {
            //        task = Tasks.Peek();
            //    }
            //}

            if (Tasks.Count > 0)
            {
                Tasks.TryPeek(out task);
            }
            return task;
        }

        public bool Remove(Guid key)
        {
            //lock (mlock)
            //{
                if (Tasks.Count > 0)
                {
                    TaskItem task = Tasks.Where(item => item.Key == key).FirstOrDefault();
                    if (task != null)
                    {
                        task.Dispose();
                        task = null;
                        return true;
                    }
                }
            //}
            return false;
        }

        //internal bool RemoveProcess(TaskItem item)
        //{
        //    //lock (mlock)
        //    //{
        //        if (Tasks.Count > 0)
        //        {
        //            TaskItem task = Tasks.Where(p => p.Key == item.Key).FirstOrDefault();
        //            if (task != null)
        //            {
        //                task.Dispose();
        //                task = null;
        //                return true;
        //            }
        //        }
        //    //}
        //    return false;
        //}

        
        public int Count
        {
            get
            {
                //lock (mlock)
                //{
                    return Tasks.Count;
                //}
            }
        }

        public int InProcess
        {
            get
            {
                //lock (mlock)
                //{
                    return Tasks.Count;
                //}
            }
        }


    #endregion

    #region timer

        static object mlock = new object();
        Thread thTask;
        private bool keepAlive;

        public static object TaskLock
        {
            get { return mlock; }
        }

        public void Start()
        {
            if (keepAlive)
                return;
            thTask = new Thread(new ThreadStart(TaskWorker));
            thTask.IsBackground = true;
            keepAlive = true;
            thTask.Start();
        }

        public void Stop()
        {
            keepAlive = false;
        }

        private void TaskWorker()
        {
            while (keepAlive)
            {
                TaskItem item = null;
                try
                {
                    //lock (mlock)
                    //{
                        item = Dequeue();

                        if (item != null && !item.IsEmpty)
                        {
                            item.ExecuteWorkItem();
                        }
                    //}
                }
                catch (Exception ex)
                {
                    OnError(item == null ? Guid.Empty : item.Key, ex);
                }

                Thread.Sleep(Interval);
            }
        }


    #endregion

    }
#endif



}
