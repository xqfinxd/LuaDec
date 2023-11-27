using LuaDec.Decompile.Condition;
using LuaDec.Parser;

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

        public override bool IsUnprotected()
        {
            return false;
        }

        public override int ScopeEnd()
        {
            return usingClose && closeType != CloseType.None ? closeLine - 1 : enterTarget - 1;
        }
    }
}