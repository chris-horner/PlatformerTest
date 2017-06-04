using UnityEngine;
using System.Collections.Generic;

public class PlatformController : RaycastController {
  public LayerMask passengerMask;
  public Vector3[] localWaypoints;
  public float speed;
  public bool cyclic;
  public float waitTime;
  [Range(0, 2)] public float easeAmount;

  private float nextMoveTime;
  private int fromWaypointIndex;
  private float percentBetweenWaypoints;
  private Vector3[] globalWaypoints;
  private List<PassengerMovement> passengerMovements;
  private readonly Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();

  protected override void Start() {
    base.Start();
    globalWaypoints = new Vector3[localWaypoints.Length];

    for (int i = 0; i < localWaypoints.Length; i++) {
      globalWaypoints[i] = localWaypoints[i] + transform.position;
    }
  }

  private void Update() {
    UpdateRaycastOrigins();
    Vector3 velocity = CalculatePlatformMovement();
    CalculatePassengerMovement(velocity);

    MovePassengers(true);
    transform.Translate(velocity);
    MovePassengers(false);
  }

  private float Ease(float x) {
    float a = easeAmount + 1;
    return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
  }

  private Vector3 CalculatePlatformMovement() {
    if (Time.time < nextMoveTime) return Vector3.zero;

    fromWaypointIndex %= globalWaypoints.Length;
    int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
    float distanceToNext = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
    percentBetweenWaypoints += Time.deltaTime * speed / distanceToNext;
    percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
    float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);

    Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex],
                                  globalWaypoints[toWaypointIndex],
                                  easedPercentBetweenWaypoints);

    if (percentBetweenWaypoints >= 1) {
      percentBetweenWaypoints = 0;
      fromWaypointIndex++;

      if (!cyclic) {
        if (fromWaypointIndex >= globalWaypoints.Length - 1) {
          fromWaypointIndex = 0;
          System.Array.Reverse(globalWaypoints);
        }
      }

      nextMoveTime = Time.time + waitTime;
    }

    return newPos - transform.position;
  }

  private void MovePassengers(bool beforeMovePlatform) {
    // BLEH! Sebastian!
    foreach (PassengerMovement passenger in passengerMovements) {
      if (!passengerDictionary.ContainsKey(passenger.transform)) {
        passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<Controller2D>());
      }

      if (passenger.moveBeforePlatform == beforeMovePlatform) {
        passengerDictionary[passenger.transform].Move(passenger.velocity, passenger.standingOnPlatform);
      }
    }
  }

  private void CalculatePassengerMovement(Vector3 velocity) {
    // Sebastian what on earth are you doing!?
    // Clean this junk up when tutorial is done.
    HashSet<Transform> movedPassengers = new HashSet<Transform>();
    passengerMovements = new List<PassengerMovement>();

    float directionX = Mathf.Sign(velocity.x);
    float directionY = Mathf.Sign(velocity.y);

    // Vertically moving platform.
    if (velocity.y != 0) {
      float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;

      for (int i = 0; i < verticalRayCount; i++) {
        Vector2 rayOrigin = directionY == -1 ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
        rayOrigin += Vector2.right * (verticalRaySpacing * i);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

        if (!hit || movedPassengers.Contains(hit.transform)) continue;

        float pushX = directionY == 1 ? velocity.x : 0;
        float pushY = velocity.y - hit.distance - SKIN_WIDTH * directionY;

        hit.transform.Translate(new Vector3(pushX, pushY));
        movedPassengers.Add(hit.transform);
        passengerMovements.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));
      }
    }

    // Horizontally moving platform.
    if (velocity.x != 0) {
      float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;

      for (int i = 0; i < horizontalRayCount; i++) {
        Vector2 rayOrigin = directionX == -1 ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
        rayOrigin += Vector2.up * horizontalRaySpacing * i;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);

        if (!hit || movedPassengers.Contains(hit.transform)) continue;

        float pushX = velocity.x - hit.distance - SKIN_WIDTH * directionX;
        float pushY = -SKIN_WIDTH;

        passengerMovements.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
        movedPassengers.Add(hit.transform);
      }
    }

    // Passenger on top of a horizontally or downward moving platform.
    if (directionY == -1 || velocity.y == 0 && velocity.x != 0) {
      float rayLength = SKIN_WIDTH * 2;

      for (int i = 0; i < verticalRayCount; i++) {
        Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

        if (!hit || movedPassengers.Contains(hit.transform)) continue;

        float pushX = velocity.x;
        float pushY = velocity.y;
        passengerMovements.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, true));
        movedPassengers.Add(hit.transform);
      }
    }
  }

  private void OnDrawGizmos() {
    if (localWaypoints == null) return;

    Gizmos.color = Color.red;
    const float size = 0.3f;

    for (int i = 0; i < localWaypoints.Length; i++) {
      Vector3 dynamicGlobalPosition = localWaypoints[i] + transform.position;
      Vector3 globalWaypointPosition = Application.isPlaying ? globalWaypoints[i] : dynamicGlobalPosition;
      Gizmos.DrawLine(globalWaypointPosition - Vector3.up * size, globalWaypointPosition + Vector3.up * size);
      Gizmos.DrawLine(globalWaypointPosition - Vector3.left * size, globalWaypointPosition + Vector3.left * size);
    }
  }

  struct PassengerMovement {
    public Transform transform;
    public Vector3 velocity;
    public bool standingOnPlatform;
    public bool moveBeforePlatform;

    public PassengerMovement(Transform transform, Vector3 velocity, bool standingOnPlatform, bool moveBeforePlatform) {
      this.transform = transform;
      this.velocity = velocity;
      this.standingOnPlatform = standingOnPlatform;
      this.moveBeforePlatform = moveBeforePlatform;
    }
  }
}
