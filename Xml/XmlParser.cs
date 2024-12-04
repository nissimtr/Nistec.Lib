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
using System.Text;
using System.Xml;
using System.Data;
#pragma warning disable  CS1591
namespace Nistec.Xml
{
    /// <summary>
    /// XmlParser utility that parse xml
    /// </summary>
    public class XmlParser : IDisposable
    {
        XmlDocument doc;

        /// <summary>
        /// Initilaize new instance of xml parser class
        /// </summary>
        /// <param name="xml"></param>
        public XmlParser(XmlDocument xml)
        {
            doc = xml;
        }
        /// <summary>
        /// Initilaize new instance of xml parser class
        /// </summary>
        /// <param name="xml"></param>
        public XmlParser(string xml)
        {
            doc = new XmlDocument();
            doc.LoadXml(xml);
        }
        /// <summary>
        /// Dispose
        /// </summary>
        public virtual void Dispose()
        {

        }
        /// <summary>
        /// Get XmlDocument
        /// </summary>
        public XmlDocument Document
        {
            get { return doc; }
        }

        /// <summary>
        /// GetAttributeValue
        /// </summary>
        /// <param name="xNode"></param>
        /// <param name="AttributeName"></param>
        /// <param name="raiseError"></param>
        /// <returns></returns>
        public string GetAttributeValue(XmlNode xNode, string AttributeName, bool raiseError)
        {
            XmlAttribute attrib = xNode.Attributes[AttributeName];
            ValidateXmlAttribute(attrib, AttributeName,raiseError);
            if (attrib != null)
            {
                return attrib.Value;
            }
            return String.Empty;
        }

        /// <summary>
        /// GetAttributeValue
        /// </summary>
        /// <param name="xNode"></param>
        /// <param name="Xpath"></param>
        /// <param name="AttributeName"></param>
        /// <param name="raiseError"></param>
        /// <returns></returns>
        public string GetAttributeValue(XmlNode xNode,string Xpath,string AttributeName, bool raiseError)
        {
            XmlNode node = SelectSingleNode(xNode, Xpath, raiseError);
            return GetAttributeValue(node, AttributeName, raiseError);
       }

        /// <summary>
        /// GetAttributeValue
        /// </summary>
        /// <param name="xNode"></param>
        /// <param name="AttributeName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string GetAttributeValue(XmlNode xNode, string AttributeName, string defaultValue)
        {
            XmlAttribute attrib = xNode.Attributes[AttributeName];
            ValidateXmlAttribute(attrib, AttributeName, false);
            if (attrib != null)
            {
                return attrib.Value;
            }
            return defaultValue;
        }
        /// <summary>
        /// GetAttributeValue
        /// </summary>
        /// <param name="xNode"></param>
        /// <param name="AttributeName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public int GetAttributeValue(XmlNode xNode, string AttributeName, int defaultValue)
        {
            XmlAttribute attrib = xNode.Attributes[AttributeName];
            ValidateXmlAttribute(attrib, AttributeName, false);
            if (attrib != null)
            {
                return Types.ToInt(attrib.Value,defaultValue);
            }
            return defaultValue;
        }

        /// <summary>
        /// GetAttributeValue
        /// </summary>
        /// <param name="xNode"></param>
        /// <param name="Xpath"></param>
        /// <param name="AttributeName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string GetAttributeValue(XmlNode xNode, string Xpath, string AttributeName, string defaultValue)
        {
            XmlNode node = SelectSingleNode(xNode, Xpath, false);
            if (node == null)
                return defaultValue;
            return GetAttributeValue(node, AttributeName, defaultValue);
        }
        /// <summary>
        /// GetAttributeValue
        /// </summary>
        /// <param name="xNode"></param>
        /// <param name="Xpath"></param>
        /// <param name="AttributeName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public int GetAttributeValue(XmlNode xNode, string Xpath, string AttributeName, int defaultValue)
        {
            XmlNode node = SelectSingleNode(xNode, Xpath, false);
            if (node == null)
                return defaultValue;
            return GetAttributeValue(node, AttributeName, defaultValue);
        }
 

        /// <summary>
        /// Get XmlNodeList SelectNodes from a specific XmlNode
        /// </summary>
        /// <param name="Node"></param>
        /// <param name="Xpath"></param>
        /// <param name="raiseError"></param>
        /// <returns></returns>
        public XmlNodeList SelectNodes(XmlNode Node, string Xpath, bool raiseError)
        {
            XmlNodeList list = Node.SelectNodes(Xpath);
            if ((list == null) || (list.Count == 0))
            {
                if (raiseError)
                    throw new XmlException("Invalid Tag " + Node.Name + Xpath);
            }
            return list;
        }

