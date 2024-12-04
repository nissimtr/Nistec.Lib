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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
#pragma warning disable  CS1591
namespace Nistec
{
    public class ConverterUtil
    {
        static readonly ConcurrentDictionary<Type, TypeConverter> _ctorCache = new ConcurrentDictionary<Type, TypeConverter>();

        public static TypeConverter GetConverter<T>()
        {
            return GetConverter(typeof(T));
        }

        public static TypeConverter GetConverter(Type type)
        {
            try
            {
                if (type == null)
                    return null;
                TypeConverter converter = null;
                if (_ctorCache.TryGetValue(type, out converter))
                {
                    return converter;
                }
                else
                {
                    converter = TypeDescriptor.GetConverter(type);
                    if (converter != null)
                    {
                        _ctorCache[type] = converter;
                    }
                    return converter;
                }
            }
            catch (Exception exc)
            {
                throw new Exception(string.Format("Failed to create instance for type '{0}' from assembly '{1}'",
                    type.FullName, type.AssemblyQualifiedName), exc);
            }
        }

        public static void ClearCache()
        {
            _ctorCache.Clear();
        }
    }
}
