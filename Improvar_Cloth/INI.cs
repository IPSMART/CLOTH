using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.InteropServices;
using System.Text;

namespace Improvar
{
    public class INI
    {
        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        public void IniWriteValue(string Section, string Key, string Value, string path)
        {
            WritePrivateProfileString(Section, Key, Value, path);
        }
        [DllImport("kernel32")]
        static extern int GetPrivateProfileString(string Section, int Key,
               string Value, [MarshalAs(UnmanagedType.LPArray)] byte[] Result,
               int Size, string FileName);

        [DllImport("kernel32")]
        static extern int GetPrivateProfileString(int Section, string Key,
               string Value, [MarshalAs(UnmanagedType.LPArray)] byte[] Result,
               int Size, string FileName);
        public string IniReadValue(string Section, string Key, string path)
        {
            StringBuilder temp = new StringBuilder(2000000);
            int i = GetPrivateProfileString(Section, Key, "", temp, 2000000, path);
            return temp.ToString();
        }

        public void DeleteSection(string sectionName, string path)
        {
            if (sectionName == null)
                throw new ArgumentNullException("sectionName");

            WritePrivateProfileString(sectionName, null, null, path);
        }
        public void DeleteKey(string sectionName, string key, string path)
        {
            if (sectionName == null)
                throw new ArgumentNullException("sectionName");
            if (key == null)
                throw new ArgumentNullException("key");

            WritePrivateProfileString(sectionName, key, null, path);
        }
        public string[] GetEntryNames(string section, string path)
        {
            for (int maxsize = 500; true; maxsize *= 2)
            {
                byte[] bytes = new byte[maxsize];
                int size = GetPrivateProfileString(section, 0, "", bytes, maxsize, path);
                if (size < maxsize - 2)
                {
                    string entries = Encoding.ASCII.GetString(bytes, 0, size - (size > 0 ? 1 : 0));
                    return entries.Split(new char[] { '\0' });
                }
            }
        }
        public string[] GetSectionNames(string path)
        {
            for (int maxsize = 500; true; maxsize *= 2)
            {
                byte[] bytes = new byte[maxsize];
                int size = GetPrivateProfileString(0, "", "", bytes, maxsize, path);

                if (size < maxsize - 2)
                {
                    string Selected = Encoding.ASCII.GetString(bytes, 0,
                                   size - (size > 0 ? 1 : 0));
                    return Selected.Split(new char[] { '\0' });
                }
            }
        }
    }
}