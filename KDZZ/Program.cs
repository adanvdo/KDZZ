using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using KDZZ.scripts;

namespace KDZZ
{
    class Program
    {
        public static string Root { get; set; }
        public static string ProjectRoot { get; set; }
        public static string Project { get; set; }
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
                for (int i = 0; i < args.Length; i++)
                {
                    string s = args[i];
                    /* args 
                     * 
                     * -p   |  project dir
                     * -b   |  extracted bins dir
                     * -r   |  reset
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
                }
                if (string.IsNullOrEmpty(projectDir))
                {
                    ReturnError("missing directory argument");
                }
                else continueLoading(projectDir, binsDir);
            }
        }

        private static void continueLoading(string projPath, string binsPath)
        {
            if (string.IsNullOrEmpty(binsPath))
            {
                binsPath = projPath;
            }
            bool init = KDZZ.FileTool.Init(projPath);
            ProjectRoot = projPath;
            Project = Path.Combine(projPath, FileTool.project_files_path);
            FileTool.ProjectRootDir = ProjectRoot;
            if (init)
            {
                NumericDialogChoice res = new NumericDialogChoice(-1, string.Empty);
                Console.WriteLine("    - Directory " + Project + " initialized.");
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
                ModelBins copied = FileTool.ProcessBins(model, Project, binsPath);
                NumericDialogChoice buildOptions = NumericDialog.ShowNumericDialog(new List<string>() { "What package(s) would you like to build?" }, new List<string>() { "Bootloader", "Modem", "LAF", "FullStock", "Quit" });
                Console.Clear();
                if(buildOptions.Choice == 5) { Environment.Exit(0); }
                PackageType selectedType = (PackageType)buildOptions.Choice - 1;
                bool binsMoved = FileTool.ArrangeFilesForPackage(selectedType);
                if(!binsMoved)
                {
                    Console.WriteLine("    - Unable to prepare package files.");
                    Console.WriteLine("    - Please make sure the source files exist, and you have the proper folder permissions");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
                Console.WriteLine("    - Package File Preparation Complete");
                Console.WriteLine("");

                string pname = string.Empty;
                while (pname.Length < 3)
                {
                    Console.WriteLine("Enter a name for your package");
                    Console.Write(" => ");
                    pname = Console.ReadLine();
                }
                List<string> script = builder.CreateUpdaterScript(selectedType, pname, copied);
                string metadir = Path.Combine(ProjectRoot, FileTool.project_files_path, "META-INF");
                string usdir = Path.Combine(ProjectRoot, FileTool.project_files_path, "META-INF", "com", "google", "android");
                File.WriteAllText(Path.Combine(usdir, "updater-script"), string.Join("\n", script));
                bool moveMeta = FileTool.MoveDir(metadir, Path.Combine(ProjectRoot, "package", "META-INF"));
                if (!moveMeta)
                    ReturnError("Error moving META-INF Directory");
                string packagepath = FileTool.CreateZipPackage(pname.Replace(" ", "_"), selectedType);
                Console.WriteLine(" ");
                Console.WriteLine("Package Location: " + packagepath);
                Console.WriteLine("Press any key to exit..");
                Console.ReadKey();
            }
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
