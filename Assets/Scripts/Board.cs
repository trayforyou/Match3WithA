using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour
{
    public int width = 8;
    public int height = 8;
    public GameObject[] fruitPrefabs;
    private bool isProcessing = false;
    public bool IsBusy => isProcessing; // публичное свойство для других скриптов

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
                GameObject fruit = Instantiate(prefabToUse, spawnPosition, Quaternion.identity, transform);
                fruit.name = $"Fruit {x},{y}";

                Fruit fruitScript = fruit.GetComponent<Fruit>();
                if (fruitScript != null)
                {
                    fruitScript.x = x;
                    fruitScript.y = y;
                }

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

            bool horizontalMatch = x >= 2 &&
                grid[x - 1, y] != null &&
                grid[x - 2, y] != null &&
                grid[x - 1, y].tag == tag &&
                grid[x - 2, y].tag == tag;

            bool verticalMatch = y >= 2 &&
                grid[x, y - 1] != null &&
                grid[x, y - 2] != null &&
                grid[x, y - 1].tag == tag &&
                grid[x, y - 2].tag == tag;

            if (!horizontalMatch && !verticalMatch)
                return candidate;

            validPrefabs.Remove(candidate);
        }

        return fruitPrefabs[0];
    }

    public bool IsInsideBoard(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public void TrySwapFruits(Fruit fruit, int dx, int dy)
    {
        if (isProcessing)
            return;

        int x1 = fruit.x;
        int y1 = fruit.y;
        int x2 = x1 + dx;
        int y2 = y1 + dy;

        if (!IsInsideBoard(x2, y2))
            return;

        GameObject otherFruitGO = grid[x2, y2];
        if (otherFruitGO == null)
            return;

        Fruit otherFruit = otherFruitGO.GetComponent<Fruit>();

        // Меняем местами в сетке
        grid[x1, y1] = otherFruitGO;
        grid[x2, y2] = fruit.gameObject;

        // Меняем координаты у фруктов
        fruit.x = x2; fruit.y = y2;
        otherFruit.x = x1; otherFruit.y = y1;

        // Запускаем анимацию (предположим SmoothMove реализован)
        StartCoroutine(fruit.SmoothMove(new Vector2(x2, y2)));
        StartCoroutine(otherFruit.SmoothMove(new Vector2(x1, y1)));

        // Проверяем, есть ли матч
        if (HasMatchAt(x2, y2) || HasMatchAt(x1, y1))
        {
            // Если матч есть — запускаем обработку
            StartCoroutine(ClearAndRefill());
        }
        else
        {
            // Нет матча — откатываем
            StartCoroutine(RevertSwap(fruit, otherFruit));
        }
    }

    private IEnumerator RevertSwap(Fruit fruit1, Fruit fruit2)
    {
        yield return new WaitForSeconds(0.3f);

        int x1 = fruit1.x;
        int y1 = fruit1.y;
        int x2 = fruit2.x;
        int y2 = fruit2.y;

        // Меняем обратно в сетке
        grid[x1, y1] = fruit1.gameObject;
        grid[x2, y2] = fruit2.gameObject;

        // Меняем координаты обратно
        fruit1.x = x2; fruit1.y = y2;
        fruit2.x = x1; fruit2.y = y1;

        StartCoroutine(fruit1.SmoothMove(new Vector2(x2, y2)));
        StartCoroutine(fruit2.SmoothMove(new Vector2(x1, y1)));
    }

    private bool HasMatchAt(int x, int y)
    {
        GameObject fruitGO = grid[x, y];
        if (fruitGO == null) return false;
        string tag = fruitGO.tag;

        int count = 1;

        // Горизонтальный матч
        for (int i = x - 1; i >= 0 && grid[i, y] != null && grid[i, y].tag == tag; i--)
            count++;
        for (int i = x + 1; i < width && grid[i, y] != null && grid[i, y].tag == tag; i++)
            count++;
        if (count >= 3)
            return true;

        // Вертикальный матч
        count = 1;
        for (int j = y - 1; j >= 0 && grid[x, j] != null && grid[x, j].tag == tag; j--)
            count++;
        for (int j = y + 1; j < height && grid[x, j] != null && grid[x, j].tag == tag; j++)
            count++;
        return count >= 3;
    }

    private IEnumerator ClearAndRefill()
    {
        isProcessing = true;

        bool comboFound;
        do
        {
            yield return new WaitForSeconds(0.3f);
            List<Vector2Int> matches = FindAllMatches();

            comboFound = matches.Count > 0;

            if (comboFound)
            {
                foreach (Vector2Int pos in matches)
                {
                    Destroy(grid[pos.x, pos.y]);
                    grid[pos.x, pos.y] = null;
                }

                yield return new WaitForSeconds(0.3f);
                CollapseColumns();
                yield return new WaitForSeconds(0.3f);
                FillEmptySpaces();
                yield return new WaitForSeconds(0.3f);
            }
        }
        while (comboFound);

        isProcessing = false;
    }

    private List<Vector2Int> FindAllMatches()
    {
        List<Vector2Int> matchedPositions = new List<Vector2Int>();

        // Поиск горизонтальных матчей
        for (int y = 0; y < height; y++)
        {
            int matchLength = 1;
            for (int x = 1; x < width; x++)
            {
                if (grid[x, y] != null && grid[x - 1, y] != null &&
                    grid[x, y].tag == grid[x - 1, y].tag)
                {
                    matchLength++;
                }
                else
                {
                    if (matchLength >= 3)
                    {
                        for (int k = 0; k < matchLength; k++)
                            matchedPositions.Add(new Vector2Int(x - 1 - k, y));
                    }
                    matchLength = 1;
                }
            }
            if (matchLength >= 3)
            {
                for (int k = 0; k < matchLength; k++)
                    matchedPositions.Add(new Vector2Int(width - 1 - k, y));
            }
        }

        // Поиск вертикальных матчей
        for (int x = 0; x < width; x++)
        {
            int matchLength = 1;
            for (int y = 1; y < height; y++)
            {
                if (grid[x, y] != null && grid[x, y - 1] != null &&
                    grid[x, y].tag == grid[x, y - 1].tag)
                {
                    matchLength++;
                }
                else
                {
                    if (matchLength >= 3)
                    {
                        for (int k = 0; k < matchLength; k++)
                            matchedPositions.Add(new Vector2Int(x, y - 1 - k));
                    }
                    matchLength = 1;
                }
            }
            if (matchLength >= 3)
            {
                for (int k = 0; k < matchLength; k++)
                    matchedPositions.Add(new Vector2Int(x, height - 1 - k));
            }
        }

        return matchedPositions;
    }

    private void CollapseColumns()
    {
        for (int x = 0; x < width; x++)
        {
            int emptySpot = -1; // индекс пустой ячейки снизу

            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null && emptySpot == -1)
                {
                    emptySpot = y;
                }
                else if (grid[x, y] != null && emptySpot != -1)
                {
                    // Сдвигаем фрукт вниз в пустую ячейку
                    grid[x, emptySpot] = grid[x, y];
                    grid[x, y] = null;

                    Fruit fruitScript = grid[x, emptySpot].GetComponent<Fruit>();
                    fruitScript.y = emptySpot;

                    StartCoroutine(fruitScript.SmoothMove(new Vector2(x, emptySpot)));

                    emptySpot++;
                }
            }
        }
    }

    private void FillEmptySpaces()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                if (grid[x, y] == null)
                {
                    GameObject prefabToUse = GetValidPrefab(x, y);
                    Vector2 spawnPosition = new Vector2(x, y);
                    GameObject fruit = Instantiate(prefabToUse, spawnPosition, Quaternion.identity, transform);
                    fruit.name = $"Fruit {x},{y}";

                    Fruit fruitScript = fruit.GetComponent<Fruit>();
                    fruitScript.x = x;
                    fruitScript.y = y;

                    grid[x, y] = fruit;
                }
            }
        }
    }
}
