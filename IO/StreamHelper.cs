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
using System.IO;
using System.Runtime.Serialization;

#pragma warning disable CS1591

namespace Nistec.IO
{
	/// <summary>
	/// A class that helps save and load with stream persistence
	/// </summary>
	public class StreamHelper
	{
		/// <summary>
		/// Constructor
		/// </summary>
		protected StreamHelper()
		{
		}

		#region Write Function

		public static void Write(Stream p_Stream, string p_Value)
		{
			if (p_Value!=null)
			{
				Write(p_Stream, p_Value.Length);
				for (int i = 0; i < p_Value.Length; i++)
					Write(p_Stream, p_Value[i]);
			}
			else
				Write(p_Stream, (int)0);
		}

		public static void Write(Stream p_Stream, String p_Value, System.Text.Encoding encoding)
		{
			byte[] bytes = encoding.GetBytes(p_Value);
			Write(p_Stream, bytes );
		}
		public static void Write(Stream p_Stream, Byte p_Value)
		{
			p_Stream.WriteByte(p_Value);
		}
		public static void Write(Stream p_Stream, Guid p_Value)
		{
			byte[] vBits = p_Value.ToByteArray();
			Write(p_Stream, vBits);
		}
		public static void Write(Stream p_Stream, Decimal p_Value)
		{
			int[] vBits = Decimal.GetBits(p_Value);
			Write(p_Stream, vBits[0]);
			Write(p_Stream, vBits[1]);
			Write(p_Stream, vBits[2]);
			Write(p_Stream, vBits[3]);
		}
		public static void Write(Stream p_Stream, DateTime p_Value)
		{
			Write(p_Stream, p_Value.ToOADate());
		}
		public static void Write(Stream p_Stream, Int16 p_Value)
		{
			WriteBytes(p_Stream, BitConverter.GetBytes(p_Value));
		}
		public static void Write(Stream p_Stream, Int32 p_Value)
		{
			WriteBytes(p_Stream, BitConverter.GetBytes(p_Value));
		}
		public static void Write(Stream p_Stream, Int64 p_Value)
		{
			WriteBytes(p_Stream, BitConverter.GetBytes(p_Value));
		}
		public static void Write(Stream p_Stream, Single p_Value)
		{
			WriteBytes(p_Stream, BitConverter.GetBytes(p_Value));
		}
		public static void Write(Stream p_Stream, Double p_Value)
		{
			WriteBytes(p_Stream, BitConverter.GetBytes(p_Value));
		}
		public static void Write(Stream p_Stream, Char p_Value)
		{
			WriteBytes(p_Stream, BitConverter.GetBytes(p_Value));
		}
		public static void Write(Stream p_Stream, Boolean p_Value)
		{
			WriteBytes(p_Stream, BitConverter.GetBytes(p_Value));
		}
		public static void Write(Stream p_Stream, UInt16 p_Value)
		{
			WriteBytes(p_Stream, BitConverter.GetBytes(p_Value));
		}
		public static void Write(Stream p_Stream, UInt32 p_Value)
		{
			WriteBytes(p_Stream, BitConverter.GetBytes(p_Value));
		}
		public static void Write(Stream p_Stream, UInt64 p_Value)
		{
			WriteBytes(p_Stream, BitConverter.GetBytes(p_Value));
		}

		public static void Write(Stream p_Stream, Byte[] p_Bytes)
		{
			Write(p_Stream, p_Bytes.Length);
			WriteBytes(p_Stream, p_Bytes);
		}


