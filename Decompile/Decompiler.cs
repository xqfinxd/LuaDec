using LuaDec.Decompile.Block;
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
                if (expression.IsConstant())
                {
                    int index = expression.GetConstantIndex();
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
                if (expression.IsConstant())
                {
                    int index = expression.GetConstantIndex();
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
                    if (statement.UseConstant(f, nextConstant))
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
            if (function.stripped || GetConfiguration().Variable == Configuration.VariableMode.Nodebug)
            {
                if (GetConfiguration().Variable == Configuration.VariableMode.Finder)
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
                    if (GetVersion().varArgtTpe.Value != Version.VarArgType.Ellipsis && (function.varArg & 1) != 0 && i < function.maximumStackSize)
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

        private IExpression.BinaryOperation DecodeBinOp(int tm)
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

        private int FB2Int(int fb)
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

        private int FB2Int50(int fb)
        {
            return (fb & 7) << (fb >> 3);
        }

        private ITarget GetMoveIntoTargetTarget(Registers r, int line, int previous)
        {
            switch (code.GetOp(line).Type)
            {
                case Op.OpT.MOVE:
                    return r.GetTarget(code.AField(line), line);

                case Op.OpT.SETUPVALUE:
                    return new UpvalueTarget(upvalues.GetName(code.BField(line)));

                case Op.OpT.SETGLOBAL:
                    return new GlobalTarget(f.GetGlobalName(code.BxField(line)));

                case Op.OpT.SETTABLE:
                    return new TableTarget(r.GetExpression(code.AField(line), previous), r.GetKExpression(code.BField(line), previous));

                case Op.OpT.SETTABLE54:
                    return new TableTarget(r.GetExpression(code.AField(line), previous), r.GetExpression(code.BField(line), previous));

                case Op.OpT.SETI:
                    return new TableTarget(r.GetExpression(code.AField(line), previous), ConstantExpression.CreateInt(code.BField(line)));

                case Op.OpT.SETFIELD:
                    return new TableTarget(r.GetExpression(code.AField(line), previous), f.GetConstantExpression(code.BField(line)));

                case Op.OpT.SETTABUP:
                {
                    int A = code.AField(line);
                    int B = code.BField(line);
                    return new TableTarget(upvalues.GetExpression(A), r.GetKExpression(B, previous));
                }
                case Op.OpT.SETTABUP54:
                {
                    int A = code.AField(line);
                    int B = code.BField(line);
                    return new TableTarget(upvalues.GetExpression(A), f.GetConstantExpression(B));
                }
                default:
                    throw new System.InvalidOperationException();
            }
        }

        private IExpression GetMoveIntoTargetValue(Registers r, int line, int previous)
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

        private void Handle50BinOp(List<IOperation> operations, State state, int line, IExpression.BinaryOperation op)
        {
            operations.Add(new RegisterSet(line, code.AField(line), IExpression.Make(op, state.r.GetKExpression(code.BField(line), line), state.r.GetKExpression(code.CField(line), line))));
        }

        private void Handle54BinKOp(List<IOperation> operations, State state, int line, IExpression.BinaryOperation op)
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
            operations.Add(new RegisterSet(line, code.AField(line), IExpression.Make(op, left, right)));
        }

        private void Handle54BinOp(List<IOperation> operations, State state, int line, IExpression.BinaryOperation op)
        {
            operations.Add(new RegisterSet(line, code.AField(line), IExpression.Make(op, state.r.GetExpression(code.BField(line), line), state.r.GetExpression(code.CField(line), line))));
        }

        private void HandleInitialDeclares(Output output)
        {
            List<Declaration> initdecls = new List<Declaration>(declarations.Length);
            int initdeclcount = paramNum;
            switch (GetVersion().varArgtTpe.Value)
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
                output.Write("local ");
                output.Write(initdecls[0].name);
                for (int i = 1; i < initdecls.Count; i++)
                {
                    output.Write(", ");
                    output.Write(initdecls[i].name);
                }
                output.WriteLine();
            }
        }

        private void HandleSetList(List<IOperation> operations, State state, int line, int stack, int count, int offset)
        {
            IExpression table = state.r.GetValue(stack, line);
            for (int i = 1; i <= count; i++)
            {
                operations.Add(new TableSet(line, table, ConstantExpression.CreateInt(offset + i), state.r.GetExpression(stack + i, line), false, state.r.GetUpdated(stack + i, line)));
            }
        }

        private void HandleUnaryOp(List<IOperation> operations, State state, int line, IExpression.UnaryOperation op)
        {
            operations.Add(new RegisterSet(line, code.AField(line), IExpression.Make(op, state.r.GetExpression(code.BField(line), line))));
        }

        private void HandleUnusedConstants(IBlock outer)
        {
            HashSet<int> unusedConstants = new HashSet<int>();
            outer.Walk(new ConstantWalker(unusedConstants));
            outer.Walk(new FunctionConstantWalker(unusedConstants, f));
        }

        private IExpression InitialExpression(State state, int register, int line)
        {
            if (line == 1)
            {
                if (register < function.numParams) throw new System.InvalidOperationException();
                return ConstantExpression.CreateNil(line);
            }
            else
            {
                return state.r.GetExpression(register, line - 1);
            }
        }

        private bool IsMoveIntoTarget(Registers r, int line)
        {
            if (code.IsUpvalueDeclaration(line)) return false;
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

        private List<IOperation> ProcessLine(State state, int line)
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
                    operations.Add(new RegisterSet(line, A, ConstantExpression.CreateInt(code.sBxField(line))));
                    break;

                case Op.OpT.LOADF:
                    operations.Add(new RegisterSet(line, A, ConstantExpression.CreateDouble((double)code.sBxField(line))));
                    break;

                case Op.OpT.LOADK:
                    operations.Add(new RegisterSet(line, A, f.GetConstantExpression(Bx)));
                    break;

                case Op.OpT.LOADKX:
                    if (line + 1 > code.Length || code.GetOp(line + 1) != Op.EXTRAARG) throw new System.InvalidOperationException();
                    operations.Add(new RegisterSet(line, A, f.GetConstantExpression(code.AxField(line + 1))));
                    break;

                case Op.OpT.LOADBOOL:
                    operations.Add(new RegisterSet(line, A, ConstantExpression.CreateBool(B != 0)));
                    break;

                case Op.OpT.LOADFALSE:
                case Op.OpT.LFALSESKIP:
                    operations.Add(new RegisterSet(line, A, ConstantExpression.CreateBool(false)));
                    break;

                case Op.OpT.LOADTRUE:
                    operations.Add(new RegisterSet(line, A, ConstantExpression.CreateBool(true)));
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
                    operations.Add(new RegisterSet(line, A, upvalues.GetExpression(B)));
                    break;

                case Op.OpT.SETUPVALUE:
                    operations.Add(new UpvalueSet(line, upvalues.GetName(B), r.GetExpression(A, line)));
                    break;

                case Op.OpT.GETTABUP:
                    operations.Add(new RegisterSet(line, A, new TableReference(upvalues.GetExpression(B), r.GetKExpression(C, line))));
                    break;

                case Op.OpT.GETTABUP54:
                    operations.Add(new RegisterSet(line, A, new TableReference(upvalues.GetExpression(B), f.GetConstantExpression(C))));
                    break;

                case Op.OpT.GETTABLE:
                    operations.Add(new RegisterSet(line, A, new TableReference(r.GetExpression(B, line), r.GetKExpression(C, line))));
                    break;

                case Op.OpT.GETTABLE54:
                    operations.Add(new RegisterSet(line, A, new TableReference(r.GetExpression(B, line), r.GetExpression(C, line))));
                    break;

                case Op.OpT.GETI:
                    operations.Add(new RegisterSet(line, A, new TableReference(r.GetExpression(B, line), ConstantExpression.CreateInt(C))));
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
                    operations.Add(new TableSet(line, r.GetExpression(A, line), ConstantExpression.CreateInt(B), r.GetKExpression54(C, code.kField(line), line), true, line));
                    break;

                case Op.OpT.SETFIELD:
                    operations.Add(new TableSet(line, r.GetExpression(A, line), f.GetConstantExpression(B), r.GetKExpression54(C, code.kField(line), line), true, line));
                    break;

                case Op.OpT.SETTABUP:
                    operations.Add(new TableSet(line, upvalues.GetExpression(A), r.GetKExpression(B, line), r.GetKExpression(C, line), true, line));
                    break;

                case Op.OpT.SETTABUP54:
                    operations.Add(new TableSet(line, upvalues.GetExpression(A), f.GetConstantExpression(B), r.GetKExpression54(C, code.kField(line), line), true, line));
                    break;

                case Op.OpT.NEWTABLE50:
                    operations.Add(new RegisterSet(line, A, new TableLiteral(FB2Int50(B), C == 0 ? 0 : 1 << C)));
                    break;

                case Op.OpT.NEWTABLE:
                    operations.Add(new RegisterSet(line, A, new TableLiteral(FB2Int(B), FB2Int(C))));
                    break;

                case Op.OpT.NEWTABLE54:
                {
                    if (code.GetOp(line + 1) != Op.EXTRAARG) throw new System.InvalidOperationException();
                    int arraySize = C;
                    if (code.kField(line))
                    {
                        arraySize += code.AxField(line + 1) * (code.GetExtractor().C.Max() + 1);
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
                    Handle50BinOp(operations, state, line, IExpression.BinaryOperation.ADD);
                    break;

                case Op.OpT.SUB:
                    Handle50BinOp(operations, state, line, IExpression.BinaryOperation.SUB);
                    break;

                case Op.OpT.MUL:
                    Handle50BinOp(operations, state, line, IExpression.BinaryOperation.MUL);
                    break;

                case Op.OpT.DIV:
                    Handle50BinOp(operations, state, line, IExpression.BinaryOperation.DIV);
                    break;

                case Op.OpT.IDIV:
                    Handle50BinOp(operations, state, line, IExpression.BinaryOperation.IDIV);
                    break;

                case Op.OpT.MOD:
                    Handle50BinOp(operations, state, line, IExpression.BinaryOperation.MOD);
                    break;

                case Op.OpT.POW:
                    Handle50BinOp(operations, state, line, IExpression.BinaryOperation.POW);
                    break;

                case Op.OpT.BAND:
                    Handle50BinOp(operations, state, line, IExpression.BinaryOperation.BAND);
                    break;

                case Op.OpT.BOR:
                    Handle50BinOp(operations, state, line, IExpression.BinaryOperation.BOR);
                    break;

                case Op.OpT.BXOR:
                    Handle50BinOp(operations, state, line, IExpression.BinaryOperation.BXOR);
                    break;

                case Op.OpT.SHL:
                    Handle50BinOp(operations, state, line, IExpression.BinaryOperation.SHL);
                    break;

                case Op.OpT.SHR:
                    Handle50BinOp(operations, state, line, IExpression.BinaryOperation.SHR);
                    break;

                case Op.OpT.ADD54:
                    Handle54BinOp(operations, state, line, IExpression.BinaryOperation.ADD);
                    break;

                case Op.OpT.SUB54:
                    Handle54BinOp(operations, state, line, IExpression.BinaryOperation.SUB);
                    break;

                case Op.OpT.MUL54:
                    Handle54BinOp(operations, state, line, IExpression.BinaryOperation.MUL);
                    break;

                case Op.OpT.DIV54:
                    Handle54BinOp(operations, state, line, IExpression.BinaryOperation.DIV);
                    break;

                case Op.OpT.IDIV54:
                    Handle54BinOp(operations, state, line, IExpression.BinaryOperation.IDIV);
                    break;

                case Op.OpT.MOD54:
                    Handle54BinOp(operations, state, line, IExpression.BinaryOperation.MOD);
                    break;

                case Op.OpT.POW54:
                    Handle54BinOp(operations, state, line, IExpression.BinaryOperation.POW);
                    break;

                case Op.OpT.BAND54:
                    Handle54BinOp(operations, state, line, IExpression.BinaryOperation.BAND);
                    break;

                case Op.OpT.BOR54:
                    Handle54BinOp(operations, state, line, IExpression.BinaryOperation.BOR);
                    break;

                case Op.OpT.BXOR54:
                    Handle54BinOp(operations, state, line, IExpression.BinaryOperation.BXOR);
                    break;

                case Op.OpT.SHL54:
                    Handle54BinOp(operations, state, line, IExpression.BinaryOperation.SHL);
                    break;

                case Op.OpT.SHR54:
                    Handle54BinOp(operations, state, line, IExpression.BinaryOperation.SHR);
                    break;

                case Op.OpT.ADDI:
                {
                    if (line + 1 > code.Length || code.GetOp(line + 1) != Op.MMBINI) throw new System.InvalidOperationException();
                    IExpression.BinaryOperation op = DecodeBinOp(code.CField(line + 1));
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
                    IExpression right = ConstantExpression.CreateInt(immediate);
                    if (swap)
                    {
                        IExpression temp = left;
                        left = right;
                        right = temp;
                    }
                    operations.Add(new RegisterSet(line, A, IExpression.Make(op, left, right)));
                    break;
                }
                case Op.OpT.ADDK:
                    Handle54BinKOp(operations, state, line, IExpression.BinaryOperation.ADD);
                    break;

                case Op.OpT.SUBK:
                    Handle54BinKOp(operations, state, line, IExpression.BinaryOperation.SUB);
                    break;

                case Op.OpT.MULK:
                    Handle54BinKOp(operations, state, line, IExpression.BinaryOperation.MUL);
                    break;

                case Op.OpT.DIVK:
                    Handle54BinKOp(operations, state, line, IExpression.BinaryOperation.DIV);
                    break;

                case Op.OpT.IDIVK:
                    Handle54BinKOp(operations, state, line, IExpression.BinaryOperation.IDIV);
                    break;

                case Op.OpT.MODK:
                    Handle54BinKOp(operations, state, line, IExpression.BinaryOperation.MOD);
                    break;

                case Op.OpT.POWK:
                    Handle54BinKOp(operations, state, line, IExpression.BinaryOperation.POW);
                    break;

                case Op.OpT.BANDK:
                    Handle54BinKOp(operations, state, line, IExpression.BinaryOperation.BAND);
                    break;

                case Op.OpT.BORK:
                    Handle54BinKOp(operations, state, line, IExpression.BinaryOperation.BOR);
                    break;

                case Op.OpT.BXORK:
                    Handle54BinKOp(operations, state, line, IExpression.BinaryOperation.BXOR);
                    break;

                case Op.OpT.SHRI:
                {
                    if (line + 1 > code.Length || code.GetOp(line + 1) != Op.MMBINI) throw new System.InvalidOperationException();
                    int immediate = code.sCField(line);
                    IExpression.BinaryOperation op = DecodeBinOp(code.CField(line + 1));
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
                    operations.Add(new RegisterSet(line, A, IExpression.Make(op, r.GetExpression(B, line), ConstantExpression.CreateInt(immediate))));
                    break;
                }
                case Op.OpT.SHLI:
                {
                    operations.Add(new RegisterSet(line, A, IExpression.Make(IExpression.BinaryOperation.SHL, ConstantExpression.CreateInt(code.sCField(line)), r.GetExpression(B, line))));
                    break;
                }
                case Op.OpT.MMBIN:
                case Op.OpT.MMBINI:
                case Op.OpT.MMBINK:
                    /* Do nothing ... handled with preceding operation. */
                    break;

                case Op.OpT.UNM:
                    HandleUnaryOp(operations, state, line, IExpression.UnaryOperation.UNM);
                    break;

                case Op.OpT.NOT:
                    HandleUnaryOp(operations, state, line, IExpression.UnaryOperation.NOT);
                    break;

                case Op.OpT.LEN:
                    HandleUnaryOp(operations, state, line, IExpression.UnaryOperation.LEN);
                    break;

                case Op.OpT.BNOT:
                    HandleUnaryOp(operations, state, line, IExpression.UnaryOperation.BNOT);
                    break;

                case Op.OpT.CONCAT:
                {
                    IExpression value = r.GetExpression(C, line);
                    //Remember that CONCAT is right associative.
                    while (C-- > B)
                    {
                        value = IExpression.Make(IExpression.BinaryOperation.CONCAT, r.GetExpression(C, line), value);
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
                        value = IExpression.Make(IExpression.BinaryOperation.CONCAT, r.GetExpression(A + B, line), value);
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
                    if (GetNoDebug() && A != B)
                    {
                        operations.Add(new RegisterSet(line, A, IExpression.Make(IExpression.BinaryOperation.OR, r.GetExpression(B, line), InitialExpression(state, A, line))));
                    }
                    break;
                }
                case Op.OpT.TESTSET:
                case Op.OpT.TESTSET54:
                {
                    if (GetNoDebug())
                    {
                        operations.Add(new RegisterSet(line, A, IExpression.Make(IExpression.BinaryOperation.OR, r.GetExpression(B, line), InitialExpression(state, A, line))));
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
                    HandleSetList(operations, state, line, A, 1 + Bx % 32, Bx - Bx % 32);
                    break;
                }
                case Op.OpT.SETLISTO:
                {
                    HandleSetList(operations, state, line, A, registers - A - 1, Bx - Bx % 32);
                    break;
                }
                case Op.OpT.SETLIST:
                {
                    if (C == 0)
                    {
                        C = code.CodePoint(line + 1);
                        skip[line + 1] = true;
                    }
                    if (B == 0)
                    {
                        B = registers - A - 1;
                    }
                    HandleSetList(operations, state, line, A, B, (C - 1) * 50);
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
                    HandleSetList(operations, state, line, A, B, (C - 1) * 50);
                    break;
                }
                case Op.OpT.SETLIST54:
                {
                    if (code.kField(line))
                    {
                        if (line + 1 > code.Length || code.GetOp(line + 1) != Op.EXTRAARG) throw new System.InvalidOperationException();
                        C += code.AxField(line + 1) * (code.GetExtractor().C.Max() + 1);
                        skip[line + 1] = true;
                    }
                    if (B == 0)
                    {
                        B = registers - A - 1;
                    }
                    HandleSetList(operations, state, line, A, B, C);
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
                    IExpression value = new VarArg(B - 1, multiple);
                    operations.Add(new MultipleRegisterSet(line, A, A + B - 2, value));
                    break;
                }
                case Op.OpT.VARARG54:
                {
                    bool multiple = (C != 2);
                    if (C == 1) throw new System.InvalidOperationException();
                    if (C == 0) C = registers - A + 1;
                    IExpression value = new VarArg(C - 1, multiple);
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

        private Assignment ProcessOperation(State state, IOperation operation, int line, int nextLine, IBlock block)
        {
            Registers r = state.r;
            bool[] skip = state.skip;
            Assignment assign = null;
            List<IStatement> stmts = operation.Process(r, block);
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
                        if (assign.GetFirstTarget().IsDeclaration(newLocal))
                        {
                            declare = true;
                            break;
                        }
                    }
                    //System.output.println("-- checking for multiassign @" + nextLine);
                    while (!declare && nextLine < block.end)
                    {
                        Op op = code.GetOp(nextLine);
                        if (IsMoveIntoTarget(r, nextLine))
                        {
                            //System.output.println("-- found multiassign @" + nextLine);
                            ITarget target = GetMoveIntoTargetTarget(r, nextLine, line + 1);
                            IExpression value = GetMoveIntoTargetValue(r, nextLine, line + 1); //updated?
                            assign.AddFirst(target, value, nextLine);
                            skip[nextLine] = true;
                            nextLine++;
                        }
                        else if (op == Op.MMBIN || op == Op.MMBINI || op == Op.MMBINK || code.IsUpvalueDeclaration(nextLine))
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
                block.AddStatement(stmt);
            }
            return assign;
        }

        private void ProcessSequence(State state, List<IBlock> blocks, int begin, int end)
        {
            Registers r = state.r;
            int blockContainerIndex = 0;
            int blockStatementIndex = 0;
            List<IBlock> blockContainers = new List<IBlock>(blocks.Count);
            List<IBlock> blockStatements = new List<IBlock>(blocks.Count);
            foreach (IBlock block in blocks)
            {
                if (block.IsContainer())
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
                    IOperation operation = endingBlock.Process(this);
                    if (blockStack.Count == 0) return;
                    if (operation == null) throw new System.InvalidOperationException();
                    operations = new List<IOperation> { operation };
                    prevLocals = r.GetNewLocals(line - 1);
                }
                else
                {
                    if (!labels_handled[line] && state.labels[line])
                    {
                        blockStack.Peek().AddStatement(new Label(line));
                        labels_handled[line] = true;
                    }

                    List<Declaration> rLocals = r.GetNewLocals(line, blockStack.Peek().closeRegister);
                    while (blockContainerIndex < blockContainers.Count && blockContainers[blockContainerIndex].begin <= line)
                    {
                        IBlock next = blockContainers[blockContainerIndex++];
                        if (rLocals.Count != 0 && next.AllowsPreDeclare() &&
                          (rLocals[0].end > next.ScopeEnd() || rLocals[0].register < next.closeRegister)
                        )
                        {
                            Assignment declaration = new Assignment();
                            int declareEnd = rLocals[0].end;
                            declaration.SetDeclare(rLocals[0].begin);
                            while (rLocals.Count != 0 && rLocals[0].end == declareEnd && (next.closeRegister == -1 || rLocals[0].register < next.closeRegister))
                            {
                                Declaration decl = rLocals[0];
                                declaration.AddLast(new VariableTarget(decl), ConstantExpression.CreateNil(line), line);
                                rLocals.RemoveAt(0);
                            }
                            blockStack.Peek().AddStatement(declaration);
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
                        IOperation operation = blockStatement.Process(this);
                        operations = new List<IOperation> { operation };
                    }
                    else
                    {
                        // After all blocks are handled for a line, we will reach here
                        nextline = line + 1;
                        if (!skip[line] && line >= begin && line <= end)
                        {
                            operations = ProcessLine(state, line);
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
                    Assignment operationAssignment = ProcessOperation(state, operation, line, nextline, block);
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
                        block.AddStatement(assignment);
                    }
                    else
                    {
                        foreach (Declaration decl in locals)
                        {
                            if (assignment.Assigns(decl))
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
                            assignment.AddLast(new VariableTarget(decl), r.GetValue(decl.register, line + 1), r.GetUpdated(decl.register, line - 1));
                        }
                    }
                }

                line = nextline;
            }
        }

        public State Decompile()
        {
            State state = new State();
            state.r = new Registers(registers, length, declarations, f, GetNoDebug());
            ControlFlowHandler.Result result = ControlFlowHandler.Process(this, state.r);
            List<IBlock> blocks = result.blocks;
            state.outer = blocks[0];
            state.labels = result.labels;
            ProcessSequence(state, blocks, 1, code.Length);
            foreach (IBlock block in blocks)
            {
                block.Resolve(state.r);
            }
            HandleUnusedConstants(state.outer);
            return state;
        }

        public Configuration GetConfiguration()
        {
            return function.header.config;
        }

        public bool GetNoDebug()
        {
            return function.header.config.Variable == Configuration.VariableMode.Nodebug ||
                function.stripped && function.header.config.Variable == Configuration.VariableMode.Default;
        }

        public Version GetVersion()
        {
            return function.header.version;
        }

        public bool HasStatement(int begin, int end)
        {
            if (begin <= end)
            {
                State state = new State();
                state.r = new Registers(registers, length, declarations, f, GetNoDebug());
                state.outer = new OuterBlock(function, code.Length);
                IBlock scoped = new DoEndBlock(function, begin, end + 1);
                state.labels = new bool[code.Length + 1];
                List<IBlock> blocks = new List<IBlock> { state.outer, scoped };
                ProcessSequence(state, blocks, 1, code.Length);
                return !scoped.IsEmpty();
            }
            else
            {
                return false;
            }
        }

        public void Write(State state)
        {
            Write(state, new Output());
        }

        public void Write(State state, IOutputProvider output)
        {
            Write(state, new Output(output));
        }

        public void Write(State state, Output output)
        {
            HandleInitialDeclares(output);
            state.outer.Write(this, output);
        }
    }
}