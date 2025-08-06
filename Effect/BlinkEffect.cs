using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkEffect : MonoBehaviour
{
    [Header("블링크 설정")]
    [SerializeField] private float blinkDuration = 0.25f; // 한 번 깜빡이는 주기
    [SerializeField] private float minAlpha = 0.1f;      // 최소 알파값

    private Coroutine blinkRoutine;
    private List<SpriteRenderer> _renderers = new();
    private List<Color> _originalColors = new();

    private void Awake()
    {
        // PlayerSpriteController에서 SpriteRenderer들을 가져옴
        var playerSpriteController = GetComponent<PlayerSpriteController>();
        if (playerSpriteController == null)
        {
            return;
        }

        _renderers = playerSpriteController.GetAllSpriteRenderers();

        foreach (var renderer in _renderers)
        {
            _originalColors.Add(renderer.color);
        }
    }

    public void StartBlink(float duration)
    {
        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        blinkRoutine = StartCoroutine(BlinkCoroutine(duration));
    }

    private IEnumerator BlinkCoroutine(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = 0f;

            while (t < blinkDuration && elapsed < duration)
            {
                float alpha = Mathf.Lerp(1f, minAlpha, Mathf.PingPong(t * 2f / blinkDuration, 1f));

                for (int i = 0; i < _renderers.Count; i++)
                {
                    var renderer = _renderers[i];
                    if (renderer != null)
                    {
                        Color c = _originalColors[i];
                        c.a = alpha;
                        renderer.color = c;
                    }
                }

                t += Time.deltaTime;
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        // 알파 복구
        for (int i = 0; i < _renderers.Count; i++)
        {
            if (_renderers[i] != null)
            {
                Color c = _originalColors[i];
                c.a = 1f;
                _renderers[i].color = c;
            }
        }

        blinkRoutine = null;
    }
}
