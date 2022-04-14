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
using System.Reflection;
using System.Text.RegularExpressions;

namespace Improvar.Controllers
{
    public class T_OUTISSPROCESSController : Controller
    {
        // GET: T_OUTISSPROCESS
        Connection Cn = new Connection(); MasterHelp masterHelp = new MasterHelp(); SchemeCal Scheme_Cal = new SchemeCal(); Salesfunc salesfunc = new Salesfunc(); DataTable DT = new DataTable(); DataTable DTNEW = new DataTable();
        EmailControl EmailControl = new EmailControl();
        T_TXN TXN; T_TXNTRANS TXNTRN; T_TXNOTH TXNOTH; T_CNTRL_HDR TCH; T_CNTRL_HDR_REM SLR;
        SMS SMS = new SMS();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: T_OUTISSPROCESS
        public ActionResult T_OUTISSPROCESS(string op = "", string key = "", int Nindex = 0, string searchValue = "", string parkID = "", string ThirdParty = "no")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    TransactionOutIssProcess VE = (parkID == "") ? new TransactionOutIssProcess() : (Improvar.ViewModels.TransactionOutIssProcess)Session[parkID];
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    switch (VE.MENU_PARA)
                    {
                        case "DY":
                            ViewBag.formname = "Issue for Dyer"; break;
                        case "PR":
                            ViewBag.formname = "Issue for Printing"; break;
                        case "ST":
                            ViewBag.formname = "Issue for Stiching"; break;
                        case "EM":
                            ViewBag.formname = "Issue for Embroidery"; break;
                        case "JW":
                            ViewBag.formname = "Issue for Other Jobs"; break;
                        case "IR":
                            ViewBag.formname = "Issue for Iron"; break;
                        default: ViewBag.formname = ""; break;
                    }
                    string LOC = CommVar.Loccd(UNQSNO);
                    string COM = CommVar.Compcd(UNQSNO);
                    string YR1 = CommVar.YearCode(UNQSNO);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    VE.DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
                    //return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
                    VE.Database_Combo1 = (from n in DB.T_TXNOTH
                                          select new Database_Combo1() { FIELD_VALUE = n.SELBY }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();

                    VE.Database_Combo2 = (from n in DB.T_TXNOTH
                                          select new Database_Combo2() { FIELD_VALUE = n.DEALBY }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();
                    VE.HSN_CODE = (from n in DBF.M_HSNCODE
                                   select new HSN_CODE() { text = n.HSNDESCN, value = n.HSNCODE }).OrderBy(s => s.text).Distinct().ToList();
                    VE.BARGEN_TYPE = masterHelp.BARGEN_TYPE();
                    VE.DropDown_list1 = (from n in DBF.M_PRCLST
                                         select new DropDown_list1() { text = n.PRCCD, value = n.PRCNM }).OrderBy(s => s.text).Distinct().ToList();

                    //VE.DropDown_list_MTRLJOBCD = (from i in DB.M_MTRLJOBMST select new DropDown_list_MTRLJOBCD() { MTRLJOBCD = i.MTRLJOBCD, MTRLJOBNM = i.MTRLJOBNM }).OrderBy(s => s.MTRLJOBNM).ToList();
                    VE.DropDown_list_MTRLJOBCD = masterHelp.MTRLJOBCD_List();
                    VE.DropDown_list_StkType = masterHelp.STK_TYPE();
                    VE.DISC_TYPE = masterHelp.DISC_TYPE();

                    foreach (var v in VE.DropDown_list_MTRLJOBCD)
                    {
                        if (VE.MENU_PARA == "DY")
                        {
                            if (v.MTRLJOBCD == "PL")
                            {
                                v.Checked = true;
                            }
                        }
                        else {
                            if (v.MTRLJOBCD == "FS")
                            {
                                v.Checked = true;
                            }
                            if (v.MTRLJOBCD == "DY")
                            {
                                v.Checked = true;
                            }
                        }

                    }
                    //VE.DropDown_list_StkType = (from n in DB.M_STKTYPE
                    //                            select new DropDown_list_StkType() { value = n.STKTYPE, text = n.STKNAME }).OrderBy(s => s.value).Distinct().ToList();
                    VE.DISC_TYPE = masterHelp.DISC_TYPE();
                    VE.TDDISC_TYPE = (from n in DB.T_BATCHDTL
                                      select new TDDISC_TYPE() { FIELD_VALUE = n.TDDISCTYPE }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();
                    VE.SCMDISC_TYPE = (from n in DB.T_BATCHDTL
                                       select new SCMDISC_TYPE() { FIELD_VALUE = n.SCMDISCTYPE }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();
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
                                       //join s in DB.T_PROGMAST on p.AUTONO equals (s.AUTONO)
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
                                if (VE.MENU_PARA == "JW")
                                {
                                    string doccd = VE.DocumentType.FirstOrDefault().value;
                                    var jobcd = (from i in DB.T_TXN
                                                 join j in DB.M_JOBMST on i.JOBCD equals j.JOBCD
                                                 orderby i.AUTONO descending
                                                 where i.DOCCD == doccd
                                                 select new { JOBCD = i.JOBCD, JOBNM = j.JOBNM }).FirstOrDefault();
                                    if (jobcd != null) { TTXN.JOBCD = jobcd.JOBCD; VE.JOBNM = jobcd.JOBNM; }
                                }
                                else
                                {
                                    var jobcd = (from i in DB.M_JOBMST where i.JOBCD == VE.MENU_PARA select new { JOBCD = i.JOBCD, JOBNM = i.JOBNM }).OrderBy(s => s.JOBNM).FirstOrDefault();
                                    if (jobcd != null) { TTXN.JOBCD = jobcd.JOBCD; VE.JOBNM = jobcd.JOBNM; }
                                }

                                TTXN.GOCD = TempData["LASTGOCD" + VE.MENU_PARA].retStr();
                                TempData.Keep();
                                if (TTXN.GOCD.retStr() == "")
                                {
                                    if (VE.DocumentType.Count() > 0)
                                    {
                                        string doccd = VE.DocumentType.FirstOrDefault().value;
                                        TTXN.GOCD = DB.T_TXN.Where(a => a.DOCCD == doccd).OrderByDescending(a => a.AUTONO).Select(b => b.GOCD).FirstOrDefault();
                                    }
                                }
                                string gocd = TTXN.GOCD.retStr();

                                if (gocd != "")
                                {
                                    VE.GONM = DBF.M_GODOWN.Where(a => a.GOCD == gocd).Select(b => b.GONM).FirstOrDefault();
                                }

                                //TTXN.JOBCD = TempData["LASTJOBCD" + VE.MENU_PARA].retStr();
                                //TempData.Keep();
                                //if (TTXN.JOBCD.retStr() == "")
                                //{
                                //    if (VE.DocumentType.Count() > 0)
                                //    {
                                //        string doccd = VE.DocumentType.FirstOrDefault().value;
                                //        TTXN.JOBCD = DB.T_TXN.Where(a => a.DOCCD == doccd).OrderByDescending(a => a.AUTONO).Select(b => b.JOBCD).FirstOrDefault();
                                //    }
                                //}
                                //string jobcd1 = TTXN.JOBCD.retStr();

                                //if (jobcd1 != "")
                                //{
                                //    VE.JOBNM = DB.M_JOBMST.Where(a => a.JOBCD == jobcd1).Select(b => b.JOBNM).FirstOrDefault();
                                //}
                                VE.T_TXN = TTXN;

                                T_TXNOTH TXNOTH = new T_TXNOTH(); VE.T_TXNOTH = TXNOTH;

                                VE.RoundOff = true;
                                List<TPROGDTL> TPROGDTL = new List<TPROGDTL>();
                                for (int i = 0; i <= 9; i++)
                                {
                                    TPROGDTL PROGDTL = new TPROGDTL();
                                    PROGDTL.SLNO = Convert.ToByte(i + 1);
                                    //PROGDTL.MTRLJOBCD = TTXN.JOBCD;
                                    //PROGDTL.MTRLJOBNM = VE.JOBNM;
                                    TPROGDTL.Add(PROGDTL);
                                    VE.TPROGDTL = TPROGDTL;
                                }
                                VE.TPROGDTL = TPROGDTL;
                                List<TPROGBOM> TPROGBOM = new List<TPROGBOM>();
                                for (int i = 0; i <= 9; i++)
                                {
                                    TPROGBOM PROGBOM = new TPROGBOM();
                                    PROGBOM.SLNO = Convert.ToByte(i + 1);
                                    PROGBOM.RSLNO = Convert.ToByte(i + 1);
                                    TPROGBOM.Add(PROGBOM);
                                    VE.TPROGBOM = TPROGBOM;
                                }
                                VE.TPROGBOM = TPROGBOM;
                                List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                                UploadDOC UPL = new UploadDOC();
                                UPL.DocumentType = Cn.DOC_TYPE();
                                UploadDOC1.Add(UPL);
                                VE.UploadDOC = UploadDOC1;

                                //List<PACKING_SLIP> PACKING_SLIP = new List<PACKING_SLIP>();
                                //for (int i = 0; i < 10; i++)
                                //{
                                //    PACKING_SLIP PACKINGSLIP = new PACKING_SLIP();
                                //    PACKINGSLIP.SLNO = Convert.ToInt16(i + 1);
                                //    PACKING_SLIP.Add(PACKINGSLIP);
                                //}
                            }
                            else
                            {
                                if (VE.UploadDOC != null) { foreach (var i in VE.UploadDOC) { i.DocumentType = Cn.DOC_TYPE(); } }
                                INI INIF = new INI();
                                INIF.DeleteKey(Session["UR_ID"].ToString(), parkID, Server.MapPath("~/Park.ini"));
                            }
                            VE = (TransactionOutIssProcess)Cn.CheckPark(VE, VE.MENU_DETAILS, LOC, COM, CommVar.CurSchema(UNQSNO), Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString());

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
                TransactionOutIssProcess VE = new TransactionOutIssProcess();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message;
                return View(VE);
            }
        }
        public TransactionOutIssProcess Navigation(TransactionOutIssProcess VE, ImprovarDB DB, int index, string searchValue)
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
                //if (TXN.ROYN == "Y") VE.RoundOff = true;
                //else VE.RoundOff = false;
                TXNTRN = DB.T_TXNTRANS.Find(TXN.AUTONO);
                TXNOTH = DB.T_TXNOTH.Find(TXN.AUTONO);
                if (TXN.SLCD.retStr() != "")
                {
                    string slcd = TXN.SLCD;
                    var subleg = (from a in DBF.M_SUBLEG where a.SLCD == slcd select new { a.SLNM, a.SLAREA, a.DISTRICT, a.GSTNO }).FirstOrDefault();
                    VE.SLNM = subleg.SLNM;
                    VE.DISTRICT = subleg.DISTRICT;
                    VE.GSTNO = subleg.GSTNO;
                }
                VE.JOBNM = TXN.JOBCD.retStr() == "" ? "" : DB.M_JOBMST.Where(a => a.JOBCD == TXN.JOBCD).Select(b => b.JOBNM).FirstOrDefault();
                VE.GONM = TXN.GOCD.retStr() == "" ? "" : DBF.M_GODOWN.Where(a => a.GOCD == TXN.GOCD).Select(b => b.GONM).FirstOrDefault();
                //VE.CONSLNM = TXN.CONSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXN.CONSLCD).Select(b => b.SLNM).FirstOrDefault();
                //VE.AGSLNM = TXNOTH.AGSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXNOTH.AGSLCD).Select(b => b.SLNM).FirstOrDefault();
                //VE.SAGSLNM = TXNOTH.SAGSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXNOTH.SAGSLCD).Select(b => b.SLNM).FirstOrDefault();

                VE.PRCNM = TXNOTH.PRCCD.retStr() == "" ? "" : DBF.M_PRCLST.Where(a => a.PRCCD == TXNOTH.PRCCD).Select(b => b.PRCNM).FirstOrDefault();

                VE.TRANSLNM = TXNTRN.TRANSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXNTRN.TRANSLCD).Select(b => b.SLNM).FirstOrDefault();
                SLR = Cn.GetTransactionReamrks(CommVar.CurSchema(UNQSNO).ToString(), TXN.AUTONO);
                VE.UploadDOC = Cn.GetUploadImageTransaction(CommVar.CurSchema(UNQSNO).ToString(), TXN.AUTONO);
                string Scm = CommVar.CurSchema(UNQSNO); double TOTAL_NOS = 0; double TOTAL_QNTY = 0; double TOTAL_BOMQNTY = 0; double TOTAL_EXTRAQNTY = 0; double TOTAL_QQNTY = 0;
                string scmf = CommVar.FinSchema(UNQSNO);
                string str = "";
                str += "select a.autono,a.slno,a.nos,a.qnty,a.itcd,a.sizecd,a.partcd,a.colrcd,a.mtrljobcd,k.itgrpcd,k.uomcd,k.styleno,itgrpnm,k.itnm,l.sizenm,l.szbarcode,m.colrnm,m.clrbarcode,p.partnm,o.mtrljobnm, ";
                str += "a.itremark,a.shade,a.cutlength,a.sample, k.styleno||' '||k.itnm itstyle,a.barno,q.decimals from " + Scm + ".T_PROGMAST a," + Scm + ".T_PROGDTL b ,";
                str += Scm + ".M_SITEM k, " + Scm + ".M_SIZE l, " + Scm + ".M_COLOR m, ";
                str += Scm + ".M_GROUP n," + Scm + ".M_MTRLJOBMST o," + Scm + ".M_PARTS p, " + scmf + ".m_uom q ";
                str += " where a.autono=b.autono(+) and a.slno=b.slno(+) and a.ITCD = k.ITCD(+) ";
                str += " and a.SIZECD = l.SIZECD(+) and a.COLRCD = m.COLRCD(+) and k.ITGRPCD=n.ITGRPCD(+) and k.uomcd=q.uomcd(+) and ";
                str += " a.MTRLJOBCD=o.MTRLJOBCD(+) and a.PARTCD=p.PARTCD(+) and a.autono='" + TXN.AUTONO + "'";
                str += "order by a.slno ";

                DataTable Progdtltbl = masterHelp.SQLquery(str);
                VE.TPROGDTL = (from DataRow dr in Progdtltbl.Rows
                               select new TPROGDTL()
                               {
                                   SLNO = Convert.ToInt16(dr["slno"]),
                                   NOS = dr["nos"].retDbl(),
                                   QNTY = dr["qnty"].retDbl(),
                                   ITGRPCD = dr["itgrpcd"].retStr(),
                                   ITGRPNM = dr["itgrpnm"].retStr(),
                                   ITCD = dr["itcd"].retStr(),
                                   ITNM = dr["itstyle"].retStr(),
                                   UOM = dr["uomcd"].retStr(),
                                   SIZECD = dr["sizecd"].retStr(),
                                   SIZENM = dr["sizenm"].retStr(),
                                   PARTCD = dr["partcd"].retStr(),
                                   PARTNM = dr["partnm"].retStr(),
                                   COLRCD = dr["colrcd"].retStr(),
                                   COLRNM = dr["colrnm"].retStr(),
                                   MTRLJOBCD = dr["mtrljobcd"].retStr(),
                                   MTRLJOBNM = dr["mtrljobnm"].retStr(),
                                   ITREMARK = dr["itremark"].retStr(),
                                   SHADE = dr["shade"].retStr(),
                                   CUTLENGTH = dr["cutlength"].retDbl(),
                                   SAMPLE = dr["sample"].retStr(),
                                   BARNO = dr["barno"].retStr(),
                                   DECIMALS = dr["decimals"].retShort(),
                                   CLRBARCODE = dr["CLRBARCODE"].retStr(),
                                   SZBARCODE = dr["SZBARCODE"].retStr(),

                               }).OrderBy(s => s.SLNO).ToList();
                string BARNO = VE.TPROGDTL.Select(a => a.BARNO).ToArray().retSqlfromStrarray();
                string ITCD = VE.TPROGDTL.Select(a => a.ITCD).ToArray().retSqlfromStrarray();
                string ITGRPCD = VE.TPROGDTL.Select(a => a.ITGRPCD).ToArray().retSqlfromStrarray();
                string MTRLJOBCD = VE.TPROGDTL.Select(a => a.MTRLJOBCD).ToArray().retSqlfromStrarray();
                //var imgdata = salesfunc.GetStock(TXN.DOCDT.retStr().Remove(10), TXN.GOCD.retSqlformat(), BARNO.retStr(), ITCD.retStr(), "", TXN.AUTONO.retSqlformat(), ITGRPCD, "", TXNOTH.PRCCD.retStr(), TXNOTH.TAXGRPCD.retStr());
                var imgdata = salesfunc.GetBarHelp(TXN.DOCDT.retStr().Remove(10), TXN.GOCD.retSqlformat(), BARNO.retStr(), ITCD.retStr(), MTRLJOBCD.retStr(), TXN.AUTONO.retSqlformat(), ITGRPCD, "", TXNOTH.PRCCD.retStr(), TXNOTH.TAXGRPCD.retStr());

                foreach (var q in VE.TPROGDTL)
                {
                    if (q.SAMPLE == "Y") q.CheckedSample = true;
                    TOTAL_NOS = TOTAL_NOS + (q.NOS == null ? 0 : q.NOS.Value);
                    TOTAL_QNTY = TOTAL_QNTY + (q.QNTY == null ? 0 : q.QNTY.Value);

                    BARNO = q.BARNO;
                    ITCD = q.ITCD;
                    ITGRPCD = q.ITGRPCD;
                    var img = imgdata.AsEnumerable().Where(a => a.Field<string>("BARNO") == BARNO && a.Field<string>("ITCD") == ITCD && a.Field<string>("ITGRPCD") == ITGRPCD).Select(b => new
                    {
                        BARIMAGE = b.Field<string>("BARIMAGE"),
                        barimagecount = b.Field<decimal?>("barimagecount"),
                    }).FirstOrDefault();
                    if (img != null)
                    {
                        q.BarImages = img.BARIMAGE;
                        q.BarImagesCount = img.barimagecount.retStr();
                    }
                }
                VE.P_T_NOS = TOTAL_NOS.retDbl();
                VE.P_T_QNTY = TOTAL_QNTY.retDbl();


                string str2 = "";
                str2 += "select a.autono,a.slno,a.rslno,a.qnty,a.bomqnty,a.extraqnty,a.itcd,a.sizecd,a.partcd,a.colrcd,a.mtrljobcd,k1.itgrpcd,n.itgrpnm, ";
                str2 += " k.itnm Qitnm,l.sizenm,m.colrnm,p.partnm,o.mtrljobnm,k.uomcd Quomcd,b.qnty qntyMst, ";
                str2 += "a.sample,k.styleno ||' '||k.itnm Qitstyle,k1.styleno||' '||k1.itnm itstyle,k1.uomcd ,b.barno from " + Scm + ".T_PROGBOM a," + Scm + ".T_PROGMAST b ,";
                str2 += Scm + ".M_SITEM k, " + Scm + ".M_SITEM k1, " + Scm + ".M_SIZE l, " + Scm + ".M_COLOR m, ";
                str2 += Scm + ".M_GROUP n," + Scm + ".M_MTRLJOBMST o," + Scm + ".M_PARTS p ";
                str2 += " where a.autono=b.autono(+) and a.slno=b.slno(+) and a.ITCD = k1.ITCD(+) and b.ITCD = k.ITCD(+)  ";
                str2 += " and a.SIZECD = l.SIZECD(+) and a.COLRCD = m.COLRCD(+) and k1.ITGRPCD=n.ITGRPCD(+) and ";
                str2 += " a.MTRLJOBCD=o.MTRLJOBCD(+) and a.PARTCD=p.PARTCD(+) and a.autono='" + TXN.AUTONO + "'";
                str2 += "order by a.slno ";

