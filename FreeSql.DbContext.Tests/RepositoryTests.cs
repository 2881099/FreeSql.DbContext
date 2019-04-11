using FreeSql.DataAnnotations;
using FreeSql;
using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;
using Npgsql.LegacyPostgis;

namespace FreeSql.Tests {
	public class RepositoryTests {

		[Fact]
		public void AddUpdate() {
			var repos = g.sqlite.GetGuidRepository<AddUpdateInfo>();

			var item = repos.Insert(new AddUpdateInfo());
			Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(item));

			item.Title = "xxx";
			repos.Update(item);
			Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(item));

			Console.WriteLine(repos.UpdateDiy.Where(a => a.Id == item.Id).Set(a => a.Clicks + 1).ToSql());
			repos.UpdateDiy.Where(a => a.Id == item.Id).Set(a => a.Clicks + 1).ExecuteAffrows();

			item = repos.Find(item.Id);
			Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(item));
		}
		public class AddUpdateInfo {

			public Guid Id { get; set; }
			public string Title { get; set; }

			public int Clicks { get; set; } = 10;
		}
	}
}
