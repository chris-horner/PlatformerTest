using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour {
  protected const float SKIN_WIDTH = 0.015f;

  protected float horizontalRaySpacing;
  protected float verticalRaySpacing;
  protected new BoxCollider2D collider;
  protected RaycastOrigins raycastOrigins;

  public LayerMask collisionMask;
  public int horizontalRayCount = 4;
  public int verticalRayCount = 4;

  protected virtual void Start() {
    collider = GetComponent<BoxCollider2D>();
    CalculateRaySpacing();
  }

  protected void UpdateRaycastOrigins() {
    Bounds bounds = collider.bounds;
    bounds.Expand(SKIN_WIDTH * -2);

    raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
    raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
    raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
    raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
  }

  protected void CalculateRaySpacing() {
    Bounds bounds = collider.bounds;
    bounds.Expand(SKIN_WIDTH * -2);

    horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
    verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

    horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
    verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
  }

  protected struct RaycastOrigins {
    public Vector2 topLeft;
    public Vector2 topRight;
    public Vector2 bottomLeft;
    public Vector2 bottomRight;
  }
}
