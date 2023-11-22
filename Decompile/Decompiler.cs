﻿using LuaDec.Decompile.Block;
using LuaDec.Decompile.Expression;
using LuaDec.Decompile.Operation;
using LuaDec.Decompile.Statement;
using LuaDec.Decompile.Target;
using LuaDec.Parser;
using System;
using System.Collections.Generic;

namespace LuaDec.Decompile
{
    public class Decompiler
    {
        public class ConstantWalker : Walker
        {
            private int nextConstant = 0;
            private HashSet<int> unusedConstants;

            public ConstantWalker(HashSet<int> unusedConstants)
            {
                this.unusedConstants = unusedConstants;
            }

            public override void VisitExpression(IExpression expression)
            {
                if (expression.isConstant())
                {
                    int index = expression.getConstantIndex();
                    if (index >= 0)
                    {
                        while (index > nextConstant)
                        {
                            unusedConstants.Add(nextConstant++);
                        }
                        if (index == nextConstant)
                        {
                            nextConstant++;
                        }
                    }
                }
            }
        }

        public class FunctionConstantWalker : Walker
        {
            private Function f;
            private int nextConstant = 0;
            private HashSet<int> unusedConstants;

            public FunctionConstantWalker(HashSet<int> unusedConstants, Function f)
            {
                this.unusedConstants = unusedConstants;
                this.f = f;
            }

            public override void VisitExpression(IExpression expression)
            {
                if (expression.isConstant())
                {
                    int index = expression.getConstantIndex();
                    if (index >= nextConstant)
                    {
                        nextConstant = index + 1;
                    }
                }
            }

            public override void VisitStatement(IStatement statement)
            {
                if (unusedConstants.Contains(nextConstant))
                {
                    if (statement.useConstant(f, nextConstant))
                    {
                        nextConstant++;
                    }
                }
            }
        }

        public class State
        {
            public bool[] labels;
            public IBlock outer;
            public Registers r;
            public bool[] skip;
        }

        private readonly Function f;
        private readonly LFunction[] functions;
        private readonly int length;
        private readonly int paramNum;
        private readonly int registers;
        private readonly Upvalues upvalues;
        private readonly int varArg;
        public readonly Code code;
        public readonly Declaration[] declarations;
        public readonly LFunction function;

        public Decompiler(LFunction function) : this(function, null, -1)
        {
        }

        public Decompiler(LFunction function, Declaration[] parentDecls, int line)
        {
            this.f = new Function(function);
            this.function = function;
            registers = function.maximumStackSize;
            length = function.code.Length;
            code = new Code(function);
            if (function.stripped || getConfiguration().Variable == Configuration.VariableMode.Nodebug)
            {
                if (getConfiguration().Variable == Configuration.VariableMode.Finder)
                {
                    declarations = VariableFinder.Process(this, function.numParams, function.maximumStackSize);
                }
                else
                {
                    declarations = new Declaration[function.maximumStackSize];
                    int scopeEnd = length + function.header.version.outerBlockScopeAdjustment.Value;
                    int i;
                    for (i = 0; i < Math.Min(function.numParams, function.maximumStackSize); i++)
                    {
                        declarations[i] = new Declaration("A" + i + "_" + function.level, 0, scopeEnd);
                    }
                    if (getVersion().varArgtTpe.Value != Version.VarArgType.Ellipsis && (function.varArg & 1) != 0 && i < function.maximumStackSize)
                    {
                        declarations[i++] = new Declaration("arg", 0, scopeEnd);
                    }
                    for (; i < function.maximumStackSize; i++)
                    {
                        declarations[i] = new Declaration("L" + i + "_" + function.level, 0, scopeEnd);
                    }
                }
            }
            else if (function.locals.Length >= function.numParams)
            {
                declarations = new Declaration[function.locals.Length];
                for (int i = 0; i < declarations.Length; i++)
                {
                    declarations[i] = new Declaration(function.locals[i], code);
                }
            }
            else
            {
                declarations = new Declaration[function.numParams];
                for (int i = 0; i < declarations.Length; i++)
                {
                    declarations[i] = new Declaration("_ARG_" + i + "_", 0, length - 1);
                }
            }
            upvalues = new Upvalues(function, parentDecls, line);
            functions = function.functions;
            paramNum = function.numParams;
            varArg = function.varArg;
        }

        private IExpression.BinaryOperation decodeBinOp(int tm)
        {
            switch (tm)
            {
                case 6: return IExpression.BinaryOperation.ADD;
                case 7: return IExpression.BinaryOperation.SUB;
                case 8: return IExpression.BinaryOperation.MUL;
                case 9: return IExpression.BinaryOperation.MOD;
                case 10: return IExpression.BinaryOperation.POW;
                case 11: return IExpression.BinaryOperation.DIV;
                case 12: return IExpression.BinaryOperation.IDIV;
                case 13: return IExpression.BinaryOperation.BAND;
                case 14: return IExpression.BinaryOperation.BOR;
                case 15: return IExpression.BinaryOperation.BXOR;
                case 16: return IExpression.BinaryOperation.SHL;
                case 17: return IExpression.BinaryOperation.SHR;
                default: throw new System.InvalidOperationException();
            }
        }

        private int fb2int(int fb)
        {
            int exponent = (fb >> 3) & 0x1f;
            if (exponent == 0)
            {
                return fb;
            }
            else
            {
                return ((fb & 7) + 8) << (exponent - 1);
            }
        }

        private int fb2int50(int fb)
        {
            return (fb & 7) << (fb >> 3);
        }

