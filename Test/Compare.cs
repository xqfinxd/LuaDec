using LuaDec;
using LuaDec.Parser;
using System.IO;

namespace Test
{
    public class Compare
    {
        public enum Mode
        {
            Normal,
            Full,
        }

        private readonly Mode mode;

        public Compare(Mode mode)
        {
            this.mode = mode;
        }

        public bool ByteCodeEqual(string file1, string file2)
        {
            LFunction main1 = File2Function(file1);
            LFunction main2 = File2Function(file2);
            return FunctionEqual(main1, main2);
        }

        public LFunction File2Function(string filename)
        {
            FileStream file = null;
            try
            {
                file = File.OpenRead(filename);
                BHeader header = new BHeader(new BinaryReader(file), new Configuration());
                return header.main;
            }
            catch (IOException)
            {
                return null;
            }
            finally
            {
                if (file != null)
                {
                    try
                    {
                        file.Close();
                    }
                    catch (IOException)
                    {
                    }
                }
            }
        }

        public bool FunctionEqual(LFunction f1, LFunction f2)
        {
            if (f1.maximumStackSize != f2.maximumStackSize)
            {
                return false;
            }
            if (f1.numParams != f2.numParams)
            {
                return false;
            }
            if (f1.numUpvalues != f2.numUpvalues)
            {
                return false;
            }
            if (f1.varArg != f2.varArg)
            {
                return false;
            }
            if (f1.code.Length != f2.code.Length)
            {
                return false;
            }
            for (int i = 0; i < f1.code.Length; i++)
            {
                if (f1.code[i] != f2.code[i])
                {
                    return false;
                }
            }
            if (f1.constants.Length != f2.constants.Length)
            {
                return false;
            }
            for (int i = 0; i < f1.constants.Length; i++)
            {
                if (!ObjectEqual(f1.constants[i], f2.constants[i]))
                {
                    return false;
                }
            }
            if (f1.locals.Length != f2.locals.Length)
            {
                return false;
            }
            for (int i = 0; i < f1.locals.Length; i++)
            {
                if (!LocalEqual(f1.locals[i], f2.locals[i]))
                {
                    return false;
                }
            }
            if (f1.upvalues.Length != f2.upvalues.Length)
            {
                return false;
            }
            for (int i = 0; i < f1.upvalues.Length; i++)
            {
                if (!f1.upvalues[i].Equals(f2.upvalues[i]))
                {
                    return false;
                }
            }
            if (f1.functions.Length != f2.functions.Length)
            {
                return false;
            }
            for (int i = 0; i < f1.functions.Length; i++)
            {
                if (!FunctionEqual(f1.functions[i], f2.functions[i]))
                {
                    return false;
                }
            }
            if (mode == Mode.Full)
            {
                if (!f1.name.Equals(f2.name))
                {
                    return false;
                }
                if (f1.lineDefined != f2.lineDefined)
                {
                    return false;
                }
                if (f1.lastLineDefined != f2.lastLineDefined)
                {
                    return false;
                }
                if (f1.lines.Length != f2.lines.Length)
                {
                    return false;
                }
                for (int i = 0; i < f1.lines.Length; i++)
                {
                    if (f1.lines[i] != f2.lines[i])
                    {
                        return false;
                    }
                }
                if ((f1.absLineInfo == null) != (f2.absLineInfo == null))
                {
                    return false;
                }
                if (f1.absLineInfo != null)
                {
                    if (f1.absLineInfo.Length != f2.absLineInfo.Length)
                    {
                        return false;
                    }
                    for (int i = 0; i < f1.absLineInfo.Length; i++)
                    {
                        if (!f1.absLineInfo[i].Equals(f2.absLineInfo[i]))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public bool LocalEqual(LLocal l1, LLocal l2)
        {
            if (l1.start != l2.start)
            {
                return false;
            }
            if (l1.end != l2.end)
            {
                return false;
            }
            if (!l1.name.Equals(l2.name))
            {
                return false;
            }
            return true;
        }

        public bool ObjectEqual(LObject o1, LObject o2)
        {
            return o1.Equals(o2);
        }
    }
}