		public static void Write(Stream stream, Type valueType, object val)
		{
			if (valueType == typeof(String))
				Write(stream, (String)val, System.Text.Encoding.UTF8);
			else if (valueType == typeof(Int16))
				Write(stream, (Int16)val);
			else if (valueType == typeof(Int32))
				Write(stream, (Int32)val);
			else if (valueType == typeof(Int64))
				Write(stream, (Int64)val);
			else if (valueType == typeof(Single))
				Write(stream, (Single)val);
			else if (valueType == typeof(Double))
				Write(stream, (Double)val);
			else if (valueType == typeof(Boolean))
				Write(stream, (Boolean)val);
			else if (valueType == typeof(Decimal))
				Write(stream, (Decimal)val);
			else if (valueType == typeof(Char))
				Write(stream, (Char)val);
			else if (valueType == typeof(Byte))
				Write(stream, (Byte)val);
			else if (valueType == typeof(Byte[]))
				Write(stream, (Byte[])val);
			else if (valueType == typeof(DateTime))
				Write(stream, (DateTime)val);
			else if (valueType == typeof(Guid))
				Write(stream, (Guid)val);
			else
			throw new NotSupportedException(valueType.ToString());

		}
		
		public static void WriteBytes(Stream p_Stream, byte[] p_Bytes)
		{
			p_Stream.Write(p_Bytes, 0, p_Bytes.Length);
		}
		#endregion

		#region Read Function
		public static void Read(Stream p_Stream, out double p_Value)
		{
			byte[] l_tmp = BitConverter.GetBytes((double)0.0);
			if (p_Stream.Read(l_tmp,0,l_tmp.Length) != l_tmp.Length)
				throw new InvalidDataException();
			p_Value = BitConverter.ToDouble(l_tmp,0);
		}
		public static void Read(Stream p_Stream, out int p_Value)
		{
			byte[] l_tmp = BitConverter.GetBytes((int)0);
			if (p_Stream.Read(l_tmp,0,l_tmp.Length) != l_tmp.Length)
				throw new InvalidDataException();
			p_Value = BitConverter.ToInt32(l_tmp,0);
		}
		public static void Read(Stream p_Stream, out float p_Value)
		{
			byte[] l_tmp = BitConverter.GetBytes((float)0.0f);
			if (p_Stream.Read(l_tmp,0,l_tmp.Length) != l_tmp.Length)
				throw new InvalidDataException();
			p_Value = BitConverter.ToSingle(l_tmp,0);
		}
		public static void Read(Stream p_Stream, out long p_Value)
		{
			byte[] l_tmp = BitConverter.GetBytes((long)0);
			if (p_Stream.Read(l_tmp,0,l_tmp.Length) != l_tmp.Length)
				throw new InvalidDataException();
			p_Value = BitConverter.ToInt64(l_tmp,0);
		}

		
		public static void Read(Stream p_Stream, out uint p_Value)
		{
			byte[] l_tmp = BitConverter.GetBytes((uint)0);
			if (p_Stream.Read(l_tmp,0,l_tmp.Length) != l_tmp.Length)
				throw new InvalidDataException();
			p_Value = BitConverter.ToUInt32(l_tmp,0);
		}
		public static void Read(Stream p_Stream, out ulong p_Value)
		{
			byte[] l_tmp = BitConverter.GetBytes((ulong)0);
			if (p_Stream.Read(l_tmp,0,l_tmp.Length) != l_tmp.Length)
				throw new InvalidDataException();
			p_Value = BitConverter.ToUInt64(l_tmp,0);
		}
		public static void Read(Stream p_Stream, out short p_Value)
		{
			byte[] l_tmp = BitConverter.GetBytes((short)0);
			if (p_Stream.Read(l_tmp,0,l_tmp.Length) != l_tmp.Length)
				throw new InvalidDataException();
			p_Value = BitConverter.ToInt16(l_tmp,0);
		}
		public static void Read(Stream p_Stream, out ushort p_Value)
		{
			byte[] l_tmp = BitConverter.GetBytes((ushort)0);
			if (p_Stream.Read(l_tmp,0,l_tmp.Length) != l_tmp.Length)
				throw new InvalidDataException();
			p_Value = BitConverter.ToUInt16(l_tmp,0);
		}
		public static void Read(Stream p_Stream, out char p_Value)
		{
			byte[] l_tmp = BitConverter.GetBytes((char)0);
			if (p_Stream.Read(l_tmp,0,l_tmp.Length) != l_tmp.Length)
				throw new InvalidDataException();
			p_Value = BitConverter.ToChar(l_tmp,0);
		}
		public static void Read(Stream p_Stream, out bool p_Value)
		{
			byte[] l_tmp = BitConverter.GetBytes((bool)false);
			if (p_Stream.Read(l_tmp,0,l_tmp.Length) != l_tmp.Length)
				throw new InvalidDataException();
			p_Value = BitConverter.ToBoolean(l_tmp,0);
		}
		public static void Read(Stream p_Stream, out string p_Value)
		{
			int l_Length;
			Read(p_Stream, out l_Length);
			if (l_Length > 0)
			{
				char[] l_tmp = new char[l_Length];
				for (int i = 0; i < l_Length; i++)
				{
					char c;
					Read(p_Stream, out c);
					l_tmp[i] = c;
				}
				p_Value = new string(l_tmp);
			}
			else
				p_Value = null;
		}
		public static void Read(Stream p_Stream, byte[] p_Value)
		{
			if (p_Stream.Read(p_Value,0,p_Value.Length) != p_Value.Length)
				throw new InvalidDataException();
		}
		//		public static void Read(Stream p_Stream, out decimal p_Value)
		//		{
		//			byte[] l_tmp = BitConverter.GetBytes((decimal)0.0);
		//			if (p_Stream.Read(l_tmp,0,l_tmp.Length) != l_tmp.Length)
		//				throw new InvalidDataException();
		//			p_Value = BitConverter.Todecimal(l_tmp,0);
		//		}
		#endregion

