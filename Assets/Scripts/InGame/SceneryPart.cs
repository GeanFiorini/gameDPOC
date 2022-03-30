using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class SceneryPart : MonoBehaviour
{
    [Serializable]
    public struct SpawnableSettings
    {
        public Vector3 position;
    }

    [SerializeField] private float _length;
    [SerializeField] private SpawnableSettings[] _spawnableSettings;

    private WalkController _walkController;

    public float Length => this._length;

    private void Start()
    {
        this._walkController = FindObjectOfType<WalkController>();

        InstantiateSpawnables();
    }

    private void Update()
    {
        this.transform.Translate(new Vector3(0.0f, 0.0f, -this._walkController.CurrentSpeed * Time.deltaTime), Space.World);
    }

    private void OnDrawGizmos()
    {
        if (this._spawnableSettings != null)
        {
            foreach (SpawnableSettings settings in this._spawnableSettings)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(this.transform.position + settings.position, .5f);
                Gizmos.color = Color.white;
            }
        }
    }

    private void InstantiateSpawnables()
    {
        foreach (SpawnableSettings settings in this._spawnableSettings)
        {
            if (Random.value <= this._walkController.SpawnableChance)
            {
                GameObject spawnablePrefab = this._walkController.ChooseSpawnableBasedOnWeights();
                GameObject spawnable = Instantiate(spawnablePrefab, this.transform.position + settings.position, Quaternion.identity);
                spawnable.transform.SetParent(this.transform);
            }
        }
    }
}
