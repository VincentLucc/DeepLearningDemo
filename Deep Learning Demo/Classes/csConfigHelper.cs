using Deep_Learning_Demo.Classes;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;


public class csConfigHelper
{
    public static csConfigModel config;

    public static string ConfigPath = Application.StartupPath + @"\Config\config.xml";


    public static bool LoadFromDefault(out string sMessage)
    {
        sMessage = "";
        if (!File.Exists(ConfigPath))
        {
            sMessage = "Unable to find the config file.";
            return false;
        }


        var newConfig = Load(ConfigPath, out sMessage);
        if (newConfig == null)
        {
            sMessage = $"Error while loading the config file.\r\n{sMessage}";
            return false;
        }

        config = newConfig;
        return true;
    }

    public static bool LoadFromFile(out string sMessage)
    {
        sMessage = string.Empty;

        try
        {
            using (XtraOpenFileDialog fileDialog = new XtraOpenFileDialog())
            {

                string sPath = Path.GetDirectoryName(ConfigPath);
                fileDialog.InitialDirectory = sPath;

                fileDialog.Filter = "XML File(*.xml)|*.xml";

                if (fileDialog.ShowDialog() != DialogResult.OK) return false;

                var newConfig = Load(fileDialog.FileName, out sMessage);
                if (newConfig == null)
                {
                    sMessage = $"Error while loading the config file.\r\n{sMessage}";
                    return false;
                }

                config = newConfig;
                return true;

            }
        }
        catch (Exception ex)
        {
            sMessage = $"Error while opening the config file.\r\n{ex.Message}";
            return false;
        }
    }

    private static csConfigModel Load(string sFilePath, out string sMessage)
    {
        sMessage = string.Empty;

        try
        {
            var bData = File.ReadAllBytes(sFilePath);

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(csConfigModel));

            using (MemoryStream stream = new MemoryStream(bData))
            {
                var configObj = xmlSerializer.Deserialize(stream);
                if (configObj is csConfigModel)
                {
                    csConfigModel newConfig = (csConfigModel)configObj;
                    bData = null;
                    return newConfig;
                }
                else
                {
                    sMessage = "Invalid data type.";
                    bData = null;
                    return null;
                }
            }
        }
        catch (Exception ex)
        {
            sMessage = ex.Message;
            Trace.WriteLine($"{csDateTimeHelper.TimeOnly_fff} ReadSettings:\r\n" + ex.Message);
            return null;
        }
    }


    public static bool LoadOrCreateConfig(out string sMessage)
    {
        sMessage = "";

        if (File.Exists(ConfigPath))
        {//File exist, directly load
            return LoadFromDefault(out sMessage);
        }
        else
        {//File not exist, create a new and save
            config = new csConfigModel();
            config.InitData();
            return SaveToDefault(out sMessage);
        }
    }


    private static bool Save(string sFilePath, out string sMessage)
    {

        sMessage = string.Empty;

        try
        {
            //Create if dolder not exist
            string sDir = Path.GetDirectoryName(sFilePath);
            if (!Directory.Exists(sDir)) Directory.CreateDirectory(sDir);


            XmlSerializer xmlSerializer = new XmlSerializer(typeof(csConfigModel));

            using (FileStream fileStream = new FileStream(sFilePath, FileMode.Create,
                FileAccess.Write, FileShare.None, 8192, FileOptions.WriteThrough))
            {
                xmlSerializer.Serialize(fileStream, config);
            }

            return true;

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{csDateTimeHelper.TimeOnly_fff} SaveConfig:\r\n" + ex.Message);
            sMessage = "Error while saving the config file.\r\n" + ex.Message;
            return false;
        }
    }


    public static bool SaveToDefault(out string sMessage)
    {
        return Save(ConfigPath, out sMessage);
    }


    public static bool SaveAs(out string sMessage)
    {
        sMessage = string.Empty;
        using (XtraSaveFileDialog fileDialog = new XtraSaveFileDialog())
        {
            fileDialog.Filter = "XML File(*.xml)|*.xml";

            string sPath = Path.GetDirectoryName(ConfigPath);
            fileDialog.InitialDirectory = sPath;


            if (fileDialog.ShowDialog() != DialogResult.OK) return false;

            return Save(fileDialog.FileName, out sMessage);
        }
    }

}