		#region Read Function
		public static Guid ReadGuid(Stream stream)
		{
			Guid val;
			byte[] bytesArray = ReadByteArray(stream);
			val = new Guid(bytesArray);
			return val;
		}
		public static Decimal ReadDecimal(Stream stream)
		{
			Decimal val;
			Int32 v1 = ReadInt32(stream);
			Int32 v2 = ReadInt32(stream);
			Int32 v3 = ReadInt32(stream);
			Int32 v4 = ReadInt32(stream);
			val = new decimal(new int[]{v1, v2, v3, v4});
			return val;
		}
		public static DateTime ReadDateTime(Stream p_Stream)
		{
			DateTime val;
			double dbl = ReadDouble(p_Stream);
			val = DateTime.FromOADate(dbl);
			return val;
		}
		public static Single ReadSingle(Stream p_Stream)
		{
			Single val;
			byte[] l_tmp = BitConverter.GetBytes((Single)0.0f);
			ReadBytes(p_Stream, l_tmp);
			val = BitConverter.ToSingle(l_tmp, 0);
			return val;
		}
		public static Double ReadDouble(Stream p_Stream)
		{
			Double val;
			byte[] l_tmp = BitConverter.GetBytes((Double)0.0f);
			ReadBytes(p_Stream, l_tmp);
			val = BitConverter.ToDouble(l_tmp, 0);
			return val;
		}
		public static Int16 ReadInt16(Stream p_Stream)
		{
			Int16 val;
			byte[] l_tmp = BitConverter.GetBytes((Int16)0);
			ReadBytes(p_Stream, l_tmp);
			val = BitConverter.ToInt16(l_tmp,0);
			return val;
		}
		public static Int32 ReadInt32(Stream p_Stream)
		{
			Int32 val;
			byte[] l_tmp = BitConverter.GetBytes((Int32)0);
			ReadBytes(p_Stream, l_tmp);
			val = BitConverter.ToInt32(l_tmp,0);
			return val;
		}
		public static Int64 ReadInt64(Stream p_Stream)
		{
			Int64 val;
			byte[] l_tmp = BitConverter.GetBytes((Int64)0);
			ReadBytes(p_Stream, l_tmp);
			val = BitConverter.ToInt64(l_tmp,0);
			return val;
		}

		
		public static UInt16 ReadUInt16(Stream p_Stream)
		{
			System.UInt16 val;
			byte[] l_tmp = BitConverter.GetBytes((UInt16)0);
			ReadBytes(p_Stream, l_tmp);
			val = BitConverter.ToUInt16(l_tmp,0);
			return val;
		}
		
