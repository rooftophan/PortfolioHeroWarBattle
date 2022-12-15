using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using System;

public class Directing_WaitUIParticles : BaseDirecting
{
    private float _endDelay;
    private bool _useInputLock;

    private UnityEngine.Object[] _objects = null;
    private List<ParticleSystem> _particles = new List<ParticleSystem>();

    public Directing_WaitUIParticles(Action complete, float endDelay, bool useInputLock, params UnityEngine.Object[] objects) : base(complete)
    {
        _endDelay = endDelay;
        _useInputLock = useInputLock;
        _objects = objects;
    }

    /// 커스텀 UIParticleSystem을 사용하기 때문에,
    /// 일반적인 Particle Alive체크로직과 다른 검사로직을 가진다.
    protected bool IsAlive()
    {
        foreach(var particle in _particles)
        {
            if(particle.isEmitting || particle.particleCount != 0)
                return true;
            else
                continue;
        }
        return false;
    }

    protected void Dipose()
    {
        _objects = null;
        _particles.Clear();
        _particles = null;
    }

    public override void Execute()
    {
        if(_useInputLock)
            GameSystem.Instance.IncreaseEventSystemLock();

        if(_objects == null)
        {
            Complete();
            return;
        }

        foreach(var each in _objects)
        {
            var target = each as GameObject;
            if (target == null)
                continue;

            target.SetActive(true);

            var targets = target.GetComponentsInChildren<ParticleSystem>(true);

            _particles.AddRange(targets);
        }

        var seq = DOTween.Sequence();
        seq.SetUpdate(false);
        seq.OnUpdate(() =>
        {
            if(!IsAlive())
            {
                seq.Kill(false);
                var delaySeq = DOTween.Sequence();
                delaySeq.SetUpdate(false);

                delaySeq.AppendInterval(_endDelay);
                delaySeq.OnComplete(() =>
                {
                    delaySeq.Kill(false);
                    Complete();
                });
                delaySeq.Play();
            }
        });

        seq.AppendInterval(float.MaxValue);

        seq.Play();
    }

    protected override void Complete()
    {
        if(_useInputLock)
            GameSystem.Instance.DecreaseEventSystemLock();

        //  종료시, 파티클을 hide.
        foreach(var each in _objects)
        {
            var target = each as GameObject;
            if(target != null)
                target.SetActive(false);
        }

        Dipose();

        base.Complete();
    }
}
