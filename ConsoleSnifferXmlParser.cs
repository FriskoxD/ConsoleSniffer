/* Program: ConsoleSniffer
 * File: ConsoleSnifferXmlParser.cs
 *
 * Author: Jan De Groot <jan.degroot@live.be>
 *
 * Copyright 2016 under the Raindrop License Agreement V1.1.
 * If you did not receive a copy of the Raindrop License Agreement
 * with this Software, please contact the Author of the Software.
 */

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;

namespace FriskoxD.ConsoleSniffer
{
    class ConsoleSnifferXmlParser
    {
        public static void GetInputManipulation(ref ConsoleSnifferManipulation manipulation, string xml)
        {
            GetManipulationStructure(ref manipulation, ConsoleSnifferText.xmlInputManipulationTag, xml);
        }

        public static void GetOutputManipulation(ref ConsoleSnifferManipulation manipulation, string xml)
        {
            GetManipulationStructure(ref manipulation, ConsoleSnifferText.xmlOutputManipulationTag, xml);
        }

        private static void GetManipulationStructure(ref ConsoleSnifferManipulation manipulation, string tagName, string xml)
        {

            XmlReader reader = XmlReader.Create(new StringReader(xml));

            while (reader.ReadToFollowing(ConsoleSnifferText.xmlConfigurationTag))
            {
                if (reader.MoveToAttribute(ConsoleSnifferText.xmlActiveAttribute) && reader.GetAttribute(ConsoleSnifferText.xmlActiveAttribute).ToUpper().Equals(ConsoleSnifferText.valueTRUE))
                {
                    reader.MoveToElement();
                    string subXml = reader.ReadInnerXml();
                    XmlReaderSettings subReaderSettings = new XmlReaderSettings();
                    subReaderSettings.ConformanceLevel = ConformanceLevel.Fragment; //Let the reader ignore that there are now multiple root nodes.
                    XmlReader subReader = XmlReader.Create(new StringReader(subXml), subReaderSettings);

                    if (subReader.ReadToFollowing(tagName))
                    {
                        subReader.MoveToElement();
                        string subSubXml = subReader.ReadInnerXml();
                        XmlReader contentReader = XmlReader.Create(new StringReader(subSubXml), subReaderSettings);
                        while(contentReader.Read())
                        {
                            if(contentReader.LocalName.ToUpper() == ConsoleSnifferText.xmlIfTag.ToUpper())
                            {
                                List<ConsoleSnifferConditionalAction> conditional = new List<ConsoleSnifferConditionalAction>();
                                contentReader.MoveToElement();
                                ReadConditionalStructure(ref conditional, contentReader.ReadOuterXml());
                                manipulation.RegisterManipulation(conditional.Cast<ConsoleSnifferAction>().ToList());
                            }
                            else if (contentReader.LocalName.ToUpper() == ConsoleSnifferText.xmlReplaceTag.ToUpper())
                            {
                                ConsoleSnifferReplaceOperation a = new ConsoleSnifferReplaceOperation(contentReader.GetAttribute(ConsoleSnifferText.xmlOriginalAttribute), contentReader.GetAttribute(ConsoleSnifferText.xmlNewAttribute));
                                manipulation.RegisterManipulation(a);
                            }
                            else if (contentReader.LocalName.ToUpper() == ConsoleSnifferText.xmlBlockInputTag.ToUpper())
                            {
                                ConsoleSnifferBlockInputAction a = new ConsoleSnifferBlockInputAction(contentReader.GetAttribute(ConsoleSnifferText.xmlValueAttribute).ToUpper() == ConsoleSnifferText.valueTRUE);
                                manipulation.RegisterManipulation(a);
                            }
                            else if (contentReader.LocalName.ToUpper() == ConsoleSnifferText.xmlBlockOutputTag.ToUpper())
                            {
                                ConsoleSnifferBlockOutputAction a = new ConsoleSnifferBlockOutputAction(contentReader.GetAttribute(ConsoleSnifferText.xmlValueAttribute).ToUpper() == ConsoleSnifferText.valueTRUE);
                                manipulation.RegisterManipulation(a);
                            }
                            else if (contentReader.LocalName.ToUpper() == ConsoleSnifferText.xmlMockInputTag.ToUpper())
                            {
                                ConsoleSnifferMockInputAction a = new ConsoleSnifferMockInputAction(contentReader.GetAttribute(ConsoleSnifferText.xmlValueAttribute));
                                manipulation.RegisterManipulation(a);
                            }
                            else if (contentReader.LocalName.ToUpper() == ConsoleSnifferText.xmlMockOutputTag.ToUpper())
                            {
                                ConsoleSnifferMockOutputAction a = new ConsoleSnifferMockOutputAction(contentReader.GetAttribute(ConsoleSnifferText.xmlValueAttribute));
                                manipulation.RegisterManipulation(a);
                            }
                            else if (contentReader.LocalName.ToUpper() == ConsoleSnifferText.xmlEchoInputTag.ToUpper())
                            {
                                ConsoleSnifferEchoInputAction a = new ConsoleSnifferEchoInputAction(contentReader.GetAttribute(ConsoleSnifferText.xmlValueAttribute).ToUpper() == ConsoleSnifferText.valueTRUE);
                                manipulation.RegisterManipulation(a);
                            }
                            else if (contentReader.LocalName.ToUpper() == ConsoleSnifferText.xmlEchoOutputTag.ToUpper())
                            {
                                ConsoleSnifferEchoOutputAction a = new ConsoleSnifferEchoOutputAction(contentReader.GetAttribute(ConsoleSnifferText.xmlValueAttribute).ToUpper() == ConsoleSnifferText.valueTRUE);
                                manipulation.RegisterManipulation(a);
                            }
                        }
                    }
                }
            }
            
        }

