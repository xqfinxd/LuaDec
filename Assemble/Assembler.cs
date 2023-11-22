using LuaDec.Decompile;
using LuaDec.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using LuaDec.Util;

namespace LuaDec.Assemble
{

    class AssemblerLabel
    {

        public string name;
        public int code_index;

    }

    class AssemblerConstant
    {

        public enum Type
        {
            NIL,
            BOOL,
            NUMBER,
            INT,
            FLOAT,
            STRING,
            LONGSTRING,
        }

        public string name;
        public Type type;

        public bool boolValue;
        public double numberValue;
        public string stringValue;
        public BigInteger intValue;
    }

    class AssemblerAbsLineInfo
    {

        public int pc;
        public int line;

    }

    class AssemblerLocal
    {

        public string name;
        public int begin;
        public int end;

    }

    class AssemblerUpvalue
    {

        public string name;
        public int index;
        public bool instack;

    }
    class AssemblerFunction
    {

        public class FunctionFixup
        {

            public int code_index;
            public string function;
            public CodeExtract.Field field;

        }

        public class JumpFixup
        {

            public int code_index;
            public string label;
            public CodeExtract.Field field;
            public bool negate;

        }

        public AssemblerChunk chunk;
        public AssemblerFunction parent;
        public string name;
        public List<AssemblerFunction> children;

        public bool hasSource;
        public string source;

        public bool hasLineDefined;
        public int linedefined;

        public bool hasLastLineDefined;
        public int lastlinedefined;

        public bool hasMaxStackSize;
        public int maxStackSize;

        public bool hasNumParams;
        public int numParams;

        public bool hasVararg;
        public int vararg;

        public List<AssemblerLabel> labels;
        public List<AssemblerConstant> constants;
        public List<AssemblerUpvalue> upvalues;
        public List<int> code;
        public List<int> lines;
        public List<AssemblerAbsLineInfo> abslineinfo;
        public List<AssemblerLocal> locals;

        public List<FunctionFixup> f_fixup;
        public List<JumpFixup> j_fixup;

        public AssemblerFunction(AssemblerChunk chunk, AssemblerFunction parent, string name)
        {
            this.chunk = chunk;
            this.parent = parent;
            this.name = name;
            children = new List<AssemblerFunction>();

            hasSource = false;
            hasLineDefined = false;
            hasLastLineDefined = false;
            hasMaxStackSize = false;
            hasNumParams = false;
            hasVararg = false;

            labels = new List<AssemblerLabel>();
            constants = new List<AssemblerConstant>();
            upvalues = new List<AssemblerUpvalue>();
            code = new List<int>();
            lines = new List<int>();
            abslineinfo = new List<AssemblerAbsLineInfo>();
            locals = new List<AssemblerLocal>();

            f_fixup = new List<FunctionFixup>();
            j_fixup = new List<JumpFixup>();
        }

        public AssemblerFunction addChild(string name)
        {
            AssemblerFunction child = new AssemblerFunction(chunk, this, name);
            children.Add(child);
            return child;
        }

        public AssemblerFunction getInnerParent(string[] parts, int index)
        {
            if (index + 1 == parts.Length) return this;
            foreach (AssemblerFunction child in children)
            {
                if (child.name == parts[index])
                {
                    return child.getInnerParent(parts, index + 1);
                }
            }
            throw new AssemblerException("Can't find outer function");
        }

