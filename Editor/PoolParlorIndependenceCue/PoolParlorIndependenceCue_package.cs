using System;
using UnityEngine;
using UnityEditor;

namespace EijisPoolParlorTableUtil
{
	public class IndependenceCuePackage
	{
		private static readonly string exportPackageFilePath = "PoolParlorIndependenceCue_20250527.unityPackage";
		static readonly string[] exportFilePaths = 
		{
			"Assets/eijis/Materials/PoolParlorIndependenceCue/ControllerDescription.mat",
			"Assets/eijis/Materials/PoolParlorIndependenceCue/ControllerDescription_LG.mat",
			"Assets/eijis/Materials/PoolParlorIndependenceCue/ControllerDescription_LT.mat",
			"Assets/eijis/Materials/PoolParlorIndependenceCue/ControllerDescription_RG.mat",
			"Assets/eijis/Materials/PoolParlorIndependenceCue/ControllerDescription_RT.mat",
			"Assets/eijis/Materials/PoolParlorIndependenceCue/Invisible.mat",
			"Assets/eijis/Materials/PoolParlorIndependenceCue/TriggerMarker_Red.mat",
			"Assets/eijis/Materials/PoolParlorIndependenceCue/TriggerMarker_Yellow.mat",
			"Assets/eijis/Prefab/PoolParlorIndependenceCue/Cue Reset.prefab",
			"Assets/eijis/Prefab/PoolParlorIndependenceCue/Description Panel Toggle.prefab",
			"Assets/eijis/Prefab/PoolParlorIndependenceCue/IndependenceCue.prefab",
			"Assets/eijis/Prefab/PoolParlorIndependenceCue/IndependenceCue_3Set.prefab",
			"Assets/eijis/Prefab/PoolParlorIndependenceCue/IndependenceCue_6Set.prefab",
			"Assets/eijis/Prefab/PoolParlorIndependenceCue/IndependenceCue_6Set_ResetButton.prefab",
			"Assets/eijis/Prefab/PoolParlorIndependenceCue/IndependenceCue_6Set_ResetButton_TogglePanel.prefab",
			"Assets/eijis/Textures/PoolParlorIndependenceCue/controller_description.png",
			"Assets/eijis/Textures/PoolParlorIndependenceCue/controller_description_LG.png",
			"Assets/eijis/Textures/PoolParlorIndependenceCue/controller_description_LT.png",
			"Assets/eijis/Textures/PoolParlorIndependenceCue/controller_description_RG.png",
			"Assets/eijis/Textures/PoolParlorIndependenceCue/controller_description_RT.png",
			"Assets/eijis/UdonScripts/PoolParlorIndependenceCue/IndependenceCueController.asset",
			"Assets/eijis/UdonScripts/PoolParlorIndependenceCue/IndependenceCueController.cs",
			"Assets/eijis/UdonScripts/PoolParlorIndependenceCue/IndependenceCueGrip.asset",
			"Assets/eijis/UdonScripts/PoolParlorIndependenceCue/IndependenceCueGrip.cs"
		};
		
		[MenuItem("GameObject/TKCH/PoolParlor/IndependenceCueExportPackage_20250527", false, 0)]
		private static void ExportPackage_Menu(MenuCommand command)
		{
			try
			{
				Debug.Log("ExportPackage");

				AssetDatabase.ExportPackage(exportFilePaths, exportPackageFilePath, ExportPackageOptions.Default);

				EditorUtility.DisplayDialog ("Custom Script Result", "ExportPackage end", "OK");
			}
			catch (Exception ex)
			{
				EditorUtility.DisplayDialog ("Custom Script Exception", ex.ToString(), "OK");
			}
		}
	}
}
