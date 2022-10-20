using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public static class JobUtility
{
    public static JobHandle GetRaycastCommandHandle(this RaysSkillData rayData, int parallelSize)
    {
        CalcRaycastCommand raycastCommands = new()
        {
            Result = rayData.CommandList,
            ShapePoints = rayData.ShapePoints,
            Path = rayData.PathList
        };

        JobHandle handle = raycastCommands.Schedule(rayData.ShapePoints.Length, parallelSize);
        return RaycastCommand.ScheduleBatch(rayData.CommandList, rayData.RayCastHitList, 20, handle);
    }

    public static JobHandle FiltrRaycastByHit(this JobHandle handle, RaysSkillData rayData, NativeArray<RaycastHit> hits, int parallelSize)
    {
        RaycastFilter raycastFilter = new()
        {
            Result = hits,
            RaycastHits = rayData.RayCastHitList,
        };

        return raycastFilter.Schedule(hits.Length, parallelSize, handle);
    }
}