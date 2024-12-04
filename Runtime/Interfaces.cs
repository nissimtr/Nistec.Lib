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
using System.IO;
using Nistec.IO;
using System.Collections;
using Nistec.Generic;
using Nistec.Serialization;

#pragma warning disable  CS1591

namespace Nistec.Runtime
{
    public enum TransformType : byte { None = 0, Object = 100, Stream = 101, Json = 102, Base64 = 103, Text = 104, Ack = 105, State = 106, Csv = 107, Xml = 108 }//{Message=0,Stream=1,Json=2 }

    public enum DuplexTypes : byte { None = 0, Respond = 1 }//, WaitOne=2}

    public interface ITransformHeader //: IDisposable
    {

        /// <summary>
        /// Get or Set indicate wether the message is a duplex type.
        /// </summary>
        bool IsDuplex { get; }//{ get; set; }

        /// <summary>
        /// Get or Set DuplexType.
        /// </summary>
        DuplexTypes DuplexType { get; }

        ///// <summary>
        /////  Get or Set The message expiration.
        ///// </summary>
        //int Expiration { get;}
        /// <summary>
        /// Get or Set The result type name.
        /// </summary>
        TransformType TransformType { get; }
    }

    public interface IMessageStream
    {
        ///// <summary>
        ///// Get or Set the message key.
        ///// </summary>
        //Guid ItemId { get;}
        /// <summary>
        /// Get or Set the message key.
        /// </summary>
        string Identifier { get;}
        /// <summary>
        /// Get or Set The serializer formatter.
        /// </summary>
        Formatters Formatter { get; set; }
        /// <summary>
        /// Get or Set the message detail.
        /// </summary>
        string Label { get; set; }
        /// <summary>
        /// Get or Set the message group.
        /// </summary>
        string CustomId { get; set; }
        /// <summary>
        /// Get or Set the message group.
        /// </summary>
        string SessionId { get; set; }
        /// <summary>
        /// Get or Set the message command.
        /// </summary>
        string Command { get; set; }
        /// <summary>
        /// Get or Set who send the message.
        /// </summary>
        string Source { get; set; }
        /// <summary>
        ///  Get or Set The message expiration.
        /// </summary>
        int Expiration { get; set; }
        /// <summary>
        /// Get or Set the last time that message was modified.
        /// </summary>
        DateTime Creation { get; set; }
        /// <summary>
        /// Get or Set the extra arguments for current message.
        /// </summary>
        NameValueArgs Args { get; set; }
        /// <summary>
        /// Get or Set the transform type name.
        /// </summary>
        TransformType TransformType { get; set; }
        /// <summary>
        /// Get or Set indicate wether the message is a duplex type.
        /// </summary>
        DuplexTypes DuplexType { get; set; }

        //ITransformHeader Transform { get;}
        /// <summary>
        /// Get entity as json
        /// </summary>
        /// <param name="pretty"></param>
        /// <returns></returns>
        string ToJson(bool pretty = false);
        /// <summary>
        /// Get body stream ready to read from position 0.
        /// </summary>
        /// <returns></returns>
        NetStream GetStream();
        ///// <summary>
        ///// Get or Set The return type name.
        ///// </summary>
        //string ReturnTypeName { get; set; }
    }

    //public interface IMessageStream : IDisposable
    //{

    //    ///// <summary>
    //    ///// Get or Set The message command.
    //    ///// </summary>
    //    //string Command { get; set; }
    //    /// <summary>
    //    /// Get or Set indicate wether the message is a duplex type.
    //    /// </summary>
    //    bool IsDuplex { get; set; }
    //    /// <summary>
    //    ///  Get or Set The message expiration.
    //    /// </summary>
    //    int Expiration { get; set; }
    //    ///// <summary>
    //    ///// Get or Set The result type name.
    //    ///// </summary>
    //    //string ReturnTypeName { get; set; }
    //}

    //public interface IMessageStream
    //{
    //    NetStream GetBodyStream();

    //    NetStream BodyStream { get; }

    //    string TypeName { get; }

    //    void SetBody(object value);

    //    void SetBody(byte[] body, Type type);

    //    object DecodeBody();

    //    bool IsEmpty { get; }
    //}

}
