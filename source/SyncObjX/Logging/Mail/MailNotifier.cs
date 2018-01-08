using System;
using System.Net.Mail;

namespace SyncObjX.Logging.Mail
{
    public class MailNotifier
    {
        public readonly MailConfig Config;

        public MailNotifier(MailConfig mailConfig)
        {
            Config = mailConfig;
        }

        public void SendMessage(string subject, string body)
        {
            try
            {
                if (subject == null || subject.Trim() == string.Empty)
                    throw new Exception("Email subject is missing or empty.");

                var smtpClient = new SmtpClient();
                smtpClient.Host = Config.Host;
                smtpClient.Port = Config.Port;
                smtpClient.EnableSsl = Config.EnableSsl;
                smtpClient.Credentials = Config.Credentials;

                using (var mailMsg = new MailMessage())
                {
                    mailMsg.From = Config.FromAddress;

                    foreach (var toAddress in Config.ToAddresses)
                        mailMsg.To.Add(toAddress);

                    mailMsg.Subject = subject;
                    mailMsg.Body = body;
                    mailMsg.IsBodyHtml = true;

                    smtpClient.Send(mailMsg);
                }
            }
            catch (Exception ex)
            {
                var mailEx = new MailSendException("A mail failure occurred. The message could not be sent.", ex);

                SyncEngineLogger.WriteExceptionToLog(mailEx);
            }
        }
    }
}
