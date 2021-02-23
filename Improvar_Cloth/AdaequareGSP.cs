using Improvar.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;

namespace Improvar
{
    public class AdaequareGSP
    {
        public String AppType; //"LIVE"/"STAGING"
        public String GSTTYPE; //INV/TIN/RET/TRE
        public String IPSAPICODE; //LENGTH=10
        public String AuthorizationToken; //
        public string sql = null;
        public string url = null;
        public Connection Cn = new Connection();
        public MasterHelp masterHelp = new MasterHelp();
        public string UNQSNO = CommVar.getQueryStringUNQSNO();

        public AdaequareGSP()
        {//Technique of Association with default constructor
            string gsptknexpin = "", gspappid = "", gspappsecret = "";
            sql = "select gspaccesstkn,gsptknexpin,gspappsecret,gspappid,LIVE_STAGING from MS_IPSMART where GSPENABLE='Y' and clcd='" + CommVar.ClientCode(UNQSNO) + "' ";
            var dt = masterHelp.SQLquery(sql);
            if (dt.Rows.Count > 0)
            {
                AppType = dt.Rows[0]["LIVE_STAGING"].ToString() == "L" ? "LIVE" : "STAGING";
                AuthorizationToken = dt.Rows[0]["gspaccesstkn"].ToString();
                gsptknexpin = dt.Rows[0]["gsptknexpin"].retDateStr();
                if (gsptknexpin != "" && Convert.ToDateTime(gsptknexpin) > System.DateTime.Today)
                {
                    AuthorizationToken = "Bearer " + AuthorizationToken; return;
                }
                gspappid = dt.Rows[0]["gspappid"].ToString().Replace(System.Environment.NewLine, string.Empty);//appid and client id are same
                gspappsecret = dt.Rows[0]["gspappsecret"].ToString().Replace(System.Environment.NewLine, string.Empty);
            }
            else
            {
                Cn.SaveTextFile("Please add a row in MS_IPSMART table.");
                return;
            }
            IPSAPICODE = "AUTHENTICA";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("gspappid", gspappid);
            dic.Add("gspappsecret", gspappsecret);
            var jsonstr = "";// JsonConvert.SerializeObject(dic);
            jsonstr = ConsumeAdqrAPI("https://gsp.adaequare.com/gsp/authenticate?action=GSP&grant_type=token", " ", dic);
            AdqrAuthenticate adqrAuthenticate = JsonConvert.DeserializeObject<AdqrAuthenticate>(jsonstr);
            sql = "update MS_IPSMART set gspaccesstkn='" + adqrAuthenticate.access_token + "', gsptkntype='" + adqrAuthenticate.token_type
                  + "', gsptknexpin=to_date('" + (System.DateTime.Today.AddDays(28)).retDateStr() + "', 'dd/mm/yyyy')"
                + ", gsptknexpin_no=" + adqrAuthenticate.expires_in + ", gspscope='" + adqrAuthenticate.scope + "', gspjti='" + adqrAuthenticate.jti + "' ";
            sql += " where GSPENABLE='Y' and clcd='" + CommVar.ClientCode(UNQSNO) + "' ";
            masterHelp.SQLNonQuery(sql);
            AuthorizationToken = "Bearer " + adqrAuthenticate.access_token;
        }
        public string GetRequestId()
        {
            return CommVar.ClientCode(UNQSNO) + System.DateTime.Now.ToString("yyMMddHHmmssfff") + Cn.GenerateOTP(true, 3);
        }

