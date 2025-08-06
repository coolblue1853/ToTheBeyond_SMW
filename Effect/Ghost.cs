using UnityEngine;

public class Ghost : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    private float _fadeTime = 0.5f;
    private float _fadeTimer;
    private Color _startColor;

    public void Init(Sprite sprite, Vector3 position, Vector3 scale, Color color, float lifetime)
    {
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();

        _spriteRenderer.sprite = sprite;
        transform.position = position;
        transform.localScale = scale;

        _startColor = color;
        _fadeTime = lifetime;
        _fadeTimer = 0f;
        _spriteRenderer.color = _startColor;

        gameObject.SetActive(true);
    }

    private void Update()
    {
        _fadeTimer += Time.deltaTime;
        float t = _fadeTimer / _fadeTime;

        if (t >= 1f)
        {
            Destroy(gameObject); // ← 완전 파괴
        }
        else
        {
            Color c = _startColor;
            c.a = Mathf.Lerp(_startColor.a, 0, t);
            _spriteRenderer.color = c;
        }
    }
}
