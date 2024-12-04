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
using System.Security;
using System.IO;
using System.Runtime;
using System.Collections;
using Nistec.Serialization;

#pragma warning disable  CS1591

namespace Nistec.IO
{

      /// <summary>
    /// Represent a binary streamer such as binary write and reader.
    /// </summary>
    public static class BinaryConverter
    {

        #region writer
       
       
        [SecuritySafeCritical]
        public static unsafe void Write(double value, Stream stream)
        {
            stream.Write(GetBytes(value), 0, 8);
        }

        public static void Write(char[] chars, Encoding encoding, Stream stream)
        {

            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }
            byte[] buffer = encoding.GetBytes(chars, 0, chars.Length);
            stream.Write(buffer, 0, buffer.Length);
        }

        public static void Write(short value, Stream stream)
        {
            stream.Write(GetBytes(value), 0, 2);
        }

        public static void Write(int value, Stream stream)
        {
             stream.Write(GetBytes(value), 0, 4);
        }

        public static void Write(long value, Stream stream)
        {
            stream.Write(GetBytes(value), 0, 8);
        }

        public static void Write(sbyte value, Stream stream)
        {
            stream.WriteByte((byte)value);
        }

        [SecuritySafeCritical]
        public static unsafe void Write(float value, Stream stream)
        {
            stream.Write(GetBytes(value), 0, 4);
        }


        public static void Write(ushort value, Stream stream)
        {
             stream.Write(GetBytes(value), 0, 2);
        }

        public static void Write(uint value, Stream stream)
        {
            stream.Write(GetBytes(value), 0, 4);
        }

        public static void Write(ulong value, Stream stream)
        {
            stream.Write(GetBytes(value), 0, 8);
        }

       
        #endregion

        #region GetBytes
        
        public static byte[] GetBytes(decimal d)
        {
            byte[] bytes = new byte[16];

            int[] bits = decimal.GetBits(d);
            int lo = bits[0];
            int mid = bits[1];
            int hi = bits[2];
            int flags = bits[3];

            bytes[0] = (byte)lo;
            bytes[1] = (byte)(lo >> 8);
            bytes[2] = (byte)(lo >> 0x10);
            bytes[3] = (byte)(lo >> 0x18);
            bytes[4] = (byte)mid;
            bytes[5] = (byte)(mid >> 8);
            bytes[6] = (byte)(mid >> 0x10);
            bytes[7] = (byte)(mid >> 0x18);
            bytes[8] = (byte)hi;
            bytes[9] = (byte)(hi >> 8);
            bytes[10] = (byte)(hi >> 0x10);
            bytes[11] = (byte)(hi >> 0x18);
            bytes[12] = (byte)flags;
            bytes[13] = (byte)(flags >> 8);
            bytes[14] = (byte)(flags >> 0x10);
            bytes[15] = (byte)(flags >> 0x18);

            return bytes;
        }



        [SecuritySafeCritical]
        public static unsafe byte[] GetBytes(double value)
        {
            byte[] buffer = new byte[8];
            ulong num = *((ulong*)&value);
            buffer[0] = (byte)num;
            buffer[1] = (byte)(num >> 8);
            buffer[2] = (byte)(num >> 0x10);
            buffer[3] = (byte)(num >> 0x18);
            buffer[4] = (byte)(num >> 0x20);
            buffer[5] = (byte)(num >> 40);
            buffer[6] = (byte)(num >> 0x30);
            buffer[7] = (byte)(num >> 0x38);
            return buffer;
        }

