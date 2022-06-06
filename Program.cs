using System;
using System.Collections.Generic;

public class Program
{
	public static void Main()
	{
		// Step 1 - register listener
		IDisposable disposable = EventRelayer.listen<FirstEvent>((e) => Console.WriteLine("Hello " + e._text));
		
		// Step 2 - create event to send
		Event first = new FirstEvent("first");
		Event second = new SecondEvent("second");
		
		// Step 3 - send event
		EventRelayer.send(first);
		EventRelayer.send(second);
		
		// Step 4 - dispose to unregister listener
		disposable.Dispose();
		
		/*
		only outputs:
		Hello first
		*/
	}

	public interface Event
	{
	}

	public class FirstEvent : Event
	{
		public string _text;
		public FirstEvent(string text)
		{
			_text = text;
		}
	}

	public class SecondEvent : Event
	{
		public string _text;
		public SecondEvent(string text)
		{
			_text = text;
		}
	}

	public static class EventRelayer
	{
		static ICollection<Action<Event>> EMPTY = new List<Action<Event>>();
		static Dictionary<Type, ICollection<Action<Event>>> _eventConsumers = new Dictionary<Type, ICollection<Action<Event>>>();
		public static void send(Event tevent)
		{
			ICollection<Action<Event>> consumers = EMPTY;
			if (_eventConsumers.TryGetValue(tevent.GetType(), out consumers))
			{
				foreach (var consumer in consumers)
				{
					consumer.Invoke(tevent);
				}
			}
		}

		public static IDisposable listen<TEvent>(Action<TEvent> consumer)
			where TEvent : Event
		{
			ICollection<Action<Event>> actionCollection;
			Type type = typeof(TEvent);
			if (_eventConsumers.ContainsKey(type))
			{
				actionCollection = _eventConsumers[type];
			}
			else
			{
				actionCollection = new List<Action<Event>>();
				_eventConsumers[type] = actionCollection;
			}
			
			Action<Event> wrapped = (e) => consumer.Invoke((TEvent)e);
			actionCollection.Add(wrapped);
			return new ConsumerRemover(() => actionCollection.Remove(wrapped));
		}
		
		public class ConsumerRemover : IDisposable
		{
			Action _disposer;
			public ConsumerRemover(Action disposer) {
				_disposer = disposer;
			}
			
			public void Dispose()
			{
				_disposer.Invoke();
			}
		}
	}
}
