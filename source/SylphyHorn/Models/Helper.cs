﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Livet;
using MetroRadiance;
using Microsoft.Win32;
using SylphyHorn.Interop;

namespace SylphyHorn.Models
{
	public static class KeyHelper
	{
		public static bool IsModifyKey(this Key key)
		{
			// System.Windows.Forms.KeyEventArgs.Modifiers が LWin と RWin を含めてくれないので…
			// あと Left と Right 区別できたらいいなって…
			// _:(´ཀ`」 ∠):_

			return key == Key.LeftAlt
				   || key == Key.LeftCtrl
				   || key == Key.LeftShift
				   || key == Key.LWin
				   || key == Key.RightAlt
				   || key == Key.RightCtrl
				   || key == Key.RightShift
				   || key == Key.RWin;
		}
	}

	internal static class InteropHelper
	{
		private const string consoleWindowClass = "ConsoleWindowClass";

		public static bool IsCurrentProcess(IntPtr hWnd)
		{
			return System.Windows.Application.Current.Windows
				.OfType<Window>()
				.Select(x => PresentationSource.FromVisual(x) as HwndSource)
				.Where(x => x != null)
				.Select(x => x.Handle)
				.Any(x => x == hWnd);
		}

		public static bool IsConsoleWindow(IntPtr hWnd)
		{
			var className = new StringBuilder(256);
			NativeMethods.GetClassName(hWnd, className, className.Capacity);
			return className.ToString() == consoleWindowClass;
		}

		public static IntPtr GetForegroundWindowEx()
		{
			var hwnd = NativeMethods.GetForegroundWindow();
			var howner = NativeMethods.GetWindow(hwnd, 4 /* GW_OWNER */);
			return howner == IntPtr.Zero ? hwnd : howner;
		}
	}

	internal static class VisualHelper
	{
		public static void InvokeOnUIDispatcher(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
		{
			DispatcherHelper.UIDispatcher.BeginInvoke(action, priority);
		}

		public static bool IsDarkTheme()
		{
			const string keyName = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize";
			const string valueName = "AppsUseLightTheme";

			var value = Registry.GetValue(keyName, valueName, null);
			return value as int? == 0;
		}

		public static int GetWindowsAccentColor()
		{
			int color;
			bool opaque;

			NativeMethods.DwmGetColorizationColor(out color, out opaque);

			return color;
		}

		public static void ForceChangeTheme(long color)
		{
			ForceChangeTheme(Color.FromArgb((byte)(color >> 24), (byte)(color >> 16), (byte)(color >> 8), (byte)color));
		}

		public static void ForceChangeTheme(Color color)
		{
			color.A = byte.MaxValue;

			var appAcent = typeof(ThemeService).GetField("appAccent", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(ThemeService.Current) as ResourceDictionary;
			if (appAcent != null)
			{
				appAcent["AccentColorKey"] = color;
				appAcent["AccentBrushKey"] = new SolidColorBrush(color);
				appAcent["AccentActiveColorKey"] = color;
				appAcent["AccentActiveBrushKey"] = new SolidColorBrush(color);
				appAcent["AccentHighlightColorKey"] = color;
				appAcent["AccentHighlightBrushKey"] = new SolidColorBrush(color);
			}
		}
	}


	internal static class ShellLinkHelper
	{
		private static string GetExecutingAssemblyFileNameWithoutExtension()
		{
			return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
		}

		private static string GetStartupFileName(string name)
		{
			var dir = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
			return Path.Combine(dir, name + ".lnk");
		}

		public static bool CreateLink(string path)
		{
			var type = Type.GetTypeFromCLSID(new Guid("00021401-0000-0000-C000-000000000046"));
			var psl = (IShellLink)Activator.CreateInstance(type);
			psl.SetPath(Assembly.GetExecutingAssembly().Location);
			var ppf = (IPersistFile)psl;
			ppf.Save(path, false);
			return true;
		}

		public static bool CreateStartup(string name)
		{
			return CreateLink(GetStartupFileName(name));
		}

		public static bool CreateStartup()
		{
			return CreateStartup(GetExecutingAssemblyFileNameWithoutExtension());
		}

		public static bool RemoveStartup(string name)
		{
			if (!ExistsStartup(name)) return false;
			File.Delete(GetStartupFileName(name));
			return true;
		}

		public static bool RemoveStartup()
		{
			return RemoveStartup(GetExecutingAssemblyFileNameWithoutExtension());
		}

		public static bool ExistsStartup(string name)
		{
			return File.Exists(GetStartupFileName(name));
		}

		public static bool ExistsStartup()
		{
			return ExistsStartup(GetExecutingAssemblyFileNameWithoutExtension());
		}
	}
}
