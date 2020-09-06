using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;
using Microsoft.Ajax.Utilities;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;
using System.IO;

namespace Improvar.Controllers
{
    public class T_SALEController : Controller
    {
        Connection Cn = new Connection(); MasterHelp masterHelp = new MasterHelp();
        Salesfunc salesfunc = new Salesfunc(); DataTable DTNEW = new DataTable();
        EmailControl EmailControl = new EmailControl();
        T_TXN TXN; T_TXNTRANS TXNTRN; T_TXNOTH TXNOTH; T_CNTRL_HDR TCH; T_CNTRL_HDR_REM SLR;
        SMS SMS = new SMS();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: T_SALE
        public ActionResult T_SALE(string op = "", string key = "", int Nindex = 0, string searchValue = "", string parkID = "", string ThirdParty = "no")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    TransactionPackingSlipEntry VE = (parkID == "") ? new TransactionPackingSlipEntry() : (Improvar.ViewModels.TransactionPackingSlipEntry)Session[parkID];
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    switch (VE.MENU_PARA)
                    {
                        case "SBPCK":
                            ViewBag.formname = "Packing Slip"; break;
                        case "SB":
                            ViewBag.formname = "Sales Bill (Agst Packing Slip)"; break;
                        case "SBDIR":
                            ViewBag.formname = "Sales Bill"; break;
                        case "SR":
                            ViewBag.formname = "Sales Return (SRM)"; break;
                        case "SBCM":
                            ViewBag.formname = "Cash Memo"; break;
                        case "SBCMR":
                            ViewBag.formname = "Cash Memo Return Note"; break;
                        case "SBEXP":
                            ViewBag.formname = "Sales Bill (Export)"; break;
                        case "PI":
                            ViewBag.formname = "Proforma Invoice"; break;
                        case "PB":
                            ViewBag.formname = "Purchase Bill"; break;
                        case "PR":
                            ViewBag.formname = "Purchase Return (PRM)"; break;
                        default: ViewBag.formname = ""; break;
                    }
                    string LOC = CommVar.Loccd(UNQSNO);
                    string COM = CommVar.Compcd(UNQSNO);
                    string YR1 = CommVar.YearCode(UNQSNO);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    VE.DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);

                    VE.BL_TYPE = masterHelp.BL_TYPE();

