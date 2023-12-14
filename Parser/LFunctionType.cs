using LuaDec.Assemble;
using System;
using System.Collections.Generic;
using System.IO;

namespace LuaDec.Parser
{
    internal class LFunctionType50 : LFunctionType
    {
        protected override void ParseMain(BinaryReader buffer, BHeader header, LFunctionParseState s)
        {
            s.name = header.stringType.Parse(buffer, header);
            s.lineBegin = header.integerType.Parse(buffer, header).AsInt();
            s.lineEnd = 0;
            s.lenUpvalues = 0xFF & buffer.ReadByte();
            CreateUpvalues(buffer, header, s);
            s.lenParameter = 0xFF & buffer.ReadByte();
            s.vararg = 0xFF & buffer.ReadByte();
            s.maximumStackSize = 0xFF & buffer.ReadByte();
            ParseDebug(buffer, header, s);
            ParseConstants(buffer, header, s);
            ParseCode(buffer, header, s);
        }

        public override List<Directive> GetDirectives()
        {
            return new List<Directive> {
                Directive.SOURCE,
                Directive.LINEDEFINED,
                Directive.NUMPARAMS,
                Directive.IS_VARARG,
                Directive.MAXSTACKSIZE,
            };
        }

        public override void Write(BinaryWriter output, BHeader header, LFunction o)
        {
            header.stringType.Write(output, header, o.name);
            header.integerType.Write(output, header, new BInteger(o.lineDefined));
            output.Write((byte)o.numUpvalues);
            output.Write((byte)o.numParams);
            output.Write((byte)o.varArg);
            output.Write((byte)o.maximumStackSize);
            WriteDebug(output, header, o);
            WriteConstants(output, header, o);
            WriteCode(output, header, o);
        }
    }

    internal class LFunctionType51 : LFunctionType
    {
        protected override void ParseMain(BinaryReader buffer, BHeader header, LFunctionParseState s)
        {
            s.name = header.stringType.Parse(buffer, header);
            s.lineBegin = header.integerType.Parse(buffer, header).AsInt();
            s.lineEnd = header.integerType.Parse(buffer, header).AsInt();
            s.lenUpvalues = 0xFF & buffer.ReadByte();
            CreateUpvalues(buffer, header, s);
            s.lenParameter = 0xFF & buffer.ReadByte();
            s.vararg = 0xFF & buffer.ReadByte();
            s.maximumStackSize = 0xFF & buffer.ReadByte();
            ParseCode(buffer, header, s);
            ParseConstants(buffer, header, s);
            ParseDebug(buffer, header, s);
        }

        public override List<Directive> GetDirectives()
        {
            return new List<Directive> {
                Directive.SOURCE,
                Directive.LINEDEFINED,
                Directive.LASTLINEDEFINED,
                Directive.NUMPARAMS,
                Directive.IS_VARARG,
                Directive.MAXSTACKSIZE,
            };
        }

        public override void Write(BinaryWriter output, BHeader header, LFunction o)
        {
            header.stringType.Write(output, header, o.name);
            header.integerType.Write(output, header, new BInteger(o.lineDefined));
            header.integerType.Write(output, header, new BInteger(o.lastLineDefined));
            output.Write((byte)o.numUpvalues);
            output.Write((byte)o.numParams);
            output.Write((byte)o.varArg);
            output.Write((byte)o.maximumStackSize);
            WriteCode(output, header, o);
            WriteConstants(output, header, o);
            WriteDebug(output, header, o);
        }
    }

    internal class LFunctionType52 : LFunctionType
    {
        protected override void ParseMain(BinaryReader buffer, BHeader header, LFunctionParseState s)
        {
            s.lineBegin = header.integerType.Parse(buffer, header).AsInt();
            s.lineEnd = header.integerType.Parse(buffer, header).AsInt();
            s.lenParameter = 0xFF & buffer.ReadByte();
            s.vararg = 0xFF & buffer.ReadByte();
            s.maximumStackSize = 0xFF & buffer.ReadByte();
            ParseCode(buffer, header, s);
            ParseConstants(buffer, header, s);
            ParseUpvalues(buffer, header, s);
            s.name = header.stringType.Parse(buffer, header);
            ParseDebug(buffer, header, s);
        }

        public override List<Directive> GetDirectives()
        {
            return new List<Directive> {
                Directive.LINEDEFINED,
                Directive.LASTLINEDEFINED,
                Directive.NUMPARAMS,
                Directive.IS_VARARG,
                Directive.MAXSTACKSIZE,
                Directive.SOURCE,
            };
        }

