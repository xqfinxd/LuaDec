﻿using LuaDec.Assemble;
using LuaDec.Parser;
using LuaDec.Util;

namespace LuaDec.Decompile
{
    public class Disassembler
    {
        private readonly Code code;
        private readonly LFunction function;
        private readonly string name;
        private readonly string parent;

        private Disassembler(LFunction function, string name, string parent)
        {
            this.function = function;
            this.code = new Code(function);
            this.name = name;
            this.parent = parent;
        }

        public Disassembler(LFunction function) : this(function, "main", null)
        {
        }

        private void Disassemble(Output output, int level, int index)
        {
            int printFlags = PrintFlag.DISASSEMBLER;
            if (parent == null)
            {
                output.WriteLine(".version\t" + function.header.version.Name);
                output.WriteLine();

                foreach (Directive directive in function.header.headerType.GetDirectives())
                {
                    directive.Disassemble(output, function.header, function.header.lheader);
                }
                output.WriteLine();

                if (function.header.opmap != function.header.version.LOpCodeMap)
                {
                    OpCodeMap opmap = function.header.opmap;
                    for (int opcode = 0; opcode < opmap.Length; opcode++)
                    {
                        Op op = opmap.GetOp(opcode);
                        if (op != null)
                        {
                            output.WriteLine(Directive.OP.Token + "\t" + opcode + "\t" + op.Name);
                        }
                    }
                    output.WriteLine();
                }
            }

            string fullname;
            if (parent == null)
            {
                fullname = name;
            }
            else
            {
                fullname = parent + "/" + name;
            }
            output.WriteLine(".function\t" + fullname);
            output.WriteLine();

            foreach (Directive directive in function.header.functionType.GetDirectives())
            {
                directive.Disassemble(output, function.header, function, printFlags);
            }
            output.WriteLine();

            if (function.locals.Length > 0)
            {
                for (int local = 1; local <= function.locals.Length; local++)
                {
                    LLocal l = function.locals[local - 1];
                    output.WriteLine(".local\t" + l.name.ToPrintable(printFlags) + "\t" + l.start + "\t" + l.end);
                }
                output.WriteLine();
            }

            if (function.upvalues.Length > 0)
            {
                for (int upvalue = 1; upvalue <= function.upvalues.Length; upvalue++)
                {
                    LUpvalue u = function.upvalues[upvalue - 1];
                    output.WriteLine(".upvalue\t" + StringUtils.ToString(u.name) + "\t" + u.idx + "\t" + u.instack);
                }
                output.WriteLine();
            }

            if (function.constants.Length > 0)
            {
                for (int constant = 1; constant <= function.constants.Length; constant++)
                {
                    output.WriteLine(".constant\tk" + (constant - 1) + "\t" + function.constants[constant - 1].ToPrintable(printFlags));
                }
                output.WriteLine();
            }

            bool[] label = new bool[function.code.Length];
            for (int line = 1; line <= function.code.Length; line++)
            {
                Op op = code.GetOp(line);
                if (op != null && op.HasJump())
                {
                    int target = code.Target(line);
                    if (target >= 1 && target <= label.Length)
                    {
                        label[target - 1] = true;
                    }
                }
            }

            int abslineinfoindex = 0;
            int upvalueCount = 0;

            for (int line = 1; line <= function.code.Length; line++)
            {
                if (label[line - 1])
                {
                    output.WriteLine(".label\t" + "l" + line);
                }
                if (function.absLineInfo != null && abslineinfoindex < function.absLineInfo.Length && function.absLineInfo[abslineinfoindex].pc == line - 1)
                {
                    LAbsLineInfo info = function.absLineInfo[abslineinfoindex++];
                    output.WriteLine(".abslineinfo\t" + info.pc + "\t" + info.line);
                }
                if (line <= function.lines.Length)
                {
                    output.Write(".line\t" + function.lines[line - 1] + "\t");
                }
                Op op = code.GetOp(line);
                string cpLabel = null;
                if (op != null && op.HasJump())
                {
                    int target = code.Target(line);
                    if (target >= 1 && target <= code.Length)
                    {
                        cpLabel = "l" + target;
                    }
                }
                if (op == null)
                {
                    output.WriteLine(Op.DefaultToString(
                        printFlags,
                        function,
                        code.CodePoint(line),
                        function.header.version,
                        code.GetExtractor(),
                        upvalueCount > 0));
                }
                else
                {
                    output.WriteLine(op.CodePointToString(
                        printFlags,
                        function,
                        code.CodePoint(line),
                        code.GetExtractor(),
                        cpLabel,
                        upvalueCount > 0));
                }

                if (upvalueCount > 0)
                {
                    upvalueCount--;
                }
                else
                {
                    if (op == Op.CLOSURE
                        && function.header.version.upvalueDeclarationType.Value == Version.UpvalueDeclarationType.Inline)
                    {
                        int f = code.BxField(line);
                        if (f >= 0 && f < function.functions.Length)
                        {
                            LFunction closed = function.functions[f];
                            if (closed.numUpvalues > 0)
                            {
                                upvalueCount = closed.numUpvalues;
                            }
                        }
                    }
                }
            }
            for (int line = function.code.Length + 1; line <= function.lines.Length; line++)
            {
                if (function.absLineInfo != null
                    && abslineinfoindex < function.absLineInfo.Length
                    && function.absLineInfo[abslineinfoindex].pc == line - 1)
                {
                    LAbsLineInfo info = function.absLineInfo[abslineinfoindex++];
                    output.WriteLine(".abslineinfo\t" + info.pc + "\t" + info.line);
                }
                output.WriteLine(".line\t" + function.lines[line - 1]);
            }
            if (function.absLineInfo != null)
            {
                while (abslineinfoindex < function.absLineInfo.Length)
                {
                    LAbsLineInfo info = function.absLineInfo[abslineinfoindex++];
                    output.WriteLine(".abslineinfo\t" + info.pc + "\t" + info.line);
                }
            }
            output.WriteLine();

            int subindex = 0;
            foreach (LFunction child in function.functions)
            {
                new Disassembler(child, "f" + subindex, fullname).Disassemble(output, level + 1, subindex);
                subindex++;
            }
        }

        public void Disassemble(Output output)
        {
            Disassemble(output, 0, 0);
        }
    }
}