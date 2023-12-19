using LuaDec.Parser;

namespace LuaDec.Decompile
{
    public class Declaration
    {
        public readonly int begin;
        public readonly int end;
        public readonly string name;
        public bool forLoop = false;
        public bool forLoopExplicit = false;
        public int register;
        public bool tbc;

        public Declaration(LLocal local, Code code)
        {
            int adjust = 0;
            if (local.start >= 1)
            {
                Op op = code.GetOp(local.start);
                if (op == Op.MMBIN || op == Op.MMBINI || op == Op.MMBINK || op == Op.EXTRAARG)
                {
                    adjust--;
                }
            }
            this.name = local.ToString();
            this.begin = local.start + adjust;
            this.end = local.end;
            this.tbc = false;
        }

        public Declaration(string name, int begin, int end)
        {
            this.name = name;
            this.begin = begin;
            this.end = end;
        }

        public bool IsSplitBy(int line, int begin, int end)
        {
            int scopeEnd = end - 1;
            if (begin == end) begin = begin - 1;
            return this.begin >= line && this.begin < begin
              || this.end >= line && this.end < begin
              || this.begin < begin && this.end >= begin && this.end < scopeEnd
              || this.begin >= begin && this.begin <= scopeEnd && this.end > scopeEnd;
        }
    }
}