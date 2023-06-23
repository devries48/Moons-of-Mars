using UnityEngine;
using System.Diagnostics.CodeAnalysis;

namespace MoonsOfMars.Shared
{
    public class EffectController : PoolableBase
    {
        internal ParticleSystem m_ps;
        internal EffectsData.Effect m_effect;

        Animator _animator;
        bool _animAlive;
        bool _isAnimation;

        void OnEnable() => _animAlive = true;

        void Start()
        {
            TryGetComponent(out _animator);
            TryGetComponent(out m_ps);

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
                if (m_ps == null)
                {
                    TryGetComponent(out m_ps);
                    if (m_ps == null)
                        print("Particle system was null");
                }

                return m_ps.IsAlive();
            }
        }

        //[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        void AnimationCompleteHandler(string name)
        {
            _animAlive = false;
        }
    }
}