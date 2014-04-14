using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace NavigationSample
{
	public partial class RootViewController : UIViewController
	{
		// the sidebar controller for the app
		public XamarinSidebar.SidebarController SidebarController { get; private set; }

		public RootViewController() : base(null, null)
		{

		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			// create a slideout navigation controller with the top navigation controller and the menu view controller
			var navigationController = new NavigationController();
			navigationController.PushViewController(new IntroController(), false);
			SidebarController = new XamarinSidebar.SidebarController(navigationController, new SideMenuController());

			// set the view to the sidebar controller
			// TODO make this unecessary somehow
			SidebarController.View.Frame = UIScreen.MainScreen.Bounds;
			// handle wiring things up so events propogate properly
			// TODO try to make this unnecessary
			AddChildViewController(SidebarController);
			View.AddSubview(SidebarController.View);
			SidebarController.DidMoveToParentViewController(this);
		}
	}
}

