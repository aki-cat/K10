using UnityEngine;

[System.Serializable]
public class FloatState : INumericValueState<float>, ICustomDisposableKill
{
	[SerializeField] float _value;
	[System.NonSerialized] private EventSlot<float> _onChange;

	public float Value { get { return _value; } set { Setter( value ); } }
	public float Get() { return _value; }

	public void SetInt( int value ) { Setter( value ); }

	public void Setter( float value )
	{
		if( Mathf.Approximately( _value, value ) ) return;
		_value = value;
		_onChange?.Trigger( value );
	}

	public void Increment( float increment )
	{
		if( Mathf.Approximately( 0, increment ) ) return;
		Setter( _value + increment );
	}

	public void Kill()
	{
		_onChange?.Kill();
		_onChange = null;
	}

	public IEventRegister<float> OnChange => Lazy.Request( ref _onChange );

	public FloatState() : this( default( float ) ) { }
	public FloatState( float initialValue ) { _value = initialValue; }


	public override string ToString() { return $"FS({_value})"; }
}