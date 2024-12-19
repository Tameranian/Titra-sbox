using Sandbox;

public partial class Interactable : Component, Component.IPressable, Component.ITriggerListener
{
	public ModelRenderer modelRenderer;
	public Rigidbody rigidbody;
	public PhysicsBody physicsBody;
	public Collider collider;
	public bool isHeld = false;
	public bool isBeingPlaced = false;
	[Property]
	public float rotationSpeed = 0f;
	private Interaction playerInteracting;
	[Property] public SoundEvent placeSound = default;

	protected override void OnStart()
	{
		if(placeSound == null)
			placeSound = ResourceLibrary.Get<SoundEvent>("sounds/impacts/melee/impact-melee-concrete.sound");

		Log.Info(placeSound);
		modelRenderer = Components.Get<ModelRenderer>();
		collider = Components.Get<Collider>();
		rigidbody = Components.Get<Rigidbody>();
		physicsBody = rigidbody.PhysicsBody;
	}

	protected override void OnUpdate()
	{
		if (rotationSpeed != 0f)
		{
			GameObject.WorldRotation *= Rotation.FromAxis(Vector3.Forward, rotationSpeed * Time.Delta);
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		if (other.GameObject.Components.Get<Interactable>() != null && isHeld && playerInteracting?.snapEnabled == true)
		{
			Log.Info("Object entered trigger: " + other.GameObject.Name);
			playerInteracting.placingBlocked = true;
		}
	}

	public void OnTriggerExit(Collider other)
	{
		if (other.GameObject.Components.Get<Interactable>() != null && isHeld && playerInteracting?.snapEnabled == true)
		{
			Log.Info("Object exited trigger: " + other.GameObject.Name);
			playerInteracting.placingBlocked = IsAnyInteractableInside();
		}
	}

	private bool IsAnyInteractableInside()
	{
		foreach (var gameobject in collider.Touching)
		{
			if (gameobject.Components.Get<Interactable>() != null)
			{
				return true;
			}
		}
		return false;
	}

	public void Hover(IPressable.Event e)
	{
		// Logic for when the player starts looking at this object
	}

	public void Look(IPressable.Event e)
	{
		// Logic for when the player is still looking at this object
	}

	public void Blur(IPressable.Event e)
	{
		// Logic for when the player stops looking at this object
	}

	public bool Press(IPressable.Event e)
	{
		playerInteracting = e.Source.Components.Get<Interaction>();

		if (playerInteracting.IsPlacementAllowed)
		{
			Log.Info("Placement Disabled");
			return false;
		}
		else
		{
			Log.Info("Pressing Interactable");
			isHeld = true;
			playerInteracting.heldInteractable = this;
			ToggleRigidbodyLock(false);
			Tags.Add("Held");
			Release(e);
			return true;
		}
	}

	public void ToggleCollider(bool hasCollision)
	{
		GameObject.Components.Get<Collider>().IsTrigger = !hasCollision;
	}

	public void ToggleRigidbodyLock(bool locked)
	{
		if (Components.TryGet<Rigidbody>(out var rb))
		{
			var locking = rb.Locking;
			locking.X = locked;
			locking.Y = locked;
			locking.Z = locked;
			locking.Pitch = locked;
			locking.Yaw = locked;
			locking.Roll = locked;
			rb.Locking = locking;
		}
	}

	public bool Pressing(IPressable.Event e)
	{
		// Logic for when the object is still being pressed
		return true;
	}

	public void Release(IPressable.Event e)
	{
		Log.Info("Releasing Interactable");
		e.Source.Components.Get<Interaction>().IsPlacementAllowed = true;
		// Logic for when the press finishes
	}

	public bool CanPress(IPressable.Event e)
	{
		// Logic to determine if the object can be pressed
		return true;
	}
}