using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Sample
{
	public partial class ContentController : UIViewController
	{
		public ContentController() : base(null, null)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			View.BackgroundColor = UIColor.LightGray;

			var menuButton = new UIButton(UIButtonType.System);
			menuButton.Frame = new RectangleF(50, 50, 220, 30);
			menuButton.SetTitle("Toggle Side Menu", UIControlState.Normal);
			menuButton.TouchUpInside += (sender, e) => {
				(UIApplication.SharedApplication.Delegate as AppDelegate).RootViewController.SidebarController.ToggleMenu();
			};

			View.Add(menuButton);
		}
	}
}

