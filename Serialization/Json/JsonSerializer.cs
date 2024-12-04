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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Specialized;
using System.Text;
using Nistec.Generic;
using Nistec.Runtime;
#pragma warning disable  CS1591

namespace Nistec.Serialization
{
   

    public class JsonSerializer : IJsonSerializer
    {
        public JsonSerializer() { }

        #region IJsonSerializer

        JsonWriter writer;
        JsonReader reader;

        public JsonSerializer(JsonSerializerMode mode, JsonSettings settings)
        {
            if (mode == JsonSerializerMode.Write || mode == JsonSerializerMode.Both)
            {
                writer = new JsonWriter(settings);
            }
            if (mode == JsonSerializerMode.Read || mode == JsonSerializerMode.Both)
            {
                reader = new JsonReader(settings);
            }

            if (settings == null)
                settings = JsonSerializer.DefaultOption;
        }

        void EnsureWrtie()
        {
            if (writer == null)
            {
                throw new Exception("Json writer not initilaized");
            }
        }
        void EnsureRead()
        {
            if (reader == null)
            {
                throw new Exception("Json reader not initilaized");
            }
        }

        /// <summary>
        /// Create a json representation for an object with parameter override on this call
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public string Write(object obj, Type baseType)
        {
            EnsureWrtie();

            if (obj == null)
                return "null";

            return writer.ConvertToJson(obj, baseType);

        }

        public string Write(object obj)
        {
            EnsureWrtie();

            if (obj == null)
                return "null";

            return writer.ConvertToJson(obj, null);
        }

        public void WriteToken(string name,object obj, Type baseType)
        {
            writer.WriteToken(name, obj, baseType);
        }
        public void WriteToken(string name, object obj)
        {
            writer.WriteToken(name, obj, null);
        }

        public string WriteOutput(bool pretty = false)
        {
            if (pretty)
            {
                var json = writer.WriteOutput(true);
                return JsonConverter.PrintJson(json);
            }
            return writer.WriteOutput(true);
        }

        /// <summary>
        /// Create a typed generic object from the json with parameter override on this call
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public T Read<T>(string json)
        {
            EnsureRead();
            return reader.ToObject<T>(json);
        }

        public object Read(string json, Type type)
        {
            EnsureRead();
            return reader.ToObject(json, type);
        }

        public void ParseTo(Dictionary<string, object> d, string json)
        {
            JsonParser.ParseTo(d, json, false);
        }
        public void ParseTo(Dictionary<string, string> d,string json)
        {
            JsonParser.ParseTo(d,json, false);
        }
        public void ParseTo<T>(Dictionary<string, T> d, string json)
        {
            JsonParser.ParseTo(d, json, false);
        }
        public void ParseTo<T>(List<T> d, string json)
        {
            JsonParser.ParseTo<T>(d, json, false);
        }
        //public IDictionary<string,object> ParseToDictionary(string json)
        //{
        //    return JsonParser.ParseToDictionary(json, false);
        //}
        //public IDictionary<string, string> ParseToDictionaryString(string json)
        //{
        //    return JsonParser.ParseToDictionaryString(json, false);
        //}

        #endregion

        #region static
        /// <summary>
        /// Default serializer option.
        /// </summary>
        public static JsonSettings DefaultOption = new JsonSettings();

        /// <summary>
        /// Default serializer with date format option.
        /// </summary>
        public static JsonSettings DefaultSettingsFormat(JsonDateFormat format)
        {
            return new JsonSettings() { JsonDateFormat = format };
        }

        /// <summary>
        /// Create a json from object using default <see cref="JsonSettings"/>.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Serialize(object obj)
        {
            return Serialize(obj, null, JsonSerializer.DefaultOption, JsonFormat.None);
        }

        /// <summary>
        /// Create a json from object using <see cref="JsonSettings"/> and format(optional) with Indented format.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="pretty"></param>
        /// <returns></returns>
        public static string Serialize(object obj, bool pretty)
        {
            return Serialize(obj, null, JsonSerializer.DefaultOption, pretty ? JsonFormat.Indented : JsonFormat.None);
        }

        /// <summary>
        /// Create a json from object using <see cref="JsonSettings"/> and format(optional).
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="settings"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string Serialize(object obj, JsonSettings settings, JsonFormat format = JsonFormat.None)
        {
            return Serialize(obj, null, settings, format);
        }

        /// <summary>
        /// Create a json from object using <see cref="JsonSettings"/> and format(optional).
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="baseType"></param>
        /// <param name="settings"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string Serialize(object obj, Type baseType, JsonSettings settings, JsonFormat format)
        {
            if (settings == null)
                settings = JsonSerializer.DefaultOption;

            
            Type t = null;

            if (obj == null)
                return "null";

            Type type = obj.GetType();

            if (type.IsGenericType)
                t = JsonActivator.Get.GetGenericTypeDefinition(type);
            if (t == typeof(Dictionary<,>) || t == typeof(List<>))
                settings.UseTypesExtension = false;
            if (typeof(IKeyValue).IsAssignableFrom(t))
                settings.UseTypesExtension = false;

            // enable extensions when you can deserialize anon types
            if (settings.EnableAnonymousTypes)
            {
                settings.UseExtensions = false;
                settings.UseTypesExtension = false;
            }
            string json = JsonWriter.Get(settings).ConvertToJson(obj, baseType);
            if (format == JsonFormat.Indented)
                return JsonConverter.PrintJson(json);
            return json;
        }

        public static string ConvertToJson(object[] keyValueParameters, JsonSettings settings, JsonFormat format= JsonFormat.None)
        {
            if (settings == null)
                settings = JsonSerializer.DefaultOption;

            if (settings.EnableAnonymousTypes)
            {
                settings.UseExtensions = false;
                settings.UseTypesExtension = false;
            }
            string json = JsonWriter.Get(settings).ConvertToJson(keyValueParameters);
            if (format == JsonFormat.Indented)
                return JsonConverter.PrintJson(json);
            return json;
        }


        public static byte[] ToBinary(string json)
        {
            return ToBinary(json, null);
        }

        public static byte[] ToBinary(string json, string encoding)
        {
            if (json == null)
            {
                throw new ArgumentNullException("ToBinary.json");
            }
            if (encoding == null)
                return Encoding.UTF8.GetBytes(json);
            return Encoding.GetEncoding(encoding).GetBytes(json);
        }

        /// <summary>
        /// Parse json tring and convert it to object.
        /// </summary>
        /// <param name="json"></param>
        /// <param name="returnType"></param>
        /// <returns></returns>
        public static object Parse(string json, Type returnType)
        {
            return JsonParser.Parse(json, returnType, JsonSerializer.DefaultOption.IgnoreCaseOnDeserialize);
        }

