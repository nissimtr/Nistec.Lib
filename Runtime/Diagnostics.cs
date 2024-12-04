using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Nistec.Runtime
{
    #pragma warning disable CS0169
    /// <summary>
    /// 
    /// </summary>
    public class SysDiagnostics
    {

        PerformanceCounter pc;
               
        //CPUCounter.NextValue();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static PerformanceCounter CPUCounter()
        {
         return  new PerformanceCounter("Processor", "% Processor Time", "_Total");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        //MemCounter.NextValue();
        public static PerformanceCounter MemCounter()
        {
            return new PerformanceCounter("Memory", "Available MBytes");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static PerformanceCounter CurrentProcessCPUCounter()
        {
            return new PerformanceCounter("Process", "% Processor Time",
            Process.GetCurrentProcess().ProcessName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static PerformanceCounter CurrentProcessMemCounter()
        {
            return new PerformanceCounter("Process", "Working Set",
            Process.GetCurrentProcess().ProcessName);
        }

    }
}
