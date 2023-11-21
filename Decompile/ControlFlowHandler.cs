﻿using LuaDec.Decompile.Block;
using LuaDec.Decompile.Condition;
using LuaDec.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile
{
    public class ControlFlowHandler
    {

        public static bool verbose = false;

        public class Branch : IComparable<Branch>
        {

            public enum Type
            {
                comparison,
                test,
                testset,
                finalset,
                jump,
            }

            public Branch previous;
            public Branch next;
            public int line;
            public int line2;
            public int target;
            public Type type;
            public ICondition cond;
            public int targetFirst;
            public int targetSecond;
            public bool inverseValue;
            public FinalSetCondition finalset;

            public Branch(int line, int line2, Type type, ICondition cond, int targetFirst, int targetSecond, FinalSetCondition finalset)
            {
                this.line = line;
                this.line2 = line2;
                this.type = type;
                this.cond = cond;
                this.targetFirst = targetFirst;
                this.targetSecond = targetSecond;
                this.inverseValue = false;
                this.target = -1;
                this.finalset = finalset;
            }

            public int CompareTo(Branch other)
            {
                return this.line - other.line;
            }
        }

        public class State
        {
            public Decompiler d;
            public LFunction function;
            public Registers r;
            public Code code;
            public Branch begin_branch;
            public Branch end_branch;
            public Branch[] branches;
            public Branch[] setbranches;
            public List<List<Branch>> finalsetbranches;
            public bool[] reverse_targets;
            public int[] resolved;
            public bool[] labels;
            public List<IBlock> blocks;
        }

        public class Result
        {

            public Result(State state)
            {
                blocks = state.blocks;
                labels = state.labels;
            }

            public List<IBlock> blocks;
            public bool[] labels;
        }

        public static Result process(Decompiler d, Registers r)
        {
            State state = new State();
            state.d = d;
            state.function = d.function;
            state.r = r;
            state.code = d.code;
            state.labels = new bool[d.code.Length + 1];
            find_reverse_targets(state);
            find_branches(state);
            combine_branches(state);
            resolve_lines(state);
            initialize_blocks(state);
            find_fixed_blocks(state);
            find_while_loops(state);
            find_repeat_loops(state);
            find_if_break(state, d.declarations);
            find_set_blocks(state);
            find_pseudo_goto_statements(state, d.declarations);
            find_do_blocks(state, d.declarations);
            state.blocks.Sort();
            // DEBUG: print branches stuff
            /*
            Branch b = state.begin_branch;
            while(b != null) {
              System.output.println("Branch at " + b.line);
              System.output.println("\tcondition: " + b.cond);
              b = b.next;
            }
            */
            return new Result(state);
        }

        private static void find_reverse_targets(State state)
        {
            Code code = state.code;
            bool[] reverse_targets = state.reverse_targets = new bool[state.code.Length + 1];
            for (int line = 1; line <= code.Length; line++)
            {
                if (is_jmp(state, line))
                {
                    int target = code.target(line);
                    if (target <= line)
                    {
                        reverse_targets[target] = true;
                    }
                }
            }
        }

        private static void resolve_lines(State state)
        {
            int[] resolved = new int[state.code.Length + 1];
            Array.ForEach(resolved, e => e = -1);
            for (int line = 1; line <= state.code.Length; line++)
            {
                int r = line;
                Branch b = state.branches[line];
                while (b != null && b.type == Branch.Type.jump)
                {
                    if (resolved[r] >= 1)
                    {
                        r = resolved[r];
                        break;
                    }
                    else if (resolved[r] == -2)
                    {
                        r = b.targetSecond;
                        break;
                    }
                    else
                    {
                        resolved[r] = -2;
                        r = b.targetSecond;
                        b = state.branches[r];
                    }
                }
                if (r == line && state.code.GetOp(line) == Op.JMP52 && is_close(state, line))
                {
                    r = line + 1;
                }
                resolved[line] = r;
            }
            state.resolved = resolved;
        }

        private static int find_loadboolblock(State state, int target)
        {
            int loadboolblock = -1;
            Op op = state.code.GetOp(target);
            if (op == Op.LOADBOOL)
            {
                if (state.code.CField(target) != 0)
                {
                    loadboolblock = target;
                }
                else if (target - 1 >= 1 && state.code.GetOp(target - 1) == Op.LOADBOOL && state.code.CField(target - 1) != 0)
                {
                    loadboolblock = target - 1;
                }
            }
            else if (op == Op.LFALSESKIP)
            {
                loadboolblock = target;
            }
            else if (target - 1 >= 1 && op == Op.LOADTRUE && state.code.GetOp(target - 1) == Op.LFALSESKIP)
            {
                loadboolblock = target - 1;
            }
            return loadboolblock;
        }

        private static void handle_loadboolblock(State state, bool[] skip, int loadboolblock, ICondition c, int line, int target)
        {
            bool loadboolvalue;
            Op op = state.code.GetOp(target);
            if (op == Op.LOADBOOL)
            {
                loadboolvalue = state.code.BField(target) != 0;
            }
            else if (op == Op.LFALSESKIP)
            {
                loadboolvalue = false;
            }
            else if (op == Op.LOADTRUE)
            {
                loadboolvalue = true;
            }
            else
            {
                throw new System.InvalidOperationException();
            }
            int readonly_line = -1;
            if (loadboolblock - 1 >= 1 && is_jmp(state, loadboolblock - 1))
            {
                int boolskip_target = state.code.target(loadboolblock - 1);
                int boolskip_target_redirected = -1;
                if (is_jmp_raw(state, loadboolblock + 2))
                {
                    boolskip_target_redirected = state.code.target(loadboolblock + 2);
                }
                if (boolskip_target == loadboolblock + 2 || boolskip_target == boolskip_target_redirected)
                {
                    skip[loadboolblock - 1] = true;
                    readonly_line = loadboolblock - 2;
                }
            }
            bool inverse = false;
            if (loadboolvalue)
            {
                inverse = true;
                c = c.inverse();
            }
            bool constant = is_jmp(state, line);
            Branch b;
            int begin = line + 2;

            if (constant)
            {
                begin--;
                b = new Branch(line, line, Branch.Type.testset, c, begin, loadboolblock + 2, null);
            }
            else if (line + 2 == loadboolblock)
            {
                b = new Branch(loadboolblock, loadboolblock, Branch.Type.finalset, c, begin, loadboolblock + 2, null);
            }
            else
            {
                b = new Branch(line, line, Branch.Type.testset, c, begin, loadboolblock + 2, null);
            }
            b.target = state.code.AField(loadboolblock);
            b.inverseValue = inverse;
            insert_branch(state, b);

            if (readonly_line != -1)
            {
                if (constant && readonly_line < begin)
                {
                    readonly_line++;
                }
                FinalSetCondition readonlyc = new FinalSetCondition(readonly_line, b.target);
                Branch readonlyb = new Branch(readonly_line, readonly_line, Branch.Type.finalset, readonlyc, readonly_line, loadboolblock + 2, readonlyc);
                readonlyb.target = b.target;
                insert_branch(state, readonlyb);
                b.finalset = readonlyc;
            }
        }

        private static void handle_test(State state, bool[] skip, int line, ICondition c, int target, bool invert)
        {
            Code code = state.code;
            int loadboolblock = find_loadboolblock(state, target);
            if (loadboolblock >= 1)
            {
                if (invert) c = c.inverse();
                handle_loadboolblock(state, skip, loadboolblock, c, line, target);
            }
            else
            {
                int ploadboolblock = target - 2 >= 1 ? find_loadboolblock(state, target - 2) : -1;
                if (ploadboolblock != -1 && ploadboolblock == target - 2 && code.AField(target - 2) == c.register() && !has_statement(state, line + 2, target - 3))
                {
                    handle_testset(state, skip, line, c, target, c.register(), invert);
                }
                else
                {
                    if (invert) c = c.inverse();
                    Branch b = new Branch(line, line, Branch.Type.test, c, line + 2, target, null);
                    b.target = code.AField(line);
                    if (invert) b.inverseValue = true;
                    insert_branch(state, b);
                }
            }
            skip[line + 1] = true;
        }

        private static void handle_testset(State state, bool[] skip, int line, ICondition c, int target, int register, bool invert)
        {
            if (state.r.isNoDebug && find_loadboolblock(state, target) == -1)
            {
                if (invert) c = c.inverse();
                Branch nb = new Branch(line, line, Branch.Type.test, c, line + 2, target, null);
                nb.target = state.code.AField(line);
                if (invert) nb.inverseValue = true;
                insert_branch(state, nb);
                skip[line + 1] = true;
                return;
            }
            Branch b = new Branch(line, line, Branch.Type.testset, c, line + 2, target, null);
            b.target = register;
            if (invert) b.inverseValue = true;
            skip[line + 1] = true;
            insert_branch(state, b);
            int readonly_line = target - 1;
            int branch_line;
            int loadboolblock = find_loadboolblock(state, target - 2);
            if (loadboolblock != -1 && state.code.AField(loadboolblock) == register)
            {
                readonly_line = loadboolblock;
                if (loadboolblock - 2 >= 1 && is_jmp(state, loadboolblock - 1) &&
                  (state.code.target(loadboolblock - 1) == target || is_jmp_raw(state, target) && state.code.target(loadboolblock - 1) == state.code.target(target))
                )
                {
                    readonly_line = loadboolblock - 2;
                }
                branch_line = readonly_line;
            }
            else
            {
                branch_line = Math.Max(readonly_line, line + 2);
            }
            FinalSetCondition readonlyc = new FinalSetCondition(readonly_line, register);
            Branch readonlyb = new Branch(branch_line, branch_line, Branch.Type.finalset, readonlyc, readonly_line, target, readonlyc);
            readonlyb.target = register;
            insert_branch(state, readonlyb);
            b.finalset = readonlyc;
        }

        private static void process_condition(State state, bool[] skip, int line, ICondition c, bool invert)
        {
            int target = state.code.target(line + 1);
            if (invert)
            {
                c = c.inverse();
            }
            int loadboolblock = find_loadboolblock(state, target);
            if (loadboolblock >= 1)
            {
                handle_loadboolblock(state, skip, loadboolblock, c, line, target);
            }
            else
            {
                Branch b = new Branch(line, line, Branch.Type.comparison, c, line + 2, target, null);
                if (invert)
                {
                    b.inverseValue = true;
                }
                insert_branch(state, b);
            }
            skip[line + 1] = true;
        }

        private static void find_branches(State state)
        {
            Code code = state.code;
            state.branches = new Branch[state.code.Length + 1];
            state.setbranches = new Branch[state.code.Length + 1];
            state.finalsetbranches = new List<List<Branch>>(state.code.Length + 1);
            for (int i = 0; i <= state.code.Length; i++) state.finalsetbranches.Add(null);
            bool[] skip = new bool[code.Length + 1];
            for (int line = 1; line <= code.Length; line++)
            {
                if (!skip[line])
                {
                    switch (code.GetOp(line).Type)
                    {
                        case Op.OpT.EQ:
                        case Op.OpT.LT:
                        case Op.OpT.LE:
                            {
                                BinaryCondition.Operator op = BinaryCondition.Operator.EQ;
                                if (code.GetOp(line) == Op.LT) op = BinaryCondition.Operator.LT;
                                if (code.GetOp(line) == Op.LE) op = BinaryCondition.Operator.LE;
                                ICondition.Operand left = new BinaryCondition.Operand(ICondition.OperandType.RK, code.BField(line));
                                ICondition.Operand right = new BinaryCondition.Operand(ICondition.OperandType.RK, code.CField(line));
                                ICondition c = new BinaryCondition(op, line, left, right);
                                process_condition(state, skip, line, c, code.AField(line) != 0);
                                break;
                            }
                        case Op.OpT.EQ54:
                        case Op.OpT.LT54:
                        case Op.OpT.LE54:
                            {
                                BinaryCondition.Operator op = BinaryCondition.Operator.EQ;
                                if (code.GetOp(line) == Op.LT54) op = BinaryCondition.Operator.LT;
                                if (code.GetOp(line) == Op.LE54) op = BinaryCondition.Operator.LE;
                                ICondition.Operand left = new ICondition.Operand(ICondition.OperandType.R, code.AField(line));
                                ICondition.Operand right = new ICondition.Operand(ICondition.OperandType.R, code.BField(line));
                                ICondition c = new BinaryCondition(op, line, left, right);
                                process_condition(state, skip, line, c, code.kField(line));
                                break;
                            }
                        case Op.OpT.EQK:
                            {
                                BinaryCondition.Operator op = BinaryCondition.Operator.EQ;
                                ICondition.Operand right = new ICondition.Operand(ICondition.OperandType.R, code.AField(line));
                                ICondition.Operand left = new ICondition.Operand(ICondition.OperandType.K, code.BField(line));
                                ICondition c = new BinaryCondition(op, line, left, right);
                                process_condition(state, skip, line, c, code.kField(line));
                                break;
                            }
                        case Op.OpT.EQI:
                        case Op.OpT.LTI:
                        case Op.OpT.LEI:
                        case Op.OpT.GTI:
                        case Op.OpT.GEI:
                            {
                                BinaryCondition.Operator op = BinaryCondition.Operator.EQ;
                                if (code.GetOp(line) == Op.LTI) op = BinaryCondition.Operator.LT;
                                if (code.GetOp(line) == Op.LEI) op = BinaryCondition.Operator.LE;
                                if (code.GetOp(line) == Op.GTI) op = BinaryCondition.Operator.GT;
                                if (code.GetOp(line) == Op.GEI) op = BinaryCondition.Operator.GE;
                                ICondition.OperandType operandType;
                                if (code.CField(line) != 0)
                                {
                                    operandType = ICondition.OperandType.F;
                                }
                                else
                                {
                                    operandType = ICondition.OperandType.I;
                                }
                                ICondition.Operand left = new ICondition.Operand(ICondition.OperandType.R, code.AField(line));
                                ICondition.Operand right = new ICondition.Operand(operandType, code.sBField(line));
                                if (op == BinaryCondition.Operator.EQ)
                                {
                                    ICondition.Operand temp = left;
                                    left = right;
                                    right = temp;
                                }
                                ICondition c = new BinaryCondition(op, line, left, right);
                                process_condition(state, skip, line, c, code.kField(line));
                                break;
                            }
                        case Op.OpT.TEST50:
                            {
                                ICondition c = new TestCondition(line, code.BField(line));
                                int target = code.target(line + 1);
                                if (code.AField(line) == code.BField(line))
                                {
                                    handle_test(state, skip, line, c, target, code.CField(line) != 0);
                                }
                                else
                                {
                                    handle_testset(state, skip, line, c, target, code.AField(line), code.CField(line) != 0);
                                }
                                break;
                            }
                        case Op.OpT.TEST:
                            {
                                ICondition c;
                                int target = code.target(line + 1);
                                c = new TestCondition(line, code.AField(line));
                                handle_test(state, skip, line, c, target, code.CField(line) != 0);
                                break;
                            }
                        case Op.OpT.TEST54:
                            {
                                ICondition c;
                                int target = code.target(line + 1);
                                c = new TestCondition(line, code.AField(line));
                                handle_test(state, skip, line, c, target, code.kField(line));
                                break;
                            }
                        case Op.OpT.TESTSET:
                            {
                                ICondition c = new TestCondition(line, code.BField(line));
                                int target = code.target(line + 1);
                                handle_testset(state, skip, line, c, target, code.AField(line), code.CField(line) != 0);
                                break;
                            }
                        case Op.OpT.TESTSET54:
                            {
                                ICondition c = new TestCondition(line, code.BField(line));
                                int target = code.target(line + 1);
                                handle_testset(state, skip, line, c, target, code.AField(line), code.kField(line));
                                break;
                            }
                        case Op.OpT.JMP:
                        case Op.OpT.JMP52:
                        case Op.OpT.JMP54:
                            {
                                if (is_jmp(state, line))
                                {
                                    int target = code.target(line);
                                    int loadboolblock = find_loadboolblock(state, target);
                                    if (loadboolblock >= 1)
                                    {
                                        handle_loadboolblock(state, skip, loadboolblock, new ConstantCondition(-1, false), line, target);
                                    }
                                    else
                                    {
                                        Branch b = new Branch(line, line, Branch.Type.jump, null, target, target, null);
                                        insert_branch(state, b);
                                    }
                                }
                                break;
                            }
                        default:
                            break;
                    }
                }
            }
            link_branches(state);
        }

        private static void combine_branches(State state)
        {
            Branch b;

            b = state.end_branch;
            while (b != null)
            {
                b = combine_left(state, b).previous;
            }
        }

        private static void initialize_blocks(State state)
        {
            state.blocks = new List<IBlock>();
        }

        private static void find_fixed_blocks(State state)
        {
            List<IBlock> blocks = state.blocks;
            Registers r = state.r;
            Code code = state.code;
            Op tforTarget = state.function.header.version.tforTarget.Value;
            Op forTarget = state.function.header.version.forTarget.Value;
            blocks.Add(new OuterBlock(state.function, state.code.Length));

            bool[] loop = new bool[state.code.Length + 1];

            Branch b = state.begin_branch;
            while (b != null)
            {
                if (b.type == Branch.Type.jump)
                {
                    int line = b.line;
                    int target = b.targetFirst;
                    if (code.GetOp(target) == tforTarget && !loop[target])
                    {
                        loop[target] = true;
                        int A = code.AField(target);
                        int C = code.CField(target);
                        if (C == 0) throw new System.InvalidOperationException();
                        remove_branch(state, state.branches[line]);
                        if (state.branches[target + 1] != null)
                        {
                            remove_branch(state, state.branches[target + 1]);
                        }

                        bool forvarClose = false;
                        bool innerClose = false;
                        int close = target - 1;
                        if (close >= line + 1 && is_close(state, close) && code.AField(close) == A + 3)
                        {
                            forvarClose = true;
                            close--;
                        }
                        if (close >= line + 1 && is_close(state, close) && code.AField(close) <= A + 3 + C)
                        {
                            innerClose = true;
                        }

                        TForBlock block = TForBlock.make51(state.function, line + 1, target + 2, A, C, forvarClose, innerClose);
                        block.handleVariableDeclarations(r);
                        blocks.Add(block);
                    }
                    else if (code.GetOp(target) == forTarget && !loop[target])
                    {
                        loop[target] = true;
                        int A = code.AField(target);

                        ForBlock block = new ForBlock50(
                          state.function, line + 1, target + 1, A,
                          get_close_type(state, target - 1), target - 1
                        );

                        block.handleVariableDeclarations(r);

                        blocks.Add(block);
                        remove_branch(state, b);
                    }
                }
                b = b.next;
            }

            for (int line = 1; line <= code.Length; line++)
            {
                switch (code.GetOp(line).Type)
                {
                    case Op.OpT.FORPREP:
                    case Op.OpT.FORPREP54: {

                            int A = code.AField(line);
                            int target = code.target(line);

                            bool forvarClose = false;
                            int closeLine = target - 1;
                            if (closeLine >= line + 1 && is_close(state, closeLine) && code.AField(closeLine) == A + 3)
                            {
                                forvarClose = true;
                                closeLine--;
                            }

                            ForBlock block = new ForBlock51(
                              state.function, line + 1, target + 1, A,
                              get_close_type(state, closeLine), closeLine, forvarClose
                            );

                            block.handleVariableDeclarations(r);
                            blocks.Add(block);
                            break;
                        }
                    case Op.OpT.TFORPREP: {
                            int target = code.target(line);
                            int A = code.AField(target);
                            int C = code.CField(target);

                            bool innerClose = false;
                            int close = target - 1;
                            if (close >= line + 1 && is_close(state, close) && code.AField(close) == A + 3 + C)
                            {
                                innerClose = true;
                            }

                            TForBlock block = TForBlock.make50(state.function, line + 1, target + 2, A, C + 1, innerClose);
                            block.handleVariableDeclarations(r);
                            blocks.Add(block);
                            remove_branch(state, state.branches[target + 1]);
                            break;
                        }
                    case Op.OpT.TFORPREP54: {
                            int target = code.target(line);
                            int A = code.AField(line);
                            int C = code.CField(target);

                            bool forvarClose = false;
                            int close = target - 1;
                            if (close >= line + 1 && is_close(state, close) && code.AField(close) == A + 4)
                            {
                                forvarClose = true;
                                close--;
                            }

                            TForBlock block = TForBlock.make54(state.function, line + 1, target + 2, A, C, forvarClose);
                            block.handleVariableDeclarations(r);
                            blocks.Add(block);
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        private static void unredirect(State state, int begin, int end, int line, int target)
        {
            Branch b = state.begin_branch;
            while (b != null)
            {
                if (b.line >= begin && b.line < end && b.targetSecond == target)
                {
                    if (b.type == Branch.Type.finalset)
                    {
                        b.targetFirst = line - 1;
                        b.targetSecond = line;
                        if (b.finalset != null)
                        {
                            b.finalset.line = line - 1;
                        }
                    }
                    else
                    {
                        b.targetSecond = line;
                        if (b.targetFirst == target)
                        {
                            b.targetFirst = line;
                        }
                    }
                }
                b = b.next;
            }
        }

        private static void find_while_loops(State state)
        {
            List<IBlock> blocks = state.blocks;
            Branch j = state.end_branch;
            while (j != null)
            {
                if (j.type == Branch.Type.jump && j.targetFirst <= j.line)
                {
                    int line = j.targetFirst;
                    int loopback = line;
                    int end = j.line + 1;
                    Branch b = state.begin_branch;
                    int extent = -1;
                    while (b != null)
                    {
                        if (is_conditional(b) && b.line >= loopback && b.line < j.line && state.resolved[b.targetSecond] == state.resolved[end] && extent <= b.line)
                        {
                            break;
                        }
                        if (b.line >= loopback)
                        {
                            extent = Math.Max(extent, b.targetSecond);
                        }
                        b = b.next;
                    }
                    if (b != null)
                    {
                        bool reverse = state.reverse_targets[loopback];
                        state.reverse_targets[loopback] = false;
                        if (has_statement(state, loopback, b.line - 1))
                        {
                            b = null;
                        }
                        state.reverse_targets[loopback] = reverse;
                    }
                    if (state.function.header.version.whileFormat.Value == Version.WhileFormat.BottomCondition)
                    {
                        b = null; // while loop aren't this style
                    }
                    IBlock loop = null;
                    if (b != null)
                    {
                        b.targetSecond = end;
                        remove_branch(state, b);
                        //Console.Error.WriteLine("while " + b.targetFirst + " " + b.targetSecond);
                        loop = new WhileBlock51(
                          state.function, b.cond, b.targetFirst, b.targetSecond, loopback,
                          get_close_type(state, end - 2), end - 2
                        );
                        unredirect(state, loopback, end, j.line, loopback);
                    }
                    if (loop == null && j.line - 5 >= 1 && state.code.GetOp(j.line - 3) == Op.CLOSE
                      && is_jmp_raw(state, j.line - 2) && state.code.target(j.line - 2) == end
                      && state.code.GetOp(j.line - 1) == Op.CLOSE
                    )
                    {
                        b = j.previous;
                        while (b != null && !(is_conditional(b) && b.line2 == j.line - 5))
                        {
                            b = b.previous;
                        }
                        if (b != null)
                        {
                            Branch skip = state.branches[j.line - 2];
                            if (skip == null) throw new System.InvalidOperationException();
                            int scopeEnd = j.line - 3;
                            if (state.function.header.version.closeInScope.Value)
                            {
                                scopeEnd = j.line - 2;
                            }
                            // TODO: make this work better with new close system
                            loop = new RepeatBlock(
                              state.function, b.cond, j.targetFirst, j.line + 1,
                              CloseType.NONE, -1,
                              true, scopeEnd
                            );
                            remove_branch(state, b);
                            remove_branch(state, skip);
                        }
                    }
                    if (loop == null)
                    {
                        bool repeat = false;
                        if (state.function.header.version.whileFormat.Value == Version.WhileFormat.BottomCondition)
                        {
                            repeat = true;
                            if (loopback - 1 >= 1 && state.branches[loopback - 1] != null)
                            {
                                Branch head = state.branches[loopback - 1];
                                if (head.type == Branch.Type.jump && head.targetFirst == j.line)
                                {
                                    remove_branch(state, head);
                                    repeat = false;
                                }
                            }
                        }
                        loop = new AlwaysLoop(state.function, loopback, end, get_close_type(state, end - 2), end - 2, repeat);
                        unredirect(state, loopback, end, j.line, loopback);
                    }
                    remove_branch(state, j);
                    blocks.Add(loop);
                }
                j = j.previous;
            }
        }

        private static void find_repeat_loops(State state)
        {
            List<IBlock> blocks = state.blocks;
            Branch b = state.begin_branch;
            while (b != null)
            {
                if (is_conditional(b))
                {
                    if (b.targetSecond < b.targetFirst)
                    {
                        IBlock block = null;
                        if (state.function.header.version.whileFormat.Value == Version.WhileFormat.BottomCondition)
                        {
                            int head = b.targetSecond - 1;
                            if (head >= 1 && state.branches[head] != null && state.branches[head].type == Branch.Type.jump)
                            {
                                Branch headb = state.branches[head];
                                if (headb.targetSecond <= b.line)
                                {
                                    if (has_statement(state, headb.targetSecond, b.line - 1))
                                    {
                                        headb = null;
                                    }
                                    if (headb != null)
                                    {
                                        block = new WhileBlock50(
                                          state.function, b.cond.inverse(), head + 1, b.targetFirst, headb.targetFirst,
                                          get_close_type(state, headb.targetFirst - 1), headb.targetFirst - 1
                                        );
                                        remove_branch(state, headb);
                                        unredirect(state, 1, headb.line, headb.line, headb.targetSecond);
                                    }
                                }
                            }
                        }
                        if (block == null)
                        {
                            if (state.function.header.version.extendedRepeatScope.Value)
                            {
                                int statementLine = b.line - 1;
                                while (statementLine >= 1 && !is_statement(state, statementLine))
                                {
                                    statementLine--;
                                }
                                block = new RepeatBlock(
                                  state.function, b.cond, b.targetSecond, b.targetFirst,
                                  get_close_type(state, statementLine), statementLine,
                                  true, statementLine
                                );
                            }
                            else if (state.function.header.version.closeSemantics.Value == Version.CloseSemantics.Jump)
                            {
                                block = new RepeatBlock(
                                  state.function, b.cond, b.targetSecond, b.targetFirst,
                                  get_close_type(state, b.targetFirst), b.targetFirst,
                                  false, -1
                                );
                            }
                            else
                            {
                                block = new RepeatBlock(
                                  state.function, b.cond, b.targetSecond, b.targetFirst,
                                  CloseType.NONE, -1,
                                  false, -1
                                );
                            }
                        }
                        remove_branch(state, b);
                        blocks.Add(block);
                    }
                }
                b = b.next;
            }
        }

        private static bool splits_decl(int line, int begin, int end, Declaration[] declList)
        {
            foreach (Declaration decl in declList)
            {
                if (decl.isSplitBy(line, begin, end))
                {
                    return true;
                }
            }
            return false;
        }

        private static int stack_reach(State state, Stack<Branch> stack)
        {
            var stackList = stack.ToList();
            stackList.Reverse();
            foreach (var b in stackList)
            {
                IBlock breakable = enclosing_breakable_block(state, b.line);
                if (breakable != null && breakable.end == b.targetSecond)
                {
                    // next
                }
                else
                {
                    return b.targetSecond;
                }
            }
            return int.MaxValue;
        }

        private static IBlock resolve_if_stack(State state, Stack<Branch> stack, int line)
        {
            IBlock block = null;
            if (stack.Count != 0 && stack_reach(state, stack) <= line)
            {
                Branch top = stack.Pop();
                int literalEnd = state.code.target(top.targetFirst - 1);
                block = new IfThenEndBlock(
                  state.function, state.r, top.cond, top.targetFirst, top.targetSecond,
                  get_close_type(state, top.targetSecond - 1), top.targetSecond - 1,
                  literalEnd != top.targetSecond
                );
                state.blocks.Add(block);
                remove_branch(state, top);
            }
            return block;
        }

        private static void resolve_else(State state, Stack<Branch> stack, Stack<Branch> hanging, Stack<ElseEndBlock> elseStack, Branch top, Branch b, int tailTargetSecond)
        {
            while (elseStack.Count != 0 && elseStack.Peek().end == tailTargetSecond && elseStack.Peek().begin >= top.targetFirst)
            {
                elseStack.Pop().end = b.line;
            }

            Stack<Branch> replace = new Stack<Branch>();
            while (hanging.Count != 0 && hanging.Peek().targetSecond == tailTargetSecond && hanging.Peek().line > top.line)
            {
                Branch hanger = hanging.Pop();
                hanger.targetSecond = b.line;
                IBlock breakable = enclosing_breakable_block(state, hanger.line);
                if (breakable != null && hanger.targetSecond >= breakable.end)
                {
                    replace.Push(hanger);
                }
                else
                {
                    stack.Push(hanger);
                    IBlock if_block = resolve_if_stack(state, stack, b.line);
                    if (if_block == null) throw new System.InvalidOperationException();
                }
            }
            while (replace.Count != 0)
            {
                hanging.Push(replace.Pop());
            }

            unredirect_finalsets(state, tailTargetSecond, b.line, top.targetFirst);

            Stack<Branch> restore = new Stack<Branch>();
            while (stack.Count != 0 && stack.Peek().line > top.line && stack.Peek().targetSecond == b.targetSecond)
            {
                stack.Peek().targetSecond = b.line;
                restore.Push(stack.Pop());
            }
            while (restore.Count != 0)
            {
                stack.Push(restore.Pop());
            }

            b.targetSecond = tailTargetSecond;
            state.blocks.Add(new IfThenElseBlock(
              state.function, top.cond, top.targetFirst, top.targetSecond, b.targetSecond,
              get_close_type(state, top.targetSecond - 2), top.targetSecond - 2
            ));
            ElseEndBlock elseBlock = new ElseEndBlock(
              state.function, top.targetSecond, b.targetSecond,
              get_close_type(state, b.targetSecond - 1), b.targetSecond - 1
            );
            state.blocks.Add(elseBlock);
            elseStack.Push(elseBlock);
            remove_branch(state, b);
        }

        private static bool is_hanger_resolvable(State state, Declaration[] declList, Branch hanging, Branch resolver)
        {
            if (
              hanging.targetSecond == resolver.targetFirst
              && enclosing_block(state, hanging.line) == enclosing_block(state, resolver.line)
              && !splits_decl(hanging.line, hanging.targetFirst, resolver.line, declList)
              && !(
                state.function.header.version.useIfBreakRewrite.Value
                && hanging.targetFirst == resolver.line - 1
                && is_jmp(state, resolver.line - 1)
              )
            )
            {
                return true;
            }
            return false;
        }

        private static bool is_hanger_resolvable(State state, Declaration[] declList, Branch hanging, Stack<Branch> resolvers)
        {
            var resolversList = resolvers.ToList();
            resolversList.Reverse();
            foreach (var r in resolversList)
            {
                if (is_hanger_resolvable(state, declList, hanging, r))
                {
                    return true;
                }
            }
            return false;
        }

        private static void resolve_hanger(State state, Declaration[] declList, Stack<Branch> stack, Branch hanger, Branch b)
        {
            hanger.targetSecond = b.line;
            stack.Push(hanger);
            IBlock if_block = resolve_if_stack(state, stack, b.line);
            if (if_block == null) throw new System.InvalidOperationException();
        }

        private static void resolve_hangers(State state, Declaration[] declList, Stack<Branch> stack, Stack<Branch> hanging, Branch b)
        {
            while (hanging.Count != 0 && is_hanger_resolvable(state, declList, hanging.Peek(), b))
            {
                resolve_hanger(state, declList, stack, hanging.Pop(), b);
            }
        }

        private static void find_if_break(State state, Declaration[] declList)
        {
            Stack<Branch> stack = new Stack<Branch>();
            Stack<Branch> hanging = new Stack<Branch>();
            Stack<ElseEndBlock> elseStack = new Stack<ElseEndBlock>();
            Branch b = state.begin_branch;
            Stack<Branch> hangingResolver = new Stack<Branch>();

            while (b != null)
            {
                while (resolve_if_stack(state, stack, b.line2) != null) { }

                while (elseStack.Count != 0 && elseStack.Peek().end <= b.line)
                {
                    elseStack.Pop();
                }

                while (hangingResolver.Count != 0 && !enclosing_block(state, hangingResolver.Peek().line).contains(b.line))
                {
                    resolve_hangers(state, declList, stack, hanging, hangingResolver.Pop());
                }

                if (is_conditional(b))
                {
                    IBlock unprotected = enclosing_unprotected_block(state, b.line);
                    if (b.targetFirst > b.targetSecond) throw new System.InvalidOperationException();
                    if (unprotected != null && !unprotected.contains(b.targetSecond))
                    {
                        if (b.targetSecond == unprotected.getUnprotectedTarget())
                        {
                            b.targetSecond = unprotected.getUnprotectedLine();
                        }
                    }

                    IBlock breakable = enclosing_breakable_block(state, b.line);
                    if (stack.Count != 0 && stack.Peek().targetSecond < b.targetSecond
                      || breakable != null && !breakable.contains(b.targetSecond)
                    )
                    {
                        hanging.Push(b);
                    }
                    else
                    {
                        stack.Push(b);
                    }
                }
                else if (b.type == Branch.Type.jump)
                {
                    int line = b.line;

                    IBlock enclosing = enclosing_block(state, b.line);

                    int tailTargetSecond = b.targetSecond;
                    IBlock unprotected = enclosing_unprotected_block(state, b.line);
                    if (unprotected != null && !unprotected.contains(b.targetSecond))
                    {
                        if (tailTargetSecond == state.resolved[unprotected.getUnprotectedTarget()])
                        {
                            tailTargetSecond = unprotected.getUnprotectedLine();
                        }
                    }

                    bool handled = false;

                    IBlock breakable = enclosing_breakable_block(state, line);
                    if (breakable != null && (b.targetFirst == breakable.end || b.targetFirst == state.resolved[breakable.end]))
                    {
                        Break block = new Break(state.function, b.line, b.targetFirst);
                        if (hanging.Count != 0 && hanging.Peek().targetSecond == b.targetFirst
                          && enclosing_block(state, hanging.Peek().line) == enclosing
                          && (stack.Count == 0
                            || stack.Peek().line < hanging.Peek().line
                            || hanging.Peek().line > stack.Peek().line)
                        )
                        {
                            hangingResolver.Push(b);
                        }
                        unredirect_finalsets(state, b.targetFirst, line, breakable.begin);
                        state.blocks.Add(block);
                        remove_branch(state, b);
                        handled = true;
                    }

                    if (!handled && state.function.header.version.useGoto.Value && breakable != null && !breakable.contains(b.targetFirst) && state.resolved[b.targetFirst] != state.resolved[breakable.end])
                    {
                        Goto block = new Goto(state.function, b.line, b.targetFirst);
                        if (hanging.Count != 0 && hanging.Peek().targetSecond == b.targetFirst
                          && enclosing_block(state, hanging.Peek().line) == enclosing
                          && (stack.Count == 0 || hanging.Peek().line > stack.Peek().line)
                        )
                        {
                            hangingResolver.Push(b);
                        }
                        unredirect_finalsets(state, b.targetFirst, line, 1);
                        state.blocks.Add(block);
                        state.labels[b.targetFirst] = true;
                        remove_branch(state, b);
                        handled = true;
                    }

                    if (!handled && stack.Count != 0 && stack.Peek().targetSecond - 1 == b.line)
                    {
                        Branch top = stack.Peek();
                        while (top != null && top.targetSecond - 1 == b.line && splits_decl(top.line, top.targetFirst, top.targetSecond, declList))
                        {
                            IBlock if_block = resolve_if_stack(state, stack, top.targetSecond);
                            if (if_block == null) throw new System.InvalidOperationException();
                            top = stack.Count == 0 ? null : stack.Peek();
                        }
                        if (top != null && top.targetSecond - 1 == b.line)
                        {
                            if (top.targetSecond != b.targetSecond)
                            {
                                resolve_else(state, stack, hanging, elseStack, top, b, tailTargetSecond);
                                stack.Pop();
                            }
                            else if (!splits_decl(top.line, top.targetFirst, top.targetSecond - 1, declList))
                            {
                                // "empty else" case
                                b.targetSecond = tailTargetSecond;
                                state.blocks.Add(new IfThenElseBlock(
                                  state.function, top.cond, top.targetFirst, top.targetSecond, b.targetSecond,
                                  get_close_type(state, top.targetSecond - 2), top.targetSecond - 2
                                ));
                                remove_branch(state, b);
                                stack.Pop();
                            }
                        }
                        handled = true; // TODO: should this always count as handled?
                    }

                    if (
                      !handled
                      && breakable != null
                      && line + 1 < state.branches.Length && state.branches[line + 1] != null
                      && state.branches[line + 1].type == Branch.Type.jump
                    )
                    {
                        var hangingList = hanging.ToList();
                        hangingList.Reverse();
                        
                        for (int i = 0; i < hangingList.Count; i++)
                        {
                            Branch hanger = hangingList[i];
                            if (
                              state.resolved[hanger.targetSecond] == state.resolved[breakable.end]
                              && line + 1 < state.branches.Length && state.branches[line + 1] != null
                              && state.branches[line + 1].targetFirst == hanger.targetSecond
                              && !splits_decl(hanger.line, hanger.targetFirst, b.line, declList) // if else
                              && !splits_decl(b.line, b.line + 1, b.line + 2, declList) // else break
                              && !splits_decl(hanger.line, hanger.targetFirst, b.line + 2, declList) // full
                            )
                            {
                                // resolve intervening hangers
                                for (int j = i; j > 0; j--)
                                {
                                    while (!is_hanger_resolvable(state, declList, hanging.Peek(), hangingResolver.Peek()))
                                    {
                                        hangingResolver.Pop();
                                    }
                                    resolve_hanger(state, declList, stack, hanging.Pop(), hangingResolver.Peek());
                                }

                                // else break
                                Branch top = hanging.Pop();
                                if (hangingResolver.Count != 0 && hangingResolver.Peek().targetFirst == top.targetSecond)
                                {
                                    hangingResolver.Pop();
                                }
                                top.targetSecond = line + 1;
                                resolve_else(state, stack, hanging, elseStack, top, b, tailTargetSecond);
                                handled = true;
                                break;
                            }
                            else if (!is_hanger_resolvable(state, declList, hanger, hangingResolver))
                            {
                                break;
                            }
                        }
                    }

                    if (
                      !handled
                      && breakable != null && breakable.isSplitable()
                      && state.resolved[b.targetFirst] == breakable.getUnprotectedTarget()
                      && line + 1 < state.branches.Length && state.branches[line + 1] != null
                      && state.branches[line + 1].type == Branch.Type.jump
                      && state.resolved[state.branches[line + 1].targetFirst] == state.resolved[breakable.end]
                    )
                    {
                        // split while condition (else break)
                        IBlock[] split = breakable.split(b.line, get_close_type(state, b.line - 1));
                        foreach (IBlock block in split)
                        {
                            state.blocks.Add(block);
                        }
                        remove_branch(state, b);
                        handled = true;
                    }

                    if (
                      !handled
                      && stack.Count != 0 && stack.Peek().targetSecond == b.targetFirst
                      && line + 1 < state.branches.Length && state.branches[line + 1] != null
                      && state.branches[line + 1].type == Branch.Type.jump
                      && state.branches[line + 1].targetFirst == b.targetFirst
                    )
                    {
                        // empty else (redirected)
                        Branch top = stack.Peek();
                        if (!splits_decl(top.line, top.targetFirst, b.line, declList))
                        {
                            top.targetSecond = line + 1;
                            b.targetSecond = line + 1;
                            state.blocks.Add(new IfThenElseBlock(
                              state.function, top.cond, top.targetFirst, top.targetSecond, b.targetSecond,
                              get_close_type(state, line - 1), line - 1
                            ));
                            remove_branch(state, b);
                            stack.Pop();
                        }
                        handled = true; // TODO:
                    }

                    if (
                      !handled
                      && hanging.Count != 0 && hanging.Peek().targetSecond == b.targetFirst
                      && line + 1 < state.branches.Length && state.branches[line + 1] != null
                      && state.branches[line + 1].type == Branch.Type.jump
                      && state.branches[line + 1].targetFirst == b.targetFirst
                    )
                    {
                        // empty else (redirected)
                        Branch top = hanging.Peek();
                        if (!splits_decl(top.line, top.targetFirst, b.line, declList))
                        {
                            if (hangingResolver.Count != 0 && hangingResolver.Peek().targetFirst == top.targetSecond)
                            {
                                hangingResolver.Pop();
                            }
                            top.targetSecond = line + 1;
                            b.targetSecond = line + 1;
                            state.blocks.Add(new IfThenElseBlock(
                              state.function, top.cond, top.targetFirst, top.targetSecond, b.targetSecond,
                              get_close_type(state, line - 1), line - 1
                            ));
                            remove_branch(state, b);
                            hanging.Pop();
                        }
                        handled = true; // TODO:
                    }

                    if (!handled && (state.function.header.version.useGoto.Value || state.r.isNoDebug))
                    {
                        Goto block = new Goto(state.function, b.line, b.targetFirst);
                        if (hanging.Count != 0 && hanging.Peek().targetSecond == b.targetFirst && enclosing_block(state, hanging.Peek().line) == enclosing)
                        {
                            hangingResolver.Push(b);
                        }
                        state.blocks.Add(block);
                        state.labels[b.targetFirst] = true;
                        remove_branch(state, b);
                        handled = true;
                    }
                }
                b = b.next;
            }
            while (hangingResolver.Count != 0)
            {
                resolve_hangers(state, declList, stack, hanging, hangingResolver.Pop());
            }
            while (hanging.Count != 0)
            {
                // if break (or if goto)
                Branch top = hanging.Pop();
                IBlock breakable = enclosing_breakable_block(state, top.line);
                if (breakable != null && breakable.end == top.targetSecond)
                {
                    if (state.function.header.version.useIfBreakRewrite.Value || state.r.isNoDebug)
                    {
                        IBlock block = new IfThenEndBlock(state.function, state.r, top.cond.inverse(), top.targetFirst - 1, top.targetFirst - 1);
                        block.addStatement(new Break(state.function, top.targetFirst - 1, top.targetSecond));
                        state.blocks.Add(block);
                    }
                    else
                    {
                        throw new System.InvalidOperationException();
                    }
                }
                else if (state.function.header.version.useGoto.Value || state.r.isNoDebug)
                {
                    if (state.function.header.version.useIfBreakRewrite.Value || state.r.isNoDebug)
                    {
                        IBlock block = new IfThenEndBlock(state.function, state.r, top.cond.inverse(), top.targetFirst - 1, top.targetFirst - 1);
                        block.addStatement(new Goto(state.function, top.targetFirst - 1, top.targetSecond));
                        state.blocks.Add(block);
                        state.labels[top.targetSecond] = true;
                    }
                    else
                    {
                        // No version supports goto without if break rewrite
                        throw new System.InvalidOperationException();
                    }
                }
                else
                {
                    throw new System.InvalidOperationException();
                }
                remove_branch(state, top);
            }
            while (resolve_if_stack(state, stack, int.MaxValue) != null) { }
        }

        private static void unredirect_finalsets(State state, int target, int line, int begin)
        {
            Branch b = state.begin_branch;
            while (b != null)
            {
                if (b.type == Branch.Type.finalset)
                {
                    if (b.targetSecond == target && b.line < line && b.line >= begin)
                    {
                        b.targetFirst = line - 1;
                        b.targetSecond = line;
                        if (b.finalset != null)
                        {
                            b.finalset.line = line - 1;
                        }
                    }
                }
                b = b.next;
            }
        }

        private static void find_set_blocks(State state)
        {
            List<IBlock> blocks = state.blocks;
            Branch b = state.begin_branch;
            while (b != null)
            {
                if (is_assignment(b) || b.type == Branch.Type.finalset)
                {
                    if (b.finalset != null)
                    {
                        FinalSetCondition c = b.finalset;
                        Op op = state.code.GetOp(c.line);
                        if (c.line >= 2 && (op == Op.MMBIN || op == Op.MMBINI || op == Op.MMBINK || op == Op.EXTRAARG))
                        {
                            c.line--;
                            if (b.targetFirst == c.line + 1)
                            {
                                b.targetFirst = c.line;
                            }
                        }
                        while (state.code.isUpvalueDeclaration(c.line))
                        {
                            c.line--;
                            if (b.targetFirst == c.line + 1)
                            {
                                b.targetFirst = c.line;
                            }
                        }

                        if (is_jmp_raw(state, c.line))
                        {
                            c.type = FinalSetCondition.Type.REGISTER;
                        }
                        else
                        {
                            c.type = FinalSetCondition.Type.VALUE;
                        }
                    }
                    if (b.cond == b.finalset)
                    {
                        remove_branch(state, b);
                    }
                    else
                    {
                        IBlock block = new SetBlock(state.function, b.cond, b.target, b.line, b.targetFirst, b.targetSecond, state.r);
                        blocks.Add(block);
                        remove_branch(state, b);
                    }
                }
                b = b.next;
            }
        }

        private static IBlock enclosing_block(State state, int line)
        {
            IBlock enclosing = null;
            foreach (IBlock block in state.blocks)
            {
                if (block.contains(line))
                {
                    if (enclosing == null || enclosing.contains(block))
                    {
                        enclosing = block;
                    }
                }
            }
            return enclosing;
        }

        private static IBlock enclosing_breakable_block(State state, int line)
        {
            IBlock enclosing = null;
            foreach (IBlock block in state.blocks)
            {
                if (block.contains(line) && block.breakable())
                {
                    if (enclosing == null || enclosing.contains(block))
                    {
                        enclosing = block;
                    }
                }
            }
            return enclosing;
        }

        private static IBlock enclosing_unprotected_block(State state, int line)
        {
            IBlock enclosing = null;
            foreach (IBlock block in state.blocks)
            {
                if (block.contains(line) && block.isUnprotected())
                {
                    if (enclosing == null || enclosing.contains(block))
                    {
                        enclosing = block;
                    }
                }
            }
            return enclosing;
        }

        private static void find_pseudo_goto_statements(State state, Declaration[] declList)
        {
            Branch b = state.begin_branch;
            while (b != null)
            {
                if (b.type == Branch.Type.jump && b.targetFirst > b.line)
                {
                    int end = b.targetFirst;
                    IBlock smallestEnclosing = null;
                    foreach (IBlock block in state.blocks)
                    {
                        if (block.contains(b.line) && block.contains(end - 1))
                        {
                            if (smallestEnclosing == null || smallestEnclosing.contains(block))
                            {
                                smallestEnclosing = block;
                            }
                        }
                    }
                    if (smallestEnclosing != null)
                    {
                        // Should always find the outer block at least...
                        IBlock wrapping = null;
                        foreach (IBlock block in state.blocks)
                        {
                            if (block != smallestEnclosing && smallestEnclosing.contains(block) && block.contains(b.line))
                            {
                                if (wrapping == null || block.contains(wrapping))
                                {
                                    wrapping = block;
                                }
                            }
                        }
                        int begin = smallestEnclosing.begin;
                        if (wrapping != null)
                        {
                            begin = Math.Max(wrapping.begin - 1, smallestEnclosing.begin);
                            //beginMax = begin;
                        }
                        int lowerBound = int.MinValue;
                        int upperBound = int.MaxValue;
                        int scopeAdjust = -1;
                        foreach (Declaration decl in declList)
                        {
                            //if(decl.begin >= begin && decl.begin < end) {

                            //}
                            if (decl.end >= begin && decl.end <= end + scopeAdjust)
                            {
                                if (decl.begin < begin)
                                {
                                    upperBound = Math.Min(decl.begin, upperBound);
                                }
                            }
                            if (decl.begin >= begin && decl.begin <= end + scopeAdjust && decl.end > end + scopeAdjust)
                            {
                                lowerBound = Math.Max(decl.begin + 1, lowerBound);
                                begin = decl.begin + 1;
                            }
                        }
                        if (lowerBound > upperBound)
                        {
                            throw new System.InvalidOperationException();
                        }
                        begin = Math.Max(lowerBound, begin);
                        begin = Math.Min(upperBound, begin);
                        IBlock breakable = enclosing_breakable_block(state, b.line);
                        if (breakable != null)
                        {
                            begin = Math.Max(breakable.begin, begin);
                        }
                        bool containsBreak = false;
                        OnceLoop loop = new OnceLoop(state.function, begin, end);
                        foreach (IBlock block in state.blocks)
                        {
                            if (loop.contains(block) && block is Break)
                            {
                                containsBreak = true;
                                break;
                            }
                        }
                        if (containsBreak)
                        {
                            // TODO: close type
                            state.blocks.Add(new IfThenElseBlock(state.function, FixedCondition.TRUE, begin, b.line + 1, end, CloseType.NONE, -1));
                            state.blocks.Add(new ElseEndBlock(state.function, b.line + 1, end, CloseType.NONE, -1));
                            remove_branch(state, b);
                        }
                        else
                        {
                            state.blocks.Add(loop);
                            Branch b2 = b;
                            while (b2 != null)
                            {
                                if (b2.type == Branch.Type.jump && b2.targetFirst > b2.line && b2.targetFirst == b.targetFirst)
                                {
                                    Break breakStatement = new Break(state.function, b2.line, b2.targetFirst);
                                    state.blocks.Add(breakStatement);
                                    breakStatement.comment = "pseudo-goto";
                                    remove_branch(state, b2);
                                    if (b.next == b2)
                                    {
                                        b = b2;
                                    }
                                }
                                b2 = b2.next;
                            }
                        }
                    }
                }
                b = b.next;
            }
        }

        private static void find_do_blocks(State state, Declaration[] declList)
        {
            List<IBlock> newBlocks = new List<IBlock>();
            foreach (IBlock block in state.blocks)
            {
                if (block.hasCloseLine() && block.getCloseLine() >= 1)
                {
                    int closeLine = block.getCloseLine();
                    IBlock enclosing = enclosing_block(state, closeLine);
                    if ((enclosing == block || enclosing.contains(block)) && is_close(state, closeLine))
                    {
                        int register = get_close_value(state, closeLine);
                        bool close = true;
                        Declaration closeDecl = null;
                        foreach (Declaration decl in declList)
                        {
                            if (!decl.forLoop && !decl.forLoopExplicit && block.contains(decl.begin))
                            {
                                if (decl.register < register)
                                {
                                    close = false;
                                }
                                else if (decl.register == register)
                                {
                                    closeDecl = decl;
                                }
                            }
                        }
                        if (close)
                        {
                            block.useClose();
                        }
                        else if (closeDecl != null)
                        {
                            IBlock inner = new DoEndBlock(state.function, closeDecl.begin, closeDecl.end + 1);
                            inner.closeRegister = register;
                            newBlocks.Add(inner);
                            strictScopeCheck(state);
                        }
                    }
                }
            }
            state.blocks.AddRange(newBlocks);

            foreach (Declaration decl in declList)
            {
                int begin = decl.begin;
                if (!decl.forLoop && !decl.forLoopExplicit)
                {
                    bool needsDoEnd = true;
                    foreach (IBlock block in state.blocks)
                    {
                        if (block.contains(decl.begin))
                        {
                            if (block.scopeEnd() == decl.end)
                            {
                                block.useScope();
                                needsDoEnd = false;
                                break;
                            }
                            else if (block.scopeEnd() < decl.end)
                            {
                                begin = Math.Min(begin, block.begin);
                            }
                        }
                    }
                    if (needsDoEnd)
                    {
                        // Without accounting for the order of declarations, we might
                        // create another do..end block later that would eliminate the
                        // need for this one. But order of decls should fix this.
                        state.blocks.Add(new DoEndBlock(state.function, begin, decl.end + 1));
                        strictScopeCheck(state);
                    }
                }
            }
        }

        private static void strictScopeCheck(State state)
        {
            if (state.function.header.config.StrictScope)
            {
                throw new ArgumentException("Violation of strict scope rule");
            }
        }

        private static bool is_conditional(Branch b)
        {
            return b.type == Branch.Type.comparison || b.type == Branch.Type.test;
        }

        private static bool is_assignment(Branch b)
        {
            return b.type == Branch.Type.testset;
        }

        private static bool is_assignment(Branch b, int r)
        {
            return b.type == Branch.Type.testset || b.type == Branch.Type.test && b.target == r;
        }

        private static bool adjacent(State state, Branch branch0, Branch branch1)
        {
            if (branch1.finalset != null && branch0.finalset == branch1.finalset)
            {
                // With redirects, there can be real statements between a finalset and paired branches.
                return true;
            }
            else if (branch0 == null || branch1 == null)
            {
                return false;
            }
            else
            {
                bool adjacent = branch0.targetFirst <= branch1.line;
                if (adjacent)
                {
                    adjacent = !has_statement(state, branch0.targetFirst, branch1.line - 1);
                    adjacent = adjacent && !state.reverse_targets[branch1.line];
                }
                return adjacent;
            }
        }

        private static Branch combine_left(State state, Branch branch1)
        {
            if (is_conditional(branch1))
            {
                return combine_conditional(state, branch1);
            }
            else if (is_assignment(branch1) || branch1.type == Branch.Type.finalset)
            {
                return combine_assignment(state, branch1);
            }
            else
            {
                return branch1;
            }
        }

        private static Branch combine_conditional(State state, Branch branch1)
        {
            Branch branch0 = branch1.previous;
            Branch branchn = branch1;
            while (branch0 != null && branch0.line > branch1.line)
            {
                branch0 = branch0.previous;
            }
            while (branch0 != null && branchn == branch1 && adjacent(state, branch0, branch1))
            {
                branchn = combine_conditional_helper(state, branch0, branch1);
                if (branch0.targetSecond > branch1.targetFirst) break;
                branch0 = branch0.previous;
            }
            return branchn;
        }

        private static Branch combine_conditional_helper(State state, Branch branch0, Branch branch1)
        {
            if (is_conditional(branch0) && is_conditional(branch1))
            {
                int branch0TargetSecond = branch0.targetSecond;
                if (is_jmp(state, branch1.targetFirst) && state.code.target(branch1.targetFirst) == branch0TargetSecond)
                {
                    // Handle redirected target
                    branch0TargetSecond = branch1.targetFirst;
                }
                if (branch0TargetSecond == branch1.targetFirst)
                {
                    // Combination if not branch0 or branch1 then
                    branch0 = combine_conditional(state, branch0);
                    ICondition c = new OrCondition(branch0.cond.inverse(), branch1.cond);
                    Branch branchn = new Branch(branch0.line, branch1.line2, Branch.Type.comparison, c, branch1.targetFirst, branch1.targetSecond, branch1.finalset);
                    branchn.inverseValue = branch1.inverseValue;
                    if (verbose) Console.Error.WriteLine("conditional or " + branchn.line);
                    replace_branch(state, branch0, branch1, branchn);
                    return combine_conditional(state, branchn);
                }
                else if (branch0TargetSecond == branch1.targetSecond)
                {
                    // Combination if branch0 and branch1 then
                    branch0 = combine_conditional(state, branch0);
                    ICondition c = new AndCondition(branch0.cond, branch1.cond);
                    Branch branchn = new Branch(branch0.line, branch1.line2, Branch.Type.comparison, c, branch1.targetFirst, branch1.targetSecond, branch1.finalset);
                    branchn.inverseValue = branch1.inverseValue;
                    if (verbose) Console.Error.WriteLine("conditional and " + branchn.line);
                    replace_branch(state, branch0, branch1, branchn);
                    return combine_conditional(state, branchn);
                }
            }
            return branch1;
        }

        private static Branch combine_assignment(State state, Branch branch1)
        {
            Branch branch0 = branch1.previous;
            Branch branchn = branch1;
            while (branch0 != null && branchn == branch1)
            {
                branchn = combine_assignment_helper(state, branch0, branch1);
                if (branch1.cond == branch1.finalset)
                {
                    // keep searching for the first branch paired with a raw finalset
                }
                else if (branch0.cond == branch0.finalset)
                {
                    // ignore duped finalset
                }
                else if (branch0.targetSecond > branch1.targetFirst)
                {
                    break;
                }
                branch0 = branch0.previous;
            }
            return branchn;
        }

        private static Branch combine_assignment_helper(State state, Branch branch0, Branch branch1)
        {
            if (adjacent(state, branch0, branch1))
            {
                int register = branch1.target;
                if (branch1.target == -1)
                {
                    throw new System.InvalidOperationException();
                }
                //Console.Error.WriteLine("blah " + branch1.line + " " + branch0.line);
                if (is_conditional(branch0) && is_assignment(branch1))
                {
                    //Console.Error.WriteLine("bridge cand " + branch1.line + " " + branch0.line);
                    if (branch0.targetSecond == branch1.targetFirst)
                    {
                        bool inverse = branch0.inverseValue;
                        if (verbose) Console.Error.WriteLine("bridge " + (inverse ? "or" : "and") + " " + branch1.line + " " + branch0.line);
                        branch0 = combine_conditional(state, branch0);
                        if (inverse != branch0.inverseValue) throw new System.InvalidOperationException();
                        ICondition c;
                        if (!branch1.inverseValue)
                        {
                            //Console.Error.WriteLine("bridge or " + branch0.line + " " + branch0.inverseValue);
                            c = new OrCondition(branch0.cond.inverse(), branch1.cond);
                        }
                        else
                        {
                            //Console.Error.WriteLine("bridge and " + branch0.line + " " + branch0.inverseValue);
                            c = new AndCondition(branch0.cond, branch1.cond);
                        }
                        Branch branchn = new Branch(branch0.line, branch1.line2, branch1.type, c, branch1.targetFirst, branch1.targetSecond, branch1.finalset);
                        branchn.inverseValue = branch1.inverseValue;
                        branchn.target = register;
                        replace_branch(state, branch0, branch1, branchn);
                        return combine_assignment(state, branchn);
                    }
                    else if (branch0.targetSecond == branch1.targetSecond)
                    {
                        /*
                        ICondition c = new AndCondition(branch0.cond, branch1.cond);
                        Branch branchn = new Branch(branch0.line, Branch.Type.comparison, c, branch1.targetFirst, branch1.targetSecond);
                        replace_branch(state, branch0, branch1, branchn);
                        return branchn;
                        */
                    }
                }

                if (is_assignment(branch0, register) && is_assignment(branch1) && branch0.inverseValue == branch1.inverseValue)
                {
                    if (branch0.targetSecond == branch1.targetSecond)
                    {
                        ICondition c;
                        //Console.Error.WriteLine("preassign " + branch1.line + " " + branch0.line + " " + branch0.targetSecond);
                        if (verbose) Console.Error.WriteLine("assign " + (branch0.inverseValue ? "or" : "and") + " " + branch1.line + " " + branch0.line);
                        if (is_conditional(branch0))
                        {
                            branch0 = combine_conditional(state, branch0);
                            if (branch0.inverseValue)
                            {
                                branch0.cond = branch0.cond.inverse(); // inverse has been double handled; undo it
                            }
                        }
                        else
                        {
                            bool inverse = branch0.inverseValue;
                            branch0 = combine_assignment(state, branch0);
                            if (inverse != branch0.inverseValue) throw new System.InvalidOperationException();
                        }
                        if (branch0.inverseValue)
                        {
                            //Console.Error.WriteLine("assign and " + branch1.line + " " + branch0.line);
                            c = new OrCondition(branch0.cond, branch1.cond);
                        }
                        else
                        {
                            //Console.Error.WriteLine("assign or " + branch1.line + " " + branch0.line);
                            c = new AndCondition(branch0.cond, branch1.cond);
                        }
                        Branch branchn = new Branch(branch0.line, branch1.line2, branch1.type, c, branch1.targetFirst, branch1.targetSecond, branch1.finalset);
                        branchn.inverseValue = branch1.inverseValue;
                        branchn.target = register;
                        replace_branch(state, branch0, branch1, branchn);
                        return combine_assignment(state, branchn);
                    }
                }
                if (is_assignment(branch0, register) && branch1.type == Branch.Type.finalset)
                {
                    if (branch0.targetSecond == branch1.targetSecond)
                    {
                        ICondition c;
                        //Console.Error.WriteLine("readonly preassign " + branch1.line + " " + branch0.line);
                        if (branch0.finalset != null && branch0.finalset != branch1.finalset)
                        {
                            Branch b = branch0.next;
                            while (b != null)
                            {
                                if (b.cond == branch0.finalset)
                                {
                                    remove_branch(state, b);
                                    break;
                                }
                                b = b.next;
                            }
                        }

                        if (is_conditional(branch0))
                        {
                            branch0 = combine_conditional(state, branch0);
                            if (branch0.inverseValue)
                            {
                                branch0.cond = branch0.cond.inverse(); // inverse has been double handled; undo it
                            }
                        }
                        else
                        {
                            bool inverse = branch0.inverseValue;
                            branch0 = combine_assignment(state, branch0);
                            if (inverse != branch0.inverseValue) throw new System.InvalidOperationException();
                        }
                        if (verbose) Console.Error.WriteLine("readonly assign " + (branch0.inverseValue ? "or" : "and") + " " + branch1.line + " " + branch0.line);

                        if (branch0.inverseValue)
                        {
                            //Console.Error.WriteLine("readonly assign or " + branch1.line + " " + branch0.line);
                            c = new OrCondition(branch0.cond, branch1.cond);
                        }
                        else
                        {
                            //Console.Error.WriteLine("readonly assign and " + branch1.line + " " + branch0.line);
                            c = new AndCondition(branch0.cond, branch1.cond);
                        }
                        Branch branchn = new Branch(branch0.line, branch1.line2, Branch.Type.finalset, c, branch1.targetFirst, branch1.targetSecond, branch1.finalset);
                        branchn.target = register;
                        replace_branch(state, branch0, branch1, branchn);
                        return combine_assignment(state, branchn);
                    }
                }
            }
            return branch1;
        }

        private static void raw_add_branch(State state, Branch b)
        {
            if (b.type == Branch.Type.finalset)
            {
                List<Branch> list = state.finalsetbranches[b.line];
                if (list == null)
                {
                    list = new List<Branch>();
                    state.finalsetbranches[b.line] = list;
                }
                list.Add(b);
            }
            else if (b.type == Branch.Type.testset)
            {
                state.setbranches[b.line] = b;
            }
            else
            {
                state.branches[b.line] = b;
            }
        }

        private static void raw_remove_branch(State state, Branch b)
        {
            if (b.type == Branch.Type.finalset)
            {
                List<Branch> list = state.finalsetbranches[b.line];
                if (list == null)
                {
                    throw new System.InvalidOperationException();
                }
                list.Remove(b);
            }
            else if (b.type == Branch.Type.testset)
            {
                state.setbranches[b.line] = null;
            }
            else
            {
                state.branches[b.line] = null;
            }
        }

        private static void replace_branch(State state, Branch branch0, Branch branch1, Branch branchn)
        {
            remove_branch(state, branch0);
            raw_remove_branch(state, branch1);
            branchn.previous = branch1.previous;
            if (branchn.previous == null)
            {
                state.begin_branch = branchn;
            }
            else
            {
                branchn.previous.next = branchn;
            }
            branchn.next = branch1.next;
            if (branchn.next == null)
            {
                state.end_branch = branchn;
            }
            else
            {
                branchn.next.previous = branchn;
            }
            raw_add_branch(state, branchn);
        }

        private static void remove_branch(State state, Branch b)
        {
            raw_remove_branch(state, b);
            Branch prev = b.previous;
            Branch next = b.next;
            if (prev != null)
            {
                prev.next = next;
            }
            else
            {
                state.begin_branch = next;
            }
            if (next != null)
            {
                next.previous = prev;
            }
            else
            {
                state.end_branch = prev;
            }
        }

        private static void insert_branch(State state, Branch b)
        {
            raw_add_branch(state, b);
        }

        private static void link_branches(State state)
        {
            Branch previous = null;
            for (int index = 0; index < state.branches.Length; index++)
            {
                for (int array = 0; array < 3; array++)
                {
                    if (array == 0)
                    {
                        List<Branch> list = state.finalsetbranches[index];
                        if (list != null)
                        {
                            foreach (Branch b in list)
                            {
                                b.previous = previous;
                                if (previous != null)
                                {
                                    previous.next = b;
                                }
                                else
                                {
                                    state.begin_branch = b;
                                }
                                previous = b;
                            }
                        }
                    }
                    else
                    {
                        Branch[] branches;
                        if (array == 1)
                        {
                            branches = state.setbranches;
                        }
                        else
                        {
                            branches = state.branches;
                        }
                        Branch b = branches[index];
                        if (b != null)
                        {
                            b.previous = previous;
                            if (previous != null)
                            {
                                previous.next = b;
                            }
                            else
                            {
                                state.begin_branch = b;
                            }
                            previous = b;
                        }
                    }
                }
            }
            state.end_branch = previous;
        }

        private static bool is_jmp_raw(State state, int line)
        {
            Op op = state.code.GetOp(line);
            return op == Op.JMP || op == Op.JMP52 || op == Op.JMP54;
        }

        private static bool is_jmp(State state, int line)
        {
            Code code = state.code;
            Op op = code.GetOp(line);
            if (op == Op.JMP || op == Op.JMP54)
            {
                return true;
            }
            else if (op == Op.JMP52)
            {
                return !is_close(state, line);
            }
            else
            {
                return false;
            }
        }

        private static bool is_close(State state, int line)
        {
            Code code = state.code;
            Op op = code.GetOp(line);
            if (op == Op.CLOSE)
            {
                return true;
            }
            else if (op == Op.JMP52)
            {
                int target = code.target(line);
                if (target == line + 1)
                {
                    return code.AField(line) != 0;
                }
                else
                {
                    if (line + 1 <= code.Length && code.GetOp(line + 1) == Op.JMP52)
                    {
                        return target == code.target(line + 1) && code.AField(line) != 0;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        private static int get_close_value(State state, int line)
        {
            Code code = state.code;
            Op op = code.GetOp(line);
            if (op == Op.CLOSE)
            {
                return code.AField(line);
            }
            else if (op == Op.JMP52)
            {
                return code.AField(line) - 1;
            }
            else
            {
                throw new System.InvalidOperationException();
            }
        }

        private static CloseType get_close_type(State state, int line)
        {
            if (line < 1 || !is_close(state, line))
            {
                return CloseType.NONE;
            }
            else
            {
                Op op = state.code.GetOp(line);
                if (op == Op.CLOSE)
                {
                    return state.function.header.version.closeSemantics.Value == Version.CloseSemantics.Lua54 ? CloseType.CLOSE54 : CloseType.CLOSE;
                }
                else
                {
                    return CloseType.JMP;
                }
            }
        }

        private static bool has_statement(State state, int begin, int end)
        {
            for (int line = begin; line <= end; line++)
            {
                if (is_statement(state, line))
                {
                    return true;
                }
            }
            return state.d.hasStatement(begin, end);
        }

        private static bool is_statement(State state, int line)
        {
            if (state.reverse_targets[line]) return true;
            Registers r = state.r;
            if (r.getNewLocals(line).Count != 0) return true;
            Code code = state.code;
            if (code.isUpvalueDeclaration(line)) return false;
            switch (code.GetOp(line).Type)
            {
                case Op.OpT.MOVE:
                case Op.OpT.LOADI:
                case Op.OpT.LOADF:
                case Op.OpT.LOADK:
                case Op.OpT.LOADKX:
                case Op.OpT.LOADBOOL:
                case Op.OpT.LOADFALSE:
                case Op.OpT.LOADTRUE:
                case Op.OpT.LFALSESKIP:
                case Op.OpT.GETGLOBAL:
                case Op.OpT.GETUPVAL:
                case Op.OpT.GETTABUP:
                case Op.OpT.GETTABUP54:
                case Op.OpT.GETTABLE:
                case Op.OpT.GETTABLE54:
                case Op.OpT.GETI:
                case Op.OpT.GETFIELD:
                case Op.OpT.NEWTABLE50:
                case Op.OpT.NEWTABLE:
                case Op.OpT.NEWTABLE54:
                case Op.OpT.ADD:
                case Op.OpT.SUB:
                case Op.OpT.MUL:
                case Op.OpT.DIV:
                case Op.OpT.IDIV:
                case Op.OpT.MOD:
                case Op.OpT.POW:
                case Op.OpT.BAND:
                case Op.OpT.BOR:
                case Op.OpT.BXOR:
                case Op.OpT.SHL:
                case Op.OpT.SHR:
                case Op.OpT.UNM:
                case Op.OpT.NOT:
                case Op.OpT.LEN:
                case Op.OpT.BNOT:
                case Op.OpT.CONCAT:
                case Op.OpT.CONCAT54:
                case Op.OpT.CLOSURE:
                case Op.OpT.TESTSET:
                case Op.OpT.TESTSET54:
                    return r.isLocal(code.AField(line), line);
                case Op.OpT.ADD54:
                case Op.OpT.SUB54:
                case Op.OpT.MUL54:
                case Op.OpT.DIV54:
                case Op.OpT.IDIV54:
                case Op.OpT.MOD54:
                case Op.OpT.POW54:
                case Op.OpT.BAND54:
                case Op.OpT.BOR54:
                case Op.OpT.BXOR54:
                case Op.OpT.SHL54:
                case Op.OpT.SHR54:
                case Op.OpT.ADDK:
                case Op.OpT.SUBK:
                case Op.OpT.MULK:
                case Op.OpT.DIVK:
                case Op.OpT.IDIVK:
                case Op.OpT.MODK:
                case Op.OpT.POWK:
                case Op.OpT.BANDK:
                case Op.OpT.BORK:
                case Op.OpT.BXORK:
                case Op.OpT.ADDI:
                case Op.OpT.SHLI:
                case Op.OpT.SHRI:
                    return false; // only count following MMBIN* instruction
                case Op.OpT.MMBIN:
                case Op.OpT.MMBINI:
                case Op.OpT.MMBINK:
                    if (line <= 1) throw new System.InvalidOperationException();
                    return r.isLocal(code.AField(line - 1), line - 1);
                case Op.OpT.LOADNIL:
                    for (int register = code.AField(line); register <= code.BField(line); register++)
                    {
                        if (r.isLocal(register, line))
                        {
                            return true;
                        }
                    }
                    return false;
                case Op.OpT.LOADNIL52:
                    for (int register = code.AField(line); register <= code.AField(line) + code.BField(line); register++)
                    {
                        if (r.isLocal(register, line))
                        {
                            return true;
                        }
                    }
                    return false;
                case Op.OpT.SETGLOBAL:
                case Op.OpT.SETUPVAL:
                case Op.OpT.SETTABUP:
                case Op.OpT.SETTABUP54:
                case Op.OpT.TAILCALL:
                case Op.OpT.TAILCALL54:
                case Op.OpT.RETURN:
                case Op.OpT.RETURN54:
                case Op.OpT.RETURN0:
                case Op.OpT.RETURN1:
                case Op.OpT.FORLOOP:
                case Op.OpT.FORLOOP54:
                case Op.OpT.FORPREP:
                case Op.OpT.FORPREP54:
                case Op.OpT.TFORCALL:
                case Op.OpT.TFORCALL54:
                case Op.OpT.TFORLOOP:
                case Op.OpT.TFORLOOP52:
                case Op.OpT.TFORLOOP54:
                case Op.OpT.TFORPREP:
                case Op.OpT.TFORPREP54:
                case Op.OpT.CLOSE:
                case Op.OpT.TBC: // TODO: ?
                    return true;
                case Op.OpT.TEST50:
                    return code.AField(line) != code.BField(line) && r.isLocal(code.AField(line), line);
                case Op.OpT.SELF:
                case Op.OpT.SELF54:
                    return r.isLocal(code.AField(line), line) || r.isLocal(code.AField(line) + 1, line);
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
                case Op.OpT.SETLIST50:
                case Op.OpT.SETLISTO:
                case Op.OpT.SETLIST:
                case Op.OpT.SETLIST52:
                case Op.OpT.SETLIST54:
                case Op.OpT.VARARGPREP:
                case Op.OpT.EXTRAARG:
                case Op.OpT.EXTRABYTE:
                    return false;
                case Op.OpT.JMP:
                case Op.OpT.JMP52: // TODO: CLOSE?
                case Op.OpT.JMP54:
                    if (line == 1)
                    {
                        return true;
                    }
                    else
                    {
                        Op prev = line >= 2 ? code.GetOp(line - 1) : null;
                        Op next = line + 1 <= code.Length ? code.GetOp(line + 1) : null;
                        if (prev == Op.EQ) return false;
                        if (prev == Op.LT) return false;
                        if (prev == Op.LE) return false;
                        if (prev == Op.EQ54) return false;
                        if (prev == Op.LT54) return false;
                        if (prev == Op.LE54) return false;
                        if (prev == Op.EQK) return false;
                        if (prev == Op.EQI) return false;
                        if (prev == Op.LTI) return false;
                        if (prev == Op.LEI) return false;
                        if (prev == Op.GTI) return false;
                        if (prev == Op.GEI) return false;
                        if (prev == Op.TEST50) return false;
                        if (prev == Op.TEST) return false;
                        if (prev == Op.TEST54) return false;
                        if (prev == Op.TESTSET) return false;
                        if (prev == Op.TESTSET54) return false;
                        if (next == Op.LOADBOOL && code.CField(line + 1) != 0) return false;
                        if (next == Op.LFALSESKIP) return false;
                        return true;
                    }
                case Op.OpT.CALL:
                    {
                        int a = code.AField(line);
                        int c = code.CField(line);
                        if (c == 1)
                        {
                            return true;
                        }
                        if (c == 0) c = r.registers - a + 1;
                        for (int register = a; register < a + c - 1; register++)
                        {
                            if (r.isLocal(register, line))
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                case Op.OpT.VARARG:
                    {
                        int a = code.AField(line);
                        int b = code.BField(line);
                        if (b == 0) b = r.registers - a + 1;
                        for (int register = a; register < a + b - 1; register++)
                        {
                            if (r.isLocal(register, line))
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                case Op.OpT.VARARG54:
                    {
                        int a = code.AField(line);
                        int c = code.CField(line);
                        if (c == 0) c = r.registers - a + 1;
                        for (int register = a; register < a + c - 1; register++)
                        {
                            if (r.isLocal(register, line))
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                case Op.OpT.SETTABLE:
                case Op.OpT.SETTABLE54:
                case Op.OpT.SETI:
                case Op.OpT.SETFIELD:
                    // special case -- this is actually ambiguous and must be resolved by the decompiler check
                    return false;
                case Op.OpT.DEFAULT:
                case Op.OpT.DEFAULT54:
                    throw new System.InvalidOperationException();
            }
            throw new System.InvalidOperationException("Illegal opcode: " + code.GetOp(line));
        }

        // static only
        private ControlFlowHandler()
        {
        }

    }

}
