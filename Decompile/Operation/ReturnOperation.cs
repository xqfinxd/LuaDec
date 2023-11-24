using LuaDec.Decompile.Block;
using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Statement;
using System.Collections.Generic;

namespace LuaDec.Decompile.Operation
{
    public class ReturnOperation : IOperation
    {
        private IExpression[] values;

        public ReturnOperation(int line, IExpression value)
            : base(line)
        {
            values = new IExpression[1];
            values[0] = value;
        }

        public ReturnOperation(int line, IExpression[] values)
            : base(line)
        {
            this.values = values;
        }

        public override List<IStatement> Process(Registers r, IBlock block)
        {
            return new List<IStatement> { new Return(values) };
        }
    }
}