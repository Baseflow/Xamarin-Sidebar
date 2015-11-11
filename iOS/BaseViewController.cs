using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using UIKit;

[assembly:ExportRenderer(typeof(XamarinFormsSample.BasePage), typeof(XamarinFormsSample.iOS.BaseViewController))]

namespace XamarinFormsSample.iOS
{
	public class BaseViewController : PageRenderer
	{
		// provide access to the sidebar controller to all inheriting controllers
		protected SidebarNavigation.SidebarController SidebarController { 
			get {
				return (UIApplication.SharedApplication.Delegate as AppDelegate).RootViewController.SidebarController;
			} 
		}

		// provide access to the sidebar controller to all inheriting controllers
		protected NavController NavController { 
			get {
				return (UIApplication.SharedApplication.Delegate as AppDelegate).RootViewController.NavController;
			} 
		}


		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			var navigationItem = this.NavigationController.TopViewController.NavigationItem;
			navigationItem.SetRightBarButtonItem(
				new UIBarButtonItem(UIImage.FromBundle("threelines")
					, UIBarButtonItemStyle.Plain
					, (sender,args) => {
						SidebarController.ToggleMenu();
					}), true);
		}
	}
}

