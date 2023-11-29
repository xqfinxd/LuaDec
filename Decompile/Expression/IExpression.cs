using LuaDec.Decompile.Target;
using System;
using System.Collections.Generic;

namespace LuaDec.Decompile.Expression
{
    public abstract class IExpression
    {
        public class BinaryOperation
        {
            public static BinaryOperation ADD = new BinaryOperation("+", PRECEDENCE_ADD, ASSOCIATIVITY_LEFT);
            public static BinaryOperation AND = new BinaryOperation("and", PRECEDENCE_AND, ASSOCIATIVITY_NONE);
            public static BinaryOperation BAND = new BinaryOperation("&", PRECEDENCE_BAND, ASSOCIATIVITY_LEFT);
            public static BinaryOperation BOR = new BinaryOperation("|", PRECEDENCE_BOR, ASSOCIATIVITY_LEFT);
            public static BinaryOperation BXOR = new BinaryOperation("~", PRECEDENCE_BXOR, ASSOCIATIVITY_LEFT);
            public static BinaryOperation CONCAT = new BinaryOperation("..", PRECEDENCE_CONCAT, ASSOCIATIVITY_RIGHT);
            public static BinaryOperation DIV = new BinaryOperation("/", PRECEDENCE_MUL, ASSOCIATIVITY_LEFT);
            public static BinaryOperation IDIV = new BinaryOperation("//", PRECEDENCE_MUL, ASSOCIATIVITY_LEFT);
            public static BinaryOperation MOD = new BinaryOperation("%", PRECEDENCE_MUL, ASSOCIATIVITY_LEFT);
            public static BinaryOperation MUL = new BinaryOperation("*", PRECEDENCE_MUL, ASSOCIATIVITY_LEFT);
            public static BinaryOperation OR = new BinaryOperation("or", PRECEDENCE_OR, ASSOCIATIVITY_NONE);
            public static BinaryOperation POW = new BinaryOperation("^", PRECEDENCE_POW, ASSOCIATIVITY_RIGHT);
            public static BinaryOperation SHL = new BinaryOperation("<<", PRECEDENCE_SHIFT, ASSOCIATIVITY_LEFT);
            public static BinaryOperation SHR = new BinaryOperation(">>", PRECEDENCE_SHIFT, ASSOCIATIVITY_LEFT);
            public static BinaryOperation SUB = new BinaryOperation("-", PRECEDENCE_ADD, ASSOCIATIVITY_LEFT);

            private readonly int associativity;
            private readonly string op;
            private readonly int precedence;
            
            public int Associativity => associativity;
            public string Op => op;
            public int Precedence => precedence;

            private BinaryOperation(string op, int precedence, int associativity)
            {
                this.op = op;
                this.precedence = precedence;
                this.associativity = associativity;
            }
        }

        public class UnaryOperation
        {
            private readonly string op;
            public static UnaryOperation BNOT = new UnaryOperation("~");
            public static UnaryOperation LEN = new UnaryOperation("#");
            public static UnaryOperation NOT = new UnaryOperation("not ");
            public static UnaryOperation UNM = new UnaryOperation("-");
            public string Op => op;

            private UnaryOperation(String op)
            {
                this.op = op;
            }
        }

        public static readonly int ASSOCIATIVITY_LEFT = 1;
        public static readonly int ASSOCIATIVITY_NONE = 0;
        public static readonly int ASSOCIATIVITY_RIGHT = 2;
        public static readonly int PRECEDENCE_ADD = 9;
        public static readonly int PRECEDENCE_AND = 2;
        public static readonly int PRECEDENCE_ATOMIC = 13;
        public static readonly int PRECEDENCE_BAND = 6;
        public static readonly int PRECEDENCE_BOR = 4;
        public static readonly int PRECEDENCE_BXOR = 5;
        public static readonly int PRECEDENCE_COMPARE = 3;
        public static readonly int PRECEDENCE_CONCAT = 8;
        public static readonly int PRECEDENCE_MUL = 10;
        public static readonly int PRECEDENCE_OR = 1;
        public static readonly int PRECEDENCE_POW = 12;
        public static readonly int PRECEDENCE_SHIFT = 7;
        public static readonly int PRECEDENCE_UNARY = 11;
        public readonly int precedence;

