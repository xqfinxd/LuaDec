using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Statement;
using LuaDec.Parser;

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

        public override bool Breakable()
        {
            return true;
        }

        public override int GetLoopback()
        {
            return begin;
        }

        public override int GetUnprotectedLine()
        {
            return end - 1;
        }

        public override int GetUnprotectedTarget()
        {
            return begin;
        }

        public override bool IsUnprotected()
        {
            return true;
        }

        public override int ScopeEnd()
        {
            return usingClose && closeType == CloseType.Close ? closeLine - 1 : end - 2;
        }

        public override bool UseConstant(Function f, int index)
        {
            if (!repeat && condition == null)
            {
                condition = f.GetConstantExpression(index);
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Write(Decompiler d, Output output)
        {
            if (repeat)
            {
                output.WriteLine("repeat");
            }
            else
            {
                output.Write("while ");
                if (condition == null)
                {
                    output.Write("true");
                }
                else
                {
                    condition.Write(d, output);
                }
                output.WriteLine(" do");
            }
            output.Indent();
            IStatement.WriteSequence(d, output, statements);
            output.Dedent();
            if (repeat)
            {
                output.Write("until false");
            }
            else
            {
                output.Write("end");
            }
        }
    }
}