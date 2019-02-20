using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace KDZZ.scripts
{
    public class builder
    {
        private static List<string> extractBLtoTMP = new List<string>() { "package_extract_dir(\"bootloader\", \"/tmp\");", "set_perm_recursive(0, 0, 0777, 0777, \"/tmp\");" };

        private static string uiprint(string text)
        {
            string s = "ui_print(\"" + text + "\");";
            return s;
        }

        private static List<string> lineExtractFile(string filePath, bool hasbak)
        {
            string name = Path.GetFileNameWithoutExtension(filePath);
            List<string> list = new List<string>();
            list.Add("package_extract_file(\"" + filePath + "\", \"/dev/block/bootdevice/by-name/" + name + "\");");
            if (hasbak) { list.Add("package_extract_file(\"" + filePath + "\", \"/dev/block/bootdevice/by-name/" + name + "bak\");"); }
            return list;
        }

        private static List<string> titleLines(string title)
        {
            List<string> t = new List<string>() { builder.uiprint(" "), builder.uiprint(title), builder.uiprint(" ") };
            return t;            
        }

        private static List<string> endLines(string packagename)
        {
            List<string> lines = new List<string>() {
                uiprint(" "),
                uiprint(" "),
                uiprint(" "),
                uiprint("       Flash Complete      "),
                uiprint(" "),
                uiprint("      " + packagename + "     "),
                uiprint(" "),
                uiprint(" "),
                uiprint("Done")
            };
            return lines;
        }

        public static List<string> getModels()
        {
            List<string> s = new List<string>();
            try
            {
                using (StreamReader reader = new StreamReader(Path.Combine(Program.GetApplicationRoot(), "firmware.json")))
                {
                    string json = reader.ReadToEnd();
                    dynamic dyn = JsonConvert.DeserializeObject<ExpandoObject>(json, new Newtonsoft.Json.Converters.ExpandoObjectConverter());
                    var devices = dyn.devices;

                    for (int i = 0; i < devices.Count; i++)
                    {
                        s.Add(devices[i].model);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return s;
        }

        public static List<string> CreateUpdaterScript(PackageType packageType, string packageName, ModelBins modelBins)
        {
            Console.WriteLine("    - Building Updater-Script...");
            List<string> sb = new List<string>();
            sb.AddRange(titleLines("Flashing " + packageName + ".."));

            if(packageType == PackageType.Bootloader)
            {
                // add sha1 checksum methods
                sb.AddRange(extractBLtoTMP);
                sb.Add(uiprint("Flashing Bootloader.."));
                foreach(string s in modelBins.BL)
                {
                    sb.AddRange(lineExtractFile(@"bootloader/" + s + ".img", modelBins.NOBAK.IndexOf(s) < 0 ? true : false));
                }
            }
            else if (packageType == PackageType.Modem)
            {
                sb.Add(uiprint("Flashing Modem.."));
                foreach (string s in modelBins.MPR)
                {
                    sb.AddRange(lineExtractFile(s + ".img", modelBins.NOBAK.IndexOf(s) < 0 ? true : false));
                }
            }
            else if (packageType == PackageType.LAF)
            {
                sb.Add(uiprint("Flashing LAF.."));
                foreach (string s in modelBins.DLR)
                {
                    if(s == "laf")
                        sb.AddRange(lineExtractFile(s + ".img", modelBins.NOBAK.IndexOf(s) < 0 ? true : false));
                }
            }
            else if(packageType == PackageType.FullStock)
            {
                sb.AddRange(extractBLtoTMP);
                sb.Add(uiprint("Flashing Bootloader.."));
                foreach (string s in modelBins.BL)
                {
                    sb.AddRange(lineExtractFile(@"bootloader/" + s + ".img", modelBins.NOBAK.IndexOf(s) < 0 ? true : false));
                }
                sb.Add(uiprint("Flashing Modem.."));
                foreach (string s in modelBins.MPR)
                {
                    sb.AddRange(lineExtractFile(s + ".img", modelBins.NOBAK.IndexOf(s) < 0 ? true : false));
                }
                sb.Add(uiprint("Flasing Boot and System"));
                sb.Add(uiprint("This may take a while..."));
                foreach(string s in modelBins.PRI)
                {
                    sb.AddRange(lineExtractFile(s + ".img", modelBins.NOBAK.IndexOf(s) < 0 ? true : false));
                }
            }
            sb.AddRange(endLines(packageName));
            Console.WriteLine("    - Updater Script Created");
            return sb;
        }
    }

    public class ModelBins
    {
        private string model;
        public string Model { get { return model; } set { model = value; } }
        public List<string> BL { get; set; }
        public List<string> MPR { get; set; } 
        public List<string> DLR { get; set; }
        public List<string> PRI { get; set; }
        public List<string> BLPATHS { get; set; }
        public List<string> MPRPATHS { get; set; }
        public List<string> DLRPATHS { get; set; }
        public List<string> PRIPATHS { get; set; }
        public List<string> NOBAK { get; set; }
        public int BinCount { get { return (BL.Count + MPR.Count + DLR.Count + PRI.Count); } }

        public ModelBins(string model)
        {
            BL = new List<string>();
            MPR = new List<string>();
            DLR = new List<string>();
            PRI = new List<string>();

            BLPATHS = new List<string>();
            MPRPATHS = new List<string>();
            DLRPATHS = new List<string>();
            PRIPATHS = new List<string>();

            NOBAK = new List<string>();

            dynamic device = getModel(model);
            if (device == null)
            {
                throw new Exception("model not found");
            }
            else
            {
                this.model = device.model;
                foreach(var obj in device.bootloader)
                {
                    string bin = Convert.ToString(obj);
                    BL.Add(bin);
                }
                foreach(var obj in device.modem)
                {
                    string bin = Convert.ToString(obj);
                    MPR.Add(bin);
                }
                foreach(var obj in device.primary)
                {
                    string image = Convert.ToString(obj);
                    PRI.Add(image);
                }
                foreach(var obj in device.dlmode_recov)
                {
                    string bin = Convert.ToString(obj);
                    DLR.Add(bin);
                }
                foreach(var obj in device.nobak)
                {
                    string bin = Convert.ToString(obj);
                    NOBAK.Add(bin);
                }
            }
        }

        

        private object getModel(string model)
        {
            using (StreamReader reader = new StreamReader(Path.Combine(Program.GetApplicationRoot(), "firmware.json")))
            {
                string json = reader.ReadToEnd();
                dynamic dyn = JsonConvert.DeserializeObject<ExpandoObject>(json, new Newtonsoft.Json.Converters.ExpandoObjectConverter());
                var devices = dyn.devices;
                for(int i = 0; i < devices.Count; i++)
                {
                    if(devices[i].model == model)
                    {
                        return devices[i];
                    }
                }
            }
            return null;
        }
    }

    public enum PackageType
    {
        Bootloader = 0,
        Modem = 1,
        LAF = 2,
        FullStock = 3,
        Custom = 4
    }
}
