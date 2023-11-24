using LuaDec.Decompile.Block;
using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Statement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Operation
{
    public class MultipleRegisterSet : IOperation
    {

        public readonly int registerFirst;
        public readonly int registerLast;
        public readonly IExpression value;

        public MultipleRegisterSet(int line, int registerFirst, int registerLast, IExpression value)
            : base(line)
        {
          this.registerFirst = registerFirst;
            this.registerLast = registerLast;
            this.value = value;
        }

        public override List<IStatement> process(Registers r, IBlock block)
        {
            int count = 0;
            Assignment assignment = new Assignment();
            for (int register = registerFirst; register <= registerLast; register++)
            {
                r.SetValue(register, line, value);
                if (r.IsAssignable(register, line))
                {
                    assignment.AddLast(r.GetTarget(register, line), value, line);
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
