using Improvar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class TPHYSTK
    {
        public short? EMD_NO { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }
       
        [StringLength(30)]
        public string AUTONO { get; set; }
       
        public short SLNO { get; set; }

        [Required]
        [StringLength(25)]
        public string BARNO { get; set; }

        [Required]
        [StringLength(1)]
        public string STKTYPE { get; set; }

        [Required]
        [StringLength(2)]
        public string MTRLJOBCD { get; set; }

        [StringLength(4)]
        public string PARTCD { get; set; }     
        public decimal? NOS { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public decimal? QNTY { get; set; }

        [StringLength(100)]
        public string ITREM { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public decimal? RATE { get; set; }

        public decimal? DIA { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public decimal? CUTLENGTH { get; set; }

        [StringLength(10)]
        public string LOCABIN { get; set; }

        [StringLength(15)]
        public string SHADE { get; set; }

        [StringLength(4)]
        public string BALEYR { get; set; }

        [StringLength(30)]
        public string BALENO { get; set; }
        public string STYLENO { get; set; }
        public bool Checked { get; set; }
        public string ITCD { get; set; }
        public string ITSTYLE { get; set; }

    }
}