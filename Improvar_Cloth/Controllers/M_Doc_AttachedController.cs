using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;

namespace Improvar.Controllers
{
    public class M_Doc_AttachedController : Controller
    {
        // GET: M_Doc_Attached
        string CS = null;
        Connection Cn = new Connection();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        MasterHelp masterHelp = new MasterHelp();
        public ActionResult M_Doc_Attached()
        {
            Trans_document_Attach VE = new Trans_document_Attach();
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Document Attach"; Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), CommVar.CommSchema());
                    VE.DocumentType = (from i in DB1.MS_DOCCTG select new DocumentType() { text = i.DOC_CTG, value = i.DOC_CTG }).OrderBy(s => s.text).ToList();
                    VE.DefaultView = true;
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                VE.msg = ex.Message;
                VE.DefaultView = false;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }

        public ActionResult GetDOCUhelp(string val)
        {
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            if (val == null)
            {
                return PartialView("_Help2", masterHelp.DocumentTypeCollection(DBF));
            }
            else
            {
                var query = (from c in DBF.M_DOCTYPE where (c.DOCCD == val) select c);
                if (query.Any())
                {
                    string str = "";
                    foreach (var i in query)
                    {
                        str = i.DOCCD + Cn.GCS() + i.DOCNM;
                    }
                    return Content(str);
                }
                else
                {
                    return Content("0");
                }
            }
        }

