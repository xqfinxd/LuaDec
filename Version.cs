using LuaDec.Decompile;
using LuaDec.Parser;
using System.Collections.Generic;

namespace LuaDec
{
    public class Version
    {
        public enum CloseSemantics
        {
            Default,
            Jump,
            Lua54,
        }

        public enum ConstantType
        {
            Lua50,
            Lua53,
            Lua54,
        }

        public enum FunctionType
        {
            Lua50,
            Lua51,
            Lua52,
            Lua53,
            Lua54,
        }

        public enum HeaderType
        {
            Lua50,
            Lua51,
            Lua52,
            Lua53,
            Lua54,
        }

        public enum InstructionFormat
        {
            Lua50,
            Lua51,
            Lua54,
        }

        public enum OpCodeMapType
        {
            Lua50,
            Lua51,
            Lua52,
            Lua53,
            Lua54,
        }

        public enum StringType
        {
            Lua50,
            Lua53,
            Lua54,
        }

        public enum UpvalueDeclarationType
        {
            Inline,
            Header,
        }

        public enum UpvalueType
        {
            Lua50,
            Lua54
        }

        public enum VarArgType
        {
            Arg,
            Hybrid,
            Ellipsis,
        }

        public enum WhileFormat
        {
            TopCondition,
            BottomCondition,
        }

        public class Setting<T>
        {
            private readonly T value;

            public T Value => value;

            public Setting(T value)
            {
                this.value = value;
            }
        }

        private readonly Op defaultOp;
        private readonly LConstantType lconstantType;
        private readonly LFunctionType lfunctionType;
        private readonly LHeaderType lheaderType;
        private readonly OpCodeMap lopCodeMap;
        private readonly LStringType lstringType;
        private readonly LUpvalueType lupvalueType;
        private readonly int major;
        private readonly int minor;
        private readonly string name;
        private readonly HashSet<string> reservedWords;

        public readonly Setting<bool> allowPreceedingSemicolon;
        public readonly Setting<bool> closeInScope;
        public readonly Setting<CloseSemantics> closeSemantics;
        public readonly Setting<string> envTable;
        public readonly Setting<bool> extendedRepeatScope;
        public readonly Setting<Op> forTarget;
        public readonly Setting<InstructionFormat> instructionFormat;
        public readonly Setting<int> outerBlockScopeAdjustment;
        public readonly Setting<int> rkOffset;
        public readonly Setting<Op> tforTarget;
        public readonly Setting<UpvalueDeclarationType> upvalueDeclarationType;
        public readonly Setting<bool> useGoto;
        public readonly Setting<bool> useIfBreakRewrite;
        public readonly Setting<bool> useNestingLongStrings;
        public readonly Setting<bool> useUpvalueCountInHeader;
        public readonly Setting<VarArgType> varArgtTpe;
        public readonly Setting<WhileFormat> whileFormat;

        public Op DefaultOp => defaultOp;
        public LConstantType LConstantType => lconstantType;
        public LFunctionType LFunctionType => lfunctionType;
        public LHeaderType LHeaderType => lheaderType;
        public OpCodeMap LOpCodeMap => lopCodeMap;
        public LStringType LStringType => lstringType;
        public LUpvalueType LUpvalueType => lupvalueType;
        public string Name => name;
        public int VersionMajor => major;
        public int VersionMinor => minor;

