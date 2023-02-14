using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using Oracle.ManagedDataAccess.Client;

namespace Improvar.Controllers
{
    public class TS_DOCAUTHController : Controller
    {
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: TS_DOCAUTH
        public ActionResult TS_DOCAUTH(string op = "", string key = "", int Nindex = 0, string searchValue = "", string param_SQL = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Document Authorisation Entry";
                    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    DocumentAuthorisationEntry VE = new DocumentAuthorisationEntry();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);

                    //show  record
                    List<DropDown_list> COMBO = new List<DropDown_list>();
                    DropDown_list COMBO1 = new DropDown_list();
                    COMBO1.text = "Pending";
                    COMBO1.value = "P";
                    COMBO.Add(COMBO1);
                    DropDown_list COMBO3 = new DropDown_list();
                    COMBO3.text = "Authorised";
                    COMBO3.value = "A";
                    COMBO.Add(COMBO3);
                    DropDown_list COMBO2 = new DropDown_list();
                    COMBO2.text = "Reject";
                    COMBO2.value = "R";
                    COMBO.Add(COMBO2);
                    DropDown_list COMBO4 = new DropDown_list();
                    COMBO4.text = "Cancel";
                    COMBO4.value = "C";
                    COMBO.Add(COMBO4);

                    //DropDown_list COMBO4 = new DropDown_list();
                    //COMBO4.text = "Show All";
                    //COMBO4.value = "S";
                    //COMBO.Add(COMBO4);
                    VE.DropDown_list = COMBO;
                    //show  record
                    VE.DOCTYPE = param_SQL;
                    VE.TCNTRLAUTH = GetGridData("", "", "P", VE.DOCTYPE, true);
                    VE.SHOW_RECORD = "P";
                    VE.DefaultView = true;
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                DocumentAuthorisationEntry VE = new DocumentAuthorisationEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        //public dynamic GetGridData(string FROMDT, string TODT, string SHOW_RECORD, string DOCTYPE, bool TAG = false)
        //{
        //    try
        //    {
        //        DocumentAuthorisationEntry VE = new DocumentAuthorisationEntry();
        //        Cn.getQueryString(VE);

        //        string DATABSE = CommVar.CurSchema(UNQSNO);
        //        string DATABSE_F = CommVar.FinSchema(UNQSNO);
        //        string compcd = CommVar.Compcd(UNQSNO);
        //        if (DOCTYPE.Length > 0)
        //        {
        //            //DOCTYPE = Cn.Decrypt(DOCTYPE);
        //            string[] param = DOCTYPE.Split('_');
        //            if (param[0] == "" && param[1] == "")
        //            {
        //                DOCTYPE = "";
        //            }
        //            else
        //            {
        //                string doctype = string.Join("','", param[0].Split(',').Select(a => a));
        //                string doccd = string.Join("','", param[1].Split(',').Select(a => a));
        //                DOCTYPE = " and d.doctype in ('" + doctype + "') and b.doccd in ('" + doccd + "') ";
        //            }
        //        }
        //        string SQL = "", SQL1 = "", SQL2 = "", SQL3 = "";
        //        if (SHOW_RECORD == "P" || SHOW_RECORD == "S")
        //        {
        //            SQL1 = "select distinct '' docpassed,a.autono,a.slno,a.pass_level,a.authcd,a.AUTHREM,c.dname,b.doccd,b.docno,b.docdt,b.usr_id,b.usr_entdt, ";
        //            SQL1 += "b.emd_no,b.slcd,b.docamt,f.slnm,c.menu_progcall,c.MENU_PARA,d.doctype,b.cancel,b.glcd,g.glnm,b.loccd from ";
        //            SQL1 += DATABSE + ".t_cntrl_doc_pass a," + DATABSE + ".t_cntrl_hdr b," + DATABSE + ".m_dtype c," + DATABSE + ".m_doctype d, ";
        //            SQL1 += DATABSE_F + ".m_sign_auth e, ";
        //            SQL1 += DATABSE_F + ".m_subleg f," + DATABSE_F + ".m_genleg g where a.autono = b.autono and a.authcd = e.authcd and b.doccd = d.doccd  ";
        //            SQL1 += " and d.doctype = c.dcd and b.slcd = f.slcd(+) and b.glcd = g.glcd(+) ";
        //            SQL1 += "and e.usrid = '" + Session["UR_ID"].ToString() + "' " + DOCTYPE + " and b.compcd='" + compcd + "' and nvl(b.cancel,'N')='N' ";
        //            if (FROMDT.retStr() != "") SQL1 += "and b.docdt >= to_date('" + FROMDT + "', 'dd/mm/yyyy') ";
        //            if (TODT.retStr() != "") SQL1 += "and b.docdt <= to_date('" + TODT + "', 'dd/mm/yyyy') ";
        //            if (SHOW_RECORD != "S") SQL1 += "order by docdt,docno ";
        //            SQL = SQL1;

