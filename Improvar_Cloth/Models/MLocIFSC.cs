using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public partial class MLocIFSC
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
         public double SLNO { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(4)]
        public string LOCCD { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(4)]
        public string COMPCD { get; set; }

        [Required]
        [StringLength(15)]
        public string IFSCCODE { get; set; }

        [Required]
        [StringLength(20)]
        public string BANKACTNO { get; set; }

        [StringLength(2)]
        public string BANKACTTYPE { get; set; }

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
        public string BANKNAME { get; set; }
        public string BRANCH { get; set; }
        public string CITY { get; set; }


    }
}