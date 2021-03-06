﻿using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class VennWireComponentSolver : ComponentSolver
{
    public VennWireComponentSolver(BombCommander bombCommander, MonoBehaviour bombComponent, IRCConnection ircConnection, CoroutineCanceller canceller) :
        base(bombCommander, bombComponent, ircConnection, canceller)
    {
        _wires = (Array)_activeWiresProperty.GetValue(bombComponent, null);
    }

    protected override IEnumerator RespondToCommandInternal(string inputCommand)
    {
        if (!inputCommand.StartsWith("cut ", StringComparison.InvariantCultureIgnoreCase))
        {
            yield break;
        }
        inputCommand = inputCommand.Substring(4);

        int beforeButtonStrikeCount = StrikeCount;

        string[] sequence = inputCommand.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string wireIndexString in sequence)
        {
            int wireIndex = 0;
            if (!int.TryParse(wireIndexString, out wireIndex))
            {
                continue;
            }

            wireIndex--;

            if (wireIndex >= 0 && wireIndex < _wires.Length)
            {
                yield return wireIndexString;

                if (Canceller.ShouldCancel)
                {
                    Canceller.ResetCancel();
                    yield break;
                }

                MonoBehaviour wire = (MonoBehaviour)_wires.GetValue(wireIndex);

                DoInteractionStart(wire);
                yield return new WaitForSeconds(0.1f);
                DoInteractionEnd(wire);

                //Escape the sequence if a part of the given sequence is wrong
                if (StrikeCount != beforeButtonStrikeCount)
                {
                    break;
                }
            }
        }
    }

    static VennWireComponentSolver()
    {
        _vennWireComponentType = ReflectionHelper.FindType("Assets.Scripts.Components.VennWire.VennWireComponent");
        _activeWiresProperty = _vennWireComponentType.GetProperty("ActiveWires", BindingFlags.Public | BindingFlags.Instance);
    }

    private static Type _vennWireComponentType = null;
    private static PropertyInfo _activeWiresProperty = null;

    private Array _wires = null;
}
