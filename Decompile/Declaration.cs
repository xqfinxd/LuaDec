using LuaDec.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile
{
    public class Declaration
    {

        public readonly string name;
        public readonly int begin;
        public readonly int end;
        public int register;
        public bool tbc;

        /**
         * Whether this is an invisible for-loop book-keeping variable.
         */
        public bool forLoop = false;

        /**
         * Whether this is an explicit for-loop declared variable.
         */
        public bool forLoopExplicit = false;

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

        public bool isSplitBy(int line, int begin, int end)
        {
            int scopeEnd = end - 1;
            return this.begin >= line && this.begin < begin
              || this.end >= line && this.end < begin
              || this.begin < begin && this.end >= begin && this.end < scopeEnd
              || this.begin >= begin && this.begin <= scopeEnd && this.end > scopeEnd;
        }

    }

}
