using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("移動速度"), Range(0, 1000)]
    public float speed = 10;
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

    /// <summary>
    /// 是否在地面上
    /// </summary>
    private bool isGround;
    private Animator ani;
    private Rigidbody rig;
    private Transform cam;
    private float x;
    private float y;

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
    }

    private void Update()
    {
        Move();
        TurnCamera();
    }

    /// <summary>
    /// 固定更新事件：50FPS
    /// </summary>
    private void FixedUpdate()
    {
        Jump();
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

        rig.velocity = ((camNew.forward * v + camNew.right * h) * speed * Time.deltaTime) + transform.up * rig.velocity.y;          // 加速度 = 前方 * 前後值 + 右方 * 左右值 * 速度 * 1/60 + 上方 * 加速度上下值

        ani.SetBool("跑步開關", rig.velocity.magnitude > 0.1f);             // 動畫.設定布林值("參數名稱"，剛體.加速度.值 > 0)

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
        if (Input.GetKeyDown(KeyCode.Space) && isGround)    // 如果 按下 空白建 並且 在地面上
        {
            rig.AddForce(Vector3.up * jump);                // 推力
        }

        // 碰撞物件陣列 = 物理.球體碰撞範圍(中心點，半徑，圖層)
        Collider[] hit = Physics.OverlapSphere(transform.position + offset, radius, 1 << 8);

        if (hit.Length > 0 && hit[0]) isGround = true;          // 如果 碰到物件陣列數量 > 0 並且 存在 就設定為在地面上
        else isGround = false;                                  // 球體沒碰到地面 就設定為 不在地面上
    }
}
