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
using Nistec.Runtime;
using Nistec.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

#pragma warning disable  CS1591

namespace Nistec.Generic
{

    public static class DynamicEntityExtension
    {
        public static T Cast<T>(this DynamicEntity de)
        {
            T entity = ActivatorUtil.CreateInstance<T>();
            PropertyInfo[] p = entity.GetType().GetProperties(true);

            if (entity == null)
            {
                return default(T);
            }
            if (p == null)
            {
                return default(T);
            }
            else
            {
                foreach (var entry in de.Properties)
                {
                    PropertyInfo pi = p.Where(pr => pr.Name == entry.Key).FirstOrDefault();
                    if (pi != null && pi.CanWrite)
                    {
                        pi.SetValue(entity, entry.Value, null);
                    }
                }

                return entity;
            }
        }
    }


    /// <summary>
    ///  The class derived from DynamicObject.
    /// </summary>
    public class DynamicEntity : DynamicObject, ISerialEntity, ISerialJson
    {
        Dictionary<string, object> properties
            = new Dictionary<string, object>();


        internal Dictionary<string, object> Properties
        {
            get { return properties; }
        }
        /// <summary>
        /// Get the number of elements
        /// </summary>
        public int Count
        {
            get
            {
                return properties.Count;
            }
        }


        public override bool TryGetMember(
            GetMemberBinder binder, out object result)
        {
            return properties.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(
            SetMemberBinder binder, object value)
        {
            properties[binder.Name] = value;
            return true;
        }

        [NoSerialize]
        public object this[string key]
        {
            get
            {
                object val;
                properties.TryGetValue(key, out val);
                return val;
            }
            set
            {
                properties[key] = value;
            }
        }

        //public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        //{
        //    dynamic method = properties[binder.Name];
        //    result = method(args[0].ToString(), args[1].ToString());
        //    return true;
        //}

        public static dynamic Get(object source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("DynamicEntity.source");
            }

            Type sourceType = source.GetType();

            if (sourceType == typeof(DynamicEntity))
            {
                return (DynamicEntity)source;
            }
            else if (sourceType == typeof(Dictionary<string, object>))
            {
                return new DynamicEntity((Dictionary<string, object>)source);
            }
            else
            {
                return new DynamicEntity(source);
            }
        }

        
        //public T Cast<T>()
        //{

        //    T entity = ActivatorUtil.CreateInstance<T>();
        //    PropertyInfo[] p = entity.GetType().GetProperties(true);

        //    if (entity == null)
        //    {
        //        return default(T);
        //    }
        //    if (p == null)
        //    {
        //        return default(T);
        //    }
        //    else
        //    {
        //        foreach (var entry in properties)
        //        {
        //            PropertyInfo pi = p.Where(pr => pr.Name == entry.Key).FirstOrDefault();
        //            if (pi != null && pi.CanWrite)
        //            {
        //                pi.SetValue(entity, entry.Value, null);
        //            }
        //        }

        //        return entity;
        //    }
        //}

        public DynamicEntity()
        {

        }

        public DynamicEntity(object source, string name = "", bool allowReadOnly = false)
        {
            if (source == null)
            {
                throw new ArgumentNullException("DynamicEntity.source");
            }
            SerializeTools.MapToDictionary(properties, source, name, allowReadOnly);
        }
        public DynamicEntity(Dictionary<string, object> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("DynamicEntity.source");
            }
            properties = source;
        }
        public DynamicEntity(DynamicEntity source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("DynamicEntity.source");
            }
            properties = source.properties;
        }

        #region  ISerialEntity


        /// <summary>
        /// Write the current object include the body and properties to stream using <see cref="IBinaryStreamer"/>, This method is a part of <see cref="ISerialEntity"/> implementation.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public void EntityWrite(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            streamer.WriteValue(properties);
            streamer.Flush();
        }


        /// <summary>
        /// Read stream to the current object include the body and properties using <see cref="IBinaryStreamer"/>, This method is a part of <see cref="ISerialEntity"/> implementation.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamer"></param>
        public void EntityRead(Stream stream, IBinaryStreamer streamer)
        {
            if (streamer == null)
                streamer = new BinaryStreamer(stream);

            properties = (Dictionary<string, object>)streamer.ReadValue();
        }

        #endregion

        #region ISerialJson

        public string EntityWrite(IJsonSerializer serializer, bool pretty = false)
        {
            if (serializer == null)
                serializer = new JsonSerializer(JsonSerializerMode.Write, null);
            if (pretty)
            {
                var json = serializer.Write(properties);
                return JsonSerializer.Print(json);
            }
            return serializer.Write(properties);
        }

        public object EntityRead(string json, IJsonSerializer serializer)
        {
            if (serializer == null)
                serializer = new JsonSerializer(JsonSerializerMode.Read, null);
            properties = serializer.Read<Dictionary<string, object>>(json);

            return this;
        }

        #endregion
    }
}