using Sandbox;
[GameResource("Interactable Definition", "interact", "Something that can be interacted with.", Category = "Interactables", Icon ="sports_handball")]
public partial class InteractableResource : GameResource
{
	public string Name { get; set; }

	public string Description { get; set; }

	public bool CanPickup { get; set; }


}