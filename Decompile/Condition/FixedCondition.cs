using LuaDec.Decompile.Expression;

namespace LuaDec.Decompile.Condition
{
    public class FixedCondition : ICondition
    {

        public static FixedCondition TRUE = new FixedCondition(ConstantExpression.CreateBool(true));

        private IExpression expression;

        private FixedCondition(IExpression expr)
        {
            expression = expr;
        }

        public override ICondition Inverse()
        {
            throw new System.InvalidOperationException();
        }

        public override bool Invertible()
        {
            return false;
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
            return expression;
        }

    }

}
