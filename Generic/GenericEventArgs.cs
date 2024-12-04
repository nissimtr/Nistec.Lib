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
using System.Text;

#pragma warning disable  CS1591

namespace Nistec.Generic
{
    #region GenericEventArgs

    public delegate void GenericEventHandler<T>(object sender, GenericEventArgs<T> e);
    public delegate void GenericEventHandler<T1,T2>(object sender, GenericEventArgs<T1, T2> e);
    public delegate void GenericEventHandler<T1, T2,T3>(object sender, GenericEventArgs<T1, T2,T3> e);

    public class GenericEventArgs<T> : EventArgs
    {
        public readonly T Args;
        public readonly int State;

        public GenericEventArgs(T arg)
        {
            Args = arg;
        }
        public GenericEventArgs(T arg, int state)
        {
            Args = arg;
            State = state;
        }
    }

    public class GenericEventArgs<T1,T2>:EventArgs
    {
        public readonly T1 Args1;
        public readonly T2 Args2;
        
        public GenericEventArgs(T1 arg)
        {
            Args1 = arg;
        }
        public GenericEventArgs(T1 arg1, T2 arg2)
        {
            Args1 = arg1;
            Args2 = arg2;
        }

    }
    
    public class GenericEventArgs<T1, T2,T3> : EventArgs
    {
        public readonly T1 Args1;
        public readonly T2 Args2;
        public readonly T3 Args3;

        public GenericEventArgs(T1 arg)
        {
            Args1 = arg;
        }
        public GenericEventArgs(T1 arg1, T2 arg2)
        {
            Args1 = arg1;
            Args2 = arg2;
        }
        public GenericEventArgs(T1 arg1, T2 arg2, T3 arg3)
        {
            Args1 = arg1;
            Args2 = arg2;
            Args3 = arg3;
        }
    }
    #endregion

 }
