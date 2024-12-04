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
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Configuration;
using System.Collections.Generic;
using Nistec;
using System.Diagnostics;
using System.Globalization;
#pragma warning disable  CS1591

namespace Nistec.Runtime
{
  
    public enum EncyptionType
    {
        ENB64,
        ENU64,
        ENHEX,
        ENB32,
        Ex32
    }


    public class Encryption
    {

        #region non static

        private string m_key;
        private byte[] mbytKey;
        private byte[] mbytIV;
        private bool mbKeyIsSet = false;
        private string mksKeyNotSetException = "Criptografy  configuration Exception.";
        private string mksPassphraseTooSmall = "Criptografy  configuration Exception {0} caracteres.";


        public Encryption() { mbKeyIsSet = false; }

        public Encryption(string key)
        {
            mbKeyIsSet = false;
            SetPassPhrase( key);
        }

        void SetPassPhrase(string key)
        {
                const int iMinLength = -1;
                m_key = key.Trim();

                if (m_key.Length > iMinLength || iMinLength == -1)
                {
                    SHA256Managed sha2 = new SHA256Managed();
                    mbytKey = sha2.ComputeHash(Types.BytesFromString(m_key));
                    string sKey = Convert.ToBase64String(mbytKey);
                    mbytIV = Encoding.ASCII.GetBytes(sKey.Remove(0, sKey.Length - 16));
                    mbKeyIsSet = true;
                    sha2 = null;
                }
                else
                {
                    mbKeyIsSet = false;
                    throw new Exception(String.Format(mksPassphraseTooSmall, (iMinLength + 1).ToString()));
                }
        }

        #region public Methods

        public bool DecryptFile(string targetFile)
        {
            if (mbKeyIsSet)
                return decryptFile(targetFile, targetFile);
            else
            {
                throw new Exception(mksKeyNotSetException);
            }
        }

        public bool DecryptFile(string encryptedFile, string plainFile)
        {
            if (mbKeyIsSet)
                return decryptFile(encryptedFile, plainFile);
            else
                throw new Exception(mksKeyNotSetException);
        }

        public MemoryStream DecryptStream(MemoryStream encryptedStream)
        {
            if (mbKeyIsSet)
            {
                try
                {
                    RijndaelManaged oCSP = new RijndaelManaged();
                    oCSP.Key = mbytKey;
                    oCSP.IV = mbytIV;
                    ICryptoTransform ct = oCSP.CreateDecryptor();
                    CryptoStream cs = new CryptoStream(encryptedStream, ct, CryptoStreamMode.Read);
                    byte[] byteArray = new byte[encryptedStream.Length];
                    int iBytesIn = cs.Read(byteArray, 0, (int)encryptedStream.Length);
                    cs.Close();
                    MemoryStream plainStream = new MemoryStream();
                    plainStream.Write(byteArray, 0, iBytesIn);
                    return plainStream;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    return null;
                }
            }
            else
            {
                throw new Exception(mksKeyNotSetException);
            }
        }

        public string DecryptString(string encryptedString)
        {
            if (mbKeyIsSet)
                return decryptString(encryptedString, true);
            else
                throw new Exception(mksKeyNotSetException);
        }

        public string DecryptString(string encryptedString, bool base64)
        {
            if (mbKeyIsSet)
                return decryptString(encryptedString, base64);
            else
                throw new Exception(mksKeyNotSetException);
        }

        public string EncryptString(string plainString)
        {
            if (mbKeyIsSet)
                return encryptString(plainString, true);
            else
                throw new Exception(mksKeyNotSetException);
        }

        public string EncryptString(string plainString, bool base64)
        {
            if (mbKeyIsSet)
                return encryptString(plainString, base64);
            else
                throw new Exception(mksKeyNotSetException);
        }

        public bool EncryptFile(string targetFile)
        {
            if (mbKeyIsSet)
                return encryptFile(targetFile, targetFile);
            else
            {
                throw new Exception(mksKeyNotSetException);
            }
        }

        public bool EncryptFile(string plainFile, string targetFile)
        {
            if (mbKeyIsSet)
                return encryptFile(plainFile, targetFile);
            else
            {
                throw new Exception(mksKeyNotSetException);
            }
        }

