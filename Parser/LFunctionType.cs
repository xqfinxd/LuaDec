using LuaDec.Assemble;
using System;
using System.Collections.Generic;
using System.IO;

namespace LuaDec.Parser
{
    public class LFunctionType : BObjectType<LFunction>
    {

        public static LFunctionType get(Version.FunctionType type)
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

        protected class LFunctionParseState
        {

            public LString name;
            public int lineBegin;
            public int lineEnd;
            public int lenUpvalues;
            public int lenParameter;
            public int vararg;
            public int maximumStackSize;
            public int length;
            public int[] code;
            public BList<LObject> constants;
            public BList<LFunction> functions;
            public BList<BInteger> lines;
            public BList<LAbsLineInfo> abslineinfo;
            public BList<LLocal> locals;
            public LUpvalue[] upvalues;
        }

        public override LFunction parse(BinaryReader buffer, BHeader header)
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
            parse_main(buffer, header, s);
            int[] lines = new int[s.lines.blength.asInt()];
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = s.lines.get(i).asInt();
            }
            LAbsLineInfo[] abslineinfo = null;
            if (s.abslineinfo != null)
            {
                abslineinfo = s.abslineinfo.asArray(new LAbsLineInfo[s.abslineinfo.blength.asInt()]);
            }
            LFunction lfunc = new LFunction(header, s.name, s.lineBegin, s.lineEnd, s.code, lines, abslineinfo, s.locals.asArray(new LLocal[s.locals.blength.asInt()]), s.constants.asArray(new LObject[s.constants.blength.asInt()]), s.upvalues, s.functions.asArray(new LFunction[s.functions.blength.asInt()]), s.maximumStackSize, s.lenUpvalues, s.lenParameter, s.vararg);
            foreach (LFunction child in lfunc.functions)
            {
                child.parent = lfunc;
            }
            if (s.lines.blength.asInt() == 0 && s.locals.blength.asInt() == 0)
            {
                lfunc.stripped = true;
            }
            return lfunc;
        }

        public override void write(BinaryWriter output, BHeader header, LFunction o)
        {
            throw new NotImplementedException();
        }

        public virtual List<Directive> get_directives()
        {
            throw new NotImplementedException();
        }

        protected virtual void parse_main(BinaryReader buffer, BHeader header, LFunctionParseState s)
        {
            throw new NotImplementedException();
        }

        protected void parse_code(BinaryReader buffer, BHeader header, LFunctionParseState s)
        {
            if (header.debug)
            {
                Console.WriteLine("-- beginning to parse bytecode list");
            }
            s.length = header.integerType.parse(buffer, header).asInt();
            s.code = new int[s.length];
            for (int i = 0; i < s.length; i++)
            {
                byte[] intBytes = new byte[4];
                buffer.Read(intBytes, 0, 4);

                s.code[i] = BitConverter.ToInt32(intBytes, 0);
                if (header.debug)
                {
                    Console.WriteLine("-- parsed codepoint " + s.code[i].ToString("X"));
                }
            }
        }

        protected void write_code(BinaryWriter output, BHeader header, LFunction o)
        {
            header.integerType.write(output, header, new BInteger(o.code.Length));
            for (int i = 0; i < o.code.Length; i++)
            {
                int codepoint = o.code[i];
                if (header.lheader.endianness == LHeader.LEndianness.LITTLE)
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

        protected void parse_constants(BinaryReader buffer, BHeader header, LFunctionParseState s)
        {
            if (header.debug)
            {
                Console.WriteLine("-- beginning to parse constants list");
            }
            s.constants = header.constantType.parseList(buffer, header);
            if (header.debug)
            {
                Console.WriteLine("-- beginning to parse functions list");
            }
            s.functions = header.functionType.parseList(buffer, header);
        }

        protected void write_constants(BinaryWriter output, BHeader header, LFunction o)
        {
            header.constantType.writeList(output, header, o.constants);
            header.functionType.writeList(output, header, o.functions);
        }

        protected void create_upvalues(BinaryReader buffer, BHeader header, LFunctionParseState s)
        {
            s.upvalues = new LUpvalue[s.lenUpvalues];
            for (int i = 0; i < s.lenUpvalues; i++)
            {
                s.upvalues[i] = new LUpvalue();
            }
        }

        protected void parse_upvalues(BinaryReader buffer, BHeader header, LFunctionParseState s)
        {
            BList<LUpvalue> upvalues = header.upvalueType.parseList(buffer, header);
            s.lenUpvalues = upvalues.blength.asInt();
            s.upvalues = upvalues.asArray(new LUpvalue[s.lenUpvalues]);
        }

        protected void write_upvalues(BinaryWriter output, BHeader header, LFunction o)
        {
            header.upvalueType.writeList(output, header, o.upvalues);
        }

        protected virtual void parse_debug(BinaryReader buffer, BHeader header, LFunctionParseState s)
        {
            if (header.debug)
            {
                Console.WriteLine("-- beginning to parse source lines list");
            }
            s.lines = header.integerType.parseList(buffer, header);
            if (header.debug)
            {
                Console.WriteLine("-- beginning to parse locals list");
            }
            s.locals = header.localType.parseList(buffer, header);
            if (header.debug)
            {
                Console.WriteLine("-- beginning to parse upvalues list");
            }
            BList<LString> upvalueNames = header.stringType.parseList(buffer, header);
            for (int i = 0; i < upvalueNames.blength.asInt(); i++)
            {
                s.upvalues[i].bname = upvalueNames.get(i);
                s.upvalues[i].name = s.upvalues[i].bname.Deref();
            }
        }

        protected virtual void write_debug(BinaryWriter output, BHeader header, LFunction o)
        {
            header.integerType.write(output, header, new BInteger(o.lines.Length));
            for (int i = 0; i < o.lines.Length; i++)
            {
                header.integerType.write(output, header, new BInteger(o.lines[i]));
            }
            header.localType.writeList(output, header, o.locals);
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
            header.integerType.write(output, header, new BInteger(upvalueNameLength));
            for (int i = 0; i < upvalueNameLength; i++)
            {
                header.stringType.write(output, header, o.upvalues[i].bname);
            }
        }

    }

    class LFunctionType50 : LFunctionType
    {

        protected override void parse_main(BinaryReader buffer, BHeader header, LFunctionParseState s)
        {
            s.name = header.stringType.parse(buffer, header);
            s.lineBegin = header.integerType.parse(buffer, header).asInt();
            s.lineEnd = 0;
            s.lenUpvalues = 0xFF & buffer.ReadByte();
            create_upvalues(buffer, header, s);
            s.lenParameter = 0xFF & buffer.ReadByte();
            s.vararg = 0xFF & buffer.ReadByte();
            s.maximumStackSize = 0xFF & buffer.ReadByte();
            parse_debug(buffer, header, s);
            parse_constants(buffer, header, s);
            parse_code(buffer, header, s);
        }

        public override List<Directive> get_directives()
        {
            return new List<Directive> {
                Directive.SOURCE,
                Directive.LINEDEFINED,
                Directive.NUMPARAMS,
                Directive.IS_VARARG,
                Directive.MAXSTACKSIZE,
            };
        }

        public override void write(BinaryWriter output, BHeader header, LFunction o)
        {
            header.stringType.write(output, header, o.name);
            header.integerType.write(output, header, new BInteger(o.lineDefined));
            output.Write((byte)o.numUpvalues);
            output.Write((byte)o.numParams);
            output.Write((byte)o.varArg);
            output.Write((byte)o.maximumStackSize);
            write_debug(output, header, o);
            write_constants(output, header, o);
            write_code(output, header, o);
        }

    }


    class LFunctionType51 : LFunctionType
    {

        protected override void parse_main(BinaryReader buffer, BHeader header, LFunctionParseState s)
        {
            s.name = header.stringType.parse(buffer, header);
            s.lineBegin = header.integerType.parse(buffer, header).asInt();
            s.lineEnd = header.integerType.parse(buffer, header).asInt();
            s.lenUpvalues = 0xFF & buffer.ReadByte();
            create_upvalues(buffer, header, s);
            s.lenParameter = 0xFF & buffer.ReadByte();
            s.vararg = 0xFF & buffer.ReadByte();
            s.maximumStackSize = 0xFF & buffer.ReadByte();
            parse_code(buffer, header, s);
            parse_constants(buffer, header, s);
            parse_debug(buffer, header, s);
        }

        public override List<Directive> get_directives()
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

        public override void write(BinaryWriter output, BHeader header, LFunction o)
        {
            header.stringType.write(output, header, o.name);
            header.integerType.write(output, header, new BInteger(o.lineDefined));
            header.integerType.write(output, header, new BInteger(o.lastLineDefined));
            output.Write((byte)o.numUpvalues);
            output.Write((byte)o.numParams);
            output.Write((byte)o.varArg);
            output.Write((byte)o.maximumStackSize);
            write_code(output, header, o);
            write_constants(output, header, o);
            write_debug(output, header, o);
        }

    }

    class LFunctionType52 : LFunctionType
    {

        protected override void parse_main(BinaryReader buffer, BHeader header, LFunctionParseState s)
        {
            s.lineBegin = header.integerType.parse(buffer, header).asInt();
            s.lineEnd = header.integerType.parse(buffer, header).asInt();
            s.lenParameter = 0xFF & buffer.ReadByte();
            s.vararg = 0xFF & buffer.ReadByte();
            s.maximumStackSize = 0xFF & buffer.ReadByte();
            parse_code(buffer, header, s);
            parse_constants(buffer, header, s);
            parse_upvalues(buffer, header, s);
            s.name = header.stringType.parse(buffer, header);
            parse_debug(buffer, header, s);
        }

        public override List<Directive> get_directives()
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

        public override void write(BinaryWriter output, BHeader header, LFunction o)
        {
            header.integerType.write(output, header, new BInteger(o.lineDefined));
            header.integerType.write(output, header, new BInteger(o.lastLineDefined));
            output.Write((byte)o.numParams);
            output.Write((byte)o.varArg);
            output.Write((byte)o.maximumStackSize);
            write_code(output, header, o);
            write_constants(output, header, o);
            write_upvalues(output, header, o);
            header.stringType.write(output, header, o.name);
            write_debug(output, header, o);
        }

    }

    class LFunctionType53 : LFunctionType
    {

        protected override void parse_main(BinaryReader buffer, BHeader header, LFunctionParseState s)
        {
            s.name = header.stringType.parse(buffer, header); //TODO: psource
            s.lineBegin = header.integerType.parse(buffer, header).asInt();
            s.lineEnd = header.integerType.parse(buffer, header).asInt();
            s.lenParameter = 0xFF & buffer.ReadByte();
            s.vararg = 0xFF & buffer.ReadByte();
            s.maximumStackSize = 0xFF & buffer.ReadByte();
            parse_code(buffer, header, s);
            s.constants = header.constantType.parseList(buffer, header);
            parse_upvalues(buffer, header, s);
            s.functions = header.functionType.parseList(buffer, header);
            parse_debug(buffer, header, s);
        }

        public override List<Directive> get_directives()
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

        public override void write(BinaryWriter output, BHeader header, LFunction o)
        {
            header.stringType.write(output, header, o.name);
            header.integerType.write(output, header, new BInteger(o.lineDefined));
            header.integerType.write(output, header, new BInteger(o.lastLineDefined));
            output.Write((byte)o.numParams);
            output.Write((byte)o.varArg);
            output.Write((byte)o.maximumStackSize);
            write_code(output, header, o);
            header.constantType.writeList(output, header, o.constants);
            write_upvalues(output, header, o);
            header.functionType.writeList(output, header, o.functions);
            write_debug(output, header, o);
        }

    }

    class LFunctionType54 : LFunctionType
    {

        protected override void parse_debug(BinaryReader buffer, BHeader header, LFunctionParseState s)
        {
            // TODO: process line info correctly
            s.lines = (new BIntegerType50(1)).parseList(buffer, header);
            s.abslineinfo = header.absLineInfo.parseList(buffer, header);
            s.locals = header.localType.parseList(buffer, header);
            BList<LString> upvalueNames = header.stringType.parseList(buffer, header);
            for (int i = 0; i < upvalueNames.blength.asInt(); i++)
            {
                s.upvalues[i].bname = upvalueNames.get(i);
                s.upvalues[i].name = s.upvalues[i].bname.Deref();
            }
        }

        protected override void write_debug(BinaryWriter output, BHeader header, LFunction o)
        {
            header.integerType.write(output, header, new BInteger(o.lines.Length));
            for (int i = 0; i < o.lines.Length; i++)
            {
                output.Write((byte)o.lines[i]);
            }
            header.absLineInfo.writeList(output, header, o.absLineInfo);
            header.localType.writeList(output, header, o.locals);
            int upvalueNameLength = 0;
            foreach (LUpvalue upvalue in o.upvalues)
            {
                if (upvalue.bname != null)
                {
                    upvalueNameLength++;
                }
                else
                {
                    break;
                }
            }
            header.integerType.write(output, header, new BInteger(upvalueNameLength));
            for (int i = 0; i < upvalueNameLength; i++)
            {
                header.stringType.write(output, header, o.upvalues[i].bname);
            }
        }

        protected override void parse_main(BinaryReader buffer, BHeader header, LFunctionParseState s)
        {
            s.name = header.stringType.parse(buffer, header);
            s.lineBegin = header.integerType.parse(buffer, header).asInt();
            s.lineEnd = header.integerType.parse(buffer, header).asInt();
            s.lenParameter = 0xFF & buffer.ReadByte();
            s.vararg = 0xFF & buffer.ReadByte();
            s.maximumStackSize = 0xFF & buffer.ReadByte();
            parse_code(buffer, header, s);
            s.constants = header.constantType.parseList(buffer, header);
            parse_upvalues(buffer, header, s);
            s.functions = header.functionType.parseList(buffer, header);
            parse_debug(buffer, header, s);
        }

        public override List<Directive> get_directives()
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

        public override void write(BinaryWriter output, BHeader header, LFunction o)
        {
            header.stringType.write(output, header, o.name);
            header.integerType.write(output, header, new BInteger(o.lineDefined));
            header.integerType.write(output, header, new BInteger(o.lastLineDefined));
            output.Write((byte)o.numParams);
            output.Write((byte)o.varArg);
            output.Write((byte)o.maximumStackSize);
            write_code(output, header, o);
            header.constantType.writeList(output, header, o.constants);
            write_upvalues(output, header, o);
            header.functionType.writeList(output, header, o.functions);
            write_debug(output, header, o);
        }

    }

}