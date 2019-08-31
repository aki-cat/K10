﻿using System;
using System.Linq;
using System.Collections.Generic;

public interface ISemaphoreInfo : IBoolStateObserver
{
	bool Free { get; }

	IEventRegister OnBlock { get; }
	IEventRegister OnRelease { get; }
	IEventRegister<bool> OnStateChange { get; }
}

public interface ISemaphoreInterection
{
	void Block( object obj );
	bool BlockButDoNotIncrease( object obj );
	void Release( object obj );
}

public static class ISemaphoreInterectionExtentions
{
	public static void BlockOn( this ISemaphoreInterection semaphore, IValueStateObserver<bool> source ) { source.Synchronize( ( value ) => { if( value ) semaphore.Block( source ); else semaphore.Release( source ); } ); }
	public static void ReleaseOn( this ISemaphoreInterection semaphore, IValueStateObserver<bool> source ) { source.Synchronize( ( value ) => { if( value ) semaphore.Release( source ); else semaphore.Block( source ); } ); }
	public static void BlockOn( this ISemaphoreInterection semaphore, IValueStateObserver<bool> source, Func<bool> eventValidation ) { source.Synchronize( new ConditionalEventListener<bool>( ( value ) => { if( value ) semaphore.Block( source ); else semaphore.Release( source ); }, eventValidation ) ); }
	public static void ReleaseOn( this ISemaphoreInterection semaphore, IValueStateObserver<bool> source, Func<bool> eventValidation ) { source.Synchronize( new ConditionalEventListener<bool>( ( value ) => { if( value ) semaphore.Release( source ); else semaphore.Block( source ); }, eventValidation ) ); }

	public static void BlockOn( this ISemaphoreInterection semaphore, IBoolStateObserver source )
	{
		source.RegisterOnTrue( () => semaphore.Block( source ) );
		source.RegisterOnFalse( () => semaphore.Release( source ) );
	}

	public static void BlockOn( this ISemaphoreInterection semaphore, IBoolStateObserver source, ConditionalEventsCollection eventValidation )
	{
		source.RegisterOnTrue( new ConditionalEventListener( () => semaphore.Block( source ), eventValidation.CurrentValidationCheck ) );
		source.RegisterOnFalse( new ConditionalEventListener( () => semaphore.Release( source ), eventValidation.CurrentValidationCheck ) );
	}

	public static void BlockOn( this ISemaphoreInterection semaphore, IBoolStateObserver source, Func<bool> eventValidation )
	{
		source.RegisterOnTrue( new ConditionalEventListener( () => semaphore.Block( source ), eventValidation ) );
		source.RegisterOnFalse( new ConditionalEventListener( () => semaphore.Release( source ), eventValidation ) );
	}

	public static void ReleaseOn( this ISemaphoreInterection semaphore, IBoolStateObserver source )
	{
		source.RegisterOnTrue( () => semaphore.Release( source ) );
		source.RegisterOnFalse( () => semaphore.Block( source ) );
	}

	public static void ReleaseOn( this ISemaphoreInterection semaphore, IBoolStateObserver source, Func<bool> eventValidation )
	{
		source.RegisterOnTrue( new ConditionalEventListener( () => semaphore.Release( source ), eventValidation ) );
		source.RegisterOnFalse( new ConditionalEventListener( () => semaphore.Block( source ), eventValidation ) );
	}

	public static void ReleaseOn( this ISemaphoreInterection semaphore, IBoolStateObserver source, ConditionalEventsCollection eventValidation )
	{
		source.RegisterOnTrue( new ConditionalEventListener( () => semaphore.Release( source ), eventValidation.CurrentValidationCheck ) );
		source.RegisterOnFalse( new ConditionalEventListener( () => semaphore.Block( source ), eventValidation.CurrentValidationCheck ) );
	}
}

public interface ISemaphore : ISemaphoreInfo, ISemaphoreInterection { }

public class Semaphore : ISemaphore
{
	public class SemaphoreObject { public int Value { get; set; } }

	private readonly Dictionary<object, SemaphoreObject> _semaphores = new Dictionary<object, SemaphoreObject>();
	public bool Free { get { return _semaphores.Count == 0; } }

	private readonly EventSlot _blockEvent = new EventSlot();
	private readonly EventSlot _releaseEvent = new EventSlot();
	private readonly EventSlot<bool> _changeStateEvent = new EventSlot<bool>();

