using System.Collections.Generic;
using Improvar.Models;
using System.ComponentModel.DataAnnotations;

namespace Improvar.ViewModels
{
    public class EWayBillReport:Permission
    {
        public string DATEFROM { get; set; }
        public string DATETO { get; set; }
        public List<EWAYBILL> EWAYBILL { get; set; }
        public string  OLD_NEW_FMT { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public bool Checkbox1 { get; set; }
        public bool Checkbox2 { get; set; }
        public string TEXTBOX1 { get; set; }
        public bool Checkbox3 { get; set; }
    }

    public class EWAYBILL {

        public short SLNO { get; set; }
        public string DOCNO { get; set; }
        public string DOCDT { get; set; }
        public string SLCD { get; set; }
        public string SLNM { get; set; }
        public string DISTRICT { get; set; }
        public string DISTANCE { get; set; }
        public string TRSLNM { get; set; }
        public string LRNO { get; set; }
        public string LRDT { get; set; }
        public string LORRYNO  { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double BLAMT { get; set; }
        public string AUTONO { get; set; }
        public bool Checked { get; set; }
        public string DOCCD { get; set; }
        public bool LORRYNOEXIST { get; set; }
        public string message { get; set; }
        public string EWBNO { get; set; }
        public string IRNNO { get; set; }

    }
}