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
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Collections;
using System.Data;

#pragma warning disable  CS1591

namespace Nistec.Generic
{
    /// <summary>
    /// Represents simple xml serializable/deserializable name/value table.
    /// </summary>
    public class XmlTable
    {
        public const string XmlDeclaration = @"<?xml version=""1.0"" encoding=""utf-8""?>";
        const string DefaultTableName = "Empty";

        private string    m_TableName = "";
        private Dictionary<string,object> m_Values   = null;

        static XmlTable SingleTable
        {
            get { return new XmlTable(DefaultTableName); }
        }

        static XmlTable MultiTable
        {
            get { return new XmlTable(DefaultTableName,true); }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public XmlTable(string tableName)
        {
            if(tableName == null || tableName == ""){
                throw new Exception("Table name can't be empty !");
            }
            IsMulti = false;
            m_TableName = tableName;

            m_Values = new Dictionary<string, object>();
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public XmlTable(string tableName, bool isMulti = false)
        {
            if (tableName == null || tableName == "")
            {
                throw new Exception("Table name can't be empty !");
            }
            IsMulti = isMulti;
            m_TableName = tableName;

            m_Values = new Dictionary<string, object>();
        }

        /// <summary>
        /// Constructor using XmlNode.Attributes.
        /// </summary>
        public XmlTable(XmlNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("XmlTable.XmlNode");
            }
            IsMulti = false;
            m_TableName = node.Name;

            m_Values = new Dictionary<string, object>();

            LoadAttributes(node, false);

        }

        /// <summary>
        /// Constructor using XmlNode.Attributes.
        /// </summary>
        public XmlTable(XmlNode node, string fieldKey)
        {
            if (node == null)
            {
                throw new ArgumentNullException("XmlTable.XmlNode");
            }
            IsMulti = true;
            m_TableName = node.Name;

            m_Values = new Dictionary<string, object>();

            LoadFromXml(node, fieldKey);
        }

        public void LoadAttributes(XmlNode node, bool clearBeforLoad)
        {
            if (clearBeforLoad)
            {
                m_Values.Clear();
            }
            foreach (XmlAttribute attr in node.Attributes)
            {
                m_Values[attr.Name] = attr.Value;
            }
        }

        #region method Add

        public void Add(string key, XmlTable value)
        {
            if (m_Values.ContainsKey(key))
            {
                throw new Exception("Specified key '" + key + "' already exists !");
            }

            m_Values.Add(key, value);
        }

        /// <summary>
        /// Adds name/value to table.
        /// </summary>
        /// <param name="key">Key of the value pair.</param>
        /// <param name="value">Value.</param>
        public void Add(string key, string value)
        {
            if (m_Values.ContainsKey(key))
            {
                throw new Exception("Specified key '" + key + "' already exists !");
            }

            m_Values.Add(key, value);
        }

        /// <summary>
        /// Adds name/value to table.
        /// </summary>
        /// <param name="key">Key of the value pair.</param>
        /// <param name="value">Value.</param>
        public void Add(string key, int value)
        {
            if (m_Values.ContainsKey(key))
            {
                throw new Exception("Specified key '" + key + "' already exists !");
            }

            m_Values.Add(key, value);
        }
        #endregion

        #region method GetValue

        /// <summary>
        /// Determines whether the XmlTable contains the specified key.
        /// </summary>
        /// <param name="key">Key of value to get.</param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            return m_Values.ContainsKey(key);
        }

        /// <summary>
        /// Determines whether the XmlTable contains the specified value.
        /// </summary>
        /// <param name="value">value to get.</param>
        /// <returns></returns>
        public bool ContainsValue(object value)
        {
            return m_Values.ContainsValue(value);
        }

        /// <summary>
        /// Gets value from name/value table.
        /// </summary>
        /// <param name="key">Key of value to get.</param>
        /// <returns></returns>
        public string GetValue(string key)
        {
            object val = null;
            if (m_Values.TryGetValue(key, out val))
            {
                return val == null ? null : val.ToString();
            }
            return null;
        }

