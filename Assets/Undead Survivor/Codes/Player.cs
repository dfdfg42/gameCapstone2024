using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    public float speed;
    public float dashDistance = 10f; // 대시 거리
    public float dashCooldown = 0.5f; // 대시 쿨타임
    public Scanner scanner;
    public Hand[] hands;
    public RuntimeAnimatorController[] animCon;
    public GameObject bulletPrefab; // Bullet 프리팹을 연결
    public GameObject ghostPrefab; // 잔상 고스트 프리팹

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;

    private bool canDash = true;
    private Vector2 lastMoveDir; // 마지막으로 움직인 방향

    private List<Vector2> dashPositions = new List<Vector2>(); // 미래의 이동 위치를 저장
    private List<GameObject> futureGhosts = new List<GameObject>(); // 미래 경로를 보여주는 고스트들을 저장
    private bool isChronoActive = false;
    private int maxDashCount = 5; // 최대 5번의 이동

    private Color[] rainbowColors = new Color[] // 무지개색 정의
    {
        new Color(1f, 0f, 0f, 0.5f), // Red
        new Color(1f, 0.5f, 0f, 0.5f), // Orange
        new Color(1f, 1f, 0f, 0.5f), // Yellow
        new Color(0f, 1f, 0f, 0.5f), // Green
        new Color(0f, 0f, 1f, 0.5f), // Blue
        new Color(0.29f, 0f, 0.51f, 0.5f), // Indigo
        new Color(0.56f, 0f, 1f, 0.5f)  // Violet
    };

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        scanner = GetComponent<Scanner>();
        hands = GetComponentsInChildren<Hand>(true);

    }

    void OnEnable()
    {
        speed *= Character.Speed;
        anim.runtimeAnimatorController = animCon[GameManager.Instance.playerId];
    }
    private void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            OnDash();
        }
    }

    void FixedUpdate()
    {
        if (!GameManager.Instance.isLive || isChronoActive)
            return;

        if (inputVec != Vector2.zero)
        {
            lastMoveDir = inputVec; // 마지막으로 움직인 방향을 저장
        }

        Vector2 nextVec = inputVec * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
    }

    void OnMove(InputValue value)
    {
        inputVec = value.Get<Vector2>();
    }

    void LateUpdate()
    {
        if (!GameManager.Instance.isLive)
            return;

        anim.SetFloat("Speed", inputVec.magnitude);

        if (inputVec.x != 0)
        {
            spriter.flipX = inputVec.x < 0;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!GameManager.Instance.isLive)
            return;

        Enemy enemy = collision.gameObject.GetComponent<Enemy>();

        //기본 몬스터의 근접 공격
        if (enemy != null){
            GameManager.Instance.health -= Time.deltaTime * 10;
        }
        if (GameManager.Instance.health < 0)
        {
            this.onDeath();
        }
    }

    public void onDeath(){
        for (int index = 2; index < transform.childCount; index++){
            transform.GetChild(index).gameObject.SetActive(false);
        }

        anim.SetTrigger("Dead");
        GameManager.Instance.GameOver();
    }

    public void OnDash()
    {
        if (canDash)
        {
            StartCoroutine(Dash());
        }
    }

    IEnumerator Dash()
    {
        canDash = false;

        Vector2 startPosition = rigid.position;

        // 플레이어의 입력 방향에 따라 대쉬 방향 결정
        if (inputVec != Vector2.zero)
        {
            lastMoveDir = inputVec.normalized; // 마지막 입력 방향을 저장
        }
        else
        {
            lastMoveDir = lastMoveDir.normalized; // 입력이 없으면 마지막 방향 유지
        }

        Vector2 dashPosition = startPosition + lastMoveDir * dashDistance;

        // 적이 경로에 있다면 시간을 멈춘다.
        if (CheckForEnemiesInPath(startPosition, dashPosition))
        {
            isChronoActive = true;
            Time.timeScale = 0f; // 시간 정지
            dashPositions.Clear(); // 대쉬 경로 초기화
            ClearFutureGhosts(); // 기존 미래 경로 고스트 제거
            dashPositions.Add(dashPosition);
            CreateFutureGhost(dashPosition, 0); // 첫 번째 고스트 생성
            StartCoroutine(ChronoDash());
            yield break; // 이후 코루틴을 종료하고, 플레이어가 이동 경로를 지정하게 한다.
        }
        else
        {
            // 적이 없는 경우도 대쉬 포지션 리스트에 추가
            dashPositions.Clear();
            dashPositions.Add(dashPosition);
            StartCoroutine(ExecuteDashWithDelay()); // 0.3초 후 대쉬 시퀀스 실행
        }

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    IEnumerator ChronoDash()
    {
        int dashCount = 1;

        while (dashCount < maxDashCount)
        {
            // 플레이어의 추가 입력을 대기
            yield return new WaitUntil(() => Input.GetButtonDown("Jump"));

            // 플레이어의 입력 방향에 따라 대쉬 방향 결정
            if (inputVec != Vector2.zero)
            {
                lastMoveDir = inputVec.normalized; // 마지막 입력 방향을 저장
            }

            Vector2 nextDashPosition = dashPositions[dashCount - 1] + lastMoveDir * dashDistance;
            dashPositions.Add(nextDashPosition);

            // 미래 경로 고스트 생성 (무지개색 적용)
            CreateFutureGhost(nextDashPosition, dashCount % rainbowColors.Length);

            dashCount++;
        }

        // 시간 재개 및 빠르게 이동 시작
        Time.timeScale = 1f;
        isChronoActive = false;

        yield return new WaitForSeconds(0.3f); // 0.3초 딜레이 후 대쉬 실행
        StartCoroutine(PerformDashSequenceWithTrails());

        ClearFutureGhosts(); // 대쉬 후 미래 경로 고스트 제거
        canDash = true;
    }

    IEnumerator PerformDashSequenceWithTrails()
    {
        // 이전 대쉬 포지션
        Vector2 previousPosition = rigid.position;

        foreach (var dashPosition in dashPositions)
        {
            // 두 지점 사이의 중간 지점을 계산
            Vector2 dashDirection = (dashPosition - previousPosition).normalized;

            // 고스트 잔상 생성
            CreateTrailGhost(previousPosition, dashDirection);

            // Bullet 생성
            CreateBullet(previousPosition, dashPosition);

            // 리지드바디 이동
            rigid.position = dashPosition;

            // 짧은 대기 (잔상이 남는 속도 조정)
            yield return new WaitForSeconds(0.05f);

            // 다음 대쉬 구간을 위해 현재 위치를 저장
            previousPosition = dashPosition;
        }
    }

    void CreateBullet(Vector2 startPosition, Vector2 endPosition)
    {
        GameObject bullet = Instantiate(bulletPrefab); // Bullet 생성
        Bullet bulletComponent = bullet.GetComponent<Bullet>();

        // Bullet이 null이 아닌 경우에만 Init 호출
        if (bulletComponent != null)
        {
            bulletComponent.Init(10f, -100, lastMoveDir);
        }

        // 두 지점 사이의 중간 지점을 계산
        Vector2 bulletCenter = (startPosition + endPosition) / 2;

        // Bullet 오브젝트의 회전 설정
        Vector2 dashDirection = (endPosition - startPosition).normalized;
        bullet.transform.right = dashDirection;

        // Bullet 오브젝트의 크기와 위치 조정
        float distance = Vector2.Distance(startPosition, endPosition);
        bullet.transform.position = bulletCenter;
        bullet.transform.localScale = new Vector3(distance, 0.1f, bullet.transform.localScale.z); // 두께를 0.1로 설정하여 얇게 만듦
    }

    void CreateFutureGhost(Vector2 position, int colorIndex)
    {
        GameObject ghost = Instantiate(ghostPrefab, position, Quaternion.identity);
        SpriteRenderer ghostRenderer = ghost.GetComponent<SpriteRenderer>();

        // 고스트에 무지개색 적용 (은은하게)
        ghostRenderer.color = rainbowColors[colorIndex];

        futureGhosts.Add(ghost);
    }

    void ClearFutureGhosts()
    {
        foreach (var ghost in futureGhosts)
        {
            Destroy(ghost);
        }
        futureGhosts.Clear();
    }

    void CreateTrailGhost(Vector2 position, Vector2 direction)
    {
        GameObject ghost = Instantiate(ghostPrefab, position, Quaternion.identity);
        SpriteRenderer ghostRenderer = ghost.GetComponent<SpriteRenderer>();

        // 고스트에 방향에 따라 약간의 회전 적용
        ghost.transform.right = direction;

        // 고스트를 일정 시간 후에 파괴
        Destroy(ghost, 0.3f); // 0.3초 후에 잔상 제거
    }

    IEnumerator ExecuteDashWithDelay()
    {
        yield return new WaitForSeconds(0.3f); // 0.3초 딜레이
        StartCoroutine(PerformDashSequenceWithTrails());
    }

    bool CheckForEnemiesInPath(Vector2 start, Vector2 end)
    {
        // 두 점 사이의 직선 경로에 적이 있는지 체크하는 로직 추가
        // Raycast나 Collider2D의 Overlap 등을 활용할 수 있음
        // 예시: RaycastHit2D hit = Physics2D.Linecast(start, end, enemyLayerMask);
        // 적이 맞으면 true 반환
        return true; // 예시용으로 무조건 true를 반환하도록 설정, 실제로는 체크 로직 구현 필요
    }
}
