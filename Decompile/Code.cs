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
                extraByte[i] = op != null && op.hasExtraByte(codepoint(line), extractor);
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
            return extractor.A.extract(code[line - 1]);
        }

        public int AxField(int line)
        {
            return extractor.Ax.extract(code[line - 1]);
        }

        public int BField(int line)
        {
            return extractor.B.extract(code[line - 1]);
        }

        public int BxField(int line)
        {
            return extractor.Bx.extract(code[line - 1]);
        }

        public int CField(int line)
        {
            return extractor.C.extract(code[line - 1]);
        }

        public int codepoint(int line)
        {
            return code[line - 1];
        }

        public CodeExtract getExtractor()
        {
            return extractor;
        }

        //public bool reentered = false;

        /**
         * Returns the operation indicated by the instruction at the given line.
         */

        public Op GetOp(int line)
        {
            /*if(!reentered) {
              reentered = true;
              System.output.println("line " + line + ": " + tostring(line));
              reentered = false;
            }*/
            if (line >= 2 && extraByte[line - 2])
            {
                return Op.EXTRABYTE;
            }
            else
            {
                return map.GetOp(opcode(line));
            }
        }

        public bool isUpvalueDeclaration(int line)
        {
            return upvalue[line - 1];
        }

        public bool kField(int line)
        {
            return extractor.k.extract(code[line - 1]) != 0;
        }

        public int opcode(int line)
        {
            return extractor.op.extract(code[line - 1]);
        }

        /**
         * Returns the A field of the instruction at the given line.
         */
        /**
         * Returns the C field of the instruction at the given line.
         */
        /**
         * Returns the sC (signed C) field of the instruction at the given line.
         */

        public int sBField(int line)
        {
            int B = BField(line);
            return B - extractor.B.max() / 2;
        }

        public int sBxField(int line)
        {
            return extractor.sBx.extract(code[line - 1]);
        }

        public int sCField(int line)
        {
            int C = CField(line);
            return C - extractor.C.max() / 2;
        }

        /**
         * Returns the k field of the instruction at the given line (1 is true, 0 is false).
         */
        /**
         * Returns the B field of the instruction at the given line.
         */
        /**
         * Returns the sB (signed B) field of the instruction at the given line.
         */
        /**
         * Returns the Ax field (A extended) of the instruction at the given line.
         */
        /**
         * Returns the Bx field (B extended) of the instruction at the given line.
         */
        /**
         * Returns the sBx field (signed B extended) of the instruction at the given line.
         */
        /**
         * Returns the absolute target address of a jump instruction and the given line.
         * This field will be chosen automatically based on the opcode.
         */

        public int target(int line)
        {
            return line + 1 + GetOp(line).jumpField(codepoint(line), extractor);
        }

        /**
         * Returns the full instruction codepoint at the given line.
         */

        public string ToString(int line)
        {
            return GetOp(line).codePointTostring(codepoint(line), extractor, null);
        }
    }
}