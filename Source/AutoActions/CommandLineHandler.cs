using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoActions
{
    public static class CommandLineHandler
    {

        public static void HandleArguments(string[] arguments)
        {
            try
            {
                string firstArgument = arguments[0];
                string output = string.Empty;
                switch (firstArgument)
                {
                    case "-a":
                        output= HandleActions(arguments[1]);
                        break;
                    case "-?":
                    default:
                        output= HandleHelp();
                        break;
                }
                Console.WriteLine(output);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static string HandleActions(string actionName)
        {
                try
                {
                if (actionName.StartsWith("\""))
                    actionName = actionName.Substring(1, actionName.Length - 1);
                    if (actionName.EndsWith("\""))
                        actionName = actionName.Substring(0, actionName.Length - 1);
                    ProjectData.Instance.Settings.ActionShortcuts.First(a => a.ShortcutName.ToUpperInvariant().Equals(actionName.ToUpperInvariant())).RunAction();
                return $"Action {actionName} was successful.";
                }
                catch (Exception ex)
                {

                return $"Action {actionName} failed: {ex.Message}";
                }
        }

        private static string HandleHelp()
        {
            return @"
AutoActions Command Line Handler

Following commands are supported:

-?  : Posts this text
-a  ""<actionName"": Runs the action with the entered name.
";
        }
    }
}
