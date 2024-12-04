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
using System.ComponentModel;
using System.Collections;
using System.Runtime.InteropServices;
using Nistec.Runtime;
using System.Collections.Specialized;
using System.Globalization;
using System.Data;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Nistec.Serialization;
using System.Net.NetworkInformation;

#pragma warning disable  CS1591

namespace Nistec.Generic
{

    public static class StringExtension
    {
        
        public static string[] SplitTrim(this string s, params char[] spliter)
        {
            if (s == null || spliter == null)
            {
                throw new ArgumentNullException();
            }
            string[] array = s.Split(spliter);
            foreach (string a in array)
            {
                a.Trim();
            }
            return array;
        }

        public static string[] SplitTrim(this string s, params string[] spliter)
        {
            if (s == null || spliter == null)
            {
                throw new ArgumentNullException();
            }
            string[] array = s.Split(spliter, StringSplitOptions.None);
            foreach (string a in array)
            {
                a.Trim();
            }
            return array;
        }

        public static string[] SplitTrim(this string s, string[] spliter, char[] trimChars)
        {
            if (s == null || spliter == null)
            {
                throw new ArgumentNullException();
            }
            string[] array = s.Split(spliter, StringSplitOptions.None);
            foreach (string a in array)
            {
                a.Trim(trimChars);
            }
            return array;
        }

        //public static string[] SplitTrim(this string str, string defaultValue, char splitter = ',')
        //{
        //    string[] list = string.IsNullOrEmpty(str) ? new string[] { defaultValue } : str.SplitTrim(splitter);
        //    return list;
        //}

        public static string[] ReTrim(this string[] str, string defaultValue = "")
        {
            if (str == null)
            {
                throw new ArgumentNullException("ReTrim.str");
            }
            for (int i = 0; i < str.Length; i++)
            {
                str[i] = string.IsNullOrEmpty(str[i]) ? defaultValue : str[i].Trim();
            }
            return str;
        }

        public static string JoinTrim(this string[] str, string splitter = ",")
        {
            if (str == null)
            {
                throw new ArgumentNullException("JoinTrim.str");
            }

            return string.Join(splitter, ReTrim(str));
        }
        public static int IndexOf(this string[] array, string stringToFind)
        {
            if (array == null)
            {
                throw new ArgumentNullException("IndexOf.array");
            }

            return Array.FindIndex(array, t => t.Equals(stringToFind, StringComparison.InvariantCultureIgnoreCase));
        }
       
    }

    public static class EnumExtension
    {
        public static bool IsEnum<T>(string value)
        {
            if (value == null)
                return false;
            return Enum.IsDefined(typeof(T), value);
        }

        public static T Cast<T>(int value, T defaultValue)
        {
            if (Enum.IsDefined(typeof(T), value))
            {
              return (T)Enum.ToObject(typeof(T), value);
            }
            else
            {
                return defaultValue;
            }
        }

