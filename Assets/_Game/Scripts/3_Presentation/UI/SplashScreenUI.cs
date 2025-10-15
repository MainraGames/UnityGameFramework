using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SplashScreenUI : BaseUI
{
    [SerializeField] private Image splashImage;

    private Animator splashAnimator;

    protected override void Awake()
    {
        base.Awake();
        splashAnimator = GetComponent<Animator>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        splashImage.DOFillAmount(1, 1);
    }
}