        public void processFunctionDirective(Assembler a, Directive d)
        {
            switch (d.value)
            {
                case DirectiveT.SOURCE:
                    if (hasSource) throw new AssemblerException("Duplicate .source directive");
                    hasSource = true;
                    source = a.getstring();
                    break;
                case DirectiveT.LINEDEFINED:
                    if (hasLineDefined)
                        throw new AssemblerException("Duplicate .linedefined directive");
                    hasLineDefined = true;
                    linedefined = a.getInteger();
                    break;
                case DirectiveT.LASTLINEDEFINED:
                    if (hasLastLineDefined)
                        throw new AssemblerException("Duplicate .lastlinedefined directive");
                    hasLastLineDefined = true;
                    lastlinedefined = a.getInteger();
                    break;
                case DirectiveT.MAXSTACKSIZE:
                    if (hasMaxStackSize)
                        throw new AssemblerException("Duplicate .maxstacksize directive");
                    hasMaxStackSize = true;
                    maxStackSize = a.getInteger();
                    break;
                case DirectiveT.NUMPARAMS:
                    if (hasNumParams) throw new AssemblerException("Duplicate .numparams directive");
                    hasNumParams = true;
                    numParams = a.getInteger();
                    break;
                case DirectiveT.IS_VARARG:
                    if (hasVararg) throw new AssemblerException("Duplicate .is_vararg directive");
                    hasVararg = true;
                    vararg = a.getInteger();
                    break;
                case DirectiveT.LABEL:
                    {
                        string name = a.getAny();
                        AssemblerLabel label = new AssemblerLabel();
                        label.name = name;
                        label.code_index = code.Count;
                        labels.Add(label);
                        break;
                    }
                case DirectiveT.CONSTANT:
                    {
                        string name = a.getName();
                        string value = a.getAny();
                        AssemblerConstant constant = new AssemblerConstant();
                        constant.name = name;
                        if (value == "nil")
                        {
                            constant.type = AssemblerConstant.Type.NIL;
                        }
                        else if (value == "true")
                        {
                            constant.type = AssemblerConstant.Type.BOOL;
                            constant.boolValue = true;
                        }
                        else if (value == "false")
                        {
                            constant.type = AssemblerConstant.Type.BOOL;
                            constant.boolValue = false;
                        }
                        else if (value.StartsWith("\""))
                        {
                            constant.type = AssemblerConstant.Type.STRING;
                            constant.stringValue = StringUtils.fromPrintString(value);
                        }
                        else if (value.StartsWith("L\""))
                        {
                            constant.type = AssemblerConstant.Type.LONGSTRING;
                            constant.stringValue = StringUtils.fromPrintString(value.Substring(1));
                        }
                        else if (value == "null")
                        {
                            constant.type = AssemblerConstant.Type.STRING;
                            constant.stringValue = null;
                        }
                        else
                        {
                            try
                            {
                                // TODO: better check
                                if (chunk.number != null)
                                {
                                    constant.numberValue = double.Parse(value);
                                    constant.type = AssemblerConstant.Type.NUMBER;
                                }
                                else
                                {
                                    if (value.Contains(".") || value.Contains("E") || value.Contains("e"))
                                    {
                                        constant.numberValue = double.Parse(value);
                                        constant.type = AssemblerConstant.Type.FLOAT;
                                    }
                                    else
                                    {
                                        constant.intValue = BigInteger.Parse(value);
                                        constant.type = AssemblerConstant.Type.INT;
                                    }
                                }
                            }
                            catch (FormatException)
                            {
                                throw new System.InvalidOperationException("Unrecognized constant value: " + value);
                            }
                        }
                        constants.Add(constant);
                        break;
                    }
                case DirectiveT.LINE:
                    {
                        lines.Add(a.getInteger());
                        break;
                    }
                case DirectiveT.ABSLINEINFO:
                    {
                        AssemblerAbsLineInfo info = new AssemblerAbsLineInfo();
                        info.pc = a.getInteger();
                        info.line = a.getInteger();
                        abslineinfo.Add(info);
                        break;
                    }
                case DirectiveT.LOCAL:
                    {
                        AssemblerLocal local = new AssemblerLocal();
                        local.name = a.getstring();
                        local.begin = a.getInteger();
                        local.end = a.getInteger();
                        locals.Add(local);
                        break;
                    }
                case DirectiveT.UPVALUE:
                    {
                        AssemblerUpvalue upvalue = new AssemblerUpvalue();
                        upvalue.name = a.getstring();
                        upvalue.index = a.getInteger();
                        upvalue.instack = a.getBoolean();
                        upvalues.Add(upvalue);
                        break;
                    }
                default:
                    throw new System.InvalidOperationException("Unhandled directive: " + d);
            }
        }

