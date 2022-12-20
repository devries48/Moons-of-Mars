using UnityEngine;
using System.Diagnostics.CodeAnalysis;

public class EffectController : PoolableMonoBehaviour
{
    internal ParticleSystem _ps;
    Animator _animator;

    internal EffectsManager.Effect m_effect;

    bool _animAlive;
    bool _isAnimation;

    void OnEnable() => _animAlive = true;

    void Start()
    {
        TryGetComponent(out _animator);
        TryGetComponent(out _ps);

        if (_animator != null)
        {
            _isAnimation = true;
            var clip = _animator.runtimeAnimatorController.animationClips[0];

            clip.AddEvent(new()
            {
                time = clip.length,
                functionName = "AnimationCompleteHandler",
                stringParameter = clip.name
            });
        }
    }

    public bool IsAlive()
    {
        if (_isAnimation)
            return _animAlive;
        else
        {
            if (_ps == null)
            {
                TryGetComponent(out _ps);
                print("Particle system was null");
            }

            return _ps.IsAlive();
        }
    }

    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
    void AnimationCompleteHandler(string name)
    {
        _animAlive = false;
    }
}