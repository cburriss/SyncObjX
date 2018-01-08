using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SyncObjX.Management
{
    [DataContract]
    public class WebServiceException
    {
        [DataMember]
        public string SourceExceptionTypeName;

        [DataMember]
        public string Source;

        [DataMember]
        public string Message;

        [DataMember]
        public string TargetSite;

        [DataMember]
        public string StackTrace;

        [DataMember]
        public WebServiceException InnerException;

        public static List<WebServiceException> Convert(IEnumerable<Exception> exceptions)
        {
            List<WebServiceException> webServiceExceptions = new List<WebServiceException>();

            foreach (var exception in exceptions)
	        {
                var webServiceException = Convert(exception);

                webServiceExceptions.Add(webServiceException);
	        }

            return webServiceExceptions;
        }

        public static WebServiceException Convert(Exception exception)
        {
            var webServiceException = new WebServiceException();

            webServiceException.SourceExceptionTypeName = exception.GetType().FullName;
            webServiceException.Source = exception.Source;
            webServiceException.Message = exception.Message;
            webServiceException.TargetSite = exception.TargetSite == null ? null : exception.TargetSite.ToString();
            webServiceException.StackTrace = exception.StackTrace;

            if (exception.InnerException != null)
                webServiceException.InnerException = Convert(exception.InnerException);

            return webServiceException;
        }
    }
}
