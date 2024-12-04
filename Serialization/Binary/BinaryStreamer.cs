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
using System.Collections;
using System.Runtime.Serialization;
using System.Data;
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using Nistec.IO;
using Nistec.Generic;
using System.Collections.Specialized;
using Nistec.Runtime;
#pragma warning disable  CS1591

namespace Nistec.Serialization
{

    /// <summary>
    /// Represent a binary streamer such as binary write and reader.
    /// </summary>
    public class BinaryStreamer : ISerializerContext, IBinaryStreamer//, IDisposable
    {

        #region members

        internal bool m_UseStreamerForUnknownType=true;

        BinaryFormatter _InternalFormatter;
        
        BinaryFormatter InternalFormatter
        {
            get
            {
                if (_InternalFormatter == null)
                {
                    _InternalFormatter = new BinaryFormatter();
                }
                return _InternalFormatter;
            }
        }

        //============== reader ====================

        private bool m_2BytesPerChar;
        private byte[] m_buffer;
        private char[] m_charBuffer;
        private byte[] m_charBytes;
        private Decoder m_decoder;
        private bool m_isNetStream;
        private int m_maxCharsSize;
        private char[] m_singleChar;
        private Stream m_stream;
        private const int MaxCharBytesSize = 0x80;

        //private StreamWriter m_streamWriter;
        //private StreamReader m_streamReader;

        //============== writer ====================

        private Encoder m_encoder;
        private Encoding m_encoding;
        private byte[] _largeByteBuffer;
        private int _maxChars;
        private const int LargeByteBufferSize = 0x100;
        private bool m_isOwnerStream;
        private int _bytesWriten;

        #endregion

        #region properties

        public virtual Encoding Encoding
        {
            get
            {
                return this.m_encoding;
            }
        }

        public virtual Stream BaseStream
        {
            get
            {
                this.Flush();
                return this.m_stream;
            }
        }
        public bool IsOwnerStream
        {
            get
            {
                return this.m_isOwnerStream;
            }
        }
        

        internal virtual Stream ReaderStream
        {
            get
            {
                return this.m_stream;
            }
        }

        public T ConvertType<T>(Type type) where T : IConvertible
        {
            return (T)Convert.ChangeType(type, typeof(T));
        }

        public int BytesWriten
        {
            get
            {
                return this._bytesWriten;
            }
        }

        #endregion

        #region ctor


        public BinaryStreamer(Stream stream, bool isOwnerStream = false)
            : this(stream, new UTF8Encoding(), isOwnerStream)
        {
        }

       public BinaryStreamer(Stream stream, Encoding encoding, bool isOwnerStream=false)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }

            if(stream is NetStream)
            {
                stream.Position = 0;
            }
            else if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            //TODO:CHECK THIS
            //if (!stream.CanWrite)
            //{
            //    throw new ArgumentException("Argument Stream Not Writable");
            //}
            //if (!stream.CanRead)
            //{
            //    throw new ArgumentException("Argument Stream Not Readable");
            //}
            //m_UseStreamerForUnknownType = true;
            //==============================
            this.m_isOwnerStream = isOwnerStream;
            this.m_encoding = encoding;
            this.m_stream = stream;

            //m_streamWriter = new StreamWriter(m_stream);
            //m_streamReader = new StreamReader(m_stream);
            //m_streamWriter.AutoFlush = true;

