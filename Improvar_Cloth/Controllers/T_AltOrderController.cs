﻿using System;
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
    public class T_AltOrderController : Controller
    {
        // GET: T_AltOrder
        Connection Cn = new Connection(); MasterHelp masterHelp = new MasterHelp();
        Salesfunc salesfunc = new Salesfunc(); DataTable DT = new DataTable();
        EmailControl EmailControl = new EmailControl();
        T_STCHALT TBH; T_CNTRL_HDR TCH; T_CNTRL_HDR_REM SLR; T_TXNMEMO TTXNMEMO;
        SMS SMS = new SMS(); string sql = "";
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public ActionResult T_AltOrder(string op = "", string key = "", int Nindex = 0, string searchValue = "", string parkID = "", string ThirdParty = "no")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Stiching/Alteration";
                    TransactionAltOrder VE = (parkID == "") ? new TransactionAltOrder() : (Improvar.ViewModels.TransactionAltOrder)Session[parkID];
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    switch (VE.MENU_PARA)
                    {
                        case "ST":
                            ViewBag.formname = "Stiching"; break;
                        case "AT":
                            ViewBag.formname = "Alteration"; break;
                        default: ViewBag.formname = ""; break;
                    }
                    string LOC = CommVar.Loccd(UNQSNO);
                    string COM = CommVar.Compcd(UNQSNO);
                    string YR1 = CommVar.YearCode(UNQSNO);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    VE.DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
                    if (VE.MENU_PARA == "ST")
                    {
                        VE.DropDown_list1 = (from i in DB.M_STCHGRP
                                             where i.STCHALT == "S"
                                             select new DropDown_list1()
                                             { value = i.STCHCD, text = i.STCHNM }).Distinct().OrderBy(s => s.text).ToList();
                    }
                    else {
                        VE.DropDown_list1 = (from i in DB.M_STCHGRP
                                             where i.STCHALT == "A"
                                             select new DropDown_list1()
                                             { value = i.STCHCD, text = i.STCHNM }).Distinct().OrderBy(s => s.text).ToList();
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
                        string GCS = Cn.GCS();
                        VE.IndexKey = (from p in DB.T_STCHALT
                                       join q in DB.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
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
                            VE.T_STCHALT = TBH;
                            VE.T_CNTRL_HDR = TCH;
                            VE.T_TXNMEMO = TTXNMEMO;
                            VE.T_CNTRL_HDR_REM = SLR;
                            if (VE.T_CNTRL_HDR.DOCNO != null) ViewBag.formname = ViewBag.formname + " (" + VE.T_CNTRL_HDR.DOCNO + ")";
                        }
                        if (op.ToString() == "A")
                        {
                            if (parkID == "")
                            {
                                T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                                TCH.DOCDT = Cn.getCurrentDate(VE.mindate);
                                VE.T_CNTRL_HDR = TCH;
                                T_TXNMEMO TXNMEMO = new T_TXNMEMO();
                                string scmf = CommVar.FinSchema(UNQSNO); string scm = CommVar.CurSchema(UNQSNO);
                                sql = "";
                                sql += " select a.rtdebcd,b.rtdebnm,b.mobile,a.inc_rate,b.city,b.add1,b.add2,b.add3,effdt ";
                                sql += "  from  " + scm + ".M_SYSCNFG a, " + scmf + ".M_RETDEB b ";
                                sql += " where a.RTDEBCD=b.RTDEBCD and a.effdt in(select max(effdt) effdt from  " + scm + ".M_SYSCNFG)";

                                DataTable syscnfgdt = masterHelp.SQLquery(sql);
                                if (syscnfgdt != null && syscnfgdt.Rows.Count > 0)
                                {
                                    TXNMEMO.RTDEBCD = syscnfgdt.Rows[0]["RTDEBCD"].retStr();
                                    VE.RTDEBNM = syscnfgdt.Rows[0]["RTDEBNM"].retStr();
                                    var addrs = syscnfgdt.Rows[0]["add1"].retStr() + " " + syscnfgdt.Rows[0]["add2"].retStr() + " " + syscnfgdt.Rows[0]["add3"].retStr();
                                    VE.ADDR = addrs + "/" + syscnfgdt.Rows[0]["city"].retStr();
                                    VE.RTMOBILE = syscnfgdt.Rows[0]["MOBILE"].retStr();
                                    VE.INC_RATE = syscnfgdt.Rows[0]["INC_RATE"].retStr() == "Y" ? true : false;

                                }
                                VE.T_TXNMEMO = TXNMEMO;

                                List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                                UploadDOC UPL = new UploadDOC();
                                UPL.DocumentType = Cn.DOC_TYPE();
                                UploadDOC1.Add(UPL);
                                VE.UploadDOC = UploadDOC1;

                                if (VE.TTXNPYMT == null || VE.TTXNPYMT.Count == 0)
                                {
                                    var MPAYMENT = (from i in DB.M_PAYMENT join j in DB.M_CNTRL_HDR on i.M_AUTONO equals j.M_AUTONO where j.INACTIVE_TAG == "N" && i.PYMTTYPE == "C" select new { PYMTCD = i.PYMTCD, PYMTNM = i.PYMTNM, GLCD = i.GLCD }).ToList();

                                    if (MPAYMENT.Count > 0)
                                    {
                                        VE.TTXNPYMT = (from i in MPAYMENT select new TTXNPYMT { PYMTCD = i.PYMTCD, PYMTNM = i.PYMTNM, GLCD = i.GLCD }).ToList();
                                        for (int p = 0; p <= VE.TTXNPYMT.Count - 1; p++)
                                        {
                                            VE.TTXNPYMT[p].SLNO = Convert.ToInt16(p + 1);
                                        }
                                    }
                                    else
                                    {
                                        int slno = 0;
                                        List<TTXNPYMT> TTXNPYMNT = new List<TTXNPYMT>();
                                        TTXNPYMT TXNPYMT = new TTXNPYMT();
                                        TXNPYMT.SLNO = Convert.ToInt16(slno + 1);
                                        TTXNPYMNT.Add(TXNPYMT);
                                        VE.TTXNPYMT = TTXNPYMNT;
                                    }
                                }

                            }
                            else
                            {
                                if (VE.UploadDOC != null) { foreach (var i in VE.UploadDOC) { i.DocumentType = Cn.DOC_TYPE(); } }
                                INI INIF = new INI();
                                INIF.DeleteKey(Session["UR_ID"].ToString(), parkID, Server.MapPath("~/Park.ini"));
                            }
                            VE = (TransactionAltOrder)Cn.CheckPark(VE, VE.MenuID, VE.MenuIndex, LOC, COM, CommVar.CurSchema(UNQSNO).ToString(), Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString());
                        }
                    }
                    else
                    {
                        VE.DefaultView = false;
                        VE.DefaultDay = 0;
                    }
                    string docdt = "";
                    if (TCH != null) if (TCH.DOCDT != null) docdt = TCH.DOCDT.ToString().Remove(10);
                    Cn.getdocmaxmindate(VE.T_CNTRL_HDR.DOCCD, docdt, VE.DefaultAction, VE.T_CNTRL_HDR.DOCONLYNO, VE);
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                TransactionAltOrder VE = new TransactionAltOrder();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message;
                return View(VE);
            }
        }
        public TransactionAltOrder Navigation(TransactionAltOrder VE, ImprovarDB DB, int index, string searchValue)
        {
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            string COM_CD = CommVar.Compcd(UNQSNO);
            string LOC_CD = CommVar.Loccd(UNQSNO);
            string DATABASE = CommVar.CurSchema(UNQSNO).ToString();
            string DATABASEF = CommVar.FinSchema(UNQSNO);
            Cn.getQueryString(VE);

            TBH = new T_STCHALT(); TCH = new T_CNTRL_HDR(); SLR = new T_CNTRL_HDR_REM(); TTXNMEMO = new T_TXNMEMO();
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
                TBH = DB.T_STCHALT.Find(aa[0].Trim());
                TTXNMEMO = DB.T_TXNMEMO.Find(TBH.AUTONO);
                TCH = DB.T_CNTRL_HDR.Find(TBH.AUTONO);
                if (TBH != null)
                { if (TBH.INC_RATE == "Y") VE.INC_RATE = true; }

                SLR = Cn.GetTransactionReamrks(CommVar.CurSchema(UNQSNO).ToString(), TBH.AUTONO);
                VE.UploadDOC = Cn.GetUploadImageTransaction(CommVar.CurSchema(UNQSNO).ToString(), TBH.AUTONO);
                string Scm = CommVar.CurSchema(UNQSNO); string Scmf = CommVar.FinSchema(UNQSNO);

                if (TTXNMEMO != null)
                {
                    var rtdebcd = TTXNMEMO.RTDEBCD;
                    sql = "";
                    sql += " select a.rtdebcd,b.rtdebnm,b.mobile,b.city,b.add1,b.add2,b.add3 ";
                    sql += "  from  " + Scm + ".T_TXNMEMO a, " + Scmf + ".M_RETDEB b ";
                    sql += " where a.RTDEBCD=b.RTDEBCD and a.RTDEBCD= '" + rtdebcd + "' ";

                    DataTable syscnfgdt = masterHelp.SQLquery(sql);
                    if (syscnfgdt != null && syscnfgdt.Rows.Count > 0)
                    {
                        VE.RTDEBNM = syscnfgdt.Rows[0]["RTDEBNM"].retStr();
                        var addrs = syscnfgdt.Rows[0]["add1"].retStr() + " " + syscnfgdt.Rows[0]["add2"].retStr() + " " + syscnfgdt.Rows[0]["add3"].retStr();
                        VE.ADDR = addrs + "/" + syscnfgdt.Rows[0]["city"].retStr();
                        VE.RTMOBILE = syscnfgdt.Rows[0]["MOBILE"].retStr();

                    }
                }
                sql = "";
                sql += "select a.AUTONO,a.SLNO,a.STCHCD,b.stchnm,a.QNTY,a.RATE ";
                sql += "from " + CommVar.CurSchema(UNQSNO) + ".T_STCHALT_DTL a, " + CommVar.CurSchema(UNQSNO) + ".M_STCHGRP b ";
                sql += "where a.stchcd = B.STCHCD and a.AUTONO ='" + TBH.AUTONO + "' ";
                sql += "order by a.SLNO ";

                DataTable tblTSTCHALT_DTLCOMP = masterHelp.SQLquery(sql);
                VE.TSTCHALT_MEASUREMENT = (from DataRow dr in tblTSTCHALT_DTLCOMP.Rows
                                           select new TSTCHALT_MEASUREMENT()
                                           {
                                               SLNO = dr["SLNO"].retStr(),
                                               STCHCD = dr["STCHCD"].retStr(),
                                               QNTY = (dr["QNTY"].retShort()),
                                               RATE = dr["RATE"].retDbl(),
                                               STCHNM = dr["STCHNM"].retStr(),
                                           }).ToList();
                //VE.DropDown_list1 = (from i in VE.TSTCHALT_MEASUREMENT
                //                     select new DropDown_list1()
                //                     { value = i.STCHCD, text = i.STCHNM }).Distinct().OrderBy(s => s.text).ToList();
                foreach (var measure in VE.TSTCHALT_MEASUREMENT)
                {
                    sql = "";
                    sql += "select a.SLNO,a.AUTONO,a.STCHCD,a.QNTY,a.RATE,b.FLDCD,b.FLDVAL,b.FLDREM,b.FLDTYPE,nvl(C.FLDNM, c.flddesc) flddesc,D.STCHNM ";
                    sql += "from " + Scm + ".T_STCHALT_DTL a, " + Scm + ".T_STCHALT_DTL_COMP b, " + Scm + ".M_STCHGRP_COMP c," + CommVar.CurSchema(UNQSNO) + ".M_STCHGRP d ";
                    sql += "where a.AUTONO = b.AUTONO(+) and b.fldcd = c.fldcd(+) and a.slno = b.slno and c.STCHCD= D.STCHCD ";
                    sql += "and a.AUTONO = '" + TBH.AUTONO + "' and a.slno='" + measure.SLNO + "' ";
                    sql += "order by  nvl(C.FLDNM, c.flddesc) ";
                    tblTSTCHALT_DTLCOMP = masterHelp.SQLquery(sql);
                    var TSTCHALT_DTLCOMP = (from DataRow dr in tblTSTCHALT_DTLCOMP.Rows
                                            select new TSTCHALT_DTLCOMP()
                                            {
                                                FLDCD = dr["FLDCD"].retStr(),
                                                FLDVAL = dr["FLDVAL"].retStr(),
                                                FLDREM = dr["FLDREM"].retStr(),
                                                FLDTYPE = dr["FLDTYPE"].retStr(),
                                            }).DistinctBy(s => s.FLDCD).ToList();
                    measure.TSTCHALT_DTLCOMP = TSTCHALT_DTLCOMP;
                    VE.QNTY = measure.QNTY;
                    VE.RATE = measure.RATE;
                }

                string str2 = "select b.SLNO,b.PYMTCD,c.PYMTNM,b.AMT,b.CARDNO,b.INSTNO,b.INSTDT,b.PYMTREM,b.GLCD from " + Scm + ".T_STCHALT a," + Scm + ".t_txnpymt b," + Scm + ".m_payment c ";
                str2 += "where a.autono=b.autono and  b.PYMTCD=c.PYMTCD and a.autono='" + TBH.AUTONO + "'";
                var PYMT_DATA = masterHelp.SQLquery(str2);
                if (PYMT_DATA != null)
                {
                    VE.TTXNPYMT = (from DataRow dr in PYMT_DATA.Rows
                                   select new TTXNPYMT()
                                   {
                                       SLNO = dr["SLNO"].retShort(),
                                       PYMTCD = dr["PYMTCD"].retStr(),
                                       PYMTNM = dr["PYMTNM"].retStr(),
                                       AMT = dr["AMT"].retDbl(),
                                       CARDNO = dr["CARDNO"].retStr(),
                                       INSTNO = dr["INSTNO"].retStr(),
                                       INSTDT = dr["INSTDT"].retDateStr(),
                                       PYMTREM = dr["PYMTREM"].retStr(),
                                       GLCD = dr["GLCD"].retStr(),
                                   }).ToList();
                }
                double T_PYMT_AMT = 0;

                for (int p = 0; p <= VE.TTXNPYMT.Count - 1; p++)
                {
                    T_PYMT_AMT = T_PYMT_AMT + VE.TTXNPYMT[p].AMT.Value;

                }
                VE.T_PYMT_AMT = T_PYMT_AMT;


            }
            //Cn.DateLock_Entry(VE, DB, TCH.DOCDT.Value);
            if (TCH.CANCEL == "Y") VE.CancelRecord = true; else VE.CancelRecord = false;
            return VE;
        }
        public ActionResult SearchPannelData(TransactionAltOrder VE, string SRC_SLCD, string SRC_DOCNO, string SRC_FDT, string SRC_TDT)
        {
            string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), yrcd = CommVar.YearCode(UNQSNO);
            Cn.getQueryString(VE);

            List<DocumentType> DocumentType = new List<DocumentType>();
            DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
            string doccd = DocumentType.Select(i => i.value).ToArray().retSqlfromStrarray();
            string sql = "";

            sql = "select a.autono, b.docno, to_char(b.docdt,'dd/mm/yyyy') docdt, b.doccd,a.AGCMNO ";
            sql += "from " + scm + ".T_STCHALT a, " + scm + ".t_cntrl_hdr b  ";
            sql += "where a.autono=b.autono and b.doccd in (" + doccd + ") and ";
            if (SRC_FDT.retStr() != "") sql += "b.docdt >= to_date('" + SRC_FDT.retDateStr() + "','dd/mm/yyyy') and ";
            if (SRC_TDT.retStr() != "") sql += "b.docdt <= to_date('" + SRC_TDT.retDateStr() + "','dd/mm/yyyy') and ";
            if (SRC_DOCNO.retStr() != "") sql += "(b.vchrno like '%" + SRC_DOCNO.retStr() + "%' or b.docno like '%" + SRC_DOCNO.retStr() + "%') and ";
            if (SRC_SLCD.retStr() != "") sql += "(a.slcd like '%" + SRC_SLCD.retStr() + "%' or upper(c.slnm) like '%" + SRC_SLCD.retStr().ToUpper() + "%') and ";
            sql += "b.loccd='" + LOC + "' and b.compcd='" + COM + "' and b.yr_cd='" + yrcd + "' ";
            sql += "order by docdt, docno ";
            DataTable tbl = masterHelp.SQLquery(sql);

            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Document Number" + Cn.GCS() + "Document Date" + Cn.GCS() + "Agst Memo No." + Cn.GCS() + "AUTO NO";
            for (int j = 0; j <= tbl.Rows.Count - 1; j++)
            {
                //SB.Append("<tr><td><b>" + tbl.Rows[j]["docno"] + "</b> [" + tbl.Rows[j]["doccd"] + "]" + " </td><td>" + tbl.Rows[j]["docdt"] + " </td><td><b>" + tbl.Rows[j]["slnm"] + "</b> [" + tbl.Rows[j]["district"] + "] (" + tbl.Rows[j]["mutslcd"] + ") </td><td>" + tbl.Rows[j]["regmobile"] + " </td><td>" + tbl.Rows[j]["autono"] + " </td></tr>");
                SB.Append("<tr><td><b>" + tbl.Rows[j]["docno"] + "</b> [" + tbl.Rows[j]["doccd"] + "]" + " </td><td>" + tbl.Rows[j]["docdt"] + " </td><td>" + tbl.Rows[j]["AGCMNO"] + " </td><td>" + tbl.Rows[j]["autono"] + " </td></tr>");
            }
            return PartialView("_SearchPannel2", masterHelp.Generate_SearchPannel(hdr, SB.ToString(), "3", "3"));
        }
        public ActionResult changeSTCHNM(string val)
        {
            try
            {
                TransactionAltOrder VE = new TransactionAltOrder();
                Cn.getQueryString(VE);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                string sql = "", strng = "";
                var query = (from c in DB.M_STCHGRP
                             join d in DB.M_STCHGRP_COMP on c.STCHCD equals d.STCHCD
                             where (c.STCHCD == val)
                             select new { d.FLDCD, d.FLDDATACOMBO, d.FLDDESC, d.FLDNM, d.FLDLEN, d.FLDTYPE }).ToList();
                strng = "<thead><tr><th>Comp</th><th>Value</th><th>Remarks</th></tr></thead>";
                strng += " <tbody> ";
                if (query != null)
                {
                    for (int i = 0; i < query.Count; i++)
                    {
                        strng += "<tr> ";
                        strng += "  <td>";
                        if (String.IsNullOrEmpty(query[i].FLDNM)) strng += query[i].FLDDESC; else strng += query[i].FLDNM;
                        strng += "  </td> ";
                        strng += "   <td> ";
                        strng += "   <input class=\"form-control text-box single-line\" id=\"FLDCD" + i + "\"  name=\"TSTCHALT_DTLCOMP[" + i + "].FLDCD\" type=\"hidden\" value=\"" + query[i].FLDCD + "\"> ";
                        strng += "   <input class=\"form-control text-box single-line\" id=\"FLDTYPE" + i + "\"  name=\"TSTCHALT_DTLCOMP[" + i + "].FLDTYPE\" type=\"hidden\" value=\"" + query[i].FLDTYPE + "\"> ";
                        strng += "   <input class=\"form-control text-box single-line\" id=\"SLNO" + i + "\" name=\"TSTCHALT_DTLCOMP[" + i + "].SLNO\" type=\"hidden\" value=\"\"> ";
                        //if (query[i].FLDTYPE == "D")
                        //{
                        //    strng += "<input class=\"form -control text-box single-line\" data-val=\"true\" data-val-length=\"The field FLDVAL must be a string with a maximum length of 500.\" data-val-length-max=\"500\" data-val-required=\"The FLDVAL field is required.\" id=\"FLDVAL+i+\" maxlength=\"+query[i].FLDLEN+\" name=\"TSTCHALT_DTLCOMP[i].FLDVAL,\"{ 0:dd / MM / yyyy} ";
                        //    strng += "\" type=\"text\" placeholder = \"dd / mm / yyyy\" onblur = \"DocumentDateCHK(this)\" autocomplete = \"off\" value=\"\"> ";
                        //    strng += "  < script > ";
                        //    if ("@Model.DefaultAction" == "A" || "@Model.DefaultAction" == "E")
                        //    {
                        //        strng += " $(function() { $(\"#\" + \"FLDVAL\").datepicker({ dateFormat: \"dd/mm/yy\", changeMonth: true, changeYear: true }); }); ";

                        //        strng += "</ script > } ";

                        //    }
                        //}
                        if (query[i].FLDTYPE == "N")
                        {
                            strng += "<input class=\"form-control text-box single-line\" autocomplete='off'  data-val=\"true\" data-val-length=\"The field FLDVAL must be a string with "
                                + "a maximum length of 500.\" data-val-length-max=\"500\" data-val-required=\"The FLDVAL field is required"
                                + ".\" id=\"TSTCHALT_DTLCOMP[" + i + "].FLDVAL\" maxlength=\"" + query[i].FLDLEN + "\" name=\"TSTCHALT_DTLCOMP[" + i + "].FLDVAL\"  type=\"text\" "
                                + "placeholder = \"0\" style = \"text-align: right;\" onkeypress =\"return numericOnly(this,4);\" value=\"\"> ";
                        }
                        else {
                            strng += "<input class=\"form-control text-box single-line\" autocomplete='off'  data-val=\"true\" data-val-length=\"The field FLDVAL must be a string with a maximum length of 500.\" data-val-length-max=\"500\" data-val-required=\"The FLDVAL field is required.\" id=\"FLDVAL+i+\" maxlength=\"" + query[i].FLDLEN + "\" name=\"TSTCHALT_DTLCOMP[i].FLDVAL ";
                            strng += "\" type=\"text\" value=\"\"> ";
                        }
                        strng += "     </td> ";
                        strng += "     <td> ";
                        strng += " <input class=\"form-control text-box single-line\" data-val=\"true\" data-val-length=\"The field FLDREM must be a string with a maximum length of 100.\" data-val-length-max=\"100\" maxlength=\"12\" name=\"TSTCHALT_DTLCOMP[" + i + "].FLDREM\" type=\"text\" value=\"\"> ";
                        strng += "    </td> ";
                        strng += "    </tr> ";
                    }
                    strng += "   </tbody> ";
                    return Content(strng);
                }

                //if (query != null)
                //{
                //    sql = Master_Help.ToReturnFieldValues(query.Take(1));
                //    return Content(sql);
                //}
                else { return Content("0"); }

            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        //public JsonResult changeSTCHNM(string val)
        //{
        //    try
        //    {
        //        ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
        //        string sql = "";
        //        var query = (from c in DB.M_STCHGRP join d in DB.M_STCHGRP_COMP on c.STCHCD equals d.STCHCD where (c.STCHCD == val)
        //                     select new { d.FLDCD, d.FLDDATACOMBO, d.FLDDESC, d.FLDNM, d.FLDLEN, d.FLDTYPE }).ToList();

        //        if (query != null)
        //        {
        //            sql = Master_Help.ToReturnFieldValues(query.Take(1));
        //            return Json("", JsonRequestBehavior.AllowGet);
        //        }
        //        //else { return Content("0"); }

        //        return Json(query, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        Cn.SaveException(ex, "");
        //        //return Content(ex.Message + ex.InnerException);
        //    }

        //    return Json("");
        //}





        //catch (Exception ex)
        //{
        //    Cn.SaveException(ex, "");
        //    return Json(ex.Message + ex.InnerException, JsonRequestBehavior.AllowGet);
        //}
        //}

        public ActionResult GetCashMemoNo(string val)
        {
            try
            {
                var sql = masterHelp.CashMemoNumber_help(val);
                if (sql.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", sql);
                }
                else
                {
                    return Content(sql);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetRefRetailDetails(string val, string Code)
        {
            try
            {
                var sql = masterHelp.RTDEBCD_help(val);

                if (sql.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", sql);
                }
                else
                {
                    //var MSG = sql.IndexOf(Cn.GCS());
                    //if (MSG >= 0)
                    //{
                    //    DataTable Taxgrpcd = salesfunc.GetSlcdDetails(Code, "");
                    //    sql += "^TAXGRPCD=^" + Taxgrpcd.Rows[0]["taxgrpcd"] + Cn.GCS();
                    //}

                    return Content(sql);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
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
        public ActionResult AddDOCRow(TransactionAltOrder VE)
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
        public ActionResult DeleteDOCRow(TransactionAltOrder VE)
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
        public ActionResult AddRowPYMT(TransactionAltOrder VE, int COUNT, string TAG)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            Cn.getQueryString(VE);
            if (VE.TTXNPYMT == null)
            {
                List<TTXNPYMT> TPROGDTL1 = new List<TTXNPYMT>();
                if (COUNT > 0 && TAG == "Y")
                {
                    int SERIAL = 0;
                    for (int j = 0; j <= COUNT - 1; j++)
                    {
                        SERIAL = SERIAL + 1;
                        TTXNPYMT MBILLDET = new TTXNPYMT();
                        MBILLDET.SLNO = SERIAL.retShort();
                        TPROGDTL1.Add(MBILLDET);
                    }
                }
                else
                {
                    TTXNPYMT MBILLDET = new TTXNPYMT();
                    MBILLDET.SLNO = 1;
                    TPROGDTL1.Add(MBILLDET);
                }
                VE.TTXNPYMT = TPROGDTL1;
            }
            else
            {
                List<TTXNPYMT> TPROGDTL = new List<TTXNPYMT>();
                for (int i = 0; i <= VE.TTXNPYMT.Count - 1; i++)
                {
                    TTXNPYMT MBILLDET = new TTXNPYMT();
                    MBILLDET = VE.TTXNPYMT[i];
                    TPROGDTL.Add(MBILLDET);
                }
                TTXNPYMT MBILLDET1 = new TTXNPYMT();
                if (COUNT > 0 && TAG == "Y")
                {
                    int SERIAL = Convert.ToInt32(VE.TTXNPYMT.Max(a => Convert.ToInt32(a.SLNO)));
                    for (int j = 0; j <= COUNT - 1; j++)
                    {
                        SERIAL = SERIAL + 1;
                        TTXNPYMT OPENING_BL = new TTXNPYMT();
                        OPENING_BL.SLNO = SERIAL.retShort();
                        TPROGDTL.Add(OPENING_BL);
                    }
                }
                else
                {
                    MBILLDET1.SLNO = Convert.ToInt16(Convert.ToByte(VE.TTXNPYMT.Max(a => Convert.ToInt32(a.SLNO))) + 1);
                    TPROGDTL.Add(MBILLDET1);
                }
                VE.TTXNPYMT = TPROGDTL;
            }
            //VE.TPROGDTL.ForEach(a => a.DRCRTA = masterHelp.DR_CR().OrderByDescending(s => s.text).ToList());
            VE.DefaultView = true;
            return PartialView("_T_AltOrder_PAYMENT", VE);
        }
        public ActionResult DeleteRowPYMT(TransactionAltOrder VE)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                List<TTXNPYMT> ITEMSIZE = new List<TTXNPYMT>();
                int count = 0;
                for (int i = 0; i <= VE.TTXNPYMT.Count - 1; i++)
                {
                    if (VE.TTXNPYMT[i].Checked == false)
                    {
                        count += 1;
                        TTXNPYMT item = new TTXNPYMT();
                        item = VE.TTXNPYMT[i];
                        item.SLNO = count.retShort();
                        ITEMSIZE.Add(item);
                    }

                }
                VE.TTXNPYMT = ITEMSIZE;
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_AltOrder_PAYMENT", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult cancelRecords(TransactionAltOrder VE, string par1)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                Cn.getQueryString(VE);
                using (var transaction = DB.Database.BeginTransaction())
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode");
                    T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                    if (par1 == "*#*")
                    {
                        TCH = Cn.T_CONTROL_HDR(VE.T_STCHALT.AUTONO, CommVar.CurSchema(UNQSNO));
                    }
                    else
                    {
                        TCH = Cn.T_CONTROL_HDR(VE.T_STCHALT.AUTONO, CommVar.CurSchema(UNQSNO), par1);
                    }
                    DB.Entry(TCH).State = System.Data.Entity.EntityState.Modified;
                    DB.SaveChanges();
                    transaction.Commit();
                    return Content("1");
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult ParkRecord(FormCollection FC, TransactionAltOrder stream, string menuID, string menuIndex)
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
                string url = masterHelp.RetriveParkFromFile(value, Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString(), "Improvar.ViewModels.TransactionAltOrder");
                return url;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public ActionResult SAVE(FormCollection FC, TransactionAltOrder VE)
        {
            Cn.getQueryString(VE);
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            string dberrmsg = "";
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();

            string dbsql = "", postdt = "", weekrem = "", duedatecalcon = "", sql = "";
            string[] dbsql1;
            double dbDrAmt = 0, dbCrAmt = 0;

            using (OracleTransaction OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                OraCmd.Transaction = OraTrans;
                try
                {
                    OraCmd.CommandText = "lock table " + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode"; OraCmd.ExecuteNonQuery();
                    String query = "";
                    string dr = ""; string cr = ""; int isl = 0; string strrem = "";
                    double igst = 0; double cgst = 0; double sgst = 0; double cess = 0; double duty = 0; double dbqty = 0; double dbamt = 0; double dbcurramt = 0;
                    Int32 z = 0; Int32 maxR = 0;
                    string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                    string ContentFlg = "";
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        T_STCHALT TBHDR = new T_STCHALT();
                        T_TXNMEMO TTXNMEMO = new T_TXNMEMO();
                        T_TXNPYMT_HDR TTXNPYMTHDR = new T_TXNPYMT_HDR();
                        T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                        string DOCPATTERN = "";
                        TCH.DOCDT = VE.T_CNTRL_HDR.DOCDT;
                        string Ddate = Convert.ToString(TCH.DOCDT);
                        TBHDR.CLCD = CommVar.ClientCode(UNQSNO);
                        string auto_no = ""; string Month = "", DOCNO = "", DOCCD = "";
                        if (VE.DefaultAction == "A")
                        {
                            TBHDR.EMD_NO = 0; TBHDR.EMD_NO = 0;
                            DOCCD = VE.T_CNTRL_HDR.DOCCD;
                            DOCNO = Cn.MaxDocNumber(DOCCD, Ddate);
                            DOCPATTERN = Cn.DocPattern(Convert.ToInt32(DOCNO), DOCCD, CommVar.CurSchema(UNQSNO), CommVar.FinSchema(UNQSNO), Ddate);
                            auto_no = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), DOCNO, DOCCD, Ddate);
                            TBHDR.AUTONO = auto_no.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
                            Month = auto_no.Split(Convert.ToChar(Cn.GCS()))[1].ToString();
                        }
                        else
                        {
                            DOCCD = VE.T_CNTRL_HDR.DOCCD;
                            DOCNO = VE.T_CNTRL_HDR.DOCONLYNO;
                            TBHDR.AUTONO = VE.T_STCHALT.AUTONO;
                            //TBHDR.AUTONO = VE.T_STCHALT.AUTONO;
                            Month = VE.T_CNTRL_HDR.MNTHCD; var MAXEMDNO = "";
                            sql = "select max(EMD_NO) EMD_NO from " + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR where AUTONO ='" + VE.T_STCHALT.AUTONO + "'";
                            var dt = masterHelp.SQLquery(sql);
                            if (dt.Rows.Count > 0) { MAXEMDNO = dt.Rows[0]["EMD_NO"].retStr(); }
                            if (MAXEMDNO == "") { TBHDR.EMD_NO = 0; } else { TBHDR.EMD_NO = Convert.ToInt16(MAXEMDNO.retInt() + 1); }
                            TBHDR.DTAG = "E";
                        }

                        TBHDR.AGCMNO = VE.T_STCHALT.AGCMNO;
                        TBHDR.AGCMDT = VE.T_STCHALT.AGCMDT;
                        TBHDR.TRLDT = VE.T_STCHALT.TRLDT;
                        TBHDR.TRLTIME = VE.T_STCHALT.TRLTIME;
                        TBHDR.DELVDT = VE.T_STCHALT.DELVDT;
                        TBHDR.DELVTIME = VE.T_STCHALT.DELVTIME;
                        TBHDR.OTHERREFNO = VE.T_STCHALT.OTHERREFNO;
                        TBHDR.REM = VE.T_STCHALT.REM;
                        if (VE.INC_RATE == true) TBHDR.INC_RATE = "Y"; else TBHDR.INC_RATE = "N";
                        TTXNMEMO.AUTONO = TBHDR.AUTONO;
                        TTXNMEMO.CLCD = TBHDR.CLCD;
                        TTXNMEMO.RTDEBCD = VE.T_TXNMEMO.RTDEBCD;
                        TTXNMEMO.NM = VE.T_TXNMEMO.NM;
                        TTXNMEMO.MOBILE = VE.T_TXNMEMO.MOBILE;
                        // -------------------------T_TXNPYMT_HDR--------------------------//   
                        TTXNPYMTHDR.EMD_NO = TBHDR.EMD_NO;
                        TTXNPYMTHDR.CLCD = TBHDR.CLCD;
                        TTXNPYMTHDR.DTAG = TBHDR.DTAG;
                        TTXNPYMTHDR.TTAG = TBHDR.TTAG;
                        TTXNPYMTHDR.AUTONO = TBHDR.AUTONO;
                        TTXNPYMTHDR.RTDEBCD = VE.T_TXNMEMO.RTDEBCD;
                        TTXNPYMTHDR.DRCR = "C";

                        //----------------------------------------------------------//
                        if (VE.DefaultAction == "E")
                        {
                            dbsql = masterHelp.TblUpdt("T_STCHALT_DTL_COMP", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            dbsql = masterHelp.TblUpdt("T_STCHALT_DTL", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            dbsql = masterHelp.TblUpdt("t_txnpymt", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            dbsql = masterHelp.TblUpdt("t_cntrl_hdr_rem", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc_dtl", TBHDR.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        }

                        //----------------------------------------------------------//
                        //dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(TBHDR.AUTONO, VE.DefaultAction, "S", Month, DOCCD, DOCPATTERN, TCH.DOCDT.retStr(), TBHDR.EMD_NO.retShort(), DOCNO, Convert.ToDouble(DOCNO), null, null, null);
                        dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(TBHDR.AUTONO, VE.DefaultAction, "S", Month, DOCCD, DOCPATTERN, TCH.DOCDT.retStr(), TBHDR.EMD_NO.retShort(), DOCNO, Convert.ToDouble(DOCNO), null, null, null,null,0,VE.Audit_REM);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                        dbsql = masterHelp.RetModeltoSql(TBHDR, VE.DefaultAction);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        dbsql = masterHelp.RetModeltoSql(TTXNMEMO, VE.DefaultAction);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        dbsql = masterHelp.RetModeltoSql(TTXNPYMTHDR, VE.DefaultAction);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        int COUNTER = 0;
                        int slno = 0;
                        if (VE.TSTCHALT_MEASUREMENT != null)
                        {
                            for (int i = 0; i <= VE.TSTCHALT_MEASUREMENT.Count - 1; i++)
                            {
                                T_STCHALT_DTL TSTCHDTL = new T_STCHALT_DTL();
                                TSTCHDTL.CLCD = TBHDR.CLCD;
                                TSTCHDTL.EMD_NO = TBHDR.EMD_NO;
                                TSTCHDTL.DTAG = TBHDR.DTAG;
                                TSTCHDTL.AUTONO = TBHDR.AUTONO;
                                TSTCHDTL.SLNO = Convert.ToByte(i + 1);
                                TSTCHDTL.STCHCD = VE.TSTCHALT_MEASUREMENT[i].STCHCD;
                                TSTCHDTL.QNTY = VE.TSTCHALT_MEASUREMENT[i].QNTY.retShort();
                                TSTCHDTL.RATE = VE.TSTCHALT_MEASUREMENT[i].RATE.retDbl();
                                dbsql = masterHelp.RetModeltoSql(TSTCHDTL);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                if (VE.TSTCHALT_MEASUREMENT[i].TSTCHALT_DTLCOMP != null)
                                {
                                    for (int j = 0; j <= VE.TSTCHALT_MEASUREMENT[i].TSTCHALT_DTLCOMP.Count - 1; j++)
                                    {
                                        T_STCHALT_DTL_COMP TSTCHDTLCMP = new T_STCHALT_DTL_COMP();
                                        TSTCHDTLCMP.CLCD = TBHDR.CLCD;
                                        TSTCHDTLCMP.DTAG = TBHDR.DTAG;
                                        TSTCHDTLCMP.EMD_NO = TBHDR.EMD_NO;
                                        TSTCHDTLCMP.AUTONO = TBHDR.AUTONO;
                                        TSTCHDTLCMP.SLNO = TSTCHDTL.SLNO;
                                        TSTCHDTLCMP.FLDCD = VE.TSTCHALT_MEASUREMENT[i].TSTCHALT_DTLCOMP[j].FLDCD;
                                        TSTCHDTLCMP.FLDVAL = VE.TSTCHALT_MEASUREMENT[i].TSTCHALT_DTLCOMP[j].FLDVAL;
                                        TSTCHDTLCMP.FLDTYPE = VE.TSTCHALT_MEASUREMENT[i].TSTCHALT_DTLCOMP[j].FLDTYPE;
                                        TSTCHDTLCMP.FLDREM = VE.TSTCHALT_MEASUREMENT[i].TSTCHALT_DTLCOMP[j].FLDREM;

                                        dbsql = masterHelp.RetModeltoSql(TSTCHDTLCMP);
                                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                        if (VE.TTXNPYMT != null)
                        {
                            for (int i = 0; i <= VE.TTXNPYMT.Count - 1; i++)
                            {
                                if (VE.TTXNPYMT[i].SLNO != 0 && VE.TTXNPYMT[i].AMT.retDbl() != 0)
                                {
                                    T_TXNPYMT TTXNPYMNT = new T_TXNPYMT();
                                    TTXNPYMNT.AUTONO = TTXNPYMTHDR.AUTONO;
                                    TTXNPYMNT.SLNO = VE.TTXNPYMT[i].SLNO;
                                    TTXNPYMNT.EMD_NO = TTXNPYMTHDR.EMD_NO;
                                    TTXNPYMNT.CLCD = TTXNPYMTHDR.CLCD;
                                    TTXNPYMNT.DTAG = TTXNPYMTHDR.DTAG;
                                    TTXNPYMNT.PYMTCD = VE.TTXNPYMT[i].PYMTCD;
                                    TTXNPYMNT.AMT = VE.TTXNPYMT[i].AMT.retDbl();
                                    TTXNPYMNT.CARDNO = VE.TTXNPYMT[i].CARDNO;
                                    TTXNPYMNT.INSTNO = VE.TTXNPYMT[i].INSTNO;
                                    if (VE.TTXNPYMT[i].INSTDT.retStr() != "")
                                    {
                                        TTXNPYMNT.INSTDT = Convert.ToDateTime(VE.TTXNPYMT[i].INSTDT);
                                    }
                                    TTXNPYMNT.PYMTREM = VE.TTXNPYMT[i].PYMTREM;
                                    TTXNPYMNT.GLCD = VE.TTXNPYMT[i].GLCD;
                                    dbsql = masterHelp.RetModeltoSql(TTXNPYMNT);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }
                        }
                        if (VE.UploadDOC != null)// add
                        {
                            var img = Cn.SaveUploadImageTransaction(VE.UploadDOC, TBHDR.AUTONO, TBHDR.EMD_NO.Value);
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
                            var NOTE = Cn.SAVETRANSACTIONREMARKS(VE.T_CNTRL_HDR_REM, TBHDR.AUTONO, TBHDR.CLCD, TBHDR.EMD_NO.Value);
                            if (NOTE.Item1.Count != 0)
                            {
                                for (int tr = 0; tr <= NOTE.Item1.Count - 1; tr++)
                                {
                                    dbsql = masterHelp.RetModeltoSql(NOTE.Item1[tr]);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                            }
                        }


                        if (VE.DefaultAction == "A")
                        {
                            ContentFlg = "1" + " (Issue No. " + DOCCD + DOCNO + ")~" + TBHDR.AUTONO;
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
                        dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc_dtl", VE.T_STCHALT.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc", VE.T_STCHALT.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_cntrl_hdr_rem", VE.T_STCHALT.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_txnpymt", VE.T_STCHALT.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("T_STCHALT_DTL_COMP", VE.T_STCHALT.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("T_STCHALT_DTL", VE.T_STCHALT.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("T_TXNPYMT_HDR", VE.T_STCHALT.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("T_TXNMEMO", VE.T_STCHALT.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("T_STCHALT", VE.T_STCHALT.AUTONO, "D");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }


                        //dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(VE.T_STCHALT.AUTONO, "D", "S", null, null, null, VE.T_CNTRL_HDR.DOCDT.retStr(), null, null, null);
                        dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(VE.T_STCHALT.AUTONO, "D", "S", null, null, null, VE.T_CNTRL_HDR.DOCDT.retStr(), null, null, null,null,null,null,null,0,VE.Audit_REM);
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
        }
        public ActionResult AddMeasurement(TransactionAltOrder VE, string slno)
        {
            try
            {
                string STCHCD = VE.STCHCD;
                short QNTY = VE.QNTY.retShort();
                double RATE = VE.RATE.retDbl();
                List<TSTCHALT_MEASUREMENT> tSTCHALT_MEASUREMENT = new List<TSTCHALT_MEASUREMENT>();
                TSTCHALT_MEASUREMENT oldmeasure = new TSTCHALT_MEASUREMENT();
                if (VE.TSTCHALT_MEASUREMENT != null)
                {
                    oldmeasure = VE.TSTCHALT_MEASUREMENT.Where(m => m.SLNO == slno).FirstOrDefault();
                    if (oldmeasure != null)
                    {
                        VE.TSTCHALT_MEASUREMENT.RemoveAll(m => m.SLNO == slno);
                    }
                    for (int i = 0; i <= VE.TSTCHALT_MEASUREMENT.Count - 1; i++)
                    {
                        TSTCHALT_MEASUREMENT MLI2 = new TSTCHALT_MEASUREMENT();
                        MLI2 = VE.TSTCHALT_MEASUREMENT[i];
                        MLI2.SLNO = (i + 1).ToString();
                        tSTCHALT_MEASUREMENT.Add(MLI2);
                    }
                }
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                var MSTCHGRP = DB.M_STCHGRP.Where(r => r.STCHCD == STCHCD).FirstOrDefault();
                TSTCHALT_MEASUREMENT MLI = new TSTCHALT_MEASUREMENT();
                MLI.STCHCD = STCHCD;
                MLI.SLNO = (tSTCHALT_MEASUREMENT.Count + 1).ToString();
                MLI.STCHNM = MSTCHGRP.STCHNM;
                MLI.QNTY = QNTY;
                MLI.RATE = RATE;
                MLI.TSTCHALT_DTLCOMP = VE.TSTCHALT_DTLCOMP;
                tSTCHALT_MEASUREMENT.Add(MLI);

                VE.TSTCHALT_MEASUREMENT = tSTCHALT_MEASUREMENT;
                ModelState.Clear();
                return PartialView("_T_AltOrder_Measure", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }

        }
        public ActionResult GetComponent(TransactionAltOrder VE, string slno, string mode)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                if (mode == "E")
                {
                    //sql = "";
                    //sql += "select a.SLNO,a.AUTONO,a.STCHCD,a.QNTY,a.RATE,b.FLDCD,b.FLDVAL,b.FLDREM,b.FLDTYPE,nvl(C.FLDNM, c.flddesc) flddesc,D.STCHNM,c.FLDLEN ";
                    //sql += "from " + CommVar.CurSchema(UNQSNO) + ".T_STCHALT_DTL a, " + CommVar.CurSchema(UNQSNO) + ".T_STCHALT_DTL_COMP b, " + CommVar.CurSchema(UNQSNO) + ".M_STCHGRP_COMP c," + CommVar.CurSchema(UNQSNO) + ".M_STCHGRP d ";
                    //sql += "where a.AUTONO = b.AUTONO(+) and b.fldcd = c.fldcd(+) and a.STCHCD = C.STCHCD and a.slno = b.slno and c.STCHCD= D.STCHCD ";
                    //sql += "and  a.autono='" + VE.T_STCHALT.AUTONO + "' and a.slno = '" + slno + "' ";
                    //sql += "order by a.SLNO ";
                    //DataTable tblTSTCHALT_DTLCOMP = masterHelp.SQLquery(sql);
                    var STCHGRPCmplst = DB.M_STCHGRP_COMP.Where(r => r.STCHCD != null).ToList();
                    TSTCHALT_MEASUREMENT tSTCHALT_MEASUREMENT = VE.TSTCHALT_MEASUREMENT.Where(v => v.SLNO == slno).FirstOrDefault();
                    var query = tSTCHALT_MEASUREMENT.TSTCHALT_DTLCOMP;
                    string strng = "";
                    strng = "<thead><tr><th>Component</th><th>Value</th><th>Remarks</th></tr></thead>";
                    strng += " <tbody> ";
                    if (query != null)
                    {
                        for (int i = 0; i < query.Count; i++)
                        {
                            string fldcd = query[i].FLDCD;
                            M_STCHGRP_COMP sTCHGRP = STCHGRPCmplst.Where(r => r.FLDCD == fldcd).FirstOrDefault();
                            query[i].FLDDESC = sTCHGRP.FLDNM.retStr() + " " + sTCHGRP.FLDDESC.retStr();
                            var FLDLEN = sTCHGRP.FLDLEN;
                            strng += "<tr> ";
                            strng += "  <td>";
                            strng += query[i].FLDDESC;
                            strng += "  </td> ";
                            strng += "   <td> ";
                            strng += "   <input class=\"form-control text-box single-line\" id=\"FLDCD" + i + "\"  name=\"TSTCHALT_DTLCOMP[" + i + "].FLDCD\" type=\"hidden\" value=\"" + query[i].FLDCD + "\"> ";
                            strng += "   <input class=\"form-control text-box single-line\" id=\"FLDTYPE" + i + "\"  name=\"TSTCHALT_DTLCOMP[" + i + "].FLDTYPE\" type=\"hidden\" value=\"" + query[i].FLDTYPE + "\"> ";
                            strng += "   <input class=\"form-control text-box single-line\" id=\"SLNO" + i + "\" name=\"TSTCHALT_DTLCOMP[" + i + "].SLNO\" type=\"hidden\" value=\"\"> ";
                            //if (query[i].FLDTYPE == "D")
                            //{
                            //    strng += "<input class=\"form -control text-box single-line\" data-val=\"true\" data-val-length=\"The field FLDVAL must be a string with a maximum length of 500.\" data-val-length-max=\"500\" data-val-required=\"The FLDVAL field is required.\" id=\"FLDVAL+i+\" maxlength=\"+query[i].FLDLEN+\" name=\"TSTCHALT_DTLCOMP[i].FLDVAL,\"{ 0:dd / MM / yyyy} ";
                            //    strng += "\" type=\"text\" placeholder = \"dd / mm / yyyy\" onblur = \"DocumentDateCHK(this)\" autocomplete = \"off\" value=\"\"> ";
                            //    strng += "  < script > ";
                            //    if ("@Model.DefaultAction" == "A" || "@Model.DefaultAction" == "E")
                            //    {
                            //        strng += " $(function() { $(\"#\" + \"FLDVAL\").datepicker({ dateFormat: \"dd/mm/yy\", changeMonth: true, changeYear: true }); }); ";

                            //        strng += "</ script > } ";

                            //    }
                            //}
                            if (query[i].FLDTYPE == "N")
                            {
                                strng += "<input class=\"form-control text-box single-line\" autocomplete='off' data-val=\"true\" data-val-length=\"The field FLDVAL must be a string with "
                                    + "a maximum length of 500.\" data-val-length-max=\"500\" data-val-required=\"The FLDVAL field is required"
                                    + ".\" id=\"TSTCHALT_DTLCOMP[" + i + "].FLDVAL\" maxlength=\"" + sTCHGRP.FLDLEN + "\" name=\"TSTCHALT_DTLCOMP[" + i + "].FLDVAL\"  type=\"text\" "
                                    + "placeholder = \"0\" style = \"text-align: right;\" onkeypress =\"return numericOnly(this,4);\" value=\"" + query[i].FLDVAL + "\"> ";
                            }
                            else {
                                strng += "<input class=\"form-control text-box single-line\" autocomplete='off' data-val=\"true\" data-val-length=\"The field FLDVAL must be a string with a maximum length of 500.\" data-val-length-max=\"500\" data-val-required=\"The FLDVAL field is required.\" id=\"FLDVAL+i+\" maxlength=\"" + sTCHGRP.FLDLEN + "\" name=\"TSTCHALT_DTLCOMP[i].FLDVAL ";
                                strng += "\" type=\"text\" value=\"" + query[i].FLDVAL + "\"> ";
                            }
                            strng += "     </td> ";
                            strng += "     <td> ";
                            strng += " <input class=\"form-control text-box single-line\" data-val=\"true\" data-val-length=\"The field FLDREM must be a string with a maximum length of 100.\" data-val-length-max=\"100\" maxlength=\"12\" name=\"TSTCHALT_DTLCOMP[" + i + "].FLDREM\" type=\"text\" value=\"" + query[i].FLDREM + "\"> ";
                            strng += "    </td> ";
                            strng += "    </tr> ";
                        }
                        strng += "   </tbody> ";
                        return Content(strng);
                    }
                }
                else if (mode == "D")
                {
                    VE.TSTCHALT_MEASUREMENT.RemoveAll(m => m.SLNO == slno);
                    ModelState.Clear();
                    return PartialView("_T_AltOrder_Measure", VE);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
            return Content("");
        }
        public ActionResult GetPaymentDetails(string val)
        {
            try
            {
                var sql = masterHelp.PAYMTCD_help(val);
                if (sql.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", sql);
                }
                else
                {
                    return Content(sql);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
    }
}