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

using System.Data;
using System.Configuration;
using System.Collections;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Web;
using System.Net;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Web.UI;
using System.Net.Mail;
#pragma warning disable  CS1591

namespace Nistec.Web
{

    public interface IMailItem
    {

        string MailSender { get; }
        string MailDisplay { get; }
        string MailReplyTo { get; }
        string Encoding { get; }
        string MailHost { get; }
        string MailSignature { get; }

        string MailHost_User { get; }
        string MailHost_Pass { get; }
        string MailHost_SslType { get; }
        bool UseNetworkCredential { get;}
        int SmtpPort { get; }
    }

    public class MailClient
    {


        string m_hostAddress;
        string m_credUserName;
        string m_credPass;
        bool m_useNetworkCredential;

        public MailClient(string hostAddress)
        {
            m_hostAddress = hostAddress;
            m_useNetworkCredential = false;
        }
        public MailClient(string hostAddress, string credUserName, string credPass)
        {
            m_hostAddress = hostAddress;
            m_credUserName = credUserName;
            m_credPass = credPass;
            m_useNetworkCredential = true;
        }

        public MailClient(IMailItem mailer)
        {
            m_hostAddress = mailer.MailHost;
            m_credUserName = mailer.MailHost_User;
            m_credPass = mailer.MailHost_Pass;
            m_useNetworkCredential = mailer.UseNetworkCredential;
        }

        public static List<Attachment> CreateAtachments(string[] fileNames)
        {
            List<Attachment> attachments = new List<Attachment>();
            foreach (string s in fileNames)
            {
                attachments.Add(new Attachment(s));
            }
            return attachments;
        }
        public static Attachment CreateServerAtachment(Page p, HtmlInputFile inpAttachment)
        {

            string strFileName = null;

            /* Bigining of Attachment1 process   & 
               Check the first open file dialog for a attachment */
            if (inpAttachment.PostedFile != null)
            {
                /* Get a reference to PostedFile object */
                HttpPostedFile attFile = inpAttachment.PostedFile;
                /* Get size of the file */
                int attachFileLength = attFile.ContentLength;
                /* Make sure the size of the file is > 0  */
                if (attachFileLength > 0)
                {
                    /* Get the file name */
                    strFileName = Path.GetFileName(inpAttachment.PostedFile.FileName);
                    /* Save the file on the server */
                    inpAttachment.PostedFile.SaveAs(p.Server.MapPath(strFileName));
                    /* Create the email attachment with the uploaded file */
                    Attachment attach = new Attachment(p.Server.MapPath(strFileName));

                    return attach;
                }
           }

            return null;
        }

        public System.Net.Mail.SmtpException SendMail(IMailItem mailer, string[] To, string Subject, string Body, List<Attachment> attachments, bool sendAsNewsletter = false)
        {
            var post = new MailMessage();
            try
            {

                Encoding encoding = Encoding.GetEncoding(mailer.Encoding);

                post.From = new MailAddress(mailer.MailSender, mailer.MailDisplay, encoding);

                if (!string.IsNullOrEmpty(mailer.MailReplyTo))
                    post.ReplyToList.Add( new MailAddress(mailer.MailReplyTo));

                foreach (string to in To)
                {
                    post.To.Add(to);
                }
                post.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                post.Subject = Subject;
                post.Body = Body;
                post.IsBodyHtml = true;
                post.BodyEncoding = encoding;
                post.SubjectEncoding = encoding;


                var htmlView = AlternateView.CreateAlternateViewFromString(post.Body, encoding, "text/html");
                post.AlternateViews.Add(htmlView);

                if (attachments != null && attachments.Count > 0)
                {
                    foreach (var at in attachments)
                    {
                        post.Attachments.Add(at);
                    }
                }


                //if you have relay privilege you can use only host data; 
                //var host = "Your SMTP Server IP Adress";
                //var postman = new SmtpClient(host);

                //you dont have relay privilege you must be use Network Credential
                var postman = new SmtpClient();
                postman.Host = m_hostAddress;

                if (m_useNetworkCredential)
                {
                    NetworkCredential cred = new NetworkCredential(m_credUserName, m_credPass);
                    postman.UseDefaultCredentials = false;
                    postman.Credentials = cred;
                }
                else
                {
                    postman.UseDefaultCredentials = true;
                }

                int sent = InvokeMail(postman, post, To, sendAsNewsletter);

                if (sent == 0)
                    return new SmtpException(SmtpStatusCode.GeneralFailure, "Message Not Sent");
                if (sent < To.Length)
                    return new SmtpException(SmtpStatusCode.Ok, "Message " + sent.ToString() + " Sent with error");
                else
                    return new SmtpException(SmtpStatusCode.Ok, "Message " + sent.ToString() + " Sent");

            }
            catch (System.Net.Mail.SmtpException sex)
            {
                Console.Write("sent error: status: {0}, message:{1}", sex.StatusCode, sex.Message);
                return sex;
            }
            catch (Exception ex)
            {
                Console.Write("sent error: status: {0}, message:{1}", -1, ex.Message);
                return new SmtpException(SmtpStatusCode.GeneralFailure, "Message Not Sent");

            }
            finally
            {
                if (post != null)
                {
                    post.Dispose();
                }
            }
        }

