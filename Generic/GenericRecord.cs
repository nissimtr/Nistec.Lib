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
using System.Collections;
using System.Runtime.Serialization;
using System.Xml;
using System.Data;
using System.IO;
using Nistec.IO;
using Nistec.Xml;
using Nistec.Serialization;
using System.Collections.Specialized;
using Nistec.Runtime;

#pragma warning disable  CS1591

namespace Nistec.Generic
{
    /// <summary>
    /// Represent Serializable Generic Dictionary that implement <see cref="ISerialEntity"/> and <see cref="IEntityDictionary"/>.
    /// </summary>
    [Serializable]
    public class GenericRecord : Dictionary<string, object>, System.Xml.Serialization.IXmlSerializable, IEntityDictionary, ISerialJson, IKeyValue<object>
    {
        public const string EntityName = "EntityRecord";
        
        #region serialization

        public GenericRecord(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region xml serialization

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            WriteElement(writer, EntityName, this);
        }

        void WriteElement(XmlWriter xmlWriter, string elementName, IDictionary properties)
        {
            string nameSpace = Nistec.Xml.XmlFormatter.TargetNamespace;
            xmlWriter.WriteStartElement(elementName, nameSpace);

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
                    xmlWriter.WriteStartElement(propName);//, m_namespace);
                    xmlWriter.WriteNode(new XmlNodeReader((XmlNode)propValue), false);
                    xmlWriter.WriteEndElement();
                }
                else
                {
                    string propValueStr = propValue.ToString();
                    if (!string.IsNullOrEmpty(propValueStr))
                        xmlWriter.WriteElementString(propName, propValueStr);
                }
            }

            xmlWriter.WriteEndElement();
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            string nodeName = "";
            bool isEnd = false;