        #region //E-Invoice
        public Dictionary<string, string> AdaequareIRNHeader(Dictionary<string, string> optHeader = null)
        {
            GSTTYPE = "TIN";
            if (AppType == "LIVE") GSTTYPE = "INV";
            string gstno = CommVar.GSTNO(UNQSNO);
            if (CommVar.FinSchema(UNQSNO) == "")
            {
                sql = "select gstuid,gstpw from M_compgstin where gstno='" + gstno + "' and GSTTYPE='" + GSTTYPE + "' ";
            }
            else
            {
                sql = "select gstuid,gstpw from  " + CommVar.FinSchema(UNQSNO) + ".M_compgstin where gstno='" + gstno + "'  and GSTTYPE='" + GSTTYPE + "' ";
            }
            var dt = masterHelp.SQLquery(sql);
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (dt.Rows.Count > 0)
            {
                dic.Add("user_name", dt.Rows[0]["gstuid"].ToString());
                dic.Add("password", Cn.Decrypt_URL(dt.Rows[0]["gstpw"].ToString()));
            }
            else
            {
                Cn.SaveTextFile(GSTTYPE + " Please add a row in M_compgstin table. #" + gstno);
                return null;
            }
            dic.Add("GSTIN", gstno);
            dic.Add("requestid", GetRequestId());
            dic.Add("Authorization", AuthorizationToken);
            if (optHeader == null) optHeader = new Dictionary<string, string>();
            foreach (var item in optHeader)
            {
                dic.Add(item.Key, item.Value);
            }
            return dic;
        }

