using System;
using Sandbox;

public class GridManager : Component
{
    public static GridManager Instance;
    [Property]
    public Curve scaleEffectCurve;

    public int gridWidth, gridHeight, gridDepth;
	[Property, Range(1, 64)] public float cellSize { get; set; } = 8.0f;

    protected override void OnAwake()
    {
        Instance = this;

    }

	public Vector3Int GetGridPosition(Vector3 worldPosition)
	{
        int x = (int)MathF.Round(worldPosition.x / cellSize);
        int y = (int)MathF.Round(worldPosition.y / cellSize);
        int z = (int)MathF.Round(worldPosition.z / cellSize);
		return new Vector3Int(x, y, z);
	}

    public Vector3 GetWorldPosition(Vector3Int gridPosition)
    {
        return new Vector3(gridPosition.x * cellSize, gridPosition.y * cellSize, gridPosition.z * cellSize);
    }

    public GameObject GetObjectsAtGridPosition(Vector3Int gridPosition)
    {
        Vector3 worldPosition = GetWorldPosition(gridPosition);
        var objects = Scene.FindInPhysics(new Sphere(worldPosition, cellSize / 2)).Where(x => x.Tags.Contains("placed"));
        foreach (GameObject interactable in objects)
        {
            if (interactable.WorldPosition == worldPosition)
            {
                Log.Info("Found object at grid position: " + gridPosition);
                return interactable;
            }
        }
        return null;
    }

}
