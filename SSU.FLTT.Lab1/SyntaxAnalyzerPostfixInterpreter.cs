using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSU.FLTT.Labs
{
    class SyntaxAnalyzerPostfixInterpreter
    {
        static void FormatOut(string str)
        {
            Console.Write("{0, 6} ", str);
        }

        private List<Lexeme> _lexemeList;
        private IEnumerator<Lexeme> _lexemeEnumerator;

        private List<PostfixEntry> EntryList { get; set; }
        private Stack<PostfixEntry> _stack;
        public List<string> Logs { get; set; }


        public bool Run(string code)
        {
            Logs = new List<string>();
            EntryList = new List<PostfixEntry>();

            _stack = new Stack<PostfixEntry>();

            LexicalAnalyser analyser = new();
            var result = analyser.Run(string.Join(Environment.NewLine, code));
            if (!result)
            {
                throw new Exception("Errors were occurred in lexical analyze");
            }

            bool syntaxResult = IsDoWhileStatement(analyser.Lexemes);
            if (syntaxResult)
            {
                Console.WriteLine(syntaxResult ? "Okay" : "It is not a while statement");
                foreach (var entry in EntryList)
                {
                    FormatOut($"{GetEntryString(entry)}");
                }
                Console.WriteLine();
                //Console.Write("ptr: ");
                for (int i = 0; i < EntryList.Count + 1; i++)
                {
                    FormatOut($"prt{i}");
                }
                Console.WriteLine();

                EnterVariableValues();
                Console.WriteLine("Result: ");
                Interpret();
                Console.WriteLine("--------------------------");
                Logs.ForEach(Console.WriteLine);
            }

            return true;
        }

        private bool IsDoWhileStatement(List<Lexeme> lexemeList)
        {
            var indFirst = EntryList.Count;
            _lexemeList = lexemeList;
            if (lexemeList.Count == 0) return false;

            _lexemeEnumerator = lexemeList.GetEnumerator();

            if (!_lexemeEnumerator.MoveNext() || _lexemeEnumerator.Current.Type != LexemeType.Do) { Support.Error("Ожидается do", _lexemeList.IndexOf(_lexemeEnumerator.Current)); }

            _lexemeEnumerator.MoveNext();


            while (IsStatement()) ;

            if (_lexemeEnumerator.Current == null || _lexemeEnumerator.Current.Type != LexemeType.Loop) { Support.Error("Ожидается loop", _lexemeList.IndexOf(_lexemeEnumerator.Current)); }

            _lexemeEnumerator.MoveNext();
            if (_lexemeEnumerator.Current == null || _lexemeEnumerator.Current.Type != LexemeType.While) { Support.Error("Ожидается while", _lexemeList.IndexOf(_lexemeEnumerator.Current)); }

            _lexemeEnumerator.MoveNext();
            if (!IsCondition()) return false;


            var indJmpExit = WriteCmdPtr(-1);
            WriteCmd(Cmd.JZ);

            WriteCmdPtr(indFirst);
            var indLast = WriteCmd(Cmd.JMP);
            SetCmdPtr(indJmpExit, indLast + 1);


            if (_lexemeEnumerator.MoveNext()) { Support.Error("Лишние символы", _lexemeList.IndexOf(_lexemeEnumerator.Current)); }
            return true;
        }

        private bool IsCondition()
        {
            if (!IsLogicalExpression()) return false;
            while (_lexemeEnumerator.Current != null && _lexemeEnumerator.Current.Type == LexemeType.Or)
            {
                _lexemeEnumerator.MoveNext();
                if (!IsLogicalExpression()) return false;

                WriteCmd(Cmd.OR);
            }
            return true;
        }

        private bool IsLogicalExpression()
        {
            if (!RelationalExpression()) return false;
            while (_lexemeEnumerator.Current != null && _lexemeEnumerator.Current.Type == LexemeType.And)
            {
                _lexemeEnumerator.MoveNext();
                if (!RelationalExpression()) return false;

                WriteCmd(Cmd.AND);
            }
            return true;
        }

        private bool RelationalExpression()
        {
            if (!IsOperand()) return false;
            if (_lexemeEnumerator.Current != null && _lexemeEnumerator.Current.Type == LexemeType.Relation)
            {
                var cmd = _lexemeEnumerator.Current.Value switch
                {
                    "<" => Cmd.CMPL,
                    "<=" => Cmd.CMPLE,
                    ">" => Cmd.CMPG,
                    ">=" => Cmd.CMPGE,
                    "==" => Cmd.CMPE,
                    "<>" => Cmd.CMPNE,
                    _ => throw new ArgumentException(_lexemeEnumerator.Current.Value)
                };

                _lexemeEnumerator.MoveNext();
                if (!IsOperand()) return false;

                WriteCmd(cmd);
            }
            return true;
        }

        private bool IsIdentifier()
        {
            if (_lexemeEnumerator.Current == null || _lexemeEnumerator.Current.Class != LexemeClass.Identifier)
            {
                Support.Error("Ожидается переменная", _lexemeList.IndexOf(_lexemeEnumerator.Current));
                return false;
            }

            WriteVar(_lexemeList.IndexOf(_lexemeEnumerator.Current));

            _lexemeEnumerator.MoveNext();
            return true;
        }

        private bool IsOperand()
        {
            if (_lexemeEnumerator.Current == null || (_lexemeEnumerator.Current.Class != LexemeClass.Identifier && _lexemeEnumerator.Current.Class != LexemeClass.Constant))
            {
                Support.Error("Ожидается переменная или константа", _lexemeList.IndexOf(_lexemeEnumerator.Current));
                return false;
            }

            if (_lexemeEnumerator.Current.Class == LexemeClass.Identifier)
            {
                WriteVar(_lexemeList.IndexOf(_lexemeEnumerator.Current));
            }
            else
            {
                WriteConst(_lexemeList.IndexOf(_lexemeEnumerator.Current));
            }

            _lexemeEnumerator.MoveNext();
            return true;
        }

        private bool IsLogicalOperation()
        {
            if (_lexemeEnumerator.Current == null || (_lexemeEnumerator.Current.Type != LexemeType.And && _lexemeEnumerator.Current.Type != LexemeType.Or))
            {
                Support.Error("Ожидается логическая операция", _lexemeList.IndexOf(_lexemeEnumerator.Current));
                return false;
            }
            _lexemeEnumerator.MoveNext();
            return true;
        }

        private bool IsStatement()
        {
            if (_lexemeEnumerator.Current != null && _lexemeEnumerator.Current.Type == LexemeType.Loop) return false;

            if (_lexemeEnumerator.Current == null || _lexemeEnumerator.Current.Class != LexemeClass.Identifier)
            {
                if (_lexemeEnumerator.Current.Type == LexemeType.Output)
                {
                    _lexemeEnumerator.MoveNext();
                    if (!IsOperand()) return false;

                    WriteCmd(Cmd.OUTPUT);

                    return true;
                }

                if (_lexemeEnumerator.Current.Type == LexemeType.Input)
                {
                    _lexemeEnumerator.MoveNext();
                    if (!IsIdentifier()) return false;

                    WriteCmd(Cmd.INPUT);

                    return true;
                }



                Support.Error("Ожидается переменная", _lexemeList.IndexOf(_lexemeEnumerator.Current));
                return false;
            }

            WriteVar(_lexemeList.IndexOf(_lexemeEnumerator.Current));

            _lexemeEnumerator.MoveNext();

            if (_lexemeEnumerator.Current == null || _lexemeEnumerator.Current.Type != LexemeType.Assignment)
            {
                Support.Error("Ожидается присваивание", _lexemeList.IndexOf(_lexemeEnumerator.Current));
                return false;
            }
            _lexemeEnumerator.MoveNext();

            if (!IsArithmeticExpression()) return false;

            WriteCmd(Cmd.SET);

            return true;
        }

        private bool IsArithmeticExpression()
        {
            if (!IsOperand()) return false;
            while (_lexemeEnumerator.Current.Type == LexemeType.ArithmeticOperation)
            {
                var cmd = _lexemeEnumerator.Current.Value switch
                {
                    "+" => Cmd.ADD,
                    "-" => Cmd.SUB,
                    "*" => Cmd.MUL,
                    "/" => Cmd.DIV,
                    _ => throw new ArgumentException(_lexemeEnumerator.Current.Value)
                };

                _lexemeEnumerator.MoveNext();
                if (!IsOperand()) return false;

                WriteCmd(cmd);
            }
            return true;
        }

        private int WriteCmd(Cmd cmd)
        {
            var command = new PostfixEntry
            {
                EntryType = EntryType.Cmd,
                Cmd = cmd,
            };
            EntryList.Add(command);
            return EntryList.Count - 1;
        }

        private int WriteVar(int index)
        {
            var variable = new PostfixEntry
            {
                EntryType = EntryType.Var,
                Value = _lexemeList[index].Value
            };
            EntryList.Add(variable);
            return EntryList.Count - 1;
        }

        private int WriteConst(int index)
        {
            var variable = new PostfixEntry
            {
                EntryType = EntryType.Const,
                Value = _lexemeList[index].Value
            };
            EntryList.Add(variable);
            return EntryList.Count - 1;
        }

        private int WriteCmdPtr(int ptr)
        {
            var cmdPtr = new PostfixEntry
            {
                EntryType = EntryType.CmdPtr,
                CmdPtr = ptr,
            };
            EntryList.Add(cmdPtr);
            return EntryList.Count - 1;
        }

        private void SetCmdPtr(int index, int ptr)
        {
            EntryList[index].CmdPtr = ptr;
        }

        //------------------------------------------------------------
        private void Interpret()
        {

            int temp;
            int pos = 0;
            Log(pos);
            while (pos < EntryList.Count)
            {
                if (EntryList[pos].EntryType == EntryType.Cmd)
                {
                    var cmd = EntryList[pos].Cmd;
                    switch (cmd)
                    {
                        case Cmd.JMP:
                            pos = PopVal();
                            break;
                        case Cmd.JZ:
                            temp = PopVal();
                            if (PopVal() != 0) pos++;
                            else pos = temp;
                            break;
                        case Cmd.SET:
                            SetVarAndPop(PopVal());
                            pos++;
                            break;
                        case Cmd.ADD:
                            PushVal(PopVal() + PopVal());
                            pos++;
                            break;
                        case Cmd.SUB:
                            PushVal(-PopVal() + PopVal());
                            pos++;
                            break;
                        case Cmd.MUL:
                            PushVal(PopVal() * PopVal());
                            pos++;
                            break;
                        case Cmd.DIV:
                            PushVal((int)(1.0 / PopVal() * PopVal()));
                            pos++;
                            break;
                        case Cmd.AND:
                            PushVal((PopVal() != 0 && PopVal() != 0) ? 1 : 0);
                            pos++;
                            break;
                        case Cmd.OR:
                            PushVal((PopVal() != 0 || PopVal() != 0) ? 1 : 0);
                            pos++;
                            break;
                        case Cmd.CMPE:
                            PushVal((PopVal() == PopVal()) ? 1 : 0);
                            pos++;
                            break;
                        case Cmd.CMPNE:
                            PushVal((PopVal() != PopVal()) ? 1 : 0);
                            pos++;
                            break;
                        case Cmd.CMPL:
                            PushVal((PopVal() > PopVal()) ? 1 : 0);
                            pos++;
                            break;
                        case Cmd.CMPLE:
                            PushVal((PopVal() >= PopVal()) ? 1 : 0);
                            pos++;
                            break;
                        case Cmd.CMPG:
                            PushVal((PopVal() < PopVal()) ? 1 : 0);
                            pos++;
                            break;
                        case Cmd.CMPGE:
                            PushVal((PopVal() <= PopVal()) ? 1 : 0);
                            pos++;
                            break;
                        case Cmd.INPUT:
                            Console.WriteLine("Введите значение:");
                            int ind = int.Parse(Console.ReadLine());
                            SetVarAndPop(ind);
                            pos++;
                            break;
                        case Cmd.OUTPUT:
                            Console.WriteLine(PopVal());
                            pos++;
                            break;
                        default:
                            break;
                    }
                }
                else PushElm(EntryList[pos++]);

                if (pos < EntryList.Count)
                    Log(pos);

            }
        }

        private int PopVal()
        {
            if (_stack.Count != 0)
            {
                var obj = _stack.Pop();
                return obj.EntryType switch
                {
                    EntryType.Var => obj.CurrentValue.Value,
                    EntryType.Const => Convert.ToInt32(obj.Value),
                    //EntryType.Const => obj.CurrentValue.Value,
                    EntryType.CmdPtr => obj.CmdPtr.Value,
                    _ => throw new ArgumentException("obj.EntryType")
                };
            }
            else
            {
                return 0;
            }
        }

        private void PushVal(int val)
        {
            var entry = new PostfixEntry
            {
                EntryType = EntryType.Const,
                Value = val.ToString()
            };
            _stack.Push(entry);
        }

        private void PushElm(PostfixEntry entry)
        {
            if (entry.EntryType == EntryType.Cmd)
            {
                throw new ArgumentException("EntryType");
            }
            _stack.Push(entry);
        }

        private void SetVarAndPop(int val)
        {
            var variable = _stack.Pop();
            if (variable.EntryType != EntryType.Var)
            {
                throw new ArgumentException("EntryType");
            }
            SetValuesToVariables(variable.Value, val);
        }

        private void Log(int pos)
        {
            Logs.Add($"Позиция: {pos} | Элемент: {GetEntryString(EntryList[pos])} | Стек: {GetStackState()} | Значения переменных: {GetVarValues()}");
        }

        private string GetEntryString(PostfixEntry entry)
        {
            if (entry.EntryType == EntryType.Var) return entry.Value;
            else if (entry.EntryType == EntryType.Const) return entry.Value;
            else if (entry.EntryType == EntryType.Cmd) return entry.Cmd.ToString();
            else if (entry.EntryType == EntryType.CmdPtr) return entry.CmdPtr.ToString();
            throw new ArgumentException("PostfixEntry");
        }

        private string GetStackState()
        {
            IEnumerable<PostfixEntry> entries = _stack;
            var sb = new StringBuilder();
            entries?.ToList().ForEach(e => sb.Append($"{GetEntryString(e)} "));
            return sb.ToString();
        }

        private string GetVarValues()
        {
            var sb = new StringBuilder();
            EntryList.Where(e => e.EntryType == EntryType.Var)
                .Select(e => new { e.Value, e.CurrentValue })
                .Distinct()
                .ToList()
                .ForEach(e => sb.Append($"{e.Value} = {e.CurrentValue}; "));
            return sb.ToString();
        }

        private IEnumerable<PostfixEntry> GetVariables()
        {
            return EntryList.Where(e => e.EntryType == EntryType.Var);
        }

        private void SetValuesToVariables(string name, int value)
        {
            GetVariables().Where(v => v.Value == name)
                .ToList()
                .ForEach(v => v.CurrentValue = value);
        }

        private void EnterVariableValues()
        {
            try
            {
                Console.WriteLine("Enter variable values:");

                var variables = GetVariables().Select(v => v.Value).Distinct();
                foreach (var variable in variables)
                {
                    Console.Write($"{variable} = ");
                    var value = int.Parse(Console.ReadLine());
                    SetValuesToVariables(variable, value);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

}
