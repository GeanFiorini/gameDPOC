using UnityEngine;
using TMPro;

public class ScoreController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private AnimationCurve _mmrcToScoreIncreaseCurve;

    private float _score;
    private PlayerStats _playerStats;

    public float Score => this._score;

    private void Start()
    {
        this._playerStats = FindObjectOfType<PlayerStats>();
        UpdateScoreText();
    }

    public void IncreaseScoreByDistanceWalked()
    {
        this._score += this._mmrcToScoreIncreaseCurve.Evaluate(this._playerStats.MMRC);
        UpdateScoreText();
    }

    public void IncreaseScoreByCorrectAnswer(float divisor)
    {
        this._score += 250f / divisor;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        this._scoreText.SetText($"{ (int)this._score }");
    }
}
