using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace KDZZ
{
    public class FileTool
    {
        public static string project_files_path = "KDZZ_PackageFiles";
        public static string full_path = string.Empty;
        public static Uri ProjectDir { get; set; }

        public static bool Init(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Console.WriteLine("Creating directory " + path);
                    Directory.CreateDirectory(path);
                }
                DirectoryInfo dir = new DirectoryInfo(path);
                DirectoryInfo[] subDirs = dir.GetDirectories("*", SearchOption.TopDirectoryOnly);
                bool dirExists = false;
                for(int i = 0; i < subDirs.Length; i++)
                {
                    if(subDirs[i].Name == project_files_path)
                    { dirExists = true; full_path = subDirs[i].FullName; break; }
                }
                if(dirExists)
                {
                    NumericDialogChoice res = new NumericDialogChoice(-1, string.Empty);
                    while (res.Choice < 0)
                    {
                        res = NumericDialog.ShowNumericDialog("Existing Project Folder Detected: " + project_files_path, new List<string>() { "Keep", "Reset" });
                        Console.Clear();
                        if (res.Choice == 2)
                        {
                            Directory.Delete(Path.Combine(path, project_files_path));
                            break;
                        }
                    }                    
                }
                
                Directory.CreateDirectory(Path.Combine(path, project_files_path));
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadKey();
            }
            return false;
        }
        
        public static bool BinsExist(List<string> bins, string path)
        {
            List<FileInfo> files = getFiles(new DirectoryInfo(path));
            foreach(string s in bins)
            {
                bool exists = false;
                foreach(FileInfo f in files)
                {
                    if(f.Name.Contains(s))
                    {
                        exists = true;
                        break;
                    }
                }
                if (!exists) return false;
            }
            return true;
        }

        public static scripts.ModelBins ProcessBins(KDZZ.scripts.ModelBins modelBins, string projPath, string binsPath)
        {
            DirectoryInfo p = new DirectoryInfo(projPath);
            DirectoryInfo b = new DirectoryInfo(binsPath);
            List<FileInfo> bins = getFiles(b);
            return ProcessDirectories(p.FullName, modelBins, bins);
        }

        public static FileInfo findByName(string binname, List<FileInfo> bins)
        {
            foreach(FileInfo f in bins)
            {
                if (binname == "system" && f.Name.Contains(binname))
                    return f;
                else if (f.Name.Contains(binname+"_"))
                    return f;
            }
            return null;
        }

        private static void displayProgress(string message, string file, string progressmessage)
        {
            Console.Write("\r{0} {1} - {2}", message, file, progressmessage);
        }

        private static scripts.ModelBins ProcessDirectories(string projPath, scripts.ModelBins modelBins, List<FileInfo> bins)
        {
            int bincount = modelBins.BinCount;
            int filecount = bins.Count;
            int processcount = 0;
            int copycount = 0;
            string msg = "    - Processing File ";

            if(!Directory.Exists(Path.Combine(projPath, "bootloader")))
                Directory.CreateDirectory(Path.Combine(projPath, "bootloader"));            
            for(int i = modelBins.BL.Count-1; i >= 0; i--) //each(string s in modelBins.BL)
            {
                string s = modelBins.BL[i];
                FileInfo f = findByName(s, bins);
                string tgt = Path.Combine(projPath, "bootloader", s + ".img");
                modelBins.BLPATHS.Add(tgt);
                processcount++;
                displayProgress(msg, f.Name, processcount.ToString() + " of " + bincount.ToString());
                int cr = copy(f.FullName, tgt);
                copycount += cr;
            }
            foreach(string s in modelBins.MPR)
            {
                FileInfo f = findByName(s, bins);
                string tgt = Path.Combine(projPath, s + ".img");
                modelBins.MPRPATHS.Add(tgt);
                processcount++;
                displayProgress(msg, f.Name, processcount.ToString() + " of " + bincount.ToString());
                int cr = copy(f.FullName, tgt);
                copycount += cr;
            }
            if (!Directory.Exists(Path.Combine(projPath, "dlmode_recov")))
                Directory.CreateDirectory(Path.Combine(projPath, "dlmode_recov"));
            foreach (string s in modelBins.DLR)
            {
                FileInfo f = findByName(s, bins);
                string tgt = Path.Combine(projPath, "dlmode_recov", s + ".img");
                modelBins.DLRPATHS.Add(tgt);
                processcount++;
                displayProgress(msg, f.Name, processcount.ToString() + " of " + bincount.ToString());
                int cr = copy(f.FullName, tgt);
                copycount += cr;
            }
            foreach (string s in modelBins.PRI)
            {
                FileInfo f = findByName(s, bins);
                string tgt = Path.Combine(projPath, s + ".img");
                modelBins.PRIPATHS.Add(tgt);
                processcount++;
                if(f.Name == "system.img")
                {
                    displayProgress("    - Processing System Image (this may take several minutes. do not close console) ", f.Name, processcount.ToString() + " of " + bincount.ToString());
                }
                else
                {                
                    displayProgress(msg, f.Name, processcount.ToString() + " of " + bincount.ToString());
                }
                int cr = copy(f.FullName, tgt);
                copycount += cr;
            }
            Console.Clear();
            Console.Write("    - Process Complete");
            return modelBins;
        }

        private static int copy(string source, string target)
        {            
            try
            {
                File.Copy(source, target, true);
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        public static async Task CopyFileAsync(string sourcePath, string destinationPath)
        {
            using (Stream source = File.Open(sourcePath, FileMode.Open))
            {
                using (Stream destination = File.Create(destinationPath))
                {
                    await source.CopyToAsync(destination);
                }
            }
        }

        private static List<FileInfo> getFiles(DirectoryInfo dir)
        {
            List<FileInfo> files = new List<FileInfo>();
            List<FileInfo> dirFiles = new List<FileInfo>();
            dirFiles.AddRange(dir.GetFiles("*", SearchOption.AllDirectories));
            foreach(FileInfo fi in dirFiles)
            {
                if (fi.Name.Contains("raw_resources") && !fi.Name.Contains("bak_"))
                    files.Add(fi);
                else if (fi.Name == "system.img")
                    files.Add(fi);
                else if (!fi.Name.Contains("system_") && !fi.Name.Contains("bak_"))
                    files.Add(fi);
            }
            return files;
        }
    }
}
