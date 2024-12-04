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
using Nistec.Runtime;

#pragma warning disable  CS1591

namespace Nistec.Generic
{

    public interface INaAttribute
    {
        bool IsNA { get; }
        bool IsIdentity { get; }
    }

    public class PropertyAttributeInfo<T>
    {
      public  PropertyInfo Property { get; set; }
      public  T Attribute { get; set; }
    }
    public class ParameterAttributeInfo<T>
    {
        public ParameterInfo Parameter { get; set; }
        public T Attribute { get; set; }
        public int Position { get; set; }
    }

    #region CustomAttributeProvider Extensions

    public static class CustomAttributeProviderExtensions
    {
        public static T[] GetCustomAttributes<T>(this ICustomAttributeProvider provider) where T : Attribute
        {
            return GetCustomAttributes<T>(provider, true);
        }

        public static T[] GetCustomAttributes<T>(this ICustomAttributeProvider provider, bool inherit) where T : Attribute
        {
            if (provider == null)
                throw new ArgumentNullException("provider");
            T[] attributes = provider.GetCustomAttributes(typeof(T), inherit) as T[];
            if (attributes == null)
            {
                // WORKAROUND: Due to a bug in the code for retrieving attributes            
                // from a dynamic generated parameter, GetCustomAttributes can return            
                // an instance of an object[] instead of T[], and hence the cast above            
                // will return null.            
                return new T[0];
            }
            return attributes;
        }
    }
    #endregion
    public static class AttributeProvider
    {

        public static PropertyAttributeInfo<T> GetCurrentPropertiesInfo<T>(Type t)
        {
            IEnumerable<PropertyAttributeInfo<T>> props = GetPropertiesInfo<T>(t);

            return props.GetEnumerator().Current;
        }

        public static IEnumerable<PropertyAttributeInfo<T>> GetPropertiesInfo<T>(Type t)
        {

            IEnumerable<PropertyAttributeInfo<T>> props = from p in t.GetProperties(true)
                                                          let attr = p.GetCustomAttributes(typeof(T), true)
                                                          where attr.Length == 1
                                                          select new PropertyAttributeInfo<T>() { Property = p, Attribute = (T)attr.First() };

            return props;
        }

       
        public static IEnumerable<object> GetPropertiesValues(object instance, bool sorted = false)
        {
            if (sorted)
                return from p in instance.GetType().GetProperties(true).OrderBy(p => p.Name)
                       select p.GetValue(instance, null);
            else
                return from p in instance.GetType().GetProperties(true)
                       select p.GetValue(instance, null);
        }

        public static IEnumerable<object> GetPropertiesValues(object instance, IEnumerable<string> fields, bool sorted = false)
        {
            if (sorted)
                return from p in instance.GetType().GetProperties(true).Where(p => fields.Contains(p.Name)).OrderBy(p => p.Name)
                       select p.GetValue(instance, null);
            else
                return from p in instance.GetType().GetProperties(true).Where(p => fields.Contains(p.Name))//.OrderBy(P=> fields)
                       select p.GetValue(instance, null);
        }

        public static IEnumerable<object> SelectPropertiesValues(object instance, IEnumerable<string> fields, bool sortedByFields)
        {
            if (sortedByFields)
                return from f in fields
                       join p in instance.GetType().GetProperties(true)//.Where(p => fields.Contains(p.Name))
                            on f equals p.Name
                       let val = p.GetValue(instance, null)
                       select new
                       {
                           val
                       };
            else
                return from p in instance.GetType().GetProperties(true)//.Where(p => fields.Contains(p.Name))
                       join f in fields
                            on p.Name equals f
                       let val = p.GetValue(instance, null)
                       select new
                       {
                           val
                       };
        }
        public static IDictionary<string,object> SelectPropertiesKeyValues(object instance, IEnumerable<string> fields, bool sortedByFields)
        {
            if (sortedByFields)
                return (from f in fields
                       join p in instance.GetType().GetProperties(true)
                            on f equals p.Name
                       let val = p.GetValue(instance, null)
                       let key = f
                       select new { key, val }).ToDictionary(c => c.key, c => c.val);
            else
                return (from p in instance.GetType().GetProperties(true)
                       join f in fields
                            on p.Name equals f
                       let val = p.GetValue(instance, null)
                       let key = f
                       select new { key, val }).ToDictionary(c => c.key, c => c.val);
        }

