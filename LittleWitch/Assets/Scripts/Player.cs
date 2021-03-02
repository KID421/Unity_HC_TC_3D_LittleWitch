using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Player : MonoBehaviour
{
    #region 基本參數
    [Header("跑步移動速度"), Range(0, 1000)]
    public float speed = 10;
    [Header("走路移動速度"), Range(0, 1000)]
    public float speedWalk = 10;
    [Header("跳躍高度"), Range(0, 1000)]
    public float jump = 10;
    [Header("攝影機旋轉速度"), Range(0, 1000)]
    public float turn = 10;
    [Header("攝影機角度限制")]
    public Vector2 camLimit = new Vector2(-30, 0);
    [Header("角色旋轉速度"), Range(0, 1000)]
    public float turnSpeed = 10;
    [Header("檢查地板球體半徑")]
    public float radius = 1f;
    [Header("檢查地板球體位移")]
    public Vector3 offset;
    [Header("跳躍次數限制")]
    public int jumpCountLimit = 2;

    [Header("血量"), Range(0, 5000)]
    public float hp = 100;
    private float hpMax;
    [Header("魔力"), Range(0, 5000)]
    public float mp = 500;
    private float mpMax;
    [Header("體力"), Range(0, 5000)]
    public float ps = 200;
    private float psMax;

    [Header("吧條")]
    public Image barHp;
    public Image barMp;
    public Image barPS;

    [Header("移動時每秒扣除體力"), Range(0, 5000)]
    public float psMove = 1;
    [Header("跳躍時扣除體力"), Range(0, 5000)]
    public float psJump = 5;
    [Header("停止時每秒恢復體力"), Range(0, 5000)]
    public float psRecover = 10;

    /// <summary>
    /// 跳躍次數
    /// </summary>
    private int jumpCount;
    /// <summary>
    /// 是否在地面上
    /// </summary>
    private bool isGround;
    private Animator ani;
    private Rigidbody rig;
    private Transform cam;
    private float x;
    private float y;
    /// <summary>
    /// 結束畫面 群組元件
    /// </summary>
    private CanvasGroup final;
    #endregion

    #region 攻擊參數
    [Header("生成攻擊特效位置")]
    public Transform attackPoint;
    [Header("攻擊特效")]
    public GameObject attackPS;
    [Header("攻擊特效速度"), Range(0, 2000)]
    public float attackSpeed = 500;
    [Header("攻擊力"), Range(0, 500)]
    public float attack = 50;
    [Header("攻擊魔力消耗"), Range(0, 500)]
    public float attackCost = 10;
    [Header("生成攻擊特效延遲"), Range(0f, 1f)]
    public float attackPSDelay = 0.15f;
    [Header("生成後多久可進行下次攻擊"), Range(0f, 5f)]
    public float attackDelay = 2f;

    /// <summary>
    /// 是否攻擊中
    /// </summary>
    private bool attacking;
    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawSphere(transform.position + offset, radius);
    }

    //喚醒事件：在 Start 前執行一次
    private void Awake()
    {
        ani = GetComponent<Animator>();
        rig = GetComponent<Rigidbody>();
        cam = GameObject.Find("攝影機根物件").transform;
        final = GameObject.Find("結束畫面").GetComponent<CanvasGroup>();

        hpMax = hp;
        mpMax = mp;
        psMax = ps;
    }

    private void Update()
    {
        if (attacking) return;

        Move();
        TurnCamera();
        Jump();
        PSSystem();
        Attack();

        // 測試用：扣血
        if (Input.GetKeyDown(KeyCode.Alpha1)) Cure(-10);
    }

    /// <summary>
    /// 固定更新事件：50FPS
    /// </summary>
    private void FixedUpdate()
    {
        
    }

    /// <summary>
    /// 攻擊
    /// </summary>
    private void Attack()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && mp >= attackCost && !attacking)                 // 按下左鍵 並且 魔力 大於等於 攻擊魔力消耗 並且 不是攻擊中
        {
            StartCoroutine(AttackTimeControl());
        }
    }

    /// <summary>
    /// 攻擊時間控制
    /// </summary>
    private IEnumerator AttackTimeControl()
    {
        rig.velocity = Vector3.zero;
        attacking = true;                                                                       // 正在攻擊中
        mp -= attackCost;                                                                       // 扣除消耗
        barMp.fillAmount = mp / mpMax;                                                          // 更新介面

        ani.SetTrigger("攻擊觸發");

        yield return new WaitForSeconds(attackPSDelay);                                         // 延遲生成

        GameObject temp = Instantiate(attackPS, attackPoint.position, attackPoint.rotation);    // 生成攻擊特效在位置上
        temp.GetComponent<Rigidbody>().AddForce(transform.forward * attackSpeed);               // 取得攻擊特效並添加推力
        temp.GetComponent<Magic>().attack = attack;

        yield return new WaitForSeconds(attackDelay);                                           // 延遲再次攻擊
        attacking = false;                                                                      // 不是在攻擊
    }

    /// <summary>
    /// 移動方法
    /// </summary>
    private void Move()
    {
        float v = Input.GetAxis("Vertical");                                // 取得 前後軸 值 W S 上 下
        float h = Input.GetAxis("Horizontal");                              // 取得 前後軸 值 A D 左 右

        Transform camNew = cam;                                             // 新攝影機座標資訊
        camNew.eulerAngles = new Vector3(0, cam.eulerAngles.y, 0);          // 去掉 X 與 Z 角度

        transform.rotation = Quaternion.Lerp(transform.rotation, camNew.rotation, 0.5f * turnSpeed * Time.deltaTime);               // 角色的角度 = 角色，攝影機 角度的插值

        if (ps > 0)
        {
            rig.velocity = ((camNew.forward * v + camNew.right * h) * speed * Time.deltaTime) + transform.up * rig.velocity.y;          // 加速度 = 前方 * 前後值 + 右方 * 左右值 * 速度 * 1/60 + 上方 * 加速度上下值
            ani.SetBool("跑步開關", rig.velocity.magnitude > 0.1f);             // 動畫.設定布林值("參數名稱"，剛體.加速度.值 > 0)
        }
        else
        {
            rig.velocity = ((camNew.forward * v + camNew.right * h) * speedWalk * Time.deltaTime) + transform.up * rig.velocity.y;
            ani.SetBool("走路開關", rig.velocity.magnitude > 0.1f);
            ani.SetBool("跑步開關", false);
        }
    }

    /// <summary>
    /// 旋轉攝影機
    /// </summary>
    private void TurnCamera()
    {
        x += Input.GetAxis("Mouse X") * turn * Time.deltaTime;  // 取得滑鼠 X 值
        y -= Input.GetAxis("Mouse Y") * turn * Time.deltaTime;  // 取得滑鼠 Y 值
        y = Mathf.Clamp(y, camLimit.x, camLimit.y);             // 限制 Y
        cam.localEulerAngles = new Vector3(y , x, 0);           // 攝影機.角度 = (Y 值，X 值，0) * 旋轉速度 * 1/60
    }

    /// <summary>
    /// 跳躍
    /// </summary>
    private void Jump()
    {
        // 碰撞物件陣列 = 物理.球體碰撞範圍(中心點，半徑，圖層)
        Collider[] hit = Physics.OverlapSphere(transform.position + offset, radius, 1 << 8);

        if (hit.Length > 0 && hit[0])
        {
            isGround = true;                                    // 如果 碰到物件陣列數量 > 0 並且 存在 就設定為在地面上
            jumpCount = 0;
        }
        else isGround = false;                                  // 球體沒碰到地面 就設定為 不在地面上

        ani.SetBool("是否在地面上", isGround);

        if (ps < psJump) return;    // 如果 體力 < 跳躍需要體力 就跳出

        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < jumpCountLimit - 1)    // 如果 按下 空白建 並且 在地面上
        {
            jumpCount++;
            rig.Sleep();                                    // 睡著
            rig.WakeUp();                                   // 醒來
            rig.AddForce(Vector3.up * jump);                // 推力
            ani.SetTrigger("跳躍觸發");

            // 跳躍扣除體力
            ps -= psJump;
            barPS.fillAmount = ps / psMax;
        }
    }

    /// <summary>
    /// 體力系統
    /// </summary>
    private void PSSystem()
    {
        if (ani.GetBool("跑步開關"))
        {
            ps -= psMove * Time.deltaTime;
            barPS.fillAmount = ps / psMax;
        }
        else if (!ani.GetBool("走路開關"))
        {
            ps += psRecover * Time.deltaTime;
            barPS.fillAmount = ps / psMax;
        }

        ps = Mathf.Clamp(ps, 0, psMax);
    }

    /// <summary>
    /// 治癒
    /// </summary>
    /// <param name="cureValue">要治癒的值</param>
    public void Cure(float cureValue)
    {
        hp += cureValue;                    // 補血
        hp = Mathf.Clamp(hp, 0, hpMax);     // 夾住 0 ~ 最大值
        barHp.fillAmount = hp / hpMax;      // 更新血條
    }

    /// <summary>
    /// 受傷
    /// </summary>
    /// <param name="getDamage">接收到的傷害值</param>
    public void Damage(float getDamage)
    {
        ani.SetTrigger("受傷觸發");
        hp -= getDamage;
        barHp.fillAmount = hp / hpMax;

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
        StartCoroutine(ShowFinal());
    }

    private IEnumerator ShowFinal()
    {
        final.interactable = true;                      // 可以互動
        final.blocksRaycasts = true;                    // 開啟遮擋 - 讓滑鼠可以點到

        float a = final.alpha;                          // 取得透明度

        // while(布林值) { 程式區塊 }

        while (a < 1)                                   // 當 透明度 小於 1 時持續執行
        {
            a += 0.1f;
            final.alpha = a;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
