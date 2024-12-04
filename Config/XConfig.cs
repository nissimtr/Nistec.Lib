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
using System.Collections;
using System.Text;
using System.Xml;
using System.IO;
using System.Reflection;
using Nistec.Generic;
using Nistec.Xml;
using Nistec.Runtime;

#pragma warning disable  CS1591

namespace Nistec.Config
{
   
    public class XConfigSettings
    {
        public const string DefaultPassword = "giykse876435365&%$^#%@$#@)_(),kxa;l bttsklf12[]}{}{)(*XCJHG^%%";

        public XConfigSettings()
        {
            AutoSave = true;
            Filename = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + ".xconfig";
            RootTag = "xconfig";
            UseFileWatcher = true;
            Encrypted = false;
            Password = DefaultPassword;
        }

        /// <summary>
        /// Get or Set value indicating is XConfig will save changes to config file when each item value has changed, 
        /// Default is true;
        /// </summary>
        public bool AutoSave{get;set;}
      
        /// <summary>
        /// Get or Set The config full file path.
        /// Default is (Current Location) + .xconfig;
        /// </summary>
        public string Filename{get;set;}
      
        /// <summary>
        /// Get or Set The Config root tag.
        /// Default is appSettings;
        /// </summary>
        public string RootTag{get;set;}
        /// <summary>
        /// Use event of a System.IO.FileSystemWatcher class.
        /// Default is true;
        /// </summary>
        public bool UseFileWatcher { get; set; }

        /// <summary>
        /// Get or Set the password for encryption.
        /// Default is internal password;
        /// </summary>
        public string Password{ get; set; }
        /// <summary>
        /// Use Encryption configuration.
        /// Default is false;
        /// </summary>
        public bool Encrypted{ get; set; }


    }



