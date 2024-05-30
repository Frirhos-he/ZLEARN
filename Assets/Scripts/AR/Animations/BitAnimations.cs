using System;
using System.Collections;
using System.Collections.Generic;
using AR;
using DG.Tweening;
using UnityEngine;

public class BitAnimations : MonoBehaviour, IARAnimation
{
    private Sequence _animationSequence;

    public List<GameObject> Bits;
    public GameObject BitsAndCPUGroup;
    public GameObject BitsGroup;
    public GameObject BitsAndCPUGroupTargetPosition;
    public GameObject BitsTargetPosition;
    private bool _isStep1Played = false;

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
                if (!_isStep1Played)
                {
                    _isStep1Played = true;
                    //definita subita ed eseguita solo una volta
                    var initialSequence = DOTween.Sequence();
                    initialSequence.SetAutoKill(true);

                    //animazione eseguita una volta sola
                    //alla fine del play le sequence è killata quindi non verrà più eseguita
                    //sposta tutto più sopra
                    initialSequence.Append(BitsAndCPUGroup.transform.DOMove(BitsAndCPUGroupTargetPosition.transform.position, 0.5f));
                    initialSequence.Append(BitsAndCPUGroup.transform.DORotate(new Vector3(-70, 0, 0), 0.5f));
                    //sposta i nit a destra
                    initialSequence.Append(BitsGroup.transform.DOLocalMove(BitsTargetPosition.transform.localPosition, 0.5f));
                    initialSequence.Play();
                }
                break;
            case 3:
            case 4:
            case 5:
            case 6:
                _animationSequence = DOTween.Sequence();
                _animationSequence.timeScale = 1;
                _animationSequence.SetAutoKill(false);
                //alzo il bit prima 0 poi 1,2,3
                _animationSequence.Append(Bits[stepIndex - 3].transform.DOLocalMoveZ(Bits[stepIndex - 3].transform.localPosition.z + 1f, 0.5f));
                _animationSequence.Play();
                break;
        }
    }
}
