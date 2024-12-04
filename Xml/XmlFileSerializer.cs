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
using System.Xml.Serialization;	 
using System.Runtime.Serialization.Formatters.Binary; 
using System.IO;				 
using System.ComponentModel;	 
using System.IO.IsolatedStorage; 
using System.Text;
using System.Xml;
#pragma warning disable  CS1591
namespace Nistec.Xml
{

   
    public class XmlFileSerializer
    {

        public enum SerializedFormatType
        {
            Binary, Document
        }

        #region Serializer class

        /// <summary>
        /// Constructor for this class.
        /// </summary>
        public XmlFileSerializer()
        {
        }

        /// <summary>
        /// Load an object from an Xml file that is in an Xml Document format.
        /// <newpara></newpara>
        /// <example>
        /// See Load method that uses the SerializedFormatType argument for more information.
        /// </example>
        /// </summary>
        public virtual Object Load(Object obj, string XmlFilePathName)
        {
            obj = this.LoadFromDocumentFormat(obj, XmlFilePathName, null);
            return obj;
        }

        /// <summary>
        /// Load an object from an Xml file that is in the specified format.
        /// <newpara></newpara>
        /// </summary>
        /// <param name="obj">Object to be loaded.</param>
        /// <param name="XmlFilePathName">File Path name of the Xml file containing object(s) serialized to Xml.</param>
        /// <param name="SerializedFormat">Xml serialized format to load the object from.</param>
        /// <returns>Returns an Object loaded from the Xml file. If the Object could not be loaded returns null.</returns>
        public virtual Object Load(Object obj, string XmlFilePathName, SerializedFormatType SerializedFormat)
        {
            switch (SerializedFormat)
            {
                case SerializedFormatType.Binary:
                    obj = this.LoadFromBinaryFormat(obj, XmlFilePathName, null);
                    break;

                case SerializedFormatType.Document:
                default:
                    obj = this.LoadFromDocumentFormat(obj, XmlFilePathName, null);
                    break;
            }

            return obj;
        }

        public virtual Object Load(Object obj, string XmlFilePathName,
            SerializedFormatType SerializedFormat, IsolatedStorageFile isolatedStorageFolder)
        {
            switch (SerializedFormat)
            {
                case SerializedFormatType.Binary:
                    obj = this.LoadFromBinaryFormat(obj, XmlFilePathName, isolatedStorageFolder);
                    break;

                case SerializedFormatType.Document:
                default:
                    obj = this.LoadFromDocumentFormat(obj, XmlFilePathName, isolatedStorageFolder);
                    break;
            }

            return obj;
        }

        /// <summary>
        /// Load an object from an Xml file that is in an Xml Document format, at a Isolated storage location.
        /// </summary>
        /// <param name="obj">Object to be loaded.</param>
        /// <param name="XmlFilePathName">File name (no path) of the Xml file containing object(s) serialized to Xml.</param>
        /// <param name="isolatedStorageFolder">Isolated Storage object that is a user and assembly specific folder location
        /// from which to Load the Xml file.</param>
        /// <returns>Returns an Object loaded from the Xml file. If the Object could not be loaded returns null.</returns>
        public virtual Object Load(Object obj, string XmlFilePathName, IsolatedStorageFile isolatedStorageFolder)
        {
            obj = this.LoadFromDocumentFormat(obj, XmlFilePathName, isolatedStorageFolder);
            return obj;
        }

        private Object LoadFromBinaryFormat(Object obj,
            string XmlFilePathName, IsolatedStorageFile isolatedStorageFolder)
        {
            FileStream fileStream = null;

            try
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();

                if (isolatedStorageFolder == null)
                    fileStream = new FileStream(XmlFilePathName, FileMode.Open);
                else
                    fileStream = new IsolatedStorageFileStream(XmlFilePathName, FileMode.Open, isolatedStorageFolder);

                obj = binaryFormatter.Deserialize(fileStream);
            }
            finally
            {
                //Make sure to close the file even if an exception is raised...
                if (fileStream != null)
                    fileStream.Close();
            }

            return obj;
        }

        private Object LoadFromDocumentFormat(Object obj,
            string XmlFilePathName, IsolatedStorageFile isolatedStorageFolder)
        {
            TextReader txrTextReader = null;

            try
            {
                Type ObjectType = obj.GetType();

                XmlSerializer xserDocumentSerializer = new XmlSerializer(ObjectType);

                if (isolatedStorageFolder == null)
                    txrTextReader = new StreamReader(XmlFilePathName);
                else
                    txrTextReader = new StreamReader(new IsolatedStorageFileStream(XmlFilePathName, FileMode.Open, isolatedStorageFolder));

                obj = xserDocumentSerializer.Deserialize(txrTextReader);
            }
            finally
            {
                //Make sure to close the file even if an exception is raised...
                if (txrTextReader != null)
                    txrTextReader.Close();
            }

            return obj;
        }

