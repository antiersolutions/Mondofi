using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Services;
using System.Xml.Serialization;

namespace AIS.EmailSender
{
    /// <summary>
    /// Email sender
    /// </summary>
    public partial class EmailSender
    {


        /// <summary>
        /// Sends an email
        /// </summary>
        /// <param name="emailAccount">Email account to use</param>
        /// <param name="subject">Subject</param>
        /// <param name="body">Body</param>
        /// <param name="fromAddress">From address</param>
        /// <param name="fromName">From display name</param>
        /// <param name="toAddress">To address</param>
        /// <param name="toName">To display name</param>
        /// <param name="replyTo">ReplyTo address</param>
        /// <param name="replyToName">ReplyTo display name</param>
        /// <param name="bcc">BCC addresses list</param>
        /// <param name="cc">CC addresses list</param>
        /// <param name="attachmentFilePath">Attachment file path</param>
        /// <param name="attachmentFileName">Attachment file name. If specified, then this file name will be sent to a recipient. Otherwise, "AttachmentFilePath" name will be used.</param>
        public virtual void SendEmail(string subject, string body,
            string toAddress, string toName, string fromAddress, string fromName,
             string replyTo = null, string replyToName = null,
            IEnumerable<string> bcc = null, IEnumerable<string> cc = null,
            string attachmentFilePath = null, string attachmentFileName = null, string attachmentIds = null)
        {
            var message = new MailMessage();
            //from, to, reply to
            message.From = new MailAddress(fromAddress, fromName);
            message.To.Add(new MailAddress(toAddress, toName));
            if (!String.IsNullOrEmpty(replyTo))
            {
                message.ReplyToList.Add(new MailAddress(replyTo, replyToName));
            }

            //BCC
            if (bcc != null)
            {
                foreach (var address in bcc.Where(bccValue => !String.IsNullOrWhiteSpace(bccValue)))
                {
                    message.Bcc.Add(address.Trim());
                }
            }

            //CC
            if (cc != null)
            {
                foreach (var address in cc.Where(ccValue => !String.IsNullOrWhiteSpace(ccValue)))
                {
                    message.CC.Add(address.Trim());
                }
            }

            //content
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;
            //string[] fils = attachmentIds.Split(',');

            //create  the file attachment for this e-mail message
            //foreach (var item in fils)
            //{
            //    var getdocument = _DocumentService.GetDocumentById(new Guid(item));
            //    var getDocumentUrl = _DocumentService.GetDocumentUrl(getdocument);
            //    var url = HostingEnvironment.MapPath(getDocumentUrl);


            //    if (!String.IsNullOrEmpty(url) &&
            //    File.Exists(url))
            //    {
            //        var attachment = new Attachment(url);
            //        attachment.ContentDisposition.CreationDate = File.GetCreationTime(url);
            //        attachment.ContentDisposition.ModificationDate = File.GetLastWriteTime(url);
            //        attachment.ContentDisposition.ReadDate = File.GetLastAccessTime(url);
            //        if (!String.IsNullOrEmpty(getdocument.DocumentName))
            //        {
            //            attachment.Name = getdocument.DocumentName;
            //        }
            //        message.Attachments.Add(attachment);
            //    }
            //}


            //send email
            using (var smtpClient = new SmtpClient())
            {
                //smtpClient.UseDefaultCredentials = emailAccount.UseDefaultCredentials;
                smtpClient.Host = ConfigurationManager.AppSettings["Host"];
                smtpClient.Port = Convert.ToInt32(ConfigurationManager.AppSettings["port"]);
                smtpClient.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSSl"]);
                smtpClient.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["User"], ConfigurationManager.AppSettings["password"]);
                smtpClient.Send(message);
            }
        }
    }
}
