using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;
using Microsoft.Ajax.Utilities;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;
using System.IO;

namespace Improvar
{
    public class Cloth
    {
        // GET: T_SALE_POS_POS
        Connection Cn = new Connection(); MasterHelp masterHelp = new MasterHelp();
        Salesfunc salesfunc = new Salesfunc(); DataTable DTNEW = new DataTable();
        EmailControl EmailControl = new EmailControl();
        SMS SMS = new SMS();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public TransactionSalePosEntry T_SALE_POS_Navigation(TransactionSalePosEntry VE, ImprovarDB DB, int index, string searchValue, string loadOrder = "N")
        {
            try
            {
                ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                string COM_CD = CommVar.Compcd(UNQSNO);
                string LOC_CD = CommVar.Loccd(UNQSNO);
                string DATABASE = CommVar.CurSchema(UNQSNO).ToString();
                string DATABASEF = CommVar.FinSchema(UNQSNO);
                Cn.getQueryString(VE);
                string scmf = CommVar.FinSchema(UNQSNO); string scm = CommVar.CurSchema(UNQSNO);

                if (VE.IndexKey.Count != 0 || loadOrder.retStr().Length > 1)
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
                    VE.T_TXN = DB.T_TXN.Find(aa[0].Trim());
                    VE.T_CNTRL_HDR = DB.T_CNTRL_HDR.Find(VE.T_TXN.AUTONO);
                    if (VE.T_TXN.ROYN == "Y") VE.RoundOff = true;
                    else VE.RoundOff = false;
                    VE.T_TXNTRANS = DB.T_TXNTRANS.Find(VE.T_TXN.AUTONO);
                    VE.T_TXNOTH = DB.T_TXNOTH.Find(VE.T_TXN.AUTONO);
                    VE.T_TXNMEMO = DB.T_TXNMEMO.Find(VE.T_TXN.AUTONO);
                   
                    
                    //var MGP = DB.M_GROUP.Find(   VE.T_TXN.ITGRPCD);

                    VE.T_TXN_LINKNO = (from a in DB.T_TXN_LINKNO where a.AUTONO == VE.T_TXN.AUTONO select a).FirstOrDefault();
                    //VE.LINKDOCNO = (from a in DB.T_CNTRL_HDR where a.AUTONO == TTXNLINKNO.LINKAUTONO select a).FirstOrDefault().DOCNO;


                    VE.RTDEBNM = VE.T_TXNMEMO.RTDEBCD.retStr() == "" ? "" : DBF.M_RETDEB.Where(a => a.RTDEBCD == VE.T_TXNMEMO.RTDEBCD).Select(b => b.RTDEBNM).FirstOrDefault();
                    VE.MOBILE = VE.T_TXNMEMO.RTDEBCD.retStr() == "" ? "" : DBF.M_RETDEB.Where(a => a.RTDEBCD == VE.T_TXNMEMO.RTDEBCD).Select(b => b.MOBILE).FirstOrDefault();
                    var add1 = VE.T_TXNMEMO.RTDEBCD.retStr() == "" ? "" : DBF.M_RETDEB.Where(a => a.RTDEBCD == VE.T_TXNMEMO.RTDEBCD).Select(b => b.ADD1).FirstOrDefault();
                    var add2 = VE.T_TXNMEMO.RTDEBCD.retStr() == "" ? "" : DBF.M_RETDEB.Where(a => a.RTDEBCD == VE.T_TXNMEMO.RTDEBCD).Select(b => b.ADD2).FirstOrDefault();
                    var add3 = VE.T_TXNMEMO.RTDEBCD.retStr() == "" ? "" : DBF.M_RETDEB.Where(a => a.RTDEBCD == VE.T_TXNMEMO.RTDEBCD).Select(b => b.ADD3).FirstOrDefault();
                    var city = VE.T_TXNMEMO.RTDEBCD.retStr() == "" ? "" : DBF.M_RETDEB.Where(a => a.RTDEBCD == VE.T_TXNMEMO.RTDEBCD).Select(b => b.CITY).FirstOrDefault();
                    VE.ADDR = add1 +" "+ add2 +" "+ add3+"/"+ city;
                   
                  
                    string sql1 = "";
                    sql1 += " select a.rtdebcd,b.rtdebnm,b.mobile,a.inc_rate,C.TAXGRPCD,a.retdebslcd,b.city,b.add1,b.add2,b.add3 ";
                    sql1 += "  from  " + scm + ".M_SYSCNFG a, " + scmf + ".M_RETDEB b, " + scm + ".M_SUBLEG_SDDTL c";
                    sql1 += " where a.RTDEBCD=b.RTDEBCD and a.effdt in(select max(effdt) effdt from  " + scm + ".M_SYSCNFG) and a.retdebslcd=C.SLCD";
                    DataTable syscnfgdt = masterHelp.SQLquery(sql1);
                    if (syscnfgdt != null && syscnfgdt.Rows.Count > 0)
                    { VE.RETDEBSLCD = syscnfgdt.Rows[0]["retdebslcd"].retStr(); }

                    VE.INC_RATE = VE.T_TXN.INCL_RATE == "Y".retStr() ? true : false;
                    VE.INCLRATEASK = VE.T_TXN.INCL_RATE.retStr();
                    VE.AGSLNM = VE.T_TXNOTH.AGSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == VE.T_TXNOTH.AGSLCD).Select(b => b.SLNM).FirstOrDefault();
                    VE.SAGSLNM = VE.T_TXNOTH.SAGSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == VE.T_TXNOTH.SAGSLCD).Select(b => b.SLNM).FirstOrDefault();
                    VE.GONM = VE.T_TXN.GOCD.retStr() == "" ? "" : DB.M_GODOWN.Where(a => a.GOCD == VE.T_TXN.GOCD).Select(b => b.GONM).FirstOrDefault();
                    //VE.GOCD = VE.T_TXN.GOCD.retStr() == "" ? "" : VE.T_TXN.GOCD;
                    VE.PRCNM = VE.T_TXNOTH.PRCCD.retStr() == "" ? "" : DBF.M_PRCLST.Where(a => a.PRCCD == VE.T_TXNOTH.PRCCD).Select(b => b.PRCNM).FirstOrDefault();
                    VE.GSTNO = VE.T_TXN.AUTONO.retStr() == "" ? "" : DBF.T_VCH_GST.Where(a => a.AUTONO == VE.T_TXN.AUTONO).Select(b => b.GSTNO).FirstOrDefault();
                    VE.GSTSLNM = VE.T_TXN.AUTONO.retStr() == "" ? "" : DBF.T_VCH_GST.Where(a => a.AUTONO == VE.T_TXN.AUTONO).Select(b => b.GSTSLNM).FirstOrDefault();
                    VE.GSTSLADD1 = VE.T_TXN.AUTONO.retStr() == "" ? "" : DBF.T_VCH_GST.Where(a => a.AUTONO == VE.T_TXN.AUTONO).Select(b => b.GSTSLADD1).FirstOrDefault();
                    VE.GSTSLDIST = VE.T_TXN.AUTONO.retStr() == "" ? "" : DBF.T_VCH_GST.Where(a => a.AUTONO == VE.T_TXN.AUTONO).Select(b => b.GSTSLDIST).FirstOrDefault();
                    VE.GSTSLPIN = VE.T_TXN.AUTONO.retStr() == "" ? "" : DBF.T_VCH_GST.Where(a => a.AUTONO == VE.T_TXN.AUTONO).Select(b => b.GSTSLPIN).FirstOrDefault();

                    VE.T_CNTRL_HDR_REM = Cn.GetTransactionReamrks(CommVar.CurSchema(UNQSNO).ToString(), VE.T_TXN.AUTONO);
                    VE.UploadDOC = Cn.GetUploadImageTransaction(CommVar.CurSchema(UNQSNO).ToString(), VE.T_TXN.AUTONO);

                    string Scm = CommVar.CurSchema(UNQSNO);
                    string str1 = "";
                    str1 += "select i.autono, i.SLNO,i.TXNSLNO,k.ITGRPCD,n.ITGRPNM,n.BARGENTYPE,i.MTRLJOBCD,o.MTRLJOBNM,o.MTBARCODE,k.ITCD,k.ITNM,k.UOMCD,k.STYLENO,i.PARTCD,p.PARTNM,p.PRTBARCODE,j.STKTYPE,q.STKNAME,i.BARNO,i.INCLRATE,i.PCSACTION,i.ORDSLNO,i.ORDAUTONO,r.PRCCD,s.docno ORDNO,s.docdt ORDDT,  ";
                    str1 += "j.COLRCD,m.COLRNM,m.CLRBARCODE,j.SIZECD,l.SIZENM,l.SZBARCODE,i.SHADE,i.QNTY,i.NOS,i.RATE,i.DISCRATE,i.DISCTYPE,i.TDDISCRATE,i.TDDISCTYPE,i.SCMDISCTYPE,i.SCMDISCRATE,i.HSNCODE,i.BALENO,j.PDESIGN,j.OURDESIGN,i.FLAGMTR,i.LOCABIN,i.BALEYR ";
                    str1 += ",n.SALGLCD,n.PURGLCD,n.SALRETGLCD,n.PURRETGLCD,j.WPRATE,j.RPRATE,i.ITREM ,r.DISCAMT,r.TDDISCAMT,r.SCMDISCAMT,r.TXBLVAL,r.IGSTPER,r.CGSTPER,r.SGSTPER,r.CESSPER,r.IGSTAMT,r.CGSTAMT,r.SGSTAMT,r.CESSAMT,r.NETAMT,r.TOTDISCAMT ,r.AMT,r.BLQNTY ";
                    str1 += "from " + Scm + ".T_BATCHDTL i, " + Scm + ".T_BATCHMST j, " + Scm + ".M_SITEM k, " + Scm + ".M_SIZE l, " + Scm + ".M_COLOR m, ";
                    str1 += Scm + ".M_GROUP n," + Scm + ".M_MTRLJOBMST o," + Scm + ".M_PARTS p," + Scm + ".M_STKTYPE q ," + Scm + ".T_TXNDTL r," + Scm + ".T_CNTRL_HDR s ";
                    str1 += "where i.AUTONO = r.AUTONO(+) and i.BARNO = j.BARNO(+) and j.ITCD = k.ITCD(+) and j.SIZECD = l.SIZECD(+) and j.COLRCD = m.COLRCD(+) and k.ITGRPCD=n.ITGRPCD(+) ";
                    str1 += "and i.MTRLJOBCD=o.MTRLJOBCD(+) and i.PARTCD=p.PARTCD(+) and j.STKTYPE=q.STKTYPE(+) and i.ORDAUTONO = s.AUTONO(+) ";
                    str1 += "and i.AUTONO = '" + VE.T_TXN.AUTONO + "'";
                    str1 += "order by i.SLNO ";
                    DataTable tbl = masterHelp.SQLquery(str1);

