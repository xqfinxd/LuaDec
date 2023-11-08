using LuaDec.Decompile.Block;
using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Statement;
using LuaDec.Decompile.Target;
using System.Collections.Generic;

namespace LuaDec.Decompile.Operation
{
    public class GlobalSet : IOperation
    {

        private ConstantExpression global;
        private IExpression value;

        public GlobalSet(int line, ConstantExpression global, IExpression value)
            : base(line)
        {
          this.global = global;
            this.value = value;
        }

        public override List<IStatement> process(Registers r, IBlock block)
        {
            return new List<IStatement> { new Assignment(new GlobalTarget(global), value, line) };
        }

    }

}
