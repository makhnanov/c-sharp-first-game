using System.Text;

namespace MyApp
{
    internal class Program
    {
        public enum AttackType
        {
            Damage,
            Self,
            Heal,
        }

        static void Main(string[] args)
        {
            string? name;
            float maxPlayerHealth = 1000f;
            float maxEnemyHealth = 2000f;

            float currentPlayerHealth = maxPlayerHealth;
            float currentEnemyHealth = maxEnemyHealth;

            float playerDamage = 50f;
            float enemyDamage = 55f;

            float fireballDamage = 200f;
            float enemyHealDamage = 100f;
            float playerHealDamage = 100f;

            string? input;

            int commandCount = 3;
            int enemySecondAbilityModifier = 3;

            Random randomizer = new Random();

            Dictionary<AttackType, List<float>> damagesToPlayer = new();
            Dictionary<AttackType, List<float>> damagesToEnemy = new();

            damagesToPlayer[AttackType.Damage] = new List<float>();
            damagesToPlayer[AttackType.Self] = new List<float>();
            damagesToPlayer[AttackType.Heal] = new List<float>();

            damagesToEnemy[AttackType.Damage] = new List<float>();
            damagesToEnemy[AttackType.Self] = new List<float>();
            damagesToEnemy[AttackType.Heal] = new List<float>();

            bool enemyDamageWithWeapon = false;

            Console.Write("Введите ваше имя: ");
            name = Console.ReadLine();

            Console.Clear();
            Console.WriteLine($"Добро пожаловать в игру, {name}\n");

            while (currentPlayerHealth > 0 && currentEnemyHealth > 0)
            {
                Console.Write("Ваше здоровье: ");
                if ((currentPlayerHealth / maxPlayerHealth) > 0.1f)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                Console.WriteLine(currentPlayerHealth);
                Console.ForegroundColor = ConsoleColor.Gray;

                Console.Write("Здоровье противника: ");
                if ((currentEnemyHealth / maxEnemyHealth) > 0.1f)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                Console.WriteLine(currentEnemyHealth);
                Console.ForegroundColor = ConsoleColor.Gray;

                Console.WriteLine($"{name}! Выберите действие:\n" +
                                  $"1. Ударить оружием (урон {playerDamage})\n" +
                                  $"2. Щит: следующая атака противника не наносит урона\n" +
                                  $"3. Огненный шар: наносит урон в размере {fireballDamage}\n" +
                                  $"4. Heal {playerHealDamage}\n");

                input = Console.ReadLine();
                bool playerShield = false;
                bool playerDamageWithWeapon = false;

                switch (input)
                {
                    case "1":
                        playerDamageWithWeapon = true;
                        currentEnemyHealth -= playerDamage;
                        damagesToEnemy[AttackType.Damage].Add(playerDamage);
                        break;
                    case "2":
                        playerShield = true;
                        break;
                    case "3":
                        currentEnemyHealth -= fireballDamage;
                        damagesToEnemy[AttackType.Damage].Add(fireballDamage);
                        break;
                    case "4":
                        if (!enemyDamageWithWeapon)
                        {
                            currentPlayerHealth += playerHealDamage;
                            damagesToEnemy[AttackType.Heal].Add(playerHealDamage);
                            if (currentPlayerHealth > maxPlayerHealth)
                                currentPlayerHealth = maxPlayerHealth;
                        }
                        break;
                    default:
                        Console.WriteLine("Команда не распознана!");
                        break;
                }

                enemyDamageWithWeapon = false;

                int enemyCommand = randomizer.Next(1, commandCount + 1);
                switch (enemyCommand)
                {
                    case 1:
                        enemyDamageWithWeapon = true;
                        if (!playerShield)
                        {
                            currentPlayerHealth -= enemyDamage;
                            damagesToPlayer[AttackType.Damage].Add(enemyDamage);
                        }
                        break;
                    case 2:
                        if (!playerShield)
                        {
                            currentEnemyHealth -= enemyDamage;
                            currentPlayerHealth -= enemyDamage * enemySecondAbilityModifier;
                            damagesToEnemy[AttackType.Self].Add(enemyDamage);
                            damagesToPlayer[AttackType.Damage].Add(enemyDamage * enemySecondAbilityModifier);
                        }
                        break;
                    case 3:
                        if (playerDamageWithWeapon)
                        {
                            currentEnemyHealth -= enemyHealDamage;
                            damagesToEnemy[AttackType.Self].Add(enemyHealDamage);
                        }
                        else
                        {
                            currentEnemyHealth += enemyHealDamage;
                            damagesToPlayer[AttackType.Heal].Add(enemyHealDamage);
                            if (currentEnemyHealth > maxEnemyHealth)
                                currentEnemyHealth = maxEnemyHealth;
                        }
                        break;
                    default:
                        Console.WriteLine("Команда не распознана!");
                        break;
                }

                WriteDamagesFromTo(damagesToEnemy, name, "Enemy");
                WriteDamagesFromTo(damagesToPlayer, "Enemy", name);


                EndTurn();
            }



            void EndTurn()
            {
                Console.WriteLine("\nFor continue press any key");
                Console.ReadKey();
                Console.Clear();
            }

            void DamageMessage(string damager, string defender, float damage)
            {
                Console.Write($"Player {damager} attack {defender} damage ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(damage);
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            void HealMessage(string healer, float healValue)
            {
                Console.Write($"Player {healer} восстановил себе здоровье в размере ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(healValue);
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            void WriteDamageWithType(AttackType attackType, string damagerName, string defenderName, List<float> damages)
            {
                foreach (var damage in damages)
                {
                    switch (attackType)
                    {
                        case AttackType.Damage:
                            DamageMessage(damagerName, defenderName, damage);
                            break;
                        case AttackType.Self:
                            DamageMessage(defenderName, defenderName, damage);
                            break;
                        case AttackType.Heal:
                            HealMessage(damagerName, damage);
                            break;
                        default:
                            break;
                    }
                }

                damages.Clear();
            }

            void WriteDamagesFromTo(Dictionary<AttackType, List<float>> damages, string damagerName, string defenderName)
            {
                foreach (var damage in damages)
                {
                    WriteDamageWithType(damage.Key, damagerName, defenderName, damage.Value);
                }
            }
        }
    }
}
