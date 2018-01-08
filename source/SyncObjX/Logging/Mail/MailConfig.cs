using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace SyncObjX.Logging.Mail
{
    public class MailConfig
    {
        public MailAddress FromAddress;

        public IEnumerable<MailAddress> ToAddresses;

        public string Host;

        public bool EnableSsl;

        public int Port;

        public ICredentialsByHost Credentials;

        public MailConfig(MailAddress fromAddress, IEnumerable<MailAddress> toAddresses, string smtpHost)
            : this(fromAddress, toAddresses, smtpHost, 25, null, false) { }

        public MailConfig(MailAddress fromAddress, IEnumerable<MailAddress> toAddresses, string smtpHost, int smtpPort)
            : this(fromAddress, toAddresses, smtpHost, smtpPort, null, false) { }

        public MailConfig(MailAddress fromAddress, IEnumerable<MailAddress> toAddresses, string smtpHost, int smtpPort, ICredentialsByHost credentials, bool enableSsl = true)
        {
            if (fromAddress == null)
                throw new Exception("From address can not be null.");

            if (toAddresses == null || toAddresses.Count() == 0)
                throw new Exception("At least one To email address is required.");

            if (smtpHost == null || smtpHost.Trim() == string.Empty)
                throw new Exception("SMTP host name is missing or empty.");

            FromAddress = fromAddress;
            ToAddresses = toAddresses;
            Host = smtpHost;
            Port = smtpPort;
            Credentials = credentials;
            EnableSsl = enableSsl;
        }
    }
}
