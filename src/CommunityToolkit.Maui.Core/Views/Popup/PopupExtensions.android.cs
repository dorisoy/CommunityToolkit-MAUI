﻿using Android.App;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Platform;
using static Android.App.ActionBar;
using AColorRes = Android.Resource.Color;
using AView = Android.Views.View;
using LayoutAlignment = Microsoft.Maui.Primitives.LayoutAlignment;

namespace CommunityToolkit.Core.Platform;

/// <summary>
/// Extension class where Helper methods for Popup lives.
/// </summary>
public static class PopupExtensions
{
	/// <summary>
	/// Method to update the <see cref="IPopup.Anchor"/> view.
	/// </summary>
	/// <param name="dialog">An instance of <see cref="Dialog"/>.</param>
	/// <param name="basePopup">An instance of <see cref="IPopup"/>.</param>
	/// <exception cref="NullReferenceException">if the <see cref="Android.Views.Window"/> is null an exception will be thrown.</exception>
	public static void SetAnchor(this Dialog dialog, in IPopup basePopup)
	{
		var window = GetWindow(dialog);

		if (basePopup.Handler is null || basePopup.Handler.MauiContext is null)
		{
			return;
		}

		if (basePopup.Anchor is not null)
		{
			var anchorView = basePopup.Anchor?.ToNative(basePopup.Handler.MauiContext);

			if (anchorView is null)
			{
				return;
			}

			var locationOnScreen = new int[2];
			anchorView.GetLocationOnScreen(locationOnScreen);

			window?.SetGravity(GravityFlags.Top | GravityFlags.Left);
			window?.DecorView.Measure((int)MeasureSpecMode.Unspecified, (int)MeasureSpecMode.Unspecified);

			// This logic is tricky, please read these notes if you need to modify
			// Android window coordinate starts (0,0) at the top left and (max,max) at the bottom right. All of the positions
			// that are being handled in this operation assume the point is at the top left of the rectangle. This means the
			// calculation operates in this order:
			// 1. Calculate top-left position of Anchor
			// 2. Calculate the Actual Center of the Anchor by adding the width /2 and height / 2
			// 3. Calculate the top-left point of where the dialog should be positioned by subtracting the Width / 2 and height / 2
			//    of the dialog that is about to be drawn.
			_ = window?.Attributes ?? throw new NullReferenceException();

			window.Attributes.X = locationOnScreen[0] + (anchorView.Width / 2) - (window.DecorView.MeasuredWidth / 2);
			window.Attributes.Y = locationOnScreen[1] + (anchorView.Height / 2) - (window.DecorView.MeasuredHeight / 2);
		}
		else
		{
			SetDialogPosition(basePopup, window);
		}
	}

	/// <summary>
	/// Method to update the <see cref="IPopup.Color"/> property.
	/// </summary>
	/// <param name="dialog">An instance of <see cref="Dialog"/>.</param>
	/// <param name="basePopup">An instance of <see cref="IPopup"/>.</param>
	public static void SetColor(this Dialog dialog, in IPopup basePopup)
	{
		if (basePopup.Color is null)
		{
			return;
		}

		var window = GetWindow(dialog);
		window?.SetBackgroundDrawable(new ColorDrawable(basePopup.Color.ToNative(AColorRes.BackgroundLight, dialog.Context)));
	}

	/// <summary>
	/// Method to update the <see cref="IPopup.IsLightDismissEnabled"/> property.
	/// </summary>
	/// <param name="dialog">An instance of <see cref="Dialog"/>.</param>
	/// <param name="basePopup">An instance of <see cref="IPopup"/>.</param>
	public static void SetLightDismiss(this Dialog dialog, in IPopup basePopup)
	{
		if (basePopup.IsLightDismissEnabled)
		{
			return;
		}

		dialog.SetCancelable(false);
		dialog.SetCanceledOnTouchOutside(false);
	}

