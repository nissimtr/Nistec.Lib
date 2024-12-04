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
using System.IO;
using System.Runtime;
using System.Security;
using System.Runtime.InteropServices;
using Nistec.Runtime;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Threading;

#pragma warning disable  CS1591

namespace Nistec.IO
{
  
    /// <summary>
    /// Provides a generic view of a sequence of bytes, using especially for serialization. 
    /// </summary>
    [Serializable]
    public class NetStream : Stream
    {
        #region members
        private byte[] m_buffer;
        private int m_capacity;
        private bool m_expandable;
        private bool m_exposable;
        private bool m_isOpen;
        private int m_length;
        private int m_origin;
        private int m_position;
        private bool m_writable;
        private const int MemStreamMaxLength = 0x7fffffff;
        #endregion

        #region static

        ///// <summary>
        ///// Convert given stream to <see cref="NetStream"/>.
        ///// </summary>
        ///// <param name="stream"></param>
        ///// <returns></returns>
        //public static NetStream EnsureNetStream(Stream stream)
        //{
        //    if (stream is MemoryStream)
        //    {
        //        return new NetStream(((MemoryStream)stream).ToArray());
        //    }
        //    if (stream is NetStream)
        //    {
        //        return (NetStream)stream;
        //    }

        //    var netStream = new NetStream();
        //    stream.CopyTo(netStream);
        //    return netStream;
        //}

        /// <summary>
        /// Copy stream to a new instance of <see cref="NetStream"/>.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="BufferSize"></param>
        /// <returns></returns>
        public static NetStream CopyStream(Stream stream, int BufferSize = 8192)
        {
            if (stream is MemoryStream)
            {
                return new NetStream(((MemoryStream)stream).ToArray());
            }
            if (stream is NetStream)
            {
                return new NetStream(((NetStream)stream).ToArray());
            }
            if (stream is PipeStream)
            {
                //return CopyStream((PipeStream)stream, 0);
                var ntStream = new NetStream();
                ntStream.CopyFrom((PipeStream)stream, BufferSize);
                return ntStream;
            }
            if (stream is NetworkStream)
            {
                var ntStream = new NetStream();
                //ntStream.CopyWithTerminateCount((NetworkStream)stream, 5000);
                ntStream.CopyFrom((NetworkStream)stream,0, BufferSize);
                return ntStream;
            }
            var netStream = new NetStream();
            stream.CopyTo(netStream);
            return netStream;
        }

        ///// <summary>
        ///// Copy <see cref="PipeStream"/> stream to a new instance of <see cref="NetStream"/>.
        ///// </summary>
        ///// <param name="stream"></param>
        ///// <param name="BufferSize"></param>
        ///// <returns></returns>
        //public static NetStream CopyStream(PipeStream stream, int BufferSize = 8192)
        //{
        //    if (BufferSize <= 0)
        //        BufferSize = 8192;
        //    NetStream ms = new NetStream();
        //    do
        //    {
        //        byte[] bytes = new byte[BufferSize];
        //        int bytesLength = bytes.Length;
        //        int cbRead = stream.Read(bytes, 0, bytesLength);
        //        ms.Write(bytes, 0, cbRead);
        //    }
        //    while (!stream.IsMessageComplete);

        //    if (ms.Length > 0)
        //    {
        //        ms.Position = 0;
        //    }
        //    return ms;
        //}

        #endregion

        #region Copy
        /// <summary>
        /// Clear the current stream and Copy the given <see cref="PipeStream"/> to the current <see cref="NetStream"/>.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="BufferSize"></param>
        public void CopyFrom(PipeStream stream, int BufferSize = 8192)
        {
            if (BufferSize <= 0)
                BufferSize = 8192;
            Array.Clear(this.m_buffer, 0, this.m_length);
            do
            {
                byte[] bytes = new byte[BufferSize];
                int bytesLength = bytes.Length;
                int cbRead = stream.Read(bytes, 0, bytesLength);
                Write(bytes, 0, cbRead);
            }
            while (!stream.IsMessageComplete);

            if (this.Length > 0)
            {
                this.Position = 0;
            }
        }

