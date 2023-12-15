using LuaDec.Decompile.Condition;
using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Statement;
using LuaDec.Parser;

namespace LuaDec.Decompile.Block
{
    public abstract class WhileBlock : ContainerBlock
    {
        private IExpression condexpr;
        protected ICondition cond;

        public WhileBlock(LFunction function, ICondition cond, int begin, int end, CloseType closeType, int closeLine)
            : base(function, begin, end, closeType, closeLine, -1)
        {
            this.cond = cond;
        }

        public override bool Breakable()
        {
            return true;
        }

        public override bool HasHeader()
        {
            return true;
        }

        public override int GetLoopback()
        {
            throw new System.InvalidOperationException();
        }

        public override void Resolve(Registers r)
        {
            condexpr = cond.AsExpression(r);
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
            output.Write("while ");
            condexpr.Write(d, output);
            output.Write(" do");
            output.WriteLine();
            output.Indent();
            WriteSequence(d, output, statements);
            output.Dedent();
            output.Write("end");
        }
    }
}