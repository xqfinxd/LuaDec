using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Statement;
using LuaDec.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Block
{
    public class AlwaysLoop : ContainerBlock
    {

        private readonly bool repeat;

        private ConstantExpression condition;

        public AlwaysLoop(LFunction function, int begin, int end, CloseType closeType, int closeLine, bool repeat)
             : base(function, begin, end, closeType, closeLine, 0)
        {
          this.repeat = repeat;
            condition = null;
        }

        public override int scopeEnd()
        {
            return usingClose && closeType == CloseType.CLOSE ? closeLine - 1 : end - 2;
        }

        public override bool breakable()
        {
            return true;
        }

        public override bool isUnprotected()
        {
            return true;
        }

        public override int getUnprotectedTarget()
        {
            return begin;
        }

        public override int getUnprotectedLine()
        {
            return end - 1;
        }

        public override int getLoopback()
        {
            return begin;
        }

        public override void print(Decompiler d, Output output)
        {
            if (repeat)
            {
                output.println("repeat");
            }
            else
            {
                output.print("while ");
                if (condition == null)
                {
                    output.print("true");
                }
                else
                {
                    condition.print(d, output);
                }
                output.println(" do");
            }
            output.indent();
            IStatement.printSequence(d, output, statements);
            output.dedent();
            if (repeat)
            {
                output.print("until false");
            }
            else
            {
                output.print("end");
            }
        }

        public override bool useConstant(Function f, int index)
        {
            if (!repeat && condition == null)
            {
                condition = f.getConstantExpression(index);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
