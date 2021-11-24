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

        private List<TStateName> _endStates;
        public List<TStateName> EndStates
        {
            get
            {
                return _endStates;
            }
            set
            {
                _endStates = value;
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

        private Dictionary<TStateName, State<TStateName>> _states;        

        private Queue<State<TStateName>> _statesQueue;
        private HashSet<State<TStateName>> _statesSet;

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
                _haveEpsilonTransitions = true;
                _epsilonMover = value;
                InitNewStatesQueue(_startState);
            }
        }        

        private Automat(StatesQueueOptions workOption = StatesQueueOptions.AllWays)
        {
            WorkOption = workOption;
            _haveEpsilonTransitions = false;
        }

        public Automat(TStateName startState, List<TStateName> endStates,
                       Dictionary<TStateName, Dictionary<TMover, List<TStateName>>> transitionDictionary,
                       StatesQueueOptions workOption = StatesQueueOptions.AllWays)
            :this(workOption)            
        {
            TransitionDictionary = transitionDictionary;
            StartState = startState;
            EndStates = endStates;
        }

        public Automat(TStateName startState, List<TStateName> endStates,
                       string transitionDictionaryInputPath,
                       StatesQueueOptions workOption = StatesQueueOptions.AllWays)
            :this(startState, endStates, GetDictionaryFromJson(transitionDictionaryInputPath), workOption) 
        { }

        public Automat(TStateName startState, List<TStateName> endStates,
                       TMover epsilonSymbol,
                       Dictionary<TStateName, Dictionary<TMover, List<TStateName>>> transitionDictionary,
                       StatesQueueOptions workOption = StatesQueueOptions.AllWays)
            : this(startState, endStates, transitionDictionary, workOption)
        {
            EpsilonMover = epsilonSymbol;
        }

        public Automat(TStateName startState, List<TStateName> endStates,
                       TMover epsilonSymbol,
                       string transitionDictionaryInputPath,
                       StatesQueueOptions workOption = StatesQueueOptions.AllWays)
            : this(startState, endStates, epsilonSymbol, GetDictionaryFromJson(transitionDictionaryInputPath), workOption)
        {  }        

        public bool Run(string input, out List<TStateName> res)
        {
            Console.WriteLine($"Создана очередь состояний с начальным состоянием {_statesQueue.Peek().Name}");
            var inputQueue = InputCommandByAplhabet(input);
            while (inputQueue.Count > 0)
            {
                DoTransitions(inputQueue.Dequeue());
            }

            res = GetResult();
            return InLanguage(res);
        }
        public bool Run(string input)
        {
            return Run(input, out _);
        }
        public bool Run(string input, TStateName startName, out List<TStateName> res)
        {
            StartState = startName;
            return Run(input, out res);
        }
        public bool Run(string input, TStateName startName)
        {
            return Run(input, startName, out _);
        }

        private void InitStatesAndTransitionDictionary(Dictionary<TStateName, Dictionary<TMover, List<TStateName>>> transitionDictionary)
        {
            _states= new Dictionary<TStateName, State<TStateName>>();

            foreach (var st in transitionDictionary)
            {
                _states.Add(st.Key, new State<TStateName>(st.Key));
            }

            InitAlphabet();
        }
        private void InitNewStatesQueue(TStateName startState)
        {
            _statesQueue = new Queue<State<TStateName>>();
            _statesQueue.Enqueue(_states[startState]);

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
            mover = default;

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
        
        private bool AddEpsilonTransitions(State<TStateName> startState, List<State<TStateName>> tempEpsilonsStates)
        {
            bool addFlag;
            if (TransitionDictionary[startState.Name].TryGetValue(EpsilonMover, out List<TStateName> endStateNames))
            {
                addFlag = false;
                foreach (var endStateName in endStateNames)
                {
                    if (!tempEpsilonsStates.Contains(_states[endStateName]))                    
                    {
                        tempEpsilonsStates.Add(_states[endStateName]);
                        addFlag = true;
                    }                    
                }
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

            var stopPoint = _statesQueue.Count;

            for (int i = 0; i < stopPoint; i++)
            {
                var tempList = new List<State<TStateName>>
                {
                    _statesQueue.Dequeue()
                };

                do
                {
                    continueFlag = false;
                    var listCount = tempList.Count;
                    for (int ind = 0; ind < listCount; ind++)
                    {
                        if (AddEpsilonTransitions(tempList[ind], tempList))
                        {
                            continueFlag = true;
                        }
                    }
                }
                while (continueFlag);

                foreach (var el in tempList)
                {
                    _statesQueue.Enqueue(el);
                }

                ProcessStatesQueue();
            }           

        }

        private void AddMoverTransitions(State<TStateName> startState, TMover currentMover)
        {
            if (TransitionDictionary[startState.Name].TryGetValue(currentMover, out List<TStateName> endStateNames))
            {
                foreach (var endStateName in endStateNames)
                {
                    _statesQueue.Enqueue(_states[endStateName]);
                }
            }

        }
        private void AddAllMoverTransitions(TMover currentMover)
        {
            int countSteps = _statesQueue.Count;
            for (int i = 0; i < countSteps; i++)
            {
                var state = _statesQueue.Dequeue();
                state.StateDo();
                AddMoverTransitions(state, currentMover);
            }
            ProcessStatesQueue();
        }

        private void DoTransitions(TMover currentMover)
        {
            if (currentMover == null)
            {
                Console.WriteLine("Отсутсвтует инициатор");
                return;
            }

            Console.WriteLine($"Для инициатора {currentMover} из состояний:");

            AddAllMoverTransitions(currentMover);

            if (_haveEpsilonTransitions)
            {
                AddAllEpsilonTransitions();
            }               
            
            Console.WriteLine("\n->");            
        }

        private void CleanStatesQueue()
        {
            _statesSet = new HashSet<State<TStateName>>(_statesQueue);
            _statesQueue = new Queue<State<TStateName>>(_statesSet);
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
            var states = _statesQueue.ToList<State<TStateName>>();
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
        
        private bool InLanguage(List<TStateName> resultStates)
        {
            bool result = false;

            foreach(var st in resultStates)
            {
                if (EndStates.Contains<TStateName>(st))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }
    }
}
