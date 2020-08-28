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
        Connection Cn = new Connection(); MasterHelp Master_Help = new MasterHelp(); MasterHelpFa MasterHelpFa = new MasterHelpFa(); SchemeCal Scheme_Cal = new SchemeCal(); Salesfunc salesfunc = new Salesfunc(); DataTable DT = new DataTable(); DataTable DTNEW = new DataTable();
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
                    ViewBag.formname = "Packing Slip Entry";
                    string LOC = CommVar.Loccd(UNQSNO);
                    string COM = CommVar.Compcd(UNQSNO);
                    string YR1 = CommVar.YearCode(UNQSNO);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    VE.DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);

                    VE.Database_Combo1 = (from n in DB.T_TXNOTH
                                          select new Database_Combo1() { FIELD_VALUE = n.BLTYPE }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();

                    VE.Database_Combo2 = (from n in DB.T_TXNOTH
                                          select new Database_Combo2() { FIELD_VALUE = n.SELBY }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();

                    VE.Database_Combo3 = (from n in DB.T_TXNOTH
                                          select new Database_Combo3() { FIELD_VALUE = n.DEALBY }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();
                    VE.DropDown_list_StkType = (from n in DB.M_STKTYPE
                                                select new DropDown_list_StkType() { value = n.STKTYPE, text = n.STKNAME }).OrderBy(s => s.value).Distinct().ToList();

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

                                //List<TTXNDTL> TTXNDTL = new List<TTXNDTL>();
                                //for (int i = 0; i <= 9; i++)
                                //{
                                //    TTXNDTL TXNDTL = new TTXNDTL();
                                //    TXNDTL.SLNO = Convert.ToInt16(i + 1);
                                //    TXNDTL.DropDown_list2 = Master_Help.STOCK_TYPE();
                                //    TXNDTL.DropDown_list3 = Master_Help.FREE_STOCK();
                                //    TXNDTL.DISC_TYPE = Master_Help.DISCOUNT_TYPE();
                                //    TTXNDTL.Add(TXNDTL);
                                //}
                                //VE.TTXNDTL = TTXNDTL;

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
                //if (TXN != null)
                //{
                //    VE.PartyGeneralLedgerName = (from z in DBF.M_GENLEG where z.GLCD == TXN.PARGLCD select z.GLNM).SingleOrDefault();
                //    VE.SalesGeneralLedgerName = (from z in DBF.M_GENLEG where z.GLCD == TXN.GLCD select z.GLNM).SingleOrDefault();
                //    VE.Class1Name = (from z in DBF.M_CLASS1 where z.CLASS1CD == TXN.CLASS1CD select z.CLASS1NM).SingleOrDefault();
                //    VE.Class2Name = (from z in DBF.M_CLASS2 where z.CLASS2CD == TXN.CLASS2CD select z.CLASS2NM).SingleOrDefault();
                //    VE.smsSendInfo = SMS.retSMSSendInfo(aa[0].Trim(), "S", "SALE");
                //}
                //if (TXNOTH != null)
                //{
                //    VE.DropDown_list1 = Master_Help.EFFECTIVE_DATE(TXNOTH.PRCCD, TXN.DOCDT.ToString().Substring(0, 10).Replace('-', '/'));
                //    VE.AgentName = (from z in DBF.M_SUBLEG where z.SLCD == TXNOTH.AGSLCD select z.SLNM).SingleOrDefault();
                //    VE.PriceName = (from z in DBF.M_PRCLST where z.PRCCD == TXNOTH.PRCCD select z.PRCNM).SingleOrDefault();

                //    // =
                //    // if (TXNOTH.COD == "Y") VE.COD_CHECK = true;
                //    //if (TXNOTH.PAIDBILTY == "Y") VE.PAIDBILTY_CHECK = true;
                //}
                //// MGP = DB.M_GROUP.Find(TXN.ITGRPCD);
                //// VE.PBLAMT = TXN.BLAMT;
                //VE.DOC_ID = TXN.DOCCD;
                //var Buyer = DBF.M_SUBLEG.Find(TXN.SLCD);
                //if (Buyer != null)
                //{
                //    VE.BuyerName = Buyer.SLNM;
                //    VE.DistrictName = Buyer.DISTRICT;
                //    VE.BuyerGSTNO = Buyer.GSTNO;
                //    VE.PSLCD = Buyer.PSLCD;
                //    var TAX_DATA = Master_Help.PRICELIST("", TXN.SLCD, "FORBUYER", "");
                //    string TAX_GRP_CODE = "";
                //    if (TAX_DATA != null && TAX_DATA != "")
                //    {
                //        TAX_GRP_CODE = TAX_DATA.Split(Convert.ToChar(Cn.GCS()))[0];
                //    }
                //    MTG = DBF.M_TAXGRP.Find(TAX_GRP_CODE);
                //}
                //if (TXN.CONSLCD != null)
                //{
                //    var Consignee = DBF.M_SUBLEG.Find(TXN.CONSLCD);
                //    VE.ConsigneeName = Consignee.SLNM;
                //    VE.ConsigneeGSTNO = Consignee.GSTNO;
                //}
                //VCHGST = (from x in DBF.T_VCH_GST where x.AUTONO == TXN.AUTONO select x).FirstOrDefault();
                //if (VCHGST != null)
                //{
                //    VE.PORTNM = DBI.MS_PORTCD.Find(VCHGST.PORTCD)?.PORTNM;
                //    VE.PORTCD = VCHGST.PORTCD;
                //    VE.SHIPDOCNO = VCHGST.SHIPDOCNO;
                //    VE.SHIPDOCDT = VCHGST.SHIPDOCDT;
                //}

                //if (TXNTRN != null)
                //{
                //    VE.TransporterName = (from z in DBF.M_SUBLEG where z.SLCD == TXNTRN.TRANSLCD select z.SLNM).SingleOrDefault();
                //    VE.CourierName = (from z in DBF.M_SUBLEG where z.SLCD == TXNTRN.CRSLCD select z.SLNM).SingleOrDefault();
                //}

                //MGO = DB.M_GODOWN.Find(TXN.GOCD);
                //MSC = DBI.MS_CURRENCY.Find(TXN.CURR_CD);
                SLR = Cn.GetTransactionReamrks(CommVar.CurSchema(UNQSNO).ToString(), TXN.AUTONO);
                VE.UploadDOC = Cn.GetUploadImageTransaction(CommVar.CurSchema(UNQSNO).ToString(), TXN.AUTONO);

                var PACK_AUTO = string.Join(",", from x in DB.T_TXN_LINKNO where x.AUTONO == TXN.AUTONO select x.LINKAUTONO);
                string[] PACK_AUTONO = null; if (PACK_AUTO != "") { PACK_AUTONO = PACK_AUTO.Split(','); }

                string sql = "", sqlpackautono = CommFunc.retSqlformat(PACK_AUTO);
                string scm = CommVar.CurSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);

                sql = "select distinct b.docno, c.prefno ";
                sql += "from " + scm + ".t_pslipdtl a, " + scm + ".t_cntrl_hdr b, " + scm + ".t_sord c ";
                sql += "where a.ordautono=b.autono(+) and a.ordautono=c.autono(+) ";
                if (sqlpackautono != "") sql += "and a.autono in (" + sqlpackautono + ") "; else sql += "and a.autono is null ";
                sql += "order by docno";
                DataTable tbl = Master_Help.SQLquery(sql);

                string ordnosinbill = string.Join(",", (from DataRow dr in tbl.Rows select dr["docno"].ToString()).Distinct());
                //VE.ordernosinbill = ordnosinbill;

                string orderprefnos = string.Join(",", (from DataRow dr in tbl.Rows select dr["prefno"].ToString()).Distinct());
                //VE.orderprefnos = orderprefnos;

                sql = "select a.autono, max(a.ordautono) ordautono, max(a.doautono) doautono, b.docno, to_char(b.docdt,'dd/mm/yyyy') docdt, ";
                sql += "sum(a.qnty) qnty ";
                sql += "from " + scm + ".t_pslipdtl a, " + scm + ".t_cntrl_hdr b ";
                sql += "where a.autono = b.autono and ";
                if (sqlpackautono != "") sql += "a.autono in (" + sqlpackautono + ") and ";
                sql += "b.compcd = '" + COM + "' and b.loccd = '" + LOC + "' ";
                sql += "group by a.autono, b.docno, to_char(b.docdt,'dd/mm/yyyy') ";
                sql += "order by a.autono ";
                tbl = Master_Help.SQLquery(sql);
                //Type tt = tbl.Rows[0]["qnty"].GetType();
                if (PACK_AUTONO != null)
                {
                    //VE.PACKING_SLIP = (from x in DB.T_PSLIPDTL
                    //                   join y in DB.T_CNTRL_HDR on x.AUTONO equals y.AUTONO
                    //                   select new { x.AUTONO, x.ORDAUTONO, x.DOAUTONO, y.DOCNO, y.DOCDT, x.QNTY }).ToList().Select(A => new PACKING_SLIP
                    //                   {
                    //                       PACKAUTONO = A.AUTONO,
                    //                       ORDAUTONO = A.ORDAUTONO,
                    //                       DOAUTONO = A.DOAUTONO,
                    //                       DOCNO = A.DOCNO,
                    //                       DOCDT = A.DOCDT.ToString().Substring(0, 10),
                    //                       PACK_QNTY = (from z in DB.T_PSLIPDTL where z.AUTONO == A.AUTONO select z.QNTY.Value).Sum()
                    //                   }).Where(Z => PACK_AUTONO.Contains(Z.PACKAUTONO)).DistinctBy(a => a.PACKAUTONO).OrderBy(a => a.PACKAUTONO).ToList();
                    //VE.PACKING_SLIP = (from dr in tbl.AsEnumerable()
                    //                   select new PACKING_SLIP()
                    //                   {
                    //                       PACKAUTONO = dr.Field<string>("autono"),
                    //                       ORDAUTONO = dr.Field<string>("ordautono"),
                    //                       DOAUTONO = dr.Field<string>("doautono"),
                    //                       DOCNO = dr.Field<string>("docno"),
                    //                       DOCDT = dr.Field<string>("docdt"),
                    //                       PACK_QNTY = Convert.ToDouble(dr.Field<decimal>("qnty"))
                    //                   }).ToList();
                }

                //if (VE.PACKING_SLIP != null && VE.PACKING_SLIP.Count > 0)
                //{
                //    short P_SL_NO = 0; double TOT_PACK = 0; VE.PACKING_SLIP.ForEach(A => { A.SLNO = ++P_SL_NO; TOT_PACK = TOT_PACK + A.PACK_QNTY; A.Checked = true; }); VE.TOTAL_P_QNTY = TOT_PACK;
                //}

                string str1 = "";
                str1 += "select i.AUTONO,i.DOAUTONO,i.ORDAUTONO,i.ITCD,j.STYLENO,i.ITCD,j.PCSPERBOX,j.ITNM,j.UOMCD,i.MTRLJOBCD,i.STKTYPE,i.FREESTK,i.STKTYPE, ";
                str1 += "i.FREESTK,i.QNTY,i.RATE,i.AMT,i.SIZECD,i.SIZECD,k.SIZENM,i.COLRCD,l.COLRNM,i.IGSTPER,i.CGSTPER,i.SGSTPER,i.CESSPER,i.IGSTAMT,i.CGSTAMT,i.SGSTAMT,i.CESSAMT, ";
                str1 += "i.DUTYPER,i.DUTYAMT,j.HSNSACCD,i.TDDISCTYPE,i.TDDISCRATE,i.TDDISCAMT,i.DISCTYPE,i.DISCRATE,i.DISCAMT,i.SCMDISCTYPE,i.SCMDISCRATE,i.SCMDISCAMT, ";
                str1 += "i.AGDOCNO,i.AGDOCDT ";
                str1 += "from " + CommVar.CurSchema(UNQSNO) + ".T_TXNDTL i, " + CommVar.CurSchema(UNQSNO) + ".M_SITEM j, " + CommVar.CurSchema(UNQSNO) + ".M_SIZE k, " + CommVar.CurSchema(UNQSNO) + ".M_COLOR l ";
                str1 += "where i.ITCD = j.ITCD(+) and i.SIZECD = k.SIZECD(+) and i.COLRCD = l.COLRCD(+) ";
                str1 += "and i.AUTONO = '" + TXN.AUTONO + "' ";
                str1 += "order by i.SLNO ";
                tbl = Master_Help.SQLquery(str1);

                //VE.TTXNDTL = (from DataRow dr in tbl.Rows
                //              select new TTXNDTL()
                //              {
                //                  AUTONO = dr["AUTONO"].retStr(),
                //                  DOAUTONO = dr["DOAUTONO"].retStr(),
                //                  ORDAUTONO = dr["ORDAUTONO"].retStr(),
                //                  ITCD = dr["ITCD"].retStr(),
                //                  ARTNO = dr["STYLENO"].retStr(),
                //                  ITEM_CODE = dr["ITCD"].retStr(),
                //                  TOTAL_PCS = dr["PCSPERBOX"].retDbl(),
                //                  ITNM = dr["ITNM"].retStr(),
                //                  UOMCD = dr["UOMCD"].retStr(),
                //                  MTRLJOBCD = dr["MTRLJOBCD"].retStr(),
                //                  STKTYPE = dr["STKTYPE"].retStr(),
                //                  FREESTK = dr["FREESTK"].retStr(),
                //                  STKTYPE_HIDDEN = dr["STKTYPE"].retStr(),
                //                  FREESTK_HIDDEN = dr["FREESTK"].retStr(),
                //                  QNTY = dr["QNTY"].retDbl(),
                //                  RATE = dr["RATE"].retDbl(),
                //                  BASAMT = dr["AMT"].retDbl(),
                //                  AMT = dr["AMT"].retDbl(),
                //                  SIZECD = dr["SIZECD"].retStr(),
                //                  ALL_SIZE = dr["SIZECD"].retStr(),
                //                  SIZENM = dr["SIZENM"].retStr(),
                //                  COLRCD = dr["COLRCD"].retStr(),
                //                  COLRNM = dr["COLRNM"].retStr(),
                //                  IGSTPER = dr["IGSTPER"].retDbl(),
                //                  CGSTPER = dr["CGSTPER"].retDbl(),
                //                  SGSTPER = dr["SGSTPER"].retDbl(),
                //                  CESSPER = dr["CESSPER"].retDbl(),
                //                  IGSTAMT = dr["IGSTAMT"].retDbl(),
                //                  CGSTAMT = dr["CGSTAMT"].retDbl(),
                //                  SGSTAMT = dr["SGSTAMT"].retDbl(),
                //                  CESSAMT = dr["CESSAMT"].retDbl(),
                //                  //DUTYPER = dr["DUTYPER"].retStr(),
                //                  //DUTYAMT = dr["DUTYAMT"].retStr(),
                //                  HSNCODE = dr["HSNSACCD"].retStr(),
                //                  TDDISCTYPE = dr["TDDISCTYPE"].retStr(),
                //                  TDDISCRATE = dr["TDDISCRATE"].retDbl(),
                //                  TDDISCAMT = dr["TDDISCAMT"].retDbl(),
                //                  DISCTYPE = dr["DISCTYPE"].retStr(),
                //                  DISCRATE = dr["DISCRATE"].retDbl(),
                //                  DISCAMT = dr["DISCAMT"].retDbl(),
                //                  SCMDISCTYPE = dr["SCMDISCTYPE"].retStr(),
                //                  SCMDISCRATE = dr["SCMDISCRATE"].retDbl(),
                //                  SCMDISCAMT = dr["SCMDISCAMT"].retDbl(),
                //                  AGSTDOCNO = dr["AGDOCNO"].retStr(),
                //                  AGSTDOCDT = dr["AGDOCDT"].retStr() == "" ? (DateTime?)null : Convert.ToDateTime(dr["AGDOCDT"].retDateStr()),
                //              }).OrderBy(s => s.ITCD).ToList();

                //VE.TTXNDTL = (from i in DB.T_TXNDTL
                //              join j in DB.M_SITEM on i.ITCD equals j.ITCD into x
                //              from j in x.DefaultIfEmpty()
                //              join k in DB.M_SIZE on i.SIZECD equals k.SIZECD into y
                //              from k in y.DefaultIfEmpty()
                //              join l in DB.M_COLOR on i.COLRCD equals l.COLRCD into z
                //              from l in z.DefaultIfEmpty()
                //              where i.AUTONO == TXN.AUTONO
                //              select new TTXNDTL()
                //              {
                //                  AUTONO = i.AUTONO,
                //                  DOAUTONO = i.DOAUTONO,
                //                  ORDAUTONO = i.ORDAUTONO,
                //                  ITCD = i.ITCD,
                //                  ARTNO = j.STYLENO,
                //                  ITEM_CODE = i.ITCD,
                //                  TOTAL_PCS = j.PCSPERBOX,
                //                  ITNM = j.ITNM,
                //                  UOMCD = j.UOMCD,
                //                  MTRLJOBCD = i.MTRLJOBCD,
                //                  STKTYPE = i.STKTYPE,
                //                  FREESTK = i.FREESTK,
                //                  STKTYPE_HIDDEN = i.STKTYPE,
                //                  FREESTK_HIDDEN = i.FREESTK,
                //                  QNTY = i.QNTY,
                //                  RATE = i.RATE,
                //                  BASAMT = i.AMT,
                //                  AMT = i.AMT,
                //                  SIZECD = i.SIZECD,
                //                  ALL_SIZE = i.SIZECD,
                //                  SIZENM = k.SIZENM,
                //                  COLRCD = i.COLRCD,
                //                  COLRNM = l.COLRNM,
                //                  IGSTPER = i.IGSTPER,
                //                  CGSTPER = i.CGSTPER,
                //                  SGSTPER = i.SGSTPER,
                //                  CESSPER = i.CESSPER,
                //                  IGSTAMT = i.IGSTAMT,
                //                  CGSTAMT = i.CGSTAMT,
                //                  SGSTAMT = i.SGSTAMT,
                //                  CESSAMT = i.CESSAMT,
                //                  //DUTYPER = i.DUTYPER,
                //                  //DUTYAMT = i.DUTYAMT,
                //                  HSNCODE = j.HSNSACCD,
                //                  TDDISCTYPE = i.TDDISCTYPE,
                //                  TDDISCRATE = i.TDDISCRATE,
                //                  TDDISCAMT = i.TDDISCAMT,
                //                  DISCTYPE = i.DISCTYPE,
                //                  DISCRATE = i.DISCRATE,
                //                  DISCAMT = i.DISCAMT,
                //                  SCMDISCTYPE = i.SCMDISCTYPE,
                //                  SCMDISCRATE = i.SCMDISCRATE,
                //                  SCMDISCAMT = i.SCMDISCAMT,
                //                  AGSTDOCNO = i.AGDOCNO,
                //                  AGSTDOCDT = i.AGDOCDT,
                //              }).OrderBy(s => s.ITCD).ToList();

                //var JOB_DATA = (from X in DB.T_TXNDTL join Y in DB.M_JOBMST on X.MTRLJOBCD equals Y.JOBCD where X.AUTONO == TXN.AUTONO select new { MTRLJOBCD = X.MTRLJOBCD, MTRLJOBNM = Y.JOBNM }).DistinctBy(A => A.MTRLJOBCD).ToList();
                //if (JOB_DATA != null && JOB_DATA.Count > 0) { VE.MTRLJOBCD = JOB_DATA[0].MTRLJOBCD; VE.MTRLJOBNM = JOB_DATA[0].MTRLJOBNM; }
                //double TOTAL_QNTY = 0; double TOTAL_AMT = 0; double TOTAL_TDS_AMT = 0; double TOTAL_SCM_AMT = 0; double TOTAL_DISC_AMT = 0; double TOTAL_GROSS = 0; double TOTAL_IGST_AMT = 0.00; double TOTAL_CGST_AMT = 0.00; double TOTAL_SGST_AMT = 0; double TOTAL_CESS_AMT = 0; double TOTAL_DUTY_AMT = 0; double TOTAL_OTHER_AMT = 0; double TOTAL_NET_AMT = 0;

                //foreach (var i in VE.TTXNDTL)
                //{
                //    if (VE.MENU_PARA == "SOTH" || VE.MENU_PARA == "SR")
                //    {
                //        string MTRLJOBCD = i.MTRLJOBCD == null ? "" : i.MTRLJOBCD;
                //        VE.MTRLJOBCD = MTRLJOBCD;
                //        VE.MTRLJOBNM = DB.M_MTRLJOBMST.Where(a => a.MTRLJOBCD == MTRLJOBCD).Select(a => a.MTRLJOBNM).SingleOrDefault();
                //    }

                //    var TEMP = (from a in DBF.M_UOM where a.UOMCD == i.UOMCD select a.UOMNM).ToList(); if (TEMP != null && TEMP.Count > 0) { i.UOMNM = TEMP[0]; }
                //    short SLNO = 0; string ITEM = i.ITCD; double? RATE = i.RATE; string FREE_STK = i.FREESTK; string STK_TYP = i.STKTYPE;

                //    VE.TTXNDTL_SIZE = (from x in VE.TTXNDTL
                //                       where x.ITCD == ITEM && x.FREESTK == FREE_STK && x.STKTYPE == STK_TYP
                //                       select new TTXNDTL_SIZE()
                //                       {
                //                           SLNO = ++SLNO,
                //                           DOAUTONO = x.DOAUTONO,
                //                           ORDAUTONO = x.ORDAUTONO,
                //                           SIZECD = (x.SIZECD == null ? "" : x.SIZECD),
                //                           SIZENM = (x.SIZENM == null ? "" : x.SIZENM),
                //                           COLRCD = (x.COLRCD == null ? "" : x.COLRCD),
                //                           COLRNM = (x.COLRNM == null ? "" : x.COLRNM),
                //                           RATE = x.RATE,
                //                           QNTY = x.QNTY,
                //                           ITCD = x.ITCD,
                //                           STKTYPE_HIDDEN = x.STKTYPE,
                //                           FREESTK_HIDDEN = x.FREESTK,
                //                           ITCOLSIZE = x.ITCD + (x.COLRCD == null ? "" : x.COLRCD) + (x.SIZECD == null ? "" : x.SIZECD),
                //                           PCS_HIDDEN = (x.TOTAL_PCS == null ? 0 : x.TOTAL_PCS.Value)
                //                       }).OrderBy(s => s.SLNO).ToList();
                //    if (VE.MENU_PARA != "SR")
                //    {
                //        if (VE.TTXNDTL_SIZE != null && VE.TTXNDTL_SIZE.Count > 0)
                //        {
                //            VE.TTXNDTL_SIZE = VE.TTXNDTL_SIZE.Where(a => a.RATE == RATE).ToList();
                //        }
                //    }
                //    var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                //    string JR = javaScriptSerializer.Serialize(VE.TTXNDTL_SIZE);
                //    i.ChildData = JR;
                //}
                //VE.TTXNDTL = (from p in VE.TTXNDTL
                //              group p by new
                //              {
                //                  p.AGSTDOCNO,
                //                  p.AGSTDOCDT,
                //                  p.DOAUTONO,
                //                  p.ORDAUTONO,
                //                  p.ARTNO,
                //                  p.ITCD,
                //                  p.ITNM,
                //                  p.TOTAL_PCS,
                //                  p.UOMCD,
                //                  p.UOMNM,
                //                  p.STKTYPE,
                //                  p.STKTYPE_HIDDEN,
                //                  p.FREESTK,
                //                  p.FREESTK_HIDDEN,
                //                  p.RATE,
                //                  p.ChildData,
                //                  p.IGSTPER,
                //                  p.CGSTPER,
                //                  p.SGSTPER,
                //                  p.CESSPER,
                //                  p.TDDISCTYPE,
                //                  p.TDDISCRATE,
                //                  p.DISCTYPE,
                //                  p.DISCRATE,
                //                  p.SCMDISCTYPE,
                //                  p.SCMDISCRATE
                //              } into z
                //              select new TTXNDTL
                //              {
                //                  AGSTDOCNO = z.Key.AGSTDOCNO,
                //                  AGSTDOCDT = z.Key.AGSTDOCDT,
                //                  DOAUTONO = z.Key.DOAUTONO,
                //                  ORDAUTONO = z.Key.ORDAUTONO,
                //                  ARTNO = z.Key.ARTNO,
                //                  ITCD = z.Key.ITCD,
                //                  ITNM = z.Key.ITNM,
                //                  TOTAL_PCS = z.Key.TOTAL_PCS,
                //                  UOMCD = z.Key.UOMCD,
                //                  UOMNM = z.Key.UOMNM,
                //                  STKTYPE = z.Key.STKTYPE,
                //                  STKTYPE_HIDDEN = z.Key.STKTYPE_HIDDEN,
                //                  FREESTK = z.Key.FREESTK,
                //                  FREESTK_HIDDEN = z.Key.FREESTK_HIDDEN,
                //                  RATE = z.Key.RATE,
                //                  ChildData = z.Key.ChildData,
                //                  AMT = z.Sum(x => x.AMT),
                //                  IGSTPER = z.Key.IGSTPER,
                //                  CGSTPER = z.Key.CGSTPER,
                //                  SGSTPER = z.Key.SGSTPER,
                //                  CESSPER = z.Key.CESSPER,
                //                  IGSTAMT = z.Sum(x => x.IGSTAMT),
                //                  CGSTAMT = z.Sum(x => x.CGSTAMT),
                //                  SGSTAMT = z.Sum(x => x.SGSTAMT),
                //                  CESSAMT = z.Sum(x => x.CESSAMT),
                //                  TDDISCTYPE = z.Key.TDDISCTYPE,
                //                  TDDISCRATE = z.Key.TDDISCRATE,
                //                  TDDISCAMT = z.Sum(x => x.TDDISCAMT),
                //                  DISCTYPE = z.Key.DISCTYPE,
                //                  DISCRATE = z.Key.DISCRATE,
                //                  DISCAMT = z.Sum(x => x.DISCAMT),
                //                  SCMDISCTYPE = z.Key.SCMDISCTYPE,
                //                  SCMDISCRATE = z.Key.SCMDISCRATE,
                //                  SCMDISCAMT = z.Sum(x => x.SCMDISCAMT),
                //                  QNTY = z.Sum(x => x.QNTY),
                //                  ALL_SIZE = string.Join(",", z.Select(i => i.SIZECD))
                //              }).OrderBy(a => a.ARTNO).ToList();

                //double IGST_PER = 0; double CGST_PER = 0; double SGST_PER = 0; double CESS_PER = 0; double DUTY_PER = 0; double TOTAL_BOXES = 0; double TOTAL_SETS = 0;
                //for (int x = 0; x <= VE.TTXNDTL.Count() - 1; x++)
                //{
                //    VE.TTXNDTL[x].DropDown_list2 = Master_Help.STOCK_TYPE(); VE.TTXNDTL[x].DropDown_list3 = Master_Help.FREE_STOCK();
                //    string ITEM = VE.TTXNDTL[x].ITCD;
                //    VE.TTXNDTL[x].HSNCODE = (from z in DB.M_SITEM where z.ITCD == ITEM select z.HSNSACCD).SingleOrDefault();
                //    double IGST = VE.TTXNDTL[x].IGSTPER == null ? 0 : VE.TTXNDTL[x].IGSTPER.Value;
                //    double CGST = VE.TTXNDTL[x].CGSTPER == null ? 0 : VE.TTXNDTL[x].CGSTPER.Value;
                //    double SGST = VE.TTXNDTL[x].SGSTPER == null ? 0 : VE.TTXNDTL[x].SGSTPER.Value;
                //    double CESS = VE.TTXNDTL[x].CESSPER == null ? 0 : VE.TTXNDTL[x].CESSPER.Value;
                //    double DUTY = VE.TTXNDTL[x].DUTYPER == null ? 0 : VE.TTXNDTL[x].DUTYPER.Value;
                //    if (IGST > IGST_PER) { IGST_PER = IGST; }
                //    if (CGST > CGST_PER) { CGST_PER = CGST; }
                //    if (SGST > SGST_PER) { SGST_PER = SGST; }
                //    if (CESS > CESS_PER) { CESS_PER = CESS; }
                //    if (DUTY > DUTY_PER) { DUTY_PER = DUTY; }

                //    VE.TTXNDTL[x].GROSS = VE.TTXNDTL[x].AMT - ((VE.TTXNDTL[x].SCMDISCAMT) + (VE.TTXNDTL[x].DISCAMT) + (VE.TTXNDTL[x].TDDISCAMT));

                //    VE.TTXNDTL[x].NETAMT = (VE.TTXNDTL[x].GROSS == null ? 0 : VE.TTXNDTL[x].GROSS.Value) +
                //                           (VE.TTXNDTL[x].IGSTAMT == null ? 0 : VE.TTXNDTL[x].IGSTAMT.Value) +
                //                           (VE.TTXNDTL[x].CGSTAMT == null ? 0 : VE.TTXNDTL[x].CGSTAMT.Value) +
                //                           (VE.TTXNDTL[x].SGSTAMT == null ? 0 : VE.TTXNDTL[x].SGSTAMT.Value) +
                //                           (VE.TTXNDTL[x].CESSAMT == null ? 0 : VE.TTXNDTL[x].CESSAMT.Value);

                //    VE.TTXNDTL[x].NETAMT_DSP = VE.TTXNDTL[x].NETAMT == null ? "0.00" : VE.TTXNDTL[x].NETAMT.Value.ToINRFormat();

                //    TOTAL_QNTY = TOTAL_QNTY + (VE.TTXNDTL[x].QNTY == null ? 0 : VE.TTXNDTL[x].QNTY.Value);
                //    TOTAL_AMT = TOTAL_AMT + VE.TTXNDTL[x].AMT;
                //    TOTAL_TDS_AMT = TOTAL_TDS_AMT + VE.TTXNDTL[x].TDDISCAMT;
                //    TOTAL_DISC_AMT = TOTAL_DISC_AMT + VE.TTXNDTL[x].DISCAMT;
                //    TOTAL_SCM_AMT = TOTAL_SCM_AMT + VE.TTXNDTL[x].SCMDISCAMT;
                //    TOTAL_GROSS = TOTAL_GROSS + (VE.TTXNDTL[x].GROSS == null ? 0 : VE.TTXNDTL[x].GROSS.Value);
                //    TOTAL_IGST_AMT = TOTAL_IGST_AMT + (VE.TTXNDTL[x].IGSTAMT == null ? 0 : VE.TTXNDTL[x].IGSTAMT.Value);
                //    TOTAL_CGST_AMT = TOTAL_CGST_AMT + (VE.TTXNDTL[x].CGSTAMT == null ? 0 : VE.TTXNDTL[x].CGSTAMT.Value);
                //    TOTAL_SGST_AMT = TOTAL_SGST_AMT + (VE.TTXNDTL[x].SGSTAMT == null ? 0 : VE.TTXNDTL[x].SGSTAMT.Value);
                //    TOTAL_CESS_AMT = TOTAL_CESS_AMT + (VE.TTXNDTL[x].CESSAMT == null ? 0 : VE.TTXNDTL[x].CESSAMT.Value);
                //    TOTAL_DUTY_AMT = TOTAL_DUTY_AMT + (VE.TTXNDTL[x].DUTYAMT == null ? 0 : VE.TTXNDTL[x].DUTYAMT.Value);
                //    TOTAL_NET_AMT = TOTAL_NET_AMT + (VE.TTXNDTL[x].NETAMT == null ? 0 : VE.TTXNDTL[x].NETAMT.Value);
                //    TOTAL_OTHER_AMT = TOTAL_OTHER_AMT + (VE.TTXNDTL[x].OTHRAMT == null ? 0 : VE.TTXNDTL[x].OTHRAMT.Value);

                //    TOTAL_BOXES = TOTAL_BOXES + salesfunc.ConvPcstoBox((VE.TTXNDTL[x].QNTY == null ? 0 : VE.TTXNDTL[x].QNTY.Value), (VE.TTXNDTL[x].TOTAL_PCS == null ? 0 : VE.TTXNDTL[x].TOTAL_PCS.Value));
                //    TOTAL_SETS = TOTAL_SETS + salesfunc.ConvPcstoSet((VE.TTXNDTL[x].QNTY == null ? 0 : VE.TTXNDTL[x].QNTY.Value), (VE.TTXNDTL[x].PCSPERSET == null ? 0 : VE.TTXNDTL[x].PCSPERSET.Value));

                //}
                //short SL_NO = 0;
                //VE.TTXNDTL.Where(a => a != null && VE.TTXNDTL.Count > 0).ForEach(a => { a.DropDown_list2 = Master_Help.STOCK_TYPE(); a.DropDown_list3 = Master_Help.FREE_STOCK(); a.DISC_TYPE = Master_Help.DISCOUNT_TYPE(); a.SLNO = ++SL_NO; });

                //string S_P = ""; if (VE.MENU_PARA == "SB" || VE.MENU_PARA == "SBEXP" || VE.MENU_PARA == "SOTH" || VE.MENU_PARA == "SR") { S_P = "S"; }

                //string A_QUERY = "select a.amtcd, b.amtnm, b.calccode, b.addless, b.CALCTYPE,b.calcformula, a.amtdesc, b.glcd, b.hsncode, a.amtrate, a.curr_amt, a.amt,a.igstper, a.igstamt, a.sgstper, a.sgstamt, a.cgstper, a.cgstamt,a.CESSPER,";
                //A_QUERY = A_QUERY + " a.CESSAMT, a.dutyper, a.dutyamt from " + DATABASE + ".t_txnamt a, " + DATABASE + ".m_amttype b where a.amtcd=b.amtcd(+) and b.salpur='" + S_P + "' and a.autono='" + TXN.AUTONO + "' ";
                //A_QUERY = A_QUERY + " union select b.amtcd, b.amtnm, b.calccode, b.addless,b.CALCTYPE, b.calcformula, '' amtdesc, b.glcd, b.hsncode, 0 amtrate, 0 curr_amt, 0 amt,0 igstper, 0 igstamt, 0 sgstper, 0 sgstamt, 0 cgstper, 0 cgstamt, ";
                //A_QUERY = A_QUERY + " 0 CESSPER, 0 CESSAMT, 0 dutyper, 0 dutyamt from " + DATABASE + ".m_amttype b, " + DATABASE + ".m_cntrl_hdr c where b.m_autono=c.m_autono(+) and b.salpur='" + S_P + "'  and nvl(c.inactive_tag,'N') = 'N' ";
                //A_QUERY = A_QUERY + "and b.amtcd not in (select amtcd from " + DATABASE + ".t_txnamt where autono='" + TXN.AUTONO + "')";

                //var AMOUNT_DATA = Master_Help.SQLquery(A_QUERY);

                //VE.TTXNAMT = (from DataRow dr in AMOUNT_DATA.Rows
                //              select new TTXNAMT()
                //              {
                //                  AMTCD = dr["amtcd"].ToString(),
                //                  ADDLESS = dr["addless"].ToString(),
                //                  GLCD = dr["GLCD"].ToString(),
                //                  AMTNM = dr["amtnm"].ToString(),
                //                  CALCCODE = dr["calccode"].ToString(),
                //                  CALCTYPE = dr["CALCTYPE"].ToString(),
                //                  CALCFORMULA = dr["calcformula"].ToString(),
                //                  AMTDESC = dr["amtdesc"].ToString(),
                //                  HSNCODE = dr["hsncode"].ToString(),
                //                  AMTRATE = Convert.ToDouble(dr["amtrate"] == DBNull.Value ? null : dr["amtrate"].ToString()),
                //                  CURR_AMT = Convert.ToDouble(dr["curr_amt"] == DBNull.Value ? null : dr["curr_amt"].ToString()),
                //                  AMT = Convert.ToDouble(dr["amt"] == DBNull.Value ? null : dr["amt"].ToString()),
                //                  IGSTPER = Convert.ToDouble(dr["igstper"] == DBNull.Value ? null : dr["igstper"].ToString()),
                //                  CGSTPER = Convert.ToDouble(dr["cgstper"] == DBNull.Value ? null : dr["cgstper"].ToString()),
                //                  SGSTPER = Convert.ToDouble(dr["sgstper"] == DBNull.Value ? null : dr["sgstper"].ToString()),
                //                  CESSPER = Convert.ToDouble(dr["CESSPER"] == DBNull.Value ? null : dr["CESSPER"].ToString()),
                //                  DUTYPER = Convert.ToDouble(dr["dutyper"] == DBNull.Value ? null : dr["dutyper"].ToString()),
                //                  IGSTAMT = Convert.ToDouble(dr["igstamt"] == DBNull.Value ? null : dr["igstamt"].ToString()),
                //                  CGSTAMT = Convert.ToDouble(dr["cgstamt"] == DBNull.Value ? null : dr["cgstamt"].ToString()),
                //                  SGSTAMT = Convert.ToDouble(dr["sgstamt"] == DBNull.Value ? null : dr["sgstamt"].ToString()),
                //                  CESSAMT = Convert.ToDouble(dr["CESSAMT"] == DBNull.Value ? null : dr["CESSAMT"].ToString()),
                //                  DUTYAMT = Convert.ToDouble(dr["dutyamt"] == DBNull.Value ? null : dr["dutyamt"].ToString()),
                //              }).ToList();
                //double A_IGST_AMT = 0; double A_CGST_AMT = 0; double A_SGST_AMT = 0; double A_CESS_AMT = 0; double A_DUTY_AMT = 0; double A_TOTAL_CURR = 0; double A_TOTAL_AMOUNT = 0; double A_TOTAL_NETAMOUNT = 0;
                //for (int p = 0; p <= VE.TTXNAMT.Count - 1; p++)
                //{
                //    VE.TTXNAMT[p].SLNO = Convert.ToInt16(p + 1);

                //    if (VE.TTXNAMT[p].IGSTPER == 0) { VE.TTXNAMT[p].IGSTPER = IGST_PER; }
                //    if (VE.TTXNAMT[p].CGSTPER == 0) { VE.TTXNAMT[p].CGSTPER = CGST_PER; }
                //    if (VE.TTXNAMT[p].SGSTPER == 0) { VE.TTXNAMT[p].SGSTPER = SGST_PER; }
                //    if (VE.TTXNAMT[p].CESSPER == 0) { VE.TTXNAMT[p].CESSPER = CESS_PER; }
                //    if (VE.TTXNAMT[p].DUTYPER == 0) { VE.TTXNAMT[p].DUTYPER = DUTY_PER; }

                //    A_TOTAL_CURR = A_TOTAL_CURR + VE.TTXNAMT[p].CURR_AMT.Value;
                //    A_TOTAL_AMOUNT = A_TOTAL_AMOUNT + VE.TTXNAMT[p].AMT.Value;
                //    A_IGST_AMT = A_IGST_AMT + VE.TTXNAMT[p].IGSTAMT.Value;
                //    A_CGST_AMT = A_CGST_AMT + VE.TTXNAMT[p].CGSTAMT.Value;
                //    A_SGST_AMT = A_SGST_AMT + VE.TTXNAMT[p].SGSTAMT.Value;
                //    A_CESS_AMT = A_CESS_AMT + VE.TTXNAMT[p].CESSAMT.Value;
                //    A_DUTY_AMT = A_DUTY_AMT + VE.TTXNAMT[p].DUTYAMT.Value;
                //    VE.TTXNAMT[p].NETAMT = VE.TTXNAMT[p].AMT.Value + VE.TTXNAMT[p].IGSTAMT.Value + VE.TTXNAMT[p].CGSTAMT.Value + VE.TTXNAMT[p].SGSTAMT.Value + VE.TTXNAMT[p].CESSAMT.Value + VE.TTXNAMT[p].DUTYAMT.Value;
                //    A_TOTAL_NETAMOUNT = A_TOTAL_NETAMOUNT + VE.TTXNAMT[p].NETAMT.Value;
                //}
                //VE.T_QNTY = TOTAL_QNTY;
                //VE.T_AMT = TOTAL_AMT;
                //VE.T_SCM_AMT = TOTAL_SCM_AMT;
                //VE.T_DISC_AMT = TOTAL_DISC_AMT;
                //VE.T_TDS_AMT = TOTAL_TDS_AMT;
                //VE.T_GROSS_AMT = TOTAL_GROSS;
                //VE.T_IGST_AMT = TOTAL_IGST_AMT;
                //VE.T_CGST_AMT = TOTAL_CGST_AMT;
                //VE.T_SGST_AMT = TOTAL_SGST_AMT;
                //VE.T_CESS_AMT = TOTAL_CESS_AMT;
                //VE.T_DUTY_AMT = TOTAL_DUTY_AMT;
                //VE.T_OTHER_AMT = TOTAL_OTHER_AMT;
                //VE.T_NET_AMT = TOTAL_NET_AMT;
                //VE.T_NET_AMT_DISP = VE.T_NET_AMT == null ? "0.00" : VE.T_NET_AMT.Value.ToINRFormat();
                //VE.P_QNTY = TOTAL_QNTY;
                //VE.TOTAL_BOXES = TOTAL_BOXES;
                //VE.TOTAL_SETS = TOTAL_SETS;

                //VE.A_T_CURR = A_TOTAL_CURR;
                //VE.A_T_AMOUNT = A_TOTAL_AMOUNT;
                //VE.A_T_NET = A_TOTAL_NETAMOUNT;
                //VE.A_T_IGST = A_IGST_AMT;
                //VE.A_T_CGST = A_CGST_AMT;
                //VE.A_T_SGST = A_SGST_AMT;
                //VE.A_T_CESS = A_CESS_AMT;
                //VE.A_T_DUTY = A_DUTY_AMT;
                //VE.TOTAL_TAX = TOTAL_IGST_AMT + TOTAL_CGST_AMT + TOTAL_SGST_AMT + A_IGST_AMT + A_CGST_AMT + A_SGST_AMT;
            }
            //Cn.DateLock_Entry(VE, DB, TCH.DOCDT.Value);
            if (TCH.CANCEL == "Y") VE.CancelRecord = true; else VE.CancelRecord = false;
            return VE;
        }
        //public ActionResult SearchPannelData(TransactionPackingSlipEntry VE, string SRC_SLCD, string SRC_DOCNO, string SRC_FDT, string SRC_TDT)
        //{
        //    string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), yrcd = CommVar.YearCode(UNQSNO);
        //    Cn.getQueryString(VE);

        //    string MNUP = VE.MENU_PARA;
        //    string[] XYZ = new string[200];
        //    switch (MNUP)
        //    {
        //        case "SB": XYZ = (string[])TempData["SB"]; break;
        //        case "SR": XYZ = (string[])TempData["SR"]; break;
        //        case "SOTH": XYZ = (string[])TempData["SOTH"]; break;
        //        case "SBEXP": XYZ = (string[])TempData["SBEXP"]; break;
        //    }
        //    TempData.Keep();

        //    string doccd = XYZ.retSqlfromStrarray();
        //    string sql = "";

        //    sql = "select a.autono, b.docno, to_char(b.docdt,'dd/mm/yyyy') docdt, b.doccd, a.slcd, c.slnm, c.district, nvl(a.blamt,0) blamt ";
        //    sql += "from " + scm + ".t_txn a, " + scm + ".t_cntrl_hdr b, " + scmf + ".m_subleg c ";
        //    sql += "where a.autono=b.autono and a.slcd=c.slcd(+) and b.doccd in (" + doccd + ") and ";
        //    if (SRC_FDT.retStr() != "") sql += "b.docdt >= to_date('" + SRC_FDT.retDateStr() + "','dd/mm/yyyy') and ";
        //    if (SRC_TDT.retStr() != "") sql += "b.docdt <= to_date('" + SRC_TDT.retDateStr() + "','dd/mm/yyyy') and ";
        //    if (SRC_DOCNO.retStr() != "") sql += "(b.vchrno like '%" + SRC_DOCNO.retStr() + "%' or b.docno like '%" + SRC_DOCNO.retStr() + "%') and ";
        //    if (SRC_SLCD.retStr() != "") sql += "(a.slcd like '%" + SRC_SLCD.retStr() + "%' or upper(c.slnm) like '%" + SRC_SLCD.retStr().ToUpper() + "%') and ";
        //    sql += "b.loccd='" + LOC + "' and b.compcd='" + COM + "' and b.yr_cd='" + yrcd + "' ";
        //    sql += "order by docdt, docno ";
        //    DataTable tbl = Master_Help.SQLquery(sql);

        //    System.Text.StringBuilder SB = new System.Text.StringBuilder();
        //    var hdr = "Document Number" + Cn.GCS() + "Document Date" + Cn.GCS() + "Party Name" + Cn.GCS() + "Bill Amt" + Cn.GCS() + "AUTO NO";
        //    for (int j = 0; j <= tbl.Rows.Count - 1; j++)
        //    {
        //        SB.Append("<tr><td><b>" + tbl.Rows[j]["docno"] + "</b> [" + tbl.Rows[j]["doccd"] + "]" + " </td><td>" + tbl.Rows[j]["docdt"] + " </td><td><b>" + tbl.Rows[j]["slnm"] + "</b> [" + tbl.Rows[j]["district"] + "] (" + tbl.Rows[j]["slcd"] + ") </td><td class='text-right'>" + Convert.ToDouble(tbl.Rows[j]["blamt"]).ToINRFormat() + " </td><td>" + tbl.Rows[j]["autono"] + " </td></tr>");
        //    }
        //    return PartialView("_SearchPannel2", Master_Help.Generate_SearchPannel(hdr, SB.ToString(), "4", "4"));
        //}
        public ActionResult GetSubLedgerDetails(string val, string Code)
        {
            try
            {
                var agent = Code.Split(Convert.ToChar(Cn.GCS()));
                if (agent.Count() > 1)
                {
                    if (agent[1] == "")
                    {
                        return Content("Please Select Agent !!");
                    }
                    else
                    {
                        Code = agent[0];
                    }
                }
                var str = Master_Help.SLCD_help(val, Code);
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
                var str = Master_Help.ITCD_help(val,"", Code);
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
        public ActionResult GetBarCodeData(string val)
        {
            TransactionPackingSlipEntry VE = new TransactionPackingSlipEntry();
            Cn.getQueryString(VE);
            string scm = CommVar.FinSchema(UNQSNO);
            string sql = "";
            sql += "select a.PRCCD,a.PRCNM ";
            sql += "from " + scm + ".M_PRCLST a, " + scm + ".M_CNTRL_HDR b ";
            sql += "where a.M_AUTONO=b.M_AUTONO(+) and b.INACTIVE_TAG = 'N' ";
            sql += "order by a.PRCCD,a.PRCNM";
            DataTable tbl = Master_Help.SQLquery(sql);

            VE.DefaultView = true;
            ModelState.Clear();
            return PartialView("_T_SALE_PRODUCT", VE);

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

                var AMOUNT_DATA = Master_Help.SQLquery(QUERY);

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
        //public ActionResult AddRow(TransactionPackingSlipEntry VE, int COUNT, string TAG)
        //{
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());

        //    if (VE.TTXNDTL == null)
        //    {
        //        List<TTXNDTL> TXNDTL_HEAD = new List<TTXNDTL>();
        //        TTXNDTL DTL = new TTXNDTL();
        //        int maxr = 1;
        //        if (TAG == "Y" && COUNT > 0) maxr = COUNT;
        //        for (Int16 q = 1; q <= maxr; q++)
        //        {
        //            DTL.SLNO = q;
        //            TXNDTL_HEAD.Add(DTL);
        //        }
        //        VE.TTXNDTL = TXNDTL_HEAD;

        //        DTL.SLNO = 1;
        //        TXNDTL_HEAD.Add(DTL);
        //        VE.TTXNDTL = TXNDTL_HEAD;
        //    }
        //    else
        //    {
        //        List<TTXNDTL> TXNDTL = new List<TTXNDTL>();
        //        for (int i = 0; i <= VE.TTXNDTL.Count - 1; i++)
        //        {
        //            TTXNDTL MIB = new TTXNDTL();
        //            MIB = VE.TTXNDTL[i];
        //            TXNDTL.Add(MIB);
        //        }
        //        TTXNDTL TXNDTL1 = new TTXNDTL();
        //        var max = VE.TTXNDTL.Max(a => Convert.ToInt32(a.SLNO));
        //        int SLNO = Convert.ToInt32(max) + 1;

        //        int maxr = 1;
        //        if (TAG == "Y" && COUNT > 0) maxr = COUNT;
        //        for (Int16 q = 1; q <= maxr; q++)
        //        {
        //            max = max + 1;
        //            TTXNDTL TXNDTL2 = new TTXNDTL();
        //            TXNDTL2.SLNO = max;
        //            TXNDTL.Add(TXNDTL2);
        //        }
        //        VE.TTXNDTL = TXNDTL;
        //    }
        //    VE.TTXNDTL.ForEach(a => { a.DropDown_list2 = Master_Help.STOCK_TYPE(); a.DropDown_list3 = Master_Help.FREE_STOCK(); a.DISC_TYPE = Master_Help.DISCOUNT_TYPE(); });
        //    VE.DefaultView = true;
        //    return PartialView("_T_SALE_PRODUCT", VE);

        //}
        //public ActionResult DeleteRow(TransactionPackingSlipEntry VE)
        //{
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
        //    short SL_NO = 0; double TOTAL_BOXES = 0; double TOTAL_SETS = 0;
        //    VE.TTXNDTL = (from x in VE.TTXNDTL where x.Checked == false select x).ToList();
        //    VE.TTXNDTL.ForEach(a =>
        //    {
        //        a.DropDown_list2 = Master_Help.STOCK_TYPE();
        //        a.DropDown_list3 = Master_Help.FREE_STOCK(); a.DISC_TYPE = Master_Help.DISCOUNT_TYPE(); a.SLNO = ++SL_NO;
        //        TOTAL_BOXES = TOTAL_BOXES + salesfunc.ConvPcstoBox((a.QNTY == null ? 0 : a.QNTY.Value), (a.TOTAL_PCS == null ? 0 : a.TOTAL_PCS.Value));
        //        TOTAL_SETS = TOTAL_SETS + salesfunc.ConvPcstoSet((a.QNTY == null ? 0 : a.QNTY.Value), (a.PCSPERSET == null ? 0 : a.PCSPERSET.Value));
        //    });
        //    VE.TOTAL_BOXES = TOTAL_BOXES;
        //    VE.TOTAL_SETS = TOTAL_SETS;
        //    ModelState.Clear();
        //    VE.DefaultView = true;
        //    return PartialView("_T_SALE_PRODUCT", VE);
        //}
        //public ActionResult Print(TransactionPackingSlipEntry TSP, string DOCNO, string DOC_CD)
        //{
        //    ReportViewinHtml ind = new ReportViewinHtml();
        //    ind.DOCCD = DOC_CD;
        //    ind.FDOCNO = DOCNO;
        //    ind.TDOCNO = DOCNO;
        //    ind.FDT = TSP.T_TXN.DOCDT.ToString().Substring(0, 10);
        //    ind.TDT = TSP.T_TXN.DOCDT.ToString().Substring(0, 10);
        //    ind.TEXTBOX2 = TSP.T_TXN.SLCD;
        //    ind.TEXTBOX3 = TSP.BuyerName;
        //    ind.MENU_PARA = TSP.MENU_PARA;
        //    if (TempData["printparameter"] != null)
        //    {
        //        TempData.Remove("printparameter");
        //    }
        //    TempData["printparameter"] = ind;
        //    return Content("");
        //}
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
                string url = Master_Help.RetriveParkFromFile(value, Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString(), "Improvar.ViewModels.TransactionPackingSlipEntry");
                return url;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        //public ActionResult SAVE(FormCollection FC, TransactionPackingSlipEntry VE)
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
        //    //
        //    DB.Configuration.ValidateOnSaveEnabled = false;
        //    using (var transaction = DB.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            //DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".T_CNTRL_HDR in  row share mode");
        //            OraCmd.CommandText = "lock table " + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode"; OraCmd.ExecuteNonQuery();
        //            String query = "";
        //            string dr = ""; string cr = ""; int isl = 0; string strrem = "";
        //            double igst = 0; double cgst = 0; double sgst = 0; double cess = 0; double duty = 0; double dbqty = 0; double dbamt = 0; double dbcurramt = 0;
        //            Int32 z = 0; Int32 maxR = 0;
        //            string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);

        //            string fdt = VE.T_TXN.DOCDT.ToString().retDateStr();
        //            string docpassrem = "";
        //            string ContentFlg = "";
        //            string[] cc = FC.Get("vchrauthremarks").Split(Convert.ToChar(Cn.GCS()));
        //            bool blactpost = true;

        //            if (VE.MENU_PARA == "OP" || VE.MENU_PARA == "PI") blactpost = false;

        //            DataTable tbl; DataTable rstmp;

        //            string trcd = "TR";
        //            switch (VE.MENU_PARA)
        //            {
        //                case "PB":
        //                    strrem = "Purchase Bill"; trcd = "PB"; break;
        //                case "SB":
        //                    strrem = "Sales Bill"; trcd = "SB"; break;
        //                case "SOTH":
        //                    strrem = "Sales Bill"; trcd = "SB"; break;
        //                case "SR":
        //                    strrem = "Sales Return"; trcd = "SC"; break;
        //                case "PR":
        //                    strrem = "Purchases Return"; trcd = "PD"; break;
        //                case "SBEXP":
        //                    strrem = "Export Sale Bill"; trcd = "SE"; break;
        //                default: break;
        //            }
        //            strrem = "Thru " + strrem;
        //            if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
        //            {
        //                T_TXN TTXN = new T_TXN();
        //                T_TXNTRANS TXNTRANS = new T_TXNTRANS();
        //                T_TXNOTH TTXNOTH = new T_TXNOTH();
        //                T_CNTRL_HDR TCH = new T_CNTRL_HDR();
        //                //T_VCH_GST TVCHGST = new T_VCH_GST();
        //                //T_CNTRL_DOC_PASS TCDP = new T_CNTRL_DOC_PASS();
        //                string DOCPATTERN = "";
        //                TTXN.DOCDT = VE.T_TXN.DOCDT;
        //                string Ddate = Convert.ToString(TTXN.DOCDT);
        //                TTXN.CLCD = CommVar.ClientCode(UNQSNO);
        //                string auto_no = ""; string Month = "";
        //                if (VE.DefaultAction == "A")
        //                {
        //                    TTXN.EMD_NO = 0;
        //                    TTXN.DOCCD = VE.DOC_ID;
        //                    if (VE.M_SLIP_NO != null && VE.M_SLIP_NO != 0)
        //                    {
        //                        TTXN.DOCNO = Convert.ToString(VE.M_SLIP_NO).PadLeft(6, '0');
        //                    }
        //                    else
        //                    {
        //                        TTXN.DOCNO = Cn.MaxDocNumber(TTXN.DOCCD, Ddate);
        //                    }
        //                    //TTXN.DOCNO = Cn.MaxDocNumber(TTXN.DOCCD, Ddate);
        //                    DOCPATTERN = Cn.DocPattern(Convert.ToInt32(TTXN.DOCNO), TTXN.DOCCD, CommVar.CurSchema(UNQSNO).ToString(), CommVar.FinSchema(UNQSNO), Ddate);
        //                    auto_no = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), TTXN.DOCNO, TTXN.DOCCD, Ddate);
        //                    TTXN.AUTONO = auto_no.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
        //                    Month = auto_no.Split(Convert.ToChar(Cn.GCS()))[1].ToString();
        //                }
        //                else
        //                {
        //                    TTXN.DOCCD = VE.T_TXN.DOCCD;
        //                    TTXN.DOCCD = VE.DOC_ID;
        //                    TTXN.DOCNO = VE.T_TXN.DOCNO;
        //                    TTXN.AUTONO = VE.T_TXN.AUTONO;
        //                    Month = VE.T_CNTRL_HDR.MNTHCD;
        //                    TTXN.EMD_NO = Convert.ToByte((VE.T_CNTRL_HDR.EMD_NO == null ? 0 : VE.T_CNTRL_HDR.EMD_NO) + 1);
        //                    DOCPATTERN = VE.T_CNTRL_HDR.DOCNO;
        //                }
        //                if (VE.MENU_PARA == "SOTH" || VE.MENU_PARA == "SBEXP") TTXN.DOCTAG = "SB"; else TTXN.DOCTAG = VE.MENU_PARA;
        //                TTXN.SLCD = VE.T_TXN.SLCD;
        //                TTXN.CONSLCD = VE.T_TXN.CONSLCD;
        //                TTXN.CURR_CD = VE.T_TXN.CURR_CD;
        //                TTXN.CURRRT = VE.T_TXN.CURRRT;
        //                TTXN.APPROXVAL = VE.T_TXN.APPROXVAL;
        //                TTXN.BLAMT = VE.T_TXN.BLAMT;
        //                TTXN.PREFDT = VE.T_TXN.PREFDT;
        //                TTXN.PREFNO = VE.T_TXN.PREFNO;
        //                TTXN.REVCHRG = VE.T_TXN.REVCHRG;
        //                TTXN.ROAMT = VE.T_TXN.ROAMT;
        //                if (VE.RoundOff == true) { TTXN.ROYN = "Y"; } else { TTXN.ROYN = "N"; }
        //                TTXN.CUTRECDOCAUTONO = VE.T_TXN.CUTRECDOCAUTONO;
        //                TTXN.FAAUTONO = VE.T_TXN.FAAUTONO;
        //                TTXN.GOCD = VE.T_TXN.GOCD;
        //                TTXN.JOBCD = VE.T_TXN.JOBCD;
        //                TTXN.MANSLIPNO = VE.T_TXN.MANSLIPNO;
        //                TTXN.PARGLCD = VE.T_TXN.PARGLCD;
        //                TTXN.GLCD = VE.T_TXN.GLCD;
        //                TTXN.CLASS1CD = VE.T_TXN.CLASS1CD;
        //                TTXN.CLASS2CD = VE.T_TXN.CLASS2CD;
        //                TTXN.TCSPER = VE.T_TXN.TCSPER;
        //                TTXN.TCSAMT = VE.T_TXN.TCSAMT;
        //                TTXN.DTAG = (VE.DefaultAction == "E" ? "E" : null);
        //                if (TTXN.PARGLCD == null || TTXN.GLCD == null)
        //                {
        //                    transaction.Rollback();
        //                    OraTrans.Rollback();
        //                    OraCon.Dispose();
        //                    return Content("Entry Can't Save ! Please Select Party General Ledger and Sales General Ledger in Finance Code Tab, Then Procced !!");
        //                }
        //                query = "";
        //                query = query + "select '" + TTXN.PARGLCD + "' parglcd, '" + TTXN.GLCD + "' prodglcd, b.rogl, '" + TTXN.CLASS1CD + "' class1cd, '" + TTXN.CLASS2CD + "' class2cd, b.tcsgl, nvl(c.crlimit,0) crlimit, nvl(c.crdays,0) crdays, ";
        //                if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "PR") query = query + "b.igst_p igstcd, b.cgst_p cgstcd, b.sgst_p sgstcd, b.cess_p cesscd, b.duty_p dutycd ";
        //                else query = query + "b.igst_s igstcd, b.cgst_s cgstcd, b.sgst_s sgstcd, b.cess_s cesscd, b.duty_s dutycd ";
        //                query = query + "from " + scmf + ".m_post b, " + scm1 + ".m_subleg_com c ";
        //                query = query + "where c.slcd in ('" + VE.T_TXN.SLCD + "',null) and ";
        //                query = query + "c.compcd in ('" + COM + "',null) ";
        //                tbl = Master_Help.SQLquery(query);

        //                if (VE.DefaultAction == "E")
        //                {
        //                    dbsql = MasterHelpFa.TblUpdt("t_txndtl", TTXN.AUTONO, "E");
        //                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                    dbsql = MasterHelpFa.TblUpdt("t_txn_linkno", TTXN.AUTONO, "E");
        //                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                    dbsql = MasterHelpFa.TblUpdt("t_txntrans", TTXN.AUTONO, "E");
        //                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                    dbsql = MasterHelpFa.TblUpdt("t_txnoth", TTXN.AUTONO, "E");
        //                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                    //dbsql = MasterHelpFa.TblUpdt("t_vch_gst", TTXN.AUTONO, "E");
        //                    //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                    dbsql = MasterHelpFa.TblUpdt("t_txnamt", TTXN.AUTONO, "E");
        //                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                    dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_rem", TTXN.AUTONO, "E");
        //                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                    dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_doc_dtl", TTXN.AUTONO, "E");
        //                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                    dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_doc", TTXN.AUTONO, "E");
        //                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                    dbsql = MasterHelpFa.TblUpdt("t_cntrl_doc_pass", TTXN.AUTONO, "E");
        //                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

        //                    dbsql = MasterHelpFa.TblUpdt("t_tdstxn", TTXN.AUTONO, "E", "F");
        //                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                    dbsql = MasterHelpFa.TblUpdt("t_vch_gst", TTXN.AUTONO, "E", "F");
        //                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                    dbsql = MasterHelpFa.TblUpdt("t_vch_bl", TTXN.AUTONO, "E", "F");
        //                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                    dbsql = MasterHelpFa.TblUpdt("t_vch_class", TTXN.AUTONO, "E", "F");
        //                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                    dbsql = MasterHelpFa.TblUpdt("t_vch_det", TTXN.AUTONO, "E", "F");
        //                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                    dbsql = MasterHelpFa.TblUpdt("t_vch_hdr", TTXN.AUTONO, "E", "F");
        //                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                }
        //                //if (docpassrem != "")
        //                //{
        //                //    docpassrem = "Doc # " + TTXN.DOCNO.ToString() + " Party Name # " + VE.BuyerName.ToString() + "~" + docpassrem;
        //                //    TCDP.CLCD = TTXN.CLCD; TCDP.DTAG = TTXN.DTAG; TCDP.EMD_NO = TTXN.EMD_NO.Value; TCDP.AUTONO = TTXN.AUTONO; TCDP.SLNO = 1; TCDP.PASS_LEVEL = 1; TCDP.AUTHCD = null; TCDP.AUTHREM = docpassrem;
        //                //}
        //                //-------------------------Transport--------------------------//
        //                TXNTRANS.AUTONO = TTXN.AUTONO;
        //                TXNTRANS.EMD_NO = TTXN.EMD_NO;
        //                TXNTRANS.CLCD = TTXN.CLCD;
        //                TXNTRANS.DTAG = TTXN.DTAG;
        //                TXNTRANS.TRANSLCD = VE.T_TXNTRANS.TRANSLCD;
        //                TXNTRANS.CRSLCD = VE.T_TXNTRANS.CRSLCD;
        //                TXNTRANS.EWAYBILLNO = VE.T_TXNTRANS.EWAYBILLNO;
        //                TXNTRANS.LRNO = VE.T_TXNTRANS.LRNO;
        //                TXNTRANS.LRDT = VE.T_TXNTRANS.LRDT;
        //                TXNTRANS.LORRYNO = VE.T_TXNTRANS.LORRYNO;
        //                TXNTRANS.GRWT = VE.T_TXNTRANS.GRWT;
        //                TXNTRANS.TRWT = VE.T_TXNTRANS.TRWT;
        //                TXNTRANS.NTWT = VE.T_TXNTRANS.NTWT;
        //                //----------------------------------------------------------//
        //                //-------------------------Other Info--------------------------//
        //                TTXNOTH.AUTONO = TTXN.AUTONO;
        //                TTXNOTH.EMD_NO = TTXN.EMD_NO;
        //                TTXNOTH.CLCD = TTXN.CLCD;
        //                TTXNOTH.DTAG = TTXN.DTAG;
        //                TTXNOTH.DOCREM = VE.T_TXNOTH.DOCREM;
        //                TTXNOTH.DNCNCD = VE.T_TXNOTH.DNCNCD;
        //                TTXNOTH.AGSLCD = VE.T_TXNOTH.AGSLCD;
        //                TTXNOTH.CASENOS = VE.T_TXNOTH.CASENOS;
        //                TTXNOTH.NOOFCASES = VE.T_TXNOTH.NOOFCASES;
        //                TTXNOTH.PRCCD = VE.T_TXNOTH.PRCCD;
        //                //----------------------------------------------------------//
        //                //   //-----------------------------Export Info-----------------------------//
        //                //      TVCHGST.EXPCD = VE.T_VCH_GST.EXPCD;
        //                //      TVCHGST.SHIPDOCNO = VE.T_VCH_GST.SHIPDOCNO;
        //                //      TVCHGST.SHIPDOCDT = VE.T_VCH_GST.SHIPDOCDT;
        //                //      TVCHGST.INVTYPECD = VE.T_VCH_GST.INVTYPECD;
        //                //      TVCHGST.PORTCD = VE.T_VCH_GST.PORTCD;
        //                ////----------------------------------------------------------//
        //                if (VE.effectiveDate != null && VE.effectiveDate != "")
        //                {
        //                    TTXNOTH.PRCEFFDT = Cn.convstr2date(VE.effectiveDate);
        //                }
        //                TTXNOTH.COD = VE.T_TXNOTH.COD;
        //                TTXNOTH.DOCTH = VE.T_TXNOTH.DOCTH;
        //                TTXNOTH.POREFNO = VE.T_TXNOTH.POREFNO;
        //                TTXNOTH.POREFDT = VE.T_TXNOTH.POREFDT;
        //                //----------------------------------------------------------//
        //                dbsql = MasterHelpFa.T_Cntrl_Hdr_Updt_Ins(TTXN.AUTONO, VE.DefaultAction, "S", Month, TTXN.DOCCD, DOCPATTERN, TTXN.DOCDT.retStr(), TTXN.EMD_NO.retShort(), TTXN.DOCNO, Convert.ToDouble(TTXN.DOCNO), null, null, null, TTXN.SLCD);
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

        //                dbsql = MasterHelpFa.RetModeltoSql(TTXN, VE.DefaultAction);
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
        //                dbsql = MasterHelpFa.RetModeltoSql(TXNTRANS);
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
        //                dbsql = MasterHelpFa.RetModeltoSql(TTXNOTH);
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
        //                //dbsql = MasterHelpFa.RetModeltoSql(TVCHGST,"A",CommVar.FinSchema(UNQSNO));
        //                //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

        //                // create header record in finschema
        //                if (blactpost == true)
        //                {
        //                    dbsql = MasterHelpFa.Instcntrl_hdr(TTXN.AUTONO, VE.DefaultAction, "F", Month, TTXN.DOCCD, DOCPATTERN, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, (VE.DefaultAction == "A" ? null : "E"), TTXN.DOCNO, Convert.ToDouble(TTXN.DOCNO), "Y", null, null, TTXN.SLCD, TTXN.BLAMT.Value);
        //                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();

        //                    dbsql = MasterHelpFa.InsVch_Hdr(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, null, null, "Y", null, trcd);
        //                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
        //                }

        //                int COUNTER = 0;
        //                VE.TTXNDTL.ForEach(a => { a.STKTYPE = a.STKTYPE_HIDDEN; a.FREESTK = a.FREESTK_HIDDEN; });
        //                double itamt = 0;
        //                string stkdrcr = "C";
        //                switch (VE.MENU_PARA)
        //                {
        //                    case "SB": stkdrcr = "C"; break;
        //                    case "SBEXP": stkdrcr = "C"; break;
        //                    case "SR": stkdrcr = "D"; break;
        //                    case "SOTH": stkdrcr = "C"; break;
        //                    case "PI": stkdrcr = "N"; break;
        //                    case "PB": stkdrcr = "D"; break;
        //                }
        //                for (int i = 0; i <= VE.TTXNDTL.Count - 1; i++)
        //                {
        //                    if (VE.TTXNDTL[i].SLNO != 0 && VE.TTXNDTL[i].ITCD != null)
        //                    {
        //                        string[] SIZE_CODE = null;
        //                        if (VE.TTXNDTL[i].ALL_SIZE != null) SIZE_CODE = VE.TTXNDTL[i].ALL_SIZE.Split(',');
        //                        if (VE.TTXNDTL[i].ChildData != null)
        //                        {
        //                            string data = VE.TTXNDTL[i].ChildData;
        //                            var helpM = new List<Improvar.Models.TTXNDTL_SIZE>();
        //                            var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        //                            helpM = javaScriptSerializer.Deserialize<List<Improvar.Models.TTXNDTL_SIZE>>(data);
        //                            if (SIZE_CODE != null) helpM = helpM.Where(a => SIZE_CODE.Contains(a.SIZECD)).ToList();

        //                            var HELPM_DATA = (from Z in helpM where Z.SLNO != 0 && Z.RATE != null && Z.QNTY != null && Z.QNTY != 0 select Z).ToList();
        //                            int DATA_INDEX = HELPM_DATA.Count - 1;

        //                            double AMOUNT = 0; double GROSS = 0; double TDDISCAMT = 0; double DISCAMT = 0; double SCMDISCAMT = 0; double IGSTAMT = 0; double CGSTAMT = 0;
        //                            double SGSTAMT = 0; double CESSAMT = 0; //double DUTYAMT = 0;

        //                            for (int j = 0; j <= helpM.Count - 1; j++)
        //                            {
        //                                if (helpM[j].SLNO != 0 && helpM[j].RATE != null && helpM[j].QNTY != null && helpM[j].QNTY != 0)
        //                                {
        //                                    COUNTER = COUNTER + 1;
        //                                    T_TXNDTL TTXNDTL = new T_TXNDTL();
        //                                    TTXNDTL.CLCD = TTXN.CLCD;
        //                                    TTXNDTL.EMD_NO = TTXN.EMD_NO;
        //                                    TTXNDTL.DTAG = TTXN.DTAG;
        //                                    TTXNDTL.AUTONO = TTXN.AUTONO;
        //                                    TTXNDTL.SLNO = Convert.ToInt16(COUNTER);
        //                                    TTXNDTL.DOCCD = TTXN.DOCCD;
        //                                    TTXNDTL.DOCNO = TTXN.DOCNO;
        //                                    TTXNDTL.DOCDT = TTXN.DOCDT;
        //                                    TTXNDTL.ITCD = VE.TTXNDTL[i].ITCD;
        //                                    TTXNDTL.PARTCD = VE.TTXNDTL[i].PARTCD;
        //                                    TTXNDTL.COLRCD = helpM[j].COLRCD;
        //                                    TTXNDTL.SIZECD = helpM[j].SIZECD;
        //                                    TTXNDTL.STKDRCR = stkdrcr;
        //                                    TTXNDTL.STKTYPE = VE.TTXNDTL[i].STKTYPE;
        //                                    TTXNDTL.FREESTK = VE.TTXNDTL[i].FREESTK;
        //                                    TTXNDTL.BATCHNO = VE.TTXNDTL[i].BATCHNO;
        //                                    TTXNDTL.GOCD = VE.T_TXN.GOCD;
        //                                    TTXNDTL.ISSPROGAUTONO = VE.TTXNDTL[i].ISSPROGAUTONO;
        //                                    TTXNDTL.ISSPROGSLNO = VE.TTXNDTL[i].ISSPROGSLNO;
        //                                    TTXNDTL.ISSPROGLOTNO = VE.TTXNDTL[i].ISSPROGLOTNO;
        //                                    TTXNDTL.JOBCD = VE.TTXNDTL[i].JOBCD;
        //                                    TTXNDTL.NOS = VE.TTXNDTL[i].NOS == null ? 0 : VE.TTXNDTL[i].NOS;
        //                                    TTXNDTL.QNTY = helpM[j].QNTY;
        //                                    TTXNDTL.STKQNTY = VE.TTXNDTL[i].STKQNTY;
        //                                    TTXNDTL.QNTYIN = VE.TTXNDTL[i].QNTYIN;
        //                                    TTXNDTL.RATE = helpM[j].RATE;
        //                                    TTXNDTL.TDDISCTYPE = VE.TTXNDTL[i].TDDISCTYPE;
        //                                    TTXNDTL.TDDISCRATE = VE.TTXNDTL[i].TDDISCRATE;
        //                                    TTXNDTL.DISCTYPE = VE.TTXNDTL[i].DISCTYPE;
        //                                    TTXNDTL.DISCRATE = VE.TTXNDTL[i].DISCRATE;
        //                                    TTXNDTL.SCMDISCTYPE = VE.TTXNDTL[i].SCMDISCTYPE;
        //                                    TTXNDTL.SCMDISCRATE = VE.TTXNDTL[i].SCMDISCRATE;
        //                                    TTXNDTL.IGSTPER = VE.TTXNDTL[i].IGSTPER;
        //                                    TTXNDTL.CGSTPER = VE.TTXNDTL[i].CGSTPER;
        //                                    TTXNDTL.SGSTPER = VE.TTXNDTL[i].SGSTPER;
        //                                    TTXNDTL.CESSPER = VE.TTXNDTL[i].CESSPER;
        //                                    TTXNDTL.DUTYPER = VE.TTXNDTL[i].DUTYPER;
        //                                    if (j != DATA_INDEX)
        //                                    {
        //                                        TTXNDTL.AMT = helpM[j].QNTY.Value * helpM[j].RATE.Value; AMOUNT = AMOUNT + TTXNDTL.AMT;
        //                                        TTXNDTL.TDDISCAMT = (TTXNDTL.AMT * TTXNDTL.TDDISCRATE) / 100; TDDISCAMT = TDDISCAMT + TTXNDTL.TDDISCAMT;
        //                                        TTXNDTL.DISCAMT = (TTXNDTL.AMT * TTXNDTL.DISCRATE) / 100; DISCAMT = DISCAMT + TTXNDTL.DISCAMT;
        //                                        TTXNDTL.SCMDISCAMT = (TTXNDTL.AMT * TTXNDTL.SCMDISCRATE) / 100; SCMDISCAMT = SCMDISCAMT + TTXNDTL.SCMDISCAMT;
        //                                        var GROSS_AMT = TTXNDTL.AMT - (TTXNDTL.TDDISCAMT + TTXNDTL.DISCAMT + TTXNDTL.SCMDISCAMT); GROSS = GROSS + GROSS_AMT;
        //                                        TTXNDTL.IGSTAMT = (GROSS_AMT * TTXNDTL.IGSTPER) / 100; IGSTAMT = IGSTAMT + TTXNDTL.IGSTAMT.Value;
        //                                        TTXNDTL.CGSTAMT = (GROSS_AMT * TTXNDTL.CGSTPER) / 100; CGSTAMT = CGSTAMT + TTXNDTL.CGSTAMT.Value;
        //                                        TTXNDTL.SGSTAMT = (GROSS_AMT * TTXNDTL.SGSTPER) / 100; SGSTAMT = SGSTAMT + TTXNDTL.SGSTAMT.Value;
        //                                        TTXNDTL.CESSAMT = (GROSS_AMT * TTXNDTL.CESSPER) / 100; CESSAMT = CESSAMT + TTXNDTL.CESSAMT.Value;
        //                                        //TTXNDTL.DUTYAMT = (GROSS_AMT * TTXNDTL.DUTYPER) / 100; //DUTYAMT = DUTYAMT + TTXNDTL.DUTYAMT.Value;
        //                                    }
        //                                    else
        //                                    {
        //                                        TTXNDTL.AMT = VE.TTXNDTL[i].AMT - AMOUNT;
        //                                        TTXNDTL.TDDISCAMT = VE.TTXNDTL[i].TDDISCAMT - TDDISCAMT;
        //                                        TTXNDTL.DISCAMT = VE.TTXNDTL[i].DISCAMT - DISCAMT;
        //                                        TTXNDTL.SCMDISCAMT = VE.TTXNDTL[i].SCMDISCAMT - SCMDISCAMT;
        //                                        TTXNDTL.IGSTAMT = VE.TTXNDTL[i].IGSTAMT - IGSTAMT;
        //                                        TTXNDTL.CGSTAMT = VE.TTXNDTL[i].CGSTAMT - CGSTAMT;
        //                                        TTXNDTL.SGSTAMT = VE.TTXNDTL[i].SGSTAMT - SGSTAMT;
        //                                        TTXNDTL.CESSAMT = VE.TTXNDTL[i].CESSAMT - CESSAMT;
        //                                        //TTXNDTL.DUTYAMT = VE.TTXNDTL[i].DUTYAMT - DUTYAMT;
        //                                    }
        //                                    TTXNDTL.RECPROGAUTONO = VE.TTXNDTL[i].RECPROGAUTONO;
        //                                    TTXNDTL.RECPROGSLNO = VE.TTXNDTL[i].RECPROGSLNO;
        //                                    TTXNDTL.RECPROGLOTNO = VE.TTXNDTL[i].RECPROGLOTNO;
        //                                    TTXNDTL.CUTLENGTH = VE.TTXNDTL[i].CUTLENGTH == null ? 0 : VE.TTXNDTL[i].CUTLENGTH;
        //                                    TTXNDTL.DIA = VE.TTXNDTL[i].DIA == null ? 0 : VE.TTXNDTL[i].DIA;
        //                                    TTXNDTL.LOCABIN = VE.TTXNDTL[i].LOCABIN;
        //                                    if (VE.MENU_PARA == "SB" || VE.MENU_PARA == "SR" || VE.MENU_PARA == "SBEXP") { TTXNDTL.MTRLJOBCD = "FS"; } else { TTXNDTL.MTRLJOBCD = VE.MTRLJOBCD; }
        //                                    TTXNDTL.MTRL_COST = VE.TTXNDTL[i].MTRL_COST;
        //                                    TTXNDTL.OTH_COST = VE.TTXNDTL[i].OTH_COST;
        //                                    TTXNDTL.ORDAUTONO = VE.TTXNDTL[i].ORDAUTONO;
        //                                    TTXNDTL.DOAUTONO = VE.TTXNDTL[i].DOAUTONO;
        //                                    //TTXNDTL.ALTITCD = VE.TTXNDTL[i].ALTITCD;
        //                                    TTXNDTL.ALTITCD = VE.TTXNDTL[i].ITCD;
        //                                    TTXNDTL.ALTPARTCD = VE.TTXNDTL[i].ALTPARTCD;
        //                                    TTXNDTL.ALTCOLRCD = VE.TTXNDTL[i].ALTCOLRCD;
        //                                    TTXNDTL.ALTSIZECD = VE.TTXNDTL[i].ALTSIZECD;
        //                                    TTXNDTL.WGHT = VE.TTXNDTL[i].WGHT;
        //                                    TTXNDTL.AGDOCNO = VE.TTXNDTL[i].AGSTDOCNO == null ? VE.TTXNDTL[0].AGSTDOCNO : VE.TTXNDTL[i].AGSTDOCNO;
        //                                    TTXNDTL.AGDOCDT = VE.TTXNDTL[i].AGSTDOCDT == null ? VE.TTXNDTL[0].AGSTDOCDT : VE.TTXNDTL[i].AGSTDOCDT;

        //                                    dbsql = MasterHelpFa.RetModeltoSql(TTXNDTL);
        //                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
        //                                }
        //                            }
        //                            igst = igst + Convert.ToDouble(VE.TTXNDTL[i].IGSTAMT);
        //                            cgst = cgst + Convert.ToDouble(VE.TTXNDTL[i].CGSTAMT);
        //                            sgst = sgst + Convert.ToDouble(VE.TTXNDTL[i].SGSTAMT);
        //                            cess = cess + Convert.ToDouble(VE.TTXNDTL[i].CESSAMT);
        //                            duty = duty + Convert.ToDouble(VE.TTXNDTL[i].DUTYAMT);
        //                            dbqty = dbqty + Convert.ToDouble(VE.TTXNDTL[i].QNTY);
        //                            dbamt = dbamt + Convert.ToDouble(VE.TTXNDTL[i].GROSS);
        //                            dbcurramt = dbcurramt + Convert.ToDouble(VE.TTXNDTL[i].CURR_AMT);
        //                        }
        //                        else
        //                        {
        //                            double AMOUNT = 0; double GROSS = 0; double TDDISCAMT = 0; double DISCAMT = 0; double SCMDISCAMT = 0; double IGSTAMT = 0; double CGSTAMT = 0;
        //                            double SGSTAMT = 0; double CESSAMT = 0; //double DUTYAMT = 0;

        //                            if (VE.TTXNDTL[i].SLNO != 0 && VE.TTXNDTL[i].RATE != null && VE.TTXNDTL[i].QNTY != null && VE.TTXNDTL[i].QNTY != 0)
        //                            {
        //                                COUNTER = COUNTER + 1;
        //                                T_TXNDTL TTXNDTL = new T_TXNDTL();
        //                                TTXNDTL.CLCD = TTXN.CLCD;
        //                                TTXNDTL.EMD_NO = TTXN.EMD_NO;
        //                                TTXNDTL.DTAG = TTXN.DTAG;
        //                                TTXNDTL.AUTONO = TTXN.AUTONO;
        //                                TTXNDTL.SLNO = Convert.ToInt16(COUNTER);
        //                                TTXNDTL.DOCCD = TTXN.DOCCD;
        //                                TTXNDTL.DOCNO = TTXN.DOCNO;
        //                                TTXNDTL.DOCDT = TTXN.DOCDT;
        //                                TTXNDTL.ITCD = VE.TTXNDTL[i].ITCD;
        //                                TTXNDTL.PARTCD = VE.TTXNDTL[i].PARTCD;
        //                                TTXNDTL.STKTYPE = "F";
        //                                TTXNDTL.COLRCD = "";
        //                                TTXNDTL.SIZECD = "";
        //                                TTXNDTL.STKDRCR = stkdrcr;
        //                                //TTXNDTL.STKTYPE = VE.TTXNDTL[i].STKTYPE;
        //                                //TTXNDTL.FREESTK = "F";
        //                                TTXNDTL.BATCHNO = VE.TTXNDTL[i].BATCHNO;
        //                                TTXNDTL.GOCD = VE.T_TXN.GOCD;
        //                                TTXNDTL.ISSPROGAUTONO = VE.TTXNDTL[i].ISSPROGAUTONO;
        //                                TTXNDTL.ISSPROGSLNO = VE.TTXNDTL[i].ISSPROGSLNO;
        //                                TTXNDTL.ISSPROGLOTNO = VE.TTXNDTL[i].ISSPROGLOTNO;
        //                                TTXNDTL.JOBCD = VE.TTXNDTL[i].JOBCD;
        //                                TTXNDTL.NOS = VE.TTXNDTL[i].NOS == null ? 0 : VE.TTXNDTL[i].NOS;
        //                                TTXNDTL.QNTY = VE.TTXNDTL[i].QNTY;
        //                                TTXNDTL.STKQNTY = VE.TTXNDTL[i].STKQNTY;
        //                                TTXNDTL.QNTYIN = VE.TTXNDTL[i].QNTYIN;
        //                                TTXNDTL.RATE = VE.TTXNDTL[i].RATE;
        //                                TTXNDTL.TDDISCTYPE = VE.TTXNDTL[i].TDDISCTYPE;
        //                                TTXNDTL.TDDISCRATE = VE.TTXNDTL[i].TDDISCRATE;
        //                                TTXNDTL.DISCTYPE = VE.TTXNDTL[i].DISCTYPE;
        //                                TTXNDTL.DISCRATE = VE.TTXNDTL[i].DISCRATE;
        //                                TTXNDTL.SCMDISCTYPE = VE.TTXNDTL[i].SCMDISCTYPE;
        //                                TTXNDTL.SCMDISCRATE = VE.TTXNDTL[i].SCMDISCRATE;
        //                                TTXNDTL.IGSTPER = VE.TTXNDTL[i].IGSTPER;
        //                                TTXNDTL.CGSTPER = VE.TTXNDTL[i].CGSTPER;
        //                                TTXNDTL.SGSTPER = VE.TTXNDTL[i].SGSTPER;
        //                                TTXNDTL.CESSPER = VE.TTXNDTL[i].CESSPER;
        //                                TTXNDTL.DUTYPER = VE.TTXNDTL[i].DUTYPER;

        //                                TTXNDTL.AMT = VE.TTXNDTL[i].AMT - AMOUNT;
        //                                TTXNDTL.TDDISCAMT = VE.TTXNDTL[i].TDDISCAMT - TDDISCAMT;
        //                                TTXNDTL.DISCAMT = VE.TTXNDTL[i].DISCAMT - DISCAMT;
        //                                TTXNDTL.SCMDISCAMT = VE.TTXNDTL[i].SCMDISCAMT - SCMDISCAMT;
        //                                TTXNDTL.IGSTAMT = VE.TTXNDTL[i].IGSTAMT - IGSTAMT;
        //                                TTXNDTL.CGSTAMT = VE.TTXNDTL[i].CGSTAMT - CGSTAMT;
        //                                TTXNDTL.SGSTAMT = VE.TTXNDTL[i].SGSTAMT - SGSTAMT;
        //                                TTXNDTL.CESSAMT = VE.TTXNDTL[i].CESSAMT - CESSAMT;
        //                                //TTXNDTL.DUTYAMT = VE.TTXNDTL[i].DUTYAMT - DUTYAMT;

        //                                TTXNDTL.RECPROGAUTONO = VE.TTXNDTL[i].RECPROGAUTONO;
        //                                TTXNDTL.RECPROGSLNO = VE.TTXNDTL[i].RECPROGSLNO;
        //                                TTXNDTL.RECPROGLOTNO = VE.TTXNDTL[i].RECPROGLOTNO;
        //                                TTXNDTL.CUTLENGTH = VE.TTXNDTL[i].CUTLENGTH == null ? 0 : VE.TTXNDTL[i].CUTLENGTH;
        //                                TTXNDTL.DIA = VE.TTXNDTL[i].DIA == null ? 0 : VE.TTXNDTL[i].DIA;
        //                                TTXNDTL.LOCABIN = VE.TTXNDTL[i].LOCABIN;
        //                                if (VE.MENU_PARA == "SB" || VE.MENU_PARA == "SR" || VE.MENU_PARA == "SBEXP") { TTXNDTL.MTRLJOBCD = "FS"; } else { TTXNDTL.MTRLJOBCD = VE.MTRLJOBCD; }
        //                                TTXNDTL.MTRL_COST = VE.TTXNDTL[i].MTRL_COST;
        //                                TTXNDTL.OTH_COST = VE.TTXNDTL[i].OTH_COST;
        //                                TTXNDTL.ORDAUTONO = VE.TTXNDTL[i].ORDAUTONO;
        //                                TTXNDTL.DOAUTONO = VE.TTXNDTL[i].DOAUTONO;
        //                                //TTXNDTL.ALTITCD = VE.TTXNDTL[i].ALTITCD;
        //                                TTXNDTL.ALTITCD = VE.TTXNDTL[i].ITCD;
        //                                TTXNDTL.ALTPARTCD = VE.TTXNDTL[i].ALTPARTCD;
        //                                TTXNDTL.ALTCOLRCD = VE.TTXNDTL[i].ALTCOLRCD;
        //                                TTXNDTL.ALTSIZECD = VE.TTXNDTL[i].ALTSIZECD;
        //                                TTXNDTL.WGHT = VE.TTXNDTL[i].WGHT;
        //                                TTXNDTL.AGDOCNO = VE.TTXNDTL[i].AGSTDOCNO == null ? VE.TTXNDTL[0].AGSTDOCNO : VE.TTXNDTL[i].AGSTDOCNO;
        //                                TTXNDTL.AGDOCDT = VE.TTXNDTL[i].AGSTDOCDT == null ? VE.TTXNDTL[0].AGSTDOCDT : VE.TTXNDTL[i].AGSTDOCDT;
        //                                dbsql = MasterHelpFa.RetModeltoSql(TTXNDTL);
        //                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
        //                            }

        //                            igst = igst + Convert.ToDouble(VE.TTXNDTL[i].IGSTAMT);
        //                            cgst = cgst + Convert.ToDouble(VE.TTXNDTL[i].CGSTAMT);
        //                            sgst = sgst + Convert.ToDouble(VE.TTXNDTL[i].SGSTAMT);
        //                            cess = cess + Convert.ToDouble(VE.TTXNDTL[i].CESSAMT);
        //                            duty = duty + Convert.ToDouble(VE.TTXNDTL[i].DUTYAMT);
        //                            dbqty = dbqty + Convert.ToDouble(VE.TTXNDTL[i].QNTY);
        //                            dbamt = dbamt + Convert.ToDouble(VE.TTXNDTL[i].GROSS);
        //                            dbcurramt = dbcurramt + Convert.ToDouble(VE.TTXNDTL[i].CURR_AMT);
        //                        }
        //                    }
        //                }
        //                itamt = dbamt;
        //                string pslipauto = "";
        //                if (VE.TTXNDTL != null && VE.TTXNDTL.Count > 0)
        //                {
        //                    if (VE.PACKING_SLIP != null && VE.PACKING_SLIP.Count > 0)
        //                    {
        //                        for (int i = 0; i <= VE.PACKING_SLIP.Count - 1; i++)
        //                        {
        //                            if (VE.PACKING_SLIP[i].Checked == true)
        //                            {
        //                                T_TXN_LINKNO TTXNPSLIP = new T_TXN_LINKNO();
        //                                TTXNPSLIP.EMD_NO = TTXN.EMD_NO;
        //                                TTXNPSLIP.CLCD = TTXN.CLCD;
        //                                TTXNPSLIP.DTAG = TTXN.DTAG;
        //                                TTXNPSLIP.TTAG = TTXN.TTAG;
        //                                TTXNPSLIP.AUTONO = TTXN.AUTONO;
        //                                TTXNPSLIP.LINKAUTONO = VE.PACKING_SLIP[i].PACKAUTONO;
        //                                if (pslipauto != "") pslipauto += ",";
        //                                pslipauto += "'" + VE.PACKING_SLIP[i].PACKAUTONO + "'";
        //                                dbsql = MasterHelpFa.RetModeltoSql(TTXNPSLIP);
        //                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
        //                            }
        //                        }
        //                    }
        //                }
        //                if (dbqty == 0)
        //                {
        //                    dberrmsg = "Quantity not entered";
        //                    goto dbnotsave;
        //                }
        //                if (pslipauto != "")
        //                {
        //                    bool qtymis = true;
        //                    sql = "select sum(a.qnty) qnty from " + scm1 + ".t_pslipdtl a where a.autono in (" + pslipauto + ") ";
        //                    DataTable rstbl = MasterHelpFa.SQLquery(sql);
        //                    if (rstbl.Rows.Count > 0)
        //                    {
        //                        if (Convert.ToDouble(rstbl.Rows[0]["qnty"]) == dbqty) qtymis = false;
        //                    }
        //                    if (qtymis == true)
        //                    {
        //                        dberrmsg = "Quantity mismatch with packing slip.. please again reselect packsing slip..";
        //                        goto dbnotsave;
        //                    }
        //                }

        //                if (VE.MENU_PARA == "SB" || VE.MENU_PARA == "SBEXP" || VE.MENU_PARA == "SOTH" || VE.MENU_PARA == "PR")
        //                {
        //                    dr = "D"; cr = "C";
        //                }
        //                else
        //                {
        //                    dr = "C"; cr = "D";
        //                }
        //                isl = 1;
        //                if (VE.TTXNAMT != null)
        //                {
        //                    for (int i = 0; i <= VE.TTXNAMT.Count - 1; i++)
        //                    {
        //                        if (VE.TTXNAMT[i].SLNO != 0 && VE.TTXNAMT[i].AMTCD != null && VE.TTXNAMT[i].AMT != 0)
        //                        {
        //                            T_TXNAMT TTXNAMT = new T_TXNAMT();
        //                            TTXNAMT.AUTONO = TTXN.AUTONO;
        //                            TTXNAMT.SLNO = VE.TTXNAMT[i].SLNO;
        //                            TTXNAMT.EMD_NO = TTXN.EMD_NO;
        //                            TTXNAMT.CLCD = TTXN.CLCD;
        //                            TTXNAMT.DTAG = TTXN.DTAG;
        //                            TTXNAMT.AMTCD = VE.TTXNAMT[i].AMTCD;
        //                            TTXNAMT.AMTDESC = VE.TTXNAMT[i].AMTDESC;
        //                            TTXNAMT.AMTRATE = VE.TTXNAMT[i].AMTRATE;
        //                            TTXNAMT.HSNCODE = VE.TTXNAMT[i].HSNCODE;
        //                            TTXNAMT.CURR_AMT = VE.TTXNAMT[i].CURR_AMT;
        //                            TTXNAMT.AMT = VE.TTXNAMT[i].AMT;
        //                            TTXNAMT.IGSTPER = VE.TTXNAMT[i].IGSTPER;
        //                            TTXNAMT.IGSTAMT = VE.TTXNAMT[i].IGSTAMT;
        //                            TTXNAMT.CGSTPER = VE.TTXNAMT[i].CGSTPER;
        //                            TTXNAMT.CGSTAMT = VE.TTXNAMT[i].CGSTAMT;
        //                            TTXNAMT.SGSTPER = VE.TTXNAMT[i].SGSTPER;
        //                            TTXNAMT.SGSTAMT = VE.TTXNAMT[i].SGSTAMT;
        //                            TTXNAMT.CESSPER = VE.TTXNAMT[i].CESSPER;
        //                            TTXNAMT.CESSAMT = VE.TTXNAMT[i].CESSAMT;
        //                            TTXNAMT.DUTYPER = VE.TTXNAMT[i].DUTYPER;
        //                            TTXNAMT.DUTYAMT = VE.TTXNAMT[i].DUTYAMT;
        //                            dbsql = MasterHelpFa.RetModeltoSql(TTXNAMT);
        //                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
        //                            if (blactpost == true)
        //                            {
        //                                isl = isl + 1;
        //                                string adrcr = cr;
        //                                if (VE.TTXNAMT[i].ADDLESS == "L") adrcr = dr;
        //                                dbsql = MasterHelpFa.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), adrcr, VE.TTXNAMT[i].GLCD, null,
        //                                        Convert.ToDouble(VE.TTXNAMT[i].AMT), strrem, tbl.Rows[0]["parglcd"].ToString(), TTXN.SLCD, 0, 0, Convert.ToDouble(VE.TTXNAMT[i].CURR_AMT));
        //                                OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
        //                                if (adrcr == "D") dbDrAmt = dbDrAmt + Convert.ToDouble(VE.TTXNAMT[i].AMT);
        //                                else dbCrAmt = dbCrAmt + Convert.ToDouble(VE.TTXNAMT[i].AMT);
        //                            }
        //                            igst = igst + Convert.ToDouble(VE.TTXNAMT[i].IGSTAMT);
        //                            cgst = cgst + Convert.ToDouble(VE.TTXNAMT[i].CGSTAMT);
        //                            sgst = sgst + Convert.ToDouble(VE.TTXNAMT[i].SGSTAMT);
        //                            cess = cess + Convert.ToDouble(VE.TTXNAMT[i].CESSAMT);
        //                            duty = duty + Convert.ToDouble(VE.TTXNAMT[i].DUTYAMT);
        //                        }
        //                    }
        //                }
        //                //-----------------------DOCUMENT PASSING DATA---------------------------//
        //                double TRAN_AMT = Convert.ToDouble(TTXN.BLAMT);
        //                if (docpassrem != "" && (VE.MENU_PARA == "SB" || VE.MENU_PARA == "SBEXP"))
        //                {
        //                    docpassrem = "Doc # " + TTXN.DOCNO.ToString() + " Party Name # " + VE.BuyerName.ToString() + "~" + docpassrem;
        //                    var TCDP_DATA1 = Cn.T_CONTROL_DOC_PASS(TTXN.DOCCD, 0, TTXN.EMD_NO.Value, TTXN.AUTONO, CommVar.CurSchema(UNQSNO).ToString(), docpassrem);
        //                    if (TCDP_DATA1.Item1.Count != 0)
        //                    {
        //                        for (int tr = 0; tr <= TCDP_DATA1.Item1.Count - 1; tr++)
        //                        {
        //                            dbsql = MasterHelpFa.RetModeltoSql(TCDP_DATA1.Item1[tr]);
        //                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    var TCDP_DATA = Cn.T_CONTROL_DOC_PASS(TTXN.DOCCD, TRAN_AMT, TTXN.EMD_NO.Value, TTXN.AUTONO, CommVar.CurSchema(UNQSNO).ToString());
        //                    if (TCDP_DATA.Item1.Count != 0)
        //                    {
        //                        for (int tr = 0; tr <= TCDP_DATA.Item1.Count - 1; tr++)
        //                        {
        //                            dbsql = MasterHelpFa.RetModeltoSql(TCDP_DATA.Item1[tr]);
        //                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
        //                        }
        //                    }
        //                }
        //                //if (docpassrem != "") DB.T_CNTRL_DOC_PASS.Add(TCDP);

        //                if (VE.UploadDOC != null)// add
        //                {
        //                    var img = Cn.SaveUploadImageTransaction(VE.UploadDOC, TTXN.AUTONO, TTXN.EMD_NO.Value);
        //                    if (img.Item1.Count != 0)
        //                    {
        //                        for (int tr = 0; tr <= img.Item1.Count - 1; tr++)
        //                        {
        //                            dbsql = MasterHelpFa.RetModeltoSql(img.Item1[tr]);
        //                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
        //                        }
        //                        for (int tr = 0; tr <= img.Item2.Count - 1; tr++)
        //                        {
        //                            dbsql = MasterHelpFa.RetModeltoSql(img.Item2[tr]);
        //                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
        //                        }
        //                    }
        //                }
        //                if (VE.T_CNTRL_HDR_REM.DOCREM != null)// add REMARKS
        //                {
        //                    var NOTE = Cn.SAVETRANSACTIONREMARKS(VE.T_CNTRL_HDR_REM, TTXN.AUTONO, TTXN.CLCD, TTXN.EMD_NO.Value);
        //                    if (NOTE.Item1.Count != 0)
        //                    {
        //                        for (int tr = 0; tr <= NOTE.Item1.Count - 1; tr++)
        //                        {
        //                            dbsql = MasterHelpFa.RetModeltoSql(NOTE.Item1[tr]);
        //                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
        //                        }
        //                    }
        //                }
        //                string sslcd = VE.T_TXN.SLCD;
        //                string salpur = "";
        //                if (blactpost == true)
        //                {
        //                    if (VE.MENU_PARA == "SB" || VE.MENU_PARA == "SBEXP" || VE.MENU_PARA == "SR" || VE.MENU_PARA == "SOTH") salpur = "S";
        //                    else salpur = "P";
        //                    string prodrem = strrem, expglcd = "";
        //                    if (postdt != "") prodrem = strrem + " " + weekrem;
        //                    string prodglcd = tbl.Rows[0]["prodglcd"].ToString();
        //                    var AMTGLCD = (from x in VE.TTXNDTL
        //                                   group x by new { x.AUTONO } into P
        //                                   select new
        //                                   {
        //                                       GLCD = prodglcd,
        //                                       CLASS1CD = "",
        //                                       CLASS2CD = "",
        //                                       QTY = P.Sum(A => A.QNTY),
        //                                       AMT = P.Sum(A => A.AMT - (A.TDDISCAMT + A.DISCAMT + A.SCMDISCAMT))
        //                                   }).Where(a => a.QTY != 0).ToList();

        //                    if (AMTGLCD != null && AMTGLCD.Count > 0)
        //                    {
        //                        for (int i = 0; i <= AMTGLCD.Count - 1; i++)
        //                        {
        //                            isl = isl + 1;
        //                            dbamt = AMTGLCD[i].AMT;
        //                            dbsql = MasterHelpFa.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), cr, AMTGLCD[i].GLCD, sslcd,
        //                                    dbamt, prodrem, tbl.Rows[0]["parglcd"].ToString(), TTXN.SLCD, AMTGLCD[i].QTY.Value, 0, 0, postdt);
        //                            OraCmd.CommandText = dbsql;
        //                            OraCmd.ExecuteNonQuery();
        //                            if (cr == "D") dbDrAmt = dbDrAmt + dbamt;
        //                            else dbCrAmt = dbCrAmt + dbamt;
        //                            dbsql = MasterHelpFa.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
        //                                    AMTGLCD[i].CLASS1CD, AMTGLCD[i].CLASS2CD, AMTGLCD[i].AMT, 0, strrem);
        //                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
        //                            expglcd = AMTGLCD[i].GLCD;
        //                        }
        //                    }
        //                    #region  GST Financial Part
        //                    string[] gstpostcd = new string[5];
        //                    double[] gstpostamt = new double[5];
        //                    int gi = 0;
        //                    for (int gt = 0; gt < 5; gt++)
        //                    {
        //                        gstpostcd[gt] = ""; gstpostamt[gt] = 0;
        //                    }

        //                    gstpostcd[gi] = tbl.Rows[0]["igstcd"].ToString(); gstpostamt[gi] = igst; gi++;
        //                    gstpostcd[gi] = tbl.Rows[0]["cgstcd"].ToString(); gstpostamt[gi] = cgst; gi++;
        //                    gstpostcd[gi] = tbl.Rows[0]["sgstcd"].ToString(); gstpostamt[gi] = sgst; gi++;
        //                    gstpostcd[gi] = tbl.Rows[0]["cesscd"].ToString(); gstpostamt[gi] = cess; gi++;
        //                    gstpostcd[gi] = tbl.Rows[0]["dutycd"].ToString(); gstpostamt[gi] = duty; gi++;

        //                    for (int gt = 0; gt < 5; gt++)
        //                    {
        //                        if (gstpostamt[gt] != 0)
        //                        {
        //                            isl = isl + 1;
        //                            dbsql = MasterHelpFa.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), cr, gstpostcd[gt], sslcd,
        //                                    gstpostamt[gt], prodrem, tbl.Rows[0]["parglcd"].ToString(), TTXN.SLCD, dbqty, 0, 0, postdt);
        //                            OraCmd.CommandText = dbsql;
        //                            OraCmd.ExecuteNonQuery();
        //                            if (cr == "D") dbDrAmt = dbDrAmt + gstpostamt[gt];
        //                            else dbCrAmt = dbCrAmt + gstpostamt[gt];
        //                            dbsql = MasterHelpFa.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), null,
        //                                    tbl.Rows[0]["class1cd"].ToString(), tbl.Rows[0]["class2cd"].ToString(), gstpostamt[gt], 0, strrem);
        //                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
        //                        }
        //                    }
        //                    if (VE.MENU_PARA == "SBEXP" && VE.T_VCH_GST.EXPCD == "01")
        //                    {
        //                        for (int gt = 0; gt < 5; gt++)
        //                        {
        //                            gstpostcd[gt] = ""; gstpostamt[gt] = 0;
        //                        }
        //                        gi = 0;
        //                        gstpostcd[gi] = "60000069"; gstpostamt[gi] = igst; gi++;
        //                        gstpostcd[gi] = tbl.Rows[0]["cgstcd"].ToString(); gstpostamt[gi] = cgst; gi++;
        //                        gstpostcd[gi] = tbl.Rows[0]["sgstcd"].ToString(); gstpostamt[gi] = sgst; gi++;
        //                        gstpostcd[gi] = tbl.Rows[0]["cesscd"].ToString(); gstpostamt[gi] = cess; gi++;
        //                        gstpostcd[gi] = tbl.Rows[0]["dutycd"].ToString(); gstpostamt[gi] = duty; gi++;

        //                        for (int gt = 0; gt < 5; gt++)
        //                        {
        //                            if (gstpostamt[gt] != 0)
        //                            {
        //                                isl = isl + 1;
        //                                dbsql = MasterHelpFa.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), dr, gstpostcd[gt], sslcd,
        //                                        gstpostamt[gt], prodrem, tbl.Rows[0]["parglcd"].ToString(), TTXN.SLCD, dbqty, 0, 0, postdt);
        //                                OraCmd.CommandText = dbsql;
        //                                OraCmd.ExecuteNonQuery();
        //                                if (dr == "D") dbDrAmt = dbDrAmt + gstpostamt[gt];
        //                                else dbCrAmt = dbCrAmt + gstpostamt[gt];
        //                                dbsql = MasterHelpFa.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), null,
        //                                        tbl.Rows[0]["class1cd"].ToString(), tbl.Rows[0]["class2cd"].ToString(), gstpostamt[gt], 0, strrem);
        //                                OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
        //                            }
        //                        }
        //                    }
        //                    #endregion
        //                    // TCS
        //                    dbamt = Convert.ToDouble(VE.T_TXN.TCSAMT);
        //                    if (dbamt != 0)
        //                    {
        //                        string adrcr = cr;

        //                        isl = isl + 1;
        //                        dbsql = MasterHelpFa.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), adrcr, tbl.Rows[0]["tcsgl"].ToString(), sslcd,
        //                                dbamt, strrem, tbl.Rows[0]["parglcd"].ToString(), TTXN.SLCD, 0, 0, 0);
        //                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
        //                        if (adrcr == "D") dbDrAmt = dbDrAmt + dbamt;
        //                        else dbCrAmt = dbCrAmt + dbamt;
        //                        dbsql = MasterHelpFa.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
        //                                null, null, Convert.ToDouble(VE.T_TXN.TCSAMT), 0, strrem + " TCS @ " + VE.T_TXN.TCSPER.ToString() + "%");
        //                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();

        //                        string blno = DOCPATTERN, bldt = TTXN.DOCDT.ToString().retDateStr(); double tdson = itamt;
        //                        dbsql = MasterHelpFa.InsVch_TdsTxn(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, blno, bldt, tbl.Rows[0]["parglcd"].ToString(), TTXN.SLCD,
        //                                 "Z", Convert.ToSByte(isl), dbamt, tdson, TTXN.TCSPER.Value, TTXN.TCSAMT.Value, bldt, null, "", "TCS");
        //                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
        //                    }
        //                    // Ronded off
        //                    dbamt = Convert.ToDouble(VE.T_TXN.ROAMT);
        //                    if (dbamt != 0)
        //                    {
        //                        string adrcr = cr;
        //                        if (dbamt < 0) adrcr = dr;
        //                        if (dbamt < 0) dbamt = dbamt * -1;

        //                        isl = isl + 1;
        //                        dbsql = MasterHelpFa.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), adrcr, tbl.Rows[0]["rogl"].ToString(), sslcd,
        //                                dbamt, strrem, tbl.Rows[0]["parglcd"].ToString(), TTXN.SLCD, 0, 0, 0);
        //                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
        //                        if (adrcr == "D") dbDrAmt = dbDrAmt + dbamt;
        //                        else dbCrAmt = dbCrAmt + dbamt;
        //                    }
        //                    //Party wise posting
        //                    isl = 1; //isl + 1;
        //                    dbsql = MasterHelpFa.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), dr,
        //                        tbl.Rows[0]["parglcd"].ToString(), sslcd, Convert.ToDouble(VE.T_TXN.BLAMT), strrem, tbl.Rows[0]["prodglcd"].ToString(),
        //                        null, dbqty, 0, dbcurramt);
        //                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
        //                    if (dr == "D") dbDrAmt = dbDrAmt + Convert.ToDouble(VE.T_TXN.BLAMT);
        //                    else dbCrAmt = dbCrAmt + Convert.ToDouble(VE.T_TXN.BLAMT);
        //                    dbsql = MasterHelpFa.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), null,
        //                            tbl.Rows[0]["class1cd"].ToString(), tbl.Rows[0]["class2cd"].ToString(), Convert.ToDouble(VE.T_TXN.BLAMT), dbcurramt, strrem);
        //                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();

        //                    string strblno = ""; string strbldt = ""; string strduedt = ""; string strvtype = ""; string strrefno = "";
        //                    if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "SB" || VE.MENU_PARA == "SBEXP" || VE.MENU_PARA == "SOTH") { strvtype = "BL"; } else { strvtype = "CN"; }
        //                    strduedt = Convert.ToDateTime(TTXN.DOCDT.Value).AddDays(Convert.ToDouble(TTXN.DUEDAYS)).ToString().retDateStr();
        //                    if ((VE.MENU_PARA == "SB" || VE.MENU_PARA == "SBEXP") && duedatecalcon == "R" && VE.T_TXNTRANS.LRDT.ToString().retDateStr() != "")
        //                    {
        //                        strduedt = Convert.ToDateTime(VE.T_TXNTRANS.LRDT.Value).AddDays(Convert.ToDouble(TTXN.DUEDAYS)).ToString().retDateStr();
        //                    }
        //                    if (VE.MENU_PARA == "PB")
        //                    {
        //                        strblno = TTXN.PREFNO;
        //                        strbldt = TTXN.PREFDT.ToString();
        //                    }
        //                    else
        //                    {
        //                        strbldt = TTXN.DOCDT.ToString();
        //                        strblno = DOCPATTERN;
        //                    }
        //                    string blconslcd = TTXN.CONSLCD;
        //                    if (TTXN.SLCD != sslcd) blconslcd = TTXN.SLCD;
        //                    if (blconslcd == sslcd) blconslcd = "";
        //                    double duedays;
        //                    if (TTXN.DUEDAYS != null) duedays = TTXN.DUEDAYS.Value; else duedays = 0;
        //                    string transnm = VE.TransporterName == null ? VE.CourierName : VE.TransporterName, lrdt = VE.T_TXNTRANS.LRDT == null ? "" : VE.T_TXNTRANS.LRDT.ToString().retDateStr();
        //                    string cod = TTXNOTH.COD;
        //                    dbsql = MasterHelpFa.InsVch_Bl(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, dr,
        //                            tbl.Rows[0]["parglcd"].ToString(), sslcd, blconslcd, VE.T_TXNOTH.AGSLCD, tbl.Rows[0]["class1cd"].ToString(), Convert.ToSByte(isl),
        //                            Convert.ToDouble(VE.T_TXN.BLAMT), strblno, strbldt, strrefno, strduedt, strvtype, duedays, itamt, "", "",
        //                            Convert.ToDouble(VE.T_TXN.BLAMT), VE.T_TXNTRANS.LRNO, lrdt, transnm, cod);
        //                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();

        //                    int gs = 0;
        //                    //Posting of GST 
        //                    double amthsnamt = 0, amtigst = 0, amtcgst = 0, amtsgst = 0, amtcess = 0, amtgstper = 0, mult = 1, titamt = 0;
        //                    double rplamt = 0, rpligst = 0, rplcgst = 0, rplsgst = 0, rplcess = 0;
        //                    double balamt = 0, baligst = 0, balcgst = 0, balsgst = 0, balcess = 0;
        //                    double amtigstper = 0, amtcgstper = 0, amtsgstper = 0;
        //                    int lastitemno = 0;

        //                    if (VE.TTXNAMT != null)
        //                    {
        //                        for (int i = 0; i <= VE.TTXNAMT.Count - 1; i++)
        //                        {
        //                            if (VE.TTXNAMT[i].AMT != 0 && VE.TTXNAMT[i].HSNCODE == null)
        //                            {
        //                                if (VE.TTXNAMT[i].ADDLESS == "A") mult = 1; else mult = -1;
        //                                amthsnamt = amthsnamt + (VE.TTXNAMT[i].AMT.Value * mult);
        //                                amtigst = amtigst + (VE.TTXNAMT[i].IGSTAMT.Value * mult);
        //                                amtcgst = amtcgst + (VE.TTXNAMT[i].CGSTAMT.Value * mult);
        //                                amtsgst = amtsgst + (VE.TTXNAMT[i].SGSTAMT.Value * mult);
        //                                amtcess = amtcess + (VE.TTXNAMT[i].CESSAMT.Value * mult);
        //                                amtigstper = VE.TTXNAMT[i].IGSTPER.Value;
        //                                amtcgstper = VE.TTXNAMT[i].CGSTPER.Value;
        //                                amtsgstper = VE.TTXNAMT[i].SGSTPER.Value;
        //                            }
        //                        }
        //                        if (amthsnamt != 0)
        //                        {
        //                            for (int i = 0; i <= VE.TTXNDTL.Count - 1; i++)
        //                            {
        //                                if (VE.TTXNDTL[i].SLNO != 0 && VE.TTXNDTL[i].ITCD != null && VE.TTXNDTL[i].IGSTPER == amtigstper && VE.TTXNDTL[i].CGSTPER == amtcgstper && VE.TTXNDTL[i].SGSTPER == amtsgstper)
        //                                {
        //                                    titamt = titamt + VE.TTXNDTL[i].AMT;
        //                                    lastitemno = i;
        //                                }
        //                            }
        //                            balamt = amthsnamt; baligst = amtigst; balcgst = amtcgst; balsgst = amtsgst; balcess = amtcess;
        //                            for (int i = 0; i <= VE.TTXNDTL.Count - 1; i++)
        //                            {
        //                                if (VE.TTXNDTL[i].SLNO != 0 && VE.TTXNDTL[i].ITCD != null && VE.TTXNDTL[i].IGSTPER == amtigstper && VE.TTXNDTL[i].CGSTPER == amtcgstper && VE.TTXNDTL[i].SGSTPER == amtsgstper)
        //                                {
        //                                    if (i == lastitemno)
        //                                    {
        //                                        rplamt = balamt; rpligst = baligst; rplcgst = balcgst; rplsgst = balsgst; rplcess = balcess;
        //                                    }
        //                                    else
        //                                    {
        //                                        rplamt = ((amthsnamt / titamt) * VE.TTXNDTL[i].AMT).toRound();
        //                                        rpligst = ((amtigst / titamt) * VE.TTXNDTL[i].AMT).toRound();
        //                                        rplcgst = ((amtcgst / titamt) * VE.TTXNDTL[i].AMT).toRound();
        //                                        rplsgst = ((amtsgst / titamt) * VE.TTXNDTL[i].AMT).toRound();
        //                                        rplcess = ((amtcess / titamt) * VE.TTXNDTL[i].AMT).toRound();
        //                                    }
        //                                    balamt = balamt - rplamt;
        //                                    baligst = baligst - rpligst;
        //                                    balcgst = balcgst - rplcgst;
        //                                    balsgst = balsgst - rplsgst;
        //                                    balcess = balcess - rplcess;

        //                                    VE.TTXNDTL[i].GROSS = VE.TTXNDTL[i].GROSS + rplamt;
        //                                    VE.TTXNDTL[i].IGSTAMT = VE.TTXNDTL[i].IGSTAMT + rpligst;
        //                                    VE.TTXNDTL[i].CGSTAMT = VE.TTXNDTL[i].CGSTAMT + rplcgst;
        //                                    VE.TTXNDTL[i].SGSTAMT = VE.TTXNDTL[i].SGSTAMT + rplsgst;
        //                                    VE.TTXNDTL[i].CESSAMT = VE.TTXNDTL[i].CESSAMT + rplcess;
        //                                }
        //                            }
        //                        }
        //                    }
        //                    string dncntag = ""; string exemptype = "";
        //                    if (VE.MENU_PARA == "SR") dncntag = "SC";
        //                    if (VE.MENU_PARA == "PR") dncntag = "PD";
        //                    bool blamtins = false; double gblamt = TTXN.BLAMT.Value; double groamt = TTXN.ROAMT.Value + (TTXN.TCSAMT == null ? 0 : Convert.ToDouble(TTXN.TCSAMT));

        //                    //Type A_T = VE.TTXNDTL["gross"].GetType)(); Type Q_T = tbl.Rows[0]["qnty"].GetType(); Type I_T = tbl.Rows[0]["igstamt"].GetType();
        //                    //Type C_T = tbl.Rows[0]["cgstamt"].GetType(); Type S_T = tbl.Rows[0]["sgstamt"].GetType(); Type E_T = tbl.Rows[0]["sgstamt"].GetType();

        //                    var GST_DATA = (from p in VE.TTXNDTL
        //                                    where (p.ITCD != null)
        //                                    group p by new
        //                                    { p.HSNCODE, p.UOMCD, p.IGSTPER, p.CGSTPER, p.SGSTPER, p.CESSPER, p.AGSTDOCNO, p.AGSTDOCDT } into X
        //                                    select new
        //                                    {
        //                                        //ITNM = X.Key.ITNM,
        //                                        ITNM = "",
        //                                        HSNCODE = X.Key.HSNCODE,
        //                                        UOMCD = X.Key.UOMCD,
        //                                        IGSTPER = X.Key.IGSTPER,
        //                                        CGSTPER = X.Key.CGSTPER,
        //                                        SGSTPER = X.Key.SGSTPER,
        //                                        CESSPER = X.Key.CESSPER,
        //                                        AGSTDOCNO = X.Key.AGSTDOCNO,
        //                                        AGSTDOCDT = X.Key.AGSTDOCDT,
        //                                        GROSS = X.Sum(Z => Z.GROSS),
        //                                        TQNTY = X.Sum(Z => Z.QNTY),
        //                                        IGSTAMT = X.Sum(Z => Z.IGSTAMT),
        //                                        CGSTAMT = X.Sum(Z => Z.CGSTAMT),
        //                                        SGSTAMT = X.Sum(Z => Z.SGSTAMT),
        //                                        CESSAMT = X.Sum(Z => Z.CESSAMT)
        //                                    }).ToList();

        //                    //var GST_DATA = (from DataRow DR in VE.TTXNDTL
        //                    //                group DR by new { ITNM = DR["itnm"].ToString(), HSNCODE = DR["hsncode"].ToString(), UOMCD = DR["uomcd"].ToString(), IGST = DR["igstper"].ToString(), CGST = DR["cgstper"].ToString(), SGST = DR["sgstper"].ToString(), CESS = DR["cessper"].ToString() } into X
        //                    //                select new
        //                    //                {
        //                    //                    ITNM = X.Key.ITNM,
        //                    //                    HSNCODE = X.Key.HSNCODE,
        //                    //                    UOMCD = X.Key.UOMCD,
        //                    //                    IGSTPER = X.Key.IGST,
        //                    //                    CGSTPER = X.Key.CGST,
        //                    //                    SGSTPER = X.Key.SGST,
        //                    //                    CESSPER = X.Key.CESS,
        //                    //                    GROSS = X.Sum(Z => Z.Field<double>("gross")),
        //                    //                    TQNTY = X.Sum(Z => Z.Field<double>("qnty")),
        //                    //                    IGSTAMT = X.Sum(Z => Z.Field<double>("igstamt")),
        //                    //                    CGSTAMT = X.Sum(Z => Z.Field<double>("cgstamt")),
        //                    //                    SGSTAMT = X.Sum(Z => Z.Field<double>("sgstamt")),
        //                    //                    CESSAMT = X.Sum(Z => Z.Field<double>("cessamt")),
        //                    //                    //GROSS = A_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("gross")) : Convert.ToDouble(X.Sum(Z => Z.Field<decimal>("gross"))),
        //                    //                    //TQNTY = Q_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("qnty")) : Convert.ToDouble(X.Sum(Z => Z.Field<decimal>("qnty"))),
        //                    //                    //IGSTAMT = I_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("igstamt")) : Convert.ToDouble(X.Sum(Z => Z.Field<decimal>("igstamt"))),
        //                    //                    //CGSTAMT = C_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("cgstamt")) : Convert.ToDouble(X.Sum(Z => Z.Field<decimal>("cgstamt"))),
        //                    //                    //SGSTAMT = S_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("sgstamt")) : Convert.ToDouble(X.Sum(Z => Z.Field<decimal>("sgstamt"))),
        //                    //                    //CESSAMT = E_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("cessamt")) : Convert.ToDouble(X.Sum(Z => Z.Field<decimal>("cessamt"))),

        //                    //                    TOTALPER = Convert.ToDouble(X.Key.IGST) + Convert.ToDouble(X.Key.CGST) + Convert.ToDouble(X.Key.SGST)
        //                    //                }).OrderBy(A => A.TOTALPER).ToList();
        //                    if (GST_DATA != null && GST_DATA.Count > 0)
        //                    {
        //                        int i = 0;
        //                        foreach (var k in GST_DATA)
        //                        {
        //                            gs = gs + 1;
        //                            if (k.IGSTAMT + k.CGSTAMT + k.SGSTAMT == 0) exemptype = "N";
        //                            dbsql = MasterHelpFa.InsVch_GST(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, cr,
        //                                    isl, gs, TTXN.SLCD, strblno, strbldt, k.HSNCODE, k.ITNM, k.TQNTY.Value, k.UOMCD,
        //                                    k.GROSS.Value, k.IGSTPER.Value, k.IGSTAMT.Value, k.CGSTPER.Value, k.CGSTAMT.Value, k.SGSTPER.Value, k.SGSTAMT.Value,
        //                                    k.CESSPER.Value, k.CESSAMT.Value, salpur, groamt, gblamt, 0, (VE.T_VCH_GST.INVTYPECD == null ? "01" : VE.T_VCH_GST.INVTYPECD), null, VE.TTXNDTL[i].AGSTDOCNO, VE.TTXNDTL[i].AGSTDOCDT.ToString(), TTXNOTH.DNCNCD,
        //                                    "", "", VE.T_VCH_GST.EXPCD, VE.T_VCH_GST.SHIPDOCNO, (VE.T_VCH_GST.SHIPDOCDT == null ? "" : VE.T_VCH_GST.SHIPDOCDT.ToString()), VE.T_VCH_GST.PORTCD, dncntag, TTXN.CONSLCD, exemptype, 0, expglcd, "Y", "", VE.T_VCH_GST.LUTNO, (VE.T_VCH_GST.LUTDT == null ? "" : VE.T_VCH_GST.LUTDT.ToString()));
        //                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
        //                            gblamt = 0; groamt = 0;
        //                        }
        //                    }

        //                    //for (int i = 0; i <= VE.TTXNDTL.Count - 1; i++)
        //                    //{
        //                    //    if (VE.TTXNDTL[i].SLNO != 0 && VE.TTXNDTL[i].ITCD != null)
        //                    //    {
        //                    //        gs = gs + 1;
        //                    //        if (VE.TTXNDTL[i].IGSTAMT.Value + VE.TTXNDTL[i].CGSTAMT.Value + VE.TTXNDTL[i].SGSTAMT.Value == 0) exemptype = "N";
        //                    //        dbsql = MasterHelpFa.InsVch_GST(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, cr,
        //                    //                isl, gs, TTXN.SLCD, strblno, strbldt, VE.TTXNDTL[i].HSNCODE, VE.TTXNDTL[i].ITNM, VE.TTXNDTL[i].QNTY.Value, VE.TTXNDTL[i].UOMCD,
        //                    //                VE.TTXNDTL[i].GROSS.Value, VE.TTXNDTL[i].IGSTPER.Value, VE.TTXNDTL[i].IGSTAMT.Value, VE.TTXNDTL[i].CGSTPER.Value, VE.TTXNDTL[i].CGSTAMT.Value, VE.TTXNDTL[i].SGSTPER.Value, VE.TTXNDTL[i].SGSTAMT.Value,
        //                    //                VE.TTXNDTL[i].CESSPER.Value, VE.TTXNDTL[i].CESSAMT.Value, salpur, groamt, gblamt, 0, "01", null, VE.TTXNDTL[i].AGSTDOCNO, VE.TTXNDTL[i].AGSTDOCDT.ToString(), TTXNOTH.DNCNCD,
        //                    //                "", "", "", VE.SHIPDOCNO, (VE.SHIPDOCDT == null ? "" : VE.SHIPDOCDT.ToString()), VE.PORTCD, dncntag, TTXN.CONSLCD, exemptype);
        //                    //        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
        //                    //        gblamt = 0; groamt = 0;
        //                    //    }
        //                    //}

        //                    //      TVCHGST.EXPCD = VE.T_VCH_GST.EXPCD;
        //                    //      TVCHGST.SHIPDOCNO = VE.T_VCH_GST.SHIPDOCNO;
        //                    //      TVCHGST.SHIPDOCDT = VE.T_VCH_GST.SHIPDOCDT;
        //                    //      TVCHGST.INVTYPECD = VE.T_VCH_GST.INVTYPECD;
        //                    //      TVCHGST.PORTCD = VE.T_VCH_GST.PORTCD;

        //                    if (VE.TTXNAMT != null)
        //                    {
        //                        for (int i = 0; i <= VE.TTXNAMT.Count - 1; i++)
        //                        {
        //                            if (VE.TTXNAMT[i].SLNO != 0 && VE.TTXNAMT[i].AMTCD != null && VE.TTXNAMT[i].AMT != 0 && VE.TTXNAMT[i].HSNCODE != null)
        //                            {
        //                                gs = gs + 1;
        //                                dbsql = MasterHelpFa.InsVch_GST(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, cr,
        //                                        isl, gs, TTXN.SLCD, strblno, strbldt, VE.TTXNAMT[i].HSNCODE, VE.TTXNAMT[i].AMTNM, 0, "OTH",
        //                                        VE.TTXNAMT[i].AMT.Value, VE.TTXNAMT[i].IGSTPER.Value, VE.TTXNAMT[i].IGSTAMT.Value, VE.TTXNAMT[i].CGSTPER.Value, VE.TTXNAMT[i].CGSTAMT.Value, VE.TTXNAMT[i].SGSTPER.Value, VE.TTXNAMT[i].SGSTAMT.Value,
        //                                        VE.TTXNAMT[i].CESSPER.Value, VE.TTXNAMT[i].CESSAMT.Value, salpur, groamt, gblamt, 0, (VE.T_VCH_GST.INVTYPECD == null ? "01" : VE.T_VCH_GST.INVTYPECD), null, VE.TTXNDTL[0].AGSTDOCNO, VE.TTXNDTL[0].AGSTDOCDT.ToString(), TTXNOTH.DNCNCD,
        //                                        "", "", VE.T_VCH_GST.EXPCD, VE.T_VCH_GST.SHIPDOCNO, (VE.T_VCH_GST.SHIPDOCDT == null ? "" : VE.T_VCH_GST.SHIPDOCDT.ToString()), VE.T_VCH_GST.PORTCD, dncntag, TTXN.CONSLCD, exemptype, 0, VE.TTXNAMT[i].GLCD, "Y", "", VE.T_VCH_GST.LUTNO, (VE.T_VCH_GST.LUTDT == null ? "" : VE.T_VCH_GST.LUTDT.ToString()));
        //                                OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
        //                                gblamt = 0; groamt = 0;
        //                            }
        //                        }
        //                    }
        //                }
        //                if (Math.Round(dbDrAmt, 2) != Math.Round(dbCrAmt, 2))
        //                {
        //                    dberrmsg = "Debit " + Math.Round(dbDrAmt, 4) + " & Credit " + Math.Round(dbCrAmt, 4) + " not matching please click on rounded off...";
        //                    goto dbnotsave;
        //                }
        //                ModelState.Clear();

        //                if (docpassrem != "")
        //                {
        //                    string[,] emlaryBody = new string[2, 2];
        //                    emlaryBody[0, 0] = "{UserName}"; emlaryBody[0, 1] = (VE.DefaultAction == "A" ? CommVar.UserID() : VE.T_CNTRL_HDR.USR_ID);
        //                    emlaryBody[1, 0] = "{Description}"; emlaryBody[1, 1] = docpassrem;
        //                    bool emailsent = EmailControl.SendHtmlFormattedEmail("defaultemail", "Sale Bill authorisation of document # " + TTXN.DOCNO + " in name of " + VE.BuyerName, "Docauth.htm", emlaryBody, null);
        //                }

        //                if (VE.DefaultAction == "A")
        //                {
        //                    ContentFlg = "1" + " (Voucher No. " + TTXN.DOCNO + ")~" + TTXN.AUTONO;
        //                }
        //                else if (VE.DefaultAction == "E")
        //                {
        //                    ContentFlg = "2";
        //                }
        //                transaction.Commit();
        //                OraTrans.Commit();
        //                OraCon.Dispose();
        //                return Content(ContentFlg);
        //            }
        //            else if (VE.DefaultAction == "V")
        //            {
        //                dbsql = MasterHelpFa.TblUpdt("t_txnoth", VE.T_TXN.AUTONO, "D");
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                dbsql = MasterHelpFa.TblUpdt("t_txnamt", VE.T_TXN.AUTONO, "D");
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                dbsql = MasterHelpFa.TblUpdt("t_txntrans", VE.T_TXN.AUTONO, "D");
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                dbsql = MasterHelpFa.TblUpdt("t_txn_linkno", VE.T_TXN.AUTONO, "D");
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                dbsql = MasterHelpFa.TblUpdt("t_txndtl", VE.T_TXN.AUTONO, "D");
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                dbsql = MasterHelpFa.TblUpdt("t_txn", VE.T_TXN.AUTONO, "D");
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

        //                dbsql = MasterHelpFa.TblUpdt("t_txnstatus", VE.T_TXN.AUTONO, "D");
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                dbsql = MasterHelpFa.TblUpdt("t_cntrl_doc_pass", VE.T_TXN.AUTONO, "D");
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_doc_dtl", VE.T_TXN.AUTONO, "D");
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_doc", VE.T_TXN.AUTONO, "D");
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_rem", VE.T_TXN.AUTONO, "D");
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                dbsql = MasterHelpFa.T_Cntrl_Hdr_Updt_Ins(VE.T_TXN.AUTONO, "D", "S", null, null, null, VE.T_TXN.DOCDT.retStr(), null, null, null);
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

        //                // Delete from financial schema
        //                dbsql = MasterHelpFa.TblUpdt("t_tdstxn", VE.T_TXN.AUTONO, "D", "F");
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                dbsql = MasterHelpFa.TblUpdt("t_vch_gst", VE.T_TXN.AUTONO, "D", "F");
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                dbsql = MasterHelpFa.TblUpdt("t_vch_bl", VE.T_TXN.AUTONO, "D", "F");
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                dbsql = MasterHelpFa.TblUpdt("t_vch_class", VE.T_TXN.AUTONO, "D", "F");
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                dbsql = MasterHelpFa.TblUpdt("t_vch_det", VE.T_TXN.AUTONO, "D", "F");
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                dbsql = MasterHelpFa.TblUpdt("t_vch_hdr", VE.T_TXN.AUTONO, "D", "F");
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
        //                dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr", VE.T_TXN.AUTONO, "D", "F");
        //                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

        //                ModelState.Clear();
        //                transaction.Commit();
        //                OraTrans.Commit();
        //                OraCon.Dispose();
        //                return Content("3");
        //            }
        //            else
        //            {
        //                return Content("");
        //            }
        //            goto dbok;
        //        dbnotsave:;
        //            transaction.Rollback();
        //            OraTrans.Rollback();
        //            OraCon.Dispose();
        //            return Content(dberrmsg);
        //        dbok:;
        //        }
        //        catch (Exception ex)
        //        {
        //            transaction.Rollback();
        //            OraTrans.Rollback();
        //            OraCon.Dispose();
        //            return Content(ex.Message + ex.InnerException);
        //        }
        //    }
        //}


    }
}