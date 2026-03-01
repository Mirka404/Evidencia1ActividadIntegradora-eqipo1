using UnityEngine;

public class imprimirPosicion : MonoBehaviour
{
    [SerializeField]public string nombre;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {   
        Debug.Log(nombre);
    }
}
