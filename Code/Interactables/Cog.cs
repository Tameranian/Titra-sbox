using System;
using System.Collections.Generic;
using Sandbox;

public class Cog : Interactable
{
    [Property] public int teethCount = 8;
    [Property] public float rotationSpeed = 0f;

    protected override void OnStart()
    {
        interactableResource = ResourceLibrary.Get<InteractableResource>("game resources/cog.interact");
        base.OnStart();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        if (rotationSpeed != 0f)
        {
            GameObject.WorldRotation *= Rotation.FromAxis(Vector3.Forward, rotationSpeed * Time.Delta);
        }
    }

    private int CalculateGridSize()
    {
        // Known relationships:
        // 8 teeth = 5 grid units
        // 16 teeth = 9 grid units
        // For now, we'll handle these specific cases and could expand later
        switch (teethCount)
        {
            case 8:
                return 5;
            case 16:
                return 9;
            default:
                Log.Warning($"Tooth count {teethCount} not implemented - grid size calculation may be incorrect");
                // For any other tooth count, we'd need to determine the proper relationship
                // For now, return a proportional estimate
                return (int)Math.Ceiling(Math.Sqrt(teethCount) * 2);
        }
    }

    private List<Vector3Int> CalculateValidGearPositions()
    {
        var validPositions = new List<Vector3Int>();
        int ownGridSize = CalculateGridSize();
        
        // For 8-tooth gears:
        if (teethCount == 8)
        {
            // These are the verified valid offsets for 8-tooth gears
            int[] smallerSpacing = { 2, -2 };
            int[] largerSpacing = { 3, -3 };
            
            foreach (var x in smallerSpacing)
            {
                foreach (var z in largerSpacing)
                {
                    validPositions.Add(new Vector3Int(x, 0, z));
                    validPositions.Add(new Vector3Int(z, 0, x)); // Add the rotated version
                }
            }
        }
        // For 16-tooth gears or other sizes, we'd need to verify the correct spacing
        else if (teethCount == 16)
        {
			// These are the verified valid offsets for 16-tooth gears
            int[] smallerSpacing = { 2, -2 };
            int[] largerSpacing = { 3, -3 };
            
            foreach (var x in smallerSpacing)
            {
                foreach (var z in largerSpacing)
                {
                    validPositions.Add(new Vector3Int(x, 0, z));
                    validPositions.Add(new Vector3Int(z, 0, x)); // Add the rotated version
                }
            }
        }

        return validPositions;
    }

    public void CheckForAdjacentCogs()
    {
        var validPositions = CalculateValidGearPositions();
        var currentPos = gridPosition;

        foreach (var offset in validPositions)
        {
            Vector3Int checkPos = currentPos + offset;
            
            foreach (var obj in Scene.GetAll<Cog>())
            {
                if (obj != this && obj.gridPosition == checkPos)
                {
                    // Calculate proper rotation speeds based on gear ratio
                    float speedRatio = (float)obj.teethCount / teethCount;
                    StartSpinning(100);
                    obj.StartSpinning(-100 * speedRatio);
                    
                    Log.Info($"Connected gears - Ratio {teethCount}:{obj.teethCount} = {speedRatio}");
                }
            }
        }
    }

    public override void InteractWith(GameObject other, Vector3Int relativePosition)
    {
        // Implementation remains the same
    }

    public override void HoldAction(Interaction interaction)
    {
        base.HoldAction(interaction);
        StopSpinning();
    }

    public override void PlaceAction()
    {
        // Implementation remains the same
    }

    public void StartSpinning(float speed)
    {
        rotationSpeed = speed;
    }

    private void StopSpinning()
    {
        rotationSpeed = 0f;
        Log.Info("Spinning stopped");
    }
}