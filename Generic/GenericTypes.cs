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
using System.ComponentModel;
using System.Collections;
using System.Reflection;
using Nistec.Runtime;
#pragma warning disable  CS1591
namespace Nistec
{
    public enum ConvertDescriptor
    {
        Default,
        Implicit,
        Entity
    }

    /// <summary>
    /// Represent a generic type converter
    /// </summary>
    public static class GenericTypes
    {


        /// <summary>
        /// Convert
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="cd"></param>
        /// <returns></returns>
        public static T Convert<T>(object value, ConvertDescriptor cd)
        {
            switch (cd)
            {
                case ConvertDescriptor.Entity:
                    return Cast<T>(value);
                case ConvertDescriptor.Implicit:
                    return ImplicitConvert<T>(value);
                case ConvertDescriptor.Default:
                default:
                    return Convert<T>(value);
            }
        }
        /// <summary>
        /// Convert
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <param name="cd"></param>
        /// <returns></returns>
        public static T Convert<T>(object value, T defaultValue, ConvertDescriptor cd)
        {
            switch (cd)
            {
                case ConvertDescriptor.Entity:
                    return Cast<T>(value);
                case ConvertDescriptor.Implicit:
                    return ImplicitConvert<T>(value, defaultValue);
                case ConvertDescriptor.Default:
                default:
                    return Convert<T>(value, defaultValue);
            }
        }

        /// <summary>
        /// Get the Default value for given generic type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Default<T>()
        {
            return IsNullable(typeof(T)) ? default(T) : ActivatorUtil.CreateInstance<T>();
        }

        /// <summary>
        /// Get the Default value for given object type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object Default(Type type)
        {
            //return type.IsNullableType() ? null : ActivatorUtil.CreateInstance(type);
            return IsNullable(type) ? null:ActivatorUtil.CreateInstance(type);
        }

        public static bool IsNullable(Type type)
        {
            if (type.IsNullableType() ||
             Nullable.GetUnderlyingType(type) != null ||
             !type.IsValueType)
                return true; // ref-type
            return false; // value-type
        }

      
        public static bool IsValueType(Type type)
        {
            return (type.IsValueType && Nullable.GetUnderlyingType(type) != null);
        }
 
   
        /// <summary>
        /// Convert input to sepcified type, if input is null or unable to convert
        /// Creates an instance of the specified type using that type's default constructor.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static object ConvertProperty(object input, PropertyInfo property)
        {
            try
            {
                
                if (property == null)
                {
                    return input;
                }
                if (input == null || input == DBNull.Value)
                {
                    return Default(property.PropertyType);
                }
                if (input.GetType() == property.PropertyType)
                {
                    return input;
                }
                var converter = ConverterUtil.GetConverter(property.PropertyType);
                if (converter != null)
                {

                    return converter.ConvertFromString(input.ToString());
                }
            }
            catch
            {
                return Default(property.PropertyType);
            }
            return null;
        }

        public static object Convert(object input, Type type)
        {
            try
            {

                if (type == null)
                {
                    return input;
                }
                if (input == null || input == DBNull.Value)
                {
                    return Default(type);
                }
                if (input.GetType() == type)
                {
                    return input;
                }
                if (type == typeof(object))
                {
                    return input;
                }
                var converter =ConverterUtil.GetConverter(type);
                if (converter != null)
                {
                    return converter.ConvertFromString(input.ToString());
                }
            }
            catch
            {
                return Default(type);
            }
            return null;
        }

        /// <summary>
        /// Convert string to struct.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T ConvertTo<T>(string value) where T : struct
        {
            if (value == null)
                return Default<T>();
            var converter = ConverterUtil.GetConverter(typeof(T));
            if (converter != null)
            {
                try
                {
                    T result = (T)converter.ConvertFromString(value);
                    return result;
                }
                catch
                {
                }
            }
            return Default<T>();
        }
        /// <summary>
        /// Convert string to struct.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T ConvertTo<T>(string value, T defaultValue) where T : struct
        {
            if (value == null)
                return defaultValue;
            var converter = ConverterUtil.GetConverter(typeof(T));
            if (converter != null)
            {
                try
                {
                    T result = (T)converter.ConvertFromString(value);
                    return result;
                }
                catch
                {

                }
            }
            return defaultValue;
        }
        /// <summary>
        /// Convert string to object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T ConvertObject<T>(object value)
        {
            try
            {
                if (value == null || value == DBNull.Value)
                    return Default<T>();
                if (typeof(T) == value.GetType())
                    return (T)value;
                var converter = ConverterUtil.GetConverter(typeof(T));
                if (converter != null)
                {
                    return (T)converter.ConvertFrom(value);
                }
                return (T)value;
            }
            catch
            {
            }
            return Default<T>();
        }

        /// <summary>
        /// Generic Converter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T Convert<T>(object value)
        {
            if (value == null || value == DBNull.Value)
                return Default<T>();
            if (typeof(T) == typeof(Object) || typeof(T) == value.GetType())
                return (T)value;
            var converter = ConverterUtil.GetConverter(typeof(T));
            if (converter != null)
            {
                try
                {
                    return (T)converter.ConvertFromString(value.ToString());
                }
                catch { }
            }
            return Default<T>();
        }

        /// <summary>
        /// Generic Converter with default value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T Convert<T>(object value, T defaultValue)
        {
            if (value == null || value == DBNull.Value)
                return defaultValue;
            if (typeof(T) == typeof(Object) || typeof(T) == value.GetType())
                return (T)value;
            var converter = ConverterUtil.GetConverter(typeof(T));
            if (converter != null)
            {
                try
                {
                    return (T)converter.ConvertFromString(value.ToString());
                }
                catch { }
            }
            return defaultValue;
        }

