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
using System.IO.Compression;
using System.IO;
#pragma warning disable  CS1591
namespace Nistec.Generic
{
  
    public class NetZipp
    {

        public static byte[] Compress(string value)
        {
            byte[] bZipped = null;

            //Transform string into byte[]  
            byte[] byteArray = Encoding.Default.GetBytes(value);

            //Prepare for compress
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                using (GZipStream sw = new System.IO.Compression.GZipStream(ms,
                       CompressionMode.Compress))
                {

                    //Compress
                    sw.Write(byteArray, 0, byteArray.Length);
                    //Close, DO NOT FLUSH cause bytes will go missing...
                    sw.Close();

                    //Transform byte[] zip data to string
                    bZipped = ms.ToArray();
                }
            }
            return bZipped;
        }

        public static string Decompress(byte[] b)
        {
            string strZipped = null;

            //Transform string into byte[]
            //byte[] byteArray = Encoding.Default.GetBytes(value);

            //Prepare for decompress
            using (MemoryStream ms = new MemoryStream(b))
            {
                using (GZipStream sr = new System.IO.Compression.GZipStream(ms,
                    CompressionMode.Decompress))
                {

                    //Reset variable to collect uncompressed result

                    //byteArray = new byte[byteArray.Length * 5];

                    //Decompress
                    byte[] byteArray = IoHelper.ReadSream(sr, 0);
                    //int rByte = sr.Read(byteArray, 0, byteArray.Length);

                    strZipped = Encoding.Default.GetString(byteArray);
                }
            }
            return strZipped;
        }

        public static string Zip(string value)
        {
            string strZipped = null;
 
            //Transform string into byte[]  
            byte[] byteArray = Encoding.Default.GetBytes(value);

            //Prepare for compress
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                using (GZipStream sw = new System.IO.Compression.GZipStream(ms,
                       CompressionMode.Compress))
                {

                    //Compress
                    sw.Write(byteArray, 0, byteArray.Length);
                    //Close, DO NOT FLUSH cause bytes will go missing...
                    sw.Close();

                    //Transform byte[] zip data to string
                    strZipped = Encoding.Default.GetString(ms.ToArray());
                }
            }
            return strZipped;
        }

        public static string UnZip(string value)
        {
            string strZipped = null;

            //Transform string into byte[]
            byte[] byteArray = Encoding.Default.GetBytes(value);

            //Prepare for decompress
            using (MemoryStream ms = new MemoryStream(byteArray))
            {
                using (GZipStream sr = new System.IO.Compression.GZipStream(ms,
                    CompressionMode.Decompress))
                {

                    //Reset variable to collect uncompressed result
                   
                    //byteArray = new byte[byteArray.Length * 5];

                    //Decompress
                    byteArray = IoHelper.ReadSream(sr, 0);
                    //int rByte = sr.Read(byteArray, 0, byteArray.Length);

                    strZipped = Encoding.Default.GetString(byteArray);
                }
            }
            return strZipped;
        }

    }
}
