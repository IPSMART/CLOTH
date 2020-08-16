using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class MSUBLEGCONT
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
        [StringLength(8)]
        public string SLCD { get; set; }

        [Key]
        [Column(Order = 1)]
        public byte SLNO { get; set; }

        [Required]
        [StringLength(60)]
        public string CPERSON { get; set; }

        [StringLength(45)]
        public string DESIG { get; set; }

        public int? EXTENSION { get; set; }

        public byte? MOBILE1PREFIX { get; set; }

        public long? MOBILE1 { get; set; }

        public byte? MOBILE2PREFIX { get; set; }

        public long? MOBILE2 { get; set; }

        public int? PHNO1STD { get; set; }

        public long? PHNO1 { get; set; }

        [StringLength(60)]
        public string PERSEMAIL { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? PERSDOB { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? PERSDOA { get; set; }

        [Required]
        [StringLength(40)]
        public string USR_ID { get; set; }

        public DateTime? USR_ENTDT { get; set; }

        [Required]
        [StringLength(15)]
        public string USR_LIP { get; set; }

        [Required]
        [StringLength(15)]
        public string USR_SIP { get; set; }

        [StringLength(50)]
        public string USR_OS { get; set; }

        [StringLength(50)]
        public string USR_MNM { get; set; }

        [StringLength(40)]
        public string LM_USR_ID { get; set; }

        public DateTime? LM_USR_ENTDT { get; set; }

        [StringLength(15)]
        public string LM_USR_LIP { get; set; }

        [StringLength(15)]
        public string LM_USR_SIP { get; set; }

        [StringLength(50)]
        public string LM_USR_OS { get; set; }

        [StringLength(50)]
        public string LM_USR_MNM { get; set; }

        [StringLength(40)]
        public string DEL_USR_ID { get; set; }

        public DateTime? DEL_USR_ENTDT { get; set; }

        [StringLength(15)]
        public string DEL_USR_LIP { get; set; }

        [StringLength(15)]
        public string DEL_USR_SIP { get; set; }

        [StringLength(50)]
        public string DEL_USR_OS { get; set; }

        [StringLength(50)]
        public string DEL_USR_MNM { get; set; }
        public bool Checked { get; set; }
        [StringLength(1)]
        public string DEPT { get; set; }
        public List<Designation> Designation { get; set; }
    }
}