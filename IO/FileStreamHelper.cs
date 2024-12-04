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
using System.Threading;

#pragma warning disable  CS1591

namespace Nistec.IO
{
    public class FileStreamHelper
    {

        public static void GetRecursiveFiles(string path, string searchPattern, ref List<string> listFiles)
        {
            string[] filesMain = Directory.GetFiles(path, searchPattern);
            foreach (string file in filesMain)
            {
                if (listFiles.IndexOf(file) == -1)
                    listFiles.Add(file);
            }

            foreach (string localPath in Directory.GetDirectories(path))
            {
                listFiles.AddRange(Directory.GetFiles(localPath, searchPattern));
                GetRecursiveFiles(localPath, searchPattern, ref listFiles);
            }
        }

        public static bool ReplaceFile(string tempFile, string targetFile, bool deleteTempFile)
        {
            bool bStatus = false;

            if (CheckWriteAccess(targetFile))
            {
                File.Copy(tempFile, targetFile, true);
                bStatus = true;
            }

            if (deleteTempFile) File.Delete(tempFile);
            return bStatus;
        }

        public static bool CheckWriteAccess(string fileName)
        {
            int iCount = 0;
            const int iLimit = 10;
            const int iDelay = 200;

            while (iCount < iLimit)
            {
                try
                {
                    FileStream fs;
                    fs = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.None);
                    fs.Close();
                    return true;
                }
                catch
                {
                    Thread.Sleep(iDelay);
                }
                finally
                {
                    iCount += 1;
                }
            }
            return false;
        }

       
        /// <summary>
        /// Returns the names of files in a specified directories that match the specified patterns using LINQ
        /// </summary>
        /// <param name="srcDirs">The directories to seach</param>
        /// <param name="searchPatterns">the list of search patterns</param>
        /// <param name="searchOption"></param>
        /// <returns>The list of files that match the specified pattern</returns>
        public static string[] GetFiles(string[] srcDirs,
             string[] searchPatterns,
             SearchOption searchOption = SearchOption.AllDirectories)
        {
            var r = from dir in srcDirs
                    from searchPattern in searchPatterns
                    from f in Directory.GetFiles(dir, searchPattern, searchOption)
                    select f;

            return r.ToArray();
        }

        /// <summary>
        /// Save a string to file.
        /// </summary>
        /// <param name="value">String value to save.</param>
        /// <param name="fileName">File name to save to.</param>
        /// <param name="appendToFile">True - to append string to file.  Default false - overwrite file.</param>
        public static void WriteToFile(string value, string fileName, bool appendToFile)
        {
            using (StreamWriter sw = new StreamWriter(fileName, appendToFile))
            {
                sw.Write(value);
            }
        }
               
        //public static string ReadAllText(string path, Encoding encoding)
        //{
        //    if (path == null)
        //    {
        //        throw new ArgumentNullException("path");
        //    }
        //    //if (path.Length == 0)
        //    //{
        //    //    throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"));
        //    //}
        //    string result;
        //    using (StreamReader streamReader = new StreamReader(path, encoding))
        //    {
        //        result = streamReader.ReadToEnd();
        //    }
        //    return result;
        //}

        public static string ReadFile(string file)
        {
            StringBuilder sb = new StringBuilder();
            using (StreamReader sr = new StreamReader(file))
            {
                String line;
                // Read and display lines from the file until the end of 
                // the file is reached.
                while ((line = sr.ReadLine()) != null)
                {
                    sb.AppendLine(line);
                }
            }
            return sb.ToString();
        }

        #region write file stream

        public static void WriteFileStream(string filename, string value, FileMode mode, FileAccess faccess = FileAccess.Write, FileShare fshare = FileShare.None)
        {
            if (filename == null)
            {
                throw new ArgumentNullException("filename");
            }
            using (FileStream fsIn = new FileStream(filename, mode,
                faccess, fshare))
            {
                // Create an instance of StreamReader that can read 
                // characters from the FileStream.
                using (StreamWriter sw = new StreamWriter(fsIn))
                {
                    // While not at the end of the file, read lines from the file.
                    sw.Write(value);
                }
            }
        }

