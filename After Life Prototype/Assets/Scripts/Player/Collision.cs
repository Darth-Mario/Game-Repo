using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision : MonoBehaviour
{
    [Header("Layers")]
    public LayerMask _groundLayer;

    [Space]

    public bool _onGround;
    public bool _onWall;
    public bool _onRightWall;
    public bool _onLeftWall;
    public int _wallSlide;

    [Space]

    [Header("Collision")]

    public float _collisionRadius = 0.25f;
    public Vector2 _bottomOffset, _rightOffset, _leftOffset;
    private Color debudCollisionColor = Color.black;



    void FixedUpdate()
    {
        _onGround = Physics2D.OverlapCircle((Vector2)transform.position + _bottomOffset, _collisionRadius, _groundLayer);
        _onWall = Physics2D.OverlapCircle((Vector2)transform.position + _rightOffset, _collisionRadius, _groundLayer) 
            || Physics2D.OverlapCircle((Vector2)transform.position + _leftOffset,_collisionRadius, _groundLayer);
        _onRightWall = Physics2D.OverlapCircle((Vector2)transform.position + _rightOffset, _collisionRadius, _groundLayer);
        _onLeftWall = Physics2D.OverlapCircle((Vector2)transform.position + _leftOffset, _collisionRadius, _groundLayer);

        _wallSlide = _onRightWall? 1 : -1;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector2[] pos = new Vector2[] {_bottomOffset,_rightOffset, _leftOffset};

        Gizmos.DrawWireSphere((Vector2)transform.position + _bottomOffset,_collisionRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + _rightOffset,_collisionRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + _leftOffset, _collisionRadius);

    }

}