        /// <summary>
        /// Clear the current stream and Copy the given <see cref="PipeStream"/> to the current <see cref="NetStream"/>.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="BufferSize"></param>
        public void CopyBlock(PipeStream stream, int BufferSize = 8192)
        {
            if (BufferSize <= 0)
                BufferSize = 8192;
            //Array.Clear(this.m_buffer, 0, this.m_length);
            do
            {
                byte[] bytes = new byte[BufferSize];
                int bytesLength = bytes.Length;
                int cbRead = stream.Read(bytes, 0, bytesLength);
                Write(bytes, 0, cbRead);
            }
            while (!stream.IsMessageComplete);

            if (this.Length > 0)
            {
                this.Position = 0;
            }
        }

        /// <summary>
        /// Clear the current stream and Copy the given <see cref="NetworkStream"/> to the current <see cref="NetStream"/>.
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="readTimeout">specifies the amount of time, in milliseconds, that will elapse before a read operation fails.</param>
        /// <param name="BufferSize"></param>
        public int CopyWithTerminateCount(NetworkStream stream, int readTimeout, int BufferSize = 8192)
        {
            if (BufferSize <= 0)
                BufferSize = 8192;
           
            Array.Clear(this.m_buffer, 0, this.m_length);
            int totalWrite = 0;
            if (stream.CanRead)
            {
                int count = 0;
                stream.ReadTimeout = readTimeout;

                byte[] intbytes = new byte[4];
                stream.Read(intbytes, 0, 4);
                count = (((intbytes[0] | (intbytes[1] << 8)) | (intbytes[2] << 0x10)) | (intbytes[3] << 0x18));

                byte[] buffer = new byte[BufferSize];
                
                int bytesRead = 0;
                do
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    this.Write(buffer, 0, bytesRead);
                    totalWrite += bytesRead;
                }
                while (totalWrite < count && bytesRead > 0);


                //byte[] buffer = new byte[BufferSize];
                //int totalRead = 0;
                //int bytesRead = 0;
                //do
                //{
                //    bytesRead = stream.Read(buffer, 0, buffer.Length);
                //    this.Write(buffer, 0, bytesRead);
                //    totalRead += bytesRead;
                //    if (count == 0)
                //    {
                //        count = PeekInt32(1);
                //    }
                //}
                //while (totalRead < count && bytesRead > 0);

                //do
                //{
                //    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                //    totalRead += bytesRead;
                //    this.Write(buffer, 0, bytesRead);
                //}
                //while (stream.DataAvailable);

            }
            else
            {
                Console.WriteLine("Cannot read from this NetworkStream.");
            }


            if (this.Length > 0)
            {
                this.Position = 0;
            }
            return totalWrite;
        }


        /// <summary>
        /// Clear the current stream and Copy the given <see cref="NetworkStream"/> to the current <see cref="NetStream"/>.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="readTimeout">specifies the amount of time, in milliseconds, that will elapse before a read operation fails.</param>
        /// <param name="BufferSize"></param>
        /// <param name="offset"></param>
        public int CopyFrom(NetworkStream stream, int readTimeout, int BufferSize = 8192, int offset=0)
        {
            if (BufferSize <= 0)
                BufferSize = 8192;
            if (readTimeout == 0)// Infinite = -1
                readTimeout = 5000;
            Array.Clear(this.m_buffer, 0, this.m_length);
            int totalWrite = 0;

            if (stream.CanRead)
            {
                stream.ReadTimeout = readTimeout;
                int BytesRead = 0;
                byte[] ReadBuffer = new byte[BufferSize];

                do
                {
                    BytesRead = stream.Read(ReadBuffer, 0, ReadBuffer.Length);
                    this.Write(ReadBuffer, 0, BytesRead);
                    totalWrite += BytesRead;
                }
                while (stream.DataAvailable);// BytesRead > 0);
            }

            if (this.Length > 0)
            {
                this.Position = 0;
            }
            return totalWrite;
        }

        public int CopyBlock(NetworkStream stream, int readTimeout, int BufferSize = 8192)
        {
            if (BufferSize <= 0)
                BufferSize = 8192;
            if (readTimeout == 0)// Infinite = -1
                readTimeout = 5000;
            //Array.Clear(this.m_buffer, 0, this.m_length);
            int totalWrite = 0;

            if (stream.CanRead)
            {
                stream.ReadTimeout = readTimeout;
                int BytesRead = 0;
                byte[] ReadBuffer = new byte[BufferSize];

                do
                {
                    BytesRead = stream.Read(ReadBuffer, 0, ReadBuffer.Length);
                    this.Write(ReadBuffer, 0, BytesRead);
                    totalWrite += BytesRead;
                }
                while (stream.DataAvailable);// BytesRead > 0);
            }

            if (this.Length > 0)
            {
                this.Position = 0;
            }
            return totalWrite;
        }