                    VE.Database_Combo1 = (from n in DB.T_TXNOTH
                                          select new Database_Combo1() { FIELD_VALUE = n.SELBY }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();

                    VE.Database_Combo2 = (from n in DB.T_TXNOTH
                                          select new Database_Combo2() { FIELD_VALUE = n.DEALBY }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();
                    //VE.HSN_CODE = (from n in DBF.M_HSNCODE
                    //               select new HSN_CODE() { text = n.HSNDESCN, value = n.HSNCODE }).OrderBy(s => s.text).Distinct().ToList();
                    VE.BARGEN_TYPE = masterHelp.BARGEN_TYPE();

                    VE.DropDown_list_MTRLJOBCD = (from i in DB.M_MTRLJOBMST select new DropDown_list_MTRLJOBCD() { MTRLJOBCD = i.MTRLJOBCD, MTRLJOBNM = i.MTRLJOBNM }).OrderBy(s => s.MTRLJOBNM).ToList();
                    foreach (var v in VE.DropDown_list_MTRLJOBCD)
                    {
                        if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "PR")
                        {
                            if (v.MTRLJOBCD == "FS" || v.MTRLJOBCD == " PL" || v.MTRLJOBCD == "DY")
                            {
                                v.Checked = true;
                            }
                        }
                        else
                        {
                            if (v.MTRLJOBCD == "FS")
                            {
                                v.Checked = true;
                            }
                        }
                    }
                    VE.DropDown_list_StkType = (from n in DB.M_STKTYPE
                                                select new DropDown_list_StkType() { value = n.STKTYPE, text = n.STKNAME }).OrderBy(s => s.value).Distinct().ToList();
                    VE.DISC_TYPE = masterHelp.DISC_TYPE();
                    string[] autoEntryWork = ThirdParty.Split('~');// for zooming
                    if (autoEntryWork[0] == "yes")
                    {
                        autoEntryWork[2] = autoEntryWork[2].Replace("$$$$$$$$", "&");
                    }
                    if (autoEntryWork[0] == "yes")
                    {
                        if (autoEntryWork[4] == "Y")
                        {
                            DocumentType dp = new DocumentType();
                            dp.text = autoEntryWork[2];
                            dp.value = autoEntryWork[1];
                            VE.DocumentType.Clear();
                            VE.DocumentType.Add(dp);
                            VE.Edit = "E";
                            VE.Delete = "D";
                        }
                    }

                    if (op.Length != 0)
                    {
                        string[] XYZ = VE.DocumentType.Select(i => i.value).ToArray();

                        VE.IndexKey = (from p in DB.T_TXN
                                       join q in DB.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                       orderby p.DOCDT, p.DOCNO
                                       where XYZ.Contains(p.DOCCD) && q.LOCCD == LOC && q.COMPCD == COM && q.YR_CD == YR1
                                       select new IndexKey() { Navikey = p.AUTONO }).ToList();
                        if (searchValue != "") { Nindex = VE.IndexKey.FindIndex(r => r.Navikey.Equals(searchValue)); }
                        if (op == "E" || op == "D" || op == "V")
                        {
                            if (searchValue.Length != 0)
                            {
                                if (searchValue != "") { Nindex = VE.IndexKey.FindIndex(r => r.Navikey.Equals(searchValue)); }
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
                            VE.T_TXN = TXN;
                            VE.T_TXNTRANS = TXNTRN;
                            VE.T_TXNOTH = TXNOTH;
                            VE.T_CNTRL_HDR = TCH;
                            VE.T_CNTRL_HDR_REM = SLR;
                            if (VE.T_CNTRL_HDR.DOCNO != null) ViewBag.formname = ViewBag.formname + " (" + VE.T_CNTRL_HDR.DOCNO + ")";
                        }
                        if (op.ToString() == "A")
                        {
                            if (parkID == "")
                            {
                                T_TXN TTXN = new T_TXN();
                                TTXN.DOCDT = Cn.getCurrentDate(VE.mindate);
                                VE.T_TXN = TTXN;

                                T_TXNOTH TXNOTH = new T_TXNOTH(); VE.T_TXNOTH = TXNOTH;

                                VE.RoundOff = true;

                                List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                                UploadDOC UPL = new UploadDOC();
                                UPL.DocumentType = Cn.DOC_TYPE();
                                UploadDOC1.Add(UPL);
                                VE.UploadDOC = UploadDOC1;

                                List<PACKING_SLIP> PACKING_SLIP = new List<PACKING_SLIP>();
                                for (int i = 0; i < 10; i++)
                                {
                                    PACKING_SLIP PACKINGSLIP = new PACKING_SLIP();
                                    PACKINGSLIP.SLNO = Convert.ToInt16(i + 1);
                                    PACKING_SLIP.Add(PACKINGSLIP);
                                }
                            }
                            else
                            {
                                if (VE.UploadDOC != null) { foreach (var i in VE.UploadDOC) { i.DocumentType = Cn.DOC_TYPE(); } }
                                INI INIF = new INI();
                                INIF.DeleteKey(Session["UR_ID"].ToString(), parkID, Server.MapPath("~/Park.ini"));
                            }
                            VE = (TransactionPackingSlipEntry)Cn.CheckPark(VE, VE.MenuID, VE.MenuIndex, LOC, COM, CommVar.CurSchema(UNQSNO).ToString(), Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString());
                        }
                    }
                    else
                    {
                        VE.DefaultView = false;
                        VE.DefaultDay = 0;
                    }
                    string docdt = "";
                    if (TCH != null) if (TCH.DOCDT != null) docdt = TCH.DOCDT.ToString().Remove(10);
                    Cn.getdocmaxmindate(VE.T_TXN.DOCCD, docdt, VE.DefaultAction, VE.T_TXN.DOCNO, VE);
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                TransactionPackingSlipEntry VE = new TransactionPackingSlipEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message;
                return View(VE);
            }
        }
        public TransactionPackingSlipEntry Navigation(TransactionPackingSlipEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            string COM_CD = CommVar.Compcd(UNQSNO);
            string LOC_CD = CommVar.Loccd(UNQSNO);
            string DATABASE = CommVar.CurSchema(UNQSNO).ToString();
            string DATABASEF = CommVar.FinSchema(UNQSNO);
            Cn.getQueryString(VE);

            TXN = new T_TXN(); TXNTRN = new T_TXNTRANS(); TXNOTH = new T_TXNOTH(); TCH = new T_CNTRL_HDR(); SLR = new T_CNTRL_HDR_REM();
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
                TXN = DB.T_TXN.Find(aa[0].Trim());
                TCH = DB.T_CNTRL_HDR.Find(TXN.AUTONO);
                if (TXN.ROYN == "Y") VE.RoundOff = true;
                else VE.RoundOff = false;
                TXNTRN = DB.T_TXNTRANS.Find(TXN.AUTONO);
                TXNOTH = DB.T_TXNOTH.Find(TXN.AUTONO);
                if (TXN.SLCD.retStr() != "")
                {
                    string slcd = TXN.SLCD;
                    var subleg = (from a in DBF.M_SUBLEG where a.SLCD == slcd select new { a.SLNM, a.SLAREA, a.DISTRICT, a.GSTNO }).FirstOrDefault();
                    VE.SLNM = subleg.SLNM;
                    VE.SLAREA = subleg.SLAREA == "" ? subleg.DISTRICT : subleg.SLAREA;
                    VE.GSTNO = subleg.GSTNO;
                }

                VE.CONSLNM = TXN.CONSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXN.CONSLCD).Select(b => b.SLNM).FirstOrDefault();
                VE.AGSLNM = TXNOTH.AGSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXNOTH.AGSLCD).Select(b => b.SLNM).FirstOrDefault();
                VE.SAGSLNM = TXNOTH.SAGSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXNOTH.SAGSLCD).Select(b => b.SLNM).FirstOrDefault();
                VE.GONM = TXN.GOCD.retStr() == "" ? "" : DB.M_GODOWN.Where(a => a.GOCD == TXN.GOCD).Select(b => b.GONM).FirstOrDefault();
                VE.PRCNM = TXNOTH.PRCCD.retStr() == "" ? "" : DBF.M_PRCLST.Where(a => a.PRCCD == TXNOTH.PRCCD).Select(b => b.PRCNM).FirstOrDefault();

                VE.TRANSLNM = TXNTRN.TRANSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXNTRN.TRANSLCD).Select(b => b.SLNM).FirstOrDefault();
                VE.CRSLNM = TXNTRN.CRSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXNTRN.CRSLCD).Select(b => b.SLNM).FirstOrDefault();
                SLR = Cn.GetTransactionReamrks(CommVar.CurSchema(UNQSNO).ToString(), TXN.AUTONO);
                VE.UploadDOC = Cn.GetUploadImageTransaction(CommVar.CurSchema(UNQSNO).ToString(), TXN.AUTONO);

                string Scm = CommVar.CurSchema(UNQSNO);
                string str1 = "";
                str1 += "select i.SLNO,i.TXNSLNO,k.ITGRPCD,n.ITGRPNM,i.MTRLJOBCD,o.MTRLJOBNM,k.ITCD,k.ITNM,k.UOMCD,k.STYLENO,i.PARTCD,p.PARTNM,q.STKTYPE,r.STKNAME,i.BARNO, ";
                str1 += "j.COLRCD,m.COLRNM,j.SIZECD,l.SIZENM,i.SHADE,i.QNTY,i.NOS,i.RATE,i.DISCRATE,i.DISCTYPE,i.TDDISCRATE,i.TDDISCTYPE,i.SCMDISCTYPE,i.SCMDISCRATE ";
                str1 += "from " + Scm + ".T_BATCHDTL i, " + Scm + ".M_SITEM_BARCODE j, " + Scm + ".M_SITEM k, " + Scm + ".M_SIZE l, " + Scm + ".M_COLOR m, ";
                str1 += Scm + ".M_GROUP n," + Scm + ".M_MTRLJOBMST o," + Scm + ".M_PARTS p," + Scm + ".T_BATCHMST q," + Scm + ".M_STKTYPE r ";
                str1 += "where i.BARNO = j.BARNO(+) and j.ITCD = k.ITCD(+) and j.SIZECD = l.SIZECD(+) and j.COLRCD = m.COLRCD(+) and k.ITGRPCD=n.ITGRPCD(+) ";
                str1 += "and i.MTRLJOBCD=o.MTRLJOBCD(+) and i.PARTCD=p.PARTCD(+) and i.BARNO=q.BARNO(+) and q.STKTYPE=r.STKTYPE(+) ";
                str1 += "and i.AUTONO = '" + TXN.AUTONO + "' ";
                str1 += "order by i.SLNO ";
                DataTable tbl = masterHelp.SQLquery(str1);

                VE.TBATCHDTL = (from DataRow dr in tbl.Rows
                                select new TBATCHDTL()
                                {
                                    SLNO = dr["SLNO"].retShort(),
                                    TXNSLNO = dr["TXNSLNO"].retShort(),
                                    ITGRPCD = dr["ITGRPCD"].retStr(),
                                    ITGRPNM = dr["ITGRPNM"].retStr(),
                                    MTRLJOBCD = dr["MTRLJOBCD"].retStr(),
                                    MTRLJOBNM = dr["MTRLJOBNM"].retStr(),
                                    ITCD = dr["ITCD"].retStr(),
                                    ITNM = dr["ITNM"].retStr(),
                                    UOM = dr["UOMCD"].retStr(),
                                    STYLENO = dr["STYLENO"].retStr(),
                                    PARTCD = dr["PARTCD"].retStr(),
                                    PARTNM = dr["PARTNM"].retStr(),
                                    COLRCD = dr["COLRCD"].retStr(),
                                    COLRNM = dr["COLRNM"].retStr(),
                                    SIZECD = dr["SIZECD"].retStr(),
                                    SIZENM = dr["SIZENM"].retStr(),
                                    SHADE = dr["SHADE"].retStr(),
                                    QNTY = dr["QNTY"].retDbl(),
                                    NOS = dr["NOS"].retDbl(),
                                    RATE = dr["RATE"].retDbl(),
                                    DISCRATE = dr["DISCRATE"].retDbl(),
                                    DISCTYPE = dr["DISCTYPE"].retStr(),
                                    DISCTYPE_DESC = dr["DISCTYPE"].retStr() == "P" ? "%" : dr["DISCTYPE"].retStr() == "N" ? "Nos" : dr["DISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
                                    TDDISCRATE = dr["TDDISCRATE"].retDbl(),
                                    TDDISCTYPE = dr["TDDISCTYPE"].retStr(),
                                    SCMDISCTYPE = dr["SCMDISCTYPE"].retStr(),
                                    SCMDISCRATE = dr["SCMDISCRATE"].retDbl(),
                                    STKTYPE = dr["STKTYPE"].retStr(),
                                    STKNAME = dr["STKNAME"].retStr(),
                                    BARNO = dr["BARNO"].retStr(),
                                }).OrderBy(s => s.SLNO).ToList();

                str1 = "";
                str1 += "select i.SLNO,j.ITGRPCD,k.ITGRPNM,i.MTRLJOBCD,l.MTRLJOBNM,i.ITCD,j.ITNM,j.UOMCD,i.STKTYPE,m.STKNAME,i.NOS,i.QNTY,i.FLAGMTR, ";
                str1 += "i.BLQNTY,i.RATE,i.AMT,i.DISCTYPE,i.DISCRATE,i.DISCAMT,i.TDDISCTYPE,i.TDDISCRATE,i.TDDISCAMT,i.SCMDISCTYPE,i.SCMDISCRATE,i.SCMDISCAMT, ";
                str1 += "i.TXBLVAL,i.IGSTPER,i.CGSTPER,i.SGSTPER,i.CESSPER,i.IGSTAMT,i.CGSTAMT,i.SGSTAMT,i.CESSAMT,i.NETAMT,i.HSNCODE,i.BALENO,i.GLCD ";
                str1 += "from " + Scm + ".T_TXNDTL i, " + Scm + ".M_SITEM j, " + Scm + ".M_GROUP k, " + Scm + ".M_MTRLJOBMST l, " + Scm + ".M_STKTYPE m ";
                str1 += "where i.ITCD = j.ITCD(+) and j.ITGRPCD=k.ITGRPCD(+) and i.MTRLJOBCD=l.MTRLJOBCD(+)  and i.STKTYPE=m.STKTYPE(+)  ";
                str1 += "and i.AUTONO = '" + TXN.AUTONO + "' ";
                str1 += "order by i.SLNO ";
                tbl = masterHelp.SQLquery(str1);

                VE.TTXNDTL = (from DataRow dr in tbl.Rows
                              select new TTXNDTL()
                              {
                                  SLNO = dr["SLNO"].retShort(),
                                  ITGRPCD = dr["ITGRPCD"].retStr(),
                                  ITGRPNM = dr["ITGRPNM"].retStr(),
                                  MTRLJOBCD = dr["MTRLJOBCD"].retStr(),
                                  MTRLJOBNM = dr["MTRLJOBNM"].retStr(),
                                  ITCD = dr["ITCD"].retStr(),
                                  ITNM = dr["ITNM"].retStr(),
                                  UOM = dr["UOMCD"].retStr(),
                                  STKTYPE = dr["STKTYPE"].retStr(),
                                  STKNAME = dr["STKNAME"].retStr(),
                                  NOS = dr["NOS"].retDbl(),
                                  QNTY = dr["QNTY"].retDbl(),
                                  FLAGMTR = dr["FLAGMTR"].retDbl(),
                                  BLQNTY = dr["BLQNTY"].retDbl(),
                                  RATE = dr["RATE"].retDbl(),
                                  AMT = dr["AMT"].retDbl(),
                                  DISCTYPE = dr["DISCTYPE"].retStr(),
                                  DISCTYPE_DESC = dr["DISCTYPE"].retStr() == "P" ? "%" : dr["DISCTYPE"].retStr() == "N" ? "Nos" : dr["DISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
                                  DISCRATE = dr["DISCRATE"].retDbl(),
                                  DISCAMT = dr["DISCAMT"].retDbl(),
                                  TDDISCTYPE = dr["TDDISCTYPE"].retStr(),
                                  TDDISCRATE = dr["TDDISCRATE"].retDbl(),
                                  TDDISCAMT = dr["TDDISCAMT"].retDbl(),
                                  SCMDISCTYPE = dr["SCMDISCTYPE"].retStr(),
                                  SCMDISCRATE = dr["SCMDISCRATE"].retDbl(),
                                  SCMDISCAMT = dr["SCMDISCAMT"].retDbl(),
                                  TXBLVAL = dr["TXBLVAL"].retDbl(),
                                  IGSTPER = dr["IGSTPER"].retDbl(),
                                  CGSTPER = dr["CGSTPER"].retDbl(),
                                  SGSTPER = dr["SGSTPER"].retDbl(),
                                  CESSPER = dr["CESSPER"].retDbl(),
                                  IGSTAMT = dr["IGSTAMT"].retDbl(),
                                  CGSTAMT = dr["CGSTAMT"].retDbl(),
                                  SGSTAMT = dr["SGSTAMT"].retDbl(),
                                  CESSAMT = dr["CESSAMT"].retDbl(),
                                  NETAMT = dr["NETAMT"].retDbl(),
                                  HSNCODE = dr["HSNCODE"].retStr(),
                                  BALENO = dr["BALENO"].retStr(),
                                  GLCD = dr["GLCD"].retStr()
                              }).OrderBy(s => s.SLNO).ToList();

                VE.B_T_QNTY = VE.TBATCHDTL.Sum(a => a.QNTY).retDbl();
                VE.T_NOS = VE.TTXNDTL.Sum(a => a.NOS).retDbl();
                VE.T_QNTY = VE.TTXNDTL.Sum(a => a.QNTY).retDbl();
                VE.T_AMT = VE.TTXNDTL.Sum(a => a.AMT).retDbl();
                VE.T_GROSS_AMT = VE.TTXNDTL.Sum(a => a.TXBLVAL).retDbl();
                VE.T_IGST_AMT = VE.TTXNDTL.Sum(a => a.IGSTAMT).retDbl();
                VE.T_CGST_AMT = VE.TTXNDTL.Sum(a => a.CGSTAMT).retDbl();
                VE.T_SGST_AMT = VE.TTXNDTL.Sum(a => a.SGSTAMT).retDbl();
                VE.T_CESS_AMT = VE.TTXNDTL.Sum(a => a.CESSAMT).retDbl();
                VE.T_NET_AMT = VE.TTXNDTL.Sum(a => a.NETAMT).retDbl();
                foreach (var v in VE.TBATCHDTL)
                {
                    v.GSTPER = VE.TTXNDTL.Where(a => a.SLNO == v.TXNSLNO).Sum(b => b.IGSTPER + b.CGSTPER + b.SGSTPER).retDbl();
                }

            }
            //Cn.DateLock_Entry(VE, DB, TCH.DOCDT.Value);
            if (TCH.CANCEL == "Y") VE.CancelRecord = true; else VE.CancelRecord = false;
            return VE;
        }
        public ActionResult SearchPannelData(TransactionPackingSlipEntry VE, string SRC_SLCD, string SRC_DOCNO, string SRC_FDT, string SRC_TDT)
        {
            string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), yrcd = CommVar.YearCode(UNQSNO);
            Cn.getQueryString(VE);
            List<DocumentType> DocumentType = new List<DocumentType>();
            DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
            string doccd = DocumentType.Select(i => i.value).ToArray().retSqlfromStrarray();
            string sql = "";

            sql = "select a.autono, b.docno, to_char(b.docdt,'dd/mm/yyyy') docdt, b.doccd, a.slcd, c.slnm, c.district, nvl(a.blamt,0) blamt ";
            sql += "from " + scm + ".t_txn a, " + scm + ".t_cntrl_hdr b, " + scmf + ".m_subleg c ";
            sql += "where a.autono=b.autono and a.slcd=c.slcd(+) and b.doccd in (" + doccd + ") and ";
            if (SRC_FDT.retStr() != "") sql += "b.docdt >= to_date('" + SRC_FDT.retDateStr() + "','dd/mm/yyyy') and ";
            if (SRC_TDT.retStr() != "") sql += "b.docdt <= to_date('" + SRC_TDT.retDateStr() + "','dd/mm/yyyy') and ";
            if (SRC_DOCNO.retStr() != "") sql += "(b.vchrno like '%" + SRC_DOCNO.retStr() + "%' or b.docno like '%" + SRC_DOCNO.retStr() + "%') and ";
            if (SRC_SLCD.retStr() != "") sql += "(a.slcd like '%" + SRC_SLCD.retStr() + "%' or upper(c.slnm) like '%" + SRC_SLCD.retStr().ToUpper() + "%') and ";
            sql += "b.loccd='" + LOC + "' and b.compcd='" + COM + "' and b.yr_cd='" + yrcd + "' ";
            sql += "order by docdt, docno ";
            DataTable tbl = masterHelp.SQLquery(sql);

            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Document Number" + Cn.GCS() + "Document Date" + Cn.GCS() + "Party Name" + Cn.GCS() + "Bill Amt" + Cn.GCS() + "AUTO NO";
            for (int j = 0; j <= tbl.Rows.Count - 1; j++)
            {
                SB.Append("<tr><td><b>" + tbl.Rows[j]["docno"] + "</b> [" + tbl.Rows[j]["doccd"] + "]" + " </td><td>" + tbl.Rows[j]["docdt"] + " </td><td><b>" + tbl.Rows[j]["slnm"] + "</b> [" + tbl.Rows[j]["district"] + "] (" + tbl.Rows[j]["slcd"] + ") </td><td class='text-right'>" + Convert.ToDouble(tbl.Rows[j]["blamt"]).ToINRFormat() + " </td><td>" + tbl.Rows[j]["autono"] + " </td></tr>");
            }
            return PartialView("_SearchPannel2", masterHelp.Generate_SearchPannel(hdr, SB.ToString(), "4", "4"));
        }
        public ActionResult GetSubLedgerDetails(string val, string Code)
        {
            try
            {

                var code_data = Code.Split(Convert.ToChar(Cn.GCS()));
                if (code_data.Count() > 1)
                {
                    if (code_data[0].retStr() == "Party")
                    {
                        if (code_data[1] == "")
                        {
                            return Content("Please Select Document Date !!");
                        }
                        else
                        {
                            Code = "";
                        }
                    }
                    else
                    {
                        if (code_data[1] == "")
                        {
                            return Content("Please Select Agent !!");
                        }
                        else
                        {
                            Code = code_data[0];
                        }
                    }


                }

                var str = masterHelp.SLCD_help(val, Code);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    if (code_data[0].retStr() == "Party")
                    {
                        if (str.IndexOf(Cn.GCS()) > 0)
                        {
                            var party_data = salesfunc.GetSlcdDetails(val, code_data[1]);
                            if (party_data != null && party_data.Rows.Count > 0)
                            {
                                str = masterHelp.ToReturnFieldValues("", party_data);
                                return Content(str);
                            }
                            else
                            {
                                return Content("Invalid Ledger Code ! Please Enter a Valid Ledger Code !!");
                            }
                        }
                        else
                        {
                            return Content(str);
                        }
                    }
                    else
                    {
                        return Content(str);
                    }

                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetGodownDetails(string val)
        {
            try
            {
                var str = masterHelp.GOCD_help(val);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetPriceDetails(string val)
        {
            try
            {
                var str = masterHelp.PRCCD_help(val);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetItemGroupDetails(string val)
        {
            try
            {
                string str = masterHelp.ITGRPCD_help(val, "");
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetMaterialJobDetails(string val)
        {
            try
            {
                string str = masterHelp.MTRLJOBCD_help(val);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetItemDetails(string val, string Code)
        {
            try
            {
                var str = masterHelp.ITCD_help(val, "", Code);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetPartDetails(string val)
        {
            try
            {
                var str = masterHelp.PARTCD_help(val);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetColorDetails(string val)
        {
            try
            {
                var str = masterHelp.COLRCD_help(val);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetSizeDetails(string val)
        {
            try
            {
                var str = masterHelp.SIZECD_help(val);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetStockDetails(string val)
        {
            try
            {
                var str = masterHelp.STKTYPE_help(val);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetLocationBinDetails(string val)
        {
            try
            {
                var str = masterHelp.LOCABIN_help(val);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetBarCodeDetails(string val, string DOCDT, string TAXGRPCD, string GOCD, string PRCCD, string MTRLJOBCD)
        {
            try
            {
                TransactionPackingSlipEntry VE = new TransactionPackingSlipEntry();
                var str = masterHelp.BARCODE_help(val);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    //return Content(str);
                    if (str.IndexOf(Cn.GCS()) > 0)
                    {
                        //string itgrpcd = "", itcd = "", styleno = "";
                        //itgrpcd = str.retCompValue("ITGRPCD");
                        //itcd = str.retCompValue("ITCD");
                        //styleno = str.retCompValue("STYLENO");
                        //var stock_data = salesfunc.GetStock(DOCDT.retStr(), GOCD.retStr().retSqlformat(), val.retStr().retSqlformat(), itcd.retStr().retSqlformat(), MTRLJOBCD.retStr().retSqlformat(), "", itgrpcd.retStr().retSqlformat(), styleno.retStr(), PRCCD.retStr(), TAXGRPCD.retStr());

                        var stock_data = salesfunc.GetStock(DOCDT.retStr(), GOCD.retStr().retSqlformat(), val.retStr().retSqlformat(), "", MTRLJOBCD.retStr().retSqlformat(), "", "", "", PRCCD.retStr(), TAXGRPCD.retStr());
                        if (stock_data == null || stock_data.Rows.Count == 0)//stock zero then return bardet from item master as blur
                        {
                            return Content(str);
                        }
                        else if (stock_data != null && stock_data.Rows.Count > 1)//if stock return more then one row then open popup
                        {
                            VE.TSALEBARNOPOPUP = (from DataRow dr in stock_data.Rows
                                                  select new TSALEBARNOPOPUP
                                                  {
                                                      //SLNO = dr["slno"].retShort(),
                                                      BARNO = dr["BARNO"].retStr(),
                                                      ITGRPCD = dr["ITGRPCD"].retStr(),
                                                      ITGRPNM = dr["ITGRPNM"].retStr(),
                                                      //MTRLJOBNM = dr["MTRLJOBNM"].retStr(),
                                                      MTRLJOBCD = dr["MTRLJOBCD"].retStr(),
                                                      ITNM = dr["ITNM"].retStr(),
                                                      ITCD = dr["ITCD"].retStr(),
                                                      STYLENO = dr["STYLENO"].retStr(),
                                                      //PARTNM = dr["PARTNM"].retStr(),
                                                      PARTCD = dr["PARTCD"].retStr(),
                                                      COLRCD = dr["COLRCD"].retStr(),
                                                      COLRNM = dr["COLRNM"].retStr(),
                                                      //SIZENM = dr["SIZENM"].retStr(),
                                                      SIZECD = dr["SIZECD"].retStr(),
                                                      SLNM = dr["SLNM"].retStr(),
                                                      SLCD = dr["SLCD"].retStr(),
                                                      UOM = dr["uomcd"].retStr(),
                                                      STKTYPE = dr["STKTYPE"].retStr(),
                                                      DOCDT = dr["DOCDT"].retStr().Remove(10),
                                                      BALQNTY = dr["BALQNTY"].retDbl(),
                                                      BALNOS = dr["BALNOS"].retDbl(),

                                                      PDESIGN = dr["PDESIGN"].retStr(),
                                                      FLAGMTR = dr["FLAGMTR"].retDbl(),
                                                      RATE = dr["RATE"].retDbl(),
                                                      HSNCODE = dr["HSNCODE"].retStr(),
                                                      PRODGRPGSTPER = dr["PRODGRPGSTPER"].retStr(),
                                                      //ALL_GSTPER = dr["ALL_GSTPER"].retStr(),
                                                      SHADE = dr["SHADE"].retStr(),
                                                      LOCABIN = dr["LOCABIN"].retStr(),
                                                  }).ToList();
                            for (int p = 0; p <= VE.TSALEBARNOPOPUP.Count - 1; p++)
                            {
                                VE.TSALEBARNOPOPUP[p].SLNO = Convert.ToInt16(p + 1);
                                VE.TSALEBARNOPOPUP[p].ALL_GSTPER = salesfunc.retGstPer(VE.TSALEBARNOPOPUP[p].PRODGRPGSTPER, VE.TSALEBARNOPOPUP[p].RATE.retDbl());
                                if (VE.TSALEBARNOPOPUP[p].ALL_GSTPER.retStr() != "")
                                {
                                    var gst = VE.TSALEBARNOPOPUP[p].ALL_GSTPER.Split(',').ToList();
                                    VE.TSALEBARNOPOPUP[p].GSTPER = (from a in gst select a.retDbl()).Sum();
                                }

                            }
                            VE.DefaultView = true;
                            return PartialView("_T_SALE_BARNODETAIL", VE);
                        }
                        else//stock return one row then return as blur
                        {
                            str = masterHelp.ToReturnFieldValues("", stock_data);
                            string all_gstper = salesfunc.retGstPer(stock_data.Rows[0]["PRODGRPGSTPER"].retStr(), stock_data.Rows[0]["RATE"].retDbl());
                            double gst = 0;
                            if (all_gstper.retStr() != "")
                            {
                                var gst_data = all_gstper.Split(',').ToList();
                                gst = (from a in gst_data select a.retDbl()).Sum();
                            }
                            str += "^ALL_GSTPER=^" + all_gstper + Cn.GCS();
                            str += "^GSTPER=^" + gst + Cn.GCS();
                            return Content(str);
                        }

                    }
                    else
                    {
                        return Content(str);
                    }
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult FillBarCodeData(TransactionPackingSlipEntry VE)
        {
            try
            {
                if (VE.TBATCHDTL == null)
                {
                    List<TBATCHDTL> TBATCHDTL = new List<TBATCHDTL>();
                    TBATCHDTL TBATCHDTL1 = new TBATCHDTL();
                    TBATCHDTL1.SLNO = 1;
                    TBATCHDTL1.TXNSLNO = VE.TXNSLNO.retShort();
                    TBATCHDTL1.ITGRPCD = VE.ITGRPCD;
                    TBATCHDTL1.ITGRPNM = VE.ITGRPNM;
                    TBATCHDTL1.MTRLJOBCD = VE.MTRLJOBCD;
                    TBATCHDTL1.MTRLJOBNM = VE.MTRLJOBNM;
                    TBATCHDTL1.ITCD = VE.ITCD;
                    TBATCHDTL1.ITNM = VE.ITNM;
                    TBATCHDTL1.STYLENO = VE.STYLENO;
                    TBATCHDTL1.STKTYPE = VE.STKTYPE;
                    TBATCHDTL1.PARTCD = VE.PARTCD;
                    TBATCHDTL1.PARTNM = VE.PARTNM;
                    TBATCHDTL1.COLRCD = VE.COLRCD;
                    TBATCHDTL1.COLRNM = VE.COLRNM;
                    TBATCHDTL1.SIZECD = VE.SIZECD;
                    TBATCHDTL1.SIZENM = VE.SIZENM;
                    TBATCHDTL1.SHADE = VE.SHADE;
                    TBATCHDTL1.QNTY = VE.QNTY;
                    TBATCHDTL1.UOM = VE.UOM;
                    TBATCHDTL1.NOS = VE.NOS;
                    TBATCHDTL1.RATE = VE.RATE;
                    TBATCHDTL1.GSTPER = VE.GSTPER;
                    TBATCHDTL1.DISCRATE = VE.DISCRATE;
                    TBATCHDTL1.DISCTYPE = VE.DISCTYPE;
                    TBATCHDTL1.DISCTYPE_DESC = VE.DISCTYPE == "P" ? "%" : VE.DISCTYPE == "N" ? "Nos" : VE.DISCTYPE == "Q" ? "Qnty" : "Fixed";
                    TBATCHDTL1.TDDISCRATE = VE.TDDISCRATE;
                    TBATCHDTL1.TDDISCTYPE = VE.TDDISCTYPE;
                    TBATCHDTL1.TDDISCTYPE_DESC = VE.TDDISCTYPE == "P" ? "%" : VE.TDDISCTYPE == "N" ? "Nos" : VE.TDDISCTYPE == "Q" ? "Qnty" : "Fixed";
                    TBATCHDTL1.SCMDISCRATE = VE.SCMDISCRATE;
                    TBATCHDTL1.SCMDISCTYPE = VE.SCMDISCTYPE;
                    TBATCHDTL1.SCMDISCTYPE_DESC = VE.SCMDISCTYPE == "P" ? "%" : VE.SCMDISCTYPE == "N" ? "Nos" : VE.SCMDISCTYPE == "Q" ? "Qnty" : "Fixed";
                    TBATCHDTL1.BARNO = VE.BARCODE;
                    TBATCHDTL1.PDESIGN = VE.PDESIGN;
                    TBATCHDTL1.FLAGMTR = VE.FLAGMTR;
                    TBATCHDTL1.HSNCODE = VE.HSNCODE;
                    TBATCHDTL1.BALENO = VE.BALENO;
                    TBATCHDTL1.LOCABIN = VE.LOCABIN;
                    TBATCHDTL1.ALL_GSTPER = VE.ALL_GSTPER;
                    TBATCHDTL.Add(TBATCHDTL1);
                    VE.TBATCHDTL = TBATCHDTL;
                }
                else
                {
                    List<TBATCHDTL> TBATCHDTL = new List<TBATCHDTL>();
                    for (int i = 0; i <= VE.TBATCHDTL.Count - 1; i++)
                    {
                        TBATCHDTL MIB = new TBATCHDTL();
                        MIB = VE.TBATCHDTL[i];
                        TBATCHDTL.Add(MIB);
                    }

                    TBATCHDTL TBATCHDTL1 = new TBATCHDTL();
                    var max = VE.TBATCHDTL.Max(a => Convert.ToInt32(a.SLNO));
                    int SLNO = Convert.ToInt32(max) + 1;
                    TBATCHDTL1.SLNO = Convert.ToSByte(SLNO);
                    TBATCHDTL1.TXNSLNO = VE.TXNSLNO.retShort();
                    TBATCHDTL1.ITGRPCD = VE.ITGRPCD;
                    TBATCHDTL1.ITGRPNM = VE.ITGRPNM;
                    TBATCHDTL1.MTRLJOBCD = VE.MTRLJOBCD;
                    TBATCHDTL1.MTRLJOBNM = VE.MTRLJOBNM;
                    TBATCHDTL1.ITCD = VE.ITCD;
                    TBATCHDTL1.ITNM = VE.ITNM;
                    TBATCHDTL1.STYLENO = VE.STYLENO;
                    TBATCHDTL1.STKTYPE = VE.STKTYPE;
                    TBATCHDTL1.PARTCD = VE.PARTCD;
                    TBATCHDTL1.PARTNM = VE.PARTNM;
                    TBATCHDTL1.COLRCD = VE.COLRCD;
                    TBATCHDTL1.COLRNM = VE.COLRNM;
                    TBATCHDTL1.SIZECD = VE.SIZECD;
                    TBATCHDTL1.SIZENM = VE.SIZENM;
                    TBATCHDTL1.SHADE = VE.SHADE;
                    TBATCHDTL1.QNTY = VE.QNTY;
                    TBATCHDTL1.UOM = VE.UOM;
                    TBATCHDTL1.NOS = VE.NOS;
                    TBATCHDTL1.RATE = VE.RATE;
                    TBATCHDTL1.GSTPER = VE.GSTPER;
                    TBATCHDTL1.DISCRATE = VE.DISCRATE;
                    TBATCHDTL1.DISCTYPE = VE.DISCTYPE;
                    TBATCHDTL1.DISCTYPE_DESC = VE.DISCTYPE == "P" ? "%" : VE.DISCTYPE == "N" ? "Nos" : VE.DISCTYPE == "Q" ? "Qnty" : "Fixed";
                    TBATCHDTL1.TDDISCRATE = VE.TDDISCRATE;
                    TBATCHDTL1.TDDISCTYPE = VE.TDDISCTYPE;
                    TBATCHDTL1.TDDISCTYPE_DESC = VE.TDDISCTYPE == "P" ? "%" : VE.TDDISCTYPE == "N" ? "Nos" : VE.TDDISCTYPE == "Q" ? "Qnty" : "Fixed";
                    TBATCHDTL1.SCMDISCRATE = VE.SCMDISCRATE;
                    TBATCHDTL1.SCMDISCTYPE = VE.SCMDISCTYPE;
                    TBATCHDTL1.SCMDISCTYPE_DESC = VE.SCMDISCTYPE == "P" ? "%" : VE.SCMDISCTYPE == "N" ? "Nos" : VE.SCMDISCTYPE == "Q" ? "Qnty" : "Fixed";
                    TBATCHDTL1.BARNO = VE.BARCODE;
                    TBATCHDTL1.PDESIGN = VE.PDESIGN;
                    TBATCHDTL1.FLAGMTR = VE.FLAGMTR;
                    TBATCHDTL1.HSNCODE = VE.HSNCODE;
                    TBATCHDTL1.BALENO = VE.BALENO;
                    TBATCHDTL1.LOCABIN = VE.LOCABIN;
                    TBATCHDTL1.ALL_GSTPER = VE.ALL_GSTPER;
                    TBATCHDTL.Add(TBATCHDTL1);
                    VE.TBATCHDTL = TBATCHDTL;

                }
                //VE.TBATCHDTL = (from x in VE.TBATCHDTL
                //                group x by new
                //                {
                //                    x.TXNSLNO,
                //                    x.ITGRPCD,
                //                    x.ITGRPNM,
                //                    x.MTRLJOBCD,
                //                    x.MTRLJOBNM,
                //                    x.ITCD,
                //                    x.ITNM,
                //                    x.STYLENO,
                //                    x.STKTYPE,
                //                    x.STKNAME,
                //                    x.PARTCD,
                //                    x.PARTNM,
                //                    x.COLRCD,
                //                    x.COLRNM,
                //                    x.SIZECD,
                //                    x.SIZENM,
                //                    x.SHADE,
                //                    x.UOM,
                //                    x.RATE,
                //                    x.GSTPER,
                //                    x.DISCRATE,
                //                    x.DISCTYPE,
                //                    x.DISCTYPE_DESC,
                //                    x.TDDISCRATE,
                //                    x.TDDISCTYPE,
                //                    x.SCMDISCRATE,
                //                    x.SCMDISCTYPE,
                //                    x.BARNO,
                //                } into P
                //                select new TBATCHDTL
                //                {
                //                    TXNSLNO = P.Key.TXNSLNO,
                //                    ITGRPCD = P.Key.ITGRPCD,
                //                    ITGRPNM = P.Key.ITGRPNM,
                //                    MTRLJOBCD = P.Key.MTRLJOBCD,
                //                    MTRLJOBNM = P.Key.MTRLJOBNM,
                //                    ITCD = P.Key.ITCD,
                //                    ITNM = P.Key.ITNM,
                //                    STYLENO = P.Key.STYLENO,
                //                    STKTYPE = P.Key.STKTYPE,
                //                    STKNAME = P.Key.STKNAME,
                //                    PARTCD = P.Key.PARTCD,
                //                    PARTNM = P.Key.PARTNM,
                //                    COLRCD = P.Key.COLRCD,
                //                    COLRNM = P.Key.COLRNM,
                //                    SIZECD = P.Key.SIZECD,
                //                    SIZENM = P.Key.SIZENM,
                //                    SHADE = P.Key.SHADE,
                //                    QNTY = P.Sum(A => A.QNTY),
                //                    UOM = P.Key.UOM,
                //                    NOS = P.Sum(A => A.NOS),
                //                    RATE = P.Key.RATE,
                //                    GSTPER = P.Key.GSTPER,
                //                    DISCRATE = P.Key.DISCRATE,
                //                    DISCTYPE = P.Key.DISCTYPE,
                //                    DISCTYPE_DESC = P.Key.DISCTYPE == "P" ? "%" : P.Key.DISCTYPE == "N" ? "Nos" : P.Key.DISCTYPE == "Q" ? "Qnty" : "Fixed",
                //                    TDDISCRATE = P.Key.TDDISCRATE,
                //                    TDDISCTYPE = P.Key.TDDISCTYPE,
                //                    SCMDISCRATE = P.Key.SCMDISCRATE,
                //                    SCMDISCTYPE = P.Key.SCMDISCTYPE,
                //                    BARNO = P.Key.BARNO,
                //                }).OrderBy(b => b.TXNSLNO).ToList();
                //for (int p = 0; p <= VE.TBATCHDTL.Count - 1; p++)
                //{
                //    VE.TBATCHDTL[p].SLNO = Convert.ToInt16(p + 1);
                //}
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_SALE_PRODUCT", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        //public ActionResult DeleteBarCodeRow(TransactionPackingSlipEntry VE, int SLNO)
        //{

        //    List<TBATCHDTL> TBATCHDTL = new List<TBATCHDTL>();
        //    int count = 0;
        //    for (int i = 0; i <= VE.TBATCHDTL.Count - 1; i++)
        //    {
        //        if (VE.TBATCHDTL[i].SLNO != SLNO)
        //        {
        //            count += 1;
        //            TBATCHDTL IFSC = new TBATCHDTL();
        //            IFSC = VE.TBATCHDTL[i];
        //            TBATCHDTL.Add(IFSC);
        //        }
        //    }
        //    VE.TBATCHDTL = TBATCHDTL;
        //    ModelState.Clear();
        //    VE.DefaultView = true;
        //    return PartialView("_T_SALE_PRODUCT", VE);

        //}
        public ActionResult FillDetailData(TransactionPackingSlipEntry VE)
        {
            try
            {
                VE.TTXNDTL = (from x in VE.TBATCHDTL
                              group x by new
                              {
                                  x.TXNSLNO,
                                  x.ITGRPCD,
                                  x.ITGRPNM,
                                  x.MTRLJOBCD,
                                  x.MTRLJOBNM,
                                  x.ITCD,
                                  x.ITNM,
                                  x.STYLENO,
                                  x.DISCTYPE,
                                  x.DISCTYPE_DESC,
                                  x.TDDISCTYPE,
                                  x.TDDISCTYPE_DESC,
                                  x.SCMDISCTYPE,
                                  x.SCMDISCTYPE_DESC,
                                  x.UOM,
                                  x.STKTYPE,
                                  x.RATE,
                                  x.DISCRATE,
                                  x.SCMDISCRATE,
                                  x.TDDISCRATE,
                                  x.GSTPER,
                                  x.ALL_GSTPER,
                                  x.FLAGMTR,
                              } into P
                              select new TTXNDTL
                              {
                                  SLNO = P.Key.TXNSLNO.retShort(),
                                  ITGRPCD = P.Key.ITGRPCD,
                                  ITGRPNM = P.Key.ITGRPNM,
                                  MTRLJOBCD = P.Key.MTRLJOBCD,
                                  MTRLJOBNM = P.Key.MTRLJOBNM,
                                  ITCD = P.Key.ITCD,
                                  ITNM = P.Key.STYLENO + "" + P.Key.ITNM,
                                  STYLENO = P.Key.STYLENO,
                                  STKTYPE = P.Key.STKTYPE,
                                  UOM = P.Key.UOM,
                                  NOS = P.Sum(A => A.NOS),
                                  QNTY = P.Sum(A => A.QNTY),
                                  FLAGMTR = P.Sum(A => A.FLAGMTR),
                                  BLQNTY = P.Sum(A => A.BLQNTY),
                                  RATE = P.Key.RATE,
                                  DISCTYPE = P.Key.DISCTYPE,
                                  DISCTYPE_DESC = P.Key.DISCTYPE_DESC,
                                  DISCRATE = P.Key.DISCRATE,
                                  TDDISCRATE = P.Key.TDDISCRATE,
                                  TDDISCTYPE = P.Key.TDDISCTYPE,
                                  TDDISCTYPE_DESC = P.Key.TDDISCTYPE_DESC,
                                  SCMDISCRATE = P.Key.SCMDISCRATE,
                                  SCMDISCTYPE = P.Key.SCMDISCTYPE,
                                  SCMDISCTYPE_DESC = P.Key.SCMDISCTYPE_DESC,
                                  ALL_GSTPER = P.Key.ALL_GSTPER,
                                  //AMT = P.Sum(A => A.BLQNTY).retDbl() == 0 ? (P.Sum(A => A.QNTY).retDbl() - P.Sum(A => A.FLAGMTR).retDbl()) * P.Key.RATE.retDbl() : P.Sum(A => A.BLQNTY).retDbl() * P.Key.RATE.retDbl(),
                              }).ToList();

                for (int p = 0; p <= VE.TTXNDTL.Count - 1; p++)
                {
                    if (VE.TTXNDTL[p].ALL_GSTPER.retStr() != "")
                    {
                        var tax_data = VE.TTXNDTL[p].ALL_GSTPER.Split(',').ToList();
                        VE.TTXNDTL[p].IGSTPER = tax_data[0].retDbl();
                        VE.TTXNDTL[p].CGSTPER = tax_data[1].retDbl();
                        VE.TTXNDTL[p].SGSTPER = tax_data[2].retDbl();
                    }
                }
                //VE.T_NOS = VE.TTXNDTL.Select(a => a.NOS).Sum().retDbl();
                //VE.T_QNTY = VE.TTXNDTL.Select(a => a.QNTY).Sum().retDbl();
                //VE.T_AMT = VE.TTXNDTL.Select(a => a.AMT).Sum().retDbl();
                //VE.T_GROSS_AMT = VE.TTXNDTL.Select(a => a.TXBLVAL).Sum().retDbl();
                //VE.T_IGST_AMT = VE.TTXNDTL.Select(a => a.IGSTAMT).Sum().retDbl();
                //VE.T_CGST_AMT = VE.TTXNDTL.Select(a => a.CGSTAMT).Sum().retDbl();
                //VE.T_SGST_AMT = VE.TTXNDTL.Select(a => a.SGSTAMT).Sum().retDbl();
                //VE.T_CESS_AMT = VE.TTXNDTL.Select(a => a.CESSAMT).Sum().retDbl();
                //VE.T_NET_AMT = VE.TTXNDTL.Select(a => a.NETAMT).Sum().retDbl();
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_SALE_DETAIL", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult OPEN_AMOUNT(TransactionPackingSlipEntry VE, string AUTO_NO, int A_T_NOS, double A_T_QNTY, double A_T_AMT, double A_T_TAXABLE, double A_T_IGST_AMT, double A_T_CGST_AMT, double A_T_SGST_AMT, double A_T_CESS_AMT, double A_T_NET_AMT, double IGST_PER, double CGST_PER, double SGST_PER, double CESS_PER)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            string COM_CD = CommVar.Compcd(UNQSNO); string LOC_CD = CommVar.Loccd(UNQSNO);
            string DATABASE = CommVar.CurSchema(UNQSNO).ToString();
            string DATABASEF = CommVar.FinSchema(UNQSNO);
            Cn.getQueryString(VE); string S_P = "";
            S_P = "S";


            ViewBag.NOS = A_T_NOS;
            ViewBag.QNTY = A_T_QNTY;
            ViewBag.TAXABLEAMT = A_T_TAXABLE;
            ViewBag.NETAMT = A_T_NET_AMT;
            ViewBag.IGSTAMT = A_T_IGST_AMT;
            ViewBag.CGSTAMT = A_T_CGST_AMT;
            ViewBag.SGSTAMT = A_T_SGST_AMT;
            ViewBag.CESSAMT = A_T_CESS_AMT;
            double A_CURR_AMT = 0; double A_TOTAL_AMT = 0; double A_IGST_AMT = 0; double A_CGST_AMT = 0; double A_SGST_AMT = 0; double A_CESS_AMT = 0; double A_DUTY_AMT = 0; double A_NET_AMT = 0;
            try
            {
                string QUERY = "select a.amtcd, b.amtnm, b.calccode, b.addless, b.CALCTYPE,b.calcformula, a.amtdesc, b.glcd, b.hsncode, a.amtrate, a.curr_amt, a.amt,a.igstper, a.igstamt, a.sgstper, a.sgstamt, a.cgstper, a.cgstamt,a.CESSPER,";
                QUERY = QUERY + " a.CESSAMT, a.dutyper, a.dutyamt from " + DATABASE + ".t_txnamt a, " + DATABASE + ".m_amttype b where a.amtcd=b.amtcd(+) and b.salpur='" + S_P + "' and a.autono='" + AUTO_NO + "' ";
                QUERY = QUERY + " union select b.amtcd, b.amtnm, b.calccode, b.addless,b.CALCTYPE, b.calcformula, '' amtdesc, b.glcd, b.hsncode, 0 amtrate, 0 curr_amt, 0 amt,0 igstper, 0 igstamt, 0 sgstper, 0 sgstamt, 0 cgstper, 0 cgstamt, ";
                QUERY = QUERY + " 0 CESSPER, 0 CESSAMT, 0 dutyper, 0 dutyamt from " + DATABASE + ".m_amttype b, " + DATABASE + ".m_cntrl_hdr c where b.m_autono=c.m_autono(+) and b.salpur='" + S_P + "'  and nvl(c.inactive_tag,'N') = 'N' ";
                QUERY = QUERY + "and b.amtcd not in (select amtcd from " + DATABASE + ".t_txnamt where autono='" + AUTO_NO + "')";

                var AMOUNT_DATA = masterHelp.SQLquery(QUERY);

                //ModelState.Clear();

                if (VE.TTXNAMT == null)
                {
                    VE.TTXNAMT = (from DataRow dr in AMOUNT_DATA.Rows
                                  select new TTXNAMT()
                                  {
                                      AMTCD = dr["amtcd"].ToString(),
                                      ADDLESS = dr["addless"].ToString(),
                                      GLCD = dr["GLCD"].ToString(),
                                      AMTNM = dr["amtnm"].ToString(),
                                      CALCCODE = dr["calccode"].ToString(),
                                      CALCTYPE = dr["CALCTYPE"].ToString(),
                                      CALCFORMULA = dr["calcformula"].ToString(),
                                      AMTDESC = dr["amtdesc"].ToString(),
                                      HSNCODE = dr["hsncode"].ToString(),
                                      AMTRATE = Convert.ToDouble(dr["amtrate"].ToString()),
                                      CURR_AMT = Convert.ToDouble(dr["curr_amt"].ToString()),
                                      AMT = Convert.ToDouble(dr["amt"].ToString()),
                                      IGSTPER = IGST_PER,
                                      CGSTPER = CGST_PER,
                                      SGSTPER = SGST_PER,
                                      CESSPER = CESS_PER,
                                      IGSTAMT = Convert.ToDouble(dr["igstamt"].ToString()),
                                      CGSTAMT = Convert.ToDouble(dr["cgstamt"].ToString()),
                                      SGSTAMT = Convert.ToDouble(dr["sgstamt"].ToString()),
                                      CESSAMT = Convert.ToDouble(dr["CESSAMT"].ToString()),
                                      DUTYAMT = Convert.ToDouble(dr["dutyamt"].ToString()),
                                  }).ToList();
                }

                for (int p = 0; p <= VE.TTXNAMT.Count - 1; p++)
                {
                    VE.TTXNAMT[p].SLNO = Convert.ToInt16(p + 1);

                    VE.TTXNAMT[p].NETAMT = VE.TTXNAMT[p].AMT == null ? 0 : VE.TTXNAMT[p].AMT.Value +
                                            VE.TTXNAMT[p].IGSTAMT == null ? 0 : VE.TTXNAMT[p].IGSTAMT.Value +
                                            VE.TTXNAMT[p].CGSTAMT == null ? 0 : VE.TTXNAMT[p].CGSTAMT.Value +
                                            VE.TTXNAMT[p].SGSTAMT == null ? 0 : VE.TTXNAMT[p].SGSTAMT.Value +
                                            VE.TTXNAMT[p].CESSAMT == null ? 0 : VE.TTXNAMT[p].CESSAMT.Value +
                                           VE.TTXNAMT[p].DUTYAMT == null ? 0 : VE.TTXNAMT[p].DUTYAMT.Value;

                    A_CURR_AMT = A_CURR_AMT + VE.TTXNAMT[p].CURR_AMT == null ? 0 : VE.TTXNAMT[p].CURR_AMT.Value;
                    A_TOTAL_AMT = A_TOTAL_AMT + VE.TTXNAMT[p].AMT == null ? 0 : VE.TTXNAMT[p].AMT.Value;
                    A_IGST_AMT = A_IGST_AMT + VE.TTXNAMT[p].IGSTAMT == null ? 0 : VE.TTXNAMT[p].IGSTAMT.Value;
                    A_CGST_AMT = A_CGST_AMT + VE.TTXNAMT[p].CGSTAMT == null ? 0 : VE.TTXNAMT[p].CGSTAMT.Value;
                    A_SGST_AMT = A_SGST_AMT + VE.TTXNAMT[p].SGSTAMT == null ? 0 : VE.TTXNAMT[p].SGSTAMT.Value;
                    A_CESS_AMT = A_CESS_AMT + VE.TTXNAMT[p].CESSAMT == null ? 0 : VE.TTXNAMT[p].CESSAMT.Value;
                    A_NET_AMT = A_NET_AMT + A_TOTAL_AMT + A_IGST_AMT + A_CGST_AMT + A_SGST_AMT + A_CESS_AMT;
                }
                VE.A_T_CURR = A_CURR_AMT;
                VE.A_T_AMOUNT = A_TOTAL_AMT;
                VE.A_T_IGST = A_IGST_AMT;
                VE.A_T_CGST = A_CGST_AMT;
                VE.A_T_SGST = A_SGST_AMT;
                VE.A_T_CESS = A_CESS_AMT;
                VE.A_T_DUTY = A_DUTY_AMT;
                VE.A_T_NET = A_NET_AMT;
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_SALE_AMOUNT", VE);

            }
            catch (Exception ex)
            {
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + ex.InnerException;
                return View(VE);
            }
        }
        public ActionResult DeleteRow(TransactionPackingSlipEntry VE)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                List<TBATCHDTL> TBATCHDTL = new List<TBATCHDTL>();
                int count = 0;
                for (int i = 0; i <= VE.TBATCHDTL.Count - 1; i++)
                {
                    if (VE.TBATCHDTL[i].Checked == false)
                    {
                        count += 1;
                        TBATCHDTL item = new TBATCHDTL();
                        item = VE.TBATCHDTL[i];
                        item.SLNO = Convert.ToSByte(count);
                        TBATCHDTL.Add(item);
                    }
                }
                VE.TBATCHDTL = TBATCHDTL;
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_SALE_PRODUCT", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult AddDOCRow(TransactionPackingSlipEntry VE)
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
        public ActionResult DeleteDOCRow(TransactionPackingSlipEntry VE)
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
        public ActionResult cancelRecords(TransactionPackingSlipEntry VE, string par1)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                Cn.getQueryString(VE);
                using (var transaction = DB.Database.BeginTransaction())
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".T_CNTRL_HDR in  row share mode");
                    T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                    if (par1 == "*#*")
                    {
                        TCH = Cn.T_CONTROL_HDR(VE.T_TXN.AUTONO, CommVar.CurSchema(UNQSNO).ToString());
                    }
                    else
                    {
                        TCH = Cn.T_CONTROL_HDR(VE.T_TXN.AUTONO, CommVar.CurSchema(UNQSNO).ToString(), par1);
                    }
                    DB.Entry(TCH).State = System.Data.Entity.EntityState.Modified;
                    DB.SaveChanges();
                    transaction.Commit();
                }
                using (var transaction = DB.Database.BeginTransaction())
                {
                    DBF.Database.ExecuteSqlCommand("lock table " + CommVar.FinSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode");
                    T_CNTRL_HDR TCH1 = new T_CNTRL_HDR();
                    if (par1 == "*#*")
                    {
                        TCH1 = Cn.T_CONTROL_HDR(VE.T_TXN.AUTONO, CommVar.FinSchema(UNQSNO));
                    }
                    else
                    {
                        TCH1 = Cn.T_CONTROL_HDR(VE.T_TXN.AUTONO, CommVar.FinSchema(UNQSNO), par1);
                    }
                    DBF.Entry(TCH1).State = System.Data.Entity.EntityState.Modified;
                    DBF.SaveChanges();
                    transaction.Commit();
                }
                return Content("1");
            }
            catch (Exception ex)
            {
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult ParkRecord(FormCollection FC, TransactionPackingSlipEntry stream, string menuID, string menuIndex)
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
                return Content(ex.Message);
            }
        }
        public string RetrivePark(string value)
        {
            try
            {
                string url = masterHelp.RetriveParkFromFile(value, Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString(), "Improvar.ViewModels.TransactionPackingSlipEntry");
                return url;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public ActionResult SAVE(FormCollection FC, TransactionPackingSlipEntry VE)
        {
            Cn.getQueryString(VE);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            //Oracle Queries
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            OracleTransaction OraTrans;
            string dbsql = "";
            string[] dbsql1;

            OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted);
            OraCmd.Transaction = OraTrans;
            //
            DB.Configuration.ValidateOnSaveEnabled = false;
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    //DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".T_CNTRL_HDR in  row share mode");
                    OraCmd.CommandText = "lock table " + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode"; OraCmd.ExecuteNonQuery();

                    string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                    string ContentFlg = "";
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        T_TXN TTXN = new T_TXN();
                        T_TXNTRANS TXNTRANS = new T_TXNTRANS();
                        T_TXNOTH TTXNOTH = new T_TXNOTH();
                        T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                        //T_VCH_GST TVCHGST = new T_VCH_GST();
                        //T_CNTRL_DOC_PASS TCDP = new T_CNTRL_DOC_PASS();
                        string DOCPATTERN = "";
                        TTXN.DOCDT = VE.T_TXN.DOCDT;
                        string Ddate = Convert.ToString(TTXN.DOCDT);
                        TTXN.CLCD = CommVar.ClientCode(UNQSNO);
                        string auto_no = ""; string Month = "";
                        if (VE.DefaultAction == "A")
                        {
                            TTXN.EMD_NO = 0;
                            TTXN.DOCCD = VE.T_TXN.DOCCD;
                            TTXN.DOCNO = Cn.MaxDocNumber(TTXN.DOCCD, Ddate);
                            //TTXN.DOCNO = Cn.MaxDocNumber(TTXN.DOCCD, Ddate);
                            DOCPATTERN = Cn.DocPattern(Convert.ToInt32(TTXN.DOCNO), TTXN.DOCCD, CommVar.CurSchema(UNQSNO).ToString(), CommVar.FinSchema(UNQSNO), Ddate);
                            auto_no = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), TTXN.DOCNO, TTXN.DOCCD, Ddate);
                            TTXN.AUTONO = auto_no.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
                            Month = auto_no.Split(Convert.ToChar(Cn.GCS()))[1].ToString();
                        }
                        else
                        {
                            TTXN.DOCCD = VE.T_TXN.DOCCD;
                            TTXN.DOCNO = VE.T_TXN.DOCNO;
                            TTXN.AUTONO = VE.T_TXN.AUTONO;
                            Month = VE.T_CNTRL_HDR.MNTHCD;
                            TTXN.EMD_NO = Convert.ToByte((VE.T_CNTRL_HDR.EMD_NO == null ? 0 : VE.T_CNTRL_HDR.EMD_NO) + 1);
                            DOCPATTERN = VE.T_CNTRL_HDR.DOCNO;
                        }
                        TTXN.DOCTAG = "AA";// VE.MENU_PARA;
                        TTXN.SLCD = VE.T_TXN.SLCD;
                        TTXN.CONSLCD = VE.T_TXN.CONSLCD;
                        TTXN.CURR_CD = VE.T_TXN.CURR_CD;
                        TTXN.CURRRT = VE.T_TXN.CURRRT;
                        TTXN.BLAMT = VE.T_TXN.BLAMT;
                        TTXN.PREFDT = VE.T_TXN.PREFDT;
                        TTXN.PREFNO = VE.T_TXN.PREFNO;
                        TTXN.REVCHRG = VE.T_TXN.REVCHRG;
                        TTXN.ROAMT = VE.T_TXN.ROAMT;
                        if (VE.RoundOff == true) { TTXN.ROYN = "Y"; } else { TTXN.ROYN = "N"; }
                        TTXN.GOCD = VE.T_TXN.GOCD;
                        TTXN.JOBCD = VE.T_TXN.JOBCD;
                        TTXN.MANSLIPNO = VE.T_TXN.MANSLIPNO;
                        TTXN.DUEDAYS = VE.T_TXN.DUEDAYS;
                        TTXN.PARGLCD = VE.T_TXN.PARGLCD;
                        TTXN.GLCD = VE.T_TXN.GLCD;
                        TTXN.CLASS1CD = VE.T_TXN.CLASS1CD;
                        TTXN.CLASS2CD = VE.T_TXN.CLASS2CD;
                        TTXN.LINECD = VE.T_TXN.LINECD;
                        TTXN.BARGENTYPE = VE.T_TXN.BARGENTYPE;
                        TTXN.WPPER = VE.T_TXN.WPPER;
                        TTXN.RPPER = VE.T_TXN.RPPER;
                        TTXN.MENU_PARA = VE.T_TXN.MENU_PARA;
                        TTXN.TCSPER = VE.T_TXN.TCSPER;
                        TTXN.TCSAMT = VE.T_TXN.TCSAMT;
                        TTXN.DTAG = (VE.DefaultAction == "E" ? "E" : null);


                        if (VE.DefaultAction == "E")
                        {
                            dbsql = masterHelp.TblUpdt("t_batchdtl", TTXN.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            dbsql = masterHelp.TblUpdt("t_batchmst", TTXN.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            dbsql = masterHelp.TblUpdt("t_txndtl", TTXN.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            dbsql = masterHelp.TblUpdt("t_txn_linkno", TTXN.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            dbsql = masterHelp.TblUpdt("t_txntrans", TTXN.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            dbsql = masterHelp.TblUpdt("t_txnoth", TTXN.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            //dbsql = masterHelp.TblUpdt("t_vch_gst", TTXN.AUTONO, "E");
                            //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            dbsql = masterHelp.TblUpdt("t_txnamt", TTXN.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            dbsql = masterHelp.TblUpdt("t_cntrl_hdr_rem", TTXN.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc_dtl", TTXN.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc", TTXN.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            dbsql = masterHelp.TblUpdt("t_cntrl_doc_pass", TTXN.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        }

                        //-------------------------Transport--------------------------//
                        TXNTRANS.AUTONO = TTXN.AUTONO;
                        TXNTRANS.EMD_NO = TTXN.EMD_NO;
                        TXNTRANS.CLCD = TTXN.CLCD;
                        TXNTRANS.DTAG = TTXN.DTAG;
                        TXNTRANS.TRANSLCD = VE.T_TXNTRANS.TRANSLCD;
                        TXNTRANS.TRANSMODE = VE.T_TXNTRANS.TRANSMODE;
                        TXNTRANS.CRSLCD = VE.T_TXNTRANS.CRSLCD;
                        TXNTRANS.EWAYBILLNO = VE.T_TXNTRANS.EWAYBILLNO;
                        TXNTRANS.LRNO = VE.T_TXNTRANS.LRNO;
                        TXNTRANS.LRDT = VE.T_TXNTRANS.LRDT;
                        TXNTRANS.LORRYNO = VE.T_TXNTRANS.LORRYNO;
                        TXNTRANS.GRWT = VE.T_TXNTRANS.GRWT;
                        TXNTRANS.TRWT = VE.T_TXNTRANS.TRWT;
                        TXNTRANS.NTWT = VE.T_TXNTRANS.NTWT;
                        TXNTRANS.DESTN = VE.T_TXNTRANS.DESTN;
                        TXNTRANS.RECVPERSON = VE.T_TXNTRANS.RECVPERSON;
                        TXNTRANS.VECHLTYPE = VE.T_TXNTRANS.VECHLTYPE;
                        TXNTRANS.GATEENTNO = VE.T_TXNTRANS.GATEENTNO;
                        //----------------------------------------------------------//
                        //-------------------------Other Info--------------------------//
                        TTXNOTH.AUTONO = TTXN.AUTONO;
                        TTXNOTH.EMD_NO = TTXN.EMD_NO;
                        TTXNOTH.CLCD = TTXN.CLCD;
                        TTXNOTH.DTAG = TTXN.DTAG;
                        TTXNOTH.DOCREM = VE.T_TXNOTH.DOCREM;
                        TTXNOTH.DNCNCD = VE.T_TXNOTH.DNCNCD;
                        TTXNOTH.DNSALPUR = VE.T_TXNOTH.DNSALPUR;
                        TTXNOTH.AGSLCD = VE.T_TXNOTH.AGSLCD;
                        TTXNOTH.SAGSLCD = VE.T_TXNOTH.SAGSLCD;
                        TTXNOTH.BLTYPE = VE.T_TXNOTH.BLTYPE;
                        TTXNOTH.DESTN = VE.T_TXNOTH.DESTN;
                        TTXNOTH.PLSUPPLY = VE.T_TXNOTH.PLSUPPLY;
                        TTXNOTH.OTHADD1 = VE.T_TXNOTH.OTHADD1;
                        TTXNOTH.OTHADD2 = VE.T_TXNOTH.OTHADD2;
                        TTXNOTH.OTHADD3 = VE.T_TXNOTH.OTHADD3;
                        TTXNOTH.OTHADD4 = VE.T_TXNOTH.OTHADD4;
                        TTXNOTH.INSBY = VE.T_TXNOTH.INSBY;
                        TTXNOTH.PAYTERMS = VE.T_TXNOTH.PAYTERMS;
                        TTXNOTH.CASENOS = VE.T_TXNOTH.CASENOS;
                        TTXNOTH.NOOFCASES = VE.T_TXNOTH.NOOFCASES;
                        TTXNOTH.PRCCD = VE.T_TXNOTH.PRCCD;
                        TTXNOTH.OTHNM = VE.T_TXNOTH.OTHNM;
                        TTXNOTH.COD = VE.T_TXNOTH.COD;
                        TTXNOTH.DOCTH = VE.T_TXNOTH.DOCTH;
                        TTXNOTH.POREFNO = VE.T_TXNOTH.POREFNO;
                        TTXNOTH.POREFDT = VE.T_TXNOTH.POREFDT;
                        TTXNOTH.ECOMM = VE.T_TXNOTH.ECOMM;
                        TTXNOTH.EXPCD = VE.T_TXNOTH.EXPCD;
                        TTXNOTH.GSTNO = VE.T_TXNOTH.GSTNO;
                        TTXNOTH.PNM = VE.T_TXNOTH.PNM;
                        TTXNOTH.POS = VE.T_TXNOTH.POS;
                        TTXNOTH.PACKBY = VE.T_TXNOTH.PACKBY;
                        TTXNOTH.SELBY = VE.T_TXNOTH.SELBY;
                        TTXNOTH.DEALBY = VE.T_TXNOTH.DEALBY;
                        TTXNOTH.DESPBY = VE.T_TXNOTH.DESPBY;
                        TTXNOTH.TAXGRPCD = VE.T_TXNOTH.TAXGRPCD;
                        TTXNOTH.TDSHD = VE.T_TXNOTH.TDSHD;
                        TTXNOTH.TDSON = VE.T_TXNOTH.TDSON;
                        TTXNOTH.TDSPER = VE.T_TXNOTH.TDSPER;
                        TTXNOTH.TDSAMT = VE.T_TXNOTH.TDSAMT;


                        //----------------------------------------------------------//
                        dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(TTXN.AUTONO, VE.DefaultAction, "S", Month, TTXN.DOCCD, DOCPATTERN, TTXN.DOCDT.retStr(), TTXN.EMD_NO.retShort(), TTXN.DOCNO, Convert.ToDouble(TTXN.DOCNO), null, null, null, TTXN.SLCD);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                        dbsql = masterHelp.RetModeltoSql(TTXN, VE.DefaultAction);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        dbsql = masterHelp.RetModeltoSql(TXNTRANS);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        dbsql = masterHelp.RetModeltoSql(TTXNOTH);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();


                        // sAVE T_CNTRL_HDR_UNIQNO
                        string transactionBarcode = ""; string docbarcode = "";
                        string sql = "select doccd,docbarcode from " + CommVar.CurSchema(UNQSNO) + ".m_doctype_bar where doccd='" + TTXN.DOCCD + "'";
                        DataTable dt = masterHelp.SQLquery(sql);
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            docbarcode = dt.Rows[0]["docbarcode"].retStr();
                            T_CNTRL_HDR_UNIQNO TCHUNIQNO = new T_CNTRL_HDR_UNIQNO();
                            TCHUNIQNO.CLCD = TTXN.CLCD;
                            TCHUNIQNO.AUTONO = TTXN.AUTONO;
                            TCHUNIQNO.UNIQNO = salesfunc.retVchrUniqId(TTXN.DOCCD, TTXN.AUTONO);
                            dbsql = masterHelp.RetModeltoSql(TCHUNIQNO);
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            transactionBarcode = TranBarcodeGenerate(TTXN.DOCCD, docbarcode, TCHUNIQNO.UNIQNO, 1);
                        }
                        //END SAVING 

                        int COUNTER = 0;
                        string stkdrcr = "C";

                        switch (VE.MENU_PARA)
                        {
                            case "SBPCK":
                                stkdrcr = "N"; break;
                            case "SB":
                                stkdrcr = "C"; break;
                            case "SBDIR":
                                stkdrcr = "C"; break;
                            case "SR":
                                stkdrcr = "D"; break;
                            case "SBCM":
                                stkdrcr = "C"; break;
                            case "SBCMR":
                                stkdrcr = "D"; break;
                            case "SBEXP":
                                stkdrcr = "C"; break;
                            case "PI":
                                stkdrcr = "0"; break;
                            case "PB":
                                stkdrcr = "D"; break;
                            case "PR":
                                stkdrcr = "C"; break;
                        }

                        for (int i = 0; i <= VE.TTXNDTL.Count - 1; i++)
                        {
                            if (VE.TTXNDTL[i].SLNO != 0 && VE.TTXNDTL[i].ITCD != null)
                            {
                                COUNTER = COUNTER + 1;
                                T_TXNDTL TTXNDTL = new T_TXNDTL();
                                TTXNDTL.CLCD = TTXN.CLCD;
                                TTXNDTL.EMD_NO = TTXN.EMD_NO;
                                TTXNDTL.DTAG = TTXN.DTAG;
                                TTXNDTL.AUTONO = TTXN.AUTONO;
                                TTXNDTL.SLNO = VE.TTXNDTL[i].SLNO;
                                TTXNDTL.MTRLJOBCD = VE.TTXNDTL[i].MTRLJOBCD;
                                TTXNDTL.ITCD = VE.TTXNDTL[i].ITCD;
                                TTXNDTL.PARTCD = VE.TTXNDTL[i].PARTCD;
                                TTXNDTL.COLRCD = VE.TTXNDTL[i].COLRCD;
                                TTXNDTL.SIZECD = VE.TTXNDTL[i].SIZECD;
                                TTXNDTL.STKDRCR = stkdrcr;
                                TTXNDTL.STKTYPE = VE.TTXNDTL[i].STKTYPE;
                                TTXNDTL.HSNCODE = VE.TTXNDTL[i].HSNCODE;
                                TTXNDTL.ITREM = VE.TTXNDTL[i].ITREM;
                                TTXNDTL.PCSREM = VE.TTXNDTL[i].PCSREM;
                                TTXNDTL.FREESTK = VE.TTXNDTL[i].FREESTK;
                                TTXNDTL.BATCHNO = VE.TTXNDTL[i].BATCHNO;
                                //TTXNDTL.BALEYR = VE.TTXNDTL[i].BALEYR;
                                TTXNDTL.BALENO = VE.TTXNDTL[i].BALENO;
                                TTXNDTL.GOCD = VE.T_TXN.GOCD;
                                TTXNDTL.JOBCD = VE.TTXNDTL[i].JOBCD;
                                TTXNDTL.NOS = VE.TTXNDTL[i].NOS == null ? 0 : VE.TTXNDTL[i].NOS;
                                TTXNDTL.QNTY = VE.TTXNDTL[i].QNTY;
                                TTXNDTL.BLQNTY = VE.TTXNDTL[i].BLQNTY;
                                TTXNDTL.RATE = VE.TTXNDTL[i].RATE;
                                TTXNDTL.AMT = VE.TTXNDTL[i].AMT;
                                TTXNDTL.FLAGMTR = VE.TTXNDTL[i].FLAGMTR;
                                //TTXNDTL.TOTDISCAMT = VE.TTXNDTL[i].TOTDISCAMT;
                                TTXNDTL.TXBLVAL = VE.TTXNDTL[i].TXBLVAL;
                                TTXNDTL.IGSTPER = VE.TTXNDTL[i].IGSTPER;
                                TTXNDTL.CGSTPER = VE.TTXNDTL[i].CGSTPER;
                                TTXNDTL.SGSTPER = VE.TTXNDTL[i].SGSTPER;
                                TTXNDTL.CESSPER = VE.TTXNDTL[i].CESSPER;
                                TTXNDTL.DUTYPER = VE.TTXNDTL[i].DUTYPER;
                                TTXNDTL.IGSTAMT = VE.TTXNDTL[i].IGSTAMT;
                                TTXNDTL.CGSTAMT = VE.TTXNDTL[i].CGSTAMT;
                                TTXNDTL.SGSTAMT = VE.TTXNDTL[i].SGSTAMT;
                                TTXNDTL.CESSAMT = VE.TTXNDTL[i].CESSAMT;
                                TTXNDTL.DUTYAMT = VE.TTXNDTL[i].DUTYAMT;
                                TTXNDTL.NETAMT = VE.TTXNDTL[i].NETAMT;
                                //TTXNDTL.OTHRAMT = VE.TTXNDTL[i].OTHRAMT;
                                //TTXNDTL.AGDOCNO = VE.TTXNDTL[i].AGSTDOCNO;
                                //TTXNDTL.AGDOCDT = VE.TTXNDTL[i].AGSTDOCDT;
                                TTXNDTL.SHORTQNTY = VE.TTXNDTL[i].SHORTQNTY;
                                TTXNDTL.DISCTYPE = VE.TTXNDTL[i].DISCTYPE;
                                TTXNDTL.DISCRATE = VE.TTXNDTL[i].DISCRATE;
                                TTXNDTL.DISCAMT = VE.TTXNDTL[i].DISCAMT;
                                TTXNDTL.SCMDISCTYPE = VE.TTXNDTL[i].SCMDISCTYPE;
                                TTXNDTL.SCMDISCRATE = VE.TTXNDTL[i].SCMDISCRATE;
                                TTXNDTL.SCMDISCAMT = VE.TTXNDTL[i].SCMDISCAMT;
                                TTXNDTL.TDDISCTYPE = VE.TTXNDTL[i].TDDISCTYPE;
                                TTXNDTL.TDDISCRATE = VE.TTXNDTL[i].TDDISCRATE;
                                TTXNDTL.TDDISCAMT = VE.TTXNDTL[i].TDDISCAMT;
                                TTXNDTL.PRCCD = VE.T_TXNOTH.PRCCD;
                                //TTXNDTL.PRCEFFDT = VE.T_TXN.PRCEFFDT;
                                TTXNDTL.BARNO = VE.TTXNDTL[i].BARNO;
                                TTXNDTL.GLCD = VE.TTXNDTL[i].GLCD;
                                //TTXNDTL.CLASS1CD = VE.TTXNDTL[i].CLASS1CD;
                                dbsql = masterHelp.RetModeltoSql(TTXNDTL);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                            }
                        }

                        var BATCHMST = (from x in VE.TBATCHDTL
                                        group x by new
                                        {
                                            //x.SLNO,
                                            x.MTRLJOBCD,
                                            x.ITCD,
                                            x.STKTYPE,
                                            x.RATE,
                                            x.FLAGMTR,
                                            x.BARNO,
                                            x.PARTCD,
                                            x.SIZECD,
                                            x.COLRCD,
                                            x.SHADE,
                                        } into P
                                        select new
                                        {
                                            //SLNO = P.Key.SLNO.retShort(),
                                            MTRLJOBCD = P.Key.MTRLJOBCD,
                                            ITCD = P.Key.ITCD,
                                            STKTYPE = P.Key.STKTYPE,
                                            NOS = P.Sum(A => A.NOS),
                                            QNTY = P.Sum(A => A.QNTY),
                                            FLAGMTR = P.Key.FLAGMTR,
                                            BLQNTY = P.Sum(A => A.BLQNTY),
                                            RATE = P.Key.RATE,
                                            BARNO = P.Key.BARNO,
                                            PARTCD = P.Key.PARTCD,
                                            SIZECD = P.Key.SIZECD,
                                            COLRCD = P.Key.COLRCD,
                                            SHADE = P.Key.SHADE,
                                        }).ToList();
                        COUNTER = 0;
                        if (BATCHMST != null && BATCHMST.Count > 0)
                        {
                            for (int i = 0; i <= BATCHMST.Count - 1; i++)
                            {
                                if (BATCHMST[i].STKTYPE != null && BATCHMST[i].BARNO != null && BATCHMST[i].MTRLJOBCD != null && BATCHMST[i].ITCD != null)
                                {
                                    COUNTER = COUNTER + 1;
                                    T_BATCHMST TBATCHMST = new T_BATCHMST();
                                    TBATCHMST.EMD_NO = TTXN.EMD_NO;
                                    TBATCHMST.CLCD = TTXN.CLCD;
                                    TBATCHMST.DTAG = TTXN.DTAG;
                                    TBATCHMST.TTAG = TTXN.TTAG;
                                    TBATCHMST.BARNO = BATCHMST[i].BARNO;
                                    TBATCHMST.AUTONO = TTXN.AUTONO;
                                    TBATCHMST.SLNO = COUNTER.retShort();
                                    TBATCHMST.SLCD = TTXN.SLCD;
                                    TBATCHMST.MTRLJOBCD = BATCHMST[i].MTRLJOBCD;
                                    TBATCHMST.STKTYPE = BATCHMST[i].STKTYPE;
                                    TBATCHMST.JOBCD = TTXN.JOBCD;
                                    TBATCHMST.ITCD = BATCHMST[i].ITCD;
                                    TBATCHMST.PARTCD = BATCHMST[i].PARTCD;
                                    TBATCHMST.SIZECD = BATCHMST[i].SIZECD;
                                    TBATCHMST.COLRCD = BATCHMST[i].COLRCD;
                                    TBATCHMST.NOS = BATCHMST[i].NOS;
                                    TBATCHMST.QNTY = BATCHMST[i].QNTY;
                                    TBATCHMST.RATE = BATCHMST[i].RATE;
                                    //TBATCHMST.AMT = BATCHMST[i].AMT;
                                    TBATCHMST.FLAGMTR = BATCHMST[i].FLAGMTR;
                                    //TBATCHMST.MTRL_COST = BATCHMST[i].MTRL_COST;
                                    //TBATCHMST.OTH_COST = BATCHMST[i].OTH_COST;
                                    //TBATCHMST.ITREM = BATCHMST[i].ITREM;
                                    //TBATCHMST.PDESIGN = BATCHMST[i].PDESIGN;
                                    //TBATCHMST.HSNCODE = BATCHMST[i].HSNCODE;
                                    //TBATCHMST.ORGBATCHAUTONO = BATCHMST[i].ORGBATCHAUTONO;
                                    //TBATCHMST.ORGBATCHSLNO = BATCHMST[i].ORGBATCHSLNO;
                                    //TBATCHMST.DIA = BATCHMST[i].DIA;
                                    //TBATCHMST.CUTLENGTH = BATCHMST[i].CUTLENGTH;
                                    //TBATCHMST.LOCABIN = BATCHMST[i].LOCABIN;
                                    TBATCHMST.SHADE = BATCHMST[i].SHADE;
                                    //TBATCHMST.MILLNM = BATCHMST[i].MILLNM;
                                    //TBATCHMST.BATCHNO = BATCHMST[i].BATCHNO;
                                    //TBATCHMST.ORDAUTONO = BATCHMST[i].ORDAUTONO;
                                    //TBATCHMST.BARNO = BATCHMST[i].ORDSLN6O;
                                    dbsql = masterHelp.RetModeltoSql(TBATCHMST);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                }
                            }
                        }
                        COUNTER = 0;
                        if (VE.TBATCHDTL != null && VE.TBATCHDTL.Count > 0)
                        {
                            for (int i = 0; i <= VE.TBATCHDTL.Count - 1; i++)
                            {
                                if (VE.TBATCHDTL[i].TXNSLNO != 0 && VE.T_TXN.GOCD != null && VE.TBATCHDTL[i].BARNO != null && VE.TBATCHDTL[i].MTRLJOBCD != null)
                                {
                                    COUNTER = COUNTER + 1;
                                    T_BATCHDTL TBATCHDTL = new T_BATCHDTL();
                                    TBATCHDTL.EMD_NO = TTXN.EMD_NO;
                                    TBATCHDTL.CLCD = TTXN.CLCD;
                                    TBATCHDTL.DTAG = TTXN.DTAG;
                                    TBATCHDTL.TTAG = TTXN.TTAG;
                                    TBATCHDTL.AUTONO = TTXN.AUTONO;
                                    TBATCHDTL.TXNSLNO = VE.TBATCHDTL[i].TXNSLNO;
                                    TBATCHDTL.SLNO = COUNTER.retShort();
                                    TBATCHDTL.GOCD = VE.T_TXN.GOCD;
                                    TBATCHDTL.BARNO = VE.TBATCHDTL[i].BARNO;
                                    TBATCHDTL.MTRLJOBCD = VE.TBATCHDTL[i].MTRLJOBCD;
                                    TBATCHDTL.PARTCD = VE.TBATCHDTL[i].PARTCD;
                                    TBATCHDTL.HSNCODE = VE.TBATCHDTL[i].HSNCODE;
                                    TBATCHDTL.STKDRCR = stkdrcr;
                                    TBATCHDTL.NOS = VE.TBATCHDTL[i].NOS;
                                    TBATCHDTL.QNTY = VE.TBATCHDTL[i].QNTY;
                                    TBATCHDTL.BLQNTY = VE.TBATCHDTL[i].BLQNTY;
                                    TBATCHDTL.FLAGMTR = VE.TBATCHDTL[i].FLAGMTR;
                                    TBATCHDTL.ITREM = VE.TBATCHDTL[i].ITREM;
                                    TBATCHDTL.RATE = VE.TBATCHDTL[i].RATE;
                                    TBATCHDTL.DISCRATE = VE.TBATCHDTL[i].DISCRATE;
                                    TBATCHDTL.DISCTYPE = VE.TBATCHDTL[i].DISCTYPE;
                                    TBATCHDTL.SCMDISCRATE = VE.TBATCHDTL[i].SCMDISCRATE;
                                    TBATCHDTL.SCMDISCTYPE = VE.TBATCHDTL[i].SCMDISCTYPE;
                                    TBATCHDTL.TDDISCRATE = VE.TBATCHDTL[i].TDDISCRATE;
                                    TBATCHDTL.TDDISCTYPE = VE.TBATCHDTL[i].TDDISCTYPE;
                                    TBATCHDTL.ORDAUTONO = VE.TBATCHDTL[i].ORDAUTONO;
                                    TBATCHDTL.ORDSLNO = VE.TBATCHDTL[i].ORDSLNO;
                                    TBATCHDTL.DIA = VE.TBATCHDTL[i].DIA;
                                    TBATCHDTL.CUTLENGTH = VE.TBATCHDTL[i].CUTLENGTH;
                                    TBATCHDTL.LOCABIN = VE.TBATCHDTL[i].LOCABIN;
                                    TBATCHDTL.SHADE = VE.TBATCHDTL[i].SHADE;
                                    TBATCHDTL.MILLNM = VE.TBATCHDTL[i].MILLNM;
                                    TBATCHDTL.BATCHNO = VE.TBATCHDTL[i].BATCHNO;
                                    //TBATCHDTL.BALEYR = VE.TBATCHDTL[i].BALEYR;
                                    //TBATCHDTL.BALENO = VE.TBATCHDTL[i].BALENO;
                                    TBATCHDTL.RECPROGAUTONO = VE.TBATCHDTL[i].RECPROGAUTONO;
                                    TBATCHDTL.RECPROGLOTNO = VE.TBATCHDTL[i].RECPROGLOTNO;
                                    TBATCHDTL.RECPROGSLNO = VE.TBATCHDTL[i].RECPROGSLNO;
                                    dbsql = masterHelp.RetModeltoSql(TBATCHDTL);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }
                        }
                        if (VE.TTXNDTL != null && VE.TTXNDTL.Count > 0)
                        {
                            if (VE.TTXNDTL != null && VE.TTXNDTL.Count > 0)
                            {
                                for (int i = 0; i <= VE.TTXNDTL.Count - 1; i++)
                                {
                                    if (VE.TTXNDTL[i].Checked == true)
                                    {
                                        T_TXN_LINKNO TTXNPSLIP = new T_TXN_LINKNO();
                                        TTXNPSLIP.EMD_NO = TTXN.EMD_NO;
                                        TTXNPSLIP.CLCD = TTXN.CLCD;
                                        TTXNPSLIP.DTAG = TTXN.DTAG;
                                        TTXNPSLIP.TTAG = TTXN.TTAG;
                                        TTXNPSLIP.AUTONO = TTXN.AUTONO;
                                        TTXNPSLIP.LINKAUTONO = TTXN.AUTONO;
                                        TTXNPSLIP.ISSAUTONO = TTXN.AUTONO;

                                        dbsql = masterHelp.RetModeltoSql(TTXNPSLIP);
                                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }


                        if (VE.TTXNAMT != null)
                        {
                            for (int i = 0; i <= VE.TTXNAMT.Count - 1; i++)
                            {
                                if (VE.TTXNAMT[i].SLNO != 0 && VE.TTXNAMT[i].AMTCD != null && VE.TTXNAMT[i].AMT != 0)
                                {
                                    T_TXNAMT TTXNAMT = new T_TXNAMT();
                                    TTXNAMT.AUTONO = TTXN.AUTONO;
                                    TTXNAMT.SLNO = VE.TTXNAMT[i].SLNO;
                                    TTXNAMT.EMD_NO = TTXN.EMD_NO;
                                    TTXNAMT.CLCD = TTXN.CLCD;
                                    TTXNAMT.DTAG = TTXN.DTAG;
                                    TTXNAMT.AMTCD = VE.TTXNAMT[i].AMTCD;
                                    TTXNAMT.AMTDESC = VE.TTXNAMT[i].AMTDESC;
                                    TTXNAMT.AMTRATE = VE.TTXNAMT[i].AMTRATE;
                                    TTXNAMT.HSNCODE = VE.TTXNAMT[i].HSNCODE;
                                    TTXNAMT.CURR_AMT = VE.TTXNAMT[i].CURR_AMT;
                                    TTXNAMT.AMT = VE.TTXNAMT[i].AMT;
                                    TTXNAMT.IGSTPER = VE.TTXNAMT[i].IGSTPER;
                                    TTXNAMT.IGSTAMT = VE.TTXNAMT[i].IGSTAMT;
                                    TTXNAMT.CGSTPER = VE.TTXNAMT[i].CGSTPER;
                                    TTXNAMT.CGSTAMT = VE.TTXNAMT[i].CGSTAMT;
                                    TTXNAMT.SGSTPER = VE.TTXNAMT[i].SGSTPER;
                                    TTXNAMT.SGSTAMT = VE.TTXNAMT[i].SGSTAMT;
                                    TTXNAMT.CESSPER = VE.TTXNAMT[i].CESSPER;
                                    TTXNAMT.CESSAMT = VE.TTXNAMT[i].CESSAMT;
                                    TTXNAMT.DUTYPER = VE.TTXNAMT[i].DUTYPER;
                                    TTXNAMT.DUTYAMT = VE.TTXNAMT[i].DUTYAMT;
                                    dbsql = masterHelp.RetModeltoSql(TTXNAMT);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }
                        }
                        //-----------------------DOCUMENT PASSING DATA---------------------------//
                        double TRAN_AMT = Convert.ToDouble(TTXN.BLAMT);

                        var TCDP_DATA = Cn.T_CONTROL_DOC_PASS(TTXN.DOCCD, TRAN_AMT, TTXN.EMD_NO.Value, TTXN.AUTONO, CommVar.CurSchema(UNQSNO).ToString());
                        if (TCDP_DATA.Item1.Count != 0)
                        {
                            for (int tr = 0; tr <= TCDP_DATA.Item1.Count - 1; tr++)
                            {
                                dbsql = masterHelp.RetModeltoSql(TCDP_DATA.Item1[tr]);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            }
                        }
                        //if (docpassrem != "") DB.T_CNTRL_DOC_PASS.Add(TCDP);

                        if (VE.UploadDOC != null)// add
                        {
                            var img = Cn.SaveUploadImageTransaction(VE.UploadDOC, TTXN.AUTONO, TTXN.EMD_NO.Value);
                            if (img.Item1.Count != 0)
                            {
                                for (int tr = 0; tr <= img.Item1.Count - 1; tr++)
                                {
                                    dbsql = masterHelp.RetModeltoSql(img.Item1[tr]);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                                for (int tr = 0; tr <= img.Item2.Count - 1; tr++)
                                {
                                    dbsql = masterHelp.RetModeltoSql(img.Item2[tr]);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }
                        }
                        if (VE.T_CNTRL_HDR_REM.DOCREM != null)// add REMARKS
                        {
                            var NOTE = Cn.SAVETRANSACTIONREMARKS(VE.T_CNTRL_HDR_REM, TTXN.AUTONO, TTXN.CLCD, TTXN.EMD_NO.Value);
                            if (NOTE.Item1.Count != 0)
                            {
                                for (int tr = 0; tr <= NOTE.Item1.Count - 1; tr++)
                                {
                                    dbsql = masterHelp.RetModeltoSql(NOTE.Item1[tr]);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }
                        }
                        string sslcd = VE.T_TXN.SLCD;


                        if (VE.DefaultAction == "A")
                        {
                            ContentFlg = "1" + " (Voucher No. " + TTXN.DOCNO + ")~" + TTXN.AUTONO;
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            ContentFlg = "2";
                        }
                        transaction.Commit();
                        OraTrans.Commit();
                        OraCon.Dispose();
                        return Content(ContentFlg);
                    }
                    else if (VE.DefaultAction == "V")
                    {
                        dbsql = masterHelp.TblUpdt("t_txnoth", VE.T_TXN.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_batchdtl", VE.T_TXN.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_batchmst", VE.T_TXN.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = masterHelp.TblUpdt("t_txnamt", VE.T_TXN.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_txntrans", VE.T_TXN.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_txn_linkno", VE.T_TXN.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_txndtl", VE.T_TXN.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_txn", VE.T_TXN.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = masterHelp.TblUpdt("t_cntrl_doc_pass", VE.T_TXN.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc_dtl", VE.T_TXN.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc", VE.T_TXN.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_cntrl_hdr_rem", VE.T_TXN.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(VE.T_TXN.AUTONO, "D", "S", null, null, null, VE.T_TXN.DOCDT.retStr(), null, null, null);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();


                        ModelState.Clear();
                        transaction.Commit();
                        OraTrans.Commit();
                        OraCon.Dispose();
                        return Content("3");
                    }
                    else
                    {
                        return Content("");
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    OraTrans.Rollback();
                    OraCon.Dispose();
                    return Content(ex.Message + ex.InnerException);
                }
            }
        }
        private string TranBarcodeGenerate(string doccd, string docbarcode, string UNIQNO, int slno)
        {//YRCODE	2,lbatchini	2,DOCCD	2,TXN UNIQ NO	7,SLNO	4
            var yrcd = CommVar.YearCode(UNQSNO).Substring(2, 2); string lbatchini = "";
            string sql = "select lbatchini from " + CommVar.CurSchema(UNQSNO) + ".m_loca where loccd='" + CommVar.Loccd(UNQSNO) + "' and compcd='" + CommVar.Compcd(UNQSNO) + "'";
            DataTable dt = masterHelp.SQLquery(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                lbatchini = dt.Rows[0]["dt"].retStr();
            }
            return yrcd + lbatchini + doccd + UNIQNO + slno.ToString().PadLeft(4, '0');
        }
        private string CommonBarcodeGenerate(string itgrpcd, string itcd, string mtrlbarcode, string prtbarcode, string clrbarcode, string szbarcode)
        {
            //itgrpcd last 3  3
            //itcd last 7  7
            //mtrljobcd mtrlbarcode 1
            //partcode prtbarcode  1
            //color clrbarcode  4
            //size szbarcode   3
        
            return itgrpcd + itcd + mtrlbarcode + prtbarcode + clrbarcode+ szbarcode;
        }
    }
}