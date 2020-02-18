using UnityEngine;

public static class SplineInterpolation
{
    public static Vector3 Extrapolate(Vector3 A, Vector3 B)
    {
        Vector3 v1 = A - B;
        return A + v1;
    }

    public static Vector3 Interpolate(Vector3 previous, Vector3 start, Vector3 end, Vector3 next, float t, float tension = 1f)
    {
        float tSquared = t * t;
        float tCubed = tSquared * t;

        Vector3 part1 = previous * (-0.5f * tension * tCubed + tension * tSquared - 0.5f * tension * t);
        Vector3 part2 = start * (1f + 0.5f * tSquared * (tension - 6f) + 0.5f * tCubed * (4f - tension));
        Vector3 part3 = end * (0.5f * tCubed * (tension - 4f) + 0.5f * tension * t - (tension - 3f) * tSquared);
        Vector3 part4 = next * (-0.5f * tension * tSquared + 0.5f * tension * tCubed);

        return part1 + part2 + part3 + part4;
    }
}