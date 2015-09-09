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
	internal class Sidebar
	{
		public const float DefaultFlingPercentage = 0.5f;
		public const float DefaultFlingVelocity = 800f;
		public const int DefaultMenuWidth = 260;
		public const int DefaultGestureActiveArea = 50;

		public static readonly nfloat SlideSpeed = 0.2f;


		private bool _disabled = false;
		private bool _shadowShown = false;

		private SidebarContentArea _sidebarContentArea;
		private SidebarMenuArea _sidebarMenuArea;



		public Sidebar(
			UIViewController rootViewController,
			UIViewController contentViewController,
			UIViewController menuViewController) 
		{
			_sidebarContentArea = new SidebarContentArea(contentViewController);
			_sidebarMenuArea = new SidebarMenuArea(menuViewController);

			SetVersion();
			SetDefaults();
		}


		public UIViewController ContentViewController { 
			get { return _sidebarContentArea.ContentViewController; } 
			set { _sidebarContentArea.ContentViewController = value; }
		}

		public UIViewController MenuViewController { 
			get { return _sidebarMenuArea.MenuViewController; } 
			set { _sidebarMenuArea.MenuViewController = value; }
		}


		public UITapGestureRecognizer TapGesture { get; set; }

		public UIPanGestureRecognizer PanGesture { get; set; }

		public bool IsIos7 { get; private set; }

		public float FlingPercentage { get; set; }

		public float FlingVelocity { get; set; }

		public float GestureActiveArea { get; set; }

		public bool Disabled {
			get {
				return _disabled;
			}
			set {
				if (value) {
					PanGesture.Enabled = false;
					TapGesture.Enabled = false;
				} else {
					PanGesture.Enabled = true;
					TapGesture.Enabled = true;
				}
				_disabled = value;
			}
		}

		public bool StatusBarMoves { get; set; }

		public bool HasShadowing { get; set; }

		public bool ReopenOnRotate { get; set; }

		public int MenuWidth { get; set; }

		public MenuLocations MenuLocation { get; set; }

		public bool IsOpen { get; set; }

		public event EventHandler<bool> StateChangeHandler;


		public void OpenMenu()
		{
			if (IsOpen || Disabled)
				return;
			ShowShadow();
			_sidebarContentArea.ContentViewController.View.EndEditing(true);
			UIView.Animate(
				Sidebar.SlideSpeed, 
				0, 
				UIViewAnimationOptions.CurveEaseInOut,
				() => { _sidebarContentArea.OpenAnimation(MenuLocation, MenuWidth); },
				() => {
					if (_sidebarContentArea.ContentViewController.View.Subviews.Length > 0)
						_sidebarContentArea.ContentViewController.View.Subviews[0].UserInteractionEnabled = false;
					_sidebarContentArea.ContentViewController.View.AddGestureRecognizer(TapGesture);
					IsOpen = true;
					if (StateChangeHandler != null) {
						StateChangeHandler.Invoke(this, IsOpen);
					}
				});
		}

		public void CloseMenu(bool animate = true)
		{
			if (!IsOpen || Disabled)
				return;
			MenuViewController.View.EndEditing(true);
			#if __UNIFIED__
			Action animation;
			Action finished;
			#else
			NSAction animation;
			NSAction finished;
			#endif
			animation = () => { _sidebarContentArea.CloseAnimation(); };
			finished = () => {
				if (_sidebarContentArea.ContentViewController.View.Subviews.Length > 0)
					_sidebarContentArea.ContentViewController.View.Subviews[0].UserInteractionEnabled = true;
				_sidebarContentArea.ContentViewController.View.RemoveGestureRecognizer (TapGesture);
				IsOpen = false;
				if (StateChangeHandler != null) {
					StateChangeHandler.Invoke(this, IsOpen);
				}
			};
			if (animate)
				UIView.Animate(Sidebar.SlideSpeed, 0, UIViewAnimationOptions.CurveEaseInOut, animation, finished);
			else {
				animation();
				finished();
			}
			HideShadow();
		}

		public void ChangeContentView(UIViewController newContentView) {
			if (ContentViewController.View != null)
			{
				ContentViewController.View.RemoveFromSuperview();
				if (TapGesture != null && IsOpen)
					ContentViewController.View.RemoveGestureRecognizer (TapGesture);
				if (PanGesture != null)
					ContentViewController.View.RemoveGestureRecognizer (PanGesture); 
			}
			if (ContentViewController != null)
				ContentViewController.RemoveFromParentViewController ();

			ContentViewController = newContentView;

			// setup a tap gesture to close the menu on root view tap
			TapGesture = new UITapGestureRecognizer ();
			TapGesture.AddTarget (() => CloseMenu());
			TapGesture.NumberOfTapsRequired = 1;
			PanGesture = new UIPanGestureRecognizer {
				Delegate = new SlideoutPanDelegate(),
				MaximumNumberOfTouches = 1,
				MinimumNumberOfTouches = 1
			};
			PanGesture.AddTarget (() => Pan());
			ContentViewController.View.AddGestureRecognizer(PanGesture);
		}

		public void ChangeMenuView(UIViewController newMenuView) {
			if (MenuViewController.View != null)
				MenuViewController.View.RemoveFromSuperview();
			if (MenuViewController != null)
				MenuViewController.RemoveFromParentViewController ();
			MenuViewController = newMenuView;
		}



		private class SlideoutPanDelegate : UIGestureRecognizerDelegate
		{
			public override bool ShouldReceiveTouch (UIGestureRecognizer recognizer, UITouch touch)
			{
				return true;
			}
		}

		public void Pan() {
			_sidebarContentArea.Pan(this);
		}

		public void SetStatusBarImage() {
			_sidebarContentArea.SetStatusBarImage(StatusBarMoves, IsIos7);
		}

		public void HideStatusBarImage() {
			_sidebarContentArea.HideStatusBarImage(IsIos7);
		}



		private void ShowShadow()
		{
			if (!HasShadowing || _shadowShown)
				return;
			var position = (MenuLocation == MenuLocations.Left) ? -5 : 5;
			_sidebarContentArea.DisplayShadow(position);
			_shadowShown = true;
		}

		private void HideShadow()
		{
			if (!HasShadowing || !_shadowShown)
				return;
			_sidebarContentArea.HideShadow();
			_shadowShown = false;
		}

		private void SetVersion() {
			var version = new System.Version(UIDevice.CurrentDevice.SystemVersion);
			IsIos7 = version.Major >= 7;
		}

		private void SetDefaults() {
			FlingPercentage = Sidebar.DefaultFlingPercentage;
			FlingVelocity = Sidebar.DefaultFlingVelocity;
			GestureActiveArea = Sidebar.DefaultGestureActiveArea;
			MenuLocation = MenuLocations.Right;
			MenuWidth = Sidebar.DefaultMenuWidth;
			HasShadowing = true;
			ReopenOnRotate = true;
			StatusBarMoves = true;
		}
	}
}