        /// <summary>
        /// Gets value from name/value table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            object val = null;
            if (m_Values.TryGetValue(key, out val))
            {
                return GenericTypes.Convert<T>(val);
            }
            return default(T);
        }

        /// <summary>
        /// Gets value from name/value table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="valueIfNull"></param>
        /// <returns></returns>
        public T Get<T>(string key, T valueIfNull)
        {
            object val = null;
            if (m_Values.TryGetValue(key, out val))
            {
                return GenericTypes.Convert<T>(val, valueIfNull);
            }
            return valueIfNull;
        }

        /// <summary>
        /// Gets value from name/value table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="valueIfNull"></param>
        /// <returns></returns>
        public T GetEnum<T>(string key, T valueIfNull)
        {
            object val = null;
            if (m_Values.TryGetValue(key, out val))
            {
                return GenericTypes.ConvertEnum<T>(val.ToString(), valueIfNull);
            }
            return valueIfNull;
        }


        public XmlTable GetRow(string rowId)
        {
            if (m_Values.ContainsKey(rowId))
            {
                return (XmlTable)m_Values[rowId];
            }
            return MultiTable;
        }


        public void SetRow(string rowId, XmlTable value)
        {
            m_Values[rowId] = value;
        }

        public void Set<T>(string rowId, string key, T value)
        {
            if (m_Values.ContainsKey(rowId))
            {
                ((XmlTable)m_Values[rowId]).Set<T>(key, value);
            }
        }

        public void Set<T>(string key, T value)
        {
            m_Values[key] = value;
        }

        public void Set(string key, object value)
        {
            m_Values[key] = value;
        }

        public bool SetNonEqual(string key, object value)
        {
            object val = null;
            if (m_Values.TryGetValue(key, out val))
            {
                if (value != val)
                {
                    m_Values[key] = value;
                    return true;
                }
            }
            return false;
        }

        public IDictionary Data
        {
            get { return m_Values; }
        }

        public XmlTable this[string rowId]
        {
            get
            {
                return GetRow(rowId);
            }
        }

        public string this[string rowId, string key]
        {
            get
            {
                return GetRow(rowId).GetValue(key);
            }
        }

        #endregion
        
        #region method Parse

        /// <summary>
        /// Parses table from byte[] xml data.
        /// </summary>
        /// <param name="data">Table data.</param>
        /// <param name="isMulti"></param>
        public void Parse(byte[] data, bool isMulti = false)
        {
            //m_Values.Clear();

            Dictionary<string, object> hvalues = new Dictionary<string, object>();

            MemoryStream ms = new MemoryStream(data);
            XmlTextReader wr = new XmlTextReader(ms);

            // Read start element
            wr.Read();

            m_TableName = wr.LocalName;

            if (isMulti)
            {
                while (wr.Read())
                {
                    if (wr.NodeType == XmlNodeType.Element)
                    {
                        if (wr.IsStartElement() && wr.MoveToContent() == XmlNodeType.Element)//.ValueType == typeof(XmlNode))
                        {
                            string rowid = wr.LocalName;
                            XmlReader wrs = wr.ReadSubtree();
                            XmlTable innerTable = new XmlTable(rowid);
                            wrs.Read();
                            while (wrs.Read())
                            {
                                if (wrs.NodeType == XmlNodeType.Element)
                                {
                                    string name = wrs.LocalName;
                                    string val = wrs.ReadElementString();
                                    innerTable.Add(name, val);
                                }
                            }

                            if (!innerTable.IsEmpty)
                            {
                                hvalues.Add(rowid, innerTable);
                            }
                        }

                    }
                }
            }
            else
            {
                // Read Name/Values
                while (wr.Read())
                {
                    if (wr.NodeType == XmlNodeType.Element)
                    {
                        hvalues.Add(wr.LocalName, wr.ReadElementString());
                    }
                }
            }
            m_Values = hvalues;
        }