        /// <summary>
        /// Create a dynamic object from the json string
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static dynamic ToDynamic(string json)
        {
            return new DynamicJson(json);
        }

        /// <summary>
        /// Create a dictionary object from the json string
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static IDictionary<string, object> ToDictionary(string json)
        {
            var parse = JsonSerializer.Deserialize(json,typeof(Dictionary<string, object>));
            if (parse == null)
                return new Dictionary<string, object>();
            return (IDictionary<string, object>)parse;
        }

        /// <summary>
        /// Create a typed generic object from the json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string json)
        {
            return JsonReader.Get(DefaultOption).ToObject<T>(json);
        }
        /// <summary>
        /// Create a typed generic object from the json with parameter override on this call
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string json, JsonSettings settings)
        {
            return JsonReader.Get(settings).ToObject<T>(json);
        }
        ///// <summary>
        ///// Create an object from the json
        ///// </summary>
        ///// <param name="json"></param>
        ///// <returns></returns>
        //public static object Deserialize(string json)
        //{
        //    return JsonReader.Get(DefaultOption).ToObject(json, null);
        //}
        /// <summary>
        /// Create an object from the json with settings parameter.
        /// </summary>
        /// <param name="json"></param>
        /// <param name="type"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static object Deserialize(string json, Type type, JsonSettings settings)
        {
            return JsonReader.Get(settings).ToObject(json, type);
        }
        /// <summary>
        /// Create an object of type from the json
        /// </summary>
        /// <param name="json"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object Deserialize(string json, Type type)
        {
            return JsonReader.Get(DefaultOption).ToObject(json, type);
        }


        /// <summary>
        /// Parse and clone to a new object.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object ParseAndCopy(object obj, Type type)
        {
            return JsonReader.Get(DefaultOption).ToObject(Serialize(obj), type);
        }
        /// <summary>
        /// Parse and clone to a new object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T ParseAndCopy<T>(T obj)
        {
            return JsonReader.Get(DefaultOption).ToObject<T>(Serialize(obj));
        }

        /// <summary>
        /// Format json. 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Print(string input)
        {
            return JsonConverter.PrintJson(input);
        }
        /// <summary>
        /// Register custom type handlers for your own types not natively handled by Nistec.Runtime.Json
        /// </summary>
        /// <param name="type"></param>
        /// <param name="serializer"></param>
        /// <param name="JsonReader"></param>
        public static void RegisterCustomType(Type type, SerializeFunc serializer, DeserializeFunc JsonReader)
        {
            JsonActivator.Get.RegisterCustomType(type, serializer, JsonReader);
        }

        /// <summary>
        /// Clear the internal reflection cache so you can start from new (you will loose performance)
        /// </summary>
        public static void ClearActivatorCache()
        {
            JsonActivator.Get.ClearCache();
        }
        #endregion

        #region formatter
        /*
        internal static string Indent = "   ";

        internal static void AppendIndent(StringBuilder sb, int count)
        {
            for (; count > 0; --count) sb.Append(Indent);
        }

        internal static string PrintIndent(string input)
        {
            var output = new StringBuilder();
            int depth = 0;
            int len = input.Length;
            char[] chars = input.ToCharArray();
            for (int i = 0; i < len; ++i)
            {
                char ch = chars[i];

                if (ch == '\"') // found string span
                {
                    bool str = true;
                    while (str)
                    {
                        output.Append(ch);
                        ch = chars[++i];
                        if (ch == '\\')
                        {
                            output.Append(ch);
                            ch = chars[++i];
                        }
                        else if (ch == '\"')
                            str = false;
                    }
                }

                switch (ch)
                {
                    case '{':
                    case '[':
                        output.Append(ch);
                        output.AppendLine();
                        AppendIndent(output, ++depth);
                        break;
                    case '}':
                    case ']':
                        output.AppendLine();
                        AppendIndent(output, --depth);
                        output.Append(ch);
                        break;
                    case ',':
                        output.Append(ch);
                        output.AppendLine();
                        AppendIndent(output, depth);
                        break;
                    case ':':
                        output.Append(" : ");
                        break;
                    default:
                        if (!char.IsWhiteSpace(ch))
                            output.Append(ch);
                        break;
                }
            }

            return output.ToString();
        }

        */
        #endregion
#if(false)
        #region Writer

        internal sealed class JsonWriter
        {
            public static JsonWriter Get(JsonSettings settings)
            {
                return new JsonWriter(settings);
            }

            private StringBuilder _output = new StringBuilder();
            private StringBuilder _before = new StringBuilder();
            readonly int _MAX_DEPTH = 20;
            int _current_depth = 0;
            private Dictionary<string, int> _GlobalTypes = new Dictionary<string, int>();
            private Dictionary<object, int> _CircularItems = new Dictionary<object, int>();
            private JsonSettings _Option;
            private bool _useEscapedUnicode = false;
            private bool _isCircular = false;

          
            internal JsonWriter(JsonSettings settings)
            {
                _Option = settings ?? JsonSerializer.DefaultOption;
                _useEscapedUnicode = _Option.UseEscapedUnicode;
            }

            internal string ConvertToJson(object obj, Type baseType)
            {
                WriteValue(obj, baseType);

                string str = "";
                if (_Option.UseTypesExtension && _GlobalTypes != null && _GlobalTypes.Count > 0)
                {
                    StringBuilder sb = _before;
                    if (_isCircular)
                        sb.Append("\"$circular\":true,");
                    sb.Append("\"$types\":{");
                    bool pendingSeparator = false;
                    foreach (var kv in _GlobalTypes)
                    {
                        if (pendingSeparator) sb.Append(',');
                        pendingSeparator = true;
                        sb.Append('\"');
                        sb.Append(kv.Key);
                        sb.Append("\":\"");
                        sb.Append(kv.Value);
                        sb.Append('\"');
                    }
                    sb.Append("},");
                    sb.Append(_output.ToString());
                    str = sb.ToString();
                }
                else
                    str = _output.ToString();

                return str;
            }

