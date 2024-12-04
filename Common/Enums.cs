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

#pragma warning disable  CS1591
namespace Nistec
{
    [Flags]
    public enum LogLevel
    {
        Debug = 2,
        Info = 4,
        Warn = 8,
        Error = 16,
        Fatal = 32,
        Trace = 64
    }

    ///// <summary>
    ///// Message Direction
    ///// </summary>
    //public enum MessageDirection
    //{
    //    Request,
    //    Response
    //}

    // Message State

    //public enum MessageState
    //{
    //    None = 0,
    //    Ok = 200,
    //    ItemNotFound = 400,
    //    Failed = 401,
    //    TimeoutError = 501,
    //    NetworkError = 502,
    //    MessageError = 503,
    //    SerializeError = 504,
    //    SecurityError = 505,
    //    ArgumentsError = 506,
    //    NotSupportedError = 507,
    //    OperationError = 508,
    //    UnexpectedError=599
    //}
    /*
    public enum MessageState
    {
        None = 0,
        Ok = 200,
        BadRequest = 400,
        Unauthorized = 401,
        Failed = 403,
        ItemNotFound = 404,
        RequestTimeout = 408,
        Unsupported = 415,
        InternalServerError = 500,
        NotImplemented = 501,
        ConnectionError = 502,
        ServiceError = 503,
        //MessageError = 503,
        TimeoutError = 504,
        NetworkError = 505,

        //ArgumentsError = 506,
        //NotSupportedError = 507,
        OperationError = 508,
        SerializeError = 510,
        SecurityError = 511,
        UnexpectedError = 599,
        Exception = -1
    }
    */
    /*
    public enum MessageState
    {
        None = 0,
        Ok = 200,
        BadRequest = 400,
        Unauthorized = 401,
        Forbidden = 403,
        NotFound = 404,
        MethodNotAllowed = 405,
        RequestTimeout = 408,
        UnsupportedMediaType = 415,
        ExpectationFailed = 417,
        Unavailable = 451,
        InternalServerError = 500,
        NotImplemented = 501,
        BadGateway = 502,
        ServiceUnavailable = 503,
        GatewayTimeout = 504,
        NetworkAuthenticationRequired = 511,
        //Unauthorized = 401,
        //Unauthorized = 401,
        //Unauthorized = 401,
        //Unauthorized = 401,

    }
    */

//598 Network read timeout error

//599 Network connect timeout error




}
