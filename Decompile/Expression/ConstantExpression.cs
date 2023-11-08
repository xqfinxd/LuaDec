using LuaDec.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Expression
{
    public class ConstantExpression : IExpression
    {

        private readonly Constant constant;
        private readonly bool identifier;
        private readonly int index;
        private readonly int line;

        public static ConstantExpression createNil(int line)
        {
            return new ConstantExpression(new Constant(LNil.NIL), false, -1, line);
        }

        public static ConstantExpression createbool(bool v)
        {
            return new ConstantExpression(new Constant(v ? LBoolean.LTRUE : LBoolean.LFALSE), false, -1);
        }

        public static ConstantExpression createint(int i)
        {
            return new ConstantExpression(new Constant(i), false, -1);
        }

        public static ConstantExpression createDouble(double x)
        {
            return new ConstantExpression(new Constant(x), false, -1);
        }

        private static int getPrecedence(Constant constant)
        {
            if (constant.isNumber() && constant.isNegative())
            {
                return PRECEDENCE_UNARY;
            }
            else
            {
                return PRECEDENCE_ATOMIC;
            }
        }

        public ConstantExpression(Constant constant, bool identifier, int index)
            : this(constant, identifier, index, -1)
        {
        }

        private ConstantExpression(Constant constant, bool identifier, int index, int line)
            : base(getPrecedence(constant))
        {
          this.constant = constant;
            this.identifier = identifier;
            this.index = index;
            this.line = line;
        }

        public override void walk(Walker w)
        {
            w.visitExpression(this);
        }

        public override int getConstantIndex()
        {
            return index;
        }

        public override int getConstantLine()
        {
            return line;
        }

        public override void print(Decompiler d, Output output)
        {
            constant.print(d, output, false);
        }

        public override void printBraced(Decompiler d, Output output)
        {
            constant.print(d, output, true);
        }

        public override bool isConstant()
        {
            return true;
        }

        public override bool isUngrouped()
        {
            return true;
        }

        public override bool isNil()
        {
            return constant.isNil();
        }

        public override bool isbool()
        {
            return constant.isbool();
        }

        public override bool isInteger()
        {
            return constant.isint();
        }

        public override int asInteger()
        {
            return constant.asint();
        }

        public override bool isString()
        {
            return constant.isstring();
        }

        public override bool isIdentifier()
        {
            return identifier;
        }

        public override string asName()
        {
            return constant.asName();
        }

        public override bool isBrief()
        {
            return !constant.isstring() || constant.asName().Length <= 10;
        }

    }

}
