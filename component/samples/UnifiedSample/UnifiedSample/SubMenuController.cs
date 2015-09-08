using System;
using System.Drawing;
using UIKit;
using Foundation;
using CoreGraphics;

namespace Sample
{
	public partial class SubMenuController : BaseController
	{
		public SubMenuController() : base(null, null)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			View.BackgroundColor = UIColor.FromRGB(.9f, .9f, .9f);

			var title = new UILabel(new RectangleF(0, 50, 270, 20));
			title.Font = UIFont.SystemFontOfSize(24.0f);
			title.TextAlignment = UITextAlignment.Center;
			title.TextColor = UIColor.Blue;
			title.Text = "Menu";

			var body = new UILabel(new RectangleF(30, 70, 200, 100));
			body.Font = UIFont.SystemFontOfSize(12.0f);
			body.TextAlignment = UITextAlignment.Center;
			body.Lines = 0;
			body.Text = @"This is a sub menu!";

			var backButton = new UIButton(UIButtonType.System);
			backButton.Frame = new RectangleF(0, 180, 260, 20);
			backButton.SetTitle("Back", UIControlState.Normal);
			backButton.TouchUpInside += (sender, e) => {
				SidebarController.ChangeMenuView(new SideMenuController());
			};

			View.Add(title);
			View.Add(body);
			View.Add(backButton);
		}
	}
}

