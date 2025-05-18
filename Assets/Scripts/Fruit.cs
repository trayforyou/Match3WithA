using System.Collections;
using UnityEngine;

public class Fruit : MonoBehaviour
{
    public int x;
    public int y;
    [SerializeField]private Board _board;

    private void Awake()
    {
        _board = FindObjectOfType<Board>();
    }

    //public void Move(int deltaX, int deltaY)
    //{
    //    int targetX = x + deltaX;
    //    int targetY = y + deltaY;

    //    // Проверка границ
    //    if (_board.IsInsideBoard(targetX, targetY) && _board.grid[targetX, targetY] == null)
    //    {
    //        // Обновить позицию в сетке
    //        _board.grid[x, y] = null;
    //        _board.grid[targetX, targetY] = this.gameObject;

    //        x = targetX;
    //        y = targetY;

    //        // Анимация перемещения
    //        StartCoroutine(SmoothMove(new Vector2(x, y)));
    //    }
    //}

    public IEnumerator SmoothMove(Vector2 targetPosition)
    {
        Vector2 start = transform.position;
        Vector2 end = targetPosition;
        float t = 0;
        float duration = 0.15f;

        while (t < 1)
        {
            t += Time.deltaTime / duration;
            transform.position = Vector2.Lerp(start, end, t);
            yield return null;
        }

        transform.position = end;
    }
}