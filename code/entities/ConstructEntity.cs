using Sandbox;
using RTS.Constructs;

namespace RTS
{
	public partial class ConstructEntity : ModelEntity
	{
		[Net] public string BuildableId { get; set; }

		public BaseConstruct GetConstruct()
		{
			return Game.Instance.FindBuildable<BaseConstruct>( BuildableId );
		}

		public void SetConstruct( BaseConstruct construct )
		{
			BuildableId = construct.UniqueId;

			if ( !string.IsNullOrEmpty( construct.Model ) )
				SetModel( construct.Model );
		}

		public ConstructEntity()
		{
			Transmit = TransmitType.Always;
		}
	}
}

