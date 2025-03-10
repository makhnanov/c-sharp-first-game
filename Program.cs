using System.Text;

namespace MyApp
{
    internal static class Program
    {
        
        private const float MaxEnemyHealth = 2000f;
        private const float MaxPlayerHealth = 1000f;
        
        private static string _userName = "";
        
        private static float _enemyHealth = MaxEnemyHealth;
        private static float _playerHealth = MaxPlayerHealth;

        private static EnemyAbility? _lastEnemyAbility;
        private static PlayerAbility? _lastPlayerAbility;

        private enum EnemyAbility
        {
            HandAttack,
            MagicAttack,
            TryToHeal,
        }
        
        private enum PlayerAbility
        {
            AttackWithSword,
            AttackWithFireball,
            SaveWithShield,
            HealWithFlask,
        }
        
        private static readonly List<string> Logs = [];
            
        private static readonly Dictionary<ConsoleKey, PlayerAbility> PlayerControllerSettings = new()
        {
            { ConsoleKey.UpArrow, PlayerAbility.AttackWithFireball },
            { ConsoleKey.DownArrow, PlayerAbility.HealWithFlask },
            { ConsoleKey.LeftArrow, PlayerAbility.SaveWithShield },
            { ConsoleKey.RightArrow, PlayerAbility.AttackWithSword },
        };
        
        private const float EnemySimpleAttack = 55f;
        private const float EnemyCriticalMultiplier = 3;
        
        private static readonly Dictionary<Enum, float> HpCost = new()
        {
            { EnemyAbility.HandAttack, EnemySimpleAttack },
            { EnemyAbility.MagicAttack, EnemySimpleAttack * EnemyCriticalMultiplier },
            { EnemyAbility.TryToHeal, 100f },
            
            { PlayerAbility.AttackWithSword, 50f },
            { PlayerAbility.AttackWithFireball, 200f },
            { PlayerAbility.HealWithFlask, 100f },
        };

        private static readonly ConsoleKey[] AllowedInputKeys =
        [
            ConsoleKey.LeftArrow,
            ConsoleKey.UpArrow,
            ConsoleKey.DownArrow,
            ConsoleKey.RightArrow
        ];
        
        private static void Main()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.OutputEncoding = Encoding.UTF8;

            GetPlayerName();
            YouAreWelcome();
            ControllerDescription();

            DrawIdle();
            RenderSmallControls();
            
            do
            {
                PlayerAction();

                if (SomebodyDies())
                    break;
                
                EnemyAction();
                
                DrawIdle();
                RenderSmallControls(_lastEnemyAbility == EnemyAbility.MagicAttack);

            } while (!SomebodyDies());
            
            Console.Clear();
            DrawIdle();

            FightResult();
        }

        private static void RenderLogs()
        {
            if (Logs.Count > 3)
                Console.WriteLine(Logs[^4]);
            if (Logs.Count > 2)
                Console.WriteLine(Logs[^3]);
            if (Logs.Count > 1)
                Console.WriteLine(Logs[^2]);
            if (Logs.Count > 0)
                Console.WriteLine(Logs[^1]);
        }

        private static void DrawIdle()
        {
            DrawAction(true, 0);
            Console.WriteLine();
        }

        private static void GetPlayerName()
        {
            Console.WriteLine("Hello Gamer! What is your name? Please, enter: ");
            string? userNameInput = Console.ReadLine();
            if (userNameInput == null)
            {
                _userName = "Anonymous";
            }
            else
            {
                userNameInput = userNameInput.Trim();
                _userName = string.Empty == userNameInput ? "Anonymous" : userNameInput;
            }
            Console.Clear();
        }
        
        private static void YouAreWelcome()
        {
            Console.WriteLine($"You Are Welcome, {_userName}! Now Need to Kill Big Black Dragon!");
            Console.WriteLine("Press any key to continue...");
            Console.WriteLine();
            Console.ReadKey();
            Console.Clear();
        }
        