        public void processOp(Assembler a, CodeExtract extract, Op op, int opcode)
        {
            if (!hasMaxStackSize) throw new AssemblerException("Expected .maxstacksize before code");
            if (opcode >= 0 && !extract.op.Check(opcode)) throw new System.InvalidOperationException("Invalid opcode: " + opcode);
            int codepoint = opcode >= 0 ? extract.op.Encode(opcode) : 0;
            foreach (OperandFormat operand in op.Operands)
            {
                CodeExtract.Field field;
                switch (operand.field)
                {
                    case OperandFormat.Field.A: field = extract.A; break;
                    case OperandFormat.Field.B: field = extract.B; break;
                    case OperandFormat.Field.C: field = extract.C; break;
                    case OperandFormat.Field.k: field = extract.k; break;
                    case OperandFormat.Field.Ax: field = extract.Ax; break;
                    case OperandFormat.Field.sJ: field = extract.sJ; break;
                    case OperandFormat.Field.Bx: field = extract.Bx; break;
                    case OperandFormat.Field.sBx: field = extract.sBx; break;
                    case OperandFormat.Field.x: field = extract.x; break;
                    default: throw new System.InvalidOperationException("Unhandled field: " + operand.field);
                }
                int x;
                switch (operand.format)
                {
                    case OperandFormat.Format.Raw:
                    case OperandFormat.Format.ImmediateUInt:
                    case OperandFormat.Format.ImmediateFloat:
                        x = a.getInteger();
                        break;
                    case OperandFormat.Format.ImmediateSInt:
                        x = a.getInteger();
                        x += field.Max() / 2;
                        break;
                    case OperandFormat.Format.Register:
                        {
                            x = a.getRegister();
                            //TODO: stack warning
                            break;
                        }
                    case OperandFormat.Format.RegisterK:
                        {
                            Assembler.RKInfo rk = a.getRegisterK54();
                            x = rk.x;
                            if (rk.constant)
                            {
                                x += chunk.version.rkOffset.Value;
                            }
                            //TODO: stack warning
                            break;
                        }
                    case OperandFormat.Format.RegisterK54:
                        {
                            Assembler.RKInfo rk = a.getRegisterK54();
                            codepoint |= extract.k.Encode(rk.constant ? 1 : 0);
                            x = rk.x;
                            break;
                        }
                    case OperandFormat.Format.Constant:
                    case OperandFormat.Format.ConstantNumber:
                    case OperandFormat.Format.ConstantString:
                        {
                            x = a.getConstant();
                            break;
                        }
                    case OperandFormat.Format.Upvalue:
                        {
                            x = a.getUpvalue();
                            break;
                        }
                    case OperandFormat.Format.Function:
                        {
                            FunctionFixup fix = new FunctionFixup();
                            fix.code_index = code.Count;
                            fix.function = a.getAny();
                            fix.field = field;
                            f_fixup.Add(fix);
                            x = 0;
                            break;
                        }
                    case OperandFormat.Format.Jump:
                        {
                            JumpFixup fix = new JumpFixup();
                            fix.code_index = code.Count;
                            fix.label = a.getAny();
                            fix.field = field;
                            fix.negate = false;
                            j_fixup.Add(fix);
                            x = 0;
                            break;
                        }
                    case OperandFormat.Format.JumpNegative:
                        {
                            JumpFixup fix = new JumpFixup();
                            fix.code_index = code.Count;
                            fix.label = a.getAny();
                            fix.field = field;
                            fix.negate = true;
                            j_fixup.Add(fix);
                            x = 0;
                            break;
                        }
                    default:
                        throw new System.InvalidOperationException("Unhandled operand foreach mat in " + operand.format);
                }
                if (!field.Check(x))
                {
                    throw new AssemblerException("Operand " + operand.field + " output of range");
                }
                codepoint |= field.Encode(x);
            }
            code.Add(codepoint);
        }