                DataTable Progbom = masterHelp.SQLquery(str2);
                VE.TPROGBOM = (from DataRow dr in Progbom.Rows
                               select new TPROGBOM()
                               {
                                   SLNO = Convert.ToInt16(dr["slno"]),
                                   RSLNO = Convert.ToInt16(dr["rslno"]),
                                   QQNTY = dr["qntyMst"].retDbl(),
                                   QNTY = dr["qnty"].retDbl(),
                                   QITNM = dr["qitstyle"].retStr(),
                                   QUOM = dr["Quomcd"].retStr(),
                                   BARNO = dr["barno"].retStr(),
                                   ITGRPCD = dr["itgrpcd"].retStr(),
                                   ITGRPNM = dr["itgrpnm"].retStr(),
                                   ITCD = dr["itcd"].retStr(),
                                   ITNM = dr["itstyle"].retStr(),
                                   SIZECD = dr["sizecd"].retStr(),
                                   SIZENM = dr["sizenm"].retStr(),
                                   PARTCD = dr["partcd"].retStr(),
                                   PARTNM = dr["partnm"].retStr(),
                                   COLRCD = dr["colrcd"].retStr(),
                                   COLRNM = dr["colrnm"].retStr(),
                                   MTRLJOBCD = dr["mtrljobcd"].retStr(),
                                   MTRLJOBNM = dr["mtrljobnm"].retStr(),
                                   UOM = dr["uomcd"].retStr(),
                                   BOMQNTY = dr["bomqnty"].retDbl(),
                                   EXTRAQNTY = dr["extraqnty"].retDbl(),
                                   Q_SAMPLE = dr["sample"].retStr()
                               }).OrderBy(s => s.SLNO).ToList();
                foreach (var q in VE.TPROGBOM)
                {
                    if (q.Q_SAMPLE == "Y") q.Q_CheckedSample = true;
                    TOTAL_QQNTY = TOTAL_QQNTY + (q.QQNTY == null ? 0 : q.QQNTY.Value);
                    TOTAL_BOMQNTY = TOTAL_BOMQNTY + (q.BOMQNTY == null ? 0 : q.BOMQNTY.Value);
                    TOTAL_EXTRAQNTY = TOTAL_EXTRAQNTY + (q.EXTRAQNTY == null ? 0 : q.EXTRAQNTY.Value);
                    //if (q.Q_SAMPLE == "Y")
                    //{
                    //    q.PROGSLNO = VE.TPROGDTL.Where(a => a.BARNO.retStr() == q.BARNO.retStr() && a.MTRLJOBCD.retStr() == q.MTRLJOBCD.retStr() && a.QNTY.retStr() == q.QQNTY.retStr()).Select(b => b.SLNO).SingleOrDefault();
                    //}
                    //else
                    //{
                    //    q.PROGSLNO = VE.TPROGDTL.Where(a => a.BARNO.retStr() == q.BARNO.retStr() && a.QNTY.retStr() == q.QQNTY.retStr()).Select(b => b.SLNO).SingleOrDefault();
                    //}
                }
                VE.T_QQNTY = TOTAL_QQNTY.retDbl();
                VE.T_BOMQNTY = TOTAL_BOMQNTY.retDbl();
                VE.T_EXTRAQNTY = TOTAL_EXTRAQNTY.retDbl();

                #region batch and detail data
                string str1 = "";
                str1 += "select i.SLNO,i.TXNSLNO,k.ITGRPCD,n.ITGRPNM,n.BARGENTYPE,i.MTRLJOBCD,o.MTRLJOBNM,o.MTBARCODE,k.ITCD,k.ITNM,k.UOMCD,k.STYLENO,i.PARTCD,p.PARTNM,p.PRTBARCODE,i.STKTYPE,q.STKNAME,i.BARNO, ";
                str1 += "j.COLRCD,m.COLRNM,m.CLRBARCODE,j.SIZECD,l.SIZENM,l.SZBARCODE,i.SHADE,i.QNTY,i.NOS,i.RATE,i.DISCRATE,i.DISCTYPE,i.TDDISCRATE,i.TDDISCTYPE,i.SCMDISCTYPE,i.SCMDISCRATE,i.HSNCODE,i.BALENO,j.PDESIGN,j.OURDESIGN,i.FLAGMTR,i.LOCABIN,i.BALEYR ";
                str1 += ",n.SALGLCD,n.PURGLCD,n.SALRETGLCD,n.PURRETGLCD,i.itrem,i.RECPROGSLNO,k.NEGSTOCK,i.cutlength ";
                str1 += "from " + Scm + ".T_BATCHDTL i, " + Scm + ".T_BATCHMST j, " + Scm + ".M_SITEM k, " + Scm + ".M_SIZE l, " + Scm + ".M_COLOR m, ";
                str1 += Scm + ".M_GROUP n," + Scm + ".M_MTRLJOBMST o," + Scm + ".M_PARTS p," + Scm + ".M_STKTYPE q ";
                str1 += "where i.BARNO = j.BARNO(+) and j.ITCD = k.ITCD(+) and j.SIZECD = l.SIZECD(+) and j.COLRCD = m.COLRCD(+) and k.ITGRPCD=n.ITGRPCD(+) ";
                str1 += "and i.MTRLJOBCD=o.MTRLJOBCD(+) and i.PARTCD=p.PARTCD(+) and i.STKTYPE=q.STKTYPE(+) ";
                str1 += "and i.AUTONO = '" + TXN.AUTONO + "' ";
                str1 += "order by i.SLNO ";
                DataTable tbl = masterHelp.SQLquery(str1);
                DataTable stock = new DataTable();

