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
    public enum JsonFormat
    {
        None,
        Indented
    }
    public enum JsonDateFormat
    {
        iso,//yyyy-MM-ddThh:mm:ss
        iso_hhmm,//yyyy-MM-ddThh:mm:ss
        iso_short,//yyyy-MM-ddThh:mm:ss
        ddmmyyyy,
        ddmmyyyy_hhmm,
        ddmmyyyy_hhmmss,
        mmddyyyy,
        mmddyyyy_hhmm,
        mmddyyyy_hhmmss,
        longDate,
        dynamic
        // short date pattern d: "M/d/yyyy",
        // long date pattern D: "dddd, MMMM dd, yyyy",
        // short time pattern t: "h:mm tt",
        // long time pattern T: "h:mm:ss tt",
        // long date, short time pattern f: "dddd, MMMM dd, yyyy h:mm tt",
        // long date, long time pattern F: "dddd, MMMM dd, yyyy h:mm:ss tt",
        // month/day pattern M: "MMMM dd",
        // month/year pattern Y: "yyyy MMMM",
        // S is a sortable format that does not vary by culture S: "yyyy\u0027-\u0027MM\u0027-\u0027dd\u0027T\u0027HH\u0027:\u0027mm\u0027:\u0027ss"

    }

    public enum JsonSerializerMode
    {
        Write,
        Read,
        Both
    }
  
}
