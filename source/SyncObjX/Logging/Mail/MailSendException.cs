using System;

namespace SyncObjX.Logging.Mail
{
    [Serializable]
    public class MailSendException : MailException
    {
        public MailSendException(string message, Exception innerException) : base(message, innerException) { }
    }
}
