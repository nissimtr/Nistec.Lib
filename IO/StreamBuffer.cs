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
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System;
using System.Reflection;
using System.IO;
using System.Text;
using System.Linq;

#pragma warning disable  CS1591

namespace Nistec.IO
{

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal sealed class ForceTokenStabilizationAttribute : Attribute
    {
    }

    [ComVisible(true)]
    public static class StreamBuffer
    {

        [SecurityCritical]
        internal static unsafe int IndexOfByte(byte* src, byte value, int index, int count)
        {
                byte* numPtr = src + index;
                while ((((int)numPtr) & 3) != 0)
                {
                    if (count == 0)
                    {
                        return -1;
                    }
                    if (numPtr[0] == value)
                    {
                        return (int)((long)((numPtr - src) / 1));
                    }
                    count--;
                    numPtr++;
                }
                uint num = (uint)((value << 8) + value);
                num = (num << 0x10) + num;
                while (count > 3)
                {
                    uint num2 = *((uint*)numPtr);
                    num2 ^= num;
                    uint num3 = 0x7efefeff + num2;
                    num2 ^= uint.MaxValue;
                    num2 ^= num3;
                    if ((num2 & 0x81010100) != 0)
                    {
                        int num4 = (int)((long)((numPtr - src) / 1));
                        if (numPtr[0] == value)
                        {
                            return num4;
                        }
                        if (numPtr[1] == value)
                        {
                            return (num4 + 1);
                        }
                        if (numPtr[2] == value)
                        {
                            return (num4 + 2);
                        }
                        if (numPtr[3] == value)
                        {
                            return (num4 + 3);
                        }
                    }
                    count -= 4;
                    numPtr += 4;
                }
                while (count > 0)
                {
                    if (numPtr[0] == value)
                    {
                        return (int)((long)((numPtr - src) / 1));
                    }
                    count--;
                    numPtr++;
                }
            return -1;
        }
        
        [SecurityCritical, ForceTokenStabilization, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static unsafe void MemoryCopy(byte* src, int srcIndex, byte[] dest, int destIndex, int len)
        {
            if (len != 0)
            {
                    fixed (byte* numRef = dest)
                    {
                        MemoryCopyImpl(src + srcIndex, numRef + destIndex, len);
                    }
            }
        }

        [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), ForceTokenStabilization]
        internal static unsafe void MemoryCopy(byte[] src, int srcIndex, byte* pDest, int destIndex, int len)
        {
            if (len != 0)
            {
                    fixed (byte* numRef = src)
                    {
                        MemoryCopyImpl(numRef + srcIndex, pDest + destIndex, len);
                    }
            }
        }

        [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), ForceTokenStabilization]
        internal static unsafe void StreamBlockCopy(byte[] src, int srcIndex, byte[] pDest, int destIndex, int len)
        {
            if (len != 0)
            {
                    fixed (byte* numRef1 = src, numRef2 = pDest)
                    {
                        MemoryCopyImpl(numRef1 + srcIndex, numRef2 + destIndex, len);
                    }
            }
        }

