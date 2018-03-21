using System;
using System.Drawing;
using System.Linq;
using UIKit;
using Foundation;
using CoreGraphics;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;
using PointF = CoreGraphics.CGPoint;

namespace SidebarNavigation
{
	internal class SidebarContentArea
	{
        protected nfloat _panOriginX;
        protected UIView _viewOverlay;

		public UIViewController ContentViewController { get; set; }

		public float ShadowRadius { get; set; } = 4.0f;

		public float ShadowOpacity { get; set; } = 0.5f;

		public UIColor ShadowColor { get; set; } = UIColor.Black;

		public SidebarContentArea(UIViewController viewController) {
			ContentViewController = viewController;

			InitializeDarkOverlay();
		}

        public virtual void DisplayShadow(float position) {
			ContentViewController.View.Layer.ShadowOffset = new SizeF(position, 0);
			ContentViewController.View.Layer.ShadowPath = UIBezierPath.FromRect(ContentViewController.View.Bounds).CGPath;
			ContentViewController.View.Layer.ShadowRadius = ShadowRadius;
			ContentViewController.View.Layer.ShadowOpacity = ShadowOpacity;
			ContentViewController.View.Layer.ShadowColor = ShadowColor.CGColor;
		}

        public virtual void HideShadow() {
			ContentViewController.View.Layer.ShadowOffset = new SizeF (0, 0);
			ContentViewController.View.Layer.ShadowRadius = 0.0f;
			ContentViewController.View.Layer.ShadowOpacity = 0.0f;
			ContentViewController.View.Layer.ShadowColor = UIColor.Clear.CGColor;
		}

        public virtual void ShowDarkOverlay(float darkOverlayAlpha) 
		{
			if(!ContentViewController.View.Subviews.Contains(_viewOverlay))
				ContentViewController.View.AddSubview(_viewOverlay);
			
			_viewOverlay.Alpha = darkOverlayAlpha;
		}

        protected virtual void InitializeDarkOverlay()
		{
			if(_viewOverlay != null) 
				return;

			_viewOverlay = new UIView { BackgroundColor = UIColor.Black};
			_viewOverlay.Alpha = 0f;
			_viewOverlay.Frame = ContentViewController.View.Bounds;
			_viewOverlay.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;

			ContentViewController.View.AddSubview(_viewOverlay);
			ContentViewController.View.BringSubviewToFront(_viewOverlay);
		}

        public virtual void HideDarkOverlay() 
		{
			_viewOverlay.Alpha = 0f;
		}

        public virtual void BeforeOpenAnimation() {
			ContentViewController.View.EndEditing(true);
		}

        public virtual void OpenAnimation(MenuLocations menuLocation, int menuWidth) {
			if (menuLocation == MenuLocations.Right){
				ContentViewController.View.Frame = 
					new RectangleF (-menuWidth, 0, ContentViewController.View.Frame.Width, ContentViewController.View.Frame.Height);
			} else if(menuLocation == MenuLocations.Left){
				ContentViewController.View.Frame = 
					new RectangleF (menuWidth, 0, ContentViewController.View.Frame.Width, ContentViewController.View.Frame.Height);
			}
		}

        public virtual void AfterOpenAnimation(UITapGestureRecognizer tapGesture) {
			if (ContentViewController.View.Subviews.Length > 0)
				ContentViewController.View.Subviews[0].UserInteractionEnabled = false;
			ContentViewController.View.AddGestureRecognizer(tapGesture);
		}

        public virtual void CloseAnimation() {
			ContentViewController.View.Frame = 
				new RectangleF (0, 0, ContentViewController.View.Frame.Width, ContentViewController.View.Frame.Height);
		}

        public virtual void AfterCloseAnimation(UITapGestureRecognizer tapGesture) {
			if (ContentViewController.View.Subviews.Length > 0)
				ContentViewController.View.Subviews[0].UserInteractionEnabled = true;
			ContentViewController.View.RemoveGestureRecognizer(tapGesture);
		}
			
        public virtual void Pan(Sidebar sidebar)
		{
			if (sidebar.PanGesture.State == UIGestureRecognizerState.Began) {
				PanBegan();
			} else if (sidebar.PanGesture.State == UIGestureRecognizerState.Changed) {
				PanChanged(sidebar);
			} else if (sidebar.PanGesture.State == UIGestureRecognizerState.Ended || 
			           sidebar.PanGesture.State == UIGestureRecognizerState.Cancelled) {
				PanEnded(sidebar);
			}
		}

		/// <summary>
		/// Returns whether a certain touch is within the area that should
		/// activate the pan gesture sliding out the menu.
		/// </summary>
		/// <param name="touch"></param>
		/// <param name="sidebar"></param>
		/// <returns></returns>
		public virtual bool TouchInActiveArea(UITouch touch, Sidebar sidebar)
		{
			var view = ContentViewController.View;
			var position = touch.LocationInView(view).X;
			var area = sidebar.GestureActiveArea;

			return sidebar.MenuLocation == MenuLocations.Left ? 
				position < area : 
				position > (view.Bounds.Width - area);
		}

        protected virtual void PanBegan() {
			_panOriginX = ContentViewController.View.Frame.X;
		}

        protected virtual void PanChanged(Sidebar sidebar) {
			var xDelta = sidebar.PanGesture.TranslationInView(ContentViewController.View).X;
			if (sidebar.MenuLocation == MenuLocations.Left) {
				PanChangedMenuLeft(sidebar.MenuWidth, sidebar.IsOpen, xDelta);
			} else if (sidebar.MenuLocation == MenuLocations.Right) {
				PanChangedMenuRight(sidebar.MenuWidth, sidebar.IsOpen, xDelta);
			}
			ShowShadowWhileDragging(sidebar.HasShadowing, sidebar.MenuLocation);
		}

        protected virtual void PanChangedMenuLeft(int menuWidth, bool isOpen, nfloat xDelta) {
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

        protected virtual void PanChangedMenuRight(int menuWidth, bool isOpen, nfloat xDelta) {
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

        protected virtual void PanEnded(Sidebar sidebar) {
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
					() => { ContentViewController.View.Frame = new RectangleF(0, 0, ContentViewController.View.Frame.Width, ContentViewController.View.Frame.Height); HideDarkOverlay(); },
					() => {});
			}
		}

        protected virtual void PanEndedTowardClose(Sidebar sidebar) {
			if (ContentViewController.View.Frame.X > -ContentViewController.View.Frame.Width / 2) {
				sidebar.CloseMenu();
			} else {
				UIView.Animate(
					Sidebar.SlideSpeed, 
					0, 
					UIViewAnimationOptions.CurveEaseInOut,
					() => { CloseAnimation(); HideDarkOverlay(); }, 
					() => { });
			}
		}

        protected virtual void ShowShadowWhileDragging(bool hasShadowing, MenuLocations menuLocation)
		{
			if (!hasShadowing)
				return;
			var xOffset = (menuLocation == MenuLocations.Left) ? -5 : 5;
			ContentViewController.View.Layer.ShadowOffset = new SizeF(xOffset, 0);
			ContentViewController.View.Layer.ShadowPath = UIBezierPath.FromRect (ContentViewController.View.Bounds).CGPath;
			ContentViewController.View.Layer.ShadowRadius = ShadowRadius;
			ContentViewController.View.Layer.ShadowOpacity = ShadowOpacity;
			ContentViewController.View.Layer.ShadowColor = ShadowColor.CGColor;
		}
	}
}