        public override void Write(BinaryWriter output, BHeader header, LFunction o)
        {
            header.integerType.Write(output, header, new BInteger(o.lineDefined));
            header.integerType.Write(output, header, new BInteger(o.lastLineDefined));
            output.Write((byte)o.numParams);
            output.Write((byte)o.varArg);
            output.Write((byte)o.maximumStackSize);
            WriteCode(output, header, o);
            WriteConstants(output, header, o);
            WriteUpvalues(output, header, o);
            header.stringType.Write(output, header, o.name);
            WriteDebug(output, header, o);
        }
    }

    internal class LFunctionType53 : LFunctionType
    {
        protected override void ParseMain(BinaryReader buffer, BHeader header, LFunctionParseState s)
        {
            s.name = header.stringType.Parse(buffer, header); //TODO: psource
            s.lineBegin = header.integerType.Parse(buffer, header).AsInt();
            s.lineEnd = header.integerType.Parse(buffer, header).AsInt();
            s.lenParameter = 0xFF & buffer.ReadByte();
            s.vararg = 0xFF & buffer.ReadByte();
            s.maximumStackSize = 0xFF & buffer.ReadByte();
            ParseCode(buffer, header, s);
            s.constants = header.constantType.ParseList(buffer, header);
            ParseUpvalues(buffer, header, s);
            s.functions = header.functionType.ParseList(buffer, header);
            ParseDebug(buffer, header, s);
        }

        public override List<Directive> GetDirectives()
        {
            return new List<Directive> {
                Directive.SOURCE,
                Directive.LINEDEFINED,
                Directive.LASTLINEDEFINED,
                Directive.NUMPARAMS,
                Directive.IS_VARARG,
                Directive.MAXSTACKSIZE,
            };
        }

        public override void Write(BinaryWriter output, BHeader header, LFunction o)
        {
            header.stringType.Write(output, header, o.name);
            header.integerType.Write(output, header, new BInteger(o.lineDefined));
            header.integerType.Write(output, header, new BInteger(o.lastLineDefined));
            output.Write((byte)o.numParams);
            output.Write((byte)o.varArg);
            output.Write((byte)o.maximumStackSize);
            WriteCode(output, header, o);
            header.constantType.WriteList(output, header, o.constants);
            WriteUpvalues(output, header, o);
            header.functionType.WriteList(output, header, o.functions);
            WriteDebug(output, header, o);
        }
    }

    internal class LFunctionType54 : LFunctionType
    {
        protected override void ParseDebug(BinaryReader buffer, BHeader header, LFunctionParseState s)
        {
            // TODO: process line info correctly
            s.lines = (new BIntegerType50(1)).ParseList(buffer, header);
            s.abslineinfo = header.absLineInfo.ParseList(buffer, header);
            s.locals = header.localType.ParseList(buffer, header);
            BList<LString> upvalueNames = header.stringType.ParseList(buffer, header);
            for (int i = 0; i < upvalueNames.length.AsInt(); i++)
            {
                s.upvalues[i].bname = upvalueNames.Get(i);
                s.upvalues[i].name = s.upvalues[i].bname.Deref();
            }
        }

        protected override void ParseMain(BinaryReader buffer, BHeader header, LFunctionParseState s)
        {
            s.name = header.stringType.Parse(buffer, header);
            s.lineBegin = header.integerType.Parse(buffer, header).AsInt();
            s.lineEnd = header.integerType.Parse(buffer, header).AsInt();
            s.lenParameter = 0xFF & buffer.ReadByte();
            s.vararg = 0xFF & buffer.ReadByte();
            s.maximumStackSize = 0xFF & buffer.ReadByte();
            ParseCode(buffer, header, s);
            s.constants = header.constantType.ParseList(buffer, header);
            ParseUpvalues(buffer, header, s);
            s.functions = header.functionType.ParseList(buffer, header);
            ParseDebug(buffer, header, s);
        }

        protected override void WriteDebug(BinaryWriter output, BHeader header, LFunction o)
        {
            header.integerType.Write(output, header, new BInteger(o.lines.Length));
            for (int i = 0; i < o.lines.Length; i++)
            {
                output.Write((byte)o.lines[i]);
            }
            header.absLineInfo.WriteList(output, header, o.absLineInfo);
            header.localType.WriteList(output, header, o.locals);
            int upvalueNameLength = 0;
            foreach (LUpvalue upvalue in o.upvalues)
            {
                if (upvalue.bname != null && upvalue.bname != LString.EmptyString)
                {
                    upvalueNameLength++;
                }
                else
                {
                    break;
                }
            }
            header.integerType.Write(output, header, new BInteger(upvalueNameLength));
            for (int i = 0; i < upvalueNameLength; i++)
            {
                header.stringType.Write(output, header, o.upvalues[i].bname);
            }
        }

