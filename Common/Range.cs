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
using System.Data;
using System.Runtime.InteropServices;
#pragma warning disable  CS1591
namespace Nistec//.Data
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Range
    {
        private int min;
        private int max;
        private bool isNotNull;

        public static Range Empty
        {
            get { return new Range(0, 0); }
        }
        public Range(int min, int max)
        {
            if (min > max)
            {
                throw new ArgumentOutOfRangeException();
            }
            this.min = min;
            this.max = max;
            this.isNotNull = true;
        }

        public int Count
        {
            get
            {
                if (this.IsNull)
                {
                    return 0;
                }
                return ((this.max - this.min) + 1);
            }
        }
        public bool IsNull
        {
            get
            {
                return !this.isNotNull;
            }
        }
        public int Max
        {
            get
            {
                this.CheckNull();
                return this.max;
            }
        }
        public int Min
        {
            get
            {
                this.CheckNull();
                return this.min;
            }
        }
        internal void CheckNull()
        {
            if (this.IsNull)
            {
                throw new ArgumentNullException();
            }
        }
    }

}