        /// <summary>
        /// Get a copy of current stream.
        /// </summary>
        /// <returns></returns>
        public NetStream Copy()
        {
            Position = 0;
            return new NetStream(ToArray());
        }

        /// <summary>
        /// Get a copy of current stream.
        /// </summary>
        /// <returns></returns>
        public NetStream Copy(int index)
        {
            Position = 0;
            int length = this.iLength;
            if (index > length || index < 0)
            {
                throw new ArgumentException("index is out of current range");
            }
            return new NetStream(ToArray(), index, length - index);
        }

        /// <summary>
        /// Get a copy of current stream.
        /// </summary>
        /// <returns></returns>
        public NetStream Copy(int index, int count)
        {
            Position = 0;
            int length = this.iLength;
            if (index > length || index < 0 || count < 0)
            {
                throw new ArgumentException("index or count is out of current range");
            }
            if ((count + index) > length)
            {
                throw new ArgumentException("index+count is out of current range");
            }
            return new NetStream(ToArray(), index, count);
        }

        /// <summary>
        /// Get the current stream ready to seek in position 0.
        /// </summary>
        /// <returns></returns>
        public NetStream Ready()
        {
            if (CanSeek && (this.m_position - this.m_origin) > 0)
            {
                Position = 0;
            }
            return this;
        }

        /// <summary>
        /// Convert to base 64 string.
        /// </summary>
        /// <returns></returns>
        public string ToBase64String()
        {
            return Convert.ToBase64String(this.ToArray());
        }
        /// <summary>
        /// Get a new instance of <see cref="NetStream"/> converted from base 64 string.
        /// </summary>
        /// <param name="base64String"></param>
        /// <returns></returns>
        public static NetStream FromBase64String(string base64String)
        {
            byte[] b = Convert.FromBase64String(base64String);
            return new NetStream(b);
        }

        public static byte[] GetBytes(string text)
        {
            return Encoding.UTF8.GetBytes(text ?? "");
        }

