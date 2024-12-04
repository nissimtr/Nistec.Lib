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
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Globalization;

#pragma warning disable CS1591
namespace Nistec
{ 

	public class Regx
	{

        public static string[] ParseKeyValue(string Txt)
        {
            bool isMatch;
            string[] res = ParseKeyValue(Txt, out isMatch);
            if (!isMatch)
                res[0] = Txt;
            return res;
        }

        public static string[] ParseKeyValue(string Txt, out bool IsMatch)
        {
            IsMatch = false;
            string [] res=new string[2];
            try
            {
                const string pattern = @"^\s*(?<Keyword>.*)=(?<Value>.*)";
                Regex regex = new Regex(pattern, RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);

                Match m = regex.Match(Txt);

                IsMatch = m.Success;
                if (m.Success)
                {
                    res[0] = m.Groups[1].Value;
                    res[1] = m.Groups[2].Value;
                }
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
            return res;
        }

        #region Validate and Replace

        public static int RegexMatchesCount(string pattern, string content)
        {
            Regex rg = new System.Text.RegularExpressions.Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            return rg.Matches(content).Count;
        }
        public static string RegexReplace(string pattern, string content, string replacement)
        {
            Regex rg = new System.Text.RegularExpressions.Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            return rg.Replace(content, replacement);
        }
        public static bool RegexValidate(string pattern, string value)
        {
            Regex rg = new System.Text.RegularExpressions.Regex(pattern);

            return rg.Match(value).Success;
        }
        public static bool RegexValidate(string pattern, string value, RegexOptions optrions)
        {
            Regex rg = new System.Text.RegularExpressions.Regex(pattern, optrions);

            return rg.Match(value).Success;
        }
        public static bool RegexValidateIgnoreCase(string pattern, string value)
        {
            Regex rg = new System.Text.RegularExpressions.Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

            return rg.Match(value).Success;
        }

        #endregion

        #region Regex Methods

        protected bool ValidateRegularExpression(string RegexPattern,string Text)
		{
			if (Text.Length > 0)
			{
				if (RegexPattern != string.Empty)
				{
					Regex regularExpr = new Regex(RegexPattern);
					return (regularExpr.IsMatch(Text));
				}
			}

			return false;
		}

		protected static int RegexMatches(string Txt,string Pattern)
		{
			Regex regex= new Regex(Pattern); 
			return regex.Matches(Txt).Count;  
		}

		protected static bool RegexIsMatch(string Txt,string Pattern)
		{
			Regex regex= new Regex(Pattern); 
			return regex.IsMatch (Txt);  
		}

		protected static string CleanInput(string strIn)
		{
			// Replace invalid characters with empty strings.
			return Regex.Replace(strIn, @"[^\w\.@-]", ""); 
		}

		#endregion

		#region Parsing decimal

		[Description("Convert string to Decimal if not validate return default value")]
		public static decimal ParseDecimal(string Txt,decimal defaultValue)
		{
			Regex regex= new Regex(RegexPattern.Number); 
			string num=CleanInput(Txt);  
	        if(!regex.IsMatch(num))
               return defaultValue;
			else
              return decimal.Parse (num);   
		}

		[Description("Convert string to Decimal if not validate return default value")]
		public static string ParseToDecimal(string Txt,decimal defaultValue,string format,int decimalPlaces)
		{
			Regex regex= new Regex(RegexPattern.Number); 
			string num=CleanInput(Txt);  
			if(!regex.IsMatch(num))
				return defaultValue.ToString (format + decimalPlaces.ToString(),CultureInfo.CurrentCulture);
			else
				return decimal.Parse (num).ToString (format + decimalPlaces.ToString(),CultureInfo.CurrentCulture);
		}

		[Description("Convert string to Decimal if not validate return default value")]
		public static string ParseToDecimal(string Txt,decimal defaultValue,string format)
		{
			Regex regex= new Regex(RegexPattern.Number); 
			string num=CleanInput(Txt);  
			if(!regex.IsMatch(num))
				return defaultValue.ToString (format ,CultureInfo.CurrentCulture);
			else
				return decimal.Parse (num).ToString (format ,CultureInfo.CurrentCulture);
		}

		[Description("Convert string to Decimal if not validate return default value")]
		public static string ParseToDecimal(string Txt,string defaultValue,string format,int decimalPlaces)
		{
			Regex regex= new Regex(RegexPattern.Number); 
			string num=CleanInput(Txt);  
			if(!regex.IsMatch(num))
				return defaultValue;
			else
				return decimal.Parse (num).ToString (format + decimalPlaces.ToString(),CultureInfo.CurrentCulture);
		}

		[Description("Convert string to Decimal if not validate return default value")]
		public static string ParseToDecimal(string Txt,string defaultValue,string format)
		{
			Regex regex= new Regex(RegexPattern.Number); 
			string num=CleanInput(Txt);  
			if(!regex.IsMatch(num))
				return defaultValue;
			else
				return decimal.Parse (num).ToString (format,CultureInfo.CurrentCulture);
		}

		[Description("Convert string to Decimal if not validate throw AppException")]
		public static string ParseToDecimal(string Txt,string format,int decimalPlaces)
		{
			Regex regex= new Regex(RegexPattern.Number); 
			string num=CleanInput(Txt);  
			if(!regex.IsMatch(num))
				throw new Exception ("Error Convert To Number");
			else
				return decimal.Parse (num).ToString (format + decimalPlaces.ToString(),CultureInfo.CurrentCulture);
		}

		[Description("Convert string to Decimal if not validate throw AppException")]
		public static string ParseToDecimal(string Txt,string format)
		{
			Regex regex= new Regex(RegexPattern.Number); 
			string num=CleanInput(Txt);  
			if(!regex.IsMatch(num))
				throw new Exception ("Error Convert To Number");
			else
				return decimal.Parse (num).ToString (format,CultureInfo.CurrentCulture);
		}

		#endregion

		#region Parsing Double

		[Description("Convert string to Double if not validate return default value")]
		public static double ParseDouble(string Txt,double defaultValue)
		{
			Regex regex= new Regex(RegexPattern.Number); 
			string num=CleanInput(Txt);  
			if(!regex.IsMatch(num))
				return defaultValue;
			else
				return double.Parse (num);   
		}

		[Description("Convert string to Double if not validate return default value")]
		public static string ParseToDouble(string Txt,double defaultValue,string format,int decimalPlaces)
		{
			Regex regex= new Regex(RegexPattern.Number); 
			string num=CleanInput(Txt);  
			if(!regex.IsMatch(num))
				return defaultValue.ToString (format + decimalPlaces.ToString(),CultureInfo.CurrentCulture);
			else
				return double.Parse (num).ToString (format + decimalPlaces.ToString(),CultureInfo.CurrentCulture);
		}

		[Description("Convert string to Double if not validate return default value")]
		public static string ParseToDouble(string Txt,double defaultValue,string format)
		{
			Regex regex= new Regex(RegexPattern.Number); 
			string num=CleanInput(Txt);  
			if(!regex.IsMatch(num))
				return defaultValue.ToString (format,CultureInfo.CurrentCulture);
			else
				return double.Parse (num).ToString (format,CultureInfo.CurrentCulture);
		}

		[Description("Convert string to Double if not validate return default value")]
		public static string ParseToDouble(string Txt,string defaultValue,string format,int decimalPlaces)
		{
			Regex regex= new Regex(RegexPattern.Number); 
			string num=CleanInput(Txt);  
			if(!regex.IsMatch(num))
				return defaultValue;
			else
				return double.Parse (num).ToString (format + decimalPlaces.ToString(),CultureInfo.CurrentCulture);
		}

		[Description("Convert string to Double if not validate return default value")]
		public static string ParseToDouble(string Txt,string defaultValue,string format)
		{
			Regex regex= new Regex(RegexPattern.Number); 
			string num=CleanInput(Txt);  
			if(!regex.IsMatch(num))
				return defaultValue;
			else
				return double.Parse (num).ToString (format,CultureInfo.CurrentCulture);
		}

		[Description("Convert string to Double if not validate throw AppException")]
		public static string ParseToDouble(string Txt,string format,int decimalPlaces)
		{
			Regex regex= new Regex(RegexPattern.Number); 
			string num=CleanInput(Txt);  
			if(!regex.IsMatch(num))
				throw new Exception ("Error Convert To Number");
			else
				return double.Parse (num).ToString (format + decimalPlaces.ToString(),CultureInfo.CurrentCulture);
		}

		[Description("Convert string to Double if not validate throw AppException")]
		public static string ParseToDouble(string Txt,string format)
		{
			Regex regex= new Regex(RegexPattern.Number); 
			string num=CleanInput(Txt);  
			if(!regex.IsMatch(num))
				throw new Exception ("Error Convert To Number");
			else
				return double.Parse (num).ToString (format ,CultureInfo.CurrentCulture);
		}

		#endregion

		#region Parsing Int

		[Description("Convert string to Int if not validate return default value")]
		public static int ParseInt(string Txt,int defaultValue)
		{
			Regex regex= new Regex(RegexPattern.IntNumber); 
			string num=CleanInput(Txt);  
			if(!regex.IsMatch(num))
				return defaultValue;
			else
				return int.Parse (num);   
		}

		[Description("Convert string to Int if not validate return default value")]
		public static int ParseInt(string Txt,string defaultValue)
		{
			Regex regex= new Regex(RegexPattern.IntNumber); 
			string num=CleanInput(Txt);  
			if(!regex.IsMatch(num))
				return int.Parse (defaultValue);
			else
				return int.Parse (num);   
		}

		[Description("Convert string to Int if not validate return default value")]
		public static string ParseIntToString(string Txt,int defaultValue)
		{
			Regex regex= new Regex(RegexPattern.IntNumber); 
			string num=CleanInput(Txt);  
			if(!regex.IsMatch(num))
				return defaultValue.ToString ();
			else
				return int.Parse (num).ToString ();   
		}

		[Description("Convert string to Int if not validate return default value")]
		public static string ParseIntToString(string Txt,string defaultValue)
		{
			Regex regex= new Regex(RegexPattern.IntNumber); 
			string num=CleanInput(Txt);  
			if(!regex.IsMatch(num))
				return int.Parse (defaultValue).ToString ();
			else
				return int.Parse (num).ToString ();   
		}


		[Description("Get Decimal number from string if not found return stringEmpty")]
		public static string GetNumbersFromString(string Txt)
		{
			return ParseNumber(Txt,RegexPattern.GetDecimalFromString);
		}

		[Description("Get int number from string  if not found return stringEmpty")]
		public static string GetIntFromString(string Txt)
		{
			return ParseNumber(Txt,RegexPattern.GetIntFromString);
		}


		[Description("Parse string to number")]
		private static string ParseNumber(string Txt,string regexPattern)
		{
			Regex regex= new Regex(regexPattern); 
			string cleanTxt=CleanInput(Txt);  
			if(regex.Matches(cleanTxt).Count != 1)
			{
				return "";
			}

			Match m = regex.Match(cleanTxt);
			if(m.Success)
			{
				return m.Value;   
			}
			return "";
		}

		#endregion

		#region Parsing Dates

		[Description("Convert string to Date if not validate return default value")]
		public static DateTime ParseDate(string Txt,DateTime defaultValue)
		{
			Regex regex= new Regex(RegexPattern.Date);
			if(IsDate (Txt,false))
			{
				try
				{
					return DateTime.Parse (Txt); 
				}
				catch
				{return defaultValue;}
			}
			return defaultValue;
		}

		[Description("Convert string to Date if not validate return default value")]
		public static string ParseToDate(string Txt,DateTime defaultValue,string format)
		{
			string txt=ParseDates(Txt,RegexPattern.Date);
			if(txt.Length >0)
			{
				return DateTime.Parse (txt).ToString (format,CultureInfo.CurrentCulture);
			}
			return defaultValue.ToString (format,CultureInfo.CurrentCulture);
		}

		[Description("Convert string to Date if not validate return default value")]
		public static string ParseToDate(string Txt,string defaultValue,string format)
		{
			string txt=ParseDates(Txt,RegexPattern.Date);
			if(txt.Length >0)
			{
				return DateTime.Parse (txt).ToString (format,CultureInfo.CurrentCulture);
			}
			return defaultValue;
		}

		[Description("Convert string to Date if not validate throw AppException")]
		public static string ParseToDate(string Txt,string format)
		{
			string txt=ParseDates(Txt,RegexPattern.Date);
			if(txt.Length >0)
			{
				return DateTime.Parse (txt).ToString (format,CultureInfo.CurrentCulture);
			}
			throw new Exception ("Error Convert To Date");
		}

		[Description("Convert string to Date if not validate return default value")]
		public static string ParseToDateTime(string Txt,DateTime defaultValue,string format)
		{
			string txt=ParseDates(Txt,RegexPattern.DateTime );
			if(txt.Length >0)
			{
				return DateTime.Parse (txt).ToString (format,CultureInfo.CurrentCulture);
			}
			return defaultValue.ToString (format,CultureInfo.CurrentCulture);
		}

		[Description("Convert string to Date if not validate return default value")]
		public static string ParseToDateTime(string Txt,string defaultValue,string format)
		{
			string txt=ParseDates(Txt,RegexPattern.DateTime );
			if(txt.Length >0)
			{
				return DateTime.Parse (txt).ToString (format,CultureInfo.CurrentCulture);
			}
			return defaultValue;
		}

		[Description("Convert string to Date if not validate throw AppException")]
		public static string ParseToDateTime(string Txt,string format)
		{
			string txt=ParseDates(Txt,RegexPattern.DateTime);
			if(txt.Length >0)
			{
				return DateTime.Parse (txt).ToString (format,CultureInfo.CurrentCulture);
			}
            throw new Exception("Error Convert To Date");
		}

		[Description("Convert string to Time if not validate return default value")]
		public static string ParseToTime(string Txt,DateTime defaultValue,string format)
		{
			string txt=ParseDates(Txt,RegexPattern.Time );
			if(txt.Length >0)
			{
				return DateTime.Parse (txt).ToString (format,CultureInfo.CurrentCulture);
			}
			return defaultValue.ToString (format,CultureInfo.CurrentCulture);
		}

		[Description("Convert string to Time if not validate return default value")]
		public static string ParseToTime(string Txt,string defaultValue,string format)
		{
			string txt=ParseDates(Txt,RegexPattern.Time );
			if(txt.Length >0)
			{
				return DateTime.Parse (txt).ToString (format,CultureInfo.CurrentCulture);
			}
			return defaultValue;
		}

		[Description("Convert string to Time if not validate throw AppException")]
		public static string ParseToTime(string Txt,string format)
		{
			string txt=ParseDates(Txt,RegexPattern.Time);
			if(txt.Length >0)
			{
				return DateTime.Parse (txt).ToString (format,CultureInfo.CurrentCulture);
			}
            throw new Exception("Error Convert To Date");
		}

		[Description("Parse string to Date string by Regex expretion")]
		private static string ParseDates(string Txt,string regexPattern)
		{
			Regex regex= new Regex(regexPattern); 
			string cleanTxt=CleanInput(Txt);  
			try
			{
				if(regex.IsMatch (Txt))
				{
					DateTime.Parse (Txt); 
					return Txt;
				}
				return "";
			}
			catch
			{return "";}
		}

		#endregion

		#region Numbers Methods

		public static bool IsNumeric(string Txt)
		{
			Regex regex= new Regex(RegexPattern.Number); 
			return regex.IsMatch(CleanInput(Txt));  
		}

		public static bool IsPosetiveNumber(string Txt)
		{
			Regex regex= new Regex(RegexPattern.PosetiveNumber); 
			return regex.IsMatch(CleanInput(Txt));  
		}

		public static bool IsCurrency(string Txt)
		{
			Regex regex= new Regex(RegexPattern.Currency); 
			return regex.IsMatch(CleanInput(Txt));  
		}

		public static bool IsPosetiveCurrency(string Txt)
		{
			Regex regex= new Regex(RegexPattern.PosetiveCurrency ); 
			return regex.IsMatch(CleanInput(Txt));  
		}

		
		public static bool IsIntNumber(string Txt)
		{
			Regex regex= new Regex(RegexPattern.IntNumber); 
			return regex.IsMatch(CleanInput(Txt));  
		}

        public static bool IsDigits(string Txt)
        {
            Regex regex = new Regex(RegexPattern.Digits);
            return regex.IsMatch(CleanInput(Txt));
        }

        public static bool IsInteger(string Txt)
		{
            Regex regex = new Regex("^(-|)[1-9][0-9]+$"); 
			return regex.IsMatch(CleanInput(Txt));  
		}
        public static bool IsPosetiveInteger(string Txt)
        {
            Regex regex = new Regex("^[1-9][0-9]+$");
            return regex.IsMatch(CleanInput(Txt));
        }

		#endregion

		#region Date Methods

		internal static  bool IsDateType(string Txt)
		{
			try
			{
				DateTime dt=System.Convert.ToDateTime (Txt); 
				return dt.GetType ().FullName.EndsWith ("DateTime") ;  
			}
			catch{return false;}
		}

        public static bool IsDate(string Txt, bool TryParse)
        {
            Regex regex = new Regex(RegexPattern.Date);
            bool isok = regex.IsMatch(Txt);
            if (isok && TryParse)
            {
                DateTime dt;
                isok = DateTime.TryParse(Txt, out dt);
            }
            return isok;
        }

		public static  bool IsDateTime(string Txt,bool TryParse)
		{
			Regex regex= new Regex(RegexPattern.DateTime ); 
			bool isok= regex.IsMatch(Txt );  
			if(isok && TryParse)
				isok= IsDateType (Txt) ;   
			return isok;
		}

		public static bool IsTime(string Txt)
		{
			Regex regex= new Regex(RegexPattern.Time ); 
			return regex.IsMatch(Txt );  
		}

		#endregion

		#region General Methods

        public static bool IsGuid(string s)
        {
            Regex regex = new Regex(RegexPattern.IsGuid, RegexOptions.Compiled);
            return regex.IsMatch(s);
        }
        
		public static bool IsBool(string Txt)
		{
			Regex regex= new Regex(RegexPattern.Bool); 
			return regex.IsMatch(Txt.ToLower());  
		}

        public static bool IsEmail(string Txt, bool isSingle = true)
        {
            Regex regex = new Regex(isSingle ? RegexPattern.EmailSingle : RegexPattern.Email);
            return regex.IsMatch(Txt.ToLower());
        }

		public static bool IsIP(string Txt)
		{
			Regex regex= new Regex(RegexPattern.IP  ); 
			return regex.IsMatch(Txt.ToLower());  
		}

        public static bool IsUrl(string url)
        {
            string pattern = @"http://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)? ";
            return Regx.RegexValidateIgnoreCase(pattern, url);
        }

        /// <summary>
        /// Validate Url
        /// </summary>
        /// <param name="prefix">http:// or other types or combination (http|https|ftp)://</param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsUrl(string prefix, string url)
        {
            string pattern = prefix + @"([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)? ";
            return Regx.RegexValidateIgnoreCase(pattern, url);
        }
		#endregion

		#region RegexPattern
		public sealed class RegexPattern
		{

			public const string Custom=@"\w+";
			public const string IntNumber=@"^(?<number>(\ |-)?([1-9][0-9]*?))\z";
			//number 		
            public const string Digits = @"^(?<number>(\ |-)?([0-9]*?))\z";
            public const string PosetiveNumber = @"^(?<number>(\ |-)?([1-9][0-9]*(?<decimal>(\.[0-9]*))?))\z";
			public const string FloatPointNumber=@"^(?<number>(\ |-)?(0?(?<decimal>\.[0-9]*))?)\z";
			public const string Number=@"^(?<number>(\ |-)?([0-9][0-9]*(?<decimal>(\.[0-9]*))?))\z";
			//currency
			public const string PosetiveCurrency=@"^(?<symbol>|.?)(\s?)?(?<number>(\ |-)?([1-9][0-9]*(?<decimal>(\.[0-9]*))?))\z";
			public const string FloatPointCurrency=@"^(?<symbol>|.?)(\s?)?(?<number>(\ |-)?(0?(?<decimal>\.[0-9]*))?)\z";
			public const string Currency=@"^(?<symbol>|.?)(\s?)?(?<number>(\ |-)?([0-9][0-9]*(?<decimal>(\.[0-9]*))?))\z";

			//public const string Currency="^(?<symbol>|.?)(\s?)?(\+|-)?[0-9][0-9]*(\.[0-9]*)?\z";

			
			public const string GetDecimalFromString=@"(\ |-)?[0-9][0-9]*(\.[0-9]*)?";
			public const string GetIntFromString=@"(\ |-)?[0-9][0-9]*";
			public const string HtmlTag=@"<(?<tag>\w*)>(?<text>.*)</\k<tag>>";
			
			public const string KeyValue=@"(?<Key>\w+)\s*=\s*(?<Value>.*)((?=\W$)|\z)";
			public const string Email=@"([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,6}|[0-9]{1,3})";
			public const string EmailSingle=@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,6}|[0-9]{1,3})$";

