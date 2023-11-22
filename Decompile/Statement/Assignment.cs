using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Target;
using System.Collections.Generic;

namespace LuaDec.Decompile.Statement
{
    public class Assignment : IStatement
    {

        private readonly List<ITarget> targets = new List<ITarget>(5);
        private readonly List<IExpression> values = new List<IExpression>(5);
        private readonly List<int> lines = new List<int>(5);

        private bool allnil = true;
        private bool declare = false;
        private int declareStart = 0;

        public Assignment()
        {

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
                expression.walk(w);
            }
        }

        public override bool BeginsWithParen()
        {
            return !declare && targets[0].BeginsWithParen();
        }

        public ITarget getFirstTarget()
        {
            return targets[0];
        }

        public ITarget getLastTarget()
        {
            return targets[targets.Count - 1];
        }

        public IExpression getFirstValue()
        {
            return values[0];
        }

        public void replaceLastValue(IExpression value)
        {
            values[values.Count - 1] = value;
        }

        public int getFirstLine()
        {
            return lines[0];
        }

        public bool assignsTarget(Declaration decl)
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

        public int getArity()
        {
            return targets.Count;
        }

        public Assignment(ITarget target, IExpression value, int line)
        {
            targets.Add(target);
            values.Add(value);
            lines.Add(line);
            allnil = allnil && value.isNil();
        }

        public void addFirst(ITarget target, IExpression value, int line)
        {
            targets.Insert(0, target);
            values.Insert(0, value);
            lines.Insert(0, line);
            allnil = allnil && value.isNil();
        }

        public void addLast(ITarget target, IExpression value, int line)
        {
            if (targets.Contains(target))
            {
                int index = targets.IndexOf(target);
                targets.RemoveAt(index);
                // value = values.RemoveAt(index);
                values.RemoveAt(index);
                lines.RemoveAt(index);
            }
            targets.Add(target);
            values.Add(value);
            lines.Add(line);
            allnil = allnil && value.isNil();
        }

        public IExpression getValue(int target)
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

        public void replaceValue(int target, IExpression value)
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

        public bool assignListEquals(List<Declaration> decls)
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

        public void SetDeclare(int declareStart)
        {
            declare = true;
            this.declareStart = declareStart;
        }

        public bool GetDeclaration()
        {
            return declare;
        }

        public bool assigns(Declaration decl)
        {
            foreach (ITarget target in targets)
            {
                if (target.IsDeclaration(decl)) return true;
            }
            return false;
        }

        public bool canDeclare(List<Declaration> locals)
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

        public override void Write(Decompiler d, Output output)
        {
            if (targets.Count > 0)
            {
                if (declare)
                {
                    output.WriteString("local ");
                }

                bool functionSugar = false;
                if (targets.Count == 1 && values.Count == 1 && values[0].isClosure() && targets[0].IsFunctionName())
                {
                    IExpression closure = values[0];

                    if (!declare || declareStart >= closure.closureUpvalueLine())
                    {
                        functionSugar = true;
                    }
                    if (targets[0].IsLocal() && closure.isUpvalueOf(targets[0].GetIndex()))
                    {
                        functionSugar = true;
                    }
                }
                if (!functionSugar)
                {
                    targets[0].Write(d, output, declare);
                    for (int i = 1; i < targets.Count; i++)
                    {
                        output.WriteString(", ");
                        targets[i].Write(d, output, declare);
                    }
                    if (!declare || !allnil)
                    {
                        output.WriteString(" = ");

                        List<IExpression> expressions = new List<IExpression>();

                        int size = values.Count;
                        if (size >= 2 && values[size - 1].isNil() && (lines[size - 1] == values[size - 1].getConstantLine() || values[size - 1].getConstantLine() == -1))
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
                                if (include || !value.isNil() || value.getConstantIndex() != -1)
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

                        IExpression.printSequence(d, output, expressions, false, targets.Count > expressions.Count);
                    }
                }
                else
                {
                    values[0].printClosure(d, output, targets[0]);
                }
                if (Comment != null)
                {
                    output.WriteString(" -- ");
                    output.WriteString(Comment);
                }
            }
        }

    }

}
