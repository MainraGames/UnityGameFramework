using System.Collections;
using _Game.Scripts.Core.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Game.Scripts.Presentation.UI
{
    public class SplashScreenUI : BaseUI
    {
        [SerializeField] private Image splashImage;

        private Animator animator;
        
        // changed: make this assignable by the DI container (remove readonly)
        [Inject]
        private ISceneLoader _sceneLoader;

        protected override void Awake()
        {
            base.Awake();
            animator = GetComponent<Animator>();
        }

        /*// Start is called once before the first execution of Update after the MonoBehaviour is created
        // changed: use Start as a coroutine to wait for animation to finish then load next scene
        private IEnumerator Start()
        {
            // ensure animator is assigned
            if (animator == null)
                animator = GetComponent<Animator>();

            // wait a frame so animator state is valid
            yield return null;

            // wait for the current animator clip (if any) to finish
            yield return WaitForAnimationToFinish();

            // load next scene via ISceneLoader
            if (_sceneLoader != null)
            {
                _sceneLoader.LoadNextScene();
            }
            else
            {
                Debug.LogWarning("ISceneLoader not injected on SplashScreenUI. Cannot load next scene automatically.");
            }
        }

        // waits until the animator's current clip finishes (best-effort)
        private IEnumerator WaitForAnimationToFinish()
        {
            if (animator == null)
                yield break;

            // try to get the current clip info
            var clips = animator.GetCurrentAnimatorClipInfo(0);
            float waitTimeout = 5f; // fallback timeout if clip info isn't available

            // give the animator a few frames to populate clip info
            int attempts = 0;
            while ((clips == null || clips.Length == 0) && attempts < 60)
            {
                attempts++;
                yield return null;
                clips = animator.GetCurrentAnimatorClipInfo(0);
            }

            if (clips == null || clips.Length == 0)
            {
                // nothing to wait for; fallback to a short delay
                yield return new WaitForSeconds(Mathf.Min(waitTimeout, 1.0f));
                yield break;
            }

            var clip = clips[0].clip;
            if (clip == null)
            {
                yield return new WaitForSeconds(Mathf.Min(waitTimeout, 1.0f));
                yield break;
            }

            if (clip.isLooping)
            {
                Debug.LogWarning("Splash animation clip is looping. Automatic scene load skipped. Use a non-looping splash animation or call LoadNextSceneNow manually.");
                yield break;
            }

            // Wait until the animator state normalized time reaches or exceeds 1 (clip finished)
            // Use a small safety timeout to avoid infinite waits
            float safety = clip.length + 2f;
            float elapsed = 0f;

            while (elapsed < safety)
            {
                if (!animator.IsInTransition(0))
                {
                    var state = animator.GetCurrentAnimatorStateInfo(0);
                    if (state.normalizedTime >= 1f)
                        yield break; // finished
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            // If we reach here, fall back to waiting the clip length then continue
            yield return new WaitForSeconds(Mathf.Max(0f, clip.length - elapsed));
        }*/

        // small helper to allow manual trigger from animation event or other code
        public void LoadNextSceneNow()
        {
            if (_sceneLoader != null)
                _sceneLoader.LoadNextScene();
            else
                Debug.LogWarning("ISceneLoader not injected on SplashScreenUI. Cannot load next scene.");
        }
    }
}