	public IEventRegister OnBlock { get { return _blockEvent; } }
	public IEventRegister OnRelease { get { return _releaseEvent; } }
	public IEventRegister<bool> OnStateChange { get { return _changeStateEvent; } }

	public bool Value { get { return Free; } }
	public bool Get() { return Free; }
	public IEventRegister<bool> OnChange { get { return _changeStateEvent; } }
	public IEventRegister OnTrueState { get { return _releaseEvent; } }
	public IEventRegister OnFalseState { get { return _blockEvent; } }

	public void RegisterAndStart( IEventTrigger<bool> evnt ) { _changeStateEvent.Register( evnt ); evnt.Trigger( Free ); }
	public void RegisterAndStart( System.Action<bool> evnt ) { _changeStateEvent.Register( evnt ); evnt( Free ); }

	public IDictionary<object, SemaphoreObject> Semaphores { get { return _semaphores; } }

	public void BlockOn( IBoolStateObserver source )
	{
		source.RegisterOnTrue( () => Block( source ) );
		source.RegisterOnFalse( () => Release( source ) );
	}

	public void BlockOn( IBoolStateObserver source, Func<bool> eventValidation )
	{
		source.RegisterOnTrue( new ConditionalEventListener( () => Block( source ), eventValidation ) );
		source.RegisterOnFalse( new ConditionalEventListener( () => Release( source ), eventValidation ) );
	}

	public void ReleaseOn( IBoolStateObserver source )
	{
		source.RegisterOnTrue( () => Release( source ) );
		source.RegisterOnFalse( () => Block( source ) );
	}

	public void ReleaseOn( IBoolStateObserver source, Func<bool> eventValidation )
	{
		source.RegisterOnTrue( new ConditionalEventListener( () => Release( source ), eventValidation ) );
		source.RegisterOnFalse( new ConditionalEventListener( () => Block( source ), eventValidation ) );
	}

	internal void Toggle( object obj )
	{
		if( _semaphores.ContainsKey( obj ) ) Release( obj );
		else Block( obj );
	}

	public void Block( object obj )
	{
		if( obj == null )
			return;

		bool trigger = Free;

		SemaphoreObject s;
		if( !_semaphores.TryGetValue( obj, out s ) )
		{
			s = new SemaphoreObject();
			_semaphores.Add( obj, s );
		}

		s.Value++;

		if( trigger )
		{
			_blockEvent.Trigger();
			_changeStateEvent.Trigger( false );
		}
	}

	public bool BlockButDoNotIncrease( object obj )
	{
		if( obj == null )
			return false;

		bool trigger = Free;

		SemaphoreObject s;
		bool newKey = !_semaphores.TryGetValue( obj, out s );
		if( newKey )
		{
			s = new SemaphoreObject();
			_semaphores.Add( obj, s );
			s.Value++;
		}

		if( trigger )
		{
			_blockEvent.Trigger();
			_changeStateEvent.Trigger( false );
		}

		return newKey;
	}

	public void Release( object obj )
	{
		if( Free || obj == null )
			return;

		SemaphoreObject s;
		if( _semaphores.TryGetValue( obj, out s ) )
		{
			s.Value--;
			if( s.Value == 0 )
				_semaphores.Remove( obj );
		}

		if( Free )
		{
			_releaseEvent.Trigger();
			_changeStateEvent.Trigger( true );
		}
	}

	public void Clear()
	{
		var initialState = Free;
		_semaphores.Clear();
		var free = Free;
		if( free != initialState )
		{
			if( free ) _releaseEvent.Trigger();
			else _blockEvent.Trigger();
			_changeStateEvent.Trigger( free );
		}
	}

	string KeyName( object obj )
	{
		if( obj == this ) return "THIS";
		if( obj is string ) return obj.ToStringOrNull();
		return obj.GetType().ToStringOrNull();
		// return obj.ToStringOrNull();
	}

	public override string ToString()
	{
		var elementsStrings = _semaphores.ToList().ConvertAll( ( obj ) => $"{KeyName( obj.Key )}({obj.Value.Value})" );
		var elements = String.Join( ", ", elementsStrings );
		return $"( [{( Free ? "Free" : "Blocked" )} Semaphore => {{ {elements} }}] )";
	}
}