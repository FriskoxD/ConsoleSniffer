/* Program: ConsoleSniffer
 * File: ConsoleSnifferConditionalAction.cs
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
    class ConsoleSnifferConditionalAction : ConsoleSnifferAction
    {
        
        string _condition = "";
        List<List<ConsoleSnifferAction>> _actions = new List<List<ConsoleSnifferAction>>();

        public ConsoleSnifferConditionalAction() { }

        public ConsoleSnifferConditionalAction(string condition)
        {
            _condition = condition;
        }

        public void RegisterAction(List<ConsoleSnifferAction> action)
        {
            _actions.Add(action);
        }

        public void RegisterAction(ConsoleSnifferAction action)
        {
            if (_actions.Count == 0)
            {
                _actions.Add(new List<ConsoleSnifferAction>());
            }

            _actions[_actions.Count - 1].Add(action);
        }

        bool ConsoleSnifferAction.Execute(ref string value)
        {

            if(_condition==null || value.Contains(_condition)) //If the condition is null we've reached an else statement.
            {
                foreach (List<ConsoleSnifferAction> actions in _actions)
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

            return false;
        }

        bool ConsoleSnifferAction.IsConditional()
        {
            return true;
        }

    }
}