        private static void ReadConditionalStructure(ref List<ConsoleSnifferConditionalAction> structure, string xml)
        {

            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.ConformanceLevel = ConformanceLevel.Fragment; //Let the reader ignore that there are now multiple root nodes.

            XmlReader contentReader = XmlReader.Create(new StringReader(xml), readerSettings);
            if(contentReader.ReadToFollowing("if") && contentReader.MoveToAttribute(ConsoleSnifferText.xmlContainsAttribute))
            {
                ConsoleSnifferConditionalAction mainsStructure = new ConsoleSnifferConditionalAction(contentReader.GetAttribute(ConsoleSnifferText.xmlContainsAttribute));

                contentReader.MoveToElement();
                string innerXml = contentReader.ReadInnerXml();

                XmlReader subContentReader = XmlReader.Create(new StringReader(innerXml), readerSettings);
                ParseConditionalSubStructure(ref mainsStructure, innerXml);

                structure.Add(mainsStructure);

                while(subContentReader.Read())

                {
                    if(subContentReader.LocalName.ToUpper() == ConsoleSnifferText.xmlIfTag.ToUpper())
                    {
                        subContentReader.Skip(); //This should already have been parsed at this point.
                    }
                    if(subContentReader.LocalName.ToUpper() == ConsoleSnifferText.xmlElseIfTag.ToUpper())
                    {
                        ConsoleSnifferConditionalAction elseStructure = new ConsoleSnifferConditionalAction(subContentReader.GetAttribute(ConsoleSnifferText.xmlContainsAttribute));
                        subContentReader.MoveToElement();
                        ParseConditionalSubStructure(ref elseStructure, subContentReader.ReadInnerXml());
                        structure.Add(elseStructure);
                    }else if(subContentReader.LocalName.ToUpper() == ConsoleSnifferText.xmlElseTag.ToUpper())
                    {
                        ConsoleSnifferConditionalAction elseStructure = new ConsoleSnifferConditionalAction(subContentReader.GetAttribute(""));
                        subContentReader.MoveToElement();
                        ParseConditionalSubStructure(ref elseStructure, subContentReader.ReadInnerXml());
                        structure.Add(elseStructure);
                    }
                }
            }
        }

        private static void ParseConditionalSubStructure(ref ConsoleSnifferConditionalAction action, string xml)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            XmlReader contentReader = XmlReader.Create(new StringReader(xml), settings);

