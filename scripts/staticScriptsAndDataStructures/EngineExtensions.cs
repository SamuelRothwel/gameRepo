
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;

public static class EngineExtensions
{
    public static void TryQueueFree(this Node2D node)
    {
        try {
            node.QueueFree();
        } catch {}
    }
}