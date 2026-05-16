using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Capybrawlers.UI
{
    public class DamagePopup : MonoBehaviour
    {
        private RectTransform   _rt;
        private TextMeshProUGUI _text;
        private CanvasGroup     _cg;

        private static readonly Color DmgColor  = new(1.00f, 0.22f, 0.10f);
        private static readonly Color MissColor = new(0.85f, 0.85f, 0.85f, 0.60f);

        // Call immediately after adding the component; starts the animation.
        public void Play(int damage, Action onDone)
        {
            _rt   = GetComponent<RectTransform>();
            _text = GetComponent<TextMeshProUGUI>();
            _cg   = GetComponent<CanvasGroup>();
            if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();

            _text.text  = damage > 0 ? $"-{damage}" : "0";
            _text.color = damage > 0 ? DmgColor : MissColor;

            StartCoroutine(Co_Animate(onDone));
        }

        private IEnumerator Co_Animate(Action onDone)
        {
            const float duration = 0.65f;
            var startPos = _rt.anchoredPosition;
            var endPos   = startPos + new Vector2(UnityEngine.Random.Range(-28f, 28f), 155f);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                _rt.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, Mathf.SmoothStep(0f, 1f, t));

                // Pop big on entry, hold, then shrink out
                float scale = t < 0.12f
                    ? Mathf.Lerp(1.6f, 1.15f, t / 0.12f)
                    : t < 0.68f ? 1.15f
                    : Mathf.Lerp(1.15f, 0f, (t - 0.68f) / 0.32f);
                transform.localScale = new Vector3(scale, scale, 1f);

                _cg.alpha = t < 0.50f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.50f) / 0.50f);
                yield return null;
            }

            onDone?.Invoke();
            Destroy(gameObject);
        }
    }
}