        public MemoryStream EncryptStream(MemoryStream plainStream)
        {
            if (mbKeyIsSet)
            {
                try
                {
                    MemoryStream encStream = new MemoryStream();
                    RijndaelManaged oCSP = new RijndaelManaged();
                    oCSP.Key = mbytKey;
                    oCSP.IV = mbytIV;
                    ICryptoTransform ct = oCSP.CreateEncryptor();
                    CryptoStream cs = new CryptoStream(encStream, ct, CryptoStreamMode.Write);
                    byte[] byteArray = plainStream.ToArray();
                    cs.Write(byteArray, 0, (int)plainStream.Length);
                    cs.FlushFinalBlock();
                    cs.Close();
                    return encStream;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    return null;
                }
            }
            else
            {
                throw new Exception(mksKeyNotSetException);
            }
        }

        public string GetHashString(string inputValue)
        {
            try
            {
                byte[] inputBytes = Types.BytesFromString(inputValue);
                byte[] hashValue = new SHA1Managed().ComputeHash(inputBytes);
                return Types.BytesToHexString(hashValue);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return String.Empty;
            }
        }

        public bool ValidPassword(string passphrase, string hashValue)
        {
            return (GetHashString(passphrase) == hashValue);
        }

        #endregion

        #region Internal helpers

        private string encryptString(string plainText, bool base64)
        {
            try
            {
                byte[] byteArray = Types.BytesFromString(plainText);
                MemoryStream msPlain = new MemoryStream(byteArray);
                MemoryStream msEnc = EncryptStream(msPlain);

                if (base64)
                {
                    return Convert.ToBase64String(msEnc.ToArray());
                }
                else
                {
                    return Types.BytesToString(msEnc.ToArray());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return String.Empty;
            }
        }

        private string decryptString(string encryptedString, bool base64)
        {
            try
            {
                byte[] byteArray;
                if (base64)
                {
                    byteArray = Convert.FromBase64String(encryptedString);
                }
                else
                {
                    byteArray = Types.BytesFromString(encryptedString);
                }

                MemoryStream msEnc = new MemoryStream(byteArray);
                MemoryStream msPlain = DecryptStream(msEnc);
                return Types.BytesToString(msPlain.GetBuffer());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return String.Empty;
            }
        }

        public bool EncryptStringToFile(string output, string filename, bool base64)
        {
            string encryptedString = EncryptString(output, base64);

            using (BinaryWriter binWriter =
           new BinaryWriter(File.Open(filename, FileMode.OpenOrCreate)))
            {
                binWriter.Write(encryptedString);
                binWriter.Close();
            }

            return true;
        }

        public string DecryptFileToString(string filename, bool base64)
        {
            StringBuilder output = new StringBuilder();

            FileStream fs = new FileStream(filename, FileMode.OpenOrCreate);
            BinaryReader r = new BinaryReader(fs);

            r.BaseStream.Seek(0, SeekOrigin.Begin);

            output.Append(r.ReadString());

            fs.Close();
            return decryptString(output.ToString(), base64);
        }

        private bool encryptFile(string plainFile, string encryptedFile)
        {
            try
            {
                bool bReplaceFile = (encryptedFile.ToLower(CultureInfo.CurrentCulture).Trim() == plainFile.ToLower(CultureInfo.CurrentCulture).Trim());
                FileStream fsIn = File.OpenRead(plainFile);
                string sEncryptedFile;

                if (bReplaceFile)
                {
                    sEncryptedFile = Path.GetTempFileName();
                }
                else
                {
                    sEncryptedFile = encryptedFile;
                }

                FileStream fsOut = File.OpenWrite(sEncryptedFile);
                RijndaelManaged oCSP = new RijndaelManaged();
                oCSP.Key = mbytKey;
                oCSP.IV = mbytIV;
                ICryptoTransform ct = oCSP.CreateEncryptor();
                CryptoStream cs = new CryptoStream(fsOut, ct, CryptoStreamMode.Write);
                byte[] bytesPlain = new byte[fsIn.Length];

                fsIn.Read(bytesPlain, 0, (int)fsIn.Length);
                cs.Write(bytesPlain, 0, (int)fsIn.Length);
                cs.FlushFinalBlock();
                cs.Close();
                fsIn.Close();
                fsOut.Close();
                if (bReplaceFile)
                    return IoHelper.ReplaceFile(sEncryptedFile, plainFile, true);
                else
                    return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }
        }

        private bool decryptFile(string encryptedFile, string plainFile)
        {
            try
            {
                bool bReplaceFile = (encryptedFile.ToLower(CultureInfo.CurrentCulture).Trim() == plainFile.ToLower(CultureInfo.CurrentCulture).Trim());
                FileStream fsIn = File.OpenRead(encryptedFile);

                RijndaelManaged oCSP = new RijndaelManaged();
                oCSP.Key = mbytKey;
                oCSP.IV = mbytIV;
                ICryptoTransform ct = oCSP.CreateDecryptor();
                CryptoStream cs = new CryptoStream(fsIn, ct, CryptoStreamMode.Read);
                byte[] bytesPlain = new byte[fsIn.Length];
                int iBytesIn = cs.Read(bytesPlain, 0, (int)fsIn.Length);
                cs.Close();
                fsIn.Close();
                string sPlainFile;

                if (bReplaceFile)
                {
                    sPlainFile = Path.GetTempFileName();
                }
                else
                {
                    sPlainFile = plainFile;
                }

                FileStream fsOut = File.OpenWrite(sPlainFile);
                fsOut.Write(bytesPlain, 0, iBytesIn);
                fsOut.Close();

                if (bReplaceFile)
                    return IoHelper.ReplaceFile(sPlainFile, encryptedFile, true);
                else
                    return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }
        }

        #endregion

        #endregion

        #region Fields

        private static byte[] key = { };

        private static byte[] IV = { 38, 55, 206, 48, 28, 64, 20, 16 };

        private static string stringKey ="AD5467TH";// "!5467a#KN";



        #endregion
        
        #region pass methods

        const string passkey = "PS5467TH";

        public static string EncryptPass(string text)
        {
            return Encryption.Encrypt(text, passkey, false);
        }
        public static string DecryptPass(string text)
        {
            return Encryption.Decrypt(text, passkey, false);
        }

        #endregion

        #region base 64 converter

        public static string ToBase64String(string text, bool useEncode)
        {
            Byte[] byteArray = Encoding.UTF8.GetBytes(text);
            string value = Convert.ToBase64String(byteArray);
            if (useEncode)
            {
                return System.Web.HttpUtility.UrlEncode(value);
            }
            return value;
        }

        public static bool TryFromBase64String(string text, bool useDecode, out string result)
        {
            result = text;
            try
            {
                if (useDecode)
                {
                    text = System.Web.HttpUtility.UrlDecode(text);
                    text = text.Replace(" ", "+");
                }

                if ((text.Length % 4) != 0)
                {
                    //result = text;
                    return false;
                }
                if (!Nistec.Regx.RegexValidateIgnoreCase("[A-Za-z0-9+/=]", text))
                {
                    //result = text;
                    return false;
                }
                Byte[] byteArray = Convert.FromBase64String(text);
                string value = Encoding.UTF8.GetString(byteArray);
                result = value;
                return true;
            }
            catch (Exception)
            {
                //result = text;
                // Handle Exception Here
                return false;
            }
        }

        public static string FromBase64String(string text, bool useDecode)
        {
            string result = "";
            TryFromBase64String(text, useDecode, out result);
            return result;
        }

        public static string FromBase64String(string text)
        {
            return FromBase64String(text, false);
        }

        #endregion
        
        #region static Public Methods

        public static string EncodeUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return "";
            return ToBase64String(url,true);
        }
        public static string DecodeUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return "";
            return FromBase64String(url, true);
        }

 
        public static string Encrypt(string text, bool useEncode)
        {
            return Encrypt( text,stringKey, useEncode);
        }

