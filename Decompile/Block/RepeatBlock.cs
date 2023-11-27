using LuaDec.Decompile.Condition;
using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Statement;
using LuaDec.Parser;

namespace LuaDec.Decompile.Block
{
    public class RepeatBlock : ContainerBlock
    {

        private readonly ICondition cond;
        private readonly bool extendedRepeatScope;
        private readonly int repeatScopeEnd;

        private IExpression condexpr;

        public RepeatBlock(LFunction function, ICondition cond, int begin, int end, CloseType closeType, int closeLine, bool extendedRepeatScope, int repeatScopeEnd)
            : base(function, begin, end, closeType, closeLine, 0)
        {
            this.cond = cond;
            this.extendedRepeatScope = extendedRepeatScope;
            this.repeatScopeEnd = repeatScopeEnd;
        }

        public override void resolve(Registers r)
        {
            condexpr = cond.asExpression(r);
        }

        public override void Walk(Walker w)
        {
            w.VisitStatement(this);
            foreach (IStatement statement in statements)
            {
                statement.Walk(w);
            }
            condexpr.Walk(w);
        }

        public override int scopeEnd()
        {
            if (extendedRepeatScope)
            {
                return usingClose && closeType != CloseType.None ? closeLine - 1 : repeatScopeEnd;
            }
            else
            {
                return usingClose && closeType != CloseType.None ? closeLine : base.scopeEnd();
            }
        }

        public override bool breakable()
        {
            return true;
        }

        public override bool isUnprotected()
        {
            return false;
        }

        public override int getLoopback()
        {
            throw new System.InvalidOperationException();
        }

        public override void Write(Decompiler d, Output output)
        {
            output.WriteString("repeat");
            output.WriteLine();
            output.Indent();
            WriteSequence(d, output, statements);
            output.Dedent();
            output.WriteString("until ");
            condexpr.Write(d, output);
        }

    }

}
