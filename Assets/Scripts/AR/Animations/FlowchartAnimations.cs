using System;
using System.Collections;
using System.Collections.Generic;
using AR;
using DG.Tweening;
using UnityEngine;

public class FlowchartAnimations : MonoBehaviour, IARAnimation
{
    private Sequence _animationSequence;
    public GameObject Start, End, ReadTemperature, Print1, Print2,TempCondition, TargetPosition;
    public List<GameObject> Arrows;

    public void DoAnimation(int stepIndex)
    {
        //reset previous animation
        if (_animationSequence != null)
        {
            _animationSequence.timeScale = 5;
            _animationSequence.PlayBackwards();
        }
        
        _animationSequence = DOTween.Sequence();
        _animationSequence.timeScale = 1;
        _animationSequence.SetAutoKill(false);

        var localX = TargetPosition.transform.position.x;
        switch (stepIndex)
        {
            case 1:
                _animationSequence.Append(Start.transform.DOMoveX(localX,0.5f));
                _animationSequence.Append(End.transform.DOMoveX(localX,0.5f));
                break;
            case 2:
                _animationSequence.Append(Print2.transform.DOMoveY(Print2.transform.position.y + 0.2f,0.3f));
                _animationSequence.Append(Print1.transform.DOMoveX(localX,0.4f));
                _animationSequence.Append(Print2.transform.DOMoveX(localX,0.4f));
                break;
            case 3:
                _animationSequence.Append(TempCondition.transform.DOMoveX(localX,0.5f));
                break;
            case 4:
                _animationSequence.Append(ReadTemperature.transform.DOMoveX(localX,0.5f));
                break;
            case 5:
                foreach (var arrow in Arrows)
                {
                    _animationSequence.Append(arrow.transform.GetComponent<Renderer>().material.DOColor(Color.yellow, 0.1f));
                }
                break;
        }
        
        _animationSequence.Play();
    }
}
