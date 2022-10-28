using System;
using System.Collections.Generic;

namespace SSU.FLTT.Labs
{
    public class SyntaxAnalyzer
    {
		private List<Lexeme> _lexemeList;
		private IEnumerator<Lexeme> _lexemeEnumerator;

		public bool Run(string code)
		{
			LexicalAnalyser analyser = new();
			var result = analyser.Run(string.Join(Environment.NewLine, code));
			if (!result)
			{
				throw new Exception("Errors were occurred in lexical analyze");
			}
			
			return IsDoWhileStatement(analyser.Lexemes);


		}

		private bool IsDoWhileStatement(List<Lexeme> lexemeList)
		{
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
			}
			return true;
		}

		private bool RelationalExpression()
		{
			if (!IsOperand()) return false;
			if (_lexemeEnumerator.Current != null && _lexemeEnumerator.Current.Type == LexemeType.Relation)
			{
				_lexemeEnumerator.MoveNext();
				if (!IsOperand()) return false;
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
					return true;
				}

				if (_lexemeEnumerator.Current.Type == LexemeType.Input)
				{
					_lexemeEnumerator.MoveNext();
					if (!IsIdentifier()) return false;
					return true;
				}

				Support.Error("Ожидается переменная", _lexemeList.IndexOf(_lexemeEnumerator.Current));
				return false;
			}
			_lexemeEnumerator.MoveNext();

			if (_lexemeEnumerator.Current == null || _lexemeEnumerator.Current.Type != LexemeType.Assignment)
			{
				Support.Error("Ожидается присваивание", _lexemeList.IndexOf(_lexemeEnumerator.Current));
				return false;
			}
			_lexemeEnumerator.MoveNext();

			if (!IsArithmeticExpression()) return false;

			return true;
		}

		private bool IsArithmeticExpression()
		{
			if (!IsOperand()) return false;
			while (_lexemeEnumerator.Current.Type == LexemeType.ArithmeticOperation)
			{
				_lexemeEnumerator.MoveNext();
				if (!IsOperand()) return false;
			}
			return true;
		}
	}
}

