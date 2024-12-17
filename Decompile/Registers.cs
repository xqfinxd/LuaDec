using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Target;
using System;
using System.Collections.Generic;

namespace LuaDec.Decompile
{
    public class Registers
    {
        private readonly Declaration[,] declarations;
        private readonly Function f;
        private readonly bool isNoDebug;
        private readonly int[,] updated;
        private readonly int value;
        private readonly IExpression[,] values;
        private bool[] startedLines;

        public bool IsNoDebug => isNoDebug;
        public int Value => value;

        public Registers(int registers, int length, Declaration[] declList, Function f, bool isNoDebug)
        {
            value = registers;
            declarations = new Declaration[registers, length + 1];
            for (int i = 0; i < declList.Length; i++)
            {
                Declaration decl = declList[i];
                int register = 0;
                while (declarations[register, decl.begin] != null)
                {
                    register++;
                }
                decl.register = register;
                for (int line = decl.begin; line <= decl.end; line++)
                {
                    declarations[register, line] = decl;
                }
            }
            values = new IExpression[registers, length + 1];
            IExpression nil = ConstantExpression.CreateNil(0);
            for (int register = 0; register < registers; register++)
            {
                values[register, 0] = nil;
            }
            updated = new int[registers, length + 1];
            startedLines = new bool[length + 1];
            for (int i = 0; i < startedLines.Length; i++)
            {
                startedLines[i] = false;
            }
            this.f = f;
            this.isNoDebug = isNoDebug;
        }

        private void NewDeclaration(Declaration decl, int register, int begin, int end)
        {
            for (int line = begin; line <= end; line++)
            {
                declarations[register, line] = decl;
            }
        }

        public Declaration GetDeclaration(int register, int line)
        {
            return declarations[register, line];
        }

        public IExpression GetExpression(int register, int line)
        {
            if (IsLocal(register, line - 1))
            {
                return new LocalVariable(GetDeclaration(register, line - 1));
            }
            else
            {
                return values[register, line - 1];
            }
        }

        public Function GetFunction()
        {
            return f;
        }

        public IExpression GetKExpression(int register, int line)
        {
            if (f.IsConstant(register))
            {
                return f.GetConstantExpression(f.ConstantIndex(register));
            }
            else
            {
                return GetExpression(register, line);
            }
        }

        public IExpression GetKExpression54(int register, bool k, int line)
        {
            if (k)
            {
                return f.GetConstantExpression(register);
            }
            else
            {
                return GetExpression(register, line);
            }
        }

        public List<Declaration> GetNewLocals(int line)
        {
            return GetNewLocals(line, 0);
        }

        public List<Declaration> GetNewLocals(int line, int first)
        {
            first = Math.Max(0, first);
            List<Declaration> locals = new List<Declaration>(Math.Max(Value - first, 0));
            for (int register = first; register < Value; register++)
            {
                if (IsNewLocal(register, line))
                {
                    locals.Add(GetDeclaration(register, line));
                }
            }
            return locals;
        }

        public ITarget GetTarget(int register, int line)
        {
            if (!IsLocal(register, line))
            {
                throw new System.InvalidOperationException("No declaration exists in register " + register + " at line " + line);
            }
            return new VariableTarget(declarations[register, line]);
        }

        public int GetUpdated(int register, int line)
        {
            return updated[register, line];
        }

        public IExpression GetValue(int register, int line)
        {
            if (IsNoDebug)
            {
                return GetExpression(register, line);
            }
            else
            {
                return values[register, line - 1];
            }
        }

        public Version GetVersion()
        {
            return f.GetVersion();
        }

        public bool IsAssignable(int register, int line)
        {
            return IsLocal(register, line) && (!declarations[register, line].forLoop || IsNoDebug);
        }

        public bool IsKConstant(int register)
        {
            return f.IsConstant(register);
        }

        public bool IsLocal(int register, int line)
        {
            if (register < 0) return false;
            return declarations[register, line] != null;
        }

        public bool IsNewLocal(int register, int line)
        {
            Declaration decl = declarations[register, line];
            return decl != null && decl.begin == line && !decl.forLoop && !decl.forLoopExplicit;
        }

        public void SetExplicitLoopVariable(int register, int begin, int end)
        {
            Declaration decl = GetDeclaration(register, begin);
            if (decl == null)
            {
                decl = new Declaration("_FORV_" + register + "_", begin, end);
                decl.register = register;
                NewDeclaration(decl, register, begin, end);
                if (!IsNoDebug)
                {
                    throw new System.InvalidOperationException("TEMP");
                }
            }
            else if (IsNoDebug)
            {
            }
            else
            {
                if (decl.begin != begin || decl.end != end)
                {
                    Console.Error.WriteLine("given: " + begin + " " + end);
                    Console.Error.WriteLine("expected: " + decl.begin + " " + decl.end);
                    // throw new System.InvalidOperationException();
                }
            }
            decl.forLoopExplicit = true;
        }

        public void SetInternalLoopVariable(int register, int begin, int end)
        {
            Declaration decl = GetDeclaration(register, begin);
            if (decl == null)
            {
                decl = new Declaration("_FOR_", begin, end);
                decl.register = register;
                NewDeclaration(decl, register, begin, end);
                if (!IsNoDebug)
                {
                    throw new System.InvalidOperationException("TEMP");
                }
            }
            else if (IsNoDebug)
            {
                //
            }
            else
            {
                if (decl.begin != begin || decl.end != end)
                {
                    Console.Error.WriteLine("given: " + begin + " " + end);
                    Console.Error.WriteLine("expected: " + decl.begin + " " + decl.end);
                    // throw new System.InvalidOperationException();
                }
            }
            decl.forLoop = true;
        }

        public void SetValue(int register, int line, IExpression expression)
        {
            values[register, line] = expression;
            updated[register, line] = line;
        }

        public void StartLine(int line)
        {
            //if(startedLines[line]) return;
            startedLines[line] = true;
            for (int register = 0; register < Value; register++)
            {
                values[register, line] = values[register, line - 1];
                updated[register, line] = updated[register, line - 1];
            }
        }
    }
}