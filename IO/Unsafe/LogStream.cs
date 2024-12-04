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
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.IO;
using System.Text;

#pragma warning disable  CS1591

namespace Nistec.IO.Unsafe
{

    public enum LogWriteOption
    {
        UnlimitedSequentialFiles,
        LimitedCircularFiles,
        SingleFileUnboundedSize,
        LimitedSequentialFiles,
        SingleFileBoundedSize
    }

    public class LogStream : Stream
    {
        private const int mm_writeOptionRetryThreshold = 2;
        internal const long DefaultFileSize = 0x9c4000L;
        internal const int DefaultNumberOfFiles = 2;
        internal const LogWriteOption DefaultRetention = LogWriteOption.SingleFileUnboundedSize;
        
        private bool m_canRead;
        private bool m_canSeek;
        private bool m_canWrite;

        private int m_currentFileNum = 1;
        private bool m_disableLogging;
        private int m_fileAccess;
        private string m_fileExt;
        private string m_fileName;
        private string m_fileNameWithoutExt;
        private string m_filePath;
        private int m_flagsAndAttributesFile;
        private long m_maxFileSize = 0x9c4000L;
        private int m_maxNumberOfFiles = 2;
        private int m_writeOptionRetryCount;
        private FileIOPermissionAccess m_filePermission;
        private Win32Io.SecurityAttributes m_secAttrib;
        private bool m_seekToEndFile;

        private SafeFileHandle m_filehandle;
        private LogWriteOption m_writeOption;
        private FileShare m_fileShare;
        private FileMode m_fileMode;

        private readonly object mm_lockObject = new object();


