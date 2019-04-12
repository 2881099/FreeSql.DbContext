using System;
using Xunit;

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

		[Fact]
		public void UpdateAttach() {
			var repos = g.sqlite.GetGuidRepository<AddUpdateInfo>();

			var item = new AddUpdateInfo { Id = Guid.NewGuid() };
			repos.Attach(item);

			item.Title = "xxx";
			repos.Update(item);
			Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(item));

			Console.WriteLine(repos.UpdateDiy.Where(a => a.Id == item.Id).Set(a => a.Clicks + 1).ToSql());
			repos.UpdateDiy.Where(a => a.Id == item.Id).Set(a => a.Clicks + 1).ExecuteAffrows();

			item = repos.Find(item.Id);
			Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(item));
		}

		[Fact]
		public void UpdateWhenNotExists() {
			var repos = g.sqlite.GetGuidRepository<AddUpdateInfo>();

			var item = new AddUpdateInfo { Id = Guid.NewGuid() };
			item.Title = "xxx";
			Assert.Throws<Exception>(() => repos.Update(item));
		}

		[Fact]
		public void Update() {
			g.sqlite.Insert(new AddUpdateInfo()).ExecuteAffrows();

			var repos = g.sqlite.GetGuidRepository<AddUpdateInfo>();

			var item = new AddUpdateInfo { Id = g.sqlite.Select<AddUpdateInfo>().First().Id };

			item.Title = "xxx";
			repos.Update(item);
			Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(item));
		}

		public class AddUpdateInfo {

			public Guid Id { get; set; }
			public string Title { get; set; }

			public int Clicks { get; set; } = 10;
		}
	}
}