            while (contentReader.Read())
            {
                if (contentReader.LocalName.ToUpper() == ConsoleSnifferText.xmlIfTag.ToUpper())
                {
                    List<ConsoleSnifferConditionalAction> subStructure = new List<ConsoleSnifferConditionalAction>();
                    contentReader.MoveToElement();
                    ReadConditionalStructure(ref subStructure, contentReader.ReadOuterXml());
                    action.RegisterAction(subStructure.Cast<ConsoleSnifferAction>().ToList());
                }
                else if (contentReader.LocalName.ToUpper() == ConsoleSnifferText.xmlReplaceTag.ToUpper())
                {
                    ConsoleSnifferReplaceOperation a = new ConsoleSnifferReplaceOperation(contentReader.GetAttribute(ConsoleSnifferText.xmlOriginalAttribute), contentReader.GetAttribute(ConsoleSnifferText.xmlNewAttribute));
                    action.RegisterAction(a);
                }
                else if (contentReader.LocalName.ToUpper() == ConsoleSnifferText.xmlBlockInputTag.ToUpper())
                {
                    ConsoleSnifferBlockInputAction a = new ConsoleSnifferBlockInputAction(contentReader.GetAttribute(ConsoleSnifferText.xmlValueAttribute).ToUpper() == ConsoleSnifferText.valueTRUE);
                    action.RegisterAction(a);
                }
                else if (contentReader.LocalName.ToUpper() == ConsoleSnifferText.xmlBlockOutputTag.ToUpper())
                {
                    ConsoleSnifferBlockOutputAction a = new ConsoleSnifferBlockOutputAction(contentReader.GetAttribute(ConsoleSnifferText.xmlValueAttribute).ToUpper() == ConsoleSnifferText.valueTRUE);
                    action.RegisterAction(a);
                }
                else if (contentReader.LocalName.ToUpper() == ConsoleSnifferText.xmlMockInputTag.ToUpper())
                {
                    ConsoleSnifferMockInputAction a = new ConsoleSnifferMockInputAction(contentReader.GetAttribute(ConsoleSnifferText.xmlValueAttribute));
                    action.RegisterAction(a);
                }
                else if (contentReader.LocalName.ToUpper() == ConsoleSnifferText.xmlMockOutputTag.ToUpper())
                {
                    ConsoleSnifferMockOutputAction a = new ConsoleSnifferMockOutputAction(contentReader.GetAttribute(ConsoleSnifferText.xmlValueAttribute));
                    action.RegisterAction(a);
                }
                else if (contentReader.LocalName.ToUpper() == ConsoleSnifferText.xmlEchoInputTag.ToUpper())
                {
                    ConsoleSnifferEchoInputAction a = new ConsoleSnifferEchoInputAction(contentReader.GetAttribute(ConsoleSnifferText.xmlValueAttribute).ToUpper() == ConsoleSnifferText.valueTRUE);
                    action.RegisterAction(a);
                }
                else if (contentReader.LocalName.ToUpper() == ConsoleSnifferText.xmlEchoOutputTag.ToUpper())
                {
                    ConsoleSnifferEchoOutputAction a = new ConsoleSnifferEchoOutputAction(contentReader.GetAttribute(ConsoleSnifferText.xmlValueAttribute).ToUpper() == ConsoleSnifferText.valueTRUE);
                    action.RegisterAction(a);
                }
                else if (contentReader.LocalName.ToUpper() == ConsoleSnifferText.xmlElseIfTag.ToUpper() || contentReader.LocalName.ToUpper() == ConsoleSnifferText.xmlElseTag.ToUpper())
                {
                    contentReader.ReadOuterXml();
                }
            }
        }

        public static bool GetApplicationLocation(ref string applicationLocation, string xml)
        {
            return GetAttributeFrom(ref applicationLocation, ConsoleSnifferText.xmlApplicationFileTag, ConsoleSnifferText.xmlLocationAttribute, xml);
        }

        public static bool GetBlockingInput(ref bool blocking, string xml)
        {
            string block = "";
            if (GetAttributeFrom(ref block, ConsoleSnifferText.xmlBlockingInputTag, ConsoleSnifferText.xmlValueAttribute, xml))
            {
                if (block.ToUpper().Equals(ConsoleSnifferText.valueTRUE))
                {
                    blocking = true;
                }
                else
                {
                    blocking = false;
                }

                return true;
            }

            return false;
        }

        public static bool GetBlockingOutput(ref bool blocking, string xml)
        {
            string block = "";
            if (GetAttributeFrom(ref block, ConsoleSnifferText.xmlBlockingOutputTag, ConsoleSnifferText.xmlValueAttribute, xml))
            {
                if (block.ToUpper().Equals(ConsoleSnifferText.valueTRUE))
                {
                    blocking = true;
                }
                else
                {
                    blocking = false;
                }

                return true;
            }

            return false;
        }