        #endregion
        
        #region method ToString

        /// <summary>
        /// Returns string representation of xml table.
        /// </summary>
        /// <returns>Returns string representation of xml table.</returns>
        public string ToStringData()
        {
            return Encoding.Default.GetString(ToByteData());
        }

        #endregion

        #region method ToByteData


        public void Write(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("XmlTable.ToXml can not retrive data for xml");
            }
            byte[] bytes = ToByteData();
            if (bytes == null || bytes.Length == 0)
            {
                throw new ArgumentNullException("XmlTable.ToXml can not retrive data for stream");
            }
            stream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Returns byte[] representation of xml table.
        /// </summary>
        /// <returns>Returns byte[] representation of xml table.</returns>
        public byte[] ToByteData()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (XmlTextWriter wr = new XmlTextWriter(ms, Encoding.UTF8))
                {

                    // Write table start
                    wr.WriteStartElement(m_TableName);
                    wr.WriteRaw("\r\n");

                    // Write elements
                    foreach (var entry in m_Values)
                    {

                        object propValue = entry.Value;

                        if (propValue is XmlTable)
                        {
                            wr.WriteRaw("\t");
                            wr.WriteStartElement(((XmlTable)propValue).TableName);
                            WriteData((XmlTable)propValue, wr);
                            wr.WriteEndElement();
                            wr.WriteRaw("\r\n");
                        }
                        else
                        {
                            wr.WriteRaw("\t");
                            wr.WriteStartElement(entry.Key.ToString());
                            wr.WriteValue(entry.Value.ToString());
                            wr.WriteEndElement();
                            wr.WriteRaw("\r\n");
                        }
                    }

                    // Write table end
                    wr.WriteEndElement();
                    wr.Flush();
                }
                return ms.ToArray();
            }
        }


        /// <summary>
        /// Returns XmlDocument representation of xml table.
        /// </summary>
        /// <returns>Returns XmlDocument representation of xml table.</returns>
        public XmlDocument ToXml()
        {

            byte[] bytes = ToByteData();
            if (bytes == null || bytes.Length == 0)
            {
                throw new ArgumentNullException("XmlTable.ToXml can not retrive data for xml");
            }
            XmlDocument doc = new XmlDocument();
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                doc.Load(ms);
            }
            return doc;
        }


        void WriteData(XmlTable table, XmlTextWriter wr)
        {
            // Write elements
            foreach (var entry in table.m_Values)
            {

                wr.WriteRaw("\t");
                wr.WriteStartElement(entry.Key.ToString());

                wr.WriteValue(entry.Value.ToString());

                wr.WriteEndElement();
                wr.WriteRaw("\r\n");

            }

            // Write table end
            //wr.WriteEndElement();
        }

        DataTable GetSchema()
        {
            DataTable dt = new DataTable(m_TableName);
            Dictionary<string, object> values = null;

            //find the first row values
            foreach (var entry in m_Values)
            {
                object propValue = entry.Value;

                if (propValue is XmlTable)
                {
                    values = ((XmlTable)propValue).m_Values;
                    break;
                }
                else
                {
                    values = m_Values;
                    break;
                }
            }

            foreach (var entry in values)//m_Values)
            {
                dt.Columns.Add(entry.Key.ToString(), entry.Value.GetType());
            }
             return dt.Clone();
        }

        /// <summary>
        /// Returns DataTable representation of xml table.
        /// </summary>
        /// <returns>Returns byte[] representation of xml table.</returns>
        public DataTable ToDataTable()
        {
            DataTable dt = GetSchema();
            DataRow r = dt.NewRow();
            bool isMulti = false;
            foreach (var entry in m_Values)
            {
                object propValue = entry.Value;
                isMulti = false;
                if (propValue is XmlTable)
                {
                    DataRow r1 = dt.NewRow();
                    foreach (var entry1 in ((XmlTable)propValue).m_Values)
                    {
                        r1[entry1.Key.ToString()] = entry1.Value;
                    }
                    dt.Rows.Add(r1);
                    isMulti = true;
                }
                else
                {
                    r[entry.Key.ToString()] = propValue;// entry.Value;
                }
            }
            if (!isMulti)
            {
                dt.Rows.Add(r);
            }
            return dt;
        }

        #endregion
        
        #region Porperties Implementation

        /// <summary>
        /// Gets or sets table name.
        /// </summary>
        public string TableName
        {
            get{ return m_TableName; }

            set{ m_TableName = value; }
        }
        /// <summary>
        /// Gets indicate wether is multi rows.
        /// </summary>
        public bool IsMulti
        {
            get;
            private set;
        }

        public bool IsEmpty
        {
            get {return m_Values== null || m_Values.Count==0; }
        }

        #endregion

        #region Load xml config


        public void LoadFromFile(string file, string tableName, string fieldKey)
        {
            if (string.IsNullOrEmpty(file))
                return;
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(file);
                LoadFromXml(doc, tableName,fieldKey);
            }
            catch (Exception ex)
            {
                OnError("LoadXmlTable file error " + ex.Message);
            }
        }

        public void LoadFromXml(string xml, string tableName, string fieldKey)
        {
            if (string.IsNullOrEmpty(xml))
                return;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                LoadFromXml(doc, tableName, fieldKey);
            }
            catch (Exception ex)
            {
                OnError("LoadXmlTable xml string error " + ex.Message);
            }
        }

        public void LoadFromXml(XmlDocument doc, string tableName, string fieldKey)
        {
            if (doc == null)
                return;

            XmlNode node = doc.SelectSingleNode("//" + tableName);
            if (node == null)
                return;
            this.TableName = tableName;

            if (string.IsNullOrEmpty(fieldKey))
            {
                LoadXmlTable(node);
            }
            else
            {
                LoadFromXml(node, fieldKey);
            }
        }


        public void LoadFromXml(XmlNode node, string fieldKey)
        {
            
            try
            {
                if (node == null)
                {
                    throw new ArgumentNullException("XmlTable.XmlNode");
                }

                XmlNodeList list = node.ChildNodes;
                if (list == null)
                    return;

                if (string.IsNullOrEmpty(fieldKey))
                {
                    throw new ArgumentException("Invalid fieldKey for multi tables");
                }

                m_Values.Clear();


                foreach (XmlNode n in list)
                {
                    if (n.NodeType == XmlNodeType.Comment)
                        continue;

                    XmlTable xml = new XmlTable(n);
                    string key = xml.Get<string>(fieldKey);
                    m_Values[key] = xml;
                }

                IsMulti = true;

            }
            catch (Exception ex)
            {
                OnError("LoadXmlTable node error " + ex.Message);
            }

        }

         public static XmlTable LoadXmlKeyValue(XmlNode node, string attrKey, string attrValue)
        {

            XmlTable table = new XmlTable(node.Name);

 
            if (node == null)
            {
                throw new ArgumentNullException("XmlTable.XmlNode");
            }

            XmlNodeList list = node.ChildNodes;
            if (list == null)
                return null;

            if (string.IsNullOrEmpty(attrKey))
            {
                throw new ArgumentException("Invalid fieldKey for multi tables");
            }


            foreach (XmlNode n in list)
            {
                if (n.NodeType == XmlNodeType.Comment)
                    continue;

                string key = n.Attributes[attrKey].Value;
                string value = n.Attributes[attrValue].Value;

                table.m_Values[key] = value;
            }
           
            return table;
        }

        void LoadXmlTable(XmlNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("XmlTable.XmlNode");
            }

            m_TableName = node.Name;

            LoadAttributes(node, true);
        }

        /// <summary>
        /// OnErrorOccured
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnError(string e)
        {
            //if (SyncError != null)
            //    SyncError(this, new GenericEventArgs<string>(e));
        }


        #endregion load xml config
    }
}