            private void WriteValue(object obj, Type baseType)
            {
                if (obj == null || obj is DBNull)
                {
                    _output.Append("null");
                    return;
                }
                Type type = baseType == null ? obj.GetType() : baseType;

                switch (type.Name)
                {
                    case "Boolean":
                        _output.Append(((bool)obj) ? "true" : "false"); break;// conform to standard
                    case "Byte":
                    case "UInt16":
                    case "UInt32":
                    case "UInt64":
                    case "SByte":
                    case "Int16":
                    case "Int32":
                    case "Int64":
                    case "Single":
                    case "Double":
                    case "Decimal":
                        _output.Append(((IConvertible)obj).ToString(NumberFormatInfo.InvariantInfo)); break;
                    case "Char":
                    case "String":
                        WriteString(obj.ToString()); break;
                    case "DateTime":
                        WriteDateTime((DateTime)obj); break;
                    case "TimeSpan":
                        WriteTimeSpan((TimeSpan)obj); break;
                    case "Guid":
                        WriteGuid((Guid)obj); break;
                    case "Byte[]":
                        WriteBytes((byte[])obj); break;
                    case "Char[]": 
                        WriteArray<Char>((Char[])obj);break;
                    case "Int16[]":
                        WriteArray<Int16>((Int16[])obj);break;
                    case "Int32[]":
                        WriteArray<Int32>((Int32[])obj);break;
                    case "Int64[]":
                        WriteArray<Int64>((Int64[])obj);break;
                    case "String[]":
                        WriteArray<String>((String[])obj);break;
                    case "Object[]": 
                        WriteArray<Object>((Object[])obj);break;
                    default:
                        if (SerializeTools.IsGenericKeyStringDictionary(type))
                        {
                            if (_Option.UseExtraKeyValueDictionary)
                                WriteDictionaryKeyValue((IDictionary)obj);
                            else
                                WriteStringDictionary((IDictionary)obj);
                        }
                        else if (SerializeTools.IsDictionary(type))
                        {
                            if (_Option.UseExtraKeyValueDictionary)
                                WriteDictionaryKeyValue((IDictionary)obj);
                            else
                                WriteDictionary((IDictionary)obj);
                        }
                        
                        else if (SerializeTools.IsDataSet(type))
                            WriteDataset((DataSet)obj);

                        else if (SerializeTools.IsDataTable(type))
                            this.WriteDataTable((DataTable)obj);

                        else if (type== typeof(DataRow))
                            this.WriteDataRow((DataRow)obj);

                        else if (type== typeof(StringDictionary))
                            WriteStringDictionary((StringDictionary)obj);

                        else if (type== typeof(NameValueCollection))
                            WriteNameValue((NameValueCollection)obj);

                        else if (SerializeTools.IsEnumerable(type))
                            WriteArray((IEnumerable)obj);

                        else if (type.IsEnum)
                            WriteEnum((Enum)obj);

                        else if (JsonActivator.Get.IsTypeRegistered(type))
                            WriteCustom(obj);

                        else if (SerializeTools.IsISerialJson(type))
                            WriteSerialJson((ISerialJson)obj);
                        else
                            WriteObject(obj, baseType);

                        break;
                }

            }

            private void WriteSerialJson(ISerialJson serialJson)
            {

                _output.Append(serialJson.EntityWrite(null));
            }

            private void WriteNameValue(NameValueCollection nameValueCollection)
            {
                _output.Append('{');

                bool pendingSeparator = false;

                foreach (string key in nameValueCollection)
                {
                    if (pendingSeparator) _output.Append(',');

                    WritePair(key, nameValueCollection[key]);

                    pendingSeparator = true;
                }
                _output.Append('}');
            }

            private void WriteStringDictionary(StringDictionary stringDictionary)
            {
                _output.Append('{');

                bool pendingSeparator = false;

                foreach (DictionaryEntry entry in stringDictionary)
                {
                    if (pendingSeparator) _output.Append(',');

                    WritePair((string)entry.Key, entry.Value);

                    pendingSeparator = true;
                }
                _output.Append('}');
            }

            private void WriteCustom(object obj)
            {
                SerializeFunc s;
                JsonActivator.Get._customSerializer.TryGetValue(obj.GetType(), out s);
                WriteStringValue(s(obj));
            }

            private void WriteEnum(Enum e)
            {
                if (_Option.UseEnumValues)
                    WriteValue(Convert.ToInt32(e), null);
                else
                    WriteStringValue(e.ToString());
            }

            private void WriteGuid(Guid g)
            {
                if (_Option.UseBinaryGuid == false)
                    WriteStringValue(g.ToString());
                else
                    WriteBytes(g.ToByteArray());
            }

            private void WriteBytes(byte[] bytes)
            {

                WriteStringValue(Convert.ToBase64String(bytes, 0, bytes.Length, Base64FormattingOptions.None));
            }

            private void WriteDateTime(DateTime dateTime)
            {
                // datetime format standard : yyyy-MM-ddTHH:mm:ss
                DateTime dt = dateTime;
                if (_Option.UseUTCDateTime)
                    dt = dateTime.ToUniversalTime();

                _output.Append('\"');
                _output.Append(dt.Year.ToString("0000", NumberFormatInfo.InvariantInfo));
                _output.Append('-');
                _output.Append(dt.Month.ToString("00", NumberFormatInfo.InvariantInfo));
                _output.Append('-');
                _output.Append(dt.Day.ToString("00", NumberFormatInfo.InvariantInfo));
                _output.Append('T');
                _output.Append(dt.Hour.ToString("00", NumberFormatInfo.InvariantInfo));
                _output.Append(':');
                _output.Append(dt.Minute.ToString("00", NumberFormatInfo.InvariantInfo));
                _output.Append(':');
                _output.Append(dt.Second.ToString("00", NumberFormatInfo.InvariantInfo));
                if (_Option.EnableDateTimeMilliseconds)
                {
                    _output.Append('.');
                    _output.Append(dt.Millisecond.ToString("000", NumberFormatInfo.InvariantInfo));
                }
                if (_Option.UseUTCDateTime)
                    _output.Append('Z');

                _output.Append('\"');
            }

            private void WriteTimeSpan(TimeSpan time)
            {
                _output.Append('\"');
                _output.Append(time.Hours.ToString("00", NumberFormatInfo.InvariantInfo));
                _output.Append(':');
                _output.Append(time.Minutes.ToString("00", NumberFormatInfo.InvariantInfo));
                _output.Append(':');
                _output.Append(time.Seconds.ToString("00", NumberFormatInfo.InvariantInfo));
                if (_Option.EnableDateTimeMilliseconds)
                {
                    _output.Append('.');
                    _output.Append(time.Milliseconds.ToString("000", NumberFormatInfo.InvariantInfo));
                }
                _output.Append('\"');
            }
            private JsonSchema GetSchema(DataTable ds)
            {
                if (ds == null) return null;

                JsonSchema m = new JsonSchema();
                m.Info = new List<string>();
                m.Name = ds.TableName;

                foreach (DataColumn c in ds.Columns)
                {
                    m.Info.Add(ds.TableName);
                    m.Info.Add(c.ColumnName);
                    m.Info.Add(c.DataType.ToString());
                }
                // FEATURE : serialize relations and constraints here

                return m;
            }

