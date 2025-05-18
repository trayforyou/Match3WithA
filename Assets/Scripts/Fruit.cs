using System.Collections;
using UnityEngine;

public enum FruitType
{
    Normal,
    LineHorizontal,
    LineVertical,
    Rainbow,
    Cross // новый тип суперфрукта
}

public class Fruit : MonoBehaviour
{
    public int x;
    public int y;
    [SerializeField]private Board _board;
    private SpriteRenderer spriteRenderer;
    public FruitType type = FruitType.Normal;
    public SpriteRenderer overlayIcon;


    private void Awake()
    {
        _board = FindObjectOfType<Board>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        overlayIcon = transform.Find("OverlayIcon")?.GetComponent<SpriteRenderer>();
    }

    public void SetSpecial(FruitType newType)
    {
        type = newType;

        if (spriteRenderer != null)
        {
            switch (type)
            {
                case FruitType.LineHorizontal:
                    if (overlayIcon != null) 
                    {
                        overlayIcon.gameObject.SetActive(true);
                        overlayIcon.enabled = true;
                        overlayIcon.sprite = _board.LineHorizontal;
                    }
                    break;
                case FruitType.LineVertical:
                    if (overlayIcon != null)
                    {
                        overlayIcon.gameObject.SetActive(true);
                        overlayIcon.enabled = true;
                        overlayIcon.sprite = _board.LineVertical;
                    }
                    break;
                case FruitType.Rainbow:
                    if (overlayIcon != null)
                    {
                        overlayIcon.gameObject.SetActive(true);
                        overlayIcon.enabled = true;
                        overlayIcon.sprite = _board.Rainbow;
                    }
                    break;
                case FruitType.Cross:
                    if (overlayIcon != null)
                    {
                        overlayIcon.gameObject.SetActive(true);
                        overlayIcon.enabled = true;
                        overlayIcon.sprite = _board.Cross;
                    }
                    break;
                default:
                    spriteRenderer.color = Color.white;
                    if (overlayIcon != null) overlayIcon.enabled = false;
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