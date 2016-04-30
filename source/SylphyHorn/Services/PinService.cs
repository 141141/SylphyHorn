﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MetroRadiance.Chrome;
using MetroRadiance.Platform;
using SylphyHorn.Views;
using VDMHelperCLR.Common;
using WindowsDesktop;

namespace SylphyHorn.Services
{
	public class PinService : IDisposable
	{
		private readonly object _sync = new object();
		private readonly Dictionary<IntPtr, PinnedWindow> _pinnedWindows = new Dictionary<IntPtr, PinnedWindow>();
		private readonly IVdmHelper _helper;

		public PinService(IVdmHelper helper)
		{
			this._helper = helper;
			VirtualDesktop.CurrentChanged += this.VirtualDesktopOnCurrentChanged;
		}

		public void Register(IntPtr hWnd)
		{
			lock (this._sync)
			{
				if (!this._pinnedWindows.ContainsKey(hWnd))
				{
					this.RegisterCore(hWnd);
				}
			}
		}

		public void Unregister(IntPtr hWnd)
		{
			lock (this._sync)
			{
				if (this._pinnedWindows.ContainsKey(hWnd))
				{
					this.UnregisterCore(hWnd);
				}
			}
		}

		public void UnregisterAll()
		{
			IntPtr[] targets;
			lock (this._sync)
			{
				targets = this._pinnedWindows.Keys.ToArray();
			}

			foreach (var hWnd in targets)
			{
				this.Unregister(hWnd);
			}
		}

		public void ToggleRegister(IntPtr hWnd)
		{
			lock (this._sync)
			{
				if (this._pinnedWindows.ContainsKey(hWnd))
				{
					this.UnregisterCore(hWnd);
				}
				else
				{
					this.RegisterCore(hWnd);
				}
			}
		}

		private void RegisterCore(IntPtr hWnd)
		{
			var external = new ExternalWindow(hWnd);
			var chrome = new WindowChrome
			{
				BorderThickness = new Thickness(4.0),
				Top = new PinMarker(),
			};
			chrome.Attach(external);
			external.Closed += (sender, e) => this.Unregister(hWnd);

			this._pinnedWindows[hWnd] = new PinnedWindow(external, chrome);
		}

		private void UnregisterCore(IntPtr hWnd)
		{
			this._pinnedWindows[hWnd].Dispose();
			this._pinnedWindows.Remove(hWnd);
		}

		private void VirtualDesktopOnCurrentChanged(object sender, VirtualDesktopChangedEventArgs e)
		{
			IntPtr[] targets;
			lock (this._sync)
			{
				targets = this._pinnedWindows.Keys.ToArray();
			}

			VisualHelper.InvokeOnUIDispatcher(() =>
			{
				foreach (var hWnd in targets.Where(x => !VirtualDesktopHelper.MoveToDesktop(x, e.NewDesktop)))
				{
					this._helper.MoveWindowToDesktop(hWnd, e.NewDesktop.Id);
				}
			});
		}

		public void Dispose()
		{
			VirtualDesktop.CurrentChanged -= this.VirtualDesktopOnCurrentChanged;
			this.UnregisterAll();
		}

		private struct PinnedWindow : IDisposable
		{
			private readonly ExternalWindow _window;
			private readonly WindowChrome _chrome;

			public PinnedWindow(ExternalWindow window, WindowChrome chrome)
			{
				this._window = window;
				this._chrome = chrome;
			}

			public void Dispose()
			{
				this._chrome.Close();
				this._window.Dispose();
			}
		}
	}
}
