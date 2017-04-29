using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour {
  private const float SKIN_WIDTH = 0.015f;
  private const float MAX_CLIMB_ANGLE = 80f;

  public LayerMask collisionMask;
  public int horizontalRayCount = 4;
  public int verticalRayCount = 4;
  public CollisionInfo collisions;

  private float horizontalRaySpacing;
  private float verticalRaySpacing;
  private new BoxCollider2D collider;
  private RaycastOrigins raycastOrigins;

  private void Start() {
    collider = GetComponent<BoxCollider2D>();
    CalculateRaySpacing();
  }

  public void Move(Vector3 velocity) {
    UpdateRaycastOrigins();
    collisions.Reset();

    if (velocity.x != 0) HorizontalCollisions(ref velocity);
    if (velocity.y != 0) VerticalCollisions(ref velocity);

    transform.Translate(velocity);
  }

  private void HorizontalCollisions(ref Vector3 velocity) {
    float directionX = Mathf.Sign(velocity.x);
    float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;

    for (int i = 0; i < horizontalRayCount; i++) {
      Vector2 rayOrigin = directionX == -1 ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
      rayOrigin += Vector2.up * horizontalRaySpacing * i;
      RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

      Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

      if (!hit) continue;

      float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

      if (i == 0 && slopeAngle <= MAX_CLIMB_ANGLE) {
        float distanceToSlopeStart = 0f;

        if (slopeAngle != collisions.slopeAngleOld) {
          distanceToSlopeStart = hit.distance - SKIN_WIDTH;
          velocity.x -= distanceToSlopeStart * directionX;
        }

        ClimbSlope(ref velocity, slopeAngle);
        velocity.x += distanceToSlopeStart * directionX;
      }

      if (collisions.climbingSlope && slopeAngle <= MAX_CLIMB_ANGLE) continue;

      velocity.x = (hit.distance - SKIN_WIDTH) * directionX;
      rayLength = hit.distance;

      if (collisions.climbingSlope) {
        velocity.y = Mathf.Tan(collisions.slopeAngle) * Mathf.Deg2Rad * Mathf.Abs(velocity.x);
      }

      collisions.left = directionX == -1;
      collisions.right = directionX == 1;
    }
  }

  private void VerticalCollisions(ref Vector3 velocity) {
    float directionY = Mathf.Sign(velocity.y);
    float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;

    for (int i = 0; i < verticalRayCount; i++) {
      Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
      rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
      RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

      Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

      if (!hit) continue;

      velocity.y = (hit.distance - SKIN_WIDTH) * directionY;
      rayLength = hit.distance;

      if (collisions.climbingSlope) {
        velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
      }

      collisions.below = directionY == -1;
      collisions.above = directionY == 1;
    }
  }

  private void ClimbSlope(ref Vector3 velocity, float angle) {
    float moveDistance = Mathf.Abs(velocity.x);
    float climbVelocityY = Mathf.Sin(angle * Mathf.Deg2Rad) * moveDistance;

    if (velocity.y > climbVelocityY) return;

    velocity.y = climbVelocityY;
    velocity.x = Mathf.Cos(angle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
    collisions.below = true;
    collisions.climbingSlope = true;
    collisions.slopeAngle = angle;
  }

  private void UpdateRaycastOrigins() {
    Bounds bounds = collider.bounds;
    bounds.Expand(SKIN_WIDTH * -2);

    raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
    raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
    raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
    raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
  }

  private void CalculateRaySpacing() {
    Bounds bounds = collider.bounds;
    bounds.Expand(SKIN_WIDTH * -2);

    horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
    verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

    horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
    verticalRaySpacing = bounds.size.y / (verticalRayCount - 1);
  }

  private struct RaycastOrigins {
    public Vector2 topLeft;
    public Vector2 topRight;
    public Vector2 bottomLeft;
    public Vector2 bottomRight;
  }

  public struct CollisionInfo {
    public bool above;
    public bool below;
    public bool left;
    public bool right;
    public bool climbingSlope;
    public float slopeAngle;
    public float slopeAngleOld;

    public void Reset() {
      above = below = left = right = climbingSlope = false;
      slopeAngleOld = slopeAngle;
      slopeAngle = 0f;
    }
  }
}
