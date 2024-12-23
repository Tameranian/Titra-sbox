using System;
using System.Collections.Generic;
using Sandbox;

public class GenericPlaceable : Interactable
{
    protected override void OnStart()
    {
        base.OnStart();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
    }


    public override void InteractWith(GameObject other, Vector3Int relativePosition)
    {
        // Implementation remains the same
    }

    public override void HoldAction(Interaction interaction)
    {
        base.HoldAction(interaction);
    }

    public override void PlaceAction()
    {
        // Implementation remains the same
    }

}