using LuaDec.Assemble;
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

        private void disassemble(Output output, int level, int index)
        {
            if (parent == null)
            {
                output.WriteLine(".version\t" + function.header.version.Name);
                output.WriteLine();

                foreach (Directive directive in function.header.headerType.get_directives())
                {
                    directive.disassemble(output, function.header, function.header.lheader);
                }
                output.WriteLine();

                if (function.header.opmap != function.header.version.OpcodeMap)
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

            foreach (Directive directive in function.header.functionType.get_directives())
            {
                directive.disassemble(output, function.header, function);
            }
            output.WriteLine();

            if (function.locals.Length > 0)
            {
                for (int local = 1; local <= function.locals.Length; local++)
                {
                    LLocal l = function.locals[local - 1];
                    output.WriteLine(".local\t" + l.name.ToPrintable() + "\t" + l.start + "\t" + l.end);
                }
                output.WriteLine();
            }

            if (function.upvalues.Length > 0)
            {
                for (int upvalue = 1; upvalue <= function.upvalues.Length; upvalue++)
                {
                    LUpvalue u = function.upvalues[upvalue - 1];
                    output.WriteLine(".upvalue\t" + StringUtils.toPrintString(u.name) + "\t" + u.idx + "\t" + u.instack);
                }
                output.WriteLine();
            }

            if (function.constants.Length > 0)
            {
                for (int constant = 1; constant <= function.constants.Length; constant++)
                {
                    output.WriteLine(".constant\tk" + (constant - 1) + "\t" + function.constants[constant - 1].ToPrintable());
                }
                output.WriteLine();
            }

            bool[] label = new bool[function.code.Length];
            for (int line = 1; line <= function.code.Length; line++)
            {
                Op op = code.GetOp(line);
                if (op != null && op.hasJump())
                {
                    int target = code.target(line);
                    if (target >= 1 && target <= label.Length)
                    {
                        label[target - 1] = true;
                    }
                }
            }

            int abslineinfoindex = 0;

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
                    output.WriteString(".line\t" + function.lines[line - 1] + "\t");
                }
                Op op = code.GetOp(line);
                string cpLabel = null;
                if (op != null && op.hasJump())
                {
                    int target = code.target(line);
                    if (target >= 1 && target <= code.Length)
                    {
                        cpLabel = "l" + target;
                    }
                }
                if (op == null)
                {
                    output.WriteLine(Op.defaultTostring(code.codepoint(line), function.header.version, code.getExtractor()));
                }
                else
                {
                    output.WriteLine(op.codePointTostring(code.codepoint(line), code.getExtractor(), cpLabel));
                }
                //output.println("\t" + code.opcode(line) + " " + code.A(line) + " " + code.B(line) + " " + code.C(line) + " " + code.Bx(line) + " " + code.sBx(line) + " " + code.codepoint(line));
            }
            for (int line = function.code.Length + 1; line <= function.lines.Length; line++)
            {
                if (function.absLineInfo != null && abslineinfoindex < function.absLineInfo.Length && function.absLineInfo[abslineinfoindex].pc == line - 1)
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
                new Disassembler(child, "f" + subindex, fullname).disassemble(output, level + 1, subindex);
                subindex++;
            }
        }

        public void disassemble(Output output)
        {
            disassemble(output, 0, 0);
        }
    }
}