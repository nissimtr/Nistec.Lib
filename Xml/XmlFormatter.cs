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
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Collections;
using System.ComponentModel;
using System.Xml.Schema;
using System.Text.RegularExpressions;
using Nistec.Runtime;
#pragma warning disable  CS1591
namespace Nistec.Xml
{
 
    public class XmlFormatter
    {
 
        #region Members
        readonly string m_rootElementName;
        readonly string m_namespace;
        readonly string m_encoding;
        #endregion

        #region Ctor

        public XmlFormatter(string rootElement, string ns, string encoding)
        {
            m_rootElementName = rootElement;
            m_namespace = ns;
            m_encoding = encoding;
        }
        #endregion

        #region format Xml

        public XmlDocument DictionaryToXmlDocument(IDictionary<string, object> properties)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(BuildXmlToStream(properties));
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                doc.LoadXml("<Error>" + error + "</Error>");
            }

            return doc;
        }

        public XmlElement DictionaryToXml(IDictionary<string, object> properties)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(BuildXmlToStream(properties));
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                doc.LoadXml("<Error>" + error + "</Error>");
            }

            return doc.DocumentElement;
        }


        public Stream BuildXmlToStream(IDictionary<string, object> properties)
        {
            Encoding enc = Encoding.GetEncoding(m_encoding);
            MemoryStream ms = new MemoryStream(8192);
            XmlWriter xmlWriter = new XmlTextWriter(ms, enc);

            xmlWriter.WriteStartDocument();

            WriteElement(xmlWriter, m_rootElementName, properties);

            xmlWriter.WriteEndDocument();

            xmlWriter.Flush();
            ms.Position = 0;

            return ms;
        }

        public void BuildXmlAndWriteToStream(IDictionary<string, object> properties, Stream stream)
        {
            Encoding enc = Encoding.GetEncoding(m_encoding);
            XmlWriter xmlWriter = new XmlTextWriter(stream, enc);
            xmlWriter.WriteStartDocument();
            WriteElement(xmlWriter, m_rootElementName, properties);
            xmlWriter.WriteEndDocument();
            xmlWriter.Flush();
            stream.Position = 0;
        }

        void WriteElement(XmlWriter xmlWriter, string elementName, IDictionary<string, object> properties)
        {
            xmlWriter.WriteStartElement(elementName, m_namespace);

            foreach (string propName in properties.Keys)
            {
                object propValue = properties[propName];
                if (propValue == null)
                    continue;

                if (propValue is IDictionary<string, object>)
                {
                    WriteElement(xmlWriter, propName, (IDictionary<string, object>)propValue);
                }
                else if (propValue is XmlNode)
                {
                    xmlWriter.WriteStartElement(propName, m_namespace);
                    xmlWriter.WriteNode(new XmlNodeReader((XmlNode)propValue), false);
                    xmlWriter.WriteEndElement();
                }
                else
                {
                    string propValueStr = propValue.ToString();
                    if (!string.IsNullOrEmpty(propValueStr))
                        xmlWriter.WriteElementString(propName, m_namespace, propValueStr);
                }
            }

            xmlWriter.WriteEndElement();
        }
        #endregion

        #region Declartion

        public const string XmlDeclartion = @"<?xml version=""1.0"" encoding=""utf-8""?>";

        public static string NormelaizeXml(string xml)
        {
            Regex regex = new Regex(@">\s*<");
            xml = regex.Replace(xml, "><");
            return xml.Replace("\r\n", "").Replace("\n", "").Trim();
        }

        public static string RemoveXmlDeclaration(string xml)
        {
            return Regex.Replace(xml, "<\\?xml.*?>", "", RegexOptions.IgnoreCase);
        }
        #endregion

        #region namespace

        public static XmlNamespaceManager GetNamespace(XmlDocument document)
        {
            XmlNamespaceManager mgr = new XmlNamespaceManager(document.NameTable);
            return mgr;
        }
        public static void AddXmlNamespace(XmlNamespaceManager mgr, string prfix, string uri)
        {
            mgr.AddNamespace(prfix, uri);
        }

        public static string GetXmlNamespace(string prfix, string uri)
        {
            return string.Format("xmlns:{0}={1}", prfix, uri);
        }

        public static string GetXmlDefaultNamespace(string prfix)
        {
            return string.Format("xmlns:{0}={1}", prfix, TargetNamespace);
        }

        //This will returns the set of included namespaces for the serializer.
        public static XmlSerializerNamespaces GetNamespaces()
        {
            XmlSerializerNamespaces ns;
            ns = new XmlSerializerNamespaces();
            ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            ns.Add("xsd", "http://www.w3.org/2001/XMLSchema");
            return ns;
        }
         
        public static XmlSerializerNamespaces GetNamespaces(string xmlns, bool enableXMLSchema)
        {
            bool hasNamespace = !string.IsNullOrEmpty(xmlns);

            if (!hasNamespace && !enableXMLSchema)
            {
                return null;
            }

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            if (hasNamespace)
            {
                ns.Add("xmlns", xmlns);
            }
            if (enableXMLSchema)
            {
                ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                ns.Add("xsd", "http://www.w3.org/2001/XMLSchema");
            }
            return ns;
        }

        public const string DefaultNamespace = "xmlns=\"http://www.w3.org/2001/XMLSchema\"";

        //Returns the target namespace for the serializer.
        public static string TargetNamespace
        {
            get
            {
                return "http://www.w3.org/2001/XMLSchema";
            }
        }

        
        #endregion

        #region static xml string converter

        public static XmlElement ToDocumentElement(string xml)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(xml);
            }
            catch (Exception ex)
            {

                string error = ex.Message;
                doc.LoadXml("<Error>" + error + "</Error>");
            }

            return doc.DocumentElement;
        }

        public static XmlDocument ToXmlDocument(string xml)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(xml);
            }
            catch (Exception ex)
            {

                string error = ex.Message;
                doc.LoadXml("<Error>" + error + "</Error>");
            }

            return doc;
        }


        public static XmlElement ToXmlElement(string xml)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(xml);
            }
            catch (Exception ex)
            {

                string error = ex.Message;
                doc.LoadXml("<Error>" + error + "</Error>");
            }

            return doc.FirstChild as XmlElement;
        }

        public static XmlElement LoadXmlfile(string filename)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(filename);
            }
            catch (Exception ex)
            {

                string error = ex.Message;
                doc.LoadXml("<Error>" + error + "</Error>");
            }

            return doc.DocumentElement;
        }

        public static string XmlfileToString(string filename)
        {
            XmlElement doc = LoadXmlfile(filename);
            try
            {
                doc = LoadXmlfile(filename);
            }
            catch (Exception ex)
            {

                string error = ex.Message;
            }

            return doc.OuterXml;
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

        
        #endregion

        #region Dictionary To Xml methods

        public string DictionaryToXmlString(IDictionary properties)
        {
            try
            {

                XmlDocument doc = BuildXml(properties);

                return doc.OuterXml;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                //doc.LoadXml("<Error>" + error + "</Error>");
                return null;
            }
        }

        public XmlElement DictionaryToXmlElement(IDictionary properties)
        {
            try
            {
                XmlDocument doc = BuildXml(properties);
                return doc.DocumentElement;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                //doc.LoadXml("<Error>" + error + "</Error>");
                return null;
            }
        }

        public XmlDocument DictionaryToXml(IDictionary properties)
        {
            try
            {
                XmlDocument doc = BuildXml(properties);
                return doc;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                //doc.LoadXml("<Error>" + error + "</Error>");
                return null;
            }
        }

        void WriteDictionaryToXml(XmlWriter xmlWriter, IDictionary properties)
        {
            try
            {
                WriteElement(xmlWriter, m_rootElementName, properties);
                xmlWriter.Flush();
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
        }

        public XmlDocument BuildXml(IDictionary properties)
        {
            Encoding enc = Encoding.GetEncoding(m_encoding);
            XmlDocument doc = new XmlDocument();

            using (MemoryStream ms = new MemoryStream(8192))
            {
                WriteXmlToStream(properties,ms);
                doc.Load(ms);
            }
            return doc;
        }

        public void WriteXmlToStream(IDictionary properties, Stream stream)
        {
            Encoding enc = Encoding.GetEncoding(m_encoding);
            XmlWriter xmlWriter = new XmlTextWriter(stream, enc);
            xmlWriter.WriteStartDocument();
            WriteElement(xmlWriter, m_rootElementName, properties);
            xmlWriter.WriteEndDocument();
            xmlWriter.Flush();
            stream.Position = 0;
        }

 
        void WriteElement(XmlWriter xmlWriter, string elementName, IDictionary properties)
        {
            xmlWriter.WriteStartElement(elementName, m_namespace);

            foreach (string propName in properties.Keys)
            {
                object propValue = properties[propName];
                if (propValue == null)
                    continue;

                if (propValue is IDictionary)
                {
                    WriteElement(xmlWriter, propName, (IDictionary)propValue);
                }
                else if (propValue is XmlNode)
                {
                    xmlWriter.WriteStartElement(propName, m_namespace);
                    xmlWriter.WriteNode(new XmlNodeReader((XmlNode)propValue), false);
                    xmlWriter.WriteEndElement();
                }
                else
                {
                    string propValueStr = propValue.ToString();
                    if (!string.IsNullOrEmpty(propValueStr))
                        xmlWriter.WriteElementString(propName, m_namespace, propValueStr);
                }
            }

            xmlWriter.WriteEndElement();
        }
        #endregion
          
        #region static Dictionary xml Serialization

        public static XmlDocument DictionaryToXml(IDictionary dictionary, string entityName)
        {
            XmlFormatter formatter = new XmlFormatter(entityName, XmlFormatter.TargetNamespace, "utf-8");
            return formatter.DictionaryToXml(dictionary);
        }

        public static void WriteXmlToDictionary(string xmlString, IDictionary dictionary)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(xmlString);
                XmlNode node = doc.DocumentElement;
                if (node == null)
                {
                    throw new System.Xml.XmlException("Root tag not found");
                }
                XmlNodeList list = node.ChildNodes;
                lock (dictionary.SyncRoot)
                {
                    foreach (XmlNode n in list)
                    {
                        if (n.NodeType == XmlNodeType.Comment)
                            continue;
                        dictionary.Add(n.Name, n.InnerText);
                    }
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
         }

        public static IDictionary XmlToDictionary(string xmlString)
        {
            IDictionary properties = new Hashtable();
            WriteXmlToDictionary(xmlString, properties);
            return properties;
        }
        #endregion

        #region static PropertyDescriptor Serialize

 
        /// <summary>
        /// Serialize object by PropertyDescriptor To Xml
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="nameSpase"></param>
        /// <returns></returns>
        public static string PropertiesToXml(object obj, string nameSpase)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (string.IsNullOrEmpty(nameSpase))
                nameSpase = XmlFormatter.TargetNamespace;
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(obj);

            Encoding enc = Encoding.UTF8;//.GetEncoding(m_encoding);
            XmlDocument doc = new XmlDocument();

            using (MemoryStream ms = new MemoryStream(8192))
            {
                XmlWriter xmlWriter = new XmlTextWriter(ms, enc);

                xmlWriter.WriteStartDocument();

                xmlWriter.WriteStartElement(obj.GetType().Name, nameSpase);


                foreach (PropertyDescriptor p in properties)
                {
                    if (!p.ShouldSerializeValue(obj))
                    {
                        continue;
                    }
                    string elementName = p.Name;
                    object value = p.GetValue(obj);
                    Type valueType = null;
                    if (value != null) valueType = value.GetType();

                    //TODO WHAT IF VALUE IS A CLASS
                    
                    if (value is XmlNode)
                    {
                        xmlWriter.WriteStartElement(p.Name);//, m_namespace);
                        xmlWriter.WriteNode(new XmlNodeReader((XmlNode)value), false);
                        xmlWriter.WriteEndElement();
                    }
                    else
                    {
                        string propValueStr = value == null ? "" : value.ToString();
                        if (!string.IsNullOrEmpty(propValueStr))
                            xmlWriter.WriteElementString(p.Name, /*m_namespace,*/ propValueStr);
                    }
                }

                xmlWriter.WriteEndElement();

                xmlWriter.WriteEndDocument();

                xmlWriter.Flush();
                ms.Position = 0;
                doc.Load(ms);
            }

            return doc.OuterXml;
        }

        /// <summary>
        /// Serialize object by PropertyDescriptor To Xml
        /// </summary>
        /// <param name="xmlString"></param>
        /// <param name="nameSpase"></param>
        /// <returns></returns>
        public static T XmlToProperties<T>(string xmlString, string nameSpase)
        {
            T obj = ActivatorUtil.CreateInstance<T>();

           
            if (string.IsNullOrEmpty(nameSpase))
                nameSpase = XmlFormatter.TargetNamespace;
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(obj);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlString);
            XmlNode node = doc.DocumentElement;
            if (node == null)
            {
                throw new System.Xml.XmlException("Root tag not found");
            }
            XmlNodeList list = node.ChildNodes;

            foreach (XmlNode n in list)
            {
                if (n.NodeType == XmlNodeType.Comment)
                    continue;

                //dictionary.Add(n.Name, n.InnerText);
                properties[n.Name].SetValue(obj, n.InnerText);

            }
            return obj;
        }

        #endregion

     
    }
}
