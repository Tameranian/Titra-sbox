using System;
using System.Collections.Generic;
using Sandbox;

public class Weapon : Item
{
    protected override void OnStart()
    {	
		if(swingSound == null && interactableResource.SwingSound != null)
		{
			swingSound = interactableResource.SwingSound;
		}
		else
		{
			swingSound = ResourceLibrary.Get<SoundEvent>("sounds/impacts/bullets/impact-bullet-generic.sound");
		}
        base.OnStart();
    }
	protected override void OnUpdate()
	{
        base.OnUpdate();
	}

    public void Consume()
    {
        Log.Info("Food has been consumed");
        Destroy();
    }

    public override void ClickAction()
    {
		Log.Info("Weapon has been swung");
		Sound.Play(swingSound, WorldPosition);
        PlaySwingAnimation();
    }

    private void PlaySwingAnimation()
    {
		
    }
    
	public override void InteractWith( GameObject other, Vector3Int relativePosition )
	{
		// throw new NotImplementedException();
	}

	public override void HoldAction( Interaction interaction )
	{
        base.HoldAction(interaction);
	}

	public override void PlaceAction()
	{
		// throw new NotImplementedException();
	}
}