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
#pragma warning disable CS1591
namespace Nistec.Xml
{
     /// <summary>
    /// XmlDocument Builder Utility
    /// </summary>
    public class XmlBuilder : IDisposable
    {

 
        XmlDocument doc;
        List<XmlNode> NodeList;
        System.Collections.Specialized.NameValueCollection _attributes;

        public System.Collections.Specialized.NameValueCollection Attributes
        {
            get
            {
                if (_attributes == null)
                {
                    _attributes = new System.Collections.Specialized.NameValueCollection();
                }
                return _attributes;
            }
        }
       

        /// <summary>
        /// Initilaize new instance of xml builder class
        /// </summary>
        public XmlBuilder()
        {
            doc = new XmlDocument();
            NodeList = new List<XmlNode>();
        }
        /// <summary>
        /// Dispose
        /// </summary>
        public virtual void Dispose()
        {
            doc = null;
            NodeList=null;
 
        }
        /// <summary>
        /// Get the XmlDocument
        /// </summary>
        public XmlDocument Document
        {
            get { return doc; }
        }

        /// <summary>
        /// Get or Set internal XmlNode
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public XmlNode this[int index]
        {
            get 
            { 
                return NodeList[index]; 
            }
            set 
            {
                NodeList.Add(value);
            }
        }

        /// <summary>
        /// CreateAttribute
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public XmlAttribute CreateAttribute(string Name, string Value)
        {
            XmlAttribute attribute = doc.CreateAttribute(Name);
            attribute.Value = Value;
            return attribute;
        }

        /// <summary>
        /// CreateAttribute Attribute to a specific XmlNode
        /// </summary>
        /// <param name="xNode"></param>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        public void CreateAttribute(XmlNode xNode, string Name, string Value)
        {
            XmlAttribute attribute = doc.CreateAttribute(Name);
            attribute.Value = Value;

            //Add the attribute to the node.
            xNode.Attributes.SetNamedItem(attribute);

        }
        /// <summary>
        /// CreateXmlNode
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public XmlNode CreateXmlNode(XmlNodeType type,string Name, string Value)
        {
            XmlNode node = doc.CreateNode(type, Name, "");
            node.Value = Value;
            return node;
        }
        /// <summary>
        /// CreateElement
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public XmlElement CreateElement(string Name, string Value)
        {
            XmlElement node = doc.CreateElement(Name);
            node.InnerText = Value;
            return node;
        }

        /// <summary>
        /// CreateNode
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="Name"></param>
        /// <returns></returns>
        public XmlNode CreateNode(XmlNodeType nodeType, string Name)
        {
            return doc.CreateNode(nodeType, Name,"");
        }

        /// <summary>
        /// CreateNode
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="Name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public XmlNode CreateNode(XmlNodeType nodeType, string Name,string value)
        {
            XmlNode node= doc.CreateNode(nodeType, Name, "");
            node.InnerText = value;
            return node;
        }
        /// <summary>
        /// CreateEndElement
        /// </summary>
        /// <param name="Name"></param>
        public void CreateEndElement(string Name)
        {
            doc.CreateNode(XmlNodeType.EndElement, Name, "");
            //doc.AppendChild(elm);
        }

        /// <summary>
        /// Append xml node to xml document 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        public void AppendNode(XmlNode parent, XmlNode child)
        {
            parent.AppendChild(child);
        }

      
        /// <summary>
        /// AppendXmlDeclaration
        /// </summary>
        public void AppendXmlDeclaration()
        {
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", null));
        }


