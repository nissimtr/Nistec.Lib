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
using System.Reflection.Emit;
using System.Reflection;
using System.Collections;
using System.Data;
using System.Collections.Specialized;
using System.Collections.Concurrent;
#pragma warning disable  CS1591

namespace Nistec.Serialization
{
    public sealed class JsonSettings
    {
        public JsonSettings()
        {
            SerializeNullValues = true;

            JsonDateFormat = JsonDateFormat.iso;//.dynamic;//.iso;

            UseUTCDateTime = false;//true;

            ShowReadOnlyProperties = false;

            UseTypesExtension = false;//true;

            IgnoreCaseOnDeserialize = false;

            _EnableAnonymousTypes = false;

            _UseExtensions = false;//true;

            UseEscapedUnicode = false;//true;

            UseExtraKeyValueDictionary = false;

            UseDatasetSchema = false;// true;

            UseBinaryGuid = false;//true;

            UseEnumValues = false;

            UseUninitializedObject = false;

            EnableDateTimeMilliseconds = false;

        }

        /// <summary>
        /// Serialize null values to the output (default = True)
        /// </summary>
        public bool SerializeNullValues {get;set;}
        /// <summary>
        /// JsonDateFormat (default = iso)
        /// </summary>
        public JsonDateFormat JsonDateFormat { get; set; }
        /// <summary>
        /// Use the UTC date format (default = False)
        /// </summary>
        public bool UseUTCDateTime { get; set; }
        /// <summary>
        /// Show readonly properties in the output (default = False)
        /// </summary>
        public bool ShowReadOnlyProperties { get; set; }
        /// <summary>
        /// Use the $types extension to optimise the output json (default = False)
        /// </summary>
        public bool UseTypesExtension { get; set; }
        /// <summary>
        /// Ignore case on json and deserialize (default = False).
        /// </summary>
        public bool IgnoreCaseOnDeserialize { get; set; }
        bool _EnableAnonymousTypes;
        /// <summary>
        /// Anonymous types have read only properties (default = False). 
        /// </summary>
        public bool EnableAnonymousTypes
        {
            get { return _EnableAnonymousTypes; }
            set
            {
                _EnableAnonymousTypes = value;
                if (value == true)
                    ShowReadOnlyProperties = true;
            }
        }
        bool _UseExtensions;
        /// <summary>
        /// Enable extensions $types, $type, $map (default = False)
        /// </summary>
        public bool UseExtensions
        {
            get { return _UseExtensions; }
            set { _UseExtensions = value; if (value == false)UseTypesExtension = false; }
        }
        /// <summary>
        /// Use escaped unicode i.e. \uXXXX format for non ASCII characters (default = True)
        /// </summary>
        public bool UseEscapedUnicode { get; set; }
        /// <summary>
        /// Output string key dictionaries as "k"/"v" format (default = False) 
        /// </summary>
        public bool UseExtraKeyValueDictionary { get; set; }
        /// <summary>
        /// Use Dataset Schema format (default = False)
        /// </summary>
        public bool UseDatasetSchema { get; set; }
        /// <summary>
        /// Use the fast GUID format (default = False)
        /// </summary>
        public bool UseBinaryGuid { get; set; }
        /// <summary>
        /// Output Enum values instead of names (default = False)
        /// </summary>
        public bool UseEnumValues { get; set; }
        /// <summary>
        /// If any object has no default constructor 
        /// then all initial values within the class will be ignored and will be not set (default = False).
        /// </summary>
        public bool UseUninitializedObject { get; set; }
        /// <summary>
        /// Enable DateTime milliseconds i.e. yyyy-MM-dd HH:mm:ss.nnn (default = false)
        /// </summary>
        public bool EnableDateTimeMilliseconds { get; set; }
 

        internal List<Type> IgnoreAttributes;
        /// <summary>
        /// Add XmlIgnoreAttribute attributes.
        /// </summary>
        /// <param name="types"></param>
        public void AddIgnoreXmlAttributes(params Type[] types)
        {
            if (types == null)
                return;
            if (IgnoreAttributes == null)
            {
                IgnoreAttributes = new List<Type> { typeof(System.Xml.Serialization.XmlIgnoreAttribute) };
            }
            IgnoreAttributes.AddRange(types);
        }
        
    }
   
    public sealed class JsonSchema
    {
        public List<string> Info;
        public string Name;
    }


}