        private static void ControllerDescription()
        {
            Console.WriteLine("Controls is:");
            Console.WriteLine();
            RenderControls(true);
            Console.WriteLine();
            Console.WriteLine("Press any key to START FIGHT!!!");
            Console.ReadKey();
            Console.Clear();
        }

        private static void RenderSmallControls(bool withFlask = false)
        {
            string text = "\u2190 - Block: No Damage  |  \u2191 - Cast FireBall: Enemy -200 HP  |  \u2192 - Attack with Sword: Enemy -50 HP";
            if (withFlask)
            {
                text += "\n\u2193 - Use Flask: +100 HP";
            }
            Console.WriteLine(text);
        }

        private static void RenderControls(bool withFlask = false)
        {
            string controlsText = GetControls();

            using StringReader controlsReader = new StringReader(controlsText);
            string? controlsLine = controlsReader.ReadLine();
            while (controlsLine != null)
            {
                if (!withFlask)
                {
                    controlsLine = controlsLine[137..];
                }
                Console.WriteLine(controlsLine);
                controlsLine = controlsReader.ReadLine();
            }
        }

        private static void FightResult()
        {
            if (_playerHealth <= 0 && _enemyHealth <= 0)
            {
                Console.WriteLine("=======================================================");
                Console.WriteLine("=     You kill Dragon but you also die! You lose!     =");
                Console.WriteLine("=======================================================");
            }
            else if (_playerHealth <= 0)
            {
                Console.WriteLine("=============================");
                Console.WriteLine("=         You lose!         =");
                Console.WriteLine("=============================");
            }
            else if (_enemyHealth <= 0)
            {
                Console.WriteLine("=============================");
                Console.WriteLine("= You won! Congratulations! =");
                Console.WriteLine("=============================");
            }

            int i = 10;
            while (i != 0)
            {
                Console.WriteLine($"Press any key {i} times to exit...");
                Console.ReadKey();
                i--;
            }
        }

        private static bool SomebodyDies()
        {
            return _playerHealth < 0 || _enemyHealth < 0;
        }

        private static void EnemyAction()
        {
            _lastEnemyAbility = GetRandomEnum<EnemyAbility>();

            bool skipAnimation = _lastEnemyAbility == EnemyAbility.TryToHeal;

            if (!skipAnimation)
            {
                for (int i = 0; i < 50; i++)
                {
                    DrawAction(false, i);
                    Thread.Sleep(20);
                    Console.Clear();
                }
            }
            
            switch (_lastEnemyAbility)
            {
                case EnemyAbility.HandAttack:
                    if (_lastPlayerAbility == PlayerAbility.SaveWithShield)
                    {
                        AddLog($"Dragon try to attack {_userName} via {_lastEnemyAbility.Value}, but shield block damage! Haha.");
                    }
                    else
                    {
                        AddLog($"Dragon attack {_userName} via {_lastEnemyAbility.Value}: {_playerHealth} - \x1b[31m{HpCost[_lastEnemyAbility]}\x1b[97m = {_playerHealth - HpCost[_lastEnemyAbility]}");
                        _playerHealth -= HpCost[_lastEnemyAbility];
                    }
                    break;
                case EnemyAbility.MagicAttack:
                    if (_lastPlayerAbility == PlayerAbility.SaveWithShield)
                    {
                        AddLog($"Dragon try to attack {_userName} via {_lastEnemyAbility.Value}, but shield block damage! XeXe.");
                    }
                    else
                    {
                        AddLog($"Dragon attack himself via {_lastEnemyAbility.Value}: {_enemyHealth} - \x1b[31m{HpCost[EnemyAbility.HandAttack]}\x1b[97m = {_enemyHealth - HpCost[EnemyAbility.HandAttack]}");
                        _enemyHealth -= HpCost[EnemyAbility.HandAttack];
                        AddLog($"Dragon attack {_userName} via {_lastEnemyAbility.Value}: {_playerHealth} - \x1b[31m{HpCost[_lastEnemyAbility]}\x1b[97m = {_playerHealth - HpCost[_lastEnemyAbility]}");
                        _playerHealth -= HpCost[_lastEnemyAbility];
                    }
                    break;
                
                case EnemyAbility.TryToHeal:
                    if (_lastPlayerAbility == PlayerAbility.AttackWithSword)
                    {
                        AddLog($"The Dragon {_lastEnemyAbility.Value}, but he is bleeding because {_userName} {_lastPlayerAbility.Value}: {_enemyHealth} - \x1b[31m{HpCost[_lastEnemyAbility]}\x1b[97m = {_enemyHealth - HpCost[_lastEnemyAbility]}");
                        _enemyHealth -= HpCost[_lastEnemyAbility];
                    }
                    else
                    {
                        AddLog(_enemyHealth + HpCost[_lastEnemyAbility] > MaxEnemyHealth
                            ? $"The Dragon {_lastEnemyAbility.Value} and he heal himself: {_enemyHealth} + {MaxEnemyHealth - _enemyHealth} = {MaxEnemyHealth}"
                            : $"The Dragon {_lastEnemyAbility.Value} and he heal himself: {_enemyHealth} + {HpCost[_lastEnemyAbility]} = {_enemyHealth + HpCost[_lastEnemyAbility]}");
                        _enemyHealth += HpCost[_lastEnemyAbility];
                        if (_enemyHealth > MaxEnemyHealth)
                            _enemyHealth = MaxEnemyHealth;
                    }
                    break;
                default:
                    Console.WriteLine("Hacker Detected! You will be detected by IP!");
                    throw new Exception("I'l find you!");
            }

            if (!skipAnimation)
            {
                for (int i = 50; i > 0; i--)
                {
                    DrawAction(false, i);
                    Thread.Sleep(20);
                    Console.Clear();
                }
            }
            Console.Clear();
        }
        
