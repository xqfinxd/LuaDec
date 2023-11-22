using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Statement;
using LuaDec.Decompile.Target;
using LuaDec.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Block
{
    public abstract class ForBlock : ContainerBlock
    {

        protected readonly int register;
        protected readonly bool forvarClose;

        protected ITarget target;
        protected IExpression start;
        protected IExpression stop;
        protected IExpression step;

        public ForBlock(LFunction function, int begin, int end, int register, CloseType closeType, int closeLine, bool forvarClose)
            : base(function, begin, end, closeType, closeLine, -1)
        {
          this.register = register;
            this.forvarClose = forvarClose;
        }

        abstract public void handleVariableDeclarations(Registers r);

        public override void Walk(Walker w)
        {
            w.VisitStatement(this);
            start.walk(w);
            stop.walk(w);
            step.walk(w);
            foreach (IStatement statement in statements)
            {
                statement.Walk(w);
            }
        }

        public override int scopeEnd()
        {
            int scopeEnd = end - 2;
            if (forvarClose) scopeEnd--;
            if (usingClose && (closeType == CloseType.Close || closeType == CloseType.Jmp)) scopeEnd--;
            return scopeEnd;
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
            output.WriteString("for ");
            target.Write(d, output, false);
            output.WriteString(" = ");
            start.print(d, output);
            output.WriteString(", ");
            stop.print(d, output);
            if (!step.isInteger() || step.asInteger() != 1)
            {
                output.WriteString(", ");
                step.print(d, output);
            }
            output.WriteString(" do");
            output.WriteLine();
            output.Indent();
            WriteSequence(d, output, statements);
            output.Dedent();
            output.WriteString("end");
        }

    }

}