		public static UInt32 ReadUInt32(Stream p_Stream)
		{
			System.UInt32 val;
			byte[] l_tmp = BitConverter.GetBytes((UInt32)0);
			ReadBytes(p_Stream, l_tmp);
			val = BitConverter.ToUInt32(l_tmp,0);
			return val;
		}
		
		public static UInt64 ReadUInt64(Stream p_Stream)
		{
			System.UInt64 val;
			byte[] l_tmp = BitConverter.GetBytes((UInt64)0);
			ReadBytes(p_Stream, l_tmp);
			val = BitConverter.ToUInt64(l_tmp,0);
			return val;
		}
		public static Byte ReadByte(Stream p_Stream)
		{
			int byteVal = p_Stream.ReadByte();
			if (byteVal == -1)
				throw new InvalidDataException();

			return (Byte)byteVal;
		}
		public static char ReadChar(Stream p_Stream)
		{
			char val;
			byte[] l_tmp = BitConverter.GetBytes((char)0);
			ReadBytes(p_Stream, l_tmp);
			val = BitConverter.ToChar(l_tmp,0);
			return val;
		}
		public static bool ReadBoolean(Stream p_Stream)
		{
			bool val;
			byte[] l_tmp = BitConverter.GetBytes((bool)false);
			ReadBytes(p_Stream, l_tmp);
			val = BitConverter.ToBoolean(l_tmp, 0);
			return val;
		}
		public static string ReadString(Stream p_Stream, System.Text.Encoding encoding)
		{
			string val;
			byte[] bytesString = ReadByteArray(p_Stream);
			val = encoding.GetString(bytesString);
			return val;
		}
		public static byte[] ReadByteArray(Stream p_Stream)
		{
			byte[] val;
			int len = ReadInt32(p_Stream);
			val = new byte[len];
			ReadBytes(p_Stream, val);
			return val;
		}

		public static object Read(Stream stream, Type valueType)
		{
			if (valueType == typeof(String))
				return ReadString(stream, System.Text.Encoding.UTF8);
			else if (valueType == typeof(Char))
				return ReadChar(stream);
			else if (valueType == typeof(Boolean))
				return ReadBoolean(stream);
			else if (valueType == typeof(Decimal))
				return ReadDecimal(stream);
			else if (valueType == typeof(Int16))
				return ReadInt16(stream);
			else if (valueType == typeof(Int32))
				return ReadInt32(stream);
			else if (valueType == typeof(Int64))
				return ReadInt64(stream);
			else if (valueType == typeof(Single))
				return ReadSingle(stream);
			else if (valueType == typeof(Double))
				return ReadDouble(stream);
			else if (valueType == typeof(Byte))
				return ReadByte(stream);
			else if (valueType == typeof(Byte[]))
				return ReadByteArray(stream);
			else if (valueType == typeof(DateTime))
				return ReadDateTime(stream);
			else if (valueType == typeof(Guid))
				return ReadGuid(stream);
			else
                throw new NotSupportedException(valueType.ToString());
		}

		public static void ReadBytes(Stream p_Stream, byte[] p_Value)
		{
			if (p_Stream.Read(p_Value,0,p_Value.Length) != p_Value.Length)
				throw new InvalidDataException();
		}
		#endregion

        public static byte[] StreamToBytes(Stream stream)
        {
            stream.Position = 0;
            if (stream is MemoryStream)
            {
                return ((MemoryStream)stream).ToArray();
            }

            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }


	}

}
