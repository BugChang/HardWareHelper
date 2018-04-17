using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using AxCPUCARDOCXLib;
using Newtonsoft.Json;

namespace HardWareHelper
{
    public partial class FrmMain : Form
    {

        private static SerialPort _serialPort;
        private static string _comData;
        private static AxCpuCardOCX axCpuCard = new AxCpuCardOCX();

        public FrmMain()
        {
            InitializeComponent();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            StartListen();
        }

        /// <summary>
        /// 开始监听Http请求
        /// </summary>
        /// <returns></returns>
        public bool StartListen()
        {
            HttpListener httpListener = new HttpListener
            {
                AuthenticationSchemes = AuthenticationSchemes.Anonymous
            };

            httpListener.Prefixes.Add("http://localhost:8181/");
            httpListener.Start();
            new Thread(new ThreadStart(delegate
            {
                while (true)
                {

                    HttpListenerContext httpListenerContext = httpListener.GetContext();
                    httpListenerContext.Response.StatusCode = 200;
                    if (httpListenerContext.Request.Url.LocalPath.Contains("."))
                    {
                        continue;
                    }
                    using (StreamWriter writer = new StreamWriter(httpListenerContext.Response.OutputStream))
                    {
                        writer.Write(ProcessingHandle(httpListenerContext));
                    }
                }
            })).Start();

            var isListening = httpListener.IsListening;
            return isListening;
        }

        /// <summary>
        /// 监听处理
        /// </summary>
        /// <returns></returns>
        public string ProcessingHandle(HttpListenerContext httpListenerContext)
        {
            var strMethod = httpListenerContext.Request.Url.Segments[1];
            Type type = typeof(FrmMain);
            var method = type.GetMethod(strMethod);
            if (method == null)
            {
                return "不存在的方法，请检查Url拼写！";
            }
            var result = method.Invoke(null, new object[] { httpListenerContext });
            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// 获取Mac地址
        /// </summary>
        /// <returns></returns>
        public static IList<string> GetMacAddresses(HttpListenerContext httpListenerContext)
        {
            var macAddresses = new List<string>();
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface ni in interfaces)
            {
                if (ni.OperationalStatus == OperationalStatus.Up && !string.IsNullOrEmpty(ni.GetPhysicalAddress().ToString()))
                {
                    macAddresses.Add(ni.GetPhysicalAddress().ToString());
                }
            }
            return macAddresses;
        }

        #region 扫描枪&读卡器

        /// <summary>
        /// 获取串口名称列表
        /// </summary>
        /// <returns></returns>
        public static string[] GetSerialPortNames(HttpListenerContext httpListenerContext)
        {
            string[] serialPortNames = SerialPort.GetPortNames();
            return serialPortNames;
        }

        /// <summary>
        /// 打开串口端口
        /// </summary>
        /// <param name="httpListenerContext"></param>
        /// <returns></returns>
        public static bool OpenSerialPort(HttpListenerContext httpListenerContext)
        {
            try
            {
                var portName = httpListenerContext.Request.QueryString["portName"];
                var baudRate = Convert.ToInt32(httpListenerContext.Request.QueryString["baudRate"]);
                _serialPort =
                    new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
                    {
                        Encoding = Encoding.GetEncoding("GB2312"),
                        Handshake = Handshake.RequestToSendXOnXOff
                    };
                _serialPort.DataReceived += _serialPort_DataReceived;
                _serialPort.Open();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 串口读取数据事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(500);//延迟500ms，防止有些硬件设备未写完数据
            var text = _serialPort.ReadExisting();
            _comData = text.Replace("\u0002", "").Replace("\u0003", "").Replace("\r", "").Replace("\n", "").Replace("\t", "");
        }

        /// <summary>
        /// 获取串口数据
        /// </summary>
        /// <returns></returns>
        public static string GetComData(HttpListenerContext httpListenerContext)
        {
            while (string.IsNullOrEmpty(_comData))
            {
                Thread.Sleep(100);
            }
            return _comData;
        }

        /// <summary>
        ///关闭串口
        /// </summary>
        public static void CloseSerialPort(HttpListenerContext httpListenerContext)
        {
            _serialPort.Close();
        }



        #endregion

        #region 国办CPU卡

        public static bool WriteCpuCard(HttpListenerContext httpListenerContext)
        {
            var port = Convert.ToInt16(httpListenerContext.Request.QueryString["port"]);
            var baud = Convert.ToInt32(httpListenerContext.Request.QueryString["baudRate"]);
            byte[] data = Encoding.UTF8.GetBytes(httpListenerContext.Request.QueryString["data"]);
            string dataBase64String = Convert.ToBase64String(data);
            axCpuCard.ClosePort();
            var bOpen = axCpuCard.OpenPort(port, baud);
            if (bOpen)
            {
                if (axCpuCard.IsCardAvailabile)
                {
                    return axCpuCard.SetFileDataBinBase64(dataBase64String);
                }
            }
            return false;
        }


        public static string ReadCpuCard(HttpListenerContext httpListenerContext)
        {
            var port = Convert.ToInt16(httpListenerContext.Request.QueryString["port"]);
            var baud = Convert.ToInt32(httpListenerContext.Request.QueryString["baudRate"]);
            axCpuCard.ClosePort();
            var bOpen = axCpuCard.OpenPort(port, baud);
            if (bOpen)
            {
                if (axCpuCard.IsCardAvailabile)
                {
                    return Encoding.UTF8.GetString(Convert.FromBase64String(axCpuCard.GetFileDataBinBase64()));
                }
            }
            return "";
        }

        #endregion

    }
}
