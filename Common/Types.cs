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
using System.Globalization;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Nistec.Generic;
using System.Runtime.Serialization;
using System.IO;
using Nistec.Serialization;
using System.Xml;
using System.Data;
using System.Collections.Concurrent;

namespace Nistec
{

    #region ObjectExtension

    #pragma warning disable  CS1591
    public static class ObjectExtension
    {
        public static bool IsNull(this Object obj)
        {
            if (obj == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //public static bool IsNotNull(this Object obj)
        //{
        //    if (IsNull(obj))
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}

        public static string ToProprtyString(this object obj, bool multiline=true)
        {
            if (obj.IsNull()) return string.Empty;
            StringBuilder sb = new StringBuilder();
            foreach (var p in obj.GetType().GetProperties())
            {
                var o = p.GetValue(obj, null);
                var ostr = !o.IsNull() ? o.ToString() : "";
                if (multiline)
                    sb.AppendLine(p.Name + ":" + ostr + ", ");
                else
                    sb.Append(p.Name + ":" + ostr + ", ");
            }
            return sb.ToString();
        }
    }


    #endregion

    #region Strings

    public sealed class Strings
    {
        internal static readonly CompareInfo m_InvariantCompareInfo;

        static Strings()
        {
            Strings.m_InvariantCompareInfo = CultureInfo.InvariantCulture.CompareInfo;
        }

        public static bool IsBase64String(string s)
        {
            if (string.IsNullOrEmpty(s))
                return false;
            s = s.Trim();
            return (s.Length % 4 == 0) && Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
        }
        public static bool IsJsonString(string s)
        {
            if (string.IsNullOrEmpty(s))
                return false;
            s = s.Trim();
            return (s.StartsWith("{") && s.EndsWith("}")) || (s.StartsWith("[") && s.EndsWith("]")) || (s.StartsWith("\"{") && s.EndsWith("}\"")) || (s.StartsWith("\"[") && s.EndsWith("]\""));
        }
        public static bool IsXmlString(string s)
        {
            if (string.IsNullOrEmpty(s))
                return false;
            s = s.Trim();
            return s.StartsWith("<") && s.EndsWith(">")|| s.StartsWith("\"<") && s.EndsWith(">\"");
        }

        public static string ReflatJson(string json, bool pretty=true)
        {
            if (json == null)
                return "na";

            json = json.Replace("\\\\", "\\").Replace("\\\"", "\"").Replace("\\\"", "\"").Replace("\\r\\n", "").Trim('\"');
            if (!IsJsonString(json))
            {
                return json;
            }
            if (pretty)
            {
                return json.Replace("{", "").Replace("}", Environment.NewLine).Replace("}\"", Environment.NewLine).Replace(",", Environment.NewLine);//.Replace(",\"", Environment.NewLine);
            }
            else
                return json.Replace("{", "").Replace("}", Environment.NewLine);
        }

        public static string StrReverse(string Expression)
        {
            if (Expression == null)
            {
                return "";
            }
            int num1 = Expression.Length;
            if (num1 == 0)
            {
                return "";
            }
            int num3 = num1 - 1;
            for (int num2 = 0; num2 <= num3; num2++)
            {
                char ch1 = Expression[num2];
                switch (char.GetUnicodeCategory(ch1))
                {
                    case UnicodeCategory.Surrogate:
                    case UnicodeCategory.NonSpacingMark:
                    case UnicodeCategory.SpacingCombiningMark:
                    case UnicodeCategory.EnclosingMark:
                        return Strings.InternalStrReverse(Expression, num2, num1);
                }
            }
            char[] chArray1 = Expression.ToCharArray();
            Array.Reverse(chArray1);
            return new string(chArray1);
        }

        private static string InternalStrReverse(string Expression, int SrcIndex, int Length)
        {
            StringBuilder builder1 = new StringBuilder(Length);
            builder1.Length = Length;
            TextElementEnumerator enumerator1 = StringInfo.GetTextElementEnumerator(Expression, SrcIndex);
            if (!enumerator1.MoveNext())
            {
                return "";
            }
            int num2 = 0;
            int num1 = Length - 1;
            while (num2 < SrcIndex)
            {
                builder1[num1] = Expression[num2];
                num1--;
                num2++;
            }
            int num3 = enumerator1.ElementIndex;
            while (num1 >= 0)
            {
                SrcIndex = num3;
                if (enumerator1.MoveNext())
                {
                    num3 = enumerator1.ElementIndex;
                }
                else
                {
                    num3 = Length;
                }
                for (num2 = num3 - 1; num2 >= SrcIndex; num2--)
                {
                    builder1[num1] = Expression[num2];
                    num1--;
                }
            }
            return builder1.ToString();
        }

        public static int Asc(char String)
        {
            int num1;
            int num2 = Convert.ToInt32(String);
            if (num2 < 0x80)
            {
                return num2;
            }
            try
            {
                byte[] buffer1;
                int num3;
                Encoding encoding1 = Types.GetFileIOEncoding();
                char[] chArray1 = new char[] { String };
                if (encoding1.GetMaxByteCount(1) == 1)
                {
                    buffer1 = new byte[1];
                    num3 = encoding1.GetBytes(chArray1, 0, 1, buffer1, 0);
                    return buffer1[0];
                }
                buffer1 = new byte[2];
                num3 = encoding1.GetBytes(chArray1, 0, 1, buffer1, 0);
                if (num3 == 1)
                {
                    return buffer1[0];
                }
                if (BitConverter.IsLittleEndian)
                {
                    byte num4 = buffer1[0];
                    buffer1[0] = buffer1[1];
                    buffer1[1] = num4;
                }
                num1 = BitConverter.ToInt16(buffer1, 0);
            }
            catch (Exception exception1)
            {
                throw exception1;
            }
            return num1;
        }

        public static int Asc(string String)
        {
            if ((String == null) || (String.Length == 0))
            {
                throw new ArgumentException("Argument_Length Zero", "String");
            }
            char ch1 = String[0];
            return Strings.Asc(ch1);
        }

        public static int AscW(string String)
        {
            if ((String == null) || (String.Length == 0))
            {
                throw new ArgumentException("Argument_Length Zero", "String");
            }
            return String[0];
        }

        public static int AscW(char String)
        {
            return String;
        }

        public static char Chr(int CharCode)
        {
            char ch1;
            if ((CharCode < -32768) || (CharCode > 0xffff))
            {
                throw new ArgumentException("Argument_Range Two Bytes ", "CharCode");
            }
            if ((CharCode >= 0) && (CharCode <= 0x7f))
            {
                return Convert.ToChar(CharCode);
            }
            try
            {
                int num1;
                Encoding encoding1 = Encoding.GetEncoding(Types.GetLocaleCodePage());
                if ((encoding1.GetMaxByteCount(1) == 1) && ((CharCode < 0) || (CharCode > 0xff)))
                {
                    throw new Exception("Error Encoding");
                }
                char[] chArray1 = new char[2];
                byte[] buffer1 = new byte[2];
                Decoder decoder1 = encoding1.GetDecoder();
                if ((CharCode >= 0) && (CharCode <= 0xff))
                {
                    buffer1[0] = (byte)(CharCode & 0xff);
                    num1 = decoder1.GetChars(buffer1, 0, 1, chArray1, 0);
                }
                else
                {
                    buffer1[0] = (byte)((CharCode & 0xff00) / 0x100);
                    buffer1[1] = (byte)(CharCode & 0xff);
                    num1 = decoder1.GetChars(buffer1, 0, 2, chArray1, 0);
                }
                ch1 = chArray1[0];
            }
            catch (Exception exception1)
            {
                throw exception1;
            }
            return ch1;
        }

        public static char ChrW(int CharCode)
        {
            if ((CharCode < -32768) || (CharCode > 0xffff))
            {
                throw new ArgumentException("Argument_Range Two Bytes", "CharCode");
            }
            return Convert.ToChar((int)(CharCode & 0xffff));
        }

        public static int StrCmp(string sLeft, string sRight, bool TextCompare)
        {
            if (sLeft == null)
            {
                sLeft = "";
            }
            if (sRight == null)
            {
                sRight = "";
            }
            if (TextCompare)
            {
                return Types.GetCultureInfo().CompareInfo.Compare(sLeft, sRight, CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreCase);
            }
            return string.CompareOrdinal(sLeft, sRight);
        }


        /// <summary>
        /// Split string to segements by maxLength per segement
        /// </summary>
        /// <param name="s"></param>
        /// <param name="limt"></param>
        /// <param name="maxLengthPerSigment"></param>
        /// <returns></returns>
        public static string[] SplitString(string s, int limt, int maxLengthPerSigment)
        {
            return SplitString(s, limt, maxLengthPerSigment, null);
        }

        /// <summary>
        /// Split string to segements by sperator and maxLength per segement
        /// </summary>
        /// <param name="s"></param>
        /// <param name="limt"></param>
        /// <param name="maxLengthPerSigment"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string[] SplitString(string s, int limt, int maxLengthPerSigment, string separator)
        {
            string source = s;//"1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890";// 1234567890 1234567890 1234567890 ";
            //int sigment = 1;
            List<string> strResult = new List<string>();
            //int limt=70;
            //int maxCharPerSigment=65;
            int length = source.Length;
            if (length <= maxLengthPerSigment)
            {
                return new string[] { s };
            }

            if (length > limt)
            {
                //sigment = (int)Math.Ceiling((float)length / (float)maxCharPerSigment);
                source = source.Substring(0, limt);//
                length = source.Length;
            }

            //strResult = new string[sigment];
            int current = 0;
            int currentIndex = 0;
            int currentStartIndex = 0;
            int currentLength = 0;
            //int currentCharPerSigment = maxLengthPerSigment;

            do
            {
                currentStartIndex += maxLengthPerSigment;// currentCharPerSigment;
                if (currentStartIndex > length)
                {
                    currentStartIndex = length;
                    currentIndex = length;
                }
                else if (separator != null)
                {
                    currentIndex = source.LastIndexOf(separator, currentStartIndex);
                    if (currentIndex > -1) //currentIndex++;
                        currentStartIndex = currentIndex;
                }
                else
                {
                    currentIndex = currentStartIndex;
                }

                if (currentIndex == -1)
                {
                    currentLength = currentIndex - current;
                    strResult.Add(source.Substring(current, length - current));
                    break;
                }
                else
                {
                    currentLength = currentIndex - current;
                    strResult.Add(source.Substring(current, currentLength));
                }
                current += (currentLength);
                //currentCharPerSigment += currentIndex;
            } while (currentStartIndex < length);

            return strResult.ToArray();

        }

        public static List<string> SplitToList(string s, char spliter)
        {
            if (string.IsNullOrEmpty(s))
                return null;

            List<string> list = new List<string>();
            return s.Split(spliter).ToList<string>();

        }
        public static string ArrayToString(string[] array)
        {
            if (array == null || array.Length == 0)
            {
                return string.Empty;
            }
            return array.JoinTrim();
        }

