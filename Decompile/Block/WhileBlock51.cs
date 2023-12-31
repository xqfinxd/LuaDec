﻿using LuaDec.Decompile.Condition;
using LuaDec.Parser;

namespace LuaDec.Decompile.Block
{
    public class WhileBlock51 : WhileBlock
    {
        private readonly int unprotectedTarget;

        public WhileBlock51(LFunction function, ICondition cond, int begin, int end, int unprotectedTarget, CloseType closeType, int closeLine)
            : base(function, cond, begin, end, closeType, closeLine)
        {
            this.unprotectedTarget = unprotectedTarget;
        }

        public override int GetUnprotectedLine()
        {
            return end - 1;
        }

        public override int GetUnprotectedTarget()
        {
            return unprotectedTarget;
        }

        public override bool IsSplitable()
        {
            return cond.IsSplitable();
        }

        public override bool IsUnprotected()
        {
            return true;
        }

        public override int ScopeEnd()
        {
            return usingClose && closeType == CloseType.Close ? end - 3 : end - 2;
        }

        public override IBlock[] Split(int line, CloseType closeType)
        {
            ICondition[] conds = cond.Split();
            cond = conds[0];
            return new IBlock[] {
              new IfThenElseBlock(function, conds[1], begin, line + 1, end - 1, closeType, line - 1),
              new ElseEndBlock(function, line + 1, end - 1, CloseType.None, -1),
            };
        }
    }
}