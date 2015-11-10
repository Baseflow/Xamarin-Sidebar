using System;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms;

[assembly:ExportRenderer(typeof(XamarinFormsSample.RootPage), typeof(XamarinFormsSample.iOS.RootViewController))]

namespace XamarinFormsSample.iOS
{
	public class RootViewController : PageRenderer
	{
		// the sidebar controller for the app
		public SidebarNavigation.SidebarController SidebarController { get; private set; }

		// the navigation controller
		public NavController NavController { get; private set; }

		public RootViewController() //: base(null, null)
		{
			var temp = 1;
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			var _introController = new IntroController();

			// create a slideout navigation controller with the top navigation controller and the menu view controller
			NavController = new NavController();
			NavController.PushViewController(new IntroPage().CreateViewController(), false);
			SidebarController = new SidebarNavigation.SidebarController(this, NavController, new SideMenuController());
			SidebarController.MenuWidth = 220;
			SidebarController.ReopenOnRotate = false;
		}
	}
}

