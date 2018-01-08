using System;
using System.Drawing;
using System.Text;
using System.Web.UI.WebControls;
using SyncObjX.Web;

namespace SyncObjX.Util
{
    public class ErrorHelper
    {
        public static string FormatException(string componentName, Exception ex)
        {
            StringBuilder msg = new StringBuilder();

            msg.AppendLine();
            msg.AppendLine();
            msg.AppendLine();

            msg.AppendLine("An unexpected error has occurred.  Please notify your System Administrator.");

            msg.AppendLine("****ENVIRONMENT DETAILS****");
            msg.AppendLine("Component Name: " + componentName);
            msg.AppendLine("Date/time: " + DateTime.Now.ToString());

            do
            {
                msg.AppendLine("****EXCEPTION DETAILS****");
                msg.AppendLine("Source: " + ex.Source);
                msg.AppendLine("Message: " + ex.Message);
                msg.AppendLine("Target Site: " + ex.TargetSite);
                msg.AppendLine("Stack Trace: " + ex.StackTrace);

                msg.AppendLine();
                msg.AppendLine();

                ex = ex.InnerException;
            }
            while (ex != null);

            return msg.ToString();
        }

        public static string FormatExceptionForWeb(int currentUserId, string componentName, string pageName, Exception ex)
        {
            StringBuilder msg = new StringBuilder();

            msg.Append(HtmlBuilder.GetLineBreak(3));

            msg.AppendLine(HtmlBuilder.WrapInBoldTags("An unexpected error has occurred.  Please notify your System Administrator.") + HtmlBuilder.GetLineBreak(2));

            msg.Append(HtmlBuilder.WrapInBoldTags("****ENVIRONMENT DETAILS****") + HtmlBuilder.GetLineBreak());
            msg.Append(HtmlBuilder.WrapInBoldTags("Component Name: ") + componentName + HtmlBuilder.GetLineBreak());
            msg.Append(HtmlBuilder.WrapInBoldTags("Page Name: ") + pageName + HtmlBuilder.GetLineBreak());
            msg.Append(HtmlBuilder.WrapInBoldTags("UserId: ") + currentUserId.ToString() + HtmlBuilder.GetLineBreak());
            msg.Append(HtmlBuilder.WrapInBoldTags("Date/time: ") + DateTime.Now.ToString() + HtmlBuilder.GetLineBreak(2));

            do
            {
                msg.Append(HtmlBuilder.WrapInBoldTags("****EXCEPTION DETAILS****") + HtmlBuilder.GetLineBreak());
                msg.Append(HtmlBuilder.WrapInBoldTags("Source: ") + ex.Source + HtmlBuilder.GetLineBreak());
                msg.Append(HtmlBuilder.WrapInBoldTags("Message: ") + ex.Message + HtmlBuilder.GetLineBreak());
                msg.Append(HtmlBuilder.WrapInBoldTags("Target Site: ") + ex.TargetSite + HtmlBuilder.GetLineBreak());
                msg.Append(HtmlBuilder.WrapInBoldTags("Stack Trace: ") + ex.StackTrace);

                msg.Append(HtmlBuilder.GetLineBreak(2));

                ex = ex.InnerException;
            }
            while (ex != null);

            Literal lit = new Literal();
            lit.Text = msg.ToString();

            Panel pnl = new Panel();
            pnl.ForeColor = ColorTranslator.FromHtml("#4d4f53");
            pnl.Font.Names = new string[] { "Tahoma", "Arial" };
            pnl.Font.Size = new FontUnit("13px");
            pnl.Controls.Add(lit);

            return HtmlBuilder.GetServerControlHtml(pnl);
        }
    }
}
