using UnityEngine;

public class PropManager : MonoBehaviour
{
    [Header("寶箱關閉")]
    public GameObject objClose;
    [Header("寶箱開啟")]
    public GameObject objOpen;
    [Header("面向角度範圍")]
    public float faceRange = 10;

    private bool playerIn;
    private Transform player;

    /// <summary>
    /// 打開道具
    /// </summary>
    private void OpenProp()
    {
        // Vector3.Angle(向量 1，向量 2)
        // 取得兩條向量的夾角
        if (playerIn && Input.GetKeyDown(KeyCode.Mouse0) && Vector3.Angle(player.forward, transform.position - player.position) < faceRange)
        {
            objClose.SetActive(false);
            objOpen.SetActive(true);
        }
    }

    private void Awake()
    {
        player = GameObject.Find("玩家").transform;
    }

    private void Update()
    {
        OpenProp();
    }

    // 進入觸發區域
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "玩家") playerIn = true;
    }

    // 離開觸發區域
    private void OnTriggerExit(Collider other)
    {
        if (other.name == "玩家") playerIn = false;
    }
}
