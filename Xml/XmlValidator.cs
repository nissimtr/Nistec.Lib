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
using System.Xml.Schema;
#pragma warning disable CS1591
namespace Nistec.Xml
{
    public class XmlValidator
    {
        public XmlValidator(XmlReaderSettings settings)
        {
            this.settings = settings;        
        }
        /// <summary>
        /// Ctor using default settings
        /// </summary>
        public XmlValidator()
        {
            settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreWhitespace = true;
            //settings.ProhibitDtd = true;
            settings.ValidationType = ValidationType.DTD;
            settings.DtdProcessing = DtdProcessing.Prohibit;
        }

        XmlReaderSettings settings;
        StringBuilder errorMessages;
        bool isValid;
        string result;


        /// <summary>
        /// Get Validate result
        /// </summary>
        public string Result
        {
            get { return result; }
        }

 
        /// <summary>
        /// Validate xml 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public bool ValidateDTD(string url)
        {
            try
            {
                XmlTextReader tr = new XmlTextReader(url);

                return ValidateDTD(tr);
            }
            catch (Exception ex)
            {
                result = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Validate xml 
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="DTD"></param>
        /// <returns></returns>
        public bool ValidateDTD(string xml, string DTD)
        {
            try
            {
                if (xml.StartsWith("<?xml"))
                {
                    int startIndx = xml.IndexOf('<', 0);
                    int endIndx = xml.IndexOf('>', startIndx);
                    xml = xml.Remove(startIndx, 1 + endIndx - startIndx);
                }
                string xmlv = DTD + xml;

                XmlTextReader tr = new XmlTextReader(xmlv, XmlNodeType.Document, null);// ("HeadCount.xml");

                return ValidateDTD(tr);

            }
            catch (Exception ex)
            {
                result = ex.Message;
                return false;
            }
        }
        /// <summary>
        /// Validate xml 
        /// </summary>
        /// <param name="tr"></param>
        /// <returns></returns>
        public bool ValidateDTD(XmlTextReader tr)
        {
            errorMessages = new StringBuilder();
            isValid = true;
            try
            {

                XmlReader vr = XmlReader.Create(tr,settings);

                while (vr.Read()) ;

                if (isValid)
                    result = "Validation finished";
                else
                    result = errorMessages.ToString();

            }
            catch (Exception ex)
            {
                result = ex.Message;
                isValid = false;
            }
            return isValid;

        }

        void ValidationHandler(object sender, ValidationEventArgs args)
        {
            isValid = false;
            errorMessages.Append(args.Message);
        }

    }
}
