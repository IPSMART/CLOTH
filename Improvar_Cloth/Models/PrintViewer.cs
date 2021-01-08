using System;
using System.Collections.Generic;
using System.Data;

namespace Improvar.Models
{
    public class PrintViewer:Permission
    {
        public PrintViewer()
        {
            NOPage = 1;
            Title = "";
            Para1 = "";
            Para2 = "";
            StaticFooter = Title + " - " + DateTime.Now.Date.ToShortDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt");
            Off_Floting = true;
            AlternetiveRowColor = false;
        }
        public int NOPage { get; set; }
        public string Title { get; set; }
        public string Para1 { get; set; }
        public string Para2 { get; set; }
        public string StaticFooter { get; set; }
        public string SpanColumnContaint { get; set; }
        public string SpanColumnLength { get; set; }
        public string SpanColumnSetIndex { get; set; }
        public bool RepetedHeader { get; set; }
        public string ReportName { get; set; }
        public bool Portrait { get; set; }
        public bool Off_Floting { get; set; }
        public bool AlternetiveRowColor { get; set; }
        public string Vname { get; set; }
        public int ColspanForPDF { get; set; }
        public bool PreDrawing { get; set; }
        public string PreDrawingTable { get; set; }
        public string FreezInnerWidth { get; set; }
        public List<ReportContaint> SetReportContaint { get; set; }
        public DataTable IR { get; set; }
        public string[,] HeaderArray { get; set; }
        public long TotReportWidth { get; set; }
    }
}