                    VE.TsalePos_TBATCHDTL = (from DataRow dr in tbl.Rows
                                             select new TsalePos_TBATCHDTL()
                                             {
                                                 SLNO = dr["SLNO"].retShort(),
                                                 TXNSLNO = dr["TXNSLNO"].retShort(),
                                                 ITGRPCD = dr["ITGRPCD"].retStr(),
                                                 ITGRPNM = dr["ITGRPNM"].retStr(),
                                                 MTRLJOBCD = dr["MTRLJOBCD"].retStr(),
                                                 MTRLJOBNM = dr["MTRLJOBNM"].retStr(),
                                                 MTBARCODE = dr["MTBARCODE"].retStr(),
                                                 ITCD = dr["ITCD"].retStr(),
                                                 ITSTYLE = dr["STYLENO"].retStr() + "" + dr["ITNM"].retStr(),
                                                 UOM = dr["UOMCD"].retStr(),
                                                 STYLENO = dr["STYLENO"].retStr(),
                                                 PARTCD = dr["PARTCD"].retStr(),
                                                 PARTNM = dr["PARTNM"].retStr(),
                                                 PRTBARCODE = dr["PRTBARCODE"].retStr(),
                                                 COLRCD = dr["COLRCD"].retStr(),
                                                 COLRNM = dr["COLRNM"].retStr(),
                                                 CLRBARCODE = dr["CLRBARCODE"].retStr(),
                                                 SIZECD = dr["SIZECD"].retStr(),
                                                 SIZENM = dr["SIZENM"].retStr(),
                                                 SZBARCODE = dr["SZBARCODE"].retStr(),
                                                 SHADE = dr["SHADE"].retStr(),
                                                 QNTY = dr["QNTY"].retDbl(),
                                                 NOS = dr["NOS"].retDbl(),
                                                 RATE = dr["RATE"].retDbl(),
                                                 DISCRATE = dr["DISCRATE"].retDbl(),
                                                 DISCTYPE = dr["DISCTYPE"].retStr(),
                                                 DISCTYPE_DESC = dr["DISCTYPE"].retStr() == "P" ? "%" : dr["DISCTYPE"].retStr() == "N" ? "Nos" : dr["DISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
                                                 DISCAMT = dr["DISCAMT"].retDbl(),
                                                 TDDISCRATE = dr["TDDISCRATE"].retDbl(),
                                                 TDDISCTYPE_DESC = dr["TDDISCTYPE"].retStr() == "P" ? "%" : dr["TDDISCTYPE"].retStr() == "N" ? "Nos" : dr["TDDISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
                                                 TDDISCTYPE = dr["TDDISCTYPE"].retStr(),
                                                 SCMDISCTYPE_DESC = dr["SCMDISCTYPE"].retStr() == "P" ? "%" : dr["SCMDISCTYPE"].retStr() == "N" ? "Nos" : dr["SCMDISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
                                                 SCMDISCTYPE = dr["SCMDISCTYPE"].retStr(),
                                                 SCMDISCRATE = dr["SCMDISCRATE"].retDbl(),
                                                 STKTYPE = dr["STKTYPE"].retStr(),
                                                 STKNAME = dr["STKNAME"].retStr(),
                                                 BARNO = dr["BARNO"].retStr(),
                                                 HSNCODE = dr["HSNCODE"].retStr(),
                                                 PDESIGN = dr["PDESIGN"].retStr(),
                                                 OURDESIGN = dr["OURDESIGN"].retStr(),
                                                 FLAGMTR = dr["FLAGMTR"].retDbl(),
                                                 LOCABIN = dr["LOCABIN"].retStr(),
                                                 BALEYR = dr["BALEYR"].retStr(),
                                                 BARGENTYPE = dr["BARGENTYPE"].retStr(),
                                                 GLCD = dr["SALGLCD"].retStr(),
                                                 ITREM = dr["ITREM"].retStr(),
                                                 TXBLVAL = dr["TXBLVAL"].retDbl(),
                                                 IGSTPER = dr["IGSTPER"].retDbl(),
                                                 CGSTPER = dr["CGSTPER"].retDbl(),
                                                 SGSTPER = dr["SGSTPER"].retDbl(),
                                                 CESSPER = dr["CESSPER"].retDbl(),
                                                 IGSTAMT = dr["IGSTAMT"].retDbl(),
                                                 CGSTAMT = dr["CGSTAMT"].retDbl(),
                                                 SGSTAMT = dr["SGSTAMT"].retDbl(),
                                                 CESSAMT = dr["CESSAMT"].retDbl(),
                                                 NETAMT = dr["NETAMT"].retDbl(),
                                                 TOTDISCAMT = dr["TOTDISCAMT"].retDbl(),
                                                 GROSSAMT = dr["AMT"].retDbl(),
                                                 INCLRATE = dr["INCLRATE"].retDbl(),
                                                 PCSACTION = dr["PCSACTION"].retStr(),
                                                 ORDAUTONO = dr["ORDAUTONO"].retStr(),
                                                 ORDSLNO = dr["ORDSLNO"].retStr(),
                                                 ORDDOCNO = dr["ORDNO"].retStr(),
                                                 ORDDOCDT = dr["ORDDT"].retDateStr(),
                                             }).OrderBy(s => s.SLNO).DistinctBy(s=>s.SLNO).ToList();
                
                        
                    //str1 = "";
                    //str1 += "select i.SLNO,j.ITGRPCD,k.ITGRPNM,i.MTRLJOBCD,l.MTRLJOBNM,l.MTBARCODE,i.ITCD,j.ITNM,j.STYLENO,j.UOMCD,i.STKTYPE,m.STKNAME,i.NOS,i.QNTY,i.FLAGMTR, ";
                    //str1 += "i.BLQNTY,i.RATE,i.AMT,i.DISCTYPE,i.DISCRATE,i.DISCAMT,i.TDDISCTYPE,i.TDDISCRATE,i.TDDISCAMT,i.SCMDISCTYPE,i.SCMDISCRATE,i.SCMDISCAMT, ";
                    //str1 += "i.TXBLVAL,i.IGSTPER,i.CGSTPER,i.SGSTPER,i.CESSPER,i.IGSTAMT,i.CGSTAMT,i.SGSTAMT,i.CESSAMT,i.NETAMT,i.HSNCODE,i.BALENO,i.GLCD,i.BALEYR,i.TOTDISCAMT,i.ITREM ";
                    //str1 += "from " + Scm + ".T_TXNDTL i, " + Scm + ".M_SITEM j, " + Scm + ".M_GROUP k, " + Scm + ".M_MTRLJOBMST l, " + Scm + ".M_STKTYPE m ";
                    //str1 += "where i.ITCD = j.ITCD(+) and j.ITGRPCD=k.ITGRPCD(+) and i.MTRLJOBCD=l.MTRLJOBCD(+)  and i.STKTYPE=m.STKTYPE(+)  ";
                    //str1 += "and i.AUTONO = '" +    VE.T_TXN.AUTONO + "' ";
                    //str1 += "order by i.SLNO ";
                    //tbl = masterHelp.SQLquery(str1);

                    //VE.TTXNDTL = (from DataRow dr in tbl.Rows
                    //              select new TTXNDTL()
                    //              {
                    //                  SLNO = dr["SLNO"].retShort(),
                    //                  ITGRPCD = dr["ITGRPCD"].retStr(),
                    //                  ITGRPNM = dr["ITGRPNM"].retStr(),
                    //                  MTRLJOBCD = dr["MTRLJOBCD"].retStr(),
                    //                  MTRLJOBNM = dr["MTRLJOBNM"].retStr(),
                    //                  //   MTBARCODE = dr["MTBARCODE"].retStr(),
                    //                  ITCD = dr["ITCD"].retStr(),
                    //                  ITSTYLE = dr["STYLENO"].retStr() + "" + dr["ITNM"].retStr(),
                    //                  UOM = dr["UOMCD"].retStr(),
                    //                  STKTYPE = dr["STKTYPE"].retStr(),
                    //                  STKNAME = dr["STKNAME"].retStr(),
                    //                  NOS = dr["NOS"].retDbl(),
                    //                  QNTY = dr["QNTY"].retDbl(),
                    //                  FLAGMTR = dr["FLAGMTR"].retDbl(),
                    //                  BLQNTY = dr["BLQNTY"].retDbl(),
                    //                  RATE = dr["RATE"].retDbl(),
                    //                  AMT = dr["AMT"].retDbl(),
                    //                  DISCTYPE = dr["DISCTYPE"].retStr(),
                    //                  DISCTYPE_DESC = dr["DISCTYPE"].retStr() == "P" ? "%" : dr["DISCTYPE"].retStr() == "N" ? "Nos" : dr["DISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
                    //                  DISCRATE = dr["DISCRATE"].retDbl(),
                    //                  DISCAMT = dr["DISCAMT"].retDbl(),
                    //                  TDDISCTYPE_DESC = dr["TDDISCTYPE"].retStr() == "P" ? "%" : dr["TDDISCTYPE"].retStr() == "N" ? "Nos" : dr["TDDISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
                    //                  TDDISCTYPE = dr["TDDISCTYPE"].retStr(),
                    //                  TDDISCRATE = dr["TDDISCRATE"].retDbl(),
                    //                  TDDISCAMT = dr["TDDISCAMT"].retDbl(),
                    //                  SCMDISCTYPE_DESC = dr["SCMDISCTYPE"].retStr() == "P" ? "%" : dr["SCMDISCTYPE"].retStr() == "N" ? "Nos" : dr["SCMDISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
                    //                  SCMDISCTYPE = dr["SCMDISCTYPE"].retStr(),
                    //                  SCMDISCRATE = dr["SCMDISCRATE"].retDbl(),
                    //                  SCMDISCAMT = dr["SCMDISCAMT"].retDbl(),
                    //                  TXBLVAL = dr["TXBLVAL"].retDbl(),
                    //                  IGSTPER = dr["IGSTPER"].retDbl(),
                    //                  CGSTPER = dr["CGSTPER"].retDbl(),
                    //                  SGSTPER = dr["SGSTPER"].retDbl(),
                    //                  CESSPER = dr["CESSPER"].retDbl(),
                    //                  IGSTAMT = dr["IGSTAMT"].retDbl(),
                    //                  CGSTAMT = dr["CGSTAMT"].retDbl(),
                    //                  SGSTAMT = dr["SGSTAMT"].retDbl(),
                    //                  CESSAMT = dr["CESSAMT"].retDbl(),
                    //                  NETAMT = dr["NETAMT"].retDbl(),
                    //                  HSNCODE = dr["HSNCODE"].retStr(),
                    //                  BALENO = dr["BALENO"].retStr(),
                    //                  GLCD = dr["GLCD"].retStr(),
                    //                  BALEYR = dr["BALEYR"].retStr(),
                    //                  TOTDISCAMT = dr["TOTDISCAMT"].retDbl(),
                    //                  ITREM = dr["ITREM"].retStr(),
                    //              }).OrderBy(s => s.SLNO).ToList();

                    VE.B_T_QNTY = VE.TsalePos_TBATCHDTL.Sum(a => a.QNTY).retDbl();
                    VE.B_T_NOS = VE.TsalePos_TBATCHDTL.Sum(a => a.NOS).retDbl();
                    VE.T_NOS = VE.TsalePos_TBATCHDTL.Sum(a => a.NOS).retDbl();
                    VE.T_QNTY = VE.TsalePos_TBATCHDTL.Sum(a => a.QNTY).retDbl();
                    VE.T_AMT = VE.TsalePos_TBATCHDTL.Sum(a => a.TXBLVAL).retDbl();
                    VE.T_B_GROSSAMT = VE.TsalePos_TBATCHDTL.Sum(a => a.TXBLVAL).retDbl();
                    VE.T_IGST_AMT = VE.TsalePos_TBATCHDTL.Sum(a => a.IGSTAMT).retDbl();
                    VE.T_CGST_AMT = VE.TsalePos_TBATCHDTL.Sum(a => a.CGSTAMT).retDbl();
                    VE.T_SGST_AMT = VE.TsalePos_TBATCHDTL.Sum(a => a.SGSTAMT).retDbl();
                    VE.T_CESS_AMT = VE.TsalePos_TBATCHDTL.Sum(a => a.CESSAMT).retDbl();
                    VE.B_T_NET = VE.TsalePos_TBATCHDTL.Sum(a => a.NETAMT).retDbl();
                    VE.T_NET_AMT = VE.TsalePos_TBATCHDTL.Sum(a => a.NETAMT).retDbl();

                    //var RETAMT = document.getElementById("RETAMT").value;
                    //if (RETAMT == "") { RETAMT = parseFloat(0); } else { RETAMT = parseFloat(RETAMT) }
                    //var T_PYMT_AMT = document.getElementById("T_PYMT_AMT").value;
                    //if (T_PYMT_AMT == "") { T_PYMT_AMT = parseFloat(0); } else { T_PYMT_AMT = parseFloat(T_PYMT_AMT) }
                    //var PAYABLE = document.getElementById("PAYABLE").value;
                    //if (PAYABLE == "") { PAYABLE = parseFloat(0); } else { PAYABLE = parseFloat(PAYABLE) }
                    //var PAYAMT = document.getElementById("PAYAMT").value;
                    //if (PAYAMT == "") { PAYAMT = parseFloat(0); } else { PAYAMT = parseFloat(PAYAMT) }
                    VE.RETAMT = VE.RETAMT.retStr()==""?0: VE.RETAMT.retDbl().toRound(2);
                    VE.PAYABLE = (VE.T_TXN.BLAMT - VE.RETAMT).retDbl().toRound(2);
                  
                    // fill prodgrpgstper in t_batchdtl
                    DataTable allprodgrpgstper_data = new DataTable();
                    string BARNO = (from a in VE.TsalePos_TBATCHDTL select a.BARNO).ToArray().retSqlfromStrarray();
                    string ITCD = (from a in VE.TsalePos_TBATCHDTL select a.ITCD).ToArray().retSqlfromStrarray();
                    string MTRLJOBCD = (from a in VE.TsalePos_TBATCHDTL select a.MTRLJOBCD).ToArray().retSqlfromStrarray();
                    string ITGRPCD = (from a in VE.TsalePos_TBATCHDTL select a.ITGRPCD).ToArray().retSqlfromStrarray();
                    
                    allprodgrpgstper_data = salesfunc.GetStock(VE.T_TXN.DOCDT.retStr().Remove(10), VE.T_TXN.GOCD.retSqlformat(), BARNO.retStr(), ITCD.retStr(), MTRLJOBCD.retStr(), VE.T_CNTRL_HDR_REM.AUTONO.retSqlformat(), ITGRPCD, "", VE.T_TXNOTH.PRCCD.retStr(), VE.T_TXNOTH.TAXGRPCD.retStr());

                    foreach (var v in VE.TsalePos_TBATCHDTL)
                    {
                        v.DISC_TYPE = masterHelp.DISC_TYPE();
                        v.PCSActionList = masterHelp.PCSAction();
                        string PRODGRPGSTPER = "", ALL_GSTPER = "", GSTPER = "";
                        v.GSTPER = VE.TsalePos_TBATCHDTL.Where(a => a.SLNO == v.TXNSLNO).Sum(b => b.IGSTPER + b.CGSTPER + b.SGSTPER).retDbl();
                        if (allprodgrpgstper_data != null && allprodgrpgstper_data.Rows.Count > 0)
                        {
                            var q = (from DataRow dr in allprodgrpgstper_data.Rows
                                          select new
                                          { BALSTOCK = dr["BALQNTY"].retDbl() }).FirstOrDefault();
                            v.BALSTOCK = q.BALSTOCK;
                            var DATA = allprodgrpgstper_data.Select("barno = '" + v.BARNO + "' and itcd= '" + v.ITCD + "' and itgrpcd = '" + v.ITGRPCD + "' " );
                            if (DATA.Count() > 0)
                            {
                                DataTable tax_data = DATA.CopyToDataTable();
                                if (tax_data != null && tax_data.Rows.Count > 0)
                                {
                                    PRODGRPGSTPER = tax_data.Rows[0]["PRODGRPGSTPER"].retStr();
                                    if (PRODGRPGSTPER != "")
                                    {
                                        ALL_GSTPER = salesfunc.retGstPer(PRODGRPGSTPER, v.RATE.retDbl());
                                        if (ALL_GSTPER.retStr() != "")
                                        {
                                            var gst = ALL_GSTPER.Split(',').ToList();
                                            GSTPER = (from a in gst select a.retDbl()).Sum().retStr();
                                        }
                                        v.PRODGRPGSTPER = PRODGRPGSTPER;
                                        v.GSTPER = GSTPER.retDbl();
                                    }
                                    if (tax_data.Rows[0]["barimage"].retStr() != "")
                                    {
                                        v.BarImages = tax_data.Rows[0]["barimage"].retStr();
                                        var brimgs = v.BarImages.retStr().Split((char)179);
                                        v.BarImagesCount = brimgs.Length == 0 ? "" : brimgs.Length.retStr();
                                        foreach (var barimg in brimgs)
                                        {
                                            string barfilename = barimg.Split('~')[0];
                                            string FROMpath = CommVar.SaveFolderPath() + "/ItemImages/" + barfilename;
                                            FROMpath = Path.Combine(FROMpath, "");
                                            string TOPATH = System.Web.Hosting.HostingEnvironment.MapPath("/UploadDocuments/" + barfilename);
                                            Cn.CopyImage(FROMpath, TOPATH);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    // fill prodgrpgstper in t_txndtl
                    foreach (var v in VE.TsalePos_TBATCHDTL)
                    {
                        string PRODGRPGSTPER = "";
                        var tax_data = (from a in VE.TsalePos_TBATCHDTL
                                        where a.TXNSLNO == v.SLNO && a.ITGRPCD == v.ITGRPCD && a.ITCD == a.ITCD && a.STKTYPE == v.STKTYPE
                                        && a.RATE == v.RATE && a.DISCTYPE == v.DISCTYPE && a.DISCRATE == v.DISCRATE && a.TDDISCTYPE == v.TDDISCTYPE
                                         && a.TDDISCRATE == v.TDDISCRATE && a.SCMDISCTYPE == v.SCMDISCTYPE && a.SCMDISCRATE == v.SCMDISCRATE
                                        select new { a.PRODGRPGSTPER }).FirstOrDefault();
                        if (tax_data != null)
                        {
                            PRODGRPGSTPER = tax_data.PRODGRPGSTPER.retStr();
                        }
                        v.PRODGRPGSTPER = PRODGRPGSTPER;                   
                    }             
                 
                    // total amt
                    VE.TOTNOS = VE.T_NOS.retDbl();
                    VE.TOTQNTY = VE.T_QNTY.retDbl();
                    VE.TOTTAXVAL = VE.T_B_GROSSAMT.retDbl() + VE.A_T_AMOUNT.retDbl();
                    VE.TOTTAX = VE.T_CGST_AMT.retDbl() + VE.A_T_CGST.retDbl() + VE.T_SGST_AMT.retDbl() + VE.A_T_SGST.retDbl() + VE.T_IGST_AMT.retDbl() + VE.A_T_IGST.retDbl();

                    string str = "select a.SLMSLCD,b.SLNM,a.PER,a.ITAMT,a.BLAMT from " + Scm + ".t_txnslsmn a," + scmf + ".m_subleg b ";
                    str += "where a.SLMSLCD=b.slcd and a.autono='" + VE.T_TXN.AUTONO + "'";
                    var SALSMN_DATA = masterHelp.SQLquery(str);
                    VE.TTXNSLSMN = (from DataRow dr in SALSMN_DATA.Rows
                                    select new TTXNSLSMN()
                                    {
                                        SLMSLCD = dr["SLMSLCD"].retStr(),
                                        SLMSLNM = dr["SLNM"].retStr(),
                                        PER = dr["PER"].retDbl(),
                                        ITAMT = dr["ITAMT"].retDbl(),
                                        BLAMT = dr["BLAMT"].retDbl(),
                                    }).ToList();
                    double S_T_GROSS_AMT = 0; double T_BILL_AMT = 0;
                     //c
                    for (int p = 0; p <= VE.TTXNSLSMN.Count - 1; p++)
                    {
                        VE.TTXNSLSMN[p].SLNO = (p + 1).retShort();

                        S_T_GROSS_AMT = S_T_GROSS_AMT + VE.TTXNSLSMN[p].ITAMT.Value;
                        T_BILL_AMT = T_BILL_AMT + VE.TTXNSLSMN[p].BLAMT.Value;
                    }
                    VE.T_ITAMT = S_T_GROSS_AMT.retDbl();
                    VE.T_BLAMT = T_BILL_AMT.retDbl();

                    string str2 = "select a.SLNO,a.PYMTCD,c.PYMTNM,a.AMT,a.CARDNO,a.INSTNO,a.INSTDT,a.PYMTREM,a.GLCD from " + Scm + ".t_txnpymt a," + Scm + ".t_txnpymt_hdr b," + Scm + ".m_payment c ";
                    str2 += "where a.autono=b.autono and  a.PYMTCD=c.PYMTCD and a.autono='" + VE.T_TXN.AUTONO + "'";
                    var PYMT_DATA = masterHelp.SQLquery(str2);
                    VE.TTXNPYMT = (from DataRow dr in PYMT_DATA.Rows
                                   select new TTXNPYMT()
                                   {
                                       SLNO = dr["SLNO"].retShort(),
                                       PYMTCD = dr["PYMTCD"].retStr(),
                                       PYMTNM = dr["PYMTNM"].retStr(),
                                       AMT = dr["AMT"].retDbl(),
                                       CARDNO = dr["CARDNO"].retStr(),
                                       INSTNO = dr["INSTNO"].retStr(),
                                       INSTDT = dr["INSTDT"].retDateStr(),
                                       PYMTREM = dr["PYMTREM"].retStr(),
                                       GLCD = dr["GLCD"].retStr(),
                                   }).ToList();
                    double T_PYMT_AMT = 0;

                    for (int p = 0; p <= VE.TTXNPYMT.Count - 1; p++)
                    {
                        T_PYMT_AMT = T_PYMT_AMT + VE.TTXNPYMT[p].AMT.Value;

                    }
                    VE.T_PYMT_AMT = T_PYMT_AMT;
                    VE.PAYAMT = T_PYMT_AMT.toRound(2);
                    VE.NETDUE = (VE.PAYABLE - VE.PAYAMT).retDbl().toRound(2);
                    if (VE.T_CNTRL_HDR.CANCEL == "Y") VE.CancelRecord = true; else VE.CancelRecord = false;
                }
                //Cn.DateLock_Entry(VE, DB,   VE.T_CNTRL_HDR.DOCDT.Value);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                throw ex;
            }
            return VE;
        }
        public string T_SALE_POS_Save(FormCollection FC, TransactionSalePosEntry VE)
        {
            Cn.getQueryString(VE);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            //  Oracle Queries
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            OracleTransaction OraTrans;
            string dbsql = "";
            string[] dbsql1;
            string dberrmsg = "";

            OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted);
            OraCmd.Transaction = OraTrans;
            DB.Configuration.ValidateOnSaveEnabled = false;
            try
            {
                DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".T_CNTRL_HDR in  row share mode");
                OraCmd.CommandText = "lock table " + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode"; OraCmd.ExecuteNonQuery();

                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                string ContentFlg = "";
                if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                {
                    //checking barcode &txndtl pge itcd wise qnty, nos should match

                    //var barcodedata = (from x in VE.TsalePos_TBATCHDTL
                    //                   group x by new { x.ITCD } into P
                    //                   select new
                    //                   {
                    //                       ITCD = P.Key.ITCD,
                    //                       QTY = P.Sum(A => A.QNTY),
                    //                       NOS = P.Sum(A => A.NOS)
                    //                   }).Where(a => a.QTY != 0).ToList();
                    //var txndtldata = (from x in VE.TTXNDTL
                    //                  group x by new { x.ITCD } into P
                    //                  select new
                    //                  {
                    //                      ITCD = P.Key.ITCD,
                    //                      QTY = P.Sum(A => A.QNTY),
                    //                      NOS = P.Sum(A => A.NOS)
                    //                  }).Where(a => a.QTY != 0).ToList();

                    //        var difList = barcodedata.Where(a => !txndtldata.Any(a1 => a1.ITCD == a.ITCD && a1.QTY == a.QTY && a1.NOS == a.NOS))
                    //.Union(txndtldata.Where(a => !barcodedata.Any(a1 => a1.ITCD == a.ITCD && a1.QTY == a.QTY && a1.NOS == a.NOS))).ToList();
                    //var difList = barcodedata.Where(a => !txndtldata.Any(a1 => a1.ITCD == a.ITCD && a1.QTY == a.QTY && a1.NOS == a.NOS)).ToList();

                    //if (difList != null && difList.Count > 0)
                    //{
                    //    OraTrans.Rollback();
                    //    OraCon.Dispose();
                    //    return Content("Barcode grid itcd wise qnty, nos should match !!");
                    //}

                    T_TXN TTXN = new T_TXN();
                    T_TXNMEMO TTXNMEMO = new T_TXNMEMO();
                    T_TXNPYMT_HDR TTXNPYMTHDR = new T_TXNPYMT_HDR();
                    T_TXNOTH TTXNOTH = new T_TXNOTH();
                    T_TXN_LINKNO TTXNLINKNO = new T_TXN_LINKNO();
                    T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                    T_VCH_GST TVCHGST = new T_VCH_GST();
                    T_CNTRL_DOC_PASS TCDP = new T_CNTRL_DOC_PASS();
                    string DOCPATTERN = "";
                    string docpassrem = "";
                    bool blactpost = true, blgstpost = true;
                    TTXN.DOCDT = VE.T_TXN.DOCDT;
                    string Ddate = Convert.ToString(TTXN.DOCDT);
                    TTXN.CLCD = CommVar.ClientCode(UNQSNO);
                    string auto_no = ""; string Month = "";

                    #region Posting to finance Preparation
                    int COUNTER = 0;
                    string expglcd = "";
                    string stkdrcr = "C", strqty = " Q=" + VE.TOTQNTY.toRound(2).ToString();
                    string trcd = "TR";
                    string revcharge = "";
                    string dr = ""; string cr = ""; int isl = 0; string strrem = "";
                    double igst = 0; double cgst = 0; double sgst = 0; double cess = 0; double duty = 0; double dbqty = 0; double dbamt = 0; double dbcurramt = 0;
                    double dbDrAmt = 0, dbCrAmt = 0;
                    blactpost = true; blgstpost = true;
                    /* string parglcd = "saldebglcd"*/
                    string parglcd = "", parclass1cd = "",class2cd="", tcsgl="", prodglcd="", rogl="",glcd="", slmslcd="";
                    string strblno = "", strbldt = "", strduedt = "", strrefno = "", strvtype = "BL";
                    dr = "D"; cr = "C";
                    string sslcd = TTXN.SLCD;
                    if (VE.PSLCD.retStr() != "") sslcd = VE.PSLCD.ToString();
                    if(VE.TTXNSLSMN != null)
                    { if (VE.TTXNSLSMN[0].SLMSLCD.retStr() != "") slmslcd = VE.TTXNSLSMN[0].SLMSLCD.retStr(); }
                    if (VE.TsalePos_TBATCHDTL[0].GLCD.retStr() != "") glcd = VE.TsalePos_TBATCHDTL[0].GLCD.retStr();
                   
                    switch (VE.MENU_PARA)
                    {
                        case "SBCM":
                            stkdrcr = "C"; trcd = "SB"; strrem = "Cash Memo" + strqty; break;
                        case "SBCMR":
                            stkdrcr = "D"; dr = "C"; cr = "D"; trcd = "SC"; strrem = "Cash Memo Credit Note" + strqty; break;
                    }

                    string slcdlink = "", slcdpara = VE.MENU_PARA;
                    //if (VE.MENU_PARA == "PR") slcdpara = "PB";
                    string sql = "";
                    sql = "select class1cd, saldebglcd  glcd from " + CommVar.CurSchema(UNQSNO) + ".m_syscnfg ";
                    DataTable tblsys = masterHelp.SQLquery(sql);
                    if (tblsys.Rows.Count == 0)
                    {
                        dberrmsg = "Debtor/Creditor code not setup"; goto dbnotsave;
                    }

                    sql = "select b.rogl, b.tcsgl, a.class1cd, null class2cd, nvl(c.crlimit,0) crlimit, nvl(c.crdays,0) crdays, ";
                    sql += "'" + glcd + "' prodglcd, ";
                    //if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "PR") sql += "b.igst_p igstcd, b.cgst_p cgstcd, b.sgst_p sgstcd, b.cess_p cesscd, b.duty_p dutycd, ";
                    sql += "b.igst_s igstcd, b.cgst_s cgstcd, b.sgst_s sgstcd, b.cess_s cesscd, b.duty_s dutycd, ";
                    sql += "a.saldebglcd parglcd, ";
                    sql += "igst_rvi, cgst_rvi, sgst_rvi, cess_rvi, igst_rvo, cgst_rvo, sgst_rvo, cess_rvo ";
                    sql += "from " + scm1 + ".m_syscnfg a, " + scmf + ".m_post b, " + scm1 + ".m_subleg_com c ";
                    sql += "where c.slcd in('" + slmslcd + "',null) and ";
                    sql += "c.compcd in ('" + COM + "',null) ";
                    DataTable tbl = masterHelp.SQLquery(sql);
                    if(tbl!=null && tbl.Rows.Count > 0)
                    {
                        parglcd = tbl.Rows[0]["parglcd"].retStr()==""?"": tbl.Rows[0]["parglcd"].retStr(); parclass1cd = tbl.Rows[0]["class1cd"].retStr()==""?"": tbl.Rows[0]["class1cd"].retStr();
                        class2cd = tbl.Rows[0]["class2cd"].retStr() == "" ? "" : tbl.Rows[0]["class2cd"].retStr();
                        tcsgl = tbl.Rows[0]["tcsgl"].retStr() == "" ? "" : tbl.Rows[0]["tcsgl"].retStr();
                        prodglcd = tbl.Rows[0]["prodglcd"].retStr() == "" ? "" : tbl.Rows[0]["prodglcd"].retStr();
                        rogl = tbl.Rows[0]["rogl"].retStr() == "" ? "" : tbl.Rows[0]["rogl"].retStr();
                    }
                 

                    //   Calculate Others Amount from amount tab to distrubute into txndtl table
                    double _amtdist = 0, _baldist = 0, _rpldist = 0, _mult = 1;
                    double _amtdistq = 0, _baldistq = 0, _rpldistq = 0;
                    double titamt = 0, titqty = 0;
                    int lastitemno = 0;
                    if (VE.TTXNAMT != null)
                    {
                        for (int i = 0; i <= VE.TTXNAMT.Count - 1; i++)
                        {
                            if (VE.TTXNAMT[i].SLNO != 0 && VE.TTXNAMT[i].AMTCD != null && VE.TTXNAMT[i].AMT != 0)
                            {
                                if (VE.TTXNAMT[i].ADDLESS == "L") _mult = -1; else _mult = 1;
                                if (VE.TTXNAMT[i].TAXCODE == "TC") _amtdistq = _amtdistq + (Convert.ToDouble(VE.TTXNAMT[i].AMT) * _mult);
                                else _amtdist = _amtdist + (Convert.ToDouble(VE.TTXNAMT[i].AMT) * _mult);
                            }
                        }
                    }
                    for (int i = 0; i <= VE.TsalePos_TBATCHDTL.Count - 1; i++)
                    {
                        if (VE.TsalePos_TBATCHDTL[i].SLNO != 0 && VE.TsalePos_TBATCHDTL[i].ITCD != null)
                        {
                            titamt = titamt + VE.TsalePos_TBATCHDTL[i].GROSSAMT.retDbl();
                            titqty = titqty + Convert.ToDouble(VE.TsalePos_TBATCHDTL[i].QNTY);
                            lastitemno = i;
                        }
                    }

                    _baldist = _amtdist; _baldistq = _amtdistq;
                    #endregion
                    if (VE.DefaultAction == "A")
                    {
                        TTXN.EMD_NO = 0;
                        TTXN.DOCCD = VE.T_TXN.DOCCD;
                        TTXN.DOCNO = Cn.MaxDocNumber(TTXN.DOCCD, Ddate);
                        TTXN.DOCNO = Cn.MaxDocNumber(TTXN.DOCCD, Ddate);
                        DOCPATTERN = Cn.DocPattern(Convert.ToInt32(TTXN.DOCNO), TTXN.DOCCD, CommVar.CurSchema(UNQSNO).ToString(), CommVar.FinSchema(UNQSNO), Ddate);
                        auto_no = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), TTXN.DOCNO, TTXN.DOCCD, Ddate);
                        TTXN.AUTONO = auto_no.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
                        Month = auto_no.Split(Convert.ToChar(Cn.GCS()))[1].ToString();
                        //TempData["LASTGOCD" + VE.MENU_PARA] = VE.T_TXN.GOCD;
                        TCH = Cn.T_CONTROL_HDR(TTXN.DOCCD, TTXN.DOCDT, TTXN.DOCNO, TTXN.AUTONO, Month, DOCPATTERN, VE.DefaultAction, scm1, null, null, "".retDbl(), null);
                    }
                    else
                    {
                        TTXN.DOCCD = VE.T_TXN.DOCCD;
                        TTXN.DOCNO = VE.T_TXN.DOCNO;
                        TTXN.AUTONO = VE.T_TXN.AUTONO;
                        Month = VE.T_CNTRL_HDR.MNTHCD;
                        TTXN.EMD_NO = Convert.ToByte((VE.T_CNTRL_HDR.EMD_NO == null ? 0 : VE.T_CNTRL_HDR.EMD_NO) + 1);
                        DOCPATTERN = VE.T_CNTRL_HDR.DOCNO;
                        TTXN.DTAG = "E";
                    }
                    TTXN.DOCTAG = VE.MENU_PARA.retStr().Length > 2 ? VE.MENU_PARA.retStr().Remove(2) : VE.MENU_PARA.retStr();
                    TTXN.SLCD = VE.RETDEBSLCD;
                    TTXN.GOCD = VE.T_TXN.GOCD;
                    TTXN.DUEDAYS = VE.T_TXN.DUEDAYS;
                    TTXN.PARGLCD = parglcd;
                    TTXN.CLASS1CD = parclass1cd;
                    TTXN.MENU_PARA = VE.T_TXN.MENU_PARA;
                    TTXN.REVCHRG = VE.T_TXN.REVCHRG;
                    TTXN.ROAMT = VE.T_TXN.ROAMT;
                    TTXN.BLAMT = VE.T_TXN.BLAMT;
                    TTXN.INCL_RATE = VE.INC_RATE == true ? "Y" : "";
                    if (VE.RoundOff == true) { TTXN.ROYN = "Y"; } else { TTXN.ROYN = "N"; }
                    if (VE.DefaultAction == "E")
                    {
                        dbsql = masterHelp.TblUpdt("t_batchdtl", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                        var comp = DB1.T_BATCHMST.Where(x => x.AUTONO == TTXN.AUTONO).OrderBy(s => s.AUTONO).ToList();
                        foreach (var v in comp)
                        {
                            if (!VE.TsalePos_TBATCHDTL.Where(s => s.BARNO == v.BARNO && s.SLNO == v.SLNO).Any())
                            {
                                dbsql = masterHelp.TblUpdt("t_batchmst", TTXN.AUTONO, "E", "S", "BARNO = '" + v.BARNO + "' and SLNO = '" + v.SLNO + "' ");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            }
                        }
                        //dbsql = masterHelp.TblUpdt("t_batchmst", TTXN.AUTONO, "E");
                        //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = masterHelp.TblUpdt("t_txndtl", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        //dbsql = masterHelp.TblUpdt("t_txntrans", TTXN.AUTONO, "E");
                        //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_txnoth", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_txnmemo", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = masterHelp.TblUpdt("t_txn_linkno", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        dbsql = masterHelp.TblUpdt("t_txnamt", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_txnpymt", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_txnslsmn", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_cntrl_hdr_rem", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc_dtl", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_cntrl_doc_pass", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        // finance
                        dbsql = masterHelp.finTblUpdt("t_vch_gst", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                        dbsql = masterHelp.finTblUpdt("t_vch_bl", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                        dbsql = masterHelp.finTblUpdt("t_vch_class", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                        dbsql = masterHelp.finTblUpdt("t_vch_det", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                        dbsql = masterHelp.finTblUpdt("t_vch_hdr", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                    }

                    // -------------------------Other Info--------------------------//
                    TTXNOTH.AUTONO = TTXN.AUTONO;
                    TTXNOTH.EMD_NO = TTXN.EMD_NO;
                    TTXNOTH.CLCD = TTXN.CLCD;
                    TTXNOTH.DTAG = TTXN.DTAG;
                    TTXNOTH.TAXGRPCD = VE.T_TXNOTH.TAXGRPCD;
                    TTXNOTH.PRCCD = VE.T_TXNOTH.PRCCD;
                    
                    //----------------------------------------------------------//

                    //TTXNLINKNO.EMD_NO = TTXN.EMD_NO;
                    //TTXNLINKNO.CLCD = TTXN.CLCD;
                    //TTXNLINKNO.DTAG = TTXN.DTAG;
                    //TTXNLINKNO.TTAG = TTXN.TTAG;
                    //TTXNLINKNO.AUTONO = TTXN.AUTONO;
                    //TTXNLINKNO.LINKAUTONO = VE.T_TXN_LINKNO.LINKAUTONO;
                    // -------------------------Ttxn Memo--------------------------//   
                    TTXNMEMO.EMD_NO = TTXN.EMD_NO;
                    TTXNMEMO.CLCD = TTXN.CLCD;
                    TTXNMEMO.DTAG = TTXN.DTAG;
                    TTXNMEMO.TTAG = TTXN.TTAG;
                    TTXNMEMO.AUTONO = TTXN.AUTONO;
                    TTXNMEMO.RTDEBCD = VE.T_TXNMEMO.RTDEBCD;
                    TTXNMEMO.NM = VE.T_TXNMEMO.NM;
                    TTXNMEMO.MOBILE = VE.T_TXNMEMO.MOBILE;
                    TTXNMEMO.CITY = "ABC";
                    TTXNMEMO.ADDR = "XYZ";
                    //----------------------------------------------------------//
                    // -------------------------T_TXNPYMT_HDR--------------------------//   
                    TTXNPYMTHDR.EMD_NO = TTXN.EMD_NO;
                    TTXNPYMTHDR.CLCD = TTXN.CLCD;
                    TTXNPYMTHDR.DTAG = TTXN.DTAG;
                    TTXNPYMTHDR.TTAG = TTXN.TTAG;
                    TTXNPYMTHDR.AUTONO = TTXN.AUTONO;
                    TTXNPYMTHDR.RTDEBCD = VE.T_TXNMEMO.RTDEBCD;
                    TTXNPYMTHDR.DRCR = stkdrcr;

                    //----------------------------------------------------------//

                    dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(TTXN.AUTONO, VE.DefaultAction, "S", Month, TTXN.DOCCD, DOCPATTERN, TTXN.DOCDT.retStr(), TTXN.EMD_NO.retShort(), TTXN.DOCNO, Convert.ToDouble(TTXN.DOCNO), null, null, null, TTXN.SLCD);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    dbsql = masterHelp.RetModeltoSql(TTXN, VE.DefaultAction);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    //dbsql = masterHelp.RetModeltoSql(TXNTRANS);
                    //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.RetModeltoSql(TTXNOTH);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    //dbsql = masterHelp.RetModeltoSql(TTXNLINKNO);
                    //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.RetModeltoSql(TTXNMEMO);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.RetModeltoSql(TTXNPYMTHDR, VE.DefaultAction);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    // SAVE T_CNTRL_HDR_UNIQNO
                    //string docbarcode = ""; string UNIQNO = salesfunc.retVchrUniqId(TTXN.DOCCD, TTXN.AUTONO);
                    //sql = "select doccd,docbarcode from " + CommVar.CurSchema(UNQSNO) + ".m_doctype_bar where doccd='" + TTXN.DOCCD + "'";
                    //DataTable dt = masterHelp.SQLquery(sql);
                    //if (dt != null && dt.Rows.Count > 0) docbarcode = dt.Rows[0]["docbarcode"].retStr();
                    //if (VE.DefaultAction == "A")
                    //{
                    //    T_CNTRL_HDR_UNIQNO TCHUNIQNO = new T_CNTRL_HDR_UNIQNO();
                    //    TCHUNIQNO.CLCD = TTXN.CLCD;
                    //    TCHUNIQNO.AUTONO = TTXN.AUTONO;
                    //    TCHUNIQNO.UNIQNO = UNIQNO;
                    //    dbsql = masterHelp.RetModeltoSql(TCHUNIQNO);
                    //    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    //}
                    //string lbatchini = "";
                    //sql = "select lbatchini from " + CommVar.FinSchema(UNQSNO) + ".m_loca where loccd='" + CommVar.Loccd(UNQSNO) + "' and compcd='" + CommVar.Compcd(UNQSNO) + "'";
                    //dt = masterHelp.SQLquery(sql);
                    //if (dt != null && dt.Rows.Count > 0)
                    //{
                    //    lbatchini = dt.Rows[0]["lbatchini"].retStr();
                    //}
                    //  END T_CNTRL_HDR_UNIQNO

                    //  create header record in finschema
                    if (blactpost == true || blgstpost == true)
                    {
                        dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(TTXN.AUTONO, VE.DefaultAction, "F", Month, TTXN.DOCCD, DOCPATTERN, TTXN.DOCDT.retStr(), TTXN.EMD_NO.retShort(), TTXN.DOCNO, Convert.ToDouble(TTXN.DOCNO), null, null, null, TTXN.SLCD);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        double currrt = 111;
                        //if (TTXN.CURRRT != null) currrt = Convert.ToDouble(TTXN.CURRRT);
                        dbsql = masterHelp.InsVch_Hdr(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, null, null, "Y", null, trcd, "", "", TTXN.CURR_CD, currrt, "", revcharge);
                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                    }


                    VE.BALEYR = Convert.ToDateTime(CommVar.FinStartDate(UNQSNO)).Year.retStr();
                    for (int i = 0; i <= VE.TsalePos_TBATCHDTL.Count - 1; i++)
                    {
                        if (VE.TsalePos_TBATCHDTL[i].SLNO != 0 && VE.TsalePos_TBATCHDTL[i].ITCD != null && VE.TsalePos_TBATCHDTL[i].MTRLJOBCD != null && VE.TsalePos_TBATCHDTL[i].STKTYPE != null)
                        {
                            if (i == lastitemno) { _rpldist = _baldist; _rpldistq = _baldistq; }
                            else
                            {
                                if (_amtdist + _amtdistq == 0) { _rpldist = 0; _rpldistq = 0; }
                                else
                                {
                                    _rpldist = ((_amtdist / titamt) * VE.TsalePos_TBATCHDTL[i].GROSSAMT).retDbl().toRound();
                                    _rpldistq = ((_amtdistq / titqty) * Convert.ToDouble(VE.TsalePos_TBATCHDTL[i].QNTY)).toRound();
                                }
                            }
                            _baldist = _baldist - _rpldist; _baldistq = _baldistq - _rpldistq;

                            COUNTER = COUNTER + 1;
                            T_TXNDTL TTXNDTL = new T_TXNDTL();
                            TTXNDTL.CLCD = TTXN.CLCD;
                            TTXNDTL.EMD_NO = TTXN.EMD_NO;
                            TTXNDTL.DTAG = TTXN.DTAG;
                            TTXNDTL.AUTONO = TTXN.AUTONO;
                            TTXNDTL.SLNO = VE.TsalePos_TBATCHDTL[i].SLNO;
                            TTXNDTL.MTRLJOBCD = VE.TsalePos_TBATCHDTL[i].MTRLJOBCD;
                            TTXNDTL.ITCD = VE.TsalePos_TBATCHDTL[i].ITCD;
                            TTXNDTL.PARTCD = VE.TsalePos_TBATCHDTL[i].PARTCD;
                            TTXNDTL.COLRCD = VE.TsalePos_TBATCHDTL[i].COLRCD;
                            TTXNDTL.SIZECD = VE.TsalePos_TBATCHDTL[i].SIZECD;
                            TTXNDTL.STKDRCR = stkdrcr;
                            TTXNDTL.STKTYPE = VE.TsalePos_TBATCHDTL[i].STKTYPE;
                            TTXNDTL.HSNCODE = VE.TsalePos_TBATCHDTL[i].HSNCODE;
                            TTXNDTL.ITREM = VE.TsalePos_TBATCHDTL[i].ITREM;
                            //TTXNDTL.PCSREM = VE.TsalePos_TBATCHDTL[i].PCSREM;
                            //TTXNDTL.FREESTK = VE.TsalePos_TBATCHDTL[i].FREESTK;
                            TTXNDTL.BATCHNO = VE.TsalePos_TBATCHDTL[i].BATCHNO;
                            TTXNDTL.BALEYR = VE.BALEYR;// VE.TTXNDTL[i].BALEYR;
                            //TTXNDTL.BALENO = VE.TTXNDTL[i].BALENO;
                            TTXNDTL.GOCD = VE.T_TXN.GOCD;
                            //TTXNDTL.JOBCD = VE.TTXNDTL[i].JOBCD;
                            TTXNDTL.NOS = VE.TsalePos_TBATCHDTL[i].NOS == null ? 0 : VE.TsalePos_TBATCHDTL[i].NOS;
                            TTXNDTL.QNTY = VE.TsalePos_TBATCHDTL[i].QNTY;
                            TTXNDTL.BLQNTY = VE.TsalePos_TBATCHDTL[i].BLQNTY;
                            TTXNDTL.RATE = VE.TsalePos_TBATCHDTL[i].RATE;
                            TTXNDTL.AMT = VE.TsalePos_TBATCHDTL[i].GROSSAMT;
                            TTXNDTL.FLAGMTR = VE.TsalePos_TBATCHDTL[i].FLAGMTR;
                            TTXNDTL.TOTDISCAMT = VE.TsalePos_TBATCHDTL[i].TOTDISCAMT;
                            TTXNDTL.TXBLVAL = VE.TsalePos_TBATCHDTL[i].TXBLVAL;
                            TTXNDTL.IGSTPER = VE.TsalePos_TBATCHDTL[i].IGSTPER;
                            TTXNDTL.CGSTPER = VE.TsalePos_TBATCHDTL[i].CGSTPER;
                            TTXNDTL.SGSTPER = VE.TsalePos_TBATCHDTL[i].SGSTPER;
                            TTXNDTL.CESSPER = VE.TsalePos_TBATCHDTL[i].CESSPER;
                            TTXNDTL.DUTYPER = VE.TsalePos_TBATCHDTL[i].DUTYPER;
                            TTXNDTL.IGSTAMT = VE.TsalePos_TBATCHDTL[i].IGSTAMT;
                            TTXNDTL.CGSTAMT = VE.TsalePos_TBATCHDTL[i].CGSTAMT;
                            TTXNDTL.SGSTAMT = VE.TsalePos_TBATCHDTL[i].SGSTAMT;
                            TTXNDTL.CESSAMT = VE.TsalePos_TBATCHDTL[i].CESSAMT;
                            TTXNDTL.DUTYAMT = VE.TsalePos_TBATCHDTL[i].DUTYAMT;
                            TTXNDTL.NETAMT = VE.TsalePos_TBATCHDTL[i].NETAMT;
                            TTXNDTL.OTHRAMT = _rpldist + _rpldistq;
                            //TTXNDTL.AGDOCNO = VE.TsalePos_TBATCHDTL[i].AGDOCNO;
                            //TTXNDTL.AGDOCDT = VE.TsalePos_TBATCHDTL[i].AGDOCDT;
                            //TTXNDTL.SHORTQNTY = VE.TsalePos_TBATCHDTL[i].SHORTQNTY;
                            TTXNDTL.DISCTYPE = VE.TsalePos_TBATCHDTL[i].DISCTYPE;
                            TTXNDTL.DISCRATE = VE.TsalePos_TBATCHDTL[i].DISCRATE;
                            TTXNDTL.DISCAMT = VE.TsalePos_TBATCHDTL[i].DISCAMT;
                            //TTXNDTL.SCMDISCTYPE = VE.TsalePos_TBATCHDTL[i].SCMDISCTYPE;
                            //TTXNDTL.SCMDISCRATE = VE.TsalePos_TBATCHDTL[i].SCMDISCRATE;
                            //TTXNDTL.SCMDISCAMT = VE.TsalePos_TBATCHDTL[i].SCMDISCAMT;
                            //TTXNDTL.TDDISCTYPE = VE.TsalePos_TBATCHDTL[i].TDDISCTYPE;
                            //TTXNDTL.TDDISCRATE = VE.TsalePos_TBATCHDTL[i].TDDISCRATE;
                            //TTXNDTL.TDDISCAMT = VE.TsalePos_TBATCHDTL[i].TDDISCAMT;
                            TTXNDTL.PRCCD = VE.T_TXNOTH.PRCCD;
                            //TTXNDTL.PRCEFFDT = VE.TsalePos_TBATCHDTL[i].PRCEFFDT;
                            TTXNDTL.GLCD = VE.TsalePos_TBATCHDTL[i].GLCD;
                            dbsql = masterHelp.RetModeltoSql(TTXNDTL);
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                            //dbqty = dbqty + Convert.ToDouble(VE.TsalePos_TBATCHDTL[i].QNTY);
                            dbqty = dbqty + Convert.ToDouble(50);
                            igst = igst + Convert.ToDouble(VE.TsalePos_TBATCHDTL[i].IGSTAMT);
                            cgst = cgst + Convert.ToDouble(VE.TsalePos_TBATCHDTL[i].CGSTAMT);
                            sgst = sgst + Convert.ToDouble(VE.TsalePos_TBATCHDTL[i].SGSTAMT);
                            cess = cess + Convert.ToDouble(VE.TsalePos_TBATCHDTL[i].CESSAMT);
                            duty = duty + Convert.ToDouble(VE.TsalePos_TBATCHDTL[i].DUTYAMT);
                        }
                    }

                    COUNTER = 0; int COUNTERBATCH = 0; bool recoexist = false;
                    if (VE.TsalePos_TBATCHDTL != null && VE.TsalePos_TBATCHDTL.Count > 0)
                    {
                        for (int i = 0; i <= VE.TsalePos_TBATCHDTL.Count - 1; i++)
                        {
                            if (VE.TsalePos_TBATCHDTL[i].ITCD != null && VE.TsalePos_TBATCHDTL[i].QNTY != 0)
                            {
                                //bool flagbatch = false;
                                //string barno = "";

                                //barno = salesfunc.GenerateBARNO(VE.TsalePos_TBATCHDTL[i].ITCD,VE.TsalePos_TBATCHDTL[i].CLRBARCODE, VE.TsalePos_TBATCHDTL[i].SZBARCODE);
                                //sql = "Select * from " + CommVar.CurSchema(UNQSNO) + ".t_batchmst where barno='" + barno + "'";
                                //OraCmd.CommandText = sql; var OraReco = OraCmd.ExecuteReader();
                                //if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();

                                //if (recoexist == false) flagbatch = true;


                                //// checking barno exist or not
                                //string Action = "", SqlCondition = "";
                                //if (VE.DefaultAction == "A")
                                //{
                                //    Action = VE.DefaultAction;
                                //}
                                //else
                                //{
                                //    sql = "Select * from " + CommVar.CurSchema(UNQSNO) + ".t_batchmst where autono='" + TTXN.AUTONO + "' and slno = " + VE.TsalePos_TBATCHDTL[i].SLNO + " and barno='" + barno + "' ";
                                //    OraCmd.CommandText = sql; OraReco = OraCmd.ExecuteReader();
                                //    if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();

                                //    if (recoexist == true)
                                //    {
                                //        Action = "E";
                                //        SqlCondition = "autono = '" + TTXN.AUTONO + "' and slno = " + VE.TsalePos_TBATCHDTL[i].SLNO + " and barno='" + barno + "' ";
                                //        flagbatch = true;
                                //        barno = VE.TsalePos_TBATCHDTL[i].BARNO;
                                //    }
                                //    else
                                //    {
                                //        Action = "A";
                                //    }

                                //}

                                //if (flagbatch == true)
                                //{
                                //    T_BATCHMST TBATCHMST = new T_BATCHMST();
                                //    TBATCHMST.EMD_NO = TTXN.EMD_NO;
                                //    TBATCHMST.CLCD = TTXN.CLCD;
                                //    TBATCHMST.DTAG = TTXN.DTAG;
                                //    TBATCHMST.TTAG = TTXN.TTAG;
                                //    TBATCHMST.SLNO = VE.TsalePos_TBATCHDTL[i].SLNO; // ++COUNTERBATCH;
                                //    TBATCHMST.AUTONO = TTXN.AUTONO;
                                //    TBATCHMST.SLCD = TTXN.SLCD;
                                //    TBATCHMST.MTRLJOBCD = VE.TsalePos_TBATCHDTL[i].MTRLJOBCD;
                                //    TBATCHMST.STKTYPE = VE.TsalePos_TBATCHDTL[i].STKTYPE.retStr();
                                //    TBATCHMST.JOBCD = TTXN.JOBCD;
                                //    TBATCHMST.BARNO = barno;
                                //    TBATCHMST.ITCD = VE.TsalePos_TBATCHDTL[i].ITCD;
                                //    TBATCHMST.PARTCD = VE.TsalePos_TBATCHDTL[i].PARTCD;
                                //    TBATCHMST.SIZECD = VE.TsalePos_TBATCHDTL[i].SIZECD;
                                //    TBATCHMST.COLRCD = VE.TsalePos_TBATCHDTL[i].COLRCD;
                                //    TBATCHMST.NOS = VE.TsalePos_TBATCHDTL[i].NOS;
                                //    TBATCHMST.QNTY = VE.TsalePos_TBATCHDTL[i].QNTY;
                                //    TBATCHMST.RATE = VE.TsalePos_TBATCHDTL[i].RATE;
                                //    TBATCHMST.AMT = VE.TsalePos_TBATCHDTL[i].AMT;
                                //    TBATCHMST.FLAGMTR = VE.TsalePos_TBATCHDTL[i].FLAGMTR;
                                //    //TBATCHMST.MTRL_COST = VE.TsalePos_TBATCHDTL[i].MTRL_COST;
                                //    //TBATCHMST.OTH_COST = VE.TsalePos_TBATCHDTL[i].OTH_COST;
                                //    TBATCHMST.ITREM = VE.TsalePos_TBATCHDTL[i].ITREM;
                                //    TBATCHMST.PDESIGN = VE.TsalePos_TBATCHDTL[i].PDESIGN;
                                //    if (VE.T_TXN.BARGENTYPE == "E")
                                //    {
                                //        TBATCHMST.HSNCODE = VE.TsalePos_TBATCHDTL[i].HSNCODE;

                                //        TBATCHMST.OURDESIGN = VE.TsalePos_TBATCHDTL[i].OURDESIGN;
                                //        //if (VE.TsalePos_TBATCHDTL[i].BarImages.retStr() != "")
                                //        //{
                                //        //    var barimg = SaveBarImage(VE.TsalePos_TBATCHDTL[i].BarImages, barno, TTXN.EMD_NO.retShort());
                                //        //    DB.T_BATCH_IMG_HDR.AddRange(barimg.Item1);
                                //        //    DB.SaveChanges();
                                //        //    var disntImgHdr = barimg.Item1.GroupBy(u => u.BARNO).Select(r => r.First()).ToList();
                                //        //    foreach (var imgbar in disntImgHdr)
                                //        //    {
                                //        //        T_BATCH_IMG_HDR_LINK m_batchImglink = new T_BATCH_IMG_HDR_LINK();
                                //        //        m_batchImglink.CLCD = TTXN.CLCD;
                                //        //        m_batchImglink.EMD_NO = TTXN.EMD_NO;
                                //        //        m_batchImglink.BARNO = imgbar.BARNO;
                                //        //        m_batchImglink.MAINBARNO = imgbar.BARNO;
                                //        //        DB.T_BATCH_IMG_HDR_LINK.Add(m_batchImglink);
                                //        //    }
                                //        //}

                                //    }
                                //    //TBATCHMST.ORGBATCHAUTONO = VE.TsalePos_TBATCHDTL[i].ORGBATCHAUTONO;
                                //    //TBATCHMST.ORGBATCHSLNO = VE.TsalePos_TBATCHDTL[i].ORGBATCHSLNO;
                                //    TBATCHMST.DIA = VE.TsalePos_TBATCHDTL[i].DIA;
                                //    TBATCHMST.CUTLENGTH = VE.TsalePos_TBATCHDTL[i].CUTLENGTH;
                                //    TBATCHMST.LOCABIN = VE.TsalePos_TBATCHDTL[i].LOCABIN;
                                //    TBATCHMST.SHADE = VE.TsalePos_TBATCHDTL[i].SHADE;
                                //    TBATCHMST.MILLNM = VE.TsalePos_TBATCHDTL[i].MILLNM;
                                //    TBATCHMST.BATCHNO = VE.TsalePos_TBATCHDTL[i].BATCHNO;
                                //    TBATCHMST.ORDAUTONO = VE.TsalePos_TBATCHDTL[i].ORDAUTONO;
                                //    TBATCHMST.ORDSLNO = VE.TsalePos_TBATCHDTL[i].ORDSLNO.retShort();
                                //    dbsql = masterHelp.RetModeltoSql(TBATCHMST);
                                //    dbsql = masterHelp.RetModeltoSql(TBATCHMST, Action, "", SqlCondition);
                                //    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                //}
                                COUNTER = COUNTER + 1;
                                T_BATCHDTL TsalePos_TBATCHDTL = new T_BATCHDTL();
                                TsalePos_TBATCHDTL.EMD_NO = TTXN.EMD_NO;
                                TsalePos_TBATCHDTL.CLCD = TTXN.CLCD;
                                TsalePos_TBATCHDTL.DTAG = TTXN.DTAG;
                                TsalePos_TBATCHDTL.TTAG = TTXN.TTAG;
                                TsalePos_TBATCHDTL.AUTONO = TTXN.AUTONO;
                                TsalePos_TBATCHDTL.SLNO = VE.TsalePos_TBATCHDTL[i].SLNO;  //COUNTER.retShort();
                                TsalePos_TBATCHDTL.TXNSLNO = VE.TsalePos_TBATCHDTL[i].SLNO;
                                TsalePos_TBATCHDTL.GOCD = VE.T_TXN.GOCD;
                                TsalePos_TBATCHDTL.BARNO = VE.TsalePos_TBATCHDTL[i].BARNO;
                                TsalePos_TBATCHDTL.MTRLJOBCD = VE.TsalePos_TBATCHDTL[i].MTRLJOBCD;
                                TsalePos_TBATCHDTL.PARTCD = VE.TsalePos_TBATCHDTL[i].PARTCD;
                                TsalePos_TBATCHDTL.HSNCODE = VE.TsalePos_TBATCHDTL[i].HSNCODE;
                                TsalePos_TBATCHDTL.STKDRCR = stkdrcr;
                                TsalePos_TBATCHDTL.NOS = VE.TsalePos_TBATCHDTL[i].NOS;
                                TsalePos_TBATCHDTL.QNTY = VE.TsalePos_TBATCHDTL[i].QNTY;
                                TsalePos_TBATCHDTL.BLQNTY = VE.TsalePos_TBATCHDTL[i].BLQNTY;
                                TsalePos_TBATCHDTL.FLAGMTR = VE.TsalePos_TBATCHDTL[i].FLAGMTR;
                                TsalePos_TBATCHDTL.ITREM = VE.TsalePos_TBATCHDTL[i].ITREM;
                                TsalePos_TBATCHDTL.RATE = VE.TsalePos_TBATCHDTL[i].RATE;
                                TsalePos_TBATCHDTL.DISCRATE = VE.TsalePos_TBATCHDTL[i].DISCRATE;
                                TsalePos_TBATCHDTL.DISCTYPE = VE.TsalePos_TBATCHDTL[i].DISCTYPE;
                                TsalePos_TBATCHDTL.SCMDISCRATE = VE.TsalePos_TBATCHDTL[i].SCMDISCRATE;
                                TsalePos_TBATCHDTL.SCMDISCTYPE = VE.TsalePos_TBATCHDTL[i].SCMDISCTYPE;
                                TsalePos_TBATCHDTL.TDDISCRATE = VE.TsalePos_TBATCHDTL[i].TDDISCRATE;
                                TsalePos_TBATCHDTL.TDDISCTYPE = VE.TsalePos_TBATCHDTL[i].TDDISCTYPE;
                                TsalePos_TBATCHDTL.ORDAUTONO = VE.TsalePos_TBATCHDTL[i].ORDAUTONO;
                                TsalePos_TBATCHDTL.ORDSLNO = VE.TsalePos_TBATCHDTL[i].ORDSLNO.retShort();
                                TsalePos_TBATCHDTL.DIA = VE.TsalePos_TBATCHDTL[i].DIA;
                                TsalePos_TBATCHDTL.CUTLENGTH = VE.TsalePos_TBATCHDTL[i].CUTLENGTH;
                                TsalePos_TBATCHDTL.LOCABIN = VE.TsalePos_TBATCHDTL[i].LOCABIN;
                                TsalePos_TBATCHDTL.SHADE = VE.TsalePos_TBATCHDTL[i].SHADE;
                                TsalePos_TBATCHDTL.MILLNM = VE.TsalePos_TBATCHDTL[i].MILLNM;
                                TsalePos_TBATCHDTL.BATCHNO = VE.TsalePos_TBATCHDTL[i].BATCHNO;
                                TsalePos_TBATCHDTL.BALEYR = VE.BALEYR;// VE.TsalePos_TBATCHDTL[i].BALEYR;
                                TsalePos_TBATCHDTL.TXNSLNO = VE.TsalePos_TBATCHDTL[i].SLNO;
                                TsalePos_TBATCHDTL.INCLRATE = VE.TsalePos_TBATCHDTL[i].INCLRATE.retDbl();
                                TsalePos_TBATCHDTL.PCSACTION = VE.TsalePos_TBATCHDTL[i].PCSACTION.retStr();

                                dbsql = masterHelp.RetModeltoSql(TsalePos_TBATCHDTL);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                            }
                        }
                    }

                    if (dbqty == 0)
                    {
                        dberrmsg = "Quantity not entered"; goto dbnotsave;
                    }
                    isl = 1;

                    if (VE.TTXNAMT != null)
                    {
                        for (int i = 0; i <= VE.TTXNAMT.Count - 1; i++)
                        {
                            if (VE.TTXNAMT[i].SLNO != 0 && VE.TTXNAMT[i].AMTCD != null && VE.TTXNAMT[i].AMT != 0)
                            {
                                T_TXNAMT TTXNAMT = new T_TXNAMT();
                                TTXNAMT.AUTONO = TTXN.AUTONO;
                                TTXNAMT.SLNO = VE.TTXNAMT[i].SLNO;
                                TTXNAMT.EMD_NO = TTXN.EMD_NO;
                                TTXNAMT.CLCD = TTXN.CLCD;
                                TTXNAMT.DTAG = TTXN.DTAG;
                                TTXNAMT.AMTCD = VE.TTXNAMT[i].AMTCD;
                                TTXNAMT.AMTDESC = VE.TTXNAMT[i].AMTDESC;
                                TTXNAMT.AMTRATE = VE.TTXNAMT[i].AMTRATE;
                                TTXNAMT.HSNCODE = VE.TTXNAMT[i].HSNCODE;
                                TTXNAMT.CURR_AMT = VE.TTXNAMT[i].CURR_AMT;
                                TTXNAMT.AMT = VE.TTXNAMT[i].AMT;
                                TTXNAMT.IGSTPER = VE.TTXNAMT[i].IGSTPER;
                                TTXNAMT.IGSTAMT = VE.TTXNAMT[i].IGSTAMT;
                                TTXNAMT.CGSTPER = VE.TTXNAMT[i].CGSTPER;
                                TTXNAMT.CGSTAMT = VE.TTXNAMT[i].CGSTAMT;
                                TTXNAMT.SGSTPER = VE.TTXNAMT[i].SGSTPER;
                                TTXNAMT.SGSTAMT = VE.TTXNAMT[i].SGSTAMT;
                                TTXNAMT.CESSPER = VE.TTXNAMT[i].CESSPER;
                                TTXNAMT.CESSAMT = VE.TTXNAMT[i].CESSAMT;
                                TTXNAMT.DUTYPER = VE.TTXNAMT[i].DUTYPER;
                                TTXNAMT.DUTYAMT = VE.TTXNAMT[i].DUTYAMT;
                                dbsql = masterHelp.RetModeltoSql(TTXNAMT);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                if (blactpost == true)
                                {
                                    isl = isl + 1;
                                    string adrcr = cr;
                                    if (VE.TTXNAMT[i].ADDLESS == "L") adrcr = dr;
                                    dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), adrcr, VE.TTXNAMT[i].GLCD, sslcd,
                                            Convert.ToDouble(VE.TTXNAMT[i].AMT), strrem, parglcd, TTXN.SLCD, 0, 0, Convert.ToDouble(VE.TTXNAMT[i].CURR_AMT));
                                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                    if (adrcr == "D") dbDrAmt = dbDrAmt + Convert.ToDouble(VE.TTXNAMT[i].AMT);
                                    else dbCrAmt = dbCrAmt + Convert.ToDouble(VE.TTXNAMT[i].AMT);

                                    if (VE.TsalePos_TBATCHDTL != null)
                                    {
                                        dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                                                VE.TsalePos_TBATCHDTL[0].CLASS1CD, "", Convert.ToDouble(VE.TTXNAMT[i].AMT), 0, strrem);
                                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                    }
                                }
                                igst = igst + Convert.ToDouble(VE.TTXNAMT[i].IGSTAMT);
                                cgst = cgst + Convert.ToDouble(VE.TTXNAMT[i].CGSTAMT);
                                sgst = sgst + Convert.ToDouble(VE.TTXNAMT[i].SGSTAMT);
                                cess = cess + Convert.ToDouble(VE.TTXNAMT[i].CESSAMT);
                                duty = duty + Convert.ToDouble(VE.TTXNAMT[i].DUTYAMT);
                            }
                        }
                    }
                    if (VE.TTXNPYMT != null)
                    {
                        for (int i = 0; i <= VE.TTXNPYMT.Count - 1; i++)
                        {
                            if (VE.TTXNPYMT[i].SLNO != 0 && VE.TTXNPYMT[i].AMT.retDbl() != 0)
                            {
                                T_TXNPYMT TTXNPYMNT = new T_TXNPYMT();
                                TTXNPYMNT.AUTONO = TTXN.AUTONO;
                                TTXNPYMNT.SLNO = VE.TTXNPYMT[i].SLNO;
                                TTXNPYMNT.EMD_NO = TTXN.EMD_NO;
                                TTXNPYMNT.CLCD = TTXN.CLCD;
                                TTXNPYMNT.DTAG = TTXN.DTAG;
                                TTXNPYMNT.PYMTCD = VE.TTXNPYMT[i].PYMTCD;
                                TTXNPYMNT.AMT = VE.TTXNPYMT[i].AMT.retDbl();
                                TTXNPYMNT.CARDNO = VE.TTXNPYMT[i].CARDNO;
                                TTXNPYMNT.INSTNO = VE.TTXNPYMT[i].INSTNO;
                                if(VE.TTXNPYMT[i].INSTDT.retStr() != "")
                                {
                                    TTXNPYMNT.INSTDT = Convert.ToDateTime(VE.TTXNPYMT[i].INSTDT);
                                }
                                TTXNPYMNT.PYMTREM = VE.TTXNPYMT[i].PYMTREM;
                                TTXNPYMNT.GLCD = VE.TTXNPYMT[i].GLCD;
                                dbsql = masterHelp.RetModeltoSql(TTXNPYMNT);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            }
                        }
                    }
                    if (VE.TTXNSLSMN != null)
                    {
                        for (int i = 0; i <= VE.TTXNSLSMN.Count - 1; i++)
                        {
                            if (VE.TTXNSLSMN[i].SLNO != 0 && VE.TTXNSLSMN[i].SLMSLCD != null && VE.TTXNSLSMN[i].BLAMT != 0)
                            {
                                T_TXNSLSMN TTXNSLSMN = new T_TXNSLSMN();
                                TTXNSLSMN.AUTONO = TTXN.AUTONO;
                                TTXNSLSMN.EMD_NO = TTXN.EMD_NO;
                                TTXNSLSMN.CLCD = TTXN.CLCD;
                                TTXNSLSMN.DTAG = TTXN.DTAG;
                                TTXNSLSMN.SLMSLCD = VE.TTXNSLSMN[i].SLMSLCD;
                                TTXNSLSMN.PER = VE.TTXNSLSMN[i].PER.retDbl();
                                TTXNSLSMN.ITAMT = VE.TTXNSLSMN[i].ITAMT.retDbl();
                                TTXNSLSMN.BLAMT = VE.TTXNSLSMN[i].BLAMT.retDbl();
                                dbsql = masterHelp.RetModeltoSql(TTXNSLSMN);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            }
                        }
                    }
                    #region Document Passing checking
                    // -----------------------DOCUMENT PASSING DATA---------------------------//
                    double TRAN_AMT = Convert.ToDouble(TTXN.BLAMT);
                    if (docpassrem != "")
                    {
                        docpassrem = "Doc # " + TTXN.DOCNO.ToString() + " Party Name # " + VE.SLNM + " [" + VE.SLAREA + "]".ToString() + "~" + docpassrem;
                        var TCDP_DATA1 = Cn.T_CONTROL_DOC_PASS(TTXN.DOCCD, 0, TTXN.EMD_NO.Value, TTXN.AUTONO, CommVar.CurSchema(UNQSNO), docpassrem);
                        if (TCDP_DATA1.Item1.Count != 0)
                        {
                            DB.T_CNTRL_DOC_PASS.AddRange(TCDP_DATA1.Item1);
                        }
                    }
                    else
                    {
                        var TCDP_DATA = Cn.T_CONTROL_DOC_PASS(TTXN.DOCCD, TRAN_AMT, TTXN.EMD_NO.Value, TTXN.AUTONO, CommVar.CurSchema(UNQSNO));
                        if (TCDP_DATA.Item1.Count != 0)
                        {
                            DB.T_CNTRL_DOC_PASS.AddRange(TCDP_DATA.Item1);
                        }
                    }
                    if (docpassrem != "") DB.T_CNTRL_DOC_PASS.Add(TCDP);
                    #endregion

                    if (VE.UploadDOC != null)// add
                    {
                        var img = Cn.SaveUploadImageTransaction(VE.UploadDOC, TTXN.AUTONO, TTXN.EMD_NO.Value);
                        if (img.Item1.Count != 0)
                        {
                            for (int tr = 0; tr <= img.Item1.Count - 1; tr++)
                            {
                                dbsql = masterHelp.RetModeltoSql(img.Item1[tr]);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            }
                            for (int tr = 0; tr <= img.Item2.Count - 1; tr++)
                            {
                                dbsql = masterHelp.RetModeltoSql(img.Item2[tr]);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            }
                        }
                    }
                    if (VE.T_CNTRL_HDR_REM.DOCREM != null)// add REMARKS
                    {
                        var NOTE = Cn.SAVETRANSACTIONREMARKS(VE.T_CNTRL_HDR_REM, TTXN.AUTONO, TTXN.CLCD, TTXN.EMD_NO.Value);
                        if (NOTE.Item1.Count != 0)
                        {
                            for (int tr = 0; tr <= NOTE.Item1.Count - 1; tr++)
                            {
                                dbsql = masterHelp.RetModeltoSql(NOTE.Item1[tr]);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            }
                        }
                    }
                    #region Financial Posting
                    string salpur = "";
                    double itamt = 0;
                    if (blactpost == true)
                    {
                        salpur = "S";
                        string prodrem = strrem; expglcd = "";

                        var AMTGLCD = (from x in VE.TsalePos_TBATCHDTL
                                       group x by new { x.GLCD, x.CLASS1CD } into P
                                       select new
                                       {
                                           GLCD = P.Key.GLCD,
                                           CLASS1CD = P.Key.CLASS1CD,
                                           QTY = P.Sum(A => A.QNTY),
                                           TXBLVAL = P.Sum(A => A.TXBLVAL)
                                       }).Where(a => a.QTY != 0).ToList();

                        if (AMTGLCD != null && AMTGLCD.Count > 0)
                        {
                            for (int i = 0; i <= AMTGLCD.Count - 1; i++)
                            {
                                isl = isl + 1;
                                dbamt = AMTGLCD[i].TXBLVAL.retDbl();
                                dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), cr, AMTGLCD[i].GLCD, sslcd,
                                        dbamt, prodrem, parglcd, TTXN.SLCD, AMTGLCD[i].QTY.retDbl(), 0, 0);
                                OraCmd.CommandText = dbsql;
                                OraCmd.ExecuteNonQuery();
                                if (cr == "D") dbDrAmt = dbDrAmt + dbamt;
                                else dbCrAmt = dbCrAmt + dbamt;
                                dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                                        AMTGLCD[i].CLASS1CD, "", AMTGLCD[i].TXBLVAL.retDbl(), 0, strrem);
                                OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                itamt = itamt + AMTGLCD[i].TXBLVAL.retDbl();
                                expglcd = AMTGLCD[i].GLCD;
                            }
                        }
                        #region  GST Financial Part
                        string[] gstpostcd = new string[5];
                        double[] gstpostamt = new double[5];
                        for (int gt = 0; gt < 5; gt++)
                        {
                            gstpostcd[gt] = ""; gstpostamt[gt] = 0;
                        }

                        int gi = 0;
                        if (revcharge != "Y")
                        {
                            if (tbl != null && tbl.Rows.Count > 0)
                            {
                                gstpostcd[gi] = tbl.Rows[0]["igstcd"].ToString(); gstpostamt[gi] = igst; gi++;
                                gstpostcd[gi] = tbl.Rows[0]["cgstcd"].ToString(); gstpostamt[gi] = cgst; gi++;
                                gstpostcd[gi] = tbl.Rows[0]["sgstcd"].ToString(); gstpostamt[gi] = sgst; gi++;
                                gstpostcd[gi] = tbl.Rows[0]["cesscd"].ToString(); gstpostamt[gi] = cess; gi++;
                                gstpostcd[gi] = tbl.Rows[0]["dutycd"].ToString(); gstpostamt[gi] = duty; gi++;
                            }
                        }
                        else
                        {
                            if (tbl != null && tbl.Rows.Count > 0)
                            {
                                gstpostcd[gi] = tbl.Rows[0]["igst_rvi"].ToString(); gstpostamt[gi] = igst; gi++;
                                gstpostcd[gi] = tbl.Rows[0]["cgst_rvi"].ToString(); gstpostamt[gi] = cgst; gi++;
                                gstpostcd[gi] = tbl.Rows[0]["sgst_rvi"].ToString(); gstpostamt[gi] = sgst; gi++;
                                gstpostcd[gi] = tbl.Rows[0]["cess_rvi"].ToString(); gstpostamt[gi] = cess; gi++;
                                gstpostcd[gi] = tbl.Rows[0]["dutycd"].ToString(); gstpostamt[gi] = duty; gi++;
                            }
                        }

                        for (int gt = 0; gt < 5; gt++)
                        {
                            if (gstpostamt[gt] != 0)
                            {
                                isl = isl + 1;
                                dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), cr, gstpostcd[gt], sslcd,
                                        gstpostamt[gt], prodrem, parglcd, TTXN.SLCD, dbqty, 0, 0);
                                OraCmd.CommandText = dbsql;
                                OraCmd.ExecuteNonQuery();
                                if (cr == "D") dbDrAmt = dbDrAmt + gstpostamt[gt];
                                else dbCrAmt = dbCrAmt + gstpostamt[gt];
                                dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                                        parclass1cd, class2cd, gstpostamt[gt], 0, strrem);
                                OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                            }
                        }
                        if (revcharge == "Y")
                        {
                            gi = 0;
                            if (tbl != null && tbl.Rows.Count > 0)
                            {
                                gstpostcd[gi] = tbl.Rows[0]["igst_rvo"].ToString(); gstpostamt[gi] = igst; gi++;
                                gstpostcd[gi] = tbl.Rows[0]["cgst_rvo"].ToString(); gstpostamt[gi] = cgst; gi++;
                                gstpostcd[gi] = tbl.Rows[0]["sgst_rvo"].ToString(); gstpostamt[gi] = sgst; gi++;
                                gstpostcd[gi] = tbl.Rows[0]["cess_rvo"].ToString(); gstpostamt[gi] = cess; gi++;
                                gstpostcd[gi] = tbl.Rows[0]["dutycd"].ToString(); gstpostamt[gi] = duty; gi++;

                            }
                            for (int gt = 0; gt < 5; gt++)
                            {
                                if (gstpostamt[gt] != 0)
                                {
                                    isl = isl + 1;
                                    dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), dr, gstpostcd[gt], null,
                                            gstpostamt[gt], prodrem, parglcd, TTXN.SLCD, dbqty, 0, 0);
                                    OraCmd.CommandText = dbsql;
                                    OraCmd.ExecuteNonQuery();
                                    if (dr == "D") dbDrAmt = dbDrAmt + gstpostamt[gt];
                                    else dbCrAmt = dbCrAmt + gstpostamt[gt];
                                    dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), null,
                                            parclass1cd, class2cd, gstpostamt[gt], 0, strrem);
                                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                }
                            }
                        }
                        #endregion
                        //  TCS
                        dbamt = Convert.ToDouble(VE.T_TXN.TCSAMT);
                        if (dbamt != 0)
                        {
                            string adrcr = cr;

                            isl = isl + 1;
                            dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), adrcr, tcsgl, sslcd,
                                    dbamt, strrem, parglcd, TTXN.SLCD, 0, 0, 0);
                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                            if (adrcr == "D") dbDrAmt = dbDrAmt + dbamt;
                            else dbCrAmt = dbCrAmt + dbamt;
                            dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                                    null, null, Convert.ToDouble(VE.T_TXN.TCSAMT), 0, strrem + " TCS @ " + VE.T_TXN.TCSPER.ToString() + "%");
                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                        }
                        //  Ronded off
                        dbamt = Convert.ToDouble(VE.T_TXN.ROAMT);
                        if (dbamt != 0)
                        {
                            string adrcr = cr;
                            if (dbamt < 0) adrcr = dr;
                            if (dbamt < 0) dbamt = dbamt * -1;

                            isl = isl + 1;
                            dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), adrcr, rogl, null,
                                    dbamt, strrem, parglcd, TTXN.SLCD, 0, 0, 0);
                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                            if (adrcr == "D") dbDrAmt = dbDrAmt + dbamt;
                            else dbCrAmt = dbCrAmt + dbamt;
                        }
                        //  Party wise posting
                        isl = 1; //isl + 1;
                        dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), dr,
                            parglcd, sslcd, Convert.ToDouble(VE.T_TXN.BLAMT), strrem, prodglcd,
                            null, dbqty, 0, dbcurramt);
                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                        if (dr == "D") dbDrAmt = dbDrAmt + Convert.ToDouble(VE.T_TXN.BLAMT);
                        else dbCrAmt = dbCrAmt + Convert.ToDouble(VE.T_TXN.BLAMT);
                        dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                                parclass1cd, class2cd, Convert.ToDouble(VE.T_TXN.BLAMT), dbcurramt, strrem);
                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();

                        strblno = ""; strbldt = ""; strduedt = ""; strvtype = ""; strrefno = "";
                        strvtype = "BL";
                        strduedt = Convert.ToDateTime(TTXN.DOCDT.Value).AddDays(Convert.ToDouble(TTXN.DUEDAYS)).ToString().retDateStr();

                        strbldt = TTXN.DOCDT.ToString();
                        strblno = DOCPATTERN;

                        string blconslcd = TTXN.CONSLCD;
                        if (TTXN.SLCD != sslcd) blconslcd = TTXN.SLCD;
                        if (blconslcd == sslcd) blconslcd = "";
                        dbsql = masterHelp.InsVch_Bl(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, dr,
                               parglcd, sslcd, blconslcd, TTXNOTH.AGSLCD, parclass1cd, Convert.ToSByte(isl),
                                VE.T_TXN.BLAMT.retDbl(), strblno, strbldt, strrefno, strduedt, strvtype, TTXN.DUEDAYS.retDbl(), itamt, TTXNOTH.POREFNO,
                                TTXNOTH.POREFDT == null ? "" : TTXNOTH.POREFDT.ToString().retDateStr(), VE.T_TXN.BLAMT.retDbl(),
                                "", "", "");
                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                    }

                    if (blgstpost == true)
                    {
                        #region TVCHGST Table update    

                        int gs = 0;
                        //  Posting of GST
                        double amthsnamt = 0, amtigst = 0, amtcgst = 0, amtsgst = 0, amtcess = 0, amtgstper = 0, mult = 1;
                        double rplamt = 0, rpligst = 0, rplcgst = 0, rplsgst = 0, rplcess = 0;
                        double balamt = 0, baligst = 0, balcgst = 0, balsgst = 0, balcess = 0;
                        double amtigstper = 0, amtcgstper = 0, amtsgstper = 0;

                        if (VE.TTXNAMT != null)
                        {
                            for (int i = 0; i <= VE.TTXNAMT.Count - 1; i++)
                            {
                                if (VE.TTXNAMT[i].AMT != 0 && VE.TTXNAMT[i].HSNCODE == null)
                                {
                                    if (VE.TTXNAMT[i].ADDLESS == "A") mult = 1; else mult = -1;
                                    amthsnamt = amthsnamt + (VE.TTXNAMT[i].AMT.retDbl() * mult);
                                    amtigst = amtigst + (VE.TTXNAMT[i].IGSTAMT.retDbl() * mult);
                                    amtcgst = amtcgst + (VE.TTXNAMT[i].CGSTAMT.retDbl() * mult);
                                    amtsgst = amtsgst + (VE.TTXNAMT[i].SGSTAMT.retDbl() * mult);
                                    amtcess = amtcess + (VE.TTXNAMT[i].CESSAMT.retDbl() * mult);
                                    amtigstper = VE.TTXNAMT[i].IGSTPER.retDbl();
                                    amtcgstper = VE.TTXNAMT[i].CGSTPER.retDbl();
                                    amtsgstper = VE.TTXNAMT[i].SGSTPER.retDbl();
                                }
                            }
                            if (amthsnamt != 0)
                            {
                                balamt = amthsnamt; baligst = amtigst; balcgst = amtcgst; balsgst = amtsgst; amtcess = balcess;
                                lastitemno = 0; titamt = 0;
                                for (int i = 0; i <= VE.TsalePos_TBATCHDTL.Count - 1; i++)
                                {
                                    if (VE.TsalePos_TBATCHDTL[i].SLNO != 0 && VE.TsalePos_TBATCHDTL[i].ITCD != null && VE.TsalePos_TBATCHDTL[i].IGSTPER == amtigstper && VE.TsalePos_TBATCHDTL[i].CGSTPER == amtcgstper && VE.TsalePos_TBATCHDTL[i].SGSTPER == amtsgstper)
                                    {
                                        titamt = titamt + VE.TsalePos_TBATCHDTL[i].GROSSAMT.retDbl();
                                        lastitemno = i;
                                    }
                                }

                                for (int i = 0; i <= VE.TsalePos_TBATCHDTL.Count - 1; i++)
                                {
                                    if (VE.TsalePos_TBATCHDTL[i].SLNO != 0 && VE.TsalePos_TBATCHDTL[i].ITCD != null && VE.TsalePos_TBATCHDTL[i].IGSTPER == amtigstper && VE.TsalePos_TBATCHDTL[i].CGSTPER == amtcgstper && VE.TsalePos_TBATCHDTL[i].SGSTPER == amtsgstper)
                                    {
                                        if (i == lastitemno)
                                        {
                                            rplamt = balamt; rpligst = baligst; rplcgst = balcgst; rplsgst = balsgst; rplcess = balcess;
                                        }
                                        else
                                        {
                                            rplamt = ((amthsnamt / titamt) * VE.TsalePos_TBATCHDTL[i].GROSSAMT).retDbl().toRound();
                                            rpligst = ((amtigst / titamt) * VE.TsalePos_TBATCHDTL[i].GROSSAMT).retDbl().toRound();
                                            rplcgst = ((amtcgst / titamt) * VE.TsalePos_TBATCHDTL[i].GROSSAMT).retDbl().toRound();
                                            rplsgst = ((amtsgst / titamt) * VE.TsalePos_TBATCHDTL[i].GROSSAMT).retDbl().toRound();
                                            rplcess = ((amtcess / titamt) * VE.TsalePos_TBATCHDTL[i].GROSSAMT).retDbl().toRound();
                                        }
                                        balamt = balamt - rplamt;
                                        baligst = baligst - rpligst;
                                        balcgst = balcgst - rplcgst;
                                        balsgst = balsgst - rplsgst;
                                        balcess = balcess - rplcess;

                                        VE.TsalePos_TBATCHDTL[i].NETAMT = VE.TsalePos_TBATCHDTL[i].NETAMT + rplamt;
                                        VE.TsalePos_TBATCHDTL[i].IGSTAMT = VE.TsalePos_TBATCHDTL[i].IGSTAMT + rpligst;
                                        VE.TsalePos_TBATCHDTL[i].CGSTAMT = VE.TsalePos_TBATCHDTL[i].CGSTAMT + rplcgst;
                                        VE.TsalePos_TBATCHDTL[i].SGSTAMT = VE.TsalePos_TBATCHDTL[i].SGSTAMT + rplsgst;
                                        VE.TsalePos_TBATCHDTL[i].CESSAMT = VE.TsalePos_TBATCHDTL[i].CESSAMT + rplcess;
                                    }
                                }
                            }
                        }

                        string dncntag = ""; string exemptype = "";
                        //if (VE.MENU_PARA == "SR" || VE.MENU_PARA == "SRPOS") dncntag = "SC";
                        //if (VE.MENU_PARA == "PR") dncntag = "PD";
                        double gblamt = TTXN.BLAMT.retDbl(); double groamt = TTXN.ROAMT.retDbl() + TTXN.TCSAMT.retDbl();
                        for (int i = 0; i <= VE.TsalePos_TBATCHDTL.Count - 1; i++)
                        {
                            if (VE.TsalePos_TBATCHDTL[i].SLNO != 0 && VE.TsalePos_TBATCHDTL[i].ITCD != null)
                            {
                                gs = gs + 1;
                                if (VE.TsalePos_TBATCHDTL[i].GSTPER.retDbl() == 0) exemptype = "N";
                                string SHIPDOCDT = VE.T_VCH_GST.SHIPDOCDT.retStr() == "" ? "" : VE.T_VCH_GST.SHIPDOCDT.retDateStr();
                                dbsql = masterHelp.InsVch_GST(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, cr,
                                        Convert.ToSByte(isl), Convert.ToSByte(gs), TTXN.SLCD, strblno, strbldt, VE.TsalePos_TBATCHDTL[i].HSNCODE, VE.TsalePos_TBATCHDTL[i].ITNM, (VE.TsalePos_TBATCHDTL[i].BLQNTY == null || VE.TsalePos_TBATCHDTL[i].BLQNTY == 0 ? VE.TsalePos_TBATCHDTL[i].QNTY.retDbl() : VE.TsalePos_TBATCHDTL[i].BLQNTY.retDbl()), (VE.TsalePos_TBATCHDTL[i].UOM == null ? VE.TsalePos_TBATCHDTL[i].UOM : VE.TsalePos_TBATCHDTL[i].UOM),
                                        VE.TsalePos_TBATCHDTL[i].NETAMT.retDbl(), VE.TsalePos_TBATCHDTL[i].IGSTPER.retDbl(), VE.TsalePos_TBATCHDTL[i].IGSTAMT.retDbl(), VE.TsalePos_TBATCHDTL[i].CGSTPER.retDbl(), VE.TsalePos_TBATCHDTL[i].CGSTAMT.retDbl(), VE.TsalePos_TBATCHDTL[i].SGSTPER.retDbl(), VE.TsalePos_TBATCHDTL[i].SGSTAMT.retDbl(),
                                        VE.TsalePos_TBATCHDTL[i].CESSPER.retDbl(), VE.TsalePos_TBATCHDTL[i].CESSAMT.retDbl(), salpur, groamt, gblamt, 0, (VE.T_VCH_GST.INVTYPECD == null ? "01" : VE.T_VCH_GST.INVTYPECD), VE.POS, VE.TsalePos_TBATCHDTL[i].AGDOCNO.retStr(), VE.TsalePos_TBATCHDTL[i].AGDOCDT.retDateStr(), TTXNOTH.DNCNCD,
                                        VE.GSTSLNM, VE.GSTNO,VE.GSTSLADD1, VE.GSTSLDIST, VE.GSTSLPIN, VE.T_VCH_GST.EXPCD, VE.T_VCH_GST.SHIPDOCNO, SHIPDOCDT, VE.T_VCH_GST.PORTCD, dncntag, TTXN.CONSLCD, exemptype, 0, expglcd, "Y");
                                OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                gblamt = 0; groamt = 0;
                            }
                        }
                        if (VE.TTXNAMT != null)
                        {
                            for (int i = 0; i <= VE.TTXNAMT.Count - 1; i++)
                            {
                                if (VE.TTXNAMT[i].SLNO != 0 && VE.TTXNAMT[i].AMTCD != null && VE.TTXNAMT[i].AMT != 0 && VE.TTXNAMT[i].HSNCODE != null)
                                {
                                    gs = gs + 1;
                                    string SHIPDOCDT = VE.T_VCH_GST.SHIPDOCDT.retStr() == "" ? "" : VE.T_VCH_GST.SHIPDOCDT.retDateStr();
                                    dbsql = masterHelp.InsVch_GST(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, cr,
                                            Convert.ToSByte(isl), Convert.ToSByte(gs), TTXN.SLCD, strblno, strbldt, VE.TTXNAMT[i].HSNCODE, VE.TTXNAMT[i].AMTNM, 0, "OTH",
                                            VE.TTXNAMT[i].AMT.retDbl(), VE.TTXNAMT[i].IGSTPER.retDbl(), VE.TTXNAMT[i].IGSTAMT.retDbl(), VE.TTXNAMT[i].CGSTPER.retDbl(), VE.TTXNAMT[i].CGSTAMT.retDbl(), VE.TTXNAMT[i].SGSTPER.retDbl(), VE.TTXNAMT[i].SGSTAMT.retDbl(),
                                            VE.TTXNAMT[i].CESSPER.retDbl(), VE.TTXNAMT[i].CESSAMT.retDbl(), salpur, groamt, gblamt, 0, (VE.T_VCH_GST.INVTYPECD == null ? "01" : VE.T_VCH_GST.INVTYPECD), VE.POS, VE.TsalePos_TBATCHDTL[0].AGDOCNO.retStr(), VE.TsalePos_TBATCHDTL[0].AGDOCDT.retDateStr(), TTXNOTH.DNCNCD,
                                            VE.GSTSLNM, VE.GSTNO, VE.GSTSLADD1, VE.GSTSLDIST, VE.GSTSLPIN, VE.T_VCH_GST.EXPCD, VE.T_VCH_GST.SHIPDOCNO, SHIPDOCDT, VE.T_VCH_GST.PORTCD, dncntag, TTXN.CONSLCD, exemptype, 0, VE.TTXNAMT[i].GLCD, "Y");
                                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                    gblamt = 0; groamt = 0;
                                }
                            }
                        }
                        #endregion
                    }

                    //if (Math.Round(dbDrAmt, 2) != Math.Round(dbCrAmt, 2))
                    //{
                    //    dberrmsg = "Debit " + Math.Round(dbDrAmt, 2) + " & Credit " + Math.Round(dbCrAmt, 2) + " not matching please click on rounded off...";
                    //    goto dbnotsave;
                    //}

                    #endregion
                    if (VE.DefaultAction == "A")
                    {
                        ContentFlg = "1" + " (Voucher No. " + DOCPATTERN + ")~" + TTXN.AUTONO;
                    }
                    else if (VE.DefaultAction == "E")
                    {
                        ContentFlg = "2";
                    }
                    OraTrans.Commit();
                    OraCon.Dispose();
                    return ContentFlg;
                }
                else if (VE.DefaultAction == "V")
                {
                    dbsql = masterHelp.TblUpdt("T_TXNPYMT_HDR", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("T_TXNMEMO", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("T_TXNTRANS", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_txnoth", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("T_CNTRL_HDR_UNIQNO", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_batchdtl", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    
                        
                    dbsql = masterHelp.TblUpdt("t_txnslsmn", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_txnpymt", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_txnamt", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_txn_linkno", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_txndtl", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_txn", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                    dbsql = masterHelp.TblUpdt("t_cntrl_doc_pass", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc_dtl", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_cntrl_hdr_rem", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                    //   Delete from financial schema
                    dbsql = masterHelp.TblUpdt("t_cntrl_auth", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.finTblUpdt("t_vch_gst", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_vch_bl", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_vch_class", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_vch_det", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_vch_hdr", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_cntrl_hdr", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                    dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(VE.T_TXN.AUTONO, "D", "S", null, null, null, VE.T_TXN.DOCDT.retStr(), null, null, null);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                    OraTrans.Commit();
                    OraCon.Dispose();
                    return "3";
                }
                goto dbok;
                dbnotsave:;
                OraTrans.Rollback();
                return dberrmsg;
                dbok:;
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                OraTrans.Rollback();
                OraCon.Dispose();
                return (ex.Message + ex.InnerException);
            }
            return "";
        }

    }
}