        public static string ArrayToString(string[] array, char spliter, bool trimEnd)
        {
            if (array == null || array.Length == 0)
            {
                return string.Empty;
            }
            StringBuilder sb = new StringBuilder();
            foreach (string s in array)
            {
                sb.AppendFormat("{0}{1}", s, spliter);
            }
            if (trimEnd)
            {
                sb.Remove(sb.Length - 1, 1);
            }
            return sb.ToString();
        }

        public static string IntTimeToString(int time)
        {
            string t = time.ToString();
            t = t.Insert(t.Length - 2, ":");
            return t;
            //return string.Format("{0}:{1}", t.Substring(0, 2), t.Substring(2, 2));
        }
        public static int StringTimeToInt(string time)
        {
            string t = time.Replace(":", "").TrimStart('0');
            return Convert.ToInt32(t);
        }
        public static TimeSpan StringTimeToTimeSpan(string time)
        {
            string[] t = time.Split(':');
            return new TimeSpan(Convert.ToInt32(t[0]), Convert.ToInt32(t[1]), 0);
        }

        public static string Trim(string value, int length = 0)
        {
            if (value == null)
                return null;
            value = value.Trim();

            if (length > 0 && value.Length > length)
                return value.Substring(0, length);

            return value;
        }

        public static string[] SplitTrim(string s, params char[] spliter)
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

        public static string[] SplitTrim(string s, params string[] spliter)
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

        public static string[] SplitTrim(string str, string defaultValue, char splitter = ',')
        {
            string[] list = string.IsNullOrEmpty(str) ? new string[] { defaultValue } : SplitTrim(str, splitter);
            return list;
        }

        public static string[] ReTrim(string[] str, string defaultValue = "")
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

        public static string JoinTrim(string[] str, string splitter = ",")
        {
            if (str == null)
            {
                throw new ArgumentNullException("JoinTrim.str");
            }

            return string.Join(splitter, ReTrim(str));
        }

        public static bool IsEqual(string[] strA, string[] strB)
        {
            if (strA == null && strB == null)
                return true;

            if (strA == null || strB == null)
            {
                return false;
            }

            return string.Join(" ", strA) == string.Join(" ", strB);
        }

        public static bool IsWhiteSpace(string s)
        {
            if (s == null)
                throw new ArgumentNullException("s");

            if (s.Length == 0)
                return false;

            for (int i = 0; i < s.Length; i++)
            {
                if (!char.IsWhiteSpace(s[i]))
                    return false;
            }

            return true;
        }

        public static string NullEmpty(string s)
        {
            return (string.IsNullOrEmpty(s)) ? null : s;
        }
        public static int Length(string value)
        {
            if (value == null)
                return 0;
            else
                return value.Length;
        }

        public static string ToCamelCase(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            if (!char.IsUpper(s[0]))
                return s;

            char[] chars = s.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                bool hasNext = (i + 1 < chars.Length);
                if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
                    break;

                chars[i] = char.ToLower(chars[i], CultureInfo.InvariantCulture);
            }

            return new string(chars);
        }

    }

    #endregion

    #region Formatter

    /// <summary>
    /// Formatter
    /// </summary>
    public static class Formatter
    {
        public const string ContentTypeForm= "application/x-www-form-urlencoded";
        public const string ContentTypeJson = "application/json";
        public const string ContentTypeXml = "text/xml; charset=utf-8";

        public static string ExceptionFormat(string message, Exception ex, bool enableTrace = false, bool enableInner = true)
        {
            message += " Message: " + ex.Message;
            if (enableInner)
                message += (ex.InnerException != null ? ", Inner: " + ex.InnerException.Message : "");
            if (enableTrace)
                message += ", Trace: " + ex.StackTrace;
            return message;
        }

        public static void DateTimeMinValue()
        {
            typeof(DateTime)
            .GetField("MinValue")
            .SetValue(typeof(DateTime), new DateTime(1901, 1, 1));
        }

        public const string LocalCultureString = "he-IL";

        public static CultureInfo LocalCulture
        {
            get { return CultureInfo.GetCultureInfo(LocalCultureString); }
        }
        public static DateTimeFormatInfo LocalDateTimeFormatInfo
        {
            get { return CultureInfo.GetCultureInfo(LocalCultureString).DateTimeFormat; }
        }

