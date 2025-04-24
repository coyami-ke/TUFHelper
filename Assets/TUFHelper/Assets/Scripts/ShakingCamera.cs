using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ShakingCamera : MonoBehaviour
{
    [SerializeField]
    public Transform _transform;
    void Start()
    {
        _transform.DOMoveY(5f, 5).From(0).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }

    void Update()
    {
        
    }
}
