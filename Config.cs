using System.Text.Json;

namespace far
{
    public class Config
    {
        private const string CONFIG_FILE = @"%LOCALAPPDATA%\far\far.json";
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public Dictionary<string, string> Hosts { get; set; } = new();

        public string LastHost { get; set; }

        public static Config Load()
        {
            if (File.Exists(Environment.ExpandEnvironmentVariables(CONFIG_FILE)))
            {
                return JsonSerializer.Deserialize<Config>(File.ReadAllText(Environment.ExpandEnvironmentVariables(CONFIG_FILE)));
            }

            return new Config();
        }

        public void Save()
        {
            File.WriteAllText(Environment.ExpandEnvironmentVariables(CONFIG_FILE), JsonSerializer.Serialize(this, _options));
            Console.WriteLine("Config saved.");
        }

        internal void AddHost(string target, string user)
        {
            Hosts.Add(target, user);
        }

        internal string GetHost(int index)
        {
            return Hosts.Keys.ToArray()[index];
        }

        internal List<string> GetHostsForUser(string user)
        {
            return Hosts.Where(h => h.Value.Equals(user,StringComparison.InvariantCultureIgnoreCase))
                        .Select(h => h.Key)
                        .ToList();
        }

        internal List<string> GetUsers()
        {
            return Hosts.Values.Distinct().ToList();
        }
    }
}