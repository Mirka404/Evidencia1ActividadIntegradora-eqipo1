using UnityEngine;

public class addBox : MonoBehaviour
{
    [Header("Prefab to create (will be controlled after instantiation)")]
    [SerializeField] private GameObject prefabToShowHide;

    [Header("Where to spawn it")]
    [SerializeField] private Transform spawnPoint;

    [Header("Default state")]
    [SerializeField] private bool visibleByDefault = false;

    [Header("Runtime state (editable in Inspector during Play Mode)")]
    [SerializeField] private bool isVisible = false;

    [Header("Touch box to change visibility")]
    [SerializeField] private bool changeWhenTouchingBox = true;
    [SerializeField] private string boxTag = "Box";
    [SerializeField] private bool toggleOnTouch = true;   
    [SerializeField] private bool setVisibleOnTouch = true;

    private GameObject spawnedInstance;
    private MeshRenderer[] spawnedRenderers;
 

    void Awake()
    {
        if (spawnPoint == null)
            spawnPoint = transform;

        isVisible = visibleByDefault;

        EnsureSpawned();
        ApplyVisibility();
    }

    void Update()
    {
  
        ApplyVisibility();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!changeWhenTouchingBox) return;
        if (!other.CompareTag(boxTag)) return;

        if (toggleOnTouch) isVisible = !isVisible;
        else isVisible = setVisibleOnTouch;

        EnsureSpawned();
        ApplyVisibility();
    }

    private void EnsureSpawned()
    {
        if (spawnedInstance != null) return;

        if (prefabToShowHide == null)
        {
            Debug.LogWarning($"{nameof(boxspawner)}: prefabToShowHide is not assigned.", this);
            return;
        }
        Vector3 spawnedPosition = new Vector3( spawnPoint.position.x, spawnPoint.position.y-0.28f, spawnPoint.position.z);
        spawnedInstance = Instantiate(prefabToShowHide, spawnedPosition, spawnPoint.rotation);
        spawnedInstance.name = prefabToShowHide.name + " (Spawned)";

      
        spawnedRenderers = spawnedInstance.GetComponentsInChildren<MeshRenderer>(true);

        if (spawnedRenderers == null || spawnedRenderers.Length == 0)
            Debug.LogWarning($"{nameof(boxspawner)}: Spawned prefab has no MeshRenderer components.", spawnedInstance);
    }

    private void ApplyVisibility()
    {
        if (spawnedInstance == null || spawnedRenderers == null) return;

        for (int i = 0; i < spawnedRenderers.Length; i++)
        {
            if (spawnedRenderers[i] != null)
                spawnedRenderers[i].enabled = isVisible;
        }
    }
}