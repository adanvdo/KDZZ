using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace KDZZ
{
    public class FileTool
    {
        public static string project_files_path = "KDZZ_PackageFiles";
        public static string full_path = string.Empty;
        public static string ProjectRootDir { get; set; }

        public static bool Init(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Console.WriteLine("    - Existing Project Folder Detected");
                    Console.WriteLine("    - Clearing Files..");
                    Directory.Delete(path, true);
                }
                
                Console.WriteLine("    - Initializing Directory.." + path);
                Directory.CreateDirectory(path);
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
            Console.Write("\r{0} {1} - {2}                 ", message, file, progressmessage);
        }

        private static scripts.ModelBins ProcessDirectories(string projPath, scripts.ModelBins modelBins, List<FileInfo> bins)
        {
            int bincount = modelBins.BinCount;
            int filecount = bins.Count;
            int processcount = 0;
            int copycount = 0;
            string msg = "    - Processing File ";

            if (!Directory.Exists(Path.Combine(projPath, "META-INF")))
                Directory.CreateDirectory(Path.Combine(projPath, "META-INF"));
            if (!Directory.Exists(Path.Combine(projPath, "META-INF", "com")))
                Directory.CreateDirectory(Path.Combine(projPath, "META-INF", "com"));
            if (!Directory.Exists(Path.Combine(projPath, "META-INF", "com", "google")))
                Directory.CreateDirectory(Path.Combine(projPath, "META-INF", "com", "google"));
            if (!Directory.Exists(Path.Combine(projPath, "META-INF", "com", "google", "android")))
                Directory.CreateDirectory(Path.Combine(projPath, "META-INF", "com", "google", "android"));
            if (!File.Exists(Path.Combine(projPath, "META-INF", "com", "google", "android", "update-binary")))
            {
                string binary = Path.Combine(KDZZ.Program.Root, "files", "update-binary");
                string binarytgt = Path.Combine(projPath, "META-INF", "com", "google", "android", "update-binary");
                int bincopied = copy(binary, binarytgt); 
                if(bincopied < 1)
                {
                    KDZZ.Program.ReturnError("Unable to copy update-binary");                    
                }
            }

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
            if (!Directory.Exists(Path.Combine(projPath, "modem")))
                Directory.CreateDirectory(Path.Combine(projPath, "modem"));
            foreach(string s in modelBins.MPR)
            {
                FileInfo f = findByName(s, bins);
                string tgt = Path.Combine(projPath, "modem", s + ".img");
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
            if (!Directory.Exists(Path.Combine(projPath, "primary")))
                Directory.CreateDirectory(Path.Combine(projPath, "primary"));
            foreach (string s in modelBins.PRI)
            {
                FileInfo f = findByName(s, bins);
                string tgt = Path.Combine(projPath, "primary", s + ".img");
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
            Console.WriteLine("    - Process Complete");
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

        private static bool moveDir(string source, string target)
        {
            try
            {
                if (!Directory.Exists(source))
                    return false;
                if (Directory.Exists(target))
                    Directory.Delete(target, true);
                Directory.Move(source, target);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
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

        public static bool ArrangeFilesForPackage(KDZZ.scripts.PackageType packageType)
        {
            Console.WriteLine("    - Preparing Package Files...");
            if (Directory.Exists(Path.Combine(ProjectRootDir, "package")))
                Directory.Delete(Path.Combine(ProjectRootDir, "package"), true);
            Directory.CreateDirectory(Path.Combine(ProjectRootDir, "package"));
            bool movemeta = moveDir(Path.Combine(ProjectRootDir, project_files_path, "META-INF"), Path.Combine(ProjectRootDir, "package", "META-INF"));
            if (packageType == scripts.PackageType.Bootloader)
            {
                bool moved = moveDir(Path.Combine(ProjectRootDir, project_files_path, "bootloader"), Path.Combine(ProjectRootDir, "package", "bootloader"));
                return moved;
            }
            else if (packageType == scripts.PackageType.Modem)
            {
                bool moved = moveDir(Path.Combine(ProjectRootDir, project_files_path, "modem"), Path.Combine(ProjectRootDir, "package"));
                return moved;
            }
            else if (packageType == scripts.PackageType.LAF)
            {
                bool moved = moveDir(Path.Combine(ProjectRootDir, project_files_path, "dlmode_recov"), Path.Combine(ProjectRootDir, "package"));
                return moved;
            }
            else if (packageType == scripts.PackageType.FullStock)
            {
                bool moved = moveDir(Path.Combine(ProjectRootDir, project_files_path, "bootloader"), Path.Combine(ProjectRootDir, "package", "bootloader")) 
                    && moveDir(Path.Combine(ProjectRootDir, project_files_path, "modem"), Path.Combine(ProjectRootDir, "package"))
                    && moveDir(Path.Combine(ProjectRootDir, project_files_path, "primary"), Path.Combine(ProjectRootDir, "package"));
                return moved;
            }
            return false;
        }

        public static string CreateZipPackage(string packageName)
        {
            Console.WriteLine("    - Creating Zip Package..");
            string p = Path.Combine(ProjectRootDir, "package");
            string zp = Path.Combine(ProjectRootDir, packageName + ".zip");
            try
            {
                ZipFile.CreateFromDirectory(p, zp);
            }
            catch(Exception ex)
            {
                KDZZ.Program.ReturnError(ex.Message);
            }
            Console.WriteLine("    - Zip Package Created");
            return zp;
        }
    }
}
