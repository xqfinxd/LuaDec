using LuaDec.Parser;
using System;
using System.Collections.Generic;

namespace LuaDec.Decompile
{
    public class VariableFinder
    {

        private class RegisterState
        {

            public RegisterState()
            {
                last_written = 1;
                last_read = -1;
                read_count = 0;
                temporary = false;
                local = false;
                read = false;
                written = false;
            }

            public int last_written;
            public int last_read;
            public int read_count;
            public bool temporary;
            public bool local;
            public bool read;
            public bool written;
        }

        private class RegisterStates
        {

            public RegisterStates(int registers, int lines)
            {
                this.registers = registers;
                this.lines = lines;
                states = new RegisterState[lines, registers];
                for (int line = 0; line < lines; line++)
                {
                    for (int register = 0; register < registers; register++)
                    {
                        states[line, register] = new RegisterState();
                    }
                }
            }

            public RegisterState get(int register, int line)
            {
                return states[line - 1, register];
            }

            public void setWritten(int register, int line)
            {
                get(register, line).written = true;
                get(register, line + 1).last_written = line;
            }

            public void setRead(int register, int line)
            {
                get(register, line).read = true;
                get(register, get(register, line).last_written).read_count++;
                get(register, get(register, line).last_written).last_read = line;
            }

            public void setLocalRead(int register, int line)
            {
                for (int r = 0; r <= register; r++)
                {
                    get(r, get(r, line).last_written).local = true;
                }
            }

            public void setLocalWrite(int register_min, int register_max, int line)
            {
                for (int r = 0; r < register_min; r++)
                {
                    get(r, get(r, line).last_written).local = true;
                }
                for (int r = register_min; r <= register_max; r++)
                {
                    get(r, line).local = true;
                }
            }

            public void setTemporaryRead(int register, int line)
            {
                for (int r = register; r < registers; r++)
                {
                    get(r, get(r, line).last_written).temporary = true;
                }
            }

            public void setTemporaryWrite(int register_min, int register_max, int line)
            {
                for (int r = register_max + 1; r < registers; r++)
                {
                    get(r, get(r, line).last_written).temporary = true;
                }
                for (int r = register_min; r <= register_max; r++)
                {
                    get(r, line).temporary = true;
                }
            }

            public void nextLine(int line)
            {
                if (line + 1 < lines)
                {
                    for (int r = 0; r < registers; r++)
                    {
                        if (get(r, line).last_written > get(r, line + 1).last_written)
                        {
                            get(r, line + 1).last_written = get(r, line).last_written;
                        }
                    }
                }
            }

            private int registers;
            private int lines;
            private RegisterState[,] states;

        }

        private static bool isConstantReference(Decompiler d, int value)
        {
            return d.function.header.extractor.is_k(value);
        }

