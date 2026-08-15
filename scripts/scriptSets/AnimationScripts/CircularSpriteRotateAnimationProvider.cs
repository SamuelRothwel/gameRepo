using Godot;
using System.Collections.Generic;

public class CircularSpriteRotateAnimationProvider : IDynamicAnimationDefinitionProvider
{
	public IEnumerable<DynamicAnimationDefinition> CreateDynamicAnimationDefinitions()
	{
		yield return CreateCircularSpriteRotateAnimation("CircularSpriteRotate", 1f);
	}

	public static DynamicAnimationDefinition CreateCircularSpriteRotateAnimation(string name, float duration)
	{
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
			PropertyRequirements = CreatePropertyRequirements(),
			Transformations = new List<DynamicAnimationTransformation>
			{
				new DynamicAnimationTransformation
				{
					PropertyName = "item.Position.X",
					LoopVariable = "i",
					LoopCountVariable = "n",
					StartTime = 0f,
					EndTime = duration,
					StartValue = "sin(i/n*360)*5",
					EndValue = "sin((i+1)/n*360)*5",
					FunctionType = "sin((i+t)/n*360)*5"
				},
				new DynamicAnimationTransformation
				{
					PropertyName = "item.ZIndex",
					LoopVariable = "i",
					LoopCountVariable = "n",
					StartTime = 0f,
					EndTime = duration,
					StartValue = "cos(i/n*360)*1000+1000",
					EndValue = "cos((i+1)/n*360)*1000+1000",
					FunctionType = "cos((i+t)/n*360)*1000+1000"
				},
				new DynamicAnimationTransformation
				{
					PropertyName = "item.Scale.X",
					LoopVariable = "i",
					LoopCountVariable = "n",
					StartTime = 0f,
					EndTime = duration,
					StartValue = "cos(i/n*360)*0.1+0.9",
					EndValue = "cos((i+1)/n*360)*0.1+0.9",
					FunctionType = "cos((i+t)/n*360)*0.1+0.9"
				},
				new DynamicAnimationTransformation
				{
					PropertyName = "item.Scale.Y",
					LoopVariable = "i",
					LoopCountVariable = "n",
					StartTime = 0f,
					EndTime = duration,
					StartValue = "cos(i/n*360)*0.1+0.9",
					EndValue = "cos((i+1)/n*360)*0.1+0.9",
					FunctionType = "cos((i+t)/n*360)*0.1+0.9"
				}
			}
		};
	}

	static List<DynamicAnimationPropertyRequirement> CreatePropertyRequirements()
	{
		return new List<DynamicAnimationPropertyRequirement>
		{
			CreatePropertyRequirement("Position.X", "float"),
			CreatePropertyRequirement("ZIndex", "int"),
			CreatePropertyRequirement("Scale.X", "float"),
			CreatePropertyRequirement("Scale.Y", "float")
		};
	}

	static DynamicAnimationPropertyRequirement CreatePropertyRequirement(string propertyName, string valueTypeName)
	{
		return new DynamicAnimationPropertyRequirement
		{
			TargetName = "item",
			TargetTypeName = typeof(Sprite2D).FullName,
			PropertyName = propertyName,
			ValueTypeName = valueTypeName,
			InterfaceName = "ICircularSpriteRotateItemAnimationProperties"
		};
	}
}
