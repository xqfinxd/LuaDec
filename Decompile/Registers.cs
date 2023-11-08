using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Target;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile
{
    public class Registers
    {

        public readonly int registers;
        public readonly int length;

        private readonly Declaration[,] decls;
        private readonly Function f;
        public readonly bool isNoDebug;
        private readonly IExpression[,] values;
        private readonly int[,] updated;

        public Registers(int registers, int length, Declaration[] declList, Function f, bool isNoDebug)
        {
            this.registers = registers;
            this.length = length;
            decls = new Declaration[registers,length + 1];
            for (int i = 0; i < declList.Length; i++)
            {
                Declaration decl = declList[i];
                int register = 0;
                while (decls[register,decl.begin] != null)
                {
                    register++;
                }
                decl.register = register;
                for (int line = decl.begin; line <= decl.end; line++)
                {
                    decls[register,line] = decl;
                }
            }
            values = new IExpression[registers,length + 1];
            IExpression nil = ConstantExpression.createNil(0);
            for (int register = 0; register < registers; register++)
            {
                values[register,0] = nil;
            }
            updated = new int[registers,length + 1];
            startedLines = new bool[length + 1];
            for (int i = 0; i < startedLines.Length; i++)
            {
                startedLines[i] = false;
            }
            this.f = f;
            this.isNoDebug = isNoDebug;
        }

        public Function getFunction()
        {
            return f;
        }

        public bool isAssignable(int register, int line)
        {
            return isLocal(register, line) && (!decls[register,line].forLoop || isNoDebug);
        }

        public bool isLocal(int register, int line)
        {
            if (register < 0) return false;
            return decls[register,line] != null;
        }

        public bool isNewLocal(int register, int line)
        {
            Declaration decl = decls[register,line];
            return decl != null && decl.begin == line && !decl.forLoop && !decl.forLoopExplicit;
        }

        public List<Declaration> getNewLocals(int line)
        {
            return getNewLocals(line, 0);
        }

        public List<Declaration> getNewLocals(int line, int first)
        {
            first = Math.Max(0, first);
            List<Declaration> locals = new List<Declaration>(Math.Max(registers - first, 0));
            for (int register = first; register < registers; register++)
            {
                if (isNewLocal(register, line))
                {
                    locals.Add(getDeclaration(register, line));
                }
            }
            return locals;
        }

        public Declaration getDeclaration(int register, int line)
        {
            return decls[register,line];
        }

        private bool[] startedLines;

        public void startLine(int line)
        {
            //if(startedLines[line]) return;
            startedLines[line] = true;
            for (int register = 0; register < registers; register++)
            {
                values[register,line] = values[register,line - 1];
                updated[register,line] = updated[register,line - 1];
            }
        }

        public bool isKConstant(int register)
        {
            return f.isConstant(register);
        }

        public IExpression getExpression(int register, int line)
        {
            if (isLocal(register, line - 1))
            {
                return new LocalVariable(getDeclaration(register, line - 1));
            }
            else
            {
                return values[register,line - 1];
            }
        }

        public IExpression getKExpression(int register, int line)
        {
            if (f.isConstant(register))
            {
                return f.getConstantExpression(f.constantIndex(register));
            }
            else
            {
                return getExpression(register, line);
            }
        }

        public IExpression getKExpression54(int register, bool k, int line)
        {
            if (k)
            {
                return f.getConstantExpression(register);
            }
            else
            {
                return getExpression(register, line);
            }
        }

        public IExpression getValue(int register, int line)
        {
            if (isNoDebug)
            {
                return getExpression(register, line);
            }
            else
            {
                return values[register,line - 1];
            }
        }

        public int getUpdated(int register, int line)
        {
            return updated[register,line];
        }

        public void setValue(int register, int line, IExpression expression)
        {
            values[register,line] = expression;
            updated[register,line] = line;
        }

        public ITarget getTarget(int register, int line)
        {
            if (!isLocal(register, line))
            {
                throw new System.InvalidOperationException("No declaration exists in register " + register + " at line " + line);
            }
            return new VariableTarget(decls[register,line]);
        }

        public void setInternalLoopVariable(int register, int begin, int end)
        {
            Declaration decl = getDeclaration(register, begin);
            if (decl == null)
            {
                decl = new Declaration("_FOR_", begin, end);
                decl.register = register;
                newDeclaration(decl, register, begin, end);
                if (!isNoDebug)
                {
                    throw new System.InvalidOperationException("TEMP");
                }
            }
            else if (isNoDebug)
            {
                //
            }
            else
            {
                if (decl.begin != begin || decl.end != end)
                {
                    Console.Error.WriteLine("given: " + begin + " " + end);
                    Console.Error.WriteLine("expected: " + decl.begin + " " + decl.end);
                    throw new System.InvalidOperationException();
                }
            }
            decl.forLoop = true;
        }

        public void setExplicitLoopVariable(int register, int begin, int end)
        {
            Declaration decl = getDeclaration(register, begin);
            if (decl == null)
            {
                decl = new Declaration("_FORV_" + register + "_", begin, end);
                decl.register = register;
                newDeclaration(decl, register, begin, end);
                if (!isNoDebug)
                {
                    throw new System.InvalidOperationException("TEMP");
                }
            }
            else if (isNoDebug)
            {

            }
            else
            {
                if (decl.begin != begin || decl.end != end)
                {
                    Console.Error.WriteLine("given: " + begin + " " + end);
                    Console.Error.WriteLine("expected: " + decl.begin + " " + decl.end);
                    throw new System.InvalidOperationException();
                }
            }
            decl.forLoopExplicit = true;
        }

        private void newDeclaration(Declaration decl, int register, int begin, int end)
        {
            for (int line = begin; line <= end; line++)
            {
                decls[register,line] = decl;
            }
        }

        public Version getVersion()
        {
            return f.getVersion();
        }

    }

}