        public AdqrRespGenIRN AdqrGenIRN(string jsonstr)
        {
            IPSAPICODE = "IRNGENERAT";
            Dictionary<string, string> AdaequareIRNHdr = AdaequareIRNHeader();
            url = "https://gsp.adaequare.com/test/enriched/ei/api/invoice";
            if (AppType == "LIVE") url = "https://gsp.adaequare.com/enriched/ei/api/invoice";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            jsonstr = ConsumeAdqrAPI(url, jsonstr, AdaequareIRNHdr);
            AdqrRespGenIRN adqrRespGENEWAYBILL = new AdqrRespGenIRN();
            AdqrRsltGenIRN adqrRsltGenIRN = new AdqrRsltGenIRN();
            if (jsonstr == "") return adqrRespGENEWAYBILL;
            try
            {
                adqrRespGENEWAYBILL = JsonConvert.DeserializeObject<AdqrRespGenIRN>(jsonstr);
            }
            catch
            {
                try
                {
                    AdqrRespGenIRNfail GenIRNfail = JsonConvert.DeserializeObject<AdqrRespGenIRNfail>(jsonstr);
                    adqrRespGENEWAYBILL.success = GenIRNfail.success;
                    adqrRespGENEWAYBILL.message = GenIRNfail.message;
                    if (adqrRespGENEWAYBILL.message == "2150 : Duplicate IRN")
                    {
                        if (GenIRNfail.result != null && GenIRNfail.result[0] != null && GenIRNfail.result[0].Desc != null && GenIRNfail.result[0].Desc.Irn != null)
                        {
                            adqrRsltGenIRN.Irn = GenIRNfail.result[0].Desc.Irn;
                            adqrRsltGenIRN.AckNo = GenIRNfail.result[0].Desc.AckNo;
                            adqrRsltGenIRN.AckDt = GenIRNfail.result[0].Desc.AckDt;
                            AdqrRespInvoiceByIRN InvByIRN = AdqrGetInvoicebyIRN(adqrRsltGenIRN.Irn);
                            if (InvByIRN != null && InvByIRN.result != null )
                            {
                                adqrRsltGenIRN.SignedInvoice = InvByIRN.result.SignedInvoice;
                                adqrRsltGenIRN.SignedQRCode = InvByIRN.result.SignedQRCode;
                                adqrRsltGenIRN.EwbNo = InvByIRN.result.EwbNo;
                                adqrRsltGenIRN.EwbDt = InvByIRN.result.EwbDt;
                                adqrRsltGenIRN.EwbValidTill = InvByIRN.result.EwbValidTill;
                            }
                        }
                    }

                    adqrRespGENEWAYBILL.result = adqrRsltGenIRN;
                }
                catch
                {

                }
            }
            return adqrRespGENEWAYBILL;
        }
        public AdqrRespGenIRNCancel AdqrGenIRNCancel(string jsonstr)
        {
            IPSAPICODE = "IRNCANCEL";
            Dictionary<string, string> AdaequareIRNHdr = AdaequareIRNHeader();
            url = "https://gsp.adaequare.com/test/enriched/ei/api/invoice/cancel";
            if (AppType == "LIVE") url = "https://gsp.adaequare.com/enriched/ei/api/invoice/cancel";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            jsonstr = ConsumeAdqrAPI(url, jsonstr, AdaequareIRNHdr);
            AdqrRespGenIRNCancel adqrRespGenIRNCancel = JsonConvert.DeserializeObject<AdqrRespGenIRNCancel>(jsonstr);
            return adqrRespGenIRNCancel;
        }
        public string ValidateIRNdetails(AdaequareIRN AI)
        {
            string msg = "";
            try
            {
                // SellerDtls
                if (AI.SellerDtls.Addr1.retStr().Length > 100) { AI.SellerDtls.Addr1 = AI.SellerDtls.Addr1.Substring(0, 100); }
                if (AI.SellerDtls.Addr2.retStr().Length > 100) { AI.SellerDtls.Addr2 = AI.SellerDtls.Addr2.Substring(0, 100); }
                if (AI.SellerDtls.Addr2.Trim() == "") { AI.SellerDtls.Addr2 = "   "; }
                if (AI.SellerDtls.Ph.IndexOf(" ") != -1 || AI.SellerDtls.Ph.IndexOf("/") != -1 || AI.SellerDtls.Ph.IndexOf("-") != -1 || AI.SellerDtls.Ph.Length < 6 || AI.SellerDtls.Ph.Length > 12)
                { msg += "Seller Phone Number(" + AI.SellerDtls.Ph + ") minimum length of 6 and a maximum length of 12,Should not contain any special char. "; }
                if (AI.SellerDtls.Pin < 100000 || AI.SellerDtls.Pin > 999999) { msg += "SellerDtls.Pin should be 6 digit; "; }
                if (string.IsNullOrEmpty(AI.SellerDtls.Addr1)) msg += "SellerDtls(" + AI.SellerDtls.Addr1 + ")Addr1 should not empty. ";
                if (string.IsNullOrEmpty(AI.SellerDtls.Ph) || CommFunc.IsValidPhone(AI.SellerDtls.Ph) == false) { AI.SellerDtls.Ph = null; }
                if (string.IsNullOrEmpty(AI.SellerDtls.Em) || CommFunc.IsValidEmail(AI.SellerDtls.Em) == false) { AI.SellerDtls.Em = null; }

                // BuyerDtls
                if (AI.BuyerDtls.Addr1.retStr().Length > 100) { AI.BuyerDtls.Addr1 = AI.BuyerDtls.Addr1.Substring(0, 100); }
                if (AI.BuyerDtls.Addr2.retStr().Length > 100) { AI.BuyerDtls.Addr2 = AI.BuyerDtls.Addr2.Substring(0, 100); }
                if (string.IsNullOrEmpty(AI.BuyerDtls.Gstin)) { AI.BuyerDtls.Gstin = null; }
                if (string.IsNullOrEmpty(AI.BuyerDtls.LglNm)) msg += "(" + AI.BuyerDtls.LglNm + ")BuyerDtls.LglNm should not empty. ";
                if (string.IsNullOrEmpty(AI.BuyerDtls.TrdNm)) msg += "BuyerDtls(" + AI.BuyerDtls.TrdNm + ")TrdNm should not empty. ";
                if (string.IsNullOrEmpty(AI.BuyerDtls.Addr1)) msg += "BuyerDtls(" + AI.BuyerDtls.Addr1 + ")Addr1 should not empty. ";
                if (AI.BuyerDtls.Addr2.Trim() == "") { AI.BuyerDtls.Addr2 = "   "; }
                if (AI.BuyerDtls.Pin < 100000 || AI.BuyerDtls.Pin > 999999) { msg += "BuyerDtls(" + AI.BuyerDtls.Pin + ")Pin should be 6 digit; "; }
                if (string.IsNullOrEmpty(AI.BuyerDtls.Ph) || CommFunc.IsValidPhone(AI.BuyerDtls.Ph) == false) { AI.BuyerDtls.Ph = null; }
                if (string.IsNullOrEmpty(AI.BuyerDtls.Em) || CommFunc.IsValidEmail(AI.BuyerDtls.Em) == false) { AI.BuyerDtls.Em = null; }

                //DispDtls
                if (AI.DispDtls != null)
                {
                    if (AI.DispDtls.Addr1.retStr().Length > 100) { AI.DispDtls.Addr1 = AI.DispDtls.Addr1.Substring(0, 100); }
                    if (AI.DispDtls.Addr2.retStr().Length > 100) { AI.DispDtls.Addr2 = AI.DispDtls.Addr2.Substring(0, 100); }
                    if (AI.DispDtls.Addr1.Trim() == "") { msg += "DispDtls.Addr1 should not blank; "; }
                    if (AI.DispDtls.Loc.Trim() == "") { msg += "DispDtls.Location/Godown district should not blank. "; }
                    if (AI.DispDtls.Pin < 100000 || AI.DispDtls.Pin > 999999) { msg += "DispDtls(" + AI.DispDtls.Pin + ")Pin should be 6 digit; "; }
                    if (AI.DispDtls.Addr2.Trim() == "") { AI.DispDtls.Addr2 = "   "; }
                }

                //ShipDtlsLglNm
                if (AI.ShipDtls != null)
                {
                    if (string.IsNullOrEmpty(AI.ShipDtls.Gstin)) { AI.ShipDtls.Gstin = null; }
                    if (AI.ShipDtls.Loc.retStr().Trim() == "") { msg += "ShipDtls.Location/Godown district should not blank; "; }
                    if (AI.ShipDtls.Pin < 100000 || AI.ShipDtls.Pin > 999999) { msg += "ShipDtls(" + AI.ShipDtls.Pin + ")Pin should be 6 digit; "; }
                    if (AI.ShipDtls.Addr1.retStr().Trim() == "") { msg += "ShipDtls.Addr1 should not blank; "; }
                    if (AI.ShipDtls.Addr2.retStr().Trim() == "") { AI.ShipDtls.Addr2 = "   "; }
                    if (AI.ShipDtls.Addr1.retStr().Length > 100) { AI.ShipDtls.Addr1 = AI.ShipDtls.Addr1.Substring(0, 100); }
                    if (AI.ShipDtls.Addr2.retStr().Length > 100) { AI.ShipDtls.Addr2 = AI.ShipDtls.Addr2.Substring(0, 100); }
                }
                //ItemListAI
                for (int i = 0; i < AI.ItemList.Count; i++)
                {
                    if (string.IsNullOrEmpty(AI.ItemList[i].PrdDesc) || AI.ItemList[i].PrdDesc.Length < 3 || AI.ItemList[i].PrdDesc.Length > 100) msg += "PrdDesc/Itnm( " + AI.ItemList[i].PrdDesc + ") should not empty. AI.ItemList[i].PrdDesc.Length <3 || AI.ItemList[i].PrdDesc.Length > 100";
                    if (string.IsNullOrEmpty(AI.ItemList[i].HsnCd) || AI.ItemList[i].HsnCd.Length < 4 && AI.ItemList[i].HsnCd.Length > 8) msg += "The field HSN(" + AI.ItemList[i].HsnCd + ") Code must be a string with a minimum length of 4 and a maximum length of 8.. ";
                    if (string.IsNullOrEmpty(AI.ItemList[i].Unit) || AI.ItemList[i].Unit.Length < 3 && AI.ItemList[i].Unit.Length > 8) msg += "The UOM(" + AI.ItemList[i].Unit + ") must be a string with a minimum length of 3 and a maximum length of 8.. ";
                }

                //ShipDtlsLglNm
                //if (string.IsNullOrEmpty(VE.ShipDtls.Gstin)) { VE.ShipDtls.Gstin = null; }
                if (msg == "") msg = "ok";
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                msg = ex.Message;
            }
            return msg;
        }
        public AdqrRespIRNEWB AdqrGenIRNtoEWB(string jsonstr)
        {
            IPSAPICODE = "IRNTOEWBGN";
            Dictionary<string, string> AdaequareIRNHdr = AdaequareIRNHeader();
            url = "https://gsp.adaequare.com/test/enriched/ei/api/ewaybill";
            if (AppType == "LIVE") url = "https://gsp.adaequare.com/enriched/ei/api/ewaybill";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            jsonstr = ConsumeAdqrAPI(url, jsonstr, AdaequareIRNHdr);
            AdqrRespIRNEWB adqrRespGenIRNEWB = JsonConvert.DeserializeObject<AdqrRespIRNEWB>(jsonstr);
            return adqrRespGenIRNEWB;
        }
        public string AdqrSaveQRimage(string IRN)
        {
            IPSAPICODE = "QRIMAGE";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("IRN", IRN);
            Dictionary<string, string> AdaequareIRNHdr = AdaequareIRNHeader(dic);
            url = "https://gsp.adaequare.com/test/enriched/ei/others/qr/image";
            if (AppType == "LIVE") url = "https://gsp.adaequare.com/enriched/ei/others/qr/image";
            string SavePNGpath = "C:/IPSMART/IRNQrcode/" + IRN + ".png";
            var response = DownloadAdqrAPI(url, "", AdaequareIRNHdr, SavePNGpath);
            if (response != null)
            {
                return "ok";
            }
            else
            {
                return "";
            }
        }
        public AdqrRespExtractInvoice AdqrExtractQR(string jsonstr)
        {
            IPSAPICODE = "EXTRACTQR";
            url = "https://gsp.adaequare.com/test/enriched/ei/others/extract/qr";
            if (AppType == "LIVE") url = "https://gsp.adaequare.com/enriched/ei/others/extract/qr";
            jsonstr = ConsumeAdqrAPI(url, jsonstr, AdaequareIRNHeader());
            AdqrRespExtractInvoice adqrRespExtractInvoice = JsonConvert.DeserializeObject<AdqrRespExtractInvoice>(jsonstr);
            return adqrRespExtractInvoice;
        }
        public AdqrRespInvoiceByIRN AdqrGetInvoicebyIRN(string IRN)
        {//
            IPSAPICODE = "IRNDETAIL";
            url = "https://gsp.adaequare.com/test/enriched/ei/api/invoice/irn?irn=" + IRN + "";
            if (AppType == "LIVE") url = "https://gsp.adaequare.com/enriched/ei/api/invoice/irn?irn=" + IRN + "";
            var jsonstr = ConsumeAdqrAPI(url, "", AdaequareIRNHeader());
            AdqrRespInvoiceByIRN adqrRespExtractInvoice = JsonConvert.DeserializeObject<AdqrRespInvoiceByIRN>(jsonstr);
            return adqrRespExtractInvoice;
        }
        public AdqrRespIRNEWB AdqrGetEWBbyIRN(string IRN)
        {//
            IPSAPICODE = "EWBBYIRN";
            url = "https://gsp.adaequare.com/test/enriched/ei/api/ewaybill/irn?irn=" + IRN + "";
            if (AppType == "LIVE") url = "https://gsp.adaequare.com/enriched/ei/api/ewaybill/irn?irn=" + IRN + "";
            var jsonstr = ConsumeAdqrAPI(url, "", AdaequareIRNHeader());
            AdqrRespIRNEWB adqrRespExtractInvoice = JsonConvert.DeserializeObject<AdqrRespIRNEWB>(jsonstr);
            return adqrRespExtractInvoice;
        }

