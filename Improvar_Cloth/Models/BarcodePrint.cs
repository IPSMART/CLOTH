using System.ComponentModel.DataAnnotations;

namespace Improvar.Models
{
    public class BarcodePrint
    {
        public short? EMD_NO { get; set; }
        public string CLCD { get; set; }
        public string DTAG { get; set; }
        public string TTAG { get; set; }
        public string AUTONO { get; set; }
        public string TAXSLNO { get; set; }
        public string BARNO { get; set; }
        public string ITGRPNM { get; set; }
        public string FABITNM { get; set; }
        public string STYLENO { get; set; }
        public string ITSTYLE { get; set; }
        public string NOS { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? WPRATE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? CPRATE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? RPRATE { get; set; }
        public string MTR { get; set; }
        public string ITGRPSHORTNM { get; set; }
        public string DESIGN { get; set; }
        public string PDESIGN { get; set; }
        public string COLRNM { get; set; }
        public string SIZENM { get; set; }
        public string WPPRICE { get; set; }
        public string WPPRICECODE { get; set; }
        public string RPPRICE { get; set; }
        public string RPPRICECODE { get; set; }
        public string COST { get; set; }
        public string COSTCODE { get; set; }
        public string DOCNO { get; set; }
        public string DOCDT { get; set; }
        public string PREFNO { get; set; }
        public string PREFDT { get; set; }
        public string DOCDTCODE { get; set; }
        public string COMPINIT { get; set; }
        public string GRPNM { get; set; }
        public string ITREM { get; set; }
        public string PARTNM { get; set; }
        public string SIZECD { get; set; }
        public string UOMCD { get; set; }
        public string QNTY { get; set; }
        public string DOCPRFX { get; set; }

        public bool Checked { get; set; }
    }
}