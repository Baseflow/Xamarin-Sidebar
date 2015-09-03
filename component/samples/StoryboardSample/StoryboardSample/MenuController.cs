using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace StoryboardSample
{
	partial class MenuController : BaseController
	{
		public MenuController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			var contentController = (ContentController)Storyboard.InstantiateViewController("ContentController");

			ContentButton.TouchUpInside += (o, e) => {
				if (NavController.TopViewController as ContentController == null)
					NavController.PushViewController(contentController, false);
				SidebarController.CloseMenu();
			};
		}
	}
}
