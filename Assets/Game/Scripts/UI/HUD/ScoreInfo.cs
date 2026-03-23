using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreValueTMP;
    [SerializeField] private TextMeshProUGUI scoreDescriptionTMP;

    public bool hasStartedDisappearing { get; private set; } = false;
    public bool isCompletelyDisappeared { get; private set; } = false;

    private Coroutine disappearCoroutine;

    //public int scoreValue { get; private set; }
    //public int streakNumber { get; private set; }


    private void Start()
    {
        RefreshVisibleCharacters();
    }

    private void OnDisable()
    {
        scoreValueTMP.text = string.Empty;
        scoreDescriptionTMP.text = string.Empty;

        hasStartedDisappearing = false;
        isCompletelyDisappeared = false;

        if (disappearCoroutine != null)
        {
            StopCoroutine(disappearCoroutine);
        }
        disappearCoroutine = null;
    }

    public void SetupScoreInfo(int _scoreValue, string _scoreDescription, int _streakNumber)
    {
        if (disappearCoroutine != null)
        {
            StopCoroutine(disappearCoroutine);
            disappearCoroutine = null;
        }

        hasStartedDisappearing = false;
        isCompletelyDisappeared = false;

        int totalScoreValue = _scoreValue * _streakNumber;
        scoreValueTMP.text = "+" + totalScoreValue.ToString();

        scoreDescriptionTMP.text = _scoreDescription;

        if (_streakNumber >= 2)
        {
            scoreDescriptionTMP.text += $" x{_streakNumber}";
        }

        RefreshVisibleCharacters();
    }

    public void Disappear()
    {
        if (disappearCoroutine != null)
            return;

        disappearCoroutine = StartCoroutine(Disappear_Coroutine());
    }

    private IEnumerator Disappear_Coroutine()
    {
        hasStartedDisappearing = true;

        while (scoreDescriptionTMP.maxVisibleCharacters > 0)
        {
            scoreDescriptionTMP.maxVisibleCharacters--;
            scoreDescriptionTMP.ForceMeshUpdate();

            yield return new WaitForSeconds(0.03f);
        }

        while (scoreValueTMP.maxVisibleCharacters > 0)
        {
            scoreValueTMP.maxVisibleCharacters--;
            scoreValueTMP.ForceMeshUpdate();

            yield return new WaitForSeconds(0.03f);
        }

        isCompletelyDisappeared = true;
        disappearCoroutine = null;
        //SpawnUtility.DestroyObject(gameObject);
    }

    private void RefreshVisibleCharacters()
    {
        scoreValueTMP.ForceMeshUpdate();
        scoreValueTMP.maxVisibleCharacters = scoreValueTMP.textInfo.characterCount;

        scoreDescriptionTMP.ForceMeshUpdate();
        scoreDescriptionTMP.maxVisibleCharacters = scoreDescriptionTMP.textInfo.characterCount;
    }
}
