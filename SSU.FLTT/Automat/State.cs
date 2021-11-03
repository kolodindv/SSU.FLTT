using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSU.FLTT.Automat
{
    class State<TStateName/*, TMover*/>
    {
        public readonly TStateName Name;
        //private List<Transition<TStateName, TMover>> Transitions;

        public State(TStateName name)
        {
            Name = name;
        }

        public void StateDo()
        {
            Console.Write(Name.ToString() + ' ');
        }
    }
}
