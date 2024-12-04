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

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class KeyValueArgs : Dictionary<string, object>, ISerialEntity, IDataRowAdaptor, ISerialJson, IKeyValue<object>
    {
        #region static
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValue"></param>
        /// <returns></returns>
        public static KeyValueArgs Get(params object[] keyValue)
        {
            if (keyValue == null)
                return null;
            return new KeyValueArgs(keyValue);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static KeyValueArgs Convert(IDictionary<string, object> dic)
        {
            if (dic == null)
                return null;
            var nva = new KeyValueArgs();

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
        public KeyValueArgs()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValueList"></param>
        public KeyValueArgs(IEnumerable<KeyValuePair<string, object>> keyValueList)
        {
            Load(keyValueList);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        public KeyValueArgs(byte[] bytes)
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
        public KeyValueArgs(NetStream stream)
        {
            EntityRead(stream, null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValue"></param>
        public KeyValueArgs(object[] keyValue)
        {
            Parse(keyValue);
            //Load(ParseQuery(keyValue));
        }

        //public static KeyValueArgs Create(params string[] keyValue)
        //{
        //    var pair = ParseQuery(keyValue);
        //    KeyValueArgs query = new KeyValueArgs();
        //    query.Load(pair);
        //    return query;
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValueParameters"></param>
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
                this[keyValueParameters[i].ToString()] = keyValueParameters[++i];
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dr"></param>
        public virtual void Prepare(DataRow dr)
        {
            this.ToKeyValue(dr);
        }

        #endregion

        #region properties
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get(string key)
        {
            object value;
            TryGetValue(key, out value);
            return value;
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

        #endregion

        #region collection methods

        /// <summary>
        /// Get this as sorted "IOrderedEnumerable !KeyValuePair string, object"
        /// </summary>
        /// <returns></returns>
        public IOrderedEnumerable<KeyValuePair<string, object>> Sorted()
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
        public new virtual void Add(string key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("KeyValueArgs.Add key");
            }

            base.Add(key, value == null ? null : value.ToString());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValueList"></param>
        void Load(IEnumerable<KeyValuePair<string, object>> keyValueList)
        {
            if (keyValueList == null)
            {
                throw new ArgumentNullException("KeyValueArgs.keyValueList");
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
                throw new ArgumentNullException("KeyValueArgs.qs");
            }

            for (int i = 0; i < qs.Count; i++)
            {
                this[qs.Keys[i]] = qs[i];
            }
        }

        void Copy(IDictionary<string, object> dic)
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
        public bool Contains(string key, object value)
        {
            //return this.Exists(p => p.Key == key && p.Value == value);
            return this.Where(p => p.Key == key && p.Value == value).Count() > 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(KeyValuePair<string, object> item)
        {
            return this.Where(p => p.Key == item.Key && p.Value == item.Value).Count() > 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual KeyValuePair<string, object> GetItem(string key, object value)
        {
            return this.Where(p => p.Key == key && p.Value == value).FirstOrDefault();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object[] ToKeyValueArray()
        {
            var list = new List<object>();

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
        public IDictionary<string, object> ToDictionary()
        {
            //var dict = this
            //   .Select(item => new { Key = item.Key, Value = item.Value })
            //   .Distinct()
            //   .ToDictionary(item => item.Key, item => item.Value, StringComparer.OrdinalIgnoreCase);
            //return dict;

            return this;
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

            ((BinaryStreamer)streamer).WriteDirectDictionary<string, object>(this);
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
            ((BinaryStreamer)streamer).TryReadDirectToDictionary<string, object>(this,false);
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
        public static KeyValueArgs ParseJson(string json)
        {
            if (json == null)
            {
                return null;
            }
            var nv = new KeyValueArgs();
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
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var entry in this)
                sb.AppendFormat("{0}:{1},", entry.Key, entry.Value);
            return sb.ToString().TrimEnd(',');
        }

    }
}
