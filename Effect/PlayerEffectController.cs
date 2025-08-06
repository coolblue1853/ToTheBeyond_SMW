using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEffectController : MonoBehaviour
{
    [SerializeField] private Transform _footPoint; 

    private ShakeEffect _shakeEffect;
    private BlinkEffect _blinkEffect;
    private GhostTrail _ghostTrail; // 대쉬 고스트
    private void Awake()
    {
        _shakeEffect = GetComponent<ShakeEffect>();  
        _blinkEffect = GetComponent<BlinkEffect>();
        //ghostTrail = GetComponent<GhostTrail>();
    }
    public void PlayShakePlayerEffect()
    {
        _shakeEffect.Shake();
    }
    public void PlayBlinkEffect(float duration)
    {
        _blinkEffect.StartBlink(duration);
    }
    public void PlayJumpEffect()
    {
        ParticleManager.Instance.Play(ParticleType.JumpDust, _footPoint.position);
    }
    public void PlayLandEffect()
    {
        ParticleManager.Instance.Play(ParticleType.LandDust, _footPoint.position);
    }
    public void PlayTrailEffect()
    {
     //   ghostTrail.StartTrail();
    }
    public void StopTrailEffect()
    {
       // ghostTrail.StopTrail();
    }
}
