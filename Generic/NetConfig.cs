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
using System.Configuration;
using System.Collections.Generic;

using Nistec;
using System.Xml;
using System.Collections;
using System.Web.Configuration;
using System.Linq;
using System.Reflection;

#pragma warning disable  CS1591

namespace Nistec.Generic
{
         
    /// <summary>
    /// NetConfig is wrapper of ConfigurationManager
    /// </summary>
    public static class NetConfig
    {
        #region settings

        public static System.Collections.Specialized.NameValueCollection AppSettings
        {
            get { return ConfigurationManager.AppSettings; }
        }

        public static ConnectionStringSettingsCollection ConnectionStrings
        {
            get { return ConfigurationManager.ConnectionStrings; }
        }
        #endregion

        #region connection

        public const string DefaultConnectionName="Default";

        public static string DefaultConnectionString()
        {
            var cnn = ConfigurationManager.ConnectionStrings[DefaultConnectionName];
            return cnn == null ? null : cnn.ConnectionString;
        }

        public static string ConnectionString(string key)
        {
            var cnn = ConfigurationManager.ConnectionStrings[key];
            return cnn == null ? null : cnn.ConnectionString;
        }

        public static System.Configuration.ConnectionStringSettings ConnectionSpliter(string key)
        {
            if (key == null)
                return null;
            if (key.Contains('@'))
            {
                string[] args = key.Split('@');
                if (args.Length > 1)
                    return new ConnectionStringSettings(args[0], args[1], "SqlServer");
            }

            return ConfigurationManager.ConnectionStrings[key];
        }

        public static System.Configuration.ConnectionStringSettings ConnectionSettings(string key)
        {
            if (key == null)
                return null;
            return ConfigurationManager.ConnectionStrings[key];
        }

 
        public static ConnectionStringSettings ConnectionContext(string key, bool optionalAppSettings = true)
        {
 
            if (optionalAppSettings && key.ToLower().StartsWith("cnn_"))
            {
                string ProviderName = NZ("cnn_provider", "SqlServer");
                string ConnectionName = key.Substring("cnn_".Length);
                return new ConnectionStringSettings(ConnectionName, ConfigurationManager.AppSettings[key], ProviderName);
            }
            return ConfigurationManager.ConnectionStrings[key];
        }

        public static string ConnectionContext(string key, ref string ConnectionName, out string ProviderName)
        {
            if (key.ToLower().StartsWith("cnn_"))
            {
                ProviderName = NZ("cnn_provider", "SqlServer");
                ConnectionName = key.Substring("cnn_".Length);
                return ConfigurationManager.AppSettings[key];
            }
            ProviderName = "SqlServer";
            return ConnectionString(key);
        }

        #endregion

        #region values

        public static string NZ(string key)
        {
            return Types.NZ(ConfigurationManager.AppSettings[key],"");
        }
        
        public static string NZ(string key,string valueIfNull)
        {
            return Types.NZ(ConfigurationManager.AppSettings[key], valueIfNull);
        }

        public static T Get<T>(string key, T valueIfNull)
        {
            return GenericTypes.Convert<T>(ConfigurationManager.AppSettings[key], valueIfNull);
        }

        public static T Get<T>(string key)
        {
            return GenericTypes.Convert<T>(ConfigurationManager.AppSettings[key]);
            //return (T)Get(key);
        }