        public static string ToDateFormat(this DateFormat format)
        {
            switch (format)
            {
                case DateFormat.sql:
                    return "yyyy-MM-dd hh:mm:ss";
                case DateFormat.sqlDate:
                case DateFormat.isoDate:
                    return "yyyy-MM-dd";
                case DateFormat.iso:
                    return "yyyy-MM-ddThh:mm:ss";
                case DateFormat.ddmmyyyy:
                case DateFormat.local:
                    return "dd/MM/yyyy";
                case DateFormat.ddmmyyyy_hhmm:
                case DateFormat.localDatetime:
                    return "dd/MM/yyyy hh:mm";
                case DateFormat.ddmmyyyy_hhmmss:
                    return "dd/MM/yyyy hh:mm:ss";
                case DateFormat.longDate:
                    return "dddd, MMMM dd, yyyy";
                case DateFormat.dynamic:
                default:
                    return "yyyy-MM-dd hh:mm:ss";
            }
        }
        public static string ToSqlDateTimeString(this DateTime date)
        {
            return date.ToString("yyyy-MM-dd hh:mm:ss");
        }
        public static string ToSqlDateTimeString(this DateTime? date)
        {
            return (date!=null && date.HasValue) ? date.Value.ToString("yyyy-MM-dd hh:mm:ss"): null;
        }
        public static string ToLocalDateString(this DateTime date)
        {
            return date.ToString("dd/MM/yyyy");
        }
        public static string ToLocalDateString(this DateTime? date)
        {
            return (date != null && date.HasValue) ? date.Value.ToString("dd/MM/yyyy") : null;
        }
        public static string ToLocalDateTimeString(this DateTime date)
        {
            return date.ToString("dd/MM/yyyy hh:mm");
        }
        public static string ToDateTimeString(DateTime date, DateFormat format)
        {
            return date.ToString(format.ToDateFormat());
        }
        public static string ToDateTimeString(DateTime? date, DateFormat format)
        {
            return (date != null && date.HasValue) ? date.Value.ToString(format.ToDateFormat()) : null;
        }
        public static DateTime ParseDateTime(string value, bool useUTCDateTime, DateFormat format)
        {
            if (string.IsNullOrEmpty(value))
                return Types.MinDate;
            if (format == DateFormat.dynamic)
                return ParseDateTime(value, useUTCDateTime);

            //                   0123456789012345678 9012 9/3
            // datetime format = yyyy-MM-dd HH:mm:ss .nnn  Z
            // datetime format = dd/MM/yyyy HH:mm:ss .nnn  Z
            //                   4/21/2015 12:00:00 AM


            try
            {

                bool utc = false;
                int hour = 0, min = 0, sec = 0, ms = 0;
                int year = 0, month = 0, day = 0;

                var args = value.Split('/', '-', 'T', ':', '.', ' ');
                if (args.Length < 3)
                    return Types.MinDate;
                value = value.ToLower();

                switch (format)
                {
                    case DateFormat.sql:
                    case DateFormat.iso:
                    case DateFormat.isoDate:
                    case DateFormat.sqlDate:
                        year = Types.ToInt(args[0]);
                        month = Types.ToInt(args[1]);
                        day = Types.ToInt(args[2]);
                        break;
                    case DateFormat.longDate:
                        return Types.ToDateTime(value, DateFormat.longDate);// DateTime.Parse(value);
                        //return Types.ToDateTime(value, new DateTimeFormatInfo() { LongDatePattern = "dddd d MMMM yyyy" });// DateTime.Parse(value);
                    case DateFormat.mmddyyyy:
                    case DateFormat.mmddyyyy_hhmm:
                    case DateFormat.mmddyyyy_hhmmss:
                        year = Types.ToInt(args[2]);
                        month = Types.ToInt(args[0]);
                        day = Types.ToInt(args[1]);
                        break;
                    case DateFormat.ddmmyyyy:
                    case DateFormat.ddmmyyyy_hhmm:
                    case DateFormat.ddmmyyyy_hhmmss:
                    case DateFormat.local:
                    case DateFormat.localDatetime:
                        year = Types.ToInt(args[2]);
                        month = Types.ToInt(args[1]);
                        day = Types.ToInt(args[0]);
                        break;
                    case DateFormat.dynamic:
                    default:
                        if (args[0].Length == 4)//iso
                        {
                            year = Types.ToInt(args[0]);
                            month = Types.ToInt(args[1]);
                            day = Types.ToInt(args[2]);
                        }
                        else if (value.Contains("am") || value.Contains("pm"))//mmddyyyy
                        {
                            year = Types.ToInt(args[2]);
                            month = Types.ToInt(args[0]);
                            day = Types.ToInt(args[1]);
                        }
                        else//ddmmyyyy
                        {
                            year = Types.ToInt(args[2]);
                            month = Types.ToInt(args[1]);
                            day = Types.ToInt(args[0]);
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
                    case DateFormat.sqlDate:
                    case DateFormat.isoDate:
                    case DateFormat.mmddyyyy:
                    case DateFormat.ddmmyyyy:
                        //do nothing
                        break;
                    case DateFormat.mmddyyyy_hhmm:
                    case DateFormat.ddmmyyyy_hhmm:
                    case DateFormat.localDatetime:
                        if (args.Length > 3)
                            hour = Types.ToInt(args[3]);
                        if (args.Length > 4)
                            min = Types.ToInt(args[4]);
                        break;
                    case DateFormat.sql:
                    case DateFormat.iso:
                    case DateFormat.mmddyyyy_hhmmss:
                    case DateFormat.ddmmyyyy_hhmmss:
                    case DateFormat.dynamic:
                        if (args.Length > 3)
                            hour = Types.ToInt(args[3]);
                        if (args.Length > 4)
                            min = Types.ToInt(args[4]);
                        if (args.Length > 5)
                            sec = Types.ToInt(args[5]);
                        if (args.Length > 6 && value[19] == '.')
                            ms = Types.ToInt(args[6]);

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
            catch (Exception)
            {
                throw new Exception("JsonConvert ToDateTime format error- " + value + " ,useUTCDateTime- " + useUTCDateTime.ToString() + " ,format- " + format.ToString());
                //return Types.MinDate;
            }
        }

        public static DateTime ParseDateTime(string value, bool useUTCDateTime = false)
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

                var args = value.Split('/', '-', 'T', ':', '.', ' ');
                if (args.Length < 3)
                    return Types.MinDate;
                value = value.ToLower();
                if (args[0].Length == 4)//iso|sql
                {
                    year = Types.ToInt(args[0]);
                    month = Types.ToInt(args[1]);
                    day = Types.ToInt(args[2]);
                }
                else if (value.Contains("am") || value.Contains("pm"))//mmddyyyy
                {
                    year = Types.ToInt(args[2]);
                    month = Types.ToInt(args[0]);
                    day = Types.ToInt(args[1]);
                }
                else//ddmmyyyy
                {
                    year = Types.ToInt(args[2]);
                    month = Types.ToInt(args[1]);
                    day = Types.ToInt(args[0]);
                    if (month > 12 && day <= 12)
                    {
                        int tmp = month;
                        month = day;
                        day = tmp;
                    }
                }

                if (args.Length > 3)
                    hour = Types.ToInt(args[3]);
                if (args.Length > 4)
                    min = Types.ToInt(args[4]);
                if (args.Length > 5)
                    sec = Types.ToInt(args[5]);
                if (args.Length > 6 && value[19] == '.')
                    ms = Types.ToInt(args[6]);

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

                if (useUTCDateTime && utc)
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

    }

    public enum DateCharFormat
    {
        d,//: 6/15/2008 
        D,//: Sunday, June 15, 2008 
        f,//: Sunday, June 15, 2008 9:15 PM
        F,//: Sunday, June 15, 2008 9:15:07 PM
        g,//: 6/15/2008 9:15 PM
        G,//: 6/15/2008 9:15:07 PM
        m,//: June 15 
        o,//: 2008-06-15T21:15:07.0000000 
        R,//: Sun, 15 Jun 2008 21:15:07 GMT
        s,//: 2008-06-15T21:15:07 
        t,//: 9:15 PM
        T,//: 9:15:07 PM
        u,//: 2008-06-15 21:15:07Z
        U,//: Monday, June 16, 2008 4:15:07 AM
        y,//: June, 2008 

        //'h:mm:ss.ff t': 9:15:07.00 P 
        //'d MMM yyyy': 15 Jun 2008 
        //'HH:mm:ss.f': 21:15:07.0 
        //'dd MMM HH:mm:ss': 15 Jun 21:15:07 
        //'\Mon\t\h\: M': Month: 6 
        //'HH:mm:ss.ffffzzz': 21:15:07.0000-07:00
    }

    /// <summary>
    /// DateFormat
    /// </summary>
    public enum DateFormat
    {
        sql,//yyyy-MM-dd hh:mm:ss
        sqlDate,//yyyy-MM-dd
        iso,//yyyy-MM-ddThh:mm:ss
        isoDate,//yyyy-MM-dd
        ddmmyyyy,//dd/MM/yyyy
        ddmmyyyy_hhmm,//dd/MM/yyyy hh:mm
        ddmmyyyy_hhmmss,//dd/MM/yyyy hh:mm:ss
        mmddyyyy,
        mmddyyyy_hhmm,
        mmddyyyy_hhmmss,
        longDate,
        local,
        localDatetime,
        dynamic
        // short date pattern d: "M/d/yyyy",
        // long date pattern D: "dddd, MMMM dd, yyyy",
        // short time pattern t: "h:mm tt",
        // long time pattern T: "h:mm:ss tt",
        // long date, short time pattern f: "dddd, MMMM dd, yyyy h:mm tt",
        // long date, long time pattern F: "dddd, MMMM dd, yyyy h:mm:ss tt",
        // month/day pattern M: "MMMM dd",
        // month/year pattern Y: "yyyy MMMM",
        // S is a sortable format that does not vary by culture S: "yyyy\u0027-\u0027MM\u0027-\u0027dd\u0027T\u0027HH\u0027:\u0027mm\u0027:\u0027ss"

    }

    #endregion

    #region Types

    /// <summary>
    /// Types converter
    /// </summary>
    public static class Types //Types
    {
        public static DateTime MinDate { get { return new DateTime(1900, 1, 1); } }

        #region is null

        public static bool IsNull(object value, int minLengrg = 0)
        {
            return (value == null || value == DBNull.Value || value.ToString().Length < minLengrg) ? true : false;
        }

        public static bool IsEmpty(this Guid guid)
        {
            return Guid.Empty == new Guid(guid.ToString());
        }

        /// <summary>
        /// IsEmpty object|string|Guid or number==0
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsEmpty(object value)
        {
            if (value == null || value == DBNull.Value)
                return true;
            Type type = value.GetType();

            if (type == typeof(string))
                return string.IsNullOrEmpty(value.ToString());
            if (type == typeof(Guid))
                return Guid.Empty == new Guid(value.ToString());
            if (type == typeof(int))
                return Types.ToInt(value) == 0;
            if (type == typeof(long))
                return Types.ToLong(value) == 0;
            if (type == typeof(decimal))
                return Types.ToDecimal(value, 0) == 0;
            if (type == typeof(float))
                return Types.ToFloat(value, 0) == 0;
            if (type == typeof(double))
                return Types.ToDouble(value, 0) == 0;
            return false;
        }
        #endregion

        #region Generic convert

        /// <summary>
        /// Generic Converter with default value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T To<T>(object value, T defaultValue= default(T)) where T : IConvertible
        {
            if (value == null || value == DBNull.Value)
                return defaultValue;
            if (typeof(T) == typeof(Object) || typeof(T) == value.GetType())
                return (T)value;
            return Parse<T>(value.ToString(),defaultValue);
        }

        public static T Parse<T>(string value, T defaultValue = default(T)) where T : IConvertible
        {

            if (value == null || value == DBNull.Value.ToString())
                return defaultValue;
            Type t = typeof(T);

            if (t == typeof(string))
                return (T)(object)value;
            if (t == typeof(Guid))
            {
                Guid v;
                return (Guid.TryParse(value, out v)) ? (T)(object)v : defaultValue;
                //return (T)(object) new Guid(value);
            }
            if (t == typeof(int))
            {
                int v;
                return (int.TryParse(value, out v)) ? (T)(object)v : defaultValue;
            }
            if (t == typeof(long))
            {
                long v;
                return (long.TryParse(value, out v)) ? (T)(object)v : defaultValue;
            }
            if (t == typeof(decimal))
            {
                decimal v;
                return (decimal.TryParse(value, out v)) ? (T)(object)v : defaultValue;
            }
            if (t == typeof(float))
            {
                float v;
                return (float.TryParse(value, out v)) ? (T)(object)v : defaultValue;
            }
            if (t == typeof(double))
            {
                double v;
                return (double.TryParse(value, out v)) ? (T)(object)v : defaultValue;
            }
            if (t == typeof(bool))
            {
                bool v;
                return (bool.TryParse(value, out v)) ? (T)(object)v : defaultValue;
            }
            if (t == typeof(DateTime))
            {
                DateTime v;
                return (DateTime.TryParse(value, out v)) ? (T)(object)v : defaultValue;
            }
            if (t == typeof(byte))
            {
                byte v;
                return (byte.TryParse(value, out v)) ? (T)(object)v : defaultValue;
            }
            if (t == typeof(uint))
            {
                uint v;
                return (uint.TryParse(value, out v)) ? (T)(object)v : defaultValue;
            }
            if (t == typeof(ulong))
            {
                ulong v;
                return (ulong.TryParse(value, out v)) ? (T)(object)v : defaultValue;
            }
            if (t == typeof(ushort))
            {
                ushort v;
                return (ushort.TryParse(value, out v)) ? (T)(object)v : defaultValue;
            }
            if (t == typeof(SByte))
            {
                SByte v;
                return (SByte.TryParse(value, out v)) ? (T)(object)v : defaultValue;
            }
            if (t == typeof(short))
            {
                short v;
                return (short.TryParse(value, out v)) ? (T)(object)v : defaultValue;
            }
            else
            {
                var converter = ConverterUtil.GetConverter(t);
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

        }

        public static T NullableParse<T>(string value, T defaultValue = default(T))
        {

            if (value == null || value == DBNull.Value.ToString())
                return defaultValue;
            Type t = typeof(T);

            if (!t.IsNullableType())
            {
                return defaultValue;
            }

            if (t == typeof(string))
                return (T)(object)value;
            if (t == typeof(Guid?))
            {
                Guid v;
                return (Guid.TryParse(value, out v)) ? (T)(object)(Guid?)v : defaultValue;
                //return (T)(object) new Guid(value);
            }
            if (t == typeof(int?))
            {
                int v;
                return (int.TryParse(value, out v)) ? (T)(object)(int?)v : defaultValue;
            }
            if (t == typeof(long?))
            {
                long v;
                return (long.TryParse(value, out v)) ? (T)(object)(long?)v : defaultValue;
            }
            if (t == typeof(decimal?))
            {
                decimal v;
                return (decimal.TryParse(value, out v)) ? (T)(object)(decimal?)v : defaultValue;
            }
            if (t == typeof(float?))
            {
                float v;
                return (float.TryParse(value, out v)) ? (T)(object)(float?)v : defaultValue;
            }
            if (t == typeof(double?))
            {
                double v;
                return (double.TryParse(value, out v)) ? (T)(object)(double?)v : defaultValue;
            }
            if (t == typeof(bool?))
            {
                bool v;
                return (bool.TryParse(value, out v)) ? (T)(object)(bool?)v : defaultValue;
            }
            if (t == typeof(DateTime?))
            {
                DateTime v;
                return (DateTime.TryParse(value, out v)) ? (T)(object)(DateTime?)v : defaultValue;
            }
            if (t == typeof(byte?))
            {
                byte v;
                return (byte.TryParse(value, out v)) ? (T)(object)(byte?)v : defaultValue;
            }
            if (t == typeof(uint?))
            {
                uint v;
                return (uint.TryParse(value, out v)) ? (T)(object)(uint?)v : defaultValue;
            }
            if (t == typeof(ulong?))
            {
                ulong v;
                return (ulong.TryParse(value, out v)) ? (T)(object)(ulong?)v : defaultValue;
            }
            if (t == typeof(ushort?))
            {
                ushort v;
                return (ushort.TryParse(value, out v)) ? (T)(object)(ushort?)v : defaultValue;
            }
            if (t == typeof(SByte?))
            {
                SByte v;
                return (SByte.TryParse(value, out v)) ? (T)(object)(SByte?)v : defaultValue;
            }
            if (t == typeof(short?))
            {
                short v;
                return (short.TryParse(value, out v)) ? (T)(object)(short?)v : defaultValue;
            }
            else
            {
                var converter = ConverterUtil.GetConverter(t);
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
        }

        //public static T Parse<T>(string value, bool isNullable) where T : IConvertible
        //{
        //    return (T?)Parse<T>(value, default(T));
        //}
        #endregion

        #region NZ

        public static string NzOr(string a, string b)
        {
            if (string.IsNullOrEmpty(a))
                return b;
            return a;
        }

        public static string NzOr(object a, object b)
        {
            if (a == null && b == null)
                return null;
            if (a == null)
                return b.ToString();
            if (b == null)
                return a.ToString();
            return NzOr(a.ToString(), b.ToString());
        }

        /// <summary>
        /// Get indicate wether the value is null or empty, if yes return the given valueIfNull argument.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="valueIfNull"></param>
        /// <returns></returns>
        public static string NZorEmpty(object value, string valueIfNull = null)
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
        public static object NZorDBNull(object value)
        {
            if ((value == null || value == DBNull.Value || value.ToString() == String.Empty))
                return DBNull.Value;
            return value;
        }

        public static bool IsDbNull(object value)
        {
            return (value == null || value == DBNull.Value);
        }
        public static int NZero(int value, int valueIfZero)
        {
            return (value == 0) ? valueIfZero : value;
        }
        public static long NZero(long value, long valueIfZero)
        {
            return (value == 0) ? valueIfZero : value;
        }
        public static double NZero(double value, double valueIfZero)
        {
            return (value == 0) ? valueIfZero : value;
        }
        public static decimal NZero(decimal value, decimal valueIfZero)
        {
            return (value == 0) ? valueIfZero : value;
        }
        public static float NZero(float value, float valueIfZero)
        {
            return (value == 0) ? valueIfZero : value;
        }
        public static int NZequal(int value, int equalTo, int valueIfEqual)
        {
            return (value == equalTo) ? valueIfEqual : value;
        }
        public static int NZlessThen(int value, int lessThen, int valueIfLessThen, bool lessThenEqual = false)
        {
            return lessThenEqual ? (value <= lessThen ? valueIfLessThen : value) : (value < lessThen ? valueIfLessThen : value);
        }
        public static int NZgreatThenl(int value, int greatThen, int valueIfGreatThen, bool greatThenEqual = false)
        {
            return greatThenEqual ? (value >= greatThen ? valueIfGreatThen : value) : (value > greatThen ? valueIfGreatThen : value);
        }
        public static object NZeq(object value, object equalTo, object ifYes,object ifNot)
        {
            return (value == equalTo) ? ifYes : ifNot;
        }
        public static string NZeq(string value, string equalTo, string ifYes, string ifNot)
        {
            return (value == equalTo) ? ifYes : ifNot;
        }


        public static object ChangeType(object value, Type type)
        {
            try
            {

                if (type.IsNullableType())
                {
                    Type underLineType = Nullable.GetUnderlyingType(type) ?? type;

                    //Coalesce to set the safe value using default(t) or the safe type.
                    if (value == null || value.ToString() == "")
                        return GenericTypes.Default(type);
                    else if (underLineType == typeof(bool))
                        return StringToBool(value.ToString(), false);
                    else if (underLineType == typeof(DateTime))
                    {
                        return ParseDateTimeObject(value, true);
                    }
                    return Convert.ChangeType(value, underLineType);
                }
                if (IsNumericType(type))
                {
                    return IsNumeric(value) == false ? GenericTypes.Default(type) : Convert.ChangeType(value, type);
                }
                if (type == typeof(bool))
                {
                    return StringToBool(value.ToString(), false);
                }
                if (type == typeof(DateTime))
                {
                    return ParseDateTimeObject(value, false, MinDate);//, MinDate);
                }
                if (type == typeof(Guid))
                {
                    return ToGuid(value);
                }
                if (type.IsEnum)
                {
                    return EnumExtension.ParseOrCast(type, value, value);
                }
                return (value == null || value == DBNull.Value) ? GenericTypes.Default(type) : Convert.ChangeType(value, type);
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                if (type == typeof(DateTime))
                    return MinDate;
                return GenericTypes.Default(type);
            }
        }

        public static object NZ(object value)
        {
            try
            {
                return (value == null || value == DBNull.Value) ? null : (object)value;
            }
            catch
            {
                return null;
            }
        }

        public static object NZ(object value, object valueIfNull)
        {
            try
            {
                return (value == null || value == DBNull.Value) ? valueIfNull : (object)value;
            }
            catch
            {
                return valueIfNull;
            }
        }

        public static int NZ(object value, int valueIfNull)
        {
            try
            {
                return (value == null || value == DBNull.Value) ? valueIfNull : ToInt(value.ToString(), NumberStyles.Number, valueIfNull);
            }
            catch
            {
                return valueIfNull;
            }
        }

        public static float NZ(object value, float valueIfNull)
        {
            try
            {
                return (value == null || value == DBNull.Value) ? valueIfNull : ToFloat(value.ToString(), NumberStyles.Number, valueIfNull);
            }
            catch
            {
                return valueIfNull;
            }
        }

        public static double NZ(object value, double valueIfNull)
        {
            try
            {
                return (value == null || value == DBNull.Value) ? valueIfNull : ToDouble(value.ToString(), NumberStyles.Number, valueIfNull);
            }
            catch
            {
                return valueIfNull;
            }
        }

        public static decimal NZ(object value, decimal valueIfNull)
        {
            try
            {
                return (value == null || value == DBNull.Value) ? valueIfNull : ToDecimal(value.ToString(), NumberStyles.Number, valueIfNull);
            }
            catch
            {
                return valueIfNull;
            }
        }


        public static DateTime NZ(object value, DateTime valueIfNull)
        {
            try
            {
                return (value == null || value == DBNull.Value) ? valueIfNull : ToDateTime(value.ToString(), valueIfNull);
            }
            catch
            {
                return valueIfNull;
            }
        }

        public static string NZ(object value, string valueIfNull)
        {
            try
            {
                return (value == null || value == DBNull.Value) ? valueIfNull : value.ToString();
            }
            catch
            {
                return valueIfNull;
            }
        }
        public static bool NZ(object value, bool valueIfNull)
        {
            try
            {
                return (value == null || value == DBNull.Value) ? valueIfNull : StringToBool(value.ToString(), valueIfNull);
            }
            catch
            {
                return valueIfNull;
            }
        }

        #endregion

        #region Types Number Methods


        public static double Fix(double Number)
        {
            if (Number >= 0)
            {
                return Math.Floor(Number);
            }
            return -Math.Floor(-Number);
        }

        public static string GetOnlyDigits(string sourceValue)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char localChar in sourceValue)
            {
                if (Char.IsDigit(localChar)) sb.Append(localChar);
            }
            return sb.ToString();
        }

        public static int GetNextDozen(int value)
        {
            int mod = 0, auxVal = value;
            do
            {
                mod = auxVal % 10;
                if (mod != 0) auxVal++;
            }
            while (mod != 0);

            return auxVal;
        }

        public static Guid ToGuid(object value)
        {
            Guid g = Guid.Empty;

            if (value == null || value == DBNull.Value)
                return g;
            Guid.TryParse(value.ToString(), out g);
            return g;
        }

        public static Guid ToGuid(object value, Guid defaultValue)
        {
            Guid g = defaultValue;

            if (value == null)
                return g;
            Guid.TryParse(value.ToString(), out g);
            return g;
        }


        public static double ToDouble(object value, double defaultValue)
        {
            if (value == null)
                return defaultValue;
            if (value is double)
                return (double)value;
            double val;//0;
            if (Double.TryParse(value.ToString(), NumberStyles.Number, null, out val))
                return val;
            return defaultValue;
        }
        public static decimal ToDecimal(object value, decimal defaultValue)
        {
            if (value == null)
                return defaultValue;
            if (value is decimal)
                return (decimal)value;
            decimal val;//0;
            if (Decimal.TryParse(value.ToString(), NumberStyles.Number, null, out val))
                return val;
            return defaultValue;
        }

        public static int ToInt(object value)
        {
            if (value == null)
                return 0;
            if (value is int)
                return (int)value;
            int val;//0;
            if (int.TryParse(value.ToString(), out val))
                return val;
            return 0;
        }
        public static int ToInt(object value, int defaultValue)
        {
            if (value == null)
                return defaultValue;
            if (value is int)
                return (int)value;
            int val;//0;
            if (int.TryParse(value.ToString(), out val))
                return val;
            return defaultValue;
        }

        public static long ToLong(object value)
        {
            if (value == null)
                return 0;
            if (value is long)
                return (long)value;
            long val;//0;
            if (long.TryParse(value.ToString(), out val))
                return val;
            return 0;
        }
        public static long ToLong(object value, long defaultValue)
        {
            if (value == null)
                return defaultValue;
            if (value is long)
                return (long)value;
            long val;//0;
            if (long.TryParse(value.ToString(), out val))
                return val;
            return defaultValue;
        }

        public static byte ToByte(object value, byte defaultValue)
        {
            if (value == null)
                return defaultValue;
            if (value is byte)
                return (byte)value;
            byte val;//0;
            if (byte.TryParse(value.ToString(), out val))
                return val;
            return defaultValue;
        }

        public static float ToFloat(object value, float defaultValue)
        {
            if (value == null)
                return defaultValue;
            if (value is float)
                return (float)value;
            float val;//0;
            if (float.TryParse(value.ToString(), out val))
                return val;
            return defaultValue;
        }

        public static double ToDouble(object value, NumberStyles style, double defaultValue)
        {
            if (value == null)
                return defaultValue;
            if (value is double)
                return (double)value;
            double val;//0;
            if (Double.TryParse(value.ToString(), style, null, out val))
                return val;
            return defaultValue;
        }

        public static decimal ToDecimal(object value, NumberStyles style, decimal defaultValue)
        {
            if (value == null)
                return defaultValue;
            if (value is decimal)
                return (decimal)value;
            decimal val;//0;
            if (Decimal.TryParse(value.ToString(), style, null, out val))
                return val;
            return defaultValue;
        }
        public static int ToInt(object value, NumberStyles style, int defaultValue)
        {
            if (value == null)
                return defaultValue;
            if (value is int)
                return (int)value;
            int val;//0;
            if (int.TryParse(value.ToString(), style, null, out val))
                return val;
            return defaultValue;
        }

        public static bool ToBool(object value, bool defaultValue)
        {
            if (value == null)
                return defaultValue;
            if (value is bool)
                return (bool)value;
            return StringToBool(value.ToString(), defaultValue);
        }

        public static byte ToByte(object value, NumberStyles style, byte defaultValue)
        {
            if (value == null)
                return defaultValue;
            if (value is byte)
                return (byte)value;
            byte val;//0;
            if (byte.TryParse(value.ToString(), style, null, out val))
                return val;
            return defaultValue;
        }

        public static float ToFloat(object value, NumberStyles style, float defaultValue)
        {
            if (value == null)
                return defaultValue;
            if (value is float)
                return (float)value;
            float val;// 0F;
            if (float.TryParse(value.ToString(), style, null, out val))
                return val;
            return defaultValue;
        }

        private static double DecimalToDouble(IConvertible ValueInterface)
        {
            return Convert.ToDouble(ValueInterface.ToDecimal(null));
        }
        public static object ConvertToNumber(string value)
        {
            if (value == null)
                return null;

            if (Regex.IsMatch(value, @"^(-|)[0-9]\.+[0-9]+$"))
            {
                return ToDouble(value, 0);
            }
            else if (Regex.IsMatch(value, @"^(-|)[1-9]+[0-9]+$"))
            {
                return ToLong(value);
            }

            //var converter = ConverterUtil.GetConverter(typeof(T));
            //if (converter != null)
            //{
            //    try
            //    {
            //        return converter.ConvertFromString(value.ToString());
            //    }
            //    catch { }
            //}
            return null;
        }

        internal static NumberFormatInfo GetNormalizedNumberFormat(NumberFormatInfo InNumberFormat)
        {
            NumberFormatInfo info2;
            NumberFormatInfo info5 = InNumberFormat;
            if (((((info5.CurrencyDecimalSeparator != null) && (info5.NumberDecimalSeparator != null)) && ((info5.CurrencyGroupSeparator != null) && (info5.NumberGroupSeparator != null))) && (((info5.CurrencyDecimalSeparator.Length == 1) && (info5.NumberDecimalSeparator.Length == 1)) && ((info5.CurrencyGroupSeparator.Length == 1) && (info5.NumberGroupSeparator.Length == 1)))) && (((info5.CurrencyDecimalSeparator[0] == info5.NumberDecimalSeparator[0]) && (info5.CurrencyGroupSeparator[0] == info5.NumberGroupSeparator[0])) && (info5.CurrencyDecimalDigits == info5.NumberDecimalDigits)))
            {
                return InNumberFormat;
            }
            info5 = null;
            NumberFormatInfo info4 = InNumberFormat;
            if ((((info4.CurrencyDecimalSeparator != null) && (info4.NumberDecimalSeparator != null)) && ((info4.CurrencyDecimalSeparator.Length == info4.NumberDecimalSeparator.Length) && (info4.CurrencyGroupSeparator != null))) && ((info4.NumberGroupSeparator != null) && (info4.CurrencyGroupSeparator.Length == info4.NumberGroupSeparator.Length)))
            {
                int num3 = info4.CurrencyDecimalSeparator.Length - 1;
                int num1 = 0;
                while (num1 <= num3)
                {
                    if (info4.CurrencyDecimalSeparator[num1] != info4.NumberDecimalSeparator[num1])
                    {
                        goto Label_019D;
                    }
                    num1++;
                }
                int num2 = info4.CurrencyGroupSeparator.Length - 1;
                for (num1 = 0; num1 <= num2; num1++)
                {
                    if (info4.CurrencyGroupSeparator[num1] != info4.NumberGroupSeparator[num1])
                    {
                        goto Label_019D;
                    }
                }
                return InNumberFormat;
            }
            info4 = null;
            Label_019D:
            info2 = (NumberFormatInfo)InNumberFormat.Clone();
            NumberFormatInfo info3 = info2;
            info3.CurrencyDecimalSeparator = info3.NumberDecimalSeparator;
            info3.CurrencyGroupSeparator = info3.NumberGroupSeparator;
            info3.CurrencyDecimalDigits = info3.NumberDecimalDigits;
            info3 = null;
            return info2;
        }

        #endregion

        #region Boolean

        public static bool StringToBool(string value, bool defaultValue)
        {
            bool val = defaultValue;
            try
            {
                if (value == "0" || value == "off")
                    return false;
                if (value == "1" || value == "on")
                    return true;
                if (Boolean.TryParse(value, out val))
                    return Boolean.Parse(value);
                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public static int BoolToInt(object value, bool defaultValue)
        {
            return BoolToInt(ToBool(value, defaultValue));
        }

        public static int BoolToInt(bool value)
        {
            return value ? 1 : 0;
        }
        #endregion

        #region Types Percent Methods

        public static bool IsPercentString(string val, System.IFormatProvider provider)
        {
            System.Globalization.NumberFormatInfo l_Info;
            if (provider == null)
                l_Info = System.Globalization.CultureInfo.CurrentCulture.NumberFormat;
            else
                l_Info = (System.Globalization.NumberFormatInfo)provider.GetFormat(typeof(System.Globalization.NumberFormatInfo));

            if (val.IndexOf(l_Info.PercentSymbol) == -1)
                return false;
            else
                return true;
        }

        public static double StringToPercentDouble(string val,
            System.Globalization.NumberStyles style,
            System.IFormatProvider provider,
            bool p_ConsiderAllStringAsPercent)
        {
            bool l_IsPercentString = IsPercentString(val, provider);
            if (l_IsPercentString)
            {
                return double.Parse(val.Replace("%", ""), style, provider) / 100.0;
            }
            else
            {
                if (p_ConsiderAllStringAsPercent)
                    return double.Parse(val, style, provider) / 100.0;
                else
                    return double.Parse(val, style, provider);
            }
        }
        public static float StringToPercentFloat(string val,
            System.Globalization.NumberStyles style,
            System.IFormatProvider provider,
            bool p_ConsiderAllStringAsPercent)
        {
            bool l_IsPercentString = IsPercentString(val, provider);
            if (l_IsPercentString)
            {
                return float.Parse(val.Replace("%", ""), style, provider) / 100;
            }
            else
            {
                if (p_ConsiderAllStringAsPercent)
                    return float.Parse(val, style, provider) / 100;
                else
                    return float.Parse(val, style, provider);
            }
        }
        public static decimal StringToPercentDecimal(string val,
            System.Globalization.NumberStyles style,
            System.IFormatProvider provider,
            bool p_ConsiderAllStringAsPercent)
        {
            bool l_IsPercentString = IsPercentString(val, provider);
            if (l_IsPercentString)
            {
                return decimal.Parse(val.Replace("%", ""), style, provider) / 100.0M;
            }
            else
            {
                if (p_ConsiderAllStringAsPercent)
                    return decimal.Parse(val, style, provider) / 100.0M;
                else
                    return decimal.Parse(val, style, provider);
            }
        }


        #endregion

        #region DateTime

        private static Calendar CurrentCalendar
        {
            get
            {
                return Thread.CurrentThread.CurrentCulture.Calendar;
            }
        }

       
        public static string DateFormatExact(object value, string sourceFormt, string returnFormt)
        {
            if (value == null || value == DBNull.Value || value.ToString() == "")
                return null;

            DateTime val;
            if (DateTime.TryParseExact(value.ToString(), sourceFormt, CultureInfo.InvariantCulture, DateTimeStyles.None, out val))
                return val.ToString(returnFormt);
            return null;
        }
        public static DateTime DateFormatExact(object value, string sourceFormt, DateTime defaultValue)
        {
            if (value == null || value == DBNull.Value || value.ToString() == "")
                return defaultValue;

            DateTime val;
            if (DateTime.TryParseExact(value.ToString(), sourceFormt, CultureInfo.InvariantCulture, DateTimeStyles.None, out val))
                return val;
            return defaultValue;
        }
        public static DateTime? DateFormatExact(object value, string sourceFormt)
        {
            if (value == null || value == DBNull.Value || value.ToString() == "")
                return (DateTime?)null;
            DateTime val;
            if (DateTime.TryParseExact(value.ToString(), sourceFormt, CultureInfo.InvariantCulture, DateTimeStyles.None, out val))
                return (DateTime?)val;
            return (DateTime?)null;
        }
        /*
       public static string[] IsoDateFormats
       {
           get
           {
               string[] isoFormats = { "yyyy-MM-dd hh:mm:ss tt", "yyyy-MM-dd hh:mm:ss", "yyyy-MM-dd hh:mm", "yyyy-MM-dd" };
               return isoFormats;
           }
       }
       public static string[] StdDateFormats
       {
           get
           {
               string[] stdFormats = {
               "d/M/yyyy h:mm:ss tt", "d/M/yyyy h:mm tt",
                        "dd/MM/yyyy hh:mm:ss", "d/M/yyyy h:mm:ss",
                        "d/M/yyyy hh:mm tt", "d/M/yyyy hh tt",
                        "d/M/yyyy h:mm", "d/M/yyyy h:mm",
                        "dd/MM/yyyy hh:mm", "dd/M/yyyy hh:mm"};
               return stdFormats;
           }
       }
       public static string[] UsDateFormats
       {
           get
           {
               string[] usFormats = {"M/d/yyyy h:mm:ss tt", "M/d/yyyy h:mm tt",
                        "MM/dd/yyyy hh:mm:ss", "M/d/yyyy h:mm:ss",
                        "M/d/yyyy hh:mm tt", "M/d/yyyy hh tt",
                        "M/d/yyyy h:mm", "M/d/yyyy h:mm",
                        "MM/dd/yyyy hh:mm", "M/dd/yyyy hh:mm"};
               return usFormats;
           }
       }

       public static DateTime? DateTimeFormatExact(object value)
       {
           if (value == null || value == DBNull.Value || value.ToString() == "")
               return (DateTime?)null;

           DateTime val;
           if (DateTime.TryParseExact(value.ToString(), StdDateFormats, Types.GetCultureInfo(), DateTimeStyles.None, out val))
               return (DateTime?)val;
           return (DateTime?)null;
       }

       public static DateTime ParseDateTime(object value, DateTime defaultValue)
       {

           if (value == null || value == DBNull.Value || value.ToString() == "")
               return defaultValue;
           string str = value.ToString();

           try
           {
               int arg1 = 0;
               int arg2 = 0;
               int arg3 = 0;

               int h = 0;
               int m = 0;
               int s = 0;


               string[] parts = str.Split(new string[] { " ", "T" }, StringSplitOptions.RemoveEmptyEntries);

               if (parts.Length > 0)
               {
                   string[] args = parts[0].Split('/', '-');
                   arg1 = RemoveLeadingZero(args[0]);
                   arg2 = RemoveLeadingZero(args[1]);
                   arg3 = RemoveLeadingZero(args[2]);
               }

               if (parts.Length > 1)
               {

                   string[] args = parts[1].Split(':', '.');

                   if (args.Length > 0)
                       h = RemoveLeadingZero(args[0]);
                   if (args.Length > 1)
                       m = RemoveLeadingZero(args[1]);
                   if (args.Length > 2)
                       s = RemoveLeadingZero(args[2]);
               }

               if (parts.Length > 2)
               {

                   //[AM|PM|am|pm]
                   switch (parts[2])
                   {
                       case "PM":
                       case "pm":
                           if (h < 12)
                               h = h + 12;
                           break;
                   }
               }

               if (str.IndexOf('-') > 0)
                   return new DateTime(arg1, arg2, arg3, h, m, s);
               else
                   return new DateTime(arg3, arg2, arg1, h, m, s);
           }
           catch
           {
               return defaultValue;
           }
       }
       public static DateTime? ParseDateTime(object value)
       {

           if (value == null || value == DBNull.Value || value.ToString() == "")
               return (DateTime?)null;
           string str = value.ToString();

           try
           {
               int arg1 = 0;
               int arg2 = 0;
               int arg3 = 0;

               int h = 0;
               int m = 0;
               int s = 0;


               string[] parts = str.Split(new string[] { " ", "T" }, StringSplitOptions.RemoveEmptyEntries);

               if (parts.Length > 0)
               {
                   string[] args = parts[0].Split('/', '-');
                   arg1 = RemoveLeadingZero(args[0]);
                   arg2 = RemoveLeadingZero(args[1]);
                   arg3 = RemoveLeadingZero(args[2]);
               }

               if (parts.Length > 1)
               {

                   string[] args = parts[1].Split(':', '.');

                   if (args.Length > 0)
                       h = RemoveLeadingZero(args[0]);
                   if (args.Length > 1)
                       m = RemoveLeadingZero(args[1]);
                   if (args.Length > 2)
                       s = RemoveLeadingZero(args[2]);
               }

               if (parts.Length > 2)
               {

                   //[AM|PM|am|pm]
                   switch (parts[2])
                   {
                       case "PM":
                       case "pm":
                           if (h < 12)
                               h = h + 12;
                           break;
                   }
               }

               if (str.IndexOf('-') > 0)
                   return new DateTime(arg1, arg2, arg3, h, m, s);
               else
                   return new DateTime(arg3, arg2, arg1, h, m, s);
           }
           catch
           {
               return null;
           }
       }
       */
        public static object ParseDateTimeObject(object value, bool isNullable, DateTime? defaultValue=null)
        {

            if (value == null || value == DBNull.Value || value.ToString() == "")
                return defaultValue;
            string str = value.ToString();
            object returnVal = null;
            try
            {
                int arg1 = 0;
                int arg2 = 0;
                int arg3 = 0;

                int h = 0;
                int m = 0;
                int s = 0;

                string[] parts = str.Split(new string[] { " ", "T" }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length > 0)
                {
                    string[] args = parts[0].Split('/', '-');
                    arg1 = RemoveLeadingZero(args[0]);
                    arg2 = RemoveLeadingZero(args[1]);
                    arg3 = RemoveLeadingZero(args[2]);
                }

                if (parts.Length > 1)
                {

                    string[] args = parts[1].Split(':', '.');

                    if (args.Length > 0)
                        h = RemoveLeadingZero(args[0]);
                    if (args.Length > 1)
                        m = RemoveLeadingZero(args[1]);
                    if (args.Length > 2)
                        s = RemoveLeadingZero(args[2]);
                }

                if (parts.Length > 2)
                {

                    //[AM|PM|am|pm]
                    switch (parts[2])
                    {
                        case "PM":
                        case "pm":
                            if (h < 12)
                                h = h + 12;
                            break;
                    }
                }

                if (str.IndexOf('-') > 0)
                    returnVal = new DateTime(arg1, arg2, arg3, h, m, s);
                else
                    returnVal = new DateTime(arg3, arg2, arg1, h, m, s);

            }
            catch
            {
                returnVal = (isNullable) ? defaultValue: (defaultValue!=null && defaultValue.HasValue) ? defaultValue.Value : Types.MinDate;
            }

            if(isNullable)
                return (DateTime?)returnVal;
            else
                return (DateTime)returnVal;
        }

        public static DateTime ToDateTime(string value, DateFormat format)
        {
            return ParseDateTime(value, DateTime.Now, Formatter.ToDateFormat(format));
        }
        public static DateTime ToDateTime(string value, DateTime defaultValue, DateFormat format)
        {
            return ParseDateTime(value, defaultValue, Formatter.ToDateFormat(format));
        }
        public static DateTime ParseDateTime(string value, DateTime defaultValue, string format)
        {
            if (value == null || value == DBNull.Value.ToString() || value == "")
                return defaultValue;

            DateTime val;
            if (DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out val))
                return val;
            return defaultValue;
        }

        //public static DateTime DateFromString(string Value)
        //{
        //    return ParseDateTime(Value, Types.GetCultureInfo(), DateTime.Now);
        //}
        public static DateTime ToLocalDateTime(string Value)
        {
            if (Value == null || Value == DBNull.Value.ToString())
                return DateTime.Now;
            return ParseDateTime(Value, Formatter.LocalCulture, DateTime.Now);
        }
        public static DateTime ToDateTime(string Value, CultureInfo culture)
        {
            if (Value == null || Value == DBNull.Value.ToString())
                return DateTime.Now;
            return ParseDateTime(Value, culture, DateTime.Now);
        }

        public static DateTime ToDateTime(string Value)
        {
            if (Value == null || Value == DBNull.Value.ToString())
                return DateTime.Now;
            return ParseDateTime(Value, DateTime.Now);
        }
        public static DateTime ToDateTime(string Value, DateTime defaultValue)
        {
            if (Value == null || Value == DBNull.Value.ToString())
                return defaultValue;
            return ParseDateTime(Value, defaultValue);
        }
        public static DateTime ToDateTime(object Value)
        {
            if (Value == null || Value == DBNull.Value)
                return DateTime.Now;
            return ParseDateTime(Value.ToString(), DateTime.Now);
        }
        public static DateTime ToDateTime(object Value, DateTime defaultValue)
        {
            if (Value == null || Value == DBNull.Value)
                return defaultValue;
            return ParseDateTime(Value.ToString(), defaultValue);
        }

        public static DateTime ToDateTime(string Value, DateTimeFormatInfo dtf, DateTime defaultValue)
        {
            DateTime val;
            if (DateTime.TryParse(Value, dtf, DateTimeStyles.None, out val))
                return val;
            return defaultValue;
        }
        //new
        static DateTime ParseDateTime(string Value, DateTime defaultValue)
        {
            DateTime val;
            if (DateTime.TryParse(Value, out val))
                return val;
            return defaultValue;
        }
        //new
        static DateTime ParseDateTime(string Value, CultureInfo culture, DateTime defaultValue)
        {
            if (culture == null)
                culture = Types.GetCultureInfo();
            DateTime val;
            if (DateTime.TryParse(Value, culture, DateTimeStyles.None, out val))
                return val;
            return defaultValue;
        }
        /*
        public static DateTime ToDateTime(object Value, DateTime defaultValue)
        {
            if (Value == null || Value == DBNull.Value)
                return defaultValue;
            return ToDateTime(Value.ToString(), defaultValue);
        }

        public static DateTime ToDateTime(string Value, DateTime defaultValue)
        {
            DateTime val;
            if (DateTime.TryParse(Value, out val))
                return val;
            return defaultValue;
        }

        public static DateTime ToDateTime(string Value, DateTimeFormatInfo dtf, DateTime defaultValue)
        {
            DateTime val;
            if (DateTime.TryParse(Value, dtf, DateTimeStyles.None, out val))
                return val;
            return defaultValue;
        }

        public static DateTime ToDateTime(string Value, CultureInfo culture, DateTime defaultValue)
        {
            DateTime val;
            if (DateTime.TryParse(Value, culture, DateTimeStyles.None, out val))
                return val;
            return defaultValue;
        }

        public static DateTime ToDateTime(string Value, DateTimeFormatInfo dtf)
        {
            return ToDateTime(Value, dtf, DateTime.Now);
        }

        public static DateTime ToDateTime(string Value, CultureInfo culture)
        {
            return ToDateTime(Value, culture, DateTime.Now);
        }
       
        /// <summary>
        /// Get string date and return formated string date ,on exception return defult value
        /// </summary>
        /// <param name="s">string dateTime</param>
        /// <param name="format">format</param>
        /// <param name="defaultValue">value to return if invalid cast exception</param>
        /// <returns>String Date Time formated</returns>
        public static string FormatDate(string s, string format, DateTime defaultValue)
        {
            try
            {
                return ToDateTime(s, defaultValue).ToString(format);
            }
            catch (Exception)
            {
                return defaultValue.ToString(format);
            }
        }
         */
        /// <summary>
        /// Get string date and return formated string date ,on exception return defult value
        /// </summary>
        /// <param name="s">string dateTime</param>
        /// <param name="format">format</param>
        /// <param name="defaultValue">string value to return if invalid cast exception</param>
        /// <returns>String Date Time formated</returns>
        public static string FormatDate(string s, string format, string defaultValue)
        {
            DateTime val;
            try
            {
                if (string.IsNullOrEmpty(s))
                    return defaultValue;
                if (DateTime.TryParse(s, out val))
                    return val.ToString(format);
                return defaultValue;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        //public static bool IsDate(string s)
        //{
        //    DateTime val;
        //    if (string.IsNullOrEmpty(s) || s.Length != 10)
        //        return false;
        //    return DateTime.TryParse(s, out val);
        //}

        public static bool IsDate(string s)
        {
            return IsDate(s, CultureInfo.CurrentCulture.Name);
        }
        public static bool IsDate(string s, string cultureName)
        {
            DateTime val;
            if (string.IsNullOrEmpty(s) || s.Length != 10)
                return false;
            return DateTime.TryParse(s, DateFormatInfo(cultureName), System.Globalization.DateTimeStyles.AssumeLocal, out val);
        }

        public static System.Globalization.DateTimeFormatInfo DateFormatInfo(string cultureName)//"he-IL"
        {
            return new CultureInfo(cultureName, false).DateTimeFormat;
        }

        public static bool IsDateTime(string s)
        {
            return IsDateTime(s, CultureInfo.CurrentCulture.Name);
        }
        public static bool IsDateTime(string s, string cultureName)
        {
            DateTime val;
            if (string.IsNullOrEmpty(s) || s.Length < 10)
                return false;
            //return DateTime.TryParse(s, out val);
            return DateTime.TryParse(s, DateFormatInfo(cultureName), System.Globalization.DateTimeStyles.None, out val);
        }

        public static bool IsDateTime(object o)
        {
            if (o == null)
                return false;
            if (o is DateTime)
                return true;
            return IsDateTime(o.ToString());
        }

        public static int DateTimeToInt(DateTime value)
        {
            return int.Parse(value.ToString("yyyyMMdd"));
        }

        /*
        public static int TodayInt()
        {
            return int.Parse(DateTime.Today.ToString("yyyyMMdd"));
        }


        public static string DateToString(DateTime value, string format)
        {
            return value.ToString(format, CultureInfo.CurrentCulture);
        }

        public static string DateToString(DateTime value)
        {
            string sep = System.Globalization.DateTimeFormatInfo.CurrentInfo.DateSeparator;
            string format = String.Empty;

            DateTime t = new DateTime(2000, 2, 1);
            string tStr = t.ToShortDateString();
            if (tStr.IndexOf("1") > tStr.IndexOf("2"))
            {
                format = "MM" + sep + "dd" + sep + "yyyy";
            }
            else
            {
                format = "dd" + sep + "MM" + sep + "yyyy";
            }

            return value.ToString(format, CultureInfo.CurrentCulture);
        }

        public static DateTime DateValue(string StringDate)
        {
            DateTime time2 = DateFromString(StringDate);
            return time2.Date;
        }



        public static DateTime DateStart(DateTime d)
        {
            return new DateTime(d.Year, d.Month, d.Day, 0, 0, 0);
        }
        public static DateTime DateEnd(DateTime d)
        {
            return new DateTime(d.Year, d.Month, d.Day, 23, 59, 59);
        }
        */
        #endregion

        #region Bytes

        public static string BytesToHexString(byte[] byteArray)
        {
            StringBuilder sb = new StringBuilder(40);
            foreach (byte bValue in byteArray)
            {
                sb.AppendFormat(CultureInfo.CurrentCulture, bValue.ToString("x2", CultureInfo.CurrentCulture).ToUpper(CultureInfo.CurrentCulture));
            }
            return sb.ToString();
        }


        public static byte[] BytesFromString(string stringValue)
        {
            return (new UnicodeEncoding()).GetBytes(stringValue);
        }

        public static string BytesToString(byte[] byteArray)
        {
            return (new UnicodeEncoding()).GetString(byteArray);
        }

        #endregion

        #region Formats

        public struct BoolRange
        {
            public object T;
            public object F;
        }


        #endregion

        #region enum

        public static object ParseEnum(Type type, object value, object defaultValue)
        {
            try
            {
                if (value == null)
                    return defaultValue;
                if (!Enum.IsDefined(type, value))
                    return defaultValue;
                return Enum.Parse(type, value.ToString(), true);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static int ParseEnum(Type type, object value, int defaultValue)
        {
            try
            {
                if (value == null)
                    return defaultValue;
                if (!Enum.IsDefined(type, value))
                    return defaultValue;
                return (int)Enum.Parse(type, value.ToString(), true);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static T ToEnum<T>(object value, T defaultValue)
        {
            try
            {
                if (value == null)
                    return defaultValue;
                if (!Enum.IsDefined(typeof(T), value))
                    return defaultValue;
                return (T)Enum.Parse(typeof(T), value.ToString(), true);
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
        public static T ToEnum<T>(object value)
        {
            if (value == null)
                throw new ArgumentNullException("Enum.Parse value");
            if (!Enum.IsDefined(typeof(T), value))
                throw new ArgumentException("Enum not defined");
            return (T)Enum.Parse(typeof(T), value.ToString(), true);
        }

        #endregion

        #region Info


        private static System.Globalization.CultureInfo GetCulture(System.Globalization.CultureInfo cultureInfo)
        {
            if (cultureInfo == null)
                return System.Globalization.CultureInfo.CurrentCulture;
            else
                return cultureInfo;
        }
        public static string GetTypeName(object o)
        {
            return o==null ? null: o.GetType().FullName;
        }
       
        public static CultureInfo GetCultureInfo()
        {
            return Thread.CurrentThread.CurrentCulture;
        }

        public static DateTimeFormatInfo GetDateTimeFormatInfo()
        {
            return Thread.CurrentThread.CurrentCulture.DateTimeFormat;
        }

        internal static Encoding GetFileIOEncoding()
        {
            return Encoding.Default;
        }

        internal static int GetLocaleCodePage()
        {
            return Thread.CurrentThread.CurrentCulture.TextInfo.ANSICodePage;
        }

        public static bool IsGuid(string candidate, out Guid output)
        {
            Regex isGuid = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);

            bool isValid = false;

            output = Guid.Empty;

            if (candidate != null)
            {
                if (isGuid.IsMatch(candidate))
                {
                    output = new Guid(candidate);
                    isValid = true;
                }
            }
            return isValid;
        }

        public static bool IsGuid(string guid)
        {
            Regex isGuid = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);

            bool isValid = false;

            if (guid != null)
            {
                if (isGuid.IsMatch(guid))
                {
                    isValid = true;
                }
            }
            return isValid;
        }


        /// <summary>
        /// IsValidTime
        /// </summary>
        /// <param name="time">12:30</param>
        /// <returns></returns>
        public static bool IsValidTime(string time)
        {
            if (string.IsNullOrEmpty(time))
                return false;
            string[] args = time.Split(':');
            if (args == null || args.Length != 2)
                return false;
            int h = Nistec.Types.ToInt(args[0], -1);
            int m = Nistec.Types.ToInt(args[1], -1);
            if (h < 0 || m < 0)
                return false;
            return (h >= 0 && h <= 23 && m >= 0 && m <= 59);

        }
        public static bool IsValidTime(string time, ref int[] value)
        {
            if (string.IsNullOrEmpty(time))
                return false;
            string[] args = time.Split(':');
            if (args == null || args.Length != 2)
                return false;
            int h = Nistec.Types.ToInt(args[0], -1);
            int m = Nistec.Types.ToInt(args[1], -1);
            if (h < 0 || m < 0)
                return false;
            if (h >= 0 && h <= 23 && m >= 0 && m <= 59)
            {
                value = new int[] { h, m };
                return true;
            }
            return false;

        }
        /// <summary>
        /// IsValidMonthDay
        /// </summary>
        /// <param name="date">22/11</param>
        /// <param name="value">22/11</param>
        /// <returns></returns>
        public static bool IsValidMonthDay(string date, ref int[] value)
        {
            if (string.IsNullOrEmpty(date))
                return false;
            string[] args = date.Split('/');
            if (args == null || args.Length != 2)
                return false;
            int d = Nistec.Types.ToInt(args[0], -1);
            int m = Nistec.Types.ToInt(args[1], -1);
            if (m < 0 || d < 0)
                return false;
            if (m >= 0 && m <= 12 && d >= 0 && d <= 31)
            {
                value = new int[] { d, m };
                return true;
            }
            return false;
        }

        public static bool? IsTrue(object value, bool? nullValue = null)
        {
            if (value == null)
                return nullValue;
            switch (value.ToString().ToLower())
            {
                case "1":
                case "on":
                case "true":
                    return true;
                case "0":
                case "off":
                case "false":
                    return false;
                default:
                    return nullValue;
            }
        }


        public static bool IsBool(object Expression)
        {
            if (Expression != null)
            {
                if (Expression is bool)
                {
                    return true;
                }
                if (Expression is string)
                {
                    string val = (string)Expression;
                    if (!(val.ToLower().Contains("false") || val.ToLower().Contains("true")))
                    {
                        return false;
                    }
                    try
                    {
                        bool res = bool.Parse((string)Expression);
                        return true;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return false;
        }
        public static bool IsDBNull(object Expression)
        {
            if ((Expression != null) && (Expression == DBNull.Value))
            {
                return true;
            }
            return false;
        }

        public static Boolean IsNumeric(object input)
        {
            if (input == null || input == DBNull.Value)
                return false;
            Double temp;
            return Double.TryParse(input.ToString(), out temp);
        }
        public static Boolean IsNumeric(String input)
        {
            Double temp;
            return Double.TryParse(input, out temp);
        }

        public static Boolean IsNumeric(String input, NumberStyles numberStyle)
        {
            Double temp;
            return Double.TryParse(input, numberStyle, CultureInfo.CurrentCulture, out temp);
        }

        internal static bool IsNumericTypeCode(TypeCode TypCode)
        {
            switch (TypCode)
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;
            }
            return false;
        }

        public static bool IsNumber(string s)
        {
            try
            {
                double result = 0;
                return Double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.CurrentInfo, out result);
            }
            catch
            {
                return false;
            }
        }
        public static bool IsNumber(string s, System.Globalization.NumberStyles style)
        {
            try
            {
                double result = 0;
                return Double.TryParse(s, style, System.Globalization.NumberFormatInfo.CurrentInfo, out result);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsNumber(object obj)
        {
            if (obj == null)
                return false;
            return IsNumber(obj.ToString());
        }

        public static bool IsNumericType(Type type)
        {

            if (type == typeof(float)
                || type == typeof(double)
                || type == typeof(byte)
                || type == typeof(short)
                || type == typeof(int)
                || type == typeof(long)
                || type == typeof(decimal)
                || type == typeof(Int16)
                || type == typeof(Int32)
                || type == typeof(Int64)
                || type == typeof(UInt16)
                || type == typeof(UInt32)
                || type == typeof(UInt64)
                || type == typeof(uint))
            {
                return true;
            }

            return false;
        }
        #endregion

        #region Nullable

        public static DateTime? ToNullableDate(string value, DateFormat format)
        {
            return ParseNullableDate(value, Formatter.ToDateFormat(format));
        }
        public static DateTime? ParseNullableDate(string value, string format)
        {
            if (value == null || value == DBNull.Value.ToString() || value == "")
                return (DateTime?)null;

            DateTime val;
            if (DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out val))
                return (DateTime?)val;
            return (DateTime?)null;
        }
        public static string ToNullableDateTimeformat(object value, string culture = "he-IL", DateFormat format= DateFormat.sql)
        {
            if (value == null || value == DBNull.Value || value.ToString() == "")
                return null;
            DateTime val;
            if (DateTime.TryParse(value.ToString(), new CultureInfo(culture, false).DateTimeFormat, DateTimeStyles.AssumeLocal, out val))
            {
                if (((DateTime?)val).HasValue)
                    return ((DateTime?)val).Value.ToString(Formatter.ToDateFormat(format));
            }
            return null;
        }

        /*
        public static string ToNullableDateTimeformat(object value, string culture = "he-IL", string format = "yyyy-MM-dd hh:mm:ss")
        {
            if (value == null || value == DBNull.Value || value.ToString() == "")
                return null;
            DateTime val;
            if (DateTime.TryParse(value.ToString(), new CultureInfo(culture, false).DateTimeFormat, DateTimeStyles.AssumeLocal, out val))
            {
                if (((DateTime?)val).HasValue)
                    return ((DateTime?)val).Value.ToString(format);
            }
            return null;
        }
        public static DateTime? ToNullableDateIso(object value)
        {
            if (value == null || value == DBNull.Value || value.ToString() == "")
                return (DateTime?)null;

            DateTime val;
            if (DateTime.TryParseExact(value.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out val))
                return (DateTime?)val;
            return (DateTime?)null;
        }
        public static DateTime? ToNullableDateTimeIso(object value)
        {
            if (value == null || value == DBNull.Value || value.ToString() == "")
                return (DateTime?)null;

            DateTime val;
            if (DateTime.TryParseExact(value.ToString(), "yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out val))
                return (DateTime?)val;
            return (DateTime?)null;
        }
        */

        public static DateTime? ToNullableDate(object value, string culture = "he-IL")
        {
            if (value == null || value == DBNull.Value || value.ToString() == "")
                return (DateTime?)null;
            DateTime val;
            if (DateTime.TryParse(value.ToString(), new CultureInfo(culture, false).DateTimeFormat, DateTimeStyles.AssumeLocal, out val))
                return (DateTime?)val;
            return (DateTime?)null;
        }

        public static int? ToNullableInt(object value)
        {
            if (value == null || value == DBNull.Value || value.ToString() == "")
                return (int?)null;
            if (value is int?)
                return (int?)value;
            int val;
            if (int.TryParse(value.ToString(), out val))
                return (int?)val;
            return (int?)null;
        }
        public static bool? ToNullableBool(object value)
        {
            try
            {
                if (value == null || value == DBNull.Value || value.ToString() == "")
                    return (bool?)null;
                if (value is bool?)
                    return (bool?)value;
                bool val;
                string str = value.ToString();

                if (str == "0" || str == "off")
                    return (bool?)false;
                if (str == "1" || str == "on")
                    return (bool?)true;
                if (Boolean.TryParse(str, out val))
                    return Boolean.Parse(str);
                return (bool?)null;
            }
            catch
            {
                return (bool?)null;
            }
        }

        internal static Type GetNonNullableType(this Type type)
        {
            if (IsNullableType(type))
            {
                return type.GetGenericArguments()[0];
            }
            return type;
        }

        internal static Type GetNullableType(Type type)
        {
            if (type.IsValueType && !IsNullableType(type))
            {
                return typeof(Nullable<>).MakeGenericType(type);
            }
            return type;
        }

        internal static bool IsNullableType(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        #endregion

        #region Is Type

        internal static bool IsBool(Type type)
        {
            return GetNonNullableType(type) == typeof(bool);
        }

        internal static bool IsNumeric(Type type)
        {
            type = GetNonNullableType(type);
            if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Char:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Double:
                    case TypeCode.Single:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        return true;
                }
            }
            return false;
        }

        internal static bool IsInteger(Type type)
        {
            type = GetNonNullableType(type);
            if (type.IsEnum)
            {
                return false;
            }
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }


        internal static bool IsArithmetic(Type type)
        {
            type = GetNonNullableType(type);
            if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Double:
                    case TypeCode.Single:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        return true;
                }
            }
            return false;
        }

        internal static bool IsUnsignedInt(Type type)
        {
            type = GetNonNullableType(type);
            if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        return true;
                }
            }
            return false;
        }

        internal static bool IsIntegerOrBool(Type type)
        {
            type = GetNonNullableType(type);
            if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Int64:
                    case TypeCode.Int32:
                    case TypeCode.Int16:
                    case TypeCode.UInt64:
                    case TypeCode.UInt32:
                    case TypeCode.UInt16:
                    case TypeCode.Boolean:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                        return true;
                }
            }
            return false;
        }

        private static bool IsDelegate(Type t)
        {
            return t.IsSubclassOf(typeof(System.MulticastDelegate));
        }

        internal static bool IsConvertible(Type type)
        {
            type = GetNonNullableType(type);
            if (type.IsEnum)
            {
                return true;
            }
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Char:
                    return true;
                default:
                    return false;
            }
        }
        internal static Type FindGenericType(Type definition, Type type)
        {
            while (type != null && type != typeof(object))
            {
                if (type.IsGenericType && AreEquivalent(type.GetGenericTypeDefinition(), definition))
                {
                    return type;
                }
                if (definition.IsInterface)
                {
                    foreach (Type itype in type.GetInterfaces())
                    {
                        Type found = FindGenericType(definition, itype);
                        if (found != null)
                            return found;
                    }
                }
                type = type.BaseType;
            }
            return null;
        }

        internal static bool IsUnsigned(Type type)
        {
            type = GetNonNullableType(type);
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.Char:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        internal static bool IsFloatingPoint(Type type)
        {
            type = GetNonNullableType(type);
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Single:
                case TypeCode.Double:
                    return true;
                default:
                    return false;
            }
        }

        internal static bool AreEquivalent(Type t1, Type t2)
        {
            return t1 == t2 || t1.IsEquivalentTo(t2);
        }

        internal static bool AreReferenceAssignable(Type dest, Type src)
        {
            // WARNING: This actually implements "Is this identity assignable and/or reference assignable?"
            if (AreEquivalent(dest, src))
            {
                return true;
            }
            if (!dest.IsValueType && !src.IsValueType && dest.IsAssignableFrom(src))
            {
                return true;
            }
            return false;
        }
        #endregion

        #region Is Serialize

        public static bool IsISerializable(Type type)
        {
            if (typeof(ISerializable).IsAssignableFrom(type))
            {
                return true;
            }
            return false;
        }

        public static bool IsPrimitiveOrString(Type type)
        {
            if (type == typeof(string))
            {
                return true;
            }

            return IsPrimitive(type);
        }

        /// <summary>
        ///   Is the simple type (string, DateTime, TimeSpan, Decimal, Enumeration or other primitive type)
        /// </summary>
        /// <param name = "type"></param>
        /// <returns></returns>
        public static bool IsPrimitive(Type type)
        {
            if (type == typeof(DateTime))
            {
                return true;
            }
            if (type == typeof(TimeSpan))
            {
                return true;
            }
            if (type == typeof(Decimal))
            {
                // new since the version 2
                return true;
            }
            if (type == typeof(Guid))
            {
                // new since the version 2.8
                return true;
            }


            return type.IsPrimitive;
        }

        /// <summary>
        ///   Is the simple type (string, DateTime, TimeSpan, Decimal, Enumeration or other primitive type)
        /// </summary>
        /// <param name = "type"></param>
        /// <returns></returns>
        public static bool IsSimple(Type type)
        {
            if (type == typeof(string))
            {
                return true;
            }
            if (type == typeof(Type) || type.IsSubclassOf(typeof(Type)))
            {
                // new since v.2.11
                return true;
            }
            if (type.IsEnum)
            {
                return true;
            }
            if (type == typeof(byte[]))
            {
                // since v.2.16 is byte[] a simple type
                return true;
            }

            return IsPrimitive(type);
        }

        /// <summary>
        ///   Is type an IEnumerable
        /// </summary>
        /// <param name = "type"></param>
        /// <returns></returns>
        public static bool IsEnumerable(Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        /// <summary>
        ///   Is type an Generic IEnumerable
        /// </summary>
        /// <param name = "type"></param>
        /// <returns></returns>
        public static bool IsGenericEnumerable(Type type)
        {
            return type.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(type);
        }

        /// <summary>
        ///   Is type ICollection
        /// </summary>
        /// <param name = "type"></param>
        /// <returns></returns>
        public static bool IsCollection(Type type)
        {
            return typeof(ICollection).IsAssignableFrom(type);
        }

        /// <summary>
        ///   Is type IDictionary
        /// </summary>
        /// <param name = "type"></param>
        /// <param name = "includeGeneric"></param>
        /// <returns></returns>
        public static bool IsAssignableFromDictionary(Type type, bool includeGeneric = false)
        {
            if (includeGeneric)
                return (type.IsGenericType || (type.BaseType != null && type.BaseType.IsGenericType)) && typeof(IDictionary).IsAssignableFrom(type);
            return typeof(IDictionary).IsAssignableFrom(type);
        }

        /// <summary>
        ///   Is type Is Generic Dictionary{string,object}
        /// </summary>
        /// <param name = "type"></param>
        /// <returns></returns>
        public static bool IsStringObjectDictionary(Type type)
        {
            return type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Dictionary<string, object>) || type.GetGenericTypeDefinition() == typeof(ConcurrentDictionary<string, object>));
        }

        /// <summary>
        ///   Is type Is Generic Dictionary
        /// </summary>
        /// <param name = "type"></param>
        /// <returns></returns>
        public static bool IsGenericDictionary(Type type)
        {
            return type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Dictionary<,>) || type.GetGenericTypeDefinition() == typeof(ConcurrentDictionary<,>));
        }

        public static bool IsGenericKeyStringDictionary(Type type)
        {
            return type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Dictionary<,>) || type.GetGenericTypeDefinition() == typeof(ConcurrentDictionary<,>)) && type.GetGenericArguments()[0] == typeof(string);
            //return type.IsGenericType && typeof(IDictionary<,>).IsAssignableFrom(type) && type.GetGenericArguments()[0] == typeof(string);
        }

