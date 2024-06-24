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
    }

    private char[][] InitializeMap()
    {
        char[][] newMap = new char[size][];
        for (int i = 0; i < size; i++)
        {
            newMap[i] = new char[size];
            for (int j = 0; j < size; j++)
            {
                newMap[i][j] = '0'; // Блокированный символ
            }
        }
        return newMap;
    }
}

public class Player
{
    internal Tuple<int, int> position;

    public Player(Tuple<int, int> startPos)
    {
        this.position = startPos;
    }
}

public static class Program
{
    public const int MAP_SIZE = 10;
    public const char BLOCKED_SYMBOL = '#';
    public const char PATH_SYMBOL = '.';
    public const char PLAYER_SYMBOL = 'P';

    static void Main(string[] args)
    {
        GameMap gameMap = new GameMap(MAP_SIZE);
        Player player = new Player(new Tuple<int, int>(MAP_SIZE / 2, MAP_SIZE / 2));
    }
}
