using LuaDec.Parser;
using System;
using System.Text;

namespace LuaDec.Decompile
{
    public static class OpV
    {
        public static readonly int Lua50 = 1;
        public static readonly int Lua51 = 2;
        public static readonly int Lua52 = 4;
        public static readonly int Lua53 = 8;
        public static readonly int Lua54 = 16;
    }

    public class Op
    {
        public enum OpT
        {
            MOVE,
            LOADK,
            LOADBOOL,
            LOADNIL,
            GETUPVALUE,
            GETGLOBAL,
            GETTABLE,
            SETGLOBAL,
            SETUPVALUE,
            SETTABLE,
            NEWTABLE,
            SELF,
            ADD,
            SUB,
            MUL,
            DIV,
            MOD,
            POW,
            UNM,
            NOT,
            LEN,
            CONCAT,
            JMP,
            EQ,
            LT,
            LE,
            TEST,
            TESTSET,
            CALL,
            TAILCALL,
            RETURN,
            FORLOOP,
            FORPREP,
            TFORLOOP,
            SETLIST,
            CLOSE,
            CLOSURE,
            VARARG,
            JMP52,
            LOADNIL52,
            LOADKX,
            GETTABUP,
            SETTABUP,
            SETLIST52,
            TFORCALL,
            TFORLOOP52,
            EXTRAARG,
            NEWTABLE50,
            SETLIST50,
            SETLISTO,
            TFORPREP,
            TEST50,
            IDIV,
            BAND,
            BOR,
            BXOR,
            SHL,
            SHR,
            BNOT,
            LOADI,
            LOADF,
            LOADFALSE,
            LFALSESKIP,
            LOADTRUE,
            GETTABUP54,
            GETTABLE54,
            GETI,
            GETFIELD,
            SETTABUP54,
            SETTABLE54,
            SETI,
            SETFIELD,
            NEWTABLE54,
            SELF54,
            ADDI,
            ADDK,
            SUBK,
            MULK,
            MODK,
            POWK,
            DIVK,
            IDIVK,
            BANDK,
            BORK,
            BXORK,
            SHRI,
            SHLI,
            ADD54,
            SUB54,
            MUL54,
            MOD54,
            POW54,
            DIV54,
            IDIV54,
            BAND54,
            BOR54,
            BXOR54,
            SHL54,
            SHR54,
            MMBIN,
            MMBINI,
            MMBINK,
            CONCAT54,
            TBC,
            JMP54,
            EQ54,
            LT54,
            LE54,
            EQK,
            EQI,
            LTI,
            LEI,
            GTI,
            GEI,
            TEST54,
            TESTSET54,
            TAILCALL54,
            RETURN54,
            RETURN0,
            RETURN1,
            FORLOOP54,
            FORPREP54,
            TFORPREP54,
            TFORCALL54,
            TFORLOOP54,
            SETLIST54,
            VARARG54,
            VARARGPREP,
            EXTRABYTE,
            DEFAULT,
            DEFAULT54,
        }

        #region public static readonly Op

        public static readonly Op ADD = new Op(OpT.ADD, "add", OpV.Lua50 | OpV.Lua51 | OpV.Lua52 | OpV.Lua53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);

        public static readonly Op ADD54 = new Op(OpT.ADD54, "add", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CR);

        public static readonly Op ADDI = new Op(OpT.ADDI, "addi", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CsI);

        public static readonly Op ADDK = new Op(OpT.ADDK, "addk", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CK);

        public static readonly Op BAND = new Op(OpT.BAND, "band", OpV.Lua53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);

        public static readonly Op BAND54 = new Op(OpT.BAND54, "band", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CR);

        public static readonly Op BANDK = new Op(OpT.BANDK, "bandk", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CKI);

        public static readonly Op BNOT = new Op(OpT.BNOT, "bnot", OpV.Lua53 | OpV.Lua54, OperandFormat.AR, OperandFormat.BR);

        public static readonly Op BOR = new Op(OpT.BOR, "bor", OpV.Lua53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);

        public static readonly Op BOR54 = new Op(OpT.BOR54, "bor", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CR);

        public static readonly Op BORK = new Op(OpT.BORK, "bork", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CKI);

        public static readonly Op BXOR = new Op(OpT.BXOR, "bxor", OpV.Lua53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);

        public static readonly Op BXOR54 = new Op(OpT.BXOR54, "bxor", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CR);

        public static readonly Op BXORK = new Op(OpT.BXORK, "bxork", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CKI);

        public static readonly Op CALL = new Op(OpT.CALL, "call", OpV.Lua50 | OpV.Lua51 | OpV.Lua52 | OpV.Lua53 | OpV.Lua54, OperandFormat.AR, OperandFormat.B, OperandFormat.C);

