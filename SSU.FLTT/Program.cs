using System;
using System.Collections.Generic;
using SSU.FLTT.Automat;
using System.IO;
using System.Text;
using System.Text.Json;
//using System.Text.Json.Serialization;

namespace SSU.FLTT
{
    class Program
    {
        private const string AUTOMATH_PATH = @"..\..\..\..\SSU.FLTT\automat.txt";
        private const string AUTOMATH_INFO_PATH = @"..\..\..\..\SSU.FLTT\automat-info.txt";

        static void Main(string[] args)
        {
            var automatDeterminateWays = new Dictionary<string, Dictionary<char, List<string>>>()
            {
                {"S1" , new Dictionary<char, List<string>>()
                    {
                        {'a', new List<string>(){"S1"}},
                        {'b', new List<string>(){"S2"}},
                        {'c', new List<string>(){"S3"}},
                    }
                },
                {"S2" , new Dictionary<char, List<string>>()
                    {
                        {'a', new List<string>(){"S2"}},
                        {'b', new List<string>(){"S3"}},
                        {'c', new List<string>(){"S4"}}
                    }
                },
                {"S3" , new Dictionary<char, List<string>>()
                    {
                        {'a', new List<string>(){"S3"}},
                        {'b', new List<string>(){"S4"}},
                        {'c', new List<string>(){"S1"}}
                    }
                },
                {"S4" , new Dictionary<char, List<string>>()
                    {
                        {'a', new List<string>(){"S4"}},
                        {'b', new List<string>(){"S1"}},
                        {'c', new List<string>(){"S2"}}
                    }
                }
            };

            var automatNonDeterminateWays = new Dictionary<string, Dictionary<string, List<string>>>()
            {
                {"S1" , new Dictionary<string, List<string>>()
                    {                        
                        {"a", new List<string>(){"S1", "S3"}},
                        {"b", new List<string>(){"S2"}},
                    }
                },
                {"S2" , new Dictionary<string, List<string>>()
                    {
                        {"EPSILON", new List<string>(){"S4"}},
                        {"a", new List<string>(){"S4"}},
                        {"b", new List<string>(){"S2"}},
                    }
                },
                {"S3" , new Dictionary<string, List<string>>()
                    {
                        {"a", new List<string>(){"S1"}},
                        {"b", new List<string>(){"S2"}},
                    }
                },
                {"S4" , new Dictionary<string, List<string>>()
                    {
                        {"a", new List<string>(){"S4"}},
                        {"b", new List<string>(){"S4", "S2"}},
                    }
                }
            };


            var deterAlphabet = new List<char>() { 'a', 'b', 'c', 'd' };
            var deterAuto = new Automat<string, char>("S1", deterAlphabet, automatDeterminateWays);

            string deterString = "baacbbba";
            deterAuto.Run(deterString);
            Console.WriteLine();
            //deterAuto.Run(deterString, "S2");
            //Console.WriteLine();
            //deterAuto.Run(deterString, "S3");
            //Console.WriteLine();
            //deterAuto.Run(deterString, "S4");

            Console.WriteLine("\n");

            var nonDeterAlphabet = new List<string>() { "a", "b" };
            var nonDeterAuto = new Automat<string, string>("S1", nonDeterAlphabet, "EPSILON", automatNonDeterminateWays, cleanStatesQueue: false);

            string nonDeterString = "bab";
            nonDeterAuto.Run(nonDeterString);

            Console.WriteLine("\n");


            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            string autoInfo = JsonSerializer.Serialize(automatNonDeterminateWays, options);
            File.WriteAllText(AUTOMATH_INFO_PATH, autoInfo);
            string auto = JsonSerializer.Serialize(nonDeterAuto, options);
            File.WriteAllText(AUTOMATH_PATH, auto);

            string p = @"D:\GitClone\SSU.FLTT\SSU.FLTT\automat-info.txt";

            string jsonString = File.ReadAllText(p);
            var tempo = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<string>>>>(jsonString);

            Console.OutputEncoding = Encoding.UTF8;

            foreach (var el in tempo["S1"])
            {

                Console.WriteLine(el.Key);
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }
    }
}
