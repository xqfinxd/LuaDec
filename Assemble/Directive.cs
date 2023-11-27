using LuaDec.Decompile;
using LuaDec.Parser;
using System.Collections.Generic;

namespace LuaDec.Assemble
{
    public enum DirectiveT
    {
        FORMAT,
        ENDIANNESS,
        INT_SIZE,
        SIZE_T_SIZE,
        INSTRUCTION_SIZE,
        SIZE_OP,
        SIZE_A,
        SIZE_B,
        SIZE_C,
        NUMBER_FORMAT,
        INT_FORMAT,
        FLOAT_FORMAT,
        OP,
        FUNCTION,
        SOURCE,
        LINEDEFINED,
        LASTLINEDEFINED,
        NUMPARAMS,
        IS_VARARG,
        MAXSTACKSIZE,
        LABEL,
        CONSTANT,
        LINE,
        ABSLINEINFO,
        LOCAL,
        UPVALUE,
    }

    public enum DirectiveType
    {
        Header,
        NewFunction,
        Function,
        Instruction,
    }

    public class Directive
    {
        private readonly string token;
        public static readonly Directive ABSLINEINFO = new Directive(DirectiveT.ABSLINEINFO, ".abslineinfo", DirectiveType.Function, 2);
        public static readonly Directive CONSTANT = new Directive(DirectiveT.CONSTANT, ".constant", DirectiveType.Function, 2);
        public static readonly Directive ENDIANNESS = new Directive(DirectiveT.ENDIANNESS, ".endianness", DirectiveType.Header, 1);
        public static readonly Directive FLOAT_FORMAT = new Directive(DirectiveT.FLOAT_FORMAT, ".float_format", DirectiveType.Header, 1);
        public static readonly Directive FORMAT = new Directive(DirectiveT.FORMAT, ".format", DirectiveType.Header, 1);
        public static readonly Directive FUNCTION = new Directive(DirectiveT.FUNCTION, ".function", DirectiveType.NewFunction, 1);
        public static readonly Directive INSTRUCTION_SIZE = new Directive(DirectiveT.INSTRUCTION_SIZE, ".instruction_size", DirectiveType.Header, 1);
        public static readonly Directive INT_FORMAT = new Directive(DirectiveT.INT_FORMAT, ".int_format", DirectiveType.Header, 1);
        public static readonly Directive INT_SIZE = new Directive(DirectiveT.INT_SIZE, ".int_size", DirectiveType.Header, 1);
        public static readonly Directive IS_VARARG = new Directive(DirectiveT.IS_VARARG, ".is_vararg", DirectiveType.Function, 1);
        public static readonly Directive LABEL = new Directive(DirectiveT.LABEL, ".label", DirectiveType.Function, 1);
        public static readonly Directive LASTLINEDEFINED = new Directive(DirectiveT.LASTLINEDEFINED, ".lastlinedefined", DirectiveType.Function, 1);
        public static readonly Directive LINE = new Directive(DirectiveT.LINE, ".line", DirectiveType.Function, 1);
        public static readonly Directive LINEDEFINED = new Directive(DirectiveT.LINEDEFINED, ".linedefined", DirectiveType.Function, 1);
        public static readonly Directive LOCAL = new Directive(DirectiveT.LOCAL, ".local", DirectiveType.Function, 3);
        public static readonly Directive MAXSTACKSIZE = new Directive(DirectiveT.MAXSTACKSIZE, ".maxstacksize", DirectiveType.Function, 1);
        public static readonly Directive NUMBER_FORMAT = new Directive(DirectiveT.NUMBER_FORMAT, ".number_format", DirectiveType.Header, 2);
        public static readonly Directive NUMPARAMS = new Directive(DirectiveT.NUMPARAMS, ".numparams", DirectiveType.Function, 1);
        public static readonly Directive OP = new Directive(DirectiveT.OP, ".op", DirectiveType.Header, 2);
        public static readonly Directive SIZE_A = new Directive(DirectiveT.SIZE_A, ".size_a", DirectiveType.Header, 1);
        public static readonly Directive SIZE_B = new Directive(DirectiveT.SIZE_B, ".size_b", DirectiveType.Header, 1);
        public static readonly Directive SIZE_C = new Directive(DirectiveT.SIZE_C, ".size_c", DirectiveType.Header, 1);
        public static readonly Directive SIZE_OP = new Directive(DirectiveT.SIZE_OP, ".size_op", DirectiveType.Header, 1);
        public static readonly Directive SIZE_T_SIZE = new Directive(DirectiveT.SIZE_T_SIZE, ".size_t_size", DirectiveType.Header, 1);
        public static readonly Directive SOURCE = new Directive(DirectiveT.SOURCE, ".source", DirectiveType.Function, 1);
        public static readonly Directive UPVALUE = new Directive(DirectiveT.UPVALUE, ".upvalue", DirectiveType.Function, 2);

        public static Dictionary<string, Directive> lookup;

        public readonly DirectiveType type;

        public readonly DirectiveT value;

        public string Token => token;

        private Directive(DirectiveT value, string token, DirectiveType type, int argcount)
        {
            this.value = value;
            this.token = token;
            this.type = type;
            if (lookup == null)
            {
                lookup = new Dictionary<string, Directive>();
            }
            lookup.Add(token, this);
        }

        public void Disassemble(Output output, BHeader chunk, LHeader header)
        {
            output.WriteString(Token + "\t");
            switch (value)
            {
                case DirectiveT.FORMAT: output.WriteLine(header.format.ToString()); break;
                case DirectiveT.ENDIANNESS: output.WriteLine(header.endianness.ToString()); break;
                case DirectiveT.INT_SIZE: output.WriteLine(header.intT.getSize().ToString()); break;
                case DirectiveT.SIZE_T_SIZE: output.WriteLine(header.sizeT.getSize().ToString()); break;
                case DirectiveT.INSTRUCTION_SIZE: output.WriteLine("4"); break;
                case DirectiveT.SIZE_OP: output.WriteLine(header.extractor.op.Size.ToString()); break;
                case DirectiveT.SIZE_A: output.WriteLine(header.extractor.A.Size.ToString()); break;
                case DirectiveT.SIZE_B: output.WriteLine(header.extractor.B.Size.ToString()); break;
                case DirectiveT.SIZE_C: output.WriteLine(header.extractor.C.Size.ToString()); break;
                case DirectiveT.NUMBER_FORMAT: output.WriteLine((header.numberT.integral ? "int" : "float") + "\t" + header.numberT.size.ToString()); break;
                case DirectiveT.INT_FORMAT: output.WriteLine(header.longT.size.ToString()); break;
                case DirectiveT.FLOAT_FORMAT: output.WriteLine(header.doubleT.size.ToString()); break;
                default: throw new System.InvalidOperationException();
            }
        }

        public void Disassemble(Output output, BHeader chunk, LFunction function)
        {
            output.WriteString(this.Token + "\t");
            switch (value)
            {
                case DirectiveT.SOURCE: output.WriteLine(function.name.ToPrintable()); break;
                case DirectiveT.LINEDEFINED: output.WriteLine(function.lineDefined.ToString()); break;
                case DirectiveT.LASTLINEDEFINED: output.WriteLine(function.lastLineDefined.ToString()); break;
                case DirectiveT.NUMPARAMS: output.WriteLine(function.numParams.ToString()); break;
                case DirectiveT.IS_VARARG: output.WriteLine(function.varArg.ToString()); break;
                case DirectiveT.MAXSTACKSIZE: output.WriteLine(function.maximumStackSize.ToString()); break;
                default: throw new System.InvalidOperationException();
            }
        }
    }
}