        private static object Get(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
        
        public static string ToString(string key)
        {
           return ConfigurationManager.AppSettings[key];
        }
        #endregion
     
        #region custom element

        public static XmlTable GetCustomConfig(string elementName)
        {
            return GetCustomConfig(elementName,IsWeb());
        }
        /// <summary>
        /// CacheSettings ctor
        /// </summary>
        /// <param name="elementName"></param>
        /// <param name="isWeb"></param>
        public static XmlTable GetCustomConfig(string elementName, bool isWeb)
        {
            XmlDocument doc = new XmlDocument();
            string filePath = null;
            if (isWeb)
            {
                var wconfig =
      System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
                filePath = wconfig.FilePath;
            }
            else
            {
                System.Configuration.Configuration config =
       ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                filePath = config.FilePath;
            }


            doc.Load(filePath);
            XmlNode node = GetCustomElement(elementName, isWeb);

            //XmlNode node = xml.SelectSingleNode("//queueSettings");
            if (node == null)
            {
                throw new ArgumentException("Inavlid Xml Root, 'CacheSettings' ");
            }

            XmlTable table = new XmlTable(node);

            return table;
        }

        public static XmlNode GetCustomElement(string elementName)
        {
            return GetCustomElement(elementName, IsWeb());
        }

        static bool IsWeb()
        {
            return System.Web.HttpContext.Current != null;
        }

        public static XmlNode GetCustomElement(string elementName, bool isWeb)
        {
            string filePath = null;
            if (isWeb)
            {
                var wconfig =
      System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
                filePath = wconfig.FilePath;
            }
            else
            {
                System.Configuration.Configuration config =
       ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                filePath = config.FilePath;
            }
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            XmlNode root = doc.SelectSingleNode("//" + elementName);

            return root;
        }

        public static IDictionary GetCustomAttributes(string elementName)
        {

            return GetCustomAttributes(elementName, IsWeb());
        }

        public static IDictionary GetCustomAttributes(string elementName, bool isWeb)
        {
            XmlNode root = GetCustomElement(elementName, isWeb);
            if (root == null)
            {
                throw new Exception("Element '" + elementName + "' not found");
            }
            XmlTable table = new XmlTable(root);
            return table.Data;
        }

        #endregion

        public static System.Configuration.Configuration GetConfiguration()
        {

            System.Configuration.Configuration configuration = null;
            if (System.Web.HttpContext.Current != null)
            {
                configuration =
                    System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
            }
            else
            {
                configuration =
                    ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            }
            return configuration;
        }
    }

    #region web custom config

    /// <summary>
    /// Represent a configuration section within a configuration file.
    /// </summary>
    public class NetConfigSection : ConfigurationSection
    {
        static NetConfigSection Config;

        /// <summary>
        /// Get <see cref="NetConfigSection"/>.
        /// </summary>
        /// <returns></returns>
        public static NetConfigSection GetConfig(string section)
        {
            if (Config == null)
                Config = (NetConfigSection)System.Configuration.ConfigurationManager.GetSection(section) ?? new NetConfigSection();
            return Config;
        }

    }



    /// <summary>
    /// Represents a configuration element within a configuration file.
    /// </summary>
    public class NetConfigItem : ConfigurationElement
    {

        [ConfigurationProperty("key", IsRequired = true)]
        public string Key
        {
            get
            {
                return this["key"] as string;
            }
        }
        [ConfigurationProperty("value", IsRequired = true)]
        public string Value
        {
            get
            {
                return this["value"] as string;
            }
        }
    }

    /// <summary>
    /// Represents a configuration element containing a collection of child elements.
    /// </summary>
    public class NetConfigItems : ConfigurationElementCollection
    {

        public string Get(string key)
        {
            NetConfigItem item = this[key];
            if (item == null)
                return null;
            return item.Value;
        }

        public T Get<T>(string key, T defaultValue)
        {
            return GenericTypes.Convert<T>(Get(key), defaultValue);
        }
        [Serialization.NoSerialize]
        public NetConfigItem this[int index]
        {
            get
            {
                return base.BaseGet(index) as NetConfigItem;
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }
        [Serialization.NoSerialize]
        public new NetConfigItem this[string key]
        {
            get { return (NetConfigItem)BaseGet(key); }
            set
            {
                if (BaseGet(key) != null)
                {
                    BaseRemoveAt(BaseIndexOf(BaseGet(key)));
                }
                BaseAdd(value);
            }
        }

        protected override System.Configuration.ConfigurationElement CreateNewElement()
        {
            return new NetConfigItem();
        }

        protected override object GetElementKey(System.Configuration.ConfigurationElement element)
        {
            return ((NetConfigItem)element).Key;
        }
    }

    #endregion


}
