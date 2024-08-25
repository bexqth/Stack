using Godot;
using System;

public partial class Stack : Node3D
{
	private  float speed = 1.5f;
	public bool canMove{get;set;}
	public string direction{get;set;}
	public Color color{get;set;}
	public CsgBox3D box{get;set;}
	public bool inUse{get;set;}
	
	
	[Signal]
	public delegate void StackStoppedEventHandler(Stack stack);


	[Signal]
	public delegate void StackMissedEventHandler(Stack stack);

	public override void _Ready()
	{	
		this.box = GetNode<CsgBox3D>("CSGBox3D");
		this.color = new Color("#599dea");
	}

	public override void _Process(double delta)
	{
		if(this.direction != null) {
			this.Move(delta);
			if(inUse) {
				if(Input.IsActionJustPressed("left_click")) 
				{
					Stop();
				}
			}
		}
		
	}

	public void changeColor(Color lastColor) {
		if (inUse) {
			float h, s, v;
			lastColor.ToHsv(out h, out s, out v);

			// Increment the hue value to create a spectrum of colors
			h += 0.05f; // Adjust this value to control the speed of color change
			if (h > 1.0f) {
				h = 0.0f;
			}

			this.color = Color.FromHsv(h, s, v);

			// Ensure the box has a unique material before changing its color
			if (this.box.Material != null) {
				this.box.Material = this.box.Material.Duplicate() as Material;
			}

			if (this.box.Material is StandardMaterial3D material) {
				material.AlbedoColor = this.color;
				// Debug print to confirm the color change
				//GD.Print("Material color changed to: ", this.color);
			} 
    	}
	
	}

	public void Move(double delta){
		 if (canMove)
		{
			Vector3 position = Position;
			if (this.direction == "left")
			{
				position.X += speed * (float)delta;
				//position.Z -= speed * (float)delta; 
			}
			else if (this.direction == "right")
			{
				//position.X -= speed * (float)delta;
				position.Z += speed * (float)delta;
			}
			Position = position;
		}
	}

	public void Stop()
	{
		canMove = false;
		inUse = false;
		EmitSignal(nameof(Stack.StackStopped), this);
		
	}

	public Vector3 CutEdges(Stack previousBlock) {
		// Calculate the overlap on the X axis
		float belowLeftX = previousBlock.Position.X - previousBlock.box.Size.X / 2;
		float belowRightX = previousBlock.Position.X + previousBlock.box.Size.X / 2;
		float currentLeftX = Position.X - box.Size.X / 2;
		float currentRightX = Position.X + box.Size.X / 2;

		float overlapLeftX = Math.Max(belowLeftX, currentLeftX);
		float overlapRightX = Math.Min(belowRightX, currentRightX);
		float overlapWidthX = overlapRightX - overlapLeftX;

		// Calculate the overlap on the Z axis
		float belowLeftZ = previousBlock.Position.Z - previousBlock.box.Size.Z / 2;
		float belowRightZ = previousBlock.Position.Z + previousBlock.box.Size.Z / 2;
		float currentLeftZ = Position.Z - box.Size.Z / 2;
		float currentRightZ = Position.Z + box.Size.Z / 2;

		float overlapLeftZ = Math.Max(belowLeftZ, currentLeftZ);
		float overlapRightZ = Math.Min(belowRightZ, currentRightZ);
		float overlapWidthZ = overlapRightZ - overlapLeftZ;

		if (overlapWidthX > 0 && overlapWidthZ > 0) {
			// Adjust the size and position of the box to keep only the overlapping part
			//box.Size = new Vector3(overlapWidthX, box.Size.Y, overlapWidthZ);
			//Position = new Vector3((overlapLeftX + overlapRightX) / 2, Position.Y, (overlapLeftZ + overlapRightZ) / 2);

			box.Size = new Vector3(overlapWidthX, box.Size.Y, overlapWidthZ);
			Position = new Vector3((overlapLeftX + overlapRightX) / 2, Position.Y, (overlapLeftZ + overlapRightZ) / 2);

			CollisionShape3D collisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
			if (collisionShape.Shape is BoxShape3D boxShape) {
				boxShape.Size = new Vector3(overlapWidthX, boxShape.Size.Y, overlapWidthZ);
			}

			collisionShape.Position = new Vector3((overlapLeftX + overlapRightX) / 2, collisionShape.Position.Y, (overlapLeftZ + overlapRightZ) / 2);

			// Create the falling blocks from the non-overlapping parts
			CreateFallingBlocks(previousBlock, overlapLeftX, overlapRightX, overlapLeftZ, overlapRightZ);

			return this.box.Size;
		} else {
			// If there's no overlap, the block falls off
			GD.Print("No overlap");
			EmitSignal(nameof(Stack.StackMissed), this);
		}
		return Vector3.Zero;
	}

