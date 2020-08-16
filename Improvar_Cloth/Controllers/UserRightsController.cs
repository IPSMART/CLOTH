using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Collections;
using System.IO;
namespace Improvar.Controllers
{
    public class UserRightsController : Controller
    {
        // GET: UserRights
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public ActionResult user_rights()
        {
            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                string SCHEMA = Cn.Getschema;
                UserRight UR = new UserRight();
                ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), SCHEMA);

                string fldcd = "", comptbl = "";
                switch (Module.Module_Code)
                {
                    case "FIN":
                        fldcd = "finaccess"; comptbl = "fin_company"; break;
                    case "INV":
                        fldcd = "invaccess"; comptbl = "inv_company"; break;
                    case "PAY":
                        fldcd = "payaccess"; comptbl = "pay_company"; break;
                    case "IPSMART_ESS":
                        fldcd = "mobapp1"; comptbl = "pay_company"; break;
                    default:
                        fldcd = "saleaccess"; comptbl = "sd_company"; break;
                }
                MasterHelp masterHelp = new MasterHelp();
                string sql = "select * from (select a.user_id, a.user_id||' - '||a.user_name user_name, a." + fldcd + " modacess from " + CommVar.CommSchema() + ".user_appl a ";
                sql += ") where nvl(modacess,'Y') = 'Y' order by user_name";
                DataTable tbl = masterHelp.SQLquery(sql);
                var doctP = (from DataRow dr in tbl.Rows
                             select new DocumentType()
                             {
                                 value = dr["user_id"].ToString(),
                                 text = dr["user_name"].ToString(),
                             }).ToList();
                UR.user = doctP;
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                ImprovarDB DB2 = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                string userid = Session["UR_ID"].ToString();
                var comp = (from i in DB2.M_COMP select new { code = i.COMPCD, name = i.COMPNM }).ToList();
                var loca = (from i in DB2.M_LOCA select new { code = i.LOCCD, name = i.LOCNM, compcd = i.COMPCD }).ToList();

