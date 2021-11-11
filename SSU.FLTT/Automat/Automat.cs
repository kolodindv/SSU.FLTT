using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.Json;

namespace SSU.FLTT.Automat
{
    public enum StatesQueueOptions
    {
        UnicWays,
        AllWays
    }

    public class Automat<TStateName, TMover> 
    {
        private static Dictionary<TStateName, Dictionary<TMover, List<TStateName>>> GetDictionaryFromJson(string path)
        {
            string jsonString = File.ReadAllText(path);
            var tempoDictionary = JsonSerializer.Deserialize<Dictionary<TStateName, Dictionary<TMover, List<TStateName>>>>(jsonString);
            return tempoDictionary;
        }       

        private TStateName _startState;
        public TStateName StartState
        {
            get
            {
                return _startState;
            }
            set
            {
                _startState = value;
                InitNewStatesQueue(_startState);
            }
        }

        private StatesQueueOptions _workOption; 
        
        public StatesQueueOptions WorkOption
        {
            get
            {
                return _workOption;
            }
            set
            {
                _workOption = value;
            }
        }        

        private Dictionary<TStateName, Dictionary<TMover, List<TStateName>>> _transitionDictionary;
        private Dictionary<TStateName, Dictionary<TMover, List<TStateName>>> TransitionDictionary
        {
            get
            {
                return _transitionDictionary;
            }
            set
            {
                _transitionDictionary = value;
                InitStatesAndTransitionDictionary(_transitionDictionary);
            }
        }

        private Dictionary<TStateName, State<TStateName>> States;

        private Queue<State<TStateName>> StatesQueue;
        private HashSet<State<TStateName>> StatesSet;

        private HashSet<TMover> _alphabet;

        private bool _haveEpsilonTransitions;
        private TMover _epsilonMover;
        public TMover EpsilonMover
        {
            get
            {
                if (_haveEpsilonTransitions)
                {
                    return _epsilonMover;
                }
                else
                {                    
                    throw new NullReferenceException();
                }
            }
            private set
            {
                _epsilonMover = value;
                _haveEpsilonTransitions = true;
            }
        }        

        private Automat(StatesQueueOptions workOption = StatesQueueOptions.AllWays)
        {
            WorkOption = workOption;
            _haveEpsilonTransitions = false;
        }

        public Automat(TStateName startState,
                       Dictionary<TStateName, Dictionary<TMover, List<TStateName>>> transitionDictionary,
                       StatesQueueOptions workOption = StatesQueueOptions.AllWays)
            :this(workOption)            
        {
            TransitionDictionary = transitionDictionary;
            StartState = startState;
        }

        public Automat(TStateName startState,
                       string transitionDictionaryInputPath,
                       StatesQueueOptions workOption = StatesQueueOptions.AllWays)
            :this(startState, GetDictionaryFromJson(transitionDictionaryInputPath), workOption) 
        { }

        public Automat(TStateName startState,
                       TMover epsilonSymbol,
                       Dictionary<TStateName, Dictionary<TMover, List<TStateName>>> transitionDictionary,
                       StatesQueueOptions workOption = StatesQueueOptions.AllWays)
            : this(startState, transitionDictionary, workOption)
        {
            EpsilonMover = epsilonSymbol;
        }

        public Automat(TStateName startState,
                       TMover epsilonSymbol,
                       string transitionDictionaryInputPath,
                       StatesQueueOptions workOption = StatesQueueOptions.AllWays)
            : this(startState, epsilonSymbol, GetDictionaryFromJson(transitionDictionaryInputPath), workOption)
        {  }        

        public List<TStateName> Run(string input)
        {
            Console.WriteLine($"Создана очередь состояний с начальным состоянием {StatesQueue.Peek().Name}");
            var inputQueue = InputCommandByAplhabet(input);
            while (inputQueue.Count > 0)
            {
                DoTransitions(inputQueue.Dequeue());
            }            
            return GetResult();
        }
        public List<TStateName> Run(string input, TStateName startName)
        {
            StartState = startName;
            return Run(input);
        }

        private void InitStatesAndTransitionDictionary(Dictionary<TStateName, Dictionary<TMover, List<TStateName>>> transitionDictionary)
        {
            States = new Dictionary<TStateName, State<TStateName>>();

            foreach (var st in transitionDictionary)
            {
                States.Add(st.Key, new State<TStateName>(st.Key));
            }

            InitAlphabet();
        }
        private void InitNewStatesQueue(TStateName startState)
        {
            StatesQueue = new Queue<State<TStateName>>();
            StatesQueue.Enqueue(States[startState]);

            if (_haveEpsilonTransitions)
            {
                AddAllEpsilonTransitions();
            }           
        }
        private void InitAlphabet()
        {
            _alphabet = new HashSet<TMover>();
            foreach(var stateTransitions in TransitionDictionary)
            {
                foreach (var moverTransition in stateTransitions.Value)
                {
                    _alphabet.Add(moverTransition.Key);
                }
            }

            if (_haveEpsilonTransitions)
            {
                _alphabet.Remove(EpsilonMover);
            }
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

            foreach (var el in _alphabet)
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
            if (TransitionDictionary[startState.Name].TryGetValue(EpsilonMover, out List<TStateName> endStateNames))
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

            ProcessStatesQueue();
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

            ProcessStatesQueue();
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

            if(_haveEpsilonTransitions)
            {
                AddAllEpsilonTransitions();
            }               
            
            Console.WriteLine("\n->");            
        }

        private void CleanStatesQueue()
        {
            StatesSet = new HashSet<State<TStateName>>(StatesQueue);
            StatesQueue = new Queue<State<TStateName>>(StatesSet);
        }
        private void ProcessStatesQueue()
        {
            if (WorkOption == StatesQueueOptions.UnicWays)
            {
                CleanStatesQueue();
            }
        }

        private List<TStateName> GetResult()
        {
            Console.WriteLine("Результирующие состояния");
            var states = StatesQueue.ToList<State<TStateName>>();
            var resultStates = new List <TStateName>();

            foreach (var state in states)
            {
                state.StateDo();
                resultStates.Add(state.Name);
            }

            //reser automat
            InitNewStatesQueue(StartState);

            Console.WriteLine();
            
            return resultStates;
        }        
    }
}
