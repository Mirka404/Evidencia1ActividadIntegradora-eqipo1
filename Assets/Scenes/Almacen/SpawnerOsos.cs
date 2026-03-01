using UnityEngine;

public class SpawnerOsos : MonoBehaviour
{
    [SerializeField] private GameObject osoPolar;
    [SerializeField] private GameObject osoKika;
    [SerializeField] private Transform spawnKika;
    [SerializeField] private Transform spawnOsoPolar;
    [SerializeField] private int numeroOsoKika = 10;
    [SerializeField] private float separacionEntreOsoKika = 2f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void SpawnOsoPolar()
    {
        Vector3 position = new Vector3(spawnOsoPolar.position.x, spawnOsoPolar.position.y, spawnOsoPolar.position.z);
        
        Instantiate(osoPolar, position, Quaternion.identity);
    }

    void SpawnOsoKika()
    {
        
        for (int i = 0; i < numeroOsoKika; i++)
        {
            Vector3 position = new Vector3(spawnKika.position.x, spawnKika.position.y +(i * separacionEntreOsoKika), spawnKika.position.z);
            Instantiate(osoKika, position, Quaternion.identity);
        }
    }
    
    void Start()
    {
       SpawnOsoKika();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
