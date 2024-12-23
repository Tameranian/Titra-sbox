using System;
using Sandbox;

public sealed class GenericBreakable : Interactable
{
	[Property]
	public int amountToDrop = 1;
	protected override void OnStart()
	{
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

		if(canDie)
		{
			for (int i = 0; i < amountToDrop; i++)
			{
				ResourceLibrary.TryGet( "prefabs/collectable_rock.prefab", out PrefabFile prefab );
				var random = new Random();
				float randomX = (float)(random.NextDouble() * 32 - 16); // Random number between -16 and 16
				float randomY = (float)(random.NextDouble() * 32 - 16); // Random number between -16 and 16
				SceneUtility.GetPrefabScene( prefab ).Clone(modelRenderer.Bounds.Center + new Vector3(randomX, randomY, 16 * (i + 1)), Rotation.Identity);
			}
			base.Kill();
		}
	}

}