        private Version(int major, int minor)
        {
            HeaderType headerType;
            StringType stringType;
            ConstantType constantType;
            UpvalueType upvalueType;
            FunctionType functionType;
            OpCodeMapType opcodeMap;
            this.major = major;
            this.minor = minor;
            name = major + "." + minor;
            if (major == 5 && minor >= 0 && minor <= 4)
            {
                switch (minor)
                {
                    case 0:
                        varArgtTpe = new Setting<VarArgType>(VarArgType.Arg);
                        useUpvalueCountInHeader = new Setting<bool>(false);
                        headerType = HeaderType.Lua50;
                        stringType = StringType.Lua50;
                        constantType = ConstantType.Lua50;
                        upvalueType = UpvalueType.Lua50;
                        functionType = FunctionType.Lua50;
                        opcodeMap = OpCodeMapType.Lua50;
                        defaultOp = Op.DEFAULT;
                        instructionFormat = new Setting<InstructionFormat>(InstructionFormat.Lua50);
                        outerBlockScopeAdjustment = new Setting<int>(-1);
                        extendedRepeatScope = new Setting<bool>(true);
                        closeInScope = new Setting<bool>(true);
                        closeSemantics = new Setting<CloseSemantics>(CloseSemantics.Default);
                        upvalueDeclarationType = new Setting<UpvalueDeclarationType>(UpvalueDeclarationType.Inline);
                        forTarget = new Setting<Op>(Op.FORLOOP);
                        tforTarget = new Setting<Op>(null);
                        whileFormat = new Setting<WhileFormat>(WhileFormat.BottomCondition);
                        allowPreceedingSemicolon = new Setting<bool>(false);
                        useNestingLongStrings = new Setting<bool>(true);
                        envTable = new Setting<string>(null);
                        useIfBreakRewrite = new Setting<bool>(false);
                        useGoto = new Setting<bool>(false);
                        rkOffset = new Setting<int>(250);
                        break;

                    case 1:
                        varArgtTpe = new Setting<VarArgType>(VarArgType.Hybrid);
                        useUpvalueCountInHeader = new Setting<bool>(false);
                        headerType = HeaderType.Lua51;
                        stringType = StringType.Lua50;
                        constantType = ConstantType.Lua50;
                        upvalueType = UpvalueType.Lua50;
                        functionType = FunctionType.Lua51;
                        opcodeMap = OpCodeMapType.Lua51;
                        defaultOp = Op.DEFAULT;
                        instructionFormat = new Setting<InstructionFormat>(InstructionFormat.Lua51);
                        outerBlockScopeAdjustment = new Setting<int>(-1);
                        extendedRepeatScope = new Setting<bool>(false);
                        closeInScope = new Setting<bool>(true);
                        closeSemantics = new Setting<CloseSemantics>(CloseSemantics.Default);
                        upvalueDeclarationType = new Setting<UpvalueDeclarationType>(UpvalueDeclarationType.Inline);
                        forTarget = new Setting<Op>(null);
                        tforTarget = new Setting<Op>(Op.TFORLOOP);
                        whileFormat = new Setting<WhileFormat>(WhileFormat.TopCondition);
                        allowPreceedingSemicolon = new Setting<bool>(false);
                        useNestingLongStrings = new Setting<bool>(false);
                        envTable = new Setting<string>(null);
                        useIfBreakRewrite = new Setting<bool>(false);
                        useGoto = new Setting<bool>(false);
                        rkOffset = new Setting<int>(256);
                        break;

                    case 2:
                        varArgtTpe = new Setting<VarArgType>(VarArgType.Ellipsis);
                        useUpvalueCountInHeader = new Setting<bool>(false);
                        headerType = HeaderType.Lua52;
                        stringType = StringType.Lua50;
                        constantType = ConstantType.Lua50;
                        upvalueType = UpvalueType.Lua50;
                        functionType = FunctionType.Lua52;
                        opcodeMap = OpCodeMapType.Lua52;
                        defaultOp = Op.DEFAULT;
                        instructionFormat = new Setting<InstructionFormat>(InstructionFormat.Lua51);
                        outerBlockScopeAdjustment = new Setting<int>(0);
                        extendedRepeatScope = new Setting<bool>(false);
                        closeInScope = new Setting<bool>(false);
                        closeSemantics = new Setting<CloseSemantics>(CloseSemantics.Jump);
                        upvalueDeclarationType = new Setting<UpvalueDeclarationType>(UpvalueDeclarationType.Header);
                        forTarget = new Setting<Op>(null);
                        tforTarget = new Setting<Op>(Op.TFORCALL);
                        whileFormat = new Setting<WhileFormat>(WhileFormat.TopCondition);
                        allowPreceedingSemicolon = new Setting<bool>(true);
                        useNestingLongStrings = new Setting<bool>(false);
                        envTable = new Setting<string>("_ENV");
                        useIfBreakRewrite = new Setting<bool>(true);
                        useGoto = new Setting<bool>(true);
                        rkOffset = new Setting<int>(256);
                        break;

                    case 3:
                        varArgtTpe = new Setting<VarArgType>(VarArgType.Ellipsis);
                        useUpvalueCountInHeader = new Setting<bool>(true);
                        headerType = HeaderType.Lua53;
                        stringType = StringType.Lua53;
                        constantType = ConstantType.Lua53;
                        upvalueType = UpvalueType.Lua50;
                        functionType = FunctionType.Lua53;
                        opcodeMap = OpCodeMapType.Lua53;
                        defaultOp = Op.DEFAULT;
                        instructionFormat = new Setting<InstructionFormat>(InstructionFormat.Lua51);
                        outerBlockScopeAdjustment = new Setting<int>(0);
                        extendedRepeatScope = new Setting<bool>(false);
                        closeInScope = new Setting<bool>(false);
                        closeSemantics = new Setting<CloseSemantics>(CloseSemantics.Jump);
                        upvalueDeclarationType = new Setting<UpvalueDeclarationType>(UpvalueDeclarationType.Header);
                        forTarget = new Setting<Op>(null);
                        tforTarget = new Setting<Op>(Op.TFORCALL);
                        whileFormat = new Setting<WhileFormat>(WhileFormat.TopCondition);
                        allowPreceedingSemicolon = new Setting<bool>(true);
                        useNestingLongStrings = new Setting<bool>(false);
                        envTable = new Setting<string>("_ENV");
                        useIfBreakRewrite = new Setting<bool>(true);
                        useGoto = new Setting<bool>(true);
                        rkOffset = new Setting<int>(256);
                        break;

                    case 4:
                        varArgtTpe = new Setting<VarArgType>(VarArgType.Ellipsis);
                        useUpvalueCountInHeader = new Setting<bool>(true);
                        headerType = HeaderType.Lua54;
                        stringType = StringType.Lua54;
                        constantType = ConstantType.Lua54;
                        upvalueType = UpvalueType.Lua54;
                        functionType = FunctionType.Lua54;
                        opcodeMap = OpCodeMapType.Lua54;
                        defaultOp = Op.DEFAULT54;
                        instructionFormat = new Setting<InstructionFormat>(InstructionFormat.Lua54);
                        outerBlockScopeAdjustment = new Setting<int>(0);
                        extendedRepeatScope = new Setting<bool>(false);
                        closeInScope = new Setting<bool>(false);
                        closeSemantics = new Setting<CloseSemantics>(CloseSemantics.Lua54);
                        upvalueDeclarationType = new Setting<UpvalueDeclarationType>(UpvalueDeclarationType.Header);
                        forTarget = new Setting<Op>(null);
                        tforTarget = new Setting<Op>(null);
                        whileFormat = new Setting<WhileFormat>(WhileFormat.TopCondition);
                        allowPreceedingSemicolon = new Setting<bool>(true);
                        useNestingLongStrings = new Setting<bool>(false);
                        envTable = new Setting<string>("_ENV");
                        useIfBreakRewrite = new Setting<bool>(true);
                        useGoto = new Setting<bool>(true);
                        rkOffset = new Setting<int>(-1);
                        break;

                    default:
                        throw new System.ArgumentException();
                }
            }
            else
            {
                throw new System.ArgumentException();
            }

            reservedWords = new HashSet<string>();
            reservedWords.Add("and");
            reservedWords.Add("break");
            reservedWords.Add("do");
            reservedWords.Add("else");
            reservedWords.Add("elseif");
            reservedWords.Add("end");
            reservedWords.Add("false");
            reservedWords.Add("for");
            reservedWords.Add("function");
            reservedWords.Add("if");
            reservedWords.Add("in");
            reservedWords.Add("local");
            reservedWords.Add("nil");
            reservedWords.Add("not");
            reservedWords.Add("or");
            reservedWords.Add("repeat");
            reservedWords.Add("return");
            reservedWords.Add("then");
            reservedWords.Add("true");
            reservedWords.Add("until");
            reservedWords.Add("while");
            if (useGoto.Value)
            {
                reservedWords.Add("goto");
            }

            lheaderType = LHeaderType.Get(headerType);
            lstringType = LStringType.Get(stringType);
            lconstantType = LConstantType.Get(constantType);
            lupvalueType = LUpvalueType.Get(upvalueType);
            lfunctionType = LFunctionType.Get(functionType);
            lopCodeMap = new OpCodeMap(opcodeMap);
        }

        public static Version GetVersion(int major, int minor)
        {
            return new Version(major, minor);
        }

        public bool IsEnvTable(string name)
        {
            string env = envTable.Value;
            if (env != null)
            {
                return name == env;
            }
            else
            {
                return false;
            }
        }

        public bool IsReserved(string name)
        {
            return reservedWords.Contains(name);
        }
    }
}