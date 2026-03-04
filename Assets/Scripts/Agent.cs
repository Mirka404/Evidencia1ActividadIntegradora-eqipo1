using System.Collections.Generic;
using UnityEngine;
public enum AgentState { Collecting, DroppingOff, Parking, Done }
public class Agent : MonoBehaviour
{
public int Id;
public Vector3Int CurrentPosition;
public Vector3Int initialPos; 
public SimulationManager SimManager;
public int MoveCount { get; private set; } = 0; 
public List<Box> carriedBoxes = new List<Box>(); 
public AgentState currentState;
private Vector3 targetVisualPosition;
private float visualMoveSpeed = 30f;
private Vector3 visualOffset;
private float visualTurnSpeed = 200f; 
public static List<Vector3Int> targetedBoxes = new List<Vector3Int>();
public static List<Vector3Int> reservedDropOffs = new List<Vector3Int>();
public static List<Vector3Int> completedDropOffs = new List<Vector3Int>();
private Vector3Int? myTargetBox = null;
private Vector3Int? dropOffTarget = null;
private float waitAfterCollisionTimer = 0f;
private Vector3Int? desiredCollisionStep = null;
public void Initialize(int id, Vector3Int startPos, SimulationManager manager)
{
Id = id;
if (id == 0) 
{
targetedBoxes.Clear(); 
reservedDropOffs.Clear();
completedDropOffs.Clear();
}
CurrentPosition = startPos;
initialPos = startPos; 
SimManager = manager;
currentState = AgentState.Collecting;
visualOffset = new Vector3((id * 0.1f) - 0.2f, 0, (id * 0.1f) - 0.2f);
targetVisualPosition = manager.grid[startPos.x, startPos.z].WorldPosition + visualOffset;
transform.position = targetVisualPosition;
waitAfterCollisionTimer = 0f;
}
void Update()
{
transform.position = Vector3.MoveTowards(transform.position, targetVisualPosition, Time.deltaTime * visualMoveSpeed);
if (waitAfterCollisionTimer > 0)
{
waitAfterCollisionTimer -= Time.deltaTime;
if (waitAfterCollisionTimer <= 0) desiredCollisionStep = null;
}
}
public void DecideAction()
{
if (waitAfterCollisionTimer > 0) return;
if (currentState == AgentState.Collecting)
{
if (carriedBoxes.Count >= 5)
{
if (myTargetBox.HasValue) targetedBoxes.Remove(myTargetBox.Value);
myTargetBox = null;
currentState = AgentState.DroppingOff;
}
else if (CountBoxesOnFloor() == 0)
{
if (myTargetBox.HasValue) targetedBoxes.Remove(myTargetBox.Value);
myTargetBox = null;
if (carriedBoxes.Count > 0 && carriedBoxes.Count < 5) currentState = AgentState.Done; 
else if (carriedBoxes.Count == 5) currentState = AgentState.DroppingOff;
else currentState = AgentState.Parking;
}
else SeekNearestBox();
}
else if (currentState == AgentState.DroppingOff) HandleDroppingOff();
else if (currentState == AgentState.Parking) HandleParking();
else if (currentState == AgentState.Done) SeekPartnerForConsolidation(); 
YieldToOthers(); 
}
private void SeekPartnerForConsolidation()
{
if (carriedBoxes.Count >= 5)
{
currentState = AgentState.DroppingOff;
return;
}
if (carriedBoxes.Count == 0)
{
currentState = AgentState.Parking;
return;
}
Agent bestPartner = null;
float minDistance = float.MaxValue;
Agent[] allAgents = FindObjectsByType<Agent>(FindObjectsSortMode.None);
foreach (Agent a in allAgents)
{
if (a.Id != this.Id && a.currentState == AgentState.Done && a.carriedBoxes.Count > 0)
{
float dist = Vector3.Distance(CurrentPosition, a.CurrentPosition);
if (dist < minDistance)
{
minDistance = dist;
bestPartner = a;
}
}
}
if (bestPartner != null)
{
float distToPartner = Vector3.Distance(CurrentPosition, bestPartner.CurrentPosition);
if (distToPartner <= 1.5f)
{
if (this.Id > bestPartner.Id && this.carriedBoxes.Count > 0 && bestPartner.carriedBoxes.Count < 5)
{
int boxesToGive = 5 - bestPartner.carriedBoxes.Count;
while (boxesToGive > 0 && this.carriedBoxes.Count > 0)
{
Box b = this.carriedBoxes[this.carriedBoxes.Count - 1];
this.carriedBoxes.RemoveAt(this.carriedBoxes.Count - 1);
bestPartner.AddBoxFromAgent(b);
boxesToGive--;
}
}
}
else MoveTowardsYield(bestPartner.CurrentPosition);
}
else RandomWalk();
}
public void AddBoxFromAgent(Box b)
{
b.transform.SetParent(this.transform);
float boxYOffset = 1.5f + (carriedBoxes.Count * 0.75f); 
b.transform.localPosition = new Vector3(0, boxYOffset, 0);
carriedBoxes.Add(b);
}
private int CountBoxesOnFloor()
{
int count = 0;
for (int x = 0; x < SimManager.GridSize.x; x++)
{
for (int z = 0; z < SimManager.GridSize.y; z++)
{
Vector3Int pos = new Vector3Int(x, 0, z);
if (SimManager.grid[x, z] != null && !completedDropOffs.Contains(pos)) count += (int)SimManager.grid[x, z].GetStackHeight();
}
}
return count;
}
private void YieldToOthers()
{
Vector3Int[] directions = { Vector3Int.right, Vector3Int.left, Vector3Int.forward, Vector3Int.back };
foreach (var dir in directions)
{
Vector3Int neighborPos = CurrentPosition + dir;
if (SimManager.IsValidPosition(neighborPos))
{
GridCell cell = SimManager.grid[neighborPos.x, neighborPos.z];
if (cell != null && cell.IsOccupied() && cell.currentAgent != this && cell.currentAgent.waitAfterCollisionTimer <= 0)
{
if (desiredCollisionStep.HasValue && cell.currentAgent.desiredCollisionStep.HasValue && desiredCollisionStep.Value == cell.currentAgent.desiredCollisionStep.Value) waitAfterCollisionTimer = 0.5f;
RandomWalk();
}
}
}
}
private void SeekNearestBox()
{
if (myTargetBox.HasValue)
{
GridCell c = SimManager.grid[myTargetBox.Value.x, myTargetBox.Value.z];
if (c == null || c.GetStackHeight() == 0)
{
targetedBoxes.Remove(myTargetBox.Value);
myTargetBox = null;
}
}
if (!myTargetBox.HasValue)
{
float minDistance = float.MaxValue;
for (int x = 0; x < SimManager.GridSize.x; x++)
{
for (int z = 0; z < SimManager.GridSize.y; z++)
{
GridCell cell = SimManager.grid[x, z];
if (cell != null && cell.GetStackHeight() > 0 && !cell.IsWall)
{
Vector3Int pos = new Vector3Int(x, 0, z);
if (!targetedBoxes.Contains(pos) && !completedDropOffs.Contains(pos)) 
{
float dist = Vector3.Distance(CurrentPosition, pos);
if (dist < minDistance)
{
minDistance = dist;
myTargetBox = pos;
}
}
}
}
}
if (myTargetBox.HasValue) targetedBoxes.Add(myTargetBox.Value);
}
if (myTargetBox.HasValue)
{
float distToTarget = Vector3.Distance(CurrentPosition, myTargetBox.Value);
if (distToTarget <= 1.5f) 
{
GridCell targetCell = SimManager.grid[myTargetBox.Value.x, myTargetBox.Value.z];
Box box = targetCell.RemoveTopBox3D();
if (box != null)
{
box.transform.SetParent(this.transform);
float boxYOffset = 1.5f + (carriedBoxes.Count * 0.75f); 
box.transform.localPosition = new Vector3(0, boxYOffset, 0);
carriedBoxes.Add(box);
}
targetedBoxes.Remove(myTargetBox.Value);
myTargetBox = null;
}
else MoveTowardsYield(myTargetBox.Value);
}
else RandomWalk(); 
}
private void HandleDroppingOff()
{
if (!dropOffTarget.HasValue)
{
for (int x = 0; x < SimManager.GridSize.x; x++)
{
for (int z = 0; z < SimManager.GridSize.y; z++)
{
GridCell cell = SimManager.grid[x, z];
if (cell != null && !cell.IsWall && cell.GetStackHeight() == 0)
{
Vector3Int pos = new Vector3Int(x, 0, z);
if (!reservedDropOffs.Contains(pos))
{
dropOffTarget = pos;
reservedDropOffs.Add(pos);
break;
}
}
}
if (dropOffTarget.HasValue) break;
}
}
if (dropOffTarget.HasValue)
{
float distToDropOff = Vector3.Distance(CurrentPosition, dropOffTarget.Value);
if (distToDropOff <= 1.5f)
{
GridCell targetCell = SimManager.grid[dropOffTarget.Value.x, dropOffTarget.Value.z];
foreach (Box b in carriedBoxes)
{
b.transform.SetParent(null);
targetCell.AddBox3D(b);
}
carriedBoxes.Clear();
completedDropOffs.Add(dropOffTarget.Value); 
reservedDropOffs.Remove(dropOffTarget.Value);
dropOffTarget = null;
if (CountBoxesOnFloor() > 0) currentState = AgentState.Collecting;
else currentState = AgentState.Parking;
}
else MoveTowardsYield(dropOffTarget.Value);
}
else RandomWalk();
}
private void HandleParking()
{
if (CurrentPosition == initialPos)
{
currentState = AgentState.Done;
myTargetBox = null;
return;
}
if (SimManager.grid[initialPos.x, initialPos.z].IsOccupied() && SimManager.grid[initialPos.x, initialPos.z].currentAgent != this)
{
currentState = AgentState.Done;
myTargetBox = null;
return;
}
MoveTowardsYield(initialPos);
}
private void MoveTowardsYield(Vector3Int target)
{
Vector3Int nextStep = CalculateNextStepBFS(target);
if (nextStep != CurrentPosition)
{
if (!TryMoveSmartYield(nextStep)) RandomWalk(); 
}
else RandomWalk();
}
private Vector3Int CalculateNextStepBFS(Vector3Int target)
{
Queue<Vector3Int> queue = new Queue<Vector3Int>();
Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();
queue.Enqueue(CurrentPosition);
cameFrom[CurrentPosition] = CurrentPosition;
bool found = false;
while (queue.Count > 0)
{
Vector3Int current = queue.Dequeue();
if (current == target)
{
found = true;
break;
}
Vector3Int[] dirs = { Vector3Int.right, Vector3Int.left, Vector3Int.forward, Vector3Int.back };
foreach (Vector3Int dir in dirs)
{
Vector3Int next = current + dir;
if (SimManager.IsValidPosition(next) && !cameFrom.ContainsKey(next))
{
GridCell cell = SimManager.grid[next.x, next.z];
if (cell != null && !cell.IsWall)
{
cameFrom[next] = current;
queue.Enqueue(next);
}
}
}
}
if (found)
{
Vector3Int curr = target;
while (cameFrom[curr] != CurrentPosition && cameFrom.ContainsKey(curr)) curr = cameFrom[curr];
return curr; 
}
return CurrentPosition;
}
private void RandomWalk()
{
Vector3Int[] directions = { Vector3Int.right, Vector3Int.left, Vector3Int.forward, Vector3Int.back };
for (int i = 0; i < 4; i++)
{
int randIndex = Random.Range(0, directions.Length);
Vector3Int temp = directions[i];
directions[i] = directions[randIndex];
directions[randIndex] = temp;
}
foreach (var dir in directions) if (TryMovePassive(CurrentPosition + dir)) return;
}
private bool TryMoveSmartYield(Vector3Int newPos)
{
if (SimManager.IsValidPosition(newPos))
{
GridCell nextCell = SimManager.grid[newPos.x, newPos.z];
if (nextCell != null && !nextCell.IsWall)
{
if (!nextCell.IsOccupied())
{
PerformTheMove(nextCell, newPos);
return true;
}
else if (nextCell.currentAgent != null && nextCell.currentAgent != this)
{
Vector3Int[] directions = { Vector3Int.right, Vector3Int.left, Vector3Int.forward, Vector3Int.back };
for (int i = 0; i < 4; i++)
{
int randIndex = Random.Range(0, directions.Length);
Vector3Int temp = directions[i];
directions[i] = directions[randIndex];
directions[randIndex] = temp;
}
foreach (var dir in directions)
{
Vector3Int neighborPos = CurrentPosition + dir;
if (SimManager.IsValidPosition(neighborPos))
{
GridCell neighborCell = SimManager.grid[neighborPos.x, neighborPos.z];
if (neighborCell != null && !neighborCell.IsWall && !neighborCell.IsOccupied())
{
PerformTheMove(neighborCell, neighborPos);
return true;
}
}
}
desiredCollisionStep = newPos;
waitAfterCollisionTimer = 0.5f;
return false;
}
}
}
return false;
}
private bool TryMovePassive(Vector3Int newPos)
{
if (SimManager.IsValidPosition(newPos))
{
GridCell nextCell = SimManager.grid[newPos.x, newPos.z];
if (nextCell != null && !nextCell.IsWall && !nextCell.IsOccupied())
{
PerformTheMove(nextCell, newPos);
return true;
}
}
return false;
}
private void PerformTheMove(GridCell nextCell, Vector3Int newPos)
{
SimManager.grid[CurrentPosition.x, CurrentPosition.z].currentAgent = null;
CurrentPosition = newPos; 
nextCell.currentAgent = this;
Vector3 moveDirection = nextCell.WorldPosition - transform.position;
moveDirection.y = 0f;
if (moveDirection != Vector3.zero) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), Time.deltaTime * visualTurnSpeed);
targetVisualPosition = nextCell.WorldPosition + visualOffset;
MoveCount++;
waitAfterCollisionTimer = 0f;
desiredCollisionStep = null;
}
}