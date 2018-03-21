using System;
using System.Drawing;
using UIKit;
using Foundation;
using CoreGraphics;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;
using PointF = CoreGraphics.CGPoint;

namespace SidebarNavigation
{
	internal class SidebarMenuArea
	{
		public UIViewController MenuViewController { get; set; }

		public SidebarMenuArea(UIViewController viewController) {
			MenuViewController = viewController;
		}
	}
}