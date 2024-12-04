using Nistec.Data;
using Nistec.IO;
using Nistec.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Nistec.Generic
{

    //public class QueryString
    //{

    //    public static Dictionary<string, string> ParseQueryString(string queryString)
    //    {
    //        //string s1 = "(colorIndex=3)(font.family=Helvicta)(font.bold=1)";

    //        string[] t = queryString.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

    //        if (t.Length % 2 != 0)
    //        {
    //            throw new ArgumentException("queryString is incorrect, Not match key value arguments");
    //        }

    //        Dictionary<string, string> dictionary =
    //           t.Select(item => item.Split('=')).ToDictionary(s => s[0], s => s[1]);
    //        return dictionary;
    //    }

    //    public static NameValueArgs Parse(System.Web.HttpRequest request)
    //    {

    //        if (request == null)
    //        {
    //            throw new ArgumentException("invalid request");
    //        }
    //        return ParseRawUrl(request.RawUrl);
    //    }

    //    public static NameValueArgs ParseRawUrl(string url)
    //    {

    //        if (url == null)
    //            url = string.Empty;

    //        string qs = string.Empty;

    //        if (url.Contains("?"))
    //        {
    //            qs = url.Substring(url.IndexOf("?") + 1);
    //            url = url.Substring(0, url.IndexOf("?"));
    //        }

    //        return ParseQueryString(qs);
    //    }

    //    private static string CLeanQueryString(string qs)
    //    {
    //        return qs.Replace("&amp;", "&");
    //    }

    //}
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class NameValueArgs : Dictionary<string, string>, ISerialEntity, IDataRowAdaptor, ISerialJson, INameValue, IKeyValue<string>
    {
        #region static
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public static NameValueArgs Create(params string[] keyValue)
        {
            if (keyValue == null)
                return null;
            return new NameValueArgs(keyValue);
        }
               
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static NameValueArgs Convert(IDictionary<string, object> dic)
        {
            if (dic == null)
                return null;
            var nva = new NameValueArgs();

            foreach (var entry in dic.ToArray())
            {
                nva[entry.Key] = entry.Value==null? null: entry.Value.ToString();
            }
            return nva;
        }

        #endregion

        #region ctor
        /// <summary>
        /// 
        /// </summary>
        public NameValueArgs()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValueList"></param>
        public NameValueArgs(IEnumerable<KeyValuePair<string, string>> keyValueList)
        {
            Load(keyValueList);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        public NameValueArgs(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                EntityRead(ms, null);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        public NameValueArgs(NetStream stream)
        {
            EntityRead(stream, null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValue"></param>
        public NameValueArgs(string[] keyValue)
        {
            Parse(keyValue);
            //Load(ParseQuery(keyValue));
        }

        //public static NameValueArgs Create(params string[] keyValue)
        //{
        //    var pair = ParseQuery(keyValue);
        //    NameValueArgs query = new NameValueArgs();
        //    query.Load(pair);
        //    return query;
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public NameValueArgs Merge(NameValueArgs keyValue)
        {
            if (keyValue == null)
                return this;
            foreach (var entry in keyValue.ToArray())
            {
                this[entry.Key] = entry.Value == null ? null : entry.Value.ToString();
            }
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public NameValueArgs Merge(params string[] keyValue)
        {
            if (keyValue == null)
                return this;
            Parse(keyValue);
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValueParameters"></param>
        void Parse(string[] keyValueParameters)
        {
            if (keyValueParameters == null)
            {
                throw new ArgumentNullException("keyValueParameters");
            }

            int count = keyValueParameters.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            for (int i = 0; i < count; i++)
            {
                this[keyValueParameters[i]] = keyValueParameters[++i];
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dr"></param>
        public virtual void Prepare(DataRow dr)
        {
            this.ToNameValue(dr);
        }

        //public static NameValueArgs Create(params string[] keyValue)
        //{
        //    NameValueArgs pair = new NameValueArgs();
        //    if (keyValue == null)
        //        return pair;
        //    string[] array = null;
        //    if(keyValue.Length==1)
        //    {
        //        if (string.IsNullOrEmpty(keyValue[0]))
        //            return pair;
        //        array = keyValue[0].Split('|');
        //    }
        //    else
        //    {
        //        array = keyValue;
        //    }
        //    int count = array.Length;
        //    if (count % 2 != 0)
        //    {
        //        throw new ArgumentException("keyValues parameter is not correct, Not match key value arguments");
        //    }
        //    for (int i = 0; i < count; i++)
        //    {
        //        string o = array[i];
        //        if (o != null)
        //        {
        //            pair.Add(new KeyValuePair<string, string>(array[i].ToString(), array[++i]));
        //        }
        //    }
        //    return pair;
        //}
        //internal static List<KeyValuePair<string, string>> ParseQuery(params string[] keyValue)
        //{
        //    List<KeyValuePair<string, string>> pair = new List<KeyValuePair<string, string>>();
        //    if (keyValue == null)
        //        return pair;
        //    int count = keyValue.Length;
        //    if (count % 2 != 0)
        //    {
        //        throw new ArgumentException("keyValues parameter is not correct, Not match key value arguments");
        //    }
        //    for (int i = 0; i < count; i++)
        //    {
        //        string o = keyValue[i];
        //        if (o != null)
        //        {
        //            pair.Add(new KeyValuePair<string, string>(keyValue[i].ToString(), keyValue[++i]));
        //        }
        //    }
        //    return pair;
        //}

        #endregion

        #region properties
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>        
        public string Get(string key)
        {
            string value;
            TryGetValue(key, out value);
            return value;
            //return this[key];
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valueIfNullOrEmpty"></param>
        /// <returns></returns>
        public string GetVal(string key, string valueIfNullOrEmpty)
        {
            string value;
            TryGetValue(key, out value);
            return string.IsNullOrEmpty(value) ? valueIfNullOrEmpty: value;
            //return this[key];
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TV"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public TV Get<TV>(string key)
        {
            return GenericTypes.Convert<TV>(this[key]);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TV"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public TV Get<TV>(string key, TV defaultValue)
        {
            return GenericTypes.Convert<TV>(this[key], defaultValue);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TV"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public TV GetEnum<TV>(string key, TV defaultValue)
        {
            return GenericTypes.ConvertEnum<TV>(this[key], defaultValue);
        }


        #endregion

        #region collection methods

        /// <summary>
        /// Get this as sorted <![CDATA[ <see cref="IOrderedEnumerable<KeyValuePair<string, object>>"/>]]>
        /// </summary>
        /// <returns></returns>
        public IOrderedEnumerable<KeyValuePair<string, string>> Sorted()
        {
            var sortedDict = from entry in this orderby entry.Key ascending select entry;
            return sortedDict;
        }
        #endregion

        #region Loaders
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValues"></param>
        public void AddArgs(params string[] keyValues)
        {
            if (keyValues == null)
                return;// null;
            int count = keyValues.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }

            for (int i = 0; i < count; i++)
            {
                string key = keyValues[i].ToString();
                string value = keyValues[++i];

                if (this.ContainsKey(key))
                    this[key] = value;
                else
                    this.Add(key, value);
            }
            //return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValues"></param>
        public void Add(params string[] keyValues)
        {
            Parse(keyValues);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public new virtual void Add(string key, string value)
        {
            base.Add(key, value == null ? null : value.ToString());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public virtual void Add(string key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("NameValueArgs.Add key");
            }

            base.Add(key, value == null ? null : value.ToString());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public virtual void Set(string key, string value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("NameValueArgs.key");
            }
            this[key] = value;
        }
        void Load(IEnumerable<KeyValuePair<string, string>> keyValueList)
        {
            if (keyValueList == null)
            {
                throw new ArgumentNullException("NameValueArgs.keyValueList");
            }
            //this.Clear();
            //this.AddRange(keyValueList.ToArray());
            foreach (var entry in keyValueList.ToArray())
            {
                this[entry.Key] = entry.Value;
            }
        }

        void Load(NameValueCollection qs)
        {
            if (qs == null)
            {
                throw new ArgumentNullException("NameValueArgs.qs");
            }

            for (int i = 0; i < qs.Count; i++)
            {
                this[qs.Keys[i]] = qs[i];
            }
        }

        void Copy(IDictionary<string, string> dic)
        {
            foreach (var entry in dic.ToArray())
            {
                this[entry.Key] = entry.Value;
            }
        }

        #endregion

        #region converter
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(string key, string value)
        {
            //return this.Exists(p => p.Key == key && p.Value == value);
            return this.Where(p => p.Key == key && p.Value == value).Count() > 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(KeyValuePair<string, string> item)
        {
            return this.Where(p => p.Key == item.Key && p.Value == item.Value).Count() > 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual KeyValuePair<string, string> GetItem(string key, string value)
        {
            return this.Where(p => p.Key == key && p.Value == value).FirstOrDefault();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="splitter"></param>
        /// <returns></returns>
        public string[] SplitTrim(string name, params char[] splitter)
        {
            var val = this[name];
            return val == null ? null : val.SplitTrim(splitter);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string JoinArg(string[] str)
        {
            return string.Join("|", str);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string[] ToKeyValueArray()
        {
            var list = new List<string>();

            foreach (var entry in this)
            {
                list.Add(entry.Key);
                list.Add(entry.Value);
            }
            return list.ToArray();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ToKeyValuePipe()
        {
            string[] val = ToKeyValueArray();
            return JoinArg(val);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> ToDictionary()
        {
            //var dict = this
            //   .Select(item => new { Key = item.Key, Value = item.Value })
            //   .Distinct()
            //   .ToDictionary(item => item.Key, item => item.Value, StringComparer.OrdinalIgnoreCase);
            //return dict;

            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ToQueryString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var entry in this)
            {
                sb.Append(entry.Key + "=" + entry.Value + "&");
            }
            return (sb.Length == 0) ? "" : sb.ToString().TrimEnd('&');
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="qs"></param>
        /// <param name="cleanAmp"></param>
        public void LoadQueryString(string qs, bool cleanAmp = true)
        {
            if (qs == null)
            {
                throw new ArgumentNullException("ParseQueryString.qs");
            }

            string str = cleanAmp ? CLeanQueryString(qs) : qs;

            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException("ParseQueryString.qs");
            }
            if (!str.Contains('='))
            {
                throw new ArgumentException("QueryString is incorrect");
            }
            this.Clear();
            foreach (string arg in str.Split(new char[] { '&' }))
            {
                if (!string.IsNullOrEmpty(arg))
                {
                    string[] strArray = arg.Split(new char[] { '=' });
                    if (strArray.Length == 2)
                    {
                        string key = cleanAmp ? strArray[0] : Regx.RegexReplace("amp;", strArray[0], "");
                        this[key] = strArray[1];
                    }
                    else
                    {
                        this[arg] = null;
                    }
                }
            }

        }

        #endregion

        #region ParseQueryString
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string ToQueryString(NameValueArgs args)
        {
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<string, string> entry in args)
            {
                sb.AppendFormat("{0}={1}&", entry.Key, entry.Value);
            }
            return sb.ToString().TrimEnd('&');
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static NameValueArgs Parse(System.Web.HttpRequest request)
        {

            if (request == null)
            {
                throw new ArgumentException("invalid request");
            }
            return ParseRawUrl(request.RawUrl);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static NameValueArgs ParseRawUrl(string url)
        {

            if (url == null)
                url = string.Empty;

            string qs = string.Empty;

            if (url.Contains("?"))
            {
                qs = url.Substring(url.IndexOf("?") + 1);
                url = url.Substring(0, url.IndexOf("?"));
            }

            return ParseQueryString(qs);
        }

        private static string CLeanQueryString(string qs)
        {
            return qs.Replace("&amp;", "&");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="qs"></param>
        /// <param name="cleanAmp"></param>
        /// <returns></returns>
        public static NameValueArgs ParseQueryString(string qs, bool cleanAmp = true)
        {
            NameValueArgs dictionary = new NameValueArgs();

            if (qs == null)
                qs = string.Empty;

            string str = cleanAmp ? CLeanQueryString(qs) : qs;

            if (string.IsNullOrEmpty(str))
            {
                return dictionary;
            }
            if (!str.Contains('='))
            {
                return dictionary;
            }


            //string[] t = qs.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

            //if (t.Length % 2 != 0)
            //{
            //    throw new ArgumentException("queryString is incorrect, Not match key value arguments");
            //}

            //dictionary =(NameValueArgs)
            //   t.Select(item => item.Split('=')).ToDictionary(s => s[0], s => s[1]);
            //return dictionary;


            foreach (string arg in str.Split(new char[] { '&' }))
            {
                if (!string.IsNullOrEmpty(arg))
                {
                    string[] strArray = arg.Split(new char[] { '=' });
                    if (strArray.Length == 2)
                    {
                        string key = cleanAmp ? strArray[0] : Regx.RegexReplace("amp;", strArray[0], "");
                        dictionary[key] = strArray[1];
                    }
                    else
                    {
                        dictionary[arg] = null;
                    }
                }
            }

            return dictionary;
        }

        #endregion

        #region  ISerialEntity
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public void EntityWrite(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            ((BinaryStreamer)streamer).WriteDirectDictionary<string,string>(this);
            streamer.Flush();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public void EntityRead(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);
            this.Clear();
            ((BinaryStreamer)streamer).TryReadDirectToDictionary<string, string>(this,false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public NetStream ToStream()
        {
            NetStream stream = new NetStream();
            EntityWrite(stream, null);
            return stream;
        }

        #endregion

        #region ISerialJson
        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static NameValueArgs ParseJson(string json)
        {
            if (json == null)
            {
                return null;
            }
            var nv = new NameValueArgs();
            nv.EntityRead(json, null);
            return nv;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pretty"></param>
        /// <returns></returns>
        public string ToJson(bool pretty = false)
        {
            return EntityWrite(new JsonSerializer(JsonSerializerMode.Write, null), pretty);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="pretty"></param>
        /// <returns></returns>
        public string EntityWrite(IJsonSerializer serializer, bool pretty = false)
        {
            if (serializer == null)
                serializer = new JsonSerializer(JsonSerializerMode.Write, null);

            foreach (var entry in this)
            {
                serializer.WriteToken(entry.Key, entry.Value);
            }
            return serializer.WriteOutput(pretty);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public object EntityRead(string json, IJsonSerializer serializer)
        {
            if (serializer == null)
                serializer = new JsonSerializer(JsonSerializerMode.Read, null);

            serializer.ParseTo(this, json);

            //var dic = serializer.ParseToDictionaryString(json);

            //AddRange(dic.ToArray());

            return this;
        }


        #endregion

    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>

    [Serializable]
    public class NameValueArgs<T> : Dictionary<string, T>, ISerialEntity, IDataRowAdaptor, ISerialJson, IKeyValue<T>
    {
        #region static
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static NameValueArgs<T> Create(string key, T value)
        {
            if (key == null)
                return null;
            var nv = new NameValueArgs<T>();
            nv.Add(key, value);
            return nv;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public static NameValueArgs<T> Create(params object[] keyValue)
        {
            if (keyValue == null)
                return null;
            return new NameValueArgs<T>(keyValue);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static NameValueArgs<T> Convert(IDictionary<string, T> dic)
        {
            if (dic == null)
                return null;
            var nva = new NameValueArgs<T>();

            foreach (var entry in dic.ToArray())
            {
                nva[entry.Key] = entry.Value;// == null ? null : entry.Value;
            }
            return nva;
        }

        #endregion

        #region ctor
        /// <summary>
        /// 
        /// </summary>
        public NameValueArgs()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValueList"></param>
        public NameValueArgs(IEnumerable<KeyValuePair<string, T>> keyValueList)
        {
            Load(keyValueList);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        public NameValueArgs(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                EntityRead(ms, null);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        public NameValueArgs(NetStream stream)
        {
            EntityRead(stream, null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValue"></param>
        public NameValueArgs(object[] keyValue)
        {
            Parse(keyValue);
            //Load(ParseQuery(keyValue));
        }

        //public static NameValueArgs Create(params string[] keyValue)
        //{
        //    var pair = ParseQuery(keyValue);
        //    NameValueArgs query = new NameValueArgs();
        //    query.Load(pair);
        //    return query;
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public NameValueArgs<T> Merge(NameValueArgs<T> keyValue)
        {
            if (keyValue == null)
                return this;
            foreach (var entry in keyValue.ToArray())
            {
                this[entry.Key] = entry.Value;
            }
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public NameValueArgs<T> Merge(params object[] keyValue)
        {
            if (keyValue == null)
                return this;
            Parse(keyValue);
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public NameValueArgs<T> Merge(string key, T value)
        {
            if (key == null)
                return this;
            this[key] = value;
            return this;
        }
        void Parse(object[] keyValueParameters)
        {
            if (keyValueParameters == null)
            {
                throw new ArgumentNullException("keyValueParameters");
            }

            int count = keyValueParameters.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            for (int i = 0; i < count; i++)
            {
                this[keyValueParameters[i].ToString()] = GenericTypes.Convert<T>(keyValueParameters[++i]);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dr"></param>
        public virtual void Prepare(DataRow dr)
        {
            this.ToNameValue(dr);
        }

        //public static NameValueArgs Create(params string[] keyValue)
        //{
        //    NameValueArgs pair = new NameValueArgs();
        //    if (keyValue == null)
        //        return pair;
        //    string[] array = null;
        //    if(keyValue.Length==1)
        //    {
        //        if (string.IsNullOrEmpty(keyValue[0]))
        //            return pair;
        //        array = keyValue[0].Split('|');
        //    }
        //    else
        //    {
        //        array = keyValue;
        //    }
        //    int count = array.Length;
        //    if (count % 2 != 0)
        //    {
        //        throw new ArgumentException("keyValues parameter is not correct, Not match key value arguments");
        //    }
        //    for (int i = 0; i < count; i++)
        //    {
        //        string o = array[i];
        //        if (o != null)
        //        {
        //            pair.Add(new KeyValuePair<string, string>(array[i].ToString(), array[++i]));
        //        }
        //    }
        //    return pair;
        //}
        //internal static List<KeyValuePair<string, string>> ParseQuery(params string[] keyValue)
        //{
        //    List<KeyValuePair<string, string>> pair = new List<KeyValuePair<string, string>>();
        //    if (keyValue == null)
        //        return pair;
        //    int count = keyValue.Length;
        //    if (count % 2 != 0)
        //    {
        //        throw new ArgumentException("keyValues parameter is not correct, Not match key value arguments");
        //    }
        //    for (int i = 0; i < count; i++)
        //    {
        //        string o = keyValue[i];
        //        if (o != null)
        //        {
        //            pair.Add(new KeyValuePair<string, string>(keyValue[i].ToString(), keyValue[++i]));
        //        }
        //    }
        //    return pair;
        //}

        #endregion

        #region properties
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get(string key)
        {
            T value;
            TryGetValue(key, out value);
            return value;
            //return this[key];
        }

        //public T GetVal(string key, string valueIfNullOrEmpty)
        //{
        //    T value;
        //    TryGetValue(key, out value);
        //    return default(T).Equals(value) ? valueIfNullOrEmpty : value;
        //    //return this[key];
        //}

        //public T Get(string key)
        //{
        //    return GenericTypes.Convert<T>(this[key]);
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T Get(string key, T defaultValue)
        {
            return GenericTypes.Convert<T>(this[key], defaultValue);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetEnum(string key, T defaultValue)
        {
            return GenericTypes.ConvertEnum<T>(this[key].ToString(), defaultValue);
        }


        #endregion

        #region collection methods

        /// <summary>
        /// Get this as sorted IOrderedEnumerable !KeyValuePair !string, object
        /// </summary>
        /// <returns></returns>
        public IOrderedEnumerable<KeyValuePair<string, T>> Sorted()
        {
            var sortedDict = from entry in this orderby entry.Key ascending select entry;
            return sortedDict;
        }
        #endregion

        #region Loaders
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public new virtual void Add(string key, T value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("NameValueArgs.Add key");
            }

            base.Add(key, value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public virtual void Set(string key, T value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("NameValueArgs.Add key");
            }
            this[key] = value;
        }

        void Load(IEnumerable<KeyValuePair<string, T>> keyValueList)
        {
            if (keyValueList == null)
            {
                throw new ArgumentNullException("NameValueArgs.keyValueList");
            }
            //this.Clear();
            //this.AddRange(keyValueList.ToArray());
            foreach (var entry in keyValueList.ToArray())
            {
                this[entry.Key] = entry.Value;
            }
        }

        void Load(NameValueCollection qs)
        {
            if (qs == null)
            {
                throw new ArgumentNullException("NameValueArgs.qs");
            }

            for (int i = 0; i < qs.Count; i++)
            {
                this[qs.Keys[i]] = GenericTypes.Convert<T>(qs[i]);
            }
        }

        void Copy(IDictionary<string, T> dic)
        {
            foreach (var entry in dic.ToArray())
            {
                this[entry.Key] = entry.Value;
            }
        }

        #endregion

        #region converter
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(string key, T value)
        {
            //return this.Exists(p => p.Key == key && p.Value == value);
            return this.Where(p => p.Key == key && p.Value.Equals(value)).Count() > 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(KeyValuePair<string, T> item)
        {
            return this.Where(p => p.Key == item.Key && p.Value.Equals(item.Value)).Count() > 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual KeyValuePair<string, T> GetItem(string key, T value)
        {
            return this.Where(p => p.Key == key && p.Value.Equals(value)).FirstOrDefault();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="splitter"></param>
        /// <returns></returns>
        public string[] SplitTrim(string name, params char[] splitter)
        {
            var val = this[name];
            return val == null ? null : val.ToString().SplitTrim(splitter);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string JoinArg(string[] str)
        {
            return string.Join("|", str);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string[] ToKeyValueArray()
        {
            var list = new List<string>();

            foreach (var entry in this)
            {
                list.Add(entry.Key);
                list.Add(entry.Value.ToString());
            }
            return list.ToArray();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ToKeyValuePipe()
        {
            string[] val = ToKeyValueArray();
            return JoinArg(val);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, T> ToDictionary()
        {
            //var dict = this
            //   .Select(item => new { Key = item.Key, Value = item.Value })
            //   .Distinct()
            //   .ToDictionary(item => item.Key, item => item.Value, StringComparer.OrdinalIgnoreCase);
            //return dict;

            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ToQueryString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var entry in this)
            {
                sb.Append(entry.Key + "=" + entry.Value + "&");
            }
            return (sb.Length == 0) ? "" : sb.ToString().TrimEnd('&');
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="qs"></param>
        /// <param name="cleanAmp"></param>
        public void LoadQueryString(string qs, bool cleanAmp = true)
        {
            if (qs == null)
            {
                throw new ArgumentNullException("ParseQueryString.qs");
            }

            string str = cleanAmp ? CLeanQueryString(qs) : qs;

            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException("ParseQueryString.qs");
            }
            if (!str.Contains('='))
            {
                throw new ArgumentException("QueryString is incorrect");
            }
            this.Clear();
            foreach (string arg in str.Split(new char[] { '&' }))
            {
                if (!string.IsNullOrEmpty(arg))
                {
                    string[] strArray = arg.Split(new char[] { '=' });
                    if (strArray.Length == 2)
                    {
                        string key = cleanAmp ? strArray[0] : Regx.RegexReplace("amp;", strArray[0], "");
                        this[key] = GenericTypes.Convert<T>(strArray[1]);
                    }
                    else
                    {
                        this[arg] = default(T);
                    }
                }
            }

        }

        #endregion

        #region ParseQueryString
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string ToQueryString(NameValueArgs<T> args)
        {
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<string, T> entry in args)
            {
                sb.AppendFormat("{0}={1}&", entry.Key, entry.Value);
            }
            return sb.ToString().TrimEnd('&');
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static NameValueArgs<T> Parse(System.Web.HttpRequest request)
        {

            if (request == null)
            {
                throw new ArgumentException("invalid request");
            }
            return ParseRawUrl(request.RawUrl);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static NameValueArgs<T> ParseRawUrl(string url)
        {

            if (url == null)
                url = string.Empty;

            string qs = string.Empty;

            if (url.Contains("?"))
            {
                qs = url.Substring(url.IndexOf("?") + 1);
                url = url.Substring(0, url.IndexOf("?"));
            }

            return ParseQueryString(qs);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="qs"></param>
        /// <returns></returns>
        private static string CLeanQueryString(string qs)
        {
            return qs.Replace("&amp;", "&");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="qs"></param>
        /// <param name="cleanAmp"></param>
        /// <returns></returns>
        public static NameValueArgs<T> ParseQueryString(string qs, bool cleanAmp = true)
        {
            NameValueArgs<T> dictionary = new NameValueArgs<T>();

            if (qs == null)
                qs = string.Empty;

            string str = cleanAmp ? CLeanQueryString(qs) : qs;

            if (string.IsNullOrEmpty(str))
            {
                return dictionary;
            }
            if (!str.Contains('='))
            {
                return dictionary;
            }


            //string[] t = qs.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

            //if (t.Length % 2 != 0)
            //{
            //    throw new ArgumentException("queryString is incorrect, Not match key value arguments");
            //}

            //dictionary =(NameValueArgs)
            //   t.Select(item => item.Split('=')).ToDictionary(s => s[0], s => s[1]);
            //return dictionary;


            foreach (string arg in str.Split(new char[] { '&' }))
            {
                if (!string.IsNullOrEmpty(arg))
                {
                    string[] strArray = arg.Split(new char[] { '=' });
                    if (strArray.Length == 2)
                    {
                        string key = cleanAmp ? strArray[0] : Regx.RegexReplace("amp;", strArray[0], "");
                        dictionary[key] = GenericTypes.Convert<T>(strArray[1]);
                    }
                    else
                    {
                        dictionary[arg] = default(T);
                    }
                }
            }

            return dictionary;
        }

        #endregion

        #region  ISerialEntity
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public void EntityWrite(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            ((BinaryStreamer)streamer).WriteDirectDictionary<string, T>(this);
            streamer.Flush();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public void EntityRead(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);
            this.Clear();
            ((BinaryStreamer)streamer).TryReadDirectToDictionary<string, T>(this, false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public NetStream ToStream()
        {
            NetStream stream = new NetStream();
            EntityWrite(stream, null);
            return stream;
        }

        #endregion

        #region ISerialJson
        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static NameValueArgs<T> ParseJson(string json)
        {
            if (json == null)
            {
                return null;
            }
            var nv = new NameValueArgs<T>();
            nv.EntityRead(json, null);
            return nv;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pretty"></param>
        /// <returns></returns>
        public string ToJson(bool pretty = false)
        {
            return EntityWrite(new JsonSerializer(JsonSerializerMode.Write, null), pretty);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="pretty"></param>
        /// <returns></returns>
        public string EntityWrite(IJsonSerializer serializer, bool pretty = false)
        {
            if (serializer == null)
                serializer = new JsonSerializer(JsonSerializerMode.Write, null);

            foreach (var entry in this)
            {
                serializer.WriteToken(entry.Key, entry.Value);
            }
            return serializer.WriteOutput(pretty);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public object EntityRead(string json, IJsonSerializer serializer)
        {
            if (serializer == null)
                serializer = new JsonSerializer(JsonSerializerMode.Read, null);

            serializer.ParseTo(this, json);

            //var dic = serializer.ParseToDictionaryString(json);

            //AddRange(dic.ToArray());

            return this;
        }


        #endregion

    }
}
