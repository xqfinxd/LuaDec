using System;
using System.IO;
using System.Text;
using System.Threading;

namespace LuaDec.Parser
{
    public abstract class LStringType : BObjectType<LString>
    {

        public static LStringType get(Version.StringType type)
        {
            switch (type)
            {
                case Version.StringType.Lua50: return new LStringType50();
                case Version.StringType.Lua53: return new LStringType53();
                case Version.StringType.Lua54: return new LStringType54();
                default: throw new System.InvalidOperationException();
            }
        }

        protected ThreadLocal<StringBuilder> b = new ThreadLocal<StringBuilder>(() => new StringBuilder());

    }

    class LStringType50 : LStringType
    {

        public override LString parse(BinaryReader buffer, BHeader header)
        {
            BInteger sizeT = header.sizeType.parse(buffer, header);
            StringBuilder b = this.b.Value;
            b.Length = 0;
            sizeT.iterate(() => b.Append((char)(0xFF & buffer.ReadByte())));
            if (b.Length == 0)
            {
                return LString.EmptyString;
            }
            else
            {
                char last = b[b.Length - 1];
                if (last != '\0')
                {
                    throw new System.InvalidOperationException("string value does not have a null terminator");
                }
                b.Remove(b.Length - 1, 1);
                string s = b.ToString();
                if (header.debug)
                {
                    Console.WriteLine("-- parsed <string> \"" + s + "\"");
                }
                return new LString(s);
            }
        }

        public override void write(BinaryWriter output, BHeader header, LString s)
        {
            int len = s.value.Length;
            if (s == LString.EmptyString)
            {
                header.sizeType.write(output, header, header.sizeType.create(0));
            }
            else
            {
                header.sizeType.write(output, header, header.sizeType.create(len + 1));
                for (int i = 0; i < len; i++)
                {
                    output.Write((byte)s.value[i]);
                }
                output.Write(0);
            }
        }
    }

    class LStringType53 : LStringType
    {

        public override LString parse(BinaryReader buffer, BHeader header)
        {
            BInteger sizeT;
            int size = 0xFF & buffer.ReadByte();
            if (size == 0xFF)
            {
                sizeT = header.sizeType.parse(buffer, header);
            }
            else
            {
                sizeT = new BInteger(size);
            }
            StringBuilder b = this.b.Value;
            b.Length = 0;
            bool first = true;
            sizeT.iterate(() =>
            {
                if (!first)
                {
                    b.Append((char)(0xFF & buffer.ReadByte()));
                }
                else
                {
                    first = false;
                }
            });
            string s = b.ToString();
            if (header.debug)
            {
                Console.WriteLine("-- parsed <string> \"" + s + "\"");
            }
            return new LString(s);
        }

        public override void write(BinaryWriter output, BHeader header, LString s)
        {
            int len = s.value.Length + 1;
            if (len < 0xFF)
            {
                output.Write((byte)len);
            }
            else
            {
                output.Write(0xFF);
                header.sizeType.write(output, header, header.sizeType.create(len));
            }
            for (int i = 0; i < s.value.Length; i++)
            {
                output.Write((byte)s.value[i]);
            }
        }
    }

    class LStringType54 : LStringType
    {

        public override LString parse(BinaryReader buffer, BHeader header)
        {
            BInteger sizeT = header.sizeType.parse(buffer, header);
            StringBuilder b = this.b.Value;
            b.Length = 0;
            bool first = true;
            sizeT.iterate(() =>
            {
                if (!first)
                {
                    b.Append((char)(0xFF & buffer.ReadByte()));
                }
                else
                {
                    first = false;
                }
            });
            string s = b.ToString();
            if (header.debug)
            {
                Console.WriteLine("-- parsed <string> \"" + s + "\"");
            }
            return new LString(s);
        }

        public override void write(BinaryWriter output, BHeader header, LString s)
        {
            header.sizeType.write(output, header, header.sizeType.create(s.value.Length + 1));
            for (int i = 0; i < s.value.Length; i++)
            {
                output.Write((byte)s.value[i]);
            }
        }
    }
}