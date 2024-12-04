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
#pragma warning disable  CS1591

namespace Nistec.Serialization
{
  
    /// <summary>
    /// Represent a binary serializer/deserializer , using <see cref="BinaryStreamer"/>.
    /// </summary>
    public class BinarySerializer
    {
 
        #region NetStream Converter

        public static NetStream ConvertToStream(object obj, Formatters formatter = Formatters.BinarySerializer)
        {
            NetStream ns = new NetStream();
            if (formatter == Formatters.BinarySerializer)
            {
                BinarySerializer f = new BinarySerializer();
                f.Serialize(ns,obj);
            }
            else //if (formatter == Formatters.BinaryFormatter)
            {
                BinaryFormatter f = new BinaryFormatter();
                f.Serialize(ns, obj);
            }

            return ns;
        }

        public static object ConvertFromStream(NetStream stream, Formatters formatter = Formatters.BinarySerializer)
        {
            if (formatter == Formatters.BinarySerializer)
            {
                BinarySerializer f = new BinarySerializer();
                return f.Deserialize(stream);
            }
            else //if (formatter == Formatters.BinaryFormatter)
            {
                BinaryFormatter f = new BinaryFormatter();
                return f.Deserialize(stream);
            }
        }
        
        #endregion

        #region Serialize/Deserialize

        bool m_useStreamerForUnknownType;
        /// <summary>
        /// Initialize a new instance of BinarySerializer.
        /// </summary>
        public BinarySerializer()
        {
            m_useStreamerForUnknownType = false;
        }
        /// <summary>
        /// Initialize a new instance of BinarySerializer.
        /// </summary>
        /// <param name="useStreamerForUnknownType"></param>
        public BinarySerializer(bool useStreamerForUnknownType)
        {
            m_useStreamerForUnknownType = useStreamerForUnknownType;
        }

        public NetStream Serialize(object value, bool enableException = false)
        {

            try
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Serialize.value");
                }
                NetStream stream = new NetStream();
                BinaryStreamer streamer = new BinaryStreamer(stream);
                streamer.m_UseStreamerForUnknownType = m_useStreamerForUnknownType;
                streamer.WriteAny(value);
                streamer.Flush();
                return stream;
            }
            catch (SerializationException e)
            {
                if (enableException)
                    throw e;
            }
            catch (Exception ex)
            {
                if (enableException)
                    throw ex;
            }

