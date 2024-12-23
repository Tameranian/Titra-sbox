using Sandbox;
using System;
using System.Collections.Generic;

public struct SplashEntry
{
	public SplashEntry()
	{
	}

	[Property]
	public string TexturePath { get; set; }
	[Property]
	public float duration { get; set; } = 3f;
	[Property]
	public float scaleDuration { get; set; } = 8;
	[Property]
	public float opacityDuration { get; set; } = 3;
	[Property]
	public Curve ScaleCurve { get; set; } = new Curve( new List<Curve.Frame>()
	{
		new(0,0,-1.8803414f,1.8803414f),
		new(1,1,0,0),
	} ) { ValueRange = new(1,1.5f) };
	[Property] public Curve OpacityCurve { get; set; } = new Curve( new List<Curve.Frame>()
	{
		new(0,0,0,0),
		new(0.5f,0.5f,0,0),
		new(1,0,0,0),
	} );
	[Property] public float WaitTimeStart { get; set; } = 0.2f;
	[Property] public float WaitTimeEnd { get; set; } = 0.2f;
}

/// <summary>
/// This component allows you to add unity style splash screens to your game
/// </summary>
[Title( "Simple splash screens - Main Component" )]
public partial class SimpleSplashScreen : PanelComponent
{
	[Property]
	Action OnFinishSplashScreen {  get; set; }

	[Property]
	public List<SplashEntry> SplashEntries {  get; set; }

	public Texture CurrentTexture;

	[Property]
	bool StartAutomatically {  get; set; }

	float Opacity = 1;
	float Scale = 1;
	float CurrentTime = 0;
	int CurrentIndex = 0;
	SplashEntry CurrentEntry;
	bool InSequence = false;

	protected override void OnStart()
	{
		if(StartAutomatically)
		{
			StartSequence();
		}
	}

	protected override void OnUpdate()
	{
		if(InSequence)
		{
			CurrentTime += Time.Delta;
			if ( CurrentTime >= 0 && CurrentTime <= CurrentEntry.duration )
			{
				Opacity = CurrentEntry.OpacityCurve.Evaluate( CurrentTime / CurrentEntry.opacityDuration );
				Scale = CurrentEntry.ScaleCurve.Evaluate( CurrentTime / CurrentEntry.scaleDuration );
			}
			if(CurrentTime > CurrentEntry.duration + CurrentEntry.WaitTimeEnd)
			{
				if ( CurrentIndex == SplashEntries.Count - 1 )
				{
					InSequence = false;
					if ( OnFinishSplashScreen != null )
					{
						OnFinishSplashScreen();
					}
				}
				else
				{
					NextImage();
				}
			}
		}
		base.OnUpdate();
	}

	void NextImage()
	{
		CurrentIndex++;
		CurrentEntry = SplashEntries[CurrentIndex];
		CurrentTime = -CurrentEntry.WaitTimeStart;
		Scale = 0;
		Opacity = 0;
	}

	public void StartSequence()
	{
		CurrentIndex = -1;
		if(SplashEntries.Count > 0)
		{
			InSequence = true;
		} else
		{
			Log.Warning( "Attempted to start splash sequence with no entries!" );
		}
	}

	protected override int BuildHash()
	{
		return base.BuildHash() + CurrentIndex.GetHashCode() + InSequence.GetHashCode() + CurrentTime.GetHashCode();
	}
}
