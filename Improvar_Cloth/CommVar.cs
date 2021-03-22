using Improvar.Models;
using System;
using System.Linq;

namespace Improvar
{
    public static class CommVar
    {
        public static string Compcd(string unqsno)
        {
            try
            {
                return System.Web.HttpContext.Current.Session["CompanyCode" + unqsno].ToString();
            }
            catch { }
            try
            {
                return System.Web.HttpContext.Current.Session["CompanyCode"].ToString();
            }
            catch
            {
                return "";
            }
        }
        public static string Loccd(string unqsno)
        {
            try
            {
                return System.Web.HttpContext.Current.Session["CompanyLocationCode" + unqsno].ToString();
            }
            catch { }
            try
            {
                return System.Web.HttpContext.Current.Session["CompanyLocationCode"].ToString();
            }
            catch
            {
                return "";
            }
        }
        public static string ClientCode(string unqsno)
        {
            try
            {
                return System.Web.HttpContext.Current.Session["CLIENT_CODE" + unqsno].ToString();
            }
            catch { }
            try
            {
                return System.Web.HttpContext.Current.Session["CLIENT_CODE"].ToString();
            }
            catch
            {
                return "";
            }
        }
        public static string ModuleCode()
        {
            return System.Web.HttpContext.Current.Session["ModuleCode"].ToString();
        }
        public static string UserID()
        {
            return System.Web.HttpContext.Current.Session["UR_ID"].ToString();
        }
        public static string UserName()
        {
            return System.Web.HttpContext.Current.Session["UR_NAME"].ToString();
        }
        public static string UserMob()
        {
            Connection Cn = new Connection();
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            string MOBILE = DB1.USER_APPL.Find(System.Web.HttpContext.Current.Session["UR_ID"].ToString()).MOBILE;
            return MOBILE;
        }
        public static string CompName(string unqsno)
        {
            try
            {
                return System.Web.HttpContext.Current.Session["CompanyName" + unqsno].ToString();
            }
            catch { }
            try
            {
                return System.Web.HttpContext.Current.Session["CompanyName"].ToString();
            }
            catch
            {
                return "";
            }
        }
        public static string LocName(string unqsno)
        {
            try
            {
                return System.Web.HttpContext.Current.Session["CompanyLocation" + unqsno].ToString();
            }
            catch { }
            try
            {
                return System.Web.HttpContext.Current.Session["CompanyLocation"].ToString();
            }
            catch
            {
                return "";
            }
        }
        public static string YearCode(string unqsno)
        {
            try
            {
                return System.Web.HttpContext.Current.Session["YEAR_CODE" + unqsno].ToString();
            }
            catch { }
            try
            {
                return System.Web.HttpContext.Current.Session["YEAR_CODE"].ToString();
            }
            catch
            {
                return "";
            }
        }
        public static string CurSchema(string unqsno)
        {
            try
            {
                return System.Web.HttpContext.Current.Session["DatabaseSchemaName" + unqsno].ToString();
            }
            catch { }
            try
            {
                return System.Web.HttpContext.Current.Session["DatabaseSchemaName"].ToString();
            }
            catch
            {
                return "";
            }
        }

