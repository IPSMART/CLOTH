﻿using System;
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
        Salesfunc salesfunc = new Salesfunc();

        public string SMSsend(string mobno, string msg, string TemplateID, string anothermobno = "")
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
                if (anothermobno.retStr() != "") mobno += "," + anothermobno;

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
            string smsmsg = "", smsmsgsend = "", altmobno = "", autosend = "";

            sql = "";
            sql += "SELECT smsmsg,tempid,ALTMOBNO, AUTOSEND FROM " + scmf + ".m_sms_dtl WHERE EXISTS(select * from " + scmf + ".m_sms_dtl WHERE compcd = '"
                + CommVar.Compcd(UNQSNO) + "') and compcd = '" + CommVar.Compcd(UNQSNO) + "' AND REPTYPE = '" + reptype + "' " + Environment.NewLine;
            sql += " union all ";
            sql += "SELECT smsmsg,tempid,ALTMOBNO, AUTOSEND FROM " + scmf + ".m_sms_dtl WHERE not EXISTS(select * from " + scmf + ".m_sms_dtl WHERE compcd = '"
                + CommVar.Compcd(UNQSNO) + "')  AND REPTYPE = '" + reptype + "' AND COMPCD IS NULL " + Environment.NewLine;
            //sql = "select a.smsmsg,tempid,ALTMOBNO, AUTOSEND from " + scmf + ".m_sms_dtl a where a.reptype='" + reptype + "' and (a.compcd='" + CommVar.Compcd(UNQSNO) + "' or a.compcd is null ) order by slno";
            DataTable tbl = masterHelp.SQLquery(sql);
            if (tbl.Rows.Count > 0)
            {
                smsmsg = tbl.Rows[0]["smsmsg"].ToString();
                tempid = tbl.Rows[0]["tempid"].ToString();
                altmobno = tbl.Rows[0]["altmobno"].ToString();
                autosend = tbl.Rows[0]["autosend"].ToString();
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
                System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex("[^a-zA-Z0-9 -]");
                slnm = rgx.Replace(slnm, "");
                slnm = CommFunc.TruncateWord(slnm, 28);
            }
            smsmsgsend = smsmsgsend.Replace("&slnm&", slnm);
            List<string> smslist = new List<string>();
            smslist.Add(smsmsgsend);
            smslist.Add(tempid);
            smslist.Add(altmobno);
            smslist.Add(autosend);
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
        #region whatsapp

        public string WHATSAPPsend(string mobno, string msg, string TemplateID, string pdfparamnm = "", List<string> pdffilenm = null, string imgparamnm = "", List<string> imgfilenm = null)
        {
            mobno = "9073223344";
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string sql = "", scmf = CommVar.FinSchema(UNQSNO);
            string smsurl = "", smsurlsend = "";
            try
            {

                sql = "select a.smsurl from " + scmf + ".m_sms_config a where (a.compcd='" + CommVar.Compcd(UNQSNO) + "' or a.compcd is null ) and smstype='W' order by slno";
                DataTable tbl = masterHelp.SQLquery(sql);
                if (tbl.Rows.Count > 0) smsurl = tbl.Rows[0]["smsurl"].ToString();
                //smsurl = "https://dealsms.in/api/send?number=91#MOBILENO#&type=media&message=#MESSAGE#&media_url=#FILEURL#&instance_id=684BF0938AA0A&access_token=6849863649fd7";
                string datastring = "";
                string sendfilenmp = "", sendfilenmi = "";
                double cnt = 0;
                int pdfindex = 0, imgindex = 0;
                while (true)
                {
                    if (cnt != 0)
                    {
                        msg = " ";
                    }
                    smsurlsend = smsurl;
                    smsurlsend = smsurlsend.Replace("#MOBILENO#", mobno);
                    smsurlsend = smsurlsend.Replace("#MESSAGE#", msg);
                    smsurlsend = smsurlsend.Replace("#TEMPID#", TemplateID);

                    string fileurl = "";
                    if (pdffilenm.Count() > 0 && pdfindex < pdffilenm.Count())
                    {
                        string path = salesfunc.GetWhatsappFilePath() + pdffilenm[pdfindex].retStr();
                        sendfilenmp += "<br/>" + pdffilenm[pdfindex].retStr();
                        fileurl += "&" + pdfparamnm + "=" + path;
                        pdfindex++;
                    }
                    else
                    {
                        if (imgfilenm.Count() > 0)
                        {
                            for (int a = 0; a < 4; a++)
                            {
                                string path = salesfunc.GetWhatsappFilePath() + imgfilenm[imgindex].retStr();
                                sendfilenmi += "<br/>" + imgfilenm[imgindex].retStr();
                                fileurl += "&" + imgparamnm + "" + (a + 1) + "=" + path;
                                imgindex++;
                                if (imgindex >= imgfilenm.Count()) break;
                            }
                        }
                    }
                    smsurlsend = smsurlsend.Replace("#FILEURL#", fileurl);


                    WebRequest rqst = HttpWebRequest.Create(smsurlsend);
                    HttpWebResponse rspns = (HttpWebResponse)rqst.GetResponse();
                    Stream strm = (Stream)rspns.GetResponseStream();
                    StreamReader strmrdr = new StreamReader(strm);
                    datastring = strmrdr.ReadToEnd();
                    rspns.Close();
                    strm.Close();
                    strmrdr.Close();
                    Cn.SaveTextFile(smsurlsend, "ERROR LOG WHATSAPP" + DateTime.Today.ToString("yyyy-MM-dd"), @"C:/IPSMART/ErrorLogWhatsapp");
                    if (imgindex >= imgfilenm.Count() && pdfindex >= pdffilenm.Count()) break;
                    cnt++;

                }
                return "=Sending File : " + sendfilenmp + sendfilenmi;
            }
            catch (Exception ex)
            {
                Cn.SaveTextFile(smsurlsend, "ERROR LOG WHATSAPP" + DateTime.Today.ToString("yyyy-MM-dd"), @"C:/IPSMART/ErrorLogWhatsapp");
                Cn.SaveException(ex, smsurl + "    " + smsurlsend);
                return ex.Message + "=" + smsurlsend;
            }
        }
        public List<string> WHATSAPPMessContectGen(string slcd, string reptype, string[,] smsvar)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            string sql = "", scmf = CommVar.FinSchema(UNQSNO); string tempid = "";
            string smsmsg = "", smsmsgsend = "", altmobno = "", autosend = "";
            sql = "select a.smsmsg,tempid,ALTMOBNO, AUTOSEND from " + scmf + ".m_sms_dtl a where a.reptype='" + reptype + "' and (a.compcd='" + CommVar.Compcd(UNQSNO) + "' or a.compcd is null ) order by slno";
            DataTable tbl = masterHelp.SQLquery(sql);
            if (tbl.Rows.Count > 0)
            {
                smsmsg = tbl.Rows[0]["smsmsg"].ToString();
                tempid = tbl.Rows[0]["tempid"].ToString();
                altmobno = tbl.Rows[0]["altmobno"].ToString();
                autosend = tbl.Rows[0]["autosend"].ToString();
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
                System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex("[^a-zA-Z0-9 -]");
                slnm = rgx.Replace(slnm, "");
                slnm = CommFunc.TruncateWord(slnm, 28);
            }
            smsmsgsend = smsmsgsend.Replace("&slnm&", slnm);
            List<string> smslist = new List<string>();
            smslist.Add(smsmsgsend);
            smslist.Add(tempid);
            smslist.Add(altmobno);
            smslist.Add(autosend);
            return smslist;
        }
        #endregion
    }
}