            return null;
        }


        /// <summary>
        /// Serialize object.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value"></param>
        /// <param name="enableException"></param>
        public void Serialize(Stream stream, object value, bool enableException = false)
        {
            try
            {
                if (stream == null)
                {
                    throw new ArgumentNullException("Serialize.stream");
                }
                if (value == null)
                {
                    throw new ArgumentNullException("Serialize.value");
                }
                BinaryStreamer streamer = new BinaryStreamer(stream);
                streamer.m_UseStreamerForUnknownType = m_useStreamerForUnknownType;
                streamer.WriteAny(value);
                streamer.Flush();
            }
            catch (Exception ex)
            {
                if (enableException)
                    throw ex;
            }
        }
        /// <summary>
        /// Serialize object.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value"></param>
        /// <param name="baseType"></param>
        /// <param name="enableException"></param>
        public void Serialize(Stream stream, object value, Type baseType, bool enableException = false)
        {
            try
            {
                if (stream == null)
                {
                    throw new ArgumentNullException("Serialize.stream");
                }
                if (value == null)
                {
                    throw new ArgumentNullException("Serialize.value");
                }
                BinaryStreamer streamer = new BinaryStreamer(stream);
                streamer.m_UseStreamerForUnknownType = m_useStreamerForUnknownType;
                streamer.WriteAny(value, baseType);
                streamer.Flush();
            }
            catch (Exception ex)
            {
                if (enableException)
                    throw ex;
            }
        }

       /// <summary>
        /// Deserialize object from stream.
       /// </summary>
       /// <param name="stream"></param>
       /// <param name="enableException"></param>
       /// <returns></returns>
        public object Deserialize(Stream stream, bool enableException = false)
        {
            try
            {
                if (stream == null)
                {
                    throw new ArgumentNullException("Deserialize.stream");
                }
                BinaryStreamer streamer = new BinaryStreamer(stream);
                return streamer.ReadAny();
            }
            catch (Exception ex)
            {
                if (enableException)
                    throw ex;
                return null;
            }
        }

        /// <summary>
        /// Reads an object which was added to the buffer by Serialize.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="enableException"></param>
        /// <returns></returns>
        public T Deserialize<T>(Stream stream, bool enableException = false)
        {
            try
            {
                if (stream == null)
                {
                    throw new ArgumentNullException("Deserialize.stream");
                }
                else if (SerializeTools.IsStream(typeof(T)))
                {
                    return GenericTypes.Cast<T>(stream, true);
                }
                BinaryStreamer streamer = new BinaryStreamer(stream);
                return GenericTypes.Cast<T>(streamer.ReadAny(), true);
            }
            catch (Exception ex)
            {
                if (enableException)
                    throw ex;
                return default(T);
            }
        }

        /// <summary>
        /// Reads an object which was added to the buffer by Serialize using <see cref="SerialContextType"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="contextType"></param>
        /// <param name="enableException"></param>
        /// <returns></returns>
        public T Deserialize<T>(Stream stream, SerialContextType contextType, bool enableException = false)
        {
            try
            {
                if (stream == null)
                {
                    throw new ArgumentNullException("Deserialize.stream");
                }
                else if (SerializeTools.IsStream(typeof(T)))
                {
                    return GenericTypes.Cast<T>(stream, true);
                }
                BinaryStreamer streamer = new BinaryStreamer(stream);

                return streamer.ReadValue<T>(contextType);
            }
            catch (Exception ex)
            {
                if (enableException)
                    throw ex;
                return default(T);
            }
        }


        #endregion

        #region Bin xml Deserialize

        public static byte[] SerializeXmlToBytes(object body)
        {
            if (body == null)
                return null;
            using (NetStream ms = new NetStream())
            {
                SerializeXmlElement(ms, body);
                return ms.ToArray();
            }
        }

        public static T DeserializeXmlFromBytes<T>(byte[] bytes)
        {
            if (bytes == null)
                return default(T);
            using (NetStream ms = new NetStream())
            {
              return  DeserializeXmlElement<T>(ms);
           }
        }

        public static void SerializeXmlElement(Stream stream, object body)
        {
            SurrogateSelector surrogateSelector = new SurrogateSelector();
            surrogateSelector.AddSurrogate(typeof(System.Xml.XmlElement), new
            StreamingContext(StreamingContextStates.All), new
            XmlElementSurrogate());

            // create a new BinaryFormatter instance 
            BinaryFormatter formatter = new BinaryFormatter();
            // serialize the class into the MemoryStream 
            formatter.SurrogateSelector = surrogateSelector;
            formatter.Serialize(stream, body);
        }

        public static object DeserializeXmlElement(Stream stream)
        {
            object result = null;

            if (stream == null)
                return result;

            SurrogateSelector surrogateSelector = new SurrogateSelector();
            surrogateSelector.AddSurrogate(typeof(System.Xml.XmlElement), new
            StreamingContext(StreamingContextStates.All), new
            XmlElementSurrogate());

            BinaryFormatter formatter = new BinaryFormatter();
            formatter.SurrogateSelector = surrogateSelector;
            result = formatter.Deserialize(stream);
            return result;
        }

        public static T DeserializeXmlElement<T>(Stream stream)
        {
            return (T)DeserializeXmlElement(stream);
        }

        public class XmlElementSurrogate : ISerializationSurrogate
        {

            void ISerializationSurrogate.GetObjectData(object obj,
            SerializationInfo info, StreamingContext context)
            {
                XmlElement element = (XmlElement)obj;
                info.AddValue("data", element.OuterXml);
            }

            object ISerializationSurrogate.SetObjectData(object obj,
            SerializationInfo info, StreamingContext context, ISurrogateSelector
            selector)
            {
                string data = info.GetString("data");
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(data);
                obj = doc.DocumentElement;
                return doc.DocumentElement;
            }
        }

        #endregion

        #region static

        /// <summary>
        /// Serialize object to <see cref="NetStream"/> stream.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="enableException"></param>
        /// <returns></returns>
        public static NetStream SerializeToStream(object value,bool enableException = false)
        {
            try
            {
                NetStream ns = new NetStream();
                BinarySerializer streamer = new BinarySerializer();
                streamer.Serialize(ns, value);
                return ns;
            }
            catch (Exception ex)
            {
                if (enableException)
                    throw ex;
                return null;
            }
        }
        /// <summary>
        /// Deserialize object from <see cref="NetStream"/> stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="enableException"></param>
        /// <returns></returns>
        public static T DeserializeFromStream<T>(NetStream stream, bool enableException = false)
        {
            try
            {
                BinarySerializer streamer = new BinarySerializer();
                return streamer.Deserialize<T>(stream);
            }
            catch (Exception ex)
            {
                if (enableException)
                    throw ex;
                return default(T);
            }
        }

        /// <summary>
        /// Reads an object which was added to the buffer by Serialize using <see cref="SerialContextType"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="contextType"></param>
        /// <param name="enableException"></param>
        /// <returns></returns>
        public static T DeserializeFromStream<T>(Stream stream, SerialContextType contextType, bool enableException = false)
        {
            try
            {
                if (stream == null)
                {
                    throw new ArgumentNullException("Deserialize.stream");
                }
                else if (SerializeTools.IsStream(typeof(T)))
                {
                    return GenericTypes.Cast<T>(stream, true);
                }
                BinaryStreamer streamer = new BinaryStreamer(stream);

                return streamer.ReadValue<T>(contextType);
            }
            catch (Exception ex)
            {
                if (enableException)
                    throw ex;
                return default(T);
            }
        }

        public static NetStream PrimitveToStream<T>(T value)
        {
            NetStream ns = new NetStream();
            BinaryStreamer streamer = new BinaryStreamer(ns);
            streamer.WritePrimitive<T>(value);
            streamer.Flush();
            return ns;
        }

        #endregion

        #region Formatters Serialization

        public static int SizeOf(object o)
        {
            try
            {
                return BinarySerializer.ConvertToStream(o).iLength;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        public static byte[] SerializeToBytes(object o, bool enableException = false)
        {
            if (o == null)
                return null;

            using (NetStream ms = new NetStream())
            {
                try
                {

                    BinarySerializer f = new BinarySerializer();
                    f.Serialize(ms, o);
                    return ms.ToArray();

                }
                catch (SerializationException e)
                {
                    if (enableException)
                        throw e;
                }
                catch (Exception ex)
                {
                    if (enableException)
                        throw ex;
                }
            }
            return null;
        }

        public static object Deserialize(byte[] buf, bool enableException = false)
        {

            if (buf == null)
                return null;

            using (NetStream ms = new NetStream())
            {
                try
                {
                    ms.Write(buf, 0, buf.Length);
                    ms.Seek(0, 0);
                    BinarySerializer f = new BinarySerializer();
                    return f.Deserialize(ms);

                }
                catch (SerializationException e)
                {
                    if (enableException)
                        throw e;
                }
                catch (Exception ex)
                {
                    if (enableException)
                        throw ex;
                }
            }

            return null;
        }

        public static T Deserialize<T>(byte[] buf, bool enableException = false)
        {

            if (buf == null)
                return default(T);
            Type type = typeof(T);

            using (NetStream ms = new NetStream())
            {
                try
                {
                    ms.Write(buf, 0, buf.Length);
                    ms.Seek(0, 0);
                    BinarySerializer f = new BinarySerializer();
                    object o = f.Deserialize(ms);
                    return GenericTypes.Cast<T>(o, true);

                }
                catch (SerializationException e)
                {
                    if (enableException)
                        throw e;
                }
                catch (Exception ex)
                {
                    if (enableException)
                        throw ex;
                }
            }

            return default(T);
        }
  
        #endregion

        #region base64 Serialization

        /// <summary>
        /// SerializeToBase64
        /// </summary>
        /// <param name="body"></param>
        /// <param name="enableException"></param>
        /// <returns></returns>
        public static string SerializeToBase64(object body, bool enableException = false)
        {
            if (body == null)
                return null;
            try
            {
                byte[] byteArray = SerializeToBytes(body);
                if (byteArray == null)
                    return null;
                return Convert.ToBase64String(byteArray, 0, byteArray.Length);
            }
            catch (SerializationException e)
            {
                if (enableException)
                {
                    throw e;
                }
            }
            catch (Exception ex)
            {
                if (enableException)
                {
                    throw ex;
                }
            }
            return null;
        }
        public static string SerializeToBase64(object body, ref int size)
        {
            size = 0;
            if (body == null)
                return null;
            try
            {
                byte[] byteArray = SerializeToBytes(body);
                if (byteArray == null)
                    return null;
                size = byteArray.Length;
                return Convert.ToBase64String(byteArray, 0, byteArray.Length);
            }
            catch (SerializationException e)
            {
                throw e;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static string ToBase64(byte[] bytes)
        {
            if (bytes == null)
                return null;
            return Convert.ToBase64String(bytes, 0, bytes.Length);
        }

        public static byte[] FromBase64(string base64String)
        {
            if (base64String == null)
                return null;

            return Convert.FromBase64String(base64String);
        }

        /// <summary>
        /// DeserializeFromBase64
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="base64String"></param>
        /// <param name="enableException"></param>
        /// <returns></returns>
        public static T DeserializeFromBase64<T>(string base64String, bool enableException = false)
        {

            if (base64String == null)
                return default(T);

            try
            {
                byte[] buf = Convert.FromBase64String(base64String);

                return Deserialize<T>(buf);
            }
            catch (SerializationException e)
            {
                if (enableException)
                {
                    throw e;
                }
            }
            catch (Exception ex)
            {
                if (enableException)
                {
                    throw ex;
                }
            }

            return default(T);
        }


        /// <summary>
        /// DeserializeFromBase64
        /// </summary>
        /// <param name="base64String"></param>
        /// <param name="enableException"></param>
        /// <returns></returns>
        public static object DeserializeFromBase64(string base64String, bool enableException = false)
        {
            if (base64String == null)
                return null;

            try
            {
                byte[] buf = Convert.FromBase64String(base64String);

                return Deserialize(buf);
            }
            catch (SerializationException e)
            {
                if (enableException)
                {
                    throw e;
                }
            }
            catch (Exception ex)
            {
                if (enableException)
                {
                    throw ex;
                }
            }
            return null;
        }

        #endregion

        #region file Serialization

        /// <summary>
        /// SerializeToBase64
        /// </summary>
        /// <param name="body"></param>
        /// <param name="filename"></param>
        /// <param name="enableException"></param>
        /// <returns></returns>
        public static bool SerializeToFile(object body, string filename, bool enableException = false)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                try
                {
                    BinarySerializer formatter = new BinarySerializer();
                    formatter.Serialize(fs, body);
                    return true;
                }
                catch (SerializationException e)
                {
                    if (enableException)
                    {
                        throw e;
                    }
                     return false;
                }
                catch (Exception ex)
                {
                    if (enableException)
                    {
                        throw ex;
                    }
                    return false;
                }
            }
        }

        /// <summary>
        /// DeserializeFromBytes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename"></param>
        /// <param name="enableException"></param>
        /// <returns></returns>
        public static T DeserializeFromFile<T>(string filename, bool enableException = false)
        {
            T result = default(T);

            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    BinarySerializer formatter = new BinarySerializer();
                    result = (T)formatter.Deserialize(fs);
                }
                catch (SerializationException e)
                {
                    if (enableException)
                    {
                        throw e;
                    }
                }
                catch (Exception ex)
                {
                    if (enableException)
                    {
                        throw ex;
                    }
                }
            }

            return result;
        }
        #endregion

    }
  
}
