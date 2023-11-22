namespace LuaDec.Decompile.Target
{
    public abstract class ITarget
    {
        public virtual bool BeginsWithParen()
        {
            return false;
        }

        public virtual int GetIndex()
        {
            throw new System.NotImplementedException();
        }

        public virtual bool IsDeclaration(Declaration decl)
        {
            return false;
        }

        public virtual bool IsFunctionName()
        {
            return true;
        }

        public virtual bool IsLocal()
        {
            return false;
        }

        public abstract void Write(Decompiler d, Output output, bool declare);

        public abstract void WriteMethod(Decompiler d, Output output);

        public abstract void Walk(Walker w);
    }
}