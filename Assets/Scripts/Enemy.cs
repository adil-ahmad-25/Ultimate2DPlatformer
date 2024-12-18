using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Enemy : MonoBehaviour
{
    private Transform player;
    public float chaseSpeed = 2f;
    public float jumpForce = 2f;
    public LayerMask groundLayer;

    public int damage = 1;

    public int maxHealth = 2;
    private int currentHealth;
    private SpriteRenderer spriteRenderer;
    private Color ogColor;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool shouldJump;

    [Header ("Loot Table")]
    public List<LootItem> lootTable = new List<LootItem>();

    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player").GetComponent<Transform>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        ogColor = spriteRenderer.color;
    }

    // Update is called once per frame
    void Update()
    {
        //is Grounded
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1f, groundLayer);

        //Player Direction
        float direction = Mathf.Sign(player.position.x - transform.position.x);

        //Player above detection
        bool isPlayerAbove = Physics2D.Raycast(transform.position, Vector2.up, 5f, 1 << player.gameObject.layer);

        if (isGrounded)
        {
            //Chase Player
            rb.velocity = new Vector2(direction * chaseSpeed, rb.velocity.y);

            //If Ground
            RaycastHit2D groundInFront = Physics2D.Raycast(transform.position, new Vector2(direction, 0), 2f, groundLayer);

            //Gap Ahead
            RaycastHit2D gapAhead = Physics2D.Raycast(transform.position + new Vector3(direction, 0, 0), Vector2.down, 2f, groundLayer);

            //If platform above
            RaycastHit2D platformAbove = Physics2D.Raycast(transform.position, Vector2.up, 5f, groundLayer);

            if (!groundInFront.collider && !gapAhead.collider)
            {
                shouldJump = true;
            }
            else if(isPlayerAbove && platformAbove.collider)
            {
                shouldJump = true;
            }
        }
    }

    private void FixedUpdate()
    {
        if (isGrounded && shouldJump)
        {
            shouldJump = false;
            Vector2 direction = (player.position - transform.position).normalized;

            Vector2 jumpDirection = direction * jumpForce;

            rb.AddForce(new Vector2(jumpDirection.x, jumpForce), ForceMode2D.Impulse);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        StartCoroutine(FlashWhite());
        if(currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashWhite()
    {
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.3f);
        spriteRenderer.color = ogColor;
    }

    void Die()
    {
        foreach (LootItem lootItem in lootTable)
        {
            if (Random.Range(0f, 100f) <= lootItem.dropChance)
            {
                InstantiateLoot(lootItem.itemPrefab);
            }
            break;
        }
        
        Destroy(gameObject);
    }

    void InstantiateLoot(GameObject loot)
    {
        if (loot)
        {
            GameObject droppedLoot = Instantiate(loot, transform.position, Quaternion.identity);

            droppedLoot.GetComponent<SpriteRenderer>().color = Color.red;
        }
    }
}