        //        }
        //        if (SHOW_RECORD.retStr() == "R" || SHOW_RECORD.retStr() == "A" || SHOW_RECORD.retStr() == "C" || SHOW_RECORD == "S")
        //        {
        //            DOCTYPE = DOCTYPE.Replace("d.", "e.");
        //            DOCTYPE = DOCTYPE.Replace("b.", "c.");
        //            SQL2 = "select distinct a.docpassed,a.autono,a.slno,a.pass_level,'' authcd,a.AUTH_REM AUTHREM,d.dname,c.doccd,c.docno,c.docdt,c.usr_id,c.usr_entdt, ";
        //            SQL2 += "c.emd_no,c.slcd,c.docamt,g.slnm,d.menu_progcall,d.MENU_PARA,e.doctype,c.cancel,c.glcd,h.glnm,c.loccd,i.user_name from ";
        //            SQL2 += DATABSE + ".T_CNTRL_AUTH a," + DATABSE + ".t_cntrl_doc_pass b," + DATABSE + ".t_cntrl_hdr c," + DATABSE + ".m_dtype d, ";
        //            SQL2 += DATABSE + ".m_doctype e, ";
        //            SQL2 += DATABSE_F + ".m_subleg g," + DATABSE_F + ".m_genleg h,user_appl i where a.autono = b.autono(+) and a.autono = c.autono  ";
        //            SQL2 += "and c.doccd = e.doccd and e.doctype = d.dcd and c.slcd = g.slcd(+) and c.glcd = h.glcd(+) and a.usr_id = i.user_id(+) ";
        //            SQL2 += "and a.usr_id = '" + Session["UR_ID"].ToString() + "' " + DOCTYPE + " and c.compcd='" + compcd + "' ";
        //            if (FROMDT.retStr() != "") SQL2 += "and c.docdt >= to_date('" + FROMDT + "', 'dd/mm/yyyy') ";
        //            if (TODT.retStr() != "") SQL2 += "and c.docdt <= to_date('" + TODT + "', 'dd/mm/yyyy') ";

        //            if (SHOW_RECORD.retStr() == "R")
        //            {
        //                SQL2 += "and a.docpassed ='N' ";
        //            }
        //            else if (SHOW_RECORD.retStr() == "A")
        //            {
        //                SQL2 += "and a.docpassed ='Y' ";
        //            }
        //            else if (SHOW_RECORD.retStr() == "C")
        //            {
        //                SQL2 += "and a.docpassed ='C' ";
        //            }
        //            if (SHOW_RECORD != "S") SQL2 += "order by docdt,docno ";
        //            SQL = SQL2;

        //        }
        //        if (SHOW_RECORD == "S")
        //        {
        //            SQL3 = "select a.docpassed,a.autono,a.slno,a.pass_level,a.authcd,a.AUTHREM,a.dname,a.doccd,a.docno,a.docdt,a.usr_id,a.usr_entdt,a.emd_no,a.slcd,a.docamt,a.slnm,a.menu_progcall,a.MENU_PARA,a.doctype,a.cancel,a.glcd,a.glnm,a.loccd from ";
        //            SQL3 += "( " + SQL1 + " union " + SQL2 + ") a ";
        //            SQL3 += "order by docdt,docno ";
        //            SQL = SQL3;
        //        }

        //        DataTable GRID_DATA = new DataTable();

        //        GRID_DATA = masterHelp.SQLquery(SQL);

        //        VE.TCNTRLAUTH = (from DataRow dr in GRID_DATA.Rows
        //                         select new TCNTRLAUTH()
        //                         {
        //                             AUTONO = dr["AUTONO"].ToString(),
        //                             SLNO = Convert.ToByte(dr["slno"].ToString()),
        //                             AUTHREM = dr["AUTHREM"].ToString(),
        //                             DNAME = dr["DNAME"].ToString(),
        //                             DOCCD = dr["DOCCD"].ToString(),
        //                             DOCTYPE = dr["DOCTYPE"].ToString(),
        //                             DOCNO = dr["DOCNO"].ToString(),
        //                             DOCDT = dr["DOCDT"].retStr().Remove(10),
        //                             USR_ID = dr["USR_ID"].ToString(),
        //                             USR_ENTDT = dr["USR_ENTDT"].retStr().Remove(10),
        //                             EMD_NO = Convert.ToInt16(dr["EMD_NO"].ToString()),
        //                             SLCD = dr["SLCD"].ToString(),
        //                             SLNM = dr["SLNM"].ToString(),
        //                             GLCD = dr["GLCD"].ToString(),
        //                             GLNM = dr["GLNM"].ToString(),
        //                             DOCAMT = Convert.ToDouble(dr["DOCAMT"].ToString()),
        //                             PASS_LEVEL = dr["pass_level"].ToString() == "" ? (byte?)null : Convert.ToByte(dr["pass_level"].ToString()),
        //                             MENU_PROGCALL = dr["menu_progcall"].ToString(),
        //                             MENU_PARA = dr["menu_PARA"].ToString(),
        //                             Cancel = (dr["CANCEL"].ToString().ToUpper() == "Y" ? true : false),
        //                             DOCPASSED = (SHOW_RECORD.retStr() == "R" || SHOW_RECORD.retStr() == "A" || SHOW_RECORD == "S") ? dr["DOCPASSED"].ToString() : "",
        //                             LOCCD = dr["LOCCD"].ToString(),
        //                             AUTH_MNM = SHOW_RECORD.retStr() == "P" ? "" : dr["user_name"].ToString(),
        //                         }).ToList();

        //        for (int p = 0; p <= VE.TCNTRLAUTH.Count - 1; p++)
        //        {
        //            VE.TCNTRLAUTH[p].AUTH_SLNO = Convert.ToByte(p + 1);
        //            if (VE.TCNTRLAUTH[p].DOCPASSED == "Y")
        //            {
        //                VE.TCNTRLAUTH[p].Approved = true;
        //            }
        //            if (VE.TCNTRLAUTH[p].DOCPASSED == "N")
        //            {
        //                VE.TCNTRLAUTH[p].UnApproved = true;
        //            }

