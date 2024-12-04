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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
#pragma warning disable  CS1591

namespace Nistec.Serialization
{
    public static class JsonConverter
    {

         public static object ChangeType(object value, Type type, CultureInfo culture)// bool useUTCDateTime, JsonDateFormat format)
        {
            try
            {
                if (culture == null)
                    culture = CultureInfo.CurrentCulture;//CultureInfo.InvariantCulture

                //if (value == null || value == DBNull.Value || value.ToString() == "")
                //{
                //    if (IsNullable(type))
                //        return value;
                //    else
                //        return UnderlyingTypeOf(type);
                //}
                if (type == typeof(DateTime?))
                    return Types.ToNullableDate(value.ToString());


                if (type.IsNullableType())
                {
                    Type underLineType = Nullable.GetUnderlyingType(type) ?? type;

                    //Coalesce to set the safe value using default(t) or the safe type.
                    if (value == null || value.ToString() == "")
                        return GenericTypes.Default(type);
                    
                    else if (underLineType == typeof(bool))
                        return Types.StringToBool(value.ToString(), false);
                    else if (underLineType == typeof(DateTime))
                        return ToDateTimeObject(value.ToString(), true);
                    return Convert.ChangeType(value, underLineType, culture);//.InvariantCulture);
                }
                if (type == typeof(string))
                {
                    return (string)value;
                }
                if (Types.IsNumericType(type))
                {
                    return Types.IsNumeric(value) == false ? GenericTypes.Default(type) : Convert.ChangeType(value, type, culture);
                }
                if (type == typeof(bool))
                {
                    return Types.StringToBool(value.ToString(), false);
                }
                if (type == typeof(DateTime))
                {
                    return ToDateTime(value.ToString(), true);
                }
                if (type == typeof(Guid))
                {
                    return Types.ToGuid(value);
                }
                return (value == null || value == DBNull.Value) ? GenericTypes.Default(type) : Convert.ChangeType(value, type, culture);
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                return GenericTypes.Default(type);
            }
        }


        #region   private methods
      
  
        public static bool IsNullable(Type t)
        {
            if (!t.IsGenericType) return false;
            Type g = t.GetGenericTypeDefinition();
            return (g.Equals(typeof(Nullable<>)));
        }

        public static Type UnderlyingTypeOf(Type t)
        {
            //return t.GetGenericArguments()[0];
            var arg = t.GetGenericArguments();
            if (arg == null || arg.Length < 1)
                return null;
            return arg[0];
        }

        internal static int ToInteger(string s, int index, int count)
        {
            s = s.Substring(index, count);
            s = s.TrimStart('0','-', '+');
            s = s.TrimEnd();
            return Types.ToInt(s);
        }
        internal static int ToInteger(string s)
        {
            if (s == null)
                return 0;
            s = s.TrimStart('0', '-', '+');
            s = s.TrimEnd();
            return Types.ToInt(s);
        }
        public static int ToInt(object o)
        {
            if (o == null)
                return 0;
            if (o.GetType() == typeof(int))
                return (int)o;
            return Types.ToInt(o);
        }
        public static long ToLong(object o)
        {
            if (o == null)
                return 0;
            if (o.GetType() == typeof(long))
                return (long)o;
            return Types.ToLong(o);
        }
        public static bool ToBool(object o)
        {
            if (o == null)
                return false;
            if (o.GetType() == typeof(bool))
                return (bool)o;
            return Types.ToBool(o,false);
        }
        public static Guid ToGuid(string s)
        {
            if (s == null)
                return Guid.Empty;

            if (s.Length > 30)
                return new Guid(s);
            else
                return new Guid(Convert.FromBase64String(s));
        }

        public static object ToEnum(Type type, object value)
        {
            if (value == null)
                return value;
            return EnumExtension.Parse(type, value.ToString(), null);
        }
        public static object ToDateTimeObject(string value, bool useUTCDateTime)
        {

            if (value == null || value == string.Empty)
                return null;
            if (value.Length < 12)
                return null;
            return ToDateTime(value, useUTCDateTime);
        }

