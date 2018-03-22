using System;
using System.Drawing;
using UIKit;
using Foundation;
using CoreGraphics;
using UnifiedSample;

namespace Sample
{
	public partial class BaseController : UIViewController
	{
		// provide access to the sidebar controller to all inheriting controllers
		protected SidebarNavigation.SidebarController SidebarController { 
			get {
				return (UIApplication.SharedApplication.Delegate as AppDelegate).RootViewController.SidebarController;
			} 
		}

		public BaseController(string nibName, NSBundle bundle) : base(nibName, bundle)
		{
		}


		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
		}
	}
}

