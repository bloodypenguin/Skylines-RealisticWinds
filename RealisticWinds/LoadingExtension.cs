using ICities;

namespace RealisticWinds
{
    public class LoadingExtension : LoadingExtensionBase
    {

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            WindSpeedExtension.Deploy();
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            WindSpeedExtension.Revert();
        }
    }
}