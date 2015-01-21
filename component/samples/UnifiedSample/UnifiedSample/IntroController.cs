using System;
using System.Drawing;
using UIKit;
using Foundation;
using CoreGraphics;

namespace Sample
{
	public partial class IntroController : BaseController
	{
		public IntroController() : base(null, null)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			View.BackgroundColor = UIColor.White;

			var title = new UILabel(new RectangleF(0, 50, 320, 30));
			title.Font = UIFont.SystemFontOfSize(24.0f);
			title.TextAlignment = UITextAlignment.Center;
			title.TextColor = UIColor.Blue;
			title.Text = "Sidebar Navigation";

			var unified = new UILabel(new RectangleF(0, 80, 320, 30));
			unified.Font = UIFont.SystemFontOfSize(24.0f);
			unified.TextAlignment = UITextAlignment.Center;
			unified.TextColor = UIColor.Blue;
			unified.Text = "Unified";

			var body = new UILabel(new RectangleF(50, 120, 220, 100));
			body.Font = UIFont.SystemFontOfSize(12.0f);
			body.TextAlignment = UITextAlignment.Center;
			body.Lines = 0;
			body.Text = @"This is the intro view controller. 
Click the button below to open the menu to switch controllers.

You can also drag the menu open from the right side of the screen";

			var menuButton = new UIButton(UIButtonType.System);
			menuButton.Frame = new RectangleF(50, 250, 220, 30);
			menuButton.SetTitle("Toggle Side Menu", UIControlState.Normal);
			menuButton.TouchUpInside += (sender, e) => {
				SidebarController.ToggleMenu();
			};

			View.Add(title);
			View.Add(unified);
			View.Add(body);
			View.Add(menuButton);
		}
	}
}

