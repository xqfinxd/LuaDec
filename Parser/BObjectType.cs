using System.Collections.Generic;
using System.IO;

namespace LuaDec.Parser
{
    public abstract class BObjectType<T> where T : BObject
    {
        public abstract T Parse(BinaryReader buffer, BHeader header);

        public BList<T> ParseList(BinaryReader buffer, BHeader header) {
            return ParseList(buffer, header, Version.ListLengthMode.Strict, null);
        }

        public BList<T> ParseList(BinaryReader buffer, BHeader header, Version.ListLengthMode mode) {
            return ParseList(buffer, header, mode, null);
        }

        public BList<T> ParseList(BinaryReader buffer, BHeader header, Version.ListLengthMode mode, BInteger knownLength)
        {
            BInteger length = header.integerType.Parse(buffer, header);
            switch (mode)
            {
                case Version.ListLengthMode.Strict:
                    break;
                case Version.ListLengthMode.AllowNegative:
                    if (length.Signum() < 0) length = new BInteger(0);
                    break;
                case Version.ListLengthMode.Ignore:
                    if (knownLength == null) throw new System.InvalidOperationException();
                    if (length.Signum() != 0) length = knownLength;
                    break;
            }
            return ParseList(buffer, header, length);
        }

        public BList<T> ParseList(BinaryReader buffer, BHeader header, BInteger length)
        {
            List<T> values = new List<T>();
            length.Iterate(() =>
            {
                values.Add(Parse(buffer, header));
            });
            return new BList<T>(length, values);
        }

        public abstract void Write(BinaryWriter output, BHeader header, T o);

        public void WriteList(BinaryWriter output, BHeader header, T[] array)
        {
            header.integerType.Write(output, header, new BInteger(array.Length));
            foreach (T o in array)
            {
                Write(output, header, o);
            }
        }

        public void WriteList(BinaryWriter output, BHeader header, BList<T> blist)
        {
            header.integerType.Write(output, header, blist.length);
            IEnumerator<T> it = blist.Iterator();
            while (it.MoveNext())
            {
                Write(output, header, it.Current);
            }
        }
    }
}