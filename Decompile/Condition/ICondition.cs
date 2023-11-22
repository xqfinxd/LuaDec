using LuaDec.Decompile.Expression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            public Operand(OperandType type, int value)
            {
                this.type = type;
                this.value = value;
            }

            public IExpression asExpression(Registers r, int line)
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

            public bool isRegister(Registers r)
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

            public int getUpdated(Registers r, int line)
            {
                switch (type)
                {
                    case OperandType.R: return r.GetUpdated(value, line);
                    case OperandType.RK:
                        if (r.IsKConstant(value)) throw new System.InvalidOperationException();
                        return r.GetUpdated(value, line);
                    default: throw new System.InvalidOperationException();
                }
            }

            public readonly OperandType type;
            public readonly int value;

        }

        public abstract ICondition inverse();

        public abstract bool invertible();

        public abstract int register();

        public abstract bool isRegisterTest();

        public abstract bool isOrCondition();

        public abstract bool isSplitable();

        public abstract ICondition[] split();

        public abstract IExpression asExpression(Registers r);

        // public override string ToString();

    }

}