        public static bool GetLogFileLocation(ref string logLocation, string xml)
        {
            return GetAttributeFrom(ref logLocation, ConsoleSnifferText.xmlLogFileTag, ConsoleSnifferText.xmlLocationAttribute, xml); ;
        }

        private static bool GetAttributeFrom(ref string attribute, string tag, string attributeName, string xml)
        {
            XmlReader reader = XmlReader.Create(new StringReader(xml));
            bool success = false;

            while(reader.ReadToFollowing(ConsoleSnifferText.xmlConfigurationTag))
            {
                if(reader.MoveToAttribute(ConsoleSnifferText.xmlActiveAttribute) && reader.GetAttribute(ConsoleSnifferText.xmlActiveAttribute).ToUpper().Equals(ConsoleSnifferText.valueTRUE))
                {
                    reader.MoveToElement();
                    string subXml = reader.ReadInnerXml();
                    XmlReaderSettings subReaderSettings = new XmlReaderSettings();
                    subReaderSettings.ConformanceLevel = ConformanceLevel.Fragment; //Let the reader ignore that there are now multiple root nodes.
                    XmlReader subReader = XmlReader.Create(new StringReader(subXml), subReaderSettings);

                    if (subReader.ReadToFollowing(tag) && subReader.MoveToAttribute(attributeName))
                    {
                        attribute = subReader.GetAttribute(attributeName);
                        success = true;
                    }
                }
            }

            return success;
        }

