using UnityEngine;

public class Enemy : MonoBehaviour
{
    #region 基本欄位
    [Header("追蹤玩家範圍"), Range(0, 100)]
    public float rangeTrack = 5;
    [Header("移動速度"), Range(0, 100)]
    public float speed = 3;

    private Animator ani;
    #endregion

    private void Awake()
    {
        ani = GetComponent<Animator>();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, rangeTrack);
    }
}