        public static string FinSchema(string unqsno)
        {
            try
            {
                return System.Web.HttpContext.Current.Session["FINSCHEMA" + unqsno].ToString();
            }
            catch { }
            try
            {
                return System.Web.HttpContext.Current.Session["FINSCHEMA"].ToString();
            }
            catch
            {
                return "";
            }
        }
        public static string InvSchema(string unqsno)
        {
            try
            {
                return System.Web.HttpContext.Current.Session["INVSCHEMA" + unqsno].ToString();
            }
            catch { }
            try
            {
                return System.Web.HttpContext.Current.Session["INVSCHEMA"].ToString();
            }
            catch
            {
                return "";
            }
        }
        public static string PaySchema(string unqsno)
        {
            try
            {
                return System.Web.HttpContext.Current.Session["PAYSCHEMA" + unqsno].ToString();
            }
            catch { }
            try
            {
                return System.Web.HttpContext.Current.Session["PAYSCHEMA"].ToString();
            }
            catch
            {
                return "";
            }
        }
        public static string SaleSchema(string unqsno)
        {
            try
            {
                return System.Web.HttpContext.Current.Session["SDSCHEMA" + unqsno].ToString();
            }
            catch { }
            try
            {
                return System.Web.HttpContext.Current.Session["SDSCHEMA"].ToString();
            }
            catch
            {
                return "";
            }
        }
        public static string LastYearSchema(string unqsno)
        {
            try
            {
                return System.Web.HttpContext.Current.Session["LastYearSchema" + unqsno].ToString();
            }
            catch { }
            try
            {
                return System.Web.HttpContext.Current.Session["LastYearSchema"].ToString();
            }
            catch
            {
                return "";
            }
        }
        public static string CommSchema()
        {
            return "IMPROVAR";
        }
        public static string FinPeriod(string unqsno)
        {
            try
            {
                return System.Web.HttpContext.Current.Session["CompanyFinancial" + unqsno].ToString();
            }
            catch { }
            try
            {
                return System.Web.HttpContext.Current.Session["CompanyFinancial"].ToString();
            }
            catch
            {
                return "";
            }
        }
        public static string FinStartDate(string unqsno)
        {
            try
            {
                string str = System.Web.HttpContext.Current.Session["CompanyFinancial" + unqsno].ToString();
                string[] dt = str.Split('-');
                return dt[0].ToString().Trim();
            }
            catch { }
            try
            {
                string str = System.Web.HttpContext.Current.Session["CompanyFinancial"].ToString();
                string[] dt = str.Split('-');
                return dt[0].ToString().Trim();
            }
            catch
            {
                return "";
            }

        }
        public static string FinEndDate(string unqsno)
        {
            try
            {
                string str = System.Web.HttpContext.Current.Session["CompanyFinancial" + unqsno].ToString();
                string[] dt = str.Split('-');
                return dt[1].ToString().Trim();
            }
            catch { }
            try
            {
                string str = System.Web.HttpContext.Current.Session["CompanyFinancial"].ToString();
                string[] dt = str.Split('-');
                return dt[1].ToString().Trim();
            }
            catch
            {
                return "";
            }

        }
        public static string CurrDate(string unqsno)
        {
            try
            {
                string str = FinPeriod(unqsno);
                string[] dt = str.Split('-');
                str = dt[1].ToString().Trim();
                DateTime tdt = Convert.ToDateTime(str);

                string rtval = System.DateTime.Today.ToString().retDateStr();
                if (tdt < System.DateTime.Today) rtval = CommVar.FinEndDate(unqsno);
                return rtval;
            }
            catch { }
            try
            {
                string str = FinPeriod(unqsno);
                string[] dt = str.Split('-');
                str = dt[1].ToString().Trim();
                DateTime tdt = Convert.ToDateTime(str);

                string rtval = System.DateTime.Today.ToString().retDateStr();
                if (tdt < System.DateTime.Today) rtval = CommVar.FinEndDate(unqsno);
                return rtval;
            }
            catch
            {
                return "";
            }

        }
        public static string SkipTrCd(string asSql = "Y", string BRSOpngtaken = "N", string SkipOpng = "N", string SkipProv = "Y", string SkipPrevBal = "Y")
        {
            string brstrncd = ",'BR'";
            if (BRSOpngtaken != "N") brstrncd = "";
            string rtval = "'BL'";
            if (SkipProv == "Y") rtval = rtval + ",'PV'";
            if (SkipPrevBal == "Y") rtval += ",'LB'";
            rtval = rtval + brstrncd;
            if (SkipOpng == "Y") rtval += ",'OP'";
            if (asSql != "Y")
            {
                rtval = "BL" + brstrncd;
                rtval = rtval.Replace("'", "");
            }
            return rtval;
        }
        public static string retTrCD(string trcd, string pay_by = "")
        {
            string rtval = "";
            switch (trcd)
            {
                case "SB": rtval = "Sales Bill"; break;
                case "PB": rtval = "Purchase Bill"; break;
                case "JV": rtval = "Journal"; break;
                case "BV": rtval = "Bank"; break;
                case "CV": rtval = "Cash"; break;
                case "SC": rtval = "Sales C/N"; break;
                case "SD": rtval = "Sales D/N"; break;
                case "PD": rtval = "Purchase D/N"; break;
                case "PC": rtval = "Purchase C/N"; break;
                case "OP": rtval = "Opening Bal"; break;
                case "BR": rtval = "BRS Opng"; break;
                case "BL": rtval = "Bill wise Opng"; break;
                case "LB": rtval = "Last Year Bal"; break;
                case "CR": rtval = "Chq.Rtd."; break;
                case "PV": rtval = "Prov.Vchr."; break;
                default: rtval = "Voucher"; break;
            }
            string pb_val = "";
            if (pay_by != "")
            {
                switch (pay_by)
                {
                    case "N": pb_val = "NEFT"; break;
                    case "R": pb_val = "RTGS"; break;
                }
            }
            if (pb_val != "") rtval += " - " + pb_val;
            return rtval;
        }
        public static string retHTTPData(string findval)
        {
            Connection CN = new Connection();
            string retval = "";
            var PreviousUrl = System.Web.HttpContext.Current.Request.UrlReferrer.AbsoluteUri;
            var uri = new Uri(PreviousUrl);//Create Virtually Query String
            var queryString = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var MENU_PARA_HTTP = queryString.Count == 0 ? "" : queryString.Get("MP").ToString().Replace(" ", "+");
            string defaultaction = queryString.Count == 0 ? "" : queryString.Get("OP").ToString().Replace(" ", "+");
            string[] para = CN.Decrypt_URL(MENU_PARA_HTTP).Split('~');

            switch (findval)
            {
                case "MENU_PARA":
                    retval = para[0]; break;
                case "DOC_CODE":
                    retval = CN.Decrypt_URL(queryString.Get("DC").ToString().Replace(" ", "+")); break;
                case "MenuID":
                    retval = CN.Decrypt_URL(queryString.Get("Id").ToString().Replace(" ", "+")); break;
                case "DefaultAction":
                    retval = defaultaction; break;
            }
            //var MenuID = queryString.Get("Id").ToString().Replace(" ", "+");
            //var MenuIndex = queryString.Get("Index").ToString().Replace(" ", "+");
            //var PermissionID = queryString.Get("PId").ToString().Replace(" ", "+");
            //var DOC_CODE = queryString.Get("DC").ToString().Replace(" ", "+");
            //var MENU_PARA = queryString.Get("MP").ToString().Replace(" ", "+");
            return retval;
        }
        public static string oldSchema(string scm, string unqsno)
        {
            try
            {
                string yrcd = (Convert.ToDecimal(CommVar.YearCode(unqsno)) - 1).ToString();
                string rval = scm.Substring(0, scm.Length - 4) + yrcd;
                rval = rval.ToUpper();
                if (LastYearSchema(unqsno) == "") rval = "";
                return rval;
            }
            catch
            {
                return "";
            }
        }
        public static string nextSchema(string scm, string unqsno)
        {
            try
            {
                string yrcd = (Convert.ToDecimal(CommVar.YearCode(unqsno)) + 1).ToString();
                string rval = scm.Substring(0, scm.Length - 4) + yrcd;
                rval = rval.ToUpper();
                return rval;
            }
            catch
            {
                return "";
            }
        }
        public static string UserType()
        {
            Connection Cn = new Connection();
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            string MOBILE = DB1.USER_APPL.Find(System.Web.HttpContext.Current.Session["UR_ID"].ToString()).USER_TYPE;
            return MOBILE;
        }
        public static string ModuleShortCode(string scm = "")
        {
            string modcd = System.Web.HttpContext.Current.Session["ModuleCode"].ToString().Substring(0, 1);
            if (scm != "")
            {

            }
            modcd = System.Web.HttpContext.Current.Session["ModuleCode"].ToString().Substring(0, 1);
            return modcd;
        }
        public static string SessionNo()
        {
            string str = System.Web.HttpContext.Current.Session["Session_No"].ToString();
            return str;
        }
        public static string getQueryStringUNQSNO()
        {
            Connection Cn = new Connection();
            return Cn.getQueryStringUNQSNO();
        }
        public static string FinSchemaPrevYr(string unqsno)
        {
            string scm = FinSchema(unqsno);
            string yrcd = (Convert.ToDecimal(CommVar.YearCode(unqsno)) - 1).ToString();
            string rval = scm.Substring(0, scm.Length - 4) + yrcd;
            rval = rval.ToUpper();
            if (LastYearSchema(unqsno) == "") rval = "";
            return rval;
        }
        public static string SaveFolderPath()
        {
            try
            {
                var configDT = (System.Data.DataTable)System.Web.HttpContext.Current.Session["M_SYSCNFG"];
                string DESIGNPATH = configDT.Rows[0]["DESIGNPATH"].retStr();
                string path = @"\\ipsmart-ibm\C\IPSMART";
                if (DESIGNPATH != "" && System.IO.Directory.Exists(DESIGNPATH))
                {
                    return DESIGNPATH;
                }
                if (System.IO.Directory.Exists(path))
                {
                    return path;
                }
            }
            catch
            {
            }
            return @"C:\Ipsmart";
        }
        public static string GSTNO(string unqsno)
        {
            try
            {
                return System.Web.HttpContext.Current.Session["GSTNO" + unqsno].retStr();
            }
            catch
            {
                return "";
            }
        }
        public static string LocalUploadDocPath(string FileName = "")
        {
            string path = System.Web.HttpContext.Current.Server.MapPath("~/UploadDocuments/" + FileName);
            return path;
        }
        public static string WebUploadDocURL(string FileName = "")
        {
            string WebHostPath = System.Web.HttpContext.Current.Session["WebHostPath"].retStr();
            WebHostPath = WebHostPath.TrimEnd('/') + "UploadDocuments/" + FileName;
            return WebHostPath;
        }


    }
}