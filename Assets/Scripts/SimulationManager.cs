using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SimulationManager : MonoBehaviour
{
public GridCell[,] grid; 
public GameObject robotPrefab;
public GameObject boxPrefab;
public GameObject floorPrefab; 
public Vector2Int GridSize = new Vector2Int(25, 20); 
public float maxTime = 300f; 
private int totalMoves;
public int numberOfBoxes = 10; 
[Header("Configuración de Entorno")]
public float cellSize = 1f; 
public LayerMask obstacleLayer; 
private List<Agent> agents = new List<Agent>();
private float elapsedTime = 0f;
public float stepInterval = 0.05f; 
void Start()
{
if (floorPrefab == null || boxPrefab == null || robotPrefab == null)
{
Debug.LogError("¡Faltan prefabs en el GameManager! Revisa el Inspector.");
return;
}
Initialize3DEnvironment();
StartCoroutine(SimulationLoop());
}
public void Initialize3DEnvironment()
{
grid = new GridCell[GridSize.x, GridSize.y];
Vector3 startOrigin = transform.position; 
for (int x = 0; x < GridSize.x; x++)
{
for (int z = 0; z < GridSize.y; z++)
{
Vector3 worldPos = startOrigin + new Vector3(x * cellSize, 0, z * cellSize);
GameObject floorObj = Instantiate(floorPrefab, worldPos, Quaternion.identity, this.transform);
floorObj.name = $"Cell_{x}_{z}";
GridCell cell = floorObj.AddComponent<GridCell>();
cell.LogicalPosition = new Vector3Int(x, 0, z);
cell.WorldPosition = worldPos;
if (Physics.CheckSphere(worldPos, cellSize * 0.4f, obstacleLayer))
{
cell.IsWall = true;
Renderer render = floorObj.GetComponentInChildren<Renderer>();
if (render != null)
{
}
}
grid[x, z] = cell;
}
}
for (int i = 0; i < numberOfBoxes; i++)
{
Vector3Int randomPos = GetRandomEmptyPosition();
Vector3 worldPos = grid[randomPos.x, randomPos.z].WorldPosition;
GameObject boxObj = Instantiate(boxPrefab, worldPos, Quaternion.identity);
Box newBox = boxObj.GetComponent<Box>();
if (newBox == null) 
{
newBox = boxObj.AddComponent<Box>();
}
newBox.Id = i;
grid[randomPos.x, randomPos.z].AddBox3D(newBox);
}
for (int i = 0; i < 5; i++)
{
Vector3Int randomPos = GetRandomEmptyPosition();
Vector3 worldPos = grid[randomPos.x, randomPos.z].WorldPosition;
GameObject agentObj = Instantiate(robotPrefab, worldPos, Quaternion.identity);
Agent newAgent = agentObj.GetComponent<Agent>();
if (newAgent == null) 
{
newAgent = agentObj.AddComponent<Agent>();
}
// CORRECCIÓN: Ya no hay roles, todos inician igual
newAgent.Initialize(i, randomPos, this);
grid[randomPos.x, randomPos.z].currentAgent = newAgent;
agents.Add(newAgent);
}
}
private IEnumerator SimulationLoop()
{
while (!IsSimulationFinished())
{
ExecuteStep();
yield return new WaitForSeconds(stepInterval);
elapsedTime += stepInterval;
}
CalculateMetrics();
Debug.Log("Simulación Terminada.");
}
public void ExecuteStep()
{
foreach (Agent agent in agents)
{
agent.DecideAction();
}
}
public void CalculateMetrics()
{
totalMoves = 0;
foreach (Agent agent in agents)
{
totalMoves += agent.MoveCount;
}
Debug.Log($"Tiempo total: {elapsedTime} segundos.");
Debug.Log($"Movimientos totales realizados: {totalMoves}");
}
public bool IsSimulationFinished()
{
if (elapsedTime >= maxTime) return true;
return false;
}
public bool IsValidPosition(Vector3Int pos)
{
return pos.x >= 0 && pos.x < GridSize.x && pos.z >= 0 && pos.z < GridSize.y;
}
private Vector3Int GetRandomEmptyPosition()
{
int x, z;
int safetyNet = 0; 
do
{
x = Random.Range(0, GridSize.x);
z = Random.Range(0, GridSize.y);
safetyNet++;
if (safetyNet > 2000) break; 
} 
while (grid[x, z] == null || grid[x, z].IsOccupied() || grid[x, z].GetStackHeight() > 0 || grid[x, z].IsWall);
return new Vector3Int(x, 0, z);
}
}