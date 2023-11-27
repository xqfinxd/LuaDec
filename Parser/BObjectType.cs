using System.Collections.Generic;
using System.IO;

namespace LuaDec.Parser
{
    public abstract class BObjectType<T> where T : BObject
    {
        public abstract T Parse(BinaryReader buffer, BHeader header);

        public BList<T> ParseList(BinaryReader buffer, BHeader header)
        {
            BInteger length = header.integerType.Parse(buffer, header);
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
            header.integerType.Write(output, header, blist.blength);
            IEnumerator<T> it = blist.Iterator();
            while (it.MoveNext())
            {
                Write(output, header, it.Current);
            }
        }
    }
}