        public static void WriteFileStream(string filename, string value)
        {
            WriteFileStream(filename, value, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
        }

        #endregion

        #region read file stream

        public static string ReadFileStream(string filename, Encoding encoding, FileAccess faccess= FileAccess.Read, FileShare fshare= FileShare.Read)
        {
            if (filename == null)
            {
                throw new ArgumentNullException("filename");
            }
            String input = null;
            using (FileStream fsIn = new FileStream(filename, FileMode.Open,
                faccess, fshare))
            {
                // Create an instance of StreamReader that can read 
                // characters from the FileStream.
                using (StreamReader sr = new StreamReader(fsIn, encoding))
                {
                    // While not at the end of the file, read lines from the file.
                    input = sr.ReadToEnd();
                }
            }
            return input;
        }

        public static string ReadFileStream(string filename, bool detectEncodingFromByteOrderMarks, FileAccess faccess = FileAccess.Read, FileShare fshare = FileShare.Read)
        {
            if (filename == null)
            {
                throw new ArgumentNullException("filename");
            }
            String input = null;
            using (FileStream fsIn = new FileStream(filename, FileMode.Open,
                faccess, fshare))
            {
                // Create an instance of StreamReader that can read 
                // characters from the FileStream.
                using (StreamReader sr = new StreamReader(fsIn, detectEncodingFromByteOrderMarks))
                {
                    // While not at the end of the file, read lines from the file.
                    input = sr.ReadToEnd();
                }
            }
            return input;
        }

        public static string ReadFileStream(string filename, Encoding encoding)
        {
            return ReadFileStream(filename,encoding, FileAccess.Read, FileShare.Read);
        }

        public static string ReadFileStream(string filename)
        {
            return ReadFileStream(filename, true, FileAccess.Read, FileShare.Read);
        }

        public static byte[] ReadBinaryStream(string filename, Encoding encoding, FileAccess faccess = FileAccess.Read, FileShare fshare = FileShare.Read)
        {
            if (filename == null)
            {
                throw new ArgumentNullException("filename");
            }
            byte[] output = null;

            using (FileStream f = new FileStream(filename, FileMode.Open,
                faccess, fshare))
            {
                // Create an instance of BinaryReader that can 
                // read bytes from the FileStream.
                using (BinaryReader sr = new BinaryReader(f, encoding))
                {
                    // While not at the end of the file, read lines from the file.
                    output = sr.ReadBytes((int)f.Length);
                }
            }
            return output;
        }

        
        public static byte[] ReadBinaryStream(string fileName, Encoding encoding)
        {
            return ReadBinaryStream(fileName, encoding,FileAccess.Read, FileShare.Read);
        }

        public static byte[] ReadBinaryStream(string fileName)
        {
            return ReadBinaryStream(fileName, Encoding.UTF8, FileAccess.Read, FileShare.Read);
        }

        public static string GetResourceStream(System.Reflection.Assembly assembly, string fileName)
        {
            string text = null;
            Stream stream1 = assembly.GetManifestResourceStream(fileName);
            if (stream1 != null)
            {
                stream1.Position = 0;
                using (StreamReader reader1 = new StreamReader(stream1))
                {
                    text = reader1.ReadToEnd();
                }
            }
            return text;
        }

        #endregion

        #region image stream

        public static byte[] ImageToStream(string fileName)
        {
            Console.WriteLine("Processing images... ");
            //const int size = 4096;
            long t0 = Environment.TickCount;
            byte[] pixels = null;
            FileStream input = null;
            try
            {
                input = new FileStream(fileName,
                    FileMode.Open, FileAccess.Read, FileShare.Read);//,size,false);
                int len = (int)input.Length;
                pixels = new byte[len];
                input.Read(pixels, 0, len);
                long t1 = Environment.TickCount;
                Console.WriteLine("Total time processing images: {0}ms", (t1 - t0));
                return pixels;
            }
            finally
            {
                input.Close();
                input = null;
            }
        }

        public static string ImageToBase64Stream(string fileName)
        {
            try
            {
                return Convert.ToBase64String(ImageToStream(fileName));
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return null;
            }
        }

        public static string ImageToBase64Stream(System.Drawing.Image image, System.Drawing.Imaging.ImageFormat format)
        {

            byte[] bytes = null;

            try
            {
                using (System.IO.Stream stream = new System.IO.MemoryStream())
                {
                    image.Save(stream, format);
                    bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, (int)stream.Length);

                    stream.Close();
                }

                return Convert.ToBase64String(bytes);
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return null;
            }
        }

        public static System.Drawing.Image ImageFromBase64Stream(string s)
        {
            System.Drawing.Image image = null;
            byte[] bytes = System.Convert.FromBase64String(s);
            using (System.IO.Stream stream = new System.IO.MemoryStream(bytes))
            {
                image = System.Drawing.Image.FromStream(stream);
                stream.Close();
            }
            return image;
        }

        public static void ProcessImage(string fileName, string base64Stream)
        {
            Console.WriteLine("Processing images... ");
            long t0 = Environment.TickCount;
            byte[] pixels = null;
            FileStream output = null;
            try
            {
                pixels = Convert.FromBase64String(base64Stream);
                output = new FileStream(fileName,
                   FileMode.Create, FileAccess.Write, FileShare.None,
                   pixels.Length, false);
                output.Write(pixels, 0, pixels.Length);
                long t1 = Environment.TickCount;
                Console.WriteLine("Total time processing images: {0}ms", (t1 - t0));
            }
            finally
            {
                output.Close();
                output = null;
            }
        }



        #endregion

    }
}
