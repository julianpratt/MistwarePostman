using System.Collections.Generic;
using System.Net.Mail;

namespace Mistware.Postman
{
    /// Defines a batch of emails to be sent
    public class EmailBatch
    {
        /// A description of the batch (used when reporting the success
        /// or otherwise back to the person in whose name the batch was sent).
        public string                    Name            { get; set; }
        
        /// The email address and name of the person sending the batch
        public MailAddress               From            { get; set; }

        /// The email address and name that is used to send each email
        /// n.b. This often differs from From, when the From email address 
        ///      belongs to an internal domain (emails would get rejected)
        ///      as they arrive in the internal domain).
        public MailAddress               Postmaster      { get; set; }

        /// A list of email recipients - this is the batch to send
        public List<EmailRecipient>      Recipients      { get; set; }
    }
}