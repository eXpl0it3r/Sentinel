using System;

namespace Stor
{
	public class PoolEvent : EventArgs
	{
		public override string ToString()
		{
			return string.Format("{0}: controller={1}, pool={2} {3}, oldPool={4} {5}", new object[]
			{
				this.type,
				this.controller.Id,
				(this.pool != null) ? this.pool.Id : "-",
				(this.pool != null) ? this.pool.Name : "-",
				(this.oldPool != null) ? this.oldPool.Id : "-",
				(this.oldPool != null) ? this.oldPool.Name : "-"
			});
		}

		public ControllerEventType type;

		public Controller controller;

		public SpacesPool pool;

		public SpacesPool oldPool;
	}
}
