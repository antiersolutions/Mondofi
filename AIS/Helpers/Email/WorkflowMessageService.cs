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

        protected virtual MessageTemplate GetActiveMessageTemplate(string messageTemplateName, UsersContext db)
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
            var restaurantName = db.Users.OrderBy(c => c.Id).Skip(1).First().VenueName;
            var messageTemplate = GetActiveMessageTemplate("Customer.OnlineBookingSucceed", db);
            messageTemplate.Subject = "Your Reservation Details for " + restaurantName;
            if (messageTemplate == null)
                return;
            var logoName = db.tabSettings.Where(s => s.Name.Contains("OnlineResosL")).Single().Value;
            var PhoneNumber = db.tabSettings.Where(s => s.Name.Contains("Phone")).Single().Value;
            var getAddress = db.tabSettings.Where(s => s.Name.Contains("Address")).Single().Value;
            var Salutation = db.tabSettings.Where(s => s.Name.Contains("Salutation")).Single().Value;
            string[] values = getAddress.Split(',');
            string laststring = getAddress.Split(',').Last();
            int stringCount = values.Count();
            int count = 1;
            var addressHtml = string.Empty;

            foreach (var item in values)
            {
                if (stringCount - 1 == count)
                {
                    addressHtml += item + "<br/>";
                }
                else if (stringCount > count)
                {
                    addressHtml += item + ",<br/>";
                }

                count++;
            }
            addressHtml += "<h3 style='color:#000;line-height:125%;font-family:Helvetica,Arial,sans-serif;font-size:16px;font-weight:bold;margin-top:10px;margin-bottom:3px;text-align:left;'>Call us at</h3>" + PhoneNumber;

            //tokens
            var tokens = new List<Token>();
            tokens.Add(new Token("Address", addressHtml, true));
            tokens.Add(new Token("RestaurantName", restaurantName, true));
            tokens.Add(new Token("Salutation", Salutation, true));
            tokens.Add(new Token("Logo", "https://www.mondofi.com" + logoName));
            tokens.Add(new Token("PhoneNumber", PhoneNumber));
            tokens.Add(new Token("ReservationId", reservation.ReservationId.ToString()));
            tokens.Add(new Token("ReservationDate", reservation.ReservationDate.ToString("ddd, MMM dd, yyyy")));
            tokens.Add(new Token("TimeForm", reservation.TimeForm.ToString("h:mm tt")));
            tokens.Add(new Token("Covers", reservation.Covers.ToString()));
            //tokens.Add(new Token("FullName", reservation.Customers.FirstName + " " +
            //    ((reservation.Customers.LastName.Length > 1) ? reservation.Customers.LastName.Remove(1) : reservation.Customers.LastName)));
            tokens.Add(new Token("FullName", reservation.Customers.FirstName));
            //tokens.Add(new Token("EditUrl", System.Web.HttpContext.Current.Request.Url.Host + "/Online/ReserveSuccess/" + reservation.ReservationId));
            tokens.Add(new Token("EditUrl", urlHelper.EncodedUrl("ReserveSuccess", "Online", new { id = reservation.ReservationId, company = db.Database.Connection.Database })));
            tokens.Add(new Token("CancelUrl", urlHelper.EncodedUrl("ReserveSuccess", "Online", new { id = reservation.ReservationId, company = db.Database.Connection.Database })));
            //tokens.Add(new Token("AppUrl", System.Web.HttpContext.Current.Request.Url.Host));
            tokens.Add(new Token("AppUrl", "https://www.mondofi.com"));
            var replyToEmail = db.tabSettings.Where(s => s.Name.Contains("ReplyToEmail")).Single();
            var Bcc = db.tabSettings.Where(s => s.Name.Contains("BCCEmail")).Single();
            messageTemplate.BccEmailAddresses = Bcc.Value;
            //var fromAddress = (string)ConfigurationManager.AppSettings["Email_To"];
            //var fromName = (string)ConfigurationManager.AppSettings["Email_To"];

            var fromAddress = replyToEmail.Value;
            var fromName = replyToEmail.Value;

            var toEmail = customerEmail.Email;
            var toName = reservation.Customers.FirstName;

            this.SendNotification(messageTemplate, fromAddress, fromName,
                tokens, toEmail, toName);
        }

        #endregion

        #endregion
    }
}