        public void fixup(CodeExtract extract)
        {
            foreach (FunctionFixup fix in f_fixup)
            {
                int codepoint = code[fix.code_index];
                int x = -1;
                for (int f = 0; f < children.Count; f++)
                {
                    AssemblerFunction child = children[f];
                    if (fix.function == child.name)
                    {
                        x = f;
                        break;
                    }
                }
                if (x == -1)
                {
                    throw new AssemblerException("Unknown function: " + fix.function);
                }
                codepoint = fix.field.Clear(codepoint);
                codepoint |= fix.field.Encode(x);
                code[fix.code_index] = codepoint;
            }

            foreach (JumpFixup fix in j_fixup)
            {
                int codepoint = code[fix.code_index];
                int x = 0;
                bool found = false;
                foreach (AssemblerLabel label in labels)
                {
                    if (fix.label == label.name)
                    {
                        x = label.code_index - fix.code_index - 1;
                        if (fix.negate) x = -x;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    throw new AssemblerException("Unknown label: " + fix.label);
                }
                codepoint = fix.field.Clear(codepoint);
                codepoint |= fix.field.Encode(x);
                code[fix.code_index] = codepoint;
            }

            foreach (AssemblerFunction f in children)
            {
                f.fixup(extract);
            }
        }

    }

    class AssemblerChunk
    {

        public Version version;

        public int format;

        public LHeader.LEndianness endianness;

        public int int_size;
        public BIntegerType intT;

        public int size_t_size;
        public BIntegerType sizeT;

        public int instruction_size;
        public int op_size;
        public int a_size;
        public int b_size;
        public int c_size;

        public Dictionary<int, Op> useropmap;

        public bool number_integral;
        public int number_size;
        public LNumberType number;

        public LNumberType lint;

        public LNumberType lfloat;

        public AssemblerFunction main;
        public AssemblerFunction current;
        public CodeExtract extract;

        public readonly HashSet<Directive> processed_directives;

        public AssemblerChunk(Version version)
        {
            this.version = version;
            processed_directives = new HashSet<Directive>();

            main = null;
            current = null;
            extract = null;
        }

        public void processHeaderDirective(Assembler a, Directive d)
        {
            if (d != Directive.OP && processed_directives.Contains(d))
            {
                throw new AssemblerException("Duplicate " + d.Token + " directive");
            }
            processed_directives.Add(d);
            switch (d.value)
            {
                case DirectiveT.FORMAT:
                    format = a.getInteger();
                    break;
                case DirectiveT.ENDIANNESS:
                    {
                        string endiannessName = a.getName();
                        switch (endiannessName)
                        {
                            case "LITTLE":
                                endianness = LHeader.LEndianness.LITTLE;
                                break;
                            case "BIG":
                                endianness = LHeader.LEndianness.BIG;
                                break;
                            default:
                                throw new AssemblerException("Unknown endianness \"" + endiannessName + "\"");
                        }
                        break;
                    }
                case DirectiveT.INT_SIZE:
                    int_size = a.getInteger();
                    intT = BIntegerType.create50Type(int_size);
                    break;
                case DirectiveT.SIZE_T_SIZE:
                    size_t_size = a.getInteger();
                    sizeT = BIntegerType.create50Type(size_t_size);
                    break;
                case DirectiveT.INSTRUCTION_SIZE:
                    instruction_size = a.getInteger();
                    break;
                case DirectiveT.SIZE_OP:
                    op_size = a.getInteger();
                    break;
                case DirectiveT.SIZE_A:
                    a_size = a.getInteger();
                    break;
                case DirectiveT.SIZE_B:
                    b_size = a.getInteger();
                    break;
                case DirectiveT.SIZE_C:
                    c_size = a.getInteger();
                    break;
                case DirectiveT.NUMBER_FORMAT:
                    {
                        string numberTypeName = a.getName();
                        switch (numberTypeName)
                        {
                            case "int": number_integral = true; break;
                            case "float": number_integral = false; break;
                            default: throw new AssemblerException("Unknown number_format \"" + numberTypeName + "\"");
                        }
                        number_size = a.getInteger();
                        number = new LNumberType(number_size, number_integral, LNumberType.NumberMode.MODE_NUMBER);
                        break;
                    }
                case DirectiveT.INT_FORMAT:
                    lint = new LNumberType(a.getInteger(), true, LNumberType.NumberMode.MODE_int);
                    break;
                case DirectiveT.FLOAT_FORMAT:
                    lfloat = new LNumberType(a.getInteger(), false, LNumberType.NumberMode.MODE_FLOAT);
                    break;
                case DirectiveT.OP:
                    {
                        if (useropmap == null)
                        {
                            useropmap = new Dictionary<int, Op>();
                        }
                        int opcode = a.getInteger();
                        string name = a.getName();
                        Op op = version.LOpCodeMap.GetOpByName(name);
                        if (op == null)
                        {
                            throw new AssemblerException("Unknown op name \"" + name + "\"");
                        }
                        useropmap.Add(opcode, op);
                        break;
                    }
                default:
                    throw new System.InvalidOperationException("Unhandled directive: " + d);
            }
        }

