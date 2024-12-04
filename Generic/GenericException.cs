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
using System.Runtime.Serialization;
#pragma warning disable CS1591
namespace Nistec.Generic
{
    /// <summary>
    /// Represents errors that occur during application execution
    /// </summary>
    [Serializable]
    public class GenericException<T> : Exception
    {
        T _State;

        public T State
        {
            get { return _State; }
        }

        /// <summary>
        /// Initializes a new instance of the Exception class.
        /// </summary>
        public GenericException()
        {
            _State=default(T);
        }

        /// <summary>
        /// Initializes a new instance of the Exception class with a specified error message.
        /// </summary>
        /// <param name="ex">The message that describes the error.</param>
        public GenericException(Exception ex)
            : base(ex.Message)
        {
            _State = default(T);
        }

        /// <summary>
        /// Initializes a new instance of the Exception class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public GenericException(string message)
            : base(message)
        {
            _State = default(T);
        }

       /// <summary>
        /// Initializes a new instance of the Exception class with a specified error message and state.
       /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="state">The value that describes the error state.</param>
        public GenericException(string message, T state)
            : base(message)
        {
            _State = state;
        }

        /// <summary>
        /// Initializes a new instance of the Exception class with serialized data.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected GenericException(SerializationInfo
            info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Exception class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="state">The value that describes the error state.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public GenericException(string message, T state, Exception innerException)
            : base(message, innerException)
        {
            _State = state;
        }
    }
}
