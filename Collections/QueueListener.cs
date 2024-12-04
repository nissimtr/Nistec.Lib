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
#define TASk//THREADS//THREADPOOL//TASk//TASKARRAY

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Nistec.Generic;

#pragma warning disable  CS1591

namespace Nistec.Collections
{
    /// <summary>
    /// Represents a thread-safe queue listener (FIFO) collection.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueueListener<T>
    {
        ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        CancellationTokenSource canceller = new CancellationTokenSource();
        Action<T> _action;
        int intervalMilliseconds;

        #region message events

        /// <summary>
        /// ErrorOcurred
        /// </summary>
        public event GenericEventHandler<string> ErrorOcurred;
        /// <summary>
        /// Message Received
        /// </summary>
        public event GenericEventHandler<T> MessageReceived;
        /// <summary>
        /// Message Arraived
        /// </summary>
        public event GenericEventHandler<T> MessageArraived;




        protected virtual void OnMessageArraived(GenericEventArgs<T> e)
        {
            if (MessageArraived != null)
                MessageArraived(this, e);
        }

        protected virtual void OnMessageReceived(GenericEventArgs<T> e)
        {
 
            if (MessageReceived != null)
                MessageReceived(this, e);
        }
        protected virtual void OnErrorOcurred(GenericEventArgs<string> e)
        {
            if (ErrorOcurred != null)
                ErrorOcurred(this, e);
        }

        /// <summary>
        /// OnErrorOcurred
        /// </summary>
        /// <param name="msg"></param>
        private void OnErrorOcurred(string msg)
        {
            Console.WriteLine("ErrorOcurred: " + msg);
            OnErrorOcurred(new GenericEventArgs<string>(msg));
        }

        #endregion

        #region ctor
        public QueueListener(int intervalMilliseconds = 100)
        {
            this.intervalMilliseconds = intervalMilliseconds;
        }
        public QueueListener(Action<T> action, int intervalMilliseconds = 100)
        {
            this.intervalMilliseconds = intervalMilliseconds;
            _action = action;

        }
        #endregion

        #region properties

        long _counter;
        /// <summary>
        /// Gets the number of elements contained in the Queue{T}.
        /// </summary>
        public long Count
        {
            get { return Interlocked.Read(ref _counter); }
        }

        /// <summary>
        /// Get indicating whether the queue is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return Interlocked.Read(ref _counter)==0; }
        }

        bool _isalive = false;
        /// <summary>
        /// Get indicating whether the queue listener ia alive.
        /// </summary>
        public bool IsAlive
        {
            get { return _isalive; }
        }
        #endregion

        #region enqueue/dequeue

        /// <summary>
        /// Adds an object to the end of the Queue{T}.
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(T item)
        {
            _queue.Enqueue(item);
            Interlocked.Increment(ref _counter);
            Thread.Sleep(1);

            OnMessageArraived(new GenericEventArgs<T>(item));

            Console.WriteLine("<{0}> QListener item added, Count: {1}", Thread.CurrentThread.ManagedThreadId,Interlocked.Read(ref _counter));
        }
        /// <summary>
        /// Attempts to remove and return the object at the beginning of the Queue{T}.
        /// </summary>
        /// <returns></returns>
        public T Dequeue()
        {
            T item;
            if (_queue.TryDequeue(out item))
            {
                Interlocked.Decrement(ref _counter);
            }
            //_queue.TryTake(out item);
            return item;
        }
        #endregion

        #region start/stop

        /// <summary>
        /// Start the queue listener.
        /// </summary>
        public void Start()
        {
            _isalive = true;
            // Start queue listener...
            Task listener = Task.Factory.StartNew(() =>
            {
                while (_isalive)
                {
                    T item;
                    if (_queue.TryDequeue(out item))
                    {
                        Interlocked.Decrement(ref _counter);
                        if (_action != null)
                        {
                            _action(item);
                        }
                        else
                            OnMessageReceived(new GenericEventArgs<T>(item));
                    }
                    Thread.Sleep(intervalMilliseconds);
                }

                Console.WriteLine("QListener stoped...");
            },
            canceller.Token,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);

            Console.WriteLine("QListener started...");
        }


        /// <summary>
        /// Stop the queue listener.
        /// </summary>
        public void Stop()
        {
            _isalive = false;
            // Shut down the listener...
            canceller.Cancel();
            //listener.Wait();
        }
        #endregion
    }
}
