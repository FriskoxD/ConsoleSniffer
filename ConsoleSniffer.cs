/* Program: ConsoleSniffer
 * File: ConsoleSniffer.cs
 *
 * Author: Jan De Groot <jan.degroot@live.be>
 *
 * Copyright 2016 under the Raindrop License Agreement V1.1.
 * If you did not receive a copy of the Raindrop License Agreement
 * with this Software, please contact the Author of the Software.
 */

 //Created in Visual Studio 2015

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace FriskoxD.ConsoleSniffer
{
    using System.Diagnostics;
    class ConsoleSniffer
    {
        internal const int CTRL_C_EVENT = 0;
        [DllImport("kernel32.dll")]
        internal static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool AttachConsole(uint dwProcessId);
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern bool FreeConsole();
        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate HandlerRoutine, bool Add);
        // Delegate type to be used as the Handler Routine for SCCH
        delegate Boolean ConsoleCtrlDelegate(uint CtrlType);

        static string _logLocation = null;
        static string _applicationLocation = null;
        static string _configurationLocation = null;
        static bool _configurationFile = false;
        static bool _logging = false;
        static bool _blockingInput = false;
        static bool _blockingOutput = false;
        static bool _help = false;
        static bool _overWriteConfig = false;
        static bool _generateConfig = false;

        static string _commandArguments = "";

        static Process _application = new Process();

        static bool _singleInputBlock = false;
        static bool _singleOutputBlock = false;
        static bool _mockingOutput = false;
        static bool _mockingInput = false;
        static bool _echoInput = false;
        static bool _echoOutput = false;
        static string _mockInput = "";
        static string _mockOutput = "";

        static ConsoleSnifferManipulation _inputConditionals = new ConsoleSnifferManipulation();
        static ConsoleSnifferManipulation _outputConditionals = new ConsoleSnifferManipulation();

        static void Main(string[] args)
        {
            ParseArguments(); //Check the arguments that have been passed to the program.

            if(_generateConfig) //The caller wants to generate a configuration file.
            {
                GenerateConfig();
                _configurationFile = true;
            }

            bool configError = false;

            if (_configurationFile)
            {
                if (CheckConfigurationFile())
                {
                    ConfigureFromFile(); //User wants to load the configuration from a file.
                    if (_overWriteConfig)
                    {
                        ParseArguments(); //Command line arguments have precedence, so we replace them again. (Note: this will only overwrite arguments that have been explicitly set in the command line.)
                    }
                }
                else
                {
                    configError = true;
                }
            }

            CheckLog(); //Initialize the log file.
            WriteLogLine(ConsoleSnifferText.logNewSession);

            //Print the configuration.
            WriteLogLine(ConsoleSnifferText.logConfiguration);
            WriteLogLine(ConsoleSnifferText.logBlockingInput + (_blockingInput ? ConsoleSnifferText.valueTRUE : ConsoleSnifferText.valueFALSE));
            WriteLogLine(ConsoleSnifferText.logBlockingOutput + (_blockingOutput ? ConsoleSnifferText.valueTRUE : ConsoleSnifferText.valueFALSE));
            WriteLogLine(ConsoleSnifferText.logLogging + (_logging ? ConsoleSnifferText.valueTRUE : ConsoleSnifferText.valueFALSE));
            WriteLogLine(ConsoleSnifferText.logHelp + (_help ? ConsoleSnifferText.valueTRUE : ConsoleSnifferText.valueFALSE));
            WriteLogLine(ConsoleSnifferText.logGenerateConfig + (_generateConfig ? ConsoleSnifferText.valueTRUE : ConsoleSnifferText.valueFALSE));
            WriteLogLine(ConsoleSnifferText.logOverwriteConfig + (_overWriteConfig ? ConsoleSnifferText.valueTRUE : ConsoleSnifferText.valueFALSE));
            WriteLogLine(ConsoleSnifferText.logUseConfig + (_configurationFile ? ConsoleSnifferText.valueTRUE : ConsoleSnifferText.valueFALSE));
            WriteLogLine(ConsoleSnifferText.logApplicationLocation + (_applicationLocation!=null ? _applicationLocation : ConsoleSnifferText.valueNULL));
            WriteLogLine(ConsoleSnifferText.logConfigurationLoaction + (_configurationLocation != null ? _configurationLocation : ConsoleSnifferText.valueNULL));
            WriteLogLine(ConsoleSnifferText.logLogLocation + (_logLocation != null ? _logLocation : ConsoleSnifferText.valueNULL));

            if (configError)
            {
                WriteLogLineTimed(ConsoleSnifferText.errorConfigFileDoesNotExist);
                Console.WriteLine(ConsoleSnifferText.errorConfigFileDoesNotExist);
                return;
            }
            
            if(!_help && !CheckApplication())
            {
                WriteLogLineTimed(ConsoleSnifferText.errorApplicationInvalid);
                Console.WriteLine(ConsoleSnifferText.errorApplicationInvalid);
                _help = true;
            }
            
            if(_help)
            {
                Console.Write(ConsoleSnifferText.helpOutput);
                return; //Quit the program.
            }

            _application.StartInfo.FileName = _applicationLocation;
            _application.StartInfo.Arguments = _commandArguments;
            _application.StartInfo.RedirectStandardInput = true;
            _application.StartInfo.RedirectStandardOutput = true;
            _application.StartInfo.UseShellExecute = false;
            _application.EnableRaisingEvents = true;
            _application.OutputDataReceived += new DataReceivedEventHandler(delegate (object sender, DataReceivedEventArgs e) {
                string outputData = e.Data;
                WriteLogLineTimed(ConsoleSnifferText.logOriginalOutput + outputData);
                _outputConditionals.Execute(ref outputData);
                if(!_blockingOutput&&!_singleOutputBlock)
                {
                    if(_mockingOutput)
                    {
                        WriteLogLineTimed(ConsoleSnifferText.logMockOutput + _mockOutput);
                        Console.WriteLine(_mockOutput);
                    }else
                    {
                        WriteLogLineTimed(ConsoleSnifferText.logActualOutput + outputData);
                        Console.WriteLine(outputData);
                    }
                }

                if(_echoOutput)
                {
                    WriteLogLineTimed(ConsoleSnifferText.logEchoOutput + outputData);
                    _application.StandardInput.WriteLine(outputData);
                }

                _echoOutput = false;
                _singleOutputBlock = false;
                _mockingOutput = false;
            });

            WriteLogLineTimed(_application.StartInfo.FileName);
            WriteLogLineTimed(_application.StartInfo.Arguments);

            _application.Start();
            _application.BeginOutputReadLine();

            string input = "";

            Console.CancelKeyPress += delegate { //Because ctrl+c isn't caught in readline.
                WriteLogLineTimed(ConsoleSnifferText.logOriginalInput + "ctrl+c");

                FreeConsole();

                if (AttachConsole((uint)_application.Id))
                {
                    SetConsoleCtrlHandler(null, true);
                    try
                    {
                        GenerateConsoleCtrlEvent(CTRL_C_EVENT, 0);
                    }
                    finally
                    {
                        FreeConsole();
                        SetConsoleCtrlHandler(null, false);
                    }
                    AttachConsole((uint)Process.GetCurrentProcess().Id);
                }

            };

            while (!_application.HasExited)
            {
                input = Console.ReadLine();
                WriteLogLineTimed(ConsoleSnifferText.logOriginalInput + input);
                _inputConditionals.Execute(ref input);

                if (!_blockingInput && !_singleInputBlock)
                {
                    if (_mockingInput)
                    {
                        WriteLogLineTimed(ConsoleSnifferText.logMockInput + _mockInput);
                        _application.StandardInput.WriteLine(_mockInput);
                    }
                    else
                    {
                        WriteLogLineTimed(ConsoleSnifferText.logActualInput + input);
                        _application.StandardInput.WriteLine(input);
                    }
                }

                if(_echoInput)
                {
                    WriteLogLineTimed(ConsoleSnifferText.logEchoInput + input);
                    Console.WriteLine(input);
                }

                _echoInput = false;
                _singleInputBlock = false;
                _mockingInput = false;
            };

            WriteLogLineTimed(ConsoleSnifferText.logApplicationExited);

        }
        
        static bool CheckConfigurationFile()
        {
            return System.IO.File.Exists(_configurationLocation);
        }

        static void ConfigureFromFile()
        {
            string xml = System.IO.File.ReadAllText(_configurationLocation);
            bool blockingInput = false;
            bool blockingOutput = false;
            string logFileLocation = "";
            string applicationLocation = "";

            if(ConsoleSnifferXmlParser.GetBlockingInput(ref blockingInput, xml)) { _blockingInput = blockingInput; }
            if(ConsoleSnifferXmlParser.GetBlockingOutput(ref blockingOutput, xml)) { _blockingOutput = blockingOutput; }
            if(ConsoleSnifferXmlParser.GetApplicationLocation(ref applicationLocation, xml)) { _applicationLocation = applicationLocation;}
            if(ConsoleSnifferXmlParser.GetLogFileLocation(ref logFileLocation, xml)) { _logLocation = logFileLocation; _logging = true; }

            ConsoleSnifferXmlParser.GetInputManipulation(ref _inputConditionals, xml);
            ConsoleSnifferXmlParser.GetOutputManipulation(ref _outputConditionals, xml);

        }

        static void GenerateConfig()
        {
            ConsoleSnifferXmlParser.CreateFile(_applicationLocation, _blockingInput, _blockingOutput, _logLocation, _configurationLocation);
        }

        static void ParseArguments()
        {
            bool valid = true;
            List<string> args = new List<string>();

            if (Environment.CommandLine.Contains(ConsoleSnifferText.argumentsBegin) && Environment.CommandLine.Contains(ConsoleSnifferText.argumentsEnd)) //Put all the arguments in a list.
            {
                string snifferArgs = Environment.CommandLine.Substring(Environment.CommandLine.IndexOf(ConsoleSnifferText.argumentsBegin) + ConsoleSnifferText.argumentsBegin.Count(), Environment.CommandLine.IndexOf(ConsoleSnifferText.argumentsEnd) -(Environment.CommandLine.IndexOf(ConsoleSnifferText.argumentsBegin) + ConsoleSnifferText.argumentsBegin.Count())); //Get the arguments meant for ConsoleSniffer.
                snifferArgs = snifferArgs.Trim(' ');
                while(snifferArgs.Contains('-'))
                {
                    if (snifferArgs.Contains(" -"))
                    {
                        args.Add(snifferArgs.Substring(0, snifferArgs.IndexOf(" -")));
                        snifferArgs = snifferArgs.Substring(snifferArgs.IndexOf(" -") + 1);
                    }
                    else //Last argument.
                    {
                        args.Add(snifferArgs);
                        snifferArgs = "";
                    }
                }
            }

            if(args.Count()==0)
            {
                valid = false;
            }

            for(int i=0; i<args.Count(); ++i)
            {
                if(args[i].Contains(ConsoleSnifferText.argumentsHelp))
                {
                    _help = true;
                }else if(args[i].Contains(ConsoleSnifferText.argumentsBlockInput))
                {
                    if(args[i].Contains("="))
                    {
                        switch(args[i].Substring(args[i].IndexOf("=")+1).ToUpper())
                        {
                            case ConsoleSnifferText.valueTRUE:
                                _blockingInput = true;
                                break;
                            case ConsoleSnifferText.valueFALSE:
                                _blockingInput = false;
                                break;
                            default:
                                valid = false;
                                break;
                        }
                    }else
                    {
                        valid = false;
                    }
                }
                else if (args[i].Contains(ConsoleSnifferText.argumentsBlockOutput))
                {
                    if (args[i].Contains("="))
                    {
                        switch (args[i].Substring(args[i].IndexOf("=") + 1).ToUpper())
                        {
                            case ConsoleSnifferText.valueTRUE:
                                _blockingOutput = true;
                                break;
                            case ConsoleSnifferText.valueFALSE:
                                _blockingOutput = false;
                                break;
                            default:
                                valid = false;
                                break;
                        }
                    }
                    else
                    {
                        valid = false;
                    }
                }
                else if (args[i].Contains(ConsoleSnifferText.argumentsConfig))
                {
                    if (args[i].Contains("="))
                    {
                        _configurationLocation = args[i].Substring(args[i].IndexOf("=") + 1);
                        _configurationFile = true;
                    }
                    else
                    {
                        valid = false;
                    }
                }
                else if (args[i].Contains(ConsoleSnifferText.argumentsGenerateConfig))
                {
                    _generateConfig = true;
                }
                else if (args[i].Contains(ConsoleSnifferText.argumentsOverwriteConfig))
                {
                    _overWriteConfig = true;
                }
                else if (args[i].Contains(ConsoleSnifferText.argumentsLog))
                {
                    if (args[i].Contains("="))
                    {
                        _logLocation = args[i].Substring(args[i].IndexOf("=") + 1);
                        _logging = true;
                    }
                    else
                    {
                        valid = false;
                    }
                }
                else if (args[i].Contains(ConsoleSnifferText.argumentsApplication))
                {
                    if (args[i].Contains("="))
                    {
                        _applicationLocation = args[i].Substring(args[i].IndexOf("=") + 1);
                    }
                    else
                    {
                        valid = false;
                    }
                }else
                {
                    valid = false;
                }

                if(valid==false)
                {
                    break;
                }
            }

            if(!valid)
            {
                Console.WriteLine(ConsoleSnifferText.errorArgumentsInvalid);
                _help = true; //Make sure the user knows which arguments are available.
            }else
            {
                string substr = Environment.CommandLine.Substring(Environment.CommandLine.IndexOf(ConsoleSnifferText.argumentsBegin), Environment.CommandLine.IndexOf(ConsoleSnifferText.argumentsEnd) + (ConsoleSnifferText.argumentsEnd).Count() - Environment.CommandLine.IndexOf(ConsoleSnifferText.argumentsBegin));
                string first = Environment.GetCommandLineArgs()[0]; //This contains the path to the current program.
                _commandArguments = Environment.CommandLine.Replace(substr, ""); //Delete the arguments meant for ConsoleSniffer.
                if(_commandArguments.Contains("\"" + first + "\"")) //The argument list often omits these, but the CommandLine string in the environment doesn't.
                {
                    first = "\"" + first + "\"";
                }
                _commandArguments = _commandArguments.Replace(first, "");  
            }
        }

        static void CheckLog() //Checks if a log file exists, and creates one if necessary.
        {
            if (_logging)
            {
                if (!System.IO.File.Exists(_logLocation))
                {
                    if (_logLocation == "") //No valid log location was specified.
                    {
                        _logging = false;
                        _help = true;
                    }
                    else
                    {
                        System.IO.FileStream s = System.IO.File.Create(_logLocation);
                        s.Close();
                    }
                }
            }
        }

        static void WriteLogLineTimed(string line)
        {
            line = DateTime.Now.ToString(ConsoleSnifferText.logDateFormat) + ": " + line;
            WriteLogLine(line);
        }

        static void WriteLogLine(String line)
        {
            if (_logging)
            {
                bool success = false;
                while (!success)
                {
                    try
                    {
                        using (System.IO.StreamWriter sw = System.IO.File.AppendText(_logLocation))
                        {
                            sw.WriteLine(line);
                            sw.Close();
                        }
                        success = true;
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

        static bool CheckApplication() //Check if the application actually exists.
        {
            if(_applicationLocation != null)
            {
                return System.IO.File.Exists(_applicationLocation);
            }

            return false;
        }

        public static void SingleOutputBlock(bool value)
        {
            _singleOutputBlock = value;
        }

        public static void SingleInputBlock(bool value)
        {
            _singleInputBlock = value;
        }

        public static void MockInput(string value)
        {
            _mockingInput = true;
            _mockInput = value;
        }

        public static void MockOutput(string value)
        {
            _mockingOutput = true;
            _mockOutput = value;
        }

        public static void EchoInput(bool value)
        {
            _echoInput = value;
        }

        public static void EchoOutput(bool value)
        {
            _echoOutput = value;
        }
    }
}
