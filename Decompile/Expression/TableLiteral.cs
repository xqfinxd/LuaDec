using System;
using System.Collections.Generic;

namespace LuaDec.Decompile.Expression
{
    public class TableLiteral : IExpression
    {
        public class Entry : IComparable<Entry>
        {
            public readonly bool isList;
            public readonly IExpression key;
            public readonly int timestamp;
            public readonly IExpression value;
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

        private readonly int hashSize;
        private List<Entry> entries;

        private int hashCount;
        private bool isList = true;
        private bool isObject = true;
        private int listLength = 1;

        public TableLiteral(int arraySize, int hashSize)
            : base(PRECEDENCE_ATOMIC)
        {
            entries = new List<Entry>(arraySize + hashSize);
            this.hashSize = hashSize;
            hashCount = 0;
        }

        private void WriteEntry(Decompiler d, int index, Output output)
        {
            Entry entry = entries[index];
            IExpression key = entry.key;
            IExpression value = entry.value;
            bool isList = entry.isList;
            bool multiple = index + 1 >= entries.Count || value.IsMultiple();
            if (isList && key.IsInteger() && listLength == key.AsInteger())
            {
                if (multiple)
                {
                    value.WriteMultiple(d, output);
                }
                else
                {
                    value.Write(d, output);
                }
                listLength++;
            }
            else if (entry.hash)
            {
                output.Write(key.AsName());
                output.Write(" = ");
                value.Write(d, output);
            }
            else
            {
                output.Write("[");
                key.WriteBraced(d, output);
                output.Write("] = ");
                value.Write(d, output);
            }
        }

        public override void AddEntry(Entry entry)
        {
            if (hashCount < hashSize && entry.key.IsIdentifier())
            {
                entry.hash = true;
                hashCount++;
            }
            entries.Add(entry);
            isObject = isObject && (entry.isList || entry.key.IsIdentifier());
            isList = isList && entry.isList;
        }

        public override int GetConstantIndex()
        {
            int index = -1;
            foreach (Entry entry in entries)
            {
                index = Math.Max(entry.key.GetConstantIndex(), index);
                index = Math.Max(entry.value.GetConstantIndex(), index);
            }
            return index;
        }

        public override bool IsBrief()
        {
            return false;
        }

        public override bool IsNewEntryAllowed()
        {
            return true;
        }

        public override bool IsTableLiteral()
        {
            return true;
        }

        public override bool IsUngrouped()
        {
            return true;
        }

        public override void Walk(Walker w)
        {
            entries.Sort();
            w.VisitExpression(this);
            bool lastEntry = false;
            foreach (Entry entry in entries)
            {
                entry.key.Walk(w);
                if (!lastEntry)
                {
                    entry.value.Walk(w);
                    if (entry.value.IsMultiple())
                    {
                        lastEntry = true;
                    }
                }
            }
        }

        public override void Write(Decompiler d, Output output)
        {
            listLength = 1;
            if (entries.Count == 0)
            {
                output.Write("{}");
            }
            else
            {
                bool lineBreak = isList && entries.Count > 5 || isObject && entries.Count > 2 || !isObject;
                if (!lineBreak)
                {
                    foreach (Entry entry in entries)
                    {
                        IExpression value = entry.value;
                        if (!(value.IsBrief()))
                        {
                            lineBreak = true;
                            break;
                        }
                    }
                }
                output.Write("{");
                if (lineBreak)
                {
                    output.WriteLine();
                    output.Indent();
                }
                WriteEntry(d, 0, output);
                if (!entries[0].value.IsMultiple())
                {
                    for (int index = 1; index < entries.Count; index++)
                    {
                        output.Write(",");
                        if (lineBreak)
                        {
                            output.WriteLine();
                        }
                        else
                        {
                            output.Write(" ");
                        }
                        WriteEntry(d, index, output);
                        if (entries[index].value.IsMultiple())
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
                output.Write("}");
            }
        }
    }
}