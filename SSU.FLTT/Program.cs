using System;
using System.Collections.Generic;
using SSU.FLTT.Automat;

namespace SSU.FLTT
{
    class Program
    {
        static void Main(string[] args)
        {
            //TStateName startState,
            //           Dictionary< TStateName, Dictionary < TMover, List < TStateName >>> transitionDictionary,
            //           Queue<TMover> inputMoversQueue
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
            var automatDeterminateInput = new Queue<char>("baacbbba");

            var automatNonDeterminateWays = new Dictionary<string, Dictionary<char, List<string>>>()
            {
                {"S1" , new Dictionary<char, List<string>>()
                    {
                        {'a', new List<string>(){"S1", "S3"}},
                        {'b', new List<string>(){"S2"}},
                    }
                },
                {"S2" , new Dictionary<char, List<string>>()
                    {
                        {'a', new List<string>(){"S2", "S4"}},
                        {'b', new List<string>(){"S2"}},
                    }
                },
                {"S3" , new Dictionary<char, List<string>>()
                    {
                        {'a', new List<string>(){"S1"}},
                        {'b', new List<string>(){"S2"}},
                    }
                },
                {"S4" , new Dictionary<char, List<string>>()
                    {
                        {'a', new List<string>(){"S4"}},
                        {'b', new List<string>(){"S4", "S2"}},
                    }
                }
            };
            var automatNonDeterminateInput = new Queue<char>("aaaaab");

            //while (enterStringQueue.Count > 0)
            //{
            //    Console.WriteLine(enterStringQueue.Dequeue());
            //}

            var deterAuto = new Automat<string, char>("S1", automatDeterminateWays, automatDeterminateInput);
            deterAuto.Run();

            Console.WriteLine("\n");
            var nonDeterAuto = new Automat<string, char>("S1", automatNonDeterminateWays, automatNonDeterminateInput);
            nonDeterAuto.Run();

            Console.ReadLine();
        }
    }
}
