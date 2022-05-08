using System.IO;

namespace GloomhavenDeckbuilder.CardEditor.Utils
{
    public class ConfigurationUtils
    {
        public const string CONFIGURATION_PATH = @".\Configurations";

        public static T? LoadConfiguration<T>(string fileName, T @default)
        {
            try
            {
                string filePath = Path.Combine(CONFIGURATION_PATH, fileName);

                if (!Directory.Exists(CONFIGURATION_PATH)) Directory.CreateDirectory("Configurations");
                if (!File.Exists(filePath)) File.WriteAllText(filePath, Newtonsoft.Json.JsonConvert.SerializeObject(@default, Newtonsoft.Json.Formatting.Indented));

                T? result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
                return result is null ? default : result;
            }
            catch
            {
                return @default;
            }

        }

        public static void SaveConfiguration(object configuration, string fileName)
        {
            try
            {
                string filePath = Path.Combine(CONFIGURATION_PATH, fileName);

                if (!Directory.Exists(CONFIGURATION_PATH)) Directory.CreateDirectory("Configurations");
                if (!File.Exists(filePath)) File.WriteAllText(filePath, "");

                File.WriteAllText(filePath, Newtonsoft.Json.JsonConvert.SerializeObject(configuration, Newtonsoft.Json.Formatting.Indented));
            }
            catch { }
        }
    }
}