            private JsonSchema GetSchema(DataSet ds)
            {
                if (ds == null) return null;

                JsonSchema m = new JsonSchema();
                m.Info = new List<string>();
                m.Name = ds.DataSetName;

                foreach (DataTable t in ds.Tables)
                {
                    foreach (DataColumn c in t.Columns)
                    {
                        m.Info.Add(t.TableName);
                        m.Info.Add(c.ColumnName);
                        m.Info.Add(c.DataType.ToString());
                    }
                }
                return m;
            }

            private string GetXmlSchema(DataTable dt)
            {
                using (var writer = new StringWriter())
                {
                    dt.WriteXmlSchema(writer);
                    return dt.ToString();
                }
            }

            private void WriteDataset(DataSet ds)
            {
                _output.Append('{');
                if (_Option.UseExtensions)
                {
                    WritePair("$schema", _Option.UseDatasetSchema ? (object)GetSchema(ds) : ds.GetXmlSchema());
                    _output.Append(',');
                }
                bool tablesep = false;
                foreach (DataTable table in ds.Tables)
                {
                    if (tablesep) _output.Append(',');
                    tablesep = true;
                    WriteDataTableData(table);
                }
                // end dataset
                _output.Append('}');
            }

            private void WriteDataRowAsArray(DataRow row)
            {

                DataTable table = row.Table;
                _output.Append('\"');
                _output.Append(table.TableName);
                _output.Append("\":[");

                DataColumnCollection cols = table.Columns;

                _output.Append('{');

                bool pendingSeperator = false;
                foreach (DataColumn column in cols)
                {
                    if (pendingSeperator) _output.Append(',');
                    WritePair(column.ColumnName, row[column], column.DataType);
                    pendingSeperator = true;
                }
                _output.Append('}');
                _output.Append(']');
            }

            private void WriteDataRowData(DataRow row)
            {

                DataTable table = row.Table;

                DataColumnCollection cols = table.Columns;
                bool pendingSeperator = false;
                foreach (DataColumn column in cols)
                {
                    if (pendingSeperator) _output.Append(',');
                    WritePair(column.ColumnName, row[column], column.DataType);
                    pendingSeperator = true;
                }
            }

            void WriteDataRow(DataRow row)
            {
                this._output.Append('{');
                if (_Option.UseExtensions)
                {
                    var dt = row.Table;
                    this.WritePair("$schema", _Option.UseDatasetSchema ? (object)this.GetSchema(dt) : this.GetXmlSchema(dt));
                    this._output.Append(',');
                }

                WriteDataRowData(row);

                // end datatable
                this._output.Append('}');
            }

            internal string WriteDataTableToJson(DataTable dt)
            {
                this._output.Append('{');
                WriteDataTableData(dt);
                this._output.Append('}');

                return _output.ToString();

            }


            private void WriteDataTableData(DataTable table)
            {
                _output.Append('\"');
                _output.Append(table.TableName);
                _output.Append("\":[");
                DataColumnCollection cols = table.Columns;
                bool rowseparator = false;
                foreach (DataRow row in table.Rows)
                {
                    if (rowseparator) _output.Append(',');
                    rowseparator = true;
                    _output.Append('{');

                    bool pendingSeperator = false;
                    foreach (DataColumn column in cols)
                    {
                        if (pendingSeperator) _output.Append(',');
                        WritePair(column.ColumnName, row[column], column.DataType);
                        pendingSeperator = true;
                    }
                    _output.Append('}');
                }

                _output.Append(']');
            }

            void WriteDataTable(DataTable dt)
            {
                this._output.Append('{');
                if (_Option.UseExtensions)
                {
                    this.WritePair("$schema", _Option.UseDatasetSchema ? (object)this.GetSchema(dt) : this.GetXmlSchema(dt));
                    this._output.Append(',');
                }
                WriteDataTableData(dt);

                // end datatable
                this._output.Append('}');
            }


            bool _TypesWritten = false;


            private void WriteObject(object obj, Type baseType)
            {
                int i = 0;
                if (_CircularItems.TryGetValue(obj, out i) == false)
                    _CircularItems.Add(obj, _CircularItems.Count + 1);
                else
                {
                    if (_current_depth > 0)
                    {
                        _isCircular = true;
                        _output.Append("{\"$i\":" + i + "}");
                        return;
                    }
                }
                if (_Option.UseTypesExtension == false)
                    _output.Append('{');
                else
                {
                    if (_TypesWritten == false)
                    {
                        _output.Append('{');
                        _before = _output;
                        _output = new StringBuilder();
                    }
                    else
                        _output.Append('{');
                }
                _TypesWritten = true;
                _current_depth++;
                if (_current_depth > _MAX_DEPTH)
                    throw new Exception("Serializer encountered maximum depth of " + _MAX_DEPTH);


                Dictionary<string, string> map = new Dictionary<string, string>();
                Type t = baseType == null ? obj.GetType() : baseType;
                bool append = false;
                if (_Option.UseExtensions)
                {
                    if (_Option.UseTypesExtension == false)
                        WritePairFast("$type", JsonActivator.Get.GetTypeAssemblyName(t));
                    else
                    {
                        int dt = 0;
                        string ct = JsonActivator.Get.GetTypeAssemblyName(t);
                        if (_GlobalTypes.TryGetValue(ct, out dt) == false)
                        {
                            dt = _GlobalTypes.Count + 1;
                            _GlobalTypes.Add(ct, dt);
                        }
                        WritePairFast("$type", dt.ToString());
                    }
                    append = true;
                }

                JsonFields[] g = JsonActivator.Get.GetFieldsGetter(t, _Option.ShowReadOnlyProperties, _Option.IgnoreAttributes);
                int c = g.Length;
                for (int ii = 0; ii < c; ii++)
                {
                    var p = g[ii];
                    object o = p.Field(obj);
                    if ((o == null || o is DBNull) && _Option.SerializeNullValues == false)
                    {
                        //append = false;
                    }
                    else
                    {
                        if (append)
                            _output.Append(',');

                        WritePair(p.Name, o);
                        if (o != null && _Option.UseExtensions)
                        {
                            Type tt = o.GetType();
                            if (tt == typeof(System.Object))
                                map.Add(p.Name, tt.ToString());
                        }
                        append = true;
                    }
                }
                if (map.Count > 0 && _Option.UseExtensions)
                {
                    _output.Append(",\"$map\":");
                    WriteStringDictionary(map);
                }
                //_current_depth--;
                _output.Append('}');
                _current_depth--;
            }

            private void WritePairFast(string name, string value)
            {
                if ((value == null) && _Option.SerializeNullValues == false)
                    return;
                WriteStringValue(name);

                _output.Append(':');

                WriteStringValue(value);
            }

