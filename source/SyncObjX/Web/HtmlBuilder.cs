using System;
using System.IO;
using System.Web.UI;

namespace SyncObjX.Web
{
    public class HtmlBuilder
    {
        public static string GetServerControlHtml(System.Web.UI.Control ctrl)
        {
            StringWriter writer = new StringWriter();
            HtmlTextWriter htmlWriter = new HtmlTextWriter(writer);
            ctrl.RenderControl(htmlWriter);
            return writer.ToString();
        }

        public static string GetLineBreak(int count = 1)
        {
            string html = "<br/>";

            for (int i = 1; i < count; i++)
                html += "<br/>";

            return html;
        }

        public static string WrapInBoldTags(string text)
        {
            if (String.IsNullOrEmpty(text))
                return "";
            else
                return "<b>" + text + "</b>";
        }
    }
}