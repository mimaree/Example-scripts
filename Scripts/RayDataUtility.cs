using UnityEngine;

public static class RayDataUtility
{
    public static RaysSkillData TransformRayRelativeToWorld(this RaysSkillData rayData, Vector3 playerCellPos, Vector3 dir)
    {
        Quaternion _quaternion = Quaternion.LookRotation(dir);
        Vector3 _pos;
        Vector3 _dir;
        int _iteration = 0;

        foreach (CurveRay ray in CubicBezierSkill.Path)
        {
            _pos = _quaternion * ray.Position + playerCellPos;
            _dir = _quaternion * ray.Direction;
            rayData.PathList[_iteration++] = new CurveRay(_pos, _dir);
        }

        rayData.ShapePoints.Rotate(CubicBezierSkill.Shape, _quaternion);

        _iteration = 0;
        foreach (Vector3 shape in CubicBezierSkill.Shape)
            rayData.ShapePoints[_iteration++] = _quaternion * shape;

        return rayData;
    }
}