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
using System.Collections;
using System.IO;
using System.Web;
#pragma warning disable  CS1591

namespace Nistec.Runtime
{
    public class BaseConverter
    {

        #region method ToHex

        /// <summary>
        /// Converts specified string to HEX string.
        /// </summary>
        /// <param name="text">String to convert.</param>
        /// <returns>Returns hex string.</returns> 
        public static string Hex(string text)
        {
            return BitConverter.ToString(Encoding.UTF8.GetBytes(text)).ToLower().Replace("-", "");
        }

        /// <summary>
        /// Converts string to hex string.
        /// </summary>
        /// <param name="data">String to convert.</param>
        /// <returns>Returns data as hex string.</returns>
        public static string ToHexString(string data)
        {
            return Encoding.UTF8.GetString(ToHex(Encoding.UTF8.GetBytes(data)));
        }

        /// <summary>
        /// Converts string to hex string.
        /// </summary>
        /// <param name="data">Data to convert.</param>
        /// <returns>Returns data as hex string.</returns>
        public static string ToHexString(byte[] data)
        {
            return Encoding.UTF8.GetString(ToHex(data));
        }

        /// <summary>
        /// Convert byte to hex data.
        /// </summary>
        /// <param name="byteValue">Byte to convert.</param>
        /// <returns></returns>
        public static byte[] ToHex(byte byteValue)
        {
            return ToHex(new byte[] { byteValue });
        }

        /// <summary>
        /// Converts data to hex data.
        /// </summary>
        /// <param name="data">Data to convert.</param>
        /// <returns></returns>
        public static byte[] ToHex(byte[] data)
        {
            byte[] val = null;
            char[] hexChars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

            using (MemoryStream retVal = new MemoryStream(data.Length * 2))
            {
                foreach (byte b in data)
                {
                    byte[] hexByte = new byte[2];

                    // left 4 bit of byte
                    hexByte[0] = (byte)hexChars[(b & 0xF0) >> 4];

                    // right 4 bit of byte
                    hexByte[1] = (byte)hexChars[b & 0x0F];

                    retVal.Write(hexByte, 0, 2);
                }
                val = retVal.ToArray();
            }
            return val;
        }

