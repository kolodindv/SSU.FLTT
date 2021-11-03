using System;
using System.Collections.Generic;
using System.Linq;

namespace SSU.FLTT.Automat
{
    class Automat <TStateName, TMover>
    {
        private Dictionary<TStateName,
            Dictionary<TMover, List<TStateName>>> TransitionDictionary;

        private Dictionary<TStateName, State<TStateName>> States;

        private Queue<State<TStateName>> StatesQueue;

        private Queue<TMover> InputMoversQueue;
        private TMover currentMover;
        
        private void StateTransitionsToEnqueue(State<TStateName> startState)
        {
            var endStateNames = TransitionDictionary[startState.Name][currentMover];
            foreach (var endStateName in endStateNames)
            {                
                StatesQueue.Enqueue(States[endStateName]);
            }
        }

        private bool CanDoTransitions()
        {
            int countSteps = StatesQueue.Count;           

            if (currentMover == null)
            {
                Console.WriteLine("Отсутсвтует инициатор");
                return false;
            }

            Console.WriteLine($"Для инициатора {currentMover} из состояний:");
            for (int i = 0; i < countSteps; i++)
            {
                var state = StatesQueue.Dequeue();
                state.StateDo();
                StateTransitionsToEnqueue(state);
            }
            Console.WriteLine("\n->");

            bool goNext = false;
            if (InputMoversQueue.Count > 0)
            {
                currentMover = InputMoversQueue.Dequeue();
                goNext = true;
            }
            return goNext;
            
        }

        private void InitStatesQueue(TStateName startState)
        {
            States = new Dictionary<TStateName, State<TStateName>>();

            foreach (var st in TransitionDictionary)
            {
                States.Add(st.Key, new State<TStateName>(st.Key));
            }

            StatesQueue = new Queue<State<TStateName>>();
            StatesQueue.Enqueue(States[startState]);
            currentMover = InputMoversQueue.Dequeue();
            Console.WriteLine($"Создана очередь состояний с начальным состоянием {StatesQueue.Peek().Name}") ;
        }

        public Automat(TStateName startState,
                       Dictionary<TStateName, Dictionary<TMover, List<TStateName>>> transitionDictionary,
                       Queue<TMover> inputMoversQueue)
        {
            TransitionDictionary = transitionDictionary;
            InputMoversQueue = inputMoversQueue;

            InitStatesQueue(startState);
        }

        private List<State<TStateName>> GetResult()
        {
            Console.WriteLine("Результирующие состояния");
            var reultStates = StatesQueue.ToList<State<TStateName>>();
            foreach (var state in reultStates)
            {
                state.StateDo();
            }
            Console.WriteLine();
            return reultStates;
        }

        public List<State<TStateName>> Run()
        {
            while (CanDoTransitions()) { }
            return GetResult();
        }
    }
}