        public AdqrRespGstInfo AdqrGstInfo(string gstin)
        {
            IPSAPICODE = "GSTINFO";
            Dictionary<string, string> AdaequareIRNHdr = AdaequareIRNHeader();
            url = "https://gsp.adaequare.com/test/enriched/ei/api/master/gstin?gstin=" + gstin + "";
            if (AppType == "LIVE") url = "https://gsp.adaequare.com/enriched/ei/api/master/gstin?gstin=" + gstin + ""; ;
            string jsonstr = ConsumeAdqrAPI(url, "", AdaequareIRNHdr);
            AdqrRespGstInfo adqrRespExtractInvoice = JsonConvert.DeserializeObject<AdqrRespGstInfo>(jsonstr);
            return adqrRespExtractInvoice;
        }
        public void AdaequareGSPAuthenticateTest()
        {
            string gspappid = "", gspappsecret = "";
            sql = "select gspaccesstkn,gsptknexpin,gspappsecret,gspappid,LIVE_STAGING from MS_IPSMART where LIVE_STAGING='S' ";
            var dt = masterHelp.SQLquery(sql);
            if (dt.Rows.Count > 0)
            {
                AppType = "STAGING";
                gspappid = dt.Rows[0]["gspappid"].ToString().Replace(System.Environment.NewLine, string.Empty);
                gspappsecret = dt.Rows[0]["gspappsecret"].ToString().Replace(System.Environment.NewLine, string.Empty);
            }
            else
            {
                Cn.SaveTextFile("Please add a row in MS_IPSMART table.");
                return;
            }
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("gspappid", gspappid);
            dic.Add("gspappsecret", gspappsecret);
            var jsonstr = "";
            jsonstr = ConsumeAdqrAPI("https://gsp.adaequare.com/gsp/authenticate?action=GSP&grant_type=token", " ", dic);
            AdqrAuthenticate adqrAuthenticate = JsonConvert.DeserializeObject<AdqrAuthenticate>(jsonstr);
            AuthorizationToken = "Bearer " + adqrAuthenticate.access_token;
        }
        public Dictionary<string, string> AdaequareIRNHeaderTest(Dictionary<string, string> optHeader = null)
        {
            GSTTYPE = "TIN";
            sql = "select gstno,gstuid,gstpw from " + CommVar.FinSchema(UNQSNO) + ".M_compgstin where GSTTYPE='" + GSTTYPE + "' ";
            var dt = masterHelp.SQLquery(sql);
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (dt.Rows.Count > 0)
            {
                dic.Add("user_name", dt.Rows[0]["gstuid"].ToString());
                dic.Add("password", Cn.Decrypt_URL(dt.Rows[0]["gstpw"].ToString()));
                dic.Add("GSTIN", dt.Rows[0]["gstno"].ToString());
            }
            else
            {
                Cn.SaveTextFile(GSTTYPE + " Please add a row in M_compgstin table. #");
                return null;
            }
            dic.Add("requestid", GetRequestId());
            AdaequareGSPAuthenticateTest();
            dic.Add("Authorization", AuthorizationToken);
            if (optHeader == null) optHeader = new Dictionary<string, string>();
            foreach (var item in optHeader)
            {
                dic.Add(item.Key, item.Value);
            }
            return dic;
        }
        public AdqrRespGstInfo AdqrGstInfoTestMode(string gstin)
        {
            IPSAPICODE = "GSTINFO";
            Dictionary<string, string> AdaequareIRNHdr = AdaequareIRNHeaderTest();
            url = "https://gsp.adaequare.com/test/enriched/ei/api/master/gstin?gstin=" + gstin + "";
            string jsonstr = ConsumeAdqrAPI(url, "", AdaequareIRNHdr);
            AdqrRespGstInfo adqrRespExtractInvoice = JsonConvert.DeserializeObject<AdqrRespGstInfo>(jsonstr);
            return adqrRespExtractInvoice;
        }


