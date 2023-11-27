using LuaDec.Decompile.Expression;

namespace LuaDec.Decompile.Condition
{
    public class TestCondition : ICondition
    {
        private int line;
        private int reg;

        public TestCondition(int line, int reg)
        {
            this.line = line;
            this.reg = reg;
        }

        public override IExpression AsExpression(Registers r)
        {
            return r.GetExpression(Register(), line);
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
            return true;
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