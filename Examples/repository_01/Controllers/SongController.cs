﻿using FreeSql;
using Microsoft.AspNetCore.Mvc;
using restful.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace restful.Controllers {

	public class SongRepository : GuidRepository<Song> {
		public SongRepository(IFreeSql fsql) : base(fsql) {
		}
	}

	[Route("restapi/[controller]")]
	public class SongsController : Controller {

		BaseRepository<Song, int> _songRepository;

		public class xxxx {
			public int Id { get; set; }

			public bool IsDeleted { get; set; }
		}

		

		public SongsController(IFreeSql fsql,
			GuidRepository<Song> repos1,
			GuidRepository<xxxx> repos2,

			DefaultRepository<Song, int> repos11,
			DefaultRepository<xxxx, int> repos21,

			BaseRepository<Song> repos3, BaseRepository<Song, int> repos4,
			IBasicRepository<Song> repos31, IBasicRepository<Song, int> repos41,
			IReadOnlyRepository<Song> repos311, IReadOnlyRepository<Song, int> repos411,

			SongRepository reposSong
			) {
			_songRepository = repos4;

			//test code
			var curd1 = fsql.GetRepository<Song, int>();
			var curd2 = fsql.GetRepository<Song, string>();
			var curd3 = fsql.GetRepository<Song, Guid>();
			var curd4 = fsql.GetGuidRepository<Song>();

			Console.WriteLine(repos1.Select.ToSql());
			Console.WriteLine(reposSong.Select.ToSql());

			Console.WriteLine(repos2.Select.ToSql());
			Console.WriteLine(repos21.Select.ToSql());

			using (reposSong.DataFilter.DisableAll()) {
				Console.WriteLine(reposSong.Select.ToSql());
			}
		}

		[HttpGet]
		public Task<List<Song>> GetItems([FromQuery] string key, [FromQuery] int page = 1, [FromQuery] int limit = 20) {
			return _songRepository.Select.WhereIf(!string.IsNullOrEmpty(key), a => a.Title.Contains(key)).Page(page, limit).ToListAsync();
		}

		[HttpGet("{id}")]
		public Task<Song> GetItem([FromRoute] int id) {
			return _songRepository.FindAsync(id);
		}

		public class ModelSong {
			public string title { get; set; }
		}

		[HttpPost, ProducesResponseType(201)]
		public Task<Song> Create([FromBody] ModelSong model) {
			return _songRepository.InsertAsync(new Song { Title = model.title });
		}

		[HttpPut("{id}")]
		public Task Update([FromRoute] int id, [FromBody] ModelSong model) {
			return _songRepository.UpdateAsync(new Song { Id = id, Title = model.title });
		}

		[HttpPatch("{id}")]
		async public Task<Song> UpdateDiy([FromRoute] int id, [FromForm] string title) {
			var up = _songRepository.UpdateDiy.Where(a => a.Id == id);
			if (!string.IsNullOrEmpty(title)) up.Set(a => a.Title, title);
			var ret = await up.ExecuteUpdatedAsync();
			return ret.FirstOrDefault();
		}

		[HttpDelete("{id}"), ProducesResponseType(204)]
		public Task Delete([FromRoute] int id) {
			return _songRepository.DeleteAsync(a => a.Id == id);
		}
	}
}
