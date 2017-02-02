namespace Dev2.Activities.Designers2.Core.Extensions
{
    public static class ActivityDesignerViewModelExtensions
    {
        public static void RunViewSetup(this ActivityDesignerViewModel designerViewModel)
        {
            if (IsItemDragged.Instance.IsDragged)
            {
                designerViewModel.ShowLarge = true;
                IsItemDragged.Instance.IsDragged = false;
            }
            else
            {
                designerViewModel.ShowLarge = false;
            } 
        }
    }
}
