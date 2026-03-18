using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialAutoFade : MonoBehaviour
{
    [SerializeField] private float fadeDealy = 2f;
    [SerializeField] private float fadeDuration = 0.5f;

    private Renderer r;

    private void Start()
    {
        r = GetComponent<Renderer>();

        AutoFade();
    }

    private void AutoFade()
    {
        StartCoroutine(AutoFade_Coroutine());
    }

    private IEnumerator AutoFade_Coroutine()
    {
        yield return new WaitForSeconds(fadeDealy);

        var material = r.material;
        float alphaValue = material.color.a;

        float timer = 0;
        float progress = 0;

        while (timer < fadeDuration)
        {
            progress = timer / fadeDuration;

            alphaValue = Mathf.Lerp(1, 0, progress);
            material.color = new Color(material.color.r, material.color.g, material.color.b, alphaValue);

            timer += Time.deltaTime;

            yield return null;
        }

        alphaValue = 0;
        material.color = new Color(material.color.r, material.color.g, material.color.b, alphaValue);
    }

    
}