        /// <summary>
        /// Converts string from hex string.
        /// </summary>
        /// <param name="data">String to convert.</param>
        /// <returns>Returns data as hex string.</returns>
        public static string FromHexString(string data)
        {

            byte[] bytes = FromHex(Encoding.UTF8.GetBytes(data));
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Converts hex byte data to normal byte data. Hex data must be in two bytes pairs, for example: 0F,FF,A3,... .
        /// </summary>
        /// <param name="hexData">Hex data.</param>
        /// <returns></returns>
        public static byte[] FromHex(byte[] hexData)
        {
            if (hexData.Length < 2 || (hexData.Length / (double)2 != Math.Floor(hexData.Length / (double)2)))
            {
                throw new Exception("Illegal hex data, hex data must be in two bytes pairs, for example: 0F,FF,A3,... .");
            }
            byte[] val = null;
            using (MemoryStream retVal = new MemoryStream(hexData.Length / 2))
            {
                // Loop hex value pairs
                for (int i = 0; i < hexData.Length; i += 2)
                {
                    byte[] hexPairInDecimal = new byte[2];
                    // We need to convert hex char to decimal number, for example F = 15
                    for (int h = 0; h < 2; h++)
                    {
                        if (((char)hexData[i + h]) == '0')
                        {
                            hexPairInDecimal[h] = 0;
                        }
                        else if (((char)hexData[i + h]) == '1')
                        {
                            hexPairInDecimal[h] = 1;
                        }
                        else if (((char)hexData[i + h]) == '2')
                        {
                            hexPairInDecimal[h] = 2;
                        }
                        else if (((char)hexData[i + h]) == '3')
                        {
                            hexPairInDecimal[h] = 3;
                        }
                        else if (((char)hexData[i + h]) == '4')
                        {
                            hexPairInDecimal[h] = 4;
                        }
                        else if (((char)hexData[i + h]) == '5')
                        {
                            hexPairInDecimal[h] = 5;
                        }
                        else if (((char)hexData[i + h]) == '6')
                        {
                            hexPairInDecimal[h] = 6;
                        }
                        else if (((char)hexData[i + h]) == '7')
                        {
                            hexPairInDecimal[h] = 7;
                        }
                        else if (((char)hexData[i + h]) == '8')
                        {
                            hexPairInDecimal[h] = 8;
                        }
                        else if (((char)hexData[i + h]) == '9')
                        {
                            hexPairInDecimal[h] = 9;
                        }
                        else if (((char)hexData[i + h]) == 'A' || ((char)hexData[i + h]) == 'a')
                        {
                            hexPairInDecimal[h] = 10;
                        }
                        else if (((char)hexData[i + h]) == 'B' || ((char)hexData[i + h]) == 'b')
                        {
                            hexPairInDecimal[h] = 11;
                        }
                        else if (((char)hexData[i + h]) == 'C' || ((char)hexData[i + h]) == 'c')
                        {
                            hexPairInDecimal[h] = 12;
                        }
                        else if (((char)hexData[i + h]) == 'D' || ((char)hexData[i + h]) == 'd')
                        {
                            hexPairInDecimal[h] = 13;
                        }
                        else if (((char)hexData[i + h]) == 'E' || ((char)hexData[i + h]) == 'e')
                        {
                            hexPairInDecimal[h] = 14;
                        }
                        else if (((char)hexData[i + h]) == 'F' || ((char)hexData[i + h]) == 'f')
                        {
                            hexPairInDecimal[h] = 15;
                        }
                    }

                    // Join hex 4 bit(left hex cahr) + 4bit(right hex char) in bytes 8 it
                    retVal.WriteByte((byte)((hexPairInDecimal[0] << 4) | hexPairInDecimal[1]));
                }
                val = retVal.ToArray();
            }

            return val;
        }


        #endregion

        #region base 32

        // the valid chars for the encoding
        //public static string Base32CharsMask = "QAZ2WSX3" + "EDC4RFV5" + "TGB6YHN7" + "UJM8K9LP";
        //private static string Base32Chars = "ABCDEFGH" + "IJKLMNOP" + "QRSTUVWX" + "YZ456789";

        public static string Base32CharsMask1 = "lc6rm5wfns2bv7ojqdyh4guaeikptxz3";

        //The RFC 4648 Base 32 alphabet
        public const string Base32CharsUpper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        //public const string Base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ456789";
        public const string Base32CharsLower = "abcdefghijklmnopqrstuvwxyz234567";

        /// <summary>
        /// Convert string to Base32 string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ToBase32(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            return ToBase32String(bytes);
        }

        /// <summary>
        /// Convert Base32String to string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string FromBase32(string text)
        {
            byte[] bytes = FromBase32String(text);
            string result = Encoding.UTF8.GetString(bytes);
            return result;
        }

        public static string Base32MaskedEncode(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            return ToBase32String(bytes, Base32CharsMask1);
        }

        public static string Base32MaskedDecode(string text)
        {
            byte[] bytes = FromBase32String(text, Base32CharsMask1);
            string result = Encoding.UTF8.GetString(bytes);
            return result;
        }

        public static string Base32Encode(string text, string map)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            return ToBase32String(bytes, map);
        }

        public static string Base32Decode(string text, string map)
        {
            byte[] bytes = FromBase32String(text,map);
            string result = Encoding.UTF8.GetString(bytes);
            return result;
        }

