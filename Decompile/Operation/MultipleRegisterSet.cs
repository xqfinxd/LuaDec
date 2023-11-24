using LuaDec.Decompile.Block;
using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Statement;
using System.Collections.Generic;

namespace LuaDec.Decompile.Operation
{
    public class MultipleRegisterSet : IOperation
    {
        private readonly int registerFirst;
        private readonly int registerLast;
        private readonly IExpression value;

        public int RegisterFirst => registerFirst;

        public int RegisterLast => registerLast;

        public IExpression Value => value;

        public MultipleRegisterSet(int line, int registerFirst, int registerLast, IExpression value)
            : base(line)
        {
            this.registerFirst = registerFirst;
            this.registerLast = registerLast;
            this.value = value;
        }

        public override List<IStatement> Process(Registers r, IBlock block)
        {
            int count = 0;
            Assignment assignment = new Assignment();
            for (int register = RegisterFirst; register <= RegisterLast; register++)
            {
                r.SetValue(register, line, Value);
                if (r.IsAssignable(register, line))
                {
                    assignment.AddLast(r.GetTarget(register, line), Value, line);
                    count++;
                }
            }
            if (count > 0)
            {
                return new List<IStatement> { assignment };
            }
            else
            {
                return new List<IStatement>();
            }
        }
    }
}