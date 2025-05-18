using UnityEngine;
using System.Collections.Generic;

public class BoardGenerator : MonoBehaviour
{
    public int width = 8;
    public int height = 8;
    public GameObject[] fruitPrefabs; // ������� �������, ������ � ���������� tag

    public GameObject[,] grid;

    void Start()
    {
        GenerateBoard();
    }

    void GenerateBoard()
    {
        grid = new GameObject[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject prefabToUse = GetValidPrefab(x, y);

                Vector2 spawnPosition = new Vector2(x, y);
                GameObject fruit = Instantiate(prefabToUse, spawnPosition, Quaternion.identity);
                fruit.transform.parent = this.transform;
                fruit.name = $"Fruit {x},{y}";

                grid[x, y] = fruit;
            }
        }
    }

    GameObject GetValidPrefab(int x, int y)
    {
        List<GameObject> validPrefabs = new List<GameObject>(fruitPrefabs);

        while (validPrefabs.Count > 0)
        {
            GameObject candidate = validPrefabs[Random.Range(0, validPrefabs.Count)];
            string tag = candidate.tag;

            // �������� �� �������������� ����
            bool horizontalMatch = x >= 2 &&
                grid[x - 1, y] != null &&
                grid[x - 2, y] != null &&
                grid[x - 1, y].tag == tag &&
                grid[x - 2, y].tag == tag;

            // �������� �� ������������ ����
            bool verticalMatch = y >= 2 &&
                grid[x, y - 1] != null &&
                grid[x, y - 2] != null &&
                grid[x, y - 1].tag == tag &&
                grid[x, y - 2].tag == tag;

            if (!horizontalMatch && !verticalMatch)
                return candidate;

            validPrefabs.Remove(candidate); // ��������� ������������
        }

        // �� ������ ������ � ���� ��� ��������� ������� ����
        return fruitPrefabs[0];
    }
}