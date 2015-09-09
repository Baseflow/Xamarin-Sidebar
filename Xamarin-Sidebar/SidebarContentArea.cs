using System;
using System.Drawing;

#if __UNIFIED__
using UIKit;
using Foundation;
using CoreGraphics;

using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;
using PointF = CoreGraphics.CGPoint;
#else
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreGraphics;

using nfloat = global::System.Single;
using nint = global::System.Int32;
using nuint = global::System.UInt32;
#endif

namespace SidebarNavigation
{
	internal class SidebarContentArea
	{
		private bool _ignorePan;
		private nfloat _panOriginX;


		public UIViewController ContentViewController { get; set; }


		public SidebarContentArea(UIViewController viewController) {
			ContentViewController = viewController;
		}


		public void DisplayShadow(float position) {
			ContentViewController.View.Layer.ShadowOffset = new SizeF(position, 0);
			ContentViewController.View.Layer.ShadowPath = UIBezierPath.FromRect(ContentViewController.View.Bounds).CGPath;
			ContentViewController.View.Layer.ShadowRadius = 4.0f;
			ContentViewController.View.Layer.ShadowOpacity = 0.5f;
			ContentViewController.View.Layer.ShadowColor = UIColor.Black.CGColor;
		}

		public void HideShadow() {
			ContentViewController.View.Layer.ShadowOffset = new SizeF (0, 0);
			ContentViewController.View.Layer.ShadowRadius = 0.0f;
			ContentViewController.View.Layer.ShadowOpacity = 0.0f;
			ContentViewController.View.Layer.ShadowColor = UIColor.Clear.CGColor;
		}

		public void BeforeOpenAnimation() {
			ContentViewController.View.EndEditing(true);
		}

		public void OpenAnimation(MenuLocations menuLocation, int menuWidth) {
			if (menuLocation == MenuLocations.Right){
				ContentViewController.View.Frame = 
					new RectangleF (-menuWidth, 0, ContentViewController.View.Frame.Width, ContentViewController.View.Frame.Height);
			} else if(menuLocation == MenuLocations.Left){
				ContentViewController.View.Frame = 
					new RectangleF (menuWidth, 0, ContentViewController.View.Frame.Width, ContentViewController.View.Frame.Height);
			}
		}

		public void AfterOpenAnimation(UITapGestureRecognizer tapGesture) {
			if (ContentViewController.View.Subviews.Length > 0)
				ContentViewController.View.Subviews[0].UserInteractionEnabled = false;
			ContentViewController.View.AddGestureRecognizer(tapGesture);
		}

		public void CloseAnimation() {
			ContentViewController.View.Frame = 
				new RectangleF (0, 0, ContentViewController.View.Frame.Width, ContentViewController.View.Frame.Height);
		}

		public void AfterCloseAnimation(UITapGestureRecognizer tapGesture) {
			if (ContentViewController.View.Subviews.Length > 0)
				ContentViewController.View.Subviews[0].UserInteractionEnabled = true;
			ContentViewController.View.RemoveGestureRecognizer(tapGesture);
		}

