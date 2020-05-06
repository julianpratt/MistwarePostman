using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Threading;
using Mistware.Utils;
using Mistware.Files;
using Mistware.Postman;

namespace MistwarePostmanTest
{
	class Program
	{
		static void Main(string[] args)
		{
            Config.Setup("appsettings.json", Directory.GetCurrentDirectory(), null, "MistwarePostmanTest");

            string connection = Config.Get("AzureConnectionString");
            string container  = Config.Get("AzureContainer");
            string logs       = Config.Get("Logs");
            IFile filesys = FileBootstrap.SetupFileSys(connection, container, null, logs);
            Log.Me.LogFile = "MistwarePostmanTest.log";
            
            EmailBatch batch  = new EmailBatch();
            batch.Postmaster  = new MailAddress("noreply@" + Config.Get("TestDomain"), "ACME Postmaster");
            batch.From        = new MailAddress(Config.Get("TestEmail"), Config.Get("TestPerson"));
            batch.Name        = "Test Batch";
            batch.Recipients  = new List<EmailRecipient>();

            EmailRecipient r1 = new EmailRecipient();
            r1.To = new MailAddress(Config.Get("TestEmail"), Config.Get("TestPerson"));
            r1.DeliveryType = "Summary";
            r1.Attachment = null;
            r1.MailMergeFields = new Dictionary<string, string>();
            r1.MailMergeFields.Add("FromName",  "Fred Bloggs");
            r1.MailMergeFields.Add("FromEmail", "fred.bloggs@gmx.com");
            r1.MailMergeFields.Add("ISBN",      "9780141198354");
            r1.MailMergeFields.Add("Title",     "Bleak House");
            r1.MailMergeFields.Add("Authors",   "Charles Dickens");
            r1.MailMergeFields.Add("Summary",   "A satirical story about the British judiciary system. \nEsther Summerson is a lonely girl who was raised by her aunt and is taken in by John Jarndyce, a rich philanthropist. Parts of the story are told from her point of view.");
            r1.MailMergeFields.Add("Link",      "https://en.wikipedia.org/wiki/Bleak_House");
            batch.Recipients.Add(r1);

            EmailRecipient r2 = new EmailRecipient();
            r2.To = new MailAddress(Config.Get("TestEmail"), Config.Get("TestPerson"));
            r2.DeliveryType = "Full";
            r2.Attachment = "BleakHouse.pdf";
            r2.MailMergeFields = new Dictionary<string, string>();
            r2.MailMergeFields.Add("FromName",  "Fred Bloggs");
            r2.MailMergeFields.Add("FromEmail", "fred.bloggs@gmx.com");
            r2.MailMergeFields.Add("ISBN",      "9780141198354");
            r2.MailMergeFields.Add("Title",     "Bleak House");
            r2.MailMergeFields.Add("Authors",   "Charles Dickens");
            batch.Recipients.Add(r2);

            List<EmailBatch> batches = new List<EmailBatch>();
            batches.Add(batch);
        
            Log.Me.Info("About to kick off the Emailing Thread");
            MailEngine engine = new MailEngine();
            engine.SendGridKey = Config.Get("SendGridKey");
            engine.FileSys = filesys;
            engine.LoadTemplates(Directory.GetCurrentDirectory() + "/", "EmailTemplates.txt");
            filesys.ChangeDirectory(Config.Get("UploadDirectory"));
            filesys.FileUpload(Directory.GetCurrentDirectory() + "/" + "BleakHouse.pdf");
            engine.Start(batches);

            Thread.Sleep(10000); // Wait 10 secs, so thread will finish before we kill the app.
            filesys.FileDelete("BleakHouse.pdf");
        }
     
    }
}