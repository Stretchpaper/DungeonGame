using System;
using System.Collections.Generic;

public class GameMap
{
    private int size;
    internal char[][] map;

    public GameMap(int size)
    {
        this.size = size;
        this.map = InitializeMap();
        PlaceObject(Program.NPC_SYMBOL);
        PlaceEnemies();
    }

    private char[][] InitializeMap()
    {
        char[][] newMap = new char[size][];
        for (int i = 0; i < size; i++)
        {
            newMap[i] = new char[size];
            for (int j = 0; j < size; j++)
            {
                newMap[i][j] = Program.BLOCKED_SYMBOL; // Блокированный символ
            }
        }
        return newMap;
    }

    public void PrintMap(Tuple<int, int> playerPos)
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                Console.Write(playerPos.Equals(new Tuple<int, int>(i, j)) ? Program.PLAYER_SYMBOL : map[i][j]);
            }
            Console.WriteLine();
        }
    }

    public void PlaceObject(char symbol)
    {
        Random random = new Random();
        int x = random.Next(0, size);
        int y = random.Next(0, size);
        map[x][y] = symbol;
    }

    public void PlaceEnemies()
    {
        for (int i = 0; i < Program.ENEMY_COUNT; i++)
        {
            PlaceObject(Program.ENEMY_SYMBOL);
        }
    }
}

public class Player
{
    internal Tuple<int, int> position;

    public Player(Tuple<int, int> startPos)
    {
        this.position = startPos;
    }

    public void Move(string direction, GameMap gameMap)
    {
        int x = position.Item1;
        int y = position.Item2;
        switch (direction)
        {
            case "w" when x - 1 >= 0:
                x -= 1;
                break;
            case "s" when x + 1 < Program.MAP_SIZE:
                x += 1;
                break;
            case "a" when y - 1 >= 0:
                y -= 1;
                break;
            case "d" when y + 1 < Program.MAP_SIZE:
                y += 1;
                break;
        }
        position = new Tuple<int, int>(x, y);

        if (gameMap.map[x][y] == Program.NPC_SYMBOL)
        {
            Console.WriteLine(Program.TEXT_NPC_ENCOUNTER);
        }
        else if (gameMap.map[x][y] == Program.ENEMY_SYMBOL)
        {
            Console.WriteLine(Program.TEXT_ENEMY_ENCOUNTER);
            PlayRockPaperScissors();
        }
    }

    private void PlayRockPaperScissors()
    {
        Random random = new Random();
        string[] choices = { "Камень", "Ножницы", "Бумага" };
        string playerChoice = "";
        string enemyChoice = choices[random.Next(0, 3)];

        Console.Write("Выберите ход (Камень, Ножницы, Бумага): ");
        playerChoice = Console.ReadLine();

        Console.WriteLine($"Враг выбрал: {enemyChoice}");

        if (playerChoice == enemyChoice)
        {
            Console.WriteLine("Ничья!");
        }
        else if ((playerChoice == "Камень" && enemyChoice == "Ножницы") ||
                 (playerChoice == "Ножницы" && enemyChoice == "Бумага") ||
                 (playerChoice == "Бумага" && enemyChoice == "Камень"))
        {
            Console.WriteLine("Вы победили!");
        }
        else
        {
            Console.WriteLine("Вы проиграли!");
        }
    }
}

public static class Program
{
    public const int MAP_SIZE = 10;
    public const int ENEMY_COUNT = 5;
    public const char BLOCKED_SYMBOL = '#';
    public const char PATH_SYMBOL = '.';
    public const char PLAYER_SYMBOL = 'P';
    public const char NPC_SYMBOL = 'N';
    public const char ENEMY_SYMBOL = 'E';
    public const string TEXT_NPC_ENCOUNTER = "Вы встретили NPC!";
    public const string TEXT_ENEMY_ENCOUNTER = "Вы встретили врага!";

    static void Main(string[] args)
    {
        GameMap gameMap = new GameMap(MAP_SIZE);
        Player player = new Player(new Tuple<int, int>(MAP_SIZE / 2, MAP_SIZE / 2));

        while (true)
        {
            Console.Clear();
            gameMap.PrintMap(player.position);
            Console.Write("Введите направление (w, a, s, d): ");
            string direction = Console.ReadLine().ToLower();
            player.Move(direction, gameMap);
        }
    }
}
