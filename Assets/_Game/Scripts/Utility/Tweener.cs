using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.Utility
{
    /// <summary>
    /// Component for running DOTween animations with configurable settings.
    /// </summary>
    /// <remarks>
    /// Supports simultaneous and sequential tweens with various tween types
    /// including Move, Scale, Rotate, Fade, and Color.
    /// </remarks>
    [AddComponentMenu("Game/Utility/Tweener")]
    public class Tweener : MonoBehaviour
    {
        #region Nested Types
        
        /// <summary>
        /// Settings for a single tween animation.
        /// </summary>
        [System.Serializable]
        public class TweenSettings
        {
            /// <summary>
            /// Type of tween animation.
            /// </summary>
            public enum TweenType
            {
                Move,
                Scale,
                Rotate,
                Fade,
                Color
            }

            [FoldoutGroup("$name")]
            public string name;
            
            [FoldoutGroup("$name")]
            public TweenType tweenType;
            
            [FoldoutGroup("$name")]
            [ShowIf("@tweenType == TweenType.Move || tweenType == TweenType.Scale || tweenType == TweenType.Rotate")]
            public Vector3 targetValue;
            
            [FoldoutGroup("$name")]
            [ShowIf("tweenType", TweenType.Fade)]
            [Range(0f, 1f)]
            public float targetAlpha;
            
            [FoldoutGroup("$name")]
            [ShowIf("tweenType", TweenType.Color)]
            public Color targetColor;
            
            [FoldoutGroup("$name")]
            [MinValue(0f)]
            public float duration;
            
            [FoldoutGroup("$name")]
            [MinValue(0f)]
            public float delay;
            
            [FoldoutGroup("$name")]
            public Ease easeType;
            
            [FoldoutGroup("$name")]
            public bool useCustomCurve;
            
            [FoldoutGroup("$name")]
            [ShowIf("useCustomCurve")]
            public AnimationCurve customCurve;
            
            [FoldoutGroup("$name")]
            public bool loop;
            
            [FoldoutGroup("$name")]
            [ShowIf("loop")]
            [Tooltip("-1 for infinite loops")]
            public int loopCount;
            
            [FoldoutGroup("$name")]
            [ShowIf("loop")]
            public bool pingpong;
            
            [FoldoutGroup("$name")]
            [Tooltip("Optional target. If null, uses this GameObject.")]
            public GameObject target;
            
            [FoldoutGroup("$name")]
            public UnityEngine.Events.UnityEvent OnTweenComplete;
        }
        
        #endregion

        #region Serialized Fields
        
        [BoxGroup("General Settings")]
        [SerializeField]
        [Tooltip("Use unscaled time for tweens")]
        private bool _useUnscaledTime = false;

        [BoxGroup("General Settings")]
        [SerializeField]
        [Tooltip("Start tween from the initial active state of the object")]
        private bool _startFromInitialActiveState = true;

        [BoxGroup("Tweens")]
        [SerializeField]
        [Tooltip("Tweens that run simultaneously")]
        private List<TweenSettings> _simultaneousTweens = new List<TweenSettings>();
        
        [BoxGroup("Tweens")]
        [SerializeField]
        [Tooltip("Tweens that run one after another")]
        private List<TweenSettings> _sequentialTweens = new List<TweenSettings>();
        
        #endregion

        #region Private Fields
        
        private int _activeSimultaneousTweens = 0;
        private List<Tween> _activeTweens = new List<Tween>();
        private Coroutine _sequentialCoroutine;
        
        #endregion

        #region Unity Lifecycle
        
        private void Start()
        {
            if (_startFromInitialActiveState)
            {
                RunSimultaneousTweens();
            }
        }

        private void OnDestroy()
        {
            CancelAllTweens();
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Runs all simultaneous tweens.
        /// </summary>
        [Button("Run Simultaneous Tweens")]
        public void RunSimultaneousTweens()
        {
            foreach (var tween in _simultaneousTweens)
            {
                RunTween(tween, gameObject);
            }
        }

        /// <summary>
        /// Runs all sequential tweens one after another.
        /// </summary>
        [Button("Run Sequential Tweens")]
        public void RunSequentialTweens()
        {
            if (_sequentialCoroutine != null)
            {
                StopCoroutine(_sequentialCoroutine);
            }
            _sequentialCoroutine = StartCoroutine(RunSequentialTweensCoroutine());
        }

        /// <summary>
        /// Cancels all active tweens.
        /// </summary>
        [Button("Cancel All Tweens")]
        public void CancelAllTweens()
        {
            foreach (var tween in _activeTweens)
            {
                tween?.Kill();
            }
            _activeTweens.Clear();
            
            if (_sequentialCoroutine != null)
            {
                StopCoroutine(_sequentialCoroutine);
                _sequentialCoroutine = null;
            }
        }

        /// <summary>
        /// Pauses all active tweens.
        /// </summary>
        public void PauseAllTweens()
        {
            foreach (var tween in _activeTweens)
            {
                tween?.Pause();
            }
        }

        /// <summary>
        /// Resumes all paused tweens.
        /// </summary>
        public void ResumeAllTweens()
        {
            foreach (var tween in _activeTweens)
            {
                tween?.Play();
            }
        }
        
        #endregion

        #region Private Methods - Tween Execution
        
        private Tween RunTween(TweenSettings tween, GameObject defaultTarget)
        {
            GameObject target = tween.target != null ? tween.target : defaultTarget;
            if (target == null)
            {
                Debug.LogWarning($"[Tweener] Target for tween '{tween.name}' is null. Skipping.");
                return null;
            }

            Tween tweenAction = tween.tweenType switch
            {
                TweenSettings.TweenType.Move => RunMoveTween(tween, target),
                TweenSettings.TweenType.Scale => RunScaleTween(tween, target),
                TweenSettings.TweenType.Rotate => RunRotateTween(tween, target),
                TweenSettings.TweenType.Fade => RunFadeTween(tween, target),
                TweenSettings.TweenType.Color => RunColorTween(tween, target),
                _ => null
            };

            if (tweenAction != null)
            {
                _activeTweens.Add(tweenAction);
            }

            return tweenAction;
        }

        private Tween RunMoveTween(TweenSettings tween, GameObject target)
        {
            Tween tweenAction = target.TryGetComponent(out RectTransform rectTransform)
                ? rectTransform.DOAnchorPos((Vector2)tween.targetValue, tween.duration)
                : target.transform.DOMove(tween.targetValue, tween.duration);

            ApplyTweenSettings(tween, tweenAction);
            return tweenAction;
        }

        private Tween RunScaleTween(TweenSettings tween, GameObject target)
        {
            Tween tweenAction = target.TryGetComponent(out RectTransform rectTransform)
                ? rectTransform.DOScale(tween.targetValue, tween.duration)
                : target.transform.DOScale(tween.targetValue, tween.duration);

            ApplyTweenSettings(tween, tweenAction);
            return tweenAction;
        }

        private Tween RunRotateTween(TweenSettings tween, GameObject target)
        {
            Tween tweenAction = target.transform.DORotate(tween.targetValue, tween.duration);
            ApplyTweenSettings(tween, tweenAction);
            return tweenAction;
        }

        private Tween RunFadeTween(TweenSettings tween, GameObject target)
        {
            Tween tweenAction = null;

            if (target.TryGetComponent(out CanvasGroup canvasGroup))
            {
                tweenAction = canvasGroup.DOFade(tween.targetAlpha, tween.duration);
            }
            else if (target.TryGetComponent(out Image image))
            {
                tweenAction = image.DOFade(tween.targetAlpha, tween.duration);
            }
            else if (target.TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                tweenAction = spriteRenderer.DOFade(tween.targetAlpha, tween.duration);
            }
            else if (target.TryGetComponent(out TMPro.TMP_Text tmpText))
            {
                tweenAction = tmpText.DOFade(tween.targetAlpha, tween.duration);
            }

            if (tweenAction != null)
            {
                ApplyTweenSettings(tween, tweenAction);
            }

            return tweenAction;
        }

        private Tween RunColorTween(TweenSettings tween, GameObject target)
        {
            Tween tweenAction = null;

            if (target.TryGetComponent(out Renderer renderer))
            {
                tweenAction = renderer.material.DOColor(tween.targetColor, tween.duration);
            }
            else if (target.TryGetComponent(out Image image))
            {
                tweenAction = image.DOColor(tween.targetColor, tween.duration);
            }
            else if (target.TryGetComponent(out TMPro.TMP_Text tmpText))
            {
                tweenAction = tmpText.DOColor(tween.targetColor, tween.duration);
            }
            else if (target.TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                tweenAction = spriteRenderer.DOColor(tween.targetColor, tween.duration);
            }

            if (tweenAction != null)
            {
                ApplyTweenSettings(tween, tweenAction);
            }

            return tweenAction;
        }
        
        #endregion

        #region Private Methods - Tween Configuration
        
        private void ApplyTweenSettings(TweenSettings tween, Tween tweenAction)
        {
            if (tween.useCustomCurve && tween.customCurve != null)
            {
                tweenAction.SetEase(tween.customCurve);
            }
            else
            {
                tweenAction.SetEase(tween.easeType);
            }

            tweenAction.SetDelay(tween.delay)
                .SetLoops(tween.loop ? (tween.loopCount > 0 ? tween.loopCount : -1) : 1,
                    tween.pingpong ? LoopType.Yoyo : LoopType.Restart)
                .OnComplete(() =>
                {
                    OnTweenComplete(tween);
                    _activeTweens.Remove(tweenAction);
                    if (--_activeSimultaneousTweens <= 0 && _sequentialCoroutine == null)
                    {
                        RunSequentialTweens();
                    }
                });

            if (_useUnscaledTime)
            {
                tweenAction.SetUpdate(true);
            }

            _activeSimultaneousTweens++;
        }

        private void OnTweenComplete(TweenSettings tween)
        {
            tween.OnTweenComplete?.Invoke();
        }

        private IEnumerator RunSequentialTweensCoroutine()
        {
            foreach (var tween in _sequentialTweens)
            {
                Tween tweenAction = RunTween(tween, gameObject);
                if (tweenAction != null)
                {
                    yield return tweenAction.WaitForCompletion();
                }
            }
            _sequentialCoroutine = null;
        }
        
        #endregion
    }
}
