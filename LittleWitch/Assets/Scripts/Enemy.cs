using UnityEngine;
using UnityEngine.AI;       // 引用 人工智慧 API
using System.Collections;

public class Enemy : MonoBehaviour
{
    #region 基本欄位
    [Header("追蹤玩家範圍"), Range(0, 100)]
    public float rangeTrack = 5;
    [Header("移動速度"), Range(0, 100)]
    public float speed = 3;
    [Header("攻擊玩家範圍"), Range(0, 100)]
    public float rangeAttack = 3;
    [Header("攻擊冷卻時間"), Range(0, 10)]
    public float attackCD = 2.5f;
    [Header("攻擊球體半徑"), Range(0, 10)]
    public float attackRadius = 1f;
    [Header("攻擊球體位移")]
    public Vector3 attackOffset;
    [Header("攻擊延遲對玩家造成傷害"), Range(0, 2)]
    public float attackDelay = 0.8f;
    [Header("攻擊力"), Range(0, 1000)]
    public float attack = 30;
    [Header("血量"), Range(0, 10000)]
    public float hp = 500;

    private Animator ani;
    private Transform player;
    private NavMeshAgent nma;
    private float timer;
    #endregion

    private void Awake()
    {
        ani = GetComponent<Animator>();
        nma = GetComponent<NavMeshAgent>();

        nma.stoppingDistance = rangeAttack;

        player = GameObject.Find("玩家").transform;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, rangeTrack);

        Gizmos.color = new Color(0, 0, 1, 0.3f);
        Gizmos.DrawSphere(transform.position, rangeAttack);

        // 攻擊球體
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawSphere(transform.position + transform.right * attackOffset.x + transform.up * attackOffset.y + transform.forward * attackOffset.z, attackRadius);
    }

    private void Update()
    {
        // 如果 正在攻擊中 就 跳出
        if (ani.GetCurrentAnimatorStateInfo(0).IsName("攻擊")) return;

        Track();
    }

    /// <summary>
    /// 追蹤
    /// </summary>
    private void Track()
    {
        // 距離 = 三維向量.距離(A 點，B 點)
        float dis = Vector3.Distance(player.position, transform.position);

        if (dis <= rangeAttack)
        {
            Attack();
        }
        else if (dis <= rangeTrack)
        {
            // 代理器.設定目的地(玩家.座標)
            nma.SetDestination(player.position);
            nma.isStopped = false;
            ani.SetBool("走路開關", true);
        }
        else
        {
            nma.isStopped = true;
            ani.SetBool("走路開關", false);
        }
    }

    /// <summary>
    /// 攻擊
    /// </summary>
    private void Attack()
    {
        if (timer >= attackCD)              // 如果 計時器 >= 冷卻
        {
            timer = 0;
            ani.SetTrigger("攻擊觸發");
            StartCoroutine(DelayAttack());  // 延遲造成玩家傷害
        }
        else
        {
            timer += Time.deltaTime;        // 累加時間

            Vector3 pos = player.position;  // 取得玩家座標
            pos.y = transform.position.y;   // 將座標 Y 軸 改為怪物的 Y 軸
            transform.LookAt(pos);          // 面向(修改後的座標)
        }

        nma.isStopped = true;
        ani.SetBool("走路開關", false);
    }

    /// <summary>
    /// 延遲攻擊
    /// </summary>
    private IEnumerator DelayAttack()
    {
        yield return new WaitForSeconds(attackDelay);

        // 碰撞陣列 = 物理.覆蓋球體(座標 + 位移，半徑)
        Collider[] hits = Physics.OverlapSphere(transform.position + transform.right * attackOffset.x + transform.up * attackOffset.y + transform.forward * attackOffset.z, attackRadius, 1 << 9);

        if (hits.Length > 0)
        {
            hits[0].GetComponent<Player>().Damage(attack);
        }
    }

    /// <summary>
    /// 受傷
    /// </summary>
    /// <param name="getDamage">接收到的傷害值</param>
    public void Damage(float getDamage)
    {
        ani.SetTrigger("受傷觸發");
        hp -= getDamage;

        if (hp <= 0) Dead();
    }

    /// <summary>
    /// 死亡
    /// </summary>
    private void Dead()
    {
        hp = 0;
        ani.SetBool("死亡開關", true);
        enabled = false;
    }
}
