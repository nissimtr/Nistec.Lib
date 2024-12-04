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
using System.Reflection;
using System.IO;
using System.Xml;
using Nistec.Runtime;
using Nistec.Generic;
using Nistec.Xml;
using System.Collections;

#pragma warning disable  CS1591
namespace Nistec
{
    /// <summary>
    /// Represent a Config file as Dictionary key-value
    /// </summary>
    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    public class AppConfig
    {
        public const string DefaultPassword = "giykse876435365&%$^#%@$#@)_(),kxa;l bttsklf12[]}{}{)(*XCJHG^%%";
        const string FileExt = ".cnf";
        const string FileAppName = "AppConfig";
        const string FieldKey = "key";
        const string FieldValue = "value";
        const string configRoot = "appSettings";

        string filename;
        
        Dictionary<string, object> _values;
        
        string password;
        bool encrypted = false;
        bool auteSave = false;
        bool hasChanges = false;

        public event GenericEventHandler<string,object> ItemChanged;
        public event EventHandler ConfigFileChanged;
        public event GenericEventHandler<string> ErrorOcurred;

        protected virtual void OnErrorOcurred(GenericEventArgs<string> e)
        {
            if (ErrorOcurred != null)
            {
                ErrorOcurred(this, e);
            }
        }

        protected virtual void OnItemChanged(string key, object value)
        {
            OnItemChanged(new GenericEventArgs<string, object>(key, value));
        }

        protected virtual void OnItemChanged(GenericEventArgs<string, object> e)
        {
            if (auteSave)
            {
                Save();
            }
            if (ItemChanged != null)
                ItemChanged(this, e);
        }

        protected virtual void OnConfigFileChanged(EventArgs e)
        {
            if (ConfigFileChanged != null)
                ConfigFileChanged(this, e);
        }



        /// <summary>
        /// AppConfig ctor with a specefied filename
        /// </summary>
        /// <param name="filename"></param>
        public AppConfig(string filename)
        {
            if (filename == null || filename.Length==0)
            {
                throw new ArgumentNullException("filename");
            }
            _values = new Dictionary<string, object>();
            this.filename = filename;
            LoadConfig();
        }

        /// <summary>
        /// AppConfig ctor with a specefied Dictionary
        /// </summary>
        /// <param name="dic"></param>
        public AppConfig(Dictionary<string,object> dic)
        {
            if (dic == null)
            {
                throw new ArgumentNullException("dic");
            }
            _values = dic;
            hasChanges = true;
        }

        /// <summary>
        /// AppConfig ctor with default filename CallingAssembly '.cnf' in current direcory
        /// </summary>
        public AppConfig()
        {
            Assembly assm = Assembly.GetCallingAssembly();
            string location = assm.Location;

            _values = new Dictionary<string, object>();
            this.filename = location + FileExt;
            LoadConfig();
        }


        #region watcher

        SysFileWatcher watcher;

        /// <summary>
        /// Initilaize the file system watcher
        /// </summary>
        public void InitWatcher()
        {
            string fpath = Path.GetDirectoryName(filename);
            string filter = Path.GetExtension(filename);

            // Create a new FileSystemWatcher and set its properties.
            watcher = new SysFileWatcher(fpath,filter);
            watcher.FileChanged += new FileSystemEventHandler(WatchFile_Changed);
        }


        /// <summary>
        /// Occoured when file changed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnFileChanged(FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
            if (e.ChangeType == WatcherChangeTypes.Changed || e.ChangeType == WatcherChangeTypes.Created)
            {

                OnConfigFileChanged(EventArgs.Empty);//  ConfigToDictionary();
            }
        }
        /// <summary>
        /// Occoured when file renamed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnFileRenamed(RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
            if (e.FullPath == Filename)
            {
                OnConfigFileChanged(EventArgs.Empty);// ConfigToDictionary();
            }
        }


        void WatchFile_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath == Filename)
            {
                OnFileChanged(e);
            }
        }
        void WatchFile_Renamed(object sender, RenamedEventArgs e)
        {
            if (e.OldFullPath == Filename || e.FullPath == Filename)
            {
                OnFileRenamed(e);
            }
        }
        #endregion

        #region properties

        /// <summary>
        /// Get string value by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetItem(string key)
        {
            object o = this[key];
            if (o == null)
            {
                return "";
            }
            return o.ToString();
        }


        /// <summary>
        /// Get or Set value by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [Serialization.NoSerialize]
        public string this[string key]
        {
            get
            {
                return GetValue(key);
            }
            set
            {
                if (SetNonEqual(key, value))
                {
                    //do nothing.
                }
            }
        }

        /// <summary>
        /// Get or Set value indicating is AppConfig will save changes to config file when each item value has changed 
        /// </summary>
        public bool AutoSave
        {
            get
            {
                return auteSave;
            }
            set
            {
                auteSave = value;
            }
        }

        /// <summary>
        /// Get value indicating if AppConfig has changes. 
        /// </summary>
        public bool HasChanges
        {
            get
            {
                return hasChanges;
            }
        }

        /// <summary>
        /// Get or Set The config full file path
        /// </summary>
        public string Filename
        {
            get
            {
                if (string.IsNullOrEmpty(filename))
                {
                    filename = Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().Location) + "\\" + FileAppName + FileExt;
                }
                return filename;
            }
            set
            {
                filename = value;
            }
        }
        /// <summary>
        /// Get value indicating if the config file exists
        /// </summary>
        public bool FileExists
        {
            get
            {
                return File.Exists(Filename);
            }
        }
        /// <summary>
        /// Get The Config root tag
        /// </summary>
        public string RootTag
        {
            get { return configRoot; }
        }
        /// <summary>
        /// Get or Set the password for encryption
        /// </summary>
        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                encrypted = !string.IsNullOrEmpty(password);
            }
        }
        /// <summary>
        /// Get all items as IDictionary
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, object> ToDictionary()
        {
            return _values;
        }

        #endregion

        #region method GetValue

        /// <summary>
        /// Determines whether the XmlTable contains the specified key.
        /// </summary>
        /// <param name="key">Key of value to get.</param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            return _values.ContainsKey(key);
        }

        /// <summary>
        /// Determines whether the XmlTable contains the specified value.
        /// </summary>
        /// <param name="value">value to get.</param>
        /// <returns></returns>
        public bool ContainsValue(object value)
        {
            return _values.ContainsValue(value);
        }

        /// <summary>
        /// Gets value from name/value table.
        /// </summary>
        /// <param name="key">Key of value to get.</param>
        /// <returns></returns>
        public string GetValue(string key)
        {
            object val = null;
            if (_values.TryGetValue(key, out val))
            {
                return val == null ? null : val.ToString();
            }
            return null;
        }

        /// <summary>
        /// Gets value from name/value table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            object val = null;
            if (_values.TryGetValue(key, out val))
            {
                return GenericTypes.Convert<T>(val);
            }
            return default(T);
        }

        /// <summary>
        /// Gets value from name/value table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="valueIfNull"></param>
        /// <returns></returns>
        public T Get<T>(string key, T valueIfNull)
        {
            object val = null;
            if (_values.TryGetValue(key, out val))
            {
                return GenericTypes.Convert<T>(val, valueIfNull);
            }
            return valueIfNull;
        }


        public bool Set<T>(string key, T value)
        {
            return SetNonEqual(key, value);
        }

        public bool Set(string key, object value)
        {
            return SetNonEqual(key, value);
        }

        bool SetNonEqual(string key, object value)
        {
            bool shouldSet = true;
            object val = null;
            if (_values.TryGetValue(key, out val))
            {
                shouldSet = (value != val);
            }
            if (shouldSet)
            {
                _values[key] = value;
                hasChanges = true;
                if (auteSave)
                {
                    SaveConfig();
                }
                OnItemChanged(key, value);
            }
            return shouldSet;
        }

        #endregion
 

        /// <summary>
        /// Read Config File
        /// </summary>
        public void Read()
        {
            try
            {
                LoadConfig();
            }
            catch (Exception ex)
            {
                OnErrorOcurred(new GenericEventArgs<string>("Error occured when try to read Config To Dictionary, Error: " + ex.Message));
            }
        }

        /// <summary>
        /// Save Changes 
        /// </summary>
        public void Save()
        {
            try
            {
                SaveConfig();
            }
            catch (Exception ex)
            {
                OnErrorOcurred(new GenericEventArgs<string>("Error occured when try to save Dictionary To Config, Error: " + ex.Message));
            }
        }

        /// <summary>
        /// Print all item to Console
        /// </summary>
        public void PrintConfig()
        {
            Console.WriteLine("<" + configRoot + ">");
            foreach (KeyValuePair<string, object> entry in _values)
            {
                Console.WriteLine("key={0}, value={1}, Type={2}", entry.Key, entry.Value, entry.Value == null ? "String" : entry.Value.GetType().ToString());
            }
            Console.WriteLine("</" + configRoot + ">");

        }


        /// <summary>
        /// Save config to file.
        /// </summary>
        public void SaveConfig()
        {
            XmlBuilder builder = new XmlBuilder();
            builder.AppendXmlDeclaration();
            builder.AppendEmptyElement(configRoot, 0);
            foreach (KeyValuePair<string, object> entry in _values)
            {
                if (entry.Value == null)
                {
                    builder.AppendElementAttributes(0, "Add", "", new string[] { "key", entry.Key, "value", string.Empty, "type", "String" });
                }
                else
                {
                    builder.AppendElementAttributes(0, "Add", "", new string[] { "key", entry.Key, "value", entry.Value.ToString(), "type", entry.Value.GetType().ToString() });
                }
            }

            if (encrypted)
                EncryptFile(builder.Document.OuterXml);
            else
                builder.Document.Save(Filename);
        }

        private void LoadConfig()
        {
            LoadConfig(Filename);
        }

        public void LoadConfig(string filename)
        {
            if (!File.Exists(filename))
                return;
            this.filename = filename;

            Dictionary<string, object> dict = new Dictionary<string, object>();

            XmlDocument doc = new XmlDocument();

            if (encrypted)
                doc.LoadXml(DecryptFile());
            else
                doc.Load(Filename);

            Console.WriteLine("Load Config: " + Filename);
            XmlNode app = doc.SelectSingleNode("//" + configRoot);
            XmlNodeList list = app.ChildNodes;

            for (int i = 0; i < list.Count; i++)
            {
                XmlNode node = list[i];
                dict[node.Attributes["key"].Value] = GetValue(node);
            }
            _values = dict;
            hasChanges = false;
        }

        private object GetValue(XmlNode node)
        {
            string value = node.Attributes["value"].Value;
            string type = "string";
            XmlAttribute attrib = node.Attributes["type"];

            if (attrib == null)
            {
                return value;
            }

            type = attrib.Value;

            if (type.ToLower().EndsWith("string"))
            {
                return value;
            }

            if (type.ToLower().Contains("int"))
            {
                return Types.ToInt(value, (int)0);
            }

            if (type.ToLower().Contains("date"))
            {
                return Types.ToDateTime(value, DateTime.Now);
            }

            if (type.ToLower().Contains("bool"))
            {
                return Types.ToBool(value, false);
            }

            if (type.ToLower().EndsWith("decimal"))
            {
                return Types.ToDecimal(value, 0.0m);
            }

            if (type.ToLower().EndsWith("float"))
            {
                return Types.ToFloat(value, 0.0F);
            }

            if (type.ToLower().EndsWith("double"))
            {
                return Types.ToDouble(value, 0.0D);
            }
            if (type.ToLower().EndsWith("byte"))
            {
                return Types.ToByte(value, 0);
            }

            return value;
        }

          private string DecryptFile()
        {
            Encryption en = new Encryption(password);
            return en.DecryptFileToString(Filename, true);
        }

        private bool EncryptFile(string ouput)
        {
            Encryption en = new Encryption(password);
            return en.EncryptStringToFile(ouput, Filename, true);
        }


    }
}