        public static bool CreateFile(string applicationLocation, bool blockingInput, bool blockingOutput, string logLocation, string configLocation)
        {
            string blockI = blockingInput ? ConsoleSnifferText.valueTRUE : ConsoleSnifferText.valueFALSE;
            string blockO = blockingOutput ? ConsoleSnifferText.valueTRUE : ConsoleSnifferText.valueFALSE;

            string file =
                "<!--" +
                "\n	This is an example configuration file for ConsoleSniffer." +
                "\n-->" +
                "\n" +
                "\n<" + ConsoleSnifferText.xmlConfigurationTag + " " + ConsoleSnifferText.xmlActiveAttribute + "=\"" + ConsoleSnifferText.valueTRUE + "\">" +
                "\n" +
                "\n	<" + ConsoleSnifferText.xmlBlockingInputTag + " " + ConsoleSnifferText.xmlValueAttribute + "=\"" + blockI + "\" />" +
                "\n	<" + ConsoleSnifferText.xmlBlockingOutputTag + " " + ConsoleSnifferText.xmlValueAttribute + "=\"" + blockO + "\" />" +
                "\n	<" + ConsoleSnifferText.xmlLogFileTag + " " + ConsoleSnifferText.xmlLocationAttribute + "=\"" + logLocation + "\" />" +
                "\n	<" + ConsoleSnifferText.xmlApplicationFileTag + " " + ConsoleSnifferText.xmlLocationAttribute + "=\"" + applicationLocation + "\" />" +
                "\n" +
                "\n	<" + ConsoleSnifferText.xmlInputManipulationTag + ">" +
                "\n		<" + ConsoleSnifferText.xmlIfTag + " " + ConsoleSnifferText.xmlContainsAttribute + "=\"monitor reset run\">" +
                "\n			<" + ConsoleSnifferText.xmlReplaceTag + " " + ConsoleSnifferText.xmlOriginalAttribute + "=\"monitor reset run\" " + ConsoleSnifferText.xmlNewAttribute + "=\"^done\" />" +
                "\n			<" + ConsoleSnifferText.xmlEchoInputTag + " " + ConsoleSnifferText.xmlValueAttribute + "=\"" + ConsoleSnifferText.valueTRUE + "\" />" +
                "\n			<" + ConsoleSnifferText.xmlBlockInputTag + " " + ConsoleSnifferText.xmlValueAttribute + "=\"" + ConsoleSnifferText.valueTRUE + "\" />" +
                "\n		<" + ConsoleSnifferText.xmlElseIfTag + " " + ConsoleSnifferText.xmlContainsAttribute + "=\"monitor delay 3000\" > " +
                "\n			<" + ConsoleSnifferText.xmlReplaceTag + " " + ConsoleSnifferText.xmlOriginalAttribute + "=\"monitor delay 3000\" " + ConsoleSnifferText.xmlNewAttribute + "=\"^done\" />" +
                "\n			<" + ConsoleSnifferText.xmlEchoInputTag + " " + ConsoleSnifferText.xmlValueAttribute + "=\"" + ConsoleSnifferText.valueTRUE + "\" />" +
                "\n			<" + ConsoleSnifferText.xmlBlockInputTag + " " + ConsoleSnifferText.xmlValueAttribute + "=\"" + ConsoleSnifferText.valueTRUE + "\" />" +
                "\n			</" + ConsoleSnifferText.xmlElseIfTag + ">" +
                "\n		<" + ConsoleSnifferText.xmlElseIfTag + " " + ConsoleSnifferText.xmlContainsAttribute + "=\"monitor halt\" > " +
                "\n			<" + ConsoleSnifferText.xmlReplaceTag + " " + ConsoleSnifferText.xmlOriginalAttribute + "=\"monitor halt\" " + ConsoleSnifferText.xmlNewAttribute + "=\"^done\" />" +
                "\n			<" + ConsoleSnifferText.xmlEchoInputTag + " " + ConsoleSnifferText.xmlValueAttribute + "=\"" + ConsoleSnifferText.valueTRUE + "\" />" +
                "\n			<" + ConsoleSnifferText.xmlBlockInputTag + " " + ConsoleSnifferText.xmlValueAttribute + "=\"" + ConsoleSnifferText.valueTRUE + "\" />" +
                "\n			</" + ConsoleSnifferText.xmlElseIfTag + ">" +
                "\n		<" + ConsoleSnifferText.xmlElseIfTag + " " + ConsoleSnifferText.xmlContainsAttribute + "=\"-gdb-set --thread-group i1 language c\" > " +
                "\n			<" + ConsoleSnifferText.xmlReplaceTag + " " + ConsoleSnifferText.xmlOriginalAttribute + "=\"-gdb-set --thread-group i1 language c\" " + ConsoleSnifferText.xmlNewAttribute + "=\"^done\" />" +
                "\n			<" + ConsoleSnifferText.xmlEchoInputTag + " " + ConsoleSnifferText.xmlValueAttribute + "=\"" + ConsoleSnifferText.valueTRUE + "\" />" +
                "\n			<" + ConsoleSnifferText.xmlBlockInputTag + " " + ConsoleSnifferText.xmlValueAttribute + "=\"" + ConsoleSnifferText.valueTRUE + "\" />" +
                "\n			</" + ConsoleSnifferText.xmlElseIfTag + ">" +
                "\n		<" + ConsoleSnifferText.xmlElseIfTag + " " + ConsoleSnifferText.xmlContainsAttribute + "=\"-interpreter-exec --thread-group i1 console &quot;p/x (char)-1&quot;\" > " +
                "\n			<" + ConsoleSnifferText.xmlReplaceTag + " " + ConsoleSnifferText.xmlOriginalAttribute + "=\"-interpreter-exec --thread-group i1 console &quot;p/x (char)-1&quot;\" " + ConsoleSnifferText.xmlNewAttribute + "=\"^done\" />" +
                "\n			<" + ConsoleSnifferText.xmlEchoInputTag + " " + ConsoleSnifferText.xmlValueAttribute + "=\"" + ConsoleSnifferText.valueTRUE + "\" />" +
                "\n			<" + ConsoleSnifferText.xmlBlockInputTag + " " + ConsoleSnifferText.xmlValueAttribute + "=\"" + ConsoleSnifferText.valueTRUE + "\" />" +
                "\n			</" + ConsoleSnifferText.xmlElseIfTag + ">" +
                "\n		<" + ConsoleSnifferText.xmlElseIfTag + " " + ConsoleSnifferText.xmlContainsAttribute + "=\"-gdb-set --thread-group i1 language auto\" > " +
                "\n			<" + ConsoleSnifferText.xmlReplaceTag + " " + ConsoleSnifferText.xmlOriginalAttribute + "=\"-gdb-set --thread-group i1 language auto\" " + ConsoleSnifferText.xmlNewAttribute + "=\"^done\" />" +
                "\n			<" + ConsoleSnifferText.xmlEchoInputTag + " " + ConsoleSnifferText.xmlValueAttribute + "=\"" + ConsoleSnifferText.valueTRUE + "\" />" +
                "\n			<" + ConsoleSnifferText.xmlBlockInputTag + " " + ConsoleSnifferText.xmlValueAttribute + "=\"" + ConsoleSnifferText.valueTRUE + "\" />" +
                "\n			</" + ConsoleSnifferText.xmlElseIfTag + ">" +
                "\n		<" + ConsoleSnifferText.xmlElseIfTag + " " + ConsoleSnifferText.xmlContainsAttribute + "=\"-list-thread-groups\" > " +
                "\n			<" + ConsoleSnifferText.xmlReplaceTag + " " + ConsoleSnifferText.xmlOriginalAttribute + "=\"-list-thread-groups\" " + ConsoleSnifferText.xmlNewAttribute + "=\"^done,groups=[{id=&quot;i1&quot;,type=&quot;process&quot;,pid=&quot;42000&quot;,executable=&quot;D:\\Documents\\PSoC Creator\\BasicDesign01\\HelloWorld_Blinky01.cydsn\\CortexM3\\ARM_GCC_493\\Debug\\HelloWorld_Blinky01.elf&quot;}]\" />" +
                "\n			<" + ConsoleSnifferText.xmlEchoInputTag + " " + ConsoleSnifferText.xmlValueAttribute + "=\"" + ConsoleSnifferText.valueTRUE + "\" />" +
                "\n			<" + ConsoleSnifferText.xmlBlockInputTag + " " + ConsoleSnifferText.xmlValueAttribute + "=\"" + ConsoleSnifferText.valueTRUE + "\" />" +
                "\n			</" + ConsoleSnifferText.xmlElseIfTag + ">" +
                "\n		<" + ConsoleSnifferText.xmlElseIfTag + " " + ConsoleSnifferText.xmlContainsAttribute + "=\"-thread-info 0\" > " +
                "\n			<" + ConsoleSnifferText.xmlReplaceTag + " " + ConsoleSnifferText.xmlOriginalAttribute + "=\"-thread-info 0\" " + ConsoleSnifferText.xmlNewAttribute + "=\"^done\" />" +
                "\n			<" + ConsoleSnifferText.xmlEchoInputTag + " " + ConsoleSnifferText.xmlValueAttribute + "=\"" + ConsoleSnifferText.valueTRUE + "\" />" +
                "\n			<" + ConsoleSnifferText.xmlBlockInputTag + " " + ConsoleSnifferText.xmlValueAttribute + "=\"" + ConsoleSnifferText.valueTRUE + "\" />" +
                "\n			</" + ConsoleSnifferText.xmlElseIfTag + ">" +
                "\n		<" + ConsoleSnifferText.xmlElseTag + ">" +
                "\n		    <" + ConsoleSnifferText.xmlIfTag + " " + ConsoleSnifferText.xmlContainsAttribute + "=\"-exec-run\" > " +
                "\n			    <" + ConsoleSnifferText.xmlReplaceTag + " " + ConsoleSnifferText.xmlOriginalAttribute + "=\"-exec-run\" " + ConsoleSnifferText.xmlNewAttribute + "=\"-exec-continue\" />" +
                "\n			</" + ConsoleSnifferText.xmlIfTag + ">" +
                "\n		    <" + ConsoleSnifferText.xmlIfTag + " " + ConsoleSnifferText.xmlContainsAttribute + "=\"--thread 0\" > " +
                "\n			    <" + ConsoleSnifferText.xmlReplaceTag + " " + ConsoleSnifferText.xmlOriginalAttribute + "=\"--thread 0\" " + ConsoleSnifferText.xmlNewAttribute + "=\"--thread 1\" />" +
                "\n			</" + ConsoleSnifferText.xmlIfTag + ">" +
                "\n			</" + ConsoleSnifferText.xmlElseTag + ">" +
                "\n		</" + ConsoleSnifferText.xmlIfTag + ">" +
                "\n	</" + ConsoleSnifferText.xmlInputManipulationTag + ">" +
                "\n	" +
                "\n	<" + ConsoleSnifferText.xmlOutputManipulationTag + ">" +
                "\n	</" + ConsoleSnifferText.xmlOutputManipulationTag + ">" +
                "\n" +
                "\n</" + ConsoleSnifferText.xmlConfigurationTag + ">";

            File.WriteAllText(configLocation, file);

            return false;
        }
    }
}
