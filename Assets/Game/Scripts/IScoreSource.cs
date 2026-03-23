using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IScoreSource
{
    public void AddScore(ScoreType _scoreType, int _scoreValue, string _scoreDescription);
}
