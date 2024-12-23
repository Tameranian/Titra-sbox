using System;
using System.Collections.Generic;
using Sandbox;

public class Food : Item
{
	[Property] public SoundEvent consumeSound;
	private int consumeUses;
	public bool canConsume;
    public float hungerRestoreValue = 0f;
    public float energyRestoreValue = 0f;
    public float healthRestoreValue = 0f;
    public float spoilTime = 0f;
    public bool canSpoil = false;

    protected override void OnStart()
    {
        base.OnStart();
        if (interactableResource != null)
        {
            hungerRestoreValue = interactableResource.HungerRestoreValue;
            energyRestoreValue = interactableResource.EnergyRestoreValue;
            healthRestoreValue = interactableResource.HealthRestoreValue;
            spoilTime = interactableResource.SpoilTime;
            canSpoil = interactableResource.CanSpoil;
            
			if(consumeSound == null && interactableResource.ConsumeSound != null)
			{
				consumeSound = interactableResource.ConsumeSound;
			}
            
			canConsume = interactableResource.CanConsume;
			consumeUses = interactableResource.ConsumeUses;
        }
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
		if(canConsume && player.currentHunger < 100f)
		{
			Sound.Play(consumeSound, player.GameObject.WorldPosition);
			consumeUses--;
			if(consumeUses <= 0)
			{
				player.grabbingItem = false;
				GameObject.Destroy();
			}
			RestoreStats(interactableResource.HungerRestoreValue, interactableResource.EnergyRestoreValue, interactableResource.HealthRestoreValue);
		}
    }

	private void RestoreStats(float hunger, float energy, float health)
	{
		player.currentHunger += hunger;
		player.currentEnergy += energy;
		player.currentHealth += health;
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