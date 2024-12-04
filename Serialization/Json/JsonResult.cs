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
using Nistec.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
#pragma warning disable  CS1591

namespace Nistec.Serialization
{
       
   
    //
    // Summary:
    //     Provides the base class for json type.
    public abstract class JsonType
    {
        //
        // Summary:
        //     Initializes a new instance of the System.ValueType class.
        protected JsonType()
        { }
    }

    public class JsonResults
    {
        public static JsonResults Get(object o, JsonSettings settings = null)
        {
            return new JsonResults()
            {
                EncodingName = Encoding.UTF8.EncodingName,
                Option=settings,
                TypeName = o.GetType().FullName,
                Result = JsonSerializer.Serialize(o, settings)
            };
        }

        public static JsonResults Get(object o, Encoding encoding, JsonSettings settings = null)
        {
            return new JsonResults()
            {
                EncodingName = encoding.EncodingName,
                Option = settings,
                TypeName = o.GetType().FullName,
                Result = JsonSerializer.Serialize(o, settings)
            };
        }

        public JsonResults()
        {
            EncodingName = Encoding.UTF8.EncodingName;
        }

        public JsonSettings Option { get; set; }
        public string EncodingName { get; set; }
        public string TypeName { get; set; }
        
        [RawJson]
        public string Result { get; set; }
        
        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
        public object Deserialize(Type returnType)
        {
           return JsonSerializer.Deserialize(Result, returnType, Option);
        }

        public T Deserialize<T>()
        {
            return JsonSerializer.Deserialize<T>(Result, Option);
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

            streamer.WriteString(TypeName);
            streamer.WriteString(EncodingName);
            streamer.WriteString(Result);
            streamer.WriteValue(Option);
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

            TypeName = streamer.ReadString();
            EncodingName = streamer.ReadString();
            Result = streamer.ReadString();
            Option = (JsonSettings)streamer.ReadValue();
        }
        #endregion
    }

    public class JsonResults<T> : JsonResults
    {
        public static JsonResults<T> Get(T o,JsonSettings settings = null)
        {
            return new JsonResults<T>()
            {
                EncodingName = Encoding.UTF8.EncodingName,
                Option = settings,
                TypeName= typeof(T).FullName,
                Result = JsonSerializer.Serialize(o, settings)
            };
        }

        public static JsonResults<T> Get(T o, Encoding encoding, JsonSettings settings = null)
        {
            return new JsonResults<T>()
            {
                EncodingName = encoding.EncodingName,
                Option = settings,
                TypeName = typeof(T).FullName,
                Result = JsonSerializer.Serialize(o, settings)
            };
        }
    }

    public static class JsonExtension
    {

        public static JsonResults ToJsonResult(this DataTable dt, JsonSettings settings = null)
        {
            //if (string.IsNullOrEmpty(dt.TableName))
            //    dt.TableName = "JsonResults";
            JsonResults result = new JsonResults()
            {
                TypeName = typeof(DataTable).FullName,
                Result = JsonSerializer.Serialize(dt, settings)
            };
            return result;
        }

        public static JsonResults ToJsonResult(this DataRow dr, JsonSettings settings = null)
        {
            JsonResults result = new JsonResults()
            {
                TypeName = typeof(DataRow).FullName,
                Result = JsonSerializer.Serialize(dr, settings)
            };
            return result;
        }

        public static JsonResults ToJsonResult(this IDictionary dic, JsonSettings settings = null)
        {
            JsonResults result = new JsonResults()
            {
                TypeName = typeof(IDictionary).FullName,
                Result = JsonSerializer.Serialize(dic, settings)
            };
            return result;
        }

    }
}
