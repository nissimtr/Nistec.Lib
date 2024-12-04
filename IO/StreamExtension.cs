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
using System.Security;
using Nistec.Runtime;

#pragma warning disable  CS1591

namespace Nistec.IO
{
    public static class StreamExtension
    {

        public static string ToBase64(this NetStream stream)
        {
            return Convert.ToBase64String(stream.ToArray());
        }


        #region peek and replace
        /// <summary>
        /// Peek a bytes from given offset and return a <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static DateTime PeekDateTime(this NetStream stream, int offset)
        {
            long g = stream.PeekInt64(offset);

            return DateTime.FromBinary(g);
        }


        /// <summary>
        /// Peek a range of bytes from given offset and count.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        [SecuritySafeCritical]
        public static string PeekString(this NetStream stream, int offset, int count)
        {
            byte[] b = stream.PeekBytes(offset, count);
            if (b == null)
                return null;
            return Encoding.UTF8.GetString(b);
        }

        /// <summary>
        /// Peek a range of bytes from given offset and offset+1 for count.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        [SecuritySafeCritical]
        public static string PeekString(this NetStream stream, int offset, out int length)
        {
            int count = stream.PeekInt32(offset + 1);
            length = count;
            return PeekString(stream,offset, count);
        }

        /// <summary>
        /// Peek a range of the last bytes from given count.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        [SecuritySafeCritical]
        public static byte[] PeekLastBytes(this NetStream stream, int count)
        {
            int offset = stream.iLength - count;
            return stream.PeekBytes(offset, count);
        }

        /// <summary>
        /// Peek a range of bytes from given offset and count.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        [SecuritySafeCritical]
        public static NetStream PeekStream(this NetStream stream, int offset, int count)
        {
            byte[] b = stream.PeekBytes(offset, count);
            if (b == null)
                return null;
            return new NetStream(stream.PeekBytes(offset, count).ToArray());
        }

        /// <summary>
        /// Peek a range of bytes from given offset and offset+1 for count.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        [SecuritySafeCritical]
        public static NetStream PeekStream(this NetStream stream, int offset, out int length)
        {
            int count = stream.PeekInt32(offset + 1);
            length = count;
            return PeekStream(stream,offset, count);
        }

        
       
        #endregion

    }
}
