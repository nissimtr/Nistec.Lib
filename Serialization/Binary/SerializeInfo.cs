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
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Nistec.IO;
using System.Data;
using System.Xml;
using Nistec.Generic;
#pragma warning disable  CS1591

namespace Nistec.Serialization
{

    public struct ValueTypeInfo
    {
        public object ItemValue { get; set; }
        public Type ItemType { get; set; }
    }

    [Serializable]
    public class SerializeInfo 
    {
        #region ctor

        public SerializeInfo()
        {
            EntityType = typeof(SerializeInfo);
            Formatter = Formatters.BinarySerializer;
         }

        public SerializeInfo(Type type, Formatters formater)
        {
            EntityType = type;
            Formatter = formater;
        }
        #endregion

        #region collection methods


        public void Add(string key, object value, Type typeBase = null)
        {
            var data = Data;
            //if (data.Contains(key))
            //    data.RemoveItem(key);
            var valueType = new ValueTypeInfo() { ItemValue = value, ItemType = typeBase };
            //data.Add(new KeyValuePair<string, ValueTypeInfo>(key, valueType));
            data[key] = valueType;
        }

        public object GetValue(string key)
        {
            ValueTypeInfo value;
            if (Data.TryGetValue(key,out value))
            {
                return value.ItemValue;
            }
            return null;

            //return Data.GetItem(key).Value.ItemType;
        }

        public T GetValue<T>(string key)
        {
            ValueTypeInfo value;
            if (Data.TryGetValue(key, out value))
            {
                return GenericTypes.Convert <T>(value.ItemValue);
            }
            return default(T);
            //return GenericTypes.Convert<T>(Data.GetItem(key).Value.ItemValue);
        }

        public ValueTypeInfo GetItem(string key)
        {
            ValueTypeInfo value;
            Data.TryGetValue(key, out value);
            return value;
            //return Data.GetItem(key).Value;
        }

        #endregion

        #region properties


        Dictionary<string,ValueTypeInfo> m_Data;
        public Dictionary<string, ValueTypeInfo> Data
        {
            get
            {
                if (m_Data == null)
                {
                    m_Data = new Dictionary<string, ValueTypeInfo>();
                }
                return m_Data;
            }
        }

        public int Count
        {
            get
            {
                return Data.Count;
            }
        }

        public string EntityTypeName
        {
            get
            {
                if (EntityType == null)
                    return null;
                return EntityType.FullName;
            }
        }

        public Type EntityType
        {
            get;
            private set;
        }

        public Formatters Formatter
        {
            get;
            private set;
        }

        public NetStream ContextStream
        {
            get;
            private set;
        }
        #endregion

        #region methods


        public void Encode(NetStream stream)
        {
            var info = Data;

            BinaryStreamer streamer = new BinaryStreamer(stream);

            streamer.WriteContextType(SerialContextType.SerialContextType);

            streamer.WriteString(EntityTypeName);
            streamer.Write((Int32)Formatter);
            streamer.Write(info.Count);
            foreach (KeyValuePair<string, ValueTypeInfo> kvp in info)
            {
                streamer.WriteString(kvp.Key);
                streamer.WriteAny(kvp.Value.ItemValue, kvp.Value.ItemType);
            }

            streamer.Flush();
        }

        public void Decode(NetStream stream)
        {
            BinaryStreamer streamer = new BinaryStreamer(stream);
            m_Data = new Dictionary<string,ValueTypeInfo>();
            EntityType = streamer.ReadType();
            Formatter = (Formatters)streamer.ReadInt32();
            int count = streamer.ReadInt32();
            if (count < 0) return;
            for (int i = 0; i < count; i++)
            {
                this.Add(streamer.ReadString(), streamer.ReadAny());
            }
        }

        #endregion
    }

} 
