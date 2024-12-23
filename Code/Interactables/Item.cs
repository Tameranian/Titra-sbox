using System;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.Services;

public abstract class Item : Interactable
{
	[Property] public SoundEvent useSound;
	[Property] public SoundEvent swingSound;
	protected Player player;
	private PlayerController playerController;
	public bool canGrab;
	public bool canSwing;
	public bool isGrabbed;

	protected override void OnStart()
	{
		base.OnStart();
		if(interactableResource != null )
		{

			canGrab = interactableResource.CanGrab;
			canTakeDamage = interactableResource.CanBreak;
			currentHealth = interactableResource.BaseHealth;
		}
		else
		{
			Log.Warning("Interactable resource not set for " + GameObject.Name);
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if(isGrabbed)
		{	
			AnimateViewBobAndSway();
			if(InputActions.IsWalking)
			{
				if(InputActions.TryLeftClick)
				{
					ThrowItem();
				}

				if(InputActions.TryReload)
				{
					DropItem();
				}
			}
			if(!InputActions.IsWalking && InputActions.TryLeftClick)
			{
				ClickAction();
			}
		}
	}

	public virtual void ClickAction()
	{
		if(canSwing)
		{
			Log.Info("Item Swung");
		}

	}

	private void DropItem()
	{
		player.GameObject.Components.Get<Interaction>().grabbingItem = false;
		playerController = playerInteracting.GameObject.Components.Get<PlayerController>();
		var finalReachLength = playerController.EyeTransform.Position + playerController.EyeTransform.Forward *
			(playerController.ReachLength - 30);
		var holdTrace = Scene.Trace.Ray(playerController.EyeTransform.Position + playerController.EyeTransform.Forward,
			finalReachLength)
			.WithoutTags("trigger", "player", "placed")
			.IgnoreGameObject(GameObject)
			.Run();

		var traceTargetPosition = holdTrace.Hit ? holdTrace.HitPosition + holdTrace.Normal : finalReachLength;
		
		isGrabbed = false;
		modelRenderer.RenderOptions.Overlay = false;
		GameObject.SetParent(null);
		GameObject.LocalScale = savedScale;
		rigidbody.Gravity = true;
		rigidbody.Enabled = true;
		collider.Enabled = true;
		WorldPosition = traceTargetPosition;
		WorldRotation = Rotation.Identity;
		timeSinceLastDrop = 0f;
	}

	private void ThrowItem()
	{
		player.GameObject.Components.Get<Interaction>().grabbingItem = false;
		isGrabbed = false;
		modelRenderer.RenderOptions.Overlay = false;
		GameObject.SetParent(null);
		GameObject.LocalScale = savedScale;
		rigidbody.Gravity = true;
		rigidbody.Enabled = true;
		collider.Enabled = true;
		WorldPosition = Scene.Camera.WorldPosition + Scene.Camera.WorldRotation.Forward * 20f;
		rigidbody.Velocity =  Scene.Camera.WorldRotation.Forward * 1000f;
		timeSinceLastDrop = 0f;
	}

	private Vector3 previousPositionOffset = Vector3.Zero;

	private void AnimateViewBobAndSway()
	{
		float bobAmount = 0.25f;
		
		float bobOffset = MathF.Sin(Time.Now * playerController.WishVelocity.Length.Clamp(0f, 72 * 0.5f) / 2) * bobAmount;

		Vector3 targetPositionOffset = Scene.Camera.WorldRotation.Forward * 20f 
			+ Scene.Camera.WorldRotation.Down * 10f 
			+ Scene.Camera.WorldRotation.Right * 10f
			+ Vector3.Up * bobOffset;

		// Sway calculation
		Vector3 swayAmount = new Vector3(
			MathF.Sin(Time.Now * 2f) * 0.1f,
			MathF.Sin(Time.Now * 1.5f) * 0.1f,
			0f
		);

		targetPositionOffset += swayAmount;

		// Lerp the position offset
		previousPositionOffset = Vector3.Lerp(previousPositionOffset, targetPositionOffset, Time.Delta * 25f);

		WorldPosition = Scene.Camera.WorldPosition + previousPositionOffset;
		WorldRotation = Scene.Camera.WorldRotation;
	}


	public bool Grab(GameObject playerObject)
	{		// Add cooldown check
		// if (timeSinceLastDrop < 0.05f)  // Half second cooldown
		// {
		// 	return false;
		// }
		isGrabbed = true;

		playerInteracting = playerObject.Components.Get<Interaction>();
		playerController = playerObject.Components.Get<PlayerController>();
		player = playerObject.Components.Get<Player>();

		player.grabbingItem = true;
		isGrabbed = true;
		Log.Info("Item has been grabbed");

		
		modelRenderer.RenderOptions.Overlay = true;


		GameObject.SetParent(playerInteracting.GameObject);
		GameObject.LocalScale = .65f;
		WorldPosition = Scene.Camera.WorldPosition +
			Scene.Camera.WorldRotation.Forward * 20f 
			+ Scene.Camera.WorldRotation.Down * 10f 
			+ Scene.Camera.WorldRotation.Right * 10f;
		WorldRotation = Scene.Camera.WorldRotation;
		rigidbody.Gravity = false;
		rigidbody.Enabled = false;
		collider.Enabled = false;
		// Release(playerObject);
		return true;
	}

	public override void TakeDamage( int damage )
	{
		if ( canTakeDamage )
		{

			currentHealth -= damage;
			currentHealth = (int)MathF.Max( currentHealth, 0f );
			if ( currentHealth <= 0 )
			{
				Kill();
			}
			Log.Info( "Current Health is: " + currentHealth );
		}

	}

	public override void Kill()
	{
		if(canDie)
		{
			GameObject.Destroy();
		}
	}
}