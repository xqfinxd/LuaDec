using LuaDec.Decompile.Statement;
using LuaDec.Decompile.Block;
using System.Collections.Generic;

namespace LuaDec.Decompile.Operation
{
    public abstract class IOperation
    {

        public readonly int line;

        public IOperation(int line)
        {
            this.line = line;
        }

        public abstract List<IStatement> process(Registers r, IBlock block);

    }

}
