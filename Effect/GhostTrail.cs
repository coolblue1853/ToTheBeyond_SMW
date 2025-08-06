using UnityEngine;

public class GhostTrail : MonoBehaviour
{
    [SerializeField] private GameObject _ghostPrefab;
    [SerializeField] private float _spawnInterval = 0.05f;
    [SerializeField] private float _ghostLifetime = 0.3f;
    [SerializeField] private Color _ghostColor = new Color(1, 1, 1, 0.5f);

    [SerializeField] private SpriteRenderer playerSprite;
    private float timer = 0f;
    private bool isDashing = false;

    private void Start()
    {
    }

    private void Update()
    {
        if (!isDashing) return;

        timer += Time.deltaTime;
        if (timer >= _spawnInterval)
        {
            SpawnGhost();
            timer = 0f;
        }
    }

    // 고스트 생성
    private void SpawnGhost()
    {
        GameObject ghost = Instantiate(_ghostPrefab);
        ghost.GetComponent<Ghost>().Init(
            playerSprite.sprite,
            transform.position,
            transform.localScale, // ← 이게 핵심
            _ghostColor,
            _ghostLifetime
        );
    }


    public void StartTrail() => isDashing = true;
    public void StopTrail() => isDashing = false;
}
