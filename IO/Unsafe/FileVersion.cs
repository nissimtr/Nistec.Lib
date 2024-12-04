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
using System.Reflection;
using System.Runtime.InteropServices;

#pragma warning disable  CS1591

namespace Nistec.IO.Unsafe
{
    public class FileVersion
    {


        unsafe public static int GetVersion(string filename, out string info)
        {
            info = null;
            try
            {
                int handle = 0;
                // Figure out how much version info there is:
                int size =
                    Win32Imports.GetFileVersionInfoSize(filename, out handle);

                if (size == 0) return -1;

                byte[] buffer = new byte[size];

                if (!Win32Imports.GetFileVersionInfo(filename, handle, size, buffer))
                {
                    Console.WriteLine("Failed to query file version information.");
                    return 1;
                }

                short* subBlock = null;
                uint len = 0;
                // Get the locale info from the version info:
                if (!Win32Imports.VerQueryValue(buffer, @"\VarFileInfo\Translation", out subBlock, out len))
                {
                    Console.WriteLine("Failed to query version information.");
                    return 1;
                }

                string spv = @"\StringFileInfo\" + subBlock[0].ToString("X4") + subBlock[1].ToString("X4") + @"\ProductVersion";

                byte* pVersion = null;
                // Get the ProductVersion value for this program:
                string versionInfo;

                if (!Win32Imports.VerQueryValue(buffer, spv, out versionInfo, out len))
                {
                    Console.WriteLine("Failed to query version information.");
                    return 1;
                }
                info = versionInfo;
                Console.WriteLine("ProductVersion == {0}", versionInfo);
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught unexpected exception " + e.Message);
                return -1;
            }

            return 0;
        }
    }
    public class Win32Imports
    {
        [DllImport("version.dll")]
        public static extern bool GetFileVersionInfo(string sFileName,
            int handle, int size, byte[] infoBuffer);
        [DllImport("version.dll")]
        public static extern int GetFileVersionInfoSize(string sFileName,
            out int handle);

        // The 3rd parameter - "out string pValue" - is automatically
        // marshaled from Ansi to Unicode:
        [DllImport("version.dll")]
        unsafe public static extern bool VerQueryValue(byte[] pBlock,
            string pSubBlock, out string pValue, out uint len);
        // This VerQueryValue overload is marked with 'unsafe' because 
        // it uses a short*:
        [DllImport("version.dll")]
        unsafe public static extern bool VerQueryValue(byte[] pBlock,
            string pSubBlock, out short* pValue, out uint len);

    }

}