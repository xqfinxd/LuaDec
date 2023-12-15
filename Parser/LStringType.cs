using System;
using System.IO;
using System.Text;
using System.Threading;

namespace LuaDec.Parser
{
    internal class LStringType50 : LStringType
    {
        public override LString Parse(BinaryReader buffer, BHeader header)
        {
            BInteger sizeT = header.sizeType.Parse(buffer, header);
            StringBuilder b = this.b.Value;
            b.Length = 0;
            sizeT.Iterate(() => b.Append((char)(0xFF & buffer.ReadByte())));
            if (b.Length == 0)
            {
                return LString.EmptyString;
            }
            else
            {
                char last = b[b.Length - 1];
                b.Remove(b.Length - 1, 1);
                string s = b.ToString();
                if (header.debug)
                {
                    Console.WriteLine("-- parsed <string> \"" + s + "\"");
                }
                return new LString(s, last);
            }
        }

        public override void Write(BinaryWriter output, BHeader header, LString s)
        {
            int len = s.value.Length;
            if (s == LString.EmptyString)
            {
                header.sizeType.Write(output, header, header.sizeType.Create(0));
            }
            else
            {
                header.sizeType.Write(output, header, header.sizeType.Create(len + 1));
                for (int i = 0; i < len; i++)
                {
                    output.Write((byte)s.value[i]);
                }
                output.Write(0);
            }
        }
    }

    internal class LStringType53 : LStringType
    {
        public override LString Parse(BinaryReader buffer, BHeader header)
        {
            BInteger sizeT;
            int size = 0xFF & buffer.ReadByte();
            if(size == 0)
            {
                return LString.EmptyString;
            }
            else if (size == 0xFF)
            {
                sizeT = header.sizeType.Parse(buffer, header);
            }
            else
            {
                sizeT = new BInteger(size);
            }
            StringBuilder b = this.b.Value;
            b.Length = 0;
            bool first = true;
            sizeT.Iterate(() =>
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

        public override void Write(BinaryWriter output, BHeader header, LString s)
        {
            int len = s.value.Length + 1;
            if (len < 0xFF)
            {
                output.Write((byte)len);
            }
            else
            {
                output.Write(0xFF);
                header.sizeType.Write(output, header, header.sizeType.Create(len));
            }
            for (int i = 0; i < s.value.Length; i++)
            {
                output.Write((byte)s.value[i]);
            }
        }
    }

    internal class LStringType54 : LStringType
    {
        public override LString Parse(BinaryReader buffer, BHeader header)
        {
            BInteger sizeT = header.sizeType.Parse(buffer, header);
            if(sizeT.AsInt() == 0)
            {
                return LString.EmptyString;
            }

            StringBuilder b = this.b.Value;
            b.Length = 0;
            bool first = true;
            sizeT.Iterate(() =>
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

        public override void Write(BinaryWriter output, BHeader header, LString s)
        {
            if(s == LString.EmptyString)
            {
                header.sizeType.Write(output, header, header.sizeType.Create(0));
            }
            else
            {
                header.sizeType.Write(output, header, header.sizeType.Create(s.value.Length + 1));
                for (int i = 0; i < s.value.Length; i++)
                {
                    output.Write((byte)s.value[i]);
                }
            }
        }
    }

    public abstract class LStringType : BObjectType<LString>
    {
        protected ThreadLocal<StringBuilder> b = new ThreadLocal<StringBuilder>(() => new StringBuilder());

        public static LStringType Get(Version.StringType type)
        {
            switch (type)
            {
                case Version.StringType.Lua50: return new LStringType50();
                case Version.StringType.Lua53: return new LStringType53();
                case Version.StringType.Lua54: return new LStringType54();
                default: throw new System.InvalidOperationException();
            }
        }
    }
}