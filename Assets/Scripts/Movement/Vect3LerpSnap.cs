using UnityEngine;

namespace BoardGame.Script.MovementFunctions
{
    public class Vect3LerpSnap
    {
        public static Vector3 SnapLerp(Vector3 fromVector, Vector3 toVector, float lerpForce, float minMagnitudeSnap)
        {
            if ((toVector - fromVector).magnitude < minMagnitudeSnap)
            {
                return toVector;
            }
            else
            {
                return Vector3.Lerp(fromVector, toVector, lerpForce * Time.deltaTime);
            }
        }
    }
}