        /// <summary>
        /// Append Attribute to xml document
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        public void AppendAttribute(string Name, string Value)
        {
            XmlAttribute attribute = doc.CreateAttribute(Name);
            attribute.Value = Value;
            doc.DocumentElement.SetAttributeNode(attribute);
        }
        /// <summary>
        /// Append Attribute to a specific XmlNode
        /// </summary>
        /// <param name="xNode"></param>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        public void AppendAttribute(XmlNode xNode,string Name, string Value)
        {
            XmlAttribute attribute = doc.CreateAttribute(Name);
            attribute.Value = Value;

            //Add the attribute to the document.
            xNode.Attributes.SetNamedItem(attribute);

        }
        /// <summary>
        /// Append Attribute to a specific XmlNode
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        public void AppendAttribute(int parent, string Name, string Value)
        {
            XmlAttribute attribute = doc.CreateAttribute(Name);
            attribute.Value = Value;

            //Add the attribute to the document.
            this[parent].Attributes.SetNamedItem(attribute);

        }
        /// <summary>
        /// Append CDATA section to xml document
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        public void AppendCDATA(string Name, string Value)
        {
            XmlNode node = doc.CreateNode(XmlNodeType.CDATA, Name, "");
            node.Value = Value;
            doc.AppendChild(node);
        }
        /// <summary>
        /// Append xml node to xml document 
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        public XmlNode AppendNode(string Name, string Value)
        {
            XmlNode node = doc.CreateNode(XmlNodeType.Element, Name, "");
            node.InnerText = Value;
           return  doc.AppendChild(node);
        }
        /// <summary>
        /// Append xml node to xml document 
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        /// <param name="appendTo"></param>
        public void AppendNode(string Name, string Value, int appendTo)
        {
            XmlNode node = doc.CreateNode(XmlNodeType.Element, Name, "");
            node.InnerText = Value;
            this[appendTo] = doc.AppendChild(node);
        }
        /// <summary>
        /// Append CDATA section to a specific XmlNode
        /// </summary>
        /// <param name="xNode"></param>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        public void AppendCDATA(XmlNode xNode, string Name, string Value)
        {
            XmlNode node = doc.CreateNode(XmlNodeType.CDATA, Name, "");
            node.Value = Value;
            xNode.AppendChild(node);  
        }
        /// <summary>
        /// Append CDATA section to a specific XmlNode
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        public void AppendCDATA(int parent, string Name, string Value)
        {
            XmlNode node = doc.CreateNode(XmlNodeType.CDATA, Name, "");
            node.Value = Value;
            this[parent].AppendChild(node);
        }
        /// <summary>
        /// Append XmlElement to a specific XmlNode
        /// </summary>
        /// <param name="Name"></param>
        public XmlNode AppendElement(string Name)
        {
            XmlElement elem = doc.CreateElement(Name);
            return doc.AppendChild(elem);
        }
        /// <summary>
        /// Append XmlElement to a specific XmlNode
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="appendTo"></param>
        public void AppendElement(string Name, int appendTo)
        {
            XmlElement elem = doc.CreateElement(Name);
            this[appendTo] = doc.AppendChild(elem);
        }

        /// <summary>
        /// Append XmlElement to a specific XmlNode
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        public XmlNode AppendElement(string Name, string Value)
        {
            XmlElement elem = doc.CreateElement(Name);
            elem.InnerText = Value;
            return doc.AppendChild(elem);
        }
        /// <summary>
        /// Append XmlElement to a specific XmlNode
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        /// <param name="appendTo"></param>
        public void AppendElement(string Name, string Value, int appendTo)
        {
            XmlElement elem = doc.CreateElement(Name);
            elem.InnerText = Value;
            this[appendTo] = doc.AppendChild(elem);
        }
        /// <summary>
        /// Append Empty XmlElement to a specific XmlNode
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="appendTo"></param>
        public void AppendEmptyElement(string Name, int appendTo)
        {
            XmlElement elem = doc.CreateElement(Name);
            this[appendTo] = doc.AppendChild(elem);
        }

        /// <summary>
        /// Append XmlElement to a specific XmlNode
        /// </summary>
        /// <param name="xNode"></param>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        public XmlNode AppendElement(XmlNode xNode, string Name, string Value)
        {
            XmlElement elem = doc.CreateElement(Name);
            elem.InnerText=Value;
            return xNode.AppendChild(elem);
        }
        /// <summary>
        /// Append XmlElement to a specific XmlNode
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        public XmlNode AppendElement(int parent, string Name, string Value)
        {
            XmlElement elem = doc.CreateElement(Name);
            elem.InnerText = Value;
            return this[parent].AppendChild(elem);
        }

        /// <summary>
        /// Append XmlElement to a specific XmlNode
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        /// <param name="appendTo"></param>
        public void AppendElement(int parent, string Name, string Value, int appendTo)
        {
            XmlElement elem = doc.CreateElement(Name);
            elem.InnerText = Value;
            this[appendTo] = this[parent].AppendChild(elem);
        }

        /// <summary>
        /// Append XmlElement to a specific XmlNode with list of attributes
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        /// <param name="attributes"></param>
        public void AppendElement(int parent, string Name, string Value, System.Collections.Specialized.NameValueCollection attributes)
        {
            XmlElement elem = doc.CreateElement(Name);
            elem.InnerText = Value;
            XmlNode node = this[parent].AppendChild(elem);
            for (int i = 0; i < attributes.Count; i++)
            {
                AppendAttribute(node, attributes.GetKey(i),attributes.Get(i));
            }
        }
        /// <summary>
        /// Append XmlElement to a specific XmlNode with list of attributes
        /// </summary>
        /// <param name="xNode"></param>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        /// <param name="attributes"></param>
        public void AppendElement(XmlNode xNode, string Name, string Value, System.Collections.Specialized.NameValueCollection attributes)
        {
            XmlElement elem = doc.CreateElement(Name);
            elem.InnerText = Value;
            XmlNode node = xNode.AppendChild(elem);

            for (int i = 0; i < attributes.Count; i++)
            {
                AppendAttribute(node, attributes.GetKey(i), attributes.Get(i));
            }
        }

