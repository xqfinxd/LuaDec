namespace LuaDec.Decompile.Expression
{
    public class LocalVariable : IExpression
    {
        private readonly Declaration decl;

        public LocalVariable(Declaration decl)
            : base(PRECEDENCE_ATOMIC)
        {
            this.decl = decl;
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

        public override void Walk(Walker w)
        {
            w.VisitExpression(this);
        }

        public override void Write(Decompiler d, Output output)
        {
            output.WriteString(decl.name);
        }
    }
}