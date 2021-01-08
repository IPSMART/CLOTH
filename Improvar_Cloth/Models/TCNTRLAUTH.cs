using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class TCNTRLAUTH
    {
        [StringLength(10)]
        public string DOCTYPE { get; set; }
        public int? EMD_NO { get; set; }

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
        public byte SLNO { get; set; }

        public byte? PASS_LEVEL { get; set; }

        [StringLength(100)]
        public string AUTH_REM { get; set; }

        [StringLength(1)]
        public string DOCPASSED { get; set; }

        [StringLength(40)]
        public string USR_ID { get; set; }

        public string USR_ENTDT { get; set; }

        [StringLength(15)]
        public string USR_LIP { get; set; }

        [StringLength(15)]
        public string USR_SIP { get; set; }

        [StringLength(50)]
        public string USR_OS { get; set; }

        [StringLength(50)]
        public string USR_MNM { get; set; }
        public string DNAME { get; set; }
        public string DOCCD { get; set; }
        public string DOCNO { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public string DOCDT { get; set; }
        public string SLCD { get; set; }
        public string SLNM { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double DOCAMT { get; set; }
        public bool Approved { get; set; }
        public bool UnApproved { get; set; }
        public bool Cancel { get; set; }
        public string MENU_PROGCALL { get; set; }
        public string MENU_PARA { get; set; }
        public string AUTHREM { get; set; }
        public int AUTH_SLNO { get; set; }
        [StringLength(8)]
        public string GLCD { get; set; }
        [StringLength(45)]
        public string GLNM { get; set; }
        public string LOCCD { get; set; }
        [StringLength(50)]
        public string AUTH_MNM { get; set; }
    }
}