        private ITarget getMoveIntoTargetTarget(Registers r, int line, int previous)
        {
            switch (code.GetOp(line).Type)
            {
                case Op.OpT.MOVE:
                    return r.GetTarget(code.AField(line), line);

                case Op.OpT.SETUPVALUE:
                    return new UpvalueTarget(upvalues.getName(code.BField(line)));

                case Op.OpT.SETGLOBAL:
                    return new GlobalTarget(f.GetGlobalName(code.BxField(line)));

                case Op.OpT.SETTABLE:
                    return new TableTarget(r.GetExpression(code.AField(line), previous), r.GetKExpression(code.BField(line), previous));

                case Op.OpT.SETTABLE54:
                    return new TableTarget(r.GetExpression(code.AField(line), previous), r.GetExpression(code.BField(line), previous));

                case Op.OpT.SETI:
                    return new TableTarget(r.GetExpression(code.AField(line), previous), ConstantExpression.createint(code.BField(line)));

                case Op.OpT.SETFIELD:
                    return new TableTarget(r.GetExpression(code.AField(line), previous), f.GetConstantExpression(code.BField(line)));

                case Op.OpT.SETTABUP:
                {
                    int A = code.AField(line);
                    int B = code.BField(line);
                    return new TableTarget(upvalues.getExpression(A), r.GetKExpression(B, previous));
                }
                case Op.OpT.SETTABUP54:
                {
                    int A = code.AField(line);
                    int B = code.BField(line);
                    return new TableTarget(upvalues.getExpression(A), f.GetConstantExpression(B));
                }
                default:
                    throw new System.InvalidOperationException();
            }
        }

        private IExpression getMoveIntoTargetValue(Registers r, int line, int previous)
        {
            int A = code.AField(line);
            int B = code.BField(line);
            int C = code.CField(line);
            switch (code.GetOp(line).Type)
            {
                case Op.OpT.MOVE:
                    return r.GetValue(B, previous);

                case Op.OpT.SETUPVALUE:
                case Op.OpT.SETGLOBAL:
                    return r.GetExpression(A, previous);

                case Op.OpT.SETTABLE:
                case Op.OpT.SETTABUP:
                    if (f.IsConstant(C))
                    {
                        throw new System.InvalidOperationException();
                    }
                    else
                    {
                        return r.GetExpression(C, previous);
                    }
                case Op.OpT.SETTABLE54:
                case Op.OpT.SETI:
                case Op.OpT.SETFIELD:
                case Op.OpT.SETTABUP54:
                    if (code.kField(line))
                    {
                        throw new System.InvalidOperationException();
                    }
                    else
                    {
                        return r.GetExpression(C, previous);
                    }
                default:
                    throw new System.InvalidOperationException();
            }
        }

        private void handle50BinOp(List<IOperation> operations, State state, int line, IExpression.BinaryOperation op)
        {
            operations.Add(new RegisterSet(line, code.AField(line), IExpression.make(op, state.r.GetKExpression(code.BField(line), line), state.r.GetKExpression(code.CField(line), line))));
        }

        private void handle54BinKOp(List<IOperation> operations, State state, int line, IExpression.BinaryOperation op)
        {
            if (line + 1 > code.Length || code.GetOp(line + 1) != Op.MMBINK) throw new System.InvalidOperationException();
            IExpression left = state.r.GetExpression(code.BField(line), line);
            IExpression right = f.GetConstantExpression(code.CField(line));
            if (code.kField(line + 1))
            {
                IExpression temp = left;
                left = right;
                right = temp;
            }
            operations.Add(new RegisterSet(line, code.AField(line), IExpression.make(op, left, right)));
        }

        private void handle54BinOp(List<IOperation> operations, State state, int line, IExpression.BinaryOperation op)
        {
            operations.Add(new RegisterSet(line, code.AField(line), IExpression.make(op, state.r.GetExpression(code.BField(line), line), state.r.GetExpression(code.CField(line), line))));
        }

        private void handleInitialDeclares(Output output)
        {
            List<Declaration> initdecls = new List<Declaration>(declarations.Length);
            int initdeclcount = paramNum;
            switch (getVersion().varArgtTpe.Value)
            {
                case Version.VarArgType.Arg:
                case Version.VarArgType.Hybrid:
                    initdeclcount += varArg & 1;
                    break;

                case Version.VarArgType.Ellipsis:
                    break;
            }
            for (int i = initdeclcount; i < declarations.Length; i++)
            {
                if (declarations[i].begin == 0)
                {
                    initdecls.Add(declarations[i]);
                }
            }
            if (initdecls.Count > 0)
            {
                output.WriteString("local ");
                output.WriteString(initdecls[0].name);
                for (int i = 1; i < initdecls.Count; i++)
                {
                    output.WriteString(", ");
                    output.WriteString(initdecls[i].name);
                }
                output.WriteLine();
            }
        }

        private void handleSetList(List<IOperation> operations, State state, int line, int stack, int count, int offset)
        {
            IExpression table = state.r.GetValue(stack, line);
            for (int i = 1; i <= count; i++)
            {
                operations.Add(new TableSet(line, table, ConstantExpression.createint(offset + i), state.r.GetExpression(stack + i, line), false, state.r.GetUpdated(stack + i, line)));
            }
        }

        private void handleUnaryOp(List<IOperation> operations, State state, int line, IExpression.UnaryOperation op)
        {
            operations.Add(new RegisterSet(line, code.AField(line), IExpression.make(op, state.r.GetExpression(code.BField(line), line))));
        }

