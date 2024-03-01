using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Beatmap.Base;
using Beatmap.Enums;
using Beatmap.Helper;

namespace ChroMapper_ExtendedLightIDs {

public class IDManager {
	private PlatformDescriptor descriptor;
	private Dictionary<int, SortedSet<int>> ids = new Dictionary<int, SortedSet<int>>();
	
	public void PlatformLoaded(PlatformDescriptor descriptor) {
		this.descriptor = descriptor;
	}
	
	public SortedSet<int> IDsForType(int type) {
		if (!ids.ContainsKey(type)) {
			if (descriptor.LightingManagers.Count() > type) {
				var lm = descriptor.LightingManagers[type];
				if (lm.LightIDPlacementMap == null) {
					lm.LoadOldLightOrder();
				}
				ids.Add(type, new SortedSet<int>(lm.LightIDPlacementMap.Values));
			}
			else {
				ids.Add(type, new SortedSet<int>());
			}
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
		List<BaseEvent> events = new List<BaseEvent>();
		var collection = BeatmapObjectContainerCollection.GetCollectionForType(Beatmap.Enums.ObjectType.Event) as EventGridContainer;
		foreach (var et in collection.AllLightEvents.Values) {
			foreach (var ev in et) {
				if (ev.CustomLightID != null) {
					IDsForType(ev.Type).UnionWith(ev.CustomLightID);
				}
			}
		}
		
		// Add them all
		foreach (var type in ids) {
			if (type.Key >= descriptor.LightingManagers.Count()) continue;
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
	
	public void AddIDs(int type, int begin, int end) {
		Debug.Log($"Add {type} {begin}-{end}");
		
		if (type < 0 || begin < 0 || end < 0 || end < begin) return;
		
		var generatedObjects = new List<BaseObject>();
		
		var collection = BeatmapObjectContainerCollection.GetCollectionForType(Beatmap.Enums.ObjectType.Event) as EventGridContainer;
		
		var actions = new List<BeatmapAction>();
		
		for (int i = begin; i <= end; ++i) {
			if (IDsForType(type).Contains(i)) {
				continue;
			}
			var off = BeatmapFactory.Event(0, type, 0);
			off.CustomLightID = new int[] { i };
			off = BeatmapFactory.Clone(off);
			generatedObjects.Add(off);
			(collection as BeatmapObjectContainerCollection).SpawnObject(off, out List<BaseObject> conflicting);
			actions.Add(new BeatmapObjectPlacementAction(off, conflicting, "ExtendedLightIDs Placeholder"));
		}
		
		BeatmapActionContainer.AddAction(
			new ActionCollectionAction(actions, false, false, "Adding ExtendedLightIDs Placeholders")
		);
		
		RefreshIDs();
		
		collection.RefreshPool(true);
		collection.PropagationEditing = collection.PropagationEditing;
	}
}

}
