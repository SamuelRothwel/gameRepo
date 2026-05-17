using Godot;
using coolbeats.scripts.staticScriptsAndDataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Microsoft.VisualBasic;

public partial class AnimationManagement : managerNode
{
	[Export] public PackedScene packedAnimations;
	public Dictionary<string, AnimationLibrary> animationSets = new Dictionary<string, AnimationLibrary>();
	public Dictionary<string, DynamicAnimationDefinition> dynamicAnimationDefinitions = new Dictionary<string, DynamicAnimationDefinition>();
	List<DynamicAnimator> activeDynamicAnimators = new List<DynamicAnimator>();
	public Dictionary<string, int> trackTypeIndex = new Dictionary<string, int>
	{ { "Value", 0 }, { "Position3D", 1 }, {"Rotation3D", 2 }, {"Scale3D", 3 }, { "BlendShape", 4 }, { "Method", 5 }, { "Bezier", 6 }, { "Audio", 7 }, { "Animation", 8 } };
	public override void setup()
	{
		DynamicAnimationGeneratedAccessors.RegisterAll();
		Node animationScene = packedAnimations.Instantiate();
		Dictionary<string, AudioStream> audioFiles = fileSearch.getAudio("");
		foreach (AnimationSet node in animationScene.GetChildren())
		{
			node.setup(audioFiles);
			CreateAnimationLibrary(node.Name, node.library.ToArray());
		}
		RegisterDynamicAnimation(new DynamicAnimationDefinition
		{
			Name = "CircularSpriteRotate",
			Duration = 1f,
			Variables = new List<DynamicAnimationVariable>
			{
				new DynamicAnimationVariable
				{
					Name = "n",
					Source = "iterator.count"
				}
			},
			PropertyRequirements = CreateCircularSpritePropertyRequirements(),
			Transformations = new List<DynamicAnimationTransformation>
			{
				new DynamicAnimationTransformation
				{
					PropertyName = "item.Position.X",
					LoopVariable = "i",
					LoopCountVariable = "n",
					StartTime = 0f,
					EndTime = 1f,
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
					EndTime = 1f,
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
					EndTime = 1f,
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
					EndTime = 1f,
					StartValue = "cos(i/n*360)*0.1+0.9",
					EndValue = "cos((i+1)/n*360)*0.1+0.9",
					FunctionType = "cos((i+t)/n*360)*0.1+0.9"
				}
			}
		});
	}

	public override void _Process(double delta)
	{
		for (int i = activeDynamicAnimators.Count - 1; i >= 0; i--)
		{
			DynamicAnimator animator = activeDynamicAnimators[i];
			animator.Process(delta);
			if (animator.IsComplete)
			{
				activeDynamicAnimators.RemoveAt(i);
			}
		}
	}

	public void RegisterDynamicAnimation(DynamicAnimationDefinition definition)
	{
		dynamicAnimationDefinitions[definition.Name] = definition;
	}

	public DynamicAnimationDefinition SaveDynamicAnimationDefinition(DynamicAnimationDefinition definition)
	{
		RegisterDynamicAnimation(definition);
		mAccess.entityFrameworkManager?.SaveAnimation(new StoredAnimation
		{
			Name = definition.Name,
			AnimationType = "DynamicProperty",
			Duration = definition.Duration,
			Variables = definition.Variables.Select(variable => new StoredAnimationVariable
				{
					Name = variable.Name,
					Source = variable.Source
				}).ToList(),
			PropertyRequirements = definition.PropertyRequirements.Select(requirement => new StoredAnimationPropertyRequirement
				{
					TargetName = requirement.TargetName,
					TargetTypeName = requirement.TargetTypeName,
					PropertyName = requirement.PropertyName,
					ValueTypeName = requirement.ValueTypeName,
					InterfaceName = requirement.InterfaceName
				}).ToList(),
			Transformations = definition.Transformations.Select(transformation => new StoredAnimationTransformation
				{
					PropertyName = transformation.PropertyName,
					LoopVariable = transformation.LoopVariable,
					LoopCountVariable = transformation.LoopCountVariable,
					StartTime = transformation.StartTime,
					EndTime = transformation.EndTime,
					StartValue = transformation.StartValue,
					EndValue = transformation.EndValue,
					FunctionType = transformation.FunctionType
				}).ToList()
		});
		return definition;
	}

