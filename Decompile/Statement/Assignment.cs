using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Target;
using System.Collections.Generic;

namespace LuaDec.Decompile.Statement
{
    public class Assignment : IStatement
    {
        private readonly List<int> lines = new List<int>(5);
        private readonly List<ITarget> targets = new List<ITarget>(5);
        private readonly List<IExpression> values = new List<IExpression>(5);
        private bool allnil = true;
        private bool declare = false;
        private int declareStart = 0;

        public Assignment()
        {
        }

        public Assignment(ITarget target, IExpression value, int line)
        {
            targets.Add(target);
            values.Add(value);
            lines.Add(line);
            allnil = allnil && value.IsNil();
        }

        public void AddFirst(ITarget target, IExpression value, int line)
        {
            targets.Insert(0, target);
            values.Insert(0, value);
            lines.Insert(0, line);
            allnil = allnil && value.IsNil();
        }

        public void AddLast(ITarget target, IExpression value, int line)
        {
            if (targets.Contains(target))
            {
                int index = targets.IndexOf(target);
                targets.RemoveAt(index);
                value = values[index];
                values.RemoveAt(index);
                lines.RemoveAt(index);
            }
            targets.Add(target);
            values.Add(value);
            lines.Add(line);
            allnil = allnil && value.IsNil();
        }

        public bool AssignListEquals(List<Declaration> decls)
        {
            if (decls.Count != targets.Count) return false;

            foreach (ITarget target in targets)
            {
                bool found = false;
                foreach (Declaration decl in decls)
                {
                    if (target.IsDeclaration(decl))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found) return false;
            }
            return true;
        }

        public bool Assigns(Declaration decl)
        {
            foreach (ITarget target in targets)
            {
                if (target.IsDeclaration(decl)) return true;
            }
            return false;
        }

        public bool AssignsTarget(Declaration decl)
        {
            foreach (ITarget target in targets)
            {
                if (target.IsDeclaration(decl))
                {
                    return true;
                }
            }
            return false;
        }

        public override bool BeginsWithParen()
        {
            return !declare && targets[0].BeginsWithParen();
        }

        public bool CanDeclare(List<Declaration> locals)
        {
            foreach (ITarget target in targets)
            {
                bool isNewLocal = false;
                foreach (Declaration decl in locals)
                {
                    if (target.IsDeclaration(decl))
                    {
                        isNewLocal = true;
                        break;
                    }
                }
                if (!isNewLocal)
                {
                    return false;
                }
            }
            return true;
        }

        public int GetArity()
        {
            return targets.Count;
        }

        public bool GetDeclaration()
        {
            return declare;
        }

        public int GetFirstLine()
        {
            return lines[0];
        }

        public ITarget GetFirstTarget()
        {
            return targets[0];
        }

        public IExpression GetFirstValue()
        {
            return values[0];
        }

        public ITarget GetLastTarget()
        {
            return targets[targets.Count - 1];
        }

        public IExpression GetValue(int target)
        {
            int index = 0;
            foreach (ITarget t in targets)
            {
                if (t.IsLocal() && t.GetIndex() == target)
                {
                    return values[index];
                }
                index++;
            }
            throw new System.InvalidOperationException();
        }

        public void ReplaceLastValue(IExpression value)
        {
            values[values.Count - 1] = value;
        }

        public void ReplaceValue(int target, IExpression value)
        {
            int index = 0;
            foreach (ITarget t in targets)
            {
                if (t.IsLocal() && t.GetIndex() == target)
                {
                    values[index] = value;
                    //lines.set(index, line);
                    return;
                }
                index++;
            }
            throw new System.InvalidOperationException();
        }

        public void SetDeclare(int declareStart)
        {
            declare = true;
            this.declareStart = declareStart;
        }

        public override void Walk(Walker w)
        {
            w.VisitStatement(this);
            foreach (ITarget target in targets)
            {
                target.Walk(w);
            }
            foreach (IExpression expression in values)
            {
                expression.Walk(w);
            }
        }

        public override void Write(Decompiler d, Output output)
        {
            if (targets.Count > 0)
            {
                if (declare)
                {
                    output.Write("local ");
                }

                bool functionSugar = false;
                if (targets.Count == 1 && values.Count == 1 && values[0].IsClosure() && targets[0].IsFunctionName())
                {
                    IExpression closure = values[0];

                    if (!declare || declareStart >= closure.ClosureUpvalueLine())
                    {
                        functionSugar = true;
                    }
                    if (targets[0].IsLocal() && closure.IsUpvalueOf(targets[0].GetIndex()))
                    {
                        functionSugar = true;
                    }
                }
                if (!functionSugar)
                {
                    targets[0].Write(d, output, declare);
                    for (int i = 1; i < targets.Count; i++)
                    {
                        output.Write(", ");
                        targets[i].Write(d, output, declare);
                    }
                    if (!declare || !allnil)
                    {
                        output.Write(" = ");

                        List<IExpression> expressions = new List<IExpression>();

                        int size = values.Count;
                        if (size >= 2 && values[size - 1].IsNil() && (lines[size - 1] == values[size - 1].GetConstantLine() || values[size - 1].GetConstantLine() == -1))
                        {
                            foreach (var item in values)
                            {
                                expressions.Add(item);
                            }
                        }
                        else
                        {
                            bool include = false;
                            for (int i = size - 1; i >= 0; i--)
                            {
                                IExpression value = values[i];
                                if (include || !value.IsNil() || value.GetConstantIndex() != -1)
                                {
                                    include = true;
                                }
                                if (include)
                                {
                                    expressions.Insert(0, value);
                                }
                            }

                            if (expressions.Count == 0 && !declare)
                            {
                                foreach (var item in values)
                                {
                                    expressions.Add(item);
                                }
                            }
                        }

                        IExpression.WriteSequence(d, output, expressions, false, targets.Count > expressions.Count);
                    }
                }
                else
                {
                    values[0].WriteClosure(d, output, targets[0]);
                }
                if (Comment != null)
                {
                    output.Write(" -- ");
                    output.Write(Comment);
                }
            }
        }
    }
}