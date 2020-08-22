using System;
using System.Linq;
using System.Web.Mvc;                                                   
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;


namespace Improvar.Controllers
{
    public class M_GrpMastController : Controller
    {
        Connection Cn = new Connection();
        string CS = null;
        M_GROUP sl;
        M_CNTRL_HDR sll;
        M_BRAND sBRND;
        M_PRODGRP sPROD;
        MasterHelp Master_Help = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: M_GrpMast
        //public string GetInternetIp()
        public ActionResult M_GrpMast(FormCollection FC, string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {
            //testing
            try {//test
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Group Master";
                    string loca1 = CommVar.Loccd(UNQSNO).Trim();
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    GroupMasterEntry VE = new GroupMasterEntry();

                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    var doctP = (from i in DB1.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();
                    VE.Database_Combo1 = (from i in DB.M_GROUP select new Database_Combo1() { FIELD_VALUE = i.ITGRPNM }).OrderBy(s => s.FIELD_VALUE).ToList();

                    //=================For Group Type================//
                    List<GroupType> GT = new List<GroupType>();
                    GroupType GT1 = new GroupType();
                    GT1.text = "Accessories";
                    GT1.value = "A";
                    GT.Add(GT1);
                    GroupType GT2 = new GroupType();
                    GT2.text = "Sales Promotion";
                    GT2.value = "S";
                    GT.Add(GT2);
                    GroupType GT3 = new GroupType();
                    GT3.text = "Fabric";
                    GT3.value = "C";
                    GT.Add(GT3);
                    GroupType GT4 = new GroupType();
                    GT4.text = "Packing Material";
                    GT4.value = "P";
                    GT.Add(GT4);
                    GroupType GT5 = new GroupType();
                    GT5.text = "Yarn";
                    GT5.value = "Y";
                    GT.Add(GT5);
                    GroupType GT6 = new GroupType();
                    GT6.text = "Finish Product";
                    GT6.value = "F";
                    GT.Add(GT6);
                    GroupType GT7 = new GroupType();
                    GT7.text = "Chemical";
                    GT7.value = "L";
                    GT.Add(GT7);
                    GroupType GT8 = new GroupType();
                    GT8.text = "Wastage";
                    GT8.value = "W";
                    GT.Add(GT8);
                    GroupType GT9 = new GroupType();
                    GT9.text = "Folding/Patti";
                    GT9.value = "T";
                    GT.Add(GT9);
                    VE.GroupType = GT;
                    //=================End Group Type================//
                    //=================For HSNCODE Database Combo================//
                    VE.Database_Combo2 = (from i in DB.M_GROUP select new Database_Combo2() { FIELD_VALUE = i.HSNCODE }).OrderBy(s => s.FIELD_VALUE).ToList();
                    //=================END HSNCODE Database Combo================//

                    //=================For Bar Code Generation Type================//
                    List<DropDown_list1> list1 = new List<DropDown_list1>();
                    DropDown_list1 obj1 = new DropDown_list1();
                    obj1.text = "Common";
                    obj1.value = "C";
                    list1.Add(obj1);
                    DropDown_list1 obj2 = new DropDown_list1();
                    obj2.text = "Individual";
                    obj2.value = "I";
                    list1.Add(obj2);
                    DropDown_list1 obj3 = new DropDown_list1();
                    obj3.text = "Entry";
                    obj3.value = "E";
                    list1.Add(obj3);
                    VE.DropDown_list1 = list1;
                    //=================End Bar Code Generation Type================//
                    VE.DefaultAction = op;

                    if (op.Length != 0)
                    {
                        VE.IndexKey = (from p in DB.M_GROUP orderby p.ITGRPCD select new IndexKey() { Navikey = p.ITGRPCD }).ToList();

                        if (op == "E" || op == "D" || op == "V")
                        {
                            VE.Searchpannel_State = true;
                            if (searchValue.Length != 0)
                            {
                                VE.Index = Nindex;
                                VE = Navigation(VE, DB, 0, searchValue);
                            }
                            else
                            {
                                if (key == "" || key == "F")
                                {
                                    VE.Index = 0;
                                    VE = Navigation(VE, DB, 0, searchValue);
                                }
                                else if (key == "L")
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
                            VE.M_GROUP = sl;
                            VE.M_BRAND = sBRND;
                            VE.M_PRODGRP = sPROD;
                            VE.M_CNTRL_HDR = sll;
                        }
                        if (op.ToString() == "A")
                        {
                            List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                            UploadDOC UPL = new UploadDOC();
                            UPL.DocumentType = doctP;
                            UploadDOC1.Add(UPL);
                            VE.UploadDOC = UploadDOC1;
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
                GroupMasterEntry VE = new GroupMasterEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public GroupMasterEntry Navigation(GroupMasterEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            try {
                sl = new M_GROUP();
                sll = new M_CNTRL_HDR();
                sBRND = new M_BRAND();
                ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
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
                    sl = DB.M_GROUP.Find(aa[0]);
                    sPROD = DB.M_PRODGRP.Find(sl.PRODGRPCD);
                    sll = DB.M_CNTRL_HDR.Find(sl.M_AUTONO);

                    if (sll.INACTIVE_TAG == "Y")
                    {
                        VE.Checked = true;
                    }
                    else
                    {
                        VE.Checked = false;
                    }
                    VE.UploadDOC = Cn.GetUploadImage(CommVar.CurSchema(UNQSNO).ToString(), Convert.ToInt32(sl.M_AUTONO));

                    if (VE.UploadDOC.Count == 0)
                    {
                        List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                        UploadDOC UPL = new UploadDOC();
                        UPL.DocumentType = doctP;
                        UploadDOC1.Add(UPL);
                        VE.UploadDOC = UploadDOC1;
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
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            var MDT = (from j in DB.M_GROUP
                       join o in DB.M_CNTRL_HDR on j.M_AUTONO equals (o.M_AUTONO)
                       where (j.M_AUTONO == o.M_AUTONO)
                       select new 
                       {
                           ITGRPCD = j.ITGRPCD,
                           ITGRPNM = j.ITGRPNM,
                           M_AUTONO = o.M_AUTONO
                       }).OrderBy(s => s.ITGRPCD).ToList();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Group Code" + Cn.GCS() + "Group Name" + Cn.GCS() + "AUTO NO";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr><td>" + MDT[j].ITGRPCD + "</td><td>" + MDT[j].ITGRPNM + "</td><td>" + MDT[j].M_AUTONO + "</td></tr>");
            }
            return PartialView("_SearchPannel2", Master_Help.Generate_SearchPannel(hdr, SB.ToString(), "0", "2"));
        }
     
        public ActionResult BrandMaster(string val)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            var query = (from c in DB.M_BRAND where (c.BRANDCD == val) select c);
            if (query.Any())
            {
                string str = "";
                foreach (var i in query)
                {
                    str = i.BRANDCD + Cn.GCS() + i.BRANDNM;
                }
                return Content(str);
            }
            else
            {
                return Content("0");
            }
        }
        public ActionResult GetProductGrp()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            return PartialView("_Help2", Master_Help.PRODGRPCD_help(null));
        }
        public ActionResult ProductGrp(string val)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            var query = (from c in DB.M_PRODGRP where (c.PRODGRPCD == val) select c);
            if (query.Any())
            {
                string str = "";
                foreach (var i in query)
                {
                    str = i.PRODGRPCD + Cn.GCS() + i.PRODGRPNM;
                }
                return Content(str);
            }
            else
            {
                return Content("0");
            }
        }
        public ActionResult CheckGroupCode(string val)
        {
            string VALUE = val.ToUpper();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            var query = (from c in DB.M_GROUP where (c.ITGRPCD == VALUE) select c);
            if (query.Any())
            {
                return Content("1");
            }
            else
            {
                return Content("0");
            }
        }
        public ActionResult AddDOCRow(GroupMasterEntry VE)
        {
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), "IMPROVAR");
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
        public ActionResult DeleteDOCRow(GroupMasterEntry VE)
        {
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), "IMPROVAR");
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
        public ActionResult SAVE(FormCollection FC, GroupMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".M_CNTRL_HDR in  row share mode");
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        M_GROUP MGROUP = new M_GROUP();
                        MGROUP.CLCD = CommVar.ClientCode(UNQSNO);
                        if (VE.DefaultAction == "A")
                        {
                            MGROUP.EMD_NO = 0;
                            MGROUP.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO).ToString());
                            string txtst = VE.M_GROUP.ITGRPNM.Substring(0, 1).ToUpper();
                            string sql = " select max(SUBSTR(ITGRPCD, 2)) ITGRPCD FROM " + CommVar.CurSchema(UNQSNO)+".M_GROUP";
                            var tbl = Master_Help.SQLquery(sql);
                            if (tbl.Rows[0]["ITGRPCD"].ToString()!="")
                            {
                                MGROUP.ITGRPCD = txtst + (tbl.Rows[0]["ITGRPCD"]).retInt().ToString("D3");
                            }
                            else
                            {
                                MGROUP.ITGRPCD = txtst + (1).ToString("D3");
                            }
                        }
                        if (VE.DefaultAction == "E")
                        {

                            MGROUP.ITGRPCD = VE.M_GROUP.ITGRPCD;
                            MGROUP.DTAG = "E";
                            MGROUP.M_AUTONO = VE.M_GROUP.M_AUTONO;
                            var MAXEMDNO = (from p in DB.M_GROUP where p.M_AUTONO == VE.M_GROUP.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                MGROUP.EMD_NO = 0;
                            }
                            else
                            {
                                MGROUP.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                            }

                            DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_GROUP.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_GROUP.M_AUTONO));

