#if UNITY_STANDALONE_WIN
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class WindowsBuild : IPostprocessBuildWithReport
{
	public int callbackOrder { get { return 0; } }

	public void OnPostprocessBuild(BuildReport report)
	{
		var path = report.summary.outputPath.Trim();
		
		if(path.Contains(".exe"))
		{
			while(path[path.Length - 1] != '/')
			{
				path = path.Remove(path.Length - 1);
			}

			

			FileUtil.CopyFileOrDirectory("Assets/win_res", path + "win_res");
		}
		else
			FileUtil.CopyFileOrDirectory("Assets/win_res", path + "/win_res");
	}
}
#endif