                VE.TBATCHDTL = (from DataRow dr in tbl.Rows
                                select new TBATCHDTL()
                                {
                                    SLNO = dr["SLNO"].retShort(),
                                    TXNSLNO = dr["TXNSLNO"].retShort(),
                                    ITGRPCD = dr["ITGRPCD"].retStr(),
                                    ITGRPNM = dr["ITGRPNM"].retStr(),
                                    MTRLJOBCD = dr["MTRLJOBCD"].retStr(),
                                    MTRLJOBNM = dr["MTRLJOBNM"].retStr(),
                                    MTBARCODE = dr["MTBARCODE"].retStr(),
                                    ITCD = dr["ITCD"].retStr(),
                                    ITSTYLE = dr["STYLENO"].retStr() + "" + dr["ITNM"].retStr(),
                                    UOM = dr["UOMCD"].retStr(),
                                    STYLENO = dr["STYLENO"].retStr(),
                                    PARTCD = dr["PARTCD"].retStr(),
                                    PARTNM = dr["PARTNM"].retStr(),
                                    PRTBARCODE = dr["PRTBARCODE"].retStr(),
                                    COLRCD = dr["COLRCD"].retStr(),
                                    COLRNM = dr["COLRNM"].retStr(),
                                    CLRBARCODE = dr["CLRBARCODE"].retStr(),
                                    SIZECD = dr["SIZECD"].retStr(),
                                    SIZENM = dr["SIZENM"].retStr(),
                                    SZBARCODE = dr["SZBARCODE"].retStr(),
                                    SHADE = dr["SHADE"].retStr(),
                                    QNTY = dr["QNTY"].retDbl(),
                                    NOS = dr["NOS"].retDbl(),
                                    RATE = dr["RATE"].retDbl(),
                                    DISCRATE = dr["DISCRATE"].retDbl(),
                                    DISCTYPE = dr["DISCTYPE"].retStr(),
                                    DISCTYPE_DESC = dr["DISCTYPE"].retStr() == "P" ? "%" : dr["DISCTYPE"].retStr() == "N" ? "Nos" : dr["DISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
                                    TDDISCRATE = dr["TDDISCRATE"].retDbl(),
                                    TDDISCTYPE_DESC = dr["TDDISCTYPE"].retStr() == "P" ? "%" : dr["TDDISCTYPE"].retStr() == "N" ? "Nos" : dr["TDDISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
                                    TDDISCTYPE = dr["TDDISCTYPE"].retStr(),
                                    SCMDISCTYPE_DESC = dr["SCMDISCTYPE"].retStr() == "P" ? "%" : dr["SCMDISCTYPE"].retStr() == "N" ? "Nos" : dr["SCMDISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
                                    SCMDISCTYPE = dr["SCMDISCTYPE"].retStr(),
                                    SCMDISCRATE = dr["SCMDISCRATE"].retDbl(),
                                    STKTYPE = dr["STKTYPE"].retStr(),
                                    STKNAME = dr["STKNAME"].retStr(),
                                    BARNO = dr["BARNO"].retStr(),
                                    HSNCODE = dr["HSNCODE"].retStr(),
                                    BALENO = dr["BALENO"].retStr(),
                                    PDESIGN = dr["PDESIGN"].retStr(),
                                    OURDESIGN = dr["OURDESIGN"].retStr(),
                                    FLAGMTR = dr["FLAGMTR"].retDbl(),
                                    LOCABIN = dr["LOCABIN"].retStr(),
                                    BALEYR = dr["BALEYR"].retStr(),
                                    BARGENTYPE = dr["BARGENTYPE"].retStr(),
                                    ITREM = dr["itrem"].retStr(),
                                    RECPROGSLNO = dr["RECPROGSLNO"].retShort(),
                                    //GLCD = VE.MENU_PARA == "SBPCK" ? dr["SALGLCD"].retStr() : VE.MENU_PARA == "SB" ? dr["SALGLCD"].retStr() : VE.MENU_PARA == "SBDIR" ? dr["SALGLCD"].retStr() : VE.MENU_PARA == "SR" ? dr["SALRETGLCD"].retStr() : VE.MENU_PARA == "SBCM" ? dr["SALGLCD"].retStr() : VE.MENU_PARA == "SBCMR" ? dr["SALGLCD"].retStr() : VE.MENU_PARA == "SBEXP" ? dr["SALGLCD"].retStr() : VE.MENU_PARA == "PI" ? "" : VE.MENU_PARA == "PB" ? dr["PURGLCD"].retStr() : VE.MENU_PARA == "PR" ? dr["PURRETGLCD"].retStr() : "",
                                    NEGSTOCK = dr["NEGSTOCK"].retStr(),
                                    CUTLENGTH = dr["cutlength"].retDbl(),
                                }).OrderBy(s => s.SLNO).ToList();

                //foreach (var v in VE.TBATCHDTL)
                //{
                //    stock = salesfunc.GetStock(TXN.DOCDT.retDateStr(), TXN.GOCD.retSqlformat(), "", "", v.MTRLJOBCD.retSqlformat(), TXN.AUTONO.retSqlformat(), "", v.BARNO, "", "", "", "", true, false);
                //    if (stock != null)
                //    {
                //        var balstock = (from DataRow dr in stock.Rows
                //                        select new
                //                        { balstock = dr["BALQNTY"].retStr() }).FirstOrDefault();
                //        v.BALSTOCK = balstock.balstock.retDbl();
                //    }

                //}


                str1 = "";
                str1 += "select i.SLNO,j.ITGRPCD,k.ITGRPNM,i.MTRLJOBCD,l.MTRLJOBNM,l.MTBARCODE,i.ITCD,j.ITNM,j.STYLENO,j.UOMCD,i.STKTYPE,m.STKNAME,i.NOS,i.QNTY,i.FLAGMTR, ";
                str1 += "i.BLQNTY,i.RATE,i.AMT,i.DISCTYPE,i.DISCRATE,i.DISCAMT,i.TDDISCTYPE,i.TDDISCRATE,i.TDDISCAMT,i.SCMDISCTYPE,i.SCMDISCRATE,i.SCMDISCAMT,i.ITREM,  ";
                str1 += "i.TXBLVAL,i.IGSTPER,i.CGSTPER,i.SGSTPER,i.CESSPER,i.IGSTAMT,i.CGSTAMT,i.SGSTAMT,i.CESSAMT,i.NETAMT,i.HSNCODE,i.BALENO,i.GLCD,i.BALEYR,i.TOTDISCAMT ";
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
                                  //MTBARCODE = dr["MTBARCODE"].retStr(),
                                  ITCD = dr["ITCD"].retStr(),
                                  ITSTYLE = dr["STYLENO"].retStr() + " " + dr["ITNM"].retStr(),
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
                                  TDDISCTYPE_DESC = dr["TDDISCTYPE"].retStr() == "P" ? "%" : dr["TDDISCTYPE"].retStr() == "N" ? "Nos" : dr["TDDISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
                                  TDDISCTYPE = dr["TDDISCTYPE"].retStr(),
                                  TDDISCRATE = dr["TDDISCRATE"].retDbl(),
                                  TDDISCAMT = dr["TDDISCAMT"].retDbl(),
                                  SCMDISCTYPE_DESC = dr["SCMDISCTYPE"].retStr() == "P" ? "%" : dr["SCMDISCTYPE"].retStr() == "N" ? "Nos" : dr["SCMDISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
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
                                  GLCD = dr["GLCD"].retStr(),
                                  BALEYR = dr["BALEYR"].retStr(),
                                  TOTDISCAMT = dr["TOTDISCAMT"].retDbl(),
                                  ITREM = dr["ITREM"].retStr()
                              }).OrderBy(s => s.SLNO).ToList();

                VE.B_T_QNTY = VE.TBATCHDTL.Sum(a => a.QNTY).retDbl();
                VE.B_T_NOS = VE.TBATCHDTL.Sum(a => a.NOS).retDbl();
                VE.T_NOS = VE.TTXNDTL.Sum(a => a.NOS).retDbl();
                VE.T_QNTY = VE.TTXNDTL.Sum(a => a.QNTY).retDbl();
                VE.T_AMT = VE.TTXNDTL.Sum(a => a.AMT).retDbl();
                VE.T_GROSS_AMT = VE.TTXNDTL.Sum(a => a.TXBLVAL).retDbl();
                VE.T_IGST_AMT = VE.TTXNDTL.Sum(a => a.IGSTAMT).retDbl();
                VE.T_CGST_AMT = VE.TTXNDTL.Sum(a => a.CGSTAMT).retDbl();
                VE.T_SGST_AMT = VE.TTXNDTL.Sum(a => a.SGSTAMT).retDbl();
                VE.T_CESS_AMT = VE.TTXNDTL.Sum(a => a.CESSAMT).retDbl();
                VE.T_NET_AMT = VE.TTXNDTL.Sum(a => a.NETAMT).retDbl();

                //fill prodgrpgstper in t_batchdtl
                DataTable allprodgrpgstper_data = new DataTable();
                BARNO = (from a in VE.TBATCHDTL select a.BARNO).Distinct().ToArray().retSqlfromStrarray();
                ITCD = (from a in VE.TBATCHDTL select a.ITCD).Distinct().ToArray().retSqlfromStrarray();
                MTRLJOBCD = (from a in VE.TBATCHDTL select a.MTRLJOBCD).Distinct().ToArray().retSqlfromStrarray();
                ITGRPCD = (from a in VE.TBATCHDTL select a.ITGRPCD).Distinct().ToArray().retSqlfromStrarray();

                //allprodgrpgstper_data = salesfunc.GetStock(TXN.DOCDT.retStr().Remove(10), TXN.GOCD.retStr(), BARNO.retStr(), ITCD.retStr(), "", "", ITGRPCD, "", TXNOTH.PRCCD.retStr(), TXNOTH.TAXGRPCD.retStr());
                allprodgrpgstper_data = salesfunc.GetStock(TXN.DOCDT.retStr().Remove(10), TXN.GOCD.retSqlformat(), BARNO.retStr(), ITCD.retStr(), MTRLJOBCD, TXN.AUTONO.retSqlformat(), ITGRPCD, "", TXNOTH.PRCCD.retStr(), TXNOTH.TAXGRPCD.retStr());
                var data = (from x in VE.TPROGBOM
                            where x.ITCD != null
                            group x by new
                            {
                                x.ITCD,
                                x.ITNM,
                                x.UOM
                            } into P
                            select new
                            {
                                ITCD = P.Key.ITCD.retStr(),
                                ITNM = P.Key.ITNM.retStr(),
                                UOM = P.Key.UOM.retStr(),
                                TOTALREQQTY = (P.Sum(A => A.BOMQNTY.retDbl()) + P.Sum(A => A.EXTRAQNTY.retDbl())).retDbl(),
                                USEDQTY = (P.Sum(A => A.BOMQNTY.retDbl()) + P.Sum(A => A.EXTRAQNTY.retDbl())).retDbl(),
                                QNTY = (P.Sum(A => A.BOMQNTY.retDbl()) + P.Sum(A => A.EXTRAQNTY.retDbl())).retDbl()
                            }).ToList();
                foreach (var v in VE.TBATCHDTL)
                {
                    var progdata = (from a in VE.TPROGDTL where a.SLNO == v.RECPROGSLNO select new { a.ITCD, a.ITNM }).FirstOrDefault();
                    if (progdata != null)
                    {
                        v.RECPROGITCD = progdata.ITCD;
                        v.RECPROGITSTYLE = progdata.ITNM;
                    }
                    string PRODGRPGSTPER = "", ALL_GSTPER = "", GSTPER = "";
                    v.GSTPER = VE.TTXNDTL.Where(a => a.SLNO == v.TXNSLNO).Sum(b => b.IGSTPER + b.CGSTPER + b.SGSTPER).retDbl();
                    if (allprodgrpgstper_data != null && allprodgrpgstper_data.Rows.Count > 0)
                    {
                        //var q = (from DataRow dr in allprodgrpgstper_data.Rows
                        //         where dr["BARNO"].retStr() == v.BARNO
                        //         select new
                        //         { BALSTOCK = dr["BALQNTY"].retDbl() }).FirstOrDefault();
                        //if (q != null)
                        //{
                        //    v.BALSTOCK = q.BALSTOCK;
                        //}
                        var DATA = allprodgrpgstper_data.Select("barno = '" + v.BARNO + "' and itcd= '" + v.ITCD + "' and itgrpcd = '" + v.ITGRPCD + "'");
                        if (DATA.Count() > 0)
                        {
                            DataTable tax_data = DATA.CopyToDataTable();
                            if (tax_data != null && tax_data.Rows.Count > 0)
                            {
                                v.BALSTOCK = tax_data.Rows[0]["BALQNTY"].retDbl();
                                PRODGRPGSTPER = tax_data.Rows[0]["PRODGRPGSTPER"].retStr();
                                if (PRODGRPGSTPER != "")
                                {
                                    ALL_GSTPER = salesfunc.retGstPer(PRODGRPGSTPER, v.RATE.retDbl());
                                    if (ALL_GSTPER.retStr() != "")
                                    {
                                        var gst = ALL_GSTPER.Split(',').ToList();
                                        GSTPER = (from a in gst select a.retDbl()).Sum().retStr();
                                    }
                                    v.PRODGRPGSTPER = PRODGRPGSTPER;
                                    v.ALL_GSTPER = ALL_GSTPER;
                                    v.GSTPER = GSTPER.retDbl();
                                }
                                if (tax_data.Rows[0]["barimage"].retStr() != "")
                                {
                                    v.BarImages = tax_data.Rows[0]["barimage"].retStr();
                                    var brimgs = v.BarImages.retStr().Split((char)179);
                                    v.BarImagesCount = brimgs.Length == 0 ? "" : brimgs.Length.retStr();
                                    foreach (var barimg in brimgs)
                                    {
                                        string barfilename = barimg.Split('~')[0];
                                        string FROMpath = CommVar.SaveFolderPath() + "/ItemImages/" + barfilename;
                                        FROMpath = Path.Combine(FROMpath, "");
                                        string TOPATH = System.Web.Hosting.HostingEnvironment.MapPath("/UploadDocuments/" + barfilename);
                                        Cn.CopyImage(FROMpath, TOPATH);
                                    }
                                }
                            }
                        }
                    }

                    //checking childdata exist against barno
                    var chk_child = (from a in DB.T_BATCHDTL where a.BARNO == v.BARNO && a.AUTONO != TXN.AUTONO select a).ToList();
                    if (chk_child.Count() > 0)
                    {
                        v.ChildData = "Y";
                    }
                    //fill bom qnty
                    double currentadjqnty = 0;
                    string itcd = "";
                    if (VE.TBATCHDTL != null)
                    {
                        itcd = v.ITCD.retStr();
                        currentadjqnty = VE.TBATCHDTL.Where(a => a.ITCD == itcd).Sum(b => b.QNTY).retDbl();
                    }
                    var TOTALREQQTY = data.Where(a => a.ITCD == itcd).Sum(b => b.TOTALREQQTY).retDbl();
                    v.BOMQNTY = (TOTALREQQTY.retDbl() - currentadjqnty.retDbl()) + "~~" + TOTALREQQTY.retDbl();
                }
                //fill prodgrpgstper in t_txndtl
                foreach (var v in VE.TTXNDTL)
                {
                    string PRODGRPGSTPER = "", ALL_GSTPER = "";
                    var tax_data = (from a in VE.TBATCHDTL
                                    where a.TXNSLNO == v.SLNO && a.ITGRPCD == v.ITGRPCD && a.ITCD == a.ITCD && a.STKTYPE == v.STKTYPE
                                    && a.RATE == v.RATE && a.DISCTYPE == v.DISCTYPE && a.DISCRATE == v.DISCRATE && a.TDDISCTYPE == v.TDDISCTYPE
                                     && a.TDDISCRATE == v.TDDISCRATE && a.SCMDISCTYPE == v.SCMDISCTYPE && a.SCMDISCRATE == v.SCMDISCRATE
                                    select new { a.PRODGRPGSTPER, a.ALL_GSTPER }).FirstOrDefault();
                    if (tax_data != null)
                    {
                        PRODGRPGSTPER = tax_data.PRODGRPGSTPER.retStr();
                        ALL_GSTPER = tax_data.ALL_GSTPER.retStr();
                    }
                    v.PRODGRPGSTPER = PRODGRPGSTPER;
                    v.ALL_GSTPER = ALL_GSTPER;
                }
                #endregion




            }
            //Cn.DateLock_Entry(VE, DB, TCH.DOCDT.Value);
            if (TCH.CANCEL == "Y") VE.CancelRecord = true; else VE.CancelRecord = false;
            return VE;
        }
        public ActionResult SearchPannelData(TransactionOutIssProcess VE, string SRC_SLCD, string SRC_DOCNO, string SRC_FDT, string SRC_TDT)
        {
            string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), yrcd = CommVar.YearCode(UNQSNO);
            Cn.getQueryString(VE);

            List<DocumentType> DocumentType = new List<DocumentType>();
            DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
            string doccd = DocumentType.Select(i => i.value).ToArray().retSqlfromStrarray();
            string sql = "";

            sql = "select a.autono, b.docno, to_char(b.docdt,'dd/mm/yyyy') docdt, b.doccd, a.slcd, c.slnm, c.district,c.gstno,a.jobcd,d.jobnm, nvl(a.blamt,0) blamt ";
            sql += "from " + scm + ".t_txn a, " + scm + ".t_cntrl_hdr b, " + scmf + ".m_subleg c ," + scm + ".m_jobmst d ";
            sql += "where a.autono=b.autono and a.slcd=c.slcd(+) and a.jobcd=d.jobcd(+) and b.doccd in (" + doccd + ") and ";
            if (SRC_FDT.retStr() != "") sql += "b.docdt >= to_date('" + SRC_FDT.retDateStr() + "','dd/mm/yyyy') and ";
            if (SRC_TDT.retStr() != "") sql += "b.docdt <= to_date('" + SRC_TDT.retDateStr() + "','dd/mm/yyyy') and ";
            if (SRC_DOCNO.retStr() != "") sql += "(b.vchrno like '%" + SRC_DOCNO.retStr() + "%' or b.docno like '%" + SRC_DOCNO.retStr() + "%') and ";
            if (SRC_SLCD.retStr() != "") sql += "(a.slcd like '%" + SRC_SLCD.retStr() + "%' or upper(c.slnm) like '%" + SRC_SLCD.retStr().ToUpper() + "%') and ";
            sql += "b.loccd='" + LOC + "' and b.compcd='" + COM + "' and b.yr_cd='" + yrcd + "' ";
            sql += "order by docdt, docno ";
            DataTable tbl = masterHelp.SQLquery(sql);

            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Document Number" + Cn.GCS() + "Document Date" + Cn.GCS() + "Party Name" + Cn.GCS() + "Job Name" + Cn.GCS() + "AUTO NO";
            for (int j = 0; j <= tbl.Rows.Count - 1; j++)
            {
                SB.Append("<tr><td><b>" + tbl.Rows[j]["docno"] + "</b> [" + tbl.Rows[j]["doccd"] + "]" + " </td><td>" + tbl.Rows[j]["docdt"] + " </td><td><b>" + tbl.Rows[j]["slnm"] + "</b> [" + tbl.Rows[j]["district"] + "][" + tbl.Rows[j]["gstno"] + "] (" + tbl.Rows[j]["slcd"] + ") </td><td>" + tbl.Rows[j]["jobnm"] + "(" + tbl.Rows[j]["jobcd"] + ") </td><td>" + tbl.Rows[j]["autono"] + " </td></tr>");
            }
            return PartialView("_SearchPannel2", masterHelp.Generate_SearchPannel(hdr, SB.ToString(), "4", "4"));
        }
        public ActionResult GetBarCodeDetailsGrid(string val, string Code)
        {
            return BarCodeDetails(val, Code, "PB");
        }
        public ActionResult GetBarCodeDetails(string val, string Code)
        {
            return BarCodeDetails(val, Code, "SB");
        }
        public ActionResult GetBarCodePopUpDetails(string val, string Code)
        {
            return BarCodeDetails(val, Code, "OTH");
        }
        public ActionResult BarCodeDetails(string val, string Code, string menupara)
        {
            try
            {
                //sequence MTRLJOBCD/PARTCD/DOCDT/TAXGRPCD/GOCD/PRCCD/ALLMTRLJOBCD
                TransactionOutIssProcess VE = new TransactionOutIssProcess();
                Cn.getQueryString(VE);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                var data = Code.Split(Convert.ToChar(Cn.GCS()));
                string barnoOrStyle = val.retStr();
                string MTRLJOBCD = data[0].retSqlformat();
                string PARTCD = data[1].retStr();
                string DOCDT = data[2].retStr();
                string TAXGRPCD = data[3].retStr();
                string GOCD = data[2].retStr() == "" ? "" : data[4].retStr().retSqlformat();
                string PRCCD = data[5].retStr();
                string BOMITCD = "";
                if (GOCD == "")
                {
                    return Content("Please Select Godown !!");
                }
                if (menupara == "SB" && VE.MENU_PARA == "DY")
                {
                    BOMITCD = data[7].retStr() == "" ? "" : data[7].retStr().retSqlformat();
                    //if (BOMITCD == "") return Content("Please select Bom Item Code ");
                }

                //if (MTRLJOBCD == "" || barnoOrStyle == "") { MTRLJOBCD = data[6].retStr(); }
                string str = masterHelp.T_TXN_BARNO_help(barnoOrStyle, menupara, DOCDT, TAXGRPCD, GOCD, PRCCD, MTRLJOBCD, BOMITCD);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    if (str.IndexOf(Cn.GCS()) == -1) return Content(str);

                    string glcd = "", mtrljobcd = "", cd = "", mtrljobnm = "";
                    glcd = str.retCompValue("SALGLCD");
                    mtrljobcd = str.retCompValue("MTRLJOBCD");
                    if (mtrljobcd == "")
                    {
                        if (VE.MENU_PARA == "DY" && menupara == "PB")
                        {
                            cd = "DY";
                        }
                        else
                        {
                            cd = "FS";
                        }
                        mtrljobnm = (from i in DB.M_MTRLJOBMST where i.MTRLJOBCD == cd select i.MTRLJOBNM).FirstOrDefault();
                    }
                    else { cd = mtrljobcd; mtrljobnm = str.retCompValue("MTRLJOBNM"); }
                    str += "^GLCD=^" + glcd + Cn.GCS() + "^MTRLJOBCD_NEW=^" + cd + Cn.GCS() + "^MTRLJOBNM_NEW=^" + mtrljobnm + Cn.GCS();

                    #region rate from stock
                    if (menupara == "SB")
                    {
                        string barno = str.retCompValue("BARNO").retSqlformat();
                        string mtrljob = str.retCompValue("MTRLJOBCD").retStr() == "" ? "" : str.retCompValue("MTRLJOBCD").retSqlformat();
                        string oldrate = str.retCompValue("RATE");
                        var ratedata = salesfunc.GenStocktblwithVal("FIFO", "", barno, mtrljob, "", "", GOCD);
                        if (ratedata != null && ratedata.Rows.Count > 0)
                        {
                            double amt = ratedata.AsEnumerable().Select(a => a.Field<double>("amt")).Sum();
                            double qnty = ratedata.AsEnumerable().Select(a => a.Field<double>("qnty")).Sum();
                            if (qnty.retDbl() != 0)
                            {
                                double rate = (amt.retDbl() / qnty.retDbl()).toRound(2);
                                string oldrt = "^RATE=^" + oldrate + Cn.GCS();
                                string rewrt = "^RATE=^" + rate + Cn.GCS();
                                str = str.Replace(oldrt.retStr(), rewrt.retStr());
                            }

                        }
                    }
                    #endregion

                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetBomItemDetails(TransactionOutIssProcess VE, string val, string Code)
        {
            try
            {
                int bomcnt = VE.TPROGBOM.Where(a => a.ITCD != null && a.QITNM != null).Select(a => a.ITCD).Distinct().Count();
                List<TPROGDTL> data = new List<TPROGDTL>();
                if (bomcnt == 0)
                {
                    data = (from x in VE.TPROGDTL
                            where x.ITCD != null
                            group x by new
                            {
                                x.ITCD,
                                x.ITNM,
                                x.UOM,
                                x.SLNO
                            } into P
                            select new TPROGDTL
                            {
                                ITCD = P.Key.ITCD.retStr(),
                                ITNM = P.Key.ITNM.retStr(),
                                UOM = P.Key.UOM.retStr(),
                                PROGSLNO = P.Key.SLNO.retShort(),
                                TOTALREQQTY = (P.Sum(A => A.QNTY.retDbl())).retDbl(),
                                USEDQTY = (P.Sum(A => A.QNTY.retDbl())).retDbl(),
                                QNTY = (P.Sum(A => A.QNTY.retDbl())).retDbl(),
                                RECPROGITCD = P.Key.ITCD.retStr(),
                                RECPROGITSTYLE = P.Key.ITNM.retStr(),
                            }).ToList();
                }
                else
                {
                    data = (from x in VE.TPROGBOM
                            where x.ITCD != null
                            group x by new
                            {
                                x.ITCD,
                                x.ITNM,
                                x.UOM,
                                x.SLNO,
                                x.QITNM,
                            } into P
                            select new TPROGDTL
                            {
                                ITCD = P.Key.ITCD.retStr(),
                                ITNM = P.Key.ITNM.retStr(),
                                UOM = P.Key.UOM.retStr(),
                                PROGSLNO = P.Key.SLNO.retShort(),
                                TOTALREQQTY = (P.Sum(A => A.BOMQNTY.retDbl()) + P.Sum(A => A.EXTRAQNTY.retDbl())).retDbl(),
                                USEDQTY = (P.Sum(A => A.BOMQNTY.retDbl()) + P.Sum(A => A.EXTRAQNTY.retDbl())).retDbl(),
                                QNTY = (P.Sum(A => A.BOMQNTY.retDbl()) + P.Sum(A => A.EXTRAQNTY.retDbl())).retDbl(),
                                RECPROGITCD = "",
                                RECPROGITSTYLE = P.Key.QITNM.retStr(),
                            }).ToList();

                }

                if (val.retStr() != "" && Code.retStr() != "")
                {
                    data = data.Where(a => a.ITCD == val && a.PROGSLNO == Code.retShort()).ToList();
                }
                else if (val.retStr() != "")
                {
                    data = data.Where(a => a.ITCD == val).ToList();
                }
                var bomdata = ListToDatatable.LINQResultToDataTable(data);
                if (bomdata != null)
                {
                    bomdata.Columns.Add("TOTALQNTY", typeof(string));
                }
                for (int i = 0; i <= bomdata.Rows.Count - 1; i++)
                {
                    double currentadjqnty = 0;
                    if (VE.TBATCHDTL != null)
                    {
                        string itcd = bomdata.Rows[i]["itcd"].retStr();
                        short progslno = bomdata.Rows[i]["progslno"].retShort();
                        currentadjqnty = VE.TBATCHDTL.Where(a => a.RECPROGSLNO == progslno).Sum(b => b.QNTY).retDbl();

                        //currentadjqnty = VE.TBATCHDTL.Where(a => a.ITCD == itcd && a.RECPROGSLNO == progslno).Sum(b => b.QNTY).retDbl();
                    }
                    bomdata.Rows[i]["USEDQTY"] = currentadjqnty.retDbl();
                    bomdata.Rows[i]["TOTALQNTY"] = (bomdata.Rows[i]["TOTALREQQTY"].retDbl() - currentadjqnty.retDbl()) + "~~" + bomdata.Rows[i]["TOTALREQQTY"].retDbl();
                }
                var filterdata = bomdata.AsEnumerable().Where(a => a.Field<double?>("TOTALREQQTY") - a.Field<double?>("USEDQTY") > 0);
                if (filterdata.Count() > 0)
                {
                    bomdata = filterdata.CopyToDataTable();
                }
                else
                {
                    bomdata = bomdata.Clone();
                }
                if (val.retStr() == "" || bomdata.Rows.Count > 1)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= bomdata.Rows.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + bomdata.Rows[i]["ITNM"] + "</td><td>" + bomdata.Rows[i]["ITCD"] + " </td><td>" + bomdata.Rows[i]["TOTALREQQTY"] + " </td><td>" + bomdata.Rows[i]["USEDQTY"] + " </td><td>" + bomdata.Rows[i]["PROGSLNO"] + " </td></tr>");
                    }
                    var hdr = "Item Name" + Cn.GCS() + "Item Code" + Cn.GCS() + "Total Req. Qnty." + Cn.GCS() + "Used Qnty" + Cn.GCS() + "Prog. Slno";
                    return PartialView("_Help2", masterHelp.Generate_help(hdr, SB.ToString()));
                }
                else
                {
                    if (bomdata.Rows.Count > 0)
                    {
                        string str = masterHelp.ToReturnFieldValues("", bomdata);
                        return Content(str);
                    }
                    else
                    {
                        return Content("Invalid Bom Item Code ! Please Enter a Valid Bom Item Code !!");
                    }
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetJobDetails(string val)
        {
            try
            {
                //var agent = Code.Split(Convert.ToChar(Cn.GCS()));
                //if (agent.Count() > 1)
                //{
                //    if (agent[1] == "")
                //    {
                //        return Content("Please Select Agent !!");
                //    }
                //    else
                //    {
                //        Code = agent[0];
                //    }
                //}
                var str = masterHelp.JOBCD_JOBMST_help(val, "");
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
        public ActionResult GetSubLedgerDetails(string val, string Code)
        {
            try
            {
                if (Code.retStr() == "")
                {
                    return Content("Please Select Document Date !!");
                }
                var str = masterHelp.SLCD_help(val, "J");
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    if (str.IndexOf(Cn.GCS()) > 0)
                    {
                        var party_data = salesfunc.GetSlcdDetails(val, Code.retStr());
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
        public ActionResult GetItemGroupDetails(string val, string Code)
        {
            try
            {
                TransactionOutIssProcess VE = new TransactionOutIssProcess();
                Cn.getQueryString(VE);
                string str = masterHelp.ITGRPCD_help(val, "", Code);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    string glcd = "";
                    //switch (VE.MENU_PARA)
                    //{
                    //    case "SBPCK"://Packing Slip
                    //        glcd = str.retCompValue("SALGLCD"); break;
                    //    case "SB"://Sales Bill (Agst Packing Slip)
                    //        glcd = str.retCompValue("SALGLCD"); break;
                    //    case "SBDIR"://Sales Bill
                    //        glcd = str.retCompValue("SALGLCD"); break;
                    //    case "SR"://Sales Return (SRM)
                    //        glcd = str.retCompValue("SALRETGLCD"); break;
                    //    case "SBCM"://Cash Memo
                    //        glcd = str.retCompValue("SALGLCD"); break;
                    //    case "SBCMR"://Cash Memo Return Note
                    //        glcd = str.retCompValue("SALGLCD"); break;
                    //    case "SBEXP"://Sales Bill (Export)
                    //        glcd = str.retCompValue("SALGLCD"); break;
                    //    case "PI"://Proforma Invoice
                    //        glcd = ""; break;
                    //    case "PB"://Purchase Bill
                    //        glcd = str.retCompValue("PURGLCD"); break;
                    //    case "PR"://Purchase Return (PRM)
                    //        glcd = str.retCompValue("PURRETGLCD"); break;
                    //    default: glcd = ""; break;
                    //}
                    str += "^GLCD=^" + glcd + Cn.GCS();
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
                TransactionOutIssProcess VE = new TransactionOutIssProcess();
                Cn.getQueryString(VE);
                var data = Code.Split(Convert.ToChar(Cn.GCS()));
                string ITGRPCD = data[0].retStr() == "" ? "" : data[0].retStr().retSqlformat();
                string DOCDT = data[1].retStr();
                if (DOCDT == "")
                {
                    return Content("Please Select Document Date");
                }
                string TAXGRPCD = data[2].retStr();
                string GOCD = "";// data[3].retStr() == "" ? "" : data[3].retStr().retSqlformat();
                string PRCCD = data[4].retStr();
                string MTRLJOBCD = data[5].retStr() == "" ? "" : data[5].retStr().retSqlformat();
                string BARNO = data[6].retStr() == "" ? "" : data[6].retStr().retSqlformat();
                double RATE = data[7].retDbl();
                var str = masterHelp.ITCD_help(val, "", data[0].retStr());
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else if (str.IndexOf(Convert.ToChar(Cn.GCS())) >= 0)
                {
                    string PRODGRPGSTPER = "", ALL_GSTPER = "", GSTPER = "";
                    DataTable tax_data = new DataTable();

                    tax_data = salesfunc.GetStock(DOCDT.retStr(), GOCD.retStr(), BARNO.retStr(), val.retStr().retSqlformat(), MTRLJOBCD.retStr(), "", ITGRPCD, "", PRCCD.retStr(), TAXGRPCD.retStr());

                    if (tax_data != null && tax_data.Rows.Count > 0)
                    {
                        PRODGRPGSTPER = tax_data.Rows[0]["PRODGRPGSTPER"].retStr();
                        if (PRODGRPGSTPER != "")
                        {
                            ALL_GSTPER = salesfunc.retGstPer(PRODGRPGSTPER, RATE);
                            if (ALL_GSTPER.retStr() != "")
                            {
                                var gst = ALL_GSTPER.Split(',').ToList();
                                GSTPER = (from a in gst select a.retDbl()).Sum().retStr();
                            }
                        }
                    }
                    str += "^PRODGRPGSTPER=^" + PRODGRPGSTPER + Cn.GCS();
                    str += "^ALL_GSTPER=^" + ALL_GSTPER + Cn.GCS();
                    str += "^GSTPER=^" + GSTPER + Cn.GCS();
                    return Content(str);
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
        public ActionResult GetGridItemDetails(string val, string Code)
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

                    return Content(str);//
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
        public ActionResult GetMaterialDetails(string val)
        {
            try
            {
                var str = masterHelp.MTRLJOBCD_help(val);
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
        public ActionResult FillBarCodeData(TransactionOutIssProcess VE)
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
                    TBATCHDTL1.STKNAME = VE.STKNAME;
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
                    TBATCHDTL1.SCMDISCRATE = VE.SCMDISCRATE;
                    TBATCHDTL1.SCMDISCTYPE = VE.SCMDISCTYPE;
                    TBATCHDTL1.BARNO = VE.BARCODE;
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
                    TBATCHDTL1.STKNAME = VE.STKNAME;
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
                    TBATCHDTL1.SCMDISCRATE = VE.SCMDISCRATE;
                    TBATCHDTL1.SCMDISCTYPE = VE.SCMDISCTYPE;
                    TBATCHDTL1.BARNO = VE.BARCODE;
                    TBATCHDTL.Add(TBATCHDTL1);
                    VE.TBATCHDTL = TBATCHDTL;

                }
                VE.DefaultView = true;
                return PartialView("_T_OUTISSPROCESS_MaterialIssue", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult FillDetailData(TransactionOutIssProcess VE)
        {
            try
            {
                Cn.getQueryString(VE);
                VE.TTXNDTL = (from x in VE.TBATCHDTL
                              group x by new
                              {
                                  x.TXNSLNO,
                                  x.ITGRPCD,
                                  x.ITGRPNM,
                                  x.MTRLJOBCD,
                                  x.MTRLJOBNM,
                                  x.MTBARCODE,
                                  x.ITCD,
                                  x.ITSTYLE,
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
                                  x.HSNCODE,
                                  x.PRODGRPGSTPER,
                                  x.BALENO,
                                  x.GLCD,
                              } into P
                              select new TTXNDTL
                              {
                                  SLNO = P.Key.TXNSLNO.retShort(),
                                  ITGRPCD = P.Key.ITGRPCD,
                                  ITGRPNM = P.Key.ITGRPNM,
                                  MTRLJOBCD = P.Key.MTRLJOBCD,
                                  MTRLJOBNM = P.Key.MTRLJOBNM,
                                  //MTBARCODE = P.Key.MTBARCODE,
                                  ITCD = P.Key.ITCD,
                                  //ITNM = P.Key.STYLENO + "" + P.Key.ITNM,
                                  ITSTYLE = P.Key.ITSTYLE,
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
                                  ALL_GSTPER = P.Key.ALL_GSTPER.retStr() == "" ? P.Key.GSTPER.retStr() : P.Key.ALL_GSTPER,
                                  //AMT = P.Sum(A => A.BLQNTY).retDbl() == 0 ? (P.Sum(A => A.QNTY).retDbl() - P.Sum(A => A.FLAGMTR).retDbl()) * P.Key.RATE.retDbl() : P.Sum(A => A.BLQNTY).retDbl() * P.Key.RATE.retDbl(),
                                  HSNCODE = P.Key.HSNCODE,
                                  PRODGRPGSTPER = P.Key.PRODGRPGSTPER,
                                  BALENO = P.Key.BALENO,
                                  GLCD = P.Key.GLCD,
                              }).ToList();

                //for (int p = 0; p <= VE.TTXNDTL.Count - 1; p++)
                //{
                //    if (VE.TTXNDTL[p].ALL_GSTPER.retStr() != "")
                //    {
                //        var tax_data = VE.TTXNDTL[p].ALL_GSTPER.Split(',').ToList();
                //        if (tax_data.Count == 1)
                //        {
                //            VE.TTXNDTL[p].IGSTPER = tax_data[0].retDbl() / 3;
                //            VE.TTXNDTL[p].CGSTPER = tax_data[0].retDbl() / 3;
                //            VE.TTXNDTL[p].SGSTPER = tax_data[0].retDbl() / 3;
                //        }
                //        else
                //        {
                //            VE.TTXNDTL[p].IGSTPER = tax_data[0].retDbl();
                //            VE.TTXNDTL[p].CGSTPER = tax_data[1].retDbl();
                //            VE.TTXNDTL[p].SGSTPER = tax_data[2].retDbl();
                //        }

                //    }
                //}
                for (int p = 0; p <= VE.TTXNDTL.Count - 1; p++)
                {
                    if (VE.TTXNDTL[p].PRODGRPGSTPER.retStr() != "")
                    {
                        var gstdata = salesfunc.retGstPer(VE.TTXNDTL[p].PRODGRPGSTPER.retStr(), VE.TTXNDTL[p].RATE.retDbl());
                        if (gstdata.retStr() != "")
                        {
                            var gst = gstdata.Split(',');
                            if (gst.Count() > 0)
                            {
                                VE.TTXNDTL[p].IGSTPER = gst[0].retDbl();
                                VE.TTXNDTL[p].CGSTPER = gst[1].retDbl();
                                VE.TTXNDTL[p].SGSTPER = gst[2].retDbl();
                            }

                        }
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
                return PartialView("_T_OUTISSPROCESS_DETAIL", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        //public ActionResult FillQtyRequirementData(TransactionOutIssProcess VE)
        //{
        //    try
        //    {
        //        ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
        //        Cn.getQueryString(VE);
        //        List<TPROGBOM> itemdata = new List<TPROGBOM>();
        //        if (VE.T_TXN.JOBCD == "DY")
        //        {
        //            VE.TPROGBOM = (from x in VE.TPROGDTL
        //                           select new TPROGBOM
        //                           {
        //                               SLNO = x.SLNO.retShort(),//progslno
        //                               RSLNO = x.SLNO.retShort(),//progslno
        //                               QITNM = x.ITNM,
        //                               QUOM = x.UOM,
        //                               QQNTY = x.QNTY,
        //                               BOMQNTY = x.QNTY,
        //                               BARNO = x.BARNO,
        //                               ITGRPCD = x.ITGRPCD,
        //                               ITGRPNM = x.ITGRPNM,
        //                               ITCD = x.ITCD,
        //                               ITNM = x.ITNM,
        //                               SIZECD = x.SIZECD,
        //                               COLRCD = x.COLRCD,
        //                               COLRNM = x.COLRNM,
        //                               UOM = x.UOM,
        //                               MTRLJOBCD = x.CheckedSample == true ? x.MTRLJOBCD : "PL",
        //                               //MTRLJOBNM = x.MTRLJOBNM,
        //                               Q_CheckedSample = x.CheckedSample == true ? true : false,
        //                           }).ToList();
        //        }
        //        else
        //        {
        //            var PROGRAMME_DATA = VE.TPROGDTL.Where(A => A.ITCD != null).Distinct().ToList();
        //            var finitems = ListToDatatable.LINQResultToDataTable(PROGRAMME_DATA);
        //            var PROG_BOM_DATA = finitems != null && finitems.Rows.Count > 0 ? salesfunc.GetSemiItems(finitems, VE.MENU_PARA, "", VE.T_TXN.DOCDT.ToString().retDateStr()) : null;

        //            VE.TPROGBOM = (from DataRow dr in PROG_BOM_DATA.Rows
        //                           select new TPROGBOM
        //                           {
        //                               QITNM = dr["itnm"].ToString(),
        //                               QUOM = dr["ruomnm"].ToString(),
        //                               QQNTY = Convert.ToDouble(dr["rqty"] == DBNull.Value ? null : dr["rqty"].ToString()),
        //                               ITCD = dr["ritcd"].ToString(),
        //                               ITNM = dr["ritnm"].ToString(),
        //                               PARTCD = dr["partcd"].ToString(),
        //                               COLRCD = dr["colrcd"].ToString(),
        //                               SIZECD = dr["sizecd"].ToString(),
        //                               BOMQNTY = Convert.ToDouble(dr["rqty"] == DBNull.Value ? null : dr["rqty"].ToString()),
        //                               MTRLJOBCD = dr["mtrljobcd"].ToString(),
        //                               MTRLJOBNM = dr["mtrljobnm"].ToString(),
        //                               SLNO = dr["slno"].retShort(),
        //                               RSLNO = dr["rslno"].retShort(),
        //                           }).ToList();
        //            string[] itcdarr = VE.TPROGBOM.Select(a => a.ITCD).Distinct().ToArray();
        //            itemdata = (from a in DB.M_SITEM
        //                        join b in DB.M_GROUP on a.ITGRPCD equals b.ITGRPCD into x
        //                        from b in x.DefaultIfEmpty()
        //                        where itcdarr.Contains(a.ITCD)
        //                        select new TPROGBOM()
        //                        {
        //                            ITCD = a.ITCD,
        //                            ITNM = a.ITNM,
        //                            ITGRPCD = a.ITGRPCD,
        //                            ITGRPNM = b.ITGRPNM,
        //                            STYLENO = a.STYLENO,
        //                            UOM = a.UOMCD,
        //                        }).ToList();
        //            //VE.TPROGBOM = (from x in VE.TPROGDTL
        //            //               group x by new
        //            //               {
        //            //                   x.SLNO,
        //            //                   x.ITCD,
        //            //                   x.ITNM,
        //            //                   x.UOM,
        //            //                   x.QNTY
        //            //               } into P
        //            //               select new TPROGBOM
        //            //               {
        //            //                   SLNO = P.Key.SLNO.retShort(),
        //            //                   QITNM = P.Key.ITNM,
        //            //                   QUOM = P.Key.UOM,
        //            //                   QQNTY = P.Key.QNTY,
        //            //                   //qnty = P.Sum(A => A.QNTY)
        //            //               }).ToList();

        //        }

        //        for (int p = 0; p <= VE.TPROGBOM.Count - 1; p++)
        //        {
        //            if (VE.T_TXN.JOBCD == "DY")
        //            {
        //                string mtrljobcd = VE.TPROGBOM[p].MTRLJOBCD;
        //                var mtrljobnm = DB.M_MTRLJOBMST.Where(a => a.MTRLJOBCD == mtrljobcd).Select(b => b.MTRLJOBNM).FirstOrDefault();
        //                VE.TPROGBOM[p].MTRLJOBNM = mtrljobnm;
        //            }
        //            else
        //            {
        //                string itcd = VE.TPROGBOM[p].ITCD;
        //                var data = (from a in itemdata where a.ITCD == itcd select new { a.ITNM, a.ITGRPCD, a.ITGRPNM, a.STYLENO ,a.UOM}).FirstOrDefault();
        //                VE.TPROGBOM[p].ITNM = data.STYLENO + " " + data.ITNM;
        //                VE.TPROGBOM[p].ITGRPCD = data.ITGRPCD;
        //                VE.TPROGBOM[p].ITGRPNM = data.ITGRPNM;
        //                VE.TPROGBOM[p].UOM = data.UOM;
        //            }
        //            //else
        //            //{
        //            //    VE.TPROGBOM[p].SLNO = Convert.ToInt16(p + 1);
        //            //}
        //            //VE.TPROGBOM[p].RSLNO = Convert.ToInt16(p + 1);


        //        }
        //        VE.T_QQNTY = VE.TPROGBOM.Sum(a => a.QQNTY).retDbl();
        //        ModelState.Clear();
        //        VE.DefaultView = true;
        //        return PartialView("_T_OUTISSPROCESS_QtyRequirement", VE);
        //    }
        //    catch (Exception ex)
        //    {
        //        Cn.SaveException(ex, "");
        //        return Content(ex.Message + ex.InnerException);
        //    }
        //}
        public ActionResult FillQtyRequirementData(TransactionOutIssProcess VE)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                Cn.getQueryString(VE);
                List<TPROGBOM> itemdata = new List<TPROGBOM>();

                if (VE.T_TXN.JOBCD != "DY")
                {
                    var PROGRAMME_DATA = VE.TPROGDTL.Where(A => A.ITCD != null).Distinct().ToList();
                    var finitems = ListToDatatable.LINQResultToDataTable(PROGRAMME_DATA);
                    var PROG_BOM_DATA = finitems != null && finitems.Rows.Count > 0 ? salesfunc.GetSemiItems(finitems, VE.MENU_PARA, "", VE.T_TXN.DOCDT.ToString().retDateStr()) : null;

                    VE.TPROGBOM = (from DataRow dr in PROG_BOM_DATA.Rows
                                   select new TPROGBOM
                                   {
                                       QITNM = dr["itnm"].ToString(),
                                       QUOM = dr["ruomnm"].ToString(),
                                       QQNTY = Convert.ToDouble(dr["rqty"] == DBNull.Value ? null : dr["rqty"].ToString()),
                                       ITCD = dr["ritcd"].ToString(),
                                       ITNM = dr["ritnm"].ToString(),
                                       PARTCD = dr["partcd"].ToString(),
                                       COLRCD = dr["colrcd"].ToString(),
                                       SIZECD = dr["sizecd"].ToString(),
                                       BOMQNTY = Convert.ToDouble(dr["rqty"] == DBNull.Value ? null : dr["rqty"].ToString()),
                                       MTRLJOBCD = dr["mtrljobcd"].ToString(),
                                       MTRLJOBNM = dr["mtrljobnm"].ToString(),
                                       SLNO = dr["slno"].retShort(),
                                       RSLNO = dr["rslno"].retShort(),
                                   }).ToList();
                    string[] itcdarr = VE.TPROGBOM.Select(a => a.ITCD).Distinct().ToArray();
                    itemdata = (from a in DB.M_SITEM
                                join b in DB.M_GROUP on a.ITGRPCD equals b.ITGRPCD into x
                                from b in x.DefaultIfEmpty()
                                where itcdarr.Contains(a.ITCD)
                                select new TPROGBOM()
                                {
                                    ITCD = a.ITCD,
                                    ITNM = a.ITNM,
                                    ITGRPCD = a.ITGRPCD,
                                    ITGRPNM = b.ITGRPNM,
                                    STYLENO = a.STYLENO,
                                    UOM = a.UOMCD,
                                }).ToList();
                    //VE.TPROGBOM = (from x in VE.TPROGDTL
                    //               group x by new
                    //               {
                    //                   x.SLNO,
                    //                   x.ITCD,
                    //                   x.ITNM,
                    //                   x.UOM,
                    //                   x.QNTY
                    //               } into P
                    //               select new TPROGBOM
                    //               {
                    //                   SLNO = P.Key.SLNO.retShort(),
                    //                   QITNM = P.Key.ITNM,
                    //                   QUOM = P.Key.UOM,
                    //                   QQNTY = P.Key.QNTY,
                    //                   //qnty = P.Sum(A => A.QNTY)
                    //               }).ToList();
                    for (int p = 0; p <= VE.TPROGBOM.Count - 1; p++)
                    {
                        string itcd = VE.TPROGBOM[p].ITCD;
                        var data = (from a in itemdata where a.ITCD == itcd select new { a.ITNM, a.ITGRPCD, a.ITGRPNM, a.STYLENO, a.UOM }).FirstOrDefault();
                        VE.TPROGBOM[p].ITNM = data.STYLENO + " " + data.ITNM;
                        VE.TPROGBOM[p].ITGRPCD = data.ITGRPCD;
                        VE.TPROGBOM[p].ITGRPNM = data.ITGRPNM;
                        VE.TPROGBOM[p].UOM = data.UOM;

                    }

                }
                if (VE.T_TXN.JOBCD == "DY" || VE.TPROGBOM == null || VE.TPROGBOM.Count == 0)
                {
                    VE.TPROGBOM = (from x in VE.TPROGDTL
                                   select new TPROGBOM
                                   {
                                       SLNO = x.SLNO.retShort(),//progslno
                                       RSLNO = 1,//progslno
                                       QITNM = x.ITNM,
                                       QUOM = x.UOM,
                                       QQNTY = x.QNTY,
                                       BOMQNTY = (VE.T_TXN.JOBCD == "DY" || x.CheckedSample == true) ? x.QNTY : (double?)null,
                                       BARNO = (VE.T_TXN.JOBCD == "DY" || x.CheckedSample == true) ? x.BARNO : "",
                                       ITGRPCD = (VE.T_TXN.JOBCD == "DY" || x.CheckedSample == true) ? x.ITGRPCD : "",
                                       ITGRPNM = (VE.T_TXN.JOBCD == "DY" || x.CheckedSample == true) ? x.ITGRPNM : "",
                                       ITCD = (VE.T_TXN.JOBCD == "DY" || x.CheckedSample == true) ? x.ITCD : "",
                                       ITNM = (VE.T_TXN.JOBCD == "DY" || x.CheckedSample == true) ? x.ITNM : "",
                                       SIZECD = (VE.T_TXN.JOBCD == "DY" || x.CheckedSample == true) ? x.SIZECD : "",
                                       COLRCD = (VE.T_TXN.JOBCD == "DY" || x.CheckedSample == true) ? x.COLRCD : "",
                                       COLRNM = (VE.T_TXN.JOBCD == "DY" || x.CheckedSample == true) ? x.COLRNM : "",
                                       UOM = (VE.T_TXN.JOBCD == "DY" || x.CheckedSample == true) ? x.UOM : "",
                                       MTRLJOBCD = VE.T_TXN.JOBCD == "DY" ? (x.CheckedSample == true ? x.MTRLJOBCD : "PL") : x.CheckedSample == true ? x.MTRLJOBCD : "",
                                       //MTRLJOBNM = x.MTRLJOBNM,
                                       Q_CheckedSample = (VE.T_TXN.JOBCD == "DY" || x.CheckedSample == true) ? (x.CheckedSample == true ? true : false) : false,
                                   }).ToList();
                }
                for (int p = 0; p <= VE.TPROGBOM.Count - 1; p++)
                {
                    if (VE.TPROGBOM[p].MTRLJOBNM.retStr() == "")
                    {
                        string mtrljobcd = VE.TPROGBOM[p].MTRLJOBCD;
                        var mtrljobnm = DB.M_MTRLJOBMST.Where(a => a.MTRLJOBCD == mtrljobcd).Select(b => b.MTRLJOBNM).FirstOrDefault();
                        VE.TPROGBOM[p].MTRLJOBNM = mtrljobnm;
                    }
                }
                VE.T_QQNTY = VE.TPROGBOM.Sum(a => a.QQNTY).retDbl();
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_OUTISSPROCESS_QtyRequirement", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetRateHistoryDetails(string SLCD, string PARTYCD, string ITCD, string TAG)
        {
            try
            {
                RateHistory RH = new RateHistory();
                TransactionOutIssProcess VE = new TransactionOutIssProcess();
                Cn.getQueryString(VE);
                //var DTRateHistory = salesfunc.GetRateHistory(VE.DOC_CODE, ITCD);
                var DTRateHistory = salesfunc.GetRateHistory(SLCD.retStr().retSqlformat(), PARTYCD.retStr().retSqlformat(), VE.DOC_CODE.retStr().retSqlformat(), ITCD.retStr().retSqlformat());
                var doctP = (from DataRow dr in DTRateHistory.Rows
                             select new RateHistoryGrid()
                             {
                                 AUTONO = dr["AUTONO"].ToString(),
                                 DOCDT = dr["DOCDT"].retDateStr(),
                                 DOCNO = dr["DOCNO"].ToString(),
                                 QNTY = dr["QNTY"].ToString(),
                                 RATE = dr["RATE"].ToString(),
                                 SLCD = dr["SLCD"].ToString(),
                                 SLNM = dr["SLNM"].ToString(),
                                 CITY = dr["CITY"].ToString(),
                             }).ToList();
                RH.RateHistoryGrid = doctP;
                ModelState.Clear();
                return PartialView("_T_SALE_RateHistory", RH);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult DeleteRowBarno(TransactionOutIssProcess VE)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                List<TBATCHDTL> TBATCHDTL = new List<TBATCHDTL>();
                int count = 0;
                if (VE.TBATCHDTL != null)
                {
                    for (int i = 0; i <= VE.TBATCHDTL.Count - 1; i++)
                    {
                        if (VE.TBATCHDTL[i].Checked == false)
                        {
                            count += 1;
                            TBATCHDTL item = new TBATCHDTL();
                            item = VE.TBATCHDTL[i];
                            if (VE.DefaultAction == "A")
                            {
                                item.SLNO = Convert.ToSByte(count);
                            }

                            TBATCHDTL.Add(item);
                        }
                    }
                }
                VE.TBATCHDTL = TBATCHDTL;
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_OUTISSPROCESS_MaterialIssue", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult AddRow(TransactionOutIssProcess VE, int COUNT, string TAG)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            Cn.getQueryString(VE);
            //ViewBag.formname = formnamebydoccd(VE.DOC_CODE);

            //List<DebitCreditType> DCT = new List<DebitCreditType>();
            //DebitCreditType DCT1 = new DebitCreditType(); DCT1.text = "DR"; DCT1.value = "D"; DCT.Add(DCT1);
            //DebitCreditType DCT2 = new DebitCreditType(); DCT2.text = "CR"; DCT2.value = "C"; DCT.Add(DCT2); VE.DebitCreditType = DCT;
            //VE.Database_Combo2 = (from i in DB.T_VCH_DET select new Database_Combo2() { FIELD_VALUE = i.BANK_NAME }).DistinctBy(a => a.FIELD_VALUE).ToList();
            //VE.Database_Combo3 = (from i in DB.T_VCH_DET select new Database_Combo3() { FIELD_VALUE = i.T_REM }).DistinctBy(a => a.FIELD_VALUE).ToList();
            //VE.DropDown_list_TDS = INT_TDS();
            if (VE.TPROGDTL == null)
            {
                List<TPROGDTL> TPROGDTL1 = new List<TPROGDTL>();
                if (COUNT > 0 && TAG == "Y")
                {
                    int SERIAL = 0;
                    for (int j = 0; j <= COUNT - 1; j++)
                    {
                        SERIAL = SERIAL + 1;
                        TPROGDTL MBILLDET = new TPROGDTL();
                        MBILLDET.SLNO = SERIAL.retShort();
                        TPROGDTL1.Add(MBILLDET);
                    }
                }
                else
                {
                    TPROGDTL MBILLDET = new TPROGDTL();
                    MBILLDET.SLNO = 1;
                    TPROGDTL1.Add(MBILLDET);
                }
                VE.TPROGDTL = TPROGDTL1;
            }
            else
            {
                List<TPROGDTL> TPROGDTL = new List<TPROGDTL>();
                for (int i = 0; i <= VE.TPROGDTL.Count - 1; i++)
                {
                    TPROGDTL MBILLDET = new TPROGDTL();
                    MBILLDET = VE.TPROGDTL[i];
                    TPROGDTL.Add(MBILLDET);
                }
                TPROGDTL MBILLDET1 = new TPROGDTL();
                if (COUNT > 0 && TAG == "Y")
                {
                    int SERIAL = Convert.ToInt32(VE.TPROGDTL.Max(a => Convert.ToInt32(a.SLNO)));
                    for (int j = 0; j <= COUNT - 1; j++)
                    {
                        SERIAL = SERIAL + 1;
                        TPROGDTL OPENING_BL = new TPROGDTL();
                        OPENING_BL.SLNO = SERIAL.retShort();
                        TPROGDTL.Add(OPENING_BL);
                    }
                }
                else
                {
                    MBILLDET1.SLNO = Convert.ToInt16(Convert.ToByte(VE.TPROGDTL.Max(a => Convert.ToInt32(a.SLNO))) + 1);
                    TPROGDTL.Add(MBILLDET1);
                }
                VE.TPROGDTL = TPROGDTL;
            }
            //VE.TPROGDTL.ForEach(a => a.DRCRTA = masterHelp.DR_CR().OrderByDescending(s => s.text).ToList());
            VE.DefaultView = true;
            return PartialView("_T_OUTISSPROCESS_Programme", VE);
        }
        public ActionResult DeleteRow(TransactionOutIssProcess VE)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                List<TPROGDTL> ITEMSIZE = new List<TPROGDTL>();
                int count = 0;
                for (int i = 0; i <= VE.TPROGDTL.Count - 1; i++)
                {
                    if (VE.TPROGDTL[i].Checked == false)
                    {
                        count += 1;
                        TPROGDTL item = new TPROGDTL();
                        item = VE.TPROGDTL[i];
                        item.SLNO = count.retShort();
                        ITEMSIZE.Add(item);
                    }

                }
                VE.TPROGDTL = ITEMSIZE;
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_OUTISSPROCESS_Programme", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult AddRowQTY(TransactionOutIssProcess VE, int COUNT, string TAG)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            Cn.getQueryString(VE);
            if (VE.TPROGBOM == null)
            {
                List<TPROGBOM> TPROGBOM1 = new List<TPROGBOM>();
                if (COUNT > 0 && TAG == "Y")
                {
                    int SERIAL = 0, rslno = 0;
                    for (int j = 0; j <= COUNT - 1; j++)
                    {
                        SERIAL = SERIAL + 1;
                        rslno = rslno + 1;
                        TPROGBOM MBILLDET = new TPROGBOM();
                        MBILLDET.SLNO = SERIAL.retShort();
                        MBILLDET.RSLNO = rslno.retShort();
                        TPROGBOM1.Add(MBILLDET);
                    }
                }
                else
                {
                    TPROGBOM MBILLDET = new TPROGBOM();
                    MBILLDET.SLNO = 1;
                    MBILLDET.RSLNO = 1;
                    TPROGBOM1.Add(MBILLDET);
                }
                VE.TPROGBOM = TPROGBOM1;
            }
            else
            {
                List<TPROGBOM> TPROGBOM = new List<TPROGBOM>();
                for (int i = 0; i <= VE.TPROGBOM.Count - 1; i++)
                {
                    TPROGBOM MBILLDET = new TPROGBOM();
                    MBILLDET = VE.TPROGBOM[i];
                    TPROGBOM.Add(MBILLDET);
                }
                TPROGBOM MBILLDET1 = new TPROGBOM();
                if (COUNT > 0 && TAG == "Y")
                {
                    int SERIAL = Convert.ToInt32(VE.TPROGBOM.Max(a => Convert.ToInt32(a.SLNO)));
                    int rslno = Convert.ToInt32(VE.TPROGBOM.Max(a => Convert.ToInt32(a.RSLNO)));
                    for (int j = 0; j <= COUNT - 1; j++)
                    {
                        SERIAL = SERIAL + 1;
                        rslno = rslno + 1;
                        TPROGBOM OPENING_BL = new TPROGBOM();
                        OPENING_BL.SLNO = SERIAL.retShort();
                        OPENING_BL.RSLNO = rslno.retShort();
                        TPROGBOM.Add(OPENING_BL);
                    }
                }
                else
                {
                    MBILLDET1.SLNO = Convert.ToInt16(Convert.ToByte(VE.TPROGBOM.Max(a => Convert.ToInt32(a.SLNO))) + 1);
                    MBILLDET1.RSLNO = Convert.ToInt16(Convert.ToByte(VE.TPROGBOM.Max(a => Convert.ToInt32(a.RSLNO))) + 1);
                    TPROGBOM.Add(MBILLDET1);
                }
                VE.TPROGBOM = TPROGBOM;
            }
            //VE.TPROGDTL.ForEach(a => a.DRCRTA = masterHelp.DR_CR().OrderByDescending(s => s.text).ToList());
            VE.DefaultView = true;
            return PartialView("_T_OUTISSPROCESS_QtyRequirement", VE);
        }
        public ActionResult DeleteRowQTY(TransactionOutIssProcess VE)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                List<TPROGBOM> ITEMSIZE = new List<TPROGBOM>();
                int count = 0;
                for (int i = 0; i <= VE.TPROGBOM.Count - 1; i++)
                {
                    if (VE.TPROGBOM[i].Checked == false)
                    {
                        count += 1;
                        TPROGBOM item = new TPROGBOM();
                        item = VE.TPROGBOM[i];
                        item.SLNO = count.retShort();
                        ITEMSIZE.Add(item);
                    }

                }
                VE.TPROGBOM = ITEMSIZE;
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_OUTISSPROCESS_QtyRequirement", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult AddDOCRow(TransactionOutIssProcess VE)
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
        public ActionResult DeleteDOCRow(TransactionOutIssProcess VE)
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
        public ActionResult RepeatAboveRow(TransactionOutIssProcess VE, int RowIndex)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            Cn.getQueryString(VE);
            string SLNO = VE.TPROGBOM[RowIndex - 1].SLNO.retStr();
            List<TPROGBOM> TPROGBOM = new List<TPROGBOM>(); bool copied = false;
            int RSLNO = 1;
            for (int k = 0; k <= VE.TPROGBOM.Count; k++)
            {
                TPROGBOM MBILLDET = new TPROGBOM();
                if (RowIndex == k || copied == true)
                {
                    foreach (PropertyInfo propA in VE.TPROGBOM[k - 1].GetType().GetProperties())
                    {
                        PropertyInfo propB = VE.TPROGBOM[k - 1].GetType().GetProperty(propA.Name);
                        propB.SetValue(MBILLDET, propA.GetValue(VE.TPROGBOM[k - 1], null), null);
                    }
                    if (RowIndex == k)
                    {
                        MBILLDET.ITCD = "";
                        MBILLDET.ITNM = "";
                        MBILLDET.UOM = "";
                        MBILLDET.MTRLJOBCD = "";
                        MBILLDET.MTRLJOBNM = "";
                        MBILLDET.PARTCD = "";
                        MBILLDET.PARTNM = "";
                        MBILLDET.COLRCD = "";
                        MBILLDET.COLRNM = "";
                        MBILLDET.SIZECD = "";
                        MBILLDET.SIZENM = "";
                        MBILLDET.BOMQNTY = 0;
                        MBILLDET.EXTRAQNTY = 0;
                        MBILLDET.Q_CheckedSample = false;
                    }

                    copied = true;
                }
                else
                {
                    MBILLDET = VE.TPROGBOM[k];
                }
                if (MBILLDET.SLNO.retStr() == SLNO)
                {
                    MBILLDET.RSLNO = RSLNO.retShort();
                    RSLNO++;
                }
                TPROGBOM.Add(MBILLDET);
            }
            VE.TPROGBOM = TPROGBOM;
            VE.DefaultView = true;
            ModelState.Clear();
            return PartialView("_T_OUTISSPROCESS_QtyRequirement", VE);
        }
        public ActionResult UpdateQntyPrgrmFrmIssue(TransactionOutIssProcess VE)
        {
            Cn.getQueryString(VE);
            try
            {

                #region Programme
                ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                VE.TPROGDTL = (from a in VE.TBATCHDTL
                               join b in DBF.M_UOM on a.UOM equals b.UOMCD into x
                               from b in x.DefaultIfEmpty()
                               select new TPROGDTL()
                               {
                                   SLNO = a.SLNO,
                                   NOS = a.NOS.retDbl(),
                                   QNTY = a.QNTY.retDbl(),
                                   ITGRPCD = a.ITGRPCD.retStr(),
                                   ITGRPNM = a.ITGRPNM.retStr(),
                                   ITCD = a.ITCD.retStr(),
                                   ITNM = a.ITSTYLE.retStr(),
                                   UOM = a.UOM.retStr(),
                                   SIZECD = a.SIZECD.retStr(),
                                   SIZENM = a.SIZENM.retStr(),
                                   PARTCD = a.PARTCD.retStr(),
                                   PARTNM = a.PARTNM.retStr(),
                                   COLRCD = a.COLRCD.retStr(),
                                   COLRNM = a.COLRNM.retStr(),
                                   MTRLJOBCD = a.MTRLJOBCD.retStr(),
                                   MTRLJOBNM = a.MTRLJOBNM.retStr(),
                                   ITREMARK = a.ITREM.retStr(),
                                   SHADE = a.SHADE.retStr(),
                                   CUTLENGTH = a.CUTLENGTH.retDbl(),
                                   SAMPLE = a.SAMPLE.retStr(),
                                   BARNO = a.BARNO.retStr(),
                                   DECIMALS = b.DECIMALS.retShort(),
                                   CLRBARCODE = a.CLRBARCODE.retStr(),
                                   SZBARCODE = a.SZBARCODE.retStr(),
                                   BarImages = a.BarImages.retStr(),
                                   BarImagesCount = a.BarImagesCount.retStr(),
                                   CheckedSample = a.SAMPLE.retStr() == "Y" ? true : false,
                               }).OrderBy(s => s.SLNO).ToList();

                VE.P_T_NOS = VE.TPROGDTL.Select(a => a.NOS).Sum().retInt();
                VE.P_T_QNTY = VE.TPROGDTL.Select(a => a.QNTY).Sum().retInt();

                #endregion

                #region QtyRequirement
                VE.TPROGBOM = (from a in VE.TBATCHDTL
                               select new TPROGBOM()
                               {
                                   SLNO = a.SLNO.retShort(),
                                   RSLNO = a.SLNO.retShort(),
                                   QQNTY = a.QNTY.retDbl(),
                                   QNTY = a.QNTY.retDbl(),
                                   QITNM = a.ITSTYLE.retStr(),
                                   QUOM = a.UOM.retStr(),
                                   BARNO = a.BARNO.retStr(),
                                   ITGRPCD = a.ITGRPCD.retStr(),
                                   ITGRPNM = a.ITGRPNM.retStr(),
                                   ITCD = a.ITCD.retStr(),
                                   ITNM = a.ITSTYLE.retStr(),
                                   SIZECD = a.SIZECD.retStr(),
                                   SIZENM = a.SIZENM.retStr(),
                                   PARTCD = a.PARTCD.retStr(),
                                   PARTNM = a.PARTNM.retStr(),
                                   COLRCD = a.COLRCD.retStr(),
                                   COLRNM = a.COLRNM.retStr(),
                                   MTRLJOBCD = a.MTRLJOBCD.retStr(),
                                   MTRLJOBNM = a.MTRLJOBNM.retStr(),
                                   UOM = a.UOM.retStr(),
                                   BOMQNTY = VE.T_TXN.JOBCD == "DY" ? a.QNTY.retDbl() : a.BOMQNTY.retDbl(),
                                   //EXTRAQNTY = a.extraqnty.retDbl(),
                                   Q_SAMPLE = a.SAMPLE.retStr(),
                                   Q_CheckedSample = a.SAMPLE.retStr() == "Y" ? true : false,
                               }).OrderBy(s => s.SLNO).ToList();

                VE.T_QQNTY = VE.TPROGBOM.Select(a => a.QQNTY).Sum().retDbl();
                VE.T_BOMQNTY = VE.TPROGBOM.Select(a => a.BOMQNTY).Sum().retDbl();
                VE.T_EXTRAQNTY = VE.TPROGBOM.Select(a => a.EXTRAQNTY).Sum().retDbl();
                #endregion

                ModelState.Clear();
                VE.DefaultView = true;
                var GRID_Programme = RenderRazorViewToString(ControllerContext, "_T_OUTISSPROCESS_Programme", VE);
                var GRID_QtyRequirement = RenderRazorViewToString(ControllerContext, "_T_OUTISSPROCESS_QtyRequirement", VE);
                return Content(GRID_Programme + "^^^^^^^^^^^^~~~~~~^^^^^^^^^^" + GRID_QtyRequirement);
            }
            catch (Exception ex)
            {
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public ActionResult UpdateQtyReqFrmIssue(TransactionOutIssProcess VE)
        {
            Cn.getQueryString(VE);
            try
            {

                #region QtyRequirement
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                VE.TPROGBOM = (from a in VE.TBATCHDTL
                               join b in DB.M_SITEM on a.RECPROGITCD equals b.ITCD
                               select new TPROGBOM()
                               {
                                   SLNO = a.RECPROGSLNO.retShort(),
                                   //RSLNO = a.SLNO.retShort(),
                                   QQNTY = a.BOMQNTY.retStr() == "" ? 0 : Regex.Split(a.BOMQNTY, @"\~\~")[1].retDbl(),
                                   QNTY = a.QNTY.retDbl(),
                                   QITNM = a.RECPROGITSTYLE.retStr(),
                                   QUOM = b.UOMCD.retStr(),
                                   BARNO = a.BARNO.retStr(),
                                   ITGRPCD = a.ITGRPCD.retStr(),
                                   ITGRPNM = a.ITGRPNM.retStr(),
                                   ITCD = a.ITCD.retStr(),
                                   ITNM = a.ITSTYLE.retStr(),
                                   SIZECD = a.SIZECD.retStr(),
                                   SIZENM = a.SIZENM.retStr(),
                                   PARTCD = a.PARTCD.retStr(),
                                   PARTNM = a.PARTNM.retStr(),
                                   COLRCD = a.COLRCD.retStr(),
                                   COLRNM = a.COLRNM.retStr(),
                                   MTRLJOBCD = a.MTRLJOBCD.retStr(),
                                   MTRLJOBNM = a.MTRLJOBNM.retStr(),
                                   UOM = a.UOM.retStr(),
                                   BOMQNTY = a.QNTY.retDbl(),
                                   //EXTRAQNTY = a.extraqnty.retDbl(),
                                   Q_SAMPLE = a.SAMPLE.retStr(),
                                   Q_CheckedSample = a.SAMPLE.retStr() == "Y" ? true : false,
                               }).OrderBy(s => s.SLNO).ToList();

                int i = 0;
                while (i <= VE.TPROGBOM.Count() - 1)
                {
                    string slno = VE.TPROGBOM[i].SLNO.retStr();
                    int rslno = 0;
                    while (VE.TPROGBOM[i].SLNO.retStr() == slno)
                    {
                        rslno++;
                        VE.TPROGBOM[i].RSLNO = rslno.retShort();

                        i++;
                        if (i > VE.TPROGBOM.Count() - 1) break;
                    }
                    if (i > VE.TPROGBOM.Count() - 1) break;
                }
                VE.T_QQNTY = VE.TPROGBOM.Select(a => a.QQNTY).Sum().retDbl();
                VE.T_BOMQNTY = VE.TPROGBOM.Select(a => a.BOMQNTY).Sum().retDbl();
                VE.T_EXTRAQNTY = VE.TPROGBOM.Select(a => a.EXTRAQNTY).Sum().retDbl();
                #endregion

                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_OUTISSPROCESS_QtyRequirement", VE);
            }
            catch (Exception ex)
            {
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
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
        public ActionResult cancelRecords(TransactionOutIssProcess VE, string par1)
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
        public ActionResult ParkRecord(FormCollection FC, TransactionOutIssProcess stream)
        {
            try
            {
                Cn.getQueryString(stream);
                if (stream.T_TXN.DOCCD.retStr() != "")
                {
                    stream.T_CNTRL_HDR.DOCCD = stream.T_TXN.DOCCD.retStr();
                }
                string MNUDET = stream.MENU_DETAILS;
                var menuID = MNUDET.Split('~')[0];
                var menuIndex = MNUDET.Split('~')[1];
                string ID = menuID + menuIndex + CommVar.Loccd(UNQSNO) + CommVar.Compcd(UNQSNO) + CommVar.CurSchema(UNQSNO) + "*" + DateTime.Now;
                ID = ID.Replace(" ", "_");
                string Userid = Session["UR_ID"].ToString();
                INI Handel_ini = new INI();
                var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                string JR = javaScriptSerializer.Serialize(stream);
                Handel_ini.IniWriteValue(Userid, ID, Cn.Encrypt(JR), Server.MapPath("~/Park.ini"));
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
                string url = masterHelp.RetriveParkFromFile(value, Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString(), "Improvar.ViewModels.TransactionOutIssProcess");
                return url;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public ActionResult Print(TransactionOutIssProcess VE)
        {
            try
            {
                Cn.getQueryString(VE);
                Rep_Doc_Print repDoc = new Rep_Doc_Print();
                string capname, reptype = "";
                capname = "Challan Printing"; reptype = "JOBISSUE";

                //switch (VE.MENU_PARA)
                //{
                //    case "KT": reptype = "YARNISSUE"; break;
                //    case "YD": reptype = "YARNISSUE"; break;
                //    case "FP": reptype = "FABISSUE"; break;
                //    case "DY": reptype = "FABISSUE"; break;
                //    case "BL": reptype = "FABISSUE"; break;
                //}

                repDoc.DOCCD = VE.T_TXN.DOCCD;
                repDoc.FDOCNO = VE.T_TXN.DOCNO;
                repDoc.TDOCNO = VE.T_TXN.DOCNO;
                repDoc.FDT = VE.T_TXN.DOCDT.ToString().retDateStr();
                repDoc.TDT = VE.T_TXN.DOCDT.ToString().retDateStr();
                repDoc.SLCD = VE.T_TXN.SLCD;
                repDoc.SLNM = VE.SLNM;
                repDoc.AskSlCd = true;
                repDoc.CaptionName = capname;
                repDoc.ActionName = "Rep_IssueChallan_Print";
                repDoc.RepType = reptype;
                repDoc.OtherPara = VE.MENU_PARA + "," + VE.T_TXN.GOCD;
                if (TempData["printparameter"] != null)
                {
                    TempData.Remove("printparameter");
                }
                TempData["printparameter"] = repDoc;
                return Content("");
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult SAVE(FormCollection FC, TransactionOutIssProcess VE)
        {
            Cn.getQueryString(VE);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            //Oracle Queries
            string dberrmsg = "";
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            OracleTransaction OraTrans;
            string dbsql = "", postdt = "", weekrem = "", duedatecalcon = "", sql = "";
            string[] dbsql1;
            double dbDrAmt = 0, dbCrAmt = 0;
            OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted);
            OraCmd.Transaction = OraTrans;
            DB.Configuration.ValidateOnSaveEnabled = false;

            try
            {
                //DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".T_CNTRL_HDR in  row share mode");
                OraCmd.CommandText = "lock table " + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode"; OraCmd.ExecuteNonQuery();
                String query = "";
                string dr = ""; string cr = ""; int isl = 0; string strrem = "";
                double igst = 0; double cgst = 0; double sgst = 0; double cess = 0; double duty = 0; double dbqty = 0; double dbamt = 0; double dbcurramt = 0;
                Int32 z = 0; Int32 maxR = 0;
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
                        TempData["LASTGOCD" + VE.MENU_PARA] = VE.T_TXN.GOCD;
                        TempData["LASTJOBCD" + VE.MENU_PARA] = VE.T_TXN.JOBCD;
                    }
                    else
                    {
                        TTXN.DOCCD = VE.T_TXN.DOCCD;
                        TTXN.DOCNO = VE.T_TXN.DOCNO;
                        TTXN.AUTONO = VE.T_TXN.AUTONO;
                        Month = VE.T_CNTRL_HDR.MNTHCD;
                        TTXN.EMD_NO = Convert.ToInt16((VE.T_CNTRL_HDR.EMD_NO == null ? 0 : VE.T_CNTRL_HDR.EMD_NO) + 1);
                        DOCPATTERN = VE.T_CNTRL_HDR.DOCNO;
                    }
                    //TTXN.DOCTAG = VE.MENU_PARA.retStr().Length > 2 ? VE.MENU_PARA.retStr().Remove(2) : VE.MENU_PARA.retStr();
                    TTXN.DOCTAG = "JC";
                    TTXN.SLCD = VE.T_TXN.SLCD;
                    //TTXN.CONSLCD = VE.T_TXN.CONSLCD;
                    //TTXN.CURR_CD = VE.T_TXN.CURR_CD;
                    //TTXN.CURRRT = VE.T_TXN.CURRRT;
                    //TTXN.BLAMT = VE.T_TXN.BLAMT;
                    //TTXN.PREFDT = VE.T_TXN.PREFDT;
                    //TTXN.PREFNO = VE.T_TXN.PREFNO;
                    //TTXN.REVCHRG = VE.T_TXN.REVCHRG;
                    //TTXN.ROAMT = VE.T_TXN.ROAMT;
                    //if (VE.RoundOff == true) { TTXN.ROYN = "Y"; } else { TTXN.ROYN = "N"; }

                    TTXN.JOBCD = VE.T_TXN.JOBCD;
                    TTXN.GOCD = VE.T_TXN.GOCD;
                    //TTXN.MANSLIPNO = VE.T_TXN.MANSLIPNO;
                    //TTXN.DUEDAYS = VE.T_TXN.DUEDAYS;
                    //TTXN.PARGLCD = VE.T_TXN.PARGLCD;
                    //TTXN.GLCD = VE.T_TXN.GLCD;
                    //TTXN.CLASS1CD = VE.T_TXN.CLASS1CD;
                    //TTXN.CLASS2CD = VE.T_TXN.CLASS2CD;
                    //TTXN.LINECD = VE.T_TXN.LINECD;
                    //TTXN.BARGENTYPE = VE.T_TXN.BARGENTYPE;
                    //TTXN.WPPER = VE.T_TXN.WPPER;
                    //TTXN.RPPER = VE.T_TXN.RPPER;
                    TTXN.MENU_PARA = VE.T_TXN.MENU_PARA;
                    //TTXN.TCSPER = VE.T_TXN.TCSPER;
                    //TTXN.TCSAMT = VE.T_TXN.TCSAMT;
                    TTXN.DTAG = (VE.DefaultAction == "E" ? "E" : null);


                    if (VE.DefaultAction == "E")
                    {
                        #region batch and detail data
                        dbsql = masterHelp.TblUpdt("t_kardtl", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = masterHelp.TblUpdt("t_batchdtl", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = masterHelp.TblUpdt("t_progbom", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_progdtl", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_progmast", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                        var comp = DB1.T_BATCHMST.Where(x => x.AUTONO == TTXN.AUTONO).OrderBy(s => s.AUTONO).ToList();
                        foreach (var v in comp)
                        {
                            if (!VE.TBATCHDTL.Where(s => s.BARNO == v.BARNO && s.SLNO == v.SLNO).Any())
                            {
                                dbsql = masterHelp.TblUpdt("t_batchmst", TTXN.AUTONO, "E", "S", "BARNO = '" + v.BARNO + "' and SLNO = '" + v.SLNO + "' ");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            }
                        }
                        dbsql = masterHelp.TblUpdt("t_txndtl", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        #endregion

                        dbsql = masterHelp.TblUpdt("t_txntrans", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_txnoth", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        //dbsql = MasterHelpFa.TblUpdt("t_vch_gst", TTXN.AUTONO, "E");
                        //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        //dbsql = MasterHelpFa.TblUpdt("t_txnamt", TTXN.AUTONO, "E");
                        //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_cntrl_hdr_rem", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc_dtl", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }


                        //dbsql = MasterHelpFa.TblUpdt("t_cntrl_doc_pass", TTXN.AUTONO, "E");
                        //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

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


                    //dbsql = MasterHelpFa.RetModeltoSql(TVCHGST,"A",CommVar.FinSchema(UNQSNO));
                    //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();



                    int COUNTER = 0;
                    string stkdrcr = "C";
                    string mtrljobcd = "";
                    string stktype = "F";

                    switch (VE.MENU_PARA)
                    {
                        case "DY":
                            stkdrcr = "C"; break;
                        case "PR":
                            stkdrcr = "C"; break;
                        case "ST":
                            stkdrcr = "C"; break;
                        case "EM":
                            stkdrcr = "C"; break;
                        case "JW":
                            stkdrcr = "C"; break;
                    }
                    string docbarcode = ""; string UNIQNO = salesfunc.retVchrUniqId(TTXN.DOCCD, TTXN.AUTONO);
                    sql = "select doccd,docbarcode from " + CommVar.CurSchema(UNQSNO) + ".m_doctype_bar where doccd='" + TTXN.DOCCD + "'";
                    DataTable dt = masterHelp.SQLquery(sql);
                    if (dt != null && dt.Rows.Count > 0) docbarcode = dt.Rows[0]["docbarcode"].retStr();

                    string lbatchini = "";
                    sql = "select lbatchini from " + CommVar.FinSchema(UNQSNO) + ".m_loca where loccd='" + CommVar.Loccd(UNQSNO) + "' and compcd='" + CommVar.Compcd(UNQSNO) + "'";
                    dt = masterHelp.SQLquery(sql);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        lbatchini = dt.Rows[0]["lbatchini"].retStr();
                    }
                    bool recoexist = false;
                    for (int i = 0; i <= VE.TPROGDTL.Count - 1; i++)
                    {
                        if (VE.TPROGDTL[i].SLNO != 0 && VE.TPROGDTL[i].ITCD != null && VE.TPROGDTL[i].QNTY.retDbl() != 0)
                        {
                            sql = "Select * from " + CommVar.CurSchema(UNQSNO) + ".t_batchmst where barno='" + VE.TPROGDTL[i].BARNO + "' ";
                            OraCmd.CommandText = sql; var OraReco = OraCmd.ExecuteReader();
                            if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();

                            if (recoexist == false)
                            {
                                string barno = salesfunc.GenerateBARNO(VE.TPROGDTL[i].ITCD, VE.TPROGDTL[i].CLRBARCODE, VE.TPROGDTL[i].SZBARCODE);

                                T_BATCHMST TBATCHMST = new T_BATCHMST();
                                TBATCHMST.EMD_NO = TTXN.EMD_NO;
                                TBATCHMST.CLCD = TTXN.CLCD;
                                TBATCHMST.DTAG = TTXN.DTAG;
                                TBATCHMST.TTAG = TTXN.TTAG;
                                TBATCHMST.SLNO = VE.TPROGDTL[i].SLNO; // ++COUNTERBATCH;
                                TBATCHMST.AUTONO = TTXN.AUTONO;
                                TBATCHMST.SLCD = TTXN.SLCD;
                                TBATCHMST.MTRLJOBCD = VE.TPROGDTL[i].MTRLJOBCD;
                                //TBATCHMST.STKTYPE = stktype;
                                TBATCHMST.JOBCD = TTXN.JOBCD;
                                TBATCHMST.BARNO = barno;
                                TBATCHMST.ITCD = VE.TPROGDTL[i].ITCD;
                                TBATCHMST.PARTCD = VE.TPROGDTL[i].PARTCD;
                                TBATCHMST.SIZECD = VE.TPROGDTL[i].SIZECD;
                                TBATCHMST.COLRCD = VE.TPROGDTL[i].COLRCD;
                                TBATCHMST.NOS = VE.TPROGDTL[i].NOS;
                                TBATCHMST.QNTY = VE.TPROGDTL[i].QNTY;
                                TBATCHMST.RATE = VE.TPROGDTL[i].RATE;
                                TBATCHMST.AMT = 0;
                                //TBATCHMST.FLAGMTR = VE.TPROGDTL[i].FLAGMTR;
                                TBATCHMST.MTRL_COST = 0;
                                //TBATCHMST.OTH_COST = VE.TPROGDTL[i].OTH_COST;
                                //TBATCHMST.ITREM = VE.TPROGDTL[i].ITREM;
                                //TBATCHMST.PDESIGN = VE.TPROGDTL[i].PDESIGN;
                                //TBATCHMST.ORGBATCHAUTONO = VE.TPROGDTL[i].ORGBATCHAUTONO;
                                //TBATCHMST.ORGBATCHSLNO = VE.TPROGDTL[i].ORGBATCHSLNO;
                                //TBATCHMST.DIA = VE.TPROGDTL[i].DIA;
                                TBATCHMST.CUTLENGTH = VE.TPROGDTL[i].CUTLENGTH;
                                //TBATCHMST.LOCABIN = VE.TPROGDTL[i].LOCABIN;
                                TBATCHMST.SHADE = VE.TPROGDTL[i].SHADE;
                                //TBATCHMST.MILLNM = VE.TPROGDTL[i].MILLNM;
                                //TBATCHMST.BATCHNO = VE.TPROGDTL[i].BATCHNO;
                                //TBATCHMST.ORDAUTONO = VE.TPROGDTL[i].ORDAUTONO;
                                //dbsql = masterHelp.RetModeltoSql(TBATCHMST);
                                dbsql = masterHelp.RetModeltoSql(TBATCHMST);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            }

                            COUNTER = COUNTER + 1;
                            T_PROGMAST TPROGMAST = new T_PROGMAST();
                            TPROGMAST.CLCD = TTXN.CLCD;
                            TPROGMAST.AUTONO = TTXN.AUTONO;
                            TPROGMAST.SLNO = VE.TPROGDTL[i].SLNO;
                            TPROGMAST.SLCD = TTXN.SLCD;
                            TPROGMAST.BARNO = VE.TPROGDTL[i].BARNO;
                            TPROGMAST.MTRLJOBCD = VE.TPROGDTL[i].MTRLJOBCD;
                            TPROGMAST.STKTYPE = stktype;
                            TPROGMAST.ITCD = VE.TPROGDTL[i].ITCD;
                            TPROGMAST.PARTCD = VE.TPROGDTL[i].PARTCD;
                            TPROGMAST.COLRCD = VE.TPROGDTL[i].COLRCD;
                            TPROGMAST.SIZECD = VE.TPROGDTL[i].SIZECD;
                            TPROGMAST.NOS = VE.TPROGDTL[i].NOS.retDcml();
                            TPROGMAST.QNTY = VE.TPROGDTL[i].QNTY.retDcml();
                            TPROGMAST.ITREMARK = VE.TPROGDTL[i].ITREMARK;
                            TPROGMAST.SHADE = VE.TPROGDTL[i].SHADE;
                            TPROGMAST.CUTLENGTH = VE.TPROGDTL[i].CUTLENGTH.retDcml();
                            TPROGMAST.JOBCD = TTXN.JOBCD;
                            //TPROGMAST.PROGUNIQNO = salesfunc.retVchrUniqId(TTXN.DOCCD, TTXN.AUTONO) + COUNTER.retStr();
                            TPROGMAST.PROGUNIQNO = salesfunc.TranBarcodeGenerate(TTXN.DOCCD, lbatchini, docbarcode, UNIQNO, (VE.TPROGDTL[i].SLNO));
                            if (VE.TPROGDTL[i].CheckedSample == true) TPROGMAST.SAMPLE = "Y"; else TPROGMAST.SAMPLE = "N";
                            dbsql = masterHelp.RetModeltoSql(TPROGMAST);
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                            T_PROGDTL TPROGDTL = new T_PROGDTL();
                            TPROGDTL.CLCD = TTXN.CLCD;
                            TPROGDTL.EMD_NO = TTXN.EMD_NO;
                            TPROGDTL.DTAG = TTXN.DTAG;
                            TPROGDTL.AUTONO = TTXN.AUTONO;
                            TPROGDTL.SLNO = VE.TPROGDTL[i].SLNO;
                            TPROGDTL.DOCCD = TTXN.DOCCD;
                            TPROGDTL.DOCDT = TTXN.DOCDT;
                            TPROGDTL.DOCNO = TTXN.DOCNO;
                            TPROGDTL.PROGAUTONO = TTXN.AUTONO;
                            TPROGDTL.PROGSLNO = VE.TPROGDTL[i].SLNO;
                            TPROGDTL.STKDRCR = stkdrcr;
                            TPROGDTL.NOS = VE.TPROGDTL[i].NOS == null ? 0 : VE.TPROGDTL[i].NOS.retDbl();
                            TPROGDTL.QNTY = VE.TPROGDTL[i].QNTY.retDbl();

                            dbsql = masterHelp.RetModeltoSql(TPROGDTL);
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                        }
                    }
                    if (VE.TPROGBOM != null)
                    {
                        for (int i = 0; i <= VE.TPROGBOM.Count - 1; i++)
                        {
                            if (VE.TPROGBOM[i].SLNO != 0 && VE.TPROGBOM[i].RSLNO != 0 && VE.TPROGBOM[i].ITCD != null)
                            {
                                COUNTER = COUNTER + 1;
                                T_PROGBOM TPROGBOM = new T_PROGBOM();
                                TPROGBOM.CLCD = TTXN.CLCD;
                                TPROGBOM.EMD_NO = TTXN.EMD_NO;
                                TPROGBOM.DTAG = TTXN.DTAG;
                                TPROGBOM.AUTONO = TTXN.AUTONO;
                                TPROGBOM.SLNO = VE.TPROGBOM[i].SLNO;
                                TPROGBOM.RSLNO = VE.TPROGBOM[i].RSLNO;
                                TPROGBOM.ITCD = VE.TPROGBOM[i].ITCD;
                                TPROGBOM.PARTCD = VE.TPROGBOM[i].PARTCD;
                                TPROGBOM.SIZECD = VE.TPROGBOM[i].SIZECD;
                                TPROGBOM.COLRCD = VE.TPROGBOM[i].COLRCD;
                                TPROGBOM.BOMQNTY = VE.TPROGBOM[i].BOMQNTY.retDcml();
                                TPROGBOM.EXTRAQNTY = VE.TPROGBOM[i].EXTRAQNTY.retDcml();
                                TPROGBOM.QNTY = VE.TPROGBOM[i].QQNTY.retDcml();
                                TPROGBOM.MTRLJOBCD = VE.TPROGBOM[i].MTRLJOBCD;
                                if (VE.TPROGBOM[i].Q_CheckedSample == true) TPROGBOM.SAMPLE = "Y"; else TPROGBOM.SAMPLE = "N";
                                dbsql = masterHelp.RetModeltoSql(TPROGBOM);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();


                            }
                        }
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
                    TTXNOTH.POREFNO = VE.T_TXNOTH.POREFNO;
                    TTXNOTH.POREFDT = VE.T_TXNOTH.POREFDT;
                    //----------------------------------------------------------//


                    #region batch and detail data
                    // SAVE T_CNTRL_HDR_UNIQNO

                    if (VE.DefaultAction == "A")
                    {
                        T_CNTRL_HDR_UNIQNO TCHUNIQNO = new T_CNTRL_HDR_UNIQNO();
                        TCHUNIQNO.CLCD = TTXN.CLCD;
                        TCHUNIQNO.AUTONO = TTXN.AUTONO;
                        TCHUNIQNO.UNIQNO = UNIQNO;
                        dbsql = masterHelp.RetModeltoSql(TCHUNIQNO);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    }

                    //END T_CNTRL_HDR_UNIQNO 

                    double _amtdist = 0, _baldist = 0, _rpldist = 0, _mult = 1;
                    double _amtdistq = 0, _baldistq = 0, _rpldistq = 0;
                    double titamt = 0, titqty = 0;
                    int lastitemno = 0;
                    VE.BALEYR = Convert.ToDateTime(CommVar.FinStartDate(UNQSNO)).Year.retStr();
                    if (VE.TTXNDTL != null && VE.TTXNDTL.Count > 0)
                    {
                        for (int i = 0; i <= VE.TTXNDTL.Count - 1; i++)
                        {
                            if (VE.TTXNDTL[i].SLNO != 0 && VE.TTXNDTL[i].ITCD != null)
                            {
                                titamt = titamt + VE.TTXNDTL[i].AMT.retDbl();
                                titqty = titqty + Convert.ToDouble(VE.TTXNDTL[i].QNTY);
                                lastitemno = i;
                            }
                        }
                        _baldist = _amtdist; _baldistq = _amtdistq;

                        for (int i = 0; i <= VE.TTXNDTL.Count - 1; i++)
                        {
                            if (VE.TTXNDTL[i].SLNO != 0 && VE.TTXNDTL[i].ITCD != null && VE.TTXNDTL[i].MTRLJOBCD != null && VE.TTXNDTL[i].STKTYPE != null)
                            {
                                if (i == lastitemno) { _rpldist = _baldist; _rpldistq = _baldistq; }
                                else
                                {
                                    if (_amtdist + _amtdistq == 0) { _rpldist = 0; _rpldistq = 0; }
                                    else
                                    {
                                        _rpldist = ((_amtdist / titamt) * VE.TTXNDTL[i].AMT).retDbl().toRound();
                                        _rpldistq = ((_amtdistq / titqty) * Convert.ToDouble(VE.TTXNDTL[i].QNTY)).toRound();
                                    }
                                }
                                _baldist = _baldist - _rpldist; _baldistq = _baldistq - _rpldistq;

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
                                TTXNDTL.BALEYR = VE.BALEYR;// VE.TTXNDTL[i].BALEYR;
                                                           //TTXNDTL.BALENO = VE.TTXNDTL[i].BALENO;
                                TTXNDTL.GOCD = VE.T_TXN.GOCD;
                                TTXNDTL.JOBCD = VE.TTXNDTL[i].JOBCD;
                                TTXNDTL.NOS = VE.TTXNDTL[i].NOS == null ? 0 : VE.TTXNDTL[i].NOS;
                                TTXNDTL.QNTY = VE.TTXNDTL[i].QNTY;
                                TTXNDTL.BLQNTY = VE.TTXNDTL[i].BLQNTY;
                                TTXNDTL.RATE = VE.TTXNDTL[i].RATE;
                                TTXNDTL.AMT = VE.TTXNDTL[i].AMT;
                                //TTXNDTL.FLAGMTR = VE.TTXNDTL[i].FLAGMTR;
                                TTXNDTL.TOTDISCAMT = VE.TTXNDTL[i].TOTDISCAMT;
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
                                TTXNDTL.OTHRAMT = _rpldist + _rpldistq;
                                TTXNDTL.AGDOCNO = VE.TTXNDTL[i].AGDOCNO;
                                TTXNDTL.AGDOCDT = VE.TTXNDTL[i].AGDOCDT;
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
                                TTXNDTL.PRCEFFDT = VE.TTXNDTL[i].PRCEFFDT;
                                TTXNDTL.GLCD = VE.TTXNDTL[i].GLCD;
                                TTXNDTL.CLASS1CD = VE.TTXNDTL[i].CLASS1CD;
                                dbsql = masterHelp.RetModeltoSql(TTXNDTL);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                dbqty = dbqty + Convert.ToDouble(VE.TTXNDTL[i].QNTY);
                                igst = igst + Convert.ToDouble(VE.TTXNDTL[i].IGSTAMT);
                                cgst = cgst + Convert.ToDouble(VE.TTXNDTL[i].CGSTAMT);
                                sgst = sgst + Convert.ToDouble(VE.TTXNDTL[i].SGSTAMT);
                                cess = cess + Convert.ToDouble(VE.TTXNDTL[i].CESSAMT);
                                duty = duty + Convert.ToDouble(VE.TTXNDTL[i].DUTYAMT);
                            }
                        }
                    }

                    COUNTER = 0; int COUNTERBATCH = 0; recoexist = false;
                    if (VE.TBATCHDTL != null && VE.TBATCHDTL.Count > 0)
                    {
                        for (int i = 0; i <= VE.TBATCHDTL.Count - 1; i++)
                        {
                            if (VE.TBATCHDTL[i].ITCD != null && VE.TBATCHDTL[i].QNTY != 0)
                            {
                                bool flagbatch = false;
                                string barno = "";
                                barno = VE.TBATCHDTL[i].BARNO;
                                sql = "Select * from " + CommVar.CurSchema(UNQSNO) + ".t_batchmst where barno='" + barno + "'";
                                OraCmd.CommandText = sql; var OraReco = OraCmd.ExecuteReader();
                                if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();

                                if (recoexist == false) flagbatch = true;


                                //checking barno exist or not
                                string Action = "", SqlCondition = "";
                                if (VE.DefaultAction == "A")
                                {
                                    Action = VE.DefaultAction;
                                }
                                else
                                {
                                    sql = "Select * from " + CommVar.CurSchema(UNQSNO) + ".t_batchmst where autono='" + TTXN.AUTONO + "' and slno = " + VE.TBATCHDTL[i].SLNO + " and barno='" + barno + "' ";
                                    OraCmd.CommandText = sql; OraReco = OraCmd.ExecuteReader();
                                    if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();

                                    if (recoexist == true)
                                    {
                                        Action = "E";
                                        SqlCondition = "autono = '" + TTXN.AUTONO + "' and slno = " + VE.TBATCHDTL[i].SLNO + " and barno='" + barno + "' ";
                                        flagbatch = true;
                                        barno = VE.TBATCHDTL[i].BARNO;
                                    }
                                    else
                                    {
                                        Action = "A";
                                    }

                                }

                                if (flagbatch == true)
                                {
                                    T_BATCHMST TBATCHMST = new T_BATCHMST();
                                    TBATCHMST.EMD_NO = TTXN.EMD_NO;
                                    TBATCHMST.CLCD = TTXN.CLCD;
                                    TBATCHMST.DTAG = TTXN.DTAG;
                                    TBATCHMST.TTAG = TTXN.TTAG;
                                    TBATCHMST.SLNO = VE.TBATCHDTL[i].SLNO; // ++COUNTERBATCH;
                                    TBATCHMST.AUTONO = TTXN.AUTONO;
                                    TBATCHMST.SLCD = TTXN.SLCD;
                                    TBATCHMST.MTRLJOBCD = VE.TBATCHDTL[i].MTRLJOBCD;
                                    //TBATCHMST.STKTYPE = VE.TBATCHDTL[i].STKTYPE;
                                    TBATCHMST.JOBCD = TTXN.JOBCD;
                                    TBATCHMST.BARNO = barno;
                                    TBATCHMST.ITCD = VE.TBATCHDTL[i].ITCD;
                                    TBATCHMST.PARTCD = VE.TBATCHDTL[i].PARTCD;
                                    TBATCHMST.SIZECD = VE.TBATCHDTL[i].SIZECD;
                                    TBATCHMST.COLRCD = VE.TBATCHDTL[i].COLRCD;
                                    TBATCHMST.NOS = VE.TBATCHDTL[i].NOS;
                                    TBATCHMST.QNTY = VE.TBATCHDTL[i].QNTY;
                                    TBATCHMST.RATE = VE.TBATCHDTL[i].RATE;
                                    //TBATCHMST.AMT = VE.TBATCHDTL[i].AMT;
                                    //TBATCHMST.FLAGMTR = VE.TBATCHDTL[i].FLAGMTR;
                                    //TBATCHMST.MTRL_COST = VE.TBATCHDTL[i].MTRL_COST;
                                    //TBATCHMST.OTH_COST = VE.TBATCHDTL[i].OTH_COST;
                                    TBATCHMST.ITREM = VE.TBATCHDTL[i].ITREM;
                                    TBATCHMST.PDESIGN = VE.TBATCHDTL[i].PDESIGN;
                                    if (VE.T_TXN.BARGENTYPE == "E")
                                    {
                                        TBATCHMST.HSNCODE = VE.TBATCHDTL[i].HSNCODE;
                                    }
                                    //TBATCHMST.ORGBATCHAUTONO = VE.TBATCHDTL[i].ORGBATCHAUTONO;
                                    //TBATCHMST.ORGBATCHSLNO = VE.TBATCHDTL[i].ORGBATCHSLNO;
                                    TBATCHMST.DIA = VE.TBATCHDTL[i].DIA;
                                    TBATCHMST.CUTLENGTH = VE.TBATCHDTL[i].CUTLENGTH;
                                    TBATCHMST.LOCABIN = VE.TBATCHDTL[i].LOCABIN;
                                    TBATCHMST.SHADE = VE.TBATCHDTL[i].SHADE;
                                    TBATCHMST.MILLNM = VE.TBATCHDTL[i].MILLNM;
                                    TBATCHMST.BATCHNO = VE.TBATCHDTL[i].BATCHNO;
                                    TBATCHMST.ORDAUTONO = VE.TBATCHDTL[i].ORDAUTONO;
                                    //dbsql = masterHelp.RetModeltoSql(TBATCHMST);
                                    dbsql = masterHelp.RetModeltoSql(TBATCHMST, Action, "", SqlCondition);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                                COUNTER = COUNTER + 1;
                                T_BATCHDTL TBATCHDTL = new T_BATCHDTL();
                                TBATCHDTL.EMD_NO = TTXN.EMD_NO;
                                TBATCHDTL.CLCD = TTXN.CLCD;
                                TBATCHDTL.DTAG = TTXN.DTAG;
                                TBATCHDTL.TTAG = TTXN.TTAG;
                                TBATCHDTL.AUTONO = TTXN.AUTONO;
                                TBATCHDTL.TXNSLNO = VE.TBATCHDTL[i].TXNSLNO;
                                TBATCHDTL.SLNO = VE.TBATCHDTL[i].SLNO;  //COUNTER.retShort();
                                TBATCHDTL.GOCD = VE.T_TXN.GOCD;
                                TBATCHDTL.BARNO = barno;
                                TBATCHDTL.MTRLJOBCD = VE.TBATCHDTL[i].MTRLJOBCD;
                                TBATCHDTL.PARTCD = VE.TBATCHDTL[i].PARTCD;
                                TBATCHDTL.HSNCODE = VE.TBATCHDTL[i].HSNCODE;
                                TBATCHDTL.STKDRCR = stkdrcr;
                                TBATCHDTL.NOS = VE.TBATCHDTL[i].NOS;
                                TBATCHDTL.QNTY = VE.TBATCHDTL[i].QNTY;
                                TBATCHDTL.BLQNTY = VE.TBATCHDTL[i].BLQNTY;
                                //TBATCHDTL.FLAGMTR = VE.TBATCHDTL[i].FLAGMTR;
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
                                TBATCHDTL.BALEYR = VE.BALEYR;// VE.TBATCHDTL[i].BALEYR;
                                                             //TBATCHDTL.BALENO = VE.TBATCHDTL[i].BALENO;
                                TBATCHDTL.RECPROGAUTONO = TTXN.AUTONO;
                                TBATCHDTL.RECPROGSLNO = VE.TBATCHDTL[i].RECPROGSLNO;
                                TBATCHDTL.STKTYPE = VE.TBATCHDTL[i].STKTYPE;
                                dbsql = masterHelp.RetModeltoSql(TBATCHDTL);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                T_KARDTL TKARDTL = new T_KARDTL();
                                TKARDTL.EMD_NO = TTXN.EMD_NO;
                                TKARDTL.CLCD = TTXN.CLCD;
                                TKARDTL.DTAG = TTXN.DTAG;
                                TKARDTL.TTAG = TTXN.TTAG;
                                TKARDTL.AUTONO = TTXN.AUTONO;
                                TKARDTL.SLNO = VE.TBATCHDTL[i].SLNO;
                                TKARDTL.RSLNO = VE.TBATCHDTL[i].SLNO;
                                TKARDTL.PROGAUTONO = TTXN.AUTONO;
                                TKARDTL.PROGSLNO = VE.TBATCHDTL[i].RECPROGSLNO;
                                TKARDTL.SLCD = TTXN.SLCD;
                                TKARDTL.STKDRCR = "D";
                                TKARDTL.BARNO = barno;
                                TKARDTL.MTRLJOBCD = VE.TBATCHDTL[i].MTRLJOBCD;
                                TKARDTL.STKTYPE = VE.TBATCHDTL[i].STKTYPE;
                                TKARDTL.ITCD = VE.TBATCHDTL[i].ITCD;
                                TKARDTL.PARTCD = VE.TBATCHDTL[i].PARTCD;
                                TKARDTL.SIZECD = VE.TBATCHDTL[i].SIZECD;
                                TKARDTL.COLRCD = VE.TBATCHDTL[i].COLRCD;
                                TKARDTL.NOS = VE.TBATCHDTL[i].NOS;
                                TKARDTL.CUTLENGTH = VE.TBATCHDTL[i].CUTLENGTH;
                                TKARDTL.QNTY = VE.TBATCHDTL[i].QNTY;
                                TKARDTL.RATE = VE.TBATCHDTL[i].RATE;
                                TKARDTL.SAMPLE = VE.TBATCHDTL[i].SAMPLE;
                                dbsql = masterHelp.RetModeltoSql(TKARDTL);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            }
                        }
                    }

                    if (dbqty == 0)
                    {
                        dberrmsg = "Quantity not entered"; goto dbnotsave;
                    }

                    #endregion
                    //  -----------------------DOCUMENT PASSING DATA---------------------------//
                    //double TRAN_AMT = Convert.ToDouble(TTXN.BLAMT);

                    //var TCDP_DATA = Cn.T_CONTROL_DOC_PASS(TTXN.DOCCD, TRAN_AMT, TTXN.EMD_NO.Value, TTXN.AUTONO, CommVar.CurSchema(UNQSNO).ToString());
                    //if (TCDP_DATA.Item1.Count != 0)
                    //{
                    //    for (int tr = 0; tr <= TCDP_DATA.Item1.Count - 1; tr++)
                    //    {
                    //        dbsql = MasterHelpFa.RetModeltoSql(TCDP_DATA.Item1[tr]);
                    //        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    //    }
                    //}
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
                        ContentFlg = "1" + " (Issue No. " + TTXN.DOCNO + ")~" + TTXN.AUTONO;
                    }
                    else if (VE.DefaultAction == "E")
                    {
                        ContentFlg = "2";
                    }
                    OraTrans.Commit();
                    OraCon.Dispose();
                    return Content(ContentFlg);
                }
                else if (VE.DefaultAction == "V")
                {
                    #region batch and detail data
                    ContentFlg = ChildRecordCheck(VE.T_TXN.AUTONO); if (ContentFlg != "") goto dbnotsave;
                    dbsql = masterHelp.TblUpdt("t_batchdtl", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                    if (VE.TBATCHDTL != null)
                    {
                        foreach (var v in VE.TBATCHDTL)
                        {
                            var IsTransactionFound = salesfunc.IsTransactionFound("", v.BARNO.retSqlformat(), VE.T_TXN.AUTONO.retSqlformat());
                            if (IsTransactionFound != "")
                            {
                                ContentFlg = "We cant delete this Bill. Transaction found at " + IsTransactionFound; goto dbnotsave;
                            }
                            if (IsTransactionFound == "")
                            {
                                dbsql = masterHelp.TblUpdt("t_batchmst", VE.T_TXN.AUTONO, "D", "", "barno='" + v.BARNO + "' and autono='" + VE.T_TXN.AUTONO + "'");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                            }
                        }
                    }

                    #endregion
                    //dbsql = MasterHelpFa.TblUpdt("t_cntrl_doc_pass", VE.T_TXN.AUTONO, "D");
                    //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc_dtl", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_cntrl_hdr_rem", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_cntrl_hdr_uniqno", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_kardtl", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_txndtl", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_progbom", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_progdtl", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_progmast", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }


                    dbsql = masterHelp.TblUpdt("T_TXNOTH", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("T_TXNTRANS", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_txn", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }


                    dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(VE.T_TXN.AUTONO, "D", "S", null, null, null, VE.T_TXN.DOCDT.retStr(), null, null, null);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();


                    ModelState.Clear();
                    OraTrans.Commit();
                    OraCon.Dispose();
                    return Content("3");
                }
                else
                {
                    return Content("");
                }
                dbnotsave:;
                OraTrans.Rollback();
                OraCon.Dispose();
                return Content(dberrmsg);
            }
            catch (Exception ex)
            {
                OraTrans.Rollback();
                OraCon.Dispose();
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult OpenPopUp(TransactionOutIssProcess VE)
        {
            DataTable PRODGRPDATA = new DataTable();
            int slno = 0;
            VE.DefaultView = true;
            return PartialView("_T_OUTISSPROCESS_POPUP", VE);
        }
        public ActionResult SelectCashMemoDetails(TransactionOutIssProcess VE)
        {
            try
            {
                Cn.getQueryString(VE); ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                var _TTXNDTLPOPUP = VE.CASHMEMOPOPUP.Where(r => r.C_Checked == true).ToList(); int slno = 0, SERIAL = 0;
                List<TPROGDTL> tsalePosReturnList = new List<TPROGDTL>();
                for (int i = VE.TPROGDTL.Count - 1; i >= 0; i--)
                {
                    if (VE.TPROGDTL[i].ITCD == null)
                        VE.TPROGDTL.RemoveAt(i);
                }
                foreach (var row in _TTXNDTLPOPUP)
                {
                    TPROGDTL tprgdtl = new TPROGDTL();
                    tprgdtl.BARNO = row.BARNO.retStr();
                    tprgdtl.ITGRPCD = row.ITGRPCD.retStr();
                    tprgdtl.ITGRPNM = row.ITGRPNM.retStr();
                    tprgdtl.BARNO = row.BARNO.retStr();
                    tprgdtl.ITCD = row.ITCD.retStr();
                    tprgdtl.ITNM = row.ITSTYLE.retStr();
                    tprgdtl.QNTY = row.QNTY.retDbl();
                    tprgdtl.UOM = row.UOM.retStr();
                    tprgdtl.DECIMALS = row.DECIMALS.retShort();
                    tprgdtl.MTRLJOBCD = row.MTRLJOBCD.retStr();
                    tprgdtl.MTRLJOBNM = row.MTRLJOBNM.retStr();
                    tprgdtl.PARTCD = row.PARTCD.retStr();
                    tprgdtl.COLRCD = row.COLRCD.retStr();
                    tprgdtl.COLRNM = row.COLRNM.retStr();
                    tprgdtl.CLRBARCODE = row.CLRBARCODE.retStr();
                    tprgdtl.SIZECD = row.SIZECD.retStr();
                    tprgdtl.SZBARCODE = row.SZBARCODE.retStr();
                    tprgdtl.NOS = row.NOS.retDbl();
                    tprgdtl.CUTLENGTH = row.CUTLENGTH.retDbl();
                    tprgdtl.SHADE = row.SHADE.retStr();
                    if (VE.TPROGDTL.Count == 0 || VE.TPROGDTL == null) VE.TPROGDTL = new List<TPROGDTL>();
                    VE.TPROGDTL.Add(tprgdtl);
                }
                if (VE.TPROGDTL.Count != 0 || VE.TPROGDTL != null)
                {
                    for (int i = 0; i < VE.TPROGDTL.Count; i++)
                    {
                        VE.TPROGDTL[i].SLNO = (1 + i).retShort();


                    }
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
            VE.DefaultView = true;
            ModelState.Clear();
            return PartialView("_T_OUTISSPROCESS_Programme", VE);

        }
        public ActionResult GetCashMemoNo(string val, String CODE)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                var data = CODE.Split(Convert.ToChar(Cn.GCS()));
                string str = "";
                string FDT = data[0].retSqlformat();
                string TDT = data[1].retSqlformat();
                string AUTONO = data[2].retStr();
                var sql = masterHelp.CashMemoNumber_help(val, FDT, TDT);
                if (sql.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", sql);
                }
                else
                {
                    var gcs = Cn.GCS();
                    var Getdata = (from h in DB.T_TXN
                                   join i in DB.T_TXNDTL on h.AUTONO equals (i.AUTONO)
                                   join
j in DB.T_BATCHDTL on i.AUTONO equals (j.AUTONO)
                                   join k in DB.M_SITEM on i.ITCD equals (k.ITCD)
                                   join l in DB.M_GROUP on k.ITGRPCD equals (l.ITGRPCD)
                                   join m in DB.M_MTRLJOBMST on i.MTRLJOBCD equals (m.MTRLJOBCD)
                                   where i.AUTONO == AUTONO
                                   select new
                                   {
                                       AUTONO = i.AUTONO,
                                       COLRCD = i.COLRCD,
                                       ITCD = i.ITCD,
                                       MTRLJOBCD = i.MTRLJOBCD,
                                       MTRLJOBNM = m.MTRLJOBNM,
                                       NOS = i.NOS,
                                       PARTCD = i.PARTCD,
                                       PRCCD = i.PRCCD,
                                       QNTY = i.QNTY,
                                       RATE = i.RATE,
                                       SIZECD = i.SIZECD,
                                       BARNO = j.BARNO,
                                       STYLENO = k.STYLENO,
                                       ITNM = k.ITNM,
                                       DOCNO = h.DOCNO,
                                       DOCDT = h.DOCDT,
                                       ITGRPCD = k.ITGRPCD,
                                       ITGRPNM = l.ITGRPNM,
                                       UOMCD = k.UOMCD

                                   }).ToList();
                    DataTable filterData = ListToDatatable.LINQResultToDataTable(Getdata);
                    if (filterData.Rows.Count > 0)
                    {
                        str = masterHelp.ToReturnFieldValues("", filterData);

                    }
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetFilterCashMemoDetails(TransactionOutIssProcess VE, string FDT, string TDT, string C_BARNO, string AUTONO)
        {
            Cn.getQueryString(VE); string scm = CommVar.CurSchema(UNQSNO);
            if (C_BARNO == null) C_BARNO = "";
            if (FDT == null) FDT = "";
            if (TDT == null) TDT = "";
            string sql = "select a.autono,b.docno,b.docdt,c.itcd,e.itnm,e.styleno,e.styleno||' '||e.itnm itstyle,e.itgrpcd,f.itgrpnm,c.qnty,e.uomcd,c.rate, ";
            sql += "d.barno,c.COLRCD,h.COLRNM,C.MTRLJOBCD,g.MTRLJOBNM from  " + scm + ".T_TXNMEMO a," + scm + ".T_TXN b, ";
            sql += "" + scm + ".T_TXNDTL c," + scm + ".T_BATCHDTL d ," + scm + ".M_SITEM e ," + scm + ".M_GROUP f, " + scm + ".M_MTRLJOBMST g, " + scm + ".M_COLOR h   ";
            sql += "where a.autono = b.autono and a.autono = c.autono(+) and c.autono = d.autono(+) ";
            sql += "and c.slno = d.txnslno(+)and c.itcd = e.itcd(+) and e.itgrpcd = f.itgrpcd(+) and c.mtrljobcd = g.mtrljobcd(+)and c.COLRCD = h.COLRCD(+)  ";
            if (FDT.retDateStr() != "") sql += "and b.docdt >= to_date('" + FDT + "', 'dd/mm/yyyy') ";
            if (TDT.retDateStr() != "") sql += " and b.docdt <= to_date('" + TDT + "', 'dd/mm/yyyy')  ";
            if (C_BARNO.retStr() != "") sql += "and d.barno = '" + C_BARNO + "' ";
            if (AUTONO.retStr() != "") sql += "and a.autono = '" + AUTONO + "' ";
            sql += "order by b.docdt, b.docno ";
            DataTable dt = masterHelp.SQLquery(sql);
            if (dt != null && dt.Rows.Count > 0)
            {

                VE.CASHMEMOPOPUP = (from DataRow dr in dt.Rows
                                    select new CASHMEMOPOPUP
                                    {
                                        ITCD = dr["itcd"].retStr(),
                                        BARNO = dr["barno"].retStr(),
                                        ITSTYLE = dr["itstyle"].retStr(),
                                        QNTY = dr["qnty"].retDbl(),
                                        AGDOCNO = dr["docno"].retStr(),
                                        AGDOCDT = dr["docdt"].retDateStr(),
                                        ITGRPCD = dr["itgrpcd"].retStr(),
                                        ITGRPNM = dr["itgrpnm"].retStr(),
                                        MTRLJOBCD = dr["MTRLJOBCD"].retStr(),
                                        MTRLJOBNM = dr["MTRLJOBNM"].retStr(),
                                        UOM = dr["uomcd"].retStr(),
                                        COLRCD = dr["COLRCD"].retStr(),
                                        COLRNM = dr["COLRNM"].retStr()
                                    }).ToList();
                int slno = 0;
                for (int p = 0; p <= VE.CASHMEMOPOPUP.Count - 1; p++)
                {
                    slno++;
                    VE.CASHMEMOPOPUP[p].SLNO = slno.retShort();
                }

            }


            VE.DefaultView = true;
            return PartialView("_T_OUTISSPROCESS_POPUP", VE);
        }
        private string ChildRecordCheck(string autono)
        {
            string message = "";
            string scm = CommVar.CurSchema(UNQSNO);
            string fcm = CommVar.FinSchema(UNQSNO);
            string sql = "";
            sql += "select a.autono,b.docno,b.docdt,c.docnm ";
            sql += "from " + scm + ".T_BALE a,  " + scm + ".t_cntrl_hdr b,  " + scm + ".m_doctype c ";
            sql += "where a.autono = B.AUTONO and b.doccd = c.DOCCD and a.blautono = '" + autono + "' ";
            sql += "and a.autono not in ('" + autono + "')";
            sql += "union all ";
            sql += "select a.autono,b.docno,b.docdt,c.docnm  ";
            sql += "from  " + fcm + ".T_vch_bl_adj a," + scm + ".t_cntrl_hdr b ," + scm + ".m_doctype c  ";
            sql += "where a.autono=B.AUTONO and b.doccd=c.DOCCD and  a.i_autono='" + autono + "'  ";
            DataTable dt = masterHelp.SQLquery(sql);
            if (dt.Rows.Count > 0)
            {
                message = "Clild record found at docno:" + dt.Rows[0]["docno"].ToString() + " docdt:" + dt.Rows[0]["docdt"].retDateStr() + " docnm:" + dt.Rows[0]["docnm"].ToString() + " autono:" + dt.Rows[0]["autono"].ToString() + " ";
                return message;
            }
            return message;
        }
        public ActionResult FillMaterialIssueData(TransactionOutIssProcess VE)
        {
            try
            {
                #region QtyRequirement
                VE.TBATCHDTL = (from a in VE.TPROGBOM
                                where a.ITCD.retStr() != ""
                                select new TBATCHDTL()
                                {
                                    SLNO = a.SLNO.retShort(),
                                    RECPROGSLNO = a.SLNO.retShort(),
                                    TXNSLNO= a.SLNO.retShort(),
                                    //QQNTY = a.QNTY.retDbl(),
                                    QNTY = a.BOMQNTY.retDbl(),
                                    RECPROGITSTYLE = a.ITNM.retStr(),
                                    //UOM = a.QUOM.retStr(),
                                    BARNO = a.BARNO.retStr(),
                                    ITGRPCD = a.ITGRPCD.retStr(),
                                    ITGRPNM = a.ITGRPNM.retStr(),
                                    ITCD = a.ITCD.retStr(),
                                    ITSTYLE = a.ITNM.retStr(),
                                    SIZECD = a.SIZECD.retStr(),
                                    SIZENM = a.SIZENM.retStr(),
                                    PARTCD = a.PARTCD.retStr(),
                                    PARTNM = a.PARTNM.retStr(),
                                    COLRCD = a.COLRCD.retStr(),
                                    COLRNM = a.COLRNM.retStr(),
                                    MTRLJOBCD = a.MTRLJOBCD.retStr(),
                                    MTRLJOBNM = a.MTRLJOBNM.retStr(),
                                    UOM = a.UOM.retStr(),
                                    BOMQNTY = a.BOMQNTY.retStr(),
                                    //EXTRAQNTY = a.extraqnty.retDbl(),
                                    SAMPLE = a.Q_SAMPLE.retStr(),
                                    STKTYPE = a.STKTYPE.retStr() == "" ? "F" : a.STKTYPE.retStr(),
                                }).OrderBy(s => s.SLNO).ToList();

                #endregion
                //fill prodgrpgstper in t_batchdtl
                DataTable allprodgrpgstper_data = new DataTable();
                string BARNO = (from a in VE.TBATCHDTL where a.BARNO.retStr() != "" select a.BARNO).ToArray().retSqlfromStrarray();
                string ITCD = (from a in VE.TBATCHDTL where a.ITCD.retStr() != "" select a.ITCD).ToArray().retSqlfromStrarray();
                //string MTRLJOBCD = (from a in VE.TBATCHDTL select a.MTRLJOBCD).ToArray().retSqlfromStrarray();
                string ITGRPCD = (from a in VE.TBATCHDTL where a.ITGRPCD.retStr() != "" select a.ITGRPCD).ToArray().retSqlfromStrarray();
                string autono = VE.T_TXN.AUTONO.retStr() == "" ? "" : VE.T_TXN.AUTONO.retSqlformat();
                allprodgrpgstper_data = salesfunc.GetStock(VE.T_TXN.DOCDT.retStr().Remove(10), VE.T_TXN.GOCD.retSqlformat(), BARNO.retStr(), ITCD.retStr(), "", autono, ITGRPCD, "", VE.T_TXNOTH.PRCCD.retStr(), VE.T_TXNOTH.TAXGRPCD.retStr(), "", "", true, true, "", "", false, false, true, "", true);


                //var MSYSCNFG = DB.M_SYSCNFG.OrderByDescending(t => t.EFFDT).FirstOrDefault();
                var MSYSCNFG = salesfunc.M_SYSCNFG(VE.T_TXN.DOCDT.retDateStr());
                foreach (var v in VE.TBATCHDTL)
                {
                    string PRODGRPGSTPER = "", ALL_GSTPER = "", GSTPER = "";
                    //v.GSTPER = VE.TTXNDTL.Where(a => a.SLNO == v.TXNSLNO).Sum(b => b.IGSTPER + b.CGSTPER + b.SGSTPER).retDbl();
                    if (allprodgrpgstper_data != null && allprodgrpgstper_data.Rows.Count > 0)
                    {
                        var DATA = allprodgrpgstper_data.Select("barno = '" + v.BARNO + "' and itcd= '" + v.ITCD + "' and itgrpcd = '" + v.ITGRPCD + "'");
                        if (DATA.Count() > 0)
                        {
                            DataTable tax_data = DATA.CopyToDataTable();
                            if (tax_data != null && tax_data.Rows.Count > 0)
                            {
                                v.BALSTOCK = tax_data.Rows[0]["BALQNTY"].retDbl();
                                PRODGRPGSTPER = tax_data.Rows[0]["PRODGRPGSTPER"].retStr();
                                if (PRODGRPGSTPER != "")
                                {
                                    ALL_GSTPER = salesfunc.retGstPer(PRODGRPGSTPER, v.RATE.retDbl());
                                    if (ALL_GSTPER.retStr() != "")
                                    {
                                        var gst = ALL_GSTPER.Split(',').ToList();
                                        GSTPER = (from a in gst select a.retDbl()).Sum().retStr();
                                    }
                                    v.PRODGRPGSTPER = PRODGRPGSTPER;
                                    v.ALL_GSTPER = ALL_GSTPER;
                                    v.GSTPER = GSTPER.retDbl();
                                }
                                if (VE.T_TXN.REVCHRG == "N")
                                {
                                    var a = PRODGRPGSTPER.Split('~')[0];
                                    var b = PRODGRPGSTPER.Split('~')[1];
                                    var c = PRODGRPGSTPER.Split('~')[2];
                                    var d = PRODGRPGSTPER.Split('~')[3];
                                    var e = PRODGRPGSTPER.Split('~')[4];
                                    var str = a + "~" + b + "~" + 0 + "~" + 0 + "~" + 0;
                                    v.PRODGRPGSTPER = str;
                                    v.ALL_GSTPER = 0 + "," + 0 + "," + 0;
                                    v.GSTPER = 0;
                                }
                                if (tax_data.Rows[0]["barimage"].retStr() != "")
                                {
                                    string barimage = tax_data.Rows[0]["barimage"].retStr();
                                    var brimgs = barimage.Split((char)179);
                                    v.BarImagesCount = brimgs.Length == 0 ? "" : brimgs.Length.retStr();
                                    foreach (var barimg in brimgs)
                                    {
                                        string barfilename = barimg.Split('~')[0];
                                        string barimgdesc = barimg.Split('~')[1];
                                        v.BarImages += (char)179 + CommVar.WebUploadDocURL(barfilename) + "~" + barimgdesc;
                                        string FROMpath = CommVar.SaveFolderPath() + "/ItemImages/" + barfilename;
                                        FROMpath = Path.Combine(FROMpath, "");
                                        string TOPATH = CommVar.LocalUploadDocPath() + barfilename;
                                        Cn.CopyImage(FROMpath, TOPATH);
                                    }
                                    v.BarImages = v.BarImages.retStr().TrimStart((char)179);
                                }
                            }
                        }
                    }

                    //checking childdata exist against barno
                    //var chk_child = (from a in DB.T_BATCHDTL where a.BARNO == v.BARNO && a.AUTONO != TXN.AUTONO select a).ToList();
                    //if (chk_child.Count() > 0)
                    //{
                    //    v.ChildData = "Y";
                    //}

                    var chk_child = ChildRecordCheck(VE.T_TXN.AUTONO);  //modify by mithun
                    if (chk_child != "")
                    {
                        v.ChildData = "Y";
                    }


                    //if ((VE.MENU_PARA == "PB") && ((TXN.BARGENTYPE == "E") || (TXN.BARGENTYPE == "C" && v.BARGENTYPE == "E")))
                    //{
                    //    v.WPRATE = v.RATE * VE.WPPER;
                    //    v.RPRATE = v.RATE * VE.WPPER;
                    //}

                    //if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "OP" || VE.MENU_PARA == "OTH" || VE.MENU_PARA == "PJRC")
                    //{

                    //    if (v.WPPRICEGEN.retStr() == "" || v.RPPRICEGEN.retStr() == "" || v.WPPER.retDbl() == 0 || v.RPPER.retDbl() == 0)
                    //    {
                    //        DataTable syscnfgdata = salesfunc.GetSyscnfgData(TXN.DOCDT.retDateStr());

                    //        if (v.WPPRICEGEN.retStr() == "")
                    //        {
                    //            if (syscnfgdata != null && syscnfgdata.Rows.Count > 0)
                    //            {
                    //                v.WPPRICEGEN = syscnfgdata.Rows[0]["wppricegen"].retStr();
                    //            }

                    //        }
                    //        if (v.RPPRICEGEN.retStr() == "")
                    //        {
                    //            if (syscnfgdata != null && syscnfgdata.Rows.Count > 0)
                    //            {
                    //                v.RPPRICEGEN = syscnfgdata.Rows[0]["rppricegen"].retStr();
                    //            }
                    //        }
                    //        if (v.WPPER.retDbl() == 0)
                    //        {
                    //            if (syscnfgdata != null && syscnfgdata.Rows.Count > 0)
                    //            {
                    //                v.WPPER = syscnfgdata.Rows[0]["WPPER"].retDbl();
                    //            }
                    //        }
                    //        if (v.RPPER.retDbl() == 0)
                    //        {
                    //            if (syscnfgdata != null && syscnfgdata.Rows.Count > 0)
                    //            {
                    //                v.RPPER = syscnfgdata.Rows[0]["RPPER"].retDbl();
                    //            }
                    //        }
                    //    }
                    //}
                }
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_OUTISSPROCESS_MaterialIssue", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
    }
}