        /// <summary>
        /// Save an object to an Xml file that is in an Xml Document format.
        /// <newpara></newpara>
        /// <example>
        /// See Save method that uses the SerializedFormatType argument for more information.
        /// </example>
        /// </summary>
        public virtual bool Save(Object ObjectToSave, string XmlFilePathName)
        {
            bool success = false;
            success = this.SaveToDocumentFormat(ObjectToSave, XmlFilePathName, null);
            return success;
        }

        /// <summary>
        /// Save an object to an Xml file that is in the specified format.
        /// <newpara></newpara>
        /// </summary>
        /// <param name="ObjectToSave">Object to be saved.</param>
        /// <param name="XmlFilePathName">File Path name of the Xml file to contain the object serialized to Xml.</param>
        /// <param name="SerializedFormat">Xml serialized format to load the object from.</param>
        /// <returns>Returns success of the object save.</returns>
        public virtual bool Save(Object ObjectToSave, string XmlFilePathName, SerializedFormatType SerializedFormat)
        {
            bool success = false;

            switch (SerializedFormat)
            {
                case SerializedFormatType.Binary:
                    success = this.SaveToBinaryFormat(ObjectToSave, XmlFilePathName, null);
                    break;

                case SerializedFormatType.Document:
                default:
                    success = this.SaveToDocumentFormat(ObjectToSave, XmlFilePathName, null);
                    break;
            }

            return success;
        }

        public virtual bool Save(Object ObjectToSave, string XmlFilePathName,
            SerializedFormatType SerializedFormat, IsolatedStorageFile isolatedStorageFolder)
        {
            bool success = false;

            switch (SerializedFormat)
            {
                case SerializedFormatType.Binary:
                    success = this.SaveToBinaryFormat(ObjectToSave, XmlFilePathName, isolatedStorageFolder);
                    break;

                case SerializedFormatType.Document:
                default:
                    success = this.SaveToDocumentFormat(ObjectToSave, XmlFilePathName, isolatedStorageFolder);
                    break;
            }

            return success;
        }


        /// <summary>
        /// Save an object to an Xml file that is in an Xml Document forward, at a Isolated storage location.
        /// </summary>
        /// <param name="ObjectToSave">Object to be saved.</param>
        /// <param name="XmlFilePathName">File name (no path) of the Xml file to contain the object serialized to Xml.</param>
        /// <param name="isolatedStorageFolder">Isolated Storage object that is a user and assembly specific folder location
        /// from which to save the Xml file.</param>
        /// <returns></returns>
        public virtual bool Save(Object ObjectToSave, string XmlFilePathName, IsolatedStorageFile isolatedStorageFolder)
        {
            bool success = false;
            success = this.SaveToDocumentFormat(ObjectToSave, XmlFilePathName, isolatedStorageFolder);
            return success;
        }

        private bool SaveToDocumentFormat(Object ObjectToSave,
            string XmlFilePathName, IsolatedStorageFile isolatedStorageFolder)
        {
            TextWriter textWriter = null;
            bool success = false;

            try
            {
                Type ObjectType = ObjectToSave.GetType();

                //Create serializer object using the type name of the Object to serialize.
                XmlSerializer xmlSerializer = new XmlSerializer(ObjectType);

                if (isolatedStorageFolder == null)
                    textWriter = new StreamWriter(XmlFilePathName);
                else
                    textWriter = new StreamWriter(new IsolatedStorageFileStream(XmlFilePathName, FileMode.OpenOrCreate, isolatedStorageFolder));

                xmlSerializer.Serialize(textWriter, ObjectToSave);

                success = true;
            }
            finally
            {
                //Make sure to close the file even if an exception is raised...
                if (textWriter != null)
                    textWriter.Close();
            }

            return success;
        }

        private bool SaveToBinaryFormat(Object ObjectToSave,
            string XmlFilePathName, IsolatedStorageFile isolatedStorageFolder)
        {
            FileStream fileStream = null;
            bool success = false;

            try
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();

                if (isolatedStorageFolder == null)
                    fileStream = new FileStream(XmlFilePathName, FileMode.OpenOrCreate);
                else
                    fileStream = new IsolatedStorageFileStream(XmlFilePathName, FileMode.OpenOrCreate, isolatedStorageFolder);

                binaryFormatter.Serialize(fileStream, ObjectToSave);

                success = true;
            }
            finally
            {
                //Make sure to close the file even if an exception is raised...
                if (fileStream != null)
                    fileStream.Close();
            }

            return success;
        }
        #endregion

    }

}
