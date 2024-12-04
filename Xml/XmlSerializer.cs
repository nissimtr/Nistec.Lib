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
using System.Xml.Serialization;	 
using System.Runtime.Serialization.Formatters.Binary; 
using System.IO;				 
using System.ComponentModel;	 
using System.IO.IsolatedStorage; 
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using System.Collections;
#pragma warning disable  CS1591
namespace Nistec.Xml
{


    /// <summary>
    /// Custom class used as a wrapper to the Xml serialization of an object to/from an Xml file.
    /// </summary>
    public static class XSerializer
    {

        #region static Serializer
  
        /// <summary>
        /// Serialize To Xml using XmlSerializer
        /// </summary>
        /// <param name="body"></param>
        /// <param name="type"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string Serialize(object body, Type type, Encoding encoding)
        {
            string result = null;

            // Create an instance of the XmlSerializer.
            XmlSerializer serializer = new XmlSerializer(type);

            using (MemoryStream ms = new MemoryStream())
            {
                XmlWriter writer = new XmlTextWriter(ms, encoding);
                //result = serializer.Serialize((reader);
                serializer.Serialize(writer, body);
                byte[] byteArray = ms.GetBuffer();
                result = encoding.GetString(byteArray);
                writer.Close();
                ms.Close();
            }
            return result;
        }
        
     
        /// <summary>
        /// Serialize To Xml using XmlSerializer with StringWriter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Serialize<T>(T obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            string result = null;
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            using (StringWriter ms = new StringWriter())
            {
                using (XmlWriter writer = new XmlTextWriter(ms))//, Encoding.UTF8);
                {
                    serializer.Serialize(writer, obj);
                    result = ms.ToString();
                }
            }
            return result;

            //return Serialize<T>(obj, (string)null);
        }

        /// <summary>
        /// Serialize To Xml using XmlSerializer with StringWriter and specific namespace
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        public static string Serialize<T>(T obj, string nameSpace)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            string result = null;

            Type type = typeof(T);

            XmlSerializer serializer = new XmlSerializer(type, nameSpace);

