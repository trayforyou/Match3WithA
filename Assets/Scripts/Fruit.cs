using System.Collections;
using UnityEngine;

public enum FruitType
{
    Normal,
    LineHorizontal,
    LineVertical
}

public class Fruit : MonoBehaviour
{
    public int x;
    public int y;
    [SerializeField]private Board _board;
    private SpriteRenderer spriteRenderer;
    public FruitType type = FruitType.Normal;

    private void Awake()
    {
        _board = FindObjectOfType<Board>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetSpecial(FruitType newType)
    {
        type = newType;

        if (spriteRenderer != null)
        {
            switch (type)
            {
                case FruitType.LineHorizontal:
                    spriteRenderer.color = Color.green;
                    break;
                case FruitType.LineVertical:
                    spriteRenderer.color = Color.black;
                    break;
                case FruitType.Normal:
                    spriteRenderer.color = Color.white; // можно задать исходный цвет
                    break;
            }
        }
    }

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