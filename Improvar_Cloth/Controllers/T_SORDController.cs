using Improvar.Models;
using Improvar.ViewModels;
using Microsoft.Ajax.Utilities;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Reflection;

namespace Improvar.Controllers
{
    public class T_SORDController : Controller
    {
        Connection Cn = new Connection();
        MasterHelp Master_Help = new MasterHelp();
        SchemeCal Scheme_Cal = new SchemeCal();
        DataTable SIZEDT = new DataTable();
        Salesfunc Salesfunc = new Salesfunc();
        MasterHelpFa MasterHelpFa = new MasterHelpFa();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: T_SORD//
        T_SORD sl; T_CNTRL_HDR sll; T_CNTRL_HDR_REM TCHR; M_JOBMST MJOB; M_FLRLOCA MFLOOR; M_DOCTYPE DOCTYP; M_SUBLEG Subleg;
        public ActionResult T_SORD(string op = "", string key = "", int Nindex = 0, string searchValue = "", string parkID = "", string loadOrder = "N")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    SalesOrderEntry VE = (parkID == "") ? new SalesOrderEntry() : (Improvar.ViewModels.SalesOrderEntry)Session[parkID];
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    switch (VE.DOC_CODE)
                    {
                        case "SORD": ViewBag.formname = "Sales Order Entry"; break;
                        case "PORD": ViewBag.formname = "Purchase Order Entry"; break;
                        case "SORDC": ViewBag.formname = "Sales Order Retail"; break;
                        default: ViewBag.formname = "Menupara not found in appl_menu"; break;
                    }                    
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    string COMP = CommVar.Compcd(UNQSNO);
                    string LOC = CommVar.Loccd(UNQSNO);
                    string yr1 = CommVar.YearCode(UNQSNO);

                    VE.DropDown_list = Master_Help.OTHER_REC_MODE(); VE.DocumentThrough = Master_Help.DocumentThrough();
                    //VE.DropDown_list5 = Master_Help.DISPATCH_THROUGH();
                    VE.CashOnDelivery = Master_Help.CashOnDelivery();

                    List<DropDown_list1> DDL = new List<DropDown_list1>();
                    VE.DropDown_list1 = DDL;

                    List<DropDown_list4> DDL4 = new List<DropDown_list4>();
                    VE.DropDown_list4 = DDL4;

                    VE.DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);

                    VE.DropDown_list_DelvType = Salesfunc.GetforDelvTypeSelection();

                    var MSYSCNFG = DB.M_SYSCNFG.OrderByDescending(t => t.EFFDT).FirstOrDefault();
                    VE.M_SYSCNFG = MSYSCNFG;

