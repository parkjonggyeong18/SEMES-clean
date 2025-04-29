using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Infra.EmailServices
{
    public class EmailService : IEmailService
    {
        /// <summary>
        /// This class implements the IEmailService interface together with its methods for sending e-mail messages,
        /// for this the SmtpClient object is initialized.
        /// Do not forget to configure the connection to the sending mail server (MailServer), with your email data 
        /// (the one that will be in charge of sending the email messages).
        /// In case you use Gmail as the sender email, you must activate the option Allow access of insecure applications
        /// so that your application can send emails.
        /// Read the quick guide for more information.
        /// </summary>

        #region -> Fields

        private SmtpClient smtpClient;//Gets or sets the SMTP client
        private struct MailServer//Get or set the mail server who is responsible for sending the mail messages.
        {
            //Here you make the Simple Mail Transfer Protocol (SMTP) settings

            public const string Address = "VitalClinicSystem@gmail.com"; // Set the address of the sender mail server (Here you put the email responsible for sending the emails).
            public const string Password = "@admin123"; // Set the password of the sender server mail (Here put the password of the mail that you established in the previous line).
            public const string DisplayName = "Vital Clinic"; // Sets the name to display when sending an email.
            public const string Host = "smtp.gmail.com"; // Sets the host name or IP address for SMTP transactions.
            public const int Port = 587; // Sets the port used for SMTP transactions.
            public const bool SSL = true; // Sets whether the SMTP client uses Secure Sockets Layer (SSL) to encrypt the connection.
        };
        #endregion

        #region -> Constructor

        public EmailService()
        {
            smtpClient = new SmtpClient();//Initialize SMTP client.
            smtpClient.Credentials = new NetworkCredential(MailServer.Address, MailServer.Password);//Set the credentials (User and password).
            smtpClient.Host = MailServer.Host;//Set the host.
            smtpClient.Port = MailServer.Port;//Set the port.
            smtpClient.EnableSsl = MailServer.SSL;//Set SSL encryption.
        }
        #endregion

        #region -> Methods

        public void Send(string recipient, string subject, string body)
        {//This method is responsible for sending an email message to only one recipient.

            var mailMessage = new MailMessage();//Initialize the mail message object.
            var mailSender = new MailAddress(MailServer.Address, MailServer.DisplayName);//Initialize the sender email address object.

            try
            {
                mailMessage.From = mailSender;// Set the sender email address.
                mailMessage.To.Add(recipient); // Set and add the recipient email address.
                mailMessage.Subject = subject; // Set the subject of the mail message.
                mailMessage.Body = body; // Set the content of the mail message.
                mailMessage.Priority = MailPriority.Normal; // Set the priority of the mail message.

                smtpClient.Send(mailMessage);//Send the mail message using the SMTP (Simple Mail Transfer Protocol) client.
            }
            catch (SmtpException ex)
            {
                throw new ArgumentException(ex.Message);
            }
            finally
            {
                mailMessage.Dispose();
            }
        }
        public void Send(List<string> recipients, string subject, string body)
        {//This method is responsible for sending a mail message to multiple recipients.

            var mailMessage = new MailMessage();//Initialize the mail message object.
            var mailSender = new MailAddress(MailServer.Address, MailServer.DisplayName);//Initialize the sender email address object.

            try
            {
                mailMessage.From = mailSender; // Set the sender email address.
                foreach (var recipientMail in recipients) // Loop through the list of recipients to get each email address.
                {
                    mailMessage.To.Add(recipientMail); // Add the recipient email address to the collection of email addresses.
                }
                mailMessage.Subject = subject; // Set the subject of the mail message.
                mailMessage.Body = body; // Set the content of the mail message.
                mailMessage.Priority = MailPriority.Normal; // Set the priority of the mail message.

                smtpClient.Send(mailMessage); // Send the mail message using the SMTP (Simple Mail Transfer Protocol) client.
            }
            catch (SmtpException ex)
            {
                throw new ArgumentException(ex.Message);
            }
            finally
            {
                mailMessage.Dispose();
            }
        }
        #endregion

    }
}