			public const string Date=@"^(?<Day>\d{1,2})/(?<Month>\d{1,2})/(?<Year>(?:\d{4}|\d{2}))\z";
   			public const string DateTime=@"^(?<Day>\d{1,2})/(?<Month>\d{1,2})/(?<Year>(?:\d{4}|\d{2}))(?<Space>\s{1})(?<Hour>\d{1,2}):(?<Minute>\d{1,2}):(?<Seconde>\d{1,2})\z";
			public const string Time=@"^(?<Hour>\d{1,2}):(?<Minute>\d{1,2}):(?<Seconde>\d{1,2})\z";
			public const string Phone=@"^(?<AreaCode>\d{2,3})-(?<Number>\d{7})\z";
			public const string Url=@"(?<Protocol>\w+):\/\/(?<Domain>[\w.]+\/?)\S*";
			public const string IP=@"(?<First>2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?<Second>2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?<Third>2[0-4]\d|25[0-5]|[01]?\d\d?)\.(?<Fourth>2[0-4]\d|25[0-5]|[01]?\d\d?)";
			public const string Bool=@"^(\0|1|-1|0|false|true)\z";
			public const string UC=@"\p{Lu}";
			public const string LC=@"\p{Ll}";
            public const string IsGuid = @"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$";
 

			const string RegexMail =(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
			const string RegexPhone=(@"\s{1}\d{3}-\d{4}|\d{4}-\d{4}|\d{3}-\d{4}");
			const string RegexWithArea=(@"[(]\d{3}[)]\d{4}-\d{4}|[(]\d{3}[)]\s{1}\d{3}-\d{4}|[(]\d{3}[)]\d{3}-\d{4}");

 
		}
		#endregion
	
	}
}	
