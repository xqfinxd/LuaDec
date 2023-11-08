using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaDec.Decompile
{
    public static class OpV
    {
        public static readonly int LUA50 = 1;
        public static readonly int LUA51 = 2;
        public static readonly int LUA52 = 4;
        public static readonly int LUA53 = 8;
        public static readonly int LUA54 = 16;
    }
    
    public class Op
    {
        public enum OpT
        {
            MOVE,
            LOADK,
            LOADBOOL,
            LOADNIL,
            GETUPVAL,
            GETGLOBAL,
            GETTABLE,
            SETGLOBAL,
            SETUPVAL,
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
        // Lua 5.1 Opcodes
        public static readonly Op MOVE = new Op(OpT.MOVE,"move", OpV.LUA50 | OpV.LUA51 | OpV.LUA52 | OpV.LUA53 | OpV.LUA54, OperandFormat.AR, OperandFormat.BR);
        public static readonly Op LOADK = new Op(OpT.LOADK,"loadk", OpV.LUA50 | OpV.LUA51 | OpV.LUA52 | OpV.LUA53 | OpV.LUA54, OperandFormat.AR, OperandFormat.BxK);
        public static readonly Op LOADBOOL = new Op(OpT.LOADBOOL,"loadbool", OpV.LUA50 | OpV.LUA51 | OpV.LUA52 | OpV.LUA53, OperandFormat.AR, OperandFormat.B, OperandFormat.C);
        public static readonly Op LOADNIL = new Op(OpT.LOADNIL,"loadnil", OpV.LUA50 | OpV.LUA51, OperandFormat.AR, OperandFormat.BR);
        public static readonly Op GETUPVAL = new Op(OpT.GETUPVAL,"getupval", OpV.LUA50 | OpV.LUA51 | OpV.LUA52 | OpV.LUA53 | OpV.LUA54, OperandFormat.AR, OperandFormat.BU);
        public static readonly Op GETGLOBAL = new Op(OpT.GETGLOBAL,"getglobal", OpV.LUA50 | OpV.LUA51, OperandFormat.AR, OperandFormat.BxK);
        public static readonly Op GETTABLE = new Op(OpT.GETTABLE,"gettable", OpV.LUA50 | OpV.LUA51 | OpV.LUA52 | OpV.LUA53, OperandFormat.AR, OperandFormat.BR, OperandFormat.CRK);
        public static readonly Op SETGLOBAL = new Op(OpT.SETGLOBAL,"setglobal", OpV.LUA50 | OpV.LUA51, OperandFormat.AR, OperandFormat.BxK);
        public static readonly Op SETUPVAL = new Op(OpT.SETUPVAL,"setupval", OpV.LUA50 | OpV.LUA51 | OpV.LUA52 | OpV.LUA53 | OpV.LUA54, OperandFormat.AR, OperandFormat.BU);
        public static readonly Op SETTABLE = new Op(OpT.SETTABLE,"settable", OpV.LUA50 | OpV.LUA51 | OpV.LUA52 | OpV.LUA53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);
        public static readonly Op NEWTABLE = new Op(OpT.NEWTABLE,"newtable", OpV.LUA51 | OpV.LUA52 | OpV.LUA53, OperandFormat.AR, OperandFormat.B, OperandFormat.C);
        public static readonly Op SELF = new Op(OpT.SELF,"self", OpV.LUA50 | OpV.LUA51 | OpV.LUA52 | OpV.LUA53, OperandFormat.AR, OperandFormat.BR, OperandFormat.CRK);
        public static readonly Op ADD = new Op(OpT.ADD,"add", OpV.LUA50 | OpV.LUA51 | OpV.LUA52 | OpV.LUA53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);
        public static readonly Op SUB = new Op(OpT.SUB,"sub", OpV.LUA50 | OpV.LUA51 | OpV.LUA52 | OpV.LUA53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);
        public static readonly Op MUL = new Op(OpT.MUL,"mul", OpV.LUA50 | OpV.LUA51 | OpV.LUA52 | OpV.LUA53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);
        public static readonly Op DIV = new Op(OpT.DIV,"div", OpV.LUA50 | OpV.LUA51 | OpV.LUA52 | OpV.LUA53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);
        public static readonly Op MOD = new Op(OpT.MOD,"mod", OpV.LUA51 | OpV.LUA52 | OpV.LUA53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);
        public static readonly Op POW = new Op(OpT.POW,"pow", OpV.LUA50 | OpV.LUA51 | OpV.LUA52 | OpV.LUA53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);
        public static readonly Op UNM = new Op(OpT.UNM,"unm", OpV.LUA50 | OpV.LUA51 | OpV.LUA52 | OpV.LUA53, OperandFormat.AR, OperandFormat.BR);
        public static readonly Op NOT = new Op(OpT.NOT,"not", OpV.LUA50 | OpV.LUA51 | OpV.LUA52 | OpV.LUA53, OperandFormat.AR, OperandFormat.BR);
        public static readonly Op LEN = new Op(OpT.LEN,"len", OpV.LUA51 | OpV.LUA52 | OpV.LUA53, OperandFormat.AR, OperandFormat.BR);
        public static readonly Op CONCAT = new Op(OpT.CONCAT,"concat", OpV.LUA50 | OpV.LUA51 | OpV.LUA52 | OpV.LUA53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);
        public static readonly Op JMP = new Op(OpT.JMP,"jmp", OpV.LUA50 | OpV.LUA51, OperandFormat.sBxJ);
        public static readonly Op EQ = new Op(OpT.EQ,"eq", OpV.LUA50 | OpV.LUA51 | OpV.LUA52 | OpV.LUA53, OperandFormat.A, OperandFormat.BRK, OperandFormat.CRK);
        public static readonly Op LT = new Op(OpT.LT,"lt", OpV.LUA50 | OpV.LUA51 | OpV.LUA52 | OpV.LUA53, OperandFormat.A, OperandFormat.BRK, OperandFormat.CRK);
        public static readonly Op LE = new Op(OpT.LE,"le", OpV.LUA50 | OpV.LUA51 | OpV.LUA52 | OpV.LUA53, OperandFormat.A, OperandFormat.BRK, OperandFormat.CRK);
        public static readonly Op TEST = new Op(OpT.TEST,"test", OpV.LUA51 | OpV.LUA52 | OpV.LUA53, OperandFormat.AR, OperandFormat.C);
        public static readonly Op TESTSET = new Op(OpT.TESTSET,"testset", OpV.LUA51 | OpV.LUA52 | OpV.LUA53, OperandFormat.AR, OperandFormat.BR, OperandFormat.C);
        public static readonly Op CALL = new Op(OpT.CALL,"call", OpV.LUA50 | OpV.LUA51 | OpV.LUA52 | OpV.LUA53 | OpV.LUA54, OperandFormat.AR, OperandFormat.B, OperandFormat.C);
        public static readonly Op TAILCALL = new Op(OpT.TAILCALL,"tailcall", OpV.LUA50 | OpV.LUA51 | OpV.LUA52 | OpV.LUA53, OperandFormat.AR, OperandFormat.B);
        public static readonly Op RETURN = new Op(OpT.RETURN,"return", OpV.LUA50 | OpV.LUA51 | OpV.LUA52 | OpV.LUA53, OperandFormat.AR, OperandFormat.B);
        public static readonly Op FORLOOP = new Op(OpT.FORLOOP,"forloop", OpV.LUA50 | OpV.LUA51 | OpV.LUA52 | OpV.LUA53, OperandFormat.AR, OperandFormat.sBxJ);
        public static readonly Op FORPREP = new Op(OpT.FORPREP,"forprep", OpV.LUA51 | OpV.LUA52 | OpV.LUA53, OperandFormat.AR, OperandFormat.sBxJ);
        public static readonly Op TFORLOOP = new Op(OpT.TFORLOOP,"tforloop", OpV.LUA50 | OpV.LUA51, OperandFormat.AR, OperandFormat.C);
        public static readonly Op SETLIST = new Op(OpT.SETLIST,"setlist", OpV.LUA51, OperandFormat.AR, OperandFormat.B, OperandFormat.C);
        public static readonly Op CLOSE = new Op(OpT.CLOSE,"close", OpV.LUA50 | OpV.LUA51 | OpV.LUA54, OperandFormat.AR);
        public static readonly Op CLOSURE = new Op(OpT.CLOSURE,"closure", OpV.LUA50 | OpV.LUA51 | OpV.LUA52 | OpV.LUA53 | OpV.LUA54, OperandFormat.AR, OperandFormat.BxF);
        public static readonly Op VARARG = new Op(OpT.VARARG,"vararg", OpV.LUA51 | OpV.LUA52 | OpV.LUA53, OperandFormat.AR, OperandFormat.B);
        // Lua 5.2 Opcodes
        public static readonly Op JMP52 = new Op(OpT.JMP52,"jmp", OpV.LUA52 | OpV.LUA53, OperandFormat.A, OperandFormat.sBxJ);
        public static readonly Op LOADNIL52 = new Op(OpT.LOADNIL52,"loadnil", OpV.LUA52 | OpV.LUA53 | OpV.LUA54, OperandFormat.AR, OperandFormat.B);
        public static readonly Op LOADKX = new Op(OpT.LOADKX,"loadkx", OpV.LUA52 | OpV.LUA53 | OpV.LUA54, OperandFormat.AR);
        public static readonly Op GETTABUP = new Op(OpT.GETTABUP,"gettabup", OpV.LUA52 | OpV.LUA53, OperandFormat.AR, OperandFormat.BU, OperandFormat.CRK);
        public static readonly Op SETTABUP = new Op(OpT.SETTABUP,"settabup", OpV.LUA52 | OpV.LUA53, OperandFormat.AU, OperandFormat.BRK, OperandFormat.CRK);
        public static readonly Op SETLIST52 = new Op(OpT.SETLIST52,"setlist", OpV.LUA52 | OpV.LUA53, OperandFormat.AR, OperandFormat.B, OperandFormat.C);
        public static readonly Op TFORCALL = new Op(OpT.TFORCALL,"tforcall", OpV.LUA52 | OpV.LUA53, OperandFormat.AR, OperandFormat.C);
        public static readonly Op TFORLOOP52 = new Op(OpT.TFORLOOP52,"tforloop", OpV.LUA52 | OpV.LUA53, OperandFormat.AR, OperandFormat.sBxJ);
        public static readonly Op EXTRAARG = new Op(OpT.EXTRAARG,"extraarg", OpV.LUA52 | OpV.LUA53 | OpV.LUA54, OperandFormat.Ax);
        // Lua 5.0 Opcodes
        public static readonly Op NEWTABLE50 = new Op(OpT.NEWTABLE50,"newtable", OpV.LUA50, OperandFormat.AR, OperandFormat.B, OperandFormat.C);
        public static readonly Op SETLIST50 = new Op(OpT.SETLIST50,"setlist", OpV.LUA50, OperandFormat.AR, OperandFormat.Bx);
        public static readonly Op SETLISTO = new Op(OpT.SETLISTO,"setlisto", OpV.LUA50, OperandFormat.AR, OperandFormat.Bx);
        public static readonly Op TFORPREP = new Op(OpT.TFORPREP,"tforprep", OpV.LUA50, OperandFormat.AR, OperandFormat.sBxJ);
        public static readonly Op TEST50 = new Op(OpT.TEST50,"test", OpV.LUA50, OperandFormat.AR, OperandFormat.BR, OperandFormat.C);
        // Lua 5.3 Opcodes
        public static readonly Op IDIV = new Op(OpT.IDIV,"idiv", OpV.LUA53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);
        public static readonly Op BAND = new Op(OpT.BAND,"band", OpV.LUA53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);
        public static readonly Op BOR = new Op(OpT.BOR,"bor", OpV.LUA53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);
        public static readonly Op BXOR = new Op(OpT.BXOR,"bxor", OpV.LUA53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);
        public static readonly Op SHL = new Op(OpT.SHL,"shl", OpV.LUA53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);
        public static readonly Op SHR = new Op(OpT.SHR,"shr", OpV.LUA53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);
        public static readonly Op BNOT = new Op(OpT.BNOT,"bnot", OpV.LUA53 | OpV.LUA54, OperandFormat.AR, OperandFormat.BR);
        // Lua 5.4 Opcodes
        public static readonly Op LOADI = new Op(OpT.LOADI,"loadi", OpV.LUA54, OperandFormat.AR, OperandFormat.sBxI);
        public static readonly Op LOADF = new Op(OpT.LOADF,"loadf", OpV.LUA54, OperandFormat.AR, OperandFormat.sBxF);
        public static readonly Op LOADFALSE = new Op(OpT.LOADFALSE,"loadfalse", OpV.LUA54, OperandFormat.AR);
        public static readonly Op LFALSESKIP = new Op(OpT.LFALSESKIP,"lfalseskip", OpV.LUA54, OperandFormat.AR);
        public static readonly Op LOADTRUE = new Op(OpT.LOADTRUE,"loadtrue", OpV.LUA54, OperandFormat.AR);
        public static readonly Op GETTABUP54 = new Op(OpT.GETTABUP54,"gettabup", OpV.LUA54, OperandFormat.AR, OperandFormat.BU, OperandFormat.CKS);
        public static readonly Op GETTABLE54 = new Op(OpT.GETTABLE54,"gettable", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CR);
        public static readonly Op GETI = new Op(OpT.GETI,"geti", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CI);
        public static readonly Op GETFIELD = new Op(OpT.GETFIELD,"getfield", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CKS);
        public static readonly Op SETTABUP54 = new Op(OpT.SETTABUP54,"settabup", OpV.LUA54, OperandFormat.AU, OperandFormat.BK, OperandFormat.CRK54);
        public static readonly Op SETTABLE54 = new Op(OpT.SETTABLE54,"settable", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CRK54);
        public static readonly Op SETI = new Op(OpT.SETI,"seti", OpV.LUA54, OperandFormat.AR, OperandFormat.BI, OperandFormat.CRK54);
        public static readonly Op SETFIELD = new Op(OpT.SETFIELD,"setfield", OpV.LUA54, OperandFormat.AR, OperandFormat.BKS, OperandFormat.CRK54);
        public static readonly Op NEWTABLE54 = new Op(OpT.NEWTABLE54,"newtable", OpV.LUA54, OperandFormat.AR, OperandFormat.B, OperandFormat.C, OperandFormat.k);
        public static readonly Op SELF54 = new Op(OpT.SELF54,"self", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CRK54);
        public static readonly Op ADDI = new Op(OpT.ADDI,"addi", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CsI);
        public static readonly Op ADDK = new Op(OpT.ADDK,"addk", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CK);
        public static readonly Op SUBK = new Op(OpT.SUBK,"subk", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CK);
        public static readonly Op MULK = new Op(OpT.MULK,"mulk", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CK);
        public static readonly Op MODK = new Op(OpT.MODK,"modk", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CK);
        public static readonly Op POWK = new Op(OpT.POWK,"powk", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CK);
        public static readonly Op DIVK = new Op(OpT.DIVK,"divk", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CK);
        public static readonly Op IDIVK = new Op(OpT.IDIVK,"idivk", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CK);
        public static readonly Op BANDK = new Op(OpT.BANDK,"bandk", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CKI);
        public static readonly Op BORK = new Op(OpT.BORK,"bork", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CKI);
        public static readonly Op BXORK = new Op(OpT.BXORK,"bxork", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CKI);
        public static readonly Op SHRI = new Op(OpT.SHRI,"shri", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CsI);
        public static readonly Op SHLI = new Op(OpT.SHLI,"shli", OpV.LUA54, OperandFormat.AR, OperandFormat.CsI, OperandFormat.BR);
        public static readonly Op ADD54 = new Op(OpT.ADD54,"add", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CR);
        public static readonly Op SUB54 = new Op(OpT.SUB54,"sub", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CR);
        public static readonly Op MUL54 = new Op(OpT.MUL54,"mul", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CR);
        public static readonly Op MOD54 = new Op(OpT.MOD54,"mod", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CR);
        public static readonly Op POW54 = new Op(OpT.POW54,"pow", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CR);
        public static readonly Op DIV54 = new Op(OpT.DIV54,"div", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CR);
        public static readonly Op IDIV54 = new Op(OpT.IDIV54,"idiv", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CR);
        public static readonly Op BAND54 = new Op(OpT.BAND54,"band", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CR);
        public static readonly Op BOR54 = new Op(OpT.BOR54,"bor", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CR);
        public static readonly Op BXOR54 = new Op(OpT.BXOR54,"bxor", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CR);
        public static readonly Op SHL54 = new Op(OpT.SHL54,"shl", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CR);
        public static readonly Op SHR54 = new Op(OpT.SHR54,"shr", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.CR);
        public static readonly Op MMBIN = new Op(OpT.MMBIN,"mmbin", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.C);
        public static readonly Op MMBINI = new Op(OpT.MMBINI,"mmbini", OpV.LUA54, OperandFormat.AR, OperandFormat.BsI, OperandFormat.C, OperandFormat.k);
        public static readonly Op MMBINK = new Op(OpT.MMBINK,"mmbink", OpV.LUA54, OperandFormat.AR, OperandFormat.BK, OperandFormat.C, OperandFormat.k);
        public static readonly Op CONCAT54 = new Op(OpT.CONCAT54,"concat", OpV.LUA54, OperandFormat.AR, OperandFormat.B);
        public static readonly Op TBC = new Op(OpT.TBC,"tbc", OpV.LUA54, OperandFormat.AR);
        public static readonly Op JMP54 = new Op(OpT.JMP54,"jmp", OpV.LUA54, OperandFormat.sJ);
        public static readonly Op EQ54 = new Op(OpT.EQ54,"eq", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.k);
        public static readonly Op LT54 = new Op(OpT.LT54,"lt", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.k);
        public static readonly Op LE54 = new Op(OpT.LE54,"le", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.k);
        public static readonly Op EQK = new Op(OpT.EQK,"eqk", OpV.LUA54, OperandFormat.AR, OperandFormat.BK, OperandFormat.k);
        public static readonly Op EQI = new Op(OpT.EQI,"eqi", OpV.LUA54, OperandFormat.AR, OperandFormat.BsI, OperandFormat.k, OperandFormat.C);
        public static readonly Op LTI = new Op(OpT.LTI,"lti", OpV.LUA54, OperandFormat.AR, OperandFormat.BsI, OperandFormat.k, OperandFormat.C);
        public static readonly Op LEI = new Op(OpT.LEI,"lei", OpV.LUA54, OperandFormat.AR, OperandFormat.BsI, OperandFormat.k, OperandFormat.C);
        public static readonly Op GTI = new Op(OpT.GTI,"gti", OpV.LUA54, OperandFormat.AR, OperandFormat.BsI, OperandFormat.k, OperandFormat.C);
        public static readonly Op GEI = new Op(OpT.GEI,"gei", OpV.LUA54, OperandFormat.AR, OperandFormat.BsI, OperandFormat.k, OperandFormat.C);
        public static readonly Op TEST54 = new Op(OpT.TEST54,"test", OpV.LUA54, OperandFormat.AR, OperandFormat.k);
        public static readonly Op TESTSET54 = new Op(OpT.TESTSET54,"testset", OpV.LUA54, OperandFormat.AR, OperandFormat.BR, OperandFormat.k);
        public static readonly Op TAILCALL54 = new Op(OpT.TAILCALL54,"tailcall", OpV.LUA54, OperandFormat.AR, OperandFormat.B, OperandFormat.C, OperandFormat.k);
        public static readonly Op RETURN54 = new Op(OpT.RETURN54,"return", OpV.LUA54, OperandFormat.AR, OperandFormat.B, OperandFormat.C, OperandFormat.k);
        public static readonly Op RETURN0 = new Op(OpT.RETURN0,"return0", OpV.LUA54, OperandFormat.AR, OperandFormat.B, OperandFormat.C, OperandFormat.k);
        public static readonly Op RETURN1 = new Op(OpT.RETURN1,"return1", OpV.LUA54, OperandFormat.AR, OperandFormat.B, OperandFormat.C, OperandFormat.k);
        public static readonly Op FORLOOP54 = new Op(OpT.FORLOOP54,"forloop", OpV.LUA54, OperandFormat.AR, OperandFormat.BxJn);
        public static readonly Op FORPREP54 = new Op(OpT.FORPREP54,"forprep", OpV.LUA54, OperandFormat.AR, OperandFormat.BxJ);
        public static readonly Op TFORPREP54 = new Op(OpT.TFORPREP54,"tforprep", OpV.LUA54, OperandFormat.AR, OperandFormat.BxJ);
        public static readonly Op TFORCALL54 = new Op(OpT.TFORCALL54,"tforcall", OpV.LUA54, OperandFormat.AR, OperandFormat.C);
        public static readonly Op TFORLOOP54 = new Op(OpT.TFORLOOP54,"tforloop", OpV.LUA54, OperandFormat.AR, OperandFormat.BxJn);
        public static readonly Op SETLIST54 = new Op(OpT.SETLIST54,"setlist", OpV.LUA54, OperandFormat.AR, OperandFormat.B, OperandFormat.C, OperandFormat.k);
        public static readonly Op VARARG54 = new Op(OpT.VARARG54,"vararg", OpV.LUA54, OperandFormat.AR, OperandFormat.C);
        public static readonly Op VARARGPREP = new Op(OpT.VARARGPREP,"varargprep", OpV.LUA54, OperandFormat.A);
        // Special
        public static readonly Op EXTRABYTE = new Op(OpT.EXTRABYTE,"extrabyte", OpV.LUA50 | OpV.LUA51 | OpV.LUA52 | OpV.LUA53 | OpV.LUA54, OperandFormat.x);
        public static readonly Op DEFAULT = new Op(OpT.DEFAULT,"default", 0, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK);
        public static readonly Op DEFAULT54 = new Op(OpT.DEFAULT54,"default", 0, OperandFormat.AR, OperandFormat.BR, OperandFormat.C, OperandFormat.k);
        #endregion

        private readonly OpT type;
        private readonly string name;
        private readonly int versions;
        private readonly OperandFormat[] operands;

        public string Name => name;
        public int Versions => versions;
        public OperandFormat[] Operands => operands;

        public OpT Type => type;

        private Op(OpT type, string name, int versions, params OperandFormat[] fmts)
        {
            this.type = type;
            this.name = name;
            this.versions = versions;
            operands = fmts;
        }

        /**
         * SETLIST sometimes uses an extra byte without tagging it.
         * This means that the value in the extra byte can be detected as any other opcode unless it is recognized.
         */
        public bool hasExtraByte(int codepoint, CodeExtract ex)
        {
            if (Type == OpT.SETLIST)
            {
                return ex.C.extract(codepoint) == 0;
            }
            else
            {
                return false;
            }
        }

        public int jumpField(int codepoint, CodeExtract ex)
        {
            switch (Type)
            {
                case OpT.FORPREP54:
                case OpT.TFORPREP54:
                    return ex.Bx.extract(codepoint);
                case OpT.FORLOOP54:
                case OpT.TFORLOOP54:
                    return -ex.Bx.extract(codepoint);
                case OpT.JMP:
                case OpT.FORLOOP:
                case OpT.FORPREP:
                case OpT.JMP52:
                case OpT.TFORLOOP52:
                case OpT.TFORPREP:
                    return ex.sBx.extract(codepoint);
                case OpT.JMP54:
                    return ex.sJ.extract(codepoint);
                default:
                    throw new System.InvalidOperationException();
            }
        }

        /**
         * Returns the target register of the instruction at the given
         * line or -1 if the instruction does not have a unique target.
         */
        public int target(int codepoint, CodeExtract ex)
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
                case OpT.GETUPVAL:
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
                    return ex.A.extract(codepoint);
                case OpT.MMBIN:
                case OpT.MMBINI:
                case OpT.MMBINK:
                    return -1; // depends on previous instruction
                case OpT.LOADNIL:
                    if (ex.A.extract(codepoint) == ex.B.extract(codepoint))
                    {
                        return ex.A.extract(codepoint);
                    }
                    else
                    {
                        return -1;
                    }
                case OpT.LOADNIL52:
                    if (ex.B.extract(codepoint) == 0)
                    {
                        return ex.A.extract(codepoint);
                    }
                    else
                    {
                        return -1;
                    }
                case OpT.SETGLOBAL:
                case OpT.SETUPVAL:
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
                        int a = ex.A.extract(codepoint);
                        int c = ex.C.extract(codepoint);
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
                        int a = ex.A.extract(codepoint);
                        int b = ex.B.extract(codepoint);
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
                        int a = ex.A.extract(codepoint);
                        int c = ex.C.extract(codepoint);
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

        private static string fixedOperand(int field)
        {
            return field.ToString();
        }

        private static string registerOperand(int field)
        {
            return "r" + field;
        }

        private static string upvalueOperand(int field)
        {
            return "u" + field;
        }

        private static string constantOperand(int field)
        {
            return "k" + field;
        }

        private static string functionOperand(int field)
        {
            return "f" + field;
        }

        public bool hasJump()
        {
            for (int i = 0; i < Operands.Length; ++i)
            {
                OperandFormat.Format format = Operands[i].format;
                if (format == OperandFormat.Format.JUMP || format == OperandFormat.Format.JUMP_NEGATIVE)
                {
                    return true;
                }
            }
            return false;
        }
        public string codePointTostring(int codepoint, CodeExtract ex, string label)
        {
            return tostringHelper(name, operands, codepoint, ex, label);
        }

        public static string defaultTostring(int codepoint, Version version, CodeExtract ex)
        {
            return tostringHelper(String.Format("op{0:D2}", ex.op.extract(codepoint)), version.DefaultOp.operands, codepoint, ex, null);
        }
        private static string tostringHelper(string name, OperandFormat[] operands, int codepoint, CodeExtract ex, string label)
        {
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
                int x = field.extract(codepoint);
                switch (operands[i].format)
                {
                    case OperandFormat.Format.IMMEDIATE_int:
                    case OperandFormat.Format.IMMEDIATE_FLOAT:
                    case OperandFormat.Format.RAW: parameters[i] = fixedOperand(x); break;
                    case OperandFormat.Format.IMMEDIATE_SIGNED_int: parameters[i] = fixedOperand(x - field.max() / 2); break;
                    case OperandFormat.Format.REGISTER: parameters[i] = registerOperand(x); break;
                    case OperandFormat.Format.UPVALUE: parameters[i] = upvalueOperand(x); break;
                    case OperandFormat.Format.REGISTER_K:
                        if (ex.is_k(x))
                        {
                            parameters[i] = constantOperand(ex.get_k(x));
                        }
                        else
                        {
                            parameters[i] = registerOperand(x);
                        }
                        break;
                    case OperandFormat.Format.REGISTER_K54:
                        if (ex.k.extract(codepoint) != 0)
                        {
                            parameters[i] = constantOperand(x);
                        }
                        else
                        {
                            parameters[i] = registerOperand(x);
                        }
                        break;
                    case OperandFormat.Format.CONSTANT:
                    case OperandFormat.Format.CONSTANT_int:
                    case OperandFormat.Format.CONSTANT_STRING: parameters[i] = constantOperand(x); break;
                    case OperandFormat.Format.FUNCTION: parameters[i] = functionOperand(x); break;
                    case OperandFormat.Format.JUMP:
                        if (label != null)
                        {
                            parameters[i] = label;
                        }
                        else
                        {
                            parameters[i] = fixedOperand(x + operands[i].offset);
                        }
                        break;
                    case OperandFormat.Format.JUMP_NEGATIVE:
                        if (label != null)
                        {
                            parameters[i] = label;
                        }
                        else
                        {
                            parameters[i] = fixedOperand(-x);
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
            return b.ToString();
        }

    }
}