        [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), ForceTokenStabilization]
        internal static unsafe void StreamBlockCopy(char[] src, int srcIndex, char[] pDest, int destIndex, int len)
        {
            if (len != 0)
            {
                    fixed (char* numRef1 = src, numRef2 = pDest)
                    {
                        MemoryCharCopy(numRef1 , srcIndex, numRef2 , destIndex, len);
                    }
            }
        }

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), ForceTokenStabilization]
        internal static unsafe void MemoryCharCopy(char* pSrc, int srcIndex, char* pDest, int destIndex, int len)
        {
            if (len != 0)
            {
                    MemoryCopyImpl((byte*)(pSrc + srcIndex), (byte*)(pDest + destIndex), len * 2);
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), ForceTokenStabilization, SecurityCritical]
        internal static unsafe void MemoryCopyImpl(byte* src, byte* dest, int len)
        {
            if (len >= 0x10)
            {
                do
                {
                    *((int*) dest) = *((int*) src);
                    *((int*) (dest + 4)) = *((int*) (src + 4));
                    *((int*) (dest + 8)) = *((int*) (src + 8));
                    *((int*) (dest + 12)) = *((int*) (src + 12));
                    dest += 0x10;
                    src += 0x10;
                }
                while ((len -= 0x10) >= 0x10);
            }
            if (len > 0)
            {
                if ((len & 8) != 0)
                {
                    *((int*) dest) = *((int*) src);
                    *((int*) (dest + 4)) = *((int*) (src + 4));
                    dest += 8;
                    src += 8;
                }
                if ((len & 4) != 0)
                {
                    *((int*) dest) = *((int*) src);
                    dest += 4;
                    src += 4;
                }
                if ((len & 2) != 0)
                {
                    *((short*) dest) = *((short*) src);
                    dest += 2;
                    src += 2;
                }
                if ((len & 1) != 0)
                {
                    //dest[0] = src[0];
                    *((byte*)dest) = *((byte*)src);
                    dest++;
                    src++;
                }
            }
        }

        [SecurityCritical]
        internal static unsafe void ZeroMemory(byte* src, long len)
        {
                while (true)
                {
                    len -= 1L;
                    if (len <= 0L)
                    {
                        return;
                    }
                    src[(int)len] = 0;
                }
        }

        private const int DEFAULT_BUFFER_SIZE = short.MaxValue; // +32767
        public static void CopyTo(Stream input, Stream output)
        {
            input.CopyTo(output, DEFAULT_BUFFER_SIZE);
            return;
        }
        public static void CopyToAsync(Stream input, Stream output, int bufferSize)
        {
            if (!input.CanRead) throw new InvalidOperationException("input must be open for reading");
            if (!output.CanWrite) throw new InvalidOperationException("output must be open for writing");

            byte[][] buf = { new byte[bufferSize], new byte[bufferSize] };
            int[] bufl = { 0, 0 };
            int bufno = 0;
            IAsyncResult read = input.BeginRead(buf[bufno], 0, buf[bufno].Length, null, null);
            IAsyncResult write = null;

            while (true)
            {

                // wait for the read operation to complete
                read.AsyncWaitHandle.WaitOne();
                bufl[bufno] = input.EndRead(read);

                // if zero bytes read, the copy is complete
                if (bufl[bufno] == 0)
                {
                    break;
                }

                // wait for the in-flight write operation, if one exists, to complete
                // the only time one won't exist is after the very first read operation completes
                if (write != null)
                {
                    write.AsyncWaitHandle.WaitOne();
                    output.EndWrite(write);
                }

                // start the new write operation
                write = output.BeginWrite(buf[bufno], 0, bufl[bufno], null, null);

                // toggle the current, in-use buffer
                // and start the read operation on the new buffer.
                //
                // Changed to use XOR to toggle between 0 and 1.
                // A little speedier than using a ternary expression.
                bufno ^= 1; // bufno = ( bufno == 0 ? 1 : 0 ) ;
                read = input.BeginRead(buf[bufno], 0, buf[bufno].Length, null, null);

            }

            // wait for the final in-flight write operation, if one exists, to complete
            // the only time one won't exist is if the input stream is empty.
            if (write != null)
            {
                write.AsyncWaitHandle.WaitOne();
                output.EndWrite(write);
            }

            output.Flush();

            // return to the caller ;
            return;
        }

        public static char[] Copy(char[] array, int startIndex, int length)
        {
            // Initialize unmanged memory to hold the array. 
            int size = Marshal.SystemDefaultCharSize * array.Length;
            IntPtr pnt = Marshal.AllocHGlobal(size);
            char[] dest = new char[length];
            try
            {
                
                // Copy the unmanaged array to another managed array. 
                Marshal.Copy(pnt, dest, startIndex, length);
            }
            finally
            {
                // Free the unmanaged memory.
                Marshal.FreeHGlobal(pnt);
            }
            return dest;
        }

        public static char[] Copy(string src, int startIndex, int length)
        {
           return Copy(src.ToCharArray(), startIndex, length);
        }

        public static int StrToFixedBytes(string str, byte[] dest, Encoding encoding)
        {
            byte[] src = encoding.GetBytes(str);
            int length = src.Length;
            //dest.Initialize();
            StreamBlockCopy(src, 0, dest, 0, length);
            return length;
        }

        public static byte[] StrToFixedBytes(string str, int length, Encoding encoding)
        {
            byte[] src = encoding.GetBytes(str);
            byte[] dest = new byte[length];
            //dest.Initialize();
            StreamBlockCopy(src, 0, dest, 0, src.Length);
            return dest;
        }

        public static string FixedBytesToStr(byte[] array, int length, Encoding encoding)
        {
            return encoding.GetString(array, 0, length);
        }

        public static char[] StrToFixedChars(string str, int length)
        {

            char[] src = str.ToCharArray();
            char[] dest = new char[length];
            //dest.Initialize();
            StreamBlockCopy(src, 0, dest, 0, src.Length);
            return dest;
        }

        public static string FixedCharsToStr(char[] array, int length)
        {
            return new string(array, 0, length);
        }

        public static byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }

        public static byte[] BlockCopy(byte[] buffer, int offset, int length)
        {
            byte[] rv = new byte[length];
            System.Buffer.BlockCopy(buffer, offset, rv, 0, length);
            return rv;
        }

        public static byte[] Copy(byte[] array, int startIndex, int length)
        {
            // Initialize unmanged memory to hold the array. 
            int size = Marshal.SystemDefaultCharSize * array.Length;
            IntPtr pnt = Marshal.AllocHGlobal(size);
            byte[] dest = new byte[length];
            try
            {

                // Copy the unmanaged array to another managed array. 
                Marshal.Copy(pnt, dest, startIndex, length);
            }
            finally
            {
                // Free the unmanaged memory.
                Marshal.FreeHGlobal(pnt);
            }
            return dest;
        }
    }

}
