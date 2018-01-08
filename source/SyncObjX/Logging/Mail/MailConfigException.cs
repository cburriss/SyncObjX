using System;

namespace SyncObjX.Logging.Mail
{
    [Serializable]
    public class MailConfigException : MailException
    {
        public MailConfigException(string message, Exception innerException) : base(message, innerException) { }
    }
}