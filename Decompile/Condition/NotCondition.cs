using LuaDec.Decompile.Expression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Condition
{
    public class NotCondition : ICondition
    {

        private ICondition cond;

        public NotCondition(ICondition cond)
        {
            this.cond = cond;
        }

        public override ICondition Inverse()
        {
            return cond;
        }

        public override bool Invertible()
        {
            return true;
        }

        public override int Register()
        {
            return cond.Register();
        }

        public override bool IsRegisterTest()
        {
            return cond.IsRegisterTest();
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
            return new UnaryExpression("not ", cond.AsExpression(r), IExpression.PRECEDENCE_UNARY);
        }

        public override string ToString()
        {
            return "not (" + cond + ")";
        }

    }

}
