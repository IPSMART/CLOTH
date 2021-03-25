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
    public class T_OUTRECPROCESSController : Controller
    {
        // GET: T_OUTRECPROCESS
        Connection Cn = new Connection(); MasterHelp masterHelp = new MasterHelp(); SchemeCal Scheme_Cal = new SchemeCal(); Salesfunc salesfunc = new Salesfunc(); DataTable DT = new DataTable(); DataTable DTNEW = new DataTable();
        EmailControl EmailControl = new EmailControl();
        T_TXN TXN; T_TXNTRANS TXNTRN; T_TXNOTH TXNOTH; T_CNTRL_HDR TCH; T_CNTRL_HDR_REM SLR;
        SMS SMS = new SMS();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: T_OUTRECPROCESS
        public ActionResult T_OUTRECPROCESS(string op = "", string key = "", int Nindex = 0, string searchValue = "", string parkID = "", string ThirdParty = "no")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    TransactionOutRecProcess VE = (parkID == "") ? new TransactionOutRecProcess() : (Improvar.ViewModels.TransactionOutRecProcess)Session[parkID];
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    switch (VE.MENU_PARA)
                    {
                        case "DY":
                            ViewBag.formname = "Receive for Dyer"; break;
                        case "PR":
                            ViewBag.formname = "Receive for Printing"; break;
                        case "ST":
                            ViewBag.formname = "Receive for Stiching"; break;
                        case "EM":
                            ViewBag.formname = "Receive for Embroidery"; break;
                        case "JW":
                            ViewBag.formname = "Receive for Other Jobs"; break;
                        case "DYU":
                            ViewBag.formname = "Return from Dyer w/o Job"; break;
                        case "PRU":
                            ViewBag.formname = "Return from Printer w/o Job"; break;
                        case "STU":
                            ViewBag.formname = "Return from Sticher w/o Job"; break;
                        case "EMU":
                            ViewBag.formname = "Return from Embroider w/o Job"; break;
                        case "JWU":
                            ViewBag.formname = "Return from Other w/o Job"; break;
                        default: ViewBag.formname = ""; break;
                    }
                    string LOC = CommVar.Loccd(UNQSNO);
                    string COM = CommVar.Compcd(UNQSNO);
                    string YR1 = CommVar.YearCode(UNQSNO);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    VE.DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
                    VE.BARGEN_TYPE = masterHelp.BARGEN_TYPE();
                    VE.Reverse_Charge = masterHelp.REV_CHRG();
                    //VE.DropDown_list_MTRLJOBCD = (from i in DB.M_MTRLJOBMST select new DropDown_list_MTRLJOBCD() { MTRLJOBCD = i.MTRLJOBCD, MTRLJOBNM = i.MTRLJOBNM }).OrderBy(s => s.MTRLJOBNM).ToList();
                    VE.DropDown_list_MTRLJOBCD = masterHelp.MTRLJOBCD_List();
                    VE.DropDown_list_StkType = masterHelp.STK_TYPE();
                    VE.DISC_TYPE = masterHelp.DISC_TYPE();
                    foreach (var v in VE.DropDown_list_MTRLJOBCD)
                    {

                        if (v.MTRLJOBCD == "FS")
                        {
                            v.Checked = true;
                        }

                    }
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
                                string jobcd = "";
                                if (VE.MENU_PARA == "DY" || VE.MENU_PARA == "DYU")
                                {
                                    jobcd = "DY";
                                }
                                else if (VE.MENU_PARA == "PR" || VE.MENU_PARA == "PRU")
                                {
                                    jobcd = "PR";
                                }
                                else if (VE.MENU_PARA == "ST" || VE.MENU_PARA == "STU")
                                {
                                    jobcd = "ST";
                                }
                                else if (VE.MENU_PARA == "EM" || VE.MENU_PARA == "EMU")
                                {
                                    jobcd = "EM";
                                }

                                var job = (from i in DB.M_JOBMST where i.JOBCD == jobcd select new { JOBCD = i.JOBCD, JOBNM = i.JOBNM }).OrderBy(s => s.JOBNM).FirstOrDefault();
                                if (job != null) { TTXN.JOBCD = job.JOBCD; VE.JOBNM = job.JOBNM; }

                                var mtrljob = (from i in DB.M_MTRLJOBMST where i.MTRLJOBCD == jobcd select new { MTRLJOBCD = i.MTRLJOBCD, MTRLJOBNM = i.MTRLJOBNM }).OrderBy(s => s.MTRLJOBNM).FirstOrDefault();
                                if (mtrljob != null) { VE.MTRLJOBCD = mtrljob.MTRLJOBCD; VE.MTRLJOBNM = mtrljob.MTRLJOBNM; }

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

                                VE.T_TXN = TTXN;

                                T_TXNOTH TXNOTH = new T_TXNOTH(); VE.T_TXNOTH = TXNOTH;

                                VE.RoundOff = true;
                                //List<TPROGDTL> TPROGDTL = new List<TPROGDTL>();
                                //for (int i = 0; i <= 9; i++)
                                //{
                                //    TPROGDTL PROGDTL = new TPROGDTL();
                                //    PROGDTL.SLNO = Convert.ToByte(i + 1);
                                //    //PROGDTL.MTRLJOBCD = TTXN.JOBCD;
                                //    //PROGDTL.MTRLJOBNM = VE.JOBNM;
                                //    TPROGDTL.Add(PROGDTL);
                                //    VE.TPROGDTL = TPROGDTL;
                                //}
                                //VE.TPROGDTL = TPROGDTL;
                                //List<TPROGBOM> TPROGBOM = new List<TPROGBOM>();
                                //for (int i = 0; i <= 9; i++)
                                //{
                                //    TPROGBOM PROGBOM = new TPROGBOM();
                                //    PROGBOM.SLNO = Convert.ToByte(i + 1);
                                //    PROGBOM.RSLNO = Convert.ToByte(i + 1);
                                //    TPROGBOM.Add(PROGBOM);
                                //    VE.TPROGBOM = TPROGBOM;
                                //}
                                //VE.TPROGBOM = TPROGBOM;
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
                            VE = (TransactionOutRecProcess)Cn.CheckPark(VE, VE.MENU_DETAILS, LOC, COM, CommVar.CurSchema(UNQSNO), Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString());
                        }
                        if (parkID == "")
                        {
                            FreightCharges(VE, VE.T_TXN.AUTONO);
                            ViewBag.PENDPROGRAMME = "N";
                        }
                        else
                        {
                            string date = Convert.ToDateTime(VE.T_TXN.DOCDT).AddDays(1).retDateStr();
                            string result = GetPendingProgramme(date, VE.T_TXN.JOBCD.retStr(), VE.T_TXN.SLCD.retStr(), VE.T_TXN.AUTONO.retStr(), "Y");
                            if (result.retStr() == "1")
                            {
                                ViewBag.PENDPROGRAMME = "Y";
                            }
                            else
                            {
                                ViewBag.PENDPROGRAMME = "N";
                            }
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
                TransactionOutRecProcess VE = new TransactionOutRecProcess();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message;
                return View(VE);
            }
        }
        public TransactionOutRecProcess Navigation(TransactionOutRecProcess VE, ImprovarDB DB, int index, string searchValue)
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
                VE.NETAMT = TXN.BLAMT.retDbl() - TXNOTH.TDSAMT.retDbl() - TXN.ADVADJ.retDbl();
                VE.TDSNM = TXNOTH.TDSHD.retStr() == "" ? "" : DBF.M_TDS_CNTRL.Where(a => a.TDSCODE == TXNOTH.TDSHD).Select(b => b.TDSNM).FirstOrDefault();
                if (TXNOTH.TDSHD.retStr() != "")
                {
                    var data = masterHelp.TDSCODE_help(TXN.DOCDT.retStr(), TXNOTH.TDSHD.retStr(), TXN.SLCD.retStr(), "", "");
                    VE.TDSROUNDCAL = data.retCompValue("TDSROUNDCAL").retStr();
                    VE.TDSCALCON = data.retCompValue("TDSCALCON").retStr();
                }
                if (TXN.SLCD.retStr() != "")
                {
                    string slcd = TXN.SLCD;
                    var subleg = (from a in DBF.M_SUBLEG where a.SLCD == slcd select new { a.SLNM, a.SLAREA, a.DISTRICT, a.GSTNO, a.PSLCD, a.TCSAPPL, a.PANNO }).FirstOrDefault();
                    VE.SLNM = subleg.SLNM;
                    VE.DISTRICT = subleg.SLAREA == "" ? subleg.DISTRICT : subleg.SLAREA;
                    VE.GSTNO = subleg.GSTNO;
                    //VE.PSLCD = subleg.PSLCD;
                    //VE.TCSAPPL = subleg.TCSAPPL;
                    //if (VE.MENU_PARA == "SR" || VE.MENU_PARA == "PR") VE.TCSAPPL = "N";
                    //panno = subleg.PANNO;
                }

                VE.SAGSLNM = TXNOTH.SAGSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXNOTH.SAGSLCD).Select(b => b.SLNM).FirstOrDefault();
                VE.GONM = TXN.GOCD.retStr() == "" ? "" : DBF.M_GODOWN.Where(a => a.GOCD == TXN.GOCD).Select(b => b.GONM).FirstOrDefault();
                VE.PRCNM = TXNOTH.PRCCD.retStr() == "" ? "" : DBF.M_PRCLST.Where(a => a.PRCCD == TXNOTH.PRCCD).Select(b => b.PRCNM).FirstOrDefault();
                VE.JOBNM = TXN.JOBCD.retStr() == "" ? "" : DB.M_JOBMST.Where(a => a.JOBCD == TXN.JOBCD).Select(b => b.JOBNM).FirstOrDefault();
                VE.TRANSLNM = TXNTRN.TRANSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXNTRN.TRANSLCD).Select(b => b.SLNM).FirstOrDefault();
                SLR = Cn.GetTransactionReamrks(CommVar.CurSchema(UNQSNO).ToString(), TXN.AUTONO);
                VE.UploadDOC = Cn.GetUploadImageTransaction(CommVar.CurSchema(UNQSNO).ToString(), TXN.AUTONO);


                string Scm = CommVar.CurSchema(UNQSNO);

                string str = "";
                str += "select a.PROGAUTONO,a.PROGSLNO,a.PROGAUTONO||a.PROGSLNO PROGAUTOSLNO,b.PROGUNIQNO,b.BARNO,a.SLNO,d.ITGRPCD,c.ITGRPNM,d.ITNM, ";
                str += "b.ITCD,d.FABITCD,d.STYLENO,d.UOMcd,e.COLRNM,b.COLRCD,b.SIZECD,b.SHADE,a.NOS,a.QNTY,b.ITREMARK,f.ITNM FABITNM,b.sample,g.COMMONUNIQBAR ";
                str += "from " + Scm + ".T_PROGDTL a , " + Scm + ".T_PROGMAST b," + Scm + ".M_GROUP c, ";
                str += Scm + ".M_SITEM d, " + Scm + ".M_COLOR e, " + Scm + ".M_SITEM f, " + Scm + ".T_BATCHMST g ";
                str += "where d.ITGRPCD=c.ITGRPCD(+) and b.ITCD = d.ITCD(+) and b.COLRCD = e.COLRCD(+) and d.FABITCD = f.ITCD(+) and b.BARNO=g.BARNO(+) ";
                str += "and a.PROGAUTONO = b.AUTONO  and  a.PROGSLNO = b.SLNO and a.autono='" + TXN.AUTONO + "' ";
                str += "order by a.slno ";

                DataTable Progdtltbl = masterHelp.SQLquery(str);
                var pendprogramme = salesfunc.getPendProg(TXN.DOCDT.retStr().Remove(10), "", TXN.SLCD.retStr().retSqlformat(), "", TXN.JOBCD.retStr().retSqlformat(), TXN.AUTONO.retStr());
                if (VE.DefaultAction == "E")
                {
                    TempData["PENDPROGRAMME" + VE.MENU_PARA] = pendprogramme;
                }
                //VE.TPROGDTL = (from DataRow dr in Progdtltbl.Rows
                //               select new TPROGDTL()
                //               {
                //                   SLNO = dr["SLNO"].retShort(),
                //                   PROGAUTONO = dr["PROGAUTONO"].retStr(),
                //                   PROGSLNO = dr["PROGSLNO"].retShort(),
                //                   PROGAUTOSLNO = dr["PROGAUTOSLNO"].retStr(),
                //                   PROGUNIQNO = dr["PROGUNIQNO"].retStr(),
                //                   BARNO = dr["BARNO"].retStr(),
                //                   ITGRPNM = dr["ITGRPNM"].retStr(),
                //                   ITGRPCD = dr["ITGRPCD"].retStr(),
                //                   ITNM = dr["FABITNM"].retStr() + " " + dr["ITNM"].retStr(),
                //                   ITCD = dr["ITCD"].retStr(),
                //                   //FABITCD = dr["FABITCD"].retStr(),
                //                   STYLENO = dr["STYLENO"].retStr(),
                //                   UOM = dr["UOMCD"].retStr(),
                //                   COLRNM = dr["COLRNM"].retStr(),
                //                   COLRCD = dr["COLRCD"].retStr(),
                //                   SIZECD = dr["SIZECD"].retStr(),
                //                   SHADE = dr["SHADE"].retStr(),
                //                   NOS = dr["NOS"].retDbl(),
                //                   QNTY = dr["QNTY"].retDbl(),
                //                   ITREMARK = dr["ITREMARK"].retStr(),
                //                   SAMPLE = dr["sample"].retStr(),
                //                   CheckedSample = dr["sample"].retStr() == "Y" ? true : false,
                //               }).OrderBy(s => s.SLNO).ToList();
                VE.TPROGDTL = (from dr in Progdtltbl.AsEnumerable()
                               join dr1 in pendprogramme.AsEnumerable() on dr.Field<string>("PROGAUTOSLNO") equals dr1.Field<string>("PROGAUTOSLNO") into Z
                               from dr1 in Z.DefaultIfEmpty()
                               select new TPROGDTL()
                               {
                                   SLNO = dr.Field<short>("SLNO").retShort(),
                                   PROGAUTONO = dr.Field<string>("PROGAUTONO").retStr(),
                                   PROGSLNO = dr.Field<short>("PROGSLNO").retShort(),
                                   PROGAUTOSLNO = dr.Field<string>("PROGAUTOSLNO").retStr(),
                                   PROGUNIQNO = dr.Field<string>("PROGUNIQNO").retStr(),
                                   BARNO = dr.Field<string>("BARNO").retStr(),
                                   ITGRPNM = dr.Field<string>("ITGRPNM").retStr(),
                                   ITGRPCD = dr.Field<string>("ITGRPCD").retStr(),
                                   ITNM = dr.Field<string>("FABITNM").retStr() + " " + dr.Field<string>("ITNM").retStr(),
                                   ITCD = dr.Field<string>("ITCD").retStr(),
                                   //FABITCD = dr["FABITCD"].retStr(),
                                   STYLENO = dr.Field<string>("STYLENO").retStr(),
                                   UOM = dr.Field<string>("UOMCD").retStr(),
                                   COLRNM = dr.Field<string>("COLRNM").retStr(),
                                   COLRCD = dr.Field<string>("COLRCD").retStr(),
                                   SIZECD = dr.Field<string>("SIZECD").retStr(),
                                   SHADE = dr.Field<string>("SHADE").retStr(),
                                   NOS = dr.Field<double>("NOS").retDbl(),
                                   QNTY = dr.Field<double>("QNTY").retDbl(),
                                   ITREMARK = dr.Field<string>("ITREMARK").retStr(),
                                   SAMPLE = dr.Field<string>("sample").retStr(),
                                   CheckedSample = dr.Field<string>("sample").retStr() == "Y" ? true : false,
                                   BALNOS = dr1.Field<decimal>("BALNOS").retDbl(),
                                   BALQNTY = dr1.Field<decimal>("BALQNTY").retDbl(),
                                   COMMONUNIQBAR = dr1.Field<string>("COMMONUNIQBAR").retStr(),
                               }).OrderBy(s => s.SLNO).ToList();

                VE.P_T_NOS = VE.TPROGDTL.Sum(a => a.NOS).retDbl();
                VE.P_T_QNTY = VE.TPROGDTL.Sum(a => a.QNTY).retDbl();

                string str1 = "";
                str1 += "select i.SLNO,i.TXNSLNO,k.ITGRPCD,n.ITGRPNM,n.BARGENTYPE,i.MTRLJOBCD,o.MTRLJOBNM,o.MTBARCODE,k.ITCD,k.ITNM,k.UOMCD,k.STYLENO,i.PARTCD,p.PARTNM, ";
                str1 += "p.PRTBARCODE,i.STKTYPE,q.STKNAME,i.BARNO,j.COLRCD,m.COLRNM,m.CLRBARCODE,j.SIZECD,l.SIZENM,l.SZBARCODE,i.SHADE,i.QNTY,i.NOS,i.RATE,i.DISCRATE, ";
                str1 += "i.DISCTYPE,i.TDDISCRATE,i.TDDISCTYPE,i.SCMDISCTYPE,i.SCMDISCRATE,i.HSNCODE,i.BALENO,j.PDESIGN,j.OURDESIGN,i.FLAGMTR,i.LOCABIN,i.BALEYR ";
                str1 += ",n.SALGLCD,n.PURGLCD,n.SALRETGLCD,n.PURRETGLCD,j.WPRATE,j.RPRATE,i.ITREM,i.ORDAUTONO,i.ORDSLNO,r.DOCNO ORDDOCNO,r.DOCDT ORDDOCDT,n.RPPRICEGEN, ";
                str1 += "n.WPPRICEGEN,i.RECPROGAUTONO,i.RECPROGSLNO,i.MTRLCOST,j.COMMONUNIQBAR ";
                str1 += "from " + Scm + ".T_BATCHDTL i, " + Scm + ".T_BATCHMST j, " + Scm + ".M_SITEM k, " + Scm + ".M_SIZE l, " + Scm + ".M_COLOR m, ";
                str1 += Scm + ".M_GROUP n," + Scm + ".M_MTRLJOBMST o," + Scm + ".M_PARTS p," + Scm + ".M_STKTYPE q," + Scm + ".T_CNTRL_HDR r ";
                str1 += "where i.BARNO = j.BARNO(+) and j.ITCD = k.ITCD(+) and j.SIZECD = l.SIZECD(+) and j.COLRCD = m.COLRCD(+) and k.ITGRPCD=n.ITGRPCD(+) ";
                str1 += "and i.MTRLJOBCD=o.MTRLJOBCD(+) and i.PARTCD=p.PARTCD(+) and i.STKTYPE=q.STKTYPE(+) and i.ORDAUTONO=r.AUTONO(+) ";
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
                                    //BALENO = dr["BALENO"].retStr(),
                                    PDESIGN = dr["PDESIGN"].retStr(),
                                    OURDESIGN = dr["OURDESIGN"].retStr(),
                                    FLAGMTR = dr["FLAGMTR"].retDbl(),
                                    LOCABIN = dr["LOCABIN"].retStr(),
                                    BALEYR = dr["BALEYR"].retStr(),
                                    BARGENTYPE = dr["BARGENTYPE"].retStr(),
                                    //GLCD = VE.MENU_PARA == "SBPCK" ? dr["SALGLCD"].retStr() : VE.MENU_PARA == "SB" ? dr["SALGLCD"].retStr() : VE.MENU_PARA == "SBDIR" ? dr["SALGLCD"].retStr() : VE.MENU_PARA == "SR" ? dr["SALRETGLCD"].retStr() : VE.MENU_PARA == "SBCM" ? dr["SALGLCD"].retStr() : VE.MENU_PARA == "SBCMR" ? dr["SALGLCD"].retStr() : VE.MENU_PARA == "SBEXP" ? dr["SALGLCD"].retStr() : VE.MENU_PARA == "PI" ? "" : VE.MENU_PARA == "PB" ? dr["PURGLCD"].retStr() : VE.MENU_PARA == "PR" ? dr["PURRETGLCD"].retStr() : "",
                                    //WPRATE = VE.MENU_PARA == "PB" ? dr["WPRATE"].retDbl() : (double?)null,
                                    //RPRATE = VE.MENU_PARA == "PB" ? dr["RPRATE"].retDbl() : (double?)null,
                                    ITREM = dr["ITREM"].retStr(),
                                    ORDAUTONO = dr["ORDAUTONO"].retStr(),
                                    ORDSLNO = dr["ORDSLNO"].retShort(),
                                    ORDDOCNO = dr["ORDDOCNO"].retStr(),
                                    ORDDOCDT = dr["ORDDOCDT"].retStr() == "" ? "" : dr["ORDDOCDT"].retStr().Remove(10),
                                    //WPPRICEGEN = VE.MENU_PARA == "PB" ? dr["WPPRICEGEN"].retStr() : "",
                                    //RPPRICEGEN = VE.MENU_PARA == "PB" ? dr["RPPRICEGEN"].retStr() : "",
                                    RECPROGAUTONO = dr["RECPROGAUTONO"].retStr(),
                                    RECPROGSLNO = dr["RECPROGSLNO"].retShort(),
                                    MTRLCOST = dr["MTRLCOST"].retDbl(),
                                    COMMONUNIQBAR = dr["COMMONUNIQBAR"].retStr(),
                                }).OrderBy(s => s.SLNO).ToList();

                str1 = "";
                str1 += "select i.SLNO,j.ITGRPCD,k.ITGRPNM,i.MTRLJOBCD,l.MTRLJOBNM,l.MTBARCODE,i.ITCD,j.ITNM,j.STYLENO,j.UOMCD,i.STKTYPE,m.STKNAME,i.NOS,i.QNTY,i.FLAGMTR, ";
                str1 += "i.BLQNTY,i.RATE,i.AMT,i.DISCTYPE,i.DISCRATE,i.DISCAMT,i.TDDISCTYPE,i.TDDISCRATE,i.TDDISCAMT,i.SCMDISCTYPE,i.SCMDISCRATE,i.SCMDISCAMT, ";
                str1 += "i.TXBLVAL,i.IGSTPER,i.CGSTPER,i.SGSTPER,i.CESSPER,i.IGSTAMT,i.CGSTAMT,i.SGSTAMT,i.CESSAMT,i.NETAMT,i.HSNCODE,i.BALENO,i.GLCD,i.BALEYR,i.TOTDISCAMT,i.ITREM ";
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
                                  ITSTYLE = dr["STYLENO"].retStr() + "" + dr["ITNM"].retStr(),
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
                                  //BALENO = dr["BALENO"].retStr(),
                                  GLCD = dr["GLCD"].retStr(),
                                  BALEYR = dr["BALEYR"].retStr(),
                                  TOTDISCAMT = dr["TOTDISCAMT"].retDbl(),
                                  ITREM = dr["ITREM"].retStr(),
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

                if (VE.TTXNDTL != null && VE.TTXNDTL.Count() > 0)
                {
                    VE.MTRLJOBCD = VE.TTXNDTL[0].MTRLJOBCD;
                    VE.MTRLJOBNM = VE.TTXNDTL[0].MTRLJOBNM;
                }
                //fill prodgrpgstper in t_batchdtl
                DataTable allprodgrpgstper_data = new DataTable();
                string BARNO = (from a in VE.TBATCHDTL select a.BARNO).ToArray().retSqlfromStrarray();
                string ITCD = (from a in VE.TBATCHDTL select a.ITCD).ToArray().retSqlfromStrarray();
                //string MTRLJOBCD = (from a in VE.TBATCHDTL select a.MTRLJOBCD).ToArray().retSqlfromStrarray();
                string ITGRPCD = (from a in VE.TBATCHDTL select a.ITGRPCD).ToArray().retSqlfromStrarray();
                if (VE.MENU_PARA == "PB")
                {
                    allprodgrpgstper_data = salesfunc.GetBarHelp(TXN.DOCDT.retStr().Remove(10), TXN.GOCD.retStr(), BARNO.retStr(), ITCD.retStr(), "", "", ITGRPCD, "", TXNOTH.PRCCD.retStr(), TXNOTH.TAXGRPCD.retStr(), "", "", true, false, VE.MENU_PARA);

                }
                else
                {
                    allprodgrpgstper_data = salesfunc.GetStock(TXN.DOCDT.retStr().Remove(10), TXN.GOCD.retSqlformat(), BARNO.retStr(), ITCD.retStr(), "", SLR.AUTONO.retSqlformat(), ITGRPCD, "", TXNOTH.PRCCD.retStr(), TXNOTH.TAXGRPCD.retStr());
                }
                foreach (var v in VE.TBATCHDTL)
                {
                    string PRODGRPGSTPER = "", ALL_GSTPER = "", GSTPER = "";
                    v.GSTPER = VE.TTXNDTL.Where(a => a.SLNO == v.TXNSLNO).Sum(b => b.IGSTPER + b.CGSTPER + b.SGSTPER).retDbl();
                    if (allprodgrpgstper_data != null && allprodgrpgstper_data.Rows.Count > 0)
                    {
                        var DATA = allprodgrpgstper_data.Select("barno = '" + v.BARNO + "' and itcd= '" + v.ITCD + "' and itgrpcd = '" + v.ITGRPCD + "'");
                        if (DATA.Count() > 0)
                        {
                            DataTable tax_data = DATA.CopyToDataTable();
                            if (tax_data != null && tax_data.Rows.Count > 0)
                            {
                                //PRODGRPGSTPER = tax_data.Rows[0]["PRODGRPGSTPER"].retStr();
                                //if (PRODGRPGSTPER != "")
                                //{
                                //    ALL_GSTPER = salesfunc.retGstPer(PRODGRPGSTPER, v.RATE.retDbl());
                                //    if (ALL_GSTPER.retStr() != "")
                                //    {
                                //        var gst = ALL_GSTPER.Split(',').ToList();
                                //        GSTPER = (from a in gst select a.retDbl()).Sum().retStr();
                                //    }
                                //    v.PRODGRPGSTPER = PRODGRPGSTPER;
                                //    v.ALL_GSTPER = ALL_GSTPER;
                                //    v.GSTPER = GSTPER.retDbl();
                                //}
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
                    //if ((VE.MENU_PARA == "PB") && ((TXN.BARGENTYPE == "E") || (TXN.BARGENTYPE == "C" && v.BARGENTYPE == "E")))
                    //{
                    //    v.WPRATE = v.RATE * VE.WPPER;
                    //    v.RPRATE = v.RATE * VE.WPPER;
                    //}

                    //if (VE.MENU_PARA == "PB")
                    //{

                    //    if (v.WPPRICEGEN.retStr() == "" || v.RPPRICEGEN.retStr() == "")
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
                    //    }
                    //}
                    v.DISC_TYPE = masterHelp.DISC_TYPE();
                    v.SAMPLE = VE.TPROGDTL.Where(a => a.PROGAUTONO == v.RECPROGAUTONO && a.PROGSLNO == v.RECPROGSLNO).Select(b => b.SAMPLE).FirstOrDefault();
                }
                //fill prodgrpgstper in t_txndtl
                double IGST_PER = 0; double CGST_PER = 0; double SGST_PER = 0; double CESS_PER = 0; double DUTY_PER = 0;
                foreach (var v in VE.TTXNDTL)
                {
                    //string PRODGRPGSTPER = "", ALL_GSTPER = "";
                    //var tax_data = (from a in VE.TBATCHDTL
                    //                where a.TXNSLNO == v.SLNO && a.ITGRPCD == v.ITGRPCD && a.ITCD == a.ITCD && a.STKTYPE == v.STKTYPE
                    //                && a.RATE == v.RATE && a.DISCTYPE == v.DISCTYPE && a.DISCRATE == v.DISCRATE && a.TDDISCTYPE == v.TDDISCTYPE
                    //                 && a.TDDISCRATE == v.TDDISCRATE && a.SCMDISCTYPE == v.SCMDISCTYPE && a.SCMDISCRATE == v.SCMDISCRATE
                    //                select new { a.PRODGRPGSTPER, a.ALL_GSTPER }).FirstOrDefault();
                    //if (tax_data != null)
                    //{
                    //    PRODGRPGSTPER = tax_data.PRODGRPGSTPER.retStr();
                    //    ALL_GSTPER = tax_data.ALL_GSTPER.retStr();
                    //}
                    //v.PRODGRPGSTPER = PRODGRPGSTPER;
                    //v.ALL_GSTPER = ALL_GSTPER;

                    double IGST = v.IGSTPER.retDbl();
                    double CGST = v.CGSTPER.retDbl();
                    double SGST = v.SGSTPER.retDbl();
                    double CESS = v.CESSPER.retDbl();
                    double DUTY = v.DUTYPER.retDbl();

                    if (IGST > IGST_PER)
                    {
                        IGST_PER = IGST;
                    }
                    if (CGST > CGST_PER)
                    {
                        CGST_PER = CGST;
                    }
                    if (SGST > SGST_PER)
                    {
                        SGST_PER = SGST;
                    }
                    if (CESS > CESS_PER)
                    {
                        CESS_PER = CESS;
                    }
                    if (DUTY > DUTY_PER)
                    {
                        DUTY_PER = DUTY;
                    }
                    v.DISC_TYPE = masterHelp.DISC_TYPE();
                }
                string S_P = "";
                if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "PR" || VE.MENU_PARA == "OP") { S_P = "P"; } else { S_P = "S"; }
                string sql = "select a.amtcd, b.amtnm, b.calccode, b.addless, b.taxcode, b.calctype, b.calcformula, a.amtdesc, ";
                sql += "b.glcd, b.hsncode, a.amtrate, a.curr_amt, a.amt,a.igstper, a.igstamt, a.sgstper, a.sgstamt, ";
                sql += "a.cgstper, a.cgstamt,a.cessper, a.cessamt, a.dutyper, a.dutyamt ";
                sql += "from " + Scm + ".t_txnamt a, " + Scm + ".m_amttype b ";
                sql += "where a.amtcd=b.amtcd(+) and b.salpur='" + S_P + "' and a.autono='" + TXN.AUTONO + "' ";
                sql += "union ";
                sql += "select b.amtcd, b.amtnm, b.calccode, b.addless, b.taxcode, b.calctype, b.calcformula, '' amtdesc, ";
                sql += "b.glcd, b.hsncode, 0 amtrate, 0 curr_amt, 0 amt,0 igstper, 0 igstamt, 0 sgstper, 0 sgstamt, ";
                sql += "0 cgstper, 0 cgstamt, 0 CESSPER, 0 CESSAMT, 0 dutyper, 0 dutyamt ";
                sql += "from " + Scm + ".m_amttype b, " + Scm + ".m_cntrl_hdr c ";
                sql += "where b.m_autono=c.m_autono(+) and b.salpur='" + S_P + "'  and nvl(c.inactive_tag,'N') = 'N' and ";
                sql += "b.amtcd not in (select amtcd from " + Scm + ".t_txnamt where autono='" + TXN.AUTONO + "') ";
                var AMOUNT_DATA = masterHelp.SQLquery(sql);

                VE.TTXNAMT = (from DataRow dr in AMOUNT_DATA.Rows
                              select new TTXNAMT()
                              {
                                  AMTCD = dr["amtcd"].ToString(),
                                  ADDLESS = dr["addless"].ToString(),
                                  TAXCODE = dr["taxcode"].ToString(),
                                  GLCD = dr["GLCD"].ToString(),
                                  AMTNM = dr["amtnm"].ToString(),
                                  CALCCODE = dr["calccode"].ToString(),
                                  CALCTYPE = dr["CALCTYPE"].ToString(),
                                  CALCFORMULA = dr["calcformula"].ToString(),
                                  AMTDESC = dr["amtdesc"].ToString(),
                                  HSNCODE = dr["hsncode"].ToString(),
                                  AMTRATE = Convert.ToDouble(dr["amtrate"] == DBNull.Value ? null : dr["amtrate"].ToString()),
                                  CURR_AMT = Convert.ToDouble(dr["curr_amt"] == DBNull.Value ? null : dr["curr_amt"].ToString()),
                                  AMT = Convert.ToDouble(dr["amt"] == DBNull.Value ? null : dr["amt"].ToString()),
                                  IGSTPER = Convert.ToDouble(dr["igstper"] == DBNull.Value ? null : dr["igstper"].ToString()),
                                  IGSTPER_DESP = Convert.ToDouble(dr["igstper"] == DBNull.Value ? null : dr["igstper"].ToString()),
                                  CGSTPER = Convert.ToDouble(dr["cgstper"] == DBNull.Value ? null : dr["cgstper"].ToString()),
                                  CGSTPER_DESP = Convert.ToDouble(dr["cgstper"] == DBNull.Value ? null : dr["cgstper"].ToString()),
                                  SGSTPER = Convert.ToDouble(dr["sgstper"] == DBNull.Value ? null : dr["sgstper"].ToString()),
                                  SGSTPER_DESP = Convert.ToDouble(dr["sgstper"] == DBNull.Value ? null : dr["sgstper"].ToString()),
                                  CESSPER = Convert.ToDouble(dr["CESSPER"] == DBNull.Value ? null : dr["CESSPER"].ToString()),
                                  CESSPER_DESP = Convert.ToDouble(dr["CESSPER"] == DBNull.Value ? null : dr["CESSPER"].ToString()),
                                  DUTYPER = Convert.ToDouble(dr["dutyper"] == DBNull.Value ? null : dr["dutyper"].ToString()),
                                  IGSTAMT = Convert.ToDouble(dr["igstamt"] == DBNull.Value ? null : dr["igstamt"].ToString()),
                                  IGSTAMT_DESP = Convert.ToDouble(dr["igstamt"] == DBNull.Value ? null : dr["igstamt"].ToString()),
                                  CGSTAMT = Convert.ToDouble(dr["cgstamt"] == DBNull.Value ? null : dr["cgstamt"].ToString()),
                                  CGSTAMT_DESP = Convert.ToDouble(dr["cgstamt"] == DBNull.Value ? null : dr["cgstamt"].ToString()),
                                  SGSTAMT = Convert.ToDouble(dr["sgstamt"] == DBNull.Value ? null : dr["sgstamt"].ToString()),
                                  SGSTAMT_DESP = Convert.ToDouble(dr["sgstamt"] == DBNull.Value ? null : dr["sgstamt"].ToString()),
                                  CESSAMT = Convert.ToDouble(dr["CESSAMT"] == DBNull.Value ? null : dr["CESSAMT"].ToString()),
                                  CESSAMT_DESP = Convert.ToDouble(dr["CESSAMT"] == DBNull.Value ? null : dr["CESSAMT"].ToString()),
                                  DUTYAMT = Convert.ToDouble(dr["dutyamt"] == DBNull.Value ? null : dr["dutyamt"].ToString()),
                              }).ToList();

                double A_IGST_AMT = 0; double A_CGST_AMT = 0; double A_SGST_AMT = 0; double A_CESS_AMT = 0; double A_DUTY_AMT = 0; double A_TOTAL_CURR = 0; double A_TOTAL_AMOUNT = 0; double A_TOTAL_NETAMOUNT = 0;

                for (int p = 0; p <= VE.TTXNAMT.Count - 1; p++)
                {
                    VE.TTXNAMT[p].SLNO = Convert.ToInt16(p + 1);

                    if (VE.TTXNAMT[p].IGSTPER == 0) { VE.TTXNAMT[p].IGSTPER = IGST_PER; }
                    if (VE.TTXNAMT[p].IGSTPER_DESP == 0) { VE.TTXNAMT[p].IGSTPER_DESP = IGST_PER; }
                    if (VE.TTXNAMT[p].CGSTPER == 0) { VE.TTXNAMT[p].CGSTPER = CGST_PER; }
                    if (VE.TTXNAMT[p].CGSTPER_DESP == 0) { VE.TTXNAMT[p].CGSTPER_DESP = CGST_PER; }
                    if (VE.TTXNAMT[p].SGSTPER == 0) { VE.TTXNAMT[p].SGSTPER = SGST_PER; }
                    if (VE.TTXNAMT[p].SGSTPER_DESP == 0) { VE.TTXNAMT[p].SGSTPER_DESP = SGST_PER; }
                    if (VE.TTXNAMT[p].CESSPER == 0) { VE.TTXNAMT[p].CESSPER = CESS_PER; }
                    if (VE.TTXNAMT[p].CESSPER_DESP == 0) { VE.TTXNAMT[p].CESSPER_DESP = CESS_PER; }
                    if (VE.TTXNAMT[p].DUTYPER == 0) { VE.TTXNAMT[p].DUTYPER = DUTY_PER; }

                    A_TOTAL_CURR = A_TOTAL_CURR + VE.TTXNAMT[p].CURR_AMT.Value;
                    A_TOTAL_AMOUNT = A_TOTAL_AMOUNT + VE.TTXNAMT[p].AMT.Value;
                    A_IGST_AMT = A_IGST_AMT + VE.TTXNAMT[p].IGSTAMT.Value;
                    A_CGST_AMT = A_CGST_AMT + VE.TTXNAMT[p].CGSTAMT.Value;
                    A_SGST_AMT = A_SGST_AMT + VE.TTXNAMT[p].SGSTAMT.Value;
                    A_CESS_AMT = A_CESS_AMT + VE.TTXNAMT[p].CESSAMT.Value;
                    A_DUTY_AMT = A_DUTY_AMT + VE.TTXNAMT[p].DUTYAMT.Value;
                    VE.TTXNAMT[p].NETAMT = VE.TTXNAMT[p].AMT.Value + VE.TTXNAMT[p].IGSTAMT.Value + VE.TTXNAMT[p].CGSTAMT.Value + VE.TTXNAMT[p].SGSTAMT.Value + VE.TTXNAMT[p].CESSAMT.Value + VE.TTXNAMT[p].DUTYAMT.Value;
                    A_TOTAL_NETAMOUNT = A_TOTAL_NETAMOUNT + VE.TTXNAMT[p].NETAMT.Value;
                }
                VE.A_T_CURR = A_TOTAL_CURR;
                VE.A_T_AMOUNT = A_TOTAL_AMOUNT;
                VE.A_T_NET = A_TOTAL_NETAMOUNT;
                VE.A_T_IGST = A_IGST_AMT;
                VE.A_T_CGST = A_CGST_AMT;
                VE.A_T_SGST = A_SGST_AMT;
                VE.A_T_CESS = A_CESS_AMT;
                VE.A_T_DUTY = A_DUTY_AMT;

                //total amt
                VE.TOTNOS = VE.T_NOS.retDbl();
                VE.TOTQNTY = VE.T_QNTY.retDbl();
                VE.TOTTAXVAL = VE.T_GROSS_AMT.retDbl() + VE.A_T_AMOUNT.retDbl();
                VE.TOTTAX = VE.T_CGST_AMT.retDbl() + VE.A_T_CGST.retDbl() + VE.T_SGST_AMT.retDbl() + VE.A_T_SGST.retDbl() + VE.T_IGST_AMT.retDbl() + VE.A_T_IGST.retDbl();


            }
            //Cn.DateLock_Entry(VE, DB, TCH.DOCDT.Value);
            if (TCH.CANCEL == "Y") VE.CancelRecord = true; else VE.CancelRecord = false;
            return VE;
        }
        //public TransactionOutRecProcess Navigation(TransactionOutRecProcess VE, ImprovarDB DB, int index, string searchValue)
        //{
        //    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
        //    ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
        //    string COM_CD = CommVar.Compcd(UNQSNO);
        //    string LOC_CD = CommVar.Loccd(UNQSNO);
        //    string DATABASE = CommVar.CurSchema(UNQSNO).ToString();
        //    string DATABASEF = CommVar.FinSchema(UNQSNO);
        //    Cn.getQueryString(VE);

        //    TXN = new T_TXN(); TXNTRN = new T_TXNTRANS(); TXNOTH = new T_TXNOTH(); TCH = new T_CNTRL_HDR(); SLR = new T_CNTRL_HDR_REM();
        //    if (VE.IndexKey.Count != 0)
        //    {
        //        string[] aa = null;
        //        if (searchValue.Length == 0)
        //        {
        //            aa = VE.IndexKey[index].Navikey.Split(Convert.ToChar(Cn.GCS()));
        //        }
        //        else
        //        {
        //            aa = searchValue.Split(Convert.ToChar(Cn.GCS()));
        //        }
        //        TXN = DB.T_TXN.Find(aa[0].Trim());
        //        TCH = DB.T_CNTRL_HDR.Find(TXN.AUTONO);
        //        //if (TXN.ROYN == "Y") VE.RoundOff = true;
        //        //else VE.RoundOff = false;
        //        //TXNTRN = DB.T_TXNTRANS.Find(TXN.AUTONO);
        //        TXNOTH = DB.T_TXNOTH.Find(TXN.AUTONO);
        //        if (TXN.SLCD.retStr() != "")
        //        {
        //            string slcd = TXN.SLCD;
        //            var subleg = (from a in DBF.M_SUBLEG where a.SLCD == slcd select new { a.SLNM, a.SLAREA, a.DISTRICT, a.GSTNO }).FirstOrDefault();
        //            VE.SLNM = subleg.SLNM;
        //            VE.DISTRICT = subleg.DISTRICT;
        //            VE.GSTNO = subleg.GSTNO;
        //        }
        //        VE.JOBNM = TXN.JOBCD.retStr() == "" ? "" : DB.M_JOBMST.Where(a => a.JOBCD == TXN.JOBCD).Select(b => b.JOBNM).FirstOrDefault();
        //        VE.GONM = TXN.GOCD.retStr() == "" ? "" : DB.M_GODOWN.Where(a => a.GOCD == TXN.GOCD).Select(b => b.GONM).FirstOrDefault();
        //        //VE.CONSLNM = TXN.CONSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXN.CONSLCD).Select(b => b.SLNM).FirstOrDefault();
        //        //VE.AGSLNM = TXNOTH.AGSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXNOTH.AGSLCD).Select(b => b.SLNM).FirstOrDefault();
        //        //VE.SAGSLNM = TXNOTH.SAGSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXNOTH.SAGSLCD).Select(b => b.SLNM).FirstOrDefault();

        //        VE.PRCNM = TXNOTH.PRCCD.retStr() == "" ? "" : DBF.M_PRCLST.Where(a => a.PRCCD == TXNOTH.PRCCD).Select(b => b.PRCNM).FirstOrDefault();

        //        //VE.TRANSLNM = TXNTRN.TRANSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXNTRN.TRANSLCD).Select(b => b.SLNM).FirstOrDefault();
        //        //VE.CRSLNM = TXNTRN.CRSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXNTRN.CRSLCD).Select(b => b.SLNM).FirstOrDefault();
        //        SLR = Cn.GetTransactionReamrks(CommVar.CurSchema(UNQSNO).ToString(), TXN.AUTONO);
        //        VE.UploadDOC = Cn.GetUploadImageTransaction(CommVar.CurSchema(UNQSNO).ToString(), TXN.AUTONO);
        //        string Scm = CommVar.CurSchema(UNQSNO); double TOTAL_NOS = 0; double TOTAL_QNTY = 0; double TOTAL_BOMQNTY = 0; double TOTAL_EXTRAQNTY = 0; double TOTAL_QQNTY = 0;
        //        string str = "";
        //        str += "select a.autono,a.slno,a.nos,a.qnty,a.itcd,a.sizecd,a.partcd,a.colrcd,a.mtrljobcd,k.itgrpcd,k.uomcd,k.styleno,itgrpnm,k.itnm,l.sizenm,m.colrnm,p.partnm,o.mtrljobnm, ";
        //        str += "a.itremark,a.shade,a.cutlength,a.sample, k.itnm||' '||k.styleno itstyle,a.barno from " + Scm + ".T_PROGMAST a," + Scm + ".T_PROGDTL b ,";
        //        str += Scm + ".M_SITEM k, " + Scm + ".M_SIZE l, " + Scm + ".M_COLOR m, ";
        //        str += Scm + ".M_GROUP n," + Scm + ".M_MTRLJOBMST o," + Scm + ".M_PARTS p ";
        //        str += " where a.autono=b.autono(+) and a.slno=b.slno(+) and a.ITCD = k.ITCD(+) ";
        //        str += " and a.SIZECD = l.SIZECD(+) and a.COLRCD = m.COLRCD(+) and k.ITGRPCD=n.ITGRPCD(+) and ";
        //        str += " a.MTRLJOBCD=o.MTRLJOBCD(+) and a.PARTCD=p.PARTCD(+) and a.autono='" + TXN.AUTONO + "'";
        //        str += "order by a.slno ";

        //        DataTable Progdtltbl = masterHelp.SQLquery(str);
        //        VE.TPROGDTL = (from DataRow dr in Progdtltbl.Rows
        //                       select new TPROGDTL()
        //                       {
        //                           SLNO = Convert.ToInt16(dr["slno"]),
        //                           NOS = dr["nos"].retDbl(),
        //                           QNTY = dr["qnty"].retDbl(),
        //                           ITGRPCD = dr["itgrpcd"].retStr(),
        //                           ITGRPNM = dr["itgrpnm"].retStr(),
        //                           ITCD = dr["itcd"].retStr(),
        //                           ITNM = dr["itstyle"].retStr(),
        //                           UOM = dr["uomcd"].retStr(),
        //                           SIZECD = dr["sizecd"].retStr(),
        //                           SIZENM = dr["sizenm"].retStr(),
        //                           PARTCD = dr["partcd"].retStr(),
        //                           PARTNM = dr["partnm"].retStr(),
        //                           COLRCD = dr["colrcd"].retStr(),
        //                           COLRNM = dr["colrnm"].retStr(),
        //                           MTRLJOBCD = dr["mtrljobcd"].retStr(),
        //                           MTRLJOBNM = dr["mtrljobnm"].retStr(),
        //                           ITREMARK = dr["itremark"].retStr(),
        //                           SHADE = dr["shade"].retStr(),
        //                           CUTLENGTH = dr["cutlength"].retDbl(),
        //                           SAMPLE = dr["sample"].retStr(),
        //                           BARNO = dr["barno"].retStr()

        //                       }).OrderBy(s => s.SLNO).ToList();
        //        foreach (var q in VE.TPROGDTL)
        //        {
        //            if (q.SAMPLE == "Y") q.CheckedSample = true;
        //            TOTAL_NOS = TOTAL_NOS + (q.NOS == null ? 0 : q.NOS.Value);
        //            TOTAL_QNTY = TOTAL_QNTY + (q.QNTY == null ? 0 : q.QNTY.Value);
        //        }
        //        VE.P_T_NOS = TOTAL_NOS.retInt();
        //        VE.P_T_QNTY = TOTAL_QNTY.retInt();


        //        string str2 = "";
        //        str2 += "select a.autono,a.slno,a.rslno,a.qnty,a.bomqnty,a.extraqnty,a.itcd,a.sizecd,a.partcd,a.colrcd,a.mtrljobcd,k1.itgrpcd,n.itgrpnm, ";
        //        str2 += " k.itnm,l.sizenm,m.colrnm,p.partnm,o.mtrljobnm,k.uomcd,b.qnty qntyMst, ";
        //        str2 += "a.sample,k.itnm||' '||k.styleno itstyle,k1.itnm||' '||k1.styleno itstyle1,k1.uomcd Quomcd,b.barno from " + Scm + ".T_PROGBOM a," + Scm + ".T_PROGMAST b ,";
        //        str2 += Scm + ".M_SITEM k, " + Scm + ".M_SITEM k1, " + Scm + ".M_SIZE l, " + Scm + ".M_COLOR m, ";
        //        str2 += Scm + ".M_GROUP n," + Scm + ".M_MTRLJOBMST o," + Scm + ".M_PARTS p ";
        //        str2 += " where a.autono=b.autono(+) and a.slno=b.slno(+) and a.ITCD = k1.ITCD(+) and b.ITCD = k.ITCD(+)  ";
        //        str2 += " and a.SIZECD = l.SIZECD(+) and a.COLRCD = m.COLRCD(+) and k1.ITGRPCD=n.ITGRPCD(+) and ";
        //        str2 += " a.MTRLJOBCD=o.MTRLJOBCD(+) and a.PARTCD=p.PARTCD(+) and a.autono='" + TXN.AUTONO + "'";
        //        str2 += "order by a.slno ";

        //        DataTable Progbom = masterHelp.SQLquery(str2);
        //        VE.TPROGBOM = (from DataRow dr in Progbom.Rows
        //                       select new TPROGBOM()
        //                       {
        //                           SLNO = Convert.ToInt16(dr["slno"]),
        //                           RSLNO = Convert.ToInt16(dr["rslno"]),
        //                           QQNTY = dr["qntyMst"].retDbl(),
        //                           QNTY = dr["qnty"].retDbl(),
        //                           QITNM = dr["itstyle1"].retStr(),
        //                           QUOM = dr["Quomcd"].retStr(),
        //                           BARNO = dr["barno"].retStr(),
        //                           ITGRPCD = dr["itgrpcd"].retStr(),
        //                           ITGRPNM = dr["itgrpnm"].retStr(),
        //                           ITCD = dr["itcd"].retStr(),
        //                           ITNM = dr["itstyle"].retStr(),
        //                           SIZECD = dr["sizecd"].retStr(),
        //                           SIZENM = dr["sizenm"].retStr(),
        //                           PARTCD = dr["partcd"].retStr(),
        //                           PARTNM = dr["partnm"].retStr(),
        //                           COLRCD = dr["colrcd"].retStr(),
        //                           COLRNM = dr["colrnm"].retStr(),
        //                           MTRLJOBCD = dr["mtrljobcd"].retStr(),
        //                           MTRLJOBNM = dr["mtrljobnm"].retStr(),
        //                           UOM = dr["uomcd"].retStr(),
        //                           BOMQNTY = dr["bomqnty"].retDbl(),
        //                           EXTRAQNTY = dr["extraqnty"].retDbl(),
        //                           Q_SAMPLE = dr["sample"].retStr()
        //                       }).OrderBy(s => s.SLNO).ToList();
        //        foreach (var q in VE.TPROGBOM)
        //        {
        //            if (q.Q_SAMPLE == "Y") q.Q_CheckedSample = true;
        //            TOTAL_QQNTY = TOTAL_QQNTY + (q.QQNTY == null ? 0 : q.QQNTY.Value);
        //            TOTAL_BOMQNTY = TOTAL_BOMQNTY + (q.BOMQNTY == null ? 0 : q.BOMQNTY.Value);
        //            TOTAL_EXTRAQNTY = TOTAL_EXTRAQNTY + (q.EXTRAQNTY == null ? 0 : q.EXTRAQNTY.Value);
        //        }
        //        VE.T_QQNTY = TOTAL_QQNTY.retInt();
        //        VE.T_BOMQNTY = TOTAL_BOMQNTY.retInt();
        //        VE.T_EXTRAQNTY = TOTAL_EXTRAQNTY.retInt();

        //        #region batch and detail data
        //        string str1 = "";
        //        str1 += "select i.SLNO,i.TXNSLNO,k.ITGRPCD,n.ITGRPNM,n.BARGENTYPE,i.MTRLJOBCD,o.MTRLJOBNM,o.MTBARCODE,k.ITCD,k.ITNM,k.UOMCD,k.STYLENO,i.PARTCD,p.PARTNM,p.PRTBARCODE,j.STKTYPE,q.STKNAME,i.BARNO, ";
        //        str1 += "j.COLRCD,m.COLRNM,m.CLRBARCODE,j.SIZECD,l.SIZENM,l.SZBARCODE,i.SHADE,i.QNTY,i.NOS,i.RATE,i.DISCRATE,i.DISCTYPE,i.TDDISCRATE,i.TDDISCTYPE,i.SCMDISCTYPE,i.SCMDISCRATE,i.HSNCODE,i.BALENO,j.PDESIGN,j.OURDESIGN,i.FLAGMTR,i.LOCABIN,i.BALEYR ";
        //        str1 += ",n.SALGLCD,n.PURGLCD,n.SALRETGLCD,n.PURRETGLCD ";
        //        str1 += "from " + Scm + ".T_BATCHDTL i, " + Scm + ".T_BATCHMST j, " + Scm + ".M_SITEM k, " + Scm + ".M_SIZE l, " + Scm + ".M_COLOR m, ";
        //        str1 += Scm + ".M_GROUP n," + Scm + ".M_MTRLJOBMST o," + Scm + ".M_PARTS p," + Scm + ".M_STKTYPE q ";
        //        str1 += "where i.BARNO = j.BARNO(+) and j.ITCD = k.ITCD(+) and j.SIZECD = l.SIZECD(+) and j.COLRCD = m.COLRCD(+) and k.ITGRPCD=n.ITGRPCD(+) ";
        //        str1 += "and i.MTRLJOBCD=o.MTRLJOBCD(+) and i.PARTCD=p.PARTCD(+) and j.STKTYPE=q.STKTYPE(+) ";
        //        str1 += "and i.AUTONO = '" + TXN.AUTONO + "' ";
        //        str1 += "order by i.SLNO ";
        //        DataTable tbl = masterHelp.SQLquery(str1);

        //        VE.TBATCHDTL = (from DataRow dr in tbl.Rows
        //                        select new TBATCHDTL()
        //                        {
        //                            SLNO = dr["SLNO"].retShort(),
        //                            TXNSLNO = dr["TXNSLNO"].retShort(),
        //                            ITGRPCD = dr["ITGRPCD"].retStr(),
        //                            ITGRPNM = dr["ITGRPNM"].retStr(),
        //                            MTRLJOBCD = dr["MTRLJOBCD"].retStr(),
        //                            MTRLJOBNM = dr["MTRLJOBNM"].retStr(),
        //                            MTBARCODE = dr["MTBARCODE"].retStr(),
        //                            ITCD = dr["ITCD"].retStr(),
        //                            ITSTYLE = dr["STYLENO"].retStr() + "" + dr["ITNM"].retStr(),
        //                            UOM = dr["UOMCD"].retStr(),
        //                            STYLENO = dr["STYLENO"].retStr(),
        //                            PARTCD = dr["PARTCD"].retStr(),
        //                            PARTNM = dr["PARTNM"].retStr(),
        //                            PRTBARCODE = dr["PRTBARCODE"].retStr(),
        //                            COLRCD = dr["COLRCD"].retStr(),
        //                            COLRNM = dr["COLRNM"].retStr(),
        //                            CLRBARCODE = dr["CLRBARCODE"].retStr(),
        //                            SIZECD = dr["SIZECD"].retStr(),
        //                            SIZENM = dr["SIZENM"].retStr(),
        //                            SZBARCODE = dr["SZBARCODE"].retStr(),
        //                            SHADE = dr["SHADE"].retStr(),
        //                            QNTY = dr["QNTY"].retDbl(),
        //                            NOS = dr["NOS"].retDbl(),
        //                            RATE = dr["RATE"].retDbl(),
        //                            DISCRATE = dr["DISCRATE"].retDbl(),
        //                            DISCTYPE = dr["DISCTYPE"].retStr(),
        //                            DISCTYPE_DESC = dr["DISCTYPE"].retStr() == "P" ? "%" : dr["DISCTYPE"].retStr() == "N" ? "Nos" : dr["DISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
        //                            TDDISCRATE = dr["TDDISCRATE"].retDbl(),
        //                            TDDISCTYPE_DESC = dr["TDDISCTYPE"].retStr() == "P" ? "%" : dr["TDDISCTYPE"].retStr() == "N" ? "Nos" : dr["TDDISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
        //                            TDDISCTYPE = dr["TDDISCTYPE"].retStr(),
        //                            SCMDISCTYPE_DESC = dr["SCMDISCTYPE"].retStr() == "P" ? "%" : dr["SCMDISCTYPE"].retStr() == "N" ? "Nos" : dr["SCMDISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
        //                            SCMDISCTYPE = dr["SCMDISCTYPE"].retStr(),
        //                            SCMDISCRATE = dr["SCMDISCRATE"].retDbl(),
        //                            STKTYPE = dr["STKTYPE"].retStr(),
        //                            STKNAME = dr["STKNAME"].retStr(),
        //                            BARNO = dr["BARNO"].retStr(),
        //                            HSNCODE = dr["HSNCODE"].retStr(),
        //                            BALENO = dr["BALENO"].retStr(),
        //                            PDESIGN = dr["PDESIGN"].retStr(),
        //                            OURDESIGN = dr["OURDESIGN"].retStr(),
        //                            FLAGMTR = dr["FLAGMTR"].retDbl(),
        //                            LOCABIN = dr["LOCABIN"].retStr(),
        //                            BALEYR = dr["BALEYR"].retStr(),
        //                            BARGENTYPE = dr["BARGENTYPE"].retStr(),
        //                            //GLCD = VE.MENU_PARA == "SBPCK" ? dr["SALGLCD"].retStr() : VE.MENU_PARA == "SB" ? dr["SALGLCD"].retStr() : VE.MENU_PARA == "SBDIR" ? dr["SALGLCD"].retStr() : VE.MENU_PARA == "SR" ? dr["SALRETGLCD"].retStr() : VE.MENU_PARA == "SBCM" ? dr["SALGLCD"].retStr() : VE.MENU_PARA == "SBCMR" ? dr["SALGLCD"].retStr() : VE.MENU_PARA == "SBEXP" ? dr["SALGLCD"].retStr() : VE.MENU_PARA == "PI" ? "" : VE.MENU_PARA == "PB" ? dr["PURGLCD"].retStr() : VE.MENU_PARA == "PR" ? dr["PURRETGLCD"].retStr() : "",
        //                        }).OrderBy(s => s.SLNO).ToList();

        //        str1 = "";
        //        str1 += "select i.SLNO,j.ITGRPCD,k.ITGRPNM,i.MTRLJOBCD,l.MTRLJOBNM,l.MTBARCODE,i.ITCD,j.ITNM,j.STYLENO,j.UOMCD,i.STKTYPE,m.STKNAME,i.NOS,i.QNTY,i.FLAGMTR, ";
        //        str1 += "i.BLQNTY,i.RATE,i.AMT,i.DISCTYPE,i.DISCRATE,i.DISCAMT,i.TDDISCTYPE,i.TDDISCRATE,i.TDDISCAMT,i.SCMDISCTYPE,i.SCMDISCRATE,i.SCMDISCAMT, ";
        //        str1 += "i.TXBLVAL,i.IGSTPER,i.CGSTPER,i.SGSTPER,i.CESSPER,i.IGSTAMT,i.CGSTAMT,i.SGSTAMT,i.CESSAMT,i.NETAMT,i.HSNCODE,i.BALENO,i.GLCD,i.BALEYR,i.TOTDISCAMT ";
        //        str1 += "from " + Scm + ".T_TXNDTL i, " + Scm + ".M_SITEM j, " + Scm + ".M_GROUP k, " + Scm + ".M_MTRLJOBMST l, " + Scm + ".M_STKTYPE m ";
        //        str1 += "where i.ITCD = j.ITCD(+) and j.ITGRPCD=k.ITGRPCD(+) and i.MTRLJOBCD=l.MTRLJOBCD(+)  and i.STKTYPE=m.STKTYPE(+)  ";
        //        str1 += "and i.AUTONO = '" + TXN.AUTONO + "' ";
        //        str1 += "order by i.SLNO ";
        //        tbl = masterHelp.SQLquery(str1);

        //        VE.TTXNDTL = (from DataRow dr in tbl.Rows
        //                      select new TTXNDTL()
        //                      {
        //                          SLNO = dr["SLNO"].retShort(),
        //                          ITGRPCD = dr["ITGRPCD"].retStr(),
        //                          ITGRPNM = dr["ITGRPNM"].retStr(),
        //                          MTRLJOBCD = dr["MTRLJOBCD"].retStr(),
        //                          MTRLJOBNM = dr["MTRLJOBNM"].retStr(),
        //                          //MTBARCODE = dr["MTBARCODE"].retStr(),
        //                          ITCD = dr["ITCD"].retStr(),
        //                          ITSTYLE = dr["STYLENO"].retStr() + " " + dr["ITNM"].retStr(),
        //                          UOM = dr["UOMCD"].retStr(),
        //                          STKTYPE = dr["STKTYPE"].retStr(),
        //                          STKNAME = dr["STKNAME"].retStr(),
        //                          NOS = dr["NOS"].retDbl(),
        //                          QNTY = dr["QNTY"].retDbl(),
        //                          FLAGMTR = dr["FLAGMTR"].retDbl(),
        //                          BLQNTY = dr["BLQNTY"].retDbl(),
        //                          RATE = dr["RATE"].retDbl(),
        //                          AMT = dr["AMT"].retDbl(),
        //                          DISCTYPE = dr["DISCTYPE"].retStr(),
        //                          DISCTYPE_DESC = dr["DISCTYPE"].retStr() == "P" ? "%" : dr["DISCTYPE"].retStr() == "N" ? "Nos" : dr["DISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
        //                          DISCRATE = dr["DISCRATE"].retDbl(),
        //                          DISCAMT = dr["DISCAMT"].retDbl(),
        //                          TDDISCTYPE_DESC = dr["TDDISCTYPE"].retStr() == "P" ? "%" : dr["TDDISCTYPE"].retStr() == "N" ? "Nos" : dr["TDDISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
        //                          TDDISCTYPE = dr["TDDISCTYPE"].retStr(),
        //                          TDDISCRATE = dr["TDDISCRATE"].retDbl(),
        //                          TDDISCAMT = dr["TDDISCAMT"].retDbl(),
        //                          SCMDISCTYPE_DESC = dr["SCMDISCTYPE"].retStr() == "P" ? "%" : dr["SCMDISCTYPE"].retStr() == "N" ? "Nos" : dr["SCMDISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
        //                          SCMDISCTYPE = dr["SCMDISCTYPE"].retStr(),
        //                          SCMDISCRATE = dr["SCMDISCRATE"].retDbl(),
        //                          SCMDISCAMT = dr["SCMDISCAMT"].retDbl(),
        //                          TXBLVAL = dr["TXBLVAL"].retDbl(),
        //                          IGSTPER = dr["IGSTPER"].retDbl(),
        //                          CGSTPER = dr["CGSTPER"].retDbl(),
        //                          SGSTPER = dr["SGSTPER"].retDbl(),
        //                          CESSPER = dr["CESSPER"].retDbl(),
        //                          IGSTAMT = dr["IGSTAMT"].retDbl(),
        //                          CGSTAMT = dr["CGSTAMT"].retDbl(),
        //                          SGSTAMT = dr["SGSTAMT"].retDbl(),
        //                          CESSAMT = dr["CESSAMT"].retDbl(),
        //                          NETAMT = dr["NETAMT"].retDbl(),
        //                          HSNCODE = dr["HSNCODE"].retStr(),
        //                          BALENO = dr["BALENO"].retStr(),
        //                          GLCD = dr["GLCD"].retStr(),
        //                          BALEYR = dr["BALEYR"].retStr(),
        //                          TOTDISCAMT = dr["TOTDISCAMT"].retDbl()
        //                      }).OrderBy(s => s.SLNO).ToList();

        //        VE.B_T_QNTY = VE.TBATCHDTL.Sum(a => a.QNTY).retDbl();
        //        VE.B_T_NOS = VE.TBATCHDTL.Sum(a => a.NOS).retDbl();
        //        VE.T_NOS = VE.TTXNDTL.Sum(a => a.NOS).retDbl();
        //        VE.T_QNTY = VE.TTXNDTL.Sum(a => a.QNTY).retDbl();
        //        VE.T_AMT = VE.TTXNDTL.Sum(a => a.AMT).retDbl();
        //        VE.T_GROSS_AMT = VE.TTXNDTL.Sum(a => a.TXBLVAL).retDbl();
        //        VE.T_IGST_AMT = VE.TTXNDTL.Sum(a => a.IGSTAMT).retDbl();
        //        VE.T_CGST_AMT = VE.TTXNDTL.Sum(a => a.CGSTAMT).retDbl();
        //        VE.T_SGST_AMT = VE.TTXNDTL.Sum(a => a.SGSTAMT).retDbl();
        //        VE.T_CESS_AMT = VE.TTXNDTL.Sum(a => a.CESSAMT).retDbl();
        //        VE.T_NET_AMT = VE.TTXNDTL.Sum(a => a.NETAMT).retDbl();

        //        //fill prodgrpgstper in t_batchdtl
        //        DataTable allprodgrpgstper_data = new DataTable();
        //        string BARNO = (from a in VE.TBATCHDTL select a.BARNO).ToArray().retSqlfromStrarray();
        //        string ITCD = (from a in VE.TBATCHDTL select a.ITCD).ToArray().retSqlfromStrarray();
        //        string MTRLJOBCD = (from a in VE.TBATCHDTL select a.MTRLJOBCD).ToArray().retSqlfromStrarray();
        //        string ITGRPCD = (from a in VE.TBATCHDTL select a.ITGRPCD).ToArray().retSqlfromStrarray();

        //        allprodgrpgstper_data = salesfunc.GetStock(TXN.DOCDT.retStr().Remove(10), TXN.GOCD.retStr(), BARNO.retStr(), ITCD.retStr(), "", "", ITGRPCD, "", TXNOTH.PRCCD.retStr(), TXNOTH.TAXGRPCD.retStr());

        //        foreach (var v in VE.TBATCHDTL)
        //        {
        //            string PRODGRPGSTPER = "", ALL_GSTPER = "", GSTPER = "";
        //            v.GSTPER = VE.TTXNDTL.Where(a => a.SLNO == v.TXNSLNO).Sum(b => b.IGSTPER + b.CGSTPER + b.SGSTPER).retDbl();
        //            if (allprodgrpgstper_data != null && allprodgrpgstper_data.Rows.Count > 0)
        //            {
        //                var DATA = allprodgrpgstper_data.Select("barno = '" + v.BARNO + "' and itcd= '" + v.ITCD + "' and itgrpcd = '" + v.ITGRPCD + "'");
        //                if (DATA.Count() > 0)
        //                {
        //                    DataTable tax_data = DATA.CopyToDataTable();
        //                    if (tax_data != null && tax_data.Rows.Count > 0)
        //                    {
        //                        PRODGRPGSTPER = tax_data.Rows[0]["PRODGRPGSTPER"].retStr();
        //                        if (PRODGRPGSTPER != "")
        //                        {
        //                            ALL_GSTPER = salesfunc.retGstPer(PRODGRPGSTPER, v.RATE.retDbl());
        //                            if (ALL_GSTPER.retStr() != "")
        //                            {
        //                                var gst = ALL_GSTPER.Split(',').ToList();
        //                                GSTPER = (from a in gst select a.retDbl()).Sum().retStr();
        //                            }
        //                            v.PRODGRPGSTPER = PRODGRPGSTPER;
        //                            v.ALL_GSTPER = ALL_GSTPER;
        //                            v.GSTPER = GSTPER.retDbl();
        //                        }
        //                    }
        //                }
        //            }

        //            //checking childdata exist against barno
        //            var chk_child = (from a in DB.T_BATCHDTL where a.BARNO == v.BARNO && a.AUTONO != TXN.AUTONO select a).ToList();
        //            if (chk_child.Count() > 0)
        //            {
        //                v.ChildData = "Y";
        //            }

        //        }
        //        //fill prodgrpgstper in t_txndtl
        //        foreach (var v in VE.TTXNDTL)
        //        {
        //            string PRODGRPGSTPER = "", ALL_GSTPER = "";
        //            var tax_data = (from a in VE.TBATCHDTL
        //                            where a.TXNSLNO == v.SLNO && a.ITGRPCD == v.ITGRPCD && a.ITCD == a.ITCD && a.STKTYPE == v.STKTYPE
        //                            && a.RATE == v.RATE && a.DISCTYPE == v.DISCTYPE && a.DISCRATE == v.DISCRATE && a.TDDISCTYPE == v.TDDISCTYPE
        //                             && a.TDDISCRATE == v.TDDISCRATE && a.SCMDISCTYPE == v.SCMDISCTYPE && a.SCMDISCRATE == v.SCMDISCRATE
        //                            select new { a.PRODGRPGSTPER, a.ALL_GSTPER }).FirstOrDefault();
        //            if (tax_data != null)
        //            {
        //                PRODGRPGSTPER = tax_data.PRODGRPGSTPER.retStr();
        //                ALL_GSTPER = tax_data.ALL_GSTPER.retStr();
        //            }
        //            v.PRODGRPGSTPER = PRODGRPGSTPER;
        //            v.ALL_GSTPER = ALL_GSTPER;
        //        }
        //        #endregion




        //    }
        //    //Cn.DateLock_Entry(VE, DB, TCH.DOCDT.Value);
        //    if (TCH.CANCEL == "Y") VE.CancelRecord = true; else VE.CancelRecord = false;
        //    return VE;
        //}
        public ActionResult SearchPannelData(TransactionOutRecProcess VE, string SRC_SLCD, string SRC_DOCNO, string SRC_FDT, string SRC_TDT)
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
                var str = masterHelp.JOBCD_help(val);
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
        public ActionResult GetPartyDetails(string val, string Code)
        {
            try
            {

                if (Code == "")
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
                        var party_data = salesfunc.GetSlcdDetails(val, Code);
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
        public ActionResult GetTDSDetails(string val, string Code)
        {
            try
            {
                var data = Code.Split(Convert.ToChar(Cn.GCS()));
                string docdt = data[0];
                string PARTY = "";
                if (data.Length > 1)
                {
                    PARTY = data[1].retStr();
                }
                if (docdt.retStr() == "") return Content("Enter Document Date");
                if (val == null)
                {
                    return PartialView("_Help2", masterHelp.TDSCODE_help(docdt, val, PARTY, "", ""));
                }
                else
                {
                    string str = masterHelp.TDSCODE_help(docdt, val, PARTY, "", "");
                    return Content(str);
                }
            }
            catch (Exception Ex)
            {
                Cn.SaveException(Ex, "");
                return Content(Ex.Message + Ex.InnerException);
            }
        }
        public ActionResult GetSubLedgerDetails(string val, string Code)
        {
            try
            {

                var str = masterHelp.SLCD_help(val, Code);
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
        public dynamic GetPendingProgramme(string docdt, string jobcd, string slcd, string autono, string flag = "")
        {
            try
            {
                TransactionOutRecProcess VE = new TransactionOutRecProcess();
                Cn.getQueryString(VE);
                jobcd = jobcd.retStr() == "" ? "" : jobcd.retStr().retSqlformat();
                slcd = slcd.retStr() == "" ? "" : slcd.retStr().retSqlformat();
                var pendprg_data = salesfunc.getPendProg(docdt.retStr(), "", slcd.retStr(), "", jobcd.retStr(), autono.retStr());
                string res = "";
                if (pendprg_data != null && pendprg_data.Rows.Count > 0)
                {
                    TempData["PENDPROGRAMME" + VE.MENU_PARA] = pendprg_data;
                    res = "1";
                }
                else
                {
                    TempData["PENDPROGRAMME" + VE.MENU_PARA] = pendprg_data.Clone();
                    res = "There was no Pending Programme !! Please select another Party..";
                }
                if (flag == "Y") return res;
                return Content(res);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetBarCodeDetails(TransactionOutRecProcess VE, string val, string Code)
        {
            try
            {
                Cn.getQueryString(VE);
                var temptbl = (DataTable)TempData["PENDPROGRAMME" + VE.MENU_PARA]; TempData.Keep();
                if (temptbl == null) { return Content("Please click on pending programme to get data!! "); }
                DataTable tbl = new DataView(temptbl).ToTable();

                if (VE.TPROGDTL != null)
                {
                    for (int i = 0; i <= tbl.Rows.Count - 1; i++)
                    {
                        string progautoslno = tbl.Rows[i]["PROGAUTOSLNO"].retStr();
                        var griddata = VE.TPROGDTL.Where(a => a.PROGAUTOSLNO == progautoslno).Select(b => new { b.QNTY, b.NOS }).ToList();
                        if (griddata.Count > 0)
                        {
                            var totalgridqnty = griddata.Select(a => a.QNTY).Sum();
                            var totalgridnos = griddata.Select(a => a.NOS).Sum();
                            tbl.Rows[i]["balnos"] = tbl.Rows[i]["balnos"].retDbl() - totalgridnos.retDbl();
                            tbl.Rows[i]["balqnty"] = tbl.Rows[i]["balqnty"].retDbl() - totalgridqnty.retDbl();
                        }
                    }
                    var tbl_data = (from DataRow dr in tbl.Rows where dr["balqnty"].retDbl() != 0 select dr);
                    if (tbl_data != null && tbl_data.Count() > 0)
                    {
                        tbl = tbl_data.CopyToDataTable();
                    }
                    else
                    {
                        tbl = tbl.Clone();
                    }
                    //string[] progautoslno = VE.TPROGDTL.Select(x => x.PROGAUTOSLNO).ToArray();
                    //var tbl_data = tbl.AsEnumerable().Where(r => !progautoslno.Any(b => r.Field<string>("PROGAUTOSLNO") == b));
                    //if (tbl_data != null && tbl_data.Count() > 0)
                    //{
                    //    tbl = tbl_data.CopyToDataTable();
                    //}
                    //else
                    //{
                    //    tbl = tbl.Clone();
                    //}
                }
                if (val.retStr() != "")
                {
                    string valsrch = val.ToUpper().Trim();
                    string str = "(proguniqno = '" + valsrch + "' or barno = '" + valsrch + "' or styleno like('%" + valsrch + "%')) ";
                    if (Code.retStr() != "") { str += "and progautoslno = '" + Code + "'"; }
                    var filterdata = tbl.Select(str);
                    if (filterdata != null && filterdata.Count() > 0)
                    {
                        tbl = filterdata.CopyToDataTable();
                    }
                    else
                    {
                        tbl = tbl.Clone();
                    }
                }

                if (val.retStr() == "" || tbl.Rows.Count > 1)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= tbl.Rows.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + tbl.Rows[i]["docno"] + "</td><td>" + tbl.Rows[i]["docdt"].retStr().Remove(10) + " </td><td>" + tbl.Rows[i]["proguniqno"] + " </td><td>"
                            + tbl.Rows[i]["barno"] + " </td><td>" + tbl.Rows[i]["itgrpnm"] + " [" + tbl.Rows[i]["itgrpcd"] + "]" + " </td><td>" + tbl.Rows[i]["itnm"] + " [" + tbl.Rows[i]["itcd"] + "]" + " </td><td>"
                            + tbl.Rows[i]["styleno"] + " </td><td>" + tbl.Rows[i]["balnos"] + " </td><td>" + tbl.Rows[i]["balqnty"] + " </td><td>"
                            + tbl.Rows[i]["progautoslno"] + " </td><td>" + tbl.Rows[i]["itremark"] + " </td></tr>");
                    }
                    var hdr = "Doc No." + Cn.GCS() + "Doc Dt." + Cn.GCS() + "Uniq. No." + Cn.GCS() + "Bar Code No" + Cn.GCS()
                        + "Item Group" + Cn.GCS() + "Item" + Cn.GCS() + "Style No" + Cn.GCS() + "bal. Nos." + Cn.GCS() + "bal. Qnty."
                        + Cn.GCS() + "progautoslno" + Cn.GCS() + "Item Remarks";
                    string str = masterHelp.Generate_help(hdr, SB.ToString(), "9");
                    return PartialView("_Help2", str);
                }
                else
                {
                    if (tbl != null && tbl.Rows.Count > 0)
                    {

                        var data = (from DataRow dr in tbl.Rows
                                    select new TPROGDTL
                                    {
                                        PROGAUTONO = dr["PROGAUTONO"].retStr(),
                                        PROGSLNO = dr["PROGSLNO"].retShort(),
                                        PROGAUTOSLNO = dr["PROGAUTOSLNO"].retStr(),
                                        PROGUNIQNO = dr["PROGUNIQNO"].retStr(),
                                        BARNO = dr["BARNO"].retStr(),
                                        ITGRPNM = dr["ITGRPNM"].retStr(),
                                        ITGRPCD = dr["ITGRPCD"].retStr(),
                                        ITNM = dr["FABITNM"].retStr() + " " + dr["ITNM"].retStr(),
                                        ITCD = dr["ITCD"].retStr(),
                                        //FABITCD = dr["FABITCD"].retStr(),
                                        STYLENO = dr["STYLENO"].retStr(),
                                        UOM = dr["UOMCD"].retStr(),
                                        COLRNM = dr["COLRNM"].retStr(),
                                        COLRCD = dr["COLRCD"].retStr(),
                                        SIZECD = dr["SIZECD"].retStr(),
                                        SHADE = dr["SHADE"].retStr(),
                                        NOS = dr["BALNOS"].retDbl(),
                                        QNTY = dr["BALQNTY"].retDbl(),
                                        //BALNOS = dr["BALNOS"].retDbl(),
                                        //BALQNTY = dr["BALQNTY"].retDbl(),
                                        ITREMARK = dr["ITREMARK"].retStr(),
                                        SAMPLE = dr["sample"].retStr(),
                                        COMMONUNIQBAR = dr["COMMONUNIQBAR"].retStr(),
                                    }).ToList();
                        if (VE.TPROGDTL != null)
                        {
                            VE.TPROGDTL.AddRange(data);
                        }
                        else
                        {
                            VE.TPROGDTL = data;
                        }
                        int slno = 1;
                        for (int p = 0; p <= VE.TPROGDTL.Count - 1; p++)
                        {
                            VE.TPROGDTL[p].SLNO = slno.retShort();
                            VE.TPROGDTL[p].CheckedSample = VE.TPROGDTL[p].SAMPLE.retStr() == "Y" ? true : false;

                            string progautoslno = VE.TPROGDTL[p].PROGAUTOSLNO;
                            var balnosqnty = (from DataRow dr in temptbl.Rows
                                              where dr["PROGAUTOSLNO"].retStr() == progautoslno
                                              select new
                                              {
                                                  balnos = dr["balnos"].retDbl(),
                                                  balqnty = dr["balqnty"].retDbl(),
                                              }).FirstOrDefault();
                            VE.TPROGDTL[p].BALNOS = balnosqnty.balnos.retDbl();
                            VE.TPROGDTL[p].BALQNTY = balnosqnty.balqnty.retDbl();
                            slno++;


                        }
                        VE.P_T_NOS = VE.TPROGDTL.Sum(a => a.NOS).retDbl();
                        VE.P_T_QNTY = VE.TPROGDTL.Sum(a => a.QNTY).retDbl();
                        ModelState.Clear();
                        VE.DefaultView = true;
                        return PartialView("_T_OUTRECPROCESS_Programme", VE);
                    }
                    else
                    {
                        return Content("Invalid Bar Code No ! Please Enter a Valid Bar Code No !!");
                    }
                }


            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult FillReceiveData(TransactionOutRecProcess VE)
        {
            try
            {
                Cn.getQueryString(VE);
                var tempdata = (DataTable)TempData["PENDPROGRAMME" + VE.MENU_PARA]; TempData.Keep();
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                VE.TBATCHDTL = (from a in VE.TPROGDTL
                                    //join b in tempdata.AsEnumerable() on a.PROGAUTOSLNO equals b.Field<string>("PROGAUTOSLNO").retStr() into Z
                                join b in DB.M_GROUP on a.ITGRPCD equals b.ITGRPCD into Z
                                from b in Z.DefaultIfEmpty()
                                select new TBATCHDTL
                                {
                                    BARNO = a.BARNO,
                                    RECPROGAUTONO = a.PROGAUTONO.retStr(),
                                    RECPROGSLNO = a.PROGSLNO.retShort(),
                                    ITGRPNM = a.ITGRPNM.retStr(),
                                    ITGRPCD = a.ITGRPCD.retStr(),
                                    ITCD = a.ITCD.retStr(),
                                    ITSTYLE = a.STYLENO.retStr() + " " + a.ITNM.retStr(),
                                    COLRCD = a.COLRCD.retStr(),
                                    COLRNM = a.COLRNM.retStr(),
                                    SIZECD = a.SIZECD.retStr(),
                                    SIZENM = a.SIZENM.retStr(),
                                    SHADE = a.SHADE.retStr(),
                                    UOM = a.UOM.retStr(),
                                    NOS = a.NOS.retDbl(),
                                    QNTY = a.QNTY.retDbl(),
                                    BARGENTYPE = b.BARGENTYPE.retStr(),
                                    //RATE = dr["RATE"].retDbl(),
                                    //DISCTYPE_DESC = dr["DISCTYPE_DESC"].retStr(),
                                    //DISCTYPE = dr["DISCTYPE"].retStr(),
                                    //DISCRATE = dr["DISCRATE"].retDbl(),
                                    //BarImages = dr["BarImages"].retStr()
                                    SAMPLE = a.SAMPLE.retStr(),
                                    COMMONUNIQBAR = a.COMMONUNIQBAR.retStr(),
                                }).ToList();

                //string[] progautoslno = VE.TPROGDTL.Select(x => x.PROGAUTOSLNO).ToArray();

                //VE.TBATCHDTL = (from DataRow dr in tempdata.Rows
                //                where progautoslno.Contains(dr["PROGAUTOSLNO"].retStr())
                //                select new TBATCHDTL
                //                {
                //                    BARNO = dr["BARNO"].retStr(),
                //                    RECPROGAUTONO = dr["PROGAUTONO"].retStr(),
                //                    RECPROGSLNO = dr["PROGSLNO"].retShort(),
                //                    ITGRPNM = dr["ITGRPNM"].retStr(),
                //                    ITGRPCD = dr["ITGRPCD"].retStr(),
                //                    ITCD = dr["ITCD"].retStr(),
                //                    ITSTYLE = dr["STYLENO"].retStr() + " " + dr["ITNM"].retStr(),
                //                    COLRCD = dr["COLRCD"].retStr(),
                //                    COLRNM = dr["COLRNM"].retStr(),
                //                    SIZECD = dr["SIZECD"].retStr(),
                //                    SIZENM = dr["SIZENM"].retStr(),
                //                    SHADE = dr["SHADE"].retStr(),
                //                    UOM = dr["UOMCD"].retStr(),
                //                    NOS = dr["BALNOS"].retDbl(),
                //                    QNTY = dr["BALQNTY"].retDbl(),
                //                    BARGENTYPE = dr["BARGENTYPE"].retStr(),
                //                    //RATE = dr["RATE"].retDbl(),
                //                    //DISCTYPE_DESC = dr["DISCTYPE_DESC"].retStr(),
                //                    //DISCTYPE = dr["DISCTYPE"].retStr(),
                //                    //DISCRATE = dr["DISCRATE"].retDbl(),
                //                    //BarImages = dr["BarImages"].retStr()
                //                    SAMPLE = dr["sample"].retStr(),
                //                }).ToList();

                string barno = VE.TPROGDTL.Select(x => x.BARNO).Distinct().ToArray().retSqlfromStrarray();
                string TAXGRPCD = VE.T_TXNOTH.TAXGRPCD.retStr();
                string GOCD = VE.T_TXN.GOCD.retStr() == "" ? "" : VE.T_TXN.GOCD.retStr().retSqlformat();
                string PRCCD = VE.T_TXNOTH.PRCCD.retStr();
                string MTRLJOBCD = VE.MTRLJOBCD.retStr() == "" ? "" : VE.MTRLJOBCD.retStr().retSqlformat();
                var barimgdata = salesfunc.GetBarHelp(VE.T_TXN.DOCDT.retStr().Remove(10), GOCD, barno, "", MTRLJOBCD, "", "", "", PRCCD, TAXGRPCD);

                string progautoslno = VE.TBATCHDTL.Select(a => a.RECPROGAUTONO + a.RECPROGSLNO).Distinct().ToArray().retSqlfromStrarray();
                string scm = CommVar.CurSchema(UNQSNO);
                string sql = "";
                sql += "select a.progautono, a.progslno, c.qnty, sum(a.qnty * a.rate) issamt ";
                sql += "  from " + scm + ".t_kardtl a, " + scm + ".t_cntrl_hdr b, " + scm + ".t_progmast c ";
                sql += "where a.autono = b.autono(+) and nvl(b.cancel, 'N')= 'N' and ";
                sql += "a.progautono = c.autono(+) and a.progslno = c.slno(+) and ";
                sql += "a.progautono||a.progslno in (" + progautoslno + ") ";
                sql += "group by a.progautono, a.progslno, c.qnty ";
                DataTable progdt = masterHelp.SQLquery(sql);

                for (int p = 0; p <= VE.TBATCHDTL.Count - 1; p++)
                {
                    var img = barimgdata.AsEnumerable().Where(a => a.Field<string>("barno").retStr() == VE.TBATCHDTL[p].BARNO).Select(b => b.Field<string>("barimage")).FirstOrDefault();
                    VE.TBATCHDTL[p].BarImages = img;
                    VE.TBATCHDTL[p].SLNO = Convert.ToInt16(p + 1);
                    VE.TBATCHDTL[p].TXNSLNO = Convert.ToInt16(p + 1);
                    //VE.TBATCHDTL[p].DISC_TYPE = masterHelp.DISC_TYPE();
                    if (VE.TBATCHDTL[p].QNTY.retDbl() != 0 && VE.TBATCHDTL[p].SAMPLE.retStr() != "Y")
                    {
                        if (progdt != null && progdt.Rows.Count > 0)
                        {
                            string progautono = VE.TBATCHDTL[p].RECPROGAUTONO;
                            short progslno = VE.TBATCHDTL[p].RECPROGSLNO.retShort();
                            var progtotalamt = progdt.AsEnumerable().Where(a => a.Field<string>("progautono") == progautono && a.Field<short>("progslno") == progslno).Select(a => a.Field<decimal>("issamt").retDbl()).Sum();
                            var progtotalqnty = progdt.AsEnumerable().Where(a => a.Field<string>("progautono") == progautono && a.Field<short>("progslno") == progslno).Select(a => a.Field<double>("qnty")).Sum();
                            double prograte = 0;
                            if (progtotalqnty.retDbl() != 0)
                            {
                                prograte = progtotalamt / progtotalqnty;
                            }
                            VE.TBATCHDTL[p].MTRLCOST = (prograte.retDbl() * VE.TBATCHDTL[p].QNTY.retDbl()).toRound(2);
                        }
                    }
                }
                VE.B_T_NOS = VE.TBATCHDTL.Sum(a => a.NOS).retDbl();
                VE.B_T_QNTY = VE.TBATCHDTL.Sum(a => a.QNTY).retDbl();
                VE.DISC_TYPE = masterHelp.DISC_TYPE();
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_OUTRECPROCESS_Receive", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult FillDetailData(TransactionOutRecProcess VE)
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
                                  x.ITCD,
                                  x.ITSTYLE,
                                  x.DISCTYPE,
                                  x.UOM,
                                  x.RATE,
                                  x.DISCRATE
                              } into P
                              select new TTXNDTL
                              {
                                  SLNO = P.Key.TXNSLNO.retShort(),
                                  ITGRPCD = P.Key.ITGRPCD,
                                  ITGRPNM = P.Key.ITGRPNM,
                                  ITCD = P.Key.ITCD,
                                  ITSTYLE = P.Key.ITSTYLE,
                                  UOM = P.Key.UOM,
                                  NOS = P.Sum(A => A.NOS),
                                  QNTY = P.Sum(A => A.QNTY),
                                  BLQNTY = P.Sum(A => A.BLQNTY),
                                  RATE = P.Key.RATE,
                                  DISCTYPE = P.Key.DISCTYPE,
                                  DISCRATE = P.Key.DISCRATE
                              }).ToList();

                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                string PRODGRPCD = DB.M_JOBMST.Where(a => a.JOBCD == VE.T_TXN.JOBCD).Select(b => b.PRODGRPCD).FirstOrDefault();
                string sql = "";
                sql += "select a.effdt, a.prodgrpcd, a.igstper, a.cgstper, a.sgstper, a.cessper from ";
                sql += "(select a.effdt, a.prodgrpcd, a.igstper, a.cgstper, a.sgstper, a.cessper, ";
                sql += "row_number() over (partition by a.prodgrpcd order by a.effdt desc) as rn ";
                sql += "from " + CommVar.CurSchema(UNQSNO) + ".m_prodtax a ";
                sql += "where a.taxgrpcd='" + VE.T_TXNOTH.TAXGRPCD + "' and a.prodgrpcd='" + PRODGRPCD.retStr() + "' and ";
                sql += "a.effdt <= to_date('" + VE.T_TXN.DOCDT.retStr().Remove(10) + "','dd/mm/yyyy') ) a where a.rn=1 ";
                DataTable Dt = masterHelp.SQLquery(sql);
                if (Dt.Rows.Count > 0)
                {
                    for (int p = 0; p <= VE.TTXNDTL.Count - 1; p++)
                    {
                        VE.TTXNDTL[p].IGSTPER = Dt.Rows[0]["igstper"].retDbl();
                        VE.TTXNDTL[p].CGSTPER = Dt.Rows[0]["cgstper"].retDbl();
                        VE.TTXNDTL[p].SGSTPER = Dt.Rows[0]["sgstper"].retDbl();
                        VE.TTXNDTL[p].CESSPER = Dt.Rows[0]["cessper"].retDbl();
                        //VE.TTXNDTL[p].DISC_TYPE = masterHelp.DISC_TYPE();
                    }
                }

                VE.DISC_TYPE = masterHelp.DISC_TYPE();
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_OUTRECPROCESS_Detail", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        private void FreightCharges(TransactionOutRecProcess VE, string AUTO_NO)
        {
            try
            {
                double A_T_CURR_AMT = 0; double A_T_AMT = 0; double A_T_TAXABLE = 0; double A_T_IGST_AMT = 0; double A_T_CGST_AMT = 0;
                double A_T_SGST_AMT = 0; double A_T_CESS_AMT = 0; double A_T_DUTY_AMT = 0; double A_T_NET_AMT = 0; double IGST_PER = 0; double CGST_PER = 0; double SGST_PER = 0;
                double CESS_PER = 0; double DUTY_PER = 0;
                string sql = "", scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
                Cn.getQueryString(VE);
                string S_P = "";
                if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "PR" || VE.MENU_PARA == "OP") { S_P = "P"; } else { S_P = "S"; }

                sql = "";
                sql += "select a.amtcd, b.amtnm, b.calccode, b.addless, b.taxcode, b.calctype, b.calcformula, ";
                sql += "a.amtdesc, b.glcd, b.hsncode, a.amtrate, a.curr_amt, a.amt,a.igstper, a.igstamt, ";
                sql += "a.sgstper, a.sgstamt, a.cgstper, a.cgstamt,a.cessper, a.cessamt, a.dutyper, a.dutyamt ";
                sql += "from " + scm + ".t_txnamt a, " + scm + ".m_amttype b ";
                sql += "where a.amtcd=b.amtcd(+) and b.salpur='" + S_P + "' and a.autono='" + AUTO_NO + "' ";
                sql += "union ";
                sql += "select b.amtcd, b.amtnm, b.calccode, b.addless, b.taxcode, b.calctype, b.calcformula, ";
                sql += "'' amtdesc, b.glcd, b.hsncode, 0 amtrate, 0 curr_amt, 0 amt,0 igstper, 0 igstamt, ";
                sql += "0 sgstper, 0 sgstamt, 0 cgstper, 0 cgstamt, 0 cessper, 0 cessamt, 0 dutyper, 0 dutyamt ";
                sql += "from " + scm + ".m_amttype b, " + scm + ".m_cntrl_hdr c ";
                sql += "where b.m_autono=c.m_autono(+) and b.salpur='" + S_P + "'  and nvl(c.inactive_tag,'N') = 'N' ";
                sql += "and b.amtcd not in (select amtcd from " + scm + ".t_txnamt where autono='" + AUTO_NO + "')";
                var AMOUNT_DATA = masterHelp.SQLquery(sql);


                VE.TTXNAMT = (from DataRow dr in AMOUNT_DATA.Rows
                              select new TTXNAMT()
                              {
                                  AMTCD = dr["amtcd"].ToString(),
                                  ADDLESS = dr["addless"].ToString(),
                                  TAXCODE = dr["taxcode"].ToString(),
                                  GLCD = dr["GLCD"].ToString(),
                                  AMTNM = dr["amtnm"].ToString(),
                                  CALCCODE = dr["calccode"].ToString(),
                                  CALCTYPE = dr["CALCTYPE"].ToString(),
                                  CALCFORMULA = dr["calcformula"].ToString(),
                                  AMTDESC = dr["amtdesc"].ToString(),
                                  HSNCODE = dr["hsncode"].ToString(),
                                  AMTRATE = dr["amtrate"].retDbl(),
                                  CURR_AMT = dr["curr_amt"].retDbl(),
                                  AMT = dr["amt"].retDbl(),
                                  IGSTPER = dr["igstper"].retDbl(),
                                  CGSTPER = dr["cgstper"].retDbl(),
                                  SGSTPER = dr["sgstper"].retDbl(),
                                  CESSPER = dr["cessper"].retDbl(),
                                  DUTYPER = dr["dutyper"].retDbl(),
                                  IGSTAMT = dr["igstamt"].retDbl(),
                                  CGSTAMT = dr["cgstamt"].retDbl(),
                                  SGSTAMT = dr["sgstamt"].retDbl(),
                                  CESSAMT = dr["cessamt"].retDbl(),
                                  DUTYAMT = dr["dutyamt"].retDbl(),
                              }).ToList();

                for (int p = 0; p <= VE.TTXNAMT.Count - 1; p++)
                {
                    VE.TTXNAMT[p].SLNO = Convert.ToInt16(p + 1);

                    VE.TTXNAMT[p].NETAMT = VE.TTXNAMT[p].AMT.Value + VE.TTXNAMT[p].IGSTAMT.Value + VE.TTXNAMT[p].CGSTAMT.Value + VE.TTXNAMT[p].SGSTAMT.Value + VE.TTXNAMT[p].CESSAMT.Value + VE.TTXNAMT[p].DUTYAMT.Value;
                    VE.TTXNAMT[p].IGSTPER = IGST_PER;
                    VE.TTXNAMT[p].CGSTPER = CGST_PER;
                    VE.TTXNAMT[p].SGSTPER = SGST_PER;
                    VE.TTXNAMT[p].CESSPER = CESS_PER;
                    A_T_CURR_AMT = A_T_CURR_AMT + VE.TTXNAMT[p].CURR_AMT.Value;
                    A_T_AMT = A_T_AMT + VE.TTXNAMT[p].AMT.Value;
                    A_T_IGST_AMT = A_T_IGST_AMT + VE.TTXNAMT[p].IGSTAMT.Value;
                    A_T_CGST_AMT = A_T_CGST_AMT + VE.TTXNAMT[p].CGSTAMT.Value;
                    A_T_SGST_AMT = A_T_SGST_AMT + VE.TTXNAMT[p].SGSTAMT.Value;
                    A_T_CESS_AMT = A_T_CESS_AMT + VE.TTXNAMT[p].CESSAMT.Value;
                    A_T_DUTY_AMT = A_T_DUTY_AMT + VE.TTXNAMT[p].DUTYAMT.Value;
                    A_T_NET_AMT = A_T_NET_AMT + A_T_AMT + A_T_IGST_AMT + A_T_CGST_AMT + A_T_SGST_AMT + A_T_CESS_AMT + A_T_DUTY_AMT;
                }
                VE.A_T_CURR = A_T_CURR_AMT;
                VE.A_T_AMOUNT = A_T_AMT;
                VE.A_T_IGST = A_T_IGST_AMT;
                VE.A_T_CGST = A_T_CGST_AMT;
                VE.A_T_SGST = A_T_SGST_AMT;
                VE.A_T_CESS = A_T_CESS_AMT;
                VE.A_T_DUTY = A_T_DUTY_AMT;
                VE.A_T_NET = A_T_NET_AMT;
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
            }
        }
        private string TranBarcodeGenerate(string doccd, string lbatchini, string docbarcode, string UNIQNO, int slno)
        {//YRCODE	2,lbatchini	2,TXN UNIQ NO	7,SLNO	4
            var yrcd = CommVar.YearCode(UNQSNO).Substring(2, 2);
            return yrcd + lbatchini + UNIQNO + slno.ToString().PadLeft(4, '0');
        }
        private string CommonBarcodeGenerate(string itgrpcd, string itcd, string MTBARCODE, string PRTBARCODE, string CLRBARCODE, string SZBARCODE)
        {
            //itgrpcd last 3  3
            //itcd last 7  7
            //mtrljobcd mtrlbarcode 1
            //partcode prtbarcode  1
            //color clrbarcode  4
            //size szbarcode   3
            return itgrpcd.retStr().Substring(1, 3) + itcd.retStr().Substring(1, 7) + MTBARCODE.retStr() + PRTBARCODE.retStr() + CLRBARCODE.retStr() + SZBARCODE.retStr();
        }
        public ActionResult UploadImages(string ImageStr, string ImageName, string ImageDesc)
        {
            try
            {
                var extension = Path.GetExtension(ImageName);
                string filename = "I".retRepname() + extension;
                var link = Cn.SaveImage(ImageStr, "/UploadDocuments/" + filename);
                return Content("/UploadDocuments/" + filename);
            }
            catch (Exception ex)
            {
                return Content("//.");
            }

        }
        public Tuple<List<T_BATCH_IMG_HDR>> SaveBarImage(string BarImage, string BARNO, short EMD)
        {
            List<T_BATCH_IMG_HDR> doc = new List<T_BATCH_IMG_HDR>();
            int slno = 0;
            try
            {
                var BarImages = BarImage.retStr().Trim(Convert.ToChar(Cn.GCS())).Split(Convert.ToChar(Cn.GCS()));
                foreach (string image in BarImages)
                {
                    if (image != "")
                    {
                        var imagedes = image.Split('~');
                        T_BATCH_IMG_HDR mdoc = new T_BATCH_IMG_HDR();
                        mdoc.CLCD = CommVar.ClientCode(UNQSNO);
                        mdoc.EMD_NO = EMD;
                        mdoc.SLNO = Convert.ToByte(++slno);
                        mdoc.DOC_CTG = "PRODUCT";
                        var extension = Path.GetExtension(imagedes[0]);
                        mdoc.DOC_FLNAME = BARNO + "_" + slno + extension;
                        mdoc.DOC_DESC = imagedes[1].retStr().Replace('~', ' ');
                        mdoc.BARNO = BARNO;
                        mdoc.DOC_EXTN = extension;
                        doc.Add(mdoc);
                        string topath = CommVar.SaveFolderPath() + "/ItemImages/" + mdoc.DOC_FLNAME;
                        topath = Path.Combine(topath, "");
                        string frompath = System.Web.Hosting.HostingEnvironment.MapPath("/UploadDocuments/" + imagedes[0]);
                        Cn.CopyImage(frompath, topath);
                    }
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, BarImage);
            }
            var result = Tuple.Create(doc);
            return result;
        }
        public ActionResult DeleteRow(TransactionOutRecProcess VE)
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
                        item.CheckedSample = VE.TPROGDTL[i].SAMPLE.retStr() == "Y" ? true : false;
                        ITEMSIZE.Add(item);
                    }

                }
                VE.TPROGDTL = ITEMSIZE;
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_OUTRECPROCESS_Programme", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult AddDOCRow(TransactionOutRecProcess VE)
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
        public ActionResult DeleteDOCRow(TransactionOutRecProcess VE)
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
        public ActionResult cancelRecords(TransactionOutRecProcess VE, string par1)
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
        public ActionResult ParkRecord(FormCollection FC, TransactionOutRecProcess stream, string MNUDET, string UNQSNO)
        {
            try
            {
                if (stream.T_TXN.DOCCD.retStr() != "")
                {
                    stream.T_CNTRL_HDR.DOCCD = stream.T_TXN.DOCCD.retStr();
                }
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
                string url = masterHelp.RetriveParkFromFile(value, Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString(), "Improvar.ViewModels.TransactionOutRecProcess");
                return url;
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return ex.Message;
            }
        }
        public ActionResult SAVE(FormCollection FC, TransactionOutRecProcess VE)
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
            string dberrmsg = "";

            OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted);
            OraCmd.Transaction = OraTrans;
            DB.Configuration.ValidateOnSaveEnabled = false;
            try
            {
                //DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".T_CNTRL_HDR in  row share mode");
                OraCmd.CommandText = "lock table " + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode"; OraCmd.ExecuteNonQuery();

                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                string ContentFlg = "";
                if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                {
                    //checking barcode & txndtl pge itcd wise qnty, nos should match

                    var barcodedata = (from x in VE.TBATCHDTL
                                       group x by new { x.ITCD } into P
                                       select new
                                       {
                                           ITCD = P.Key.ITCD,
                                           QTY = P.Sum(A => A.QNTY),
                                           NOS = P.Sum(A => A.NOS)
                                       }).Where(a => a.QTY != 0).ToList();
                    var txndtldata = (from x in VE.TTXNDTL
                                      group x by new { x.ITCD } into P
                                      select new
                                      {
                                          ITCD = P.Key.ITCD,
                                          QTY = P.Sum(A => A.QNTY),
                                          NOS = P.Sum(A => A.NOS)
                                      }).Where(a => a.QTY != 0).ToList();

                    var difList = barcodedata.Where(a => !txndtldata.Any(a1 => a1.ITCD == a.ITCD && a1.QTY == a.QTY && a1.NOS == a.NOS))
            .Union(txndtldata.Where(a => !barcodedata.Any(a1 => a1.ITCD == a.ITCD && a1.QTY == a.QTY && a1.NOS == a.NOS))).ToList();
                    if (difList != null && difList.Count > 0)
                    {
                        OraTrans.Rollback();
                        OraCon.Dispose();
                        return Content("Barcode grid & Detail grid itcd wise qnty, nos should match !!");
                    }

                    T_TXN TTXN = new T_TXN();
                    T_TXNTRANS TXNTRANS = new T_TXNTRANS();
                    T_TXNOTH TTXNOTH = new T_TXNOTH();

                    string DOCPATTERN = "";
                    string docpassrem = "";
                    bool blactpost = true, blgstpost = true;
                    TTXN.DOCDT = VE.T_TXN.DOCDT;
                    string Ddate = Convert.ToString(TTXN.DOCDT);
                    TTXN.CLCD = CommVar.ClientCode(UNQSNO);
                    string auto_no = ""; string Month = "";

                    #region Posting to finance Preparation
                    int COUNTER = 0;
                    string expglcd = "";
                    string stkdrcr = "C", strqty = " Q=" + VE.TOTQNTY.toRound(2).ToString();
                    string trcd = "TR";
                    string revcharge = "";
                    string dr = ""; string cr = ""; int isl = 0; string strrem = "";
                    double igst = 0; double cgst = 0; double sgst = 0; double cess = 0; double duty = 0; double dbqty = 0; double dbamt = 0; double dbcurramt = 0;
                    double dbDrAmt = 0, dbCrAmt = 0;
                    blactpost = true; blgstpost = true;
                    string parglcd = "saldebglcd", parclass1cd = "";
                    string strblno = "", strbldt = "", strduedt = "", strrefno = "", strvtype = "BL";
                    dr = "D"; cr = "C";
                    string sslcd = TTXN.SLCD;
                    //if (VE.PSLCD.retStr() != "") sslcd = VE.PSLCD.ToString();

                    switch (VE.MENU_PARA)
                    {
                        case "DY":
                            stkdrcr = "D"; blactpost = false; blgstpost = false; break;
                        case "PR":
                            stkdrcr = "D"; trcd = "SB"; strrem = "Sale" + strqty; break;
                        case "ST":
                            stkdrcr = "D"; trcd = "SB"; strrem = "Sale" + strqty; break;
                        case "EM":
                            stkdrcr = "D"; dr = "C"; cr = "D"; trcd = "SC"; strrem = "SRM agst " + VE.T_TXN.PREFNO + " dtd. " + VE.T_TXN.PREFDT.ToString().retDateStr() + strqty; break;
                        case "JW":
                            stkdrcr = "D"; trcd = "SB"; strrem = "Cash Sale" + strqty; break;
                        case "DYU":
                            stkdrcr = "D"; dr = "C"; cr = "D"; trcd = "SC"; strrem = "Cash Sale Return" + strqty; break;
                        case "PRU":
                            stkdrcr = "D"; trcd = "SB"; strrem = "Sale Export" + strqty; break;
                        case "STU":
                            stkdrcr = "D"; blactpost = false; blgstpost = true; break;
                        case "EMU":
                            stkdrcr = "D"; parglcd = "purdebglcd"; dr = "C"; cr = "D"; trcd = "PB"; strrem = "Purchase Blno " + VE.T_TXN.PREFNO + " dtd. " + VE.T_TXN.PREFDT.ToString().retDateStr() + strqty; break;
                        case "JWU":
                            stkdrcr = "D"; parglcd = "purdebglcd"; trcd = "PD"; strrem = "PRM agst " + VE.T_TXN.PREFNO + " dtd. " + VE.T_TXN.PREFDT.ToString().retDateStr() + strqty; break;
                    }

                    string slcdlink = "", slcdpara = VE.MENU_PARA;
                    if (VE.MENU_PARA == "PR") slcdpara = "PB";
                    string sql = "";
                    sql = "select class1cd, " + parglcd + " glcd from " + CommVar.CurSchema(UNQSNO) + ".m_syscnfg ";
                    DataTable tblsys = masterHelp.SQLquery(sql);
                    if (tblsys.Rows.Count == 0)
                    {
                        dberrmsg = "Debtor/Creditor code not setup"; goto dbnotsave;
                    }

                    sql = "select b.rogl, b.tcsgl, a.class1cd, null class2cd, nvl(c.crlimit,0) crlimit, nvl(c.crdays,0) crdays, ";
                    sql += "'" + VE.TTXNDTL[0].GLCD.retStr() + "' prodglcd, ";
                    if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "PR") sql += "b.igst_p igstcd, b.cgst_p cgstcd, b.sgst_p sgstcd, b.cess_p cesscd, b.duty_p dutycd, ";
                    else sql += "b.igst_s igstcd, b.cgst_s cgstcd, b.sgst_s sgstcd, b.cess_s cesscd, b.duty_s dutycd, ";
                    if (slcdpara == "PB") sql += "a.purdebglcd parglcd, "; else sql += "a.saldebglcd parglcd, ";
                    sql += "igst_rvi, cgst_rvi, sgst_rvi, cess_rvi, igst_rvo, cgst_rvo, sgst_rvo, cess_rvo ";
                    sql += "from " + scm1 + ".m_syscnfg a, " + scmf + ".m_post b, " + scm1 + ".m_subleg_com c ";
                    sql += "where c.slcd in('" + VE.T_TXN.SLCD + "',null) and ";
                    sql += "c.compcd in ('" + COM + "',null) ";
                    DataTable tbl = masterHelp.SQLquery(sql);

                    parglcd = tbl.Rows[0]["parglcd"].retStr(); parclass1cd = tbl.Rows[0]["class1cd"].retStr();

                    //Calculate Others Amount from amount tab to distrubute into txndtl table
                    double _amtdist = 0, _baldist = 0, _rpldist = 0, _mult = 1;
                    double _amtdistq = 0, _baldistq = 0, _rpldistq = 0;
                    double titamt = 0, titqty = 0;
                    int lastitemno = 0;
                    if (VE.TTXNAMT != null)
                    {
                        for (int i = 0; i <= VE.TTXNAMT.Count - 1; i++)
                        {
                            if (VE.TTXNAMT[i].SLNO != 0 && VE.TTXNAMT[i].AMTCD != null && VE.TTXNAMT[i].AMT != 0)
                            {
                                if (VE.TTXNAMT[i].ADDLESS == "L") _mult = -1; else _mult = 1;
                                if (VE.TTXNAMT[i].TAXCODE == "TC") _amtdistq = _amtdistq + (Convert.ToDouble(VE.TTXNAMT[i].AMT) * _mult);
                                else _amtdist = _amtdist + (Convert.ToDouble(VE.TTXNAMT[i].AMT) * _mult);
                            }
                        }
                    }
                    for (int i = 0; i <= VE.TTXNDTL.Count - 1; i++)
                    {
                        if (VE.TTXNDTL[i].SLNO != 0 && VE.TTXNDTL[i].ITCD != null)
                        {
                            titamt = titamt + VE.TTXNDTL[i].AMT.retDbl();
                            titqty = titqty + Convert.ToDouble(VE.TTXNDTL[i].QNTY);
                            lastitemno = i;
                        }
                    }
                    //
                    _baldist = _amtdist; _baldistq = _amtdistq;
                    #endregion
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
                        //TCH = Cn.T_CONTROL_HDR(TTXN.DOCCD, TTXN.DOCDT, TTXN.DOCNO, TTXN.AUTONO, Month, DOCPATTERN, VE.DefaultAction, scm1, null, TTXN.SLCD, TTXN.BLAMT.Value, null);
                    }
                    else
                    {
                        TTXN.DOCCD = VE.T_TXN.DOCCD;
                        TTXN.DOCNO = VE.T_TXN.DOCNO;
                        TTXN.AUTONO = VE.T_TXN.AUTONO;
                        Month = VE.T_CNTRL_HDR.MNTHCD;
                        TTXN.EMD_NO = Convert.ToInt16((VE.T_CNTRL_HDR.EMD_NO == null ? 0 : VE.T_CNTRL_HDR.EMD_NO) + 1);
                        DOCPATTERN = VE.T_CNTRL_HDR.DOCNO;
                        TTXN.DTAG = "E";
                    }
                    //TTXN.DOCTAG = VE.MENU_PARA.retStr().Length > 2 ? VE.MENU_PARA.retStr().Remove(2) : VE.MENU_PARA.retStr();
                    TTXN.DOCTAG = (VE.MENU_PARA == "DY" || VE.MENU_PARA == "PR" || VE.MENU_PARA == "ST" || VE.MENU_PARA == "EM" || VE.MENU_PARA == "JW") ? "JR" : "JU";
                    TTXN.SLCD = VE.T_TXN.SLCD;
                    //TTXN.CONSLCD = VE.T_TXN.CONSLCD;
                    //TTXN.CURR_CD = VE.T_TXN.CURR_CD;
                    //TTXN.CURRRT = VE.T_TXN.CURRRT;
                    TTXN.BLAMT = VE.T_TXN.BLAMT;
                    TTXN.PREFDT = VE.T_TXN.PREFDT;
                    TTXN.PREFNO = VE.T_TXN.PREFNO;
                    TTXN.REVCHRG = VE.T_TXN.REVCHRG;
                    TTXN.ROAMT = VE.T_TXN.ROAMT;
                    if (VE.RoundOff == true) { TTXN.ROYN = "Y"; } else { TTXN.ROYN = "N"; }
                    TTXN.GOCD = VE.T_TXN.GOCD;
                    TTXN.JOBCD = VE.T_TXN.JOBCD;
                    //TTXN.MANSLIPNO = VE.T_TXN.MANSLIPNO;
                    TTXN.DUEDAYS = VE.T_TXN.DUEDAYS;
                    TTXN.PARGLCD = parglcd; // VE.T_TXN.PARGLCD;
                    //TTXN.GLCD = VE.T_TXN.GLCD;
                    TTXN.CLASS1CD = parclass1cd; // VE.T_TXN.CLASS1CD;
                    TTXN.CLASS2CD = VE.T_TXN.CLASS2CD;
                    //TTXN.LINECD = VE.T_TXN.LINECD;
                    TTXN.BARGENTYPE = VE.T_TXN.BARGENTYPE;
                    //TTXN.WPPER = VE.T_TXN.WPPER;
                    //TTXN.RPPER = VE.T_TXN.RPPER;
                    TTXN.MENU_PARA = VE.T_TXN.MENU_PARA;
                    //TTXN.TCSPER = VE.T_TXN.TCSPER;
                    //TTXN.TCSAMT = VE.T_TXN.TCSAMT;
                    //TTXN.TCSON = VE.T_TXN.TCSON;
                    //TTXN.TDSCODE = VE.T_TXN.TDSCODE;
                    TTXN.ADVADJ = VE.T_TXN.ADVADJ;
                    if (VE.DefaultAction == "E")
                    {
                        dbsql = masterHelp.TblUpdt("t_batchdtl", TTXN.AUTONO, "E");
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
                        dbsql = masterHelp.TblUpdt("t_progdtl", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_txndtl", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_txntrans", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_txnoth", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
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

                        //finance
                        dbsql = masterHelp.finTblUpdt("t_vch_gst", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                        dbsql = masterHelp.finTblUpdt("t_vch_bl", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                        dbsql = masterHelp.finTblUpdt("t_vch_class", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                        dbsql = masterHelp.finTblUpdt("t_vch_det", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                        dbsql = masterHelp.finTblUpdt("t_vch_hdr", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

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


                    dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(TTXN.AUTONO, VE.DefaultAction, "S", Month, TTXN.DOCCD, DOCPATTERN, TTXN.DOCDT.retStr(), TTXN.EMD_NO.retShort(), TTXN.DOCNO, Convert.ToDouble(TTXN.DOCNO), null, null, null, TTXN.SLCD);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    dbsql = masterHelp.RetModeltoSql(TTXN, VE.DefaultAction);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.RetModeltoSql(TXNTRANS);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.RetModeltoSql(TTXNOTH);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    // SAVE T_CNTRL_HDR_UNIQNO
                    string docbarcode = ""; string UNIQNO = salesfunc.retVchrUniqId(TTXN.DOCCD, TTXN.AUTONO);
                    sql = "select doccd,docbarcode from " + CommVar.CurSchema(UNQSNO) + ".m_doctype_bar where doccd='" + TTXN.DOCCD + "'";
                    DataTable dt = masterHelp.SQLquery(sql);
                    if (dt != null && dt.Rows.Count > 0) docbarcode = dt.Rows[0]["docbarcode"].retStr();
                    if (VE.DefaultAction == "A")
                    {
                        T_CNTRL_HDR_UNIQNO TCHUNIQNO = new T_CNTRL_HDR_UNIQNO();
                        TCHUNIQNO.CLCD = TTXN.CLCD;
                        TCHUNIQNO.AUTONO = TTXN.AUTONO;
                        TCHUNIQNO.UNIQNO = UNIQNO;
                        dbsql = masterHelp.RetModeltoSql(TCHUNIQNO);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    }
                    string lbatchini = "";
                    sql = "select lbatchini from " + CommVar.FinSchema(UNQSNO) + ".m_loca where loccd='" + CommVar.Loccd(UNQSNO) + "' and compcd='" + CommVar.Compcd(UNQSNO) + "'";
                    dt = masterHelp.SQLquery(sql);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        lbatchini = dt.Rows[0]["lbatchini"].retStr();
                    }
                    //END T_CNTRL_HDR_UNIQNO 
                    // create header record in finschema
                    if (blactpost == true || blgstpost == true)
                    {
                        Cn.Create_DOCCD(UNQSNO, "F", TTXN.DOCCD);
                        dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(TTXN.AUTONO, VE.DefaultAction, "F", Month, TTXN.DOCCD, DOCPATTERN, TTXN.DOCDT.retStr(), TTXN.EMD_NO.retShort(), TTXN.DOCNO, Convert.ToDouble(TTXN.DOCNO), null, null, null, TTXN.SLCD);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        double currrt = 0;
                        if (TTXN.CURRRT != null) currrt = Convert.ToDouble(TTXN.CURRRT);
                        dbsql = masterHelp.InsVch_Hdr(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, null, null, "Y", null, trcd, "", "", TTXN.CURR_CD, currrt, "", revcharge);
                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                    }


                    for (int i = 0; i <= VE.TPROGDTL.Count - 1; i++)
                    {
                        if (VE.TPROGDTL[i].SLNO != 0 && VE.TPROGDTL[i].ITCD != null)
                        {
                            T_PROGDTL TPROGDTL = new T_PROGDTL();
                            TPROGDTL.CLCD = TTXN.CLCD;
                            TPROGDTL.EMD_NO = TTXN.EMD_NO;
                            TPROGDTL.DTAG = TTXN.DTAG;
                            TPROGDTL.AUTONO = TTXN.AUTONO;
                            TPROGDTL.SLNO = VE.TPROGDTL[i].SLNO;
                            TPROGDTL.DOCCD = TTXN.DOCCD;
                            TPROGDTL.DOCDT = TTXN.DOCDT;
                            TPROGDTL.DOCNO = TTXN.DOCNO;
                            TPROGDTL.PROGAUTONO = VE.TPROGDTL[i].PROGAUTONO;
                            TPROGDTL.PROGSLNO = VE.TPROGDTL[i].PROGSLNO;
                            TPROGDTL.STKDRCR = stkdrcr;
                            TPROGDTL.NOS = VE.TPROGDTL[i].NOS == null ? 0 : VE.TPROGDTL[i].NOS.retDbl();
                            TPROGDTL.QNTY = VE.TPROGDTL[i].QNTY.retDbl();

                            dbsql = masterHelp.RetModeltoSql(TPROGDTL);
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                        }
                    }
                    for (int i = 0; i <= VE.TTXNDTL.Count - 1; i++)
                    {
                        //if (VE.TTXNDTL[i].SLNO != 0 && VE.TTXNDTL[i].ITCD != null && VE.TTXNDTL[i].MTRLJOBCD != null && VE.TTXNDTL[i].STKTYPE != null)
                        if (VE.TTXNDTL[i].SLNO != 0 && VE.TTXNDTL[i].ITCD != null)
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
                            TTXNDTL.MTRLJOBCD = VE.MTRLJOBCD;
                            TTXNDTL.ITCD = VE.TTXNDTL[i].ITCD;
                            TTXNDTL.PARTCD = VE.TTXNDTL[i].PARTCD;
                            TTXNDTL.COLRCD = VE.TTXNDTL[i].COLRCD;
                            TTXNDTL.SIZECD = VE.TTXNDTL[i].SIZECD;
                            TTXNDTL.STKDRCR = stkdrcr;
                            TTXNDTL.STKTYPE = "F";
                            TTXNDTL.HSNCODE = VE.TTXNDTL[i].HSNCODE;
                            TTXNDTL.ITREM = VE.TTXNDTL[i].ITREM;
                            TTXNDTL.PCSREM = VE.TTXNDTL[i].PCSREM;
                            TTXNDTL.FREESTK = VE.TTXNDTL[i].FREESTK;
                            TTXNDTL.BATCHNO = VE.TTXNDTL[i].BATCHNO;
                            //TTXNDTL.BALEYR = VE.BALEYR;// VE.TTXNDTL[i].BALEYR;
                            //TTXNDTL.BALENO = VE.TTXNDTL[i].BALENO;
                            TTXNDTL.GOCD = VE.T_TXN.GOCD;
                            TTXNDTL.JOBCD = VE.TTXNDTL[i].JOBCD;
                            TTXNDTL.NOS = VE.TTXNDTL[i].NOS == null ? 0 : VE.TTXNDTL[i].NOS;
                            TTXNDTL.QNTY = VE.TTXNDTL[i].QNTY;
                            TTXNDTL.BLQNTY = VE.TTXNDTL[i].BLQNTY;
                            TTXNDTL.RATE = VE.TTXNDTL[i].RATE;
                            TTXNDTL.AMT = VE.TTXNDTL[i].AMT;
                            TTXNDTL.FLAGMTR = VE.TTXNDTL[i].FLAGMTR;
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

                    COUNTER = 0; int COUNTERBATCH = 0; bool recoexist = false;
                    if (VE.TBATCHDTL != null && VE.TBATCHDTL.Count > 0)
                    {
                        for (int i = 0; i <= VE.TBATCHDTL.Count - 1; i++)
                        {
                            if (VE.TBATCHDTL[i].ITCD != null && VE.TBATCHDTL[i].QNTY != 0)
                            {
                                bool isNewBatch = false;
                                string barno = "";
                                string Action = "", SqlCondition = "";
                                if (VE.TBATCHDTL[i].SAMPLE == "Y")
                                {
                                    barno = VE.TBATCHDTL[i].BARNO;
                                    Action = "E";
                                    SqlCondition = "barno = '" + barno + "' and autono='" + TTXN.AUTONO + "' ";
                                }
                                else
                                {
                                    if (VE.TBATCHDTL[i].COMMONUNIQBAR == "E")
                                    {
                                        barno = VE.TBATCHDTL[i].BARNO;
                                        Action = "E";
                                        SqlCondition = "barno = '" + barno + "' and autono='" + TTXN.AUTONO + "' ";
                                    }
                                    else if (VE.T_TXN.BARGENTYPE == "E" || VE.TBATCHDTL[i].BARGENTYPE == "E")
                                    {
                                        barno = TranBarcodeGenerate(TTXN.DOCCD, lbatchini, docbarcode, UNIQNO, (VE.TBATCHDTL[i].SLNO));
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(VE.TBATCHDTL[i].BARNO))
                                        {
                                            barno = salesfunc.GenerateBARNO(VE.TBATCHDTL[i].ITCD, VE.TBATCHDTL[i].CLRBARCODE, VE.TBATCHDTL[i].SZBARCODE);
                                        }
                                        else
                                        {
                                            barno = VE.TBATCHDTL[i].BARNO;
                                            Action = "E";
                                            SqlCondition = "barno = '" + barno + "' and autono='" + TTXN.AUTONO + "' ";
                                        }
                                        sql = "  select ITCD,BARNO from " + CommVar.CurSchema(UNQSNO) + ".T_BATCHmst where barno='" + barno + "'";
                                        dt = masterHelp.SQLquery(sql);
                                        if (dt.Rows.Count == 0)
                                        {
                                            ContentFlg = "Barno:" + barno + " not found at Item master at rowno:" + VE.TBATCHDTL[i].SLNO + " and itcd=" + VE.TBATCHDTL[i].ITCD; goto dbnotsave;
                                        }
                                    }
                                }
                                sql = "Select BARNO from " + CommVar.CurSchema(UNQSNO) + ".t_batchmst where barno='" + barno + "'";
                                OraCmd.CommandText = sql;
                                using (var OraReco = OraCmd.ExecuteReader())
                                {
                                    if (OraReco.HasRows == false) isNewBatch = true;
                                }

                                //checking barno exist or not
                                if (Action == "E" && SqlCondition != "") isNewBatch = true;

                                if (isNewBatch == true)
                                {
                                    T_BATCHMST TBATCHMST = new T_BATCHMST();
                                    TBATCHMST.EMD_NO = TTXN.EMD_NO;
                                    TBATCHMST.CLCD = TTXN.CLCD;
                                    TBATCHMST.DTAG = TTXN.DTAG;
                                    TBATCHMST.TTAG = TTXN.TTAG;
                                    TBATCHMST.SLNO = VE.TBATCHDTL[i].SLNO; // ++COUNTERBATCH;
                                    TBATCHMST.AUTONO = TTXN.AUTONO;
                                    TBATCHMST.SLCD = TTXN.SLCD;
                                    TBATCHMST.MTRLJOBCD = VE.MTRLJOBCD;
                                    //TBATCHMST.STKTYPE = "F";
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
                                    TBATCHMST.FLAGMTR = VE.TBATCHDTL[i].FLAGMTR;
                                    //TBATCHMST.MTRL_COST = VE.TBATCHDTL[i].MTRL_COST;
                                    //TBATCHMST.OTH_COST = VE.TBATCHDTL[i].OTH_COST;
                                    TBATCHMST.ITREM = VE.TBATCHDTL[i].ITREM;
                                    TBATCHMST.PDESIGN = VE.TBATCHDTL[i].PDESIGN;
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
                                    if (VE.T_TXN.BARGENTYPE == "E" || VE.TBATCHDTL[i].BARGENTYPE == "E")
                                    {
                                        TBATCHMST.HSNCODE = VE.TBATCHDTL[i].HSNCODE;

                                        TBATCHMST.OURDESIGN = VE.TBATCHDTL[i].OURDESIGN;
                                        if (VE.TBATCHDTL[i].BarImages.retStr() != "")
                                        {
                                            dbsql = masterHelp.TblUpdt("T_BATCH_IMG_HDR", "", "E", "", "barno='" + barno + "'");
                                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                                            dbsql = masterHelp.TblUpdt("T_BATCH_IMG_HDR_LINK", "", "E", "", "barno='" + barno + "'");
                                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                                            var barimg = SaveBarImage(VE.TBATCHDTL[i].BarImages, barno, TTXN.EMD_NO.retShort());
                                            dbsql = masterHelp.RetModeltoSql(barimg);

                                            for (int tr = 0; tr <= barimg.Item1.Count - 1; tr++)
                                            {
                                                dbsql = masterHelp.RetModeltoSql(barimg.Item1[tr]);
                                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                            }
                                            var disntImgHdr = barimg.Item1.GroupBy(u => u.BARNO).Select(r => r.First()).ToList();
                                            foreach (var imgbar in disntImgHdr)
                                            {
                                                T_BATCH_IMG_HDR_LINK m_batchImglink = new T_BATCH_IMG_HDR_LINK();
                                                m_batchImglink.CLCD = TTXN.CLCD;
                                                m_batchImglink.EMD_NO = TTXN.EMD_NO;
                                                m_batchImglink.BARNO = imgbar.BARNO;
                                                m_batchImglink.MAINBARNO = imgbar.BARNO;

                                                dbsql = masterHelp.RetModeltoSql(m_batchImglink);
                                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                            }
                                        }

                                    }

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
                                TBATCHDTL.MTRLJOBCD = VE.MTRLJOBCD;
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
                                //TBATCHDTL.SCMDISCRATE = VE.TBATCHDTL[i].SCMDISCRATE;
                                //TBATCHDTL.SCMDISCTYPE = VE.TBATCHDTL[i].SCMDISCTYPE;
                                //TBATCHDTL.TDDISCRATE = VE.TBATCHDTL[i].TDDISCRATE;
                                //TBATCHDTL.TDDISCTYPE = VE.TBATCHDTL[i].TDDISCTYPE;
                                //TBATCHDTL.ORDAUTONO = VE.TBATCHDTL[i].ORDAUTONO;
                                //TBATCHDTL.ORDSLNO = VE.TBATCHDTL[i].ORDSLNO;
                                TBATCHDTL.DIA = VE.TBATCHDTL[i].DIA;
                                TBATCHDTL.CUTLENGTH = VE.TBATCHDTL[i].CUTLENGTH;
                                TBATCHDTL.LOCABIN = VE.TBATCHDTL[i].LOCABIN;
                                TBATCHDTL.SHADE = VE.TBATCHDTL[i].SHADE;
                                TBATCHDTL.MILLNM = VE.TBATCHDTL[i].MILLNM;
                                TBATCHDTL.BATCHNO = VE.TBATCHDTL[i].BATCHNO;
                                //TBATCHDTL.BALEYR = VE.BALEYR;// VE.TBATCHDTL[i].BALEYR;
                                //TBATCHDTL.BALENO = VE.TBATCHDTL[i].BALENO;
                                TBATCHDTL.RECPROGAUTONO = VE.TBATCHDTL[i].RECPROGAUTONO;
                                TBATCHDTL.RECPROGSLNO = VE.TBATCHDTL[i].RECPROGSLNO;
                                TBATCHDTL.STKTYPE = "F";
                                TBATCHDTL.MTRLCOST = VE.TBATCHDTL[i].MTRLCOST;
                                dbsql = masterHelp.RetModeltoSql(TBATCHDTL);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                            }
                        }
                    }

                    if (dbqty == 0)
                    {
                        dberrmsg = "Quantity not entered"; goto dbnotsave;
                    }
                    if (VE.T_TXN.PREFNO == null)
                    {
                        dberrmsg = "Party Bill No not entered"; goto dbnotsave;
                    }
                    isl = 1;

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

                                if (blactpost == true)
                                {
                                    isl = isl + 1;
                                    string adrcr = cr;
                                    if (VE.TTXNAMT[i].ADDLESS == "L") adrcr = dr;
                                    dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), adrcr, VE.TTXNAMT[i].GLCD, sslcd,
                                            Convert.ToDouble(VE.TTXNAMT[i].AMT), strrem, parglcd, TTXN.SLCD, 0, 0, Convert.ToDouble(VE.TTXNAMT[i].CURR_AMT));
                                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                    if (adrcr == "D") dbDrAmt = dbDrAmt + Convert.ToDouble(VE.TTXNAMT[i].AMT);
                                    else dbCrAmt = dbCrAmt + Convert.ToDouble(VE.TTXNAMT[i].AMT);

                                    if (VE.TTXNDTL != null && VE.TTXNDTL[0].CLASS1CD != null)
                                    {
                                        dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                                                VE.TTXNDTL[0].CLASS1CD, "", Convert.ToDouble(VE.TTXNAMT[i].AMT), 0, strrem);
                                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                    }
                                }
                                igst = igst + Convert.ToDouble(VE.TTXNAMT[i].IGSTAMT);
                                cgst = cgst + Convert.ToDouble(VE.TTXNAMT[i].CGSTAMT);
                                sgst = sgst + Convert.ToDouble(VE.TTXNAMT[i].SGSTAMT);
                                cess = cess + Convert.ToDouble(VE.TTXNAMT[i].CESSAMT);
                                duty = duty + Convert.ToDouble(VE.TTXNAMT[i].DUTYAMT);
                            }
                        }
                    }
                    #region Document Passing checking
                    //-----------------------DOCUMENT PASSING DATA---------------------------//
                    double TRAN_AMT = Convert.ToDouble(TTXN.BLAMT);

                    var TCDP_DATA = Cn.T_CONTROL_DOC_PASS(TTXN.DOCCD, TRAN_AMT, TTXN.EMD_NO.Value, TTXN.AUTONO, CommVar.CurSchema(UNQSNO));
                    if (TCDP_DATA.Item1.Count != 0)
                    {
                        DB.T_CNTRL_DOC_PASS.AddRange(TCDP_DATA.Item1);
                    }

                    //if (docpassrem != "") DB.T_CNTRL_DOC_PASS.Add(TCDP);
                    #endregion

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
                    #region Financial Posting
                    string salpur = "";
                    double itamt = 0;
                    if (blactpost == true)
                    {
                        if (VE.MENU_PARA == "SB" || VE.MENU_PARA == "SD" || VE.MENU_PARA == "SR" || VE.MENU_PARA == "SBPOS" || VE.MENU_PARA == "SRPOS") salpur = "S";
                        else salpur = "P";
                        string prodrem = strrem; expglcd = "";

                        var AMTGLCD = (from x in VE.TTXNDTL
                                       group x by new { x.GLCD, x.CLASS1CD } into P
                                       select new
                                       {
                                           GLCD = P.Key.GLCD,
                                           CLASS1CD = P.Key.CLASS1CD,
                                           QTY = P.Sum(A => A.QNTY),
                                           TXBLVAL = P.Sum(A => A.TXBLVAL)
                                       }).Where(a => a.QTY != 0).ToList();

                        if (AMTGLCD != null && AMTGLCD.Count > 0)
                        {
                            for (int i = 0; i <= AMTGLCD.Count - 1; i++)
                            {
                                isl = isl + 1;
                                dbamt = AMTGLCD[i].TXBLVAL.retDbl();
                                dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), cr, AMTGLCD[i].GLCD, sslcd,
                                        dbamt, prodrem, parglcd, TTXN.SLCD, AMTGLCD[i].QTY.retDbl(), 0, 0);
                                OraCmd.CommandText = dbsql;
                                OraCmd.ExecuteNonQuery();
                                if (cr == "D") dbDrAmt = dbDrAmt + dbamt;
                                else dbCrAmt = dbCrAmt + dbamt;
                                dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                                        AMTGLCD[i].CLASS1CD, "", AMTGLCD[i].TXBLVAL.retDbl(), 0, strrem);
                                OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                itamt = itamt + AMTGLCD[i].TXBLVAL.retDbl();
                                expglcd = AMTGLCD[i].GLCD;
                            }
                        }
                        #region  GST Financial Part
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
                                dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), cr, gstpostcd[gt], sslcd,
                                        gstpostamt[gt], prodrem, tbl.Rows[0]["parglcd"].ToString(), TTXN.SLCD, dbqty, 0, 0);
                                OraCmd.CommandText = dbsql;
                                OraCmd.ExecuteNonQuery();
                                if (cr == "D") dbDrAmt = dbDrAmt + gstpostamt[gt];
                                else dbCrAmt = dbCrAmt + gstpostamt[gt];
                                dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                                        tbl.Rows[0]["class1cd"].ToString(), tbl.Rows[0]["class2cd"].ToString(), gstpostamt[gt], 0, strrem);
                                OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
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
                                    dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), dr, gstpostcd[gt], null,
                                            gstpostamt[gt], prodrem, tbl.Rows[0]["parglcd"].ToString(), TTXN.SLCD, dbqty, 0, 0);
                                    OraCmd.CommandText = dbsql;
                                    OraCmd.ExecuteNonQuery();
                                    if (dr == "D") dbDrAmt = dbDrAmt + gstpostamt[gt];
                                    else dbCrAmt = dbCrAmt + gstpostamt[gt];
                                    dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), null,
                                            tbl.Rows[0]["class1cd"].ToString(), tbl.Rows[0]["class2cd"].ToString(), gstpostamt[gt], 0, strrem);
                                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                }
                            }
                        }
                        #endregion
                        // TCS
                        dbamt = Convert.ToDouble(VE.T_TXN.TCSAMT);
                        if (dbamt != 0)
                        {
                            string adrcr = cr;

                            isl = isl + 1;
                            dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), adrcr, tbl.Rows[0]["tcsgl"].ToString(), sslcd,
                                    dbamt, strrem, tbl.Rows[0]["parglcd"].ToString(), TTXN.SLCD, 0, 0, 0);
                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                            if (adrcr == "D") dbDrAmt = dbDrAmt + dbamt;
                            else dbCrAmt = dbCrAmt + dbamt;
                            dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                                    null, null, Convert.ToDouble(VE.T_TXN.TCSAMT), 0, strrem + " TCS @ " + VE.T_TXN.TCSPER.ToString() + "%");
                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                        }
                        // Ronded off
                        dbamt = Convert.ToDouble(VE.T_TXN.ROAMT);
                        if (dbamt != 0)
                        {
                            string adrcr = cr;
                            if (dbamt < 0) adrcr = dr;
                            if (dbamt < 0) dbamt = dbamt * -1;

                            isl = isl + 1;
                            dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), adrcr, tbl.Rows[0]["rogl"].ToString(), null,
                                    dbamt, strrem, tbl.Rows[0]["parglcd"].ToString(), TTXN.SLCD, 0, 0, 0);
                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                            if (adrcr == "D") dbDrAmt = dbDrAmt + dbamt;
                            else dbCrAmt = dbCrAmt + dbamt;
                        }
                        //Party wise posting
                        isl = 1; //isl + 1;
                        dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), dr,
                            tbl.Rows[0]["parglcd"].ToString(), sslcd, Convert.ToDouble(VE.T_TXN.BLAMT), strrem, tbl.Rows[0]["prodglcd"].ToString(),
                            null, dbqty, 0, dbcurramt);
                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                        if (dr == "D") dbDrAmt = dbDrAmt + Convert.ToDouble(VE.T_TXN.BLAMT);
                        else dbCrAmt = dbCrAmt + Convert.ToDouble(VE.T_TXN.BLAMT);
                        dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                                tbl.Rows[0]["class1cd"].ToString(), tbl.Rows[0]["class2cd"].ToString(), Convert.ToDouble(VE.T_TXN.BLAMT), dbcurramt, strrem);
                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();

                        strblno = ""; strbldt = ""; strduedt = ""; strvtype = ""; strrefno = "";
                        if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "SB" || VE.MENU_PARA == "SBPOS") { strvtype = "BL"; } else if (VE.MENU_PARA == "SD") { strvtype = "DN"; } else { strvtype = "CN"; }
                        strduedt = Convert.ToDateTime(TTXN.DOCDT.Value).AddDays(Convert.ToDouble(TTXN.DUEDAYS)).ToString().retDateStr();
                        if (VE.MENU_PARA == "PB")
                        {
                            strblno = TTXN.PREFNO;
                            strbldt = TTXN.PREFDT.ToString();
                        }
                        else
                        {
                            strbldt = TTXN.DOCDT.ToString();
                            strblno = DOCPATTERN;
                        }
                        string blconslcd = TTXN.CONSLCD;
                        if (TTXN.SLCD != sslcd) blconslcd = TTXN.SLCD;
                        if (blconslcd == sslcd) blconslcd = "";
                        dbsql = masterHelp.InsVch_Bl(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, dr,
                                tbl.Rows[0]["parglcd"].ToString(), sslcd, blconslcd, TTXNOTH.AGSLCD, tbl.Rows[0]["class1cd"].ToString(), Convert.ToSByte(isl),
                                VE.T_TXN.BLAMT.retDbl(), strblno, strbldt, strrefno, strduedt, strvtype, TTXN.DUEDAYS.retDbl(), itamt, TTXNOTH.POREFNO,
                                TTXNOTH.POREFDT == null ? "" : TTXNOTH.POREFDT.ToString().retDateStr(), VE.T_TXN.BLAMT.retDbl(),
                                VE.T_TXNTRANS.LRNO, VE.T_TXNTRANS.LRDT == null ? "" : VE.T_TXNTRANS.LRDT.ToString().retDateStr(), VE.TRANSLNM);
                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                    }

                    if (blgstpost == true)
                    {
                        #region TVCHGST Table update    

                        int gs = 0;
                        //Posting of GST 
                        double amthsnamt = 0, amtigst = 0, amtcgst = 0, amtsgst = 0, amtcess = 0, amtgstper = 0, mult = 1;
                        double rplamt = 0, rpligst = 0, rplcgst = 0, rplsgst = 0, rplcess = 0;
                        double balamt = 0, baligst = 0, balcgst = 0, balsgst = 0, balcess = 0;
                        double amtigstper = 0, amtcgstper = 0, amtsgstper = 0;

                        if (VE.TTXNAMT != null)
                        {
                            for (int i = 0; i <= VE.TTXNAMT.Count - 1; i++)
                            {
                                if (VE.TTXNAMT[i].AMT != 0 && VE.TTXNAMT[i].HSNCODE == null)
                                {
                                    if (VE.TTXNAMT[i].ADDLESS == "A") mult = 1; else mult = -1;
                                    amthsnamt = amthsnamt + (VE.TTXNAMT[i].AMT.retDbl() * mult);
                                    amtigst = amtigst + (VE.TTXNAMT[i].IGSTAMT.retDbl() * mult);
                                    amtcgst = amtcgst + (VE.TTXNAMT[i].CGSTAMT.retDbl() * mult);
                                    amtsgst = amtsgst + (VE.TTXNAMT[i].SGSTAMT.retDbl() * mult);
                                    amtcess = amtcess + (VE.TTXNAMT[i].CESSAMT.retDbl() * mult);
                                    amtigstper = VE.TTXNAMT[i].IGSTPER.retDbl();
                                    amtcgstper = VE.TTXNAMT[i].CGSTPER.retDbl();
                                    amtsgstper = VE.TTXNAMT[i].SGSTPER.retDbl();
                                }
                            }
                            if (amthsnamt != 0)
                            {
                                balamt = amthsnamt; baligst = amtigst; balcgst = amtcgst; balsgst = amtsgst; amtcess = balcess;
                                lastitemno = 0; titamt = 0;
                                for (int i = 0; i <= VE.TTXNDTL.Count - 1; i++)
                                {
                                    if (VE.TTXNDTL[i].SLNO != 0 && VE.TTXNDTL[i].ITCD != null && VE.TTXNDTL[i].IGSTPER == amtigstper && VE.TTXNDTL[i].CGSTPER == amtcgstper && VE.TTXNDTL[i].SGSTPER == amtsgstper)
                                    {
                                        titamt = titamt + VE.TTXNDTL[i].AMT.retDbl();
                                        lastitemno = i;
                                    }
                                }

                                for (int i = 0; i <= VE.TTXNDTL.Count - 1; i++)
                                {
                                    if (VE.TTXNDTL[i].SLNO != 0 && VE.TTXNDTL[i].ITCD != null && VE.TTXNDTL[i].IGSTPER == amtigstper && VE.TTXNDTL[i].CGSTPER == amtcgstper && VE.TTXNDTL[i].SGSTPER == amtsgstper)
                                    {
                                        if (i == lastitemno)
                                        {
                                            rplamt = balamt; rpligst = baligst; rplcgst = balcgst; rplsgst = balsgst; rplcess = balcess;
                                        }
                                        else
                                        {
                                            rplamt = ((amthsnamt / titamt) * VE.TTXNDTL[i].AMT).retDbl().toRound();
                                            rpligst = ((amtigst / titamt) * VE.TTXNDTL[i].AMT).retDbl().toRound();
                                            rplcgst = ((amtcgst / titamt) * VE.TTXNDTL[i].AMT).retDbl().toRound();
                                            rplsgst = ((amtsgst / titamt) * VE.TTXNDTL[i].AMT).retDbl().toRound();
                                            rplcess = ((amtcess / titamt) * VE.TTXNDTL[i].AMT).retDbl().toRound();
                                        }
                                        balamt = balamt - rplamt;
                                        baligst = baligst - rpligst;
                                        balcgst = balcgst - rplcgst;
                                        balsgst = balsgst - rplsgst;
                                        balcess = balcess - rplcess;

                                        VE.TTXNDTL[i].NETAMT = VE.TTXNDTL[i].NETAMT + rplamt;
                                        VE.TTXNDTL[i].IGSTAMT = VE.TTXNDTL[i].IGSTAMT + rpligst;
                                        VE.TTXNDTL[i].CGSTAMT = VE.TTXNDTL[i].CGSTAMT + rplcgst;
                                        VE.TTXNDTL[i].SGSTAMT = VE.TTXNDTL[i].SGSTAMT + rplsgst;
                                        VE.TTXNDTL[i].CESSAMT = VE.TTXNDTL[i].CESSAMT + rplcess;
                                    }
                                }
                            }
                        }

                        string dncntag = ""; string exemptype = "";
                        if (VE.MENU_PARA == "SR" || VE.MENU_PARA == "SRPOS") dncntag = "SC";
                        if (VE.MENU_PARA == "PR") dncntag = "PD";
                        double gblamt = TTXN.BLAMT.retDbl(); double groamt = TTXN.ROAMT.retDbl() + TTXN.TCSAMT.retDbl();
                        if (VE.T_VCH_GST == null) { VE.T_VCH_GST = new T_VCH_GST(); }
                        for (int i = 0; i <= VE.TTXNDTL.Count - 1; i++)
                        {
                            if (VE.TTXNDTL[i].SLNO != 0 && VE.TTXNDTL[i].ITCD != null)
                            {
                                gs = gs + 1;
                                if (VE.TTXNDTL[i].GSTPER.retDbl() == 0) exemptype = "N";
                                string SHIPDOCDT = VE.T_VCH_GST.SHIPDOCDT.retStr() == "" ? "" : VE.T_VCH_GST.SHIPDOCDT.retDateStr();
                                dbsql = masterHelp.InsVch_GST(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, cr,
                                        Convert.ToSByte(isl), Convert.ToSByte(gs), TTXN.SLCD, strblno, strbldt, VE.TTXNDTL[i].HSNCODE, VE.TTXNDTL[i].ITNM, (VE.TTXNDTL[i].BLQNTY == null || VE.TTXNDTL[i].BLQNTY == 0 ? VE.TTXNDTL[i].QNTY.retDbl() : VE.TTXNDTL[i].BLQNTY.retDbl()), (VE.TTXNDTL[i].UOM == null ? VE.TTXNDTL[i].UOM : VE.TTXNDTL[i].UOM),
                                        VE.TTXNDTL[i].NETAMT.retDbl(), VE.TTXNDTL[i].IGSTPER.retDbl(), VE.TTXNDTL[i].IGSTAMT.retDbl(), VE.TTXNDTL[i].CGSTPER.retDbl(), VE.TTXNDTL[i].CGSTAMT.retDbl(), VE.TTXNDTL[i].SGSTPER.retDbl(), VE.TTXNDTL[i].SGSTAMT.retDbl(),
                                        VE.TTXNDTL[i].CESSPER.retDbl(), VE.TTXNDTL[i].CESSAMT.retDbl(), salpur, groamt, gblamt, 0, (VE.T_VCH_GST.INVTYPECD == null ? "01" : VE.T_VCH_GST.INVTYPECD), VE.POS, VE.TTXNDTL[i].AGDOCNO, VE.TTXNDTL[i].AGDOCDT.ToString(), TTXNOTH.DNCNCD,
                                        VE.GSTSLNM, VE.GSTNO, "", "", "", VE.T_VCH_GST.EXPCD, VE.T_VCH_GST.SHIPDOCNO, SHIPDOCDT, VE.T_VCH_GST.PORTCD, dncntag, TTXN.CONSLCD, exemptype, 0, expglcd, "Y");
                                OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                gblamt = 0; groamt = 0;
                            }
                        }
                        if (VE.TTXNAMT != null)
                        {
                            for (int i = 0; i <= VE.TTXNAMT.Count - 1; i++)
                            {
                                if (VE.TTXNAMT[i].SLNO != 0 && VE.TTXNAMT[i].AMTCD != null && VE.TTXNAMT[i].AMT != 0 && VE.TTXNAMT[i].HSNCODE != null)
                                {
                                    gs = gs + 1;
                                    string SHIPDOCDT = VE.T_VCH_GST.SHIPDOCDT.retStr() == "" ? "" : VE.T_VCH_GST.SHIPDOCDT.retDateStr();
                                    dbsql = masterHelp.InsVch_GST(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, cr,
                                            Convert.ToSByte(isl), Convert.ToSByte(gs), TTXN.SLCD, strblno, strbldt, VE.TTXNAMT[i].HSNCODE, VE.TTXNAMT[i].AMTNM, 0, "OTH",
                                            VE.TTXNAMT[i].AMT.retDbl(), VE.TTXNAMT[i].IGSTPER.retDbl(), VE.TTXNAMT[i].IGSTAMT.retDbl(), VE.TTXNAMT[i].CGSTPER.retDbl(), VE.TTXNAMT[i].CGSTAMT.retDbl(), VE.TTXNAMT[i].SGSTPER.retDbl(), VE.TTXNAMT[i].SGSTAMT.retDbl(),
                                            VE.TTXNAMT[i].CESSPER.retDbl(), VE.TTXNAMT[i].CESSAMT.retDbl(), salpur, groamt, gblamt, 0, (VE.T_VCH_GST.INVTYPECD == null ? "01" : VE.T_VCH_GST.INVTYPECD), VE.POS, VE.TTXNDTL[0].AGDOCNO, VE.TTXNDTL[0].AGDOCDT.ToString(), TTXNOTH.DNCNCD,
                                            VE.GSTSLNM, VE.GSTNO, "", "", "", VE.T_VCH_GST.EXPCD, VE.T_VCH_GST.SHIPDOCNO, SHIPDOCDT, VE.T_VCH_GST.PORTCD, dncntag, TTXN.CONSLCD, exemptype, 0, VE.TTXNAMT[i].GLCD, "Y");
                                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                    gblamt = 0; groamt = 0;
                                }
                            }
                        }
                        #endregion
                    }

                    if (Math.Round(dbDrAmt, 2) != Math.Round(dbCrAmt, 2))
                    {
                        dberrmsg = "Debit " + Math.Round(dbDrAmt, 2) + " & Credit " + Math.Round(dbCrAmt, 2) + " not matching please click on rounded off...";
                        goto dbnotsave;
                    }

                    #endregion
                    if (VE.DefaultAction == "A")
                    {
                        ContentFlg = "1" + " (Voucher No. " + DOCPATTERN + ")~" + TTXN.AUTONO;
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
                    dbsql = masterHelp.TblUpdt("t_batchdtl", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                    foreach (var v in VE.TBATCHDTL)
                    {
                        var IsTransactionFound = salesfunc.IsTransactionFound("", v.BARNO.retSqlformat(), VE.T_TXN.AUTONO.retSqlformat());
                        if (VE.MENU_PARA == "PB" && IsTransactionFound != "")
                        {
                            dberrmsg = "We cant delete this Bill. Transaction found at " + IsTransactionFound; goto dbnotsave;
                        }
                        else if ((VE.T_TXN.BARGENTYPE == "E" || v.BARGENTYPE == "E") && v.SAMPLE.retStr() != "Y")
                        {
                            dbsql = masterHelp.TblUpdt("T_BATCH_IMG_HDR", "", "D", "", "barno='" + v.BARNO + "'");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                            dbsql = masterHelp.TblUpdt("T_BATCH_IMG_HDR_LINK", "", "D", "", "barno='" + v.BARNO + "'");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();


                            dbsql = masterHelp.TblUpdt("t_batchmst", VE.T_TXN.AUTONO, "D", "", "barno='" + v.BARNO + "'");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        }
                    }

                    dbsql = masterHelp.TblUpdt("t_txnoth", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_txnamt", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_txntrans", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_progdtl", VE.T_TXN.AUTONO, "D");
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
                    dbsql = masterHelp.TblUpdt("t_cntrl_hdr_uniqno", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                    // Delete from financial schema
                    dbsql = masterHelp.TblUpdt("t_cntrl_auth", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.finTblUpdt("t_vch_gst", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_vch_bl", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_vch_class", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_vch_det", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_vch_hdr", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_cntrl_hdr", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

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
                goto dbok;
            dbnotsave:;
                OraTrans.Rollback();
                OraCon.Dispose();
                return Content(dberrmsg);
            dbok:;
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                OraTrans.Rollback();
                OraCon.Dispose();
                return Content(ex.Message + ex.InnerException);
            }
        }
        //public ActionResult SAVE(FormCollection FC, TransactionOutRecProcess VE)
        //{
        //    Cn.getQueryString(VE);
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
        //    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
        //    //Oracle Queries
        //    string dberrmsg = "";
        //    OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
        //    OraCon.Open();
        //    OracleCommand OraCmd = OraCon.CreateCommand();
        //    OracleTransaction OraTrans;
        //    string dbsql = "", postdt = "", weekrem = "", duedatecalcon = "", sql = "";
        //    string[] dbsql1;
        //    double dbDrAmt = 0, dbCrAmt = 0;
        //    OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted);
        //    OraCmd.Transaction = OraTrans;
        //    DB.Configuration.ValidateOnSaveEnabled = false;

        //    try
        //    {
        //        //DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".T_CNTRL_HDR in  row share mode");
        //        OraCmd.CommandText = "lock table " + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode"; OraCmd.ExecuteNonQuery();
        //        String query = "";
        //        string dr = ""; string cr = ""; int isl = 0; string strrem = "";
        //        double igst = 0; double cgst = 0; double sgst = 0; double cess = 0; double duty = 0; double dbqty = 0; double dbamt = 0; double dbcurramt = 0;
        //        Int32 z = 0; Int32 maxR = 0;
        //        string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
        //        string ContentFlg = "";
        //        if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
        //        {
        //            T_TXN TTXN = new T_TXN();
        //            T_TXNTRANS TXNTRANS = new T_TXNTRANS();
        //            T_TXNOTH TTXNOTH = new T_TXNOTH();
        //            T_CNTRL_HDR TCH = new T_CNTRL_HDR();
        //            //T_VCH_GST TVCHGST = new T_VCH_GST();
        //            //T_CNTRL_DOC_PASS TCDP = new T_CNTRL_DOC_PASS();
        //            string DOCPATTERN = "";
        //            TTXN.DOCDT = VE.T_TXN.DOCDT;
        //            string Ddate = Convert.ToString(TTXN.DOCDT);
        //            TTXN.CLCD = CommVar.ClientCode(UNQSNO);
        //            string auto_no = ""; string Month = "";
        //            if (VE.DefaultAction == "A")
        //            {
        //                TTXN.EMD_NO = 0;
        //                TTXN.DOCCD = VE.T_TXN.DOCCD;
        //                TTXN.DOCNO = Cn.MaxDocNumber(TTXN.DOCCD, Ddate);
        //                //TTXN.DOCNO = Cn.MaxDocNumber(TTXN.DOCCD, Ddate);
        //                DOCPATTERN = Cn.DocPattern(Convert.ToInt32(TTXN.DOCNO), TTXN.DOCCD, CommVar.CurSchema(UNQSNO).ToString(), CommVar.FinSchema(UNQSNO), Ddate);
        //                auto_no = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), TTXN.DOCNO, TTXN.DOCCD, Ddate);
        //                TTXN.AUTONO = auto_no.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
        //                Month = auto_no.Split(Convert.ToChar(Cn.GCS()))[1].ToString();
        //            }
        //            else
        //            {
        //                TTXN.DOCCD = VE.T_TXN.DOCCD;
        //                TTXN.DOCNO = VE.T_TXN.DOCNO;
        //                TTXN.AUTONO = VE.T_TXN.AUTONO;
        //                Month = VE.T_CNTRL_HDR.MNTHCD;
        //                TTXN.EMD_NO = Convert.ToInt16((VE.T_CNTRL_HDR.EMD_NO == null ? 0 : VE.T_CNTRL_HDR.EMD_NO) + 1);
        //                DOCPATTERN = VE.T_CNTRL_HDR.DOCNO;
        //            }
        //            TTXN.DOCTAG = "AA";// VE.MENU_PARA;
        //            TTXN.SLCD = VE.T_TXN.SLCD;
        //            //TTXN.CONSLCD = VE.T_TXN.CONSLCD;
        //            //TTXN.CURR_CD = VE.T_TXN.CURR_CD;
        //            //TTXN.CURRRT = VE.T_TXN.CURRRT;
        //            //TTXN.BLAMT = VE.T_TXN.BLAMT;
        //            //TTXN.PREFDT = VE.T_TXN.PREFDT;
        //            //TTXN.PREFNO = VE.T_TXN.PREFNO;
        //            //TTXN.REVCHRG = VE.T_TXN.REVCHRG;
        //            //TTXN.ROAMT = VE.T_TXN.ROAMT;
        //            //if (VE.RoundOff == true) { TTXN.ROYN = "Y"; } else { TTXN.ROYN = "N"; }

        //            TTXN.JOBCD = VE.T_TXN.JOBCD;
        //            TTXN.GOCD = VE.T_TXN.GOCD;
        //            //TTXN.MANSLIPNO = VE.T_TXN.MANSLIPNO;
        //            //TTXN.DUEDAYS = VE.T_TXN.DUEDAYS;
        //            //TTXN.PARGLCD = VE.T_TXN.PARGLCD;
        //            //TTXN.GLCD = VE.T_TXN.GLCD;
        //            //TTXN.CLASS1CD = VE.T_TXN.CLASS1CD;
        //            //TTXN.CLASS2CD = VE.T_TXN.CLASS2CD;
        //            //TTXN.LINECD = VE.T_TXN.LINECD;
        //            //TTXN.BARGENTYPE = VE.T_TXN.BARGENTYPE;
        //            //TTXN.WPPER = VE.T_TXN.WPPER;
        //            //TTXN.RPPER = VE.T_TXN.RPPER;
        //            TTXN.MENU_PARA = VE.T_TXN.MENU_PARA;
        //            //TTXN.TCSPER = VE.T_TXN.TCSPER;
        //            //TTXN.TCSAMT = VE.T_TXN.TCSAMT;
        //            TTXN.DTAG = (VE.DefaultAction == "E" ? "E" : null);


        //            if (VE.DefaultAction == "E")
        //            {
        //                #region batch and detail data
        //                dbsql = masterHelp.TblUpdt("t_batchdtl", TTXN.AUTONO, "E");
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

        //                ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
        //                var comp = DB1.T_BATCHMST.Where(x => x.AUTONO == TTXN.AUTONO).OrderBy(s => s.AUTONO).ToList();
        //                foreach (var v in comp)
        //                {
        //                    if (!VE.TBATCHDTL.Where(s => s.BARNO == v.BARNO && s.SLNO == v.SLNO).Any())
        //                    {
        //                        dbsql = masterHelp.TblUpdt("t_batchmst", TTXN.AUTONO, "E", "S", "BARNO = '" + v.BARNO + "' and SLNO = '" + v.SLNO + "' ");
        //                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                    }
        //                }
        //                //dbsql = masterHelp.TblUpdt("t_batchmst", TTXN.AUTONO, "E");
        //                //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                dbsql = masterHelp.TblUpdt("t_txndtl", TTXN.AUTONO, "E");
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                #endregion

        //                dbsql = masterHelp.TblUpdt("t_progbom", TTXN.AUTONO, "E");
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                dbsql = masterHelp.TblUpdt("t_progdtl", TTXN.AUTONO, "E");
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                dbsql = masterHelp.TblUpdt("t_progmast", TTXN.AUTONO, "E");
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }


        //                dbsql = masterHelp.TblUpdt("t_batchdtl", TTXN.AUTONO, "E");
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                dbsql = masterHelp.TblUpdt("t_batchmst", TTXN.AUTONO, "E");
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                //dbsql = MasterHelpFa.TblUpdt("t_txndtl", TTXN.AUTONO, "E");
        //                //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                //dbsql = MasterHelpFa.TblUpdt("t_txn_linkno", TTXN.AUTONO, "E");
        //                //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                //dbsql = masterHelp.TblUpdt("t_txntrans", TTXN.AUTONO, "E");
        //                //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                //dbsql = masterHelp.TblUpdt("t_txnoth", TTXN.AUTONO, "E");
        //                //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                //dbsql = MasterHelpFa.TblUpdt("t_vch_gst", TTXN.AUTONO, "E");
        //                //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                //dbsql = MasterHelpFa.TblUpdt("t_txnamt", TTXN.AUTONO, "E");
        //                //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                dbsql = masterHelp.TblUpdt("t_cntrl_hdr_rem", TTXN.AUTONO, "E");
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc", TTXN.AUTONO, "E");
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc_dtl", TTXN.AUTONO, "E");
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }


        //                //dbsql = MasterHelpFa.TblUpdt("t_cntrl_doc_pass", TTXN.AUTONO, "E");
        //                //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

        //            }

        //            //-------------------------Transport--------------------------//
        //            TXNTRANS.AUTONO = TTXN.AUTONO;
        //            TXNTRANS.EMD_NO = TTXN.EMD_NO;
        //            TXNTRANS.CLCD = TTXN.CLCD;
        //            TXNTRANS.DTAG = TTXN.DTAG;
        //            TXNTRANS.TRANSLCD = VE.T_TXNTRANS.TRANSLCD;
        //            TXNTRANS.TRANSMODE = VE.T_TXNTRANS.TRANSMODE;
        //            TXNTRANS.CRSLCD = VE.T_TXNTRANS.CRSLCD;
        //            TXNTRANS.EWAYBILLNO = VE.T_TXNTRANS.EWAYBILLNO;
        //            TXNTRANS.LRNO = VE.T_TXNTRANS.LRNO;
        //            TXNTRANS.LRDT = VE.T_TXNTRANS.LRDT;
        //            TXNTRANS.LORRYNO = VE.T_TXNTRANS.LORRYNO;
        //            TXNTRANS.GRWT = VE.T_TXNTRANS.GRWT;
        //            TXNTRANS.TRWT = VE.T_TXNTRANS.TRWT;
        //            TXNTRANS.NTWT = VE.T_TXNTRANS.NTWT;
        //            TXNTRANS.DESTN = VE.T_TXNTRANS.DESTN;
        //            TXNTRANS.RECVPERSON = VE.T_TXNTRANS.RECVPERSON;
        //            TXNTRANS.VECHLTYPE = VE.T_TXNTRANS.VECHLTYPE;
        //            TXNTRANS.GATEENTNO = VE.T_TXNTRANS.GATEENTNO;
        //            //----------------------------------------------------------//
        //            //-------------------------Other Info--------------------------//
        //            TTXNOTH.AUTONO = TTXN.AUTONO;
        //            TTXNOTH.EMD_NO = TTXN.EMD_NO;
        //            TTXNOTH.CLCD = TTXN.CLCD;
        //            TTXNOTH.DTAG = TTXN.DTAG;
        //            TTXNOTH.DOCREM = VE.T_TXNOTH.DOCREM;
        //            TTXNOTH.DNCNCD = VE.T_TXNOTH.DNCNCD;
        //            TTXNOTH.DNSALPUR = VE.T_TXNOTH.DNSALPUR;
        //            TTXNOTH.AGSLCD = VE.T_TXNOTH.AGSLCD;
        //            TTXNOTH.SAGSLCD = VE.T_TXNOTH.SAGSLCD;
        //            TTXNOTH.BLTYPE = VE.T_TXNOTH.BLTYPE;
        //            TTXNOTH.DESTN = VE.T_TXNOTH.DESTN;
        //            TTXNOTH.PLSUPPLY = VE.T_TXNOTH.PLSUPPLY;
        //            TTXNOTH.OTHADD1 = VE.T_TXNOTH.OTHADD1;
        //            TTXNOTH.OTHADD2 = VE.T_TXNOTH.OTHADD2;
        //            TTXNOTH.OTHADD3 = VE.T_TXNOTH.OTHADD3;
        //            TTXNOTH.OTHADD4 = VE.T_TXNOTH.OTHADD4;
        //            TTXNOTH.INSBY = VE.T_TXNOTH.INSBY;
        //            TTXNOTH.PAYTERMS = VE.T_TXNOTH.PAYTERMS;
        //            TTXNOTH.CASENOS = VE.T_TXNOTH.CASENOS;
        //            TTXNOTH.NOOFCASES = VE.T_TXNOTH.NOOFCASES;
        //            TTXNOTH.PRCCD = VE.T_TXNOTH.PRCCD;
        //            TTXNOTH.OTHNM = VE.T_TXNOTH.OTHNM;
        //            TTXNOTH.COD = VE.T_TXNOTH.COD;
        //            TTXNOTH.DOCTH = VE.T_TXNOTH.DOCTH;
        //            TTXNOTH.POREFNO = VE.T_TXNOTH.POREFNO;
        //            TTXNOTH.POREFDT = VE.T_TXNOTH.POREFDT;
        //            TTXNOTH.ECOMM = VE.T_TXNOTH.ECOMM;
        //            TTXNOTH.EXPCD = VE.T_TXNOTH.EXPCD;
        //            TTXNOTH.GSTNO = VE.T_TXNOTH.GSTNO;
        //            TTXNOTH.PNM = VE.T_TXNOTH.PNM;
        //            TTXNOTH.POS = VE.T_TXNOTH.POS;
        //            TTXNOTH.PACKBY = VE.T_TXNOTH.PACKBY;
        //            TTXNOTH.SELBY = VE.T_TXNOTH.SELBY;
        //            TTXNOTH.DEALBY = VE.T_TXNOTH.DEALBY;
        //            TTXNOTH.DESPBY = VE.T_TXNOTH.DESPBY;
        //            TTXNOTH.TAXGRPCD = VE.T_TXNOTH.TAXGRPCD;
        //            TTXNOTH.TDSHD = VE.T_TXNOTH.TDSHD;
        //            TTXNOTH.TDSON = VE.T_TXNOTH.TDSON;
        //            TTXNOTH.TDSPER = VE.T_TXNOTH.TDSPER;
        //            TTXNOTH.TDSAMT = VE.T_TXNOTH.TDSAMT;
        //            //----------------------------------------------------------//

        //            dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(TTXN.AUTONO, VE.DefaultAction, "S", Month, TTXN.DOCCD, DOCPATTERN, TTXN.DOCDT.retStr(), TTXN.EMD_NO.retShort(), TTXN.DOCNO, Convert.ToDouble(TTXN.DOCNO), null, null, null, TTXN.SLCD);
        //            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

        //            dbsql = masterHelp.RetModeltoSql(TTXN, VE.DefaultAction);
        //            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
        //            dbsql = masterHelp.RetModeltoSql(TXNTRANS);
        //            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
        //            dbsql = masterHelp.RetModeltoSql(TTXNOTH);
        //            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();


        //            //dbsql = MasterHelpFa.RetModeltoSql(TVCHGST,"A",CommVar.FinSchema(UNQSNO));
        //            //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();



        //            int COUNTER = 0;
        //            string stkdrcr = "C";
        //            string mtrljobcd = "";
        //            string stktype = "F";

        //            switch (VE.MENU_PARA)
        //            {
        //                case "DY":
        //                    stkdrcr = "C"; break;
        //                case "PR":
        //                    stkdrcr = "C"; break;
        //                case "ST":
        //                    stkdrcr = "C"; break;
        //                case "EM":
        //                    stkdrcr = "C"; break;
        //                case "JW":
        //                    stkdrcr = "C"; break;
        //            }

        //            for (int i = 0; i <= VE.TPROGDTL.Count - 1; i++)
        //            {
        //                if (VE.TPROGDTL[i].SLNO != 0 && VE.TPROGDTL[i].ITCD != null)
        //                {
        //                    COUNTER = COUNTER + 1;
        //                    T_PROGMAST TPROGMAST = new T_PROGMAST();
        //                    TPROGMAST.CLCD = TTXN.CLCD;
        //                    TPROGMAST.AUTONO = TTXN.AUTONO;
        //                    TPROGMAST.SLNO = VE.TPROGDTL[i].SLNO;
        //                    TPROGMAST.SLCD = TTXN.SLCD;
        //                    TPROGMAST.BARNO = VE.TPROGDTL[i].BARNO;
        //                    TPROGMAST.MTRLJOBCD = VE.TPROGDTL[i].MTRLJOBCD;
        //                    TPROGMAST.STKTYPE = stktype;
        //                    TPROGMAST.ITCD = VE.TPROGDTL[i].ITCD;
        //                    TPROGMAST.PARTCD = VE.TPROGDTL[i].PARTCD;
        //                    TPROGMAST.COLRCD = VE.TPROGDTL[i].COLRCD;
        //                    TPROGMAST.SIZECD = VE.TPROGDTL[i].SIZECD;
        //                    TPROGMAST.NOS = VE.TPROGDTL[i].NOS.retDcml();
        //                    TPROGMAST.QNTY = VE.TPROGDTL[i].QNTY.retDcml();
        //                    TPROGMAST.ITREMARK = VE.TPROGDTL[i].ITREMARK;
        //                    TPROGMAST.SHADE = VE.TPROGDTL[i].SHADE;
        //                    TPROGMAST.CUTLENGTH = VE.TPROGDTL[i].CUTLENGTH.retDcml();
        //                    TPROGMAST.JOBCD = TTXN.JOBCD;
        //                    TPROGMAST.PROGUNIQNO = salesfunc.retVchrUniqId(TTXN.DOCCD, TTXN.AUTONO) + COUNTER.retStr();
        //                    if (VE.TPROGDTL[i].CheckedSample == true) TPROGMAST.SAMPLE = "Y"; else TPROGMAST.SAMPLE = "N";
        //                    dbsql = masterHelp.RetModeltoSql(TPROGMAST);
        //                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

        //                    T_PROGDTL TPROGDTL = new T_PROGDTL();
        //                    TPROGDTL.CLCD = TTXN.CLCD;
        //                    TPROGDTL.EMD_NO = TTXN.EMD_NO;
        //                    TPROGDTL.DTAG = TTXN.DTAG;
        //                    TPROGDTL.AUTONO = TTXN.AUTONO;
        //                    TPROGDTL.SLNO = VE.TPROGDTL[i].SLNO;
        //                    TPROGDTL.DOCCD = TTXN.DOCCD;
        //                    TPROGDTL.DOCDT = TTXN.DOCDT;
        //                    TPROGDTL.DOCNO = TTXN.DOCNO;
        //                    TPROGDTL.PROGAUTONO = TTXN.AUTONO;
        //                    TPROGDTL.PROGSLNO = VE.TPROGDTL[i].SLNO;
        //                    TPROGDTL.STKDRCR = stkdrcr;
        //                    TPROGDTL.NOS = VE.TPROGDTL[i].NOS == null ? 0 : VE.TPROGDTL[i].NOS.retDbl();
        //                    TPROGDTL.QNTY = VE.TPROGDTL[i].QNTY.retDbl();

        //                    dbsql = masterHelp.RetModeltoSql(TPROGDTL);
        //                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

        //                }
        //            }
        //            for (int i = 0; i <= VE.TPROGBOM.Count - 1; i++)
        //            {
        //                if (VE.TPROGBOM[i].SLNO != 0 && VE.TPROGBOM[i].RSLNO != 0 && VE.TPROGBOM[i].ITCD != null)
        //                {
        //                    COUNTER = COUNTER + 1;
        //                    T_PROGBOM TPROGBOM = new T_PROGBOM();
        //                    TPROGBOM.CLCD = TTXN.CLCD;
        //                    TPROGBOM.EMD_NO = TTXN.EMD_NO;
        //                    TPROGBOM.DTAG = TTXN.DTAG;
        //                    TPROGBOM.AUTONO = TTXN.AUTONO;
        //                    TPROGBOM.SLNO = VE.TPROGBOM[i].SLNO;
        //                    TPROGBOM.RSLNO = VE.TPROGBOM[i].RSLNO;
        //                    TPROGBOM.ITCD = VE.TPROGBOM[i].ITCD;
        //                    TPROGBOM.PARTCD = VE.TPROGBOM[i].PARTCD;
        //                    TPROGBOM.SIZECD = VE.TPROGBOM[i].SIZECD;
        //                    TPROGBOM.COLRCD = VE.TPROGBOM[i].COLRCD;
        //                    TPROGBOM.BOMQNTY = VE.TPROGBOM[i].BOMQNTY.retDcml();
        //                    TPROGBOM.EXTRAQNTY = VE.TPROGBOM[i].EXTRAQNTY.retDcml();
        //                    TPROGBOM.QNTY = VE.TPROGBOM[i].QNTY.retDcml();
        //                    TPROGBOM.MTRLJOBCD = VE.TPROGBOM[i].MTRLJOBCD;
        //                    if (VE.TPROGBOM[i].Q_CheckedSample == true) TPROGBOM.SAMPLE = "Y"; else TPROGBOM.SAMPLE = "N";
        //                    dbsql = masterHelp.RetModeltoSql(TPROGBOM);
        //                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();


        //                }
        //            }
        //            //-------------------------Transport--------------------------//
        //            TXNTRANS.AUTONO = TTXN.AUTONO;
        //            TXNTRANS.EMD_NO = TTXN.EMD_NO;
        //            TXNTRANS.CLCD = TTXN.CLCD;
        //            TXNTRANS.DTAG = TTXN.DTAG;
        //            TXNTRANS.TRANSLCD = VE.T_TXNTRANS.TRANSLCD;
        //            TXNTRANS.TRANSMODE = VE.T_TXNTRANS.TRANSMODE;
        //            TXNTRANS.CRSLCD = VE.T_TXNTRANS.CRSLCD;
        //            TXNTRANS.EWAYBILLNO = VE.T_TXNTRANS.EWAYBILLNO;
        //            TXNTRANS.LRNO = VE.T_TXNTRANS.LRNO;
        //            TXNTRANS.LRDT = VE.T_TXNTRANS.LRDT;
        //            TXNTRANS.LORRYNO = VE.T_TXNTRANS.LORRYNO;
        //            TXNTRANS.GRWT = VE.T_TXNTRANS.GRWT;
        //            TXNTRANS.TRWT = VE.T_TXNTRANS.TRWT;
        //            TXNTRANS.NTWT = VE.T_TXNTRANS.NTWT;
        //            TXNTRANS.DESTN = VE.T_TXNTRANS.DESTN;
        //            TXNTRANS.RECVPERSON = VE.T_TXNTRANS.RECVPERSON;
        //            TXNTRANS.VECHLTYPE = VE.T_TXNTRANS.VECHLTYPE;
        //            TXNTRANS.GATEENTNO = VE.T_TXNTRANS.GATEENTNO;
        //            //----------------------------------------------------------//
        //            //-------------------------Other Info--------------------------//
        //            TTXNOTH.AUTONO = TTXN.AUTONO;
        //            TTXNOTH.EMD_NO = TTXN.EMD_NO;
        //            TTXNOTH.CLCD = TTXN.CLCD;
        //            TTXNOTH.DTAG = TTXN.DTAG;
        //            TTXNOTH.DOCREM = VE.T_TXNOTH.DOCREM;
        //            TTXNOTH.DNCNCD = VE.T_TXNOTH.DNCNCD;
        //            TTXNOTH.DNSALPUR = VE.T_TXNOTH.DNSALPUR;
        //            TTXNOTH.AGSLCD = VE.T_TXNOTH.AGSLCD;
        //            TTXNOTH.SAGSLCD = VE.T_TXNOTH.SAGSLCD;
        //            TTXNOTH.BLTYPE = VE.T_TXNOTH.BLTYPE;
        //            TTXNOTH.DESTN = VE.T_TXNOTH.DESTN;
        //            TTXNOTH.PLSUPPLY = VE.T_TXNOTH.PLSUPPLY;
        //            TTXNOTH.OTHADD1 = VE.T_TXNOTH.OTHADD1;
        //            TTXNOTH.OTHADD2 = VE.T_TXNOTH.OTHADD2;
        //            TTXNOTH.OTHADD3 = VE.T_TXNOTH.OTHADD3;
        //            TTXNOTH.OTHADD4 = VE.T_TXNOTH.OTHADD4;
        //            TTXNOTH.INSBY = VE.T_TXNOTH.INSBY;
        //            TTXNOTH.PAYTERMS = VE.T_TXNOTH.PAYTERMS;
        //            TTXNOTH.CASENOS = VE.T_TXNOTH.CASENOS;
        //            TTXNOTH.NOOFCASES = VE.T_TXNOTH.NOOFCASES;
        //            TTXNOTH.PRCCD = VE.T_TXNOTH.PRCCD;
        //            TTXNOTH.OTHNM = VE.T_TXNOTH.OTHNM;
        //            TTXNOTH.COD = VE.T_TXNOTH.COD;
        //            TTXNOTH.DOCTH = VE.T_TXNOTH.DOCTH;
        //            TTXNOTH.POREFNO = VE.T_TXNOTH.POREFNO;
        //            TTXNOTH.POREFDT = VE.T_TXNOTH.POREFDT;
        //            TTXNOTH.ECOMM = VE.T_TXNOTH.ECOMM;
        //            TTXNOTH.EXPCD = VE.T_TXNOTH.EXPCD;
        //            TTXNOTH.GSTNO = VE.T_TXNOTH.GSTNO;
        //            TTXNOTH.PNM = VE.T_TXNOTH.PNM;
        //            TTXNOTH.POS = VE.T_TXNOTH.POS;
        //            TTXNOTH.PACKBY = VE.T_TXNOTH.PACKBY;
        //            TTXNOTH.SELBY = VE.T_TXNOTH.SELBY;
        //            TTXNOTH.DEALBY = VE.T_TXNOTH.DEALBY;
        //            TTXNOTH.DESPBY = VE.T_TXNOTH.DESPBY;
        //            TTXNOTH.TAXGRPCD = VE.T_TXNOTH.TAXGRPCD;
        //            TTXNOTH.TDSHD = VE.T_TXNOTH.TDSHD;
        //            TTXNOTH.TDSON = VE.T_TXNOTH.TDSON;
        //            TTXNOTH.TDSPER = VE.T_TXNOTH.TDSPER;
        //            TTXNOTH.TDSAMT = VE.T_TXNOTH.TDSAMT;
        //            TTXNOTH.POREFNO = VE.T_TXNOTH.POREFNO;
        //            TTXNOTH.POREFDT = VE.T_TXNOTH.POREFDT;
        //            //----------------------------------------------------------//


        //            #region batch and detail data
        //            // SAVE T_CNTRL_HDR_UNIQNO
        //            string docbarcode = ""; string UNIQNO = salesfunc.retVchrUniqId(TTXN.DOCCD, TTXN.AUTONO);
        //            sql = "select doccd,docbarcode from " + CommVar.CurSchema(UNQSNO) + ".m_doctype_bar where doccd='" + TTXN.DOCCD + "'";
        //            DataTable dt = masterHelp.SQLquery(sql);
        //            if (dt != null && dt.Rows.Count > 0) docbarcode = dt.Rows[0]["docbarcode"].retStr();
        //            if (VE.DefaultAction == "A")
        //            {
        //                T_CNTRL_HDR_UNIQNO TCHUNIQNO = new T_CNTRL_HDR_UNIQNO();
        //                TCHUNIQNO.CLCD = TTXN.CLCD;
        //                TCHUNIQNO.AUTONO = TTXN.AUTONO;
        //                TCHUNIQNO.UNIQNO = UNIQNO;
        //                dbsql = masterHelp.RetModeltoSql(TCHUNIQNO);
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
        //            }
        //            string lbatchini = "";
        //            sql = "select lbatchini from " + CommVar.FinSchema(UNQSNO) + ".m_loca where loccd='" + CommVar.Loccd(UNQSNO) + "' and compcd='" + CommVar.Compcd(UNQSNO) + "'";
        //            dt = masterHelp.SQLquery(sql);
        //            if (dt != null && dt.Rows.Count > 0)
        //            {
        //                lbatchini = dt.Rows[0]["lbatchini"].retStr();
        //            }
        //            //END T_CNTRL_HDR_UNIQNO 

        //            double _amtdist = 0, _baldist = 0, _rpldist = 0, _mult = 1;
        //            double _amtdistq = 0, _baldistq = 0, _rpldistq = 0;
        //            double titamt = 0, titqty = 0;
        //            int lastitemno = 0;
        //            VE.BALEYR = Convert.ToDateTime(CommVar.FinStartDate(UNQSNO)).Year.retStr();
        //            for (int i = 0; i <= VE.TTXNDTL.Count - 1; i++)
        //            {
        //                if (VE.TTXNDTL[i].SLNO != 0 && VE.TTXNDTL[i].ITCD != null)
        //                {
        //                    titamt = titamt + VE.TTXNDTL[i].AMT.retDbl();
        //                    titqty = titqty + Convert.ToDouble(VE.TTXNDTL[i].QNTY);
        //                    lastitemno = i;
        //                }
        //            }
        //            _baldist = _amtdist; _baldistq = _amtdistq;

        //            for (int i = 0; i <= VE.TTXNDTL.Count - 1; i++)
        //            {
        //                if (VE.TTXNDTL[i].SLNO != 0 && VE.TTXNDTL[i].ITCD != null && VE.TTXNDTL[i].MTRLJOBCD != null && VE.TTXNDTL[i].STKTYPE != null)
        //                {
        //                    if (i == lastitemno) { _rpldist = _baldist; _rpldistq = _baldistq; }
        //                    else
        //                    {
        //                        if (_amtdist + _amtdistq == 0) { _rpldist = 0; _rpldistq = 0; }
        //                        else
        //                        {
        //                            _rpldist = ((_amtdist / titamt) * VE.TTXNDTL[i].AMT).retDbl().toRound();
        //                            _rpldistq = ((_amtdistq / titqty) * Convert.ToDouble(VE.TTXNDTL[i].QNTY)).toRound();
        //                        }
        //                    }
        //                    _baldist = _baldist - _rpldist; _baldistq = _baldistq - _rpldistq;

        //                    COUNTER = COUNTER + 1;
        //                    T_TXNDTL TTXNDTL = new T_TXNDTL();
        //                    TTXNDTL.CLCD = TTXN.CLCD;
        //                    TTXNDTL.EMD_NO = TTXN.EMD_NO;
        //                    TTXNDTL.DTAG = TTXN.DTAG;
        //                    TTXNDTL.AUTONO = TTXN.AUTONO;
        //                    TTXNDTL.SLNO = VE.TTXNDTL[i].SLNO;
        //                    TTXNDTL.MTRLJOBCD = VE.TTXNDTL[i].MTRLJOBCD;
        //                    TTXNDTL.ITCD = VE.TTXNDTL[i].ITCD;
        //                    TTXNDTL.PARTCD = VE.TTXNDTL[i].PARTCD;
        //                    TTXNDTL.COLRCD = VE.TTXNDTL[i].COLRCD;
        //                    TTXNDTL.SIZECD = VE.TTXNDTL[i].SIZECD;
        //                    TTXNDTL.STKDRCR = stkdrcr;
        //                    TTXNDTL.STKTYPE = VE.TTXNDTL[i].STKTYPE;
        //                    TTXNDTL.HSNCODE = VE.TTXNDTL[i].HSNCODE;
        //                    TTXNDTL.ITREM = VE.TTXNDTL[i].ITREM;
        //                    TTXNDTL.PCSREM = VE.TTXNDTL[i].PCSREM;
        //                    TTXNDTL.FREESTK = VE.TTXNDTL[i].FREESTK;
        //                    TTXNDTL.BATCHNO = VE.TTXNDTL[i].BATCHNO;
        //                    TTXNDTL.BALEYR = VE.BALEYR;// VE.TTXNDTL[i].BALEYR;
        //                    //TTXNDTL.BALENO = VE.TTXNDTL[i].BALENO;
        //                    TTXNDTL.GOCD = VE.T_TXN.GOCD;
        //                    TTXNDTL.JOBCD = VE.TTXNDTL[i].JOBCD;
        //                    TTXNDTL.NOS = VE.TTXNDTL[i].NOS == null ? 0 : VE.TTXNDTL[i].NOS;
        //                    TTXNDTL.QNTY = VE.TTXNDTL[i].QNTY;
        //                    TTXNDTL.BLQNTY = VE.TTXNDTL[i].BLQNTY;
        //                    TTXNDTL.RATE = VE.TTXNDTL[i].RATE;
        //                    TTXNDTL.AMT = VE.TTXNDTL[i].AMT;
        //                    //TTXNDTL.FLAGMTR = VE.TTXNDTL[i].FLAGMTR;
        //                    TTXNDTL.TOTDISCAMT = VE.TTXNDTL[i].TOTDISCAMT;
        //                    TTXNDTL.TXBLVAL = VE.TTXNDTL[i].TXBLVAL;
        //                    TTXNDTL.IGSTPER = VE.TTXNDTL[i].IGSTPER;
        //                    TTXNDTL.CGSTPER = VE.TTXNDTL[i].CGSTPER;
        //                    TTXNDTL.SGSTPER = VE.TTXNDTL[i].SGSTPER;
        //                    TTXNDTL.CESSPER = VE.TTXNDTL[i].CESSPER;
        //                    TTXNDTL.DUTYPER = VE.TTXNDTL[i].DUTYPER;
        //                    TTXNDTL.IGSTAMT = VE.TTXNDTL[i].IGSTAMT;
        //                    TTXNDTL.CGSTAMT = VE.TTXNDTL[i].CGSTAMT;
        //                    TTXNDTL.SGSTAMT = VE.TTXNDTL[i].SGSTAMT;
        //                    TTXNDTL.CESSAMT = VE.TTXNDTL[i].CESSAMT;
        //                    TTXNDTL.DUTYAMT = VE.TTXNDTL[i].DUTYAMT;
        //                    TTXNDTL.NETAMT = VE.TTXNDTL[i].NETAMT;
        //                    TTXNDTL.OTHRAMT = _rpldist + _rpldistq;
        //                    TTXNDTL.AGDOCNO = VE.TTXNDTL[i].AGDOCNO;
        //                    TTXNDTL.AGDOCDT = VE.TTXNDTL[i].AGDOCDT;
        //                    TTXNDTL.SHORTQNTY = VE.TTXNDTL[i].SHORTQNTY;
        //                    TTXNDTL.DISCTYPE = VE.TTXNDTL[i].DISCTYPE;
        //                    TTXNDTL.DISCRATE = VE.TTXNDTL[i].DISCRATE;
        //                    TTXNDTL.DISCAMT = VE.TTXNDTL[i].DISCAMT;
        //                    TTXNDTL.SCMDISCTYPE = VE.TTXNDTL[i].SCMDISCTYPE;
        //                    TTXNDTL.SCMDISCRATE = VE.TTXNDTL[i].SCMDISCRATE;
        //                    TTXNDTL.SCMDISCAMT = VE.TTXNDTL[i].SCMDISCAMT;
        //                    TTXNDTL.TDDISCTYPE = VE.TTXNDTL[i].TDDISCTYPE;
        //                    TTXNDTL.TDDISCRATE = VE.TTXNDTL[i].TDDISCRATE;
        //                    TTXNDTL.TDDISCAMT = VE.TTXNDTL[i].TDDISCAMT;
        //                    TTXNDTL.PRCCD = VE.T_TXNOTH.PRCCD;
        //                    TTXNDTL.PRCEFFDT = VE.TTXNDTL[i].PRCEFFDT;
        //                    TTXNDTL.GLCD = VE.TTXNDTL[i].GLCD;
        //                    TTXNDTL.CLASS1CD = VE.TTXNDTL[i].CLASS1CD;
        //                    dbsql = masterHelp.RetModeltoSql(TTXNDTL);
        //                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

        //                    dbqty = dbqty + Convert.ToDouble(VE.TTXNDTL[i].QNTY);
        //                    igst = igst + Convert.ToDouble(VE.TTXNDTL[i].IGSTAMT);
        //                    cgst = cgst + Convert.ToDouble(VE.TTXNDTL[i].CGSTAMT);
        //                    sgst = sgst + Convert.ToDouble(VE.TTXNDTL[i].SGSTAMT);
        //                    cess = cess + Convert.ToDouble(VE.TTXNDTL[i].CESSAMT);
        //                    duty = duty + Convert.ToDouble(VE.TTXNDTL[i].DUTYAMT);
        //                }
        //            }

        //            COUNTER = 0; int COUNTERBATCH = 0; bool recoexist = false;
        //            if (VE.TBATCHDTL != null && VE.TBATCHDTL.Count > 0)
        //            {
        //                for (int i = 0; i <= VE.TBATCHDTL.Count - 1; i++)
        //                {
        //                    if (VE.TBATCHDTL[i].ITCD != null && VE.TBATCHDTL[i].QNTY != 0)
        //                    {
        //                        bool flagbatch = false;
        //                        string barno = "";

        //                        barno = CommonBarcodeGenerate(VE.TBATCHDTL[i].ITGRPCD, VE.TBATCHDTL[i].ITCD, VE.TBATCHDTL[i].MTBARCODE, VE.TBATCHDTL[i].PRTBARCODE, VE.TBATCHDTL[i].CLRBARCODE, VE.TBATCHDTL[i].SZBARCODE);
        //                        sql = "Select * from " + CommVar.CurSchema(UNQSNO) + ".t_batchmst where barno='" + barno + "'";
        //                        OraCmd.CommandText = sql; var OraReco = OraCmd.ExecuteReader();
        //                        if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();

        //                        if (recoexist == false) flagbatch = true;


        //                        //checking barno exist or not
        //                        string Action = "", SqlCondition = "";
        //                        if (VE.DefaultAction == "A")
        //                        {
        //                            Action = VE.DefaultAction;
        //                        }
        //                        else
        //                        {
        //                            sql = "Select * from " + CommVar.CurSchema(UNQSNO) + ".t_batchmst where autono='" + TTXN.AUTONO + "' and slno = " + VE.TBATCHDTL[i].SLNO + " and barno='" + barno + "' ";
        //                            OraCmd.CommandText = sql; OraReco = OraCmd.ExecuteReader();
        //                            if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();

        //                            if (recoexist == true)
        //                            {
        //                                Action = "E";
        //                                SqlCondition = "autono = '" + TTXN.AUTONO + "' and slno = " + VE.TBATCHDTL[i].SLNO + " and barno='" + barno + "' ";
        //                                flagbatch = true;
        //                                barno = VE.TBATCHDTL[i].BARNO;
        //                            }
        //                            else
        //                            {
        //                                Action = "A";
        //                            }

        //                        }

        //                        if (flagbatch == true)
        //                        {
        //                            T_BATCHMST TBATCHMST = new T_BATCHMST();
        //                            TBATCHMST.EMD_NO = TTXN.EMD_NO;
        //                            TBATCHMST.CLCD = TTXN.CLCD;
        //                            TBATCHMST.DTAG = TTXN.DTAG;
        //                            TBATCHMST.TTAG = TTXN.TTAG;
        //                            TBATCHMST.SLNO = VE.TBATCHDTL[i].SLNO; // ++COUNTERBATCH;
        //                            TBATCHMST.AUTONO = TTXN.AUTONO;
        //                            TBATCHMST.SLCD = TTXN.SLCD;
        //                            TBATCHMST.MTRLJOBCD = VE.TBATCHDTL[i].MTRLJOBCD;
        //                            TBATCHMST.STKTYPE = VE.TBATCHDTL[i].STKTYPE;
        //                            TBATCHMST.JOBCD = TTXN.JOBCD;
        //                            TBATCHMST.BARNO = barno;
        //                            TBATCHMST.ITCD = VE.TBATCHDTL[i].ITCD;
        //                            TBATCHMST.PARTCD = VE.TBATCHDTL[i].PARTCD;
        //                            TBATCHMST.SIZECD = VE.TBATCHDTL[i].SIZECD;
        //                            TBATCHMST.COLRCD = VE.TBATCHDTL[i].COLRCD;
        //                            TBATCHMST.NOS = VE.TBATCHDTL[i].NOS;
        //                            TBATCHMST.QNTY = VE.TBATCHDTL[i].QNTY;
        //                            TBATCHMST.RATE = VE.TBATCHDTL[i].RATE;
        //                            //TBATCHMST.AMT = VE.TBATCHDTL[i].AMT;
        //                            //TBATCHMST.FLAGMTR = VE.TBATCHDTL[i].FLAGMTR;
        //                            //TBATCHMST.MTRL_COST = VE.TBATCHDTL[i].MTRL_COST;
        //                            //TBATCHMST.OTH_COST = VE.TBATCHDTL[i].OTH_COST;
        //                            TBATCHMST.ITREM = VE.TBATCHDTL[i].ITREM;
        //                            TBATCHMST.PDESIGN = VE.TBATCHDTL[i].PDESIGN;
        //                            if (VE.T_TXN.BARGENTYPE == "E")
        //                            {
        //                                TBATCHMST.HSNCODE = VE.TBATCHDTL[i].HSNCODE;
        //                            }
        //                            //TBATCHMST.ORGBATCHAUTONO = VE.TBATCHDTL[i].ORGBATCHAUTONO;
        //                            //TBATCHMST.ORGBATCHSLNO = VE.TBATCHDTL[i].ORGBATCHSLNO;
        //                            TBATCHMST.DIA = VE.TBATCHDTL[i].DIA;
        //                            TBATCHMST.CUTLENGTH = VE.TBATCHDTL[i].CUTLENGTH;
        //                            TBATCHMST.LOCABIN = VE.TBATCHDTL[i].LOCABIN;
        //                            TBATCHMST.SHADE = VE.TBATCHDTL[i].SHADE;
        //                            TBATCHMST.MILLNM = VE.TBATCHDTL[i].MILLNM;
        //                            TBATCHMST.BATCHNO = VE.TBATCHDTL[i].BATCHNO;
        //                            TBATCHMST.ORDAUTONO = VE.TBATCHDTL[i].ORDAUTONO;
        //                            //dbsql = masterHelp.RetModeltoSql(TBATCHMST);
        //                            dbsql = masterHelp.RetModeltoSql(TBATCHMST, Action, "", SqlCondition);
        //                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
        //                        }
        //                        COUNTER = COUNTER + 1;
        //                        T_BATCHDTL TBATCHDTL = new T_BATCHDTL();
        //                        TBATCHDTL.EMD_NO = TTXN.EMD_NO;
        //                        TBATCHDTL.CLCD = TTXN.CLCD;
        //                        TBATCHDTL.DTAG = TTXN.DTAG;
        //                        TBATCHDTL.TTAG = TTXN.TTAG;
        //                        TBATCHDTL.AUTONO = TTXN.AUTONO;
        //                        TBATCHDTL.TXNSLNO = VE.TBATCHDTL[i].TXNSLNO;
        //                        TBATCHDTL.SLNO = VE.TBATCHDTL[i].SLNO;  //COUNTER.retShort();
        //                        TBATCHDTL.GOCD = VE.T_TXN.GOCD;
        //                        TBATCHDTL.BARNO = barno;
        //                        TBATCHDTL.MTRLJOBCD = VE.TBATCHDTL[i].MTRLJOBCD;
        //                        TBATCHDTL.PARTCD = VE.TBATCHDTL[i].PARTCD;
        //                        TBATCHDTL.HSNCODE = VE.TBATCHDTL[i].HSNCODE;
        //                        TBATCHDTL.STKDRCR = stkdrcr;
        //                        TBATCHDTL.NOS = VE.TBATCHDTL[i].NOS;
        //                        TBATCHDTL.QNTY = VE.TBATCHDTL[i].QNTY;
        //                        TBATCHDTL.BLQNTY = VE.TBATCHDTL[i].BLQNTY;
        //                        //TBATCHDTL.FLAGMTR = VE.TBATCHDTL[i].FLAGMTR;
        //                        TBATCHDTL.ITREM = VE.TBATCHDTL[i].ITREM;
        //                        TBATCHDTL.RATE = VE.TBATCHDTL[i].RATE;
        //                        TBATCHDTL.DISCRATE = VE.TBATCHDTL[i].DISCRATE;
        //                        TBATCHDTL.DISCTYPE = VE.TBATCHDTL[i].DISCTYPE;
        //                        TBATCHDTL.SCMDISCRATE = VE.TBATCHDTL[i].SCMDISCRATE;
        //                        TBATCHDTL.SCMDISCTYPE = VE.TBATCHDTL[i].SCMDISCTYPE;
        //                        TBATCHDTL.TDDISCRATE = VE.TBATCHDTL[i].TDDISCRATE;
        //                        TBATCHDTL.TDDISCTYPE = VE.TBATCHDTL[i].TDDISCTYPE;
        //                        TBATCHDTL.ORDAUTONO = VE.TBATCHDTL[i].ORDAUTONO;
        //                        TBATCHDTL.ORDSLNO = VE.TBATCHDTL[i].ORDSLNO;
        //                        TBATCHDTL.DIA = VE.TBATCHDTL[i].DIA;
        //                        TBATCHDTL.CUTLENGTH = VE.TBATCHDTL[i].CUTLENGTH;
        //                        TBATCHDTL.LOCABIN = VE.TBATCHDTL[i].LOCABIN;
        //                        TBATCHDTL.SHADE = VE.TBATCHDTL[i].SHADE;
        //                        TBATCHDTL.MILLNM = VE.TBATCHDTL[i].MILLNM;
        //                        TBATCHDTL.BATCHNO = VE.TBATCHDTL[i].BATCHNO;
        //                        TBATCHDTL.BALEYR = VE.BALEYR;// VE.TBATCHDTL[i].BALEYR;
        //                        //TBATCHDTL.BALENO = VE.TBATCHDTL[i].BALENO;
        //                        dbsql = masterHelp.RetModeltoSql(TBATCHDTL);
        //                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
        //                    }
        //                }
        //            }

        //            if (dbqty == 0)
        //            {
        //                dberrmsg = "Quantity not entered"; goto dbnotsave;
        //            }

        //            #endregion
        //            //  -----------------------DOCUMENT PASSING DATA---------------------------//
        //            //double TRAN_AMT = Convert.ToDouble(TTXN.BLAMT);

        //            //var TCDP_DATA = Cn.T_CONTROL_DOC_PASS(TTXN.DOCCD, TRAN_AMT, TTXN.EMD_NO.Value, TTXN.AUTONO, CommVar.CurSchema(UNQSNO).ToString());
        //            //if (TCDP_DATA.Item1.Count != 0)
        //            //{
        //            //    for (int tr = 0; tr <= TCDP_DATA.Item1.Count - 1; tr++)
        //            //    {
        //            //        dbsql = MasterHelpFa.RetModeltoSql(TCDP_DATA.Item1[tr]);
        //            //        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
        //            //    }
        //            //}
        //            //if (docpassrem != "") DB.T_CNTRL_DOC_PASS.Add(TCDP);

        //            if (VE.UploadDOC != null)// add
        //            {
        //                var img = Cn.SaveUploadImageTransaction(VE.UploadDOC, TTXN.AUTONO, TTXN.EMD_NO.Value);
        //                if (img.Item1.Count != 0)
        //                {
        //                    for (int tr = 0; tr <= img.Item1.Count - 1; tr++)
        //                    {
        //                        dbsql = masterHelp.RetModeltoSql(img.Item1[tr]);
        //                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
        //                    }
        //                    for (int tr = 0; tr <= img.Item2.Count - 1; tr++)
        //                    {
        //                        dbsql = masterHelp.RetModeltoSql(img.Item2[tr]);
        //                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
        //                    }
        //                }
        //            }
        //            if (VE.T_CNTRL_HDR_REM.DOCREM != null)// add REMARKS
        //            {
        //                var NOTE = Cn.SAVETRANSACTIONREMARKS(VE.T_CNTRL_HDR_REM, TTXN.AUTONO, TTXN.CLCD, TTXN.EMD_NO.Value);
        //                if (NOTE.Item1.Count != 0)
        //                {
        //                    for (int tr = 0; tr <= NOTE.Item1.Count - 1; tr++)
        //                    {
        //                        dbsql = masterHelp.RetModeltoSql(NOTE.Item1[tr]);
        //                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
        //                    }
        //                }
        //            }
        //            string sslcd = VE.T_TXN.SLCD;


        //            if (VE.DefaultAction == "A")
        //            {
        //                ContentFlg = "1" + " (Receive No. " + TTXN.DOCNO + ")~" + TTXN.AUTONO;
        //            }
        //            else if (VE.DefaultAction == "E")
        //            {
        //                ContentFlg = "2";
        //            }
        //            OraTrans.Commit();
        //            OraCon.Dispose();
        //            return Content(ContentFlg);
        //        }
        //        else if (VE.DefaultAction == "V")
        //        {
        //            #region batch and detail data


        //            #endregion
        //            //dbsql = MasterHelpFa.TblUpdt("t_cntrl_doc_pass", VE.T_TXN.AUTONO, "D");
        //            //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //            dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc_dtl", VE.T_TXN.AUTONO, "D");
        //            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //            dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc", VE.T_TXN.AUTONO, "D");
        //            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //            dbsql = masterHelp.TblUpdt("t_cntrl_hdr_rem", VE.T_TXN.AUTONO, "D");
        //            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //            dbsql = masterHelp.TblUpdt("t_cntrl_hdr_uniqno", VE.T_TXN.AUTONO, "D");
        //            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //            dbsql = masterHelp.TblUpdt("t_batchdtl", VE.T_TXN.AUTONO, "D");
        //            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //            dbsql = masterHelp.TblUpdt("t_batchmst", VE.T_TXN.AUTONO, "D");
        //            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //            dbsql = masterHelp.TblUpdt("t_txndtl", VE.T_TXN.AUTONO, "D");
        //            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //            dbsql = masterHelp.TblUpdt("t_progbom", VE.T_TXN.AUTONO, "D");
        //            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //            dbsql = masterHelp.TblUpdt("t_progdtl", VE.T_TXN.AUTONO, "D");
        //            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //            //dbsql = MasterHelpFa.TblUpdt("t_txndtl", VE.T_TXN.AUTONO, "D");
        //            //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //            //dbsql = MasterHelpFa.TblUpdt("t_batchdtl", VE.T_TXN.AUTONO, "D");
        //            //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //            //dbsql = masterHelp.TblUpdt("t_batchmst", VE.T_TXN.AUTONO, "D");
        //            //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //            dbsql = masterHelp.TblUpdt("t_progmast", VE.T_TXN.AUTONO, "D");
        //            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //            dbsql = masterHelp.TblUpdt("T_TXNOTH", VE.T_TXN.AUTONO, "D");
        //            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //            dbsql = masterHelp.TblUpdt("T_TXNTRANS", VE.T_TXN.AUTONO, "D");
        //            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //            dbsql = masterHelp.TblUpdt("t_txn", VE.T_TXN.AUTONO, "D");
        //            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }


        //            dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(VE.T_TXN.AUTONO, "D", "S", null, null, null, VE.T_TXN.DOCDT.retStr(), null, null, null);
        //            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();


        //            ModelState.Clear();
        //            OraTrans.Commit();
        //            OraCon.Dispose();
        //            return Content("3");
        //        }
        //        else
        //        {
        //            return Content("");
        //        }
        //    dbnotsave:;
        //        OraTrans.Rollback();
        //        OraCon.Dispose();
        //        return Content(dberrmsg);
        //    }
        //    catch (Exception ex)
        //    {
        //        OraTrans.Rollback();
        //        OraCon.Dispose();
        //        return Content(ex.Message + ex.InnerException);
        //    }
        //}
    }
}