        public static readonly Op CLOSE = new Op(OpT.CLOSE, "close", OpV.Lua50 | OpV.Lua51 | OpV.Lua54, OperandFormat.AR);

        public static readonly Op CLOSURE = new Op(OpT.CLOSURE, "closure", OpV.Lua50 | OpV.Lua51 | OpV.Lua52 | OpV.Lua53 | OpV.Lua54, OperandFormat.AR, OperandFormat.BxF);

        public static readonly Op CONCAT = new Op(OpT.CONCAT, "concat", OpV.Lua50 | OpV.Lua51 | OpV.Lua52 | OpV.Lua53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);

        public static readonly Op CONCAT54 = new Op(OpT.CONCAT54, "concat", OpV.Lua54, OperandFormat.AR, OperandFormat.B);

        public static readonly Op DEFAULT = new Op(OpT.DEFAULT, "default", 0, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);

        public static readonly Op DEFAULT54 = new Op(OpT.DEFAULT54, "default", 0, OperandFormat.AR, OperandFormat.BR, OperandFormat.C, OperandFormat.k);

        public static readonly Op DIV = new Op(OpT.DIV, "div", OpV.Lua50 | OpV.Lua51 | OpV.Lua52 | OpV.Lua53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);

        public static readonly Op DIV54 = new Op(OpT.DIV54, "div", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CR);

        public static readonly Op DIVK = new Op(OpT.DIVK, "divk", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CK);

        public static readonly Op EQ = new Op(OpT.EQ, "eq", OpV.Lua50 | OpV.Lua51 | OpV.Lua52 | OpV.Lua53, OperandFormat.A, OperandFormat.BRK, OperandFormat.CRK);

        public static readonly Op EQ54 = new Op(OpT.EQ54, "eq", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.k, OperandFormat.C);

        public static readonly Op EQI = new Op(OpT.EQI, "eqi", OpV.Lua54, OperandFormat.AR, OperandFormat.BsI, OperandFormat.k, OperandFormat.C);

        public static readonly Op EQK = new Op(OpT.EQK, "eqk", OpV.Lua54, OperandFormat.AR, OperandFormat.BK, OperandFormat.k, OperandFormat.C);

        public static readonly Op EXTRAARG = new Op(OpT.EXTRAARG, "extraarg", OpV.Lua52 | OpV.Lua53 | OpV.Lua54, OperandFormat.Ax);

        // Special
        public static readonly Op EXTRABYTE = new Op(OpT.EXTRABYTE, "extrabyte", OpV.Lua50 | OpV.Lua51 | OpV.Lua52 | OpV.Lua53 | OpV.Lua54, OperandFormat.x);

        public static readonly Op FORLOOP = new Op(OpT.FORLOOP, "forloop", OpV.Lua50 | OpV.Lua51 | OpV.Lua52 | OpV.Lua53, OperandFormat.AR, OperandFormat.sBxJ);

        public static readonly Op FORLOOP54 = new Op(OpT.FORLOOP54, "forloop", OpV.Lua54, OperandFormat.AR, OperandFormat.BxJn);

        public static readonly Op FORPREP = new Op(OpT.FORPREP, "forprep", OpV.Lua51 | OpV.Lua52 | OpV.Lua53, OperandFormat.AR, OperandFormat.sBxJ);

        public static readonly Op FORPREP54 = new Op(OpT.FORPREP54, "forprep", OpV.Lua54, OperandFormat.AR, OperandFormat.BxJ);

        public static readonly Op GEI = new Op(OpT.GEI, "gei", OpV.Lua54, OperandFormat.AR, OperandFormat.BsI, OperandFormat.k, OperandFormat.C);

        public static readonly Op GETFIELD = new Op(OpT.GETFIELD, "getfield", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CKS);

        public static readonly Op GETGLOBAL = new Op(OpT.GETGLOBAL, "getglobal", OpV.Lua50 | OpV.Lua51, OperandFormat.AR, OperandFormat.BxK);

        public static readonly Op GETI = new Op(OpT.GETI, "geti", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CI);

        public static readonly Op GETTABLE = new Op(OpT.GETTABLE, "gettable", OpV.Lua50 | OpV.Lua51 | OpV.Lua52 | OpV.Lua53, OperandFormat.AR, OperandFormat.BR, OperandFormat.CRK);

        public static readonly Op GETTABLE54 = new Op(OpT.GETTABLE54, "gettable", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CR);

        public static readonly Op GETTABUP = new Op(OpT.GETTABUP, "gettabup", OpV.Lua52 | OpV.Lua53, OperandFormat.AR, OperandFormat.BU, OperandFormat.CRK);

