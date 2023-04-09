using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ChroMapper_MoarLightIDs {

[Plugin("Extended LightIDs")]
public class Plugin {
	private PlatformDescriptor descriptor;
	
	[Init]
	private void Init() {
		LoadInitialMap.PlatformLoadedEvent += PlatformLoaded;
		LoadInitialMap.LevelLoadedEvent += AddIds;
	}
	
	public void PlatformLoaded(PlatformDescriptor descriptor) {
		this.descriptor = descriptor;
	}
	
	public void AddIds() {
		var max_ids = new Dictionary<int, int>();
		// Get max from Environment Enhancements
		
		foreach (var eh in BeatSaberSongContainer.Instance.Map.EnvironmentEnhancements) {
			if (eh.LightID is int LightID) {
				int type = (eh.Components["ILightWithId"].HasKey("type"))
					? eh.Components["ILightWithId"]["type"].AsInt
					: GuessType(eh);
				if (type < 0) {
					continue;
				}
				int old_max = 0;
				max_ids.TryGetValue(type, out old_max);
				max_ids[type] = System.Math.Max(old_max, LightID);
			}
		}
		
		// Get max from events
		foreach (var ev in BeatSaberSongContainer.Instance.Map.Events) {
			if (ev.CustomLightID != null) {
				int old_max = 0;
				max_ids.TryGetValue(ev.Type, out old_max);
				max_ids[ev.Type] = System.Math.Max(old_max, ev.CustomLightID.Max());
			}
		}
		
		// Add them all
		foreach (var ids in max_ids) {
			var lm = descriptor.LightingManagers[ids.Key];
			Debug.Log($"Adding type {ids.Key} from {lm.LightIDPlacementMapReverse.Count} to {ids.Value}");
			for (int i = lm.LightIDPlacementMapReverse.Count + 1; i <= ids.Value; ++i) {
				lm.LightIDPlacementMap.Add(i - 1, i);
				lm.LightIDPlacementMapReverse.Add(i, i - 1);
			}
		}
	}
	
	public int GuessType(Beatmap.Base.Customs.BaseEnvironmentEnhancement eh) {
		string platform = descriptor.gameObject.name.Replace("(Clone)", "");
		if (eh.ID.Contains("Color")) {
			return 0;
		}
		if (eh.ID.Contains("Ring") && platform != "Kaleidoscope") {
			return 1;
		}
		if (eh.ID.Contains("BaseL")) {
			return 2;
		}
		if (eh.ID.Contains("BaseR")) {
			return 3;
		}
		if (eh.ID.Contains("Front")) {
			return 4;
		}
		Debug.Log("Couldn't guess event type from '{eh.ID}' platform {platform}");
		return -1;
	}
	
	[Exit]
	private void Exit() {
		
	}
}

}
