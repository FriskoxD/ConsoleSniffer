/* Program: ConsoleSniffer
 * File: ConsoleSnifferManipulation.cs
 *
 * Author: Jan De Groot <jan.degroot@live.be>
 *
 * Copyright 2016 under the Raindrop License Agreement V1.1.
 * If you did not receive a copy of the Raindrop License Agreement
 * with this Software, please contact the Author of the Software.
 */

using System.Collections.Generic;

namespace FriskoxD.ConsoleSniffer
{
    class ConsoleSnifferManipulation
    {
        List<List<ConsoleSnifferAction>> _manipulations = new List<List<ConsoleSnifferAction>>();

        public void RegisterManipulation(List<ConsoleSnifferAction> manipulation)
        {
            _manipulations.Add(manipulation);
        }

        public void RegisterManipulation(ConsoleSnifferAction manipulation)
        {
            if(_manipulations.Count==0)
            {
                _manipulations.Add(new List<ConsoleSnifferAction>());
            }

            _manipulations[_manipulations.Count - 1].Add(manipulation);
        }

        public bool Execute(ref string value)
        {
            foreach (List<ConsoleSnifferAction> actions in _manipulations)
            {
                bool conditionalMet = false;
                foreach (ConsoleSnifferAction action in actions)
                {
                    if (action.IsConditional() && !conditionalMet)
                    {
                        conditionalMet = action.Execute(ref value);
                    }
                    else if (!action.IsConditional())
                    {
                        action.Execute(ref value);
                    }
                }
            }

            return true;
        }
        
    }
}
