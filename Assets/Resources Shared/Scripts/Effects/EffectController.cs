using UnityEngine;
using System.Diagnostics.CodeAnalysis;

public class EffectController : PoolableMonoBehaviour
{
    ParticleSystem _ps;
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

    public bool IsAlive() => _isAnimation ? _animAlive : _ps.IsAlive();

    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
    void AnimationCompleteHandler(string name)
    {
        Debug.Log($"{name} animation complete.");
        _animAlive= false;
    }
}