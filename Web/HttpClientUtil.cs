using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
#pragma warning disable  CS1591

namespace Nistec.Web
{

    public struct HttpAck
    {

        public string Response { get; set; }
        public string Status { get; set; }
    }
    public class HttpClientUtil
    {

        public const string ContentType_UrlEncoded = "application/x-www-form-urlencoded";
        public const string ContentType_Json = "application/json";
        public const string ContentType_TextXml = "text/xml";

        public static HttpAck DoPostForm(string txtUrl, string txtRequest,  int TimeoutSeconds)
        {
            return Exec(txtUrl, null, txtRequest, "POST", "Form", TimeoutSeconds);
        }
        public static HttpAck DoGetForm(string txtUrl, string txtRequest, int TimeoutSeconds)
        {
            return Exec(txtUrl, null, txtRequest, "GET", "Form", TimeoutSeconds);
        }
        public static HttpAck DoPostSoap(string txtUrl, string txtAction, string txtRequest, int TimeoutSeconds)
        {
            return Exec(txtUrl, txtAction, txtRequest, "POST", "SoapXml", TimeoutSeconds);
        }
        public static HttpAck DoPost(string txtUrl, string txtRequest, int TimeoutSeconds)
        {
            return Exec(txtUrl, null, txtRequest, "POST", "json", TimeoutSeconds);
        }
        public static HttpAck DoGet(string txtUrl, string txtRequest, int TimeoutSeconds)
        {
            return Exec(txtUrl, null, txtRequest, "GET", "json", TimeoutSeconds);
        }

        static HttpAck Exec(string txtUrl, string txtAction, string txtRequest, string method, string formatType, int TimeoutSeconds)
        {
            string txtResponse = "";
            string txtStatus = "";

            CancellationToken cancelToken;

            try
            {
                //InvalidateVisual();
                //Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);


                if (txtUrl == null || txtUrl.Length == 0)
                    throw new ArgumentException("Invalid Url!");
                //else if (contentType == "Form")
                //{
                //    isValid = true;
                //}

                else if (txtRequest == null || txtRequest.Length == 0)
                    throw new ArgumentException("Invalid Request!");

                //if (contentType == null)
                //    contentType = "application/json";
                var httpMethod = GetMethod(method);
                string contentType = null;
                txtStatus = "Do request, Please Wait...";
                //InvalidateVisual();
                //Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);


                switch (formatType)
                {
                    case "Form":
                        {
                            contentType = "application/x-www-form-urlencoded";//"text/xml; charset=utf-8";
                            txtResponse = DoRequest(txtUrl, method, contentType, txtRequest, TimeoutSeconds);
                        }
                        break;
                    case "SoapXml":
                        {
                            if (txtAction == null || txtAction.Length == 0)
                                throw new ArgumentException("Invalid Action!");
                            contentType = "text/xml";
                            string response = DoSoapRequest(txtUrl, txtAction, method, contentType, txtRequest, TimeoutSeconds);
                            txtResponse = PrintXML(response);
                        }
                        break;
                    case "Xml":
                        {
                            contentType = "text/xml";
                            Task<string> response = null;
                            if (httpMethod == HttpMethod.Get)
                                response = DoJsonGetRequest(txtUrl);
                            else //if (httpMethod == HttpMethod.Post)
                                response = DoHttpRequest(txtUrl, GetMethod(method), contentType, txtRequest, TimeoutSeconds);
                            txtResponse = PrintXML(response.Result);
                        }
                        break;
                    default:
                        {
                            contentType = "application/json";
                            Task<string> response = null;
                            if (httpMethod == HttpMethod.Get)
                                response = DoJsonGetRequest(txtUrl);
                            else
                                response = DoHttpRequest(txtUrl, GetMethod(method), contentType, txtRequest, TimeoutSeconds);

                            if (response == null)

                                txtResponse = "No response from server!";
                            else
                                txtResponse = PrintJson(response.Result);

                        }
                        break;

                }
                txtStatus = "Completed!";

            }
            catch (AggregateException ex)
            {
                txtStatus = "Error!";
                txtResponse = FormatAggrigateException(ex, cancelToken);
            }
            catch (Exception ex)
            {
                txtStatus = "Error!";
                txtResponse = "Send messsage error: " + FormatException(ex, formatType);
            }
            return new HttpAck() { Response = txtResponse, Status = txtStatus };
        }

        #region helpers
        static string FormatException(Exception ex, string format)
        {
            if (ex == null)
                return "";
            if (ex.InnerException != null)
                return ex.Message + " Inner: " + ex.InnerException;
            if (format == "Xml")
                return PrintXML(ex.Message);
            return ex.Message;
        }

