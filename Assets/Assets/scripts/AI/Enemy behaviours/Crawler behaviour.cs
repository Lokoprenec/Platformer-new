using UnityEngine;

public class Crawlerbehaviour : MonoBehaviour
{
    private BoxCollider2D col;
    private Rigidbody2D rb;
    public GameObject groundCheck;
    public GameObject wallCheck;
    public LayerMask groundLayer;
    public float crawlSpeed;
    public int direction;
    [SerializeField] private CrawlerStates currentState;
    public float turnCooldown;
    private float turnTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        col = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        transform.localScale = new Vector2(direction, transform.localScale.y);
        currentState = CrawlerStates.Crawling;
    }

    // Update is called once per frame
    void Update()
    {
        if (rb.linearVelocityY != 0)
        {
            rb.linearVelocityX = 0;
            currentState = CrawlerStates.Falling;
        }
        else if (currentState == CrawlerStates.Falling)
        {
            currentState = CrawlerStates.Turning;
            turnTimer = turnCooldown;
        }

        StateMachine();
    }

    void StateMachine()
    {
        switch (currentState)
        {
            case CrawlerStates.Crawling:

                WhileCrawling();
                CheckForTurn();

                break;

            case CrawlerStates.Turning:

                WhileTurning();

                break;

            case CrawlerStates.Falling:

                WhileFalling();

                break;
        }
    }

    void WhileCrawling()
    {
        rb.linearVelocityX = crawlSpeed * direction;
    }

    void CheckForTurn()
    {
        if (isOnTheEdge() || isNearTheWall())
        {
            direction = -direction;
            transform.localScale = new Vector2(direction, transform.localScale.y);
            currentState = CrawlerStates.Turning;
            turnTimer = turnCooldown;
            rb.linearVelocityX = 0;
        }
    }

    void WhileTurning()
    {
        turnTimer -= Time.deltaTime;

        if (turnTimer < 0)
        {
            currentState = CrawlerStates.Crawling;
        }
    }

    void WhileFalling()
    {
        // no function yet
    }

    bool isOnTheEdge()
    {
        return Physics2D.OverlapPoint(groundCheck.transform.position, groundLayer) == null;
    }

    bool isNearTheWall()
    {
        return Physics2D.OverlapPoint(wallCheck.transform.position, groundLayer);
    }
}

public enum CrawlerStates
{
    Crawling, Turning, Falling
}
