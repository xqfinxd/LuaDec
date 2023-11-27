namespace LuaDec.Decompile.Expression
{
    public class UnaryExpression : IExpression
    {
        private readonly IExpression expression;
        private readonly string op;

        public UnaryExpression(string op, IExpression expression, int precedence)
            : base(precedence)
        {
            this.op = op;
            this.expression = expression;
        }

        public override int GetConstantIndex()
        {
            return expression.GetConstantIndex();
        }

        public override bool IsUngrouped()
        {
            return true;
        }

        public override void Walk(Walker w)
        {
            w.VisitExpression(this);
            expression.Walk(w);
        }

        public override void Write(Decompiler d, Output output)
        {
            output.WriteString(op);
            if (precedence > expression.precedence)
                output.WriteString("(");
            expression.Write(d, output);
            if (precedence > expression.precedence)
                output.WriteString(")");
        }
    }
}