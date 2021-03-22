using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Improvar.Models;
using System.Web.Configuration;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using Microsoft.VisualBasic;
using Improvar.ViewModels;
using System.Net;
using System.IO;

namespace Improvar.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        string CS = null;
        int Attem = 0;
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        SMS sms = new SMS();
        EmailControl EmailControl = new EmailControl();
        public ActionResult Datelock(string str = "")
        {
            ViewBag.Title = str;
            return View();
        }
        public ActionResult Login()
        {
            Connection Cn = new Connection();
            //Session.Clear();
            DateTime Current;
            try
            {
                CS = Cn.SetConnectionString();
                if (CS.Length == 0)
                {
                    Session.Add("Flag", "0");
                    Response.BufferOutput = true;
                    return RedirectToAction("ServerConfigaration");
                }
                else
                {
                    if (WebConfigurationManager.ConnectionStrings["local"] == null)
                    {
                        Configuration config = WebConfigurationManager.OpenWebConfiguration(System.Web.HttpContext.Current.Request.ApplicationPath);
                        ConnectionStringSettings ccs = new ConnectionStringSettings();
                        ccs.Name = "local";
                        ccs.ConnectionString = CS;
                        config.ConnectionStrings.ConnectionStrings.Add(ccs);
                        config.Save();
                    }
                    else
                    {
                        CS = Cn.GetConnectionString();
                    }
                    // =========================== Application Validation Part ===================================================
                    Cn.con = new OracleConnection(CS);

                    if (Cn.con.State == ConnectionState.Closed)
                    {
                        Cn.con.Open();
                    }
                    Cn.com = new OracleCommand("select datelock,sysdate from Signature", Cn.con);
                    Cn.dr = Cn.com.ExecuteReader();
                    Cn.dr.Read();
                    string date = Cn.dr[1].ToString();
                    string datelock = Cn.dr[0].ToString();
                    string[] Retrive = Cn.Decrypt(datelock).Split(',');
                    Cn.dr.Close();
                    Cn.con.Close();
                    DateTime LastAccess = new DateTime(Convert.ToInt32(Retrive[0]), Convert.ToInt32(Retrive[1]), Convert.ToInt32(Retrive[2]));
                    Current = Convert.ToDateTime(date);
                    //if (Current >= LastAccess)
                    //{
                    //    if (MyGlobal.ValidDateForPackage < Current.Date)
                    //    {
                    //        return RedirectToAction("Datelock", "Login", new { str = "Your Package Has Expired!" });
                    //    }
                    //}
                    //else
                    //{
                    //    return RedirectToAction("Datelock", "Login", new { str = "Your Date Setting Not Correct!" });
                    //}
                }
                string Savedate = Current.Year.ToString() + "," + Current.Month + "," + Current.Day;
                if (Cn.con.State == ConnectionState.Closed)
                {
                    Cn.con.Open();
                }
                Cn.com = new OracleCommand("update Signature set datelock='" + Cn.Encrypt(Savedate) + "'", Cn.con);
                Cn.com.ExecuteNonQuery();
                Cn.con.Close();
                var ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                Session.Add("publishversion", ver.ToString());

                Login log = new Login();
                if (Request.Cookies["UserName"] != null && Request.Cookies["Password"] != null)
                {
                    log.UserName = Request.Cookies["UserName"].Value;
                    log.Password = Request.Cookies["Password"].Value;
                    log.REMEMBERME = Convert.ToBoolean(Request.Cookies["Rememberme"].Value);
                }
                return View(log);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Datelock", "Login", new { str = ex.Message });
            }
        }
        [HttpPost]
        public ActionResult Login(Login log, FormCollection FC)
        {
            try
            {
                Connection Cn = new Connection();
                string SCHEMA = Cn.Getschema;
                CS = Cn.GetConnectionString();
                Cn.con = new OracleConnection(CS);
                string MWORD = "";
                string PWD = "";
                string userType = "";
                if (Cn.con.State == ConnectionState.Closed)
                {
                    Cn.con.Open();
                }
                Cn.com = new OracleCommand("select pswrd, user_type from " + SCHEMA + ".user_appl where user_id='" + log.UserName + "'", Cn.con);
                Cn.dr = Cn.com.ExecuteReader();
                if (Cn.dr.HasRows)
                {
                    Cn.dr.Read();
                    string XACSTR = Cn.dr[0].ToString();
                    userType = Cn.dr[1].ToString();
                    Cn.dr.Close();
                    MWORD = Cn.Decrypt(XACSTR);

                    Session["clientip"] = log.IP;
                    string user = log.UserName + "56";
                    string sub = MWORD.Substring(user.Length);
                    MWORD = sub;
                    if (MWORD.Trim() != log.Password)
                    {
                        PWD = "NO";
                    }
                    else
                    {
                        PWD = "YES";
                    }
                }
                Cn.com = new OracleCommand(SCHEMA + ".SP_USER_VALID", Cn.con);
                Cn.com.CommandType = CommandType.StoredProcedure;
                Cn.com.Parameters.Add("USR", log.UserName);
                Cn.com.Parameters.Add("PWD", PWD);
                Cn.com.Parameters.Add("ACS", Module.Module_Code);
                Cn.com.Parameters.Add("ATM", Attem);
                Cn.com.Parameters.Add("USR_IP", Cn.GetIp());
                Cn.com.Parameters.Add("USR_SIP", Cn.GetStaticIp());
                OracleParameter pm = new OracleParameter("C1", OracleDbType.Int32);
                pm.Direction = ParameterDirection.Output;
                Cn.com.Parameters.Add(pm);
                OracleParameter pm1 = new OracleParameter("USER_NAME", OracleDbType.Varchar2);
                pm1.Direction = ParameterDirection.Output;
                pm1.Size = 100;
                Cn.com.Parameters.Add(pm1);
                OracleParameter pm2 = new OracleParameter("C3", OracleDbType.Varchar2);
                pm2.Size = 100;
                pm2.Direction = ParameterDirection.Output;
                Cn.com.Parameters.Add(pm2);
                OracleParameter pm3 = new OracleParameter("CR", OracleDbType.Varchar2);
                pm3.Direction = ParameterDirection.Output;
                pm3.Size = 100;
                Cn.com.Parameters.Add(pm3);
                OracleParameter pm4 = new OracleParameter("TIME_OUT", OracleDbType.Int32);
                pm4.Direction = ParameterDirection.Output;
                Cn.com.Parameters.Add(pm4);
                OracleParameter pm5 = new OracleParameter("REMOT_CONFIG", OracleDbType.Varchar2);
                pm5.Direction = ParameterDirection.Output;
                pm5.Size = 1;
                Cn.com.Parameters.Add(pm5);
                OracleParameter pm6 = new OracleParameter("REM1", OracleDbType.Varchar2);
                pm6.Direction = ParameterDirection.Output;
                pm6.Size = 500;
                Cn.com.Parameters.Add(pm6);
                Cn.com.ExecuteNonQuery();
                Cn.con.Close();
                string outp = Cn.com.Parameters["C1"].Value.ToString();
                string msg = Cn.com.Parameters["CR"].Value.ToString();
                string USER_NAME = Cn.com.Parameters["USER_NAME"].Value.ToString();
                string[] session_no = Cn.com.Parameters["CR"].Value.ToString().Split(',');
                string dt = Cn.com.Parameters["C3"].Value.ToString();
                Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
                Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
                Response.Cache.SetNoStore();
                string WebHostPath = System.Web.HttpContext.Current.Request.Url?.Scheme + Uri.SchemeDelimiter + System.Web.HttpContext.Current.Request.Url?.Host + ":" + System.Web.HttpContext.Current.Request.UrlReferrer?.Port.retStr() + System.Web.HttpContext.Current.Request.Url?.LocalPath;
                Session.Add("WebHostPath", WebHostPath);
                if (outp == "0")
                {
                    bool validIP = Cn.LoginIPValidate(log.UserName);
                    if (validIP == false)
                    {
                        msg = "You are not authorised to login from IP "+ Cn.GetStaticIp() + "... Contact System Administrator...";
                        ViewBag.Msg = msg;
                        ModelState.Clear();
                    }
                    else
                    {
                        Session.Add("UR_NAME", USER_NAME);
                        Session.Add("UR_ID", log.UserName);
                        Session.Add("LST_LOG_DT", dt);
                        Session.Add("Session_No", session_no[1]);
                        Session.Add("USER_TYPE", userType);
                        if (log.REMEMBERME)
                        {
                            Response.Cookies["UserName"].Expires = DateTime.Now.AddDays(30);
                            Response.Cookies["Password"].Expires = DateTime.Now.AddDays(30);
                            Response.Cookies["Rememberme"].Expires = DateTime.Now.AddDays(30);
                        }
                        else
                        {
                            Response.Cookies["UserName"].Expires = DateTime.Now.AddDays(-1);
                            Response.Cookies["Password"].Expires = DateTime.Now.AddDays(-1);
                            Response.Cookies["Rememberme"].Expires = DateTime.Now.AddDays(-1);
                        }
                        Response.Cookies["UserName"].Value = log.UserName.Trim();
                        Response.Cookies["Password"].Value = log.Password.Trim();
                        Response.Cookies["Rememberme"].Value = Convert.ToString(log.REMEMBERME);
                        return RedirectToAction("CompanySelection", "Home");
                    }
                }
                else if (outp == "2")
                {
                    Session.Add("UR_ID", log.UserName);
                    ViewBag.Msg = msg;
                }
                else
                {
                    ViewBag.Msg = msg;
                    ModelState.Clear();
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                ViewBag.Msg = ex.Message.ToString();
            }
            return View(log);
        }
     
        public ActionResult ChangePassword(string USERID)
        {
            Password pass = new Password();
            pass.UserName = USERID;
            string sql = "select * from ipsmart_policy where rownum=1";
            DataTable dt = masterHelp.SQLquery(sql);
            var ipsmart_policy = (from DataRow dr in dt.Rows
                      select new
                      {
                          NOOFTXTCHAR = dr["NOOFTXTCHAR"],
                          NOOFLOWERCHAR = dr["NOOFLOWERCHAR"],
                          NOOFUPPERCHAR = dr["NOOFUPPERCHAR"],
                          NOOFSPCHAR = dr["NOOFSPCHAR"],
                          NOOFNUMCHAR = dr["NOOFNUMCHAR"],
                          MINPWDLENGTH = dr["MINPWDLENGTH"],
                          MAXPWDLENGTH = dr["MAXPWDLENGTH"],
                      }).FirstOrDefault();
            if (ipsmart_policy != null)
            {
                pass.NOOFTXTCHAR = ipsmart_policy.NOOFTXTCHAR.retInt();
                pass.NOOFLOWERCHAR = ipsmart_policy.NOOFLOWERCHAR.retInt();
                pass.NOOFUPPERCHAR = ipsmart_policy.NOOFUPPERCHAR.retInt();
                pass.NOOFSPCHAR = ipsmart_policy.NOOFSPCHAR.retInt();
                pass.NOOFNUMCHAR = ipsmart_policy.NOOFNUMCHAR.retInt();
                pass.MINPWDLENGTH = ipsmart_policy.MINPWDLENGTH.retInt();
                pass.MAXPWDLENGTH = ipsmart_policy.MAXPWDLENGTH.retInt();
            }
            return View(pass);
        }
        [HttpPost]
        public ActionResult ChangePassword(Password Pass)
        {
            try
            {
                using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CommSchema()))
                {
                    USER_APPL userdata = DB.USER_APPL.Where(s => s.USER_ID.ToUpper() == Pass.UserName.ToUpper()).FirstOrDefault();
                    if (userdata == null)
                    {
                        Pass.UserName = "";
                        ViewBag.Msg = "user id not found";
                        return View(Pass);
                    }
                    userdata = DB.USER_APPL.Where(s => s.USER_ID.ToUpper() == Pass.UserName.ToUpper() && s.ACTIVE_TAG != "N").FirstOrDefault();
                    if (userdata == null)
                    {
                        ViewBag.Msg = "user id not activated.Please Contact System Administrator";
                        return View(Pass);
                    }
                }
                string SCHEMA = Cn.Getschema;
                CS = Cn.GetConnectionString();
                Cn.con = new OracleConnection(CS);
                string Userid = Pass.UserName;
                double[] RAND1 = new double[10];
                string MWORD = "";
                double SEED = 0;
                double SEED1 = 0;
                string outp = "";
                string msg = "";
                if (Pass.CurrentPassword == Pass.NewPassword || Pass.CurrentPassword == Pass.ConfirmPassword)
                {
                    outp = "1";
                    msg = "Same Password as Before";
                    ViewBag.Msg = msg;
                }
                else
                {
                    if (Pass.NewPassword != Pass.ConfirmPassword)
                    {
                        msg = "New and Confirm Password does not match";
                        ViewBag.Msg = msg;
                        outp = "2";
                    }
                    else
                    {
                        #region Decrypt
                        if (Cn.con.State == ConnectionState.Closed)
                        {
                            Cn.con.Open();
                        }
                        Cn.com = new OracleCommand("select pswrd from " + SCHEMA + ".User_appl where user_id='" + Userid + "'", Cn.con);
                        Cn.dr = Cn.com.ExecuteReader();
                        if (Cn.dr.HasRows)
                        {
                            Cn.dr.Read();
                            string Password = Cn.dr[0].ToString();
                            string XACSTR = Cn.dr[0].ToString();
                            Cn.dr.Close();
                            MWORD = Cn.Decrypt(XACSTR);
                        }
                        Cn.con.Close();
                        string user = Userid + "56";
                        MWORD = MWORD.Substring(user.Length);
                        #endregion
                        if (MWORD.Trim() != Pass.CurrentPassword)
                        {
                            outp = "9";
                            msg = "Current password not matched...";
                        }
                        else
                        {
                            string[] pwdchk = Cn.passwordcheckfrompolicy(Pass.UserName, Pass.NewPassword).Split(Convert.ToChar(Cn.GCS()));
                            outp = pwdchk[0];
                            if (pwdchk.Count() > 1) msg = pwdchk[1];
                        }
                        if (outp == "0")
                        {
                            string MNWORD = "";
                            string usernm = Userid + "56";
                            string newpd = usernm + Pass.NewPassword;
                            MNWORD = Cn.Encrypt(newpd);
                            string oldpd = usernm + Pass.CurrentPassword;
                            OracleTransaction OT;
                            if (Cn.con.State == ConnectionState.Closed)
                            {
                                Cn.con.Open();
                            }
                            OT = Cn.con.BeginTransaction(IsolationLevel.ReadCommitted);
                            Cn.com.Transaction = OT;
                            string str = "insert into " + SCHEMA + ".pswd_history (USER_ID, OLD_PSWRD, NEW_PSWRD,LASTPWDCHANGED,CURRPWDCHANGED, USR_ENTDT,USR_ID) ";
                            str += "select user_id, pswrd,'" + MNWORD + "', lastpwdchanged, sysdate, sysdate, '" + Userid + "' ";
                            str += "from " + SCHEMA + ".user_appl where user_id='" + Userid + "' ";
                            Cn.com = new OracleCommand(str, Cn.con);
                            Cn.com.ExecuteNonQuery();
                            str = "UPDATE " + SCHEMA + ".USER_APPL SET PSWRD='" + MNWORD + "', U_ENTDATE_NEW = SYSDATE,LAST_LOGGED = SYSDATE, ";
                            str += "TIMES_LOOGED = NVL(TIMES_LOOGED,0) + 1, TIMES_PSSW_CHANGED = NVL(TIMES_PSSW_CHANGED,0) + 1, LASTPWDCHANGED=SYSDATE ";
                            str += "WHERE USER_ID ='" + Userid + "'";
                            Cn.com = new OracleCommand(str, Cn.con);
                            Cn.com.ExecuteNonQuery();
                            OT.Commit();
                            Cn.con.Close();
                            outp = "5";
                            msg = "Password Changed Successfully";
                            ViewBag.MessageCS = msg; msg = "";
                        }
                    }
                }
                if (outp == "0")
                {
                    ViewBag.FlagCS = "0";
                }
                else if (outp == "5")
                {
                    ViewBag.FlagCS = "1";
                }
                else
                {
                    ViewBag.FlagCS = "0";
                }
                ViewBag.Msg = msg;
            }
            catch (Exception ex)
            {
                ViewBag.FlagCS = "0";
                ViewBag.Msg = ex.Message;
            }
            return View(Pass);
        }
        public ActionResult ForgotPassword(string USERID)
        {
            Password pass = new Password();
            pass.UserName = USERID;
            string sql = "select * from ipsmart_policy where rownum=1";
            DataTable dt = masterHelp.SQLquery(sql);
            var ipsmart_policy = (from DataRow dr in dt.Rows
                                  select new
                                  {
                                      NOOFTXTCHAR = dr["NOOFTXTCHAR"],
                                      NOOFLOWERCHAR = dr["NOOFLOWERCHAR"],
                                      NOOFUPPERCHAR = dr["NOOFUPPERCHAR"],
                                      NOOFSPCHAR = dr["NOOFSPCHAR"],
                                      NOOFNUMCHAR = dr["NOOFNUMCHAR"],
                                      MINPWDLENGTH = dr["MINPWDLENGTH"],
                                      MAXPWDLENGTH = dr["MAXPWDLENGTH"],
                                  }).FirstOrDefault();
            if (ipsmart_policy != null)
            {
                pass.NOOFTXTCHAR = ipsmart_policy.NOOFTXTCHAR.retInt();
                pass.NOOFLOWERCHAR = ipsmart_policy.NOOFLOWERCHAR.retInt();
                pass.NOOFUPPERCHAR = ipsmart_policy.NOOFUPPERCHAR.retInt();
                pass.NOOFSPCHAR = ipsmart_policy.NOOFSPCHAR.retInt();
                pass.NOOFNUMCHAR = ipsmart_policy.NOOFNUMCHAR.retInt();
                pass.MINPWDLENGTH = ipsmart_policy.MINPWDLENGTH.retInt();
                pass.MAXPWDLENGTH = ipsmart_policy.MAXPWDLENGTH.retInt();
            }
            return View(pass);
        }
        public ActionResult GetOtp(string USERID)
        {
            using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CommSchema()))
            {
                USER_APPL userdata = DB.USER_APPL.Where(s => s.USER_ID.ToUpper() == USERID.ToUpper()).FirstOrDefault();
                if (userdata == null)
                {
                    return Content("user id not found");
                }
                userdata = DB.USER_APPL.Where(s => s.USER_ID.ToUpper() == USERID.ToUpper() && s.ACTIVE_TAG != "N").FirstOrDefault();
                if (userdata == null)
                {
                    return Content("User Id not activated.Please Contact System Administrator.");
                }
                bool validIP = Cn.LoginIPValidate(USERID);
                if (validIP == false)
                {
                    return Content("You are not authorised to login from IP " + Cn.GetStaticIp() + "... Contact System Administrator...");
                }
                string otp = Cn.GenerateOTP(false, 6);
                // bool otpsend = false;
                string sql = "";

                sql = "insert into pssw_invalid (user_id, password, login_date, user_ip, user_static_ip) values (";
                sql += "'" + USERID + "','OTP',SYSDATE,'" + Cn.GetIp() + "','" + Cn.GetStaticIp() + "')";
                masterHelp.SQLNonQuery(sql);
                sql = "select count(*) treco from pssw_invalid where user_id='" + USERID + "' and password='OTP' and " ;
                sql += "to_char(login_date,'yyyy') = to_char(sysdate,'yyyy')";
                DataTable tblpw = masterHelp.SQLquery(sql);
                if (tblpw.Rows.Count > 0)
                {
                    if (Convert.ToDouble(tblpw.Rows[0]["treco"]) > 40)
                    {
                        return Content("You have tried maximum 40 attempt in OTP/Forgot Password..Contact Administrator");
                    }
                }
                string msg = "";
                string smsbody = otp + " is your OTP for forgot password on IPSMART ERP. Do not share with anyone.";
                if (string.IsNullOrEmpty(userdata.MOBILE))
                {
                    msg += " Mobile No Not Found. ";
                }
                else
                {
                    SMS smsobj = new SMS();
                    smsobj.SMSfromIpSmart(smsbody, userdata.MOBILE, "1707161155978940290");
                }
                if (string.IsNullOrEmpty(userdata.EMAIL))
                {
                    msg += " Email ID No Not Found. ";
                }           
     
                string body = "Hi " + userdata.USER_NAME + ",<br/><br/><b>" + otp + "</b> is your OTP for forgot password on IPSMART ERP. Do not share with anyone. <br/><br/><br/> Thanks and regards<br/> IPSMART TEAM <BR/>PH: 033-4602-1119";
                if (EmailControl.SendEmailfromIpsmart(userdata.EMAIL, "Forgot Password", body, "") == true)
                {
                    //   otpsend = true;
                    msg += " Mail send to " + userdata.EMAIL;
                }
                Session["FORGOTOTP"] = otp;
                return Content(msg);
            }
        }
        [HttpPost]
        public ActionResult ForgotPassword(Password Pass)
        {
            try
            {
                Connection Cn = new Connection();
                string SCHEMA = Cn.Getschema;
                CS = Cn.GetConnectionString();
                Cn.con = new OracleConnection(CS);
                string Userid = Pass.UserName;
                double[] RAND1 = new double[10];
                string outp = "";
                string msg = "";
                var OTP = Session["FORGOTOTP"];

                if (OTP != null && Pass.OTP == OTP.ToString())
                {
                }
                else
                {
                    msg = "Entered wrong OTP";
                    ViewBag.Msg = msg;
                    return View(Pass);
                }

                if (Pass.NewPassword != Pass.ConfirmPassword)
                {
                    msg = "New and Confirm Password does not match";
                    ViewBag.Msg = msg;
                    outp = "2";
                }
                else
                {
                    string[] pwdchk = Cn.passwordcheckfrompolicy(Pass.UserName, Pass.NewPassword).Split(Convert.ToChar(Cn.GCS()));
                    outp = pwdchk[0];
                    if (pwdchk.Count() > 1) msg = pwdchk[1];
                }
                if (outp == "0")
                {
                    string MNWORD = "";
                    string usernm = Userid + "56";
                    string newpd = usernm + Pass.NewPassword;
                    MNWORD = Cn.Encrypt(newpd);
                    string oldpd = usernm + Pass.CurrentPassword;
                    OracleTransaction OT;
                    if (Cn.con.State == ConnectionState.Closed)
                    {
                        Cn.con.Open();
                    }
                    OT = Cn.con.BeginTransaction(IsolationLevel.ReadCommitted);
                    Cn.com.Transaction = OT;

                    string str = "insert into " + SCHEMA + ".pswd_history (USER_ID, OLD_PSWRD, NEW_PSWRD,LASTPWDCHANGED,CURRPWDCHANGED, USR_ENTDT,USR_ID) ";
                    str += "select user_id, pswrd, '" + MNWORD + "', lastpwdchanged, sysdate, sysdate, '" + Userid + "' ";
                    str += "from " + SCHEMA + ".user_appl where user_id='" + Userid + "' ";
                    Cn.com = new OracleCommand(str, Cn.con);
                    Cn.com.ExecuteNonQuery();

                    str = "update pssw_invalid set password='Forgot' where user_id='" + Userid + "' and password='NO' and ";
                    str += "to_char(login_date,'dd/mm/yyyy') = to_char(sysdate,'dd/mm/yyyy')";
                    Cn.com = new OracleCommand(str, Cn.con);
                    Cn.com.ExecuteNonQuery();

                    str = "UPDATE " + SCHEMA + ".USER_APPL SET PSWRD='" + MNWORD + "', U_ENTDATE_NEW = SYSDATE,LAST_LOGGED = SYSDATE, ";
                    str += "TIMES_LOOGED = NVL(TIMES_LOOGED,0) + 1, TIMES_PSSW_CHANGED = NVL(TIMES_PSSW_CHANGED,0) + 1, LASTPWDCHANGED=SYSDATE, ";
                    str += "LASTFORGOTUSED= SYSDATE, TIMESFORGOTUSED=NVL(TIMESFORGOTUSED,0)+1 ";
                    str += "WHERE USER_ID ='" + Userid + "'";
                    Cn.com = new OracleCommand(str, Cn.con);
                    Cn.com.ExecuteNonQuery();
                    OT.Commit();
                    Cn.con.Close();
                    outp = "5";
                    msg = "Password Changed Successfully";
                    ViewBag.MessageCS = msg; msg = "";
                }
                if (outp == "0")
                {
                    ViewBag.FlagCS = "0";
                }
                else if (outp == "5")
                {
                    if (Pass.CPF == "1")
                    {
                        ViewBag.FlagCS = "1";
                    }
                    else
                    {
                        ViewBag.FlagCS = "1";
                    }
                }
                else
                {
                    ViewBag.FlagCS = "0";
                }
                ViewBag.Msg = msg;  
            }
            catch (Exception ex)
            {
                ViewBag.FlagCS = "0";
                ViewBag.Msg = ex.Message;
            }
            return View(Pass);
        }
    }
}