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
using System.Net;
using System.IO;
using System.Threading;
using System.Web.Util;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

using System.Security.Cryptography;
using System.Security.Permissions;
#pragma warning disable  CS1591

namespace Nistec.Web
{
    /// <summary>
    /// HttpRequest
    /// </summary>
    public class HttpUtil
    {
        #region members
        /// <summary>
        /// Http WebRequest
        /// </summary>
        private HttpWebRequest request;
        /// <summary>
        /// postData
        /// </summary>
        private string m_postData;
        /// <summary>
        /// AsyncWorker
        /// </summary>
        public event EventHandler AsyncWorker;
        /// <summary>
        /// ManualReset
        /// </summary>
        public ManualResetEvent ManualReset = new ManualResetEvent(false);
        /// <summary>
        /// m_codePage
        /// </summary>
        private string m_CodePage;
        /// <summary>
        /// m_CodePageNum
        /// </summary>
        private int m_CodePageNum;
        /// <summary>
        /// ContentType
        /// </summary>
        private string m_ContentType;

        private bool m_IsUrlEncoded;

        private bool m_IsXml;

        private string m_url;

        private WebExceptionStatus m_WebExceptionStatus;

        private HttpStatusCode m_HttpStatusCode;

        private string m_HttpStatusDescription;

        private WebException m_Exception;

        //public bool ContentLengthByByte = false;
        #endregion

        #region ctor
        /// <summary>
        /// Initialized new instance of HttpRequest class
        /// </summary>
        /// <param name="url"></param>
        public HttpUtil(string url):this(url,"POST")
        {
        }
        /// <summary>
        /// Initialized new instance of HttpRequest class
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        public HttpUtil(string url, string method)
        {
            request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            m_url = url;
            m_ContentType = "application/x-www-form-urlencoded";
            m_CodePage = "utf-8";
            m_IsUrlEncoded = false;
            m_IsXml = false;
        }

        /// <summary>
        /// Initialized new instance of HttpRequest class
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="codePage"></param>
        /// <param name="isXml"></param>
        public HttpUtil(string url, string method, string codePage, bool isXml)
        {
            request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            m_url = url;
            if (isXml)
                m_ContentType = "text/xml; charset=" + codePage;
            else
                m_ContentType = "application/x-www-form-urlencoded";
            m_CodePage = codePage;
            m_IsXml = isXml;
        }


        #endregion

        #region properties
        /// <summary>
        /// Get HttpWebRequest
        /// </summary>
        public HttpWebRequest HttpWebRequest
        {
            get
            {
                return request;
            }
        }

        /// <summary>
        /// Get or Set UrlEncoded
        /// </summary>
        public bool IsUrlEncoded
        {
            get
            {
                return m_IsUrlEncoded;
            }
            set { m_IsUrlEncoded = value; }
        }
        /// <summary>
        /// Get or Set IsXml
        /// </summary>
        public bool IsXml
        {
            get
            {
                return m_IsXml;
            }
            set { m_IsXml = value; }
        }
        /// <summary>
        /// Get or Set ContentType
        /// </summary>
        public string ContentType
        {
            get
            {
                return m_ContentType;
            }
            set { m_ContentType = value; }
        }
        /// <summary>
        /// Get the WebExceptionStatus
        /// </summary>
        public WebExceptionStatus ExceptionStatus
        {
            get
            {
                return m_WebExceptionStatus;
            }
        }
        /// <summary>
        /// Get the HttpStatusCode
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get
            {
                return m_HttpStatusCode;
            }
        }
        /// <summary>
        /// Get the HttpStatus Description
        /// </summary>
        public string HttpStatusDescription
        {
            get
            {
                return m_HttpStatusDescription;
            }
        }

        /// <summary>
        /// Get the WebException
        /// </summary>
        public WebException HttpWebException
        {
            get
            {
                return m_Exception;
            }
        }

        #endregion

        #region private handles

        private string HandleWebExcption(WebException webExcp)
        {
            Console.WriteLine("A WebException has been caught.");
            m_Exception = webExcp;
            m_WebExceptionStatus = webExcp.Status;


            // If status is WebExceptionStatus.ProtocolError, 
            //   there has been a protocol error and a WebResponse 
            //   should exist. Display the protocol error.
            if (m_WebExceptionStatus == WebExceptionStatus.ProtocolError)
            {
                Console.Write("The server returned protocol error ");
                // Get HttpWebResponse so that you can check the HTTP status code.

                HttpWebResponse httpResponse = null;
                StreamReader resStream = null;
                try
                {
                    httpResponse = (HttpWebResponse)webExcp.Response;
                    m_HttpStatusCode = httpResponse.StatusCode;
                    m_HttpStatusDescription = httpResponse.StatusDescription;

                    resStream = new StreamReader(httpResponse.GetResponseStream(), Encoding.GetEncoding(m_CodePage));//.UTF8);
                    return resStream.ReadToEnd();
                }
                catch { }
                finally
                {
                    if (httpResponse != null)
                        httpResponse.Close();
                    if (resStream != null)
                        resStream.Close();
                    httpResponse = null;
                    resStream = null;
                }
            }
            return null;
        }

