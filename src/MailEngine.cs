using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading;
using Mistware.Files;
using Mistware.Utils;


namespace Mistware.Postman
{    
    /// Background worker thread to send emails
    public class MailEngine
    {
        private List<EmailBatch> Batches    { get; set; }

        /// SendGrid API Key (obtain from configuration)
        public  string           SendGridKey 
        {
            get { return SendMail.ApiKey;  } 
            set { SendMail.ApiKey = value; }
        }

        /// Mistware.Files File System (use FileBootstrap to construct this)
        public  IFile            FileSys    { get; set; }
        private Thread           _oThread = null;

        /// Kick off the background thread
        public void Start(List<EmailBatch> batches)
        {
            if (SendGridKey == null)
            {
                Log.Me.Error("Cannot send emails from MailEngine. No SendGridKey has been provided.");
                return;
            }
            if (MailMerge.Me.Templates == null)
            {
                Log.Me.Error("Cannot send emails from MailEngine. No Templates have been provided.");
                return;
            }

            Batches  = batches;
            if (Batches.Count <= 0)
            {
                // No emails need to be sent.
                Log.Me.Info("No emails to send");
                return;
            }

            if (_oThread == null)
            {
                _oThread = new Thread(ThreadFunction);
                _oThread.IsBackground = true;
                _oThread.Priority = ThreadPriority.Lowest;
                _oThread.Start();
            }
        }
        
         /// Load the templates from a txt file
        public void LoadTemplates(string path, string filename)
        {
            MailMerge.Me.LoadTemplates(path, filename);
        }

        private void ThreadFunction()
        {        
            Log.Me.Info("Start Email Thread");

            Log.Me.Debug(Batches.Count.ToString() + " batches of emails to send");

            foreach (EmailBatch batch in Batches)
            { 
                ArrayList sEmailsSent = new ArrayList();   // String list to hold the lines for confirmation email

                foreach (EmailRecipient recipient in batch.Recipients)
                {
                    SendEMail(recipient, batch.Postmaster, batch.From);
                    //-------------------------------------------------------------------------
                    // Log the fact that we have sent this email successflly, or at least
                    // it looks like we've sent it successfullt.Then at the end -send a report
                    // to the Person doing the commit to say that the emails have been sent.
                    //-------------------------------------------------------------------------
                    sEmailsSent.Add(string.Format("{0}  -  {1}", recipient.DeliveryType, recipient.To.ToString()));
                }
   
                //-----------------------------------------------------------------------------------------------
                // Now we've processed all the emails in this batch - send a confirmation to the Sender that 
                // everything has worked ok
                //-----------------------------------------------------------------------------------------------
                string now =  String.Format("{0:f}", DateTime.Now);
                string body = "The " + batch.Name + " email batch was successfully distributed at " + now + "\n\n";
                if (sEmailsSent.Count > 0)
                {
                    body += "The batch has been sent electronically to the following recipients :-\n\n";
                    foreach (string s in sEmailsSent) body += s + "\n";
                    body += "\nIf there were any difficulties sending any of the emails, you should receive separate email notification of the error from the Postmaster."; 
                }
                else 
                {
                    body += "The email batch has NOT been sent electronically to any recipients\n";
                }
                try
                {
                    SendMail.Send(batch.Postmaster, batch.From, batch.Name + " - Distribution Confirmation", body);
                }
                catch (Exception ex)
                {
                    try
                    {
                        Log.Me.Error("=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*");
                        Log.Me.Error("----Exception Raised----");
                        Log.Me.Error(ex.Message);
                        Log.Me.Error("");
                        Log.Me.Error("ABORTING PROCESS");
                        Log.Me.Error("=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*=*");
                    }
                    catch { }
                }
            }
            
            Log.Me.Info("End Email Thread");
        }

