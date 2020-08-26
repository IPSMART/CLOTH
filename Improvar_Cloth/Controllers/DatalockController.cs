using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Collections;

namespace Improvar.Controllers
{
    public class DatalockController : Controller
    {
        // GET: Datalock
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        string MODULE = Module.Module_Code;
        public ActionResult datalock()
        {
            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                try
                {
                    dataLock dtl = new ViewModels.dataLock();
                    dtl.Defaultview = true;
                    string UNQSNO = Cn.getQueryStringUNQSNO();
                    dtl.UNQSNO = UNQSNO;
                    return View(dtl);
                }
                catch (Exception ex)
                {
                    dataLock dtl = new ViewModels.dataLock();
                    dtl.Defaultview = false;
                    ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                    Cn.SaveException(ex, "");
                    return View(dtl);
                }
            }
        }
        public ActionResult TreeMenuGen()
        {
            string GLOBAL_MENU_NAME = "";
            try
            {
                System.Text.StringBuilder SB1 = new System.Text.StringBuilder();
                string uid = Session["UR_ID"].ToString();
                string COM = Session["CompanyCode" + UNQSNO].ToString();
                string LOC = Session["CompanyLocationCode" + UNQSNO].ToString();
                string scm1 = Session["DatabaseSchemaName" + UNQSNO].ToString();
                string MODULE_CODE = Session["ModuleCode"].ToString();
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                List<DateLockRight> DLR = new List<DateLockRight>();
                var ddl = DB.M_LOCKDATA.Where(a => a.COMPCD == COM && a.LOCCD == LOC).ToList();
                foreach (var i in ddl)
                {
                    DateLockRight DDLR = new DateLockRight();
                    DDLR.Lockdate = i.LOCKDT.Value.ToString("dd/MM/yyyy");
                    DDLR.Menu_ID = i.MENU_ID;
                    DDLR.Menu_index = i.MENU_INDEX;
                    DDLR.BackDate = i.BACKDATE == "Y" ? true : false;
                    DDLR.M_AUTONO = i.M_AUTONO;
                    DLR.Add(DDLR);
                }
                dataLock Mnu = new dataLock();
                #region Menugenerate                 
                string sql = "";

                //sql = "Select m.MENU_ID,m.MENU_NAME,m.MENU_INDEX,m.MENU_PARENT_ID,m.menu_find_id,"
                //    + " m.MENU_ID||'~'||m.MENU_INDEX||'~'||nvl(m.MENU_PROGCALL,'Notset')||'~'||m.menu_date_option||'~'||m.menu_type||'~'||p.E_DAY||'~'||p.D_DAY  as MENU_PERMISSIONID, ";
                //sql += fld1;
                //sql += "p.E_DAY,p.A_DAY,'Y' AS PERDOTNETMENU,'Y' AS ATVDOTNETMENU,p.D_DAY,m.MENU_ORDER_CODE,m.MENU_DOCCD,m.MENU_PARA  menu_para, ";
                //sql += "(select case  when exists(select 1 from appl_menu_fav f where  m.menu_id=f.menu_id and m.menu_index=f.menu_index and m.module_code='"
                //    + CommVar.ModuleCode() + "' and user_id='" + CommVar.UserID() + "') then 'Y'  else 'N' end from dual) as menu_fav ";
                //sql += " from " + CommVar.CurSchema(unqsno) + ".M_USR_ACS p , APPL_MENU m ";
                //sql += " where p.MENU_NAME=m.MENU_ID and p.MENU_INDEX=m.MENU_INDEX and p.USER_ID='" + CommVar.UserID() + "' and m.module_code='" + CommVar.ModuleCode() + "' and ";
                //sql += "p.compcd = '" + CommVar.Compcd(unqsno) + "' and p.loccd = '" + CommVar.Loccd(unqsno) + "' and p.schema_name = '" + CommVar.CurSchema(unqsno) + "' " + fld2;
                //sql += "union ";
                //sql += "Select m.MENU_ID,m.MENU_NAME,m.MENU_INDEX,m.MENU_PARENT_ID,m.menu_find_id,"
                //    + " m.MENU_ID||'~'||m.MENU_INDEX||'~'||nvl(m.MENU_PROGCALL,'Notset')||'~'||m.menu_date_option||'~'||m.menu_type||'~'||p.E_DAY||'~'||p.D_DAY as MENU_PERMISSIONID, ";
                //sql += fld1;
                //sql += "p.E_DAY,p.A_DAY,'Y' AS PERDOTNETMENU,'Y' AS ATVDOTNETMENU,p.D_DAY,m.MENU_ORDER_CODE,m.MENU_DOCCD,m.MENU_PARA , ";
                //sql += "(select case  when exists(select 1 from appl_menu_fav f where  m.menu_id=f.menu_id and m.menu_index=f.menu_index and m.module_code='"
                //    + CommVar.ModuleCode() + "' and user_id='" + CommVar.UserID() + "') then 'Y'  else 'N' end from dual) as menu_fav ";
                //sql += " from " + CommVar.CurSchema(unqsno) + ".m_usr_acs_grpdtl q, " + CommVar.CurSchema(unqsno) + ".M_USR_ACS p ,APPL_MENU m ";
                //sql += " where p.MENU_NAME=m.MENU_ID and p.MENU_INDEX=m.MENU_INDEX and q.USER_ID='" + CommVar.UserID() + "' and m.module_code='" + CommVar.ModuleCode() + "' and ";
                //sql += " p.compcd = '" + CommVar.Compcd(unqsno) + "' and p.loccd = '" + CommVar.Loccd(unqsno) + "' and p.schema_name='" + CommVar.CurSchema(unqsno) + "' and q.linkuser_id=p.user_id and ";
                //sql += " m.menu_id||m.menu_index not in (select menu_name||menu_index from " + CommVar.CurSchema(unqsno) + ".m_usr_acs where user_id='" + CommVar.UserID() + "') " + fld2;
                //sql += " order by MENU_ORDER_CODE,MENU_PARENT_ID";

                sql = "";
                sql += "Select m.MENU_ID,m.MENU_NAME,m.MENU_INDEX,m.MENU_PARENT_ID, m.MENU_ID||'~'||m.MENU_INDEX||'~'||nvl(m.MENU_PROGCALL,'Notset') as MENU_PROGCALL,m.MENU_ORDER_CODE,m.menu_type ";
                sql += "from APPL_MENU m ";
                sql += "where m.module_code='" + CommVar.ModuleCode() + "' order by m.menu_order_code,m.MENU_PARENT_ID";

                DataTable menuTable = masterHelp.SQLquery(sql);

                string IDENTIFIER = Session["MotherMenuIdentifier"].ToString();
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                Stack ulStack = new Stack();
                foreach (DataRow maindr in menuTable.Rows)
                {
                    string MENU_ID = maindr["MENU_ID"].retStr();
                    string MENU_NAME = maindr["MENU_NAME"].retStr();
                    string MENU_INDEX = maindr["MENU_INDEX"].retStr();
                    string parentId = maindr["MENU_PARENT_ID"].retStr();
                    //string menu_find_id = maindr["menu_find_id"].retStr() == "" ? "" : " -" + maindr["menu_find_id"].retStr();
                    //string MENU_DETAIL = maindr["MENU_PERMISSIONID"].retStr();
                    //string MENU_PROGCALL = MENU_DETAIL.Split('~')[2];
                    //string MENU_DOCCD = maindr["MENU_DOCCD"].retStr();
                    //string MENU_PARA = maindr["menu_para"].retStr();
                    //string mnamefav = maindr["menu_fav"].retStr();
                    string menu_type = maindr["menu_type"].retStr();
                    string Uidindex = "";
                    if (parentId == IDENTIFIER)
                    {
                        Uidindex = IDENTIFIER;
                    }
                    else
                    {
                        Uidindex = MENU_ID + "!" + MENU_INDEX;
                    }
                    string Menu = "";
                    if (hasChildMenutbl(menuTable, Uidindex))
                    {
                        while (ulStack.Count > 0)
                        {
                            var tu = (ulStack.Contains(parentId));
                            if (parentId == IDENTIFIER && ulStack.Count > 0)
                            {
                                ulStack.Pop(); Menu += "</li></ul>";
                            }
                            else if (parentId == ulStack.Peek().retStr())
                            {
                                ulStack.Pop(); Menu += "</ul>";
                            }
                            else if (ulStack.Contains(parentId))
                            {
                                ulStack.Pop(); Menu += "</li></ul>";
                            }
                            else
                            {
                                break;
                            }
                        }
                        //Menu += "<li>";
                        //Menu += "<input type='checkbox'  id='" + MENU_DETAIL + "'/>" + "<label class='tree_label' for='" + MENU_DETAIL
                        //    + "'><img src='../Image/folder.png' class='folderimg'/>&nbsp;" + MENU_NAME + "</label>";
                        //Menu += "<ul>";

                        Menu += "<li>";
                        Menu += "<input type='checkbox' checked='checked'  id='" + Uidindex + "'/>" + "<label class='tree_label' for='"
                            + Uidindex + "'><img src='../Image/folder.png' class='folderimg'/>&nbsp;" + MENU_NAME +
                            "</label><span class='default_label'>&nbsp;(</span><span style='color:#ececec' class='default_label' onclick=defaultdatewindow('"
                            + MENU_ID + "!" + MENU_INDEX +
                            "');>Set Defalt</span><span class='default_label'>)</span><span class='default_label' onclick=defaultPermission('" + MENU_ID + "!" + MENU_INDEX + "',1);>&nbsp;(Reset)</span>";
                        Menu += "<ul>";

                        if (ulStack.Count == 0 || parentId != ulStack.Peek().retStr())
                        {
                            ulStack.Push(parentId);
                        }
                    }
                    else
                    {
                        if (MENU_NAME == "Product (Misc Bill)")
                        {

                        }
                        while (ulStack.Count > 0)
                        {
                            if (parentId == IDENTIFIER && ulStack.Count > 0)
                            {
                                ulStack.Pop(); Menu += "</li></ul>";
                            }
                            else if (parentId == ulStack.Peek().retStr())
                            {
                                ulStack.Pop(); Menu += "</ul>";
                            }
                            else if (parentId == ulStack.Peek().retStr())
                            {
                                ulStack.Pop(); Menu += "</li></ul>";
                            }
                            else
                            {
                                break;
                            }
                        }


                        string image = "";
                        string displayDT = "";
                        if (DLR.Any(a => a.Menu_ID == MENU_ID && a.Menu_index == Convert.ToInt32(MENU_INDEX)))
                        {
                            image = "&nbsp;<img src='../Image/greenlight.png' class='rightimg' id='" + MENU_ID + MENU_INDEX + "'/>";
                            displayDT = "(" + DLR.Where(a => a.Menu_ID == MENU_ID && a.Menu_index == Convert.ToInt32(MENU_INDEX)).Select(a => a.Lockdate).SingleOrDefault() + ")";
                        }
                        else
                        {
                            image = "&nbsp;<img src='../Image/redlight.png' class='rightimg' id='" + MENU_ID + MENU_INDEX + "'/>";
                        }
                        var tmpMenu = " <li><span class='tree_label'>" + "<img src='../Image/file1.png' class='fileimg'/>&nbsp;" + "<span title='" + MENU_NAME
                             + "' onclick=return&nbsp;setpermission('" + MENU_ID + "'," + MENU_INDEX + ");>" + MENU_NAME + image
                             + "</span>&nbsp;<span style='color:#ececec' class='default_label_date'>" + displayDT + "</span>" + "</span></li>";

                        //Menu += " <li><span class='tree_label'>";
                        //Menu += " <img src='../Image/file1.png' class='fileimg'/>&nbsp;" + "<a onclick =return&nbsp;winopen('" + MENU_PROGCALL + "','" + enc_MENU_DETAIL + "','" + enc_MENU_DOCCD + "','" + enc_MENU_PARA + "');>" + MENU_NAME + menu_find_id + "</a>&nbsp;";

                        //    Menu += "<img src = '../Image/favrit1.png' class='fileimg'  onclick =return&nbsp;addFavorite('" + MENU_ID + "','" + MENU_INDEX + "'); />";
                        //string Omenu = " <li><span class='tree_label'>" + "<img src='../Image/file1.png' class='fileimg'/>&nbsp;" + "<span title='" + MENU_NAME + "' onclick=return&nbsp;setpermission('" + MENU_ID + "'," + MENU_INDEX+ ");>" + MENU_NAME + image + "</span>&nbsp;<span style='color:#ececec' class='default_label_date'>" + displayDT + "</span>" + "</span></li>";

                        //Menu += " </span></li>";

                        if (menu_type == "E")
                        {
                            Menu += tmpMenu;
                            //continue;//skip this
                        }
                    }
                    SB.Append(Menu);
                }
                while (ulStack.Count > 0)
                {
                    ulStack.Pop();
                    SB.Append("</ul>");
                }
                Mnu.ManuDetails = SB.ToString();
                SB.Clear();
                var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                Mnu.serializeString = javaScriptSerializer.Serialize(DLR);
                return PartialView("_datalockTree", Mnu);

                #endregion
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                //GLOBAL_MENU_NAME = "MENU NAME (" + GLOBAL_MENU_NAME + ") Need to Change the menu type in the database APPL_MENU TABLE. [Contact with IPSMART Team]. ";
                string INNEREX = "";
                if (ex.InnerException != null) { INNEREX = ex.InnerException.Message; }
                return Content(ex.Message + "<BR/>" + INNEREX + "<BR/>" + GLOBAL_MENU_NAME);
            }
        }
        private bool hasChildMenutbl(DataTable dt, string MENU_ID)
        {
            if (dt.Select("MENU_PARENT_ID='" + MENU_ID + "'").Length > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public ActionResult Individual_Right(string serial, string id, int index)
        {
            try
            {
                dataLock UR = new dataLock();
                List<DateLockRight> MR = new List<DateLockRight>();
                var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                MR = javaScriptSerializer.Deserialize<List<DateLockRight>>(serial);
                UR.DateLockRight = MR;
                string progcall = id;
                DateLockRight active = UR.DateLockRight.Where(s => s.Menu_ID == progcall && s.Menu_index == index).SingleOrDefault();
                if (active == null)
                {
                    active = new DateLockRight();
                    active.Menu_ID = id;
                    active.Menu_index = index;
                    active.BackDate = false;
                }
                UR.index = UR.DateLockRight.IndexOf(active);
                return PartialView("_datelockRights", active);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return null;
            }
        }
        public ActionResult UpdateRights(DateLockRight MR, string serialize)
        {
            List<DateLockRight> MR1 = new List<DateLockRight>();
            var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            MR1 = javaScriptSerializer.Deserialize<List<DateLockRight>>(serialize);
            DateLockRight active = MR1.Where(s => s.Menu_ID == MR.Menu_ID && s.Menu_index == MR.Menu_index).SingleOrDefault();
            if (active == null)
            {
                active = MR;
                MR1.Add(MR);
            }
            else
            {
                int index = MR1.IndexOf(active);
                MR1[index] = MR;
            }
            string newSerialize = javaScriptSerializer.Serialize(MR1);
            return Content(newSerialize);
        }

        public ActionResult Save(string serialize)
        {
            try
            {
                List<DateLockRight> MR1 = new List<DateLockRight>();
                string comcode =CommVar.Compcd(UNQSNO);
                string LOCCODE = CommVar.Loccd(UNQSNO);
                var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                MR1 = javaScriptSerializer.Deserialize<List<DateLockRight>>(serialize);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));       
                Hashtable checkParent = new Hashtable();
                long Autonumber = 0;
                string Action = "";               
                using (var transaction = DB.Database.BeginTransaction())
                {
                    if (DB.M_LOCKDATA.Any(x => x.COMPCD == comcode && x.LOCCD == LOCCODE))
                    {
                        Autonumber = DB.M_LOCKDATA.Where(x => x.COMPCD == comcode && x.LOCCD == LOCCODE).Max(x => x.M_AUTONO);
                        Action = "E";
                    }
                    else
                    {
                        Autonumber = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO));
                        Action = "A";
                    }
                    DB.M_LOCKDATA.RemoveRange(DB.M_LOCKDATA.Where(x => x.COMPCD == comcode && x.LOCCD == LOCCODE));
                    M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(false, "M_LOCKDATA", Autonumber, Action, CommVar.CurSchema(UNQSNO));
                    foreach (DateLockRight MRU in MR1)
                    {
                        M_LOCKDATA ML = new M_LOCKDATA();
                        if (MRU.Lockdate != null)
                        {
                            ML.CLCD = CommVar.ClientCode(UNQSNO);
                            ML.COMPCD = comcode;
                            ML.LOCCD = LOCCODE;
                            ML.LOCKDT = DateTime.ParseExact(MRU.Lockdate, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                            ML.MENU_ID = MRU.Menu_ID;
                            ML.MENU_INDEX = Convert.ToByte(MRU.Menu_index);
                            ML.BACKDATE = MRU.BackDate == true ? "Y" : "N";
                            ML.SCHEMA_NAME = CommVar.CurSchema(UNQSNO);
                            ML.M_AUTONO = Autonumber;
                            DB.M_LOCKDATA.Add(ML);
                        }
                    }
                    if (Action == "A")
                    {
                        DB.M_CNTRL_HDR.Add(MCH);
                    }
                    else
                    {
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                    }
                    DB.SaveChanges();
                    transaction.Commit();
                }
                return Content("1");
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + " " + ex.InnerException);
            }
        }

        public ActionResult DefaultPermission(string progcalid, string serial, string Flag, string DDT, string CHKALW)
        {
            string UNQSNO = Cn.getQueryStringUNQSNO();
            ArrayList ManuLine = null;
            DataTable MenuTble;
            dataLock Mnu = new dataLock();
            bool ALWBAKDT = CHKALW == null ? false : true;
            Hashtable checkParent = new Hashtable();
            #region Menugenerate
            string SCHEMA = Cn.Getschema;
            CS = Cn.GetConnectionString();
            Cn.con = new OracleConnection(CS);
            if ((Cn.ds.Tables["fill_mainmenu"] == null) == false)
            {
                Cn.ds.Tables["fill_mainmenu"].Clear();
            }
            if (Cn.con.State == ConnectionState.Closed)
            {
                Cn.con.Open();
            }
            Cn.com = new OracleCommand(SCHEMA + ".MENU_PERMI_USR", Cn.con);//checking for menu permission
            Cn.com.CommandType = CommandType.StoredProcedure;
            string sql = null;
            System.Text.StringBuilder SB1 = new System.Text.StringBuilder();
            string uid = Session["UR_ID"].ToString();
            string modid = Session["ModuleCode"].ToString();
            string COM = Session["CompanyCode"+UNQSNO].ToString();
            string LOC = Session["CompanyLocationCode" + UNQSNO].ToString();
            string schm = Cn.Getschema;
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            List<DateLockRight> DLR = new List<DateLockRight>();
            var javaScriptDeSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            DLR = javaScriptDeSerializer.Deserialize<List<DateLockRight>>(serial);
            //DLR.AddRange(DLR1);
            sql = "";
            sql = sql + "Select m.MENU_ID,m.MENU_NAME,m.MENU_INDEX,m.MENU_PARENT_ID, m.MENU_ID||'~'||m.MENU_INDEX||'~'||nvl(m.MENU_PROGCALL,'Notset') as MENU_PROGCALL,m.MENU_ORDER_CODE,m.menu_type ";
            sql = sql + "from " + schm + ".APPL_MENU m ";
            sql = sql + "where m.module_code='" + modid + "' order by m.menu_order_code";
            Cn.com.Parameters.Add("SQL_STRING", sql);
            Cn.com.Parameters.Add("schema1", SCHEMA);
            OracleParameter pm2 = new OracleParameter("prc", OracleDbType.RefCursor);
            pm2.Direction = ParameterDirection.Output;
            Cn.com.Parameters.Add(pm2);
            Cn.da.SelectCommand = Cn.com;
            bool bu1 = Convert.ToBoolean(Cn.da.Fill(Cn.ds, "fill_mainmenu"));
            Cn.con.Close();
            MenuTble = Cn.ds.Tables["fill_mainmenu"];
            int root = 0, child = 0;
            string menu = "";
            ManuLine = new ArrayList();
            if (bu1 == true)
            {
                string IDENTIFIER = Session["MotherMenuIdentifier"].ToString();
                IEnumerable<DataRow> results = from row in Cn.ds.Tables["fill_mainmenu"].AsEnumerable() where row.Field<string>("MENU_TYPE") == "E" select row;
                ManuLine = new ArrayList();
                int childNode = 0;
                foreach (DataRow menu_row in results)
                {
                    string parent = menu_row.ItemArray[3].ToString().Trim();
                    string mname = menu_row.ItemArray[1].ToString().Trim();
                    string[] menuid = parent.Split('!');
                    string image = "";
                    string displayDT = "";
                    var parent_menu = (from row in Cn.ds.Tables["fill_mainmenu"].AsEnumerable() where row.Field<string>("MENU_PARENT_ID") == progcalid select row).ToList();
                    foreach (var menu_row1 in parent_menu)
                    {
                        string idstore = menu_row1.ItemArray[0].ToString() + "!" + menu_row1.ItemArray[2].ToString();
                        if (idstore == menu_row.ItemArray[0].ToString() + "!" + menu_row.ItemArray[2].ToString())
                        {
                            childNode = 1;
                            break;
                        }
                        else
                        {
                            string backidstore = idstore;
                            Stack temp = new Stack();
                            var parent_menu1 = (from row in Cn.ds.Tables["fill_mainmenu"].AsEnumerable() where row.Field<string>("MENU_PARENT_ID") == backidstore select row).ToList();
                            foreach (var menu_row2 in parent_menu1)
                            {
                                string idstore1 = menu_row2.ItemArray[0].ToString() + "!" + menu_row2.ItemArray[2].ToString();
                                if (idstore1 == menu_row.ItemArray[0].ToString() + "!" + menu_row.ItemArray[2].ToString())
                                {
                                    childNode = 1;
                                    break;
                                }
                                else
                                {
                                    while (true)
                                    {
                                        var parent_menu2 = (from row in Cn.ds.Tables["fill_mainmenu"].AsEnumerable() where row.Field<string>("MENU_PARENT_ID") == idstore1 select row).ToList();
                                        foreach (var menu_row3 in parent_menu2)
                                        {
                                            string idstore2 = menu_row3.ItemArray[0].ToString() + "!" + menu_row3.ItemArray[2].ToString();
                                            if (idstore2 == menu_row.ItemArray[0].ToString() + "!" + menu_row.ItemArray[2].ToString())
                                            {
                                                childNode = 1;
                                                break;
                                            }
                                            else
                                            {
                                                idstore1 = idstore2;
                                                parent_menu2.Remove(menu_row3);
                                                temp.Push(parent_menu2);
                                                break;
                                            }
                                        }
                                        if (childNode == 1)
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            if (temp.Count > 0)
                                            {
                                                var returnNode = temp.Pop();
                                                //parent_menu2 = returnNode;
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (childNode == 1)
                                {
                                    break;
                                }
                            }
                        }
                        if (childNode == 1)
                        {
                            break;
                        }
                    }
                    if (DLR.Any(a => a.Menu_ID == menu_row.ItemArray[0].ToString() && a.Menu_index == Convert.ToInt32(menu_row.ItemArray[2])))
                    {
                        if (childNode == 1)
                        {
                            if (Flag == "1")
                            {
                                image = "&nbsp;<img src='../Image/redlight.png' class='rightimg' id='" + menu_row.ItemArray[0].ToString() + menu_row.ItemArray[2].ToString() + "'/>";
                                displayDT = "";
                                DateLockRight getval = DLR.Where(a => a.Menu_ID == menu_row.ItemArray[0].ToString() && a.Menu_index == Convert.ToInt32(menu_row.ItemArray[2])).SingleOrDefault();
                                int indx = DLR.IndexOf(getval);
                                getval.BackDate = false;
                                getval.Lockdate = null;
                                DLR.Remove(getval);
                                childNode = 0;
                            }
                            else
                            {
                                image = "&nbsp;<img src='../Image/greenlight.png' class='rightimg' id='" + menu_row.ItemArray[0].ToString() + menu_row.ItemArray[2].ToString() + "'/>";
                                displayDT = "(" + DDT + ")";
                                DateLockRight getval = DLR.Where(a => a.Menu_ID == menu_row.ItemArray[0].ToString() && a.Menu_index == Convert.ToInt32(menu_row.ItemArray[2])).SingleOrDefault();
                                int indx = DLR.IndexOf(getval);
                                getval.BackDate = ALWBAKDT;
                                getval.Lockdate = DDT;
                                DLR[indx] = getval;
                                childNode = 0;
                            }
                        }
                        else
                        {
                            image = "&nbsp;<img src='../Image/greenlight.png' class='rightimg' id='" + menu_row.ItemArray[0].ToString() + menu_row.ItemArray[2].ToString() + "'/>";
                            displayDT = "(" + DLR.Where(a => a.Menu_ID == menu_row.ItemArray[0].ToString() && a.Menu_index == Convert.ToInt32(menu_row.ItemArray[2])).Select(a => a.Lockdate).SingleOrDefault() + ")";
                        }
                    }
                    else
                    {
                        if (childNode == 1)
                        {
                            if (Flag == "1")
                            {
                                image = "&nbsp;<img src='../Image/redlight.png' class='rightimg' id='" + menu_row.ItemArray[0].ToString() + menu_row.ItemArray[2].ToString() + "'/>";
                                displayDT = "";
                                DateLockRight getval = new DateLockRight();
                                getval.BackDate = false;
                                getval.Lockdate = null;
                                getval.Menu_ID = menu_row.ItemArray[0].ToString();
                                getval.Menu_index = Convert.ToInt32(menu_row.ItemArray[2]);
                                DLR.Remove(getval);
                                childNode = 0;
                            }
                            else
                            {
                                image = "&nbsp;<img src='../Image/greenlight.png' class='rightimg' id='" + menu_row.ItemArray[0].ToString() + menu_row.ItemArray[2].ToString() + "'/>";
                                displayDT = "(" + DDT + ")";
                                DateLockRight getval = new DateLockRight();
                                getval.BackDate = ALWBAKDT;
                                getval.Lockdate = DDT;
                                getval.Menu_ID = menu_row.ItemArray[0].ToString();
                                getval.Menu_index = Convert.ToInt32(menu_row.ItemArray[2]);
                                DLR.Add(getval);
                                childNode = 0;
                            }
                        }
                        else
                        {
                            image = "&nbsp;<img src='../Image/redlight.png' class='rightimg' id='" + menu_row.ItemArray[0].ToString() + menu_row.ItemArray[2].ToString() + "'/>";
                        }
                    }
                    string Omenu = " <li><span class='tree_label'>" + "<img src='../Image/file1.png' class='fileimg'/>&nbsp;" + "<span title='" + mname + "' onclick=return&nbsp;setpermission('" + menu_row.ItemArray[0].ToString().Trim() + "'," + menu_row.ItemArray[2] + ");>" + mname + image + "</span>&nbsp;<span style='color:#ececec' class='default_label_date'>" + displayDT + "</span>" + "</span></li>";
                    while (true)
                    {
                        IEnumerable<DataRow> results1 =
                        from row in Cn.ds.Tables["fill_mainmenu"].AsEnumerable()
                        where row.Field<string>("MENU_ID") == menuid[0].ToString() && row.Field<Int16>("MENU_INDEX") == Convert.ToInt16(menuid[1])
                        orderby row.Field<string>("MENU_ORDER_CODE")
                        select row;
                        DataTable boundTable = results1.CopyToDataTable<DataRow>();
                        if (boundTable.Rows[0]["MENU_PARENT_ID"].ToString() == IDENTIFIER)
                        {
                            string parent2 = boundTable.Rows[0][3].ToString();
                            string id = menuid[0] + "!" + menuid[1];
                            if (checkParent.ContainsKey(menuid[0] + "!" + menuid[1]) == false)
                            {
                                string Menu = "<li>";
                                Menu = Menu + "<input type='checkbox' checked='checked'  id='" + id + "'/>" + "<label class='tree_label' for='" + id + "'><img src='../Image/folder.png' class='folderimg'/>&nbsp;" + boundTable.Rows[0][1].ToString() + "</label><span class='default_label'>&nbsp;(</span><span style='color:#ececec' class='default_label' onclick=defaultdatewindow('" + boundTable.Rows[0][0].ToString() + "!" + boundTable.Rows[0][2].ToString() + "');>Set Defalt</span><span class='default_label'>)</span><span class='default_label' onclick=defaultPermission('" + boundTable.Rows[0][0].ToString() + "!" + boundTable.Rows[0][2].ToString() + "',1);>&nbsp;(Reset)</span>";
                                Menu = Menu + "<ul>";
                                checkParent.Add(boundTable.Rows[0][0].ToString() + "!" + boundTable.Rows[0][2].ToString(), Menu);
                                if (root == 0)
                                {
                                    menu = Menu + menu;
                                    root = 1;
                                }
                                else
                                {
                                    //menu = menu + "</ul> </li></ul> </li>";
                                    menu = menu + "</ul></li>";
                                    ManuLine.Add(menu);
                                    menu = Menu + Omenu;
                                    child = 0;
                                }
                            }
                            else
                            {
                                if (child == 0)
                                {
                                    menu = menu + Omenu;
                                }
                            }
                            break;
                        }
                        else
                        {
                            string parent1 = boundTable.Rows[0][3].ToString();
                            string id = menuid[0] + "!" + menuid[1];
                            if (checkParent.ContainsKey(menuid[0] + "!" + menuid[1]) == false)
                            {
                                string SubMenu = "<li>";
                                SubMenu = SubMenu + "<input type='checkbox' checked='checked'  id='" + id + "'/>" + "<label class='tree_label' for='" + id + "'><img src='../Image/folder.png' class='folderimg'/>&nbsp;" + boundTable.Rows[0][1].ToString() + "</label><span class='default_label'>&nbsp;(</span><span style='color:#ececec' class='default_label' onclick=defaultdatewindow('" + boundTable.Rows[0][0].ToString() + "!" + boundTable.Rows[0][2].ToString() + "');>Set Defalt</span><span class='default_label'>)</span><span class='default_label' onclick=defaultPermission('" + boundTable.Rows[0][0].ToString() + "!" + boundTable.Rows[0][2].ToString() + "',1);>&nbsp;(Reset)</span>";
                                SubMenu = SubMenu + "<ul>";
                                checkParent.Add(boundTable.Rows[0][0].ToString() + "!" + boundTable.Rows[0][2].ToString(), SubMenu);
                                if (child == 0)
                                {
                                    menu = SubMenu + Omenu;
                                    child = 1;
                                }
                                else
                                {
                                    if (checkParent.ContainsKey(parent1) == false)
                                    {
                                        menu = menu + "</ul></li>";
                                        ManuLine.Add(menu);
                                        root = 0;
                                        menu = SubMenu + Omenu;
                                    }
                                    else
                                    {
                                        menu = menu + "</ul> </li>" + SubMenu + Omenu;
                                    }
                                }
                            }
                            else
                            {
                                menu = menu + Omenu;
                                break;
                            }
                            menuid = parent1.Split('!');
                        }
                    }
                }
            }
            ManuLine.Add(menu + "</ul> </li>");
            #endregion
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            if (ManuLine != null)
            {
                for (int i = 0; i <= ManuLine.Count - 1; i++)
                {
                    SB.Append(ManuLine[i].ToString());
                }
            }
            Mnu.ManuDetails = SB.ToString();
            var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            Mnu.serializeString = javaScriptSerializer.Serialize(DLR);
            return PartialView("_datalockTree", Mnu);
        }

        public ActionResult DefaultDatePermission()
        {
            return PartialView("_datelockDefaultdate", null);
        }
    }
}