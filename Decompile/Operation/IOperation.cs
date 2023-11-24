using LuaDec.Decompile.Block;
using LuaDec.Decompile.Statement;
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

        public abstract List<IStatement> Process(Registers r, IBlock block);
    }
}