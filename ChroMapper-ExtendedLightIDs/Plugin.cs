using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Beatmap.Base;
using Beatmap.Enums;

namespace ChroMapper_ExtendedLightIDs {

[Plugin("Extended LightIDs")]
public class Plugin {
	public IDManager manager;
	
	[Init]
	private void Init() {
		SceneManager.sceneLoaded += SceneLoaded;
		
		LoadInitialMap.PlatformLoadedEvent += (p) => manager.PlatformLoaded(p);
		LoadInitialMap.LevelLoadedEvent += () => manager.RefreshIDs();
		CMInputCallbackInstaller.InputInstance.EventGrid.ToggleLightPropagation.performed += (_) => manager.RefreshIDs();
		CMInputCallbackInstaller.InputInstance.EventGrid.CycleLightPropagationUp.performed += (_) => manager.RefreshIDs();
		CMInputCallbackInstaller.InputInstance.EventGrid.CycleLightPropagationDown.performed += (_) => manager.RefreshIDs();
		CMInputCallbackInstaller.InputInstance.EventGrid.ToggleLightIdMode.performed += (_) => manager.RefreshIDs();
		
		ExtensionButtons.AddButton(
			LoadSprite("ChroMapper_ExtendedLightIDs.Resources.Icon.png"),
			"Extend Light IDs",
			() => {});
	}
	
	private void SceneLoaded(Scene scene, LoadSceneMode mode) {
		if (scene.buildIndex == 3) {
			manager = new IDManager();
		}
	}
	
	[Exit]
	private void Exit() {
		
	}
	
	public static Sprite LoadSprite(string asset) {
		Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(asset);
		byte[] data = new byte[stream.Length];
		stream.Read(data, 0, (int)stream.Length);
		
		Texture2D texture2D = new Texture2D(256, 256);
		texture2D.LoadImage(data);
		
		return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0, 0), 100.0f);
	}
}

}
