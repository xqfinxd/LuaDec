using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Target
{
    public class UpvalueTarget : ITarget
    {

        private readonly string name;

        public UpvalueTarget(string name)
        {
            this.name = name;
        }

        public override void walk(Walker w) { }

        public override void print(Decompiler d, Output output, bool declare)
        {
            output.WriteString(name);
        }

        public override void printMethod(Decompiler d, Output output)
        {
            throw new System.InvalidOperationException();
        }

    }

}
