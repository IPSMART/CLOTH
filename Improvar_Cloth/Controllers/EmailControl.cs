using System;
using Improvar.Models;
using System.IO;
using System.Net.Mail;
using System.Configuration;
namespace Improvar
{
    public partial class EmailControl : System.Web.UI.Page
    {
        string CS = null;
        Connection Cn = new Connection();

        public bool SendHtmlFormattedEmail(string recepientEmail, string subject, string emailTemplate, string[,] emailvar, Attachment emailattachItem, string ccemail = "", string bccemail = "")
        {
            using (MailMessage mailMessage = new MailMessage())
            {
                try
                {
                    string body = string.Empty;
                    if (emailTemplate != "")
                    {
                        using (StreamReader reader = new StreamReader(Server.MapPath("~/Templates/Email/" + emailTemplate)))
                        {
                            body = reader.ReadToEnd();
                        }
                        for (int i = 0; i <= emailvar.GetLength(0) - 1; i++)
                        {
                            body = body.Replace(emailvar[i, 0], emailvar[i, 1]);
                        }
                        body = body.Replace("~", "<br />");
                    }
                    if (recepientEmail == "defaultemail") recepientEmail = ConfigurationManager.AppSettings["emailtosenddefault"];

                    mailMessage.From = new MailAddress(ConfigurationManager.AppSettings["emailuserName"]);
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    if (emailattachItem != null) mailMessage.Attachments.Add(emailattachItem);
                    mailMessage.IsBodyHtml = true;

                    foreach (var address in recepientEmail.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        mailMessage.To.Add(address);
                    }
                    
                    if (ccemail != "")
                    {
                        foreach (var address in ccemail.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            mailMessage.CC.Add(address);
                        }
                    }
                    if (bccemail != "")
                    {
                        foreach (var address in bccemail.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            mailMessage.Bcc.Add(address);
                        }
                    }
                    //mailMessage.To.Add(new MailAddress(recepientEmail));
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = ConfigurationManager.AppSettings["emailhost"];
                    smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["emailenableSsl"]);
                    System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                    NetworkCred.UserName = ConfigurationManager.AppSettings["emailuserName"];
                    NetworkCred.Password = ConfigurationManager.AppSettings["emailpassword"];
                    smtp.UseDefaultCredentials = true;
                    smtp.Credentials = NetworkCred;
                    smtp.Port = int.Parse(ConfigurationManager.AppSettings["emailport"]);
                    smtp.Send(mailMessage);
                    smtp.Dispose();
                    return true;
                }
                catch (Exception)
                {
                    //throw;
                    return false;
                }
            }
        }
    }
}