        public static readonly Op GETTABUP54 = new Op(OpT.GETTABUP54, "gettabup", OpV.Lua54, OperandFormat.AR, OperandFormat.BU, OperandFormat.CKS);

        public static readonly Op GETUPVAL = new Op(OpT.GETUPVALUE, "getupval", OpV.Lua50 | OpV.Lua51 | OpV.Lua52 | OpV.Lua53 | OpV.Lua54, OperandFormat.AR, OperandFormat.BU);

        public static readonly Op GTI = new Op(OpT.GTI, "gti", OpV.Lua54, OperandFormat.AR, OperandFormat.BsI, OperandFormat.k, OperandFormat.C);

        // Lua 5.3 Opcodes
        public static readonly Op IDIV = new Op(OpT.IDIV, "idiv", OpV.Lua53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);

        public static readonly Op IDIV54 = new Op(OpT.IDIV54, "idiv", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CR);

        public static readonly Op IDIVK = new Op(OpT.IDIVK, "idivk", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CK);

        public static readonly Op JMP = new Op(OpT.JMP, "jmp", OpV.Lua50 | OpV.Lua51, OperandFormat.sBxJ);

        // Lua 5.2 Opcodes
        public static readonly Op JMP52 = new Op(OpT.JMP52, "jmp", OpV.Lua52 | OpV.Lua53, OperandFormat.A, OperandFormat.sBxJ);

        public static readonly Op JMP54 = new Op(OpT.JMP54, "jmp", OpV.Lua54, OperandFormat.sJ);

        public static readonly Op LE = new Op(OpT.LE, "le", OpV.Lua50 | OpV.Lua51 | OpV.Lua52 | OpV.Lua53, OperandFormat.A, OperandFormat.BRK, OperandFormat.CRK);

        public static readonly Op LE54 = new Op(OpT.LE54, "le", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.k, OperandFormat.C);

        public static readonly Op LEI = new Op(OpT.LEI, "lei", OpV.Lua54, OperandFormat.AR, OperandFormat.BsI, OperandFormat.k, OperandFormat.C);

        public static readonly Op LEN = new Op(OpT.LEN, "len", OpV.Lua51 | OpV.Lua52 | OpV.Lua53, OperandFormat.AR, OperandFormat.BR);

        public static readonly Op LFALSESKIP = new Op(OpT.LFALSESKIP, "lfalseskip", OpV.Lua54, OperandFormat.AR);

        public static readonly Op LOADBOOL = new Op(OpT.LOADBOOL, "loadbool", OpV.Lua50 | OpV.Lua51 | OpV.Lua52 | OpV.Lua53, OperandFormat.AR, OperandFormat.B, OperandFormat.C);

        public static readonly Op LOADF = new Op(OpT.LOADF, "loadf", OpV.Lua54, OperandFormat.AR, OperandFormat.sBxF);

        public static readonly Op LOADFALSE = new Op(OpT.LOADFALSE, "loadfalse", OpV.Lua54, OperandFormat.AR);

        // Lua 5.4 Opcodes
        public static readonly Op LOADI = new Op(OpT.LOADI, "loadi", OpV.Lua54, OperandFormat.AR, OperandFormat.sBxI);

        public static readonly Op LOADK = new Op(OpT.LOADK, "loadk", OpV.Lua50 | OpV.Lua51 | OpV.Lua52 | OpV.Lua53 | OpV.Lua54, OperandFormat.AR, OperandFormat.BxK);

        public static readonly Op LOADKX = new Op(OpT.LOADKX, "loadkx", OpV.Lua52 | OpV.Lua53 | OpV.Lua54, OperandFormat.AR);

        public static readonly Op LOADNIL = new Op(OpT.LOADNIL, "loadnil", OpV.Lua50 | OpV.Lua51, OperandFormat.AR, OperandFormat.BR);

        public static readonly Op LOADNIL52 = new Op(OpT.LOADNIL52, "loadnil", OpV.Lua52 | OpV.Lua53 | OpV.Lua54, OperandFormat.AR, OperandFormat.B);

        public static readonly Op LOADTRUE = new Op(OpT.LOADTRUE, "loadtrue", OpV.Lua54, OperandFormat.AR);

        public static readonly Op LT = new Op(OpT.LT, "lt", OpV.Lua50 | OpV.Lua51 | OpV.Lua52 | OpV.Lua53, OperandFormat.A, OperandFormat.BRK, OperandFormat.CRK);

        public static readonly Op LT54 = new Op(OpT.LT54, "lt", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.k, OperandFormat.C);