        public override List<Directive> GetDirectives()
        {
            return new List<Directive> {
                Directive.SOURCE,
                Directive.LINEDEFINED,
                Directive.LASTLINEDEFINED,
                Directive.NUMPARAMS,
                Directive.IS_VARARG,
                Directive.MAXSTACKSIZE,
            };
        }

        public override void Write(BinaryWriter output, BHeader header, LFunction o)
        {
            header.stringType.Write(output, header, o.name);
            header.integerType.Write(output, header, new BInteger(o.lineDefined));
            header.integerType.Write(output, header, new BInteger(o.lastLineDefined));
            output.Write((byte)o.numParams);
            output.Write((byte)o.varArg);
            output.Write((byte)o.maximumStackSize);
            WriteCode(output, header, o);
            header.constantType.WriteList(output, header, o.constants);
            WriteUpvalues(output, header, o);
            header.functionType.WriteList(output, header, o.functions);
            WriteDebug(output, header, o);
        }
    }

    public class LFunctionType : BObjectType<LFunction>
    {
        protected class LFunctionParseState
        {
            public BList<LAbsLineInfo> abslineinfo;
            public int[] code;
            public BList<LObject> constants;
            public BList<LFunction> functions;
            public int length;
            public int lenParameter;
            public int lenUpvalues;
            public int lineBegin;
            public int lineEnd;
            public BList<BInteger> lines;
            public BList<LLocal> locals;
            public int maximumStackSize;
            public LString name;
            public LUpvalue[] upvalues;
            public int vararg;
        }

        protected void CreateUpvalues(BinaryReader buffer, BHeader header, LFunctionParseState s)
        {
            s.upvalues = new LUpvalue[s.lenUpvalues];
            for (int i = 0; i < s.lenUpvalues; i++)
            {
                s.upvalues[i] = new LUpvalue();
            }
        }

        protected void ParseCode(BinaryReader buffer, BHeader header, LFunctionParseState s)
        {
            if (header.debug)
            {
                Console.WriteLine("-- beginning to parse bytecode list");
            }
            s.length = header.integerType.Parse(buffer, header).AsInt();
            s.code = new int[s.length];
            for (int i = 0; i < s.length; i++)
            {
                s.code[i] = buffer.ReadInt32();
                if (header.debug)
                {
                    Console.WriteLine("-- parsed codepoint " + s.code[i].ToString("X"));
                }
            }
        }

        protected void ParseConstants(BinaryReader buffer, BHeader header, LFunctionParseState s)
        {
            if (header.debug)
            {
                Console.WriteLine("-- beginning to parse constants list");
            }
            s.constants = header.constantType.ParseList(buffer, header);
            if (header.debug)
            {
                Console.WriteLine("-- beginning to parse functions list");
            }
            s.functions = header.functionType.ParseList(buffer, header);
        }

        protected virtual void ParseDebug(BinaryReader buffer, BHeader header, LFunctionParseState s)
        {
            if (header.debug)
            {
                Console.WriteLine("-- beginning to parse source lines list");
            }
            s.lines = header.integerType.ParseList(buffer, header);
            if (header.debug)
            {
                Console.WriteLine("-- beginning to parse locals list");
            }
            s.locals = header.localType.ParseList(buffer, header);
            if (header.debug)
            {
                Console.WriteLine("-- beginning to parse upvalues list");
            }
            BList<LString> upvalueNames = header.stringType.ParseList(buffer, header);
            for (int i = 0; i < upvalueNames.length.AsInt(); i++)
            {
                s.upvalues[i].bname = upvalueNames.Get(i);
                s.upvalues[i].name = s.upvalues[i].bname.Deref();
            }
        }

        protected virtual void ParseMain(BinaryReader buffer, BHeader header, LFunctionParseState s)
        {
            throw new NotImplementedException();
        }

        protected void ParseUpvalues(BinaryReader buffer, BHeader header, LFunctionParseState s)
        {
            BList<LUpvalue> upvalues = header.upvalueType.ParseList(buffer, header);
            s.lenUpvalues = upvalues.length.AsInt();
            s.upvalues = upvalues.AsArray(new LUpvalue[s.lenUpvalues]);
        }

