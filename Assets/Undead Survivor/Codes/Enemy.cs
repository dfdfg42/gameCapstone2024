using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Enemy : MonoBehaviour
{
    //default info
    public float speed;
    public float health;
    public float maxHealth;
    public float rangedDistance = 3f; //공격 거리

    //Attack
    public float damage = 10f;
    //public EnemyAttackType attackType = EnemyAttackType.melee;
    public EnemyAttackType attackType = EnemyAttackType.ranged; //임시

    //특수 Attack
    public float attackDamage = 3f; //특수 공격 데미지
    public float attackSpeed = 1f; //특수 공격 스피드

    //Trigger
    public bool attacked;
    public float attackMaxCooldown = 2f; //단위는 second
    public float attackCooldown = 2f;

    bool isLive;


    public RuntimeAnimatorController[] animCon;
    public Rigidbody2D target;

    Rigidbody2D rigid;
    Collider2D coll;
    Animator anim;
    SpriteRenderer spriter;
    WaitForFixedUpdate wait;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();
        wait = new WaitForFixedUpdate();

        //////////////test/////////////////////////
        ///
        rigid.gravityScale = 0;  // 중력 영향 제거
        rigid.constraints = RigidbodyConstraints2D.FreezeRotation;  // 회전 방지
        rigid.collisionDetectionMode = CollisionDetectionMode2D.Continuous;  // 충돌 감지 모드 설정
        rigid.drag = 10;  // 마찰력 증가로 미끄러짐 감소
        rigid.mass = 100;  // 질량 증가로 밀림 현상 감소
    }

    void FixedUpdate()
    {
        if (!GameManager.Instance.isLive)
            return;

        if (!isLive || anim.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
            return;

        Vector2 dirVec = target.position - rigid.position;
        float distance = Vector2.Distance(target.position, rigid.position);
        rigid.velocity = Vector2.zero;

        //Player tracking
        if (attackType == EnemyAttackType.ranged) //원거리 공격 타입일 때
        {

            Vector2 moveVec = Vector2.zero;

            // 거리가 rangedDistance보다 클 때만 이동
            if (distance < rangedDistance - 0.1f)  // 여유 범위 추가
            {
                //플레이어와 가까우면 뒤로 이동
                moveVec = -dirVec.normalized * speed;
            }
            else if (distance > rangedDistance + 0.1f) 
            {
                //플레이어와 멀면 앞으로 이동
                moveVec = dirVec.normalized * speed;
          
            }
            if (moveVec != Vector2.zero)
            {
                Vector2 newPosition = rigid.position + moveVec * Time.fixedDeltaTime;
                rigid.MovePosition(newPosition);
            }
        }

        else if(attackType == EnemyAttackType.rush)
        {
            if(attacked==true){
                return;
            }
            // 기본 이동.
            if (distance < rangedDistance - 0.1f)  // 여유 범위 추가
            {
                //플레이어와 가까우면 뒤로 이동
                onAttack();
            }
            Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
            rigid.MovePosition(rigid.position + nextVec);
        }

        else //기본 타입일 때
        {
            Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
            rigid.MovePosition(rigid.position + nextVec);
            
        }
    }

    void LateUpdate()
    {
        if (!GameManager.Instance.isLive)
            return;

        if (!isLive)
            return;

        spriter.flipX = target.position.x < rigid.position.x;
        if( (attackCooldown >= 0f) && (attacked == false)) {
            attackCooldown -= Time.deltaTime;
        }
    }

    void OnEnable()
    {
        target = GameManager.Instance.player.GetComponent<Rigidbody2D>();
        isLive = true;
        coll.enabled = true;
        rigid.simulated = true;
        spriter.sortingOrder = 2;
        anim.SetBool("Dead", false);
        health = maxHealth;
    }   

    public void onAttack()
    {
        if (attackType == EnemyAttackType.ranged)
        {
            //Ranged Attack
            //조건 플레이어와 거리가 특정 이하일 경우 & 쿨타임 돌았을 때.
            //발사체 오브젝트 생성해서 고유 데미지 값으로 플레이어 데미지 닳게 하기.
        }
        else if (attackType == EnemyAttackType.rush)
        {
            //rush Attack
            //조건 플레이어와 거리가 특정 이하일 경우 & 쿨타임 돌았을 때.
            //플레이어 방향으로 돌진하게 하면서 고유 데미지 값으로 플레이어 데미지 닳게하기.
            if((attacked == false) && (attackCooldown <= 0f) ){
                attackCooldown = 3f;
                attacked = true;
                StartCoroutine(rushAttack());
            }
        }
    }

    public void Init(SpawnData data)
    {
        anim.runtimeAnimatorController = animCon[data.spriteType];
        speed = data.speed;
        maxHealth = data.health;
        health = maxHealth;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Bullet") || !isLive)
            return;


        health -= collision.GetComponent<Bullet>().damage;
        StartCoroutine(KnockBack());

        if (health > 0)
        {
            anim.SetTrigger("Hit");
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
        }
        else
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
        }

    }

    IEnumerator KnockBack()
    {
        yield return wait; //다음 물리 프레임까지 딜레이
        Vector3 playerPos = GameManager.Instance.player.transform.position;
        Vector3 dirVec = transform.position - playerPos;
        rigid.AddForce(dirVec.normalized * 3, ForceMode2D.Impulse);
    }

    IEnumerator rushAttack(){
        //공격 전 돌진 준비 모션.
        yield return new WaitForSeconds(0.75f);


        //돌진 로직
        Vector2 dirVec = target.position - rigid.position;
        float distance = Vector2.Distance(target.position, rigid.position);
        rigid.velocity = Vector2.zero;
        
        for(int i = 0; i<10; i++){
            Vector2 nextVec = dirVec.normalized * speed * 20 * Time.fixedDeltaTime;
            rigid.MovePosition(rigid.position + nextVec);
            yield return new WaitForSeconds(0.01f);
        }
    }

    void Dead()
    {
        gameObject.SetActive(false);
    }
}
public enum EnemyAttackType
{
    ranged,
    melee,
    rush
}