        static string FormatAggrigateException(AggregateException ex, CancellationToken cancelToken)
        {

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Send messsage error: " + ex.Message);
            foreach (var inner in ex.InnerExceptions)
            {

                if (inner is TaskCanceledException)
                {
                    if (cancelToken.IsCancellationRequested && ((TaskCanceledException)inner).CancellationToken == cancelToken)
                    {
                        // a real cancellation, triggered by the caller
                        sb.AppendLine("Send messsage canceled by user");
                    }
                    sb.AppendLine(inner.Source + ":" + inner.Message + " (possibly request timeout)");
                    continue;
                }
                // a web request timeout (possibly other things!?)
                sb.AppendLine(inner.Source + ":" + inner.Message);
            }
            return sb.ToString();
        }

        static HttpMethod GetMethod(string method)
        {
            switch (method.ToUpper())
            {
                case "GET":
                    return HttpMethod.Get;
                case "POST":
                default:
                    return HttpMethod.Post;

            }
        }
        #endregion

        #region http request methods

 

        public static Task<string> DoHttpRequest(string address, HttpMethod method, string contentType, string data, int TimeoutSeconds)
        {
            using (var httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(TimeoutSeconds) })
            using (var request = new HttpRequestMessage(method, address))
            using (request.Content = new StringContent(data, Encoding.UTF8, contentType))
            using (var response = httpClient.SendAsync(request))
            {

                return response.Result.Content.ReadAsStringAsync();
            }
        }

