using Improvar.Models;
using Improvar.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Improvar.Controllers
{
    public class PrintPageController : Controller
    {
        // GET: PrintPage
        public ActionResult PrintPage_Landscape()
        {

            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                PrintPage VE = new PrintPage();
                VE = (PrintPage)TempData["DataList"];// cast tempdata to List of string
             //   VE.PrintingPageContent = Tmplst;
                return View(VE);
            }
        }
    }
}