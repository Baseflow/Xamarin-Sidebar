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
		private UIViewController _menuController;

		// gesture recognizers
		private UITapGestureRecognizer _tapGesture;
		private UIPanGestureRecognizer _panGesture;

		private UIImageView _statusImage;

		#endregion

		#region Public Properties

		/// <summary>
		/// The view shown in the content area.
		/// </summary>
		public UIViewController ContentAreaController { get; private set; }

		/// <summary>
		/// The view controller for the side menu.
		/// This is what will be shown when the menu is displayed.
		/// </summary>
		public UIViewController MenuAreaController
		{
			get { return _menuController; }
			private set { _menuController = value; }
		}

		/// <summary>
		/// Determines if the status bar should be made static.
		/// </summary>
		/// <value>True to make the status bar static, false to make it move with the content area.</value>
		public bool StatusBarMoves { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether there should be shadowing effects on the content view.
		/// </summary>
		public bool HasShadowing { get; set; }

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
					CloseMenu();
				else
					OpenMenu();
			}
		}

		#endregion

		#region Private Properties

		/// <summary>
		/// The UIView of the content view controller.
		/// </summary>
		private UIView _contentAreaView
		{
			get
			{
				if (ContentAreaController == null)
					return null;
				return ContentAreaController.View;
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
		/// <param name="contentAreaController">
		/// The view controller for the content area.
		/// </param>
		/// <param name="navigationAreaController">
		/// The view controller for the side menu.
		/// </param>
		public SidebarController(UIViewController rootViewController, UIViewController contentAreaController, UIViewController navigationAreaController)
		{
			Initialize(contentAreaController, navigationAreaController);
			// handle wiring things up so events propogate properly
			rootViewController.AddChildViewController(this);
			rootViewController.View.AddSubview(this.View);
			this.DidMoveToParentViewController(rootViewController);
		}
			
		#endregion

		#region Public Methods

		/// <summary>
		/// Toggles the menu open or closed.
		/// </summary>
		public void ToggleMenu()
		{
			if (!IsOpen && ContentAreaController != null && ContentAreaController.IsViewLoaded)
				ResignFirstResponders(ContentAreaController.View);
			if (IsOpen)
				CloseMenu();
			else
				OpenMenu();
		}

		/// <summary>
		/// Shows the slideout navigation menu.
		/// </summary>
		public void OpenMenu()
		{
			if (IsOpen)
				return;
			ShowShadow(5);
			var view = _contentAreaView;
			UIView.Animate(
				_slideSpeed, 
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
		public void CloseMenu(bool animate = true)
		{
			if (!IsOpen)
				return;
			var view = _contentAreaView;
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

		/// <summary>
		/// Replaces the content area view controller with the specified view controller.
		/// </summary>
		/// <param name="newContentView">
		/// New content view.
		/// </param>
		public void ChangeContentView(UIViewController newContentView) {
			if (_contentAreaView != null)
				_contentAreaView.RemoveFromSuperview();
			ContentAreaController = newContentView;
			CloseMenu(true);
			SetVisibleView();
			// setup a tap gesture to close the menu on root view tap
			_tapGesture = new UITapGestureRecognizer ();
			_tapGesture.AddTarget (() => CloseMenu());
			_tapGesture.NumberOfTapsRequired = 1;
			_panGesture = new UIPanGestureRecognizer {
				Delegate = new SlideoutPanDelegate(this),
				MaximumNumberOfTouches = 1,
				MinimumNumberOfTouches = 1
			};
			_panGesture.AddTarget (() => Pan (_contentAreaView));
			_contentAreaView.AddGestureRecognizer(_panGesture);
		}

		#endregion

		#region Private Methods

		private void Initialize(UIViewController currentViewController, UIViewController navigationViewController)
		{
			ContentAreaController = currentViewController;
			_menuController = navigationViewController;

			// enable shadow by default
			HasShadowing = true;

			// make the status bar static
			StatusBarMoves = true;
			_statusImage = new UIImageView();

			// set iOS 7 flag
			var version = new System.Version(UIDevice.CurrentDevice.SystemVersion);
			_isIos7 = version.Major >= 7;

			// add the navigation view on the right
			var navigationFrame = _menuController.View.Frame;
			navigationFrame.X = navigationFrame.Width - _menuWidth;
			navigationFrame.Width = _menuWidth;
			_menuController.View.Frame = navigationFrame;
			View.AddSubview(_menuController.View);

			ChangeContentView(currentViewController);
		}

		/// <summary>
		/// Places the root view on top of the navigation view.
		/// </summary>
		private void SetVisibleView()
		{
			if(!StatusBarMoves)
				UIApplication.SharedApplication.SetStatusBarHidden(false, UIStatusBarAnimation.Fade);
			bool isOpen = false;

			RectangleF frame = View.Bounds;
			if (isOpen)
				frame.X = _menuWidth;

			SetViewSize();
			SetLocation(frame);

			View.AddSubview(_contentAreaView);
			AddChildViewController(ContentAreaController);
		}

		/// <summary>
		/// Sets the size of the root view.
		/// </summary>
		private void SetViewSize()
		{
			RectangleF frame = View.Bounds;
			if (_contentAreaView.Bounds == frame)
				return;
			_contentAreaView.Bounds = frame;
		}

		/// <summary>
		/// Sets the location of the root view.
		/// </summary>
		/// <param name="frame">Frame.</param>
		private void SetLocation(RectangleF frame)
		{
			frame.Y = 0;
			_contentAreaView.Layer.AnchorPoint = new PointF(.5f, .5f);

			// exit if we're already at the desired location
			if (_contentAreaView.Frame.Location == frame.Location)
				return;

			frame.Size = _contentAreaView.Frame.Size;

			// set the root views cetner
			var center = new PointF(frame.Left + frame.Width / 2,
				frame.Top + frame.Height / 2);
			_contentAreaView.Center = center;

			// if x is greater than 0 then position the status view
			if (Math.Abs(frame.X - 0) > float.Epsilon)
			{
				GetStatusImage();
				var statusFrame = _statusImage.Frame;
				statusFrame.X = _contentAreaView.Frame.X;
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
						CloseMenu();
					} else {
						UIView.Animate(_slideSpeed, 0, UIViewAnimationOptions.CurveEaseInOut,
							() => {
								view.Frame = new RectangleF(-_menuWidth, view.Frame.Y, view.Frame.Width, view.Frame.Height);
							}, () => {
							});
					}
				} else {
					if (velocity < -800.0f || (view.Frame.X < -(view.Frame.Width / 2))) {
						OpenMenu();
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
			if (!HasShadowing)
				return;
			_contentAreaView.Layer.ShadowOffset = new SizeF(5, 0);
			_contentAreaView.Layer.ShadowPath = UIBezierPath.FromRect (_contentAreaView.Bounds).CGPath;
			_contentAreaView.Layer.ShadowRadius = 4.0f;
			_contentAreaView.Layer.ShadowOpacity = 0.5f;
			_contentAreaView.Layer.ShadowColor = UIColor.Black.CGColor;
		}

		/// <summary>
		/// Shows the shadow.
		/// </summary>
		private void ShowShadow(float position)
		{
			//Dont need to call this twice if its already shown
			if (!HasShadowing || _shadowShown)
				return;
			_contentAreaView.Layer.ShadowOffset = new SizeF(position, 0);
			_contentAreaView.Layer.ShadowPath = UIBezierPath.FromRect (_contentAreaView.Bounds).CGPath;
			_contentAreaView.Layer.ShadowRadius = 4.0f;
			_contentAreaView.Layer.ShadowOpacity = 0.5f;
			_contentAreaView.Layer.ShadowColor = UIColor.Black.CGColor;
			_shadowShown = true;
		}

		/// <summary>
		/// Hides the shadow of the root view.
		/// </summary>
		private void HideShadow()
		{
			//Dont need to call this twice if its already hidden
			if (!HasShadowing || !_shadowShown)
				return;
			_contentAreaView.Layer.ShadowOffset = new SizeF (0, 0);
			_contentAreaView.Layer.ShadowRadius = 0.0f;
			_contentAreaView.Layer.ShadowOpacity = 0.0f;
			_contentAreaView.Layer.ShadowColor = UIColor.Clear.CGColor;
			_shadowShown = false;
		}

		/// <summary>
		/// Places the static status image.
		/// </summary>
		private void GetStatusImage()
		{
			if (StatusBarMoves || !_isIos7 || _statusImage.Superview != null)
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
			if (_menuController.View.Frame != navigationFrame)
				_menuController.View.Frame = navigationFrame;
		}

		public override void ViewWillAppear(bool animated)
		{
			RectangleF navigationFrame = _menuController.View.Frame;
			navigationFrame.X = navigationFrame.Width - _menuWidth;
			navigationFrame.Width = _menuWidth;
			navigationFrame.Location = PointF.Empty;
			_menuController.View.Frame = navigationFrame;
			base.ViewWillAppear(animated);
		}
	}
}

