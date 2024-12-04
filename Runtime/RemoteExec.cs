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
#pragma warning disable CS1591
namespace Nistec.Runtime

{

	/// <summary>
	/// Invokes static method 'Main' from the assembly.
	/// </summary>
	public class RemoteExecAssembly: MarshalByRefObject
	{

		#region Members

		string workingDir;
		
		#endregion

		#region Methods
		/// <summary>
		/// AppDomain evant handler. This handler will be called if CLR cannot resolve 
		/// referenced local assemblies 
		/// </summary>
		public Assembly ResolveEventHandler(object sender, ResolveEventArgs args)
		{
			return AssemblyResolver.ResolveAssembly(args.Name, workingDir);
		}

		public object ExecuteAssembly(string filename, string[] args)
		{
			workingDir = Path.GetDirectoryName(filename);
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ResolveEventHandler);
			Assembly assembly = Assembly.LoadFrom(filename);
			return InvokeStaticMain(assembly, args);
		}
	
		private object InvokeStaticMain(Assembly compiledAssembly, string[] scriptArgs)
		{
			MethodInfo method = null;
			object res=null;

			foreach (Module m in compiledAssembly.GetModules())
			{
				foreach (Type t in m.GetTypes())
				{
					BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Static ;
					foreach (MemberInfo mi in t.GetMembers(bf))
					{
						if (mi.Name == "Main")
						{
							method = t.GetMethod(mi.Name, bf);
						}
						if (method != null)
							break;
					}
					if (method != null)
						break;
				}
				if (method != null)
					break;
			}
			if (method != null)
			{
				if (method.GetParameters().Length != 0)
				{
					res=method.Invoke( new object(), new object[]{(Object)scriptArgs});
				}
				else
				{
					res=method.Invoke( new object(), null);
				}
			}
			else
			{
				throw new Exception("Cannot find entry point. Make sure script file contains methos: 'public static Main(...)'"); 
			}
			return res;
		}
		#endregion
	}
}
			
