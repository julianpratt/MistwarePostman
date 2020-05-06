using System;
using System.Collections.Generic;
using System.IO;
using System.Net; 
using System.Net.Mail;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using Mistware.Utils;


namespace Mistware.Postman
{
    /// Interface with SendGrid
    public static class SendMail
    {  
        /// ApiKey, which must be set before SendMail can be used
        public static string ApiKey = null;

        /// <summary>
        /// Send an eMail
        /// </summary>
        /// <param name="from">email address of the sender</param>
        /// <param name="to">email address of the recipient</param>
        /// <param name="subject">eMail Subject (aka header)</param>
        /// <param name="body">text of the eMail</param>
        public static void Send(string from, string to, string subject, string body)
        {
            Send(new MailAddress(from), new MailAddress(to), null, subject, body);
        }

        /// <summary>
        /// Send an eMail
        /// </summary>
        /// <param name="from">email address of the sender</param>
        /// <param name="to">email address of the recipient</param>
        /// <param name="subject">eMail Subject (aka header)</param>
        /// <param name="body">text of the eMail</param>
        /// <param name="attachment">byte array with attachment</param>
        /// <param name="filename">filename of the attachment</param>
        public static void Send(string from, string to, string subject, string body, byte[] attachment, string filename)
        {
            Send(new MailAddress(from), new MailAddress(to), null, subject, body, attachment, filename);
        }

        /// <summary>
        /// Send an eMail
        /// </summary>
        /// <param name="from">email address of the sender</param>
        /// <param name="to">email address of the recipient</param>
        /// <param name="cc">email address of copy recipient</param>
        /// <param name="subject">eMail Subject (aka header)</param>
        /// <param name="body">text of the eMail</param>
        public static void Send(string from, string to, string cc, string subject, string body)
        {
            Send(new MailAddress(from), new MailAddress(to), new MailAddress(cc), subject, body);
        }

        /// <summary>
        /// Send an eMail
        /// </summary>
        /// <param name="from">email address of the sender</param>
        /// <param name="to">email address of the recipient</param>
        /// <param name="cc">email address of copy recipient</param>
        /// <param name="subject">eMail Subject (aka header)</param>
        /// <param name="body">text of the eMail</param>
        /// <param name="attachment">byte array with attachment</param>
        /// <param name="filename">filename of the attachment</param>
        public static void Send(string from, string to, string cc, string subject, string body, byte[] attachment, string filename)
        {
            Send(new MailAddress(from), new MailAddress(to), new MailAddress(cc), subject, body, attachment, filename);
        }

        /// <summary>
        /// Send an eMail
        /// </summary>
        /// <param name="from">email address of the sender</param>
        /// <param name="to">email address of the recipient</param>
        /// <param name="subject">eMail Subject (aka header)</param>
        /// <param name="body">text of the eMail</param>
        public static void Send(MailAddress from, MailAddress to, string subject, string body)
        {
            Send(from, to, null, subject, body);
        }

        /// <summary>
        /// Send an eMail
        /// </summary>
        /// <param name="from">email address of the sender</param>
        /// <param name="to">email address of the recipient</param>
        /// <param name="subject">eMail Subject (aka header)</param>
        /// <param name="body">text of the eMail</param>
        /// <param name="attachment">byte array with attachment</param>
        /// <param name="filename">filename of the attachment</param>
        public static void Send(MailAddress from, MailAddress to, string subject, string body, byte[] attachment, string filename)
        {
            Send(from, to, null, subject, body, attachment, filename);
        }

        /// <summary>
        /// Send an eMail
        /// </summary>
        /// <param name="from">email address of the sender</param>
        /// <param name="to">email address of the recipient</param>
        /// <param name="cc">email address of copy recipient</param>
        /// <param name="subject">eMail Subject (aka header)</param>
        /// <param name="body">text of the eMail</param>
        public static void Send(MailAddress from, MailAddress to, MailAddress cc, string subject, string body)
        {
            Deliver(Header(from, to, cc, subject), body);
        }

