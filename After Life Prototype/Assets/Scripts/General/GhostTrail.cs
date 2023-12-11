using DG.Tweening;
using UnityEngine;

public class GhostTrail : MonoBehaviour
{
    [SerializeField] private PlayerController _movement;
    [SerializeField] private AnimationScript _anim;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Transform _ghostParents;
    [SerializeField] private Color _trailColor;
    [SerializeField] private Color _fadeColor;
    [SerializeField] private float _fadeTime;
    [SerializeField] private float _ghostInterval;
   
    // Start is called before the first frame update
    void Start()
    {
        _anim = GetComponent<AnimationScript>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _movement= GetComponent<PlayerController>();        
    }

    // Update is called once per frame
    public void ShowGhost()
    {
        Sequence s = DOTween.Sequence();

        for(int i = 0; i < _ghostParents.childCount; i++)
        {
            Transform currentGhost = _ghostParents.GetChild(i);
            s.AppendCallback(() => currentGhost.position = _movement.transform.position);
            s.AppendCallback(() => currentGhost.GetComponent<SpriteRenderer>().flipX = _anim._spriteRenderer.flipX);
            s.AppendCallback(() => currentGhost.GetComponent<SpriteRenderer>().sprite = _anim._spriteRenderer.sprite);
            s.Append(currentGhost.GetComponent<SpriteRenderer>().material.DOColor(_trailColor, 0));
            s.AppendCallback(() => FadeGhost(currentGhost));
            s.AppendInterval(_ghostInterval);
        }
    }

    public void FadeGhost(Transform current)
    {
        current.GetComponent<SpriteRenderer>().material.DOKill();
        current.GetComponent<SpriteRenderer>().material.DOColor(_fadeColor, _fadeTime);
    }
}
