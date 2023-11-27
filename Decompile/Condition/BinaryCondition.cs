using LuaDec.Decompile.Expression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Condition
{
    public class BinaryCondition : ICondition
    {

        public enum Operator
        {
            EQ,
            LT,
            LE,
            GT,
            GE
        }

        private static string operator_to_string(Operator op, bool inverted, bool transposed)
        {
            switch (op)
            {
                case Operator.EQ: return inverted ? "~=" : "==";
                case Operator.LT: return transposed ? ">" : "<";
                case Operator.LE: return transposed ? ">=" : "<=";
                case Operator.GT: return transposed ? "<" : ">";
                case Operator.GE: return transposed ? "<=" : ">=";
            }
            throw new System.InvalidOperationException();
        }

        private readonly Operator op;
        private readonly int line;
        private readonly Operand left;
        private readonly Operand right;
        private readonly bool inverted;

        public BinaryCondition(Operator op, int line, Operand left, Operand right) : this(op, line, left, right, false)
        {
        }

        private BinaryCondition(Operator op, int line, Operand left, Operand right, bool inverted)
        {
            this.op = op;
            this.line = line;
            this.left = left;
            this.right = right;
            this.inverted = inverted;
        }

        public override ICondition Inverse()
        {
            if (op == Operator.EQ)
            {
                return new BinaryCondition(op, line, left, right, !inverted);
            }
            else
            {
                return new NotCondition(this);
            }
        }

        public override bool Invertible()
        {
            return op == Operator.EQ;
        }

        public override int Register()
        {
            return -1;
        }

        public override bool IsRegisterTest()
        {
            return false;
        }

        public override bool IsOrCondition()
        {
            return false;
        }

        public override bool IsSplitable()
        {
            return false;
        }

        public override ICondition[] Split()
        {
            throw new System.InvalidOperationException();
        }

        public override IExpression AsExpression(Registers r)
        {
            bool transpose = false;
            IExpression leftExpression = left.AsExpression(r, line);
            IExpression rightExpression = right.AsExpression(r, line);
            if (op != Operator.EQ || left.type == OperandType.K)
            {
                if (left.IsRegister(r) && right.IsRegister(r))
                {
                    transpose = left.GetUpdated(r, line) > right.GetUpdated(r, line);
                }
                else
                {
                    int rightIndex = rightExpression.GetConstantIndex();
                    int leftIndex = leftExpression.GetConstantIndex();
                    if (rightIndex != -1 && leftIndex != -1)
                    {
                        if (left.type == OperandType.K && rightIndex == leftIndex)
                        {
                            transpose = true;
                        }
                        else
                        {
                            transpose = rightIndex < leftIndex;
                        }
                    }
                }
            }
            string opstring = operator_to_string(op, inverted, transpose);
            IExpression rtn = new BinaryExpression(opstring, !transpose ? leftExpression : rightExpression, !transpose ? rightExpression : leftExpression, IExpression.PRECEDENCE_COMPARE, IExpression.ASSOCIATIVITY_LEFT);
            /*
            if(inverted) {
              rtn = new UnaryExpression("not ", rtn, Expression.PRECEDENCE_UNARY);
            }
            */
            return rtn;
        }

        public override string ToString()
        {
            return left + " " + operator_to_string(op, inverted, false) + " " + right;
        }

    }

}
