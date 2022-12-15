using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NewBattleCore;

public class Directing_WaveStartDirecting : BaseDirecting
{
    private CurrentBattleDirectingWaveInfo _waveInfo;
    private React.Reactor _reactor;
    protected Action<CurrentBattleDirectingWaveInfo> _onStartDirecting = null;
    protected Action<CurrentBattleDirectingWaveInfo> _onEndDirectingType = null;

    public Directing_WaveStartDirecting(Action complete, CurrentBattleDirectingWaveInfo waveInfo, Action<CurrentBattleDirectingWaveInfo> onStartDirecting = null, Action<CurrentBattleDirectingWaveInfo> onEndDirectingType = null) : base(complete)
    {
        _waveInfo = waveInfo;
        _onStartDirecting = onStartDirecting;
        _onEndDirectingType = onEndDirectingType;
    }

    public override void Execute()
    {
        if (!_waveInfo.IsActive)
        {
            Complete();
            return;
        }

        if(_onStartDirecting != null) {
            _onStartDirecting(_waveInfo);
        }

        var _scene = BattleManager.Instance.scene;
        string directingPath = _waveInfo.DirectingPath;
        _reactor = ResourceLoader.Instantiate<React.Reactor>(directingPath);

        var dialogue = _scene.System.GetService<DialogueController>();
        if (dialogue != null)
            dialogue.ToggleSkipButton(_scene.Info.DirectingSkipEnable);

        _reactor.Context = _scene.System;
        _reactor.OnFinish = Complete;
        _reactor.Resume();
    }

    protected override void Complete()
    {
        var _scene = BattleManager.Instance.scene;
        DirectingCleanUp.CleanUp(_scene);

        if (_reactor != null)
        {
            _reactor.Dispose();
            _reactor = null;
        }

        if(_onEndDirectingType != null) {
            _onEndDirectingType(_waveInfo);
        }

        base.Complete();

    }
}
