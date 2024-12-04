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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

#pragma warning disable  CS1591

namespace Nistec.Config
{
    public class ConfigWatcher
    {

        SysFileWatcher _configFileWatcher;
        bool initilaized = false;
        DateTime lastTimeRead = DateTime.MinValue;
        string lastFileRead = "";
        void Init(string configFilename)
        {
            if (initilaized)
                return;
            string filnenmae = Path.Combine(Environment.CurrentDirectory, configFilename);//"Nistec.Cache.Agent.exe.config");

            _configFileWatcher = new SysFileWatcher(filnenmae, true);
            _configFileWatcher.FileChanged += new FileSystemEventHandler(_ConfigFileWatcher_FileChanged);
            initilaized = true;
        }

        public event FileSystemEventHandler FileChanged;

        void _ConfigFileWatcher_FileChanged(object sender, FileSystemEventArgs e)
        {
            ConfigurationManager.RefreshSection("appSettings");
            ConfigurationManager.RefreshSection("connectionStrings");

            if (FileChanged != null)
            {
                if (_configFileWatcher.Filename.ToLower() == e.Name.ToLower())
                {
                    DateTime lastWriteTime = File.GetLastWriteTime(e.FullPath);

                    if (lastWriteTime != lastTimeRead || lastFileRead != e.FullPath)
                    {
                        lastTimeRead = lastWriteTime;
                        lastFileRead = e.FullPath;
                        FileChanged(this, e);
                        Console.WriteLine(e.ToString());
                    }
                }
            }
        }
        
        bool _IsListen;
        void Listen()
        {
            while(_IsListen)
            {
                Thread.Sleep(120000);
            }
        }

        public void Start(string configFilename,bool useListener)
        {
            if (_IsListen)
                return;
            if (!initilaized)
            {
                Init(configFilename);
            }
            if (useListener)
            {
                _IsListen = true;
                Thread th = new Thread(new ThreadStart(Listen));
                th.IsBackground = true;
                th.Start();
            }
            //Netlog.Debug("ConfigFileWatcher started...");
        }

        public void Stop()
        {
            _IsListen = false;
            if (initilaized)
                _configFileWatcher.FileChanged -= new FileSystemEventHandler(_ConfigFileWatcher_FileChanged);
            initilaized = false;
            //Netlog.Debug("ConfigFileWatcher stoped...");

        }

    }
}