            using (StringWriter ms = new StringWriter())
            {
                using (XmlWriter writer = new XmlTextWriter(ms))//, Encoding.UTF8);
                {
                    serializer.Serialize(writer, obj);
                    result = ms.ToString();
                }
            }
            return result;
        }

        /// <summary>
        /// Serialize To Xml using XmlSerializer with StringWriter and specific namespace
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="nameSpace"></param>
        /// <param name="enableXMLSchema"></param>
        /// <returns></returns>
        public static string Serialize<T>(T obj, string nameSpace, bool enableXMLSchema)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            string result = null;

            Type type = typeof(T);

            XmlSerializer serializer = new XmlSerializer(type, nameSpace);

            using (StringWriter ms = new StringWriter())
            {
                using (XmlWriter writer = new XmlTextWriter(ms))//, Encoding.UTF8);
                {
                    serializer.Serialize(writer, obj, XmlFormatter.GetNamespaces(nameSpace, enableXMLSchema));
                    result = ms.ToString();
                }
            }
            return result;
        }

        /// <summary>
        /// Serialize To Xml using XmlSerializer with StringWriter and specific namespace
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="nameSpace"></param>
        /// <param name="enableXMLSchema"></param>
        /// <param name="replaceDocumentElement"></param>
        /// <returns></returns>
        public static string Serialize<T>(T obj, string nameSpace, bool enableXMLSchema, bool replaceDocumentElement)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            string result = null;

            Type type = typeof(T);

            XmlSerializer serializer = new XmlSerializer(type, nameSpace);

            using (StringWriter ms = new StringWriter())
            {
                using (XmlWriter writer = new XmlTextWriter(ms))//, Encoding.UTF8);
                {
                    serializer.Serialize(writer, obj, XmlFormatter.GetNamespaces(nameSpace, enableXMLSchema));
                    result = ms.ToString();
                }
            }

            if (replaceDocumentElement)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(result);
                string docElement = doc.DocumentElement.Name;
                result = result.Replace(docElement, type.Name);
            }

            return result;
        }

        /// <summary>
        /// Serialize To Xml using XmlSerializer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="writer"></param>
        /// <returns></returns>
        public static void Serialize<T>(T obj, XmlWriter writer)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            Type type = typeof(T);
            XmlSerializer serializer = new XmlSerializer(type);
            serializer.Serialize(writer, obj);
        }

        /// <summary>
        /// Serialize To Xml using XmlSerializer with MemoryStream and specific encode and namespace
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="encode"></param>
        /// <param name="nameSpace"></param>
        /// <param name="enableXMLSchema"></param>
        /// <returns></returns>
        public static string Serialize<T>(T obj, string encode, string nameSpace, bool enableXMLSchema)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            string result = null;

            Type type = typeof(T);

            // Create an instance of the XmlSerializer.
            XmlSerializer serializer = new XmlSerializer(type, nameSpace);

            using (MemoryStream ms = new MemoryStream())
            {
                XmlWriter writer = new XmlTextWriter(ms, Encoding.GetEncoding(encode));
                serializer.Serialize(writer, obj, XmlFormatter.GetNamespaces(nameSpace, enableXMLSchema));
                byte[] byteArray = ms.GetBuffer();
                result = Encoding.GetEncoding(encode).GetString(byteArray);
                writer.Close();
                ms.Close();
            }
            return result;
        }
        #endregion

        #region static Deserialize

        /// <summary>
        /// Deserialize from Xml using XmlSerializer and XmlDocument
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static T Deserialize<T>(XmlReader reader)
        {
            T rVal = default(T);

            XmlSerializer serializer = new XmlSerializer(typeof(T));

            rVal = (T)serializer.Deserialize(reader);

            return rVal;
        }

        /// <summary>
        /// Deserialize from Xml using XmlSerializer with StringReader and XmlDocument
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string xmlString)
        {
            return Deserialize<T>(xmlString, null, false);
        }

        /// <summary>
        /// Deserialize from Xml using XmlSerializer with StringReader and specific namespace
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlString"></param>
        /// <param name="Namespace">Namespace or null</param>
        /// <returns></returns>
        public static T Deserialize<T>(string xmlString, string Namespace)
        {
            return Deserialize<T>(xmlString, Namespace, false);
        }

        /// <summary>
        /// Deserialize from Xml using XmlSerializer with StringReader and specific namespace
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlString"></param>
        /// <param name="Namespace">Namespace or null</param>
        /// <param name="removeXmlDeclaration">should remove XmlDeclaration</param>
        /// <returns></returns>
        public static T Deserialize<T>(string xmlString, /*string ElementName,*/ string Namespace, bool removeXmlDeclaration = false)
        {

            T rVal = default(T);
            if (removeXmlDeclaration)
                xmlString = RemoveXmlDeclaration(xmlString.Trim());

            XmlSerializer serializer = new XmlSerializer(typeof(T), (string)Namespace);// xRoot);
            using (StringReader ms = new StringReader(xmlString))
            {
                XmlReader reader = new XmlTextReader(ms);
                rVal = (T)serializer.Deserialize(reader);
            }

            return rVal;

        }


        /// <summary>
        /// Deserialize from Xml using XmlSerializer with specific encoding and namespace
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlString"></param>
        /// <param name="encode"></param>
        /// <param name="Namespace">Namespace or null</param>
        /// <param name="removeXmlDeclaration">should remove XmlDeclaration</param>
        /// <returns></returns>
        public static T Deserialize<T>(string xmlString, string encode, string Namespace, bool removeXmlDeclaration)
        {
            T rVal = default(T);
            if (removeXmlDeclaration)
                xmlString = RemoveXmlDeclaration(xmlString.Trim());
            //XmlRootAttribute xRoot = new XmlRootAttribute();
            //xRoot.ElementName = ElementName;
            //xRoot.Namespace = Namespace;
            //xRoot.IsNullable = false;

            XmlSerializer serializer = new XmlSerializer(typeof(T), (string)Namespace);// xRoot);
            byte[] bytes = Encoding.GetEncoding(encode).GetBytes(xmlString);

            using (MemoryStream ms = new MemoryStream(bytes))
            {
                XmlReader reader = new XmlTextReader(ms);
                rVal = (T)serializer.Deserialize(reader);
            }

            return rVal;

            //Type type=typeof(T);
            //XmlSerializer serializer = new XmlSerializer(type,ns);
            //using (StringReader ms = new StringReader(xmlString))
            //{
            //    XmlReader reader = new XmlTextReader(ms);
            //   return(T) serializer.Deserialize(reader);
            //}
        }

        /// <summary>
        /// Deserialize from Xml using XmlSerializer
        /// </summary>
        /// <param name="smlString"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object Deserialize(string smlString, Type type)
        {
            object result = null;

            // Create an instance of the XmlSerializer.
            XmlSerializer serializer = new XmlSerializer(type);
            byte[] byteArray = Encoding.Default.GetBytes(smlString);

            using (MemoryStream ms = new MemoryStream(byteArray))
            {
                XmlReader reader = new XmlTextReader(ms);
                result = serializer.Deserialize(reader);
                reader.Close();
                ms.Close();
            }
            return result;
        }
        /// <summary>
        /// Deserialize from Xml using XmlSerializer
        /// </summary>
        /// <param name="smlString"></param>
        /// <param name="type"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        public static object Deserialize(string smlString, Type type, string encode)
        {
            object result = null;

            // Create an instance of the XmlSerializer.
            XmlSerializer serializer = new XmlSerializer(type);
            byte[] byteArray = Encoding.GetEncoding(encode).GetBytes(smlString);

            using (MemoryStream ms = new MemoryStream(byteArray))
            {
                XmlReader reader = new XmlTextReader(ms);
                result = serializer.Deserialize(reader);
                reader.Close();
                ms.Close();
            }
            return result;
        }
        #endregion

        #region static serializer with Namespace

        public static T XmlDeserialize<T>(string xml, string ElementName, string Namespace)
        {
            T rVal = default(T);
            XmlRootAttribute xRoot = new XmlRootAttribute();
            xRoot.ElementName = ElementName;
            xRoot.Namespace = Namespace;
            xRoot.IsNullable = false;

            XmlSerializer serializer = new XmlSerializer(typeof(T), xRoot);
            byte[] bytes = Encoding.UTF8.GetBytes(xml);

            using (MemoryStream ms = new MemoryStream(bytes))
            {
                XmlReader reader = new XmlTextReader(ms);
                rVal = (T)serializer.Deserialize(reader);
            }

            return rVal;
        }

       
        public static T XmlDeserialize<T>(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            Type type = typeof(T);
            XmlRootAttribute rootAttribute = new XmlRootAttribute(doc.DocumentElement.Name);
            rootAttribute.Namespace = doc.DocumentElement.NamespaceURI;

            XmlSerializer ser = new XmlSerializer(type, rootAttribute);
            return (T)ser.Deserialize(new XmlNodeReader(doc.DocumentElement));
        }

       
        public static String Serialize(Object obj, Type type, string Namespace)
        {
            try
            {
                String XmlizedString = null;
                MemoryStream memoryStream = new MemoryStream();
                XmlSerializer xs = new XmlSerializer(type, Namespace);
                XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
                xs.Serialize(xmlTextWriter, obj);
                memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
                XmlizedString = Encoding.UTF8.GetString(memoryStream.ToArray());
                return XmlizedString;
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e); return null;
            }
        }

        public static string SerializeAndNormalize(object obj, string defaultNamespace)
        {
            string Temp = Serialize(obj, defaultNamespace);

            Temp = Regex.Replace(Temp, "&lt;", "<", RegexOptions.IgnoreCase);
            Temp = Regex.Replace(Temp, "&gt;", ">", RegexOptions.IgnoreCase);
            Temp = Regex.Replace(Temp, "&#xD;", string.Empty, RegexOptions.IgnoreCase);

            return Temp;
        }

        public static string Serialize(object obj, string Namespace)
        {
            using (StringWriter writer = new StringWriter())
            {
                XmlSerializer serializer = new XmlSerializer(obj.GetType(), Namespace);
                serializer.Serialize(writer, obj);
                writer.Flush();
                return writer.ToString();
            }
        }

        public static string ArrayToXmlString(Array Arr, string nameSpace, bool removeXmlDeclaration)
        {
            string list = string.Empty;
            using (StringWriter writer = new StringWriter())
            {
                XmlSerializer serializer = new XmlSerializer(Arr.GetType(), nameSpace);
                serializer.Serialize(writer, Arr);
                writer.Flush();
                list = writer.ToString();
                if (removeXmlDeclaration)
                {
                    list = Regex.Replace(list, "<\\?xml.*?>", "", RegexOptions.IgnoreCase);
                }
            }
            return list;
        }

        public static string RemoveXmlDeclaration(string xml)
        {
            return Regex.Replace(xml, "<\\?xml.*?>", "", RegexOptions.IgnoreCase);
        }

        #endregion

        #region extended

       
        static Hashtable serializers = new Hashtable(10);

        static XmlSerializer GetSerializer(Type type, XmlRootAttribute rootAttr)
        {
            string key = type.FullName + rootAttr.ElementName + rootAttr.Namespace;

            // try to get serializer
            XmlSerializer serializer = (XmlSerializer)serializers[key];
            if (serializer == null)
            {
                lock (serializers.SyncRoot)
                {
                    serializer = (XmlSerializer)serializers[key];
                    if (serializer == null)
                    {
                        serializer = new XmlSerializer(type, rootAttr);
                        serializers.Add(key, serializer);
                    }
                }
            }
            return serializer;
        }


        public static string Serialize(object obj)
        {
            return Serialize(obj, (string)null);
        }

        public static string SerializeAndNormalize(object obj)
        {
            string Temp = Serialize(obj, (string)null);

            Temp = Regex.Replace(Temp, "&lt;", "<", RegexOptions.IgnoreCase);
            Temp = Regex.Replace(Temp, "&gt;", ">", RegexOptions.IgnoreCase);
            Temp = Regex.Replace(Temp, "&#xD;", string.Empty, RegexOptions.IgnoreCase);

            return Temp;
        }

        public const string XmlDeclartion = @"<?xml version=""1.0"" encoding=""utf-8""?>";

        public static string NormelaizeXml(string xml)
        {
            Regex regex = new Regex(@">\s*<");
            xml = regex.Replace(xml, "><");
            return xml.Replace("\r\n", "").Replace("\n", "").Trim();
        }

        #endregion

 
        public static object XmlDeserialize(string xml, Type type)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            XmlRootAttribute rootAttribute = new XmlRootAttribute(doc.DocumentElement.Name);
            rootAttribute.Namespace = doc.DocumentElement.NamespaceURI;

            XmlSerializer ser = new XmlSerializer(type, rootAttribute);
            return ser.Deserialize(new XmlNodeReader(doc.DocumentElement));
        }

        public static object XmlDeserialize(XmlElement xml, Type type, string Namespace)
        {
            XmlSerializer ser = new XmlSerializer(type, Namespace);
            return ser.Deserialize(new XmlNodeReader(xml));
        }

    }

}