            this.m_decoder = NetDecoder.GetDecoder(encoding);// encoding.GetDecoder();
            this.m_maxCharsSize = encoding.GetMaxCharCount(0x80);
            int maxByteCount = encoding.GetMaxByteCount(1);
            if (maxByteCount < 0x10)
            {
                maxByteCount = 0x10;
            }
            this.m_buffer = new byte[maxByteCount];
            this.m_charBuffer = null;
            this.m_charBytes = null;
            this.m_2BytesPerChar = encoding is UnicodeEncoding;
            this.m_isNetStream = this.m_stream.GetType() == typeof(NetStream);
            //write
            this.m_encoder = NetEncoder.GetEncoder(encoding);
        }

        public virtual void Flush()
        {
            this.m_stream.Flush();
        }

        private void ReStreamer(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            this.m_stream = stream;
            //m_streamWriter = new StreamWriter(m_stream);
            //m_streamReader = new StreamReader(m_stream);

            int maxByteCount = m_encoding.GetMaxByteCount(1);
            if (maxByteCount < 0x10)
            {
                maxByteCount = 0x10;
            }
            this.m_buffer = new byte[maxByteCount];
            this.m_charBuffer = null;
            this.m_charBytes = null;

            if (stream is NetStream)
            {
                this.m_isNetStream = true;
                stream.Position = 0;
            }
            else
                this.m_isNetStream = false;
        }

        #endregion

        #region Dispose

        public virtual void Close()
        {
            this.Dispose(true);
        }

        [SecuritySafeCritical]
        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (IsOwnerStream==false)
                {
                    Stream stream = this.m_stream;
                    this.m_stream = null;
                    if (stream != null)
                    {
                        stream.Close();
                    }
                }
            }
            
            this.m_buffer = null;
            this.m_decoder = null;
            this.m_charBytes = null;
            this.m_singleChar = null;
            this.m_charBuffer = null;
            this.m_encoder = null;
            this.m_encoding = null;
           
        }
        #endregion

        #region SerialType

        public void WriteContextType(SerialContextType contextType)
        {
            if (contextType == SerialContextType.None)
                return;
            Write((byte)contextType);
        }

        public SerialContextType ReadContextType()
        {
            return (SerialContextType)ReadByte();
        }

        public byte ReadContextType(SerialContextType contextType)
        {
            if (contextType == SerialContextType.None)
                return (byte)0;

            return ReadByte();
        }
 
        #endregion

        #region Write any

        /// <summary> Writes an arbitrary object to the buffer.  Useful where we have something of type "object"
        /// and don't know how to treat it.  This works out the best method to use to write to the buffer. </summary>
        public void Encode(object value)
        {
            //m_UseStreamerForUnknownType = true;
            WriteAny(value);
        }

        /// <summary> Writes an arbitrary object to the buffer.  Useful where we have something of type "object"
        /// and don't know how to treat it.  This works out the best method to use to write to the buffer. </summary>
        internal void WriteAny(object value, Type baseType = null)
        {
            if (value == null || value == DBNull.Value)
            {
                Write((byte)SerialType.nullType);
            }
            else
            {
                Type type = baseType == null ? value.GetType() : baseType;

                SerialType t = SerializeTools.GetSerialType(type, baseType);//, m_UseStreamerForUnknownType);

                WriteItem(t, value, true);
            }
        }

        void WriteItem(SerialType t, object value, bool writeTypeId)
        {
            if (value == null || value == DBNull.Value)
            {
                Write((byte)SerialType.nullType);
            }
            else
            {
                if (writeTypeId)
                {
                    Write((byte)t);
                }

                switch (t)
                {
                    case SerialType.boolType:
                        Write((bool)value);
                        break;
                    case SerialType.byteType:
                        Write((byte)value);
                        break;
                    case SerialType.uint16Type:
                        Write((ushort)value);
                        break;
                    case SerialType.uint32Type:
                        Write((uint)value);
                        break;
                    case SerialType.uint64Type:
                        Write((ulong)value);
                        break;
                    case SerialType.sbyteType:
                        Write((sbyte)value);
                        break;
                    case SerialType.int16Type:
                        Write((short)value);
                        break;
                    case SerialType.int32Type:
                        Write((int)value);
                        break;
                    case SerialType.int64Type:
                        Write((long)value);
                        break;
                    case SerialType.charType:
                        Write((char)value);
                        break;
                    case SerialType.stringType:
                        Write((string)value);
                        break;
                    case SerialType.singleType:
                        Write((float)value);
                        break;
                    case SerialType.doubleType:
                        Write((double)value);
                        break;
                    case SerialType.decimalType:
                        Write((decimal)value);
                        break;
                    case SerialType.dateTimeType:
                        Write((DateTime)value);
                        break;
                    case SerialType.timeSpanType:
                        Write(((TimeSpan)value).Ticks);
                        break;
                    case SerialType.byteArrayType:
                        WriteByteArray((byte[])value);
                        break;
                    case SerialType.charArrayType:
                        WriteCharArray((char[])value);
                        break;
                    case SerialType.guidType:
                        Write((Guid)value);
                        break;
                    case SerialType.int16ArrayType:
                        WritePrimitiveArray<Int16>((Int16[])value);
                        break;
                    case SerialType.int32ArrayType:
                        WritePrimitiveArray<Int32>((Int32[])value);
                        break;
                    case SerialType.int64ArrayType:
                        WritePrimitiveArray<Int64>((Int64[])value);
                        break;
                    case SerialType.stringArrayType:
                        WriteArray<String>((String[])value);
                        break;
                    case SerialType.objectArrayType:
                        WriteArray<object>((object[])value);
                        break;
                    case SerialType.enumType:
                        WriteEnum(value);
                        break;
                    case SerialType.serialEntityType:
                        WriteSerialEntityInternal(value,true);
                        break;
                    case SerialType.serialContextType:
                        WriteSerialContext(value);
                        break;
                    case SerialType.dictionaryGenericType:
                        WriteGenericDictionary((IDictionary)value);
                        break;
                    case SerialType.dictionaryEntityType:
                        WriteDictionaryAsEntity((Dictionary<string,object>)value,false, true);
                        break;
                    case SerialType.listGenericType:
                        WriteGenericList((IList)value);
                        break;
                    case SerialType.hashtableType:
                        WriteHashtable((IDictionary)value);
                        break;
                    case SerialType.dictionaryAssignType:
                        WriteAssignFromDictionary(value);
                        break;
                    case SerialType.dataTableType:
                        WriteDataTable((DataTable)value);
                        break;
                    case SerialType.dataSetType:
                        WriteDataSet((DataSet)value);
                        break;
                    case SerialType.netStreamType:
                        WriteNetStream((NetStream)value);
                        break;
                    case SerialType.streamType:
                        WriteStream((Stream)value);
                        break;
                    case SerialType.xmlDocumentType:
                        WriteXmlDocument((XmlDocument)value);
                        break;
                    case SerialType.xmlNodeType:
                        WriteXmlNode((XmlNode)value);
                        break;
                    case SerialType.genericKeyValueType:
                        WriteKeyValue<string,object>((List<KeyValuePair<string, object>>)value);// WriteGenericKeyValue((GenericKeyValue)value);
                        break;
                    case SerialType.stringDictionary:
                        WriteStringDictionary((StringDictionary)value);
                        break;
                    case SerialType.nameValueCollection:
                        WriteNameValueCollection((NameValueCollection)value);
                        break;
                    case SerialType.iEntityDictionaryType:
                        WriteSerialEntityInternal(value,true);
                        break;
                     case SerialType.typeType:
                        Write(((Type)value).AssemblyQualifiedName);
                        break;
                    case SerialType.anyClassType:
                        WriteAnyClass(value);
                        break;
                    default:
                        InternalFormatter.Serialize(BaseStream, value);
                        break;

                } // switch

            } // if value==null

        } // Serialize

        internal void WriteItem<T>(object value, Type baseType = null)
        {
            if (value == null || value == DBNull.Value)
            {
                Write((byte)SerialType.nullType);
            }
            else
            {
                Type type = typeof(T);
                SerialType t = SerializeTools.GetSerialType(type, baseType);//, m_UseStreamerForUnknownType);
                WriteItem(t, value, false);
            }

        }

  
        #endregion

        #region Read any
        
        /// <summary> Reads an object which was added to the buffer by WriteObject. </summary>
        public object Decode()
        {
           return ReadAny();
        }
        public T Decode<T>(bool enableExeption)
        {
            return GenericTypes.Cast<T>(ReadAny(), enableExeption);
        }
        internal T ReadAny<T>()
        {
            return GenericTypes.Cast<T>(ReadAny(), true);
        }

        /// <summary> Reads an object which was added to the buffer by WriteObject. </summary>
        internal object ReadAny()
        {
            SerialType t = (SerialType)ReadByte();
            switch (t)
            {
                case SerialType.boolType: return ReadBoolean();
                case SerialType.byteType: return ReadByte();
                case SerialType.uint16Type: return ReadUInt16();
                case SerialType.uint32Type: return ReadUInt32();
                case SerialType.uint64Type: return ReadUInt64();
                case SerialType.sbyteType: return ReadSByte();
                case SerialType.int16Type: return ReadInt16();
                case SerialType.int32Type: return ReadInt32();
                case SerialType.int64Type: return ReadInt64();
                case SerialType.charType: return ReadChar();
                case SerialType.stringType: return ReadString(false);
                case SerialType.singleType: return ReadSingle();
                case SerialType.doubleType: return ReadDouble();
                case SerialType.decimalType: return ReadDecimal();
                case SerialType.dateTimeType: return ReadDateTime();
                case SerialType.timeSpanType: return ReadTimeSpan();
                case SerialType.byteArrayType: return ReadByteArray();
                case SerialType.charArrayType: return ReadCharArray();
                case SerialType.guidType: return ReadGuid();
                case SerialType.enumType: return ReadEnum();
                case SerialType.typeType: return ReadType();
                case SerialType.int16ArrayType: return ReadPrimitiveArray<Int16>(SerialType.int16ArrayType);
                case SerialType.int32ArrayType: return ReadPrimitiveArray<Int32>(SerialType.int32ArrayType);
                case SerialType.int64ArrayType: return ReadPrimitiveArray<Int64>(SerialType.int64ArrayType);
                case SerialType.stringArrayType: return ReadArray<string>();
                case SerialType.objectArrayType: return ReadArray<object>();
                case SerialType.dictionaryEntityType:return ReadEntityAsDictionary(false,true);
                case SerialType.listGenericType: return ReadGenericList();
                case SerialType.hashtableType: return ReadHashtable();
                case SerialType.dictionaryGenericType: return ReadGenericDictionary();
                case SerialType.dataTableType: return ReadDataTable();
                case SerialType.dataSetType: return ReadDataSet();
                case SerialType.netStreamType: return ReadNetStream();
                case SerialType.streamType: return ReadStream();
                case SerialType.xmlDocumentType: return ReadXmlDocument();
                case SerialType.xmlNodeType: return ReadXmlNode();
                case SerialType.anyClassType: return ReadAnyClass();
                case SerialType.serialEntityType: return ReadSerialEntityInternal();
                case SerialType.serialContextType: return ReadSerialContext();
                case SerialType.genericKeyValueType: return ReadKeyValue<string, object>();
                case SerialType.stringDictionary: return ReadStringDictionary();
                case SerialType.nameValueCollection: return ReadNameValueCollection();
                case SerialType.dictionaryAssignType: return ReadAssignFromDictionary();
                case SerialType.iEntityDictionaryType: return ReadGenericEntityAsIDictionary();
                case SerialType.otherType: return InternalFormatter.Deserialize(ReaderStream);

                case SerialType.genericEntityAsIEntityType:
                    {
                        throw new ArgumentException("This type not supported here, try use directly from BinaryStreamer.ReadAnyEntity()<T>()");
                        //return ReadGenericEntityAsEntity();//<string>();
                    }
                case SerialType.genericEntityAsIDictionaryType:
                    {
                        throw new ArgumentException("This type not supported here, try use directly from BinaryStreamer.ReadGenericEntityAsDictionary<T>()");
                        //return ReadGenericEntityAsDictionary();//<string>();
                    }

                default: return null;
            }
        }


        T ReadItem<T>(SerialType t, bool isKnownType)
        {
            if (isKnownType)
                return GenericTypes.Cast<T>(ReadItem(t), true);
            else
                return GenericTypes.Cast<T>(ReadAny(), true);
        }

        object ReadItem(SerialType t, bool isKnownType)
        {
            if (isKnownType)
                return ReadItem(t);
            else
                return ReadAny();
        }

        internal T ReadItem<T>(SerialType t)
        {
            return GenericTypes.Cast<T>(ReadItem(t), true);
        }

        /// <summary> Reads an object which was added to the buffer by WriteObject. </summary>
        internal object ReadItem(SerialType t)
        {
            
            switch (t)
            {
                case SerialType.boolType: return ReadBoolean();
                case SerialType.byteType: return ReadByte();
                case SerialType.uint16Type: return ReadUInt16();
                case SerialType.uint32Type: return ReadUInt32();
                case SerialType.uint64Type: return ReadUInt64();
                case SerialType.sbyteType: return ReadSByte();
                case SerialType.int16Type: return ReadInt16();
                case SerialType.int32Type: return ReadInt32();
                case SerialType.int64Type: return ReadInt64();
                case SerialType.charType: return ReadChar();
                case SerialType.stringType: return ReadString(false);
                case SerialType.singleType: return ReadSingle();
                case SerialType.doubleType: return ReadDouble();
                case SerialType.decimalType: return ReadDecimal();
                case SerialType.dateTimeType: return ReadDateTime();
                case SerialType.byteArrayType: return ReadByteArray();
                case SerialType.charArrayType: return ReadCharArray();
                case SerialType.guidType: return ReadGuid();
                case SerialType.enumType: return ReadEnum();
                case SerialType.typeType: return ReadType();
                case SerialType.int16ArrayType: return ReadPrimitiveArray<Int16>(SerialType.int16ArrayType);
                case SerialType.int32ArrayType: return ReadPrimitiveArray<Int32>(SerialType.int32ArrayType);
                case SerialType.int64ArrayType: return ReadPrimitiveArray<Int64>(SerialType.int64ArrayType);
                case SerialType.stringArrayType: return ReadArray<string>();
                case SerialType.objectArrayType: return ReadArray<object>();
                case SerialType.listGenericType: return ReadGenericList();
                case SerialType.hashtableType: return ReadHashtable();
                case SerialType.dictionaryGenericType: return ReadGenericDictionary();
                case SerialType.dictionaryEntityType:return ReadEntityAsDictionary(false,true);
                case SerialType.dataTableType: return ReadDataTable();
                case SerialType.dataSetType: return ReadDataSet();
                case SerialType.netStreamType: return ReadNetStream();
                case SerialType.streamType: return ReadStream();
                case SerialType.xmlDocumentType: return ReadXmlDocument();
                case SerialType.xmlNodeType: return ReadXmlNode();
                case SerialType.anyClassType: return ReadAnyClass();
                case SerialType.serialEntityType: return ReadSerialEntityInternal();
                case SerialType.serialContextType: return ReadSerialContext();
                case SerialType.genericKeyValueType: return ReadKeyValue<string,object>();
                case SerialType.stringDictionary: return ReadStringDictionary();
                case SerialType.nameValueCollection: return ReadNameValueCollection();
                case SerialType.dictionaryAssignType: return ReadAssignFromDictionary();
                case SerialType.iEntityDictionaryType: return ReadGenericEntityAsIDictionary();
                case SerialType.otherType: return InternalFormatter.Deserialize(ReaderStream);
                default: return null;
            }
        }

        #endregion

        #region Write/Read values

        public void WritePrimitive<T>(T value)
        {
            Type type =typeof(T);
            if (!SerializeTools.IsPrimitive(type))
            {
                throw new NotSupportedException("The value is not a Primitive type");
            }
            SerialType t = SerializeTools.GetSerialType(type, null);//, true);
            WriteItem(t, value, true);
        }

        public T ReadPrimitive<T>()
        {
            Type type = typeof(T);
            if (!SerializeTools.IsPrimitive(type))
            {
                throw new NotSupportedException("The value is not a Primitive type");
            }
            return GenericTypes.Cast<T>(ReadAny(), true);
        }


        /// <summary> Writes an arbitrary object to the buffer.  Useful where we have something of type "object"
        /// and don't know how to treat it.  This works out the best method to use to write to the buffer. </summary>
        public void WriteValue(object value, Type baseType = null)
        {
            WriteAny(value, baseType);
        }

        public void WriteCount(int value)
        {
            Write((Int32)value);
        }

        public int ReadCount()
        {
            return ReadInt32();
        }

        /// <summary> Writes an arbitrary object to the buffer.  Useful where we have something of type "object"
        /// and don't know how to treat it.  This works out the best method to use to write to the buffer. </summary>
        public object ReadValue()
        {
            return ReadAny();
        }

        List<int> _Mapper;
        public List<int> GetMapper()
        {
            return _Mapper;
        }
        public void MapperBegin()
        {
            _Mapper = new List<int>();
        }

        public T ReadMapValue<T>()
        {
            _Mapper.Add((int)m_stream.Position);
            return GenericTypes.Cast<T>(ReadAny(), true);
        }
        public string ReadMapString()
        {
            _Mapper.Add((int)m_stream.Position);
            return ReadString();
        }
        public T ReadValue<T>()
        {
            return GenericTypes.Cast<T>(ReadAny(), true);
        }

        /// <summary> Writes a string to the buffer.  Overrides the base implementation so it can cope with nulls </summary>
        public void WriteString(string str)
        {
            if (str == null)
            {
                Write((byte)SerialType.nullType);
            }
            else
            {
                Write((byte)SerialType.stringType);
                this.Write(str);
            }
        }

        /// <summary> Reads a string from the buffer.  Overrides the base implementation so it can cope with nulls. </summary>
        public string ReadString()
        {
            SerialType t = (SerialType)ReadByte();
            if (t == SerialType.stringType) return this.ReadString(false);
            return null;
        }

        public void WriteFixedString(string str, int length)
        {
            if (str == null)
            {
                Write((byte)SerialType.nullType);
            }
            else
            {
                Write((byte)SerialType.byteArrayType);
                byte[] b = new byte[length];
                int strlen = StreamBuffer.StrToFixedBytes(str, b, Encoding.UTF8);
                Write(strlen);
                WriteByteArray(b);
            }
        }

        public string ReadFixedString()
        {
            SerialType t = (SerialType)ReadByte();
            if (t != SerialType.byteArrayType) return null;
            int strlen = ReadInt32();
            byte[] b = ReadByteArray();
            return StreamBuffer.FixedBytesToStr(b, strlen, Encoding.UTF8);
        }

        public T ReadValue<T>(SerialContextType contextType)
        {
            switch (contextType)
            {
                case SerialContextType.GenericEntityAsIDictionaryType:
                    return ReadGenericEntityAsDictionary<T>();
                case SerialContextType.GenericEntityAsIEntityType:
                    return ReadAnyEntity<T>();// ReadGenericEntityAsEntity<T>();
            }
            object o = ReadAny();
            return GenericTypes.Cast<T>(o, true);
        }

        public void WriteValueEntity(object value)
        {
            Write((byte)SerialType.anyClassType);
            WriteAnyClass(value);
        }

        public object ReadValueEntity()
        {
            SerialType t = (SerialType)ReadByte();
            if (t == SerialType.anyClassType)
            {
                throw new ArgumentException("Stream has incorrect contextType");
            }
            return ReadAnyClass();
        }

        #endregion

        #region Write/Read primitive list

        void WritePrimitive<T>(IList<T> values)
        {
            string typeName = typeof(T).Name;
            int count = values.Count;

            for (int i = 0; i < count; i++)
            {
                object value = values[i];

                if (value == null || value == DBNull.Value)
                {
                    Write((byte)SerialType.nullType);
                }
                else
                {

                    switch (typeName)
                    {
                        case "Boolean": Write((bool)value);
                            break;
                        case "Byte": Write((byte)value);
                            break;
                        case "UInt16": Write((ushort)value);
                            break;
                        case "UInt32": Write((uint)value);
                            break;
                        case "UInt64":
                            Write((ulong)value);
                            break;
                        case "SByte":
                            Write((sbyte)value);
                            break;
                        case "Int16":
                            Write((short)value);
                            break;
                        case "Int32":
                            Write((int)value);
                            break;
                        case "Int64":
                            Write((long)value);
                            break;
                        case "Char":
                            Write((char)value);
                            break;
                        case "String":
                            Write((string)value);
                            break;
                        case "Single":
                            Write((float)value);
                            break;
                        case "Double":
                            Write((double)value);
                            break;
                        case "Decimal":
                            Write((decimal)value);
                            break;
                        case "DateTime":
                            Write((DateTime)value);
                            break;
                        case "TimeSpan":
                            Write(((TimeSpan)value).Ticks);
                            break;
                        case "Guid":
                            Write((Guid)value);
                            break;
                        default:
                            Write((byte)SerialType.otherType);
                            InternalFormatter.Serialize(BaseStream, value);
                            break;
                    }
                }
            }

        }

        List<T> ReadPremitive<T>(SerialType t, int count)
        {

            List<T> list = new List<T>();

            for (int i = 0; i < count; i++)
            {
                object val = null;

                switch (t)
                {
                    case SerialType.boolType: val = ReadBoolean(); break;
                    case SerialType.byteType: val = ReadByte(); break;
                    case SerialType.uint16Type: val = ReadUInt16(); break;
                    case SerialType.uint32Type: val = ReadUInt32(); break;
                    case SerialType.uint64Type: val = ReadUInt64(); break;
                    case SerialType.sbyteType: val = ReadSByte(); break;
                    case SerialType.int16Type: val = ReadInt16(); break;
                    case SerialType.int32Type: val = ReadInt32(); break;
                    case SerialType.int64Type: val = ReadInt64(); break;
                    case SerialType.charType: val = ReadChar(); break;
                    case SerialType.stringType: val = ReadString(false); break;
                    case SerialType.singleType: val = ReadSingle(); break;
                    case SerialType.doubleType: val = ReadDouble(); break;
                    case SerialType.decimalType: val = ReadDecimal(); break;
                    case SerialType.dateTimeType: val = ReadDateTime(); break;
                    case SerialType.byteArrayType: val = ReadByteArray(); break;
                    case SerialType.charArrayType: val = ReadCharArray(); break;
                    case SerialType.guidType: val = ReadGuid(); break;
                    case SerialType.enumType: val = ReadEnum(); break;
                    case SerialType.typeType: val = ReadType(); break;
                    case SerialType.int16ArrayType: val = ReadInt16(); break;
                    case SerialType.int32ArrayType: val = ReadInt32(); break;
                    case SerialType.int64ArrayType: val = ReadInt64(); break;
                    case SerialType.stringArrayType: val = ReadString(); break;
                    case SerialType.otherType: val = InternalFormatter.Deserialize(ReaderStream); break;

                    default: val = null; break;
                }
                list.Add((T)val);
            }
            return list;
        }


        #endregion

        #region Write/Read ISerialEntity

        public void WriteSerialEntity(ISerialEntity value, bool writeType=true)
        {
            if (value == null)
            {
                Write((byte)SerialType.nullType);
            }
            else
            {
                WriteContextType(SerialContextType.SerialEntityType);
                WriteSerialEntityInternal(value, writeType);
            }
            Flush();
        }

        public T ReadSerialEntity<T>(bool readType=true)
        {
            ReadByte();
            if (readType)
            {
                Type type = ReadType();
                if (type == null)
                    return default(T);
            }
            T entity = ActivatorUtil.CreateInstance<T>();
            ((ISerialEntity)entity).EntityRead(ReaderStream, this);
            return entity;
        }

        void WriteSerialEntityInternal(object value, bool writeType = true)
        {
            if (writeType)
            {
                Write((string)value.GetType().FullName);
            }
            ((ISerialEntity)value).EntityWrite(BaseStream, this);
        }

        object ReadSerialEntityInternal()
        {
            Type type = ReadType();
            if (type == null)
                return null;
            object entity = ActivatorUtil.CreateInstance(type);
            ((ISerialEntity)entity).EntityRead(ReaderStream,this);
            return entity;
        }

        IDictionary ReadGenericEntityAsIDictionary()
        {
            return (IDictionary)ReadEntityAsDictionary(false, true);

            //string strtype = ReadString(false);
            //if (strtype == null)
            //    return null;


            ////int count = ReadInt32();
            ////if (count < 0) return null;

            ////IDictionary instance = new Hashtable();
            ////for (int i = 0; i < count; i++)
            ////{
            ////    string name = ReadString(false);
            ////    object value = ReadValue();
            ////    instance.Add(name, value);
            ////}
            ////return instance;

            //byte b = ReadByte();

            //IDictionary val = ReadGenericDictionary();

            //return val;

        }

        public IDictionary<string, object> ReadGenericEntityAsDictionary(bool validateSerialContextType)
        {
            SerialContextType t = (SerialContextType)ReadByte();

            if (validateSerialContextType)
            {
                if (t != SerialContextType.SerialEntityType && t != SerialContextType.dictionaryEntityType && t != SerialContextType.GenericEntityAsIDictionaryType)
                {
                    return null;
                }
            }

            //read GenericEntity type 
            var entityType = ReadType();

            int count = ReadInt32();
            if (count < 0) return null;

            IDictionary<string, object> instance = new Dictionary<string, object>();
            for (int i = 0; i < count; i++)
            {
                string name = ReadString(false);
                object value = ReadValue();
                instance.Add(name,value);
            }

            return instance;

            ////read GenericEntity type 
            //var entityType = ReadType();
            //if (entityType == null)
            //    return null;

            //return ReadDictionaryObject();
        }

        public IDictionary<string, TValue> ReadGenericEntityAsDictionary<TValue>(bool validateSerialContextType) 
        {
            SerialContextType t = (SerialContextType)ReadByte();

            if (validateSerialContextType)
            {
                if (t != SerialContextType.SerialEntityType && t != SerialContextType.dictionaryEntityType && t != SerialContextType.GenericEntityAsIDictionaryType)
                {
                    return null;
                }
            }

            //read GenericEntity type 
            var entityType = ReadType();

            int count = ReadInt32();
            if (count < 0) return null;

            IDictionary<string, TValue> instance = new Dictionary<string, TValue>();
            for (int i = 0; i < count; i++)
            {
                string name = ReadString(false);
                TValue value = ReadValue<TValue>();
                instance.Add(name, value);
            }

            return instance;


            //read GenericEntity type 
            //var entityType = ReadType();
            //if (entityType == null)
            //    return default(T);

            //IDictionary val = ReadHashtable();// ReadGenericDictionary();

            //return GenericTypes.Cast<T>(val, true);
        }

        public T ReadGenericEntityAsDictionary<T>()
        {
            SerialContextType t = (SerialContextType)ReadByte();

            if (t != SerialContextType.SerialEntityType && t != SerialContextType.GenericEntityAsIDictionaryType)
            {
                return default(T);
            }

            //read GenericEntity type 
            var entityType = ReadType();

            int count = ReadInt32();
            if (count < 0) return default(T);

            IDictionary instance = new Hashtable();
            for (int i = 0; i < count; i++)
            {
                string name = ReadString(false);
                object value = ReadValue();
                instance.Add(name, value);
            }


            return GenericTypes.Cast<T>(instance, true);
        }


        //public T ReadGenericEntityAsEntity<T>()
        //{
        //    //read IEntityFromDictionaryType
        //    SerialContextType t = (SerialContextType)ReadByte();

        //    if (t != SerialContextType.SerialEntityType && t != SerialContextType.GenericEntityAsIEntityType)
        //    {
        //        return default(T);
        //    }

        //    //read GenericEntity type 
        //    var entityType= ReadType();

        //    //read SerialEntityType
        //    byte b=ReadByte();

        //    int count = ReadInt32();
        //    if (count < 0) return default(T);
        //    Type keyType = ReadType();
        //    Type valueType = ReadType();

        //    T instance = ActivatorUtil.CreateInstance<T>();
        //    PropertyInfo[] pi = typeof(T).GetProperties(true);

        //    for (int i = 0; i < count; i++)
        //    {
        //        string name = ReadAny().ToString();
        //        object value=ReadAny();
        //        PropertyInfo p = pi.Where(pa => pa.Name == name).FirstOrDefault();
        //        if (p != null)
        //        {
        //            p.SetValue(instance, value, null);
        //        }
        //    }

        //    return instance;

        //}

        public object ReadAnyEntity(Type type)
        {
            ReadByte();
            if (SerializeTools.IsAssignableFromDictionary(type))
               return ReadEntityAsDictionary(type,false, true);
            return ReadAnyClass(type);
        }

        public T ReadAnyEntity<T>()
        {
            ReadByte();
            if (SerializeTools.IsAssignableFromDictionary(typeof(T), true))
                return GenericTypes.Cast<T>(ReadEntityAsDictionary(typeof(T), false, true));
            return ReadAnyClass<T>();
        }

        #endregion

        #region Write/Read SerializeInfo

        /// <summary> Adds the BinaryWriterEx buffer to the SerializationInfo at the end of GetObjectData(). </summary>
        public void AddToInfo(SerializationInfo info)
        {
            byte[] b = ((MemoryStream)this.BaseStream).ToArray();
            info.AddValue("X", b, typeof(byte[]));
        }

        public void WriteSerializeInfo(SerializeInfo info)
        {
           
            Write((string)info.EntityTypeName);
            Write((Int32)info.Formatter);
            Write(info.Count);
            foreach (KeyValuePair<string, ValueTypeInfo> kvp in info.Data)
            {
                Write(kvp.Key);
                WriteAny(kvp.Value.ItemValue, kvp.Value.ItemType);
            }

            Flush();
        }

        public SerializeInfo ReadSerializeInfo()
        {
            SerializeInfo info = null;
            //ReadBitSign();
            Type type = ReadType();
            Formatters formatter = (Formatters)ReadInt32();
            info = new SerializeInfo(type, formatter);
            int count = ReadInt32();
            if (count < 0) return info;
            for (int i = 0; i < count; i++)
            {
                info.Add(ReadString(false), ReadAny());
            }
            return info;
        }

        public void WriteSerialContext(object value)
        {
            Write((string)value.GetType().FullName);
            ((ISerialContext)value).WriteContext(this);
        }

        public object ReadSerialContext()
        {
            Type type = ReadType();
            //SerializeInfo info = ReadSerializeInfo();
            object entity = ActivatorUtil.CreateInstance(type);
            ((ISerialContext)entity).ReadContext(this);
            return entity;
        }
        #endregion

        #region Write/Read AnyClass

        internal void WriteAnyClass(object value)
        {
            if (value == null)
            {
                Write(-1);
            }

            Write((string)value.GetType().FullName);

            PropertyInfo[] p = value.GetType().GetProperties(true);
            if (p == null)
            {
                Write(-1);
            }
            else
            {
                Write(p.Length);

                foreach (PropertyInfo kvp in p)
                {
                    if (kvp.CanWrite)
                    {
                        Write(kvp.Name);
                        WriteAny(kvp.GetValue(value, null));
                    }
                }
            }
        }

        dynamic ReadAnyClassAsDynamicEntity(bool readType=false)
        {
            if (readType)
            {
                string typeName = ReadString(false);
                Type type = SerializeTools.GetQualifiedType(typeName);
            }

            int count = ReadInt32();
            if (count < 0) return null;
            DynamicEntity entity = ActivatorUtil.CreateInstance<DynamicEntity>();
            for (int i = 0; i < count; i++)
            {
                string name = ReadString(false);
                object value = ReadAny();
                entity[name]=value;
           }
            return entity;
        }
        T ReadAnyClassAsDynamicEntity<T>(bool readType = false)
        {
            if (readType)
            {
                string typeName = ReadString(false);
                Type type = SerializeTools.GetQualifiedType(typeName);
            }

            int count = ReadInt32();
            if (count < 0) return default(T);
            DynamicEntity entity = ActivatorUtil.CreateInstance<DynamicEntity>();
            for (int i = 0; i < count; i++)
            {
                string name = ReadString(false);
                object value = ReadAny();
                entity[name] = value;
            }
            return entity.Cast<T>();
        }


        internal object ReadAnyClass()
        {
            string typeName = ReadString(false);
            Type type = SerializeTools.GetQualifiedType(typeName);

            //Type type = ReadType();
            if (type == null)
            {
                if (!string.IsNullOrWhiteSpace(typeName))
                {
                    //is outside assembly ,none QualifiedType
                    return ReadAnyClassAsDynamicEntity();
                }
                throw new ArgumentNullException("ReadAnyClassInternal.type");
            }
            int count = ReadInt32();
            if (count < 0) return null;
            object entity = ActivatorUtil.CreateInstance(type);
            PropertyInfo[] p = entity.GetType().GetProperties(true);

            if (entity == null)
            {
                return null;
            }
            if (p == null)
            {
                return null;
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    string name = ReadString(false);
                    object value = ReadAny();
                    PropertyInfo pi = p.Where(pr => pr.Name == name).FirstOrDefault();
                    if (pi != null && pi.CanWrite)
                    {
                        pi.SetValue(entity, value, null);
                    }
                }

                return entity;
            }
        }

        internal T ReadAnyClass<T>()
        {
            string typeName = ReadString(false);
            Type type = SerializeTools.GetQualifiedType(typeName);

            //Type type = ReadType();
            if (type == null)
            {
                if (!string.IsNullOrWhiteSpace(typeName))
                {
                    //is outside assembly ,none QualifiedType
                    return ReadAnyClassAsDynamicEntity<T>();
                }
                throw new ArgumentNullException("ReadAnyClassInternal.type");
            }

            //Type type = ReadType();
            int count = ReadInt32();
            if (count < 0) return default(T);

            T entity = ActivatorUtil.CreateInstance<T>();
            PropertyInfo[] p = entity.GetType().GetProperties(true);

            if (entity == null)
            {
                return default(T);
            }
            if (p == null)
            {
                return default(T);
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    string name = ReadString(false);
                    object value = ReadAny();
                    PropertyInfo pi = p.Where(pr => pr.Name == name).FirstOrDefault();
                    if (pi != null && pi.CanWrite)
                    {
                        pi.SetValue(entity, value, null);
                    }

                }

                return entity;
            }
        }

        internal object ReadAnyClass(Type type)
        {
            string typeName = ReadString(false);

            if (type == null)
            {
                throw new ArgumentNullException("ReadAnyClassInternal.type");
            }

            int count = ReadInt32();
            if (count < 0) return null;
            object entity = ActivatorUtil.CreateInstance(type);
            PropertyInfo[] p = entity.GetType().GetProperties(true);

            if (entity == null)
            {
                return null;
            }
            if (p == null)
            {
                return null;
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    string name = ReadString(false);
                    object value = ReadAny();
                    PropertyInfo pi = p.Where(pr => pr.Name == name).FirstOrDefault();
                    if (pi != null && pi.CanWrite)
                    {
                        pi.SetValue(entity, value, null);
                    }

                }

                return entity;
            }
        }
        /// <summary>
        /// TryReadToAnyClass
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="readSerialType"></param>
        /// <returns></returns>
        public bool TryReadToAnyClass(object entity, bool readSerialType)
        {

            if (entity == null)
            {
                throw new ArgumentNullException("TryReadToAnyClass.entity");
            }
            if (readSerialType)
            {
                SerialType t = (SerialType)ReadByte();
            }
            string typeName = ReadString(false);

            int count = ReadInt32();
            if (count < 0) return false;
            //object entity = ActivatorUtil.CreateInstance(type);
            PropertyInfo[] p = entity.GetType().GetProperties(true);

            if (p == null)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    string name = ReadString(false);
                    object value = ReadAny();
                    PropertyInfo pi = p.Where(pr => pr.Name == name).FirstOrDefault();
                    if (pi != null && pi.CanWrite)
                    {
                        pi.SetValue(entity, value, null);
                    }

                }

                return true;
            }
        }

        #endregion

        #region Collection<T>

        /// <summary>
        ///  Writes a generic ICollection (such as an IList of T) to the buffer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="c"></param>
        internal void WriteList<T>(IList<T> c)
        {
            if (c == null)
            {
                Write(-1);
            }
            else
            {
                Write(c.Count);
                foreach (T item in c) WriteAny(item);
            }
        }

        /// <summary> Reads a generic list from the buffer. </summary>
        internal IList<T> ReadList<T>() 
        {
            int count = ReadInt32();
            if (count < 0) return null;
            IList<T> d = new List<T>();
            for (int i = 0; i < count; i++) d.Add((T)ReadAny());
            return d;
        }

        internal void WriteCollection(ICollection c)
        {
            if (c == null)
            {
                Write(-1);
            }
            else
            {
                Write(c.Count);
                foreach (object item in c) WriteAny(item);
            }
        }

        internal ICollection ReadCollection()
        {
            int count = ReadInt32();
            if (count < 0) return null;
            IList d = new ArrayList();
            for (int i = 0; i < count; i++) d.Add(ReadAny());
            return d;
        }

        internal void WriteArray<T>(IList<T> c) 
        {
            if (c == null)
            {
                Write(-1);
            }
            else
            {
                Write(c.Count);
                foreach (T item in c) WriteAny(item);
            }
        }

        internal T[] ReadArray<T>() 
        {
            int count = ReadInt32();
            if (count < 0) return null;
            IList<T> d = new List<T>();
            for (int i = 0; i < count; i++) d.Add((T)ReadAny());
            return d.ToArray();
        }

        internal void WritePrimitiveArray<T>(IList<T> c) where T : struct
        {
            if (!SerializeTools.IsPrimitive(typeof(T)))
            {
                throw new NotSupportedException("Type not supported");
            }
           if (c == null)
            {
                Write(-1);
            }
            else
            {
                Write(c.Count);
                WritePrimitive<T>(c);
            }
        }

        internal T[] ReadPrimitiveArray<T>(SerialType t) where T : struct
        {
            if (!SerializeTools.IsPrimitive(typeof(T)))
            {
                throw new NotSupportedException("Type not supported");
            }
            int count = ReadInt32();
            if (count < 0) return null;
            IList<T> d = ReadPremitive<T>(t, count);
            return d.ToArray();
        }

        #endregion

        #region NameValueCollection/string Dictionary

        internal void WriteStringDictionary(StringDictionary d)
        {
            if (d == null)
            {
                Write(-1);
            }
            else
            {
                Write(d.Count);
                foreach (DictionaryEntry entry in d)
                {
                    WriteString((string)entry.Key);
                    WriteString((string)entry.Value);
                }
            }
        }

        internal StringDictionary ReadStringDictionary()
        {
            int count = ReadInt32();
            if (count < 0) return null;
            StringDictionary d = new StringDictionary();

            SerialType st = SerialType.stringType;
            for (int i = 0; i < count; i++)
            {
                d.Add(ReadItem<string>(st), ReadItem<string>(st));
            }
            return d;
        }

        internal void WriteNameValueCollection(NameValueCollection d)
        {
            if (d == null)
            {
                Write(-1);
            }
            else
            {
                Write(d.Count);
                foreach (string key in d)
                {
                    WriteString(key);
                    WriteString(d[key]);
                }
            }
        }

        internal NameValueCollection ReadNameValueCollection()
        {
            int count = ReadInt32();
            if (count < 0) return null;
            NameValueCollection d = new NameValueCollection();

            SerialType st = SerialType.stringType;
            for (int i = 0; i < count; i++)
            {
                d.Add(ReadItem<string>(st), ReadItem<string>(st));
            }
            return d;
        }

        #endregion

        #region public write\read direct Dictionary

        public void WriteDirectDictionary<TKey, TValue>(IDictionary<TKey, TValue> d, bool writeSerialType = false)
        {
            if (writeSerialType)
            {
                Write((byte)SerialType.dictionaryGenericType);
            }

            if (d == null)
            {
                Write(-1);
            }
            else
            {
                SerialType oKey = SerializeTools.GetSerialType<TKey>(null);
                SerialType oValue = SerializeTools.GetSerialType<TValue>(null);

                Write(d.Count);
                foreach (KeyValuePair<TKey, TValue> kvp in d)
                {
                    WriteItem(oKey, kvp.Key, false);
                    WriteItem(oValue, kvp.Value, false);
                }
            }
        }

        public IDictionary<TKey, TValue> ReadDirectDictionary<TKey, TValue>(bool readSerialType = false)
        {
            if (readSerialType)
            {
                SerialType t = (SerialType)ReadByte();
                if (t != SerialType.dictionaryGenericType)
                {
                    throw new ArgumentException("SerialType is incorrect: " + t.ToString());
                }
            }
            int count = ReadInt32();
            if (count < 0) return null;
            IDictionary<TKey, TValue> d = new Dictionary<TKey, TValue>();

            SerialType oKey = SerializeTools.GetSerialType<TKey>(null);
            SerialType oValue = SerializeTools.GetSerialType<TValue>(null);
            for (int i = 0; i < count; i++)
            {
                d.Add(new KeyValuePair<TKey, TValue>(ReadItem<TKey>(oKey), ReadItem<TValue>(oValue)));
            }
            return d;
        }

        public bool TryReadDirectToDictionary<TKey, TValue>(IDictionary<TKey, TValue> d, bool readSerialType = false)
        {
            if (readSerialType)
            {
                SerialType t = (SerialType)ReadByte();
                if (t != SerialType.dictionaryGenericType)
                {
                    throw new ArgumentException("SerialType is incorrect: " + t.ToString());
                }
            }
            int count = ReadInt32();
            if (count < 0) return false;

            if (d == null)
                d = new Dictionary<TKey, TValue>();

            SerialType oKey = SerializeTools.GetSerialType<TKey>(null);
            SerialType oValue = SerializeTools.GetSerialType<TValue>(null);
            for (int i = 0; i < count; i++)
            {
                d.Add(new KeyValuePair<TKey, TValue>(ReadItem<TKey>(oKey), ReadItem<TValue>(oValue)));
            }
            return true;
        }


        #endregion

        #region hashtable

        internal void WriteHashtable(IDictionary d)
        {
            if (d == null)
            {
                Write(-1);
            }
            else
            {
                Write(d.Count);
                foreach (System.Collections.DictionaryEntry kvp in d)
                {
                    WriteAny(kvp.Key);
                    WriteAny(kvp.Value);
                }
            }
        }

        internal IDictionary ReadHashtable()
        {
            int count = ReadInt32();
            if (count < 0) return null;
            IDictionary d = new Hashtable();
            for (int i = 0; i < count; i++)
                d.Add(ReadAny(), ReadAny());
            return d;
        }

        #endregion

        #region public DictionaryEntity

        public void WriteDictionaryAsEntity(IDictionary<string, object> d, bool writeSerialType, bool writeType)
        {
            if (writeSerialType)
            {
                Write((byte)SerialType.dictionaryGenericType);
            }

            if (d == null)
            {
                Write(-1);
            }
            else
            {
                if (writeType)
                    WriteType(d.GetType());
                Write(d.Count);

                foreach (var entry in d)
                {
                    Write(entry.Key);
                    WriteAny(entry.Value);
                }
            }
        }

        public IDictionary<string,object> ReadEntityAsDictionary(bool readSerialType, bool readType)
        {
            if (readSerialType)
            {
                SerialType t = (SerialType)ReadByte();
            }
            
            string type= (readType)?ReadString(false):"";

            int count = ReadInt32();
            if (count < 0) return null;
            Dictionary<string, object> d = new Dictionary<string, object>();
            for (int i = 0; i < count; i++)
            {
                string key = ReadString(false);
                object value = ReadAny();
                d.Add(key, value);
            }
            return d;
        }

        public IDictionary ReadEntityAsDictionary(Type type, bool readSerialType, bool readType)
        {
            if (readSerialType)
            {
                SerialType t = (SerialType)ReadByte();
            }

            string typename = (readType) ? ReadString(false) : "";

            int count = ReadInt32();
            if (count < 0) return null;

            IDictionary d = (IDictionary)ActivatorUtil.CreateInstance(type);

            for (int i = 0; i < count; i++)
            {
                string key = ReadString(false);
                object value = ReadAny();
                d.Add(key, value);
            }
            return d;
        }

        public bool TryReadEntityToDictionary(Dictionary<string, object> d, bool readSerialType, bool readType)
        {
            if (readSerialType)
            {
                SerialType t = (SerialType)ReadByte();
            }
            string type = (readType) ? ReadString(false) : "";

            int count = ReadInt32();
            if (count < 0) return false;
            if(d==null)
                d = new Dictionary<string, object>();
            for (int i = 0; i < count; i++)
            {
                string key = ReadString(false);
                object value = ReadAny();
                d.Add(key, value);
            }
            return true;
        }

        #endregion

        #region Dictionary

        internal void WriteAssignFromDictionary(object value)
        {
            if (value == null)
            {
                Write(-1);
            }
            else
            {
                WriteType(value.GetType());

                IDictionary d = (IDictionary)value;
                Write(d.Count);
                foreach (System.Collections.DictionaryEntry kvp in d)
                {
                    WriteAny(kvp.Key);
                    WriteAny(kvp.Value);
                }
            }
        }

        internal IDictionary ReadAssignFromDictionary()
        {
            var type=ReadType();
            var instance= ActivatorUtil.CreateInstance(type);
            IDictionary d =(IDictionary) instance;
            int count = ReadInt32();
            if (count < 0) return null;
            //IDictionary d = new Hashtable();
            for (int i = 0; i < count; i++) //d[ReadAny()] = ReadAny();
                d.Add(ReadAny(), ReadAny());
            return d;
        }

        public void ReadToAssignDictionary(IDictionary d, bool readSerialType = true)
        {
            if (d == null)
            {
                throw new ArgumentNullException("ReadToHashtable.d");
            }
            if (readSerialType)
            {
                SerialType t = (SerialType)ReadByte();
                //if (t != SerialType.dictionaryAssignType)
                //{
                //    throw new ArgumentException("SerialType is incorrect: " + t.ToString());
                //}
            }
            int count = ReadInt32();
            if (count < 0) return;
            for (int i = 0; i < count; i++)
                d.Add(ReadAny(), ReadAny());
        }

        #endregion

        #region generic dictionary

        internal void WriteGenericDictionary(IDictionary d)
        {
            if (d == null)
            {
                Write(-1);
            }
            else
            {

                Type dicType = SerializeTools.GetGenericBaseType(d.GetType());
                Type keyType = dicType.GetGenericArguments()[0];
                Type valueType = dicType.GetGenericArguments()[1];

                Write(d.Count);
                Write(keyType.FullName);
                Write(valueType.FullName);


                foreach (DictionaryEntry kvp in d)
                {
                    WriteAny(kvp.Key);
                    WriteAny(kvp.Value);
                }
            }
        }

        internal IDictionary ReadGenericDictionary()
        {

            int count = ReadInt32();
            if (count < 0) return null;
            Type keyType = ReadType();
            Type valueType = ReadType();

            IDictionary d = SerializeTools.CreateGenericDictionary(keyType, valueType);

            for (int i = 0; i < count; i++)
            {
                d.Add(ReadAny(), ReadAny());
            }
            return d;

        }

        public bool TryReadToGenericDictionary(IDictionary d, bool readSerialType = true)
        {
            if (d == null)
            {
                throw new ArgumentNullException("ReadToGenericDictionary.d");
            }
            if (readSerialType)
            {
                SerialType t = (SerialType)ReadByte();
                if (t != SerialType.dictionaryGenericType)
                {
                    throw new ArgumentException("SerialType is incorrect: " + t.ToString());
                }
            }
            int count = ReadInt32();
            if (count < 0) return false;
            Type keyType = ReadType();
            Type valueType = ReadType();

            for (int i = 0; i < count; i++)
            {
                d.Add(ReadAny(), ReadAny());
            }
            return true;
        }

        public bool TryReadToGenericDictionary<TKey, TValue>(IDictionary<TKey, TValue> d, bool readSerialType = true)
        {
            if (d == null)
            {
                throw new ArgumentNullException("ReadToGenericDictionary.d");
            }
            if (readSerialType)
            {
                SerialType t = (SerialType)ReadByte();
                if (t != SerialType.dictionaryGenericType)
                {
                    throw new ArgumentException("SerialType is incorrect: " + t.ToString());
                }
            }
            int count = ReadInt32();
            if (count < 0) return false;
            Type keyType = ReadType();
            Type valueType = ReadType();

            //for (int i = 0; i < count; i++)
            //{
            //    d.Add(ReadAny(), ReadAny());
            //}
            //SerialType oKey = SerializeTools.GetSerialType<TKey>(null);
            //SerialType oValue = SerializeTools.GetSerialType<TValue>(null);

            for (int i = 0; i < count; i++)
            {
                d.Add(new KeyValuePair<TKey, TValue>(ReadAny<TKey>(), ReadAny<TValue>()));
            }
            return true;
        }


        #endregion

        #region KeyValue


        public void WriteKeyValue<TKey, TValue>(List<KeyValuePair<TKey, TValue>> d)
        {
            if (d == null)
            {
                Write(-1);
            }
            else
            {
                SerialType oKey = SerializeTools.GetSerialType<TKey>(null);
                SerialType oValue = SerializeTools.GetSerialType<TValue>(null);
                bool isKnownKeyType = SerializeTools.IsKnownGenericType(typeof(TKey));
                bool isKnownValueType = SerializeTools.IsKnownGenericType(typeof(TValue));
                Write(d.Count);

                byte b1 = (byte)(isKnownKeyType ? 1 : 0);
                byte b2 = (byte)(isKnownValueType ? 1 : 0);

                Write(b1);
                Write(b2);

                foreach (KeyValuePair<TKey, TValue> kvp in d)
                {
                    if (!isKnownKeyType)
                        oKey = SerializeTools.GetSerialType(kvp.Key==null ? null:kvp.Key.GetType(), null);
                    if (!isKnownValueType)
                        oValue = SerializeTools.GetSerialType(kvp.Value == null ? null : kvp.Value.GetType(), null);
                    if (oKey == SerialType.nullType)
                        continue;
                    WriteItem(oKey, kvp.Key, !isKnownKeyType);
                    WriteItem(oValue, kvp.Value, !isKnownValueType);
                }
            }
        }

        public List<KeyValuePair<TKey, TValue>> ReadKeyValue<TKey, TValue>()
        {
            int count = ReadInt32();
            if (count < 0) return null;
            List<KeyValuePair<TKey, TValue>> d = new List<KeyValuePair<TKey, TValue>>();

            SerialType oKey=SerializeTools.GetSerialType<TKey>(null);
            SerialType oValue = SerializeTools.GetSerialType<TValue>(null);

            bool isKnownKeyType = ReadByte()==1;
            bool isKnownValueType = ReadByte() == 1;

            for (int i = 0; i < count; i++)
            {
                d.Add(new KeyValuePair<TKey, TValue>(ReadItem<TKey>(oKey,isKnownKeyType), ReadItem<TValue>(oValue,isKnownValueType)));
            }
            return d;
        }


        internal KeyValuePair<TKey, TValue> ReadGenericKeyValuePair<TKey, TValue>(SerialType oKey, SerialType oValue)
        {
            return new KeyValuePair<TKey, TValue>(ReadItem<TKey>(oKey), ReadItem<TValue>(oValue));
        }

        internal void WriteGenericKeyValue<T>(List<KeyValuePair<string, T>> d)
        {
            if (d == null)
            {
                Write(-1);
            }
            else
            {

                Write(d.Count);
                foreach (KeyValuePair<string, T> kvp in d)
                {
                    Write(kvp.Key);
                    WriteAny(kvp.Value);

                }
            }
        }

        internal List<KeyValuePair<string, T>> ReadGenericKeyValue<T>()
        {
            int count = ReadInt32();
            if (count < 0) return null;
            List<KeyValuePair<string, T>> d = new List<KeyValuePair<string, T>>();
            for (int i = 0; i < count; i++)
            {
                d.Add(new KeyValuePair<string, T>(ReadString(false), (T)ReadAny()));
            }
            return d;
        }

        #endregion

        #region Dynamic Collection Generic

         internal void WriteGenericList(IList value)
        {

            if (value == null)
            {
                Write(-1);
            }
            else
            {
                Type listType = SerializeTools.GetGenericBaseType(value.GetType());
                Type itemType = listType.GetGenericArguments()[0];

                Write(value.Count);
                Write(itemType.FullName);

                foreach (var item in value)
                {
                    WriteAny(item);
                }
            }
        }

        internal IList ReadGenericList()
        {
            int count = ReadInt32();
            if (count < 0) return null;
            Type type = ReadType();  
            if(type==null)
            {
                Console.WriteLine("Error read type: ReadGenericList");
                return null;
            }
            IList list = SerializeTools.CreateGenericList(type);

            try
            {
                for (int i = 0; i < count; i++)
                {
                    var item = ReadAny();
                    list.Add(item);
                }
            }
            catch(Exception ex)
            {
                string err = ex.Message;
            }
            return list;
        }

         
        #endregion
        
        #region DataSets

        void WriteDataSetXml(DataSet ds)
        {
            WriteString(ds.DataSetName);
            Write(ds.Tables.Count);
            WriteString(ds.Namespace);
            WriteString(ds.Prefix);
            Write(ds.CaseSensitive);
            ds.WriteXmlSchema(BaseStream);
            ds.WriteXml(BaseStream);
        }

        internal DataSet ReadDataSetXml()
        {
            string dsName = ReadString();
            int tables = ReadInt32();
            string dsNs = ReadString();
            string dsPrefix = ReadString();
            bool dsCaseSensitive = ReadBoolean();

            DataSet ds = new DataSet(dsName);
            ds.Namespace = dsNs;
            ds.Prefix = dsPrefix;
            ds.CaseSensitive = dsCaseSensitive;

            ds.ReadXmlSchema(ReaderStream);
            ds.ReadXml(ReaderStream);
            return ds;
        }

        public void WriteDataSet(DataSet ds)
        {
            WriteString(ds.DataSetName);
            Write(ds.Tables.Count);
            WriteString(ds.Namespace);
            WriteString(ds.Prefix);
            Write(ds.CaseSensitive);


            foreach (DataTable dt in ds.Tables)
            {
                WriteDataTable(dt);
            }
        }

        public DataSet ReadDataSet()
        {
            string dsName = ReadString();
            int tables = ReadInt32();
            string dsNs = ReadString();
            string dsPrefix = ReadString();
            bool dsCaseSensitive = ReadBoolean();

            DataSet ds = new DataSet(dsName);
            ds.Namespace = dsNs;
            ds.Prefix = dsPrefix;
            ds.CaseSensitive = dsCaseSensitive;

            for (int i = 0; i < tables; i++)
            {
                ds.Tables.Add(ReadDataTable());
            }
            return ds;
        }
        
        public void WriteDataTable(DataTable dt)
        {
            WriteString(dt.TableName);
            Write(dt.Columns.Count);
            Write(dt.Rows.Count);

            foreach (DataColumn dc in dt.Columns)
            {
                WriteString(dc.ColumnName);
                Write(dc.AllowDBNull);
                WriteString(dc.DataType.AssemblyQualifiedName);
            }

            foreach (DataRow dr in dt.Rows)
            {
                foreach (DataColumn dc in dt.Columns)
                {
                    WriteAny(dr[dc]);
                }
            }

        }

        public DataTable ReadDataTable()
        {
            string tableName = ReadString();
            int columns = ReadInt32();
            int rows = ReadInt32();
            DataTable dtIn = new DataTable();

            for (int x = 0; x < columns; x++)
            {
                string columnName = ReadString();
                bool allowNulls = ReadBoolean();
                string type = ReadString();

                DataColumn dc = new DataColumn(columnName, Type.GetType(type));
                dc.AllowDBNull = allowNulls;
                dtIn.Columns.Add(dc);
            }

            for (int y = 0; y < rows; y++)
            {
                DataRow dr = dtIn.NewRow();

                for (int x = 0; x < columns; x++)
                {
                    DataColumn dc = dtIn.Columns[x];
                    object obj = ReadAny();

                    dr[dc] = obj;
                }

                dtIn.Rows.Add(dr);
            }
            return dtIn;
        }


        public void WriteXmlDocument(XmlDocument doc, bool omitXmlDeclaration=true)
        {
            WriteString(doc.Prefix);
            XmlWriter writer = XmlWriter.Create(BaseStream, new XmlWriterSettings() { OmitXmlDeclaration = omitXmlDeclaration });
            doc.WriteTo(writer);
            writer.Flush();
        }

        public XmlDocument ReadXmlDocument()
        {
            string prefix = ReadString();
            XmlDocument x = new XmlDocument();
            x.Load(BaseStream);
            return x;
        }

        public void SerializeXmlDocument(XmlDocument doc, bool omitXmlDeclaration = true)
        {
            WriteString(doc.Prefix);
            StringBuilder sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb, new XmlWriterSettings() { OmitXmlDeclaration = omitXmlDeclaration });
            doc.WriteTo(writer);
            writer.Flush();
            WriteString(sb.ToString());
        }

        public XmlDocument DeserializeXmlDocument()
        {
            string prefix = ReadString();
            string xml = ReadString();
            var des = new System.Xml.Serialization.XmlSerializer(typeof(XmlDocument));
            XmlDocument x = (XmlDocument)des.Deserialize(new StringReader(xml));
            return x;
        }


        public void WriteXmlNode(XmlNode node)
        {
            WriteXmlDocument(node.OwnerDocument,true);
        }

        public XmlNode ReadXmlNode()
        {
            XmlDocument doc = ReadXmlDocument();
            if (doc == null)
                return null;
            return doc.FirstChild;
        }

        T xmlDeserialize<T>(string xmlString)
        {
            return (T)(new System.Xml.Serialization.XmlSerializer(typeof(T))).Deserialize(new StringReader(xmlString));
        }

        
        #endregion

        #region override

        /// <summary> Writes a byte array to the buffer.  Overrides the base implementation to
        /// send the length of the array which is needed when it is retrieved </summary>
        public void WriteByteArray(byte[] b)
        {
            if (b == null)
            {
                Write(-1);
            }
            else
            {
                int len = b.Length;
                Write(len);
                if (len > 0) this.Write(b);
            }
        }

        /// <summary> Reads a byte array from the buffer, handling nulls and the array length. </summary>
        public byte[] ReadByteArray()
        {
            int len = ReadInt32();
            if (len > 0) return ReadBytes(len);
            if (len < 0) return null;
            return new byte[0];
        }

        /// <summary> Writes a char array to the buffer.  Overrides the base implementation to
        /// sends the length of the array which is needed when it is read. </summary>
        public void WriteCharArray(char[] c)
        {
            if (c == null)
            {
                Write(-1);
            }
            else
            {
                int len = c.Length;
                Write(len);
                if (len > 0) this.Write(c);
            }
        }

        /// <summary> Reads a char array from the buffer, handling nulls and the array length. </summary>
        public char[] ReadCharArray()
        {
            int len = ReadInt32();
            if (len > 0) return ReadChars(len);
            if (len < 0) return null;
            return new char[0];
        }

        

        #endregion

        #region complex

        public object StreamToValue(NetStream stream)
        {
            ReStreamer(stream);
            return ReadAny();
        }
        public object StreamToAnyEntity(NetStream stream, Type type)
        {
            ReStreamer(stream);
            return ReadAnyEntity(type);
        }
        public T StreamToAnyEntity<T>(NetStream stream)
        {
            ReStreamer(stream);
            return ReadAnyEntity<T>();
        }
        public void WriteNetStream(NetStream stream)
        {
            if (stream == null)
            {
                Write(-1);
            }
            else
            {
                byte[] b = stream.ToArray();
                Int64 length = b.Length;
                Write(length);
                //WriteType(stream.GetType());
                if (length > 0) this.Write(b);
            }
        }


        public NetStream ReadNetStream()
        {
            Int64 count = ReadInt64();
            //Type type = ReadType();
            if (count <= 0) return null;
            byte[] bytes = ReadBytes((int)count);
            return new NetStream(bytes);
        }

        public void WriteStream(Stream stream)
        {
            if (stream == null)
            {
                Write(-1);
            }
            else
            {
                byte[] b = SerializeTools.StreamToBytes(stream);
                Int64 length = b.Length;
                Write(length);
                WriteType(stream.GetType());
                if (length > 0) this.Write(b);
            }
        }

       
        public Stream ReadStream()
        {
            Int64 count = ReadInt64();
            Type type = ReadType();
            if (count < 0) return null;
            byte[] bytes = ReadBytes((int)count);
            return SerializeTools.BytesToStream(bytes, type);
        }
 
        #endregion

        #region read/write ex

        /// <summary>
        /// Writes a Guid to the buffer.
        /// </summary>
        /// <param name="g"></param>
        public virtual void Write(Guid g)
        {
            Write(g.ToByteArray());
        }
        
        /// <summary> Reads a Guid from the buffer. </summary>
        public virtual Guid ReadGuid()
        {
            return new Guid(ReadBytes(16));
        }

        /// <summary>
        /// Writes a Enum to the buffer.
        /// </summary>
        /// <param name="value"></param>
        public virtual void WriteEnum(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("WriteEnum.value");
            }
            WriteType(value.GetType());
            Write(Convert.ToInt32(value));
        }

        /// <summary> Reads a Enum from the buffer. </summary>
        public virtual object ReadEnum()
        {
            Type type = ReadType();
            int value = ReadInt32();
            return Enum.ToObject(type, value);
        }

        //public void WriteNetStream(NetStream stream)
        //{
        //    if (stream == null)
        //    {
        //        Write(-1);
        //    }
        //    else
        //    {
        //        byte[] b = SerializeTools.StreamToBytes(stream);
        //        Int64 length = b.Length;
        //        Write(length);
        //        if (length > 0) this.Write(b);
        //        //stream.CopyTo(BaseStream, length);
        //    }
        //}

        //public NetStream ReadNetStream()
        //{
        //    Int64 count = ReadInt64();
        //    if (count < 0) return null;
        //    byte[] bytes = ReadBytes((int)count);
        //    return new NetStream(bytes);
        //}

       
        /// <summary>
        /// Writes a Type to the buffer.
        /// </summary>
        /// <param name="type"></param>
        public virtual void WriteType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("WriteType.value");
            }
            Write((string)type.FullName);
        }

        /// <summary> Reads a Type from the buffer. </summary>
        public virtual Type ReadType()
        {
            string type=ReadString(false);
            return SerializeTools.GetQualifiedType(type);
        }

        /// <summary>
        /// Writes a DateTime to the buffer.
        /// </summary>
        /// <param name="value"></param>
        public virtual void Write(DateTime value) 
        { 
            Write(value.Ticks); 
        }

        /// <summary> Reads a DateTime from the buffer. </summary>
        public virtual DateTime ReadDateTime()
        {
            //var tics = ReadInt64();
            //return new DateTime(tics);

            return new DateTime(ReadInt64());
        }

        /// <summary>
        /// Writes a TimeSpan to the buffer.
        /// </summary>
        /// <param name="value"></param>
        public virtual void Write(TimeSpan value) 
        { 
            Write(value.Ticks); 
        }

        /// <summary> Reads a TimeSpan from the buffer. </summary>
        public virtual TimeSpan ReadTimeSpan()
        {
            return new TimeSpan(ReadInt64());
        }

        #endregion

        #region writer

        public virtual long Seek(int offset, SeekOrigin origin)
        {
            return this.m_stream.Seek((long)offset, origin);
        }

        public virtual void Write(bool value)
        {
            this.m_buffer[0] = value ? ((byte)1) : ((byte)0);
            this.m_stream.Write(this.m_buffer, 0, 1);
            _bytesWriten += 1;
        }

        
        public virtual void Write(byte value)
        {
            this.m_stream.WriteByte(value);
            _bytesWriten += 1;
        }

        
        [SecuritySafeCritical]
        public virtual unsafe void Write(char ch)
        {
            if (char.IsSurrogate(ch))
            {
                throw new ArgumentException("Arg Surrogates Not Allowed As Single Char");
            }
            int count = 0;
            fixed (byte* numRef = this.m_buffer)
            {
                count = this.m_encoder.GetBytes(&ch, 1, numRef, 0x10, true);
            }
            this.m_stream.Write(this.m_buffer, 0, count);
            _bytesWriten += count;
        }

        protected virtual void Write(byte[] buffer)
        {

            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            this.m_stream.Write(buffer, 0, buffer.Length);
            _bytesWriten += buffer.Length;
        }

        public virtual void Write(decimal value)
        {
            int[] bits = decimal.GetBits(value);
            int lo = bits[0];
            int mid = bits[1];
            int hi = bits[2];
            int flags = bits[3];

            this.m_buffer[0] = (byte)lo;
            this.m_buffer[1] = (byte)(lo >> 8);
            this.m_buffer[2] = (byte)(lo >> 0x10);
            this.m_buffer[3] = (byte)(lo >> 0x18);
            this.m_buffer[4] = (byte)mid;
            this.m_buffer[5] = (byte)(mid >> 8);
            this.m_buffer[6] = (byte)(mid >> 0x10);
            this.m_buffer[7] = (byte)(mid >> 0x18);
            this.m_buffer[8] = (byte)hi;
            this.m_buffer[9] = (byte)(hi >> 8);
            this.m_buffer[10] = (byte)(hi >> 0x10);
            this.m_buffer[11] = (byte)(hi >> 0x18);
            this.m_buffer[12] = (byte)flags;
            this.m_buffer[13] = (byte)(flags >> 8);
            this.m_buffer[14] = (byte)(flags >> 0x10);
            this.m_buffer[15] = (byte)(flags >> 0x18);

            //decimal.GetBytes(value, this._buffer);
            //GetBytes(value, this.m_buffer);
            this.m_stream.Write(this.m_buffer, 0, 0x10);
            _bytesWriten += 16;
        }

        [SecuritySafeCritical]
        public virtual unsafe void Write(double value)
        {
            ulong num = *((ulong*)&value);
            this.m_buffer[0] = (byte)num;
            this.m_buffer[1] = (byte)(num >> 8);
            this.m_buffer[2] = (byte)(num >> 0x10);
            this.m_buffer[3] = (byte)(num >> 0x18);
            this.m_buffer[4] = (byte)(num >> 0x20);
            this.m_buffer[5] = (byte)(num >> 40);
            this.m_buffer[6] = (byte)(num >> 0x30);
            this.m_buffer[7] = (byte)(num >> 0x38);
            this.m_stream.Write(this.m_buffer, 0, 8);
            _bytesWriten += 8;
        }

        protected virtual void Write(char[] chars)
        {

            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }
            byte[] buffer = this.m_encoding.GetBytes(chars, 0, chars.Length);
            this.m_stream.Write(buffer, 0, buffer.Length);
            _bytesWriten += buffer.Length;
        }

        public virtual void Write(short value)
        {
            this.m_buffer[0] = (byte)value;
            this.m_buffer[1] = (byte)(value >> 8);
            this.m_stream.Write(this.m_buffer, 0, 2);
            _bytesWriten += 2;
        }

        public virtual void Write(int value)
        {
            this.m_buffer[0] = (byte)value;
            this.m_buffer[1] = (byte)(value >> 8);
            this.m_buffer[2] = (byte)(value >> 0x10);
            this.m_buffer[3] = (byte)(value >> 0x18);
            this.m_stream.Write(this.m_buffer, 0, 4);
            _bytesWriten += 4;
        }

        public virtual void Write(long value)
        {
            this.m_buffer[0] = (byte)value;
            this.m_buffer[1] = (byte)(value >> 8);
            this.m_buffer[2] = (byte)(value >> 0x10);
            this.m_buffer[3] = (byte)(value >> 0x18);
            this.m_buffer[4] = (byte)(value >> 0x20);
            this.m_buffer[5] = (byte)(value >> 40);
            this.m_buffer[6] = (byte)(value >> 0x30);
            this.m_buffer[7] = (byte)(value >> 0x38);
            this.m_stream.Write(this.m_buffer, 0, 8);
            _bytesWriten += 8;
        }

        public virtual void Write(sbyte value)
        {
            this.m_stream.WriteByte((byte)value);
            _bytesWriten += 1;
        }

        [SecuritySafeCritical]
        public virtual unsafe void Write(float value)
        {
            uint num = *((uint*)&value);
            this.m_buffer[0] = (byte)num;
            this.m_buffer[1] = (byte)(num >> 8);
            this.m_buffer[2] = (byte)(num >> 0x10);
            this.m_buffer[3] = (byte)(num >> 0x18);
            this.m_stream.Write(this.m_buffer, 0, 4);
            _bytesWriten += 4;
        }


        [SecuritySafeCritical]
        protected virtual unsafe void Write(string value)
        {

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            int byteCount = this.m_encoding.GetByteCount(value);
            this.Write7BitEncodedInt(byteCount);
            if (this._largeByteBuffer == null)
            {
                this._largeByteBuffer = new byte[0x100];
                this._maxChars = 0x100 / this.m_encoding.GetMaxByteCount(1);
            }
            if (byteCount <= 0x100)
            {
                this.m_encoding.GetBytes(value, 0, value.Length, this._largeByteBuffer, 0);
                this.m_stream.Write(this._largeByteBuffer, 0, byteCount);
                _bytesWriten += byteCount;
            }
            else
            {
                int charCount;
                int num2 = 0;
                for (int i = value.Length; i > 0; i -= charCount)
                {
                    charCount = (i > this._maxChars) ? this._maxChars : i;
                    //unsafe
                    //{
                    //    char* str = value;

                        fixed (char* str = value)//((char*)value))
                        {
                            int len;
                            char* chPtr = str;
                            fixed (byte* numRef = this._largeByteBuffer)
                            {
                            len = this.m_encoder.GetBytes(chPtr + num2, charCount, numRef, 0x100, charCount == i);
                                //str = null;
                            }
                            this.m_stream.Write(this._largeByteBuffer, 0, len);
                            _bytesWriten += len;
                            num2 += charCount;
                        }
                    //}
                }
            }
        }
        
 
         public virtual void Write(ushort value)
        {
            this.m_buffer[0] = (byte)value;
            this.m_buffer[1] = (byte)(value >> 8);
            this.m_stream.Write(this.m_buffer, 0, 2);
            _bytesWriten += 2;
        }

        public virtual void Write(uint value)
        {
            this.m_buffer[0] = (byte)value;
            this.m_buffer[1] = (byte)(value >> 8);
            this.m_buffer[2] = (byte)(value >> 0x10);
            this.m_buffer[3] = (byte)(value >> 0x18);
            this.m_stream.Write(this.m_buffer, 0, 4);
            _bytesWriten += 4;
        }

        public virtual void Write(ulong value)
        {
            this.m_buffer[0] = (byte)value;
            this.m_buffer[1] = (byte)(value >> 8);
            this.m_buffer[2] = (byte)(value >> 0x10);
            this.m_buffer[3] = (byte)(value >> 0x18);
            this.m_buffer[4] = (byte)(value >> 0x20);
            this.m_buffer[5] = (byte)(value >> 40);
            this.m_buffer[6] = (byte)(value >> 0x30);
            this.m_buffer[7] = (byte)(value >> 0x38);
            this.m_stream.Write(this.m_buffer, 0, 8);
            _bytesWriten += 8;
        }

        public virtual void Write(byte[] buffer, int index, int count)
        {
            this.m_stream.Write(buffer, index, count);
            _bytesWriten += count;
        }

        public virtual void Write(char[] chars, int index, int count)
        {
            byte[] buffer = this.m_encoding.GetBytes(chars, index, count);
            this.m_stream.Write(buffer, 0, buffer.Length);
            _bytesWriten += buffer.Length;
        }

        protected void Write7BitEncodedInt(int value)
        {
            uint num = (uint)value;
            while (num >= 0x80)
            {
                this.Write((byte)(num | 0x80));
                num = num >> 7;
            }
            this.Write((byte)num);
        }

        #endregion

        #region reader

        protected virtual void FillBuffer(int numBytes)
        {
            if ((this.m_buffer != null) && ((numBytes < 0) || (numBytes > this.m_buffer.Length)))
            {
                throw new ArgumentOutOfRangeException("numBytes", "Argument Out Of Range Binary Fill Buffer");
            }
            int offset = 0;
            int num2 = 0;
            if (this.m_stream == null)
            {
                IoErrors.FileNotOpen();
            }
            if (numBytes == 1)
            {
                num2 = this.m_stream.ReadByte();
                if (num2 == -1)
                {
                    IoErrors.EndOfFile();
                }
                this.m_buffer[0] = (byte)num2;
            }
            else
            {
                do
                {
                    num2 = this.m_stream.Read(this.m_buffer, offset, numBytes - offset);
                    if (num2 == 0)
                    {
                        IoErrors.EndOfFile();
                    }
                    offset += num2;
                }
                while (offset < numBytes);
            }
        }

        

        [SecuritySafeCritical]
        private unsafe int InternalReadChars(char[] buffer, int index, int count)
        {
           
            int num = 0;
            int charCount = count;
            if (this.m_charBytes == null)
            {
                this.m_charBytes = new byte[0x80];
            }


            bool decoderHasState = NetDecoder.IsDecoderHasState(m_decoder, m_encoding);
          
            while (charCount > 0)
            {
                int num3 = 0;
                num = charCount;
                

                if (decoderHasState && num > 1)
                {
                    num--;
                }
                if (this.m_2BytesPerChar)
                {
                    num = num << 1;
                }
                if (num > 0x80)
                {
                    num = 0x80;
                }
                int position = 0;
                byte[] charBytes = null;
                if (this.m_isNetStream)
                {
                    NetStream stream = this.m_stream as NetStream;
                    position = stream.InternalGetPosition();
                    num = stream.InternalEmulateRead(num);
                    charBytes = stream.InternalGetBuffer();
                }
                else
                {
                    num = this.m_stream.Read(this.m_charBytes, 0, num);
                    charBytes = this.m_charBytes;
                }
                if (num == 0)
                {
                    return (count - charCount);
                }
                fixed (byte* numRef = charBytes)
                {
                    fixed (char* chRef = buffer)
                    {
                        num3 = this.m_decoder.GetChars(numRef + position, num, chRef + index, charCount, false);
                    }
                }
                charCount -= num3;
                index += num3;
            }
            return (count - charCount);
        }

        private int InternalReadOneChar()
        {
            long position;
            int num = 0;
            int byteCount = 0;
            position = position = 0L;
            if (this.m_stream.CanSeek)
            {
                position = this.m_stream.Position;
            }
            if (this.m_charBytes == null)
            {
                this.m_charBytes = new byte[0x80];
            }
            if (this.m_singleChar == null)
            {
                this.m_singleChar = new char[1];
            }
            while (num == 0)
            {
                byteCount = this.m_2BytesPerChar ? 2 : 1;
                int num4 = this.m_stream.ReadByte();
                this.m_charBytes[0] = (byte)num4;
                if (num4 == -1)
                {
                    byteCount = 0;
                }
                if (byteCount == 2)
                {
                    num4 = this.m_stream.ReadByte();
                    this.m_charBytes[1] = (byte)num4;
                    if (num4 == -1)
                    {
                        byteCount = 1;
                    }
                }
                if (byteCount == 0)
                {
                    return -1;
                }
                try
                {
                    num = this.m_decoder.GetChars(this.m_charBytes, 0, byteCount, this.m_singleChar, 0);
                    continue;
                }
                catch
                {
                    if (this.m_stream.CanSeek)
                    {
                        this.m_stream.Seek(position - this.m_stream.Position, SeekOrigin.Current);
                    }
                    throw;
                }
            }
            if (num == 0)
            {
                return -1;
            }
            return this.m_singleChar[0];
        }

        public virtual int PeekChar()
        {
            if (this.m_stream == null)
            {
                IoErrors.FileNotOpen();
            }
            if (!this.m_stream.CanSeek)
            {
                return -1;
            }
            long position = this.m_stream.Position;
            int num2 = this.Read();
            this.m_stream.Position = position;
            return num2;
        }

        public virtual int Read()
        {
            if (this.m_stream == null)
            {
                IoErrors.FileNotOpen();
            }
            return this.InternalReadOneChar();
        }

        public virtual int Read(byte[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer", "ArgumentNull_Buffer");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", "Argument Out Of Range Need Non Negative Num");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "Argument Out Of Range Need Non Negative Num");
            }
            if ((buffer.Length - index) < count)
            {
                throw new ArgumentException("Argument_InvalidOffLen");
            }
            if (this.m_stream == null)
            {
                IoErrors.FileNotOpen();
            }
            return this.m_stream.Read(buffer, index, count);
        }

        protected internal int Read7BitEncodedInt()
        {
            byte num3;
            int num = 0;
            int num2 = 0;
            do
            {
                if (num2 == 0x23)
                {
                    throw new FormatException("Format Bad 7Bit Int32");
                }
                num3 = this.ReadByte();
                num |= (num3 & 0x7f) << num2;
                num2 += 7;
            }
            while ((num3 & 0x80) != 0);
            return num;
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public virtual bool ReadBoolean()
        {
            this.FillBuffer(1);
            return (this.m_buffer[0] != 0);
        }

        public virtual byte ReadByte()
        {
            if (this.m_stream == null)
            {
                IoErrors.FileNotOpen();
            }
            int num = this.m_stream.ReadByte();
            if (num == -1)
            {
                IoErrors.EndOfFile();
            }
            return (byte)num;
        }

        public virtual byte PeekByte()
        {
            if (this.m_stream == null)
            {
                IoErrors.FileNotOpen();
            }
            if (!this.m_stream.CanSeek)
            {
                IoErrors.SeekNotSupported();
            }

            int position = (int)m_stream.Position;
            int num = this.m_stream.ReadByte();
            if (num == -1)
            {
                IoErrors.EndOfFile();
            }
            m_stream.Seek(position-1, SeekOrigin.Current);//.Position--;
            return (byte)num;
        }

        [SecuritySafeCritical]
        public virtual byte[] ReadBytes(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "Argument Out Of Range Need Non Negative Num");
            }
            if (this.m_stream == null)
            {
                IoErrors.FileNotOpen();
            }
            byte[] buffer = new byte[count];
            int offset = 0;
            do
            {
                int num2 = this.m_stream.Read(buffer, offset, count);
                if (num2 == 0)
                {
                    break;
                }
                offset += num2;
                count -= num2;
            }
            while (count > 0);
            if (offset != buffer.Length)
            {
                byte[] dst = new byte[offset];
                StreamBuffer.StreamBlockCopy(buffer, 0, dst, 0, offset);
                buffer = dst;
            }
            return buffer;
        }

        protected virtual char ReadChar()
        {
            int num = this.Read();
            if (num == -1)
            {
                IoErrors.EndOfFile();
            }
            return (char)num;
        }

        [SecuritySafeCritical]
        public virtual char[] ReadChars(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "Argument Out Of Range Need Non Negative Num");
            }
            if (this.m_stream == null)
            {
                IoErrors.FileNotOpen();
            }
            char[] buffer = new char[count];
            int num = this.InternalReadChars(buffer, 0, count);
            if (num != count)
            {
                char[] dst = new char[num];
                StreamBuffer.StreamBlockCopy(buffer, 0, dst, 0, 2 * num);
                buffer = dst;
            }
            return buffer;
        }

        [SecuritySafeCritical]
        public virtual decimal ReadDecimal()
        {
            this.FillBuffer(0x10);
            int[] bits = new int[4];
            bits[0] = ((this.m_buffer[0] | (this.m_buffer[1] << 8)) | (this.m_buffer[2] << 0x10)) | (this.m_buffer[3] << 0x18); //lo
            bits[1] = ((this.m_buffer[4] | (this.m_buffer[5] << 8)) | (this.m_buffer[6] << 0x10)) | (this.m_buffer[7] << 0x18); //mid
            bits[2] = ((this.m_buffer[8] | (this.m_buffer[9] << 8)) | (this.m_buffer[10] << 0x10)) | (this.m_buffer[11] << 0x18); //hi
            bits[3] = ((this.m_buffer[12] | (this.m_buffer[13] << 8)) | (this.m_buffer[14] << 0x10)) | (this.m_buffer[15] << 0x18); //flags
            return new decimal(bits);
        }

        [SecuritySafeCritical]
        public virtual unsafe double ReadDouble()
        {
            this.FillBuffer(8);
            uint num = (uint)(((this.m_buffer[0] | (this.m_buffer[1] << 8)) | (this.m_buffer[2] << 0x10)) | (this.m_buffer[3] << 0x18));
            uint num2 = (uint)(((this.m_buffer[4] | (this.m_buffer[5] << 8)) | (this.m_buffer[6] << 0x10)) | (this.m_buffer[7] << 0x18));
            ulong num3 = (num2 << 0x20) | num;
            return *(((double*)&num3));
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public virtual short ReadInt16()
        {
            this.FillBuffer(2);
            return (short)(this.m_buffer[0] | (this.m_buffer[1] << 8));
        }

        public virtual int ReadInt32()
        {
            if (this.m_isNetStream)
            {
                if (this.m_stream == null)
                {
                    IoErrors.FileNotOpen();
                }
                NetStream stream = this.m_stream as NetStream;
                return stream.InternalReadInt32();
            }
            this.FillBuffer(4);
            return (((this.m_buffer[0] | (this.m_buffer[1] << 8)) | (this.m_buffer[2] << 0x10)) | (this.m_buffer[3] << 0x18));
        }

        [SecuritySafeCritical]
        public unsafe virtual long ReadInt64()
        {
            this.FillBuffer(8);
            fixed (byte* numRef = &(this.m_buffer[0]))
            {
                return *(((long*)numRef));
            }

            //uint num = (uint)(((this.m_buffer[0] | (this.m_buffer[1] << 8)) | (this.m_buffer[2] << 0x10)) | (this.m_buffer[3] << 0x18));
            //uint num2 = (uint)(((this.m_buffer[4] | (this.m_buffer[5] << 8)) | (this.m_buffer[6] << 0x10)) | (this.m_buffer[7] << 0x18));
            //return (long)((num2 << 0x20) | num);
        }

        public virtual sbyte ReadSByte()
        {
            this.FillBuffer(1);
            return (sbyte)this.m_buffer[0];
        }

        [SecuritySafeCritical]
        public virtual unsafe float ReadSingle()
        {
            this.FillBuffer(4);
            uint num = (uint)(((this.m_buffer[0] | (this.m_buffer[1] << 8)) | (this.m_buffer[2] << 0x10)) | (this.m_buffer[3] << 0x18));
            return *(((float*)&num));
        }

        [SecuritySafeCritical]
        protected virtual string ReadString(bool allowNull)
        {
           
            if (this.m_stream == null)
            {
                if (allowNull)
                {
                    return null;
                }
                IoErrors.FileNotOpen();
            }
            if (allowNull)
            {
                SerialType t = (SerialType)ReadByte();
                if (t != SerialType.stringType)
                    return null;
            }

            int num = 0;
            int capacity = this.Read7BitEncodedInt();
            if (capacity < 0)
            {
                throw new IOException("IO Invalid String Len: " + capacity.ToString());
            }
            if (capacity == 0)
            {
                return string.Empty;
            }
            if (this.m_charBytes == null)
            {
                this.m_charBytes = new byte[0x80];
            }
            if (this.m_charBuffer == null)
            {
                this.m_charBuffer = new char[this.m_maxCharsSize];
            }
            StringBuilder builder = null;
            do
            {
                int count = ((capacity - num) > 0x80) ? 0x80 : (capacity - num);
                int byteCount = this.m_stream.Read(this.m_charBytes, 0, count);
                if (byteCount == 0)
                {
                    IoErrors.EndOfFile();
                }
                int length = this.m_decoder.GetChars(this.m_charBytes, 0, byteCount, this.m_charBuffer, 0);
                if ((num == 0) && (byteCount == capacity))
                {
                    return new string(this.m_charBuffer, 0, length);
                }
                if (builder == null)
                {
                    builder = new StringBuilder(capacity);
                }
                builder.Append(this.m_charBuffer, 0, length);
                num += byteCount;
            }
            while (num < capacity);
            return builder.ToString();
        }
        

       public virtual ushort ReadUInt16()
        {
            this.FillBuffer(2);
            return (ushort)(this.m_buffer[0] | (this.m_buffer[1] << 8));
        }

        public virtual uint ReadUInt32()
        {
            this.FillBuffer(4);
            return (uint)(((this.m_buffer[0] | (this.m_buffer[1] << 8)) | (this.m_buffer[2] << 0x10)) | (this.m_buffer[3] << 0x18));
        }

        public virtual ulong ReadUInt64()
        {
            return (ulong)ReadInt64();
        }


        #endregion

        #region read/write string
        
        sealed class StringHelper
        {
            public StringHelper()
            {
                this.Encoding = new UTF8Encoding(false, true);
            }

            public const int BYTEBUFFERLEN = 256;
            public const int CHARBUFFERLEN = 128;

            Encoder m_encoder;
            Decoder m_decoder;

            byte[] m_byteBuffer;
            char[] m_charBuffer;

            public UTF8Encoding Encoding { get; private set; }
            public Encoder Encoder { get { if (m_encoder == null) m_encoder = this.Encoding.GetEncoder(); return m_encoder; } }
            public Decoder Decoder { get { if (m_decoder == null) m_decoder = this.Encoding.GetDecoder(); return m_decoder; } }

            public byte[] ByteBuffer { get { if (m_byteBuffer == null) m_byteBuffer = new byte[BYTEBUFFERLEN]; return m_byteBuffer; } }
            public char[] CharBuffer { get { if (m_charBuffer == null) m_charBuffer = new char[CHARBUFFERLEN]; return m_charBuffer; } }
        }

        #endregion

        #region encoder
      
        #endregion
    }

    #region NetDecoder

    [Serializable]
    internal class NetDecoder : Decoder, ISerializable, IObjectReference
    {
        public static Decoder GetDecoder(Encoding encoding)
        {
            if (encoding is UTF8Encoding)
                return ((UTF8Encoding)encoding).GetDecoder();
            return new NetDecoder(encoding);
        }

        public static bool IsDecoderHasState(Decoder decoder, Encoding encoding)
        {
            if (decoder == null || encoding == null)
                return false;
            if (encoding is UTF8Encoding)
                return true;
            if (decoder is NetDecoder)
            {
                NetDecoder ndecoder = (NetDecoder)decoder;
                if (((ndecoder.InitializedEncoding && ndecoder.Encoding is UTF8Encoding) && ndecoder.HasState))
                    return true;
            }

            return false;
        }

        private Encoding m_encoding;
        [NonSerialized]
        private bool m_hasInitializedEncoding;
        internal int bits;

        public bool InitializedEncoding
        {
            get { return m_hasInitializedEncoding; }
        }

        public System.Text.Encoding Encoding
        {
            get
            {
                return this.m_encoding;
            }
        }

        internal bool HasState
        {
            get
            {
                return (this.bits != 0);
            }
        }

        public override void Reset()
        {
            this.bits = 0;
            if (base.FallbackBuffer != null)
            {
                base.FallbackBuffer.Reset();
            }
        }

        public NetDecoder(Encoding encoding)
        {
            this.m_encoding = encoding;
            this.m_hasInitializedEncoding = encoding != null;// true;
        }

        internal NetDecoder(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            this.m_encoding = (Encoding)info.GetValue("encoding", typeof(Encoding));
            try
            {
                this.bits = (int)info.GetValue("wbits", typeof(int));
                base.Fallback = (DecoderFallback)info.GetValue("m_fallback", typeof(DecoderFallback));
            }
            catch (SerializationException)
            {
                base.Fallback = null;
                this.bits = 0;
            }
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return this.GetCharCount(bytes, index, count, false);
        }

        [SecurityCritical]
        public override unsafe int GetCharCount(byte* bytes, int count, bool flush)
        {
            return this.m_encoding.GetCharCount(bytes, count);
        }

        public override int GetCharCount(byte[] bytes, int index, int count, bool flush)
        {
            return this.m_encoding.GetCharCount(bytes, index, count);
        }

        [SecurityCritical]
        public override unsafe int GetChars(byte* bytes, int byteCount, char* chars, int charCount, bool flush)
        {
            return this.m_encoding.GetChars(bytes, byteCount, chars, charCount);
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            return this.GetChars(bytes, byteIndex, byteCount, chars, charIndex, false);
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex, bool flush)
        {
            return this.m_encoding.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
        }

        [SecurityCritical]
        public object GetRealObject(StreamingContext context)
        {
            if (this.m_hasInitializedEncoding)
            {
                return this;
            }
            System.Text.Decoder decoder = this.m_encoding.GetDecoder();
            if (base.Fallback != null)
            {
                decoder.Fallback = base.Fallback;
            }
            return decoder;
        }

       
        [SecurityCritical]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
 
            info.AddValue("encoding", this.m_encoding);
            info.AddValue("wbits", this.bits);
            info.SetType(typeof(NetDecoder));
        }
    }

    #endregion

    #region NetEncoder

    [Serializable]
    internal class NetEncoder : Encoder, ISerializable, IObjectReference
    {

        public static Encoder GetEncoder(Encoding encoding)
        {
            if (encoding is UTF8Encoding)
                return ((UTF8Encoding)encoding).GetEncoder();
            return new NetEncoder(encoding);
        }

        [NonSerialized]
        internal char charLeftOver;
        private Encoding m_encoding;
        [NonSerialized]
        private bool m_hasInitializedEncoding;

        public bool InitializedEncoding
        {
            get { return m_hasInitializedEncoding; }
        }

        internal bool HasState
        {
            get
            {
                return (this.charLeftOver != '\0');
            }
        }

        public System.Text.Encoding Encoding
        {
            get
            {
                return this.m_encoding;
            }
        }

        public NetEncoder(Encoding encoding)
        {
            this.m_encoding = encoding;
            this.m_hasInitializedEncoding = encoding != null;//true;
        }

        internal NetEncoder(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            this.m_encoding = (Encoding)info.GetValue("encoding", typeof(Encoding));
            try
            {
                base.Fallback = (EncoderFallback)info.GetValue("m_fallback", typeof(EncoderFallback));
                this.charLeftOver = (char)info.GetValue("charLeftOver", typeof(char));
            }
            catch (SerializationException)
            {
            }
        }


        [SecurityCritical]
        public override unsafe int GetByteCount(char* chars, int count, bool flush)
        {
            return this.m_encoding.GetByteCount(chars, count);
        }

        public override int GetByteCount(char[] chars, int index, int count, bool flush)
        {
            return this.m_encoding.GetByteCount(chars, index, count);
        }

        [SecurityCritical]
        public override unsafe int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, bool flush)
        {
            return this.m_encoding.GetBytes(chars, charCount, bytes, byteCount);
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, bool flush)
        {
            return this.m_encoding.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
        }

        [SecurityCritical]
        public object GetRealObject(StreamingContext context)
        {
            if (this.m_hasInitializedEncoding)
            {
                return this;
            }
            System.Text.Encoder encoder = this.m_encoding.GetEncoder();
            if (base.Fallback != null)
            {
                encoder.Fallback = base.Fallback;
            }
            
            return encoder;
        }

        [SecurityCritical]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            
            info.AddValue("encoding", this.m_encoding);
            info.AddValue("charLeftOver", this.charLeftOver);
            info.SetType(typeof(NetEncoder));
        }
    }
       #endregion
}