        public static T ParseOrCast<T>(object value, T defaultValue)
        {
            try
            {
                if (value == null)
                    return defaultValue;
                if (Regx.IsInteger(value.ToString()))
                {
                    if (Enum.IsDefined(typeof(T), value))
                    {
                        return (T)Enum.ToObject(typeof(T), value);
                    }
                    else
                    {
                        return defaultValue;
                    }
                }

                return (T)Enum.Parse(typeof(T), value.ToString(), true);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static object ParseOrCast(Type type,object value, object defaultValue)
        {
            try
            {
                if (value == null)
                    return defaultValue;
                if (Regx.IsInteger(value.ToString()))
                {
                    if (Enum.IsDefined(type, value))
                    {
                        return Enum.ToObject(type, value);
                    }
                    else
                    {
                        return defaultValue;
                    }
                }

                return Enum.Parse(type, value.ToString(), true);
            }
            catch
            {
                return defaultValue;
            }
        }
              

        public static T Parse<T>(string value, T defaultValue)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                    return defaultValue;
                return (T) Enum.Parse(typeof(T), value, true);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static object Parse(Type type, string value, object defaultValue)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                    return defaultValue;
                return Enum.Parse(type, value, true);
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
        public static T Parse<T>(string value)
        {
            if (value == null)
                throw new ArgumentNullException("Enum.Parse value");
            if (!Enum.IsDefined(typeof(T), value))
                throw new ArgumentException("Enum not defined");
            return (T)Enum.Parse(typeof(T), value, true);
        }
        
        public static string GetDescription(Enum value)
        {
           //return Enumerations.GetEnumDescription(value);

            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        /// <summary>
        /// Get enum flags
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="flags"></param>
        /// <param name="DefaultValue"></param>
        /// <returns></returns>
        public static T[] GetEnumFlags<T>(string flags, T DefaultValue)
        {
            if (flags == null)
            {
                throw new ArgumentNullException("EnumExtension.GetEnumValues.flags");
            }
            string[] args = flags.Split('|');
            return GetEnumFlags<T>(args, DefaultValue);
        }

        /// <summary>
        /// GetEnumFlags
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <param name="DefaultValue"></param>
        /// <returns></returns>
        public static T[] GetEnumFlags<T>(string[] args, T DefaultValue)
        {
            if (args == null)
            {
                throw new ArgumentNullException("EnumExtension.GetEnumValues.args");
            }
            List<T> res = new List<T>();
            T sum = DefaultValue;
            foreach (string s in args)
            {
                T v = EnumExtension.Parse<T>(s.Trim(), DefaultValue);
                res.Add(v);
            }
            return res.ToArray();
        }

    }
    /// <summary>
    /// UUID
    /// </summary>
    public static class UUID
    {

        #region UID

        public static long UniqueId()
        {
            byte[] buffer = NewUuid().ToByteArray();
            return BitConverter.ToInt64(buffer, 0);
        }

        public static long UniqueId(this Guid guid)
        {
            byte[] buffer = guid.ToByteArray();
            return BitConverter.ToInt64(buffer, 0);
        }

        public static UInt64 UId()
        {
            byte[] buffer = NewUuid().ToByteArray();
            return BitConverter.ToUInt64(buffer, 0);
        }

        public static UInt64 UId(this Guid guid)
        {
            byte[] buffer = guid.ToByteArray();
            return BitConverter.ToUInt64(buffer, 0);
        }

        public static string UxId()
        {
            byte[] buffer = NewUuid().ToByteArray();
            return BitConverter.ToUInt64(buffer, 0).ToString("x");
        }

        public static string UxId(this Guid guid)
        {
            byte[] buffer = guid.ToByteArray();
            return BitConverter.ToUInt64(buffer, 0).ToString("x");
        }

        /// <summary>
        /// Create new unique from base62
        /// </summary>
        /// <returns></returns>
        public static string NewId()
        {
            return BaseConverter.ToBase62(UniqueId());
        }

        #endregion

        #region big int
               

        public static Guid ToGuid(ulong id)
        {
            byte[] lo = BitConverter.GetBytes(id);
            byte[] hi = BitConverter.GetBytes((ulong)0);

            byte[] bytes = new byte[16];

            Array.Copy(lo, bytes, 8);
            Array.Copy(hi, 0, bytes, 8, 8);

            return new Guid(bytes);
        }

        public static Guid ToGuid(long id)
        {
            byte[] lo = BitConverter.GetBytes(id);
            byte[] hi = BitConverter.GetBytes((long)0);

            byte[] bytes = new byte[16];

            Array.Copy(lo, bytes, 8);
            Array.Copy(hi, 0, bytes, 8, 8);
            return new Guid(bytes);
        }
        #endregion

        static string _MacAddress;
        /// <summary>
        /// MacAddress
        /// </summary>
        /// <returns></returns>
        public static string MacAddress()
        {
            if(_MacAddress==null)
            {
                _MacAddress = (
                    from nic in NetworkInterface.GetAllNetworkInterfaces()
                    where nic.OperationalStatus == OperationalStatus.Up
                    select nic.GetPhysicalAddress().ToString()
                ).FirstOrDefault();
            }
            return _MacAddress;
        }
        /// <summary>
        /// NewUxid
        /// </summary>
        /// <returns></returns>
        public static string NewUxid()
        {
            return NewUsid(MacAddress());
        }
        /// <summary>
        /// NewUuid
        /// </summary>
        /// <returns></returns>
        public static Guid NewUuid()
        {

            Guid guid;
            int result = UuidCreateSequential(out guid);
            if (result == (int)RetUuidCodes.RPC_S_OK)
                return guid;
            else
                return Guid.NewGuid();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string Identifier()
        {
            Guid guid = NewUuid();
            return Strings.StrReverse(BaseConverter.ToBase32String(guid.ToByteArray())).ToLower();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="generatorId"></param>
        /// <returns></returns>
        public static string NewUsid(string generatorId)
        {
            Guid guid= NewUuid();
            return Strings.StrReverse(BaseConverter.ToBase32String(guid.ToByteArray())) + generatorId;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        public static void SetGenerator(byte a, byte b, byte c)
        {
            GENERATORID = string.Format("{0}{1}{2}", a, b, c);
        }
        internal static string GENERATORID = "000";
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string NewUsid()
        {
            return NewUsid(GENERATORID);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GuidSegment()
        {
            return NewUuid().ToString().Split('-')[0];
        }

        #region UUID

        [DllImport("rpcrt4.dll", SetLastError = true)]
        public static extern int UuidCreateSequential(out Guid value);

        [Flags]
        public enum RetUuidCodes : int
        {
            RPC_S_OK = 0, //The call succeeded.
            RPC_S_UUID_LOCAL_ONLY = 1824, //The UUID is guaranteed to be unique to this computer only.
            RPC_S_UUID_NO_ADDRESS = 1739 //Cannot get Ethernet or token-ring hardware address for this computer.
        }

        /// <summary>
        /// This function converts a string generated by the StringFromCLSID function back into the original class identifier.
        /// </summary>
        /// <param name="sz">String that represents the class identifier</param>
        /// <param name="clsid">On return will contain the class identifier</param>
        /// <returns>
        /// Positive or zero if class identifier was obtained successfully
        /// Negative if the call failed
        /// </returns>
        [DllImport("ole32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = true)]
        public static extern int CLSIDFromString(string sz, out Guid clsid);


        #endregion

    }
    /// <summary>
    /// 
    /// </summary>
    public static class DictionaryExtension
    {
 
        #region method GetValue

        public static bool TryGetValue<T>(this Dictionary<string,object> dic, string field, out T value)
        {
            object ovalue = null;
            if (dic.TryGetValue(field, out ovalue))
            {
                try
                {
                    value = (T)GenericTypes.Convert<T>(ovalue);
                    return true;
                }
                catch { }
            }
            value = default(T);
            return false;
        }

        public static bool TryGetValue<K,V,T>(this Dictionary<K, V> dic, K field, out T value)
        {
            V ovalue = default(V);
            if (dic.TryGetValue(field, out ovalue))
            {
                try
                {
                    value = (T)GenericTypes.Convert<T>(ovalue);
                    return true;
                }
                catch { }
            }
            value = default(T);
            return false;
        }

        public static object Get(this IDictionary<string, object> dic, string key)
        {
            object value = null;
            dic.TryGetValue(key, out value);
            return value;
        }
        public static T Get<T>(this IDictionary<string, object> dic, string key)
        {
            object value = null;
            dic.TryGetValue(key, out value);
            return GenericTypes.Convert<T>(value);
        }
        public static T GetEnum<T>(this Dictionary<string, object> dic, string key, T defaultValue)
        {
            object value = null;
            if(dic.TryGetValue(key, out value))
            {
                if (value == null)
                {
                    return defaultValue;
                }
                if (value is string)
                {
                    return EnumExtension.Parse<T>(value.ToString(), defaultValue);
                }
                return (T)value;
            }
            return defaultValue;
        }

        public static T Get<T>(this IDictionary<string, object> dic, string key, T valueIfNull)
        {
            object value = null;
            dic.TryGetValue(key, out value);
            return GenericTypes.Convert<T>(value, valueIfNull);
        }

        public static bool TryGetValue<T>(this IDictionary<string, object> dic, string key, out T value)
        {
            object o = null;
            if (dic.TryGetValue(key, out o))
            {
                value = GenericTypes.Convert<T>(o);
                return value != null;
            }

            value = default(T);
            return false;
        }

        public static T Get<T>(this IDictionary dic, object key)//, ConvertDescriptor cd= ConvertDescriptor.Default)
        {
            if (!dic.Contains(key))
            {
                return GenericTypes.Default<T>();
            }
            return GenericTypes.Convert<T>(dic[key]);
        }

        public static T Get<T>(this IDictionary dic, object key, T valueIfNull)//, ConvertDescriptor cd= ConvertDescriptor.Default)
        {
            if (!dic.Contains(key))
            {
                return valueIfNull;
            }
             return GenericTypes.Convert<T>(dic[key], valueIfNull);
        }


        public static void SetNotExists<T>(this IDictionary dic, object key, T value)
        {
            if (!dic.Contains(key))
            {
                dic[key] = value;
            }
        }


        public static void SetKeyValues(this IDictionary dic, params object[] keyValueParameters)
        {
            int count = keyValueParameters.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            for (int i = 0; i < count; i++)
            {
                string key = KeySet.CleanKey(keyValueParameters[i].ToString());
                dic[key] = keyValueParameters[++i];
            }
        }

        public static Dictionary<string, string> ToDictionary(this string[] keyValueArgs)
        {
            if (keyValueArgs == null)
                return null;

            int count = keyValueArgs.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter is not correct, Not match key value arguments");
            }
            Dictionary<string, string> list = new Dictionary<string, string>();
            for (int i = 0; i < count; i++)
            {
                string key = KeySet.CleanKey(keyValueArgs[i]);
                list[key] = keyValueArgs[++i];
            }

            return list;
        }

         #endregion

        #region KeyValue

 
        public static T Get<T>(this IDictionary dic, object key, object altKey, T defaultValue)
        {
            if (dic.Contains(key))
                return GenericTypes.Convert<T>(dic[key]);
            if (dic.Contains(altKey))
                return GenericTypes.Convert<T>(dic[altKey]);
            return defaultValue;
        }

        public static T Get<T>(this IDictionary dic, string key, T minValue, T maxValue, T defaultValue)
        {
            if (!dic.Contains(key))
                return defaultValue;

            T result = GenericTypes.Convert<T>(dic[key], defaultValue);
            if (Comparer<T>.Default.Compare(result, minValue) < 0 || Comparer<T>.Default.Compare(result, maxValue) > 0)
                return defaultValue;
            return result;
        }

        public static T GetEnum<T>(this IDictionary dic, string key, T defaultValue)
        {
            if (dic.Contains(key))
            {
                string val = (string)dic[key];
                if (Types.IsNumeric(val))
                    return GenericTypes.ImplicitConvert<T>(Types.ToInt(dic[key], GenericTypes.ImplicitConvert<int>(defaultValue)));
                return GenericTypes.ConvertEnum<T>((string)dic[key], defaultValue);
            }
            return defaultValue;
        }

        public static string Get(this Dictionary<string, string> dic, string key, string defaultValue = null)
        {
            if (dic.ContainsKey(key))
                return dic[key];
            return defaultValue;
        }

        public static string Get(this Dictionary<string, string> dic, string key, string defaultValue, int trimLength = -1)
        {
            if (dic.ContainsKey(key))
                return trimLength >= 0 ? Strings.Trim(dic[key], trimLength) : dic[key];
            return defaultValue;
        }

        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="de"></param>
        /// <returns></returns>
        public static T Cast<T>(this IDictionary<string, object> de)
        {
            T entity = ActivatorUtil.CreateInstance<T>();
            PropertyInfo[] p = entity.GetType().GetProperties(true);

            if (entity == null)
            {
                return default(T);
            }
            if (p == null)
            {
                return default(T);
            }
            else
            {
                foreach (var entry in de)
                {
                    PropertyInfo pi = p.Where(pr => pr.Name == entry.Key).FirstOrDefault();
                    if (pi != null && pi.CanWrite)
                    {
                        pi.SetValue(entity, entry.Value, null);
                    }
                }

                return entity;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="original"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> CloneDictionary<TKey, TValue> (this Dictionary<TKey, TValue> original) where TValue : ICloneable
        {
            Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(original.Count, original.Comparer);
            foreach (KeyValuePair<TKey, TValue> entry in original)
            {
                ret.Add(entry.Key, (TValue)entry.Value.Clone());
            }
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="qs"></param>
        /// <param name="enableNull"></param>
        /// <returns></returns>
        public static Dictionary<string, T> ParseQueryString<T>(string qs, bool enableNull)
        {
            Dictionary<string, T> dictionary = new Dictionary<string, T>();

            if (qs == null)
                qs = string.Empty;

            string str = qs.Replace("&amp;", "&"); 

            if (string.IsNullOrEmpty(str))
            {
                return dictionary;
            }
            if (!str.Contains('='))
            {
                return dictionary;
            }

            foreach (string arg in str.Split(new char[] { '&' }))
            {
                if (!string.IsNullOrEmpty(arg))
                {
                    string[] strArray = arg.Split(new char[] { '=' });
                    if (strArray.Length == 2)
                    {
                        string key = Regx.RegexReplace("amp;", strArray[0], "");
                        object val = strArray[1];
                        if (val != null || (enableNull && val == null))
                        {
                            dictionary[key] = (T)GenericTypes.Cast<T>(val);
                        }
                    }
                    //else
                    //{
                    //    dictionary[arg] = null;
                    //}
                }
            }

            return dictionary;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public static class KeyValueExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nv"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Get(this NameValueArgs nv, string key)
        {
            return nv.Get(key);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nv"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Get<T>(this NameValueArgs<T> nv, string key)
        {
            return nv.Get<T>(key);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValueArray"></param>
        /// <param name="keyToFind"></param>
        /// <param name="matchTo"></param>
        /// <returns></returns>
        public static bool IsMatch(this object[] keyValueArray, string keyToFind, object matchTo)
        {
            if (keyValueArray == null || keyToFind == null || matchTo == null)
                return false;
            int i = Array.FindIndex(keyValueArray, item => item.ToString().ToLower() == keyToFind.ToLower());
            if (i >= 0 && i < keyValueArray.Length && i % 2 == 0)
            {
                return keyValueArray[i + 1] == matchTo;
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValueArray"></param>
        /// <param name="keyToFind"></param>
        /// <param name="matchTo"></param>
        /// <returns></returns>
        public static bool IsMatch(this object[] keyValueArray, string keyToFind, string matchTo)
        {
            if (keyValueArray == null || keyToFind == null || matchTo == null)
                return false;
            int i = Array.FindIndex(keyValueArray, item => item.ToString().ToLower() == keyToFind.ToLower());
            if (i >= 0 && i < keyValueArray.Length && i % 2 == 0)
            {
                if (keyValueArray[i + 1] == null)
                    return false;
                return keyValueArray[i + 1].ToString().ToLower() == matchTo.ToLower();
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyValueArray"></param>
        /// <param name="keyToFind"></param>
        /// <param name="matchTo"></param>
        /// <returns></returns>
        public static bool IsMatch<T>(this object[] keyValueArray, string keyToFind, T matchTo)
        {
            if (keyValueArray == null || keyToFind == null || matchTo == null)
                return false;
            int i = Array.FindIndex(keyValueArray, item => item.ToString().ToLower() == keyToFind.ToLower());
            if (i >= 0 && i < keyValueArray.Length && i % 2 == 0)
            {
                return matchTo.Equals(GenericTypes.Convert<T>(keyValueArray[i + 1]));
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValueArray"></param>
        /// <param name="key"></param>
        /// <param name="defaulValue"></param>
        /// <returns></returns>
        public static object FindValue(this object[] keyValueArray, string key, object defaulValue=null)
        {
            if (keyValueArray == null || key == null)
                return defaulValue;
            int i = Array.FindIndex(keyValueArray, item => item.ToString().ToLower() == key.ToLower());
            if (i >= 0 && i < keyValueArray.Length && i % 2==0)
                return keyValueArray[i + 1];
            return defaulValue;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyValueArray"></param>
        /// <param name="key"></param>
        /// <param name="defaulValue"></param>
        /// <returns></returns>
        public static T FindValue<T>(this object[] keyValueArray, string key, T defaulValue = default(T))
        {
            if (keyValueArray == null || key == null)
                return defaulValue;
            int i = Array.FindIndex(keyValueArray, item => item.ToString().ToLower() == key.ToLower());
            if (i >= 0 && i < keyValueArray.Length && i % 2 == 0)
                return GenericTypes.Convert<T>(keyValueArray[i + 1]);
            return defaulValue;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValueArray"></param>
        /// <param name="key"></param>
        /// <param name="defaulValue"></param>
        /// <returns></returns>
        public static string FindValue(this string[] keyValueArray, string key, string defaulValue = null)
        {
            if (keyValueArray == null || key == null)
                return defaulValue;
            int i = Array.FindIndex(keyValueArray, item => item.ToLower() == key.ToLower());
            if (i >= 0 && i < keyValueArray.Length && i % 2 == 0)
                return keyValueArray[i + 1];
            return defaulValue;
        }

        //public static string ToJson(params object[] keyValueArray)
        //{
        //    int count = keyValueArray.Length;
        //    if (count % 2 != 0)
        //    {
        //        throw new ArgumentException("values parameter not correct, Not match key value arguments");
        //    }
        //    StringBuilder _output = new StringBuilder();
        //    _output.Append("{");
        //    for (int i = 0; i < count; i++)
        //    {

        //        string key = string.Format("{0}", keyValueArray[i]);
        //        _output.AppendFormat("{0}:", keyValueArray[i]);

        //        object obj = keyValueArray[++i];

        //        if (obj == null || obj is DBNull)
        //        {
        //            _output.Append("null");
        //        }
        //        else
        //        {
        //            Type type = obj.GetType();

        //            switch (type.Name)
        //            {
        //                case "Boolean":
        //                    _output.Append(((bool)obj) ? "true" : "false"); break;// conform to standard
        //                case "Byte":
        //                case "UInt16":
        //                case "UInt32":
        //                case "UInt64":
        //                case "SByte":
        //                case "Int16":
        //                case "Int32":
        //                case "Int64":
        //                case "Single":
        //                case "Double":
        //                case "Decimal":
        //                    _output.Append(((IConvertible)obj).ToString(NumberFormatInfo.InvariantInfo)); break;
        //                case "Char":
        //                case "String":
        //                    _output.Append(obj.ToString()); break;
        //                case "DateTime":
        //                    _output.Append((DateTime)obj); break;
        //                case "TimeSpan":
        //                    _output.Append((TimeSpan)obj); break;
        //                case "Guid":
        //                    _output.Append((Guid)obj); break;

        //                //case "Byte[]":
        //                //    WriteBytes((byte[])obj); break;
        //                //case "Char[]":
        //                //    WriteArray<Char>((Char[])obj); break;
        //                //case "Int16[]":
        //                //    WriteArray<Int16>((Int16[])obj); break;
        //                //case "Int32[]":
        //                //    WriteArray<Int32>((Int32[])obj); break;
        //                //case "Int64[]":
        //                //    WriteArray<Int64>((Int64[])obj); break;
        //                //case "String[]":
        //                //    WriteArray<String>((String[])obj); break;
        //                //case "Object[]":
        //                //    WriteArray<Object>((Object[])obj); break;
        //                default:
        //                    _output.Append(obj.ToString()); break;
        //            }
        //        }
        //        _output.Append(",");

        //    }
        //    _output.Remove(_output.Length - 1, 1);// remove las "," 
        //    _output.Append("}");
        //    return _output.ToString();
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="dr"></param>
        public static void ToKeyValue<T>(this IKeyValue<T> instance, DataRow dr)
        {
            if (dr == null)
                return;
            DataTable dt = dr.Table;
            if (dt == null)
                return;
            instance.Clear();
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                string colName = dt.Columns[i].ColumnName;
                instance[colName] = GenericTypes.Convert<T>(dr[colName]);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="dr"></param>
        public static void ToNameValue(this INameValue instance, DataRow dr)
        {
            if (dr == null)
                return;
            DataTable dt = dr.Table;
            if (dt == null)
                return;
            instance.Clear();
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                string colName = dt.Columns[i].ColumnName;
                instance[colName] = GenericTypes.NZ(dr[colName], "");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="dr"></param>
        public static void ToNameValue<T>(this NameValueArgs<T> instance, DataRow dr)
        {
            if (dr == null)
                return;
            DataTable dt = dr.Table;
            if (dt == null)
                return;
            instance.Clear();
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                string colName = dt.Columns[i].ColumnName;
                instance[colName] = GenericTypes.Convert<T>(dr[colName], default(T));
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="dt"></param>
        /// <param name="colKey"></param>
        /// <param name="colValue"></param>
        public static void ToNameValue(this INameValue instance, DataTable dt, string colKey=null, string colValue = null)
        {
            if (dt == null)
                return;
            if (dt.Columns.Count < 2)
                return;

            if (colKey == null)
                colKey = dt.Columns[0].ColumnName;
            if (colValue == null)
                colValue = dt.Columns[1].ColumnName;


            instance.Clear();

            foreach (DataRow dr in dt.Rows)
            {
                string key = Types.NZ(dr[colKey], null);
                if(key!=null)
                instance[key] = GenericTypes.NZ(dr[colValue], "");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="colKey"></param>
        /// <param name="colValue"></param>
        /// <returns></returns>
        public static INameValue ToNameValue(this DataTable dt, string colKey = null, string colValue = null)
        {
            if (dt == null)
                return null;
            if (dt.Columns.Count < 2)
                return null;

            if (colKey == null)
                colKey = dt.Columns[0].ColumnName;
            if (colValue == null)
                colValue = dt.Columns[1].ColumnName;

            NameValueArgs instance = new NameValueArgs();
            
            foreach (DataRow dr in dt.Rows)
            {
                string key = Types.NZ(dr[colKey], null);
                if (key != null)
                    instance[key] = GenericTypes.NZ(dr[colValue], "");
            }
            return instance;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="kv"></param>
        /// <param name="key"></param>
        /// <param name="valueIfNull"></param>
        /// <returns></returns>
        public static string[] SplitArg(this IKeyValue kv, string key, string valueIfNull)
        {
            string val = kv.Get<string>(key, valueIfNull);
            if (val == null)
                return valueIfNull == null ? null : new string[] { valueIfNull };
            return val.SplitTrim('|');
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kv"></param>
        /// <param name="key"></param>
        /// <param name="valueIfNull"></param>
        /// <returns></returns>
        public static TimeSpan TimeArg(this IKeyValue kv, string key, string valueIfNull)
        {
            string val = kv.Get<string>(key, valueIfNull);
            TimeSpan time = string.IsNullOrEmpty(val) ? TimeSpan.Zero : TimeSpan.Parse(val);
            return time;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="valueIfNull"></param>
        /// <returns></returns>
        public static string[] SplitArg(this NameValueArgs dic, string key, string valueIfNull)
        {
            string val = dic.Get<string>(key, valueIfNull);
            if (val == null)
                return valueIfNull == null ? null : new string[] { valueIfNull };
            return val.SplitTrim('|');
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="valueIfNull"></param>
        /// <returns></returns>
        public static TimeSpan TimeArg(this NameValueArgs dic, string key, string valueIfNull)
        {
            string val = dic.Get<string>(key, valueIfNull);
            TimeSpan time = string.IsNullOrEmpty(val) ? TimeSpan.Zero : TimeSpan.Parse(val);
            return time;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public static class CollectionExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string ToQueryString(this NameValueCollection args)
        {
            StringBuilder sb = new StringBuilder();

            foreach (string key in args)
            {
                var value = args[key];
                sb.AppendFormat("{0}={1}&", key, value);
            }
            return sb.ToString().TrimEnd('&');
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object[] ToNameValue(this NameValueCollection args)
        {
            List<object> o = new List<object>();

            foreach (string key in args)
            {
                o.Add(key);
                o.Add(args[key]);
            }
            return o.ToArray();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nv"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Get<T>(this NameValueCollection nv, string key)
        {
            return GenericTypes.Convert<T>(nv[key]);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nv"></param>
        /// <param name="key"></param>
        /// <param name="valueIfNull"></param>
        /// <returns></returns>
        public static T Get<T>(this NameValueCollection nv, string key, T valueIfNull)
        {
            return GenericTypes.Convert<T>(nv[key], valueIfNull);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nv"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T GetEnum<T>(this NameValueCollection nv, string key, T defaultValue)
        {
            string value = nv[key];
            if (value == null)
            {
                return defaultValue;
            }
            int val;
            if(int.TryParse(value, out val))
            {
                return (T)EnumExtension.Cast<T>(val,defaultValue);
            }
            return EnumExtension.Parse<T>(value.ToString(), defaultValue);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nv"></param>
        /// <param name="key"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T Get<T>(this NameValueCollection nv, string key, T minValue, T maxValue, T defaultValue)
        {
            T result = GenericTypes.Convert<T>(nv[key], defaultValue);
            if(Comparer<T>.Default.Compare(result,minValue)<0 || Comparer<T>.Default.Compare(result,maxValue)>0)
                return defaultValue;
            return result;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class KeyValueUtil
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public static string KeyValueToQueryString(params string[] keyValueParameters)
        {
            StringBuilder sb = new StringBuilder();

            if (keyValueParameters == null || keyValueParameters.Length == 0)
                return null;

            int count = keyValueParameters.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            for (int i = 0; i < count; i++)
            {
                sb.AppendFormat("{0}={1}&", keyValueParameters[i], keyValueParameters[++i]);
            }
            return sb.ToString().TrimEnd('&');
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public static NameValueCollection KeyValueToNameValue(params string[] keyValueParameters)
        {
            NameValueCollection dic = new NameValueCollection();

            if (keyValueParameters == null || keyValueParameters.Length == 0)
                return dic;

            int count = keyValueParameters.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            for (int i = 0; i < count; i++)
            {
                dic.Add(keyValueParameters[i], keyValueParameters[++i]);
            }

            return dic;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="splitterList"></param>
        /// <param name="spliterKeyValue"></param>
        /// <returns></returns>
        public static NameValueCollection ParseCommaString(string s, char splitterList='|', char spliterKeyValue='=')
        {
            NameValueCollection dic = new NameValueCollection();
            if (string.IsNullOrEmpty(s))
                return dic;

            string[] args = s.SplitTrim(splitterList);
            foreach (string arg in args)
            {
                string[] kv = arg.SplitTrim(spliterKeyValue);
                if (kv.Length == 2)
                    dic.Add(kv[0].Trim(), kv[1].Trim());
            }

            return dic;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static NameValueCollection ParseCommaPipe(string s)
        {
            NameValueCollection dic = new NameValueCollection();
            if (string.IsNullOrEmpty(s))
                return dic;

            string[] args = s.SplitTrim('|');
            foreach (string arg in args)
            {
                string[] kv = arg.SplitTrim('=');
                    if (kv.Length == 2)
                    dic.Add(kv[0].Trim(), kv[1].Trim());
            }

            return dic;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public static IDictionary<string,object> KeyValueToDictionary(params object[] keyValueParameters)
        {
            IDictionary<string, object> dic = new Dictionary<string, object>();

            if (keyValueParameters == null || keyValueParameters.Length == 0)
                return dic;

            int count = keyValueParameters.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            for (int i = 0; i < count; i++)
            {
                dic.Add(keyValueParameters[i].ToString(), keyValueParameters[++i]);
            }

            return dic;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValueParameters"></param>
        /// <returns></returns>
        public static IDictionary<string, string> KeyValueToDictionaryString(params string[] keyValueParameters)
        {
            IDictionary<string, string> dic = new Dictionary<string, string>();

            if (keyValueParameters == null || keyValueParameters.Length == 0)
                return dic;

            int count = keyValueParameters.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            for (int i = 0; i < count; i++)
            {
                dic.Add(keyValueParameters[i] , keyValueParameters[++i]);
            }

            return dic;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class DictionaryUtil
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="initialCapacity"></param>
        /// <returns></returns>
        public static ConcurrentDictionary<TKey, TValue> CreateConcurrentDictionary<TKey, TValue>(int initialCapacity = 101)
        {
            // We know how many items we want to insert into the ConcurrentDictionary.
            // So set the initial capacity to some prime number above that, to ensure that
            // the ConcurrentDictionary does not need to be resized while initializing it.
            //int initialCapacity = 101;

            // The higher the concurrencyLevel, the higher the theoretical number of operations
            // that could be performed concurrently on the ConcurrentDictionary.  However, global
            // operations like resizing the dictionary take longer as the concurrencyLevel rises. 
            // For the purposes of this example, we'll compromise at numCores * 2.
            int numProcs = Environment.ProcessorCount;
            int concurrencyLevel = numProcs * 2;

            // Construct the dictionary with the desired concurrencyLevel and initialCapacity
            return new ConcurrentDictionary<TKey, TValue>(concurrencyLevel, initialCapacity);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="name"></param>
        /// <param name="allowReadOnly"></param>
        /// <returns></returns>
        public static object ToDictionaryOrObject(object source, string name, bool allowReadOnly = false)
        {
            if (source == null)
            {
                return null;
            }
            Type sourceType = source.GetType();

            if (sourceType.IsEnum)
            {
                return (int)Convert.ToInt32(source);
            }
            else if (SerializeTools.IsSimple(sourceType))
            {
                return source;
            }
            else if (SerializeTools.IsStream(sourceType))
            {
                return source;
            }
            else if (SerializeTools.IsType(sourceType))
            {
                return sourceType;
            }

            var dictionary = new Dictionary<string, object>();
            SerializeTools.MapToDictionary(dictionary, source, name);
            return dictionary;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="name"></param>
        /// <param name="allowReadOnly"></param>
        /// <returns></returns>
        public static IDictionary<string, object> ToDictionary(object source, string name, bool allowReadOnly = false)
        {
            if (source == null)
            {
                return null;
            }
            var dictionary = new Dictionary<string, object>();
            SerializeTools.MapToDictionary(dictionary, source, name);
            return dictionary;
        }

        //internal static void MapToDictionaryInternal(
        //    Dictionary<string, object> dictionary, object source, string name, bool allowReadOnly = false)
        //{
        //    Type sourceType = source.GetType();
        //    if (sourceType.IsEnum)
        //    {
        //        dictionary[sourceType.Name] = (int)Convert.ToInt32(source);
        //        return;
        //    }
        //    else if (SerializeTools.IsSimple(sourceType))
        //    {
        //        dictionary[sourceType.Name] = source;
        //        return;
        //    }
        //    else if (SerializeTools.IsStream(sourceType))
        //    {
        //        dictionary[sourceType.Name] = source;
        //        return;
        //    }
        //    else if (SerializeTools.IsType(sourceType))
        //    {
        //        dictionary[sourceType.Name] = sourceType.FullName;
        //        return;
        //    }

        //    var properties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance, allowReadOnly);
        //    foreach (var p in properties)
        //    {

        //        if(!allowReadOnly && !p.CanWrite) { continue; }

        //        if (p.CanRead)
        //        {
        //            var key = !string.IsNullOrWhiteSpace(name) ? name + "." + p.Name : p.Name;
        //            object value = p.GetValue(source, null);
        //            if (value == null)
        //                continue;
        //            Type valueType = value.GetType();

        //            if (valueType.IsEnum)
        //            {
        //                dictionary[key] =(int) Convert.ToInt32(value);
        //            }
        //            else if (SerializeTools.IsSimple(valueType))
        //            {
        //                dictionary[key] = value;
        //            }
        //            else if (SerializeTools.IsStream(valueType))
        //            {
        //                dictionary[key] = value;
        //            }
        //            else if (SerializeTools.IsType(valueType))
        //            {
        //                dictionary[key] = valueType.FullName;
        //            }
        //            else if (SerializeTools.IsDataTable(valueType) || SerializeTools.IsDataSet(valueType))
        //            {
        //                dictionary[key] = value;
        //            }
        //            else if (typeof(IKeyValue).IsAssignableFrom(valueType))
        //            {
        //                dictionary[key] = ((IKeyValue)value).Dictionary();
        //            }
        //            else if (typeof(IKeyValue<object>).IsAssignableFrom(valueType))
        //            {
        //                dictionary[key] = ((IKeyValue<object>)value).ToDictionary();
        //            }
        //            else if (SerializeTools.IsAssignableFromDictionary(valueType, true))
        //            {
        //                dictionary[key] = value;
        //            }
        //            else if (SerializeTools.IsEnumerable(valueType))
        //            {
        //                var i = 0;
        //                var subdictionary = new Dictionary<string, object>();
        //                foreach (object o in (IEnumerable)value)
        //                {
        //                    MapToDictionaryInternal(subdictionary, o, key + "[" + i + "]");
        //                    i++;
        //                }
        //                dictionary[key]= subdictionary;
        //            }
        //            else
        //            {
        //                var subdictionary = new Dictionary<string, object>();
        //                MapToDictionaryInternal(subdictionary, value, key, allowReadOnly);
        //                dictionary[key] = subdictionary;
        //            }
        //        }
        //    }
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="source"></param>
        /// <param name="itemsToSync"></param>
        /// <param name="removeNonExists"></param>
        /// <param name="logAction"></param>
        public void Synchronize<TKey, TValue>(IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> itemsToSync, bool removeNonExists, Action<string> logAction)
        {
            object osync = new Object();

            List<TKey> itemsToRemove = new List<TKey>();

            //stor all the keys that should be removed
            if (removeNonExists)
            {
                foreach(var entry in source)
                {
                    var searchKey = entry.Key;
                    if (!itemsToSync.ContainsKey(searchKey))
                    {
                        itemsToRemove.Add(searchKey);
                    }
                };
            }

            //synchronize data with the new 
            foreach (var entry in itemsToSync)
            {
                var searchKey = entry.Key;
                TValue retrievedValue;
                TValue newValue = entry.Value;
                retrievedValue = source[searchKey] = newValue;
            };

            //remove all items from data that not exists in the new data
            if (removeNonExists && itemsToRemove.Count > 0)
            {
                foreach (var searchKey in itemsToRemove)
                {
                    if (!source.Remove(searchKey))
                    {
                        //The data was not updated. Log error, throw exception, etc.
                        if (logAction != null)
                            logAction(string.Format("Synchronize.TryRemove Item failed : {0}", searchKey));
                        //CacheLogger.Logger.LogAction(CacheAction.SyncItem, CacheActionState.Failed, "Synchronize.TryRemove Item failed : {0}", searchKey);
                    }
                };
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="source"></param>
        /// <param name="itemsToSync"></param>
        /// <param name="removeNonExists"></param>
        /// <param name="logAction"></param>
        public void SynchronizeParallel<TKey, TValue>(Dictionary<TKey, TValue> source, Dictionary<TKey, TValue> itemsToSync, bool removeNonExists, Action<string> logAction)
        {

            ConcurrentBag<TKey> itemsToRemove = new ConcurrentBag<TKey>();

            //stor all the keys that should be removed
            if (removeNonExists)
            {
                Parallel.ForEach(source, entry =>
                {
                    var searchKey = entry.Key;
                    if (!itemsToSync.ContainsKey(searchKey))
                    {
                        itemsToRemove.Add(searchKey);
                    }
                });
            }

            //synchronize data with the new 
            Parallel.ForEach(itemsToSync, entry =>
            {
                var searchKey = entry.Key;
                TValue retrievedValue;
                TValue newValue = entry.Value;
                retrievedValue = source[searchKey]=newValue;

            });

            //remove all items from data that not exists in the new data
            if (removeNonExists && itemsToRemove.Count > 0)
            {
                Parallel.ForEach(itemsToRemove, searchKey =>
                {
                    TValue retrievedValue;
                    if(source.TryGetValue(searchKey, out retrievedValue))
                    {
                        source.Remove(searchKey);
                        if (logAction != null)
                            logAction(string.Format("Synchronize.TryRemove Item failed : {0}", searchKey));

                    }
                });
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="source"></param>
        /// <param name="itemsToSync"></param>
        /// <param name="removeNonExists"></param>
        /// <param name="logAction"></param>
        public void SynchronizeParallel<TKey, TValue>(ConcurrentDictionary<TKey, TValue> source, ConcurrentDictionary<TKey, TValue> itemsToSync, bool removeNonExists, Action<string> logAction)
        {

            ConcurrentBag<TKey> itemsToRemove = new ConcurrentBag<TKey>();

            //stor all the keys that should be removed
            if (removeNonExists)
            {
                Parallel.ForEach(source, entry =>
                {
                    var searchKey = entry.Key;
                    if (!itemsToSync.ContainsKey(searchKey))
                    {
                        itemsToRemove.Add(searchKey);
                    }
                });
            }

            //synchronize data with the new 
            Parallel.ForEach(itemsToSync, entry =>
            {
                var searchKey = entry.Key;
                TValue retrievedValue;
                TValue newValue = entry.Value;
                retrievedValue = source.AddOrUpdate(searchKey, newValue, (key, oldValue) => newValue);

            });

            //remove all items from data that not exists in the new data
            if (removeNonExists && itemsToRemove.Count > 0)
            {
                Parallel.ForEach(itemsToRemove, searchKey =>
                {
                    TValue retrievedValue;
                    if (!source.TryRemove(searchKey, out retrievedValue))
                    {
                        //The data was not updated. Log error, throw exception, etc.
                        if (logAction != null)
                            logAction(string.Format("Synchronize.TryRemove Item failed : {0}", searchKey));
                        //CacheLogger.Logger.LogAction(CacheAction.SyncItem, CacheActionState.Failed, "Synchronize.TryRemove Item failed : {0}", searchKey);
                    }
                });
            }

            //synchronize data with the new
            //foreach (var entry in items)
            //{
            //    string searchKey = entry.Key;
            //    EntityStream retrievedValue;
            //    EntityStream newValue = entry.Value;

            //    if (_Items.TryGetValue(searchKey, out retrievedValue))
            //    {
            //        if (!_Items.TryUpdate(searchKey, retrievedValue, newValue))
            //        {
            //            //The data was not updated. Log error, throw exception, etc.
            //            CacheLogger.Logger.LogAction(CacheAction.SyncItem, CacheActionState.Failed, "Synchronize.TryUpdate Item failed : {0}", searchKey);
            //        }
            //    }
            //    else
            //    {
            //        if (!_Items.TryAdd(searchKey, newValue))
            //        {
            //            CacheLogger.Logger.LogAction(CacheAction.SyncItem, CacheActionState.Failed, "Synchronize.TryAdd Unable to add data for : {0}", searchKey);
            //        }
            //    }
            //}

            //if (removeNonExists)
            //{
            //    //remove all items from data that not exists in the new data
            //    foreach (var entry in _Items)
            //    {
            //        string searchKey = entry.Key;
            //        EntityStream retrievedValue;
            //        if (!items.TryGetValue(searchKey, out retrievedValue))
            //        {
            //            if (!_Items.TryRemove(searchKey, out retrievedValue))
            //            {
            //                //The data was not updated. Log error, throw exception, etc.
            //                CacheLogger.Logger.LogAction(CacheAction.SyncItem, CacheActionState.Failed, "Synchronize.TryRemove Item failed : {0}", searchKey);
            //            }
            //        }
            //    }
            //}
        }
    }
}
