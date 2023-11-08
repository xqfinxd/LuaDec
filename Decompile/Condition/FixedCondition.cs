using LuaDec.Decompile.Expression;

namespace LuaDec.Decompile.Condition
{
    public class FixedCondition : ICondition
    {

        public static FixedCondition TRUE = new FixedCondition(ConstantExpression.createbool(true));

        private IExpression expression;

        private FixedCondition(IExpression expr)
        {
            expression = expr;
        }

        public override ICondition inverse()
        {
            throw new System.InvalidOperationException();
        }

        public override bool invertible()
        {
            return false;
        }

        public override int register()
        {
            return -1;
        }

        public override bool isRegisterTest()
        {
            return false;
        }

        public override bool isOrCondition()
        {
            return false;
        }

        public override bool isSplitable()
        {
            return false;
        }

        public override ICondition[] split()
        {
            throw new System.InvalidOperationException();
        }

        public override IExpression asExpression(Registers r)
        {
            return expression;
        }

    }

}
