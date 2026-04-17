using UnityEngine;

public class ulletMove : MonoBehaviour
{
    [Header("烟雾检测")]
    public float checkRange = 1f;
    public float holeRadius = 2.2f;

    private void Update()
    {
        // 【不检测粒子】而是找附近的烟雾发生器
        Collider[] cols = Physics.OverlapSphere(transform.position, checkRange);

        foreach (var col in cols)
        {
            SmokeGenerator gen = col.GetComponentInParent<SmokeGenerator>();
            if (gen != null)
            {
                // 在子弹当前位置打穿烟雾
                gen.OnBulletHit(transform.position, transform.forward);
                // 可选：穿一次就消失
                // Destroy(gameObject);
                break;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, checkRange);
    }
}