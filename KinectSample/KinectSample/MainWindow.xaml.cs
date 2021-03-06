﻿using System;
using System.IO;
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
using System.Windows.Threading;

namespace KinectSample
{
	using Microsoft.Research.Kinect.Nui;
	using System.Runtime.Serialization;
	using System.Runtime.Serialization.Formatters.Binary;

	/// <summary>
	/// Lógica de interacción para MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Runtime _nuiRuntime;
		private DateTime _lastTime = DateTime.MaxValue;
		private bool _isCameraON;
		private bool _isPlayingVideo;

		//Ruta del archivo donde se guardará el video.
		private string _pathSaveData;
		//TRUE si está grabando el video.
		private bool _recordData;
		//Contiene toda la información que se está grabando.
		private Queue<PlanarImageSerializable> _imageQueue = new Queue<PlanarImageSerializable>();

		private delegate void SaveDataDelegate();
		private delegate void PlayVideoDelegate(string value);

		public MainWindow()
		{
			InitializeComponent();

			InitRuntime();
		}

		/// <summary>
		/// Inicializa el motor de video de kinect.
		/// </summary>
		private void InitRuntime()
		{
			_nuiRuntime = new Runtime();

			try { _nuiRuntime.Initialize(RuntimeOptions.UseColor); } catch (InvalidOperationException) { return; }

			try { _nuiRuntime.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color); } catch (InvalidOperationException) { return; }

			_lastTime = DateTime.Now;