        [SecurityCritical]
        public LogStream(string path, int bufferSize, LogWriteOption writeOption, long maxFileSize, int maxNumOfFiles)
        {
            string fullPath = Path.GetFullPath(path);
            this.m_fileName = fullPath;
            if (fullPath.StartsWith(@"\\.\", StringComparison.Ordinal))
            {
                throw new NotSupportedException("Not Supported, IO Non File Devices");
            }
            Win32Io.SecurityAttributes secAttrs = GetSecurityAttrib(FileShare.Read);
            int num = 0x100000;
            this.m_canWrite = true;
            this.m_filePath = fullPath;
            this.m_fileAccess = 0x40000000;
            this.m_fileShare = FileShare.ReadWrite;// FileShare.Read;
            this.m_secAttrib = secAttrs;
            this.m_filePermission = FileIOPermissionAccess.Write;
            this.m_fileMode = FileMode.OpenOrCreate;
            this.m_flagsAndAttributesFile = num;
            this.m_seekToEndFile = true;
            this.bufferSize = bufferSize;
            this.m_writeOption = writeOption;
            this.m_maxFileSize = maxFileSize;
            this.m_maxNumberOfFiles = maxNumOfFiles;
            this.m_Init(fullPath, this.m_fileAccess, this.m_fileShare, this.m_secAttrib, this.m_filePermission, this.m_fileMode, this.m_flagsAndAttributesFile, this.m_seekToEndFile);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void m_DisableLogging()
        {
            this.m_disableLogging = true;
        }

        [SecurityCritical]
        internal void m_Init(string path, int fAccess, FileShare share, Win32Io.SecurityAttributes secAttrs, FileIOPermissionAccess secAccess, FileMode mode, int flagsAndAttributes, bool seekToEnd)
        {
            string fullPath = Path.GetFullPath(path);
            this.m_fileName = fullPath;
            new FileIOPermission(secAccess, new string[] { fullPath }).Demand();
            int newMode = Win32Io.SetErrorMode(1);
            try
            {
                this.m_filehandle = Win32Io.SafeCreateFile(fullPath, fAccess, share, secAttrs, mode, flagsAndAttributes, Win32Io.NULL);
                int errorCode = Marshal.GetLastWin32Error();
                if (this.m_filehandle.IsInvalid)
                {
                    bool flag = false;
                    try
                    {
                        new FileIOPermission(FileIOPermissionAccess.PathDiscovery, new string[] { this.m_fileName }).Demand();
                        flag = true;
                    }
                    catch (SecurityException)
                    {
                    }
                    if (flag)
                    {
                        IoErrors.WinIOError(errorCode, this.m_fileName);
                    }
                    else
                    {
                        IoErrors.WinIOError(errorCode, Path.GetFileName(this.m_fileName));
                    }
                }
            }
            finally
            {
                Win32Io.SetErrorMode(newMode);
            }
            this.pos = 0L;
            if (seekToEnd)
            {
                this.SeekCore(0L, SeekOrigin.End);
            }
        }

        [SecurityCritical]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if ((this.m_filehandle == null) || this.m_filehandle.IsClosed)
                {
                    this.DiscardBuffer();
                }
            }
            finally
            {
                try
                {
                    if (this.m_writePos > 0)
                    {
                        this.FlushWrite(disposing);
                    }

                }
                finally
                {


                    if ((this.m_filehandle != null) && !this.m_filehandle.IsClosed)
                    {
                        this.m_filehandle.Dispose();
                    }
                    this.m_filehandle = null;
                    this.m_canRead = false;
                    this.m_canWrite = false;
                    this.m_canSeek = false;
                    
                    this.m_readPos = 0;
                    this.m_readLen = 0;
                    this.m_writePos = 0;
                    base.Dispose(disposing);
                }
            }
        }

        [SecurityCritical]
        private void EnforceHandlePolicy(SafeFileHandle handle, long lastPos)
        {
            switch (this.m_writeOption)
            {
                case LogWriteOption.UnlimitedSequentialFiles:
                case LogWriteOption.LimitedCircularFiles:
                case LogWriteOption.LimitedSequentialFiles:
                    if ((lastPos < this.m_maxFileSize) || (handle != this.m_filehandle))
                    {
                        return;
                    }
                    lock (this.mm_lockObject)
                    {
                        if ((handle == this.m_filehandle) && (lastPos >= this.m_maxFileSize))
                        {
                            this.m_currentFileNum++;
                            if ((this.m_writeOption == LogWriteOption.LimitedCircularFiles) && (this.m_currentFileNum > this.m_maxNumberOfFiles))
                            {
                                this.m_currentFileNum = 1;
                            }
                            else if ((this.m_writeOption == LogWriteOption.LimitedSequentialFiles) && (this.m_currentFileNum > this.m_maxNumberOfFiles))
                            {
                                this.m_DisableLogging();
                                return;
                            }
                            if (this.m_fileNameWithoutExt == null)
                            {
                                this.m_fileNameWithoutExt = Path.Combine(Path.GetDirectoryName(this.m_filePath), Path.GetFileNameWithoutExtension(this.m_filePath));
                                this.m_fileExt = Path.GetExtension(this.m_filePath);
                            }
                            string path = (this.m_currentFileNum == 1) ? this.m_filePath : (this.m_fileNameWithoutExt + this.m_currentFileNum.ToString(CultureInfo.InvariantCulture) + this.m_fileExt);
                            try
                            {
                                this.m_Init(path, this.m_fileAccess, this.m_fileShare, this.m_secAttrib, this.m_filePermission, this.m_fileMode, this.m_flagsAndAttributesFile, this.m_seekToEndFile);
                                if ((handle != null) && !handle.IsClosed)
                                {
                                    handle.Dispose();
                                }
                            }
                            catch (IOException)
                            {
                                this.m_filehandle = handle;
                                this.m_writeOptionRetryCount++;
                                if (this.m_writeOptionRetryCount >= 2)
                                {
                                    this.m_DisableLogging();
                                }
                            }
                            catch (UnauthorizedAccessException)
                            {
                                this.m_DisableLogging();
                            }
                            catch (Exception)
                            {
                                this.m_DisableLogging();
                            }
                        }
                        return;
                    }
                // break;

                case LogWriteOption.SingleFileUnboundedSize:
                    return;

                case LogWriteOption.SingleFileBoundedSize:
                    break;

                default:
                    return;
            }
            if (lastPos >= this.m_maxFileSize)
            {
                this.m_DisableLogging();
            }
        }

        [SecurityCritical]
        ~LogStream()
        {
            if (this.m_filehandle != null)
            {
                this.Dispose(false);
            }
        }

        [SecurityCritical]
        private static Win32Io.SecurityAttributes GetSecurityAttrib(FileShare share)
        {
            Win32Io.SecurityAttributes structure = null;
            if ((share & FileShare.Inheritable) != FileShare.None)
            {
                structure = new Win32Io.SecurityAttributes();
                structure.nLength = Marshal.SizeOf(structure);
                structure.bInheritHandle = 1;
            }
            return structure;
        }

        public override int Read(byte[] array, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        [SecurityCritical]
        private long SeekCore(long offset, SeekOrigin origin)
        {
            int hr = 0;
            long num2 = 0L;
            num2 = Win32Io.SetFilePointer(this.m_filehandle, offset, origin, out hr);
            if (num2 == -1L)
            {
                if (hr == 6)
                {
                    this.m_filehandle.SetHandleAsInvalid();
                }
                IoErrors.WinIOError(hr, string.Empty);
            }
            this.UnderlyingStreamPosition = num2;
            return num2;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        [SecurityCritical]
        protected void WriteCore(byte[] buffer, int offset, int count, bool blockForWrite, out long streamPos)
        {
            int hr = 0;
            int num2 = 0;
            unsafe
            {
                num2 = this.WriteFileNative(buffer, offset, count, null, out hr);
            }
            if (num2 == -1)
            {
                switch (hr)
                {
                    case 0xe8:
                        num2 = 0;
                        goto Labelm_ex;

                    case 0x57:
                        throw new IOException("File Too Long Or Handle Not Sync");
                }
                IoErrors.WinIOError(hr, string.Empty);
            }
        Labelm_ex:
            streamPos = this.AddUnderlyingStreamPosition((long)num2);
            this.EnforceHandlePolicy(this.m_filehandle, streamPos);
            streamPos = this.pos;
        }

        [SecurityCritical]
        private unsafe int WriteFileNative(byte[] bytes, int offset, int count, NativeOverlapped* overlapped, out int hr)
        {
            if (this.m_filehandle.IsClosed)
            {
                IoErrors.FileNotOpen();
            }
            if (this.m_disableLogging)
            {
                hr = 0;
                return 0;
            }
            if ((bytes.Length - offset) < count)
            {
                throw new IndexOutOfRangeException("Index Out Of Range, IO Race Condition");
            }
            if (bytes.Length == 0)
            {
                hr = 0;
                return 0;
            }
            int numBytesWritten = 0;
            int num2 = 0;
            fixed (byte* numRef = bytes)
            {
                num2 = Win32Io.WriteFile(this.m_filehandle, numRef + offset, count, out numBytesWritten, overlapped);
            }
            if (num2 == 0)
            {
                hr = Marshal.GetLastWin32Error();
                if (hr == 6)
                {
                    this.m_filehandle.SetHandleAsInvalid();
                }
                return -1;
            }
            hr = 0;
            return numBytesWritten;
        }

        public override bool CanRead
        {
            get
            {
                return this.m_canRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return this.m_canSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this.m_canWrite;
            }
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        private byte[] m_buffer;
        private int m_pendingBufferCopy;
        private int m_readLen;
        private int m_readPos;
        private int m_writePos;
        protected int bufferSize;
        protected internal const int DefaultBufferSize = 0x8000;
        protected long pos;

        protected long AddUnderlyingStreamPosition(long posDelta)
        {
            return Interlocked.Add(ref this.pos, posDelta);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected internal void DiscardBuffer()
        {
            this.m_readPos = 0;
            this.m_readLen = 0;
            this.m_writePos = 0;
        }

 
        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void Flush()
        {
            try
            {
                if (this.m_writePos > 0)
                {
                    this.FlushWrite(false);
                }
                else if (this.m_readPos < this.m_readLen)
                {
                    this.FlushRead();
                }
            }
            finally
            {
                this.m_writePos = 0;
                this.m_readPos = 0;
                this.m_readLen = 0;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void FlushRead()
        {
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void FlushWrite(bool blockForWrite)
        {
            if (this.m_writePos > 0)
            {
                this.WriteCore(this.m_buffer, 0, this.m_writePos, blockForWrite);
            }
            this.m_writePos = 0;
        }

        [SecuritySafeCritical]
        public void Write(string value, Encoding encoding)
        {
            if (value != null)
            {
                byte[] b = encoding.GetBytes(value);
                Write(b, 0, b.Length);
            }
        }

        [SecuritySafeCritical]
        public void Write(string value)
        {
            if (value != null)
            {
                byte[] b = Encoding.UTF8.GetBytes(value);
                Write(b, 0, b.Length);
            }
        }

        [SecuritySafeCritical]
        public void WriteFormat(string value, params object[] args)
        {
            if (value != null)
            {
                byte[] b = Encoding.UTF8.GetBytes(string.Format(value, args));
                Write(b, 0, b.Length);
             }
        }


        [SecuritySafeCritical]
        public void WriteLine(string value)
        {
            if (value != null)
            {
                byte[] b = Encoding.UTF8.GetBytes(value + "\r\n");
                Write(b, 0, b.Length);
             }
        }

        public override void Write(byte[] array, int offset, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array", "Argument Null, Buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "Argument Out Of Range, Need Non Negative Number");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "Argument Out Of Range, Need Non Negative Number");
            }
            if ((array.Length - offset) < count)
            {
                throw new ArgumentException("Argument error, Invalid Off Len");
            }
            if (this.m_writePos == 0)
            {
                if (!this.CanWrite)
                {
                    IoErrors.WriteNotSupported();
                }
                if (this.m_readPos < this.m_readLen)
                {
                    this.FlushRead();
                }
                this.m_readPos = 0;
                this.m_readLen = 0;
            }
            if (count == 0)
            {
                return;
            }
        Labelm_pos:
            while (this.m_writePos > this.bufferSize)
            {
                Thread.Sleep(1);
            }
            if ((this.m_writePos == 0) && (count >= this.bufferSize))
            {
                this.WriteCore(array, offset, count, true);
            }
            else
            {
                Thread.BeginCriticalRegion();
                Interlocked.Increment(ref this.m_pendingBufferCopy);
                int num = Interlocked.Add(ref this.m_writePos, count);
                int num2 = num - count;
                if (num > this.bufferSize)
                {
                    Interlocked.Decrement(ref this.m_pendingBufferCopy);
                    Thread.EndCriticalRegion();
                    if (((this.m_writePos > this.bufferSize) && (num2 <= this.bufferSize)) && (num2 > 0))
                    {
                        while (this.m_pendingBufferCopy != 0)
                        {
                            Thread.SpinWait(1);
                        }
                        this.WriteCore(this.m_buffer, 0, num2, true);
                        this.m_writePos = 0;
                    }
                    goto Labelm_pos;
                }
                if (this.m_buffer == null)
                {
                    Interlocked.CompareExchange<byte[]>(ref this.m_buffer, new byte[this.bufferSize], null);
                }
                Buffer.BlockCopy(array, offset, this.m_buffer, num2, count);
                Interlocked.Decrement(ref this.m_pendingBufferCopy);
                Thread.EndCriticalRegion();
            }
        }

        private void WriteCore(byte[] buffer, int offset, int count, bool blockForWrite)
        {
            long num;
            this.WriteCore(buffer, offset, count, blockForWrite, out num);
        }

        protected long UnderlyingStreamPosition
        {
            get
            {
                return this.pos;
            }
            set
            {
                Interlocked.Exchange(ref this.pos, value);
            }
        }

    }
}