	private void CreateFallingBlocks(Stack previousBlock, float overlapLeftX, float overlapRightX, float overlapLeftZ, float overlapRightZ) {
		// Calculate the non-overlapping parts
		float leftWidth = overlapLeftX - (previousBlock.Position.X - previousBlock.box.Size.X / 2);
		float rightWidth = (previousBlock.Position.X + previousBlock.box.Size.X / 2) - overlapRightX;
		float frontWidth = overlapLeftZ - (previousBlock.Position.Z - previousBlock.box.Size.Z / 2);
		float backWidth = (previousBlock.Position.Z + previousBlock.box.Size.Z / 2) - overlapRightZ;

		Material originalMaterial = box.Material;

		// Create and position the falling blocks on the opposite side
		if (leftWidth > 0) {
			Vector3 rightPosition = new Vector3(Position.X + box.Size.X / 2 + leftWidth / 2, Position.Y, Position.Z);
			RigidBody3D rightBlock = CreateBlock(new Vector3(leftWidth, box.Size.Y, box.Size.Z), rightPosition, originalMaterial);
			rightBlock.ApplyCentralForce(new Vector3(150, 0, 0)); // Apply strong force to the right and downward
		}
		if (rightWidth > 0) {
			Vector3 leftPosition = new Vector3(Position.X - box.Size.X / 2 - rightWidth / 2, Position.Y, Position.Z);
			RigidBody3D leftBlock = CreateBlock(new Vector3(rightWidth, box.Size.Y, box.Size.Z), leftPosition, originalMaterial);
			leftBlock.ApplyCentralForce(new Vector3(-150, 0, 0)); // Apply strong force to the left and downward
		}
		if (frontWidth > 0) {
			Vector3 backPosition = new Vector3(Position.X, Position.Y, Position.Z + box.Size.Z / 2 + frontWidth / 2);
			RigidBody3D backBlock = CreateBlock(new Vector3(box.Size.X, box.Size.Y, frontWidth), backPosition, originalMaterial);
			backBlock.ApplyCentralForce(new Vector3(0, 0, 150)); // Apply strong force to the back and downward
		}
		if (backWidth > 0) {
			Vector3 frontPosition = new Vector3(Position.X, Position.Y, Position.Z - box.Size.Z / 2 - backWidth / 2);
			RigidBody3D frontBlock = CreateBlock(new Vector3(box.Size.X, box.Size.Y, backWidth), frontPosition, originalMaterial);
			frontBlock.ApplyCentralForce(new Vector3(0, 0, -150)); // Apply strong force to the front and downward
		}
	}

	private RigidBody3D CreateBlock(Vector3 size, Vector3 position, Material material) {

		RigidBody3D rigidBody = new RigidBody3D();
		rigidBody.GravityScale = 1.0f;
		rigidBody.Mass = 1;
		
		CsgBox3D blockBox = new CsgBox3D();
		blockBox.Size = size;
		blockBox.Position = position;
		blockBox.Material = material;

		CollisionShape3D collisionShape = new CollisionShape3D();
		BoxShape3D boxShape = new BoxShape3D();
		boxShape.Size = size;

		collisionShape.Shape = boxShape;
		collisionShape.Position = blockBox.Position;

		rigidBody.AddChild(blockBox);
		rigidBody.AddChild(collisionShape);

		GetTree().Root.AddChild(rigidBody);
		//GetParent().AddChild(rigidBody);
		return rigidBody;
	}


	private void _on_area_3d_body_entered(Node3D body)
	{
		// Replace with function body.
	}


	private void _on_area_3d_area_shape_entered(Rid area_rid, Area3D area, long area_shape_index, long local_shape_index)
	{
		/*Node parent = area.GetParent();
		if (parent is Stack stackUp)
		{
			//CutEdges(stackUp);
		}*/
	}
}


