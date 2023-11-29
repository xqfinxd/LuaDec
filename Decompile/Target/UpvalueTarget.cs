namespace LuaDec.Decompile.Target
{
    public class UpvalueTarget : ITarget
    {
        private readonly string name;

        public UpvalueTarget(string name)
        {
            this.name = name;
        }

        public override void Walk(Walker w)
        { }

        public override void Write(Decompiler d, Output output, bool declare)
        {
            output.Write(name);
        }

        public override void WriteMethod(Decompiler d, Output output)
        {
            throw new System.InvalidOperationException();
        }
    }
}