        public static bool IsKeyValuePair(Type type)
        {
            if (type.IsGenericType)
            {
                return type.GetGenericTypeDefinition() != null ? type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>) : false;
            }
            return false;
        }
        public static bool IsKeyValuePaire<TKey, TValue>(Type type)
        {
            return type == typeof(KeyValuePair<TKey, TValue>);
        }


        /// <summary>
        ///   Is type Is Generic List
        /// </summary>
        /// <param name = "type"></param>
        /// <returns></returns>
        public static bool IsGenericList(Type type)
        {

            return (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>)));

            //return type.IsGenericType && typeof(IList<>).IsAssignableFrom(type);
        }

        /// <summary>
        ///   Is type Is Generic Hashset
        /// </summary>
        /// <param name = "type"></param>
        /// <returns></returns>
        public static bool IsGenericSet(Type type)
        {
            return type.IsGenericType && typeof(ISet<>).IsAssignableFrom(type);
        }

        /// <summary>
        ///   Is it array? It does not matter if singledimensional or multidimensional
        /// </summary>
        /// <param name = "type"></param>
        /// <returns></returns>
        public static bool IsArray(Type type)
        {
            return type.IsArray;
        }

        public static bool IsType(Type type)
        {
            return type == typeof(Type) || type.IsSubclassOf(typeof(Type));
        }
        public static bool IsDataTable(Type type)
        {
            return type == typeof(DataTable);
        }
        public static bool IsDataSet(Type type)
        {
            return type == typeof(DataSet);
        }
        public static bool IsStream(Type type)
        {
            return typeof(Stream).IsAssignableFrom(type);
        }
        public static bool IsXmlDocument(Type type)
        {
            return typeof(XmlDocument) == type;
        }
        public static bool IsXmlNode(Type type)
        {
            return typeof(XmlNode).IsAssignableFrom(type);
        }
        public static bool IsListKeyValue<TKey, TValue>(Type type)
        {
            return typeof(IKeyValue).IsAssignableFrom(type)
                || type == typeof(List<KeyValuePair<TKey, TValue>>);
        }

        public static bool IsISerialEntity(Type type)
        {
            return typeof(ISerialEntity).IsAssignableFrom(type);
        }
        public static bool IsISerialContext(Type type)
        {
            return typeof(ISerialContext).IsAssignableFrom(type);
        }
        public static bool IsISerialJson(Type type)
        {
            return typeof(ISerialJson).IsAssignableFrom(type);
        }


        #endregion

        #region extended

        public static int RemoveLeadingZero(string value)
        {
            //string str = Regx.RegexReplace("^0+(?!$)", value, "");
            int val;
            int.TryParse(value.TrimStart('0'), out val);
            return val;
        }

        public static object StringToObject(string type, string value)
        {
            if (type == null)
                return value;
            switch (type.ToLower())
            {
                case "boolean":
                case "bool":
                    return ToBool(value, false);
                case "byte":
                case "sbyte":
                    return ToByte(value, (byte)0);
                case "int16":
                case "int32":
                case "int":
                    return ToInt(value);
                case "int64":
                case "long":
                    return ToLong(value);
                case "uint16":
                case "uint32":
                case "uint":
                    return (uint)ToInt(value);
                case "uint64":
                case "ulong":
                    return (ulong)ToLong(value);
                case "single":
                case "float":
                    return ToFloat(value, 0f);
                case "double":
                    return ToDouble(value, 0);
                case "datetime":
                case "date":
                    return ToDateTime(value);
                case "decimal":
                    return ToDecimal(value, 0);
                case "guid":
                    return ToGuid(value);
                default:
                    return value;
            }
        }
        #endregion
    }
	#endregion
}
