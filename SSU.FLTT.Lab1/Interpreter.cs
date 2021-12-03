using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSU.FLTT.Lab1
{
    class Interpreter
    {
        static void FormatOut(string str)
        {
            Console.Write("{0, 6} ", str);
        }

        private List<PostfixEntry> _entryList;
        public List<PostfixEntry> EntryList 
        { 
            get 
            {
                return _entryList;
            }            
        }

        private Stack<PostfixEntry> _stack;
        public List<string> Logs { get; set; }

        public bool Run(string code)
        {
            Logs = new List<string>();
            _entryList = new List<PostfixEntry>();

            _stack = new Stack<PostfixEntry>();

            SyntaxAnalyzerPostfix syntaxAnalyzerPostfix = new();

            bool syntaxResult = syntaxAnalyzerPostfix.Run(code, out _entryList);
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
