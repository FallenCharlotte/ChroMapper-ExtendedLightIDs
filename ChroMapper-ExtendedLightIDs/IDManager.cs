using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Beatmap.Base;
using Beatmap.Enums;

namespace ChroMapper_ExtendedLightIDs {

public class IDManager {
	private PlatformDescriptor descriptor;
	private Dictionary<int, SortedSet<int>> ids = new Dictionary<int, SortedSet<int>>();
	
	public void PlatformLoaded(PlatformDescriptor descriptor) {
		this.descriptor = descriptor;
	}
	
	public SortedSet<int> IDsForType(int type) {
		if (!ids.ContainsKey(type)) {
			var lm = descriptor.LightingManagers[type];
			ids.Add(type, new SortedSet<int>(lm.LightIDPlacementMap.Values));
		}
		return ids[type];
	}
	
	public void RefreshIDs() {
		// Get IDs from Environment Enhancements, V3 only
		foreach (var eh in BeatSaberSongContainer.Instance.Map.EnvironmentEnhancements) {
			if (eh.LightID is int id) {
				int type = (eh.Components["ILightWithId"].HasKey("type"))
					? eh.Components["ILightWithId"]["type"].AsInt
					: -1;
				if (type < 0) {
					continue;
				}
				IDsForType(type).Add(id);
			}
		}
		
		// Get IDs from events
		foreach (var o in BeatmapObjectContainerCollection.GetCollectionForType(Beatmap.Enums.ObjectType.Event).UnsortedObjects) {
			var ev = (BaseEvent)o;
			if (ev.CustomLightID != null) {
				IDsForType(ev.Type).UnionWith(ev.CustomLightID);
				Debug.Log(string.Join(",", IDsForType(ev.Type).ToList()));
			}
		}
		
		// Add them all
		foreach (var type in ids) {
			var lm = descriptor.LightingManagers[type.Key];
			
			var i = 0;
			lm.LightIDPlacementMap.Clear();
			lm.LightIDPlacementMapReverse.Clear();
			foreach (var id in type.Value) {
				lm.LightIDPlacementMap.Add(i, id);
				lm.LightIDPlacementMapReverse.Add(id, i);
				++i;
			}
		}
	}
}

}
