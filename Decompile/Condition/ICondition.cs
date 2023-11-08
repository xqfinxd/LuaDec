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
                    case OperandType.R: return r.getExpression(value, line);
                    case OperandType.RK: return r.getKExpression(value, line);
                    case OperandType.K: return r.getFunction().getConstantExpression(value);
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
                    case OperandType.RK: return !r.isKConstant(value);
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
                    case OperandType.R: return r.getUpdated(value, line);
                    case OperandType.RK:
                        if (r.isKConstant(value)) throw new System.InvalidOperationException();
                        return r.getUpdated(value, line);
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
