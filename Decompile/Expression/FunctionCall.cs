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

        public override void walk(Walker w)
        {
            w.VisitExpression(this);
            function.walk(w);
            foreach (IExpression expression in arguments)
            {
                expression.walk(w);
            }
        }

        public override int getConstantIndex()
        {
            int index = function.getConstantIndex();
            foreach (IExpression argument in arguments)
            {
                index = Math.Max(argument.getConstantIndex(), index);
            }
            return index;
        }

        public override bool isMultiple()
        {
            return multiple;
        }

        public override void printMultiple(Decompiler d, Output output)
        {
            if (!multiple)
            {
                output.WriteString("(");
            }
            print(d, output);
            if (!multiple)
            {
                output.WriteString(")");
            }
        }

        private bool isMethodCall()
        {
            return function.isMemberAccess() && arguments.Length > 0 && function.getTable() == arguments[0];
        }

        public override bool beginsWithParen()
        {
            if (isMethodCall())
            {
                IExpression obj = function.getTable();
                return obj.isUngrouped() || obj.beginsWithParen();
            }
            else
            {
                return function.isUngrouped() || function.beginsWithParen();
            }
        }

        public override void print(Decompiler d, Output output)
        {
            List<IExpression> args = new List<IExpression>(arguments.Length);
            if (isMethodCall())
            {
                IExpression obj = function.getTable();
                if (obj.isUngrouped())
                {
                    output.WriteString("(");
                    obj.print(d, output);
                    output.WriteString(")");
                }
                else
                {
                    obj.print(d, output);
                }
                output.WriteString(":");
                output.WriteString(function.getField());
                for (int i = 1; i < arguments.Length; i++)
                {
                    args.Add(arguments[i]);
                }
            }
            else
            {
                if (function.isUngrouped())
                {
                    output.WriteString("(");
                    function.print(d, output);
                    output.WriteString(")");
                }
                else
                {
                    function.print(d, output);
                }
                for (int i = 0; i < arguments.Length; i++)
                {
                    args.Add(arguments[i]);
                }
            }
            output.WriteString("(");
            IExpression.printSequence(d, output, args, false, true);
            output.WriteString(")");
        }

    }

}