        /// <summary>
        /// Generic implicit Convert an object with default value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T ImplicitConvert<T>(object value, T defaultValue)
        {
            if (value == null || value == DBNull.Value)
                return defaultValue;
            try
            {
                return (T)value;
            }
            catch { }
            return defaultValue;
        }

        /// <summary>
        /// Generic implicit Convert an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T ImplicitConvert<T>(object value)
        {
            if (value == null || value == DBNull.Value)
                return Default<T>();
            try
            {
                return (T)value;
            }
            catch { }
            return Default<T>();
        }

        /// <summary>
        /// Get indicate wether the value is null or empty, if yes return the given valueIfNull argument.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="valueIfNull"></param>
        /// <returns></returns>
        public static string NZorEmpty(object value, string valueIfNull)
        {
            try
            {
                return (value == null || value == DBNull.Value || value.ToString() == String.Empty) ? valueIfNull : value.ToString();
            }
            catch
            {
                return valueIfNull;
            }
        }

        /// <summary>
        /// Generic converter , if the given value is null or DBNull.Value return valueIfNull argument.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="valueIfNull"></param>
        /// <returns></returns>
        public static string NZ(object value, string valueIfNull = "")
        {
            return (value == null || value == DBNull.Value) ? valueIfNull : value.ToString();
        }

 
        /// <summary>  
        /// Checks the specified value to see if it can be  
        /// converted into the specified type.  
        /// <remarks>  
        /// The method supports all the primitive types of the CLR  
        /// such as int, boolean, double, guid etc. as well as other  
        /// simple types like Color and Unit and custom enum types.  
        /// </remarks>  
        /// </summary>  
        /// <param name="value">The value to check.</param>  
        /// <param name="type">The type that the value will be checked against.</param>  
        /// <returns>True if the value can convert to the given type, otherwise false. </returns>  
        public static bool CanConvert(string value, Type type)
        {
            if (string.IsNullOrEmpty(value) || type == null) return false;
            TypeConverter conv = ConverterUtil.GetConverter(type);
            if (conv.CanConvertFrom(typeof(string)))
            {
                try
                {
                    conv.ConvertFrom(value);
                    return true;
                }
                catch { }
            }
            return false;
        }

        /// <summary>
        /// Is ? Can ConvertFrom
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool CanConvert<T>(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            var conv = ConverterUtil.GetConverter(typeof(T));
            if (conv.CanConvertFrom(typeof(string)))
            {
                try
                {
                    conv.ConvertFrom(input);
                    return true;
                }
                catch { }
            } return false;
        }

        /// <summary>
        /// Convert array of object to generic list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static List<T> ConvertList<T>(object[] array)
        {
            return array.Cast<T>().ToList<T>();
        }
        /// <summary>
        /// Convert list of object to generic list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<T> ConvertList<T>(List<object> list)
        {
            return list.Cast<T>().ToList<T>();
        }
        /// <summary>
        /// Convert array of object to generic array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static T[] ConvertArray<T>(object[] array)
        {
            return Array.ConvertAll(array, item => (T)item);
        }
        //public static T ConvertItem<T>(object o)
        //{
        //    return new object[] { o }.Cast<T>().FirstOrDefault();
        //}

        /// <summary>
        /// Convert an object of the specified type and whose value is equivalent to the specified object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <param name="enableException"></param>
        /// <returns></returns>
        public static T Cast<T>(object o, bool enableException =false)
        {
            if (o is T)
            {
                return (T)o;
            }
            else
            {
                try
                {
                    return (T)System.Convert.ChangeType(o, typeof(T));
                }
                catch (InvalidCastException cex)
                {
                    if (enableException)
                        throw cex;
                    return default(T);
                }
            }

        }
        public static T Cast<T>(object o, Action<string> onFault)
        {
            if (o is T)
            {
                return (T)o;
            }
            else
            {
                try
                {
                    return (T)System.Convert.ChangeType(o, typeof(T));
                }
                catch (InvalidCastException cex)
                {
                    if (onFault!=null)
                        onFault(cex.Message);
                    return default(T);
                }
            }

        }

        public static bool TryConvert<T>(object obj, out T result)
        {
            result = default(T);
            if (obj is T)
            {
                result = (T)obj;
                return true;
            }

            if (obj != null)
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter.CanConvertFrom(obj.GetType()))
                    result = (T)converter.ConvertFrom(obj);
                else
                    return false;

                return true;
            }

            //Be permissive if the object was null and the target is a ref-type
            return !typeof(T).IsValueType;
        }

        public static T OfType<T>(object o)
        {
            return OfType<T>(new object[] { o }).FirstOrDefault();
        }

        public static IEnumerable<T> OfType<T>(this IEnumerable source)
        {
            foreach (object o in source)
                if (o is T)
                    yield return (T)o;
        }

        public static T ConvertEnum<T>(string value, T defaultValue)
        {
            try
            {
                if (value == null)
                    return defaultValue;
                if (!Enum.IsDefined(typeof(T), value))
                    return defaultValue;
                return (T)Enum.Parse(typeof(T), value, true);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        ///  Converts the string representation of the name or numeric value of one or
        ///  more enumerated constants to an equivalent enumerated object. A string parameter
        ///  is not case-insensitive.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public static T ConvertEnum<T>(string value)
        {
            if (value == null)
                throw new ArgumentNullException("Enum.Parse value");
            if (!Enum.IsDefined(typeof(T), value))
                throw new ArgumentException("Enum not defined");
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static T ReadEnum<T>(int value, T defaultValue)
        {
            if (Enum.IsDefined(typeof(T), value))
                return (T) Cast<T>(value);
            return defaultValue;
        }

     
    }
}
