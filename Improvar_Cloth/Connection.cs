using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Configuration;
using System.Web.Configuration;
using System.Net;
using System.Globalization;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Net.Mail;
using System.Collections;
using System.Drawing;
using Improvar.Models;
using System.Net.Sockets;
using Microsoft.VisualBasic;
using System.Drawing.Imaging;
using QRCoder;
//using ;

namespace Improvar
{
    public class Connection
    {
        public OracleConnection con = new OracleConnection();
        public OracleCommand com = new OracleCommand();
        public OracleDataAdapter da = new OracleDataAdapter();
        public DataSet ds = new DataSet();
        public OracleCommandBuilder cb = new OracleCommandBuilder();
        public OracleDataReader dr;
        private static string Dftl_Schema = null;
        private static int AutoTimeout = 1000;
        private static string Dftl_Machin = null;
        private static string AutoLOgMsg = null;
        public static string staticUser = "aeD+dvIFO8u4hk2fvbWOXd+2qeOeOcOQx1UV4jhfBUY=";
        public static string staticPassword = "k//xFEtFle7TIRUZiZOfQknsqUcm3pnAClXVuZQEF7o=";
        public string SetConnectionString()
        {
            string str1 = null;
            Configuration config = WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);
            string ser_nm = WebConfigurationManager.AppSettings["SERVICE_NAME"];
            string uid = WebConfigurationManager.AppSettings["USER"];
            string pass = WebConfigurationManager.AppSettings["PASSWORD"];
            if (ser_nm.Length == 0 || uid.Length == 0 || pass.Length == 0)
            {
                str1 = "";
            }
            else if (ser_nm.Length != 0 || uid.Length != 0 || pass.Length != 0)
            {
                str1 = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=" + WebConfigurationManager.AppSettings["HOST"] + ")(PORT=" + WebConfigurationManager.AppSettings["PORT"] + ")))(CONNECT_DATA=(SERVER=" + WebConfigurationManager.AppSettings["SERVER"] + ")(SERVICE_NAME=" + WebConfigurationManager.AppSettings["SERVICE_NAME"] + ")));User Id=" + WebConfigurationManager.AppSettings["USER"] + ";Password=" + WebConfigurationManager.AppSettings["PASSWORD"] + ";";
                Getschema = Decrypt(WebConfigurationManager.AppSettings["USER"]);
            }
            return str1;
        }
        public string Getschema
        {
            get { return Dftl_Schema; }
            set { Dftl_Schema = value; }
        }
        public int Get_AutoTimeout
        {
            get { return AutoTimeout; }
            set { AutoTimeout = value; }
        }
        public string GetConnectionString()
        {
            Getschema = Decrypt(WebConfigurationManager.AppSettings["USER"]);
            string Ps = WebConfigurationManager.ConnectionStrings["local"].ConnectionString.ToString();
            string[] cc = Ps.Split(';');
            string user = cc[1].ToString().Substring(8);
            string pass = cc[2].ToString().Substring(9);
            return cc[0].ToString() + "; User ID=" + Decrypt(user) + "; Password=" + Decrypt(pass);
        }
        public string DetermineCompName(string IP)
        {
            IPAddress myIP = IPAddress.Parse(IP);
            IPHostEntry GetIPHost = Dns.GetHostEntry(myIP);
            List<string> compName = GetIPHost.HostName.ToString().Split('.').ToList();
            return compName.First();
        }
        public string GetMachin
        {
            get { return Dftl_Machin; }
            set { Dftl_Machin = value; }
        }
        public string GetstaticUser
        {
            get { return staticUser; }
        }
        public string GetstaticPassword
        {
            get { return staticPassword; }
        }
        public string GetIp()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress address in ipHostInfo.AddressList)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                    return address.ToString();
            }
            return string.Empty;
        }
        public string GetStaticIp()
        {
            string StaticIp = System.Web.HttpContext.Current.Request.UserHostAddress;
            if (CheckValidIP(StaticIp) == false)
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress address in ipHostInfo.AddressList)
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork)
                        return address.ToString();
                }
            }
            return StaticIp;
        }
        public string GetAutoLogMsg
        {
            get { return AutoLOgMsg; }
            set { AutoLOgMsg = value; }
        }
        public Stream ResStream { get; private set; }
        public object RemoteEndpointMessageProperty { get; private set; }
        public string GetDate(string dt)
        {
            string[] st = dt.Split('/');
            string st1;
            string mn;
            int op = DateTime.ParseExact(st[1], "MMM", CultureInfo.CurrentCulture).Month;
            if (op.ToString().Length > 1) { mn = op.ToString(); } else { mn = "0" + op.ToString(); };
            st1 = st[0] + "/" + mn + "/" + st[2];
            DateTime dt1 = Convert.ToDateTime(st1);
            return st1 = dt1.ToString("dd/MM/yyyy");
        }
        public string GetNOB(double nob, double qty)
        {
            double str = 0;
            //str = Math.Truncate(qty * nob) + (qty - Math.Truncate(qty)) * 100;
            if (nob == 1)
            {
                str = nob * qty;
            }
            else
            {
                if (qty > 0 && qty < nob)
                {
                    str = 0.01 * qty;
                }
                else if (qty == nob)
                {
                    str = qty / nob;
                }
                else
                {
                    str = qty / nob;
                    if (str.ToString().Length > 1)
                    {
                        string[] sp = str.ToString().Split('.');
                        if (sp.Length > 1)
                        {
                            string sp1 = "." + sp[1];
                            double sp2 = nob * Convert.ToDouble(sp1);
                            if (sp2.ToString().Split('.').Length > 1)
                            {
                                int uu = Convert.ToInt32(sp2);
                                if (uu.ToString().Length > 1)
                                {
                                    str = Convert.ToDouble(sp[0] + "." + uu);
                                }
                                else
                                {
                                    str = Convert.ToDouble(sp[0] + ".0" + uu);
                                }
                            }
                            else
                            {
                                if (sp2.ToString().Length > 1)
                                {
                                    str = Convert.ToDouble(sp[0] + "." + sp2);
                                }
                                else
                                {
                                    str = Convert.ToDouble(sp[0] + ".0" + sp2);
                                }
                            }
                        }
                        else
                        {
                            str = Convert.ToDouble(sp[0]);
                        }
                    }
                }
            }
            return str.ToString();
        }
        public string FullDate(string date)
        {
            try
            {
                string ld = "";
                System.Collections.Hashtable month = new System.Collections.Hashtable();
                month.Add(1, "January");
                month.Add(2, "February");
                month.Add(3, "March");
                month.Add(4, "April");
                month.Add(5, "May");
                month.Add(6, "June");
                month.Add(7, "July");
                month.Add(8, "August");
                month.Add(9, "September");
                month.Add(10, "October");
                month.Add(11, "November");
                month.Add(12, "December");
                string[] date1 = date.Split('/');
                if (date1.Length <= 1)
                {
                    date1 = date.Split('-');
                }
                int day = Convert.ToInt32(date1[0]);
                int mon = Convert.ToInt32(date1[1]);
                if (day == 1 || day == 21 || day == 31)
                {
                    ld = date1[0].ToString() + "st " + month[mon].ToString() + " " + date1[2].ToString();
                }
                else if (day == 2 || day == 22)
                {
                    ld = date1[0].ToString() + "nd " + month[mon].ToString() + " " + date1[2].ToString();
                }
                else if (day == 3 || day == 23)
                {
                    ld = date1[0].ToString() + "rd " + month[mon].ToString() + " " + date1[2].ToString();
                }
                else
                {
                    ld = date1[0].ToString() + "th " + month[mon].ToString() + " " + date1[2].ToString();
                }
                return ld;
            }
            catch (Exception ex)
            {
                return date;
            }
        }
        public string format_date(string date)
        {
            try
            {
                string dt = "";
                System.Collections.Hashtable month = new System.Collections.Hashtable();
                month.Add(1, "Jan");
                month.Add(2, "Feb");
                month.Add(3, "Mar");
                month.Add(4, "Apr");
                month.Add(5, "May");
                month.Add(6, "Jun");
                month.Add(7, "Jul");
                month.Add(8, "Aug");
                month.Add(9, "Sep");
                month.Add(10, "Oct");
                month.Add(11, "Nov");
                month.Add(12, "Dec");
                string[] ss = date.Split('/');
                if (ss.Length > 1)
                {
                    dt = ss[0] + "/" + month[Convert.ToInt32(ss[1])] + "/" + ss[2];
                }
                else
                {
                    string[] ss1 = date.Split('-');
                    dt = ss1[0] + "/" + month[Convert.ToInt32(ss1[1])] + "/" + ss1[2];
                }
                return dt;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        public string Encrypt(string clearText)
        {
            string EncryptionKey = "775466@@##@@!!jaguar86866454";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }
        public string Decrypt(string cipherText)
        {
            string EncryptionKey = "775466@@##@@!!jaguar86866454";
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
        //public string Encrypt_URL(string clearText)
        //{
        //    string sesscode = Convert.ToString(System.Web.HttpContext.Current.Session["Session_No"]);
        //    string EncryptionKey = "775466@@##@@!!" + sesscode + "86866454";
        //    byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
        //    using (Aes encryptor = Aes.Create())
        //    {
        //        Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
        //        encryptor.Key = pdb.GetBytes(32);
        //        encryptor.IV = pdb.GetBytes(16);
        //        using (MemoryStream ms = new MemoryStream())
        //        {
        //            using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
        //            {
        //                cs.Write(clearBytes, 0, clearBytes.Length);
        //                cs.Close();
        //            }
        //            clearText = Convert.ToBase64String(ms.ToArray());
        //        }
        //    }
        //    return clearText;
        //}
        public string Encrypt_URL(string text)
        {
            try
            {
                return Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(text));
            }
            catch
            {
                return "";
            }
        }
        public string Decrypt_URL(string text)
        {
            try
            {
                return System.Text.ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(text));
            }
            catch
            {
                return "";
            }
        }
        //public string (string cipherText)
        //{
        //    string sesscode = Convert.ToString(System.Web.HttpContext.Current.Session["Session_No"]);
        //    string EncryptionKey = "775466@@##@@!!" + sesscode + "86866454";
        //    byte[] cipherBytes = Convert.FromBase64String(cipherText);
        //    using (Aes encryptor = Aes.Create())
        //    {
        //        Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
        //        encryptor.Key = pdb.GetBytes(32);
        //        encryptor.IV = pdb.GetBytes(16);
        //        using (MemoryStream ms = new MemoryStream())
        //        {
        //            using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
        //            {
        //                cs.Write(cipherBytes, 0, cipherBytes.Length);
        //                cs.Close();
        //            }
        //            cipherText = Encoding.Unicode.GetString(ms.ToArray());
        //        }
        //    }
        //    return cipherText;
        //}
        public bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        public int SetASCII(string value)
        {
            string s = value;
            byte[] bytes = Encoding.ASCII.GetBytes(s);
            int result = BitConverter.ToInt32(bytes, 0);
            return result;
        }
        public string GetASCII(int value)
        {
            int i = value;
            byte[] bytes2 = BitConverter.GetBytes(i);
            string s2 = Encoding.ASCII.GetString(bytes2);
            return s2;
        }
        public string GCS()
        {
            char SP = ((char)181);
            string GS = SP.ToString();
            return GS;
        }
        public string SendMail(string body, string toaddress, string EmailPort, string EmailServer, string EmailUsr, string EmailPass, string EmailDefault, string title, string CC_Address)
        {
            string RUL = "";
            MailMessage mail = new MailMessage();
            SmtpClient client = new SmtpClient();
            client.Port = Convert.ToInt32(EmailPort);
            client.Host = EmailServer;
            client.EnableSsl = true;
            client.Timeout = 100000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(EmailUsr, EmailPass);
            MailAddress from = new MailAddress(EmailUsr, EmailDefault);
            MailAddress to = new MailAddress(toaddress);//toaddress
            MailMessage mm = new MailMessage(from, to);
            mm.Subject = title;
            mm.Body = body;
            mm.IsBodyHtml = true;
            mm.Priority = MailPriority.High;
            mm.BodyEncoding = UTF8Encoding.UTF8;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
            if (CC_Address.Length > 0)
            {
                string[] CCId = CC_Address.Split(',');
                foreach (string CCEmail in CCId)
                {
                    mm.CC.Add(new MailAddress(CCEmail)); //Adding Multiple CC email Id
                }
            }
            try
            {
                client.Send(mm);
                System.Threading.Thread.Sleep(10000);
                RUL = "OK";
            }
            catch (Exception ex)
            {
                RUL = "NO";
            }
            return RUL;
        }
        public string SendMail(byte[] body, string toaddress, string company_name, int Flag, string EmailPort, string EmailServer, string EmailUsr, string EmailPass, string EmailDefault, string title, string head, string filename, string CC_Address)
        {
            string RUL = "";
            MailMessage mail = new MailMessage();
            SmtpClient client = new SmtpClient();
            client.Port = Convert.ToInt32(EmailPort);
            client.Host = EmailServer;
            client.EnableSsl = true;
            client.Timeout = 100000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(EmailUsr, EmailPass);
            MailAddress from = new MailAddress(EmailUsr, EmailDefault);
            MailAddress to = new MailAddress(toaddress);
            MailMessage mm = new MailMessage(from, to);
            mm.Subject = title;
            string ss = "<table style='font-family:Calibri' border='0' cellpadding='3' cellspacing='0' style='width:500px; height:350px'>";
            ss = ss + "<tr style='vertical-align:top; background-color:#004a8f; color:White; font-size:20px; font-family:Calibri; font-weight:bold;width:500px'><td align='center' style='height:20px'>" + head + "</td></tr>";
            ss = ss + " <tr><td style='height:10px'>Dear Sir/Madam,<br /><br /></td></tr>";
            ss = ss + "<tr><td style='height:10px'>Please find the attachment along with this mail<br /><br /></td></tr>";
            ss = ss + " <tr><td style='height:10px'>Thanking You,<br /></td></tr>";
            ss = ss + "<tr><td style='height:10px'>From - " + company_name + "<br /></td></tr>";
            ss = ss + "<tr><td><br /></td></tr>";
            ss = ss + " <tr><td align='center' style='vertical-align:bottom;height:10px; background-color:#e2effa;width:500px'>*This is an auto generated mail*&nbsp; &nbsp;&nbsp; Prepared  By - Ipsmart Team</td></tr></table>";
            mm.Body = ss;
            if (Flag == 1)
            {
                mm.Attachments.Add(new Attachment(new MemoryStream(body), filename, "application/vnd.ms-excel"));
            }
            else
            {
                mm.Attachments.Add(new Attachment(new MemoryStream(body), filename, "application/pdf"));
            }
            mm.IsBodyHtml = true;
            mm.Priority = MailPriority.High;
            mm.BodyEncoding = UTF8Encoding.UTF8;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
            if (CC_Address.Length > 0)
            {
                string[] CCId = CC_Address.Split(',');
                foreach (string CCEmail in CCId)
                {
                    mm.CC.Add(new MailAddress(CCEmail)); //Adding Multiple CC email Id
                }
            }
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                client.Send(mm);
                System.Threading.Thread.Sleep(10000);
                RUL = "OK";
            }
            catch (Exception ex)
            {
                RUL = "NO";
            }
            return RUL;
        }
        public string SendMail_MulAttach(byte[] body, string toaddress, string company_name, int Flag, string EmailPort, string EmailServer, string EmailUsr, string EmailPass, string EmailDefault, string title, string head, string filename, string CC_Address, ArrayList Attachment)
        {
            string RUL = "";
            MailMessage mail = new MailMessage();
            SmtpClient client = new SmtpClient();
            client.Port = Convert.ToInt32(EmailPort);
            client.Host = EmailServer;
            client.EnableSsl = true;
            client.Timeout = 100000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(EmailUsr, EmailPass);
            MailAddress from = new MailAddress(EmailUsr, EmailDefault);
            MailAddress to = new MailAddress(toaddress);
            MailMessage mm = new MailMessage(from, to);
            mm.Subject = title;
            string ss = "<table style='font-family:Calibri' border='0' cellpadding='3' cellspacing='0' style='width:500px; height:350px'>";
            ss = ss + "<tr style='vertical-align:top; background-color:#004a8f; color:White; font-size:20px; font-family:Calibri; font-weight:bold;width:500px'><td align='center' style='height:20px'>" + head + "</td></tr>";
            ss = ss + " <tr><td style='height:10px'>Dear Sir/Madam,<br /><br /></td></tr>";
            ss = ss + "<tr><td style='height:10px'>Please find the attachment along with this mail<br /><br /></td></tr>";
            ss = ss + " <tr><td style='height:10px'>Thanking You,<br /></td></tr>";
            ss = ss + "<tr><td style='height:10px'>From - " + company_name + "<br /></td></tr>";
            ss = ss + "<tr><td><br /></td></tr>";
            ss = ss + " <tr><td align='center' style='vertical-align:bottom;height:10px; background-color:#e2effa;width:500px'>*This is an auto generated mail*&nbsp; &nbsp;&nbsp;    Prepared  By -Improvar Team</td></tr></table>";
            mm.Body = ss;
            if (Flag == 1)
            {
                mm.Attachments.Add(new Attachment(new MemoryStream(body), filename, "application/vnd.ms-excel"));
                for (int i = 0; i <= Attachment.Count - 1; i++)
                {
                    byte[] atta = (byte[])Attachment[i];
                    mm.Attachments.Add(new Attachment(new MemoryStream(atta), filename, "application/vnd.ms-excel"));
                }
            }
            else
            {
                mm.Attachments.Add(new Attachment(new MemoryStream(body), filename, "application/pdf"));
                for (int i = 0; i <= Attachment.Count - 1; i++)
                {
                    byte[] atta = (byte[])Attachment[i];
                    mm.Attachments.Add(new Attachment(new MemoryStream(atta), filename, "application/vnd.ms-excel"));
                }
            }
            mm.IsBodyHtml = true;
            mm.Priority = MailPriority.High;
            mm.BodyEncoding = UTF8Encoding.UTF8;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
            if (CC_Address.Length > 0)
            {
                string[] CCId = CC_Address.Split(',');
                foreach (string CCEmail in CCId)
                {
                    mm.CC.Add(new MailAddress(CCEmail)); //Adding Multiple CC email Id
                }
            }
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                client.Send(mm);
                System.Threading.Thread.Sleep(10000);
                RUL = "OK";
            }
            catch (Exception ex)
            {
                RUL = "NO";
            }
            return RUL;
        }
        public string ImageToBase64(Image image, System.Drawing.Imaging.ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, format);
                byte[] imageBytes = ms.ToArray();

                // Convert byte[] to Base64 String
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }
        public Image Base64ToImage(string base64String)
        {
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0,
              imageBytes.Length);

            // Convert byte[] to Image
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);
            return image;
        }
        public long M_AUTONO(string schema)
        {
            long autonum = 0;
            Improvar.Models.ImprovarDB DB = new Models.ImprovarDB(GetConnectionString(), schema);
            var MAXMAUTONO = DB.M_CNTRL_HDR.Select(p => p.M_AUTONO).DefaultIfEmpty(0).Max();
            if (MAXMAUTONO == 0)
            {
                autonum = 1;
            }
            else
            {
                MAXMAUTONO = MAXMAUTONO + 1;
                autonum = Convert.ToInt32(MAXMAUTONO);
            }
            return autonum;
        }
        public List<DocumentType> DOCTYPE()
        {
            var UNQSNO = getQueryStringUNQSNO();

            Improvar.Models.ImprovarDB DB = new Models.ImprovarDB(GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            Improvar.Models.ImprovarDB DB1 = new Models.ImprovarDB(GetConnectionString(), Getschema);

            string MENUID = System.Web.HttpContext.Current.Session["menuid"].ToString();
            byte MENUINDEX = Convert.ToByte(System.Web.HttpContext.Current.Session["menuindex"]);
            string MODULECODE = System.Web.HttpContext.Current.Session["ModuleCode"].ToString();
            string DOCCD = System.Web.HttpContext.Current.Session["menudoccd"].ToString();

            string[] XYZ = DOCCD.ToString().Split(',');

            List<DocumentType> DOC_TYPE = (from i in DB.M_DOCTYPE where (XYZ.Contains(i.DOCTYPE)) select new DocumentType() { value = i.DOCCD, text = i.DOCNM }).OrderBy(s => s.text).ToList();

            return DOC_TYPE;

        }
        public List<DocumentType> DOCTYPE1(string MENU_DOCCODE, string Mode = "")
        {
            MasterHelp masterHelp = new MasterHelp();
            var UNQSNO = getQueryStringUNQSNO();
            string Scm1 = CommVar.CurSchema(UNQSNO), LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), USR = CommVar.UserID();
            string DOCCD = MENU_DOCCODE.retSqlformat();
            string QUERY = "";
            QUERY = QUERY + "select b.doccd, b.docnm  ";
            QUERY = QUERY + "from " + Scm1 + ".m_usr_acs_doccd a, " + Scm1 + ".m_doctype b," + Scm1 + ".m_cntrl_hdr c ";
            QUERY = QUERY + "where a.user_id='" + USR + "' and a.compcd='" + COM + "' and a.loccd='" + LOC + "' and  ";
            QUERY = QUERY + "a.doccd=b.doccd(+) and b.doctype in(" + DOCCD + ")and b.m_autono=c.m_autono(+) ";
            if (Mode == "A") QUERY = QUERY + " and nvl(c.inactive_tag,'N') = 'N' ";
            QUERY = QUERY + "union ";
            QUERY = QUERY + "select a.doccd, a.docnm  ";
            QUERY = QUERY + "from " + Scm1 + ".m_doctype a," + Scm1 + ".m_cntrl_hdr c  ";
            QUERY = QUERY + "where  a.m_autono=c.m_autono(+) ";
            if (Mode == "A") QUERY = QUERY + " and nvl(c.inactive_tag,'N') = 'N' ";
            QUERY = QUERY + "and  a.doctype in(" + DOCCD + ") and a.doctype not in ( ";
            QUERY = QUERY + "select b.doctype  ";
            QUERY = QUERY + "from " + Scm1 + ".m_usr_acs_doccd a, " + Scm1 + ".m_doctype b ";
            QUERY = QUERY + "where a.user_id='" + USR + "' and a.compcd='" + COM + "' and a.loccd='" + LOC + "' and ";
            QUERY = QUERY + "b.doctype in(" + DOCCD + ")) ";
            DataTable rsTmp = masterHelp.SQLquery(QUERY);
            List<DocumentType> DOC_TYPE = (from DataRow dr in rsTmp.Rows
                                           select new DocumentType
                                           {
                                               value = dr["doccd"].ToString(),
                                               text = dr["docnm"].ToString(),
                                           }).ToList();
            return DOC_TYPE;
        }
        //public List<DocumentType> DOCTYPE1(string MENU_DOCCODE)
        //{
        //    MasterHelp masterHelp = new MasterHelp();
        //    var UNQSNO = getQueryStringUNQSNO();
        //    string Scm1 = CommVar.CurSchema(UNQSNO), LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), USR = CommVar.UserID();
        //    string DOCCD = MENU_DOCCODE.retSqlformat();
        //    string QUERY = "";
        //    QUERY = QUERY + "select b.doccd, b.docnm  ";
        //    QUERY = QUERY + "from " + Scm1 + ".m_usr_acs_doccd a, " + Scm1 + ".m_doctype b ";
        //    QUERY = QUERY + "where a.user_id='" + USR + "' and a.compcd='" + COM + "' and a.loccd='" + LOC + "' and  ";
        //    QUERY = QUERY + "a.doccd=b.doccd(+) and b.doctype in(" + DOCCD + ") ";
        //    QUERY = QUERY + "union ";
        //    QUERY = QUERY + "select a.doccd, a.docnm  ";
        //    QUERY = QUERY + "from " + Scm1 + ".m_doctype a ";
        //    QUERY = QUERY + "where a.doctype in(" + DOCCD + ") and a.doctype not in ( ";
        //    QUERY = QUERY + "select b.doctype  ";
        //    QUERY = QUERY + "from " + Scm1 + ".m_usr_acs_doccd a, " + Scm1 + ".m_doctype b ";
        //    QUERY = QUERY + "where a.user_id='" + USR + "' and a.compcd='" + COM + "' and a.loccd='" + LOC + "' and ";
        //    QUERY = QUERY + "b.doctype in(" + DOCCD + ")) ";
        //    DataTable rsTmp = masterHelp.SQLquery(QUERY);
        //    List<DocumentType> DOC_TYPE = (from DataRow dr in rsTmp.Rows
        //                                   select new DocumentType
        //                                   {
        //                                       value = dr["doccd"].ToString(),
        //                                       text = dr["docnm"].ToString(),
        //                                   }).ToList();
        //    return DOC_TYPE;
        //}
        public Models.M_CNTRL_HDR M_CONTROL_HDR(bool Tag, string Table_Name, long Auto_Number, string ACTION, string DATABASE, string auditrem)
        {
            var UNQSNO = getQueryStringUNQSNO();
            Improvar.Models.ImprovarDB DB = new Models.ImprovarDB(GetConnectionString(), DATABASE);
            Models.M_CNTRL_HDR MCH = new Models.M_CNTRL_HDR();

            var M_CNTRL_HDR_TEMP = (from i in DB.M_CNTRL_HDR
                                    where (i.M_AUTONO == Auto_Number)
                                    select new
                                    {
                                        PKGLEGACYCD = i.PKGLEGACYCD,
                                        EMD_NO = i.EMD_NO,
                                        USR_ID = i.USR_ID,
                                        USR_ENTDT = i.USR_ENTDT,
                                        USR_LIP = i.USR_LIP,
                                        USR_SIP = i.USR_SIP,
                                        USR_OS = i.USR_OS,
                                        USR_MNM = i.USR_MNM,
                                        LM_USR_ID = i.LM_USR_ID,
                                        LM_USR_ENTDT = i.LM_USR_ENTDT,
                                        LM_USR_LIP = i.LM_USR_LIP,
                                        LM_USR_SIP = i.LM_USR_SIP,
                                        LM_USR_OS = i.LM_USR_OS,
                                        LM_USR_MNM = i.LM_USR_MNM,
                                        INACTIVE_TAG = i.INACTIVE_TAG,
                                        NONOP_DT = i.NONOP_DT,
                                        NONOP_REM = i.NONOP_REM,
                                        LM_REM = i.LM_REM,
                                        DEL_REM = i.DEL_REM,
                                    }).ToList();

            MCH.M_AUTONO = Auto_Number;
            MCH.M_TBLNM = Table_Name;
            MCH.USR_ENTDT = System.DateTime.Now;
            MCH.CLCD = CommVar.ClientCode(UNQSNO);

            if (ACTION == "A" || ACTION == "E")
            {
                if (Tag == true)
                {
                    MCH.INACTIVE_TAG = "Y";
                }
                else
                {
                    MCH.INACTIVE_TAG = "N";
                }
                if (MCH.INACTIVE_TAG == "Y")
                {

                    MCH.NONOP_DT = System.DateTime.Now;
                }
            }

            if (ACTION == "E" || ACTION == "V")
            {
                MCH.PKGLEGACYCD = M_CNTRL_HDR_TEMP[0].PKGLEGACYCD;
                MCH.USR_ID = M_CNTRL_HDR_TEMP[0].USR_ID;
                MCH.USR_ENTDT = M_CNTRL_HDR_TEMP[0].USR_ENTDT;
                MCH.USR_LIP = M_CNTRL_HDR_TEMP[0].USR_LIP;
                MCH.USR_SIP = M_CNTRL_HDR_TEMP[0].USR_SIP;
                MCH.USR_OS = M_CNTRL_HDR_TEMP[0].USR_OS;
                MCH.USR_MNM = M_CNTRL_HDR_TEMP[0].USR_MNM;
            }

            if (ACTION == "A")
            {
                MCH.USR_ID = System.Web.HttpContext.Current.Session["UR_ID"].ToString();
                MCH.USR_ENTDT = System.DateTime.Now;
                MCH.USR_LIP = GetIp();
                MCH.USR_SIP = GetStaticIp();
                MCH.USR_OS = null;
                MCH.USR_MNM = DetermineCompName(GetIp());
                MCH.DTAG = "";
                MCH.EMD_NO = 0;
            }
            else if (ACTION == "V")
            {
                MCH.NONOP_DT = M_CNTRL_HDR_TEMP[0].NONOP_DT;
                MCH.INACTIVE_TAG = M_CNTRL_HDR_TEMP[0].INACTIVE_TAG;
                MCH.NONOP_REM = M_CNTRL_HDR_TEMP[0].NONOP_REM;

                MCH.LM_USR_ID = M_CNTRL_HDR_TEMP[0].LM_USR_ID;
                MCH.LM_USR_ENTDT = M_CNTRL_HDR_TEMP[0].LM_USR_ENTDT;
                MCH.LM_USR_LIP = M_CNTRL_HDR_TEMP[0].USR_LIP;
                MCH.LM_USR_SIP = M_CNTRL_HDR_TEMP[0].LM_USR_SIP;
                MCH.LM_USR_OS = M_CNTRL_HDR_TEMP[0].LM_USR_OS;
                MCH.LM_USR_MNM = M_CNTRL_HDR_TEMP[0].LM_USR_MNM;

                MCH.DTAG = "D";
                MCH.EMD_NO = M_CNTRL_HDR_TEMP[0].EMD_NO;
                MCH.DEL_USR_ID = System.Web.HttpContext.Current.Session["UR_ID"].ToString();
                MCH.DEL_USR_ENTDT = System.DateTime.Now;
                MCH.DEL_USR_LIP = GetIp();
                MCH.DEL_USR_SIP = GetStaticIp();
                MCH.DEL_USR_OS = null;
                MCH.DEL_USR_MNM = DetermineCompName(GetIp());  //GetMachin;
            }
            else if (ACTION == "E")
            {
                MCH.DTAG = "E";
                MCH.LM_USR_ID = System.Web.HttpContext.Current.Session["UR_ID"].ToString();
                MCH.LM_USR_ENTDT = System.DateTime.Now;
                MCH.LM_USR_LIP = GetIp();
                MCH.LM_USR_SIP = GetStaticIp();
                MCH.LM_USR_OS = null;
                MCH.LM_USR_MNM = DetermineCompName(GetIp());
                var MAXEMDNO = (from p in DB.M_CNTRL_HDR where p.M_AUTONO == Auto_Number select p.EMD_NO).Max();
                if (MAXEMDNO == null)
                {
                    MCH.EMD_NO = 0;
                }
                else
                {
                    MCH.EMD_NO = Convert.ToInt32(MAXEMDNO + 1);
                }

            }
            if (ACTION == "E" || ACTION == "A")
            {
                MCH.LM_REM = auditrem;
            }
            else if (ACTION == "V")
            {
                MCH.DEL_REM = auditrem;
                MCH.LM_REM = M_CNTRL_HDR_TEMP[0].LM_REM;
            }
            return MCH;
        }
        public Models.T_CNTRL_HDR T_CONTROL_HDR(string DCODE, DateTime? DDATE, string DNO, string Auto_Number, string MCODE, string DPATTERN, string ACTION, string DATABASE, string glcd, string slcd, double docamt, string calauto, string auditrem)
        {
            var UNQSNO = getQueryStringUNQSNO();
            Improvar.Models.ImprovarDB DB = new Models.ImprovarDB(GetConnectionString(), DATABASE);
            Models.T_CNTRL_HDR TCH = new Models.T_CNTRL_HDR();

            var T_CNTRL_HDR_TEMP = (from i in DB.T_CNTRL_HDR
                                    where (i.AUTONO == Auto_Number)
                                    select new
                                    {
                                        EMD_NO = i.EMD_NO,
                                        DOCCD = i.DOCCD,
                                        DOCONLYNO = i.DOCONLYNO,
                                        DOCNO = i.DOCNO,
                                        VCHRNO = i.VCHRNO,
                                        VCHRSUFFIX = i.VCHRSUFFIX,
                                        MNTHCD = i.MNTHCD,
                                        DOCDT = i.DOCDT,
                                        GLCD = i.GLCD,
                                        SLCD = i.SLCD,
                                        DOCAMT = i.DOCAMT,
                                        CALAUTO = i.CALAUTO,

                                        USR_ID = i.USR_ID,
                                        USR_ENTDT = i.USR_ENTDT,
                                        USR_LIP = i.USR_LIP,
                                        USR_SIP = i.USR_SIP,
                                        USR_OS = i.USR_OS,
                                        USR_MNM = i.USR_MNM,
                                        LM_USR_ID = i.LM_USR_ID,
                                        LM_USR_ENTDT = i.LM_USR_ENTDT,
                                        LM_USR_LIP = i.LM_USR_LIP,
                                        LM_USR_SIP = i.LM_USR_SIP,
                                        LM_USR_OS = i.LM_USR_OS,
                                        LM_USR_MNM = i.LM_USR_MNM,
                                        LM_REM = i.LM_REM,
                                        DEL_REM = i.DEL_REM,
                                    }).ToList();

            TCH.AUTONO = Auto_Number;
            //TCH.DOCNO = DPATTERN;
            string[] fin = CommVar.FinPeriod(UNQSNO).Split('-');
            string YEARCD = fin[0].Substring(6).Trim();
            TCH.YR_CD = YEARCD;
            TCH.COMPCD = CommVar.Compcd(UNQSNO);
            TCH.LOCCD = CommVar.Loccd(UNQSNO);
            TCH.MODCD = "S";
            TCH.CLCD = CommVar.ClientCode(UNQSNO);
            if (ACTION == "A" || ACTION == "E")
            {
                TCH.DOCCD = DCODE;
                TCH.VCHRNO = Convert.ToInt32(DNO);
                TCH.DOCONLYNO = DNO;
                if (T_CNTRL_HDR_TEMP.Count > 0)
                {
                    TCH.DOCNO = T_CNTRL_HDR_TEMP[0].DOCNO;
                    TCH.MNTHCD = T_CNTRL_HDR_TEMP[0].MNTHCD;
                }
                else
                {
                    TCH.DOCNO = DPATTERN;
                    TCH.MNTHCD = MCODE;
                }
                TCH.DOCDT = DDATE;
                TCH.GLCD = glcd;
                TCH.SLCD = slcd;
                TCH.DOCAMT = docamt;
                TCH.CALAUTO = calauto;
            }
            if (ACTION == "E" || ACTION == "V")
            {
                TCH.USR_ID = T_CNTRL_HDR_TEMP[0].USR_ID;
                TCH.USR_ENTDT = T_CNTRL_HDR_TEMP[0].USR_ENTDT;
                TCH.USR_LIP = T_CNTRL_HDR_TEMP[0].USR_LIP;
                TCH.USR_SIP = T_CNTRL_HDR_TEMP[0].USR_SIP;
                TCH.USR_OS = T_CNTRL_HDR_TEMP[0].USR_OS;
                TCH.USR_MNM = T_CNTRL_HDR_TEMP[0].USR_MNM;
            }
            if (ACTION == "A")
            {
                TCH.USR_ID = System.Web.HttpContext.Current.Session["UR_ID"].ToString();
                TCH.USR_ENTDT = System.DateTime.Now;
                TCH.USR_LIP = GetIp();
                TCH.USR_SIP = GetStaticIp();
                TCH.USR_OS = null;
                TCH.USR_MNM = DetermineCompName(GetIp());  //GetMachin;
                TCH.DTAG = "";
                TCH.EMD_NO = 0;
            }
            else if (ACTION == "V")
            {
                TCH.DOCCD = T_CNTRL_HDR_TEMP[0].DOCCD;
                TCH.VCHRNO = T_CNTRL_HDR_TEMP[0].VCHRNO;
                TCH.DOCONLYNO = T_CNTRL_HDR_TEMP[0].DOCONLYNO;
                TCH.DOCNO = T_CNTRL_HDR_TEMP[0].DOCNO;
                TCH.MNTHCD = T_CNTRL_HDR_TEMP[0].MNTHCD;
                TCH.DOCDT = T_CNTRL_HDR_TEMP[0].DOCDT;
                TCH.GLCD = T_CNTRL_HDR_TEMP[0].GLCD;
                TCH.SLCD = T_CNTRL_HDR_TEMP[0].SLCD;
                TCH.DOCAMT = T_CNTRL_HDR_TEMP[0].DOCAMT;
                TCH.CALAUTO = T_CNTRL_HDR_TEMP[0].CALAUTO;
                TCH.EMD_NO = T_CNTRL_HDR_TEMP[0].EMD_NO;

                TCH.LM_USR_ID = T_CNTRL_HDR_TEMP[0].LM_USR_ID;
                TCH.LM_USR_ENTDT = T_CNTRL_HDR_TEMP[0].LM_USR_ENTDT;
                TCH.LM_USR_LIP = T_CNTRL_HDR_TEMP[0].USR_LIP;
                TCH.LM_USR_SIP = T_CNTRL_HDR_TEMP[0].LM_USR_SIP;
                TCH.LM_USR_OS = T_CNTRL_HDR_TEMP[0].LM_USR_OS;
                TCH.LM_USR_MNM = T_CNTRL_HDR_TEMP[0].LM_USR_MNM;

                TCH.DTAG = "D";
                TCH.DEL_USR_ID = System.Web.HttpContext.Current.Session["UR_ID"].ToString();
                TCH.DEL_USR_ENTDT = System.DateTime.Now;
                TCH.DEL_USR_LIP = GetIp();
                TCH.DEL_USR_SIP = GetStaticIp();
                TCH.DEL_USR_OS = null;
                TCH.DEL_USR_MNM = DetermineCompName(GetIp());  //GetMachin;
            }
            else if (ACTION == "E")
            {
                TCH.DTAG = "E";
                TCH.LM_USR_ID = System.Web.HttpContext.Current.Session["UR_ID"].ToString();
                TCH.LM_USR_ENTDT = System.DateTime.Now;
                TCH.LM_USR_LIP = GetIp();
                TCH.LM_USR_SIP = GetStaticIp();
                TCH.LM_USR_OS = null;
                TCH.LM_USR_MNM = DetermineCompName(GetIp());  //GetMachin;
                var MAXEMDNO = (from p in DB.T_CNTRL_HDR where p.AUTONO == Auto_Number select p.EMD_NO).Max();
                if (MAXEMDNO == null)
                {
                    TCH.EMD_NO = 0;
                }
                else
                {
                    TCH.EMD_NO = Convert.ToInt16(MAXEMDNO + 1);
                }
            }
            if (ACTION == "E" || ACTION == "A")
            {
                TCH.LM_REM = auditrem;
            }
            else if (ACTION == "V")
            {
                TCH.DEL_REM = auditrem;
                TCH.LM_REM = T_CNTRL_HDR_TEMP[0].LM_REM;
            }
            return TCH;
        }
        public List<Models.UploadDOC> GetUploadImage(string schema, long AutoNO)
        {
            string SCHEMA = Getschema;
            Improvar.Models.ImprovarDB DB1 = new Models.ImprovarDB(GetConnectionString(), SCHEMA);
            var doctP = (from i in DB1.MS_DOCCTG
                         select new DocumentType()
                         {
                             value = i.DOC_CTG,
                             text = i.DOC_CTG
                         }).OrderBy(s => s.text).ToList();
            Improvar.Models.ImprovarDB DB = new Models.ImprovarDB(GetConnectionString(), schema);
            List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
            var doc = (from s in DB.M_CNTRL_HDR_DOC where s.M_AUTONO == AutoNO select s).ToList();
            foreach (var i in doc)
            {
                UploadDOC UPL = new UploadDOC();
                UPL.DocumentType = doctP;
                UPL.docID = i.DOC_CTG;
                UPL.DOC_FILE_NAME = i.DOC_FLNAME;
                UPL.DOC_DESC = i.DOC_DESC;
                var image = (from h in DB.M_CNTRL_HDR_DOC_DTL
                             where h.M_AUTONO == i.M_AUTONO && h.SLNO == i.SLNO
                             select h).OrderBy(d => d.RSLNO).ToList();

                var hh = image.GroupBy(x => x.M_AUTONO).Select(x => new
                {
                    namefl = string.Join("", x.Select(n => n.DOC_STRING))
                });
                foreach (var ii in hh)
                {
                    UPL.DOC_FILE = ii.namefl;
                }
                UploadDOC1.Add(UPL);
            }

            return UploadDOC1;
        }
        public Tuple<List<M_CNTRL_HDR_DOC>, List<M_CNTRL_HDR_DOC_DTL>> SaveUploadImage(string table_name, List<UploadDOC> UploadDOC, long autono, int EMD)
        {
            var UNQSNO = getQueryStringUNQSNO();
            List<M_CNTRL_HDR_DOC> doc = new List<M_CNTRL_HDR_DOC>();
            List<M_CNTRL_HDR_DOC_DTL> doc1 = new List<M_CNTRL_HDR_DOC_DTL>();
            int sl = 0;
            foreach (var ss in UploadDOC)
            {
                if (ss.DOC_FILE != null)
                {

                    sl += 1;
                    M_CNTRL_HDR_DOC mdoc = new M_CNTRL_HDR_DOC();
                    mdoc.DOC_CTG = ss.docID;
                    mdoc.DOC_DESC = ss.DOC_DESC;
                    mdoc.M_AUTONO = autono;
                    mdoc.CLCD = CommVar.ClientCode(UNQSNO);
                    mdoc.EMD_NO = EMD;
                    mdoc.M_TBLNM = table_name;
                    mdoc.DOC_FLNAME = ss.DOC_FILE_NAME;
                    mdoc.DOC_EXTN = "NA";

                    mdoc.SLNO = Convert.ToByte(sl);
                    doc.Add(mdoc);
                    if (ss.DOC_FILE.Length <= 2000)
                    {
                        M_CNTRL_HDR_DOC_DTL mdoct = new M_CNTRL_HDR_DOC_DTL();
                        mdoct.M_AUTONO = autono;
                        mdoct.CLCD = CommVar.ClientCode(UNQSNO);
                        mdoct.EMD_NO = EMD;
                        mdoct.SLNO = Convert.ToByte(sl);
                        mdoct.RSLNO = 1;
                        mdoct.DOC_STRING = ss.DOC_FILE;
                        doc1.Add(mdoct);
                    }
                    else
                    {
                        long length = ss.DOC_FILE.Length;
                        long count = length / 2000;
                        int index = 0;
                        for (int i = 0; i <= count - 1; i++)
                        {
                            M_CNTRL_HDR_DOC_DTL mdoct = new M_CNTRL_HDR_DOC_DTL();
                            mdoct.M_AUTONO = autono;
                            mdoct.CLCD = CommVar.ClientCode(UNQSNO);
                            mdoct.EMD_NO = EMD;
                            mdoct.SLNO = Convert.ToInt16(sl);
                            mdoct.DOC_STRING = ss.DOC_FILE.Substring(index, 2000);
                            mdoct.RSLNO = Convert.ToInt16(i + 1);
                            index += 2000;
                            doc1.Add(mdoct);
                        }
                        if (index < ss.DOC_FILE.Length)
                        {
                            int rest = ss.DOC_FILE.Length - index;
                            M_CNTRL_HDR_DOC_DTL mdoct = new M_CNTRL_HDR_DOC_DTL();
                            mdoct.M_AUTONO = autono;
                            mdoct.CLCD = CommVar.ClientCode(UNQSNO);
                            mdoct.EMD_NO = EMD;
                            mdoct.SLNO = Convert.ToInt16(sl);
                            mdoct.DOC_STRING = ss.DOC_FILE.Substring(index, rest);
                            mdoct.RSLNO = Convert.ToInt16(count + 1);
                            index += rest;
                            doc1.Add(mdoct);
                        }
                    }
                }
            }
            var result = Tuple.Create(doc, doc1);
            return result;
        }
        public Models.T_CNTRL_HDR_REM GetTransactionReamrks(string schema, string AutoNO)
        {
            Improvar.Models.ImprovarDB DB = new Models.ImprovarDB(GetConnectionString(), schema);

            T_CNTRL_HDR_REM TCHREM = new T_CNTRL_HDR_REM();

            var REM = (from h in DB.T_CNTRL_HDR_REM where h.AUTONO == AutoNO select h).OrderBy(d => d.SLNO).ToList();

            var RE = REM.GroupBy(x => x.AUTONO).Select(x => new
            {
                TRANS_REM = string.Join("", x.Select(n => n.DOCREM))
            });
            foreach (var i in RE)
            {
                TCHREM.DOCREM = i.TRANS_REM.ToString();
            }
            return TCHREM;
        }
        public List<Models.UploadDOC> GetUploadImageTransaction(string schema, string AutoNO)
        {
            if (AutoNO == null) AutoNO = "";
            if (AutoNO != "") if (AutoNO.IndexOf("'") < 0) AutoNO = "'" + AutoNO + "'";
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            MasterHelp MasterHelp = new MasterHelp();
            string SCHEMA = Getschema;
            Improvar.Models.ImprovarDB DB1 = new Models.ImprovarDB(GetConnectionString(), SCHEMA);
            var doctP = (from i in DB1.MS_DOCCTG
                         select new DocumentType()
                         {
                             value = i.DOC_CTG,
                             text = i.DOC_CTG
                         }).OrderBy(s => s.text).ToList();
            string sql = "select a.autono, a.doc_ctg, a.doc_desc, a.doc_extn, a.doc_flname, a.slno, b.rslno, b.doc_string ";
            sql += "from " + schema + ".t_cntrl_hdr_doc a, " + schema + ".t_cntrl_hdr_doc_dtl b ";
            sql += "where a.autono = b.autono(+) and a.slno = b.slno(+) ";
            sql += "and a.autono in ( " + AutoNO + " )";
            sql += "order by autono, slno, rslno ";
            DataTable maintbl = MasterHelp.SQLquery(sql);
            DataView dv = new DataView(maintbl);
            string[] cloum = new string[] { "slno", "doc_ctg", "doc_flname", "doc_desc" };
            DataTable slnotbl = dv.ToTable(true, cloum);
            List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
            foreach (DataRow dr in slnotbl.Rows)
            {
                Int16 slno = Convert.ToInt16(dr["slno"].retStr());
                UploadDOC UPL = new UploadDOC();
                var doc_string = string.Join("", maintbl.AsEnumerable()
                .Where(r => ((Int16)r["slno"]) == slno)
                                     .Select(x => x["doc_string"].ToString())
                                     .ToArray());
                UPL.DocumentType = doctP;
                UPL.DOC_FILE = doc_string;
                UPL.docID = dr["doc_ctg"].ToString();
                UPL.DOC_FILE_NAME = dr["DOC_FLNAME"].ToString();
                UPL.DOC_DESC = dr["DOC_DESC"].ToString();
                UploadDOC1.Add(UPL);
            }
            return UploadDOC1;
        }
        public Tuple<List<T_CNTRL_HDR_REM>> SAVETRANSACTIONREMARKS(T_CNTRL_HDR_REM TCNTRLHDRREM, string AUTONO, string CLCD, int EMD)
        {

            List<T_CNTRL_HDR_REM> REM = new List<T_CNTRL_HDR_REM>();

            if (TCNTRLHDRREM.DOCREM.Length <= 1000)
            {
                T_CNTRL_HDR_REM REMARKS = new T_CNTRL_HDR_REM();
                REMARKS.AUTONO = AUTONO;
                REMARKS.CLCD = CLCD;
                REMARKS.EMD_NO = EMD;
                REMARKS.SLNO = 1;
                REMARKS.DOCREM = TCNTRLHDRREM.DOCREM;
                REM.Add(REMARKS);
            }
            else
            {
                long length = TCNTRLHDRREM.DOCREM.Length;
                long count = length / 1000;
                int index = 0;
                for (int i = 0; i <= count - 1; i++)
                {
                    T_CNTRL_HDR_REM REMARKS = new T_CNTRL_HDR_REM();
                    REMARKS.AUTONO = AUTONO;
                    REMARKS.CLCD = CLCD;
                    REMARKS.EMD_NO = EMD;
                    REMARKS.SLNO = Convert.ToInt16(i + 1);
                    REMARKS.DOCREM = TCNTRLHDRREM.DOCREM.Substring(index, 1000);
                    index += 1000;
                    REM.Add(REMARKS);
                }
                if (index < TCNTRLHDRREM.DOCREM.Length)
                {
                    int rest = TCNTRLHDRREM.DOCREM.Length - index;
                    T_CNTRL_HDR_REM REMARKS = new T_CNTRL_HDR_REM();
                    REMARKS.AUTONO = AUTONO;
                    REMARKS.CLCD = CLCD;
                    REMARKS.EMD_NO = EMD;
                    REMARKS.SLNO = Convert.ToInt16(count + 1);
                    REMARKS.DOCREM = TCNTRLHDRREM.DOCREM.Substring(index, rest);
                    index += rest;
                    REM.Add(REMARKS);
                }
            }
            var result = Tuple.Create(REM);
            return result;
        }
        public Tuple<List<T_CNTRL_DOC_PASS>> T_CONTROL_DOC_PASS(string DCODE, double Transaction_Amount, int EMD_NUMBER, string Auto_Number, string DATABASE, string authrem = "", byte level = 0)
        {
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            Improvar.Models.ImprovarDB DB = new Models.ImprovarDB(GetConnectionString(), DATABASE);
            List<T_CNTRL_DOC_PASS> TCDP = new List<T_CNTRL_DOC_PASS>();
            byte MAXSLNO = 0;
            var MDOCAUTH_DATA = (from i in DB.M_DOC_AUTH
                                 where i.DOCCD == DCODE && Transaction_Amount >= i.AMT_FR && Transaction_Amount <= i.AMT_TO
                                 select new { PASS_LEVEL = i.LVL, AUTHCD = i.AUTHCD }).ToList();
            if (level.retDbl() != 0)
            {
                MDOCAUTH_DATA = MDOCAUTH_DATA.Where(a => a.PASS_LEVEL == level).ToList();
                MAXSLNO = DB.T_CNTRL_DOC_PASS.Where(a => a.AUTONO == Auto_Number).Select(a => a.SLNO).DefaultIfEmpty().Max();
            }

            for (int x = 0; x <= MDOCAUTH_DATA.Count() - 1; x++)
            {
                MAXSLNO++;
                Models.T_CNTRL_DOC_PASS TCDP1 = new Models.T_CNTRL_DOC_PASS();
                TCDP1.AUTONO = Auto_Number;
                TCDP1.SLNO = MAXSLNO;// Convert.ToByte(x + 1);
                TCDP1.EMD_NO = EMD_NUMBER;
                TCDP1.CLCD = CommVar.ClientCode(UNQSNO);
                TCDP1.PASS_LEVEL = MDOCAUTH_DATA[x].PASS_LEVEL;
                TCDP1.AUTHCD = MDOCAUTH_DATA[x].AUTHCD;
                TCDP1.AUTHREM = authrem;
                TCDP.Add(TCDP1);
            }
            var result = Tuple.Create(TCDP);
            return result;
        }
        public Tuple<List<T_CNTRL_HDR_DOC>, List<T_CNTRL_HDR_DOC_DTL>> SaveUploadImageTransaction(List<UploadDOC> UploadDOC, string autono, int EMD, int slno = 0)
        {
            var UNQSNO = getQueryStringUNQSNO();
            List<T_CNTRL_HDR_DOC> doc = new List<T_CNTRL_HDR_DOC>();
            List<T_CNTRL_HDR_DOC_DTL> doc1 = new List<T_CNTRL_HDR_DOC_DTL>();
            int sl = slno;
            foreach (var ss in UploadDOC)
            {
                if (ss.DOC_FILE != null)
                {
                    sl += 1;
                    T_CNTRL_HDR_DOC mdoc = new T_CNTRL_HDR_DOC();
                    mdoc.DOC_CTG = ss.docID;
                    mdoc.DOC_DESC = ss.DOC_DESC;
                    mdoc.AUTONO = autono;
                    mdoc.EMD_NO = EMD;
                    mdoc.CLCD = CommVar.ClientCode(UNQSNO);
                    mdoc.DOC_FLNAME = ss.DOC_FILE_NAME;
                    mdoc.DOC_EXTN = "NA";

                    mdoc.SLNO = Convert.ToByte(sl);
                    doc.Add(mdoc);

                    if (ss.DOC_FILE.Length <= 2000)
                    {
                        T_CNTRL_HDR_DOC_DTL mdoct = new T_CNTRL_HDR_DOC_DTL();
                        mdoct.AUTONO = autono;
                        mdoct.EMD_NO = EMD;
                        mdoct.CLCD = CommVar.ClientCode(UNQSNO);
                        mdoct.SLNO = Convert.ToByte(sl);
                        mdoct.RSLNO = 1;
                        mdoct.DOC_STRING = ss.DOC_FILE;
                        doc1.Add(mdoct);
                    }
                    else
                    {
                        long length = ss.DOC_FILE.Length;
                        long count = length / 2000;
                        int index = 0;
                        for (int i = 0; i <= count - 1; i++)
                        {
                            T_CNTRL_HDR_DOC_DTL mdoct = new T_CNTRL_HDR_DOC_DTL();
                            mdoct.AUTONO = autono;
                            mdoct.EMD_NO = EMD;
                            mdoct.CLCD = CommVar.ClientCode(UNQSNO);
                            mdoct.SLNO = Convert.ToInt16(sl);
                            mdoct.DOC_STRING = ss.DOC_FILE.Substring(index, 2000);
                            mdoct.RSLNO = Convert.ToInt16(i + 1);
                            index += 2000;
                            doc1.Add(mdoct);
                        }
                        if (index < ss.DOC_FILE.Length)
                        {
                            int rest = ss.DOC_FILE.Length - index;
                            T_CNTRL_HDR_DOC_DTL mdoct = new T_CNTRL_HDR_DOC_DTL();
                            mdoct.AUTONO = autono;
                            mdoct.EMD_NO = EMD;
                            mdoct.CLCD = CommVar.ClientCode(UNQSNO);
                            mdoct.SLNO = Convert.ToInt16(sl);
                            mdoct.DOC_STRING = ss.DOC_FILE.Substring(index, rest);
                            mdoct.RSLNO = Convert.ToInt16(count + 1);
                            index += rest;
                            doc1.Add(mdoct);
                        }
                    }

                }
            }
            var result = Tuple.Create(doc, doc1);
            return result;
        }
        public string LookUpVal(string OraCommand, Connection Cn1)
        {

            DataTable Extra1 = new DataTable();
            Cn1.com = new OracleCommand(OraCommand, Cn1.con);
            Cn1.da.SelectCommand = Cn1.com;
            Cn1.da.Fill(Extra1);
            string LookUpVal1 = "";
            if (Extra1.Rows.Count != 0)
            {
                string new2 = Extra1.Columns[0].ColumnName;
                LookUpVal1 = Extra1.Rows[0][new2].ToString();
            }

            return LookUpVal1;
        }
        public void getQueryString(Permission VE)
        {
            if (!HttpContext.Current.Request.QueryString.AllKeys.Contains("op"))
            {
                var PreviousUrl = HttpContext.Current.Request.UrlReferrer.AbsoluteUri;
                var uri = new Uri(PreviousUrl);//Create Virtually Query String
                var queryString = HttpUtility.ParseQueryString(uri.Query);
                var MNUDET = queryString.Get("MNUDET").ToString().Replace(" ", "+");
                var UNIQUESESSIONNO = queryString.Get("US").ToString().Replace(" ", "+");
                var DOC_CODE = queryString.Get("DC").ToString().Replace(" ", "+");
                var MENU_PARA = queryString.Get("MP").ToString().Replace(" ", "+");
                VE.MENU_DETAILS = Decrypt_URL(MNUDET);
                VE.UNQSNO_ENCRYPTED = UNIQUESESSIONNO;
                VE.UNQSNO = Decrypt_URL(UNIQUESESSIONNO);
                VE.DOC_CODE = Decrypt_URL(DOC_CODE);
                VE.MENU_PARA = Decrypt_URL(MENU_PARA);
                string[] para = VE.MENU_DETAILS.Split('~');
                VE.MenuID = para[0];
                VE.MenuIndex = para[1];
                VE.ControllerName = para[2];
                VE.MENU_DATE_OPTION = para[3];
                VE.MENU_TYPE = para[4];
                VE.MENU_RIGHT = para[5];

                VE.SearchValue = HttpContext.Current.Request.QueryString["searchValue"];
                if (VE.SearchValue != null && VE.SearchValue.IndexOf('+') >= 0)
                {
                    VE.SearchValue = VE.SearchValue.Replace("+", "%2B");//encodeURIComponent('+')="%2B"
                }
                VE.Searchpannel_State = true;  //Open Search Pannel without help box 
                VE.DefaultAction = queryString.Get("OP").ToString().Replace(" ", "+");
            }
            else
            {
                var MNUDET = HttpContext.Current.Request.QueryString["MNUDET"].ToString().Replace(" ", "+");
                var UNIQUESESSIONNO = HttpContext.Current.Request.QueryString["US"].ToString().Replace(" ", "+");
                var DOC_CODE = HttpContext.Current.Request.QueryString["DC"].ToString().Replace(" ", "+");
                var MENU_PARA = HttpContext.Current.Request.QueryString["MP"].ToString().Replace(" ", "+");
                VE.MENU_DETAILS = Decrypt_URL(MNUDET);
                VE.UNQSNO_ENCRYPTED = UNIQUESESSIONNO;
                VE.UNQSNO = Decrypt_URL(UNIQUESESSIONNO);
                VE.DOC_CODE = Decrypt_URL(DOC_CODE);
                VE.MENU_PARA = Decrypt_URL(MENU_PARA);
                string[] para = VE.MENU_DETAILS.Split('~');
                VE.MenuID = para[0];
                VE.MenuIndex = para[1];
                VE.ControllerName = para[2];
                VE.MENU_DATE_OPTION = para[3];
                VE.MENU_TYPE = para[4];
                VE.MENU_RIGHT = para[5];

                VE.SearchValue = HttpContext.Current.Request.QueryString["searchValue"];
                if (VE.SearchValue != null && VE.SearchValue.IndexOf('+') >= 0)
                {
                    VE.SearchValue = VE.SearchValue.Replace("+", "%2B");//encodeURIComponent('+')="%2B"
                }
                VE.Searchpannel_State = true;  //Open Search Pannel with out help box 
                VE.DefaultAction = HttpContext.Current.Request.QueryString["OP"].ToString().Replace(" ", "+");
            }
            //Modification Remarks Bind
            VE.ListAuditRemarks = Dropdown_AuditRemarks();
            //end Modification Remarks Bind
        }
        public Models.T_CNTRL_HDR T_CONTROL_HDR(string Auto_Number, string DATABASE, string Cancel_Remarks)
        {
            Improvar.Models.ImprovarDB DB = new Models.ImprovarDB(GetConnectionString(), DATABASE);
            Models.T_CNTRL_HDR TCH = new Models.T_CNTRL_HDR();
            TCH = DB.T_CNTRL_HDR.Where(x => x.AUTONO == Auto_Number).SingleOrDefault();
            TCH.CANCEL = "Y";
            TCH.CANC_REM = Cancel_Remarks;
            TCH.CANC_USR_ID = System.Web.HttpContext.Current.Session["UR_ID"].ToString();
            TCH.CANC_USR_ENTDT = System.DateTime.Now;
            TCH.CANC_USR_LIP = GetIp();
            TCH.CANC_USR_SIP = GetStaticIp();
            TCH.CANC_USR_OS = null;
            TCH.CANC_USR_MNM = DetermineCompName(GetIp());
            return TCH;
        }
        public Models.T_CNTRL_HDR T_CONTROL_HDR(string Auto_Number, string DATABASE)
        {
            Improvar.Models.ImprovarDB DB = new Models.ImprovarDB(GetConnectionString(), DATABASE);
            Models.T_CNTRL_HDR TCH = new Models.T_CNTRL_HDR();
            TCH = DB.T_CNTRL_HDR.Where(x => x.AUTONO == Auto_Number).SingleOrDefault();
            if (TCH != null)
            {
                TCH.CANCEL = null;
                TCH.CANC_REM = null;
                TCH.CANC_USR_ID = null;
                TCH.CANC_USR_ENTDT = null;
                TCH.CANC_USR_LIP = null;
                TCH.CANC_USR_SIP = null;
                TCH.CANC_USR_OS = null;
                TCH.CANC_USR_MNM = null;
            }
            return TCH;
        }
        public void DateLock_Entry(Permission VE, ImprovarDB DB, DateTime DOCDT)
        {   //===============Lock date Settings=================
            var UNQSNO = getQueryStringUNQSNO();
            if (VE.MENU_TYPE == "E")
            {
                byte menuindx = Convert.ToByte(VE.MenuIndex);
                M_LOCKDATA LockDt = (from a in DB.M_LOCKDATA where (a.MENU_ID == VE.MenuID && a.MENU_INDEX == menuindx && a.COMPCD == CommVar.Compcd(UNQSNO) && a.LOCCD == CommVar.Loccd(UNQSNO)) select a).SingleOrDefault();
                VE.LOCKDT = LockDt != null ? LockDt.LOCKDT : new DateTime(1900, 12, 12);
                if (VE.LOCKDT != new DateTime(1900, 12, 12))
                {
                    VE.mindate = VE.LOCKDT.Value.Date.AddDays(1).ToString("dd/MM/yyyy");
                } //Will not possible delete back date
                VE.DOCDT = DOCDT;
            }
            //=============End===========================
        }
        public object CheckPark(object ViewClass, string MenuID, string MenuIndex, string loc, string com, string schema, string path, string userid)
        {
            //=======================================park cheking=====================================================
            INI Handel_ini = new INI();
            string[] keys = Handel_ini.GetEntryNames(userid, path);
            if (keys.Length > 0)
            {
                if (keys[0].Length > 0)
                {
                    string id = MenuID + MenuIndex + loc + com + schema;
                    string i = Array.Find(keys, element => element.StartsWith(id, StringComparison.Ordinal));
                    ViewClass.GetType().GetProperty("VisiblePark").SetValue(ViewClass, i == null ? false : true);
                }
            }
            //=============================================end cheking===================================================
            return ViewClass;
        }
        public object CheckPark(object ViewClass, string MNUDET, string loc, string com, string schema, string path, string userid)
        {
            //=======================================park cheking=====================================================
            INI Handel_ini = new INI();
            var MenuID = MNUDET.Split('~')[0];
            var Menuindex = MNUDET.Split('~')[1];
            string[] keys = Handel_ini.GetEntryNames(userid, path);
            if (keys.Length > 0)
            {
                if (keys[0].Length > 0)
                {
                    string id = MenuID + Menuindex + loc + com + schema;
                    string i = Array.Find(keys, element => element.StartsWith(id, StringComparison.Ordinal));
                    ViewClass.GetType().GetProperty("VisiblePark").SetValue(ViewClass, i == null ? false : true);
                }
            }
            //=============================================end cheking===================================================
            return ViewClass;
        }
        public string ConvertNumbertoWords(long number)
        {
            if (number == 0) return "zero";
            if (number < 0) return "minus " + ConvertNumbertoWords(Math.Abs(number));
            string words = "";
            if ((number / 10000000) > 0)
            {
                words += ConvertNumbertoWords(number / 10000000) + " CRORE ";
                number %= 10000000;
            }
            if ((number / 100000) > 0)
            {
                words += ConvertNumbertoWords(number / 100000) + " LAC ";
                number %= 100000;
            }
            if ((number / 1000) > 0)
            {
                words += ConvertNumbertoWords(number / 1000) + " THOUSAND ";
                number %= 1000;
            }
            if ((number / 100) > 0)
            {
                words += ConvertNumbertoWords(number / 100) + " HUNDRED ";
                number %= 100;
            }
            if (number > 0)
            {
                if (words != "") words += "";
                var unitsMap = new[]
                {
            "ZERO", "ONE", "TWO", "THREE", "FOUR", "FIVE", "SIX", "SEVEN", "EIGHT", "NINE", "TEN", "ELEVEN", "TWELVE", "THIRTEEN", "FOURTEEN", "FIFTEEN", "SIXTEEN", "SEVENTEEN", "EIGHTEEN", "NINETEEN"
        };
                var tensMap = new[]
                {
            "ZERO", "TEN", "TWENTY", "THIRTY", "FORTY", "FIFTY", "SIXTY", "SEVENTY", "EIGHTY", "NINETY"
        };
                if (number < 20) words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0) words += " " + unitsMap[number % 10];
                }
            }
            return words;
        }
        public string ConvertNumbertoWordsUSD(long number)
        {
            if (number == 0) return "zero";
            if (number < 0) return "minus " + ConvertNumbertoWords(Math.Abs(number));
            string words = "";
            if ((number / 100000000) > 0)
            {
                words += ConvertNumbertoWords(number / 100000000) + " BILLION ";
                number %= 100000000;
            }
            if ((number / 1000000) > 0)
            {
                words += ConvertNumbertoWords(number / 1000000) + " MILLION ";
                number %= 1000000;
            }
            if ((number / 1000) > 0)
            {
                words += ConvertNumbertoWords(number / 1000) + " THOUSAND ";
                number %= 1000;
            }
            if ((number / 100) > 0)
            {
                words += ConvertNumbertoWords(number / 100) + " HUNDRED ";
                number %= 100;
            }
            if (number > 0)
            {
                if (words != "") words += "";
                var unitsMap = new[]
                {
            "ZERO", "ONE", "TWO", "THREE", "FOUR", "FIVE", "SIX", "SEVEN", "EIGHT", "NINE", "TEN", "ELEVEN", "TWELVE", "THIRTEEN", "FOURTEEN", "FIFTEEN", "SIXTEEN", "SEVENTEEN", "EIGHTEEN", "NINETEEN"
              };
                var tensMap = new[]
                {
            "ZERO", "TEN", "TWENTY", "THIRTY", "FORTY", "FIFTY", "SIXTY", "SEVENTY", "EIGHTY", "NINETY"
                };
                if (number < 20) words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0) words += " " + unitsMap[number % 10];
                }
            }
            return words;
        }
        //public string AmountInWords(string amount)
        //{
        //    double m = Convert.ToInt64(Math.Floor(Convert.ToDouble(amount)));
        //    double l = Convert.ToDouble(amount);
        //    double j = (l - m) * 100;
        //    var beforefloating = ConvertNumbertoWords(Convert.ToInt64(m));
        //    var afterfloating = ConvertNumbertoWords(Convert.ToInt64(j));
        //    string Content = beforefloating;
        //    if (afterfloating != "") Content = Content + " and " + ' ' + afterfloating + ' ' + " Paise Only";
        //    Content = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(Content.ToLower());
        //    //Content = Content.ToUpperInvariant();
        //    return Content;
        //}
        public string AmountInWords(string amount, string CURRCD = "")
        {
            double m = Convert.ToInt64(Math.Floor(Convert.ToDouble(amount)));
            double l = Convert.ToDouble(amount);
            double j = (l - m) * 100; var beforefloating = ""; var afterfloating = ""; string Content = "";
            if (CURRCD == "USD")
            {
                beforefloating = ConvertNumbertoWordsUSD(Convert.ToInt64(m));
                afterfloating = ConvertNumbertoWordsUSD(Convert.ToInt64(j));
                if (afterfloating != "") Content = beforefloating + " DOLLAR and " + ' ' + afterfloating + ' ' + " CENTS Only";
                else Content = beforefloating + " DOLLAR Only";
            }
            else
            {
                beforefloating = ConvertNumbertoWords(Convert.ToInt64(m));
                afterfloating = ConvertNumbertoWords(Convert.ToInt64(j));

                if (afterfloating != "") Content = beforefloating + " Rupes and " + ' ' + afterfloating + ' ' + " Paise Only";
                else Content = beforefloating + " Rupes Only";
            }
            Content = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(Content.ToLower());
            return Content;
        }
        public DateTime? getCurrentDate(string mindate, bool checkwithfinyr = true)
        {
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            DateTime dmindate = System.DateTime.Now.Date;
            if (mindate != "") dmindate = Convert.ToDateTime(mindate);

            DateTime? curdate = System.DateTime.Now.Date;
            string[] financialyeardate = CommVar.FinPeriod(UNQSNO).Split('-');
            DateTime F_TODate = DateTime.ParseExact(financialyeardate[1].Trim().ToString(), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            if (mindate != "")
            {
                if (curdate < dmindate) curdate = dmindate;
                if (checkwithfinyr == true && Convert.ToDateTime(mindate) > F_TODate) curdate = (DateTime?)null;
            }
            if (checkwithfinyr == true && curdate > F_TODate) curdate = F_TODate;
            if (curdate != null) curdate = Convert.ToDateTime(curdate.ToString().Remove(10));
            return curdate;
        }

        public DateTime convstr2date(string dt)
        {
            DateTime retdt;
            retdt = DateTime.ParseExact(dt.Substring(0, 10).Replace('-', '/'), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            return retdt;
        }
        public bool SaveImagetoImprovar(string DBImgString, string ImgName, string imgExtention = "jpg")
        {
            try
            {
                if (DBImgString != "" || DBImgString.IndexOf(',') > 1)
                {
                    DBImgString = DBImgString.Substring(DBImgString.IndexOf(',') + 1);
                }
                String path = @"c:/Improvar";
                string imageName = ImgName + "." + imgExtention;
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path); //Delete file if it  exist
                }
                string imgPath = Path.Combine(path, imageName);
                byte[] imageBytes = Convert.FromBase64String(DBImgString);
                System.IO.File.WriteAllBytes(imgPath, imageBytes);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public string SaveBase64toDrive(string DBImgString, string path)
        {
            try
            {
                if (DBImgString != "" || DBImgString.IndexOf(',') > 1)
                {
                    DBImgString = DBImgString.Substring(DBImgString.IndexOf(',') + 1);
                }
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path); //Delete file if it  exist
                }
                string imgPath = Path.Combine(path, "");
                byte[] imageBytes = Convert.FromBase64String(DBImgString);
                File.WriteAllBytes(imgPath, imageBytes); ;
                return path;
            }
            catch (Exception ex)
            {
                SaveException(ex, "");
                return "";
            }
        }
        public double Roundoff(double amt, int dec = 2)
        {
            double ramt = 0;
            ramt = Math.Round(amt, dec, MidpointRounding.AwayFromZero);
            return ramt;
        }

        public string Indian_Number_format(string number, string cellstyle)
        {
            int i = cellstyle.IndexOf('.');
            if (i < 0)
            {
                return String.Format(new CultureInfo("en-IN", false), "{0:n0}", Convert.ToDouble(number));
            }
            else
            {
                string dec = cellstyle.Substring(i + 1);
                return String.Format(new CultureInfo("en-IN", false), "{0:n" + dec.Length + "}", Convert.ToDouble(number));
            }
        }
        public string GenMasterCode(string tblnm, string fldnm, string cdlike, long maxlength, string schema = "C")
        {
            var UNQSNO = getQueryStringUNQSNO();
            MasterHelpFa MasterHelpFa = new MasterHelpFa();
            string newcode = "";
            string scm = "", sql = "";
            switch (schema)
            {
                case "F":
                    scm = CommVar.FinSchema(UNQSNO); break;
                case "I":
                    scm = CommVar.InvSchema(UNQSNO); break;
                case "S":
                    scm = CommVar.SaleSchema(UNQSNO); break;
                case "P":
                    scm = CommVar.PaySchema(UNQSNO); break;
                default:
                    scm = CommVar.CurSchema(UNQSNO); break;
            }

            sql = "select max(" + fldnm + ") cd from " + scm + "." + tblnm + " where " + fldnm + " like '" + cdlike + "%' ";
            DataTable tbl = MasterHelpFa.SQLquery(sql);
            long noslen = maxlength - cdlike.Length;
            long rplval = 0;
            string fldval = tbl.Rows[0]["cd"].ToString();
            if (fldval != "")
            {
                Int32 spos = cdlike.Length, lpos = fldval.Length;
                rplval = Convert.ToInt32(fldval.ToString().Substring(spos, lpos - spos));
            }
            rplval++;
            newcode = cdlike + rplval.ToString().PadLeft(Convert.ToInt32(noslen), '0');
            return newcode;
        }
        public List<DocumentType> DOC_TYPE()
        {
            ImprovarDB DBI = new ImprovarDB(GetConnectionString(), Getschema);
            var doctP = (from i in DBI.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();
            return doctP;
        }
        public string getlowdednm(string lowded = "")
        {
            string lowdednm = "";
            switch (lowded)
            {
                case "Y":
                    lowdednm = "Lower Deduction or no deduction on a/c of Certificate u/Sec. 197"; break;
                case "N":
                    lowdednm = "No deduction on acount of declaration u/s 197A"; break;
                case "T":
                    lowdednm = "No Deduction for Transporter (Against Valid Pan)"; break;
                default:
                    lowdednm = ""; break;
            }
            return lowdednm;
        }
        public string MaxDocNo_Oth_Schema(string module_code, string doc_cd, string doc_date)
        {
            var UNQSNO = getQueryStringUNQSNO();
            string Schema = "";
            switch (module_code)
            {
                case "I":
                    Schema = CommVar.InvSchema(UNQSNO); break;
                case "F":
                    Schema = System.Web.HttpContext.Current.Session["FIN_SCHEMA"].ToString(); break;
                case "S":
                    Schema = System.Web.HttpContext.Current.Session["SALES_SCHEMA"].ToString(); break;
                case "P":
                    Schema = System.Web.HttpContext.Current.Session["PAY_SCHEMA"].ToString(); break;
            }

            ImprovarDB DB = new Models.ImprovarDB(GetConnectionString(), Schema);
            string LOC = CommVar.Loccd(UNQSNO);
            string COM = CommVar.Compcd(UNQSNO);
            string MonthCode = "";
            String YR1 = CommVar.YearCode(UNQSNO);

            var query = (from c in DB.M_DOCTYPE where (c.DOCCD == doc_cd) select new { DOCJNRL = c.DOCJNRL }).ToList();

            string DocumentNumType = query[0].DOCJNRL.ToString();
            if (DocumentNumType == "Y" || DocumentNumType == "C")
            {
                MonthCode = "0000";
            }
            else if (DocumentNumType == "D")
            {
                string ddddt = doc_date;
                string date = ddddt.Substring(0, 2);
                string month = ddddt.Substring(3, 2);
                MonthCode = month + date;
            }
            else
            {
                DateTime DOC_D = Convert.ToDateTime(doc_date);

                var MCode = (from c in DB.M_MONTH where (DOC_D >= c.SDATE && DOC_D <= c.EDATE) select new { MTHCD = c.MNTHCD }).ToList();
                if (MCode.Count != 0) MonthCode = MCode[0].MTHCD.ToString();
                else MonthCode = DOC_D.Year.ToString().Substring(2, 2) + DOC_D.Month.ToString().PadLeft(2, '0');
            }
            var MAXDOCNO = DB.T_CNTRL_HDR.Where(a => a.DOCCD == doc_cd && a.COMPCD == COM && a.LOCCD == LOC && a.YR_CD == YR1 && a.MNTHCD == MonthCode).Select(a => a.VCHRNO).DefaultIfEmpty().Max();
            string DOCNO = "";
            if (MAXDOCNO == 0)
            {
                DOCNO = "000001";
            }
            else
            {
                MAXDOCNO = MAXDOCNO + 1;
                DOCNO = Convert.ToString(MAXDOCNO).PadLeft(6, '0');
            }
            return DOCNO;
        }

        public string AutoNo_Oth_Schema(string module_code, string doc_no, string doc_code, string Ddate)
        {
            var UNQSNO = getQueryStringUNQSNO();
            string Schema = "";
            switch (module_code)
            {
                case "I":
                    Schema = CommVar.InvSchema(UNQSNO); break;
                case "F":
                    Schema = System.Web.HttpContext.Current.Session["FIN_SCHEMA"].ToString(); break;
                case "S":
                    Schema = System.Web.HttpContext.Current.Session["SALES_SCHEMA"].ToString(); break;
                case "P":
                    Schema = System.Web.HttpContext.Current.Session["PAY_SCHEMA"].ToString(); break;
            }

            ImprovarDB DB = new Models.ImprovarDB(GetConnectionString(), Schema);
            //Improvar.Models.ImprovarDB DB1 = new Models.ImprovarDB(GetConnectionString(), Getschema);
            string LOC = CommVar.Loccd(UNQSNO);
            string COM = CommVar.Compcd(UNQSNO);
            string YR_CD = CommVar.YearCode(UNQSNO);
            string MonthCode = "";
            string autonum = "";
            string ReturnMonthCode = "";

            var query = (from c in DB.M_DOCTYPE where (c.DOCCD == doc_code) select new { DOCJNRL = c.DOCJNRL }).ToList();
            string DocumentNumType = query[0].DOCJNRL.ToString();
            if (DocumentNumType == "Y" || DocumentNumType == "C")
            {
                MonthCode = "0000";
                ReturnMonthCode = MonthCode;
            }
            else if (DocumentNumType == "D")
            {
                string ddddt = Ddate;
                string date = ddddt.Substring(0, 2);
                string month = ddddt.Substring(3, 2);
                MonthCode = month + date;
                ReturnMonthCode = MonthCode;
            }
            else
            {
                DateTime DOC_D = Convert.ToDateTime(Ddate);
                var MCode = (from c in DB.M_MONTH where (DOC_D >= c.SDATE && DOC_D <= c.EDATE) select new { MTHCD = c.MNTHCD }).ToList();
                if (MCode.Count != 0) MonthCode = MCode[0].MTHCD.ToString();
                else MonthCode = DOC_D.Year.ToString().Substring(2, 2) + DOC_D.Month.ToString().PadLeft(2, '0');
                ReturnMonthCode = MonthCode;
            }
            if (doc_code.Length < 5)
            {
                doc_code = doc_code.PadRight(5, '0');
            }
            if (MonthCode.Length < 4)
            {
                MonthCode = MonthCode.PadRight(4, '0');
            }
            autonum = YR_CD + COM + LOC + module_code + doc_code + MonthCode + doc_no;

            return autonum + GCS() + ReturnMonthCode;
        }

        public string Update_PO_Qty(string Autono1, string opt1, string Ind_Po)
        {
            var UNQSNO = getQueryStringUNQSNO();
            string LOCCD = CommVar.Loccd(UNQSNO);
            string COMCD = CommVar.Compcd(UNQSNO);
            string Scm1 = CommVar.InvSchema(UNQSNO);
            string query = "";
            string wh1 = "";
            if (opt1 == "D")
            {
                wh1 = " and x.autono <> '" + Autono1 + "' ";
            }
            Improvar.Models.ImprovarDB DB = new Models.ImprovarDB(GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            Improvar.Models.ImprovarDB DB1 = new Models.ImprovarDB(GetConnectionString(), Getschema);
            if (Ind_Po == "P")
            {
                query = "update " + Scm1 + ".t_pord_dtl a set grn_qty = ";
                query = query + " nvl((select sum(nvl(qnty, 0)) from " + Scm1 + ".t_grn_dtl x, " + Scm1 + ".t_cntrl_hdr y ";
                query = query + " where x.autono = y.autono and ind_type || ind_docno || ind_docdt||po_type||po_docno||po_docdt = a.ind_type||a.ind_docno||a.ind_docdt||a.doccd || a.docno || a.docdt ";
                query = query + " and nvl(slitcd, itcd) = nvl(a.slitcd, a.itcd) and compcd = '" + COMCD + "' and loccd = '" + LOCCD + "' and nvl(y.cancel,'N') = 'N' " + wh1 + "),0) where ";
                query = query + " doccd||docno||docdt in (select po_type||po_docno||po_docdt from " + Scm1 + ".t_grn_dtl where autono ='" + Autono1 + "' )";
            }
            else
            {
                query = "update " + Scm1 + ".t_preq_dtl a set grn_qty = ";
                query = query + " nvl((select sum(nvl(qnty, 0)) from " + Scm1 + ".t_grn_dtl x, " + Scm1 + ".t_cntrl_hdr y ";
                query = query + " where x.autono = y.autono and ind_type || ind_docno || ind_docdt = a.doccd || a.docno || a.docdt ";
                query = query + " and nvl(slitcd, itcd) = nvl(a.slitcd, a.itcd) and compcd = '" + COMCD + "' and loccd = '" + LOCCD + "' and nvl(y.cancel,'N') = 'N' " + wh1 + "),0) where ";
                query = query + " doccd||docno||docdt in (select ind_type||ind_docno||ind_docdt from " + Scm1 + ".t_grn_dtl where autono ='" + Autono1 + "' )";
            }
            return query;
        }
        public string dateRangeForMonthlyDoc_Type(object ViewClass, string doccd, string docdt = "")
        {
            MasterHelp masterHelp = new MasterHelp();
            var UNQSNO = getQueryStringUNQSNO();
            string DisableDate = "";
            string[] financialyeardate = CommVar.FinPeriod(UNQSNO).Split('-');
            financialyeardate[0] = financialyeardate[0].Trim(); financialyeardate[1] = financialyeardate[1].Trim();
            Improvar.Models.ImprovarDB DB = new Models.ImprovarDB(GetConnectionString(), CommVar.CurSchema(UNQSNO));
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            string YRCD = CommVar.YearCode(UNQSNO);
            string scm = CommVar.CurSchema(UNQSNO);
            System.Reflection.PropertyInfo pi = ViewClass.GetType().GetProperty("MenuID");
            String menuname = (String)(pi.GetValue(ViewClass, null));

            System.Reflection.PropertyInfo pii = ViewClass.GetType().GetProperty("MenuIndex");
            Int16 menuindex = Convert.ToInt16((String)(pii.GetValue(ViewClass, null)));
            var lockdate = (from i in DB.M_LOCKDATA where (i.COMPCD == COM && i.LOCCD == LOC && i.SCHEMA_NAME == scm && i.MENU_ID == menuname && i.MENU_INDEX == menuindex) select i.LOCKDT).SingleOrDefault();
            var Fdate = (from i in DB.M_DOCTYPE where (i.DOCCD == doccd) select i.FDATE).SingleOrDefault();
            if (docdt == "")
            {
                string mindate = "", maxdate = "";
                if (lockdate != null)
                {
                    mindate = lockdate.Value.AddDays(1).ToString("dd/MM/yyyy");
                }
                else
                {
                    mindate = financialyeardate[0];
                }
                if (Fdate != null)
                {
                    if (Fdate == "Y")
                    {
                        DateTime dt = convstr2date(financialyeardate[1]);
                        dt = dt.Date.AddYears(5);
                        maxdate = dt.Date.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        //if (lockdate != null)
                        //{
                        maxdate = DateTime.Now.Date.ToString("dd/MM/yyyy");
                        //}
                        //else
                        //{
                        //    maxdate = financialyeardate[1];
                        //}
                    }
                }
                else
                {
                    if (lockdate != null)
                    {
                        maxdate = DateTime.Now.Date.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        maxdate = financialyeardate[1];
                    }
                }
                if (Convert.ToDateTime(maxdate) > Convert.ToDateTime(financialyeardate[1]))
                {
                    maxdate = financialyeardate[1];
                }
                var start = convstr2date(mindate);
                var end = convstr2date(maxdate);
                string str = "select max(docdt)docdt, to_char(max(docdt), 'mmyyyy') monthcode ";
                str += "from " + CommVar.CurSchema(UNQSNO) + ".t_cntrl_hdr ";
                str += "where doccd = '" + doccd + "' and compcd='" + COM + "' and loccd='" + LOC + "' and ";
                str += "docdt >= to_date('" + mindate + "', 'dd/mm/yyyy') and docdt <= to_date('" + maxdate + "', 'dd/mm/yyyy') ";
                str += "group by to_char(docdt, 'mmyyyy') order by docdt";
                DataTable DT = masterHelp.SQLquery(str);
                var maxDate = (from i in DT.AsEnumerable()
                               select new
                               {
                                   MAXDOCDT = i.Field<DateTime>("docdt"),
                                   MONTHCODE = i.Field<string>("monthcode")
                               }).ToList();
                var diff = Enumerable.Range(0, Int32.MaxValue)
                                    .Select(a => start.AddMonths(a))
                                    .TakeWhile(a => a <= end)
                                    .Select(a => a.ToString("MMyyyy")).ToList();
                foreach (var i in diff)
                {
                    var maxDT = (from x in maxDate where x.MONTHCODE == i select x.MAXDOCDT).SingleOrDefault();
                    if (maxDT.Year > 2000)
                    {
                        if (maxDT != null)
                        {
                            DateTime startDT = convstr2date("01/" + maxDT.Month.ToString().PadLeft(2, '0') + "/" + maxDT.Year.ToString());
                            DateTime endDT = DateTime.Now;
                            //if (maxDT.Month == 1)
                            //{
                            //    endDT = maxDT;
                            //}
                            //else
                            //{
                            //    endDT = maxDT.AddDays(-1);
                            //}
                            endDT = maxDT.AddDays(-1);
                            var days = Enumerable.Range(0, Int32.MaxValue)
                                         .Select(a => startDT.AddDays(a))
                                         .TakeWhile(a => a <= endDT)
                                         .Select(a => a.ToString("dd/MM/yyyy")).ToList();
                            DisableDate = DisableDate.Length > 0 ? DisableDate + "," + string.Join(",", days) : DisableDate + string.Join(",", days);
                        }
                    }
                }
                return mindate + "~" + maxdate + "~" + DisableDate;
            }
            else
            {
                DateTime now = DateTime.Now;
                var start = convstr2date(docdt);
                string startDT = "01/" + start.Month.ToString().PadLeft(2, '0') + "/" + start.Year.ToString();
                string lastDT = DateTime.DaysInMonth(start.Year, start.Month).ToString() + "/" + start.Month.ToString().PadLeft(2, '0') + "/" + start.Year.ToString();

                string sql = "select * from (select to_char(max(docdt),'dd/mm/yyyy') mindate ";
                sql += "from " + CommVar.CurSchema(UNQSNO) + ".t_cntrl_hdr ";
                sql += "where doccd = '" + doccd + "' and compcd='" + COM + "' and loccd='" + LOC + "' and ";
                sql += "docdt < to_date('" + docdt + "', 'dd/mm/yyyy') and docdt >= to_date('" + startDT + "', 'dd/mm/yyyy')) a, ";
                sql += "(select to_char(min(docdt),'dd/mm/yyyy') maxdate from " + CommVar.CurSchema(UNQSNO) + ".t_cntrl_hdr ";
                sql += "where doccd = '" + doccd + "' and compcd='" + COM + "' and loccd='" + LOC + "' and ";
                sql += "docdt > to_date('" + docdt + "', 'dd/mm/yyyy') and docdt<= to_date('" + lastDT + "', 'dd/mm/yyyy')) b, ";
                sql += "(select count(autono) total_entry ";
                sql += "from " + CommVar.CurSchema(UNQSNO) + ".t_cntrl_hdr ";
                sql += "where doccd = '" + doccd + "' and compcd='" + COM + "' and loccd='" + LOC + "' and ";
                sql += "docdt =to_date('" + docdt + "', 'dd/mm/yyyy')) c ";
                DataTable DT = masterHelp.SQLquery(sql);

                if (Convert.ToInt32(DT.Rows[0]["total_entry"]) > 1)
                {
                    DT.Rows[0]["mindate"] = docdt;
                }
                if (now.ToString("MM/yyyy") == start.ToString("MM/yyyy"))
                {
                    lastDT = now.ToString("dd/MM/yyyy");
                }
                if (lockdate != null)
                {
                    if (lockdate.Value.ToString("MM/yyyy") == start.ToString("MM/yyyy"))
                    {
                        if (start > lockdate.Value)
                        {
                            var tempdt = lockdate.Value.AddDays(1);
                            startDT = tempdt.ToString("dd/MM/yyyy");
                        }
                        else
                        {
                            startDT = docdt;
                            lastDT = docdt;
                        }
                    }
                }
                string mindate = (DT.Rows[0]["mindate"] == DBNull.Value ? startDT : DT.Rows[0]["mindate"].ToString());
                string maxdate = (DT.Rows[0]["maxdate"] == DBNull.Value ? lastDT : DT.Rows[0]["maxdate"].ToString());
                return mindate + "~" + maxdate;
            }
        }
        public String getdocmaxmindate(string doccd, string docdt, string action, string docno, object ViewClass, bool opening = false, string AUTONO = "")
        {
            try
            {
                MasterHelp masterHelp = new MasterHelp();
                var UNQSNO = getQueryStringUNQSNO();
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), YRCD = CommVar.YearCode(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                using (ImprovarDB DB = new ImprovarDB(GetConnectionString(), CommVar.CurSchema(UNQSNO)))
                {
                    string MonthCode = "";
                    Int64 idocno = 0; Int64 imaxdocno = 0;
                    if (docno != "") idocno = Convert.ToInt64(docno);

                    System.Reflection.PropertyInfo pi = ViewClass.GetType().GetProperty("MenuID");
                    String menuname = (String)(pi.GetValue(ViewClass, null));

                    System.Reflection.PropertyInfo pii = ViewClass.GetType().GetProperty("MenuIndex");
                    String menuindex = (String)(pii.GetValue(ViewClass, null));

                    string FDATE = "Y"; string DocumentNumType = "C";
                    string MFDATE = "", MTDATE = "";
                    if (docdt == "") docdt = System.DateTime.Now.Date.ToString().Substring(0, 10);

                    Boolean backdateval = true;
                    if (doccd.retStr() != "")
                    {
                        var query = (from c in DB.M_DOCTYPE where (c.DOCCD == doccd) select new { DOCJNRL = c.DOCJNRL, FDATE = c.FDATE, BACKDATE = c.BACKDATE }).ToList();
                        if (query.Any())
                        {
                            DocumentNumType = query[0].DOCJNRL.ToString();
                            FDATE = query[0].FDATE.retStr() == "" ? "N" : query[0].FDATE.ToString();
                            if (query[0].BACKDATE.retStr() == "N") backdateval = false;
                        }
                    }

                    if (DocumentNumType == "Y" || DocumentNumType == "C")
                    {
                        MonthCode = "0000";
                    }
                    else if (DocumentNumType == "D")
                    {
                        string ddddt = docdt;
                        string date = ddddt.Substring(0, 2);
                        string month = ddddt.Substring(3, 2);
                        MonthCode = month + date;
                    }
                    else
                    {
                        DateTime DOC_D = convstr2date(docdt);
                        var MCode = (from c in DB.M_MONTH where (DOC_D >= c.SDATE && DOC_D <= c.EDATE) select new { MTHCD = c.MNTHCD, SDATE = c.SDATE, EDATE = c.EDATE }).ToList();
                        if (MCode.Count != 0)
                        {
                            MonthCode = MCode[0].MTHCD.ToString();
                            MFDATE = MCode[0].SDATE.ToString();
                            MTDATE = MCode[0].EDATE.ToString();
                        }
                        else
                        {
                            MonthCode = DOC_D.Year.ToString().Substring(2, 2) + DOC_D.Month.ToString().PadLeft(2, '0');
                        }
                    }
                    string sql = ""; bool runsndqry = true;
                    DataTable rsMax = new DataTable();
                    if (doccd != "")
                    {
                        sql = "";
                        sql += "select a.lockdt, nvl(b.backdate,a.backdate) backdate, ";
                        sql += "(case when b.mindt < a.lockdt then a.lockdt+1 else b.mindt end) mindt, b.maxdt, nvl(b.maxdocno,0) maxdocno from ( ";
                        sql += "select b.lockdt, nvl(b.backdate,'Y') backdate ";
                        sql += "from appl_menu a, " + scm + ".m_lockdata b ";
                        sql += "where a.menu_id='" + menuname + "' and a.menu_index=" + Convert.ToInt32(menuindex) + " and a.menu_id=b.menu_id(+) and a.menu_index=b.menu_index(+) and ";
                        sql += "a.module_code='" + CommVar.ModuleCode() + "' and (b.compcd='" + COM + "' or b.compcd is null) and (b.loccd='" + LOC + "' or b.loccd is null) ) a, ";
                        sql += "( ";
                        sql += "select a.doccd, a.backdate, max(mindt) mindt, min(maxdt) maxdt, max(maxdocno) maxdocno from ";
                        sql += "(select a.doccd, nvl(b.backdate,'N') backdate, min(a.docdt) mindt, max(a.docdt) maxdt, max(a.vchrno) maxdocno ";
                        sql += "from " + scm + ".t_cntrl_hdr a, " + scm + ".m_doctype b ";
                        sql += "where a.doccd=b.doccd(+) and a.doccd='" + doccd + "' and a.mnthcd='" + MonthCode + "' and a.compcd='" + COM + "' and a.loccd='" + LOC + "' and a.yr_cd='" + YRCD + "' ";
                        sql += "group by a.doccd, nvl(b.backdate,'N') ";
                        if (action == "E" || idocno != 0)
                        {
                            double fno = idocno - 1;
                            double tno = idocno + 1;
                            sql += "union all ";
                            sql += "select a.doccd, nvl(b.backdate,'N') backdate, min(a.docdt) mindt, max(a.docdt) maxdt, max(a.vchrno) maxdocno ";
                            sql += "from " + scm + ".t_cntrl_hdr a, " + scm + ".m_doctype b ";
                            sql += "where a.doccd=b.doccd(+) and a.doccd='" + doccd + "' and a.mnthcd='" + MonthCode + "' and a.compcd='" + COM + "' and a.loccd='" + LOC + "' and a.yr_cd='" + YRCD + "' and ";
                            sql += "a.doconlyno between " + fno.ToString() + " and " + tno.ToString() + " ";
                            sql += "group by a.doccd, nvl(b.backdate,'N') ";
                        }
                        sql += ") a ";
                        sql += "group by a.doccd, a.backdate ";
                        sql += ") b";// where b.backdate=a.backdate(+) ";
                        rsMax = masterHelp.SQLquery(sql);
                        if (rsMax.Rows.Count != 0) runsndqry = false;
                    }

                    if (runsndqry == true || doccd == "")
                    {
                        sql = "";
                        sql += "select nvl(b.lockdt,'') lockdt, nvl(b.backdate,'Y') backdate, nvl(b.lockdt+1,'') mindt, '' maxdt, 0 maxdocno ";
                        sql += "from appl_menu a, " + scm + ".m_lockdata b ";
                        sql += "where a.menu_id='" + menuname + "' and a.menu_index=" + Convert.ToInt32(menuindex) + " and a.menu_id=b.menu_id(+) and a.menu_index=b.menu_index(+) and ";
                        sql += "a.module_code='" + CommVar.ModuleCode() + "' and (b.compcd='" + COM + "' or b.compcd is null) and (b.loccd='" + LOC + "' or b.loccd is null) ";
                        rsMax = masterHelp.SQLquery(sql);
                    }
                    if (rsMax.Rows.Count > 0)
                    {
                        if (rsMax.Rows[0]["backdate"].ToString() == "N") backdateval = false;
                    }
                    string[] financialyeardate = CommVar.FinPeriod(UNQSNO).Split('-');
                    financialyeardate[0] = financialyeardate[0].Trim(); financialyeardate[1] = financialyeardate[1].Trim();
                    //DateTime F_FROMDate = DateTime.ParseExact(financialyeardate[0].Trim().ToString(), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                    string cmaxdate = financialyeardate[1].Trim().ToString(), cmindate = financialyeardate[0].Trim().ToString();
                    if (opening == true)
                    {
                        cmaxdate = Convert.ToDateTime(CommVar.FinStartDate(UNQSNO)).AddDays(-1).ToString().retDateStr();
                        cmindate = "01/01/2000";
                    }
                    DateTime cLockdate = new DateTime(1900, 12, 12);
                    //DateTime cLockdate = Convert.ToDateTime(financialyeardate[0].ToString());
                    if (rsMax.Rows.Count != 0)
                    {
                        if (rsMax.Rows[0]["mindt"].ToString() != "") cmindate = rsMax.Rows[0]["mindt"].ToString();
                        if (rsMax.Rows[0]["maxdt"].ToString() != "") cmaxdate = rsMax.Rows[0]["maxdt"].ToString();
                        if (rsMax.Rows[0]["lockdt"].ToString() != "") cLockdate = convstr2date(rsMax.Rows[0]["lockdt"].ToString());
                        if (rsMax.Rows[0]["backdate"].ToString() == "N") backdateval = false;
                        if (action == "A" && backdateval == false && rsMax.Rows[0]["maxdt"].ToString() != "" && doccd != "") cmindate = rsMax.Rows[0]["maxdt"].ToString();
                        imaxdocno = Convert.ToInt64(rsMax.Rows[0]["maxdocno"]);
                    }
                    if (convstr2date(cmaxdate) < convstr2date(cmindate)) cmaxdate = cmindate;

                    //get Current date or fin.to year whichever is earlier
                    DateTime dmindate = System.DateTime.Now.Date;
                    if (cmindate != "") dmindate = convstr2date(cmindate);

                    DateTime curdate = System.DateTime.Now.Date;
                    DateTime F_TODate = DateTime.ParseExact(financialyeardate[1].Trim().ToString(), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    string ccurdate = "";
                    if (cmindate != "")
                    {
                        if (curdate < dmindate) curdate = dmindate;
                    }
                    if (curdate > F_TODate) curdate = F_TODate;
                    curdate = convstr2date(curdate.ToString().Substring(0, 10));
                    ccurdate = curdate.ToString().Substring(0, 10);
                    //eof current date
                    string clockdateplus1 = cLockdate.AddDays(1).ToString().Substring(0, 10);

                    if (docdt == "") docdt = curdate.ToString();
                    string cfutdate = convstr2date(cmaxdate).AddYears(5).ToString().Substring(0, 10);

                    if (DocumentNumType == "M")
                    {

                        var msdt = new DateTime(curdate.Year, curdate.Month, 1);
                        var mtdt = msdt.AddMonths(1).AddDays(-1);

                        if (MFDATE == "") MFDATE = msdt.ToString().Remove(10);
                        if (MTDATE == "") MTDATE = mtdt.ToString().Remove(10);
                        if (convstr2date(MFDATE) < convstr2date(clockdateplus1)) MFDATE = clockdateplus1;
                        if (convstr2date(ccurdate) > convstr2date(MTDATE)) ccurdate = MTDATE;
                        if (action == "A")
                        {
                            //if (FDATE == "Y") cmaxdate = MTDATE; else if (FDATE == "Z") cmaxdate = cfutdate; else cmaxdate = ccurdate;
                            if (FDATE == "Y") cmaxdate = financialyeardate[1]; else if (FDATE == "Z") cmaxdate = cfutdate; else cmaxdate = ccurdate;
                            if (backdateval == true)
                            {
                                cmindate = clockdateplus1;
                                //cmaxdate = System.DateTime.Now.ToString("dd/MM/yyyy");
                            }
                        }
                        if (action == "E" && idocno == imaxdocno || action == "E" && backdateval == true)
                        {
                            if (FDATE == "Y") cmaxdate = MTDATE; else if (FDATE == "Z") cmaxdate = cfutdate; else cmaxdate = ccurdate;
                            if (backdateval == true) cmindate = financialyeardate[0]; cmaxdate = System.DateTime.Now.ToString("dd/MM/yyyy");
                        }
                    }
                    else
                    {
                        if (action == "A")
                        {
                            if (FDATE == "Y") cmaxdate = financialyeardate[1]; else if (FDATE == "Z") cmaxdate = cfutdate; else cmaxdate = ccurdate;
                        }
                        if ((action == "E" && idocno == imaxdocno) || action == "E" && backdateval == true)
                        {
                            if (FDATE == "Y") cmaxdate = financialyeardate[1]; else if (FDATE == "Z") cmaxdate = cfutdate; else cmaxdate = ccurdate;
                        }
                        if (backdateval == true) cmindate = clockdateplus1;
                        //if (FDATE != "Y" && FDATE != "Z") { cmaxdate = System.DateTime.Now.ToString("dd/MM/yyyy"); }
                    }
                    if (DocumentNumType == "D")
                    {
                        cmindate = clockdateplus1;
                        if (FDATE == "Z") cmaxdate = cfutdate; else cmaxdate = financialyeardate[1];
                        if (opening == true) cmaxdate = Convert.ToDateTime(CommVar.FinStartDate(UNQSNO)).AddDays(-1).ToString().retDateStr();
                    }
                    if (backdateval == true && FDATE == "Z") cmaxdate = cfutdate;
                    if (action == "V")
                    {
                        if (docdt == "") docdt = System.DateTime.Now.Date.ToString();
                        cmindate = docdt; cmaxdate = docdt;
                    }
                    if (convstr2date(cmindate) < convstr2date(clockdateplus1)) cmindate = clockdateplus1;
                    //if (convstr2date(cmindate) > convstr2date(cmaxdate)) cmaxdate = cmindate;

                    if (FDATE != "Z" && opening == false && convstr2date(cmindate) < convstr2date(financialyeardate[0].ToString())) cmindate = financialyeardate[0].ToString();

                    cmindate = cmindate.Trim().Substring(0, 10); cmaxdate = cmaxdate.Trim().Substring(0, 10);
                    ViewClass.GetType().GetProperty("LOCKDT").SetValue(ViewClass, cLockdate.AddDays(1));
                    ViewClass.GetType().GetProperty("mindate").SetValue(ViewClass, cmindate);
                    ViewClass.GetType().GetProperty("maxdate").SetValue(ViewClass, cmaxdate);
                    ViewClass.GetType().GetProperty("DOCDT").SetValue(ViewClass, convstr2date(docdt));
                    ViewClass.GetType().GetProperty("AllowBackDate").SetValue(ViewClass, backdateval);
                    //CHECK AND AUTHORISATION start
                    if (AUTONO != "" && action == "V")
                    {
                        string nextlvlsts = "N", prevlvlsts = "A", authcd = "", authslno = "", authlvl = "";
                        DataTable dt = new DataTable();

                        //userid respect level details
                        sql = " select A.AUTHcd,a.LVL,a.slno from (";
                        sql += " select A.AUTHcd,B.LVL,b.slno ";
                        sql += ",row_number() over(partition by A.AUTHcd order by A.AUTHcd,B.LVL asc) as rn " + Environment.NewLine;
                        sql += "from   " + scmf + ".m_sign_auth a,  " + scm + ".M_DOC_AUTH b ";
                        sql += " where a.authcd = b.authcd and a.usrid = '" + CommVar.UserID() + "' AND b.DOCCD = '" + doccd + "'";
                        sql += ") a where rn=1 ";
                        dt = masterHelp.SQLquery(sql);
                        if (dt.Rows.Count > 0)
                        {
                            authcd = dt.Rows[0]["AUTHcd"].retStr();
                            authslno = dt.Rows[0]["slno"].retStr();
                            authlvl = dt.Rows[0]["LVL"].retStr();
                        }


                        sql = "select AUTONO,STSTYPE,FLAG1,USR_ID from  " + scm + ".t_txnstatus where autono='" + AUTONO + "'";
                        dt = masterHelp.SQLquery(sql);
                        bool flagauth = false;
                        foreach (DataRow dr in dt.Rows)
                        {
                            string STSTYPE = dr["STSTYPE"].ToString();

                            if (dr["STSTYPE"].ToString() == "K")
                            {//cheched
                                ViewClass.GetType().GetProperty("IsChecked").SetValue(ViewClass, true);
                            }
                            else if (dr["STSTYPE"].ToString() == "A" || dr["STSTYPE"].ToString() == "N")
                            {
                                string LVL = "";
                                if (STSTYPE == "A")
                                {
                                    sql = "select max(c.lvl)lvl from " + scm + ".t_txnstatus a," + scmf + ".m_sign_auth b," + scm + ".M_DOC_AUTH c " + Environment.NewLine;
                                    sql += "where b.authcd = c.authcd and STSTYPE='" + STSTYPE + "' " + Environment.NewLine;
                                    sql += "and c.lvl not in (select distinct PASS_LEVEL from " + scm + ".t_cntrl_doc_pass where autono=a.autono) " + Environment.NewLine;
                                    sql += "and a.autono='" + dr["autono"].ToString() + "' and  a.USR_ID ='" + dr["USR_ID"].ToString() + "' and  c.doccd ='" + doccd + "' " + Environment.NewLine;
                                }
                                else
                                {
                                    sql = "select min(c.lvl)lvl from " + scm + ".t_txnstatus a," + scmf + ".m_sign_auth b," + scm + ".M_DOC_AUTH c " + Environment.NewLine;
                                    sql += "where b.authcd = c.authcd and STSTYPE='" + STSTYPE + "' " + Environment.NewLine;
                                    sql += "and c.lvl in (select distinct PASS_LEVEL from " + scm + ".t_cntrl_doc_pass where autono=a.autono) " + Environment.NewLine;
                                    sql += "and a.autono='" + dr["autono"].ToString() + "' and  a.USR_ID ='" + dr["USR_ID"].ToString() + "' and  c.doccd ='" + doccd + "' " + Environment.NewLine;
                                }
                                DataTable dt1 = masterHelp.SQLquery(sql);
                                LVL = dt1.Rows[0]["lvl"].retStr();
                                flagauth = true;
                                if (authlvl.retDbl() == 0)
                                {
                                    if (dr["STSTYPE"].ToString() == "A")
                                    {
                                        //Authorised
                                        ViewClass.GetType().GetProperty("AuthorisationStatus").SetValue(ViewClass, "A");
                                    }
                                    else if (dr["STSTYPE"].ToString() == "N")
                                    {
                                        //Authorised
                                        ViewClass.GetType().GetProperty("AuthorisationStatus").SetValue(ViewClass, "N");
                                    }
                                    nextlvlsts = "N"; prevlvlsts = "N";
                                }
                                else if (authlvl.retDbl() <= LVL.retDbl() && dr["STSTYPE"].ToString() == "A")
                                {
                                    //Authorised
                                    ViewClass.GetType().GetProperty("AuthorisationStatus").SetValue(ViewClass, "A");
                                    if (authlvl.retDbl() < LVL.retDbl()) { nextlvlsts = "A"; prevlvlsts = "A"; }
                                }
                                else if (LVL.ToString() == authlvl && dr["STSTYPE"].ToString() == "N")
                                {
                                    //Reject
                                    ViewClass.GetType().GetProperty("AuthorisationStatus").SetValue(ViewClass, "N");
                                }
                                else if (authlvl.retDbl() < LVL.retDbl() && dr["STSTYPE"].ToString() == "N")
                                {
                                    //Authorised
                                    ViewClass.GetType().GetProperty("AuthorisationStatus").SetValue(ViewClass, "A");
                                    if (authlvl.retDbl() < LVL.retDbl() - 1) { nextlvlsts = "A"; prevlvlsts = "A"; }
                                }
                                else if (authlvl.retDbl() > LVL.retDbl() && dr["STSTYPE"].ToString() == "N")
                                {
                                    nextlvlsts = "N"; prevlvlsts = "N";
                                }
                            }
                        }
                        var chk = (from DataRow dr in dt.Rows where dr["STSTYPE"].retStr() == "A" || dr["STSTYPE"].retStr() == "N" select dr).Count();
                        if ((dt == null || chk == 0) && authlvl.retDbl() > 1)
                        {
                            nextlvlsts = "N"; prevlvlsts = "N";
                        }
                        if (prevlvlsts == "A" && nextlvlsts == "N")
                        {
                            ViewClass.GetType().GetProperty("AuthorisationAUTHCD").SetValue(ViewClass, authcd);
                            ViewClass.GetType().GetProperty("AuthorisationSLNO").SetValue(ViewClass, authslno);
                            ViewClass.GetType().GetProperty("AuthorisationLVL").SetValue(ViewClass, authlvl);
                        }

                        sql = "select distinct B.LVL from   " + scmf + ".m_sign_auth a,  " + scm + ".M_DOC_AUTH b ";
                        sql += "where a.authcd = b.authcd AND b.DOCCD = '" + doccd + "'";
                        sql += "and b.lvl not in (select pass_level from " + scm + ".t_cntrl_doc_pass where autono='" + AUTONO + "' )";
                        dt = masterHelp.SQLquery(sql);
                        if (dt.Rows.Count > 0 && flagauth == true)
                        {
                            ViewClass.GetType().GetProperty("Edit").SetValue(ViewClass, "");
                            ViewClass.GetType().GetProperty("Delete").SetValue(ViewClass, "");
                        }
                    }
                    //CHECK AND AUTHORISATION end

                    if (DocumentNumType == "M" && backdateval == false)
                    {
                        if (action == "A")
                        {
                            return dateRangeForMonthlyDoc_Type(ViewClass, doccd);
                        }
                        else
                        {
                            return dateRangeForMonthlyDoc_Type(ViewClass, doccd, docdt);
                        }
                    }
                    else
                    {
                        return cmindate + "~" + cmaxdate;
                    }
                }
            }
            catch (Exception ex)
            {
                SaveException(ex, "");
                return "";
            }
        }
        //public String getdocmaxmindate(string doccd, string docdt, string action, string docno, object ViewClass, bool opening = false, string AUTONO = "")
        //{
        //    try
        //    {
        //        MasterHelp masterHelp = new MasterHelp();
        //        var UNQSNO = getQueryStringUNQSNO();
        //        string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), YRCD = CommVar.YearCode(UNQSNO), scm = CommVar.CurSchema(UNQSNO);
        //        using (ImprovarDB DB = new ImprovarDB(GetConnectionString(), CommVar.CurSchema(UNQSNO)))
        //        {
        //            string MonthCode = "";
        //            Int64 idocno = 0; Int64 imaxdocno = 0;
        //            if (docno != "") idocno = Convert.ToInt64(docno);

        //            System.Reflection.PropertyInfo pi = ViewClass.GetType().GetProperty("MenuID");
        //            String menuname = (String)(pi.GetValue(ViewClass, null));

        //            System.Reflection.PropertyInfo pii = ViewClass.GetType().GetProperty("MenuIndex");
        //            String menuindex = (String)(pii.GetValue(ViewClass, null));

        //            string FDATE = "Y"; string DocumentNumType = "C";
        //            string MFDATE = "", MTDATE = "";
        //            if (docdt == "") docdt = System.DateTime.Now.Date.ToString().Substring(0, 10);

        //            Boolean backdateval = true;
        //            if (doccd.retStr() != "")
        //            {
        //                var query = (from c in DB.M_DOCTYPE where (c.DOCCD == doccd) select new { DOCJNRL = c.DOCJNRL, FDATE = c.FDATE, BACKDATE = c.BACKDATE }).ToList();
        //                if (query.Any())
        //                {
        //                    DocumentNumType = query[0].DOCJNRL.ToString();
        //                    FDATE = query[0].FDATE.retStr() == "" ? "N" : query[0].FDATE.ToString();
        //                    if (query[0].BACKDATE.retStr() == "N") backdateval = false;
        //                }
        //            }

        //            if (DocumentNumType == "Y" || DocumentNumType == "C")
        //            {
        //                MonthCode = "0000";
        //            }
        //            else if (DocumentNumType == "D")
        //            {
        //                string ddddt = docdt;
        //                string date = ddddt.Substring(0, 2);
        //                string month = ddddt.Substring(3, 2);
        //                MonthCode = month + date;
        //            }
        //            else
        //            {
        //                DateTime DOC_D = convstr2date(docdt);
        //                var MCode = (from c in DB.M_MONTH where (DOC_D >= c.SDATE && DOC_D <= c.EDATE) select new { MTHCD = c.MNTHCD, SDATE = c.SDATE, EDATE = c.EDATE }).ToList();
        //                if (MCode.Count != 0)
        //                {
        //                    MonthCode = MCode[0].MTHCD.ToString();
        //                    MFDATE = MCode[0].SDATE.ToString();
        //                    MTDATE = MCode[0].EDATE.ToString();
        //                }
        //                else
        //                {
        //                    MonthCode = DOC_D.Year.ToString().Substring(2, 2) + DOC_D.Month.ToString().PadLeft(2, '0');
        //                }
        //            }
        //            string sql = ""; bool runsndqry = true;
        //            DataTable rsMax = new DataTable();
        //            if (doccd != "")
        //            {
        //                sql = "";
        //                sql += "select a.lockdt, nvl(b.backdate,a.backdate) backdate, ";
        //                sql += "(case when b.mindt < a.lockdt then a.lockdt+1 else b.mindt end) mindt, b.maxdt, nvl(b.maxdocno,0) maxdocno from ( ";
        //                sql += "select b.lockdt, nvl(b.backdate,'Y') backdate ";
        //                sql += "from appl_menu a, " + scm + ".m_lockdata b ";
        //                sql += "where a.menu_id='" + menuname + "' and a.menu_index=" + Convert.ToInt32(menuindex) + " and a.menu_id=b.menu_id(+) and a.menu_index=b.menu_index(+) and ";
        //                sql += "a.module_code='" + CommVar.ModuleCode() + "' and (b.compcd='" + COM + "' or b.compcd is null) and (b.loccd='" + LOC + "' or b.loccd is null) ) a, ";
        //                sql += "( ";
        //                sql += "select a.doccd, a.backdate, max(mindt) mindt, min(maxdt) maxdt, max(maxdocno) maxdocno from ";
        //                sql += "(select a.doccd, nvl(b.backdate,'N') backdate, min(a.docdt) mindt, max(a.docdt) maxdt, max(a.vchrno) maxdocno ";
        //                sql += "from " + scm + ".t_cntrl_hdr a, " + scm + ".m_doctype b ";
        //                sql += "where a.doccd=b.doccd(+) and a.doccd='" + doccd + "' and a.mnthcd='" + MonthCode + "' and a.compcd='" + COM + "' and a.loccd='" + LOC + "' and a.yr_cd='" + YRCD + "' ";
        //                sql += "group by a.doccd, nvl(b.backdate,'N') ";
        //                if (action == "E" || idocno != 0)
        //                {
        //                    double fno = idocno - 1;
        //                    double tno = idocno + 1;
        //                    sql += "union all ";
        //                    sql += "select a.doccd, nvl(b.backdate,'N') backdate, min(a.docdt) mindt, max(a.docdt) maxdt, max(a.vchrno) maxdocno ";
        //                    sql += "from " + scm + ".t_cntrl_hdr a, " + scm + ".m_doctype b ";
        //                    sql += "where a.doccd=b.doccd(+) and a.doccd='" + doccd + "' and a.mnthcd='" + MonthCode + "' and a.compcd='" + COM + "' and a.loccd='" + LOC + "' and a.yr_cd='" + YRCD + "' and ";
        //                    sql += "a.doconlyno between " + fno.ToString() + " and " + tno.ToString() + " ";
        //                    sql += "group by a.doccd, nvl(b.backdate,'N') ";
        //                }
        //                sql += ") a ";
        //                sql += "group by a.doccd, a.backdate ";
        //                sql += ") b";// where b.backdate=a.backdate(+) ";
        //                rsMax = masterHelp.SQLquery(sql);
        //                if (rsMax.Rows.Count != 0) runsndqry = false;
        //            }

        //            if (runsndqry == true || doccd == "")
        //            {
        //                sql = "";
        //                sql += "select nvl(b.lockdt,'') lockdt, nvl(b.backdate,'Y') backdate, nvl(b.lockdt+1,'') mindt, '' maxdt, 0 maxdocno ";
        //                sql += "from appl_menu a, " + scm + ".m_lockdata b ";
        //                sql += "where a.menu_id='" + menuname + "' and a.menu_index=" + Convert.ToInt32(menuindex) + " and a.menu_id=b.menu_id(+) and a.menu_index=b.menu_index(+) and ";
        //                sql += "a.module_code='" + CommVar.ModuleCode() + "' and (b.compcd='" + COM + "' or b.compcd is null) and (b.loccd='" + LOC + "' or b.loccd is null) ";
        //                rsMax = masterHelp.SQLquery(sql);
        //            }
        //            if (rsMax.Rows.Count > 0)
        //            {
        //                if (rsMax.Rows[0]["backdate"].ToString() == "N") backdateval = false;
        //            }
        //            string[] financialyeardate = CommVar.FinPeriod(UNQSNO).Split('-');
        //            financialyeardate[0] = financialyeardate[0].Trim(); financialyeardate[1] = financialyeardate[1].Trim();
        //            //DateTime F_FROMDate = DateTime.ParseExact(financialyeardate[0].Trim().ToString(), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

        //            string cmaxdate = financialyeardate[1].Trim().ToString(), cmindate = financialyeardate[0].Trim().ToString();
        //            if (opening == true)
        //            {
        //                cmaxdate = Convert.ToDateTime(CommVar.FinStartDate(UNQSNO)).AddDays(-1).ToString().retDateStr();
        //                cmindate = "01/01/2000";
        //            }
        //            DateTime cLockdate = new DateTime(1900, 12, 12);
        //            //DateTime cLockdate = Convert.ToDateTime(financialyeardate[0].ToString());
        //            if (rsMax.Rows.Count != 0)
        //            {
        //                if (rsMax.Rows[0]["mindt"].ToString() != "") cmindate = rsMax.Rows[0]["mindt"].ToString();
        //                if (rsMax.Rows[0]["maxdt"].ToString() != "") cmaxdate = rsMax.Rows[0]["maxdt"].ToString();
        //                if (rsMax.Rows[0]["lockdt"].ToString() != "") cLockdate = convstr2date(rsMax.Rows[0]["lockdt"].ToString());
        //                if (rsMax.Rows[0]["backdate"].ToString() == "N") backdateval = false;
        //                if (action == "A" && backdateval == false && rsMax.Rows[0]["maxdt"].ToString() != "" && doccd != "") cmindate = rsMax.Rows[0]["maxdt"].ToString();
        //                imaxdocno = Convert.ToInt64(rsMax.Rows[0]["maxdocno"]);
        //            }
        //            if (convstr2date(cmaxdate) < convstr2date(cmindate)) cmaxdate = cmindate;

        //            //get Current date or fin.to year whichever is earlier
        //            DateTime dmindate = System.DateTime.Now.Date;
        //            if (cmindate != "") dmindate = convstr2date(cmindate);

        //            DateTime curdate = System.DateTime.Now.Date;
        //            DateTime F_TODate = DateTime.ParseExact(financialyeardate[1].Trim().ToString(), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
        //            string ccurdate = "";
        //            if (cmindate != "")
        //            {
        //                if (curdate < dmindate) curdate = dmindate;
        //            }
        //            if (curdate > F_TODate) curdate = F_TODate;
        //            curdate = convstr2date(curdate.ToString().Substring(0, 10));
        //            ccurdate = curdate.ToString().Substring(0, 10);
        //            //eof current date
        //            string clockdateplus1 = cLockdate.AddDays(1).ToString().Substring(0, 10);

        //            if (docdt == "") docdt = curdate.ToString();
        //            string cfutdate = convstr2date(cmaxdate).AddYears(5).ToString().Substring(0, 10);

        //            if (DocumentNumType == "M")
        //            {

        //                var msdt = new DateTime(curdate.Year, curdate.Month, 1);
        //                var mtdt = msdt.AddMonths(1).AddDays(-1);

        //                if (MFDATE == "") MFDATE = msdt.ToString().Remove(10);
        //                if (MTDATE == "") MTDATE = mtdt.ToString().Remove(10);
        //                if (convstr2date(MFDATE) < convstr2date(clockdateplus1)) MFDATE = clockdateplus1;
        //                if (convstr2date(ccurdate) > convstr2date(MTDATE)) ccurdate = MTDATE;
        //                if (action == "A")
        //                {
        //                    //if (FDATE == "Y") cmaxdate = MTDATE; else if (FDATE == "Z") cmaxdate = cfutdate; else cmaxdate = ccurdate;
        //                    if (FDATE == "Y") cmaxdate = financialyeardate[1]; else if (FDATE == "Z") cmaxdate = cfutdate; else cmaxdate = ccurdate;
        //                    if (backdateval == true) { cmindate = clockdateplus1; cmaxdate = System.DateTime.Now.ToString("dd/MM/yyyy"); }
        //                }
        //                if (action == "E" && idocno == imaxdocno || action == "E" && backdateval == true)
        //                {
        //                    if (FDATE == "Y") cmaxdate = MTDATE; else if (FDATE == "Z") cmaxdate = cfutdate; else cmaxdate = ccurdate;
        //                    if (backdateval == true) cmindate = financialyeardate[0]; cmaxdate = System.DateTime.Now.ToString("dd/MM/yyyy");
        //                }
        //            }
        //            else
        //            {
        //                if (action == "A")
        //                {
        //                    if (FDATE == "Y") cmaxdate = financialyeardate[1]; else if (FDATE == "Z") cmaxdate = cfutdate; else cmaxdate = ccurdate;
        //                }
        //                if ((action == "E" && idocno == imaxdocno) || action == "E" && backdateval == true)
        //                {
        //                    if (FDATE == "Y") cmaxdate = financialyeardate[1]; else if (FDATE == "Z") cmaxdate = cfutdate; else cmaxdate = ccurdate;
        //                }
        //                if (backdateval == true) cmindate = clockdateplus1;
        //                if (FDATE != "Y" && FDATE != "Z") { cmaxdate = System.DateTime.Now.ToString("dd/MM/yyyy"); }
        //            }
        //            if (DocumentNumType == "D")
        //            {
        //                cmindate = clockdateplus1;
        //                if (FDATE == "Z") cmaxdate = cfutdate; else cmaxdate = financialyeardate[1];
        //                if (opening == true) cmaxdate = Convert.ToDateTime(CommVar.FinStartDate(UNQSNO)).AddDays(-1).ToString().retDateStr();
        //            }
        //            if (backdateval == true && FDATE == "Z") cmaxdate = cfutdate;
        //            if (action == "V")
        //            {
        //                if (docdt == "") docdt = System.DateTime.Now.Date.ToString();
        //                cmindate = docdt; cmaxdate = docdt;
        //            }
        //            if (convstr2date(cmindate) < convstr2date(clockdateplus1)) cmindate = clockdateplus1;
        //            if (convstr2date(cmindate) > convstr2date(cmaxdate)) cmaxdate = cmindate;

        //            if (FDATE != "Z" && opening == false && convstr2date(cmindate) < convstr2date(financialyeardate[0].ToString())) cmindate = financialyeardate[0].ToString();

        //            cmindate = cmindate.Trim().Substring(0, 10); cmaxdate = cmaxdate.Trim().Substring(0, 10);
        //            ViewClass.GetType().GetProperty("LOCKDT").SetValue(ViewClass, cLockdate.AddDays(1));
        //            ViewClass.GetType().GetProperty("mindate").SetValue(ViewClass, cmindate);
        //            ViewClass.GetType().GetProperty("maxdate").SetValue(ViewClass, cmaxdate);
        //            ViewClass.GetType().GetProperty("DOCDT").SetValue(ViewClass, convstr2date(docdt));
        //            ViewClass.GetType().GetProperty("AllowBackDate").SetValue(ViewClass, backdateval);
        //            //CHECK AND AUTHORISATION start
        //            if (AUTONO != "" && action == "V")
        //            {
        //                sql = "select AUTONO,STSTYPE,FLAG1 from  " + CommVar.CurSchema(UNQSNO) + ".t_txnstatus where autono='" + AUTONO + "'";
        //                DataTable dt = masterHelp.SQLquery(sql);
        //                foreach (DataRow dr in dt.Rows)
        //                {
        //                    if (dr["STSTYPE"].ToString() == "K")
        //                    {//cheched
        //                        ViewClass.GetType().GetProperty("IsChecked").SetValue(ViewClass, true);
        //                    }
        //                    else if (dr["STSTYPE"].ToString() == "A")
        //                    {//Authorised
        //                        ViewClass.GetType().GetProperty("AuthorisationStatus").SetValue(ViewClass, "A");
        //                        ViewClass.GetType().GetProperty("Edit").SetValue(ViewClass, "");
        //                    }
        //                    else if (dr["STSTYPE"].ToString() == "N")
        //                    {//Reject
        //                        ViewClass.GetType().GetProperty("AuthorisationStatus").SetValue(ViewClass, "N");
        //                    }
        //                }

        //                sql = " select A.AUTHcd,B.LVL,b.slno from   " + CommVar.FinSchema(UNQSNO) + ".m_sign_auth a,  " + CommVar.CurSchema(UNQSNO) + ".M_DOC_AUTH b" +
        //                      " where a.authcd = b.authcd and a.usrid = '" + CommVar.UserID() + "' AND b.DOCCD = '" + doccd + "'";
        //                dt = masterHelp.SQLquery(sql);
        //                if (dt.Rows.Count > 0)
        //                {
        //                    ViewClass.GetType().GetProperty("AuthorisationAUTHCD").SetValue(ViewClass, dt.Rows[0]["AUTHcd"].retStr());
        //                    ViewClass.GetType().GetProperty("AuthorisationSLNO").SetValue(ViewClass, dt.Rows[0]["slno"].retStr());
        //                    ViewClass.GetType().GetProperty("AuthorisationLVL").SetValue(ViewClass, dt.Rows[0]["LVL"].retStr());
        //                }
        //            }
        //            //CHECK AND AUTHORISATION end

        //            if (DocumentNumType == "M" && backdateval == false)
        //            {
        //                if (action == "A")
        //                {
        //                    return dateRangeForMonthlyDoc_Type(ViewClass, doccd);
        //                }
        //                else
        //                {
        //                    return dateRangeForMonthlyDoc_Type(ViewClass, doccd, docdt);
        //                }
        //            }
        //            else
        //            {
        //                return cmindate + "~" + cmaxdate;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        SaveException(ex, "");
        //        return "";
        //    }
        //}
        //public string MaxDocNumber(string doc_cd, string doc_date)
        //{
        //    Improvar.Models.ImprovarDB DB = new Models.ImprovarDB(GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
        //    Improvar.Models.ImprovarDB DB1 = new Models.ImprovarDB(GetConnectionString(), Getschema);
        //    string LOC =  CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO);
        //    string MonthCode = "";

        //    var query = (from c in DB.M_DOCTYPE where (c.DOCCD == doc_cd) select new { DOCJNRL = c.DOCJNRL }).ToList();

        //    string DocumentNumType = query[0].DOCJNRL.ToString();
        //    if (DocumentNumType == "Y" || DocumentNumType == "C")
        //    {
        //        MonthCode = "0000";
        //    }
        //    else if (DocumentNumType == "D")
        //    {
        //        string ddddt = doc_date;
        //        string date = ddddt.Substring(0, 2);
        //        string month = ddddt.Substring(3, 2);
        //        MonthCode = month + date;
        //    }
        //    else
        //    {
        //        DateTime DOC_D = Convert.ToDateTime(doc_date);

        //        var MCode = (from c in DB.M_MONTH where (DOC_D >= c.SDATE && DOC_D <= c.EDATE) select new { MTHCD = c.MNTHCD }).ToList();
        //        if (MCode.Count != 0) MonthCode = MCode[0].MTHCD.ToString();
        //        else MonthCode = DOC_D.Year.ToString().Substring(2, 2) + DOC_D.Month.ToString().PadLeft(2, '0');
        //    }
        //    string yrcd = CommVar.YearCode(UNQSNO);
        //    var MAXDOCNO = DB.T_CNTRL_HDR.Where(a => a.DOCCD == doc_cd && a.COMPCD == COM && a.LOCCD == LOC && a.MNTHCD == MonthCode && a.YR_CD == yrcd).Select(a => a.VCHRNO).DefaultIfEmpty().Max();
        //    string DOCNO = "";
        //    if (MAXDOCNO == 0)
        //    {
        //        DOCNO = "000001";
        //    }
        //    else
        //    {
        //        MAXDOCNO = MAXDOCNO + 1;
        //        DOCNO = Convert.ToString(MAXDOCNO).PadLeft(6, '0');
        //    }
        //    return DOCNO;
        //}
        public string Autonumber_Transaction(string com_code, string loc_code, string doc_no, string doc_code, string Ddate, string oldautono = "", string module_code = "")
        {
            var UNQSNO = getQueryStringUNQSNO();
            Improvar.Models.ImprovarDB DB = new Models.ImprovarDB(GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            Improvar.Models.ImprovarDB DB1 = new Models.ImprovarDB(GetConnectionString(), Getschema);
            string LOC = CommVar.Loccd(UNQSNO);
            string COM = CommVar.Compcd(UNQSNO);
            string YR_CD = CommVar.YearCode(UNQSNO);
            string MonthCode = "";
            string autonum = "";
            string ReturnMonthCode = "";

            var query = (from c in DB.M_DOCTYPE where (c.DOCCD == doc_code) select new { DOCJNRL = c.DOCJNRL }).ToList();
            string DocumentNumType = query[0].DOCJNRL.ToString();
            if (DocumentNumType == "Y" || DocumentNumType == "C")
            {
                MonthCode = "0000";
                ReturnMonthCode = MonthCode;
            }
            else if (DocumentNumType == "D")
            {
                string ddddt = Ddate;
                string date = ddddt.Substring(0, 2);
                string month = ddddt.Substring(3, 2);
                MonthCode = month + date;
                ReturnMonthCode = MonthCode;
            }
            else
            {
                DateTime DOC_D = Convert.ToDateTime(Ddate);
                if (Ddate == null) DOC_D = Convert.ToDateTime(System.DateTime.Now.ToString().retDateStr());
                var MCode = (from c in DB.M_MONTH where (DOC_D >= c.SDATE && DOC_D <= c.EDATE) select new { MTHCD = c.MNTHCD }).ToList();
                if (MCode.Count != 0) MonthCode = MCode[0].MTHCD.ToString();
                else MonthCode = DOC_D.Year.ToString().Substring(2, 2) + DOC_D.Month.ToString().PadLeft(2, '0');
                ReturnMonthCode = MonthCode;
            }
            if (doc_code.Length < 5)
            {
                doc_code = doc_code.PadRight(5, '0');
            }
            if (MonthCode.Length < 4)
            {
                MonthCode = MonthCode.PadRight(4, '0');
            }
            autonum = YR_CD + com_code + loc_code + "S" + doc_code + MonthCode + doc_no;

            return autonum + GCS() + ReturnMonthCode;
        }
        public string GenerateOTP(bool Alphanumeric, int length)
        {
            string alphabets = "ABCDEFGHJKLMNOPQRSTUVWXYZ";
            string small_alphabets = "abcdefghijkmnopqrstuvwxyz";
            string numbers = "1234567890";
            string specialchar = "@$";
            string characters = numbers;
            if (Alphanumeric)
            {
                characters += alphabets + specialchar + small_alphabets + numbers;
            }
            string otp = string.Empty;
            for (int i = 0; i < length; i++)
            {
                string character = string.Empty;
                do
                {
                    int index = new Random().Next(0, characters.Length);
                    character = characters.ToCharArray()[index].ToString();
                } while (otp.IndexOf(character) != -1);
                otp += character;
            }
            return otp;
        }
        public string passwordcheckfrompolicy(string uid, string pwd)
        {
            MasterHelpFa MasterHelpFa = new MasterHelpFa();
            string outp = "", msg = "";
            int TNUMCHR = 0, TTXTCHR = 0, TSPLCHR = 0, TPWDREUSE = 0, TUPR = 0, TLWR = 0;
            string XPWDSEQ = "T", REUSE = "N";
            int XNUMCHR = 0, XTXTCHR = 0, XSPLCHR = 0, XTPWDCHNG = 0, MINLEN = 0, MAXLEN = 0, XCNT = 0, XUPR = 0, XLWR = 0;
            string sql = "";
            DataTable tbl = new DataTable();
            sql = "SELECT POLICY_NO, NOOFNUMCHAR, NOOFTXTCHAR, NOOFSPCHAR, NVL(REUSE_PSSW_CYCL,0) as PWDRESUEAFTER, NVL(REUSE_OF_PSSW,'Y') REUSE_OF_PSSW, ";
            sql += "MINPWDLENGTH, MAXPWDLENGTH, NOOFLOWERCHAR, NOOFUPPERCHAR FROM IPSMART_POLICY WHERE POLICY_DATE <= TO_DATE('" + DateTime.Now.Date.ToShortDateString() + "', 'DD/MM/YYYY') order by policy_no desc";
            tbl = MasterHelpFa.SQLquery(sql);

            TNUMCHR = Convert.ToInt32(tbl.Rows[0]["NOOFNUMCHAR"]);
            TTXTCHR = Convert.ToInt32(tbl.Rows[0]["NOOFTXTCHAR"]);
            TSPLCHR = Convert.ToInt32(tbl.Rows[0]["NOOFSPCHAR"]);
            TPWDREUSE = Convert.ToInt32(tbl.Rows[0]["PWDRESUEAFTER"]);
            MINLEN = Convert.ToInt32(tbl.Rows[0]["MINPWDLENGTH"]);
            MAXLEN = Convert.ToInt32(tbl.Rows[0]["MAXPWDLENGTH"]);
            TUPR = Convert.ToInt32(tbl.Rows[0]["NOOFUPPERCHAR"]);
            TLWR = Convert.ToInt32(tbl.Rows[0]["NOOFLOWERCHAR"]);
            REUSE = tbl.Rows[0]["REUSE_OF_PSSW"].ToString();

            if (pwd.Length < MINLEN)
            {
                outp = "4";
                msg = "Password should be minimum length " + MINLEN.ToString();
            }
            else if (pwd.Length > MAXLEN)
            {
                outp = "4";
                msg = "Password should be maximum length " + MAXLEN.ToString();
            }
            else
            {
                foreach (char ch in pwd.ToCharArray())
                {
                    if (ch >= 'A' && ch <= 'Z')
                    {
                        XTXTCHR = XTXTCHR + 1; XUPR = XUPR + 1;
                    }
                    else if (ch >= 'a' && ch <= 'z')
                    {
                        XTXTCHR = XTXTCHR + 1; XLWR = XLWR + 1;
                    }
                    else if (ch >= '0' && ch <= '9')
                    {
                        XNUMCHR = XNUMCHR + 1;
                    }
                    else
                    {
                        XSPLCHR = XSPLCHR + 1;
                    }
                }
                //for (int i = 0; i <= MAXLEN - 1; i++)
                //{
                //    if (Strings.Asc(pwd.ToUpper().PadRight(MAXLEN, ' ').Substring(i, 1)) >= 48 && Strings.Asc(pwd.ToUpper().PadRight(MAXLEN, ' ').Substring(i, 1)) <= 57)
                //    {
                //        XNUMCHR = XNUMCHR + 1;
                //    }
                //    else if (Strings.Asc(pwd.PadRight(MAXLEN, ' ').Substring(i, 1)) >= 65 && Strings.Asc(pwd.PadRight(MAXLEN, ' ').Substring(i, 1)) <= 90)
                //    {
                //        XTXTCHR = XTXTCHR + 1; XUPR = XUPR + 1;
                //    }
                //    else if (Strings.Asc(pwd.PadRight(MAXLEN, ' ').Substring(i, 1)) >= 97 && Strings.Asc(pwd.PadRight(MAXLEN, ' ').Substring(i, 1)) <= 122)
                //    {
                //        XTXTCHR = XTXTCHR + 1; XLWR = XLWR + 1;
                //    }
                //    else if (Strings.Asc(pwd.ToUpper().PadRight(MAXLEN, ' ').Substring(i, 1)) >= 33 && Strings.Asc(pwd.ToUpper().PadRight(MAXLEN, ' ').Substring(i, 1)) <= 47 || Strings.Asc(pwd.ToUpper().PadRight(MAXLEN, ' ').Substring(i, 1)) >= 58 && Strings.Asc(pwd.ToUpper().PadRight(MAXLEN, ' ').Substring(i, 1)) <= 64)
                //    {
                //        XSPLCHR = XSPLCHR + 1;
                //    }
                //}
                if ((XNUMCHR < TNUMCHR) || (XTXTCHR < TTXTCHR) || (XSPLCHR < TSPLCHR) || XUPR < TUPR || XLWR < TLWR)
                {
                    outp = "3";
                    msg = "Password should contain at least " + TNUMCHR + " Numeric, " + TTXTCHR + " Text and " + TSPLCHR + " Special and " + TUPR + " Upper " + TLWR + " Lower Character(s)";
                }
                else if (REUSE == "N" && pwd.ToUpper().IndexOf(uid.ToUpper()) != -1)
                {
                    outp = "3";
                    msg = "Password should not contain userid";
                }
                else
                {
                    XTPWDCHNG = 0;
                    sql = "SELECT NVL(TIMES_PSSW_CHANGED,1)as TIMESPWDCHANGED FROM USER_APPL WHERE USER_ID ='" + uid + "'";
                    tbl = MasterHelpFa.SQLquery(sql);
                    XTPWDCHNG = Convert.ToInt32(tbl.Rows[0]["TIMESPWDCHANGED"]);

                    sql = "SELECT COUNT(*) FROM PSWD_HISTORY WHERE USER_ID = '" + uid + "'";
                    tbl = MasterHelpFa.SQLquery(sql);
                    XCNT = Convert.ToInt32(tbl.Rows[0][0]);
                    outp = "0";
                    if (XCNT > 0 || TPWDREUSE > 0)
                    {
                        string MWORD = "";
                        sql = "select * from (";
                        sql += "SELECT OLD_PSWRD,NEW_PSWRD FROM PSWD_HISTORY WHERE USER_ID ='" + uid + "' ";
                        sql += "ORDER BY USR_ENTDT DESC ) where rownum <= " + TPWDREUSE + " ";
                        tbl = MasterHelpFa.SQLquery(sql);
                        if (tbl.Rows.Count > 0)
                        {
                            for (int x = 0; x <= tbl.Rows.Count - 1; x++)
                            {
                                string XXXSTR = tbl.Rows[x][0].ToString();
                                MWORD = Decrypt(XXXSTR);
                                if (uid + "56" + pwd == MWORD.Trim())
                                {
                                    outp = "4";
                                    msg = "Password can't be of last " + TPWDREUSE + " password"; break;
                                }
                                MWORD = "";
                            }
                        }
                    }
                }
            }
            return outp + GCS() + msg;
        }
        public bool CheckValidIP(string IPA)
        {
            bool retval = false;
            IPAddress IP;
            bool flag = IPAddress.TryParse(IPA, out IP);
            if (flag)
            {
                if (IP.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) flag = false;
            }
            retval = flag;
            if (IPA == "::1") retval = false;
            return retval;
        }
        public bool LoginIPValidate(string uid)
        {
            MasterHelpFa MasterHelpFa = new MasterHelpFa();
            bool retval = true;
            string sql = "", outaccess = "Y";
            sql = "select nvl(outaccess,'Y') outaccess from " + CommVar.CommSchema() + ".user_appl where user_id='" + uid + "' ";
            DataTable tbl = MasterHelpFa.SQLquery(sql);
            if (tbl.Rows.Count > 0) outaccess = tbl.Rows[0]["outaccess"].ToString();
            if (outaccess == "N")
            {
                string ipAddr = GetStaticIp();
                bool lanip = false;
                switch (ipAddr.Substring(0, 3))
                {
                    case "192": lanip = true; break;
                    case "172": lanip = true; break;
                    case "127": lanip = true; break;
                    case "10.": lanip = true; break;
                    case "..1": lanip = true; break;
                    default: lanip = false; break;
                }
                if (lanip == false)
                {
                    sql = "select staticip from " + CommVar.CommSchema() + ".user_webnetwork";
                    tbl = MasterHelpFa.SQLquery(sql);
                    if (tbl.Rows.Count > 0) retval = false;
                    for (int i = 0; i <= tbl.Rows.Count - 1; i++)
                    {
                        if (tbl.Rows[i]["staticip"].ToString() == ipAddr) { retval = true; break; }
                    }
                }
            }
            if (retval == false)
            {
                sql = "insert into pssw_invalid (user_id, password, login_date, user_ip, user_static_ip) values (";
                sql += "'" + uid + "','Invalid Access',SYSDATE,'" + GetIp() + "','" + GetStaticIp() + "')";
                MasterHelpFa.SQLNonQuery(sql);
            }
            return retval;
        }
        public string MaxDocNumber(string doc_cd, string doc_date, string YR_CD = "")
        {
            var UNQSNO = getQueryStringUNQSNO();
            string date_code = "", month_code = "";
            date_code = doc_date.Substring(3, 2) + doc_date.Substring(0, 2);

            DateTime DOC_D = Convert.ToDateTime(doc_date);

            month_code = DOC_D.Year.ToString().Substring(2, 2) + DOC_D.Month.ToString().PadLeft(2, '0');

            string yrcd = YR_CD;
            if (YR_CD == "") yrcd = CommVar.YearCode(UNQSNO); else yrcd = YR_CD;

            MasterHelp MasterHelp = new MasterHelp();
            string sql = "", scm = CommVar.CurSchema(UNQSNO);

            sql += "select nvl(b.maxdocno,0) maxdocno from ";

            sql += "(select a.docjnrl, a.doccd, (case a.docjnrl when 'M' then nvl(b.mnthcd,a.mnthcd) else a.mnthcd end) mnthcd from ";
            sql += "(select a.docjnrl, a.doccd, ";
            sql += "(case a.docjnrl when 'D' then '" + date_code + "' when 'M' then '" + month_code + "' else '0000' end ) mnthcd ";
            sql += "from " + scm + ".m_doctype a where a.doccd='" + doc_cd + "' ) a, ";

            sql += "(select max(a.mnthcd) mnthcd ";
            sql += "from " + scm + ".m_month a where to_date('" + doc_date.retDateStr() + "','dd/mm/yyyy') between a.sdate and a.edate ) b ";
            sql += ") a, ";

            sql += "( select a.mnthcd, max(a.vchrno) maxdocno ";
            sql += "from " + scm + ".t_cntrl_hdr a ";
            sql += "where a.doccd='" + doc_cd + "' and ";
            sql += "a.compcd='" + CommVar.Compcd(UNQSNO) + "' and a.loccd='" + CommVar.Loccd(UNQSNO) + "' and a.yr_cd='" + yrcd + "' ";
            sql += "group by a.mnthcd) b ";

            sql += "where a.mnthcd=b.mnthcd(+) ";

            DataTable tbl = MasterHelp.SQLquery(sql);

            string DOCNO = Convert.ToString(Convert.ToDouble(tbl.Rows[0]["maxdocno"]) + 1).PadLeft(6, '0');
            return DOCNO;
        }
        //public string DocPattern(long docno, string doc_type_code, string schema, string FIN_SCHEMAS, string docdt = "")
        //{
        //    var UNQSNO = getQueryStringUNQSNO();
        //    Improvar.Models.ImprovarDB DB = new Models.ImprovarDB(GetConnectionString(), schema);
        //    M_DOCTYPE MD = DB.M_DOCTYPE.Find(doc_type_code);
        //    string[] pattern = new[] { "&comprefix&", "&locprefix&", "&docprefix&", "&mm&-&docno&", "&mmm&", "&docno&", "&finyr&", "&finyrs&" };
        //    string newPattern = MD.DOCNOPATTERN;
        //    string docno1 = "";
        //    DateTime ddate;
        //    if (docdt == "") ddate = DateTime.Now; else ddate = Convert.ToDateTime(docdt);

        //    if (MD.DOCNOWOZERO == "N")
        //    {
        //        docno1 = docno.ToString().PadLeft(Convert.ToInt16(MD.DOCNOMAXLENGTH), '0');
        //    }
        //    else
        //    {
        //        docno1 = docno.ToString();
        //    }
        //    string[] finyr = CommVar.FinPeriod(UNQSNO).Split('-');
        //    string asstyr = "", asstyrs = "";
        //    if (finyr[0].ToString().Trim().Substring(8) == finyr[1].ToString().Trim().Substring(8)) asstyr = finyr[0].ToString().Trim().Substring(8);
        //    else asstyr = finyr[0].ToString().Trim().Substring(8) + "-" + finyr[1].ToString().Trim().Substring(8);
        //    asstyrs = asstyr.Replace("-", "");
        //    Improvar.Models.ImprovarDB DB1 = new Models.ImprovarDB(GetConnectionString(), FIN_SCHEMAS);
        //    M_LOCA MLOCA = DB1.M_LOCA.Find(CommVar.Loccd(UNQSNO), CommVar.Compcd(UNQSNO));
        //    string[] pattern1 = new[] { CommVar.Compcd(UNQSNO), MLOCA.LOCA_CODE, MD.DOCPRFX, ddate.Month.ToString().PadLeft(2, '0') + "-" + docno1.ToString(), CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(ddate.Month), docno1.ToString(), asstyr, asstyrs };
        //    for (int i = 0; i <= pattern.Length - 1; i++)
        //    {
        //        int index = newPattern.IndexOf(pattern[i]);
        //        if (index >= 0)
        //        {
        //            newPattern = newPattern.Replace(pattern[i], pattern1[i]);
        //        }
        //    }
        //    return newPattern;
        //}

        public string DocPattern(long docno, string doc_type_code, string schema, string FIN_SCHEMAS, string docdt = "", string docpara = "")
        {
            MasterHelp masterHelp = new MasterHelp();
            var UNQSNO = getQueryStringUNQSNO();
            Improvar.Models.ImprovarDB DB = new Models.ImprovarDB(GetConnectionString(), schema);
            M_DOCTYPE MD = DB.M_DOCTYPE.Find(doc_type_code);
            string[] pattern = new[] { "&comprefix&", "&locprefix&", "&docprefix&", "&mm&-&docno&", "&mmm&", "&docno&", "&yy&", "&finyr&", "&finyrs&", "&finyrf&", "&docpara&" };
            string newPattern = MD.DOCNOPATTERN;
            string docno1 = "";
            DateTime ddate;
            if (docdt == "") ddate = DateTime.Now; else ddate = Convert.ToDateTime(docdt);

            if (MD.DOCNOWOZERO == "N")
            {
                docno1 = docno.ToString().PadLeft(Convert.ToInt16(MD.DOCNOMAXLENGTH), '0');
            }
            else
            {
                docno1 = docno.ToString();
            }
            string[] dfinyr = CommVar.FinPeriod(UNQSNO).Split('-');
            string finyr = "", finyrs = "", yy = "";
            yy = dfinyr[0].ToString().Trim().Substring(8);
            if (yy == dfinyr[1].ToString().Trim().Substring(8)) finyr = yy;
            else finyr = yy + "-" + dfinyr[1].ToString().Trim().Substring(8);
            finyrs = finyr.Replace("-", "");
            string finyrf = dfinyr[0].ToString().Trim().Substring(6) + "-" + yy;
            Improvar.Models.ImprovarDB DB1 = new Models.ImprovarDB(GetConnectionString(), FIN_SCHEMAS);
            M_LOCA MLOCA = DB1.M_LOCA.Find(CommVar.Loccd(UNQSNO), CommVar.Compcd(UNQSNO));
            string[] pattern1 = new[] { CommVar.Compcd(UNQSNO), MLOCA.LOCA_CODE, MD.DOCPRFX, ddate.Month.ToString().PadLeft(2, '0') + "-" + docno1.ToString(), CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(ddate.Month), docno1.ToString(), yy, finyr, finyrs, finyrf, docpara };
            for (int i = 0; i <= pattern.Length - 1; i++)
            {
                int index = newPattern.IndexOf(pattern[i]);
                if (index >= 0)
                {
                    newPattern = newPattern.Replace(pattern[i], pattern1[i]);
                }
            }
            return newPattern;
        }

        public Models.T_CNTRL_HDR Model_T_Cntrl_Hdr(ImprovarDB DB, string AUTONO, string DCODE, DateTime DOCDT, string DNO, string MCODE, string DPATTERN, string ACTION, string glcd = "", string slcd = "", double docamt = 0, string calauto = "", string modcd = "", string yrcd = "", string auditrem = "")
        {
            var UNQSNO = getQueryStringUNQSNO();
            MasterHelp MasterHelp = new MasterHelp();
            Models.T_CNTRL_HDR TCH = new Models.T_CNTRL_HDR();
            string scm = DB.CacheKey;
            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            if (modcd.retStr() == "") modcd = Module.Module_Code.Substring(0, 1);
            if (yrcd.retStr() == "") yrcd = CommVar.YearCode(UNQSNO);

            if (ACTION == "E")
            {
                string sql = "update " + scm + ".t_cntrl_hdr set ";
                sql += "dtag='E', ";
                sql += "emd_no=nvl(emd_no,0)+1, ";
                sql += "docdt=to_date('" + DOCDT.retDateStr() + "','dd/mm/yyyy'), ";
                sql += "glcd='" + glcd + "', ";
                sql += "slcd='" + slcd + "', ";
                sql += "docamt=" + docamt.ToString() + ", ";
                sql += "lm_usr_id='" + CommVar.UserID() + "', ";
                sql += "lm_usr_entdt=sysdate, ";
                sql += "lm_usr_lip='" + GetIp() + "', ";
                sql += "lm_usr_sip='" + GetStaticIp() + "', ";
                sql += "lm_usr_mnm='" + DetermineCompName(GetIp()) + "' ";
                sql = sql + ", LM_REM=" + MasterHelp.filc(auditrem);
                sql += " where autono=:autono ";
                string sqld = "select docno, nvl(emd_no,0)+1 emd_no, mnthcd, doccd, vchrno, doconlyno ";
                sqld += "from " + DB.CacheKey + ".t_cntrl_hdr where autono='" + AUTONO + "'";
                DataTable tbl = MasterHelp.SQLquery(sqld);

                DB.Database.ExecuteSqlCommand(sql, new OracleParameter("autono", OracleDbType.Varchar2, 30, AUTONO, ParameterDirection.Input));
                TCH.MNTHCD = tbl.Rows[0]["mnthcd"].ToString();
                TCH.DOCCD = tbl.Rows[0]["doccd"].ToString();
                TCH.DOCNO = tbl.Rows[0]["docno"].ToString();
                TCH.DOCDT = DOCDT;
                TCH.DOCONLYNO = tbl.Rows[0]["doconlyno"].ToString();
                TCH.DTAG = "E";
                TCH.EMD_NO = Convert.ToInt16(tbl.Rows[0]["emd_no"].ToString());
                TCH.VCHRNO = Convert.ToInt32(tbl.Rows[0]["vchrno"].ToString());
            }
            else if (ACTION == "V")
            {
                string sql = "update " + scm + ".t_cntrl_hdr set ";
                sql += "dtag='D', ";
                sql += "del_usr_id='" + CommVar.UserID() + "', ";
                sql += "del_usr_entdt=sysdate, ";
                sql += "del_usr_lip='" + GetIp() + "', ";
                sql += "del_usr_sip='" + GetStaticIp() + "', ";
                sql += "del_usr_mnm='" + DetermineCompName(GetIp()) + "' ";
                sql = sql + ", DEL_REM=" + MasterHelp.filc(auditrem);
                sql += " where autono=:autono ";

                DB.Database.ExecuteSqlCommand(sql, new OracleParameter("autono", OracleDbType.Varchar2, 30, AUTONO, ParameterDirection.Input));
            }
            else
            {
                TCH.AUTONO = AUTONO;
                TCH.COMPCD = COM;
                TCH.LOCCD = LOC;
                TCH.MODCD = modcd;
                TCH.CLCD = CommVar.ClientCode(UNQSNO);
                TCH.DOCCD = DCODE;
                TCH.DOCNO = DPATTERN;
                TCH.YR_CD = yrcd;
                TCH.VCHRNO = Convert.ToInt32(DNO);
                TCH.DOCONLYNO = DNO;
                TCH.MNTHCD = MCODE;
                TCH.DOCDT = DOCDT;
                TCH.CALAUTO = calauto;
                TCH.USR_ID = CommVar.UserID();
                TCH.USR_ENTDT = System.DateTime.Now;
                TCH.USR_LIP = GetIp();
                TCH.USR_SIP = GetStaticIp();
                TCH.USR_OS = null;
                TCH.USR_MNM = DetermineCompName(GetIp());
                TCH.DTAG = "";
                TCH.EMD_NO = 0;
                TCH.LM_REM = auditrem;
            }
            TCH.AUTONO = AUTONO;
            TCH.GLCD = glcd;
            TCH.SLCD = slcd;
            TCH.DOCAMT = docamt;

            return TCH;
        }
        public void SaveUploadDocumentTransaction(ImprovarDB DB, string schema, string CLCD, string EMD, List<UploadDOC> UploadDOC, string autono)
        {
            var UNQSNO = getQueryStringUNQSNO();
            DB.Database.ExecuteSqlCommand("delete from " + CommVar.CurSchema(UNQSNO) + ".t_cntrl_hdr_doc where autono=:autono ", new OracleParameter("autono", OracleDbType.Varchar2, 30, autono, ParameterDirection.Input));
            DB.Database.ExecuteSqlCommand("delete from " + CommVar.CurSchema(UNQSNO) + ".t_cntrl_hdr_doc_dtl where autono=:autono ", new OracleParameter("autono", OracleDbType.Varchar2, 30, autono, ParameterDirection.Input));
            int slno = 0; string sql = "";
            foreach (var ss in UploadDOC)
            {
                if (ss.DOC_FILE != null)
                {
                    sql = "INSERT INTO " + schema + ".t_cntrl_hdr_doc(EMD_NO,CLCD,AUTONO,SLNO,DOC_FLNAME,DOC_EXTN,DOC_CTG,DOC_DESC)"
                            + " VALUES(:EMD_NO,:CLCD,:AUTONO,:SLNO,:DOC_FLNAME,:DOC_EXTN,:DOC_CTG,:DOC_DESC)";
                    DB.Database.ExecuteSqlCommand(sql
                        , new OracleParameter("EMD_NO", OracleDbType.Int16, 4, EMD, ParameterDirection.Input)
                        , new OracleParameter("CLCD", OracleDbType.Varchar2, 4, CLCD, ParameterDirection.Input)
                        , new OracleParameter("AUTONO", OracleDbType.Varchar2, 30, autono, ParameterDirection.Input)
                        , new OracleParameter("SLNO", OracleDbType.Int16, 30, (++slno).ToString(), ParameterDirection.Input)
                        , new OracleParameter("DOC_FLNAME", OracleDbType.Varchar2, 100, ss.DOC_FILE, ParameterDirection.Input)
                        , new OracleParameter("DOC_EXTN", OracleDbType.Varchar2, 10, "NA", ParameterDirection.Input)
                        , new OracleParameter("DOC_CTG", OracleDbType.Varchar2, 15, ss.docID, ParameterDirection.Input)
                        , new OracleParameter("DOC_DESC", OracleDbType.Varchar2, 300, ss.DOC_DESC, ParameterDirection.Input));
                    long length = ss.DOC_FILE.Length;
                    long count = length / 2000;
                    int index = 0;
                    for (int i = 0; i <= count; i++)
                    {
                        string file = "";
                        if (length - index > 2000)
                        {
                            file = ss.DOC_FILE.Substring(index, 2000);
                        }
                        else
                        {
                            file = ss.DOC_FILE.Substring(index, ss.DOC_FILE.Length - index);
                        }
                        sql = " INSERT INTO " + schema + ".T_CNTRL_HDR_DOC_DTL(EMD_NO, CLCD, AUTONO, SLNO, RSLNO, DOC_STRING) VALUES(" + EMD + ",'" + CLCD + "', '" + autono + "', " + slno + "," + (i + 1) + ",'" + file + "')";
                        DB.Database.ExecuteSqlCommand(sql);
                        index += 2000;
                    }
                }
            }
        }
        public string getQueryStringUNQSNO()
        {
            try
            {
                if (!HttpContext.Current.Request.QueryString.AllKeys.Contains("US"))
                {
                    var PreviousUrl = HttpContext.Current.Request.UrlReferrer.AbsoluteUri;
                    var uri = new Uri(PreviousUrl);//Create Virtually Query String
                    var queryString = HttpUtility.ParseQueryString(uri.Query);
                    var UNIQUESESSIONNO = queryString.Get("US").ToString().Replace(" ", "+");
                    return Decrypt_URL(UNIQUESESSIONNO);
                }
                else
                {
                    var UNIQUESESSIONNO = HttpContext.Current.Request.QueryString["US"].ToString().Replace(" ", "+");
                    return Decrypt_URL(UNIQUESESSIONNO);
                }
            }
            catch
            {
            }
            return "";
        }
        public void ValidateMenuPermission(Permission VE)
        {
            var MENU_DETAILS = VE.MENU_DETAILS.Split('~');
            string Permidetail = MENU_DETAILS[7];
            foreach (char v in Permidetail)
            {
                if (v.ToString() == "A")
                {
                    VE.Add = "A";
                    VE.AddDay = 0;
                }
                else if (v.ToString() == "E")
                {
                    VE.Edit = "E";
                    VE.EditDay = MENU_DETAILS[5].retInt();
                }
                else if (v.ToString() == "D")
                {
                    VE.Delete = "D";
                    VE.DeleteDay = MENU_DETAILS[6].retInt();
                }
                else if (v.ToString() == "V")
                {
                    VE.View = "V";
                }
                else if (v.ToString() == "C")
                {
                    VE.Check = "C";
                }
            }

            //Modification Remarks Bind
            VE.ListAuditRemarks = Dropdown_AuditRemarks();
            //end Modification Remarks Bind

            VE.IsChecked = false;
            VE.Search_nav = true;
            VE.Audit_nav = true;
            VE.DefaultView = true;
            VE.ExitMode = 1;
        }
        public void SaveTextFile(string message)
        {
            try
            {
                var line = Environment.NewLine + Environment.NewLine;
                string filepath = Path.Combine(@"C:/IPSMART", "");
                filepath = filepath + "/" + "ERROR LOG " + DateTime.Today.ToString("yyyy-MM-dd") + ".txt";   //Text File Name
                if (!File.Exists(filepath))
                {
                    File.Create(filepath).Dispose();
                }
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(message);
                    sw.WriteLine("--------------------------------*End*--------------------------------------");
                    sw.WriteLine(line);
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception e)
            {
                e.ToString();
            }
        }

        public void SaveException(Exception ex, string message)
        {
            try
            {
                var UNQSNO = getQueryStringUNQSNO();
                var line = Environment.NewLine + Environment.NewLine;
                String ErrorlineNo = ex.StackTrace.ToString();
                String extype = ex.GetType().ToString() + "," + ex.GetType().Name.ToString();
                String Errormsg = ex.Message.ToString();
                string InnerException = "";
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.InnerException != null)
                    {
                        InnerException = ex.InnerException.InnerException.Message;
                    }
                    else
                    {
                        InnerException = ex.InnerException.Message;
                    }
                }
                string filepath = @"C:/IPSMART/ErrorLog/" + "ERROR LOG " + DateTime.Today.ToString("yyyy-MM-dd") + ".txt";   //Text File Name
                if (!Directory.Exists(Path.GetDirectoryName(filepath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filepath));
                }
                if (!File.Exists(filepath))
                {
                    File.Create(filepath).Dispose();
                }
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    string error = "Error Line No :" + " " + ErrorlineNo + line + "Error Message:" + " " + Errormsg + line + "Exception Type:" + " " + extype
                        + line + "InnerException :" + " " + InnerException + line + "User Define Message:" + message + line
                        + "User Id: " + CommVar.UserID() + " COMPCD:" + CommVar.Compcd(UNQSNO) + " LOCCD:" + CommVar.Loccd(UNQSNO) + " SCHEMA:" + CommVar.CurSchema(UNQSNO)
                        + line
                        + "Client Ipv4:" + GetStaticIp();
                    sw.WriteLine("-----------Exception Details on " + " " + DateTime.Now.ToString() + "-----------------");
                    sw.WriteLine(error);
                    sw.WriteLine("--------------------------------*End*------------------------------------------");
                    sw.WriteLine(line);
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception Ex)
            {
                Ex.ToString();
            }
        }
        public void Create_DOCCD(string UNQSNO, string module_code, string doccd)
        {
            try
            {
                MasterHelp masterHelp = new MasterHelp();
                string Schema = "";
                switch (module_code)
                {
                    case "I":
                        Schema = CommVar.InvSchema(UNQSNO); break;
                    case "F":
                        Schema = CommVar.FinSchema(UNQSNO); break;
                    case "S":
                        Schema = CommVar.SaleSchema(UNQSNO); break;
                    case "P":
                        Schema = CommVar.PaySchema(UNQSNO); break;
                }
                DataTable dt = masterHelp.SQLquery("select doctype from " + Schema + ".m_doctype where doccd='" + doccd + "'");
                if (dt.Rows.Count == 0)
                {
                    string doctype = masterHelp.SQLquery("select doctype from " + CommVar.CurSchema(UNQSNO) + ".m_doctype where doccd='" + doccd + "'").Rows[0]["doctype"].retStr();
                    dt = masterHelp.SQLquery("select dcd from " + Schema + ".m_dtype where dcd='" + doctype + "'");
                    if (dt.Rows.Count == 0)
                    {
                        masterHelp.SQLNonQuery("insert into " + Schema + ".m_dtype "
                      + " select dcd, dname, fin, menu_progcall, menu_para, flag1 from " + CommVar.CurSchema(UNQSNO) + ".m_dtype where dcd = '" + doctype + "'");
                    }
                    masterHelp.SQLNonQuery("insert into " + Schema + ".m_doctype"
                    + " select a.emd_no,a.clcd,a.dtag,a.ttag,a.doccd,a.docnm,a.frdt,a.todt,a.docprfx,a.doctype,a.docjnrl,a.docfoot,a.docprn,a.docnopattern,a.docnomaxlength,a.docnowozero,1"//M_AUTONO
                    + " ,a.pro,a.fdate,a.backdate,a.maindoccd,a.flag1"
                    + " from " + CommVar.CurSchema(UNQSNO) + ".m_doctype A  where doccd = '" + doccd + "'");
                }
            }
            catch (Exception ex)
            {
                SaveException(ex, "");
            }

        }
        public string CreateMenuUrl(string menu_progcall, string menu_doccd, string menu_para)
        {
            MasterHelpFa masterHelpFa = new MasterHelpFa();
            MasterHelp masterHelp = new MasterHelp();
            var UNQSNO = getQueryStringUNQSNO();
            bool masterdisable = false;
            bool trandisable = false;
            string sql = "";
            sql = "select masterdisable from  sys_cnfg";
            DataTable rsTmp = masterHelp.SQLquery(sql);
            if (rsTmp.Rows.Count > 0)
            {
                if (rsTmp.Rows[0]["masterdisable"].ToString() == "Y") masterdisable = true;
            }
            if (HttpContext.Current.Session["MIRROR_TAG"].ToString() == "Y")
            {
                trandisable = true;
            }
            string fld1 = "p.user_right,", fld2 = "";
            if (trandisable == true)
            {
                fld2 = " and (m.menu_type <> 'O' or menu_type is null) ";
            }
            if (masterdisable == true) { fld1 = "decode(m.menu_type,'M',trim(translate(p.USER_RIGHT,'AED','   ')),p.user_right) user_right,"; }
            if (trandisable == true) { fld1 = "decode(m.menu_type,'E',trim(translate(p.USER_RIGHT,'AED','   ')),p.user_right) user_right,"; }
            if (trandisable == true && masterdisable == true) { fld1 = "decode(m.menu_type,'M',trim(translate(p.USER_RIGHT,'AED','   ')),'E',trim(translate(p.USER_RIGHT,'AED','   ')),p.user_right) user_right,"; }

            sql = "Select m.MENU_ID,m.MENU_NAME,m.MENU_INDEX,m.MENU_PARENT_ID,m.menu_find_id,m.MENU_PROGCALL,"
               + " m.MENU_ID||'~'||m.MENU_INDEX||'~'||nvl(m.MENU_PROGCALL,'Notset')||'~'||m.menu_date_option||'~'||m.menu_type||'~'||p.E_DAY||'~'||p.D_DAY  as MENU_PERMISSIONID, ";
            sql += fld1;
            sql += "p.E_DAY,p.A_DAY,'Y' AS PERDOTNETMENU,'Y' AS ATVDOTNETMENU,p.D_DAY,m.MENU_ORDER_CODE,m.MENU_DOCCD,m.MENU_PARA  menu_para, ";
            sql += "(select case  when exists(select 1 from appl_menu_fav f where  m.menu_id=f.menu_id and m.menu_index=f.menu_index and m.module_code='"
                + CommVar.ModuleCode() + "' and user_id='" + CommVar.UserID() + "') then 'Y'  else 'N' end from dual) as menu_fav ";
            sql += " from " + CommVar.CurSchema(UNQSNO) + ".M_USR_ACS p , APPL_MENU m ";
            sql += " where p.MENU_NAME=m.MENU_ID and p.MENU_INDEX=m.MENU_INDEX and p.USER_ID='" + CommVar.UserID() + "' and m.module_code='" + CommVar.ModuleCode() + "' and upper(MENU_PROGCALL)='" + menu_progcall.ToUpper() + "' AND ";
            sql += "p.compcd = '" + CommVar.Compcd(UNQSNO) + "' and p.loccd = '" + CommVar.Loccd(UNQSNO) + "' and p.schema_name = '" + CommVar.CurSchema(UNQSNO) + "' " + fld2;
            if (menu_doccd != "") sql += "and M.MENU_DOCCD = '" + menu_doccd + "' ";
            if (menu_para != "") sql += "and M.MENU_PARA = '" + menu_para + "' ";

            DataTable DT = masterHelp.SQLquery(sql);
            if (DT.Rows.Count > 0)
            {
                var mnu = (from DataRow DR in DT.Rows
                           select new
                           {
                               MENU_PROGCALL = DR["MENU_PROGCALL"].retStr(),
                               MENU_PARA = DR["MENU_PARA"].retStr(),
                               menu_name = DR["menu_name"].retStr(),
                               MENU_DOCCD = DR["MENU_DOCCD"].retStr(),
                               user_right = DR["user_right"].retStr(),
                               MENU_PERMISSIONID = DR["MENU_PERMISSIONID"].retStr(),
                           }).FirstOrDefault();
                string enc_MENU_DETAIL = Encrypt_URL((mnu.MENU_PERMISSIONID + "~" + mnu.user_right));
                string enc_MENU_DOCCD = Encrypt_URL(mnu.MENU_DOCCD);
                string enc_MENU_PARA = Encrypt_URL(mnu.MENU_PARA);
                var URL = mnu.MENU_PROGCALL + "/" + mnu.MENU_PROGCALL + "/?op=V&MNUDET=" + enc_MENU_DETAIL + "&US=" + Encrypt_URL(UNQSNO) + "&DC=" + enc_MENU_DOCCD + "&MP=" + enc_MENU_PARA + "";
                return URL;
            }
            else
            {
                return "";
            }
        }
        public void DocumentAuthorisation_Save(string DOCCD, string AUTONO, int SLNO, double DOCAMT, short EMD_NO, string FLAG1, string AUTH_REM, byte PASS_LEVEL, string DOCPASSED)
        {
            //   T_TXNSTATUS
            //S   SMS
            //P   PRINT
            //K   CHECK
            //C   CANCEL
            //U   UNCHECK
            //A   AUTHORISES
            //N   UNAUTHORISES
            bool Approved = false, UnApproved = false, Cancel = false;
            if (DOCPASSED == "A")//Approved
            {
                Approved = true;
            }
            else if (DOCPASSED == "N")//UnApproved
            {
                UnApproved = true;
            }
            else if (DOCPASSED == "C")//Cancel
            {
                Cancel = true;
            }
            MasterHelp masterHelp = new MasterHelp();
            var UNQSNO = getQueryStringUNQSNO();
            OracleConnection OraCon = new OracleConnection(GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            string dbsql = "";
            string[] dbsql1;

            ImprovarDB DB1 = new ImprovarDB(GetConnectionString(), CommVar.CurSchema(UNQSNO));
            using (OracleTransaction OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    OraCmd.CommandText = "lock table " + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode"; OraCmd.ExecuteNonQuery();
                    string USER = HttpContext.Current.Session["UR_ID"].ToString();

                    string lastStatus = DB1.T_TXNSTATUS.Where(x => x.AUTONO == AUTONO).FirstOrDefault()?.STSTYPE;
                    T_TXNSTATUS TTXNSTATUS = new T_TXNSTATUS();
                    T_CNTRL_AUTH CNTRLAUTH = new T_CNTRL_AUTH();
                    if (Approved == true || UnApproved == true || Cancel == true)
                    {
                        if (UnApproved == true)
                        {
                            dbsql = masterHelp.TblUpdt("T_CNTRL_AUTH", AUTONO, "D");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        }
                        else
                        {
                            dbsql = masterHelp.TblUpdt("T_CNTRL_AUTH", AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        }
                        dbsql = masterHelp.TblUpdt("T_TXNSTATUS", AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        //T_TXNSTATUS

                        TTXNSTATUS.EMD_NO = Convert.ToInt16(EMD_NO);
                        TTXNSTATUS.CLCD = CommVar.ClientCode(UNQSNO);
                        TTXNSTATUS.AUTONO = AUTONO;
                        if (Approved == true)
                        {
                            TTXNSTATUS.STSTYPE = "A";
                        }
                        else if (UnApproved == true)
                        {
                            TTXNSTATUS.STSTYPE = "N";
                        }
                        else if (Cancel == true)
                        {
                            TTXNSTATUS.STSTYPE = "C";
                        }
                        TTXNSTATUS.FLAG1 = FLAG1;

                        TTXNSTATUS.USR_ID = USER;
                        TTXNSTATUS.USR_ENTDT = System.DateTime.Now;
                        TTXNSTATUS.USR_LIP = GetIp();
                        TTXNSTATUS.USR_SIP = GetIp();
                        TTXNSTATUS.USR_OS = null;
                        TTXNSTATUS.USR_MNM = DetermineCompName(GetIp());
                        TTXNSTATUS.STSREM = AUTH_REM;

                        dbsql = masterHelp.RetModeltoSql(TTXNSTATUS);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        //
                        if (UnApproved != true)
                        {
                            CNTRLAUTH.AUTONO = AUTONO;
                            CNTRLAUTH.SLNO = SLNO;
                            CNTRLAUTH.EMD_NO = EMD_NO;
                            CNTRLAUTH.CLCD = CommVar.ClientCode(UNQSNO);
                            CNTRLAUTH.PASS_LEVEL = PASS_LEVEL;
                            CNTRLAUTH.AUTH_REM = AUTH_REM;

                            if (Approved == true)
                            {
                                CNTRLAUTH.DOCPASSED = "Y";
                            }
                            else if (UnApproved == true)
                            {
                                CNTRLAUTH.DOCPASSED = "N";
                            }
                            else if (Cancel == true)
                            {
                                CNTRLAUTH.DOCPASSED = "C";
                            }

                            CNTRLAUTH.USR_ID = USER;
                            CNTRLAUTH.USR_ENTDT = System.DateTime.Now;
                            CNTRLAUTH.USR_LIP = GetIp();
                            CNTRLAUTH.USR_SIP = GetIp();
                            CNTRLAUTH.USR_OS = null;
                            CNTRLAUTH.USR_MNM = DetermineCompName(GetIp());

                            dbsql = masterHelp.RetModeltoSql(CNTRLAUTH);
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        }

                        dbsql = masterHelp.TblUpdt("T_CNTRL_DOC_PASS", AUTONO, "E", "", "pass_level = '" + PASS_LEVEL + "'");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    }
                    if (Approved == true || UnApproved == true)
                    {
                        if (lastStatus.retStr() == "A" && UnApproved == true)
                        {

                            var T_CNTRL_DOC_PASS_DATA = DB1.T_CNTRL_DOC_PASS.Where(x => x.AUTONO == AUTONO && x.PASS_LEVEL == PASS_LEVEL).ToList();
                            if (T_CNTRL_DOC_PASS_DATA.Count() == 0)
                            {
                                var TCDP_DATA = T_CONTROL_DOC_PASS(DOCCD, DOCAMT, EMD_NO, AUTONO, CommVar.CurSchema(UNQSNO));
                                if (TCDP_DATA.Item1.Count != 0)
                                {
                                    for (int tr = 0; tr <= TCDP_DATA.Item1.Count - 1; tr++)
                                    {
                                        dbsql = masterHelp.RetModeltoSql(TCDP_DATA.Item1[tr]);
                                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    }

                                }
                            }
                        }
                        else if (UnApproved == true)
                        {
                            dbsql = masterHelp.TblUpdt("T_CNTRL_AUTH", AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        }
                    }
                    else
                    {
                        T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                        if (Cancel == true)
                        {
                            TCH = T_CONTROL_HDR(AUTONO, CommVar.CurSchema(UNQSNO), AUTH_REM);
                        }
                        else
                        {
                            TCH = T_CONTROL_HDR(AUTONO, CommVar.CurSchema(UNQSNO));
                        }

                        dbsql = masterHelp.RetModeltoSql(TCH, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    }

                    OraTrans.Commit();
                    OraCon.Close();
                    OraCon.Dispose();
                }
                catch (Exception ex)
                {
                    OraTrans.Rollback();
                    OraCon.Close();
                    OraCon.Dispose();
                    SaveException(ex, "");
                }
            }

        }
        public Models.M_CNTRL_HDR_REM GetMasterReamrks(string schema, long AutoNO)
        {
            Improvar.Models.ImprovarDB DB = new Models.ImprovarDB(GetConnectionString(), schema);

            M_CNTRL_HDR_REM MCHREM = new M_CNTRL_HDR_REM();

            var REM = (from h in DB.M_CNTRL_HDR_REM where h.M_AUTONO == AutoNO select h).OrderBy(d => d.SLNO).ToList();

            var RE = REM.GroupBy(x => x.M_AUTONO).Select(x => new
            {
                MASTER_REM = string.Join("", x.Select(n => n.DOCREM))
            });
            foreach (var i in RE)
            {
                MCHREM.DOCREM = i.MASTER_REM.ToString();
            }
            return MCHREM;
        }
        public Tuple<List<M_CNTRL_HDR_REM>> SAVEMASTERREMARKS(M_CNTRL_HDR_REM MSTRHDRREM, long AUTONO, string CLCD, int? EMD)
        {

            List<M_CNTRL_HDR_REM> REM = new List<M_CNTRL_HDR_REM>();

            if (MSTRHDRREM.DOCREM.Length <= 1000)
            {
                M_CNTRL_HDR_REM REMARKS = new M_CNTRL_HDR_REM();
                REMARKS.M_AUTONO = AUTONO;
                REMARKS.CLCD = CLCD;
                REMARKS.EMD_NO = EMD;
                REMARKS.SLNO = 1;
                REMARKS.DOCREM = MSTRHDRREM.DOCREM;
                REM.Add(REMARKS);
            }
            else
            {
                long length = MSTRHDRREM.DOCREM.Length;
                long count = length / 1000;
                int index = 0;
                for (int i = 0; i <= count - 1; i++)
                {
                    M_CNTRL_HDR_REM REMARKS = new M_CNTRL_HDR_REM();
                    REMARKS.M_AUTONO = AUTONO;
                    REMARKS.CLCD = CLCD;
                    REMARKS.EMD_NO = EMD;
                    REMARKS.SLNO = Convert.ToInt16(i + 1);
                    REMARKS.DOCREM = MSTRHDRREM.DOCREM.Substring(index, 1000);
                    index += 1000;
                    REM.Add(REMARKS);
                }
                if (index < MSTRHDRREM.DOCREM.Length)
                {
                    int rest = MSTRHDRREM.DOCREM.Length - index;
                    M_CNTRL_HDR_REM REMARKS = new M_CNTRL_HDR_REM();
                    REMARKS.M_AUTONO = AUTONO;
                    REMARKS.CLCD = CLCD;
                    REMARKS.EMD_NO = EMD;
                    REMARKS.SLNO = Convert.ToInt16(count + 1);
                    REMARKS.DOCREM = MSTRHDRREM.DOCREM.Substring(index, rest);
                    index += rest;
                    REM.Add(REMARKS);
                }
            }
            var result = Tuple.Create(REM);
            return result;
        }
        public string SaveImage(string DBImgString, string ImgPath)
        {
            try
            {
                if (DBImgString != "" || DBImgString.IndexOf(',') > 1)
                {
                    DBImgString = DBImgString.Substring(DBImgString.IndexOf(',') + 1);
                }
                var sPath = ImgPath;
                //String path = @"c:/IPSMART";
                if (System.IO.File.Exists(sPath))
                {
                    try
                    {
                        System.IO.File.Delete(sPath);
                    }
                    catch (Exception ex)
                    {
                        SaveException(ex, "Delete problem");
                        return sPath;
                    }
                }
                //byte[] imageBytes = Encoding.ASCII.GetBytes(DBImgString);
                DBImgString = DBImgString.Replace(" ", "+");
                byte[] imageBytes = Convert.FromBase64String(DBImgString);
                File.WriteAllBytes(sPath, imageBytes);
                return sPath;
            }
            catch (Exception ex)
            {
                SaveException(ex, "");
                return "";
            }
        }
        //public dynamic GenerateBarcode(string Barcodestr, string Rettype, bool IncludeLabel, string Savepath = "")
        //{// Rettype=string/image
        //    try
        //    {//Install-Package BarcodeLib -Version 1.3.0
        //        int width = 600;
        //        int Height = 200; int FontSize = 30;
        //        using (Barcode barcode = new Barcode())
        //        {
        //            barcode.IncludeLabel = IncludeLabel;
        //            barcode.Alignment = AlignmentPositions.LEFT;
        //            barcode.LabelFont = new Font(FontFamily.GenericSansSerif, FontSize, FontStyle.Regular);
        //            var barcodeImage = barcode.Encode(TYPE.CODE128, Barcodestr, Color.Black, Color.White);
        //            if (Rettype == "byte")
        //            {
        //                using (MemoryStream ms = new MemoryStream())
        //                {
        //                    barcodeImage.Save(ms, ImageFormat.Jpeg);
        //                    byte[] imageBytes = ms.ToArray();
        //                    return imageBytes;
        //                }
        //            }
        //            //return barcodeImage;
        //            if (Savepath == "")
        //                Savepath = "c:/IPSMART/Barcode/" + Barcodestr + ".jpeg";
        //            if (!Directory.Exists(Path.GetDirectoryName(Savepath)))
        //            {
        //                Directory.CreateDirectory(Path.GetDirectoryName(Savepath));
        //            }
        //            if (System.IO.File.Exists(Barcodestr))
        //            {
        //                System.IO.File.Delete(Barcodestr);
        //                GC.Collect();
        //            }
        //            barcodeImage.Save(Savepath, ImageFormat.Jpeg);
        //        }
        //        return Barcodestr;
        //    }
        //    catch (Exception ex)
        //    {
        //        SaveException(ex, "");
        //        return ex.Message;
        //    }
        //}
        public dynamic GenerateBarcode(string Barcodestr, string Rettype, bool IncludeLabel, string Savepath = "")
        {// Rettype=string/image
            try
            {//Install-Package BarcodeLib -Version 1.3.0
                BarcodeLib.Barcode.CrystalReports.LinearCrystal barcode = new BarcodeLib.Barcode.CrystalReports.LinearCrystal();
                // Barcode settings
                barcode.Type = BarcodeLib.Barcode.BarcodeType.CODE128;
                barcode.BarHeight = 50; //50 pixel
                barcode.ShowText = false;
                barcode.ImageFormat = System.Drawing.Imaging.ImageFormat.Png;
                barcode.Data = Barcodestr;
                byte[] imageData = barcode.drawBarcodeAsBytes();
                return imageData;
                //int width = 600;
                //int Height = 200; int FontSize = 30;
                //using (Barcode barcode = new Barcode())
                //{
                //    barcode.IncludeLabel = IncludeLabel;
                //    barcode.Alignment = AlignmentPositions.LEFT;
                //    barcode.LabelFont = new Font(FontFamily.GenericSansSerif, FontSize, FontStyle.Regular);
                //    var barcodeImage = barcode.Encode(TYPE.CODE128, Barcodestr, Color.Black, Color.White);
                //    if (Rettype == "byte")
                //    {
                //        using (MemoryStream ms = new MemoryStream())
                //        {
                //            barcodeImage.Save(ms, ImageFormat.Jpeg);
                //            byte[] imageBytes = ms.ToArray();
                //            return imageBytes;
                //        }
                //    }
                //    //return barcodeImage;
                //    if (Savepath == "")
                //        Savepath = "c:/IPSMART/Barcode/" + Barcodestr + ".jpeg";
                //    if (!Directory.Exists(Path.GetDirectoryName(Savepath)))
                //    {
                //        Directory.CreateDirectory(Path.GetDirectoryName(Savepath));
                //    }
                //    if (System.IO.File.Exists(Barcodestr))
                //    {
                //        System.IO.File.Delete(Barcodestr);
                //        GC.Collect();
                //    }
                //    barcodeImage.Save(Savepath, ImageFormat.Jpeg);
                //}
                return Barcodestr;
            }
            catch (Exception ex)
            {
                SaveException(ex, "");
                return ex.Message;
            }
        }

        public string CopyImage(string copyFromPath, string copyToPath)
        {
            try
            {
                if (System.IO.File.Exists(copyToPath))
                {
                    System.IO.File.Delete(copyToPath);
                }
                File.Copy(copyFromPath, copyToPath);
                return copyToPath;
            }
            catch (Exception ex)
            {
                SaveException(ex, copyToPath);
                return "";
            }
        }
        public dynamic GenerateQRcode(string QRtext, string Rettype, string Savepath = "")
        {
            try
            {//Install-Package QRCoder -Version 1.4.1
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(QRtext, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(20);
                qrCodeImage = new Bitmap(qrCodeImage, new Size(qrCodeImage.Width / 5, qrCodeImage.Height / 5));
                if (Rettype == "byte")
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (MemoryStream stream = new MemoryStream())
                        {
                            qrCodeImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                            return stream.ToArray();
                        }
                    }
                }
                //return barcodeImage;
                if (Savepath == "")
                    Savepath = "C:/IPSMART/Qrcode/" + QRtext + ".jpeg";
                if (!Directory.Exists(Path.GetDirectoryName(Savepath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(Savepath));
                }
                if (System.IO.File.Exists(QRtext))
                {
                    System.IO.File.Delete(QRtext);
                    GC.Collect();
                }
                qrCodeImage.Save(Savepath, System.Drawing.Imaging.ImageFormat.Jpeg);
                return Savepath;
            }
            catch (Exception ex)
            {
                SaveException(ex, "");
                return ex.Message;
            }
        }
        public dynamic GetOsDate(string BoardCode, string compcd)
        {
            var UNQSNO = getQueryStringUNQSNO();
            MasterHelp masterHelp = new MasterHelp();
            string sql = "";
            string DB = CommVar.CurSchema(UNQSNO);
            string DBF = CommVar.FinSchema(UNQSNO);
            EMAILSTATUSTBL EMAILSTATUSTBL = new EMAILSTATUSTBL();
            if (BoardCode == "FABOARD4")
            {
                sql += "select usr_entdt, usr_id, autono from( ";
                sql += Environment.NewLine + "select a.usr_entdt, a.usr_id, a.autono, ";
                sql += Environment.NewLine + "row_number() over(partition by a.usr_entdt order by a.usr_entdt desc) as rn ";
                sql += Environment.NewLine + "from " + DBF + ".t_txnstatus a ";
                sql += Environment.NewLine + "where a.autono like '" + compcd + "%' and a.flag1='AOSRM') where rn = 1 ";

                DataTable tbl = masterHelp.SQLquery(sql);

                int maxr = tbl.Rows.Count;


                if (tbl == null || tbl.Rows.Count == 0)
                {
                    EMAILSTATUSTBL.curdt = System.DateTime.Now.Date.retDateStr();
                }
                else
                {
                    for (int i = 0; i < maxr; i++)
                    {
                        //compcd+|current date (ddmmyy) | skip party check box | DBL | grace days | hh(24 format)|0001..
                        var arrautono = tbl.Rows[i]["autono"].retStr().Split('|');

                        EMAILSTATUSTBL.curdt = System.DateTime.Now.Date.retDateStr();
                        EMAILSTATUSTBL.autoreminderofff = arrautono[2].retStr() == "Y" ? true : false;
                        EMAILSTATUSTBL.caldt = arrautono[3];
                        EMAILSTATUSTBL.graceday = arrautono[4];
                        EMAILSTATUSTBL.luserid = tbl.Rows[i]["usr_id"].ToString();
                        EMAILSTATUSTBL.luserentdt = "[" + tbl.Rows[i]["usr_entdt"].ToString() + "]";
                    }
                }

            }
            return EMAILSTATUSTBL;
        }
        public DataTable GetDashboardTable(string BoardCode, string compcd)
        {
            DataTable IR = new DataTable();
            MasterHelp masterHelp = new MasterHelp();
            string sql = "";
            var UNQSNO = getQueryStringUNQSNO();
            string DB = CommVar.CurSchema(UNQSNO);
            string DBF = CommVar.FinSchema(UNQSNO);
            //sql += " select distinct a.compcd||a.loccd
            //sql += " from sd_hpc2021.m_usr_acs a, appl_menu b
            //sql += " where a.menu_name=b.menu_id(+) and a.menu_index=b.menu_index(+) and a.user_id='IPSTEAM' and
            //sql += " b.menu_progcall='DB_SALCASU_PND_SLIP'
            sql = "";
            if (BoardCode == "DB_SALCASU_PND_SLIP")
            {
                //  sql += " select a.autono, c.compcd, c.loccd, c.docno, c.docdt, a.slcd, d.slnm, nvl(d.slarea, d.district) slarea, nvl(b.qnty, 0) qnty from";
                sql += " select c.docno, TO_CHAR(c.docdt,'dd/mm/yyyy') docdt ,  d.slnm, nvl(d.slarea, d.district) slarea,b.pcsperbox, nvl(b.qnty, 0) qntypcs, f.slnm trslnm from";
                sql += Environment.NewLine + " (select a.autono, a.slcd";
                sql += Environment.NewLine + " from " + DB + ".t_pslip a, " + DB + ".t_cntrl_hdr b, " + DB + ".t_txn_linkno c";
                sql += Environment.NewLine + " where a.autono = b.autono and a.autono = c.linkautono(+) and c.autono is null";
                sql += Environment.NewLine + " ";
                sql += Environment.NewLine + " and b.compcd || b.loccd in (";
                sql += Environment.NewLine + " select distinct a.compcd || a.loccd as comploccd";
                sql += Environment.NewLine + " from " + DB + ".m_usr_acs a, appl_menu b";
                sql += Environment.NewLine + " where a.menu_name = b.menu_id(+) and a.menu_index = b.menu_index(+) and a.user_id = '" + CommVar.UserID() + "' and";
                sql += Environment.NewLine + " b.menu_progcall = 'DB_SALCASU_PND_SLIP'";
                sql += Environment.NewLine + " ";
                sql += Environment.NewLine + " ) ";
                sql += Environment.NewLine + " ";
                sql += Environment.NewLine + " and nvl(b.cancel, 'N')= 'N' ) a,";
                sql += Environment.NewLine + " ";
                sql += Environment.NewLine + " (select a.autono, max(nvl(c.trslcd,c.crslcd)) trslcd, max(a.ordautono) ordautono,b.pcsperbox, sum(a.qnty) qnty";
                sql += Environment.NewLine + " from " + DB + ".t_pslipdtl a, " + DB + ".m_sitem b, " + DB + ".t_sord c";
                sql += Environment.NewLine + " where a.itcd = b.itcd(+) and a.ordautono=c.autono(+)";
                sql += Environment.NewLine + " group by a.autono,b.pcsperbox ) b,";
                sql += Environment.NewLine + " ";
                sql += Environment.NewLine + " " + DB + ".t_cntrl_hdr c, " + DBF + ".m_subleg d, " + DBF + ".m_subleg f";
                sql += Environment.NewLine + " ";
                sql += Environment.NewLine + " where a.autono = b.autono(+) and a.slcd = d.slcd(+) and a.autono = c.autono and ";
                if (compcd != "") sql += " c.compcd='" + compcd + "' and ";
                sql += Environment.NewLine + " b.trslcd=f.slcd(+)";
                sql += Environment.NewLine + " order by compcd, docdt, docno";

                IR.Columns.Add("Docno", typeof(string));
                IR.Columns.Add("Docdt", typeof(string));
                IR.Columns.Add("Slnm", typeof(string));
                IR.Columns.Add("Slarea", typeof(string));
                IR.Columns.Add("QntyBox", typeof(string));
                IR.Columns.Add("Trslnm", typeof(string));

                DataTable tbl = masterHelp.SQLquery(sql); string nextdocno = ""; double box = 0;
                int maxr = tbl.Rows.Count;
                for (int i = 0; i < maxr; i++)
                {
                    nextdocno = "";
                    string docno = tbl.Rows[i]["docno"].ToString();
                    string docdt = tbl.Rows[i]["docno"].ToString();
                    double qntypcs = tbl.Rows[i]["qntypcs"].retDbl();
                    double pcsperbox = tbl.Rows[i]["pcsperbox"].retDbl();
                    double pcstoBox = 0;
                    box += pcstoBox;
                    if (i < maxr - 1) { nextdocno = tbl.Rows[i + 1]["docno"].ToString(); }
                    if (nextdocno == docno)
                    {
                    }
                    else
                    {
                        DataRow dr = IR.NewRow();
                        dr["Docno"] = docno;
                        dr["Docdt"] = docdt;
                        dr["Slnm"] = tbl.Rows[i]["Slnm"].ToString();
                        dr["Slarea"] = tbl.Rows[i]["Slarea"].ToString();
                        dr["QntyBox"] = box.ToString();
                        dr["trslnm"] = tbl.Rows[i]["trslnm"].ToString();
                        IR.Rows.Add(dr);
                        box = 0;
                    }
                }
            }
            return IR;
        }
        public List<Dropdown_AuditRemarks> Dropdown_AuditRemarks()
        {
            List<Dropdown_AuditRemarks> DDL = new List<Dropdown_AuditRemarks>();
            Dropdown_AuditRemarks DDL2 = new Dropdown_AuditRemarks();
            DDL2.text = "1. Scheme/Rate Difference";
            DDL2.value = "1. Scheme/Rate Difference";
            DDL.Add(DDL2);
            Dropdown_AuditRemarks DDL3 = new Dropdown_AuditRemarks();
            DDL3.text = "2. Change in Item/Quantity";
            DDL3.value = "2. Change in Item/Quantity";
            DDL.Add(DDL3);
            Dropdown_AuditRemarks DDL1 = new Dropdown_AuditRemarks();
            DDL1.text = "3. Billwise Adjustment";
            DDL1.value = "3. Billwise Adjustment";
            DDL.Add(DDL1);
            Dropdown_AuditRemarks DDL5 = new Dropdown_AuditRemarks();
            DDL5.text = "4. Account Head Corrections";
            DDL5.value = "4. Account Head Corrections";
            DDL.Add(DDL5);
            Dropdown_AuditRemarks DDL4 = new Dropdown_AuditRemarks();
            DDL4.text = "0. Any Other";
            DDL4.value = "0. Any Other";
            DDL.Add(DDL4);
            return DDL;
        }
    }
}
