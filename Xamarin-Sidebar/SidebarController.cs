using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.CoreGraphics;

namespace XamarinSidebar
{
	public class SidebarController : UIViewController
	{
		public const int _menuWidth = 260;

		private event UITouchEventArgs _shouldReceiveTouch;

		#region Private Fields

		private float _slideSpeed = 0.2f;
		private bool _isIos7 = false;
		private bool _isOpen = false;
		private bool _shadowShown;

		// for swipe gesture
		private float _panOriginX;
		private bool _ignorePan;

		/// <summary>
		/// The view controller for the navigation menu.
		/// This is what is shown when the the menu is displayed.
		/// </summary>
		private UIViewController _navigation;

		// gesture recognizers
		private UITapGestureRecognizer _tapGesture;
		private UIPanGestureRecognizer _panGesture;

		private UIImageView _statusImage;

		#endregion

		#region Public Properties

		/// <summary>
		/// The view shown the content area.
		/// </summary>
		public UIViewController RootViewController { get; private set; }

		/// <summary>
		/// The view controller for the navigation menu.
		/// This is what will be shown when the menu is displayed.
		/// </summary>
		public UIViewController NavigationViewController
		{
			get { return _navigation; }
			set { _navigation = value; }
		}

		/// <summary>
		/// Determines if the status bar should be made static
		/// </summary>
		/// <value><c>true</c> if disable status bar moving; otherwise, <c>false</c>.</value>
		public bool DisableStatusBarMoving { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether there should be shadowing effects on the root view
		/// </summary>
		public bool LayerShadowing { get; set; }

		#endregion

		#region Private Properties

		/// <summary>
		/// The UIView of the root view controller.
		/// </summary>
		private UIView _rootView
		{
			get
			{
				if (RootViewController == null)
					return null;
				return RootViewController.View;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Required contructor.
		/// </summary>
		public SidebarController(IntPtr handle) : base(handle)
		{
		}

		/// <summary>
		/// Contructor.
		/// </summary>
		/// <param name="rootViewController">
		/// The current root view controller of the application.
		/// This is what will be panned to show the menu.
		/// </param>
		/// <param name="navigationViewController">The view controller for the navigation menu.</param>
		public SidebarController(UIViewController rootViewController, UIViewController navigationViewController = null)
		{
			if (navigationViewController == null)
				navigationViewController = new UIViewController();
			Initialize(rootViewController, navigationViewController);
		}

		/// <summary>
		/// Do stuff when the view is show.
		/// </summary>
		public override void ViewWillAppear(bool animated)
		{
			RectangleF navigationFrame = _navigation.View.Frame;
			navigationFrame.X = navigationFrame.Width - _menuWidth;
			navigationFrame.Width = _menuWidth;
			navigationFrame.Location = PointF.Empty;
			_navigation.View.Frame = navigationFrame;

			base.ViewWillAppear(animated);
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets the current state of the menu.
		/// Setting this property will open/close the menu respectively.
		/// </summary>
		public bool IsOpen
		{
			get { return _isOpen; }
			set
			{
				_isOpen = value;
				if (value)
					HideMenu();
				else
					ShowMenu();
			}
		}

		/// <summary>
		/// Toggles the menu.
		/// </summary>
		public void ToggleMenu()
		{
			if (!IsOpen && RootViewController != null && RootViewController.IsViewLoaded)
				ResignFirstResponders(RootViewController.View);

			if (IsOpen)
				HideMenu();
			else
				ShowMenu();
		}

		/// <summary>
		/// Shows the slideout navigation menu.
		/// </summary>
		public void ShowMenu()
		{
			if (IsOpen)
				return;
			ShowShadow(5);
			var view = _rootView;
			UIView.Animate(_slideSpeed, 
				0, 
				UIViewAnimationOptions.CurveEaseInOut,
				() => { view.Frame = new RectangleF (-_menuWidth, 0, view.Frame.Width, view.Frame.Height); },
				() => {
					if (view.Subviews.Length > 0)
						view.Subviews[0].UserInteractionEnabled = false;
					view.AddGestureRecognizer(_tapGesture);
					_isOpen = true;
				});

		}

		/// <summary>
		/// Hides the slideout navigation menu.
		/// </summary>
		public void HideMenu(bool animate = true)
		{
			if (!IsOpen)
				return;
			var view = _rootView;
			// define the animation
			NSAction animation = () => { view.Frame = new RectangleF (0, 0, view.Frame.Width, view.Frame.Height); };
			// define the action for finished animation
			NSAction finished = () => {
				if (view.Subviews.Length > 0)
					view.Subviews[0].UserInteractionEnabled = true;
				view.RemoveGestureRecognizer (_tapGesture);
				_isOpen = false;
			};
			if (animate)
				UIView.Animate(_slideSpeed, 0, UIViewAnimationOptions.CurveEaseInOut, animation, finished);
			else {
				// fire the animation results manually
				animation();
				finished();
			}
			HideShadow();
		}

		public void ChangeContentView(UIViewController newContentView) {
			if (_rootView != null)
				_rootView.RemoveFromSuperview();
			RootViewController = newContentView;
			HideMenu(true);
			SetVisibleView();
			// setup a tap gesture to close the menu on root view tap
			_tapGesture = new UITapGestureRecognizer ();
			_tapGesture.AddTarget (() => HideMenu());
			_tapGesture.NumberOfTapsRequired = 1;
			
			_panGesture = new UIPanGestureRecognizer {
				Delegate = new SlideoutPanDelegate(this),
				MaximumNumberOfTouches = 1,
				MinimumNumberOfTouches = 1
			};
			_panGesture.AddTarget (() => Pan (_rootView));
			_rootView.AddGestureRecognizer (_panGesture);
		}

		#endregion

		#region Private Methods

		private void Initialize(UIViewController currentViewController, UIViewController navigationViewController)
		{
			RootViewController = currentViewController;
			_navigation = navigationViewController;

			// enable shadow by default
			LayerShadowing = true;

			// make the status bar static
			DisableStatusBarMoving = true;
			_statusImage = new UIImageView();

			// set iOS 7 flag
			var version = new System.Version(UIDevice.CurrentDevice.SystemVersion);
			_isIos7 = version.Major >= 7;

			// add the navigation view on the right
			var navigationFrame = _navigation.View.Frame;
			navigationFrame.X = navigationFrame.Width - _menuWidth;
			navigationFrame.Width = _menuWidth;
			_navigation.View.Frame = navigationFrame;
			View.AddSubview(_navigation.View);

			ChangeContentView(currentViewController);
		}

		/// <summary>
		/// Places the root view on top of the navigation view.
		/// </summary>
		private void SetVisibleView()
		{
			if(!DisableStatusBarMoving)
				UIApplication.SharedApplication.SetStatusBarHidden(false, UIStatusBarAnimation.Fade);
			bool isOpen = false;

			RectangleF frame = View.Bounds;
			if (isOpen)
				frame.X = _menuWidth;

			SetViewSize();
			SetLocation(frame);

			View.AddSubview(_rootView);
			AddChildViewController(RootViewController);
		}

		/// <summary>
		/// Sets the size of the root view.
		/// </summary>
		private void SetViewSize()
		{
			RectangleF frame = View.Bounds;
			if (_rootView.Bounds == frame)
				return;
			_rootView.Bounds = frame;
		}

		/// <summary>
		/// Sets the location of the root view.
		/// </summary>
		/// <param name="frame">Frame.</param>
		private void SetLocation(RectangleF frame)
		{
			frame.Y = 0;
			_rootView.Layer.AnchorPoint = new PointF(.5f, .5f);

			// exit if we're already at the desired location
			if (_rootView.Frame.Location == frame.Location)
				return;

			frame.Size = _rootView.Frame.Size;

			// set the root views cetner
			var center = new PointF(frame.Left + frame.Width / 2,
				frame.Top + frame.Height / 2);
			_rootView.Center = center;

			// if x is greater than 0 then position the status view
			if (Math.Abs(frame.X - 0) > float.Epsilon)
			{
				GetStatusImage();
				var statusFrame = _statusImage.Frame;
				statusFrame.X = _rootView.Frame.X;
				_statusImage.Frame = statusFrame;
			}
		}

		/// <summary>
		/// Pan the specified view.
		/// </summary>
		private void Pan(UIView view)
		{
			if (_panGesture.State == UIGestureRecognizerState.Began) {
				_panOriginX = view.Frame.X;
				_ignorePan = false;
			} else if (!_ignorePan && (_panGesture.State == UIGestureRecognizerState.Changed)) {
				float t = _panGesture.TranslationInView(view).X;
				if (t < -_menuWidth)
					t = -_menuWidth;
				else if (t > _menuWidth)
					t = _menuWidth; 
				if ((_panOriginX + t) <= 0)
					view.Frame = new RectangleF(_panOriginX + t, view.Frame.Y, view.Frame.Width, view.Frame.Height);
				ShowShadowWhileDragging();
			} else if (!_ignorePan && (_panGesture.State == UIGestureRecognizerState.Ended || _panGesture.State == UIGestureRecognizerState.Cancelled)) {
				float velocity = _panGesture.VelocityInView(view).X;
				if (IsOpen) {
					if (view.Frame.X > -(view.Frame.Width / 2)) {
						HideMenu();
					} else {
						UIView.Animate(_slideSpeed, 0, UIViewAnimationOptions.CurveEaseInOut,
							() => {
								view.Frame = new RectangleF(-_menuWidth, view.Frame.Y, view.Frame.Width, view.Frame.Height);
							}, () => {
							});
					}
				} else {
					if (velocity < -800.0f || (view.Frame.X < -(view.Frame.Width / 2))) {
						ShowMenu();
					} else {
						UIView.Animate(_slideSpeed, 0, UIViewAnimationOptions.CurveEaseInOut,
							() => {
								view.Frame = new RectangleF(0, 0, view.Frame.Width, view.Frame.Height);
							}, () => {
							});
					}
				}
			}
		}

		/// <summary>
		/// Shows the shadow of the root view while dragging.
		/// </summary>
		private void ShowShadowWhileDragging()
		{
			if (!LayerShadowing)
				return;
			_rootView.Layer.ShadowOffset = new SizeF(5, 0);
			_rootView.Layer.ShadowPath = UIBezierPath.FromRect (_rootView.Bounds).CGPath;
			_rootView.Layer.ShadowRadius = 4.0f;
			_rootView.Layer.ShadowOpacity = 0.5f;
			_rootView.Layer.ShadowColor = UIColor.Black.CGColor;
		}

		/// <summary>
		/// Shows the shadow.
		/// </summary>
		private void ShowShadow(float position)
		{
			//Dont need to call this twice if its already shown
			if (!LayerShadowing || _shadowShown)
				return;
			_rootView.Layer.ShadowOffset = new SizeF(position, 0);
			_rootView.Layer.ShadowPath = UIBezierPath.FromRect (_rootView.Bounds).CGPath;
			_rootView.Layer.ShadowRadius = 4.0f;
			_rootView.Layer.ShadowOpacity = 0.5f;
			_rootView.Layer.ShadowColor = UIColor.Black.CGColor;
			_shadowShown = true;
		}

		/// <summary>
		/// Hides the shadow of the root view.
		/// </summary>
		private void HideShadow()
		{
			//Dont need to call this twice if its already hidden
			if (!LayerShadowing || !_shadowShown)
				return;
			_rootView.Layer.ShadowOffset = new SizeF (0, 0);
			_rootView.Layer.ShadowRadius = 0.0f;
			_rootView.Layer.ShadowOpacity = 0.0f;
			_rootView.Layer.ShadowColor = UIColor.Clear.CGColor;
			_shadowShown = false;
		}

		/// <summary>
		/// Places the static status image.
		/// </summary>
		private void GetStatusImage()
		{
			if (DisableStatusBarMoving || !_isIos7 || _statusImage.Superview != null)
				return;
			this.View.AddSubview(_statusImage);
			_statusImage.Image = CaptureStatusBarImage();
			_statusImage.Frame = UIApplication.SharedApplication.StatusBarFrame;
			UIApplication.SharedApplication.StatusBarHidden = true;
		}

		/// <summary>
		/// Gets a static image of the status bar.
		/// </summary>
		private UIImage CaptureStatusBarImage()
		{
			var frame = UIApplication.SharedApplication.StatusBarFrame;
			frame.Width *= 2;
			frame.Height *= 2;
			var image = CGImage.ScreenImage;
			image = image.WithImageInRect(frame);
			var newImage = new UIImage(image).Scale(UIApplication.SharedApplication.StatusBarFrame.Size, 2f);
			return newImage;
		}

		/// <summary>
		/// Hides the static status bar image.
		/// </summary>
		private void HideStatusImage()
		{
			if (!_isIos7)
				return;
			_statusImage.RemoveFromSuperview();
			UIApplication.SharedApplication.StatusBarHidden = false;
		}

		/// <summary>
		/// Hides the static status bar when the close animation completes
		/// </summary>
		[Export("animationEnded")]
		private void HideComplete()
		{
			HideStatusImage();
		}

		/// <summary>
		/// Should the receive touch.
		/// </summary>
		internal bool ShouldReceiveTouch(UIGestureRecognizer gesture, UITouch touch)
		{
			if (_shouldReceiveTouch != null)
				return _shouldReceiveTouch(gesture, touch);
			return true;
		}

		/// <summary>
		/// TODO: Not sure what this does yet.
		/// </summary>
		private void ResignFirstResponders(UIView view)
		{
			if (view.Subviews == null)
				return;
			foreach (UIView subview in view.Subviews)
			{
				if (subview.IsFirstResponder)
					subview.ResignFirstResponder();
				ResignFirstResponders(subview);
			}
		}

		#endregion

		#region Other

		private class SlideoutPanDelegate : UIGestureRecognizerDelegate
		{
			private readonly UIViewController _controller;

			public SlideoutPanDelegate (UIViewController controller)
			{
				_controller = controller;
			}

			public override bool ShouldReceiveTouch (UIGestureRecognizer recognizer, UITouch touch)
			{
				return ((touch.LocationInView (_controller.View).Y <= _menuWidth));
			}
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();
			RectangleF navigationFrame = View.Bounds;
			navigationFrame.X = navigationFrame.Width - _menuWidth;
			navigationFrame.Width = _menuWidth;
			if (_navigation.View.Frame != navigationFrame)
				_navigation.View.Frame = navigationFrame;
		}

		#endregion
	}
}

