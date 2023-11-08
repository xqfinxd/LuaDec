using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Target
{
    abstract public class ITarget
    {

        public abstract void walk(Walker w);

        public abstract void print(Decompiler d, Output output, bool declare);

        public abstract void printMethod(Decompiler d, Output output);

        public virtual bool isDeclaration(Declaration decl)
        {
            return false;
        }

        public virtual bool isLocal()
        {
            return false;
        }

        public virtual int getIndex()
        {
            throw new System.InvalidOperationException();
        }

        public virtual bool isFunctionName()
        {
            return true;
        }

        public virtual bool beginsWithParen()
        {
            return false;
        }

    }

}
