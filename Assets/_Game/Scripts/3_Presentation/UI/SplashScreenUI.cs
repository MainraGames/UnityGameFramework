using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SplashScreenUI : BaseUI
{
    [SerializeField] private Image splashImage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        splashImage.DOFillAmount(1, 1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