        public CodeExtract getCodeExtract()
        {
            if (extract == null)
            {
                extract = new CodeExtract(version, op_size, a_size, b_size, c_size);
            }
            return extract;
        }

        public void processNewFunction(Assembler a)
        {
            string name = a.getName();
            string[] parts = name.Split('/');
            if (main == null)
            {
                if (parts.Length != 1) throw new AssemblerException("First (main) function declaration must not have a \"/\" in the name");
                main = new AssemblerFunction(this, null, name);
                current = main;
            }
            else
            {
                if (parts.Length == 1 || parts[0] != main.name) throw new AssemblerException("Function \"" + name + "\" isn't contained in the main function");
                AssemblerFunction parent = main.getInnerParent(parts, 1);
                current = parent.addChild(parts[parts.Length - 1]);
            }
        }

        public void processFunctionDirective(Assembler a, Directive d)
        {
            if (current == null)
            {
                throw new AssemblerException("Misplaced function directive before declaration of any function");
            }
            current.processFunctionDirective(a, d);
        }

        public void processOp(Assembler a, Op op, int opcode)
        {
            if (current == null)
            {
                throw new AssemblerException("Misplaced code before declaration of any function");
            }
            current.processOp(a, getCodeExtract(), op, opcode);
        }

        public void fixup()
        {
            main.fixup(getCodeExtract());
        }

        public void write(BinaryWriter output)
        {
            LBooleanType boolType = new LBooleanType();
            LStringType stringType = version.LStringType;
            LConstantType constantType = version.LConstantType;
            LAbsLineInfoType abslineinfo = new LAbsLineInfoType();
            LLocalType local = new LLocalType();
            LUpvalueType upvalue = version.LUpvalueType;
            LFunctionType function = version.LFunctionType;
            CodeExtract extract = getCodeExtract();

            if (intT == null)
            {
                intT = BIntegerType.create54();
                sizeT = intT;
            }

            LHeader lheader = new LHeader(format, endianness, intT, sizeT, boolType, number, lint, lfloat, stringType, constantType, abslineinfo, local, upvalue, function, extract);
            BHeader header = new BHeader(version, lheader);
            LFunction main = convert_function(header, this.main);
            header = new BHeader(version, lheader, main);

            header.write(output);
        }

        private LFunction convert_function(BHeader header, AssemblerFunction function)
        {
            int i;
            int[] code = new int[function.code.Count];
            i = 0;
            foreach (int codepoint in function.code)
            {
                code[i++] = codepoint;
            }
            int[] lines = new int[function.lines.Count];
            i = 0;
            foreach (int line in function.lines)
            {
                lines[i++] = line;
            }
            LAbsLineInfo[] abslineinfo = new LAbsLineInfo[function.abslineinfo.Count];
            i = 0;
            foreach (AssemblerAbsLineInfo info in function.abslineinfo)
            {
                abslineinfo[i++] = new LAbsLineInfo(info.pc, info.line);
            }
            LLocal[] locals = new LLocal[function.locals.Count];
            i = 0;
            foreach (AssemblerLocal local in function.locals)
            {
                locals[i++] = new LLocal(convert_string(header, local.name), new BInteger(local.begin), new BInteger(local.end));
            }
            LObject[] constants = new LObject[function.constants.Count];
            i = 0;
            foreach (AssemblerConstant constant in function.constants)
            {
                LObject o;
                switch (constant.type)
                {
                    case AssemblerConstant.Type.NIL:
                        o = LNil.NIL;
                        break;
                    case AssemblerConstant.Type.BOOL:
                        o = constant.boolValue ? LBoolean.LTRUE : LBoolean.LFALSE;
                        break;
                    case AssemblerConstant.Type.NUMBER:
                        o = header.numberType.create(constant.numberValue);
                        break;
                    case AssemblerConstant.Type.INT:
                        o = header.longType.create(constant.intValue);
                        break;
                    case AssemblerConstant.Type.FLOAT:
                        o = header.doubleType.create(constant.numberValue);
                        break;
                    case AssemblerConstant.Type.STRING:
                        o = convert_string(header, constant.stringValue);
                        break;
                    case AssemblerConstant.Type.LONGSTRING:
                        o = convert_long_string(header, constant.stringValue);
                        break;
                    default:
                        throw new System.InvalidOperationException();
                }
                constants[i++] = o;
            }
            LUpvalue[] upvalues = new LUpvalue[function.upvalues.Count];
            i = 0;
            foreach (AssemblerUpvalue upvalue in function.upvalues)
            {
                LUpvalue lup = new LUpvalue();
                lup.bname = convert_string(header, upvalue.name);
                lup.idx = upvalue.index;
                lup.instack = upvalue.instack;
                upvalues[i++] = lup;
            }
            LFunction[] functions = new LFunction[function.children.Count];
            i = 0;
            foreach (AssemblerFunction f in function.children)
            {
                functions[i++] = convert_function(header, f);
            }
            return new LFunction(
              header,
              convert_string(header, function.source),
              function.linedefined,
              function.lastlinedefined,
              code,
              lines,
              abslineinfo,
              locals,
              constants,
              upvalues,
              functions,
              function.maxStackSize,
              function.upvalues.Count,
              function.numParams,
              function.vararg
           );
        }

