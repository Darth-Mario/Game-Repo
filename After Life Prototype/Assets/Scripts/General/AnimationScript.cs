using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationScript : MonoBehaviour
{
    private Animator _anim;
    private PlayerController _movement;
    private Collision _collision;
    [HideInInspector]
    public SpriteRenderer _spriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        _anim = GetComponent<Animator>();
        _movement = GetComponent<PlayerController>();
        _collision = GetComponent<Collision>();
        _spriteRenderer= GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        _anim.SetBool("_onGround", _collision._onGround);
        _anim.SetBool("_onWall", _collision._onWall);
        _anim.SetBool("_onRightWall", _collision._onRightWall);
        _anim.SetBool("wallGrab", _movement._wallGrab);
        _anim.SetBool("_canMove", _movement._canMove);
        _anim.SetBool("_isDashing", _movement._isDashing);        
    }

    public void SetHorizontalMovement(float x, float y, float yVelocity)
    {
        _anim.SetFloat("HorizontalAxis", x);
        _anim.SetFloat("VerticalAxis", y);
        _anim.SetFloat("VerticalVelocity", yVelocity);
    }

    public void SetTrigger(string trigger)
    {
        _anim.SetTrigger(trigger);
    }

    public void Flip(int side)
    {
        if (_movement._wallGrab || _movement._wallSlide)
        {
            if (side == -1 && _spriteRenderer.flipX)
            {
                return;
            }

            if (side == 1 && !_spriteRenderer.flipX)
            {
                return;
            }
        }
        bool state = (side == 1) ? false: true;
        _spriteRenderer.flipX = state;
    }
}
