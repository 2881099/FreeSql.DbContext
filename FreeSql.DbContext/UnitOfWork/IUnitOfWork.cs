using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;

namespace FreeSql {
	public interface IUnitOfWork : IDisposable {

		DbTransaction GetOrBeginTransaction(bool isCreate = true);

		IsolationLevel? IsolationLevel { get; set; }

		void Commit();

		void Rollback();
	}
}