        /// <summary>
        /// Get All values from XmlNodeList by Select a specific XmlNode
        /// </summary>
        /// <param name="Node"></param>
        /// <param name="Xpath"></param>
        /// <param name="raiseError"></param>
        /// <returns></returns>
        public string[] GetNodeListValues(XmlNode Node, string Xpath, bool raiseError)
        {
            XmlNodeList list = Node.SelectNodes(Xpath);
            return GetNodeListValues(list, raiseError);
        }

        /// <summary>
        /// Get All values from XmlNodeList
        /// </summary>
        /// <param name="list"></param>
        /// <param name="raiseError"></param>
        /// <returns></returns>
        public string[] GetNodeListValues(XmlNodeList list, bool raiseError)
        {
            string[] values = null;
            if ((list == null) || (list.Count == 0))
            {
                if (raiseError)
                    throw new XmlException("Invalid XmlNodeList");
                else
                    return null;
            }
            try
            {
                values = new string[list.Count];
                int index = 0;
                foreach (XmlNode node in list)
                {
                    if (node.NodeType == XmlNodeType.Comment)
                        continue;

                    if (node.NodeType == XmlNodeType.Comment)
                        continue;

                    values[index] = node.InnerText;
                    index++;
                }
            }
            catch (Exception ex)
            {
                if (raiseError)
                    throw new XmlException(ex.Message);
            }
            return values;
        }


        /// <summary>
        /// Get All values with all attributes from XmlNodeList by Select a specific XmlNode
        /// </summary>
        /// <param name="Node"></param>
        /// <param name="Xpath"></param>
        /// <param name="raiseError"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public string[] GetNodeListValues(XmlNode Node, string Xpath, bool raiseError, ref string[,] attributes)
        {
            XmlNodeList list = Node.SelectNodes(Xpath);
            return GetNodeListValues(list, raiseError, ref attributes);
        }

        /// <summary>
        /// Get All values from XmlNodeList with all attributes
        /// </summary>
        /// <param name="list"></param>
        /// <param name="raiseError"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public string[] GetNodeListValues(XmlNodeList list, bool raiseError, ref string[,] attributes)
        {
            string[] values = null;
            if ((list == null) || (list.Count == 0))
            {
                if (raiseError)
                    throw new XmlException("Invalid XmlNodeList");
                else
                    return null;
            }
            try
            {
                values = new string[list.Count];
                int index = 0;
                int attribCount = 0;
                attribCount = list[0].FirstChild.Attributes.Count;
                if (attributes == null)
                {
                    attributes = new string[list.Count, attribCount];
                }
                foreach (XmlNode node in list)
                {
                    if (node.NodeType == XmlNodeType.Comment)
                        continue;

                    for (int i = 0; i < attribCount; i++)
                    {
                        attributes[index, i] = node.FirstChild.Attributes[i].Value;
                    }
                    values[index] = node.FirstChild.InnerText;
                    index++;
                }
            }
            catch (Exception ex)
            {
                if (raiseError)
                    throw new XmlException(ex.Message);
            }
            return values;
        }
        /// <summary>
        /// Select Single Node by xpath
        /// </summary>
        /// <param name="Node"></param>
        /// <param name="Xpath"></param>
        /// <param name="raiseError"></param>
        /// <returns></returns>
        public XmlNode SelectSingleNode(XmlNode Node, string Xpath, bool raiseError)
        {
            XmlNode node = Node.SelectSingleNode(Xpath);
            ValidateXmlNode(node,Xpath, raiseError);
            return node;
        }
        /// <summary>
        /// Select Single Node by xpath
        /// </summary>
        /// <param name="Xpath"></param>
        /// <param name="raiseError"></param>
        /// <returns></returns>
        public XmlNode SelectSingleNode(string Xpath, bool raiseError)
        {
            XmlNode node = doc.SelectSingleNode(Xpath);
            ValidateXmlNode(node,Xpath, raiseError);
            return node;
        }
        /// <summary>
        /// Select Single Node by xpath and return the node value
        /// </summary>
        /// <param name="Xpath"></param>
        /// <param name="raiseError"></param>
        /// <returns></returns>
        public string GetNodeValue(string Xpath, bool raiseError)
        {
            XmlNode node = SelectSingleNode(Xpath, raiseError);
            if (node != null)
            {
                return node.Value;
            }
            return String.Empty;
        }