        public static string Encrypt(string text,string strKey, bool useEncode=false)
        {
            string value = string.Empty;
            try
            {

                key = Encoding.UTF8.GetBytes(strKey.Substring(0, 8));
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                Byte[] byteArray = Encoding.UTF8.GetBytes(text);
                MemoryStream memoryStream = new MemoryStream();
                CryptoStream cryptoStream = new CryptoStream(memoryStream,
                    des.CreateEncryptor(key, IV), CryptoStreamMode.Write);
                cryptoStream.Write(byteArray, 0, byteArray.Length);
                cryptoStream.FlushFinalBlock();
                value = Convert.ToBase64String(memoryStream.ToArray());
                memoryStream.Close();
                if (useEncode)
                {
                    return System.Web.HttpUtility.UrlEncode(value);
                }
                return value;
            }
            catch (Exception)
            {
                // Handle Exception Here
                return text;
            }
        }

        public static string Decrypt(string text, bool useDecode=false)
        {
            return Decrypt( text, stringKey, useDecode);
        }

        public static string Decrypt(string text, string strKey, bool useDecode=false)
        {
            string value = string.Empty;
            try
            {
                if (useDecode)
                {
                    text = System.Web.HttpUtility.UrlDecode(text);
                }

               
                key = Encoding.UTF8.GetBytes(strKey.Substring(0, 8));
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                Byte[] byteArray = Convert.FromBase64String(text);
                MemoryStream memoryStream = new MemoryStream();
                CryptoStream cryptoStream = new CryptoStream(memoryStream,
                des.CreateDecryptor(key, IV), CryptoStreamMode.Write);
                cryptoStream.Write(byteArray, 0, byteArray.Length);
                cryptoStream.FlushFinalBlock();
                value= Encoding.UTF8.GetString(memoryStream.ToArray());
                memoryStream.Close();
               
                return value;
            }

            catch (Exception ex)
            {
                string err = ex.Message;
                // Handle Exception Here
                return text;
            }
        }


