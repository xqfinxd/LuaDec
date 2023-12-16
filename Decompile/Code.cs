using LuaDec.Parser;

namespace LuaDec.Decompile
{
    public class Code
    {
        private readonly int[] code;
        private readonly bool[] extraByte;
        private readonly CodeExtract extractor;
        private readonly OpCodeMap map;
        private readonly bool[] upvalue;

        public int Length => code.Length;

        public Code(LFunction function)
        {
            code = function.code;
            map = function.header.opmap;
            extractor = function.header.extractor;
            extraByte = new bool[Length];
            for (int i = 0; i < Length; i++)
            {
                int line = i + 1;
                Op op = GetOp(line);
                extraByte[i] = op != null && op.HasExtraByte(CodePoint(line), extractor);
            }
            upvalue = new bool[Length];
            if (function.header.version.upvalueDeclarationType.Value == Version.UpvalueDeclarationType.Inline)
            {
                for (int i = 0; i < Length; i++)
                {
                    int line = i + 1;
                    if (GetOp(line) == Op.CLOSURE)
                    {
                        int f = BxField(line);
                        if (f < function.functions.Length)
                        {
                            int nups = function.functions[f].numUpvalues;
                            for (int j = 1; j <= nups; j++)
                            {
                                if (i + j < Length)
                                {
                                    upvalue[i + j] = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        public int AField(int line)
        {
            return extractor.A.Extract(code[line - 1]);
        }

        public int AxField(int line)
        {
            return extractor.Ax.Extract(code[line - 1]);
        }

        public int BField(int line)
        {
            return extractor.B.Extract(code[line - 1]);
        }

        public int BxField(int line)
        {
            return extractor.Bx.Extract(code[line - 1]);
        }

        public int CField(int line)
        {
            return extractor.C.Extract(code[line - 1]);
        }

        public int CodePoint(int line)
        {
            return code[line - 1];
        }

        public CodeExtract GetExtractor()
        {
            return extractor;
        }

        public Op GetOp(int line)
        {
            if (line >= 2 && extraByte[line - 2])
            {
                return Op.EXTRABYTE;
            }
            else
            {
                return map.GetOp(OpCode(line));
            }
        }

        public bool IsUpvalueDeclaration(int line)
        {
            return upvalue[line - 1];
        }

        public bool kField(int line)
        {
            return extractor.k.Extract(code[line - 1]) != 0;
        }

        public int OpCode(int line)
        {
            return extractor.op.Extract(code[line - 1]);
        }

        public int sBField(int line)
        {
            int B = BField(line);
            return B - extractor.B.Max() / 2;
        }

        public int sBxField(int line)
        {
            return extractor.sBx.Extract(code[line - 1]);
        }

        public int sCField(int line)
        {
            int C = CField(line);
            return C - extractor.C.Max() / 2;
        }

        public int Target(int line)
        {
            return line + 1 + GetOp(line).JumpField(CodePoint(line), extractor);
        }

        public string ToString(int line)
        {
            return GetOp(line).CodePointTostring(null, CodePoint(line), extractor, null);
        }
    }
}