        public static readonly Op LTI = new Op(OpT.LTI, "lti", OpV.Lua54, OperandFormat.AR, OperandFormat.BsI, OperandFormat.k, OperandFormat.C);

        public static readonly Op MMBIN = new Op(OpT.MMBIN, "mmbin", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.C);

        public static readonly Op MMBINI = new Op(OpT.MMBINI, "mmbini", OpV.Lua54, OperandFormat.AR, OperandFormat.BsI, OperandFormat.C, OperandFormat.k);

        public static readonly Op MMBINK = new Op(OpT.MMBINK, "mmbink", OpV.Lua54, OperandFormat.AR, OperandFormat.BK, OperandFormat.C, OperandFormat.k);

        public static readonly Op MOD = new Op(OpT.MOD, "mod", OpV.Lua51 | OpV.Lua52 | OpV.Lua53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);

        public static readonly Op MOD54 = new Op(OpT.MOD54, "mod", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CR);

        public static readonly Op MODK = new Op(OpT.MODK, "modk", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CK);

        // Lua 5.1 Opcodes
        public static readonly Op MOVE = new Op(OpT.MOVE, "move", OpV.Lua50 | OpV.Lua51 | OpV.Lua52 | OpV.Lua53 | OpV.Lua54, OperandFormat.AR, OperandFormat.BR);

        public static readonly Op MUL = new Op(OpT.MUL, "mul", OpV.Lua50 | OpV.Lua51 | OpV.Lua52 | OpV.Lua53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);
        public static readonly Op MUL54 = new Op(OpT.MUL54, "mul", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CR);
        public static readonly Op MULK = new Op(OpT.MULK, "mulk", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CK);
        public static readonly Op NEWTABLE = new Op(OpT.NEWTABLE, "newtable", OpV.Lua51 | OpV.Lua52 | OpV.Lua53, OperandFormat.AR, OperandFormat.B, OperandFormat.C);

        // Lua 5.0 Opcodes
        public static readonly Op NEWTABLE50 = new Op(OpT.NEWTABLE50, "newtable", OpV.Lua50, OperandFormat.AR, OperandFormat.B, OperandFormat.C);