                sql = "select a.compcd, a.loccd, a.locnm, b.compnm ";
                sql += "from " + CommVar.FinSchema(UNQSNO) + ".m_loca a, " + CommVar.FinSchema(UNQSNO) + ".m_comp b, " + comptbl + " c ";
                sql += "where a.compcd=b.compcd(+) and a.compcd||a.loccd = c.compcd||c.loccd and c.schema_name = '" + CommVar.CurSchema(UNQSNO) + "' ";
                sql += "order by compcd, locnm ";
                tbl = masterHelp.SQLquery(sql);
                UR.Comp_List = (from DataRow dr in tbl.Rows
                                select new URightByComp()
                                {
                                    Check = false,
                                    comcd = dr["compcd"].ToString(),
                                    loccd = dr["loccd"].ToString(),
                                    comnm = dr["compnm"].ToString(),
                                    locnm = dr["locnm"].ToString(),
                                }).Distinct().ToList();
                UR.UNQSNO = UNQSNO;
                //string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
                //if (UR.Comp_List.Count == 0)
                //{
                //    UR.Comp_List = (from i in DB1.USER_APPL
                //                    where i.USER_ID == userid
                //                    select new URightByComp()
                //                    {
                //                        Check = false,
                //                        comcd = COM,
                //                        loccd = LOC,
                //                        comnm = "",
                //                        locnm = ""
                //                    }).Distinct().ToList();
                //}
                //foreach (var i in UR.Comp_List)
                //{
                //    i.comnm = (from x in comp where (x.code == i.comcd) select x.name).SingleOrDefault();
                //    i.locnm = (from x in loca where (x.code == i.loccd && x.compcd == i.comcd) select x.name).SingleOrDefault();
                //}
                return View(UR);
            }
        }
        public ActionResult generateTree(UserRight URight, string user)
        {

            var UNQSNO = Cn.getQueryStringUNQSNO();
            Hashtable Main_Menu;
            Hashtable Main_Menu_Head;
            Hashtable Permission_Details;
            ArrayList ManuLine = null;
            string Child = null;
            string ReParent = null;
            int Reuse = 0;
            Stack ST1 = new Stack();
            DataTable MenuTble;
            UserRight UR = new UserRight();
            #region Menugenerate
            List<MenuRightByUser> OnlyMenu = new List<MenuRightByUser>();
            string SCHEMA = Cn.Getschema;
            CS = Cn.GetConnectionString();
            string sql = null;
            string comcode = CommVar.Compcd(UNQSNO);
            string LOCCODE = CommVar.Loccd(UNQSNO);
            sql = "select distinct a.MENU_ID,a.MENU_NAME,a.MENU_INDEX,a.MENU_PARENT_ID,a.MENU_PROGCALL,b.USER_RIGHT,b.E_DAY,b.A_DAY,'Y' AS PERDOTNETMENU,'Y' AS ATVDOTNETMENU,b.D_DAY,a.MENU_ORDER_CODE,b.USER_ID,a.menu_type,a.menu_date_option ,a.PKG_INDEX from ("
                + "Select m.MENU_ID, m.MENU_NAME,m.MENU_INDEX,m.PKG_INDEX, m.MENU_PARENT_ID,  m.MENU_ID||'~'||m.MENU_INDEX||'~'||nvl(m.MENU_PROGCALL,'Notset') as MENU_PROGCALL, 'Y' AS PERDOTNETMENU, 'Y' AS ATVDOTNETMENU, m.MENU_ORDER_CODE,m.menu_type,m.menu_date_option from " + SCHEMA + ".APPL_MENU m";
            sql += " where M.MODULE_CODE = '" + CommVar.ModuleCode() + "')a left join (Select m.MENU_ID, m.MENU_NAME, m.MENU_INDEX, m.MENU_PARENT_ID, m.MENU_ID||'~'||m.MENU_INDEX||'~'||nvl(m.MENU_PROGCALL,'Notset') as MENU_PROGCALL, p.USER_RIGHT, p.E_DAY, p.A_DAY, 'Y' AS PERDOTNETMENU, 'Y' AS ATVDOTNETMENU,"
                + " p.D_DAY, m.MENU_ORDER_CODE, P.USER_ID from " + SCHEMA + ".APPL_MENU m  inner join " + CommVar.CurSchema(UNQSNO) + ".M_USR_ACS p  on p.MENU_NAME = m.MENU_ID and p.MENU_INDEX = m.MENU_INDEX";
            sql += "  where M.MODULE_CODE = '" + CommVar.ModuleCode() + "' and P.USER_ID = '" + user + "' and p.compcd='" + comcode + "' and p.loccd = '" + LOCCODE + "')b on  a.MENU_NAME = b.MENU_NAME and a.MENU_INDEX = b.MENU_INDEX and a.MENU_ID = b.MENU_ID order by a.MENU_ORDER_CODE,a.PKG_INDEX";
            MenuTble = masterHelp.SQLquery(sql);
            if (MenuTble.Rows.Count > 0)
            {
                List<MenuRightByUser> MRU = new List<MenuRightByUser>();
                for (int i = 0; i <= MenuTble.Rows.Count - 1; i++)
                {
                    MenuRightByUser MU = new MenuRightByUser();
                    string userri = MenuTble.Rows[i]["USER_RIGHT"] == DBNull.Value ? "" : MenuTble.Rows[i]["USER_RIGHT"].ToString();
                    if (userri.Length > 0)
                    {
                        for (int j = 0; j <= userri.Length - 1; j++)
                        {
                            string aedv = userri.Substring(j, 1);
                            if (aedv == "A")
                            {
                                MU.Add = true;
                            }
                            else if (aedv == "E")
                            {
                                MU.Edit = true;
                            }
                            else if (aedv == "D")
                            {
                                MU.Delete = true;
                            }
                            else if (aedv == "V")
                            {
                                MU.View = true;
                            }
                            else if (aedv == "C")
                            {
                                MU.Check = true;
                            }
                        }
                    }
                    MU.A_DAY = MenuTble.Rows[i]["A_DAY"] == DBNull.Value ? Convert.ToInt16(0) : Convert.ToInt16(MenuTble.Rows[i]["A_DAY"]);
                    MU.D_DAY = MenuTble.Rows[i]["D_DAY"] == DBNull.Value ? Convert.ToInt16(0) : Convert.ToInt16(MenuTble.Rows[i]["D_DAY"]);
                    MU.E_DAY = MenuTble.Rows[i]["E_DAY"] == DBNull.Value ? Convert.ToInt16(0) : Convert.ToInt16(MenuTble.Rows[i]["E_DAY"]);
                    MU.MENU_INDEX = Convert.ToByte(MenuTble.Rows[i]["MENU_INDEX"]);
                    MU.MENU_ID = MenuTble.Rows[i]["MENU_ID"].ToString();
                    MU.MENU_NAME = MenuTble.Rows[i]["MENU_NAME"].ToString();
                    MU.ParentID = MenuTble.Rows[i]["MENU_PARENT_ID"].ToString();
                    MU.USR_ID = MenuTble.Rows[i]["USER_ID"] == DBNull.Value ? "" : MenuTble.Rows[i]["USER_ID"].ToString();
                    MU.Active = MenuTble.Rows[i]["USER_ID"] == DBNull.Value ? false : true;

                    MU.MENU_PROGCALL = MenuTble.Rows[i]["MENU_PROGCALL"].ToString();
                    MU.MENU_type = MenuTble.Rows[i]["menu_type"] == DBNull.Value ? "" : MenuTble.Rows[i]["menu_type"].ToString();
                    MU.MENU_date_option = MenuTble.Rows[i]["menu_date_option"] == DBNull.Value ? "" : MenuTble.Rows[i]["menu_date_option"].ToString();
                    MRU.Add(MU);
                }
                UR.MenuRightByUser = MRU;
                string IDENTIFIER = Session["MotherMenuIdentifier"].ToString();
                IEnumerable<DataRow> results =
                from row in MenuTble.AsEnumerable()
                where row.Field<string>("MENU_PARENT_ID") == IDENTIFIER
                orderby row.Field<Int16>("PKG_INDEX")
                select row;
                Main_Menu = new Hashtable();
                Main_Menu_Head = new Hashtable();
                Permission_Details = new Hashtable();
                ManuLine = new ArrayList();
                foreach (DataRow menu_row in results)
                {
                    string parent = menu_row.ItemArray[4].ToString().Trim();
                    string menuid_Child = menu_row.ItemArray[0].ToString().Trim() + "!" + menu_row.ItemArray[2].ToString().Trim();
                    string mname = menu_row.ItemArray[1].ToString().Trim();
                    if (menu_row.ItemArray[8].ToString().Trim() == "Y")
                    {
                        string Menu = "<li>";
                        Menu = Menu + "<input type='checkbox'  id='" + parent + "'/>" + "<label class='tree_label' for='" + parent + "'><img src='../Image/folder1.png' class='folderimg'/>&nbsp;" + mname + "</label><span class='default_label'>&nbsp;(</span><span style='color:#ececec' class='default_label' onclick=defaultPermission('" + parent + "',0);>Set Defalt</span><span class='default_label'>)</span><span class='default_label' onclick=defaultPermission('" + parent + "',1);>&nbsp;(Unset)</span>";
                        Menu = Menu + "<ul>";
                        Main_Menu.Add(parent, mname);
                        ManuLine.Add(Menu);
                        Main_Menu_Head.Add(parent, ManuLine.Count - 1);
                        Child = menuid_Child;
                        ReParent = parent;
                        Permission_Details.Add(parent, menu_row.ItemArray[5] == null ? "" : menu_row.ItemArray[5].ToString() + "#" + menu_row.ItemArray[6].ToString() + "#" + menu_row.ItemArray[7].ToString() + "#" + menu_row.ItemArray[10].ToString());
                        ST1.Push(menuid_Child + "." + Reuse + "." + ReParent);
                        while (true)
                        {
                            IEnumerable<DataRow> results1 =
                            from row in MenuTble.AsEnumerable()
                            where row.Field<string>("MENU_PARENT_ID") == Child
                            orderby row.Field<string>("MENU_ORDER_CODE")
                            select row;
                            if (results1.Any() == true)
                            {
                                DataTable boundTable = results1.CopyToDataTable<DataRow>();
                                for (int x = Reuse; x <= boundTable.Rows.Count - 1; x++)
                                {
                                    string parent1 = boundTable.Rows[x][4].ToString();
                                    string menuid_Child1 = boundTable.Rows[x][0].ToString() + "!" + boundTable.Rows[x][2].ToString();
                                    string SubMenu = "<li>";
                                    SubMenu = SubMenu + "<input type='checkbox'  id='" + parent1 + "'/>" + "<label class='tree_label' for='" + parent1 + "'><img src='../Image/folder1.png' class='folderimg'/>&nbsp;" + boundTable.Rows[x][1].ToString() + "</label><span class='default_label'>&nbsp;(</span><span style='color:#ececec' class='default_label' onclick=defaultPermission('" + parent1 + "',0);>Set Defalt</span><span class='default_label'>)</span><span class='default_label' onclick=defaultPermission('" + parent + "',1);>&nbsp;(Unset)</span>";
                                    SubMenu = SubMenu + "<ul>";
                                    Main_Menu.Add(parent1, boundTable.Rows[x][1].ToString());
                                    Permission_Details.Add(parent1, boundTable.Rows[x][5] == null ? "" : boundTable.Rows[x][5].ToString() + "#" + boundTable.Rows[x][6].ToString() + "#" + boundTable.Rows[x][7].ToString() + "#" + boundTable.Rows[x][10].ToString());
                                    ManuLine.Add(SubMenu);
                                    Main_Menu_Head.Add(parent1, ManuLine.Count - 1);
                                    Child = menuid_Child1;
                                    ReParent = parent1;
                                    Reuse = 0;
                                    ST1.Push(menuid_Child1 + "." + Reuse + "." + ReParent);
                                    break;
                                }
                                if (Reuse > boundTable.Rows.Count - 1)
                                {
                                    if (ST1.Count > 0)
                                    {
                                        ST1.Pop();
                                        ManuLine.Add("</ul> </li>");
                                        if (ST1.Count == 0)
                                        {
                                            Reuse = 0;
                                            break;
                                        }
                                        string str = ST1.Pop().ToString();
                                        string[] getParent;
                                        getParent = str.Split('.');
                                        Child = getParent[0];
                                        ReParent = getParent[2];
                                        Reuse = Convert.ToInt32(getParent[1]);
                                        Reuse += 1;
                                        ST1.Push(Child + "." + Reuse + "." + ReParent);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (ST1.Count > 0)
                                {
                                    string Recall = ST1.Peek().ToString();
                                    string[] RecallDetails;
                                    RecallDetails = Recall.Split('.');
                                    int index = (int)Main_Menu_Head[RecallDetails[2]];
                                    string Manuname = (string)Main_Menu[RecallDetails[2]];

                                    string code = "Menu_" + CommVar.ModuleCode() + "_" + RecallDetails[2].ToString();
                                    string Controller = "Con_" + CommVar.ModuleCode() + "_" + RecallDetails[2].ToString();

                                    string ssv = Server.MapPath("");
                                    string[] M_INDEX = RecallDetails[0].ToString().Split('!');
                                    MenuRightByUser active = UR.MenuRightByUser.Where(s => s.MENU_PROGCALL == RecallDetails[2]).SingleOrDefault();
                                    OnlyMenu.Add(active);
                                    string title = "";
                                    if (active.Active)
                                    {
                                        title = (active.Add == true ? "Add=Yes, " : "Add=No, ") + (active.Edit == true ? "Edit=Yes, " : "Edit=No, ") + (active.Delete == true ? "Delete=Yes, " : "Delete=No, ") + (active.View == true ? "View=Yes, " : "View=No, ") + (active.Check == true ? "Check=Yes, " : "Check=No, ") + "Add Day=" + active.A_DAY.ToString() + ", " + "Edit Day=" + active.E_DAY.ToString() + ", " + "Delete Day=" + active.D_DAY.ToString();
                                    }
                                    string SubMenu = " <li><span class='tree_label'>";
                                    SubMenu = SubMenu + (active.Active == true ? "<img id='" + RecallDetails[2].ToString() + "' src='../Image/userpermission1.png' class='fileimg'/>&nbsp;" + "<span title='" + title + "' onclick=return&nbsp;setpermission('" + RecallDetails[2].ToString() + "'," + M_INDEX[1] + ");>" + Manuname + "</span>" : "<img id='" + RecallDetails[2].ToString() + "' src='../Image/userpermission.png' class='fileimg'/>&nbsp;" + "<span onclick=return&nbsp;setpermission('" + RecallDetails[2].ToString() + "'," + M_INDEX[1] + ");>" + Manuname + "</span>");
                                    SubMenu = SubMenu + "</span></li>";
                                    ManuLine.RemoveAt(index);
                                    ManuLine.Insert(index, SubMenu);
                                    ST1.Pop();
                                    if (ST1.Count == 0)
                                    {
                                        Reuse = 0;
                                        break;
                                    }
                                    string str = ST1.Pop().ToString();
                                    string[] getParent;
                                    getParent = str.Split('.');
                                    Child = getParent[0];
                                    ReParent = getParent[2];
                                    Reuse = Convert.ToInt32(getParent[1]);
                                    Reuse += 1;
                                    ST1.Push(Child + "." + Reuse + "." + ReParent);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                Session.Add("Permission", Permission_Details);
            }
            #endregion
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            if (ManuLine != null)
            {
                for (int i = 0; i <= ManuLine.Count - 1; i++)
                {
                    SB.Append(ManuLine[i].ToString());
                }
            }
            UR.ManuDetails = SB.ToString();
            var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            UR.serializeString = javaScriptSerializer.Serialize(UR.MenuRightByUser);
            UR.serializeStringChild = javaScriptSerializer.Serialize(OnlyMenu);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            var Comp_List = (from i in DB.M_USR_ACS
                             where i.USER_ID == user
                             select new URightByComp()
                             {
                                 Check = false,
                                 comcd = i.COMPCD,
                                 loccd = i.LOCCD,
                                 comnm = "",
                                 locnm = ""
                             }).Distinct().ToList();
            UR.Comp_List = URight.Comp_List;

            if (UR.Comp_List != null)
            {
                foreach (var i in UR.Comp_List)
                {
                    i.Check = false;
                }
                foreach (var i in Comp_List)
                {
                    UR.Comp_List.Where(x => x.comcd == i.comcd && x.loccd == i.loccd).ToList().ForEach(x => { x.Check = true; });
                }
            }
            ModelState.Clear();
            var utree = RenderRazorViewToString(this.ControllerContext, "_userrightTree", UR);
            var compermission = RenderRazorViewToString(this.ControllerContext, "_userrightcompanygrid", UR);
            var json = Json(new { utree, compermission });
            return json;
        }
        public static String RenderRazorViewToString(ControllerContext controllerContext, String viewName, Object model)
        {
            controllerContext.Controller.ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var ViewResult = ViewEngines.Engines.FindPartialView(controllerContext, viewName);
                var ViewContext = new ViewContext(controllerContext, ViewResult.View, controllerContext.Controller.ViewData, controllerContext.Controller.TempData, sw);
                ViewResult.View.Render(ViewContext, sw);
                ViewResult.ViewEngine.ReleaseView(controllerContext, ViewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
        public ActionResult Individual_UserRight(string serial, string id, string index)
        {
            UserRight UR = new UserRight();
            List<MenuRightByUser> MR = new List<MenuRightByUser>();
            var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            MR = javaScriptSerializer.Deserialize<List<MenuRightByUser>>(serial);
            UR.MenuRightByUser = MR;
            string progcall = id;
            MenuRightByUser active = UR.MenuRightByUser.Where(s => s.MENU_PROGCALL == progcall).SingleOrDefault();
            UR.index = UR.MenuRightByUser.IndexOf(active);
            return PartialView("_userRights", active);
        }
        public ActionResult UpdateRights(MenuRightByUser MR, string serialize)
        {
            List<MenuRightByUser> MR1 = new List<MenuRightByUser>();
            var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            MR1 = javaScriptSerializer.Deserialize<List<MenuRightByUser>>(serialize);
            MenuRightByUser active = MR1.Where(s => s.MENU_PROGCALL == MR.MENU_PROGCALL && s.ParentID == MR.ParentID && s.MENU_NAME == MR.MENU_NAME && s.MENU_INDEX == MR.MENU_INDEX).SingleOrDefault();
            int index = MR1.IndexOf(active);
            string flag = "";
            if (MR.MENU_type == "E" || MR.MENU_type == "M")
            {
                if (MR.Add == true || MR.Edit == true || MR.Delete == true || MR.Check == true || MR.View == true)
                {
                    flag = "userpermission1.png";
                }
                else
                {
                    flag = "userpermission.png";
                }
            }
            else if (MR.MENU_type == "" || MR.MENU_type == null || MR.MENU_type == "O")
            {
                if (MR.Active)
                {
                    flag = "userpermission1.png";
                }
                else
                {
                    flag = "userpermission.png";
                }
            }
            MR1[index] = MR;
            string newSerialize = javaScriptSerializer.Serialize(MR1);
            return Content(newSerialize + "^^^~~~~^^^" + flag);
        }
        public ActionResult Save(UserRight URight, string serialize, string user, string child)
        {
            try
            {
                var UNQSNO = Cn.getQueryStringUNQSNO();
                List<MenuRightByUser> MR1 = new List<MenuRightByUser>();
                List<MenuRightByUser> MR2 = new List<MenuRightByUser>();
                var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                MR1 = javaScriptSerializer.Deserialize<List<MenuRightByUser>>(serialize);
                MR2 = javaScriptSerializer.Deserialize<List<MenuRightByUser>>(child);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                string SCHEMA = Cn.Getschema;
                CS = Cn.GetConnectionString();
                Cn.con = new OracleConnection(CS);
                Hashtable checkParent = new Hashtable();
                if (Cn.con.State == ConnectionState.Closed)
                {
                    Cn.con.Open();
                }
                using (var transaction = DB.Database.BeginTransaction())
                {
                    foreach (var i in URight.Comp_List)
                    {
                        string comcode = i.comcd;
                        string LOCCODE = i.loccd;
                        var emdno = DB.M_USR_ACS.Where(x => x.USER_ID == user && x.COMPCD == comcode && x.LOCCD == LOCCODE).Max(s => s.EMD_NO);
                        if (emdno == null)
                        {
                            emdno = 0;
                        }
                        else
                        {
                            emdno = Convert.ToByte(emdno + 1);
                        }
                        if (i.Check)
                        {
                            DB.M_USR_ACS.RemoveRange(DB.M_USR_ACS.Where(x => x.USER_ID == user && x.COMPCD == i.comcd && x.LOCCD == i.loccd));                       
                            checkParent.Clear();
                            foreach (MenuRightByUser MRU in MR2)
                            {
                                MenuRightByUser MRU1 = (from h in MR1 where (h.MENU_PROGCALL == MRU.MENU_PROGCALL) select h).SingleOrDefault();
                                if (MRU1.MENU_type == "E" || MRU1.MENU_type == "M")
                                {
                                    if (MRU1.Add == true || MRU1.Edit == true || MRU1.Delete == true || MRU1.Check == true || MRU1.View == true)
                                    {
                                        string[] parent = MRU1.ParentID.Split('!');
                                        M_USR_ACS MUA = new M_USR_ACS();
                                        MUA.COMPCD = comcode;
                                        if (MRU1.MENU_date_option == "Y")
                                        {
                                            MUA.A_DAY = MRU1.A_DAY;
                                            MUA.D_DAY = MRU1.D_DAY;
                                            MUA.E_DAY = MRU1.E_DAY;
                                        }
                                        else
                                        {
                                            MUA.A_DAY = 0;
                                            MUA.D_DAY = 0;
                                            MUA.E_DAY = 0;
                                        }
                                        MUA.LOCCD = LOCCODE;
                                        MUA.MENU_INDEX = MRU1.MENU_INDEX;
                                        MUA.EMD_NO = emdno;
                                        MUA.MENU_NAME = MRU1.MENU_ID;
                                        MUA.CLCD = CommVar.ClientCode(UNQSNO);
                                        MUA.SCHEMA_NAME = CommVar.CurSchema(UNQSNO);
                                        MUA.USER_ID = user;
                                        MUA.USR_ID = Session["UR_ID"].ToString();
                                        string rights = "";
                                        int viewflag = 0;
                                        if (MRU1.Add == true)
                                        {
                                            rights = rights + "A";
                                        }
                                        if (MRU1.Edit == true)
                                        {
                                            rights = rights + "E";
                                            viewflag = 1;
                                        }
                                        if (MRU1.Delete == true)
                                        {
                                            rights = rights + "D";
                                            viewflag = 1;
                                        }
                                        if (MRU1.Check == true)
                                        {
                                            rights = rights + "C";
                                            viewflag = 1;
                                        }
                                        if (viewflag == 1 || MRU1.View == true)
                                        {
                                            rights = rights + "V";
                                        }
                                        MUA.USER_RIGHT = rights;
                                        MUA.USR_ENTDT = DateTime.Now;
                                        DB.M_USR_ACS.Add(MUA);
                                        Cn.com = new OracleCommand("select * from " + SCHEMA + ".appl_menu where menu_id='" + parent[0] + "' and menu_index=" + parent[1] + " and MODULE_CODE='" + CommVar.ModuleCode() + "'", Cn.con);
                                        OracleDataReader dr = Cn.com.ExecuteReader();
                                        dr.Read();
                                        if (dr["MENU_ID"].ToString().Trim() == dr["MENU_PARENT_ID"].ToString().Trim())
                                        {
                                            string progcall = dr["MENU_ID"].ToString() + "~" + dr["MENU_INDEX"].ToString() + "~" + (dr["MENU_PROGCALL"] == DBNull.Value ? "Notset" : dr["MENU_PROGCALL"].ToString());
                                            if (checkParent[progcall] == null)
                                            {
                                                MenuRightByUser MRU2 = (from h in MR1 where (h.MENU_PROGCALL == progcall) select h).SingleOrDefault();
                                                M_USR_ACS MUA1 = new M_USR_ACS();
                                                MUA1.COMPCD = comcode;
                                                MUA1.LOCCD = LOCCODE;
                                                MUA1.CLCD = CommVar.ClientCode(UNQSNO);
                                                MUA1.MENU_INDEX = MRU2.MENU_INDEX;
                                                MUA1.MENU_NAME = MRU2.MENU_ID;
                                                MUA1.EMD_NO = emdno;
                                                MUA1.SCHEMA_NAME = CommVar.CurSchema(UNQSNO);
                                                MUA1.USER_ID = user;
                                                MUA1.USR_ID = Session["UR_ID"].ToString();
                                                MUA1.USER_RIGHT = "";
                                                MUA1.USR_ENTDT = DateTime.Now;
                                                DB.M_USR_ACS.Add(MUA1);
                                                checkParent.Add(progcall, MRU2.MENU_NAME);
                                            }
                                            dr.Close();
                                        }
                                        else
                                        {
                                            string[] parendid = dr["MENU_PARENT_ID"].ToString().Split('!');
                                            string progcall = dr["MENU_ID"].ToString() + "~" + dr["MENU_INDEX"].ToString() + "~" + (dr["MENU_PROGCALL"] == DBNull.Value ? "Notset" : dr["MENU_PROGCALL"].ToString());
                                            if (checkParent[progcall] == null)
                                            {
                                                MenuRightByUser MRU2 = (from h in MR1 where (h.MENU_PROGCALL == progcall) select h).SingleOrDefault();
                                                M_USR_ACS MUA1 = new M_USR_ACS();
                                                MUA1.COMPCD = comcode;
                                                MUA1.LOCCD = LOCCODE;
                                                MUA1.CLCD = CommVar.ClientCode(UNQSNO);
                                                MUA1.EMD_NO = emdno;
                                                MUA1.MENU_INDEX = MRU2.MENU_INDEX;
                                                MUA1.MENU_NAME = MRU2.MENU_ID;
                                                MUA1.SCHEMA_NAME = CommVar.CurSchema(UNQSNO);
                                                MUA1.USER_ID = user;
                                                MUA1.USR_ID = Session["UR_ID"].ToString();
                                                MUA1.USER_RIGHT = "";
                                                MUA1.USR_ENTDT = DateTime.Now;
                                                DB.M_USR_ACS.Add(MUA1);
                                                checkParent.Add(progcall, MRU2.MENU_NAME);
                                            }
                                            dr.Close();
                                            while (true)
                                            {
                                                Cn.com = new OracleCommand("select * from " + SCHEMA + ".appl_menu where menu_id='" + parendid[0] + "' and menu_index=" + parendid[1] + " and MODULE_CODE='" + CommVar.ModuleCode() + "'", Cn.con);
                                                OracleDataReader dr1 = Cn.com.ExecuteReader();
                                                dr1.Read();
                                                if (dr1["MENU_ID"].ToString().Trim() == dr1["MENU_PARENT_ID"].ToString().Trim())
                                                {
                                                    string progcall1 = dr1["MENU_ID"].ToString() + "~" + dr1["MENU_INDEX"].ToString() + "~" + (dr1["MENU_PROGCALL"] == DBNull.Value ? "Notset" : dr1["MENU_PROGCALL"].ToString());
                                                    if (checkParent[progcall1] == null)
                                                    {
                                                        MenuRightByUser MRU3 = (from h in MR1 where (h.MENU_PROGCALL == progcall1) select h).SingleOrDefault();
                                                        M_USR_ACS MUA2 = new M_USR_ACS();
                                                        MUA2.COMPCD = comcode;
                                                        MUA2.LOCCD = LOCCODE;
                                                        MUA2.CLCD = CommVar.ClientCode(UNQSNO);
                                                        MUA2.EMD_NO = emdno;
                                                        MUA2.MENU_INDEX = MRU3.MENU_INDEX;
                                                        MUA2.MENU_NAME = MRU3.MENU_ID;
                                                        MUA2.SCHEMA_NAME = CommVar.CurSchema(UNQSNO);
                                                        MUA2.USER_ID = user;
                                                        MUA2.USR_ID = Session["UR_ID"].ToString();
                                                        MUA2.USER_RIGHT = "";
                                                        MUA2.USR_ENTDT = DateTime.Now;
                                                        DB.M_USR_ACS.Add(MUA2);
                                                        checkParent.Add(progcall1, MRU3.MENU_NAME);
                                                    }
                                                    dr1.Close();
                                                    break;
                                                }
                                                else
                                                {
                                                    string progcall1 = dr1["MENU_ID"].ToString() + "~" + dr1["MENU_INDEX"].ToString() + "~" + (dr1["MENU_PROGCALL"] == DBNull.Value ? "Notset" : dr1["MENU_PROGCALL"].ToString());
                                                    if (checkParent[progcall1] == null)
                                                    {
                                                        MenuRightByUser MRU3 = (from h in MR1 where (h.MENU_PROGCALL == progcall1) select h).SingleOrDefault();
                                                        M_USR_ACS MUA2 = new M_USR_ACS();
                                                        MUA2.COMPCD = comcode;
                                                        MUA2.LOCCD = LOCCODE;
                                                        MUA2.CLCD = CommVar.ClientCode(UNQSNO);
                                                        MUA2.EMD_NO = emdno;
                                                        MUA2.MENU_INDEX = MRU3.MENU_INDEX;
                                                        MUA2.MENU_NAME = MRU3.MENU_ID;
                                                        MUA2.SCHEMA_NAME = CommVar.CurSchema(UNQSNO);
                                                        MUA2.USER_ID = user;
                                                        MUA2.USR_ID = Session["UR_ID"].ToString();
                                                        MUA2.USER_RIGHT = "";
                                                        MUA2.USR_ENTDT = DateTime.Now;
                                                        DB.M_USR_ACS.Add(MUA2);
                                                        checkParent.Add(progcall1, MRU3.MENU_NAME);
                                                    }
                                                    parendid = dr1["MENU_PARENT_ID"].ToString().Split('!');
                                                    dr1.Close();
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (MRU1.MENU_type == "" || MRU1.MENU_type == null || MRU1.MENU_type == "O")
                                {
                                    if (MRU1.Active)
                                    {
                                        string[] parent = MRU1.ParentID.Split('!');
                                        M_USR_ACS MUA = new M_USR_ACS();
                                        MUA.COMPCD = comcode;
                                        if (MRU1.MENU_date_option == "Y")
                                        {
                                            MUA.A_DAY = MRU1.A_DAY;
                                            MUA.D_DAY = MRU1.D_DAY;
                                            MUA.E_DAY = MRU1.E_DAY;
                                        }
                                        else
                                        {
                                            MUA.A_DAY = 0;
                                            MUA.D_DAY = 0;
                                            MUA.E_DAY = 0;
                                        }
                                        MUA.LOCCD = LOCCODE;
                                        MUA.CLCD = CommVar.ClientCode(UNQSNO);
                                        MUA.MENU_INDEX = MRU1.MENU_INDEX;
                                        MUA.EMD_NO = emdno;
                                        MUA.MENU_NAME = MRU1.MENU_ID;
                                        MUA.SCHEMA_NAME = CommVar.CurSchema(UNQSNO);
                                        MUA.USER_ID = user;
                                        MUA.USR_ID = Session["UR_ID"].ToString();
                                        MUA.USER_RIGHT = "";
                                        MUA.USR_ENTDT = DateTime.Now;
                                        DB.M_USR_ACS.Add(MUA);
                                        if (parent.Count() > 1)
                                        {
                                            Cn.com = new OracleCommand("select * from " + SCHEMA + ".appl_menu where menu_id='" + parent[0] + "' and menu_index=" + parent[1] + " and MODULE_CODE='" + CommVar.ModuleCode() + "'", Cn.con);
                                        }
                                        else
                                        {
                                            Cn.com = new OracleCommand("select * from " + SCHEMA + ".appl_menu where menu_id='" + parent[0] + "' and menu_index='' and MODULE_CODE='" + CommVar.ModuleCode() + "'", Cn.con);
                                        }
                                        OracleDataReader dr = Cn.com.ExecuteReader();
                                        dr.Read();
                                        if (dr.HasRows)
                                        {
                                            if (dr["MENU_ID"].ToString().Trim() == dr["MENU_PARENT_ID"].ToString().Trim())
                                            {
                                                string progcall = dr["MENU_ID"].ToString() + "~" + dr["MENU_INDEX"].ToString() + "~" + (dr["MENU_PROGCALL"] == DBNull.Value ? "Notset" : dr["MENU_PROGCALL"].ToString());
                                                if (checkParent[progcall] == null)
                                                {
                                                    MenuRightByUser MRU2 = (from h in MR1 where (h.MENU_PROGCALL == progcall) select h).SingleOrDefault();
                                                    M_USR_ACS MUA1 = new M_USR_ACS();
                                                    MUA1.COMPCD = comcode;
                                                    MUA1.LOCCD = LOCCODE;
                                                    MUA1.CLCD = CommVar.ClientCode(UNQSNO);
                                                    MUA1.EMD_NO = emdno;
                                                    MUA1.MENU_INDEX = MRU2.MENU_INDEX;
                                                    MUA1.MENU_NAME = MRU2.MENU_ID;
                                                    MUA1.SCHEMA_NAME = CommVar.CurSchema(UNQSNO);
                                                    MUA1.USER_ID = user;
                                                    MUA1.USR_ID = Session["UR_ID"].ToString();
                                                    MUA1.USER_RIGHT = "";
                                                    MUA1.USR_ENTDT = DateTime.Now;
                                                    DB.M_USR_ACS.Add(MUA1);
                                                    checkParent.Add(progcall, MRU2.MENU_NAME);
                                                }
                                                dr.Close();
                                            }
                                            else
                                            {
                                                string[] parendid = dr["MENU_PARENT_ID"].ToString().Split('!');
                                                string progcall = dr["MENU_ID"].ToString() + "~" + dr["MENU_INDEX"].ToString() + "~" + (dr["MENU_PROGCALL"] == DBNull.Value ? "Notset" : dr["MENU_PROGCALL"].ToString());
                                                if (checkParent[progcall] == null)
                                                {
                                                    MenuRightByUser MRU2 = (from h in MR1 where (h.MENU_PROGCALL == progcall) select h).SingleOrDefault();
                                                    M_USR_ACS MUA1 = new M_USR_ACS();
                                                    MUA1.COMPCD = comcode;
                                                    MUA1.LOCCD = LOCCODE;
                                                    MUA1.CLCD = CommVar.ClientCode(UNQSNO);
                                                    MUA1.EMD_NO = emdno;
                                                    MUA1.MENU_INDEX = MRU2.MENU_INDEX;
                                                    MUA1.MENU_NAME = MRU2.MENU_ID;
                                                    MUA1.SCHEMA_NAME = CommVar.CurSchema(UNQSNO);
                                                    MUA1.USER_ID = user;
                                                    MUA1.USR_ID = Session["UR_ID"].ToString();
                                                    MUA1.USER_RIGHT = "";
                                                    MUA1.USR_ENTDT = DateTime.Now;
                                                    DB.M_USR_ACS.Add(MUA1);
                                                    checkParent.Add(progcall, MRU2.MENU_NAME);
                                                }
                                                dr.Close();
                                                while (true)
                                                {
                                                    Cn.com = new OracleCommand("select * from " + SCHEMA + ".appl_menu where menu_id='" + parendid[0] + "' and menu_index=" + parendid[1] + " and MODULE_CODE='" + CommVar.ModuleCode() + "'", Cn.con);
                                                    OracleDataReader dr1 = Cn.com.ExecuteReader();
                                                    dr1.Read();
                                                    if (dr1["MENU_ID"].ToString().Trim() == dr1["MENU_PARENT_ID"].ToString().Trim())
                                                    {
                                                        string progcall1 = dr1["MENU_ID"].ToString() + "~" + dr1["MENU_INDEX"].ToString() + "~" + (dr1["MENU_PROGCALL"] == DBNull.Value ? "Notset" : dr1["MENU_PROGCALL"].ToString());
                                                        if (checkParent[progcall1] == null)
                                                        {
                                                            MenuRightByUser MRU3 = (from h in MR1 where (h.MENU_PROGCALL == progcall1) select h).SingleOrDefault();
                                                            M_USR_ACS MUA2 = new M_USR_ACS();
                                                            MUA2.COMPCD = comcode;
                                                            MUA2.LOCCD = LOCCODE;
                                                            MUA2.CLCD = CommVar.ClientCode(UNQSNO);
                                                            MUA2.EMD_NO = emdno;
                                                            MUA2.MENU_INDEX = MRU3.MENU_INDEX;
                                                            MUA2.MENU_NAME = MRU3.MENU_ID;
                                                            MUA2.SCHEMA_NAME = CommVar.CurSchema(UNQSNO);
                                                            MUA2.USER_ID = user;
                                                            MUA2.USR_ID = Session["UR_ID"].ToString();
                                                            MUA2.USER_RIGHT = "";
                                                            MUA2.USR_ENTDT = DateTime.Now;
                                                            DB.M_USR_ACS.Add(MUA2);
                                                            checkParent.Add(progcall1, MRU3.MENU_NAME);
                                                        }
                                                        dr1.Close();
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        string progcall1 = dr1["MENU_ID"].ToString() + "~" + dr1["MENU_INDEX"].ToString() + "~" + (dr1["MENU_PROGCALL"] == DBNull.Value ? "Notset" : dr1["MENU_PROGCALL"].ToString());
                                                        if (checkParent[progcall1] == null)
                                                        {
                                                            MenuRightByUser MRU3 = (from h in MR1 where (h.MENU_PROGCALL == progcall1) select h).SingleOrDefault();
                                                            M_USR_ACS MUA2 = new M_USR_ACS();
                                                            MUA2.COMPCD = comcode;
                                                            MUA2.LOCCD = LOCCODE;
                                                            MUA2.CLCD = CommVar.ClientCode(UNQSNO);
                                                            MUA2.EMD_NO = emdno;
                                                            MUA2.MENU_INDEX = MRU3.MENU_INDEX;
                                                            MUA2.MENU_NAME = MRU3.MENU_ID;
                                                            MUA2.SCHEMA_NAME = CommVar.CurSchema(UNQSNO);
                                                            MUA2.USER_ID = user;
                                                            MUA2.USR_ID = Session["UR_ID"].ToString();
                                                            MUA2.USER_RIGHT = "";
                                                            MUA2.USR_ENTDT = DateTime.Now;
                                                            DB.M_USR_ACS.Add(MUA2);
                                                            checkParent.Add(progcall1, MRU3.MENU_NAME);
                                                        }
                                                        parendid = dr1["MENU_PARENT_ID"].ToString().Split('!');
                                                        dr1.Close();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    Cn.con.Close();
                    DB.SaveChanges();
                    transaction.Commit();
                    ImprovarDB DBimp = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    using (var imptransaction = DBimp.Database.BeginTransaction())
                    {
                        try
                        {
                            var clcd = CommVar.ClientCode(UNQSNO);
                            DBimp.MS_MUSRACS.RemoveRange(DBimp.MS_MUSRACS.Where(x => x.USER_ID == user && x.CLCD == clcd&&x.SCHEMA_NAME== DB.CacheKey));
                            DBimp.SaveChanges();
                            string sql = " select distinct clcd, user_id, compcd, loccd, schema_name  from " + CommVar.CurSchema(UNQSNO) + ".m_usr_acs where USER_ID = '" + user + "' union ";
                            sql += " select distinct a.clcd, a.user_id, b.compcd, b.loccd, b.schema_name ";
                            sql += " from " + CommVar.CurSchema(UNQSNO) + ".m_usr_acs_grpdtl a, " + CommVar.CurSchema(UNQSNO) + ".m_usr_acs b where a.linkuser_id = b.user_id AND B.USER_ID = '" + user + "' ";
                            var tbl = masterHelp.SQLquery(sql);
                            foreach (DataRow dr in tbl.Rows)
                            {
                                MS_MUSRACS MUSRACS = new MS_MUSRACS();
                                MUSRACS.CLCD = CommVar.ClientCode(UNQSNO);
                                MUSRACS.COMPCD = dr["compcd"].retStr();
                                MUSRACS.LOCCD = dr["loccd"].retStr();
                                MUSRACS.USER_ID = dr["user_id"].retStr();
                                MUSRACS.SCHEMA_NAME = dr["schema_name"].retStr();
                                MUSRACS.MODULE_CODE = CommVar.ModuleCode();
                                DBimp.MS_MUSRACS.Add(MUSRACS);
                            }
                            DBimp.SaveChanges();
                            imptransaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            Cn.SaveException(ex, "");
                            imptransaction.Rollback();
                        }
                    }
                }
                return Content("1");
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + " " + ex.InnerException);
            }
        }
        public ActionResult Individual_UserRight_auto(string serial, string id, string index)
        {
            List<MenuRightByUser> MR = new List<MenuRightByUser>();
            var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            MR = javaScriptSerializer.Deserialize<List<MenuRightByUser>>(serial);
            string progcall = id;
            MenuRightByUser active = MR.Where(s => s.MENU_PROGCALL == progcall).SingleOrDefault();
            int index1 = MR.IndexOf(active);
            if (active.MENU_type == "E" || active.MENU_type == "M")
            {
                active.Add = true;
                active.Edit = true;
                active.Delete = true;
                active.View = true;
                if (active.MENU_type != "M")
                {
                    active.Check = true;
                }
                active.A_DAY = 0;
                active.D_DAY = 0;
                active.E_DAY = 0;
            }
            else if (active.MENU_type == "" || active.MENU_type == null || active.MENU_type == "O")
            {
                active.Active = true;
            }
            MR[index1] = active;
            string newSerialize = javaScriptSerializer.Serialize(MR);
            return Content(newSerialize);
        }
        public ActionResult DefaultPermission(string progcalid, string serial, string Flag)
        {
            string image_name = "";
            Hashtable Main_Menu;
            Hashtable Main_Menu_Head;
            ArrayList ManuLine = null;
            string Child = null;
            string ReParent = null;
            int Reuse = 0;
            Stack ST1 = new Stack();
            Main_Menu = new Hashtable();
            Main_Menu_Head = new Hashtable();
            ManuLine = new ArrayList();
            List<MenuRightByUser> MR = new List<MenuRightByUser>();
            var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            MR = javaScriptSerializer.Deserialize<List<MenuRightByUser>>(serial);
            var getdata = MR.Where(s => s.MENU_PROGCALL == progcalid).SingleOrDefault();
            var data = MR.Where(s => s.ParentID == getdata.MENU_ID + "!" + getdata.MENU_INDEX).ToList();
            foreach (var menu_row in data)
            {
                string parent = menu_row.MENU_PROGCALL;
                string menuid_Child = menu_row.MENU_ID + "!" + menu_row.MENU_INDEX;
                string mname = menu_row.MENU_NAME;
                Main_Menu.Add(parent, mname);
                ManuLine.Add("1");
                Main_Menu_Head.Add(parent, ManuLine.Count - 1);
                Child = menuid_Child;
                ReParent = parent;
                ST1.Push(menuid_Child + "." + Reuse + "." + ReParent);
                while (true)
                {
                    var data1 = MR.Where(s => s.ParentID == Child).ToList();
                    if (data1.Any() == true)
                    {
                        for (int x = Reuse; x <= data1.Count - 1; x++)
                        {
                            string parent1 = data1[x].MENU_PROGCALL;
                            string menuid_Child1 = data1[x].MENU_ID + "!" + data1[x].MENU_INDEX;
                            Main_Menu.Add(parent1, data1[x].MENU_NAME);
                            ManuLine.Add("");
                            Main_Menu_Head.Add(parent1, ManuLine.Count - 1);
                            Child = menuid_Child1;
                            ReParent = parent1;
                            Reuse = 0;
                            ST1.Push(menuid_Child1 + "." + Reuse + "." + ReParent);
                            break;
                        }
                        if (Reuse > data1.Count - 1)
                        {
                            if (ST1.Count > 0)
                            {
                                ST1.Pop();
                                if (ST1.Count == 0)
                                {
                                    Reuse = 0;
                                    break;
                                }
                                string str = ST1.Pop().ToString();
                                string[] getParent;
                                getParent = str.Split('.');
                                Child = getParent[0];
                                ReParent = getParent[2];
                                Reuse = Convert.ToInt32(getParent[1]);
                                Reuse += 1;
                                ST1.Push(Child + "." + Reuse + "." + ReParent);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (ST1.Count > 0)
                        {
                            string Recall = ST1.Peek().ToString();
                            string[] RecallDetails;
                            RecallDetails = Recall.Split('.');
                            image_name = image_name + RecallDetails[2] + "*";
                            MenuRightByUser active = MR.Where(s => s.MENU_PROGCALL == RecallDetails[2]).SingleOrDefault();
                            int index1 = MR.IndexOf(active);
                            if (active.MENU_type == "E" || active.MENU_type == "M")
                            {
                                if (Flag == "0")
                                {
                                    active.Add = true;
                                    active.Edit = true;
                                    active.Delete = true;
                                    active.View = true;
                                    if (active.MENU_type != "M")
                                    {
                                        active.Check = true;
                                    }
                                    active.A_DAY = 0;
                                    active.D_DAY = 0;
                                    active.E_DAY = 0;
                                }
                                else
                                {
                                    active.Add = false;
                                    active.Edit = false;
                                    active.Delete = false;
                                    active.View = false;
                                    if (active.MENU_type != "M")
                                    {
                                        active.Check = false;
                                    }
                                    active.A_DAY = 0;
                                    active.D_DAY = 0;
                                    active.E_DAY = 0;
                                }
                            }
                            else if (active.MENU_type == "" || active.MENU_type == null || active.MENU_type == "O")
                            {
                                if (Flag == "0")
                                {
                                    active.Active = true;
                                }
                                else
                                {
                                    active.Active = false;
                                }
                            }
                            MR[index1] = active;
                            ST1.Pop();
                            if (ST1.Count == 0)
                            {
                                Reuse = 0;
                                break;
                            }
                            string str = ST1.Pop().ToString();
                            string[] getParent;
                            getParent = str.Split('.');
                            Child = getParent[0];
                            ReParent = getParent[2];
                            Reuse = Convert.ToInt32(getParent[1]);
                            Reuse += 1;
                            ST1.Push(Child + "." + Reuse + "." + ReParent);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            string Serialize = javaScriptSerializer.Serialize(MR);
            image_name = image_name.Substring(0, image_name.Length - 1);
            return Content(Serialize + "^^^^^^^+++++++^^^^^^" + image_name);
        }
    }
}