	public DynamicAnimationDefinition SaveCircularSpriteRotateAnimation(string name, float duration, float rotationDegrees)
	{
		return SaveDynamicAnimationDefinition(new DynamicAnimationDefinition
		{
			Name = name,
			Duration = duration,
			Variables = new List<DynamicAnimationVariable>
			{
				new DynamicAnimationVariable
				{
					Name = "n",
					Source = "iterator.count"
				}
			},
			PropertyRequirements = CreateCircularSpritePropertyRequirements(),
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
		});
	}

	public void RegisterStoredDynamicAnimations(IEnumerable<StoredAnimation> animations)
	{
		foreach (StoredAnimation animation in animations)
		{
			RegisterDynamicAnimation(new DynamicAnimationDefinition
			{
				Name = animation.Name,
				Duration = animation.Duration,
				Variables = animation.Variables.Select(variable => new DynamicAnimationVariable
				{
					Name = variable.Name,
					Source = variable.Source
				}).ToList(),
				PropertyRequirements = animation.PropertyRequirements.Select(requirement => new DynamicAnimationPropertyRequirement
				{
					TargetName = requirement.TargetName,
					TargetTypeName = requirement.TargetTypeName,
					PropertyName = requirement.PropertyName,
					ValueTypeName = requirement.ValueTypeName,
					InterfaceName = requirement.InterfaceName
				}).ToList(),
				Transformations = animation.Transformations.Select(transformation => new DynamicAnimationTransformation
				{
					PropertyName = transformation.PropertyName,
					LoopVariable = transformation.LoopVariable,
					LoopCountVariable = transformation.LoopCountVariable,
					StartTime = transformation.StartTime,
					EndTime = transformation.EndTime,
					StartValue = transformation.StartValue,
					EndValue = transformation.EndValue,
					FunctionType = transformation.FunctionType
				}).ToList()
			});
		}
	}

	List<DynamicAnimationPropertyRequirement> CreateCircularSpritePropertyRequirements()
	{
		return new List<DynamicAnimationPropertyRequirement>
		{
			CreateCircularSpritePropertyRequirement("Position.X", "float"),
			CreateCircularSpritePropertyRequirement("ZIndex", "int"),
			CreateCircularSpritePropertyRequirement("Scale.X", "float"),
			CreateCircularSpritePropertyRequirement("Scale.Y", "float")
		};
	}

	DynamicAnimationPropertyRequirement CreateCircularSpritePropertyRequirement(string propertyName, string valueTypeName)
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

	public DynamicAnimator RotateCircularSprites(
		CircularEnumerator<Sprite2D> sprites,
		Action<string> eventHandler = null,
		float speed = 1f)
	{
		DynamicAnimationDefinition definition = dynamicAnimationDefinitions["CircularSpriteRotate"];
		DynamicAnimator animator = new DynamicAnimator(
			definition,
			new CircularSpriteAnimationTarget(sprites),
			eventHandler)
		{
			Speed = speed
		};
		activeDynamicAnimators.Add(animator);
		return animator;
	}
	public Godot.Collections.Dictionary methodCaller(string name, Variant[] args = null)
	{
		return new Godot.Collections.Dictionary
		{
			{ "method", name },
			{ "args", new Godot.Collections.Array ( args==null? new Variant[0] : args)}
		};
	}
	public Godot.Collections.Dictionary audioCaller(AudioStream stream, float startOffset = 0f, float endOffset = 0f)
	{
		return new Godot.Collections.Dictionary
		{
			{ "stream", stream },
			{ "start_offset", startOffset },
			{ "end_offset", endOffset }
		};
	}
	public Godot.Collections.Dictionary audioCaller(string streamPath, float startOffset = 0f, float endOffset = 0f)
	{
		AudioStream stream = GD.Load<AudioStream>(streamPath);
		return audioCaller(stream, startOffset, endOffset);
	}
	public Texture2D spriteCaller(string path)
	{
		return GD.Load<Texture2D>(path);
	}
	#region geters

	
	#endregion
	#region animationLibrary
	public void CreateAnimationLibrary(string libraryName, (string, (float, (string, Tuple<double, Variant>[])[]))[] storedAnimations)
	{
		AnimationLibrary library = new AnimationLibrary();
		for (int i = 0; i < storedAnimations.Count(); i++)
		{
			library.AddAnimation(storedAnimations[i].Item1, CreateAnimation(ref storedAnimations[i].Item2));
		}
		animationSets.Add(libraryName, library);
	}

	public Animation CreateAnimation(ref (float, (string, Tuple<double, Variant>[])[]) tracks)
	{
		Animation animation = new Animation();
		animation.Length = tracks.Item1;
		for (int i = 0; i < tracks.Item2.Count(); i++)
		{
			AddTrack(animation, ref tracks.Item2[i]);
		}
		return animation;
	}

