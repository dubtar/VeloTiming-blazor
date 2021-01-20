﻿using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VeloTiming.Server
{
	internal static class Utils
	{
		internal static Timestamp? ToTimestamp(this DateTime? date)
		{
			if (date == null) return null;
			if (date.Value.Kind == DateTimeKind.Unspecified)
				date = DateTime.SpecifyKind(date.Value, DateTimeKind.Utc);
			return Timestamp.FromDateTime(date.Value.ToUniversalTime());
		}

		internal static Proto.StartType ToProto(this Data.StartType type)
		{
			return type == Data.StartType.Laps ? Proto.StartType.Laps :
				type == Data.StartType.TimeTrial ? Proto.StartType.TimeTrial :
				throw new NotImplementedException($"Unknown start type: {type}");
		}

		internal static Data.StartType FromProto(this Proto.StartType type)
		{
			return type == Proto.StartType.Laps ? Data.StartType.Laps :
				type == Proto.StartType.TimeTrial ? Data.StartType.TimeTrial :
				throw new NotImplementedException($"Unknown start type: {type}");
		}

		internal static void UpdateCollection<E, M>(ICollection<E> entities, ICollection<M> models,
			Func<E, M, bool> areSameFunc, Func<M, E> createFunc)
		{
			var toAdd = models.Where(m => !entities.Any(e => areSameFunc(e, m))).ToArray();
			var toRemove = entities.Where(e => !models.Any(m => areSameFunc(e, m))).ToArray();
			foreach(var e in toRemove)
			{
				entities.Remove(e);
			}
			foreach(var m in toAdd)
			{
				entities.Add(createFunc(m));
			}
		}
	}
}
