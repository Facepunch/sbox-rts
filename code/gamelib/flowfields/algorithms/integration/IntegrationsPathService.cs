namespace Gamelib.FlowFields.Algorithms
{
    public class IntegrationsPathService : IntegrationService
    {
        private static IntegrationsPathService _default;

        public static IntegrationsPathService Default
        {
            get
            {
                if (_default == null) return _default = new();
                return _default;
            }
        }
    }
}
