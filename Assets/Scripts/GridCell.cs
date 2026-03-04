using System.Collections.Generic;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    // Atributos del UML
    public Vector3Int LogicalPosition;
    public Vector3 WorldPosition;
    public bool IsWall = false;
    
    [Header("Ajuste visual")]
    public float boxHeight = 0.75f; 

    private Stack<Box> boxStack = new Stack<Box>();
    public Agent currentAgent; 
    public bool AddBox3D(Box newBox)
    {
        if (boxStack.Count < 5)
        {
            boxStack.Push(newBox);
            float boxHeightOffset = (boxStack.Count - 1) * boxHeight; 
            
            if (newBox != null)
            {
                newBox.UpdatePhysicalPosition(WorldPosition + new Vector3(0, boxHeightOffset, 0));
            }
            
            return true;
        }
        return false; 
    }
    public Box RemoveTopBox3D()
    {
        if (boxStack.Count > 0)
        {
            return boxStack.Pop();
        }
        return null; 
    }
    public float GetStackHeight()
    {
        return boxStack.Count;
    }
    public bool IsOccupied()
    {
        return currentAgent != null;
    }
}