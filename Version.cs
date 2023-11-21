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

        public enum OpcodeMapType
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
            private readonly T _value;

            public T Value => _value;

            public Setting(T value)
            {
                _value = value;
            }
        }

        private readonly Op _defaultOp;

        private readonly LConstantType _lconstantType;

        private readonly LFunctionType _lfunctionType;

        private readonly LHeaderType _lheaderType;

        private readonly LStringType _lstringType;

        private readonly LUpvalueType _lupvalueType;

        private readonly int _major;

        private readonly int _minor;

        private readonly string _name;

        private readonly OpCodeMap _opcodeMap;

        private readonly HashSet<string> _reservedWords;

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

        public Op DefaultOp => _defaultOp;

        public LConstantType LConstantType => _lconstantType;

        public LFunctionType LFunctionType => _lfunctionType;

        public LHeaderType LHeaderType => _lheaderType;

        public LStringType LstringType => _lstringType;

        public LUpvalueType LUpvalueType => _lupvalueType;

        public string Name => _name;

        public OpCodeMap OpcodeMap => _opcodeMap;

        public int VersionMajor => _major;

        public int VersionMinor => _minor;

        private Version(int major, int minor)
        {
            HeaderType headerType;
            StringType stringType;
            ConstantType constantType;
            UpvalueType upvalueType;
            FunctionType functionType;
            OpcodeMapType opcodeMap;
            _major = major;
            _minor = minor;
            _name = major + "." + minor;
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
                        opcodeMap = OpcodeMapType.Lua50;
                        _defaultOp = Op.DEFAULT;
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
                        opcodeMap = OpcodeMapType.Lua51;
                        _defaultOp = Op.DEFAULT;
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
                        opcodeMap = OpcodeMapType.Lua52;
                        _defaultOp = Op.DEFAULT;
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
                        opcodeMap = OpcodeMapType.Lua53;
                        _defaultOp = Op.DEFAULT;
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
                        opcodeMap = OpcodeMapType.Lua54;
                        _defaultOp = Op.DEFAULT54;
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

            _reservedWords = new HashSet<string>();
            _reservedWords.Add("and");
            _reservedWords.Add("break");
            _reservedWords.Add("do");
            _reservedWords.Add("else");
            _reservedWords.Add("elseif");
            _reservedWords.Add("end");
            _reservedWords.Add("false");
            _reservedWords.Add("for");
            _reservedWords.Add("function");
            _reservedWords.Add("if");
            _reservedWords.Add("in");
            _reservedWords.Add("local");
            _reservedWords.Add("nil");
            _reservedWords.Add("not");
            _reservedWords.Add("or");
            _reservedWords.Add("repeat");
            _reservedWords.Add("return");
            _reservedWords.Add("then");
            _reservedWords.Add("true");
            _reservedWords.Add("until");
            _reservedWords.Add("while");
            if (useGoto.Value)
            {
                _reservedWords.Add("goto");
            }

            _lheaderType = LHeaderType.get(headerType);
            _lstringType = LStringType.get(stringType);
            _lconstantType = LConstantType.get(constantType);
            _lupvalueType = LUpvalueType.get(upvalueType);
            _lfunctionType = LFunctionType.get(functionType);
            _opcodeMap = new OpCodeMap(opcodeMap);
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
            return _reservedWords.Contains(name);
        }
    }
}