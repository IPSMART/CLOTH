using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class MITEMPLISTDTL
    {
        public int? EMD_NO { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(4)]
        public string PRCCD { get; set; }

        [Key]
        [Column(Order = 1)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime EFFDT { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(4)]
        public string BRANDCD { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short SLNO { get; set; }

        [Required]
        [StringLength(10)]
        public string ITCD { get; set; }

        [StringLength(4)]
        public string SIZECD { get; set; }

        [StringLength(4)]
        public string COLRCD { get; set; }

         public double RATE { get; set; }

        public bool Checked { get; set; }
        [StringLength(60)]
        public string ITNM { get; set; }
        public short PCSPERBOX { get; set; }

        [StringLength(12)]
        public string STYLENO { get; set; }

        [StringLength(10)]
        public string SIZENM { get; set; }
        [StringLength(3)]
        public string PRINT_SEQ { get; set; }
        [StringLength(20)]
        public string COLRNM { get; set; }
        [StringLength(100)]
        public string AUTH_REM { get; set; }

        [StringLength(4)]
        public string SIZE1 { get; set; }
    }
}