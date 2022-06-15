using UnityEngine;

public class Spawnable : MonoBehaviour
{
    public enum SpawnableType
    {
        RiskFactor,
        Coin,
        Box,
        Inhaler
    }

    [SerializeField] private SpawnableType _type;
    [SerializeField] private float _mmrcIncreaseWeight;

    private WalkController _walkController;
    private ParticleSystem[] _particleSystems; // Utilizado para controlar a velocidade das particulas, já que é o cenário que se move
    private Canvas _canva;

    private void Start()
    {
        this._walkController = FindObjectOfType<WalkController>();
        this._particleSystems = GetComponentsInChildren<ParticleSystem>();
        this._canva = FindObjectOfType<Canvas>();
    }

    private void Update()
    {
        UpdateParticlesVelocity();
    }

    public void OnHitPlayer(Player player, PlayerStats playerStats)
    {
        if (this._type == SpawnableType.RiskFactor)
        {
            playerStats.OnPlayerHitRiskFactor(this._mmrcIncreaseWeight);
        }

        else if (this._type == SpawnableType.Coin)
        {
            playerStats._coins += 1;
            _walkController._coinsText.SetText(playerStats._coins.ToString());
        }

        else if (this._type == SpawnableType.Inhaler)
        {
            playerStats.OnPlayerHitInhaler(0.2f);
        }

        else if (this._type == SpawnableType.Box)
        {
            _walkController.PauseWalk();
            Instantiate(_walkController._questions[Random.Range(0, _walkController._questions.Length)], _walkController._questions_position.transform);
        }

        Destroy(this.gameObject);
    }

    private void UpdateParticlesVelocity()
    {
        foreach (ParticleSystem system in this._particleSystems)
        {
            ParticleSystem.VelocityOverLifetimeModule module = system.velocityOverLifetime;
            module.enabled = true;
            module.space = ParticleSystemSimulationSpace.World;
            module.z = -this._walkController.CurrentSpeed;
        }
    }
}
