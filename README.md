Mistware.Postman - a Mail Merge Engine
========================================

Mistware is an identity chosen to evoke the concept of [Vapourware](https://en.wikipedia.org/wiki/Vaporware).

This Mail Merge Engine enables a web app to quickly construct a list of emails to send, and then have this list processed as a background thread. It uses SendGrid to send the emails. Each email has a plain text body, and an optional attachment. The subject and body of each email is generated from a template. 

Emails are sent as batch of related emails from one sender. Each email batch consists of:
1. Name - The name of the batch.
2. From - The person sending the batch (email address and name).
3. The email to be used as the sending address (email address and name) - Postmaster.
4. A list of email recipients (see next) - Recipients.

It is typically the case that Postman (i.e. SendGrid) is used outside an internal company network. In that case emails cannot be sent from the sender (From email address and name), because those emails will be blocked by the company's firewall / mail gateway (which quite rationally rejects external emails with an internal domain). Instead emails must be sent from a fictitious 'Postmaster' email address (which has a domain outside the company domain).   

Each email recipient consists of: 
1. A recipient (email address and name) - To.
2. A delivery type - a code the selects the respective template - DeliveryType.
3. A dictionary of parameters to substitute into the template - MailMergeFields.
4. The filename of an optional attachment (if the filename is null, then no attachment is sent) - Attachment.

The templates are stored in a single file, where each tempate is identfied by a code (delivery type), for example the XYZ template:

```
%%XYZ-Subject
The email subject line goes here with a  : {Reference}
%%XYZ-Body
Please find attached a summary of the ACME Corporation report:

Report Number : {Reference}

Title : {Title}

Author : {Author}

{Summary}

%%ABC-Subject
etc ...
```

Other templates (e.g. the ABC template) follow in the same file. Templates come in pairs (e.g. XYZ-Subject and XYZ-Body). The double percentage ("%%") is the delimiter indicating the start of the next template. Each template field (e.g. {Reference}) must be a key in the dictionary. For example this template expects the keys: Reference, Title, Author and Summary. If any keys are omitted then the email is sent, but the field is not replaced with a value. 

Attachments are obtained using a Mistware.Files file system (either local or an Azure File Share). If no attachments are to be sent, then the file system can be left uninitialised (though the dependency of Mistware.Files will remain). In addition to setting up the file system, it is also necessary to change to the directory on the file system that contains the files to be attached. Each attachment's filename is specified with the recipient, though it is anticpated that a batch will typically send just one attachment (e.g. one report). This makes it possible to send a mixture of emails, some with and some without attachments, in the same batch.   

The classes are:
- MailEngine - the core of this package, which exposes: a property that must be set (SendGridKey), a property that must be set if attachments are to be sent (FileSys), a method to load the templates and a method to send a list of batches.  
- EmailBatch - the definition of each email batch
- EMailRecipient - the definition of each recipient

Other classes are internal to the package and can be ignored (including SendMail, which is an interface to the SendGrid package).


Usage
--------

To add the nuget package to a .Net Core application:

```
dotnet add package Mistware.Postman
```

Dependences include the following packages: Mistware.Utils, Mistware.Files and SendGrid.

All that is needed to use the mail engine is to create the list of EmailBatch (including a list of EmailRecipient, each with a dictionary of template or mail merge fields). And then to call the following:

```
IFile filesys = FileBootstrap.SetupFileSys(connection, container, root, logs);
filesys.ChangeDirectory(folder);

MailEngine engine = new MailEngine();
engine.SendGridKey = key; // SendGrid API key from configuration
engine.FileSys = filesys;
engine.LoadTemplates(path, filename);
engine.Start(batches); // where batches is List<EmailBatch>
```

See Mistware.Files documentation on the use of FileBootstrap and ChangeDirectory.

Mistware.Postman uses the logging provided by Mistware.Utils and Mistware.Files (see their respective documentation). Either a rolling file log on local file storage or logging on Azure BLOB storage. FileBootstrap will setup the logging as well as the file system.


Testing
---------------------
Mistware.Postman has a basic test program in the test folder (MistwarePostmanTest.csproj), which is in itself an example of how to use this package. 
