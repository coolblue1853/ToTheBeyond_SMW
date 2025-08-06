using UnityEngine;
using System.Collections;

public class FlashEffect : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Material _flashMaterial;
    [SerializeField] private float _flashDuration = 0.1f;

    private Material originalMaterial;
    private Coroutine _flashRoutine;

    private void Awake()
    {
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_spriteRenderer != null)
            originalMaterial = _spriteRenderer.material;
    }

    // 적 피격시 피격 효과를 위한 반짝임
    public void TryTriggerFlash()
    {
        // 객체가 파괴되었거나 비활성 상태면 실행하지 않음
        if (this == null || !gameObject.activeInHierarchy || _spriteRenderer == null)
            return;

        TriggerFlash();
    }

    private void TriggerFlash()
    {
        if (_flashRoutine != null)
            StopCoroutine(_flashRoutine);

        _flashRoutine = StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        if (_spriteRenderer == null || _flashMaterial == null || originalMaterial == null)
            yield break;

        _spriteRenderer.material = _flashMaterial;

        float elapsed = 0f;
        while (elapsed < _flashDuration)
        {
            // 실행 도중 객체가 파괴되었을 경우 종료
            if (this == null || _spriteRenderer == null)
                yield break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (_spriteRenderer != null)
            _spriteRenderer.material = originalMaterial;
    }
}