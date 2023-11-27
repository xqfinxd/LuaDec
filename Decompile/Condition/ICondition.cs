using LuaDec.Decompile.Expression;

namespace LuaDec.Decompile.Condition
{
    public abstract class ICondition
    {
        public enum OperandType
        {
            R,
            RK,
            K,
            I,
            F,
        }

        public class Operand
        {
            public readonly OperandType type;

            public readonly int value;

            public Operand(OperandType type, int value)
            {
                this.type = type;
                this.value = value;
            }

            public IExpression AsExpression(Registers r, int line)
            {
                switch (type)
                {
                    case OperandType.R: return r.GetExpression(value, line);
                    case OperandType.RK: return r.GetKExpression(value, line);
                    case OperandType.K: return r.GetFunction().GetConstantExpression(value);
                    case OperandType.I: return ConstantExpression.createint(value);
                    case OperandType.F: return ConstantExpression.createDouble(value);
                    default: throw new System.InvalidOperationException();
                }
            }

            public int GetUpdated(Registers r, int line)
            {
                switch (type)
                {
                    case OperandType.R:
                        return r.GetUpdated(value, line);
                    case OperandType.RK:
                        if (r.IsKConstant(value))
                            throw new System.InvalidOperationException();
                        return r.GetUpdated(value, line);

                    default: throw new System.InvalidOperationException();
                }
            }

            public bool IsRegister(Registers r)
            {
                switch (type)
                {
                    case OperandType.R: return true;
                    case OperandType.RK: return !r.IsKConstant(value);
                    case OperandType.K: return false;
                    case OperandType.I: return false;
                    case OperandType.F: return false;
                    default: throw new System.InvalidOperationException();
                }
            }
        }

        public abstract IExpression AsExpression(Registers r);

        public abstract ICondition Inverse();

        public abstract bool Invertible();

        public abstract bool IsOrCondition();

        public abstract bool IsRegisterTest();

        public abstract bool IsSplitable();

        public abstract int Register();

        public abstract ICondition[] Split();
    }
}