            private void WritePair(string name, object value)
            {
                if ((value == null || value is DBNull) && _Option.SerializeNullValues == false)
                    return;
                WriteStringValue(name);

                _output.Append(':');

                WriteValue(value, null);
            }

            private void WritePair(string name, object value, Type baseType)
            {
                if ((value == null || value is DBNull) && _Option.SerializeNullValues == false)
                    return;
                WriteStringValue(name);

                _output.Append(':');

                WriteValue(value, baseType);
            }
            private void WriteArray(IEnumerable array)
            {
                _output.Append('[');

                bool pendingSeperator = false;

                foreach (object obj in array)
                {
                    if (pendingSeperator) _output.Append(',');

                    WriteValue(obj, null);

                    pendingSeperator = true;
                }
                _output.Append(']');
            }

            private void WriteArray<T>(IEnumerable<T> array)
            {
                _output.Append('[');

                bool pendingSeperator = false;

                foreach (var obj in array)
                {
                    if (pendingSeperator) _output.Append(',');

                    WriteValue(obj, typeof(T));

                    pendingSeperator = true;
                }
                _output.Append(']');
            }
            private void WriteStringDictionary(IDictionary dic)
            {
                _output.Append('{');

                bool pendingSeparator = false;

                foreach (DictionaryEntry entry in dic)
                {
                    if (pendingSeparator) _output.Append(',');

                    WritePair((string)entry.Key, entry.Value);

                    pendingSeparator = true;
                }
                _output.Append('}');
            }

            private void WriteDictionary(IDictionary dic)
            {
                _output.Append('{');

                bool pendingSeparator = false;

                foreach (DictionaryEntry entry in dic)
                {
                    if (pendingSeparator) _output.Append(',');
                    WritePair((string)entry.Key, entry.Value);
                    pendingSeparator = true;
                }
                _output.Append('}');
            }


            private void WriteDictionaryKeyValue(IDictionary dic)
            {
                _output.Append('[');

                bool pendingSeparator = false;

                foreach (DictionaryEntry entry in dic)
                {
                    if (pendingSeparator) _output.Append(',');
                    _output.Append('{');

                    WritePair("k", entry.Key);
                    _output.Append(",");
                    WritePair("v", entry.Value);

                    _output.Append('}');

                    pendingSeparator = true;
                }
                _output.Append(']');
            }

            private void WriteKeyValue(IKeyValue kv)
            {

                _output.Append('{');

                bool pendingSeparator = false;

                foreach (var entry in kv.ToList())
                {
                    if (pendingSeparator) _output.Append(',');
                    WritePair((string)entry.Key, entry.Value);
                    pendingSeparator = true;
                }
                _output.Append('}');

            }

            private void WriteStringValue(string s)
            {
                _output.Append('\"');
                _output.Append(s);
                _output.Append('\"');
            }

            private void WriteString(string s)
            {
                _output.Append('\"');

                int runIndex = -1;
                int l = s.Length;
                for (var index = 0; index < l; ++index)
                {
                    var c = s[index];

                    if (_useEscapedUnicode)
                    {
                        if (c >= ' ' && c < 128 && c != '\"' && c != '\\')
                        {
                            if (runIndex == -1)
                                runIndex = index;

                            continue;
                        }
                    }
                    else
                    {
                        if (c != '\t' && c != '\n' && c != '\r' && c != '\"' && c != '\\')// && c != ':' && c!=',')
                        {
                            if (runIndex == -1)
                                runIndex = index;

                            continue;
                        }
                    }

                    if (runIndex != -1)
                    {
                        _output.Append(s, runIndex, index - runIndex);
                        runIndex = -1;
                    }

                    switch (c)
                    {
                        case '\t': _output.Append("\\t"); break;
                        case '\r': _output.Append("\\r"); break;
                        case '\n': _output.Append("\\n"); break;
                        case '"':
                        case '\\': _output.Append('\\'); _output.Append(c); break;
                        default:
                            if (_useEscapedUnicode)
                            {
                                _output.Append("\\u");
                                _output.Append(((int)c).ToString("X4", NumberFormatInfo.InvariantInfo));
                            }
                            else
                                _output.Append(c);

                            break;
                    }
                }

                if (runIndex != -1)
                    _output.Append(s, runIndex, s.Length - runIndex);


                _output.Append('\"');
            }
        }

        #endregion

        #region Reader

        internal class JsonReader
        {
            public static JsonReader Get(JsonSettings settings)
            {
                return new JsonReader(settings);
            }

            public JsonReader(JsonSettings settings)
            {
                _Option = settings ?? JsonSerializer.DefaultOption;
            }

            private JsonSettings _Option;
            private bool _useGlobalTypes = false;
            private Dictionary<object, int> _CircularItems = new Dictionary<object, int>();
            private Dictionary<int, object> _CircularItemsRev = new Dictionary<int, object>();
            private bool _isCircular = true;

            public T ToObject<T>(string json)
            {
                Type t = typeof(T);
                var o = ToObject(json, t);

                if (t.IsArray)
                {
                    if ((o as ICollection).Count == 0) // edge case for "[]" -> T[]
                    {
                        Type tt = t.GetElementType();
                        object oo = Array.CreateInstance(tt, 0);
                        return (T)oo;
                    }
                    else
                        return (T)o;
                }
                else
                    return (T)o;
            }

            public object ToObject(string json)
            {
                return ToObject(json, null);
            }

            public object ToObject(string json, Type type)
            {
                _Option.EnsureValues();
                Type t = null;
                if (type != null && type.IsGenericType)
                    t = JsonActivator.Get.GetGenericTypeDefinition(type);
                if (t == typeof(Dictionary<,>) || t == typeof(List<>))
                    _Option.UseTypesExtension = false;

                _useGlobalTypes = _Option.UseTypesExtension;

                if (type != null && SerializeTools.IsISerialJson(type))
                {
                    object instance = ActivatorUtil.CreateInstance(type);
                    return ((ISerialJson)instance).EntityRead(json, null);
                }

                object o = JsonParser.Parse(json, _Option.IgnoreCaseOnDeserialize);
                if (o == null)
                    return null;

 
                if (type != null && type == typeof(DataSet))
                    return CreateDataset(o as Dictionary<string, object>, null);

                if (type != null && type == typeof(DataTable))
                    return CreateDataTable(o as Dictionary<string, object>, null);

                if (o is IDictionary)
                {
                    if (type != null && t == typeof(Dictionary<,>)) // deserialize a dictionary
                        return RootDictionary(o, type);
                    else // deserialize an object
                        return ParseDictionary(o as Dictionary<string, object>, null, type, null);
                }

