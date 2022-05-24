using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private Image _mmrcBar;
    [SerializeField] private Gradient _mmrcBarColors;
    [SerializeField] private Transform _healthBar;

    private float _health;
    private float _mmrc;
    private Player _player;
    private WalkController _walkController;
    public int _coins;

    public float Health => this._health;
    public float MMRC => this._mmrc;

    private void Awake()
    {
        this._player = GetComponent<Player>();
        this._health = 1f;
        this._walkController = FindObjectOfType<WalkController>();
        this._coins = 0;

        UpdateMMRCBar();
        UpdateHealthBar();
    }

    public void OnPlayerHitRiskFactor(float damage)
    {
        this._mmrc += this._walkController.MMRCIncreaseFactor * damage;
        UpdateMMRCBar();

        this._health -= this._walkController.HealthDecreaseCurve.Evaluate(Mathf.Clamp01(this._mmrc / 4f));
        this._health = Mathf.Clamp01(this._health);
        UpdateHealthBar();

        if (this._mmrc >= 4f)
        {
            this._walkController.OnPlayerDeath();
            this._player.OnDeath();
        }
    }

    public void OnPlayerHitInhaler(float damage)
    {
        
        this._mmrc -= damage;
        this._mmrc = Mathf.Clamp(_mmrc, 0, 4);
        UpdateMMRCBar();

        this._health -= this._walkController.HealthDecreaseCurve.Evaluate(Mathf.Clamp01(this._mmrc / 4f));
        this._health = Mathf.Clamp01(this._health);
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        this._healthBar.transform.localScale = new Vector3(Mathf.Clamp01(this._health), 1f, 1f);
    }

    private void UpdateMMRCBar()
    {
        float mmrcPercent = this._mmrc / 4f;
        this._mmrcBar.transform.localScale = new Vector3(Mathf.Clamp01(mmrcPercent), 1f, 1f);
        this._mmrcBar.color = this._mmrcBarColors.Evaluate(mmrcPercent);
    }
}
