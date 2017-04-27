using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour {
  public float jumpHeight = 4;
  public float timeToJumpApex = 0.4f;

  float accelerationTimeAirborne = 0.1f;
  float accelerationTimeGrounded = 0.02f;
  float moveSpeed = 6;
  float gravity;
  float jumpVelocity;
  float velocityXSmoothing;
  Vector3 velocity;
  Controller2D controller;

  void Start() {
    controller = GetComponent<Controller2D>();

    gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
    jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
    print("Gavity: " + gravity + " Jump Velocity: " + jumpVelocity);
  }

  void Update() {
    if (controller.collisions.above || controller.collisions.below) {
      velocity.y = 0;
    }

    Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

    if (Input.GetKeyDown(KeyCode.Space) && controller.collisions.below) {
      velocity.y = jumpVelocity;
    }

    float targetVelocityX = input.x * moveSpeed;
    float smoothTime = controller.collisions.below ? accelerationTimeGrounded : accelerationTimeAirborne;
    velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, smoothTime);
    velocity.y += gravity * Time.deltaTime;
    controller.Move(velocity * Time.deltaTime);
  }
}
