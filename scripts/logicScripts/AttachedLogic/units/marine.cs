using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using coolbeats.scripts.logicScripts.AttachedLogic.Components;
using Godot;

namespace coolbeats.scripts.logicScripts.AttachedLogic.units
{
    public partial class marine : unitControler
    {
        public marine()
        {
            unitKey = "marine";
            type = "attacker";
            radius = 30;
            detectionRadius = 150;
            maxHP = 50;
            traits.SetNumber("speed", 1);
            traits.SetNumber("attackRange", 0);
            traits.SetDescription("role", "attacker");
            QueueRedraw();
        }
    }

    public class MarineDefinitionProvider : IUnitDefinitionProvider
    {
        public IEnumerable<UnitDefinition> CreateDefinitions(Func<IEnumerable<IUnitBehavior>> behaviorFactory)
        {
            UnitBehaviorProfile marineBehaviors = new UnitBehaviorProfile();
            marineBehaviors.SetCommand("move", "chaseTarget");
            marineBehaviors.SetCommand("attack", "attackTarget", "chaseTarget");
            marineBehaviors.SetCommand("idle", "scanAttack", "scanChase");
            marineBehaviors.SetCommand("holdPosition", "scanAttack", "attackTarget");
            marineBehaviors.SetCommand("attackMove", "scanAttack", "scanChase", "chaseTarget");

            UnitDefinition marine = new UnitDefinition
            {
                Name = "marine",
                CommandType = "attacker",
                Radius = 30,
                DetectionRadius = 150,
                MaxHP = 50,
                BehaviorProfile = marineBehaviors,
                BehaviorFactory = behaviorFactory
            };
            marine.NumericalTraits["speed"] = 1;
            marine.NumericalTraits["attackRange"] = 0;
            marine.DescriptiveTraits["role"] = "attacker";
            marine.SpriteAttachments.Add(new UnitSpriteAttachmentData
            {
                Name = "body",
                SpriteSetKey = "Marine",
                Order = 0,
                Traits = new List<UnitDataTrait>
                {
                    new UnitDataTrait { Key = "damageable", ValueType = "bool", ValueJson = "true" },
                    new UnitDataTrait { Key = "hitboxEnabled", ValueType = "bool", ValueJson = "true" }
                }
            });
            marine.ComponentAttachments.Add(CreateGunComponent(new Vector2(13, -10)));
            yield return marine;
        }

        public UnitDefinition NormalizeDefinition(UnitDefinition definition, Func<Guid, UnitDefinition> getUnitDefinition)
        {
            if (definition == null || definition.Name != "marine")
            {
                return definition;
            }
            if (definition.ComponentAttachments.Any(component => component.Name == "gun"))
            {
                return definition;
            }

            UnitSubUnitAttachmentData gunSubUnit = definition.SubUnitAttachments
                .FirstOrDefault(subUnit => subUnit.Name == "gun" || getUnitDefinition(subUnit.ChildUnitId)?.Name == "marineGun");
            Vector2 gunPosition = gunSubUnit?.Position ?? new Vector2(13, -10);
            if (gunSubUnit != null)
            {
                definition.SubUnitAttachments.Remove(gunSubUnit);
            }

            definition.ComponentAttachments.Add(CreateGunComponent(gunPosition));
            return definition;
        }

        static UnitComponentAttachmentData CreateGunComponent(Vector2 position)
        {
            UnitComponentAttachmentData gun = new UnitComponentAttachmentData
            {
                Name = "gun",
                TypeName = typeof(componentGun).FullName ?? nameof(componentGun),
                Position = position,
                Order = 0,
                Traits = new List<UnitDataTrait>
                {
                    new UnitDataTrait { Key = "role", ValueType = "text", ValueJson = JsonSerializer.Serialize("weapon") },
                    new UnitDataTrait { Key = "automaticBehaviour", ValueType = "text", ValueJson = JsonSerializer.Serialize("none") },
                    new UnitDataTrait { Key = "positionX", ValueType = "number", ValueJson = JsonSerializer.Serialize(position.X) },
                    new UnitDataTrait { Key = "positionY", ValueType = "number", ValueJson = JsonSerializer.Serialize(position.Y) }
                }
            };

            gun.ChildComponents.Add(new UnitComponentAttachmentData
            {
                Name = "circular sprites",
                TypeName = typeof(GunComponentRing).FullName ?? nameof(GunComponentRing),
                Order = 0,
                Traits = new List<UnitDataTrait>
                {
                    new UnitDataTrait { Key = "spriteIterator", ValueType = "text", ValueJson = JsonSerializer.Serialize("circular") },
                    new UnitDataTrait { Key = "spriteCount", ValueType = "number", ValueJson = JsonSerializer.Serialize(3f) }
                },
                SpriteAttachments = new List<UnitSpriteAttachmentData>
                {
                    new UnitSpriteAttachmentData { Name = "barrel", SpriteSetKey = "GunBarrel", Order = 0 },
                    new UnitSpriteAttachmentData { Name = "barrel", SpriteSetKey = "GunBarrel", Order = 1 },
                    new UnitSpriteAttachmentData { Name = "barrel", SpriteSetKey = "GunBarrel", Order = 2 }
                }
            });

            return gun;
        }
    }
}
