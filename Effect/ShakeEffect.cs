using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeEffect : MonoBehaviour
{
    private Vector3 _initialLocalPos;
    private bool _initialized = false;
    private Coroutine _shakeCoroutine;

    public void Shake(float duration = 0.15f, float magnitude = 0.1f)
    {
        // 최초 1회만 기준 위치 저장
        if (!_initialized)
        {
            _initialLocalPos = transform.localPosition;
            _initialized = true;
        }

        if (_shakeCoroutine != null)
            StopCoroutine(_shakeCoroutine);

        _shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = _initialLocalPos + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = _initialLocalPos;
    }
}
