using System;
using System.Collections.Generic;
using System.Text;

namespace SyncObjX.Web
{
    public class UrlBuilder
    {
        public string BaseUrl { get; set; }

        public bool HasParametersInBaseUrl { get; set; }

        public Dictionary<string, string> Parameters = new Dictionary<string, string>();

        public string GetUrl()
        {
            StringBuilder sb = new StringBuilder();

            if (String.IsNullOrEmpty(BaseUrl))
                throw new Exception("BaseUrl must have a value.");
            else
            {
                sb.Append(BaseUrl);

                if (!HasParametersInBaseUrl)
                    sb.Append("?");

                foreach (KeyValuePair<string, string> kvp in Parameters)
                    sb.Append(String.Format("{0}={1}&", kvp.Key, kvp.Value));

                sb.Remove(sb.Length - 1, 1);

                return sb.ToString();
            }
        }

        public override string ToString()
        {
            return GetUrl();
        }
    }
}