        /// <summary>
        /// Write text to stream
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static NetStream WriteTo(string text)
        {
            return new NetStream(Encoding.UTF8.GetBytes(text ?? ""));

            //var stream = new NetStream();
            //var writer = new StreamWriter(stream);
            //writer.Write(text);
            //writer.Flush();
            //stream.Position = 0;
            //return stream;
        }
        /// <summary>
        /// Read stream to text
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string ReadTo(NetStream stream)
        {
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        /// <summary>
        /// Save the current stream to file.
        /// </summary>
        /// <param name="filename"></param>
        public void SaveToFile(string filename)
        {
             using (FileStream file = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                this.WriteTo(file);
            }
        }

        public void Clear()
        {
            Array.Clear(this.m_buffer, 0, this.m_length);
        }
        #endregion

        #region ctor
        /// <summary>
        /// Initialize a new instance of NetStream.
        /// </summary>
        public NetStream()
            : this(0)
        {
        }
        /// <summary>
        /// Initialize a new instance of NetStream.
        /// </summary>
        /// <param name="capacity"></param>
        public NetStream(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException("capacity", "Argument Out Of Range, Negative Capacity Not Allowed");
            }
            this.m_buffer = new byte[capacity];
            this.m_capacity = capacity;
            this.m_expandable = true;
            this.m_writable = true;
            this.m_exposable = true;
            this.m_origin = 0;
            this.m_isOpen = true;
        }
        /// <summary>
        /// Initialize a new instance of NetStream.
        /// </summary>
        /// <param name="buffer"></param>
        public NetStream(byte[] buffer)
            : this(buffer, true)
        {
        }
        /// <summary>
        /// Initialize a new instance of NetStream.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="writable"></param>
        public NetStream(byte[] buffer, bool writable)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer", "Argument Null.Buffer");
            }
            this.m_buffer = buffer;
            this.m_length = this.m_capacity = buffer.Length;
            this.m_writable = writable;
            this.m_exposable = false;
            this.m_origin = 0;
            this.m_isOpen = true;
        }
        /// <summary>
        /// Initialize a new instance of NetStream.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        public NetStream(byte[] buffer, int index, int count)
            : this(buffer, index, count, true, false)
        {
        }
        /// <summary>
        /// Initialize a new instance of NetStream.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <param name="writable"></param>
        public NetStream(byte[] buffer, int index, int count, bool writable)
            : this(buffer, index, count, writable, false)
        {
        }
        /// <summary>
        /// Initialize a new instance of NetStream.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <param name="writable"></param>
        /// <param name="publiclyVisible"></param>
        public NetStream(byte[] buffer, int index, int count, bool writable, bool publiclyVisible)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer", "Argument Null.Buffer");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", "Argument Out Of Range, Negative number not allowed");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "Argument Out Of Range, Negative number not allowed");
            }
            if ((buffer.Length - index) < count)
            {
                throw new ArgumentException("Argument Invalid Length");
            }
            this.m_buffer = buffer;
            this.m_origin = this.m_position = index;
            this.m_length = this.m_capacity = index + count;
            this.m_writable = writable;
            this.m_exposable = publiclyVisible;
            this.m_expandable = false;
            this.m_isOpen = true;
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this.m_isOpen = false;
                    this.m_writable = false;
                    this.m_expandable = false;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        #endregion

        #region peek and replace
        ///// <summary>
        ///// Peek a bytes from given offset and return a <see cref="DateTime"/> value.
        ///// </summary>
        ///// <param name="offset"></param>
        ///// <returns></returns>
        //public DateTime PeekDateTime(int offset)
        //{
        //    long g = PeekInt64(offset);

        //    return DateTime.FromBinary(g);
        //}

        /// <summary>
        /// Peek a bytes from given offset and return a <see cref="Int64"/> value.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public Int64 PeekInt64(int offset)
        {
            ValidatePeak(offset, 8);

            uint count1 = (uint)(((this.m_buffer[offset + 0] | (this.m_buffer[offset + 1] << 8)) | (this.m_buffer[offset + 2] << 0x10)) | (this.m_buffer[offset + 3] << 0x18));
            uint count2 = (uint)(((this.m_buffer[offset + 4] | (this.m_buffer[offset + 5] << 8)) | (this.m_buffer[offset + 6] << 0x10)) | (this.m_buffer[offset + 7] << 0x18));
            long g = (long)((count2 << 0x20) | count1);

            return g;

        }
        /// <summary>
        /// Peek a bytes from given offset and return a <see cref="Int32"/> value.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public int PeekInt32(int offset)
        {
            ValidatePeak(offset, 4);
            offset += 4;
            return (((this.m_buffer[offset - 4] | (this.m_buffer[offset - 3] << 8)) | (this.m_buffer[offset - 2] << 0x10)) | (this.m_buffer[offset - 1] << 0x18));
        }
        /// <summary>
        /// Peek a byte from given offset and return it as a <see cref="Int32"/> value.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public int PeekByte(int offset)
        {
            ValidatePeak(offset, 0);
            return this.m_buffer[offset];
        }

        ///// <summary>
        ///// Peek a range of bytes from given offset and count.
        ///// </summary>
        ///// <param name="offset"></param>
        ///// <param name="count"></param>
        ///// <returns></returns>
        //[SecuritySafeCritical]
        //public string PeekString(int offset, int count)
        //{
        //    byte[] b = PeekBytes(offset, count);
        //    if (b == null)
        //        return null;
        //    return Encoding.UTF8.GetString(b);
        //}

        ///// <summary>
        ///// Peek a range of bytes from given offset and offset+1 for count.
        ///// </summary>
        ///// <param name="offset"></param>
        ///// <param name="length"></param>
        ///// <returns></returns>
        //[SecuritySafeCritical]
        //public string PeekString(int offset, out int length)
        //{
        //    int count = PeekInt32(offset+1);
        //    length = count;
        //    return PeekString(offset, count);
        //}

        ///// <summary>
        ///// Peek a range of the last bytes from given count.
        ///// </summary>
        ///// <param name="offset"></param>
        ///// <param name="count"></param>
        ///// <returns></returns>
        //[SecuritySafeCritical]
        //public byte[] PeekLastBytes(int count)
        //{
        //    int offset = iLength - count;
        //    return PeekBytes(offset, count);
        //}

        /// <summary>
        /// Peek a range of bytes from given offset and count.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        [SecuritySafeCritical]
        public byte[] PeekBytes(int offset, int count)
        {
            ValidatePeak(offset, count);

            //if ((m_buffer.Length - offset) < count)
            //{
            //    throw new ArgumentException("Argument Invalid Off Len");
            //}

            byte[] buffer = new byte[count];

            int byteCount = count;

            if (byteCount <= 0)
            {
                return null;
            }
            if (byteCount <= 8)
            {
                int icount = byteCount;
                while (--icount >= 0)
                {
                    buffer[0 + icount] = this.m_buffer[offset + icount];
                }
            }
            else
            {
                StreamBuffer.StreamBlockCopy(this.m_buffer, offset, buffer, 0, byteCount);
            }

            return buffer;
        }

        void ValidatePeak(int offset, int count)
        {
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "Argument Out Of Range, Need Non Negative Num");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "Argument Out Of Range, Need Non Negative Num");
            }
            if (!this.m_isOpen)
            {
                IoErrors.StreamIsClosed();
            }
            if (!this.CanRead)
            {
                IoErrors.ReadNotSupported();
            }
            if (offset + count >= this.m_length)
            {
                throw new ArgumentException("Argument Invalid Off Len");
            }
            //if ((m_buffer.Length - offset) < count)
            //{
            //    throw new ArgumentException("Argument Invalid Off Len");
            //}
        }

        void ValidateReplace(int offset, int count)
        {
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "Argument Out Of Range, Need Non Negative Num");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "Argument Out Of Range, Need Non Negative Num");
            }
            if (!this.m_isOpen)
            {
                IoErrors.StreamIsClosed();
            }
            if (!this.CanWrite)
            {
                IoErrors.WriteNotSupported();
            }
            if (offset + count >= this.m_length)
            {
                throw new ArgumentException("Argument Invalid Off Len");
            }
        }
        /// <summary>
        /// Replace a range of bytes in buffer to an int value from offset and value parameters.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="offset"></param>
        public void Replace(int value, int offset)
        {
            ValidateReplace(offset, 4);

            this.m_buffer[0 + offset] = (byte)value;
            this.m_buffer[1 + offset] = (byte)(value >> 8);
            this.m_buffer[2 + offset] = (byte)(value >> 0x10);
            this.m_buffer[3 + offset] = (byte)(value >> 0x18);
            //this.m_stream.Write(this.m_buffer, 0, 4);
        }
        /// <summary>
        /// Replace a range of bytes in buffer to an long value from offset and value parameters.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="offset"></param>
        public void Replace(long value, int offset)
        {
            ValidateReplace(offset, 8);
            this.m_buffer[0 + offset] = (byte)value;
            this.m_buffer[1 + offset] = (byte)(value >> 8);
            this.m_buffer[2 + offset] = (byte)(value >> 0x10);
            this.m_buffer[3 + offset] = (byte)(value >> 0x18);
            this.m_buffer[4 + offset] = (byte)(value >> 0x20);
            this.m_buffer[5 + offset] = (byte)(value >> 40);
            this.m_buffer[6 + offset] = (byte)(value >> 0x30);
            this.m_buffer[7 + offset] = (byte)(value >> 0x38);
            //this.m_stream.Write(this.m_buffer, 0, 8);
        }
        /// <summary>
        /// Replace a range of bytes in buffer to an byte value from offset and value parameters.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="offset"></param>
        public void Replace(byte value, int offset)
        {
            ValidateReplace(offset, 0);
            this.m_buffer[offset] = value;
        }

        /// <summary>
        /// Replace a range of bytes in buffer from byte array and offset parameters.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        [SecuritySafeCritical]
        public void Replace(byte[] buffer, int offset, int count)
        {
            ValidateReplace(offset, count);

            if (buffer == null)
            {
                throw new ArgumentNullException("buffer", "Argument Null Buffer");
            }

            //if ((m_buffer.Length - offset) < count)
            //{
            //    throw new ArgumentException("Argument Invalid Off Len");
            //}

            int val = offset + count;
            if (val < 0)
            {
                throw new IOException("IO Stream TooLong");
            }
            if (val > this.m_length)
            {
                throw new IOException("Buffer Stream Too Long");
            }
            if ((count <= 8) && (buffer != this.m_buffer))
            {
                int icount = count;
                while (--icount >= 0)
                {
                    this.m_buffer[offset + icount] = buffer[offset + icount];
                }
            }
            else
            {
                StreamBuffer.StreamBlockCopy(buffer, 0, this.m_buffer, offset, count);
            }
            //this.m_position = val;
            //WriteCore(buffer, offset, count);
        }

        #endregion

        #region methods

        internal bool EnsureCapacity(int value)
        {
            if (value < 0)
            {
                throw new IOException("IO Stream Too Long");
            }
            if (value <= this.m_capacity)
            {
                return false;
            }
            int val = value;
            if (val < 0x100)
            {
                val = 0x100;
            }
            if (val < (this.m_capacity * 2))
            {
                val = this.m_capacity * 2;
            }
            this.Capacity = val;
            return true;
        }
        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override void Flush()
        {
        }
        /// <summary>
        /// Get the current buffer as byte array.
        /// </summary>
        /// <returns></returns>
        public virtual byte[] GetBuffer()
        {
            if (!this.m_exposable)
            {
                throw new UnauthorizedAccessException("Unauthorized Access NetStream Buffer");
            }
            return this.m_buffer;
        }

        internal int InternalEmulateRead(int count)
        {
            if (!this.m_isOpen)
            {
                IoErrors.StreamIsClosed();
            }
            int val = this.m_length - this.m_position;
            if (val > count)
            {
                val = count;
            }
            if (val < 0)
            {
                val = 0;
            }
            this.m_position += val;
            return val;
        }

        internal byte[] InternalGetBuffer()
        {
            return this.m_buffer;
        }

        internal void InternalGetOriginAndLength(out int origin, out int length)
        {
            if (!this.m_isOpen)
            {
                IoErrors.StreamIsClosed();
            }
            origin = this.m_origin;
            length = this.m_length;
        }

        internal int InternalGetPosition()
        {
            if (!this.m_isOpen)
            {
                IoErrors.StreamIsClosed();
            }
            return this.m_position;
        }

        internal int InternalReadInt32()
        {
            if (!this.m_isOpen)
            {
                IoErrors.StreamIsClosed();
            }
            int val = this.m_position += 4;
            if (val > this.m_length)
            {
                this.m_position = this.m_length;
                IoErrors.EndOfFile();
            }
            return (((this.m_buffer[val - 4] | (this.m_buffer[val - 3] << 8)) | (this.m_buffer[val - 2] << 0x10)) | (this.m_buffer[val - 1] << 0x18));
        }

        /// <summary>
        /// Provides support for a System.Diagnostics.Contracts.Contract.
        /// </summary>
        [Obsolete]
        protected override void ObjectInvariant()
        {
        }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        [SecuritySafeCritical]
        public override int Read([In, Out] byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer", "Argument Null Buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "Argument Out Of Range, Need Non Negative Num");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "Argument Out Of Range, Need Non Negative Num");
            }
            if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException("Argument Invalid Off Len");
            }
            if (!this.m_isOpen)
            {
                IoErrors.StreamIsClosed();
            }
            int byteCount = this.m_length - this.m_position;
            if (byteCount > count)
            {
                byteCount = count;
            }
            if (byteCount <= 0)
            {
                return 0;
            }
            if (byteCount <= 8)
            {
                int icount = byteCount;
                while (--icount >= 0)
                {
                    buffer[offset + icount] = this.m_buffer[this.m_position + icount];
                }
            }
            else
            {
                StreamBuffer.StreamBlockCopy(this.m_buffer, this.m_position, buffer, offset, byteCount);
            }
            this.m_position += byteCount;
            ReadCore(buffer, offset, count);
            return byteCount;
        }

        //public override int Read([In, Out] byte[] buffer, int offset, int count, Action<byte[]> action)
        //{

        //}
        /// <summary>
        /// Read Core is a virtual method for overriding when read.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        [SecurityCritical]
        protected virtual void ReadCore(byte[] buffer, int offset, int count)
        {
            
        }

        /// <summary>
        /// Read a bytes from given offset and return a <see cref="Int32"/> value.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public int ReadInt32(int offset)
        {
            byte[] buffer = new byte[4];
            int readed = Read(buffer, offset, 4);
            return (((buffer[offset + 0] | (buffer[offset + 1] << 8)) | (buffer[offset + 2] << 0x10)) | (buffer[offset + 3] << 0x18));
        }

        /// <summary>
        /// Reads a byte from the stream and advances the position within the stream
        ///     by one byte, or returns -1 if at the end of the stream.
        /// </summary>
        /// <returns></returns>
        public override int ReadByte()
        {
            if (!this.m_isOpen)
            {
                IoErrors.StreamIsClosed();
            }
            if (this.m_position >= this.m_length)
            {
                return -1;
            }
            return this.m_buffer[this.m_position++];
        }
        /// <summary>
        /// Sets the position within the current stream.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="loc"></param>
        /// <returns></returns>
        public override long Seek(long offset, SeekOrigin loc)
        {
            if (!this.m_isOpen)
            {
                IoErrors.StreamIsClosed();
            }
            if (offset > 0x7fffffffL)
            {
                throw new ArgumentOutOfRangeException("offset", "Argument Out Of Range, Stream Length");
            }
            switch (loc)
            {
                case SeekOrigin.Begin:
                    {
                        int val = this.m_origin + ((int)offset);
                        if ((offset < 0L) || (val < this.m_origin))
                        {
                            throw new IOException("IO Seek Before Begin");
                        }
                        this.m_position = val;
                        break;
                    }
                case SeekOrigin.Current:
                    {
                        int ipos = this.m_position + ((int)offset);
                        if (((this.m_position + offset) < this.m_origin) || (ipos < this.m_origin))
                        {
                            throw new IOException("IO Seek Before Begin");
                        }
                        this.m_position = ipos;
                        break;
                    }
                case SeekOrigin.End:
                    {
                        int iendpos = this.m_length + ((int)offset);
                        if (((this.m_length + offset) < this.m_origin) || (iendpos < this.m_origin))
                        {
                            throw new IOException("IO Seek Before Begin");
                        }
                        this.m_position = iendpos;
                        break;
                    }
                default:
                    throw new ArgumentException("Argument Invalid Seek Origin");
            }
            return (long)this.m_position;
        }
        /// <summary>
        /// Sets the length of the current stream.
        /// </summary>
        /// <param name="value"></param>
        public override void SetLength(long value)
        {
            if ((value < 0L) || (value > 0x7fffffffL))
            {
                throw new ArgumentOutOfRangeException("value", "Argument Out Of Range, Stream Length");
            }
            if (!this.CanWrite)
            {
                IoErrors.WriteNotSupported();
            }
            if (value > (0x7fffffff - this.m_origin))
            {
                throw new ArgumentOutOfRangeException("value", "Argument Out Of Range, Stream Length");
            }
            int val = this.m_origin + ((int)value);
            if (!this.EnsureCapacity(val) && (val > this.m_length))
            {
                Array.Clear(this.m_buffer, this.m_length, val - this.m_length);
            }
            this.m_length = val;
            if (this.m_position > val)
            {
                this.m_position = val;
            }
        }
        /// <summary>
        /// Get the current stream as a byte array.
        /// </summary>
        /// <returns></returns>
        [SecuritySafeCritical]
        public virtual byte[] ToArray()
        {
            byte[] dst = new byte[this.m_length - this.m_origin];
            StreamBuffer.StreamBlockCopy(this.m_buffer, this.m_origin, dst, 0, this.m_length - this.m_origin);
            return dst;
        }
        /// <summary>
        /// Writes a sequence of bytes to the current
        ///     stream and advances the current position within this stream by the number
        ///     of bytes written.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        [SecuritySafeCritical]
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer", "Argument Null Buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "Argument Out Of Range, Need Non Negative Num");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "Argument Out Of Range, Need Non Negative Num");
            }
            if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException("Argument Invalid Off Len");
            }
            if (!this.m_isOpen)
            {
                IoErrors.StreamIsClosed();
            }
            if (!this.CanWrite)
            {
                IoErrors.WriteNotSupported();
            }
            int val = this.m_position + count;
            if (val < 0)
            {
                throw new IOException("IO Stream TooLong");
            }
            if (val > this.m_length)
            {
                bool flag = this.m_position > this.m_length;
                if ((val > this.m_capacity) && this.EnsureCapacity(val))
                {
                    flag = false;
                }
                if (flag)
                {
                    Array.Clear(this.m_buffer, this.m_length, val - this.m_length);
                }
                this.m_length = val;
            }
            if ((count <= 8) && (buffer != this.m_buffer))
            {
                int icount = count;
                while (--icount >= 0)
                {
                    this.m_buffer[this.m_position + icount] = buffer[offset + icount];
                }
            }
            else
            {
                StreamBuffer.StreamBlockCopy(buffer, offset, this.m_buffer, this.m_position, count);
            }
            this.m_position = val;
            WriteCore(buffer, offset, count);
        }

        /// <summary>
        /// Write Core is a virtual method for overriding when write.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        [SecurityCritical]
        protected virtual void WriteCore(byte[] buffer, int offset, int count)
        {

        }
        /// <summary>
        ///  Writes a byte to the current position in the stream and advances the position
        ///     within the stream by one byte.
        /// </summary>
        /// <param name="value"></param>
        public override void WriteByte(byte value)
        {
            if (!this.m_isOpen)
            {
                IoErrors.StreamIsClosed();
            }
            if (!this.CanWrite)
            {
                IoErrors.WriteNotSupported();
            }
            if (this.m_position >= this.m_length)
            {
                int val = this.m_position + 1;
                bool flag = this.m_position > this.m_length;
                if ((val >= this.m_capacity) && this.EnsureCapacity(val))
                {
                    flag = false;
                }
                if (flag)
                {
                    Array.Clear(this.m_buffer, this.m_length, this.m_position - this.m_length);
                }
                this.m_length = val;
            }
            this.m_buffer[this.m_position++] = value;
        }
        /// <summary>
        /// Write the current stream to a given stream.
        /// </summary>
        /// <param name="stream"></param>
        public virtual void WriteTo(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream", "Argument Null Stream");
            }
            if (!this.m_isOpen)
            {
                IoErrors.StreamIsClosed();
            }
            stream.Write(this.m_buffer, this.m_origin, this.m_length - this.m_origin);
        }

        /// <summary>
        /// Write the current stream to a given stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        /// <param name="limitCount"></param>
        public virtual void Write(NetStream stream, int offset=0, int limitCount = 0)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream", "Argument Null Stream");
            }
            if (!this.m_isOpen)
            {
                IoErrors.StreamIsClosed();
            }
            int length = stream.iLength;

            if (limitCount > 0)
            {
                length = limitCount;
            }
            Write(stream.ToArray(), offset, length);
        }
        #endregion

        #region properties

        /// <summary>
        /// Get a value indicating whether the current
        ///     stream supports reading.
        /// </summary>
        public override bool CanRead
        {
            get
            {
                return this.m_isOpen;
            }
        }
        /// <summary>
        /// Get a value indicating whether the current
        ///     stream supports seeking.
        /// </summary>
        public override bool CanSeek
        {
            get
            {
                return this.m_isOpen;
            }
        }
        /// <summary>
        /// Get a value indicating whether the current
        ///     stream supports writing.
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                return this.m_writable;
            }
        }
        /// <summary>
        /// Get or Set the capacity of current stream.
        /// </summary>
        public virtual int Capacity
        {
            get
            {
                if (!this.m_isOpen)
                {
                    IoErrors.StreamIsClosed();
                }
                return (this.m_capacity - this.m_origin);
            }
            [SecuritySafeCritical]
            set
            {
                if (value < this.Length)
                {
                    throw new ArgumentOutOfRangeException("value", "Argument Out Of Range Small Capacity");
                }
                if (!this.m_isOpen)
                {
                    IoErrors.StreamIsClosed();
                }
                if (!this.m_expandable && (value != this.Capacity))
                {
                    IoErrors.MemoryStreamNotExpandable();
                }
                if (this.m_expandable && (value != this.m_capacity))
                {
                    if (value > 0)
                    {
                        byte[] dst = new byte[value];
                        if (this.m_length > 0)
                        {
                            StreamBuffer.StreamBlockCopy(this.m_buffer, 0, dst, 0, this.m_length);
                        }
                        this.m_buffer = dst;
                    }
                    else
                    {
                        this.m_buffer = null;
                    }
                    this.m_capacity = value;
                }
            }
        }
        /// <summary>
        /// Get the length in bytes of the stream.
        /// </summary>
        public override long Length
        {
            get
            {
                if (!this.m_isOpen)
                {
                    IoErrors.StreamIsClosed();
                }
                return (long)(this.m_length - this.m_origin);
            }
        }
        /// <summary>
        /// Get the length in bytes of the stream as int.
        /// </summary>
        public int iLength
        {
            get
            {
                if (!this.m_isOpen)
                {
                    IoErrors.StreamIsClosed();
                }
                return (this.m_length - this.m_origin);
            }
        }
        /// <summary>
        /// Get or sets the position within the
        ///     current stream.
        /// </summary>
        public override long Position
        {
            get
            {
                if (!this.m_isOpen)
                {
                    IoErrors.StreamIsClosed();
                }
                return (long)(this.m_position - this.m_origin);
            }
            set
            {
                if (value < 0L)
                {
                    throw new ArgumentOutOfRangeException("value", "Argument Out Of Range, Need Non Negative Num");
                }
                if (!this.m_isOpen)
                {
                    IoErrors.StreamIsClosed();
                }
                if (value > 0x7fffffffL)
                {
                    throw new ArgumentOutOfRangeException("value", "Argument Out Of Range, Stream Length");
                }
                this.m_position = this.m_origin + ((int)value);
            }
        }
        #endregion
    }
}
