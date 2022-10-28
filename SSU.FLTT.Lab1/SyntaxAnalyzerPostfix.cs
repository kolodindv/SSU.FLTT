using System;
using System.Collections.Generic;

namespace SSU.FLTT.Labs
{
    class SyntaxAnalyzerPostfix
	{
		private List<Lexeme> _lexemeList;
		private IEnumerator<Lexeme> _lexemeEnumerator;

		private List<PostfixEntry> EntryList { get; set; }

		public bool Run(string code, out List<PostfixEntry> postfixEntries)
		{
			EntryList = new List<PostfixEntry>();

			LexicalAnalyser analyser = new();
			var result = analyser.Run(string.Join(Environment.NewLine, code));
			if (!result)
			{
				throw new Exception("Errors were occurred in lexical analyze");
			}

			bool res = IsDoWhileStatement(analyser.Lexemes);
			postfixEntries = new(EntryList);
			return res;
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
	}
}
