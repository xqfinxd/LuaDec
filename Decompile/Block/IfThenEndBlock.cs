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
        private class IfThenEndBlockOperation_ : IOperation
        {
            private Assignment fassign;
            private ICondition fcombined;
            private int test;

            public IfThenEndBlockOperation_(Assignment fassign, ICondition fcombined, int test, int line)
                : base(line)
            {
                this.fassign = fassign;
                this.fcombined = fcombined;
                this.test = test;
            }

            public override List<IStatement> Process(Registers r, IBlock block)
            {
                if (fassign == null)
                {
                    r.SetValue(test, line, fcombined.AsExpression(r));
                    return new List<IStatement>();
                }
                else
                {
                    return new List<IStatement> { fassign };
                }
            }
        }

        private readonly ICondition cond;
        private readonly Registers r;
        private readonly bool redirected;
        private IExpression condexpr;

        public IfThenEndBlock(LFunction function, Registers r, ICondition cond, int begin, int end, CloseType closeType, int closeLine, bool redirected)
             : base(function, begin == end ? begin - 1 : begin, end, closeType, closeLine, -1)
        {
            this.r = r;
            this.cond = cond;
            this.redirected = redirected;
        }

        public IfThenEndBlock(LFunction function, Registers r, ICondition cond, int begin, int end)
            : this(function, r, cond, begin, end, CloseType.None, -1, false)
        {
        }

        public override bool Breakable()
        {
            return false;
        }

        public override int GetLoopback()
        {
            throw new System.InvalidOperationException();
        }

        public override bool IsUnprotected()
        {
            return false;
        }

        public override IOperation Process(Decompiler d)
        {
            int test = cond.Register();
            //System.err.println(cond);
            if (!scopeUsed && !redirected && test >= 0 && r.GetUpdated(test, end - 1) >= begin && !d.GetNoDebug())
            {
                // Check for a single assignment
                Assignment assign = null;
                if (statements.Count == 1)
                {
                    IStatement stmt = statements[0];
                    if (stmt is Assignment)
                    {
                        assign = (Assignment)stmt;
                        if (assign.GetArity() > 1)
                        {
                            int line = assign.GetFirstLine();
                            if (line >= begin && line < end)
                            {
                                assign = null;
                            }
                        }
                    }
                }
                if (assign != null && (cond.IsRegisterTest() || cond.IsOrCondition() || assign.GetDeclaration()) && assign.GetLastTarget().IsLocal() && assign.GetLastTarget().GetIndex() == test || statements.Count == 0)
                {
                    FinalSetCondition readonlyset = new FinalSetCondition(end - 1, test);
                    readonlyset.type = FinalSetCondition.Type.VALUE;
                    ICondition combined;

                    if (cond.Invertible())
                    {
                        combined = new OrCondition(cond.Inverse(), readonlyset);
                    }
                    else
                    {
                        combined = new AndCondition(cond, readonlyset);
                    }
                    Assignment fassign;
                    if (assign != null)
                    {
                        fassign = assign;
                        fassign.ReplaceLastValue(combined.AsExpression(r));
                    }
                    else
                    {
                        fassign = null;
                    }
                    ICondition fcombined = combined;
                    return new IfThenEndBlockOperation_(fassign, fcombined, test, end - 1);
                }
            }
            return base.Process(d);
        }

        public override void Resolve(Registers r)
        {
            condexpr = cond.AsExpression(r);
        }

        public override int ScopeEnd()
        {
            return usingClose && closeType == CloseType.Close ? closeLine - 1 : base.ScopeEnd();
        }

        public override void Walk(Walker w)
        {
            w.VisitStatement(this);
            condexpr.Walk(w);
            foreach (IStatement statement in statements)
            {
                statement.Walk(w);
            }
        }

        public override void Write(Decompiler d, Output output)
        {
            output.WriteString("if ");
            condexpr.Write(d, output);
            output.WriteString(" then");
            output.WriteLine();
            output.Indent();
            WriteSequence(d, output, statements);
            output.Dedent();
            output.WriteString("end");
        }
    }
}