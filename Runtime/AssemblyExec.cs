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
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Security.Policy;
using System.Collections;
using System.Threading;
using System.Text;
#pragma warning disable  CS1591
namespace Nistec.Runtime
{
    /// <summary>
    /// Executes "public static void Main(..)" of assembly in a separate domain.
    /// </summary>
    public class AssemblyExec
    {
        #region Members
        AppDomain appDomain;
        RemoteExecAssembly remoteExecAssembly;
        string assemblyFileName;

        #endregion

        #region Constructor

        public AssemblyExec(string fileNname, string domainName)
        {
            assemblyFileName = fileNname;
            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = Path.GetDirectoryName(assemblyFileName);
            setup.PrivateBinPath = AppDomain.CurrentDomain.BaseDirectory;
            setup.ApplicationName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
            setup.ShadowCopyFiles = "true";
            setup.ShadowCopyDirectories = Path.GetDirectoryName(assemblyFileName);
            appDomain = AppDomain.CreateDomain(domainName, null, setup);

            remoteExecAssembly = (RemoteExecAssembly)appDomain.CreateInstanceFromAndUnwrap(Assembly.GetExecutingAssembly().Location, typeof(RemoteExecAssembly).ToString());
        }
        #endregion

        #region Methods

        public object Execute(string[] args)
        {
            return remoteExecAssembly.ExecuteAssembly(assemblyFileName, args);
        }
        public void Unload()
        {
            AppDomain.Unload(appDomain);
            appDomain = null;
        }
        #endregion

        #region Exec Dll
 
        /// <summary>
        /// Invoke assembly (dll)
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="ClassName"></param>
        /// <param name="methodName"></param>
        /// <param name="Params"></param>
        /// <returns></returns>
        public static object InvokeAssembly(string fileName, string ClassName, string methodName, object[] Params)
        {

            object res = null;

            if (!File.Exists(fileName))
            {
                throw new Exception("Error:  Assembly file not exist");
            }

            try
            {
                //Load the Assembly
                Assembly assembly = Assembly.LoadFile(fileName);

                // Get a Type object corresponding to assembly.
                Type oType = assembly.GetType(ClassName);

                if (oType == null)
                {
                    throw new Exception("Error:  Load the Assembly Class Name Failed");
                }

                Type[] typeArray = null;

                if (Params != null)
                {
                    int prmCount = Params.Length;

                    // Create a Parameters Type array.
                    typeArray = new Type[prmCount];
                    for (int i = 0; i < prmCount; i++)
                    {
                        typeArray.SetValue(typeof(object), i);
                    }
                }


                MethodInfo mi = oType.GetMethod(methodName, typeArray);

                if (mi != null)
                {
                    object obj = assembly.CreateInstance(oType.Name);
                    res = mi.Invoke(obj, Params);
                    return res;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        #endregion
    }
}
			
