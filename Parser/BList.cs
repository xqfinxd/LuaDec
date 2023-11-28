using System.Collections.Generic;

namespace LuaDec.Parser
{
    public class BList<T> : BObject where T : BObject
    {
        private readonly List<T> values;
        public readonly BInteger length;

        public BList(BInteger length, List<T> values)
        {
            this.length = length;
            this.values = values;
        }

        public T[] AsArray(T[] array)
        {
            int i = 0;
            length.Iterate(() =>
            {
                array[i] = values[i];
                i++;
            });
            return array;
        }

        public T Get(int index)
        {
            return values[index];
        }

        public IEnumerator<T> Iterator()
        {
            return values.GetEnumerator();
        }
    }
}