using UnityEngine;

public class HitEffect : MonoBehaviour
{
    public Collider2D particleSpawnArea;  // 파티클 생성 영역용 콜라이더
    public ParticleType hitParticleType = ParticleType.Hit;

    Vector2 GetRandomPointInCollider(Collider2D col)
    {
        Bounds bounds = col.bounds;

        for (int i = 0; i < 10; i++)  // 최대 10번 시도
        {
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float y = Random.Range(bounds.min.y, bounds.max.y);
            Vector2 point = new Vector2(x, y);

            if (col.OverlapPoint(point))
                return point;
        }
        return col.bounds.center;
    }

    public void PlayHitEffect(int count = 1)
    {
        if (particleSpawnArea == null)
        {
            Debug.LogWarning("particleSpawnArea is null!");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Vector2 spawnPos = GetRandomPointInCollider(particleSpawnArea);
            ParticleManager.Instance.Play(hitParticleType, spawnPos, Quaternion.identity);
        }
    }
}
