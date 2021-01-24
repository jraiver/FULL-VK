using System;
using System.Collections.Generic;
using static fullvk.TextConsole;

namespace fullvk.Methods.Music
{
	class Correct
	{
		public class Filter
		{
			public enum FilterType
			{
				Artist,
				Title,
				bDuration,
				aDuration
			}

			public static Dictionary<FilterType, string> WhatIsType = new Dictionary<FilterType, string>()
			{
				{FilterType.Artist, "Артист"},
				{FilterType.Title, "Название"},
				{FilterType.aDuration, "Не длиннее"},
				{FilterType.bDuration, "Короче"}
			};

			public static FilterType type { get; set; }
			public static string value { get; set; }
		}

		public static void Add(Dictionary<Type, string> filters)
		{
			Console.Clear();
			int countF = 1;

			foreach (var filter in Filter.WhatIsType)
			{
				PrintConsole.Print($"[{countF}] - {filter.Value}");
				countF++;
			}

			PrintConsole.Print("Выберите тип:", MenuType.Input);

			switch (Console.ReadKey())
			{

			}
		}

	}
}
