using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [SerializeField] private Character _character;

    public Character Character => _character;
    
    private List<ContinuousEffect> _effectsList = new List<ContinuousEffect>();
    private List<Coroutine> _effectsCoroutines = new List<Coroutine>();

    public void Start()
    {
        int count = EffectsGenerator.Instance.CountOfEffects;
        for (int i = 0; i < count; i++)
        {
            _effectsCoroutines.Add(null);
            _effectsList.Add(null);
        }
    }

    public void Apply(InstantEffect effect)
    {
        effect.Apply();
    }

    public void Apply(ContinuousEffect effect)
    {
        if (_effectsList[effect.EffectId] != null )
        {
            Coroutine effectCoroutine = _effectsCoroutines[effect.EffectId];
            StopCoroutine(effectCoroutine);
            effectCoroutine = StartCoroutine(EffectDuration(effect));
            _effectsCoroutines[effect.EffectId] = effectCoroutine;
        }
        else
        {
            effect.Apply();
            _effectsList[effect.EffectId] = effect;
            Coroutine effectCoroutine = StartCoroutine(EffectDuration(effect));
            _effectsCoroutines[effect.EffectId] = effectCoroutine;
        }
    }

    public bool HasEffect(Type continuousEffectType)
    {
        return _effectsList.Where(contEffect => contEffect.GetType() == continuousEffectType).Count() > 0;
    }

    private void Remove(ContinuousEffect effect)
    {
        effect.Remove();
        _effectsList[effect.EffectId] = null;
        _effectsCoroutines[effect.EffectId] = null;
    }

    private IEnumerator EffectDuration(ContinuousEffect effect)
    {
        yield return new WaitForSeconds(effect.Duration);
        Remove(effect);
    }
}
