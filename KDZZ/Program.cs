using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using KDZZ.scripts;

namespace KDZZ
{
    class Program
    {
        public static string Root { get; set; }
        private static ConsoleColor dftForeColor = Console.ForegroundColor;
        private static ConsoleColor dftBackColor = Console.BackgroundColor;

        static void Main(string[] args)
        {

            Root = GetApplicationRoot();
            if (args.Length < 1)
                ReturnError("error: missing arguments");
            else
            {
                string projectDir = string.Empty;
                string binsDir = string.Empty;
                bool useLGFE = false;
                for (int i = 0; i < args.Length; i++)
                {
                    string s = args[i];
                    /* args 
                     * 
                     * -p   |  project dir
                     * -b   |  extracted bins dir
                     * -r   |  reset
                     * -X   |  use LGFirmwareExtract dir structure
                     * 
                     * 
                     */

                    if (s == "-p")
                    {
                        try
                        {
                            projectDir = Path.GetFullPath(args[i + 1]);
                        }
                        catch (Exception ex)
                        {
                            ReturnError(ex.Message);
                        }
                    }

                    if (s == "-b")
                    {
                        try
                        {
                            binsDir = Path.GetFullPath(args[i + 1]);
                        }
                        catch (Exception ex)
                        {
                            ReturnError(ex.Message);
                        }
                    }
                    if (s == "-X")
                    {
                        useLGFE = true;
                    }
                }
                if (string.IsNullOrEmpty(projectDir))
                {
                    ReturnError("missing directory argument");
                }
                else continueLoading(projectDir, binsDir, useLGFE);
            }
        }

        private static void continueLoading(string projPath, string binsPath, bool useLGFE)
        {
            if (string.IsNullOrEmpty(binsPath))
            {
                binsPath = projPath;
            }
            bool init = KDZZ.FileTool.Init(projPath);
            if (init)
            {
                NumericDialogChoice res = new NumericDialogChoice(-1, string.Empty);
                Console.WriteLine("    - Directory " + Path.Combine(projPath, FileTool.project_files_path) + " initialized.");
                Console.WriteLine("    - Bins Directory: " + binsPath);
                res = NumericDialog.ShowNumericDialog("Please verify KDZ has been extracted to: " + binsPath, new List<string>() { "Continue", "Quit" });
                Console.Clear();
                if (res.Choice == 2) { Environment.Exit(0); }
                List<string> models = builder.getModels();
                NumericDialogChoice response = NumericDialog.ShowNumericDialog("Select Device Model", models);
                Console.Clear();
                Console.WriteLine("    - Selected: " + response.Description);
                Console.WriteLine("    - Loading KDZ File List..");
                ModelBins model = new ModelBins(response.Description);
                bool binsexist = false;
                while (!binsexist)
                {
                    List<string> bins = new List<string>();
                    bins.AddRange(model.BL);
                    bins.AddRange(model.MPR);
                    bins.AddRange(model.PRI);
                    bins.AddRange(model.DLR);

                    binsexist = FileTool.BinsExist(bins, binsPath);
                    Console.WriteLine("    - Bins Exist: " + binsexist.ToString());
                    if (!binsexist)
                    {
                        NumericDialogChoice binres = NumericDialog.ShowNumericDialog(new List<string>() { "Please extract all KDZ bins and images to", binsPath }, new List<string>() { "Continue", "Quit" });
                        Console.Clear();
                        if (binres.Choice == 2) { Environment.Exit(0); }
                    }
                }
                processBins(projPath, binsPath, model, useLGFE);
            }
        }

        private static async void processBins(string projPath, string binsPath, ModelBins modelBins, bool useLGFE)
        {
            ModelBins binfiles = await FileTool.ProcessBins(modelBins, projPath, binsPath, useLGFE);
            Console.ReadKey();
        }



        public static void ReturnError(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine("press any key to exit");
            Console.ReadKey();
            Environment.Exit(0);
        }

        public static string GetApplicationRoot()
        {
            var exePath = Path.GetDirectoryName(System.Reflection
                              .Assembly.GetExecutingAssembly().CodeBase);
            Regex appPathMatcher = new Regex(@"(?<!fil)[A-Za-z]:\\+[\S\s]*?(?=\\+bin)");
            var appRoot = appPathMatcher.Match(exePath).Value;
            return appRoot;
        }
    }
}
