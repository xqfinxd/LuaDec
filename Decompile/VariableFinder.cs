using LuaDec.Parser;
using System.Collections.Generic;

namespace LuaDec.Decompile
{
    public class VariableFinder
    {
        private static int LocalCounter = 0;

        private struct RegisterState
        {
            public bool hasRead;
            public bool hasWritten;
            public bool isLocal;
            public bool isTemporary;
            public int lastRead;
            public int lastWritten;
            public int readCount;

            public void Init()
            {
                lastWritten = 1;
                lastRead = -1;
                readCount = 0;
                isTemporary = false;
                isLocal = false;
                hasRead = false;
                hasWritten = false;
            }
        }

        private class RegisterStates
        {
            private int lines;
            private int registers;
            private RegisterState[,] states;

            public RegisterStates(int registers, int lines)
            {
                this.registers = registers;
                this.lines = lines;
                states = new RegisterState[lines, registers];
                for (int line = 0; line < lines; line++)
                {
                    for (int register = 0; register < registers; register++)
                    {
                        states[line, register].Init();
                    }
                }
            }

            public ref RegisterState GetState(int register, int line)
            {
                return ref states[line - 1, register];
            }

            public void NextLine(int line)
            {
                if (line + 1 < lines)
                {
                    for (int r = 0; r < registers; r++)
                    {
                        if (GetState(r, line).lastWritten > GetState(r, line + 1).lastWritten)
                        {
                            GetState(r, line + 1).lastWritten = GetState(r, line).lastWritten;
                        }
                    }
                }
            }

            public void SetLocalRead(int register, int line)
            {
                for (int r = 0; r <= register; r++)
                {
                    GetState(r, GetState(r, line).lastWritten).isLocal = true;
                }
            }

            public void SetLocalWrite(int register_min, int register_max, int line)
            {
                for (int r = 0; r < register_min; r++)
                {
                    GetState(r, GetState(r, line).lastWritten).isLocal = true;
                }
                for (int r = register_min; r <= register_max; r++)
                {
                    GetState(r, line).isLocal = true;
                }
            }

            public void SetRead(int register, int line)
            {
                GetState(register, line).hasRead = true;
                GetState(register, GetState(register, line).lastWritten).readCount++;
                GetState(register, GetState(register, line).lastWritten).lastRead = line;
            }

            public void SetTemporaryRead(int register, int line)
            {
                for (int r = register; r < registers; r++)
                {
                    GetState(r, GetState(r, line).lastWritten).isTemporary = true;
                }
            }

            public void SetTemporaryWrite(int register_min, int register_max, int line)
            {
                for (int r = register_max + 1; r < registers; r++)
                {
                    GetState(r, GetState(r, line).lastWritten).isTemporary = true;
                }
                for (int r = register_min; r <= register_max; r++)
                {
                    GetState(r, line).isTemporary = true;
                }
            }

            public void SetWritten(int register, int line)
            {
                GetState(register, line).hasWritten = true;
                GetState(register, line + 1).lastWritten = line;
            }
        }

        private VariableFinder()
        {
        }

        private static bool IsConstantReference(Decompiler d, int value)
        {
            return d.function.header.extractor.IsK(value);
        }

