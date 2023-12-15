using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Statement;
using LuaDec.Decompile.Target;
using LuaDec.Parser;

namespace LuaDec.Decompile.Block
{
    public abstract class ForBlock : ContainerBlock
    {
        protected readonly bool forvarClose;
        protected readonly int register;
        protected IExpression start;
        protected IExpression step;
        protected IExpression stop;
        protected ITarget target;

        public ForBlock(LFunction function, int begin, int end, int register, CloseType closeType, int closeLine, bool forvarClose)
            : base(function, begin, end, closeType, closeLine, -1)
        {
            this.register = register;
            this.forvarClose = forvarClose;
        }

        public override bool Breakable()
        {
            return true;
        }

        public override int GetLoopback()
        {
            throw new System.NotImplementedException();
        }

        public abstract void HandleVariableDeclarations(Registers r);

        public override bool HasHeader()
        {
            return true;
        }

        public override bool IsUnprotected()
        {
            return false;
        }

        public override int ScopeEnd()
        {
            int scopeEnd = end - 2;
            if (forvarClose) scopeEnd--;
            if (usingClose && (closeType == CloseType.Close || closeType == CloseType.Jmp)) scopeEnd--;
            return scopeEnd;
        }

        public override void Walk(Walker w)
        {
            w.VisitStatement(this);
            start.Walk(w);
            stop.Walk(w);
            step.Walk(w);
            foreach (IStatement statement in statements)
            {
                statement.Walk(w);
            }
        }

        public override void Write(Decompiler d, Output output)
        {
            output.Write("for ");
            target.Write(d, output, false);
            output.Write(" = ");
            start.Write(d, output);
            output.Write(", ");
            stop.Write(d, output);
            if (!step.IsInteger() || step.AsInteger() != 1)
            {
                output.Write(", ");
                step.Write(d, output);
            }
            output.Write(" do");
            output.WriteLine();
            output.Indent();
            WriteSequence(d, output, statements);
            output.Dedent();
            output.Write("end");
        }
    }
}