        public static readonly Op NEWTABLE54 = new Op(OpT.NEWTABLE54, "newtable", OpV.Lua54, OperandFormat.AR, OperandFormat.B, OperandFormat.C, OperandFormat.k);
        public static readonly Op NOT = new Op(OpT.NOT, "not", OpV.Lua50 | OpV.Lua51 | OpV.Lua52 | OpV.Lua53, OperandFormat.AR, OperandFormat.BR);
        public static readonly Op POW = new Op(OpT.POW, "pow", OpV.Lua50 | OpV.Lua51 | OpV.Lua52 | OpV.Lua53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);
        public static readonly Op POW54 = new Op(OpT.POW54, "pow", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CR);
        public static readonly Op POWK = new Op(OpT.POWK, "powk", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CK);
        public static readonly Op RETURN = new Op(OpT.RETURN, "return", OpV.Lua50 | OpV.Lua51 | OpV.Lua52 | OpV.Lua53, OperandFormat.AR, OperandFormat.B);
        public static readonly Op RETURN0 = new Op(OpT.RETURN0, "return0", OpV.Lua54, OperandFormat.AR, OperandFormat.B, OperandFormat.C, OperandFormat.k);
        public static readonly Op RETURN1 = new Op(OpT.RETURN1, "return1", OpV.Lua54, OperandFormat.AR, OperandFormat.B, OperandFormat.C, OperandFormat.k);
        public static readonly Op RETURN54 = new Op(OpT.RETURN54, "return", OpV.Lua54, OperandFormat.AR, OperandFormat.B, OperandFormat.C, OperandFormat.k);
        public static readonly Op SELF = new Op(OpT.SELF, "self", OpV.Lua50 | OpV.Lua51 | OpV.Lua52 | OpV.Lua53, OperandFormat.AR, OperandFormat.BR, OperandFormat.CRK);
        public static readonly Op SELF54 = new Op(OpT.SELF54, "self", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CRK54);
        public static readonly Op SETFIELD = new Op(OpT.SETFIELD, "setfield", OpV.Lua54, OperandFormat.AR, OperandFormat.BKS, OperandFormat.CRK54);
        public static readonly Op SETGLOBAL = new Op(OpT.SETGLOBAL, "setglobal", OpV.Lua50 | OpV.Lua51, OperandFormat.AR, OperandFormat.BxK);
        public static readonly Op SETI = new Op(OpT.SETI, "seti", OpV.Lua54, OperandFormat.AR, OperandFormat.BI, OperandFormat.CRK54);
        public static readonly Op SETLIST = new Op(OpT.SETLIST, "setlist", OpV.Lua51, OperandFormat.AR, OperandFormat.B, OperandFormat.C);
        public static readonly Op SETLIST50 = new Op(OpT.SETLIST50, "setlist", OpV.Lua50, OperandFormat.AR, OperandFormat.Bx);
        public static readonly Op SETLIST52 = new Op(OpT.SETLIST52, "setlist", OpV.Lua52 | OpV.Lua53, OperandFormat.AR, OperandFormat.B, OperandFormat.C);
        public static readonly Op SETLIST54 = new Op(OpT.SETLIST54, "setlist", OpV.Lua54, OperandFormat.AR, OperandFormat.B, OperandFormat.C, OperandFormat.k);
        public static readonly Op SETLISTO = new Op(OpT.SETLISTO, "setlisto", OpV.Lua50, OperandFormat.AR, OperandFormat.Bx);
        public static readonly Op SETTABLE = new Op(OpT.SETTABLE, "settable", OpV.Lua50 | OpV.Lua51 | OpV.Lua52 | OpV.Lua53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);
        public static readonly Op SETTABLE54 = new Op(OpT.SETTABLE54, "settable", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CRK54);
        public static readonly Op SETTABUP = new Op(OpT.SETTABUP, "settabup", OpV.Lua52 | OpV.Lua53, OperandFormat.AU, OperandFormat.BRK, OperandFormat.CRK);
        public static readonly Op SETTABUP54 = new Op(OpT.SETTABUP54, "settabup", OpV.Lua54, OperandFormat.AU, OperandFormat.BK, OperandFormat.CRK54);
        public static readonly Op SETUPVAL = new Op(OpT.SETUPVALUE, "setupval", OpV.Lua50 | OpV.Lua51 | OpV.Lua52 | OpV.Lua53 | OpV.Lua54, OperandFormat.AR, OperandFormat.BU);
        public static readonly Op SHL = new Op(OpT.SHL, "shl", OpV.Lua53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);
        public static readonly Op SHL54 = new Op(OpT.SHL54, "shl", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CR);
        public static readonly Op SHLI = new Op(OpT.SHLI, "shli", OpV.Lua54, OperandFormat.AR, OperandFormat.CsI, OperandFormat.BR);
        public static readonly Op SHR = new Op(OpT.SHR, "shr", OpV.Lua53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);
        public static readonly Op SHR54 = new Op(OpT.SHR54, "shr", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CR);
        public static readonly Op SHRI = new Op(OpT.SHRI, "shri", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CsI);
        public static readonly Op SUB = new Op(OpT.SUB, "sub", OpV.Lua50 | OpV.Lua51 | OpV.Lua52 | OpV.Lua53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);
        public static readonly Op SUB54 = new Op(OpT.SUB54, "sub", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CR);
        public static readonly Op SUBK = new Op(OpT.SUBK, "subk", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CK);
        public static readonly Op TAILCALL = new Op(OpT.TAILCALL, "tailcall", OpV.Lua50 | OpV.Lua51 | OpV.Lua52 | OpV.Lua53, OperandFormat.AR, OperandFormat.B);
        public static readonly Op TAILCALL54 = new Op(OpT.TAILCALL54, "tailcall", OpV.Lua54, OperandFormat.AR, OperandFormat.B, OperandFormat.C, OperandFormat.k);
        public static readonly Op TBC = new Op(OpT.TBC, "tbc", OpV.Lua54, OperandFormat.AR);
        public static readonly Op TEST = new Op(OpT.TEST, "test", OpV.Lua51 | OpV.Lua52 | OpV.Lua53, OperandFormat.AR, OperandFormat.C);
        public static readonly Op TEST50 = new Op(OpT.TEST50, "test", OpV.Lua50, OperandFormat.AR, OperandFormat.BR, OperandFormat.C);
        public static readonly Op TEST54 = new Op(OpT.TEST54, "test", OpV.Lua54, OperandFormat.AR, OperandFormat.k);
        public static readonly Op TESTSET = new Op(OpT.TESTSET, "testset", OpV.Lua51 | OpV.Lua52 | OpV.Lua53, OperandFormat.AR, OperandFormat.BR, OperandFormat.C);
        public static readonly Op TESTSET54 = new Op(OpT.TESTSET54, "testset", OpV.Lua54, OperandFormat.AR, OperandFormat.BR, OperandFormat.k);
        public static readonly Op TFORCALL = new Op(OpT.TFORCALL, "tforcall", OpV.Lua52 | OpV.Lua53, OperandFormat.AR, OperandFormat.C);
        public static readonly Op TFORCALL54 = new Op(OpT.TFORCALL54, "tforcall", OpV.Lua54, OperandFormat.AR, OperandFormat.C);
        public static readonly Op TFORLOOP = new Op(OpT.TFORLOOP, "tforloop", OpV.Lua50 | OpV.Lua51, OperandFormat.AR, OperandFormat.C);
        public static readonly Op TFORLOOP52 = new Op(OpT.TFORLOOP52, "tforloop", OpV.Lua52 | OpV.Lua53, OperandFormat.AR, OperandFormat.sBxJ);
        public static readonly Op TFORLOOP54 = new Op(OpT.TFORLOOP54, "tforloop", OpV.Lua54, OperandFormat.AR, OperandFormat.BxJn);
        public static readonly Op TFORPREP = new Op(OpT.TFORPREP, "tforprep", OpV.Lua50, OperandFormat.AR, OperandFormat.sBxJ);
        public static readonly Op TFORPREP54 = new Op(OpT.TFORPREP54, "tforprep", OpV.Lua54, OperandFormat.AR, OperandFormat.BxJ);
        public static readonly Op UNM = new Op(OpT.UNM, "unm", OpV.Lua50 | OpV.Lua51 | OpV.Lua52 | OpV.Lua53, OperandFormat.AR, OperandFormat.BR);
        public static readonly Op VARARG = new Op(OpT.VARARG, "vararg", OpV.Lua51 | OpV.Lua52 | OpV.Lua53, OperandFormat.AR, OperandFormat.B);
        public static readonly Op VARARG54 = new Op(OpT.VARARG54, "vararg", OpV.Lua54, OperandFormat.AR, OperandFormat.C);
        public static readonly Op VARARGPREP = new Op(OpT.VARARGPREP, "varargprep", OpV.Lua54, OperandFormat.A);

