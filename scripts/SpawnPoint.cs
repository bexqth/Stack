using Godot;
using System;

public partial class SpawnPoint : RayCast3D
{
	// Called when the node enters the scene tree for the first time.
	
	private Stack newStack;
	private PackedScene newStackScene;
	[Export]
	public string newStackDirection;
	private Color lastColor;
	public override void _Ready()
	{
		this.lastColor = new Color("#599dea");
	}

	public override void _Process(double delta)
	{
		
	}

	public Stack GetNewStack(Vector3 newSize) {
		newStackScene = GD.Load<PackedScene>("res://scenes/stack.tscn");
		newStack = (Stack)newStackScene.Instantiate();
		//newStack.Position = this.Position; 
		//newStack.direction = newStackDirection;
		newStack.GetNode<CsgBox3D>("CSGBox3D").Size = newSize;
		newStack.inUse = true;
		GetParent().AddChild(newStack);
		return newStack;
	}
	
}
