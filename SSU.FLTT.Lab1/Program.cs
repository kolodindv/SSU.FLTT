using System;
using System.Collections.Generic;
using System.Text;

namespace SSU.FLTT.Lab1
{
    class Program
    {
        static List<string> codeStrings = new List<string>
            {
                "do",
                "	input ind",
                //"	input 13",
                "	a=12342",
                "	a= a + 2",
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
                "loop while ind >= 20"
            };
        
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

        static void FormatOut(string str)
        {
            Console.Write("{0, 6} ", str);
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
            //using var stream = new StreamReader("input.txt");
            //var code = stream.ReadToEnd();
            var analyser = new SyntaxAnalyzerPostfix();
            try
            {
                var result = analyser.Run(string.Join(Environment.NewLine, codeStrings));
                Console.WriteLine(result ? "Okay" : "It is not a while statement");
                foreach (var entry in analyser.EntryList)
                {
                    if (entry.EntryType == EntryType.Var) FormatOut(entry.Value);
                    else if (entry.EntryType == EntryType.Const) FormatOut(entry.Value);
                    else if (entry.EntryType == EntryType.Cmd) FormatOut(entry.Cmd.ToString());
                    else if (entry.EntryType == EntryType.CmdPtr) FormatOut($"ptr{entry.CmdPtr}");
                }
                Console.WriteLine();
                for (int i = 0; i < analyser.EntryList.Count+1; i++)
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



        static void Main(string[] args)
        {
            //Lab1();
            //Lab2();
            Lab3();
        }
    }
}



















































