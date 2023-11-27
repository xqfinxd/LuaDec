namespace LuaDec.Decompile.Expression
{
    public class GlobalExpression : IExpression
    {
        private readonly int index;
        private readonly ConstantExpression name;

        public GlobalExpression(ConstantExpression name, int index)
            : base(PRECEDENCE_ATOMIC)
        {
            this.name = name;
            this.index = index;
        }

        public override int GetConstantIndex()
        {
            return index;
        }

        public override bool IsBrief()
        {
            return true;
        }

        public override bool IsDotChain()
        {
            return true;
        }

        public override void Walk(Walker w)
        {
            w.VisitExpression(this);
            name.Walk(w);
        }

        public override void Write(Decompiler d, Output output)
        {
            output.WriteString(name.AsName());
        }
    }
}