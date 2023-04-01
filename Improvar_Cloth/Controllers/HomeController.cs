using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using Oracle.ManagedDataAccess.Client;
using System.Web.Configuration;
using System.Data;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Improvar.Controllers
{
    public class HomeController : Controller
    {
        string CS = null;
        Connection Cn = new Connection();
        //  string MODULE = Module.Module_Code;
        MasterHelp masterHelp = new MasterHelp();
        public ActionResult CompanySelection(string MSG = "")
        {
            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                CompanySelection VE = new ViewModels.CompanySelection();
                string SCHEMA = Cn.Getschema;
                var comp_table = "";
                switch (Module.MODULE)
                {
                    case "FINANCE":
                        comp_table = "FIN_COMPANY";
                        break;
                    case "SALES":
                        comp_table = "SD_COMPANY";
                        break;
                    case "PAYROLL":
                        comp_table = "PAY_COMPANY";
                        break;
                    case "INVENTORY":
                        comp_table = "INV_COMPANY";
                        break;
                }
                string str = "SELECT DISTINCT COMPCD,COMPNM from " + SCHEMA + "." + comp_table;
                str = str + " where compcd in (select compcd from ms_musracs ";
                str = str + " where user_id='" + Session["UR_ID"] + "' and module_code like '" + Module.Module_Code + "%') ";
                str = str + " order by COMPNM";
                DataTable recordCompany = masterHelp.SQLquery(str);
                VE.CompanyCode = recordCompany.DataTableToListConvertion<CompanyCode>();
                ViewBag.UserName = "Welcome to IP Smart, USER - " + Session["UR_NAME"].ToString();
                ViewBag.Lastlog = "Your Last Loging Date : " + Cn.format_date(Session["LST_LOG_DT"].ToString());
                INI Handel_Ini = new INI();
                string Userid = Session["UR_ID"].ToString();
                VE.Ischecked = false;

                string sql = "select distinct compcd,loccd,locnm,compnm  from " + SCHEMA + "." + comp_table + "  where  ";
                sql = sql + " loccd in (select loccd from ms_musracs where user_id='" + Userid + "' and module_code like '" + Module.Module_Code + "%') ";
                sql = sql + " order by locnm";
                var dt = masterHelp.SQLquery(sql);
                var CompanyLocation = (from DataRow dr in dt.Rows
                                       select new CompanyLocation()
                                       {
                                           COMPCD = dr["compcd"].retStr(),
                                           LOCCD = dr["loccd"].retStr(),
                                           LOCNM = dr["locnm"].retStr(),
                                           COMPNM = dr["COMPNM"].retStr(),
                                       }).ToList();
                VE.CompanyLocation = new List<Models.CompanyLocation>();
                VE.LocationJSON = JsonConvert.SerializeObject(CompanyLocation);
                sql = "select DISTINCT compcd,compnm,loccd, TO_CHAR(from_date,'DD/MM/YYYY') ||' - '|| TO_CHAR(upto_date,'DD/MM/YYYY') as FINYR, from_date ";
                sql += " from " + SCHEMA + "." + comp_table + " where ";
                sql += " schema_name in (select schema_name from ms_musracs where user_id='" + Userid + "' and module_code like '" + Module.Module_Code + "%') ";
                sql += " order by from_date desc ";
                var finyear = masterHelp.SQLquery(sql);
                var CompanyFinyr = (from DataRow dr in finyear.Rows
                                    select new CompanyFinyr()
                                    {
                                        COMPCD = dr["compcd"].retStr(),
                                        LOCCD = dr["LOCCD"].retStr(),
                                        FINYR = dr["FINYR"].retStr(),
                                        COMPNM = dr["COMPNM"].retStr(),
                                    }).ToList();

                VE.CompanyFinyr = new List<Models.CompanyFinyr>();
                VE.FinYearJSON = JsonConvert.SerializeObject(CompanyFinyr);

                string iniVal_Com = Handel_Ini.IniReadValue(Userid, "COMPCD", Server.MapPath("~/Ipsmart.ini"));
                if (iniVal_Com.Length != 0)
                {
                    VE.COMPCD = iniVal_Com;
                    string iniVal_Loc = Handel_Ini.IniReadValue(Userid, "LOCCD", Server.MapPath("~/Ipsmart.ini"));
                    if (iniVal_Loc.Length != 0)
                    {
                        VE.LOCCD = iniVal_Loc;
                        sql = "select distinct compcd,compnm,loccd,locnm  from " + SCHEMA + "." + comp_table + "  where COMPCD='" + iniVal_Com + "' AND ";
                        sql += " loccd in (select loccd from ms_musracs where user_id='" + Userid + "' and module_code like '" + Module.Module_Code + "%') ";
                        sql += " order by locnm";
                        dt = masterHelp.SQLquery(sql);
                        VE.CompanyLocation = (from DataRow dr in dt.Rows
                                              select new CompanyLocation()
                                              {
                                                  COMPCD = dr["compcd"].retStr(),
                                                  LOCCD = dr["loccd"].retStr(),
                                                  LOCNM = dr["locnm"].retStr(),
                                                  COMPNM = dr["COMPNM"].retStr(),
                                              }).ToList();
                        string iniVal_Fin = Handel_Ini.IniReadValue(Userid, "Fin_year", Server.MapPath("~/Ipsmart.ini"));
                        if (iniVal_Fin.Length != 0)
                        {
                            VE.Finyr = iniVal_Fin;
                            sql = "select DISTINCT compcd,compnm,loccd, TO_CHAR(from_date,'DD/MM/YYYY') ||' - '|| TO_CHAR(upto_date,'DD/MM/YYYY') as FINYR, from_date ";
                            sql += " from " + SCHEMA + "." + comp_table + " where  COMPCD='" + iniVal_Com + "' AND  loccd='" + iniVal_Loc + "' AND ";
                            sql += " schema_name in (select schema_name from ms_musracs where user_id='" + Userid + "' and module_code like '" + Module.Module_Code + "%') ";
                            sql += " order by from_date desc ";
                            dt = masterHelp.SQLquery(sql);
                            VE.CompanyFinyr = (from DataRow dr in dt.Rows
                                               select new CompanyFinyr()
                                               {
                                                   COMPCD = dr["compcd"].retStr(),
                                                   LOCCD = dr["LOCCD"].retStr(),
                                                   FINYR = dr["FINYR"].retStr(),
                                                   COMPNM = dr["COMPNM"].retStr(),
                                               }).ToList();
                            VE.Ischecked = true;
                        }
                    }
                }
                ViewBag.Msg = MSG;
                return View(VE);
            }
        }
        [HttpPost]
        public ActionResult CompanySelection(CompanySelection VE, FormCollection FC, string submitbutton)
        {
            switch (submitbutton)
            {
                case "Logout": return (Logout());
                case "OK": return (OK(VE, FC));
                default: return (View());
            }
        }
        public ActionResult Logout()
        {
            try
            {
                string SCHEMA = Cn.Getschema;
                string upQuery = ("Update " + SCHEMA + ".USER_LOGIN set LOGOUT_DATE=SYSDATE where SESSION_NO=" + Session["Session_No"] + " and  USER_ID='" + Session["UR_ID"] + "'");
                masterHelp.SQLNonQuery(upQuery);
                Session.Clear();
                Session.Abandon();
                return RedirectToAction("Login", "Login");
            }
            catch (Exception ex)
            {
                Session.Clear();
                Session.Abandon();
                return RedirectToAction("Login", "Login");
            }
        }
        public ActionResult OK(CompanySelection VE, FormCollection FC)
        {
            var comp_table = "";
            switch (Module.MODULE)
            {
                case "FINANCE":
                    comp_table = "FIN_COMPANY";
                    break;
                case "SALES":
                    comp_table = "SD_COMPANY";
                    break;
                case "PAYROLL":
                    comp_table = "PAY_COMPANY";
                    break;
                case "INVENTORY":
                    comp_table = "INV_COMPANY";
                    break;
            }
            string message = "";
            string Userid = Session["UR_ID"].ToString();
            string SCHEMA = Cn.Getschema;
            CS = Cn.GetConnectionString();
            Cn.con = new OracleConnection(CS);
            String finyr = FC.Get("finyr");
            string[] yr_frm = FC.Get("finyr").Split('-');
            int dataver = Convert.ToInt32(MyGlobal.Databaseversion);
            string FINAN = finyr;
            string[] FIN = FINAN.Split('-');
            //===== version check========
            Connection Cn1 = new Connection();
            Cn1.con = new OracleConnection(CS);
            if (Cn1.con.State == ConnectionState.Closed)
            {
                Cn1.con.Open();
            }
            string sql = "";
            sql = "select MODULE_CODE from " + comp_table + " where COMPCD='" + VE.COMPCD + "' and LOCCD='" + VE.LOCCD + "' and  FROM_DATE=to_date('" + FIN[0].Trim() + "','dd/mm/yyyy') and UPTO_DATE=to_date('" + FIN[1].Trim() + "','dd/mm/yyyy')";
            DataTable tbl = masterHelp.SQLquery(sql);
            Session.Add("ModuleCode", tbl.Rows[0]["module_code"].ToString());

            Cn1.com = new OracleCommand("select data_version from " + SCHEMA + "." + comp_table + " where COMPCD='" + VE.COMPCD + "' and LOCCD='" + VE.LOCCD + "' and from_date=to_date('" + yr_frm[0] + "','dd/mm/yyyy') and upto_date=to_date('" + yr_frm[1] + "','dd/mm/yyyy') and data_version='" + dataver + "'", Cn1.con);
            Cn1.da.SelectCommand = Cn1.com;
            bool bul = Convert.ToBoolean(Cn1.da.Fill(Cn1.ds, "chkver"));
            Cn1.con.Close();
            if (bul)
            {
                Session.Add("Version", dataver.ToString());
                string publish = Session["publishversion"].ToString().Substring(6);
                if (Cn1.con.State == ConnectionState.Closed)
                {
                    Cn1.con.Open();
                }
                Cn1.com = new OracleCommand("select  * from (select IMP_VER,DATA_VERSION,to_char(INST_DT,'dd/mm/yyyy')INSTDATE,MODULE_CODE from " + SCHEMA + ".ms_version where MODULE_CODE like '" + Module.Module_Code + "%' and DATA_VERSION='" + dataver.ToString() + "' order by IMP_VER desc) where rownum=1", Cn1.con);
                Cn1.dr = Cn1.com.ExecuteReader();
                if (Cn1.dr.HasRows)
                {
                    Cn1.dr.Read();
                    int verno = Convert.ToInt32(Cn1.dr[0]);
                    string publish1 = Cn1.dr[1].ToString();
                    string insdate = Cn1.dr[2].ToString();
                    DateTime InstDate = DateTime.ParseExact(insdate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    Cn1.dr.Close();
                    Cn1.con.Close();
                    if (verno < Convert.ToInt32(publish))
                    {
                        if (Cn1.con.State == ConnectionState.Closed)
                        {
                            Cn1.con.Open();
                        }
                        Cn1.com = new OracleCommand("insert into " + SCHEMA + ".ms_version values(" + publish + ",'" + MyGlobal.Databaseversion_DETAILS + "',SYSDATE,'" + CommVar.ModuleCode() + "','" + dataver + "')", Cn1.con);
                        Cn1.com.ExecuteNonQuery();
                        Cn1.con.Close();
                        //string Mseg = "New version found and install succesfully, Re select company again";
                        //return RedirectToAction("CompanySelection", "Home", new { MSG = Mseg });
                        TempData["NewVersionMsg"] = "New version found and installed succesfully.";
                    }
                    else if (verno > Convert.ToInt32(publish))
                    {
                        double days = (DateTime.Now.Date - InstDate.Date).TotalDays;
                        if (days > 120)
                        {
                            string Mseg = "You application is lock due use of old version over " + days.ToString() + " days";
                            return RedirectToAction("CompanySelection", "Home", new { MSG = Mseg });
                        }
                        else
                        {
                            message = "You are running old version number, Trail period left " + (120 - days) + " days";
                        }
                    }
                }
                else
                {
                    if (Cn1.con.State == ConnectionState.Closed)
                    {
                        Cn1.con.Open();
                    }
                    Cn1.com = new OracleCommand("insert into " + SCHEMA + ".ms_version values(" + publish + ",'" + MyGlobal.Databaseversion_DETAILS + "',SYSDATE,'" + CommVar.ModuleCode() + "','" + dataver + "')", Cn1.con);
                    Cn1.com.ExecuteNonQuery();
                    Cn1.con.Close();
                    //string Mseg = "New version found and install succesfully, Re select Company";
                    //return RedirectToAction("CompanySelection", "Home", new { MSG = Mseg });
                    TempData["NewVersionMsg"] = "New version found and installed succesfully.";
                }
            }
            else
            {
                string Mseg = "Please upgrade your package due to mismatch database current version.";
                return RedirectToAction("CompanySelection", "Home", new { MSG = Mseg });
            }
            //===== end version  ======== 
            try
            {
                if (VE.Ischecked == true)
                {
                    INI Handel_ini = new INI();
                    Handel_ini.IniWriteValue(Userid, "COMPCD", VE.COMPCD, Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(Userid, "LOCCD", VE.LOCCD, Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(Userid, "Fin_year", finyr, Server.MapPath("~/Ipsmart.ini"));
                }
                else
                {
                    INI Handel_ini = new INI();
                    Handel_ini.DeleteSection(Userid, Server.MapPath("~/Ipsmart.ini"));
                }
                string sstr = "select SCHEMA_NAME,sales_schema,fin_schema,invENTORY_schema,pay_schema,compnm,locnm,DATA_VERSION,CLIENT_CODE,YEAR_CODE,import_tag,mirror_tag, prev_schema, next_schema ";
                sstr += "from " + comp_table + " where COMPCD='" + VE.COMPCD + "' and LOCCD='"
                    + VE.LOCCD + "' and  FROM_DATE=to_date('" + FIN[0].ToString() + "','dd/mm/yyyy') and UPTO_DATE=to_date('" + FIN[1].ToString() + "','dd/mm/yyyy')";

                tbl = masterHelp.SQLquery(sstr);
                int version = Convert.ToInt32(WebConfigurationManager.AppSettings["Version"]);
                string SESSIONNO = Session["Session_No"].retStr();
                string YRCD = tbl.Rows[0]["YEAR_CODE"].ToString();
                string UNIQUESESSION = VE.COMPCD + VE.LOCCD + YRCD + SESSIONNO;
                Session.Add("DatabaseSchemaName" + UNIQUESESSION, tbl.Rows[0]["SCHEMA_NAME"].ToString());
                Session.Add("DatabaseSchemaName", tbl.Rows[0]["SCHEMA_NAME"].ToString());
                string DatabaseSchemaName = tbl.Rows[0]["SCHEMA_NAME"].ToString();
                Session.Add("CompanyName", tbl.Rows[0]["COMPNM"].ToString());
                Session.Add("CompanyName" + UNIQUESESSION, tbl.Rows[0]["COMPNM"].ToString());
                Session.Add("CompanyLocation", tbl.Rows[0]["LOCNM"].ToString());
                Session.Add("CompanyLocation" + UNIQUESESSION, tbl.Rows[0]["LOCNM"].ToString());
                Session.Add("CompanyFinancial" + UNIQUESESSION, finyr);
                Session.Add("CompanyFinancial", finyr);
                Session.Add("SDSCHEMA" + UNIQUESESSION, tbl.Rows[0]["sales_schema"].ToString());
                Session.Add("SDSCHEMA", tbl.Rows[0]["sales_schema"].ToString());
                Session.Add("PAYSCHEMA" + UNIQUESESSION, tbl.Rows[0]["pay_schema"].ToString());
                Session.Add("PAYSCHEMA", tbl.Rows[0]["pay_schema"].ToString());
                Session.Add("INVSCHEMA" + UNIQUESESSION, tbl.Rows[0]["invENTORY_schema"].ToString());
                Session.Add("INVSCHEMA", tbl.Rows[0]["invENTORY_schema"].ToString());
                Session.Add("FINSCHEMA" + UNIQUESESSION, tbl.Rows[0]["fin_schema"].ToString());
                Session.Add("FINSCHEMA", tbl.Rows[0]["fin_schema"].ToString());
                Session.Add("LastYearSchema" + UNIQUESESSION, tbl.Rows[0]["PREV_SCHEMA"].ToString());
                Session.Add("LastYearSchema", tbl.Rows[0]["PREV_SCHEMA"].ToString());
                Session.Add("NEXTSCHEMA" + UNIQUESESSION, tbl.Rows[0]["NEXT_SCHEMA"].ToString());
                Session.Add("NEXTSCHEMA", tbl.Rows[0]["NEXT_SCHEMA"].ToString());
                int dataversion = Convert.ToInt32(tbl.Rows[0]["DATA_VERSION"]);
                Session.Add("CompanyLocationCode" + UNIQUESESSION, VE.LOCCD);
                Session.Add("CompanyLocationCode", VE.LOCCD);
                Session.Add("CompanyCode" + UNIQUESESSION, VE.COMPCD);
                Session.Add("CompanyCode", VE.COMPCD);

                Session.Add("CLIENT_CODE" + UNIQUESESSION, tbl.Rows[0]["CLIENT_CODE"].ToString());
                Session.Add("CLIENT_CODE", tbl.Rows[0]["CLIENT_CODE"].ToString());
                Session.Add("YEAR_CODE" + UNIQUESESSION, tbl.Rows[0]["YEAR_CODE"].ToString());
                Session.Add("YEAR_CODE", tbl.Rows[0]["YEAR_CODE"].ToString());
                Session.Add("IMPORT_TAG", tbl.Rows[0]["IMPORT_TAG"].ToString());
                Session.Add("MIRROR_TAG", tbl.Rows[0]["MIRROR_TAG"].ToString());
                sql = "";
                sql = "select GSTNO, nvl(showloccd,loccd) showloccd from " + tbl.Rows[0]["fin_schema"].ToString() + ".M_loca a where loccd='" + VE.LOCCD + "' and compcd='" + VE.COMPCD + "'";
                var dt = masterHelp.SQLquery(sql);
                if (dt.Rows.Count > 0)
                {
                    Session.Add("GSTNO" + UNIQUESESSION, dt.Rows[0]["GSTNO"].ToString());
                    Session.Add("GSTNO", dt.Rows[0]["GSTNO"].ToString());
                    Session.Add("SHOWLOCCD" + UNIQUESESSION, dt.Rows[0]["showloccd"].ToString());
                    Session.Add("SHOWLOCCD", dt.Rows[0]["showloccd"].ToString());
                }
                if (version < dataversion)
                {
                    Session.Add("package", "Package is too old to run, Copy latest version no " + dataversion.ToString());
                    Response.BufferOutput = true;
                    return RedirectToAction("CompanySelection", "Home");
                }
                else if (version > dataversion)
                {
                    Session.Add("package", "Old Database version is running,Please install version no " + version.ToString());
                    Response.BufferOutput = true;
                    return RedirectToAction("CompanySelection", "Home");
                }
                else
                {
                    switch (Module.MODULE)
                    {
                        case "FINANCE":
                            Session.Add("MotherMenuIdentifier", "FAIMPROVAR");
                            break;
                        case "SALES":
                            Session.Add("MotherMenuIdentifier", "SDIMPROVAR");
                            break;
                        case "PAYROLL":
                            Session.Add("MotherMenuIdentifier", "PAYIMPROVAR");
                            break;
                        case "INVENTORY":
                            Session.Add("MotherMenuIdentifier", "INVIMPROVAR");
                            break;
                    }
                    Session.Add("ModuleNAME", CommVar.ModuleCode());

                    sql = "select listagg('a.'||a.column_name,', ') within group(order by a.COLUMN_ID) colnm from all_tab_cols a ";
                    sql += "where table_name = 'M_SYSCNFG' and owner = '" + DatabaseSchemaName + "'";
                    DataTable rstmp = masterHelp.SQLquery(sql);
                    if (rstmp.Rows.Count > 0 && rstmp.Rows[0]["colnm"].retStr() != "")
                    {
                        sql = "";
                        //sql = "select " + rstmp.Rows[0]["colnm"] + " from " + DatabaseSchemaName + ".m_syscnfg a";
                        sql += " select " + rstmp.Rows[0]["colnm"] + ",a.rtdebcd,b.rtdebnm,b.mobile,a.inc_rate,C.TAXGRPCD,a.retdebslcd,b.city,b.add1,b.add2,b.add3, a.effdt, d.prccd, e.prcnm,a.wppricegen,a.rppricegen,a.wpper, a.rpper, a.priceincode ";
                        sql += "  from  " + DatabaseSchemaName + ".M_SYSCNFG a, " + tbl.Rows[0]["fin_schema"].ToString() + ".M_RETDEB b, " + DatabaseSchemaName + ".M_SUBLEG_SDDTL c, " + DatabaseSchemaName + ".m_subleg_com d, " + tbl.Rows[0]["fin_schema"].ToString() + ".m_prclst e ";
                        sql += " where a.RTDEBCD=b.RTDEBCD and a.retdebslcd=d.slcd(+) and ";
                        sql += "a.retdebslcd=C.SLCD(+) and c.compcd='" + VE.COMPCD + "' and c.loccd='" + VE.LOCCD + "' and d.prccd=e.prccd(+) ";
                        rstmp = masterHelp.SQLquery(sql);

                        Session.Add("M_SYSCNFG", rstmp);
                    }
                    Response.BufferOutput = true;
                    return RedirectToAction("multiVu", "Multiviewer", new { US = Cn.Encrypt_URL(UNIQUESESSION) });
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return RedirectToAction("CompanySelection", "Home");
            }
        }
        public ActionResult ModuleSelector(string code, string controlschema, string menuschema, string sortby, string mname)
        {
            Session.Add("ModuleCode", code);
            Session.Add("ModuleControlSchema", controlschema);
            Session.Add("ModuleMenuSchema", menuschema);
            Session.Add("ModuleMenuSortBy", sortby);
            Session.Add("ModuleNAME", mname);
            return RedirectToAction("CompanySelection", "Home");
        }
        public ActionResult UserActivity_OpenForm(string MNUDET, string US)
        {
            try
            {
                MNUDET = Cn.Decrypt_URL(MNUDET);
                var MenuID = MNUDET.Split('~')[0];
                var Menuindex = MNUDET.Split('~')[1];
                US = Cn.Decrypt_URL(US);
                Improvar.Models.ImprovarDB DB = new Models.ImprovarDB(Cn.GetConnectionString(), CommVar.CommSchema());
                USER_ACTIVITY USRACT = new USER_ACTIVITY();
                USRACT.USER_ID = Convert.ToString(Session["UR_ID"]);
                USRACT.COMPCD = CommVar.Compcd(US);
                USRACT.LOCCD = CommVar.Loccd(US);
                USRACT.SESSION_NO = Convert.ToInt32(Session["Session_No"]);
                USRACT.MENU_ID = MenuID;
                USRACT.MENU_INDEX = Convert.ToByte(Menuindex);
                byte zero = Convert.ToByte(0);

                var MAXSESSION = DB.USER_ACTIVITY.Where(a => a.USER_ID == USRACT.USER_ID && a.COMPCD == USRACT.COMPCD && a.LOCCD == USRACT.LOCCD && a.SESSION_NO == USRACT.SESSION_NO && a.MENU_ID == USRACT.MENU_ID && a.MENU_INDEX == USRACT.MENU_INDEX).Select(a => a.MENU_SESSION).DefaultIfEmpty(zero).Max();

                if (MAXSESSION == 0)
                {
                    MAXSESSION = Convert.ToByte(MAXSESSION + 1);
                    USRACT.MENU_SESSION = MAXSESSION;
                }
                else
                {
                    MAXSESSION = Convert.ToByte(MAXSESSION + 1);
                    USRACT.MENU_SESSION = MAXSESSION;
                }
                USRACT.MODULE_CODE = Convert.ToString(Session["ModuleCode"]);
                USRACT.IN_DATE = System.DateTime.Now;
                USRACT.OUT_DATE = null;

                DB.USER_ACTIVITY.Add(USRACT);
                DB.SaveChanges();
                //Session.Add("menuid", MenuID);
                //Session.Add("menuindex", Menuindex);
                Session.Add("menusession", MAXSESSION);
                //Session.Add("menupermission", PermissionID);
                //Session.Add("menudoccd", Doccd);
                // Session.Add("menuparacd", MenuPara);
                return Content("OK");
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content("NO");
            }
        }

        public ActionResult UserActivity_CloseForm(string MENU_DETAILS, string UNQSNO)
        {
            try
            {
                //MENU_DETAILS = Cn.Decrypt_URL(MENU_DETAILS);
                var MenuID = MENU_DETAILS.Split('~')[0];
                var Menuindex = MENU_DETAILS.Split('~')[1];
                Improvar.Models.ImprovarDB DB = new Models.ImprovarDB(Cn.GetConnectionString(), CommVar.CommSchema());
                long sesscode = Convert.ToInt32(Session["Session_No"]);
                int mm = Convert.ToInt16(Menuindex);
                //int ms = Convert.ToInt16(Menusession);
                string uid = Session["UR_ID"].ToString();
                string cid = CommVar.Compcd(UNQSNO);
                string sid = CommVar.Loccd(UNQSNO);
                USER_ACTIVITY USRACT = (from i in DB.USER_ACTIVITY where (i.USER_ID == uid && i.COMPCD == cid && i.LOCCD == sid && i.SESSION_NO == sesscode && i.MENU_ID == MenuID && i.MENU_INDEX == mm && i.OUT_DATE == null) select i).FirstOrDefault();
                if (USRACT != null)
                {
                    USRACT.OUT_DATE = System.DateTime.Now;
                    DB.Entry(USRACT).State = System.Data.Entity.EntityState.Modified;
                    DB.SaveChanges();
                }
                return null;
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return null;
            }
        }

        public ActionResult ModifyLogDetail(string autono, string schema)
        { //called from every view Page
            try
            {
                string UNQSNO = Cn.getQueryStringUNQSNO();
                string dbnm = CommVar.CurSchema(UNQSNO);
                if (schema == "FIN")
                {
                    dbnm = CommVar.FinSchema(UNQSNO);
                }
                DataTable tbl;
                string sql = "";
                sql += " select a.emd_no, a.usr_entdt, nvl(b.user_name,a.usr_id) usr_id from (";
                sql += " select to_char(a.emd_no) emd_no, nvl(a.lm_usr_entdt,a.usr_entdt) usr_entdt, nvl(a.lm_usr_id,a.usr_id) usr_id,  ";
                sql += " nvl(a.lm_usr_lip,a.usr_lip) lm_usr_lip, nvl(a.lm_usr_sip,a.usr_sip) usr_sip  ";
                sql += " from " + dbnm + ".t_cntrl_hdrt a ";
                sql += " where a.autono in ('" + autono + "') ";
                sql += " union  ";
                sql += " select to_char(a.emd_no) emd_no, nvl(a.lm_usr_entdt,a.usr_entdt) usr_entdt, nvl(a.lm_usr_id,a.usr_id) usr_id,  ";
                sql += " nvl(a.lm_usr_lip,a.usr_lip) lm_usr_lip, nvl(a.lm_usr_sip,a.usr_sip) usr_sip ";
                sql += " from " + dbnm + ".t_cntrl_hdr a ";
                sql += " where a.autono in ('" + autono + "') ";
                sql += " union ";
                sql += " select to_char(a.emd_no) emd_no, nvl(a.lm_usr_entdt,a.usr_entdt) usr_entdt, nvl(a.lm_usr_id,a.usr_id) usr_id,  ";
                sql += " nvl(a.lm_usr_lip,a.usr_lip) lm_usr_lip, nvl(a.lm_usr_sip,a.usr_sip) usr_sip  ";
                sql += " from " + dbnm + ".m_cntrl_hdrt a ";
                sql += " where to_char(a.m_autono) in ('" + autono + "') ";
                sql += " union ";
                sql += " select to_char(a.emd_no) emd_no, nvl(a.lm_usr_entdt,a.usr_entdt) usr_entdt, nvl(a.lm_usr_id,a.usr_id) usr_id,  ";
                sql += " nvl(a.lm_usr_lip,a.usr_lip) lm_usr_lip, nvl(a.lm_usr_sip,a.usr_sip) usr_sip  ";
                sql += " from " + dbnm + ".m_cntrl_hdr a ";
                sql += " where to_char(a.m_autono) in ('" + autono + "') ";
                sql += " union ";
                sql += " select 'AUTH'||decode(a.slno,1,'','-'||to_char(a.slno)) emd_no, a.usr_entdt, a.usr_id, a.usr_lip, a.usr_sip";
                sql += " from " + dbnm + ".t_cntrl_auth a ";
                sql += " where a.autono in ('" + autono + "') ";
                sql += " union ";
                sql += " select a.ststype||a.flag1 emd_no, a.usr_entdt, a.usr_id, a.usr_lip, a.usr_sip";
                sql += " from " + dbnm + ".t_txnstatus a ";
                sql += " where a.autono in ('" + autono + "') ";
                sql += " ) a,  ";
                sql += " user_appl b where a.usr_id = b.user_id(+) ";
                sql += " order by usr_entdt desc ";
                tbl = masterHelp.SQLquery(sql);
                ModifyLog VE = new ModifyLog();
                VE.ModifyLogGrid = (from DataRow dr in tbl.Rows
                                    select new ModifyLogGrid()
                                    {
                                        SLNO = (dr["emd_no"].ToString() == "" || dr["emd_no"].ToString() == "0") ? "Original" : dr["emd_no"].ToString(),
                                        USERID = dr["usr_id"].ToString(),
                                        MODIFYDT = dr["usr_entdt"].ToString(),
                                    }).ToList();

                return PartialView("_ModifyLog", VE);

            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
        public ActionResult ReadParkRecord(string MNUDET)
        {
            try
            {
                var MenuID = MNUDET.Split('~')[0];
                var Menuindex = MNUDET.Split('~')[1];
                var UNQSNO = Cn.getQueryStringUNQSNO();
                string SB = masterHelp.PARK_ENTRIES(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), MenuID, Menuindex, Session["UR_ID"].ToString(), Server.MapPath("~/Park.ini"));
                return PartialView("_Park", SB);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return null;
            }
        }
        public ActionResult DeleteParkRecord(string ID)
        {
            try
            {
                INI iniH = new INI();
                iniH.DeleteKey(Session["UR_ID"].ToString(), ID, Server.MapPath("~/Park.ini"));
                return Content("1");
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return null;
            }
        }
    }
}