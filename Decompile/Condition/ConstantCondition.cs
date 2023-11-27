using LuaDec.Decompile.Expression;

namespace LuaDec.Decompile.Condition
{
    public class ConstantCondition : ICondition
    {
        private int reg;
        private bool value;

        public ConstantCondition(int reg, bool value)
        {
            this.reg = reg;
            this.value = value;
        }

        public override IExpression AsExpression(Registers r)
        {
            return ConstantExpression.CreateBool(value);
        }

        public override ICondition Inverse()
        {
            return new ConstantCondition(reg, !value);
        }

        public override bool Invertible()
        {
            return true;
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
    }
}