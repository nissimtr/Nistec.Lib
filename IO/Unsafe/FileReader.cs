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
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
#pragma warning disable  CS1591
namespace Nistec.IO.Unsafe
{

    public class FileReader
    {
        const uint GENERIC_READ = 0x80000000;
        const uint OPEN_EXISTING = 3;
        IntPtr handle;

        [DllImport("kernel32", SetLastError = true)]
        static extern unsafe IntPtr CreateFile(
            string FileName,				// file name
            uint DesiredAccess,				// access mode
            uint ShareMode,					// share mode
            uint SecurityAttributes,		// Security Attributes
            uint CreationDisposition,		// how to create
            uint FlagsAndAttributes,		// file attributes
            int hTemplateFile				// handle to template file
            );



        [DllImport("kernel32", SetLastError = true)]
        static extern unsafe bool ReadFile(
            IntPtr hFile,					// handle to file
            void* pBuffer,				// data buffer
            int NumberOfBytesToRead,	// number of bytes to read
            int* pNumberOfBytesRead,		// number of bytes read
            int Overlapped				// overlapped buffer
            );


        [DllImport("kernel32", SetLastError = true)]
        static extern unsafe bool CloseHandle(
            IntPtr hObject   // handle to object
            );

      
        public bool Open(string FileName)
        {

            // open the existing file for reading
            handle = CreateFile(
                FileName,
                GENERIC_READ,
                0,
                0,
                OPEN_EXISTING,
                0,
                0);

            if (handle != IntPtr.Zero)
                return true;
            else
                return false;
        }

        public unsafe int Read(byte[] buffer, int index, int count)
        {
            int n = 0;
            fixed (byte* p = buffer)
            {
                if (!ReadFile(handle, p + index, count, &n, 0))
                    return 0;
            }
            return n;
        }

        public bool Close()
        {
            //close file handle
            return CloseHandle(handle);
        }

        public static string ReadFile(string filename,Encoding encoding )
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }
            if (!System.IO.File.Exists(filename))
            {
                throw new System.IO.IOException("File " + filename + " not found.");
            }

            StringBuilder sb = new StringBuilder();
            FileReader fr = null;
            try
            {

                byte[] buffer = new byte[512];
                fr = new FileReader();

                if (fr.Open(filename))
                {

                    int bytesRead;
                    do
                    {
                        bytesRead = fr.Read(buffer, 0, buffer.Length);
                        sb.Append(encoding.GetString(buffer, 0, bytesRead));
                    }
                    while (bytesRead > 0);

                    fr.Close();
                    return sb.ToString();
                }
                else
                {
                    throw new System.IO.IOException("Failed to open requested file.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (fr != null)
                {
                    fr.Close();
                }
            }
        }

        public static void ReadFile(string filename, Stream stream)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }
            if (!System.IO.File.Exists(filename))
            {
                throw new System.IO.IOException("File " + filename + " not found.");
            }

            StringBuilder sb = new StringBuilder();
            FileReader fr = null;
            try
            {

                byte[] buffer = new byte[512];
                fr = new FileReader();

                if (fr.Open(filename))
                {

                    int bytesRead;
                    do
                    {
                        bytesRead = fr.Read(buffer, 0, buffer.Length);
                        stream.Write(buffer, 0, buffer.Length);
                    }
                    while (bytesRead > 0);

                    fr.Close();
                }
                else
                {
                    throw new System.IO.IOException("Failed to open requested file.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (fr != null)
                {
                    fr.Close();
                }
            }
        }

    }

}