        private LString convert_string(BHeader header, string str)
        {
            if (str == null)
            {
                return LString.EmptyString;
            }
            else
            {
                return new LString(str);
            }
        }

        private LString convert_long_string(BHeader header, string str)
        {
            return new LString(str, true);
        }

    }
    public class Assembler
    {

        private Tokenizer t;
        private BinaryWriter output;
        private Version version;

        public Assembler(StreamReader input, BinaryWriter output)
        {
            t = new Tokenizer(input);
            this.output = output;
        }

        public void assemble()
        {
            string tok = t.next();
            if (tok != ".version") throw new AssemblerException("First directive must be .version, instead was \"" + tok + "\"");
            tok = t.next();
            int major;
            int minor;
            string[] parts = tok.Split('.');
            if (parts.Length == 2)
            {
                try
                {
                    major = int.Parse(parts[0]);
                    minor = int.Parse(parts[1]);
                }
                catch (FormatException)
                {
                    throw new AssemblerException("Unsupported version " + tok);
                }
            }
            else
            {
                throw new AssemblerException("Unsupported version " + tok);
            }
            if (major < 0 || major > 0xF || minor < 0 || minor > 0xF)
            {
                throw new AssemblerException("Unsupported version " + tok);
            }

            version = Version.GetVersion(major, minor);

            if (version == null)
            {
                throw new AssemblerException("Unsupported version " + tok);
            }

            Dictionary<string, Op> oplookup = null;
            Dictionary<Op, int> opcodelookup = null;

            AssemblerChunk chunk = new AssemblerChunk(version);
            bool opinit = false;

            while ((tok = t.next()) != null)
            {
                Directive d = Directive.lookup[tok];
                if (d != null)
                {
                    switch (d.type)
                    {
                        case DirectiveType.HEADER:
                            chunk.processHeaderDirective(this, d);
                            break;
                        case DirectiveType.NEWFUNCTION:
                            if (!opinit)
                            {
                                opinit = true;
                                OpCodeMap opmap;
                                if (chunk.useropmap != null)
                                {
                                    opmap = new OpCodeMap(chunk.useropmap);
                                }
                                else
                                {
                                    opmap = version.LOpCodeMap;
                                }
                                oplookup = new Dictionary<string, Op>();
                                opcodelookup = new Dictionary<Op, int>();
                                for (int i = 0; i < opmap.Length; i++)
                                {
                                    Op op = opmap.GetOp(i);
                                    if (op != null)
                                    {
                                        oplookup.Add(op.Name, op);
                                        opcodelookup.Add(op, i);
                                    }
                                }

                                oplookup.Add(Op.EXTRABYTE.Name, Op.EXTRABYTE);
                                opcodelookup.Add(Op.EXTRABYTE, -1);
                            }

                            chunk.processNewFunction(this);
                            break;
                        case DirectiveType.FUNCTION:
                            chunk.processFunctionDirective(this, d);
                            break;
                        default:
                            throw new System.InvalidOperationException();
                    }

                }
                else
                {
                    Op op = oplookup[tok];
                    if (op != null)
                    {
                        // TODO:
                        chunk.processOp(this, op, opcodelookup[op]);
                    }
                    else
                    {
                        throw new AssemblerException("Unexpected token \"" + tok + "\"");
                    }
                }

            }

            chunk.fixup();

            chunk.write(output);

        }

