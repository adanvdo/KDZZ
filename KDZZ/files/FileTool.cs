using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

        public static scripts.ModelBins ProcessBins(KDZZ.scripts.ModelBins modelBins, string projPath, string binsPath, bool useLGFE)
        {
            DirectoryInfo p = new DirectoryInfo(projPath);
            DirectoryInfo b = new DirectoryInfo(binsPath);
            List<FileInfo> bins = getFiles(b);
            return processDirectories(p.FullName, modelBins, bins);
        }

        public static FileInfo findByName(string binname, List<FileInfo> bins)
        {
            foreach(FileInfo f in bins)
            {
                if (f.Name.Contains(binname))
                    return f;
            }
            return null;
        }

        private static scripts.ModelBins processDirectories(string projPath, scripts.ModelBins modelBins, List<FileInfo> bins)
        {            

            if(!Directory.Exists(Path.Combine(projPath, "bootloader")))
                Directory.CreateDirectory(Path.Combine(projPath, "bootloader"));            
            foreach(string s in modelBins.BL)
            {
                FileInfo f = findByName(s, bins);
                modelBins.BLPATHS.Add(Path.Combine(projPath, "bootloader", s + ".img"));
                f.CopyTo(Path.Combine(projPath, "bootloader", s + ".img"), true);
            }
            foreach(string s in modelBins.MPR)
            {
                FileInfo f = findByName(s, bins);
                modelBins.MPRPATHS.Add(Path.Combine(projPath, s + ".img"));
                f.CopyTo(Path.Combine(projPath, s + ".img"), true);
            }
            foreach (string s in modelBins.PRI)
            {
                FileInfo f = findByName(s, bins);
                modelBins.PRIPATHS.Add(Path.Combine(projPath, s + ".img"));
                f.CopyTo(Path.Combine(projPath, s + ".img"), true);
            }
            if (!Directory.Exists(Path.Combine(projPath, "dlmode_recov")))
                Directory.CreateDirectory(Path.Combine(projPath, "dlmode_recov"));
            foreach (string s in modelBins.DLR)
            {
                FileInfo f = findByName(s, bins);
                modelBins.DLRPATHS.Add(Path.Combine(projPath, "dlmode_recov", s + ".img"));
                f.CopyTo(Path.Combine(projPath, "dlmode_recov", s + ".img"), true);
            }
            return modelBins;
        }


        private static List<FileInfo> getFiles(DirectoryInfo dir)
        {
            List<FileInfo> files = new List<FileInfo>();
            files.AddRange(dir.GetFiles("*", SearchOption.AllDirectories));
            return files;
        }
    }
}