        #endregion //E-Invoice end

        #region //E-Waybill
        public Dictionary<string, string> AdqrEWBHeader()
        {
            GSTTYPE = "TIN";
            if (AppType == "LIVE") GSTTYPE = "INV";
            string gstno = CommVar.GSTNO(UNQSNO);
            if (CommVar.FinSchema(UNQSNO) == "")
            {
                sql = "select gstuid,gstpw from M_compgstin where gstno='" + gstno + "'  and GSTTYPE='" + GSTTYPE + "'";
            }
            else
            {
                sql = "select gstuid,gstpw from  " + CommVar.FinSchema(UNQSNO) + ".M_compgstin where gstno='" + gstno + "' and GSTTYPE='" + GSTTYPE + "' ";
            }
            var dt = masterHelp.SQLquery(sql);
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (dt.Rows.Count > 0)
            {
                dic.Add("username", dt.Rows[0]["gstuid"].ToString());
                dic.Add("password", Cn.Decrypt_URL(dt.Rows[0]["gstpw"].ToString()));
            }
            else
            {
                Cn.SaveTextFile(GSTTYPE + " Please add a row in M_compgstin table. #" + gstno);
                return null;
            }
            dic.Add("GSTIN", gstno);
            dic.Add("requestid", GetRequestId());
            dic.Add("Authorization", AuthorizationToken);
            return dic;
        }