                            DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_GROUP.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_GROUP.M_AUTONO));

                        }
                        MGROUP.ITGRPNM = VE.M_GROUP.ITGRPNM;
                        MGROUP.ITGRPTYPE = FC["grptype"].ToString();
                        MGROUP.PRODGRPCD = VE.M_GROUP.PRODGRPCD;
                        MGROUP.HSNCODE = VE.M_GROUP.HSNCODE;
                        MGROUP.BARGENTYPE = VE.M_GROUP.BARGENTYPE;
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_GROUP", MGROUP.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString());
                        if (VE.DefaultAction == "A")
                        {
                            DB.M_GROUP.Add(MGROUP);
                            DB.M_CNTRL_HDR.Add(MCH);
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            DB.Entry(MGROUP).State = System.Data.Entity.EntityState.Modified;
                            DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        }
                        if (VE.UploadDOC != null)
                        {
                            var img = Cn.SaveUploadImage("M_GROUP", VE.UploadDOC, MGROUP.M_AUTONO, MGROUP.EMD_NO.Value);
                            DB.M_CNTRL_HDR_DOC.AddRange(img.Item1);
                            DB.M_CNTRL_HDR_DOC_DTL.AddRange(img.Item2);
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

                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_GROUP", VE.M_GROUP.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString());
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();

                        DB.M_GROUP.Where(x => x.M_AUTONO == VE.M_GROUP.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_GROUP.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_GROUP.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_GROUP.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });

                        DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_GROUP.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_GROUP.M_AUTONO));
                        DB.SaveChanges();

                        DB.M_GROUP.RemoveRange(DB.M_GROUP.Where(x => x.M_AUTONO == VE.M_GROUP.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_GROUP.M_AUTONO));
                        DB.SaveChanges();
                        transaction.Commit();
                        return Content("3");
                    }
                    else
                    {
                        return Content("");
                    }
                }
                catch (Exception ex)
                {
                    Cn.SaveException(ex, "");
                    return Content(ex.Message);
                }
            }
        }
        [HttpPost]
        public ActionResult M_GrpMast(FormCollection FC)
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
                query = query + "end itgrptype from " + dbname + ".m_group a, " + dbname + ".m_cntrl_hdr c ";
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

        private ActionResult ResponsivePrintReport()
        {
            throw new NotImplementedException();
        }

        public ActionResult PrintReport()
        {
            return RedirectToAction("PrintViewer", "RPTViewer");
        }
    }
}