        public static DateTime ToDateTime(string value, bool useUTCDateTime, JsonDateFormat format)
        {
            if (format == JsonDateFormat.dynamic)
                return ToDateTime(value, useUTCDateTime);

            if (string.IsNullOrEmpty(value))
                return Types.MinDate;

            //                   0123456789012345678 9012 9/3
            // datetime format = yyyy-MM-dd HH:mm:ss .nnn  Z
            // datetime format = dd/MM/yyyy HH:mm:ss .nnn  Z
            //                   4/21/2015 12:00:00 AM


            try
            {

                bool utc = false;
                int hour = 0, min = 0, sec = 0, ms = 0;
                int year = 0, month = 0, day = 0;

                var args = value.Split('/', '-', 'T', ':','.',' ');
                if(args.Length<3)
                    return Types.MinDate;
                value = value.ToLower();
                               
                switch (format)
                {
                    case JsonDateFormat.iso:
                    case JsonDateFormat.iso_hhmm:
                    case JsonDateFormat.iso_short:
                        year = ToInteger(args[0]);
                        month = ToInteger(args[1]);
                        day = ToInteger(args[2]);
                        break;
                    case JsonDateFormat.longDate:
                        return Types.ToDateTime(value, DateFormat.longDate);// DateTime.Parse(value);
                        //return Types.ToDateTime(value, new DateTimeFormatInfo() { LongDatePattern= "dddd d MMMM yyyy" });// DateTime.Parse(value);
                    case JsonDateFormat.mmddyyyy:
                    case JsonDateFormat.mmddyyyy_hhmm:
                    case JsonDateFormat.mmddyyyy_hhmmss:
                        year = ToInteger(args[2]);
                        month = ToInteger(args[0]);
                        day = ToInteger(args[1]);
                        break;
                    case JsonDateFormat.ddmmyyyy:
                    case JsonDateFormat.ddmmyyyy_hhmm:
                    case JsonDateFormat.ddmmyyyy_hhmmss:
                        year = ToInteger(args[2]);
                        month = ToInteger(args[1]);
                        day = ToInteger(args[0]);
                        break;
                    case JsonDateFormat.dynamic:
                    default:
                        if (args[0].Length == 4)//iso
                        {
                            year = ToInteger(args[0]);
                            month = ToInteger(args[1]);
                            day = ToInteger(args[2]);
                        }
                        else if (value.Contains("am") || value.Contains("pm"))//mmddyyyy
                        {
                            year = ToInteger(args[2]);
                            month = ToInteger(args[0]);
                            day = ToInteger(args[1]);
                        }
                        else//ddmmyyyy
                        {
                            year = ToInteger(args[2]);
                            month = ToInteger(args[1]);
                            day = ToInteger(args[0]);
                            if (month > 12 && day <= 12)//mmddyyyy
                            {
                                int tmp = month;
                                month = day;
                                day = tmp;
                            }
                        }
                        break;
                }

                switch (format)
                {
                    case JsonDateFormat.iso_short:
                    case JsonDateFormat.mmddyyyy:
                    case JsonDateFormat.ddmmyyyy:
                        //do nothing
                        break;
                    case JsonDateFormat.iso_hhmm:
                    case JsonDateFormat.mmddyyyy_hhmm:
                    case JsonDateFormat.ddmmyyyy_hhmm:
                        if (args.Length > 3)
                            hour = ToInteger(args[3]);
                        if (args.Length > 4)
                            min = ToInteger(args[4]);
                        break;
                    case JsonDateFormat.iso:
                    case JsonDateFormat.mmddyyyy_hhmmss:
                    case JsonDateFormat.ddmmyyyy_hhmmss:
                    case JsonDateFormat.dynamic:
                        if (args.Length > 3)
                            hour = ToInteger(args[3]);
                        if (args.Length > 4)
                            min = ToInteger(args[4]);
                        if (args.Length > 5)
                            sec = ToInteger(args[5]);
                        if (args.Length > 6 && value[19] == '.')
                            ms = ToInteger(args[6]);

                        if (value.Contains("pm"))
                        {
                            hour = (hour % 12) + 12; //convert 12-hour time to 24-hour
                        }
                        //else if (value.Contains("am"))
                        //{
                        //    hour = (hour % 12);
                        //}

                        break;
                }

                //if (value.EndsWith("Z"))
                if (value[value.Length - 1] == 'z')
                    utc = true;

                if (value.Length < 16)
                {
                    if (useUTCDateTime == false && utc == false)
                        return new DateTime(year, month, day);
                    else
                        return new DateTime(year, month, day, 0, 0, 0, ms, DateTimeKind.Utc).ToLocalTime();
                }

                if (useUTCDateTime == false && utc == false)
                    return new DateTime(year, month, day, hour, min, sec, ms);
                else
                    return new DateTime(year, month, day, hour, min, sec, ms, DateTimeKind.Utc).ToLocalTime();
            }
            catch(Exception)
            {
                throw new Exception("JsonConvert ToDateTime format error- " + value + " ,useUTCDateTime- " + useUTCDateTime.ToString() + " ,format- " + format.ToString());
                //return Types.MinDate;
            }
        }