	/// <summary>
	/// Method to update the <see cref="IPopup.Size"/> property.
	/// </summary>
	/// <param name="dialog">An instance of <see cref="Dialog"/>.</param>
	/// <param name="basePopup">An instance of <see cref="IPopup"/>.</param>
	/// <param name="container">The native representation of <see cref="IPopup.Content"/>.</param>
	/// <exception cref="NullReferenceException">if the <see cref="Android.Views.Window"/> is null an exception will be thrown. If the <paramref name="container"/> is null an exception will be thrown.</exception>
	public static void SetSize(this Dialog dialog, in IPopup basePopup, in AView? container)
	{
		var window = GetWindow(dialog);
		var context = dialog.Context;
		if (basePopup.Content is not null && basePopup.Size != default)
		{
			var decorView = (ViewGroup)(window?.DecorView ?? throw new NullReferenceException());
			var child = decorView?.GetChildAt(0) ?? throw new NullReferenceException();

			var realWidth = (int)context.ToPixels(basePopup.Size.Width);
			var realHeight = (int)context.ToPixels(basePopup.Size.Height);

			var realContentWidth = (int)context.ToPixels(basePopup.Content.Width);
			var realContentHeight = (int)context.ToPixels(basePopup.Content.Height);

			var childLayoutParams = (FrameLayout.LayoutParams)(child?.LayoutParameters ?? throw new NullReferenceException());
			childLayoutParams.Width = realWidth;
			childLayoutParams.Height = realHeight;
			child.LayoutParameters = childLayoutParams;

			var horizontalParams = -1;
			switch (basePopup.Content.HorizontalLayoutAlignment)
			{
				case LayoutAlignment.Center:
				case LayoutAlignment.End:
				case LayoutAlignment.Start:
					horizontalParams = ViewGroup.LayoutParams.WrapContent;
					break;
				case LayoutAlignment.Fill:
					horizontalParams = ViewGroup.LayoutParams.MatchParent;
					break;
			}

			var verticalParams = -1;
			switch (basePopup.Content.VerticalLayoutAlignment)
			{
				case LayoutAlignment.Center:
				case LayoutAlignment.End:
				case LayoutAlignment.Start:
					verticalParams = ViewGroup.LayoutParams.WrapContent;
					break;
				case LayoutAlignment.Fill:
					verticalParams = ViewGroup.LayoutParams.MatchParent;
					break;
			}

			_ = container ?? throw new NullReferenceException();
			if (realContentWidth > -1)
			{
				var inputMeasuredWidth = realContentWidth > realWidth ?
					realWidth : realContentWidth;
				container.Measure(inputMeasuredWidth, (int)MeasureSpecMode.Unspecified);
				horizontalParams = container.MeasuredWidth;
			}
			else
			{
				container.Measure(realWidth, (int)MeasureSpecMode.Unspecified);
				horizontalParams = container.MeasuredWidth > realWidth ?
					realWidth : container.MeasuredWidth;
			}

			if (realContentHeight > -1)
			{
				verticalParams = realContentHeight;
			}
			else
			{
				var inputMeasuredWidth = realContentWidth > -1 ? horizontalParams : realWidth;
				container.Measure(inputMeasuredWidth, (int)MeasureSpecMode.Unspecified);
				verticalParams = container.MeasuredHeight > realHeight ?
					realHeight : container.MeasuredHeight;
			}

			var containerLayoutParams = new FrameLayout.LayoutParams(horizontalParams, verticalParams);

			switch (basePopup.Content.VerticalLayoutAlignment)
			{
				case LayoutAlignment.Start:
					containerLayoutParams.Gravity = GravityFlags.Top;
					break;
				case LayoutAlignment.Center:
				case LayoutAlignment.Fill:
					containerLayoutParams.Gravity = GravityFlags.FillVertical;
					containerLayoutParams.Height = realHeight;
					//container.MatchHeight = true;
					break;
				case LayoutAlignment.End:
					containerLayoutParams.Gravity = GravityFlags.Bottom;
					break;
			}

			switch (basePopup.Content.HorizontalLayoutAlignment)
			{
				case LayoutAlignment.Start:
					containerLayoutParams.Gravity |= GravityFlags.Left;
					break;
				case LayoutAlignment.Center:
				case LayoutAlignment.Fill:
					containerLayoutParams.Gravity |= GravityFlags.FillHorizontal;
					containerLayoutParams.Width = realWidth;
					//container.MatchWidth = true;
					break;
				case LayoutAlignment.End:
					containerLayoutParams.Gravity |= GravityFlags.Right;
					break;
			}

			container.LayoutParameters = containerLayoutParams;
		}
	}

	static void SetDialogPosition(in IPopup basePopup, Android.Views.Window window)
	{
		var gravityFlags = basePopup.VerticalOptions switch
		{
			LayoutAlignment.Start => GravityFlags.Top,
			LayoutAlignment.End => GravityFlags.Bottom,
			_ => GravityFlags.CenterVertical,
		};
		gravityFlags |= basePopup.HorizontalOptions switch
		{
			LayoutAlignment.Start => GravityFlags.Left,
			LayoutAlignment.End => GravityFlags.Right,
			_ => GravityFlags.CenterHorizontal,
		};
		window?.SetGravity(gravityFlags);
	}

	static Android.Views.Window GetWindow(in Dialog dialog)
	{
		var window = dialog.Window ?? throw new NullReferenceException("Android.Views.Window is null!");

		return window;
	}
}