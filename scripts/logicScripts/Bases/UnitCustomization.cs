using System;
using System.Collections.Generic;
using System.Linq;
using coolbeats.scripts.logicScripts.AttachedLogic.Components;
using coolbeats.scripts.logicScripts.AttachedLogic.SubComponents;
using coolbeats.scripts.managerScripts;
using Godot;

public enum UnitBehaviorResult
{
    Continue,
    Stop
}

public interface IUnitBehavior
{
    string Name { get; }
    UnitBehaviorResult Process(unitControler unit, double delta);
}

public class UnitTraitSet
{
    readonly Dictionary<string, float> numericalTraits = new();
    readonly Dictionary<string, string> descriptiveTraits = new();

    public IReadOnlyDictionary<string, float> Numerical => numericalTraits;
    public IReadOnlyDictionary<string, string> Descriptive => descriptiveTraits;

    public void SetNumber(string name, float value) => numericalTraits[name] = value;
    public float GetNumber(string name, float fallback = 0)
    {
        return numericalTraits.TryGetValue(name, out float value) ? value : fallback;
    }

    public void SetDescription(string name, string value) => descriptiveTraits[name] = value;
    public string GetDescription(string name, string fallback = "")
    {
        return descriptiveTraits.TryGetValue(name, out string value) ? value : fallback;
    }

    public bool HasDescription(string name, string value)
    {
        return descriptiveTraits.TryGetValue(name, out string current) && current == value;
    }

    public void Clear()
    {
        numericalTraits.Clear();
        descriptiveTraits.Clear();
    }
}

public class UnitBehaviorProfile
{
    readonly Dictionary<string, List<string>> commandBehaviors = new();

    public void SetCommand(string commandName, params string[] behaviorNames)
    {
        commandBehaviors[commandName] = behaviorNames.ToList();
    }

    public IReadOnlyList<string> GetBehaviors(string commandName)
    {
        return commandBehaviors.TryGetValue(commandName, out List<string> behaviors)
            ? behaviors
            : Array.Empty<string>();
    }
}

public class UnitBehaviorController
{
    readonly Dictionary<string, IUnitBehavior> availableBehaviors = new();
    readonly List<IUnitBehavior> activeBehaviors = new();

    public UnitBehaviorProfile Profile { get; private set; } = new();

    public IReadOnlyList<IUnitBehavior> ActiveBehaviors => activeBehaviors;

    public void SetProfile(UnitBehaviorProfile profile)
    {
        Profile = profile;
    }

    public void Register(IUnitBehavior behavior)
    {
        availableBehaviors[behavior.Name] = behavior;
    }

    public void ActivateCommand(string commandName)
    {
        activeBehaviors.Clear();
        foreach (string behaviorName in Profile.GetBehaviors(commandName))
        {
            if (availableBehaviors.TryGetValue(behaviorName, out IUnitBehavior behavior))
            {
                activeBehaviors.Add(behavior);
            }
            else
            {
                GD.Print("Missing unit behavior: ", behaviorName);
            }
        }
    }

    public void Process(unitControler unit, double delta)
    {
        foreach (IUnitBehavior behavior in activeBehaviors)
        {
            if (behavior.Process(unit, delta) == UnitBehaviorResult.Stop)
            {
                return;
            }
        }
    }
}

public class UnitAttachment
{
    readonly Dictionary<string, Func<unitControler, command, bool>> callableBehaviors = new();

    public UnitAttachment(string name, Node2D node)
    {
        Name = name;
        Node = node;
    }

    public string Name { get; }
    public Node2D Node { get; }
    public UnitTraitSet Traits { get; } = new();

    public void RegisterBehavior(string name, Func<unitControler, command, bool> behavior)
    {
        callableBehaviors[name] = behavior;
    }

    public bool Call(string behaviorName, unitControler owner, command command)
    {
        return callableBehaviors.TryGetValue(behaviorName, out Func<unitControler, command, bool> behavior)
            && behavior(owner, command);
    }
}

public class UnitAttachmentController
{
    readonly Dictionary<string, UnitAttachment> attachments = new();

    public IReadOnlyDictionary<string, UnitAttachment> Attachments => attachments;

    public UnitAttachment RegisterComponent(componentController component)
    {
        UnitAttachment attachment = new UnitAttachment(component.self.Name.ToString(), component.self);
        attachment.Traits.SetDescription("source", "component");

        if (component is componentGun gun)
        {
            attachment.Traits.SetDescription("role", "weapon");
            attachment.RegisterBehavior("fire", (owner, command) =>
            {
                if (command.unit == Guid.Empty)
                {
                    return false;
                }
                gun.target(command.unit);
                return true;
            });
        }

        attachments[attachment.Name] = attachment;
        return attachment;
    }

