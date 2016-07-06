/* Program: ConsoleSniffer
 * File: ConsoleSnifferText.cs
 *
 * Author: Jan De Groot <jan.degroot@live.be>
 *
 * Copyright 2016 under the Raindrop License Agreement V1.1.
 * If you did not receive a copy of the Raindrop License Agreement
 * with this Software, please contact the Author of the Software.
 */

namespace FriskoxD.ConsoleSniffer
{
    class ConsoleSnifferText
    {
        public const string argumentsHelp              = "-help";
        public const string argumentsBlockInput        = "-blockInput";
        public const string argumentsBlockOutput       = "-blockOutput";
        public const string argumentsConfig            = "-config";
        public const string argumentsGenerateConfig    = "-generateConfig";
        public const string argumentsOverwriteConfig   = "-overwriteConfig";
        public const string argumentsLog               = "-log";
        public const string argumentsApplication       = "-application";
        public const string argumentsBegin             = "--snifferArgsBegin";
        public const string argumentsEnd               = "--snifferArgsEnd";

        public const string valueTRUE  = "TRUE"; //This must be uppercase.
        public const string valueFALSE = "FALSE"; //This must be uppercase.
        public const string valueNULL  = "NULL"; //This must be uppercase.

        public const string errorConfigFileDoesNotExist    = "Configuration file does not exist.";
        public const string errorApplicationInvalid        = "Invalid or no application specified.";
        public const string errorArgumentsInvalid          = "Invalid or no arguments sent to ConsoleSniffer.";
        public const string helpOutput                     = "Congratulations, you have found the help function for ConsoleSniffer!\n" +
                                                                "Here are the things you can do:\n\n" +
                                                                "To pass arguments to ConsoleSniffer you must enclose them between " + argumentsBegin + " and " + argumentsEnd + ". (eg. " + argumentsBegin + " " + argumentsBlockInput + "=" + valueTRUE + " " + argumentsBlockOutput + "=" + valueFALSE + " " + argumentsEnd + ")\n\n" +

                                                                "Arguments:\n\n" +

                                                                " " + argumentsBegin + "                Display this notice.\n" +
                                                                " " + argumentsBlockInput + "=BOOL     Prevent the caller's input from going through to the\n" +
                                                                "                      called application. BOOL is either false or true.\n" +
                                                                " " + argumentsBlockOutput + "t=BOOL    Prevent the called application's output from being sent\n" +
                                                                "                      through to the caller. BOOL is either false or true.\n" +
                                                                " " + argumentsConfig + "=FILE         Use a configuration file to configure ConsoleSniffer.\n" +
                                                                "                      When passing command line arguments that are also defined\n" +
                                                                "                      in the config file, the config file's settings will take\n" +
                                                                "                      precedence.\n" +
                                                                "                      If this behaviour is not desired, use -overwriteConfig.\n" +
                                                                " " + argumentsGenerateConfig + "=FILE Generate a config file that uses the passed command line\n" +
                                                                "                      arguments.\n" +
                                                                " " + argumentsOverwriteConfig + "     When passing both arguments and a config file to\n" +
                                                                "                      ConsoleSniffer the command line arguments will take\n" +
                                                                "                      precendence.This does not actually alter the config file.\n" +
                                                                " " + argumentsLog + "=FILE            Appends all command line IO to the FILE.\n" +
                                                                " " + argumentsApplication + "=FILE    The application that is to be called.\n";

        public const string logNewSession              = "--------New Session---------";
        public const string logConfiguration           = "Configuration:";
        public const string logDateFormat              = "yyyy/MM/dd HH:mm:ss";
        public const string logOriginalInput           = "ORIGINAL_INPUT: ";
        public const string logActualInput             = "ACTUAL_INPUT: ";
        public const string logMockInput               = "MOCK_INPUT: ";
        public const string logEchoInput               = "ECHO_INPUT: ";
        public const string logOriginalOutput          = "ORIGINAL_OUTPUT: ";
        public const string logActualOutput            = "ACTUAL_OUTPUT: ";
        public const string logMockOutput              = "MOCK_OUTPUT: ";
        public const string logEchoOutput              = "ECHO_OUTPUT: ";
        public const string logBlockingInput           = "-Blocking input: ";
        public const string logBlockingOutput          = "-Blocking output: ";
        public const string logHelp                    = "-Help: ";
        public const string logLogging                 = "-Logging: ";
        public const string logGenerateConfig          = "-Generate config: ";
        public const string logOverwriteConfig         = "-Overwrite config: ";
        public const string logUseConfig               = "-Use config file: ";
        public const string logApplicationLocation     = "-Application location: ";
        public const string logConfigurationLoaction   = "-Configuration location: ";
        public const string logLogLocation             = "-Log location: ";
        public const string logApplicationExited       = "Application has exited.";

        public const string xmlConfigurationTag         = "configuration";
        public const string xmlApplicationFileTag       = "applicationFile";
        public const string xmlBlockingInputTag         = "blockingInput";
        public const string xmlBlockingOutputTag        = "blockingOutput";
        public const string xmlLogFileTag               = "logFile";
        public const string xmlInputManipulationTag     = "inputManipulation";
        public const string xmlOutputManipulationTag    = "outputManipulation";
        public const string xmlIfTag                    = "if";
        public const string xmlElseIfTag                = "elseif";
        public const string xmlElseTag                  = "else";
        public const string xmlReplaceTag               = "replace";
        public const string xmlBlockInputTag            = "blockInput";
        public const string xmlBlockOutputTag           = "blockOutput";
        public const string xmlMockInputTag             = "mockInput";
        public const string xmlMockOutputTag            = "mockOutput";
        public const string xmlEchoInputTag             = "echoInput";
        public const string xmlEchoOutputTag            = "echoOutput";
        public const string xmlContainsAttribute        = "contains";
        public const string xmlActiveAttribute          = "active";
        public const string xmlLocationAttribute        = "location";
        public const string xmlOriginalAttribute        = "original";
        public const string xmlNewAttribute             = "new";
        public const string xmlValueAttribute           = "value";

    }
}
