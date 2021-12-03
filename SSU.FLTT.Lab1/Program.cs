using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SSU.FLTT.Lab1
{
    class Program
    {
        static List<string> codeStrings = new List<string>
            {
                "do",
                //"	input ind",
                //"	input 13",
                "	a=12",
                //"	a= (a + 2) + b + c * (12 + a)",
                "	a=a+ 5",

                "	output a",
               // "",
               // "	ind = ind+ ind - while1",
               // //"	ind <> ind",
               // "	ind = 17",
               //// "	ind > ind",
               //// "	ind == 12",
               // "",
               // "	output ind",
                "",
                "loop while a <= 20"
            };

        static void FormatOut(string str)
        {
            Console.Write("{0, 6} ", str);
        }

        static void WriteCode()
        {
            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Encoding.Unicode;
            Console.WriteLine("Enter code for analyser:");
            foreach (var str in codeStrings)
            {
                Console.WriteLine(str);
            }
        }

        static void Lab1()
        {
            WriteCode();

            var analyser = new LexicalAnalyser();

            //var code = Console.ReadLine();
            //var result = analyser.Run(codeInString);

            var result = analyser.Run(string.Join(Environment.NewLine, codeStrings));

            var lexemes = analyser.Lexemes;

            Console.WriteLine("---------------------------------");

            Console.WriteLine("Result:");

            for (int i = 0; i < lexemes.Count; i++)
            {
                Console.WriteLine($"Index: {i}, Class: {lexemes[i].Class}, " +
                    $"Type: {lexemes[i].Type}, Value {lexemes[i].Value}");
            }
        }

        static void Lab2()
        {
            WriteCode();

            var analyser = new SyntaxAnalyzer();
            try
            {
                var result = analyser.Run(string.Join(Environment.NewLine, codeStrings));
                Console.WriteLine(result ? "Okay" : "It is not a while statement");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void Lab3()
        {
            WriteCode();
            var analyser = new SyntaxAnalyzerPostfix();
            try
            {
                var result = analyser.Run(string.Join(Environment.NewLine, codeStrings), out List<PostfixEntry> entryList);
                Console.WriteLine(result ? "Okay" : "It is not a while statement");
                foreach (var entry in entryList)
                {
                    if (entry.EntryType == EntryType.Var) FormatOut(entry.Value);
                    else if (entry.EntryType == EntryType.Const) FormatOut(entry.Value);
                    else if (entry.EntryType == EntryType.Cmd) FormatOut(entry.Cmd.ToString());
                    else if (entry.EntryType == EntryType.CmdPtr) FormatOut($"ptr{entry.CmdPtr}");
                }
                Console.WriteLine();
                for (int i = 0; i < entryList.Count+1; i++)
                {
                    FormatOut($"ptr{i}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine();
        }

        static void Lab4()
        {
            using var stream = new StreamReader(@"..\..\..\codeStrings.txt");
            var code = stream.ReadToEnd();
            Console.WriteLine(code);



            //var lexA = new LexicalAnalyser();
            //lexA.Run(code);
            //var lexemes = lexA.Lexemes;

            //Console.WriteLine("---------------------------------");

            //Console.WriteLine("Result:");

            //for (int i = 0; i < lexemes.Count; i++)
            //{
            //    Console.WriteLine($"Index: {i}, Class: {lexemes[i].Class}, " +
            //        $"Type: {lexemes[i].Type}, Value {lexemes[i].Value}");
            //}

            var analyser = new SyntaxAnalyzerPostfixInterpreter();
            try
            {
                var result = analyser.Run(code);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void Lab4UPdated()
        {
            using var stream = new StreamReader(@"..\..\..\codeStrings.txt");
            var code = stream.ReadToEnd();
            Console.WriteLine(code);

            Interpreter interpreter = new();
            try
            {
                interpreter.Run(code);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void Main(string[] args)
        {
            //Lab1();
            //Lab2();
            //Lab3();
            //Lab4();
            //Lab4UPdated();
        }
    }
}



















































