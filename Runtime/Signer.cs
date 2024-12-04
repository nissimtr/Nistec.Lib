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
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Configuration;
using System.Collections.Generic;
using Nistec;
using System.Diagnostics;
using System.Globalization;
using System.Xml;
using System.Security.Cryptography.Xml;
using Nistec.Serialization;
#pragma warning disable  CS1591

namespace Nistec.Runtime
{
    /*
     *  var sd=Nistec.Runtime.Signer.GenerateSignature("Hello, from the .NET Docs!");
            string decded=sd.Encode();
            var sid= SignedDoc.Decode(decded);
            var text=Nistec.Runtime.Signer.VerifySignature(sid);

            Console.WriteLine(text);
    */

    public class SignedDoc
    {
        public SignedDoc(byte[] signedHash, byte[] hash, RSAParameters sharedParameters)
        {
            SignedHash = signedHash;
            Hash = hash;
            Exponent = sharedParameters.Exponent;
            Modulus = sharedParameters.Modulus;
            P = sharedParameters.P;
            Q = sharedParameters.Q;
            DP = sharedParameters.DP;
            DQ = sharedParameters.DQ;
            InverseQ = sharedParameters.InverseQ;
            D = sharedParameters.D;
        }
        public SignedDoc()
        {

        }
        public byte[] SignedHash { get; set; }
        public byte[] Hash { get; set; }
        //
        // Summary:
        //     Represents the Exponent parameter for the System.Security.Cryptography.RSA algorithm.
        public byte[] Exponent { get; set; }
        //
        // Summary:
        //     Represents the Modulus parameter for the System.Security.Cryptography.RSA algorithm.
        public byte[] Modulus { get; set; }
        //
        // Summary:
        //     Represents the P parameter for the System.Security.Cryptography.RSA algorithm.
        public byte[] P { get; set; }
        //
        // Summary:
        //     Represents the Q parameter for the System.Security.Cryptography.RSA algorithm.
        public byte[] Q { get; set; }
        //
        // Summary:
        //     Represents the DP parameter for the System.Security.Cryptography.RSA algorithm.
        public byte[] DP { get; set; }
        //
        // Summary:
        //     Represents the DQ parameter for the System.Security.Cryptography.RSA algorithm.
        public byte[] DQ { get; set; }
        //
        // Summary:
        //     Represents the InverseQ parameter for the System.Security.Cryptography.RSA algorithm.
        public byte[] InverseQ { get; set; }
        //
        // Summary:
        //     Represents the D parameter for the System.Security.Cryptography.RSA algorithm.
        public byte[] D { get; set; }

        public RSAParameters sharedParameters()
        {
            return new RSAParameters()
            {
                Exponent = Exponent,
                Modulus = Modulus,
                P = P,
                Q = Q,
                DP = DP,
                DQ = DQ,
                InverseQ = InverseQ,
                D = D
            };
        }


        public string Encode()
        {
            var bytes = BinarySerializer.SerializeToBytes(this);
            return Base64Url.EncodeBytes(bytes);
        }
        public static SignedDoc Decode(string encoded)
        {
            var bytes = Base64Url.DecodeBytes(encoded);
            return BinarySerializer.Deserialize<SignedDoc>(bytes);
        }

        public string DecodeDoc()
        {
            using (SHA256 alg = SHA256.Create())
            {
                return Encoding.UTF8.GetString(Hash);
            }
        }


        /*
        public byte[] Serialize()
        {
            return BinarySerializer.SerializeToBytes(this);
        }
        public string ToBase64TUrl()
        {
           return Base64Url.EncodeBytes(Serialize());
        }

        public static byte[] FromBase64TUrl(string base64)
        {
            return Base64Url.DecodeBytes(base64);
        }

        public SignedDoc Deserialize(byte[] bytes)
        {
            return BinarySerializer.Deserialize<SignedDoc>(bytes);
        }
        */
    }

    public class Signer
    {
        public static SignedDoc GenerateSignature(string doc) {

            byte[] signedHash;
            byte[] hash;
            RSAParameters sharedParameters;

            using (SHA256 alg = SHA256.Create())
            {
                byte[] data = Encoding.UTF8.GetBytes(doc);
                hash = alg.ComputeHash(data);


                // Generate signature
                using (RSA rsa = RSA.Create())
                {
                    sharedParameters = rsa.ExportParameters(false);

                    RSAPKCS1SignatureFormatter rsaFormatter = new RSAPKCS1SignatureFormatter(rsa);
                    rsaFormatter.SetHashAlgorithm(nameof(SHA256));

                    signedHash = rsaFormatter.CreateSignature(hash);
                }
            }

            return new SignedDoc(signedHash, hash, sharedParameters);
        }

        public static bool VerifySignature(string signedToken)
        {
            var sid = SignedDoc.Decode(signedToken);
            return VerifySignature(sid.SignedHash, sid.sharedParameters(), sid.Hash);
        }