        private void handleUnusedConstants(IBlock outer)
        {
            HashSet<int> unusedConstants = new HashSet<int>();
            outer.walk(new ConstantWalker(unusedConstants));
            outer.walk(new FunctionConstantWalker(unusedConstants, f));
        }

        private IExpression initialExpression(State state, int register, int line)
        {
            if (line == 1)
            {
                if (register < function.numParams) throw new System.InvalidOperationException();
                return ConstantExpression.createNil(line);
            }
            else
            {
                return state.r.GetExpression(register, line - 1);
            }
        }

        private bool isMoveIntoTarget(Registers r, int line)
        {
            if (code.isUpvalueDeclaration(line)) return false;
            switch (code.GetOp(line).Type)
            {
                case Op.OpT.MOVE:
                    return r.IsAssignable(code.AField(line), line) && !r.IsLocal(code.BField(line), line);

                case Op.OpT.SETUPVALUE:
                case Op.OpT.SETGLOBAL:
                    return !r.IsLocal(code.AField(line), line);

                case Op.OpT.SETTABLE:
                case Op.OpT.SETTABUP:
                {
                    int C = code.CField(line);
                    if (f.IsConstant(C))
                    {
                        return false;
                    }
                    else
                    {
                        return !r.IsLocal(C, line);
                    }
                }
                case Op.OpT.SETTABLE54:
                case Op.OpT.SETI:
                case Op.OpT.SETFIELD:
                case Op.OpT.SETTABUP54:
                {
                    if (code.kField(line))
                    {
                        return false;
                    }
                    else
                    {
                        return !r.IsLocal(code.CField(line), line);
                    }
                }
                default:
                    return false;
            }
        }