        //        }
        //        VE.SHOW_RECORD = SHOW_RECORD;
        //        if (TAG == true)
        //        {
        //            return VE.TCNTRLAUTH;
        //        }
        //        VE.DefaultView = true;
        //        ModelState.Clear();
        //        return PartialView("_TS_DOCAUTH_MAIN", VE);
        //    }
        //    catch (Exception ex)
        //    {
        //        Cn.SaveException(ex, "");
        //        return Content(ex.Message + ex.InnerException);
        //    }
        //}
        public dynamic GetGridData(string FROMDT, string TODT, string SHOW_RECORD, string DOCTYPE, bool TAG = false)
        {
            try
            {
                DocumentAuthorisationEntry VE = new DocumentAuthorisationEntry();
                Cn.getQueryString(VE);

                string DATABSE = CommVar.CurSchema(UNQSNO);
                string DATABSE_F = CommVar.FinSchema(UNQSNO);
                string compcd = CommVar.Compcd(UNQSNO);
                if (DOCTYPE.Length > 0)
                {
                    //DOCTYPE = Cn.Decrypt(DOCTYPE);
                    string[] param = DOCTYPE.Split('_');
                    if (param[0] == "" && param[1] == "")
                    {
                        DOCTYPE = "";
                    }
                    else
                    {
                        string doctype = string.Join("','", param[0].Split(',').Select(a => a));
                        string doccd = string.Join("','", param[1].Split(',').Select(a => a));
                        DOCTYPE = " and e.doctype in ('" + doctype + "') and c.doccd in ('" + doccd + "') ";
                    }
                }
                string SQL = "", SQL1 = "", SQL2 = "", SQL3 = "";
                string SQLC = "and a.autono not in (select autono from " + DATABSE_F + ".t_txneinv) ";
                if (SHOW_RECORD == "P" || SHOW_RECORD.retStr() == "C")
                {
                    //pending=new entry,current level auth pending but prev level auth done and next level auth pending
                    string DOCTYPE1 = DOCTYPE.Replace("e.", "d.");
                    DOCTYPE1 = DOCTYPE1.Replace("c.", "b.");
                    SQL1 = "select distinct x.docpassed,x.autono,x.slno,x.pass_level,x.authcd,x.AUTHREM,x.dname,x.doccd,x.docno,x.docdt,x.usr_id,x.usr_entdt, " + Environment.NewLine;
                    SQL1 += "x.emd_no,x.slcd,x.docamt,x.slnm,x.menu_progcall,x.MENU_PARA,x.doctype,x.cancel,x.glcd,x.glnm,x.loccd,x.user_name,l.lvl authrisedlvl from( " + Environment.NewLine;

                    SQL1 += "select j.STSTYPE docpassed,a.autono,a.slno,min(a.pass_level)pass_level,a.authcd,a.AUTHREM,c.dname,b.doccd,b.docno,b.docdt,b.usr_id,b.usr_entdt, " + Environment.NewLine;
                    SQL1 += "b.emd_no,b.slcd,b.docamt,f.slnm,c.menu_progcall,c.MENU_PARA,d.doctype,b.cancel,b.glcd,g.glnm,b.loccd,'' user_name,j.USR_ID authrisedUSR_ID, " + Environment.NewLine;
                    SQL1 += "row_number() over(partition by a.autono order by a.autono desc) as rn " + Environment.NewLine;
                    SQL1 += "from " + Environment.NewLine;
                    SQL1 += DATABSE + ".t_cntrl_doc_pass a," + DATABSE + ".t_cntrl_hdr b," + DATABSE + ".m_dtype c," + DATABSE + ".m_doctype d, " + Environment.NewLine;
                    SQL1 += DATABSE_F + ".m_sign_auth e, " + Environment.NewLine;
                    SQL1 += DATABSE_F + ".m_subleg f," + DATABSE_F + ".m_genleg g," + DATABSE + ".M_DOC_AUTH i," + DATABSE + ".T_TXNSTATUS j " + Environment.NewLine;
                    SQL1 += "where a.autono = b.autono and a.authcd = e.authcd and b.doccd = d.doccd and e.authcd=i.authcd and b.doccd=i.doccd and a.autono=j.autono(+)  " + Environment.NewLine;
                    SQL1 += " and d.doctype = c.dcd and b.slcd = f.slcd(+) and b.glcd = g.glcd(+) " + Environment.NewLine;
                    SQL1 += " and (a.autono not in (select autono from " + DATABSE + ".T_TXNSTATUS where STSTYPE in ('N','C','A') )or " + Environment.NewLine;
                    SQL1 += " a.autono in (select autono from " + DATABSE + ".T_TXNSTATUS where STSTYPE in ('A') )) " + Environment.NewLine;
                    SQL1 += "and e.usrid = '" + Session["UR_ID"].ToString() + "' " + DOCTYPE1 + " and b.compcd='" + compcd + "' " + Environment.NewLine;
                    if (SHOW_RECORD == "P") { SQL1 += "  and nvl(b.cancel,'N')='N' " + Environment.NewLine; } else { SQL1 += "  and nvl(b.cancel,'N')='Y' " + Environment.NewLine; }
                    if (FROMDT.retStr() != "") SQL1 += "and b.docdt >= to_date('" + FROMDT + "', 'dd/mm/yyyy') " + Environment.NewLine;
                    if (TODT.retStr() != "") SQL1 += "and b.docdt <= to_date('" + TODT + "', 'dd/mm/yyyy') " + Environment.NewLine;
                    SQL1 += SQLC;
                    SQL1 += "group by j.STSTYPE,a.autono,a.slno,a.authcd,a.AUTHREM,c.dname,b.doccd,b.docno,b.docdt,b.usr_id,b.usr_entdt, " + Environment.NewLine;
                    SQL1 += "b.emd_no,b.slcd,b.docamt,f.slnm,c.menu_progcall,c.MENU_PARA,d.doctype,b.cancel,b.glcd,g.glnm,b.loccd,j.USR_ID " + Environment.NewLine;
                    SQL1 += ") x, ";

                    SQL1 += "(select a.autono,max(c.lvl)lvl,b.usrid,c.DOCCD from " + DATABSE + ".t_txnstatus a," + DATABSE_F + ".m_sign_auth b," + DATABSE + ".M_DOC_AUTH c " + Environment.NewLine;
                    SQL1 += "where b.authcd = c.authcd and STSTYPE='A' " + Environment.NewLine;
                    SQL1 += "and c.lvl not in (select distinct PASS_LEVEL from " + DATABSE + ".t_cntrl_doc_pass where autono=a.autono) " + Environment.NewLine;
                    SQL1 += "and b.authcd = c.authcd and STSTYPE='A' " + Environment.NewLine;
                    SQL1 += "group by  a.autono,b.usrid,c.DOCCD " + Environment.NewLine;
                    SQL1 += ")l " + Environment.NewLine;

                    SQL1 += "where x.autono=l.autono(+) and x.authrisedUSR_ID=l.usrid(+) AND x.DOCCD = l.doccd(+) " + Environment.NewLine;
                    SQL1 += "and x.rn = 1 " + Environment.NewLine;
                    if (SHOW_RECORD == "P") SQL1 += "order by docdt,docno " + Environment.NewLine;
                    SQL = SQL1;
                }
                if (SHOW_RECORD.retStr() == "A" || SHOW_RECORD.retStr() == "C")//auth=current status=auth,upto current level auth done and next level auth pending
                {
                    //current auth data
                    SQL2 = "select distinct a.STSTYPE docpassed, a.autono,b.slno,b.pass_level,'' authcd,a.STSREM AUTHREM, d.dname,c.doccd,c.docno,c.docdt,c.usr_id,c.usr_entdt, " + Environment.NewLine;
                    SQL2 += "c.emd_no,c.slcd,c.docamt,g.slnm,d.menu_progcall,d.MENU_PARA,e.doctype,c.cancel,c.glcd,h.glnm,c.loccd,i.user_name,0 authrisedlvl " + Environment.NewLine;
                    SQL2 += "from " + DATABSE + ".T_TXNSTATUS a, " + DATABSE + ".T_CNTRL_AUTH b, " + DATABSE + ".t_cntrl_hdr c, " + DATABSE + ".m_dtype d, " + DATABSE + ".m_doctype e, " + Environment.NewLine;
                    SQL2 += "" + DATABSE_F + ".m_subleg g, " + DATABSE_F + ".m_genleg h, user_appl i," + DATABSE + ".M_DOC_AUTH j," + DATABSE_F + ".m_sign_auth k " + Environment.NewLine;
                    SQL2 += "where a.autono = b.autono(+) and a.autono = c.autono  and c.doccd = e.doccd and e.doctype = d.dcd and c.slcd = g.slcd(+) " + Environment.NewLine;
                    SQL2 += "and c.doccd=j.doccd and b.pass_level=j.lvl and j.authcd=k.authcd ";
                    SQL2 += "and c.glcd = h.glcd(+) and a.usr_id = i.user_id(+) and k.usrid = '" + Session["UR_ID"].ToString() + "'  and c.compcd = '" + compcd + "'  " + DOCTYPE + " " + Environment.NewLine;
                    if (SHOW_RECORD.retStr() == "A") { SQL2 += "  and a.STSTYPE = 'A' " + Environment.NewLine; } else { SQL2 += "  and a.STSTYPE = 'C' " + Environment.NewLine; }
                    if (FROMDT.retStr() != "") SQL2 += "and c.docdt >= to_date('" + FROMDT + "', 'dd/mm/yyyy') " + Environment.NewLine;
                    if (TODT.retStr() != "") SQL2 += "and c.docdt <= to_date('" + TODT + "', 'dd/mm/yyyy') " + Environment.NewLine;
                    SQL2 += SQLC;

                    //current level unauth prev lavel data
                    SQL2 += "union " + Environment.NewLine;
                    SQL2 += "select distinct 'A' docpassed,a.autono,j.slno,i.lvl pass_level,''authcd,''AUTHREM,c.dname,b.doccd,b.docno,b.docdt,b.usr_id,b.usr_entdt, " + Environment.NewLine;
                    SQL2 += "b.emd_no,b.slcd,b.docamt,f.slnm,c.menu_progcall,c.MENU_PARA,d.doctype,b.cancel,b.glcd,g.glnm,b.loccd,'' user_name,0 authrisedlvl from " + Environment.NewLine;
                    SQL2 += DATABSE + ".t_cntrl_doc_pass a," + DATABSE + ".t_cntrl_hdr b," + DATABSE + ".m_dtype c," + DATABSE + ".m_doctype d, " + Environment.NewLine;
                    SQL2 += DATABSE_F + ".m_subleg f," + DATABSE_F + ".m_genleg g," + DATABSE + ".M_DOC_AUTH i," + DATABSE + ".T_TXNSTATUS j," + DATABSE_F + ".m_sign_auth e," + DATABSE + ".M_DOC_AUTH j," + DATABSE_F + ".m_sign_auth k " + Environment.NewLine;
                    SQL2 += "where a.autono = b.autono and b.doccd = d.doccd and a.authcd = i.authcd and b.doccd=i.doccd(+) and a.autono=j.autono and i.authcd = e.authcd  " + Environment.NewLine;
                    SQL2 += "and d.doctype = c.dcd and b.slcd = f.slcd(+) and b.glcd = g.glcd(+) and " + Environment.NewLine;
                    SQL2 += "b.doccd=j.doccd and i.lvl=j.lvl and j.authcd=k.authcd and ";

                    SQL2 += "i.lvl+1 in (select min(z.lvl)lvl from " + DATABSE + ".t_txnstatus x," + DATABSE_F + ".m_sign_auth y," + DATABSE + ".M_DOC_AUTH z " + Environment.NewLine;
                    SQL2 += "where y.authcd = z.authcd and STSTYPE='N' and x.autono=a.autono " + Environment.NewLine;
                    SQL2 += "and z.lvl in (select distinct PASS_LEVEL from " + DATABSE + ".t_cntrl_doc_pass where autono=x.autono) " + Environment.NewLine;
                    SQL2 += ") " + Environment.NewLine;

                    SQL2 += "and k.usrid = '" + Session["UR_ID"].ToString() + "' " + DOCTYPE + " and b.compcd='" + compcd + "' " + Environment.NewLine;
                    if (FROMDT.retStr() != "") SQL2 += "and b.docdt >= to_date('" + FROMDT + "', 'dd/mm/yyyy') " + Environment.NewLine;
                    if (TODT.retStr() != "") SQL2 += "and b.docdt <= to_date('" + TODT + "', 'dd/mm/yyyy') " + Environment.NewLine;
                    SQL2 += SQLC;

                    if (SHOW_RECORD.retStr() == "A") SQL2 += "order by docdt,docno " + Environment.NewLine;
                    SQL = SQL2;

                }
                if (SHOW_RECORD.retStr() == "R" || SHOW_RECORD.retStr() == "C")
                {
                    SQL3 = "select x.docpassed, x.autono,x.slno,x.pass_level,x.authcd,x.AUTHREM, x.dname,x.doccd,x.docno,x.docdt,x.usr_id,x.usr_entdt, " + Environment.NewLine;
                    SQL3 += "x.emd_no,x.slcd,x.docamt,x.slnm,x.menu_progcall,x.MENU_PARA,x.doctype,x.cancel,x.glcd,x.glnm,x.loccd,x.user_name,l.lvl authrisedlvl from (" + Environment.NewLine;

                    SQL3 += "select distinct a.STSTYPE docpassed, a.autono,b.slno,b.pass_level,'' authcd,a.STSREM AUTHREM, d.dname,c.doccd,c.docno,c.docdt,c.usr_id,c.usr_entdt, " + Environment.NewLine;
                    SQL3 += "c.emd_no,c.slcd,c.docamt,g.slnm,d.menu_progcall,d.MENU_PARA,e.doctype,c.cancel,c.glcd,h.glnm,c.loccd,i.user_name,0 authrisedlvl,a.USR_ID authrisedUSR_ID " + Environment.NewLine;
                    SQL3 += ",row_number() over(partition by a.autono order by a.autono,b.pass_level asc) as rn " + Environment.NewLine;
                    SQL3 += "from " + DATABSE + ".T_TXNSTATUS a, " + DATABSE + ".t_cntrl_doc_pass b, " + DATABSE + ".t_cntrl_hdr c, " + DATABSE + ".m_dtype d, " + DATABSE + ".m_doctype e, " + DATABSE_F + ".m_sign_auth f, " + Environment.NewLine;
                    SQL3 += "" + DATABSE_F + ".m_subleg g, " + DATABSE_F + ".m_genleg h, user_appl i," + DATABSE + ".M_DOC_AUTH j," + DATABSE_F + ".m_sign_auth k " + Environment.NewLine;
                    SQL3 += "where a.autono = b.autono(+) and a.autono = c.autono  and c.doccd = e.doccd and e.doctype = d.dcd and c.slcd = g.slcd(+) " + Environment.NewLine;
                    //SQL3 += "and c.glcd = h.glcd(+) and a.usr_id = i.user_id(+) and b.authcd = f.authcd(+) and f.usrid = '" + Session["UR_ID"].ToString() + "' and a.usr_id = '" + Session["UR_ID"].ToString() + "'  and c.compcd = '" + compcd + "' " + DOCTYPE + " ";
                    SQL3 += "and c.glcd = h.glcd(+) and a.usr_id = i.user_id(+) and a.usr_id = f.usrid(+) and b.authcd = f.authcd  and c.compcd = '" + compcd + "' " + DOCTYPE + " " + Environment.NewLine;
                    SQL3 += "and c.doccd=j.doccd and b.pass_level=j.lvl and j.authcd=k.authcd and k.usrid = '" + Session["UR_ID"].ToString() + "' ";
                    if (SHOW_RECORD.retStr() == "R") { SQL3 += "and a.STSTYPE = 'N' " + Environment.NewLine; } else { SQL3 += "and a.STSTYPE = 'C' " + Environment.NewLine; }
                    if (FROMDT.retStr() != "") SQL3 += "and c.docdt >= to_date('" + FROMDT + "', 'dd/mm/yyyy') " + Environment.NewLine;
                    if (TODT.retStr() != "") SQL3 += "and c.docdt <= to_date('" + TODT + "', 'dd/mm/yyyy') " + Environment.NewLine;
                    SQL3 += SQLC;
                    SQL3 += ")x, ";

                    SQL3 += "(select a.autono,min(c.lvl)lvl,b.usrid,c.DOCCD from " + DATABSE + ".t_txnstatus a," + DATABSE_F + ".m_sign_auth b," + DATABSE + ".M_DOC_AUTH c " + Environment.NewLine;
                    SQL3 += "where b.authcd = c.authcd and STSTYPE='N' " + Environment.NewLine;
                    SQL3 += "and c.lvl in (select distinct PASS_LEVEL from " + DATABSE + ".t_cntrl_doc_pass where autono=a.autono) " + Environment.NewLine;
                    SQL3 += "and b.authcd = c.authcd " + Environment.NewLine;
                    SQL3 += "group by  a.autono,b.usrid,c.DOCCD " + Environment.NewLine;
                    SQL3 += ")l " + Environment.NewLine;
                    SQL3 += "where x.autono=l.autono(+) and x.authrisedUSR_ID=l.usrid(+) AND x.DOCCD = l.doccd(+) and x.pass_level=l.lvl " + Environment.NewLine;
                    SQL3 += "and x.rn = 1 " + Environment.NewLine;

                    if (SHOW_RECORD.retStr() == "R") SQL3 += "order by docdt,docno " + Environment.NewLine;
                    SQL = SQL3;
                }

                if (SHOW_RECORD.retStr() == "C")
                {
                    SQL = SQL1 + " union " + SQL2 + " union " + SQL3 + " order by docdt,docno ";
                }

                DataTable GRID_DATA = new DataTable();

                GRID_DATA = masterHelp.SQLquery(SQL);

                string sql = "select a.AUTONO,STSTYPE,b.doccd from  " + DATABSE + ".t_txnstatus a," + DATABSE + ".t_cntrl_hdr b ";
                sql += "where a.autono=b.autono ";
                DataTable dt = masterHelp.SQLquery(sql);

                VE.TCNTRLAUTH = (from DataRow dr in GRID_DATA.Rows
                                 select new TCNTRLAUTH()
                                 {
                                     AUTONO = dr["AUTONO"].ToString(),
                                     SLNO = Convert.ToByte(dr["slno"].ToString()),
                                     AUTHREM = dr["AUTHREM"].ToString(),
                                     DNAME = dr["DNAME"].ToString(),
                                     DOCCD = dr["DOCCD"].ToString(),
                                     DOCTYPE = dr["DOCTYPE"].ToString(),
                                     DOCNO = dr["DOCNO"].ToString(),
                                     DOCDT = dr["DOCDT"].retStr().Remove(10),
                                     USR_ID = dr["USR_ID"].ToString(),
                                     USR_ENTDT = dr["USR_ENTDT"].retStr().Remove(10),
                                     EMD_NO = Convert.ToInt16(dr["EMD_NO"].ToString()),
                                     SLCD = dr["SLCD"].ToString(),
                                     SLNM = dr["SLNM"].ToString(),
                                     GLCD = dr["GLCD"].ToString(),
                                     GLNM = dr["GLNM"].ToString(),
                                     DOCAMT = Convert.ToDouble(dr["DOCAMT"].ToString()),
                                     PASS_LEVEL = dr["pass_level"].ToString() == "" ? (byte?)null : Convert.ToByte(dr["pass_level"].ToString()),
                                     MENU_PROGCALL = dr["menu_progcall"].ToString(),
                                     MENU_PARA = dr["menu_PARA"].ToString(),
                                     Cancel = (dr["CANCEL"].ToString().ToUpper() == "Y" ? true : false),
                                     //DOCPASSED = (SHOW_RECORD.retStr() == "R" || SHOW_RECORD.retStr() == "A") ? dr["DOCPASSED"].ToString() : "",
                                     DOCPASSED = dr["DOCPASSED"].ToString(),
                                     LOCCD = dr["LOCCD"].ToString(),
                                     AUTH_MNM = SHOW_RECORD.retStr() == "P" ? "" : dr["user_name"].ToString(),
                                     AUTHLVL = dr["authrisedlvl"].ToString(),
                                     PREVLVLSTS = "A",
                                     NXTLVLSTS = "N",
                                 }).ToList();
                int cnt = 1;
                for (int p = 0; p <= VE.TCNTRLAUTH.Count - 1; p++)
                {

                    string authlvl = VE.TCNTRLAUTH[p].PASS_LEVEL.retStr();

                    if (SHOW_RECORD.retStr() == "P")
                    {
                        VE.TCNTRLAUTH[p].DOCPASSED = "";
                        if (authlvl.retDbl() == 0)
                        {
                            VE.TCNTRLAUTH[p].NXTLVLSTS = "N"; VE.TCNTRLAUTH[p].PREVLVLSTS = "N";
                        }
                        else if (authlvl.retDbl() <= VE.TCNTRLAUTH[p].AUTHLVL.retDbl() && VE.TCNTRLAUTH[p].DOCPASSED.retStr() == "A")
                        {
                            //Authorised
                            if (authlvl.retDbl() < VE.TCNTRLAUTH[p].AUTHLVL.retDbl()) { VE.TCNTRLAUTH[p].NXTLVLSTS = "A"; VE.TCNTRLAUTH[p].PREVLVLSTS = "A"; }
                        }
                        else if (authlvl.retDbl() < VE.TCNTRLAUTH[p].AUTHLVL.retDbl() && VE.TCNTRLAUTH[p].DOCPASSED.retStr() == "N")
                        {
                            //Authorised
                            if (authlvl.retDbl() < VE.TCNTRLAUTH[p].AUTHLVL.retDbl() - 1) { VE.TCNTRLAUTH[p].NXTLVLSTS = "A"; VE.TCNTRLAUTH[p].PREVLVLSTS = "A"; }
                        }
                        else if (authlvl.retDbl() > VE.TCNTRLAUTH[p].AUTHLVL.retDbl() && VE.TCNTRLAUTH[p].DOCPASSED.retStr() == "N")
                        {
                            VE.TCNTRLAUTH[p].NXTLVLSTS = "N"; VE.TCNTRLAUTH[p].PREVLVLSTS = "N";
                        }

                        string doccd = VE.TCNTRLAUTH[p].DOCCD.retStr();
                        string autono = VE.TCNTRLAUTH[p].AUTONO.retStr();

                        var chk = (from DataRow dr in dt.Rows where dr["doccd"].retStr() == doccd && dr["autono"].retStr() == autono && (dr["STSTYPE"].retStr() == "A" || dr["STSTYPE"].retStr() == "N") select dr).Count();
                        if ((dt == null || chk == 0) && authlvl.retDbl() > 1)
                        {
                            VE.TCNTRLAUTH[p].NXTLVLSTS = "N"; VE.TCNTRLAUTH[p].PREVLVLSTS = "N";
                        }
                    }
                    else if (SHOW_RECORD.retStr() == "C")
                    {
                        VE.TCNTRLAUTH[p].DOCPASSED = "";
                    }
                    if (VE.TCNTRLAUTH[p].DOCPASSED == "A")
                    {
                        VE.TCNTRLAUTH[p].Approved = true;
                    }
                    if (VE.TCNTRLAUTH[p].DOCPASSED == "N")
                    {
                        VE.TCNTRLAUTH[p].UnApproved = true;
                    }
                    if (VE.TCNTRLAUTH[p].PREVLVLSTS == "A" && VE.TCNTRLAUTH[p].NXTLVLSTS == "N")
                    {
                        VE.TCNTRLAUTH[p].AUTH_SLNO = cnt;
                        cnt++;
                    }
                }
                VE.TCNTRLAUTH = (from a in VE.TCNTRLAUTH where a.PREVLVLSTS == "A" && a.NXTLVLSTS == "N" select a).ToList();
                VE.SHOW_RECORD = SHOW_RECORD;
                if (TAG == true)
                {
                    return VE.TCNTRLAUTH;
                }
                VE.DefaultView = true;
                ModelState.Clear();
                return PartialView("_TS_DOCAUTH_MAIN", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult SAVE(FormCollection FC, DocumentAuthorisationEntry VE)
        {

            try
            {

                for (int i = 0; i <= VE.TCNTRLAUTH.Count - 1; i++)
                {
                    if (VE.TCNTRLAUTH[i].Approved == true || VE.TCNTRLAUTH[i].UnApproved == true || VE.TCNTRLAUTH[i].Cancel == true)
                    {
                        string DOCPASSED = "",flag="";
                        if (VE.TCNTRLAUTH[i].Approved == true)
                        {
                            DOCPASSED = "A";
                            flag = "Auth";
                        }
                        else if (VE.TCNTRLAUTH[i].UnApproved == true)
                        {
                            DOCPASSED = "N";
                            flag = "UnAth";
                        }
                        else if (VE.TCNTRLAUTH[i].Cancel == true)
                        {
                            DOCPASSED = "C";
                            flag = "Cancel";
                        }
                        Cn.DocumentAuthorisation_Save(VE.TCNTRLAUTH[i].DOCCD, VE.TCNTRLAUTH[i].AUTONO, VE.TCNTRLAUTH[i].SLNO, VE.TCNTRLAUTH[i].DOCAMT, VE.TCNTRLAUTH[i].EMD_NO.retShort(), flag, VE.TCNTRLAUTH[i].AUTH_REM, Convert.ToByte(VE.TCNTRLAUTH[i].PASS_LEVEL), DOCPASSED);
                    }
                }

                ModelState.Clear();
                return Content("1");
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(CommFunc.retErrmsg(ex));
            }

        }
        public ActionResult POPUPSCREEN1(string autono)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));

                string chk_loccd = DB.T_CNTRL_HDR.Find(autono).LOCCD;
                if (CommVar.Loccd(UNQSNO) != chk_loccd)
                {
                    return Content("0");
                }
                var DOCD = DB.T_CNTRL_HDR.Find(autono);
                var Doctp = DB.M_DOCTYPE.Find(DOCD.DOCCD);
                var DCD = DB.M_DTYPE.Find(Doctp.DOCTYPE);
                string BackupDOCD = DCD.DCD;
                string autoEntryWork = "N";
                if (DCD.MENU_PROGCALL == null)
                {
                    return Content("null");
                }
                else
                {
                    string url = Cn.CreateMenuUrl(DCD.MENU_PROGCALL, DCD.DCD, DCD.MENU_PARA) + "&searchValue=" + autono + "&ThirdParty=yes~" + Doctp.DOCCD + "~" + Doctp.DOCNM.Replace("&", "$$$$$$$$") + "~" + Doctp.PRO + "~" + autoEntryWork;
                    return Content(url);
                }

                //if (DCD.MENU_PROGCALL == null)
                //{
                //    return Content("null");
                //}
                //else
                //{
                //    string sql = "select * from appl_menu where MENU_PROGCALL='" + DCD.MENU_PROGCALL + "' and MENU_PARA='" + DCD.MENU_PARA + "' and MENU_DOCCD='" + DCD.DCD + "'";
                //    DataTable tbl = masterHelp.SQLquery(sql);
                //    string MyURL = tbl.Rows[0]["MENU_PROGCALL"].ToString() + "/" + tbl.Rows[0]["MENU_PROGCALL"].ToString() + "?op=V" + "&Id=" + Cn.Encrypt_URL(tbl.Rows[0]["MENU_ID"].ToString()) + "&Index=" + Cn.Encrypt_URL(tbl.Rows[0]["MENU_INDEX"].ToString()) + "&PId=" + Cn.Encrypt_URL(tbl.Rows[0]["MENU_ID"].ToString() + "~" + tbl.Rows[0]["MENU_INDEX"].ToString() + "~" + tbl.Rows[0]["MENU_PROGCALL"].ToString()) + "&DC=" + Cn.Encrypt_URL(tbl.Rows[0]["MENU_DOCCD"].ToString()) + "&MP=" + Cn.Encrypt_URL(tbl.Rows[0]["MENU_PARA"].ToString() + "~" + tbl.Rows[0]["MENU_DATE_OPTION"].ToString() + "~" + tbl.Rows[0]["MENU_TYPE"].ToString()) + "&searchValue=" + autono + "&ThirdParty=yes~" + Doctp.DOCCD + "~" + Doctp.DOCNM.Replace("&", "$$$$$$$$") + "~" + Doctp.PRO + "~" + autoEntryWork;
                //    return Content(MyURL);
                //}
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
        [HttpPost]
        public ActionResult TS_DOCAUTH(DocumentAuthorisationEntry VE)
        {
            try
            {

                VE.TCNTRLAUTH = GetGridData(VE.FROMDT, VE.TODT, VE.SHOW_RECORD, VE.DOCTYPE.retStr(), true);
                DataTable tbl = ListToDatatable.LINQResultToDataTable(VE.TCNTRLAUTH);
                DataView dv = tbl.DefaultView;
                if (VE.SHOW_RECORD == "P")
                {
                    tbl = dv.ToTable(true, "AUTH_SLNO", "DNAME", "DOCCD", "DOCNO", "DOCDT", "USR_ID", "USR_ENTDT", "SLCD", "SLNM", "DOCAMT", "EMD_NO", "GLCD", "GLNM", "AUTHREM");
                }
                else
                {
                    tbl = dv.ToTable(true, "AUTH_SLNO", "DNAME", "DOCCD", "DOCNO", "DOCDT", "USR_ID", "USR_ENTDT", "SLCD", "SLNM", "DOCAMT", "EMD_NO", "GLCD", "GLNM", "AUTH_MNM", "AUTHREM");
                }
                //rename column name
                tbl.Columns["AUTH_SLNO"].ColumnName = "SL No";
                tbl.Columns["DNAME"].ColumnName = "Document Description";
                tbl.Columns["DOCCD"].ColumnName = "Document Type";
                tbl.Columns["DOCNO"].ColumnName = "Document No.";
                tbl.Columns["DOCDT"].ColumnName = "Document Date";
                tbl.Columns["USR_ID"].ColumnName = "Entered By";
                tbl.Columns["USR_ENTDT"].ColumnName = "Entered On";
                tbl.Columns["SLCD"].ColumnName = "SL Code";
                tbl.Columns["SLNM"].ColumnName = "SL Name";
                tbl.Columns["DOCAMT"].ColumnName = "Amount";
                tbl.Columns["EMD_NO"].ColumnName = "Update No.";
                tbl.Columns["GLCD"].ColumnName = "GL Code";
                tbl.Columns["GLNM"].ColumnName = "GL Name";
                tbl.Columns["AUTHREM"].ColumnName = "Authorization Remarks";
                if (VE.SHOW_RECORD != "P")
                {
                    tbl.Columns["AUTH_MNM"].ColumnName = "Authorised By";
                }
                tbl.AcceptChanges();


                using (ExcelPackage pck = new ExcelPackage())
                {
                    ExcelWorksheet worksheet = pck.Workbook.Worksheets.Add("Sheet1");
                    worksheet.Cells["A1"].Value = CommVar.CompName(UNQSNO) + ", " + CommVar.LocName(UNQSNO);
                    worksheet.Cells["A1"].Style.Font.Bold = true;

                    string fd = VE.FROMDT.retStr() == "" ? "" : " From " + VE.FROMDT.retDateStr();
                    string td = (VE.FROMDT.retStr() == "" && VE.TODT.retStr() == "") ? " As On " + System.DateTime.Now.retDateStr() : (VE.FROMDT.retStr() != "" && VE.TODT.retStr() != "") ? " To " + VE.TODT.retDateStr() : (VE.FROMDT.retStr() == "" && VE.TODT.retStr() != "") ? " As On " + VE.TODT.retDateStr() : " To " + System.DateTime.Now.retDateStr();

                    string hdr = VE.SHOW_RECORD == "P" ? "Pending" : VE.SHOW_RECORD == "R" ? "Rejected" : VE.SHOW_RECORD == "A" ? "Authorised" : VE.SHOW_RECORD == "C" ? "Cancel" : "";
                    worksheet.Cells["A2"].Value = hdr + " Document Details " + fd + td;
                    worksheet.Cells["A2"].Style.Font.Bold = true;
                    worksheet.Cells["A3"].LoadFromDataTable(tbl, true);
                    worksheet.Row(3).Style.Font.Bold = true;
                    //worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                    worksheet.View.FreezePanes(4, 1);
                    string filename = hdr + "_Document_Details".retRepname();
                    Byte[] fileBytes = pck.GetAsByteArray();
                    Response.ClearContent();
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", "attachment;filename=" + filename + ".xlsx");
                    Response.BinaryWrite(fileBytes);
                    Response.End();
                }

            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
            return null;
        }

    }
}