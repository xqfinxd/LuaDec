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

        public override void Walk(Walker w)
        {
            w.VisitStatement(this);
            foreach (IExpression expression in values)
            {
                expression.Walk(w);
            }
        }

        public override void Write(Decompiler d, Output output)
        {
            output.Write("do ");
            WriteTail(d, output);
            output.Write(" end");
        }

        public override void WriteTail(Decompiler d, Output output)
        {
            output.Write("return");
            if (values.Length > 0)
            {
                output.Write(" ");
                List<IExpression> rtns = new List<IExpression>(values.Length);
                foreach (IExpression value in values)
                {
                    rtns.Add(value);
                }
                IExpression.WriteSequence(d, output, rtns, false, true);
            }
        }
    }
}