        public static IEnumerable<PropertyAttributeInfo<T>> GetPropertiesInfo<T>(object instance)
        {

            IEnumerable<PropertyAttributeInfo<T>> props = from p in instance.GetType().GetProperties(true)
             let attr = p.GetCustomAttributes(typeof(T), true)
             where attr.Length == 1
            select new PropertyAttributeInfo<T>() { Property = p, Attribute = (T)attr.First() };

            return props;
        }

        public static IEnumerable<PropertyAttributeInfo<T>> GetPropertiesInfoOptional<T>(object instance)
        {

            IEnumerable<PropertyAttributeInfo<T>> props = from p in instance.GetType().GetProperties(true)
                                                          let attr = p.GetCustomAttributes(typeof(T), true)
                                                          //where attr.Length == 1
                                                          select new PropertyAttributeInfo<T>() { Property = p, Attribute =attr==null?default(T):(T)attr.FirstOrDefault() };

            return props;
        }
       
        public static IEnumerable<ParameterAttributeInfo<T>> GetParametersInfo<T>(MethodInfo mi, int attributeLength )
        {

            IEnumerable<ParameterAttributeInfo<T>> props = 
                from p in mi.GetParameters()
                let attr = p.GetCustomAttributes(typeof(T), true)
                where attr.Length == attributeLength
                orderby p.Position
                select new ParameterAttributeInfo<T>() { Parameter = p, Attribute = (T)attr.FirstOrDefault(), Position = p.Position};

            return props;
        }

        public static T GetCustomAttribute<T>(Type t) where T : Attribute
        {
            IEnumerable<PropertyAttributeInfo<T>> props = GetPropertiesInfo<T>(t);

            if (props != null)
            {
                PropertyAttributeInfo<T> p = props.FirstOrDefault();
                if (p != null)
                    return p.Attribute;
            }
            return null;
        }

        public static T[] GetCustomAttributes<T>(object instance) where T : Attribute
        {
            return instance.GetType().GetCustomAttributes<T>();
        }

       
        public static T[] GetPropertiesAttributes<T>(object instance) where T : Attribute
        {
            return GetPropertiesAttributes<T>(instance, true, false);
        }

        public static T[] GetPropertiesAttributes<T>(object instance, bool canRead, bool canWrite) where T : Attribute
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            PropertyInfo[] properties = instance.GetType().GetProperties(true);
            List<T> list = new List<T>();

            foreach (PropertyInfo property in properties)
            {
                T attr = (T)Attribute.GetCustomAttribute(property, typeof(T));
                if (canWrite && !property.CanWrite)
                {
                    continue;
                }
                if (canRead && !property.CanRead)
                {
                    continue;
                }
                if (attr != null)
                {
                    if (attr is INaAttribute)
                    {
                        if (((INaAttribute)attr).IsNA)
                            continue;
                    }
                    list.Add(attr);
                }
            }

            return list.ToArray();
        }



        public static PropertyInfo[] GetActiveProperties<T>(object instance, bool canRead, bool canWright) where T : Attribute
        {

            PropertyInfo[] properties = instance.GetType().GetProperties(true);
            List<PropertyInfo> list = new List<PropertyInfo>();

            foreach (PropertyInfo property in properties)
            {

                T attr = (T)Attribute.GetCustomAttribute(property, typeof(T));

                if (attr != null)
                {
                    if (canRead && !property.CanRead)
                    {
                        continue;
                    }
                    if (canWright && !property.CanWrite)
                    {
                        continue;
                    }
                    if (attr is INaAttribute)
                    {
                        if (((INaAttribute)attr).IsNA)
                            continue;
                    }
                    list.Add(property);
                }
            }

            return list.ToArray();

        }

        public static PropertyInfo[] GetActiveProperties<T>(object instance, bool canRead, bool canWright, bool disableIdentity) where T : Attribute
        {

            PropertyInfo[] properties = instance.GetType().GetProperties(true);
            List<PropertyInfo> list = new List<PropertyInfo>();

            foreach (PropertyInfo property in properties)
            {

                T attr = (T)Attribute.GetCustomAttribute(property, typeof(T));

                if (attr != null)
                {
                    if (canRead && !property.CanRead)
                    {
                        continue;
                    }
                    if (canWright && !property.CanWrite)
                    {
                        continue;
                    }
                    if (attr is INaAttribute)
                    {
                        if (((INaAttribute)attr).IsNA)
                            continue;
                        if (disableIdentity && ((INaAttribute)attr).IsIdentity)
                            continue;
                    }
                    list.Add(property);
                }
            }

            return list.ToArray();

        }

    }
}
