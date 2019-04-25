//By Marcus Wynwood, May 2006, www.mwynwood.com

#region Using directives

using System;
using System.IO;
using System.Windows.Forms;
using System.Collections;

#endregion

namespace AllSorts
{
    /// <summary>
    /// This class is used to make very simple settings files
    /// </summary>
    public class FlatFileSettings
    {
        public string SettingsFileName;
        private const char KEY_SEPERATOR = '=';

        public FlatFileSettings(string newFileName)
        {
            SettingsFileName = newFileName;
        }

        public void saveSetting(string settingKeyToSave, string valueToSave)
        {

            // read the file into an array
            // remove duplicate keys
            // add the new key to the array
            // delete (or blank) the file
            // write the array to the file

            ArrayList tempArray = new ArrayList();

            // If the settings file exists, read it into tempArray:
            if (File.Exists(SettingsFileName) == true)
            {
                StreamReader reader = new StreamReader(SettingsFileName);
                string line;
                // while there are more lines and the line isn't the new key...
                while ((line = reader.ReadLine()) != null && !line.StartsWith(settingKeyToSave + KEY_SEPERATOR))
                {
                    tempArray.Add(line); // add it to the array
                }
                reader.Close();
            }

            // tempArray should now be full of the settings file MINUS the new setting, we'd better add it:
            tempArray.Add(settingKeyToSave + KEY_SEPERATOR + valueToSave);

            // Lets delete the old settings file:
            File.Delete(SettingsFileName);

            // Now, lets make a new one from the values in tempArray
            // If there was no file, this will create one
            StreamWriter writer = File.AppendText(SettingsFileName);
            foreach (string s in tempArray)
            {
                writer.WriteLine(s);
            }
            writer.Flush();
            writer.Close();

            // Done.
        }

        public string loadSetting(string settingKeyToLoad, string defaultValueToUseIfThereIsAnError)
        {
            string theLineWeArelookingFor = "";

            if (File.Exists(SettingsFileName) == true)
            {
                StreamReader reader = new StreamReader(SettingsFileName);
                string line = "";
                // check each line for our key:
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith(settingKeyToLoad + KEY_SEPERATOR))
                    {
                        theLineWeArelookingFor = line;
                    }
                }
                reader.Close();

                if (theLineWeArelookingFor.StartsWith(settingKeyToLoad + KEY_SEPERATOR))
                {
                    // return only the value
                    return theLineWeArelookingFor.Replace(settingKeyToLoad + KEY_SEPERATOR, "");
                }
                else
                {
                    return defaultValueToUseIfThereIsAnError;
                }
            }
            else
            {
                return defaultValueToUseIfThereIsAnError;
            }
        }
    }
}