        #endregion public static readonly Op

        private readonly string name;
        private readonly OperandFormat[] operands;
        private readonly OpT type;
        private readonly int versions;
        public string Name => name;
        public OperandFormat[] Operands => operands;
        public OpT Type => type;
        public int Versions => versions;

        private Op(OpT type, string name, int versions, params OperandFormat[] fmts)
        {
            this.type = type;
            this.name = name;
            this.versions = versions;
            operands = fmts;
        }

        private static string ConstantOperand(int field)
        {
            return "k" + field;
        }

        private static string FixedOperand(int field)
        {
            return field.ToString();
        }

        private static string FunctionOperand(int field)
        {
            return "f" + field;
        }

        private static string RegisterOperand(int field)
        {
            return "r" + field;
        }

        private static string ToStringHelper(
            LFunction function,
            string name,
            OperandFormat[] operands,
            int codepoint,
            CodeExtract ex,
            string label,
            bool upvalue)
        {
            int constant = -1;
            int width = 10;
            StringBuilder b = new StringBuilder();
            b.Append(name);
            for (int i = 0; i < width - name.Length; i++)
            {
                b.Append(' ');
            }
            string[] parameters = new string[operands.Length];
            for (int i = 0; i < operands.Length; ++i)
            {
                CodeExtract.Field field;
                switch (operands[i].field)
                {
                    case OperandFormat.Field.A: field = ex.A; break;
                    case OperandFormat.Field.B: field = ex.B; break;
                    case OperandFormat.Field.C: field = ex.C; break;
                    case OperandFormat.Field.k: field = ex.k; break;
                    case OperandFormat.Field.Ax: field = ex.Ax; break;
                    case OperandFormat.Field.sJ: field = ex.sJ; break;
                    case OperandFormat.Field.Bx: field = ex.Bx; break;
                    case OperandFormat.Field.sBx: field = ex.sBx; break;
                    case OperandFormat.Field.x: field = ex.x; break;
                    default: throw new System.InvalidOperationException();
                }
                int x = field.Extract(codepoint);
                switch (operands[i].format)
                {
                    case OperandFormat.Format.ImmediateUInt:
                    case OperandFormat.Format.ImmediateFloat:
                    case OperandFormat.Format.Raw: parameters[i] = FixedOperand(x); break;
                    case OperandFormat.Format.ImmediateSInt: parameters[i] = FixedOperand(x - field.Max() / 2); break;
                    case OperandFormat.Format.Register: parameters[i] = RegisterOperand(x); break;
                    case OperandFormat.Format.Upvalue: parameters[i] = UpvalueOperand(x); break;
                    case OperandFormat.Format.RegisterK:
                        if (ex.IsK(x))
                        {
                            constant = ex.GetK(x);
                            parameters[i] = ConstantOperand(constant);
                        }
                        else
                        {
                            parameters[i] = RegisterOperand(x);
                        }
                        break;

                    case OperandFormat.Format.RegisterK54:
                        if (ex.k.Extract(codepoint) != 0)
                        {
                            constant = x;
                            parameters[i] = ConstantOperand(x);
                        }
                        else
                        {
                            parameters[i] = RegisterOperand(x);
                        }
                        break;

                    case OperandFormat.Format.Constant:
                    case OperandFormat.Format.ConstantNumber:
                    case OperandFormat.Format.ConstantString:
                        constant = x;
                        parameters[i] = ConstantOperand(x);
                        break;
                    case OperandFormat.Format.Function: parameters[i] = FunctionOperand(x); break;
                    case OperandFormat.Format.Jump:
                        if (label != null)
                        {
                            parameters[i] = label;
                        }
                        else
                        {
                            parameters[i] = FixedOperand(x + operands[i].offset);
                        }
                        break;

                    case OperandFormat.Format.JumpNegative:
                        if (label != null)
                        {
                            parameters[i] = label;
                        }
                        else
                        {
                            parameters[i] = FixedOperand(-x);
                        }
                        break;

                    default:
                        throw new System.InvalidOperationException();
                }
            }
            foreach (string parameter in parameters)
            {
                b.Append(' ');
                for (int i = 0; i < 5 - parameter.Length; i++)
                {
                    b.Append(' ');
                }
                b.Append(parameter);
            }
            if (upvalue)
            {
                b.Append(" ; upvalue declaration");
            }
            else if (function != null && constant >= 0)
            {
                b.Append(" ; ");
                b.Append(ConstantOperand(constant));
                if (constant < function.constants.Length)
                {
                    b.Append(" = ");
                    b.Append(function.constants[constant].ToShortString());
                }
                else
                {
                    b.Append(" out of range");
                }
            }
            return b.ToString();
        }

