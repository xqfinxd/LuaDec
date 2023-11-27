using System.IO;
using System.Text;

namespace LuaDec.Assemble
{
    public class Tokenizer
    {
        private StringBuilder b;
        private StreamReader input;

        public Tokenizer(StreamReader input)
        {
            this.input = input;
            b = new StringBuilder();
        }

        public string next()
        {
            b.Length = 0;

            bool inToken = false;
            bool inString = false;
            bool inComment = false;
            bool isLPrefix = false;
            bool inEscape = false;

            for (; ; )
            {
                int code = input.Read();
                if (code == -1) break;
                char c = (char)code;

                if (inString)
                {
                    if (c == '\\' && !inEscape)
                    {
                        inEscape = true;
                        b.Append(c);
                    }
                    else if (c == '"' && !inEscape)
                    {
                        b.Append(c);
                        break;
                    }
                    else
                    {
                        inEscape = false;
                        b.Append(c);
                    }
                }
                else if (inComment)
                {
                    if (c == '\n' || c == '\r')
                    {
                        inComment = false;
                        if (inToken)
                        {
                            break;
                        }
                    }
                }
                else if (c == ';')
                {
                    inComment = true;
                }
                else if (char.IsWhiteSpace(c))
                {
                    if (inToken)
                    {
                        break;
                    }
                }
                else
                {
                    if ((!inToken || isLPrefix) && c == '"')
                    {
                        inString = true;
                    }
                    else if (!inToken && c == 'L')
                    {
                        isLPrefix = true;
                    }
                    else
                    {
                        isLPrefix = false;
                    }
                    inToken = true;
                    b.Append(c);
                }
            }

            if (b.Length == 0)
            {
                return null;
            }
            else
            {
                return b.ToString();
            }
        }
    }
}