using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Web;
using AIS.Helpers.Email;
using AISModels;
using AIS.Models;
using AIS.Helpers.Caching;
using System.Web.Mvc;
using AIS.Extensions;

namespace AIS.Helpers.Email
{
    public partial class WorkflowMessageService
    {
        #region Fields

        private readonly UsersContext _db;
        private readonly EmailSender _emailSender;
        private readonly Tokenizer _tokenizer;

        #endregion

        #region Ctor

        public WorkflowMessageService()
        {
            this._db = new UsersContext();
            this._emailSender = new EmailSender();
            this._tokenizer = new Tokenizer();
        }

        #endregion

        #region Utilities

        public virtual void SendNotification(MessageTemplate messageTemplate,
            string fromEmailAddress, string fromName, IEnumerable<Token> tokens,
            string toEmailAddress, string toName,
            string attachmentFilePath = null, string attachmentFileName = null,
            string replyToEmailAddress = null, string replyToName = null)
        {
            //retrieve localized message template data
            var bcc = messageTemplate.BccEmailAddresses;
            var bccList = String.IsNullOrWhiteSpace(bcc)
                            ? null
                            : bcc.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var subject = messageTemplate.Subject;
            var body = messageTemplate.Body;

            //Replace subject and body tokens 
            var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);

            _emailSender.SendEmail(subjectReplaced,
                       bodyReplaced,
                      toEmailAddress,
                      toName,
                      fromEmailAddress,
                      fromName,
                      replyToEmailAddress,
                      replyToName,
                      bcc: bccList,
                      attachmentFilePath: attachmentFilePath,
                      attachmentFileName: attachmentFileName);
        }

        protected virtual MessageTemplate GetActiveMessageTemplate(string messageTemplateName,UsersContext db)
        {
            var messageTemplate = db.GetMessageTemplateByName(messageTemplateName);

            //no template found
            if (messageTemplate == null)
                return null;

            //ensure it's active
            var isActive = messageTemplate.IsActive;
            if (!isActive)
                return null;

            return messageTemplate;
        }

        #endregion

        #region Methods

        #region Reservation Emails

        /// <summary>
        /// Sends a success message to customer
        /// </summary>
        /// <param name="reservation">Resrervation</param>
        public virtual void SendCustomerBookingSuccess(UrlHelper urlHelper, Reservation reservation, UsersContext db)
        {
            if (reservation == null)
                throw new ArgumentNullException("reseravation");

            if (reservation.Customers.Emails == null)
                return;

            var customerEmail = reservation.Customers.Emails.LastOrDefault();
            if (customerEmail == null)
                return;

            var messageTemplate = GetActiveMessageTemplate("Customer.OnlineBookingSucceed", db);
            if (messageTemplate == null)
                return;

            //tokens
            var tokens = new List<Token>();
            tokens.Add(new Token("ReservationId", reservation.ReservationId.ToString()));
            tokens.Add(new Token("ReservationDate", reservation.ReservationDate.ToString("ddd, MMM dd, yyyy")));
            tokens.Add(new Token("TimeForm", reservation.TimeForm.ToString("h:mm tt")));
            tokens.Add(new Token("Covers", reservation.Covers.ToString()));
            //tokens.Add(new Token("FullName", reservation.Customers.FirstName + " " +
            //    ((reservation.Customers.LastName.Length > 1) ? reservation.Customers.LastName.Remove(1) : reservation.Customers.LastName)));
            tokens.Add(new Token("FullName", reservation.Customers.FirstName));
            //tokens.Add(new Token("EditUrl", System.Web.HttpContext.Current.Request.Url.Host + "/Online/ReserveSuccess/" + reservation.ReservationId));
            tokens.Add(new Token("EditUrl", urlHelper.EncodedUrl("ReserveSuccess", "Online", new { id = reservation.ReservationId })));
            tokens.Add(new Token("CancelUrl", urlHelper.EncodedUrl("ReserveSuccess", "Online", new { id = reservation.ReservationId })));
            //tokens.Add(new Token("AppUrl", System.Web.HttpContext.Current.Request.Url.Host));
            tokens.Add(new Token("AppUrl", "http://media.vanfish.com/reservation_email"));

            var fromAddress = (string)ConfigurationManager.AppSettings["Email_To"];
            var fromName = (string)ConfigurationManager.AppSettings["Email_To"];

            var toEmail = customerEmail.Email;
            var toName = reservation.Customers.FirstName;

            this.SendNotification(messageTemplate, fromAddress, fromName,
                tokens, toEmail, toName);
        }

        #endregion

        #endregion
    }
}
