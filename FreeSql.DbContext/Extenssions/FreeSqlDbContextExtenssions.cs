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
}