        int InvokeMail(SmtpClient client, MailMessage message, string[] To, bool sendAsNewsletter)
        {
            int sentOk = 0;

           
            if (sendAsNewsletter && To.Length > 1)
            {
                foreach (string to in To)
                {
                    try
                    {
                        if (message.To.Count == 0)
                            message.To.Add(to);
                        else
                            message.To[0] = new MailAddress(to);

                        client.Send(message);
                        sentOk++;
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            else
            {
                foreach (string to in To)
                {
                    message.To.Add(to);
                }
                client.Send(message);
                sentOk = To.Length;
            }
            return sentOk;
        }

        public bool SendMail(IMailItem mailer, string To, string Subject, string Body, List<Attachment> attachments)
        {
            var post = new MailMessage();
            try
            {


                post.From = new MailAddress(mailer.MailSender, mailer.MailDisplay, Encoding.GetEncoding("window-1255"));
                if (!string.IsNullOrEmpty(mailer.MailReplyTo))
                    post.ReplyToList.Add( new MailAddress(mailer.MailReplyTo));
                post.To.Add(To);
                post.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                post.Subject = Subject;
                //post.Body = Body;

                post.Body = string.Format(@"<html><body><div style=""font-family:verdana;font-size:10pt;"">
                {0}</div></body></html>", Body.Replace("\r\n", "<br/>"));

                var htmlView = AlternateView.CreateAlternateViewFromString(post.Body, null, "text/html");
                post.AlternateViews.Add(htmlView);

                if (attachments != null && attachments.Count > 0)
                {
                    foreach (var at in attachments)
                    {
                        post.Attachments.Add(at);
                    }
                }

                post.IsBodyHtml = true;

                //if you have relay privilege you can use only host data; 
                //var host = "Your SMTP Server IP Adress";
                //var postman = new SmtpClient(host);

                //you dont have relay privilege you must be use Network Credential
                var postman = new SmtpClient();
                postman.Host = m_hostAddress;
                
                if (m_useNetworkCredential)
                {
                    NetworkCredential cred = new NetworkCredential(m_credUserName, m_credPass);
                    postman.UseDefaultCredentials = false;
                    postman.Credentials = cred;
                }
                else
                {
                    postman.UseDefaultCredentials = true;
                }
                postman.Send(post);
                return true;
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return false;
            }
            finally
            {
                if (post != null)
                {
                    post.Dispose();
                }
            }
        }

        public bool SendMail(string From, string To, string Subject, string Body, List<Attachment> attachments)
        {
            return SendMail(From, null, null, To, Subject, Body, attachments);
        }

        public bool SendMail(string From, string Display, string ReplyTo, string To, string Subject, string Body, List<Attachment> attachments)
        {
            var post = new MailMessage();
            try
            {


                post.From = new MailAddress(From, Display, Encoding.GetEncoding("window-1255"));
                if (!string.IsNullOrEmpty(ReplyTo))
                    post.ReplyToList.Add(new MailAddress(ReplyTo));
                post.To.Add(To);
                post.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                post.Subject = Subject;
               

                post.Body = string.Format(@"<html><body><div style=""font-family:verdana;font-size:10pt;"">
                {0}</div></body></html>", Body.Replace("\r\n", "<br/>"));

                var htmlView = AlternateView.CreateAlternateViewFromString(post.Body, null, "text/html");
                post.AlternateViews.Add(htmlView);

                if (attachments != null && attachments.Count > 0)
                {
                    foreach (var at in attachments)
                    {
                        post.Attachments.Add(at);
                    }
                }

                post.IsBodyHtml = true;

                //if you have relay privilege you can use only host data; 
                //var host = "Your SMTP Server IP Adress";
                //var postman = new SmtpClient(host);

                //you dont have relay privilege you must be use Network Credential
                var postman = new SmtpClient();
                postman.Host = m_hostAddress;
                if (m_useNetworkCredential)
                {
                    NetworkCredential cred = new NetworkCredential(m_credUserName, m_credPass);
                    postman.UseDefaultCredentials = false;
                    postman.Credentials = cred;
                }
                else
                {
                    postman.UseDefaultCredentials = true;
                }
                postman.Send(post);
                return true;
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                return false;
            }
            finally
            {
                if (post != null)
                {
                    post.Dispose();
                }
            }
        }

        #region static

        /// <summary>
        /// Transmit an email message to a recipient without
        /// any attachments
        /// </summary>
        /// <param name="hostAddress">host Address</param>
        /// <param name="sendTo">Recipient Email Address</param>
        /// <param name="sendFrom">Sender Email Address</param>
        /// <param name="sendSubject">Subject Line Describing Message</param>
        /// <param name="sendMessage">The Email Message Body</param>
        /// <returns>Status Message as String</returns>
        public static string SendMessage(
            string hostAddress,
            string sendTo, 
            string sendFrom,
            string sendSubject, 
            string sendMessage)
        {

            try

            {
                if (string.IsNullOrEmpty(hostAddress))
                    hostAddress = "127.0.0.1";

                // validate the email address
                bool bTest = ValidateEmailAddress(sendTo);

                // if the email address is bad, return message
                if (bTest == false)
                    return "Invalid recipient email address: " + sendTo;

                // create the email message

                MailMessage message = new MailMessage(
                   sendFrom,
                   sendTo,
                   sendSubject,
                   sendMessage);
 

                // create smtp client at mail server location
                SmtpClient client = new SmtpClient(hostAddress);

                // add credentials
                client.UseDefaultCredentials = true;

                // send message
                client.Send(message);

                return "Message sent to " + sendTo + " at " + DateTime.Now.ToString() + ".";

            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }


        /// <summary>
        /// Transmit an email message with
        /// attachments
        /// </summary>
        /// <param name="hostAddress">host Address</param>
        /// <param name="sendTo">Recipient Email Address</param>
        /// <param name="sendFrom">Sender Email Address</param>
        /// <param name="sendSubject">Subject Line Describing Message</param>
        /// <param name="sendMessage">The Email Message Body</param>
        /// <param name="attachments">A string array pointing to the location of each attachment</param>
        /// <returns>Status Message as String</returns>

        public static string SendMessageWithAttachment(
            string hostAddress, 
            string sendTo, 
            string sendFrom, 
            string sendSubject, 
            string sendMessage, 
            ArrayList attachments)
        {

            try
            {
                if (string.IsNullOrEmpty(hostAddress))
                    hostAddress = "127.0.0.1";

                // validate email address
                bool bTest = ValidateEmailAddress(sendTo);


                if (bTest == false)
                    return "Invalid recipient email address: " + sendTo;

                // Create the basic message
                MailMessage message = new MailMessage(
                   sendFrom,
                   sendTo,
                   sendSubject,
                   sendMessage);


                // The attachments array should point to a file location     
                // where
                // the attachment resides - add the attachments to the
                // message
                foreach (string attach in attachments)
                {
                    Attachment attached = new Attachment(attach,
                    MediaTypeNames.Application.Octet);
                    message.Attachments.Add(attached);
                }

                // create smtp client at mail server location
                SmtpClient client = new SmtpClient(hostAddress);

                // Add credentials
                client.UseDefaultCredentials = true;

                // send message
                client.Send(message);
                return "Message sent to " + sendTo + " at " + DateTime.Now.ToString() + ".";
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }


        /// <summary>
        /// Confirm that an email address is valid
        /// in format
        /// </summary>
        /// <param name="emailAddress">Full email address to validate</param>
        /// <returns>True if email address is valid</returns>
        public static bool ValidateEmailAddress(string emailAddress)
        {
            try
            {
                string TextToValidate = emailAddress;
                Regex expression = new Regex(@"\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,3}");


                // test email address with expression
                if (expression.IsMatch(TextToValidate))
                {
                    // is valid email address
                    return true;
                }
                else
                {
                   // is not valid email address
                    return false;
                }
            }
            catch (Exception)
            {
                throw;

            }

        }
        #endregion
    }
}

