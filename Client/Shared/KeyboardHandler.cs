using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;

namespace VeloTiming.Client.Shared
{
	public static class KeyboardHandler
	{
		public static event EventHandler<KeyboardEvent>? OnDocumentKeyDown;

		[JSInvokable]
		public static void DocumentKeyDown(KeyboardEvent args)
		{
			OnDocumentKeyDown?.Invoke(null, args);
		}
	}

	public class KeyboardEvent
	{
		public string? Key { get; set; }
	}
}
