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

        public override bool Breakable()
        {
            return true;
        }

        public override int GetLoopback()
        {
            throw new System.InvalidOperationException();
        }

        public override bool IsUnprotected()
        {
            return false;
        }

        public override void Resolve(Registers r)
        {
            condexpr = cond.AsExpression(r);
        }

        public override int ScopeEnd()
        {
            if (extendedRepeatScope)
            {
                return usingClose && closeType != CloseType.None ? closeLine - 1 : repeatScopeEnd;
            }
            else
            {
                return usingClose && closeType != CloseType.None ? closeLine : base.ScopeEnd();
            }
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

        public override void Write(Decompiler d, Output output)
        {
            output.Write("repeat");
            output.WriteLine();
            output.Indent();
            WriteSequence(d, output, statements);
            output.Dedent();
            output.Write("until ");
            condexpr.Write(d, output);
        }
    }
}