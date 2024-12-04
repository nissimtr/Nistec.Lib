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
using System.Xml;
using System.Xml.Serialization;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.CodeDom;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Text.RegularExpressions;
#pragma warning disable  CS1591
namespace Nistec.Xml
{


    public class XmlUtil
    {
    
        /// <summary>
        ///Serialize to xml using string builder and property descriptor
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Serialize(object obj)
        {
            StringBuilder xml = new StringBuilder();
            xml.Append("<");
            xml.Append(obj.GetType().FullName);
            xml.Append(">");

            // Now, walk all the properties of the object.
            PropertyDescriptorCollection properties;
            //PropertyDescriptor  p;

            properties = TypeDescriptor.GetProperties(obj);

            foreach (PropertyDescriptor p in properties)
            {
                if (!p.ShouldSerializeValue(obj))
                {
                    continue;
                }

                object value = p.GetValue(obj);
                Type valueType = null;
                if (value != null) valueType = value.GetType();

                // You have a valid property to write.
                xml.AppendFormat("<{0}>{1}</{0}>", p.Name, value);
            }

            xml.AppendFormat("</{0}>", obj.GetType().FullName);
            return xml.ToString();
        }

        public abstract class XmlSerializerBase
        {
            public abstract string Serialize(
          IDesignerSerializationManager m,
         object obj);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeConvertible( IDesignerSerializationManager m, object obj)
        {

            if (obj == null) return string.Empty;

            IConvertible c = obj as IConvertible;
            if (c == null)
            {
                // Rather than throwing exceptions, add a list of errors 
                // to the serialization manager.
                m.ReportError("Object is not IConvertible");
                return null;
            }

            return c.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
       
        /// <summary>
        /// Serialize to xml using string builder and property descriptor
        /// </summary>
        /// <param name="m"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Serialize( IDesignerSerializationManager m,  object obj)
        {

            StringBuilder xml = new StringBuilder();
            xml.Append("<");
            xml.Append(obj.GetType().FullName);
            xml.Append(">");

            // Now, walk all the properties of the object.
            PropertyDescriptorCollection properties;
            //PropertyDescriptor  p;

            properties = TypeDescriptor.GetProperties(obj);

            foreach (PropertyDescriptor p in properties)
            {
                if (!p.ShouldSerializeValue(obj))
                {
                    continue;
                }

                object value = p.GetValue(obj);
                Type valueType = null;
                if (value != null) valueType = value.GetType();

                // Get the serializer for this property
                XmlSerializerBase s = m.GetSerializer(
                    valueType,
                    typeof(XmlSerializerBase)) as XmlSerializerBase;

                if (s == null)
                {
                    // Because there is no serializer, 
                    // this property must be passed over.  
                    // Tell the serialization manager
                    // of the error.
                    m.ReportError(string.Format(
                        "Property {0} does not support XML serialization",
                        p.Name));
                    continue;
                }

                // You have a valid property to write.
                xml.AppendFormat("<{0}>", p.Name);
                xml.Append(s.Serialize(m, value));
                xml.AppendFormat("</{0}>", p.Name);

            }

            xml.AppendFormat("</{0}>", obj.GetType().FullName);
            return xml.ToString();
        }




  	
        public static DataSet ReadXmlFile(string file)
         {
             try
             {
                 System.Data.DataSet DSet = new DataSet();
                 DSet.ReadXml(file, XmlReadMode.Auto);
                 return DSet;
             }
             catch (ApplicationException ex)
             {
                 throw new ApplicationException(ex.Message);
             }
         }

        public static DataSet ReadXmlStream(string s)
         {
             try
             {
                 StringReader stream = new StringReader(s);
                 DataSet DSet = new DataSet();
                 XmlTextReader reader = new XmlTextReader(stream);
                 DSet.ReadXml(reader, XmlReadMode.Auto);

                 return DSet;
             }
             catch (ApplicationException ex)
             {
                 throw new ApplicationException(ex.Message);
             }
         }

        public static string NormelaizeXml(string xml)
        {
            Regex regex = new Regex(@">\s*<");
            xml = regex.Replace(xml, "><");
            return xml.Replace("\r\n", "").Replace("\n", "").Trim();
        }
    }

}
