using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using Oracle.ManagedDataAccess.Client;
using System.Web.Configuration;
using System.Data;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Improvar.Controllers
{
    public class Audit_DetailsController : Controller
    {
        Connection Cn = new Connection();
        MasterHelp Master_Help = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        DataTable FinalDt = new DataTable();

        // GET: Audit_Details
        public ActionResult Audit_Details(string autono, string emdno)
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    AuditDetails VE = new AuditDetails();
                    var dt = Getdata(autono.retSqlformat(), emdno);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                        double maxemdno = (from a in DB.T_CNTRL_HDR where a.AUTONO == autono select a.EMD_NO).FirstOrDefault().retDbl();
                        if ((emdno.retDbl() - 1) != 0)
                        {
                            VE.PrevEMD_NO = (emdno.retDbl() - 1).retStr();
                        }
                        if (emdno.retDbl() != maxemdno)
                        {
                            VE.NextEMD_NO = (emdno.retDbl() + 1).retStr();
                        }
                        VE.AUTONO = autono;
                        VE.EMD_NO = emdno;
                        VE.DOCNO = dt.Rows[0]["docno"].retStr();
                        VE.DOCDT1 = dt.Rows[0]["docdt"].retDateStr();
                        VE.OldActivby = dt.Rows[0]["ActivityUserOld"].retStr();
                        VE.OldActivDate = dt.Rows[0]["ActivityDateOld"].retStr();
                        VE.OldEMDNO = dt.Rows[0]["ActivityNoOld"].retStr();
                        VE.NewActivby = dt.Rows[0]["ActivityUser"].retStr();
                        VE.NewActivDate = dt.Rows[0]["ActivityDate"].retStr();
                        VE.NewEMDNO = dt.Rows[0]["ActivityNo"].retStr();


                        VE.AuditDetailsGrid_Old = (from DataRow dr in dt.Rows
                                                   select new AuditDetailsGrid_Old()
                                                   {
                                                       ColName = dr["Colnm"].retStr(),
                                                       TableName = dr["tblnm"].retStr(),
                                                       OLD_ColValue = dr["OldVal"].retStr(),
                                                       New_ColValue = dr["CurrentVal"].retStr(),
                                                   }).DistinctBy(a => a.TableName + a.ColName + a.New_ColValue + a.OLD_ColValue).ToList();

                        VE.DefaultView = true;
                    }
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                AuditDetails VE = new AuditDetails();
                Cn.SaveException(ex, "");
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message;
                return View(VE);
            }
        }

        public DataTable Getdata(string autono, string emdno)
        {
            try
            {
                DataTable rsCols = new DataTable();
                DataTable rstbls = new DataTable();
                DataTable rsTblHdr = new DataTable();
                DataTable rsTblHdrOld = new DataTable();
                DataTable[] rs1 = new DataTable[50];
                DataTable[] rs2 = new DataTable[50];

                string STR, strT, ownr, modcd, tblnm, s_Cols, autoemd, fldnm, flddsc, diffstr, sql = "", sql1 = "", sql2 = "", sqlC = "", strInAutoNo = autono, pkey, strAuto, strAutoEmdNo;
                int tbl;
                double maxTbls;


                ownr = CommVar.CurSchema(UNQSNO); modcd = "S"; tblnm = "T_CNTRL_HDR";

                STR = "select a.owner||a.table_name owntable, a.owner, a.table_name, a.column_name, nvl(b.coldesc,a.column_name) coldesc, nvl(b.colslno,a.column_id) column_id, " + Environment.NewLine;
                STR += "a.data_type from " + Environment.NewLine;
                STR += "all_tab_cols a, " + ownr + ".m_tblcollist b " + Environment.NewLine;
                STR += "where a.owner='" + ownr + "' and a.table_name like 'T_%' and a.owner||a.table_name in (select owner||table_name from all_triggers) and " + Environment.NewLine;
                STR += "a.table_name = b.tblnm(+) and a.column_name=b.colnm(+) " + Environment.NewLine;
                STR += "order by owner, table_name, column_id ";
                rsCols = Master_Help.SQLquery(STR);

                strT = "";
                strT = "select distinct c.slno, a.owner||a.table_name owntable, a.owner, a.table_name, c.flag_f_1, c.flag_s_1, " + Environment.NewLine;
                strT += "listagg('a.'||b.column_name,',') within group (order by b.position) OVER (PARTITION BY A.TABLE_NAME, b.CONSTRAINT_NAME) as pkey_cols  " + Environment.NewLine;
                strT += "from all_triggers a, all_cons_columns b, " + ownr + ".m_tbllist c " + Environment.NewLine;
                strT += "where a.owner='" + ownr + "' and a.table_name like 'T_%' and a.table_name=c.tblnm and " + Environment.NewLine;
                strT += "a.owner||a.table_name = b.owner||b.table_name and b.constraint_name like 'PKEY_%' " + Environment.NewLine;
                strT += "order by owner, slno, table_name ";
                rstbls = Master_Help.SQLquery(strT);



                maxTbls = rstbls.Rows.Count;

                if (rstbls.Rows.Count == 0) return null;
                tbl = 1;
                Int32 a = 0;
                while (a <= maxTbls - 1)
                {
                    tblnm = rstbls.Rows[a]["table_name"].retStr();
                    ownr = CommVar.CurSchema(UNQSNO);
                    s_Cols = "";
                    if (rsCols.Rows.Count != 0)
                    {
                        var temprsCols = rsCols.Select("owntable = '" + ownr + tblnm + "'");

                        int b = 0;
                        while (b <= temprsCols.Count() - 1)
                        {
                            while (temprsCols[b]["owntable"].retStr() == ownr + tblnm)
                            {
                                if (temprsCols[b]["data_type"].retStr() == "NUMBER")
                                {
                                    s_Cols = s_Cols + (s_Cols != "" ? "," : "") + "nvl(a." + temprsCols[b]["column_name"].retStr() + ",0)" + temprsCols[b]["column_name"].retStr();
                                }
                                else
                                {
                                    s_Cols = s_Cols + (s_Cols != "" ? "," : "") + "a." + temprsCols[b]["column_name"].retStr();
                                }
                                b++;
                                if (b > temprsCols.Count() - 1) break;
                            }
                            if (b > temprsCols.Count() - 1) break;
                        }
                    }
                    for (int i = 0; i <= 1; i++)
                    {
                        if (i == 0)
                        {
                            sqlC = "";
                            sqlC += "and nvl(b.emd_no,0)=" + emdno;
                        }
                        else
                        {
                            sqlC = "";
                            sqlC += "and nvl(b.emd_no,0)=" + (emdno.retDbl() - 1);
                        }
                        STR = "";
                        STR += "select a.autono||nvl(a.emd_no,0) autoemdno, " + rstbls.Rows[a]["pkey_cols"].retStr().Replace(",", "||") + " pkeycols, " + Environment.NewLine;
                        STR += "a.autono||nvl(a.emd_no,0)||" + rstbls.Rows[a]["pkey_cols"].retStr().Replace(",", "||") + " autopkey, " + s_Cols + " " + Environment.NewLine;
                        STR += "from " + ownr + "." + tblnm + " a, " + ownr + ".t_cntrl_hdr b " + Environment.NewLine;
                        STR += "where a.autono||a.emd_no= b.autono||b.emd_no and " + Environment.NewLine;
                        STR += "nvl(a.dtag,'') <> 'D' " + Environment.NewLine;
                        STR += "and a.autono in (" + strInAutoNo + ") " + Environment.NewLine;
                        STR += sqlC;
                        STR += " union " + Environment.NewLine;
                        STR += "select a.autono||nvl(a.emd_no,0) autoemdno, " + (rstbls.Rows[a]["pkey_cols"].retStr().Replace(",", "||")) + " pkeycols, " + Environment.NewLine;
                        STR += "a.autono || nvl(a.emd_no, 0) || " + (rstbls.Rows[a]["pkey_cols"].retStr().Replace(",", " ||")) + " autopkey, " + s_Cols + " " + Environment.NewLine;
                        STR += "from " + ownr + "." + tblnm + "T" + " a, " + ownr + ".t_cntrl_hdrt b " + Environment.NewLine;
                        STR += "where a.autono||a.emd_no= b.autono||b.emd_no  " + Environment.NewLine;
                        STR += "and a.autono in (" + strInAutoNo + ") " + Environment.NewLine;
                        STR += sqlC;
                        STR += "and (nvl(a.dtag,'') <> 'D' " + Environment.NewLine;

                        if (i == 1)
                        {
                            STR += "or b.lm_usr_entdt is null " + Environment.NewLine;
                            //if (strInAutoNo != "")
                            //{
                            //    STR += "AND a.autono in (" + strInAutoNo + ") " + Environment.NewLine;
                            //}
                        }
                        STR += ") ";
                        // STR += "order by autono, emd_no " + Environment.NewLine;
                        if (i == 0)
                        {
                            sql1 = STR + Environment.NewLine;
                            //sqlC = "";
                            //sqlC += "select a.autono " + Environment.NewLine;
                            //sqlC += "from " + ownr + "." + tblnm + " a, " + ownr + ".t_cntrl_hdr b " + Environment.NewLine;
                            //sqlC += "where a.autono||a.emd_no= b.autono||b.emd_no and " + Environment.NewLine;
                            //sqlC += "nvl(a.dtag,'') <> 'D' " + Environment.NewLine;
                            //sqlC += "and a.autono in (" + strInAutoNo + ") " + Environment.NewLine;
                            //sqlC += " union " + Environment.NewLine;
                            //sqlC += "select a.autono " + Environment.NewLine;
                            //sqlC += "from " + ownr + "." + tblnm + "T" + " a, " + ownr + ".t_cntrl_hdrt b ," + ownr + ".t_cntrl_hdr c " + Environment.NewLine;
                            //sqlC += "where a.autono||a.emd_no= b.autono||b.emd_no and " + Environment.NewLine;
                            //sqlC += "nvl(a.dtag,'') <> 'D' " + Environment.NewLine;
                            //sqlC += "and a.autono in (" + strInAutoNo + ") " + Environment.NewLine;

                        }
                        else {
                            sql2 = STR;
                        }

                        if (i == 0)
                        {
                            rs1[tbl] = Master_Help.SQLquery(sql1);
                            if (tbl == 1)
                            {
                                if (rs1[tbl].Rows.Count != 0)
                                {
                                    int c = 0;
                                    while (c <= rs1[tbl].Rows.Count - 1)
                                    {
                                        strInAutoNo = strInAutoNo + (strInAutoNo != "" ? "," : "") + "'" + rs1[tbl].Rows[c]["AUTONO"] + "'";
                                        c++;
                                    }
                                }
                            }
                            //else {
                            //    rs2[tbl] = Master_Help.SQLquery(sql2);
                            //}
                        }
                        else {
                            rs2[tbl] = Master_Help.SQLquery(sql2);
                        }
                    }

                    a++;
                    if (tbl == 1)
                    {
                        sql = "select distinct autono, emd_no, autoemdno, docdt, docno, lm_usr_id, lm_usr_entdt from ( " + sql1 + " ) order by autono ";
                        rsTblHdr = Master_Help.SQLquery(sql);

                        sql = "select distinct autono, emd_no, autoemdno, docdt, docno, lm_usr_id, lm_usr_entdt,usr_id,usr_entdt from ( " + sql2 + " ) order by autono ";
                        rsTblHdrOld = Master_Help.SQLquery(sql);
                    }
                    tbl = tbl + 1;
                }

                if (rsTblHdr.Rows.Count != 0)
                {
                    DTGen();
                    int d = 0;
                    while (d <= rsTblHdr.Rows.Count - 1)
                    {
                        strAuto = rsTblHdr.Rows[d]["AUTONO"].retStr();
                        strAutoEmdNo = rsTblHdr.Rows[d]["AUTOEMDNO"].retStr();
                        tbl = 1;

                        //diffstr = "Doc No. " + rsTblHdr.Rows[d]["docno"] + "[ " + rsTblHdr.Rows[d]["emd_no"].retStr() + "   " + rsTblHdr.Rows[d]["AUTONO"] + "] dtd. " + rsTblHdr.Rows[d]["DOCDT"].retDateStr() + "<br/>";
                        //diffstr = diffstr + "Modify by : " + rsTblHdr.Rows[d]["lm_usr_id"] + " at " + rsTblHdr.Rows[d]["lm_usr_entdt"] + "<br/>";
                        int e = 0;
                        while (e <= rstbls.Rows.Count - 1)
                        {
                            tblnm = rstbls.Rows[e]["table_name"].retStr();
                            //current data respect checking
                            if (rs1[tbl].Rows.Count != 0)
                            {
                                var temprs1 = rs1[tbl].Select("AUTOEMDNO = '" + strAutoEmdNo + "'");
                                if (temprs1.Count() != 0)
                                {
                                    int f = 0;
                                    while (f <= temprs1.Count() - 1)
                                    {
                                        while (temprs1[f]["AUTOEMDNO"].retStr() == strAutoEmdNo)
                                        {
                                            autoemd = temprs1[f]["AUTONO"] + (temprs1[f]["emd_no"].retDbl() - 1).retStr();
                                            //string pkey_cols = (rstbls.Rows[e]["pkey_cols"].retStr().Replace("a.", "").Replace(",", "||"));
                                            pkey = temprs1[f]["pkeycols"].retStr();
                                            while (temprs1[f]["AUTOEMDNO"].retStr() == strAutoEmdNo && temprs1[f]["AUTOPKEY"].retStr() == strAutoEmdNo + pkey)
                                            {
                                                var temprs2 = rs2[tbl].Select("autoemdno='" + autoemd + "'  and  pkeycols  = '" + pkey + "' and autopkey='" + autoemd + pkey + "' ");
                                                if (rsCols.Rows.Count != 0)
                                                {
                                                    var temprsCols = rsCols.Select("owntable = '" + ownr + tblnm + "'");
                                                    if (temprsCols.Count() != 0)
                                                    {
                                                        //diffstr = diffstr + "Table : " + tblnm + "   Key : " + pkey + "<br/>";
                                                        int g = 0;
                                                        while (g <= temprsCols.Count() - 1)
                                                        {
                                                            while (temprsCols[g]["owntable"].retStr() == ownr + tblnm)
                                                            {

                                                                fldnm = temprsCols[g]["column_name"].retStr();
                                                                flddsc = temprsCols[g]["coldesc"].retStr();
                                                                bool insrecord = false;
                                                                if (temprs2.Count() == 0)
                                                                {
                                                                    if (temprs1[f][fldnm].retStr() != "0" && temprs1[f][fldnm].retStr() != "") insrecord = true;
                                                                }
                                                                else if (temprs1[f][fldnm].retStr() != temprs2[0][fldnm].retStr()) insrecord = true;

                                                                if (insrecord == true)
                                                                {
                                                                    DataRow ROWDATA = FinalDt.NewRow();
                                                                    ROWDATA["autono"] = rsTblHdr.Rows[d]["autono"];
                                                                    ROWDATA["docno"] = rsTblHdr.Rows[d]["docno"];
                                                                    ROWDATA["docdt"] = rsTblHdr.Rows[d]["docdt"];
                                                                    ROWDATA["ActivityUser"] = rsTblHdr.Rows[d]["LM_USR_ID"];
                                                                    ROWDATA["ActivityDate"] = rsTblHdr.Rows[d]["LM_USR_ENTDT"];
                                                                    ROWDATA["ActivityNo"] = rsTblHdr.Rows[d]["EMD_NO"];

                                                                    ROWDATA["ActivityUserOld"] = rsTblHdrOld.Rows[d]["LM_USR_ID"].retStr() == "" ? rsTblHdrOld.Rows[d]["usr_id"].retStr() : rsTblHdrOld.Rows[d]["LM_USR_ID"].retStr();
                                                                    ROWDATA["ActivityDateOld"] = rsTblHdrOld.Rows[d]["LM_USR_ENTDT"].retStr() == "" ? rsTblHdrOld.Rows[d]["usr_entdt"].retStr() : rsTblHdrOld.Rows[d]["LM_USR_ENTDT"].retStr();
                                                                    ROWDATA["ActivityNoOld"] = rsTblHdrOld.Rows[d]["EMD_NO"];

                                                                    ROWDATA["tblnm"] = tblnm;
                                                                    ROWDATA["Colnm"] = flddsc;
                                                                    ROWDATA["CurrentVal"] = temprs1[f][fldnm].retStr();
                                                                    ROWDATA["OldVal"] = temprs2.Count() == 0 ? "" : temprs2[0][fldnm].retStr();
                                                                    FinalDt.Rows.Add(ROWDATA);
                                                                    //diffstr = diffstr + flddsc + " : " + temprs1[f][fldnm].retStr() + "  [" + temprs2[0][fldnm].retStr() + " ]" + "<br/>";
                                                                }
                                                                g++;
                                                                if (g > temprsCols.Count() - 1) break;
                                                            }
                                                            if (g > temprsCols.Count() - 1) break;
                                                        }
                                                    }
                                                }
                                                f++;
                                                if (f > temprs1.Count() - 1) break;
                                            }
                                            if (f > temprs1.Count() - 1) break;
                                        }
                                        if (f > temprs1.Count() - 1) break;
                                    }
                                }
                            }
                            //end current data respect checking

                            //old data respect checking
                            DataTable tempolddata = new DataTable();
                            if (rs2[tbl].Rows.Count != 0 && rs1[tbl].Rows.Count == 0)
                            {
                                tempolddata = rs2[tbl];
                            }
                            else
                            {
                                var data = rs2[tbl].AsEnumerable().Where(x => !rs1[tbl].AsEnumerable().Any(a1 => a1.Field<string>("pkeycols") == x.Field<string>("pkeycols")));
                                if (data != null && data.Count() > 0)
                                {
                                    tempolddata = data.CopyToDataTable();
                                }
                                else
                                {
                                    tempolddata = rs2[tbl].Clone();
                                }
                            }

                            for (int xx = 0; xx <= tempolddata.Rows.Count - 1; xx++)
                            {
                                if (rsCols.Rows.Count != 0)
                                {
                                    var temprsCols = rsCols.Select("owntable = '" + ownr + tblnm + "'");
                                    if (temprsCols.Count() != 0)
                                    {
                                        //diffstr = diffstr + "Table : " + tblnm + "   Key : " + pkey + "<br/>";
                                        int g = 0;
                                        while (g <= temprsCols.Count() - 1)
                                        {
                                            while (temprsCols[g]["owntable"].retStr() == ownr + tblnm)
                                            {
                                                fldnm = temprsCols[g]["column_name"].retStr();
                                                flddsc = temprsCols[g]["coldesc"].retStr();
                                                if (tempolddata.Rows[xx][fldnm].retStr() != "" && tempolddata.Rows[xx][fldnm].retStr() != "0")
                                                {
                                                    DataRow ROWDATA = FinalDt.NewRow();
                                                    ROWDATA["autono"] = rsTblHdr.Rows[d]["autono"];
                                                    ROWDATA["docno"] = rsTblHdr.Rows[d]["docno"];
                                                    ROWDATA["docdt"] = rsTblHdr.Rows[d]["docdt"];
                                                    ROWDATA["ActivityUser"] = rsTblHdr.Rows[d]["LM_USR_ID"];
                                                    ROWDATA["ActivityDate"] = rsTblHdr.Rows[d]["LM_USR_ENTDT"];
                                                    ROWDATA["ActivityNo"] = rsTblHdr.Rows[d]["EMD_NO"];

                                                    ROWDATA["ActivityUserOld"] = rsTblHdrOld.Rows[d]["LM_USR_ID"].retStr() == "" ? rsTblHdrOld.Rows[d]["usr_id"].retStr() : rsTblHdrOld.Rows[d]["LM_USR_ID"].retStr();
                                                    ROWDATA["ActivityDateOld"] = rsTblHdrOld.Rows[d]["LM_USR_ENTDT"].retStr() == "" ? rsTblHdrOld.Rows[d]["usr_entdt"].retStr() : rsTblHdrOld.Rows[d]["LM_USR_ENTDT"].retStr();
                                                    ROWDATA["ActivityNoOld"] = rsTblHdrOld.Rows[d]["EMD_NO"];

                                                    ROWDATA["tblnm"] = tblnm;
                                                    ROWDATA["Colnm"] = flddsc;
                                                    ROWDATA["CurrentVal"] = "";
                                                    ROWDATA["OldVal"] = tempolddata.Rows[xx][fldnm].retStr();
                                                    FinalDt.Rows.Add(ROWDATA);
                                                }
                                                g++;
                                                if (g > temprsCols.Count() - 1) break;
                                            }
                                            if (g > temprsCols.Count() - 1) break;
                                        }
                                    }
                                }
                            }
                            //end old data respect checking

                            tbl = tbl + 1;
                            e++;
                        }
                        d++;
                    }
                }
                return FinalDt;
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return null;
            }
        }
        private void DTGen()
        {
            FinalDt.Columns.Add("autono", typeof(string));
            FinalDt.Columns.Add("docno", typeof(string));
            FinalDt.Columns.Add("docdt", typeof(string));
            FinalDt.Columns.Add("ActivityUser", typeof(string));
            FinalDt.Columns.Add("ActivityDate", typeof(string));
            FinalDt.Columns.Add("ActivityNo", typeof(string));
            FinalDt.Columns.Add("ActivityUserOld", typeof(string));
            FinalDt.Columns.Add("ActivityDateOld", typeof(string));
            FinalDt.Columns.Add("ActivityNoOld", typeof(string));
            FinalDt.Columns.Add("tblnm", typeof(string));
            FinalDt.Columns.Add("Colnm", typeof(string));
            FinalDt.Columns.Add("CurrentVal", typeof(string));
            FinalDt.Columns.Add("OldVal", typeof(string));
        }

    }
}