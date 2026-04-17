using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class SmokeGenerator : MonoBehaviour
{
    [Header("=== 烟雾核心配置 ===")]
    public ParticleSystem smokeParticle;
    public LayerMask sceneObjLayer;

    [Header("=== 生成规则 ===")]
    public int maxSmokeCount = 256;
    public float stepDistance = 1f;
    public float pointCheckRadius = 0.3f; // 扩大检测半径，防止薄墙
    public float duration = 0.5f;

    [Header("=== 子弹吹散 ===")]
    public float blowForce = 10f;
    public float blowRadius = 2.2f;
    public float restoreSpeed = 3.5f;
    public float bulletPushForce = 30f;
    public float laserPushForce = 40f;

    private Vector3[] sphereDirs;
    private int currentCount;
    private ParticleSystem.Particle[] particles = new ParticleSystem.Particle[256];
    private Vector3[] originalPositions = new Vector3[256];
    private HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
    private Queue<Vector3> bfsQueue = new Queue<Vector3>();
    private float generateTimer;

    private void Awake()
    {
        if (smokeParticle == null)
            smokeParticle = GetComponent<ParticleSystem>();

        sphereDirs = Generate3x3CubeDirections();
    }

    private Vector3[] Generate3x3CubeDirections()
    {
        List<Vector3> dirs = new List<Vector3>();
        for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++)
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && y == 0 && z == 0) continue;
                    dirs.Add(new Vector3(x, y, z).normalized);
                }
        return dirs.ToArray();
    }

    private void Start()
    {
     StartGenerate(transform.position,duration);
    }

    public void StartGenerate(Vector3 startPos, float genUseTime)
    {
        currentCount = 0;
        generateTimer = 0f;
        visited.Clear();
        bfsQueue.Clear();

        bfsQueue.Enqueue(startPos);
        visited.Add(Vector3Int.RoundToInt(startPos));
    }

    private void Update()
    {
        if (currentCount >= maxSmokeCount || bfsQueue.Count == 0)
            return;

        generateTimer += Time.deltaTime;
        float totalTime = duration;
        float ratio = generateTimer / totalTime;
        int targetCount = Mathf.Min(maxSmokeCount, Mathf.CeilToInt(ratio * maxSmokeCount));

        // 分批生成，保证均匀
        while (currentCount < targetCount && bfsQueue.Count > 0)
        {
            Vector3 center = bfsQueue.Dequeue();
            ExpandOneStep(center);
        }
    }

    private void ExpandOneStep(Vector3 center)
    {
        foreach (var dir in sphereDirs)
        {
            Vector3 checkDir = dir;
            Vector3 targetPos = center + checkDir * stepDistance;

            Vector3Int gridPos = Vector3Int.RoundToInt(targetPos);
            if (visited.Contains(gridPos)) continue;

            // 检测1：射线路径是否被阻挡
            if (Physics.Raycast(center, checkDir, stepDistance, sceneObjLayer))
            {
                visited.Add(gridPos);
                continue;
            }

            // 检测2：目标点本身是否在墙体内
            if (Physics.OverlapSphere(targetPos, pointCheckRadius, sceneObjLayer).Length > 0)
            {
                visited.Add(gridPos);
                continue;
            }

            // 检测3：反向射线，判断是否能回到起点（防止在封闭空间外生成）
            if (Physics.Raycast(targetPos, -checkDir, stepDistance, sceneObjLayer))
            {
                visited.Add(gridPos);
                continue;
            }

            // 三重检测都通过，才生成烟雾
            EmitSmoke(targetPos, 1f);
            originalPositions[currentCount] = targetPos;
            currentCount++;
            visited.Add(gridPos);

            if (currentCount < maxSmokeCount)
                bfsQueue.Enqueue(targetPos);
        }
    }

    private void EmitSmoke(Vector3 pos, float size)
    {
        ParticleSystem.EmitParams p = new ParticleSystem.EmitParams();
        p.position = pos;
        p.startSize = size;
        p.startColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
        smokeParticle.Emit(p, 1);
    }

    private void LateUpdate()
    {
        int count = smokeParticle.GetParticles(particles);

        for (int i = 0; i < count; i++)
        {
            if (i >= currentCount) continue;
            Vector3 backDir = originalPositions[i] - particles[i].position;
            if (backDir.magnitude > 0.05f)
            {
                // 恢复速度降低，更慢更自然
                particles[i].velocity = backDir.normalized * 0.8f;
            }
            else
            {
                particles[i].velocity = Vector3.zero;
                particles[i].position = originalPositions[i];
            }
        }
        smokeParticle.SetParticles(particles, count);
    }

    public void OnBulletHit(Vector3 bulletPos, Vector3 bulletDir)
    {
        int count = smokeParticle.GetParticles(particles);

        for (int i = 0; i < count; i++)
        {
            // 扩大范围！让你一定看得到效果
            float dist = Vector3.Distance(particles[i].position, bulletPos);

            if (dist < 1.2f) // 更大范围
            {
                Vector3 dir = (particles[i].position - bulletPos).normalized;
                // 超级强力！
                particles[i].velocity = dir * bulletPushForce;
            }
        }

        smokeParticle.SetParticles(particles, count);
    }

    // 激光：持续沿线段推开烟雾（调用方每帧调用）
    public void OnLaserHit(Vector3 laserStart, Vector3 laserEnd)
    {
        int count = smokeParticle.GetParticles(particles);
        Vector3 dir = (laserEnd - laserStart).normalized;
        float length = Vector3.Distance(laserStart, laserEnd);

        for (int i = 0; i < count; i++)
        {
            // 粒子到激光线段的距离
            float dist = DistancePointToLineSegment(particles[i].position, laserStart, laserEnd);

            if (dist < 0.3f) // 激光宽度，越窄越精细
            {
                // 计算垂直于激光的推开方向
                Vector3 proj = Vector3.Project(particles[i].position - laserStart, dir);
                Vector3 perp = (particles[i].position - (laserStart + proj)).normalized;
                particles[i].velocity = perp * laserPushForce; // 持续推开力
            }
        }

        smokeParticle.SetParticles(particles, count);
    }

    // 工具：点到线段的距离（必须要有）
    private float DistancePointToLineSegment(Vector3 point, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        Vector3 ap = point - a;
        float t = Mathf.Clamp01(Vector3.Dot(ap, ab) / Vector3.Dot(ab, ab));
        return Vector3.Distance(point, a + t * ab);
    }

    public void ClearAllSmoke()
    {
        smokeParticle.Clear();
        currentCount = 0;
        visited.Clear();
        bfsQueue.Clear();
    }
}