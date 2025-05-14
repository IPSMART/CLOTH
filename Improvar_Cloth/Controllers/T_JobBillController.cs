using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;
using Microsoft.Ajax.Utilities;
using System.Web.UI.WebControls;
using System.IO;
using Oracle.ManagedDataAccess.Client;
using System.Data.Entity.Validation;

namespace Improvar.Controllers
{
    public class T_JobBillController : Controller
    {
        Connection Cn = new Connection();
        MasterHelp Master_Help = new MasterHelp();
        MasterHelpFa masfa = new MasterHelpFa();
        Salesfunc SALES_FUNC = new Salesfunc();
        T_CNTRL_HDR TCH; T_JBILL TJBILL; T_CNTRL_HDR_REM SLR; M_SITEM MSITM;
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: T_JobBill
        public ActionResult T_JobBill(string op = "", string key = "", int Nindex = 0, string searchValue = "", string parkID = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    TJobBillEntry VE = (parkID == "") ? new TJobBillEntry() : (Improvar.ViewModels.TJobBillEntry)Session[parkID];
                    Cn.getQueryString(VE);
                    Cn.ValidateMenuPermission(VE);
                    string MNUP = VE.MENU_PARA;
                    switch (MNUP)
                    {
                        case "CN":
                            ViewBag.formname = "Credit Note"; break;
                        case "DN":
                            ViewBag.formname = "Debit Note"; break;
                        case "JB":
                            ViewBag.formname = "Job Bill"; break;
                        default: ViewBag.formname = ""; break;
                    }
                    string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    string YR1 = CommVar.YearCode(UNQSNO);
                    var doctP = (from i in DB1.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();
                    List<DocumentType> DueCal = new List<DocumentType>();// add due cal option
                    DocumentType Due1 = new DocumentType();
                    Due1.text = "Doc Date"; Due1.value = "D";
                    DocumentType Due2 = new DocumentType();
                    Due2.text = "Party Bill Date"; Due2.value = "P";
                    DueCal.Add(Due1); DueCal.Add(Due2);
                    VE.DueCal = DueCal;
                    List<DocumentType> Reverse = new List<DocumentType>();//add reverse charge option
                    DocumentType Reverse0 = new DocumentType();
                    Reverse0.text = "No"; Reverse0.value = "";
                    DocumentType Reverse1 = new DocumentType();
                    Reverse1.text = "Yes"; Reverse1.value = "Y";
                    DocumentType Reverse2 = new DocumentType();
                    Reverse2.text = "Not Applicable"; Reverse2.value = "N";
                    //Reverse.Add(Reverse0);
                    Reverse.Add(Reverse1); Reverse.Add(Reverse2);
                    VE.Reverse_Charge = Reverse;
                    List<DocumentType> DDL = new List<DocumentType>(); //Lower TDS TYPE                   
                    DocumentType DDL1 = new DocumentType(); DDL1.text = ""; DDL1.value = ""; DDL.Add(DDL1);
                    DocumentType DDL2 = new DocumentType(); DDL2.text = "Y"; DDL2.value = "Y"; DDL.Add(DDL2);
                    DocumentType DDL3 = new DocumentType(); DDL3.text = "N"; DDL3.value = "N"; DDL.Add(DDL3);
                    DocumentType DDL4 = new DocumentType(); DDL4.text = "T"; DDL4.value = "T"; DDL.Add(DDL4);
                    VE.LowerTDSType = DDL;
                    VE.DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
                    if (op.Length != 0)
                    {
                        string[] XYZ = VE.DocumentType.Select(i => i.value).ToArray();
                        VE.IndexKey = (from p in DB.T_JBILL
                                       join q in DB.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                       orderby p.DOCDT, p.DOCNO
                                       where XYZ.Contains(p.DOCCD) && q.LOCCD == LOC && q.COMPCD == COM && q.YR_CD == YR1
                                       select new IndexKey() { Navikey = p.AUTONO }).ToList();

                        if (searchValue != "") { Nindex = VE.IndexKey.FindIndex(r => r.Navikey.Equals(searchValue)); }
                        if (op == "E" || op == "D" || op == "V")
                        {
                            if (VE.IndexKey.Count <= 0)
                            {
                                TJBILL = new T_JBILL();
                                TJBILL.DOCNO = "";
                                VE.T_JBILL = TJBILL;
                            }
                            else
                            {
                                if (searchValue.Length != 0)
                                {
                                    VE.Index = Nindex;
                                    VE = Navigation(VE, DB, 0, searchValue);
                                }
                                else
                                {
                                    if (key == "F")
                                    {
                                        VE.Index = 0;
                                        VE = Navigation(VE, DB, 0, searchValue);
                                    }
                                    else if (key == "" || key == "L")
                                    {
                                        VE.Index = VE.IndexKey.Count - 1;
                                        VE = Navigation(VE, DB, VE.IndexKey.Count - 1, searchValue);
                                    }
                                    else if (key == "P")
                                    {
                                        Nindex -= 1;
                                        if (Nindex < 0)
                                        {
                                            Nindex = 0;
                                        }
                                        VE.Index = Nindex;
                                        VE = Navigation(VE, DB, Nindex, searchValue);
                                    }
                                    else if (key == "N")
                                    {
                                        Nindex += 1;
                                        if (Nindex > VE.IndexKey.Count - 1)
                                        {
                                            Nindex = VE.IndexKey.Count - 1;
                                        }
                                        VE.Index = Nindex;
                                        VE = Navigation(VE, DB, Nindex, searchValue);
                                    }
                                }
                                VE.T_JBILL = TJBILL;
                                VE.T_CNTRL_HDR = TCH;
                                VE.T_CNTRL_HDR_REM = SLR;
                                ViewBag.formname = ViewBag.formname + " [" + TCH.DOCNO + "]";
                            }
                        }
                        if (op.ToString() == "A")
                        {
                            if (parkID == "")
                            {
                                TJBILL = new T_JBILL();
                                TJBILL.DOCDT = Cn.getCurrentDate(VE.mindate);
                                if (VE.DocumentType != null && VE.DocumentType.Count > 0)
                                {
                                    TJBILL.DOCCD = VE.DocumentType[0].value;
                                }
                                VE.T_JBILL = TJBILL;
                                List<T_JBILLDTL> T_JBILLDTL = new List<T_JBILLDTL>();
                                VE.T_JBILLDTL = T_JBILLDTL;
                                VE.T_JBILL.DUECALCON = "D"; VE.T_JBILL.CRDAYS = 0;
                                List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                                UploadDOC UPL = new UploadDOC();
                                UPL.DocumentType = doctP;
                                UploadDOC1.Add(UPL);
                                VE.UploadDOC = UploadDOC1;
                                VE.Roundoff_DCNote = true;
                            }
                            else
                            {
                                if (VE.ItemDetails != null)
                                {
                                    if (VE.ItemDetails.Count > 0)
                                    {
                                        foreach (var u in VE.ItemDetails)
                                        {
                                            u.QNTY_UNIT_PC = DDL;
                                        }
                                    }
                                }
                                if (VE.DRCRDetails != null)
                                {
                                    if (VE.DRCRDetails.Count > 0)
                                    {
                                        foreach (var u in VE.DRCRDetails)
                                        {
                                            u.QNTY_UNIT_DNCN = DDL;
                                        }
                                    }
                                }
                                if (VE.UploadDOC != null)
                                {
                                    foreach (var i in VE.UploadDOC)
                                    {
                                        i.DocumentType = doctP;
                                    }
                                }
                                INI INIF = new INI();
                                INIF.DeleteKey(Session["UR_ID"].ToString(), parkID, Server.MapPath("~/Park.ini"));
                            }
                            VE = (TJobBillEntry)Cn.CheckPark(VE, VE.MenuID, VE.MenuIndex, LOC, COM, CommVar.CurSchema(UNQSNO).ToString(), Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString());
                        }
                    }
                    else
                    {
                        VE.DefaultView = false;
                        VE.DefaultDay = 0;
                    }
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    VE.TDSROUND = (from i in DBF.M_POST select i).FirstOrDefault()?.TDSROUND.retStr();
                    VE.MSG = "";
                    string docdt = "";
                    if (TCH != null) if (TCH.DOCDT != null) docdt = TCH.DOCDT.ToString().Remove(10);
                    Cn.getdocmaxmindate(VE.T_JBILL.DOCCD, docdt, VE.DefaultAction, VE.T_JBILL.DOCNO, VE);
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                TJobBillEntry VE = new TJobBillEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public TJobBillEntry Navigation(TJobBillEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            //ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            //ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            using (ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO)))
            {
                using (ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), CommVar.CommSchema()))
                {
                    string COM_CD = CommVar.Compcd(UNQSNO), LOC_CD = CommVar.Loccd(UNQSNO);
                    string DATABASE = CommVar.CurSchema(UNQSNO);
                    Cn.getQueryString(VE);
                    TJBILL = new T_JBILL(); TCH = new T_CNTRL_HDR(); SLR = new T_CNTRL_HDR_REM();

                    if (VE.IndexKey.Count != 0)
                    {
                        string[] aa = null;
                        if (searchValue.Length == 0)
                        {
                            aa = VE.IndexKey[index].Navikey.Split(Convert.ToChar(Cn.GCS()));
                        }
                        else
                        {
                            aa = searchValue.Split(Convert.ToChar(Cn.GCS()));
                        }
                        TJBILL = DB.T_JBILL.Find(aa[0].Trim());
                        TCH = DB.T_CNTRL_HDR.Find(TJBILL.AUTONO);

                        SLR = Cn.GetTransactionReamrks(CommVar.CurSchema(UNQSNO), TJBILL.AUTONO);
                        VE.UploadDOC = Cn.GetUploadImageTransaction(CommVar.CurSchema(UNQSNO), TJBILL.AUTONO);
                        List<DocumentType> DDL = new List<DocumentType>(); //qntycalcon combo                   
                                                                           //DocumentType DDL1 = new DocumentType(); DDL1.text = "Unit"; DDL1.value = "P"; DDL.Add(DDL1);
                                                                           //DocumentType DDL2 = new DocumentType(); DDL2.text = "Dzn"; DDL2.value = "D"; DDL.Add(DDL2);
                                                                           //DocumentType DDL3 = new DocumentType(); DDL3.text = "Box"; DDL3.value = "B"; DDL.Add(DDL3);
                        DocumentType DDL2 = new DocumentType(); DDL2.text = "Qnty"; DDL2.value = "Q"; DDL.Add(DDL2);
                        DocumentType DDL1 = new DocumentType(); DDL1.text = "Nos"; DDL1.value = "N"; DDL.Add(DDL1);
                        List<PendingChallanItemDetails> JBD = new List<PendingChallanItemDetails>();
                        if (VE.MENU_PARA == "JB")
                        {
                            VE.ItemDetails = (from i in DB.T_JBILLDTL
                                              join p in DB.M_SITEM on i.ITCD equals p.ITCD
                                              where (i.AUTONO == TJBILL.AUTONO)
                                              select new PendingChallanItemDetails()
                                              {
                                                  AUTONO = i.AUTONO,
                                                  BillQNTY = ((double)(i.BQNTY == null ? 0 : i.BQNTY)) + ((double)(i.DNCNQNTY == null ? 0 : i.DNCNQNTY)),
                                                  ShortQNTY = ((double)(i.DNCNQNTY == null ? 0 : i.DNCNQNTY)),
                                                  ShortQNTY_DISPLAY = ((double)(i.DNCNQNTY == null ? 0 : i.DNCNQNTY)),
                                                  ITCD = i.ITCD,
                                                  ITNM = p.ITNM,
                                                  UOM = p.UOMCD,
                                                  ITREM = i.ITREM,
                                                  STYLENO = p.STYLENO,
                                                  PARTCD = i.PARTCD,
                                                  NOS = i.NOS,
                                                  PASSQNTY = ((double)(i.PQNTY == null ? 0 : i.PQNTY)),
                                                  addless = ((double)(i.ADDLESSAMT == null ? 0 : i.ADDLESSAMT)),
                                                  RECQNTY = ((double)(i.RQNTY == null ? 0 : i.RQNTY)),
                                                  RECQNTY_DISPLAY = ((double)(i.RQNTY == null ? 0 : i.RQNTY)),
                                                  SLNO = i.SLNO,
                                                  EFFDT = ((DateTime)i.EFFDT),
                                                  igstper = ((double)(i.IGSTPER == null ? 0 : i.IGSTPER)),
                                                  cgstper = ((double)(i.CGSTPER == null ? 0 : i.CGSTPER)),
                                                  sgstper = ((double)(i.SGSTPER == null ? 0 : i.SGSTPER)),
                                                  cessper = ((double)(i.CESSPER == null ? 0 : i.CESSPER)),
                                                  DISCPER = ((double)(i.DISCPER == null ? 0 : i.DISCPER)),
                                                  DISCAMT = ((double)(i.DISCAMT == null ? 0 : i.DISCAMT)),
                                                  qtncalcon = i.QNTYCALCON,
                                                  RATE = ((double)(i.RATE == null ? 0 : i.RATE)),
                                                  AMOUNT = ((double)(i.AMT == null ? 0 : i.AMT)),
                                                  TAXABLE = ((double)(i.TAXABLEVAL == null ? 0 : i.TAXABLEVAL)),
                                                  cessamt = ((double)(i.CESSAMT == null ? 0 : i.CESSAMT)),
                                                  cgstamt = ((double)(i.CGSTAMT == null ? 0 : i.CGSTAMT)),
                                                  igstamt = ((double)(i.IGSTAMT == null ? 0 : i.IGSTAMT)),
                                                  sgstamt = ((double)(i.SGSTAMT == null ? 0 : i.SGSTAMT)),
                                                  PRODGRPCD = i.PRODGRPCD,
                                                  NETAMOUNT = (((double)(i.TAXABLEVAL == null ? 0 : i.TAXABLEVAL)) + ((double)(i.IGSTAMT == null ? 0 : i.IGSTAMT)) + ((double)(i.CGSTAMT == null ? 0 : i.CGSTAMT)) + ((double)(i.SGSTAMT == null ? 0 : i.SGSTAMT)) + ((double)(i.CESSAMT == null ? 0 : i.CESSAMT)))
                                              }).ToList();
                            VE.PRODGRPCD = VE.ItemDetails[0].PRODGRPCD;
                        }
                        VE.TAXGRPNM = DBF.M_TAXGRP.Where(a => a.TAXGRPCD == TJBILL.TAXGRPCD).Select(a => a.TAXGRPNM).SingleOrDefault();
                        VE.BrokerSLNM = DBF.M_SUBLEG.Where(a => a.SLCD == TJBILL.BROKSLCD).Select(a => a.SLNM).SingleOrDefault();
                        VE.SLNM = DBF.M_SUBLEG.Where(a => a.SLCD == TJBILL.SLCD).Select(a => a.SLNM).SingleOrDefault();
                        VE.JOBNM = DB.M_JOBMST.Where(a => a.JOBCD == TJBILL.JOBCD).Select(a => a.JOBNM).SingleOrDefault();
                        VE.UOMNM = DBF.M_UOM.Where(a => a.UOMCD == TJBILL.UOMCD).Select(a => a.UOMNM).SingleOrDefault();
                        VE.CURRNM = DBI.MS_CURRENCY.Where(a => a.CURRCD == TJBILL.CURR_CD).Select(a => a.CURRNM).SingleOrDefault();
                        VE.TDSNM = DBF.M_TDS_CNTRL.Where(a => a.TDSCODE == TJBILL.TDSHD).Select(a => a.TDSNM).SingleOrDefault();
                        VE.LOW_TDS_DESC = Cn.getlowdednm(TJBILL.LOWTDS);
                        VE.EXPGLNM = DBF.M_GENLEG.Where(a => a.GLCD == TJBILL.EXPGLCD).Select(a => a.GLNM).SingleOrDefault();
                        VE.CREGLNM = DBF.M_GENLEG.Where(a => a.GLCD == TJBILL.CREGLCD).Select(a => a.GLNM).SingleOrDefault();
                        VE.Roundoff_Item = TJBILL.ROYN == "Y" ? true : false;
                        if (VE.MENU_PARA == "JB")
                        {
                            //DataTable Attached_CHALLANS = SALES_FUNC.GetPendChallans(TJBILL.JOBCD, TJBILL.SLCD, TJBILL.DOCDT.ToString().retDateStr(), TJBILL.AUTONO, "", "", false, true);
                            DataTable Attached_CHALLANS = SALES_FUNC.GetPendChallans(TJBILL.JOBCD, TJBILL.SLCD, TJBILL.DOCDT.ToString().retDateStr(), TJBILL.AUTONO, "", "", false, true, "", "", "", "", true);
                            var temptable = (from DataRow dr in Attached_CHALLANS.Rows
                                             select new Pending_Challan_SLIP
                                             {
                                                 AUTONO = dr["autono"].ToString(),
                                                 DOCNO = dr["docno"].ToString(),
                                                 DOCDT = dr["docdt"] == null ? "" : dr["docdt"].ToString().retDateStr(),
                                                 PREFNO = dr["prefno"].ToString(),
                                                 ISSAUTONO = dr["issautono"].ToString(),
                                                 ISSDOCNO = dr["issdocno"].ToString(),
                                                 ISSDOCDT = dr["issdocdt"] == null ? "" : dr["issdocdt"].ToString().retDateStr(),
                                                 Checked = true,
                                                 QNTY = dr["qnty"] == null ? 0 : Convert.ToDouble(dr["qnty"]),
                                                 ITCD = dr["itcd"].ToString(),
                                                 ITNM = dr["itnm"].ToString(),
                                                 STYLENO = dr["styleno"].ToString(),
                                                 SHORTQNTY = dr["SHORTQNTY"] == DBNull.Value ? 0 : Convert.ToDouble(dr["SHORTQNTY"]),
                                                 SLCD = dr["slcd"].ToString(),
                                                 PARTCD = dr["PARTCD"].ToString()
                                             }).Where(a => a.ITCD != null).OrderBy(a => a.AUTONO).ToList();
                            var javaScriptSerializer1 = new System.Web.Script.Serialization.JavaScriptSerializer();
                            string JR1 = javaScriptSerializer1.Serialize(temptable);
                            VE.BackupTable = JR1;
                            var itmdata = (from DataRow dr in Attached_CHALLANS.Rows
                                           group dr by new
                                           {
                                               AUTONO = dr["autono"].ToString(),
                                               PARTCD = dr["PARTCD"].ToString()
                                           } into X
                                           select new
                                           {
                                               AUTONO = X.Key.AUTONO,
                                               SHORTQNTY = Convert.ToDouble(X.Sum(Z => Z.Field<decimal>("SHORTQNTY"))),
                                               QNTY = Convert.ToDouble(X.Sum(Z => Z.Field<decimal>("qnty"))),
                                               DOCNO = (from DataRow i in Attached_CHALLANS.Rows where (i.Field<string>("autono") == X.Key.AUTONO) select new { DOCNO = i.Field<string>("docno") }).FirstOrDefault(),
                                               DOCDT = (from DataRow i in Attached_CHALLANS.Rows where (i.Field<string>("autono") == X.Key.AUTONO) select new { DOCDT = i.Field<DateTime>("docdt") == null ? "" : i.Field<DateTime>("docdt").ToString().retDateStr() }).FirstOrDefault(),
                                               PREFNO = (from DataRow i in Attached_CHALLANS.Rows where (i.Field<string>("autono") == X.Key.AUTONO) select new { PREFNO = i.Field<string>("prefno") }).FirstOrDefault(),
                                               PARTCD = X.Key.PARTCD,
                                               ISSAUTONO = (from DataRow i in Attached_CHALLANS.Rows where (i.Field<string>("autono") == X.Key.AUTONO) select new { ISSAUTONO = i.Field<string>("issautono") }).FirstOrDefault(),
                                               ISSDOCNO = (from DataRow i in Attached_CHALLANS.Rows where (i.Field<string>("autono") == X.Key.AUTONO) select new { ISSDOCNO = i.Field<string>("issdocno") }).FirstOrDefault(),
                                               ISSDOCDT = (from DataRow i in Attached_CHALLANS.Rows where (i.Field<string>("autono") == X.Key.AUTONO) select new { ISSDOCDT = i.Field<DateTime>("issdocdt") == null ? "" : i.Field<DateTime>("issdocdt").ToString().retDateStr() }).FirstOrDefault(),
                                               STYLENO = (from DataRow i in Attached_CHALLANS.Rows where (i.Field<string>("autono") == X.Key.AUTONO) select new { STYLENO = i.Field<string>("styleno") }).FirstOrDefault(),
                                               SLCD = (from DataRow i in Attached_CHALLANS.Rows where (i.Field<string>("autono") == X.Key.AUTONO) select new { SLCD = i.Field<string>("slcd") }).FirstOrDefault(),

                                           }).DistinctBy(a => a.AUTONO).OrderBy(a => a.AUTONO).ToList();

                            var temp = (from dr in itmdata
                                        select new Pending_Challan_SLIP
                                        {
                                            AUTONO = dr.AUTONO,
                                            DOCNO = dr.DOCNO.DOCNO,
                                            DOCDT = dr.DOCDT.DOCDT,
                                            PREFNO = dr.PREFNO.PREFNO,
                                            ISSAUTONO = dr.ISSAUTONO.ISSAUTONO,
                                            ISSDOCNO = dr.ISSDOCNO.ISSDOCNO,
                                            ISSDOCDT = dr.ISSDOCDT.ISSDOCDT,
                                            Checked = true,
                                            QNTY = dr.QNTY,
                                            ITCD = "",
                                            ITNM = "",
                                            STYLENO = dr.STYLENO.STYLENO,
                                            SHORTQNTY = dr.SHORTQNTY,
                                            SLCD = dr.SLCD.SLCD,
                                            PARTCD = dr.PARTCD
                                        }).DistinctBy(a => a.AUTONO).OrderBy(a => a.AUTONO).ToList();
                            short SL_NO = 0;
                            if (temp != null && temp.Count > 0)
                            {
                                temp.ForEach(a =>
                                {
                                    a.SLNO = ++SL_NO;
                                });
                            }
                            var QTN = temp.Sum(a => a.QNTY);
                            VE.TOTAL_P_QNTY = Convert.ToDouble(QTN);
                            var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                            string JR = javaScriptSerializer.Serialize(temp);
                            VE.ChildData_PendingSlip = JR;
                        }
                        if (VE.MENU_PARA == "JB")
                        {
                            T_JBILL SBILLJB = new T_JBILL();
                            SBILLJB = (from i in DB.T_JBILL where (i.OTHAUTONO1 == TJBILL.AUTONO) select (i)).SingleOrDefault();
                            if (SBILLJB != null)
                            {
                                VE.TAXABLEVAL_sbill = Convert.ToDouble(SBILLJB.TAXABLEVAL);
                                VE.RoundoffAMT_sbill = Convert.ToDouble(SBILLJB.ROAMT);
                                VE.Roundoff_sbill = SBILLJB.ROYN == "Y" ? true : false;
                                VE.EXPGLNM1 = DBF.M_GENLEG.Where(a => a.GLCD == SBILLJB.EXPGLCD).Select(a => a.GLNM).SingleOrDefault();
                                VE.EXPGLCD1 = SBILLJB.EXPGLCD;
                                VE.SBillSortage = (from i in DB.T_JBILLDTL
                                                   join p in DB.M_SITEM on i.ITCD equals p.ITCD
                                                   where (i.AUTONO == SBILLJB.AUTONO)
                                                   select new PendingChallan_SBill_Sortage()
                                                   {
                                                       AUTONO = i.AUTONO,
                                                       QUAN = ((double)(i.DNCNQNTY == null ? 0 : i.DNCNQNTY)),
                                                       ITCD = i.ITCD,
                                                       ITNM = p.ITNM,
                                                       PARTCD = i.PARTCD,
                                                       SLNO = i.SLNO,
                                                       EFFDT = ((DateTime)(i.EFFDT)),
                                                       igstper = ((double)(i.IGSTPER == null ? 0 : i.IGSTPER)),
                                                       cgstper = ((double)(i.CGSTPER == null ? 0 : i.CGSTPER)),
                                                       sgstper = ((double)(i.SGSTPER == null ? 0 : i.SGSTPER)),
                                                       cessper = ((double)(i.CESSPER == null ? 0 : i.CESSPER)),
                                                       qtncalcon = i.QNTYCALCON,
                                                       RATE = ((double)(i.RATE == null ? 0 : i.RATE)),
                                                       AMOUNT = ((double)(i.AMT == null ? 0 : i.AMT)),
                                                       TAXABLE = ((double)(i.TAXABLEVAL == null ? 0 : i.TAXABLEVAL)),
                                                       cessamt = ((double)(i.CESSAMT == null ? 0 : i.CESSAMT)),
                                                       cgstamt = ((double)(i.CGSTAMT == null ? 0 : i.CGSTAMT)),
                                                       igstamt = ((double)(i.IGSTAMT == null ? 0 : i.IGSTAMT)),
                                                       sgstamt = ((double)(i.SGSTAMT == null ? 0 : i.SGSTAMT)),
                                                       PRODGRPCD = i.PRODGRPCD,
                                                       NETAMOUNT = ((double)(i.TAXABLEVAL == null ? 0 : i.TAXABLEVAL)) + ((double)(i.CESSAMT == null ? 0 : i.CESSAMT)) + ((double)(i.CGSTAMT == null ? 0 : i.CGSTAMT)) + ((double)(i.IGSTAMT == null ? 0 : i.IGSTAMT)) + ((double)(i.SGSTAMT == null ? 0 : i.SGSTAMT)),
                                                   }).ToList();
                                foreach (var i in VE.SBillSortage)
                                {
                                    i.QNTY_UNIT_DNCN = DDL;
                                    i.QUAN = UOMCOnvertionFromPices(i.qtncalcon, i.QUAN);
                                }
                            }
                        }
                        T_JBILL DNCNJB = new T_JBILL();
                        if (VE.MENU_PARA == "JB")
                        {
                            DNCNJB = (from i in DB.T_JBILL where (i.OTHAUTONO == TJBILL.AUTONO) select (i)).SingleOrDefault();
                        }
                        else
                        {
                            DNCNJB = TJBILL;
                        }
                        if (DNCNJB != null)
                        {
                            VE.TAXABLEVAL_DNCNNOTE = Convert.ToDouble(DNCNJB.TAXABLEVAL);
                            VE.RoundoffAMT_DCNote = Convert.ToDouble(DNCNJB.ROAMT);
                            VE.Roundoff_DCNote = DNCNJB.ROYN == "Y" ? true : false;
                            List<DocumentType> doccd = new List<DocumentType>();
                            if (VE.MENU_PARA == "JB")
                            {
                                doccd = Cn.DOCTYPE1("JBDN");
                            }
                            else if (VE.MENU_PARA == "DN")
                            {
                                doccd = Cn.DOCTYPE1("JBDN");
                            }
                            else if (VE.MENU_PARA == "CN")
                            {
                                doccd = Cn.DOCTYPE1("JBCN");
                            }
                            VE.DRCRDetails = (from i in DB.T_JBILLDTL
                                              join p in DB.M_SITEM on i.ITCD equals p.ITCD
                                              where (i.AUTONO == DNCNJB.AUTONO)
                                              select new PendingChallanDr_Cr_NoteDetails()
                                              {
                                                  AUTONO = i.AUTONO,
                                                  QUAN = ((double)(i.DNCNQNTY == null ? 0 : i.DNCNQNTY)),
                                                  ITCD = i.ITCD,
                                                  ITNM = p.ITNM,
                                                  PARTCD = i.PARTCD,
                                                  SLNO = i.SLNO,
                                                  RelWithItem = i.AGDOCAUTONO.Length > 0 ? true : false,
                                                  ADOCNO_AUTONO = i.AGDOCAUTONO,
                                                  EFFDT = ((DateTime)(i.EFFDT)),
                                                  igstper = ((double)(i.IGSTPER == null ? 0 : i.IGSTPER)),
                                                  cgstper = ((double)(i.CGSTPER == null ? 0 : i.CGSTPER)),
                                                  sgstper = ((double)(i.SGSTPER == null ? 0 : i.SGSTPER)),
                                                  cessper = ((double)(i.CESSPER == null ? 0 : i.CESSPER)),
                                                  qtncalcon = i.QNTYCALCON,
                                                  RATE = ((double)(i.RATE == null ? 0 : i.RATE)),
                                                  AMOUNT = ((double)(i.AMT == null ? 0 : i.AMT)),
                                                  TAXABLE = ((double)(i.TAXABLEVAL == null ? 0 : i.TAXABLEVAL)),
                                                  cessamt = ((double)(i.CESSAMT == null ? 0 : i.CESSAMT)),
                                                  cgstamt = ((double)(i.CGSTAMT == null ? 0 : i.CGSTAMT)),
                                                  igstamt = ((double)(i.IGSTAMT == null ? 0 : i.IGSTAMT)),
                                                  sgstamt = ((double)(i.SGSTAMT == null ? 0 : i.SGSTAMT)),
                                                  PRODGRPCD = i.PRODGRPCD,
                                                  ADOCDT = i.AGDOCDT,
                                                  ADOCNO = i.AGDOCNO,
                                                  NETAMOUNT = ((double)(i.TAXABLEVAL == null ? 0 : i.TAXABLEVAL)) + ((double)(i.CESSAMT == null ? 0 : i.CESSAMT)) + ((double)(i.CGSTAMT == null ? 0 : i.CGSTAMT)) + ((double)(i.IGSTAMT == null ? 0 : i.IGSTAMT)) + ((double)(i.SGSTAMT == null ? 0 : i.SGSTAMT)),
                                              }).ToList();
                            foreach (var i in VE.DRCRDetails)
                            {
                                i.QNTY_UNIT_DNCN = DDL;
                                i.QUAN = UOMCOnvertionFromPices(i.qtncalcon, i.QUAN);
                                i.DocumentCode = doccd;
                                i.DcodeDRCR = DNCNJB.DOCCD;
                            }
                            if (VE.MENU_PARA != "JB")
                            {
                                VE.PRODGRPCD = VE.DRCRDetails[0].PRODGRPCD;
                            }
                        }
                        if (VE.MENU_PARA == "JB")
                        {
                            foreach (var i in VE.ItemDetails)
                            {
                                i.QNTY_UNIT_PC = DDL;
                                i.BillQNTY = UOMCOnvertionFromPices(i.qtncalcon, i.BillQNTY);
                                i.PASSQNTY = UOMCOnvertionFromPices(i.qtncalcon, i.PASSQNTY);
                                i.RECQNTY_DISPLAY = UOMCOnvertionFromPices(i.qtncalcon, i.RECQNTY_DISPLAY);
                                i.ShortQNTY_DISPLAY = UOMCOnvertionFromPices(i.qtncalcon, i.ShortQNTY_DISPLAY);
                            }
                        }
                        VE.Item_UomTotal = string.Join(", ", (from x in VE.ItemDetails
                                                              where x.UOM.retStr() != ""
                                                              group x by new
                                                              {
                                                                  x.UOM
                                                              } into P
                                                              select P.Key.UOM.retStr() + " : " + P.Sum(A => A.BillQNTY.retDbl()).retDbl()).ToList());
                        VE.Total_RECEVEQNTY = VE.ItemDetails.Sum(a => a.RECQNTY_DISPLAY).retDbl();
                        VE.Total_BillQNTY = VE.ItemDetails.Sum(a => a.BillQNTY).retDbl();
                        VE.Total_PASSQNTY = VE.ItemDetails.Sum(a => a.PASSQNTY).retDbl();
                        VE.Total_NOS = VE.ItemDetails.Sum(a => a.NOS).retDbl();
                        VE.Total_AMOUNT = VE.ItemDetails.Sum(a => a.AMOUNT).retDbl();
                        VE.Total_ShortQNTY = VE.ItemDetails.Sum(a => a.ShortQNTY_DISPLAY).retDbl();
                        VE.Total_DISCAMT = VE.ItemDetails.Sum(a => a.DISCAMT).retDbl();
                        VE.Total_addless = VE.ItemDetails.Sum(a => a.addless).retDbl();
                        VE.Total_TAXABLE = VE.ItemDetails.Sum(a => a.TAXABLE).retDbl();
                        VE.Total_igstamt = VE.ItemDetails.Sum(a => a.igstamt).retDbl();
                        VE.Total_cgstamt = VE.ItemDetails.Sum(a => a.cgstamt).retDbl();
                        VE.Total_sgstamt = VE.ItemDetails.Sum(a => a.sgstamt).retDbl();
                        VE.Total_cessamt = VE.ItemDetails.Sum(a => a.cessamt).retDbl();
                        VE.Total_NETAMOUNT = VE.ItemDetails.Sum(a => a.NETAMOUNT).retDbl();
                        VE.TDISAMT = VE.ItemDetails.Sum(a => a.DISCAMT).retDbl();
                        VE.TGSTAMT = VE.ItemDetails.Sum(a => a.igstamt + a.cgstamt + a.sgstamt + a.cessamt).retDbl();

                        string DOCDT = TJBILL.DOCDT.Value.ToString("dd/MM/yyyy");
                        string sql = "";
                        sql += "select a.effdt, a.prodgrpcd, a.igstper, a.cgstper, a.sgstper, a.cessper from ";
                        sql += "(select a.effdt, a.prodgrpcd, a.igstper, a.cgstper, a.sgstper, a.cessper, ";
                        sql += "row_number() over (partition by a.prodgrpcd order by a.effdt desc) as rn ";
                        sql += "from " + CommVar.CurSchema(UNQSNO).ToString() + ".m_prodtax a ";
                        sql += "where a.taxgrpcd='" + TJBILL.TAXGRPCD + "' and a.prodgrpcd='" + VE.PRODGRPCD + "' and ";
                        sql += "a.effdt <= to_date('" + DOCDT + "','dd/mm/yyyy') ) a where a.rn=1 ";
                        DataTable Dt = Master_Help.SQLquery(sql);
                        if (Dt != null && Dt.Rows.Count > 0)
                        {
                            VE.BackupIGSTPER = Convert.ToDouble(Dt.Rows[0]["igstper"]);
                            VE.BackupCGSTPER = Convert.ToDouble(Dt.Rows[0]["cgstper"]);
                            VE.BackupSGSTPER = Convert.ToDouble(Dt.Rows[0]["sgstper"]);
                            VE.BackupCESSTPER = Convert.ToDouble(Dt.Rows[0]["cessper"]);
                        }
                        else
                        {
                            VE.BackupIGSTPER = 0;
                            VE.BackupCGSTPER = 0;
                            VE.BackupSGSTPER = 0;
                            VE.BackupCESSTPER = 0;
                        }
                        VE.MSG = "";
                    }
                    if (TCH.CANCEL == "Y") VE.CancelRecord = true; else VE.CancelRecord = false;
                    return VE;
                }

            }

        }
        public ActionResult GetJobCode()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            return PartialView("_Help2", Master_Help.JOBCD_help(DB));
        }
        public ActionResult JobCode(string val)
        {
            //ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            //ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            using (ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO)))
            {
                using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
                {
                    var query = (from c in DB.M_JOBMST where (c.JOBCD == val) select c);
                    if (query.Any())
                    {
                        string str = "";
                        foreach (var i in query)
                        {
                            string UOMNM = (from u in DB1.M_UOM where (u.UOMCD == i.UOMCD) select u.UOMNM).SingleOrDefault();
                            string EXPGLNM = (from u in DB1.M_GENLEG where (u.GLCD == i.EXPGLCD) select u.GLNM).SingleOrDefault();
                            string SLGLNM = (from u in DB1.M_GENLEG where (u.GLCD == i.SCGLCD) select u.GLNM).SingleOrDefault();
                            str = i.JOBCD + Cn.GCS() + i.JOBNM + Cn.GCS() + i.HSNCODE + Cn.GCS() + i.PRODGRPCD + Cn.GCS() + i.UOMCD + Cn.GCS() + UOMNM + Cn.GCS() + i.EXPGLCD + Cn.GCS() + EXPGLNM + Cn.GCS() + i.SCGLCD + Cn.GCS() + SLGLNM;
                        }
                        return Content(str);
                    }
                    else
                    {
                        return Content("0");
                    }
                }
            }
        }
        public ActionResult GetSubLedgerDetails(string val, string TAG, string DOC_DT)
        {
            try
            {
                string CAPTION = (TAG == "J" ? "Jobber" : "Broker");
                if (val == null)
                {
                    return PartialView("_Help2", Master_Help.SubLeg_Help(val, TAG, CAPTION));
                }
                else
                {
                    if (TAG == "A")
                    {
                        ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                        var query = (from c in DB.M_SUBLEG where (c.SLCD == val) select c);
                        if (query.Any())
                        {
                            string str = "";
                            str = Master_Help.ToReturnFieldValues(query);
                            return Content(str);
                        }
                        else
                        {
                            return Content("Invalid " + CAPTION + " Code ! Please Enter a Valid " + CAPTION + " Code !!");
                        }
                    }
                    else
                    {
                        var PARTY_DATA = SALES_FUNC.GetSlcdDetails(val, DOC_DT, TAG);
                        var query = (from DataRow DR in PARTY_DATA.Rows select new { SLCD = DR["SLCD"], SLNM = DR["SLNM"], TAXGRPCD = DR["TAXGRPCD"], TAXGRPNM = DR["TAXGRPNM"], GSTNO = DR["GSTNO"] }).ToList();
                        if (query.Any())
                        {
                            string str = "";
                            foreach (var i in query)
                            {
                                str = Master_Help.ToReturnFieldValues(query);
                            }
                            return Content(str);
                        }
                        else
                        {
                            return Content("Invalid " + CAPTION + " Code ! Please Enter a Valid " + CAPTION + " Code !!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult CheckBillNumber(TJobBillEntry VE, string BILL_NO, string SUPPLIER, string AUTO_NO)
        {
            Cn.getQueryString(VE);
            if (VE.DefaultAction == "A") AUTO_NO = "";
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            string COM_CD = CommVar.Compcd(UNQSNO);
            string LOC_CD = CommVar.Loccd(UNQSNO);
            if (BILL_NO.retStr() == "") return Content("0");
            //if (VE.MENU_PARA != "MBPUR") return Content("0");

            var query = (from c in DB.T_JBILL
                         join d in DB.T_CNTRL_HDR on c.AUTONO equals d.AUTONO
                         where (c.PBLNO == BILL_NO && c.SLCD == SUPPLIER && c.AUTONO != AUTO_NO && d.COMPCD == COM_CD && d.CANCEL != "Y")
                         select c);
            if (query.Any())
            {
                return Content("1");
            }
            else
            {
                return Content("0");
            }

        }
        public ActionResult changeDocumentType(Permission VE, string DOC_CD, string DOCDT = "", bool AllowBDATE = false, string DOCNO = "")
        {
            Cn.getQueryString(VE);
            string str = "";
            if (DOC_CD != "0")
            {
                str = str + Cn.GCS() + "^" + Cn.GCS() + Cn.getdocmaxmindate(DOC_CD, DOCDT, VE.DefaultAction, DOCNO, VE);
            }
            return Content(str);

        }
        public ActionResult GetCurrencyDetails(string val)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                if (val == null)
                {
                    return PartialView("_Help2", Master_Help.CURRCD_help(DB));
                }
                else
                {
                    var query = (from i in DB.MS_CURRENCY where i.CURRCD == val select new { CURRCD = i.CURRCD, CURRNM = i.CURRNM }).OrderBy(s => s.CURRNM).ToList();
                    if (query.Any())
                    {
                        string str = "";
                        foreach (var i in query)
                        {
                            str = i.CURRCD + Cn.GCS() + i.CURRNM;
                        }
                        return Content(str);
                    }
                    else
                    {
                        return Content("0");
                    }
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult AddDOCRow(TJobBillEntry VE)
        {
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            var doctP = (from i in DB1.MS_DOCCTG
                         select new DocumentType()
                         {
                             value = i.DOC_CTG,
                             text = i.DOC_CTG
                         }).OrderBy(s => s.text).ToList();
            if (VE.UploadDOC == null)
            {
                List<UploadDOC> MLocIFSC1 = new List<UploadDOC>();
                UploadDOC MLI = new UploadDOC();
                MLI.DocumentType = doctP;
                MLocIFSC1.Add(MLI);
                VE.UploadDOC = MLocIFSC1;
            }
            else
            {
                List<UploadDOC> MLocIFSC1 = new List<UploadDOC>();
                for (int i = 0; i <= VE.UploadDOC.Count - 1; i++)
                {
                    UploadDOC MLI = new UploadDOC();
                    MLI = VE.UploadDOC[i];
                    MLI.DocumentType = doctP;
                    MLocIFSC1.Add(MLI);
                }
                UploadDOC MLI1 = new UploadDOC();
                MLI1.DocumentType = doctP;
                MLocIFSC1.Add(MLI1);
                VE.UploadDOC = MLocIFSC1;
            }
            VE.DefaultView = true;
            return PartialView("_UPLOADDOCUMENTS", VE);
        }
        public ActionResult DeleteDOCRow(TJobBillEntry VE)
        {
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            var doctP = (from i in DB1.MS_DOCCTG
                         select new DocumentType()
                         {
                             value = i.DOC_CTG,
                             text = i.DOC_CTG
                         }).OrderBy(s => s.text).ToList();
            List<UploadDOC> LOCAIFSC = new List<UploadDOC>();
            int count = 0;
            for (int i = 0; i <= VE.UploadDOC.Count - 1; i++)
            {
                if (VE.UploadDOC[i].chk == false)
                {
                    count += 1;
                    UploadDOC IFSC = new UploadDOC();
                    IFSC = VE.UploadDOC[i];
                    IFSC.DocumentType = doctP;
                    LOCAIFSC.Add(IFSC);
                }
            }
            VE.UploadDOC = LOCAIFSC;
            ModelState.Clear();
            VE.DefaultView = true;
            return PartialView("_UPLOADDOCUMENTS", VE);
        }
        public ActionResult GETTDSNAME(string CODE)
        {
            string str = Cn.getlowdednm(CODE);
            return Content(str);
        }
        public ActionResult GetTDSDetails(string val, string TAG, string PARTY)
        {
            try
            {
                if (TAG.retStr() == "") return Content("Enter Document Date");
                if (val == null)
                {
                    return PartialView("_Help2", Master_Help.TDSCODE_help(TAG, val, PARTY));
                }
                else
                {
                    return Content(Master_Help.TDSCODE_help(TAG, val, PARTY));
                }
            }
            catch (Exception Ex)
            {
                Cn.SaveException(Ex, "");
                return Content(Ex.Message + Ex.InnerException);
            }
        }
        public ActionResult GetPendingChallan(TJobBillEntry VE, string PARTY, string DOCDT, string JOB)
        {
            try
            {
                Cn.getQueryString(VE);
                using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
                {
                    //DataTable PENDING_CHALLANS = SALES_FUNC.GetPendChallans(JOB, PARTY, DOCDT, "", "", VE.T_JBILL.AUTONO, true, true);
                    DataTable PENDING_CHALLANS = SALES_FUNC.GetPendChallans(JOB, PARTY, DOCDT, "", "", VE.T_JBILL.AUTONO, true, true, "", "", "", "", true);
                    var temptable = (from DataRow dr in PENDING_CHALLANS.Rows
                                     select new Pending_Challan_SLIP
                                     {
                                         AUTONO = dr["autono"].ToString(),
                                         DOCNO = dr["docno"].ToString(),
                                         DOCDT = dr["docdt"] == DBNull.Value ? "" : dr["docdt"].ToString().retDateStr(),
                                         PREFNO = dr["prefno"].ToString(),
                                         PREFDT = dr["PREFDT"].retDateStr(),
                                         ISSAUTONO = dr["issautono"].ToString(),
                                         ISSDOCNO = dr["issdocno"].ToString(),
                                         ISSDOCDT = dr["issdocdt"] == DBNull.Value ? "" : dr["issdocdt"].ToString().retDateStr(),
                                         Checked = false,
                                         QNTY = (dr["qnty"] == DBNull.Value ? 0 : Convert.ToDouble(dr["qnty"])) - (dr["short_allow"] == DBNull.Value ? 0 : Convert.ToDouble(dr["short_allow"])),
                                         ITCD = dr["itcd"] == DBNull.Value ? "" : dr["itcd"].ToString(),
                                         ITNM = dr["itnm"] == DBNull.Value ? "" : dr["itnm"].ToString(),
                                         STYLENO = dr["styleno"] == DBNull.Value ? "" : dr["styleno"].ToString(),
                                         HSNSACCD = dr["hsncode"] == DBNull.Value ? "" : dr["hsncode"].ToString(),
                                         SHORTQNTY = dr["SHORTQNTY"] == DBNull.Value ? 0 : Convert.ToDouble(dr["SHORTQNTY"]),
                                         NOS = dr["NOS"].retDbl(),
                                         SLCD = dr["slcd"] == DBNull.Value ? "" : dr["slcd"].ToString(),
                                         PARTCD = dr["PARTCD"] == DBNull.Value ? "" : dr["PARTCD"].ToString(),
                                         RATE = dr["rate"] == DBNull.Value ? 0 : dr["rate"].retDbl()
                                     }).OrderBy(a => a.AUTONO).ToList();
                    if (VE.BackupTable != null)
                    {
                        if (VE.BackupTable.Length > 0)
                        {
                            var helpMT2 = new List<Pending_Challan_SLIP>();
                            var javaScriptSerializer2 = new System.Web.Script.Serialization.JavaScriptSerializer();
                            helpMT2 = javaScriptSerializer2.Deserialize<List<Pending_Challan_SLIP>>(VE.BackupTable.Replace("&quot;", "\""));
                            var yy = helpMT2.Union(temptable).DistinctBy(a => a.AUTONO + a.ITCD + a.QNTY + a.SHORTQNTY).ToList();
                            temptable = yy;
                        }
                    }
                    if (PENDING_CHALLANS != null && PENDING_CHALLANS.Rows.Count > 0)
                    {
                        Type DT1 = PENDING_CHALLANS.Rows[0]["issdocdt"].GetType();
                        Type DT2 = PENDING_CHALLANS.Rows[0]["qnty"].GetType();
                        Type DT3 = PENDING_CHALLANS.Rows[0]["nos"].GetType();
                        var itmdata = (from DataRow dr in PENDING_CHALLANS.Rows
                                       group dr by new
                                       {
                                           AUTONO = dr["autono"].ToString(),
                                           PARTCD = dr["PARTCD"].ToString()
                                       } into X
                                       select new
                                       {
                                           AUTONO = X.Key.AUTONO,
                                           SHORTQNTY = Convert.ToDouble(X.Sum(Z => Z.Field<decimal?>("SHORTQNTY") ?? 0)),
                                           QNTY = Convert.ToDouble(X.Sum(Z => Z.Field<decimal?>("qnty") ?? 0)),
                                           NOS = Convert.ToDouble(X.Sum(Z => Z.Field<double?>("nos") ?? 0)),
                                           DOCNO = (from DataRow i in PENDING_CHALLANS.Rows where (i.Field<string>("autono") == X.Key.AUTONO) select new { DOCNO = i.Field<string>("docno") }).FirstOrDefault(),
                                           DOCDT = (from DataRow i in PENDING_CHALLANS.Rows where (i.Field<string>("autono") == X.Key.AUTONO) select new { DOCDT = i.Field<DateTime>("docdt") == null ? "" : i.Field<DateTime>("docdt").ToString().retDateStr() }).FirstOrDefault(),
                                           PREFNO = (from DataRow i in PENDING_CHALLANS.Rows where (i.Field<string>("autono") == X.Key.AUTONO) select new { PREFNO = i.Field<string>("prefno") }).FirstOrDefault(),
                                           PARTCD = X.Key.PARTCD,
                                           ISSAUTONO = (from DataRow i in PENDING_CHALLANS.Rows where (i.Field<string>("autono") == X.Key.AUTONO) select new { ISSAUTONO = i.Field<string>("issautono") }).FirstOrDefault(),
                                           ISSDOCNO = (from DataRow i in PENDING_CHALLANS.Rows where (i.Field<string>("autono") == X.Key.AUTONO) select new { ISSDOCNO = i.Field<string>("issdocno") }).FirstOrDefault(),
                                           ISSDOCDT = (from DataRow i in PENDING_CHALLANS.Rows where (i.Field<string>("autono") == X.Key.AUTONO) select new { ISSDOCDT = DT1.Name == "DBNull" ? "" : i.Field<DateTime>("issdocdt").ToString().retDateStr() }).FirstOrDefault(),
                                           STYLENO = (from DataRow i in PENDING_CHALLANS.Rows where (i.Field<string>("autono") == X.Key.AUTONO) select new { STYLENO = i.Field<string>("styleno") }).FirstOrDefault(),
                                           HSNSACCD = (from DataRow i in PENDING_CHALLANS.Rows where (i.Field<string>("autono") == X.Key.AUTONO) select new { HSNSACCD = i.Field<string>("hsncode") }).FirstOrDefault(),
                                           SLCD = (from DataRow i in PENDING_CHALLANS.Rows where (i.Field<string>("autono") == X.Key.AUTONO) select new { SLCD = i.Field<string>("slcd") }).FirstOrDefault(),

                                       }).DistinctBy(a => a.AUTONO).OrderBy(a => a.AUTONO).ToList();

                        var javaScriptSerializer1 = new System.Web.Script.Serialization.JavaScriptSerializer();
                        string JR = javaScriptSerializer1.Serialize(temptable);

                        VE.Pending_SLIP = (from dr in itmdata
                                           select new Pending_Challan_SLIP
                                           {
                                               AUTONO = dr.AUTONO,
                                               DOCNO = dr.DOCNO.DOCNO,
                                               DOCDT = dr.DOCDT.DOCDT,
                                               PREFNO = dr.PREFNO.PREFNO,
                                               ISSAUTONO = dr.ISSAUTONO.ISSAUTONO,
                                               ISSDOCNO = dr.ISSDOCNO.ISSDOCNO,
                                               ISSDOCDT = dr.ISSDOCDT.ISSDOCDT,
                                               Checked = false,
                                               QNTY = dr.QNTY,
                                               NOS = dr.NOS,
                                               ITCD = "",
                                               ITNM = "",
                                               STYLENO = dr.STYLENO.STYLENO,
                                               HSNSACCD = dr.HSNSACCD.HSNSACCD,
                                               SHORTQNTY = dr.SHORTQNTY,
                                               SLCD = dr.SLCD.SLCD,
                                               PARTCD = dr.PARTCD
                                           }).DistinctBy(a => a.AUTONO).OrderBy(a => a.AUTONO).ToList();
                        VE.BackupTable = JR;
                        var helpMT = new List<Pending_Challan_SLIP>();
                        var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                        if (VE.ChildData_PendingSlip != null)
                        {
                            helpMT = javaScriptSerializer.Deserialize<List<Pending_Challan_SLIP>>(VE.ChildData_PendingSlip);
                            if (helpMT.Any())
                            {
                                if (VE.DefaultAction == "A")
                                {
                                    VE.Pending_SLIP = helpMT;
                                }
                                else
                                {
                                    var yy = helpMT.Union(VE.Pending_SLIP).DistinctBy(a => a.AUTONO).ToList();
                                    VE.Pending_SLIP = yy;
                                }
                            }
                        }
                        short SL_NO = 0;
                        if (VE.Pending_SLIP != null && VE.Pending_SLIP.Count > 0)
                        {
                            VE.Pending_SLIP.ForEach(a =>
                            {
                                a.SLNO = ++SL_NO;
                            });
                        }

                        var QTN = VE.Pending_SLIP.Sum(a => a.QNTY);
                        VE.TOTAL_P_QNTY = Convert.ToDouble(QTN);
                        VE.TOTAL_P_NOS = VE.Pending_SLIP.Sum(a => a.NOS).retDbl();
                    }
                    VE.DefaultView = true;
                    ModelState.Clear();
                    return PartialView("_T_JobBill_Pending_Challan_Grid", VE);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetItemsDetails(TJobBillEntry VE, string DOCDT, string JOB)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                VE.MSG = "";
                string sql = "";
                sql += "select a.effdt, a.prodgrpcd, a.igstper, a.cgstper, a.sgstper, a.cessper from ";
                sql += "(select a.effdt, a.prodgrpcd, a.igstper, a.cgstper, a.sgstper, a.cessper, ";
                sql += "row_number() over (partition by a.prodgrpcd order by a.effdt desc) as rn ";
                sql += "from " + CommVar.CurSchema(UNQSNO) + ".m_prodtax a ";
                sql += "where a.taxgrpcd='" + VE.T_JBILL.TAXGRPCD + "' and a.prodgrpcd='" + VE.PRODGRPCD + "' and ";
                sql += "a.effdt <= to_date('" + DOCDT + "','dd/mm/yyyy') ) a where a.rn=1 ";
                DataTable Dt = Master_Help.SQLquery(sql);
                if (Dt.Rows.Count > 0)
                {
                    VE.BackupIGSTPER = Convert.ToDouble(Dt.Rows[0]["igstper"]);
                    VE.BackupCGSTPER = Convert.ToDouble(Dt.Rows[0]["cgstper"]);
                    VE.BackupSGSTPER = Convert.ToDouble(Dt.Rows[0]["sgstper"]);
                    VE.BackupCESSTPER = Convert.ToDouble(Dt.Rows[0]["cessper"]);
                }
                else
                {
                    VE.BackupIGSTPER = 0;
                    VE.BackupCGSTPER = 0;
                    VE.BackupSGSTPER = 0;
                    VE.BackupCESSTPER = 0;
                }
                List<DocumentType> DDL = new List<DocumentType>(); //qntycalcon combo                   
                                                                   //DocumentType DDL1 = new DocumentType(); DDL1.text = "Unit"; DDL1.value = "P"; DDL.Add(DDL1);
                                                                   //DocumentType DDL2 = new DocumentType(); DDL2.text = "Dzn"; DDL2.value = "D"; DDL.Add(DDL2);
                                                                   //DocumentType DDL3 = new DocumentType(); DDL3.text = "Box"; DDL3.value = "B"; DDL.Add(DDL3);
                DocumentType DDL2 = new DocumentType(); DDL2.text = "Qnty"; DDL2.value = "Q"; DDL.Add(DDL2);
                DocumentType DDL1 = new DocumentType(); DDL1.text = "Nos"; DDL1.value = "N"; DDL.Add(DDL1);
                List<PendingChallanItemDetails> PCID = new List<PendingChallanItemDetails>();
                List<PendingChallan_SBill_Sortage> PCDCND = new List<PendingChallan_SBill_Sortage>();
                Int16 SLNO = 0; Int16 SLNO1 = 0;
                var helpMT = new List<Pending_Challan_SLIP>();
                var javaScriptSerializer1 = new System.Web.Script.Serialization.JavaScriptSerializer();
                string strJSON = VE.BackupTable.Replace("&quot;", "\"");
                helpMT = javaScriptSerializer1.Deserialize<List<Pending_Challan_SLIP>>(strJSON);
                List<string> selauto = new List<string>();
                foreach (var i in VE.Pending_SLIP)
                {
                    if (i.Checked)
                    {
                        selauto.Add(i.AUTONO);
                    }
                }
                //var itmdata = (from dr in helpMT
                //               where (selauto.Contains(dr.AUTONO))
                //               group dr by new
                //               {
                //                   ITCD = dr.ITCD,
                //                   ITNM = dr.ITNM,
                //                   STYLENO = dr.STYLENO,
                //                   HSNSACCD = dr.HSNSACCD
                //               } into X
                //               select new
                //               {
                //                   ITCD = X.Key.ITCD,
                //                   ITNM = X.Key.ITNM,
                //                   SHORTQNTY = Convert.ToDouble(X.Sum(Z => Z.SHORTQNTY)),
                //                   QNTY = Convert.ToDouble(X.Sum(Z => Z.QNTY)),
                //                   STYLENO = X.Key.STYLENO,
                //                   HSNSACCD = X.Key.HSNSACCD,
                //               }).ToList();
                var itmdata = (from dr in helpMT
                               where (selauto.Contains(dr.AUTONO))
                               select new
                               {
                                   ITCD = dr.ITCD,
                                   ITNM = dr.ITNM,
                                   SHORTQNTY = dr.SHORTQNTY,
                                   QNTY = dr.QNTY,
                                   NOS = dr.NOS,
                                   STYLENO = dr.STYLENO,
                                   HSNSACCD = dr.HSNSACCD,
                                   RATE = dr.RATE,
                                   PREFNO = dr.PREFNO,
                                   PREFDT = dr.PREFDT,

                               }).ToList();
                string PREFNO = itmdata.Where(a => a.PREFNO.retStr() != "").Select(a => a.PREFNO).LastOrDefault();
                string PREFDT = itmdata.Where(a => a.PREFNO.retStr() != "").Select(a => a.PREFDT).LastOrDefault();
                foreach (var i in itmdata)
                {
                    if (i.ITCD != "")
                    {
                        string qtyCalcAs = retQtyCalcas(JOB);
                        SLNO += 1;
                        PendingChallanItemDetails PC = new PendingChallanItemDetails();
                        PC.QNTY_UNIT_PC = DDL;
                        PC.qtncalcon = qtyCalcAs;
                        PC.AUTONO = "";
                        PC.BillQNTY = i.QNTY + i.SHORTQNTY;
                        PC.NOS = i.NOS;
                        PC.ShortQNTY = i.SHORTQNTY;
                        PC.ShortQNTY_DISPLAY = i.SHORTQNTY;
                        PC.ITCD = i.ITCD;
                        PC.ITNM = i.ITNM;
                        PC.STYLENO = i.STYLENO;
                        //PC.PARTCD = i.PARTCD;
                        PC.PASSQNTY = i.QNTY;
                        PC.RECQNTY = i.QNTY;
                        PC.RECQNTY_DISPLAY = i.QNTY;
                        PC.SLNO = SLNO;
                        PC.HSNSACCD = i.HSNSACCD;
                        PC.RATE = i.RATE;
                        PC.UOM = (from a in DB.M_SITEM where a.ITCD == PC.ITCD select a.UOMCD).FirstOrDefault();
                        if (Dt != null && Dt.Rows.Count > 0)
                        {
                            PC.EFFDT = Cn.convstr2date(Dt.Rows[0]["effdt"].ToString());
                            PC.igstper = Convert.ToDouble(Dt.Rows[0]["igstper"]);
                            PC.cgstper = Convert.ToDouble(Dt.Rows[0]["cgstper"]);
                            PC.sgstper = Convert.ToDouble(Dt.Rows[0]["sgstper"]);
                            PC.cessper = Convert.ToDouble(Dt.Rows[0]["cessper"]);
                        }
                        PCID.Add(PC);
                        PendingChallan_SBill_Sortage PCND = new PendingChallan_SBill_Sortage();
                        if (i.SHORTQNTY > 0)
                        {
                            SLNO1 += 1;
                            if (Dt != null && Dt.Rows.Count > 0)
                            {
                                PCND.EFFDT = Cn.convstr2date(Dt.Rows[0]["effdt"].ToString());
                                PCND.igstper = Convert.ToDouble(Dt.Rows[0]["igstper"]);
                                PCND.cgstper = Convert.ToDouble(Dt.Rows[0]["cgstper"]);
                                PCND.sgstper = Convert.ToDouble(Dt.Rows[0]["sgstper"]);
                                PCND.cessper = Convert.ToDouble(Dt.Rows[0]["cessper"]);
                            }
                            PCND.ITCD = i.ITCD;
                            PCND.ITNM = i.ITNM;
                            PCND.STYLENO = i.STYLENO;
                            PCND.HSNSACCD = i.HSNSACCD;
                            PCND.RelWithItem = true;
                            PCND.ADOCNO_AUTONO = SLNO.ToString();
                            //PCND.PARTCD = i.PARTCD;
                            PCND.QNTY_UNIT_DNCN = DDL;
                            PCND.SLNO = SLNO1;
                            PCND.QUAN = i.SHORTQNTY;
                            PCND.RATE = i.RATE;
                            PCDCND.Add(PCND);
                        }
                    }
                }
                var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                string JR = javaScriptSerializer.Serialize(VE.Pending_SLIP);
                VE.ChildData_PendingSlip = JR;
                VE.ItemDetails = PCID;
                VE.SBillSortage = PCDCND;
                VE.RoundoffAMT_sbill = 0.00;
                VE.TAXABLEVAL_sbill = 0;
                VE.TOTALBILLAMT_sbill = 0;
                VE.Roundoff_Item = true;
                VE.Roundoff_sbill = true;

                VE.Item_UomTotal = string.Join(", ", (from x in VE.ItemDetails
                                                      where x.UOM.retStr() != ""
                                                      group x by new
                                                      {
                                                          x.UOM
                                                      } into P
                                                      select P.Key.UOM.retStr() + " : " + P.Sum(A => A.BillQNTY.retDbl()).retDbl()).ToList());

                ModelState.Clear();
                var Itemdetails = RenderRazorViewToString(ControllerContext, "_T_JobBill_ItemDetails", VE);
                var SbillDetails = RenderRazorViewToString(ControllerContext, "_T_JobBill_SBillDetails", VE);
                return Content(Itemdetails + "^^^^^^^^^^^^~~~~~~^^^^^^^^^^" + SbillDetails + "^^^^^^^^^^^^~~~~~~^^^^^^^^^^" + PREFNO + "^^^^^^^^^^^^~~~~~~^^^^^^^^^^" + PREFDT);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                VE.MSG = ex.Message + " " + ex.InnerException;
                return Content("0");
            }
        }
        public static string RenderRazorViewToString(ControllerContext controllerContext, string viewName, object model)
        {
            controllerContext.Controller.ViewData.Model = model;

            using (var stringWriter = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controllerContext, viewName);
                var viewContext = new ViewContext(controllerContext, viewResult.View, controllerContext.Controller.ViewData, controllerContext.Controller.TempData, stringWriter);
                viewResult.View.Render(viewContext, stringWriter);
                viewResult.ViewEngine.ReleaseView(controllerContext, viewResult.View);
                return stringWriter.GetStringBuilder().ToString();
            }
        }
        public ActionResult AddRow(TJobBillEntry VE, string DOCDT)
        {
            try
            {
                VE.MSG = "";
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                List<DocumentType> DDL = new List<DocumentType>(); //qntycalcon combo                   
                DocumentType DDL1 = new DocumentType(); DDL1.text = "Unit"; DDL1.value = "P"; DDL.Add(DDL1);
                DocumentType DDL2 = new DocumentType(); DDL2.text = "Dzn"; DDL2.value = "D"; DDL.Add(DDL2);
                DocumentType DDL3 = new DocumentType(); DDL3.text = "Box"; DDL3.value = "B"; DDL.Add(DDL3);
                List<DocumentType> doccd = new List<DocumentType>();
                if (VE.MENU_PARA == "JB")
                {
                    doccd = Cn.DOCTYPE1("JBDN");
                }
                else
                {
                    doccd = Cn.DOCTYPE1(VE.DOC_CODE);
                }
                string sql = "";
                sql += "select a.effdt, a.prodgrpcd, a.igstper, a.cgstper, a.sgstper, a.cessper from ";
                sql += "(select a.effdt, a.prodgrpcd, a.igstper, a.cgstper, a.sgstper, a.cessper, ";
                sql += "row_number() over (partition by a.prodgrpcd order by a.effdt desc) as rn ";
                sql += "from " + CommVar.CurSchema(UNQSNO).ToString() + ".m_prodtax a ";
                sql += "where a.taxgrpcd='" + VE.T_JBILL.TAXGRPCD + "' and a.prodgrpcd='" + VE.PRODGRPCD + "' and ";
                sql += "a.effdt <= to_date('" + DOCDT + "','dd/mm/yyyy') ) a where a.rn=1 ";
                DataTable Dt = Master_Help.SQLquery(sql);
                double igstper = 0, cgstper = 0, sgstper = 0, cessper = 0;
                if (Dt.Rows.Count > 0)
                {
                    igstper = Convert.ToDouble(Dt.Rows[0]["igstper"]);
                    cgstper = Convert.ToDouble(Dt.Rows[0]["cgstper"]);
                    sgstper = Convert.ToDouble(Dt.Rows[0]["sgstper"]);
                    cessper = Convert.ToDouble(Dt.Rows[0]["cessper"]);
                }
                if (VE.DRCRDetails == null)
                {
                    List<PendingChallanDr_Cr_NoteDetails> TXNDTL_HEAD = new List<PendingChallanDr_Cr_NoteDetails>();
                    PendingChallanDr_Cr_NoteDetails DTL = new PendingChallanDr_Cr_NoteDetails();
                    DTL.SLNO = 1;
                    DTL.igstper = igstper;
                    DTL.cgstper = cgstper;
                    DTL.sgstper = sgstper;
                    DTL.cessper = cessper;
                    TXNDTL_HEAD.Add(DTL);
                    VE.DRCRDetails = TXNDTL_HEAD;
                }
                else
                {
                    List<PendingChallanDr_Cr_NoteDetails> TXNDTL = new List<PendingChallanDr_Cr_NoteDetails>();
                    for (int i = 0; i <= VE.DRCRDetails.Count - 1; i++)
                    {
                        PendingChallanDr_Cr_NoteDetails MIB = new PendingChallanDr_Cr_NoteDetails();
                        MIB = VE.DRCRDetails[i];
                        TXNDTL.Add(MIB);
                    }
                    PendingChallanDr_Cr_NoteDetails TXNDTL1 = new PendingChallanDr_Cr_NoteDetails();
                    var max = VE.DRCRDetails.Max(a => Convert.ToInt32(a.SLNO));
                    int SLNO = Convert.ToInt32(max) + 1;
                    TXNDTL1.SLNO = Convert.ToSByte(SLNO);
                    TXNDTL1.igstper = igstper;
                    TXNDTL1.cgstper = cgstper;
                    TXNDTL1.sgstper = sgstper;
                    TXNDTL1.cessper = cessper;
                    TXNDTL.Add(TXNDTL1);
                    VE.DRCRDetails = TXNDTL;
                }
                VE.DRCRDetails.ForEach(a => { a.QNTY_UNIT_DNCN = DDL; a.DocumentCode = doccd; });
                VE.DefaultView = true;
                return PartialView("_T_JBill_Dr_Cr_Note", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
        public ActionResult DeleteRow(TJobBillEntry VE)
        {
            try
            {
                VE.MSG = "";
                List<DocumentType> DDL = new List<DocumentType>(); //qntycalcon combo                   
                DocumentType DDL1 = new DocumentType(); DDL1.text = "Unit"; DDL1.value = "P"; DDL.Add(DDL1);
                DocumentType DDL2 = new DocumentType(); DDL2.text = "Dzn"; DDL2.value = "D"; DDL.Add(DDL2);
                DocumentType DDL3 = new DocumentType(); DDL3.text = "Box"; DDL3.value = "B"; DDL.Add(DDL3);
                List<DocumentType> doccd = new List<DocumentType>();
                if (VE.MENU_PARA == "JB")
                {
                    doccd = Cn.DOCTYPE1("JBDN,JBCN");
                }
                else
                {
                    doccd = Cn.DOCTYPE1(VE.DOC_CODE);
                }
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                short SL_NO = 0;
                VE.DRCRDetails = (from x in VE.DRCRDetails where x.Checked == false select x).ToList();
                VE.DRCRDetails.ForEach(a => { a.QNTY_UNIT_DNCN = DDL; a.SLNO = ++SL_NO; a.DocumentCode = doccd; });
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_JBill_Dr_Cr_Note", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
        public ActionResult GetItems(string Sval, string Flag = "0")
        {
            using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
            {
                if (Flag == "0")
                {
                    return null;// PartialView("_Help2", Master_Help.ARTICLE_ITEM_DETAILS(null));
                }
                else
                {
                    string res = "";// Master_Help.ARTICLE_ITEM_DETAILS(Sval, "F", "", "", "C");
                    if (res.IndexOf("Invalid Item Code") > -1)
                    {
                        res = "0";
                    }
                    if (res == "0")
                    {
                        return Content("0");
                    }
                    else
                    {
                        string[] results = res.Split(Convert.ToChar(Cn.GCS()));
                        return Content(results[1] + "‡" + results[2] + "‡" + results[4]);
                    }
                }
            }
        }
        public ActionResult cancelRecords(TJobBillEntry VE, string par1)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));

            string auto = VE.T_JBILL.AUTONO, auto1 = "", auto2 = "";
            auto1 = VE.T_JBILL.OTHAUTONO.retStr(); auto2 = VE.T_JBILL.OTHAUTONO1.retStr();
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".T_CNTRL_HDR in  row share mode");
                    T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                    if (par1 == "*#*")
                    {
                        TCH = Cn.T_CONTROL_HDR(auto, CommVar.CurSchema(UNQSNO));
                        if (auto1 != "") TCH = Cn.T_CONTROL_HDR(auto1, CommVar.CurSchema(UNQSNO));
                        if (auto2 != "") TCH = Cn.T_CONTROL_HDR(auto2, CommVar.CurSchema(UNQSNO));
                    }
                    else
                    {
                        TCH = Cn.T_CONTROL_HDR(VE.T_JBILL.AUTONO, CommVar.CurSchema(UNQSNO), par1);
                        if (auto1 != "") TCH = Cn.T_CONTROL_HDR(auto1, CommVar.CurSchema(UNQSNO), par1);
                        if (auto2 != "") TCH = Cn.T_CONTROL_HDR(auto2, CommVar.CurSchema(UNQSNO), par1);
                    }
                    DB.Entry(TCH).State = System.Data.Entity.EntityState.Modified;
                    DB.SaveChanges();

                    DBF.Database.ExecuteSqlCommand("lock table " + CommVar.FinSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode");
                    T_CNTRL_HDR TCH1 = new T_CNTRL_HDR();
                    if (par1 == "*#*")
                    {
                        TCH1 = Cn.T_CONTROL_HDR(auto, CommVar.FinSchema(UNQSNO));
                        if (auto1 != "") TCH1 = Cn.T_CONTROL_HDR(auto1, CommVar.FinSchema(UNQSNO));
                        if (auto2 != "") TCH1 = Cn.T_CONTROL_HDR(auto2, CommVar.FinSchema(UNQSNO));
                    }
                    else
                    {
                        TCH1 = Cn.T_CONTROL_HDR(auto, CommVar.FinSchema(UNQSNO), par1);
                        if (auto1 != "") TCH1 = Cn.T_CONTROL_HDR(auto1, CommVar.FinSchema(UNQSNO), par1);
                        if (auto2 != "") TCH1 = Cn.T_CONTROL_HDR(auto2, CommVar.FinSchema(UNQSNO), par1);
                    }
                    DBF.Entry(TCH1).State = System.Data.Entity.EntityState.Modified;
                    DBF.SaveChanges();
                    transaction.Commit();
                    return Content("1");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Cn.SaveException(ex, "");
                    return Content(ex.Message + ex.InnerException);
                }
            }
        }
        private T_JBILL FillJB(TJobBillEntry VE, out string Ddate1, string DOCCD, out string DOCPATTERN1, out string auto_no1, out string Month1, string recotype, string M_SLIP_NO = "")
        {
            //ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
            {
                T_JBILL TJBILL = new T_JBILL();
                TJBILL.DOCDT = VE.T_JBILL.DOCDT;
                TJBILL.CLCD = CommVar.ClientCode(UNQSNO);
                string Ddate = Convert.ToString(TJBILL.DOCDT);
                string DOCPATTERN = "";
                string auto_no = "";
                string Month = "";
                if (VE.DefaultAction == "A")
                {
                    TJBILL.EMD_NO = 0;
                    TJBILL.DOCCD = DOCCD;
                    //TJBILL.DOCNO = Cn.MaxDocNumber(TJBILL.DOCCD, Ddate);
                    if (VE.M_SLIP_NO.retStr().Trim(' ') != "")
                    {
                        TJBILL.DOCNO = Convert.ToString(VE.M_SLIP_NO).PadLeft(6, '0');
                    }
                    else
                    {
                        TJBILL.DOCNO = Cn.MaxDocNumber(TJBILL.DOCCD, Ddate);
                    }
                    DOCPATTERN = Cn.DocPattern(Convert.ToInt32(TJBILL.DOCNO), TJBILL.DOCCD, CommVar.CurSchema(UNQSNO).ToString(), CommVar.FinSchema(UNQSNO), Ddate);
                    auto_no = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), TJBILL.DOCNO, TJBILL.DOCCD, Ddate);
                    TJBILL.AUTONO = auto_no.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
                    Month = auto_no.Split(Convert.ToChar(Cn.GCS()))[1].ToString();
                }
                else
                {
                    string autonocheck = "";
                    if (recotype == "S")
                    {
                        autonocheck = VE.SBillSortage[0].AUTONO;
                        var sdocno = (from p in DB.T_CNTRL_HDR where p.AUTONO == autonocheck select new { DOCNO = p.DOCONLYNO }).ToList();
                        if (sdocno != null)
                        {
                            TJBILL.DOCNO = sdocno[0].DOCNO;
                        }
                    }
                    else if (recotype == "D")
                    {
                        autonocheck = VE.DRCRDetails[0].AUTONO;
                        var sdocno = (from p in DB.T_CNTRL_HDR where p.AUTONO == autonocheck select new { DOCNO = p.DOCONLYNO }).ToList();
                        if (sdocno != null)
                        {
                            TJBILL.DOCNO = sdocno[0].DOCNO;
                        }
                    }
                    else
                    {
                        TJBILL.DOCNO = VE.T_JBILL.DOCNO;
                        autonocheck = VE.T_JBILL.AUTONO;
                    }
                    TJBILL.DOCCD = DOCCD;
                    TJBILL.AUTONO = autonocheck;
                    Month = VE.T_CNTRL_HDR.MNTHCD;
                    var MAXEMDNO = (from p in DB.T_CNTRL_HDR where p.AUTONO == autonocheck select p.EMD_NO).Max();
                    if (MAXEMDNO == null) { TJBILL.EMD_NO = 0; } else { TJBILL.EMD_NO = Convert.ToByte(MAXEMDNO + 1); }
                }
                DOCPATTERN1 = DOCPATTERN;
                auto_no1 = auto_no;
                Month1 = Month;
                Ddate1 = Ddate;
                return TJBILL;
            }
        }
        public ActionResult ParkRecord(FormCollection FC, TJobBillEntry stream, string menuID, string menuIndex)
        {
            try
            {
                Connection cn = new Connection();
                string ID = menuID + menuIndex + CommVar.Loccd(UNQSNO) + CommVar.Compcd(UNQSNO) + CommVar.CurSchema(UNQSNO).ToString() + "*" + DateTime.Now;
                ID = ID.Replace(" ", "_");
                string Userid = Session["UR_ID"].ToString();
                INI Handel_ini = new INI();
                var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                string JR = javaScriptSerializer.Serialize(stream);
                Handel_ini.IniWriteValue(Userid, ID, cn.Encrypt(JR), Server.MapPath("~/Park.ini"));
                return Content("1");
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
        public string RetrivePark(string value)
        {
            try
            {
                string url = Master_Help.RetriveParkFromFile(value, Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString(), "Improvar.ViewModels.TJobBillEntry");
                return url;
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return ex.Message;
            }
        }
        public ActionResult SearchPannelData()
        {
            TJobBillEntry VE = new TJobBillEntry();
            string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), yrcd = CommVar.YearCode(UNQSNO);
            Cn.getQueryString(VE);
            VE.DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
            string MNUP = VE.MENU_PARA;
            string[] XYZ = VE.DocumentType.Select(i => i.value).ToArray();
            string doccd = XYZ.retSqlfromStrarray();
            string sql = "";
            sql += "select distinct a.autono, d.doccd, to_char(d.docdt,'dd/mm/yyyy') docdt, d.docno, d.doconlyno, c.jobnm,nvl(d.cancel,'N')cancel, a.slcd, e.slnm, a.pblno, d.docdt ddocdt ";
            sql += "from " + scm + ".t_jbill a, " + scm + ".m_jobmst c, " + scm + ".t_cntrl_hdr d, " + scmf + ".m_subleg e ";
            sql += "where a.slcd = e.slcd(+) and a.autono = d.autono(+) and a.jobcd=c.jobcd(+) and ";
            sql += "d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and d.yr_cd = '" + yrcd + "' and d.doccd in (" + doccd + ") ";
            sql += "order by docno, ddocdt ";
            DataTable tbl = Master_Help.SQLquery(sql);

            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Doc No" + Cn.GCS() + "Doc Date" + Cn.GCS() + "Job Name" + Cn.GCS() + "P/Blno" + Cn.GCS() + "Party Name" + Cn.GCS() + "AUTONO";
            for (int j = 0; j <= tbl.Rows.Count - 1; j++)
            {
                string cancel = tbl.Rows[j]["cancel"].retStr() == "Y" ? "<b> (Cancelled)</b>" : "";
                SB.Append("<tr><td><b>" + tbl.Rows[j]["docno"] + "</b> [" + tbl.Rows[j]["doccd"] + "]" + cancel + "</td><td>" + tbl.Rows[j]["docdt"] + "</td><td>" + tbl.Rows[j]["jobnm"] + "</td><td>" + tbl.Rows[j]["pblno"] +
                    " </td><td><b>" + tbl.Rows[j]["slnm"] + " </b> (" + tbl.Rows[j]["slcd"] + ") </td><td>" + tbl.Rows[j]["autono"] + " </td></tr>");
            }
            return PartialView("_SearchPannel2", masfa.Generate_SearchPannel(hdr, SB.ToString(), "5", "5"));
        }
        public double UOMCOnvertionFromPices(string uom, double qty)
        {
            double rtvL = 0;
            string chk = qty.ToString("0.00");
            string sbefd = chk.Substring(0, chk.Length - 3);
            string saftd = chk.Substring(chk.Length - 2, 2);
            double box = Convert.ToDouble(sbefd);
            double lpcs = Convert.ToDouble(saftd);
            if (uom == "D")
            {
                var temp = (box + lpcs) / 12;
                var chk1 = temp.ToString("0.00");
                var sbefd1 = chk1.Substring(0, chk1.Length - 3);
                var box1 = Convert.ToDouble(sbefd1);
                var lpcs1 = box - (box1 * 12);
                if (lpcs1 > 0)
                {
                    var pp = lpcs1.ToString();
                    pp = pp.PadLeft(2, '0');
                    rtvL = Convert.ToDouble(box1 + "." + pp);
                }
                else {
                    rtvL = box1;
                }
            }
            else
            {
                rtvL = qty;
            }
            return rtvL;
        }
        public ActionResult GetLedgerCode(string TAG, string val = null)
        {
            if (val == null)
            {
                return null;// PartialView("_Help2", Master_Help.VCH_GENLEDGER(TAG, val));
            }
            else
            {
                return null;// Content(Master_Help.VCH_GENLEDGER(TAG, val));
            }
        }
        public string retQtyCalcas(string jobcd)
        {
            string rval = "";
            switch (jobcd)
            {
                case "KT":
                case "DY":
                case "BL":
                case "FP":
                case "IR":
                    rval = "U"; break;
                case "CT":
                case "PR":
                case "WA":
                    rval = "D"; break;
                case "ST":
                    rval = "B"; break;
            }
            return rval;
        }
        public ActionResult SAVE(FormCollection FC, TJobBillEntry VE)
        {
            Cn.getQueryString(VE);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            //Oracle Queries
            string dberrmsg = "";
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            //OracleTransaction OraTrans;
            string dbsql = "", postdt = "", weekrem = "", duedatecalcon = "", sql = "";
            string[] dbsql1;
            double dbDrAmt = 0, dbCrAmt = 0;

            //OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted);
            //OraCmd.Transaction = OraTrans;
            //
            using (OracleTransaction OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                using (var transaction = DB.Database.BeginTransaction())
                {
                    try
                    {
                        try
                        {
                            string ContentFlg = "";
                            DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode");
                            string dr = ""; string cr = ""; int isl = 1; string strrem = "";
                            double igst = 0, cgst = 0, sgst = 0, cess = 0, duty = 0, dbqty = 0, dbamt = 0, dbcurramt = 0;
                            Int32 z = 0; Int32 maxR = 0;
                            bool blactpost = true;
                            string revcharge = "N";
                            if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                            {
                                List<T_CNTRL_HDR> TCH = new List<T_CNTRL_HDR>();
                                List<T_JBILL> TJB = new List<T_JBILL>();
                                List<T_JBILLDTL> TJBD = new List<T_JBILLDTL>();
                                List<T_TXN_LINKNO> TXNL = new List<T_TXN_LINKNO>();
                                T_JBILL TJBILL = new T_JBILL();
                                T_CNTRL_HDR TCH_ITEM = new T_CNTRL_HDR();
                                T_JBILL TJBILL_DNCN = new T_JBILL();
                                T_CNTRL_HDR TCH_DNCN = new T_CNTRL_HDR();
                                T_CNTRL_HDR TCH_SBILL = new T_CNTRL_HDR();
                                string Ddate = "";
                                string DOCPATTERN = "";
                                string DNCNDOCPATTERN = "";
                                string SBILLDOCPATTERN = "";
                                string auto_no = ""; string Month = "";
                                string Month_Sbill = ""; string Month_DNCN = "";
                                T_JBILL TJBILL_SBILL = new T_JBILL();
                                if (VE.DRCRDetails != null)
                                {
                                    if (VE.DRCRDetails.Count > 0)
                                    {
                                        if (VE.MENU_PARA == "JB")
                                        {
                                            string doccd = VE.DRCRDetails[0].DcodeDRCR;
                                            TJBILL_DNCN = FillJB(VE, out Ddate, doccd, out DNCNDOCPATTERN, out auto_no, out Month_DNCN, "D");
                                        }
                                        else
                                        {
                                            TJBILL_DNCN = FillJB(VE, out Ddate, VE.T_JBILL.DOCCD, out DNCNDOCPATTERN, out auto_no, out Month_DNCN, "D");
                                        }
                                    }
                                }
                                if (VE.SBillSortage != null)
                                {
                                    if (VE.SBillSortage.Count > 0)
                                    {
                                        string doccd = DB.M_DOCTYPE.Where(a => a.DOCTYPE == "SSHSB").Select(a => a.DOCCD).SingleOrDefault();
                                        if (doccd == null)
                                        {
                                            return Content("Sale Bill Shortage Document Code Not Found!!");
                                        }
                                        TJBILL_SBILL = FillJB(VE, out Ddate, doccd, out SBILLDOCPATTERN, out auto_no, out Month_Sbill, "S");
                                    }
                                }
                                if (VE.MENU_PARA == "JB")
                                {
                                    if (VE.T_JBILL.DUECALCON == null) return Content("Due Date Calc from blank !");
                                    else
                                    {
                                        double ddys = 0;
                                        if (VE.T_JBILL.CRDAYS != null) ddys = Convert.ToDouble(VE.T_JBILL.CRDAYS);
                                        if (VE.T_JBILL.DUEDT == null) VE.T_JBILL.DUEDT = VE.T_JBILL.DUECALCON == "P" ? Convert.ToDateTime(VE.T_JBILL.PBLDT).AddDays(ddys) : Convert.ToDateTime(VE.T_JBILL.DOCDT).AddDays(ddys);
                                    }
                                    TJBILL = FillJB(VE, out Ddate, VE.T_JBILL.DOCCD, out DOCPATTERN, out auto_no, out Month, "J", VE.M_SLIP_NO);
                                    if (VE.DefaultAction == "E")
                                    {
                                        if (VE.MENU_PARA == "JB")
                                        {
                                            if (VE.DRCRDetails != null)
                                            {
                                                if (VE.DRCRDetails.Count > 0)
                                                {
                                                    if (VE.T_JBILL.OTHAUTONO == null)
                                                    {
                                                        string docno = Cn.MaxDocNumber(TJBILL_DNCN.DOCCD, Ddate);
                                                        string auto = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), docno, TJBILL_DNCN.DOCCD, Ddate);
                                                        DNCNDOCPATTERN = Cn.DocPattern(Convert.ToInt32(docno), TJBILL_DNCN.DOCCD, CommVar.CurSchema(UNQSNO).ToString(), CommVar.FinSchema(UNQSNO), Ddate);
                                                        TJBILL_DNCN.AUTONO = auto.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
                                                        TJBILL_DNCN.DOCNO = docno;
                                                    }
                                                    else
                                                    {
                                                        TJBILL_DNCN.AUTONO = VE.T_JBILL.OTHAUTONO;
                                                    }
                                                }
                                            }
                                            if (VE.SBillSortage != null)
                                            {
                                                if (VE.SBillSortage.Count > 0)
                                                {
                                                    TJBILL_SBILL.AUTONO = VE.T_JBILL.OTHAUTONO1;
                                                }
                                            }
                                        }
                                    }
                                    TJBILL.SLCD = VE.T_JBILL.SLCD;
                                    TJBILL.PBLNO = VE.T_JBILL.PBLNO;
                                    TJBILL.PBLDT = VE.T_JBILL.PBLDT;
                                    TJBILL.CRDAYS = VE.T_JBILL.CRDAYS;
                                    TJBILL.DUECALCON = VE.T_JBILL.DUECALCON;
                                    TJBILL.DUEDT = VE.T_JBILL.DUEDT;
                                    TJBILL.BROKSLCD = VE.T_JBILL.BROKSLCD;
                                    TJBILL.JOBCD = VE.T_JBILL.JOBCD;
                                    TJBILL.HSNCODE = VE.T_JBILL.HSNCODE;
                                    TJBILL.EXPGLCD = VE.T_JBILL.EXPGLCD;
                                    TJBILL.CREGLCD = VE.T_JBILL.CREGLCD;
                                    TJBILL.UOMCD = VE.T_JBILL.UOMCD;
                                    TJBILL.TAXABLEVAL = VE.TAXABLEVAL_ITEM;
                                    TJBILL.ROYN = VE.Roundoff_Item == true ? "Y" : "N";
                                    TJBILL.ROAMT = VE.RoundoffAMT_Item;
                                    TJBILL.BLAMT = VE.T_JBILL.BLAMT;
                                    TJBILL.TDSHD = VE.T_JBILL.TDSHD;
                                    TJBILL.TDSON = VE.T_JBILL.TDSON;
                                    TJBILL.TDSPER = VE.T_JBILL.TDSPER;
                                    TJBILL.TDSAMT = VE.T_JBILL.TDSAMT;
                                    TJBILL.LOWTDS = VE.T_JBILL.LOWTDS;
                                    TJBILL.ADVAMT = VE.T_JBILL.ADVAMT;
                                    TJBILL.CURR_CD = VE.T_JBILL.CURR_CD;
                                    TJBILL.CURRRT = VE.T_JBILL.CURRRT;
                                    TJBILL.REVCHG = VE.T_JBILL.REVCHG;
                                    TJBILL.OTHAUTONO = TJBILL_DNCN.AUTONO;
                                    if (VE.TOTALBILLAMT_sbill > 0) TJBILL.OTHAUTONO1 = TJBILL_SBILL.AUTONO;
                                    TJBILL.DOCTAG = "JB";
                                    TJBILL.DNCNAMT = VE.T_JBILL.DNCNAMT;
                                    TJBILL.DNCNAMT1 = VE.T_JBILL.DNCNAMT1;
                                    TJBILL.TAXGRPCD = VE.T_JBILL.TAXGRPCD;
                                    //TCH_ITEM = Cn.T_CONTROL_HDR(TJBILL.DOCCD, TJBILL.DOCDT, TJBILL.DOCNO, TJBILL.AUTONO, Month, DOCPATTERN, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString(), null, TJBILL.SLCD, Convert.ToDouble(VE.T_JBILL.BLAMT), null);
                                    TCH_ITEM = Cn.T_CONTROL_HDR(TJBILL.DOCCD, TJBILL.DOCDT, TJBILL.DOCNO, TJBILL.AUTONO, Month, DOCPATTERN, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString(), null, TJBILL.SLCD, Convert.ToDouble(VE.T_JBILL.BLAMT), null, VE.Audit_REM);
                                    //TCH_ITEM = Cn.Model_T_Cntrl_Hdr(DB, TJBILL.AUTONO, TJBILL.DOCCD, TJBILL.DOCDT.Value, TJBILL.DOCNO, Month, DOCPATTERN, VE.DefaultAction, null, TJBILL.SLCD, Convert.ToDouble(VE.T_JBILL.BLAMT));
                                    TJB.Add(TJBILL);
                                    TCH.Add(TCH_ITEM);
                                    foreach (var i in VE.ItemDetails)
                                    {
                                        T_JBILLDTL TD = new T_JBILLDTL();
                                        TD.EMD_NO = TJBILL.EMD_NO;
                                        TD.CLCD = TJBILL.CLCD;
                                        TD.DTAG = TJBILL.DTAG;
                                        TD.TTAG = TJBILL.TTAG;
                                        TD.AUTONO = TJBILL.AUTONO;
                                        TD.SLNO = i.SLNO;
                                        TD.DRCR = "C";
                                        TD.ITCD = i.ITCD;
                                        TD.ITREM = i.ITREM;
                                        TD.PARTCD = i.PARTCD;
                                        TD.QNTYCALCON = i.qtncalcon;
                                        TD.DNCNQNTY = i.ShortQNTY;
                                        TD.PQNTY = i.PASSQNTY;
                                        TD.BQNTY = i.BillQNTY;
                                        TD.NOS = i.NOS;
                                        TD.ADDLESSAMT = i.addless;
                                        //TD.PQNTY = i.PASSQNTY;
                                        //TD.BQNTY = i.BillQNTY;
                                        TD.RQNTY = i.RECQNTY;
                                        TD.RATE = i.RATE;
                                        TD.AMT = i.AMOUNT;
                                        TD.DISCPER = i.DISCPER;
                                        TD.DISCAMT = i.DISCAMT;
                                        TD.TAXABLEVAL = i.TAXABLE;
                                        TD.IGSTPER = i.igstper;
                                        TD.IGSTAMT = i.igstamt;
                                        TD.CGSTPER = i.cgstper;
                                        TD.CGSTAMT = i.cgstamt;
                                        TD.SGSTPER = i.sgstper;
                                        TD.SGSTAMT = i.sgstamt;
                                        TD.CESSPER = i.cessper;
                                        TD.CESSAMT = i.cessamt;
                                        TD.PRODGRPCD = VE.PRODGRPCD;
                                        TD.EFFDT = i.EFFDT;
                                        TJBD.Add(TD);
                                    }
                                    #region Sbill_Shortage
                                    if (VE.SBillSortage != null)
                                    {
                                        if (VE.SBillSortage.Count > 0 && VE.TOTALBILLAMT_sbill > 0)
                                        {
                                            TJBILL_SBILL.SLCD = VE.T_JBILL.SLCD;
                                            TJBILL_SBILL.EXPGLCD = VE.EXPGLCD1;
                                            TJBILL_SBILL.CREGLCD = VE.T_JBILL.CREGLCD;
                                            TJBILL_SBILL.PBLNO = VE.T_JBILL.PBLNO;
                                            TJBILL_SBILL.PBLDT = VE.T_JBILL.PBLDT;
                                            TJBILL_SBILL.CRDAYS = VE.T_JBILL.CRDAYS;
                                            TJBILL_SBILL.DUECALCON = VE.T_JBILL.DUECALCON;
                                            TJBILL_SBILL.DUEDT = VE.T_JBILL.DUEDT;
                                            TJBILL_SBILL.BROKSLCD = VE.T_JBILL.BROKSLCD;
                                            TJBILL_SBILL.JOBCD = VE.T_JBILL.JOBCD;
                                            TJBILL_SBILL.HSNCODE = VE.T_JBILL.HSNCODE;
                                            TJBILL_SBILL.UOMCD = VE.T_JBILL.UOMCD;
                                            TJBILL_SBILL.TAXABLEVAL = VE.TAXABLEVAL_sbill;
                                            TJBILL_SBILL.ROYN = VE.Roundoff_sbill == true ? "Y" : "N";
                                            TJBILL_SBILL.ROAMT = VE.RoundoffAMT_sbill;
                                            TJBILL_SBILL.BLAMT = VE.TOTALBILLAMT_sbill; // shortdata[0].NETAMT; //.VE.T_JBILL.BLAMT;
                                            TJBILL_SBILL.TDSHD = null; // VE.T_JBILL.TDSHD;
                                            TJBILL_SBILL.TDSON = 0; // VE.T_JBILL.TDSON;
                                            TJBILL_SBILL.TDSPER = 0; // VE.T_JBILL.TDSPER;
                                            TJBILL_SBILL.TDSAMT = 0;// VE.T_JBILL.TDSAMT;
                                            TJBILL_SBILL.LOWTDS = null; // VE.T_JBILL.LOWTDS;
                                            TJBILL_SBILL.ADVAMT = VE.T_JBILL.ADVAMT;
                                            TJBILL_SBILL.CURR_CD = VE.T_JBILL.CURR_CD;
                                            TJBILL_SBILL.CURRRT = VE.T_JBILL.CURRRT;
                                            TJBILL_SBILL.REVCHG = null; // VE.T_JBILL.REVCHG;
                                            TJBILL_SBILL.DOCTAG = "JS";
                                            TJBILL_SBILL.OTHAUTONO1 = TJBILL.AUTONO;
                                            TJBILL_SBILL.DNCNAMT1 = VE.T_JBILL.DNCNAMT1;
                                            TJBILL_SBILL.TAXGRPCD = VE.T_JBILL.TAXGRPCD;
                                            //TCH_SBILL = Cn.T_CONTROL_HDR(TJBILL_SBILL.DOCCD, TJBILL_SBILL.DOCDT, TJBILL_SBILL.DOCNO, TJBILL_SBILL.AUTONO, Month_Sbill, SBILLDOCPATTERN, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString(), null, TJBILL_SBILL.SLCD, Convert.ToDouble(VE.T_JBILL.DNCNAMT1), null);
                                            TCH_SBILL = Cn.Model_T_Cntrl_Hdr(DB, TJBILL_SBILL.AUTONO, TJBILL_SBILL.DOCCD, TJBILL_SBILL.DOCDT.Value, TJBILL_SBILL.DOCNO, Month_Sbill, SBILLDOCPATTERN, VE.DefaultAction, null, TJBILL_SBILL.SLCD, Convert.ToDouble(VE.T_JBILL.DNCNAMT1));
                                            TJB.Add(TJBILL_SBILL);
                                            TCH.Add(TCH_SBILL);
                                            foreach (var i in VE.SBillSortage)
                                            {
                                                T_JBILLDTL TD = new T_JBILLDTL();
                                                TD.EMD_NO = TJBILL.EMD_NO;
                                                TD.CLCD = TJBILL.CLCD;
                                                TD.DTAG = TJBILL.DTAG;
                                                TD.TTAG = TJBILL.TTAG;
                                                if (i.RelWithItem)
                                                {
                                                    TD.AGDOCAUTONO = TJBILL_SBILL.OTHAUTONO1;
                                                }
                                                TD.AUTONO = TJBILL_SBILL.AUTONO;
                                                TD.SLNO = i.SLNO;
                                                TD.DRCR = "D";
                                                TD.ITCD = i.ITCD;
                                                TD.PARTCD = i.PARTCD;
                                                TD.QNTYCALCON = i.qtncalcon;
                                                TD.DNCNQNTY = i.QUAN;
                                                TD.RATE = i.RATE;
                                                TD.AMT = i.AMOUNT;
                                                TD.TAXABLEVAL = i.TAXABLE;
                                                TD.IGSTPER = i.igstper;
                                                TD.IGSTAMT = i.igstamt;
                                                TD.CGSTPER = i.cgstper;
                                                TD.CGSTAMT = i.cgstamt;
                                                TD.SGSTPER = i.sgstper;
                                                TD.SGSTAMT = i.sgstamt;
                                                TD.CESSPER = i.cessper;
                                                TD.CESSAMT = i.cessamt;
                                                TD.PRODGRPCD = VE.PRODGRPCD;
                                                TD.EFFDT = i.EFFDT;
                                                TJBD.Add(TD);
                                            }
                                        }
                                    }
                                    #endregion
                                }

                                if (VE.DRCRDetails != null)
                                {
                                    if (VE.DRCRDetails.Count > 0)
                                    {
                                        TJBILL_DNCN.SLCD = VE.T_JBILL.SLCD;
                                        TJBILL_DNCN.EXPGLCD = VE.T_JBILL.EXPGLCD;
                                        TJBILL_DNCN.CREGLCD = VE.T_JBILL.CREGLCD;
                                        TJBILL_DNCN.PBLNO = VE.T_JBILL.PBLNO;
                                        TJBILL_DNCN.PBLDT = VE.T_JBILL.PBLDT;
                                        TJBILL_DNCN.CRDAYS = VE.T_JBILL.CRDAYS;
                                        TJBILL_DNCN.DUECALCON = VE.T_JBILL.DUECALCON;
                                        TJBILL_DNCN.DUEDT = VE.T_JBILL.DUEDT;
                                        TJBILL_DNCN.BROKSLCD = VE.T_JBILL.BROKSLCD;
                                        TJBILL_DNCN.JOBCD = VE.T_JBILL.JOBCD;
                                        TJBILL_DNCN.HSNCODE = VE.T_JBILL.HSNCODE;
                                        TJBILL_DNCN.UOMCD = VE.T_JBILL.UOMCD;
                                        TJBILL_DNCN.TAXABLEVAL = VE.TAXABLEVAL_DNCNNOTE;
                                        TJBILL_DNCN.ROYN = VE.Roundoff_DCNote == true ? "Y" : "N";
                                        TJBILL_DNCN.ROAMT = VE.RoundoffAMT_DCNote;
                                        TJBILL_DNCN.BLAMT = VE.T_JBILL.BLAMT;
                                        TJBILL_DNCN.TDSHD = VE.T_JBILL.TDSHD;
                                        TJBILL_DNCN.TDSON = VE.T_JBILL.TDSON;
                                        TJBILL_DNCN.TDSPER = VE.T_JBILL.TDSPER;
                                        TJBILL_DNCN.TDSAMT = VE.T_JBILL.TDSAMT;
                                        TJBILL_DNCN.LOWTDS = VE.T_JBILL.LOWTDS;
                                        TJBILL_DNCN.ADVAMT = VE.T_JBILL.ADVAMT;
                                        TJBILL_DNCN.CURR_CD = VE.T_JBILL.CURR_CD;
                                        TJBILL_DNCN.CURRRT = VE.T_JBILL.CURRRT;
                                        TJBILL_DNCN.REVCHG = VE.T_JBILL.REVCHG;
                                        if (VE.MENU_PARA == "JB")
                                        {
                                            TJBILL_DNCN.DOCTAG = "JD";
                                        }
                                        else if (VE.MENU_PARA == "DN")
                                        {
                                            TJBILL_DNCN.DOCTAG = "JD";
                                        }
                                        else
                                        {
                                            TJBILL_DNCN.DOCTAG = "JC";
                                        }
                                        if (VE.MENU_PARA == "JB")
                                        {
                                            TJBILL_DNCN.OTHAUTONO = TJBILL.AUTONO;
                                        }
                                        else
                                        {
                                            TJBILL_DNCN.OTHAUTONO = VE.T_JBILL.OTHAUTONO;
                                        }
                                        TJBILL_DNCN.DNCNAMT = VE.T_JBILL.DNCNAMT;
                                        TJBILL_DNCN.TAXGRPCD = VE.T_JBILL.TAXGRPCD;
                                        if (VE.T_JBILL.OTHAUTONO == null)
                                        {
                                            //TCH_DNCN = Cn.T_CONTROL_HDR(TJBILL_DNCN.DOCCD, TJBILL_DNCN.DOCDT, TJBILL_DNCN.DOCNO, TJBILL_DNCN.AUTONO, Month_DNCN, DNCNDOCPATTERN, "A", CommVar.CurSchema(UNQSNO).ToString(), null, TJBILL_DNCN.SLCD, Convert.ToDouble(VE.T_JBILL.DNCNAMT), null);
                                            TCH_DNCN = Cn.T_CONTROL_HDR(TJBILL_DNCN.DOCCD, TJBILL_DNCN.DOCDT, TJBILL_DNCN.DOCNO, TJBILL_DNCN.AUTONO, Month_DNCN, DNCNDOCPATTERN, "A", CommVar.CurSchema(UNQSNO).ToString(), null, TJBILL_DNCN.SLCD, Convert.ToDouble(VE.T_JBILL.DNCNAMT), null, VE.Audit_REM);
                                            //TCH_DNCN = Cn.Model_T_Cntrl_Hdr(DB, TJBILL_DNCN.AUTONO, TJBILL_DNCN.DOCCD, TJBILL_DNCN.DOCDT.Value, TJBILL_DNCN.DOCNO, Month_DNCN, DNCNDOCPATTERN, "A", null, TJBILL_DNCN.SLCD, Convert.ToDouble(VE.T_JBILL.DNCNAMT));
                                        }
                                        else
                                        {
                                            //TCH_DNCN = Cn.T_CONTROL_HDR(TJBILL_DNCN.DOCCD, TJBILL_DNCN.DOCDT, TJBILL_DNCN.DOCNO, TJBILL_DNCN.AUTONO, Month_DNCN, DNCNDOCPATTERN, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString(), null, TJBILL_DNCN.SLCD, Convert.ToDouble(VE.T_JBILL.DNCNAMT), null);
                                            TCH_DNCN = Cn.T_CONTROL_HDR(TJBILL_DNCN.DOCCD, TJBILL_DNCN.DOCDT, TJBILL_DNCN.DOCNO, TJBILL_DNCN.AUTONO, Month_DNCN, DNCNDOCPATTERN, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString(), null, TJBILL_DNCN.SLCD, Convert.ToDouble(VE.T_JBILL.DNCNAMT), null, VE.Audit_REM);
                                            //TCH_DNCN = Cn.Model_T_Cntrl_Hdr(DB, TJBILL_DNCN.AUTONO, TJBILL_DNCN.DOCCD, TJBILL_DNCN.DOCDT.Value, TJBILL_DNCN.DOCNO, Month_DNCN, DNCNDOCPATTERN, VE.DefaultAction, null, TJBILL_DNCN.SLCD, Convert.ToDouble(VE.T_JBILL.DNCNAMT));
                                        }
                                        TJB.Add(TJBILL_DNCN);
                                        TCH.Add(TCH_DNCN);
                                        foreach (var i in VE.DRCRDetails)
                                        {
                                            T_JBILLDTL TD = new T_JBILLDTL();
                                            if (VE.MENU_PARA == "JB")
                                            {
                                                TD.EMD_NO = TJBILL.EMD_NO;
                                                TD.CLCD = TJBILL.CLCD;
                                                TD.DTAG = TJBILL.DTAG;
                                                TD.TTAG = TJBILL.TTAG;
                                            }
                                            else
                                            {
                                                TD.EMD_NO = TJBILL_DNCN.EMD_NO;
                                                TD.CLCD = TJBILL_DNCN.CLCD;
                                                TD.DTAG = TJBILL_DNCN.DTAG;
                                                TD.TTAG = TJBILL_DNCN.TTAG;
                                            }
                                            if (i.RelWithItem)
                                            {
                                                TD.AGDOCAUTONO = TJBILL_DNCN.OTHAUTONO;
                                            }
                                            TD.AUTONO = TJBILL_DNCN.AUTONO;
                                            TD.SLNO = i.SLNO;
                                            if (VE.MENU_PARA == "JB")
                                            {
                                                TD.DRCR = "D";
                                            }
                                            else if (VE.MENU_PARA == "DN")
                                            {
                                                TD.DRCR = "D";
                                            }
                                            else
                                            {
                                                TD.DRCR = "C";
                                            }
                                            TD.ITCD = i.ITCD;
                                            TD.PARTCD = i.PARTCD;
                                            TD.QNTYCALCON = i.qtncalcon;
                                            TD.DNCNQNTY = i.PCSPERBOX;

                                            //TD.DNCNQNTY = i.QUAN;
                                            TD.RATE = i.RATE;
                                            TD.AMT = i.AMOUNT;
                                            TD.TAXABLEVAL = i.TAXABLE;
                                            TD.IGSTPER = i.igstper;
                                            TD.IGSTAMT = i.igstamt;
                                            TD.CGSTPER = i.cgstper;
                                            TD.CGSTAMT = i.cgstamt;
                                            TD.SGSTPER = i.sgstper;
                                            TD.SGSTAMT = i.sgstamt;
                                            TD.CESSPER = i.cessper;
                                            TD.CESSAMT = i.cessamt;
                                            TD.PRODGRPCD = VE.PRODGRPCD;
                                            TD.EFFDT = i.EFFDT;
                                            TD.AGDOCNO = i.ADOCNO;
                                            TD.AGDOCDT = i.ADOCDT;
                                            TJBD.Add(TD);
                                        }
                                    }
                                }
                                if (VE.MENU_PARA == "JB")
                                {
                                    var helpMT = new List<Pending_Challan_SLIP>();
                                    var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                                    if (VE.ChildData_PendingSlip != null)
                                    {
                                        helpMT = javaScriptSerializer.Deserialize<List<Pending_Challan_SLIP>>(VE.ChildData_PendingSlip);
                                        if (helpMT.Any())
                                        {
                                            foreach (var i in helpMT)
                                            {
                                                if (i.Checked)
                                                {
                                                    T_TXN_LINKNO TTX = new T_TXN_LINKNO();
                                                    TTX.EMD_NO = TJBILL.EMD_NO;
                                                    TTX.CLCD = TJBILL.CLCD;
                                                    TTX.DTAG = TJBILL.DTAG;
                                                    TTX.TTAG = TJBILL.TTAG;
                                                    TTX.AUTONO = TJBILL.AUTONO;
                                                    TTX.LINKAUTONO = i.AUTONO;
                                                    TTX.ISSAUTONO = i.ISSAUTONO;
                                                    TXNL.Add(TTX);
                                                }
                                            }
                                        }
                                    }
                                }

                                DataTable rstmp;
                                string sslcd = VE.T_JBILL.SLCD;
                                string scm1 = CommVar.CurSchema(UNQSNO);
                                string scmf = CommVar.FinSchema(UNQSNO);
                                //if (VE.PSLCD != null) sslcd = VE.PSLCD.ToString();
                                if (VE.DefaultAction == "E")
                                {
                                    if (VE.MENU_PARA == "JB")
                                    {
                                        DB.T_JBILL.Where(x => x.AUTONO == VE.T_JBILL.AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                        DB.T_JBILL.Where(x => x.AUTONO == VE.T_JBILL.OTHAUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                        DB.T_JBILL.Where(x => x.AUTONO == VE.T_JBILL.OTHAUTONO1).ToList().ForEach(x => { x.DTAG = "E"; });

                                        DB.T_JBILLDTL.Where(x => x.AUTONO == VE.T_JBILL.OTHAUTONO1).ToList().ForEach(x => { x.DTAG = "E"; });
                                        DB.T_JBILLDTL.Where(x => x.AUTONO == VE.T_JBILL.OTHAUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                        DB.T_JBILLDTL.Where(x => x.AUTONO == VE.T_JBILL.AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });

                                        DB.T_TXN_LINKNO.Where(x => x.AUTONO == VE.T_JBILL.AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                        DB.SaveChanges();
                                        DB.T_TXN_LINKNO.RemoveRange(DB.T_TXN_LINKNO.Where(x => x.AUTONO == VE.T_JBILL.AUTONO));
                                        DB.SaveChanges();

                                        DB.T_JBILLDTL.RemoveRange(DB.T_JBILLDTL.Where(x => x.AUTONO == VE.T_JBILL.AUTONO));
                                        DB.SaveChanges();

                                        DB.T_JBILLDTL.RemoveRange(DB.T_JBILLDTL.Where(x => x.AUTONO == VE.T_JBILL.OTHAUTONO));
                                        DB.SaveChanges();

                                        DB.T_JBILLDTL.RemoveRange(DB.T_JBILLDTL.Where(x => x.AUTONO == VE.T_JBILL.OTHAUTONO1));
                                        DB.SaveChanges();

                                        DB.T_JBILL.RemoveRange(DB.T_JBILL.Where(x => x.AUTONO == VE.T_JBILL.OTHAUTONO));
                                        DB.SaveChanges();

                                        DB.T_JBILL.RemoveRange(DB.T_JBILL.Where(x => x.AUTONO == VE.T_JBILL.OTHAUTONO1));
                                        DB.SaveChanges();

                                        DB.T_CNTRL_HDR_REM.Where(x => x.AUTONO == VE.T_JBILL.AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                        DB.T_CNTRL_HDR_REM.RemoveRange(DB.T_CNTRL_HDR_REM.Where(x => x.AUTONO == VE.T_JBILL.AUTONO));
                                        DB.T_CNTRL_HDR_DOC.Where(x => x.AUTONO == VE.T_JBILL.AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                        DB.T_CNTRL_HDR_DOC.RemoveRange(DB.T_CNTRL_HDR_DOC.Where(x => x.AUTONO == VE.T_JBILL.AUTONO));
                                        DB.T_CNTRL_HDR_DOC_DTL.Where(x => x.AUTONO == VE.T_JBILL.AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                        DB.T_CNTRL_HDR_DOC_DTL.RemoveRange(DB.T_CNTRL_HDR_DOC_DTL.Where(x => x.AUTONO == VE.T_JBILL.AUTONO));
                                        DB.T_JBILL.RemoveRange(DB.T_JBILL.Where(x => x.AUTONO == VE.T_JBILL.AUTONO));
                                        DB.SaveChanges();
                                        if (VE.DRCRDetails == null)
                                        {
                                            DB.T_CNTRL_HDR.Where(x => x.AUTONO == VE.T_JBILL.OTHAUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                                            DB.T_CNTRL_HDR.RemoveRange(DB.T_CNTRL_HDR.Where(x => x.AUTONO == VE.T_JBILL.OTHAUTONO));
                                            DB.SaveChanges();
                                        }
                                    }
                                    else
                                    {
                                        DB.T_JBILL.Where(x => x.AUTONO == VE.T_JBILL.AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });

                                        DB.T_JBILLDTL.Where(x => x.AUTONO == VE.T_JBILL.AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });

                                        DB.T_JBILLDTL.RemoveRange(DB.T_JBILLDTL.Where(x => x.AUTONO == VE.T_JBILL.AUTONO));
                                        DB.SaveChanges();

                                        DB.T_CNTRL_HDR_REM.Where(x => x.AUTONO == VE.T_JBILL.AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                        DB.T_CNTRL_HDR_REM.RemoveRange(DB.T_CNTRL_HDR_REM.Where(x => x.AUTONO == VE.T_JBILL.AUTONO));

                                        DB.T_CNTRL_HDR_DOC.Where(x => x.AUTONO == VE.T_JBILL.AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                        DB.T_CNTRL_HDR_DOC.RemoveRange(DB.T_CNTRL_HDR_DOC.Where(x => x.AUTONO == VE.T_JBILL.AUTONO));

                                        DB.T_CNTRL_HDR_DOC_DTL.Where(x => x.AUTONO == VE.T_JBILL.AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                        DB.T_CNTRL_HDR_DOC_DTL.RemoveRange(DB.T_CNTRL_HDR_DOC_DTL.Where(x => x.AUTONO == VE.T_JBILL.AUTONO));

                                        DB.T_JBILL.RemoveRange(DB.T_JBILL.Where(x => x.AUTONO == VE.T_JBILL.AUTONO));
                                        DB.SaveChanges();
                                    }
                                    List<string> selauto = new List<string>();
                                    selauto.Add(VE.T_JBILL.AUTONO);
                                    if (VE.MENU_PARA == "JB")
                                    {
                                        selauto.Add(VE.T_JBILL.OTHAUTONO);
                                        selauto.Add(VE.T_JBILL.OTHAUTONO1);
                                    }
                                    string sqlautono = "'" + string.Join("','", selauto) + "'";

                                    dbsql = Master_Help.finTblUpdt("t_vch_gst", sqlautono, "E");
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                                    dbsql = Master_Help.finTblUpdt("t_tdstxn", sqlautono, "E");
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                                    dbsql = Master_Help.finTblUpdt("t_vch_bl_adj", sqlautono, "E");
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                                    dbsql = Master_Help.finTblUpdt("t_vch_bl", sqlautono, "E");
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                                    dbsql = Master_Help.finTblUpdt("t_vch_class", sqlautono, "E");
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                                    dbsql = Master_Help.finTblUpdt("t_tdstxn", sqlautono, "E");
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                                    dbsql = Master_Help.finTblUpdt("t_vch_det", sqlautono, "E");
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                                    dbsql = Master_Help.finTblUpdt("t_vch_hdr", sqlautono, "E");
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                                    //
                                    if (VE.T_JBILL.PBLNO == null || VE.T_JBILL.PBLNO.ToString() == "")
                                    {
                                        dberrmsg = "Party Bill no can not be blank."; goto dbnotsave;
                                    }
                                    if (VE.T_JBILL.PBLDT == null)
                                    {
                                        dberrmsg = "Party Bill date can not be blank."; goto dbnotsave;
                                    }
                                }
                                if (VE.UploadDOC != null)// add
                                {
                                    var img = VE.MENU_PARA == "JB" ? Cn.SaveUploadImageTransaction(VE.UploadDOC, TJBILL.AUTONO, TJBILL.EMD_NO.Value) : Cn.SaveUploadImageTransaction(VE.UploadDOC, TJBILL_DNCN.AUTONO, TJBILL_DNCN.EMD_NO.Value);
                                    if (img.Item1.Count != 0)
                                    {
                                        DB.T_CNTRL_HDR_DOC.AddRange(img.Item1);
                                        DB.T_CNTRL_HDR_DOC_DTL.AddRange(img.Item2);
                                    }
                                }
                                if (VE.T_CNTRL_HDR_REM.DOCREM != null)// add REMARKS
                                {
                                    var NOTE = VE.MENU_PARA == "JB" ? Cn.SAVETRANSACTIONREMARKS(VE.T_CNTRL_HDR_REM, TJBILL.AUTONO, TJBILL.CLCD, TJBILL.EMD_NO.Value) : Cn.SAVETRANSACTIONREMARKS(VE.T_CNTRL_HDR_REM, TJBILL_DNCN.AUTONO, TJBILL_DNCN.CLCD, TJBILL_DNCN.EMD_NO.Value);
                                    if (NOTE.Item1.Count != 0)
                                    {
                                        DB.T_CNTRL_HDR_REM.AddRange(NOTE.Item1);
                                    }
                                }
                                if (VE.DefaultAction == "A")
                                {
                                    DB.T_CNTRL_HDR.AddRange(TCH);
                                    DB.T_JBILL.AddRange(TJB);
                                    DB.T_JBILLDTL.AddRange(TJBD);
                                    if (VE.MENU_PARA == "JB")
                                    {
                                        DB.T_TXN_LINKNO.AddRange(TXNL);
                                    }
                                    if (VE.MENU_PARA == "JB")
                                    {
                                        ContentFlg = "1" + " (Voucher No. " + TJBILL.DOCNO + ")~" + TJBILL.AUTONO;
                                    }
                                    else
                                    {
                                        ContentFlg = "1" + " (Voucher No. " + TJBILL_DNCN.DOCNO + ")~" + TJBILL_DNCN.AUTONO;
                                    }
                                }
                                else if (VE.DefaultAction == "E")
                                {
                                    foreach (var u in TCH)
                                    {
                                        if (u.DTAG == "E")
                                        {
                                            DB.Entry(u).State = System.Data.Entity.EntityState.Modified;
                                        }
                                        else
                                        {
                                            DB.T_CNTRL_HDR.Add(u);
                                        }
                                    }
                                    DB.T_JBILL.AddRange(TJB);
                                    DB.T_JBILLDTL.AddRange(TJBD);
                                    if (VE.MENU_PARA == "JB")
                                    {
                                        DB.T_TXN_LINKNO.AddRange(TXNL);
                                    }
                                    ContentFlg = "2";
                                }
                                #region Financial Posting
                                for (int j = 0; j < TJB.Count; j++)
                                {
                                    DataTable tbl;
                                    sql = "";
                                    sql += "select b.rogl, '" + VE.T_JBILL.JOBCD + "' class1cd, null class2cd, ";
                                    if (TJB[j].DOCTAG == "JS") sql += "'" + VE.T_JBILL.CREGLCD + "' parglcd, b.igst_s igstcd, b.cgst_s cgstcd, b.sgst_s sgstcd, b.cess_s cesscd, b.duty_s dutycd, ";
                                    else sql += "'" + VE.T_JBILL.CREGLCD + "' parglcd, b.igst_p igstcd, b.cgst_p cgstcd, b.sgst_p sgstcd, b.cess_p cesscd, b.duty_p dutycd, ";
                                    sql += "igst_rvi, cgst_rvi, sgst_rvi, igst_rvo, cgst_rvo, sgst_rvo, cess_rvo ";
                                    sql += "from " + scmf + ".m_post b, " + scm1 + ".m_jobmst c ";
                                    sql += "where c.jobcd='" + VE.T_JBILL.JOBCD + "' ";
                                    tbl = Master_Help.SQLquery(sql);

                                    string dncntag = "", exemptype = "", salpur = "", expglcd = "";
                                    dr = "C"; cr = "D";
                                    strrem = "Thru J/Bill " + TJB[j].PBLNO + " dtd " + TJB[j].PBLDT.ToString().retDateStr();
                                    string trcd = "JB";
                                    expglcd = TJB[j].EXPGLCD;
                                    revcharge = TJB[j].REVCHG;
                                    igst = 0; cgst = 0; sgst = 0; cess = 0; duty = 0; dbqty = 0; dbamt = 0; dbcurramt = 0;
                                    switch (TJB[j].DOCTAG)
                                    {
                                        case "JS":
                                            dr = "D"; cr = "C";
                                            strrem = "Thru Shortage/Bill " + TCH[j].DOCNO + " dtd " + TJB[j].DOCDT.ToString().retDateStr();
                                            trcd = "SB";
                                            dncntag = "";
                                            salpur = "S";
                                            //expglcd = "10050008";
                                            sql = "select shortage_glcd from " + CommVar.CurSchema(UNQSNO) + ".M_syscnfg";
                                            DataTable msyscnfg = Master_Help.SQLquery(sql);
                                            if (msyscnfg.Rows.Count > 0)
                                            {
                                                expglcd = msyscnfg.Rows[0]["shortage_glcd"].retStr();
                                            }
                                            break;
                                        case "JD":
                                            dr = "D"; cr = "C";
                                            strrem = "Thru J/DN agst " + TJBD[0].AGDOCNO + " dtd " + TJBD[0].AGDOCDT.ToString().retDateStr();
                                            trcd = "PD";
                                            dncntag = "PD";
                                            salpur = "P";
                                            TJB[j].BLAMT = Convert.ToDouble(TJB[j].DNCNAMT == null ? 0 : TJB[j].DNCNAMT);
                                            break;
                                        case "JC":
                                            dr = "C"; cr = "D";
                                            strrem = "Thru J/CN agst " + TJBD[0].AGDOCNO + " dtd " + TJBD[0].AGDOCDT.ToString().retDateStr();
                                            trcd = "PC";
                                            dncntag = "PC";
                                            salpur = "P";
                                            TJB[j].BLAMT = Convert.ToDouble(TJB[j].DNCNAMT == null ? 0 : TJB[j].DNCNAMT);
                                            break;
                                        default:
                                            dr = "C"; cr = "D";
                                            strrem = "Thru J/Bill " + TJB[j].PBLNO + " dtd " + TJB[j].PBLDT.ToString().retDateStr();
                                            trcd = "PB";
                                            salpur = "P";
                                            break;
                                    }
                                    // create header record in finschema
                                    bool recoins = true;
                                    if (TCH[j].DOCAMT.Value == 0) recoins = false;
                                    if (recoins == false) blactpost = false;
                                    if (blactpost == true)
                                    {
                                        short emdno = TCH[j].EMD_NO.retShort();
                                        dbsql = Master_Help.Instcntrl_hdr(TCH[j].AUTONO, VE.DefaultAction, "F", TCH[j].MNTHCD, TCH[j].DOCCD, TCH[j].DOCNO, TCH[j].DOCDT.ToString(), emdno, TCH[j].DTAG, TCH[j].DOCONLYNO, TCH[j].VCHRNO, "Y", TCH[j].VCHRSUFFIX, TCH[j].GLCD, TCH[j].SLCD, TCH[j].DOCAMT.Value);
                                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();

                                        dbsql = Master_Help.InsVch_Hdr(TCH[j].AUTONO, TCH[j].DOCCD, TCH[j].DOCONLYNO, TCH[j].DOCDT.ToString(), emdno, TCH[j].DTAG, null, null, "Y", null, trcd);
                                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                    }

                                    if (TJBD != null)
                                    {
                                        for (int i = 0; i <= TJBD.Count - 1; i++)
                                        {
                                            if (TJBD[i].SLNO != 0 && TJBD[i].AMT != 0 && TJBD[i].AUTONO == TJB[j].AUTONO)
                                            {
                                                igst = igst + TJBD[i].IGSTAMT.Value;
                                                cgst = cgst + TJBD[i].CGSTAMT.Value;
                                                sgst = sgst + TJBD[i].SGSTAMT.Value;
                                                cess = cess + TJBD[i].CESSAMT.Value;
                                                dbamt = dbamt + TJBD[i].TAXABLEVAL.Value;
                                            }
                                        }
                                    }
                                    double itamt = dbamt;
                                    //Item Value
                                    isl = 2;
                                    //dbamt = TJB[j].TAXABLEVAL.Value;
                                    if (blactpost == true)
                                    {
                                        if (string.IsNullOrEmpty(expglcd))
                                        {
                                            dberrmsg = "Shortage/Bill Should not empty."; goto dbnotsave;
                                        }
                                        dbsql = Master_Help.InsVch_Det(TJB[j].AUTONO, TJB[j].DOCCD, TJB[j].DOCNO, TJB[j].DOCDT.ToString(), TJB[j].EMD_NO.Value, TJB[j].DTAG, Convert.ToSByte(isl), cr,
                                            expglcd, sslcd, dbamt, strrem, "",
                                            null, dbqty, 0, dbcurramt);
                                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                        if (cr == "D") dbDrAmt = dbDrAmt + dbamt;
                                        else dbCrAmt = dbCrAmt + dbamt;
                                        if (SALES_FUNC.IsClassMandatoryInGlcd(expglcd) == "Y")
                                        {
                                            dbsql = Master_Help.InsVch_Class(TJB[j].AUTONO, TJB[j].DOCCD, TJB[j].DOCNO, TJB[j].DOCDT.ToString(), TJB[j].EMD_NO.Value, TJB[j].DTAG, Convert.ToSByte(1), Convert.ToSByte(isl), sslcd,
                                                                                          tbl.Rows[0]["class1cd"].ToString(), tbl.Rows[0]["class2cd"].ToString(), dbamt, dbcurramt, strrem);
                                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                        }
                                    }
                                    bool IsClassMandatInGlcd = false;

                                    #region  GST Financial Part
                                    if (blactpost == true)
                                    {
                                        if (string.IsNullOrEmpty(tbl.Rows[0]["parglcd"].ToString()))
                                        {

                                            dberrmsg = "tcsglcd Should not empty."; goto dbnotsave;
                                        }
                                        if (SALES_FUNC.IsClassMandatoryInGlcd(expglcd) == "Y")
                                        {
                                            IsClassMandatInGlcd = true;
                                        }
                                        string[] gstpostcd = new string[5];
                                        double[] gstpostamt = new double[5];
                                        for (int gt = 0; gt < 5; gt++)
                                        {
                                            gstpostcd[gt] = ""; gstpostamt[gt] = 0;
                                        }

                                        int gi = 0;
                                        if (revcharge != "Y")
                                        {
                                            gstpostcd[gi] = tbl.Rows[0]["igstcd"].ToString(); gstpostamt[gi] = igst; gi++;
                                            gstpostcd[gi] = tbl.Rows[0]["cgstcd"].ToString(); gstpostamt[gi] = cgst; gi++;
                                            gstpostcd[gi] = tbl.Rows[0]["sgstcd"].ToString(); gstpostamt[gi] = sgst; gi++;
                                            gstpostcd[gi] = tbl.Rows[0]["cesscd"].ToString(); gstpostamt[gi] = cess; gi++;
                                            gstpostcd[gi] = tbl.Rows[0]["dutycd"].ToString(); gstpostamt[gi] = duty; gi++;
                                        }
                                        else
                                        {
                                            gstpostcd[gi] = tbl.Rows[0]["igst_rvi"].ToString(); gstpostamt[gi] = igst; gi++;
                                            gstpostcd[gi] = tbl.Rows[0]["cgst_rvi"].ToString(); gstpostamt[gi] = cgst; gi++;
                                            gstpostcd[gi] = tbl.Rows[0]["sgst_rvi"].ToString(); gstpostamt[gi] = sgst; gi++;
                                            gstpostcd[gi] = tbl.Rows[0]["cess_rvi"].ToString(); gstpostamt[gi] = cess; gi++;
                                            gstpostcd[gi] = tbl.Rows[0]["dutycd"].ToString(); gstpostamt[gi] = duty; gi++;
                                        }

                                        for (int gt = 0; gt < 5; gt++)
                                        {
                                            if (gstpostamt[gt] != 0)
                                            {

                                                isl = isl + 1;
                                                //dbsql = Master_Help.InsVch_Det(TJB[j].AUTONO, TJB[j].DOCCD, TJB[j].DOCNO, TJB[j].DOCDT.ToString(), TJB[j].EMD_NO.Value, TJB[j].DTAG, Convert.ToSByte(isl), cr, gstpostcd[gt], null,
                                                dbsql = Master_Help.InsVch_Det(TJB[j].AUTONO, TJB[j].DOCCD, TJB[j].DOCNO, TJB[j].DOCDT.ToString(), TJB[j].EMD_NO.Value, TJB[j].DTAG, Convert.ToSByte(isl), cr, gstpostcd[gt], sslcd,
                                                gstpostamt[gt], strrem, tbl.Rows[0]["parglcd"].ToString(), TJB[j].SLCD, dbqty, 0, 0, postdt);
                                                OraCmd.CommandText = dbsql;
                                                OraCmd.ExecuteNonQuery();
                                                if (cr == "D") dbDrAmt = dbDrAmt + gstpostamt[gt];
                                                else dbCrAmt = dbCrAmt + gstpostamt[gt];
                                                if (IsClassMandatInGlcd)
                                                {
                                                    dbsql = Master_Help.InsVch_Class(TJB[j].AUTONO, TJB[j].DOCCD, TJB[j].DOCNO, TJB[j].DOCDT.ToString(), TJB[j].EMD_NO.Value, TJB[j].DTAG, 1, Convert.ToSByte(isl), null,
                                                                                              tbl.Rows[0]["class1cd"].ToString(), tbl.Rows[0]["class2cd"].ToString(), gstpostamt[gt], 0, strrem);
                                                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                                }

                                            }
                                        }

                                        if (revcharge == "Y")
                                        {
                                            gi = 0;
                                            gstpostcd[gi] = tbl.Rows[0]["igst_rvo"].ToString(); gstpostamt[gi] = igst; gi++;
                                            gstpostcd[gi] = tbl.Rows[0]["cgst_rvo"].ToString(); gstpostamt[gi] = cgst; gi++;
                                            gstpostcd[gi] = tbl.Rows[0]["sgst_rvo"].ToString(); gstpostamt[gi] = sgst; gi++;
                                            gstpostcd[gi] = tbl.Rows[0]["cess_rvo"].ToString(); gstpostamt[gi] = cess; gi++;
                                            gstpostcd[gi] = tbl.Rows[0]["dutycd"].ToString(); gstpostamt[gi] = duty; gi++;
                                            for (int gt = 0; gt < 5; gt++)
                                            {
                                                if (gstpostamt[gt] != 0)
                                                {
                                                    isl = isl + 1;
                                                    //dbsql = Master_Help.InsVch_Det(TJB[j].AUTONO, TJB[j].DOCCD, TJB[j].DOCNO, TJB[j].DOCDT.ToString(), TJB[j].EMD_NO.Value, TJB[j].DTAG, Convert.ToSByte(isl), dr, gstpostcd[gt], null,
                                                    dbsql = Master_Help.InsVch_Det(TJB[j].AUTONO, TJB[j].DOCCD, TJB[j].DOCNO, TJB[j].DOCDT.ToString(), TJB[j].EMD_NO.Value, TJB[j].DTAG, Convert.ToSByte(isl), dr, gstpostcd[gt], sslcd,
                                                    gstpostamt[gt], strrem, tbl.Rows[0]["parglcd"].ToString(), TJB[j].SLCD, dbqty, 0, 0, postdt);
                                                    OraCmd.CommandText = dbsql;
                                                    OraCmd.ExecuteNonQuery();
                                                    if (dr == "D") dbDrAmt = dbDrAmt + gstpostamt[gt];
                                                    else dbCrAmt = dbCrAmt + gstpostamt[gt];
                                                    if (IsClassMandatInGlcd)
                                                    {
                                                        dbsql = Master_Help.InsVch_Class(TJB[j].AUTONO, TJB[j].DOCCD, TJB[j].DOCNO, TJB[j].DOCDT.ToString(), TJB[j].EMD_NO.Value, TJB[j].DTAG, 1, Convert.ToSByte(isl), null,
                                                            tbl.Rows[0]["class1cd"].ToString(), tbl.Rows[0]["class2cd"].ToString(), gstpostamt[gt], 0, strrem);
                                                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                    string strblno = ""; string strbldt = ""; string strduedt = ""; string strvtype = ""; string strrefno = "";
                                    string blconslcd = TJB[j].SLCD;
                                    if (blactpost == true)
                                    {
                                        if (string.IsNullOrEmpty(tbl.Rows[0]["parglcd"].ToString()))
                                        {
                                            dberrmsg = "Glcd m_post Should not empty."; goto dbnotsave;
                                        }
                                        // Ronded off
                                        dbamt = Convert.ToDouble(TJB[j].ROAMT);
                                        if (dbamt != 0)
                                        {

                                            string adrcr = cr;
                                            string rglcd = tbl.Rows[0]["rogl"].ToString();
                                            if (rglcd == "") rglcd = TJB[j].EXPGLCD;
                                            if (dbamt < 0) adrcr = dr;
                                            if (dbamt < 0) dbamt = dbamt * -1;

                                            isl = isl + 1;
                                            //dbsql = Master_Help.InsVch_Det(TJB[j].AUTONO, TJB[j].DOCCD, TJB[j].DOCNO, TJB[j].DOCDT.ToString(), TJB[j].EMD_NO.Value, TJB[j].DTAG, Convert.ToSByte(isl), adrcr, tbl.Rows[0]["rogl"].ToString(), null,
                                            dbsql = Master_Help.InsVch_Det(TJB[j].AUTONO, TJB[j].DOCCD, TJB[j].DOCNO, TJB[j].DOCDT.ToString(), TJB[j].EMD_NO.Value, TJB[j].DTAG, Convert.ToSByte(isl), adrcr, tbl.Rows[0]["rogl"].ToString(), sslcd,
                                            dbamt, strrem, tbl.Rows[0]["parglcd"].ToString(), TJB[j].SLCD, 0, 0, 0);
                                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                            if (adrcr == "D") dbDrAmt = dbDrAmt + dbamt;
                                            else dbCrAmt = dbCrAmt + dbamt;
                                        }
                                        //Party wise posting
                                        dbamt = TJB[j].BLAMT.Value;
                                        dbsql = Master_Help.InsVch_Det(TJB[j].AUTONO, TJB[j].DOCCD, TJB[j].DOCNO, TJB[j].DOCDT.ToString(), TJB[j].EMD_NO.Value, TJB[j].DTAG, Convert.ToSByte(1), dr,
                                            tbl.Rows[0]["parglcd"].ToString(), sslcd, dbamt, strrem, "",
                                            null, dbqty, 0, dbcurramt);
                                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                        if (dr == "D") dbDrAmt = dbDrAmt + Convert.ToDouble(TJB[j].BLAMT);
                                        else dbCrAmt = dbCrAmt + Convert.ToDouble(TJB[j].BLAMT);
                                        if (IsClassMandatInGlcd)
                                        {
                                            dbsql = Master_Help.InsVch_Class(TJB[j].AUTONO, TJB[j].DOCCD, TJB[j].DOCNO, TJB[j].DOCDT.ToString(), TJB[j].EMD_NO.Value, TJB[j].DTAG, 1, Convert.ToSByte(1), null,
                                                tbl.Rows[0]["class1cd"].ToString(), tbl.Rows[0]["class2cd"].ToString(), dbamt, dbcurramt, strrem);
                                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                        }
                                        strvtype = TJB[j].DOCTAG == "JD" ? "DN" : TJB[j].DOCTAG == "JC" ? "CN" : "BL";
                                        strduedt = TJB[j].DUEDT.ToString().retDateStr();

                                        if (VE.MENU_PARA == "JB" && TJB[j].DOCTAG != "JB")
                                        {
                                            strblno = TCH[j].DOCNO;
                                            strbldt = TJB[j].DOCDT.ToString();
                                        }
                                        else
                                        {
                                            strblno = TJB[j].PBLNO;
                                            strbldt = TJB[j].PBLDT.ToString();
                                        }

                                        if (TJB[j].SLCD != sslcd) blconslcd = TJB[j].SLCD;
                                        blconslcd = "";
                                        if (blconslcd == sslcd) blconslcd = "";
                                        string class1cd = IsClassMandatInGlcd == true ? tbl.Rows[0]["class1cd"].ToString() : "";
                                        dbsql = Master_Help.InsVch_Bl(TJB[j].AUTONO, TJB[j].DOCCD, TJB[j].DOCNO, TJB[j].DOCDT.ToString(), TJB[j].EMD_NO.Value, TJB[j].DTAG, dr,
                                             tbl.Rows[0]["parglcd"].ToString(), sslcd, blconslcd, "", class1cd, Convert.ToSByte(1),
                                             Convert.ToDouble(TJB[j].BLAMT), strblno, strbldt, strrefno, strduedt, strvtype, TJB[j].CRDAYS == null ? 0 : Convert.ToDouble(TJB[j].CRDAYS), itamt);

                                        //dbsql = Master_Help.InsVch_Bl(TJB[j].AUTONO, TJB[j].DOCCD, TJB[j].DOCNO, TJB[j].DOCDT.ToString(), TJB[j].EMD_NO.Value, TJB[j].DTAG, dr,
                                        //        tbl.Rows[0]["parglcd"].ToString(), sslcd, blconslcd, "", tbl.Rows[0]["class1cd"].ToString(), Convert.ToSByte(1),
                                        //        Convert.ToDouble(TJB[j].BLAMT), strblno, strbldt, strrefno, strduedt, strvtype, TJB[j].CRDAYS == null ? 0 : Convert.ToDouble(TJB[j].CRDAYS), itamt);
                                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                        if (VE.MENU_PARA == "JB" && TJB[j].DOCTAG != "JB")
                                        {
                                            dbsql = Master_Help.InsVch_Bl_Adj(TJB[j].AUTONO, TJB[j].EMD_NO.Value, TJB[j].DTAG, Convert.ToSByte(1),
                                                    TJB[0].AUTONO, Convert.ToSByte(Convert.ToSByte(1)), Convert.ToDouble(TJB[0].BLAMT), TJB[j].AUTONO, 1,
                                                    Convert.ToDouble(TJB[j].BLAMT), Convert.ToDouble(TJB[j].BLAMT));
                                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                        }
                                    }

                                    int gs = 0;
                                    //Posting of GST 
                                    bool blamtins = false; double gblamt = TJB[j].BLAMT.Value; double groamt = Convert.ToDouble(TJB[j].ROAMT.Value);
                                    if (TJBD != null && blactpost == true)
                                    {
                                        for (int i = 0; i <= TJBD.Count - 1; i++)
                                        {
                                            if (TJBD[i].SLNO != 0 && TJBD[i].ITCD != "" && TJBD[i].AMT != 0 && TJBD[i].AUTONO == TJB[j].AUTONO)
                                            {
                                                string hsncode = "", uomcd = "", itnm = "", gblno = "", gbldt = ""; double gqnty = 0;
                                                MSITM = DB.M_SITEM.Find(TJBD[i].ITCD);
                                                if (TJB[j].DOCTAG == "JS")
                                                {
                                                    //hsncode = MSITM.HSNSACCD; uomcd = MSITM.UOMCD; gqnty = TJBD[i].DNCNQNTY.Value;
                                                    gblno = TCH[j].DOCNO; gbldt = TCH[j].DOCDT.ToString().retDateStr();
                                                    itnm = DB.M_SITEM.Find(TJBD[i].ITCD).ITNM;
                                                }
                                                else
                                                {
                                                    hsncode = TJB[j].HSNCODE; uomcd = "OTH"; gqnty = 0;
                                                    gblno = strblno; gbldt = strbldt;
                                                    itnm = "Job Charges";
                                                }
                                                gs = gs + 1;

                                                T_VCH_GST TVCHGST = new T_VCH_GST();
                                                TVCHGST.EMD_NO = TJB[j].EMD_NO;
                                                TVCHGST.CLCD = TJB[j].CLCD;
                                                TVCHGST.DTAG = TJB[j].DTAG;
                                                TVCHGST.AUTONO = TJB[j].AUTONO;
                                                TVCHGST.DOCCD = TJB[j].DOCCD;
                                                TVCHGST.DOCNO = TJB[j].DOCNO;
                                                TVCHGST.DOCDT = TJB[j].DOCDT;
                                                TVCHGST.DSLNO = Convert.ToSByte(1);
                                                TVCHGST.SLNO = Convert.ToSByte(gs);
                                                TVCHGST.PCODE = TJB[j].SLCD;
                                                TVCHGST.BLNO = gblno;
                                                TVCHGST.BLDT = Convert.ToDateTime(gbldt);
                                                TVCHGST.HSNCODE = hsncode;
                                                TVCHGST.ITNM = itnm;
                                                TVCHGST.AMT = TJBD[i].AMT.retDbl() - TJBD[i].DISCAMT.retDbl() + TJBD[i].ADDLESSAMT.retDbl();
                                                TVCHGST.CGSTPER = TJBD[i].CGSTPER;
                                                TVCHGST.SGSTPER = TJBD[i].SGSTPER;
                                                TVCHGST.IGSTPER = TJBD[i].IGSTPER;
                                                TVCHGST.CGSTAMT = TJBD[i].CGSTAMT;
                                                TVCHGST.SGSTAMT = TJBD[i].SGSTAMT;
                                                TVCHGST.IGSTAMT = TJBD[i].IGSTAMT;
                                                TVCHGST.CESSPER = TJBD[i].CESSPER;
                                                TVCHGST.CESSAMT = TJBD[i].CESSAMT;
                                                TVCHGST.DRCR = cr;
                                                TVCHGST.QNTY = gqnty;
                                                TVCHGST.UOM = uomcd;
                                                TVCHGST.SALPUR = salpur;
                                                TVCHGST.INVTYPECD = "01";
                                                TVCHGST.GSTSLNM = "";
                                                TVCHGST.GSTNO = "";
                                                TVCHGST.POS = null;
                                                TVCHGST.OTHRAMT = 0;
                                                TVCHGST.ROAMT = groamt;
                                                TVCHGST.BLAMT = gblamt;
                                                TVCHGST.DNCNSALPUR = dncntag;
                                                TVCHGST.APPLTAXRATE = 0;
                                                TVCHGST.EXEMPTEDTYPE = exemptype;
                                                TVCHGST.GSTREGNTYPE = "";
                                                TVCHGST.GOOD_SERV = (TJB[j].DOCTAG == "JS" ? "G" : "S");
                                                TVCHGST.EXPGLCD = expglcd;
                                                TVCHGST.INPTCLAIM = "Y";
                                                TVCHGST.DELVSLCD = "";
                                                TVCHGST.ITEMEXTDESC = "";
                                                TVCHGST.BATCHNO = "";
                                                TVCHGST.GSTSLADD1 = "";
                                                TVCHGST.GSTSLADD2 = "";
                                                TVCHGST.GSTSLDIST = "";
                                                TVCHGST.GSTSLPIN = "";
                                                TVCHGST.BASAMT = TJBD[i].AMT;
                                                TVCHGST.DISCAMT = TJBD[i].DISCAMT;
                                                TVCHGST.RATE = TJBD[i].RATE;
                                                dbsql = Master_Help.RetModeltoSql(TVCHGST, "A", CommVar.FinSchema(UNQSNO));
                                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                                gblamt = 0; groamt = 0;
                                            }
                                        }
                                    }
                                    #region tds transaction
                                    bool tdsins = true;
                                    if (TJB[j].TDSAMT.Value == 0) tdsins = false;
                                    if (TJB[j].LOWTDS == "T") tdsins = true;
                                    if (TJB[j].DOCTAG != "JB") tdsins = false;
                                    if (tdsins == true)
                                    {
                                        sql = "select a.glcd from " + scmf + ".m_tds_cntrl a where tdscode='" + TJB[j].TDSHD + "' ";
                                        rstmp = Master_Help.SQLquery(sql);
                                        if (rstmp.Rows.Count == 0)
                                        {
                                            dberrmsg = "Tds code not found"; goto dbnotsave;
                                        }
                                        string tdsglcd = rstmp.Rows[0]["glcd"].ToString();

                                        isl++;
                                        strrem = "Th J/Bill " + TJB[j].PBLNO + " TDS @ " + TJB[j].TDSPER.ToString() + " on " + TJB[j].TDSON.ToString();
                                        dbsql = Master_Help.InsVch_TdsTxn(TJB[j].AUTONO, TJB[j].DOCCD, TJB[j].DOCNO, TJB[j].DOCDT.ToString(), TJB[j].EMD_NO.Value, TJB[j].DTAG,
                                                    TJB[j].PBLNO, TJB[j].PBLDT.ToString(), tbl.Rows[0]["parglcd"].ToString(), TJB[j].SLCD, TJB[j].TDSHD, Convert.ToSByte(isl), TJB[j].BLAMT.Value,
                                                    TJB[j].TDSON.Value, TJB[j].TDSPER.Value, TJB[j].TDSAMT.Value, TJB[j].DOCDT.ToString(), TJB[j].LOWTDS, "", "", TJB[j].ADVAMT.retDbl(), expglcd);
                                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                        if (TJB[j].TDSAMT != 0)
                                        {
                                            isl++;
                                            dbsql = Master_Help.InsVch_Det(TJB[j].AUTONO, TJB[j].DOCCD, TJB[j].DOCNO, TJB[j].DOCDT.ToString(), TJB[j].EMD_NO.Value, TJB[j].DTAG, Convert.ToSByte(isl), cr,
                                                tbl.Rows[0]["parglcd"].ToString(), sslcd, Convert.ToDouble(TJB[j].TDSAMT), strrem, tdsglcd,
                                                null, dbqty, 0, dbcurramt);
                                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                            if (cr == "D") dbDrAmt = dbDrAmt + Convert.ToDouble(TJB[j].TDSAMT);
                                            else dbCrAmt = dbCrAmt + Convert.ToDouble(TJB[j].TDSAMT);
                                            if (IsClassMandatInGlcd)
                                            {
                                                dbsql = Master_Help.InsVch_Class(TJB[j].AUTONO, TJB[j].DOCCD, TJB[j].DOCNO, TJB[j].DOCDT.ToString(), TJB[j].EMD_NO.Value, TJB[j].DTAG, 1, Convert.ToSByte(isl), null,
                                                    tbl.Rows[0]["class1cd"].ToString(), tbl.Rows[0]["class2cd"].ToString(), Convert.ToDouble(TJB[j].TDSAMT), dbcurramt, strrem);
                                                OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                            }
                                            isl++;
                                            dbsql = Master_Help.InsVch_Det(TJB[j].AUTONO, TJB[j].DOCCD, TJB[j].DOCNO, TJB[j].DOCDT.ToString(), TJB[j].EMD_NO.Value, TJB[j].DTAG, Convert.ToSByte(isl), dr,
                                                tdsglcd, sslcd, Convert.ToDouble(TJB[j].TDSAMT), strrem, tbl.Rows[0]["parglcd"].ToString(),
                                                null, dbqty, 0, dbcurramt);
                                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                            if (dr == "D") dbDrAmt = dbDrAmt + Convert.ToDouble(TJB[j].TDSAMT);
                                            else dbCrAmt = dbCrAmt + Convert.ToDouble(TJB[j].TDSAMT);
                                            if (IsClassMandatInGlcd)
                                            {
                                                dbsql = Master_Help.InsVch_Class(TJB[j].AUTONO, TJB[j].DOCCD, TJB[j].DOCNO, TJB[j].DOCDT.ToString(), TJB[j].EMD_NO.Value, TJB[j].DTAG, 1, Convert.ToSByte(isl), TJB[j].SLCD,
                                                    tbl.Rows[0]["class1cd"].ToString(), tbl.Rows[0]["class2cd"].ToString(), Convert.ToDouble(TJB[j].TDSAMT), dbcurramt, strrem);
                                                OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                            }
                                            strvtype = "";
                                            //strduedt = Convert.ToDateTime(TJB[j].DOCDT.Value).AddDays(Convert.ToDouble(TJB[j].DUEDAYS)).ToString().Substring(0, 10);
                                            strblno = TJB[j].PBLNO;
                                            strbldt = TJB[j].PBLDT.ToString();

                                            if (TJB[j].SLCD != sslcd) blconslcd = TJB[j].SLCD;
                                            blconslcd = "";
                                            if (blconslcd == sslcd) blconslcd = "";
                                            string class1cd = IsClassMandatInGlcd == true ? tbl.Rows[0]["class1cd"].ToString() : "";
                                            dbsql = Master_Help.InsVch_Bl(TJB[j].AUTONO, TJB[j].DOCCD, TJB[j].DOCNO, TJB[j].DOCDT.ToString(), TJB[j].EMD_NO.Value, TJB[j].DTAG, cr,
                                                  tbl.Rows[0]["parglcd"].ToString(), sslcd, blconslcd, "", class1cd, Convert.ToSByte(isl - 1),
                                                  Convert.ToDouble(TJB[j].TDSAMT), strblno, strbldt, strrefno, strduedt, "TD");

                                            //dbsql = Master_Help.InsVch_Bl(TJB[j].AUTONO, TJB[j].DOCCD, TJB[j].DOCNO, TJB[j].DOCDT.ToString(), TJB[j].EMD_NO.Value, TJB[j].DTAG, cr,
                                            //        tbl.Rows[0]["parglcd"].ToString(), sslcd, blconslcd, "", tbl.Rows[0]["class1cd"].ToString(), Convert.ToSByte(isl - 1),
                                            //        Convert.ToDouble(TJB[j].TDSAMT), strblno, strbldt, strrefno, strduedt, "TD");
                                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();

                                            dbsql = Master_Help.InsVch_Bl_Adj(TJB[j].AUTONO, TJB[j].EMD_NO.Value, TJB[j].DTAG, Convert.ToSByte(1),
                                                    TJB[j].AUTONO, Convert.ToSByte(Convert.ToSByte(1)), Convert.ToDouble(TJB[j].TDSAMT), TJB[j].AUTONO, Convert.ToSByte(isl - 1),
                                                    Convert.ToDouble(TJB[j].TDSAMT), Convert.ToDouble(TJB[j].TDSAMT));
                                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                        }
                                    }
                                    #endregion
                                    if (Math.Round(dbDrAmt, 2) != Math.Round(dbCrAmt, 2))
                                    {
                                        dberrmsg = "Debit " + Math.Round(dbDrAmt, 2) + " & Credit " + Math.Round(dbCrAmt, 2) + " not matching please click on rounded off...";
                                        goto dbnotsave;
                                    }
                                }
                                #endregion
                                DB.SaveChanges();
                                ModelState.Clear();
                                transaction.Commit();
                                OraTrans.Commit();
                                OraCon.Dispose();
                                return Content(ContentFlg);
                            }
                            else if (VE.DefaultAction == "V")
                            {
                                List<string> selauto = new List<string>();
                                selauto.Add(VE.T_JBILL.AUTONO);
                                if (VE.MENU_PARA == "JB")
                                {
                                    if (VE.T_JBILL.OTHAUTONO.retStr() != "") selauto.Add(VE.T_JBILL.OTHAUTONO);
                                    if (VE.T_JBILL.OTHAUTONO1.retStr() != "") selauto.Add(VE.T_JBILL.OTHAUTONO1);
                                }
                                else
                                {
                                    ImprovarDB DBTEMP = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                                    var checkItem = DBTEMP.T_JBILL.Where(a => a.OTHAUTONO == VE.T_JBILL.AUTONO).Select(a => a);
                                    if (checkItem.Any())
                                    {
                                        return Content("Entry Can't be delete! It's create from Bill Passing.");
                                    }
                                }
                                string sqlautono = "'" + string.Join("','", selauto) + "'";

                                foreach (var v in selauto)
                                {
                                    T_CNTRL_HDR TCH = Cn.T_CONTROL_HDR(VE.T_JBILL.DOCCD, VE.T_JBILL.DOCDT, VE.T_JBILL.DOCNO, v, "", "", VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString(), null, VE.T_JBILL.SLCD, Convert.ToDouble(VE.T_JBILL.DNCNAMT), null, VE.Audit_REM);
                                    DB.Entry(TCH).State = System.Data.Entity.EntityState.Modified;
                                    DB.SaveChanges();
                                }
                                DB.T_CNTRL_HDR.Where(x => selauto.Contains(x.AUTONO)).ToList().ForEach(x => { x.DTAG = "D"; });
                                DB.T_JBILL.Where(x => selauto.Contains(x.AUTONO)).ToList().ForEach(x => { x.DTAG = "D"; });
                                DB.T_JBILLDTL.Where(x => selauto.Contains(x.AUTONO)).ToList().ForEach(x => { x.DTAG = "D"; });
                                DB.T_TXN_LINKNO.Where(x => selauto.Contains(x.AUTONO)).ToList().ForEach(x => { x.DTAG = "D"; });
                                DB.T_CNTRL_HDR_DOC_DTL.Where(x => selauto.Contains(x.AUTONO)).ToList().ForEach(x => { x.DTAG = "D"; });
                                DB.T_CNTRL_HDR_DOC.Where(x => selauto.Contains(x.AUTONO)).ToList().ForEach(x => { x.DTAG = "D"; });
                                DB.T_CNTRL_HDR_REM.Where(x => selauto.Contains(x.AUTONO)).ToList().ForEach(x => { x.DTAG = "D"; });
                                DB.SaveChanges();
                                DB.T_JBILLDTL.RemoveRange(DB.T_JBILLDTL.Where(x => selauto.Contains(x.AUTONO)));
                                DB.SaveChanges();
                                DB.T_TXN_LINKNO.RemoveRange(DB.T_TXN_LINKNO.Where(x => selauto.Contains(x.AUTONO)));
                                DB.SaveChanges();
                                DB.T_JBILL.RemoveRange(DB.T_JBILL.Where(x => selauto.Contains(x.AUTONO)));
                                DB.SaveChanges();
                                DB.T_CNTRL_HDR_DOC_DTL.RemoveRange(DB.T_CNTRL_HDR_DOC_DTL.Where(x => selauto.Contains(x.AUTONO)));
                                DB.SaveChanges();
                                DB.T_CNTRL_HDR_DOC.RemoveRange(DB.T_CNTRL_HDR_DOC.Where(x => selauto.Contains(x.AUTONO)));
                                DB.SaveChanges();
                                DB.T_CNTRL_HDR_REM.RemoveRange(DB.T_CNTRL_HDR_REM.Where(x => selauto.Contains(x.AUTONO)));
                                DB.SaveChanges();
                                DB.T_CNTRL_HDR.RemoveRange(DB.T_CNTRL_HDR.Where(x => selauto.Contains(x.AUTONO)));
                                DB.SaveChanges();

                                // Delete from financial schema
                                dbsql = Master_Help.finTblUpdt("t_vch_gst", sqlautono, "D");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                                dbsql = Master_Help.finTblUpdt("t_tdstxn", sqlautono, "D");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                                dbsql = Master_Help.finTblUpdt("t_vch_bl_adj", sqlautono, "D");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                                dbsql = Master_Help.finTblUpdt("t_vch_bl", sqlautono, "D");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                                dbsql = Master_Help.finTblUpdt("t_vch_class", sqlautono, "D");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                                dbsql = Master_Help.finTblUpdt("t_tdstxn", sqlautono, "D");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                                dbsql = Master_Help.finTblUpdt("t_vch_det", sqlautono, "D");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                                dbsql = Master_Help.finTblUpdt("t_vch_hdr", sqlautono, "D");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                                dbsql = Master_Help.finTblUpdt("t_cntrl_hdr", sqlautono, "D", "F", VE.Audit_REM);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                                ModelState.Clear();
                                transaction.Commit();
                                OraTrans.Commit();
                                OraCon.Dispose();
                                return Content("3");
                            }
                            else
                            {
                                return Content(ContentFlg);
                            }
                            dbnotsave:;
                            transaction.Rollback();
                            OraTrans.Rollback();
                            OraCon.Dispose();
                            return Content(dberrmsg);
                        }
                        catch (DbEntityValidationException ex)
                        {
                            transaction.Rollback();
                            OraTrans.Rollback();
                            OraCon.Dispose();
                            var errorMessages = ex.EntityValidationErrors
                                    .SelectMany(x => x.ValidationErrors)
                                    .Select(x => x.ErrorMessage);
                            var fullErrorMessage = string.Join("&quot;", errorMessages);
                            var exceptionMessage = string.Concat(ex.Message, "<br/> &quot; The validation errors are: &quot;", fullErrorMessage);
                            throw new DbEntityValidationException(exceptionMessage, ex.EntityValidationErrors);
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        OraTrans.Rollback();
                        OraCon.Dispose();
                        Cn.SaveException(ex, "");
                        return Content(ex.Message + ex.InnerException);
                    }
                }
            }
        }
        public ActionResult Print(TJobBillEntry TSP, string DOCNO, string DOC_CD)
        {
            ReportViewinHtml ind = new ReportViewinHtml();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            //FabricEntry VE = new FabricEntry();
            //Cn.getQueryString(VE);
            string MENU_PARA = "";// VE.MENU_PARA;
            if (MENU_PARA == "JB")
            {
                string OTHAUTONO = (from z in DB.T_JBILL where z.AUTONO == TSP.T_JBILL.AUTONO select z.OTHAUTONO1).SingleOrDefault();
                if (OTHAUTONO != null && OTHAUTONO != "")
                {
                    var DOC_DATA = (from X in DB.T_CNTRL_HDR where X.AUTONO == OTHAUTONO select X).ToList();
                    if (DOC_DATA != null && DOC_DATA.Count > 0)
                    {
                        DOC_CD = DOC_DATA[0].DOCCD;
                        DOCNO = DOC_DATA[0].DOCONLYNO;
                    }
                }
            }
            ind.DOCCD = DOC_CD;
            ind.TEXTBOX1 = (from X in DB.M_DOCTYPE where X.DOCCD == DOC_CD select X.DOCNM).SingleOrDefault();
            ind.FDOCNO = DOCNO;
            ind.TDOCNO = DOCNO;
            ind.FDT = TSP.T_JBILL.DOCDT.ToString().Substring(0, 10);
            ind.TDT = TSP.T_JBILL.DOCDT.ToString().Substring(0, 10);
            ind.TEXTBOX2 = TSP.T_JBILL.SLCD;
            ind.TEXTBOX3 = DBF.M_SUBLEG.Find(TSP.T_JBILL.SLCD).SLNM;
            ind.MENU_PARA = TSP.MENU_PARA;
            if (TempData["printparameter"] != null)
            {
                TempData.Remove("printparameter");
            }
            TempData["printparameter"] = ind;
            return Content("");
        }
    }
}