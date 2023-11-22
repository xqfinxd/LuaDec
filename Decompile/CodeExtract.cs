namespace LuaDec.Decompile
{
    public class CodeExtract
    {
        public class Field
        {
            private readonly int mask;
            private readonly int offset;
            private readonly int shift;
            private readonly int size;

            public int Size => size;

            public Field(int size, int shift) : this(size, shift, 0)
            {
            }

            public Field(int size, int shift, int offset)
            {
                this.size = size;
                this.shift = shift;
                this.mask = Size2Mask(size);
                this.offset = offset;
            }

            public bool Check(int x)
            {
                return ((x + offset) & ~mask) == 0;
            }

            public int Clear(int codepoint)
            {
                return codepoint & ~(mask << shift);
            }

            public int Encode(int x)
            {
                return (x + offset) << shift;
            }

            public int Extract(int codepoint)
            {
                return ((int)((uint)codepoint >> shift) & mask) - offset;
            }

            public int Max()
            {
                return mask - offset;
            }
        }

        private readonly int rkOffset;
        public readonly Field A;
        public readonly Field Ax;
        public readonly Field B;
        public readonly Field Bx;
        public readonly Field C;
        public readonly Field k;
        public readonly Field op;
        public readonly Field sBx;
        public readonly Field sJ;
        public readonly Field x;

        public CodeExtract(Version version, int sizeOp, int sizeA, int sizeB, int sizeC)
        {
            switch (version.instructionFormat.Value)
            {
                case Version.InstructionFormat.Lua50:
                    op = new Field(sizeOp, 0);
                    A = new Field(sizeA, sizeB + sizeC + sizeOp);
                    B = new Field(sizeB, sizeB + sizeOp);
                    C = new Field(sizeC, sizeOp);
                    k = null;
                    Ax = null;
                    sJ = null;
                    Bx = new Field(sizeB + sizeC, sizeOp);
                    sBx = new Field(sizeB + sizeC, sizeOp, Size2Mask(sizeB + sizeC) / 2);
                    x = new Field(32, 0);
                    break;

                case Version.InstructionFormat.Lua51:
                    op = new Field(6, 0);
                    A = new Field(8, 6);
                    B = new Field(9, 23);
                    C = new Field(9, 14);
                    k = null;
                    Ax = new Field(26, 6);
                    sJ = null;
                    Bx = new Field(18, 14);
                    sBx = new Field(18, 14, 131071);
                    x = new Field(32, 0);
                    break;

                case Version.InstructionFormat.Lua54:
                    op = new Field(7, 0);
                    A = new Field(8, 7);
                    B = new Field(8, 16);
                    C = new Field(8, 24);
                    k = new Field(1, 15);
                    Ax = new Field(25, 7);
                    sJ = new Field(25, 7, (1 << 24) - 1);
                    Bx = new Field(17, 15);
                    sBx = new Field(17, 15, (1 << 16) - 1);
                    x = new Field(32, 0);
                    break;

                default:
                    throw new System.InvalidOperationException();
            }
            int rk_offset = version.rkOffset.Value;
            this.rkOffset = rk_offset;
        }

        private static int Size2Mask(int size)
        {
            return (int)((1L << size) - 1);
        }

        public int EncodeK(int constant)
        {
            return constant + rkOffset;
        }

        public int GetK(int field)
        {
            return field - rkOffset;
        }

        public bool IsK(int field)
        {
            return field >= rkOffset;
        }
    }
}