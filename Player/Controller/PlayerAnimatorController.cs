using System.Collections;
using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    [SerializeField] private Animator _bodyAnimator; // 상체 애니메이터 
    [SerializeField] private Animator _legAnimator;  // 하체 애니메이터

    private string _currentUpperBodyState;
    private string _currentLegState;

    private Coroutine _upperBodyResetRoutine;
    private bool _isReloading = false;
    public bool isActing = false;

    // 재장전 애니메이션 
    public void SetReloading(bool value, string idleStr = "")
    {
        _isReloading = value;
        if(idleStr != "" && value != true)
            PlayUpperBodyAnimation(idleStr);
    }

    // 상채 애니메이션 출력 
    public void PlayUpperBodyAnimation(string clipName, float playbackSpeed = 1f)
    {
        if (string.IsNullOrEmpty(clipName)) return;

        if (_currentUpperBodyState != clipName)
        {
            _bodyAnimator.Play(clipName, 0, 0f);
            _currentUpperBodyState = clipName;
        }

        _bodyAnimator.speed = playbackSpeed;
    }

    // 자동 초기화가 있는 상체 애니메이션 출력 
    public void PlayUpperBodyAttackWithAutoReset(string animName, string idleAnimName, float attackInterval)
    {
        if (_isReloading) return; 

        if (string.IsNullOrEmpty(animName) || string.IsNullOrEmpty(idleAnimName)) return;

        float clipLength = GetAnimationClipLength(animName);
        float speed = Mathf.Clamp(clipLength / attackInterval, 0.1f, 10f);

        _bodyAnimator.Play(animName, 0, 0f);
        _bodyAnimator.speed = speed;
        _currentUpperBodyState = animName;

        StartResetRoutine(idleAnimName, attackInterval);
    }

    // 상체 공격 애니메이션 출력, 공격속도 반영 
    public void PlayUpperBodyAttack(string animName, float attackInterval)
    {
        if (_isReloading) return; 

        if (string.IsNullOrEmpty(animName) ) return;

        float clipLength = GetAnimationClipLength(animName);
        float speed = Mathf.Clamp(clipLength / attackInterval, 0.1f, 10f);

        _bodyAnimator.Play(animName, 0, 0f);
        _bodyAnimator.speed = speed;
        _currentUpperBodyState = animName;
    }

   // 자동 초기화가 있는 재장전 애니메이션 
    public void PlayUpperBodyReloadWithAutoReset(string reloadAnim, string idleAnim, float reloadDuration, float animSpeedMultiplier = 1f)
    {
        if (string.IsNullOrEmpty(reloadAnim) || string.IsNullOrEmpty(idleAnim)) return;

        float clipLength = GetAnimationClipLength(reloadAnim);
        float speed = Mathf.Clamp(clipLength / reloadDuration, 0.1f, 10f);

        _bodyAnimator.Play(reloadAnim, 0, 0f);
        _bodyAnimator.speed = speed * animSpeedMultiplier;
        _currentUpperBodyState = reloadAnim;

        StartResetRoutine(idleAnim, reloadDuration);
    }

    // 재장전 애니메이션 출력 
    public void PlayUpperBodyReload(string reloadAnim , float reloadDuration, float animSpeedMultiplier = 1f)
    {
        if (string.IsNullOrEmpty(reloadAnim)) return;

        float clipLength = GetAnimationClipLength(reloadAnim);
        float speed = Mathf.Clamp(clipLength / reloadDuration, 0.1f, 10f);

        _bodyAnimator.Play(reloadAnim, 0, 0f);
        _bodyAnimator.speed = speed * animSpeedMultiplier;
        _currentUpperBodyState = reloadAnim;
    }

    // Idle 초기화 루틴
    public void StartResetRoutine(string idleAnimName, float delay)
    {
        if (_upperBodyResetRoutine != null)
            StopCoroutine(_upperBodyResetRoutine);

        _upperBodyResetRoutine = StartCoroutine(ResetUpperBodyRoutine(idleAnimName, delay));
    }


    // 방어구의 Idle 상태로 강제 변경
    private IEnumerator ResetUpperBodyRoutine(string idleAnimName, float delay)
    {
        yield return new WaitForSeconds(delay);

        // 재장전 중이거나 이미 idle이면 덮어쓰지 않음
        if (isActing || _isReloading || 
            (!string.IsNullOrEmpty(_currentUpperBodyState) && _currentUpperBodyState == idleAnimName))
            yield break;

        _bodyAnimator.Play(idleAnimName);
        _currentUpperBodyState = idleAnimName;
        _bodyAnimator.speed = 1f;
        _upperBodyResetRoutine = null;
    }

    private float GetAnimationClipLength(string clipName)
    {
        foreach (var clip in _bodyAnimator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
                return clip.length;
        }
        return 1f;
    }

    public void ResetUpperBodySpeed()
    {
        _bodyAnimator.speed = 1f;
    }

    public void PlayLegAnimation(string stateName)
    {
        if (_currentLegState == stateName) return;

        _legAnimator.Play(stateName);
        _currentLegState = stateName;
    }

    public void UpdateLegAnimation(bool isDashing, bool isGrounded, float verticalVelocity, float horizontalSpeed)
    {
        if (isDashing)
        {
            PlayLegAnimation("Dash");
        }
        else if (!isGrounded)
        {
            if (verticalVelocity > 0.1f)
                PlayLegAnimation("JumpUp");
            else
                PlayLegAnimation("JumpDown");
        }
        else if (Mathf.Abs(horizontalSpeed) > 0.1f)
        {
            PlayLegAnimation("Run");
        }
        else
        {
            PlayLegAnimation("Idle");
        }
    }
}
