using Godot;
using System.Collections.Generic;

public class HorizontalMoveAnimationProvider : IDynamicAnimationDefinitionProvider
{
	public const string AnimationName = "Move 200 Left And Right";

	public IEnumerable<DynamicAnimationDefinition> CreateDynamicAnimationDefinitions()
	{
		yield return CreateHorizontalMoveAnimation(AnimationName, 2f, 200f);
	}

	public static DynamicAnimationDefinition CreateHorizontalMoveAnimation(
		string name,
		float duration,
		float distance)
	{
		float halfDuration = duration / 2f;
		return new DynamicAnimationDefinition
		{
			Name = name,
			Duration = duration,
			Variables = new List<DynamicAnimationVariable>
			{
				new DynamicAnimationVariable
				{
					Name = "n",
					Source = "enumerable.count"
				}
			},
			PropertyRequirements = new List<DynamicAnimationPropertyRequirement>
			{
				new DynamicAnimationPropertyRequirement
				{
					TargetName = "item",
					TargetTypeName = typeof(Node2D).FullName,
					PropertyName = "Position.X",
					ValueTypeName = "float",
					InterfaceName = "IHorizontalMoveItemAnimationProperties"
				}
			},
			Transformations = new List<DynamicAnimationTransformation>
			{
				CreateMovement(0f, halfDuration, 0f, -distance),
				CreateMovement(halfDuration, duration, -distance, 0f)
			}
		};
	}

	static DynamicAnimationTransformation CreateMovement(
		float startTime,
		float endTime,
		float startX,
		float endX)
	{
		return new DynamicAnimationTransformation
		{
			PropertyName = "item.Position.X",
			LoopVariable = "i",
			LoopCountVariable = "n",
			StartTime = startTime,
			EndTime = endTime,
			StartValue = startX.ToString(System.Globalization.CultureInfo.InvariantCulture),
			EndValue = endX.ToString(System.Globalization.CultureInfo.InvariantCulture),
			FunctionType = "linear"
		};
	}
}