        private List<IOperation> processLine(State state, int line)
        {
            Registers r = state.r;
            bool[] skip = state.skip;
            List<IOperation> operations = new List<IOperation>();
            int A = code.AField(line);
            int B = code.BField(line);
            int C = code.CField(line);
            int Bx = code.BxField(line);
            switch (code.GetOp(line).Type)
            {
                case Op.OpT.MOVE:
                    operations.Add(new RegisterSet(line, A, r.GetExpression(B, line)));
                    break;

                case Op.OpT.LOADI:
                    operations.Add(new RegisterSet(line, A, ConstantExpression.createint(code.sBxField(line))));
                    break;

                case Op.OpT.LOADF:
                    operations.Add(new RegisterSet(line, A, ConstantExpression.createDouble((double)code.sBxField(line))));
                    break;

                case Op.OpT.LOADK:
                    operations.Add(new RegisterSet(line, A, f.GetConstantExpression(Bx)));
                    break;

                case Op.OpT.LOADKX:
                    if (line + 1 > code.Length || code.GetOp(line + 1) != Op.EXTRAARG) throw new System.InvalidOperationException();
                    operations.Add(new RegisterSet(line, A, f.GetConstantExpression(code.AxField(line + 1))));
                    break;

                case Op.OpT.LOADBOOL:
                    operations.Add(new RegisterSet(line, A, ConstantExpression.createbool(B != 0)));
                    break;

                case Op.OpT.LOADFALSE:
                case Op.OpT.LFALSESKIP:
                    operations.Add(new RegisterSet(line, A, ConstantExpression.createbool(false)));
                    break;

                case Op.OpT.LOADTRUE:
                    operations.Add(new RegisterSet(line, A, ConstantExpression.createbool(true)));
                    break;

                case Op.OpT.LOADNIL:
                    operations.Add(new LoadNil(line, A, B));
                    break;

                case Op.OpT.LOADNIL52:
                    operations.Add(new LoadNil(line, A, A + B));
                    break;

                case Op.OpT.GETGLOBAL:
                    operations.Add(new RegisterSet(line, A, f.GetGlobalExpression(Bx)));
                    break;

                case Op.OpT.SETGLOBAL:
                    operations.Add(new GlobalSet(line, f.GetGlobalName(Bx), r.GetExpression(A, line)));
                    break;

                case Op.OpT.GETUPVALUE:
                    operations.Add(new RegisterSet(line, A, upvalues.getExpression(B)));
                    break;

                case Op.OpT.SETUPVALUE:
                    operations.Add(new UpvalueSet(line, upvalues.getName(B), r.GetExpression(A, line)));
                    break;

                case Op.OpT.GETTABUP:
                    operations.Add(new RegisterSet(line, A, new TableReference(upvalues.getExpression(B), r.GetKExpression(C, line))));
                    break;

                case Op.OpT.GETTABUP54:
                    operations.Add(new RegisterSet(line, A, new TableReference(upvalues.getExpression(B), f.GetConstantExpression(C))));
                    break;

                case Op.OpT.GETTABLE:
                    operations.Add(new RegisterSet(line, A, new TableReference(r.GetExpression(B, line), r.GetKExpression(C, line))));
                    break;

                case Op.OpT.GETTABLE54:
                    operations.Add(new RegisterSet(line, A, new TableReference(r.GetExpression(B, line), r.GetExpression(C, line))));
                    break;

                case Op.OpT.GETI:
                    operations.Add(new RegisterSet(line, A, new TableReference(r.GetExpression(B, line), ConstantExpression.createint(C))));
                    break;

                case Op.OpT.GETFIELD:
                    operations.Add(new RegisterSet(line, A, new TableReference(r.GetExpression(B, line), f.GetConstantExpression(C))));
                    break;

                case Op.OpT.SETTABLE:
                    operations.Add(new TableSet(line, r.GetExpression(A, line), r.GetKExpression(B, line), r.GetKExpression(C, line), true, line));
                    break;

                case Op.OpT.SETTABLE54:
                    operations.Add(new TableSet(line, r.GetExpression(A, line), r.GetExpression(B, line), r.GetKExpression54(C, code.kField(line), line), true, line));
                    break;

                case Op.OpT.SETI:
                    operations.Add(new TableSet(line, r.GetExpression(A, line), ConstantExpression.createint(B), r.GetKExpression54(C, code.kField(line), line), true, line));
                    break;

                case Op.OpT.SETFIELD:
                    operations.Add(new TableSet(line, r.GetExpression(A, line), f.GetConstantExpression(B), r.GetKExpression54(C, code.kField(line), line), true, line));
                    break;

                case Op.OpT.SETTABUP:
                    operations.Add(new TableSet(line, upvalues.getExpression(A), r.GetKExpression(B, line), r.GetKExpression(C, line), true, line));
                    break;

                case Op.OpT.SETTABUP54:
                    operations.Add(new TableSet(line, upvalues.getExpression(A), f.GetConstantExpression(B), r.GetKExpression54(C, code.kField(line), line), true, line));
                    break;

                case Op.OpT.NEWTABLE50:
                    operations.Add(new RegisterSet(line, A, new TableLiteral(fb2int50(B), C == 0 ? 0 : 1 << C)));
                    break;

                case Op.OpT.NEWTABLE:
                    operations.Add(new RegisterSet(line, A, new TableLiteral(fb2int(B), fb2int(C))));
                    break;

                case Op.OpT.NEWTABLE54:
                {
                    if (code.GetOp(line + 1) != Op.EXTRAARG) throw new System.InvalidOperationException();
                    int arraySize = C;
                    if (code.kField(line))
                    {
                        arraySize += code.AxField(line + 1) * (code.getExtractor().C.max() + 1);
                    }
                    operations.Add(new RegisterSet(line, A, new TableLiteral(arraySize, B == 0 ? 0 : (1 << (B - 1)))));
                    break;
                }
                case Op.OpT.SELF:
                {
                    // We can later determine if : syntax was used by comparing subexpressions with ==
                    IExpression common = r.GetExpression(B, line);
                    operations.Add(new RegisterSet(line, A + 1, common));
                    operations.Add(new RegisterSet(line, A, new TableReference(common, r.GetKExpression(C, line))));
                    break;
                }
                case Op.OpT.SELF54:
                {
                    // We can later determine if : syntax was used by comparing subexpressions with ==
                    IExpression common = r.GetExpression(B, line);
                    operations.Add(new RegisterSet(line, A + 1, common));
                    operations.Add(new RegisterSet(line, A, new TableReference(common, r.GetKExpression54(C, code.kField(line), line))));
                    break;
                }
                case Op.OpT.ADD:
                    handle50BinOp(operations, state, line, IExpression.BinaryOperation.ADD);
                    break;

                case Op.OpT.SUB:
                    handle50BinOp(operations, state, line, IExpression.BinaryOperation.SUB);
                    break;

                case Op.OpT.MUL:
                    handle50BinOp(operations, state, line, IExpression.BinaryOperation.MUL);
                    break;

                case Op.OpT.DIV:
                    handle50BinOp(operations, state, line, IExpression.BinaryOperation.DIV);
                    break;

                case Op.OpT.IDIV:
                    handle50BinOp(operations, state, line, IExpression.BinaryOperation.IDIV);
                    break;

                case Op.OpT.MOD:
                    handle50BinOp(operations, state, line, IExpression.BinaryOperation.MOD);
                    break;

                case Op.OpT.POW:
                    handle50BinOp(operations, state, line, IExpression.BinaryOperation.POW);
                    break;

                case Op.OpT.BAND:
                    handle50BinOp(operations, state, line, IExpression.BinaryOperation.BAND);
                    break;

                case Op.OpT.BOR:
                    handle50BinOp(operations, state, line, IExpression.BinaryOperation.BOR);
                    break;

                case Op.OpT.BXOR:
                    handle50BinOp(operations, state, line, IExpression.BinaryOperation.BXOR);
                    break;

                case Op.OpT.SHL:
                    handle50BinOp(operations, state, line, IExpression.BinaryOperation.SHL);
                    break;

                case Op.OpT.SHR:
                    handle50BinOp(operations, state, line, IExpression.BinaryOperation.SHR);
                    break;

                case Op.OpT.ADD54:
                    handle54BinOp(operations, state, line, IExpression.BinaryOperation.ADD);
                    break;

                case Op.OpT.SUB54:
                    handle54BinOp(operations, state, line, IExpression.BinaryOperation.SUB);
                    break;

                case Op.OpT.MUL54:
                    handle54BinOp(operations, state, line, IExpression.BinaryOperation.MUL);
                    break;

                case Op.OpT.DIV54:
                    handle54BinOp(operations, state, line, IExpression.BinaryOperation.DIV);
                    break;

                case Op.OpT.IDIV54:
                    handle54BinOp(operations, state, line, IExpression.BinaryOperation.IDIV);
                    break;

                case Op.OpT.MOD54:
                    handle54BinOp(operations, state, line, IExpression.BinaryOperation.MOD);
                    break;

                case Op.OpT.POW54:
                    handle54BinOp(operations, state, line, IExpression.BinaryOperation.POW);
                    break;

                case Op.OpT.BAND54:
                    handle54BinOp(operations, state, line, IExpression.BinaryOperation.BAND);
                    break;

                case Op.OpT.BOR54:
                    handle54BinOp(operations, state, line, IExpression.BinaryOperation.BOR);
                    break;

                case Op.OpT.BXOR54:
                    handle54BinOp(operations, state, line, IExpression.BinaryOperation.BXOR);
                    break;

                case Op.OpT.SHL54:
                    handle54BinOp(operations, state, line, IExpression.BinaryOperation.SHL);
                    break;

                case Op.OpT.SHR54:
                    handle54BinOp(operations, state, line, IExpression.BinaryOperation.SHR);
                    break;

                case Op.OpT.ADDI:
                {
                    if (line + 1 > code.Length || code.GetOp(line + 1) != Op.MMBINI) throw new System.InvalidOperationException();
                    IExpression.BinaryOperation op = decodeBinOp(code.CField(line + 1));
                    int immediate = code.sCField(line);
                    bool swap = false;
                    if (code.kField(line + 1))
                    {
                        if (op != IExpression.BinaryOperation.ADD)
                        {
                            throw new System.InvalidOperationException();
                        }
                        swap = true;
                    }
                    else
                    {
                        if (op == IExpression.BinaryOperation.ADD)
                        {
                            // do nothing
                        }
                        else if (op == IExpression.BinaryOperation.SUB)
                        {
                            immediate = -immediate;
                        }
                        else
                        {
                            throw new System.InvalidOperationException();
                        }
                    }
                    IExpression left = r.GetExpression(B, line);
                    IExpression right = ConstantExpression.createint(immediate);
                    if (swap)
                    {
                        IExpression temp = left;
                        left = right;
                        right = temp;
                    }
                    operations.Add(new RegisterSet(line, A, IExpression.make(op, left, right)));
                    break;
                }
                case Op.OpT.ADDK:
                    handle54BinKOp(operations, state, line, IExpression.BinaryOperation.ADD);
                    break;

                case Op.OpT.SUBK:
                    handle54BinKOp(operations, state, line, IExpression.BinaryOperation.SUB);
                    break;

                case Op.OpT.MULK:
                    handle54BinKOp(operations, state, line, IExpression.BinaryOperation.MUL);
                    break;

                case Op.OpT.DIVK:
                    handle54BinKOp(operations, state, line, IExpression.BinaryOperation.DIV);
                    break;

                case Op.OpT.IDIVK:
                    handle54BinKOp(operations, state, line, IExpression.BinaryOperation.IDIV);
                    break;

                case Op.OpT.MODK:
                    handle54BinKOp(operations, state, line, IExpression.BinaryOperation.MOD);
                    break;

                case Op.OpT.POWK:
                    handle54BinKOp(operations, state, line, IExpression.BinaryOperation.POW);
                    break;

                case Op.OpT.BANDK:
                    handle54BinKOp(operations, state, line, IExpression.BinaryOperation.BAND);
                    break;

                case Op.OpT.BORK:
                    handle54BinKOp(operations, state, line, IExpression.BinaryOperation.BOR);
                    break;

                case Op.OpT.BXORK:
                    handle54BinKOp(operations, state, line, IExpression.BinaryOperation.BXOR);
                    break;

                case Op.OpT.SHRI:
                {
                    if (line + 1 > code.Length || code.GetOp(line + 1) != Op.MMBINI) throw new System.InvalidOperationException();
                    int immediate = code.sCField(line);
                    IExpression.BinaryOperation op = decodeBinOp(code.CField(line + 1));
                    if (op == IExpression.BinaryOperation.SHR)
                    {
                        // okay
                    }
                    else if (op == IExpression.BinaryOperation.SHL)
                    {
                        immediate = -immediate;
                    }
                    else
                    {
                        throw new System.InvalidOperationException();
                    }
                    operations.Add(new RegisterSet(line, A, IExpression.make(op, r.GetExpression(B, line), ConstantExpression.createint(immediate))));
                    break;
                }
                case Op.OpT.SHLI:
                {
                    operations.Add(new RegisterSet(line, A, IExpression.make(IExpression.BinaryOperation.SHL, ConstantExpression.createint(code.sCField(line)), r.GetExpression(B, line))));
                    break;
                }
                case Op.OpT.MMBIN:
                case Op.OpT.MMBINI:
                case Op.OpT.MMBINK:
                    /* Do nothing ... handled with preceding operation. */
                    break;

                case Op.OpT.UNM:
                    handleUnaryOp(operations, state, line, IExpression.UnaryOperation.UNM);
                    break;

                case Op.OpT.NOT:
                    handleUnaryOp(operations, state, line, IExpression.UnaryOperation.NOT);
                    break;

                case Op.OpT.LEN:
                    handleUnaryOp(operations, state, line, IExpression.UnaryOperation.LEN);
                    break;

                case Op.OpT.BNOT:
                    handleUnaryOp(operations, state, line, IExpression.UnaryOperation.BNOT);
                    break;

                case Op.OpT.CONCAT:
                {
                    IExpression value = r.GetExpression(C, line);
                    //Remember that CONCAT is right associative.
                    while (C-- > B)
                    {
                        value = IExpression.make(IExpression.BinaryOperation.CONCAT, r.GetExpression(C, line), value);
                    }
                    operations.Add(new RegisterSet(line, A, value));
                    break;
                }
                case Op.OpT.CONCAT54:
                {
                    if (B < 2) throw new System.InvalidOperationException();
                    B--;
                    IExpression value = r.GetExpression(A + B, line);
                    while (B-- > 0)
                    {
                        value = IExpression.make(IExpression.BinaryOperation.CONCAT, r.GetExpression(A + B, line), value);
                    }
                    operations.Add(new RegisterSet(line, A, value));
                    break;
                }
                case Op.OpT.JMP:
                case Op.OpT.JMP52:
                case Op.OpT.JMP54:
                case Op.OpT.EQ:
                case Op.OpT.LT:
                case Op.OpT.LE:
                case Op.OpT.EQ54:
                case Op.OpT.LT54:
                case Op.OpT.LE54:
                case Op.OpT.EQK:
                case Op.OpT.EQI:
                case Op.OpT.LTI:
                case Op.OpT.LEI:
                case Op.OpT.GTI:
                case Op.OpT.GEI:
                case Op.OpT.TEST:
                case Op.OpT.TEST54:
                    /* Do nothing ... handled with branches */
                    break;

                case Op.OpT.TEST50:
                {
                    if (getNoDebug() && A != B)
                    {
                        operations.Add(new RegisterSet(line, A, IExpression.make(IExpression.BinaryOperation.OR, r.GetExpression(B, line), initialExpression(state, A, line))));
                    }
                    break;
                }
                case Op.OpT.TESTSET:
                case Op.OpT.TESTSET54:
                {
                    if (getNoDebug())
                    {
                        operations.Add(new RegisterSet(line, A, IExpression.make(IExpression.BinaryOperation.OR, r.GetExpression(B, line), initialExpression(state, A, line))));
                    }
                    break;
                }
                case Op.OpT.CALL:
                {
                    bool multiple = (C >= 3 || C == 0);
                    if (B == 0) B = registers - A;
                    if (C == 0) C = registers - A + 1;
                    IExpression function = r.GetExpression(A, line);
                    IExpression[] arguments = new IExpression[B - 1];
                    for (int register = A + 1; register <= A + B - 1; register++)
                    {
                        arguments[register - A - 1] = r.GetExpression(register, line);
                    }
                    FunctionCall value = new FunctionCall(function, arguments, multiple);
                    if (C == 1)
                    {
                        operations.Add(new CallOperation(line, value));
                    }
                    else
                    {
                        if (C == 2 && !multiple)
                        {
                            operations.Add(new RegisterSet(line, A, value));
                        }
                        else
                        {
                            operations.Add(new MultipleRegisterSet(line, A, A + C - 2, value));
                        }
                    }
                    break;
                }
                case Op.OpT.TAILCALL:
                case Op.OpT.TAILCALL54:
                {
                    if (B == 0) B = registers - A;
                    IExpression function = r.GetExpression(A, line);
                    IExpression[] arguments = new IExpression[B - 1];
                    for (int register = A + 1; register <= A + B - 1; register++)
                    {
                        arguments[register - A - 1] = r.GetExpression(register, line);
                    }
                    FunctionCall value = new FunctionCall(function, arguments, true);
                    operations.Add(new ReturnOperation(line, value));
                    skip[line + 1] = true;
                    break;
                }
                case Op.OpT.RETURN:
                case Op.OpT.RETURN54:
                {
                    if (B == 0) B = registers - A + 1;
                    IExpression[] values = new IExpression[B - 1];
                    for (int register = A; register <= A + B - 2; register++)
                    {
                        values[register - A] = r.GetExpression(register, line);
                    }
                    operations.Add(new ReturnOperation(line, values));
                    break;
                }
                case Op.OpT.RETURN0:
                    operations.Add(new ReturnOperation(line, new IExpression[0]));
                    break;

                case Op.OpT.RETURN1:
                    operations.Add(new ReturnOperation(line, new IExpression[] { r.GetExpression(A, line) }));
                    break;

                case Op.OpT.FORLOOP:
                case Op.OpT.FORLOOP54:
                case Op.OpT.FORPREP:
                case Op.OpT.FORPREP54:
                case Op.OpT.TFORPREP:
                case Op.OpT.TFORPREP54:
                case Op.OpT.TFORCALL:
                case Op.OpT.TFORCALL54:
                case Op.OpT.TFORLOOP:
                case Op.OpT.TFORLOOP52:
                case Op.OpT.TFORLOOP54:
                    /* Do nothing ... handled with branches */
                    break;

                case Op.OpT.SETLIST50:
                {
                    handleSetList(operations, state, line, A, 1 + Bx % 32, Bx - Bx % 32);
                    break;
                }
                case Op.OpT.SETLISTO:
                {
                    handleSetList(operations, state, line, A, registers - A - 1, Bx - Bx % 32);
                    break;
                }
                case Op.OpT.SETLIST:
                {
                    if (C == 0)
                    {
                        C = code.codepoint(line + 1);
                        skip[line + 1] = true;
                    }
                    if (B == 0)
                    {
                        B = registers - A - 1;
                    }
                    handleSetList(operations, state, line, A, B, (C - 1) * 50);
                    break;
                }
                case Op.OpT.SETLIST52:
                {
                    if (C == 0)
                    {
                        if (line + 1 > code.Length || code.GetOp(line + 1) != Op.EXTRAARG) throw new System.InvalidOperationException();
                        C = code.AxField(line + 1);
                        skip[line + 1] = true;
                    }
                    if (B == 0)
                    {
                        B = registers - A - 1;
                    }
                    handleSetList(operations, state, line, A, B, (C - 1) * 50);
                    break;
                }
                case Op.OpT.SETLIST54:
                {
                    if (code.kField(line))
                    {
                        if (line + 1 > code.Length || code.GetOp(line + 1) != Op.EXTRAARG) throw new System.InvalidOperationException();
                        C += code.AxField(line + 1) * (code.getExtractor().C.max() + 1);
                        skip[line + 1] = true;
                    }
                    if (B == 0)
                    {
                        B = registers - A - 1;
                    }
                    handleSetList(operations, state, line, A, B, C);
                    break;
                }
                case Op.OpT.TBC:
                    r.GetDeclaration(A, line).tbc = true;
                    break;

                case Op.OpT.CLOSE:
                    break;

                case Op.OpT.CLOSURE:
                {
                    LFunction f = functions[Bx];
                    operations.Add(new RegisterSet(line, A, new ClosureExpression(f, line + 1)));
                    if (function.header.version.upvalueDeclarationType.Value == Version.UpvalueDeclarationType.Inline)
                    {
                        // Handle upvalue declarations
                        for (int i = 0; i < f.numUpvalues; i++)
                        {
                            LUpvalue upvalue = f.upvalues[i];
                            switch (code.GetOp(line + 1 + i).Type)
                            {
                                case Op.OpT.MOVE:
                                    upvalue.instack = true;
                                    break;

                                case Op.OpT.GETUPVALUE:
                                    upvalue.instack = false;
                                    break;

                                default:
                                    throw new System.InvalidOperationException();
                            }
                            upvalue.idx = code.BField(line + 1 + i);
                            skip[line + 1 + i] = true;
                        }
                    }
                    break;
                }
                case Op.OpT.VARARGPREP:
                    /* Do nothing ... internal operation */
                    break;

                case Op.OpT.VARARG:
                {
                    bool multiple = (B != 2);
                    if (B == 1) throw new System.InvalidOperationException();
                    if (B == 0) B = registers - A + 1;
                    IExpression value = new Vararg(B - 1, multiple);
                    operations.Add(new MultipleRegisterSet(line, A, A + B - 2, value));
                    break;
                }
                case Op.OpT.VARARG54:
                {
                    bool multiple = (C != 2);
                    if (C == 1) throw new System.InvalidOperationException();
                    if (C == 0) C = registers - A + 1;
                    IExpression value = new Vararg(C - 1, multiple);
                    operations.Add(new MultipleRegisterSet(line, A, A + C - 2, value));
                    break;
                }
                case Op.OpT.EXTRAARG:
                case Op.OpT.EXTRABYTE:
                    /* Do nothing ... handled by previous instruction */
                    break;

                case Op.OpT.DEFAULT:
                case Op.OpT.DEFAULT54:
                    throw new System.InvalidOperationException();
            }
            return operations;
        }

