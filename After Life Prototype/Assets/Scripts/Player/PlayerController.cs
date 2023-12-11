using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using DG.Tweening;


[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    private Collision _collider;     
    [HideInInspector]
    public Rigidbody2D _rigidBody;
    private AnimationScript _animation;

    [Space]
    [Header("Stats")]
    public float _speed = 10;
    public float _jumpForce = 50;
    public float _slideSpeed = 5;
    public float _wallJumpLerp = 10;
    public float _dashSpeed = 20;

    public int _side = 1;

    [Space]
    [Header("Boolean")]
    public bool _canMove;
    public bool _wallGrab;
    public bool _wallJumped;
    public bool _wallSlide;
    public bool _isDashing;

    [Space]

    private bool _groundTouch;
    private bool _hasDashed;

    [Space]
    [Header("Polish")]
    public ParticleSystem _dashParticles;
    public ParticleSystem _jumpParticles;
    public ParticleSystem _wallJumpParticles;
    public ParticleSystem _slideParticles;


    void Start()
    {
        _collider = GetComponent<Collision>();
        _rigidBody = GetComponent<Rigidbody2D>();
        _animation = GetComponentInChildren<AnimationScript>();
    }

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        float xRaw = Input.GetAxisRaw("Horizontal");
        float yRaw = Input.GetAxisRaw("Vertical");

        Vector2 dir = new Vector2(x, y);

        Walk(dir);
        _animation.SetHorizontalMovement(x, y, _rigidBody.velocity.y);

        if (_collider._onWall && Input.GetButton("Fire3") && _canMove)
        {
            if (_side != _collider._wallSlide)
                _animation.Flip(_side * -1);
            _wallGrab = true;
            _wallSlide = false;
        }

        if (Input.GetButton("Fire3") || !_collider._onWall || !_canMove)
        {
            _wallSlide = false;
            _wallGrab = false;
        }

        if (_collider._onGround && !_isDashing)
        {
            _wallJumped = false;
            GetComponent<BetterJumping>().enabled = true;
        }

        if (_wallGrab && !_isDashing)
        {
            _rigidBody.gravityScale = 0;
            if (x > .2f || x < -.2f)
                _rigidBody.velocity = new Vector2(_rigidBody.velocity.x, 0);

            float speedModifier = y > 0 ? .5f : 1;

            _rigidBody.velocity = new Vector2(_rigidBody.velocity.x, y * (_speed * speedModifier));
        }
        else
        {
            _rigidBody.gravityScale = 3;
        }
        if (_collider._onWall && !_collider._onGround)
            _wallSlide= false;
        if (Input.GetButtonDown("Jump"))
        {
            _animation.SetTrigger("jump");

            if (_collider._onGround)
                Jump(Vector2.up, false);
            if (_collider._onWall && !_collider._onGround)
                WallJumped();
        }

        if (Input.GetButtonDown("Fire1") && !_hasDashed)
        {
            if(xRaw !=0 || yRaw!=0)
            {
                Dash(xRaw, yRaw);
            }
        }

        if(_collider._onGround && !_groundTouch)
        {
            GroudTouch();
            _groundTouch= true;
        }

        if(!_collider._onGround && _groundTouch)
        {
            _groundTouch = false;
        }

        _wallJumpParticles(y);

        if(_wallGrab || _wallSlide || !_canMove)
        {
            return;
        }
        if (x > 0)
        {
            _side = 1;
            _animation.Flip(_side);
        }
        if (x < 0)
        {
            _side= -1;
            _animation.Flip(_side);
        }
    }      

    void GroudTouch()
    {
       _hasDashed= false;
       _isDashing= false;

        _side = _animation._spriteRenderer.flipX ? -1 : 1;

        _jumpParticles.Play();
    }
    void Dash(float x, float y)
    {
        Camera.main.transform.DOComplete();
        Camera.main.transform.DOShakePosition(.2f, .5f, 14, 90, false, true);
        FindObjectOfType<RippleEffect>().Emit(Camera.main.WorldToViewportPoint(transform.position));

        _hasDashed= true;

        _animation.SetTrigger("dash");

        _rigidBody.velocity = Vector2.zero;
        Vector2 dir = new Vector2(x, y);

        _rigidBody.velocity += dir.normalized * _dashSpeed;
        StartCoroutine(DashWait());
    }

    IEnumerator DashWait()
    {
        FindObjectOfType<GhostTrail>().ShowGhost();
        StartCoroutine(GroundDash());
        DOVirtual.Float(14, 0, .8f, RigidbodyDrag);

        _dashParticles.Play();
        _rigidBody.gravityScale = 0;
        GetComponent<BetterJumping>().enabled = false;
        _wallJumped = true;
        _isDashing = true;

        yield return new WaitForSeconds(.3f);

        _dashParticles.Stop();
        _rigidBody.gravityScale = 3;
        GetComponent<BetterJumping>().enabled = true;
        _wallJumped = false;
        _isDashing = false;    
    }

    IEnumerator GroundDash()
    {
        yield return new WaitForSeconds(.15f);
        if (_collider._onGround)
        {
            _hasDashed = false;
        }
    }

    private void WallJumped()
    {
        if ((_side == 1 && _collider._onRightWall || _side == -1 && !_collider._onRightWall))
        {
            _side *= -1;
            _animation.Flip(_side);
        }

        StopCoroutine(DisableMovement(0));
        StopCoroutine(DisableMovement(.1f));

        Vector2 wallDir = _collider._onRightWall ? Vector2.left : Vector2.right;
        Jump((Vector2.up / 1.5f + wallDir / 1.5f), true);

        _wallJumped = true;
    }

    private void WallSlide()
    {
        if (_collider._wallSlide != _side)
        {
            _animation.Flip(_side * -1);
        } 
        if (!_canMove)
        {
            return;
        }

        bool pushingWall = false;

        if (_rigidBody.velocity.x > 0 && _collider._onRightWall) || (_rigidBody.velocity.x < 0 && _collider._onLeftWall))
        {
            pushingWall= true;
        } 
        float push = pushingWall? 0 : _rigidBody.velocity.x;

        _rigidBody.velocity = new Vector2(push, -_slideSpeed);
    }

    private void Walk(Vector2 dir)
    {
        if (!_canMove)
        {
            return;
        }

        if (!_wallGrab)
        {
            return;
        }

        if (!_wallJumped)
        {
            _rigidBody.velocity = new Vector2(dir.x * _speed, _rigidBody.velocity.y);
        }
        else
        {
            _rigidBody.velocity = Vector2.Lerp(_rigidBody.velocity, (new Vector2(dir.x * _speed, _rigidBody.velocity.y)), _wallJumpLerp * Time.deltaTime);

        }
    }

    private void Jump(Vector2 dir, bool wall)
    {
        _slideParticles.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
        ParticleSystem particle = wall ? _wallJumpParticles : _jumpParticles;

        _rigidBody.velocity = new Vector2(_rigidBody.velocity.x, 0);
        _rigidBody.velocity += dir * _jumpForce;

        particle.Play();
    }

    IEnumerator DisableMovement(float time)
    {
        _canMove= false;
        yield return new WaitForSeconds(time);
        _canMove= true;
    }

    void RigidbodyDrag(float x)
    {
        _rigidBody.drag = x;
    }

    void Wallparticle(float vertical)
    {
        var main = _slideParticles.main;
        if (_wallSlide || (_wallGrab && vertical < 0))
        {
            _slideParticles.transform.parent.localScale = new Vector3(ParticleSide(),1, 1);
            main.startColor= Color.white;
        }
        else
        {
            main.startColor= Color.clear;
        }
    }

    int ParticleSide()
    {
        int particleSide = _collider._onRightWall ? 1: -1;
        return particleSide;
    }
}
