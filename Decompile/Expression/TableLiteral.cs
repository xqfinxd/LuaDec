using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile.Expression
{
    public class TableLiteral : IExpression
    {

        public class Entry : IComparable<Entry>
        {

            public readonly IExpression key;
            public readonly IExpression value;
            public readonly bool isList;
            public readonly int timestamp;
            public bool hash;

            public Entry(IExpression key, IExpression value, bool isList, int timestamp)
            {
                this.key = key;
                this.value = value;
                this.isList = isList;
                this.timestamp = timestamp;
            }

            public int CompareTo(Entry e)
            {
                return timestamp.CompareTo(e.timestamp);
            }
        }

        private List<Entry> entries;

        private bool isObject = true;
        private bool isList = true;
        private int listLength = 1;

        private readonly int hashSize;
        private int hashCount;

        public TableLiteral(int arraySize, int hashSize)
            : base(PRECEDENCE_ATOMIC)
        {
          entries = new List<Entry>(arraySize + hashSize);
            this.hashSize = hashSize;
            hashCount = 0;
        }

        public override void walk(Walker w)
        {
            entries.Sort();
            w.VisitExpression(this);
            bool lastEntry = false;
            foreach (Entry entry in entries)
            {
                entry.key.walk(w);
                if (!lastEntry)
                {
                    entry.value.walk(w);
                    if (entry.value.isMultiple())
                    {
                        lastEntry = true;
                    }
                }
            }
        }

        public override int getConstantIndex()
        {
            int index = -1;
            foreach (Entry entry in entries)
            {
                index = Math.Max(entry.key.getConstantIndex(), index);
                index = Math.Max(entry.value.getConstantIndex(), index);
            }
            return index;
        }

        public override void print(Decompiler d, Output output)
        {
            listLength = 1;
            if (entries.Count == 0)
            {
                output.WriteString("{}");
            }
            else
            {
                bool lineBreak = isList && entries.Count > 5 || isObject && entries.Count > 2 || !isObject;
                if (!lineBreak)
                {
                    foreach (Entry entry in entries)
                    {
                        IExpression value = entry.value;
                        if (!(value.isBrief()))
                        {
                            lineBreak = true;
                            break;
                        }
                    }
                }
                output.WriteString("{");
                if (lineBreak)
                {
                    output.WriteLine();
                    output.Indent();
                }
                printEntry(d, 0, output);
                if (!entries[0].value.isMultiple())
                {
                    for (int index = 1; index < entries.Count; index++)
                    {
                        output.WriteString(",");
                        if (lineBreak)
                        {
                            output.WriteLine();
                        }
                        else
                        {
                            output.WriteString(" ");
                        }
                        printEntry(d, index, output);
                        if (entries[index].value.isMultiple())
                        {
                            break;
                        }
                    }
                }
                if (lineBreak)
                {
                    output.WriteLine();
                    output.Dedent();
                }
                output.WriteString("}");
            }
        }

        private void printEntry(Decompiler d, int index, Output output)
        {
            Entry entry = entries[index];
            IExpression key = entry.key;
            IExpression value = entry.value;
            bool isList = entry.isList;
            bool multiple = index + 1 >= entries.Count || value.isMultiple();
            if (isList && key.isInteger() && listLength == key.asInteger())
            {
                if (multiple)
                {
                    value.printMultiple(d, output);
                }
                else
                {
                    value.print(d, output);
                }
                listLength++;
            }
            else if (entry.hash)
            {
                output.WriteString(key.asName());
                output.WriteString(" = ");
                value.print(d, output);
            }
            else
            {
                output.WriteString("[");
                key.printBraced(d, output);
                output.WriteString("] = ");
                value.print(d, output);
            }
        }

        public override bool isTableLiteral()
        {
            return true;
        }

        public override bool isUngrouped()
        {
            return true;
        }

        public override bool isNewEntryAllowed()
        {
            return true;
        }

        public override void addEntry(Entry entry)
        {
            if (hashCount < hashSize && entry.key.isIdentifier())
            {
                entry.hash = true;
                hashCount++;
            }
            entries.Add(entry);
            isObject = isObject && (entry.isList || entry.key.isIdentifier());
            isList = isList && entry.isList;
        }

        public override bool isBrief()
        {
            return false;
        }

    }

}