        static T GetRandomEnum<T>() where T : Enum
        {
            Random random = new Random();
            Array values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(random.Next(values.Length))!;
        }

        private static void PlayerAction()
        {
            ConsoleKey key = GetPlayerInputKey();
            Console.Clear();

            _lastPlayerAbility = PlayerControllerSettings[key];

            bool skipAttackAnimation = _lastPlayerAbility != PlayerAbility.SaveWithShield
                                       && _lastPlayerAbility != PlayerAbility.HealWithFlask;
            if (skipAttackAnimation)
            {
                for (int i = 0; i < 50; i++)
                {
                    DrawAction(true, i);
                    Thread.Sleep(20);
                    Console.Clear();
                }
            }

            switch (_lastPlayerAbility)
            {
                case PlayerAbility.AttackWithSword:
                case PlayerAbility.AttackWithFireball:
                    AddLog($"{_userName} attacks Dragon with {_lastPlayerAbility.Value}: {_enemyHealth} - \x1b[31m{HpCost[_lastPlayerAbility]}\x1b[97m = {_enemyHealth - HpCost[_lastPlayerAbility]}");
                    _enemyHealth -= HpCost[_lastPlayerAbility];
                    break;
                case PlayerAbility.HealWithFlask:
                    AddLog(_playerHealth + HpCost[_lastPlayerAbility] > MaxPlayerHealth
                        ? $"{_userName} heal drink {_lastPlayerAbility.Value}: {_playerHealth} + {MaxPlayerHealth - _playerHealth} = {MaxPlayerHealth}"
                        : $"{_userName} heal drink {_lastPlayerAbility.Value}: {_playerHealth} + {HpCost[_lastPlayerAbility]} = {_playerHealth + HpCost[_lastPlayerAbility]}");
                    _playerHealth += HpCost[_lastPlayerAbility];
                    if (_playerHealth > MaxPlayerHealth)
                    {
                        _playerHealth = MaxPlayerHealth;
                    }
                    break;
                case PlayerAbility.SaveWithShield:
                    AddLog($"{_userName} block any damage!");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_lastPlayerAbility), "PlayerAction() detection by key error!");
            }

            if (skipAttackAnimation)
            {
                for (int i = 50; i > 0; i--)
                {
                    DrawAction(true, i);
                    Thread.Sleep(20);
                    Console.Clear();
                }
            }
            Console.Clear();
        }

        private static void AddLog(string logString)
        {
            DateTime currentTime = DateTime.Now;
            Logs.Add("[" + currentTime.ToString("yyyy-MM-dd HH:mm:ss") + "] " + logString);
        }

        private static ConsoleKey GetPlayerInputKey()
        {
            ConsoleKey userInputKey = Console.ReadKey().Key;
            if (!AllowedInputKeys.Contains(userInputKey))
            {
                Console.WriteLine("Wrong key! Use arrows! Try again!");
                return GetPlayerInputKey();
            }

            if (
                PlayerControllerSettings[userInputKey] == PlayerAbility.HealWithFlask
                && _lastEnemyAbility != EnemyAbility.MagicAttack
                )
            {
                Console.WriteLine("Flask can be used if Enemy attack you with magic skill!");
                Console.WriteLine("Use another key! Try again!");
                return GetPlayerInputKey();
            }
            
            return userInputKey;
        }
        
        private static void DrawAction(bool isPlayer, int step)
        {
            const int maxPlayerLength = 80;
            string player = GetPlayer();
            string enemy = GetDragon();
            
            using (StringReader playerReader = new StringReader(player))
            using (StringReader enemyReader = new StringReader(enemy))
            {
                int playerGap = 6;
                int currentGap = 0;
                int currentLine = 0;
                bool isEnemy = !isPlayer;
                string? enemyLine = enemyReader.ReadLine();

                while (enemyLine != null)
                {
                    var playerLine = "";
                    if (currentGap < playerGap)
                    {
                        currentGap++;
                    }
                    else
                    {
                        playerLine = playerReader.ReadLine() ?? "";
                    }

                    playerLine += new string(' ', maxPlayerLength - playerLine.Length);

                    if (isPlayer)
                    {
                        playerLine = new string(' ', step) + playerLine;
                    }
                    
                    if (playerLine.Length > maxPlayerLength)
                    {
                        int diffLength = playerLine.Length - maxPlayerLength;
                        
                        string lastSymbols = playerLine.Substring(playerLine.Length - diffLength);
                        for (int i = 0; i < lastSymbols.Length; i++)
                        {
                            if (lastSymbols[i] != ' ')
                            {
                                char[] chars = enemyLine.ToCharArray();
                                chars[i] = ' ';
                                enemyLine = new string(chars);
                            }
                        }
                        playerLine = playerLine.Substring(0, playerLine.Length - diffLength);
                    }
                    
                    playerLine = playerLine.Substring(0, maxPlayerLength);
                    
                    if (isEnemy)
                    {
                        playerLine = playerLine.Remove(playerLine.Length - step);
                    }

                    enemyLine = enemyReader.ReadLine();
                    if (enemyLine == null)
                    {
                        break;
                    }

                    if (currentLine == 1)
                    {
                        char[] playerHealthChars = $"{_playerHealth}".ToCharArray();
                        char[] playerChars = playerLine.ToCharArray();
                        playerChars[1] = playerHealthChars[0];
                        playerChars[2] = playerHealthChars.Length > 1 ? playerHealthChars[1] : ' ';
                        playerChars[3] = playerHealthChars.Length > 2 ? playerHealthChars[2] : ' ';
                        playerChars[4] = playerHealthChars.Length > 3 ? playerHealthChars[3] : ' ';
                        // playerChars[5,6,7] = ' / ';
                        char[] playerMaxHealthChars = $"{MaxPlayerHealth}".ToCharArray();
                        playerChars[8] = playerMaxHealthChars[0];
                        playerChars[9] = playerMaxHealthChars.Length > 1 ? playerMaxHealthChars[1] : ' ';
                        playerChars[10] = playerMaxHealthChars.Length > 2 ? playerMaxHealthChars[2] : ' ';
                        playerChars[11] = playerMaxHealthChars.Length > 3 ? playerMaxHealthChars[3] : ' ';
                        playerLine = new string(playerChars);

                        const string colorGreen = "\x1b[32m";
                        const string colorRed = "\x1b[31m";
                        playerLine = (_playerHealth > MaxPlayerHealth / 10
                                         ? colorGreen
                                         : colorRed)
                                     + playerLine.Substring(0, 5)
                                     + "\x1b[97m / " + playerLine.Substring(7);
                        
                        char[] enemyHealthChars = $"{_enemyHealth}".ToCharArray();

                        char[] enemyChars = enemyLine.ToCharArray();
                        enemyChars[41] = enemyHealthChars[0];
                        enemyChars[42] = enemyHealthChars.Length > 1 ? enemyHealthChars[1] : ' ';
                        enemyChars[43] = enemyHealthChars.Length > 2 ? enemyHealthChars[2] : ' ';
                        enemyChars[44] = enemyHealthChars.Length > 3 ? enemyHealthChars[3] : ' ';
                        
                        enemyChars[45] = ' ';
                        enemyChars[46] = '/';
                        enemyChars[47] = ' ';
                        
                        char[] enemyMaxHealthChars = $"{MaxEnemyHealth}".ToCharArray();
                        enemyChars[48] = enemyMaxHealthChars[0];
                        enemyChars[49] = enemyMaxHealthChars.Length > 1 ? enemyMaxHealthChars[1] : ' ';
                        enemyChars[50] = enemyMaxHealthChars.Length > 2 ? enemyMaxHealthChars[2] : ' ';
                        enemyChars[51] = enemyMaxHealthChars.Length > 3 ? enemyMaxHealthChars[3] : ' ';
                        
                        enemyLine = new string(enemyChars);

                        enemyLine = enemyLine.Substring(0, 40)
                                    + (_enemyHealth > MaxEnemyHealth / 10
                                        ? colorGreen
                                        : colorRed)
                                    + enemyLine.Substring(40, 5)
                                    + "\x1b[97m"
                                    + enemyLine.Substring(45);
                    }

                    if (currentLine == 2)
                    {
                        Console.Write("[");
                        Console.BackgroundColor = ConsoleColor.Green;
                        int current = (int)(28 * _playerHealth / MaxPlayerHealth);
                        for (int i = 0; i < 29; i++)
                        {
                            if (i <= current)
                            {
                                Console.Write(" ");
                            }
                            else
                            {
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.Write(" ");
                            }
                        }
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.Write("]");
                        Console.Write(playerLine.Substring(30, playerLine.Length - 30));


                        Console.Write(enemyLine.Substring(0, 40));
                        Console.Write("[");
                        Console.BackgroundColor = ConsoleColor.DarkRed;
                        
                        current = (int)(28 * _enemyHealth / MaxEnemyHealth);
                        for (int i = 0; i < 29; i++)
                        {
                            if (i <= current)
                            {
                                Console.Write(" ");
                            }
                            else
                            {
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.Write(" ");
                            }
                        }
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.Write("]");
                        Console.WriteLine(enemyLine.Substring(41 + 30));
                        
                        ++currentLine;
                        continue;
                    }
                    
                    
                    if (currentLine == 4)
                    {
                        char[] playerChars = playerLine.ToCharArray();
                        char[] userNameChars = _userName.ToCharArray();
                        const int gapFromLeft = 10;
                        for (int i = 0; i < 11; i++)
                        {
                            if (i < userNameChars.Length)
                            {
                                playerChars[gapFromLeft + i] = userNameChars[i];
                            }
                            else
                            {
                                playerChars[gapFromLeft + i] = ' ';
                            }
                        }
                        playerLine = new string(playerChars);
                    }
                    
                    Console.Write(playerLine);
                    Console.WriteLine(enemyLine);
                    
                    ++currentLine;
                }
            }
                
            RenderLogs();
        }

        private static string GetPlayer()
        {
            return "            *##%+                      \n           **###**                     \n      *+=*#%#%%##*                     \n    #%%%%%%%###%*                      \n *#%%%%%%%%%%%%                        \n+#%%%%%%%%%%%%%#                       \n+%%#%%#  %%%%%%%*%                     \n+###%%%%%%%%%%%%+%#              +*#%#+\n #%%%%%%%%%%%%%#**@#-      +*#%#%%*    \n  #%%%%%%%%%%%%%%%*%%  *#%%#**         \n   +#%%%%#%%@@%@@@###*##+              \n      ==*@@@%%%@@%*#                   \n      =+%@@@%%@@@#                     \n      ++#@@@ %@@@#                     \n      =+@@%  *@@#                      \n      -%@#   %@%                       \n      #@#    %@*                       \n      %@+    %%                        \n     +%#     %%                        \n    +%@      ##*+=                     \n  +###%%    %%####                            ";
        }
        
        private static string GetDragon()
        {
            return "           -==\\\\               /==-                                       \n-===\\\\        |||            //                                           \n     \\\\    -+*=-=-:::      ///                                            \n      \\\\-:+*###%%###******-/                                              \n        =*%#%%%%%%%%##%###+                                               \n     =+--#####%%%%%%%%###*=-:                                             \n    :==-*   #%####%#%%%%%%#*=                -:*#***#####****-            \n   :*+:+   ####%%%%%##*#%%%%#+          +*####%%%%%%%%%%%%%%%#*=:         \n  .:#*+###**####*#%#%%%+#%%##:       =+#####%###%%%%####%%%%###%%#+       \n  *###########*=.:+*#%%#*%%##*    - ##%%%%%%%##**+++*#####%**##%%%#+      \n  =**+#%%####+     +#%%#%%%#*=:. +##%%%%#######**+*##***###%##+=*+*###:   \n  :##*#%%##=      +##%#*#%%#=   =#%#%%#***###+             ##*##***##%+   \n  :#####*.      +####*##%#%*:  =##%%#*###**###=                 .+*+*#%*. \n  ##+-+:      --=+##*#%%%+*:  .*%%%##%%#*=-+***-                 .+**#%#: \n =##-#+      ***+=+%%%###     =*#####%#+                           -*###*:\n  |        -+=+++*%%###+:    -#%%##*%#-                            =*#%*  \n  |       :+=*-*%##%%*#      :#%#**#%*                               ++%# \n  |      :+==-*%%#%%*:      :*#%####%+                               =+#*.\n /       ++:+*%%##%#+       -#%%####%=                               .=#+ \n        :*+=-#%####*+:      :#%%%*#%%+                               +##- \n        .+=.-%%##%#:       -**#%%##%%*                               +#=  \n        -+=:=%%###*+        =#%%%####*                              -#*   \n        =.-#%%###=:   -+***#%%%####*                              *=.     \n        ==++*%##%#+++####%%%%#####%:                             *:       \n         +=+*#%%####%%%%%%%###%%%##                            -#:        \n           +-*##%%%%%%%%%%%%%%###+:                        +*##*          \n            ++**###%%%%%%#####*=+                       -+#%%%%+          \n             -++*###%%%###*#*+-                       +#%%%%%%#+          \n                 .*######*-                          *#%%%%%#=            \n                   *#####                          %%%%%#%*-              \n                    ***#                           *****                  \n                     -*                                                   ";
        }

        private static string GetControls()
        {
            return "      .*%-.                                      |           #.                                |         .#                                |      .=+++++:                                  \n    .-####+.                                     |        .:+#.                                |         .##.                              |      .*####*:                                  \n   :+######*:                                    |     .:*%#*#.                                |         .####*:.                          |      .*####*.         Use Flask: You + 100 HP  \n .-##########-.                                  |  .:=%####%%%@@%%%#%#%=                      | #########%%####%+:.   Attack with Sword:  |       +####*.                                  \n.:###=####=###:.  Cast FireBall: Enemy - 200 HP  | .=%##################=    Block: No Damage  | #################%#%                      |  .-############-.                              \n    .+####+                                      |   .:*####*#%%%%######=                      | %%%%%%#*=#+%##%*:.    Enemy - 50 HP       |   .-##########-.         Only If Previous      \n    .*#####.                                     |      .=###.                                 |         .#+%##.                           |     :*######*:             Enemy Attack        \n    -*####*.                                     |        .:#.                                 |         .###                              |      .+####=.               Was Magic          \n    :=++++=:                                     |         #.                                  |         .#                                |       .-%*.                                                                                                          ";
        }
    }
}
