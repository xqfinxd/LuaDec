using LuaDec.Decompile.Target;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Expression
{
    public abstract class IExpression
    {
        public static readonly int PRECEDENCE_OR = 1;
        public static readonly int PRECEDENCE_AND = 2;
        public static readonly int PRECEDENCE_COMPARE = 3;
        public static readonly int PRECEDENCE_BOR = 4;
        public static readonly int PRECEDENCE_BXOR = 5;
        public static readonly int PRECEDENCE_BAND = 6;
        public static readonly int PRECEDENCE_SHIFT = 7;
        public static readonly int PRECEDENCE_CONCAT = 8;
        public static readonly int PRECEDENCE_ADD = 9;
        public static readonly int PRECEDENCE_MUL = 10;
        public static readonly int PRECEDENCE_UNARY = 11;
        public static readonly int PRECEDENCE_POW = 12;
        public static readonly int PRECEDENCE_ATOMIC = 13;

        public static readonly int ASSOCIATIVITY_NONE = 0;
        public static readonly int ASSOCIATIVITY_LEFT = 1;
        public static readonly int ASSOCIATIVITY_RIGHT = 2;

        public class BinaryOperation
        {
            public static BinaryOperation CONCAT = new BinaryOperation("..", PRECEDENCE_CONCAT, ASSOCIATIVITY_RIGHT);
            public static BinaryOperation ADD = new BinaryOperation("+", PRECEDENCE_ADD, ASSOCIATIVITY_LEFT);
            public static BinaryOperation SUB = new BinaryOperation("-", PRECEDENCE_ADD, ASSOCIATIVITY_LEFT);
            public static BinaryOperation MUL = new BinaryOperation("*", PRECEDENCE_MUL, ASSOCIATIVITY_LEFT);
            public static BinaryOperation DIV = new BinaryOperation("/", PRECEDENCE_MUL, ASSOCIATIVITY_LEFT);
            public static BinaryOperation IDIV = new BinaryOperation("//", PRECEDENCE_MUL, ASSOCIATIVITY_LEFT);
            public static BinaryOperation MOD = new BinaryOperation("%", PRECEDENCE_MUL, ASSOCIATIVITY_LEFT);
            public static BinaryOperation POW = new BinaryOperation("^", PRECEDENCE_POW, ASSOCIATIVITY_RIGHT);
            public static BinaryOperation BAND = new BinaryOperation("&", PRECEDENCE_BAND, ASSOCIATIVITY_LEFT);
            public static BinaryOperation BOR = new BinaryOperation("|", PRECEDENCE_BOR, ASSOCIATIVITY_LEFT);
            public static BinaryOperation BXOR = new BinaryOperation("~", PRECEDENCE_BXOR, ASSOCIATIVITY_LEFT);
            public static BinaryOperation SHL = new BinaryOperation("<<", PRECEDENCE_SHIFT, ASSOCIATIVITY_LEFT);
            public static BinaryOperation SHR = new BinaryOperation(">>", PRECEDENCE_SHIFT, ASSOCIATIVITY_LEFT);
            public static BinaryOperation OR = new BinaryOperation("or", PRECEDENCE_OR, ASSOCIATIVITY_NONE);
            public static BinaryOperation AND = new BinaryOperation("and", PRECEDENCE_AND, ASSOCIATIVITY_NONE);

            private readonly string op;
            private readonly int precedence;
            private readonly int associativity;

            public string Op => op;
            public int Precedence => precedence;
            public int Associativity => associativity;

            private BinaryOperation(string op, int precedence, int associativity)
            {
                this.op = op;
                this.precedence = precedence;
                this.associativity = associativity;
            }
        }

        public class UnaryOperation
        {
            public static UnaryOperation UNM = new UnaryOperation("-");
            public static UnaryOperation NOT = new UnaryOperation("not ");
            public static UnaryOperation LEN = new UnaryOperation("#");
            public static UnaryOperation BNOT = new UnaryOperation("~");


            private readonly string op;

            private UnaryOperation(String op)
            {
                this.op = op;
            }

            public string Op => op;
        }

        public static BinaryExpression make(BinaryOperation op, IExpression left, IExpression right, bool flip)
        {
            if (flip)
            {
                IExpression swap = left;
                left = right;
                right = swap;
            }
            return new BinaryExpression(op.Op, left, right, op.Precedence, op.Associativity);
        }

        public static BinaryExpression make(BinaryOperation op, IExpression left, IExpression right)
        {
            return make(op, left, right, false);
        }

        public static UnaryExpression make(UnaryOperation op, IExpression expression)
        {
            return new UnaryExpression(op.Op, expression, PRECEDENCE_UNARY);
        }

        /**
         * Prints out a sequences of expressions with commas, and optionally
         * handling multiple expressions and return value adjustment.
         */
        public static void printSequence(Decompiler d, Output output, List<IExpression> exprs, bool linebreak, bool multiple)
        {
            int n = exprs.Count;
            int i = 1;
            foreach (IExpression expr in exprs)
            {
                bool last = (i == n);
                if (expr.isMultiple())
                {
                    last = true;
                }
                if (last)
                {
                    if (multiple)
                    {
                        expr.printMultiple(d, output);
                    }
                    else
                    {
                        expr.print(d, output);
                    }
                    break;
                }
                else
                {
                    expr.print(d, output);
                    output.print(",");
                    if (linebreak)
                    {
                        output.println();
                    }
                    else
                    {
                        output.print(" ");
                    }
                }
                i++;
            }
        }

        public readonly int precedence;

        public IExpression(int precedence)
        {
            this.precedence = precedence;
        }

        protected static void printUnary(Decompiler d, Output output, String op, IExpression expression)
        {
            output.print(op);
            expression.print(d, output);
        }

        protected static void printBinary(Decompiler d, Output output, String op, IExpression left, IExpression right)
        {
            left.print(d, output);
            output.print(" ");
            output.print(op);
            output.print(" ");
            right.print(d, output);
        }

        public abstract void walk(Walker w);

        public abstract void print(Decompiler d, Output output);

        /**
         * Prints the expression in a context where it is surrounded by braces.
         * (Thus if the expression would begin with a brace, it must be enclosed
         * in parentheses to avoid ambiguity.)
         */
        public virtual void printBraced(Decompiler d, Output output)
        {
            print(d, output);
        }

        /**
         * Prints the expression in a context that accepts multiple values.
         * (Thus, if an expression that normally could return multiple values
         * doesn't, it should use parens to adjust to 1.)
         */
        public virtual void printMultiple(Decompiler d, Output output)
        {
            print(d, output);
        }

        /**
         * Determines the index of the last-declared constant in this expression.
         * If there is no constant in the expression, return -1.
         */
        public abstract int getConstantIndex();

        public virtual int getConstantLine()
        {
            return -1;
        }

        public virtual bool beginsWithParen()
        {
            return false;
        }

        public virtual bool isNil()
        {
            return false;
        }

        public virtual bool isClosure()
        {
            return false;
        }

        public virtual bool isConstant()
        {
            return false;
        }

        /**
         * An ungrouped expression is one that needs to be enclosed in parentheses
         * before it can be dereferenced. This doesn't apply to multiply-valued expressions
         * as those will be given parentheses automatically when converted to a single value.
         * e.g.
         *  (a+b).c; ("asdf"):gsub()
         */
        public virtual bool isUngrouped()
        {
            return false;
        }

        // Only supported for closures
        public virtual bool isUpvalueOf(int register)
        {
            throw new System.InvalidOperationException();
        }

        public virtual bool isbool()
        {
            return false;
        }

        public virtual bool isInteger()
        {
            return false;
        }

        public virtual int asInteger()
        {
            throw new System.InvalidOperationException();
        }

        public virtual bool isString()
        {
            return false;
        }

        public virtual bool isIdentifier()
        {
            return false;
        }

        /**
         * Determines if this can be part of a function name.
         * Is it of the form: {Name . } Name
         */
        public virtual bool isDotChain()
        {
            return false;
        }

        public virtual int closureUpvalueLine()
        {
            throw new System.InvalidOperationException();
        }

        public virtual void printClosure(Decompiler d, Output output, ITarget name)
        {
            throw new System.InvalidOperationException();
        }

        public virtual string asName()
        {
            throw new System.InvalidOperationException();
        }

        public virtual bool isTableLiteral()
        {
            return false;
        }

        public virtual bool isNewEntryAllowed()
        {
            throw new System.InvalidOperationException();
        }

        public virtual void addEntry(TableLiteral.Entry entry)
        {
            throw new System.InvalidOperationException();
        }

        /**
         * Whether the expression has more than one return stored into registers.
         */
        public virtual bool isMultiple()
        {
            return false;
        }

        public virtual bool isMemberAccess()
        {
            return false;
        }

        public virtual IExpression getTable()
        {
            throw new System.InvalidOperationException();
        }

        public virtual string getField()
        {
            throw new System.InvalidOperationException();
        }

        public virtual bool isBrief()
        {
            return false;
        }

        public virtual bool isEnvironmentTable(Decompiler d)
        {
            return false;
        }

    }

}