        protected void WriteCode(BinaryWriter output, BHeader header, LFunction o)
        {
            header.integerType.Write(output, header, new BInteger(o.code.Length));
            for (int i = 0; i < o.code.Length; i++)
            {
                int codepoint = o.code[i];
                if (header.lheader.endianness == LHeader.LEndianness.Little)
                {
                    output.Write((byte)(0xFF & (codepoint)));
                    output.Write((byte)(0xFF & (codepoint >> 8)));
                    output.Write((byte)(0xFF & (codepoint >> 16)));
                    output.Write((byte)(0xFF & (codepoint >> 24)));
                }
                else
                {
                    output.Write((byte)(0xFF & (codepoint >> 24)));
                    output.Write((byte)(0xFF & (codepoint >> 16)));
                    output.Write((byte)(0xFF & (codepoint >> 8)));
                    output.Write((byte)(0xFF & (codepoint)));
                }
            }
        }

        protected void WriteConstants(BinaryWriter output, BHeader header, LFunction o)
        {
            header.constantType.WriteList(output, header, o.constants);
            header.functionType.WriteList(output, header, o.functions);
        }

        protected virtual void WriteDebug(BinaryWriter output, BHeader header, LFunction o)
        {
            header.integerType.Write(output, header, new BInteger(o.lines.Length));
            for (int i = 0; i < o.lines.Length; i++)
            {
                header.integerType.Write(output, header, new BInteger(o.lines[i]));
            }
            header.localType.WriteList(output, header, o.locals);
            int upvalueNameLength = 0;
            foreach (LUpvalue upvalue in o.upvalues)
            {
                if (upvalue.bname != null && upvalue.bname != LString.EmptyString)
                {
                    upvalueNameLength++;
                }
                else
                {
                    break;
                }
            }
            header.integerType.Write(output, header, new BInteger(upvalueNameLength));
            for (int i = 0; i < upvalueNameLength; i++)
            {
                header.stringType.Write(output, header, o.upvalues[i].bname);
            }
        }

        protected void WriteUpvalues(BinaryWriter output, BHeader header, LFunction o)
        {
            header.upvalueType.WriteList(output, header, o.upvalues);
        }

        public static LFunctionType Get(Version.FunctionType type)
        {
            switch (type)
            {
                case Version.FunctionType.Lua50: return new LFunctionType50();
                case Version.FunctionType.Lua51: return new LFunctionType51();
                case Version.FunctionType.Lua52: return new LFunctionType52();
                case Version.FunctionType.Lua53: return new LFunctionType53();
                case Version.FunctionType.Lua54: return new LFunctionType54();
                default: throw new System.InvalidOperationException();
            }
        }

        public virtual List<Directive> GetDirectives()
        {
            throw new NotImplementedException();
        }

        public override LFunction Parse(BinaryReader buffer, BHeader header)
        {
            if (header.debug)
            {
                Console.WriteLine("-- beginning to parse function");
            }
            if (header.debug)
            {
                Console.WriteLine("-- parsing name...start...end...upvalues...params...varargs...stack");
            }
            LFunctionParseState s = new LFunctionParseState();
            ParseMain(buffer, header, s);
            int[] lines = new int[s.lines.length.AsInt()];
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = s.lines.Get(i).AsInt();
            }
            LAbsLineInfo[] abslineinfo = null;
            if (s.abslineinfo != null)
            {
                abslineinfo = s.abslineinfo.AsArray(new LAbsLineInfo[s.abslineinfo.length.AsInt()]);
            }
            LFunction lfunc = new LFunction(header, s.name, s.lineBegin, s.lineEnd, s.code, lines, abslineinfo, s.locals.AsArray(new LLocal[s.locals.length.AsInt()]), s.constants.AsArray(new LObject[s.constants.length.AsInt()]), s.upvalues, s.functions.AsArray(new LFunction[s.functions.length.AsInt()]), s.maximumStackSize, s.lenUpvalues, s.lenParameter, s.vararg);
            foreach (LFunction child in lfunc.functions)
            {
                child.parent = lfunc;
            }
            if (s.lines.length.AsInt() == 0 && s.locals.length.AsInt() == 0)
            {
                lfunc.stripped = true;
            }
            return lfunc;
        }

        public override void Write(BinaryWriter output, BHeader header, LFunction o)
        {
            throw new NotImplementedException();
        }
    }
}