                    String YR1 = CommVar.YearCode(UNQSNO);
                    if (op.Length != 0)
                    {
                        string[] XYZ = VE.DocumentType.Select(i => i.value).ToArray();
                        VE.IndexKey = (from p in DB.T_SORD join q in DB.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO) where XYZ.Contains(p.DOCCD) && q.LOCCD == LOC && q.COMPCD == COMP select new IndexKey() { Navikey = p.AUTONO }).OrderBy(a => a.Navikey).ToList();
                        if (op == "E" || op == "D" || op == "V" || loadOrder == "Y")
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
                            VE.T_SORD = sl;
                            VE.M_SUBLEG = Subleg;
                            VE.T_CNTRL_HDR = sll;
                            VE.M_JOBMST = MJOB;
                            VE.M_FLRLOCA = MFLOOR;
                            VE.T_CNTRL_HDR_REM = TCHR;
                            VE.M_DOCTYPE = DOCTYP;
                        }
                        if (sll != null && sll.DOCNO != null) ViewBag.formname = ViewBag.formname + " (" + sll.DOCNO + ")";
                        if (op.ToString() == "A" && loadOrder == "N")
                        {
                            if (parkID == "")
                            {
                                List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                                UploadDOC UPL = new UploadDOC();
                                UPL.DocumentType = Cn.DOC_TYPE();
                                UploadDOC1.Add(UPL);
                                VE.UploadDOC = UploadDOC1;
                                List<TSORDDTL> TSORDDTL = new List<TSORDDTL>();
                                for (int i = 0; i <= 49; i++)
                                {
                                    TSORDDTL SORDDTL = new TSORDDTL();
                                    SORDDTL.SLNO = Convert.ToInt16(i + 1);
                                    SORDDTL.DropDown_list2 = Master_Help.STOCK_TYPE();
                                    SORDDTL.DropDown_list3 = Master_Help.FREE_STOCK();
                                    TSORDDTL.Add(SORDDTL);
                                }
                                VE.TSORDDTL = TSORDDTL;
                                //List<SCHEME_DTL> SCHEME_DTL = new List<SCHEME_DTL>(); for (int i = 0; i < 20; i++) { SCHEME_DTL SCHEMEDTL = new SCHEME_DTL(); SCHEME_DTL.Add(SCHEMEDTL); }
                                //VE.SCHEME_DTL = SCHEME_DTL;
                                //List<TSORDDTL_SEARCHPANEL> TSORDDTL_SEARCHPANEL = new List<TSORDDTL_SEARCHPANEL>(); for (int i = 0; i < 10; i++) { TSORDDTL_SEARCHPANEL SEARCHPANEL = new TSORDDTL_SEARCHPANEL(); SEARCHPANEL.SLNO = Convert.ToInt16(i + 1); TSORDDTL_SEARCHPANEL.Add(SEARCHPANEL); }
                                //VE.TSORDDTL_SEARCHPANEL = TSORDDTL_SEARCHPANEL;
                                T_SORD aaa = new Models.T_SORD(); aaa.DOCDT = System.DateTime.Now.Date; if (VE.DocumentType.Count > 0) { aaa.DOCCD = VE.DocumentType.First().value; }
                                string scmf = CommVar.FinSchema(UNQSNO); string scm = CommVar.CurSchema(UNQSNO);
                                string sql1 = "";
                                sql1 += " select a.rtdebcd,b.rtdebnm,b.mobile,a.inc_rate,a.retdebslcd,b.city,b.add1,b.add2,b.add3,effdt ";
                                sql1 += "  from  " + scm + ".M_SYSCNFG a, " + scmf + ".M_RETDEB b";
                                sql1 += " where a.RTDEBCD=b.RTDEBCD and a.effdt in(select max(effdt) effdt from  " + scm + ".M_SYSCNFG) ";
                                DataTable syscnfgdt = Master_Help.SQLquery(sql1);
                                if (syscnfgdt != null && syscnfgdt.Rows.Count > 0)
                                {
                                    aaa.RTDEBCD = syscnfgdt.Rows[0]["rtdebcd"].retStr();
                                    VE.RTDEBNM = syscnfgdt.Rows[0]["RTDEBNM"].retStr();
                                    var addrs = syscnfgdt.Rows[0]["add1"].retStr() + " " + syscnfgdt.Rows[0]["add2"].retStr() + " " + syscnfgdt.Rows[0]["add3"].retStr();
                                    VE.ADDR = addrs + "/" + syscnfgdt.Rows[0]["city"].retStr();
                                    VE.MOBILE = syscnfgdt.Rows[0]["MOBILE"].retStr();
                                    VE.RETDEBSLCD = syscnfgdt.Rows[0]["retdebslcd"].retStr();
                                }
                                VE.T_SORD = aaa;
                            }
                            else
                            {
                                if (VE.UploadDOC != null) { foreach (var i in VE.UploadDOC) { i.DocumentType = Cn.DOC_TYPE(); } }
                                INI INIF = new INI();
                                INIF.DeleteKey(Session["UR_ID"].ToString(), parkID, Server.MapPath("~/Park.ini"));
                            }
                            VE = (SalesOrderEntry)Cn.CheckPark(VE, VE.MENU_DETAILS, LOC, COMP, CommVar.CurSchema(UNQSNO), Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString());
                            VE.TSORDDTL.ForEach(A => A.DropDown_list2 = Master_Help.STOCK_TYPE());
                            VE.TSORDDTL.ForEach(A => A.DropDown_list3 = Master_Help.FREE_STOCK());
                        }
                        string docdt = "";
                        if (VE.T_CNTRL_HDR != null) if (VE.T_CNTRL_HDR.DOCDT != null) docdt = VE.T_CNTRL_HDR.DOCDT.ToString().Remove(10);
                        Cn.getdocmaxmindate(VE.T_SORD.DOCCD, docdt, VE.DefaultAction, VE.T_SORD.DOCNO, VE);


                        ModelState.Clear();
                        return View(VE);
                    }
                    else
                    {
                        VE.DefaultView = false;
                        VE.DefaultDay = 0;
                        return View(VE);
                    }
                }
            }
            catch (Exception ex)
            {
                SalesOrderEntry VE = new SalesOrderEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public SalesOrderEntry Navigation(SalesOrderEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            try
            {
                using (ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO)))
                {
                    using (ImprovarDB DB_I = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema))
                    {
                        using (DB)
                        {
                            sl = new T_SORD(); sll = new T_CNTRL_HDR(); TCHR = new T_CNTRL_HDR_REM(); MJOB = new M_JOBMST(); MFLOOR = new M_FLRLOCA(); Subleg = new M_SUBLEG();
                            string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
                            string SCM = CommVar.CurSchema(UNQSNO);
                            double? TOTAL_BOXES = 0; double? TOTAL_QNTY = 0; double? TOTAL_SETS = 0;
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
                                sl = DB.T_SORD.Find(aa[0].Trim());
                                Subleg = DBF.M_SUBLEG.Find(sl.SLCD);
                                sll = DB.T_CNTRL_HDR.Find(sl.AUTONO);
                                DOCTYP = DB.M_DOCTYPE.Find(sl.DOCCD);
                                TCHR = Cn.GetTransactionReamrks(CommVar.CurSchema(UNQSNO), sl.AUTONO);
                                VE.UploadDOC = Cn.GetUploadImageTransaction(CommVar.CurSchema(UNQSNO), sl.AUTONO);
                                if (sl != null)
                                {
                                    VE.OTHRECMD = sl.RECMODE;
                                    var Party = DBF.M_SUBLEG.Find(sl.SLCD); if (Party != null) { VE.PartyName = Party.SLNM; VE.PARTYCD = sl.SLCD; VE.PARTYNM = Party.SLNM; VE.DistrictName = Party.DISTRICT; }
                                    var SalesMen = DBF.M_SUBLEG.Find(sl.SLMSLCD); if (SalesMen != null) { VE.SalesManName = SalesMen.SLNM; }
                                    var Agent = DBF.M_SUBLEG.Find(sl.AGSLCD); if (Agent != null) { VE.AgentName = Agent.SLNM; VE.AGTCD = sl.AGSLCD; VE.AGTNM = Agent.SLNM; }
                                    var Transporter = DBF.M_SUBLEG.Find(sl.TRSLCD); if (Transporter != null) { VE.TransporterName = Transporter.SLNM; VE.TRNSCD = sl.TRSLCD; VE.TRNSNM = Transporter.SLNM; }
                                    if (sl.CONSLCD != null)
                                    {
                                        VE.CONSLNM = DBF.M_SUBLEG.Find(sl.CONSLCD).SLNM;
                                    }
                                    if (sl.SAGSLCD != null)
                                    {
                                        VE.SAGSLNM = DBF.M_SUBLEG.Find(sl.SAGSLCD).SLNM;
                                    }
                                    string scmf = CommVar.FinSchema(UNQSNO); string scm = CommVar.CurSchema(UNQSNO);
                                    if (VE.MENU_PARA == "SBCM" && sl.RTDEBCD!=null)
                                    {
                                        string sql1 = "";
                                        sql1 += " select a.rtdebcd,b.rtdebnm,b.mobile,a.inc_rate,a.retdebslcd,b.city,b.add1,b.add2,b.add3,effdt ";
                                        sql1 += "  from  " + scm + ".M_SYSCNFG a, " + scmf + ".M_RETDEB b";
                                        sql1 += " where a.RTDEBCD=b.RTDEBCD and a.rtdebcd='" + sl.RTDEBCD + "' and a.effdt in(select max(effdt) effdt from  " + scm + ".M_SYSCNFG)  ";
                                        DataTable syscnfgdt = Master_Help.SQLquery(sql1);
                                        if (syscnfgdt != null && syscnfgdt.Rows.Count > 0)
                                        {
                                            VE.RTDEBNM = syscnfgdt.Rows[0]["RTDEBNM"].retStr();
                                            var addrs = syscnfgdt.Rows[0]["add1"].retStr() + " " + syscnfgdt.Rows[0]["add2"].retStr() + " " + syscnfgdt.Rows[0]["add3"].retStr();
                                            VE.ADDR = addrs + "/" + syscnfgdt.Rows[0]["city"].retStr();
                                            VE.MOBILE = syscnfgdt.Rows[0]["MOBILE"].retStr();
                                            VE.RETDEBSLCD = syscnfgdt.Rows[0]["retdebslcd"].retStr();
                                        }
                                    }
                                       
                                    if (VE.DefaultAction == "V")
                                    {
                                        string str = "";
                                        str += " select distinct b.doccd, b.docno, b.docdt, b.autono, ";
                                        str += " b.usr_id sb_madeby_id, c.user_name sb_madeby_name, b.usr_entdt sb_madeby_dt ";
                                        str += " from " + CommVar.CurSchema(UNQSNO) + ".t_txndtl a, " + CommVar.CurSchema(UNQSNO) + ".t_cntrl_hdr b, user_appl c ";
                                        str += " where a.autono = b.autono and a.AGDOCNO = '" + sl.AUTONO + "' and b.usr_id = c.user_id(+) ";
                                        DataTable dtbl = Master_Help.SQLquery(str);
                                        VE.TSORDDTL_SEARCHPANEL = (from DataRow dr1 in dtbl.Rows
                                                                   select new TSORDDTL_SEARCHPANEL()
                                                                   {
                                                                       DOCCD = dr1["doccd"].retStr(),
                                                                       DOCNO = dr1["docno"].retStr(),
                                                                       DOCDT = dr1["docdt"].retStr().Substring(0, 10),
                                                                       AUTONO = dr1["autono"].retStr(),
                                                                       SB_MADEBY_ID = dr1["sb_madeby_id"].retStr(),
                                                                       SB_MADEBY_NAME = dr1["sb_madeby_name"].retStr(),
                                                                       SB_MADEDT = dr1["sb_madeby_dt"].retStr()
                                                                   }).OrderBy(s => s.AUTONO).ToList();
                                    }


                                    string str1 = "";
                                    //str1 = "select ROW_NUMBER() OVER(ORDER BY j.styleno) AS SLNO, i.AUTONO, i.STKDRCR, i.STKTYPE, i.FREESTK, i.ITCD, j.ITNM, j.STYLENO, j.PCSPERBOX, j.PCSPERSET, j.UOMCD, sum(i.SCMDISCAMT) SCMDISCAMT, sum(i.DISCAMT)DISCAMT, sum(I.QNTY) QNTY ";
                                    //str1 += " from " + CommVar.CurSchema(UNQSNO) + ".T_SORDDTL i, " + CommVar.CurSchema(UNQSNO) + ".M_SITEM j  where i.ITCD = j.ITCD(+)   and i.AUTONO = '" + sl.AUTONO + "' ";
                                    //str1 += " group by i.AUTONO, i.STKDRCR, i.STKTYPE, i.FREESTK, i.ITCD, j.ITNM, j.STYLENO, j.PCSPERBOX, j.PCSPERSET, j.UOMCD ";

                                    str1 += "select a.SLNO,a.AUTONO, a.STKDRCR, a.STKTYPE, a.FREESTK, a.ITCD, c.ITNM, c.STYLENO, c.PCSPERSET, c.UOMCD, ";
                                    str1 += "a.sizecd, a.rate, a.scmdiscamt, a.discamt, a.qnty,A.DELVDT,a.ITREM,a.PDESIGN,c.itgrpcd, d.itgrpnm,c.fabitcd, ";
                                    str1 += "e.itnm fabitnm,a.colrcd,a.partcd,f.colrnm,g.sizenm,h.partnm,a.rate from ";
                                    str1 += SCM + ".T_SORDDTL a, " + SCM + ".T_CNTRL_HDR b, ";
                                    str1 += SCM + ".m_sitem c, " + SCM + ".m_group d, " + SCM + ".m_sitem e, " + SCM + ".m_color f, " + SCM + ".m_size g, " + SCM + ".m_parts h ";
                                    str1 += "where a.autono = b.autono(+) and a.itcd = c.itcd(+) and c.itgrpcd=d.itgrpcd and c.fabitcd=e.itcd(+) ";
                                    str1 += "and a.colrcd=f.colrcd(+) and a.sizecd=g.sizecd(+) and a.partcd=h.partcd(+) and a.autono='"+sl.AUTONO+"' ";
                                    str1 += "order by styleno ";

                                    DataTable tbl = Master_Help.SQLquery(str1);
                                    VE.TSORDDTL = (from DataRow dr in tbl.Rows
                                                   select new TSORDDTL()
                                                   {
                                                       AUTONO = dr["AUTONO"].retStr(),
                                                       SLNO = dr["SLNO"].retInt(),
                                                       STKDRCR = dr["STKDRCR"].retStr(),
                                                       STKTYPE = dr["STKTYPE"].retStr(),
                                                       FREESTK = dr["FREESTK"].retStr(),
                                                       ITCD = dr["ITCD"].retStr(),
                                                       ITNM = dr["ITNM"].retStr(),
                                                       STYLENO = dr["STYLENO"].retStr(),
                                                       //TOTAL_PCS = dr["PCSPERBOX"].retDbl(),
                                                       PCSPERSET = dr["PCSPERSET"].retDbl(),
                                                       UOM = dr["UOMCD"].retStr(),
                                                       QNTY = dr["QNTY"].retDbl(),
                                                       DELVDT = dr["DELVDT"].retStr() == "" ? (DateTime?)null : Convert.ToDateTime(dr["DELVDT"].retDateStr()),
                                                       ITREM = dr["ITREM"].ToString(),
                                                       PDESIGN = dr["PDESIGN"].ToString(),
                                                       ITGRPCD = dr["ITGRPCD"].ToString(),
                                                       ITGRPNM = dr["ITGRPNM"].ToString(),
                                                       FABITCD = dr["FABITCD"].ToString(),
                                                       FABITNM = dr["FABITNM"].ToString(),
                                                       COLRCD = dr["COLRCD"].ToString(),
                                                       COLRNM = dr["COLRNM"].ToString(),
                                                       SIZECD = dr["SIZECD"].ToString(),
                                                       SIZENM = dr["SIZENM"].ToString(),
                                                       PARTCD = dr["PARTCD"].ToString(),
                                                       PARTNM = dr["PARTNM"].ToString(),
                                                       RATE = dr["RATE"].retDbl(),
                                                   }).OrderBy(s => s.SLNO).ToList();
                                    double tqty = 0;
                                    foreach(var v in VE.TSORDDTL)
                                    { tqty = tqty + v.QNTY.retDbl(); }
                                    VE.TOTAL_QNTY = tqty;
                                    if (VE.DefaultAction == "E")
                                    {
                                        int ROW_COUNT = 0;
                                        List<TSORDDTL> TSORDDTL = new List<TSORDDTL>();
                                        if (VE.TSORDDTL != null && VE.TSORDDTL.Count > 0)
                                        {
                                            for (int i = 0; i <= VE.TSORDDTL.Count - 1; i++)
                                            {
                                                if (VE.TSORDDTL[i].ITCD != null)
                                                {
                                                    ROW_COUNT = ROW_COUNT + 1;
                                                    TSORDDTL DTL = new TSORDDTL();
                                                    DTL = VE.TSORDDTL[i];
                                                    TSORDDTL.Add(DTL);
                                                }
                                            }
                                            int TOTAL_COUNT = 50 - ROW_COUNT;
                                            for (int j = 0; j <= TOTAL_COUNT - 1; j++)
                                            {
                                                ROW_COUNT = ROW_COUNT + 1;
                                                TSORDDTL SORDDTL = new TSORDDTL();
                                                SORDDTL.SLNO = ROW_COUNT;
                                                SORDDTL.DropDown_list2 = Master_Help.STOCK_TYPE();
                                                SORDDTL.DropDown_list3 = Master_Help.FREE_STOCK();
                                                TSORDDTL.Add(SORDDTL);
                                            }
                                            VE.TSORDDTL = TSORDDTL;
                                        }
                                    }

                                    //VE.TOTAL_BOXES = TOTAL_BOXES.Value;
                                    //VE.TOTAL_QNTY = TOTAL_QNTY.Value;
                                    //VE.TOTAL_SETS = TOTAL_SETS.Value;
                                    VE.T_CNTRL_HDR = sll;
                                    if (VE.T_CNTRL_HDR.CANCEL == "Y") VE.CancelRecord = true; else VE.CancelRecord = false;
                                    VE.Edit_Tag = true;
                                    if (sll.YR_CD != CommVar.YearCode(UNQSNO))
                                    {
                                        VE.Edit = ""; VE.Delete = "";
                                    }

                                }
                                else
                                {

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
            }
            return VE;
        }
        public ActionResult SearchPannelData(SalesOrderEntry VE, string SRC_SLCD, string SRC_DOCNO, string SRC_FDT, string SRC_TDT)
        {
            Cn.getQueryString(VE);
            string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), yr_code = CommVar.YearCode(UNQSNO);
            VE.DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
            string doccd = VE.DocumentType.Select(i => i.value).ToArray().retSqlfromStrarray();
            string sql = "select  a.autono, a.doccd, c.docno, to_char(a.docdt,'dd/mm/yyyy') docdt, to_char(e.DELVDT,'dd/mm/yyyy') DELVDT, ";
            sql += "c.docdt ddocdt, a.slcd, d.slnm, d.district, a.prefno, 0 tbox,a.aproxval,sum(e.QNTY)QNTY,e.ITCD,f.ITCD,f.STYLENO,f.ITNM,a.PREFDT ";
            sql += " from " + scm + ".t_sord a, " + scm + ".t_cntrl_hdr c, " + scmf + ".m_subleg d, " + scm + ".t_sorddtl e, " + scm + ".M_SITEM f ";
            sql += "where a.autono = c.autono and a.autono = e.autono and e.ITCD = f.ITCD and c.compcd = '" + COM + "' and c.loccd = '" + LOC + "' and ";
            if (SRC_FDT.retStr() != "") sql += "c.docdt >= to_date('" + SRC_FDT.retDateStr() + "','dd/mm/yyyy') and ";
            if (SRC_TDT.retStr() != "") sql += "c.docdt <= to_date('" + SRC_TDT.retDateStr() + "','dd/mm/yyyy') and ";
            if (SRC_DOCNO.retStr() != "") sql += "(c.vchrno like '%" + SRC_DOCNO.retStr() + "%' or c.docno like '%" + SRC_DOCNO.retStr() + "%') and ";
            if (SRC_SLCD.retStr() != "") sql += "(a.slcd like '%" + SRC_SLCD.retStr() + "%' or upper(d.slnm) like '%" + SRC_SLCD.retStr().ToUpper() + "%') and ";
            sql += "a.slcd = d.slcd and c.yr_cd='" + CommVar.YearCode(UNQSNO) + "' and c.doccd in(" + doccd + ") ";
            sql += "group by  a.autono, a.doccd, c.docno, to_char(a.docdt,'dd/mm/yyyy') , ";
            sql += "c.docdt , a.slcd, d.slnm, d.district, a.prefno, 0 ,a.aproxval,e.ITCD,f.ITCD,f.STYLENO,f.ITNM,a.PREFDT,e.DELVDT ";
            sql += "order by c.docdt, docno ";
            var tbl = Master_Help.SQLquery(sql);

            System.Text.StringBuilder SB = new System.Text.StringBuilder(); 
             var hdr = "Order Number" + Cn.GCS() + "Order Date" + Cn.GCS() + "Party Ref Number" + Cn.GCS() + "Party Ref Date" + Cn.GCS() + "Delivery Date" + Cn.GCS() 
                + "Party Name"+ Cn.GCS() + "Order Approx Value" + Cn.GCS() + "Design Number" + Cn.GCS() + "Total Quantity"  + Cn.GCS() + "AUTO NO";
            for (int j = 0; j <= tbl.Rows.Count - 1; j++)
            {                
                SB.Append("<tr><td>" + tbl.Rows[j]["docno"] + "</td><td>" + tbl.Rows[j]["docdt"] + " </td><td>" + tbl.Rows[j]["prefno"] + " </td><td>"
                    + tbl.Rows[j]["PREFDT"].retDateStr() + " </td><td>" + tbl.Rows[j]["DELVDT"] + " </td><td><b>" + tbl.Rows[j]["slnm"] + "</b> ["+ tbl.Rows[j]["district"] + "]  (" + tbl.Rows[j]["slcd"] + ") "
                    + " </td><td>" + tbl.Rows[j]["aproxval"] + " </td><td>" + tbl.Rows[j]["STYLENO"] + " " + tbl.Rows[j]["ITNM"] + " </td><td>" + tbl.Rows[j]["QNTY"] + "</td><td>"
                    + tbl.Rows[j]["autono"] + " </td></tr>");
            }
            return PartialView("_SearchPannel2", Master_Help.Generate_SearchPannel(hdr, SB.ToString(), "9", "9"));
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
        public ActionResult GetArticleItem(SalesOrderEntry VE, string val, string RowIndex)
        {
            try
            {
                string ITGTYPE = "F";
                if (val == null)
                {
                    return PartialView("_Help2", Master_Help.ITCD_help(val, ITGTYPE));
                }
                else
                {
                    int slno = RowIndex.retInt() + 1;
                    var MAINROW = (from H in VE.TSORDDTL where H.SLNO == slno select H).First();
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    var itcd_db = DB.M_SITEM.Where(r => r.STYLENO == MAINROW.ARTNO).FirstOrDefault()?.ITCD;
                    if (string.IsNullOrEmpty(MAINROW.ITCD))
                    {
                        if (string.IsNullOrEmpty(itcd_db))
                        {
                            return Content("INVALID ARTICLE,Reselect Article");
                        }
                        else
                        {
                            return Content(Master_Help.ITCD_help(MAINROW.ARTNO, ITGTYPE) + Cn.GCS() + "");
                        }
                    }
                    else
                    {
                        if (MAINROW.ITCD == itcd_db)
                        {
                            return Content(Master_Help.ITCD_help(MAINROW.ARTNO, ITGTYPE) + Cn.GCS() + "");
                        }
                        else if (string.IsNullOrEmpty(itcd_db))
                        {
                            return Content(Master_Help.ITCD_help(MAINROW.ITCD, ITGTYPE) + Cn.GCS() + "STAYOLD ARTICLE");
                        }
                        else
                        {
                            return Content(Master_Help.ITCD_help(MAINROW.ARTNO, ITGTYPE) + Cn.GCS() + "ITCDCHANGED");
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content("Error Item Selection.");
            }
        }
        public ActionResult CheckArticle(SalesOrderEntry VE, string ART_NO, string STK_TYPE, string FREE_STK, string SLNO)
        {
            if (VE.DefaultAction == "V") return Content("0" + Cn.GCS() + "");
            try
            {
                var CHECK_DATA = (from X in VE.TSORDDTL where X.ARTNO == ART_NO && X.STKTYPE == STK_TYPE && X.FREESTK == FREE_STK && X.SLNO != Convert.ToInt16(SLNO) select X).ToList();
                if (CHECK_DATA != null && CHECK_DATA.Count > 0)
                {
                    ViewBag.msgDuplicateitem = CHECK_DATA[0].SLNO - 1;
                    return Content("Article already exist in Sl.No. " + CHECK_DATA[0].SLNO.ToString() + " with same Stock type !!" + Cn.GCS() + (CHECK_DATA[0].SLNO - 1));
                }
                else
                {
                    return Content("0" + Cn.GCS() + "");
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetBrandDetails(string val)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            if (val == null)
            {
                return PartialView("_Help2", Master_Help.TRNS_BRAND(DB, val));
            }
            else
            {
                string str = Master_Help.TRNS_BRAND(DB, val);
                return Content(str);
            }
        }
        public ActionResult GetPartyDetails(string val, string Code, string BRAND)
        {
            if (val == null)
            {
                return PartialView("_Help2", Master_Help.SUBLEDGER(val, "D"));
            }
            else
            {
                string str = Master_Help.SUBLEDGER(val, "D");
                var DATA = str.Split(Convert.ToChar(Cn.GCS()));
                double latedays = Salesfunc.retMaxLateDays(null, val, true);
                string latedaysdsp = "";
                if (latedays > 60) latedaysdsp = "Late by " + latedays.ToString();
                try
                {
                    var str1 = DATA[0] + Cn.GCS() + DATA[1] + Cn.GCS() + Master_Help.PRICELIST("", val, "FORPARTY", BRAND) + Cn.GCS() + DATA[2] + Cn.GCS() + latedays.ToString() + Cn.GCS() + latedaysdsp;

                    return Content(str1);
                }
                catch (Exception ex)
                {
                    Cn.SaveException(ex, "");
                    return Content("0");
                }
            }
        }
        public ActionResult GetEffectiveDate(SalesOrderEntry VE, string PRC_CD, string DOC_DT)
        {
            VE.DropDown_list1 = Master_Help.EFFECTIVE_DATE(PRC_CD, DOC_DT);
            return Json(VE.DropDown_list1, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetDiscEffectiveDate(SalesOrderEntry VE, string DISC_CD, string DOC_DT)
        {
            VE.DropDown_list4 = Master_Help.DISC_EFFECTIVE_DATE(DISC_CD, DOC_DT);
            return Json(VE.DropDown_list4, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetSchemeData(SalesOrderEntry VE, string PARTY = "", string BRAND = "", string DATE = "")
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            var SCHEME_DATA = Scheme_Cal.GetSchmeCode(DATE, PARTY, BRAND);
            if (SCHEME_DATA != null)
            {
                VE.SCHEME_DTL = (from DataRow DR in SCHEME_DATA.Rows
                                 select new SCHEME_DTL
                                 {
                                     SCMCODE = DR["SCMCD"].ToString(),
                                     SCMNAME = DR["SCMNM"].ToString(),
                                     SCMDATE = DR["SCMDT"].ToString(),
                                     SCMTYPE = DR["SCMTYPE"].ToString(),
                                 }).ToList();
            }
            VE.SCHEME_DTL.ForEach(A => A.Checked = true);
            VE.DefaultView = true;
            return PartialView("_T_SORD_SCHEME", VE);
        }
        public ActionResult GetTransporterDetails(string val)
        {
            //string SQL = "select b.slcd,a.slnm from " + Session["FINSCHEMA"].ToString() + ".m_subleg a ," + DB.CacheKey.ToString() + ".M_SUBLEG_SDDTL b where a.slcd=b.TRSLCD ";
            if (val == null)
            {
                return PartialView("_Help2", Master_Help.SUBLEDGER(val, "T"));
            }
            else
            {
                string str = Master_Help.SUBLEDGER(val, "T");
                return Content(str);
            }
        }
        public ActionResult GetCourierDetails(string val)
        {
            if (val == null)
            {
                return PartialView("_Help2", Master_Help.SUBLEDGER(val, "U"));
            }
            else
            {
                string str = Master_Help.SUBLEDGER(val, "U");
                return Content(str);
            }
        }
        public ActionResult GetAgentDetails(string val)
        {
            if (val == null)
            {
                return PartialView("_Help2", Master_Help.SUBLEDGER(val, "A"));
            }
            else
            {
                string str = Master_Help.SUBLEDGER(val, "A");
                return Content(str);
            }
        }
        public ActionResult DOCNO_ORDERhelp(string Code, string val)
        {
            if (val == null)
            {
                return PartialView("_Help2", Master_Help.DOCNO_ORDER_help(Code, val));
            }
            else
            {
                string str = Master_Help.DOCNO_ORDER_help(Code, val);
                return Content(str);
            }
        }
        public ActionResult GetSalesMenDetails(string val)
        {
            if (val == null)
            {
                return PartialView("_Help2", Master_Help.SUBLEDGER(val, "M"));
            }
            else
            {
                string str = Master_Help.SUBLEDGER(val, "M");
                return Content(str);
            }
        }
        public ActionResult GetPriceListDetails(string val, string Code)
        {
            if (Code == "")
            {
                return Content("Party Not Selected !!  <br/>  Please Enter / Select a Valid Party to Get Price List Details !!");
            }
            else
            {
                if (val == null)
                {
                    return PartialView("_Help2", Master_Help.PRICELIST(val, Code, "", ""));
                }
                else
                {
                    string str = Master_Help.PRICELIST(val, Code, "", "");
                    return Content(str);
                }
            }
        }
        //public ActionResult GetPayTermsDetails(string val)
        //{
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
        //    if (val == null)
        //    {
        //        return PartialView("_Help2", Master_Help.PAY_TERMS(DB, val));
        //    }
        //    else
        //    {
        //        string str = Master_Help.PAY_TERMS(DB, val);
        //        return Content(str);
        //    }
        //}
        public ActionResult AddDOCRow(SalesOrderEntry VE)
        {
            string SCHEMA = Cn.Getschema;
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), SCHEMA);
            var doctP = (from i in DB1.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();
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
        public ActionResult DeleteDOCRow(SalesOrderEntry VE)
        {
            string SCHEMA = Cn.Getschema;
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), SCHEMA);
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
        public ActionResult AddRow(SalesOrderEntry VE, int COUNT, string TAG)
        {
            using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
            {
                if (VE.TSORDDTL == null)
                {
                    List<TSORDDTL> TSORDDTL1 = new List<TSORDDTL>();
                    TSORDDTL SORDDTL = new TSORDDTL();
                    int maxr = 1;
                    if (TAG == "Y" && COUNT > 0) maxr = COUNT;
                    for (Int16 q = 1; q <= maxr; q++)
                    {
                        SORDDTL.SLNO = q;
                        TSORDDTL1.Add(SORDDTL);
                    }
                    VE.TSORDDTL = TSORDDTL1;
                }
                else
                {
                    List<TSORDDTL> SORDDTL = new List<TSORDDTL>();
                    for (int i = 0; i <= VE.TSORDDTL.Count - 1; i++)
                    {
                        TSORDDTL ORDDTL = new TSORDDTL();
                        ORDDTL = VE.TSORDDTL[i];
                        SORDDTL.Add(ORDDTL);
                    }
                    TSORDDTL ORDDTL1 = new TSORDDTL();
                    var max = VE.TSORDDTL.Max(a => Convert.ToInt32(a.SLNO));
                    int SLNO = Convert.ToInt32(max) + 1;

                    int maxr = 1;
                    if (TAG == "Y" && COUNT > 0) maxr = COUNT;
                    for (Int16 q = 1; q <= maxr; q++)
                    {
                        max = max + 1;
                        TSORDDTL ORDDTL = new TSORDDTL();
                        ORDDTL.SLNO = max;
                        SORDDTL.Add(ORDDTL);
                    }
                    VE.TSORDDTL = SORDDTL;
                }
                VE.TSORDDTL.ForEach(A => A.DropDown_list2 = Master_Help.STOCK_TYPE());
                VE.TSORDDTL.ForEach(A => A.DropDown_list3 = Master_Help.FREE_STOCK());
                VE.DefaultView = true;
                return PartialView("_T_SORD_MAIN", VE);
            }
        }
        public ActionResult DeleteRow(SalesOrderEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            List<TSORDDTL> SORDDTL = new List<TSORDDTL>();
            int count = 0;
            for (int i = 0; i <= VE.TSORDDTL.Count - 1; i++)
            {
                if (VE.TSORDDTL[i].Checked == false)
                {
                    count += 1;
                    TSORDDTL item = new TSORDDTL();
                    item = VE.TSORDDTL[i];
                    item.SLNO = count;
                    SORDDTL.Add(item);
                }
            }
            VE.TSORDDTL = SORDDTL;
            VE.TSORDDTL.ForEach(A => A.DropDown_list2 = Master_Help.STOCK_TYPE());
            VE.TSORDDTL.ForEach(A => A.DropDown_list3 = Master_Help.FREE_STOCK());
            ModelState.Clear();
            VE.DefaultView = true;
            return PartialView("_T_SORD_MAIN", VE);

        }
        public ActionResult GOTO_INVOICE(SalesOrderEntry VE, string AUTONO, string DCODE)
        {
            try
            {
                Cn.getQueryString(VE);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                var DOCD = DB.T_CNTRL_HDR.Find(AUTONO);
                var Doctp = DB.M_DOCTYPE.Find(DOCD.DOCCD);
                var DCD = DB.M_DTYPE.Find(Doctp.DOCTYPE);
                string BackupDOCD = DCD.DCD;
                string autoEntryWork = "N";
                if (DCD.MENU_PROGCALL == null)
                {
                    autoEntryWork = "Y";
                    DCD.MENU_PROGCALL = "T_SALE";
                    DCD.MENU_PARA = VE.MENU_PARA;
                }
                string sql = "select * from appl_menu where MENU_PROGCALL='" + DCD.MENU_PROGCALL + "' and MENU_PARA='" + DCD.MENU_PARA + "' and MENU_DOCCD='" + Doctp.DOCTYPE + "'";
                DataTable tbl = Master_Help.SQLquery(sql);
                string MNUDET = tbl.Rows[0]["MENU_ID"].ToString() + "~"
                                 + tbl.Rows[0]["MENU_INDEX"].ToString() + "~"
                                 + tbl.Rows[0]["MENU_PROGCALL"].ToString() + "~"
                                 + tbl.Rows[0]["menu_date_option"].ToString() + "~"
                                 + tbl.Rows[0]["menu_type"].ToString() + "~"
                                 + "0" + "~"
                                 + "0" + "~"
                                 + "AEV";
                string US = VE.UNQSNO_ENCRYPTED;
                string DC = tbl.Rows[0]["MENU_DOCCD"].ToString();
                string MP = tbl.Rows[0]["MENU_PARA"].ToString();

                string MyURL = tbl.Rows[0]["MENU_PROGCALL"].ToString() + "/" + tbl.Rows[0]["MENU_PROGCALL"].ToString() + "?op=V" + "&MNUDET="
                    + Cn.Encrypt_URL(MNUDET) + "&US=" + US
                    + "&DC=" + Cn.Encrypt_URL(DC)
                    + "&MP=" + Cn.Encrypt_URL(MP) + "&searchValue=" + AUTONO
                    + "&ThirdParty=yes~" + Doctp.DOCCD + "~" + Doctp.DOCNM.Replace("&", "$$$$$$$$") + "~" + Doctp.PRO + "~" + autoEntryWork;
                //  var ty= "&MNUDET=" + MENU_PERMISSION + "&US=" + usno + "&DC=" + DC + "&MP=" + MP
                //var ty="b.MENU_ID||'~'||b.MENU_INDEX||'~'||nvl(b.MENU_PROGCALL,'Notset')||'~'||b.menu_date_option||'~'||b.menu_type||'~'||c.E_DAY||'~'||c.D_DAY as MENU_PERMISSIONID,";
                return Content(MyURL);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
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
        public ActionResult SAVE(FormCollection FC, SalesOrderEntry VE)
        {
            //Oracle Queries
            string dberrmsg = "";
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            OracleTransaction OraTrans;
            string dbsql = "", sql = "";
            string[] dbsql1;

            OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted);
            OraCmd.Transaction = OraTrans;
            //
            using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
            {
                using (var transaction = DB.Database.BeginTransaction())
                {
                    try
                    {
                        Cn.getQueryString(VE);
                        //DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode");
                        //DB.Configuration.ValidateOnSaveEnabled = false;
                        OraCmd.CommandText = "lock table " + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode"; OraCmd.ExecuteNonQuery();

                        if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                        {
                            T_SORD TSORD = new T_SORD();

                            T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                            string DOCPATTERN = "";
                            TSORD.DOCDT = VE.T_SORD.DOCDT;
                            string Ddate = Convert.ToString(TSORD.DOCDT);
                            TSORD.CLCD = CommVar.ClientCode(UNQSNO);
                            string auto_no = ""; string Month = "";
                            if (VE.DefaultAction == "A")
                            {
                                TSORD.EMD_NO = 0;
                                TSORD.DOCCD = VE.T_SORD.DOCCD;
                                TSORD.DOCNO = Cn.MaxDocNumber(TSORD.DOCCD, Ddate);
                                DOCPATTERN = Cn.DocPattern(Convert.ToInt32(TSORD.DOCNO), TSORD.DOCCD, CommVar.CurSchema(UNQSNO), CommVar.FinSchema(UNQSNO), Ddate);
                                auto_no = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), TSORD.DOCNO, TSORD.DOCCD, Ddate);
                                TSORD.AUTONO = auto_no.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
                                Month = auto_no.Split(Convert.ToChar(Cn.GCS()))[1].ToString();
                            }
                            else
                            {
                                TSORD.DOCCD = VE.T_SORD.DOCCD;
                                TSORD.DOCNO = VE.T_SORD.DOCNO;
                                TSORD.AUTONO = VE.T_SORD.AUTONO;
                                Month = VE.T_CNTRL_HDR.MNTHCD;
                                TSORD.EMD_NO = Convert.ToInt16((VE.T_CNTRL_HDR.EMD_NO == null ? 0 : VE.T_CNTRL_HDR.EMD_NO) + 1);
                            }
                            TSORD.SLCD = VE.T_SORD.SLCD;
                            TSORD.AGSLCD = VE.T_SORD.AGSLCD;
                            TSORD.TRSLCD = VE.T_SORD.TRSLCD;
                            TSORD.SLMSLCD = VE.T_SORD.SLMSLCD;
                            TSORD.PREFNO = VE.T_SORD.PREFNO;
                            TSORD.PREFDT = VE.T_SORD.PREFDT;
                            TSORD.RECMODE = VE.OTHRECMD;
                            TSORD.COD = VE.T_SORD.COD;
                            TSORD.DELVINS = VE.T_SORD.DELVINS;
                            TSORD.DUEDAYS = VE.T_SORD.DUEDAYS;
                            TSORD.DOCTH = VE.T_SORD.DOCTH;
                            TSORD.APROXVAL = VE.T_SORD.APROXVAL;
                            TSORD.SPLNOTE = VE.T_SORD.SPLNOTE;
                            TSORD.RATEPER = VE.T_SORD.RATEPER;
                            TSORD.CONSLCD = VE.T_SORD.CONSLCD;
                            TSORD.SAGSLCD = VE.T_SORD.SAGSLCD;
                            TSORD.ORDBY = VE.T_SORD.ORDBY;
                            TSORD.SELBY = VE.T_SORD.SELBY;
                            TSORD.PAYTRMS = VE.T_SORD.PAYTRMS;
                           if(VE.MENU_PARA=="SBCM") TSORD.RTDEBCD = VE.T_SORD.RTDEBCD;
                            
                            if (VE.DefaultAction == "E")
                            {
                                TSORD.DTAG = "E";
                                //dbsql = MasterHelpFa.TblUpdt("t_sord_scheme", TSORD.AUTONO, "E");
                                //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                                dbsql = MasterHelpFa.TblUpdt("t_sorddtl", TSORD.AUTONO, "E");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                                dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_rem", TSORD.AUTONO, "E");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                                dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_doc", TSORD.AUTONO, "E");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                                dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_doc_dtl", TSORD.AUTONO, "E");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            }
                            //dbsql = MasterHelpFa.T_Cntrl_Hdr_Updt_Ins(TSORD.AUTONO, VE.DefaultAction, "S", Month, TSORD.DOCCD, DOCPATTERN, TSORD.DOCDT.retStr(), TSORD.EMD_NO.retShort(), TSORD.DOCNO, Convert.ToDouble(TSORD.DOCNO), null, null, null, TSORD.SLCD);
                            dbsql = MasterHelpFa.T_Cntrl_Hdr_Updt_Ins(TSORD.AUTONO, VE.DefaultAction, "S", Month, TSORD.DOCCD, DOCPATTERN, TSORD.DOCDT.retStr(), TSORD.EMD_NO.retShort(), TSORD.DOCNO, Convert.ToDouble(TSORD.DOCNO), null, null, null, TSORD.SLCD,0,VE.Audit_REM);
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            dbsql = MasterHelpFa.RetModeltoSql(TSORD, VE.DefaultAction);
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                            int COUNTER = 0;
                            //var DO_DATA = (from z in DB.T_DODTL where z.ORDAUTONO == TSORD.AUTONO select z).DistinctBy(A => A.AUTONO).ToList();
                            //string DO_AUTONO = "";
                            //if (DO_DATA != null && DO_DATA.Count > 0 && VE.DefaultAction == "E")
                            //{
                            //    DO_AUTONO = DO_DATA[0].AUTONO;
                            //    dbsql = MasterHelpFa.TblUpdt("t_dodtl", DO_AUTONO, "E");
                            //    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            //}
                            for (int i = 0; i <= VE.TSORDDTL.Count - 1; i++)
                            {
                                if (VE.TSORDDTL[i].SLNO != 0 && VE.TSORDDTL[i].ITCD != null)
                                {
                                    COUNTER = COUNTER + 1;

                                    T_SORDDTL TSORDDTL = new T_SORDDTL();
                                    TSORDDTL.EMD_NO = TSORD.EMD_NO;
                                    TSORDDTL.CLCD = TSORD.CLCD;
                                    TSORDDTL.DTAG = TSORD.DTAG;
                                    TSORDDTL.TTAG = TSORD.TTAG;
                                    TSORDDTL.AUTONO = TSORD.AUTONO;
                                    TSORDDTL.SLNO = Convert.ToInt16(COUNTER);
                                    TSORDDTL.BLSLNO = Convert.ToInt16(COUNTER);
                                    if (VE.MENU_PARA == "SB")
                                    {
                                        TSORDDTL.STKDRCR = "D";
                                    }
                                    else
                                    {
                                        TSORDDTL.STKDRCR = "C";
                                    }
                                    TSORDDTL.STKTYPE = "F";//"T";// VE.TSORDDTL[i].STKTYPE;
                                    TSORDDTL.FREESTK = VE.TSORDDTL[i].FREESTK;
                                    TSORDDTL.ITCD = VE.TSORDDTL[i].ITCD;
                                    TSORDDTL.SIZECD = VE.TSORDDTL[i].SIZECD;
                                    TSORDDTL.COLRCD = VE.TSORDDTL[i].COLRCD;
                                    TSORDDTL.PARTCD = VE.TSORDDTL[i].PARTCD;
                                    TSORDDTL.QNTY = VE.TSORDDTL[i].QNTY;
                                    TSORDDTL.RATE = VE.TSORDDTL[i].RATE;
                                    TSORDDTL.SCMDISCAMT = VE.TSORDDTL[i].SCMDISCAMT;
                                    TSORDDTL.DISCAMT = VE.TSORDDTL[i].DISCAMT;
                                    TSORDDTL.TAXAMT = 0;
                                    TSORDDTL.ORDAUTONO = VE.TSORDDTL[i].ORDAUTONO;
                                    TSORDDTL.ORDSLNO = VE.TSORDDTL[i].ORDSLNO;
                                    TSORDDTL.PRCCD = "WP";// VE.TSORDDTL[i].PRCCD;
                                    TSORDDTL.PRCEFFDT = VE.TSORDDTL[i].PRCEFFDT;
                                    TSORDDTL.DELVDT = VE.TSORDDTL[i].DELVDT;
                                    TSORDDTL.ITREM = VE.TSORDDTL[i].ITREM;
                                    TSORDDTL.PDESIGN = VE.TSORDDTL[i].PDESIGN;

                                    dbsql = MasterHelpFa.RetModeltoSql(TSORDDTL);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();


                                }
                            }
                            if (VE.UploadDOC != null)// add DOCUMENT
                            {
                                var img = Cn.SaveUploadImageTransaction(VE.UploadDOC, TSORD.AUTONO, TSORD.EMD_NO.Value);
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
                                var NOTE = Cn.SAVETRANSACTIONREMARKS(VE.T_CNTRL_HDR_REM, TSORD.AUTONO, TSORD.CLCD, TSORD.EMD_NO.Value);
                                if (NOTE.Item1.Count != 0)
                                {
                                    for (int tr = 0; tr <= NOTE.Item1.Count - 1; tr++)
                                    {
                                        dbsql = MasterHelpFa.RetModeltoSql(NOTE.Item1[tr]);
                                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                    }
                                }
                            }
                            ModelState.Clear();
                            transaction.Commit();
                            OraTrans.Commit();
                            OraCon.Dispose();
                            string ContentFlg = "";
                            if (VE.DefaultAction == "A")
                            {
                                ContentFlg = "1" + "<br/><br/>Order Number : " + TSORD.DOCCD + TSORD.DOCNO + (dberrmsg == "" ? " " : "Article not save " + dberrmsg) + "~" + TSORD.AUTONO;
                            }
                            else if (VE.DefaultAction == "E")
                            {
                                ContentFlg = "2";
                            }
                            return Content(ContentFlg);
                        }
                        else if (VE.DefaultAction == "V")
                        {
                            //dbsql = MasterHelpFa.TblUpdt("t_sord_scheme", VE.T_SORD.AUTONO, "D");
                            //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_rem", VE.T_SORD.AUTONO, "D");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                            dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_doc", VE.T_SORD.AUTONO, "D");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                            dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_doc_dtl", VE.T_SORD.AUTONO, "D");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            dbsql = MasterHelpFa.TblUpdt("t_sorddtl", VE.T_SORD.AUTONO, "D");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                            dbsql = MasterHelpFa.TblUpdt("t_sord", VE.T_SORD.AUTONO, "D");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }


                            //dbsql = MasterHelpFa.T_Cntrl_Hdr_Updt_Ins(VE.T_SORD.AUTONO, "D", "S", null, null, null, VE.T_SORD.DOCDT.retStr(), null, null, null);
                            dbsql = MasterHelpFa.T_Cntrl_Hdr_Updt_Ins(VE.T_SORD.AUTONO, "D", "S", null, null, null, VE.T_SORD.DOCDT.retStr(), null, null, null,null,null,null,null,0,VE.Audit_REM);
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

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
                        ModelState.Clear();
                        OraTrans.Rollback();
                        OraCon.Dispose();
                        Cn.SaveException(ex, "");
                        return Content(ex.Message + ex.InnerException);
                    }
                }
            }
        }
        public ActionResult Print(string DOCNO, string DOC_CD, string DOCDT)
        {
            ReportViewinHtml ind = new ReportViewinHtml();
            ind.FDT = DOCDT;
            ind.TDT = DOCDT;
            ind.DOCCD = DOC_CD;
            ind.FDOCNO = DOCNO;
            ind.TDOCNO = DOCNO;
            if (TempData["printparameter"] != null)
            {
                TempData.Remove("printparameter");
            }
            TempData["printparameter"] = ind;
            return Content("");
        }
        public ActionResult ParkRecord(FormCollection FC, SalesOrderEntry stream)
        {
            try
            {
                Cn.getQueryString(stream);
                if (stream.T_SORD.DOCCD.retStr() != "")
                {
                    stream.T_CNTRL_HDR.DOCCD = stream.T_SORD.DOCCD.retStr();
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
                string url = Master_Help.RetriveParkFromFile(value, Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString(), "Improvar.ViewModels.SalesOrderEntry");
                return url;
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return ex.Message;
            }
        }
        //public ActionResult ShowLogDetails(SalesOrderEntry TSP, string DOCNO, string DOC_CD)
        //{
        //    ReportViewinHtml ind = new ReportViewinHtml();
        //    ind.DOCCD = DOC_CD;
        //    ind.FDOCNO = DOCNO;
        //    ind.TDOCNO = DOCNO;
        //    if (TempData["printparameter"] != null)
        //    {
        //        TempData.Remove("printparameter");
        //    }
        //    TempData["printparameter"] = ind;
        //    return Content("");
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
                else if (Code == "party")
                {
                    SalesOrderEntry VE = new SalesOrderEntry();
                    Cn.getQueryString(VE);
                    switch (VE.DOC_CODE)
                    {
                        case "SORD": Code = "D"; break;
                        case "PORD": Code = "C"; break;
                        default: Code = "D"; break;
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
        public ActionResult GetSubCodeDetails(string Code, string val)
        {
            var str = Master_Help.SLCD_help(val, Code);
            if (str.IndexOf("='helpmnu'") >= 0)
            {
                return PartialView("_Help2", str);
            }
            else
            {
                if (str.IndexOf(Cn.GCS()) > 0)
                {
                    //var DATA = str.Split(Convert.ToChar(Cn.GCS()));
                    double latedays = Salesfunc.retMaxLateDays(null, val, true);
                    string latedaysdsp = "";
                    if (latedays > 60) latedaysdsp = "Late by " + latedays.ToString();
                    var str1 = str.retCompValue("slcd") + Cn.GCS() + str.retCompValue("slnm") + Cn.GCS() + Master_Help.PRICELIST("", val, "FORPARTY") + Cn.GCS() + str.retCompValue("district") + Cn.GCS() + latedays.ToString() + Cn.GCS() + latedaysdsp;

                    return Content(str1);
                }
                else
                {
                    return Content(str);
                }
            }
        }
        public ActionResult GetItemDetails(string val, string Code)
        {
            try
            {
                var data = Code.Split(Convert.ToChar(Cn.GCS()));
                var str = Master_Help.ITCD_help(val, "", data[0].retStr(), data[1].retSqlformat());
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
        public ActionResult RepeatAboveRow(SalesOrderEntry VE, int RowIndex)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            Cn.getQueryString(VE);
            List<TSORDDTL> TSORDDTL = new List<TSORDDTL>(); bool copied = false;
            for (int k = 0; k <= VE.TSORDDTL.Count; k++)
            {
                TSORDDTL TSORDDTLl = new TSORDDTL();
                if (RowIndex == k || copied == true)
                {
                    foreach (PropertyInfo propA in VE.TSORDDTL[k - 1].GetType().GetProperties())
                    {
                        PropertyInfo propB = VE.TSORDDTL[k - 1].GetType().GetProperty(propA.Name);
                        propB.SetValue(TSORDDTLl, propA.GetValue(VE.TSORDDTL[k - 1], null), null);
                    }
                    TSORDDTLl.SLNO = (k + 1).retShort();
                    TSORDDTL.Add(TSORDDTLl);
                    copied = true;
                }
                else
                {
                    TSORDDTLl = VE.TSORDDTL[k]; TSORDDTLl.SLNO = (k + 1).retShort();
                    TSORDDTL.Add(TSORDDTLl);
                    
                }
                
            }
            VE.TSORDDTL = TSORDDTL;
            VE.DefaultView = true;
            ModelState.Clear();
            return PartialView("_T_SORD_MAIN", VE);
        }
        public ActionResult cancelRecords(SalesOrderEntry VE, string par1)
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
                        TCH = Cn.T_CONTROL_HDR(VE.T_SORD.AUTONO, CommVar.CurSchema(UNQSNO).ToString());
                    }
                    else
                    {
                        TCH = Cn.T_CONTROL_HDR(VE.T_SORD.AUTONO, CommVar.CurSchema(UNQSNO).ToString(), par1);
                    }
                    DB.Entry(TCH).State = System.Data.Entity.EntityState.Modified;
                    DB.SaveChanges();
                    transaction.Commit();
                }
                return Content("1");
            }
            catch (Exception ex)
            {
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetRefRetailDetails(string val, string Code)
        {
            try
            {
                var str = Master_Help.RTDEBCD_help(val);

                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    //var MSG = str.IndexOf(Cn.GCS());
                    //if (MSG >= 0)
                    //{
                    //    DataTable Taxgrpcd = salesfunc.GetSlcdDetails(Code, "");
                    //    str += "^TAXGRPCD=^" + Taxgrpcd.Rows[0]["taxgrpcd"] + Cn.GCS();
                    //}

                    return Content(str);
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
