using LuaDec.Assemble;
using LuaDec.Decompile;
using System;
using System.Collections.Generic;
using System.IO;

namespace LuaDec.Parser
{
    public class BHeader
    {
        private static readonly byte[] Signature = {
            0x1B, 0x4C, 0x75, 0x61,
        };

        public readonly LAbsLineInfoType absLineInfo;
        public readonly LBooleanType booleanType;
        public readonly Configuration config;
        public readonly LConstantType constantType;
        public readonly bool debug = false;
        public readonly LNumberType doubleType;
        public readonly CodeExtract extractor;
        public readonly LFunctionType functionType;
        public readonly LHeaderType headerType;
        public readonly BIntegerType integerType;
        public readonly LHeader lheader;
        public readonly LLocalType localType;
        public readonly LNumberType longType;
        public readonly LFunction main;
        public readonly LNumberType numberType;
        public readonly OpCodeMap opmap;
        public readonly BIntegerType sizeType;
        public readonly LStringType stringType;
        public readonly LUpvalueType upvalueType;
        public readonly Version version;

        public BHeader(Version version, LHeader lheader) : this(version, lheader, null)
        {
        }

        public BHeader(Version version, LHeader lheader, LFunction main)
        {
            this.config = null;
            this.version = version;
            this.lheader = lheader;
            this.headerType = version.LHeaderType;
            integerType = lheader.intT;
            sizeType = lheader.sizeT;
            booleanType = lheader.boolT;
            numberType = lheader.numberT;
            longType = lheader.longT;
            doubleType = lheader.doubleT;
            stringType = lheader.stringT;
            constantType = lheader.constant;
            absLineInfo = lheader.abslineinfo;
            localType = lheader.local;
            upvalueType = lheader.upvalue;
            functionType = lheader.function;
            extractor = lheader.extractor;
            opmap = version.LOpCodeMap;
            this.main = main;
        }

        public BHeader(BinaryReader buffer, Configuration config)
        {
            this.config = config;
            // 4 byte Lua signature
            for (int i = 0; i < Signature.Length; i++)
            {
                if (buffer.ReadByte() != Signature[i])
                {
                    throw new System.InvalidOperationException("The input file does not have the signature of a valid Lua file.");
                }
            }

            int versionNumber = 0xFF & buffer.ReadByte();
            int major = versionNumber >> 4;
            int minor = versionNumber & 0x0F;

            version = Version.GetVersion(major, minor);
            if (version == null)
            {
                throw new System.InvalidOperationException("The input chunk's Lua version is " + major + "." + minor + "; unluac can only handle Lua 5.0 - Lua 5.4.");
            }

            headerType = version.LHeaderType;
            lheader = headerType.Parse(buffer, this);
            integerType = lheader.intT;
            sizeType = lheader.sizeT;
            booleanType = lheader.boolT;
            numberType = lheader.numberT;
            longType = lheader.longT;
            doubleType = lheader.doubleT;
            stringType = lheader.stringT;
            constantType = lheader.constant;
            absLineInfo = lheader.abslineinfo;
            localType = lheader.local;
            upvalueType = lheader.upvalue;
            functionType = lheader.function;
            extractor = lheader.extractor;

            if (config.OpMapFile != null)
            {
                try
                {
                    Tokenizer t = new Tokenizer(new StreamReader(File.Open(config.OpMapFile, FileMode.Open)));
                    string tok;
                    Dictionary<int, Op> useropmap = new Dictionary<int, Op>();
                    while ((tok = t.next()) != null)
                    {
                        if (tok == ".op")
                        {
                            tok = t.next();
                            if (tok == null) throw new System.InvalidOperationException("Unexpected end of opmap file.");
                            int opcode;
                            try
                            {
                                opcode = int.Parse(tok);
                            }
                            catch (FormatException)
                            {
                                throw new System.InvalidOperationException("Excepted number in opmap file, got \"" + tok + "\".");
                            }
                            tok = t.next();
                            if (tok == null) throw new System.InvalidOperationException("Unexpected end of opmap file.");
                            Op op = version.LOpCodeMap.GetOpByName(tok);
                            if (op == null) throw new System.InvalidOperationException("Unknown op name \"" + tok + "\" in opmap file.");
                            useropmap.Add(opcode, op);
                        }
                        else
                        {
                            throw new System.InvalidOperationException("Unexpected token \"" + tok + "\" + in opmap file.");
                        }
                    }
                    opmap = new OpCodeMap(useropmap);
                }
                catch (IOException e)
                {
                    throw new System.InvalidOperationException(e.Message);
                }
            }
            else
            {
                opmap = version.LOpCodeMap;
            }

            int upvalues = -1;
            if (versionNumber >= 0x53)
            {
                upvalues = 0xFF & buffer.ReadByte();
                if (debug)
                {
                    Console.WriteLine("-- main chunk upvalue count: " + upvalues);
                }
                // TODO: check this value
            }
            main = functionType.Parse(buffer, this);
            if (upvalues >= 0)
            {
                if (main.numUpvalues != upvalues)
                {
                    throw new System.InvalidOperationException("The main chunk has the wrong number of upvalues: " + main.numUpvalues + " (" + upvalues + " expected)");
                }
            }
            if (main.numUpvalues >= 1 && versionNumber >= 0x52 && (main.upvalues[0].name == null || main.upvalues[0].name.Length == 0))
            {
                main.upvalues[0].name = "_ENV";
            }
            main.SetLevel(1);
        }

        public void Write(BinaryWriter output)
        {
            output.Write(Signature, 0, Signature.Length);
            int major = version.VersionMajor;
            int minor = version.VersionMinor;
            int versionNumber = (major << 4) | minor;
            output.Write((byte)versionNumber);
            version.LHeaderType.Write(output, this, lheader);
            if (version.useUpvalueCountInHeader.Value)
            {
                output.Write((byte)main.numUpvalues);
            }
            functionType.Write(output, this, main);
        }
    }
}