        private static string UpvalueOperand(int field)
        {
            return "u" + field;
        }

        public static string DefaultToString(LFunction function, int codepoint, Version version, CodeExtract ex, bool upvalue)
        {
            return ToStringHelper(
                function,
                string.Format("op{0:D2}",
                ex.op.Extract(codepoint)),
                version.DefaultOp.operands,
                codepoint,
                ex,
                null,
                upvalue);
        }

        public string CodePointTostring(LFunction function, int codepoint, CodeExtract ex, string label, bool upvalue)
        {
            return ToStringHelper(function, name, operands, codepoint, ex, label, upvalue);
        }

        public bool HasExtraByte(int codepoint, CodeExtract ex)
        {
            if (Type == OpT.SETLIST)
            {
                return ex.C.Extract(codepoint) == 0;
            }
            else
            {
                return false;
            }
        }

        public bool HasJump()
        {
            for (int i = 0; i < Operands.Length; ++i)
            {
                OperandFormat.Format format = Operands[i].format;
                if (format == OperandFormat.Format.Jump || format == OperandFormat.Format.JumpNegative)
                {
                    return true;
                }
            }
            return false;
        }

        public int JumpField(int codepoint, CodeExtract ex)
        {
            switch (Type)
            {
                case OpT.FORPREP54:
                case OpT.TFORPREP54:
                    return ex.Bx.Extract(codepoint);

                case OpT.FORLOOP54:
                case OpT.TFORLOOP54:
                    return -ex.Bx.Extract(codepoint);

                case OpT.JMP:
                case OpT.FORLOOP:
                case OpT.FORPREP:
                case OpT.JMP52:
                case OpT.TFORLOOP52:
                case OpT.TFORPREP:
                    return ex.sBx.Extract(codepoint);

                case OpT.JMP54:
                    return ex.sJ.Extract(codepoint);

                default:
                    throw new System.InvalidOperationException();
            }
        }