        public AdqrRespGENEWAYBILL AdqrGenEwayBill(string jsonstr)
        {
            IPSAPICODE = "EWBGEN";
            url = "https://gsp.adaequare.com/test/enriched/ewb/ewayapi?action=GENEWAYBILL";
            if (AppType == "LIVE") url = "https://gsp.adaequare.com/enriched/ewb/ewayapi?action=GENEWAYBILL";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            jsonstr = ConsumeAdqrAPI(url, jsonstr, AdqrEWBHeader());
            AdqrRespGENEWAYBILL adqrRespGENEWAYBILL = JsonConvert.DeserializeObject<AdqrRespGENEWAYBILL>(jsonstr);
            return adqrRespGENEWAYBILL;
        }
        #endregion //E-Waybill end

        public string ConsumeAdqrAPI(string url, string jsonStr, Dictionary<string, string> headerdic)
        {
            if (headerdic == null) return "";
            string resp = "", hdrString = "";
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                hdrString = url + System.Environment.NewLine;
                using (HttpClient client = new HttpClient())
                {
                    StringContent data = new StringContent(jsonStr, Encoding.UTF8, "application/json");
                    foreach (var item in headerdic)
                    {
                        client.DefaultRequestHeaders.Add(item.Key, item.Value);
                        hdrString += item.Key + " :" + item.Value + System.Environment.NewLine;
                    }
                    hdrString += jsonStr;
                    Cn.SaveTextFile(hdrString);
                    if (jsonStr == "")
                    {
                        response = client.PostAsync(url, data).GetAwaiter().GetResult(); //Make sure it is synchonrous  //   response = client.GetAsync(url).Result;
                    }
                    else
                    {
                        response = client.PostAsync(url, data).GetAwaiter().GetResult(); //Make sure it is synchonrous
                    }
                    resp = response.Content.ReadAsStringAsync().Result;//Make sure it is synchonrous
                    int StatusCode = (int)response.StatusCode;
                    Cn.SaveTextFile("Response: " + resp);
                    if (StatusCode > 200)
                    {
                        return "{\"message\":\"" + ErrorAdqrAPI(resp).Replace("\"", "") + "\"}";
                    }
                    response.EnsureSuccessStatusCode();
                    AdqrAccessLog("Y", resp);
                    return resp;
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, resp);
                return "{\"message\":\"" + ex.Message + "\"}";
            }
        }
        private string ErrorAdqrAPI(string response)
        {
            string resp = "";
            try
            {
                AdqrRespError adqrRespExtractInvoice = JsonConvert.DeserializeObject<AdqrRespError>(response);
                if (adqrRespExtractInvoice != null)
                {
                    if (adqrRespExtractInvoice.error == "invalid_token")
                    {
                        sql = "";
                        sql = "update MS_IPSMART set gspaccesstkn='', gsptknexpin=null where GSPENABLE='Y' and clcd='" + CommVar.ClientCode(UNQSNO) + "' ";
                        masterHelp.SQLNonQuery(sql);
                    }
                    AdqrAccessLog("N", adqrRespExtractInvoice.error_description);
                    return adqrRespExtractInvoice.error_description;
                }
            }
            catch
            {
            }
            return resp;
        }