        public static Declaration[] Process(Decompiler d, int args, int registers)
        {
            Code code = d.code;
            RegisterStates states = new RegisterStates(registers, code.Length);
            bool[] skip = new bool[code.Length];
            for (int line = 1; line <= code.Length; line++)
            {
                states.NextLine(line);
                if (skip[line - 1]) continue;

                int A = code.AField(line);
                int B = code.BField(line);
                int C = code.CField(line);
                switch (code.GetOp(line).Type)
                {
                    case Op.OpT.MOVE:
                        states.SetWritten(A, line);
                        states.SetRead(B, line);
                        if (A < B)
                        {
                            states.SetLocalWrite(A, A, line);
                        }
                        else if (B < A)
                        {
                            states.SetLocalRead(B, line);
                        }
                        break;

                    case Op.OpT.LOADK:
                    case Op.OpT.LOADBOOL:
                    case Op.OpT.GETUPVALUE:
                    case Op.OpT.GETGLOBAL:
                    case Op.OpT.NEWTABLE:
                    case Op.OpT.NEWTABLE50:
                        states.SetWritten(A, line);
                        break;

                    case Op.OpT.LOADNIL:
                    {
                        int maximum = B;
                        int register = code.AField(line);
                        while (register <= maximum)
                        {
                            states.SetWritten(register, line);
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
                            states.SetWritten(register, line);
                            register++;
                        }
                        break;
                    }
                    case Op.OpT.GETTABLE:
                        states.SetWritten(A, line);
                        if (!IsConstantReference(d, code.BField(line))) states.SetRead(B, line);
                        if (!IsConstantReference(d, code.CField(line))) states.SetRead(C, line);
                        break;

                    case Op.OpT.SETGLOBAL:
                    case Op.OpT.SETUPVALUE:
                        states.SetRead(A, line);
                        break;

                    case Op.OpT.SETTABLE:
                    case Op.OpT.ADD:
                    case Op.OpT.SUB:
                    case Op.OpT.MUL:
                    case Op.OpT.DIV:
                    case Op.OpT.MOD:
                    case Op.OpT.POW:
                        states.SetWritten(A, line);
                        if (!IsConstantReference(d, code.BField(line))) states.SetRead(B, line);
                        if (!IsConstantReference(d, code.CField(line))) states.SetRead(C, line);
                        break;

                    case Op.OpT.SELF:
                        states.SetWritten(A, line);
                        states.SetWritten(A + 1, line);
                        states.SetRead(B, line);
                        if (!IsConstantReference(d, code.CField(line))) states.SetRead(C, line);
                        break;

                    case Op.OpT.UNM:
                    case Op.OpT.NOT:
                    case Op.OpT.LEN:
                        states.GetState(code.AField(line), line).hasWritten = true;
                        states.GetState(code.BField(line), line).hasRead = true;
                        break;

                    case Op.OpT.CONCAT:
                        states.SetWritten(A, line);
                        for (int register = B; register <= C; register++)
                        {
                            states.SetRead(register, line);
                            states.SetTemporaryRead(register, line);
                        }
                        break;

                    case Op.OpT.SETLIST:
                        states.SetTemporaryRead(A + 1, line);
                        break;

                    case Op.OpT.JMP:
                    case Op.OpT.JMP52:
                        break;

                    case Op.OpT.EQ:
                    case Op.OpT.LT:
                    case Op.OpT.LE:
                        if (!IsConstantReference(d, code.BField(line))) states.SetRead(B, line);
                        if (!IsConstantReference(d, code.CField(line))) states.SetRead(C, line);
                        break;

                    case Op.OpT.TEST:
                        states.SetRead(A, line);
                        break;

                    case Op.OpT.TESTSET:
                        states.SetWritten(A, line);
                        states.SetLocalWrite(A, A, line);
                        states.SetRead(B, line);
                        break;

                    case Op.OpT.CLOSURE:
                    {
                        LFunction f = d.function.functions[code.BxField(line)];
                        foreach (LUpvalue upvalue in f.upvalues)
                        {
                            if (upvalue.instack)
                            {
                                states.SetLocalRead(upvalue.idx, line);
                            }
                        }
                        states.GetState(code.AField(line), line).hasWritten = true;
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
                                    states.SetWritten(register, line);
                                }
                            }
                        }
                        for (int register = A; register <= A + B - 1; register++)
                        {
                            states.SetRead(register, line);
                            states.SetTemporaryRead(register, line);
                        }
                        if (C >= 2)
                        {
                            int nline = line + 1;
                            int register = A + C - 2;
                            while (register >= A && nline <= code.Length)
                            {
                                if (code.GetOp(nline) == Op.MOVE && code.BField(nline) == register)
                                {
                                    states.SetWritten(code.AField(nline), nline);
                                    states.SetRead(code.BField(nline), nline);
                                    states.SetLocalWrite(code.AField(nline), code.AField(nline), nline);
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
                            states.GetState(register, line).hasRead = true;
                        }
                        break;
                    }
                    default:
                        break;
                }
            }
            for (int register = 0; register < registers; register++)
            {
                states.SetWritten(register, 1);
            }
            for (int line = 1; line <= code.Length; line++)
            {
                for (int register = 0; register < registers; register++)
                {
                    RegisterState s = states.GetState(register, line);
                    if (s.hasWritten)
                    {
                        if (s.readCount >= 2 || (line >= 2 && s.readCount == 0))
                        {
                            states.SetLocalWrite(register, register, line);
                        }
                    }
                }
            }
            for (int line = 1; line <= code.Length; line++)
            {
                for (int register = 0; register < registers; register++)
                {
                    RegisterState s = states.GetState(register, line);
                    if (s.hasWritten && s.isTemporary)
                    {
                        List<int> ancestors = new List<int>();
                        for (int read = 0; read < registers; read++)
                        {
                            RegisterState r = states.GetState(read, line);
                            if (r.hasRead && !r.isLocal)
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
                                if (states.GetState(pregister, pline).hasWritten && ancestors.Contains(pregister))
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
                                RegisterState a = states.GetState(pregister, pline);
                                if (a.hasRead && !a.isLocal)
                                {
                                    ancestors.Add(pregister);
                                }
                            }
                        }
                        foreach (int ancestor in ancestors)
                        {
                            if (pline >= 1)
                            {
                                states.SetLocalRead(ancestor, pline);
                            }
                        }
                    }
                }
            }

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
                    switch (d.GetVersion().varArgtTpe.Value)
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
                    for (int line = 1; line <= code.Length; line++)
                    {
                        RegisterState state = states.GetState(register, line);
                        if (state.isLocal)
                        {
                            temporary = false;
                            local = true;
                        }
                        if (state.isTemporary)
                        {
                            start = line + 1;
                            temporary = true;
                        }
                        if (state.hasRead)
                        {
                            written = 0; read++;
                        }
                        if (state.hasWritten)
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
                        name = id + register + "_" + LocalCounter++;
                    }
                    Declaration decl = new Declaration(name, start, code.Length + d.GetVersion().outerBlockScopeAdjustment.Value);
                    decl.register = register;
                    declList.Add(decl);
                }
            }
            return declList.ToArray();
        }
    }
}