        public static byte[] GetBytes(char[] chars, Encoding encoding)
        {

            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }
            byte[] buffer = encoding.GetBytes(chars, 0, chars.Length);
            return buffer;
        }

        public static byte[] GetBytes(char[] chars)
        {
            return GetBytes(chars, Encoding.UTF8);
        }

        public static byte[] GetBytes(char value)
        {
            byte[] buffer = new byte[2];
            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
            return buffer;
        }

        [SecuritySafeCritical]
        public unsafe static byte[] GetBytes(short value)
        {
            byte[] buffer = new byte[2];
            fixed (byte* numRef = buffer)
            {
                *((short*)numRef) = value;
            }
            return buffer;

        }

        [SecuritySafeCritical]
        public unsafe static byte[] GetBytes(int value)
        {
            byte[] buffer = new byte[4];
            fixed (byte* numRef = buffer)
            {
                *((int*)numRef) = value;
            }
            return buffer;

        }

        [SecuritySafeCritical]
        public unsafe static byte[] GetBytes(long value)
        {
            byte[] buffer = new byte[8];
            fixed (byte* numRef = buffer)
            {
                *((long*)numRef) = value;
            }
            return buffer;

        }

        public static byte[] GetBytes(sbyte value)
        {
           return GetBytes((byte)value);
        }

        [SecuritySafeCritical]
        public static unsafe byte[] GetBytes(float value)
        {
            return GetBytes(*((int*)&value));
        }


        public static byte[] GetBytes(ushort value)
        {
            return GetBytes((short)value);
        }

        public static byte[] GetBytes(uint value)
        {
            return GetBytes((int)value);
        }

        public static byte[] GetBytes(ulong value)
        {
            return GetBytes((long)value);
        }

        #endregion

        #region reader

        [SecuritySafeCritical]
        public static decimal ReadDecimal(byte[] bytes)
        {
            int[] bits = new int[4];
            bits[0] = ((bytes[0] | (bytes[1] << 8)) | (bytes[2] << 0x10)) | (bytes[3] << 0x18); //lo
            bits[1] = ((bytes[4] | (bytes[5] << 8)) | (bytes[6] << 0x10)) | (bytes[7] << 0x18); //mid
            bits[2] = ((bytes[8] | (bytes[9] << 8)) | (bytes[10] << 0x10)) | (bytes[11] << 0x18); //hi
            bits[3] = ((bytes[12] | (bytes[13] << 8)) | (bytes[14] << 0x10)) | (bytes[15] << 0x18); //flags

            return new decimal(bits);
        }

        [SecuritySafeCritical]
        public static decimal ReadDecimal(byte[] bytes, int offset)
        {
            int[] bits = new int[4];
            bits[0] = ((bytes[offset + 0] | (bytes[offset + 1] << 8)) | (bytes[offset + 2] << 0x10)) | (bytes[offset + 3] << 0x18); //lo
            bits[1] = ((bytes[offset + 4] | (bytes[offset + 5] << 8)) | (bytes[offset + 6] << 0x10)) | (bytes[offset + 7] << 0x18); //mid
            bits[2] = ((bytes[offset + 8] | (bytes[offset + 9] << 8)) | (bytes[offset + 10] << 0x10)) | (bytes[offset + 11] << 0x18); //hi
            bits[3] = ((bytes[offset + 12] | (bytes[offset + 13] << 8)) | (bytes[offset + 14] << 0x10)) | (bytes[offset + 15] << 0x18); //flags

            return new decimal(bits);
        }

        [SecuritySafeCritical]
        public static unsafe double ReadDouble(byte[] buffer)
        {
            uint num = (uint)(((buffer[0] | (buffer[1] << 8)) | (buffer[2] << 0x10)) | (buffer[3] << 0x18));
            uint num2 = (uint)(((buffer[4] | (buffer[5] << 8)) | (buffer[6] << 0x10)) | (buffer[7] << 0x18));
            ulong num3 = (num2 << 0x20) | num;
            return *(((double*)&num3));
        }

        [SecuritySafeCritical]
        public static unsafe double ReadDouble(byte[] buffer, int offset)
        {
            uint num = (uint)(((buffer[offset + 0] | (buffer[offset + 1] << 8)) | (buffer[offset + 2] << 0x10)) | (buffer[offset + 3] << 0x18));
            uint num2 = (uint)(((buffer[offset + 4] | (buffer[offset + 5] << 8)) | (buffer[offset + 6] << 0x10)) | (buffer[offset + 7] << 0x18));
            ulong num3 = (num2 << 0x20) | num;
            return *(((double*)&num3));
        }

        public static short ReadInt16(byte[] buffer)
        {
            return (short)(buffer[0] | (buffer[1] << 8));
        }

        public static short ReadInt16(byte[] buffer, int offset)
        {
            return (short)(buffer[offset + 0] | (buffer[offset + 1] << 8));
        }

        public static int ReadInt32(byte[] buffer)
        {
            return (((buffer[0] | (buffer[1] << 8)) | (buffer[2] << 0x10)) | (buffer[3] << 0x18));
        }

        public static int ReadInt32(byte[] buffer, int offset)
        {
            return (((buffer[offset + 0] | (buffer[offset + 1] << 8)) | (buffer[offset + 2] << 0x10)) | (buffer[offset + 3] << 0x18));
        }

        [SecuritySafeCritical]
        public unsafe static long ReadInt64(byte[] buffer)
        {
            fixed (byte* numRef = &(buffer[0]))
            {
                return *(((long*)numRef));
            }
        }

        [SecuritySafeCritical]
        public unsafe static long ReadInt64(byte[] buffer, int offset)
        {
            bool IsLittleEndian = true;

            fixed (byte* numRef = &(buffer[offset]))
            {
                if ((offset % 8) == 0)
                {
                    return *(((long*)numRef));
                }
                if (IsLittleEndian)
                {
                    int num = ((numRef[0] | (numRef[1] << 8)) | (numRef[2] << 0x10)) | (numRef[3] << 0x18);
                    int num2 = ((numRef[4] | (numRef[5] << 8)) | (numRef[6] << 0x10)) | (numRef[7] << 0x18);
                    return (long)((num2 << 0x20) | num);
                }
                int num3 = (((numRef[0] << 0x18) | (numRef[1] << 0x10)) | (numRef[2] << 8)) | numRef[3];
                int num4 = (((numRef[4] << 0x18) | (numRef[5] << 0x10)) | (numRef[6] << 8)) | numRef[7];
                return (long)((num3 << 0x20) | num4);
            }
        }

        public static sbyte ReadSByte(byte[] buffer)
        {
            return (sbyte)buffer[0];
        }

        public static sbyte ReadSByte(byte[] buffer, int offset)
        {
            return (sbyte)buffer[offset + 0];
        }

        [SecuritySafeCritical]
        public static unsafe float ReadSingle(byte[] buffer)
        {
            uint num = (uint)(((buffer[0] | (buffer[1] << 8)) | (buffer[2] << 0x10)) | (buffer[3] << 0x18));
            return *(((float*)&num));
        }

        [SecuritySafeCritical]
        public static unsafe float ReadSingle(byte[] buffer, int offset)
        {
            uint num = (uint)(((buffer[offset + 0] | (buffer[offset + 1] << 8)) | (buffer[offset + 2] << 0x10)) | (buffer[offset + 3] << 0x18));
            return *(((float*)&num));
        }

        [SecuritySafeCritical]
        public static ushort ReadUInt16(byte[] buffer)
        {
            return (ushort)ReadInt16(buffer);
        }
        public static ushort ReadUInt16(byte[] buffer, int offset)
        {
            return (ushort)ReadInt16(buffer, offset);
        }
        public static uint ReadUInt32(byte[] buffer)
        {
            return (uint)ReadInt32(buffer);
        }
        public static uint ReadUInt32(byte[] buffer, int offset)
        {
            return (uint)ReadInt32(buffer, offset);
        }
        public static ulong ReadUInt64(byte[] buffer)
        {
            return (ulong)ReadInt64(buffer);
        }
        public static ulong ReadUInt64(byte[] buffer, int offset)
        {
            return (ulong)ReadInt64(buffer, offset);
        }

        public static TimeSpan ReadTimeSpan(byte[] buffer)
        {
            return new TimeSpan(ReadInt64(buffer));
        }

        [SecuritySafeCritical]
        public static char[] ReadChars(byte[] buffer)
        {
            return Encoding.UTF8.GetString(buffer).ToCharArray();
        }

        [SecuritySafeCritical]
        public static char[] ReadChars(byte[] buffer, int offset, int length)
        {
            return Encoding.UTF8.GetString(buffer, offset, length).ToCharArray();
        }

        public static char ReadChar(byte[] buffer)
        {
            int num = ReadInt16(buffer);
            if (num == -1)
            {
                IoErrors.EndOfFile();
            }
            return (char)num;
        }

        public static char ReadChar(byte[] buffer, int offset)
        {
            int num = ReadInt16(buffer, offset);
            if (num == -1)
            {
                IoErrors.EndOfFile();
            }
            return (char)num;
        }

        #endregion

        #region Guid converter

        public static byte[] GetBytes(Guid val)
        {
            return val.ToByteArray();
        }

        public static Guid ReadGuid(byte[] buffer)
        {
            return new Guid(buffer);
        }

        public static Guid ReadGuid(byte[] buffer, int offset)
        {
            return new Guid(Copy(buffer, offset, 16));
        }

        #endregion

        #region static method DateTime converter

        /// <summary>
        /// Convert DateTime value to byte[8].
        /// </summary>
        /// <param name="val">DateTime value.</param>
        /// <returns></returns>
        public static byte[] GetBytes(DateTime val)
        {
            return GetBytes(val.Ticks);
        }

        /// <summary>
        /// Converts 8 bytes to DateTime value. Offset byte is included.
        /// </summary>
        /// <param name="buffer">Data array.</param>
        /// <returns></returns>
        public static DateTime ReadDateTime(byte[] buffer)
        {
            return DateTime.FromBinary(ReadInt64(buffer));
        }

        /// <summary>
        /// Converts 8 bytes to DateTime value. Offset byte is included.
        /// </summary>
        /// <param name="buffer">Data array.</param>
        /// <param name="offset">Offset where 8 bytes long value starts. Offset byte is included.</param>
        /// <returns></returns>
        public static DateTime ReadDateTime(byte[] buffer, int offset)
        {
            return DateTime.FromBinary(ReadInt64(buffer, offset));
        }

        #endregion


        public static void CopyTo(byte[] buffer, int offset, UInt64 value)
        {
            byte[] rv = GetBytes(value);
            System.Buffer.BlockCopy(buffer, offset, rv, 0, rv.Length);
        }

        public static byte[] Copy(byte[] buffer, int offset, int length)
        {
            byte[] rv = new byte[length];
            System.Buffer.BlockCopy(buffer, offset, rv, 0, length);
            return rv;
        }


        public static byte[] GetBytes(string s)
        {
            return System.Text.Encoding.UTF8.GetBytes(s);
        }
        public static string ReadString(byte[] buffer, int offset, int length)
        {
            return System.Text.Encoding.UTF8.GetString(buffer, offset, length);
        }

        public static byte[] ReadBytesWithLeadCount(byte[] buffer, int offset)
        {
            int length = ReadInt32(buffer, offset);
            byte[] l = GetBytes(length);
            byte[] array = new byte[length];
            System.Buffer.BlockCopy(l, 0, array, 0, l.Length);
            System.Buffer.BlockCopy(buffer, offset, array, 0, length);

            return array;
        }

        public static string ReadStringWithLeadCount(byte[] buffer, int offset)
        {
            int length = ReadInt32(buffer, offset);
            return System.Text.Encoding.UTF8.GetString(buffer, offset + 4, length);
        }

        public static byte[] NullBytes
        {
            get { return new byte[] { (byte)'\0' }; }
        }
        public static DateTime NullDateTime
        {
            get { return new DateTime(1900, 1, 1); }
        }
        public static byte[] CrLf
        {
            get { return GetBytes("\r\n"); }
        }

        internal static SerialBaseType GetSerialType(Type type)
        {
            if (type == null)
            {
                return SerialBaseType.nullType;
            }
            
            switch (type.Name.ToLower())
            {
                case "int":
                case "int32":
                    return SerialBaseType.int32Type;
                case "int16":
                    return SerialBaseType.int16Type;
                 case "uint16":
                    return SerialBaseType.uint16Type;
                case "uint32":
                    return SerialBaseType.uint32Type;
                case "long":
                case "int64":
                    return SerialBaseType.int64Type;
                case "ulong":
                case "uint64":
                    return SerialBaseType.uint64Type;
                case "byte":
                    return SerialBaseType.byteType;
                case "bool":
                case "boolean":
                    return SerialBaseType.boolType;
                case "float":
                    return SerialBaseType.singleType;
                case "double":
                    return SerialBaseType.doubleType;
                case "decimal":
                    return SerialBaseType.decimalType;
                case "datetime":
                    return SerialBaseType.dateTimeType;
                case "timespan":
                    return SerialBaseType.timeSpanType;
                case "guid":
                    return SerialBaseType.guidType;
                case "string":
                    return SerialBaseType.stringType;
                case "byte[]":
                    return SerialBaseType.byteArrayType;
                case "char[]":
                    return SerialBaseType.charArrayType;
                case "char":
                    return SerialBaseType.charType;
                default:
                    return SerialBaseType.otherType;
            }
        }

        internal static byte[] GetBytes(object o, out SerialBaseType st)
        {
            if (o == null)
            {
                st = SerialBaseType.nullType;
                return NullBytes;
            }
            st = GetSerialType(o.GetType());
            
            switch (st)
            {
                case SerialBaseType.int32Type:
                    return GetBytes((int)o);
                case SerialBaseType.int16Type:
                    return GetBytes((Int16)o);
                case SerialBaseType.uint16Type:
                    return GetBytes((UInt16)o);
                case SerialBaseType.uint32Type:
                    return GetBytes((UInt32)o);
                case SerialBaseType.int64Type:
                    return GetBytes((Int64)o);
                case SerialBaseType.uint64Type:
                    return GetBytes((UInt64)o);
                case SerialBaseType.byteType:
                    return GetBytes((byte)o);
                case SerialBaseType.boolType:
                    return GetBytes((bool)o);
                case SerialBaseType.singleType:
                    return GetBytes((float)o);
                case SerialBaseType.doubleType:
                    return GetBytes((double)o);
                case SerialBaseType.decimalType:
                    return GetBytes((decimal)o);
                case SerialBaseType.dateTimeType:
                    return GetBytes((DateTime)o);
                case SerialBaseType.timeSpanType:
                    return GetBytes((TimeSpan)o);
                case SerialBaseType.guidType:
                    return GetBytes((Guid)o);
                case SerialBaseType.stringType:
                    return GetBytes(o.ToString());
                case SerialBaseType.byteArrayType:
                    return GetBytes((byte[])o);
                case SerialBaseType.charArrayType:
                    return GetBytes((char[])o);
                case SerialBaseType.charType:
                    return GetBytes((char)o);
                default:
                    st = SerialBaseType.otherType;
                    return BinarySerializer.SerializeToBytes(o);
            }
        }

        internal static object ReadBytes(byte[] b, SerialBaseType st)
        {
            if (b == null)
            {
                return NullValue(st);
            }
            switch (st)
            {
                case SerialBaseType.int32Type:
                    return ReadInt32(b);
                case SerialBaseType.int16Type:
                    return ReadInt16(b);
                case SerialBaseType.uint16Type:
                    return ReadUInt16(b);
                case SerialBaseType.uint32Type:
                    return ReadUInt32(b);
                case SerialBaseType.int64Type:
                    return ReadUInt64(b);
                case SerialBaseType.uint64Type:
                    return ReadUInt64(b);
                case SerialBaseType.byteType:
                    return b.Length == 0 ? 0 : b[0];
                case SerialBaseType.boolType:
                    return Convert.ToBoolean(b);
                case SerialBaseType.singleType:
                    return ReadSingle(b);
                case SerialBaseType.doubleType:
                    return ReadDouble(b);
                case SerialBaseType.decimalType:
                    return ReadDecimal(b);
                case SerialBaseType.dateTimeType:
                    return ReadDateTime(b);
                case SerialBaseType.timeSpanType:
                    return ReadTimeSpan(b);
                case SerialBaseType.guidType:
                    return ReadGuid(b);
                case SerialBaseType.stringType:
                    return ReadString(b, 0, b.Length);
                case SerialBaseType.byteArrayType:
                    return b;
                case SerialBaseType.charArrayType:
                    return ReadChars(b, 0, b.Length);
                case SerialBaseType.charType:
                    return ReadChar(b, 0);
                default:
                    return BinarySerializer.Deserialize(b);
            }
        }

        internal static object NullValue(SerialBaseType st)
        {
          
            switch (st)
            {
                case SerialBaseType.int32Type:
                case SerialBaseType.int16Type:
                case SerialBaseType.uint16Type:
                case SerialBaseType.uint32Type:
                case SerialBaseType.int64Type:
                case SerialBaseType.uint64Type:
                case SerialBaseType.byteType:
                    return 0;
                case SerialBaseType.boolType:
                    return false;
                case SerialBaseType.singleType:
                    return (float)0;
                case SerialBaseType.doubleType:
                    return (double)0;
                case SerialBaseType.decimalType:
                    return (decimal)0;
                case SerialBaseType.dateTimeType:
                    return NullDateTime;
                case SerialBaseType.timeSpanType:
                    return TimeSpan.Zero;
                case SerialBaseType.guidType:
                    return Guid.Empty;
                case SerialBaseType.stringType:
                    return (string)"";
                case SerialBaseType.byteArrayType:
                    return NullBytes;
                case SerialBaseType.charArrayType:
                    return (char[])new char[] { '\0' };
                case SerialBaseType.charType:
                    return (char)'\0';
                default:
                    //st = SerialBaseType.otherType;
                    return null;
            }
        }

        public static byte[] GetBytes(object o)
        {
            if (o == null)
                return null;
            Type type = o.GetType();
            switch (type.Name.ToLower())
            {
                case "int":
                case "int32":
                    return GetBytes((int)o);
                case "int16":
                    return GetBytes((Int16)o);
                case "uint16":
                    return GetBytes((UInt16)o);
                case "uint32":
                    return GetBytes((UInt32)o);
                case "long":
                case "int64":
                    return GetBytes((Int64)o);
                case "ulong":
                case "uint64":
                    return GetBytes((UInt64)o);
                case "byte":
                    return GetBytes((byte)o);
                case "bool":
                case "boolean":
                    return GetBytes((bool)o);
                case "float":
                    return GetBytes((float)o);
                case "double":
                    return GetBytes((double)o);
                case "decimal":
                    return GetBytes((decimal)o);
                case "datetime":
                    return GetBytes((DateTime)o);
                case "timespan":
                    return GetBytes((TimeSpan)o);
                case "guid":
                    return GetBytes((Guid)o);
                case "byte[]":
                    return (byte[])o;
                case "char[]":
                    return GetBytes((char[])o);
                case "char":
                    return GetBytes((char)o);
                default:
                    return GetBytes(o.ToString());
            }
        }

        public static byte[] GetBytes(object[] values)
        {
            using (MemoryStream msRecord = new MemoryStream())
            {
                // Write values
                for (int i = 0; i < values.Length; i++)
                {
                    byte[] b = GetBytes(values[i]);
                    msRecord.Write(b, 0, b.Length);
                }
                return msRecord.ToArray();
            }
        }

        internal static byte[] Concat(List<byte[]> rowByteValues)
        {
            using (MemoryStream msRecord = new MemoryStream())
            {
                // Write values
                for (int i = 0; i < rowByteValues.Count; i++)
                {
                    byte[] b = (byte[])rowByteValues[i];
                    msRecord.Write(b, 0, b.Length);
                }
                return msRecord.ToArray();
            }
        }

    }

}

