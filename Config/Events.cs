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

#pragma warning disable CS1591

namespace Nistec.Config
{	
	
	/// <summary>
	///   EventArgs class to be passed as the second parameter of a <see cref="Nistec.Config.ConfigChangedArgs" /> event handler. </summary>
	/// <remarks>
	///   This class provides all the information relevant to the change made to the Config.
	///   It is also used as a convenient base class for the ConfigChangingArgs class which is passed 
	///   as the second parameter of the <see cref="Nistec.Config.ConfigChangingArgs" /> event handler. </remarks>
	/// <seealso cref="ConfigChangingArgs" />
	public class ConfigChangedArgs : EventArgs
	{   
		// Fields
		private readonly string m_section;
		private readonly string m_entry;
		private readonly object m_value;

		/// <summary>
		///   Initializes a new instance of the ConfigChangedArgs class by initializing all of its properties. </summary>
		/// <param name="section">
		///   The name of the section involved in the change or null. </param>
		/// <param name="entry">
		///   The name of the entry involved in the change, or if changeType is set to Other, the name of the method/property that was changed. </param>
		/// <param name="value">
		///   The new value for the entry or method/property, based on the value of changeType. </param>
		public ConfigChangedArgs(string section, string entry, object value) 
		{
			m_section = section;
			m_entry = entry;
			m_value = value;
		}
		
		/// <summary>
		///   Gets the name of the section involved in the change, or null if not applicable. </summary>
		public string Section
		{
			get 
			{
				return m_section;
			}
		}

		/// <summary>
		///   Gets the name of the entry involved in the change, or null if not applicable. </summary>
		/// <remarks> 
		///   If ChangeType is set to Other, this property holds the name of the 
		///   method/property that was changed. </remarks>
		public string Entry
		{
			get 
			{
				return m_entry;
			}
		}
		
		/// <summary>
		///   Gets the new value for the entry or method/property, based on the value of ChangeType. </summary>
		public object Value
		{
			get 
			{
				return m_value;
			}
		}
	}

	/// <summary>
	///   EventArgs class to be passed as the second parameter of a <see cref="Nistec.Config.ConfigChangingArgs" /> event handler. </summary>
	/// <remarks>
	///   This class provides all the information relevant to the change about to be made to the Config.
	///   Besides the properties of ConfigChangedArgs, it adds the Cancel property which allows the 
	///   event handler to prevent the change from happening. </remarks>
	/// <seealso cref="ConfigChangedArgs" />
	public class ConfigChangingArgs : ConfigChangedArgs
	{   
		private bool m_cancel;
		
		/// <summary>
		///   Initializes a new instance of the ConfigChangingArgs class by initializing all of its properties. </summary>
		/// <param name="section">
		///   The name of the section involved in the change or null. </param>
		/// <param name="entry">
		///   The name of the entry involved in the change, or if changeType is set to Other, the name of the method/property that was changed. </param>
		/// <param name="value">
		///   The new value for the entry or method/property, based on the value of changeType. </param>
		public ConfigChangingArgs(string section, string entry, object value) :
			base(section, entry, value)
		{
		}
		                    
		/// <summary>
		///   Gets or sets whether the change about to the made should be canceled or not. </summary>
		/// <remarks> 
		///   By default this property is set to false, meaning that the change is allowed.  </remarks>
		public bool Cancel
		{
			get 
			{
				return m_cancel;
			}
			set
			{
				m_cancel = value;
			}
		}
	}
   
	/// <summary>
	///   Definition of the <see cref="Nistec.Config.ConfigChangingArgs" /> event handler. </summary>
	/// <remarks>
	///   This definition complies with the .NET Framework's standard for event handlers.
	///   The sender is always set to the Config object that raised the event. </remarks>
	public delegate void ConfigChangingHandler(object sender, ConfigChangingArgs e);

	/// <summary>
	///   Definition of the <see cref="Nistec.Config.ConfigChangedArgs" /> event handler. </summary>
	/// <remarks>
	///   This definition complies with the .NET Framework's standard for event handlers.
	///   The sender is always set to the Config object that raised the event. </remarks>
	public delegate void ConfigChangedHandler(object sender, ConfigChangedArgs e);
}

