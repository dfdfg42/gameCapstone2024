using UnityEngine;
using System.Collections;
using Assets.Undead_Survivor.Codes;

public abstract class Enemy : MonoBehaviour, IObjectDameged
{
    [Header("기본 스탯")]
    public float speed = 2f;
    public float health = 10f;
    public float maxHealth = 10f;
    public float damage = 10f;

    [Header("컴포넌트")]
    protected Rigidbody2D rigid;
    protected Collider2D coll;
    protected Animator anim;
    protected SpriteRenderer spriter;
    protected WaitForFixedUpdate wait;

    [Header("타겟")]
    protected Transform target;
    protected bool isLive;

    // 이벤트
    public System.Action<Enemy> OnDeath;
    public System.Action<float> OnDamaged;

    protected virtual void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();
        wait = new WaitForFixedUpdate();

        SetupRigidbody();
    }

    protected virtual void OnEnable()
    {
        if (GameManager.Instance?.player != null)
        {
            target = GameManager.Instance.player.transform;
        }

        isLive = true;
        coll.enabled = true;
        rigid.simulated = true;
        spriter.sortingOrder = 2;
        anim.SetBool("Dead", false);
        health = maxHealth;
    }

    protected virtual void FixedUpdate()
    {
        if (!GameManager.Instance.isLive || !isLive)
            return;

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
            return;

        Move();
    }

    protected virtual void LateUpdate()
    {
        if (!GameManager.Instance.isLive || !isLive)
            return;

        UpdateVisuals();
    }

    // 추상 메서드 - 각 적 타입별로 구현
    protected abstract void Move();
    protected abstract void Attack();

    // 가상 메서드 - 필요시 오버라이드
    protected virtual void UpdateVisuals()
    {
        if (target != null)
            spriter.flipX = target.position.x < transform.position.x;
    }

    protected virtual void SetupRigidbody()
    {
        rigid.gravityScale = 0;
        rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
        rigid.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rigid.drag = 10;
        rigid.mass = 100;
    }

    public virtual void Init(SpawnData data)
    {
        if (anim != null && data.spriteType < animCon.Length)
        {
            anim.runtimeAnimatorController = animCon[data.spriteType];
        }
        speed = data.speed;
        maxHealth = data.health;
        health = maxHealth;
    }

    public virtual void Dameged(float damage)
    {
        health -= damage;
        StartCoroutine(KnockBack());
        OnDamaged?.Invoke(damage);

        if (health > 0)
        {
            anim.SetTrigger("Hit");
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
        }
        else
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        isLive = false;
        coll.enabled = false;
        rigid.simulated = false;
        spriter.sortingOrder = 1;
        anim.SetBool("Dead", true);

        GameManager.Instance.kill++;
        GameManager.Instance.GetExp();

        if (GameManager.Instance.isLive)
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Dead);

        OnDeath?.Invoke(this);
        gameObject.SetActive(false);
    }

    protected virtual IEnumerator KnockBack()
    {
        yield return wait;
        Vector3 playerPos = GameManager.Instance.player.transform.position;
        Vector3 dirVec = transform.position - playerPos;
        rigid.AddForce(dirVec.normalized * 1.3f, ForceMode2D.Impulse);
    }

    public RuntimeAnimatorController[] animCon;
}
