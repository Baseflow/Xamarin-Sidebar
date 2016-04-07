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
				PanBegan(sidebar.PanGesture, sidebar.MenuLocation, sidebar.GestureActiveArea);
			} else if (!_ignorePan && (sidebar.PanGesture.State == UIGestureRecognizerState.Changed)) {
				PanChanged(sidebar);
			} else if (!_ignorePan && (sidebar.PanGesture.State == UIGestureRecognizerState.Ended || 
									   sidebar.PanGesture.State == UIGestureRecognizerState.Cancelled)) {
				PanEnded(sidebar);
			}
		}


		private void PanBegan(UIPanGestureRecognizer panGesture, MenuLocations menuLocation, nfloat gestureActiveArea) {
			_panOriginX = ContentViewController.View.Frame.X;
			_ignorePan = PanGestureInActiveArea(panGesture, menuLocation, gestureActiveArea);
		}

		private bool PanGestureInActiveArea(UIPanGestureRecognizer panGesture, MenuLocations menuLocation, nfloat gestureActiveArea) {
			var position = panGesture.LocationInView(ContentViewController.View).X;
			if (menuLocation == MenuLocations.Left)
				return position > gestureActiveArea;
			else
				return position < ContentViewController.View.Bounds.Width - gestureActiveArea;
		}

		private void PanChanged(Sidebar sidebar) {
			var xDelta = sidebar.PanGesture.TranslationInView(ContentViewController.View).X;
			if (sidebar.MenuLocation == MenuLocations.Left) {
				PanChangedMenuLeft(sidebar.MenuWidth, sidebar.IsOpen, xDelta);
			} else if (sidebar.MenuLocation == MenuLocations.Right) {
				PanChangedMenuRight(sidebar.MenuWidth, sidebar.IsOpen, xDelta);
			}
			ShowShadowWhileDragging(sidebar.HasShadowing, sidebar.MenuLocation);
		}

		private void PanChangedMenuLeft(int menuWidth, bool isOpen, nfloat xDelta) {
			if ((xDelta > 0 && !isOpen) || (xDelta < 0 && isOpen)) {
				if (xDelta > menuWidth)
					xDelta = menuWidth;
				else if (xDelta < -menuWidth && isOpen)
					xDelta = menuWidth; 
				if (_panOriginX + xDelta <= menuWidth)
					ContentViewController.View.Frame = 
						new RectangleF(_panOriginX + xDelta, ContentViewController.View.Frame.Y, ContentViewController.View.Frame.Width, ContentViewController.View.Frame.Height);
			}
		}

		private void PanChangedMenuRight(int menuWidth, bool isOpen, nfloat xDelta) {
			if ((xDelta < 0 && !isOpen) || (xDelta > 0 && isOpen)) {
				if (xDelta < -menuWidth)
					xDelta = -menuWidth;
				else if (xDelta > menuWidth)
					xDelta = menuWidth; 
				if (_panOriginX + xDelta <= 0) 
					ContentViewController.View.Frame = 
						new RectangleF(_panOriginX + xDelta, ContentViewController.View.Frame.Y, ContentViewController.View.Frame.Width, ContentViewController.View.Frame.Height);
			}
		}

		private void PanEnded(Sidebar sidebar) {
			var xDelta = sidebar.PanGesture.TranslationInView(ContentViewController.View).X;
			var velocity = sidebar.PanGesture.VelocityInView(ContentViewController.View).X;
			if ((sidebar.MenuLocation == MenuLocations.Left && sidebar.IsOpen && xDelta < 0) || 
				(sidebar.MenuLocation == MenuLocations.Right && sidebar.IsOpen && xDelta > 0)) 
			{
				PanEndedTowardClose(sidebar);
			} 
			var flungOpenFromLeft = (sidebar.MenuLocation == MenuLocations.Left && (velocity > sidebar.FlingVelocity || ContentViewController.View.Frame.X > (sidebar.MenuWidth * sidebar.FlingPercentage)));
			var flungOpenFromRight = (sidebar.MenuLocation == MenuLocations.Right && (velocity < -sidebar.FlingVelocity || ContentViewController.View.Frame.X < -(sidebar.MenuWidth * sidebar.FlingPercentage)));
			if (flungOpenFromLeft || flungOpenFromRight) {
				sidebar.OpenMenu();
			} else {
				UIView.Animate(
					Sidebar.SlideSpeed, 
					0, 
					UIViewAnimationOptions.CurveEaseInOut,
					() => { ContentViewController.View.Frame = new RectangleF(0, 0, ContentViewController.View.Frame.Width, ContentViewController.View.Frame.Height); },
					() => {});
			}
		}

		private void PanEndedTowardClose(Sidebar sidebar) {
			if (ContentViewController.View.Frame.X > -ContentViewController.View.Frame.Width / 2) {
				sidebar.CloseMenu();
			} else {
				UIView.Animate(
					Sidebar.SlideSpeed, 
					0, 
					UIViewAnimationOptions.CurveEaseInOut,
					() => { CloseAnimation(); }, 
					() => { });
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

