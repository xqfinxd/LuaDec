using LuaDec.Decompile.Condition;
using LuaDec.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Block
{
    public class WhileBlock50 : WhileBlock
    {

        private readonly int enterTarget;

        public WhileBlock50(LFunction function, ICondition cond, int begin, int end, int enterTarget, CloseType closeType, int closeLine)
            : base(function, cond, begin, end, closeType, closeLine)
        {
            this.enterTarget = enterTarget;
        }

        public override int scopeEnd()
        {
            return usingClose && closeType != CloseType.None ? closeLine - 1 : enterTarget - 1;
        }

        public override bool isUnprotected()
        {
            return false;
        }

    }

}