        /// <summary>
        /// Converts an array of bytes to a Base32 string.
        /// </summary>
        public static string ToBase32String(byte[] bytes, string map= Base32CharsLower)
        {
            StringBuilder sb = new StringBuilder();         // holds the base32 chars
            byte index;
            int hi = 5;
            int currentByte = 0;

            while (currentByte < bytes.Length)
            {
                // do we need to use the next byte?
                if (hi > 8)
                {
                    // get the last piece from the current byte, shift it to the right
                    // and increment the byte counter
                    index = (byte)(bytes[currentByte++] >> (hi - 5));
                    if (currentByte != bytes.Length)
                    {
                        // if we are not at the end, get the first piece from
                        // the next byte, clear it and shift it to the left
                        index = (byte)(((byte)(bytes[currentByte] << (16 - hi)) >> 3) | index);
                    }

                    hi -= 3;
                }
                else if (hi == 8)
                {
                    index = (byte)(bytes[currentByte++] >> 3);
                    hi -= 3;
                }
                else
                {

                    // simply get the stuff from the current byte
                    index = (byte)((byte)(bytes[currentByte] << (8 - hi)) >> 3);
                    hi += 5;
                }

                sb.Append(map[index]);
            }

            return sb.ToString();
        }


        /// <summary>
        /// Converts a Base32-k string into an array of bytes.
        /// </summary>
        /// <exception cref="System.ArgumentException">
        /// Input string <paramref name="str">s</paramref> contains invalid Base32 characters.
        /// </exception>
        public static byte[] FromBase32String(string str, string map= Base32CharsLower)
        {
            int numBytes = str.Length * 5 / 8;
            byte[] bytes = new Byte[numBytes];

            // all UPPERCASE chars
            //str = str.ToUpper();

            int bit_buffer;
            int currentCharIndex;
            int bits_in_buffer;

            if (str.Length < 3)
            {
                bytes[0] = (byte)(map.IndexOf(str[0]) | map.IndexOf(str[1]) << 5);
                return bytes;
            }

            bit_buffer = (map.IndexOf(str[0]) | map.IndexOf(str[1]) << 5);
            bits_in_buffer = 10;
            currentCharIndex = 2;
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)bit_buffer;
                bit_buffer >>= 8;
                bits_in_buffer -= 8;
                while (bits_in_buffer < 8 && currentCharIndex < str.Length)
                {
                    bit_buffer |= map.IndexOf(str[currentCharIndex++]) << bits_in_buffer;
                    bits_in_buffer += 5;
                }
            }

            return bytes;
        }

        #endregion

        #region  base converter

        static char[] map = new char[] {
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K',
        'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W',
        'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g',
        'h', 'j', 'k', 'm', 'n', 'p', 'q', 'r', 's', 't',
        'u', 'v', 'x', 'y', 'z', '2', '3', '4', };

        static char[] base62Map = new char[] { '0','1','2','3','4','5','6','7','8','9',
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K',
        'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W',
        'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g',
        'h', 'j', 'k', 'm', 'n', 'p', 'q', 'r', 's', 't',
        'u', 'v', 'x', 'y', 'z', '2', '3', '4', };



        public static string ToBase62(long inp)
        {
            return Encode(inp, base62Map);
        }

        public static long FromBase62(string encoded)
        {
            return Decode(encoded, base62Map);
        }

        // This does not "pad" values 
        public static string Encode(long inp, IEnumerable<char> map)
        {
            var b = map.Count();
            // value -> character 
            var toChar = map.Select((v, i) => new { Value = v, Index = i }).ToDictionary(i => i.Index, i => i.Value);
            var res = "";
            if (inp == 0)
            {
                return "" + toChar[0];
            }
            while (inp > 0)
            {
                // encoded least-to-most significant 
                var val = (int)(inp % b);
                inp = inp / b;
                res += toChar[val];
            }
            return res;
        }

        public static long Decode(string encoded, IEnumerable<char> map)
        {
            var b = map.Count();
            // character -> value 
            var toVal = map.Select((v, i) => new { Value = v, Index = i }).ToDictionary(i => i.Value, i => i.Index);
            long res = 0;
            // go in reverse to mirror encoding 
            for (var i = encoded.Length - 1; i >= 0; i--)
            {
                var ch = encoded[i];
                var val = toVal[ch];
                res = (res * b) + val;
            }
            return res;
        }
        #endregion

        #region Base64 converter

        /// <summary>
        /// Convert string to Base64 string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ToBase64(string text)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
        }

        /// <summary>
        /// Convert Base64String to string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string FromBase64(string text)
        {
            byte[] bytes = Convert.FromBase64String(text);
            return Encoding.UTF8.GetString(bytes);
        }

        #endregion

        #region ecape converter

        public static string Escape(string text, string expression, string replacment)
        {
            if (text == null || expression == null || replacment == null)
                return text;
            return text.Replace(expression, replacment);
        }

        public static string UnEscape(string text, string expression, string replacment)
        {
            if (text == null || expression == null || replacment == null)
                return text;
            return text.Replace(replacment, expression);
        }

        public static string Escape(string text, string[] expReplacment)
        {
            if (text == null || expReplacment == null || expReplacment.Length == 0 || expReplacment.Length % 2 != 0)
                return text;

            for (int i = 0; i < expReplacment.Length; i++)
            {
                text = text.Replace(expReplacment[i], expReplacment[i + 1]);
                i++;
            }
            return text;
        }

        public static string UnEscape(string text, string[] expReplacment)
        {
            if (text == null || expReplacment == null || expReplacment.Length == 0 || expReplacment.Length % 2 != 0)
                return text;

            for (int i = 0; i < expReplacment.Length; i++)
            {
                text = text.Replace(expReplacment[i + 1], expReplacment[i]);
                i++;
            }
            return text;
        }

        public static string Escape(string text, char wrapper, params string[] replacments)
        {
            if (text == null || replacments == null || replacments.Length == 0 || replacments.Length % 2 != 0)
                return text;

            for (int i = 0; i < replacments.Length; i++)
            {
                text = text.Replace(replacments[i], wrapper.ToString() + replacments[i] + wrapper.ToString());
            }
            return text;
        }

        public static string UnEscape(string text, char wrapper, params string[] replacments)
        {
            if (text == null || replacments == null || replacments.Length == 0 || replacments.Length % 2 != 0)
                return text;

            for (int i = 0; i < replacments.Length; i++)
            {
                text = text.Replace(wrapper.ToString() + replacments[i] + wrapper.ToString(), replacments[i]);
            }
            return text;
        }
        #endregion
    }

    /// <summary>
    /// Base64Url
    /// </summary>
    public static class Base64Url
    {
        public static string Encode(object ostr, bool urlEncode = false, string encode = "utf-8")//"windows-1255")
        {
            if (ostr == null)
                return "";
            if (urlEncode)
                return Encode(HttpUtility.UrlEncode(ostr.ToString()), encode);
            return Encode(ostr.ToString(), encode);

        }

        public static string Encode(string str, string encode = "utf-8")//"windows-1255")
        {
            if (string.IsNullOrEmpty(str))
                return str;
            return EncodeBytes(Encoding.GetEncoding(encode).GetBytes(str));

        }
        public static string EncodeBytes(byte[] arg)
        {
            if (arg == null)
            {
                throw new ArgumentNullException("arg");
            }

            var s = Convert.ToBase64String(arg);
            return s
                .Replace("=", "")
                .Replace("/", "_")
                .Replace("+", "-");
        }

        static string ToBase64(string arg)
        {
            if (arg == null)
            {
                throw new ArgumentNullException("arg");
            }

            var s = arg
                    .PadRight(arg.Length + (4 - arg.Length % 4) % 4, '=')
                    .Replace("_", "/")
                    .Replace("-", "+");

            return s;
        }

        public static byte[] DecodeBytes(string arg)
        {
            if (string.IsNullOrEmpty(arg))
                return null;

            var decrypted = ToBase64(arg);

            return Convert.FromBase64String(decrypted);
        }

        public static string Decode(string base64String, string encode = "utf-8")//"windows-1255")
        {
            if (string.IsNullOrEmpty(base64String))
                return base64String;

            return Encoding.GetEncoding(encode).GetString(DecodeBytes(base64String));
        }
    }
}
