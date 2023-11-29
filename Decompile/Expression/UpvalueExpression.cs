namespace LuaDec.Decompile.Expression
{
    public class UpvalueExpression : IExpression
    {
        private readonly string name;

        public UpvalueExpression(string name)
            : base(PRECEDENCE_ATOMIC)
        {
            this.name = name;
        }

        public override int GetConstantIndex()
        {
            return -1;
        }

        public override bool IsBrief()
        {
            return true;
        }

        public override bool IsDotChain()
        {
            return true;
        }

        public override bool IsEnvironmentTable(Decompiler d)
        {
            return d.GetVersion().IsEnvTable(name);
        }

        public override void Walk(Walker w)
        {
            w.VisitExpression(this);
        }

        public override void Write(Decompiler d, Output output)
        {
            output.Write(name);
        }
    }
}