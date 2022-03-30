using UnityEngine;

public class Spawnable : MonoBehaviour
{
    public enum SpawnableType
    {
        RiskFactor
    }

    [SerializeField] private SpawnableType _type;
    [SerializeField] private float _mmrcIncreaseWeight;

    private WalkController _walkController;
    private ParticleSystem[] _particleSystems; // Utilizado para controlar a velocidade das particulas, já que é o cenário que se move

    private void Start()
    {
        this._walkController = FindObjectOfType<WalkController>();
        this._particleSystems = GetComponentsInChildren<ParticleSystem>();
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