                if (o is List<object>)
                {
                    if (type != null && t == typeof(Dictionary<,>)) // kv format
                        return RootDictionary(o, type);

                    if (type != null && t == typeof(List<>)) // deserialize to generic list
                        return RootList(o, type);

                    if (type == typeof(Hashtable))
                        return RootHashTable((List<object>)o);
                    else
                        return (o as List<object>).ToArray();
                }

                if (type != null && o.GetType() != type)
                    return ChangeType(o, type);

                return o;
            }

            #region   private methods
            private object RootHashTable(List<object> o)
            {
                Hashtable h = new Hashtable();

                foreach (Dictionary<string, object> values in o)
                {
                    object key = values["k"];
                    object val = values["v"];
                    if (key is Dictionary<string, object>)
                        key = ParseDictionary((Dictionary<string, object>)key, null, typeof(object), null);

                    if (val is Dictionary<string, object>)
                        val = ParseDictionary((Dictionary<string, object>)val, null, typeof(object), null);

                    h.Add(key, val);
                }

                return h;
            }
            
            private object ChangeType(object value, Type conversionType)
            {

                if (value == null)
                {
                    if (IsNullable(conversionType))
                        return value;
                    else
                        return UnderlyingTypeOf(conversionType);
                }


                if (conversionType == typeof(int))
                    return (int)((long)value);

                else if (conversionType == typeof(long))
                    return (long)value;

                else if (conversionType == typeof(string))
                    return (string)value;

                else if (conversionType == typeof(Guid))
                    return JsonConverter.ToGuid((string)value);

                else if (conversionType.IsEnum)
                    return JsonConverter.ToEnum(conversionType, value);

                else if (conversionType == typeof(DateTime))
                    return JsonConverter.ToDateTime((string)value, _Option.UseUTCDateTime);

                else if (JsonActivator.Get.IsTypeRegistered(conversionType))
                    return JsonActivator.Get.CreateCustom((string)value, conversionType);

                if (IsNullable(conversionType))
                {
                    if (value == null)
                    {
                        return value;
                    }
                    conversionType = UnderlyingTypeOf(conversionType);
                }

                return Convert.ChangeType(value, conversionType, CultureInfo.InvariantCulture);
            }

            private bool IsNullable(Type t)
            {
                if (!t.IsGenericType) return false;
                Type g = t.GetGenericTypeDefinition();
                return (g.Equals(typeof(Nullable<>)));
            }

            private Type UnderlyingTypeOf(Type t)
            {
                return t.GetGenericArguments()[0];
            }
            
            private object RootList(object parse, Type type)
            {
                Type[] gtypes = JsonActivator.Get.GetGenericArguments(type);
                IList o = (IList)ActivatorUtil.CreateInstance(type);

                foreach (var k in (IList)parse)
                {
                    _useGlobalTypes = false;
                    object v = k;
                    if (k is Dictionary<string, object>)
                        v = ParseDictionary(k as Dictionary<string, object>, null, gtypes[0], null);
                    else
                        v = ChangeType(k, gtypes[0]);

                    o.Add(v);
                }
                return o;
            }


            private object RootDictionary(object parse, Type type)
            {
                Type[] gtypes = JsonActivator.Get.GetGenericArguments(type);
                Type t1 = null;
                Type t2 = null;
                if (gtypes != null)
                {
                    t1 = gtypes[0];
                    t2 = gtypes[1];
                }
                if (parse is Dictionary<string, object>)
                {
                    IDictionary o = (IDictionary)ActivatorUtil.CreateInstance(type);

                    foreach (var kv in (Dictionary<string, object>)parse)
                    {
                        object v;
                        object k = ChangeType(kv.Key, t1);

                        if (kv.Value is Dictionary<string, object>)
                            v = ParseDictionary(kv.Value as Dictionary<string, object>, null, t2, null);

                        else if (t2.IsArray)
                            v = CreateArray((List<object>)kv.Value, t2, t2.GetElementType(), null);

                        else if (kv.Value is IList)
                            v = CreateGenericList((List<object>)kv.Value, t2, t1, null);

                        else
                            v = ChangeType(kv.Value, t2);

                        o.Add(k, v);
                    }

                    return o;
                }
                if (parse is List<object>)
                    return CreateDictionary(parse as List<object>, type, gtypes, null);

                return null;
            }

            internal object ParseDictionary(Dictionary<string, object> d, Dictionary<string, object> globaltypes, Type type, object input)
            {
                object tn = "";
                if (type == typeof(NameValueCollection))
                    return CreateNameValueCollection(d);
                if (type == typeof(StringDictionary))
                    return CreateStringDictionary(d);
                if (_isCircular == false)
                    _isCircular = d.TryGetValue("$circular", out tn);

                if (d.TryGetValue("$i", out tn))
                {
                    object v = null;
                    _CircularItemsRev.TryGetValue((int)(long)tn, out v);
                    return v;
                }

                if (d.TryGetValue("$types", out tn))
                {
                    _useGlobalTypes = true;
                    globaltypes = new Dictionary<string, object>();
                    foreach (var kv in (Dictionary<string, object>)tn)
                    {
                        globaltypes.Add((string)kv.Value, kv.Key);
                    }
                }

                bool found = d.TryGetValue("$type", out tn);

                if (found == false && type == typeof(System.Object))
                {
                    return d;   // CreateDataset(d, globaltypes);
                }

                if (found)
                {
                    if (_useGlobalTypes)
                    {
                        object tname = "";
                        if (globaltypes != null && globaltypes.TryGetValue((string)tn, out tname))
                            tn = tname;
                    }
                    //--type = JsonActivator.Instance.GetTypeFromCache((string)tn);
                    type = JsonActivator.Get.GetTypeFromName((string)tn);
                }

                if (type == null)
                    throw new Exception("Cannot determine type");

                string typename = type.FullName;
                object o = input;
                if (o == null)
                {
                    if (_Option.UseUninitializedObject)
                        o = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
                    else
                        o = ActivatorUtil.CreateInstance(type);
                }
                if (_isCircular)
                {
                    int i = 0;
                    if (_CircularItems.TryGetValue(o, out i) == false)
                    {
                        i = _CircularItems.Count + 1;
                        _CircularItems.Add(o, i);
                        _CircularItemsRev.Add(i, o);
                    }
                }

