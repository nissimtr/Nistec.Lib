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
using System.Resources;
using System.Globalization;
using System.Reflection;
using System.Collections;
using Nistec.Generic;
using System.Text.RegularExpressions;
using System.IO;

#pragma warning disable  CS1591

namespace Nistec.Generic
{
    public static class ResourceExtension
    {
        public static string Translate(this ResourceSet rs, string name)
        {
            string resource = rs.GetString(name, true);
            return resource ?? name;
        }
    }

    /// <summary>
    /// EntityLang interface
    /// </summary>
    public interface ILocalizer
    {
        CultureInfo Culture { get; set; }
        T GetValue<T>(string name);
        T GetValue<T>(CultureInfo culture, string name);

        string GetString(string name);
        string GetString(string name, string defaultValue);
        string GetString(CultureInfo culture, string name);
        string GetString(CultureInfo culture, string name, string defaultValue);

        string TranslateWords(CultureInfo cultureInfo, string text);
 
        string TranslateLines(CultureInfo cultureInfo, string text);

        string Translate(CultureInfo cultureInfo, string[] seprartor, string text);

        string Translate(CultureInfo cultureInfo, string pattern, string text);
    }

    public class Localizer : ILocalizer//NetResourceManager
    {
        
		#region Ctor
		private ResourceManager resources;
		public static	CultureInfo DefualtCulture;

		private	static CultureInfo	_CultureInfo;

        public static ResourceManager GetResourceManager(string resource,Type type)
        {
            return new ResourceManager(resource, type.Assembly);
        }

        public static ResourceManager GetResourceManager(string resource)
        {
            return new ResourceManager(resource, Assembly.GetExecutingAssembly());
        }

        protected void Init(string culture, string configKey)
        {
            _CultureInfo = new CultureInfo(culture, false);
            this.resources = new ResourceManager(NetConfig.AppSettings[configKey], Assembly.GetExecutingAssembly());
        }

        protected void Init(string culture, ResourceManager resource)
        {
            _CultureInfo = new CultureInfo(culture, false);
            this.resources = resource;
        }


        protected void Init(string culture, string resource, Assembly assembly)
        {
            _CultureInfo = new CultureInfo(culture, false);

            this.resources = new ResourceManager(resource, assembly);
            //this.resources = new ResourceManager("Nistec.Framework.Resources.SR", base.GetType().Module.Assembly);
        }

        public Localizer()
        {
            _CultureInfo = CultureInfo.CurrentCulture;
        }

        public Localizer(string culture, string configKey)
        {

            _CultureInfo = new CultureInfo(culture, false);
            this.resources = new ResourceManager(NetConfig.AppSettings[configKey], Assembly.GetExecutingAssembly());
        }

        public Localizer(string culture, string resource, Assembly assembly)
        {
            _CultureInfo = new CultureInfo(culture, false);
			
            this.resources = new ResourceManager(resource, assembly);
        }

        public Localizer(string culture, ResourceManager resource)
        {
            _CultureInfo = new CultureInfo(culture, false);
            this.resources = resource;
        }

		#endregion

		#region Cultures

 
        public ResourceManager RM
        {
            get { return resources; }
        }

		public  CultureInfo Culture
		{
			get
			{
				if(_CultureInfo==null)
				{
                    _CultureInfo = Localizer.DefualtCulture;
				}
				return _CultureInfo;
			} 
			set
			{
                _CultureInfo = value;
			}
		}
		

		#endregion
 
		#region Methods

        public static CultureInfo Current
        {
            get { return CultureInfo.CurrentUICulture; }
        }

        public ResourceSet GetResourceSet(CultureInfo cultureInfo)
        {
            ResourceSet resourceSet = RM.GetResourceSet(cultureInfo, true, true);
            return resourceSet;
        }

        public string TranslateWords(string culture, string text)
        {
            CultureInfo cultureInfo = new CultureInfo(culture);
            return TranslateWords(cultureInfo, text);
        }

