using System;

namespace SyncObjX.Logging.Mail
{
    [Serializable]
    public abstract class MailException : ApplicationException
    {
        public MailException(string message, Exception innerException) : base(message, innerException) { }
    }
}
