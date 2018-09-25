﻿using Nistec.Data;
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

    [Serializable]
    public class NameValueArgs : Dictionary<string, string>, ISerialEntity, IDataRowAdaptor, ISerialJson, INameValue, IKeyValue<string>
    {
        #region static

        public static NameValueArgs Get(params string[] keyValue)
        {
            if (keyValue == null)
                return null;
            return new NameValueArgs(keyValue);
        }

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

        public NameValueArgs()
        {
        }
        public NameValueArgs(IEnumerable<KeyValuePair<string, string>> keyValueList)
        {
            Load(keyValueList);
        }
        public NameValueArgs(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                EntityRead(ms, null);
            }
        }
        public NameValueArgs(NetStream stream)
        {
            EntityRead(stream, null);
        }

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

        public string Get(string key)
        {
            string value;
            TryGetValue(key, out value);
            return value;
            //return this[key];
        }

        public TV Get<TV>(string key)
        {
            return GenericTypes.Convert<TV>(this[key]);
        }

        public TV Get<TV>(string key, TV defaultValue)
        {
            return GenericTypes.Convert<TV>(this[key], defaultValue);
        }

        public TV GetEnum<TV>(string key, TV defaultValue)
        {
            return GenericTypes.ConvertEnum<TV>(this[key], defaultValue);
        }


        #endregion

        #region collection methods

        /// <summary>
        /// Get this as sorted <see cref="IOrderedEnumerable<KeyValuePair<string, object>>"/>
        /// </summary>
        /// <returns></returns>
        public IOrderedEnumerable<KeyValuePair<string, string>> Sorted()
        {
            var sortedDict = from entry in this orderby entry.Key ascending select entry;
            return sortedDict;
        }
        #endregion

        #region Loaders

        public virtual void Add(string key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("NameValueArgs.Add key");
            }

            base.Add(key, value == null ? null : value.ToString());
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
        public bool Contains(string key, string value)
        {
            //return this.Exists(p => p.Key == key && p.Value == value);
            return this.Where(p => p.Key == key && p.Value == value).Count() > 0;
        }

        public new bool Contains(KeyValuePair<string, string> item)
        {
            return this.Where(p => p.Key == item.Key && p.Value == item.Value).Count() > 0;
        }

        public virtual KeyValuePair<string, string> GetItem(string key, string value)
        {
            return this.Where(p => p.Key == key && p.Value == value).FirstOrDefault();
        }
        public string[] SplitTrim(string name, params char[] splitter)
        {
            var val = this[name];
            return val == null ? null : val.SplitTrim(splitter);
        }
        public static string JoinArg(string[] str)
        {
            return string.Join("|", str);
        }
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
        public string ToKeyValuePipe()
        {
            string[] val = ToKeyValueArray();
            return JoinArg(val);
        }
        public IDictionary<string, string> ToDictionary()
        {
            //var dict = this
            //   .Select(item => new { Key = item.Key, Value = item.Value })
            //   .Distinct()
            //   .ToDictionary(item => item.Key, item => item.Value, StringComparer.OrdinalIgnoreCase);
            //return dict;

            return this;
        }

        public string ToQueryString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var entry in this)
            {
                sb.Append(entry.Key + "=" + entry.Value + "&");
            }
            return (sb.Length == 0) ? "" : sb.ToString().TrimEnd('&');
        }

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

        public static string ToQueryString(NameValueArgs args)
        {
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<string, string> entry in args)
            {
                sb.AppendFormat("{0}={1}&", entry.Key, entry.Value);
            }
            return sb.ToString().TrimEnd('&');
        }

        public static NameValueArgs Parse(System.Web.HttpRequest request)
        {

            if (request == null)
            {
                throw new ArgumentException("invalid request");
            }
            return ParseRawUrl(request.RawUrl);
        }

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

        public void EntityWrite(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            ((BinaryStreamer)streamer).WriteDirectDictionary<string,string>(this);
            streamer.Flush();
        }

        public void EntityRead(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);
            this.Clear();
            ((BinaryStreamer)streamer).TryReadDirectToDictionary<string, string>(this,false);
        }

        public NetStream ToStream()
        {
            NetStream stream = new NetStream();
            EntityWrite(stream, null);
            return stream;
        }

        #endregion

        #region ISerialJson

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

        public string ToJson(bool pritty = false)
        {
            return EntityWrite(new JsonSerializer(JsonSerializerMode.Write, null), pritty);
        }

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
