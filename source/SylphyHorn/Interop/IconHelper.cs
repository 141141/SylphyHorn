﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MetroRadiance.Interop;
using SylphyHorn.Services;
using System.Windows;

namespace SylphyHorn.Interop
{
    // reuse objects if possible (fontfamily, font, brush?)
    public static class IconHelper
	{
		public static Icon GetIconFromResource(Uri uri)
		{
			var streamResourceInfo = System.Windows.Application.GetResourceStream(uri);
			if (streamResourceInfo == null) throw new ArgumentException("Resource not found.", nameof(uri));

			using (var stream = streamResourceInfo.Stream)
			{
                var icon = new Icon(stream);

                return ScaleIconToDpi(icon);
			}
		}

        public static Icon GetDesktopInfoIcon(int currentDesktop, int totalDesktopCount, Color color)
        {
            using (var iconBitmap = totalDesktopCount < 10
                ? DrawHorizontalInfo(currentDesktop, totalDesktopCount, color)
                : DrawVerticalInfo(currentDesktop, totalDesktopCount, color))
            {
                return iconBitmap.ToIcon();
            }
        }

        private static Icon ToIcon(this Bitmap bitmap)
        {
            IntPtr iconHandle = bitmap.GetHicon();
            var icon = Icon.FromHandle(iconHandle);

            return icon;
        }
        
        private static Bitmap DrawHorizontalInfo(int currentDesktop, int totalDesktopCount, Color color)
        {
            var iconSize = GetIconSize();
            var bitmap = new Bitmap((int)iconSize.Width, (int)iconSize.Height);

            var stringToDraw = $"{currentDesktop}/{totalDesktopCount}";

            var offset = GetHorizontalStringOffset();

            using (var fontFamily = new FontFamily("Segoe UI"))
            {
                using (var font = new Font(fontFamily, 7, System.Drawing.FontStyle.Bold))
                {
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        using (var brush = new SolidBrush(color))
                        {
                            graphics.DrawString(stringToDraw, font, brush, offset);
                        }
                    }
                }
            }

            return bitmap;
        }

        private static Bitmap DrawVerticalInfo(int currentDesktop, int totalDesktops, Color color)
        {
            var iconSize = GetIconSize();
            var bitmap = new Bitmap((int)iconSize.Width, (int)iconSize.Height);

            var firstOffset = GetFirstVerticalStringOffset(currentDesktop);
            var secondOffset = GetSecondVerticalStringOffset(totalDesktops, bitmap.Height);

            var firstString = currentDesktop.ToString();
            var secondString = totalDesktops.ToString();

            using (var fontFamily = new FontFamily("Segoe UI"))
            {
                using (var font = new Font(fontFamily, 6, System.Drawing.FontStyle.Bold))
                {
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        using (var brush = new SolidBrush(color))
                        {
                            graphics.DrawString(firstString, font, brush, firstOffset);
                            graphics.DrawString(secondString, font, brush, secondOffset);
                        }
                    }
                }
            }

            return bitmap;
        }

        private static PointF GetHorizontalStringOffset()
        {
            return new PointF(-2, 0);
        }

        private static PointF GetFirstVerticalStringOffset(int value)
        {
            var offset = new PointF(-2, -2);

            if (value < 10)
            {
                offset.X += 7;
            }
            else if (value < 100)
            {
                offset.X += 4;
            }

            return offset;
        }

        private static PointF GetSecondVerticalStringOffset(int value, int bitmapHeight)
        {
            var offset = GetFirstVerticalStringOffset(value);

            offset.Y += bitmapHeight / 2;

            return offset;
        }

        private static Icon ScaleIconToDpi(Icon targetIcon)
        {
            var dpi = GetMonitorDpi();

            return new Icon(targetIcon, new System.Drawing.Size((int)(16 * dpi.ScaleX), (int)(16 * dpi.ScaleY)));
        }

        private static System.Windows.Size GetIconSize()
        {
            var dpi = GetMonitorDpi();

            return new System.Windows.Size(SystemParameters.SmallIconWidth * dpi.ScaleX, SystemParameters.SmallIconHeight * dpi.ScaleY);
        }
        
        private static Dpi GetMonitorDpi()
        {
            return PerMonitorDpi.GetDpi(IntPtr.Zero);
        }
	}
}