        /// <summary>
        /// Send an eMail
        /// </summary>
        /// <param name="from">email address of the sender</param>
        /// <param name="to">email address of the recipient</param>
        /// <param name="cc">email address of copy recipient</param>
        /// <param name="subject">eMail Subject (aka header)</param>
        /// <param name="body">text of the eMail</param>
        /// <param name="attachment">byte array with attachment</param>
        /// <param name="filename">filename of the attachment</param>
        public static void Send(MailAddress from, MailAddress to, MailAddress cc, string subject, string body, byte[] attachment, string filename)
        {
            Deliver(Attach(Header(from, to, cc, subject), attachment, filename), body);
        }

        /// <summary>
        /// Make an eMail header
        /// </summary>
        /// <param name="from">email address of the sender</param>
        /// <param name="to">email address of the recipient</param>
        /// <param name="cc">email address of copy recipient</param>
        /// <param name="subject">eMail Subject (aka header)</param>
        /// <returns>new SendGridMessage, which can be passed into Deliver()</returns>
        public static SendGridMessage Header(MailAddress from, MailAddress to, MailAddress cc, string subject)
        {
            // Create the email object first, then add the properties.
            SendGridMessage msg = new SendGridMessage();

            // Add the message properties.
            msg.From = new EmailAddress(from.Address, from.DisplayName);
            msg.AddTo(new EmailAddress(to.Address, to.DisplayName));
            if (cc != null) msg.AddCc(new EmailAddress(cc.Address, cc.DisplayName));
            msg.Subject = subject;

            msg.SetClickTracking(false, false);

            return msg;
        } 

        /// <summary>
        /// Send an eMail
        /// </summary>
        /// <param name="from">email address of the sender</param>
        /// <param name="to">List of recipient email addressses</param>
        /// <param name="subject">eMail Subject (aka header)</param>
        /// <param name="body">text of the eMail</param>
        public static void Send(string from, List<string> to, string subject, string body)
        {
            Send(from, to, null, subject, body);
        }

        /// <summary>
        /// Send an eMail
        /// </summary>
        /// <param name="from">email address of the sender</param>
        /// <param name="to">List of recipient email addressses</param>
        /// <param name="subject">eMail Subject (aka header)</param>
        /// <param name="body">text of the eMail</param>
        /// <param name="attachment">byte array with attachment</param>
        /// <param name="filename">filename of the attachment</param>
        public static void Send(string from, List<string> to, string subject, string body, byte[] attachment, string filename)
        {
            Send(from, to, null, subject, body, attachment, filename);
        }

        /// <summary>
        /// Send an eMail
        /// </summary>
        /// <param name="from">email address of the sender</param>
        /// <param name="to">List of recipient email addressses</param>
        /// <param name="cc">List of email addresses of copy recipients</param>
        /// <param name="subject">eMail Subject (aka header)</param>
        /// <param name="body">text of the eMail</param>
        public static void Send(string from, List<string> to, List<string> cc, string subject, string body)
        {
            Send(new MailAddress(from), AddressList(to), AddressList(cc), subject, body);
        }

        /// <summary>
        /// Send an eMail
        /// </summary>
        /// <param name="from">email address of the sender</param>
        /// <param name="to">List of recipient email addressses</param>
        /// <param name="cc">List of email addresses of copy recipients</param>
        /// <param name="subject">eMail Subject (aka header)</param>
        /// <param name="body">text of the eMail</param>
        /// <param name="attachment">byte array with attachment</param>
        /// <param name="filename">filename of the attachment</param>
        public static void Send(string from, List<string> to, List<string> cc, string subject, string body, byte[] attachment, string filename)
        {
            Send(new MailAddress(from), AddressList(to), AddressList(cc), subject, body, attachment, filename);
        }

        private static List<MailAddress> AddressList(List<string> addresses)
        {
            if (addresses == null) return null;
            List<MailAddress> l = new List<MailAddress>();
            foreach (string s in addresses) l.Add(new MailAddress(s)); 
            return l;
        }

