using LuaDec.Decompile.Expression;

namespace LuaDec.Decompile.Target
{
    public class GlobalTarget : ITarget
    {
        private readonly IExpression name;

        public GlobalTarget(ConstantExpression name)
        {
            this.name = name;
        }

        public override void Walk(Walker w)
        {
            name.Walk(w);
        }

        public override void Write(Decompiler d, Output output, bool declare)
        {
            output.WriteString(name.AsName());
        }

        public override void WriteMethod(Decompiler d, Output output)
        {
            throw new System.InvalidOperationException();
        }
    }
}