    public UnitAttachment AttachUnit(unitControler owner, unitControler unit, string attachmentName)
    {
        if (unit.GetParent() != owner)
        {
            unit.GetParent()?.RemoveChild(unit);
            owner.AddChild(unit);
        }

        UnitAttachment attachment = new UnitAttachment(attachmentName, unit);
        attachment.Traits.SetDescription("source", "unit");
        attachments[attachmentName] = attachment;
        return attachment;
    }

    public bool Call(string attachmentName, string behaviorName, unitControler owner, command command)
    {
        return attachments.TryGetValue(attachmentName, out UnitAttachment attachment)
            && attachment.Call(behaviorName, owner, command);
    }
}

public class UnitDefinition
{
    public string Name { get; set; }
    public string CommandType { get; set; }
    public float Radius { get; set; }
    public float DetectionRadius { get; set; }
    public float MaxHP { get; set; }
    public Dictionary<string, float> NumericalTraits { get; } = new();
    public Dictionary<string, string> DescriptiveTraits { get; } = new();
    public UnitBehaviorProfile BehaviorProfile { get; set; } = new();
    public Func<IEnumerable<IUnitBehavior>> BehaviorFactory { get; set; } = () => Array.Empty<IUnitBehavior>();

    public void ApplyTo(unitControler unit)
    {
        unit.unitKey = Name;
        unit.type = CommandType;
        unit.radius = Radius;
        unit.detectionRadius = DetectionRadius;
        unit.maxHP = MaxHP;

        unit.traits.Clear();
        foreach (KeyValuePair<string, float> trait in NumericalTraits)
        {
            unit.traits.SetNumber(trait.Key, trait.Value);
        }
        foreach (KeyValuePair<string, string> trait in DescriptiveTraits)
        {
            unit.traits.SetDescription(trait.Key, trait.Value);
        }

        unit.behaviors = new UnitBehaviorController();
        unit.behaviors.SetProfile(BehaviorProfile);
        foreach (IUnitBehavior behavior in BehaviorFactory())
        {
            unit.behaviors.Register(behavior);
        }
        unit.sendCommand(new command("idle"));
    }
}

public class ScanAttackBehavior : IUnitBehavior
{
    public string Name => "scanAttack";

    public UnitBehaviorResult Process(unitControler unit, double delta)
    {
        bool stationaryAttack = false;
        foreach (componentGun gun in unit.GetComponents<componentGun>())
        {
            Guid? target = gun.scan();
            if (target != null)
            {
                stationaryAttack = true;
                unit.Rotate(unit.GetAngleTo(mAccess.unitManager.units[(Guid)target].Position) + math.PI / 2);
            }
        }
        return stationaryAttack ? UnitBehaviorResult.Stop : UnitBehaviorResult.Continue;
    }
}

public class ScanChaseBehavior : IUnitBehavior
{
    public string Name => "scanChase";

    public UnitBehaviorResult Process(unitControler unit, double delta)
    {
        Guid? target = unit.scanTargets(unit.traits.GetNumber("attackRange"));
        if (target == null)
        {
            return UnitBehaviorResult.Continue;
        }

        unit.move(mAccess.unitManager.units[(Guid)target].Position);
        return UnitBehaviorResult.Stop;
    }
}

public class AttackTargetBehavior : IUnitBehavior
{
    public string Name => "attackTarget";

    public UnitBehaviorResult Process(unitControler unit, double delta)
    {
        if (unit.activeCommand.unit == Guid.Empty)
        {
            return UnitBehaviorResult.Continue;
        }

        Guid target = unit.activeCommand.unit;
        if (unit.Position.DistanceTo(mAccess.unitManager.units[target].Position) < unit.traits.GetNumber("attackRange"))
        {
            unit.attack(target);
            return UnitBehaviorResult.Stop;
        }

        return UnitBehaviorResult.Continue;
    }
}

public class ChaseTargetBehavior : IUnitBehavior
{
    public string Name => "chaseTarget";

    public UnitBehaviorResult Process(unitControler unit, double delta)
    {
        if (unit.activeCommand.unit != Guid.Empty)
        {
            unit.move(mAccess.unitManager.units[unit.activeCommand.unit].Position);
        }
        else
        {
            unit.move(unit.activeCommand.coordinates);
        }
        return UnitBehaviorResult.Continue;
    }
}
