using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KDZZ
{
    public class NumericDialog
    {        
        public static NumericDialogChoice ShowNumericDialog(string message, List<string> options)
        {
            return ShowNumericDialog(new List<string>() { message }, options);
        }

        public static NumericDialogChoice ShowNumericDialog(List<string> message, List<string> options)
        {
            int choice = -1;
            int attempts = 0;
            string res = string.Empty;
            while(!validChoice(res, options))
            {
                if(attempts > 0)
                {
                    Console.WriteLine("    - Invalid Selection");
                }
                BoxedLine.DrawBoxedLine(message.ToArray(), 75, ConsoleColor.Black, ConsoleColor.White, true, true, true);
                //Console.WriteLine(message);
                //Console.WriteLine("");
                List<string> msg = new List<string>();
                msg.Add("");
                for (int i = 0; i < options.Count; i++)
                {
                    msg.Add((i + 1).ToString() + ". " + options[i]);
                    //Console.WriteLine((i + 1).ToString() + ". " + options[i]);
                }
                msg.Add("");
                BoxedLine.DrawBoxedLine(msg.ToArray(), 75, ConsoleColor.Black, ConsoleColor.White, false, false, true);
                //Console.WriteLine("");
                Console.Write(" => ");
                res = Console.ReadLine();
                Console.WriteLine("");
            }
            bool valid = Int32.TryParse(res, out choice);
            return new NumericDialogChoice(choice, options[choice-1]);
        }

        private static bool validChoice(string choice, List<string> options)
        {
            bool valid = false;
            int validated = -1;
            valid = Int32.TryParse(choice, out validated);
            return valid && validated <= options.Count;
        }
    }

    public class NumericDialogChoice
    {
        public int Choice { get; set; }
        public string Description { get; set; }

        public NumericDialogChoice(int choice, string desc)
        {
            Choice = choice;
            Description = desc;
        }
    }
    
}
