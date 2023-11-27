using LuaDec.Decompile.Expression;

namespace LuaDec.Decompile.Condition
{
    public class FinalSetCondition : ICondition
    {
        public enum Type
        {
            NONE,
            REGISTER,
            VALUE,
        }

        private int reg;
        public int line;
        public Type type;

        public FinalSetCondition(int line, int reg)
        {
            this.line = line;
            this.reg = reg;
            type = Type.NONE;
            if (reg < 0)
            {
                throw new System.InvalidOperationException();
            }
        }

        public override IExpression AsExpression(Registers r)
        {
            IExpression expr;
            switch (type)
            {
                case Type.REGISTER:
                    expr = r.GetExpression(Register(), line + 1);
                    break;

                case Type.VALUE:
                    expr = r.GetValue(Register(), line + 1);
                    break;

                case Type.NONE:
                default:
                    expr = ConstantExpression.CreateDouble(reg + (line) / 100.0);
                    break;
            }
            if (expr == null)
            {
                throw new System.InvalidOperationException();
            }
            return expr;
        }

        public override ICondition Inverse()
        {
            return new NotCondition(this);
        }

        public override bool Invertible()
        {
            return false;
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
            return reg;
        }

        public override ICondition[] Split()
        {
            throw new System.InvalidOperationException();
        }

        public override string ToString()
        {
            return "(" + reg + ")";
        }
    }
}