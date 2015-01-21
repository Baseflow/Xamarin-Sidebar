using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Sample
{
	public partial class ContentController : BaseController
	{
		public ContentController() : base(null, null)
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

			var body = new UILabel(new RectangleF(50, 120, 220, 100));
			body.Font = UIFont.SystemFontOfSize(12.0f);
			body.TextAlignment = UITextAlignment.Center;
			body.Lines = 0;
			body.Text = @"This is the content view controller.";

			var menuButton = new UIButton(UIButtonType.System);
			menuButton.Frame = new RectangleF(50, 250, 220, 30);
			menuButton.SetTitle("Toggle Side Menu", UIControlState.Normal);
			menuButton.TouchUpInside += (sender, e) => {
				SidebarController.ToggleMenu();
			};

			View.Add(title);
			View.Add(body);
			View.Add(menuButton);
		}
	}
}

