using System;
using System.Collections.Generic;
using DStults.Utils;

namespace DazzleADV
{

	public class LocationConnector
	{
		public Location Parent { get; init; }
		public Location Destination { get; private set; }
		public LocationPlayerEvent OnPlayerEnter { get; private set; }
		public LocationPlayerEvent OnPlayerExit { get; private set; }
		public ConnectorSelfEvent OnTickEvent { get; private set; }

		public bool IsUnlocked { get; private set; }
		public bool IsVisible { get; private set; }

		public LocationConnector(Location parent, Location destination, bool visible = true, bool unlocked = true, LocationPlayerEvent enterEvent = null, LocationPlayerEvent exitEvent = null, ConnectorSelfEvent tickEvent = null)
		{
			if (parent == null)
				throw new ArgumentNullException("Error: LocationConnector constructor null parent location");
			if (destination == null)
				throw new ArgumentNullException("Error: LocationConnector constructor null destination location");
			
			if (enterEvent == null)
				enterEvent = new LocationPlayerEvent(null, LocationEvents.DoNothing);
			if (exitEvent == null)
				exitEvent = new LocationPlayerEvent(null, LocationEvents.DoNothing);
			if (tickEvent == null)
				tickEvent = new ConnectorSelfEvent(ConnectorEventTemplates.DoNothing);
			
			this.Parent = parent;
			this.Destination = destination;
			this.IsVisible = visible;
			this.IsUnlocked = unlocked;
			this.OnPlayerEnter = enterEvent;
			this.OnPlayerExit = exitEvent;
			this.OnTickEvent = tickEvent;
		}

		public void ToggleVisibility()
		{
			if (IsVisible)
				MakeInvisible();
			else
				MakeVisible();
		}

		public void MakeVisible()
		{
			GameEngine.SayToLocation(Parent, $"An entrance to {Destination.Name} has appeared.");
			IsVisible = true;
		}

		public void MakeInvisible()
		{
			GameEngine.SayToLocation(Parent, $"The entrance to {Destination.Name} has disappeared.");
			IsVisible = false;
		}

		public void ToggleLockState()
		{
			if (IsUnlocked)
				Lock();
			else
				Unlock();
		}

		public void Unlock()
		{
			GameEngine.SayToLocation(Parent, $"The entrance to {Destination.Name} has unlocked.");
			IsUnlocked = true;
		}

		public void Lock()
		{
			GameEngine.SayToLocation(Parent, $"The entrance to {Destination.Name} has locked.");
			IsUnlocked = false;
		}

		public void RunTickEvent() => this.OnTickEvent.PerformAction(this);

		public void SetEntryEvent(LocationPlayerEvent locationEvent)
		{
			if (locationEvent == null)
				throw new ArgumentNullException($"Error: {Parent.Name}->Connector.AddEntryEvent null event");

			this.OnPlayerEnter = locationEvent;
		}

		public void ClearTickEvent() => OnTickEvent = null;

		public void SetExitEvent(LocationPlayerEvent locationEvent)
		{
			if (locationEvent == null)
				throw new ArgumentNullException($"Error: {Parent.Name}->Connector.AddExitEvent null event");

			this.OnPlayerExit = locationEvent;
		}

		public void SetTickEvent(ConnectorSelfEvent tickEvent)
		{
			if (tickEvent == null)
				throw new ArgumentNullException($"Error: {Parent.Name}->Connector.AddTickEvent null event");

			this.OnTickEvent = tickEvent;
		}

	}
	
}