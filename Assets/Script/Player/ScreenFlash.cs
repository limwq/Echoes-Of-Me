using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenFlash : MonoBehaviour
{
    public void TriggerFlash() {
        StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine() {
        CanvasGroup cg = GetComponent<CanvasGroup>();
        cg.alpha = 1; // Instant White
        while (cg.alpha > 0) {
            cg.alpha -= Time.deltaTime; // Fade out fast
            yield return null;
        }
    }
}
