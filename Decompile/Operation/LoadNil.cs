using LuaDec.Decompile.Block;
using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Statement;
using System.Collections.Generic;

namespace LuaDec.Decompile.Operation
{
    public class LoadNil : IOperation
    {
        public readonly int registerFirst;
        public readonly int registerLast;

        public LoadNil(int line, int registerFirst, int registerLast)
            : base(line)
        {
            this.registerFirst = registerFirst;
            this.registerLast = registerLast;
        }

        public override List<IStatement> Process(Registers r, IBlock block)
        {
            List<IStatement> assignments = new List<IStatement>(registerLast - registerFirst + 1);
            IExpression nil = ConstantExpression.CreateNil(line);
            Assignment declare = null;
            int scopeEnd = -1;
            for (int register = registerFirst; register <= registerLast; register++)
            {
                if (r.IsAssignable(register, line))
                {
                    scopeEnd = r.GetDeclaration(register, line).end;
                }
            }
            for (int register = registerFirst; register <= registerLast; register++)
            {
                r.SetValue(register, line, nil);
                if (r.IsAssignable(register, line) && r.GetDeclaration(register, line).end == scopeEnd && register >= block.closeRegister)
                {
                    if ((r.GetDeclaration(register, line).begin == line))
                    {
                        if (declare == null)
                        {
                            declare = new Assignment();
                            assignments.Add(declare);
                        }
                        declare.AddLast(r.GetTarget(register, line), nil, line);
                    }
                    else
                    {
                        assignments.Add(new Assignment(r.GetTarget(register, line), nil, line));
                    }
                }
            }
            return assignments;
        }
    }
}