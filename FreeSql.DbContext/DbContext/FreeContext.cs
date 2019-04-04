using System;
using System.Collections.Generic;
using System.Text;

namespace FreeSql {
	public class FreeContext : DbContext {

		public FreeContext(IFreeSql orm) {
			_orm = orm;
		}
	}
}
