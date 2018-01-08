using System;
using System.Drawing;
using System.Text;
using System.Web.UI.WebControls;
using SyncObjX.Web;

namespace SyncObjX.Logging
{
    public class ExceptionFormatter
    {
        public static string Format(Exception ex)
        {
            StringBuilder msg = new StringBuilder();

            bool isInnerException = false;

            do
            {
                if (isInnerException)
                    msg.AppendLine("****INNER EXCEPTION DETAILS****");
                else
                    msg.AppendLine("****EXCEPTION DETAILS****");

                msg.AppendLine("Source: " + ex.Source);
                msg.AppendLine("Message: " + ex.Message);
                msg.AppendLine("Target Site: " + ex.TargetSite);
                msg.AppendLine("Stack Trace: " + ex.StackTrace);

                msg.AppendLine();

                ex = ex.InnerException;
                
                isInnerException = true;
            }
            while (ex != null);

            return msg.ToString();
        }

        public static string FormatExceptionForWeb(Exception ex)
        {
            StringBuilder msg = new StringBuilder();

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
