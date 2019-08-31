using UnityEngine;
using static UnityEngine.Mathf;

public interface IRangedFloatState : IValueState<float>
{
	IValueState<float> Min { get; }
	IValueState<float> Max { get; }
}

public interface IRangedFloatStateObserver : IValueStateObserver<float>
{
	IValueStateObserver<float> Min { get; }
	IValueStateObserver<float> Max { get; }
}

[System.Serializable]
public class RangedFloatState : IRangedFloatState, IRangedFloatStateObserver
{
	[SerializeField] protected readonly FloatState _value;
	[SerializeField] protected readonly FloatState _min;
	[SerializeField] protected readonly FloatState _max;

	public float Value => _value.Value;
	public IEventRegister<float> OnChange => _value.OnChange;

	public float Get() => _value.Get();

	public void Setter( float t )
	{
		var val = Clamp( t, _min.Value, _max.Value );
		_value.Setter( val );
	}

	void CheckValueRange() { Setter( _value.Value ); }

	public IValueState<float> Min => _min;
	public IValueState<float> Max => _max;

	IValueStateObserver<float> IRangedFloatStateObserver.Min => _min;
	IValueStateObserver<float> IRangedFloatStateObserver.Max => _max;

	public RangedFloatState( float initialValue, float maxValue ) : this( initialValue, 0, maxValue ) { }
	public RangedFloatState( float initialValue, float minValue, float maxValue )
	{
		_max = new FloatState( maxValue );
		_min = new FloatState( minValue );
		_value = new FloatState( Clamp( initialValue, _min.Value, _max.Value ) );

		_min.OnChange.Register( CheckValueRange );
		_max.OnChange.Register( CheckValueRange );
	}
}
