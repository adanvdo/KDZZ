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
        private static string extractBLtoTMP = "package_extract_dir(\"bootloader\", \"/tmp\");\n" +
"set_perm_recursive(0, 0, 0777, 0777, \"/tmp\");\n" +
"ui_print(\"Verifying Bootloader contents..\");\n";

        private static string uiprint(string text)
        {
            string s = "ui_print(\"" + text + "\");\n";
            return s;
        }

        private static string lineExtractFile(string filePath)
        {
            string name = Path.GetFileNameWithoutExtension(filePath);
            string s = "package_extract_file(\"" + filePath + "\", \"/dev/block/bootdevice/by-name/" + name + "\");\n";
            return s;
        }

        private static string titleLines(string title)
        {
            string t = builder.uiprint(" ") + builder.uiprint(title) + builder.uiprint(" ");
            return t;            
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

        public static void LoadModel(string model)
        {             
            ModelBins scripts = new ModelBins(model);
            StringBuilder sbs = new StringBuilder();
            Console.Write("Enter Package Title => ");
            string title = Console.ReadLine();
            sbs.Append(titleLines(title));
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
}
