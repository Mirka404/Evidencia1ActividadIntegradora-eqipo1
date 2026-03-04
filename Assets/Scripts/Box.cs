using UnityEngine;

public class Box : MonoBehaviour
{
    public int Id;
    private Transform boxTransform;

    void Awake()
    {
        boxTransform = this.transform;
    }

    // Método del UML
    public void UpdatePhysicalPosition(Vector3 newPos)
    {
        boxTransform.position = newPos;
    }
}