using System.Drawing;
using System.Text;
using System.Web.UI.WebControls;
using SyncObjX.Management;
using SyncObjX.SyncObjects;
using SyncObjX.Web;

namespace SyncObjX.Logging.Mail
{
    public static class MailFormatter
    {
        public static string GetFormattedJobInfoAsHtml(Integration integration, JobInstance jobInstance, JobStepInstance jobStepInstance)
        {
            StringBuilder jobInfo = new StringBuilder();

            jobInfo.Append(HtmlBuilder.WrapInBoldTags("Integration: "));
            if (integration == null)
                jobInfo.Append("N/A");
            else
                jobInfo.Append(string.Format("{0} ({1})", integration.Name, integration.Id));
            jobInfo.Append(HtmlBuilder.GetLineBreak());

            if (jobInstance == null)
            {
                jobInfo.Append(HtmlBuilder.WrapInBoldTags("Job:") + " N/A");
                jobInfo.Append(HtmlBuilder.GetLineBreak());
                jobInfo.Append(HtmlBuilder.WrapInBoldTags("Queue Request:") + " N/A");
                jobInfo.Append(HtmlBuilder.GetLineBreak());
                jobInfo.Append(HtmlBuilder.WrapInBoldTags("Job Instance:") + " N/A");
                jobInfo.Append(HtmlBuilder.GetLineBreak());
            }
            else
            {
                jobInfo.Append(HtmlBuilder.WrapInBoldTags("Job: "));
                jobInfo.Append(string.Format("{0} ({1})", jobInstance.Job.Name, jobInstance.Job.Id));
                jobInfo.Append(HtmlBuilder.GetLineBreak());
                jobInfo.Append(HtmlBuilder.WrapInBoldTags("Queue Request: "));
                jobInfo.Append(string.Format("{0} ({1})", jobInstance.QueueRequest.InvocationSourceType, jobInstance.QueueRequest.Id));
                jobInfo.Append(HtmlBuilder.GetLineBreak());
                jobInfo.Append(HtmlBuilder.WrapInBoldTags("Job Instance: "));
                jobInfo.Append(jobInstance.Id);
                jobInfo.Append(HtmlBuilder.GetLineBreak());
            }

            if (jobStepInstance == null)
            {
                jobInfo.Append(HtmlBuilder.WrapInBoldTags("Job Step:") + " N/A");
                jobInfo.Append(HtmlBuilder.GetLineBreak());
                jobInfo.Append(HtmlBuilder.WrapInBoldTags("Job Step Instance:") + " N/A");
                jobInfo.Append(HtmlBuilder.GetLineBreak());
            }
            else
            {
                jobInfo.Append(HtmlBuilder.WrapInBoldTags("Job Step: "));
                jobInfo.Append(string.Format("{0} ({1})", jobStepInstance.JobStep.Name, jobStepInstance.JobStep.Id));
                jobInfo.Append(HtmlBuilder.GetLineBreak());
                jobInfo.Append(HtmlBuilder.WrapInBoldTags("Job Step Instance: "));
                jobInfo.Append(jobStepInstance.Id);
                jobInfo.Append(HtmlBuilder.GetLineBreak());
            }

            jobInfo.Append(HtmlBuilder.GetLineBreak());

            return jobInfo.ToString();
        }

        public static string GetTextAsFormattedDiv(string text)
        {
            Literal lit = new Literal();
            lit.Text = text;

            Panel pnl = new Panel();
            pnl.ForeColor = ColorTranslator.FromHtml("#4d4f53");
            pnl.Font.Names = new string[] { "Tahoma", "Arial" };
            pnl.Font.Size = new FontUnit("13px");
            pnl.Controls.Add(lit);

            return HtmlBuilder.GetServerControlHtml(pnl);
        }
    }
}