        public static DateTime ToDateTime(string value, bool useUTCDateTime=false)
        {
            if (string.IsNullOrEmpty(value))
                return Types.MinDate;
            //                   0123456789012345678 9012 9/3
            // datetime format = yyyy-MM-dd HH:mm:ss .nnn  Z
            // datetime format = dd/MM/yyyy HH:mm:ss .nnn  Z
            //                   4/21/2015 12:00:00 AM


            try
            {
                //DateTime val;
                //if (DateTime.TryParse(value, CultureInfo.CurrentCulture.DateTimeFormat, DateTimeStyles.AssumeLocal, out val))
                //    return val;

                bool utc = false;
                int hour = 0, min = 0, sec = 0, ms = 0;
                int year = 0, month = 0, day = 0;

                var args = value.Split('/', '-', 'T', ':', '.',' ');
                if (args.Length < 3)
                    return Types.MinDate;
                value = value.ToLower();
                if (args[0].Length == 4)//iso
                {
                    year = ToInteger(args[0]);
                    month = ToInteger(args[1]);
                    day = ToInteger(args[2]);
                }
                else if (value.Contains("am") || value.Contains("pm"))//mmddyyyy
                {
                    year = ToInteger(args[2]);
                    month = ToInteger(args[0]);
                    day = ToInteger(args[1]);
                }
                else//ddmmyyyy
                {
                    year = ToInteger(args[2]);
                    month = ToInteger(args[1]);
                    day = ToInteger(args[0]);
                    if (month > 12 && day <= 12)
                    {
                        int tmp = month;
                        month = day;
                        day = tmp;
                    }
                }

                if (args.Length > 3)
                    hour =ToInteger(args[3]);
                if (args.Length > 4)
                    min = ToInteger(args[4]);
                if (args.Length > 5)
                    sec = ToInteger(args[5]);
                if (args.Length > 6 && value[19] == '.')
                    ms = ToInteger(args[6]);

                if (value.Contains("pm"))
                {
                    hour = (hour % 12) + 12; //convert 12-hour time to 24-hour
                }
                //else if (value.Contains("am"))
                //{
                //    hour = (hour % 12);
                //}


                //if (value.EndsWith("Z"))
                if (value[value.Length - 1] == 'z')
                    utc = true;

                //if (value.Length < 16)
                //{
                //    if (useUTCDateTime == false && utc == false)
                //        return new DateTime(year, month, day);
                //    else
                //        return new DateTime(year, month, day, 0, 0, 0, ms, DateTimeKind.Utc).ToLocalTime();
                //}

                if (useUTCDateTime  && utc )
                    return new DateTime(year, month, day, hour, min, sec, ms, DateTimeKind.Utc).ToLocalTime();
                else
                    return new DateTime(year, month, day, hour, min, sec, ms);

            }
            catch (Exception)
            {
                throw new Exception("JsonConvert ToDateTime error- " + value + " ,useUTCDateTime- " + useUTCDateTime.ToString());
                //return Types.MinDate;
            }
        }
        #endregion

        #region formatter

        internal static string Indent = "   ";

        internal static void AppendIndent(StringBuilder sb, int count)
        {
            for (; count > 0; --count) sb.Append(Indent);
        }
        public static string PrintJson(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var output = new StringBuilder();
            int depth = 0;
            int len = input.Length;
            char[] chars = input.ToCharArray();
            for (int i = 0; i < len; ++i)
            {
                char ch = chars[i];

                if (ch == '\"') // found string span
                {
                    bool str = true;
                    while (str)
                    {
                        output.Append(ch);
                        ch = chars[++i];
                        if (ch == '\\')
                        {
                            output.Append(ch);
                            ch = chars[++i];
                        }
                        else if (ch == '\"')
                            str = false;
                    }
                }

                switch (ch)
                {
                    case '{':
                    case '[':
                        output.Append(ch);
                        output.AppendLine();
                        AppendIndent(output, ++depth);
                        break;
                    case '}':
                    case ']':
                        output.AppendLine();
                        AppendIndent(output, --depth);
                        output.Append(ch);
                        break;
                    case ',':
                        output.Append(ch);
                        output.AppendLine();
                        AppendIndent(output, depth);
                        break;
                    case ':':
                        output.Append(" : ");
                        break;
                    default:
                        if (!char.IsWhiteSpace(ch))
                            output.Append(ch);
                        break;
                }
            }

            return output.ToString();
        }

        public static string PrintReflat(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var output = new StringBuilder();
            int depth = 0;
            int len = input.Length;
            char[] chars = input.ToCharArray();
            for (int i = 0; i < len; ++i)
            {
                char ch = chars[i];

                if (ch == '\"') // found string span
                {
                    bool str = true;
                    while (str)
                    {
                        output.Append(ch);
                        ch = chars[++i];
                        if (ch == '\\')
                        {
                            output.Append(ch);
                            ch = chars[++i];
                        }
                        else if (ch == '\"')
                            str = false;
                    }
                }

                switch (ch)
                {
                    case '{':
                        //output.Append(ch);
                        output.AppendLine();
                        //AppendIndent(output, ++depth);
                        break;
                    case '[':
                        output.Append(ch);
                        //output.AppendLine();
                        //AppendIndent(output, ++depth);
                        break;
                    case '}':
                        output.AppendLine();
                        //AppendIndent(output, --depth);
                        //output.Append(ch);
                        break;
                    case ']':
                        //output.AppendLine();
                        //AppendIndent(output, --depth);
                        output.Append(ch);
                        break;
                    case ',':
                        output.Append(ch);
                        output.AppendLine();
                        AppendIndent(output, depth);
                        break;
                    case ':':
                        output.Append(" : ");
                        break;
                    default:
                        if (!char.IsWhiteSpace(ch))
                            output.Append(ch);
                        break;
                }
            }

            return output.ToString();
        }
        #endregion
    }
}
