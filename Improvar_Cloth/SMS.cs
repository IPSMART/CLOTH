using System;
using System.Linq;
using System.Data;
using System.Net;
using System.IO;
using Improvar.Models;
using System.Collections.Generic;

namespace Improvar
{
    public class SMS : MasterHelpFa
    {
        Connection Cn = new Connection();
        M_SUBLEG MSUBLEG;
        MasterHelp masterHelp = new MasterHelp();
        public string SMSsend(string mobno, string msg, string TemplateID)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string sql = "", scmf = CommVar.FinSchema(UNQSNO);
            string smsurl = "", smsurlsend = "";
            try
            {
                sql = "select a.smsurl from " + scmf + ".m_sms_config a where (a.compcd='" + CommVar.Compcd(UNQSNO) + "' or a.compcd is null ) order by slno";
                DataTable tbl = masterHelp.SQLquery(sql);
                if (tbl.Rows.Count > 0) smsurl = tbl.Rows[0]["smsurl"].ToString();
                string datastring = "";
                smsurlsend = smsurl;
                smsurlsend = smsurlsend.Replace("#MOBILENO#", mobno);
                smsurlsend = smsurlsend.Replace("#MESSAGE#", msg);
                smsurlsend = smsurlsend.Replace("#TEMPID#", TemplateID);

                WebRequest rqst = HttpWebRequest.Create(smsurlsend);
                HttpWebResponse rspns = (HttpWebResponse)rqst.GetResponse();
                Stream strm = (Stream)rspns.GetResponseStream();
                StreamReader strmrdr = new StreamReader(strm);
                datastring = strmrdr.ReadToEnd();
                rspns.Close();
                strm.Close();
                strmrdr.Close();

                return "";
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, smsurl);
                return ex.Message;
            }
        }
        public string SMSfromIpSmart(string msg, string regmobno, string TemplateID)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string sql = "", scmf = CommVar.FinSchema(UNQSNO);
            string smsurl = "", smsurlsend = "";
            try
            {
                smsurl = "http://info.nkinfo.in/sendsms?uname=ipsmart&pwd=ipsmart12345&senderid=IPSMRT&to=#MOBILENO#&msg=#MESSAGE# IPSMART&route=T&peid=1701159188145376946&tempid=#TEMPID#";
                //smsurl = "http://59.162.167.52/api/MessageCompose?admin=info@nkinfo.in&user=ipsmart@nkinfo.in:ipsmart12345&senderID=IPSMRT&receipientno=#MOBILENO#&msgtxt=#MESSAGE#&state=4";
                string datastring = "";
                if (regmobno == "") datastring = "Reg.Mob.No. not set=XXX";
                else
                {
                    smsurlsend = smsurl;
                    smsurlsend = smsurlsend.Replace("#MOBILENO#", regmobno);
                    smsurlsend = smsurlsend.Replace("#MESSAGE#", msg);
                    smsurlsend = smsurlsend.Replace("#TEMPID#", TemplateID);

                    WebRequest rqst = HttpWebRequest.Create(smsurlsend);
                    HttpWebResponse rspns = (HttpWebResponse)rqst.GetResponse();
                    Stream strm = (Stream)rspns.GetResponseStream();
                    StreamReader strmrdr = new StreamReader(strm);
                    datastring = strmrdr.ReadToEnd();
                    rspns.Close();
                    strm.Close();
                    strmrdr.Close();
                }
                return datastring;
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, smsurlsend);
                return ex.Message;
            }
        }
        public List<string> SMSMessContectGen(string slcd, string reptype, string[,] smsvar)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            string sql = "", scmf = CommVar.FinSchema(UNQSNO); string tempid = "";
            string smsmsg = "", smsmsgsend = "";
            sql = "select a.smsmsg,tempid from " + scmf + ".m_sms_dtl a where a.reptype='" + reptype + "' and (a.compcd='" + CommVar.Compcd(UNQSNO) + "' or a.compcd is null ) order by slno";
            DataTable tbl = masterHelp.SQLquery(sql);
            if (tbl.Rows.Count > 0)
            {
                smsmsg = tbl.Rows[0]["smsmsg"].ToString();
                tempid = tbl.Rows[0]["tempid"].ToString();
            }
            smsmsgsend = smsmsg;
            for (int i = 0; i <= (smsvar.Length / 2) - 1; i++)
            {
                smsmsgsend = smsmsgsend.Replace(smsvar[i, 0], smsvar[i, 1]);
            }
            string slnm = "";
            if (!string.IsNullOrEmpty(slcd))
            {
                MSUBLEG = DBF.M_SUBLEG.Find(slcd);
                if (MSUBLEG != null) slnm = MSUBLEG.FULLNAME == null ? MSUBLEG.SLNM : MSUBLEG.FULLNAME;
                slnm = CommFunc.TruncateWord(slnm, 25);
            }
            smsmsgsend = smsmsgsend.Replace("&slnm&", slnm);
            List<string> smslist = new List<string>();
            smslist.Add(smsmsgsend);
            smslist.Add(tempid);
            return smslist;
        }
        public void insT_TXNSTATUS(string Auto_Number, string ststype, string flag1, string stsrem)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            Improvar.Models.ImprovarDB DB = new Models.ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            Models.T_TXNSTATUS TCH = new Models.T_TXNSTATUS();

            var MAXEMDNO = (from p in DB.T_TXNSTATUS where (p.AUTONO == Auto_Number && p.FLAG1 == flag1 && p.STSTYPE == ststype) select p.EMD_NO).Max();
            short emdno = 0;
            if (MAXEMDNO == null) emdno = 0; else emdno = Convert.ToByte(MAXEMDNO + 1);

            var TCHOLD = (from i in DB.T_TXNSTATUS
                          where (i.AUTONO == Auto_Number && i.STSTYPE == ststype && i.FLAG1 == flag1)
                          select i).ToList();
            if (TCHOLD.Any())
            {
                DB.T_TXNSTATUS.Where(x => x.AUTONO == Auto_Number && x.FLAG1 == flag1 && x.STSTYPE == ststype).ToList().ForEach(x => { x.DTAG = "D"; });
                DB.T_TXNSTATUS.RemoveRange(DB.T_TXNSTATUS.Where(x => x.AUTONO == Auto_Number && x.FLAG1 == flag1 && x.STSTYPE == ststype));
            }
            TCH.AUTONO = Auto_Number;
            TCH.STSTYPE = ststype;
            TCH.FLAG1 = flag1;
            TCH.CLCD = CommVar.ClientCode(UNQSNO);
            TCH.STSREM = stsrem;
            TCH.USR_ID = System.Web.HttpContext.Current.Session["UR_ID"].ToString();
            TCH.USR_ENTDT = System.DateTime.Now;
            TCH.USR_LIP = Cn.GetIp();
            TCH.USR_SIP = Cn.GetStaticIp();
            TCH.USR_OS = null;
            TCH.USR_MNM = Cn.DetermineCompName(Cn.GetIp());  //GetMachin;
            TCH.DTAG = "";
            TCH.EMD_NO = emdno;

            DB.T_TXNSTATUS.Add(TCH);
            DB.SaveChanges();
            return;
        }
        public string SMSPara(string reptype, string slcd = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            string sql = "", scmf = CommVar.FinSchema(UNQSNO);
            string smsautosend = "N", smsurl = "", smsretval = "", slcdregmob = "";

            sql = "select a.smsurl from " + scmf + ".m_sms_config a where (a.compcd='" + CommVar.Compcd(UNQSNO) + "' or a.compcd is null ) order by slno";
            DataTable tbl = masterHelp.SQLquery(sql);
            if (tbl.Rows.Count > 0) smsurl = "Y";

            if (smsurl != "")
            {
                sql = "select nvl(a.autosend,'N') autosend from " + scmf + ".m_sms_dtl a where a.reptype='" + reptype + "' and (a.compcd='" + CommVar.Compcd(UNQSNO) + "' or a.compcd is null ) order by slno";
                tbl = masterHelp.SQLquery(sql);
                if (tbl.Rows.Count > 0) smsautosend = tbl.Rows[0]["autosend"].ToString();
            }

            if (slcd != "" && smsurl != "")
            {
                sql = "select a.regemailid, a.regmobile from " + scmf + ".m_subleg a where slcd='" + slcd + "'";
                tbl = masterHelp.SQLquery(sql);
                if (tbl.Rows.Count > 0)
                {
                    if (tbl.Rows[0]["regmobile"].ToString() != "") slcdregmob = "Y";
                }
            }

            smsretval = smsurl + Cn.GCS() + smsautosend + Cn.GCS() + slcdregmob;
            return smsretval;
        }
        public string retSMSSendInfo(string autono, string ststype, string flag1)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string rval = "";
            Improvar.Models.ImprovarDB DB = new Models.ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            var sms = DB.T_TXNSTATUS.Where(a => a.AUTONO == autono && a.STSTYPE == ststype && a.FLAG1 == flag1).SingleOrDefault();
            if (sms != null)
            {
                rval = "Sent by " + sms.USR_ID + " on " + sms.USR_ENTDT.Value.ToShortDateString() + sms.USR_ENTDT.Value.ToLongTimeString() + " [" + sms.EMD_NO.ToString() + "]";
            }
            return rval;
        }

}
}