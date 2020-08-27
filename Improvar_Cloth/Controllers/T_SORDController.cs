using Improvar.Models;
using Improvar.ViewModels;
using Microsoft.Ajax.Utilities;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

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
        // GET: T_SORD
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
                                VE.T_SORD = aaa;
                            }
                            else
                            {
                                if (VE.UploadDOC != null) { foreach (var i in VE.UploadDOC) { i.DocumentType = Cn.DOC_TYPE(); } }
                                INI INIF = new INI();
                                INIF.DeleteKey(Session["UR_ID"].ToString(), parkID, Server.MapPath("~/Park.ini"));
                            }
                            VE = (SalesOrderEntry)Cn.CheckPark(VE, VE.MenuID, VE.MenuIndex, LOC, COMP, CommVar.CurSchema(UNQSNO), Server.MapPath("~/Park.ini"), CommVar.UserID());
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

                                    ////var PriceList = DBF.M_PRCLST.Find(sl.PRCCD); if (PriceList != null) { VE.PriceName = PriceList.PRCNM; VE.PRCCD = sl.PRCCD; VE.PRCNM = PriceList.PRCNM; }
                                    ////var Brand = DB.M_BRAND.Find(sl.BRANDCD); if (Brand != null) { VE.BrandName = Brand.BRANDNM; }
                                    ////var PayTerms = DB.M_PAYTRMS.Find(sl.PAYTRMCD); if (PayTerms != null) { VE.PayTermName = PayTerms.PAYTRMNM; }
                                    ////VE.DropDown_list1 = Master_Help.EFFECTIVE_DATE(sl.PRCCD, sl.DOCDT.ToString().Substring(0, 10).Replace('-', '/'));
                                    ////if (sl.PRCEFFDT != null) { VE.EFF_DATE_ID = sl.PRCEFFDT.ToString().Substring(0, 10).Replace('-', '/'); }
                                    ////VE.DropDown_list4 = Master_Help.DISC_EFFECTIVE_DATE(sl.DISCRTCD, sl.DOCDT.ToString().Substring(0, 10).Replace('-', '/'));
                                    ////if (sl.DISCRTEFFDT != null) { VE.DISC_EFF_DATE_ID = sl.DISCRTEFFDT.ToString().Substring(0, 10).Replace('-', '/'); }
                                    ////var Discount = DBF.M_DISCRT.Find(sl.DISCRTCD); if (Discount != null) { VE.DiscountName = Discount.DISCRTNM; VE.DISCD = sl.DISCRTCD; VE.DISNM = Discount.DISCRTNM; }
                                  
                                    if (VE.DefaultAction == "V")
                                    {
                                        //VE.TSORDDTL_SEARCHPANEL = (from X in DB.T_TXNDTL
                                        //                           join Y in DB.T_CNTRL_HDR on X.AUTONO equals Y.AUTONO
                                        //                           where X.ORDAUTONO == sl.AUTONO
                                        //                           select new { Y.DOCCD, Y.DOCNO, Y.DOCDT, Y.AUTONO, Y.USR_ID, Y.USR_ENTDT }).ToList().Select(Z => new TSORDDTL_SEARCHPANEL()
                                        //                           {
                                        //                               DOCCD = Z.DOCCD,
                                        //                               DOCNO = Z.DOCNO,
                                        //                               DOCDT = Z.DOCDT.ToString().Substring(0, 10),
                                        //                               AUTONO = Z.AUTONO,
                                        //                               SB_MADEBY_ID = Z.USR_ID,
                                        //                               SB_MADEBY_NAME = (from P in DB_I.USER_APPL where P.USER_ID == Z.USR_ID select P.USER_NAME).SingleOrDefault(),
                                        //                               SB_MADEDT = Z.USR_ENTDT.ToString()
                                        //                           }).DistinctBy(x => x.AUTONO).ToList();
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
                                    //////SCHEME DETAILS
                                    ////VE.SCHEME_DTL = (from x in DB.T_SORD_SCHEME
                                    ////                 join y in DB.M_SCHEME_HDR on x.SCMCD equals y.SCMCD into z
                                    ////                 from y in z.DefaultIfEmpty()
                                    ////                 where x.AUTONO == sl.AUTONO
                                    ////                 select new { x.SCMCD, y.SCMNM, y.SCMDT, y.SCMTYPE }).ToList().Select(A => new SCHEME_DTL()
                                    ////                 {
                                    ////                     SCMCODE = A.SCMCD,
                                    ////                     SCMNAME = A.SCMNM,
                                    ////                     SCMDATE = A.SCMDT.Value.ToString().Replace('-', '/').Substring(0, 10),
                                    ////                     SCMTYPE = A.SCMTYPE
                                    ////                 }).OrderBy(s => s.SCMCODE).ToList();
                                    ////if (VE.SCHEME_DTL != null)
                                    ////{
                                    ////    VE.SCHEME_DTL.ForEach(x => x.Checked = true);
                                    ////}
                                    var javaScriptSerializerData = new System.Web.Script.Serialization.JavaScriptSerializer();
                                    string JR_Data = javaScriptSerializerData.Serialize(VE.SCHEME_DTL);
                                    VE.HIDDEN_SCHEME_DATA = JR_Data;
                                    //SCHEME DETAILS END

                                    string str1 = "";
                                    //str1 = "select ROW_NUMBER() OVER(ORDER BY j.styleno) AS SLNO, i.AUTONO, i.STKDRCR, i.STKTYPE, i.FREESTK, i.ITCD, j.ITNM, j.STYLENO, j.PCSPERBOX, j.PCSPERSET, j.UOMCD, sum(i.SCMDISCAMT) SCMDISCAMT, sum(i.DISCAMT)DISCAMT, sum(I.QNTY) QNTY ";
                                    //str1 += " from " + CommVar.CurSchema(UNQSNO) + ".T_SORDDTL i, " + CommVar.CurSchema(UNQSNO) + ".M_SITEM j  where i.ITCD = j.ITCD(+)   and i.AUTONO = '" + sl.AUTONO + "' ";
                                    //str1 += " group by i.AUTONO, i.STKDRCR, i.STKTYPE, i.FREESTK, i.ITCD, j.ITNM, j.STYLENO, j.PCSPERBOX, j.PCSPERSET, j.UOMCD ";
                                    str1 += "select ROW_NUMBER() OVER(ORDER BY c.styleno) AS SLNO, ";
                                    str1 += "a.AUTONO, a.STKDRCR, a.STKTYPE, a.FREESTK, a.ITCD, c.ITNM, c.STYLENO, c.PCSPERSET, c.UOMCD, ";
                                    str1 += "a.sizedsp, b.ratedsp, a.scmdiscamt, a.discamt, a.qnty,A.DELVDT,a.ITREM,c.itgrpcd, d.itgrpnm,c.fabitcd, e.itnm fabitnm from ";
                                    str1 += "(select a.AUTONO, a.STKDRCR, a.STKTYPE, a.FREESTK, a.ITCD,A.DELVDT,a.ITREM, ";
                                    str1 += "a.autono || a.stkdrcr || nvl(a.stktype, '') || nvl(a.freestk, '') || a.itcd autoitcd, ";
                                    str1 += "LISTAGG(b.SIZENM, ',') WITHIN GROUP(order by a.autono, a.stkdrcr, a.stktype, a.freestk, a.ITCD, b.PRINT_SEQ) sizedsp, ";
                                    str1 += "sum(a.SCMDISCAMT) SCMDISCAMT, sum(a.DISCAMT) DISCAMT, sum(a.QNTY) QNTY ";
                                    str1 += "from " + CommVar.CurSchema(UNQSNO) + ".T_SORDDTL a, " + CommVar.CurSchema(UNQSNO) + ".M_SIZE b ";
                                    str1 += "where a.sizecd = b.sizecd(+) and a.AUTONO = '" + sl.AUTONO + "' ";
                                    str1 += "group by a.AUTONO, a.STKDRCR, a.STKTYPE, a.FREESTK, a.ITCD,A.DELVDT,a.ITREM, ";
                                    str1 += "a.autono || a.stkdrcr || nvl(a.stktype, '') || nvl(a.freestk, '') || a.itcd) a, ";
                                    str1 += "(select autono, autono|| stkdrcr || nvl(stktype, '') || nvl(freestk, '') || itcd autoitcd, ";
                                    str1 += "listagg(decode(minsizenm, maxsizenm, minsizenm, minsizenm || '-' || maxsizenm) || ' @' || to_char(rate), ', ') within group(order by autono, itcd, stktype, freestk) ratedsp from ";
                                    str1 += "(select a.AUTONO, a.STKDRCR, a.STKTYPE, a.FREESTK, a.ITCD, a.rate, min(b.sizenm) minsizenm, max(b.sizenm) maxsizenm ";
                                    str1 += "from " + CommVar.CurSchema(UNQSNO) + ".T_SORDDTL a, " + CommVar.CurSchema(UNQSNO) + ".M_SIZE b ";
                                    str1 += "where a.sizecd = b.sizecd(+) and a.AUTONO = '" + sl.AUTONO + "' ";
                                    str1 += "group by a.AUTONO, a.STKDRCR, a.STKTYPE, a.FREESTK, a.ITCD, a.rate) ";
                                    str1 += "group by autono, autono || stkdrcr || nvl(stktype, '') || nvl(freestk, '') || itcd) b, ";
                                    str1 += CommVar.CurSchema(UNQSNO) + ".m_sitem c, " + CommVar.CurSchema(UNQSNO) + ".m_group d, " + CommVar.CurSchema(UNQSNO) + ".m_sitem e ";
                                    str1 += "where a.autoitcd = b.autoitcd(+) and a.itcd = c.itcd(+) and c.itgrpcd=d.itgrpcd and c.fabitcd=e.itcd(+) ";
                                    str1 += "order by styleno ";

                                    //str1 += "select ROW_NUMBER() OVER(ORDER BY c.styleno) AS SLNO, ";
                                    //str1 += "a.AUTONO, a.STKDRCR, a.STKTYPE, a.FREESTK, a.ITCD, c.ITNM, c.STYLENO, c.PCSPERBOX, c.PCSPERSET, c.UOMCD, ";
                                    //str1 += "a.sizedsp, b.ratedsp, a.scmdiscamt, a.discamt, a.qnty from ";
                                    //str1 += "(select a.AUTONO, a.STKDRCR, a.STKTYPE, a.FREESTK, a.ITCD, ";
                                    //str1 += "a.autono || a.stkdrcr || nvl(a.stktype, '') || nvl(a.freestk, '') || a.itcd autoitcd, ";
                                    //str1 += "LISTAGG(b.SIZENM, ',') WITHIN GROUP(order by a.autono, a.stkdrcr, a.stktype, a.freestk, a.ITCD, b.PRINT_SEQ) sizedsp, ";
                                    //str1 += "sum(a.SCMDISCAMT) SCMDISCAMT, sum(a.DISCAMT) DISCAMT, sum(a.QNTY) QNTY ";
                                    //str1 += "from " + CommVar.CurSchema(UNQSNO) + ".T_SORDDTL a, " + CommVar.CurSchema(UNQSNO) + ".M_SIZE b ";
                                    //str1 += "where a.sizecd = b.sizecd(+) and a.AUTONO = '" + sl.AUTONO + "' ";
                                    //str1 += "group by a.AUTONO, a.STKDRCR, a.STKTYPE, a.FREESTK, a.ITCD, ";
                                    //str1 += "a.autono || a.stkdrcr || nvl(a.stktype, '') || nvl(a.freestk, '') || a.itcd) a, ";
                                    //str1 += "(select autono, autono|| stkdrcr || nvl(stktype, '') || nvl(freestk, '') || itcd autoitcd, ";
                                    //str1 += "listagg(decode(minsizenm, maxsizenm, minsizenm, minsizenm || '-' || maxsizenm) || ' @' || to_char(rate), ', ') within group(order by autono, itcd, stktype, freestk) ratedsp from ";
                                    //str1 += "(select a.AUTONO, a.STKDRCR, a.STKTYPE, a.FREESTK, a.ITCD, a.rate, ";
                                    //str1 += "first_value(b.sizenm) over(partition by a.AUTONO, a.STKDRCR, a.STKTYPE, a.FREESTK, a.ITCD, a.rate order by b.print_seq) ";
                                    //str1 += "RANGE BETWEEN UNBOUNDED PRECEDING AND UNBOUNDED FOLLOWING) as minisizenm ";
                                    //str1 += "first_value(b.sizenm) over(partition by a.AUTONO, a.STKDRCR, a.STKTYPE, a.FREESTK, a.ITCD, a.rate order by b.print_seq desc) ";
                                    //str1 += "RANGE BETWEEN UNBOUNDED PRECEDING AND UNBOUNDED FOLLOWING) as maxsizenm ";
                                    //str1 += "from " + CommVar.CurSchema(UNQSNO) + ".T_SORDDTL a, " + CommVar.CurSchema(UNQSNO) + ".M_SIZE b ";
                                    //str1 += "where a.sizecd = b.sizecd(+) and a.AUTONO = '" + sl.AUTONO + "' ";
                                    //str1 += "group by a.AUTONO, a.STKDRCR, a.STKTYPE, a.FREESTK, a.ITCD, a.rate) ";
                                    //str1 += "group by autono, autono || stkdrcr || nvl(stktype, '') || nvl(freestk, '') || itcd) b, ";
                                    //str1 += CommVar.CurSchema(UNQSNO) + ".m_sitem c ";
                                    //str1 += "where a.autoitcd = b.autoitcd(+) and a.itcd = c.itcd(+) ";
                                    //str1 += "order by styleno "; 
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
                                                       RATE_DISPLAY = dr["ratedsp"].ToString(),
                                                       ALL_SIZE = dr["sizedsp"].ToString(),
                                                       DELVDT = dr["DELVDT"].retStr() == "" ? (DateTime?)null : Convert.ToDateTime(dr["DELVDT"].retDateStr()),
                                                       ITREM = dr["ITREM"].ToString(),
                                                       ITGRPCD = dr["ITGRPCD"].ToString(),
                                                       ITGRPNM = dr["ITGRPNM"].ToString(),
                                                       FABITCD = dr["FABITCD"].ToString(),
                                                       FABITNM = dr["FABITNM"].ToString()
                                                   }).OrderBy(s => s.SLNO).ToList();
                                    foreach (var z in VE.TSORDDTL)
                                    {
                                        str1 = "select i.SIZECD,k.SIZENM,i.COLRCD,i.QNTY,i.RATE,i.SCMDISCAMT,i.DISCAMT,k.PRINT_SEQ "
                                             + "from " + CommVar.CurSchema(UNQSNO) + ".T_SORDDTL i, " + CommVar.CurSchema(UNQSNO) + ".M_SIZE k "
                                             + " where  i.SIZECD = k.SIZECD(+)  and i.AUTONO = '" + z.AUTONO + "' AND I.ITCD='" + z.ITCD + "' "
                                             + " order by k.PRINT_SEQ";
                                        tbl = Master_Help.SQLquery(str1);
                                        VE.TSORDDTL_SIZE = (from DataRow dr in tbl.Rows
                                                            select new TSORDDTL_SIZE()
                                                            {
                                                                PRINT_SEQ = dr["PRINT_SEQ"].retStr(),
                                                                SIZECD = dr["SIZECD"].retStr(),
                                                                SIZENM = dr["SIZENM"].retStr(),
                                                                COLRCD = dr["COLRCD"].retStr(),
                                                                RATE = dr["RATE"].retDbl(),
                                                                QNTY = dr["QNTY"].retDbl(),
                                                                AUTONO = z.AUTONO,
                                                                ITCD = z.ITCD,
                                                                SLNO = z.SLNO,
                                                                ITCOLSIZE = z.ITCD + dr["COLRCD"].retStr() + dr["SIZECD"].retStr(),
                                                                PCS_HIDDEN = z.TOTAL_PCS.retDbl(),
                                                                PCSPERSET_HIDDEN = z.PCSPERSET.retDbl(),
                                                                QNTY_HIDDEN = z.QNTY,
                                                            }).ToList();
                                        z.BOXES = Salesfunc.ConvPcstoBox(z.QNTY.retDbl(), z.TOTAL_PCS.retDbl()).retDbl();
                                        z.SETS = Salesfunc.ConvPcstoSet(z.QNTY.retDbl(), z.TOTAL_PCS.retDbl()).retDbl();

                                        z.DropDown_list2 = Master_Help.STOCK_TYPE();
                                        z.DropDown_list3 = Master_Help.FREE_STOCK();
                                        var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                                        string JR = javaScriptSerializer.Serialize(VE.TSORDDTL_SIZE);
                                        z.ChildData = JR;
                                        TOTAL_QNTY += z.QNTY.retDbl();
                                        TOTAL_BOXES += z.BOXES.retDbl();
                                        TOTAL_SETS += z.SETS.retDbl();
                                    }
                                    //for (int z = 0; z <= VE.TSORDDTL.Count - 1; z++)
                                    //{
                                    //    VE.TSORDDTL[z].NOOFSETS = VE.TSORDDTL[z].QNTY / VE.TSORDDTL[z].PCSPERSET;
                                    //    VE.TSORDDTL[z].DropDown_list2 = Master_Help.STOCK_TYPE();
                                    //    VE.TSORDDTL[z].DropDown_list3 = Master_Help.FREE_STOCK();
                                    //    VE.TSORDDTL[z].SLNO = Convert.ToInt16(z + 1);
                                    //    if (VE.TSORDDTL[z].STKTYPE == "F")
                                    //    {
                                    //        VE.TSORDDTL[z].BOXES = Salesfunc.ConvPcstoBox(VE.TSORDDTL[z].QNTY == null ? 0 : VE.TSORDDTL[z].QNTY.Value, VE.TSORDDTL[z].TOTAL_PCS == null ? 0 : VE.TSORDDTL[z].TOTAL_PCS.Value);
                                    //        VE.TSORDDTL[z].SETS = Salesfunc.ConvPcstoSet(VE.TSORDDTL[z].QNTY == null ? 0 : VE.TSORDDTL[z].QNTY.Value, VE.TSORDDTL[z].PCSPERSET == null ? 0 : VE.TSORDDTL[z].PCSPERSET.Value);
                                    //    }
                                    //    else
                                    //    {
                                    //        VE.TSORDDTL[z].BOXES = 0; VE.TSORDDTL[z].SETS = 0;
                                    //    }
                                    //    TOTAL_SETS = TOTAL_SETS + (VE.TSORDDTL[z].SETS == null ? 0 : VE.TSORDDTL[z].SETS);
                                    //    TOTAL_NOOFSETS = TOTAL_NOOFSETS + (VE.TSORDDTL[z].NOOFSETS == null ? 0 : VE.TSORDDTL[z].NOOFSETS);
                                    //    TOTAL_PIECES = TOTAL_PIECES + (VE.TSORDDTL[z].QNTY == null ? 0 : VE.TSORDDTL[z].QNTY);
                                    //    TOTAL_BOXES = TOTAL_BOXES + Salesfunc.ConvPcstoBox((VE.TSORDDTL[z].QNTY == null ? 0 : VE.TSORDDTL[z].QNTY.Value), (VE.TSORDDTL[z].TOTAL_PCS == null ? 0 : VE.TSORDDTL[z].TOTAL_PCS.Value));
                                    //}
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
                                    //VE.TOTPCS = TOTAL_QNTY;  //new
                                    //VE.TOTBOX = TOTAL_BOXES; //new
                                    ////sl.TOTQNTY = sl.TOTQNTY.retStr() == "" ? TOTAL_QNTY : sl.TOTQNTY;//new
                                    ////sl.TOTBOX = sl.TOTBOX.retStr() == "" ? TOTAL_BOXES : sl.TOTBOX;  //new
                                    ////sl.TOTSET = sl.TOTSET.retStr() == "" ? TOTAL_SETS : sl.TOTSET;   //new

                                    VE.TOTAL_BOXES = TOTAL_BOXES.Value;
                                    VE.TOTAL_QNTY = TOTAL_QNTY.Value;
                                    VE.TOTAL_SETS = TOTAL_SETS.Value;
                                    VE.T_CNTRL_HDR = sll;
                                    if (VE.T_CNTRL_HDR.CANCEL == "Y") VE.CancelRecord = true; else VE.CancelRecord = false;
                                    VE.Edit_Tag = true;
                                    if (sll.YR_CD != CommVar.YearCode(UNQSNO))
                                    {
                                        VE.Edit = ""; VE.Delete = "";
                                    }
                                    //var DATA_EXIST = (from x in DB.T_DODTL where x.ORDAUTONO == sl.AUTONO select x).ToList();
                                    //if (DATA_EXIST.Any())
                                    //{
                                    //    if (DATA_EXIST.Count > 0) VE.Edit_Tag = false;
                                    //}
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
            string sql = "select distinct a.autono, a.doccd, c.docno, to_char(a.docdt,'dd/mm/yyyy') docdt, ";
            sql += "c.docdt ddocdt, a.slcd, d.slnm, d.district, a.prefno, 0 tbox,a.aproxval ";
            sql += " from " + scm + ".t_sord a, " + scm + ".t_cntrl_hdr c, " + scmf + ".m_subleg d ";
            sql += "where a.autono = c.autono and c.compcd = '" + COM + "' and c.loccd = '" + LOC + "' and ";
            if (SRC_FDT.retStr() != "") sql += "c.docdt >= to_date('" + SRC_FDT.retDateStr() + "','dd/mm/yyyy') and ";
            if (SRC_TDT.retStr() != "") sql += "c.docdt <= to_date('" + SRC_TDT.retDateStr() + "','dd/mm/yyyy') and ";
            if (SRC_DOCNO.retStr() != "") sql += "(c.vchrno like '%" + SRC_DOCNO.retStr() + "%' or c.docno like '%" + SRC_DOCNO.retStr() + "%') and ";
            if (SRC_SLCD.retStr() != "") sql += "(a.slcd like '%" + SRC_SLCD.retStr() + "%' or upper(d.slnm) like '%" + SRC_SLCD.retStr().ToUpper() + "%') and ";
            sql += "a.slcd = d.slcd and c.yr_cd='" + CommVar.YearCode(UNQSNO) + "' and c.doccd in(" + doccd + ") ";
            sql += "order by docdt, docno ";
            var tbl = Master_Help.SQLquery(sql);

            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Order Number" + Cn.GCS() + "Order Date" + Cn.GCS() + "Party Order Number" + Cn.GCS() + "Party Name"
                + Cn.GCS() + "Order Approx Value" + Cn.GCS() + "AUTO NO";
            for (int j = 0; j <= tbl.Rows.Count - 1; j++)
            {

                SB.Append("<tr><td>" + tbl.Rows[j]["docno"] + "</td><td>" + tbl.Rows[j]["docdt"] + " </td><td>"
                    + tbl.Rows[j]["prefno"] + " </td><td><b>" + tbl.Rows[j]["slnm"] + "</b> ["
                    + tbl.Rows[j]["district"] + "]  (" + tbl.Rows[j]["slcd"] + ") " + " </td><td>" + tbl.Rows[j]["aproxval"] + " </td><td>"
                    + tbl.Rows[j]["autono"] + " </td></tr>");
            }
            return PartialView("_SearchPannel2", Master_Help.Generate_SearchPannel(hdr, SB.ToString(), "5", "5"));
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
        public ActionResult OPENSIZE(SalesOrderEntry VE, short SerialNo, string ITEM, string ITEM_NM, string ART_NO, string UOM, double? QNTY, string TAG)
        {
            //Thread.Sleep(5000);
            Cn.getQueryString(VE);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            string COM = CommVar.Compcd(UNQSNO); string LOC = CommVar.Loccd(UNQSNO); string scm1 = DB.CacheKey.ToString();
            try
            {
                ART_NO = ART_NO.Replace('μ', '+'); ART_NO = ART_NO.Replace('‡', '&');
                ViewBag.Article = ART_NO; //ViewBag.Pcs = PCS; ViewBag.Set = PCS_PERSET;
                TempData["OpenedSizeSerialNo"] = SerialNo.retShort();
                short POSRL = Convert.ToInt16(SerialNo);
                string prceffdt = VE.EFF_DATE_ID; string prccd = VE.PRCCD;
                //var query = (from c in VE.TSORDDTL where (c.ITCD == ITEM && c.FREESTK == FREE_STK) select c).ToList();
                var query = (from c in VE.TSORDDTL where (c.ITCD == ITEM) select c).ToList();

                if (query != null)
                {
                    string sql = "";
                    sql += "select a.itcd, a.styleno, nvl(a.pcsperset,0) pcsperset, a.sizecd, a.sizenm, a.print_seq, a.colrcd, a.colrnm, a.slno, nvl(b.rate,0) rate from ";
                    sql += "( select a.itcd, a.styleno,  a.pcsperset, b.sizecd, d.sizenm, d.print_seq, c.colrcd, c.slno, e.colrnm ";
                    sql += "from " + scm1 + ".m_sitem a, " + scm1 + ".m_sitem_size b, " + scm1 + ".m_sitem_color c, ";
                    sql += scm1 + ".m_size d, " + scm1 + ".m_color e ";
                    sql += "where a.itcd = '" + ITEM + "' and a.itcd = b.itcd(+) and a.itcd = c.itcd(+) and b.sizecd = d.sizecd(+) and c.colrcd = e.colrcd(+) and ";
                    sql += "nvl(b.inactive_tag, 'N')= 'N' and nvl(c.inactive_tag, 'N')= 'N' ) a, ";
                    sql += "( select a.itcd, a.sizecd, a.rate,a.colrcd ";
                    sql += "from " + scm1 + ".m_itemplistdtl a ";
                    //sql += "where a.prccd='" + prccd + "' and a.effdt=to_date('" + prceffdt + "','dd/mm/yyyy') and a.itcd='" + ITEM + "' ) b ";
                    sql += "where a.effdt=to_date('" + VE.T_SORD.DOCDT.retStr().Remove(10) + "','dd/mm/yyyy') and a.itcd='" + ITEM + "' ) b ";
                    sql += "where a.itcd=b.itcd(+) and a.sizecd=b.sizecd(+) and a.colrcd=b.colrcd(+) ";
                    sql += "order by print_seq, slno ";

                    var SIZE_DATA = Master_Help.SQLquery(sql); int SL_NO = 0;
                    var SIZE_COUNT = SIZE_DATA.Rows.Count;
                    //Double PCSPERBOX = Convert.ToDouble(SIZE_DATA.Rows[0]["pcsperbox"]);
                    VE.TSORDDTL_SIZE = (from DataRow dr in SIZE_DATA.Rows
                                        select new TSORDDTL_SIZE()
                                        {
                                            SLNO = ++SL_NO,
                                            //MIXSIZE = dr["mixsize"].ToString(),
                                            PRINT_SEQ = dr["print_seq"].ToString(),
                                            SIZECD = dr["sizecd"].ToString(),
                                            SIZENM = dr["sizenm"].ToString(),
                                            COLRCD = dr["colrcd"].ToString(),
                                            COLRNM = dr["colrnm"].ToString(),
                                            ParentSerialNo = SerialNo,
                                            ITCD_HIDDEN = ITEM,
                                            ITNM_HIDDEN = ITEM_NM,
                                            ARTNO_HIDDEN = ART_NO,
                                            UOM_HIDDEN = UOM,
                                            //  PCS_HIDDEN = PCS.Value,
                                            // PCSPERSET_HIDDEN = PCS_PERSET,
                                            // FREESTK_HIDDEN = FREE_STK,
                                            //STKTYPE_HIDDEN = STK_TYPE,
                                            ITCOLSIZE = ITEM + dr["colrcd"].ToString() + dr["sizecd"].ToString(),
                                            RATE = Convert.ToDouble(dr["rate"].ToString()),
                                            //QNTY = NO_SETS != 0 && PCS_PERSET != 0 ? (dr["mixsize"].ToString() == "M" ? (NO_SETS * PCS_PERSET / SIZE_COUNT) : (PCSPERBOX != PCS_PERSET ? (NO_SETS * PCS_PERSET / SIZE_COUNT) : NO_SETS * PCS_PERSET)) : (NO_SETS * PCS_PERSET),
                                            //QNTY_HIDDEN = NO_SETS != 0 && PCS_PERSET != 0 ? (dr["mixsize"].ToString() == "M" ? (NO_SETS * PCS_PERSET / SIZE_COUNT) : (PCSPERBOX != PCS_PERSET ? (NO_SETS * PCS_PERSET / SIZE_COUNT) : NO_SETS * PCS_PERSET)) : QNTY
                                        }).OrderBy(a => a.PRINT_SEQ).ToList();
                    //bool isItcdChanged = false;
                    //if (query != null && query.First().ALL_SIZE != null)
                    //{
                    //    var list1 = VE.TSORDDTL_SIZE.Select(x => x.SIZECD).ToList();
                    //    var list2 = string.Join(",", query.Select(s => s.ALL_SIZE).ToList()).Split(',').ToList();
                    //    var secondNotFirst = list2.Except(list1).ToList();
                    //    if (secondNotFirst.Any())
                    //    {
                    //        isItcdChanged = true;
                    //    }
                    //    else
                    //    {
                    //        string notsamerate = VE.TSORDDTL_SIZE.Where(e => e.RATE == RATE && e.SLNO == SerialNo).Select(x => x.SIZECD).FirstOrDefault();
                    //        if (notsamerate == null)
                    //        {
                    //            isItcdChanged = true;
                    //        }
                    //    }
                    //}
                    if (query[0].ChildData == null)
                    {
                        var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                        string JR = javaScriptSerializer.Serialize(VE.TSORDDTL_SIZE);
                        query.ForEach(a => a.ChildData = JR);
                    }
                    else
                    {
                        if (VE.TSORDDTL != null)
                        {
                            string SIZECD = ""; var query1 = (from c in VE.TSORDDTL where (c.ITCD == ITEM) select c).ToList();
                            if (query1 != null)
                            {
                                var helpMT = new List<Improvar.Models.TSORDDTL_SIZE>();
                                var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                                helpMT = javaScriptSerializer.Deserialize<List<Improvar.Models.TSORDDTL_SIZE>>(query1[0].ChildData);
                                if (helpMT != null && helpMT.Count > 0)
                                {
                                    SIZECD = string.Join(Cn.GCS(), from Z in helpMT where Z.RATE != null select Z.SIZECD);
                                }
                                var SIZE_CODE = SIZECD.Split(Convert.ToChar(Cn.GCS())).ToList();

                                if (SIZECD != "") { SIZE_CODE = SIZECD.Split(Convert.ToChar(Cn.GCS())).ToList(); }

                                VE.TSORDDTL_SIZE = VE.TSORDDTL_SIZE.Where(a => !SIZE_CODE.Contains(a.SIZECD)).ToList();
                                if (TAG == "1") VE.TSORDDTL_SIZE.ForEach(a => a.QNTY = 0);
                                helpMT.ForEach(a =>
                                {
                                    a.ParentSerialNo = SerialNo; a.ITCD_HIDDEN = ITEM; a.ITNM_HIDDEN = ITEM_NM; a.ARTNO_HIDDEN = ART_NO; a.UOM_HIDDEN = UOM;
                                    // a.PCS_HIDDEN = PCS.Value;
                                    a.QNTY_HIDDEN = QNTY;
                                    // a.FREESTK_HIDDEN = FREE_STK; a.STKTYPE_HIDDEN = STK_TYPE; a.PCSPERSET_HIDDEN = PCS_PERSET;
                                    if (a.QNTY_HIDDEN == 0) { a.QNTY_HIDDEN = null; a.QNTY = null; }
                                    if (TAG == "0")
                                    {
                                        //var CAL_QNTY = a.PCSPERSET_HIDDEN != 0 && NO_SETS != 0 ? NO_SETS * PCS_PERSET : 0;
                                        // a.QNTY = a.MIXSIZE == "M" ? CAL_QNTY / helpMT.Count : (NO_SETS != a.PCSPERSET_HIDDEN ? CAL_QNTY / helpMT.Count : CAL_QNTY);
                                        //var CAL_QNTY = a.PCSPERSET_HIDDEN != 0 ? NO_SETS * PCS_PERSET : 0;
                                        //a.QNTY = a.MIXSIZE == "M" ? CAL_QNTY / helpMT.Count : CAL_QNTY;
                                        a.QNTY_HIDDEN = a.QNTY;
                                        if (a.QNTY_HIDDEN == 0) { a.QNTY_HIDDEN = null; a.QNTY = null; }
                                    }
                                });
                                VE.TSORDDTL_SIZE.AddRange(helpMT);
                                var javaScriptSerializer_New = new System.Web.Script.Serialization.JavaScriptSerializer();
                                string JR_New = javaScriptSerializer_New.Serialize(VE.TSORDDTL_SIZE);
                                query.ForEach(a => a.ChildData = JR_New);
                            }
                        }
                    }
                    //string SIZE = "";
                    //VE.TSORDDTL = (from x in VE.TSORDDTL where x.ITCD == ITEM && x.STKTYPE == STK_TYPE && x.FREESTK == FREE_STK select x).ToList();
                    //if (VE.TSORDDTL != null && VE.TSORDDTL.Count > 0)
                    //{
                    //    for (int i = 0; i <= VE.TSORDDTL.Count - 1; i++)
                    //    {
                    //        var helpMT = new List<Improvar.Models.TSORDDTL_SIZE>();
                    //        var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    //        helpMT = javaScriptSerializer.Deserialize<List<Improvar.Models.TSORDDTL_SIZE>>(VE.TSORDDTL[i].ChildData);

                    //        if (helpMT != null && helpMT.Count > 0)
                    //        {
                    //            SIZE = string.Join(Cn.GCS(), from Z in helpMT where Z.RATE != null select Z.SIZECD);
                    //        }
                    //        helpMT.ForEach(a =>
                    //        {
                    //            a.ParentSerialNo = SerialNo; a.ITCD_HIDDEN = ITEM; a.ITNM_HIDDEN = ITEM_NM;
                    //            a.ARTNO_HIDDEN = ART_NO; a.UOM_HIDDEN = UOM; a.PCS_HIDDEN = PCS.Value;
                    //            a.QNTY_HIDDEN = QNTY; a.FREESTK_HIDDEN = FREE_STK; a.STKTYPE_HIDDEN = STK_TYPE; a.PCSPERSET_HIDDEN = PCS_PERSET;
                    //            if (a.QNTY_HIDDEN == 0) { a.QNTY_HIDDEN = null; a.QNTY = null; }
                    //            if (TAG == "0")
                    //            {
                    //                var CAL_QNTY = a.PCSPERSET_HIDDEN != 0 ? NO_SETS * PCS_PERSET : 0;
                    //                a.QNTY = a.MIXSIZE == "M" ? CAL_QNTY / helpMT.Count : CAL_QNTY;
                    //                //a.QNTY = a.MIXSIZE == "M" ? CAL_QNTY / helpMT.Count : CAL_QNTY;
                    //                a.QNTY_HIDDEN = a.QNTY; if (a.QNTY_HIDDEN == 0) { a.QNTY_HIDDEN = null; a.QNTY = null; }
                    //            }
                    //        });
                    //        var SIZE_CODE = SIZE.Split(Convert.ToChar(Cn.GCS())).ToList();
                    //        if (SIZE != "")
                    //        {
                    //            SIZE_CODE = SIZE.Split(Convert.ToChar(Cn.GCS())).ToList();
                    //        }
                    //        VE.TSORDDTL_SIZE = VE.TSORDDTL_SIZE.Where(a => !SIZE_CODE.Contains(a.SIZECD)).ToList();
                    //        VE.TSORDDTL_SIZE.AddRange(helpMT);
                    //    }
                    //}
                    VE.TSORDDTL_SIZE = VE.TSORDDTL_SIZE.OrderBy(a => a.PRINT_SEQ).ToList();
                    int SL_NO1 = 0;
                    if (VE.TSORDDTL_SIZE != null && VE.TSORDDTL_SIZE.Count > 0)
                    {
                        VE.TSORDDTL_SIZE.ForEach(A =>
                        {
                            A.SLNO = ++SL_NO1;
                            A.ITCOLSIZE = A.ITCD_HIDDEN + A.COLRCD + A.SIZECD;
                            if (A.RATE == 0)
                            {
                                string SQLquery = "select distinct a.itcd, a.sizecd, a.rate from " + DB.CacheKey + ".m_itemplistdtl a where a.effdt= ";
                                SQLquery += "(select max(b.effdt) effdt from " + DB.CacheKey + ".m_itemplistdtl b where b.itcd='" + A.ITCD + "' and b.sizecd='" + A.SIZECD + "' and ";
                                SQLquery += "effdt <= to_date('" + VE.T_SORD.DOCDT.ToString().Remove(10) + "', 'dd/mm/yyyy')) and ";
                                SQLquery += "a.itcd='" + A.ITCD + "' and a.sizecd='" + A.SIZECD + "' ";
                                var RATE_DATA = Master_Help.SQLquery(SQLquery);
                                double MASTER_RATE = 0; if (RATE_DATA != null && RATE_DATA.Rows.Count > 0) { MASTER_RATE = Convert.ToDouble(RATE_DATA.Rows[0]["rate"].ToString()); }
                                if (MASTER_RATE != 0) { A.RATE = MASTER_RATE; }
                            }
                        });
                    }
                    var javaScriptSerializer_FINAL = new System.Web.Script.Serialization.JavaScriptSerializer();
                    string JR_FINAL = javaScriptSerializer_FINAL.Serialize(VE.TSORDDTL_SIZE);
                    query.ForEach(a => a.ChildData = JR_FINAL);
                    //query.ChildData = JR_FINAL;
                    VE.SERIAL = SerialNo;
                    VE.DefaultView = true;
                    ModelState.Clear();
                    return PartialView("_T_SORD_SIZE", VE);
                }
                else
                {
                    VE.SERIAL = SerialNo;
                    VE.DefaultView = true;
                    ModelState.Clear();
                    return PartialView("_T_SORD_SIZE", VE);
                }
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
        public JsonResult CLOSESIZE(SalesOrderEntry VE, FormCollection FC)
        {
            SalesOrderEntry_MAINGRID salesOrderEntry_SIZEGRID = new SalesOrderEntry_MAINGRID();
            try
            {
                List<TSORDDTL_SIZE> DTL_SIZE = VE.TSORDDTL_SIZE.Where(a => a.QNTY != null).Where(a => a.QNTY > 0).ToList();
                double PCSPERBOX = Convert.ToDouble(DTL_SIZE.FirstOrDefault()?.PCS_HIDDEN.retDbl());
                if (DTL_SIZE != null && DTL_SIZE.Count > 0)
                {
                    short SerialNo = TempData["OpenedSizeSerialNo"].retShort(); TempData.Keep(); salesOrderEntry_SIZEGRID.SerialNo = SerialNo.retStr();
                    string strSizeRate = ""; double amt = 0;
                    var GroupedByRate = DTL_SIZE.GroupBy(r => r.RATE);
                    foreach (var group in GroupedByRate)
                    {
                        string min = group.OrderBy(x => x.PRINT_SEQ).First().SIZENM; // (w => w.SIZENM);
                        string max = group.OrderBy(x => x.PRINT_SEQ).Last().SIZENM;  //group.OrderBy(x => x.PRINT_SEQ).Max(w => w.SIZENM);
                        strSizeRate += min + "-" + max + "@" + group.Key + ", ";
                    }
                    salesOrderEntry_SIZEGRID.ALL_SIZE = string.Join(",", DTL_SIZE.Select(r => r.SIZECD));
                    salesOrderEntry_SIZEGRID.RATE_DISPLAY = strSizeRate.Trim().TrimEnd(',');
                    double totqnty = DTL_SIZE.Sum(a => a.QNTY).retDbl();
                    salesOrderEntry_SIZEGRID.QNTY = totqnty.retStr();
                    amt = DTL_SIZE.Sum(e => e.QNTY * e.RATE).retDbl();//new
                    salesOrderEntry_SIZEGRID.AMOUNT = amt.retStr();
                    salesOrderEntry_SIZEGRID.BOXES = Salesfunc.ConvPcstoBox(totqnty, PCSPERBOX).retStr();
                    salesOrderEntry_SIZEGRID.SETS = Salesfunc.ConvPcstoSet(totqnty, PCSPERBOX).retStr();
                    var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    string JR = javaScriptSerializer.Serialize(DTL_SIZE);
                    salesOrderEntry_SIZEGRID.ChildData = JR;
                }
                salesOrderEntry_SIZEGRID.MESSAGE = "SUCCESS";
                ModelState.Clear();
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                salesOrderEntry_SIZEGRID.MESSAGE = ex.Message;
            }

            return Json(salesOrderEntry_SIZEGRID, JsonRequestBehavior.AllowGet);
        }
        //public JsonResult CLOSESISZE(SalesOrderEntry VE, FormCollection FC)
        //{
        //    List<TSORDDTL_SIZE> DTL_SIZE = new List<TSORDDTL_SIZE>();
        //    DTL_SIZE = VE.TSORDDTL_SIZE.Where(a => a.QNTY != null).Where(a => a.QNTY > 0).ToList();
        //    if (DTL_SIZE != null)
        //    {
        //        short SerialNo = TempData["OpenedSizeSerialNo"].retShort();
        //        string strSizeRate = "";
        //        foreach (var v in DTL_SIZE)
        //        {

        //        }

        //        //var FILTER_DATA = (from p in DTL_SIZE
        //        //                   group p by new
        //        //                   { p.ITCD_HIDDEN, p.ITNM_HIDDEN, p.ARTNO_HIDDEN, p.PCS_HIDDEN, p.PCSPERSET_HIDDEN, p.NOOFSETS_HIDDEN, p.UOM_HIDDEN, p.STKTYPE_HIDDEN, p.FREESTK_HIDDEN} into g
        //        //                   select new
        //        //                   {
        //        //                       ITCD_HIDDEN = g.Key.ITCD_HIDDEN,
        //        //                       ITNM_HIDDEN = g.Key.ITNM_HIDDEN,
        //        //                       ARTNO_HIDDEN = g.Key.ARTNO_HIDDEN,
        //        //                       PCS_HIDDEN = g.Key.PCS_HIDDEN,
        //        //                       NOOFSETS_HIDDEN = g.Key.NOOFSETS_HIDDEN,
        //        //                       PCSPERSET_HIDDEN = g.Key.PCSPERSET_HIDDEN,
        //        //                       UOM_HIDDEN = g.Key.UOM_HIDDEN,
        //        //                       STKTYPE_HIDDEN = g.Key.STKTYPE_HIDDEN,
        //        //                       FREESTK_HIDDEN = g.Key.FREESTK_HIDDEN,
        //        //                       RATE = g.Key.RATE,
        //        //                       QNTY = g.Sum(x => x.QNTY),
        //        //                       ALL_SIZE = string.Join(",", g.Select(i => i.SIZECD))
        //        //                   }).ToList();


        //        //var TSORDDTL_FINALDATA = (from a in DTL_SIZE
        //        //                          select new TSORDDTL
        //        //                          {
        //        //                              ITCD = a.ITCD_HIDDEN,
        //        //                              ITNM = a.ITNM_HIDDEN,
        //        //                              ARTNO = a.ARTNO_HIDDEN,
        //        //                              TOTAL_PCS = a.PCS_HIDDEN,
        //        //                              NOOFSETS = a.NOOFSETS_HIDDEN,
        //        //                              PCSPERSET = a.PCSPERSET_HIDDEN,
        //        //                              UOM = a.UOM_HIDDEN,
        //        //                              STKTYPE = a.STKTYPE_HIDDEN,
        //        //                              FREESTK = a.FREESTK_HIDDEN,
        //        //                              RATE = a.RATE,
        //        //                              QNTY = a.QNTY,
        //        //                              ALL_SIZE = string.Join(",", a.ALL_SIZE
        //        //                          }).ToList();

        //        if (TSORDDTL_FINALDATA != null && TSORDDTL_FINALDATA.Count > 0)
        //        {
        //            VE.TSORDDTL.RemoveAll(x => x.ITCD == TSORDDTL_FINALDATA[0].ITCD && x.STKTYPE == TSORDDTL_FINALDATA[0].STKTYPE && x.FREESTK == TSORDDTL_FINALDATA[0].FREESTK);
        //            var INDEX = VE.TSORDDTL.FindIndex(Z => Z.ITCD == null);
        //            if (INDEX == -1)
        //            {
        //                VE.TSORDDTL.AddRange(TSORDDTL_FINALDATA);
        //            }
        //            else
        //            {
        //                VE.TSORDDTL.InsertRange(INDEX, TSORDDTL_FINALDATA);
        //            }
        //        }
        //        string ITEM = "";
        //        if (DTL_SIZE != null && DTL_SIZE.Count > 0)
        //        {
        //            ITEM = DTL_SIZE[0].ITCD_HIDDEN;
        //        }
        //        var query = (from c in VE.TSORDDTL where (c.ITCD == ITEM) select c).ToList();
        //        if (query != null && query.Count > 0)
        //        {
        //            var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        //            string JR = javaScriptSerializer.Serialize(DTL_SIZE);
        //            query.ForEach(a => a.ChildData = JR);
        //        }
        //    }
        //    else
        //    {
        //        string ITEM = FC["hiddenITEM"].ToString();
        //        TSORDDTL query = (from c in VE.TSORDDTL where (c.ITCD == ITEM) select c).SingleOrDefault();
        //        if (query != null)
        //        {
        //            query.ChildData = null;
        //        }
        //    }
        //    short SL_NO = 0;
        //    var SIZE_NULL_COUNT = VE.TSORDDTL_SIZE.Where(a => a.QNTY != null).Where(a => a.QNTY == 0 || a.QNTY == null).ToList();
        //    VE.TSORDDTL.ToList().ForEach(a =>
        //    {
        //        a.DropDown_list2 = Master_Help.STOCK_TYPE(); a.DropDown_list3 = Master_Help.FREE_STOCK(); a.SLNO = ++SL_NO;
        //        if (a.STKTYPE == "F")
        //        {
        //            a.BOXES = Salesfunc.ConvPcstoBox(a.QNTY == null ? 0 : a.QNTY.Value, a.TOTAL_PCS == null ? 0 : a.TOTAL_PCS.Value);
        //            a.SETS = Salesfunc.ConvPcstoSet(a.QNTY == null ? 0 : a.QNTY.Value, a.PCSPERSET == null ? 0 : a.PCSPERSET.Value);
        //        }
        //        else
        //        {
        //            a.BOXES = 0; a.SETS = 0;
        //        }
        //        if (SIZE_NULL_COUNT != null && SIZE_NULL_COUNT.Count > 0)
        //        {
        //            a.NOOFSETS = 0;
        //        }
        //    });
        //    VE.DefaultView = true;
        //    ModelState.Clear();
        //    return PartialView("_T_SORD_MAIN", VE);
        //}
        //public ActionResult GetArticleItem(string val)
        //{
        //    string ITGTYPE = "F";
        //    if (val == null)
        //    {
        //        return PartialView("_Help2", Master_Help.ARTICLE_ITEM_DETAILS(val, ITGTYPE));
        //    }
        //    else
        //    {
        //        string str = Master_Help.ARTICLE_ITEM_DETAILS(val, ITGTYPE);
        //        return Content(str);
        //    }
        //}
        //public ActionResult GetArticleItem(SalesOrderEntry VE, string val, string RowIndex)
        //{
        //    try
        //    {
        //        string ITGTYPE = "F";
        //        if (val == null)
        //        {
        //            return PartialView("_Help2", Master_Help.ARTICLE_ITEM_DETAILS(val, ITGTYPE));
        //        }
        //        else
        //        {
        //            string scm1 = CommVar.CurSchema(UNQSNO);
        //            int slno = RowIndex.retInt() + 1;
        //            var rate = VE.TSORDDTL.Where(r => r.SLNO == slno).Select(r => r.RATE).FirstOrDefault();
        //            string prceffdt = VE.EFF_DATE_ID; string prccd = VE.T_SORD.PRCCD;
        //            string sql = "";
        //            sql += "select a.itcd, a.styleno, nvl(a.pcsperbox,0) pcsperbox, nvl(a.pcsperset,0) pcsperset, a.sizecd, a.sizenm, a.mixsize, a.print_seq, a.colrcd, a.colrnm, a.slno, nvl(b.rate,0) rate from ";
        //            sql += "(select a.itcd, a.styleno, a.pcsperbox, a.pcsperset, b.sizecd, d.sizenm, a.mixsize, d.print_seq, c.colrcd, c.slno, e.colrnm ";
        //            sql += "from " + scm1 + ".m_sitem a, " + scm1 + ".m_sitem_size b, " + scm1 + ".m_sitem_color c, ";
        //            sql += scm1 + ".m_size d, " + scm1 + ".m_color e ";
        //            sql += "where a.styleno = '" + val + "' and a.itcd = b.itcd(+) and a.itcd = c.itcd(+) and b.sizecd = d.sizecd(+) and c.colrcd = e.colrcd(+) and ";
        //            sql += "nvl(b.inactive_tag, 'N')= 'N' and nvl(c.inactive_tag, 'N')= 'N' ) a, ";
        //            sql += "(select a.itcd, a.sizecd, a.rate ";
        //            sql += "from " + scm1 + ".m_itemplistdtl a," + scm1 + ".m_sitem b ";
        //            sql += "where a.itcd=b.itcd and a.prccd='" + prccd + "' and a.effdt=to_date('" + prceffdt + "','dd/mm/yyyy') and";
        //            if (rate != null && rate != 0) sql += " a.rate= " + rate + " and ";
        //            sql += " b.styleno='" + val + "' ) b ";
        //            if (rate != null && rate != 0)
        //            {
        //                sql += "where a.itcd=b.itcd and a.sizecd=b.sizecd(+) ";
        //            }
        //            else
        //            {
        //                sql += "where a.itcd=b.itcd(+) and a.sizecd=b.sizecd(+) ";
        //            }
        //            sql += "order by print_seq, slno ";
        //            var SIZE_DATA = Master_Help.SQLquery(sql);
        //            if (SIZE_DATA == null || SIZE_DATA.Rows.Count == 0) { return Content("Delete the row and reselect article/item."); }
        //            string str = Master_Help.ARTICLE_ITEM_DETAILS(val, ITGTYPE);
        //            return Content(str);
        //        }
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //    return Content("Error Item Selection.");
        //}
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

                    //string scm1 = CommVar.CurSchema(UNQSNO); 
                    //string ChildData = VE.TSORDDTL.Where(r => r.SLNO == slno).Select(r => r.ChildData).FirstOrDefault();
                    //var helpMT = new List<Improvar.Models.TSORDDTL_SIZE>();
                    //if (!string.IsNullOrEmpty(ChildData))
                    //{
                    //    var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    //    helpMT = javaScriptSerializer.Deserialize<List<Improvar.Models.TSORDDTL_SIZE>>(ChildData);
                    //}
                    //string rate = string.Join(",", helpMT.Select(s => s.RATE).Distinct().ToList());
                    //string size = string.Join(",", helpMT.Select(s => s.SIZECD).Distinct().ToList());

                    //string prceffdt = VE.EFF_DATE_ID; string prccd = VE.T_SORD.PRCCD;
                    //string sql = "";
                    //sql += "select a.itcd, a.styleno, nvl(a.pcsperbox,0) pcsperbox, nvl(a.pcsperset,0) pcsperset, a.sizecd, a.sizenm, a.mixsize, a.print_seq, a.colrcd, a.colrnm, a.slno, nvl(b.rate,0) rate from ";
                    //sql += "(select a.itcd, a.styleno, a.pcsperbox, a.pcsperset, b.sizecd, d.sizenm, a.mixsize, d.print_seq, c.colrcd, c.slno, e.colrnm ";
                    //sql += "from " + scm1 + ".m_sitem a, " + scm1 + ".m_sitem_size b, " + scm1 + ".m_sitem_color c, ";
                    //sql += scm1 + ".m_size d, " + scm1 + ".m_color e ";
                    //sql += "where a.styleno = '" + article + "' and a.itcd = b.itcd(+) and a.itcd = c.itcd(+) and b.sizecd = d.sizecd(+) and c.colrcd = e.colrcd(+) and ";
                    //sql += "nvl(b.inactive_tag, 'N')= 'N' and nvl(c.inactive_tag, 'N')= 'N' ) a, ";
                    //sql += "(select a.itcd, a.sizecd, a.rate ";
                    //sql += "from " + scm1 + ".m_itemplistdtl a," + scm1 + ".m_sitem b ";
                    //sql += "where a.itcd=b.itcd and a.prccd='" + prccd + "' and a.effdt=to_date('" + prceffdt + "','dd/mm/yyyy') and";
                    //if (!string.IsNullOrEmpty(ChildData)) { sql += " a.rate IN ( " + rate + ") and  a.sizecd in ( " + size.retSqlformat() + ") and"; }
                    //sql += " b.styleno='" + article + "' ) b ";
                    //if (!string.IsNullOrEmpty(ChildData))
                    //{
                    //    sql += "where a.itcd=b.itcd and a.sizecd=b.sizecd ";
                    //}
                    //else
                    //{
                    //    sql += "where a.itcd=b.itcd(+) and a.sizecd=b.sizecd(+) ";
                    //}
                    //sql += "order by print_seq, slno ";
                    //var SIZE_DATA = Master_Help.SQLquery(sql);
                    //if (SIZE_DATA == null || SIZE_DATA.Rows.Count == 0) {
                    //    str += Cn.GCS() + "ITCDCHANGED";
                    //}
                    //else
                    //{
                    //    str +=  Cn.GCS() + "";
                    //}                
                    return Content("INVALID" + Cn.GCS() + "");
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
        public ActionResult ApplyRate(SalesOrderEntry VE, double RATE_PER)
        {
            //var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            //VE.TSORDDTL.Where(a => a.RATE != null).ForEach(a =>
            //{
            //    a.RATE = a.RATE + a.RATE * RATE_PER / 100;
            //    {
            //        VE.TSORDDTL_SIZE = javaScriptSerializer.Deserialize<List<Improvar.Models.TSORDDTL_SIZE>>(a.ChildData);
            //        VE.TSORDDTL_SIZE.Where(b => b.RATE != null).ForEach(b => b.RATE = b.RATE + b.RATE * RATE_PER / 100);
            //    }
            //    a.ChildData = javaScriptSerializer.Serialize(VE.TSORDDTL_SIZE);
            //});
            //VE.TSORDDTL.ForEach(a => { a.DropDown_list2 = Master_Help.STOCK_TYPE(); a.DropDown_list3 = Master_Help.FREE_STOCK(); });
            //VE.DefaultView = true;
            //ModelState.Clear();
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
                                TSORD.EMD_NO = Convert.ToByte((VE.T_CNTRL_HDR.EMD_NO == null ? 0 : VE.T_CNTRL_HDR.EMD_NO) + 1);
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
                            dbsql = MasterHelpFa.T_Cntrl_Hdr_Updt_Ins(TSORD.AUTONO, VE.DefaultAction, "S", Month, TSORD.DOCCD, DOCPATTERN, TSORD.DOCDT.retStr(), TSORD.EMD_NO.retShort(), TSORD.DOCNO, Convert.ToDouble(TSORD.DOCNO), null, null, null, TSORD.SLCD);
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

                                    var SIZE_CODE = VE.TSORDDTL[i].ALL_SIZE.Split(',');
                                    if (VE.TSORDDTL[i].ChildData != null)
                                    {
                                        string data = VE.TSORDDTL[i].ChildData;
                                        var helpM = new List<Improvar.Models.TSORDDTL_SIZE>();
                                        var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                                        helpM = javaScriptSerializer.Deserialize<List<Improvar.Models.TSORDDTL_SIZE>>(data);
                                        helpM = helpM.Where(a => SIZE_CODE.Contains(a.SIZECD)).ToList();
                                        for (int j = 0; j <= helpM.Count - 1; j++)
                                        {
                                            if (helpM[j].SLNO != 0 && helpM[j].RATE != null && helpM[j].QNTY != null && helpM[j].QNTY != 0)
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
                                                TSORDDTL.STKTYPE = "T";// VE.TSORDDTL[i].STKTYPE;
                                                TSORDDTL.FREESTK = VE.TSORDDTL[i].FREESTK;
                                                TSORDDTL.ITCD = VE.TSORDDTL[i].ITCD;
                                                TSORDDTL.SIZECD = helpM[j].SIZECD;
                                                TSORDDTL.COLRCD = helpM[j].COLRCD;
                                                TSORDDTL.QNTY = helpM[j].QNTY;
                                                TSORDDTL.RATE = helpM[j].RATE;
                                                TSORDDTL.SCMDISCAMT = helpM[j].SCMDISCAMT;
                                                TSORDDTL.DISCAMT = helpM[j].DISCAMT;
                                                TSORDDTL.TAXAMT = 0;
                                                TSORDDTL.ORDAUTONO = VE.TSORDDTL[i].ORDAUTONO;
                                                TSORDDTL.ORDSLNO = VE.TSORDDTL[i].ORDSLNO;
                                                TSORDDTL.PRCCD = "EXFI";// VE.TSORDDTL[i].PRCCD;
                                                TSORDDTL.PRCEFFDT = VE.TSORDDTL[i].PRCEFFDT;
                                                TSORDDTL.DELVDT = VE.TSORDDTL[i].DELVDT;
                                                TSORDDTL.ITREM = VE.TSORDDTL[i].ITREM;

                                                dbsql = MasterHelpFa.RetModeltoSql(TSORDDTL);
                                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                            }
                                        }
                                    }

                                }
                            }

                            //if (VE.SCHEME_DTL != null)
                            //{
                            //    for (int j = 0; j <= VE.SCHEME_DTL.Count - 1; j++)
                            //    {
                            //        if (VE.SCHEME_DTL[j].Checked == true)
                            //        {
                            //            T_SORD_SCHEME TSORDSCHEME = new T_SORD_SCHEME();
                            //            TSORDSCHEME.EMD_NO = TSORD.EMD_NO;
                            //            TSORDSCHEME.CLCD = TSORD.CLCD;
                            //            TSORDSCHEME.DTAG = TSORD.DTAG;
                            //            TSORDSCHEME.TTAG = TSORD.TTAG;
                            //            TSORDSCHEME.AUTONO = TSORD.AUTONO;
                            //            TSORDSCHEME.SCMCD = VE.SCHEME_DTL[j].SCMCODE;

                            //            dbsql = MasterHelpFa.RetModeltoSql(TSORDSCHEME);
                            //            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            //        }
                            //    }
                            //}
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

                            dbsql = MasterHelpFa.TblUpdt("t_sorddtl", VE.T_SORD.AUTONO, "D");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                            dbsql = MasterHelpFa.TblUpdt("t_sord", VE.T_SORD.AUTONO, "D");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                            dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_rem", VE.T_SORD.AUTONO, "D");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                            dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_doc", VE.T_SORD.AUTONO, "D");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                            dbsql = MasterHelpFa.TblUpdt("t_cntrl_hdr_doc_dtl", VE.T_SORD.AUTONO, "D");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                            dbsql = MasterHelpFa.T_Cntrl_Hdr_Updt_Ins(VE.T_SORD.AUTONO, "D", "S", null, null, null, VE.T_SORD.DOCDT.retStr(), null, null, null);
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
        ////public ActionResult CalcSchmDisc(SalesOrderEntry VE)
        ////{
        ////    string scm1 = CommVar.CurSchema(UNQSNO);
        ////    string sql = "";
        ////    Int32 i = 0, maxR = 0, rNo = 0;
        ////    DataTable rsTmp = new DataTable();
        ////    string selitcd = string.Join(",", (from orddtl in VE.TSORDDTL select "'" + orddtl.ITCD + "'").Distinct());
        ////    Int32 countitcd = VE.TSORDDTL.Select(a => a.ITCD).Distinct().Count();
        ////    if (countitcd > 1000) selitcd = "";

        ////    string selscmcd = "";
        ////    if (VE.SCHEME_DTL != null)
        ////    {
        ////        selscmcd = string.Join(",", (from ord in VE.SCHEME_DTL.Where(a => a.Checked == true)
        ////                                     select "'" + ord.SCMCODE + "'").Distinct());
        ////    }
        ////    bool calcDisc = false, calcScheme = false;
        ////    if (VE.DISCRTCD != null && VE.DISC_EFF_DATE_ID != null) calcDisc = true;
        ////    if (selscmcd != "") calcScheme = true;

        ////    #region Merge Data of All Item with sizes (ItemAll)
        ////    var DATA = (from Z in VE.TSORDDTL select Z.ChildData).Distinct().ToList();
        ////    string ALL_CHILD_DATA = string.Join(",", (from Z in VE.TSORDDTL select "'" + Z.ChildData + "'").Distinct());
        ////    ALL_CHILD_DATA = ALL_CHILD_DATA.Replace("'", "").Replace("[", "").Replace("]", "").Replace("]", "");
        ////    ALL_CHILD_DATA = "[" + ALL_CHILD_DATA + "]";
        ////    ALL_CHILD_DATA = ALL_CHILD_DATA.Replace(",]", "]");
        ////    var ItemAll = new List<Improvar.Models.TSORDDTL_SIZE>();
        ////    var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        ////    ItemAll = javaScriptSerializer.Deserialize<List<Improvar.Models.TSORDDTL_SIZE>>(ALL_CHILD_DATA);
        ////    #endregion

        ////    #region Calculate Discount
        ////    if (calcDisc == true)
        ////    {
        ////        sql = "";
        ////        sql += "select a.discrtcd, a.effdt, b.disccalctype, a.scmitmgrpcd, a.discper, a.discrate ";
        ////        sql += "from " + scm1 + ".m_discrtdtl a, " + scm1 + ".m_discrthdr b ";
        ////        sql += "where a.discrtcd||a.effdt = b.discrtcd||b.effdt and ";
        ////        sql += "a.discrtcd='" + VE.DISCRTCD + "' and a.effdt=to_date('" + VE.DISC_EFF_DATE_ID + "','dd/mm/yyyy') ";
        ////        rsTmp = Master_Help.SQLquery(sql);
        ////        maxR = rsTmp.Rows.Count - 1;
        ////        while (i <= maxR)
        ////        {
        ////            string ScmItmGrpCd = "'" + rsTmp.Rows[i]["scmitmgrpcd"].ToString() + "'";
        ////            DataTable rsScmItmGrpcd = Scheme_Cal.GenScmItmGrpData(ScmItmGrpCd, selitcd);

        ////            if (rsScmItmGrpcd.Rows.Count > 1)
        ////            {
        ////                for (Int32 z = 0; z <= ItemAll.Count() - 1; z++)
        ////                {
        ////                    double sdiscper = Convert.ToDouble(rsTmp.Rows[i]["discper"]);
        ////                    double sdiscrate = Convert.ToDouble(rsTmp.Rows[i]["discrate"]);
        ////                    double discamt = 0;
        ////                    double amt = Cn.Roundoff(ItemAll[z].QNTY.Value * ItemAll[z].RATE.Value, 2);
        ////                    var rsTmp1 = rsScmItmGrpcd.Select("ITCOLSIZE='" + ItemAll[z].ITCOLSIZE + "'").ToList();

        ////                    if (rsTmp1.Any())
        ////                    {
        ////                        if (rsTmp.Rows[i]["disccalctype"].ToString() == "B")
        ////                        {
        ////                            sdiscrate = Cn.Roundoff(sdiscrate / ItemAll[z].PCS_HIDDEN, 6);
        ////                        }
        ////                        if (sdiscper != 0)
        ////                        {
        ////                            discamt = Cn.Roundoff((amt * sdiscper) / 100, 2);
        ////                        }
        ////                        else
        ////                        {
        ////                            discamt = Cn.Roundoff(sdiscrate * ItemAll[z].QNTY.Value, 2);
        ////                        }
        ////                        ItemAll[z].AMT = amt;
        ////                        ItemAll[z].DISCAMT = discamt;
        ////                    }
        ////                }
        ////            }
        ////            i++;
        ////        }
        ////    }
        ////    #endregion eof Discount calculation 

        ////    #region Calculate Scheme
        ////    if (calcScheme == true)
        ////    {

        ////    }
        ////    #endregion eof Scheme calculation


        ////    return Content("");
        ////}
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
        public ActionResult ParkRecord(FormCollection FC, SalesOrderEntry stream, string menuID, string menuIndex)
        {
            try
            {
                Connection cn = new Connection();
                string ID = menuID + menuIndex + CommVar.Loccd(UNQSNO) + CommVar.Compcd(UNQSNO) + CommVar.CurSchema(UNQSNO) + "*" + DateTime.Now;
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
                return Content(ex.Message + ex.InnerException);
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
                return ex.Message + ex.InnerException;
            }
        }
        //public ActionResult ShowLogDetails(TransactionSaleEntry TSP, string DOCNO, string DOC_CD)
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
                var str = Master_Help.ITCD_help(val, "", data[0].retSqlformat(), data[1].retSqlformat());
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
    }
}
