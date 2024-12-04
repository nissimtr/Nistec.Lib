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
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.IO;
using System.ComponentModel;
using System.Threading;
using System.Transactions;
using System.Security;
using System.Security.Permissions;

#pragma warning disable  CS1591

namespace Nistec.IO.Unsafe
{
    public class FileStreamer:IDisposable
    {

        #region static

        public static int CalcBufferLength(long length)
        {
            if (length <= 1024)
                return 1024;
            if (length <= 8192)
                return 4096;
            if (length <= 16384)
                return 8192;
            return 16384;
        }

        public static short[] ConvertBuffer(byte[] buffer)
        {
            return new short[buffer.Length >> 1];
        }

        public static ushort[] ConvertUBuffer(byte[] buffer)
        {
            return new ushort[buffer.Length >> 1];
        }

        public static unsafe void ReadStream(FileStream fs, short[] buffer, int offset, int count)
        {
            if (null == fs)
                throw new ObjectDisposedException("The file streamer is disposed");
            if (null == buffer)
                throw new ArgumentNullException("buffer");
            if (offset < 0 || count < 0 || offset + count > buffer.Length)
                throw new IndexOutOfRangeException("offset + count is out of range");

                        
            uint bytesToRead = (uint)(2 * count);
            if (bytesToRead < count)
                throw new IndexOutOfRangeException("The count of bytes To Read is out of range"); // detect integer overflow


            long unused; uint BytesRead;
            SafeFileHandle nativeHandle = fs.SafeFileHandle; // clears Position property
            Win32Io.SetFilePointerEx(nativeHandle, offset * sizeof(short), out unused, 0);

            fixed (short* pFirst = &buffer[0])
                Win32Io.ReadFile(nativeHandle, (IntPtr)pFirst, (uint)count * sizeof(short), out BytesRead, IntPtr.Zero);
        }

        public static unsafe void ReadStream(FileStream fs, byte[] buffer, int offset, int count)
        {
            if (null == fs)
                throw new ObjectDisposedException("The file streamer is disposed");
            if (null == buffer)
                throw new ArgumentNullException("buffer");
            if (offset < 0 || count < 0 || offset + count > buffer.Length)
                throw new IndexOutOfRangeException("offset + count is out of range");

            long unused; uint BytesRead;
            SafeFileHandle nativeHandle = fs.SafeFileHandle; // clears Position property
            Win32Io.SetFilePointerEx(nativeHandle, offset * sizeof(byte), out unused, 0);

            fixed (byte* pFirst = &buffer[0])
                Win32Io.ReadFile(nativeHandle, (IntPtr)pFirst, (uint)count * sizeof(byte), out BytesRead, IntPtr.Zero);
        }

        public static unsafe int WriteStream(FileStream fs, byte[] buffer, int offset, int count)
        {
            if (null == fs)
                throw new ObjectDisposedException("The file streamer is disposed");
            if (null == buffer)
                throw new ArgumentNullException("buffer");
            if (offset < 0 || count < 0 || offset + count > buffer.Length)
                throw new IndexOutOfRangeException("offset + count is out of range");

           
            long unused; int BytesWrite;
            SafeFileHandle nativeHandle = fs.SafeFileHandle; // clears Position property
            Win32Io.SetFilePointerEx(nativeHandle, offset * sizeof(byte), out unused, 0);

            fixed (byte* pFirst = &buffer[0])
                Win32Io.WriteFile(nativeHandle, pFirst, count * sizeof(byte), out BytesWrite, null);
            return BytesWrite;
        }

        
        #endregion

        #region ctor/Dispose

        FileStream m_fs;

        public FileStreamer(string filename,FileMode mode,FileAccess access, FileShare share)
        {
            m_fs = new FileStream(filename, mode, access, share);
        }

        public FileStreamer(FileStream fs)
        {
            m_fs = fs;
        }

        public FileStreamer(SafeFileHandle fh, FileAccess access)
        {
            m_fs = new FileStream(fh, access);
        }

        public void Dispose()
        {
            if (m_fs != null)
            {
                m_fs.Dispose();
                m_fs = null;
            }
        }

        #endregion
        