                Dictionary<string, TypeInfo> props = JsonActivator.Get.GetTypesInfo(type, typename, _Option.IgnoreCaseOnDeserialize, JsonActivator.Get.IsTypeRegistered(type));
                foreach (string n in d.Keys)
                {
                    string name = n;
                    if (_Option.IgnoreCaseOnDeserialize) name = name.ToLower();
                    if (name == "$map")
                    {
                        ProcessMap(o, props, (Dictionary<string, object>)d[name]);
                        continue;
                    }
                    TypeInfo typeInfo;
                    if (props.TryGetValue(name, out typeInfo) == false)
                        continue;
                    if (typeInfo.CanWrite)
                    {
                        object v = d[name];

                        if (v != null)
                        {
                            object oset = null;

                            switch (typeInfo.serialType)
                            {
                                case SerialJsonType.Int: oset = (int)((long)v); break;
                                case SerialJsonType.Long: oset = (long)v; break;
                                case SerialJsonType.String: oset = (string)v; break;
                                case SerialJsonType.Bool: oset = (bool)v; break;
                                case SerialJsonType.DateTime: oset = JsonConverter.ToDateTime((string)v,true); break;
                                case SerialJsonType.Enum: oset = JsonConverter.ToEnum(typeInfo.propertyType, v); break;
                                case SerialJsonType.Guid: oset = JsonConverter.ToGuid((string)v); break;

                                case SerialJsonType.Array:
                                    if (!typeInfo.IsValueType)
                                        oset = CreateArray((List<object>)v, typeInfo.propertyType, typeInfo.elementType, globaltypes);
                                    // what about 'else'?
                                    break;
                                case SerialJsonType.ByteArray: oset = Convert.FromBase64String((string)v); break;

                                case SerialJsonType.DataSet: oset = CreateDataset((Dictionary<string, object>)v, globaltypes); break;
                                case SerialJsonType.DataTable: oset = CreateDataTable((Dictionary<string, object>)v, globaltypes); break;
                                case SerialJsonType.Hashtable: // same case as Dictionary

                                case SerialJsonType.Dictionary: oset = CreateDictionary((List<object>)v, typeInfo.propertyType, typeInfo.GenericTypes, globaltypes); break;
                                case SerialJsonType.StringKeyDictionary: oset = CreateStringKeyDictionary((Dictionary<string, object>)v, typeInfo.propertyType, typeInfo.GenericTypes, globaltypes); break;
                                case SerialJsonType.NameValue: oset = CreateNameValueCollection((Dictionary<string, object>)v); break;
                                case SerialJsonType.StringDictionary: oset = CreateStringDictionary((Dictionary<string, object>)v); break;
                                case SerialJsonType.Custom: oset = JsonActivator.Get.CreateCustom((string)v, typeInfo.propertyType); break;
                                default:
                                    {
                                        if (typeInfo.IsGenericType && typeInfo.IsValueType == false && v is List<object>)
                                            oset = CreateGenericList((List<object>)v, typeInfo.propertyType, typeInfo.elementType, globaltypes);

                                        else if ((typeInfo.IsClass || typeInfo.IsStruct) && v is Dictionary<string, object>)
                                            oset = ParseDictionary((Dictionary<string, object>)v, globaltypes, typeInfo.propertyType, typeInfo.getterField(o));

                                        else if (v is List<object>)
                                            oset = CreateArray((List<object>)v, typeInfo.propertyType, typeof(object), globaltypes);

                                        else if (typeInfo.IsValueType)
                                            oset = ChangeType(v, typeInfo.changeType);

                                        else
                                            oset = v;
                                    }
                                    break;
                            }

                            o = typeInfo.setterField(o, oset);
                        }
                    }
                }
                return o;
            }

            private StringDictionary CreateStringDictionary(Dictionary<string, object> d)
            {
                StringDictionary nv = new StringDictionary();

                foreach (var o in d)
                    nv.Add(o.Key, (string)o.Value);

                return nv;
            }

            private NameValueCollection CreateNameValueCollection(Dictionary<string, object> d)
            {
                NameValueCollection nv = new NameValueCollection();

                foreach (var o in d)
                    nv.Add(o.Key, (string)o.Value);

                return nv;
            }

            private void ProcessMap(object obj, Dictionary<string, TypeInfo> props, Dictionary<string, object> dic)
            {
                foreach (KeyValuePair<string, object> kv in dic)
                {
                    TypeInfo p = props[kv.Key];
                    object o = p.getterField(obj);
                    Type t = Type.GetType((string)kv.Value);
                    if (t == typeof(Guid))
                        p.setterField(obj, JsonConverter.ToGuid((string)o));
                }
            }
            /*
            private int CreateInteger(string s, int index, int count)
            {
                int num = 0;
                bool neg = false;
                for (int x = 0; x < count; x++, index++)
                {
                    char cc = s[index];

                    if (cc == '-')
                        neg = true;
                    else if (cc == '+')
                        neg = false;
                    else
                    {
                        num *= 10;
                        num += (int)(cc - '0');
                    }
                }
                if (neg) num = -num;

                return num;
            }

            private object CreateEnum(Type type, object value)
            {
                if (value == null)
                    return value;
                return EnumExtension.Parse(type, value.ToString(), null);
            }

            private Guid CreateGuid(string s)
            {
                if (s == null)
                    return Guid.Empty;

                if (s.Length > 30)
                    return new Guid(s);
                else
                    return new Guid(Convert.FromBase64String(s));
            }

            private DateTime CreateDateTime(string value)
            {
                bool utc = false;
                //                   0123456789012345678 9012 9/3
                // datetime format = yyyy-MM-dd HH:mm:ss .nnn  Z
                int year;
                int month;
                int day;
                int hour;
                int min;
                int sec;
                int ms = 0;

                year = CreateInteger(value, 0, 4);
                month = CreateInteger( value, 5, 2);
                day = CreateInteger(value, 8, 2);
                hour = CreateInteger( value, 11, 2);
                min = CreateInteger( value, 14, 2);
                sec = CreateInteger( value, 17, 2);
                if (value.Length > 21 && value[19] == '.')
                    ms = CreateInteger( value, 20, 3);

                //if (value.EndsWith("Z"))
                if (value[value.Length - 1] == 'Z')
                    utc = true;

                if (_Option.UseUTCDateTime == false && utc == false)
                    return new DateTime(year, month, day, hour, min, sec, ms);
                else
                    return new DateTime(year, month, day, hour, min, sec, ms, DateTimeKind.Utc).ToLocalTime();
            }
            */
            private object CreateArray(List<object> data, Type pt, Type elementType, Dictionary<string, object> globalTypes)
            {
                Array col = Array.CreateInstance(elementType, data.Count);
                // create an array of objects
                for (int i = 0; i < data.Count; i++)
                {
                    object ob = data[i];
                    if (ob is IDictionary)
                        col.SetValue(ParseDictionary((Dictionary<string, object>)ob, globalTypes, elementType, null), i);
                    else
                        col.SetValue(ChangeType(ob, elementType), i);
                }

                return col;
            }

            private object CreateArray<T>(List<T> data)
            {
                Type elementType=typeof(T);

