using UnityEngine;

public class Spawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private int numberOfColums;
    [SerializeField] private int numberOfRows;
    [SerializeField] private GameObject prefabToSpawn;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float rowOffset;
    [SerializeField] private float columnOffset;
    [SerializeField] private LayerMask anaquelLayer;
    [SerializeField] private float checkRadius = 2f;
    [SerializeField] private int numberOfBoxes = 500;
    [SerializeField] private GameObject minBounds;
    [SerializeField] private GameObject maxBounds;
    [SerializeField] private GameObject boxPrefab;
    [SerializeField] private GameObject montacargasPrefab;
    [SerializeField] private int nMontacargas = 5;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnAnaquelesInGrid();
        SpawnBoxes();
        SpawnMontacargas();
    }
    
    
    
    private Vector3 GetRandomPositionInBounds()
    {
        float x = Random.Range(minBounds.transform.position.x, maxBounds.transform.position.x);
        float y = minBounds.transform.position.y; 
        float z = Random.Range(minBounds.transform.position.z, maxBounds.transform.position.z);

        return new Vector3(x, y, z);
    }
    private void SpawnBoxes()
    {
        int spawned = 0;
        int attempts = 0;

        while (spawned < numberOfBoxes && attempts < numberOfBoxes * 20)
        {
            attempts++;

            Vector3 randomPos = GetRandomPositionInBounds();

            if (!Physics.CheckSphere(randomPos, checkRadius, anaquelLayer))
            {
                Instantiate(boxPrefab, randomPos, Quaternion.identity);
                spawned++;
            }
        }
    }

    private void SpawnMontacargas()
    {
        int spawned = 0;
        int attempts = 0;

        while (spawned < nMontacargas && attempts < nMontacargas * 20)
        {
            attempts++;

            Vector3 randomPos = GetRandomPositionInBounds();

            if (!Physics.CheckSphere(randomPos, checkRadius, anaquelLayer))
            {
                Instantiate(montacargasPrefab, randomPos, Quaternion.identity);
                spawned++;
            }
        }
    }
    private void SpawnAnaquelesInGrid()
    {
        Vector3 startPos = spawnPoint.position;
        for (int i = 0; i < numberOfColums; i++)
        {
          
            for (int j = 0; j < numberOfRows; j++)
            {
                
                float x = startPos.x - i * columnOffset;   // columnas = X
                float z = startPos.z - j * rowOffset;    
                Instantiate(
                    prefabToSpawn,
                    new Vector3(x, startPos.y, z),
                    Quaternion.identity,
                    spawnPoint
                );
            }
        }
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
