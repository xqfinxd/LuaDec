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
        public ElseEndBlock partner;

        private IExpression condexpr;

        public IfThenElseBlock(LFunction function, ICondition cond, int begin, int end, int elseTarget, CloseType closeType, int closeLine)
            : base(function, begin, end, closeType, closeLine, -1)
        {
            this.cond = cond;
            this.elseTarget = elseTarget;
        }

        public override void resolve(Registers r)
        {
            condexpr = cond.asExpression(r);
        }

        public override void walk(Walker w)
        {
            w.visitStatement(this);
            condexpr.walk(w);
            foreach (IStatement statement in statements)
            {
                statement.walk(w);
            }
        }

        public override bool suppressNewline()
        {
            return true;
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

        public override bool breakable()
        {
            return false;
        }

        public override int scopeEnd()
        {
            return usingClose && closeType == CloseType.CLOSE ? closeLine - 1 : end - 2;
        }

        public override bool isUnprotected()
        {
            return true;
        }

        public override int getUnprotectedLine()
        {
            return end - 1;
        }

        public override int getUnprotectedTarget()
        {
            return elseTarget;
        }

        public override int getLoopback()
        {
            throw new System.InvalidOperationException();
        }

        public override void print(Decompiler d, Output output)
        {
            output.print("if ");
            condexpr.print(d, output);
            output.print(" then");
            output.println();
            output.indent();

            IStatement.printSequence(d, output, statements);

            output.dedent();

            // Handle the "empty else" case
            if (end == elseTarget)
            {
                output.println("else");
                output.println("end");
            }

        }

    }

}