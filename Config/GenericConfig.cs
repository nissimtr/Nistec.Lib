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
using Nistec.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

#pragma warning disable  CS1591

namespace Nistec.Config
{
    public class GenericConfig<T> where T: IConfigurable
    {
        #region members
        public const string fileName = "ConnectionSettings.config";

        int synchronized;
        bool _initialized;
        SysFileWatcher _SyncFileWatcher;
        Action<string> OnLog;
        #endregion

        #region properties
        public string RootNode { get; private set; }
        public string FilePath { get; private set; }
        public bool EnableAsyncTask { get; set; }
        //public bool ReloadAllItemsOnChange { get; set; }
        public bool EnableSyncFileWatcher { get; set; }
        #endregion

        #region ctor
        public GenericConfig(string path, string rootNode, Action<string> onLog)
        {
            _initialized = false;
            RootNode = rootNode;
            FilePath = Path.Combine(path,fileName);
            EnableSyncFileWatcher = true;
            //ReloadAllItemsOnChange = false;
            EnableAsyncTask = false;
            OnLog = onLog;
        }
        #endregion

        #region start/stop

      

        /// <summary>
        /// Start Cache Synchronization.
        /// </summary>
        public void Start(bool enableSyncFileWatcher= true)
        {
            if (_initialized)
            {
                return;
            }
            EnableSyncFileWatcher = enableSyncFileWatcher;
            //ReloadAllItemsOnChange = reloadOnChange;
            if (enableSyncFileWatcher)
            {
                _SyncFileWatcher = new SysFileWatcher(FilePath, true);
                _SyncFileWatcher.FileChanged += new FileSystemEventHandler(_SyncFileWatcher_FileChanged);

                OnSyncFileChange(new FileSystemEventArgs(WatcherChangeTypes.Created, _SyncFileWatcher.SyncPath, _SyncFileWatcher.Filename));
            }
            else
            {
                LoadConfigFile(FilePath);
            }

            _initialized = true;
            WriteLog("GenericConfig Started!");
        }

        /// <summary>
        /// Stop Cache Synchronization.
        /// </summary>
        public void Stop()
        {
            _initialized = false;
            WriteLog("GenericConfig Stoped!");
        }

        #endregion

        #region private events
        void _SyncFileWatcher_FileChanged(object sender, FileSystemEventArgs e)
        {
            Task task = Task.Factory.StartNew(() => OnSyncFileChange(e));
        }

        void OnSyncFileChange(object args)//GenericEventArgs<string> e)
        {
            FileSystemEventArgs e = (FileSystemEventArgs)args;

            LoadConfigFile(e.FullPath);
        }
        #endregion

        #region public events

        protected void WriteLog(string log)
        {
            if (OnLog != null)
                OnLog(log);
        }

        /// <summary>
        /// Sync Error Event Handler.
        /// </summary>
        public event EventHandler<GenericEventArgs<string>> SyncError;
        /// <summary>
        /// Sync LoadCompleted Event Handler.
        /// </summary>
        public event EventHandler<GenericEventArgs<T[]>> LoadCompleted;

        /// <summary>
        /// On Error Occured
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnError(string e)
        {
            if (SyncError != null)
                SyncError(this, new GenericEventArgs<string>(e));
        }

        protected virtual void OnLoadCompleted(T[] e)
        {
            if (LoadCompleted != null)
                LoadCompleted(this, new GenericEventArgs<T[]>(e));
        }

        #endregion

        #region Load xml config
        /// <summary>
        /// Load sync cache from config file.
        /// </summary>
        public void LoadConfig()
        {

            string file = FilePath;
            LoadConfigFile(file);
        }

        //public void LoadConfigFile(string file, int retrys)
        //{

        //    int counter = 0;
        //    bool reloaded = false;
        //    while (!reloaded && counter < retrys)
        //    {
        //        reloaded = LoadConfigFile(file);
        //        counter++;
        //        if (!reloaded)
        //        {
        //            WriteLog("LoadConfigFile retry: " + counter);
        //            Thread.Sleep(100);
        //        }
        //    }
        //    if (reloaded)
        //    {
        //        OnSyncReload(new GenericEventArgs<string>(file));
        //    }
        //}

        /// <summary>
        /// Load sync cache from config file.
        /// </summary>
        /// <param name="file"></param>
        public bool LoadConfigFile(string file)
        {
            if (string.IsNullOrEmpty(file))
                return false;
            Thread.Sleep(1000);
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(file);
                LoadConfig(doc);
                return true;
            }
            catch (Exception ex)
            {
                WriteLog("LoadConfigFile error: " + ex.Message);
                OnError("LoadConfigFile error " + ex.Message);
                return false;
            }
        }
        /// <summary>
        /// Load sync cache from xml string argument.
        /// </summary>
        /// <param name="xml"></param>
        public bool LoadConfig(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return true;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                LoadConfig(doc);
                return true;
            }
            catch (Exception ex)
            {
                WriteLog("LoadConfig error: " + ex.Message);
                OnError("LoadConfig error " + ex.Message);
                return false;
            }
        }
        /// <summary>
        /// Load sync cache from <see cref="XmlDocument"/> document.
        /// </summary>
        /// <param name="doc"></param>
        public void LoadConfig(XmlDocument doc)
        {
            if (doc == null)
                return;

            XmlNode items = doc.SelectSingleNode("//"+RootNode);
            if (items == null)
                return;
            LoadItems(items);
        }

        protected virtual void LoadItems(XmlNode node)
        {
            if (node == null)
                return;
            try
            {
                if (0 == Interlocked.Exchange(ref synchronized, 1))
                {
                    XmlNodeList list = node.ChildNodes;
                    if (list == null)
                    {
                        WriteLog("Load items is empty");
                        return;
                    }
                    var newItems = GetItems(list);

                    if (newItems == null || newItems.Length == 0)
                    {
                        throw new Exception("Can not Load config items, Items not found");
                    }

                    OnLoadCompleted(newItems);
                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("Load config items error: {0}", ex.Message));

                OnError("Load config error " + ex.Message);

            }
            finally
            {
                //Release the lock
                Interlocked.Exchange(ref synchronized, 0);
            }
        }

        protected XmlTable[] GetXmlItems(XmlNodeList list)
        {
            List<XmlTable> items = new List<XmlTable>();

            foreach (XmlNode n in list)
            {
                if (n.NodeType == XmlNodeType.Comment)
                    continue;
                XmlTable xt = new XmlTable(n);
                items.Add(xt);
            }
            return items.ToArray();
        }

        protected virtual T[] GetItems(XmlNodeList list)
        {
            List<T> items = new List<T>();

            foreach (XmlNode n in list)
            {
                if (n.NodeType == XmlNodeType.Comment)
                    continue;
                XmlTable xt = new XmlTable(n);
                T instance = ActivatorUtil.CreateInstance<T>();
                instance.LoadConfig(xt);
                if (items.Exists(s => s.IsEqual(instance)))
                {
                    WriteLog("Duplicate in item, entity: " + instance.ToString());
                    continue;
                }
                items.Add(instance);
            }
            return items.ToArray();
        }

        #endregion load xml config

    }
}