        /// <summary>
        /// Select Single Node by xpath and return the node value
        /// </summary>
        /// <param name="Xpath"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string GetNodeValue(string Xpath, string defaultValue)
        {
            XmlNode node = SelectSingleNode(Xpath, false);
            if (node != null)
            {
                return node.Value;
            }
            return defaultValue;
        }
        /// <summary>
        /// Select Single Node by xpath and return the node value
        /// </summary>
        /// <param name="Xpath"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public int GetNodeValue(string Xpath, int defaultValue)
        {
            XmlNode node = SelectSingleNode(Xpath, false);
            if (node != null)
            {
                return Types.ToInt( node.Value,defaultValue);
            }
            return defaultValue;
        }
 
        /// <summary>
        /// Select Single Node by xpath and return the node value
        /// </summary>
        /// <param name="Node"></param>
        /// <param name="Xpath"></param>
        /// <param name="raiseError"></param>
        /// <returns></returns>
        public string GetNodeValue(XmlNode Node, string Xpath, bool raiseError)
        {
            XmlNode node = SelectSingleNode(Node, Xpath, raiseError);
            if (node != null)
            {
                return node.Value;
            }
            return String.Empty;
        }

        /// <summary>
        /// Select Single Node by xpath and return the node value
        /// </summary>
        /// <param name="Node"></param>
        /// <param name="Xpath"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string GetNodeValue(XmlNode Node, string Xpath, string defaultValue)
        {
            XmlNode node = SelectSingleNode(Node, Xpath, false);
            if (node != null)
            {
                return node.Value;
            }
            return defaultValue;
        }
        /// <summary>
        /// Select Single Node by xpath and return the node value
        /// </summary>
        /// <param name="Node"></param>
        /// <param name="Xpath"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public int GetNodeValue(XmlNode Node, string Xpath, int defaultValue)
        {
            XmlNode node = SelectSingleNode(Node, Xpath, false);
            if (node != null)
            {
                return Types.ToInt( node.Value,defaultValue);
            }
            return defaultValue;
        }
 
        /// <summary>
        /// Select Single Node by xpath and return the node InnerText
        /// </summary>
        /// <param name="Xpath"></param>
        /// <param name="raiseError"></param>
        /// <returns></returns>
        public string GetNodeInnerText(string Xpath, bool raiseError)
        {
            XmlNode node = SelectSingleNode(Xpath, raiseError);
            if (node != null)
            {
                return node.InnerText;
            }
            return String.Empty;
        }

        /// <summary>
        /// Select Single Node by xpath and return the node InnerText
        /// </summary>
        /// <param name="Node"></param>
        /// <param name="Xpath"></param>
        /// <param name="raiseError"></param>
        /// <returns></returns>
        public string GetNodeInnerText(XmlNode Node, string Xpath, bool raiseError)
        {
            XmlNode node = SelectSingleNode(Node, Xpath, raiseError);
            if (node != null)
            {
                return node.InnerText;
            }
            return String.Empty;
        }

        /// <summary>
        /// Select Single Node by xpath and return the node InnerText
        /// </summary>
        /// <param name="Node"></param>
        /// <param name="Xpath"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string GetNodeInnerText(XmlNode Node, string Xpath, string defaultValue)
        {
            XmlNode node = SelectSingleNode(Node, Xpath, false);
            if (node != null)
            {
                return node.InnerText;
            }
            return defaultValue;
        }
        /// <summary>
        /// Select Single Node by xpath and return the node InnerText
        /// </summary>
        /// <param name="Node"></param>
        /// <param name="Xpath"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public int GetNodeInnerText(XmlNode Node, string Xpath, int defaultValue)
        {
            XmlNode node = SelectSingleNode(Node, Xpath, false);
            if (node != null)
            {
                return Types.ToInt(node.InnerText, defaultValue);
            }
            return defaultValue;
        }
      

        private void ValidateXmlNode(XmlNode node,string nodeName, bool raiseError)
        {
            if (node == null && raiseError)
            {
                throw new XmlException("Invalid Tag " + nodeName);
            }
        }
        private void ValidateXmlAttribute(XmlAttribute attrib,string attributeName, bool raiseError)
        {
            if (attrib == null && raiseError)
            {
                throw new XmlException("Invalid Attributes " + attributeName);
            }
        }

    }
}
