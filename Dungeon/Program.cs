using System;
using System.Collections.Generic;

public class GameMap
{
    private int size;
    internal char[][] map;
    internal bool[,] visitedCells; // Добавлено поле для отслеживания посещенных ячеек

    public GameMap(int size)
    {
        this.size = size;
        this.map = InitializeMap();
        this.visitedCells = new bool[size, size]; // Инициализация массива посещенных ячеек
        CreatePathOnMap();
        ReplaceZeroIndexes();
    }

    private char[][] InitializeMap()
    {
        char[][] newMap = new char[size][];
        for (int i = 0; i < size; i++)
        {
            newMap[i] = new char[size];
            for (int j = 0; j < size; j++)
            {
                newMap[i][j] = '0'; // ASCII код блокированного символа
            }
        }
        return newMap;
    }

    private void CreatePathOnMap()
    {
        int painterX = size / 2;
        int painterY = size / 2;
        int steps = 0;
        Random random = new Random();

        while (steps < Program.PAINTER_STEPS_AMOUNT)
        {
            int direction = random.Next(0, 4);
            switch (direction)
            {
                case 0 when painterX + 1 < size:
                    painterX += 1;
                    break;
                case 1 when painterX - 1 >= 0:
                    painterX -= 1;
                    break;
                case 2 when painterY + 1 < size:
                    painterY += 1;
                    break;
                case 3 when painterY - 1 >= 0:
                    painterY -= 1;
                    break;
            }

            if (map[painterX][painterY] != Program.PATH_SYMBOL)
            {
                map[painterX][painterY] = Program.PATH_SYMBOL;
                visitedCells[painterX, painterY] = true; // Помечаем ячейку как посещенную
                steps += 1;
            }
        }
    }

    private void ReplaceZeroIndexes()
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (map[i][j] == '0')
                {
                    map[i][j] = Program.BLOCKED_SYMBOL;
                }
            }
        }
    }

    public Tuple<int, int> PlaceObject(char symbol)
    {
        int attempts = 0;
        Random random = new Random();

        while (attempts < 1000)
        {
            int x = random.Next(0, size);
            int y = random.Next(0, size);
            if (map[x][y] == Program.PATH_SYMBOL)
            {
                map[x][y] = symbol;
                return new Tuple<int, int>(x, y);
            }
            attempts++;
        }
        return null;
    }

    public void PrintMap(Tuple<int, int> playerPos, int level, int souls, int health, int healthPotions)
    {
        List<string> mapLines = new List<string>();
        for (int i = 0; i < size; i++)
        {
            string line = "";
            for (int j = 0; j < size; j++)
            {
                line += new Tuple<int, int>(i, j).Equals(playerPos) ? Program.PLAYER_SYMBOL : map[i][j];
            }
            mapLines.Add(line);
        }

        string[] levelInfo = {
            $"Уровень: {level}",
            $"Души: {souls}",
            $"Здоровье: {health}",
            $"Зелья здоровья: {healthPotions}"
        };

        for (int i = 0; i < mapLines.Count; i++)
        {
            Console.WriteLine(mapLines[i] + (i < levelInfo.Length ? " | " + levelInfo[i] : ""));
        }
    }
}

public class Player
{
    internal Tuple<int, int> position;
    internal int health;
    internal int healthPotions;

    public Player(Tuple<int, int> startPos, int health, int healthPotions)
    {
        this.position = startPos;
        this.health = health;
        this.healthPotions = healthPotions;
    }

    public void Move(string direction, GameMap gameMap)
    {
        int x = position.Item1;
        int y = position.Item2;
        switch (direction)
        {
            case "w" when x - 1 >= 0 && gameMap.map[x - 1][y] != Program.BLOCKED_SYMBOL:
                x -= 1;
                break;
            case "s" when x + 1 < Program.MAP_SIZE && gameMap.map[x + 1][y] != Program.BLOCKED_SYMBOL:
                x += 1;
                break;
            case "a" when y - 1 >= 0 && gameMap.map[x][y - 1] != Program.BLOCKED_SYMBOL:
                y -= 1;
                break;
            case "d" when y + 1 < Program.MAP_SIZE && gameMap.map[x][y + 1] != Program.BLOCKED_SYMBOL:
                y += 1;
                break;
        }
        position = new Tuple<int, int>(x, y);
        gameMap.visitedCells[x, y] = true; // Помечаем ячейку как посещенную
    }

