using AirService.Model;

namespace AirService.Web.ViewModels
{
    public class MobileApplicationSettingsViewModel
    {
        public MobileApplicationSettings MobileApplicationSettings { get; set; }
        public string SelectedHeaderImage { get; set; }
        public string SelectedBackgroundImage { get; set; }
        public string SelectedThemeColour { get; set; }
    }
}