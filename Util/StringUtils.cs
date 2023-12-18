using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Util
{
    public class StringUtils
    {

        public static string ToString(string s)
        {
            return ToString(s, -1);
        }
        public static string ToString(string s, int limit)
        {
            if (s == null) return "null";
            if (limit < 0) limit = s.Length;
            limit = Math.Min(limit, s.Length);

            StringBuilder b = new StringBuilder();
            b.Append('"');
            for (int i = 0; i < limit; i++)
            {
                char c = s[i];
                int ci = (int)c;
                if (c == '"')
                {
                    b.Append("\\\"");
                }
                else if (c == '\\')
                {
                    b.Append("\\\\");
                }
                else if (ci >= 32 && ci <= 126)
                {
                    b.Append(c);
                }
                else if (c == '\n')
                {
                    b.Append("\\n");
                }
                else if (c == '\t')
                {
                    b.Append("\\t");
                }
                else if (c == '\r')
                {
                    b.Append("\\r");
                }
                else if (c == '\b')
                {
                    b.Append("\\b");
                }
                else if (c == '\f')
                {
                    b.Append("\\f");
                }
                else if (ci == 11)
                {
                    b.Append("\\v");
                }
                else if (ci == 7)
                {
                    b.Append("\\a");
                }
                else
                {
                    b.Append(string.Format("\\x{0:X2}", ci));
                }
            }
            b.Append('"');
            return b.ToString();
        }

        public static string ParseString(string s)
        {
            if (s == "null") return null;
            if (s[0] != '"') throw new System.InvalidOperationException("Bad string " + s);
            if (s[s.Length - 1] != '"') throw new System.InvalidOperationException("Bad string " + s);
            StringBuilder b = new StringBuilder();
            for (int i = 1; i < s.Length - 1; /* nothing */)
            {
                char c = s[i++];
                if (c == '\\')
                {
                    if (i < s.Length - 1)
                    {
                        c = s[i++];
                        if (c == '"')
                        {
                            b.Append('"');
                        }
                        else if (c == '\\')
                        {
                            b.Append('\\');
                        }
                        else if (c == 'n')
                        {
                            b.Append('\n');
                        }
                        else if (c == 't')
                        {
                            b.Append('\t');
                        }
                        else if (c == 'r')
                        {
                            b.Append('\r');
                        }
                        else if (c == 'b')
                        {
                            b.Append('\b');
                        }
                        else if (c == 'f')
                        {
                            b.Append('\f');
                        }
                        else if (c == 'v')
                        {
                            b.Append((char)11);
                        }
                        else if (c == 'a')
                        {
                            b.Append((char)7);
                        }
                        else if (c == 'x')
                        {
                            if (i + 1 < s.Length - 1)
                            {
                                string digits = s.Substring(i, i + 2);
                                i += 2;
                                b.Append((char)int.Parse(digits, System.Globalization.NumberStyles.HexNumber));
                            }
                            else
                            {
                                return null; // error
                            }
                        }
                        else
                        {
                            return null; // error
                        }
                    }
                    else
                    {
                        return null; // error
                    }
                }
                else
                {
                    b.Append(c);
                }
            }
            return b.ToString();
        }

        private StringUtils() { }
    }

}