        public int Target(int codepoint, CodeExtract ex)
        {
            switch (Type)
            {
                case OpT.MOVE:
                case OpT.LOADI:
                case OpT.LOADF:
                case OpT.LOADK:
                case OpT.LOADKX:
                case OpT.LOADBOOL:
                case OpT.LOADFALSE:
                case OpT.LFALSESKIP:
                case OpT.LOADTRUE:
                case OpT.GETUPVALUE:
                case OpT.GETTABUP:
                case OpT.GETTABUP54:
                case OpT.GETGLOBAL:
                case OpT.GETTABLE:
                case OpT.GETTABLE54:
                case OpT.GETI:
                case OpT.GETFIELD:
                case OpT.NEWTABLE50:
                case OpT.NEWTABLE:
                case OpT.NEWTABLE54:
                case OpT.ADD:
                case OpT.SUB:
                case OpT.MUL:
                case OpT.DIV:
                case OpT.IDIV:
                case OpT.MOD:
                case OpT.POW:
                case OpT.BAND:
                case OpT.BOR:
                case OpT.BXOR:
                case OpT.SHL:
                case OpT.SHR:
                case OpT.ADD54:
                case OpT.SUB54:
                case OpT.MUL54:
                case OpT.DIV54:
                case OpT.IDIV54:
                case OpT.MOD54:
                case OpT.POW54:
                case OpT.BAND54:
                case OpT.BOR54:
                case OpT.BXOR54:
                case OpT.SHL54:
                case OpT.SHR54:
                case OpT.ADDK:
                case OpT.SUBK:
                case OpT.MULK:
                case OpT.DIVK:
                case OpT.IDIVK:
                case OpT.MODK:
                case OpT.POWK:
                case OpT.BANDK:
                case OpT.BORK:
                case OpT.BXORK:
                case OpT.ADDI:
                case OpT.SHLI:
                case OpT.SHRI:
                case OpT.UNM:
                case OpT.NOT:
                case OpT.LEN:
                case OpT.BNOT:
                case OpT.CONCAT:
                case OpT.CONCAT54:
                case OpT.CLOSURE:
                case OpT.TEST50:
                case OpT.TESTSET:
                case OpT.TESTSET54:
                    return ex.A.Extract(codepoint);

                case OpT.MMBIN:
                case OpT.MMBINI:
                case OpT.MMBINK:
                    return -1; // depends on previous instruction
                case OpT.LOADNIL:
                    if (ex.A.Extract(codepoint) == ex.B.Extract(codepoint))
                    {
                        return ex.A.Extract(codepoint);
                    }
                    else
                    {
                        return -1;
                    }
                case OpT.LOADNIL52:
                    if (ex.B.Extract(codepoint) == 0)
                    {
                        return ex.A.Extract(codepoint);
                    }
                    else
                    {
                        return -1;
                    }
                case OpT.SETGLOBAL:
                case OpT.SETUPVALUE:
                case OpT.SETTABUP:
                case OpT.SETTABUP54:
                case OpT.SETTABLE:
                case OpT.SETTABLE54:
                case OpT.SETI:
                case OpT.SETFIELD:
                case OpT.JMP:
                case OpT.JMP52:
                case OpT.JMP54:
                case OpT.TAILCALL:
                case OpT.TAILCALL54:
                case OpT.RETURN:
                case OpT.RETURN54:
                case OpT.RETURN0:
                case OpT.RETURN1:
                case OpT.FORLOOP:
                case OpT.FORLOOP54:
                case OpT.FORPREP:
                case OpT.FORPREP54:
                case OpT.TFORPREP:
                case OpT.TFORPREP54:
                case OpT.TFORCALL:
                case OpT.TFORCALL54:
                case OpT.TFORLOOP:
                case OpT.TFORLOOP52:
                case OpT.TFORLOOP54:
                case OpT.TBC:
                case OpT.CLOSE:
                case OpT.EXTRAARG:
                case OpT.SELF:
                case OpT.SELF54:
                case OpT.EQ:
                case OpT.LT:
                case OpT.LE:
                case OpT.EQ54:
                case OpT.LT54:
                case OpT.LE54:
                case OpT.EQK:
                case OpT.EQI:
                case OpT.LTI:
                case OpT.LEI:
                case OpT.GTI:
                case OpT.GEI:
                case OpT.TEST:
                case OpT.TEST54:
                case OpT.SETLIST50:
                case OpT.SETLISTO:
                case OpT.SETLIST:
                case OpT.SETLIST52:
                case OpT.SETLIST54:
                case OpT.VARARGPREP:
                    return -1;

                case OpT.CALL:
                {
                    int a = ex.A.Extract(codepoint);
                    int c = ex.C.Extract(codepoint);
                    if (c == 2)
                    {
                        return a;
                    }
                    else
                    {
                        return -1;
                    }
                }
                case OpT.VARARG:
                {
                    int a = ex.A.Extract(codepoint);
                    int b = ex.B.Extract(codepoint);
                    if (b == 2)
                    {
                        return a;
                    }
                    else
                    {
                        return -1;
                    }
                }
                case OpT.VARARG54:
                {
                    int a = ex.A.Extract(codepoint);
                    int c = ex.C.Extract(codepoint);
                    if (c == 2)
                    {
                        return a;
                    }
                    else
                    {
                        return -1;
                    }
                }
                case OpT.EXTRABYTE:
                    return -1;

                case OpT.DEFAULT:
                case OpT.DEFAULT54:
                    throw new System.InvalidOperationException();
            }
            throw new System.InvalidOperationException(this.Name);
        }
    }
}