        /// <summary>
        /// Append XmlElement to a specific XmlNode with list of attributes
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        /// <param name="attributes"></param>
        public void AppendElementAttributes(int parent, string Name, string Value, params string[] attributes)
        {
            XmlElement elem = doc.CreateElement(Name);
            elem.InnerText = Value;
            XmlNode node = this[parent].AppendChild(elem);
            if (attributes != null)
            {
                if (attributes.Length % 2 > 0)
                {
                    throw new ArgumentException("attributes params should be dual");
                }
                for (int i = 0; i < attributes.Length; i++)
                {
                    AppendAttribute(node, attributes[i], attributes[i + 1]);
                    i++;
                }
            }
        }
        /// <summary>
        /// Append XmlElement to a specific XmlNode with list of attributes
        /// </summary>
        /// <param name="xNode"></param>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        /// <param name="attributes"></param>
        public void AppendElementAttributes(XmlNode xNode, string Name, string Value,  params string[] attributes)
        {
            XmlElement elem = doc.CreateElement(Name);
            elem.InnerText = Value;
            XmlNode node = xNode.AppendChild(elem);
            if (attributes != null)
            {
                if (attributes.Length % 2 > 0)
                {
                    throw new ArgumentException("attributes params should be dual");
                }
                for (int i = 0; i < attributes.Length; i++)
                {
                    AppendAttribute(node, attributes[i], attributes[i + 1]);
                    i++;
                }
            }
         }

        /// <summary>
        /// Append EmptyXmlElement to a specific XmlNode
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="Name"></param>
        /// <param name="appendTo"></param>
        public void AppendEmptyElement(int parent, string Name, int appendTo)
        {
            XmlElement elem = doc.CreateElement(Name);
            this[appendTo] = this[parent].AppendChild(elem);
        }

        /// <summary>
        /// Create XmlElement and Append Node List
        /// </summary>
        /// <param name="NodeName"></param>
        /// <param name="Names"></param>
        /// <param name="Values"></param>
        public void AppendNodeList(string NodeName, string[] Names, string[] Values)
        {
            XmlElement elem = doc.CreateElement(NodeName);
            doc.AppendChild(elem);
            AppendNodeList(elem,Names,Values);
        }

        /// <summary>
        /// Append Node List to a spesific XmlNode
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="Names"></param>
        /// <param name="Values"></param>
        public void AppendNodeList(int parent, string[] Names, string[] Values)
        {

            AppendNodeList(this[parent], Names, Values);
        }

        /// <summary>
        /// Append Node List to a spesific XmlNode
        /// </summary>
        /// <param name="xNode"></param>
        /// <param name="Names"></param>
        /// <param name="Values"></param>
        public void AppendNodeList(XmlNode xNode, string[] Names, string[] Values)
        {
            if (Values == null || Values.Length == 0 || Names == null || Names.Length == 0 || Names.Length != Values.Length)
                return;

            if (Names.Length != Values.Length)
            {
                throw new ArgumentException("Values Not equal ToString Names");
            }

            try
            {
                for (int i = 0; i < Values.Length; i++)
                {
                    AppendElement(xNode,Names[i], Values[i]);
                }
            }
            catch (Exception ex)
            {
                throw new XmlException(ex.Message);
            }
        }

        /// <summary>
        /// Append Node List to a spesific XmlNode
        /// </summary>
        /// <param name="xNode"></param>
        /// <param name="childs"></param>
        public void AppendNodeList(XmlNode xNode, XmlNode[] childs)
        {
            if (childs==null || childs.Length ==0)
            {
                throw new ArgumentException("Invalid children nodes");
            }

            try
            {
                for (int i = 0; i < childs.Length; i++)
                {
                    xNode.AppendChild(childs[i]);
                }
            }
            catch (Exception ex)
            {
                throw new XmlException(ex.Message);
            }
        }

        /// <summary>
        /// Create XmlElement and Append Node List
        /// </summary>
        /// <param name="NodeName"></param>
        /// <param name="Names"></param>
        /// <param name="Values"></param>
        public void AppendNodeList(string NodeName, string Names, string[] Values)
        {
            XmlElement elem = doc.CreateElement(NodeName);
            doc.AppendChild(elem);
            AppendNodeList(elem,Names, Values);
        }
        /// <summary>
        /// Append Node List to a spesific XmlNode
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="Names"></param>
        /// <param name="Values"></param>
        public void AppendNodeList(int parent, string Names, string[] Values)
        {
            AppendNodeList(this[parent], Names, Values);
        }
        /// <summary>
        /// Append Node List to a spesific XmlNode
        /// </summary>
        /// <param name="xNode"></param>
        /// <param name="Names"></param>
        /// <param name="Values"></param>
        public void AppendNodeList(XmlNode xNode, string Names, string[] Values)
        {
            if (Values == null || Values.Length == 0 || Names == null || Names.Length == 0 || Names.Length != Values.Length)
                return;

            if (Names.Length != Values.Length)
            {
                throw new ArgumentException("Values Not equal ToString Names");
            }

            try
            {
                for (int i = 0; i < Values.Length; i++)
                {
                    AppendElement(xNode,Names, Values[i]);
                }
            }
            catch (Exception ex)
            {
                throw new XmlException(ex.Message);
            }
        }

    }

 }
