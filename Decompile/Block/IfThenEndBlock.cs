using LuaDec.Decompile.Condition;
using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Operation;
using LuaDec.Decompile.Statement;
using LuaDec.Parser;
using System.Collections.Generic;

namespace LuaDec.Decompile.Block
{
    public class IfThenEndBlock : ContainerBlock
    {

        private readonly ICondition cond;
        private readonly bool redirected;
        private readonly Registers r;

        private IExpression condexpr;

        public IfThenEndBlock(LFunction function, Registers r, ICondition cond, int begin, int end, CloseType closeType, int closeLine, bool redirected)
             : base(function, begin == end ? begin - 1 : begin, end, closeType, closeLine, -1)
        {
            this.r = r;
            this.cond = cond;
            this.redirected = redirected;
        }

        public IfThenEndBlock(LFunction function, Registers r, ICondition cond, int begin, int end)
            : this(function, r, cond, begin, end, CloseType.NONE, -1, false)
        {

        }

        public override int scopeEnd()
        {
            return usingClose && closeType == CloseType.CLOSE ? closeLine - 1 : base.scopeEnd();
        }

        public override void resolve(Registers r)
        {
            condexpr = cond.asExpression(r);
        }

        public override void walk(Walker w)
        {
            w.VisitStatement(this);
            condexpr.walk(w);
            foreach (IStatement statement in statements)
            {
                statement.walk(w);
            }
        }

        public override bool breakable()
        {
            return false;
        }

        public override bool isUnprotected()
        {
            return false;
        }

        public override int getLoopback()
        {
            throw new System.InvalidOperationException();
        }

        private class IfThenEndBlockOperation_ : IOperation
        {
            Assignment fassign;
            ICondition fcombined;
            int test;
            public IfThenEndBlockOperation_(Assignment fassign, ICondition fcombined, int test, int line)
                : base(line)
            {
                this.fassign = fassign;
                this.fcombined = fcombined;
                this.test = test;
            }

            public override List<IStatement> process(Registers r, IBlock block)
            {
                if (fassign == null)
                {
                    r.setValue(test, line, fcombined.asExpression(r));
                    return new List<IStatement>();
                }
                else
                {
                    return new List<IStatement> { fassign };
                }
            }
        }
        public override IOperation process(Decompiler d)
        {
            int test = cond.register();
            //System.err.println(cond);
            if (!scopeUsed && !redirected && test >= 0 && r.getUpdated(test, end - 1) >= begin && !d.getNoDebug())
            {
                // Check for a single assignment
                Assignment assign = null;
                if (statements.Count == 1)
                {
                    IStatement stmt = statements[0];
                    if (stmt is Assignment)
                    {
                        assign = (Assignment)stmt;
                        if (assign.getArity() > 1)
                        {
                            int line = assign.getFirstLine();
                            if (line >= begin && line < end)
                            {
                                assign = null;
                            }
                        }
                    }
                }
                if (assign != null && (cond.isRegisterTest() || cond.isOrCondition() || assign.GetDeclaration()) && assign.getLastTarget().isLocal() && assign.getLastTarget().getIndex() == test || statements.Count == 0)
                {
                    FinalSetCondition readonlyset = new FinalSetCondition(end - 1, test);
                    readonlyset.type = FinalSetCondition.Type.VALUE;
                    ICondition combined;

                    if (cond.invertible())
                    {
                        combined = new OrCondition(cond.inverse(), readonlyset);
                    }
                    else
                    {
                        combined = new AndCondition(cond, readonlyset);
                    }
                    Assignment fassign;
                    if (assign != null)
                    {
                        fassign = assign;
                        fassign.replaceLastValue(combined.asExpression(r));
                    }
                    else
                    {
                        fassign = null;
                    }
                    ICondition fcombined = combined;
                    return new IfThenEndBlockOperation_(fassign, fcombined, test, end - 1);
                }
            }
            return base.process(d);
        }

        public override void print(Decompiler d, Output output)
        {
            output.WriteString("if ");
            condexpr.print(d, output);
            output.WriteString(" then");
            output.WriteLine();
            output.Indent();
            printSequence(d, output, statements);
            output.Dedent();
            output.WriteString("end");
        }
    }
}