        public string TranslateWords(CultureInfo cultureInfo, string text)
        {
            ResourceSet rs = RM.GetResourceSet(cultureInfo, true, true);
            if (rs == null)
                return text;
            string[] src = text.Split(new Char[] { ' ', ',', '.', ':', ';', '!', '?', '\t' });
            foreach (string s in src)
            {
                if (s.Length > 0)
                {
                    text.Replace(s, rs.Translate(s));
                }
            }
            return text;
        }
        

        public string TranslateLines(CultureInfo cultureInfo, string text)
        {
            ResourceSet rs = RM.GetResourceSet(cultureInfo, true, true);
            if (rs == null)
                return text;

            using (StringReader sr = new StringReader(text))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Length > 0)
                        text.Replace(line, rs.Translate(line));
                }//while
            }//using
            return text;
       }

        public string Translate(CultureInfo cultureInfo, string[] seprartor, string text)
        {
            ResourceSet rs = RM.GetResourceSet(cultureInfo, true, true);
            if (rs == null)
                return text;

            string[] src = text.Split(seprartor, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in src)
            {
                if (s.Length > 0)
                    text.Replace(s, rs.Translate(s));
            }
            return text;
        }

        public string Translate(CultureInfo cultureInfo, string pattern, string text)
        {
            ResourceSet rs = RM.GetResourceSet(cultureInfo, true, true);
            if (rs == null)
                return text;

            string[] src = Regex.Split(text,pattern, RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
            foreach (string s in src)
            {
                if (s.Length > 0)
                    text.Replace(s, rs.Translate(s));
            }
            return text;
        }

		public object GetObject(string name)
		{
            return GetObject(null, name);
		}

		public object GetObject(CultureInfo culture, string name)
		{
	        if (RM == null)
			{
				return null;
			}
            return RM.GetObject(name, culture);
		}


        public T GetValue<T>(string name)
        {
            return GenericTypes.Convert<T>(GetString(name));
        }

        public T GetValue<T>(CultureInfo culture, string name)
        {
            return GenericTypes.Convert<T>(GetString(culture, name));
        }

        public string GetString(string name, string defaultValue)
		{
            return GetString(null, name, defaultValue);
		}

        public string GetString(string name)
        {
            return GetString(null, name, name);
        }

        public string GetString(CultureInfo culture, string name)
        {
            return GetString(culture, name, name);
        }

        public string GetString(CultureInfo culture, string name, string defaultValue)
        {
           try
            {
                if (RM == null)
                {
                    return defaultValue;
                }
                if (culture == null)
                {
                    culture = Culture;
                }
                return GenericTypes.NZorEmpty(resources.GetString(name, culture), defaultValue);
            }
            catch
            {
                try
                {
                    return GenericTypes.NZorEmpty(resources.GetString(name, Localizer.DefualtCulture), defaultValue);
                }
                catch
                {
                    return defaultValue;
                }
            }
        }


		public string GetStringFormat(string name, params object[] args)
		{
			return GetStringFormat(null, name, args);
		}

		public string GetStringFormat(CultureInfo culture,string name, string args)
		{
			return GetStringFormat(null, name, new object[]{args});
		}

 
		public string GetStringFormat(CultureInfo culture, string name, params object[] args)
		{
	        if (RM == null)
			{
				return null;
			}
			string text1 = name;
			if(culture==null)
			{
				culture=Culture;
			}

			try
			{
                text1 = GenericTypes.NZorEmpty(resources.GetString(name, culture), name);
			}
			catch
			{
				try
				{
                    text1 = GenericTypes.NZorEmpty(resources.GetString(name, Culture), name);
				}
				catch
				{
					text1 = name;
				}
			}

			if ((args != null) && (args.Length > 0))
			{
				return string.Format(text1, args);
			}
			return text1;
		}


		#endregion
   }
}
