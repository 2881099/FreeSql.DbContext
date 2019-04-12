using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FreeSql.Extensions.EntityUtil {
	public static class TempExtensions {

		/// <summary>
		/// 使用新实体的主键值，复盖旧实体的主键值
		/// </summary>
		static ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Action<object, object>>> _dicMapEntityKeyValue = new ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Action<object, object>>>();
		public static void MapEntityKeyValue<TEntity>(this IFreeSql orm, TEntity from, TEntity to) {
			var func = _dicMapEntityKeyValue.GetOrAdd(orm.Ado.DataType, dt => new ConcurrentDictionary<Type, Action<object, object>>()).GetOrAdd(typeof(TEntity), t => {
				var _table = orm.CodeFirst.GetTableByEntity(t);
				var pks = _table.Primarys;
				var parm1 = Expression.Parameter(typeof(object));
				var parm2 = Expression.Parameter(typeof(object));
				var var1Parm = Expression.Variable(t);
				var var2Parm = Expression.Variable(t);
				var exps = new List<Expression>(new Expression[] {
					Expression.Assign(var1Parm, Expression.TypeAs(parm1, t)),
					Expression.Assign(var2Parm, Expression.TypeAs(parm2, t))
				});
				foreach (var pk in pks) {
					exps.Add(
						Expression.Assign(
							Expression.MakeMemberAccess(var2Parm, _table.Properties[pk.CsName]),
							Expression.MakeMemberAccess(var1Parm, _table.Properties[pk.CsName])
						)
					);
				}
				return Expression.Lambda<Action<object, object>>(Expression.Block(new[] { var1Parm, var2Parm }, exps), new[] { parm1, parm2 }).Compile();
			});
			func(from, to);
		}

	}
}
