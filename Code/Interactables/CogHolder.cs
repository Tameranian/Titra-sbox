using System;

public class CogHolder : Interactable
{

	protected override void OnStart()
    {
	  	interactableResource = ResourceLibrary.Get<InteractableResource>("game resources/cog_fixture.interact");
        base.OnStart();
    }
    protected override void OnUpdate()
    {
        base.OnUpdate();

    }

    public override void InteractWith(GameObject other, Vector3Int relativePosition)
    {
		// Log.Info("Cog interacted with: " + other.Name);

    }

    public override void HoldAction(Interaction interaction)
    {
		var overlappingInteractable = GetObjectsAWorldPosition(WorldPosition, GameObject);
		if(overlappingInteractable != null && overlappingInteractable.Components.Get<Cog>() != null)
		{
			overlappingInteractable.Components.Get<Cog>().Press(new IPressable.Event(interaction));
			Log.Info("CogHolder interacted with: " + overlappingInteractable.Name);
		}
		else
		{

      base.HoldAction(interaction);
		}

    }

    public override void PlaceAction()
    {
    }

	// public void CheckForOverlappingCog()
	// {
	// 	if (gridPosition != Vector3Int.Zero)
	// 	{
	// 		Log.Info(GetObjectsAWorldPosition(WorldPosition, GameObject));
	// 	}
	// }

    // public void StartSpinning(float speed)
    // {
    //     rotationSpeed = speed; 
    //     Log.Info("Spinning started with speed: " + rotationSpeed);
    // }

    // private void StopSpinning()
    // {
    //     rotationSpeed = 0f; 
    //     Log.Info("Spinning stopped");
    // }

	

}