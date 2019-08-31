using UnityEngine;

[System.Serializable]
public class LongState : IValueState<long>
{
    [SerializeField] long _value;
    [System.NonSerialized] EventSlot<long> _onChange = new EventSlot<long>();

    public long Value { get { return _value; } set { Setter( value ); } }
    public long Get() { return _value; }

    public void Setter( long value )
    {
        if( _value == value ) return;
        _value = value;

        if (_onChange == null) _onChange = new EventSlot<long>();

        _onChange.Trigger( value );
    }

    public void Increment( long value = 1 )
    {
        Setter( _value + value );
    }

    public IEventRegister<long> OnChange { get { if(_onChange == null) _onChange = new EventSlot<long>(); return _onChange; } }

    public LongState( long initialValue = default( long) ) { _value = initialValue; }

    public override string ToString() { return string.Format( "LS({1})", typeof( long ).ToString(), _value ); }
}