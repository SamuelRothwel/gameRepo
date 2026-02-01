using coolbeats.scripts.managerScripts;
using Godot;
using System;
using System.Collections.Generic;

public partial class unitAbilities : GridContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		mAccess.inputManager.unitSelected += new EventHandler(newUnit);
		Columns = 4;
	}
	void newUnit(object sender, EventArgs e)
	{
		foreach (Node child in GetChildren())
		{
			child.QueueFree();
		}
		Dictionary<Godot.Key, (string[], string)> commands = mAccess.unitManager.commandSets[mAccess.inputManager.selectedType].Item2;
		foreach (KeyValuePair<Godot.Key, (string[], string)> command in commands)
		{
			abilityButton newButton = new abilityButton(command.Key, command.Value.Item2);
			AddChild(newButton);
		}
	}
	internal partial class abilityButton : Button
	{
		Key key;
		public abilityButton(Key k, string name)
		{
			Text = name;
			key = k;
		}
        public override void _Pressed()
        {
            
        }
	} 
}
