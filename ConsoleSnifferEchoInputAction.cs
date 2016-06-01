/* Program: ConsoleSniffer
 * File: ConsoleSnifferEchoInputAction.cs
 *
 * Author: Jan De Groot <jan.degroot@live.be>
 *
 * Copyright 2016 under the Raindrop License Agreement V1.1.
 * If you did not receive a copy of the Raindrop License Agreement
 * with this Software, please contact the Author of the Software.
 */

namespace FriskoxD.ConsoleSniffer
{
    class ConsoleSnifferEchoInputAction : ConsoleSnifferAction
    {
        bool blocking = false;

        public ConsoleSnifferEchoInputAction(bool value)
        {
            blocking = value;
        }

        bool ConsoleSnifferAction.IsConditional()
        {
            return false;
        }

        bool ConsoleSnifferAction.Execute(ref string value)
        {
            ConsoleSniffer.EchoInput(blocking);
            return true;
        }
    }
}
