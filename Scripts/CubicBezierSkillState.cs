using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class CubicBezierSkillState : ISkillState
{
    RaysSkillData _raysData;
    NativeArray<RaycastHit> _hits;
    ICollection<RaycastHit> _hitsCollection;
    IPlayer _player;
    ICell _playerCell;
    ICell _cellUnderMouse;

    public void OnEnter()
    {
        _playerCell = StateUtility.GetPlayersCell(_player);
        _hits = new NativeArray<RaycastHit>(CubicBezierSkill.Path.Count, Allocator.Persistent);
        _hitsCollection = new List<RaycastHit>();
        _raysData = new RaysSkillData(NativeCollectionUtility.ToNativeArray(CubicBezierSkill.Path, Allocator.Persistent),
                                      NativeCollectionUtility.ToNativeArray(CubicBezierSkill.Shape, Allocator.Persistent),
                                      new NativeArray<RaycastCommand>(CubicBezierSkill.Size, Allocator.Persistent),
                                      new NativeArray<RaycastHit>(CubicBezierSkill.Size, Allocator.Persistent));
    }

    public void OnExit()
    {
        _raysData.PathList.Dispose();
        _raysData.ShapePoints.Dispose();
        _raysData.CommandList.Dispose();
        _raysData.RayCastHitList.Dispose();

        _hits.Dispose();
        _hitsCollection.Clear();
    }

    public void Tick()
    {
        if (StateUtility.GetCellUnderMouseIfChanged(out var cell, _cellUnderMouse))
        {
            _cellUnderMouse = cell;

            if (GridUtility.GetDistance(_playerCell, _cellUnderMouse) > CubicBezierSkill.MaxSelectRange)
                return;

            _raysData.TransformRayRelativeToWorld(_playerCell.GetPosition(),
                                                GridUtility.GetDirection(_playerCell, _cellUnderMouse))
                     .GetRaycastCommandHandle(CubicBezierSkill.parallelSize)
                     .FiltrRaycastByHit(_raysData, _hits, CubicBezierSkill.parallelSize)
                     .Complete();

            GetAllHitsFromRaycast(_hits, _hitsCollection);

            SkillVisualizer.VisualizeSkill(_raysData.CommandList.ToArray(),
                                           _hitsCollection.ToNewArray(),
                                           CubicBezierSkill.Color,
                                           CubicBezierSkill.Time,
                                           CubicBezierSkill.Scale);

            SetSkillParametr(_playerCell, _cellUnderMouse);
            _hitsCollection.Clear();
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (_player != null && _raysData != null && _cellUnderMouse != null && _playerCell != null)
            {
                Context.Get<TurnController>().SubmitAction(new CubicBezierSkill(_player, _raysData, _cellUnderMouse, _playerCell));
                StateHolder.SetState(States.SelectState);
            }
        }
    }

    public ISkillState WithPlayer(IPlayer player)
    {
        _player = player;
        return this;
    }

    void SetSkillParametr(ICell playerCell, ICell cellUnderMouse)
    {
        Quaternion rotation = Quaternion.LookRotation(GridUtility.GetDirection(playerCell, cellUnderMouse));
        Vector3 startPos = rotation * CubicBezierSkill.CubicBezier.GetOrientedPoint(0).Position;

        VisualEffect visualEffect = VisualService.Visualise(CubicBezierSkill.Scale.Path, startPos, Quaternion.identity).GetComponent<VisualEffect>();

        visualEffect.SetVector3(CubicBezierSkill.visualEffectNames[0], rotation * CubicBezierSkill.CubicBezier._startPosHandler);
        visualEffect.SetVector3(CubicBezierSkill.visualEffectNames[1], rotation * CubicBezierSkill.CubicBezier._endPosHandler);
        visualEffect.SetVector3(CubicBezierSkill.visualEffectNames[2], rotation * CubicBezierSkill.CubicBezier._endPos);
    }

    void GetAllHitsFromRaycast(NativeArray<RaycastHit> hits, ICollection<RaycastHit> hitCollection)
    {
        foreach (RaycastHit hit in hits)
        {
            if (hit.IsDeafult())
                continue;

            hitCollection.Add(hit);
        }
    }
}