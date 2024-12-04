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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Web.Script.Serialization;

#pragma warning disable  CS1591

namespace Nistec.Serialization
{
    

    /// <summary>
    /// Class for serialization utilities
    /// </summary>
    public static class NetSerializer
    {

        #region Formatters Serialization
        
        public static byte[] SerializeBinary(object o, Formatters formatter, bool enableException = false)
        {
            if (o == null)
                return null;

            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    if (formatter == Formatters.BinaryFormatter)
                    {
                        BinaryFormatter f = new BinaryFormatter();
                        f.Serialize(ms, o);
                        return ms.ToArray();
                    }
                    else if (formatter == Formatters.BinarySerializer)
                    {
                        BinarySerializer f = new BinarySerializer();
                        f.Serialize(ms,o);
                        return ms.ToArray();
                    }
                }
                catch (SerializationException e)
                {
                    if (enableException)
                        throw e;
                    string s = e.Message;
                }
            }
            return null;
        }
        
        public static object Deserialize(byte[] buf, Formatters formatter, Type type, bool enableException = false)
        {
 
            if (buf == null)
                return null;

            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    ms.Write(buf, 0, buf.Length);
                    ms.Seek(0, 0);

                    if (formatter == Formatters.BinaryFormatter)
                    {
                        BinaryFormatter f = new BinaryFormatter();
                        return f.Deserialize(ms);
                    }
                    else if (formatter == Formatters.BinarySerializer)
                    {
                        BinarySerializer f = new BinarySerializer();
                        return f.Deserialize(ms);
                    }
                }
                catch (SerializationException e)
                {
                    if (enableException)
                        throw e;
                    string s = e.Message;
                }
            }

            return null;
        }

        public static T Deserialize<T>(byte[] buf, Formatters formatter, bool enableException = false)
        {

            if (buf == null)
                return default(T);
            Type type = typeof(T);

            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    ms.Write(buf, 0, buf.Length);
                    ms.Seek(0, 0);

                    if (formatter == Formatters.BinaryFormatter)
                    {
                        BinaryFormatter f = new BinaryFormatter();
                        object o = f.Deserialize(ms);
                        return GenericTypes.Cast<T>(o);
                    }
                    else if (formatter == Formatters.BinarySerializer)
                    {
                        BinarySerializer f = new BinarySerializer();
                        object o = f.Deserialize(ms);
                        return GenericTypes.Cast<T>(o);
                    }
                }
                catch (SerializationException e)
                {
                    if (enableException)
                        throw e;
                    string s = e.Message;
                }
            }

            return default(T);
        }

        public static object Deserialize(Stream stream, Formatters formatter, Type type, bool enableException = false)
        {

            if (stream == null)
                return null;

            try
            {
                
                if (formatter == Formatters.BinaryFormatter)
                {
                    BinaryFormatter f = new BinaryFormatter();
                    return f.Deserialize(stream);
                }
                else if (formatter == Formatters.BinarySerializer)
                {
                    BinarySerializer f = new BinarySerializer();
                    return f.Deserialize(stream);
                }
            }
            catch (SerializationException e)
            {
                if (enableException)
                    throw e;
                string s = e.Message;
            }
            return null;
        }

        public static T Deserialize<T>(Stream stream, Formatters formatter, bool enableException = false)
        {

            if (stream == null)
                return default(T);
            Type type = typeof(T);

            try
            {
                if (formatter == Formatters.BinaryFormatter)
                {
                    BinaryFormatter f = new BinaryFormatter();
                    object o = f.Deserialize(stream);
                    return GenericTypes.Cast<T>(o);
                }
                else if (formatter == Formatters.BinarySerializer)
                {
                    BinarySerializer f = new BinarySerializer();
                    object o = f.Deserialize(stream);
                    return GenericTypes.Cast<T>(o);
                }
            }
            catch (SerializationException e)
            {
                if (enableException)
                    throw e;
                string s = e.Message;
            }
            return default(T);
        }

        #endregion

    
        #region Bin xml Deserialize

        public static T BinXmlDeserialize<T>(Stream p_Stream)
        {
            T result = default(T);

            if (p_Stream == null)
                return result;

            SurrogateSelector surrogateSelector = new SurrogateSelector();
            surrogateSelector.AddSurrogate(typeof(System.Xml.XmlElement), new
            StreamingContext(StreamingContextStates.All), new
            XmlElementSurrogate());

            BinaryFormatter formatter = new BinaryFormatter();
            formatter.SurrogateSelector = surrogateSelector;
            result = (T)formatter.Deserialize(p_Stream);
            return result;
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


        /// <summary>
        /// SerializeToBytes
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public static byte[] SerializeXmlToBytes(object body)
        {

            byte[] byteArray = null;
            // instantiate a MemoryStream and a new instance of our class          
            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    SurrogateSelector surrogateSelector = new SurrogateSelector();
                    surrogateSelector.AddSurrogate(typeof(System.Xml.XmlElement), new
                    StreamingContext(StreamingContextStates.All), new
                    XmlElementSurrogate());

                    // create a new BinaryFormatter instance 
                    BinaryFormatter formatter = new BinaryFormatter();
                    // serialize the class into the MemoryStream 
                    formatter.SurrogateSelector = surrogateSelector;
                    formatter.Serialize(ms, body);
                    byteArray = ms.ToArray();
                }
                catch (SerializationException e)
                {
                    string s = e.Message;
                    return null;
                }
            }
            return byteArray;
        }

     
        #endregion

        #region Bin Serialize/Deserialize

        public static object BinDeserialize(Stream stream)
        {
            if (stream == null)
                return null;
            BinaryFormatter f = new BinaryFormatter();
            object o = f.Deserialize(stream);
            return o;
        }

        public static T BinDeserialize<T>(Stream stream)
        {
            T result = default(T);

            if (stream == null)
                return result;

            BinaryFormatter f = new BinaryFormatter();
            result = (T)f.Deserialize(stream);
            return result;
        }

        public static void BinSerialize(Stream stream, object o)
        {
            if (stream == null)
                throw new ArgumentNullException("BinSerialize.stream");
            if (o == null)
                throw new ArgumentNullException("BinSerialize.o");

            BinaryFormatter f = new BinaryFormatter();
            f.Serialize(stream, o);
        }

        public static object BinDeserialize(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException("BinDeserialize.fileName");

            object tmp;
            using (FileStream l_Stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                tmp = BinDeserialize(l_Stream);
                l_Stream.Close();
            }
            return tmp;
        }

        public static void BinSerialize(string fileName, object o)
        {
            if (fileName == null)
                throw new ArgumentNullException("BinSerialize.fileName");
            if (o == null)
                throw new ArgumentNullException("BinSerialize.o");

            using (FileStream l_Stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                BinSerialize(l_Stream, o);
                l_Stream.Close();
            }
        }
        #endregion

        #region file Serialization

        /// <summary>
        /// SerializeToBase64
        /// </summary>
        /// <param name="body"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static void SerializeToFile(object body, string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                try
                {
                    // create a new BinaryFormatter instance 
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(fs, body);
                }
                catch (SerializationException e)
                {
                    string s = e.Message;
                }
            }
        }

        /// <summary>
        /// DeserializeFromBytes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static T DeserializeFromFile<T>(string filename)
        {
            T result = default(T);

            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    // create a new BinaryFormatter instance 
                    BinaryFormatter formatter = new BinaryFormatter();
                    result = (T)formatter.Deserialize(fs);
                }
                catch (SerializationException e)
                {
                    string s = e.Message;
                }
            }

            return result;
        }
        #endregion

        #region base64 Serialization

        /// <summary>
        /// SerializeToBase64
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public static string SerializeToBase64(object body)
        {
            if (body == null)
                return null;

            string result = null;
            // instantiate a MemoryStream and a new instance of our class          
            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    // create a new BinaryFormatter instance 
                    BinaryFormatter formatter = new BinaryFormatter();
                    // serialize the class into the MemoryStream 
                    formatter.Serialize(ms, body);
                    byte[] byteArray = ms.ToArray();
                    result = Convert.ToBase64String(byteArray, 0, byteArray.Length);
                }
                catch (SerializationException e)
                {
                    string s = e.Message;
                }
            }
            return result;
        }

        /// <summary>
        /// DeserializeFromBase64
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="base64String"></param>
        /// <returns></returns>
        public static T DeserializeFromBase64<T>(string base64String)
        {
            T result = default(T);

            if (base64String == null)
                return result;

            byte[] buf = Convert.FromBase64String(base64String);

            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    ms.Write(buf, 0, buf.Length);

                    ms.Seek(0, 0);

                    BinaryFormatter formatter = new BinaryFormatter();

                    result = (T)formatter.Deserialize(ms);

                }
                catch (SerializationException e)
                {
                    string s = e.Message;
                }
            }

            return result;
        }


        /// <summary>
        /// DeserializeFromBase64
        /// </summary>
        /// <param name="base64String"></param>
        /// <returns></returns>
        public static object DeserializeFromBase64(string base64String)
        {
            object result = null;

            if (base64String == null)
                return null;

            byte[] buf = Convert.FromBase64String(base64String);

            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    ms.Write(buf, 0, buf.Length);

                    ms.Seek(0, 0);

                    BinaryFormatter formatter = new BinaryFormatter();

                    result = formatter.Deserialize(ms);

                }
                catch (SerializationException e)
                {
                    string s = e.Message;
                }
            }

            return result;
        }


        /// <summary>
        /// DeserializeFromBase64
        /// </summary>
        /// <param name="base64String"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static object DeserializeFromBase64(string base64String, ref int size)
        {
            object result = null;

            if (base64String == null)
                return null;

            byte[] buf = Convert.FromBase64String(base64String);
            size = buf.Length;

            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    ms.Write(buf, 0, buf.Length);

                    ms.Seek(0, 0);

                    BinaryFormatter formatter = new BinaryFormatter();

                    result = formatter.Deserialize(ms);

                }
                catch (SerializationException e)
                {
                    string s = e.Message;
                }
            }

            return result;
        }

        /// <summary>
        /// SerializeToBase64
        /// </summary>
        /// <param name="body"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string SerializeToBase64(object body, ref int size)
        {
            string result = null;
            // instantiate a MemoryStream and a new instance of our class          
            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    // create a new BinaryFormatter instance 
                    BinaryFormatter formatter = new BinaryFormatter();
                    // serialize the class into the MemoryStream 
                    formatter.Serialize(ms, body);
                    byte[] byteArray = ms.ToArray();
                    size = byteArray.Length;
                    result = Convert.ToBase64String(byteArray, 0, byteArray.Length);
                }
                catch (SerializationException e)
                {
                    string s = e.Message;
                }
            }
            return result;
        }

        public static string ConvertToBase64(byte[] byteArray)
        {
            return Convert.ToBase64String(byteArray, 0, byteArray.Length);

        }

        public static byte[] ConvertFromBase64(string base64String)
        {
            return Convert.FromBase64String(base64String);
        }
        #endregion

        #region bytes Serialization

        /// <summary>
        /// SerializeToBytes
        /// </summary>
        /// <param name="body"></param>
        /// <param name="enableException"></param>
        /// <returns></returns>
        public static byte[] SerializeToBytes(object body, bool enableException = false)
        {

            byte[] byteArray = null;
            // instantiate a MemoryStream and a new instance of our class          
            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    // create a new BinaryFormatter instance 
                    BinaryFormatter formatter = new BinaryFormatter();
                    // serialize the class into the MemoryStream 
                    formatter.Serialize(ms, body);
                    byteArray = ms.ToArray();
                }
                catch (SerializationException e)
                {
                    if (enableException)
                        throw e;
                    string s = e.Message;
                    return null;
                }
            }
            return byteArray;
        }

        /// <summary>
        /// DeserializeFromBytes
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="enableException"></param>
        /// <returns></returns>
        public static object DeserializeFromBytes(byte[] buf, bool enableException = false)
        {
            object result = null;

            if (buf == null)
                return null;

            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    ms.Write(buf, 0, buf.Length);

                    ms.Seek(0, 0);

                    BinaryFormatter formatter = new BinaryFormatter();

                    result = formatter.Deserialize(ms);
                }
                catch (SerializationException e)
                {
                    if (enableException)
                        throw e;
                    string s = e.Message;
                }
            }

            return result;
        }

        /// <summary>
        /// DeserializeFromBytes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buf"></param>
        /// <param name="enableException"></param>
        /// <returns></returns>
        public static T DeserializeFromBytes<T>(byte[] buf, bool enableException = false)
        {
            T result = default(T);

            if (buf == null)
                return result;

            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    ms.Write(buf, 0, buf.Length);

                    ms.Seek(0, 0);

                    BinaryFormatter formatter = new BinaryFormatter();

                    result = (T)formatter.Deserialize(ms);
                }
                catch (SerializationException e)
                {
                    if (enableException)
                        throw e;
                    string s = e.Message;
                }
            }

            return result;
        }
        #endregion

        #region SizeOf

        /// <summary>
        /// SizeOf object in Bytes
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public static int SizeOf(object body)
        {

            byte[] byteArray = null;
            // instantiate a MemoryStream and a new instance of our class          
            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    // create a new BinaryFormatter instance 
                    BinaryFormatter formatter = new BinaryFormatter();
                    // serialize the class into the MemoryStream 
                    formatter.Serialize(ms, body);
                    byteArray = ms.ToArray();
                }
                catch (SerializationException e)
                {
                    string s = e.Message;
                    return 0;
                }
            }
            return byteArray.Length;
        }
        #endregion

        #region structure


        /// <summary>
        /// Structure To ByteArray
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] StructureToByteArray(object obj)
        {
            try
            {
                int len = Marshal.SizeOf(obj);

                byte[] arr = new byte[len];

                IntPtr ptr = Marshal.AllocHGlobal(len);

                Marshal.StructureToPtr(obj, ptr, true);

                Marshal.Copy(ptr, arr, 0, len);

                Marshal.FreeHGlobal(ptr);

                return arr;
            }
            catch
            {
                return null;
            }

        }

        /// <summary>
        /// ByteArray To Structure
        /// </summary>
        /// <param name="bytearray"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object StructureFromByteArray(byte[] bytearray, Type type)//, ref object obj)
        {
            object obj = null;

            try
            {
                //int len = Marshal.SizeOf(obj);
                int len = bytearray.Length;

                IntPtr i = Marshal.AllocHGlobal(len);

                Marshal.Copy(bytearray, 0, i, len);

                obj = Marshal.PtrToStructure(i, type);//obj.GetType());

                Marshal.FreeHGlobal(i);

                return obj;
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// Structure To Base64
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string StructureToBase64(object obj)
        {
            int size = 0;
            return StructureToBase64(obj, ref size);

        }

        /// <summary>
        /// ByteArray To Structure
        /// </summary>
        /// <param name="base64String"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object StructureFromBase64(string base64String, Type type)//, ref object obj)
        {
            int size = 0;
            return StructureFromBase64(base64String, type, ref size);
        }

        /// <summary>
        /// Structure To Base64
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string StructureToBase64(object obj, ref int size)
        {


            try
            {
                int len = Marshal.SizeOf(obj);

                byte[] byteArray = new byte[len];

                size = byteArray.Length;

                IntPtr ptr = Marshal.AllocHGlobal(len);

                Marshal.StructureToPtr(obj, ptr, true);

                Marshal.Copy(ptr, byteArray, 0, len);

                Marshal.FreeHGlobal(ptr);

                return Convert.ToBase64String(byteArray, 0, byteArray.Length);
            }
            catch
            {
                return null;
            }

        }

        /// <summary>
        /// ByteArray To Structure
        /// </summary>
        /// <param name="base64String"></param>
        /// <param name="type"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static object StructureFromBase64(string base64String, Type type, ref int size)//, ref object obj)
        {
            object obj = null;

            try
            {
                byte[] bytearray = Convert.FromBase64String(base64String);

                //int len = Marshal.SizeOf(obj);
                int len = bytearray.Length;

                size = len;

                IntPtr i = Marshal.AllocHGlobal(len);

                Marshal.Copy(bytearray, 0, i, len);

                obj = Marshal.PtrToStructure(i, type);//obj.GetType());

                Marshal.FreeHGlobal(i);

                return obj;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region jason

        public static string SerializeToJson(object o)
        {
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();

            return jsonSerializer.Serialize(o);
        }

        public static T DeserializeFromJson<T>(string s)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();

            return serializer.Deserialize<T>(s);
        }

        public static string SerializeToJsonArray<T>(List<T> list)
        {
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

            return javaScriptSerializer.Serialize(list);
        }

        public static List<T> DeserializeFromJsonArray<T>(string s)
        {
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

            return javaScriptSerializer.Deserialize<List<T>>(s);
        }
        #endregion

    }

}
