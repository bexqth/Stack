using Godot;
using System;

public partial class Stack : Node3D
{
	private  float speed = 1.5f;
	public bool canMove{get;set;}
	public string direction{get;set;}
	public Color color{get;set;}
	public CsgBox3D box;
	public bool inUse{get;set;}
	
	
	[Signal]
	public delegate void StackStoppedEventHandler(Stack stack);


	[Signal]
	public delegate void StackMissedEventHandler(Stack stack);

	public override void _Ready()
	{	
		this.box = GetNode<CsgBox3D>("CSGBox3D");
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
			} else {
				//GD.Print("Material is not a StandardMaterial3D");
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
				position.Z -= speed * (float)delta; 
			}
			else if (this.direction == "right")
			{
				position.X -= speed * (float)delta;
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

	public Vector3 CutEdges(Stack stackUp) {
		// Calculate the overlap on the X axis
		float belowLeftX = stackUp.Position.X - stackUp.box.Size.X / 2;
		float belowRightX = stackUp.Position.X + stackUp.box.Size.X / 2;
		float currentLeftX = Position.X - box.Size.X / 2;
		float currentRightX = Position.X + box.Size.X / 2;

		float overlapLeftX = Math.Max(belowLeftX, currentLeftX);
		float overlapRightX = Math.Min(belowRightX, currentRightX);
		float overlapWidthX = overlapRightX - overlapLeftX;

		// Calculate the overlap on the Z axis
		float belowLeftZ = stackUp.Position.Z - stackUp.box.Size.Z / 2;
		float belowRightZ = stackUp.Position.Z + stackUp.box.Size.Z / 2;
		float currentLeftZ = Position.Z - box.Size.Z / 2;
		float currentRightZ = Position.Z + box.Size.Z / 2;

		float overlapLeftZ = Math.Max(belowLeftZ, currentLeftZ);
		float overlapRightZ = Math.Min(belowRightZ, currentRightZ);
		float overlapWidthZ = overlapRightZ - overlapLeftZ;

		if (overlapWidthX > 0 && overlapWidthZ > 0)
		{
			// Adjust the size and position of the box to keep only the overlapping part
			box.Size = new Vector3(overlapWidthX, box.Size.Y, overlapWidthZ);
			Position = new Vector3((overlapLeftX + overlapRightX) / 2, Position.Y, (overlapLeftZ + overlapRightZ) / 2);
			return this.box.Size;
		} else {
			// If there's no overlap, the block falls off
			GD.Print("No overlap");
			EmitSignal(nameof(Stack.StackMissed), this);
		}
		return Vector3.Zero;
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


