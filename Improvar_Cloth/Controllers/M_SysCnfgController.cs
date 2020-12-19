using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Data.Entity.Validation;

namespace Improvar.Controllers
{
    public class M_SysCnfgController : Controller
    {
        Connection Cn = new Connection();
        string CS = null;
        M_SYSCNFG sl;
        M_CNTRL_HDR sll;
        M_BRAND sBRND;
        M_PRODGRP sPROD;
        MasterHelp Master_Help = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: M_SysCnfg

        public ActionResult M_SysCnfg(FormCollection FC, string op = "", string key = "", int Nindex = 0, string searchValue = "", string loadItem = "N")
        {
            //testing
            try
            {//test
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Posting & Terms Setup";
                    string loca1 = CommVar.Loccd(UNQSNO).Trim();
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    SysCnfgMasterEntry VE = new SysCnfgMasterEntry();

                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    var doctP = (from i in DB1.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();

                    //=================For WP/rp Price Gen================//
                    List<DropDown_list1> GT = new List<DropDown_list1>();
                    DropDown_list1 GT1 = new DropDown_list1();
                    GT1.text = "ROUND";
                    GT1.value = "RD";
                    GT.Add(GT1);
                    DropDown_list1 GT2 = new DropDown_list1();
                    GT2.text = "ROUNDNEXT";
                    GT2.value = "RN";
                    GT.Add(GT2);
                    DropDown_list1 GT3 = new DropDown_list1();
                    GT3.text = "NEAR";
                    GT3.value = "NR";
                    GT.Add(GT3);
                    DropDown_list1 GT4 = new DropDown_list1();
                    GT4.text = "NEXT";
                    GT4.value = "NT";
                    GT.Add(GT4);
                    VE.DropDown_list1 = GT;
                    //=================End WP/rp Price Gen ================//

                    //=================For Due Date Calc on ================//
                    List<DropDown_list3> list2 = new List<DropDown_list3>();
                    DropDown_list3 obj5 = new DropDown_list3();
                    obj5.text = "Bill Date";
                    obj5.value = "";
                    list2.Add(obj5);
                    DropDown_list3 obj6 = new DropDown_list3();
                    obj6.text = "LR Date";
                    obj6.value = "R";
                    list2.Add(obj6);
                    VE.DropDown_list3 = list2;
                    //=================End  Due Date Calc on ================//
                    VE.DefaultAction = op;
                    string GCS = Cn.GCS();
                    if (op.Length != 0)
                    {
                        VE.IndexKey = (from p in DB.M_SYSCNFG
                                       orderby p.M_AUTONO
                                       select new { M_AUTONO = p.M_AUTONO }).Select(a => new
                                     IndexKey()
                                       { Navikey = a.M_AUTONO.ToString() }).ToList();
                        if (searchValue != "") { Nindex = VE.IndexKey.FindIndex(r => r.Navikey.Equals(searchValue)); }

                        if (op == "E" || op == "D" || op == "V" || loadItem == "Y")
                        {
                            VE.Searchpannel_State = true;
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
                            VE.M_SYSCNFG = sl;
                            VE.M_CNTRL_HDR = sll;
                        }
                        if (op.ToString() == "A" && loadItem == "N")
                        {
                            //List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                            //UploadDOC UPL = new UploadDOC();
                            //UPL.DocumentType = doctP;
                            //UploadDOC1.Add(UPL);
                            //VE.UploadDOC = UploadDOC1;
                        }
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
                SysCnfgMasterEntry VE = new SysCnfgMasterEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public SysCnfgMasterEntry Navigation(SysCnfgMasterEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            try
            {
                sl = new M_SYSCNFG();
                sll = new M_CNTRL_HDR();
                sBRND = new M_BRAND();
                ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                var doctP = (from i in DB1.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();

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
                    //DateTime EDT = Convert.ToDateTime(aa[1]);
                    int autono = aa[0].retInt();
                    sl = DB.M_SYSCNFG.Where(m => m.M_AUTONO == autono).First();
                    sll = DB.M_CNTRL_HDR.Find(sl.M_AUTONO);
                    string class1cd = sl.CLASS1CD.retStr();
                    string SALDEBGLCD = sl.SALDEBGLCD.retStr();
                    string PURDEBGLCD = sl.PURDEBGLCD.retStr();
                    string RETDEBSLCD = sl.RETDEBSLCD.retStr();
                    if (class1cd != "")
                    {
                        var classnm = (from a in DBF.M_CLASS1 where a.CLASS1CD == class1cd select new { a.CLASS1NM }).FirstOrDefault();
                        VE.CLASS1NM = classnm.CLASS1NM;
                    }
                    if (SALDEBGLCD != "")
                    {
                        var salglnm = (from a in DBF.M_GENLEG where a.GLCD == SALDEBGLCD select new { a.GLNM }).FirstOrDefault();
                        VE.SALDEBGLNM = salglnm.GLNM;
                    }
                    if (PURDEBGLCD != "")
                    {
                        var purglnm = (from a in DBF.M_GENLEG where a.GLCD == PURDEBGLCD select new { a.GLNM }).FirstOrDefault();
                        VE.PURDEBGLNM = purglnm.GLNM;
                    }
                    if (RETDEBSLCD != "")
                    {
                        var salretglnm = (from a in DBF.M_SUBLEG where a.SLCD == RETDEBSLCD select new { a.SLNM }).FirstOrDefault();
                        VE.RETDEBSLNM = salretglnm.SLNM;
                    }
                    if (sl.RTDEBCD != null)
                    { var Party = DBF.M_RETDEB.Find(sl.RTDEBCD); if (Party != null) { VE.RTDBNM = Party.RTDEBNM; } }
                    if (sl.INC_RATE == "Y")
                    {
                        VE.INC_RATE = true;
                    }
                    else
                    {
                        VE.INC_RATE = false;
                    }
                    if (sl.MNTNSIZE == "Y")
                    {
                        VE.MNTNSIZE = true;
                    }
                    else
                    {
                        VE.MNTNSIZE = false;
                    }
                    if (sl.MNTNCOLOR == "Y")
                    {
                        VE.MNTNCOLOR = true;
                    }
                    else
                    {
                        VE.MNTNCOLOR = false;
                    }
                    if (sl.MNTNPART == "Y")
                    {
                        VE.MNTNPART = true;
                    }
                    else
                    {
                        VE.MNTNPART = false;
                    }
                    if (sl.MNTNFLAGMTR == "Y")
                    {
                        VE.MNTNFLAGMTR = true;
                    }
                    else
                    {
                        VE.MNTNFLAGMTR = false;
                    }
                    if (sl.WPPRICEGEN.retStr() != "")
                    {
                        VE.WPPRICEGENCD = sl.WPPRICEGEN.Substring(0, 2);
                        VE.WPPRICEGENAMT = sl.WPPRICEGEN.Substring(2, 2);
                    }
                    if (sl.RPPRICEGEN.retStr() != "")
                    {
                        VE.RPPRICEGENCD = sl.RPPRICEGEN.Substring(0, 2);
                        VE.RPPRICEGENAMT = sl.RPPRICEGEN.Substring(2, 2);
                    }
                    if (sll.INACTIVE_TAG == "Y")
                    {
                        VE.Checked = true;
                    }
                    else
                    {
                        VE.Checked = false;
                    }
                    if (VE.DefaultAction == "A")
                    {
                        sl.EFFDT = System.DateTime.Now.Date;
                    }

                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
            }
            return VE;
        }
        public ActionResult SearchPannelData()
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string scm = CommVar.CurSchema(UNQSNO);
            string scmf = CommVar.FinSchema(UNQSNO);
            string sql = "";
            sql += "select a.m_autono,a.compcd,a.effdt,b.glnm saldebglnm,c.glnm purdebglnm from " + scm + ".m_syscnfg a," + scmf + ".m_genleg b,  ";
            sql += scmf + ".m_genleg c where a.saldebglcd=b.glcd(+) and a.purdebglcd=c.glcd(+) ";
            DataTable tbl = Master_Help.SQLquery(sql);
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Autono" + Cn.GCS() + "Company Code" + Cn.GCS() + "Effective Date" + Cn.GCS() + "Debtors" + Cn.GCS() + "Creditors";
            for (int i = 0; i <= tbl.Rows.Count - 1; i++)
            {
                SB.Append("<tr><td>" + tbl.Rows[i]["m_autono"] + "</td><td>" + tbl.Rows[i]["compcd"] + "</td><td>" + tbl.Rows[i]["effdt"].retDateStr() + "</td><td>" + tbl.Rows[i]["saldebglnm"] + "</td><td>" + tbl.Rows[i]["purdebglnm"] + "</td></tr>");
            }
            return PartialView("_SearchPannel2", Master_Help.Generate_SearchPannel(hdr, SB.ToString(), "0", "0"));
        }
        public ActionResult GetSubLedgerDetails(string val, string Code)
        {
            try
            {
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
        public ActionResult GetClass1Details(string val)
        {
            var str = Master_Help.CLASS1(val);
            if (str.IndexOf("='helpmnu'") >= 0)
            {
                return PartialView("_Help2", str);
            }
            else
            {
                return Content(str);
            }
        }
        public ActionResult GetGenLedgerDetails(string val, string Code)
        {
            var str = Master_Help.GLCD_help(val, Code);
            if (str.IndexOf("='helpmnu'") >= 0)
            {
                return PartialView("_Help2", str);
            }
            else
            {
                return Content(str);
            }
        }
        public ActionResult GetSysConfgDetails(string val)
        {
            var str = Master_Help.Get_SysConfgDetails(val);
            if (str.IndexOf("='helpmnu'") >= 0)
            {
                return PartialView("_Help2", str);
            }
            else
            {
                return Content(str);
            }
        }
        public ActionResult GetRefRetailDetails(string val,string code)
        {
            try
            {
                var str = Master_Help.RTDEBCD_help(val, code);
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
        public ActionResult SAVE(FormCollection FC, SysCnfgMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".M_CNTRL_HDR in  row share mode");
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        string compcd = CommVar.Compcd(UNQSNO);
                        M_SYSCNFG MSYSCNFG = new M_SYSCNFG();
                        MSYSCNFG.CLCD = CommVar.ClientCode(UNQSNO);
                        MSYSCNFG.COMPCD = CommVar.Compcd(UNQSNO);
                        MSYSCNFG.EFFDT = VE.M_SYSCNFG.EFFDT;
                        if (VE.DefaultAction == "A")
                        {
                           string query = "select EFFDT from  " + CommVar.CurSchema(UNQSNO) + ".M_SYSCNFG  where EFFDT=to_date('" + VE.M_SYSCNFG.EFFDT.retDateStr() + "', 'dd/mm/yyyy') and COMPCD='" + compcd + "'";
                            DataTable EffdtChk = Master_Help.SQLquery(query);
                            if (EffdtChk.Rows.Count > 0) { transaction.Rollback(); return Content(" "+EffdtChk.Rows[0]["EFFDT"].retDateStr()+" Effective Date already exsist.Please select another Effective Date !"); }
                            MSYSCNFG.EMD_NO = 0;
                            MSYSCNFG.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO).ToString());

                        }
                        if (VE.DefaultAction == "E")
                        {
                            MSYSCNFG.DTAG = "E";
                            MSYSCNFG.M_AUTONO = VE.M_SYSCNFG.M_AUTONO;
                            var MAXEMDNO = (from p in DB.M_SYSCNFG where p.M_AUTONO == VE.M_SYSCNFG.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                MSYSCNFG.EMD_NO = 0;
                            }
                            else
                            {
                                MSYSCNFG.EMD_NO = Convert.ToInt16(MAXEMDNO + 1);
                            }
                        }
                        MSYSCNFG.SALDEBGLCD = VE.M_SYSCNFG.SALDEBGLCD;
                        MSYSCNFG.PURDEBGLCD = VE.M_SYSCNFG.PURDEBGLCD;
                        MSYSCNFG.CLASS1CD = VE.M_SYSCNFG.CLASS1CD;
                        MSYSCNFG.RETDEBSLCD = VE.M_SYSCNFG.RETDEBSLCD;
                        if (VE.WPPRICEGENCD.retStr() != "")
                        {
                            MSYSCNFG.WPPRICEGEN = VE.WPPRICEGENCD.retStr() + VE.WPPRICEGENAMT.retStr().PadLeft(2, '0');//NR99
                        }
                        if (VE.RPPRICEGENCD.retStr() != "")
                        {
                            MSYSCNFG.RPPRICEGEN = VE.RPPRICEGENCD.retStr() + VE.RPPRICEGENAMT.retStr().PadLeft(2, '0');//NT99
                        }
                        MSYSCNFG.DEALSIN = VE.M_SYSCNFG.DEALSIN;
                        MSYSCNFG.INSPOLDESC = VE.M_SYSCNFG.INSPOLDESC;
                        MSYSCNFG.BLTERMS = VE.M_SYSCNFG.BLTERMS;
                        MSYSCNFG.DUEDATECALCON = VE.M_SYSCNFG.DUEDATECALCON;
                        MSYSCNFG.BANLSLNO = VE.M_SYSCNFG.BANLSLNO;
                        MSYSCNFG.WPPER = VE.M_SYSCNFG.WPPER;
                        MSYSCNFG.RPPER = VE.M_SYSCNFG.RPPER;
                        MSYSCNFG.PRICEINCODE = VE.M_SYSCNFG.PRICEINCODE;
                        MSYSCNFG.RTDEBCD = VE.M_SYSCNFG.RTDEBCD;
                        MSYSCNFG.DESIGNPATH = VE.M_SYSCNFG.DESIGNPATH;
                        if (VE.INC_RATE == true) { MSYSCNFG.INC_RATE = "Y"; } else { MSYSCNFG.INC_RATE = "N"; }
                        if (VE.MNTNSIZE == true) { MSYSCNFG.MNTNSIZE = "Y"; } else { MSYSCNFG.MNTNSIZE = "N"; }
                        if (VE.MNTNCOLOR == true) { MSYSCNFG.MNTNCOLOR = "Y"; } else { MSYSCNFG.MNTNCOLOR = "N"; }
                        if (VE.MNTNPART == true) { MSYSCNFG.MNTNPART = "Y"; } else { MSYSCNFG.MNTNPART = "N"; }
                        if (VE.MNTNFLAGMTR == true) { MSYSCNFG.MNTNFLAGMTR = "Y"; } else { MSYSCNFG.MNTNFLAGMTR = "N"; }
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_SYSCNFG", MSYSCNFG.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString());
                        if (VE.DefaultAction == "A")
                        {
                            DB.M_SYSCNFG.Add(MSYSCNFG);
                            DB.M_CNTRL_HDR.Add(MCH);
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            DB.Entry(MSYSCNFG).State = System.Data.Entity.EntityState.Modified;
                            DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        }
                        DB.SaveChanges();
                        transaction.Commit();
                        string ContentFlg = "";
                        if (VE.DefaultAction == "A")
                        {
                            ContentFlg = "1";
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            ContentFlg = "2";
                        }
                        return Content(ContentFlg);
                    }
                    else if (VE.DefaultAction == "V")
                    {

                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_SYSCNFG", VE.M_SYSCNFG.M_AUTONO.retInt(), VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString());
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();

                        DB.M_SYSCNFG.Where(x => x.M_AUTONO == VE.M_SYSCNFG.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_SYSCNFG.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();
                        DB.M_SYSCNFG.RemoveRange(DB.M_SYSCNFG.Where(x => x.M_AUTONO == VE.M_SYSCNFG.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_SYSCNFG.M_AUTONO));
                        DB.SaveChanges();
                        transaction.Commit();
                        return Content("3");
                    }
                    else
                    {
                        return Content("");
                    }
                }
                catch (DbEntityValidationException ex)
                {
                    var errorMessages = ex.EntityValidationErrors
                            .SelectMany(x => x.ValidationErrors)
                            .Select(x => x.ErrorMessage);
                    var fullErrorMessage = string.Join("&quot;", errorMessages);
                    var exceptionMessage = string.Concat(ex.Message, "<br/> &quot; The validation errors are: &quot;", fullErrorMessage);
                    throw new DbEntityValidationException(exceptionMessage, ex.EntityValidationErrors);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Cn.SaveException(ex, "");
                    return Content(ex.Message + ex.InnerException?.InnerException.Message);
                }
            }
        }
        [HttpPost]
        public ActionResult M_SysCnfg(FormCollection FC)
        {
            try
            {
                string dbname = CommVar.CurSchema(UNQSNO).ToString();

                string query = "select a.itgrpnm, a.itgrpcd, case ";
                query = query + "when a.itgrptype='F' then 'Finish Product' ";
                query = query + "when a.itgrptype='A' then 'Accessories' ";
                query = query + "when a.itgrptype='Y' then 'Yarn' ";
                query = query + "when a.itgrptype='S' then 'Scheme Material' ";
                query = query + "when a.itgrptype='C' then 'Fabric' ";
                query = query + "end itgrptype from " + dbname + ".M_SYSCNFG a, " + dbname + ".m_cntrl_hdr c ";
                query = query + "where  a.m_autono=c.m_autono(+) and nvl(c.inactive_tag,'N') = 'N' ";
                query = query + "order by itgrptype, a.itgrpnm";

                CS = Cn.GetConnectionString();

                Cn.con = new OracleConnection(CS);
                if ((Cn.ds.Tables["mst_rep"] == null) == false)
                {
                    Cn.ds.Tables["mst_rep"].Clear();
                }
                if (Cn.con.State == ConnectionState.Closed)
                {
                    Cn.con.Open();
                }

                string str = query;
                Cn.com = new OracleCommand(str, Cn.con);
                Cn.da.SelectCommand = Cn.com;
                bool bu = Convert.ToBoolean(Cn.da.Fill(Cn.ds, "mst_rep"));
                Cn.con.Close();
                var record = Master_Help.SQLquery(query);
                if (bu)
                {
                    DataTable IR = new DataTable("mstrep");

                    Models.PrintViewer PV = new Models.PrintViewer();
                    HtmlConverter HC = new HtmlConverter();

                    HC.RepStart(IR, 2);
                    HC.GetPrintHeader(IR, "itgrpnm", "string", "c,20", "Group Name");
                    HC.GetPrintHeader(IR, "itgrpcd", "string", "c,7", "Group Code");
                    //HC.GetPrintHeader(IR, "brandnm", "string", "c,20", "Brande");
                    HC.GetPrintHeader(IR, "itgrptype", "string", "c,10", "Group Type");

                    for (int i = 0; i <= Cn.ds.Tables["mst_rep"].Rows.Count - 1; i++)
                    {
                        DataRow dr = IR.NewRow();
                        dr["itgrpnm"] = Cn.ds.Tables["mst_rep"].Rows[i]["itgrpnm"];
                        dr["itgrpcd"] = Cn.ds.Tables["mst_rep"].Rows[i]["itgrpcd"];
                        //dr["brandnm"] = Cn.ds.Tables["mst_rep"].Rows[i]["brandnm"];
                        dr["itgrptype"] = Cn.ds.Tables["mst_rep"].Rows[i]["itgrptype"];
                        dr["Flag"] = " class='grid_td'";
                        IR.Rows.Add(dr);
                    }
                    string pghdr1 = "";
                    string repname = CommFunc.retRepname("Group Master Details");
                    pghdr1 = "Group Master Details";
                    PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);
                    return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });

                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
            return View();
        }
            }
}