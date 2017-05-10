using UnityEngine;

public class Controller2D : RaycastController {
  private const float MAX_CLIMB_ANGLE = 80f;
  private const float MAX_DESCEND_ANGLE = 80f;

  public CollisionInfo collisions;

  public void Move(Vector3 velocity, bool standingOnPlatform = false) {
    UpdateRaycastOrigins();
    collisions.Reset();
    collisions.velocityOld = velocity;

    if (velocity.y < 0) DescendSlope(ref velocity);
    if (velocity.x != 0) HorizontalCollisions(ref velocity);
    if (velocity.y != 0) VerticalCollisions(ref velocity);

    transform.Translate(velocity);

    if (standingOnPlatform) collisions.below = true;
  }

  private void HorizontalCollisions(ref Vector3 velocity) {
    float directionX = Mathf.Sign(velocity.x);
    float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;

    for (int i = 0; i < horizontalRayCount; i++) {
      Vector2 rayOrigin = directionX == -1 ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
      rayOrigin += Vector2.up * horizontalRaySpacing * i;
      RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

      Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

      if (!hit || hit.distance == 0) continue;

      float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

      if (i == 0 && slopeAngle <= MAX_CLIMB_ANGLE) {
        if (collisions.descendingSlope) {
          collisions.descendingSlope = false;
          velocity = collisions.velocityOld;
        }

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
      Vector2 rayOrigin = directionY == -1 ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
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

    if (collisions.climbingSlope) {
      float directionX = Mathf.Sign(velocity.x);
      rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;
      Vector2 bottom = directionX == -1 ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
      Vector2 rayOrigin = bottom + Vector2.up * velocity.y;
      RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

      if (hit) {
        float angle = Vector2.Angle(hit.normal, Vector2.up);

        if (angle != collisions.slopeAngle) {
          velocity.x = (hit.distance - SKIN_WIDTH) * directionX;
          collisions.slopeAngle = angle;
        }
      }
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

  private void DescendSlope(ref Vector3 velocity) {
    float directionX = Mathf.Sign(velocity.x);
    Vector2 rayOrigin = directionX == -1 ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
    RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

    if (hit) {
      float angle = Vector2.Angle(hit.normal, Vector2.up);

      if (angle != 0 && angle <= MAX_DESCEND_ANGLE) {
        if (Mathf.Sign(hit.normal.x) == directionX) {
          if (hit.distance - SKIN_WIDTH <= Mathf.Tan(angle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x)) {
            float moveDistance = Mathf.Abs(velocity.x);
            float descendVelocity = Mathf.Sin(angle * Mathf.Deg2Rad) * moveDistance;
            velocity.x = Mathf.Cos(angle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            velocity.y -= descendVelocity;

            collisions.slopeAngle = angle;
            collisions.descendingSlope = true;
            collisions.below = true;
          }
        }
      }
    }
  }

  public struct CollisionInfo {
    public bool above;
    public bool below;
    public bool left;
    public bool right;
    public bool climbingSlope;
    public bool descendingSlope;
    public float slopeAngle;
    public float slopeAngleOld;
    public Vector3 velocityOld;

    public void Reset() {
      above = below = left = right = climbingSlope = descendingSlope = false;
      slopeAngleOld = slopeAngle;
      slopeAngle = 0f;
    }
  }
}
