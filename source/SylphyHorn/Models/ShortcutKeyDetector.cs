﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Open.WinKeyboardHook;

namespace SylphyHorn.Models
{
	/// <summary>
	/// Provides the function to detect a shortcut key ([modifier key(s)] + [key] style) by use of global key hook.
	/// </summary>
	public class ShortcutKeyDetector
	{
		private readonly IKeyboardInterceptor interceptor = new KeyboardInterceptor();
		private readonly HashSet<Key> pressedModifiers = new HashSet<Key>();

		/// <summary>
		/// Occurs when detects a shortcut key.
		/// </summary>
		public event EventHandler<ShortcutKey> Pressed;

		public ShortcutKeyDetector()
		{
			this.interceptor.KeyDown += this.InterceptorOnKeyDown;
			this.interceptor.KeyUp += this.InterceptorOnKeyUp;
		}

		public void Start()
		{
			this.interceptor.StartCapturing();
		}

		public void Stop()
		{
			this.interceptor.StopCapturing();
		}

		private void InterceptorOnKeyDown(object sender, System.Windows.Forms.KeyEventArgs args)
		{
			var key = KeyInterop.KeyFromVirtualKey((int)args.KeyCode);
			if (key.IsModifyKey())
			{
				this.pressedModifiers.Add(key);
			}
			else
			{
				this.Pressed?.Invoke(this, new ShortcutKey(key, this.pressedModifiers));
			}
		}

		private void InterceptorOnKeyUp(object sender, System.Windows.Forms.KeyEventArgs args)
		{
			if (this.pressedModifiers.Count == 0) return;

			var key = KeyInterop.KeyFromVirtualKey((int)args.KeyCode);
			if (key.IsModifyKey())
			{
				this.pressedModifiers.Remove(key);
			}
		}
	}
}
