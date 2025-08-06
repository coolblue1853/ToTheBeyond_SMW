using UnityEngine;

public enum ParticleType
{
    WalkDust,
    JumpDust,
    LandDust,
    Hit,
}

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance;

    [System.Serializable]
    public class ParticleData
    {
        public ParticleType type;
        public ParticleSystem prefab;
    }

    [SerializeField] private ParticleData[] _particles;

    private void Awake()
    {
        Instance = this;
    }

    public void Play(ParticleType type, Vector3 position, Quaternion rotation = default)
    {
        var prefab = GetParticlePrefab(type);
        if (prefab == null) return;

        ParticleSystem ps = Instantiate(prefab, position, rotation);
        ps.Play();
        Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
    }

    private ParticleSystem GetParticlePrefab(ParticleType type)
    {
        foreach (var p in _particles)
        {
            if (p.type == type)
                return p.prefab;
        }
        return null;
    }
}
