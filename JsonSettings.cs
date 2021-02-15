using Newtonsoft.Json;
using System.IO;

namespace AutoAbsenSKI
{
    public class JsonSettings
    {
        public string ChromiumPath { get; set; }
        public Viewport Viewport { get; set; } = new Viewport { Width = 1920, Height = 1080 };
        public EmailAccount EmailAccount { get; set; }
        public EmailAccount AtlassianAccount { get; set; }
        public string EmployeeName { get; set; }
        public string[] Recipients { get; set; }

        public static JsonSettings Load(string path)
        {
            if (File.Exists(path))
            {
                var content = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<JsonSettings>(content);
            }
            else
            {
                var settings = new JsonSettings();
                Save(settings, path);
                return settings;
            }            
        }

        public static void Save(JsonSettings settings, string path)
        {
            var content = JsonConvert.SerializeObject(settings);
            File.WriteAllText(path, content);
        }

        public bool IsValidState()
        {
            return Viewport is not null &&
                Viewport.Width > 0 &&
                Viewport.Height > 0 &&

                EmailAccount is not null &&
                !string.IsNullOrWhiteSpace(EmailAccount.Email) &&
                !string.IsNullOrWhiteSpace(EmailAccount.Password) &&
                !string.IsNullOrWhiteSpace(EmailAccount.Host) &&

                AtlassianAccount is not null &&
                !string.IsNullOrWhiteSpace(AtlassianAccount.Email) &&
                !string.IsNullOrWhiteSpace(AtlassianAccount.Password) &&

                !string.IsNullOrWhiteSpace(EmployeeName) &&
                Recipients.Length > 0;
        }
    }
}
