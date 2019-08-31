using UnityEngine;

[System.Serializable]
public class FloatState : IValueState<float>
{
	[SerializeField] float _value;
	[System.NonSerialized] EventSlot<float> _onChange = new EventSlot<float>();

	public float Value { get { return _value; } set { Setter( value ); } }
	public float Get() { return _value; }

	public void Setter( float value )
	{
		if( Mathf.Approximately( _value, value ) ) return;
		_value = value;
		_onChange.Trigger( value );
	}

	public void Sum( float value )
	{
		if( Mathf.Approximately( 0, value ) ) return;
		_value += value;
		_onChange.Trigger( _value );
	}

	public IEventRegister<float> OnChange { get { return _onChange; } }

	public FloatState( float initialValue = default( float ) ) { _value = initialValue; }

	public override string ToString() { return string.Format( "FS({1})", typeof( float ).ToString(), _value ); }

	public void SetInt( int value ) { Setter( value ); }
}