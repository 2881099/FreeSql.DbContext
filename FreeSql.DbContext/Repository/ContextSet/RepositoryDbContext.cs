using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql {
	internal class RepositoryDbContext<TEntity> : DbContext where TEntity : class {

		protected BaseRepository<TEntity> _repos;
		public RepositoryDbContext(IFreeSql orm, BaseRepository<TEntity> repos) : base() {
			_orm = orm;
			_repos = repos;
			_isUseUnitOfWork = false;
			_uowPriv = _repos.UnitOfWork;
		}

		public override object Set(Type entityType) {
			if (_dicSet.ContainsKey(entityType)) return _dicSet[entityType];

			var tb = _orm.CodeFirst.GetTableByEntity(entityType);
			if (tb == null) return null;

			object repos = _repos;
			if (entityType != typeof(TEntity)) {
				var filter = _repos.DataFilter as DataFilter<TEntity>;
				repos = Activator.CreateInstance(typeof(DefaultRepository<,>).MakeGenericType(entityType, typeof(int)), _repos.Orm);
				(repos as IBaseRepository).UnitOfWork = _repos.UnitOfWork;
				DataFilterUtil.SetRepositoryDataFilter(repos, fl => {
					foreach (var f in filter._filters)
						fl.Apply<TEntity>(f.Key, f.Value.Expression);
				});
			}

			var sd = Activator.CreateInstance(typeof(RepositoryDbSet<>).MakeGenericType(entityType), repos);
			if (entityType != typeof(object)) _dicSet.Add(entityType, sd);
			return sd;
		}

		RepositoryDbSet<TEntity> _dbSet;
		public RepositoryDbSet<TEntity> DbSet => _dbSet ?? (_dbSet = Set<TEntity>() as RepositoryDbSet<TEntity>);

		public override int SaveChanges() {
			ExecCommand();
			var ret = _affrows;
			_affrows = 0;
			return ret;
		}
		async public override Task<int> SaveChangesAsync() {
			await ExecCommandAsync();
			var ret = _affrows;
			_affrows = 0;
			return ret;
		}
	}
}