        public static string EncryptArgs(char spliter, bool useEncode,params string[] args)
        {
            if (args == null)
                return null;
            StringBuilder sb = new StringBuilder();
            int len=args.Length;
            if (len <= 0)
                return null;
            for (int i = 0; i < len; i++)
            {
                sb.AppendFormat("{0}{1}", args[i], (i == len - 1) ? "" : spliter.ToString());
            }
            return Encrypt(sb.ToString(), useEncode);
        }

        public static string[] DecryptArgs(string text, bool useDecode, int minArgs, params string[] spliter)
        {
            string dec = Decrypt(text, useDecode);
            if (dec == null)
                return null;

            string[] args = dec.Split(spliter, StringSplitOptions.None);
            if (args == null || args.Length < minArgs)
                return null;
            return args;

        }

        public static string[] DecryptArgs(string text, bool useDecode, int minArgs, params char[] spliter)
        {
            string dec = Decrypt(text, useDecode);
            if (dec == null)
                return null;

            string[] args= dec.Split(spliter);
            if (args == null || args.Length < minArgs)
                return null;
            return args;
        }

        public static string Enlock()
        {
            string value = string.Empty;
            try
            {
                string text = DateTime.Today.ToString("yyyyMMdd") + "000000";
                byte[] key = { 65, 68, 53, 52, 54, 55, 84, 72 };
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                Byte[] byteArray = Encoding.UTF8.GetBytes(text);
                MemoryStream memoryStream = new MemoryStream();
                CryptoStream cryptoStream = new CryptoStream(memoryStream,
                    des.CreateEncryptor(key, IV), CryptoStreamMode.Write);
                cryptoStream.Write(byteArray, 0, byteArray.Length);
                cryptoStream.FlushFinalBlock();
                value = Convert.ToBase64String(memoryStream.ToArray());
                memoryStream.Close();
                return value;
            }
            catch (Exception)
            {
                // Handle Exception Here
                return null;
            }
        }

        public static bool Delock(string text)
        {
            string value = string.Empty;
            try
            {
                //string prefix = Nistec.Runtime.AssemblyResolver.GetAssemblyKey(type);
                byte[] key = { 65, 68, 53, 52, 54, 55, 84, 72 };
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                Byte[] byteArray = Convert.FromBase64String(text);
                MemoryStream memoryStream = new MemoryStream();
                CryptoStream cryptoStream = new CryptoStream(memoryStream,
                des.CreateDecryptor(key, IV), CryptoStreamMode.Write);
                cryptoStream.Write(byteArray, 0, byteArray.Length);
                cryptoStream.FlushFinalBlock();
                value = Encoding.UTF8.GetString(memoryStream.ToArray());
                memoryStream.Close();
                string time = DateTime.Today.ToString("yyyyMMdd") + "000000";
                return value == time;
            }

            catch (Exception)
            {
                // Handle Exception Here
                return false;
            }
        }

        #endregion
            
        #region static method ComputeMd5

        /// <summary>
        /// Computes md5 hash.
        /// </summary>
        /// <param name="text">Text to hash.</param>
        /// <param name="hex">Specifies if md5 value is returned as hex string.</param>
        /// <returns>Resturns md5 value or md5 hex value.</returns>
        public static string ComputeMd5(string text, bool hex)
        {
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(text));

            if (hex)
            {
                return BaseConverter.ToHexString(System.Text.Encoding.Default.GetString(hash)).ToLower();
            }
            else
            {
                return System.Text.Encoding.Default.GetString(hash);
            }
        }

 
        public static string HashPassword(string password, string prefix, out string encrypted)
        {
            encrypted = EncryptPass(prefix + password);
            string hash = Convert.ToBase64String(System.Security.Cryptography.MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(password)));
            return hash;
        }

        public static string HashPassword(string password)
        {
            string hash = Convert.ToBase64String(System.Security.Cryptography.MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(password)));
            return hash;
        }

        public static bool IsValidHashPassword(string password, string hashcode)
        {
            string hash = Convert.ToBase64String(System.Security.Cryptography.MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(password)));
            return hash == hashcode;
        }

        #endregion
 
    }

}
