using UnityEngine;

public class Magic : MonoBehaviour
{
    /// <summary>
    /// 玩家施放的技能攻擊力
    /// </summary>
    public float attack;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "怪物") other.gameObject.GetComponent<Enemy>().Damage(attack);

        Destroy(gameObject);
    }
}
