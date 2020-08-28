using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class TTXNAMT
    {
        public short? EMD_NO { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(30)]
        public string AUTONO { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short SLNO { get; set; }

        [Required]
        [StringLength(4)]
        public string AMTCD { get; set; }

        [StringLength(50)]
        public string AMTDESC { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double AMTRATE { get; set; }

        [StringLength(8)]
        public string HSNCODE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? AMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? CURR_AMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? IGSTAMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? IGSTPER { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? CGSTAMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? CGSTPER { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? SGSTAMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? SGSTPER { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? CESSAMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? CESSPER { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? DUTYAMT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? DUTYPER { get; set; }
        public string GLCD { get; set; }
        public string ADDLESS { get; set; }
        public string AMTNM { get; set; }
        public string CALCCODE { get; set; }
        public string CALCFORMULA { get; set; }
        public string CALCTYPE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? NETAMT { get; set; }
    }
}