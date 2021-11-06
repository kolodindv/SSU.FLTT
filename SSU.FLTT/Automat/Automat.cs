using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSU.FLTT.Automat
{
    class Automat<TStateName, TMover>
    {
        private bool cleanStatesQueue;

        private Dictionary<TStateName,
            Dictionary<TMover, List<TStateName>>> TransitionDictionary;

        private Dictionary<TStateName, State<TStateName>> States;

        private Queue<State<TStateName>> StatesQueue;
        private HashSet<State<TStateName>> StatesSet;

        private List<TMover> alphabet;
        private TMover epsilonMover;

        public Automat(TStateName startState,
                       List<TMover> alphabet,
                       Dictionary<TStateName, Dictionary<TMover, List<TStateName>>> transitionDictionary, 
                       bool cleanStatesQueue = true)
        {
            this.alphabet = alphabet;
            this.cleanStatesQueue = cleanStatesQueue;
            InitStatesAndTransitionDictionary(transitionDictionary);
            InitNewStatesQueue(startState);
        }

        public Automat(TStateName startState,
                       List<TMover> alphabet, TMover epsilonSymbol,
                       Dictionary<TStateName, Dictionary<TMover, List<TStateName>>> transitionDictionary,
                       bool cleanStatesQueue = true)
            : this(startState, alphabet, transitionDictionary, cleanStatesQueue)
        {
            this.epsilonMover = epsilonSymbol;
        }

        private bool CanGetNewMoverFromString(out TMover mover, string input)
        {
            mover = default(TMover);

            //пришла пустая строка
            if (input.Length == 0)
            {                
                return false;
            }
            
            bool canFlag = false;  

            foreach (var el in alphabet)
            {
                if (input.StartsWith(el.ToString()))
                {
                    canFlag = true;
                    mover = el;
                    break;
                }                    
            }
            return canFlag;
        }
        private Queue<TMover> InputCommandByAplhabet(string input) 
        {
            var tempQueue = new Queue<TMover>();
            var inputBuilder = new StringBuilder(input);

            while(CanGetNewMoverFromString(out TMover mover, inputBuilder.ToString()))
            {
                tempQueue.Enqueue(mover);
                inputBuilder.Remove(0, mover.ToString().Length);
            }

            if(inputBuilder.Length > 0)
            {
                Console.WriteLine("Введена строка несоответствующая алфавиту автомата");
                Console.Write($"Остаток строки: {inputBuilder}\n");
                tempQueue.Clear();
            }

            return tempQueue;

        }        
        
        private bool AddEpsilonTransitions(State<TStateName> startState, Queue<State<TStateName>> tempEpsilonsStates)
        {
            bool addFlag;
            if (TransitionDictionary[startState.Name].TryGetValue(epsilonMover, out List<TStateName> endStateNames))
            {

                foreach (var endStateName in endStateNames)
                {
                    tempEpsilonsStates.Enqueue(States[endStateName]);
                    StatesQueue.Enqueue(States[endStateName]);
                }
                addFlag = true;
            }
            else
            {
                addFlag = false;
            }
            return addFlag;
            
        }
        private void AddAllEpsilonTransitions()
        {
            bool continueFlag;
            var tempQueue = new Queue<State<TStateName>>(StatesQueue);  

            do
            {
                continueFlag = false;
                
                while(tempQueue.Count > 0)
                {
                    if (AddEpsilonTransitions(tempQueue.Dequeue(), tempQueue))
                    {
                        continueFlag = true;
                    }                    
                }
            }
            while (continueFlag);

            
            

        }

        private void AddMoverTransitions(State<TStateName> startState, TMover currentMover)
        {
            if (TransitionDictionary[startState.Name].TryGetValue(currentMover, out List<TStateName> endStateNames))
            {
                foreach (var endStateName in endStateNames)
                {
                    StatesQueue.Enqueue(States[endStateName]);
                }
            }            
        }

        private void DoTransitions(TMover currentMover)
        {
            int countSteps = StatesQueue.Count;           

            if (currentMover == null)
            {
                Console.WriteLine("Отсутсвтует инициатор");
                return;
            }

            Console.WriteLine($"Для инициатора {currentMover} из состояний:");
            for (int i = 0; i < countSteps; i++)
            {
                var state = StatesQueue.Dequeue();
                state.StateDo();
                AddMoverTransitions(state, currentMover);
            }

            if(epsilonMover != null)
            {
                AddAllEpsilonTransitions();
            }            

            if (cleanStatesQueue)
            {
                CleanStatesQueue();
            }
            
            Console.WriteLine("\n->");            
        }
        private void CleanStatesQueue()
        {
            StatesSet = new HashSet<State<TStateName>>(StatesQueue);
            StatesQueue = new Queue<State<TStateName>>(StatesSet);
        }

        private void InitStatesAndTransitionDictionary(Dictionary<TStateName, Dictionary<TMover, List<TStateName>>> transitionDictionary)
        {
            TransitionDictionary = transitionDictionary;
            States = new Dictionary<TStateName, State<TStateName>>();

            foreach (var st in TransitionDictionary)
            {
                States.Add(st.Key, new State<TStateName>(st.Key));
            }

            StatesQueue = new Queue<State<TStateName>>();
        }
        private void InitNewStatesQueue(TStateName startState)
        {
            StatesQueue = new Queue<State<TStateName>>();
            StatesQueue.Enqueue(States[startState]);
            if (epsilonMover != null) 
            {
                AddAllEpsilonTransitions();
            } 
            Console.WriteLine($"Создана очередь состояний с начальным состоянием {StatesQueue.Peek().Name}");
        }        

        private List<State<TStateName>> GetResult()
        {
            Console.WriteLine("Результирующие состояния");
            var resultStates = StatesQueue.ToList<State<TStateName>>();
            StatesQueue.Clear();
            foreach (var state in resultStates)
            {
                state.StateDo();
            }
            Console.WriteLine();
            return resultStates;
        }

        public List<State<TStateName>> Run(string input)
        {
            var inputQueue = InputCommandByAplhabet(input);
            while (inputQueue.Count > 0)
            {
                DoTransitions(inputQueue.Dequeue());
            }                
            return GetResult();
        }
        public List<State<TStateName>> Run(string input, TStateName startName)
        {
            InitNewStatesQueue(startName);
            return Run(input);
        }
    }
}
