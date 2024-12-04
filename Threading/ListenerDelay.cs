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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
#pragma warning disable  CS1591

namespace Nistec.Threading
{
    /// <summary>
    /// Represent listener interval algorithem.
    /// </summary>
    public class ListenerDelay
    {
        public const int MIN_DELAY = 10;
        public const int MAX_DELAY = 60000;
        public const float MAX_CPU = 50.0f;

        private PerformanceCounter CPUCounter;

        public ListenerDelay() : this(100, 3000)
        {

        }

        public ListenerDelay(int minDelay, int maxDelay, float maxCpu = MAX_CPU)
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
            this.cpuArea = midStPower;

            this.CPUCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

            Console.WriteLine("DelayPerformance start - Thread:{0}, minDelay:{1}, maxDelay:{2}, highStPower:{3}, midStPower:{4},lowStPower:{5}", Thread.CurrentThread.ManagedThreadId, minDelay, maxDelay, highStPower, midStPower, lowStPower);

        }

        const int maxSycle = 10;
        int cpuArea = 100;

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

                //Console.WriteLine("sum- Thread:{0}, current:{1}, power:{2}, cpuAvg:{3}, step:{4}, avgDelay:{5} ", Thread.CurrentThread.ManagedThreadId, icurrent, (float)(icollector / sumDelay), cpuAvg, step, avgDelay);

                if (cpuAvg > MAX_CPU)//need more power
                {
                    if (icurrent + step < maxDelay)
                    {
                        Interlocked.Add(ref current, step); // realease power by cpu
                    }
                }
                else if (icollector >= (sumDelay * highFactor))//need more power
                {
                    if (avgDelay > minDelay)//allow get more power
                    {
                        if (avgDelay <= cpuArea)//high step will take more power
                        {
                            if (cpuAvg < maxCpu && icurrent - step >= minDelay)//if current avg cpu is allow more power
                            {
                                Interlocked.Add(ref current, step * -1);// add power
                            }
                        }
                        else if (icurrent - step >= minDelay)
                        {
                            Interlocked.Add(ref current, step * -1); // add power
                        }
                    }
                }
                else if (icollector < sumDelay)//release power or stay same using
                {
                    if ((icurrent + step) >= maxDelay)//can release power
                    {
                        Interlocked.Exchange(ref current, maxDelay); //release max power
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
                    }
                    else if ((icurrent + step) < maxDelay)//can release power
                    {
                        Interlocked.Add(ref current, step); //release power
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
}