                Array col = Array.CreateInstance(elementType, data.Count);
                // create an array of T
                for (int i = 0; i < data.Count; i++)
                {
                    col.SetValue(data[i], i);
                }

                return col;
            }

            private object CreateGenericList(List<object> data, Type propertyType, Type elementType, Dictionary<string, object> globalTypes)
            {
                IList col = (IList)ActivatorUtil.CreateInstance(propertyType);
                // create an array of objects
                foreach (object ob in data)
                {
                    if (ob is IDictionary)
                        col.Add(ParseDictionary((Dictionary<string, object>)ob, globalTypes, elementType, null));

                    else if (ob is List<object>)
                    {
                        if (elementType.IsGenericType)
                            col.Add((List<object>)ob);//).ToArray());
                        else
                            col.Add(((List<object>)ob).ToArray());
                    }
                    else
                        col.Add(ChangeType(ob, elementType));
                }
                return col;
            }

            private object CreateStringKeyDictionary(Dictionary<string, object> reader, Type propertyType, Type[] types, Dictionary<string, object> globalTypes)
            {
                var col = (IDictionary)ActivatorUtil.CreateInstance(propertyType);
                Type t1 = null;
                Type t2 = null;
                if (types != null)
                {
                    t1 = types[0];
                    t2 = types[1];
                }

                foreach (KeyValuePair<string, object> values in reader)
                {
                    var key = values.Key;
                    object val = null;

                    if (values.Value is Dictionary<string, object>)
                        val = ParseDictionary((Dictionary<string, object>)values.Value, globalTypes, t2, null);

                    else if (types != null && t2.IsArray)
                        val = CreateArray((List<object>)values.Value, t2, t2.GetElementType(), globalTypes);

                    else if (values.Value is IList)
                        val = CreateGenericList((List<object>)values.Value, t2, t1, globalTypes);

                    else
                        val = ChangeType(values.Value, t2);

                    col.Add(key, val);
                }

                return col;
            }

            private object CreateDictionary(List<object> reader, Type propertyType, Type[] types, Dictionary<string, object> globalTypes)
            {
                IDictionary col = (IDictionary)ActivatorUtil.CreateInstance(propertyType);
                Type t1 = null;
                Type t2 = null;
                if (types != null)
                {
                    t1 = types[0];
                    t2 = types[1];
                }

                foreach (Dictionary<string, object> values in reader)
                {
                    object key = values["k"];
                    object val = values["v"];

                    if (key is Dictionary<string, object>)
                        key = ParseDictionary((Dictionary<string, object>)key, globalTypes, t1, null);
                    else
                        key = ChangeType(key, t1);

                    if (val is Dictionary<string, object>)
                        val = ParseDictionary((Dictionary<string, object>)val, globalTypes, t2, null);
                    else
                        val = ChangeType(val, t2);

                    col.Add(key, val);
                }

                return col;
            }


            private DataSet CreateDataset(Dictionary<string, object> reader, Dictionary<string, object> globalTypes)
            {
                DataSet ds = new DataSet();
                ds.EnforceConstraints = false;
                ds.BeginInit();

                // read dataset schema here
                var schema = reader["$schema"];

                if (schema is string)
                {
                    TextReader tr = new StringReader((string)schema);
                    ds.ReadXmlSchema(tr);
                }
                else
                {
                    JsonSchema ms = (JsonSchema)ParseDictionary((Dictionary<string, object>)schema, globalTypes, typeof(JsonSchema), null);
                    ds.DataSetName = ms.Name;
                    for (int i = 0; i < ms.Info.Count; i += 3)
                    {
                        if (ds.Tables.Contains(ms.Info[i]) == false)
                            ds.Tables.Add(ms.Info[i]);
                        ds.Tables[ms.Info[i]].Columns.Add(ms.Info[i + 1], Type.GetType(ms.Info[i + 2]));
                    }
                }

                foreach (KeyValuePair<string, object> pair in reader)
                {
                    if (pair.Key == "$type" || pair.Key == "$schema") continue;

                    List<object> rows = (List<object>)pair.Value;
                    if (rows == null) continue;

                    DataTable dt = ds.Tables[pair.Key];
                    ReadDataTable(rows, dt);
                }

                ds.EndInit();

                return ds;
            }

            private void ReadDataTable(List<object> rows, DataTable dt)
            {
                dt.BeginInit();
                dt.BeginLoadData();
                List<int> guidcols = new List<int>();
                List<int> datecol = new List<int>();

                foreach (DataColumn c in dt.Columns)
                {
                    if (c.DataType == typeof(Guid) || c.DataType == typeof(Guid?))
                        guidcols.Add(c.Ordinal);
                    if (_Option.UseUTCDateTime && (c.DataType == typeof(DateTime) || c.DataType == typeof(DateTime?)))
                        datecol.Add(c.Ordinal);
                }

                foreach (List<object> row in rows)
                {
                    object[] v = new object[row.Count];
                    row.CopyTo(v, 0);
                    foreach (int i in guidcols)
                    {
                        string s = (string)v[i];
                        if (s != null && s.Length < 36)
                            v[i] = new Guid(Convert.FromBase64String(s));
                    }
                    if (_Option.UseUTCDateTime)
                    {
                        foreach (int i in datecol)
                        {
                            string s = (string)v[i];
                            if (s != null)
                                v[i] = JsonConverter.ToDateTime(s,_Option.UseUTCDateTime);
                        }
                    }
                    dt.Rows.Add(v);
                }

                dt.EndLoadData();
                dt.EndInit();
            }

            DataTable CreateDataTable(Dictionary<string, object> reader, Dictionary<string, object> globalTypes)
            {
                var dt = new DataTable();


                // read dataset schema here
                var schema = reader["$schema"];

                if (schema != null && schema is string)
                {
                    TextReader tr = new StringReader((string)schema);
                    dt.ReadXmlSchema(tr);
                }
                else
                {
                    var ms = (JsonSchema)this.ParseDictionary((Dictionary<string, object>)schema, globalTypes, typeof(JsonSchema), null);
                    dt.TableName = ms.Info[0];
                    for (int i = 0; i < ms.Info.Count; i += 3)
                    {
                        dt.Columns.Add(ms.Info[i + 1], Type.GetType(ms.Info[i + 2]));
                    }
                }


                foreach (var pair in reader)
                {
                    if (pair.Key == "$type" || pair.Key == "$schema")
                        continue;

                    var rows = (List<object>)pair.Value;
                    if (rows == null)
                        continue;

                    if (!dt.TableName.Equals(pair.Key, StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    ReadDataTable(rows, dt);
                }

                return dt;
            }

            #endregion
        }


        #endregion
#endif
    }
}