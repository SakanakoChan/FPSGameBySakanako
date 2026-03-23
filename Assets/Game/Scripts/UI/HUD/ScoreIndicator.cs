using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ScoreType
{
    Kill,
    Headshot
}


public class ScoreIndicator : MonoBehaviour
{
    private class ScoreStreakState
    {
        public ScoreInfo scoreInfo;
        public int streakNumber;
        public float countDown;
    }

    [SerializeField] private GameObject scoreInfoPrefab;
    [SerializeField] private int maxScoreInfoNumber = 5;
    [SerializeField] private float scoreStreakWindow = 5;

    private Dictionary<ScoreType, ScoreStreakState> scoreStreakDictionary = new Dictionary<ScoreType, ScoreStreakState>();
    private List<ScoreType> expiredScoreStreakTypeList = new List<ScoreType>();

    private void Start()
    {
        GameEvents.OnScore += AddScore;
    }

    private void Update()
    {
        foreach (var pair in scoreStreakDictionary)
        {
            var state = pair.Value;
            state.countDown -= Time.deltaTime;

            if (state.countDown < 0)
            {
                if (state.scoreInfo == null)
                    continue;

                if (state.scoreInfo.hasStartedDisappearing == false)
                    state.scoreInfo.Disappear();

                if (state.scoreInfo.isCompletelyDisappeared)
                {
                    expiredScoreStreakTypeList.Add(pair.Key);
                }
            }
        }

        foreach (var type in expiredScoreStreakTypeList)
        {
            if (scoreStreakDictionary.TryGetValue(type, out var state))
            {
                if (state.scoreInfo != null)
                {
                    SpawnUtility.DestroyObject(state.scoreInfo.gameObject);
                    state.scoreInfo = null;
                }
            }

            scoreStreakDictionary.Remove(type);
        }

        expiredScoreStreakTypeList.Clear();
    }

    private void OnDestroy()
    {
        GameEvents.OnScore -= AddScore;
    }


    public void AddScore(ScoreType _scoreType, int _scoreValue, string _scoreDescription)
    {
        ScoreStreakState state;

        if (scoreStreakDictionary.ContainsKey(_scoreType) == false)
        {
            state = new ScoreStreakState();

            state.streakNumber = 1;
            state.countDown = scoreStreakWindow;

            var scoreInfo = SpawnUtility.SpawnObject(scoreInfoPrefab);
            scoreInfo.transform.SetParent(transform);

            var scoreInfoScript = scoreInfo.GetComponent<ScoreInfo>();
            scoreInfoScript?.SetupScoreInfo(_scoreValue, _scoreDescription, state.streakNumber);

            state.scoreInfo = scoreInfoScript;

            scoreStreakDictionary[_scoreType] = state;
        }
        else
        {
            state = scoreStreakDictionary[_scoreType];

            state.streakNumber++;
            state.countDown = scoreStreakWindow;

            state.scoreInfo?.SetupScoreInfo(_scoreValue, _scoreDescription, state.streakNumber);
        }
    }
}
