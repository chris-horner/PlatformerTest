using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour {
  const float SKIN_WIDTH = 0.015f;

  public LayerMask collisionMask;
  public int horizontalRayCount = 4;
  public int verticalRayCount = 4;

  float horizontalRaySpacing;
  float verticalRaySpacing;
  BoxCollider2D collider;
  RaycastOrigins raycastOrigins;

  void Start() {
    collider = GetComponent<BoxCollider2D>();
    CalculateRaySpacing();
  }

  public void Move(Vector3 velocity) {
    UpdateRaycastOrigins();

    if (velocity.x != 0) HorizontalCollisions(ref velocity);
    if (velocity.y != 0) VerticalCollisions(ref velocity);

    transform.Translate(velocity);
  }

  void HorizontalCollisions(ref Vector3 velocity) {
    float directionX = Mathf.Sign(velocity.x);
    float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;

    for (int i = 0; i < horizontalRayCount; i++) {
      Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
      rayOrigin += Vector2.up * horizontalRaySpacing * i;
      RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

      Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

      if (hit) {
        velocity.x = (hit.distance - SKIN_WIDTH) * directionX;
        rayLength = hit.distance;
      }
    }
  }
  void VerticalCollisions(ref Vector3 velocity) {
    float directionY = Mathf.Sign(velocity.y);
    float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;

    for (int i = 0; i < verticalRayCount; i++) {
      Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
      rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
      RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

      Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

      if (hit) {
        velocity.y = (hit.distance - SKIN_WIDTH) * directionY;
        rayLength = hit.distance;
      }
    }
  }

  void UpdateRaycastOrigins() {
    Bounds bounds = collider.bounds;
    bounds.Expand(SKIN_WIDTH * -2);

    raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
    raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
    raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
    raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
  }

  void CalculateRaySpacing() {
    Bounds bounds = collider.bounds;
    bounds.Expand(SKIN_WIDTH * -2);

    horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
    verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

    horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
    verticalRaySpacing = bounds.size.y / (verticalRayCount - 1);
  }

  struct RaycastOrigins {
    public Vector2 topLeft;
    public Vector2 topRight;
    public Vector2 bottomLeft;
    public Vector2 bottomRight;
  }
}
