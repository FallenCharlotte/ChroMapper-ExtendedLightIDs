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
		CMInputCallbackInstaller.InputInstance.EventGrid.ToggleLightPropagation.performed += (_) => manager?.RefreshIDs();
		CMInputCallbackInstaller.InputInstance.EventGrid.CycleLightPropagationUp.performed += (_) => manager?.RefreshIDs();
		CMInputCallbackInstaller.InputInstance.EventGrid.CycleLightPropagationDown.performed += (_) => manager?.RefreshIDs();
		CMInputCallbackInstaller.InputInstance.EventGrid.ToggleLightIdMode.performed += (_) => manager?.RefreshIDs();
		
		ExtensionButtons.AddButton(
			LoadSprite("ChroMapper_ExtendedLightIDs.Resources.Icon.png"),
			"Extend Light IDs",
			ShowDialog);
	}
	
	private void SceneLoaded(Scene scene, LoadSceneMode mode) {
		if (scene.buildIndex == 3) {
			manager = new IDManager();
		}
		else {
			manager = null;
		}
	}
	
	private void ShowDialog() {
		queued_type = ((EventGridContainer)BeatmapObjectContainerCollection.GetCollectionForType(Beatmap.Enums.ObjectType.Event))
			.EventTypeToPropagate;
		var dialog = PersistentUI.Instance.CreateNewDialogBox().WithTitle("Add LightID Lanes");
		dialog.AddComponent<TextBoxComponent>()
			.WithLabel("Type")
			.WithInitialValue($"{queued_type}")
			.OnChanged((string s) => {
				int.TryParse(s, out queued_type);
			});
		dialog.AddComponent<TextBoxComponent>()
			.WithLabel("Beginning ID")
			.OnChanged((string s) => {
				int.TryParse(s, out queued_begin);
			});
		dialog.AddComponent<TextBoxComponent>()
			.WithLabel("Ending ID")
			.OnChanged((string s) => {
				int.TryParse(s, out queued_end);
			});
		dialog.AddFooterButton(null, "Cancel");
		dialog.AddFooterButton(() => {
			manager.AddIDs(queued_type, queued_begin, queued_end);
			queued_begin = -1;
			queued_end = -1;
		}, "Add");
		dialog.Open();
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
	
	private int queued_type = -1;
	private int queued_begin = -1;
	private int queued_end = -1;
}

}
