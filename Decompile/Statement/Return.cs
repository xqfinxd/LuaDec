using LuaDec.Decompile.Expression;
using System.Collections.Generic;

namespace LuaDec.Decompile.Statement
{
    public class Return : IStatement
    {

        private IExpression[] values;

        public Return()
        {
            values = new IExpression[0];
        }

        public Return(IExpression value)
        {
            values = new IExpression[1];
            values[0] = value;
        }

        public Return(IExpression[] values)
        {
            this.values = values;
        }

        public override void walk(Walker w)
        {
            w.visitStatement(this);
            foreach (IExpression expression in values)
            {
                expression.walk(w);
            }
        }

        public override void print(Decompiler d, Output output)
        {
            output.print("do ");
            printTail(d, output);
            output.print(" end");
        }

        public override void printTail(Decompiler d, Output output)
        {
            output.print("return");
            if (values.Length > 0)
            {
                output.print(" ");
                List<IExpression> rtns = new List<IExpression>(values.Length);
                foreach (IExpression value in values)
                {
                    rtns.Add(value);
                }
                IExpression.printSequence(d, output, rtns, false, true);
            }
        }

    }

}
