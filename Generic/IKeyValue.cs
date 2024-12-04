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
using Nistec.Serialization;

#pragma warning disable  CS1591

namespace Nistec.Generic
{

    public interface IKeyValue<T> 
    {
        T this[string key] { get; set; }
        void Clear();

        IDictionary<string, T> ToDictionary();
    }
    public interface INameValue: IKeyValue<string>
    {

    }
    public interface IKeyValue : ICollection, IEnumerable, ISerialEntity
    {
        #region collection methods

        void Add(string key, object value);
        
        void RemoveItem(string key);

        Type GetKeyType();

        Type GetValueType();

        bool Contains(string key);

        #endregion

        #region properties

        TV Get<TV>(string key, TV defaultValue);
        TV Get<TV>(string key);

        //void Set<TV>(string key, TV value);

        #endregion

        IDictionary Dictionary();
        List<KeyValuePair<string,object>> ToList();
    }
}

