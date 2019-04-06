using FreeSql;

public static class FreeSqlDbContextExtenssions {

	/// <summary>
	/// 创建普通数据上下文档对象
	/// </summary>
	/// <param name="that"></param>
	/// <returns></returns>
	public static DbContext CreateDbContext(this IFreeSql that) {
		return new FreeContext(that);
	}

	/// <summary>
	/// 不跟踪查询的实体数据（在不需要更新其数据时使用），可提长查询性能
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="select"></param>
	/// <returns></returns>
	public static ISelect<T> NoTracking<T>(this ISelect<T> select) where T : class {
		return select.TrackToList(null);
	}
}