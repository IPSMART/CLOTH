using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Collections;
using System.Data;
using OfficeOpenXml;
using Oracle.ManagedDataAccess.Client;
namespace Improvar.Controllers
{
    public class Rep_PartyItemSummController : Controller
    {
        // GET: M_Grpmas
        Connection Cn = new Connection(); string CS = null;
        MasterHelp masterHelp = new MasterHelp();
        DropDownHelp dropDownHelp = new DropDownHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public ActionResult Rep_PartyItemSumm(string SLCD = "", string FDT = "", string TDT = "", string CHECK = "", string ITGRPCD = "")
        {
            
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Party & Design Summary";
                    PartyitemSummReport VE = new PartyitemSummReport();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    string scmf = CommVar.FinSchema(UNQSNO);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    VE.DropDown_list_ITGRP = dropDownHelp.GetItgrpcdforSelection();
                    VE.ITGRPCD = masterHelp.ComboFill("ITGRPCD", VE.DropDown_list_ITGRP, 0, 1);
                    if (SLCD == "" && FDT == "" && TDT == "" && CHECK == "" && ITGRPCD == "")
                    {
                        //VE.MGRPLIST = (from i in DB.M_TMGRP
                        //               select new MGRPLIST()
                        //               {
                        //                   MGRPCD = i.SLCD,
                        //                   MGRPNM = i.MGRPNM,
                        //                   MGRPTYPE = i.MGRPTYPE
                        //               }).OrderBy(x => x.MGRPNM).ToList();
                    }
                    else
                    {                                    
                        VE.SLCD = SLCD; VE.FDT = FDT; VE.TDT = TDT;  VE.ITGRPCD = ITGRPCD;
                        if (CHECK == "Y")
                        {
                            VE.CHECK = true;
                        }
                        else
                        {
                            VE.CHECK =false;
                        }

                        //VE.MGRPNM = DB.M_TMGRP.Where(a => a.MGRPCD == MGRPCD).Select(a => a.MGRPNM).SingleOrDefault();
                        //var base64 = DB.M_TGRP.Where(a => a.MGRPCD == MGRPCD).ToList();
                        //if (base64.Any())
                        //{
                        //    VE.FOUNDMGRP = true;
                        //}
                        //var group = (from i in DB.M_TGRP
                        //             where (i.MGRPCD == MGRPCD && i.GLCD == null)
                        //             select i).OrderBy(a => a.ROOTCD).ThenBy(a => a.GRPCDFULL).ToList();

                            //if (group.Any())
                            //{
                            //    List<Temp_TGRP> MLIST = new List<Temp_TGRP>();
                            //    VE.Tree = GenerateTree(group, ref MLIST);
                            //    VE.MLIST = MLIST;
                            //}
                            //var Mtype = DB.M_TMGRP.Where(a => a.MGRPCD == MGRPCD).Select(a => a.MGRPTYPE).SingleOrDefault();
                            //DataTable dt = new DataTable();
                            //if (Mtype == "SL")
                            //{
                            //    //string str = "select * from (select glcd, '' slcd, '' class1cd, glnm,'' slnm, '' class1nm from " + scmf + ".m_genleg where nvl(slcdmust,'N') = 'N' union all ";
                            //    //str = str + " select a.glcd, '', '' class1cd, a.glnm, c.slnm, '' class1nm from " + CommVar.CurSchema(UNQSNO) + ".m_genleg a, " + scmf + ".m_subleg_gl b, " + scmf + ".m_subleg c where a.glcd = b.glcd ";
                            //    //str = str + " and nvl(slcdmust,'N') = 'Y' and b.slcd = c.slcd) where glcd||slcd||class1cd not in (select glcd||slcd||class1cd from " + CommVar.CurSchema(UNQSNO) + ".m_tgrp where mgrpcd = '" + MGRPCD + "' and glcd||slcd||class1cd is not null)";
                            //    //DataTable dt = masterHelp.SQLquery(str);
                            //    //VE.AvailableGroup = (from i in dt.AsEnumerable()
                            //    //                     select new AvailableACGroup()
                            //    //                     {
                            //    //                         Checked = false,
                            //    //                         CLASS1CD = i.Field<string>("class1cd"),
                            //    //                         CLASS1NM = i.Field<string>("class1nm"),
                            //    //                         GLCD = i.Field<string>("glcd"),
                            //    //                         GLNM = i.Field<string>("glnm"),
                            //    //                         SLCD = i.Field<string>("slcd"),
                            //    //                         SLNM = i.Field<string>("slnm"),
                            //    //                     }).ToList();
                            //}
                            //else if (Mtype == "GL")
                            //{
                            //    string str = "select glcd, '' class1cd, glnm, '' class1nm from " + scmf + ".m_genleg where glcd not in (select glcd from " + scmf + ".m_tgrp where mgrpcd = '" + MGRPCD + "' and glcd is not null) order by glnm";
                            //    dt = masterHelp.SQLquery(str);
                            //}
                            //else if (Mtype == "CL" || Mtype == "SL")
                            //{
                            //    string str = "select * from (select glcd, '' class1cd, glnm, '' class1nm from " + scmf + ".m_genleg where nvl(class1cdmust,'N') = 'N' union all ";
                            //    str = str + "select distinct a.glcd, b.class1cd, c.glnm, e.class1nm from " + scmf + ".t_vch_det a, " + scmf + ".t_vch_class b, " + scmf + ".m_genleg c, ";
                            //    str = str + scmf + ".m_class1 e where a.autono = b.autono and a.slno = b.dslno and a.glcd = c.glcd and b.class1cd = e.class1cd and nvl(c.class1cdmust, 'N')= 'Y' order by glcd,class1cd) ";
                            //    str = str + " where glcd|| class1cd not in (select glcd || class1cd from " + scmf + ".m_tgrp where mgrpcd = '" + MGRPCD + "' and glcd|| class1cd is not null)";
                            //    dt = masterHelp.SQLquery(str);
                            //}
                            //VE.AvailableGroup = (from i in dt.AsEnumerable()
                            //                     select new AvailableACGroup()
                            //                     {
                            //                         Checked = false,
                            //                         CLASS1CD = i.Field<string>("class1cd"),
                            //                         CLASS1NM = i.Field<string>("class1nm"),
                            //                         GLCD = i.Field<string>("glcd"),
                            //                         GLNM = i.Field<string>("glnm"),
                            //                     }).ToList();
                        }
                    VE.DefaultView = true;
                    //List<DropDown_list1> drplist = new List<DropDown_list1>();
                    //DropDown_list1 drop1 = new DropDown_list1();
                    //drop1.text = "Yes"; drop1.value = "Y"; drplist.Add(drop1);
                    //DropDown_list1 drop2 = new DropDown_list1();
                    //drop2.text = "No"; drop2.value = "N"; drplist.Add(drop2);
                    //DropDown_list1 drop3 = new DropDown_list1();
                    //drop3.text = "Main"; drop3.value = "M"; drplist.Add(drop3);
                    //VE.SchedulePart = drplist;
                    //VE.LEGDTLSKP = false;

                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                PartyitemSummReport VE = new PartyitemSummReport();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                return View(VE);
            }
        }
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
                    PartyitemSummReport VE = new PartyitemSummReport();
                    Cn.getQueryString(VE);
                    switch (VE.DOC_CODE)
                    {
                        case "SORD": Code = "D"; break;
                        case "PORD": Code = "C"; break;
                        default: Code = "D"; break;
                    }

                }
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

        public string BtnSubmit(string slcd, string fdt, string tdt, string check, string itgrpcd)
        {
            try
            {
                string url = "";
                var PreviousUrl = System.Web.HttpContext.Current.Request.UrlReferrer.AbsoluteUri;
                var uri = new Uri(PreviousUrl);//Create Virtually Query String
                var queryString = System.Web.HttpUtility.ParseQueryString(uri.Query);
                if (queryString.Get("SLCD").retStr() == "" && queryString.Get("FDT").retStr() == "" && queryString.Get("TDT").retStr() == "" && queryString.Get("CHECK").retStr() == "" && queryString.Get("ITGRPCD").retStr() == "")
                {
                    url = Request.UrlReferrer.ToString() + "&SLCD=" + slcd + "&FDT=" + fdt + "&TDT=" + tdt + "&CHECK=" + check + "&ITGRPCD=" + itgrpcd;
                }
                else
                {
                    string dd = Request.UrlReferrer.ToString();
                    int pos = Request.UrlReferrer.ToString().IndexOf("&SLCD=");
                    url = dd.Substring(0, pos);
                    url = url + "&SLCD=" + slcd;
                }
                return url;
            }
            catch (Exception)
            {
                return null;
            }
        }


        //public ActionResult BSGroupCode(string val)
        //{
        //    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
        //    var query = (from c in DB1.MS_GLHDGRP where (c.GLHDGRPCD == val) select c);
        //    if (query.Any())
        //    {
        //        string str = "";
        //        foreach (var i in query)
        //        {
        //            str = i.GLHDGRPCD + Cn.GCS() + i.GLHDGRPNM;
        //        }
        //        return Content(str);
        //    }
        //    else
        //    {
        //        return Content("0");
        //    }
        //}
        //public ActionResult GetBSGroupCode()
        //{
        //    ImprovarDB DBIMP = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
        //    return PartialView("_Help2", masterHelp.GLHDGRPCD_help(DBIMP));
        //}
        //public ActionResult CreateMainGroup(string Gname, string Budgt, string BSCode, string Gdetails, string MGRPCD)
        //{
        //    BalanceSheetGroup VE = new BalanceSheetGroup();
        //    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
        //    if (VE.Add != "A")
        //    {
        //        return Content("You have no permission to create Group.Please add from next year!");
        //    }
        //    M_TGRP MTG = new M_TGRP();
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
        //    using (var transaction = DB.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            MTG.EMD_NO = 0;
        //            MTG.CLCD = CommVar.ClientCode(UNQSNO);
        //            MTG.MGRPCD = MGRPCD;
        //            var gcd = DB.M_TGRP.Where(a => a.MGRPCD == MGRPCD).Select(a => a.ROOTCD).Max();
        //            if (gcd == null)
        //            {
        //                gcd = "1";
        //            }
        //            else
        //            {
        //                gcd = (Convert.ToInt32(gcd) + 1).ToString();
        //            }
        //            MTG.GRPSLNO = Convert.ToDecimal(gcd);
        //            MTG.GCD = gcd.PadLeft(6, '0');
        //            MTG.ROOTCD = MTG.GCD;
        //            MTG.GRPCDFULL = MTG.GCD;
        //            MTG.GRPNM = Gname;
        //            MTG.GRPNMDTL = Gdetails;
        //            MTG.BUDGTAMT = Budgt == "" ? 0 : Convert.ToDecimal(Budgt);
        //            MTG.GLHDGRPCD = BSCode;
        //            DB.M_TGRP.Add(MTG);
        //            DB.SaveChanges();
        //            transaction.Commit();
        //            if (Session["account_treeview"] != null)
        //            {
        //                Session.Remove("account_treeview");
        //            }
        //            return Content("0");
        //        }
        //        catch (Exception ex)
        //        {
        //            return Content(ex.Message);
        //        }
        //    }
        //}
        public string GenerateTree()
        {
            return null;
            //string liid = "";
            //if (Session["account_treeview"] != null)
            //{
            //    liid = Session["account_treeview"].ToString();
            //}
            //Hashtable Main_Menu;
            //Hashtable Main_Menu_Head;
            //ArrayList ManuLine = null;
            //string Child = null;
            //string ReParent = null;
            //int Reuse = 0;
            //Stack ST1 = new Stack();
            //var results = from row in TGRP
            //              where row.PARENTCD == null
            //              orderby row.GRPSLNO
            //              select row;
            //Main_Menu = new Hashtable();
            //Main_Menu_Head = new Hashtable();
            //ManuLine = new ArrayList();
            //foreach (var menu_row in results)
            //{
            //    bool autochk = false;
            //    string parent = menu_row.GRPCDFULL;
            //    string menuid_Child = menu_row.GCD;
            //    string mname = menu_row.GRPNM;
            //    string parentchild = parent;
            //    if (liid == parentchild)
            //    {
            //        autochk = true;
            //    }
            //    string syntax = "";
            //    if (autochk)
            //    {
            //        syntax = "checked='checked'";
            //    }
            //    string Menu = "<li>";
            //    //Menu = Menu + "<input type='checkbox' "+ syntax + "  id='" + parent + "'/>" + "<label class='tree_label' for='" + parent + "'><img src='../Image/Glow.png' class='groupimg'/>&nbsp;<span id='" + mname.Replace(' ', '_') + "'>" + mname + "<script>Rmenu('" + mname.Replace(' ', '_') + "','" + menuid_Child + "^" + parent + "',3,'" + parentchild + "');</script></span></label>";
            //    Menu = Menu + "<input type='checkbox' " + syntax + "  id='C" + parent + "'/>" + "<label class='tree_label' for='C" + parent + "'><img src='../Image/Glow.png' class='groupimg'/>&nbsp;<span id='" + parent + "'>" + mname + "<script>Rmenu('" + parent + "','" + menuid_Child + "^" + parent + "',3,'" + parentchild + "');</script></span></label>";
            //    Menu = Menu + "<ul>";
            //    Main_Menu.Add(parent, mname);
            //    ManuLine.Add(Menu);
            //    Temp_TGRP TTG = new Temp_TGRP();
            //    TTG.GCD = menu_row.GCD;
            //    TTG.GRPCDFULL = menu_row.GRPCDFULL;
            //    TTG.GRPNM = menu_row.GRPNM;
            //    TTG.GRPSLNO = Convert.ToInt32(menu_row.GRPSLNO);
            //    TTG.MGRPCD = menu_row.MGRPCD;
            //    TTG.PARENTCD = menu_row.PARENTCD;
            //    TTG.ROOTCD = menu_row.ROOTCD;
            //    TTG.Space = "";
            //    MLIST.Add(TTG);
            //    Main_Menu_Head.Add(parent, ManuLine.Count - 1);
            //    Child = menuid_Child;
            //    ReParent = parent;
            //    ST1.Push(menuid_Child + "." + Reuse + "." + ReParent);
            //    while (true)
            //    {
            //        var results1 =
            //        (from row in TGRP
            //         where row.PARENTCD == Child
            //         orderby row.GRPSLNO
            //         select row).ToList();
            //        if (results1.Any() == true)
            //        {
            //            for (int x = Reuse; x <= results1.Count() - 1; x++)
            //            {
            //                var boundTable = results1[x];
            //                string parent1 = boundTable.GRPCDFULL;
            //                string menuid_Child1 = boundTable.GCD;
            //                string mname1 = boundTable.GRPNM;
            //                string space = "";
            //                int sp_count = boundTable.GRPCDFULL.Length / 6;
            //                for (int p = 0; p <= sp_count - 1; p++)
            //                {
            //                    space = space + "&nbsp;&nbsp;&nbsp;&nbsp;";
            //                }
            //                string SubMenu = "<li>";
            //                SubMenu = SubMenu + "<input type='checkbox' " + syntax + "  id='C" + parent1 + "'/>" + "<label class='tree_label' for='C" + parent1 + "'><img src='../Image/Generic.png' class='groupimg'/>&nbsp;<span id='" + parent1 + "'>" + boundTable.GRPNM + "<script>Rmenu('" + parent1 + "','" + menuid_Child1 + "^" + parent1 + "',3,'" + parentchild + "');</script></span></label>";
            //                SubMenu = SubMenu + "<ul>";
            //                Main_Menu.Add(parent1, boundTable.GRPNM);
            //                ManuLine.Add(SubMenu);
            //                Temp_TGRP TTG1 = new Temp_TGRP();
            //                TTG1.GCD = boundTable.GCD;
            //                TTG1.GRPCDFULL = boundTable.GRPCDFULL;
            //                TTG1.GRPNM = boundTable.GRPNM;
            //                TTG1.GRPSLNO = Convert.ToInt32(boundTable.GRPSLNO);
            //                TTG1.MGRPCD = boundTable.MGRPCD;
            //                TTG1.PARENTCD = boundTable.PARENTCD;
            //                TTG1.ROOTCD = boundTable.ROOTCD;
            //                TTG1.Space = space;
            //                MLIST.Add(TTG1);
            //                Main_Menu_Head.Add(parent1, ManuLine.Count - 1);
            //                Child = menuid_Child1;
            //                ReParent = parent1;
            //                Reuse = 0;
            //                ST1.Push(menuid_Child1 + "." + Reuse + "." + ReParent);
            //                break;
            //            }
            //            if (Reuse > results1.Count() - 1)
            //            {
            //                if (ST1.Count > 0)
            //                {
            //                    ST1.Pop();
            //                    ManuLine.Add("</ul> </li>");
            //                    if (ST1.Count == 0)
            //                    {
            //                        Reuse = 0;
            //                        break;
            //                    }
            //                    string str = ST1.Pop().ToString();
            //                    string[] getParent;
            //                    getParent = str.Split('.');
            //                    Child = getParent[0];
            //                    ReParent = getParent[2];
            //                    Reuse = Convert.ToInt32(getParent[1]);
            //                    Reuse += 1;
            //                    ST1.Push(Child + "." + Reuse + "." + ReParent);
            //                }
            //                else
            //                {
            //                    break;
            //                }
            //            }
            //        }
            //        else
            //        {
            //            if (ST1.Count > 0)
            //            {
            //                string Recall = ST1.Peek().ToString();
            //                string[] RecallDetails;
            //                RecallDetails = Recall.Split('.');
            //                int index = (int)Main_Menu_Head[RecallDetails[2]];
            //                string Manuname = (string)Main_Menu[RecallDetails[2]];
            //                string Controller = "";
            //                string SubMenu = " <li><span class='tree_label'>";
            //                if (RecallDetails[0] == RecallDetails[2])  //+ RecallDetails[2] is fullgrpcd
            //                {
            //                    SubMenu = SubMenu + "<img src='../Image/Glow.png' class='groupimg'/>&nbsp;<span id='" + RecallDetails[2] + "' onclick=ExistTag('" + RecallDetails[0] + "^" + RecallDetails[2] + "');>" + Manuname + "<script>Rmenu('" + RecallDetails[2] + "','" + RecallDetails[0] + "^" + RecallDetails[2] + "',2,'" + parentchild + "');</script></span>&nbsp;";
            //                }
            //                else
            //                {
            //                    SubMenu = SubMenu + "<img src='../Image/Generic.png' class='groupimg'/>&nbsp;<span id='" + RecallDetails[2] + "' onclick=ExistTag('" + RecallDetails[0] + "^" + RecallDetails[2] + "');>" + Manuname + "<script>Rmenu('" + RecallDetails[2] + "','" + RecallDetails[0] + "^" + RecallDetails[2] + "',2,'" + parentchild + "');</script></span>&nbsp;";
            //                }
            //                SubMenu = SubMenu + "</span></li>";
            //                ManuLine.RemoveAt(index);
            //                ManuLine.Insert(index, SubMenu);
            //                ST1.Pop();
            //                if (ST1.Count == 0)
            //                {
            //                    Reuse = 0;
            //                    break;
            //                }
            //                string str = ST1.Pop().ToString();
            //                string[] getParent;
            //                getParent = str.Split('.');
            //                Child = getParent[0];
            //                ReParent = getParent[2];
            //                Reuse = Convert.ToInt32(getParent[1]);
            //                Reuse += 1;
            //                ST1.Push(Child + "." + Reuse + "." + ReParent);
            //            }
            //            else
            //            {
            //                break;
            //            }
            //        }
            //    }
            //}
            //System.Text.StringBuilder SB = new System.Text.StringBuilder();
            //if (ManuLine != null)
            //{
            //    for (int i = 0; i <= ManuLine.Count - 1; i++)
            //    {
            //        SB.Append(ManuLine[i].ToString());
            //    }
            //}
            //return SB.ToString();
        }
        //public ActionResult CreateSubGroup(string Gname, string Budgt, string Gdetails, string MGRPCD, string Parent, string Liid, string legdtlskp, string schdl)
        //{
        //    M_TGRP MTG = new M_TGRP();
        //    string[] Parameter = Parent.Split('^');
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
        //    using (var transaction = DB.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            MTG.EMD_NO = 0;
        //            MTG.CLCD = CommVar.ClientCode(UNQSNO);
        //            MTG.MGRPCD = MGRPCD;
        //            string Gcd = Parameter[0];
        //            var Rootcd = DB.M_TGRP.Where(a => a.MGRPCD == MGRPCD && a.GCD == Gcd).Select(a => a.ROOTCD).FirstOrDefault();
        //            var gcd = DB.M_TGRP.Where(a => a.MGRPCD == MGRPCD && a.ROOTCD == Rootcd && a.PARENTCD == Gcd).Select(a => a.GRPSLNO).Max();
        //            var CODE = DB.M_TGRP.Where(a => a.MGRPCD == MGRPCD && a.PARENTCD != null).Select(a => a.GCD).Max();
        //            if (gcd == null)
        //            {
        //                gcd = 1;
        //            }
        //            else
        //            {
        //                gcd = gcd + 1;
        //            }
        //            int temp = 0;
        //            if (CODE == null)
        //            {
        //                temp = 1;
        //            }
        //            else
        //            {
        //                temp = Convert.ToInt32(CODE.Substring(1)) + 1;
        //            }
        //            MTG.GRPSLNO = gcd;
        //            MTG.GCD = "S" + temp.ToString().PadLeft(5, '0');
        //            MTG.ROOTCD = Rootcd;
        //            MTG.PARENTCD = Gcd;
        //            MTG.GRPCDFULL = Parameter[1] + MTG.GCD;
        //            MTG.GRPNM = Gname.Replace("'", "`");
        //            MTG.GRPNMDTL = Gdetails;
        //            MTG.BUDGTAMT = Budgt == "" ? 0 : Convert.ToDecimal(Budgt);
        //            MTG.LEGDTLSKP = legdtlskp == "true" ? "Y" : "";
        //            MTG.SCHDL = schdl;
        //            DB.M_TGRP.Add(MTG);
        //            DB.SaveChanges();
        //            transaction.Commit();
        //            if (Session["account_treeview"] == null)
        //            {
        //                Session.Add("account_treeview", Liid);
        //            }
        //            else
        //            {
        //                Session["account_treeview"] = Liid;
        //            }
        //            return Content("0");
        //        }
        //        catch (Exception ex)
        //        {
        //            return Content(ex.Message);
        //        }
        //    }
        //}
        //public ActionResult GetGRPCDDTL(string Parent)
        //{
        //    try
        //    {
        //        if (Parent.retStr() != "")
        //        {
        //            string[] Parameter = Parent.Split('^');
        //            if (Parameter.Length > 1)
        //            {
        //                var GRPCDFULL = Parameter[1];
        //                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
        //                //ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), CommVar.CommSchema());
        //                var M_SUBLEGGRPlst = DB.M_TGRP.Where(D => D.GRPCDFULL == GRPCDFULL).ToList().Take(1);
        //                var str = masterHelp.ToReturnFieldValues(M_SUBLEGGRPlst, null);
        //                return Content(str);
        //            }
        //        }
        //        return Content("No Parent Found");
        //    }
        //    catch (Exception ex)
        //    {
        //        return Content(ex.Message);
        //    }
        //}
        //public ActionResult RenameGroup(string Gname, string MGRPCD, string Parent, string Details, string Budgt, string Liid, string legdtlskp, string schdl)
        //{
        //    M_TGRP MTG = new M_TGRP();
        //    string[] Parameter = Parent.Split('^');
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
        //    decimal B_amt = Budgt == "" ? 0 : Convert.ToDecimal(Budgt);
        //    using (var transaction = DB.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            string Gcd = Parameter[0];
        //            string gfull = Parameter[1];
        //            var emd = DB.M_TGRP.Where(a => a.MGRPCD == MGRPCD && a.GCD == Gcd && a.GRPCDFULL == gfull).Select(a => a.EMD_NO).SingleOrDefault();
        //            emd += 1;
        //            DB.M_TGRP.Where(a => a.MGRPCD == MGRPCD && a.GCD == Gcd && a.GRPCDFULL == gfull).ToList().ForEach(x =>
        //            {
        //                x.GRPNM = Gname.Replace("'", "`");
        //                x.EMD_NO = emd; x.GRPNMDTL = Details; x.BUDGTAMT = B_amt; x.LEGDTLSKP = legdtlskp == "true" ? "Y" : ""; x.SCHDL = schdl;
        //            });
        //            DB.SaveChanges();
        //            transaction.Commit();
        //            if (Session["account_treeview"] == null)
        //            {
        //                Session.Add("account_treeview", Liid);
        //            }
        //            else
        //            {
        //                Session["account_treeview"] = Liid;
        //            }
        //            return Content("0");
        //        }
        //        catch (Exception ex)
        //        {
        //            return Content(ex.Message);
        //        }
        //    }
        //}
        //public ActionResult DeleteGroup(string MGRPCD, string Parent, string Liid)
        //{
        //    M_TGRP MTG = new M_TGRP();
        //    string[] Parameter = Parent.Split('^');
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
        //    using (var transaction = DB.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            string Gcd = Parameter[0];
        //            string gfull = Parameter[1];
        //            List<String> GetGCD = GetHIRELINK(Gcd, CommVar.CurSchema(UNQSNO), MGRPCD);
        //            var AR = GetGCD.Distinct().ToList();
        //            DB.M_TGRP.RemoveRange(DB.M_TGRP.Where(a => a.MGRPCD == MGRPCD && AR.Contains(a.GCD)));
        //            DB.SaveChanges();
        //            transaction.Commit();
        //            if (Session["account_treeview"] == null)
        //            {
        //                Session.Add("account_treeview", Liid);
        //            }
        //            else
        //            {
        //                Session["account_treeview"] = Liid;
        //            }
        //            return Content("0");
        //        }
        //        catch (Exception ex)
        //        {
        //            return Content(ex.Message);
        //        }
        //    }
        //}
        //public string urlRenameValue(string code)
        //{
        //    try
        //    {
        //        string url = "";
        //        var PreviousUrl = System.Web.HttpContext.Current.Request.UrlReferrer.AbsoluteUri;
        //        var uri = new Uri(PreviousUrl);//Create Virtually Query String
        //        var queryString = System.Web.HttpUtility.ParseQueryString(uri.Query);
        //        if (queryString.Get("MGRPCD") == null)
        //        {
        //            url = Request.UrlReferrer.ToString() + "&MGRPCD=" + code;
        //        }
        //        else
        //        {
        //            string dd = Request.UrlReferrer.ToString();
        //            int pos = Request.UrlReferrer.ToString().IndexOf("&MGRPCD=");
        //            url = dd.Substring(0, pos);
        //            url = url + "&MGRPCD=" + code;
        //        }
        //        return url;
        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }
        //}
        //public ActionResult IndexGroup(BalanceSheetGroup VE, string MGRPCD)
        //{
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
        //    using (var transaction = DB.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            foreach (var i in VE.MLIST)
        //            {
        //                DB.M_TGRP.Where(a => a.MGRPCD == MGRPCD && a.GCD == i.GCD && a.GRPCDFULL == i.GRPCDFULL && a.ROOTCD == i.ROOTCD).ToList().ForEach(x => { x.GRPSLNO = i.GRPSLNO; });
        //            }
        //            DB.SaveChanges();
        //            transaction.Commit();
        //            if (Session["account_treeview"] != null)
        //            {
        //                Session.Remove("account_treeview");
        //            }
        //            return Content("0");
        //        }
        //        catch (Exception ex)
        //        {
        //            return Content(ex.Message);
        //        }
        //    }
        //}
        //public ActionResult ADDACCOUNT(BalanceSheetGroup VE, string MGRPCD, string Parent, string Liid)
        //{
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
        //    string[] Parameter = Parent.Split('^');
        //    List<M_TGRP> MTG1 = new List<M_TGRP>();
        //    string Gcd = Parameter[0];
        //    string gfull = Parameter[1];
        //    using (var transaction = DB.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            var temmp = DB.M_TGRP.Where(a => a.MGRPCD == MGRPCD && a.GCD == Gcd && a.GRPCDFULL == gfull).Select(a => a).SingleOrDefault();
        //            foreach (var i in VE.AvailableGroup)
        //            {
        //                if (i.Checked)
        //                {
        //                    M_TGRP MTG = new M_TGRP();
        //                    MTG.BUDGTAMT = temmp.BUDGTAMT;
        //                    MTG.CLCD = temmp.CLCD;
        //                    MTG.DTAG = temmp.DTAG;
        //                    MTG.EMD_NO = temmp.EMD_NO;
        //                    MTG.GCD = temmp.GCD;
        //                    MTG.GLHDGRPCD = temmp.GLHDGRPCD;
        //                    MTG.GRPNM = temmp.GRPNM;
        //                    MTG.GRPNMDTL = temmp.GRPNMDTL;
        //                    MTG.MGRPCD = MGRPCD;
        //                    MTG.PARENTCD = temmp.PARENTCD;
        //                    MTG.ROOTCD = temmp.ROOTCD;
        //                    MTG.GLCD = i.GLCD;
        //                    MTG.CLASS1CD = i.CLASS1CD;
        //                    MTG.GRPCDFULL = gfull + i.GLCD + i.CLASS1CD;
        //                    MTG.GRPSLNO = temmp.GRPSLNO;
        //                    MTG.LEGDTLSKP = temmp.LEGDTLSKP;
        //                    MTG.SCHDL = "Y";
        //                    MTG1.Add(MTG);
        //                }
        //            }
        //            DB.M_TGRP.AddRange(MTG1);
        //            DB.SaveChanges();
        //            transaction.Commit();
        //            if (Session["account_treeview"] == null)
        //            {
        //                Session.Add("account_treeview", Liid);
        //            }
        //            else
        //            {
        //                Session["account_treeview"] = Liid;
        //            }
        //            return Content("0");
        //        }
        //        catch (Exception ex)
        //        {
        //            return Content(ex.Message);
        //        }
        //    }
        //}
        //public ActionResult ExistingAccount(BalanceSheetGroup VE, string MGRPCD, string Parent)
        //{
        //    try
        //    {
        //        string[] Parameter = Parent.Split('^');
        //        string Gcd = Parameter[0];
        //        string gfull = Parameter[1];
        //        ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
        //        string scmf = CommVar.FinSchema(UNQSNO);
        //        DataTable dt = new DataTable();
        //        var Mtype = DB.M_TMGRP.Where(a => a.MGRPCD == MGRPCD).Select(a => a.MGRPTYPE).SingleOrDefault();
        //        if (Mtype == "SL")
        //        {
        //            //string str = "select * from (select glcd,'' slcd,'' class1cd,glnm,'' slnm,'' class1nm from " + CommVar.CurSchema(UNQSNO) + ".m_genleg where nvl(slcdmust,'N') = 'N' union all ";
        //            //str = str + " select a.glcd,b.slcd,'' class1cd,a.glnm,c.slnm,'' class1nm from " + CommVar.CurSchema(UNQSNO) + ".m_genleg a, " + CommVar.CurSchema(UNQSNO) + ".m_subleg_gl b, " + CommVar.CurSchema(UNQSNO) + ".m_subleg c where a.glcd = b.glcd ";
        //            //str = str + " and nvl(slcdmust,'N') = 'Y' and b.slcd = c.slcd) where glcd||slcd||class1cd in (select glcd||slcd||class1cd from " + CommVar.CurSchema(UNQSNO) + ".m_tgrp where mgrpcd = '" + MGRPCD + "' and gcd='" + Gcd + "'  and glcd||slcd||class1cd is not null)";
        //            //DataTable dt = masterHelp.SQLquery(str);
        //            //VE.ExistingGroup = (from i in dt.AsEnumerable()
        //            //                     select new AvailableACGroup()
        //            //                     {
        //            //                         Checked = false,
        //            //                         CLASS1CD = i.Field<string>("class1cd"),
        //            //                         CLASS1NM = i.Field<string>("class1nm"),
        //            //                         GLCD = i.Field<string>("glcd"),
        //            //                         GLNM = i.Field<string>("glnm"),
        //            //                         SLCD = i.Field<string>("slcd"),
        //            //                         SLNM = i.Field<string>("slnm"),
        //            //                     }).ToList();
        //        }
        //        else if (Mtype == "GL")
        //        {
        //            string str = "select glcd, '' class1cd, glnm, '' class1nm from " + scmf + ".m_genleg where glcd in (select glcd from " + scmf + ".m_tgrp where mgrpcd = '" + MGRPCD + "' and gcd='" + Gcd + "'  and glcd is not null) order by glnm ";
        //            dt = masterHelp.SQLquery(str);
        //        }
        //        else if (Mtype == "CL" || Mtype == "SL")
        //        {
        //            string str = "select * from (select glcd, '' class1cd, glnm, '' class1nm from " + scmf + ".m_genleg where nvl(class1cdmust,'N') = 'N' union all ";
        //            str = str + "select distinct a.glcd, b.class1cd, c.glnm, e.class1nm from " + scmf + ".t_vch_det a, " + scmf + ".t_vch_class b, " + scmf + ".m_genleg c, ";
        //            str = str + scmf + ".m_class1 e where a.autono = b.autono and a.slno = b.dslno and a.glcd = c.glcd and b.class1cd = e.class1cd and nvl(c.class1cdmust, 'N')= 'Y' order by glcd,class1cd) ";
        //            str = str + "where glcd|| class1cd in (select glcd || class1cd from " + scmf + ".m_tgrp where mgrpcd = '" + MGRPCD + "' and gcd='" + Gcd + "'  and glcd|| class1cd is not null)";
        //            dt = masterHelp.SQLquery(str);
        //        }
        //        VE.ExistingGroup = (from i in dt.AsEnumerable()
        //                            select new AvailableACGroup()
        //                            {
        //                                Checked = false,
        //                                CLASS1CD = i.Field<string>("class1cd"),
        //                                CLASS1NM = i.Field<string>("class1nm"),
        //                                GLCD = i.Field<string>("glcd"),
        //                                GLNM = i.Field<string>("glnm"),
        //                            }).ToList();
        //        ModelState.Clear();
        //        return PartialView("_tree_ExistingTag", VE);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Content("0^" + ex.Message);
        //    }
        //}
        //public ActionResult DELETEACCOUNT(BalanceSheetGroup VE, string MGRPCD, string Parent, string Liid)
        //{
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
        //    string[] Parameter = Parent.Split('^');
        //    string Gcd = Parameter[0];
        //    string gfull = Parameter[1];
        //    using (var transaction = DB.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            foreach (var i in VE.ExistingGroup)
        //            {
        //                if (i.Checked)
        //                {
        //                    DB.M_TGRP.RemoveRange(DB.M_TGRP.Where(a => a.MGRPCD == MGRPCD && a.GCD == Gcd && a.GLCD == i.GLCD));
        //                }
        //            }
        //            DB.SaveChanges();
        //            transaction.Commit();
        //            if (Session["account_treeview"] == null)
        //            {
        //                Session.Add("account_treeview", Liid);
        //            }
        //            else
        //            {
        //                Session["account_treeview"] = Liid;
        //            }
        //            return Content("0");
        //        }
        //        catch (Exception ex)
        //        {
        //            return Content(ex.Message);
        //        }
        //    }
        //}
        //public List<string> GetHIRELINK(string EGCD, string SCHEMA, string MGRPCD)
        //{
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), SCHEMA);
        //    List<String> AR = new List<String>();
        //    AR.Add(EGCD);
        //    System.Collections.Stack STK = new System.Collections.Stack();
        //    var EMPLO = (from i in DB.M_TGRP where (i.PARENTCD == EGCD && i.MGRPCD == MGRPCD) orderby i.GRPSLNO select i.GCD).ToList();
        //    for (int i = 0; i <= EMPLO.Count() - 1; i++)
        //    {
        //        string EMPCD2 = EMPLO[i];
        //        AR.Add(EMPCD2);
        //        STK.Push(EMPCD2);
        //        while (true)
        //        {
        //            string Qid = (string)STK.Pop();
        //            var EMPLO1 = (from q in DB.M_TGRP where (q.PARENTCD == Qid && q.MGRPCD == MGRPCD) orderby q.GRPSLNO select q.GCD).ToList();
        //            if (EMPLO1.Any() == false)
        //            {
        //                if (STK.Count <= 0)
        //                {
        //                    break;
        //                }
        //            }
        //            else
        //            {
        //                for (int x = 0; x <= EMPLO1.Count() - 1; x++)
        //                {
        //                    STK.Push(EMPLO1[x]);
        //                    AR.Add(EMPLO1[x]);
        //                }
        //            }
        //        }
        //    }
        //    return AR;
        //}
        //public ActionResult DownloadExcel(string MGRPCD)
        //{
        //    try
        //    {
        //        ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
        //        string MGRPNM = DB.M_TMGRP.Where(a => a.MGRPCD == MGRPCD).FirstOrDefault()?.MGRPNM;
        //        DataTable dt = new DataTable();
        //        dt.Columns.Add("GrpHdr1", typeof(string));
        //        dt.Columns.Add("GrpHdr2", typeof(string));
        //        dt.Columns.Add("GrpHdr3", typeof(string));
        //        dt.Columns.Add("GrpHdr4", typeof(string));
        //        dt.Columns.Add("GrpHdr5", typeof(string));
        //        dt.Columns.Add("LegerName", typeof(string));
        //        dt.Columns.Add("GLCD", typeof(string));
        //        //dt.Columns.Add("slcdgrpnmdesc", typeof(string));
        //        string str = "";

        //        str = " select grpcdfull,";
        //        str += " a.slcd,CASE WHEN nvl(a.slcd, ' ') = ' ' THEN A.GRPNM  else B.SLNM end SLNM,";
        //        str += " C.GLCD,CASE WHEN nvl(a.GLCD, ' ') = ' ' THEN A.GRPNM  else C.GLNM end GRPNM";
        //        str += " from " + CommVar.FinSchema(UNQSNO) + ".M_tGRP a, " + CommVar.FinSchema(UNQSNO) + ".m_subleg b, " + CommVar.FinSchema(UNQSNO) + ".m_GENLEG C ";
        //        str += " where A.SLCD = b.slcd(+) and A.GLCD = C.GLCD(+) AND a.mgrpcd = '" + MGRPCD + "'";
        //        str += " order by grpcdfull,grpslno";
        //        var tmpdt = masterHelp.SQLquery(str);
        //        foreach (DataRow dr in tmpdt.Rows)
        //        {
        //            DataRow newdr = dt.NewRow();
        //            string grpcdfull = dr["grpcdfull"].ToString();
        //            string glcd = dr["GLCD"].ToString();
        //            if (glcd == "")
        //            {
        //                if (grpcdfull.Length % 2 != 0 && glcd != "") grpcdfull = grpcdfull.Substring(0, grpcdfull.Length - 8);

        //                int exlColIndex = grpcdfull.Length / 6;
        //                if (exlColIndex <= 5)
        //                {
        //                    newdr["GrpHdr" + exlColIndex] = dr["GRPNM"];
        //                }
        //                else
        //                {
        //                    newdr["GrpHdr5"] = dr["GRPNM"];
        //                }
        //            }
        //            else
        //            {
        //                newdr["LegerName"] = dr["GRPNM"].ToString();
        //            }
        //            newdr["GLCD"] = dr["GLCD"];
        //            dt.Rows.Add(newdr);
        //        }
        //        using (ExcelPackage pck = new ExcelPackage())
        //        {
        //            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Sheet1");
        //            for (int q = 1; q <= 5; q++)
        //            {
        //                ws.Column(q).Width = 2.82;
        //                ws.Column(q).Style.Font.Bold = true;
        //            }
        //            ws.Column(6).Width = 35;
        //            ws.Cells[1, 1].Value = MGRPNM + " analysis as on " + System.DateTime.Now.retDateStr();
        //            ws.Cells["A2"].LoadFromDataTable(dt, true);
        //            Byte[] fileBytes = pck.GetAsByteArray();
        //            Response.ClearContent();
        //            Response.AddHeader("content-disposition", "attachment;filename=" + MGRPNM + " analysis " + DateTime.Now.ToString("dd_HHmm") + ".xlsx");
        //            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //            Response.BinaryWrite(fileBytes);
        //            Response.End();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Content(ex.Message);
        //    }
        //    return Content("Excel Downloded.");
        //}
        //public ActionResult SavePreviousYrData(BalanceSheetGroup VE, FormCollection FC)
        //{
        //    CS = Cn.GetConnectionString();
        //    Cn.con = new OracleConnection(CS);
        //    if ((Cn.ds.Tables["fill_RECORD"] == null) == false)
        //    {
        //        Cn.ds.Tables["fill_RECORD"].Clear();
        //    }
        //    if (Cn.con.State == ConnectionState.Closed)
        //    {
        //        Cn.con.Open();
        //    }
        //    var transaction = Cn.con.BeginTransaction();
        //    try
        //    {
        //        string PrvYrScm = CommVar.FinSchemaPrevYr(UNQSNO);
        //        string Scm = CommVar.CurSchema(UNQSNO);
        //        string sql1 = "select a.glcd,b.glnm from  " + Scm + ".m_tgrp a," + Scm + ".m_genleg b where a.glcd=b.glcd and ";
        //        sql1 += " a.glcd  in( ";
        //        sql1 += "select glcd from " + Scm + ".m_genleg where glcd not in (select glcd from " + PrvYrScm + ".m_genleg )) ";
        //        DataTable MDT = masterHelp.SQLquery(sql1);
        //        if (MDT.Rows.Count > 0) { transaction.Rollback(); return Content(""+MDT.Rows[0]["glnm"].retStr()+"("+MDT.Rows[0]["glcd"].retStr()+") not found at the last year !!"); }
        //        string sql = " delete from  " + PrvYrScm + ".m_tgrp ";
        //        Cn.com = new OracleCommand(sql, Cn.con);
        //        Cn.com.ExecuteNonQuery();
        //        string sqll = " insert into " + PrvYrScm + ".m_tgrp ";
        //        sqll += "(select * from " + Scm + ".m_tgrp where glcd in (select glcd from " + PrvYrScm + ".m_genleg ) or glcd is null) ";
        //        //sqll += "(select * from " + Scm + ".m_tgrp where glcd  in( ";
        //        //sqll += "select glcd from " + Scm + ".m_genleg where glcd not in (select glcd from " + PrvYrScm + ".m_genleg ))) ";

        //        Cn.com = new OracleCommand(sqll, Cn.con);
        //        Cn.com.ExecuteNonQuery();
        //        ModelState.Clear();
        //        transaction.Commit();
        //        return Content("1");
        //    }

        //    catch (Exception ex)
        //    {
        //        transaction.Rollback();
        //        ModelState.Clear();
        //        Cn.SaveException(ex, "");
        //        return Content(ex.Message + " " + ex.InnerException);
        //    }

        //}
    }
}