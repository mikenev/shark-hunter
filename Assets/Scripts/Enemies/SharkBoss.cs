using UnityEngine;

// Boss behaviour: patrol -> telegraph (flash red, line up with the diver) -> charge -> recover.
public class SharkBoss : MonoBehaviour
{
    public int maxHealth = 10;
    public Transform target;
    public Vector2 arenaMin;
    public Vector2 arenaMax;
    public SpriteRenderer spriteRenderer;
    public Collider2D bodyCollider;

    enum State { Patrol, Telegraph, Charge, Recover, Dying }

    const float PatrolSpeed = 2.5f;
    const float ChargeSpeed = 11f;
    const float EdgeMargin = 1f;

    State state;
    int health;
    int dir = 1;
    float timer;
    float targetY;
    float baseY;
    float flashUntil;

    void Start()
    {
        health = maxHealth;
        baseY = (arenaMin.y + arenaMax.y) * 0.5f;
        EnterPatrol();
    }

    void EnterPatrol()
    {
        state = State.Patrol;
        timer = Random.Range(2f, 3.5f);
    }

    void Update()
    {
        var p = transform.position;
        float dt = Time.deltaTime;
        bool blinkRed = false;

        switch (state)
        {
            case State.Patrol:
                p.x += dir * PatrolSpeed * dt;
                p.y = baseY + Mathf.Sin(Time.time * 1.5f) * 2f;
                if (p.x > arenaMax.x - EdgeMargin) dir = -1;
                else if (p.x < arenaMin.x + EdgeMargin) dir = 1;
                timer -= dt;
                if (timer <= 0f)
                {
                    state = State.Telegraph;
                    timer = 0.7f;
                    AudioManager.Play(Sfx.Warning);
                    if (target != null)
                    {
                        dir = target.position.x > p.x ? 1 : -1;
                        targetY = target.position.y;
                    }
                }
                break;

            case State.Telegraph:
                blinkRed = (int)(Time.time * 12f) % 2 == 0;
                p.y = Mathf.MoveTowards(p.y, targetY, 6f * dt);
                timer -= dt;
                if (timer <= 0f)
                {
                    state = State.Charge;
                    AudioManager.Play(Sfx.Charge);
                }
                break;

            case State.Charge:
                p.x += dir * ChargeSpeed * dt;
                if (p.x <= arenaMin.x + EdgeMargin || p.x >= arenaMax.x - EdgeMargin)
                {
                    p.x = Mathf.Clamp(p.x, arenaMin.x + EdgeMargin, arenaMax.x - EdgeMargin);
                    state = State.Recover;
                    timer = 1f;
                }
                break;

            case State.Recover:
                timer -= dt;
                if (timer <= 0f)
                {
                    dir = p.x > (arenaMin.x + arenaMax.x) * 0.5f ? -1 : 1;
                    EnterPatrol();
                }
                break;

            case State.Dying:
                p.y -= 3f * dt;
                transform.Rotate(0f, 0f, -120f * dt);
                timer -= dt;
                if (timer <= 0f)
                {
                    GameState.Instance.Win();
                    enabled = false;
                }
                break;
        }

        transform.position = p;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = dir < 0;
            var color = blinkRed ? Color.red : Color.white;
            if (Time.time < flashUntil) color = new Color(1f, 1f, 0.4f);
            spriteRenderer.color = color;
        }

        if (state != State.Dying) GameState.Instance.BossHealth01 = (float)health / maxHealth;
    }

    public void Hit(int damage)
    {
        if (state == State.Dying) return;

        health -= damage;
        flashUntil = Time.time + 0.1f;
        AudioManager.Play(Sfx.SharkHit);
        GameState.Instance.AddScore(50);

        if (health <= 0)
        {
            health = 0;
            GameState.Instance.BossHealth01 = 0f;
            state = State.Dying;
            timer = 1.5f;
            if (bodyCollider != null) bodyCollider.enabled = false;
        }
    }
}
