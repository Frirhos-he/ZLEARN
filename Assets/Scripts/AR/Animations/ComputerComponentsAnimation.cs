using System.Collections;
using System.Collections.Generic;
using AR;
using DG.Tweening;
using UnityEngine;

public class ComputerComponentsAnimation : MonoBehaviour, IARAnimation
{
    private Sequence _animationSequence;

    public GameObject CPUGameObject;
    public GameObject CPUTargetPosition;
    
    public GameObject RAMGameObject;
    public GameObject RAMTargetPosition;
    
    public GameObject HDDGameObject;
    public GameObject HDDTargetPosition;
    public void DoAnimation(int stepIndex)
    {
        //reset previous animation
        if (_animationSequence != null)
        {
            _animationSequence.timeScale = 5;
            _animationSequence.PlayBackwards();
        }
        
        switch (stepIndex)
        {
            case 1:
                _animationSequence = DOTween.Sequence();
                _animationSequence.timeScale = 1;
                _animationSequence.SetAutoKill(false);
                _animationSequence.Append(CPUGameObject.transform.DOMove(CPUTargetPosition.transform.position, 0.5f));
                _animationSequence.Append(CPUGameObject.transform.DOLocalRotate(new Vector3(-70, 0, 0), 0.5f));
                _animationSequence.PlayForward();
                break;
            case 2:
                _animationSequence = DOTween.Sequence();
                _animationSequence.timeScale = 1;
                _animationSequence.SetAutoKill(false);
                _animationSequence.Append(RAMGameObject.transform.DOMove(RAMTargetPosition.transform.position, 0.5f));
                _animationSequence.Append(RAMGameObject.transform.DOLocalRotate(new Vector3(0, 90, 0), 0.5f));
                _animationSequence.PlayForward();
                break;
            case 3:
                _animationSequence = DOTween.Sequence();
                _animationSequence.timeScale = 1;
                _animationSequence.SetAutoKill(false);
                _animationSequence.Append(HDDGameObject.transform.DOMove(HDDTargetPosition.transform.position, 0.5f));
                _animationSequence.Append(HDDGameObject.transform.DOLocalRotate(new Vector3(-70, 0, 0), 0.5f));
                _animationSequence.PlayForward();
                break;
        }
    }
}