        private Assignment processOperation(State state, IOperation operation, int line, int nextLine, IBlock block)
        {
            Registers r = state.r;
            bool[] skip = state.skip;
            Assignment assign = null;
            List<IStatement> stmts = operation.process(r, block);
            if (stmts.Count == 1)
            {
                IStatement stmt = stmts[0];
                if (stmt is Assignment)
                {
                    assign = (Assignment)stmt;
                }
                //System.output.println("-- added statemtent @" + line);
                if (assign != null)
                {
                    bool declare = false;
                    foreach (Declaration newLocal in r.GetNewLocals(line, block.closeRegister))
                    {
                        if (assign.getFirstTarget().isDeclaration(newLocal))
                        {
                            declare = true;
                            break;
                        }
                    }
                    //System.output.println("-- checking for multiassign @" + nextLine);
                    while (!declare && nextLine < block.end)
                    {
                        Op op = code.GetOp(nextLine);
                        if (isMoveIntoTarget(r, nextLine))
                        {
                            //System.output.println("-- found multiassign @" + nextLine);
                            ITarget target = getMoveIntoTargetTarget(r, nextLine, line + 1);
                            IExpression value = getMoveIntoTargetValue(r, nextLine, line + 1); //updated?
                            assign.addFirst(target, value, nextLine);
                            skip[nextLine] = true;
                            nextLine++;
                        }
                        else if (op == Op.MMBIN || op == Op.MMBINI || op == Op.MMBINK || code.isUpvalueDeclaration(nextLine))
                        {
                            // skip
                            nextLine++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            foreach (IStatement stmt in stmts)
            {
                block.addStatement(stmt);
            }
            return assign;
        }

        private void processSequence(State state, List<IBlock> blocks, int begin, int end)
        {
            Registers r = state.r;
            int blockContainerIndex = 0;
            int blockStatementIndex = 0;
            List<IBlock> blockContainers = new List<IBlock>(blocks.Count);
            List<IBlock> blockStatements = new List<IBlock>(blocks.Count);
            foreach (IBlock block in blocks)
            {
                if (block.isContainer())
                {
                    blockContainers.Add(block);
                }
                else
                {
                    blockStatements.Add(block);
                }
            }
            Stack<IBlock> blockStack = new Stack<IBlock>();
            blockStack.Push(blockContainers[blockContainerIndex++]);

            state.skip = new bool[code.Length + 1];
            bool[] skip = state.skip;
            bool[] labels_handled = new bool[code.Length + 1];

            int line = 1;
            while (true)
            {
                int nextline = line;
                List<IOperation> operations = null;
                List<Declaration> prevLocals = null;
                List<Declaration> newLocals = null;

                // Handle container blocks
                if (blockStack.Peek().end <= line)
                {
                    IBlock endingBlock = blockStack.Pop();
                    IOperation operation = endingBlock.process(this);
                    if (blockStack.Count == 0) return;
                    if (operation == null) throw new System.InvalidOperationException();
                    operations = new List<IOperation> { operation };
                    prevLocals = r.GetNewLocals(line - 1);
                }
                else
                {
                    if (!labels_handled[line] && state.labels[line])
                    {
                        blockStack.Peek().addStatement(new Label(line));
                        labels_handled[line] = true;
                    }

                    List<Declaration> rLocals = r.GetNewLocals(line, blockStack.Peek().closeRegister);
                    while (blockContainerIndex < blockContainers.Count && blockContainers[blockContainerIndex].begin <= line)
                    {
                        IBlock next = blockContainers[blockContainerIndex++];
                        if (rLocals.Count != 0 && next.allowsPreDeclare() &&
                          (rLocals[0].end > next.scopeEnd() || rLocals[0].register < next.closeRegister)
                        )
                        {
                            Assignment declaration = new Assignment();
                            int declareEnd = rLocals[0].end;
                            declaration.SetDeclare(rLocals[0].begin);
                            while (rLocals.Count != 0 && rLocals[0].end == declareEnd && (next.closeRegister == -1 || rLocals[0].register < next.closeRegister))
                            {
                                Declaration decl = rLocals[0];
                                declaration.addLast(new VariableTarget(decl), ConstantExpression.createNil(line), line);
                                rLocals.RemoveAt(0);
                            }
                            blockStack.Peek().addStatement(declaration);
                        }
                        blockStack.Push(next);
                    }
                }

                IBlock block = blockStack.Peek();

                r.StartLine(line);

                // Handle other sources of operations (after pushing any new container block)
                if (operations == null)
                {
                    if (blockStatementIndex < blockStatements.Count && blockStatements[blockStatementIndex].begin <= line)
                    {
                        IBlock blockStatement = blockStatements[blockStatementIndex++];
                        IOperation operation = blockStatement.process(this);
                        operations = new List<IOperation> { operation };
                    }
                    else
                    {
                        // After all blocks are handled for a line, we will reach here
                        nextline = line + 1;
                        if (!skip[line] && line >= begin && line <= end)
                        {
                            operations = processLine(state, line);
                        }
                        else
                        {
                            operations = new List<IOperation>();
                        }
                        if (line >= begin && line <= end)
                        {
                            newLocals = r.GetNewLocals(line, block.closeRegister);
                        }
                    }
                }

                // Need to capture the assignment (if any) to attach local variable declarations
                Assignment assignment = null;

                foreach (IOperation operation in operations)
                {
                    Assignment operationAssignment = processOperation(state, operation, line, nextline, block);
                    if (operationAssignment != null)
                    {
                        assignment = operationAssignment;
                    }
                }

                // Some declarations may be swallowed by assignment blocks.
                // These are restored via prevLocals
                List<Declaration> locals = newLocals;
                if (assignment != null && prevLocals != null)
                {
                    locals = prevLocals;
                }
                if (locals != null && locals.Count != 0)
                {
                    int scopeEnd = -1;
                    if (assignment == null)
                    {
                        // Create a new Assignment to hold the declarations
                        assignment = new Assignment();
                        block.addStatement(assignment);
                    }
                    else
                    {
                        foreach (Declaration decl in locals)
                        {
                            if (assignment.assigns(decl))
                            {
                                scopeEnd = decl.end;
                                break;
                            }
                        }
                    }

                    assignment.SetDeclare(locals[0].begin);
                    foreach (Declaration decl in locals)
                    {
                        if ((scopeEnd == -1 || decl.end == scopeEnd) && decl.register >= block.closeRegister)
                        {
                            assignment.addLast(new VariableTarget(decl), r.GetValue(decl.register, line + 1), r.GetUpdated(decl.register, line - 1));
                        }
                    }
                }

                line = nextline;
            }
        }

        public State decompile()
        {
            State state = new State();
            state.r = new Registers(registers, length, declarations, f, getNoDebug());
            ControlFlowHandler.Result result = ControlFlowHandler.Process(this, state.r);
            List<IBlock> blocks = result.blocks;
            state.outer = blocks[0];
            state.labels = result.labels;
            processSequence(state, blocks, 1, code.Length);
            foreach (IBlock block in blocks)
            {
                block.resolve(state.r);
            }
            handleUnusedConstants(state.outer);
            return state;
        }

        public Configuration getConfiguration()
        {
            return function.header.config;
        }

        public bool getNoDebug()
        {
            return function.header.config.Variable == Configuration.VariableMode.Nodebug ||
                function.stripped && function.header.config.Variable == Configuration.VariableMode.Default;
        }

        public Version getVersion()
        {
            return function.header.version;
        }

        public bool hasStatement(int begin, int end)
        {
            if (begin <= end)
            {
                State state = new State();
                state.r = new Registers(registers, length, declarations, f, getNoDebug());
                state.outer = new OuterBlock(function, code.Length);
                IBlock scoped = new DoEndBlock(function, begin, end + 1);
                state.labels = new bool[code.Length + 1];
                List<IBlock> blocks = new List<IBlock> { state.outer, scoped };
                processSequence(state, blocks, 1, code.Length);
                return !scoped.isEmpty();
            }
            else
            {
                return false;
            }
        }

        public void print(State state)
        {
            print(state, new Output());
        }

        public void print(State state, IOutputProvider output)
        {
            print(state, new Output(output));
        }

        public void print(State state, Output output)
        {
            handleInitialDeclares(output);
            state.outer.print(this, output);
        }

        /**
         * Decodes values from the Lua TMS enumeration used for the MMBIN family of operations.
         */
    }
}