			//_nuiRuntime.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(OnVideoFrameReady);
		}

		/// <summary>
		/// Inicia el proceso de guadar la información en una operación asincrona.
		/// </summary>
		private void SaveData()
		{
			ShowEstado(String.Format("Guardando información : {0}", DateTime.Now));

			SaveDataDelegate myDelegate = new SaveDataDelegate(BeginSaveData);
			IAsyncResult result = myDelegate.BeginInvoke(EndSaveData, null);
		}

		/// <summary>
		/// Guardar la información grabada en un archivo.
		/// </summary>
		private void BeginSaveData()
		{
			MemoryStream ms = new MemoryStream();
			BinaryFormatter myFormatter = new BinaryFormatter();

			try {
				myFormatter.Serialize(ms, _imageQueue);
				CompressData(ms, _pathSaveData);
			} catch (SerializationException ex) {
				throw;
			} finally {
				//sw.Close();
			}
		}

		private void CompressData(Stream stream, string savePath)
		{
			using (FileStream fs = File.Create(savePath)) {
				using (System.IO.Compression.GZipStream gz = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Compress)) {
					stream.Seek(0, SeekOrigin.Begin);
					stream.CopyTo(gz);
				}
			}
		}

		/*
		 * OutOfMemory cuando el archivo es muy grande.
		 */
		private Stream DecompressData(string pathData)
		{
			Stream retVal = new MemoryStream();

			using (FileStream fsIn = new FileStream(pathData, FileMode.Open)) {
				using (System.IO.Compression.GZipStream gz = new System.IO.Compression.GZipStream(fsIn, System.IO.Compression.CompressionMode.Decompress)) {
					gz.CopyTo(retVal);
				}
			}

			return retVal;
		}

		/// <summary>
		/// Comunica la finalización del guardaddo de datos a través de un mensaje de estado
		/// y habilita el botón para grabar de nuevo.
		/// </summary>
		/// <param name="result"></param>
		private void EndSaveData(IAsyncResult result)
		{
			//Mensaje en el cuadro de estado
			Dispatcher.BeginInvoke(
				DispatcherPriority.Background,
				(System.Threading.SendOrPostCallback)delegate
				{
					ShowEstado(String.Format("Información guardada: {0}", DateTime.Now));
				},
				null);

			//Habilita el botón de grabar
			Dispatcher.BeginInvoke(
				DispatcherPriority.Background,
				(System.Threading.SendOrPostCallback)delegate
				{
					btnStartGrabar.IsEnabled = true;
				},
				null);

			//Habilita el botón de reproducir video
			Dispatcher.BeginInvoke(
				DispatcherPriority.Background,
				(System.Threading.SendOrPostCallback)delegate
				{
					btnReproducir.IsEnabled = true;
				},
				null);
		}

		/// <summary>
		/// Para la reproducción de la cámara.
		/// </summary>
		private void StopCamara()
		{
			_nuiRuntime.VideoFrameReady -= OnVideoFrameReady;
			_isCameraON = false;
			btnONOFF.Content = "ON";
			btnStartGrabar.IsEnabled = false;
			btnStopGrabar.IsEnabled = false;
		}

		/// <summary>
		/// Inicia la reproducción de la cámara
		/// </summary>
		private void StartCamara()
		{
			_nuiRuntime.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(OnVideoFrameReady);
			_isCameraON = true;
			btnONOFF.Content = "OFF";
			btnStartGrabar.IsEnabled = true;
			btnStopGrabar.IsEnabled = true;
		}

		/// <summary>
		/// Inicia la reproducción del archivo de video pasado por parámetro
		/// </summary>
		/// <param name="fileName">Arhivo de video</param>
		private void StartVideo(string fileName)
		{
			Stream myStream = DecompressData(fileName);
			Queue<PlanarImageSerializable> myImages = DeserializeVideoData(myStream);

			while (myImages.Count > 0) {
				PlanarImageSerializable pi = myImages.Dequeue();
				Dispatcher.BeginInvoke(DispatcherPriority.Background, (System.Threading.SendOrPostCallback)delegate
				{
					imgVideo.Source = BitmapSource.Create(pi.Width, pi.Height, 96, 96, PixelFormats.Bgr32, null, pi.Bits, pi.Width * pi.BytesPerPixel);
					//Con la parada cada 33 ms simula los 30 frames por segundo de la grabación original
					System.Threading.Thread.Sleep(33);
				},null);
			}

			//Ha finalizado la reproducción del video.
			_isPlayingVideo = false;
		}

		/// <summary>
		/// Deserializa el archivo que contiene los datos de la grabación
		/// </summary>
		/// <param name="fileData">Ruta del arhivo</param>
		/// <returns>Cola con los datos de las imágenes grabadas.</returns>
		private Queue<PlanarImageSerializable> DeserializeVideoData(Stream streamData)
		{
			Queue<PlanarImageSerializable> retVal;
			//FileStream fs = new FileStream(fileData, FileMode.Open);

			try {
				BinaryFormatter myFormatter = new BinaryFormatter();
				streamData.Seek(0, SeekOrigin.Begin);
				retVal = (Queue<PlanarImageSerializable>)myFormatter.Deserialize(streamData);
			} catch (Exception) {

				throw;
			} finally {
				//fs.Close();
			}

			return retVal;
		}

		/// <summary>
		/// Para la reproducción del video grabado
		/// </summary>
		private void StopVideo()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Muestra un mensaje en la lista
		/// </summary>
		/// <param name="message">Mensaje a mostrar</param>
		private void ShowEstado(string message)
		{
			lstEstado.Items.Add(message);
		}

		/// <summary>
		/// Muestra un diálogo según su tipo para abrir o guardar un fichero.
		/// </summary>
		/// <typeparam name="T">Tipo de diálogo a mostrar</typeparam>
		/// <returns>Arhivo seleccionado</returns>
		private string ShowDialog<T>() where T : Microsoft.Win32.FileDialog, new()
		{
			string retVal = string.Empty;
			T myDlg = new T();

			myDlg.Filter = "Kinect Data (*.knt) | *.knt";

			if (myDlg.ShowDialog().HasValue)
				retVal = myDlg.FileName;

			return retVal;
		}


		#region "EVENTOS"

		private void OnVideoFrameReady(object sender, ImageFrameReadyEventArgs e)
		{
			PlanarImageSerializable myImage = new PlanarImageSerializable() {
				Bits = e.ImageFrame.Image.Bits,
				Width = e.ImageFrame.Image.Width,
				Height = e.ImageFrame.Image.Height,
				BytesPerPixel = e.ImageFrame.Image.BytesPerPixel
			};

			if (_recordData)
				_imageQueue.Enqueue(myImage);

			imgVideo.Source = BitmapSource.Create(myImage.Width, myImage.Height, 96, 96, PixelFormats.Bgr32, null, myImage.Bits, myImage.Width * myImage.BytesPerPixel);
		}

		//Llamada al dialogo para seleccionar el archivo donde guardar los datos
		//y activar el proceso de grabación del video.
		private void btnStartGrabar_Click(object sender, RoutedEventArgs e)
		{
			_pathSaveData = ShowDialog<Microsoft.Win32.SaveFileDialog>();

			if (!String.IsNullOrEmpty(_pathSaveData)) {
				_recordData = true;
				btnStartGrabar.IsEnabled = false;
				btnReproducir.IsEnabled = false;

				ShowEstado(String.Format("Guardando en: {0}", _pathSaveData));
				ShowEstado(String.Format("Iniciando grabación: {0}", DateTime.Now));
			}
		}

		//Para el proceso de grabación y guarda los datos grabados.
		private void btnStopGrabar_Click(object sender, RoutedEventArgs e)
		{
			_recordData = false;

			ShowEstado(String.Format("Stop grabación : {0}", DateTime.Now));

			SaveData();
		}

		//Des/habilita la reproducción de la imágen.
		private void btnCameraONOFF_Click(object sender, RoutedEventArgs e)
		{
			if (_isCameraON) {
				StopCamara();
			} else {
				StartCamara();
			}
		}

		//Inicia o para la reproducción de un video.
		private void btnReproducir_Click(object sender, RoutedEventArgs e)
		{
			if (_isPlayingVideo) {
				_isPlayingVideo = false;
				StopVideo();
				StartCamara();
			} else {
				string myFile = ShowDialog<Microsoft.Win32.OpenFileDialog>();

				if (!String.IsNullOrEmpty(myFile)) {
					_isPlayingVideo = true;
					StopCamara();

					PlayVideoDelegate myDelegate = new PlayVideoDelegate(StartVideo);
					IAsyncResult result = myDelegate.BeginInvoke(myFile, null, null);
				}
			}
		}

		//Sube el ángulo de visión de la cámara
		private void btnCameraUp_Click(object sender, RoutedEventArgs e)
		{
			_nuiRuntime.NuiCamera.ElevationAngle += 5;
		}

		//Baja el ángulo de visión de la cámara
		private void btnCameraDown_Click(object sender, RoutedEventArgs e)
		{
			_nuiRuntime.NuiCamera.ElevationAngle -= 5;
		}

		#endregion



	}
}
