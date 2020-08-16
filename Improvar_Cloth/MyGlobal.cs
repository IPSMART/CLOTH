using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar
{
    public static class MyGlobal
    {
        // for 01/04/2016 signature key="6IzNnGFfOkRlQ8CuPK8Eu8acRzs/+4WXjQi45YRn9SU="
        // for 01/04/2017 signature key="cAsRqTZdoAs5F36krFGjQJ0UggI2XQali69HuUhXDb0="
        public static string MyGlobalString { get; set; }
        public static string ReportName { get; set; }

        public static DateTime ValidDateForPackage = new DateTime(2018, 3, 31); //yyyy,M,dd
        public static string Databaseversion = "1";
        public static string Databaseversion_DETAILS = "start version 1 for first time";
    }
}