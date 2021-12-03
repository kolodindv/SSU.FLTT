using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSU.FLTT.Lab1
{
    public class LexicalAnalyser

    {
        public List<Lexeme> Lexemes { get; private set; }

        public LexicalAnalyser()
        {
            Lexemes = new List<Lexeme>();
        }

        public bool Run(string text)

        {
            Lexemes = new List<Lexeme>();

            State state = State.Start;
            State prevState;

            bool isAbleToAdd;

            text += " ";

            StringBuilder lexBufNext = new();

            StringBuilder lexBufCur = new();

            int textIndex = 0;

            while (state != State.Error && state != State.Final)
            {
                prevState = state;

                isAbleToAdd = true;

                if (textIndex == text.Length && state != State.Error)
                {
                    state = State.Final;
                    break;
                }

                if (textIndex == text.Length)
                {
                    break;
                }

                char symbol = text[textIndex];

                switch (state)
                {
                    case State.Start:
                        {
                            if (char.IsWhiteSpace(symbol)) state = State.Start;

                            else if (char.IsDigit(symbol)) state = State.Constant;

                            else if (char.IsLetter(symbol)) state = State.Identifier;

                            else if (symbol == '>') state = State.Comparison;
                            else if (symbol == '<') state = State.ReverseComparison;

                            else if (symbol == '+' || symbol == '-' || symbol == '/' || symbol == '*') state = State.ArithmeticOperation;

                            else if (symbol == '=') state = State.Assignment;

                            else state = State.Error;

                            isAbleToAdd = false;

                            if (!char.IsWhiteSpace(symbol)) lexBufCur.Append(symbol);

                            break;
                        }                        

                    case State.Comparison:
                        {
                            if (char.IsWhiteSpace(symbol)) state = State.Start;

                            else if (symbol == '=')
                            {
                                state = State.Start;
                                lexBufCur.Append(symbol);
                            }

                            else if (char.IsLetter(symbol))
                            {
                                state = State.Identifier;
                                lexBufNext.Append(symbol);
                            }

                            else if (char.IsDigit(symbol))
                            {
                                state = State.Constant;
                                lexBufNext.Append(symbol);
                            }

                            else
                            {
                                state = State.Error;
                                isAbleToAdd = false;
                            }

                            break;
                        }                       

                    case State.ReverseComparison:
                        {
                            if (char.IsWhiteSpace(symbol)) state = State.Start;

                            else if (symbol == '>')
                            {
                                state = State.Start;
                                lexBufCur.Append(symbol);
                            }

                            else if (symbol == '=')
                            {
                                state = State.Start;
                                lexBufCur.Append(symbol);
                            }

                            else if (char.IsLetter(symbol))
                            {
                                state = State.Identifier;
                                lexBufNext.Append(symbol);
                            }

                            else if (char.IsDigit(symbol))
                            {
                                state = State.Constant;
                                lexBufNext.Append(symbol);
                            }

                            else
                            {
                                state = State.Error;
                                isAbleToAdd = false;
                            }

                            break;
                        }                        

                    case State.Assignment:
                        {
                            if (symbol == '=')
                            {
                                state = State.Comparison;
                                lexBufCur.Append(symbol);
                            }

                            else //if (char.IsWhiteSpace(symbol))
                            {
                                state = State.Start;
                                lexBufNext.Append(symbol);
                            }

                            //else
                            //{
                            //    state = State.Error;
                            //    isAbleToAdd = false;
                            //}

                            break;
                        }                       

                    case State.Constant:
                        {
                            if (char.IsWhiteSpace(symbol)) state = State.Start;

                            else if (char.IsDigit(symbol))
                            {
                                isAbleToAdd = false;
                                state = State.Constant;
                                lexBufCur.Append(symbol);
                            }

                            else if (symbol == '<')

                            {
                                state = State.ReverseComparison;
                                lexBufNext.Append(symbol);
                            }

                            else if (symbol == '>')
                            {
                                state = State.Comparison;
                                lexBufNext.Append(symbol);
                            }

                            else if (symbol == '=')
                            {
                                state = State.Assignment;
                                lexBufNext.Append(symbol);
                            }


                            else if (symbol == '+' || symbol == '-' || symbol == '/' || symbol == '*')
                            {
                                state = State.ArithmeticOperation;
                                lexBufNext.Append(symbol);
                            }

                            else
                            {
                                state = State.Error;
                                isAbleToAdd = false;
                            }

                            break;
                        }                        

                    case State.Identifier:
                        {
                            if (char.IsWhiteSpace(symbol)) state = State.Start;

                            else if (char.IsDigit(symbol) || char.IsLetter(symbol))
                            {
                                state = State.Identifier;
                                isAbleToAdd = false;
                                lexBufCur.Append(symbol);
                            }

                            else if (symbol == '<')
                            {
                                state = State.ReverseComparison;
                                lexBufNext.Append(symbol);
                            }

                            else if (symbol == '>')
                            {
                                state = State.Comparison;
                                lexBufNext.Append(symbol);
                            }

                            else if (symbol == '=')
                            {
                                state = State.Assignment;
                                lexBufNext.Append(symbol);
                            }

                            else if (symbol == '+' || symbol == '-' || symbol == '/' || symbol == '*')
                            {
                                state = State.ArithmeticOperation;
                                lexBufNext.Append(symbol);
                            }

                            else if (symbol == ':')
                            {
                                state = State.Assignment;
                                lexBufNext.Append(symbol);
                            }

                            else
                            {
                                state = State.Error;
                                isAbleToAdd = false;
                            }

                            break;
                        }                        

                    case State.ArithmeticOperation:
                        {
                            if (char.IsWhiteSpace(symbol))
                            {
                                state = State.Start;
                            }

                            else if (char.IsLetter(symbol))
                            {
                                state = State.Identifier;
                                lexBufNext.Append(symbol);
                            }

                            else if (char.IsDigit(symbol))
                            {
                                state = State.Constant;
                                lexBufNext.Append(symbol);
                            }

                            else if(symbol == '-' || symbol == '+' || symbol == '/' || symbol == '*')
                            {
                                state = State.ArithmeticOperation;
                                lexBufNext.Append(symbol);
                            }

                            else
                            {
                                state = State.Error;
                                isAbleToAdd = false;
                            }

                            break;
                        }                        
                }

                if (isAbleToAdd)
                {
                    AddLexeme(prevState, lexBufCur.ToString());
                    lexBufCur = new StringBuilder(lexBufNext.ToString());
                    lexBufNext.Clear();
                }

                textIndex++;
            }

            return state == State.Final;
        }

        private void AddLexeme(State prevState, string value)
        {
            LexemeType lexType = LexemeType.Undefined;

            LexemeClass lexClass = LexemeClass.Undefined;

            if (prevState == State.ArithmeticOperation)
            {
                lexType = LexemeType.ArithmeticOperation;
                lexClass = LexemeClass.SpecialSymbols;
            }
            else if (prevState == State.Assignment)
            {
                lexClass = LexemeClass.SpecialSymbols;
                if (value == "==")
                {
                    lexType = LexemeType.Relation;
                }
                else
                {
                    lexType = LexemeType.Assignment;
                }
            }
            else if (prevState == State.Constant)
            {
                lexType = LexemeType.Undefined;
                lexClass = LexemeClass.Constant;
            }
            else if (prevState == State.ReverseComparison)
            {
                lexType = LexemeType.Relation;
                lexClass = LexemeClass.SpecialSymbols;
            }
            else if (prevState == State.Comparison)
            {
                lexType = LexemeType.Relation;
                lexClass = LexemeClass.SpecialSymbols;
            }
            else if (prevState == State.Identifier)
            {

                bool isKeyword = true;

                if(value == "not") lexType = LexemeType.Not;
                else if (value == "and") lexType = LexemeType.And;
                else if (value == "or") lexType = LexemeType.Or;
                else if (value == "loop") lexType = LexemeType.Loop;
                else if (value == "input") lexType = LexemeType.Input;
                else if (value == "output") lexType = LexemeType.Output;
                else if (value == "do") lexType = LexemeType.Do;
                else if (value == "while") lexType = LexemeType.While;
                else
                {
                    lexType = LexemeType.Undefined;
                    isKeyword = false;
                }

                if (isKeyword) lexClass = LexemeClass.Keyword;
                else lexClass = LexemeClass.Identifier;
            }

            var lexeme = new Lexeme
            {
                Class = lexClass,
                Type = lexType,
                Value = value.Trim(),
            };


            if (lexeme.Value.Length > 0)
            {
                Lexemes.Add(lexeme);
            }

        }
    }
}

