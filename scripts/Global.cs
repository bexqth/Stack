using Godot;
using System;

public partial class Global : Node
{


	// Called when the node enters the scene tree for the first time.

	[Export]
	public Label ScoreLabel;
	
	private SpawnPoint rayCast3DLeft;
	
	private SpawnPoint rayCast3DRight;
	private Camera3D camera;
	private float step = 10;
	private int spawnDirection = 0;
	private Stack newStack;
	[Export]
	public Stack previousStack;
	private bool gameOver;
	private Color color;
	private int score;
	public override void _Ready()
	{
		this.rayCast3DLeft = GetNode<SpawnPoint>("RayCast3D_left");
		this.rayCast3DRight = GetNode<SpawnPoint>("RayCast3D_right");
		this.camera = GetNode<Camera3D>("Camera3D");
		this.color = new Color("#599dea");
		this.gameOver = false;
		var firstSize = new Vector3(1f, 0.1f, 1f);

		SpawnNewStack(previousStack.box.Size);
		this.score = 0;
		displayScore(this.score);

	}

	public override void _Process(double delta)
	{
	}

	public void changeSpawnDirection() {
		if(spawnDirection == 1) {
			spawnDirection = 0;
		} else if(spawnDirection == 0) {
			spawnDirection = 1;
		}
	}

	public void AddScore() {
		this.score++;
	}

	public void displayScore(int score) {
		this.ScoreLabel.Text = score.ToString();
	}

	private void SpawnNewStack(Vector3 newSize)
	{
		if(!gameOver) {
			if (spawnDirection == 1) // RIGHT
			{	
				newStack = rayCast3DRight.GetNewStack(newSize);
				newStack.Connect(nameof(Stack.StackStopped), new Callable(this, nameof(onStackStopped)));
				newStack.Connect(nameof(Stack.StackMissed), new Callable(this, nameof(onStackMissed)));
				if(previousStack == null) {
					newStack.changeColor(this.color);
				} else {
					newStack.changeColor(previousStack.color);
				}
			    newStack.direction = "right";
				//GD.Print(newStack.direction);
				newStack.Position = rayCast3DRight.Position;
				newStack.canMove = true;
			}
			else if (spawnDirection == 0) // LEFT
			{
				newStack = rayCast3DRight.GetNewStack(newSize);
				newStack.Connect(nameof(Stack.StackStopped), new Callable(this, nameof(onStackStopped)));
				newStack.Connect(nameof(Stack.StackMissed), new Callable(this, nameof(onStackMissed)));
				if(previousStack == null) {
					newStack.changeColor(this.color);
				} else {
					newStack.changeColor(previousStack.color);
				}
				newStack.direction = "left";
				newStack.Position = rayCast3DLeft.Position;
				//GD.Print(newStack.direction);
				newStack.canMove = true;
				
			}
		}
	}

	public void onStackStopped(Stack stack) {
		var newSize = new Vector3(1f, 0.1f, 1f);
		//GD.Print("New block");
		newStack = null;
		changeSpawnDirection();
		this.AddScore();
		this.displayScore(this.score);
		//GD.Print(spawnDirection);
		//if (previousStack != null)
		//{
			newSize = stack.CutEdges(previousStack);
		//}
		previousStack = stack;
		movePositionUp();
		SpawnNewStack(newSize);
		
	}


	public void onStackMissed(Stack stack) {
		this.gameOver = true;
	}

	public void movePositionUp() {
		Vector3 newPositionLeft = rayCast3DLeft.Position;
		newPositionLeft.Y += 0.1f;
		rayCast3DLeft.Position = newPositionLeft;

		Vector3 newPositionRight = rayCast3DRight.Position;
		newPositionRight.Y += 0.1f;
		rayCast3DRight.Position = newPositionRight;

		Vector3 newPositionCamera = camera.Position;
		newPositionCamera.Y += 0.05f;
		camera.Position = newPositionCamera;
	}
}
