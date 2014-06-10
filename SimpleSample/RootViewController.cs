using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using SidebarNavigation;

namespace Sample
{
	public partial class RootViewController : UIViewController
	{
		// the sidebar controller for the app
		public SidebarController SidebarController { get; private set; }

		public RootViewController() : base(null, null)
		{

		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			// create a slideout navigation controller with the top navigation controller and the menu view controller
			SidebarController = new SidebarController(this, new IntroController(), new SideMenuController());
			SidebarController.MenuLocation = SidebarController.MenuLocations.Left;
		}
	}
}

