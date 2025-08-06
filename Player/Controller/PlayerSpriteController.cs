using UnityEngine;
using System.Collections.Generic;



public class PlayerSpriteController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _lHand;
    [SerializeField] private SpriteRenderer _rHand;
    [SerializeField] private SpriteRenderer _upperBody;
    [SerializeField] private SpriteRenderer _headAcess;
    [SerializeField] private SpriteRenderer _lArm;
    [SerializeField] private SpriteRenderer _rArm;
    [SerializeField] private SpriteRenderer _backPack;
    [SerializeField] private SpriteRenderer _backLine;
    [SerializeField] private SpriteRenderer _head;
    [SerializeField] private SpriteRenderer _upBody;
    [SerializeField] private SpriteRenderer _body;
    [SerializeField] private SpriteRenderer _leg;
    [SerializeField] private Animator _animator;


    public void ChangeSprite(ArmorSprites sprites)
    {
        _lHand.sprite = sprites.leftHand;
        _rHand.sprite = sprites.rightHand;
        _upperBody.sprite = sprites.upperBody;
        _headAcess.sprite = sprites.headAccessory;
        _lArm.sprite = sprites.leftArm;
        _rArm.sprite = sprites.rightArm;
        _backPack.sprite = sprites.backPack;
        _backLine.sprite = sprites.backLine;
        _head.sprite = sprites.head;
        _upBody.sprite = sprites.upBody;
        _body.sprite = sprites.body;
        _animator.runtimeAnimatorController = sprites.legAnimController;
    }

    public List<SpriteRenderer> GetAllSpriteRenderers()
    {
        return new List<SpriteRenderer>
    {
        _lHand,_rHand, _upperBody, _headAcess, _lArm, _rArm,
        _backPack, _backLine, _head, _upBody, _body, _leg
    };
    }

}
