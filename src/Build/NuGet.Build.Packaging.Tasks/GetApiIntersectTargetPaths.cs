﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet.Frameworks;

namespace NuGet.Build.Packaging.Tasks
{
	public class GetApiIntersectTargetPaths : Task
	{
		[Required]
		public ITaskItem[] Frameworks { get; set; }

		[Required]
		public ITaskItem[] Assemblies { get; set; }

		[Required]
		public ITaskItem RootOutputDirectory { get; set; }

		[Output]
		public ITaskItem[] TargetPaths { get; set; }

		public override bool Execute()
		{
			string assemblyFileName = GetAssemblyFileName();

			TargetPaths = Frameworks.NullAsEmpty()
				.Select(framework => GetTargetPath(framework, assemblyFileName))
				.ToArray();

			return true;
		}

		string GetAssemblyFileName()
		{
			return Assemblies.NullAsEmpty()
				.Select(assembly => Path.GetFileName(assembly.ItemSpec))
				.FirstOrDefault();
		}

		private ITaskItem GetTargetPath(ITaskItem framework, string assemblyFileName)
		{
			var nugetFramework = NuGetFramework.Parse(framework.ItemSpec);
			string outputDirectory = GetOutputDirectory(RootOutputDirectory.ItemSpec, nugetFramework);

			string outputPath = Path.Combine(outputDirectory, "bin", assemblyFileName);

			var item = new TaskItem(outputPath);
			item.SetMetadata(MetadataName.TargetFrameworkMoniker, framework.ItemSpec);

			return item;
		}

		internal static string GetOutputDirectory(string rootDirectory, NuGetFramework framework)
		{
			return Path.Combine(rootDirectory, GetOutputDirectoryName(framework));
		}

		static string GetOutputDirectoryName(NuGetFramework framework)
		{
			if (framework.IsPCL && framework.HasProfile)
			{
				return framework.Profile;
			}

			return framework.GetShortFolderName();
		}
	}
}
