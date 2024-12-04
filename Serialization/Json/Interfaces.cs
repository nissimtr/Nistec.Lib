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

namespace Nistec.Serialization
{
    public interface IJsonSerializer
    {
        string Write(object obj);

        string Write(object obj, Type baseType);

        object Read(string json, Type type);

        T Read<T>(string json);

        void WriteToken(string name, object obj, Type baseType);
        void WriteToken(string name, object obj);
        string WriteOutput(bool pretty = false);

        void ParseTo(Dictionary<string, object> d, string json);

        void ParseTo(Dictionary<string, string> d, string json);

        void ParseTo<T>(Dictionary<string, T> d, string json);

        void ParseTo<T>(List<T> d, string json);

        //IDictionary<string, object> ParseToDictionary(string json);
        //void ParseToDictionary<T>(string json, IDictionary<string, T> d);
    }

    public interface ISerialJson
    {
        /// <summary>
        /// Write entity to stream.
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="pretty"></param>
        string EntityWrite(IJsonSerializer serializer, bool pretty = false);

        /// <summary>
        /// Read entity from json.
        /// </summary>
        /// <param name="json"></param>
        /// <param name="serializer"></param>
        object EntityRead(string json, IJsonSerializer serializer);
    }
}
