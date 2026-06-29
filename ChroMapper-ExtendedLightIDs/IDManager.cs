using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Beatmap.Base;
using Beatmap.Enums;
using Beatmap.Helper;

namespace ChroMapper_ExtendedLightIDs {

public class IDManager {
	private Dictionary<int, SortedSet<int>> LightIDMap = new Dictionary<int, SortedSet<int>>();
	
#if CHROMPER_13
	private PlatformDescriptor descriptor;
	
	public void PlatformLoaded(PlatformDescriptor descriptor) {
		this.descriptor = descriptor;
	}
#else
	private EnvironmentDescriptor descriptor;
	
	public void PlatformLoaded(EnvironmentDescriptor descriptor) {
		this.descriptor = descriptor;
	}
#endif
	
	public SortedSet<int> IDsForType(int type) {
		if (!LightIDMap.ContainsKey(type)) {
#if CHROMPER_13
			if (descriptor.LightingManagers.Count() > type) {
				var lm = descriptor.LightingManagers[type];
				if (lm.LightIDPlacementMap == null) {
					lm.LoadOldLightOrder();
				}
				LightIDMap.Add(type, new SortedSet<int>(lm.LightIDPlacementMap.Values));
			}
#else
			var types = descriptor.BasicEventEffectManager.EventTypeToEffects;
			if (types.ContainsKey(type)) {
				var lm = types[type][0] as BasicLightEffect;
				LightIDMap.Add(type, new SortedSet<int>(lm.LightIDToLane.Values));
			}
#endif
			else {
				LightIDMap.Add(type, new SortedSet<int>());
			}
		}
		return LightIDMap[type];
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
		foreach (var type in LightIDMap) {
#if CHROMPER_13
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
#else
			if (!descriptor.BasicEventEffectManager.EventTypeToEffects.ContainsKey(type.Key)) continue;
			var lm = descriptor.BasicEventEffectManager.EventTypeToEffects[type.Key][0] as BasicLightEffect;
			
			var i = 0;
			lm.LightIDToLane.Clear();
			lm.LaneToLightID.Clear();
			foreach (var id in type.Value) {
				lm.LightIDToLane.Add(id, i);
				lm.LaneToLightID.Add(id);
				++i;
			}
#endif
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
			var off = new BaseEvent { JsonTime = 0, Type = type, Value = 0 };
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