		public void Pan(Sidebar sidebar)
		{
			if (sidebar.PanGesture.State == UIGestureRecognizerState.Began) {
				_panOriginX = ContentViewController.View.Frame.X;
				if (sidebar.MenuLocation == MenuLocations.Left)
					_ignorePan = sidebar.PanGesture.LocationInView(ContentViewController.View).X > sidebar.GestureActiveArea;
				else
					_ignorePan = sidebar.PanGesture.LocationInView(ContentViewController.View).X < ContentViewController.View.Bounds.Width - sidebar.GestureActiveArea;
			} else if (!_ignorePan && (sidebar.PanGesture.State == UIGestureRecognizerState.Changed)) {
				var t = sidebar.PanGesture.TranslationInView(ContentViewController.View).X;
				if (sidebar.MenuLocation == MenuLocations.Left) {
					if ((t > 0 && !sidebar.IsOpen) || (t < 0 && sidebar.IsOpen)) {
						if (t > sidebar.MenuWidth)
							t = sidebar.MenuWidth;
						else if (t < -sidebar.MenuWidth && sidebar.IsOpen)
							t = sidebar.MenuWidth; 
						if (_panOriginX + t <= sidebar.MenuWidth)
							ContentViewController.View.Frame = new RectangleF(_panOriginX + t, ContentViewController.View.Frame.Y, ContentViewController.View.Frame.Width, ContentViewController.View.Frame.Height);
						ShowShadowWhileDragging(sidebar.HasShadowing, sidebar.MenuLocation);
					}
				} else if (sidebar.MenuLocation == MenuLocations.Right) {
					if ((t < 0 && !sidebar.IsOpen) || (t > 0 && sidebar.IsOpen)) {
						if (t < -sidebar.MenuWidth)
							t = -sidebar.MenuWidth;
						else if (t > sidebar.MenuWidth)
							t = sidebar.MenuWidth; 
						if (_panOriginX + t <= 0)
							ContentViewController.View.Frame = new RectangleF(_panOriginX + t, ContentViewController.View.Frame.Y, ContentViewController.View.Frame.Width, ContentViewController.View.Frame.Height);
						ShowShadowWhileDragging(sidebar.HasShadowing, sidebar.MenuLocation);
					}
				}
			} else if (!_ignorePan && (sidebar.PanGesture.State == UIGestureRecognizerState.Ended || sidebar.PanGesture.State == UIGestureRecognizerState.Cancelled)) {
				var t = sidebar.PanGesture.TranslationInView(ContentViewController.View).X;
				var velocity = sidebar.PanGesture.VelocityInView(ContentViewController.View).X;
				if ((sidebar.MenuLocation == MenuLocations.Left && sidebar.IsOpen && t < 0) || (sidebar.MenuLocation == MenuLocations.Right && sidebar.IsOpen && t > 0)) {
					if (ContentViewController.View.Frame.X > -ContentViewController.View.Frame.Width / 2) {
						sidebar.CloseMenu();
					} else {
						UIView.Animate(Sidebar.SlideSpeed, 0, UIViewAnimationOptions.CurveEaseInOut,
							() => {
								ContentViewController.View.Frame = new RectangleF(-sidebar.MenuWidth, ContentViewController.View.Frame.Y, ContentViewController.View.Frame.Width, ContentViewController.View.Frame.Height);
							}, () => {
							});
					}
				} if ((sidebar.MenuLocation == MenuLocations.Left && (velocity > sidebar.FlingVelocity || ContentViewController.View.Frame.X > (sidebar.MenuWidth * sidebar.FlingPercentage)))
					|| (sidebar.MenuLocation == MenuLocations.Right && (velocity < -sidebar.FlingVelocity || ContentViewController.View.Frame.X < -(sidebar.MenuWidth * sidebar.FlingPercentage)))) {
					sidebar.OpenMenu();
				} else {
					UIView.Animate(Sidebar.SlideSpeed, 0, UIViewAnimationOptions.CurveEaseInOut,
						() =>
						{
							ContentViewController.View.Frame = new RectangleF(0, 0, ContentViewController.View.Frame.Width, ContentViewController.View.Frame.Height);
						}, () =>
						{
						});
				}
			}
		}


		private void ShowShadowWhileDragging(bool hasShadowing, MenuLocations menuLocation)
		{
			if (!hasShadowing)
				return;
			var xOffset = (menuLocation == MenuLocations.Left) ? -5 : 5;
			ContentViewController.View.Layer.ShadowOffset = new SizeF(xOffset, 0);
			ContentViewController.View.Layer.ShadowPath = UIBezierPath.FromRect (ContentViewController.View.Bounds).CGPath;
			ContentViewController.View.Layer.ShadowRadius = 4.0f;
			ContentViewController.View.Layer.ShadowOpacity = 0.5f;
			ContentViewController.View.Layer.ShadowColor = UIColor.Black.CGColor;
		}
	}
}

