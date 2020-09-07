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

namespace Improvar.Controllers
{
    public class T_BiltyG_MutiaController : Controller
    {
        // GET: T_BiltyG_Mutia
        Connection Cn = new Connection(); MasterHelp Master_Help = new MasterHelp(); MasterHelpFa MasterHelpFa = new MasterHelpFa(); SchemeCal Scheme_Cal = new SchemeCal(); Salesfunc salesfunc = new Salesfunc(); DataTable DT = new DataTable(); DataTable DTNEW = new DataTable();
        EmailControl EmailControl = new EmailControl();
        T_BILTY_HDR TBH; T_TXNTRANS TXNTRN; T_TXNOTH TXNOTH; T_CNTRL_HDR TCH; T_CNTRL_HDR_REM SLR;
        SMS SMS = new SMS();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: T_BiltyG_Mutia
        public ActionResult T_BiltyG_Mutia(string op = "", string key = "", int Nindex = 0, string searchValue = "", string parkID = "", string ThirdParty = "no")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Bilty Given to Mutia";
                    TransactionBiltyGMutiaEntry VE = (parkID == "") ? new TransactionBiltyGMutiaEntry() : (Improvar.ViewModels.TransactionBiltyGMutiaEntry)Session[parkID];
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);

                    string LOC = CommVar.Loccd(UNQSNO);
                    string COM = CommVar.Compcd(UNQSNO);
                    string YR1 = CommVar.YearCode(UNQSNO);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    VE.DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
                    //=================For Issue================//
                    //List<DropDownList1> ISSUE = new List<DropDownList1>();
                    //DropDownList1 ISSUE1 = new DropDownList1();
                    //ISSUE1.text = "";
                    //ISSUE1.Value = "";
                    //ISSUE.Add(ISSUE1);
                    //DropDownList1 ISSUE2 = new DropDownList1();
                    //ISSUE2.text = "Boys";
                    //ISSUE2.value = "B";
                    //ISSUE.Add(ISSUE2);
                    //VE.DropDownList1 = ISSUE;
                    //=================For Issue================//
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

