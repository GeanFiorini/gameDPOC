using UnityEngine;

// Escolhe um eixo aleatório e rotaciona ao seu redor ao longo
// da vida do objeto
// Está inserido nos prefabs das Hemácias
public class RandomRotator : MonoBehaviour
{
    public float rotationSpeed;

    private Vector3 axis;

    private void Start()
    {
        this.axis = Random.insideUnitSphere; // eixo aleatório
        this.transform.rotation = Quaternion.Euler(Random.insideUnitSphere * 360.0f); // começa com rotação aleatória
    }

    private void Update()
    {
        this.transform.Rotate(this.axis, Time.deltaTime * this.rotationSpeed);
    }
}