        public static Declaration[] process(Decompiler d, int args, int registers)
        {
            Code code = d.code;
            RegisterStates states = new RegisterStates(registers, code.Length());
            bool[] skip = new bool[code.Length()];
            for (int line = 1; line <= code.Length(); line++)
            {
                states.nextLine(line);
                if (skip[line - 1]) continue;
                int A = code.AField(line);
                int B = code.BField(line);
                int C = code.CField(line);
                switch (code.GetOp(line).Type)
                {
                    case Op.OpT.MOVE:
                        states.setWritten(A, line);
                        states.setRead(B, line);
                        if (A < B)
                        {
                            states.setLocalWrite(A, A, line);
                        }
                        else if (B < A)
                        {
                            states.setLocalRead(B, line);
                        }
                        break;
                    case Op.OpT.LOADK:
                    case Op.OpT.LOADBOOL:
                    case Op.OpT.GETUPVAL:
                    case Op.OpT.GETGLOBAL:
                    case Op.OpT.NEWTABLE:
                    case Op.OpT.NEWTABLE50:
                        states.setWritten(A, line);
                        break;
                    case Op.OpT.LOADNIL:
                        {
                            int maximum = B;
                            int register = code.AField(line);
                            while (register <= maximum)
                            {
                                states.setWritten(register, line);
                                register++;
                            }
                            break;
                        }
                    case Op.OpT.LOADNIL52:
                        {
                            int maximum = A + B;
                            int register = code.AField(line);
                            while (register <= maximum)
                            {
                                states.setWritten(register, line);
                                register++;
                            }
                            break;
                        }
                    case Op.OpT.GETTABLE:
                        states.setWritten(A, line);
                        if (!isConstantReference(d, code.BField(line))) states.setRead(B, line);
                        if (!isConstantReference(d, code.CField(line))) states.setRead(C, line);
                        break;
                    case Op.OpT.SETGLOBAL:
                    case Op.OpT.SETUPVAL:
                        states.setRead(A, line);
                        break;
                    case Op.OpT.SETTABLE:
                    case Op.OpT.ADD:
                    case Op.OpT.SUB:
                    case Op.OpT.MUL:
                    case Op.OpT.DIV:
                    case Op.OpT.MOD:
                    case Op.OpT.POW:
                        states.setWritten(A, line);
                        if (!isConstantReference(d, code.BField(line))) states.setRead(B, line);
                        if (!isConstantReference(d, code.CField(line))) states.setRead(C, line);
                        break;
                    case Op.OpT.SELF:
                        states.setWritten(A, line);
                        states.setWritten(A + 1, line);
                        states.setRead(B, line);
                        if (!isConstantReference(d, code.CField(line))) states.setRead(C, line);
                        break;
                    case Op.OpT.UNM:
                    case Op.OpT.NOT:
                    case Op.OpT.LEN:
                        states.get(code.AField(line), line).written = true;
                        states.get(code.BField(line), line).read = true;
                        break;
                    case Op.OpT.CONCAT:
                        states.setWritten(A, line);
                        for (int register = B; register <= C; register++)
                        {
                            states.setRead(register, line);
                            states.setTemporaryRead(register, line);
                        }
                        break;
                    case Op.OpT.SETLIST:
                        states.setTemporaryRead(A + 1, line);
                        break;
                    case Op.OpT.JMP:
                    case Op.OpT.JMP52:
                        break;
                    case Op.OpT.EQ:
                    case Op.OpT.LT:
                    case Op.OpT.LE:
                        if (!isConstantReference(d, code.BField(line))) states.setRead(B, line);
                        if (!isConstantReference(d, code.CField(line))) states.setRead(C, line);
                        break;
                    case Op.OpT.TEST:
                        states.setRead(A, line);
                        break;
                    case Op.OpT.TESTSET:
                        states.setWritten(A, line);
                        states.setLocalWrite(A, A, line);
                        states.setRead(B, line);
                        break;
                    case Op.OpT.CLOSURE:
                        {
                            LFunction f = d.function.functions[code.BxField(line)];
                            foreach (LUpvalue upvalue in f.upvalues)
                            {
                                if (upvalue.instack)
                                {
                                    states.setLocalRead(upvalue.idx, line);
                                }
                            }
                            states.get(code.AField(line), line).written = true;
                            break;
                        }
                    case Op.OpT.CALL:
                    case Op.OpT.TAILCALL:
                        {
                            if (code.GetOp(line) != Op.TAILCALL)
                            {
                                if (C >= 2)
                                {
                                    for (int register = A; register <= A + C - 2; register++)
                                    {
                                        states.setWritten(register, line);
                                    }
                                }
                            }
                            for (int register = A; register <= A + B - 1; register++)
                            {
                                states.setRead(register, line);
                                states.setTemporaryRead(register, line);
                            }
                            if (C >= 2)
                            {
                                int nline = line + 1;
                                int register = A + C - 2;
                                while (register >= A && nline <= code.Length())
                                {
                                    if (code.GetOp(nline) == Op.MOVE && code.BField(nline) == register)
                                    {
                                        states.setWritten(code.AField(nline), nline);
                                        states.setRead(code.BField(nline), nline);
                                        states.setLocalWrite(code.AField(nline), code.AField(nline), nline);
                                        skip[nline - 1] = true;
                                    }
                                    register--;
                                    nline++;
                                }
                            }
                            break;
                        }
                    case Op.OpT.RETURN:
                        {
                            if (B == 0) B = registers - code.AField(line) + 1;
                            for (int register = A; register <= A + B - 2; register++)
                            {
                                states.get(register, line).read = true;
                            }
                            break;
                        }
                    default:
                        break;
                }
            }
            for (int register = 0; register < registers; register++)
            {
                states.setWritten(register, 1);
            }
            for (int line = 1; line <= code.Length(); line++)
            {
                for (int register = 0; register < registers; register++)
                {
                    RegisterState s = states.get(register, line);
                    if (s.written)
                    {
                        if (s.read_count >= 2 || (line >= 2 && s.read_count == 0))
                        {
                            states.setLocalWrite(register, register, line);
                        }
                    }
                }
            }
            for (int line = 1; line <= code.Length(); line++)
            {
                for (int register = 0; register < registers; register++)
                {
                    RegisterState s = states.get(register, line);
                    if (s.written && s.temporary)
                    {
                        List<int> ancestors = new List<int>();
                        for (int read = 0; read < registers; read++)
                        {
                            RegisterState r = states.get(read, line);
                            if (r.read && !r.local)
                            {
                                ancestors.Add(read);
                            }
                        }
                        int pline;
                        for (pline = line - 1; pline >= 1; pline--)
                        {
                            bool any_written = false;
                            for (int pregister = 0; pregister < registers; pregister++)
                            {
                                if (states.get(pregister, pline).written && ancestors.Contains(pregister))
                                {
                                    any_written = true;
                                    ancestors.Remove(pregister);
                                }
                            }
                            if (!any_written)
                            {
                                break;
                            }
                            for (int pregister = 0; pregister < registers; pregister++)
                            {
                                RegisterState a = states.get(pregister, pline);
                                if (a.read && !a.local)
                                {
                                    ancestors.Add(pregister);
                                }
                            }
                        }
                        foreach (int ancestor in ancestors)
                        {
                            if (pline >= 1)
                            {
                                states.setLocalRead(ancestor, pline);
                            }
                        }
                    }
                }
            }
            /*
            for(int register = 0; register < registers; register++) {
              for(int line = 1; line <= code.Length(); line++) {
                RegisterState s = states.get(register, line);
                if(s.written || line == 1) {
                  System.output.println("WRITE r:" + register + " l:" + line + " .. " + s.last_read);
                  if(s.local) System.output.println("  LOCAL");
                  if(s.temporary) System.output.println("  TEMPORARY");
                  System.output.println("  READ_COUNT " + s.read_count);
                }
              }
            }
            //*/
            List<Declaration> declList = new List<Declaration>(registers);
            for (int register = 0; register < registers; register++)
            {
                string id = "L";
                bool local = false;
                bool temporary = false;
                int read = 0;
                int written = 0;
                int start = 0;
                if (register < args)
                {
                    local = true;
                    id = "A";
                }
                bool is_arg = false;
                if (register == args)
                {
                    switch (d.getVersion().varArgtTpe.Value)
                    {
                        case Version.VarArgType.Arg:
                        case Version.VarArgType.Hybrid:
                            if ((d.function.varArg & 1) != 0)
                            {
                                local = true;
                                is_arg = true;
                            }
                            break;
                        case Version.VarArgType.Ellipsis:
                            break;
                    }
                }
                if (!local && !temporary)
                {
                    for (int line = 1; line <= code.Length(); line++)
                    {
                        RegisterState state = states.get(register, line);
                        if (state.local)
                        {
                            temporary = false;
                            local = true;
                        }
                        if (state.temporary)
                        {
                            start = line + 1;
                            temporary = true;
                        }
                        if (state.read)
                        {
                            written = 0; read++;
                        }
                        if (state.written)
                        {
                            if (written > 0 && read == 0)
                            {
                                temporary = false;
                                local = true;
                            }
                            read = 0; written++;
                        }
                    }
                }
                if (!local && !temporary)
                {
                    if (read >= 2 || read == 0 && written != 0)
                    {
                        local = true;
                    }
                }
                if (local && temporary)
                {
                    //throw new System.InvalidOperationException();
                }
                if (local)
                {
                    string name;
                    if (is_arg)
                    {
                        name = "arg";
                    }
                    else
                    {
                        name = id + register + "_" + lc++;
                    }
                    Declaration decl = new Declaration(name, start, code.Length() + d.getVersion().outerBlockScopeAdjustment.Value);
                    decl.register = register;
                    declList.Add(decl);
                }
            }
            //DEBUG
            /*
            foreach (Declaration decl  in declList) {
              System.output.println("decl: " + decl.name + " " + decl.begin + " " + decl.end);
            }*/
            return declList.ToArray();
        }

        static int lc = 0;

        private VariableFinder() { }

    }

}
