/* Program: ConsoleSniffer
 * File: ConsoleSnifferAction.cs
 *
 * Author: Jan De Groot <jan.degroot@live.be>
 *
 * Copyright 2016 under the Raindrop License Agreement V1.1.
 * If you did not receive a copy of the Raindrop License Agreement
 * with this Software, please contact the Author of the Software.
 */

namespace FriskoxD.ConsoleSniffer
{
    interface ConsoleSnifferAction
    {
        bool Execute(ref string value);
        bool IsConditional();
    }
}