                        VE.IndexKey = (from p in DB.T_BILTY_HDR
                                       join q in DB.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                       //join s in DB.T_PROGMAST on p.AUTONO equals (s.AUTONO)
                                       orderby q.DOCDT, q.DOCNO
                                       where XYZ.Contains(q.DOCCD) && q.LOCCD == LOC && q.COMPCD == COM && q.YR_CD == YR1
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
                            VE.T_BILTY_HDR = TBH;
                            //VE.T_TXNTRANS = TXNTRN;
                            //VE.T_TXNOTH = TXNOTH;
                            VE.T_CNTRL_HDR = TCH;
                            VE.T_CNTRL_HDR_REM = SLR;
                            if (VE.T_CNTRL_HDR.DOCNO != null) ViewBag.formname = ViewBag.formname + " (" + VE.T_CNTRL_HDR.DOCNO + ")";
                        }
                        if (op.ToString() == "A")
                        {
                            if (parkID == "")
                            {
                                T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                                TCH.DOCDT = Cn.getCurrentDate(VE.mindate);
                                //var jobcd = (from i in DB.M_JOBMST where i.JOBCD == VE.MENU_PARA select new { JOBCD = i.JOBCD, JOBNM = i.JOBNM }).OrderBy(s => s.JOBNM).FirstOrDefault();
                                //if (jobcd != null) TTXN.JOBCD = jobcd.JOBCD; VE.JOBNM = jobcd.JOBNM;
                                //VE.T_BILTY_HDR = TTXN;

                                //T_TXNOTH TXNOTH = new T_TXNOTH(); VE.T_TXNOTH = TXNOTH;

                                //VE.RoundOff = true;
                                List<TBILTY> TBILTY = new List<TBILTY>();
                                for (int i = 0; i <= 0; i++)
                                {
                                    TBILTY BILTY = new TBILTY();
                                    BILTY.SLNO = Convert.ToByte(i + 1);
                                    TBILTY.Add(BILTY);
                                    VE.TBILTY = TBILTY;
                                }
                                VE.TBILTY = TBILTY;
                                List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                                UploadDOC UPL = new UploadDOC();
                                UPL.DocumentType = Cn.DOC_TYPE();
                                UploadDOC1.Add(UPL);
                                VE.UploadDOC = UploadDOC1;

                            }
                            else
                            {
                                if (VE.UploadDOC != null) { foreach (var i in VE.UploadDOC) { i.DocumentType = Cn.DOC_TYPE(); } }
                                INI INIF = new INI();
                                INIF.DeleteKey(Session["UR_ID"].ToString(), parkID, Server.MapPath("~/Park.ini"));
                            }
                            VE = (TransactionBiltyGMutiaEntry)Cn.CheckPark(VE, VE.MenuID, VE.MenuIndex, LOC, COM, CommVar.CurSchema(UNQSNO).ToString(), Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString());
                        }
                    }
                    else
                    {
                        VE.DefaultView = false;
                        VE.DefaultDay = 0;
                    }
                    string docdt = "";
                    if (TCH != null) if (TCH.DOCDT != null) docdt = TCH.DOCDT.ToString().Remove(10);
                    Cn.getdocmaxmindate(VE.T_CNTRL_HDR.DOCCD, docdt, VE.DefaultAction, VE.T_CNTRL_HDR.DOCNO, VE);
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                TransactionBiltyGMutiaEntry VE = new TransactionBiltyGMutiaEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message;
                return View(VE);
            }
        }
        public TransactionBiltyGMutiaEntry Navigation(TransactionBiltyGMutiaEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            string COM_CD = CommVar.Compcd(UNQSNO);
            string LOC_CD = CommVar.Loccd(UNQSNO);
            string DATABASE = CommVar.CurSchema(UNQSNO).ToString();
            string DATABASEF = CommVar.FinSchema(UNQSNO);
            Cn.getQueryString(VE);

            TBH = new T_BILTY_HDR(); TXNTRN = new T_TXNTRANS(); TXNOTH = new T_TXNOTH(); TCH = new T_CNTRL_HDR(); SLR = new T_CNTRL_HDR_REM();
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
                TBH = DB.T_BILTY_HDR.Find(aa[0].Trim());
                TCH = DB.T_CNTRL_HDR.Find(TBH.AUTONO);
                //if (TBH.ROYN == "Y") VE.RoundOff = true;
                //else VE.RoundOff = false;
                //TXNTRN = DB.T_TXNTRANS.Find(TBH.AUTONO);
                //TXNOTH = DB.T_TXNOTH.Find(TBH.AUTONO);
                if (TBH.MUTSLCD.retStr() != "")
                {
                    string slcd = TBH.MUTSLCD;
                    var subleg = (from a in DBF.M_SUBLEG where a.SLCD == slcd select new { a.SLNM, a.REGMOBILE }).FirstOrDefault();
                    VE.SLNM = subleg.SLNM;
                    VE.REGMOBILE = subleg.REGMOBILE.ToString();
                }
                //VE.JOBNM = TBH.JOBCD.retStr() == "" ? "" : DB.M_JOBMST.Where(a => a.JOBCD == TBH.JOBCD).Select(b => b.JOBNM).FirstOrDefault();
                //VE.CONSLNM = TBH.CONSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TBH.CONSLCD).Select(b => b.SLNM).FirstOrDefault();
                //VE.AGSLNM = TXNOTH.AGSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXNOTH.AGSLCD).Select(b => b.SLNM).FirstOrDefault();
                //VE.SAGSLNM = TXNOTH.SAGSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXNOTH.SAGSLCD).Select(b => b.SLNM).FirstOrDefault();
                //VE.GONM = TBH.GOCD.retStr() == "" ? "" : DB.M_GODOWN.Where(a => a.GOCD == TBH.GOCD).Select(b => b.GONM).FirstOrDefault();
                //VE.PRCNM = TXNOTH.PRCCD.retStr() == "" ? "" : DBF.M_PRCLST.Where(a => a.PRCCD == TXNOTH.PRCCD).Select(b => b.PRCNM).FirstOrDefault();

                //VE.TRANSLNM = TXNTRN.TRANSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXNTRN.TRANSLCD).Select(b => b.SLNM).FirstOrDefault();
                //VE.CRSLNM = TXNTRN.CRSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXNTRN.CRSLCD).Select(b => b.SLNM).FirstOrDefault();
                SLR = Cn.GetTransactionReamrks(CommVar.CurSchema(UNQSNO).ToString(), TBH.AUTONO);
                VE.UploadDOC = Cn.GetUploadImageTransaction(CommVar.CurSchema(UNQSNO).ToString(), TBH.AUTONO);
                string Scm = CommVar.CurSchema(UNQSNO); double TOTAL_NOS = 0; double TOTAL_QNTY = 0; double TOTAL_BOMQNTY = 0; double TOTAL_EXTRAQNTY = 0; double TOTAL_QQNTY = 0;
                string str = "";
                str += "select a.autono,a.slno,a.nos,a.qnty,a.itcd,a.sizecd,a.partcd,a.colrcd,a.mtrljobcd,k.itgrpcd,k.uomcd,k.styleno,itgrpnm,k.itnm,l.sizenm,m.colrnm,p.partnm,o.mtrljobnm, ";
                str += "a.itremark,a.shade,a.cutlength,a.sample, k.itnm||' '||k.styleno itstyle from " + Scm + ".T_PROGMAST a," + Scm + ".T_PROGDTL b ,";
                str += Scm + ".M_SITEM k, " + Scm + ".M_SIZE l, " + Scm + ".M_COLOR m, ";
                str += Scm + ".M_GROUP n," + Scm + ".M_MTRLJOBMST o," + Scm + ".M_PARTS p ";
                str += " where a.autono=b.autono(+) and a.slno=b.slno(+) and a.ITCD = k.ITCD(+) ";
                str += " and a.SIZECD = l.SIZECD(+) and a.COLRCD = m.COLRCD(+) and k.ITGRPCD=n.ITGRPCD(+) and ";
                str += " a.MTRLJOBCD=o.MTRLJOBCD(+) and a.PARTCD=p.PARTCD(+) and a.autono='" + TBH.AUTONO + "'";
                str += "order by a.slno ";

                DataTable Progdtltbl = Master_Help.SQLquery(str);
                //VE.TPROGDTL = (from DataRow dr in Progdtltbl.Rows
                //               select new TPROGDTL()
                //               {
                //                   SLNO = Convert.ToInt16(dr["slno"]),
                //                   NOS = dr["nos"].retDbl(),
                //                   QNTY = dr["qnty"].retDbl(),
                //                   ITGRPCD = dr["itgrpcd"].ToString(),
                //                   ITGRPNM = dr["itgrpnm"].ToString(),
                //                   ITCD = dr["itcd"].ToString(),
                //                   ITNM = dr["itstyle"].ToString(),
                //                   UOM = dr["uomcd"].ToString(),
                //                   SIZECD = dr["sizecd"].retStr(),
                //                   SIZENM = dr["sizenm"].retStr(),
                //                   PARTCD = dr["partcd"].retStr(),
                //                   PARTNM = dr["partnm"].retStr(),
                //                   COLRCD = dr["colrcd"].retStr(),
                //                   COLRNM = dr["colrnm"].retStr(),
                //                   MTRLJOBCD = dr["mtrljobcd"].retStr(),
                //                   MTRLJOBNM = dr["mtrljobnm"].retStr(),
                //                   ITREMARK = dr["itremark"].retStr(),
                //                   SHADE = dr["shade"].retStr(),
                //                   CUTLENGTH = dr["cutlength"].retDbl(),
                //                   SAMPLE = dr["sample"].retStr()
                //               }).OrderBy(s => s.SLNO).ToList();
                //foreach (var q in VE.TPROGDTL)
                //{
                //    if (q.SAMPLE == "Y") q.CheckedSample = true;
                //    TOTAL_NOS = TOTAL_NOS + (q.NOS == null ? 0 : q.NOS.Value);
                //    TOTAL_QNTY = TOTAL_QNTY + (q.QNTY == null ? 0 : q.QNTY.Value);
                //}
                //VE.T_NOS = TOTAL_NOS.retInt();
                //VE.T_QNTY = TOTAL_QNTY.retInt();


                string str2 = "";
                str2 += "select a.autono,a.slno,a.rslno,a.qnty,a.bomqnty,a.extraqnty,a.itcd,a.sizecd,a.partcd,a.colrcd,a.mtrljobcd,k.itgrpcd,n.itgrpnm, ";
                str2 += " k.itnm,l.sizenm,m.colrnm,p.partnm,o.mtrljobnm,k.uomcd,b.qnty qntyMst, ";
                str2 += "a.sample,k.itnm||' '||k.styleno itstyle from " + Scm + ".T_PROGBOM a," + Scm + ".T_PROGMAST b ,";
                str2 += Scm + ".M_SITEM k, " + Scm + ".M_SIZE l, " + Scm + ".M_COLOR m, ";
                str2 += Scm + ".M_GROUP n," + Scm + ".M_MTRLJOBMST o," + Scm + ".M_PARTS p ";
                str2 += " where a.autono=b.autono(+) and a.slno=b.slno(+) and a.ITCD = k.ITCD(+)  ";
                str2 += " and a.SIZECD = l.SIZECD(+) and a.COLRCD = m.COLRCD(+) and k.ITGRPCD=n.ITGRPCD(+) and ";
                str2 += " a.MTRLJOBCD=o.MTRLJOBCD(+) and a.PARTCD=p.PARTCD(+) and a.autono='" + TBH.AUTONO + "'";
                str2 += "order by a.slno ";

                DataTable Progbom = Master_Help.SQLquery(str2);
                //VE.TPROGBOM = (from DataRow dr in Progbom.Rows
                //               select new TPROGBOM()
                //               {
                //                   SLNO = Convert.ToInt16(dr["slno"]),
                //                   RSLNO = Convert.ToInt16(dr["rslno"]),
                //                   QQNTY = dr["qntyMst"].retDbl(),
                //                   QNTY = dr["qnty"].retDbl(),
                //                   ITGRPCD = dr["itgrpcd"].ToString(),
                //                   ITGRPNM = dr["itgrpnm"].ToString(),
                //                   ITCD = dr["itcd"].ToString(),
                //                   ITNM = dr["itstyle"].ToString(),
                //                   SIZECD = dr["sizecd"].retStr(),
                //                   SIZENM = dr["sizenm"].retStr(),
                //                   PARTCD = dr["partcd"].retStr(),
                //                   PARTNM = dr["partnm"].retStr(),
                //                   COLRCD = dr["colrcd"].retStr(),
                //                   COLRNM = dr["colrnm"].retStr(),
                //                   MTRLJOBCD = dr["mtrljobcd"].retStr(),
                //                   MTRLJOBNM = dr["mtrljobnm"].retStr(),
                //                   UOM = dr["uomcd"].retStr(),
                //                   BOMQNTY = dr["bomqnty"].retDbl(),
                //                   EXTRAQNTY = dr["extraqnty"].retDbl(),
                //                   Q_SAMPLE = dr["sample"].retStr()
                //               }).OrderBy(s => s.SLNO).ToList();
                //foreach (var q in VE.TPROGBOM)
                //{
                //    if (q.Q_SAMPLE == "Y") q.Q_CheckedSample = true;
                //    TOTAL_QQNTY = TOTAL_QQNTY + (q.QQNTY == null ? 0 : q.QQNTY.Value);
                //    TOTAL_BOMQNTY = TOTAL_BOMQNTY + (q.BOMQNTY == null ? 0 : q.BOMQNTY.Value);
                //    TOTAL_EXTRAQNTY = TOTAL_EXTRAQNTY + (q.EXTRAQNTY == null ? 0 : q.EXTRAQNTY.Value);
                //}
                //VE.T_QQNTY = TOTAL_QQNTY.retInt();
                //VE.T_BOMQNTY = TOTAL_BOMQNTY.retInt();
                //VE.T_EXTRAQNTY = TOTAL_EXTRAQNTY.retInt();



                string str1 = "";
                str1 += "select i.SLNO,i.TXNSLNO,k.ITGRPCD,n.ITGRPNM,i.MTRLJOBCD,o.MTRLJOBNM,k.ITCD,k.ITNM,k.UOMCD,k.STYLENO,i.PARTCD,p.PARTNM,q.STKTYPE,r.STKNAME,i.BARNO, ";
                str1 += "j.COLRCD,m.COLRNM,j.SIZECD,l.SIZENM,i.SHADE,i.QNTY,i.NOS,i.RATE,i.DISCRATE,i.DISCTYPE,i.TDDISCRATE,i.TDDISCTYPE,i.SCMDISCTYPE,i.SCMDISCRATE ";
                str1 += "from " + Scm + ".T_BATCHDTL i, " + Scm + ".M_SITEM_BARCODE j, " + Scm + ".M_SITEM k, " + Scm + ".M_SIZE l, " + Scm + ".M_COLOR m, ";
                str1 += Scm + ".M_GROUP n," + Scm + ".M_MTRLJOBMST o," + Scm + ".M_PARTS p," + Scm + ".T_BATCHMST q," + Scm + ".M_STKTYPE r ";
                str1 += "where i.BARNO = j.BARNO(+) and j.ITCD = k.ITCD(+) and j.SIZECD = l.SIZECD(+) and j.COLRCD = m.COLRCD(+) and k.ITGRPCD=n.ITGRPCD(+) ";
                str1 += "and i.MTRLJOBCD=o.MTRLJOBCD(+) and i.PARTCD=p.PARTCD(+) and i.BARNO=q.BARNO(+) and q.STKTYPE=r.STKTYPE(+) ";
                str1 += "and i.AUTONO = '" + TBH.AUTONO + "' ";
                str1 += "order by i.SLNO ";
                DataTable tbl = Master_Help.SQLquery(str1);

                //VE.TBATCHDTL = (from DataRow dr in tbl.Rows
                //                select new TBATCHDTL()
                //                {
                //                    SLNO = dr["SLNO"].retShort(),
                //                    TXNSLNO = dr["TXNSLNO"].retShort(),
                //                    ITGRPCD = dr["ITGRPCD"].retStr(),
                //                    ITGRPNM = dr["ITGRPNM"].retStr(),
                //                    MTRLJOBCD = dr["MTRLJOBCD"].retStr(),
                //                    MTRLJOBNM = dr["MTRLJOBNM"].retStr(),
                //                    ITCD = dr["ITCD"].retStr(),
                //                    ITNM = dr["ITNM"].retStr(),
                //                    UOM = dr["UOMCD"].retStr(),
                //                    STYLENO = dr["STYLENO"].retStr(),
                //                    PARTCD = dr["PARTCD"].retStr(),
                //                    PARTNM = dr["PARTNM"].retStr(),
                //                    COLRCD = dr["COLRCD"].retStr(),
                //                    COLRNM = dr["COLRNM"].retStr(),
                //                    SIZECD = dr["SIZECD"].retStr(),
                //                    SIZENM = dr["SIZENM"].retStr(),
                //                    SHADE = dr["SHADE"].retStr(),
                //                    QNTY = dr["QNTY"].retDbl(),
                //                    NOS = dr["NOS"].retDbl(),
                //                    RATE = dr["RATE"].retDbl(),
                //                    DISCRATE = dr["DISCRATE"].retDbl(),
                //                    DISCTYPE = dr["DISCTYPE"].retStr(),
                //                    DISCTYPE_DESC = dr["DISCTYPE"].retStr() == "P" ? "%" : dr["DISCTYPE"].retStr() == "N" ? "Nos" : dr["DISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
                //                    TDDISCRATE = dr["TDDISCRATE"].retDbl(),
                //                    TDDISCTYPE = dr["TDDISCTYPE"].retStr(),
                //                    SCMDISCTYPE = dr["SCMDISCTYPE"].retStr(),
                //                    SCMDISCRATE = dr["SCMDISCRATE"].retDbl(),
                //                    STKTYPE = dr["STKTYPE"].retStr(),
                //                    STKNAME = dr["STKNAME"].retStr(),
                //                    BARNO = dr["BARNO"].retStr(),
                //                }).OrderBy(s => s.SLNO).ToList();

                str1 = "";
                str1 += "select i.SLNO,j.ITGRPCD,k.ITGRPNM,i.MTRLJOBCD,l.MTRLJOBNM,i.ITCD,j.ITNM,j.UOMCD,i.STKTYPE,n.STKNAME,i.NOS,i.QNTY,i.FLAGMTR, ";
                str1 += "i.BLQNTY,i.RATE,i.AMT,i.DISCTYPE,i.DISCRATE,i.DISCAMT,i.TDDISCTYPE,i.TDDISCRATE,i.TDDISCAMT,i.SCMDISCTYPE,i.SCMDISCRATE,i.SCMDISCAMT, ";
                str1 += "i.TXBLVAL,i.IGSTPER,i.CGSTPER,i.SGSTPER,i.CESSPER,i.IGSTAMT,i.CGSTAMT,i.SGSTAMT,i.CESSAMT,i.NETAMT,i.HSNCODE,i.BALENO,i.GLCD ";
                str1 += "from " + Scm + ".T_TXNDTL i, " + Scm + ".M_SITEM j, " + Scm + ".M_GROUP k, " + Scm + ".M_MTRLJOBMST l, " + Scm + ".M_MTRLJOBMST m, ";
                str1 += Scm + ".M_STKTYPE n ";
                str1 += "where i.ITCD = j.ITCD(+) and j.ITGRPCD=k.ITGRPCD(+) and i.MTRLJOBCD=l.MTRLJOBCD(+)  and i.STKTYPE=n.STKTYPE(+)  ";
                str1 += "and i.AUTONO = '" + TBH.AUTONO + "' ";
                str1 += "order by i.SLNO ";
                tbl = Master_Help.SQLquery(str1);

                //    VE.TTXNDTL = (from DataRow dr in tbl.Rows
                //                  select new TTXNDTL()//
                //                  {
                //                      SLNO = dr["SLNO"].retShort(),
                //                      ITGRPCD = dr["ITGRPCD"].retStr(),
                //                      ITGRPNM = dr["ITGRPNM"].retStr(),
                //                      MTRLJOBCD = dr["MTRLJOBCD"].retStr(),
                //                      MTRLJOBNM = dr["MTRLJOBNM"].retStr(),
                //                      ITCD = dr["ITCD"].retStr(),
                //                      ITNM = dr["ITNM"].retStr(),
                //                      UOM = dr["UOMCD"].retStr(),
                //                      STKTYPE = dr["STKTYPE"].retStr(),
                //                      STKNAME = dr["STKNAME"].retStr(),
                //                      NOS = dr["NOS"].retDbl(),
                //                      QNTY = dr["QNTY"].retDbl(),
                //                      FLAGMTR = dr["FLAGMTR"].retDbl(),
                //                      BLQNTY = dr["BLQNTY"].retDbl(),
                //                      RATE = dr["RATE"].retDbl(),
                //                      AMT = dr["AMT"].retDbl(),
                //                      DISCTYPE = dr["DISCTYPE"].retStr(),
                //                      DISCTYPE_DESC = dr["DISCTYPE"].retStr() == "P" ? "%" : dr["DISCTYPE"].retStr() == "N" ? "Nos" : dr["DISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
                //                      DISCRATE = dr["DISCRATE"].retDbl(),
                //                      DISCAMT = dr["DISCAMT"].retDbl(),
                //                      TDDISCTYPE = dr["TDDISCTYPE"].retStr(),
                //                      TDDISCRATE = dr["TDDISCRATE"].retDbl(),
                //                      TDDISCAMT = dr["TDDISCAMT"].retDbl(),
                //                      SCMDISCTYPE = dr["SCMDISCTYPE"].retStr(),
                //                      SCMDISCRATE = dr["SCMDISCRATE"].retDbl(),
                //                      SCMDISCAMT = dr["SCMDISCAMT"].retDbl(),
                //                      TXBLVAL = dr["TXBLVAL"].retDbl(),
                //                      IGSTPER = dr["IGSTPER"].retDbl(),
                //                      CGSTPER = dr["CGSTPER"].retDbl(),
                //                      SGSTPER = dr["SGSTPER"].retDbl(),
                //                      CESSPER = dr["CESSPER"].retDbl(),
                //                      IGSTAMT = dr["IGSTAMT"].retDbl(),
                //                      CGSTAMT = dr["CGSTAMT"].retDbl(),
                //                      SGSTAMT = dr["SGSTAMT"].retDbl(),
                //                      CESSAMT = dr["CESSAMT"].retDbl(),
                //                      NETAMT = dr["NETAMT"].retDbl(),
                //                      HSNCODE = dr["HSNCODE"].retStr(),
                //                      BALENO = dr["BALENO"].retStr(),
                //                      GLCD = dr["GLCD"].retStr()
                //                  }).OrderBy(s => s.SLNO).ToList();


            }
            //Cn.DateLock_Entry(VE, DB, TCH.DOCDT.Value);
            if (TCH.CANCEL == "Y") VE.CancelRecord = true; else VE.CancelRecord = false;
            return VE;
        }
        public ActionResult SearchPannelData(TransactionBiltyGMutiaEntry VE, string SRC_SLCD, string SRC_DOCNO, string SRC_FDT, string SRC_TDT)
        {
            string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), yrcd = CommVar.YearCode(UNQSNO);
            Cn.getQueryString(VE);

            List<DocumentType> DocumentType = new List<DocumentType>();
            DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
            string doccd = DocumentType.Select(i => i.value).ToArray().retSqlfromStrarray();
            string sql = "";

            sql = "select a.autono, b.docno, to_char(b.docdt,'dd/mm/yyyy') docdt, b.doccd, a.slcd, c.slnm, c.district,c.gstno,a.jobcd,d.jobnm, nvl(a.blamt,0) blamt ";
            sql += "from " + scm + ".T_BILTY_HDR a, " + scm + ".t_cntrl_hdr b, " + scmf + ".m_subleg c ," + scm + ".m_jobmst d ";
            sql += "where a.autono=b.autono and a.slcd=c.slcd(+) and a.jobcd=d.jobcd(+) and b.doccd in (" + doccd + ") and ";
            if (SRC_FDT.retStr() != "") sql += "b.docdt >= to_date('" + SRC_FDT.retDateStr() + "','dd/mm/yyyy') and ";
            if (SRC_TDT.retStr() != "") sql += "b.docdt <= to_date('" + SRC_TDT.retDateStr() + "','dd/mm/yyyy') and ";
            if (SRC_DOCNO.retStr() != "") sql += "(b.vchrno like '%" + SRC_DOCNO.retStr() + "%' or b.docno like '%" + SRC_DOCNO.retStr() + "%') and ";
            if (SRC_SLCD.retStr() != "") sql += "(a.slcd like '%" + SRC_SLCD.retStr() + "%' or upper(c.slnm) like '%" + SRC_SLCD.retStr().ToUpper() + "%') and ";
            sql += "b.loccd='" + LOC + "' and b.compcd='" + COM + "' and b.yr_cd='" + yrcd + "' ";
            sql += "order by docdt, docno ";
            DataTable tbl = Master_Help.SQLquery(sql);

            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Document Number" + Cn.GCS() + "Document Date" + Cn.GCS() + "Party Name" + Cn.GCS() + "Job Name" + Cn.GCS() + "AUTO NO";
            for (int j = 0; j <= tbl.Rows.Count - 1; j++)
            {
                SB.Append("<tr><td><b>" + tbl.Rows[j]["docno"] + "</b> [" + tbl.Rows[j]["doccd"] + "]" + " </td><td>" + tbl.Rows[j]["docdt"] + " </td><td><b>" + tbl.Rows[j]["slnm"] + "</b> [" + tbl.Rows[j]["district"] + "][" + tbl.Rows[j]["gstno"] + "] (" + tbl.Rows[j]["slcd"] + ") </td><td>" + tbl.Rows[j]["jobnm"] + "(" + tbl.Rows[j]["jobcd"] + ") </td><td>" + tbl.Rows[j]["autono"] + " </td></tr>");
            }
            return PartialView("_SearchPannel2", Master_Help.Generate_SearchPannel(hdr, SB.ToString(), "4", "4"));
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
                var str = Master_Help.JOBCD_help(val);
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
        public ActionResult GetSubLedgerDetails(string val)
        {
            try
            {
                var str = Master_Help.SLCD_help(val);
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

        public ActionResult GetGodownDetails(string val)
        {
            try
            {
                var str = Master_Help.GOCD_help(val);
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
                var str = Master_Help.PRCCD_help(val);
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
                string str = Master_Help.ITGRPCD_help(val, "");
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
                string str = Master_Help.MTRLJOBCD_help(val);
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
                var str = Master_Help.ITCD_help(val, "", Code);
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
                var str = Master_Help.PARTCD_help(val);
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
                var str = Master_Help.COLRCD_help(val);
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
                var str = Master_Help.MTRLJOBCD_help(val);
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
                var str = Master_Help.SIZECD_help(val);
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
                var str = Master_Help.STKTYPE_help(val);
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
                var str = Master_Help.LOCABIN_help(val);
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
        public ActionResult GetBarCodeDetails(string val)
        {
            try
            {
                var str = Master_Help.BARCODE_help(val);
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
        public ActionResult GetPendingData(TransactionBiltyGMutiaEntry VE, string DOCDT)
        {
            try
            {
                var GetPendig_Data = salesfunc.getPendBiltytoIssue(DOCDT);
                if (GetPendig_Data != null)
                {
                    VE.TBILTY_POPUP = (from DataRow dr in GetPendig_Data.Rows
                                       select new TBILTY_POPUP
                                       {
                                           BLAUTONO = dr["autono"].retStr(),
                                           LRNO = dr["lrno"].retStr(),
                                           LRDT = Convert.ToDateTime(dr["lrdt"].retDateStr()),
                                           BALENO = dr["baleno"].retStr(),
                                           PREFNO = dr["prefno"].retStr(),

                                       }).ToList();
                    for (int p = 0; p <= VE.TBILTY_POPUP.Count - 1; p++)
                    {
                        VE.TBILTY_POPUP[p].SLNO = Convert.ToInt16(p + 1);

                    }
                }

                VE.DefaultView = true;
                return PartialView("_T_BiltyG_Mutia_PopUp", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        //public ActionResult FillDetailData(TransactionBiltyGMutiaEntry VE)
        //{
        //    try
        //    {
        //        VE.TTXNDTL = (from x in VE.TBATCHDTL
        //                      group x by new
        //                      {
        //                          x.TXNSLNO,
        //                          x.ITGRPCD,
        //                          x.ITGRPNM,
        //                          x.MTRLJOBCD,
        //                          x.MTRLJOBNM,
        //                          x.ITCD,
        //                          x.ITNM,
        //                          x.STYLENO,
        //                          x.DISCTYPE,
        //                          x.DISCTYPE_DESC,
        //                          x.TDDISCTYPE,
        //                          x.SCMDISCTYPE,
        //                          x.UOM,
        //                          x.STKTYPE,
        //                          x.RATE,
        //                          x.DISCRATE,
        //                          x.SCMDISCRATE,
        //                          x.TDDISCRATE,
        //                          x.GSTPER,
        //                          x.FLAGMTR,
        //                          x.STKNAME
        //                      } into P
        //                      select new TTXNDTL
        //                      {
        //                          SLNO = P.Key.TXNSLNO.retShort(),
        //                          ITGRPCD = P.Key.ITGRPCD,
        //                          ITGRPNM = P.Key.ITGRPNM,
        //                          MTRLJOBCD = P.Key.MTRLJOBCD,
        //                          MTRLJOBNM = P.Key.MTRLJOBNM,
        //                          ITCD = P.Key.ITCD,
        //                          ITNM = P.Key.STYLENO + "" + P.Key.ITNM,
        //                          STYLENO = P.Key.STYLENO,
        //                          STKTYPE = P.Key.STKTYPE,
        //                          STKNAME = P.Key.STKNAME,
        //                          UOM = P.Key.UOM,
        //                          NOS = P.Sum(A => A.NOS),
        //                          QNTY = P.Sum(A => A.QNTY),
        //                          FLAGMTR = P.Key.FLAGMTR,
        //                          BLQNTY = P.Sum(A => A.BLQNTY),
        //                          RATE = P.Key.RATE,
        //                          DISCTYPE = P.Key.DISCTYPE,
        //                          DISCTYPE_DESC = P.Key.DISCTYPE_DESC,
        //                          DISCRATE = P.Key.DISCRATE,
        //                          TDDISCRATE = P.Key.TDDISCRATE,
        //                          TDDISCTYPE = P.Key.TDDISCTYPE,
        //                          SCMDISCRATE = P.Key.SCMDISCRATE,
        //                          SCMDISCTYPE = P.Key.SCMDISCTYPE,
        //                      }).ToList();
        //        //for (int p = 0; p <= VE.TTXNDTL.Count - 1; p++)
        //        //{
        //        //    VE.TTXNDTL[p].SLNO = Convert.ToInt16(p + 1);
        //        //}
        //        ModelState.Clear();
        //        VE.DefaultView = true;
        //        return PartialView("_T_SALE_DETAIL", VE);
        //    }
        //    catch (Exception ex)
        //    {
        //        Cn.SaveException(ex, "");
        //        return Content(ex.Message + ex.InnerException);
        //    }
        //}
        //public ActionResult FillQtyRequirementData(TransactionBiltyGMutiaEntry VE)
        //{
        //    try
        //    {
        //        VE.TPROGBOM = (from x in VE.TPROGDTL
        //                       group x by new
        //                       {
        //                           x.SLNO,
        //                           x.ITCD,
        //                           x.ITNM,
        //                           x.UOM,
        //                           x.QNTY
        //                       } into P
        //                       select new TPROGBOM
        //                       {
        //                           SLNO = P.Key.SLNO.retShort(),
        //                           QITNM = P.Key.ITNM,
        //                           QUOM = P.Key.UOM,
        //                           QQNTY = P.Key.QNTY,
        //                           //qnty = P.Sum(A => A.QNTY)
        //                       }).ToList();
        //        for (int p = 0; p <= VE.TPROGBOM.Count - 1; p++)
        //        {
        //            VE.TPROGBOM[p].RSLNO = Convert.ToInt16(p + 1);

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
        //public ActionResult OPEN_AMOUNT(TransactionBiltyGMutiaEntry VE, string AUTO_NO, int A_T_NOS, double A_T_QNTY, double A_T_AMT, double A_T_TAXABLE, double A_T_IGST_AMT, double A_T_CGST_AMT, double A_T_SGST_AMT, double A_T_CESS_AMT, double A_T_NET_AMT, double IGST_PER, double CGST_PER, double SGST_PER, double CESS_PER)
        //{
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
        //    string COM_CD = CommVar.Compcd(UNQSNO); string LOC_CD = CommVar.Loccd(UNQSNO);
        //    string DATABASE = CommVar.CurSchema(UNQSNO).ToString();
        //    string DATABASEF = CommVar.FinSchema(UNQSNO);
        //    Cn.getQueryString(VE); string S_P = "";
        //    S_P = "S";


        //    ViewBag.NOS = A_T_NOS;
        //    ViewBag.QNTY = A_T_QNTY;
        //    ViewBag.TAXABLEAMT = A_T_TAXABLE;
        //    ViewBag.NETAMT = A_T_NET_AMT;
        //    ViewBag.IGSTAMT = A_T_IGST_AMT;
        //    ViewBag.CGSTAMT = A_T_CGST_AMT;
        //    ViewBag.SGSTAMT = A_T_SGST_AMT;
        //    ViewBag.CESSAMT = A_T_CESS_AMT;
        //    double A_CURR_AMT = 0; double A_TOTAL_AMT = 0; double A_IGST_AMT = 0; double A_CGST_AMT = 0; double A_SGST_AMT = 0; double A_CESS_AMT = 0; double A_DUTY_AMT = 0; double A_NET_AMT = 0;
        //    try
        //    {
        //        string QUERY = "select a.amtcd, b.amtnm, b.calccode, b.addless, b.CALCTYPE,b.calcformula, a.amtdesc, b.glcd, b.hsncode, a.amtrate, a.curr_amt, a.amt,a.igstper, a.igstamt, a.sgstper, a.sgstamt, a.cgstper, a.cgstamt,a.CESSPER,";
        //        QUERY = QUERY + " a.CESSAMT, a.dutyper, a.dutyamt from " + DATABASE + ".t_txnamt a, " + DATABASE + ".m_amttype b where a.amtcd=b.amtcd(+) and b.salpur='" + S_P + "' and a.autono='" + AUTO_NO + "' ";
        //        QUERY = QUERY + " union select b.amtcd, b.amtnm, b.calccode, b.addless,b.CALCTYPE, b.calcformula, '' amtdesc, b.glcd, b.hsncode, 0 amtrate, 0 curr_amt, 0 amt,0 igstper, 0 igstamt, 0 sgstper, 0 sgstamt, 0 cgstper, 0 cgstamt, ";
        //        QUERY = QUERY + " 0 CESSPER, 0 CESSAMT, 0 dutyper, 0 dutyamt from " + DATABASE + ".m_amttype b, " + DATABASE + ".m_cntrl_hdr c where b.m_autono=c.m_autono(+) and b.salpur='" + S_P + "'  and nvl(c.inactive_tag,'N') = 'N' ";
        //        QUERY = QUERY + "and b.amtcd not in (select amtcd from " + DATABASE + ".t_txnamt where autono='" + AUTO_NO + "')";

        //        var AMOUNT_DATA = Master_Help.SQLquery(QUERY);

        //        //ModelState.Clear();

        //        if (VE.TTXNAMT == null)
        //        {
        //            VE.TTXNAMT = (from DataRow dr in AMOUNT_DATA.Rows
        //                          select new TTXNAMT()
        //                          {
        //                              AMTCD = dr["amtcd"].ToString(),
        //                              ADDLESS = dr["addless"].ToString(),
        //                              GLCD = dr["GLCD"].ToString(),
        //                              AMTNM = dr["amtnm"].ToString(),
        //                              CALCCODE = dr["calccode"].ToString(),
        //                              CALCTYPE = dr["CALCTYPE"].ToString(),
        //                              CALCFORMULA = dr["calcformula"].ToString(),
        //                              AMTDESC = dr["amtdesc"].ToString(),
        //                              HSNCODE = dr["hsncode"].ToString(),
        //                              AMTRATE = Convert.ToDouble(dr["amtrate"].ToString()),
        //                              CURR_AMT = Convert.ToDouble(dr["curr_amt"].ToString()),
        //                              AMT = Convert.ToDouble(dr["amt"].ToString()),
        //                              IGSTPER = IGST_PER,
        //                              CGSTPER = CGST_PER,
        //                              SGSTPER = SGST_PER,
        //                              CESSPER = CESS_PER,
        //                              IGSTAMT = Convert.ToDouble(dr["igstamt"].ToString()),
        //                              CGSTAMT = Convert.ToDouble(dr["cgstamt"].ToString()),
        //                              SGSTAMT = Convert.ToDouble(dr["sgstamt"].ToString()),
        //                              CESSAMT = Convert.ToDouble(dr["CESSAMT"].ToString()),
        //                              DUTYAMT = Convert.ToDouble(dr["dutyamt"].ToString()),
        //                          }).ToList();
        //        }

        //        for (int p = 0; p <= VE.TTXNAMT.Count - 1; p++)
        //        {
        //            VE.TTXNAMT[p].SLNO = Convert.ToInt16(p + 1);

        //            VE.TTXNAMT[p].NETAMT = VE.TTXNAMT[p].AMT == null ? 0 : VE.TTXNAMT[p].AMT.Value +
        //                                    VE.TTXNAMT[p].IGSTAMT == null ? 0 : VE.TTXNAMT[p].IGSTAMT.Value +
        //                                    VE.TTXNAMT[p].CGSTAMT == null ? 0 : VE.TTXNAMT[p].CGSTAMT.Value +
        //                                    VE.TTXNAMT[p].SGSTAMT == null ? 0 : VE.TTXNAMT[p].SGSTAMT.Value +
        //                                    VE.TTXNAMT[p].CESSAMT == null ? 0 : VE.TTXNAMT[p].CESSAMT.Value +
        //                                   VE.TTXNAMT[p].DUTYAMT == null ? 0 : VE.TTXNAMT[p].DUTYAMT.Value;

        //            A_CURR_AMT = A_CURR_AMT + VE.TTXNAMT[p].CURR_AMT == null ? 0 : VE.TTXNAMT[p].CURR_AMT.Value;
        //            A_TOTAL_AMT = A_TOTAL_AMT + VE.TTXNAMT[p].AMT == null ? 0 : VE.TTXNAMT[p].AMT.Value;
        //            A_IGST_AMT = A_IGST_AMT + VE.TTXNAMT[p].IGSTAMT == null ? 0 : VE.TTXNAMT[p].IGSTAMT.Value;
        //            A_CGST_AMT = A_CGST_AMT + VE.TTXNAMT[p].CGSTAMT == null ? 0 : VE.TTXNAMT[p].CGSTAMT.Value;
        //            A_SGST_AMT = A_SGST_AMT + VE.TTXNAMT[p].SGSTAMT == null ? 0 : VE.TTXNAMT[p].SGSTAMT.Value;
        //            A_CESS_AMT = A_CESS_AMT + VE.TTXNAMT[p].CESSAMT == null ? 0 : VE.TTXNAMT[p].CESSAMT.Value;
        //            A_NET_AMT = A_NET_AMT + A_TOTAL_AMT + A_IGST_AMT + A_CGST_AMT + A_SGST_AMT + A_CESS_AMT;
        //        }
        //        VE.A_T_CURR = A_CURR_AMT;
        //        VE.A_T_AMOUNT = A_TOTAL_AMT;
        //        VE.A_T_IGST = A_IGST_AMT;
        //        VE.A_T_CGST = A_CGST_AMT;
        //        VE.A_T_SGST = A_SGST_AMT;
        //        VE.A_T_CESS = A_CESS_AMT;
        //        VE.A_T_DUTY = A_DUTY_AMT;
        //        VE.A_T_NET = A_NET_AMT;
        //        ModelState.Clear();
        //        VE.DefaultView = true;
        //        return PartialView("_T_SALE_AMOUNT", VE);

        //    }
        //    catch (Exception ex)
        //    {
        //        VE.DefaultView = false;
        //        VE.DefaultDay = 0;
        //        ViewBag.ErrorMessage = ex.Message + ex.InnerException;
        //        return View(VE);
        //    }
        //}
        //public ActionResult AddRow(TransactionBiltyGMutiaEntry VE, int COUNT, string TAG)
        //{
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
        //    Cn.getQueryString(VE);
        //    //ViewBag.formname = formnamebydoccd(VE.DOC_CODE);

        //    //List<DebitCreditType> DCT = new List<DebitCreditType>();
        //    //DebitCreditType DCT1 = new DebitCreditType(); DCT1.text = "DR"; DCT1.value = "D"; DCT.Add(DCT1);
        //    //DebitCreditType DCT2 = new DebitCreditType(); DCT2.text = "CR"; DCT2.value = "C"; DCT.Add(DCT2); VE.DebitCreditType = DCT;
        //    //VE.Database_Combo2 = (from i in DB.T_VCH_DET select new Database_Combo2() { FIELD_VALUE = i.BANK_NAME }).DistinctBy(a => a.FIELD_VALUE).ToList();
        //    //VE.Database_Combo3 = (from i in DB.T_VCH_DET select new Database_Combo3() { FIELD_VALUE = i.T_REM }).DistinctBy(a => a.FIELD_VALUE).ToList();
        //    //VE.DropDown_list_TDS = INT_TDS();
        //    if (VE.TPROGDTL == null)
        //    {
        //        List<TPROGDTL> TPROGDTL1 = new List<TPROGDTL>();
        //        if (COUNT > 0 && TAG == "Y")
        //        {
        //            int SERIAL = 0;
        //            for (int j = 0; j <= COUNT - 1; j++)
        //            {
        //                SERIAL = SERIAL + 1;
        //                TPROGDTL MBILLDET = new TPROGDTL();
        //                MBILLDET.SLNO = SERIAL.retShort();
        //                TPROGDTL1.Add(MBILLDET);
        //            }
        //        }
        //        else
        //        {
        //            TPROGDTL MBILLDET = new TPROGDTL();
        //            MBILLDET.SLNO = 1;
        //            TPROGDTL1.Add(MBILLDET);
        //        }
        //        VE.TPROGDTL = TPROGDTL1;
        //    }
        //    else
        //    {
        //        List<TPROGDTL> TPROGDTL = new List<TPROGDTL>();
        //        for (int i = 0; i <= VE.TPROGDTL.Count - 1; i++)
        //        {
        //            TPROGDTL MBILLDET = new TPROGDTL();
        //            MBILLDET = VE.TPROGDTL[i];
        //            TPROGDTL.Add(MBILLDET);
        //        }
        //        TPROGDTL MBILLDET1 = new TPROGDTL();
        //        if (COUNT > 0 && TAG == "Y")
        //        {
        //            int SERIAL = Convert.ToInt32(VE.TPROGDTL.Max(a => Convert.ToInt32(a.SLNO)));
        //            for (int j = 0; j <= COUNT - 1; j++)
        //            {
        //                SERIAL = SERIAL + 1;
        //                TPROGDTL OPENING_BL = new TPROGDTL();
        //                OPENING_BL.SLNO = SERIAL.retShort();
        //                TPROGDTL.Add(OPENING_BL);
        //            }
        //        }
        //        else
        //        {
        //            MBILLDET1.SLNO = Convert.ToInt16(Convert.ToByte(VE.TPROGDTL.Max(a => Convert.ToInt32(a.SLNO))) + 1);
        //            TPROGDTL.Add(MBILLDET1);
        //        }
        //        VE.TPROGDTL = TPROGDTL;
        //    }
        //    //VE.TPROGDTL.ForEach(a => a.DRCRTA = masterHelp.DR_CR().OrderByDescending(s => s.text).ToList());
        //    VE.DefaultView = true;
        //    return PartialView("_T_OUTISSPROCESS_Programme", VE);
        //}
        //public ActionResult DeleteRow(TransactionBiltyGMutiaEntry VE)
        //{
        //    try
        //    {
        //        ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
        //        List<TPROGDTL> ITEMSIZE = new List<TPROGDTL>();
        //        int count = 0;
        //        for (int i = 0; i <= VE.TPROGDTL.Count - 1; i++)
        //        {
        //            if (VE.TPROGDTL[i].Checked == false)
        //            {
        //                count += 1;
        //                TPROGDTL item = new TPROGDTL();
        //                item = VE.TPROGDTL[i];
        //                item.SLNO = count.retShort();
        //                ITEMSIZE.Add(item);
        //            }

        //        }
        //        VE.TPROGDTL = ITEMSIZE;
        //        ModelState.Clear();
        //        VE.DefaultView = true;
        //        return PartialView("_T_OUTISSPROCESS_Programme", VE);
        //    }
        //    catch (Exception ex)
        //    {
        //        Cn.SaveException(ex, "");
        //        return Content(ex.Message + ex.InnerException);
        //    }
        //}
        //public ActionResult AddRowQTY(TransactionBiltyGMutiaEntry VE, int COUNT, string TAG)
        //{
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
        //    Cn.getQueryString(VE);
        //    if (VE.TPROGBOM == null)
        //    {
        //        List<TPROGBOM> TPROGBOM1 = new List<TPROGBOM>();
        //        if (COUNT > 0 && TAG == "Y")
        //        {
        //            int SERIAL = 0, rslno = 0;
        //            for (int j = 0; j <= COUNT - 1; j++)
        //            {
        //                SERIAL = SERIAL + 1;
        //                rslno = rslno + 1;
        //                TPROGBOM MBILLDET = new TPROGBOM();
        //                MBILLDET.SLNO = SERIAL.retShort();
        //                MBILLDET.RSLNO = rslno.retShort();
        //                TPROGBOM1.Add(MBILLDET);
        //            }
        //        }
        //        else
        //        {
        //            TPROGBOM MBILLDET = new TPROGBOM();
        //            MBILLDET.SLNO = 1;
        //            MBILLDET.RSLNO = 1;
        //            TPROGBOM1.Add(MBILLDET);
        //        }
        //        VE.TPROGBOM = TPROGBOM1;
        //    }
        //    else
        //    {
        //        List<TPROGBOM> TPROGBOM = new List<TPROGBOM>();
        //        for (int i = 0; i <= VE.TPROGBOM.Count - 1; i++)
        //        {
        //            TPROGBOM MBILLDET = new TPROGBOM();
        //            MBILLDET = VE.TPROGBOM[i];
        //            TPROGBOM.Add(MBILLDET);
        //        }
        //        TPROGBOM MBILLDET1 = new TPROGBOM();
        //        if (COUNT > 0 && TAG == "Y")
        //        {
        //            int SERIAL = Convert.ToInt32(VE.TPROGBOM.Max(a => Convert.ToInt32(a.SLNO)));
        //            int rslno = Convert.ToInt32(VE.TPROGBOM.Max(a => Convert.ToInt32(a.RSLNO)));
        //            for (int j = 0; j <= COUNT - 1; j++)
        //            {
        //                SERIAL = SERIAL + 1;
        //                rslno = rslno + 1;
        //                TPROGBOM OPENING_BL = new TPROGBOM();
        //                OPENING_BL.SLNO = SERIAL.retShort();
        //                OPENING_BL.RSLNO = rslno.retShort();
        //                TPROGBOM.Add(OPENING_BL);
        //            }
        //        }
        //        else
        //        {
        //            MBILLDET1.SLNO = Convert.ToInt16(Convert.ToByte(VE.TPROGBOM.Max(a => Convert.ToInt32(a.SLNO))) + 1);
        //            MBILLDET1.RSLNO = Convert.ToInt16(Convert.ToByte(VE.TPROGBOM.Max(a => Convert.ToInt32(a.RSLNO))) + 1);
        //            TPROGBOM.Add(MBILLDET1);
        //        }
        //        VE.TPROGBOM = TPROGBOM;
        //    }
        //    //VE.TPROGDTL.ForEach(a => a.DRCRTA = masterHelp.DR_CR().OrderByDescending(s => s.text).ToList());
        //    VE.DefaultView = true;
        //    return PartialView("_T_OUTISSPROCESS_QtyRequirement", VE);
        //}
        //public ActionResult DeleteRowQTY(TransactionBiltyGMutiaEntry VE)
        //{
        //    try
        //    {
        //        ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
        //        List<TPROGBOM> ITEMSIZE = new List<TPROGBOM>();
        //        int count = 0;
        //        for (int i = 0; i <= VE.TPROGBOM.Count - 1; i++)
        //        {
        //            if (VE.TPROGBOM[i].Checked == false)
        //            {
        //                count += 1;
        //                TPROGBOM item = new TPROGBOM();
        //                item = VE.TPROGBOM[i];
        //                item.SLNO = count.retShort();
        //                ITEMSIZE.Add(item);
        //            }

        //        }
        //        VE.TPROGBOM = ITEMSIZE;
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
        public ActionResult AddDOCRow(TransactionBiltyGMutiaEntry VE)
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
        public ActionResult DeleteDOCRow(TransactionBiltyGMutiaEntry VE)
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
        //public ActionResult RepeatAboveRow(TransactionBiltyGMutiaEntry VE, int RowIndex)
        //{
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
        //    Cn.getQueryString(VE);

        //    List<TPROGBOM> TPROGBOM = new List<TPROGBOM>(); bool copied = false;
        //    for (int k = 0; k <= VE.TPROGBOM.Count; k++)
        //    {
        //        TPROGBOM MBILLDET = new TPROGBOM();
        //        if (RowIndex == k || copied == true)
        //        {
        //            foreach (PropertyInfo propA in VE.TPROGBOM[k - 1].GetType().GetProperties())
        //            {
        //                PropertyInfo propB = VE.TPROGBOM[k - 1].GetType().GetProperty(propA.Name);
        //                propB.SetValue(MBILLDET, propA.GetValue(VE.TPROGBOM[k - 1], null), null);
        //            }
        //            MBILLDET.SLNO = (k + 1).retShort();
        //            TPROGBOM.Add(MBILLDET);
        //            copied = true;
        //        }
        //        else
        //        {
        //            MBILLDET = VE.TPROGBOM[k]; MBILLDET.SLNO = (k + 1).retShort();
        //            TPROGBOM.Add(MBILLDET);
        //        }
        //    }
        //    VE.TPROGBOM = TPROGBOM;
        //    VE.DefaultView = true;
        //    ModelState.Clear();
        //    return PartialView("_T_OUTISSPROCESS_QtyRequirement", VE);
        //}
        public ActionResult cancelRecords(TransactionBiltyGMutiaEntry VE, string par1)
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
                        TCH = Cn.T_CONTROL_HDR(VE.T_BILTY_HDR.AUTONO, CommVar.CurSchema(UNQSNO).ToString());
                    }
                    else
                    {
                        TCH = Cn.T_CONTROL_HDR(VE.T_BILTY_HDR.AUTONO, CommVar.CurSchema(UNQSNO).ToString(), par1);
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
                        TCH1 = Cn.T_CONTROL_HDR(VE.T_BILTY_HDR.AUTONO, CommVar.FinSchema(UNQSNO));
                    }
                    else
                    {
                        TCH1 = Cn.T_CONTROL_HDR(VE.T_BILTY_HDR.AUTONO, CommVar.FinSchema(UNQSNO), par1);
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
        public ActionResult ParkRecord(FormCollection FC, TransactionBiltyGMutiaEntry stream, string menuID, string menuIndex)
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
                string url = Master_Help.RetriveParkFromFile(value, Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString(), "Improvar.ViewModels.TransactionBiltyGMutiaEntry");
                return url;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public ActionResult SAVE(FormCollection FC, TransactionBiltyGMutiaEntry VE)
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
            //
            DB.Configuration.ValidateOnSaveEnabled = false;
            using (var transaction = DB.Database.BeginTransaction())
            {
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
                        T_BILTY_HDR TBHDR = new T_BILTY_HDR();
                        T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                        string DOCPATTERN = "";
                        TCH.DOCDT = VE.T_CNTRL_HDR.DOCDT;
                        string Ddate = Convert.ToString(TCH.DOCDT);
                        TBHDR.CLCD = CommVar.ClientCode(UNQSNO);
                        string auto_no = ""; string Month = "";
                        if (VE.DefaultAction == "A")
                        {
                            TBHDR.EMD_NO = 0;
                            TCH.DOCCD = VE.T_CNTRL_HDR.DOCCD;
                            TCH.DOCNO = Cn.MaxDocNumber(TCH.DOCCD, Ddate);
                            //TTXN.DOCNO = Cn.MaxDocNumber(TTXN.DOCCD, Ddate);
                            DOCPATTERN = Cn.DocPattern(Convert.ToInt32(TCH.DOCNO), TCH.DOCCD, CommVar.CurSchema(UNQSNO).ToString(), CommVar.FinSchema(UNQSNO), Ddate);
                            auto_no = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), TCH.DOCNO, TCH.DOCCD, Ddate);
                            TBHDR.AUTONO = auto_no.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
                            Month = auto_no.Split(Convert.ToChar(Cn.GCS()))[1].ToString();
                        }
                        else
                        {
                            TCH.DOCCD = VE.T_CNTRL_HDR.DOCCD;
                            TCH.DOCNO = VE.T_CNTRL_HDR.DOCNO;
                            TBHDR.AUTONO = VE.T_BILTY_HDR.AUTONO;
                            Month = VE.T_CNTRL_HDR.MNTHCD;
                            TBHDR.EMD_NO = Convert.ToByte((VE.T_CNTRL_HDR.EMD_NO == null ? 0 : VE.T_CNTRL_HDR.EMD_NO) + 1);
                            DOCPATTERN = VE.T_CNTRL_HDR.DOCNO;
                        }

                        TBHDR.MUTSLCD = VE.T_BILTY_HDR.MUTSLCD;
                        TBHDR.TREM = VE.T_BILTY_HDR.TREM;

                        if (VE.DefaultAction == "E")
                        {
                            dbsql = MasterHelpFa.TblUpdt("t_bilty", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                            dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_rem", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_doc", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_doc_dtl", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }


                            //dbsql = MasterHelpFa.TblUpdt("t_cntrl_doc_pass", TBHDR.AUTONO, "E");
                            //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        }

                        //----------------------------------------------------------//
                        dbsql = MasterHelpFa.T_Cntrl_Hdr_Updt_Ins(TBHDR.AUTONO, VE.DefaultAction, "S", Month, TCH.DOCCD, DOCPATTERN, TCH.DOCDT.retStr(), TBHDR.EMD_NO.retShort(), TCH.DOCNO, Convert.ToDouble(TCH.DOCNO), null, null, null, TBHDR.MUTSLCD);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                        dbsql = MasterHelpFa.RetModeltoSql(TBHDR, VE.DefaultAction);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        int COUNTER = 0;

                        for (int i = 0; i <= VE.TBILTY.Count - 1; i++)
                        {
                            if (VE.TBILTY[i].SLNO != 0)
                            {
                                COUNTER = COUNTER + 1;
                                T_BILTY TBILTY = new T_BILTY();
                                TBILTY.CLCD = TBHDR.CLCD;
                                TBILTY.AUTONO = TBHDR.AUTONO;
                                TBILTY.SLNO = VE.TBILTY[i].SLNO;
                                TBILTY.BLAUTONO = VE.TBILTY[i].BLAUTONO;
                                TBILTY.DRCR = VE.DRCR;
                                TBILTY.LRDT = VE.TBILTY[i].LRDT;
                                TBILTY.LRNO = VE.TBILTY[i].LRNO;
                                TBILTY.BALEYR = "";
                                TBILTY.BALENO = VE.TBILTY[i].BALENO;
                                dbsql = MasterHelpFa.RetModeltoSql(TBILTY);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                            }
                        }

                        if (VE.UploadDOC != null)// add
                        {
                            var img = Cn.SaveUploadImageTransaction(VE.UploadDOC, TBHDR.AUTONO, TBHDR.EMD_NO.Value);
                            if (img.Item1.Count != 0)
                            {
                                for (int tr = 0; tr <= img.Item1.Count - 1; tr++)
                                {
                                    dbsql = MasterHelpFa.RetModeltoSql(img.Item1[tr]);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                                for (int tr = 0; tr <= img.Item2.Count - 1; tr++)
                                {
                                    dbsql = MasterHelpFa.RetModeltoSql(img.Item2[tr]);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }
                        }
                        if (VE.T_CNTRL_HDR_REM.DOCREM != null)// add REMARKS
                        {
                            var NOTE = Cn.SAVETRANSACTIONREMARKS(VE.T_CNTRL_HDR_REM, TBHDR.AUTONO, TBHDR.CLCD, TBHDR.EMD_NO.Value);
                            if (NOTE.Item1.Count != 0)
                            {
                                for (int tr = 0; tr <= NOTE.Item1.Count - 1; tr++)
                                {
                                    dbsql = MasterHelpFa.RetModeltoSql(NOTE.Item1[tr]);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }
                        }


                        if (VE.DefaultAction == "A")
                        {
                            ContentFlg = "1" + " (Issue No. " + TCH.DOCNO + ")~" + TBHDR.AUTONO;
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
                        dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_doc_dtl", VE.T_BILTY_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_doc", VE.T_BILTY_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_rem", VE.T_BILTY_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = MasterHelpFa.TblUpdt("t_bilty", VE.T_BILTY_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = MasterHelpFa.TblUpdt("T_BILTY_HDR", VE.T_BILTY_HDR.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }


                        dbsql = MasterHelpFa.T_Cntrl_Hdr_Updt_Ins(VE.T_BILTY_HDR.AUTONO, "D", "S", null, null, null, VE.T_CNTRL_HDR.DOCDT.retStr(), null, null, null);
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
                    goto dbok;
                    dbnotsave:;
                    transaction.Rollback();
                    OraTrans.Rollback();
                    OraCon.Dispose();
                    return Content(dberrmsg);
                    dbok:;
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
    }
}