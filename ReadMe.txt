ConsoleSniffer is a program that monitors (and alters) the console input/output action between two programs.
This will only work if you can get the calling program to call ConsoleSniffer in stead of the called program. ConsoleSniffer will then call the desired program itself and monitor all communications.

ConsoleSniffer WILL NOT monitor interactions between programs it has no correlation to.

If you've managed to read this far, you're probably interested in using ConsoleSniffer. In that case I'll provide you with some information to make your life a bit easier.

First and foremost are the command line arguments. As ConsoleSniffer is designed to pass through arguments to the called program all arguments meant for it should be enclosed within --snifferArgsBegin and --snifferArgsEnd. (eg. --snifferArgsBegin -application=D:/someapplication.exe -log=D:/logfile.txt --snifferArgsEnd)
Next I'll give you an overview of the possible arguments:
 -help             Display a notice with all possible arguments.
 -blockInput=BOOL  Prevent the calling application's input from being sent through to the called application. Bool is either true or false.
 -blockOutput=BOOL Prevent the called application's output from being sent through to the calling application. BOOL is either true or false.
 -config=FILE      Use a configuration file to configure ConsoleSniffer. When passing command line arguments that are also defined in the config file, the command line arguments will be overriden by default.
 -generateConfig   Generate an example configuration file that uses the passed command line arguments.
 -overwriteConfig  Turn off the default overriding of command line arguments by the config file.
 -log=FILE         Append all the command line IO to a file.
 -application=FILE The called application.

The real power of ConsoleSniffer comes to show when a configuration file is used. To get a good idea of the possibilities you might want to check out the ConsoleSnifferExampleConfig.xml file.
It allows you to set the location of the log file and the application file. It also allows blocking the input or output and manipulating the input and output.

To manipulate the IO the following possibilities are supported:
-if, elseIf and else conditional structures. These can evaluate if the IO contains a certain string and then execute actions.
-replace action. This can replace a string in the IO with a different string.
-blockInput, blockOutput actions. These only take effect if the respective global blockingInput/blockingOutput are set to false. They block the input/output for one line only.
-mockInput, mockOutput actions. Gives the possibility to add in a fake IO line.
-echoInput, echoOutput actions. Echo the IO back to the respective application. This can be used after the IO has been manipulated.