        public IExpression(int precedence)
        {
            this.precedence = precedence;
        }

        protected static void WriteBinary(Decompiler d, Output output, String op, IExpression left, IExpression right)
        {
            left.Write(d, output);
            output.Write(" ");
            output.Write(op);
            output.Write(" ");
            right.Write(d, output);
        }

        protected static void WriteUnary(Decompiler d, Output output, String op, IExpression expression)
        {
            output.Write(op);
            expression.Write(d, output);
        }

        public static BinaryExpression Make(BinaryOperation op, IExpression left, IExpression right, bool flip)
        {
            if (flip)
            {
                IExpression swap = left;
                left = right;
                right = swap;
            }
            return new BinaryExpression(op.Op, left, right, op.Precedence, op.Associativity);
        }

        public static BinaryExpression Make(BinaryOperation op, IExpression left, IExpression right)
        {
            return Make(op, left, right, false);
        }

        public static UnaryExpression Make(UnaryOperation op, IExpression expression)
        {
            return new UnaryExpression(op.Op, expression, PRECEDENCE_UNARY);
        }

        public static void WriteSequence(Decompiler d, Output output, List<IExpression> exprs, bool linebreak, bool multiple)
        {
            int n = exprs.Count;
            int i = 1;
            foreach (IExpression expr in exprs)
            {
                bool last = (i == n);
                if (expr.IsMultiple())
                {
                    last = true;
                }
                if (last)
                {
                    if (multiple)
                    {
                        expr.WriteMultiple(d, output);
                    }
                    else
                    {
                        expr.Write(d, output);
                    }
                    break;
                }
                else
                {
                    expr.Write(d, output);
                    output.Write(",");
                    if (linebreak)
                    {
                        output.WriteLine();
                    }
                    else
                    {
                        output.Write(" ");
                    }
                }
                i++;
            }
        }

        public virtual void AddEntry(TableLiteral.Entry entry)
        {
            throw new System.NotImplementedException();
        }

        public virtual int AsInteger()
        {
            throw new System.NotImplementedException();
        }

        public virtual string AsName()
        {
            throw new System.NotImplementedException();
        }

        public virtual bool BeginsWithParen()
        {
            return false;
        }

        public virtual int ClosureUpvalueLine()
        {
            throw new System.NotImplementedException();
        }

        public abstract int GetConstantIndex();

        public virtual int GetConstantLine()
        {
            return -1;
        }

        public virtual string GetField()
        {
            throw new System.NotImplementedException();
        }

        public virtual IExpression GetTable()
        {
            throw new System.NotImplementedException();
        }

        public virtual bool IsBool()
        {
            return false;
        }

        public virtual bool IsBrief()
        {
            return false;
        }

        public virtual bool IsClosure()
        {
            return false;
        }

        public virtual bool IsConstant()
        {
            return false;
        }

        public virtual bool IsDotChain()
        {
            return false;
        }

        public virtual bool IsEnvironmentTable(Decompiler d)
        {
            return false;
        }

        public virtual bool IsIdentifier()
        {
            return false;
        }

        public virtual bool IsInteger()
        {
            return false;
        }

        public virtual bool IsMemberAccess()
        {
            return false;
        }

        public virtual bool IsMultiple()
        {
            return false;
        }

        public virtual bool IsNewEntryAllowed()
        {
            throw new System.NotImplementedException();
        }

        public virtual bool IsNil()
        {
            return false;
        }

        public virtual bool IsString()
        {
            return false;
        }

        public virtual bool IsTableLiteral()
        {
            return false;
        }

        public virtual bool IsUngrouped()
        {
            return false;
        }

        // Only supported for closures
        public virtual bool IsUpvalueOf(int register)
        {
            throw new System.NotImplementedException();
        }

        public abstract void Write(Decompiler d, Output output);

        public virtual void WriteBraced(Decompiler d, Output output)
        {
            Write(d, output);
        }

        public virtual void WriteClosure(Decompiler d, Output output, ITarget name)
        {
            throw new System.NotImplementedException();
        }

        public virtual void WriteMultiple(Decompiler d, Output output)
        {
            Write(d, output);
        }

        public abstract void Walk(Walker w);
    }
}