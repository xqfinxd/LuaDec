using LuaDec.Parser;
using System;

namespace LuaDec.Decompile
{
    public class Constant
    {
        private enum Type
        {
            NIL,
            BOOL,
            NUMBER,
            STRING,
        }

        private readonly bool boolValue;
        private readonly LNumber numberValue;
        private readonly string stringValue;
        private readonly Type type;

        public Constant(int constant)
        {
            type = Type.NUMBER;
            boolValue = false;
            numberValue = LNumber.MakeInt(constant);
            stringValue = null;
        }

        public Constant(double x)
        {
            type = Type.NUMBER;
            boolValue = false;
            numberValue = LNumber.MakeDouble(x);
            stringValue = null;
        }

        public Constant(LObject constant)
        {
            if (constant is LNil)
            {
                type = Type.NIL;
                boolValue = false;
                numberValue = null;
                stringValue = null;
            }
            else if (constant is LBoolean)
            {
                type = Type.BOOL;
                boolValue = constant == LBoolean.LTRUE;
                numberValue = null;
                stringValue = null;
            }
            else if (constant is LNumber)
            {
                type = Type.NUMBER;
                boolValue = false;
                numberValue = (LNumber)constant;
                stringValue = null;
            }
            else if (constant is LString)
            {
                type = Type.STRING;
                boolValue = false;
                numberValue = null;
                stringValue = ((LString)constant).Deref();
            }
            else
            {
                throw new System.InvalidOperationException("Illegal constant type: " + constant.ToString());
            }
        }

        public int AsInt()
        {
            if (!IsInt())
            {
                throw new System.InvalidOperationException();
            }
            return (int)numberValue.GetValue();
        }

        public string AsName()
        {
            if (type != Type.STRING)
            {
                throw new System.InvalidOperationException();
            }
            return stringValue;
        }

        public bool IsBool()
        {
            return type == Type.BOOL;
        }

        public bool IsIdentifier(Version version)
        {
            if (!IsIdentifierPermissive(version))
            {
                return false;
            }
            char start = stringValue[0];
            if (start != '_' && !char.IsLetter(start))
            {
                return false;
            }
            for (int i = 1; i < stringValue.Length; i++)
            {
                char next = stringValue[i];
                if (char.IsLetter(next))
                {
                    continue;
                }
                if (char.IsDigit(next))
                {
                    continue;
                }
                if (next == '_')
                {
                    continue;
                }
                return false;
            }
            return true;
        }

        public bool IsIdentifierPermissive(Version version)
        {
            if (!IsString() || version.IsReserved(stringValue))
            {
                return false;
            }
            if (stringValue.Length == 0)
            {
                return false;
            }
            char start = stringValue[0];
            if (char.IsDigit(start) && start != ' ' && !char.IsLetter(start))
            {
                return false;
            }
            return true;
        }

        public bool IsInt()
        {
            return numberValue.GetValue() == Math.Round(numberValue.GetValue());
        }

        public bool IsNegative()
        {
            // Tricky to catch -0.0 here
            return numberValue.GetValue().ToString().StartsWith("-");
        }

        public bool IsNil()
        {
            return type == Type.NIL;
        }

        public bool IsNumber()
        {
            return type == Type.NUMBER;
        }

        public bool IsString()
        {
            return type == Type.STRING;
        }

        public void Print(Decompiler d, Output output, bool braced)
        {
            switch (type)
            {
                case Type.NIL:
                    output.Write("nil");
                    break;

                case Type.BOOL:
                    output.Write(boolValue ? "true" : "false");
                    break;

                case Type.NUMBER:
                    output.Write(numberValue.ToPrintable(0));
                    break;

                case Type.STRING:
                    int newlines = 0;
                    int unprintable = 0;
                    bool rawstring = d.GetConfiguration().RawString;
                    for (int i = 0; i < stringValue.Length; i++)
                    {
                        char c = stringValue[i];
                        if (c == '\n')
                        {
                            newlines++;
                        }
                        else if ((c <= 31 && c != '\t') || c >= 127)
                        {
                            unprintable++;
                        }
                    }
                    bool longstring = (newlines > 1 || (newlines == 1 && stringValue.IndexOf('\n') != stringValue.Length - 1)); // heuristic
                    longstring = longstring && unprintable == 0; // can't escape and for robustness, don't want to allow non-ASCII output
                    longstring = longstring && !stringValue.Contains("[["); // triggers compatibility error in 5.1 TODO: avoidable?
                    if (d.function.header.version.useNestingLongStrings.Value)
                    {
                        longstring = longstring && !stringValue.Contains("]]") && !stringValue.EndsWith("]"); // no piping TODO: allow proper nesting
                    }
                    if (longstring)
                    {
                        int pipe = 0;
                        string pipestring = "]]";
                        string startPipestring = "]";
                        while (stringValue.EndsWith(startPipestring) || stringValue.IndexOf(pipestring) >= 0)
                        {
                            pipe++;
                            pipestring = "]";
                            int i = pipe;
                            while (i-- > 0) pipestring += "=";
                            startPipestring = pipestring;
                            pipestring += "]";
                        }
                        if (braced) output.Write("(");
                        output.Write("[");
                        while (pipe-- > 0) output.Write("=");
                        output.Write("[");
                        int indent = output.IndentLevel;
                        output.IndentLevel = 0;
                        output.WriteLine();
                        output.Write(stringValue);
                        output.Write(pipestring);
                        if (braced) output.Write(")");
                        output.IndentLevel = indent;
                    }
                    else
                    {
                        output.Write("\"");
                        for (int i = 0; i < stringValue.Length; i++)
                        {
                            char c = stringValue[i];
                            if (c <= 31 || c >= 127)
                            {
                                if (c == 7)
                                {
                                    output.Write("\\a");
                                }
                                else if (c == 8)
                                {
                                    output.Write("\\b");
                                }
                                else if (c == 12)
                                {
                                    output.Write("\\f");
                                }
                                else if (c == 10)
                                {
                                    output.Write("\\n");
                                }
                                else if (c == 13)
                                {
                                    output.Write("\\r");
                                }
                                else if (c == 9)
                                {
                                    output.Write("\\t");
                                }
                                else if (c == 11)
                                {
                                    output.Write("\\v");
                                }
                                else if (!rawstring || c <= 127)
                                {
                                    string dec = c.ToString();
                                    int len = dec.Length;
                                    output.Write("\\");
                                    while (len++ < 3)
                                    {
                                        output.Write("0");
                                    }
                                    output.Write(dec);
                                }
                                else
                                {
                                    output.WriteByte((byte)c);
                                }
                            }
                            else if (c == 34)
                            {
                                output.Write("\\\"");
                            }
                            else if (c == 92)
                            {
                                output.Write("\\\\");
                            }
                            else
                            {
                                output.Write(c.ToString());
                            }
                        }
                        output.Write("\"");
                    }
                    break;

                default:
                    throw new System.InvalidOperationException();
            }
        }
    }
}