	public void AddTrack(Animation animation, ref (string, Tuple<double, Variant>[]) track)
	{
		string[] trackType = track.Item1.Split("|");
		int trackIndex = animation.AddTrack((Animation.TrackType)trackTypeIndex[trackType[0]]);
		if (trackType.Count() > 1)
		{
			animation.TrackSetPath(trackIndex, trackType[1]);
		}
		for (int i = 0; i < track.Item2.Count(); i++)
		{
			animation.TrackInsertKey(trackIndex, track.Item2[i].Item1, track.Item2[i].Item2);
		}
	}
	#endregion
}

public class DynamicAnimationDefinition
{
	public string Name { get; set; } = "";
	public float Duration { get; set; } = 1f;
	public List<DynamicAnimationVariable> Variables { get; set; } = new();
	public List<DynamicAnimationPropertyRequirement> PropertyRequirements { get; set; } = new();
	public List<DynamicAnimationTransformation> Transformations { get; set; } = new();
}

public class DynamicAnimationVariable
{
	public string Name { get; set; } = "";
	public string Source { get; set; } = "";
}

public class DynamicAnimationPropertyRequirement
{
	public string TargetName { get; set; } = "";
	public string TargetTypeName { get; set; } = "";
	public string PropertyName { get; set; } = "";
	public string ValueTypeName { get; set; } = "";
	public string InterfaceName { get; set; } = "";
}

public class DynamicAnimationTransformation
{
	public string PropertyName { get; set; } = "";
	public string LoopVariable { get; set; } = "";
	public string LoopCountVariable { get; set; } = "";
	public float StartTime { get; set; }
	public float EndTime { get; set; }
	public string StartValue { get; set; } = "0";
	public string EndValue { get; set; } = "0";
	public string FunctionType { get; set; } = "linear";
}

public interface IDynamicAnimationTarget
{
	float GetVariable(string source);
	object ResolveAnimationTarget(string name, int index);
	void CompleteAnimation();
}

public class DynamicAnimator
{
	Action<string> eventHandler;
	IDynamicAnimationTarget target;
	DynamicAnimationDefinition definition;
	Dictionary<string, float> variables = new Dictionary<string, float>();
	double elapsed;
	public string Name { get; }
	public float Speed { get; set; } = 1f;
	public bool IsComplete { get; private set; }

	public DynamicAnimator(DynamicAnimationDefinition definition, IDynamicAnimationTarget target, Action<string> eventHandler)
	{
		Name = definition.Name;
		this.definition = definition;
		this.target = target;
		this.eventHandler = eventHandler;
		foreach (DynamicAnimationVariable variable in definition.Variables)
		{
			variables[variable.Name] = target.GetVariable(variable.Source);
		}
	}

	public void Process(double delta)
	{
		if (IsComplete)
		{
			return;
		}

		double scaledDelta = delta * Speed;
		double previousElapsed = elapsed;
		elapsed = Math.Min(elapsed + scaledDelta, Math.Max(definition.Duration, 0.001f));
		variables["time"] = (float)elapsed;
		variables["t"] = (float)(elapsed / Math.Max(definition.Duration, 0.001f));
		foreach (DynamicAnimationTransformation transformation in definition.Transformations)
		{
			ApplyTransformation(transformation, elapsed);
		}

		if (elapsed >= definition.Duration)
		{
			IsComplete = true;
			target.CompleteAnimation();
			eventHandler?.Invoke("finished");
		}
	}

	void ApplyTransformation(DynamicAnimationTransformation transformation, double currentElapsed)
	{
		float startTime = Math.Min(transformation.StartTime, transformation.EndTime);
		float endTime = Math.Max(transformation.StartTime, transformation.EndTime);
		if (currentElapsed < startTime || currentElapsed > endTime)
		{
			return;
		}

		float localDuration = Math.Max(endTime - startTime, 0.001f);
		variables["t"] = (float)((currentElapsed - startTime) / localDuration);
		if (!string.IsNullOrEmpty(transformation.LoopVariable))
		{
			int count = Math.Max(0, (int)GetVariableValue(transformation.LoopCountVariable));
			for (int i = 0; i < count; i++)
			{
				variables[transformation.LoopVariable] = i;
				ApplyPropertyPath(transformation.PropertyName, i, GetValueAt(transformation));
			}
			return;
		}

		ApplyPropertyPath(transformation.PropertyName, -1, GetValueAt(transformation));
	}

