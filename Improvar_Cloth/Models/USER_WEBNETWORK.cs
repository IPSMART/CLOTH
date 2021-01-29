namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("USER_WEBNETWORK")]
    public partial class USER_WEBNETWORK
    {
        [Key]
        [StringLength(15)]
        public string STATICIP { get; set; }
    }
}