            while (reader.Read() && !isEnd)
            {

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        nodeName = reader.Name;
                        break;

                    case XmlNodeType.Text:

                        if (!string.IsNullOrEmpty(nodeName))
                        {
                            this.Add(nodeName, reader.Value);
                        }
                        break;

                    case XmlNodeType.EndElement:

                        if (reader.Name.Contains(EntityName))
                            isEnd = true;
                        nodeName = string.Empty;
                        break;

                    default:

                        break;
                }
            }
        }

        public System.Xml.Schema.XmlSchema GetSchema() { return null; }


        public string SerializeToXml(string entityName)
        {
            if (string.IsNullOrEmpty(entityName))
                entityName = GenericRecord.EntityName;
            XmlFormatter formatter = new XmlFormatter(entityName, Xml.XmlFormatter.TargetNamespace, "utf-8");
            return formatter.DictionaryToXmlString(this);
        }

        public static GenericRecord DeserializeFromXml(string xmlString, string entityName)
        {
            GenericRecord gr = new GenericRecord();

            XmlFormatter.WriteXmlToDictionary(xmlString, gr);

            return gr;
        }

        #endregion

        #region static

        public static GenericRecord Parse(string json)
        {
            var dic= (IDictionary<string, object>)JsonSerializer.Deserialize(json,typeof(Dictionary<string,object>));
            GenericRecord record = new GenericRecord(dic);
            return record;
        }

        public static GenericRecord Parse(DataRow dr)
        {
            GenericRecord record = new GenericRecord(dr);
            return record;
        }

        public static GenericRecord Parse(DataTable dt)
        {
            if (dt == null)
            {
                throw new ArgumentNullException("dt");
            }

            GenericRecord dic = new GenericRecord();

            if (dt.Rows.Count==0)
                return dic;

            if (dt.Rows.Count > 1)
            {
                int i = 0;
                foreach (DataRow dr in dt.Rows)
                {
                    GenericRecord row = new GenericRecord();
                    foreach (DataColumn col in dt.Columns)
                    {
                        row[col.ColumnName] = dr[col];
                    }
                    dic[FormatRowIndex(i)] = row;
                    i++;
                }
                dic._IsRecordSet = true;
            }
            else
            {
                DataRow row = dt.Rows[0];
                foreach (DataColumn col in dt.Columns)
                {
                    dic[col.ColumnName] = row[col];
                }
            }
            return dic;
        }

        public static List<GenericRecord> ParseList(DataTable dt)
        {
            if (dt == null)
            {
                throw new ArgumentNullException("dt");
            }
            List<GenericRecord> dic = new List<GenericRecord>();
            foreach (DataRow dr in dt.Rows)
            {
                GenericRecord row = new GenericRecord();
                foreach (DataColumn col in dt.Columns)
                {
                    row[col.ColumnName] = dr[col];
                }
                dic.Add(row);
            }
            return dic;
        }

        public static GenericRecord ParseKeyValue(params object[] keyValueParameters)
        {
            if (keyValueParameters == null)
                return null;
            int count = keyValueParameters.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            GenericRecord record = new GenericRecord();
            for (int i = 0; i < count; i++)
            {
                record[keyValueParameters[i].ToString()] = keyValueParameters[++i];
            }

            return record;
        }

        // public static GenericRecord ParseKeyValue<T>(NameValueCollection keyValue)
        //{

        //    GenericRecord record = new GenericRecord();
        //    for (int i = 0; i < keyValue.Count; i++)
        //    {

        //        record[keyValue[i]] = keyValue[++i];
        //    }

        //    return record;
        //}

        public static object[] StringToKeyValue(string value, char splitterOutside, char splitterInside)
        {
            value = BaseConverter.Escape(value,'%',  splitterOutside.ToString(), splitterInside.ToString());

            List<object> o = new List<object>();
            string[] items = value.Split(splitterOutside);
            foreach (string s in items)
            {
                string[] args = s.Split(splitterInside);
                if (args.Length != 2)
                {
                    throw new ArgumentException("values parameter not correct, Not match key value arguments");
                }
                args[0] = BaseConverter.UnEscape(args[0], '%', splitterOutside.ToString(), splitterInside.ToString());
                args[1] = BaseConverter.UnEscape(args[1], '%', splitterOutside.ToString(), splitterInside.ToString());

                o.Add(args[0]);
                o.Add(args[1]);
            }
            return o.ToArray();
        }

        #endregion

        #region ctor

        public GenericRecord()
        {
            
        }
        public GenericRecord(Stream stream, IBinaryStreamer streamer)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("GenericRecord.stream");
            }
            EntityRead(stream, streamer);
        }

        public GenericRecord(SerializeInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException("GenericRecord.info");
            }
            foreach (var item in info.Data)
            {
                this[item.Key] = item.Value.ItemValue;
            }
        }

        public GenericRecord(DataRow dr)
        {
            if (dr == null)
            {
                throw new ArgumentNullException("GenericRecord.dr");
            }
            LoadDictionaryFromDataRow(this, dr);
        }

        public GenericRecord(DataRow dr,string [] columns)
        {
            if (dr == null)
            {
                throw new ArgumentNullException("GenericRecord.dr");
            }
            if (columns == null)
            {
                throw new ArgumentNullException("GenericRecord.columns");
            }
            LoadDictionaryFromDataRow(this, dr, columns);
        }
               
        public GenericRecord(IDictionary dr)
        {
            if (dr == null)
            {
                throw new ArgumentNullException("GenericRecord.dr");
            }
            Copy(dr);
        }

        public GenericRecord(IDictionary<string, object> dr)
        {
            if (dr == null)
            {
                throw new ArgumentNullException("GenericRecord.dr");
            }
            Copy(dr);
        }

        void Copy(IDictionary dic)
        {
            foreach (DictionaryEntry entry in dic)
            {
                this[entry.Key.ToString()] = entry.Value;
            }
        }

        void Copy(IDictionary<string,object> dic)
        {
            foreach (var entry in dic)
            {
                this[entry.Key] = entry.Value;
            }
        }
        public void Prepare(DataRow dr)
        {
            this.ToKeyValue(dr);
        }
        #endregion

        #region DataUtil

        static void LoadDictionaryFromDataRow(IDictionary instance, DataRow dr, string[] columns)
        {
            if (dr == null || columns == null)
                return;
            for (int i = 0; i < columns.Length; i++)
            {
                string colName = columns[i];
                instance[colName] = dr[colName];
            }
        }

        static void LoadDictionaryFromDataRow(IDictionary instance, DataRow dr)
        {
            if (dr == null)
                return;
            DataTable dt = dr.Table;
            if (dt == null)
                return;
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                string colName = dt.Columns[i].ColumnName;
                instance[colName] = dr[colName];
            }
        }

        public static DataRow IDictionaryToDataRow(IDictionary/*<string, object>*/ h, string tableName = null)
        {
            if (h == null || h.Count == 0)
                return null;

            string[] names = new string[h.Count];
            object[] values = new object[h.Count];
            h.Keys.CopyTo(names, 0);
            h.Values.CopyTo(values, 0);

            DataTable dt = new DataTable(tableName);
            for (int i = 0; i < h.Count; i++)
            {
                dt.Columns.Add(names[i]);
            }
            dt.Rows.Add(values);

            return dt.Rows[0];
        }

        public static IDictionary<string, object> ToDictionary(DataTable dt)
        {

            if (dt == null)
            {
                throw new ArgumentNullException("dt");
            }

            IDictionary<string, object> dic = new Dictionary<string, object>();
            int i = 0;
            foreach (DataRow dr in dt.Rows)
            {
                IDictionary<string, object> row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    row[col.ColumnName] = dr[col];
                }
                dic[i.ToString()] = row;
            }
            return dic;
        }

        #endregion

        #region Load

        public void Load(DataRow dr)
        {
            this.Clear();
            LoadDictionaryFromDataRow(this, dr);
        }

        public void Load(GenericRecord value)
        {
            this.Clear();
            Copy((IDictionary<string,object>)value);
        }

        public void Load(IDictionary dic)
        {
            this.Clear();
            Copy(dic);
        }
        #endregion

        #region Properties

        public bool IsReadOnly
        {
            get { return false; }
        }

        public IDictionary<string, object> Record
        {
            get { return this; }
        }

        public GenericRecord GetRecord(int i)
        {
            return GetValue<GenericRecord>(FormatRowIndex(i));
        }

        public static string FormatRowIndex(int i)
        {
            return i.ToString();
        }

        bool? _IsRecordSet;
        public bool IsRecordSet
        {
            get
            {
                if (_IsRecordSet == null)
                {
                    if (IsEmpty)
                        return false;
                    _IsRecordSet = Record.Where(v => v.Value != null && typeof(IDictionary<string,object>).IsAssignableFrom(v.Value.GetType())) != null;
                }
                if (_IsRecordSet.HasValue)
                    return _IsRecordSet.Value;
                return false;
            }
        }

        #endregion

        #region Values

        /// <summary>
        /// GetValue
        /// </summary>
        /// <param name="field">the column name in data row</param>
        /// <returns></returns>
        public object GetValue(string field)
        {
            object value = null;
            base.TryGetValue(field, out value);
            return value;
        }

        /// <summary>
        /// GetValue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">the column name in data row</param>
        /// <returns>T</returns>
        public T GetValue<T>(string field)
        {
            object value = null;
            if (base.TryGetValue(field, out value))
            {
                return (T)GenericTypes.Convert<T>(value);
            }
            return default(T);
        }

        /// <summary>
        /// GetValue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">the column name in data row</param>
        /// <param name="defaultValue"></param>
        /// <returns>if null or error return defaultValue</returns>
        public T GetValue<T>(string field, T defaultValue)
        {
            object value = null;
            if (base.TryGetValue(field, out value))
            {
                return GenericTypes.Convert<T>(value/*base[field]*/, defaultValue);
            }
            return defaultValue;
        }

        
        /// <summary>
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">The key whose value to get.</param>
        /// <param name="value">
        ///     When this method returns, the value associated with the specified key, if
        ///     the key is found; otherwise, the default value for the type of the value
        ///     parameter. This parameter is passed uninitialized.
        ///</param>
        /// <returns>
        ///     true if the object that implements System.Collections.Generic.IDictionary
        ///     contains an element with the specified key; otherwise, false.
        ///</returns>
        ///<exception cref="System.ArgumentNullException">key is null.</exception>
        public bool TryGetValue<T>(string field, out T value)
        {
 
            object ovalue = null;
            if (base.TryGetValue(field, out ovalue))
            {
                try
                {
                    value = (T)GenericTypes.Convert<T>(ovalue);
                    return true;
                }
                catch { }
            }
            value = default(T);
            return false;
        }

        /// <summary>
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <param name="field">The key whose value to get.</param>
        /// <param name="value">
        ///     When this method returns, the value associated with the specified key, if
        ///     the key is found; otherwise, the default value for the type of the value
        ///     parameter. This parameter is passed uninitialized.
        ///</param>
        /// <returns>
        ///     true if the object that implements System.Collections.Generic.IDictionary
        ///     contains an element with the specified key; otherwise, false.
        ///</returns>
        ///<exception cref="System.ArgumentNullException">key is null.</exception>
        public new bool TryGetValue(string field, out object value)
        {
            return base.TryGetValue(field, out value);
        }

        /// <summary>
        ///  Gets the value associated with the specified key.
        /// </summary>
        /// <param name="field">The key whose value to get.</param>
        /// <param name="type"></param>
        /// <param name="value">
        ///     When this method returns, the value associated with the specified key, if
        ///     the key is found; otherwise, the default value for the type of the value
        ///     parameter. This parameter is passed uninitialized.
        ///</param>
        /// <returns>
        ///     true if the object that implements System.Collections.Generic.IDictionary
        ///     contains an element with the specified key; otherwise, false.
        ///</returns>
        ///<exception cref="System.ArgumentNullException">key is null.</exception>
        public bool TryGetValue(string field, Type type, out object value)
        {

            object val = null;
            if (base.TryGetValue(field, out val))
            {
                value = GenericTypes.Convert(val, type);
                return true;
            }
            value = GenericTypes.Default(type);
            return false;
        }
       

        /// <summary>
        /// SetValue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">the column name in data row</param>
        /// <param name="value">the T value to insert</param>
        public void SetValue<T>(string field, T value)
        {
            base[field] = value;
        }

        /// <summary>
        /// SetValue
        /// </summary>
        /// <param name="field">the column name in data row</param>
        /// <param name="value">the value to insert</param>
        public void SetValue(string field, object value)
        {
            base[field] = value;
        }

        /// <summary>
        /// SetValue
        /// </summary>
        /// <param name="field">the column name in data row</param>
        /// <param name="value">the value to insert</param>
        public void SetValue(object field, object value)
        {
            if (field == null)
                return;
            base[field.ToString()] = value;
        }
        #endregion

        #region compare

        /// <summary>
        /// Compare Values between current field value and valueToComparee
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field">the column name in data row</param>
        /// <param name="valueToCompare">the T value to Compare</param>
        /// <returns></returns>
        public bool CompareValues<T>(string field, T valueToCompare) //where T : class 
        {
            T x = GetValue<T>(field);
            return EqualityComparer<T>.Default.Equals(x, valueToCompare);
        }

        /// <summary>
        /// Compare Values between current field value and valueToComparee
        /// </summary>
        /// <param name="field">the column name in data row</param>
        /// <param name="valueToCompare">the value to Compare</param>
        /// <returns></returns>
        public bool CompareValues(string field, object valueToCompare)
        {
            object x = GetValue(field);
            //return EqualityComparer<object>.Default.Equals(x, valueToCompare);
            return EqualityComparer<object>.Default.Equals(x, valueToCompare);
        }

        /// <summary>
        ///  Compare  Values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Compare<T>(T x, T y)
        {
            return EqualityComparer<T>.Default.Equals(x, y);
        }

        /// <summary>
        /// Compare between 2 GenericRecords
        /// </summary>
        /// <param name="gr"></param>
        /// <returns></returns>
        public bool Compare(GenericRecord gr)
        {
            foreach (KeyValuePair<string,object> entry in gr)
            {
                bool ok = CompareValues(entry.Key.ToString(), entry.Value);
                if (!ok)
                    return false;
            }
            return true;
        }

        #endregion
        
        #region public

        /// <summary>
        /// Get Indicate if GenericRecord is Empty
        /// </summary>
        public bool IsEmpty
        {
            get { return this.Count == 0; }
        }

        /// <summary>
        /// Convert GenericRecord to Json string
        /// </summary>
        /// <returns></returns>
        public string ToJson(bool pretty = false)
        {
            var json = JsonSerializer.Serialize(this.Record, pretty);
            return json;
        }

        /// <summary>
        /// Convert GenericRecord to DataRow
        /// </summary>
        /// <returns></returns>
        public DataRow ToDataRow()
        {
            return IDictionaryToDataRow(this, EntityName);
        }

        /// <summary>
        /// Add current record to specified DataTable using fields list
        /// </summary>
        /// <param name="table"></param>
        /// <param name="enableIdentity"></param>
        public void AddTo(DataTable table,bool enableIdentity)
        {
            if (table == null || table.Columns.Count == 0)
            {
                throw new ArgumentException("AddDataRow.table is null or empty");
            }
 
            DataRow dr= table.NewRow();

            foreach (DataColumn col in table.Columns)
            {
                if (!enableIdentity && col.AutoIncrement)
                    continue;

                if (this.ContainsKey(col.ColumnName))
                    dr[col.ColumnName] = this[col.ColumnName];
                else
                {
                    if (col.AllowDBNull)
                        dr[col.ColumnName] = null;
                    else if(Types.IsNumericType(col.DataType))
                        dr[col.ColumnName] = 0;
                    else if (col.DataType==typeof(Guid))
                        dr[col.ColumnName] = Guid.Empty;
                    else
                        dr[col.ColumnName] = string.Empty;
                }
            }
            table.Rows.Add(dr);
        }

        /// <summary>
        /// Display data as Vertical view, 
        /// </summary>
        /// <param name="headerName"></param>
        /// <param name="headerValue"></param>
        /// <returns></returns>
        public string Print(string headerName, string headerValue)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendFormat("{0}\t{1}", headerName, headerValue);

            foreach (KeyValuePair<string, object> entry in this)
            {
                sb.AppendLine();
                sb.AppendFormat("{0}: {1}", entry.Key, entry.Value);
            }
            sb.AppendLine();
            return sb.ToString();
        }

       /// <summary>
       /// Get this as sorted.
       /// </summary>
       /// <returns></returns>
        public IOrderedEnumerable<KeyValuePair<string, object>> Sorted()
        {
            var sortedDict = from entry in this orderby entry.Key ascending select entry;
            return sortedDict;
        }

        #endregion

        #region ISerialJson

        public string EntityWrite(IJsonSerializer serializer, bool pretty = false)
        {
            if (serializer == null)
                serializer = new JsonSerializer( JsonSerializerMode.Write,null);
            if(pretty)
            {
                var json= serializer.Write(this.Record, this.GetType().BaseType);
                return JsonSerializer.Print(json);
            }
            return serializer.Write(this.Record, this.GetType().BaseType);
        }

        public object EntityRead(string json, IJsonSerializer serializer)
        {
            if (serializer == null)
                serializer = new JsonSerializer(JsonSerializerMode.Read, null);
            IDictionary<string, object> o = serializer.Read<IDictionary<string, object>>(json);

            IDictionary<string, object> gr = (IDictionary<string, object>)o;

            if (gr != null)
            {
                Load(gr as IDictionary);
            }
            return o;
        }

        #endregion
        
        #region IEntityDictionary

        public IDictionary<string,object> ToDictionary()
        {
            return this; 
        }

        public IDictionary EntityDictionary()
        {
             return this; 
        }

        public virtual Type EntityType
        {
            get { return typeof(GenericRecord); }
        }

        public void EntityWrite(Stream stream, IBinaryStreamer streamer)
        {
            bool rwSerialType = true;

            if (streamer == null)
                streamer = new BinaryStreamer(stream);
            else
                rwSerialType = false;

            ((BinaryStreamer)streamer).WriteDictionaryAsEntity(this.Record, rwSerialType, rwSerialType);

            //streamer.WriteValue(this, this.GetType().BaseType);
        }

        public void EntityRead(Stream stream, IBinaryStreamer streamer)
        {
            bool rwSerialType = true;

            if (streamer == null)
                streamer = new BinaryStreamer(stream);
            else
                rwSerialType = false;

            this.Clear();
            
            ((BinaryStreamer)streamer).TryReadEntityToDictionary(this, rwSerialType, rwSerialType);

            //((BinaryStreamer)streamer).ReadToDictionary(this,true);
        }

        public NetStream ToStream()
        {
            NetStream stream = new NetStream();
            EntityWrite(stream, null);
            return stream;
        }

        #endregion

    }
}
 