    public void UseHealthPotion()
    {
        if (healthPotions > 0)
        {
            health += 1;
            healthPotions -= 1;
        }
    }
}

public class Game
{
    private int level;
    private int souls;
    private GameMap gameMap;
    private Player player;
    private Tuple<int, int> npcPosition;
    private List<Tuple<int, int>> enemies;
    private List<Tuple<int, int>> healthPotionsPositions;
    private List<Tuple<int, int>> soulPositions;
    private Tuple<int, int> nextLevelPosition;

    public Game(int level = 1, int souls = 0, int healthPotions = 0, int health = Program.INITIAL_HEALTH)
    {
        this.level = level;
        this.souls = souls;
        gameMap = new GameMap(Program.MAP_SIZE);
        player = new Player(new Tuple<int, int>(Program.MAP_SIZE / 2, Program.MAP_SIZE / 2), health, healthPotions);
        npcPosition = gameMap.PlaceObject(Program.NPC_SYMBOL);
        enemies = PlaceObjects(Program.ENEMY_COUNT, Program.ENEMY_SYMBOL);
        healthPotionsPositions = PlaceObjects(Program.HEALTH_POTION_COUNT, Program.HEALTH_POTION_SYMBOL);
        soulPositions = new List<Tuple<int, int>>();
        nextLevelPosition = null;
    }

    private void ClearConsole()
    {
        Console.Clear();
    }

    private List<Tuple<int, int>> PlaceObjects(int count, char symbol)
    {
        List<Tuple<int, int>> positions = new List<Tuple<int, int>>();
        for (int i = 0; i < count; i++)
        {
            positions.Add(gameMap.PlaceObject(symbol));
        }
        return positions;
    }

    private bool PlayRockPaperScissors()
    {
        int playerScore = 0;
        int enemyScore = 0;
        Dictionary<int, string> moveMapping = new Dictionary<int, string>
        {
            { 1, "Камень" },
            { 2, "Ножницы" },
            { 3, "Бумага" }
        };
        Random random = new Random();

        while (playerScore < 3 && enemyScore < 3)
        {
            Console.Write(Program.TEXT_CHOOSE_MOVE);
            if (!int.TryParse(Console.ReadLine(), out int playerMoveNum) || !moveMapping.ContainsKey(playerMoveNum))
            {
                Console.WriteLine(Program.TEXT_INVALID_INPUT);
                continue;
            }
            string playerMove = moveMapping[playerMoveNum];
            string enemyMove = moveMapping[random.Next(1, 4)];

            Console.WriteLine($"{Program.TEXT_OPPONENT_CHOICE} {enemyMove}");

            if (playerMove == enemyMove)
            {
                Console.WriteLine(Program.TEXT_DRAW);
            }
            else if ((playerMove == "Камень" && enemyMove == "Ножницы") ||
                     (playerMove == "Ножницы" && enemyMove == "Бумага") ||
                     (playerMove == "Бумага" && enemyMove == "Камень"))
            {
                Console.WriteLine(Program.TEXT_ROUND_WIN);
                playerScore++;
            }
            else
            {
                Console.WriteLine(Program.TEXT_ROUND_LOSS);
                enemyScore++;
            }

            Console.WriteLine(Program.TEXT_SCORE.Replace("{player_score}", playerScore.ToString()).Replace("{enemy_score}", enemyScore.ToString()));
        }

        if (playerScore == 3)
        {
            Console.WriteLine(Program.TEXT_GAME_WIN);
            Console.ReadLine();
            return true;
        }
        else
        {
            Console.WriteLine(Program.TEXT_GAME_LOSS);
            Console.ReadLine();
            return false;
        }
    }

