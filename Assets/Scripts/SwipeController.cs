using UnityEngine;

public class SwipeController : MonoBehaviour
{
    private Vector2 startPos;
    private Fruit selectedFruit;
    [SerializeField] private Board fruitBoard;

    void Start()
    {
        if (fruitBoard == null)
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
                selectedFruit = hit.GetComponent<Fruit>();
                if (selectedFruit != null)
                    startPos = worldPos;
            }
        }

        if (Input.GetMouseButtonUp(0) && selectedFruit != null)
        {
            if (fruitBoard.IsBusy)
            {
                selectedFruit = null;
                return;
            }

            Vector2 endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 swipe = endPos - startPos;

            if (swipe.magnitude > 0.3f)
            {
                int dx = 0, dy = 0;

                if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
                    dx = swipe.x > 0 ? 1 : -1;
                else
                    dy = swipe.y > 0 ? 1 : -1;

                fruitBoard.TrySwapFruits(selectedFruit, dx, dy);
            }

            selectedFruit = null;
        }
    }
}