        private void SendEMail(EmailRecipient recipient, MailAddress postmaster, MailAddress from)
        {
            string subject = "";
            string body    = "";

            try
            {
                Log.Me.Debug("------------------------------------------------------------");
                Log.Me.Debug("Building Email Message");
                Log.Me.Debug("  To            : " + recipient.To.ToString());
                Log.Me.Debug("  From          : " + postmaster.ToString());

                string deliveryType = recipient.DeliveryType;
                                            
                // Email body and subject
                Log.Me.Debug("  Delivery Type : " + deliveryType);

                subject = MailMerge.Me.GetSubject(deliveryType, recipient.MailMergeFields);
                if (subject == null) throw new Exception("Mailmerge failure. Stopping. See Log above.");

                Log.Me.Debug("  Subject : ");
                Log.Me.Debug(subject);
                Log.Me.Debug("");
                        
                body = MailMerge.Me.GetBody(deliveryType, recipient.MailMergeFields);
                if (body == null) throw new Exception("Mailmerge failure. Stopping. See Log above.");

                Log.Me.Debug("  Body : ");
                Log.Me.Debug("---------------------------------");
                Log.Me.Debug(body);
                Log.Me.Debug("---------------------------------");
                        
                //-------------------------------------------------------------------------
                // Send the Email
                //-------------------------------------------------------------------------
                Log.Me.Debug("Sending Email....");
                            
                string attachment = recipient.Attachment;
                if (attachment != null)
                {
                    Log.Me.Debug("Attachment is " + attachment);
                    if (!FileSys.FileExists(attachment))
                    {
                        Log.Me.Error("Cannot send email with " + attachment + " - it isn't there.");
                    }
                    else if (FileSys == null)
                    {
                        Log.Me.Error("Cannot send email with " + attachment + " - File System has not been provided.");
                    }
                    else
                    {
                        long filelen = FileSys.FileLength(attachment);
                        Log.Me.Debug(attachment + " is " + filelen.ToString() + " bytes long.");
                        byte[] file = FileSys.FileDownload(attachment);
                        Log.Me.Debug("Attachment Downloaded");
                        SendMail.Send(postmaster, recipient.To, subject, body, file, attachment);
                    }
                }      
                else 
                {
                    SendMail.Send(postmaster, recipient.To, subject, body);
                }
                
                Log.Me.Info("Email SENT to " + recipient.To.ToString());

                           
            }
            catch (Exception ex)
            {
                //---------------------------------------------------------------------------------
                // If we get an Error from Sending the Email - Log it and Email the Error to the
                // person sending this email - crazy I know...
                //---------------------------------------------------------------------------------

                //---------------------------------------------------------------------------------
                // Find the first Exception thrown in the stack
                //---------------------------------------------------------------------------------
                Exception xInner = ex;
                while (xInner.InnerException != null) {Log.Me.Error(xInner.Message); xInner = xInner.InnerException;}
                Log.Me.Error("ERROR Sending Email - " + xInner.Message);
                Log.Me.Debug("Attempting to Send details of this Error to the Sender");

                string errbody = "Encountered an error while attempting to send an email.\n\n";
                errbody += "Error Message :\n" + xInner.Message;
                errbody += "--Original Message------------------------------------------------------------------\n";
                errbody += "To      : " + recipient.To.ToString() + "\n";
                errbody += "From    : " + postmaster.ToString() + "\n";
                errbody += "Subject : " + subject + "\n";
                errbody += "Message  : \n";
                errbody += body;

                try
                {
                    Log.Me.Info("Sending Error Report Email to " + from.ToString());
                    SendMail.Send(postmaster, from, "Error Sending email with subject: " + subject, errbody);
                }
                catch (Exception ex2)
                {
                    //--------------------------------------------------------------------------------
                    // If we then get an error trying to send the error email to the sender - 
                    // Abort the whole thing and leave the log file
                    //--------------------------------------------------------------------------------
                    Log.Me.Fatal("Error sending the Error Email - giving up!");
                    throw ex2;
                }
            }
        }      
    }  
}