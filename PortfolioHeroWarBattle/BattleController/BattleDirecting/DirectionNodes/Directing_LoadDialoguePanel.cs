using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Directing_LoadDialoguePanel : BaseDirecting
{
    public Directing_LoadDialoguePanel(Action complete = null) : base(complete)
    {
    }

    public override void Execute()
    {
        var dialogueController = GameSystem.Instance.GetService<DialogueController>();
        if (dialogueController == null)
        {
            dialogueController = new DialogueController();
            dialogueController.Init(GameSystem.Instance);
        }

        dialogueController.LoadView();

        Complete();
    }
}
