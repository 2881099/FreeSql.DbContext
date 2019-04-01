using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FreeSql.Extensions.EntityUtil {
	public static class TempExtensions {

		

		static ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Action<object>>> _dicClearEntityPrimaryValueWithIdentity = new ConcurrentDictionary<DataType, ConcurrentDictionary<Type, Action<object>>>();
		/// <summary>
		/// 清除实体的主键值，将自增、Guid类型的主键值清除
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="orm"></param>
		/// <param name="item"></param>
		public static void ClearEntityPrimaryValueWithIdentity<TEntity>(this IFreeSql orm, TEntity item) {
			var func = _dicClearEntityPrimaryValueWithIdentity.GetOrAdd(orm.Ado.DataType, dt => new ConcurrentDictionary<Type, Action<object>>()).GetOrAdd(typeof(TEntity), t => {
				var _table = orm.CodeFirst.GetTableByEntity(t);
				var identitys = _table.Primarys.Where(a => a.Attribute.IsIdentity);
				var parm1 = Expression.Parameter(typeof(object));
				var var1Parm = Expression.Variable(t);
				var exps = new List<Expression>(new Expression[] {
					Expression.Assign(var1Parm, Expression.TypeAs(parm1, t))
				});
				foreach (var pk in _table.Primarys) {
					if (pk.Attribute.IsIdentity) {
						exps.Add(
							Expression.Assign(
								Expression.MakeMemberAccess(var1Parm, _table.Properties[pk.CsName]),
								Expression.Default(pk.CsType)
							)
						);
					}
				}
				return Expression.Lambda<Action<object>>(Expression.Block(new[] { var1Parm }, exps), new[] { parm1 }).Compile();
			});
			func(item);
		}

		
	}
}
