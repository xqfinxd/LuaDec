using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Expression
{
    public class FunctionCall : IExpression
    {

        private readonly IExpression function;
        private readonly IExpression[] arguments;
        private readonly bool multiple;

        public FunctionCall(IExpression function, IExpression[] arguments, bool multiple)
            : base(PRECEDENCE_ATOMIC)
        {
            this.function = function;
            this.arguments = arguments;
            this.multiple = multiple;
        }

        public override void Walk(Walker w)
        {
            w.VisitExpression(this);
            function.Walk(w);
            foreach (IExpression expression in arguments)
            {
                expression.Walk(w);
            }
        }

        public override int GetConstantIndex()
        {
            int index = function.GetConstantIndex();
            foreach (IExpression argument in arguments)
            {
                index = Math.Max(argument.GetConstantIndex(), index);
            }
            return index;
        }

        public override bool IsMultiple()
        {
            return multiple;
        }

        public override void WriteMultiple(Decompiler d, Output output)
        {
            if (!multiple)
            {
                output.WriteString("(");
            }
            Write(d, output);
            if (!multiple)
            {
                output.WriteString(")");
            }
        }

        private bool isMethodCall()
        {
            return function.IsMemberAccess() && arguments.Length > 0 && function.GetTable() == arguments[0];
        }

        public override bool BeginsWithParen()
        {
            if (isMethodCall())
            {
                IExpression obj = function.GetTable();
                return obj.IsUngrouped() || obj.BeginsWithParen();
            }
            else
            {
                return function.IsUngrouped() || function.BeginsWithParen();
            }
        }

        public override void Write(Decompiler d, Output output)
        {
            List<IExpression> args = new List<IExpression>(arguments.Length);
            if (isMethodCall())
            {
                IExpression obj = function.GetTable();
                if (obj.IsUngrouped())
                {
                    output.WriteString("(");
                    obj.Write(d, output);
                    output.WriteString(")");
                }
                else
                {
                    obj.Write(d, output);
                }
                output.WriteString(":");
                output.WriteString(function.GetField());
                for (int i = 1; i < arguments.Length; i++)
                {
                    args.Add(arguments[i]);
                }
            }
            else
            {
                if (function.IsUngrouped())
                {
                    output.WriteString("(");
                    function.Write(d, output);
                    output.WriteString(")");
                }
                else
                {
                    function.Write(d, output);
                }
                for (int i = 0; i < arguments.Length; i++)
                {
                    args.Add(arguments[i]);
                }
            }
            output.WriteString("(");
            IExpression.WriteSequence(d, output, args, false, true);
            output.WriteString(")");
        }

    }

}
