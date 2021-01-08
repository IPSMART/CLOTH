using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Improvar
{
    public static class CommFunc
    {
        public static string toProper(this string str)
        {
            TextInfo ti = new CultureInfo("en-US", false).TextInfo;
            return str = ti.ToTitleCase(str.ToLower());
        }
        public static double toRound(this double val, int deci = 2)
        {
            double rtval = 0;
            rtval = Math.Round(val, deci, MidpointRounding.AwayFromZero);
            return rtval;
        }
        public static string retDateStr(this object val, string yearas = "yyyy", string dateform = "dd/MM/yyyy")
        {
            if (val == null) return "";
            string rtval = "";
            if (val.ToString().retStr() != "") val = Convert.ToDateTime(val).ToString(dateform);
            if (val == null) rtval = "";
            else if (val.ToString() == "") rtval = "";
            else if (yearas == "yy") rtval = val.ToString().Substring(0, 6) + val.ToString().Substring(8, 2);
            else rtval = val.ToString().Substring(0, 10);
            return rtval;
        }
        public static string retStr(this object val)
        {
            string rtval = "";
            if (val == null) rtval = "";
            else if (val.ToString() == "") rtval = "";
            else rtval = val.ToString();
            return rtval;
        }
        public static string ToRoman(int number)
        {
            if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException("insert value betwheen 1 and 3999");
            if (number < 1) return string.Empty;
            if (number >= 1000) return "M" + ToRoman(number - 1000);
            if (number >= 900) return "CM" + ToRoman(number - 900);
            if (number >= 500) return "D" + ToRoman(number - 500);
            if (number >= 400) return "CD" + ToRoman(number - 400);
            if (number >= 100) return "C" + ToRoman(number - 100);
            if (number >= 90) return "XC" + ToRoman(number - 90);
            if (number >= 50) return "L" + ToRoman(number - 50);
            if (number >= 40) return "XL" + ToRoman(number - 40);
            if (number >= 10) return "X" + ToRoman(number - 10);
            if (number >= 9) return "IX" + ToRoman(number - 9);
            if (number >= 5) return "V" + ToRoman(number - 5);
            if (number >= 4) return "IV" + ToRoman(number - 4);
            if (number >= 1) return "I" + ToRoman(number - 1);
            throw new ArgumentOutOfRangeException("something bad happened");
        }
        public static string ToINRFormat(this double amt, string style = "###,##,##,##,##0.00")
        {
            Connection Cn = new Connection();
            string str = Cn.Indian_Number_format(amt.ToString(), style);
            return str;
        }
        public static string retSqlformat(this object codestr)
        {
            codestr = codestr.retStr().Replace("'","");
            string rtval = "";
            string[] cdval = codestr.ToString().Split(',');
            if (cdval.Count() > 0 && cdval.Count() < 1000 && codestr.ToString().Trim() != "")
            {
                rtval = "'" + string.Join("','", cdval) + "'";
            }
            return rtval;
        }
        public static string retHtmlCell(string cellval, string celltype = "C", bool cellbold = false, int cellfontsize = 0, int blankcells = 0, string cellstyle = "", Int32 colspan = 0)
        {
            string rval = "";
            if (blankcells > 0)
            {
                for (int q = 1; q <= blankcells; q++)
                {
                    rval += "<td>" + "</td>";
                }
            }
            else
            {
                var cellv = (dynamic)null;
                switch (celltype.Substring(0, 1))
                {
                    case "N":
                        cellv = Convert.ToDouble(cellval); break;
                    default:
                        cellv = cellval; break;
                }
                rval = "<td";
                if (colspan > 0) rval += " colspan=" + colspan.ToString() + " ";
                if (cellbold == true || cellfontsize != 0) rval += " style='";
                if (cellbold == true) rval += " font-weight:bold;";
                if (cellfontsize != 0) rval += " font-size:" + cellfontsize.ToString() + "px;";
                if (cellbold == true || cellfontsize != 0) rval += "'";
                if (cellstyle != "") rval += cellstyle;
                rval += ">" + cellv;
                rval += "</td>";
            }
            return rval;
        }
        public static string retErrmsg(Exception ex)
        {
            string errmsg = "";
            errmsg = "Message:" + ex.Message;
            if (ex.InnerException != null)
                errmsg += " InnerException" + ex.InnerException.InnerException.Message;
            return errmsg;
        }
        public static string ZeroIfEmpty(this string s)
        {
            return string.IsNullOrEmpty(s) ? "0" : s;
        }
        public static string retSqlfromStrarray(this string[] codestr)
        {
            string rtval = "";
            for (int i = 0; i <= codestr.Count() - 1; i++)
            {
                if (rtval != "") rtval += ",";
                rtval += "'" + codestr[i] + "'";
            }
            return rtval;
        }
        public static string retRepname(this string repname)
        {
            repname = (Regex.Replace(repname.retStr(), @"[^0-9a-zA-Z_]+", "_"));
            return repname + "_" + System.DateTime.Now.ToString("yyMMddHHmmss");
        }
        public static double retDbl(this object val)
        {
            double rtval = 0;
            if (val == null || val.retStr() == "") rtval = 0;
            else rtval = Convert.ToDouble(val);
            return rtval;
        }
        public static int retInt(this object val)
        {
            if (val.retStr() == "") return 0;
            else return Convert.ToInt32(val);
        }
        public static float MMtoPointFloat(this object val)
        {
            if (val.retStr() == "") return 0;
            else {
                return float.Parse((val.retDbl() * 2.83465).ToString());
            }
        }
        public static float PointtoMMFloat(this object val)
        {
            if (val.retStr() == "") return 0;
            else {
                return float.Parse((val.retDbl() * 2.83465).ToString());
            }
        }
        public static decimal retDcml(this object val)
        {
            decimal rtval = 0;
            if (val == null || val.retStr() == "") rtval = 0;
            else rtval = Convert.ToDecimal(val);
            return rtval;
        }
        public static short retShort(this object val)
        {
            if (val.retStr() == "") return 0;
            else return Convert.ToInt16(val);
        }
        public static string retCompValue(this string val, string colname)
        {
            if (val == "" || colname == "") return "";
            var findin = val;
            var retval = "";
            var colfind = "^" + colname.ToUpper() + "=^";
            var spltchar = ((char)181);

            var pos = findin.IndexOf(colfind);
            if (pos < 0) retval = "";
            else {
                var extpos = colname.Length + 3;
                var reststr = findin.Substring(pos + extpos);
                var npos = reststr.IndexOf(spltchar);
                retval = findin.Substring(pos + extpos, npos);
            }
            return retval;
        }
        public static string Encrypt_URL(string UNQSNO)
        {
            Connection Cn = new Connection();
            return Cn.Encrypt_URL(UNQSNO);
        }
        public static IEnumerable<TSource> DistinctBy<TSource>(this IEnumerable<TSource> source, Func<TSource, object> predicate)
        {
            return from item in source.GroupBy(predicate) select item.First();
        }
        public static int CharmPrice(string ChrmType, int Rate, string RoundVal)
        {
            RoundVal = RoundVal.PadLeft(2, '0');
            if (Rate < 10) Rate = 10 + Rate;
            int RoundAmt = RoundVal.retInt();
            if (RoundAmt == 0) RoundAmt = 100;
            int small = ((Rate / RoundAmt) * RoundAmt);
            int big = small + RoundAmt;
            if (ChrmType == "RD")//ROUND
            {
                int roundValu = (Rate - small >= big - Rate) ? big : small;
                return roundValu;
            }
            else if (ChrmType == "RN")//ROUNDNEXT
            {
                return big;
            }
            else if (ChrmType == "NT")//NEXT
            {
                int last2rate = (Rate % 100);
                if (last2rate <= RoundVal.retInt())
                {
                    big = (Rate.ToString().Substring(0, Rate.ToString().Length - 2) + RoundVal).retInt();
                }
                else
                {
                    big = ((Rate + 100).ToString().Substring(0, (Rate + 100).ToString().Length - 2) + RoundVal).retInt();
                }
                return big.retInt();
            }
            else if (ChrmType == "NR")//NEAR
            {
                int last2rate = (Rate % 100);
                if (last2rate <= RoundVal.retInt())
                {
                    big = (Rate.ToString().Substring(0, Rate.ToString().Length - 2) + RoundVal).retInt();
                    small = ((Rate - 100).ToString().Substring(0, (Rate - 100).ToString().Length - 2) + RoundVal).retInt();
                }
                else
                {
                    big = ((Rate + 100).ToString().Substring(0, (Rate + 100).ToString().Length - 2) + RoundVal).retInt();
                    small = ((Rate).ToString().Substring(0, Rate.ToString().Length - 2) + RoundVal).retInt();
                }
                int NEAR = (Rate - small >= big - Rate) ? big : small;
                return NEAR.retInt();
            }
            else
            {
                return 0;
            }
        }
        public static bool IsValidEmail(object email)
        {
            try
            {
                var mail = new System.Net.Mail.MailAddress(email.retStr());
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool IsValidPhone(object PhoneNo)
        {
            return Regex.IsMatch(PhoneNo.retStr(), "^([0-9]{6,12})$", RegexOptions.IgnoreCase);
        }
        public static string TruncateWord(this object value, int length)
        {
            string input = value.retStr();
            if (input == null || input.Length < length)
                return input;
            int iNextSpace = input.LastIndexOf(" ", length, StringComparison.Ordinal);
            return string.Format("{0}", input.Substring(0, (iNextSpace > 0) ? iNextSpace : length).Trim());
        }
    }
}