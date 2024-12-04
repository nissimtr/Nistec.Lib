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
using System.Data;
using System.Reflection;
using System.Collections;

#pragma warning disable  CS1591

namespace Nistec.Generic
{

    /// <summary>
    /// Represent KeySet collection
    /// </summary>
    [Serializable]
    public class KeySet : Dictionary<string, object>
    {
        public const string Separator = "|";
        public const char SeparatorCh = '|';
        public static readonly string[] SeparatorArray = new string[] { Separator };
        public static readonly char[] TrimChars = new char[] { '[', ']' };

        public static string ReadArgs(string[] args, int index)
        {
            if (args == null || args.Length <= index)
                return null;
            return args[index];
        }
        public static string[] SplitTrim(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            string[] array = s.Split(SeparatorArray, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = array[i].Trim(TrimChars);
            }
            return array;
        }
        public static string[] SplitKeysTrim(string s)
        {
            if (s == null)
            {
                return null;//  throw new ArgumentNullException("s");
            }
            string[] array = s.Split(new string[] { Separator, ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
            for(int i=0;i< array.Length;i++)
            {
                array[i]= array[i].Trim(TrimChars);
            }
            return array;
        }
        public static string Join(string[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            string value = string.Join(Separator, array);
            return value;//.Replace(TrimChars[0].ToString(), "").Replace(TrimChars[1].ToString(), "");
        }
        public static string JoinTrim(string[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            StringBuilder sb = new StringBuilder();
            foreach (string a in array)
            {
                sb.Append(a.Trim(TrimChars) + KeySet.Separator);
            }
            string value = sb.ToString().TrimEnd(KeySet.SeparatorCh);
            return value;
        }

        #region ctor
        public KeySet()
        {

        }
        public KeySet(IDictionary<string, object> keyset)
        {
            foreach (var entry in keyset)
                this.Add(entry.Key, entry.Value);
        }

        public KeySet(IDictionary keyset)
        {
            foreach (DictionaryEntry entry in keyset)
                this.Add(entry.Key.ToString(), entry.Value);
        }

        //public KeySet(params string[] keys)
        //{
        //    this.AddRange(keys);
        //}

        //public KeySet(DataColumn[] columnKeys)
        //{
        //    string[] keys = (from property in columnKeys
        //                     select property.ColumnName).ToArray();
        //    this.AddRange(keys);
        //}
        #endregion


        #region static BuildKeys

        public static KeySet BuildKeys(IDictionary dic, string[] fields)
        {
            KeySet k = new KeySet();// fields);
            for (int i = 0; i < fields.Length; i++)
            {
                string key = KeySet.CleanKey(fields[i]);
                k[key] = dic[key];
            }
            return k;
        }

        public static KeySet BuildKeys(IDictionary dic, DataColumn[] fields)
        {
            KeySet k = new KeySet();// fields);
            for (int i = 0; i < fields.Length; i++)
            {
                string key = KeySet.CleanKey(fields[i].ColumnName);
                k[key] = dic[key];
            }
            return k;
        }

        public static KeySet Get(params object[] keyValue)
        {
            KeySet k = new KeySet();
            k.SetKeyValues(keyValue);
            return k;
        }

        public static KeySet BuildKeys(object instance, IEnumerable<string> fields, bool sortedByFields)
        {
            KeySet k = new KeySet();
            var keyValue = AttributeProvider.SelectPropertiesKeyValues(instance, fields, sortedByFields);
            k.SetKeyValues(keyValue);
            return k;
        }

        public static string CleanKey(string s)
        {
            if (s == null)
                return null;
            return s.Replace("[", "").Replace("]", "");
        }
        public static string[] CleanKeys(string[] array)
        {
            if (array == null)
                return null;
            for (int i = 0; i < array.Length; i++)
                array[i] = CleanKey(array[i]);
            return array;
        }
        #endregion


        #region methods

        public void AddKeys(string[] keys)
        {
            foreach (string s in keys)
            {
                this[s] = KeySet.CleanKey(s);
            }
        }
        [Serialization.NoSerialize]
        public string this[int index]
        {
            get { return this.Keys.ElementAt(index); }
        }

        ///// <summary>
        ///// Set DataTable primary key 
        ///// </summary>
        ///// <param name="dt"></param>
        //public void SetPrimaryKeys(DataTable dt)
        //{
        //    if (Count == 0)
        //    {
        //        return;
        //    }
        //    List<DataColumn> columns = new List<DataColumn>();
        //    for (int i = 0; i < this.Count; i++)
        //    {
        //        columns.Add(dt.Columns[this[i]]);
        //    }
        //    dt.PrimaryKey = columns.ToArray();
        //}

        //public string[] ToArray()
        //{
        //    return this.Keys.ToArray();
        //}

        
        /// <summary>
        /// Get Keys as string 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Count == 0)
            {
                return "";
            }
            return FormatPrimaryKey();
            //return FormatPrimaryKey(this.ToArray());
        }

        //public string ToString(string separator, bool sorted)
        //{
        //    if (Count == 0)
        //    {
        //        return "";
        //    }
        //    return FormatPrimaryKey(separator, sorted);
        //    //return string.Join(separator, this);
        //}

        //public string ToString(bool sorted)
        //{
        //    if (Count == 0)
        //    {
        //        return "";
        //    }
        //    return FormatPrimaryKey(KeySet.Separator, sorted);
        //}

        //public string CreatePrimaryKey(object instance, bool sorted = false)
        //{
        //    IEnumerable<object> values = AttributeProvider.GetPropertiesValues(instance, this.Keys.ToArray(), sorted);
        //    return FormatPrimaryKey(values.ToArray());
        //}

        //public string CreatePrimaryKey(IDictionary<string, object> record, bool sorted = false)
        //{
        //    IEnumerable<object> values = (sorted) ?
        //        from p in record.Where(p => this.Keys.Contains(p.Key)).OrderBy(p => p.Key)
        //        select p.Value
        //        :
        //        from p in record.Where(p => this.Keys.Contains(p.Key))
        //        select p.Value;
        //    return FormatPrimaryKey(values.ToArray());
        //}

        public string FormatPrimaryKey(bool sorted = false)
        {

            if (sorted)
                return KeySet.FormatPrimaryKey(KeySet.Separator, (from p in this.OrderBy(p => p.Key) select p.Value).ToArray());
            else
                return KeySet.FormatPrimaryKey(KeySet.Separator, this.Values.ToArray());

            //IEnumerable<object> values = (sorted) ?
            //from p in this.OrderBy(p => p.Key)
            //select p.Value
            //:
            //from p in this
            //select p.Value;

            //return KeySet.FormatPrimaryKey(KeySet.Separator, values.ToArray());
        }

        //public string FormatPrimaryKey(string separator, bool sorted = false)
        //{
        //    IEnumerable<object> values = (sorted) ?
        //        from p in this.OrderBy(p => p.Key)
        //        select p.Value
        //        :
        //        from p in this
        //        select p.Value;
        //    return FormatPrimaryKey(separator,values.ToArray());
        //}

        //public string CreatePrimaryKey(bool sorted = false)
        //{
        //    return CreatePrimaryKey(this, sorted);
        //}
        #endregion

        #region KeyFields

        //public string GetPrimary(string fields)
        //{
            
        //    if (string.IsNullOrWhiteSpace(fields))
        //    {
        //        return KeySet.FormatPrimaryKey(this.Values.ToArray());
        //    }
        //    else
        //    {
        //        List<object> values = new List<object>();

        //        string[] args = fields.SplitTrim(',');
        //        for (int i = 0; i < args.Length; i++)
        //        {
        //            values.Add(this[args[i]]);
        //        }
        //        return KeySet.FormatPrimaryKey(values.ToArray());
        //    }
        //}

        public bool HasValues()
        {
            return this.All(p => p.Value != null);
        }

        public bool IsMatch(IDictionary<string, object> record)
        {
            var result = this.All(k => record.Any(r => k.Key == r.Key && k.Value == r.Value));
            return result;
        }

        #endregion

        #region static format

        //public IList<string> KeyList()
        //{
        //    return this.Keys.ToList<string>();
        //}

        //public IEnumerable<string> Sorted()
        //{
        //    return this.Keys.OrderBy(k => k);
        //}


        public static string FormatPrimaryKey(object instance, IEnumerable<string> keys, bool sortedByFields = false)
        {
            IEnumerable<object> values = AttributeProvider.SelectPropertiesValues(instance, keys, sortedByFields);
            return FormatPrimaryKey(KeySet.Separator, values.ToArray());
        }

        //public static string FormatPrimaryKey(object instance, IEnumerable<string> keys, bool sorted = false)
        //{
        //    IEnumerable<object> values = AttributeProvider.GetPropertiesValues(instance, keys, sorted);
        //    return FormatPrimaryKey(KeySet.Separator, values.ToArray());
        //}

        public static string FormatPrimaryKey(object[] values)
        {
            return FormatPrimaryKey(KeySet.Separator, values);
            //return string.Join("_", keys);
        }

        private static string FormatPrimaryKey(string separator,object[] values)
        {

            if (values == null || values.Length == 0)
                return null;
            if (values.Length == 1)
                return values[0] == null ? null : values[0].ToString();

            return string.Join(separator, values);

            //return string.Join(separator, keys);
        }


        #endregion

        public static string GetPrimaryKey(string[] keyValueArgs, string[] fieldsKey)
        {

            if (keyValueArgs == null)
            {
                throw new ArgumentNullException("keyValueArgs");
            }
            if (fieldsKey == null)
            {
                throw new ArgumentNullException("fieldsKey");
            }

            string[] cleanFieldsKey = CleanKeys(fieldsKey);
            int length = fieldsKey.Length;


            int count = keyValueArgs.Length;
            if (count % 2 != 0)
            {
                throw new ArgumentException("values parameter not correct, Not match key value arguments");
            }
            if ((length * 2) < count)
            {
                throw new ArgumentOutOfRangeException("values parameter Not match to fieldsKey range");
            }

            object[] values = new object[length];

            for (int i = 0; i < count; i++)
            {
                string key = keyValueArgs[i];
                int index = cleanFieldsKey.IndexOf(key);
                ++i;
                if (index >= 0)
                {
                    values[index] = keyValueArgs[i];
                }
            }

            return KeySet.FormatPrimaryKey(values);
        }

        public static string GetPrimaryKey(NameValueArgs keyValueArgs, string[] fieldsKey)
        {

            if (keyValueArgs == null)
            {
                throw new ArgumentNullException("keyValueArgs");
            }
            if (fieldsKey == null)
            {
                throw new ArgumentNullException("fieldsKey");
            }

            string[] cleanFieldsKey = CleanKeys(fieldsKey);
            int length = fieldsKey.Length;


            int count = keyValueArgs.Count;
            if (length < count)
            {
                throw new ArgumentOutOfRangeException("values parameter Not match to fieldsKey range");
            }

            object[] values = new object[length];
            foreach (var entry in keyValueArgs)
            {
                int index = cleanFieldsKey.IndexOf(entry.Key);
                if (index >= 0)
                {
                    values[index] = entry.Value;
                }
            }

            return KeySet.FormatPrimaryKey(values);
        }

        public static string GetPrimaryKey(NameValueArgs keyValueArgs)
        {

            if (keyValueArgs == null)
            {
                throw new ArgumentNullException("keyValueArgs");
            }

            int count = keyValueArgs.Count;
            int i = 0;
            object[] values = new object[count];
            foreach (var entry in keyValueArgs)
            {
                values[i] = entry.Value;
                i++;
            }

            return KeySet.FormatPrimaryKey(values);
        }
        public static string GetPrimaryKey(string queryString, string[] fieldsKey)
        {

            if (queryString == null)
            {
                throw new ArgumentNullException("queryString");
            }

            NameValueArgs nv = NameValueArgs.ParseQueryString(queryString);

            return GetPrimaryKey(nv,fieldsKey);
        }

    }
}
