﻿using System;

namespace SSU.FLTT.Labs
{
    public enum LexemeType { Do, Loop, While, Not, And, Or, Input, Output, Relation, ArithmeticOperation, Assignment, Undefined }

    public enum LexemeClass { Keyword, Identifier, Constant, SpecialSymbols, Undefined }

    public enum State { Start, Identifier, Constant, Error, Final, Comparison, ReverseComparison, ArithmeticOperation, Assignment }

    public enum EntryType { Cmd, Var, Const, CmdPtr }

    public enum Cmd { JMP, JZ, SET, ADD, SUB, MUL, DIV, AND, OR, CMPE, CMPNE, CMPL, CMPLE, CMPG, CMPGE, OUTPUT, INPUT}

    public static class Support
    {
        public static void Error(string message, int position)
        {
            throw new Exception($"{message} в позиции: {position}");
        }
    }    
}
