using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KinectSkeletalSample
{
	using Microsoft.Research.Kinect.Nui;



	/// <summary>
	/// Lógica de interacción para MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Runtime _nuiRuntime;
		Dictionary<JointID, Brush> _jointColors;

		public MainWindow()
		{
			InitializeComponent();

			SetColor();

			InitNui();

		}

		private void InitNui()
		{
			_nuiRuntime = new Runtime();

			try { _nuiRuntime.Initialize(RuntimeOptions.UseDepthAndPlayerIndex | RuntimeOptions.UseSkeletalTracking | RuntimeOptions.UseColor); } catch (InvalidOperationException) { return; }

			try {
				_nuiRuntime.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);
				_nuiRuntime.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution320x240, ImageType.DepthAndPlayerIndex);
			} catch (InvalidOperationException) {
				return;
			}

			_nuiRuntime.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(_nuiRuntime_SkeletonFrameReady);
			_nuiRuntime.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(_nuiRuntime_ColorFrameReady);
		}

		private void SetColor()
		{
			_jointColors = new Dictionary<JointID, Brush>() { 
	    {JointID.HipCenter, new SolidColorBrush(Color.FromRgb(169, 176, 155))},
	    {JointID.Spine, new SolidColorBrush(Color.FromRgb(169, 176, 155))},
	    {JointID.ShoulderCenter, new SolidColorBrush(Color.FromRgb(168, 230, 29))},
	    {JointID.Head, new SolidColorBrush(Color.FromRgb(200, 0,   0))},
	    {JointID.ShoulderLeft, new SolidColorBrush(Color.FromRgb(79,  84,  33))},
	    {JointID.ElbowLeft, new SolidColorBrush(Color.FromRgb(84,  33,  42))},
	    {JointID.WristLeft, new SolidColorBrush(Color.FromRgb(255, 126, 0))},
	    {JointID.HandLeft, new SolidColorBrush(Color.FromRgb(215,  86, 0))},
	    {JointID.ShoulderRight, new SolidColorBrush(Color.FromRgb(33,  79,  84))},
	    {JointID.ElbowRight, new SolidColorBrush(Color.FromRgb(33,  33,  84))},
	    {JointID.WristRight, new SolidColorBrush(Color.FromRgb(77,  109, 243))},
	    {JointID.HandRight, new SolidColorBrush(Color.FromRgb(37,   69, 243))},
	    {JointID.HipLeft, new SolidColorBrush(Color.FromRgb(77,  109, 243))},
	    {JointID.KneeLeft, new SolidColorBrush(Color.FromRgb(69,  33,  84))},
	    {JointID.AnkleLeft, new SolidColorBrush(Color.FromRgb(229, 170, 122))},
	    {JointID.FootLeft, new SolidColorBrush(Color.FromRgb(255, 126, 0))},
	    {JointID.HipRight, new SolidColorBrush(Color.FromRgb(181, 165, 213))},
	    {JointID.KneeRight, new SolidColorBrush(Color.FromRgb(71, 222,  76))},
	    {JointID.AnkleRight, new SolidColorBrush(Color.FromRgb(245, 228, 156))},
	    {JointID.FootRight, new SolidColorBrush(Color.FromRgb(77,  109, 243))}
			};
		}

		private void _nuiRuntime_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
		{
			SkeletonFrame skeletonFrame = e.SkeletonFrame;
			int iSkeleton = 0;
			Brush[] brushes = new Brush[6];
			brushes[0] = new SolidColorBrush(Color.FromRgb(255, 0, 0));
			brushes[1] = new SolidColorBrush(Color.FromRgb(0, 255, 0));
			brushes[2] = new SolidColorBrush(Color.FromRgb(64, 255, 255));
			brushes[3] = new SolidColorBrush(Color.FromRgb(255, 255, 64));
			brushes[4] = new SolidColorBrush(Color.FromRgb(255, 64, 255));
			brushes[5] = new SolidColorBrush(Color.FromRgb(128, 128, 255));


			imgSkeletor.Children.Clear();
			imgSkeletor.Children.Add(video);
			foreach (SkeletonData data in skeletonFrame.Skeletons) {
				if (SkeletonTrackingState.Tracked == data.TrackingState) {
					// Draw bones
					Brush brush = brushes[iSkeleton % brushes.Length];
					imgSkeletor.Children.Add(getBodySegment(data.Joints, brush, JointID.HipCenter, JointID.Spine, JointID.ShoulderCenter, JointID.Head));
					imgSkeletor.Children.Add(getBodySegment(data.Joints, brush, JointID.ShoulderCenter, JointID.ShoulderLeft, JointID.ElbowLeft, JointID.WristLeft, JointID.HandLeft));
					imgSkeletor.Children.Add(getBodySegment(data.Joints, brush, JointID.ShoulderCenter, JointID.ShoulderRight, JointID.ElbowRight, JointID.WristRight, JointID.HandRight));
					imgSkeletor.Children.Add(getBodySegment(data.Joints, brush, JointID.HipCenter, JointID.HipLeft, JointID.KneeLeft, JointID.AnkleLeft, JointID.FootLeft));
					imgSkeletor.Children.Add(getBodySegment(data.Joints, brush, JointID.HipCenter, JointID.HipRight, JointID.KneeRight, JointID.AnkleRight, JointID.FootRight));

					// Draw joints
					foreach (Joint joint in data.Joints) {
						Point jointPos = getDisplayPosition(joint);
						Line jointLine = new Line();
						jointLine.X1 = jointPos.X - 3;
						jointLine.X2 = jointLine.X1 + 6;
						jointLine.Y1 = jointLine.Y2 = jointPos.Y;
						jointLine.Stroke = _jointColors[joint.ID];
						jointLine.StrokeThickness = 6;
						imgSkeletor.Children.Add(jointLine);
					}
				}
				iSkeleton++;
			} // for each skeleton
		}

		void _nuiRuntime_ColorFrameReady(object sender, ImageFrameReadyEventArgs e)
		{
			// 32-bit per pixel, RGBA image
			PlanarImage Image = e.ImageFrame.Image;
			video.Source = BitmapSource.Create(
			    Image.Width, Image.Height, 96, 96, PixelFormats.Bgr32, null, Image.Bits, Image.Width * Image.BytesPerPixel);

		}

		Polyline getBodySegment(Microsoft.Research.Kinect.Nui.JointsCollection joints, Brush brush, params JointID[] ids)
		{
			PointCollection points = new PointCollection(ids.Length);
			for (int i = 0; i < ids.Length; ++i) {
				points.Add(getDisplayPosition(joints[ids[i]]));
			}

			Polyline polyline = new Polyline();
			polyline.Points = points;
			polyline.Stroke = brush;
			polyline.StrokeThickness = 5;
			return polyline;
		}

		private Point getDisplayPosition(Joint joint)
		{
			float depthX, depthY;
			_nuiRuntime.SkeletonEngine.SkeletonToDepthImage(joint.Position, out depthX, out depthY);
			depthX = Math.Max(0, Math.Min(depthX * 320, 320));  //convert to 320, 240 space
			depthY = Math.Max(0, Math.Min(depthY * 240, 240));  //convert to 320, 240 space
			int colorX, colorY;
			ImageViewArea iv = new ImageViewArea();
			// only ImageResolution.Resolution640x480 is supported at this point
			_nuiRuntime.NuiCamera.GetColorPixelCoordinatesFromDepthPixel(ImageResolution.Resolution640x480, iv, (int)depthX, (int)depthY, (short)0, out colorX, out colorY);

			// map back to skeleton.Width & skeleton.Height
			return new Point((int)(imgSkeletor.Width * colorX / 640.0), (int)(imgSkeletor.Height * colorY / 480));
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			_nuiRuntime.Uninitialize();
			Environment.Exit(0);
		}
	}
}
