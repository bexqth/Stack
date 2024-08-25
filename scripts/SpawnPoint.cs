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
		/*newStackScene = GD.Load<PackedScene>("res://scenes/stack.tscn");
		newStack = (Stack)newStackScene.Instantiate();
		//newStack.Position = this.Position; 
		//newStack.direction = newStackDirection;
		newStack.GetNode<CsgBox3D>("CSGBox3D").Size = newSize;
		//newStack.GetNode<CollisionShape3D>("CollisionShape3D").Shape. = newStack.GetNode<CsgBox3D>("CSGBox3D");
		//newStack.GetNode<RigidBody3D>("RigidBody3D").GetNode<CsgBox3D>("CSGBox3D").Size = newSize;
		newStack.inUse = true;
		GetParent().AddChild(newStack);
		return newStack;*/

		PackedScene newStackScene = GD.Load<PackedScene>("res://scenes/stack.tscn");
		Stack newStack = (Stack)newStackScene.Instantiate();
		
		// Update the size of the CsgBox3D
		CsgBox3D csgBox = newStack.GetNode<CsgBox3D>("CSGBox3D");
		csgBox.Size = newSize;
		
		// Update the size of the CollisionShape3D
		CollisionShape3D collisionShape = newStack.GetNode<CollisionShape3D>("CollisionShape3D");
		if (collisionShape.Shape is BoxShape3D boxShape) {
			boxShape.Size = newSize;
		}
		
		newStack.inUse = true;
		GetParent().AddChild(newStack);
		return newStack;	

	}
	
}