        public ActionResult GetTransaction(string category, string dtype, string fdt, string tdt, string fddt, string tddt, string userid)
        {
            try
            {
                fdt = Convert.ToString(Convert.ToDateTime(fdt)).Substring(0, 10);
                tdt = Convert.ToString(Convert.ToDateTime(tdt)).Substring(0, 10);
                fddt = Convert.ToString(Convert.ToDateTime(fddt)).Substring(0, 10);
                tddt = Convert.ToString(Convert.ToDateTime(tddt)).Substring(0, 10);
                Trans_document_Attach TDA = new Trans_document_Attach();
                string[] categoryM = category.Split(',');
                string scm1 = CommVar.CurSchema(UNQSNO);
                string scmf = (string)Session["FINSCHEMA"];
                string sql = "";
                sql += "select distinct a.autono, a.docno, to_char(a.docdt,'DD/MM/YYYY')docdt, a.slcd, a.slnm, a.doconlyno, a.docamt, ";
                sql += "b.doc_ctg  from ";
                sql += "( select a.autono, a.docno, a.docdt, a.slcd, c.slnm, a.doconlyno, TO_CHAR(a.docamt,'99,99,99,999.99')docamt ";
                sql += "from " + scm1 + ".t_cntrl_hdr a, " + scm1 + ".t_cntrl_hdr_doc b, " + scmf + ".m_subleg c ";
                sql += "where a.autono=b.autono(+) and a.slcd=c.slcd(+) and ";
                if (fddt != "") sql += "a.docdt >= to_date('" + fddt + "','dd/mm/yyyy') and ";
                if (tddt != "") sql += "a.docdt <= to_date('" + tddt + "', 'dd/mm/yyyy') and ";
                if (fdt != "") sql += "a.usr_entdt >= to_date('" + fdt + "','dd/mm/yyyy') and ";
                if (tdt != "") sql += "a.usr_entdt <= to_date('" + tdt + "','dd/mm/yyyy') and ";
                if (userid != "") sql += "a.usr_id='" + userid + "' and ";
                sql += "a.doccd='" + dtype + "' and a.compcd='" + CommVar.Compcd(UNQSNO) + "' and a.loccd='" + CommVar.Loccd(UNQSNO) + "' ) a, ";
                sql += "( select autono,listagg (doc_ctg, ',') WITHIN GROUP (ORDER BY doc_ctg) doc_ctg ";
                sql += "from " + scm1 + ".t_cntrl_hdr_doc ";
                sql += "group by autono) b ";
                sql += "where a.autono=b.autono(+) order by to_date(docdt,'dd/mm/yyyy')";
                DataTable tbl = masterHelp.SQLquery(sql);
                List<BulkDocumentAttached> BDA = new List<BulkDocumentAttached>();
                for (int i = 0; i <= tbl.Rows.Count - 1; i++)
                {
                    BulkDocumentAttached temp = new BulkDocumentAttached();
                    temp.amount = tbl.Rows[i]["docamt"].ToString();
                    temp.autono = tbl.Rows[i]["autono"].ToString();
                    temp.category = tbl.Rows[i]["doc_ctg"].ToString();
                    temp.DDate = tbl.Rows[i]["docdt"].ToString();
                    temp.Docno = tbl.Rows[i]["docno"].ToString();
                    temp.slcd = tbl.Rows[i]["slcd"].ToString();
                    temp.slnm = tbl.Rows[i]["slnm"].ToString();
                    temp.refno = tbl.Rows[i]["doconlyno"].ToString();
                    for (int j = 0; j <= categoryM.Length - 1; j++)
                    {
                        string dd = "Cate" + j.ToString();
                        string cd = "doccat" + j.ToString();
                        Type typeclass = Type.GetType("Improvar.Models." + dd);
                        var helpM1 = Activator.CreateInstance(typeclass);
                        helpM1.GetType().GetProperty(cd).SetValue(helpM1, categoryM[j]);
                        temp.GetType().GetProperty(dd).SetValue(temp, helpM1);
                    }
                    BDA.Add(temp);
                }
                TDA.GridData = BDA;
                TDA.UserSelectedCategory = category;
                return PartialView("_BulkDocumentAttached", TDA);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }

        public ActionResult POPUPSCREEN1(string autono)
        {
            try
            {
                Trans_document_Attach VE = new Trans_document_Attach();
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                var DOCD = DB.T_CNTRL_HDR.Find(autono);
                var Doctp = DB.M_DOCTYPE.Find(DOCD.DOCCD);
                var DCD = DB.M_DTYPE.Find(Doctp.DOCTYPE);
                string BackupDOCD = DCD.DCD;
                string autoEntryWork = "Y";
                //if (DCD.MENU_PROGCALL == null)
                //{
                //    autoEntryWork = "Y";
                //    DCD.MENU_PROGCALL = "T_Voucher";
                //    DCD.MENU_PARA = "MAIN";
                //    DCD.DCD = "JOURNAL";
                //}


                //
                //string sql = "select * from appl_menu where MENU_PROGCALL='" + DCD.MENU_PROGCALL + "' and MENU_PARA='" + DCD.MENU_PARA + "' and MENU_DOCCD='" + Doctp.DOCTYPE + "'";
                //DataTable tbl = masterHelp.SQLquery(sql);
                //string MNUDET = tbl.Rows[0]["MENU_ID"].ToString() + "~"
                //                 + tbl.Rows[0]["MENU_INDEX"].ToString() + "~"
                //                 + tbl.Rows[0]["MENU_PROGCALL"].ToString() + "~"
                //                 + tbl.Rows[0]["menu_date_option"].ToString() + "~"
                //                 + tbl.Rows[0]["menu_type"].ToString() + "~"
                //                 + "0" + "~"
                //                 + "0" + "~"
                //                 + "AEV";
                //string US = VE.UNQSNO_ENCRYPTED;
                //string DC = tbl.Rows[0]["MENU_DOCCD"].ToString();
                //string MP = tbl.Rows[0]["MENU_PARA"].ToString();

                //string MyURL = tbl.Rows[0]["MENU_PROGCALL"].ToString() + "/" + tbl.Rows[0]["MENU_PROGCALL"].ToString() + "?op=V" + "&MNUDET="
                //    + Cn.Encrypt_URL(MNUDET) + "&US=" + US
                //    + "&DC=" + Cn.Encrypt_URL(DC)
                //    + "&MP=" + Cn.Encrypt_URL(MP) + "&searchValue=" + AUTONO
                //    + "&ThirdParty=yes~" + Doctp.DOCCD + "~" + Doctp.DOCNM.Replace("&", "$$$$$$$$") + "~" + Doctp.PRO + "~" + autoEntryWork;
                //

                string sql = "select * from appl_menu where MENU_PROGCALL='" + DCD.MENU_PROGCALL + "' and MENU_PARA='" + DCD.MENU_PARA + "' and MENU_DOCCD='" + DCD.DCD + "'";
                DataTable tbl = masterHelp.SQLquery(sql);
                string MNUDET = tbl.Rows[0]["MENU_ID"].ToString() + "~"
                                 + tbl.Rows[0]["MENU_INDEX"].ToString() + "~"
                                 + tbl.Rows[0]["MENU_PROGCALL"].ToString() + "~"
                                 + tbl.Rows[0]["menu_date_option"].ToString() + "~"
                                 + tbl.Rows[0]["menu_type"].ToString() + "~"
                                 + "0" + "~"
                                 + "0" + "~"
                                 + "AEV";
                string US = VE.UNQSNO_ENCRYPTED;
                //string MyURL = tbl.Rows[0]["MENU_PROGCALL"].ToString() + "/" + tbl.Rows[0]["MENU_PROGCALL"].ToString() + "?op=V" + "&MNUDET="
                //    + Cn.Encrypt_URL(MNUDET) + "&US=" + US + "&Id=" + Cn.Encrypt_URL(tbl.Rows[0]["MENU_ID"].ToString()) + "&Index=" + Cn.Encrypt_URL(tbl.Rows[0]["MENU_INDEX"].ToString()) + "&PId=" + Cn.Encrypt_URL(tbl.Rows[0]["MENU_ID"].ToString() + "~" + tbl.Rows[0]["MENU_INDEX"].ToString() + "~" + tbl.Rows[0]["MENU_PROGCALL"].ToString()) + "&DC=" + Cn.Encrypt_URL(tbl.Rows[0]["MENU_DOCCD"].ToString()) + "&MP=" + Cn.Encrypt_URL(tbl.Rows[0]["MENU_PARA"].ToString() + "~" + tbl.Rows[0]["MENU_DATE_OPTION"].ToString() + "~" + tbl.Rows[0]["MENU_TYPE"].ToString()) + "&searchValue=" + autono + "&ThirdParty=yes~" + Doctp.DOCCD + "~" + Doctp.DOCNM.Replace("&", "$$$$$$$$") + "~" + Doctp.PRO + "~" + autoEntryWork;

                string MyURL = tbl.Rows[0]["MENU_PROGCALL"].ToString() + "/" + tbl.Rows[0]["MENU_PROGCALL"].ToString() + "?op=V" + "&MNUDET="
                   + Cn.Encrypt_URL(MNUDET) + "&US=" + US + "&Id=" + Cn.Encrypt_URL(tbl.Rows[0]["MENU_ID"].ToString()) + "&Index=" + Cn.Encrypt_URL(tbl.Rows[0]["MENU_INDEX"].ToString()) + "&PId=" + Cn.Encrypt_URL(tbl.Rows[0]["MENU_ID"].ToString() + "~" + tbl.Rows[0]["MENU_INDEX"].ToString() + "~" + tbl.Rows[0]["MENU_PROGCALL"].ToString()) + "&DC=" + Cn.Encrypt_URL(tbl.Rows[0]["MENU_DOCCD"].ToString()) + "&MP=" + Cn.Encrypt_URL(tbl.Rows[0]["MENU_PARA"].ToString()) + "&searchValue=" + autono + "&ThirdParty=yes~" + Doctp.DOCCD + "~" + Doctp.DOCNM.Replace("&", "$$$$$$$$") + "~" + Doctp.PRO + "~" + autoEntryWork;
                return Content(MyURL);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
        [HttpPost]
        public ActionResult M_Doc_Attached(Trans_document_Attach TDA, FormCollection FC)
        {
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), CommVar.CommSchema());
            TDA.DocumentType = (from i in DB1.MS_DOCCTG select new DocumentType() { text = i.DOC_CTG, value = i.DOC_CTG }).OrderBy(s => s.text).ToList();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    if (TDA.GridData != null)// add DOCUMENT
                    {
                        int Flag = 0;
                        foreach (var i in TDA.GridData)
                        {
                            List<UploadDOC> UploadDOC1 = new List<Models.UploadDOC>();
                            if (i.Cate0 != null)
                            {
                                UploadDOC ULD = new UploadDOC();
                                ULD.DocumentType = TDA.DocumentType;
                                ULD.DOC_DESC = "";
                                ULD.DOC_FILE = i.Cate0.DOC_FILE_NAME0;
                                ULD.DOC_FILE_NAME = i.Cate0.DOC_FILE0;
                                ULD.docID = i.Cate0.doccat0;
                                if (ULD.DOC_FILE != null)
                                {
                                    UploadDOC1.Add(ULD);
                                    Flag = 1;
                                }
                            }
                            if (i.Cate1 != null)
                            {
                                UploadDOC ULD = new UploadDOC();
                                ULD.DocumentType = TDA.DocumentType;
                                ULD.DOC_DESC = "";
                                ULD.DOC_FILE = i.Cate1.DOC_FILE_NAME1;
                                ULD.DOC_FILE_NAME = i.Cate1.DOC_FILE1;
                                ULD.docID = i.Cate1.doccat1;
                                if (ULD.DOC_FILE != null)
                                {
                                    UploadDOC1.Add(ULD);
                                    Flag = 1;
                                }
                            }
                            if (i.Cate2 != null)
                            {
                                UploadDOC ULD = new UploadDOC();
                                ULD.DocumentType = TDA.DocumentType;
                                ULD.DOC_DESC = "";
                                ULD.DOC_FILE = i.Cate2.DOC_FILE_NAME2;
                                ULD.DOC_FILE_NAME = i.Cate2.DOC_FILE2;
                                ULD.docID = i.Cate2.doccat2;
                                if (ULD.DOC_FILE != null)
                                {
                                    UploadDOC1.Add(ULD);
                                    Flag = 1;
                                }
                            }
                            if (i.Cate3 != null)
                            {
                                UploadDOC ULD = new UploadDOC();
                                ULD.DocumentType = TDA.DocumentType;
                                ULD.DOC_DESC = "";
                                ULD.DOC_FILE = i.Cate3.DOC_FILE_NAME3;
                                ULD.DOC_FILE_NAME = i.Cate3.DOC_FILE3;
                                ULD.docID = i.Cate3.doccat3;
                                if (ULD.DOC_FILE != null)
                                {
                                    UploadDOC1.Add(ULD);
                                    Flag = 1;
                                }
                            }
                            if (i.Cate4 != null)
                            {
                                UploadDOC ULD = new UploadDOC();
                                ULD.DocumentType = TDA.DocumentType;
                                ULD.DOC_DESC = "";
                                ULD.DOC_FILE = i.Cate4.DOC_FILE_NAME4;
                                ULD.DOC_FILE_NAME = i.Cate4.DOC_FILE4;
                                ULD.docID = i.Cate4.doccat4;
                                if (ULD.DOC_FILE != null)
                                {
                                    UploadDOC1.Add(ULD);
                                    Flag = 1;
                                }
                            }
                            if (i.Cate5 != null)
                            {
                                UploadDOC ULD = new UploadDOC();
                                ULD.DocumentType = TDA.DocumentType;
                                ULD.DOC_DESC = "";
                                ULD.DOC_FILE = i.Cate5.DOC_FILE_NAME5;
                                ULD.DOC_FILE_NAME = i.Cate5.DOC_FILE5;
                                ULD.docID = i.Cate5.doccat5;
                                if (ULD.DOC_FILE != null)
                                {
                                    UploadDOC1.Add(ULD);
                                    Flag = 1;
                                }
                            }
                            if (i.Cate6 != null)
                            {
                                UploadDOC ULD = new UploadDOC();
                                ULD.DocumentType = TDA.DocumentType;
                                ULD.DOC_DESC = "";
                                ULD.DOC_FILE = i.Cate6.DOC_FILE_NAME6;
                                ULD.DOC_FILE_NAME = i.Cate6.DOC_FILE6;
                                ULD.docID = i.Cate6.doccat6;
                                if (ULD.DOC_FILE != null)
                                {
                                    UploadDOC1.Add(ULD);
                                    Flag = 1;
                                }
                            }
                            if (i.Cate7 != null)
                            {
                                UploadDOC ULD = new UploadDOC();
                                ULD.DocumentType = TDA.DocumentType;
                                ULD.DOC_DESC = "";
                                ULD.DOC_FILE = i.Cate7.DOC_FILE_NAME7;
                                ULD.DOC_FILE_NAME = i.Cate7.DOC_FILE7;
                                ULD.docID = i.Cate7.doccat7;
                                if (ULD.DOC_FILE != null)
                                {
                                    UploadDOC1.Add(ULD);
                                    Flag = 1;
                                }
                            }
                            if (i.Cate8 != null)
                            {
                                UploadDOC ULD = new UploadDOC();
                                ULD.DocumentType = TDA.DocumentType;
                                ULD.DOC_DESC = "";
                                ULD.DOC_FILE = i.Cate8.DOC_FILE_NAME8;
                                ULD.DOC_FILE_NAME = i.Cate8.DOC_FILE8;
                                ULD.docID = i.Cate8.doccat8;
                                if (ULD.DOC_FILE != null)
                                {
                                    UploadDOC1.Add(ULD);
                                    Flag = 1;
                                }
                            }
                            if (i.Cate9 != null)
                            {
                                UploadDOC ULD = new UploadDOC();
                                ULD.DocumentType = TDA.DocumentType;
                                ULD.DOC_DESC = "";
                                ULD.DOC_FILE = i.Cate9.DOC_FILE_NAME9;
                                ULD.DOC_FILE_NAME = i.Cate9.DOC_FILE9;
                                ULD.docID = i.Cate9.doccat9;
                                if (ULD.DOC_FILE != null)
                                {
                                    UploadDOC1.Add(ULD);
                                    Flag = 1;
                                }
                            }
                            if (Flag == 1)
                            {
                                var img = Cn.SaveUploadImageTransaction(UploadDOC1, i.autono, 0);
                                if (img.Item1.Count != 0)
                                {
                                    int maxslno = (from ii in DB.T_CNTRL_HDR_DOC where (ii.AUTONO == i.autono) select ii.SLNO).DefaultIfEmpty().Max();
                                    int maxsl = (from ii in DB.T_CNTRL_HDR_DOC where (ii.AUTONO == i.autono) select ii.SLNO).DefaultIfEmpty().Max();
                                    //maxsl += 1;
                                    //img.Item1.ForEach(a => a.SLNO = Convert.ToByte(maxsl));
                                     //maxslno = maxsl + 1;
                                    foreach (var j in img.Item1)
                                    {
                                        maxsl += 1;
                                        j.SLNO = Convert.ToByte(maxsl);
                                    }
                                    //img.Item2.ForEach(a => a.SLNO = Convert.ToByte(maxsl));
                                    foreach (var j in img.Item2)
                                    {
                                        maxslno += 1;
                                        j.SLNO = Convert.ToByte(maxslno);
                                    }
                                    DB.T_CNTRL_HDR_DOC.AddRange(img.Item1);
                                    DB.T_CNTRL_HDR_DOC_DTL.AddRange(img.Item2);
                                }
                            }
                        }
                        if (Flag == 1)
                        {
                            DB.SaveChanges();
                            ModelState.Clear();
                            transaction.Commit();
                            TDA.msg = "Document Upload Sucessfully";
                        }
                        else
                        {
                            ModelState.Clear();
                            TDA.msg = "Oops ! Upload Document Not Found";
                        }
                    }
                    TDA.DefaultView = true;
                    TDA.docTypeID = "";
                    TDA.docTypeName = "";
                    return View(TDA);
                }
                catch (Exception ex)
                {
                    TDA.DefaultView = false;
                    TDA.msg = ex.Message;
                    transaction.Rollback();
                    ModelState.Clear();
                    Cn.SaveException(ex, "");
                    return View(TDA);
                }
            }
        }
    }
}