using System;
using Xunit;

namespace FreeSql.Tests {
	public class RepositoryTests {

		[Fact]
		public void AddUpdate() {
			var repos = g.sqlite.GetGuidRepository<AddUpdateInfo>();

			var item = repos.Insert(new AddUpdateInfo());
			Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(item));

			item = repos.Insert(new AddUpdateInfo { Id = Guid.NewGuid() });
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

		[Fact]
		public void UnitOfWorkRepository() {
			foreach (var fsql in new[] { g.sqlite, /*g.mysql, g.pgsql, g.oracle, g.sqlserver*/ }) {

				fsql.CodeFirst.ConfigEntity<FlowModel>(f => {
					f.Property(b => b.UserId).IsPrimary(true);
					f.Property(b => b.Id).IsPrimary(true).IsIdentity(true);
					f.Property(b => b.Name).IsNullable(false);
				});

				FlowModel flow = new FlowModel() {
					CreateTime = DateTime.Now,
					Name = "aaa",
					LastModifyTime = DateTime.Now,
					UserId = 1,
				};
				var flowRepos = fsql.GetRepository<FlowModel>();
				flowRepos.Insert(flow);

				//�������
				flow = new FlowModel() {
					CreateTime = DateTime.Now,
					Name = "aaa",
					LastModifyTime = DateTime.Now,
					UserId = 1,
				};
				using (var uow = fsql.CreateUnitOfWork()) {
					flowRepos = uow.GetRepository<FlowModel>();
					flowRepos.Insert(flow);
					uow.Commit();
				}
            }
		}

        [Fact]
        public void UnitOfWorkRepositoryWithDisableBeforeInsert()
        {
            foreach (var fsql in new[] { g.sqlite,  })
            {
                fsql.CodeFirst.ConfigEntity<FlowModel>(f => {
                    f.Property(b => b.UserId).IsPrimary(true);
                    f.Property(b => b.Id).IsPrimary(true).IsIdentity(true);
                    f.Property(b => b.Name).IsNullable(false);
                });

                var flowRepos = fsql.GetRepository<FlowModel>();

                var flow = new FlowModel()
                {
                    CreateTime = DateTime.Now,
                    Name = "aaa",
                    LastModifyTime = DateTime.Now,
                    UserId = 1,
                };

                //��������ݿ����Ѵ��ڵ����ݣ�Ϊ�˽������Ĳ������
                flowRepos.Delete(a => a.UserId == 1 &&a.Name== "aaa");

                using (var uow = fsql.CreateUnitOfWork())
                {
                    //�رչ�����Ԫ�����Ὺʼ����
                    uow.Disable();
                    var uowFlowRepos = uow.GetRepository<FlowModel>();
                    uowFlowRepos.Insert(flow);
                    //�ѹرչ�����Ԫ���᲻�ύ��ûӰ�죬�˴�ע����ȷ��������Ԫ�����Ƿ���Ч���ر��ˣ���CommitҲӦ�ò�������
                    //uow.Commit();
                }
                
                Assert.True(flowRepos.Select.Any(a => a.UserId == 1 && a.Name == "aaa"));
            }

        }

        [Fact]
        public void UnitOfWorkRepositoryWithDisableAfterInsert()
        {
            foreach (var fsql in new[] {g.sqlite,})
            {
                fsql.CodeFirst.ConfigEntity<FlowModel>(f =>
                {
                    f.Property(b => b.UserId).IsPrimary(true);
                    f.Property(b => b.Id).IsPrimary(true).IsIdentity(true);
                    f.Property(b => b.Name).IsNullable(false);
                });

                var flowRepos = fsql.GetRepository<FlowModel>();

                //��������ݿ����Ѵ��ڵ����ݣ�Ϊ�˽������Ĳ������
                flowRepos.Delete(a => a.UserId == 1 && a.Name == "aaa");

                var flow = new FlowModel()
                {
                    CreateTime = DateTime.Now,
                    Name = "aaa",
                    LastModifyTime = DateTime.Now,
                    UserId = 1,
                };


                Assert.Throws<Exception>(() =>
                {
                    using (var uow = fsql.CreateUnitOfWork())
                    {
                        var uowFlowRepos = uow.GetRepository<FlowModel>();
                        uowFlowRepos.Insert(flow);
                        //�������� Insert/Update/Delete ���ùر�uow�ķ������ᷢ���쳣
                        uow.Disable();
                        uow.Commit();
                    }

                });
            }
        }

        [Fact]
        public void UnitOfWorkRepositoryWithoutDisable()
        {
            foreach (var fsql in new[] { g.sqlite, })
            {
                fsql.CodeFirst.ConfigEntity<FlowModel>(f =>
                {
                    f.Property(b => b.UserId).IsPrimary(true);
                    f.Property(b => b.Id).IsPrimary(true).IsIdentity(true);
                    f.Property(b => b.Name).IsNullable(false);
                });

                var flowRepos = fsql.GetRepository<FlowModel>();
                if (flowRepos.Select.Any(a => a.UserId == 1 && a.Name == "aaa"))
                {
                    flowRepos.Delete(a => a.UserId == 1);
                }


                var flow = new FlowModel()
                {
                    CreateTime = DateTime.Now,
                    Name = "aaa",
                    LastModifyTime = DateTime.Now,
                    UserId = 1,
                };


                using (var uow = fsql.CreateUnitOfWork())
                {
                    var uowFlowRepos = uow.GetRepository<FlowModel>();
                    uowFlowRepos.Insert(flow);
                    //������commit�������ύ���ݿ����
                    //uow.Commit();
                }
                Assert.False(flowRepos.Select.Any(a => a.UserId == 1 && a.Name == "aaa"));
            }
        }


        public partial class FlowModel {
			public int UserId { get; set; }
			public int Id { get; set; }
			public int? ParentId { get; set; }
			public string Name { get; set; }
			public DateTime CreateTime { get; set; }
			public DateTime LastModifyTime { get; set; }
			public string Desc { get; set; }
		}

		[Fact]
		public void AsType() {
			g.sqlite.Insert(new AddUpdateInfo()).ExecuteAffrows();

			var repos = g.sqlite.GetGuidRepository<object>();
			repos.AsType(typeof(AddUpdateInfo));

			var item = new AddUpdateInfo();
			repos.Insert(item);
			repos.Update(item);

			item.Clicks += 1;
			repos.InsertOrUpdate(item);

			var item2 = repos.Find(item.Id) as AddUpdateInfo;
			Assert.Equal(item.Clicks, item2.Clicks);

			repos.DataFilter.Apply("xxx", a => (a as AddUpdateInfo).Clicks == 2);
			Assert.Null(repos.Find(item.Id));
		}
	}
}
