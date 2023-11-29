using LuaDec.Decompile.Condition;
using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Statement;
using LuaDec.Parser;

namespace LuaDec.Decompile.Block
{
    public class IfThenElseBlock : ContainerBlock
    {
        private readonly ICondition cond;
        private readonly int elseTarget;
        private IExpression condexpr;
        public ElseEndBlock partner;

        public IfThenElseBlock(LFunction function, ICondition cond, int begin, int end, int elseTarget, CloseType closeType, int closeLine)
            : base(function, begin, end, closeType, closeLine, -1)
        {
            this.cond = cond;
            this.elseTarget = elseTarget;
        }

        public override bool Breakable()
        {
            return false;
        }

        public override int CompareTo(IBlock block)
        {
            if (block == partner)
            {
                return -1;
            }
            else
            {
                return base.CompareTo(block);
            }
        }

        public override int GetLoopback()
        {
            throw new System.InvalidOperationException();
        }

        public override int GetUnprotectedLine()
        {
            return end - 1;
        }

        public override int GetUnprotectedTarget()
        {
            return elseTarget;
        }

        public override bool IsUnprotected()
        {
            return true;
        }

        public override void Resolve(Registers r)
        {
            condexpr = cond.AsExpression(r);
        }

        public override int ScopeEnd()
        {
            return usingClose && closeType == CloseType.Close ? closeLine - 1 : end - 2;
        }

        public override bool SuppressNewline()
        {
            return true;
        }

        public override void Walk(Walker w)
        {
            w.VisitStatement(this);
            condexpr.Walk(w);
            foreach (IStatement statement in statements)
            {
                statement.Walk(w);
            }
        }

        public override void Write(Decompiler d, Output output)
        {
            output.Write("if ");
            condexpr.Write(d, output);
            output.Write(" then");
            output.WriteLine();
            output.Indent();

            IStatement.WriteSequence(d, output, statements);

            output.Dedent();

            // Handle the "empty else" case
            if (end == elseTarget)
            {
                output.WriteLine("else");
                output.WriteLine("end");
            }
        }
    }
}