using System;
using Sandbox;

public sealed class Tree : Interactable
{
	[Property]
	public bool choppedDown;
	public int amountToDrop = 1;
	protected override void OnStart()
	{
		interactableResource = ResourceLibrary.Get<InteractableResource>("game resources/tree.interact");
		placeSound = interactableResource.PlaceSound;
		base.OnStart();
	}
	
	protected override void OnUpdate()
	{

        base.OnUpdate();
	}

	public override void TakeDamage( int damage )
	{
		base.TakeDamage(damage);
	}

	public override void HoldAction( Interaction interaction )
	{
		throw new System.NotImplementedException();
	}

	public override void InteractWith( GameObject other, Vector3Int relativePosition )
	{
		throw new System.NotImplementedException();
	}

	public override void PlaceAction()
	{
		throw new System.NotImplementedException();
	}

	public override void Kill()
	{
		if(canDie && !choppedDown)
		{
			modelRenderer.SetBodyGroup("base", 1);
			choppedDown = true;
			currentHealth = 100;
			ResourceLibrary.TryGet( "prefabs/tree_cut.prefab", out PrefabFile prefab );
			GameObject log = SceneUtility.GetPrefabScene( prefab ).Clone(WorldPosition, WorldRotation);
			log.Components.Get<Tree>().amountToDrop = 4;
		}
		else if(canDie && choppedDown)
		{
			for (int i = 0; i < amountToDrop; i++)
			{
				ResourceLibrary.TryGet( "prefabs/collectable_log.prefab", out PrefabFile prefab );
				var random = new Random();
				float randomX = (float)(random.NextDouble() * 32 - 16); // Random number between -16 and 16
				float randomY = (float)(random.NextDouble() * 32 - 16); // Random number between -16 and 16
				SceneUtility.GetPrefabScene( prefab ).Clone(modelRenderer.Bounds.Center + new Vector3(randomX, randomY, 16 * (i + 1)), Rotation.Identity);
			}
			base.Kill();
		}
	}

}
