﻿using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	public unsafe partial struct SKColorSpacePrimaries
	{
		public SKColorSpacePrimaries (float[] values)
		{
			if (values == null)
				throw new ArgumentNullException (nameof (values));
			if (values.Length != 8)
				throw new ArgumentException ("The values must have exactly 8 items, one for each of [RX, RY, GX, GY, BX, BY, WX, WY].", nameof (values));

			fRX = values[0];
			fRY = values[1];
			fGX = values[2];
			fGY = values[3];
			fBX = values[4];
			fBY = values[5];
			fWX = values[6];
			fWY = values[7];
		}

		public SKColorSpacePrimaries (float rx, float ry, float gx, float gy, float bx, float by, float wx, float wy)
		{
			fRX = rx;
			fRY = ry;
			fGX = gx;
			fGY = gy;
			fBX = bx;
			fBY = by;
			fWX = wx;
			fWY = wy;
		}

		public float[] Values => new[] { fRX, fRY, fGX, fGY, fBX, fBY, fWX, fWY };

		public bool ToXyzD50 (SKMatrix44 toXyzD50)
		{
			if (toXyzD50 == null)
				throw new ArgumentNullException (nameof (toXyzD50));
			fixed (SKColorSpacePrimaries* t = &this) {
				return SkiaApi.sk_colorspace_primaries_to_xyzd50 (t, toXyzD50.Handle);
			}
		}

		public SKMatrix44 ToXyzD50 ()
		{
			var xyzD50 = new SKMatrix44 ();
			if (!ToXyzD50 (xyzD50)) {
				xyzD50.Dispose ();
				xyzD50 = null;
			}
			return xyzD50;
		}
	}

	public unsafe partial struct SKColorSpaceTransferFn
	{
		public SKColorSpaceTransferFn (float[] values)
		{
			if (values == null)
				throw new ArgumentNullException (nameof (values));
			if (values.Length != 7)
				throw new ArgumentException ("The values must have exactly 7 items, one for each of [G, A, B, C, D, E, F].", nameof (values));

			fG = values[0];
			fA = values[1];
			fB = values[2];
			fC = values[3];
			fD = values[4];
			fE = values[5];
			fF = values[6];
		}

		public SKColorSpaceTransferFn (float g, float a, float b, float c, float d, float e, float f)
		{
			fG = g;
			fA = a;
			fB = b;
			fC = c;
			fD = d;
			fE = e;
			fF = f;
		}

		public float[] Values => new[] { fG, fA, fB, fC, fD, fE, fF };

		public SKColorSpaceTransferFn Invert ()
		{
			SKColorSpaceTransferFn inverted;
			fixed (SKColorSpaceTransferFn* t = &this) {
				SkiaApi.sk_colorspace_transfer_fn_invert (t, &inverted);
			}
			return inverted;
		}

		public float Transform (float x)
		{
			fixed (SKColorSpaceTransferFn* t = &this) {
				return SkiaApi.sk_colorspace_transfer_fn_eval (t, x);
			}
		}
	}

	public unsafe class SKColorSpace : SKObject, ISKReferenceCounted
	{
		private static readonly SKColorSpace srgb;
		private static readonly SKColorSpace srgbLinear;

		static SKColorSpace ()
		{
			srgb = new SKColorSpaceStatic (SkiaApi.sk_colorspace_new_srgb ());
			srgbLinear = new SKColorSpaceStatic (SkiaApi.sk_colorspace_new_srgb_linear ());
		}

		internal static void EnsureStaticInstanceAreInitialized ()
		{
			// IMPORTANT: do not remove to ensure that the static instances
			//            are initialized before any access is made to them
		}

		[Preserve]
		internal SKColorSpace (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		public bool GammaIsCloseToSrgb => SkiaApi.sk_colorspace_gamma_close_to_srgb (Handle);

		public bool GammaIsLinear => SkiaApi.sk_colorspace_gamma_is_linear (Handle);

		public bool IsSrgb => SkiaApi.sk_colorspace_is_srgb (Handle);

		public bool IsNumericalTransferFunction => GetNumericalTransferFunction (out _);

		public static bool Equal (SKColorSpace left, SKColorSpace right)
		{
			if (left == null) {
				throw new ArgumentNullException (nameof (left));
			}
			if (right == null) {
				throw new ArgumentNullException (nameof (right));
			}

			return SkiaApi.sk_colorspace_equals (left.Handle, right.Handle);
		}

		public static SKColorSpace CreateSrgb () => srgb;

		public static SKColorSpace CreateSrgbLinear () => srgbLinear;

		public static SKColorSpace CreateIcc (IntPtr input, long length)
		{
			if (input == IntPtr.Zero)
				throw new ArgumentNullException (nameof (input));

			return GetObject<SKColorSpace> (SkiaApi.sk_colorspace_new_icc ((void*)input, (IntPtr)length));
		}

		public static SKColorSpace CreateIcc (byte[] input, long length)
		{
			if (input == null)
				throw new ArgumentNullException (nameof (input));

			fixed (byte* i = input) {
				return GetObject<SKColorSpace> (SkiaApi.sk_colorspace_new_icc (i, (IntPtr)length));
			}
		}

		public static SKColorSpace CreateIcc (byte[] input)
		{
			if (input == null)
				throw new ArgumentNullException (nameof (input));

			fixed (byte* i = input) {
				return GetObject<SKColorSpace> (SkiaApi.sk_colorspace_new_icc (i, (IntPtr)input.Length));
			}
		}

		public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKMatrix44 toXyzD50)
		{
			if (toXyzD50 == null)
				throw new ArgumentNullException (nameof (toXyzD50));
			return GetObject<SKColorSpace> (SkiaApi.sk_colorspace_new_rgb_matrix44 (&coeffs, toXyzD50.Handle));
		}

		public static SKColorSpace CreateRgb (SKNamedTransferFn coeffs, SKNamedGamut toXYZD50) =>
			GetObject<SKColorSpace> (SkiaApi.sk_colorspace_new_rgb_named (coeffs, toXYZD50));

		public bool ToXyzD50 (SKMatrix44 toXyzD50)
		{
			if (toXyzD50 == null)
				throw new ArgumentNullException (nameof (toXyzD50));
			return SkiaApi.sk_colorspace_to_xyzd50 (Handle, toXyzD50.Handle);
		}

		public bool GetNumericalTransferFunction (out SKColorSpaceTransferFn fn)
		{
			fixed (SKColorSpaceTransferFn* f = &fn) {
				return SkiaApi.sk_colorspace_is_numerical_transfer_fn (Handle, f);
			}
		}

		private sealed class SKColorSpaceStatic : SKColorSpace
		{
			internal SKColorSpaceStatic (IntPtr x)
				: base (x, false)
			{
				IgnorePublicDispose = true;
			}

			protected override void Dispose (bool disposing)
			{
				// do not dispose
			}
		}
	}
}