        /// <summary>
        /// Send an eMail
        /// </summary>
        /// <param name="from">email address of the sender</param>
        /// <param name="to">List of recipient email addressses</param>
        /// <param name="subject">eMail Subject (aka header)</param>
        /// <param name="body">text of the eMail</param>
        public static void Send(MailAddress from, List<MailAddress> to, string subject, string body)
        {
            Send(from, to, null, subject, body);
        }

        /// <summary>
        /// Send an eMail
        /// </summary>
        /// <param name="from">email address of the sender</param>
        /// <param name="to">List of recipient email addressses</param>
        /// <param name="subject">eMail Subject (aka header)</param>
        /// <param name="body">text of the eMail</param>
        /// <param name="attachment">byte array with attachment</param>
        /// <param name="filename">filename of the attachment</param>
        public static void Send(MailAddress from, List<MailAddress> to, string subject, string body, byte[] attachment, string filename)
        {
            Send(from, to, null, subject, body, attachment, filename);
        }

        /// <summary>
        /// Send an eMail
        /// </summary>
        /// <param name="from">email address of the sender</param>
        /// <param name="to">List of recipient email addressses</param>
        /// <param name="cc">List of cc recipient email addressses</param>
        /// <param name="subject">eMail Subject (aka header)</param>
        /// <param name="body">text of the eMail</param>
        public static void Send(MailAddress from, List<MailAddress> to, List<MailAddress> cc, string subject, string body)
        {
            Deliver(Header(from, to, cc, subject), body);
        }


        /// <summary>
        /// Send an eMail
        /// </summary>
        /// <param name="from">email address of the sender</param>
        /// <param name="to">List of recipient email addressses</param>
        /// <param name="cc">List of cc recipient email addressses</param>
        /// <param name="subject">eMail Subject (aka header)</param>
        /// <param name="body">text of the eMail</param>
        /// <param name="attachment">byte array with attachment</param>
        /// <param name="filename">filename of the attachment</param>
        public static void Send(MailAddress from, List<MailAddress> to, List<MailAddress> cc, string subject, string body, byte[] attachment, string filename)
        {
            Deliver(Attach(Header(from, to, cc, subject), attachment, filename), body);
        }

        /// <summary>
        /// Make an eMail header
        /// </summary>
        /// <param name="from">email address of the sender</param>
        /// <param name="to">List of recipient email addressses</param>
        /// <param name="cc">List of cc recipient email addressses</param>
        /// <param name="subject">eMail Subject (aka header)</param>
        /// <returns>new SendGridMessage, which can be passed into Deliver()</returns>
        public static SendGridMessage Header(MailAddress from, List<MailAddress> to, List<MailAddress> cc, string subject)
        {
            // Create the email object first, then add the properties.
            SendGridMessage msg = new SendGridMessage();

            // Add the message properties.
            msg.From = new EmailAddress(from.Address, from.DisplayName);
            foreach(MailAddress a in to) msg.AddTo(new EmailAddress(a.Address, a.DisplayName));
            if (cc != null) foreach(MailAddress c in cc) msg.AddCc(new EmailAddress(c.Address, c.DisplayName));
            msg.Subject = subject;

            msg.SetClickTracking(false, false);

            return msg;
        }

        /// <summary>
        /// Attach a file to an eMail
        /// </summary>
        /// <param name="msg">email message created by Header()</param>
        /// <param name="attachment">byte array with attachment</param>
        /// <param name="filename">filename of the attachment</param>
        public static SendGridMessage Attach(SendGridMessage msg, byte[] attachment, string filename)
        {         
            msg.AddAttachment(filename, Convert.ToBase64String(attachment));
            return msg;
        }

        /// <summary>
        /// Deliver an eMail
        /// </summary>
        /// <param name="msg">email message created by Header()</param>
        /// <param name="body">text of the eMail</param>
        public static void Deliver(SendGridMessage msg, string body) 
        {
            if (!ApiKey.HasValue()) throw new Exception("No ApiKey in SendMail");

            msg.PlainTextContent = body;

            SendGridClient client = new SendGridClient(ApiKey);

            // Send the email.
            client.SendEmailAsync(msg).Wait();
        }
    }    
}
