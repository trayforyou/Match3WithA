using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Board : MonoBehaviour
{
    public Sprite Cross;
    public Sprite Rainbow;
    public Sprite LineVertical;
    public Sprite LineHorizontal;
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

    private void SetSpecialFruit(int x, int y, FruitType specialType)
    {
        if (!IsInsideBoard(x, y)) return;

        GameObject fruit = grid[x, y];
        if (fruit == null) return;

        Fruit fruitScript = fruit.GetComponent<Fruit>();
        if (fruitScript != null)
        {
            fruitScript.SetSpecial(specialType);
        }
    }

    private List<Vector2Int> FindAllMatches()
    {
        List<Vector2Int> matchedPositions = new List<Vector2Int>();

        // Горизонтальные матчи
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
                    if (matchLength == 4)
                    {
                        // Помечаем три на удаление
                        matchedPositions.Add(new Vector2Int(x - 1, y));
                        matchedPositions.Add(new Vector2Int(x - 2, y));
                        matchedPositions.Add(new Vector2Int(x - 3, y));

                        // Превращаем четвёртый (x-4) в суперфрукт
                        SetSpecialFruit(x - 4, y, FruitType.LineHorizontal);
                    }
                    else if (matchLength >= 3)
                    {
                        for (int k = 0; k < matchLength; k++)
                            matchedPositions.Add(new Vector2Int(x - 1 - k, y));
                    }

                    matchLength = 1;
                }
            }

            if (matchLength == 4)
            {
                matchedPositions.Add(new Vector2Int(width - 1, y));
                matchedPositions.Add(new Vector2Int(width - 2, y));
                matchedPositions.Add(new Vector2Int(width - 3, y));

                SetSpecialFruit(width - 4, y, FruitType.LineHorizontal);
            }
            else if (matchLength >= 3)
            {
                for (int k = 0; k < matchLength; k++)
                    matchedPositions.Add(new Vector2Int(width - 1 - k, y));
            }
        }

        // Вертикальные матчи
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
                    if (matchLength == 4)
                    {
                        matchedPositions.Add(new Vector2Int(x, y - 1));
                        matchedPositions.Add(new Vector2Int(x, y - 2));
                        matchedPositions.Add(new Vector2Int(x, y - 3));

                        SetSpecialFruit(x, y - 4, FruitType.LineVertical);
                    }
                    else if (matchLength >= 3)
                    {
                        for (int k = 0; k < matchLength; k++)
                            matchedPositions.Add(new Vector2Int(x, y - 1 - k));
                    }

                    matchLength = 1;
                }
            }

            if (matchLength == 4)
            {
                matchedPositions.Add(new Vector2Int(x, height - 1));
                matchedPositions.Add(new Vector2Int(x, height - 2));
                matchedPositions.Add(new Vector2Int(x, height - 3));

                SetSpecialFruit(x, height - 4, FruitType.LineVertical);
            }
            else if (matchLength >= 3)
            {
                for (int k = 0; k < matchLength; k++)
                    matchedPositions.Add(new Vector2Int(x, height - 1 - k));
            }
        }

        return matchedPositions;
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

    public void TrySwapFruits(Fruit fruit1, Fruit fruit2)
    {
        if (isProcessing) return;

        int x1 = fruit1.x;
        int y1 = fruit1.y;
        int x2 = fruit2.x;
        int y2 = fruit2.y;

        // Проверка соседства
        if (!((Mathf.Abs(x1 - x2) == 1 && y1 == y2) || (Mathf.Abs(y1 - y2) == 1 && x1 == x2)))
            return;

        StartCoroutine(SwapRoutine(fruit1, fruit2));
    }

    private IEnumerator SwapRoutine(Fruit f1, Fruit f2)
    {
        isProcessing = true;

        int x1 = f1.x;
        int y1 = f1.y;
        int x2 = f2.x;
        int y2 = f2.y;

        // Меняем в сетке
        grid[x1, y1] = f2.gameObject;
        grid[x2, y2] = f1.gameObject;

        // Меняем координаты фруктов
        f1.x = x2;
        f1.y = y2;
        f2.x = x1;
        f2.y = y1;

        // Анимация движения (предполагается метод SmoothMove)
        yield return StartCoroutine(f1.SmoothMove(new Vector2(x2, y2)));
        yield return StartCoroutine(f2.SmoothMove(new Vector2(x1, y1)));

        // Проверка матча
        if (HasMatchAt(x2, y2) || HasMatchAt(x1, y1))
        {
            // Если матч есть — запускаем Clear и Refilling
            yield return StartCoroutine(ClearAndRefill());
        }
        else
        {
            // Если нет матча — меняем обратно
            grid[x1, y1] = f1.gameObject;
            grid[x2, y2] = f2.gameObject;

            f1.x = x1;
            f1.y = y1;
            f2.x = x2;
            f2.y = y2;

            yield return StartCoroutine(f1.SmoothMove(new Vector2(x1, y1)));
            yield return StartCoroutine(f2.SmoothMove(new Vector2(x2, y2)));
        }

        isProcessing = false;
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

    private Dictionary<string, List<Fruit>> GroupMatchesByTag(List<Vector2Int> positions)
    {
        var result = new Dictionary<string, List<Fruit>>();

        foreach (var pos in positions)
        {
            GameObject go = grid[pos.x, pos.y];
            if (go == null) continue;

            Fruit fruit = go.GetComponent<Fruit>();
            if (fruit == null) continue;

            if (!result.ContainsKey(go.tag))
                result[go.tag] = new List<Fruit>();

            result[go.tag].Add(fruit);
        }

        return result;
    }

    private IEnumerator ClearAndRefill()
    {
        isProcessing = true;
        bool comboFound;

        do
        {
            yield return new WaitForSeconds(0.2f);

            List<Vector2Int> matches = FindAllMatches();
            comboFound = matches.Count > 0;

            if (comboFound)
            {
                Dictionary<string, List<Fruit>> groupedMatches = GroupMatchesByTag(matches);
                HashSet<GameObject> toDestroy = new HashSet<GameObject>();

                foreach (var group in groupedMatches)
                {
                    List<Fruit> fruits = group.Value;

                    if (fruits.Count == 3)
                    {
                        // собираем данные
                        int horizontal = 0;
                        int vertical = 0;
                        Fruit normalFruit = null;

                        foreach (var fruit in fruits)
                        {
                            switch (fruit.type)
                            {
                                case FruitType.LineHorizontal:
                                    horizontal++;
                                    break;
                                case FruitType.LineVertical:
                                    vertical++;
                                    break;
                                default:
                                    normalFruit = fruit;
                                    break;
                            }
                        }

                        if (horizontal == 1 && vertical == 1 && normalFruit != null)
                        {
                            // создаём крест на месте обычного фрукта
                            normalFruit.SetSpecial(FruitType.Cross);
                            toDestroy.UnionWith(fruits.Where(f => f != normalFruit).Select(f => f.gameObject));
                        }
                        else if (horizontal + vertical == 3)
                        {
                            // все три — суперфрукты, активируем каждый
                            foreach (var f in fruits)
                            {
                                yield return ActivateSuperFruit(f);
                            }
                        }
                        else
                        {
                            // есть только один суперфрукт — активируем его
                            Fruit super = fruits.FirstOrDefault(f => f.type != FruitType.Normal);
                            if (super != null)
                            {
                                yield return ActivateSuperFruit(super);
                                foreach (var f in fruits)
                                {
                                    if (f != super)
                                        toDestroy.Add(f.gameObject);
                                }
                            }
                            else
                            {
                                // обычный матч
                                foreach (var f in fruits)
                                    toDestroy.Add(f.gameObject);
                            }
                        }
                    }
                    else
                    {
                        // стандартное поведение — удаляем всё
                        foreach (var f in fruits)
                            toDestroy.Add(f.gameObject);
                    }
                }

                // Удаляем объекты
                foreach (var obj in toDestroy)
                {
                    Fruit fruit = obj.GetComponent<Fruit>();
                    if (fruit != null)
                        grid[fruit.x, fruit.y] = null;

                    Destroy(obj);
                }

                yield return new WaitForSeconds(0.2f);
                CollapseColumns();
                yield return new WaitForSeconds(0.2f);
                FillEmptySpaces();
                yield return new WaitForSeconds(0.2f);
            }

        } while (comboFound);

        isProcessing = false;
    }

    private IEnumerator ActivateSuperFruit(Fruit fruit)
    {
        if (fruit == null) yield break;

        // Сохраняем координаты и убираем ссылку из сетки заранее
        int fx = fruit.x;
        int fy = fruit.y;
        grid[fx, fy] = null;

        // Выполняем эффект
        switch (fruit.type)
        {
            case FruitType.LineHorizontal:
                yield return StartCoroutine(DestroyRow(fy));
                break;
            case FruitType.LineVertical:
                yield return StartCoroutine(DestroyColumn(fx));
                break;
            case FruitType.Cross:
                yield return StartCoroutine(DestroyRow(fy));
                yield return StartCoroutine(DestroyColumn(fx));
                break;
        }

        // Теперь безопасно удалить сам фрукт
        Destroy(fruit.gameObject);
    }



    //private IEnumerator ClearAndRefill()
    //{
    //    isProcessing = true;

    //    bool comboFound;

    //    do
    //    {
    //        yield return new WaitForSeconds(0.2f);

    //        List<Vector2Int> matches = FindAllMatches();
    //        comboFound = matches.Count > 0;

    //        HashSet<GameObject> fruitsToDestroy = new HashSet<GameObject>();

    //        if (comboFound)
    //        {
    //            foreach (Vector2Int pos in matches)
    //            {
    //                GameObject fruitObj = grid[pos.x, pos.y];
    //                if (fruitObj == null) continue;

    //                Fruit fruit = fruitObj.GetComponent<Fruit>();
    //                if (fruit == null) continue;

    //                // если суперфрукт — активируем его эффект
    //                if (fruit.type == FruitType.LineHorizontal)
    //                {
    //                    yield return StartCoroutine(DestroyRow(fruit.y));
    //                }
    //                else if (fruit.type == FruitType.LineVertical)
    //                {
    //                    yield return StartCoroutine(DestroyColumn(fruit.x));
    //                }
    //                else
    //                {
    //                    // обычный фрукт — просто на уничтожение
    //                    fruitsToDestroy.Add(fruitObj);
    //                }
    //            }

    //            // Удаляем обычные фрукты
    //            foreach (GameObject obj in fruitsToDestroy)
    //            {
    //                Fruit f = obj.GetComponent<Fruit>();
    //                if (f != null)
    //                    grid[f.x, f.y] = null;

    //                Destroy(obj);
    //            }

    //            yield return new WaitForSeconds(0.2f);

    //            CollapseColumns();
    //            yield return new WaitForSeconds(0.2f);

    //            FillEmptySpaces();
    //            yield return new WaitForSeconds(0.2f);
    //        }

    //    } while (comboFound);

    //    isProcessing = false;
    //}

    private IEnumerator DestroyRow(int row)
    {
        List<GameObject> toDestroy = new List<GameObject>();

        for (int x = 0; x < width; x++)
        {
            var obj = grid[x, row];
            // grid[x,row] уже null для спецфрукта, поэтому он не попадёт в этот список
            if (obj != null)
            {
                toDestroy.Add(obj);
                grid[x, row] = null;
            }
        }

        foreach (var obj in toDestroy)
        {
            if (obj != null)
                Destroy(obj);
        }

        yield return new WaitForSeconds(0.1f);
    }

    private IEnumerator DestroyColumn(int col)
    {
        List<GameObject> toDestroy = new List<GameObject>();

        for (int y = 0; y < height; y++)
        {
            var obj = grid[col, y];
            if (obj != null)
            {
                toDestroy.Add(obj);
                grid[col, y] = null;
            }
        }

        foreach (var obj in toDestroy)
        {
            if (obj != null)
                Destroy(obj);
        }

        yield return new WaitForSeconds(0.1f);
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