        public string getAny()
        {
            string s = t.next();
            if (s == null) throw new AssemblerException("Unexcepted end of file");
            return s;
        }

        public string getName()
        {
            string s = t.next();
            if (s == null) throw new AssemblerException("Unexcepted end of file");
            return s;
        }

        public string getstring()
        {
            string s = t.next();
            if (s == null) throw new AssemblerException("Unexcepted end of file");
            return StringUtils.fromPrintString(s);
        }

        public int getInteger()
        {
            string s = t.next();
            if (s == null) throw new AssemblerException("Unexcepted end of file");
            int i;
            try
            {
                i = int.Parse(s);
            }
            catch (FormatException)
            {
                throw new AssemblerException("Excepted number, got \"" + s + "\"");
            }
            return i;
        }

        public bool getBoolean()
        {
            string s = t.next();
            if (s == null) throw new AssemblerException("Unexcepted end of file");
            bool b;
            if (s == "true")
            {
                b = true;
            }
            else if (s == "false")
            {
                b = false;
            }
            else
            {
                throw new AssemblerException("Expected bool, got \"" + s + "\"");
            }
            return b;
        }

        public int getRegister()
        {
            string s = t.next();
            if (s == null) throw new AssemblerException("Unexcepted end of file");
            int r;
            if (s.Length >= 2 && s[0] == 'r')
            {
                try
                {
                    r = int.Parse(s.Substring(1));
                }
                catch (FormatException)
                {
                    throw new AssemblerException("Excepted register, got \"" + s + "\"");
                }
            }
            else
            {
                throw new AssemblerException("Excepted register, got \"" + s + "\"");
            }
            return r;
        }

        public class RKInfo
        {
            public int x;
            public bool constant;
        }

        public RKInfo getRegisterK54()
        {
            string s = t.next();
            if (s == null) throw new AssemblerException("Unexcepted end of file");
            RKInfo rk = new RKInfo();
            if (s.Length >= 2 && s[0] == 'r')
            {
                rk.constant = false;
                try
                {
                    rk.x = int.Parse(s.Substring(1));
                }
                catch (FormatException)
                {
                    throw new AssemblerException("Excepted register, got \"" + s + "\"");
                }
            }
            else if (s.Length >= 2 && s[0] == 'k')
            {
                rk.constant = true;
                try
                {
                    rk.x = int.Parse(s.Substring(1));
                }
                catch (FormatException)
                {
                    throw new AssemblerException("Excepted constant, got \"" + s + "\"");
                }
            }
            else
            {
                throw new AssemblerException("Excepted register or constant, got \"" + s + "\"");
            }
            return rk;
        }

        public int getConstant()
        {
            string s = t.next();
            if (s == null) throw new AssemblerException("Unexpected end of file");
            int k;
            if (s.Length >= 2 && s[0] == 'k')
            {
                try
                {
                    k = int.Parse(s.Substring(1));
                }
                catch (FormatException)
                {
                    throw new AssemblerException("Excepted constant, got \"" + s + "\"");
                }
            }
            else
            {
                throw new AssemblerException("Excepted constant, got \"" + s + "\"");
            }
            return k;
        }

        public int getUpvalue()
        {
            string s = t.next();
            if (s == null) throw new AssemblerException("Unexcepted end of file");
            int u;
            if (s.Length >= 2 && s[0] == 'u')
            {
                try
                {
                    u = int.Parse(s.Substring(1));
                }
                catch (FormatException)
                {
                    throw new AssemblerException("Excepted register, got \"" + s + "\"");
                }
            }
            else
            {
                throw new AssemblerException("Excepted register, got \"" + s + "\"");
            }
            return u;
        }

    }
}
