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
	public class Sidebar
	{
		public const float DefaultFlingPercentage = 0.5f;
		public const float DefaultFlingVelocity = 800f;
		public const int DefaultMenuWidth = 260;
		public const int DefaultGestureActiveArea = 50;


		public static readonly nfloat SlideSpeed = 0.2f;


		private bool _isOpen = false;
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

			SetDefaults();
			SetupGestureRecognizers();
		}


		public UIViewController ContentViewController { 
			get { return _sidebarContentArea.ContentViewController; } 
			set { _sidebarContentArea.ContentViewController = value; }
		}

		public UIViewController MenuViewController { 
			get { return _sidebarMenuArea.MenuViewController; } 
			set { _sidebarMenuArea.MenuViewController = value; }
		}
			
		public bool IsOpen {
			get {
				return _isOpen;
			}
			set {
				_isOpen = value;
				if (StateChangeHandler != null) {
					StateChangeHandler.Invoke(this, _isOpen);
				}
			}
		}

		public int MenuWidth { get; set; }

		public MenuLocations MenuLocation { get; set; }

		public UITapGestureRecognizer TapGesture { get; private set; }

		public UIPanGestureRecognizer PanGesture { get; private set; }

		public float FlingPercentage { get; set; }

		public float FlingVelocity { get; set; }

		public float GestureActiveArea { get; set; }

		public bool HasShadowing { get; set; }

		public bool ReopenOnRotate { get; set; }

		public bool Disabled {
			get {
				return _disabled;
			}
			set {
				_disabled = value;
				if (_disabled) {
					PanGesture.Enabled = false;
					TapGesture.Enabled = false;
				} else {
					PanGesture.Enabled = true;
					TapGesture.Enabled = true;
				}
			}
		}


		public event EventHandler<bool> StateChangeHandler;


		public void OpenMenu()
		{
			if (IsOpen || Disabled)
				return;
			ShowShadow();
			_sidebarContentArea.BeforeOpenAnimation();
			UIView.Animate(
				Sidebar.SlideSpeed, 
				0, 
				UIViewAnimationOptions.CurveEaseInOut,
				() => { _sidebarContentArea.OpenAnimation(MenuLocation, MenuWidth); },
				() => {
					_sidebarContentArea.AfterOpenAnimation(TapGesture);
					IsOpen = true;
				});
		}

		public void CloseMenu(bool animate = true)
		{
			if (!IsOpen || Disabled)
				return;
			MenuViewController.View.EndEditing(true);
			UIView.Animate(
				animate ? Sidebar.SlideSpeed : 0, 
				0, 
				UIViewAnimationOptions.CurveEaseInOut, 
				() => { _sidebarContentArea.CloseAnimation(); }, 
				() => {
					_sidebarContentArea.AfterCloseAnimation(TapGesture);
					IsOpen = false;
				});
			HideShadow();
		}

		public void ChangeContentView(UIViewController newContentView) {
			RemoveContentView();
			ContentViewController = newContentView;
			ContentViewController.View.AddGestureRecognizer(PanGesture);
		}

		public void ChangeMenuView(UIViewController newMenuView) {
			RemoveMenuView();
			MenuViewController = newMenuView;
		}

		public void Pan() {
			_sidebarContentArea.Pan(this);
		}
			

		private void RemoveContentView() {
			if (ContentViewController.View != null)
			{
				ContentViewController.View.RemoveFromSuperview();
				if (TapGesture != null && IsOpen)
					ContentViewController.View.RemoveGestureRecognizer (TapGesture);
				if (PanGesture != null)
					ContentViewController.View.RemoveGestureRecognizer (PanGesture); 
			}
			if (ContentViewController != null)
				ContentViewController.RemoveFromParentViewController();
		}

		private void RemoveMenuView() {
			if (MenuViewController.View != null)
				MenuViewController.View.RemoveFromSuperview();
			if (MenuViewController != null)
				MenuViewController.RemoveFromParentViewController();
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

		private void SetDefaults() {
			FlingPercentage = Sidebar.DefaultFlingPercentage;
			FlingVelocity = Sidebar.DefaultFlingVelocity;
			GestureActiveArea = Sidebar.DefaultGestureActiveArea;
			MenuLocation = MenuLocations.Right;
			MenuWidth = Sidebar.DefaultMenuWidth;
			HasShadowing = true;
			ReopenOnRotate = true;
		}

		private void SetupGestureRecognizers() {
			TapGesture = new UITapGestureRecognizer ();
			TapGesture.AddTarget (() => CloseMenu());
			TapGesture.NumberOfTapsRequired = 1;
			PanGesture = new UIPanGestureRecognizer {
				Delegate = new SlideoutPanDelegate(),
				MaximumNumberOfTouches = 1,
				MinimumNumberOfTouches = 1
			};
			PanGesture.AddTarget(() => Pan());
		}


		private class SlideoutPanDelegate : UIGestureRecognizerDelegate
		{
			public override bool ShouldReceiveTouch (UIGestureRecognizer recognizer, UITouch touch)
			{
				return true;
			}
		}
	}
}

