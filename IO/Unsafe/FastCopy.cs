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

#pragma warning disable  CS1591

namespace Nistec.IO.Unsafe
{
    public class FastCopy
    {
        // The unsafe keyword allows pointers to be used within
        // the following method:
        static unsafe void Copy(byte[] src, int srcIndex,
            byte[] dst, int dstIndex, int count)
        {
            if (src == null || srcIndex < 0 ||
                dst == null || dstIndex < 0 || count < 0)
            {
                throw new ArgumentException();
            }
            int srcLen = src.Length;
            int dstLen = dst.Length;
            if (srcLen - srcIndex < count ||
                dstLen - dstIndex < count)
            {
                throw new ArgumentException();
            }


            // The following fixed statement pins the location of
            // the src and dst objects in memory so that they will
            // not be moved by garbage collection.		
            fixed (byte* pSrc = src, pDst = dst)
            {
                byte* ps = pSrc;
                byte* pd = pDst;

                // Loop over the count in blocks of 4 bytes, copying an
                // integer (4 bytes) at a time:
                for (int n = 0; n < count / 4; n++)
                {
                    *((int*)pd) = *((int*)ps);
                    pd += 4;
                    ps += 4;
                }

                // Complete the copy by moving any bytes that weren't
                // moved in blocks of 4:
                for (int n = 0; n < count % 4; n++)
                {
                    *pd = *ps;
                    pd++;
                    ps++;
                }
            }
        }

        public static void FileCopy(string src, string dest)
        {
            FileStream src_stream = null;

            if (string.IsNullOrEmpty(src))
            {
                throw new ArgumentNullException("src");
            }
            if (string.IsNullOrEmpty(dest))
            {
                throw new ArgumentNullException("dest");
            }
            try
            {
                src_stream = new FileStream(src, FileMode.Open);
                int length = (int)src_stream.Length;
                byte[] src_bytes = new byte[src_stream.Length];
                src_stream.Read(src_bytes, 0, src_bytes.Length);
                byte[] dst_bytes = new byte[src_bytes.Length];

                Copy(src_bytes, 0, dst_bytes, 0, length);


            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                if(src_stream!=null)
                {
                    src_stream.Close();
                    src_stream=null;
                }
            }
        }

    }

}