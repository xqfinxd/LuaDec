namespace LuaDec.Decompile.Expression
{
    public class VarArg : IExpression
    {
        private readonly bool multiple;

        public VarArg(int length, bool multiple)
            : base(PRECEDENCE_ATOMIC)
        {
            this.multiple = multiple;
        }

        public override int GetConstantIndex()
        {
            return -1;
        }

        public override bool IsMultiple()
        {
            return multiple;
        }

        public override void Walk(Walker w)
        {
            w.VisitExpression(this);
        }

        public override void Write(Decompiler d, Output output)
        {
            output.Write(multiple ? "..." : "(...)");
        }

        public override void WriteMultiple(Decompiler d, Output output)
        {
            output.Write(multiple ? "..." : "(...)");
        }
    }
}