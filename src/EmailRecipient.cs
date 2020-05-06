using System.Collections.Generic;
using System.Net.Mail;

namespace Mistware.Postman
{
    /// An email to send (part of a batch)
    public class EmailRecipient
    {
        /// The email address and name of the person to send the email to
        public MailAddress               To              { get; set; }

        /// The delivery type of the email, which needs to match a template code
        /// n.b. delvery types beginning "EF" should have an attachment
        public string                    DeliveryType    { get; set; }

        /// A string dictionary of Mail Merge Fields.
        /// Curly braces are wrapped around each key (e.g. {Title}) 
        /// and the resulting string is matched in the template (Subject and Body) 
        /// and replaced with the Dictionary value. 
        public Dictionary<string,string> MailMergeFields { get; set; }

        /// The filename of the attachment
        public string                    Attachment      { get; set; }
    }
}