using Sandbox;
[GameResource("Interactable Definition", "interact", "Something that can be interacted with.", Category = "Interactables", Icon ="sports_handball")]
public partial class InteractableResource : GameResource
{
	public string Name { get; set; }

	public int BaseHealth { get; set; }

	public SoundEvent PlaceSound { get; set; }

	public SoundEvent HurtSound { get; set; }
	
	public SoundEvent UseSound { get; set; }

	public SoundEvent ConsumeSound { get; set; }

	public SoundEvent SwingSound { get; set; }

	public string Description { get; set; }

	public string InteractText { get; set; }

	public bool CanPickup { get; set; }

	public bool CanHurt { get; set; }

	public bool CanDie { get; set; }
	
	public bool CanGrab { get; set; }

	public bool CanUse { get; set; }

	public bool CanConsume { get; set; }

	public bool CanEquip { get; set; }

	public bool CanDrop { get; set; }

	public bool CanBreak { get; set; }

	public bool CanSpoil { get; set; }

	public PrefabFile[] DroppableCollectable { get; set; }
	
	public float HungerRestoreValue { get; set; }

	public float EnergyRestoreValue { get; set; }

	public float HealthRestoreValue { get; set; }

	public int ConsumeUses { get; set; }

	public float SpoilTime { get; set; }

	public float Damage { get; set; }

	public float AttackSpeed { get; set; }

	public float Range { get; set; }

	public bool CanSwing { get; set; }

	public bool CanShoot { get; set; }

	public float MaxAmmo { get; set; }

	

}