        private void AdqrAccessLog(string Success, string message)
        {
            message = message.Replace("'", "");
            if (message.retStr().Length > 200)
            {
                message = message.Substring(0, 200);
            }
            if (IPSAPICODE.retStr().Length > 10)
            {
                IPSAPICODE = IPSAPICODE.Substring(0, 10);
            }
            sql = "";
            sql = " insert into USER_GSPLOG(CLIENT_CODE, GSTTYPE, GSTNO, APICODE, SUCCESS,MSG, USR_ID, USR_ENTDT, USR_LIP, USR_SIP)";
            sql += "values('" + CommVar.ClientCode(UNQSNO) + "','" + GSTTYPE + "','" + CommVar.GSTNO(UNQSNO) + "','" + IPSAPICODE + "','" + Success + "','" + message
                + "','" + CommVar.UserID() + "',SYSDATE,'" + Cn.GetIp() + "','" + Cn.GetStaticIp() + "')";
            masterHelp.SQLNonQuery(sql);
        }
        public byte[] DownloadAdqrAPI(string url, string jsonStr, Dictionary<string, string> headerdic, string SavePNGpath)
        {
            try
            {
                string hdrString = url + System.Environment.NewLine;
                using (HttpClient client = new HttpClient())
                {
                    StringContent data = new StringContent(jsonStr, Encoding.UTF8, "application/json");
                    foreach (var item in headerdic)
                    {
                        client.DefaultRequestHeaders.Add(item.Key, item.Value);
                        hdrString += item.Key + " : " + item.Value + System.Environment.NewLine;
                    }
                    hdrString += jsonStr;
                    Cn.SaveTextFile(hdrString);
                    HttpResponseMessage response = new HttpResponseMessage();
                    if (jsonStr == "")
                    {//get
                        response = client.GetAsync(url).Result; //Make sure it is synchonrous               
                    }
                    else
                    {
                        response = client.PostAsync(url, data).Result; //Make sure it is synchonrous
                    }
                    response.EnsureSuccessStatusCode();
                    byte[] byteImage = response.Content.ReadAsByteArrayAsync().Result;
                    if (!Directory.Exists(Path.GetDirectoryName(SavePNGpath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(SavePNGpath));
                    }
                    using (MemoryStream mem = new MemoryStream(byteImage))
                    {
                        using (var yourImage = Image.FromStream(mem))
                        {
                            yourImage.Save(SavePNGpath, ImageFormat.Png);
                        }
                    }
                    return byteImage; //Make sure it is synchonrous
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return null;
            }
        }
    }
}
public class AdqrAuthenticate
{
    public string access_token { get; set; }
    public string token_type { get; set; }
    public int expires_in { get; set; }
    public string scope { get; set; }
    public string jti { get; set; }
}
public class AdqrRsltGstInfo
{
    public string Gstin { get; set; }
    public string TradeName { get; set; }
    public string LegalName { get; set; }
    public string AddrBnm { get; set; }
    public string AddrBno { get; set; }
    public string AddrFlno { get; set; }
    public string AddrSt { get; set; }
    public string AddrLoc { get; set; }
    public int StateCode { get; set; }
    public int AddrPncd { get; set; }
    public string TxpType { get; set; }
    public string Status { get; set; }
    public object BlkStatus { get; set; }
}

public class AdqrRespGstInfo
{
    public bool success { get; set; }
    public string message { get; set; }
    public AdqrRsltGstInfo result { get; set; }
}
public class AdqrRespError
{
    public string error { get; set; }
    public string error_description { get; set; }
}