        public static Task<string> DoHttpRequest(string address, HttpMethod method, string contentType, string data, int TimeoutSeconds, CancellationToken ctsTocken)
        {
            using (var httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(TimeoutSeconds) })
            using (var request = new HttpRequestMessage(method, address))
            using (request.Content = new StringContent(data, Encoding.UTF8, contentType))
            using (var response = httpClient.SendAsync(request, ctsTocken))
            {

                return response.Result.Content.ReadAsStringAsync();
            }
        }

        public static Task<string> DoHttpRequestAsync(string address, HttpMethod method, string contentType, string data, int TimeoutSeconds)
        {
            using (var httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(TimeoutSeconds) })
            using (var cts = new CancellationTokenSource())//TimeSpan.FromSeconds(TimeoutSeconds)))
            using (var request = new HttpRequestMessage(method, address))
            using (request.Content = new StringContent(data, Encoding.UTF8, contentType))
            using (var response = httpClient.SendAsync(request, cts.Token))
            {

                return response.Result.Content.ReadAsStringAsync();
            }
        }

        public static Task<string> DoJsonPostRequest(string address, string data)
        {
            using (var client = new HttpClient() { Timeout = TimeSpan.FromSeconds(20) })
            using (var content = new StringContent(data, Encoding.UTF8))
            using (Task<HttpResponseMessage> response = client.PostAsync(address, content))
            using (HttpContent result = response.Result.Content)
            {
                return result.ReadAsStringAsync();
            }

        }

        public static Task<string> DoJsonGetRequest(string address)
        {
            using (HttpClient client = new HttpClient() { Timeout = TimeSpan.FromSeconds(20) })
            using (Task<HttpResponseMessage> response = client.GetAsync(address))
            using (HttpContent content = response.Result.Content)
            {
                return content.ReadAsStringAsync();
            }
        }

        public static string DoSoapRequest(string url, string soapAction, string method, string contentType, string soapBody, int TimeoutSeconds)
        {
            string result = null;

            try
            {
                //Create HttpWebRequest
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                request.Method = method;
                request.ContentType = contentType + "; charset=utf-8";//"text/xml; charset=utf-8";
                request.Timeout = (int)TimeSpan.FromSeconds(TimeoutSeconds).TotalMilliseconds;
                request.KeepAlive = false;
                request.UseDefaultCredentials = true;
                request.Headers["SOAPAction"] = soapAction;

                byte[] bytes = Encoding.UTF8.GetBytes(soapBody);
                request.ContentLength = bytes.Length;

                //Create request stream
                using (Stream OutputStream = request.GetRequestStream())
                {
                    if (!OutputStream.CanWrite)
                    {
                        throw new Exception("Could not wirte to RequestStream");
                    }
                    OutputStream.Write(bytes, 0, bytes.Length);
                }

                //Get response stream
                using (WebResponse resp = request.GetResponse())
                {
                    using (Stream ResponseStream = resp.GetResponseStream())
                    {
                        using (StreamReader readStream =
                                new StreamReader(ResponseStream, Encoding.UTF8))
                        {
                            result = readStream.ReadToEnd();
                        }
                    }
                }

                //result = SoapRequest(url, soapAction, soapBody);
            }
            catch (WebException wex)
            {
                result = "Error: " + wex.Message;
            }
            catch (Exception ex)
            {
                result = "Error: " + ex.Message;
            }
            return result;
        }


        public static string DoRequest(string url, string method, string contentType, string postArgs, int TimeoutSeconds)
        {

            string response = null;

            WebRequest request = null;
            Stream newStream = null;
            Stream receiveStream = null;
            StreamReader readStream = null;
            WebResponse wresponse = null;
            string encoding = "utf-8";
            int timeout = (int)TimeSpan.FromSeconds(TimeoutSeconds).TotalMilliseconds;

            try
            {
                Encoding enc = Encoding.GetEncoding(encoding);
                string[] args = postArgs.Replace("\r\n", "").Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
                StringBuilder sb = new StringBuilder();
                int counter = 0;
                foreach (string s in args)
                {
                    string[] arg = s.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);

                    if (counter > 0)
                        sb.Append("&");
                    sb.Append(arg[0].Trim() + "=" + HttpUtility.UrlEncode(arg[1].Trim()));

                    counter++;
                }

                string postData = sb.ToString();


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

        #endregion
               
        #region formatter

        internal static string Indent = "   ";

        internal static void AppendIndent(StringBuilder sb, int count)
        {
            for (; count > 0; --count) sb.Append(Indent);
        }

        internal static string PrintJson(string input)
        {
            var output = new StringBuilder();
            int depth = 0;
            int len = input.Length;
            char[] chars = input.ToCharArray();
            for (int i = 0; i < len; ++i)
            {
                char ch = chars[i];

                if (ch == '\"') // found string span
                {
                    bool str = true;
                    while (str)
                    {
                        output.Append(ch);
                        ch = chars[++i];
                        if (ch == '\\')
                        {
                            output.Append(ch);
                            ch = chars[++i];
                        }
                        else if (ch == '\"')
                            str = false;
                    }
                }

                switch (ch)
                {
                    case '{':
                    case '[':
                        output.Append(ch);
                        output.AppendLine();
                        AppendIndent(output, ++depth);
                        break;
                    case '}':
                    case ']':
                        output.AppendLine();
                        AppendIndent(output, --depth);
                        output.Append(ch);
                        break;
                    case ',':
                        output.Append(ch);
                        output.AppendLine();
                        AppendIndent(output, depth);
                        break;
                    case ':':
                        output.Append(" : ");
                        break;
                    default:
                        if (!char.IsWhiteSpace(ch))
                            output.Append(ch);
                        break;
                }
            }

            return output.ToString();
        }

        public static String PrintXML(String XML)
        {

            XmlDocument document = new XmlDocument();

            try
            {
                document.LoadXml(XML);
                using (MemoryStream mStream = new MemoryStream())
                using (XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.Unicode))
                {
                    writer.Formatting = Formatting.Indented;

                    // Write the XML into a formatting XmlTextWriter
                    document.WriteContentTo(writer);
                    writer.Flush();
                    mStream.Flush();

                    // Have to rewind the MemoryStream in order to read
                    // its contents.
                    mStream.Position = 0;

                    // Read MemoryStream contents into a StreamReader.
                    StreamReader sReader = new StreamReader(mStream);

                    // Extract the text from the StreamReader.
                    String FormattedXML = sReader.ReadToEnd();

                    return FormattedXML;
                }
            }
            catch (Exception)
            {
                return XML;
            }

        }

        #endregion
    }

    /// <summary>
    /// HttpC lient Request
    /// </summary>
    public class HttpRequester
    {
        public static Task<string> DoPostJson(string address, string data, string accessToken, string schema = "Bearer", int TimeoutSeconds = 0)
        {

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(schema, accessToken);
                if (TimeoutSeconds > 0)
                    client.Timeout = TimeSpan.FromSeconds(TimeoutSeconds);
                using (var request = new HttpRequestMessage(HttpMethod.Post, address))
                //request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                using (request.Content = new StringContent(data, Encoding.UTF8, "application/json"))
                using (var response = client.SendAsync(request))
                {
                    return response.Result.Content.ReadAsStringAsync();
                }
            }
        }

        public static Task<string> DoPostJson(string address, string data, Dictionary<string, string> headers, int TimeoutSeconds = 0)
        {

            using (HttpClient client = new HttpClient())
            {
                if (headers != null)
                {
                    foreach (var h in headers)
                        client.DefaultRequestHeaders.Add(h.Key, h.Value);
                }
                if (TimeoutSeconds > 0)
                    client.Timeout = TimeSpan.FromSeconds(TimeoutSeconds);
                using (var request = new HttpRequestMessage(HttpMethod.Post, address))
                //request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                using (request.Content = new StringContent(data, Encoding.UTF8, "application/json"))
                using (var response = client.SendAsync(request))
                {
                    return response.Result.Content.ReadAsStringAsync();
                }
            }
        }

        public static Task<string> DoPostJson(string address, string data, Dictionary<string, IEnumerable<string>> headers, int TimeoutSeconds = 0)
        {

            using (HttpClient client = new HttpClient())
            {
                if (headers != null)
                {
                    foreach (var h in headers)
                        client.DefaultRequestHeaders.Add(h.Key, h.Value);
                }

                if (TimeoutSeconds > 0)
                    client.Timeout = TimeSpan.FromSeconds(TimeoutSeconds);
                using (var request = new HttpRequestMessage(HttpMethod.Post, address))
                //request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                using (request.Content = new StringContent(data, Encoding.UTF8, "application/json"))
                using (var response = client.SendAsync(request))
                {
                    return response.Result.Content.ReadAsStringAsync();
                }
            }
        }

        public static Task<string> DoPostJson(string address, string data, string contentType, Dictionary<string, IEnumerable<string>> headers, int TimeoutSeconds = 0)
        {

            using (HttpClient client = new HttpClient())
            {
                if (headers != null)
                {
                    foreach (var h in headers)
                        client.DefaultRequestHeaders.Add(h.Key, h.Value);
                }

                if (TimeoutSeconds > 0)
                    client.Timeout = TimeSpan.FromSeconds(TimeoutSeconds);
                using (var request = new HttpRequestMessage(HttpMethod.Post, address))
                //request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                using (request.Content = new StringContent(data, Encoding.UTF8, contentType))
                using (var response = client.SendAsync(request))
                {
                    return response.Result.Content.ReadAsStringAsync();
                }
            }
        }

 

        public static Task<string> DoPostJson(string address, string data, Dictionary<string, IEnumerable<string>> headers, CancellationToken ctsTocken)
        {

            using (HttpClient client = new HttpClient())
            {
                if (headers != null)
                {
                    foreach (var h in headers)
                        client.DefaultRequestHeaders.Add(h.Key, h.Value);
                }

                using (var request = new HttpRequestMessage(HttpMethod.Post, address))
                //request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                using (request.Content = new StringContent(data, Encoding.UTF8, "application/json"))
                using (var response = client.SendAsync(request, ctsTocken))
                {
                    return response.Result.Content.ReadAsStringAsync();
                }
            }

        }

        public static Task<string> DoPost(string address, int TimeoutSeconds = 0)
        {
            using (HttpClient client = new HttpClient())
            {
                if (TimeoutSeconds > 0)
                    client.Timeout = TimeSpan.FromSeconds(TimeoutSeconds);
                //using (var request = new HttpRequestMessage(HttpMethod.Post, address))
                using (var response = client.PostAsync(address, null))
                {
                    return response.Result.Content.ReadAsStringAsync();
                }
            }
        }

        public static Task<string> DoPost(string address, CancellationToken ctsTocken)
        {
            using (HttpClient client = new HttpClient())
            {
                //using (var request = new HttpRequestMessage(HttpMethod.Post, address))
                using (var response = client.PostAsync(address, null, ctsTocken))
                {
                    return response.Result.Content.ReadAsStringAsync();
                }
            }
        }

        public static Task<string> DoRequest(string address, string data, HttpMethod method, string contentType, Dictionary<string, IEnumerable<string>> headers, int TimeoutSeconds = 0)
        {

            using (HttpClient client = new HttpClient())
            {
                if (headers != null)
                {
                    foreach (var h in headers)
                        client.DefaultRequestHeaders.Add(h.Key, h.Value);
                }

                if (TimeoutSeconds > 0)
                    client.Timeout = TimeSpan.FromSeconds(TimeoutSeconds);
                using (var request = new HttpRequestMessage(method, address))
                //request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                using (request.Content = new StringContent(data, Encoding.UTF8, contentType))
                using (var response = client.SendAsync(request))
                {
                    return response.Result.Content.ReadAsStringAsync();
                }
            }
        }
    }
}
