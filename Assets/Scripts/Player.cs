using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour {
  private const float ACCELERATION_TIME_AIRBORNE = 0.1f;
  private const float ACCELERATION_TIME_GROUNDED = 0.02f;
  private const float MOVE_SPEED = 6;

  public float jumpHeight = 4;
  public float timeToJumpApex = 0.4f;

  private float gravity;
  private float jumpVelocity;
  private float velocityXSmoothing;
  private Vector3 velocity;
  private Controller2D controller;

  private void Start() {
    controller = GetComponent<Controller2D>();

    gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
    jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
    print("Gavity: " + gravity + " Jump Velocity: " + jumpVelocity);
  }

  private void Update() {
    if (controller.collisions.above || controller.collisions.below) {
      velocity.y = 0;
    }

    Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

    if (Input.GetKeyDown(KeyCode.Space) && controller.collisions.below) {
      velocity.y = jumpVelocity;
    }

    float targetVelocityX = input.x * MOVE_SPEED;
    float smoothTime = controller.collisions.below ? ACCELERATION_TIME_GROUNDED : ACCELERATION_TIME_AIRBORNE;
    velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, smoothTime);
    velocity.y += gravity * Time.deltaTime;
    controller.Move(velocity * Time.deltaTime);
  }
}
