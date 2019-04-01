这是 [FreeSql](https://github.com/2881099/FreeSql) 衍生出来的扩展包，包含 DbContext & DbSet、Repository & UnitOfWork 实现面向对象的特性。

> dotnet add package FreeSql.DbContext

## 更新日志

### v0.4.2

- 增加 DbSet UpdateAsync/UpdateRangeAsync 方法，当一个实体被更新两次时，会先执行前面的队列；
- 增加 GetRepository 获取联合主键的适用仓储类；
- 增加 DbSet 在 Add/Update 时对导航属性(OneToMany) 的处理（AddOrUpdate）；

### v0.4.1
- 独立 FreeSql.DbContext 项目；
- 实现 Repository + DbSet 统一的状态跟踪与工作单元；
- 增加 DbSet AddOrUpdate 方法；
- 增加 Repository InsertOrUpdate 方法；

## DbContext & DbSet

```csharp
using (var ctx = new SongContext()) {
    var song = new Song { BigNumber = "1000000000000000000" };
    ctx.Songs.Add(song);

    ctx.Songs.Update(song);

    song.BigNumber = (BigInteger.Parse(song.BigNumber) + 1).ToString();
    ctx.Songs.Update(song);

    ctx.SaveChanges();
}
```

## Repository & UnitOfWork

仓储与工作单元一起使用，工作单元具有事务特点。

```csharp
using (var unitOfWork = fsql.CreateUnitOfWork()) {
    var songRepository = unitOfWork.GetRepository<Song, int>();

    var song = new Song { BigNumber = "1000000000000000000" };
    songRepository.Insert(song);

    songRepository.Update(song);

    song.BigNumber = (BigInteger.Parse(song.BigNumber) + 1).ToString();
    songRepository.Update(song);

    ctx.Commit();
}
```

## Repository

简单使用仓储，有状态跟踪，它不包含事务的特点。

```csharp
var songRepository = fsql.GetRepository<Song, int>();
var song = new Song { BigNumber = "1000000000000000000" };
songRepository.Insert(song);
```

## IFreeSql 核心定义

```csharp
var fsql = new FreeSql.FreeSqlBuilder()
    .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=|DataDirectory|\dd2.db;Pooling=true;Max Pool Size=10")
    .UseAutoSyncStructure(true)
    .UseNoneCommandParameter(true)

    .UseMonitorCommand(cmd => Trace.WriteLine(cmd.CommandText))
    .Build();

public class Song {
    [Column(IsIdentity = true)]
    public int Id { get; set; }
    public string BigNumber { get; set; }

    [Column(IsVersion = true)] //乐观锁
    public long versionRow { get; set; }
}

public class SongContext : DbContext {
    public DbSet<Song> Songs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder) {
        builder.UseFreeSql(fsql);
    }
}
```

# 过滤器与验证

假设我们有User(用户)、Topic(主题)两个实体，在领域类中定义了两个仓储：

```csharp
var userRepository = fsql.GetGuidRepository<User>();
var topicRepository = fsql.GetGuidRepository<Topic>();
```

在开发过程中，总是担心 topicRepository 的数据安全问题，即有可能查询或操作到其他用户的主题。因此我们在v0.0.7版本进行了改进，增加了 filter lambad 表达式参数。

```csharp
var userRepository = fsql.GetGuidRepository<User>(a => a.Id == 1);
var topicRepository = fsql.GetGuidRepository<Topic>(a => a.UserId == 1);
```

* 在查询/修改/删除时附加此条件，从而达到不会修改其他用户的数据；
* 在添加时，使用表达式验证数据的合法性，若不合法则抛出异常；

# 分表与分库

FreeSql 提供 AsTable 分表的基础方法，GuidRepository 作为分存式仓储将实现了分表与分库（不支持跨服务器分库）的封装。

```csharp
var logRepository = fsql.GetGuidRepository<Log>(null, oldname => $"{oldname}_{DateTime.Now.ToString("YYYYMM")}");
```

上面我们得到一个日志仓储按年月分表，使用它 CURD 最终会操作 Log_201903 表。

合并两个仓储，实现分表下的联表查询：

```csharp
fsql.GetGuidRepository<User>().Select.FromRepository(logRepository)
    .LeftJoin<Log>(b => b.UserId == a.Id)
    .ToList();
```

注意事项：

* 不能使用 CodeFirst 迁移分表，开发环境时仍然可以迁移 Log 表；
* 不可在分表分库的实体类型中使用《延时加载》；
