using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DebuffIconUI : MonoBehaviour
{
    // 디버프 UI 관리 
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text stackText;
    [SerializeField] private Animator _animator;

    private float _remainingDuration;
    private bool _isTracking = false;
    private Transform _followTarget;

    // 외부 주입 
    public void Initialize(DebuffInfo info)
    {
        iconImage.sprite = info.icon;
        _remainingDuration = info.duration;
        _isTracking = true;

        if(info.animatorController != null)
            _animator.runtimeAnimatorController = info.animatorController;

        if (stackText != null)
            stackText.gameObject.SetActive(false);
    }

    // 디버프 재적용시 남은 시간 갱신 
    public void RefreshDuration(float newDuration)
    {
        _remainingDuration = newDuration;
    }

    // 스택형인 경우 스택 증가 
    public void UpdateStack(int count)
    {
        if (stackText == null) return;

        stackText.text = count.ToString();
        stackText.gameObject.SetActive(count > 1);
    }

    public void FollowTarget(Transform target)
    {
        _followTarget = target;
    }


    // 등장 위치를 위한 안전망 
    public IEnumerator CorrectInitialPosition()
    {
        yield return new WaitForEndOfFrame();

        if (_followTarget != null)
        {
            transform.position = _followTarget.position + Vector3.up * 1.5f;
        }
    }

    private void Update()
    {
        if (_followTarget != null)
        {
            Vector3 newPos = _followTarget.position + Vector3.up * 1.5f;

            if (Vector3.Distance(transform.position, newPos) < 50f)
            {
                transform.position = newPos;
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (!_isTracking) return;

        _remainingDuration -= Time.deltaTime;
        if (_remainingDuration <= 0f)
        {
            _isTracking = false;
            Destroy(gameObject);
        }
    }
}