using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkController : MonoBehaviour
{
    [Header("Difficulty Controls")]
    [SerializeField] private AnimationCurve _mmrcToSpeedCurve;
    [SerializeField] private AnimationCurve _mmrcToSpawnableChanceCurve;
    [SerializeField] private AnimationCurve _healthDecreaseCurve;
    [SerializeField] private float _mmrcIncreaseFactor;
    [SerializeField] private float _minimumDistanceToWalk;

    [Header("Scenery Stuff")]
    [Tooltip("Partes que já estão na cena (Opcional)")]
    [SerializeField] private SceneryPart[] _initialParts;
    [Tooltip("Partes que serão instanciadas aleatóriamente durante a caminhada")]
    [SerializeField] private GameObject[] _randomPartsPrefabs;
    [Tooltip("Chances de cada spawnable ser selecionado, utiliza mesma ordem dos prefabs")]
    [SerializeField] private float[] _spawnableWeights;
    [Tooltip("Prefabs dos fatores de risco e power ups")]
    [SerializeField] private GameObject[] _spawnablePrefabs;
    [SerializeField] private GameObject _finishLinePrefab;

    [Header("Misc")]
    [SerializeField] private GameObject _firstQuiz;
    [SerializeField] private GameObject _loseScreen;

    private List<SceneryPart> activeParts = new List<SceneryPart>();
    private float _currentSpeed;
    private float _spawnableChance;
    private bool _isGameOver;
    private float _totalDistanceWalked;
    private float _totalDistanceSpawned; // O total de percurso que já foi instanciado na cena
    private float[] _spawnableWeightsSumCache; // Utilizado para escolher um spawnable a partir dos pesos (cache para não criar um array toda hora) 
    private bool _isPaused;
    private Player _player;
    private PlayerStats _playerStats;
    private float _lastSavedSpeed;
    private bool _isFinishLineSpawned;
    private ScoreController _scoreController;
    private float _lastScoreDist; // Utilizado pra adicionar pontos a cada distância

    public AnimationCurve HealthDecreaseCurve => this._healthDecreaseCurve;
    public float MMRCIncreaseFactor => this._mmrcIncreaseFactor;
    public bool IsGameOver => this._isGameOver;
    public float CurrentSpeed => this._currentSpeed;
    public float SpawnableChance => this._spawnableChance;
    public bool IsGamePaused => this._isPaused;

    private void Start()
    {
        this._player = FindObjectOfType<Player>();
        this._playerStats = FindObjectOfType<PlayerStats>();

        UpdateDifficultyBasedOnMMRC();

        foreach (SceneryPart part in this._initialParts)
        {
            this.activeParts.Add(part);
        }

        CacheSpawnableWeightSum();

        for (int i = 0; i < 7; i++)
        {
            SpawnRandomPartAtEnd();
        }

        this._totalDistanceSpawned = CalculateInitialSpawnedDistance();

        this._scoreController = FindObjectOfType<ScoreController>();

        PauseWalk();
    }

    private void LateUpdate()
    {
        if (!this._isPaused)
        {
            if (!this._isGameOver)
            {
                this._totalDistanceWalked += this._currentSpeed * Time.deltaTime;
            }

            if (this.activeParts.Count > 0)
            {
                if (ShouldDestroyFirstPart())
                {
                    DestroyFirstPart();
                    SpawnRandomPartAtEnd();
                }
            }

            if (this._totalDistanceWalked - this._lastScoreDist >= 1f)
            {
                this._scoreController.IncreaseScoreByDistanceWalked();
                this._lastScoreDist += 1f;
            }

            UpdateDifficultyBasedOnMMRC();
        }
    }

    public void PauseWalk()
    {
        this._lastSavedSpeed = this._currentSpeed;
        this._currentSpeed = 0f;
        this._isPaused = true;
    }

    public void ResumeWalk()
    {
        this._currentSpeed = this._lastSavedSpeed;
        this._isPaused = false;
    }

    public GameObject ChooseSpawnableBasedOnWeights()
    {
        float totalWeight = this._spawnableWeightsSumCache[this._spawnableWeightsSumCache.Length - 1];
        float randomValue = Random.value * totalWeight;

        for (int i = 0; i < this._spawnableWeightsSumCache.Length; i++)
            if (randomValue <= this._spawnableWeightsSumCache[i] && this._spawnableWeightsSumCache[i] != 0.0f)
                return this._spawnablePrefabs[i];

        return this._spawnablePrefabs[Random.Range(0, this._spawnablePrefabs.Length)]; // retorno padrão, não deve cair aqui
    }

    public void OnPlayerDeath()
    {
        this._currentSpeed = 0;
        this._isGameOver = true;
        StartCoroutine(ShowGameOverScreen());
    }

    public void OnReacheadFinishLine()
    {
        this._currentSpeed = 0f;
        this._isGameOver = true;
        StartCoroutine(StartQuiz());
    }

    private IEnumerator ShowGameOverScreen()
    {
        yield return new WaitForSeconds(3f);

        this._loseScreen.SetActive(true);
    }

    private IEnumerator StartQuiz()
    {
        yield return new WaitForSeconds(5f);

        this._firstQuiz.SetActive(true);
    }

    private void UpdateDifficultyBasedOnMMRC()
    {
        if (this._isGameOver || this._isPaused) return;

        this._currentSpeed = this._mmrcToSpeedCurve.Evaluate(this._playerStats.MMRC);
        this._spawnableChance = this._mmrcToSpawnableChanceCurve.Evaluate(this._playerStats.MMRC);
    }

    // Utilizado para estimar a distancia correta ao criar a linha de chegada
    private float CalculateInitialSpawnedDistance()
    {
        float dist = 0.0f;
        for (int i = 1; i < this.activeParts.Count; i++) // ignora a primeira parte que deve estar atras do player
        {
            dist += this.activeParts[i].Length;
        }
        return dist;
    }

    private void SpawnRandomPartAtEnd()
    {
        if (this._isFinishLineSpawned) return;

        float spawnPos = 0.0f;
        if (this.activeParts.Count > 0)
        {
            SceneryPart lastPart = this.activeParts[this.activeParts.Count - 1];
            spawnPos = lastPart.transform.position.z + lastPart.Length;
        }

        GameObject partPrefab = this._randomPartsPrefabs[Random.Range(0, this._randomPartsPrefabs.Length)];
        if (this._totalDistanceSpawned > this._minimumDistanceToWalk)
        {
            partPrefab = this._finishLinePrefab;
            this._isFinishLineSpawned = true;
        }

        SceneryPart spawnedPart = Instantiate(partPrefab, new Vector3(0f, 0f, spawnPos), Quaternion.identity).GetComponent<SceneryPart>();
        this.activeParts.Add(spawnedPart);
        this._totalDistanceSpawned += spawnedPart.Length;
    }

    private void DestroyFirstPart()
    {
        SceneryPart firstPart = this.activeParts[0];
        this.activeParts.RemoveAt(0);
        Destroy(firstPart.gameObject);
    }

    private bool ShouldDestroyFirstPart()
    {
        if (this.activeParts.Count == 0)
        {
            return false;
        }

        SceneryPart firstPart = this.activeParts[0];
        return firstPart.transform.position.z < -firstPart.Length * 2f;
    }

    private void CacheSpawnableWeightSum()
    {
        this._spawnableWeightsSumCache = new float[this._spawnableWeights.Length];
        this._spawnableWeightsSumCache[0] = this._spawnableWeights[0];
        for (int i = 1; i < this._spawnableWeightsSumCache.Length; i++)
        {
            this._spawnableWeightsSumCache[i] = this._spawnableWeights[i] + this._spawnableWeightsSumCache[i - 1];
        }
    }
}
