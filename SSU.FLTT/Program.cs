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

            var deterAuto = new Automat<string, char>("S1", automatDeterminateWays);

            string deterString = "baacbbba";
            deterAuto.Run(deterString);  
            Console.WriteLine("\n\n");

            var nonDeterAuto = new Automat<string, string>("S1", automatNonDeterminateWays, StatesQueueOptions.UnicWays);

            string nonDeterString = "babab";
            nonDeterAuto.Run(nonDeterString);
            Console.WriteLine();
            nonDeterAuto.WorkOption = StatesQueueOptions.AllWays;
            nonDeterAuto.Run(nonDeterString);

            Console.WriteLine("\n\n");

            string p = @"D:\GitClone\SSU.FLTT\SSU.FLTT\automat-info.txt";
            var nonDeterEpsAuto = new Automat<string, string>("S1", "EPSILON", p, StatesQueueOptions.UnicWays);

            string nonDeterEpsString = "babab";
            nonDeterEpsAuto.Run(nonDeterEpsString);
            Console.WriteLine();
            nonDeterEpsAuto.WorkOption = StatesQueueOptions.AllWays;
            nonDeterEpsAuto.Run(nonDeterEpsString);


            //var options = new JsonSerializerOptions
            //{
            //    WriteIndented = true,
            //};
            //string autoInfo = JsonSerializer.Serialize(automatNonDeterminateWays, options);
            //File.WriteAllText(AUTOMATH_INFO_PATH, autoInfo);
            //string auto = JsonSerializer.Serialize(nonDeterAuto, options);
            //File.WriteAllText(AUTOMATH_PATH, auto);

            //

            //var tempo = Automat<string, string>.getDictionaryFromJson(p);  

            //string jsonString = File.ReadAllText(p);
            //var tempo = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<string>>>>(jsonString);



            Console.OutputEncoding = Encoding.UTF8;

           
            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }
    }
}
