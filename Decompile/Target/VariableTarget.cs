namespace LuaDec.Decompile.Target
{
    public class VariableTarget : ITarget
    {
        public readonly Declaration decl;

        public VariableTarget(Declaration decl)
        {
            this.decl = decl;
        }

        public override bool Equals(object obj)
        {
            if (obj is VariableTarget)
            {
                VariableTarget t = (VariableTarget)obj;
                return decl == t.decl;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override int GetIndex()
        {
            return decl.register;
        }

        public override bool IsDeclaration(Declaration decl)
        {
            return this.decl == decl;
        }

        public override bool IsLocal()
        {
            return true;
        }

        public override void Walk(Walker w)
        { }

        public override void Write(Decompiler d, Output output, bool declare)
        {
            output.Write(decl.name);
            if (declare && decl.tbc)
            {
                output.Write(" <close>");
            }
        }

        public override void WriteMethod(Decompiler d, Output output)
        {
            throw new System.InvalidOperationException();
        }
    }
}