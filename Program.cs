using System.Diagnostics;

namespace far
{
    internal static class Program
    {
        private static readonly Config _cfg = Config.Load();

        internal static void Main(string[] args)
        {
            PromptAction();

            while (true)
            {
                var command = Console.ReadKey().Key;
                Console.WriteLine();
                Console.WriteLine();

                switch (command)
                {
                    case ConsoleKey.NumPad1:
                    case ConsoleKey.D1:
                        Add();
                        break;

                    case ConsoleKey.NumPad2:
                    case ConsoleKey.D2:
                        Connect();
                        break;

                    case ConsoleKey.NumPad3:
                    case ConsoleKey.D3:
                        UpdatePassword();
                        break;

                    case ConsoleKey.NumPad4:
                    case ConsoleKey.D4:
                        Reconnect();
                        break;
                }
                Console.WriteLine();
                PromptAction();
            }
        }

        private static void UpdatePassword()
        {
            Console.WriteLine("Select password to update:");

            var users = _cfg.GetUsers();
            var selected = GetSelectedUser(users);

            if (selected < 0 || selected >= users.Count)
            {
                Console.WriteLine("No user selected!");
            }
            else
            {
                var user = users[selected];
                Console.Write($"Enter new credential for {user}: ");
                var pass = Console.ReadLine();

                foreach (var host in _cfg.GetHostsForUser(user))
                {
                    AddCredentials(host, user, pass);
                }
            }
            Console.WriteLine();
        }

        private static int GetSelectedUser(List<string> users)
        {
            var counter = 1;
            foreach (var user in users)
            {
                Console.WriteLine($"{counter++}. {user}");
            }

            while (true)
            {
                if (int.TryParse(Console.ReadKey().KeyChar.ToString(), out var selectedUser))
                {
                    Console.WriteLine();
                    return selectedUser - 1;
                }
                else
                {
                    Console.WriteLine("Invalid option selected.");
                }
            }
        }

        private static void Add()
        {
            Console.Clear();
            Console.Write("Enter the machine you want to add (for example machine.k2test.net): ");
            var target = Console.ReadLine();

            Console.Write(@"Enter the username (for example k2test\user.name): ");
            var user = Console.ReadLine();

            Console.Write($"Enter the password for {user}: ");
            var pass = Console.ReadLine();

            AddCredentials(target, user, pass);
            _cfg.AddHost(target, user);
            _cfg.Save();
        }

        private static void AddCredentials(string target, string user, string pass)
        {
            Console.WriteLine($"Add credentials for {target}.");
            RemoveCredentials(target);

            var proc = new Process();
            proc.StartInfo.FileName = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\system32\cmdkey.exe");
            proc.StartInfo.Arguments = $@"/generic:TERMSRV/{target} /user:{user} /pass:{pass}";
            proc.Start();
            proc.WaitForExit();
        }

        private static void Connect()
        {
            Console.Clear();
            if (_cfg.Hosts.Count == 0)
            {
                Console.WriteLine("No hosts found, add new host.");
            }
            else
            {
                int selectedHost;

                do
                {
                    GetHost(out selectedHost);
                }
                while (selectedHost <= 0 || selectedHost > _cfg.Hosts.Count);

                StartRemoteSession(_cfg.GetHost(selectedHost - 1));
            }
        }

        private static void GetHost(out int selectedHost)
        {
            var counter = 1;
            Console.WriteLine("Select a host and press Enter:");

            foreach (var host in _cfg.Hosts)
            {
                Console.WriteLine($"{counter++}. {host.Key}");
            }
            Console.WriteLine();

            if (_cfg.Hosts.Count > 9)
            {
                Console.Write("Select a host and press enter: ");
                if (!int.TryParse(Console.ReadLine(), out selectedHost))
                {
                    Console.WriteLine();
                    Console.WriteLine("Invalid option selected.");
                }
            }
            else
            {
                Console.Write("Select a host: ");
                if (!int.TryParse(Console.ReadKey().KeyChar.ToString(), out selectedHost))
                {
                    Console.WriteLine();
                    Console.WriteLine("Invalid option selected.");
                }
            }

            Console.WriteLine();
        }

        private static void PromptAction()
        {
            Console.Clear();
            Console.WriteLine("What do you want to do?");
            Console.WriteLine("1. Add Environment");

            if (_cfg.Hosts.Count > 0)
            {
                Console.WriteLine("2. Connect");
                Console.WriteLine("3. Update Credentials");
            }

            if (!string.IsNullOrEmpty(_cfg.LastHost))
            {
                Console.WriteLine($"4. Reconnect to {_cfg.LastHost}");
            }


            Console.WriteLine();
        }

        private static void Reconnect()
        {
            StartRemoteSession(_cfg.LastHost);
        }

        private static void RemoveCredentials(string target)
        {
            Console.WriteLine($"Remove any existing credentials for {target}.");
            var proc = new Process();
            proc.StartInfo.FileName = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\system32\cmdkey.exe");
            proc.StartInfo.Arguments = $"/delete:TERMSRV/{target}";
            proc.Start();
            proc.WaitForExit();
        }

        private static void StartRemoteSession(string target)
        {
            Console.WriteLine("Connecting....");

            var process2 = new Process();
            process2.StartInfo.FileName = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\system32\mstsc.exe");
            process2.StartInfo.Arguments = $"/v {target}";
            process2.Start();

            Console.WriteLine();
            Console.WriteLine("CONNECTED");

            _cfg.LastHost = target;
            _cfg.Save();

            Thread.Sleep(500);

            Console.Clear();
        }
    }
}