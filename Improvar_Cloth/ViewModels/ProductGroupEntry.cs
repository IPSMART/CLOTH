using Improvar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace Improvar.ViewModels
{
    public class ProductGroupEntry : Permission
    {
        public M_PRODGRP M_PRODGRP { get; set; }
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }

        public bool Checked { get; set; }
    }
}