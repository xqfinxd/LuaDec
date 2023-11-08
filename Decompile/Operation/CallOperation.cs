using LuaDec.Decompile.Block;
using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Statement;
using System.Collections.Generic;

namespace LuaDec.Decompile.Operation
{
    public class CallOperation : IOperation
    {

        private FunctionCall call;

        public CallOperation(int line, FunctionCall call)
            : base(line)
        {
          this.call = call;
        }

        public override List<IStatement> process(Registers r, IBlock block)
        {
            return new List<IStatement> { new FunctionCallStatement(call) };
        }

    }

}
