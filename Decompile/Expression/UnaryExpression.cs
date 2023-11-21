namespace LuaDec.Decompile.Expression
{
    public class UnaryExpression : IExpression
    {

        private readonly string op;
        private readonly IExpression expression;

        public UnaryExpression(string op, IExpression expression, int precedence)
            : base(precedence)
        {
            this.op = op;
            this.expression = expression;
        }

        public override void walk(Walker w)
        {
            w.visitExpression(this);
            expression.walk(w);
        }

        public override bool isUngrouped()
        {
            return true;
        }

        public override int getConstantIndex()
        {
            return expression.getConstantIndex();
        }

        public override void print(Decompiler d, Output output)
        {
            output.WriteString(op);
            if (precedence > expression.precedence) output.WriteString("(");
            expression.print(d, output);
            if (precedence > expression.precedence) output.WriteString(")");
        }

    }
}
