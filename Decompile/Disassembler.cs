using LuaDec.Assemble;
using LuaDec.Parser;
using LuaDec.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile
{
    public class Disassembler
    {

        private readonly LFunction function;
        private readonly Code code;
        private readonly string name;
        private readonly string parent;

        public Disassembler(LFunction function) : this(function, "main", null)
        {
            
        }

        private Disassembler(LFunction function, string name, string parent)
        {
            this.function = function;
            this.code = new Code(function);
            this.name = name;
            this.parent = parent;
        }

        public void disassemble(Output output)
        {
            disassemble(output, 0, 0);
        }

        private void disassemble(Output output, int level, int index)
        {
            if (parent == null)
            {
                output.println(".version\t" + function.header.version.Name);
                output.println();

                foreach (Directive directive in function.header.headerType.get_directives())
                {
                    directive.disassemble(output, function.header, function.header.lheader);
                }
                output.println();

                if (function.header.opmap != function.header.version.OpcodeMap)
                {
                    OpcodeMap opmap = function.header.opmap;
                    for (int opcode = 0; opcode < opmap.size(); opcode++)
                    {
                        Op op = opmap.get(opcode);
                        if (op != null)
                        {
                            output.println(Directive.OP.Token + "\t" + opcode + "\t" + op.Name);
                        }
                    }
                    output.println();
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
            output.println(".function\t" + fullname);
            output.println();

            foreach (Directive directive in function.header.functionType.get_directives())
            {
                directive.disassemble(output, function.header, function);
            }
            output.println();

            if (function.locals.Length > 0)
            {
                for (int local = 1; local <= function.locals.Length; local++)
                {
                    LLocal l = function.locals[local - 1];
                    output.println(".local\t" + l.name.ToPrintable() + "\t" + l.start + "\t" + l.end);
                }
                output.println();
            }

            if (function.upvalues.Length > 0)
            {
                for (int upvalue = 1; upvalue <= function.upvalues.Length; upvalue++)
                {
                    LUpvalue u = function.upvalues[upvalue - 1];
                    output.println(".upvalue\t" + StringUtils.toPrintString(u.name) + "\t" + u.idx + "\t" + u.instack);
                }
                output.println();
            }

            if (function.constants.Length > 0)
            {
                for (int constant = 1; constant <= function.constants.Length; constant++)
                {
                    output.println(".constant\tk" + (constant - 1) + "\t" + function.constants[constant - 1].ToPrintable());
                }
                output.println();
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
                    output.println(".label\t" + "l" + line);
                }
                if (function.absLineInfo != null && abslineinfoindex < function.absLineInfo.Length && function.absLineInfo[abslineinfoindex].pc == line - 1)
                {
                    LAbsLineInfo info = function.absLineInfo[abslineinfoindex++];
                    output.println(".abslineinfo\t" + info.pc + "\t" + info.line);
                }
                if (line <= function.lines.Length)
                {
                    output.print(".line\t" + function.lines[line - 1] + "\t");
                }
                Op op = code.GetOp(line);
                string cpLabel = null;
                if (op != null && op.hasJump())
                {
                    int target = code.target(line);
                    if (target >= 1 && target <= code.length)
                    {
                        cpLabel = "l" + target;
                    }
                }
                if (op == null)
                {
                    output.println(Op.defaultTostring(code.codepoint(line), function.header.version, code.getExtractor()));
                }
                else
                {
                    output.println(op.codePointTostring(code.codepoint(line), code.getExtractor(), cpLabel));
                }
                //output.println("\t" + code.opcode(line) + " " + code.A(line) + " " + code.B(line) + " " + code.C(line) + " " + code.Bx(line) + " " + code.sBx(line) + " " + code.codepoint(line));
            }
            for (int line = function.code.Length + 1; line <= function.lines.Length; line++)
            {
                if (function.absLineInfo != null && abslineinfoindex < function.absLineInfo.Length && function.absLineInfo[abslineinfoindex].pc == line - 1)
                {
                    LAbsLineInfo info = function.absLineInfo[abslineinfoindex++];
                    output.println(".abslineinfo\t" + info.pc + "\t" + info.line);
                }
                output.println(".line\t" + function.lines[line - 1]);
            }
            if (function.absLineInfo != null)
            {
                while (abslineinfoindex < function.absLineInfo.Length)
                {
                    LAbsLineInfo info = function.absLineInfo[abslineinfoindex++];
                    output.println(".abslineinfo\t" + info.pc + "\t" + info.line);
                }
            }
            output.println();

            int subindex = 0;
            foreach (LFunction child in function.functions)
            {
                new Disassembler(child, "f" + subindex, fullname).disassemble(output, level + 1, subindex);
                subindex++;
            }
        }

    }

}