        public static bool VerifySignature(SignedDoc doc)
        {
            return VerifySignature(doc.SignedHash, doc.sharedParameters(), doc.Hash);
        }
        public static bool VerifySignature(byte[] signedHash, RSAParameters sharedParameters, byte[] hash)
        {

            // Verify signature
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportParameters(sharedParameters);

                RSAPKCS1SignatureDeformatter rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
                rsaDeformatter.SetHashAlgorithm(nameof(SHA256));

                if (rsaDeformatter.VerifySignature(hash, signedHash))
                {
                    Console.WriteLine("The signature is valid.");
                    return true;
                }
                else
                {
                    Console.WriteLine("The signature is not valid.");
                    return false;
                }
            }
        }

        #region SignXmlDoc
        public static string SignXmlDoc(string xml)
        {
            try
            {
                // Create a new CspParameters object to specify
                // a key container.
                CspParameters cspParams = new CspParameters()
                {
                    KeyContainerName = "XML_DSIG_RSA_KEY"
                };

                // Create a new RSA signing key and save it in the container.
                RSACryptoServiceProvider rsaKey = new RSACryptoServiceProvider(cspParams);

                // Create a new XML document.
                XmlDocument xmlDoc = new XmlDocument()
                {
                    // Load an XML file into the XmlDocument object.
                    PreserveWhitespace = true
                };
                xmlDoc.LoadXml(xml);// ("test.xml");

                // Sign the XML document.
                SignXml(xmlDoc, rsaKey);

                Console.WriteLine("XML file signed.");

                // Save the document.
                //xmlDoc.Save("test.xml");

                return xmlDoc.OuterXml;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        // Sign an XML file.
        // This document cannot be verified unless the verifying
        // code has the key with which it was signed.
        public static void SignXml(XmlDocument xmlDoc, RSA rsaKey)
        {
            // Check arguments.
            if (xmlDoc == null)
                throw new ArgumentException(null, nameof(xmlDoc));
            if (rsaKey == null)
                throw new ArgumentException(null, nameof(rsaKey));

            // Create a SignedXml object.
            SignedXml signedXml = new SignedXml(xmlDoc)
            {

                // Add the key to the SignedXml document.
                SigningKey = rsaKey
            };

            // Create a reference to be signed.
            Reference reference = new Reference()
            {
                Uri = ""
            };

            // Add an enveloped transformation to the reference.
            XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(env);

            // Add the reference to the SignedXml object.
            signedXml.AddReference(reference);

            // Compute the signature.
            signedXml.ComputeSignature();

            // Get the XML representation of the signature and save
            // it to an XmlElement object.
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            // Append the element to the XML document.
            xmlDoc.DocumentElement?.AppendChild(xmlDoc.ImportNode(xmlDigitalSignature, true));
        }

        public static bool VerifyXmlDoc(string xml)
        {
            bool result = false;
            try
            {
                // Create a new CspParameters object to specify
                // a key container.
                CspParameters cspParams = new CspParameters()
                {
                    KeyContainerName = "XML_DSIG_RSA_KEY"
                };

                // Create a new RSA signing key and save it in the container.
                RSACryptoServiceProvider rsaKey = new RSACryptoServiceProvider(cspParams);

                // Create a new XML document.
                XmlDocument xmlDoc = new XmlDocument()
                {
                    // Load an XML file into the XmlDocument object.
                    PreserveWhitespace = true
                };
                xmlDoc.LoadXml(xml);// "test.xml");

                // Verify the signature of the signed XML.
                Console.WriteLine("Verifying signature...");
                result = VerifyXml(xmlDoc, rsaKey);

                // Display the results of the signature verification to
                // the console.
                if (result)
                {
                    Console.WriteLine("The XML signature is valid.");
                }
                else
                {
                    Console.WriteLine("The XML signature is not valid.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }

        // Verify the signature of an XML file against an asymmetric
        // algorithm and return the result.
        public static bool VerifyXml(XmlDocument xmlDoc, RSA key)
        {
            // Check arguments.
            if (xmlDoc == null)
                throw new ArgumentException(null, nameof(xmlDoc));
            if (key == null)
                throw new ArgumentException(null, nameof(key));

            // Create a new SignedXml object and pass it
            // the XML document class.
            SignedXml signedXml = new SignedXml(xmlDoc);

            // Find the "Signature" node and create a new
            // XmlNodeList object.
            XmlNodeList nodeList = xmlDoc.GetElementsByTagName("Signature");

            // Throw an exception if no signature was found.
            if (nodeList.Count <= 0)
            {
                throw new CryptographicException("Verification failed: No Signature was found in the document.");
            }

            // This example only supports one signature for
            // the entire XML document.  Throw an exception
            // if more than one signature was found.
            if (nodeList.Count >= 2)
            {
                throw new CryptographicException("Verification failed: More that one signature was found for the document.");
            }

            // Load the first <signature> node.
            signedXml.LoadXml((XmlElement)nodeList[0]);

            // Check the signature and return the result.
            return signedXml.CheckSignature(key);
        }
        #endregion

    }
}
