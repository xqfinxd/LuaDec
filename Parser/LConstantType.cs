using System;
using System.IO;

namespace LuaDec.Parser
{
    public class LConstantType : BObjectType<LObject>
    {

        public static LConstantType get(Version.ConstantType type)
        {
            switch (type)
            {
                case Version.ConstantType.Lua50: return new LConstantType50();
                case Version.ConstantType.Lua53: return new LConstantType53();
                case Version.ConstantType.Lua54: return new LConstantType54();
                default: throw new System.InvalidOperationException();
            }
        }

        public override LObject parse(BinaryReader buffer, BHeader header)
        {
            throw new NotImplementedException();
        }

        public override void write(BinaryWriter output, BHeader header, LObject o)
        {
            throw new NotImplementedException();
        }
    }

    class LConstantType50 : LConstantType
    {

        public override LObject parse(BinaryReader buffer, BHeader header)
        {
            int type = 0xFF & buffer.ReadByte();
            if (header.debug)
            {
                Console.WriteLine("-- parsing <constant>, type is ");
                switch (type)
                {
                    case 0:
                        Console.WriteLine("<nil>");
                        break;
                    case 1:
                        Console.WriteLine("<bool>");
                        break;
                    case 3:
                        Console.WriteLine("<number>");
                        break;
                    case 4:
                        Console.WriteLine("<string>");
                        break;
                    default:
                        Console.WriteLine("illegal " + type);
                        break;
                }
            }
            switch (type)
            {
                case 0:
                    return LNil.NIL;
                case 1:
                    return header.booleanType.parse(buffer, header);
                case 3:
                    return header.numberType.parse(buffer, header);
                case 4:
                    return header.stringType.parse(buffer, header);
                default:
                    throw new InvalidOperationException();
            }
        }

        public override void write(BinaryWriter output, BHeader header, LObject o)
        {
            if (o is LNil)
            {
                output.Write(0);
            }
            else if (o is LBoolean)
            {
                output.Write(1);
                header.booleanType.write(output, header, (LBoolean)o);
            }
            else if (o is LNumber)
            {
                output.Write(3);
                header.numberType.write(output, header, (LNumber)o);
            }
            else if (o is LString)
            {
                output.Write(4);
                header.stringType.write(output, header, (LString)o);
            }
            else
            {
                throw new System.InvalidOperationException();
            }
        }

    }

    class LConstantType53 : LConstantType
    {

        public override LObject parse(BinaryReader buffer, BHeader header)
        {
            int type = 0xFF & buffer.ReadByte();
            if (header.debug)
            {
                Console.Write("-- parsing <constant>, type is ");
                switch (type)
                {
                    case 0:
                        Console.WriteLine("<nil>");
                        break;
                    case 1:
                        Console.WriteLine("<bool>");
                        break;
                    case 3:
                        Console.WriteLine("<float>");
                        break;
                    case 0x13:
                        Console.WriteLine("<int>");
                        break;
                    case 4:
                        Console.WriteLine("<short string>");
                        break;
                    case 0x14:
                        Console.WriteLine("<long string>");
                        break;
                    default:
                        Console.WriteLine("illegal " + type);
                        break;
                }
            }
            switch (type)
            {
                case 0:
                    return LNil.NIL;
                case 1:
                    return header.booleanType.parse(buffer, header);
                case 3:
                    return header.doubleType.parse(buffer, header);
                case 0x13:
                    return header.longType.parse(buffer, header);
                case 4:
                    return header.stringType.parse(buffer, header);
                case 0x14:
                    {
                        LString s = header.stringType.parse(buffer, header);
                        s.islong = true;
                        return s;
                    }
                default:
                    throw new System.InvalidOperationException();
            }
        }

        public override void write(BinaryWriter output, BHeader header, LObject o)
        {
            if (o is LNil)
            {
                output.Write(0);
            }
            else if (o is LBoolean)
            {
                output.Write(1);
                header.booleanType.write(output, header, (LBoolean)o);
            }
            else if (o is LNumber)
            {
                LNumber n = (LNumber)o;
                if (!n.integralType())
                {
                    output.Write(3);
                    header.doubleType.write(output, header, (LNumber)o);
                }
                else
                {
                    output.Write(0x13);
                    header.longType.write(output, header, (LNumber)o);
                }
            }
            else if (o is LString)
            {
                LString s = (LString)o;
                output.Write(s.islong ? (byte)0x14 : (byte)4);
                header.stringType.write(output, header, s);
            }
            else
            {
                throw new System.InvalidOperationException();
            }
        }

    }

    class LConstantType54 : LConstantType
    {

        public override LObject parse(BinaryReader buffer, BHeader header)
        {
            int type = 0xFF & buffer.ReadByte();
            switch (type)
            {
                case 0:
                    return LNil.NIL;
                case 1:
                    return LBoolean.LFALSE;
                case 0x11:
                    return LBoolean.LTRUE;
                case 3:
                    return header.longType.parse(buffer, header);
                case 0x13:
                    return header.doubleType.parse(buffer, header);
                case 4:
                    return header.stringType.parse(buffer, header);
                case 0x14:
                    {
                        LString s = header.stringType.parse(buffer, header);
                        s.islong = true;
                        return s;
                    }
                default:
                    throw new System.InvalidOperationException();
            }
        }

        public override void write(BinaryWriter output, BHeader header, LObject o)
        {
            if (o is LNil)
            {
                output.Write(0);
            }
            else if (o is LBoolean)
            {
                if (((LBoolean)o).Value)
                {
                    output.Write(0x11);
                }
                else
                {
                    output.Write(1);
                }
            }
            else if (o is LNumber)
            {
                LNumber n = (LNumber)o;
                if (!n.integralType())
                {
                    output.Write(0x13);
                    header.doubleType.write(output, header, (LNumber)o);
                }
                else
                {
                    output.Write(3);
                    header.longType.write(output, header, (LNumber)o);
                }
            }
            else if (o is LString)
            {
                LString s = (LString)o;
                output.Write(s.islong ? (byte)0x14 : (byte)4);
                header.stringType.write(output, header, s);
            }
            else
            {
                throw new System.InvalidOperationException();
            }
        }

    }
}