    /// <summary>
    /// Represent a Config file as Dictionary key-value
    /// <example>
    /// <sppSttings>
    /// <myname value='nissim' />
    /// <mycompany value='mcontrol' />
    /// </sppSttings>
    /// </example>
    /// </summary>
    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    public class XConfig
    {

        static readonly XConfig _Instance = new XConfig(new XConfigSettings());

        public static XConfig Instance
        {
            get { return _Instance; }
        }

        IDictionary<string,object> dictionary;
        XConfigSettings settings;
        bool ignorEvent = false;
        public event ConfigChangedHandler ItemChanged;
        public event EventHandler ConfigFileChanged;
        public event GenericEventHandler<string> ErrorOcurred;

        protected virtual void OnErrorOcurred(GenericEventArgs<string> e)
        {
            if (ErrorOcurred != null)
            {
                ErrorOcurred(this,e);
            }
        }

        protected virtual void OnItemChanged(string key, object value)
        {
            OnItemChanged(new ConfigChangedArgs("xconfig",key, value));
        }

        protected virtual void OnItemChanged(ConfigChangedArgs e)
        {
            if (settings.AutoSave)
            {
                Save();
            }
            if (ItemChanged != null)
                ItemChanged(this, e);
        }

         protected virtual void OnConfigFileChanged(EventArgs e)
        {
            FileToDictionary();

            if (ConfigFileChanged != null)
                ConfigFileChanged(this, e);
        }


        
        /// <summary>
        /// XConfig ctor with a specefied filename
        /// </summary>
        /// <param name="filename"></param>
        public XConfig(string filename)
        {
            dictionary = new Dictionary<string, object>();
            this.settings = new XConfigSettings();
            if (string.IsNullOrEmpty(filename) == false)
                this.settings.Filename = filename;
            Init();
        }

        /// <summary>
        /// XConfig ctor with a specefied Dictionary
        /// </summary>
        /// <param name="dict"></param>
        public XConfig(IDictionary<string, object> dict)
        {
            this.settings = new XConfigSettings();
            dictionary = dict;
            Init();
        }

        /// <summary>
        /// XConfig ctor with default filename CallingAssembly '.mconfig' in current direcory
        /// </summary>
        public XConfig(XConfigSettings settings)
        {
            this.settings = settings;
            dictionary = new Dictionary<string, object>();
        }


        public void Load(IDictionary<string, object> dict)
        {
            dictionary = dict;
            ignorEvent = true;
            try
            {
                DictionaryToFile();
                FileToDictionary();
            }
            finally
            {
                ignorEvent = false;
            }
        }

        #region watcher

        FileSystemWatcher watcher;

        /// <summary>
        /// Initilaize the file system watcher
        /// </summary>
        void InitWatcher()
        {
            string fpath = Path.GetDirectoryName(settings.Filename);
            string filter = Path.GetExtension(settings.Filename);

             // Create a new FileSystemWatcher and set its properties.
            watcher = new FileSystemWatcher();
            watcher.Path = fpath;
            /* Watch for changes in LastAccess and LastWrite times, and 
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only watch text files.
            watcher.Filter ="*"+ filter;// "*.txt";

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(WatchFile_Changed);
            watcher.Created += new FileSystemEventHandler(WatchFile_Changed);
            watcher.Deleted += new FileSystemEventHandler(WatchFile_Changed);
            watcher.Renamed += new RenamedEventHandler(WatchFile_Renamed);

            // Begin watching.
            watcher.EnableRaisingEvents = true;

            // Wait for the user to quit the program.
            //Console.WriteLine("Press \'q\' to quit the sample.");
            //while (Console.Read() != 'q') ;
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
                OnConfigFileChanged(EventArgs.Empty);
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
            if (e.FullPath == settings.Filename)
            {
                OnConfigFileChanged(EventArgs.Empty);
            }
        }


        void WatchFile_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath == settings.Filename)
            {
                if (ignorEvent == false)
                    OnFileChanged(e);
            }
        }
        void WatchFile_Renamed(object sender, RenamedEventArgs e)
        {
            if (e.OldFullPath == settings.Filename || e.FullPath == settings.Filename)
            {
                OnFileRenamed(e);
            }
        }
        #endregion

        #region properties


        /// <summary>
        /// Get or Set value by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [Serialization.NoSerialize]
        public object this[string key]
        {
            get 
            {
                if (!dictionary.ContainsKey(key))
                {
                    return null;
                }
                return dictionary[key]; 
            }

            set
            {
                if (dictionary.ContainsKey(key))
                {
                    if (dictionary[key] != value)
                    {
                        dictionary[key] = value;
                        OnItemChanged(key, value);
                    }
                }
                else
                {
                    dictionary[key] = value;
                    OnItemChanged(key, value);
                }

            }
        }

        /// <summary>
        /// Get value indicating if the config file exists
        /// </summary>
        public bool FileExists
        {
            get
            {
                return File.Exists(settings.Filename);
            }
        }

 
        /// <summary>
        /// Get all items as IDictionary
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, object> ToDictionary()
        {
            return dictionary;
        }

        #endregion

        /// <summary>
        /// Read Config File
        /// </summary>
        public void Read()
        {
            try
            {
                FileToDictionary();
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
                DictionaryToFile();
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
            Console.WriteLine("<" + settings.RootTag + ">");

            foreach (KeyValuePair<string, object> entry in dictionary)
            {
                Console.WriteLine("key={0}, value={1}, Type={2}", entry.Key , entry.Value,entry.Value==null? "String": entry.Value.GetType().ToString());
            }
            Console.WriteLine("</" + settings.RootTag + ">");

        }
   
    
        /// <summary>
        /// Init new config file from dictionary
        /// </summary>
        private void Init()
        {
 
            XmlBuilder builder = new XmlBuilder();
            builder.AppendXmlDeclaration();
            builder.AppendEmptyElement(settings.RootTag, 0);
            foreach (KeyValuePair<string,object> entry in dictionary)
            {
                if (entry.Value == null)
                {
                    builder.AppendElementAttributes(0, "Add", "", new string[]{"key", entry.Key,"value", string.Empty, "type", "String"});
                }
                else
                {
                    builder.AppendElementAttributes(0, "Add", "", new string[] { "key", entry.Key, "value", entry.Value.ToString(), "type", entry.Value.GetType().ToString() });
                }
            }

            if (settings.Encrypted)
                EncryptFile(builder.Document.OuterXml);
            else
                builder.Document.Save(settings.Filename);

            if (settings.UseFileWatcher)
                InitWatcher();
        }

        private void FileToDictionary()
        {

            Dictionary<string,object> dict=new Dictionary<string,object>();

            XmlDocument doc = new XmlDocument();

            if (settings.Encrypted)
                doc.LoadXml(DecryptFile());
            else
                doc.Load(settings.Filename);

            Console.WriteLine("Load Config: " + settings.Filename);
            //XmlParser parser=new XmlParser(filename);

            XmlNode app = doc.SelectSingleNode("//" + settings.RootTag);
            XmlNodeList list = app.ChildNodes;

            for (int i = 0; i < list.Count; i++)
            {
                XmlNode node = list[i];
                var attkey = node.Attributes["key"];
                if (attkey != null)
                    dict[attkey.Value] = GetValue(node);

            }
            dictionary = dict;
        }

        private object GetValue(XmlNode node)
        {
            string value = node.Attributes["value"].Value;
            string type ="string";
            XmlAttribute attrib= node.Attributes["type"];

            if (attrib == null)
            {
                return value;
            }

            type = attrib.Value;

            return Types.StringToObject(type, value);
         }

        private void ValidateConfig()
        {
            if (!FileExists && dictionary.Count > 0)
            {
                Init();
            }
        }

        private void DictionaryToFile()
        {
            ValidateConfig();

            XmlDocument doc = new XmlDocument();
            if (settings.Encrypted)
                doc.LoadXml(DecryptFile());
            else
                doc.Load(settings.Filename);

            XmlNode app = doc.SelectSingleNode("//" + settings.RootTag);
            XmlNodeList list = app.ChildNodes;

            for (int i = 0; i < list.Count; i++)
            {
                XmlNode node = list[i];
              
                string key=node.Attributes["key"].Value;
                object value=dictionary[key];

                if (value != null)
                {
                  node.Attributes["value"].Value =value.ToString();
                  node.Attributes["type"].Value = value.GetType().ToString();
                }
                else
                {
                    node.Attributes["value"].Value = string.Empty;
                    node.Attributes["type"].Value ="String";
                }
            }

            if (settings.Encrypted)
            {
                EncryptFile(doc.OuterXml);
            }
            else
                doc.Save(settings.Filename);
        }

       

        private string DecryptFile()
        {
            Encryption en = new Encryption(settings.Password);
            return en.DecryptFileToString(settings.Filename, true);
        }

        private bool EncryptFile(string ouput)
        {
            Encryption en = new Encryption(settings.Password);
            return en.EncryptStringToFile(ouput, settings.Filename, true);
        }

     
    }
}