        private byte[] GetByte(string postData)
        {
            byte[] byteArray = null;

            // Convert the string into a byte array.
            if (m_CodePageNum > 0)
            {
                byteArray = Encoding.GetEncoding(m_CodePageNum).GetBytes(postData);//.UTF8.GetBytes(postData);
            }
            else
            {
                byteArray = Encoding.GetEncoding(m_CodePage).GetBytes(postData);//.UTF8.GetBytes(postData);
            }
            return byteArray;
        }
        #endregion

        #region Async
        /// <summary>
        /// OnAsyncWorker
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnAsyncWorker(EventArgs e)
        {
            if (AsyncWorker != null)
                AsyncWorker(this, e);
        }

 

        /// <summary>
        /// Async Request with empty code page
        /// </summary>
        /// <param name="postData"></param>
        /// <returns></returns>
        public string AsyncRequest(string postData)
        {
            return AsyncRequest(postData, "");
        }

        /// <summary>
        /// Async Request UTF8
        /// </summary>
        /// <param name="postData"></param>
        /// <returns></returns>
        public string AsyncRequestUTF8(string postData)
        {
            return AsyncRequest(postData, "utf-8");
        }

        /// <summary>
        /// Async Request
        /// </summary>
        /// <param name="postData"></param>
        /// <param name="codePage"></param>
        /// <returns></returns>
        public string AsyncRequest(string postData, string codePage)
        {
            Stream streamResponse = null;
            StreamReader streamRead = null;
            HttpWebResponse response = null;
            string result = null; 

            try
            {
                m_postData = postData;
                // Create a new HttpWebRequest object.
                if (!string.IsNullOrEmpty(codePage))
                {
                    m_ContentType += "; charset = " + codePage;
                    m_CodePage = codePage;
                }

                // Set the ContentType property. 
                request.ContentType = m_ContentType;

                // Start the asynchronous operation.    
                request.BeginGetRequestStream(new AsyncCallback(ReadCallback), request);

                OnAsyncWorker(EventArgs.Empty);

                // Keep the main thread from continuing while the asynchronous
                // operation completes. A real world application
                // could do something useful such as updating its user interface. 
                ManualReset.WaitOne();

                // Get the response.
                response = (HttpWebResponse)request.GetResponse();
                streamResponse = response.GetResponseStream();
                streamRead = new StreamReader(streamResponse);
                result = streamRead.ReadToEnd();
               
                m_WebExceptionStatus = WebExceptionStatus.Success;

            }
            catch (System.Net.WebException webExcp)
            {
                result = HandleWebExcption(webExcp);
            }
            catch (Exception ex)
            {
                m_WebExceptionStatus = WebExceptionStatus.UnknownError;
                throw ex;
            }
            finally
            {
                // Close the stream object.
                if (streamResponse != null)
                    streamResponse.Close();
                if (streamRead != null)
                    streamRead.Close();
                // Release the HttpWebResponse.
                if (response != null)
                    response.Close();
                streamResponse = null;
                streamRead = null;
                response = null;
            }
            return result;
        }


        /// <summary>
        /// AsyncRequest
        /// </summary>
        /// <param name="postData"></param>
        /// <param name="codePage"></param>
        /// <returns></returns>
        public string AsyncRequest(string postData, int codePage)
        {
            Stream streamResponse = null;
            StreamReader streamRead = null;
            HttpWebResponse response = null;
            string result = null;
            try
            {

                m_postData = postData;
                
                m_CodePageNum = codePage;
                m_ContentType += "; charset = " + codePage.ToString();

                // Set the ContentType property. 
                request.ContentType = m_ContentType;

                // Start the asynchronous operation.    
                request.BeginGetRequestStream(new AsyncCallback(ReadCallback), request);

                OnAsyncWorker(EventArgs.Empty);

                // Keep the main thread from continuing while the asynchronous
                // operation completes. A real world application
                // could do something useful such as updating its user interface. 
                ManualReset.WaitOne();

                // Get the response.
                response = (HttpWebResponse)request.GetResponse();
                streamResponse = response.GetResponseStream();
                streamRead = new StreamReader(streamResponse);

                result = streamRead.ReadToEnd();
              

                m_WebExceptionStatus = WebExceptionStatus.Success;

            }
            catch (System.Net.WebException webExcp)
            {
                result = HandleWebExcption(webExcp);
            }
            catch (Exception ex)
            {
                m_WebExceptionStatus = WebExceptionStatus.UnknownError;
                throw ex;
            }
            finally
            {
                // Close the stream object.
                if (streamResponse != null)
                    streamResponse.Close();
                if (streamRead != null)
                    streamRead.Close();
                // Release the HttpWebResponse.
                if (response != null)
                    response.Close();
                streamResponse = null;
                streamRead = null;
                response = null;
            }
            return result;
        }

