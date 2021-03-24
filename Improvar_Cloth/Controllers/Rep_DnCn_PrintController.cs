using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;

namespace Improvar.Controllers
{
    public class Rep_DnCn_PrintController : Controller
    {
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        MasterHelpFa MasterHelpFa = new MasterHelpFa();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: Rep_DnCn_Print
        public ActionResult Rep_DnCn_Print()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "DnCn Printing";
                    ReportViewinHtml VE;
                    if (TempData["printparameter"] == null)
                    {
                        VE = new ReportViewinHtml();
                    }
                    else
                    {
                        VE = (ReportViewinHtml)TempData["printparameter"];
                    }
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    string reptype = "JOBBILL";
                    if (VE.maxdate == "CHALLAN") reptype = "CHALLAN";
                    DataTable repformat = Salesfunc.getRepFormat(reptype, VE.DOCCD);
                    if (repformat != null)
                    {
                        VE.DropDown_list1 = (from DataRow dr in repformat.Rows
                                             select new DropDown_list1()
                                             {
                                                 text = dr["text"].ToString(),
                                                 value = dr["value"].ToString()
                                             }).ToList();
                    }
                    else
                    {
                        List<DropDown_list1> drplst = new List<DropDown_list1>();
                        VE.DropDown_list1 = drplst;
                    }
                    //VE = (ReportViewinHtml)Cn.EntryCommonLoading(VE, VE.PermissionID);
                    VE.DefaultView = true;
                    VE.ExitMode = 1;
                    VE.DefaultDay = 0;
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetDOC_Code(string val)
        {
            try
            {
                string doctype = "JBDN,JBCN,JBBL,SSHCD";
                if (val == null)
                {
                    return PartialView("_Help2", masterHelp.DOCCD_help(val, doctype, ""));
                }
                else
                {
                    string str = masterHelp.DOCCD_help(val, doctype, "");
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetDOC_Number(string val, string Code)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", masterHelp.DOCNO_help(val, Code));
                }
                else
                {
                    string str = masterHelp.DOCNO_help(val, Code);
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetSLCDhelp(string val)
        {
            try
            {
                string LINK_CD = "D,C";
                if (val == null)
                {
                    return PartialView("_Help2", masterHelp.SubLeg_Help(val, LINK_CD));
                }
                else
                {
                    string str = masterHelp.SubLeg_Help(val, LINK_CD);
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        [HttpPost]
        public ActionResult Rep_DnCn_Print(ReportViewinHtml VE, FormCollection FC)
        {
            try
            {
                string Scm1 = CommVar.CurSchema(UNQSNO);
                string FScm1 = CommVar.FinSchema(UNQSNO);
                string com = CommVar.Compcd(UNQSNO);
                string loc = CommVar.Loccd(UNQSNO);
                string fdate = "", tdate = "";

                string doccd = VE.DOCCD;
                string fdocno = VE.FDOCNO;
                string tdocno = VE.TDOCNO;
                if (VE.FDT != null)
                {
                    fdate = Convert.ToString(Convert.ToDateTime(FC["FDT"].ToString())).Substring(0, 10);
                }
                if (VE.TDT != null)
                {
                    tdate = Convert.ToString(Convert.ToDateTime(FC["TDT"].ToString())).Substring(0, 10);
                }
                string slcd = VE.TEXTBOX2;

                string[] copyno = new string[6];
                if (VE.Checkbox1 == true) copyno[0] = "Y"; else copyno[0] = "N";
                if (VE.Checkbox2 == true) copyno[1] = "Y"; else copyno[1] = "N";
                if (VE.Checkbox3 == true) copyno[2] = "Y"; else copyno[2] = "N";
                if (VE.Checkbox4 == true) copyno[3] = "Y"; else copyno[3] = "N";
                if (VE.Checkbox5 == true) copyno[4] = "Y"; else copyno[4] = "N";
                if (VE.Checkbox6 == true) copyno[5] = "Y"; else copyno[5] = "N";

                if (copyno[0] == "N" && copyno[1] == "N" && copyno[2] == "N" && copyno[3] == "N" && copyno[4] == "N" && copyno[5] == "N")
                {
                    copyno[0] = "Y";
                }
                string MENU_PARA = VE.MENU_PARA;


                CS = Cn.GetConnectionString();
                Cn.con = new OracleConnection(CS);
                if ((Cn.ds.Tables["fill_RECORD"] == null) == false)
                {
                    Cn.ds.Tables["fill_RECORD"].Clear();
                }
                if (Cn.con.State == ConnectionState.Closed)
                {
                    Cn.con.Open();
                }
                string sql = "", sql1 = "", sql2 = "";

                DataTable tblHSN = new DataTable();

                sql1 = "select x.autono,x.doccd,x.usr_id, x.docno,x.docdt,x.pblno,x.pbldt, x.duedays,x.duedt, ";
                sql1 += "x.slcd,x.blamt,x.docnopat, x.gstno, x.add1, x.add2, x.district, ";
                sql1 += "x.slno, x.drcr, x.glcd, x.slcd1, x.amt,  ";
                sql1 += "x.slnm,x.glnm,x.slnm1,y.authorised_by,z.checked_by from ( ";
                sql1 += "select a.autono,a.doccd,c.usr_id, a.docno,a.docdt,a.pblno,to_char(a.pbldt,'dd/mm/yyyy') pbldt,a.crdays duedays,a.duedt, ";
                sql1 += "a.slcd,a.blamt,c.docno docnopat, d.gstno, d.add1, d.add2, d.district, ";
                sql1 += "b.slno, b.drcr, b.glcd, b.slcd slcd1, b.amt,  ";
                sql1 += "d.slnm,e.glnm,f.slnm slnm1 ";
                sql1 += "from " + Scm1 + ".t_jbill a," + FScm1 + ".t_vch_det b," + Scm1 + ".t_cntrl_hdr c, ";
                sql1 += FScm1 + ".m_subleg d," + FScm1 + ".m_genleg e," + FScm1 + ".m_subleg f ";
                sql1 += "where a.autono = b.autono and a.autono = c.autono and ";
                sql1 += "a.slcd = d.slcd and b.glcd = e.glcd and b.slcd = f.slcd(+) and ";
                if (fdate.retStr() != "") sql1 += "a.docdt >= to_date('" + fdate + "','dd/mm/yyyy') and ";
                if (tdate.retStr() != "") sql1 += "a.docdt <= to_date('" + tdate + "','dd/mm/yyyy') and ";
                if (fdocno.retStr() != "") sql1 += "a.docno >= '" + fdocno + "' and ";
                if (tdocno.retStr() != "") sql1 += "a.docno <= '" + tdocno + "' and ";
                if (slcd.retStr() != "") sql1 += "a.slcd = '" + slcd + "' and ";
                sql1 += "c.compcd = '" + com + "' and c.loccd = '" + loc + "' and a.doccd = '" + doccd + "' )x, ";
                sql1 += "(select autono,usr_id authorised_by from " + Scm1 + ".t_txnstatus where ststype='A')y , ";
                sql1 += "(select autono,usr_id checked_by  from " + Scm1 + ".t_txnstatus where ststype='K')z ";
                sql1 += "where x.autono=y.autono(+) and x.autono=z.autono(+) ";
                sql1 += "order by x.autono,x.slno ";


                sql2 = "select distinct autono,doccd,usr_id, docno,docdt,pblno,pbldt,duedays,duedt,slcd,blamt, ";
                sql2 += "docnopat,slnm, add1,add2, district, gstno,authorised_by,checked_by ";
                sql2 += "from (" + sql1 + ") order by autono ";

                sql = "select b.autono, b.slno, '' glcd, a.slcd, b.drcr, b.bqnty qty, d.uom, d.good_serv, d.hsncode, b.amt, b.drcr,nvl(e.styleno,e.itnm)styleno,b.rate ";
                sql += "from " + Scm1 + ".t_jbill a," + Scm1 + ".t_jbilldtl b," + Scm1 + ".t_cntrl_hdr c, " + FScm1 + ".t_vch_gst d, " + Scm1 + ".m_sitem e ";
                sql += "where a.autono = b.autono and a.autono = c.autono and b.autono=d.autono(+) and b.slno=d.slno(+) and b.itcd=e.itcd(+) and  ";
                if (fdate.retStr() != "") sql += "a.docdt >= to_date('" + fdate + "','dd/mm/yyyy') and ";
                if (tdate.retStr() != "") sql += "a.docdt <= to_date('" + tdate + "','dd/mm/yyyy') and ";
                if (fdocno.retStr() != "") sql += "a.docno >= '" + fdocno + "' and ";
                if (tdocno.retStr() != "") sql += "a.docno <= '" + tdocno + "' and ";
                if (slcd.retStr() != "") sql += "a.slcd = '" + slcd + "' and ";
                sql += "c.compcd = '" + com + "' and c.loccd = '" + loc + "' and a.doccd = '" + doccd + "' ";
                sql += "order by autono, slno ";
                tblHSN = masterHelp.SQLquery(sql);


                DataTable TVCH = masterHelp.SQLquery(sql1);
                DataTable THDR = masterHelp.SQLquery(sql2);
                DataTable tbltax = masterHelp.SQLquery(sql);

                DataTable FREC = new DataTable();

                string docnm = ""; string docnohead = "";
                if (MENU_PARA == "JB") { docnm = "JOB PURCHASE BILL"; docnohead = "Document No."; }
                if (MENU_PARA == "SBSAL") { docnm = "TAX INVOICE"; docnohead = "Tax Invoice No."; }
                if (MENU_PARA == "MBSAL") { docnm = "TAX INVOICE"; docnohead = "Tax Invoice No."; }
                if (MENU_PARA == "DN") { docnm = "DEBIT NOTE"; docnohead = "Debit Note No."; }
                if (MENU_PARA == "CN") { docnm = "CREDIT NOTE"; docnohead = "Credit Note No."; }

                FREC.Columns.Add("Srl", typeof(double));
                FREC.Columns.Add("docno", typeof(string));
                FREC.Columns.Add("docdt", typeof(string));
                FREC.Columns.Add("pcd", typeof(string));
                FREC.Columns.Add("bill_no", typeof(string));
                FREC.Columns.Add("bill_dt", typeof(string));
                FREC.Columns.Add("jvno", typeof(string));
                FREC.Columns.Add("acode", typeof(string));
                FREC.Columns.Add("subcode", typeof(string));
                FREC.Columns.Add("acname", typeof(string));
                FREC.Columns.Add("d_amount", typeof(double));
                FREC.Columns.Add("c_amount", typeof(double));
                FREC.Columns.Add("note", typeof(string));
                FREC.Columns.Add("descn1", typeof(string));
                FREC.Columns.Add("type", typeof(string));
                FREC.Columns.Add("text", typeof(string));
                FREC.Columns.Add("tag", typeof(string));
                FREC.Columns.Add("sorder", typeof(string));
                FREC.Columns.Add("padd", typeof(string));
                FREC.Columns.Add("muser", typeof(string));
                FREC.Columns.Add("pcdname", typeof(string));
                FREC.Columns.Add("rem1", typeof(string));
                FREC.Columns.Add("recotype", typeof(string));
                FREC.Columns.Add("checked_by", typeof(string));
                FREC.Columns.Add("authorised_by", typeof(string));

                if (THDR.Rows.Count != 0)
                {
                    for (int m = 0; m <= THDR.Rows.Count - 1; m++)
                    {
                        #region HSN Details
                        string sel1 = "autono = '" + THDR.Rows[m]["autono"].ToString() + "'";
                        var rm1 = TVCH.Select(sel1);
                        //sel1 = "autono = '" + tblHSN.Rows[m]["autono"].ToString() + "'";
                        var Hs1 = tblHSN.Select(sel1);
                        for (int j = 0; j <= Hs1.Count() - 1; j++)
                        {
                            DataRow dtrow = FREC.NewRow();
                            dtrow["docno"] = rm1[0]["docnopat"];
                            dtrow["docdt"] = rm1[0]["docdt"].ToString().Substring(0, 10);
                            dtrow["pcd"] = rm1[0]["slcd"];
                            dtrow["pcdname"] = rm1[0]["slnm"];
                            dtrow["bill_no"] = rm1[0]["pblno"];
                            dtrow["bill_dt"] = rm1[0]["pbldt"];
                            dtrow["muser"] = rm1[0]["usr_id"];
                            dtrow["checked_by"] = rm1[0]["checked_by"];
                            dtrow["authorised_by"] = rm1[0]["authorised_by"];
                            //dtrow["rem1"] = rm1[0]["rem1"];
                            dtrow["padd"] = THDR.Rows[m]["district"].ToString() + (THDR.Rows[m]["gstno"] == DBNull.Value ? "" : "  [GST # " + THDR.Rows[m]["gstno"].ToString() + "]");

                            dtrow["srl"] = Hs1[j]["slno"];
                            dtrow["acode"] = Hs1[j]["glcd"];
                            dtrow["subcode"] = Hs1[j]["styleno"];
                            dtrow["recotype"] = "1";

                            string hsnnm = "";
                            //hsnnm += Hs1[j]["qty"].retDbl() == 0 ? "" : "Qty " + Hs1[j]["qty"].retDbl().ToString("0.000") + " " + Hs1[j]["uom"].retStr() + " ";
                            //hsnnm += "HSN [" + (Hs1[j]["good_serv"].retStr() == "S" ? "S" : "G") + "] " + Hs1[j]["hsncode"].retStr();
                            hsnnm += Hs1[j]["qty"].retDbl() == 0 ? "" : "Qty " + Hs1[j]["qty"].retDbl().ToString("0.000") + " @" + Hs1[j]["rate"].retDbl().ToString("0.00");
                            dtrow["acname"] = hsnnm;
                            if (Hs1[j]["drcr"].ToString() == "D") dtrow["d_amount"] = Hs1[j]["amt"];
                            else dtrow["c_amount"] = Hs1[j]["amt"];
                            FREC.Rows.Add(dtrow);
                        }
                        DataRow dtrow1 = FREC.NewRow();
                        dtrow1["docno"] = rm1[0]["docnopat"];
                        dtrow1["docdt"] = rm1[0]["docdt"].ToString().Substring(0, 10);
                        dtrow1["pcd"] = rm1[0]["slcd"];
                        dtrow1["pcdname"] = rm1[0]["slnm"];
                        dtrow1["bill_no"] = rm1[0]["pblno"];
                        dtrow1["bill_dt"] = rm1[0]["pbldt"];
                        dtrow1["muser"] = rm1[0]["usr_id"];
                        dtrow1["checked_by"] = rm1[0]["checked_by"];
                        dtrow1["authorised_by"] = rm1[0]["authorised_by"];
                        //dtrow1["rem1"] = rm1[0]["rem1"];
                        dtrow1["padd"] = THDR.Rows[m]["district"].ToString() + (THDR.Rows[m]["gstno"] == DBNull.Value ? "" : "  [GST # " + THDR.Rows[m]["gstno"].ToString() + "]");
                        dtrow1["recotype"] = "L";
                        FREC.Rows.Add(dtrow1);

                        #endregion
                        double d1 = 0, d2 = 0;
                        for (int j = 0; j <= rm1.Count() - 1; j++)
                        {
                            DataRow dtrow = FREC.NewRow();
                            dtrow["srl"] = rm1[j]["slno"];
                            dtrow["docno"] = rm1[j]["docnopat"];
                            dtrow["docdt"] = rm1[j]["docdt"].ToString().Substring(0, 10);
                            dtrow["pcd"] = rm1[j]["slcd"];
                            dtrow["bill_no"] = rm1[j]["pblno"];
                            dtrow["bill_dt"] = rm1[j]["pbldt"];
                            dtrow["acode"] = rm1[j]["glcd"];
                            dtrow["subcode"] = rm1[j]["slcd"];
                            dtrow["pcdname"] = rm1[j]["slnm"];
                            dtrow["muser"] = rm1[j]["usr_id"];
                            dtrow["checked_by"] = rm1[j]["checked_by"];
                            dtrow["authorised_by"] = rm1[j]["authorised_by"];
                            //dtrow["rem1"] = rm1[j]["rem1"];
                            dtrow["padd"] = THDR.Rows[m]["district"].ToString() + (THDR.Rows[m]["gstno"] == DBNull.Value ? "" : "  [GST # " + THDR.Rows[m]["gstno"].ToString() + "]");
                            string slnm1 = "";
                            if (rm1[j]["slnm1"].ToString() != "")
                            {
                                slnm1 = rm1[j]["slnm1"].ToString() + "  [" + rm1[j]["glnm"] + "]";
                            }
                            else
                            {
                                slnm1 = rm1[j]["glnm"].ToString();
                            }
                            dtrow["acname"] = slnm1;
                            if (rm1[j]["drcr"].ToString() == "D")
                            {
                                dtrow["d_amount"] = rm1[j]["amt"];
                                d1 = d1 + (rm1[j]["amt"]).retDbl();
                            }
                            else
                            {
                                dtrow["c_amount"] = rm1[j]["amt"];
                                d2 = d2 + (rm1[j]["amt"]).retDbl();
                            }
                            dtrow["recotype"] = "2";
                            FREC.Rows.Add(dtrow);
                        }
                    }

                }

                string repnm1 = "";
                repnm1 = "Rep_JobBill.rpt";
                string repname = VE.TEXTBOX6 == null ? repnm1 : VE.TEXTBOX6;
                repname = "~/Report/" + repname;
                string compaddress = masterHelp.retCompAddress(); //.Split(Convert.ToChar(Cn.GCS()));

                ReportDocument reportdocument = new ReportDocument();
                reportdocument.Load(Server.MapPath(repname));
                reportdocument.SetDataSource(FREC);
                reportdocument.SetParameterValue("complogo", masterHelp.retCompLogo());
                reportdocument.SetParameterValue("complogo1", masterHelp.retCompLogo1());
                reportdocument.SetParameterValue("billheading", docnm);
                reportdocument.SetParameterValue("compnm", compaddress.retCompValue("compnm"));
                reportdocument.SetParameterValue("compadd", compaddress.retCompValue("compadd"));
                reportdocument.SetParameterValue("compcommu", compaddress.retCompValue("compcommu"));
                reportdocument.SetParameterValue("compstat", compaddress.retCompValue("compstat"));
                reportdocument.SetParameterValue("locaadd", compaddress.retCompValue("locaadd"));
                reportdocument.SetParameterValue("locacommu", compaddress.retCompValue("locacommu"));
                reportdocument.SetParameterValue("locastat", compaddress.retCompValue("locastat"));
                reportdocument.SetParameterValue("legalname", compaddress.retCompValue("legalname"));
                reportdocument.SetParameterValue("corpadd", compaddress.retCompValue("corpadd"));
                reportdocument.SetParameterValue("corpcommu", compaddress.retCompValue("corpcommu"));
                //new
                Response.Buffer = false;
                Response.ClearContent();
                Response.ClearHeaders();
                Stream stream = reportdocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                reportdocument.Close(); reportdocument.Dispose(); GC.Collect();
                stream.Seek(0, SeekOrigin.Begin);
                return new FileStreamResult(stream, "application/pdf");
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + " " + ex.InnerException);
            }
        }
        #region OLD
        //public ActionResult Rep_DnCn_Print(ReportViewinHtml VE, FormCollection FC, string submitbutton)
        //{
        //    try
        //    {
        //        ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
        //        ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
        //        ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));

        //        var printemail = submitbutton.ToString();
        //        string fdate = "", tdate = "";
        //        if (VE.FDT != null)
        //        {
        //            fdate = Convert.ToString(Convert.ToDateTime(FC["FDT"].ToString())).Substring(0, 10);
        //        }
        //        if (VE.TDT != null)
        //        {
        //            tdate = Convert.ToString(Convert.ToDateTime(FC["TDT"].ToString())).Substring(0, 10);
        //        }
        //        string fdocno = VE.FDOCNO;
        //        string tdocno = VE.TDOCNO;
        //        string doccd = VE.DOCCD;
        //        string docnm = DB.M_DOCTYPE.Find(doccd).DOCNM;
        //        string slcd = VE.TEXTBOX2;

        //        string[] copyno = new string[6];
        //        if (VE.Checkbox1 == true) copyno[0] = "Y"; else copyno[0] = "N";
        //        if (VE.Checkbox2 == true) copyno[1] = "Y"; else copyno[1] = "N";
        //        if (VE.Checkbox3 == true) copyno[2] = "Y"; else copyno[2] = "N";
        //        if (VE.Checkbox4 == true) copyno[3] = "Y"; else copyno[3] = "N";
        //        if (VE.Checkbox5 == true) copyno[4] = "Y"; else copyno[4] = "N";
        //        if (VE.Checkbox6 == true) copyno[5] = "Y"; else copyno[5] = "N";

        //        if (copyno[0] == "N" && copyno[1] == "N" && copyno[2] == "N" && copyno[3] == "N" && copyno[4] == "N" && copyno[5] == "N")
        //        {
        //            copyno[0] = "Y";
        //        }

        //        string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), Scm1 = CommVar.CurSchema(UNQSNO), Scmf = CommVar.FinSchema(UNQSNO);
        //        string str1 = "";
        //        DataTable rsTmp;
        //        string doctype = "";
        //        str1 = "select doctype from " + Scm1 + ".m_doctype where doccd='" + VE.DOCCD + "'";
        //        rsTmp = masterHelp.SQLquery(str1);
        //        doctype = rsTmp.Rows[0]["doctype"].ToString();

        //        string prnemailid = "";
        //        if (VE.TEXTBOX5 != null) prnemailid = "'" + VE.TEXTBOX5 + "' regemailid"; else prnemailid = "a.regemailid";
        //        if (VE.TEXTBOX5 != null) prnemailid = "'" + VE.TEXTBOX5 + "' regemailid"; else prnemailid = "e.regemailid";

        //        string sql = "", sqlc = "";
        //        sql = "";

        //        sqlc = "";
        //        sqlc += "f.compcd='" + COM + "' and f.loccd='" + LOC + "' and ";
        //        if (fdocno.retStr() != "") sqlc += "f.doconlyno >= '" + fdocno + "' and ";
        //        if (tdocno.retStr() != "") sqlc += "f.doconlyno <= '" + tdocno + "' and ";
        //        if (fdate != "") sqlc += "f.docdt >= to_date('" + fdate + "','dd/mm/yyyy') and ";
        //        if (tdate != "") sqlc += "f.docdt <= to_date('" + tdate + "','dd/mm/yyyy') and ";
        //        if (slcd != null) sqlc += "a.slcd='" + slcd + "' and ";
        //        sqlc += "f.doccd = '" + doccd + "' ";

        //        sql += "select a.autono,a.slcd, a.doctag, a.pblno, a.pbldt,a.blamt,a.roamt, b.itcd, b.rate, b.qntycalcon, b.drcr,nvl(nvl(b.dncnqnty,0),nvl(b.bqnty,0)) qnty, b.amt, b.discper,nvl( b.discamt,0) discamt, ";
        //        sql += "b.igstper, b.igstamt, b.cgstper, b.cgstamt, b.sgstper, b.sgstamt,b.igstper+b.cgstper+b.sgstper gstper, b.cessper, b.cessamt, b.taxableval, b.itrem,	";
        //        sql += "c.itnm, c.uomcd, c.hsnsaccd, d.uomnm, d.decimals qdecimal, e.slnm, e.gstno, e.panno, e.statecd, e.state, a.duedt, a.crdays, ";
        //        sql += "e.add1 sladd1, e.add2 sladd2, e.add3 sladd3, e.add4 sladd4, e.add5 sladd5, e.add6 sladd6, e.add7 sladd7,e.phno1 phno,e.country, ";
        //        sql += "f.usr_id,f.canc_rem, f.cancel,f.docno,a.doccd,a.docdt ";
        //        sql += "from " + Scm1 + ".t_jbill a, " + Scm1 + ".t_jbilldtl b, " + Scm1 + ".m_sitem c, " + Scmf + ".m_uom d, " + Scmf + ".m_subleg e , " + Scm1 + ".t_cntrl_hdr f ";
        //        sql += "where a.autono = b.autono(+) and b.itcd = c.itcd(+) and c.uomcd = d.uomcd(+) and a.slcd = e.slcd(+) and a.autono=f.autono and a.doctag in ('JD','JC','JS') and ";
        //        sql += sqlc;

        //        DataTable tbl = new DataTable();
        //        tbl = masterHelp.SQLquery(sql);
        //        if (tbl.Rows.Count == 0) return RedirectToAction("NoRecords", "RPTViewer", new { errmsg = "Records not found !!" });

        //        //DataTable rsStkPrcDesc;
        //        //sql = "";
        //        //sql += "select distinct a.autono, e.prcdesc stkprcdesc ";
        //        //sql += "from " + Scm1 + ".t_batchdtl a, " + Scm1 + ".t_batchmst d, " + Scm1 + ".t_jbill b, " + Scm1 + ".m_itemplist e, " + Scm1 + ".t_cntrl_hdr c ";
        //        //sql += "where a.batchautono=d.batchautono(+) and d.itmprccd=e.itmprccd(+) and a.autono=b.autono(+) and ";
        //        //sql += sqlc;
        //        //sql += "a.autono=c.autono ";
        //        //rsStkPrcDesc = masterHelp.SQLquery(sql);

        //        string blterms = "", inspoldesc = "", dealsin = "";
        //        //sql = "select blterms, inspoldesc, dealsin from " + Scm1 + ".m_mgroup_spl where compcd='" + CommVar.Compcd(UNQSNO) + "' and itgrpcd='" + tbl.Rows[0]["itgrpcd"].ToString() + "'";
        //        //DataTable rsMgroupSpl = masterHelp.SQLquery(sql);
        //        //if (rsMgroupSpl.Rows.Count > 0)
        //        //{
        //        //    if (rsMgroupSpl.Rows[0]["blterms"].ToString() != "") blterms = rsMgroupSpl.Rows[0]["blterms"].ToString();
        //        //    inspoldesc = rsMgroupSpl.Rows[0]["inspoldesc"].ToString();
        //        //    dealsin = rsMgroupSpl.Rows[0]["dealsin"].ToString();
        //        //}

        //        #region  Datatabe IR generate
        //        DataTable IR = new DataTable();
        //        IR.Columns.Add("rateqntybag", typeof(string), "");
        //        IR.Columns.Add("regemailid", typeof(string), "");
        //        IR.Columns.Add("autono", typeof(string), "");
        //        IR.Columns.Add("copymode", typeof(string), "");
        //        IR.Columns.Add("docno", typeof(string), "");
        //        IR.Columns.Add("docdt", typeof(string), "");
        //        IR.Columns.Add("areacd", typeof(string), "");
        //        IR.Columns.Add("invisstime", typeof(double), "");
        //        IR.Columns.Add("duedays", typeof(double), "");
        //        IR.Columns.Add("itmprccd", typeof(string), "");
        //        IR.Columns.Add("itmprcdesc", typeof(string), "");
        //        IR.Columns.Add("prceffdt", typeof(string), "");
        //        IR.Columns.Add("stkprcdesc", typeof(string), "");
        //        IR.Columns.Add("gocd", typeof(string), "");
        //        IR.Columns.Add("gonm", typeof(string), "");
        //        IR.Columns.Add("goadd1", typeof(string), "");
        //        IR.Columns.Add("weekno", typeof(double), "");
        //        IR.Columns.Add("slcd", typeof(string), "");
        //        IR.Columns.Add("partycd", typeof(string), "");
        //        IR.Columns.Add("slnm", typeof(string), "");
        //        IR.Columns.Add("sladd1", typeof(string), "");
        //        IR.Columns.Add("sladd2", typeof(string), "");
        //        IR.Columns.Add("sladd3", typeof(string), "");
        //        IR.Columns.Add("sladd4", typeof(string), "");
        //        IR.Columns.Add("sladd5", typeof(string), "");
        //        IR.Columns.Add("sladd6", typeof(string), "");
        //        IR.Columns.Add("sladd7", typeof(string), "");
        //        IR.Columns.Add("sladd8", typeof(string), "");
        //        IR.Columns.Add("sladd9", typeof(string), "");
        //        IR.Columns.Add("cpartycd", typeof(string), "");
        //        IR.Columns.Add("ordrefno", typeof(string), "");
        //        IR.Columns.Add("ordrefdt", typeof(string), "");
        //        IR.Columns.Add("trslcd", typeof(string), "");
        //        IR.Columns.Add("trslnm", typeof(string), "");
        //        IR.Columns.Add("trgst", typeof(string), "");
        //        IR.Columns.Add("lrno", typeof(string), "");
        //        IR.Columns.Add("lrdt", typeof(string), "");
        //        IR.Columns.Add("lorryno", typeof(string), "");
        //        IR.Columns.Add("ewaybillno", typeof(string), "");
        //        IR.Columns.Add("grwt", typeof(double), "");
        //        IR.Columns.Add("ntwt", typeof(double), "");
        //        IR.Columns.Add("caltype", typeof(double), "");
        //        IR.Columns.Add("slno", typeof(double), "");
        //        IR.Columns.Add("itcd", typeof(string), "");
        //        IR.Columns.Add("itnm", typeof(string), "");
        //        IR.Columns.Add("itrem", typeof(string), "");
        //        IR.Columns.Add("itdesc", typeof(string), "");
        //        IR.Columns.Add("batchdtl", typeof(string), "");
        //        IR.Columns.Add("packsize", typeof(double), "");
        //        IR.Columns.Add("hsnsaccd", typeof(string), "");
        //        IR.Columns.Add("prodcd", typeof(string), "");
        //        IR.Columns.Add("nos", typeof(double), "");
        //        IR.Columns.Add("qnty", typeof(double), "");
        //        IR.Columns.Add("uomnm", typeof(string), "");
        //        IR.Columns.Add("qdecimal", typeof(double), "");
        //        IR.Columns.Add("rate", typeof(double), "");
        //        IR.Columns.Add("rateuomnm", typeof(string), "");
        //        IR.Columns.Add("basamt", typeof(double), "");
        //        IR.Columns.Add("stddisc", typeof(string), "");
        //        IR.Columns.Add("stddiscamt", typeof(double), "");
        //        IR.Columns.Add("disc", typeof(string), "");
        //        IR.Columns.Add("discamt", typeof(double), "");
        //        IR.Columns.Add("taxableval", typeof(string), "");
        //        IR.Columns.Add("amt", typeof(string), "");
        //        IR.Columns.Add("cgstdsp", typeof(string), "");
        //        IR.Columns.Add("cgstper", typeof(double), "");
        //        IR.Columns.Add("cgstamt", typeof(double), "");
        //        IR.Columns.Add("sgstper", typeof(double), "");
        //        IR.Columns.Add("sgstamt", typeof(double), "");
        //        IR.Columns.Add("cessper", typeof(double), "");
        //        IR.Columns.Add("cessamt", typeof(double), "");
        //        IR.Columns.Add("gstper", typeof(double), "");
        //        IR.Columns.Add("discper", typeof(double), "");
        //        IR.Columns.Add("revchrg", typeof(string), "");
        //        IR.Columns.Add("rupinword", typeof(string), "");
        //        IR.Columns.Add("netamt", typeof(double), "");
        //        IR.Columns.Add("roamt", typeof(double), "");
        //        IR.Columns.Add("blamt", typeof(string), "");
        //        IR.Columns.Add("tcsper", typeof(double), "");
        //        IR.Columns.Add("tcsamt", typeof(double), "");
        //        IR.Columns.Add("blremarks", typeof(string), "");
        //        IR.Columns.Add("agstdocno", typeof(string), "");
        //        IR.Columns.Add("agstdocdt", typeof(string), "");
        //        IR.Columns.Add("user_id", typeof(string), "");
        //        IR.Columns.Add("cancel", typeof(string), "");
        //        IR.Columns.Add("canc_rem", typeof(string), "");

        //        IR.Columns.Add("makenm", typeof(string), "");
        //        IR.Columns.Add("bltophead", typeof(string), "");

        //        IR.Columns.Add("nopkgs", typeof(string), "");
        //        IR.Columns.Add("docth", typeof(string), "");
        //        IR.Columns.Add("transgst", typeof(string), "");
        //        IR.Columns.Add("agentnm", typeof(string), "");
        //        IR.Columns.Add("trsladd1", typeof(string), "");
        //        IR.Columns.Add("trsladd2", typeof(string), "");
        //        IR.Columns.Add("trsladd3", typeof(string), "");
        //        IR.Columns.Add("trsladd4", typeof(string), "");
        //        IR.Columns.Add("payterms", typeof(string), "");

        //        IR.Columns.Add("bankactno", typeof(string), "");
        //        IR.Columns.Add("bankname", typeof(string), "");
        //        IR.Columns.Add("bankbranch", typeof(string), "");
        //        IR.Columns.Add("bankifsc", typeof(string), "");
        //        IR.Columns.Add("bankadd", typeof(string), "");
        //        IR.Columns.Add("bankrtgs", typeof(string), "");

        //        IR.Columns.Add("duedt", typeof(string), "");
        //        IR.Columns.Add("mrp", typeof(double), "");
        //        IR.Columns.Add("poslno", typeof(string), "");
        //        IR.Columns.Add("plsupply", typeof(string), "");
        //        IR.Columns.Add("destn", typeof(string), "");
        //        IR.Columns.Add("dtldsc", typeof(string), "");
        //        IR.Columns.Add("dtlamt", typeof(string), "");
        //        IR.Columns.Add("mtrlcd", typeof(string), "");
        //        IR.Columns.Add("bas_rate", typeof(string), "");
        //        IR.Columns.Add("pv_per", typeof(string), "");
        //        IR.Columns.Add("insudesc", typeof(string), "");
        //        IR.Columns.Add("dealsin", typeof(string), "");
        //        IR.Columns.Add("blterms", typeof(string), "");

        //        IR.Columns.Add("hsn_cd", typeof(string), "");
        //        IR.Columns.Add("hsn_hddsp1", typeof(string), "");
        //        IR.Columns.Add("hsn_hddsp2", typeof(string), "");
        //        IR.Columns.Add("hsn_txblval", typeof(string), "");
        //        IR.Columns.Add("hsn_gstper1", typeof(string), "");
        //        IR.Columns.Add("hsn_gstamt1", typeof(string), "");
        //        IR.Columns.Add("hsn_gstper2", typeof(string), "");
        //        IR.Columns.Add("hsn_gstamt2", typeof(string), "");
        //        IR.Columns.Add("hsn_gstper3", typeof(string), "");
        //        IR.Columns.Add("hsn_gstamt3", typeof(string), "");
        //        IR.Columns.Add("hsn_cessamt", typeof(string), "");
        //        IR.Columns.Add("hsn_gstamt", typeof(string), "");
        //        IR.Columns.Add("hsn_qnty", typeof(string), "");
        //        IR.Columns.Add("hsn_tqnty", typeof(string), "");
        //        IR.Columns.Add("hsn_ttxblval", typeof(string), "");
        //        IR.Columns.Add("hsn_tgstamt1", typeof(string), "");
        //        IR.Columns.Add("hsn_tgstamt2", typeof(string), "");
        //        IR.Columns.Add("hsn_tgstamt3", typeof(string), "");
        //        IR.Columns.Add("totalosamt", typeof(string), "");
        //        #endregion

        //        string bankname = "", bankactno = "", bankbranch = "", bankifsc = "", bankadd = "", bankrtgs = "";
        //        sql = "";
        //        sql += "select a.bankname, a.bankactno, a.ifsccode, a.address, a.branch ";
        //        sql += "from " + Scmf + ".m_loca_ifsc a ";
        //        sql += "where a.compcd = '" + COM + "' and a.loccd = '" + LOC + "' and a.defltbank = 'T' ";
        //        DataTable rsbank = masterHelp.SQLquery(sql);
        //        if (rsbank.Rows.Count > 0)
        //        {
        //            bankrtgs += "You can Make RTGS/NEFT to " + rsbank.Rows[0]["bankname"] + " ( IFSC-" + rsbank.Rows[0]["ifsccode"] + " ) A/c No-" + rsbank.Rows[0]["bankactno"];
        //            if (rsbank.Rows[0]["address"].ToString() != "") bankrtgs += " Address - " + rsbank.Rows[0]["address"];
        //            bankrtgs += ".";

        //            bankname = rsbank.Rows[0]["bankname"].ToString();
        //            bankactno = rsbank.Rows[0]["bankactno"].ToString();
        //            bankifsc = rsbank.Rows[0]["ifsccode"].ToString();
        //            bankbranch = rsbank.Rows[0]["branch"].ToString();
        //            bankadd = rsbank.Rows[0]["address"].ToString();
        //        }

        //        int maxR = tbl.Rows.Count - 1;
        //        Int32 i = 0; int istore = 0; int lslno = 0; int ilast = 0;
        //        string auto1 = ""; string copymode = ""; string blrem = ""; string itdsc = "";
        //        string blhead = ""; string fssailicno = ""; string grpemailid = ""; string goadd = "", gocd = "";
        //        string rupinwords = "";
        //        int uomdecimal = 3; int uommaxdecimal = 0;

        //        switch (tbl.Rows[0]["doctag"].ToString())
        //        {
        //            case "JC":
        //                blhead = "CREDIT NOTE"; break;
        //            case "JD":
        //                blhead = "DEBIT NOTE"; break;
        //            case "JS":
        //                blhead = "DEBIT NOTE (SHORTAGE)"; break;
        //            default: blhead = ""; break;
        //        }

        //        Int16 maxCopy = 5;

        //        while (i <= maxR)
        //        {
        //            //fssailicno = tbl.Rows[i]["fssailicno"].ToString();
        //            //grpemailid = tbl.Rows[i]["grpemailid"].ToString();
        //            //gocd = tbl.Rows[i]["gocd"].ToString();
        //            //goadd = tbl.Rows[i]["goadd1"].ToString() + " " + tbl.Rows[i]["goadd2"].ToString() + " " + tbl.Rows[i]["goadd3"].ToString();
        //            //if (tbl.Rows[i]["gophno"].ToString() != "") goadd = goadd + " Phone : " + tbl.Rows[i]["gophno"].ToString();
        //            istore = i;

        //            for (int ic = 0; ic <= maxCopy; ic++)
        //            {
        //                i = istore;
        //                lslno = 0;
        //                auto1 = tbl.Rows[i]["autono"].ToString();
        //                double dbasamt = 0; double ddisc1 = 0; double ddisc2 = 0; double dtxblval = 0; double damt = 0;
        //                double dcgstamt = 0; double dsgstamt = 0; double dnetamt = 0; double dnos = 0; double dqnty = 0;
        //                bool doctotprint = false; bool totalreadyprint = false; bool delvchrg = false;

        //                string dtldsc = "", dtlamt = "";
        //                double tqnty = 0, tamt = 0, tgst = 0, blamt = 0, totalosamt = 0;
        //                string hsnqnty = "", hsntaxblval = "", hsngstper1 = "", hsngstper2 = "", hsngstper3 = "", hsngstamt1 = "", hsngstamt2 = "", hsngstamt3 = "", hsncode = "";
        //                double gstper1 = 0, gstamt1 = 0, total_qnty = 0, total_taxval = 0, total_gstamt1 = 0, total_gstamt2 = 0, total_gstamt3 = 0;
        //                bool flagi = false, flagc = false, flags = false;

        //                if (copyno[ic].ToString() != "N")
        //                {
        //                    rupinwords = Cn.AmountInWords(tbl.Rows[i]["blamt"].ToString());
        //                    string oslcd = "", oglcd = "", odocdt = "", oclass1cd = "";

        //                    //if (doctype == "SBILL")
        //                    //{
        //                    //    oslcd = tbl.Rows[i]["oslcd"].ToString();
        //                    //    oglcd = tbl.Rows[i]["debglcd"].ToString();
        //                    //    odocdt = Convert.ToDateTime(tbl.Rows[i]["docdt"].ToString()).AddDays(-1).ToString().retDateStr();
        //                    //    totalosamt = Convert.ToDouble(MasterHelpFa.slcdbal(oslcd, oglcd, odocdt, oclass1cd));
        //                    //    oslcd.retStr();

        //                    //    sql = "";
        //                    //    sql += "select sum(a.blamt) blamt from ( ";
        //                    //    sql += "select nvl(b.pslcd,a.slcd) oslcd, sum(a.blamt) blamt ";
        //                    //    sql += "from " + Scm1 + ".t_jbill a, " + Scmf + ".m_subleg b, " + Scm1 + ".t_cntrl_hdr c, " + Scm1 + ".m_doctype d ";
        //                    //    sql += "where a.autono=c.autono and c.doccd=d.doccd and a.slcd=b.slcd and nvl(c.cancel,'N')='N' and c.compcd='" + COM + "' and ";
        //                    //    sql += "c.docdt=to_date('" + tbl.Rows[i]["docdt"].ToString().retDateStr() + "','dd/mm/yyyy') and ";
        //                    //    sql += "d.doctype='SBILL' and c.vchrno <= " + Convert.ToDouble(tbl.Rows[i]["vchrno"]) + " and c.doccd='" + doccd + "' ";
        //                    //    sql += "group by nvl(b.pslcd,a.slcd) ) a ";
        //                    //    rsTmp = MasterHelpFa.SQLquery(sql);
        //                    //    if (rsTmp.Rows.Count > 0) totalosamt = totalosamt + Convert.ToDouble(rsTmp.Rows[0]["blamt"]);
        //                    //}

        //                    //Type A_T = tbl.Rows[0]["basamt"].GetType(); 
        //                    Type Q_T = tbl.Rows[0]["qnty"].GetType(); Type I_T = tbl.Rows[0]["igstamt"].GetType();
        //                    Type C_T = tbl.Rows[0]["cgstamt"].GetType(); Type S_T = tbl.Rows[0]["sgstamt"].GetType();

        //                    var GST_DATA = (from DataRow DR in tbl.Rows
        //                                    where DR["autono"].ToString() == auto1
        //                                    group DR by new { IGST = DR["igstper"].ToString(), CGST = DR["cgstper"].ToString(), SGST = DR["sgstper"].ToString() } into X
        //                                    select new
        //                                    {
        //                                        IGSTPER = X.Key.IGST,
        //                                        CGSTPER = X.Key.CGST,
        //                                        SGSTPER = X.Key.SGST,
        //                                        //TAMT = A_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("basamt")) : Convert.ToDouble(X.Sum(Z => Z.Field<decimal>("basamt"))),
        //                                        TQNTY = Q_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("qnty")) : Convert.ToDouble(X.Sum(Z => Z.Field<decimal>("qnty"))),
        //                                        IGSTAMT = I_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("igstamt")) : Convert.ToDouble(X.Sum(Z => Z.Field<decimal>("igstamt"))),
        //                                        CGSTAMT = C_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("cgstamt")) : Convert.ToDouble(X.Sum(Z => Z.Field<decimal>("cgstamt"))),
        //                                        SGSTAMT = S_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("sgstamt")) : Convert.ToDouble(X.Sum(Z => Z.Field<decimal>("sgstamt"))),
        //                                        TOTALPER = Convert.ToDouble(X.Key.IGST) + Convert.ToDouble(X.Key.CGST) + Convert.ToDouble(X.Key.SGST)
        //                                    }).OrderBy(A => A.TOTALPER).ToList();

        //                    if (GST_DATA != null && GST_DATA.Count > 0)
        //                    {
        //                        foreach (var k in GST_DATA)
        //                        {
        //                            if (k.IGSTAMT != 0) { dtldsc += "(+) IGST @ " + Cn.Indian_Number_format(k.IGSTPER, "0.00") + " %~"; dtlamt += Convert.ToDouble(k.IGSTAMT).ToINRFormat() + "~"; }
        //                            if (k.CGSTAMT != 0) { dtldsc += "(+) CGST @ " + Cn.Indian_Number_format(k.CGSTPER, "0.00") + " %~"; dtlamt += Convert.ToDouble(k.CGSTAMT).ToINRFormat() + "~"; }
        //                            if (k.SGSTAMT != 0) { dtldsc += "(+) SGST @ " + Cn.Indian_Number_format(k.SGSTPER, "0.00") + " %~"; dtlamt += Convert.ToDouble(k.SGSTAMT).ToINRFormat() + "~"; }
        //                            tqnty = tqnty + Convert.ToDouble(k.TQNTY);
        //                            //tamt = tamt + Convert.ToDouble(k.TAMT);
        //                            tgst = tgst + Convert.ToDouble(k.IGSTAMT) + Convert.ToDouble(k.CGSTAMT) + Convert.ToDouble(k.SGSTAMT);
        //                        }
        //                    }

        //                    string breakpoint = "";

        //                    var HSN_DATA = (from a in DBF.T_VCH_GST
        //                                    where a.AUTONO == auto1
        //                                    group a by new { HSNCODE = a.HSNCODE, IGSTPER = a.IGSTPER, CGSTPER = a.CGSTPER, SGSTPER = a.SGSTPER } into x
        //                                    select new
        //                                    {

        //                                        HSNCODE = x.Key.HSNCODE,
        //                                        IGSTPER = x.Key.IGSTPER,
        //                                        CGSTPER = x.Key.CGSTPER,
        //                                        SGSTPER = x.Key.SGSTPER,
        //                                        TIGSTAMT = x.Sum(s => s.IGSTAMT),
        //                                        TCGSTAMT = x.Sum(s => s.CGSTAMT),
        //                                        TSGSTAMT = x.Sum(s => s.SGSTAMT),
        //                                        TAMT = x.Sum(s => s.AMT),
        //                                        TQNTY = x.Sum(s => s.QNTY),
        //                                        //DECIMAL = (from z in DBF.M_UOM
        //                                        //           where z.UOMCD == (from y in DBF.T_VCH_GST where y.AUTONO == auto1 select y.UOM).FirstOrDefault()
        //                                        //           select z.DECIMALS).FirstOrDefault()
        //                                        //DECIMALS = (from c in DBF.M_UOM where c.UOMCD ==  select c.DECIMALS)
        //                                    }).ToList();

        //                    if (HSN_DATA != null && HSN_DATA.Count > 0)
        //                    {
        //                        foreach (var k in HSN_DATA)
        //                        {
        //                            var uom = (from a in DBF.T_VCH_GST
        //                                       where a.AUTONO == auto1 && a.IGSTPER == k.IGSTPER && a.CGSTPER == k.CGSTPER
        //                              && a.SGSTPER == k.SGSTPER && a.HSNCODE == k.HSNCODE
        //                                       select a.UOM).FirstOrDefault();
        //                            double DECIMAL = 0; string umnm = "";
        //                            var uomdata = DBF.M_UOM.Find(uom);
        //                            DECIMAL = Convert.ToDouble(uomdata.DECIMALS);
        //                            umnm = uomdata.UOMNM;
        //                            if (k.TIGSTAMT > 0) flagi = true;
        //                            if (k.TCGSTAMT > 0) flagc = true;

        //                            gstper1 = Convert.ToDouble(k.CGSTPER) + Convert.ToDouble(k.IGSTPER);
        //                            gstamt1 = Convert.ToDouble(k.TCGSTAMT) + Convert.ToDouble(k.TIGSTAMT);

        //                            if (k.HSNCODE != "") { hsncode += k.HSNCODE + "~"; }
        //                            if (k.TQNTY != 0) { hsnqnty += Convert.ToDouble(k.TQNTY).ToString("n" + DECIMAL.ToString()) + " " + umnm + "~"; }
        //                            if (k.TCGSTAMT + k.TIGSTAMT != 0)
        //                            {
        //                                if (k.IGSTPER != 0) hsngstper1 += Cn.Indian_Number_format(k.IGSTPER.ToString(), "0.00") + " %~";
        //                                if (k.TIGSTAMT != 0) hsngstamt1 += Convert.ToDouble(k.TIGSTAMT).ToINRFormat() + "~";
        //                            }
        //                            if (k.TCGSTAMT + k.TCGSTAMT != 0)
        //                            {
        //                                if (k.CGSTPER != 0) hsngstper2 += Cn.Indian_Number_format(k.CGSTPER.ToString(), "0.00") + " %~";
        //                                if (k.TCGSTAMT != 0) hsngstamt2 += Convert.ToDouble(k.TCGSTAMT).ToINRFormat() + "~";
        //                            }
        //                            if (k.TSGSTAMT != 0)
        //                            {
        //                                flags = true;
        //                                if (k.SGSTPER != 0) hsngstper3 += Cn.Indian_Number_format(k.SGSTPER.ToString(), "0.00") + " %~";
        //                                if (k.TSGSTAMT != 0) hsngstamt3 += Convert.ToDouble(k.TSGSTAMT).ToINRFormat() + "~";
        //                            }
        //                            if (k.TAMT != 0) { hsntaxblval += Convert.ToDouble(k.TAMT).ToINRFormat() + "~"; }

        //                            total_qnty = total_qnty + Convert.ToDouble(k.TQNTY);
        //                            total_taxval = total_taxval + Convert.ToDouble(k.TAMT);
        //                            total_gstamt1 = total_gstamt1 + Convert.ToDouble(k.TIGSTAMT);
        //                            total_gstamt2 = total_gstamt2 + Convert.ToDouble(k.TCGSTAMT);
        //                            total_gstamt3 = total_gstamt3 + Convert.ToDouble(k.TSGSTAMT);
        //                        }
        //                    }
        //                }

        //                while (tbl.Rows[i]["autono"].ToString() == auto1)
        //                {
        //                    if (copyno[ic].ToString() == "N")
        //                    {
        //                        i = i + 1;
        //                        break;
        //                    }
        //                    switch (ic)
        //                    {
        //                        case 0:
        //                            copymode = "ORIGINAL FOR RECIPIENT"; break;
        //                        case 1:
        //                            copymode = "DUPLICATE FOR TRANSPORTER"; break;
        //                        case 2:
        //                            copymode = "TRIPLICATE FOR SUPPLIER"; break;
        //                        case 3:
        //                            copymode = "EXTRA COPY"; break;
        //                        case 4:
        //                            copymode = "EXTRA COPY"; break;
        //                        case 5:
        //                            copymode = "EXTRA COPY"; break;
        //                        default: copymode = ""; break;
        //                    }

        //                    DataRow dr1 = IR.NewRow();
        //                docstart:
        //                    double duedays = 0;
        //                    string payterms = "";
        //                    //duedays = tbl.Rows[i]["duedays"] == DBNull.Value ? 0 : Convert.ToDouble(tbl.Rows[i]["duedays"]);
        //                    //payterms = tbl.Rows[i]["payterms"].ToString();
        //                    if (payterms == "")
        //                    {
        //                        if (duedays == 0) payterms = ""; else payterms = duedays.ToString() + " days.";
        //                    }

        //                    dr1["autono"] = auto1 + ic.ToString();
        //                    dr1["user_id"] = tbl.Rows[i]["usr_id"].ToString();
        //                    dr1["cancel"] = tbl.Rows[i]["cancel"].ToString();
        //                    dr1["canc_rem"] = tbl.Rows[i]["canc_rem"].ToString();
        //                    dr1["copymode"] = copymode;
        //                    dr1["docno"] = tbl.Rows[i]["docno"].ToString();
        //                    dr1["docdt"] = tbl.Rows[i]["docdt"] == DBNull.Value ? "" : tbl.Rows[i]["docdt"].ToString().Substring(0, 10).ToString();
        //                    dr1["areacd"] = "";
        //                    dr1["invisstime"] = Convert.ToDouble(0);
        //                    dr1["duedays"] = duedays;
        //                    dr1["itmprccd"] = "";
        //                    dr1["itmprcdesc"] = "";
        //                    dr1["prceffdt"] = "";
        //                    dr1["duedt"] = Convert.ToDateTime(tbl.Rows[i]["docdt"].ToString()).AddDays(duedays).ToString().retDateStr();
        //                    dr1["payterms"] = payterms;
        //                    //if (rsStkPrcDesc.Rows.Count > 0  && tbl.Rows[i]["doccd"].ToString() == "SGS")
        //                    //if (rsStkPrcDesc.Rows.Count > 0 && tbl.Rows[i]["itgrpcd"].ToString() == "G001")
        //                    //{
        //                    //    var DATA = (from DataRow DR in rsStkPrcDesc.Rows where DR["autono"].ToString() == auto1 select DR["stkprcdesc"].ToString()).ToList();
        //                    //    if (DATA.Count > 0) dr1["stkprcdesc"] = DATA[0];
        //                    //}
        //                    dr1["gocd"] = "";
        //                    dr1["gonm"] = "";
        //                    dr1["goadd1"] = "";
        //                    dr1["weekno"] = Convert.ToDouble(0);

        //                    dr1["slcd"] = tbl.Rows[i]["slcd"].ToString();
        //                    //if (tbl.Rows[i]["partycd"].ToString() != "") dr1["partycd"] = "SAP - " + tbl.Rows[i]["partycd"].ToString();
        //                    dr1["slnm"] = tbl.Rows[i]["slnm"].ToString();
        //                    //dr1["regemailid"] = tbl.Rows[i]["regemailid"].ToString();
        //                    string cfld = "", rfld = ""; int rf = 0;
        //                    for (int f = 1; f <= 6; f++)
        //                    {
        //                        cfld = "sladd" + Convert.ToString(f).ToString();
        //                        if (tbl.Rows[i][cfld].ToString() != "")
        //                        {
        //                            rf = rf + 1;
        //                            rfld = "sladd" + Convert.ToString(rf);
        //                            dr1[rfld] = tbl.Rows[i][cfld].ToString();
        //                        }
        //                    }
        //                    rf = rf + 1;
        //                    rfld = "sladd" + Convert.ToString(rf);
        //                    dr1[rfld] = tbl.Rows[i]["state"].ToString() + " [" + tbl.Rows[i]["statecd"].ToString() + "]";
        //                    if (tbl.Rows[i]["gstno"].ToString() != "")
        //                    {
        //                        rf = rf + 1;
        //                        rfld = "sladd" + Convert.ToString(rf);
        //                        dr1[rfld] = "GST # " + tbl.Rows[i]["gstno"].ToString();
        //                    }
        //                    if (tbl.Rows[i]["panno"].ToString() != "")
        //                    {
        //                        rf = rf + 1;
        //                        rfld = "sladd" + Convert.ToString(rf);
        //                        dr1[rfld] = "PAN # " + tbl.Rows[i]["panno"].ToString();
        //                    }
        //                    if (tbl.Rows[i]["phno"].ToString() != "")
        //                    {
        //                        rf = rf + 1;
        //                        rfld = "sladd" + Convert.ToString(rf);
        //                        dr1[rfld] = "Ph. # " + tbl.Rows[i]["phno"].ToString();
        //                    }
        //                    dr1["grwt"] = Convert.ToDouble(0);
        //                    dr1["ntwt"] = Convert.ToDouble(0);


        //                    dr1["revchrg"] = "N";
        //                    dr1["roamt"] = tbl.Rows[i]["roamt"] == DBNull.Value ? 0 : Convert.ToDouble(tbl.Rows[i]["roamt"]);
        //                    dr1["tcsper"] = 0;
        //                    dr1["tcsamt"] = 0;
        //                    dr1["blamt"] = tbl.Rows[i]["blamt"].ToString().retDbl().ToINRFormat(); // == DBNull.Value ? 0 : Convert.ToDouble(tbl.Rows[i]["blamt"]);
        //                    dr1["rupinword"] = rupinwords;
        //                    blrem = "";
        //                    //if (tbl.Rows[i]["sapopdno"].ToString() != "") blrem = blrem + "ODP No. " + tbl.Rows[i]["sapopdno"].ToString() + "  ";
        //                    //if (tbl.Rows[i]["sapblno"].ToString() != "") blrem = blrem + "SAP Bill # " + tbl.Rows[i]["sapblno"].ToString() + "  ";
        //                    //if (tbl.Rows[i]["sapshipno"].ToString() != "") blrem = blrem + "SAP Shipment # " + tbl.Rows[i]["sapshipno"].ToString() + "  ";
        //                    //if (tbl.Rows[i]["docrem"].ToString() != "") blrem = blrem + tbl.Rows[i]["docrem"].ToString() + "  ";
        //                    dr1["blremarks"] = blrem;

        //                    //Bank Detals
        //                    dr1["bankactno"] = bankactno;
        //                    dr1["bankname"] = bankname;
        //                    dr1["bankifsc"] = bankifsc;
        //                    dr1["bankbranch"] = bankbranch;
        //                    dr1["bankadd"] = bankadd;
        //                    dr1["bankrtgs"] = bankrtgs;

        //                    dr1["dtldsc"] = dtldsc;
        //                    dr1["dtlamt"] = dtlamt;

        //                    dr1["hsn_cd"] = hsncode;

        //                    if (flagi == true)
        //                    {
        //                        dr1["hsn_hddsp1"] = "IGST";
        //                    }
        //                    else
        //                    {
        //                        if (flagc == true)
        //                        {
        //                            dr1["hsn_hddsp1"] = "CGST";
        //                        }
        //                        else
        //                        {
        //                            dr1["hsn_hddsp1"] = "";
        //                        }
        //                    }
        //                    dr1["hsn_hddsp2"] = flags == true ? "SGST" : "";
        //                    dr1["hsn_txblval"] = hsntaxblval;
        //                    dr1["hsn_gstper1"] = hsngstper1;
        //                    dr1["hsn_gstamt1"] = hsngstamt1;
        //                    dr1["hsn_gstper2"] = hsngstper2;
        //                    dr1["hsn_gstamt2"] = hsngstamt2;
        //                    dr1["hsn_gstper3"] = hsngstper3;
        //                    dr1["hsn_gstamt3"] = hsngstamt3;
        //                    dr1["hsn_cessamt"] = "";
        //                    dr1["hsn_gstamt"] = "";
        //                    dr1["hsn_qnty"] = hsnqnty;
        //                    dr1["hsn_tqnty"] = total_qnty;
        //                    dr1["hsn_ttxblval"] = total_taxval.ToINRFormat();
        //                    dr1["hsn_tgstamt1"] = total_gstamt1.ToINRFormat();
        //                    dr1["hsn_tgstamt2"] = total_gstamt2.ToINRFormat();
        //                    dr1["hsn_tgstamt3"] = total_gstamt3.ToINRFormat();
        //                    if (totalosamt != 0) dr1["totalosamt"] = totalosamt.ToINRFormat();
        //                    dr1["poslno"] = "";
        //                    dr1["mtrlcd"] = "";
        //                    dr1["bas_rate"] = "";
        //                    dr1["pv_per"] = "";
        //                    //if (tbl.Rows[i]["insby"].ToString().retStr() == "Y") dr1["insudesc"] = inspoldesc;
        //                    dr1["dealsin"] = dealsin;
        //                    dr1["blterms"] = blterms;

        //                    if (doctotprint == false)
        //                    {
        //                        itdsc = "";
        //                        if (tbl.Rows[i]["itcd"].ToString() != "")
        //                        {
        //                            lslno = lslno + 1;
        //                            delvchrg = false;
        //                        }
        //                        else
        //                        {
        //                            lslno = 0;
        //                            delvchrg = true;
        //                        }
        //                        if (tbl.Rows[i]["itrem"].ToString() != "") itdsc = tbl.Rows[i]["itrem"].ToString();
        //                        //if (Convert.ToDouble(tbl.Rows[i]["nosinbag"]) != 0)
        //                        //{
        //                        //    double dbnopcks = Convert.ToDouble(tbl.Rows[i]["nosinbag"]) * Convert.ToDouble(tbl.Rows[i]["nos"]);
        //                        //    itdsc = itdsc + "CLD: " + Convert.ToDouble(tbl.Rows[i]["nosinbag"]).ToString("0") + " NOPCKS: " + dbnopcks.ToString();
        //                        //}
        //                        //if (tbl.Rows[i]["prodcd"].ToString() != "") itdsc = itdsc + "PCD: " + tbl.Rows[i]["prodcd"].ToString() + " ";
        //                        //if (tbl.Rows[i]["batchdlprint"].ToString() == "Y" && tbl.Rows[i]["batchdtl"].ToString() != "") itdsc += "Batch # " + tbl.Rows[i]["batchdtl"].ToString(); else dr1["batchdtl"] = "";
        //                        if (tbl.Rows[i]["itcd"].ToString() != "") dr1["caltype"] = 1; else dr1["caltype"] = 0;
        //                        dr1["agstdocno"] = "";
        //                        dr1["agstdocdt"] = "";
        //                        dr1["slno"] = lslno;
        //                        dr1["itcd"] = tbl.Rows[i]["itcd"].ToString();
        //                        dr1["prodcd"] = "";
        //                        dr1["itnm"] = tbl.Rows[i]["itnm"].ToString();
        //                        dr1["itdesc"] = itdsc;
        //                        dr1["bltophead"] = "";
        //                        dr1["makenm"] = "";
        //                        dr1["mrp"] = 0;
        //                        //if (tbl.Rows[i]["batchdlprint"].ToString() == "Y" &&  tbl.Rows[i]["batchdtl"].ToString() != "") dr1["batchdtl"] = "Batch # "+tbl.Rows[i]["batchdtl"].ToString(); else dr1["batchdtl"] = "";
        //                        dr1["hsnsaccd"] = tbl.Rows[i]["hsnsaccd"].ToString();
        //                        dr1["packsize"] = Convert.ToDouble(0);
        //                        dr1["nos"] = Convert.ToDouble(0);
        //                        dr1["qnty"] = tbl.Rows[i]["qnty"] == DBNull.Value ? 0 : Convert.ToDouble(tbl.Rows[i]["qnty"]);
        //                        uomdecimal = tbl.Rows[i]["qdecimal"] == DBNull.Value ? 0 : Convert.ToInt16(tbl.Rows[i]["qdecimal"]);
        //                        string dbqtyu = string.Format("{0:N6}", Convert.ToDouble(dr1["qnty"]));
        //                        if (dbqtyu.Substring(dbqtyu.Length - 2, 2) == "00")
        //                        {
        //                            if (uomdecimal > 4) uomdecimal = 4;
        //                        }
        //                        if (uomdecimal > uommaxdecimal) uommaxdecimal = uomdecimal;
        //                        if (VE.DOCCD == "SOOS" && uomdecimal == 6) uomdecimal = 4;

        //                        dr1["qdecimal"] = uomdecimal;
        //                        dr1["uomnm"] = tbl.Rows[i]["uomnm"].ToString();
        //                        dr1["rate"] = tbl.Rows[i]["rate"] == DBNull.Value ? 0 : Convert.ToDouble(tbl.Rows[i]["rate"]);
        //                        dr1["basamt"] = Convert.ToDouble(0);
        //                        //if (tbl.Rows[i]["rateqntybag"].ToString() == "B") dr1["rateuomnm"] = "Case"; else dr1["rateuomnm"] = dr1["uomnm"];
        //                        string strdsc = "";
        //                        //if (Convert.ToDouble(tbl.Rows[i]["stddiscamt"]) != 0)
        //                        //{
        //                        //    switch (tbl.Rows[i]["stddisctype"].ToString())
        //                        //    {
        //                        //        case "Q":
        //                        //            //strdsc = "Q " + Convert.ToDouble(tbl.Rows[i]["stddiscrate"]).ToString("0.00"); break;
        //                        //            strdsc = ""; break;
        //                        //        case "N":
        //                        //            //strdsc = "N " + Convert.ToDouble(tbl.Rows[i]["stddiscrate"]).ToString("0.00"); break;
        //                        //            strdsc = ""; break;
        //                        //        case "F":
        //                        //            strdsc = "F"; break;
        //                        //        default:
        //                        //            dr1["discper"] = Convert.ToDouble(tbl.Rows[i]["stddiscrate"]);
        //                        //            strdsc = Convert.ToDouble(tbl.Rows[i]["stddiscrate"]).ToString("0.00") + "%"; break;
        //                        //    }
        //                        //}
        //                        dr1["stddisc"] = strdsc;
        //                        dr1["stddiscamt"] = Convert.ToDouble(0);
        //                        if (Convert.ToDouble(tbl.Rows[i]["discamt"]) != 0)
        //                        {
        //                            switch (tbl.Rows[i]["disctype"].ToString())
        //                            {
        //                                case "Q":
        //                                    strdsc = "Q" + Convert.ToDouble(tbl.Rows[i]["discrate"]).ToString("0.00"); break;
        //                                case "B":
        //                                    strdsc = "B" + Convert.ToDouble(tbl.Rows[i]["discrate"]).ToString("0.00"); break;
        //                                case "F":
        //                                    strdsc = "F"; break;
        //                                case "P":
        //                                    strdsc = Convert.ToDouble(tbl.Rows[i]["discrate"]).ToString("0.00") + "%"; break;
        //                            }
        //                        }
        //                        dr1["disc"] = strdsc;
        //                        dr1["discamt"] = Convert.ToDouble(0);
        //                        dr1["taxableval"] = (Convert.ToDouble(tbl.Rows[i]["taxableval"]));
        //                        dr1["amt"] = Convert.ToDouble(tbl.Rows[i]["amt"]).ToINRFormat();
        //                        if (Convert.ToDouble(tbl.Rows[i]["igstamt"]) != 0) dr1["cgstdsp"] = "IGST"; else dr1["cgstdsp"] = "CGST";
        //                        dr1["cgstper"] = Convert.ToDouble(tbl.Rows[i]["cgstper"]) + Convert.ToDouble(tbl.Rows[i]["igstper"]);
        //                        dr1["cgstamt"] = Convert.ToDouble(tbl.Rows[i]["cgstamt"]) + Convert.ToDouble(tbl.Rows[i]["igstamt"]);
        //                        dr1["sgstper"] = Convert.ToDouble(tbl.Rows[i]["sgstper"]);
        //                        dr1["sgstamt"] = Convert.ToDouble(tbl.Rows[i]["sgstamt"]);
        //                        dr1["cessper"] = Convert.ToDouble(tbl.Rows[i]["cessper"]);
        //                        dr1["cessamt"] = Convert.ToDouble(tbl.Rows[i]["cessamt"]);
        //                        dr1["gstper"] = Convert.ToDouble(tbl.Rows[i]["gstper"]);
        //                        dr1["netamt"] = Convert.ToDouble(dr1["cgstamt"].ToString()) + Convert.ToDouble(dr1["sgstamt"].ToString()) + Convert.ToDouble(tbl.Rows[i]["taxableval"]) + Convert.ToDouble(dr1["cessamt"].ToString());
        //                        //totals
        //                        //dnos = dnos + Convert.ToDouble(dr1["nos"].ToString());
        //                        dqnty = dqnty + Convert.ToDouble(dr1["qnty"].ToString());
        //                        //dbasamt = dbasamt + Convert.ToDouble(dr1["basamt"].ToString());
        //                        //ddisc1 = ddisc1 + Convert.ToDouble(dr1["stddiscamt"].ToString());
        //                        //ddisc2 = ddisc2 + Convert.ToDouble(dr1["discamt"].ToString());
        //                        dtxblval = dtxblval + Convert.ToDouble(dr1["taxableval"].ToString());
        //                        damt = damt + Convert.ToDouble(dr1["amt"].ToString());
        //                        dcgstamt = dcgstamt + Convert.ToDouble(dr1["cgstamt"].ToString());
        //                        dsgstamt = dsgstamt + Convert.ToDouble(dr1["sgstamt"].ToString());
        //                        //dnetamt = dnetamt + Convert.ToDouble(dr1["taxableval"].ToString());
        //                    }
        //                    IR.Rows.Add(dr1);

        //                    if (totalreadyprint == false)
        //                    {
        //                        if (i == maxR) doctotprint = true;
        //                        else if (tbl.Rows[i + 1]["autono"].ToString() != auto1) doctotprint = true;
        //                        else if (tbl.Rows[i + 1]["itcd"].ToString() == "") doctotprint = true;
        //                    }
        //                    if (delvchrg == true)
        //                    {
        //                        doctotprint = true; totalreadyprint = false; delvchrg = false;
        //                    }
        //                    if (doctotprint == true && totalreadyprint == false)
        //                    {
        //                        dr1 = IR.NewRow();
        //                        dr1["autono"] = auto1 + copymode;
        //                        dr1["copymode"] = copymode;
        //                        dr1["docno"] = tbl.Rows[i]["docno"].ToString();

        //                        dr1["itnm"] = "Total";
        //                        dr1["nos"] = dnos;
        //                        dr1["qnty"] = dqnty;
        //                        if (VE.DOCCD == "SOOS" && uommaxdecimal == 6) uommaxdecimal = 4;
        //                        dr1["qdecimal"] = uommaxdecimal;
        //                        dr1["basamt"] = dbasamt;
        //                        dr1["stddiscamt"] = ddisc1;
        //                        dr1["discamt"] = ddisc2;
        //                        dr1["taxableval"] = dtxblval.ToINRFormat();
        //                        dr1["amt"] = dtxblval.ToINRFormat();
        //                        dr1["cgstamt"] = dcgstamt;
        //                        dr1["sgstamt"] = dsgstamt;
        //                        dr1["netamt"] = dnetamt;
        //                        totalreadyprint = true;
        //                        goto docstart;
        //                    }
        //                    doctotprint = false;
        //                    i = i + 1;
        //                    ilast = i;
        //                    if (i > maxR) break;
        //                }
        //                i = ilast;
        //            }
        //        }

        //        string compaddress = masterHelp.retCompAddress(gocd, grpemailid); //.Split(Convert.ToChar(Cn.GCS()));
        //        string stremail = compaddress.retCompValue("EMAIL");

        //        //string[] compaddress; string stremail = "";
        //        //compaddress = Salesfunc.retCompAddress(gocd, grpemailid).Split(Convert.ToChar(Cn.GCS()));
        //        //stremail = compaddress[6];

        //        //sql = "";
        //        //sql += "select b.compnm, b.add1, b.add2, b.add3, b.state, b.panno, b.cinno, b.propname, ";
        //        //sql += "a.add1 ladd1, a.add2 ladd2, a.add3 ladd3, a.add4 ladd4, a.add5 ladd5, a.add6 ladd6, ";
        //        //sql += "a.state lstate, a.statecd lstatecd, a.phno3, ";
        //        //sql += "a.gstno, a.phno1, a.phno2, a.regemailid ";
        //        //sql += "from " + Scmf + ".m_loca a, " + Scmf + ".m_comp b ";
        //        //sql += "where a.compcd='" + COM + "' and a.loccd='" + LOC + "' and a.compcd=b.compcd(+) ";
        //        //DataTable comptbl = masterHelp.SQLquery(sql);
        //        //string compstat = ""; string compadd = ""; string locaadd = ""; string locastat = ""; string cregadd = ""; string stremail = "", legalname="";
        //        //if (grpemailid == "") stremail = comptbl.Rows[0]["regemailid"].ToString(); else stremail = grpemailid;
        //        //string mfld = "";
        //        //if (comptbl.Rows[0]["propname"].ToString().retStr() != "") legalname = "Prop. " + comptbl.Rows[0]["propname"].ToString();
        //        //for (int f = 1; f <= 6; f++)
        //        //{
        //        //    mfld = "ladd" + Convert.ToString(f).ToString();
        //        //    if (comptbl.Rows[0][mfld].ToString() != "")
        //        //    {
        //        //        compadd = compadd + comptbl.Rows[0][mfld].ToString() + " ";
        //        //    }
        //        //}
        //        //compadd = compadd + " " + comptbl.Rows[0]["lstate"].ToString() + " (" + comptbl.Rows[0]["lstatecd"].ToString() + ")";
        //        //compadd = compadd + ", Phone : " + comptbl.Rows[0]["phno1"].ToString();
        //        //if (comptbl.Rows[0]["phno3"].ToString() != "") compadd = compadd + ", Fax : " + comptbl.Rows[0]["phno3"].ToString();
        //        //compadd = compadd + ", email : " + stremail;
        //        //cregadd = comptbl.Rows[0]["add1"].ToString() + " " + comptbl.Rows[0]["add2"].ToString() + " " + comptbl.Rows[0]["add3"].ToString() + comptbl.Rows[0]["state"].ToString();
        //        //cregadd = cregadd.Trim();
        //        //if (comptbl.Rows[0]["panno"].ToString() != "") compstat = "PAN # " + comptbl.Rows[0]["panno"].ToString() + " ";
        //        //if (comptbl.Rows[0]["cinno"].ToString() != "") compstat = compstat + "CIN # " + comptbl.Rows[0]["cinno"].ToString() + " ";

        //        //locastat = "GST # " + comptbl.Rows[0]["gstno"].ToString();
        //        //if (fssailicno != "") locastat = locastat + "   FSSAI LICENCE # " + fssailicno;

        //        //if (LOC == "KOLK")
        //        //{
        //        //    locaadd = "Godown : " + goadd;
        //        //    compadd = "Office : " + compadd;
        //        //}
        //        //else
        //        //{
        //        //    locaadd = "Factory : " + compadd;
        //        //    compadd = "Regd Office : " + cregadd;
        //        //}

        //        string ccemail = grpemailid;
        //        if (ccemail == "") ccemail = stremail;

        //        ReportDocument reportdocument = new ReportDocument();
        //        string complogo = Salesfunc.retCompLogo();
        //        EmailControl EmailControl = new EmailControl();

        //        string complogosrc = complogo;
        //        string compfixlogosrc = "c:\\improvar\\" + CommVar.Compcd(UNQSNO) + "fix.jpg";
        //        string sendemailids = "";
        //        string rptfile = "DnCn_Print.rpt";
        //        if (VE.TEXTBOX6 != null) rptfile = VE.TEXTBOX6 + ".rpt";
        //        string rptname = "~/Report/" + rptfile; // "SaleBill.rpt";
        //        if (VE.maxdate == "CHALLAN") blhead = "CHALLAN";

        //        if (printemail == "Email")
        //        {
        //            var rsemailid = (from DataRow dr in IR.Rows
        //                             select new
        //                             {
        //                                 email = dr["regemailid"]
        //                             }).Distinct().ToList();

        //            for (int z = 0; z < rsemailid.Count; z++)
        //            {
        //                if (rsemailid[z].email.ToString() != "")
        //                {

        //                    var queryq = from row in IR.AsEnumerable()
        //                                 where row.Field<string>("regemailid") == rsemailid[z].email.ToString()
        //                                 select row;

        //                    var rsemailid1 = queryq.AsDataView().ToTable();

        //                    if (doctype == "CINV") reportdocument.Load(Server.MapPath("~/Report/CommSaleBill.rpt"));
        //                    else reportdocument.Load(Server.MapPath(rptname));

        //                    maxR = rsemailid1.Rows.Count - 1;
        //                    Int32 iz = 0;
        //                    string slnm = "", emlslcd = "", body = "", chkfld = "", chkfld1 = "";

        //                    while (iz <= maxR)
        //                    {
        //                        slnm = rsemailid1.Rows[iz]["slnm"].ToString();
        //                        emlslcd = rsemailid1.Rows[iz]["slcd"].ToString();
        //                        body += "<tr>";
        //                        body += "<td>" + rsemailid1.Rows[iz]["docno"] + "</td>";
        //                        body += "<td>" + rsemailid1.Rows[iz]["docdt"] + "</td>";
        //                        body += "<td style='text-align:right'>" + Cn.Indian_Number_format(Convert.ToDouble(rsemailid1.Rows[iz]["blamt"]).ToString(), "0.00") + "</td>";
        //                        body += "</tr>";

        //                        chkfld = rsemailid1.Rows[iz]["autono"].ToString().Substring(0, rsemailid1.Rows[iz]["autono"].ToString().Length - 1);

        //                        while (rsemailid1.Rows[iz]["autono"].ToString().Substring(0, rsemailid1.Rows[iz]["autono"].ToString().Length - 1) == chkfld)
        //                        {
        //                            iz++;
        //                            if (iz > maxR) break;
        //                        }
        //                    }
        //                    string uid = Session["UR_ID"].ToString();
        //                    string MOBILE = DB1.USER_APPL.Find(uid).MOBILE;
        //                    string ldt = rsemailid1.Rows[rsemailid1.Rows.Count - 1]["docdt"].ToString();

        //                    reportdocument.SetDataSource(rsemailid1);
        //                    reportdocument.SetParameterValue("complogo", complogo);
        //                    reportdocument.SetParameterValue("billheading", blhead);
        //                    reportdocument.SetParameterValue("prodlogo", "");
        //                    reportdocument.SetParameterValue("compnm", compaddress.retCompValue("compnm"));
        //                    reportdocument.SetParameterValue("compadd", compaddress.retCompValue("compadd"));
        //                    reportdocument.SetParameterValue("compcommu", compaddress.retCompValue("compcommu"));
        //                    reportdocument.SetParameterValue("compstat", compaddress.retCompValue("compstat"));
        //                    reportdocument.SetParameterValue("locaadd", compaddress.retCompValue("locaadd"));
        //                    reportdocument.SetParameterValue("locacommu", compaddress.retCompValue("locacommu"));
        //                    reportdocument.SetParameterValue("locastat", compaddress.retCompValue("locastat"));
        //                    reportdocument.SetParameterValue("legalname", compaddress.retCompValue("legalname"));
        //                    reportdocument.SetParameterValue("corpadd", compaddress.retCompValue("corpadd"));
        //                    reportdocument.SetParameterValue("corpcommu", compaddress.retCompValue("corpcommu"));

        //                    Response.Buffer = false;
        //                    Response.ClearContent();
        //                    Response.ClearHeaders();
        //                    Stream stream = reportdocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
        //                    stream.Seek(0, SeekOrigin.Begin);
        //                    string path_Save = @"C:\improvar\" + doccd + "-" + emlslcd + "-" + ldt.Substring(6, 4) + ldt.Substring(3, 2) + ldt.Substring(0, 2) + ".pdf";
        //                    if (System.IO.File.Exists(path_Save))
        //                    {
        //                        System.IO.File.Delete(path_Save);
        //                    }
        //                    reportdocument.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, path_Save);
        //                    // email

        //                    List<System.Net.Mail.Attachment> attchmail = new List<System.Net.Mail.Attachment>();// System.Net.Mail.Attachment(path_Save);
        //                    attchmail.Add(new System.Net.Mail.Attachment(path_Save));
        //                    //System.Net.Mail.Attachment attchmail = new System.Net.Mail.Attachment(path_Save);

        //                    string[,] emlaryBody = new string[7, 2];
        //                    if (VE.TEXTBOX5 != null)
        //                    {
        //                        bool emailsent = EmailControl.SendHtmlFormattedEmail(VE.TEXTBOX5, "", "", emlaryBody, attchmail, grpemailid);
        //                        if (emailsent == true) sendemailids = sendemailids + VE.TEXTBOX5 + ";"; else sendemailids = " not able to send on " + VE.TEXTBOX5 + ";";
        //                    }
        //                    else
        //                    {
        //                        emlaryBody[0, 0] = "{slnm}"; emlaryBody[0, 1] = slnm;
        //                        emlaryBody[1, 0] = "{tbody}"; emlaryBody[1, 1] = body;
        //                        emlaryBody[2, 0] = "{username}"; emlaryBody[2, 1] = System.Web.HttpContext.Current.Session["UR_NAME"].ToString();
        //                        emlaryBody[3, 0] = "{compname}"; emlaryBody[3, 1] = compaddress.retCompValue("compnm").retStr();
        //                        emlaryBody[4, 0] = "{usermobno}"; emlaryBody[4, 1] = MOBILE;
        //                        emlaryBody[5, 0] = "{complogo}"; emlaryBody[5, 1] = complogosrc;
        //                        emlaryBody[6, 0] = "{compfixlogo}"; emlaryBody[6, 1] = compfixlogosrc;
        //                        bool emailsent = EmailControl.SendHtmlFormattedEmail(rsemailid[z].email.ToString(), "Sales Bill copy of " + docnm, "Salebill.htm", emlaryBody, attchmail, grpemailid);
        //                        if (emailsent == true) sendemailids = sendemailids + rsemailid[z].email.ToString() + ";"; else sendemailids = sendemailids + " not able to send on " + rsemailid[z].email.ToString();
        //                    }
        //                    System.IO.File.Delete(path_Save);
        //                    //eof email sending
        //                }
        //            }
        //            reportdocument.Close(); reportdocument.Dispose(); GC.Collect();
        //            string emailretmsg = "email : " + sendemailids + "<br /> CC email on " + grpemailid;
        //            return Content(emailretmsg);
        //        }
        //        else
        //        {
        //            if (doctype == "CINV") reportdocument.Load(Server.MapPath("~/Report/CommSaleBill.rpt"));
        //            else reportdocument.Load(Server.MapPath(rptname));

        //            reportdocument.SetDataSource(IR);
        //            reportdocument.SetParameterValue("complogo", complogo);
        //            reportdocument.SetParameterValue("billheading", blhead);
        //            reportdocument.SetParameterValue("prodlogo", "");
        //            reportdocument.SetParameterValue("compnm", compaddress.retCompValue("compnm"));
        //            reportdocument.SetParameterValue("compadd", compaddress.retCompValue("compadd"));
        //            reportdocument.SetParameterValue("compcommu", compaddress.retCompValue("compcommu"));
        //            reportdocument.SetParameterValue("compstat", compaddress.retCompValue("compstat"));
        //            reportdocument.SetParameterValue("locaadd", compaddress.retCompValue("locaadd"));
        //            reportdocument.SetParameterValue("locacommu", compaddress.retCompValue("locacommu"));
        //            reportdocument.SetParameterValue("locastat", compaddress.retCompValue("locastat"));
        //            reportdocument.SetParameterValue("legalname", compaddress.retCompValue("legalname"));
        //            reportdocument.SetParameterValue("corpadd", compaddress.retCompValue("corpadd"));
        //            reportdocument.SetParameterValue("corpcommu", compaddress.retCompValue("corpcommu"));

        //            Response.Buffer = false;
        //            Response.ClearContent();
        //            Response.ClearHeaders();
        //            Stream stream = reportdocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
        //            reportdocument.Close(); reportdocument.Dispose(); GC.Collect();
        //            stream.Seek(0, SeekOrigin.Begin);
        //            reportdocument.Close(); reportdocument.Dispose(); GC.Collect();
        //            return new FileStreamResult(stream, "application/pdf");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Cn.SaveException(ex, "");
        //        return Content(ex.Message);
        //    }
        //}
        #endregion
        public ActionResult ShowReport()
        {
            return RedirectToAction("ResponsivePrintViewer", "RPTViewer");
        }
    }
}