using System;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using SylphyHorn.Annotations;
using SylphyHorn.Interop;
using Windows.ApplicationModel;

namespace SylphyHorn
{
	[Dark("#if directive is evil...")]
	public class Startup
	{
		[UsedImplicitly]
		private readonly string _path;

		[UsedImplicitly]
		private readonly StartupTask _task;

		public bool IsExists
		{
			get
			{
#if APPX
				return this._task.State == StartupTaskState.Enabled || this._task.State == StartupTaskState.DisabledByUser;
#else
				return File.Exists(this._path);
#endif
			}
		}

		public Startup()
			: this(GetExecutingAssemblyFileNameWithoutExtension())
		{
		}

		public Startup(string name)
		{
#if APPX
			this._task = GetStartupTask();
#endif
			this._path = GetStartupFilePath(name);
		}

		public void Create()
		{
#if APPX
			if (this._task.State == StartupTaskState.Disabled)
			{
				this._task.RequestEnableAsync().ToTask().Wait();
			}
#else
			MetroTrilithon.Desktop.ShellLink.Create(this._path);
#endif
		}

		public void Remove()
		{
#if APPX
			if (this._task.State == StartupTaskState.Enabled || this._task.State == StartupTaskState.DisabledByUser)
			{
				this._task.Disable();
			}
#else
			if (this.IsExists)
			{
				File.Delete(this._path);
			}
#endif
		}

		private static string GetStartupFilePath(string name)
		{
			var dir = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
			return Path.Combine(dir, name + ".lnk");
		}

		private static string GetExecutingAssemblyFileNameWithoutExtension()
		{
			return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
		}

		[UsedImplicitly]
		private static StartupTask GetStartupTask()
		{
			return StartupTask.GetAsync("SylphyHornEngineStartupTask").ToTask().Result;
		}
	}
}