        public int ReadTo(NetStream stream)
        {
            if (null == m_fs)
                throw new ObjectDisposedException("The file streamer is disposed");
            if (null == stream)
                throw new ArgumentNullException("stream");


            int bufferLength = CalcBufferLength(m_fs.Length);
            int bytesReceived = 0;
            byte[] buffer = new byte[bufferLength];
            int numBytesToRead = bufferLength;

            while (numBytesToRead > 0)
            {
                // Read may return anything from 0 to numBytesToRead.
                int n = Read(buffer, 0, bufferLength);
                // The end of the file is reached.
                if (n == 0)
                    break;

                stream.Write(buffer,0,n);
                bytesReceived += n;
                numBytesToRead -= n;
            }
            return bytesReceived;
        }

        public unsafe int Read(byte[] buffer, int offset, int count)
        {

            if (null == m_fs)
                throw new  ObjectDisposedException("The file streamer is disposed");
            if (null == buffer)
                throw new ArgumentNullException("buffer");
            if (offset < 0 || count < 0 || offset + count > buffer.Length)
                throw new IndexOutOfRangeException("offset + count is out of range");

            long unused; uint BytesRead;
            SafeFileHandle nativeHandle = m_fs.SafeFileHandle; // clears Position property
            Win32Io.SetFilePointerEx(nativeHandle, offset * sizeof(byte), out unused, 0);

            fixed (byte* pFirst = &buffer[0])
                Win32Io.ReadFile(nativeHandle, (IntPtr)pFirst, (uint)count * sizeof(byte), out BytesRead, IntPtr.Zero);
            return (int)BytesRead;
        }

        public unsafe int Read(short[] buffer, int offset, int count)
        {
            if (null == m_fs)
                throw new ObjectDisposedException("The file streamer is disposed");
            if (null == buffer)
                throw new ArgumentNullException("buffer");
            if (offset < 0 || count < 0 || offset + count > buffer.Length)
                throw new IndexOutOfRangeException("offset + count is out of range");

            uint bytesToRead = (uint)(2 * count);
            if (bytesToRead < count)
                throw new ArgumentException(); // detect integer overflow


            long unused; uint BytesRead;
            SafeFileHandle nativeHandle = m_fs.SafeFileHandle; // clears Position property
            Win32Io.SetFilePointerEx(nativeHandle, offset * sizeof(short), out unused, 0);

            fixed (short* pFirst = &buffer[0])
                Win32Io.ReadFile(nativeHandle, (IntPtr)pFirst, (uint)count * sizeof(short), out BytesRead, IntPtr.Zero);
            return (int)BytesRead;
        }

        public unsafe int Write(byte[] buffer, int offset, int count)
        {
            if (null == m_fs)
                throw new ObjectDisposedException("The file streamer is disposed");
            if (null == buffer)
                throw new ArgumentNullException("buffer");
            if (offset < 0 || count < 0 || offset + count > buffer.Length)
                throw new IndexOutOfRangeException("offset + count is out of range");

            long unused; int BytesWrite;
            SafeFileHandle nativeHandle = m_fs.SafeFileHandle; // clears Position property
            Win32Io.SetFilePointerEx(nativeHandle, offset * sizeof(byte), out unused, 0);

            fixed (byte* pFirst = &buffer[0])
                Win32Io.WriteFile(nativeHandle, pFirst, count * sizeof(byte), out BytesWrite, null);
            return BytesWrite;
        }

        public unsafe int Write(string s)
        {
            if (null == m_fs)
                throw new ObjectDisposedException("The file streamer is disposed");
            if (null == s)
                throw new ArgumentNullException("s");
            byte[] b = Encoding.UTF8.GetBytes(s);
            return Write(b, 0, b.Length);
        }

        public int ReadAndDelete(NetStream stream)
        {
            int readcount = 0;
            readcount = ReadTo(stream);
            string filename = m_fs.Name;
            m_fs.Close();
            File.Delete(filename);
            return readcount;
        }

        [SecuritySafeCritical]
        public virtual void Unlock(long position, long length)
        {
            if (m_fs == null)
                m_fs.Unlock(position, length);
        }

        [SecuritySafeCritical]
        public virtual void Lock(long position, long length)
        {
            if (m_fs == null)
                m_fs.Lock(position, length);

        }

    }
}