        /// <summary>
        /// ReadCallback
        /// </summary>
        /// <param name="asynchronousResult"></param>
        private void ReadCallback(IAsyncResult asynchronousResult)
        {
            Stream postStream = null;
            //bool shouldManualReset = false;
            try
            {
                HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;
                // End the operation.
                postStream = request.EndGetRequestStream(asynchronousResult);

                
                byte[] byteArray = GetByte(m_postData); 

                
                postStream.Write(byteArray, 0, m_postData.Length);
              
            }
           
            catch //(Exception ex)
            {
                //throw ex;
            }
            finally
            {
                if (postStream != null)
                    postStream.Close();
                postStream = null;
                //if (shouldManualReset)
                //{
                    ManualReset.Set();
                //}
            }
        }
        #endregion

        #region Sync
        /// <summary>
        /// Send HttpWebRequest
        /// </summary>
        /// <param name="postData"></param>
        /// <returns></returns>
        public string DoRequest(string postData)
        {
            return DoRequest(postData, "utf-8");
        }

        /// <summary>
        /// DoRequest
        /// </summary>
        /// <param name="postData"></param>
        /// <param name="codePage"></param>
        /// <param name="maxRetry"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public string DoRequest(string postData, string codePage, int maxRetry, int delay)
        {
            int retry = 0;
            string result = null;
            do
            {
                try
                {
                    retry++;
                    if (retry > 1)
                    {
                        request = (HttpWebRequest)WebRequest.Create(m_url);
                        m_HttpStatusCode = HttpStatusCode.OK;
                        m_HttpStatusDescription = "";
                    }
                    result = DoRequest(postData, codePage);

                    if (ExceptionStatus == WebExceptionStatus.Success)
                        return result;

                    if (retry < maxRetry)
                    {
                        Thread.Sleep(delay);
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            } while (/*exc != null &&*/ retry < maxRetry);

         
            return result;

        }

        /// <summary>
        /// Send HttpWebRequest
        /// </summary>
        /// <param name="postData"></param>
        /// <param name="codePage">codePage</param>
        /// <returns></returns>
        public string DoRequest(string postData, string codePage)
        {

            return DoRequest(m_url, postData, "POST", codePage, ContentType, IsUrlEncoded, 120000);
        }

  

         #endregion
 
        #region static
  
        /// <summary>
        /// Send HttpWebRequest
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="codePage">codePage</param>
        /// <returns></returns>
        public static string DoGet(string url, string postData, string codePage)
        {
            Stream receiveStream = null;
            StreamReader readStream = null;
            try
            {

                string qs = string.IsNullOrEmpty(postData) ? "" : "?" + postData;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + qs);
                request.Method = "GET";

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                // Get the stream associated with the response.
                receiveStream = response.GetResponseStream();

                // Pipes the stream to a higher level stream reader with the required encoding format. 
                readStream = new StreamReader(receiveStream, Encoding.GetEncoding(codePage));//.UTF8);

                string result = readStream.ReadToEnd();
                response.Close();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (receiveStream != null)
                    receiveStream.Close();
                if (readStream != null)
                    readStream.Close();
            }
        }

        /// <summary>
        /// Do post as "application/x-www-form-urlencoded"
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static string DoPost(string url, string postData, int timeout)
        {
            return DoRequest(url, postData, "post", "utf-8", "", true, timeout);
        }
        public static string DoPost(string url, string postData, string encoding, string contentType, bool isUrlEncoded, int timeout)
        {
            return DoRequest(url, postData, "post", encoding, contentType, isUrlEncoded, timeout);
        }
        public static string DoRequest(string url, string postData, string method, string encoding)
        {
            return DoRequest(url, postData, method, encoding, null, false, 100000);
        }
        public static string DoRequest(string url, string postData, string method, string encoding,  int timeout)
        {
            return DoRequest(url, postData, method, encoding, null, false, timeout);
        }

        public static string DoRequest(string url, string postData,string method, string encoding, string contentType, bool isUrlEncoded, int timeout)
        {

            string response = null;

            WebRequest request = null;
            Stream newStream = null;
            Stream receiveStream = null;
            StreamReader readStream = null;
            WebResponse wresponse = null;

            try
            {
                Encoding enc = Encoding.GetEncoding(encoding);
                if (isUrlEncoded)
                {
                    postData = System.Web.HttpUtility.UrlEncode(postData, enc);
                    if (string.IsNullOrEmpty(contentType))
                    {
                        contentType = "application/x-www-form-urlencoded";
                    }
                }

                if (method.ToUpper() == "GET")
                {
                    string qs = string.IsNullOrEmpty(postData) ? "" : "?" + postData;

                    request = WebRequest.Create(url + qs);
                    request.Timeout = timeout <= 0 ? 100000 : timeout;
                    if (!string.IsNullOrEmpty(contentType))
                        request.ContentType = contentType;// string.IsNullOrEmpty(contentType) ? "application/x-www-form-urlencoded" : contentType;

                }
                else
                {
                    request = WebRequest.Create(url);
                    request.Method = "POST";
                    request.Credentials = CredentialCache.DefaultCredentials;

                    request.Timeout = timeout <= 0 ? 100000 : timeout;
                    request.ContentType = string.IsNullOrEmpty(contentType) ? "application/x-www-form-urlencoded" : contentType;

                    byte[] byteArray = enc.GetBytes(postData);
                    request.ContentLength = byteArray.Length;

                    newStream = request.GetRequestStream();
                    newStream.Write(byteArray, 0, byteArray.Length);
                    newStream.Close();

                }

 
                // Get the response.
                wresponse = request.GetResponse();
                receiveStream = wresponse.GetResponseStream();
                readStream = new StreamReader(receiveStream, enc);
                response = readStream.ReadToEnd();

                return response;
            }
            catch (System.Net.WebException webExcp)
            {
                throw webExcp;
            }
            catch (System.IO.IOException ioe)
            {
                throw ioe;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (newStream != null)
                    newStream.Close();
                if (receiveStream != null)
                    receiveStream.Close();
                if (readStream != null)
                    readStream.Close();
            }
        }

        public static string DoRequestSSL(string url, string postData, string encoding, int timeout, string user, string pass)
        {
            return DoRequestSSL(url, postData, encoding, null, false, timeout, user, pass);
        }

        public static string DoRequestSSL(string url, string postData, string encoding, string contentType, bool isUrlEncoded,int timeout, string user, string pass) 
        {

            string response = null;

            WebRequest request = null;
            Stream newStream = null;
            Stream receiveStream = null;
            StreamReader readStream = null;
            WebResponse wresponse = null;
            try
            {

                TrustedCertificatePolicy policy = new TrustedCertificatePolicy();

                ServicePointManager.ServerCertificateValidationCallback = CheckValidationResult;

                Encoding enc = Encoding.GetEncoding(encoding);
                if (isUrlEncoded)
                {
                    postData = System.Web.HttpUtility.UrlEncode(postData, enc);
                }
                request = WebRequest.Create(url);
                request.PreAuthenticate = true;
                request.Credentials = new NetworkCredential(user, pass);
                //request.Credentials = CredentialCache.DefaultCredentials;
                request.Method = "POST";
                request.Timeout = timeout <= 0 ? 100000 : timeout;
                request.ContentType = string.IsNullOrEmpty(contentType) ? "application/x-www-form-urlencoded" : contentType;

                byte[] byteArray = enc.GetBytes(postData);// Encoding.GetEncoding("Windows-1255").GetBytes(postData);
               //request.ContentType = "text/xml";

                request.ContentLength = byteArray.Length;
                newStream = request.GetRequestStream();
                newStream.Write(byteArray, 0, byteArray.Length);
                newStream.Close();

                // Get the response.
                wresponse = request.GetResponse();
                receiveStream = wresponse.GetResponseStream();
                readStream = new StreamReader(receiveStream, enc);
                response = readStream.ReadToEnd();

                return response;
            }
            catch (System.Net.WebException webExcp)
            {
                throw webExcp;
            }
            catch (System.IO.IOException ioe)
            {
                throw ioe;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (newStream != null)
                    newStream.Close();
                if (receiveStream != null)
                    receiveStream.Close();
                if (readStream != null)
                    readStream.Close();
            }
        }

        public static bool CheckValidationResult(Object sender, X509Certificate certificate, X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public class TrustedCertificatePolicy : System.Net.ICertificatePolicy
        {
            public TrustedCertificatePolicy() { }

            public bool CheckValidationResult
            (
                System.Net.ServicePoint sp,
                System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                System.Net.WebRequest request, int problem)
            {
                return true;
            }
        }
        #endregion

    }

}

