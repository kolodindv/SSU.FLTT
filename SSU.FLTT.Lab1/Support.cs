using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSU.FLTT.Lab1
{
    public enum LexemeType { Do, While, Loop, Not, And, Or, Input, Output, Relation, ArithmeticOperation, Assignment, Undefined }

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

        //public static void Error(string message)
        //{
        //    Console.WriteLine(new Exception($"{message}")); 
        //}

    }
}
