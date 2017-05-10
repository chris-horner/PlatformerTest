using UnityEngine;
using System.Collections.Generic;

public class PlatformController : RaycastController {
  public LayerMask passengerMask;
  public Vector3 move;

  private List<PassengerMovement> passengerMovements;
  private Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();

  private void Update() {
    UpdateRaycastOrigins();
    Vector3 velocity = move * Time.deltaTime;
    CalculatePassengerMovement(velocity);

    MovePassengers(true);
    transform.Translate(velocity);
    MovePassengers(false);
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