	void ApplyPropertyPath(string propertyPath, int index, float value)
	{
		string[] path = propertyPath.Split('.', 2);
		if (path.Length != 2)
		{
			return;
		}

		object targetObject = target.ResolveAnimationTarget(path[0], index);
		if (targetObject == null)
		{
			return;
		}

		IDynamicAnimationPropertyAccessor accessor = DynamicAnimationPropertyAccessorRegistry.Get(path[0], path[1], targetObject.GetType());
		accessor?.SetValue(targetObject, path[1], value);
	}

	float GetValueAt(DynamicAnimationTransformation transformation)
	{
		if (transformation.FunctionType == "linear")
		{
			return Mathf.Lerp(
				ExpressionEvaluator.Evaluate(transformation.StartValue, variables),
				ExpressionEvaluator.Evaluate(transformation.EndValue, variables),
				GetVariableValue("t"));
		}

		return ExpressionEvaluator.Evaluate(transformation.FunctionType, variables);
	}

	float GetVariableValue(string name)
	{
		return variables.TryGetValue(name, out float value) ? value : 0f;
	}
}

public class CircularSpriteAnimationTarget : IDynamicAnimationTarget
{
	CircularEnumerator<Sprite2D> sprites;

	public CircularSpriteAnimationTarget(CircularEnumerator<Sprite2D> sprites)
	{
		this.sprites = sprites;
	}

	public float GetVariable(string source)
	{
		if (source == "iterator.count")
		{
			return sprites.Count;
		}

		return 0;
	}

	public object ResolveAnimationTarget(string name, int index)
	{
		if (name != "item" || index < 0 || index >= sprites.Count)
		{
			return null;
		}

		return sprites.GetLoopItem(index);
	}

	public void CompleteAnimation()
	{
		sprites.MoveNext();
	}
}

public static class ExpressionEvaluator
{
	public static float Evaluate(string expression, Dictionary<string, float> variables)
	{
		return new ExpressionParser(expression, variables).Parse();
	}
}

public class ExpressionParser
{
	string expression;
	Dictionary<string, float> variables;
	int index;

	public ExpressionParser(string expression, Dictionary<string, float> variables)
	{
		this.expression = expression ?? "0";
		this.variables = variables;
	}

	public float Parse()
	{
		float value = ParseExpression();
		SkipWhitespace();
		return value;
	}

	float ParseExpression()
	{
		float value = ParseTerm();
		while (true)
		{
			SkipWhitespace();
			if (Match('+'))
			{
				value += ParseTerm();
			}
			else if (Match('-'))
			{
				value -= ParseTerm();
			}
			else
			{
				return value;
			}
		}
	}

	float ParseTerm()
	{
		float value = ParseFactor();
		while (true)
		{
			SkipWhitespace();
			if (Match('*'))
			{
				value *= ParseFactor();
			}
			else if (Match('/'))
			{
				float divisor = ParseFactor();
				value = divisor == 0 ? 0 : value / divisor;
			}
			else
			{
				return value;
			}
		}
	}

	float ParseFactor()
	{
		SkipWhitespace();
		if (Match('+'))
		{
			return ParseFactor();
		}
		if (Match('-'))
		{
			return -ParseFactor();
		}
		if (Match('('))
		{
			float value = ParseExpression();
			Match(')');
			return value;
		}
		if (char.IsLetter(Peek()))
		{
			string name = ParseName();
			SkipWhitespace();
			if (Match('('))
			{
				float value = ParseExpression();
				Match(')');
				return ApplyFunction(name, value);
			}
			return variables.TryGetValue(name, out float variableValue) ? variableValue : 0f;
		}

		return ParseNumber();
	}

	float ApplyFunction(string name, float value)
	{
		if (name == "sin")
		{
			return (float)Math.Sin(value * Math.PI / 180f);
		}
		if (name == "cos")
		{
			return (float)Math.Cos(value * Math.PI / 180f);
		}
		return value;
	}

	string ParseName()
	{
		int start = index;
		while (char.IsLetterOrDigit(Peek()) || Peek() == '_')
		{
			index++;
		}
		return expression.Substring(start, index - start);
	}

	float ParseNumber()
	{
		int start = index;
		while (char.IsDigit(Peek()) || Peek() == '.')
		{
			index++;
		}
		if (start == index)
		{
			return 0f;
		}
		return float.TryParse(expression.Substring(start, index - start), out float value) ? value : 0f;
	}

	bool Match(char character)
	{
		if (Peek() != character)
		{
			return false;
		}
		index++;
		return true;
	}

	char Peek()
	{
		return index < expression.Length ? expression[index] : '\0';
	}

	void SkipWhitespace()
	{
		while (char.IsWhiteSpace(Peek()))
		{
			index++;
		}
	}
}