    public void PlayGame()
    {
        while (true)
        {
            ClearConsole();
            gameMap.PrintMap(player.position, level, souls, player.health, player.healthPotions);

            Console.Write(Program.TEXT_CHOOSE_ACTION);
            string action = Console.ReadLine();
            switch (action)
            {
                case "w":
                case "a":
                case "s":
                case "d":
                    player.Move(action, gameMap);
                    break;
                case "h":
                    player.UseHealthPotion();
                    break;
                case "q":
                    Console.WriteLine(Program.TEXT_GAME_OVER);
                    return;
                default:
                    Console.WriteLine(Program.TEXT_INVALID_INPUT);
                    break;
            }

            if (player.position.Equals(npcPosition))
            {
                Console.WriteLine(Program.TEXT_MET_NPC);
                Console.ReadLine();
            }

            // Пример: встретили врага
            foreach (var enemy in enemies)
            {
                if (player.position.Equals(enemy))
                {
                    bool won = PlayRockPaperScissors();
                    if (!won)
                    {
                        Console.WriteLine(Program.TEXT_GAME_OVER);
                        return;
                    }
                    else
                    {
                        enemies.Remove(enemy);
                        break;
                    }
                }
            }

            // Пример: собрали зелье здоровья
            foreach (var potion in healthPotionsPositions)
            {
                if (player.position.Equals(potion))
                {
                    player.healthPotions++;
                    healthPotionsPositions.Remove(potion);
                    break;
                }
            }

            // Пример: собрали душу
            foreach (var soul in soulPositions)
            {
                if (player.position.Equals(soul))
                {
                    souls++;
                    soulPositions.Remove(soul);
                    break;
                }
            }

            // Пример: достигли следующего уровня
            if (player.position.Equals(nextLevelPosition))
            {
                level++;
                player.health = Program.INITIAL_HEALTH;
                player.healthPotions = Program.INITIAL_HEALTH_POTIONS;
                gameMap = new GameMap(Program.MAP_SIZE);
                player.position = new Tuple<int, int>(Program.MAP_SIZE / 2, Program.MAP_SIZE / 2);
                npcPosition = gameMap.PlaceObject(Program.NPC_SYMBOL);
                enemies = PlaceObjects(Program.ENEMY_COUNT, Program.ENEMY_SYMBOL);
                healthPotionsPositions = PlaceObjects(Program.HEALTH_POTION_COUNT, Program.HEALTH_POTION_SYMBOL);
                soulPositions = new List<Tuple<int, int>>();
                nextLevelPosition = null;
            }
        }
    }
}

public class Program
{
    public const int MAP_SIZE = 10;
    public const int PAINTER_STEPS_AMOUNT = 50;
    public const char PATH_SYMBOL = '1';
    public const char BLOCKED_SYMBOL = '█';
    public const char PLAYER_SYMBOL = '@';
    public const char NPC_SYMBOL = 'N';
    public const char ENEMY_SYMBOL = 'E';
    public const char HEALTH_POTION_SYMBOL = 'H';
    public const int ENEMY_COUNT = 5;
    public const int HEALTH_POTION_COUNT = 3;
    public const int INITIAL_HEALTH = 10;
    public const int INITIAL_HEALTH_POTIONS = 2;
    public const string TEXT_CHOOSE_MOVE = "Выберите ход: (1) Камень (2) Ножницы (3) Бумага: ";
    public const string TEXT_OPPONENT_CHOICE = "Ваш противник выбрал: ";
    public const string TEXT_DRAW = "Ничья!";
    public const string TEXT_ROUND_WIN = "Вы выиграли раунд!";
    public const string TEXT_ROUND_LOSS = "Вы проиграли раунд!";
    public const string TEXT_SCORE = "Счет - Вы: {player_score}, Противник: {enemy_score}";
    public const string TEXT_GAME_WIN = "Вы выиграли игру!";
    public const string TEXT_GAME_LOSS = "Вы проиграли игру!";
    public const string TEXT_INVALID_INPUT = "Неверный ввод. Попробуйте снова.";
    public const string TEXT_MET_NPC = "Вы встретили NPC!";
    public const string TEXT_CHOOSE_ACTION = "Выберите действие: (w) вверх, (a) влево, (s) вниз, (d) вправо, (h) использовать зелье, (q) выйти: ";
    public const string TEXT_GAME_OVER = "Игра окончена.";

    public static void Main()
    {
        Game game = new Game();
        game.PlayGame();
    }
}
