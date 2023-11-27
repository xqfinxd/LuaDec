using LuaDec.Decompile.Expression;

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

        private readonly bool inverted;

        private readonly Operand left;

        private readonly int line;

        private readonly Operator op;

        private readonly Operand right;

        private BinaryCondition(Operator op, int line, Operand left, Operand right, bool inverted)
        {
            this.op = op;
            this.line = line;
            this.left = left;
            this.right = right;
            this.inverted = inverted;
        }

        public BinaryCondition(Operator op, int line, Operand left, Operand right) : this(op, line, left, right, false)
        {
        }

        private static string Operator2String(Operator op, bool inverted, bool transposed)
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
            string opstring = Operator2String(op, inverted, transpose);
            IExpression rtn = new BinaryExpression(
                opstring,
                !transpose ? leftExpression : rightExpression,
                !transpose ? rightExpression : leftExpression,
                IExpression.PRECEDENCE_COMPARE,
                IExpression.ASSOCIATIVITY_LEFT);
            return rtn;
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

        public override bool IsOrCondition()
        {
            return false;
        }

        public override bool IsRegisterTest()
        {
            return false;
        }

        public override bool IsSplitable()
        {
            return false;
        }

        public override int Register()
        {
            return -1;
        }

        public override ICondition[] Split()
        {
            throw new System.InvalidOperationException();
        }

        public override string ToString()
        {
            return left + " " + Operator2String(op, inverted, false) + " " + right;
        }
    }
}