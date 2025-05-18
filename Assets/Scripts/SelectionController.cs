using UnityEngine;

public class SelectionController : MonoBehaviour
{
    private Fruit firstSelected = null;
    private Fruit secondSelected = null;

    [SerializeField] private Board fruitBoard;

    void Start()
    {
        fruitBoard = FindObjectOfType<Board>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (fruitBoard.IsBusy) return;

            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(worldPos);

            if (hit != null)
            {
                Fruit clickedFruit = hit.GetComponent<Fruit>();
                if (clickedFruit != null)
                    HandleSelection(clickedFruit);
            }
        }
    }

    private void HandleSelection(Fruit clickedFruit)
    {
        if (firstSelected == null)
        {
            // �������� ������ �����
            firstSelected = clickedFruit;
            HighlightFruit(firstSelected, true);
        }
        else if (firstSelected == clickedFruit)
        {
            // ���� �������� �� ��� �� ����� � �������� �����
            HighlightFruit(firstSelected, false);
            firstSelected = null;
        }
        else
        {
            // ������ ����� ������
            secondSelected = clickedFruit;

            // ���������, �������� �� ��
            if (AreNeighbors(firstSelected, secondSelected))
            {
                // ������� �������� �������
                HighlightFruit(firstSelected, false);
                fruitBoard.TrySwapFruits(firstSelected, secondSelected);
            }
            else
            {
                // �� ����� � ���������� ����� ������� � �������� �������
                HighlightFruit(firstSelected, false);
                firstSelected = secondSelected;
                HighlightFruit(firstSelected, true);
            }

            secondSelected = null;
        }
    }

    private bool AreNeighbors(Fruit f1, Fruit f2)
    {
        int dx = Mathf.Abs(f1.x - f2.x);
        int dy = Mathf.Abs(f1.y - f2.y);

        return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
    }

    private void HighlightFruit(Fruit fruit, bool highlight)
    {
        // ��� ����� �������� ���������� ��������� (��������, ������ ����, ��������� Outline � �.�.)
        SpriteRenderer sr = fruit.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = highlight ? Color.yellow : Color.white;
        }
    }
}

