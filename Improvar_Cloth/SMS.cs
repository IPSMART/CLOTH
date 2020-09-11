using System;
using System.Linq;
using System.Data;
using System.Net;
using System.IO;
using Improvar.Models;

namespace Improvar
{
    public class SMS : MasterHelpFa
    {
        Connection Cn = new Connection();
        M_SUBLEG MSUBLEG; string UNQSNO = CommVar.getQueryStringUNQSNO();
        MasterHelpFa MasterHelpFa = new MasterHelpFa(); Salesfunc Salesfunc = new Salesfunc();
        public string SMSSend(string slcd, string msg, string altmobno = "", bool usealtmobno = false)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));

            string sql = "", scmf = CommVar.FinSchema(UNQSNO);
            string smsurl = "", smsurlsend = "";
            try
            {
                sql = "select a.smsurl from " + scmf + ".m_sms_config a where (a.compcd='" + CommVar.Compcd(UNQSNO) + "' or a.compcd is null ) order by slno";
                DataTable tbl = SQLquery(sql);
                if (tbl.Rows.Count > 0) smsurl = tbl.Rows[0]["smsurl"].ToString();

                MSUBLEG = DBF.M_SUBLEG.Find(slcd);
                string regmobno = "";
                if (MSUBLEG != null) regmobno = MSUBLEG.REGMOBILE.ToString();
                if (altmobno.retStr() != "") regmobno += "," + altmobno;
                string datastring = "";
                if (regmobno == "") datastring = "Reg.Mob.No. not set=XXX";
                else
                {
                    msg = msg.Replace("&", " ");
                    smsurlsend = smsurl;
                    smsurlsend = smsurlsend.Replace("#MOBILENO#", regmobno);
                    smsurlsend = smsurlsend.Replace("#MESSAGE#", msg);
                    string rtval = "";

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
                return ex.Message;
            }
        }
        public string SMSMessGen(string slcd, string reptype, string[,] smsvar)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            string sql = "", scmf = CommVar.FinSchema(UNQSNO);
            string smsmsg = "", smsmsgsend = "";
            sql = "select a.smsmsg from " + scmf + ".m_sms_dtl a where a.reptype='" + reptype + "' and (a.compcd='" + CommVar.Compcd(UNQSNO) + "' or a.compcd is null ) order by slno";
            DataTable tbl = SQLquery(sql);
            if (tbl.Rows.Count > 0) smsmsg = tbl.Rows[0]["smsmsg"].ToString();
            smsmsgsend = smsmsg;
            for (int i = 0; i <= (smsvar.Length / 2) - 1; i++)
            {
                smsmsgsend = smsmsgsend.Replace(smsvar[i, 0], smsvar[i, 1]);
            }
            MSUBLEG = DBF.M_SUBLEG.Find(slcd);
            string slnm = "";
            if (MSUBLEG != null) slnm = MSUBLEG.FULLNAME == null ? MSUBLEG.SLNM : MSUBLEG.FULLNAME;
            smsmsgsend = smsmsgsend.Replace("&slnm&", slnm);
            return smsmsgsend;
        }
        public void insT_TXNSTATUS(string Auto_Number, string ststype, string flag1, string stsrem)
        {
           
            Connection Cn = new Connection();
            var UNQSNO = Cn.getQueryStringUNQSNO();
            Improvar.Models.ImprovarDB DB = new Models.ImprovarDB(Cn.GetConnectionString(),  CommVar.CurSchema(UNQSNO));
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
        public string retSMSSendInfo(string autono, string ststype, string flag1)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string rval = "";
            Improvar.Models.ImprovarDB DB = new Models.ImprovarDB(Cn.GetConnectionString(),  CommVar.CurSchema(UNQSNO));
            var sms = DB.T_TXNSTATUS.Where(a => a.AUTONO == autono && a.STSTYPE == ststype && a.FLAG1 == flag1).SingleOrDefault();
            if (sms != null)
            {
                rval = "Sent by " + sms.USR_ID + " on " + sms.USR_ENTDT.Value.ToShortDateString() + sms.USR_ENTDT.Value.ToLongTimeString() + " [" + sms.EMD_NO.ToString() + "]";
            }
            return rval;
        }
        public string sendSaleSMS(string autono = "", string doccd = "", string slcd = "", string fdocdt = "", string tdocdt = "", string fdocno = "", string tdocno = "")
        {
            string sql = "", scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
            sql = "select a.autono, a.docno, a.docdt, c.blamt, b.lrno, b.lrdt, d.agslcd, c.slcd, d.nopkgs, ";
            sql += "b.ewaybillno, nvl(f.slnm,g.slnm) trslnm, e.regmobile ";
            sql += "from " + scm + ".t_cntrl_hdr a, " + scm + ".t_txntrans b, " + scm + ".t_txn c, " + scm + ".t_txnoth d, ";
            sql += scmf + ".m_subleg e, " + scmf + ".m_subleg f, " + scmf + ".m_subleg g ";
            sql += "where a.autono=b.autono(+) and a.autono=c.autono(+) and a.autono=d.autono(+) and ";
            if (autono.retStr() != "") sql += "a.autono in (" + autono + ") and ";
            if (fdocno.retStr() != "") sql += "a.doconlyno >= '" + fdocno + "' and a.doconlyno <= '" + tdocno + "' and ";
            if (fdocdt.retStr() != "") sql += "a.docdt >= to_date('" + fdocdt + "','dd/mm/yyyy') and ";
            if (tdocdt.retStr() != "") sql += "a.docdt <= to_date('" + tdocdt + "','dd/mm/yyyy') and ";
            if (slcd.retStr() != "") sql += "c.slcd='" + slcd + "' and ";
            if (doccd.retStr() != "") sql += "a.doccd='" + doccd + "' and ";
            if (autono.retStr() == "")
            {
                sql += "a.autono not in (select autono from " + scm + ".t_txnstatus where ststype='S' and flag1='SALE' ) and ";
            }
            sql += "d.agslcd=e.slcd(+) and b.translcd=f.slcd(+) and b.crslcd=g.slcd(+) ";
            DataTable tbl = MasterHelpFa.SQLquery(sql);

            DataTable comptbl = Salesfunc.retComptbl();

            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            string[,] smsaryMsg = new string[8, 2];
            string sendmsg = "", agmobno = "", retmsgtxt = "";
            string msgresult = "";
            if (tbl.Rows.Count == 0) return "-1=No Records";

            for (int i = 0; i <= tbl.Rows.Count - 1; i++)
            {
                if (tbl.Rows.Count > 0)
                {
                    slcd = tbl.Rows[i]["slcd"].ToString();
                    smsaryMsg[0, 0] = "&nopkgs&"; smsaryMsg[0, 1] = tbl.Rows[i]["nopkgs"].ToString();
                    smsaryMsg[1, 0] = "&lrno&"; smsaryMsg[1, 1] = tbl.Rows[i]["lrno"].ToString();
                    smsaryMsg[2, 0] = "&lrdt&"; smsaryMsg[2, 1] = tbl.Rows[i]["lrdt"].ToString().retDateStr();
                    smsaryMsg[3, 0] = "&trslnm&"; smsaryMsg[3, 1] = tbl.Rows[i]["trslnm"].ToString();
                    smsaryMsg[4, 0] = "&docno&"; smsaryMsg[4, 1] = tbl.Rows[i]["docno"].ToString();
                    smsaryMsg[5, 0] = "&docdt&"; smsaryMsg[5, 1] = tbl.Rows[i]["docdt"].ToString().retDateStr();
                    smsaryMsg[6, 0] = "&docamt&"; smsaryMsg[6, 1] = Convert.ToDouble(tbl.Rows[i]["blamt"]).ToINRFormat();
                    smsaryMsg[7, 0] = "&compnm&"; smsaryMsg[7, 1] = comptbl.Rows[0]["compnm"].ToString();
                    if (tbl.Rows[i]["regmobile"].ToString().retStr() != "") agmobno = tbl.Rows[i]["regmobile"].ToString();
                    sendmsg = SMSMessGen(slcd, "SALE", smsaryMsg);
                    msgresult = SMSSend(tbl.Rows[i]["slcd"].ToString(), sendmsg, agmobno);

                    string[] msgretval = msgresult.Split('=');
                    if (msgretval[1].ToString().Substring(0, 1) == "0")
                    {
                        insT_TXNSTATUS(tbl.Rows[i]["autono"].ToString(), "S", "SALE", msgresult);
                    }
                }
            }
            return msgresult;
        }
    }
}