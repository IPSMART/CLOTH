using System;
using System.IO;
using System.Net.Mail;
using System.Configuration;
using System.Net.Mime;
using System.Net;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace Improvar
{
    public partial class EmailControl : System.Web.UI.Page
    {
        string CS = null;
        Connection Cn = new Connection();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public bool SendHtmlFormattedEmail(string recepientEmail, string subject, string emailTemplate, string[,] emailvar, List<Attachment> emailattachItem, string ccemail = "", string bccemail = "", string body = "", string sfemailid = "", string sfemailpw = "", string sfemailhost = "", string sfemailport = "", string sfemailssl = "")
        {
            using (MailMessage mailMessage = new MailMessage())
            {
                try
                {
                    if (recepientEmail == "defaultemail") recepientEmail = ConfigurationManager.AppSettings["emailtosenddefault"];

                    if (sfemailid == "")
                    {
                        if (ConfigurationManager.AppSettings["emailuserName" + CommVar.Compcd(UNQSNO)] != null)
                        {
                            sfemailid = ConfigurationManager.AppSettings["emailuserName" + CommVar.Compcd(UNQSNO)];
                            sfemailpw = ConfigurationManager.AppSettings["emailpassword" + CommVar.Compcd(UNQSNO)];
                            sfemailhost = ConfigurationManager.AppSettings["emailhost" + CommVar.Compcd(UNQSNO)];
                            sfemailport = ConfigurationManager.AppSettings["emailport" + CommVar.Compcd(UNQSNO)];
                            sfemailssl = ConfigurationManager.AppSettings["emailenableSsl" + CommVar.Compcd(UNQSNO)];
                        }
                        else
                        {
                            sfemailid = ConfigurationManager.AppSettings["emailuserName"];
                            sfemailpw = ConfigurationManager.AppSettings["emailpassword"];
                            sfemailhost = ConfigurationManager.AppSettings["emailhost"];
                            sfemailport = ConfigurationManager.AppSettings["emailport"];
                            sfemailssl = ConfigurationManager.AppSettings["emailenableSsl"];
                        }
                    }

                    mailMessage.From = new MailAddress(sfemailid);
                    mailMessage.Subject = subject;
                    if (body == "") mailMessage.AlternateViews.Add(Mail_Body(emailTemplate, emailvar));
                    if (body != "") mailMessage.Body = body;

                    if (emailattachItem != null)
                    {
                        foreach (var ema in emailattachItem)
                        {
                            mailMessage.Attachments.Add(ema);
                        }
                    }

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
                    smtp.Host = sfemailhost;
                    smtp.EnableSsl = Convert.ToBoolean(sfemailssl);
                    System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                    NetworkCred.UserName = sfemailid;
                    NetworkCred.Password = sfemailpw;
                    smtp.UseDefaultCredentials = true;
                    smtp.Credentials = NetworkCred;
                    smtp.Port = int.Parse(sfemailport);
                    smtp.Send(mailMessage);
                    smtp.Dispose();
                    return true;
                }
                catch (Exception ex)
                {
                    Cn.SaveException(ex,"");
                    return false;
                }
            }
        }
        //public bool SendHtmlFormattedEmail(string recepientEmail, string subject, string emailTemplate, string[,] emailvar, List<Attachment> emailattachItem, string ccemail = "", string bccemail = "", string body = "")
        //{
        //    using (MailMessage mailMessage = new MailMessage())
        //    {
        //        try
        //        {
        //            if (recepientEmail == "defaultemail") recepientEmail = ConfigurationManager.AppSettings["emailtosenddefault"];

        //            mailMessage.From = new MailAddress(ConfigurationManager.AppSettings["emailuserName"]);
        //            mailMessage.Subject = subject;
        //            if (body == "") mailMessage.AlternateViews.Add(Mail_Body(emailTemplate, emailvar));
        //            if (body != "") mailMessage.Body = body;
        //            if (emailattachItem != null)
        //            {
        //                foreach (var ema in emailattachItem)
        //                {
        //                    mailMessage.Attachments.Add(ema);
        //                }
        //            }

        //            mailMessage.IsBodyHtml = true;

        //            foreach (var address in recepientEmail.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
        //            {
        //                mailMessage.To.Add(address);
        //            }

        //            if (ccemail != "")
        //            {
        //                foreach (var address in ccemail.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
        //                {
        //                    mailMessage.CC.Add(address);
        //                }
        //            }
        //            if (bccemail != "")
        //            {
        //                foreach (var address in bccemail.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
        //                {
        //                    mailMessage.Bcc.Add(address);
        //                }
        //            }
        //            //mailMessage.To.Add(new MailAddress(recepientEmail));
        //            SmtpClient smtp = new SmtpClient();
        //            smtp.Host = ConfigurationManager.AppSettings["emailhost"];
        //            smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["emailenableSsl"]);
        //            System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
        //            NetworkCred.UserName = ConfigurationManager.AppSettings["emailuserName"];
        //            NetworkCred.Password = ConfigurationManager.AppSettings["emailpassword"];
        //            smtp.UseDefaultCredentials = true;
        //            smtp.Credentials = NetworkCred;
        //            smtp.Port = int.Parse(ConfigurationManager.AppSettings["emailport"]);
        //            smtp.Send(mailMessage);
        //            smtp.Dispose();
        //            return true;
        //        }
        //        catch (Exception ex)
        //        {
        //            //throw;
        //            return false;
        //        }
        //    }
        //}

        public AlternateView Mail_Body(string emailTemplate, string[,] emailvar) 
        {
            string body = string.Empty;
            LinkedResource Img = null;
            LinkedResource Img1 = null;
            if (emailTemplate != "")
            {
                using (StreamReader reader = new StreamReader(Server.MapPath("~/Templates/Email/" + emailTemplate)))
                {
                    body = reader.ReadToEnd();
                }
                for (int i = 0; i <= emailvar.GetLength(0) - 1; i++)
                {
                    if (emailvar[i, 0] == "{complogo}")
                    {
                        try
                        {
                            string path = emailvar[i, 1];
                            Img = new LinkedResource(path, MediaTypeNames.Image.Jpeg);
                            Img.ContentId = "Mycomplogo";
                            body = body.Replace(emailvar[i, 0], "cid:Mycomplogo");
                        }
                        catch (Exception)
                        {

                        }
                    }
                    else if (emailvar[i, 0] == "{compfixlogo}")
                    {
                        try
                        {
                            string path = emailvar[i, 1];
                            Img1 = new LinkedResource(path, MediaTypeNames.Image.Jpeg);
                            Img1.ContentId = "Mycompfixlogo";
                            body = body.Replace(emailvar[i, 0], "cid:Mycompfixlogo");
                        }
                        catch (Exception)
                        {

                        }
                    }
                    else
                    {
                        body = body.Replace(emailvar[i, 0], emailvar[i, 1]);
                    }
                }
                body = body.Replace("~", "<br />");
            }
            AlternateView AV =
            AlternateView.CreateAlternateViewFromString(@body, null, MediaTypeNames.Text.Html);
            if (Img != null)
            {
                AV.LinkedResources.Add(Img);
            }
            if (Img1 != null)
            {
                AV.LinkedResources.Add(Img1);
            }
            return AV;
        }
        public bool SendEmailfromIpsmart(string recepientEmail, string subject, string body, string ccemail)
        {
            using (MailMessage mailMessage = new MailMessage())
            {
                try
                {
                    mailMessage.From = new MailAddress("ipsmart.erp@gmail.com");
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    mailMessage.IsBodyHtml = true;
                    mailMessage.To.Add(recepientEmail);
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.EnableSsl = true;
                    System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                    NetworkCred.UserName = "ipsmart.erp@gmail.com";
                    NetworkCred.Password = "ipsmart@123";
                    smtp.UseDefaultCredentials = true;
                    smtp.Credentials = NetworkCred;
                    smtp.Port = 587;
                    smtp.Send(mailMessage);
                    smtp.Dispose();
                    return true;
                }
                catch (Exception ex)
                {
                    Cn.SaveException(ex, "");
                    return false;
                }
            }
        }      
    }
}


